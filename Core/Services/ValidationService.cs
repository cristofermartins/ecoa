using Ecoa.Core.Entities;
using Ecoa.Core.Ports;
using Ecoa.Core.Utils;

namespace Ecoa.Core.Services;

public class ValidationService
{
    private readonly IValidationRepository _validationRepository;
    private readonly IActionRepository _actionRepository;
    private readonly IStellarService _stellarService;
    private readonly ITokenTransactionRepository _tokenTransactionRepository;
    private readonly IUserRepository _userRepository;

    public ValidationService(
        IValidationRepository validationRepository,
        IActionRepository actionRepository,
        IStellarService stellarService,
        ITokenTransactionRepository tokenTransactionRepository,
        IUserRepository userRepository)
    {
        _validationRepository = validationRepository;
        _actionRepository = actionRepository;
        _stellarService = stellarService;
        _tokenTransactionRepository = tokenTransactionRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<Validation>> ApproveActionAsync(uint actionId, uint validatorId)
    {
        var action = await _actionRepository.GetByIdAsync(actionId);
        if (action == null)
            return Result<Validation>.Failure("Ação não encontrada.");

        if (action.Status != ActionStatus.Pending)
            return Result<Validation>.Failure("Ação já foi processada.");

        var validation = new Validation
        {
            ActionId = actionId,
            ValidatorId = validatorId,
            Status = ValidationStatus.Approved,
            ReviewedAt = DateTime.UtcNow
        };

        var created = await _validationRepository.CreateAsync(validation);

        action.Status = ActionStatus.Validated;
        action.ValidatedAt = DateTime.UtcNow;
        await _actionRepository.UpdateAsync(action);

        var user = await _userRepository.GetByIdAsync(action.UserId);
        if (user != null && string.IsNullOrEmpty(user.StellarPublicKey))
        {
            var wallet = await _stellarService.CreateWalletAsync();
            user.StellarPublicKey = wallet.PublicKey;
            user.StellarSecretKey = wallet.SecretKey;
            await _userRepository.UpdateAsync(user);
        }

        if (user?.StellarPublicKey != null)
        {
            try
            {
                var txHash = await _stellarService.MintEcoaAsync(user.StellarPublicKey, action.EcoaAmount);
                action.Status = ActionStatus.TokenIssued;
                await _actionRepository.UpdateAsync(action);

                var qualitativeValue = action.QualitativeValue > 0 ? action.QualitativeValue : (int)(action.DistanceKm ?? 0);
                var qualitativeUnit = !string.IsNullOrEmpty(action.QualitativeUnit) ? action.QualitativeUnit : "km";

                await _stellarService.ValidateActionWithMetadataAsync(
                    actionId.ToString(),
                    user.StellarPublicKey,
                    action.Type.ToString(),
                    action.EcoaAmount,
                    qualitativeValue,
                    qualitativeUnit
                );

                var metadata = System.Text.Json.JsonSerializer.Serialize(new
                {
                    action_type = action.Type.ToString(),
                    qualitative_value = qualitativeValue,
                    qualitative_unit = qualitativeUnit,
                    distance_km = action.DistanceKm
                });

                await _tokenTransactionRepository.CreateAsync(new TokenTransaction
                {
                    UserId = action.UserId,
                    ActionId = actionId,
                    Type = TransactionType.Mint,
                    Amount = action.EcoaAmount,
                    TxHash = txHash,
                    Metadata = metadata,
                    Description = $"ECOA emitido por ação: {action.Type}"
                });
            }
            catch
            {
            }
        }

        return Result<Validation>.Success(created);
    }

    public async Task<Result<Validation>> RejectActionAsync(uint actionId, uint validatorId, string notes)
    {
        var action = await _actionRepository.GetByIdAsync(actionId);
        if (action == null)
            return Result<Validation>.Failure("Ação não encontrada.");

        var validation = new Validation
        {
            ActionId = actionId,
            ValidatorId = validatorId,
            Status = ValidationStatus.Rejected,
            Notes = notes,
            ReviewedAt = DateTime.UtcNow
        };

        var created = await _validationRepository.CreateAsync(validation);

        action.Status = ActionStatus.Rejected;
        await _actionRepository.UpdateAsync(action);

        return Result<Validation>.Success(created);
    }
}
