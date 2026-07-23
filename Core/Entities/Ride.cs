namespace Ecoa.Core.Entities;

public enum RideStatus
{
    Active,
    PendingValidation,
    Validated,
    Rejected
}

public enum RideAutoDecision
{
    Rejected,
    PendingValidation,
    AutoValidated
}

public class Ride
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public uint UserId { get; set; }
    public User User { get; set; } = null!;
    public RideStatus Status { get; set; } = RideStatus.Active;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public double? TotalDistanceKm { get; set; }
    public double? AvgSpeedKmh { get; set; }
    public double? MaxSpeedKmh { get; set; }
    public double? CyclePathMatchPercent { get; set; }
    public int? EcoaAmount { get; set; }
    public string? Flags { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double? AvgCadence { get; set; }
    public double? PedalingPercent { get; set; }
    public bool? AutoValidated { get; set; }
    public string? Reason { get; set; }
    public List<RidePoint> Points { get; set; } = new();
    public List<RideImuSample> ImuSamples { get; set; } = new();
}
