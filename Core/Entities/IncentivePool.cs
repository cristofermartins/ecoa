namespace Ecoa.Core.Entities;

public class IncentivePool
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
