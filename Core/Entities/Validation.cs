namespace Ecoa.Core.Entities;

public enum ValidationStatus
{
    Pending,
    Approved,
    Rejected
}

public class Validation
{
    public uint Id { get; set; }
    public uint ActionId { get; set; }
    public EnvironmentalAction Action { get; set; } = null!;
    public uint? ValidatorId { get; set; }
    public User? Validator { get; set; }
    public ValidationStatus Status { get; set; } = ValidationStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
