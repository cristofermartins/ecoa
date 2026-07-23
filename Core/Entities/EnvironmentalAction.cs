namespace Ecoa.Core.Entities;

public enum ActionType
{
    BikeRide
}

public enum ActionStatus
{
    Pending,
    Validated,
    Rejected,
    TokenIssued
}

public class EnvironmentalAction
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public User User { get; set; } = null!;
    public ActionType Type { get; set; }
    public ActionStatus Status { get; set; } = ActionStatus.Pending;
    public string? QrCodeData { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? DistanceKm { get; set; }
    public int EcoaAmount { get; set; }
    public int QualitativeValue { get; set; }
    public string QualitativeUnit { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ValidatedAt { get; set; }
    public List<ActionEvidence> Evidences { get; set; } = new();
    public List<Validation> Validations { get; set; } = new();
}
