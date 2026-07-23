using Ecoa.Core.Entities;

namespace Ecoa.Core.Ports;

public interface ICyclePathRepository
{
    Task<List<CyclePath>> GetAllAsync();
    Task<CyclePath> CreateAsync(CyclePath cyclePath);
    Task AddRangeAsync(List<CyclePath> cyclePaths);
    Task ClearAllAsync();
}
