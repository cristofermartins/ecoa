using Microsoft.EntityFrameworkCore;
using Ecoa.Core.Entities;
using Ecoa.Core.Ports;

namespace Ecoa.Infrastructure.Data;

public class CyclePathRepository : ICyclePathRepository
{
    private readonly AppDbContext _context;

    public CyclePathRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CyclePath>> GetAllAsync()
        => await _context.CyclePaths.ToListAsync();

    public async Task<CyclePath> CreateAsync(CyclePath cyclePath)
    {
        _context.CyclePaths.Add(cyclePath);
        await _context.SaveChangesAsync();
        return cyclePath;
    }

    public async Task AddRangeAsync(List<CyclePath> cyclePaths)
    {
        _context.CyclePaths.AddRange(cyclePaths);
        await _context.SaveChangesAsync();
    }

    public async Task ClearAllAsync()
    {
        var all = await _context.CyclePaths.ToListAsync();
        _context.CyclePaths.RemoveRange(all);
        await _context.SaveChangesAsync();
    }
}
