namespace Ecoa.Core.Ports;

public class StellarWalletInfo
{
    public string PublicKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}

public class StellarBalance
{
    public string AssetCode { get; set; } = string.Empty;
    public string Balance { get; set; } = "0";
}

public class IncentiveInfo
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long Price { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool Available { get; set; }
    public string? RedeemedBy { get; set; }
}

public class ActionMetadataInfo
{
    public string ActionId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public long EcoaAmount { get; set; }
    public long QualitativeValue { get; set; }
    public string QualitativeUnit { get; set; } = string.Empty;
    public ulong Timestamp { get; set; }
    public bool Validated { get; set; }
}

public interface IStellarService
{
    Task<StellarWalletInfo> CreateWalletAsync();
    Task<StellarBalance> GetEcoaBalanceAsync(string publicKey);
    Task<string> MintEcoaAsync(string toPublicKey, int amount);
    Task<string> BurnEcoaAsync(string fromSecretKey, int amount);
    Task<string> TransferEcoaAsync(string fromSecretKey, string toPublicKey, int amount);

    Task<string> AddIncentiveAsync(string name, string description, long price, string code, string provider);
    Task<IncentiveInfo> RedeemIncentiveAsync(string userSecretKey, ulong incentiveId);
    Task<IncentiveInfo?> GetIncentiveAsync(ulong incentiveId);
    Task<List<IncentiveInfo>> GetAllIncentivesAsync();
    Task<List<IncentiveInfo>> GetAvailableIncentivesAsync();

    Task<string> ValidateActionWithMetadataAsync(
        string actionId, string userPublicKey, string actionType,
        int ecoaAmount, int qualitativeValue, string qualitativeUnit);
    Task<ActionMetadataInfo?> GetActionMetadataAsync(string actionId);
}
