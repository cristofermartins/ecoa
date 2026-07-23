using Ecoa.Core.Entities;

namespace Ecoa.Core.Ports;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(uint id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CpfExistsAsync(string cpf);
}
