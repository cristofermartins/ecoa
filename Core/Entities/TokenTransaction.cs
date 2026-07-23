namespace Ecoa.Core.Entities;

public enum TransactionType
{
    Mint,
    Burn,
    Transfer
}

public class TokenTransaction
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public User User { get; set; } = null!;
    public TransactionType Type { get; set; }
    public int Amount { get; set; }
    public string? TxHash { get; set; }
    public uint? ActionId { get; set; }
    public EnvironmentalAction? Action { get; set; }
    public string? Metadata { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
