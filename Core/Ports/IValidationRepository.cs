using Ecoa.Core.Entities;

namespace Ecoa.Core.Ports;

public interface IValidationRepository
{
    Task<Validation?> GetByIdAsync(uint id);
    Task<List<Validation>> GetPendingByActionIdAsync(uint actionId);
    Task<Validation> CreateAsync(Validation validation);
    Task UpdateAsync(Validation validation);
}
