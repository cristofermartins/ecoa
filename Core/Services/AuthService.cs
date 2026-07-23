using Ecoa.Core.Entities;
using Ecoa.Core.Ports;
using Ecoa.Core.Utils;

namespace Ecoa.Core.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IStellarService _stellarService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IStellarService stellarService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _stellarService = stellarService;
    }

    public async Task<Result<User>> RegisterAsync(string name, string email, string cpf, string password)
    {
        if (await _userRepository.EmailExistsAsync(email))
            return Result<User>.Failure("Email já cadastrado.");

        if (await _userRepository.CpfExistsAsync(cpf))
            return Result<User>.Failure("CPF já cadastrado.");

        var wallet = await _stellarService.CreateWalletAsync();

        var user = new User
        {
            Name = name,
            Email = email,
            Cpf = cpf,
            Password = _passwordHasher.Hash(password),
            Role = UserRole.Fazedor,
            StellarPublicKey = wallet.PublicKey,
            StellarSecretKey = wallet.SecretKey
        };

        var created = await _userRepository.CreateAsync(user);
        return Result<User>.Success(created);
    }

    public async Task<Result<User>> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return Result<User>.Failure("Email ou senha inválidos.");

        if (!_passwordHasher.Verify(password, user.Password))
            return Result<User>.Failure("Email ou senha inválidos.");

        return Result<User>.Success(user);
    }

    public async Task<Result<User>> GetUserByIdAsync(uint id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return Result<User>.Failure("Usuário não encontrado.");
        return Result<User>.Success(user);
    }

    public async Task<Result> UpdateStellarKeysAsync(uint userId, string publicKey, string secretKey)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure("Usuário não encontrado.");

        user.StellarPublicKey = publicKey;
        user.StellarSecretKey = secretKey;
        await _userRepository.UpdateAsync(user);
        return Result.Success();
    }

    public async Task EnsureWalletAsync(uint userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !string.IsNullOrEmpty(user.StellarPublicKey))
            return;

        var wallet = await _stellarService.CreateWalletAsync();
        user.StellarPublicKey = wallet.PublicKey;
        user.StellarSecretKey = wallet.SecretKey;
        await _userRepository.UpdateAsync(user);
    }
}
