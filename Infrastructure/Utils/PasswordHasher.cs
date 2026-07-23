using Microsoft.AspNetCore.Identity;
using Ecoa.Core.Ports;

namespace Ecoa.Infrastructure.Utils;

public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(new object(), password);

    public bool Verify(string password, string hash)
        => _hasher.VerifyHashedPassword(new object(), hash, password) == PasswordVerificationResult.Success;
}
