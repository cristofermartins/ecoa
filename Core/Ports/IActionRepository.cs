using Ecoa.Core.Entities;

namespace Ecoa.Core.Ports;

public interface IActionRepository
{
    Task<EnvironmentalAction?> GetByIdAsync(uint id);
    Task<List<EnvironmentalAction>> GetByUserIdAsync(uint userId);
    Task<List<EnvironmentalAction>> GetPendingAsync();
    Task<EnvironmentalAction> CreateAsync(EnvironmentalAction action);
    Task UpdateAsync(EnvironmentalAction action);
}
