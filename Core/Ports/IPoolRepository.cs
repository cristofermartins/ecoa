using Ecoa.Core.Entities;

namespace Ecoa.Core.Ports;

public interface IPoolRepository
{
    Task<IncentivePool?> GetDefaultAsync();
    Task<IncentivePool> CreateAsync(IncentivePool pool);
    Task UpdateAsync(IncentivePool pool);
}
