using Microsoft.EntityFrameworkCore;
using Ecoa.Core.Entities;
using Ecoa.Core.Ports;

namespace Ecoa.Infrastructure.Data;

public class ValidationRepository : IValidationRepository
{
    private readonly AppDbContext _context;

    public ValidationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Validation?> GetByIdAsync(uint id)
        => await _context.Validations.FindAsync(id);

    public async Task<List<Validation>> GetPendingByActionIdAsync(uint actionId)
        => await _context.Validations
            .Where(v => v.ActionId == actionId && v.Status == ValidationStatus.Pending)
            .ToListAsync();

    public async Task<Validation> CreateAsync(Validation validation)
    {
        _context.Validations.Add(validation);
        await _context.SaveChangesAsync();
        return validation;
    }

    public async Task UpdateAsync(Validation validation)
    {
        _context.Validations.Update(validation);
        await _context.SaveChangesAsync();
    }
}
