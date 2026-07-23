using Ecoa.Core.Entities;
using Ecoa.Core.Ports;
using Ecoa.Core.Utils;

namespace Ecoa.Core.Services;

public class TokenService
{
    private readonly IStellarService _stellarService;
    private readonly ITokenTransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;

    public TokenService(
        IStellarService stellarService,
        ITokenTransactionRepository transactionRepository,
        IUserRepository userRepository)
    {
        _stellarService = stellarService;
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<StellarBalance>> GetBalanceAsync(uint userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<StellarBalance>.Failure("Usuário não encontrado.");

        if (string.IsNullOrEmpty(user.StellarPublicKey))
            return Result<StellarBalance>.Failure("Carteira Stellar não configurada.");

        var balance = await _stellarService.GetEcoaBalanceAsync(user.StellarPublicKey);
        return Result<StellarBalance>.Success(balance);
    }

    public async Task<Result<List<TokenTransaction>>> GetTransactionsAsync(uint userId)
    {
        var transactions = await _transactionRepository.GetByUserIdAsync(userId);
        return Result<List<TokenTransaction>>.Success(transactions);
    }

    public async Task<Result<List<IncentiveInfo>>> GetAvailableIncentivesAsync()
    {
        try
        {
            var incentives = await _stellarService.GetAvailableIncentivesAsync();
            return Result<List<IncentiveInfo>>.Success(incentives);
        }
        catch (Exception ex)
        {
            return Result<List<IncentiveInfo>>.Failure($"Erro ao listar incentivos: {ex.Message}");
        }
    }

    public async Task<Result<IncentiveInfo>> RedeemIncentiveAsync(uint userId, ulong incentiveId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<IncentiveInfo>.Failure("Usuário não encontrado.");

        if (string.IsNullOrEmpty(user.StellarSecretKey))
            return Result<IncentiveInfo>.Failure("Carteira Stellar não configurada.");

        try
        {
            var incentive = await _stellarService.GetIncentiveAsync(incentiveId);
            if (incentive == null)
                return Result<IncentiveInfo>.Failure("Incentivo não encontrado.");

            if (!incentive.Available)
                return Result<IncentiveInfo>.Failure("Incentivo já foi resgatado.");

            var redeemed = await _stellarService.RedeemIncentiveAsync(user.StellarSecretKey, incentiveId);

            var burnTxHash = await _stellarService.BurnEcoaAsync(user.StellarSecretKey, (int)incentive.Price);

            await _transactionRepository.CreateAsync(new TokenTransaction
            {
                UserId = userId,
                Type = TransactionType.Burn,
                Amount = (int)incentive.Price,
                TxHash = burnTxHash,
                Description = $"Resgate de incentivo: {incentive.Name} ({incentive.Price} ECOA)"
            });

            return Result<IncentiveInfo>.Success(redeemed);
        }
        catch (Exception ex)
        {
            return Result<IncentiveInfo>.Failure($"Erro ao resgatar incentivo: {ex.Message}");
        }
    }
}
