using Ecoa.Core.Entities;

namespace Ecoa.Core.Ports;

public interface ITokenTransactionRepository
{
    Task<List<TokenTransaction>> GetByUserIdAsync(uint userId);
    Task<TokenTransaction> CreateAsync(TokenTransaction transaction);
}
