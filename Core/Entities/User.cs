namespace Ecoa.Core.Entities;

public enum UserRole
{
    Fazedor,
    Admin
}

public class User
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Fazedor;
    public string? StellarPublicKey { get; set; }
    public string? StellarSecretKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
