using Microsoft.EntityFrameworkCore;
using Ecoa.Core.Entities;
using Ecoa.Core.Ports;

namespace Ecoa.Infrastructure.Data;

public class ActionRepository : IActionRepository
{
    private readonly AppDbContext _context;

    public ActionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EnvironmentalAction?> GetByIdAsync(uint id)
        => await _context.EnvironmentalActions
            .Include(a => a.Evidences)
            .Include(a => a.Validations)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<List<EnvironmentalAction>> GetByUserIdAsync(uint userId)
        => await _context.EnvironmentalActions
            .Include(a => a.Evidences)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<List<EnvironmentalAction>> GetPendingAsync()
        => await _context.EnvironmentalActions
            .Include(a => a.Evidences)
            .Include(a => a.User)
            .Where(a => a.Status == ActionStatus.Pending)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

    public async Task<EnvironmentalAction> CreateAsync(EnvironmentalAction action)
    {
        _context.EnvironmentalActions.Add(action);
        await _context.SaveChangesAsync();
        return action;
    }

    public async Task UpdateAsync(EnvironmentalAction action)
    {
        _context.EnvironmentalActions.Update(action);
        await _context.SaveChangesAsync();
    }
}
