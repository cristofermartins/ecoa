namespace Ecoa.Core.Entities;

public enum EvidenceType
{
    QRCode,
    GPS,
    Photo
}

public class ActionEvidence
{
    public uint Id { get; set; }
    public uint ActionId { get; set; }
    public EnvironmentalAction Action { get; set; } = null!;
    public EvidenceType Type { get; set; }
    public string Data { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
