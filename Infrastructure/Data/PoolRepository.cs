using Microsoft.EntityFrameworkCore;
using Ecoa.Core.Entities;
using Ecoa.Core.Ports;

namespace Ecoa.Infrastructure.Data;

public class PoolRepository : IPoolRepository
{
    private readonly AppDbContext _context;

    public PoolRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IncentivePool?> GetDefaultAsync()
        => await _context.IncentivePools.FirstOrDefaultAsync();

    public async Task<IncentivePool> CreateAsync(IncentivePool pool)
    {
        _context.IncentivePools.Add(pool);
        await _context.SaveChangesAsync();
        return pool;
    }

    public async Task UpdateAsync(IncentivePool pool)
    {
        _context.IncentivePools.Update(pool);
        await _context.SaveChangesAsync();
    }
}
