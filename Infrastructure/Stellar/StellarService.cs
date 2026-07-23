using Ecoa.Core.Ports;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;

namespace Ecoa.Infrastructure.Stellar;

public class StellarService : IStellarService, IDisposable
{
    private readonly StellarRpcServer _rpcServer;
    private readonly string _ecoaTokenContractId;
    private readonly string _incentivePoolContractId;
    private readonly string _oracleContractId;
    private readonly string _adminSecretKey;
    private readonly Network _network;

    public StellarService(string networkUrl, string ecoaTokenContractId, string incentivePoolContractId, string oracleContractId, string adminSecretKey)
    {
        _ecoaTokenContractId = ecoaTokenContractId;
        _incentivePoolContractId = incentivePoolContractId;
        _oracleContractId = oracleContractId;
        _adminSecretKey = adminSecretKey;
        _network = networkUrl.Contains("testnet") ? Network.Test() : Network.Public();

        var resilienceOptions = HttpResilienceOptionsPresets.ForSorobanPolling();
        _rpcServer = new StellarRpcServer(networkUrl, resilienceOptions);
    }

    public Task<StellarWalletInfo> CreateWalletAsync()
    {
        var keyPair = KeyPair.Random();
        return Task.FromResult(new StellarWalletInfo
        {
            PublicKey = keyPair.AccountId ?? string.Empty,
            SecretKey = keyPair.SecretSeed ?? string.Empty
        });
    }

    public async Task<StellarBalance> GetEcoaBalanceAsync(string publicKey)
    {
        try
        {
            var dummyKeyPair = KeyPair.Random();
            var dummyAccount = new Account(dummyKeyPair.AccountId, 0);

            var operation = new InvokeContractOperation(
                _ecoaTokenContractId,
                "balance",
                new SCVal[] { new ScAccountId(publicKey) }
            );

            var transaction = new TransactionBuilder(dummyAccount)
                .AddOperation(operation)
                .Build();

            var simResponse = await _rpcServer.SimulateTransaction(transaction);

            if (simResponse.Results is { Length: > 0 })
            {
                var xdrBase64 = simResponse.Results[0].Xdr;
                if (xdrBase64 != null)
                {
                    var scVal = SCVal.FromXdrBase64(xdrBase64);
                    if (scVal is SCInt128 balance)
                    {
                        var value = ((System.Numerics.BigInteger)balance.Hi << 64) + balance.Lo;
                        return new StellarBalance
                        {
                            AssetCode = "ECOA",
                            Balance = value.ToString()
                        };
                    }
                }
            }
        }
        catch
        {
        }

        return new StellarBalance { AssetCode = "ECOA", Balance = "0" };
    }

    public async Task<string> MintEcoaAsync(string toPublicKey, int amount)
    {
        var adminKeyPair = KeyPair.FromSecretSeed(_adminSecretKey);
        return await SubmitContractCallAsync(
            adminKeyPair,
            _ecoaTokenContractId,
            "mint",
            new SCVal[]
            {
                new ScAccountId(toPublicKey),
                new SCInt128(amount.ToString())
            }
        );
    }

    public async Task<string> BurnEcoaAsync(string fromSecretKey, int amount)
    {
        var userKeyPair = KeyPair.FromSecretSeed(fromSecretKey);
        return await SubmitContractCallAsync(
            userKeyPair,
            _ecoaTokenContractId,
            "burn",
            new SCVal[]
            {
                new ScAccountId(userKeyPair.AccountId),
                new SCInt128(amount.ToString())
            },
            useFeeBump: true
        );
    }

    public async Task<string> TransferEcoaAsync(string fromSecretKey, string toPublicKey, int amount)
    {
        var userKeyPair = KeyPair.FromSecretSeed(fromSecretKey);
        return await SubmitContractCallAsync(
            userKeyPair,
            _ecoaTokenContractId,
            "transfer",
            new SCVal[]
            {
                new ScAccountId(userKeyPair.AccountId),
                new ScAccountId(toPublicKey),
                new SCInt128(amount.ToString())
            },
            useFeeBump: true
        );
    }

    public async Task<string> AddIncentiveAsync(string name, string description, long price, string code, string provider)
    {
        var adminKeyPair = KeyPair.FromSecretSeed(_adminSecretKey);
        return await SubmitContractCallAsync(
            adminKeyPair,
            _incentivePoolContractId,
            "add_incentive",
            new SCVal[]
            {
                new SCString(name),
                new SCString(description),
                new SCInt128(price.ToString()),
                new SCString(code),
                new SCString(provider)
            }
        );
    }

    public async Task<IncentiveInfo> RedeemIncentiveAsync(string userSecretKey, ulong incentiveId)
    {
        var userKeyPair = KeyPair.FromSecretSeed(userSecretKey);
        var txHash = await SubmitContractCallAsync(
            userKeyPair,
            _incentivePoolContractId,
            "redeem_incentive",
            new SCVal[]
            {
                new ScAccountId(userKeyPair.AccountId),
                new SCUint64(incentiveId)
            },
            useFeeBump: true
        );

        var incentive = await GetIncentiveAsync(incentiveId);
        return incentive ?? new IncentiveInfo { Id = incentiveId };
    }

    public async Task<IncentiveInfo?> GetIncentiveAsync(ulong incentiveId)
    {
        try
        {
            var dummyKeyPair = KeyPair.Random();
            var dummyAccount = new Account(dummyKeyPair.AccountId, 0);

            var operation = new InvokeContractOperation(
                _incentivePoolContractId,
                "get_incentive",
                new SCVal[] { new SCUint64(incentiveId) }
            );

            var transaction = new TransactionBuilder(dummyAccount).AddOperation(operation).Build();
            var simResponse = await _rpcServer.SimulateTransaction(transaction);

            if (simResponse.Results is { Length: > 0 } && simResponse.Results[0].Xdr != null)
            {
                var scVal = SCVal.FromXdrBase64(simResponse.Results[0].Xdr);
                return ParseIncentive(scVal);
            }
        }
        catch
        {
        }

        return null;
    }

    public async Task<List<IncentiveInfo>> GetAllIncentivesAsync()
    {
        return await FetchIncentiveList("get_all_incentives");
    }

    public async Task<List<IncentiveInfo>> GetAvailableIncentivesAsync()
    {
        return await FetchIncentiveList("get_available_incentives");
    }

    private async Task<List<IncentiveInfo>> FetchIncentiveList(string functionName)
    {
        try
        {
            var dummyKeyPair = KeyPair.Random();
            var dummyAccount = new Account(dummyKeyPair.AccountId, 0);

            var operation = new InvokeContractOperation(
                _incentivePoolContractId,
                functionName,
                Array.Empty<SCVal>()
            );

            var transaction = new TransactionBuilder(dummyAccount).AddOperation(operation).Build();
            var simResponse = await _rpcServer.SimulateTransaction(transaction);

            if (simResponse.Results is { Length: > 0 } && simResponse.Results[0].Xdr != null)
            {
                var scVal = SCVal.FromXdrBase64(simResponse.Results[0].Xdr);
                if (scVal is SCVec vec)
                {
                    var result = new List<IncentiveInfo>();
                    foreach (var item in vec.InnerValue)
                    {
                        var incentive = ParseIncentive(item);
                        if (incentive != null)
                            result.Add(incentive);
                    }
                    return result;
                }
            }
        }
        catch
        {
        }

        return new List<IncentiveInfo>();
    }

    private IncentiveInfo? ParseIncentive(SCVal scVal)
    {
        if (scVal is SCMap map)
        {
            var result = new IncentiveInfo();
            foreach (var entry in map.Entries)
            {
                if (entry.Key is SCSymbol keySym)
                {
                    var key = keySym.InnerValue;
                    switch (key)
                    {
                        case "id" when entry.Value is SCUint64 idVal:
                            result.Id = idVal.InnerValue;
                            break;
                        case "name" when entry.Value is SCString nameVal:
                            result.Name = nameVal.InnerValue;
                            break;
                        case "description" when entry.Value is SCString descVal:
                            result.Description = descVal.InnerValue;
                            break;
                        case "price" when entry.Value is SCInt128 priceVal:
                            result.Price = (long)((System.Numerics.BigInteger)priceVal.Hi << 64) + (long)priceVal.Lo;
                            break;
                        case "code" when entry.Value is SCString codeVal:
                            result.Code = codeVal.InnerValue;
                            break;
                        case "provider" when entry.Value is SCString provVal:
                            result.Provider = provVal.InnerValue;
                            break;
                        case "available" when entry.Value is SCBool availVal:
                            result.Available = availVal.InnerValue;
                            break;
                        case "redeemed_by" when entry.Value is ScAccountId redeemedVal:
                            result.RedeemedBy = redeemedVal.InnerValue;
                            break;
                    }
                }
            }
            return result;
        }
        return null;
    }

    public async Task<string> ValidateActionWithMetadataAsync(
        string actionId, string userPublicKey, string actionType,
        int ecoaAmount, int qualitativeValue, string qualitativeUnit)
    {
        var adminKeyPair = KeyPair.FromSecretSeed(_adminSecretKey);
        return await SubmitContractCallAsync(
            adminKeyPair,
            _oracleContractId,
            "validate_action",
            new SCVal[]
            {
                new SCString(actionId),
                new ScAccountId(userPublicKey),
                new SCString(actionType),
                new SCInt128(ecoaAmount.ToString()),
                new SCInt128(qualitativeValue.ToString()),
                new SCString(qualitativeUnit)
            }
        );
    }

    public async Task<ActionMetadataInfo?> GetActionMetadataAsync(string actionId)
    {
        try
        {
            var dummyKeyPair = KeyPair.Random();
            var dummyAccount = new Account(dummyKeyPair.AccountId, 0);

            var operation = new InvokeContractOperation(
                _oracleContractId,
                "get_action_metadata",
                new SCVal[] { new SCString(actionId) }
            );

            var transaction = new TransactionBuilder(dummyAccount).AddOperation(operation).Build();
            var simResponse = await _rpcServer.SimulateTransaction(transaction);

            if (simResponse.Results is { Length: > 0 } && simResponse.Results[0].Xdr != null)
            {
                var scVal = SCVal.FromXdrBase64(simResponse.Results[0].Xdr);
                if (scVal is SCMap map)
                {
                    return ParseActionMetadata(map);
                }
            }
        }
        catch
        {
        }

        return null;
    }

    private ActionMetadataInfo? ParseActionMetadata(SCMap map)
    {
        var result = new ActionMetadataInfo();
        foreach (var entry in map.Entries)
        {
            if (entry.Key is SCSymbol keySym && entry.Value is SCString valStr)
            {
                var key = keySym.InnerValue;
                if (key == "action_id") result.ActionId = valStr.InnerValue;
                else if (key == "action_type") result.ActionType = valStr.InnerValue;
                else if (key == "qualitative_unit") result.QualitativeUnit = valStr.InnerValue;
            }
            else if (entry.Key is SCSymbol keySym2 && entry.Value is SCInt128 valInt)
            {
                var key = keySym2.InnerValue;
                var value = (long)((System.Numerics.BigInteger)valInt.Hi << 64) + (long)valInt.Lo;
                if (key == "ecoa_amount") result.EcoaAmount = value;
                else if (key == "qualitative_value") result.QualitativeValue = value;
            }
            else if (entry.Key is SCSymbol keySym3 && entry.Value is SCBool valBool)
            {
                if (keySym3.InnerValue == "validated") result.Validated = valBool.InnerValue;
            }
            else if (entry.Key is SCSymbol keySym4 && entry.Value is SCUint64 valUint)
            {
                if (keySym4.InnerValue == "timestamp") result.Timestamp = valUint.InnerValue;
            }
        }
        return result;
    }

    private async Task<string> SubmitContractCallAsync(
        KeyPair signer,
        string contractId,
        string functionName,
        SCVal[] args,
        bool useFeeBump = false)
    {
        var account = await _rpcServer.GetAccount(signer.AccountId);

        var operation = new InvokeContractOperation(
            contractId,
            functionName,
            args
        );

        var transaction = new TransactionBuilder(account)
            .AddOperation(operation)
            .Build();

        var simResponse = await _rpcServer.SimulateTransaction(transaction);

        if (simResponse.SorobanTransactionData == null)
        {
            throw new Exception($"{functionName} simulation failed: no transaction data");
        }

        transaction.SetSorobanTransactionData(simResponse.SorobanTransactionData);
        transaction.AddResourceFee(simResponse.MinResourceFee ?? 0);

        if (simResponse.SorobanAuthorization is { Length: > 0 })
        {
            transaction.SetSorobanAuthorization(simResponse.SorobanAuthorization);
        }

        transaction.Sign(signer, _network);

        if (useFeeBump)
        {
            var adminKeyPair = KeyPair.FromSecretSeed(_adminSecretKey);
            var feeBump = TransactionBuilder.BuildFeeBumpTransaction(adminKeyPair, transaction);
            feeBump.Sign(adminKeyPair, _network);

            var response = await _rpcServer.SendTransaction(feeBump);

            if (response.Status == SendTransactionResponse.SendTransactionStatus.ERROR)
            {
                throw new Exception($"{functionName} failed");
            }

            return response.Hash;
        }

        var txResponse = await _rpcServer.SendTransaction(transaction);

        if (txResponse.Status == SendTransactionResponse.SendTransactionStatus.ERROR)
        {
            throw new Exception($"{functionName} failed");
        }

        return txResponse.Hash;
    }

    public void Dispose()
    {
        _rpcServer.Dispose();
    }
}
