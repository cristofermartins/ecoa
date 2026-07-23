namespace Ecoa.Core.Dtos;

public class UserBriefResponse
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RidePointResponse
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Accuracy { get; set; }
    public double? Speed { get; set; }
    public DateTime RecordedAt { get; set; }
    public bool? NearCyclePath { get; set; }
    public double? DistanceToPath { get; set; }
}

public class RideImuSampleResponse
{
    public Guid Id { get; set; }
    public DateTime RecordedAt { get; set; }
    public double AccelX { get; set; }
    public double AccelY { get; set; }
    public double AccelZ { get; set; }
    public double GyroX { get; set; }
    public double GyroY { get; set; }
    public double GyroZ { get; set; }
}

public class RideResponse
{
    public Guid Id { get; set; }
    public UserBriefResponse User { get; set; } = null!;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public double? TotalDistanceKm { get; set; }
    public double? AvgSpeedKmh { get; set; }
    public double? MaxSpeedKmh { get; set; }
    public double? CyclePathMatchPercent { get; set; }
    public int? EcoaAmount { get; set; }
    public string? Flags { get; set; }
    public DateTime CreatedAt { get; set; }
    public double? AvgCadence { get; set; }
    public double? PedalingPercent { get; set; }
    public bool? AutoValidated { get; set; }
    public string? Reason { get; set; }
    public List<RidePointResponse> Points { get; set; } = new();
    public List<RideImuSampleResponse> ImuSamples { get; set; } = new();
}

public static class RideDtoMapper
{
    public static UserBriefResponse ToBriefResponse(this Entities.User user)
    {
        return new UserBriefResponse
        {
            Id = user.Id,
            Name = user.Name
        };
    }

    public static RidePointResponse ToResponse(this Entities.RidePoint point)
    {
        return new RidePointResponse
        {
            Id = point.Id,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            Accuracy = point.Accuracy,
            Speed = point.Speed,
            RecordedAt = point.RecordedAt,
            NearCyclePath = point.NearCyclePath,
            DistanceToPath = point.DistanceToPath
        };
    }

    public static RideImuSampleResponse ToResponse(this Entities.RideImuSample sample)
    {
        return new RideImuSampleResponse
        {
            Id = sample.Id,
            RecordedAt = sample.RecordedAt,
            AccelX = sample.AccelX,
            AccelY = sample.AccelY,
            AccelZ = sample.AccelZ,
            GyroX = sample.GyroX,
            GyroY = sample.GyroY,
            GyroZ = sample.GyroZ
        };
    }

    public static RideResponse ToResponse(this Entities.Ride ride)
    {
        return new RideResponse
        {
            Id = ride.Id,
            User = ride.User.ToBriefResponse(),
            Status = ride.Status.ToString(),
            StartedAt = ride.StartedAt,
            EndedAt = ride.EndedAt,
            TotalDistanceKm = ride.TotalDistanceKm,
            AvgSpeedKmh = ride.AvgSpeedKmh,
            MaxSpeedKmh = ride.MaxSpeedKmh,
            CyclePathMatchPercent = ride.CyclePathMatchPercent,
            EcoaAmount = ride.EcoaAmount,
            Flags = ride.Flags,
            CreatedAt = ride.CreatedAt,
            AvgCadence = ride.AvgCadence,
            PedalingPercent = ride.PedalingPercent,
            AutoValidated = ride.AutoValidated,
            Reason = ride.Reason,
            Points = ride.Points.Select(p => p.ToResponse()).ToList(),
            ImuSamples = ride.ImuSamples.Select(s => s.ToResponse()).ToList()
        };
    }
}
