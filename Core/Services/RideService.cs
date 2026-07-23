using System.Text.Json;
using Ecoa.Core.Dtos;
using Ecoa.Core.Entities;
using Ecoa.Core.Ports;
using Ecoa.Core.Utils;

namespace Ecoa.Core.Services;

public class RideService
{
    private readonly IRideRepository _rideRepository;
    private readonly ICyclePathRepository _cyclePathRepository;
    private readonly IActionRepository _actionRepository;
    private readonly IStellarService _stellarService;
    private readonly ITokenTransactionRepository _tokenTransactionRepository;
    private readonly IUserRepository _userRepository;

    private const double CyclePathProximityMeters = 25.0;
    private const double AccuracyFilterMeters = 20.0;
    private const int EcoaPerKm = 50000000;

    public RideService(
        IRideRepository rideRepository,
        ICyclePathRepository cyclePathRepository,
        IActionRepository actionRepository,
        IStellarService stellarService,
        ITokenTransactionRepository tokenTransactionRepository,
        IUserRepository userRepository)
    {
        _rideRepository = rideRepository;
        _cyclePathRepository = cyclePathRepository;
        _actionRepository = actionRepository;
        _stellarService = stellarService;
        _tokenTransactionRepository = tokenTransactionRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<RideResponse>> StartRideAsync(uint userId)
    {
        var existing = await _rideRepository.GetActiveByUserIdAsync(userId);
        if (existing != null)
            return Result<RideResponse>.Failure("Você já possui uma pedalada ativa. Finalize-a antes de iniciar outra.");

        var ride = new Ride
        {
            UserId = userId,
            Status = RideStatus.Active,
            StartedAt = DateTime.UtcNow
        };

        var created = await _rideRepository.CreateAsync(ride);
        created.User = (await _userRepository.GetByIdAsync(userId))!;
        return Result<RideResponse>.Success(created.ToResponse());
    }

    public async Task<Result<RideResponse>> GetActiveRideAsync(uint userId)
    {
        var ride = await _rideRepository.GetActiveByUserIdAsync(userId);
        if (ride == null)
            return Result<RideResponse>.Failure("Nenhuma pedalada ativa.");
        return Result<RideResponse>.Success(ride.ToResponse());
    }

    public async Task<Result> AddPointsAsync(Guid rideId, List<AddPointRequest> points)
    {
        var ride = await _rideRepository.GetByIdAsync(rideId);
        if (ride == null)
            return Result.Failure("Pedalada não encontrada.");
        if (ride.Status != RideStatus.Active)
            return Result.Failure("Pedalada não está ativa.");

        var cyclePaths = await _cyclePathRepository.GetAllAsync();

        var ridePoints = new List<RidePoint>();
        foreach (var p in points)
        {
            var (nearPath, distToPath) = CheckCyclePathProximity(p.Latitude, p.Longitude, cyclePaths);

            ridePoints.Add(new RidePoint
            {
                RideId = rideId,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Accuracy = p.Accuracy,
                Speed = p.Speed,
                RecordedAt = p.RecordedAt,
                NearCyclePath = nearPath,
                DistanceToPath = distToPath
            });
        }

        await _rideRepository.AddPointsAsync(ridePoints);
        return Result.Success();
    }

    public async Task<Result<RideResponse>> StopRideAsync(Guid rideId)
    {
        var ride = await _rideRepository.GetByIdWithPointsAndImuAsync(rideId);
        if (ride == null)
            return Result<RideResponse>.Failure("Pedalada não encontrada.");
        if (ride.Status != RideStatus.Active)
            return Result<RideResponse>.Failure("Pedalada não está ativa.");

        ride.EndedAt = DateTime.UtcNow;

        var points = ride.Points.OrderBy(p => p.RecordedAt).ToList();
        if (points.Count < 2)
        {
            ride.Status = RideStatus.Rejected;
            ride.Flags = "insufficient_points";
            await _rideRepository.UpdateAsync(ride);
            return Result<RideResponse>.Failure("Pedalada com pontos insuficientes.");
        }

        double totalDistance = 0;
        var speeds = new List<double>();
        double maxSpeed = 0;
        int highSpeedCount = 0;
        int gpsJumpCount = 0;
        double maxAccel = 0;
        int nearPathCount = 0;
        double totalAccuracy = 0;
        double? prevSpeed = null;
        RidePoint? lastAccuratePoint = null;

        for (int i = 0; i < points.Count; i++)
        {
            var curr = points[i];

            if (curr.Accuracy.HasValue)
                totalAccuracy += curr.Accuracy.Value;

            if (curr.NearCyclePath == true)
                nearPathCount++;

            if (curr.Accuracy.HasValue && curr.Accuracy.Value > AccuracyFilterMeters)
                continue;

            if (lastAccuratePoint != null)
            {
                var prev = lastAccuratePoint;

                double deltaTime = (curr.RecordedAt - prev.RecordedAt).TotalSeconds;
                if (deltaTime <= 0) continue;

                double deltaDist = HaversineDistance(prev.Latitude, prev.Longitude, curr.Latitude, curr.Longitude);
                totalDistance += deltaDist;

                double speedKmh = (deltaDist / deltaTime) * 3600.0;
                speeds.Add(speedKmh);

                if (speedKmh > maxSpeed)
                    maxSpeed = speedKmh;

                if (prevSpeed.HasValue)
                {
                    double accel = Math.Abs(speedKmh - prevSpeed.Value) / deltaTime;
                    if (accel > maxAccel)
                        maxAccel = accel;
                }

                prevSpeed = speedKmh;
            }

            lastAccuratePoint = curr;
        }

        double totalTimeHours = (points[^1].RecordedAt - points[0].RecordedAt).TotalHours;
        double avgSpeed = totalTimeHours > 0 ? totalDistance / totalTimeHours : 0;
        double stdDev = CalculateStdDev(speeds);
        double cyclePathPercent = points.Count > 0 ? (double)nearPathCount / points.Count * 100 : 0;
        double avgAccuracy = points.Count > 0 ? totalAccuracy / points.Count : 0;
        bool hasBusPattern = DetectBusPattern(points);

        PedalingAnalysisResult? pedalingResult = null;

        if (ride.ImuSamples.Count > 0)
        {
            var analyzer = new PedalingAnalyzer();
            pedalingResult = analyzer.Analyze(ride.ImuSamples);
        }

        var decisionEngine = new RideDecisionEngine();
        var decision = decisionEngine.Evaluate(
            totalDistance, avgSpeed, maxSpeed, stdDev,
            cyclePathPercent, avgAccuracy, points.Count,
            highSpeedCount, gpsJumpCount, maxAccel,
            hasBusPattern, pedalingResult);

        ride.TotalDistanceKm = Math.Round(totalDistance, 2);
        ride.AvgSpeedKmh = Math.Round(avgSpeed, 1);
        ride.MaxSpeedKmh = Math.Round(maxSpeed, 1);
        ride.CyclePathMatchPercent = Math.Round(cyclePathPercent, 1);
        ride.AvgCadence = Math.Round(pedalingResult?.AvgCadence ?? 0, 1);
        ride.PedalingPercent = Math.Round(pedalingResult?.PedalingPercent ?? 0, 1);
        ride.Flags = decision.Flags.Count > 0 ? string.Join(",", decision.Flags) : null;
        ride.Reason = decision.Reason;

        switch (decision.Decision)
        {
            case RideAutoDecision.Rejected:
                ride.Status = RideStatus.Rejected;
                break;
            case RideAutoDecision.PendingValidation:
            case RideAutoDecision.AutoValidated:
                ride.Status = RideStatus.Validated;
                ride.AutoValidated = true;
                await ValidateRideInternalAsync(ride);
                break;
        }

        await _rideRepository.UpdateAsync(ride);
        return Result<RideResponse>.Success(ride.ToResponse());
    }

    public async Task<Result<RideResponse>> ValidateRideAsync(Guid rideId, bool approved, uint adminId, string? notes)
    {
        var ride = await _rideRepository.GetByIdWithPointsAndImuAsync(rideId);
        if (ride == null)
            return Result<RideResponse>.Failure("Pedalada não encontrada.");
        if (ride.Status != RideStatus.PendingValidation)
            return Result<RideResponse>.Failure("Pedalada não está pendente de validação.");

        if (!approved)
        {
            ride.Status = RideStatus.Rejected;
            await _rideRepository.UpdateAsync(ride);
            return Result<RideResponse>.Success(ride.ToResponse());
        }

        ride.Status = RideStatus.Validated;
        await ValidateRideInternalAsync(ride);
        await _rideRepository.UpdateAsync(ride);
        return Result<RideResponse>.Success(ride.ToResponse());
    }

    private async Task ValidateRideInternalAsync(Ride ride)
    {
        var ecoaAmount = (int)((ride.TotalDistanceKm ?? 0) * EcoaPerKm);
        ride.EcoaAmount = ecoaAmount;

        var action = new EnvironmentalAction
        {
            UserId = ride.UserId,
            Type = ActionType.BikeRide,
            Latitude = ride.Points.FirstOrDefault()?.Latitude,
            Longitude = ride.Points.FirstOrDefault()?.Longitude,
            DistanceKm = ride.TotalDistanceKm,
            EcoaAmount = ecoaAmount,
            QualitativeValue = (int)(ride.TotalDistanceKm ?? 0),
            QualitativeUnit = "km",
            Status = ActionStatus.Validated,
            ValidatedAt = DateTime.UtcNow,
            Evidences = { new ActionEvidence { Type = EvidenceType.GPS, Data = $"ride:{ride.Id}" } }
        };

        await _actionRepository.CreateAsync(action);

        var user = await _userRepository.GetByIdAsync(ride.UserId);
        if (user != null && string.IsNullOrEmpty(user.StellarPublicKey))
        {
            var wallet = await _stellarService.CreateWalletAsync();
            user.StellarPublicKey = wallet.PublicKey;
            user.StellarSecretKey = wallet.SecretKey;
            await _userRepository.UpdateAsync(user);
        }

        if (user?.StellarPublicKey != null)
        {
            try
            {
                var txHash = await _stellarService.MintEcoaAsync(user.StellarPublicKey, ecoaAmount);
                action.Status = ActionStatus.TokenIssued;
                await _actionRepository.UpdateAsync(action);

                await _stellarService.ValidateActionWithMetadataAsync(
                    action.Id.ToString(),
                    user.StellarPublicKey,
                    action.Type.ToString(),
                    ecoaAmount,
                    action.QualitativeValue,
                    action.QualitativeUnit
                );

                var metadata = JsonSerializer.Serialize(new
                {
                    action_type = action.Type.ToString(),
                    qualitative_value = action.QualitativeValue,
                    qualitative_unit = action.QualitativeUnit,
                    distance_km = ride.TotalDistanceKm,
                    ride_id = ride.Id.ToString()
                });

                await _tokenTransactionRepository.CreateAsync(new TokenTransaction
                {
                    UserId = ride.UserId,
                    ActionId = action.Id,
                    Type = TransactionType.Mint,
                    Amount = ecoaAmount,
                    TxHash = txHash,
                    Metadata = metadata,
                    Description = $"ECOA emitido por pedalada verificada: {ride.TotalDistanceKm}km"
                });
            }
            catch
            {
            }
        }
    }

    public async Task<Result<RideResponse>> GetRideAsync(Guid id)
    {
        var ride = await _rideRepository.GetByIdWithPointsAsync(id);
        if (ride == null)
            return Result<RideResponse>.Failure("Pedalada não encontrada.");
        return Result<RideResponse>.Success(ride.ToResponse());
    }

    public async Task<Result<List<RidePointResponse>>> GetRidePointsAsync(Guid rideId)
    {
        var points = await _rideRepository.GetPointsByRideIdAsync(rideId);
        return Result<List<RidePointResponse>>.Success(points.Select(p => p.ToResponse()).ToList());
    }

    public async Task<Result> AddImuSamplesAsync(Guid rideId, List<AddImuSampleRequest> samples)
    {
        var ride = await _rideRepository.GetByIdAsync(rideId);
        if (ride == null)
            return Result.Failure("Pedalada não encontrada.");
        if (ride.Status != RideStatus.Active)
            return Result.Failure("Pedalada não está ativa.");

        var imuSamples = samples.Select(s => new RideImuSample
        {
            RideId = rideId,
            RecordedAt = s.RecordedAt,
            AccelX = s.AccelX,
            AccelY = s.AccelY,
            AccelZ = s.AccelZ,
            GyroX = s.GyroX,
            GyroY = s.GyroY,
            GyroZ = s.GyroZ
        }).ToList();

        await _rideRepository.AddImuSamplesAsync(imuSamples);
        return Result.Success();
    }

    public async Task<Result<List<RideImuSampleResponse>>> GetImuSamplesAsync(Guid rideId)
    {
        var samples = await _rideRepository.GetImuSamplesByRideIdAsync(rideId);
        return Result<List<RideImuSampleResponse>>.Success(samples.Select(s => s.ToResponse()).ToList());
    }

    public async Task<Result<List<RideResponse>>> GetUserRidesAsync(uint userId)
    {
        var rides = await _rideRepository.GetByUserIdAsync(userId);
        return Result<List<RideResponse>>.Success(rides.Select(r => r.ToResponse()).ToList());
    }

    public async Task<Result<List<RideResponse>>> GetPendingRidesAsync()
    {
        var rides = await _rideRepository.GetPendingValidationAsync();
        return Result<List<RideResponse>>.Success(rides.Select(r => r.ToResponse()).ToList());
    }

    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0;
        double dLat = ToRad(lat2 - lat1);
        double dLon = ToRad(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;

    private static double CalculateStdDev(List<double> values)
    {
        if (values.Count == 0) return 0;
        double avg = values.Average();
        double sumSq = values.Sum(v => (v - avg) * (v - avg));
        return Math.Sqrt(sumSq / values.Count);
    }

    private static (bool nearPath, double? distance) CheckCyclePathProximity(
        double lat, double lng, List<CyclePath> cyclePaths)
    {
        if (cyclePaths.Count == 0)
            return (false, null);

        double minDist = double.MaxValue;

        foreach (var cp in cyclePaths)
        {
            if (lat < cp.MinLatitude - 0.01 || lat > cp.MaxLatitude + 0.01 ||
                lng < cp.MinLongitude - 0.01 || lng > cp.MaxLongitude + 0.01)
                continue;

            try
            {
                var geoJson = JsonSerializer.Deserialize<GeoJsonFeature>(cp.GeoJson);
                if (geoJson?.geometry?.coordinates == null) continue;

                var coords = geoJson.geometry.coordinates;
                for (int i = 1; i < coords.Count; i++)
                {
                    double dist = PointToSegmentDistance(
                        lat, lng,
                        coords[i - 1][1], coords[i - 1][0],
                        coords[i][1], coords[i][0]);
                    if (dist < minDist)
                        minDist = dist;
                }
            }
            catch
            {
                continue;
            }
        }

        double distMeters = minDist * 1000.0;
        return (distMeters <= CyclePathProximityMeters, Math.Round(distMeters, 1));
    }

    private static double PointToSegmentDistance(
        double px, double py,
        double ax, double ay,
        double bx, double by)
    {
        double abx = bx - ax;
        double aby = by - ay;
        double apx = px - ax;
        double apy = py - ay;
        double abLenSq = abx * abx + aby * aby;
        double t = abLenSq > 0 ? Math.Max(0, Math.Min(1, (apx * abx + apy * aby) / abLenSq)) : 0;
        double projX = ax + t * abx;
        double projY = ay + t * aby;
        return HaversineDistance(px, py, projX, projY);
    }

    private static bool DetectBusPattern(List<RidePoint> points)
    {
        if (points.Count < 10) return false;

        var stops = new List<(double dist, double duration)>();
        double cumulativeDist = 0;
        DateTime? stopStart = null;

        for (int i = 1; i < points.Count; i++)
        {
            double deltaDist = HaversineDistance(
                points[i - 1].Latitude, points[i - 1].Longitude,
                points[i].Latitude, points[i].Longitude);
            double deltaTime = (points[i].RecordedAt - points[i - 1].RecordedAt).TotalSeconds;

            if (deltaTime <= 0) continue;

            double speed = (deltaDist / deltaTime) * 3600.0;

            if (speed < 2.0)
            {
                if (stopStart == null)
                    stopStart = points[i - 1].RecordedAt;
            }
            else
            {
                if (stopStart != null)
                {
                    double stopDuration = (points[i].RecordedAt - stopStart.Value).TotalSeconds;
                    if (stopDuration >= 10 && stopDuration <= 40)
                        stops.Add((cumulativeDist, stopDuration));
                    stopStart = null;
                }
            }

            cumulativeDist += deltaDist;
        }

        if (stops.Count < 2) return false;

        var distancesBetweenStops = new List<double>();
        for (int i = 1; i < stops.Count; i++)
            distancesBetweenStops.Add(stops[i].dist - stops[i - 1].dist);

        if (distancesBetweenStops.Count == 0) return false;

        double avgDist = distancesBetweenStops.Average();
        double stdDist = Math.Sqrt(distancesBetweenStops.Sum(d => (d - avgDist) * (d - avgDist)) / distancesBetweenStops.Count);

        return avgDist >= 0.3 && avgDist <= 0.8 && stdDist < 0.2;
    }

    public record AddPointRequest(double Latitude, double Longitude, double? Accuracy, double? Speed, DateTime RecordedAt);
    public record AddImuSampleRequest(DateTime RecordedAt, double AccelX, double AccelY, double AccelZ, double GyroX, double GyroY, double GyroZ);

    private class GeoJsonFeature
    {
        public GeoJsonGeometry? geometry { get; set; }
    }

    private class GeoJsonGeometry
    {
        public List<List<double>>? coordinates { get; set; }
    }
}
