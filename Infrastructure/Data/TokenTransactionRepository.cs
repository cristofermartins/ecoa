using Microsoft.EntityFrameworkCore;
using Ecoa.Core.Entities;
using Ecoa.Core.Ports;

namespace Ecoa.Infrastructure.Data;

public class TokenTransactionRepository : ITokenTransactionRepository
{
    private readonly AppDbContext _context;

    public TokenTransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TokenTransaction>> GetByUserIdAsync(uint userId)
        => await _context.TokenTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<TokenTransaction> CreateAsync(TokenTransaction transaction)
    {
        _context.TokenTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }
}
