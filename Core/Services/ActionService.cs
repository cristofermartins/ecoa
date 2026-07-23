using Ecoa.Core.Entities;
using Ecoa.Core.Ports;
using Ecoa.Core.Utils;

namespace Ecoa.Core.Services;

public class ActionService
{
    private readonly IActionRepository _actionRepository;

    public ActionService(IActionRepository actionRepository)
    {
        _actionRepository = actionRepository;
    }

    public async Task<Result<List<EnvironmentalAction>>> GetUserActionsAsync(uint userId)
    {
        var actions = await _actionRepository.GetByUserIdAsync(userId);
        return Result<List<EnvironmentalAction>>.Success(actions);
    }

    public async Task<Result<EnvironmentalAction>> GetActionAsync(uint id)
    {
        var action = await _actionRepository.GetByIdAsync(id);
        if (action == null)
            return Result<EnvironmentalAction>.Failure("Ação não encontrada.");
        return Result<EnvironmentalAction>.Success(action);
    }

    public async Task<Result<List<EnvironmentalAction>>> GetPendingActionsAsync()
    {
        var actions = await _actionRepository.GetPendingAsync();
        return Result<List<EnvironmentalAction>>.Success(actions);
    }
}
