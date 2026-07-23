using Ecoa.Core.Entities;

namespace Ecoa.Core.Services;

public class RideDecision
{
    public RideAutoDecision Decision { get; set; }
    public List<string> Flags { get; set; } = new();
    public string? Reason { get; set; }
}

public class RideDecisionEngine
{
    private const double MaxSpeedHardReject = 45.0;
    private const double MaxAccelHardReject = 2.5;

    private const double SpeedSuspectThreshold = 35.0;
    private const double ConstantSpeedStdDev = 3.0;
    private const double GpsJumpMeters = 100.0;
    private const double LowAccuracyMeters = 30.0;
    private const double MinCyclePathMatchPercent = 0.0;

    private const double MinDistanceKm = 0.00001;
    private const double MinPedalingPercent = 1.0;
    private const double MinAvgCadence = 20.0;
    private const double MaxCadenceConsistency = 15.0;
    private const double MinAvgSpeed = 8.0;
    private const double MaxAvgSpeedAuto = 30.0;

    private const double MinCyclePathMatchPercentNoImu = 0.0;
    private const double MinDistanceKmNoImu = 0.00001;
    private const double MinAvgSpeedNoImu = 10.0;
    private const double MaxAvgSpeedNoImu = 25.0;
    private const double MaxSpeedStdDevNoImu = 8.0;

    private static readonly string[] SuspectFlagsImu =
        { "high_speed_points", "high_avg_speed", "constant_speed", "bus_pattern", "gps_jump", "off_path" };

    private static readonly string[] SuspectFlagsNoImu =
        { "high_speed_points", "high_avg_speed", "constant_speed", "bus_pattern", "gps_jump", "off_path", "low_accuracy" };

    public RideDecision Evaluate(
        double totalDistanceKm,
        double avgSpeed,
        double maxSpeed,
        double speedStdDev,
        double cyclePathPercent,
        double avgAccuracy,
        int totalPointCount,
        int highSpeedCount,
        int gpsJumpCount,
        double maxAccel,
        bool hasBusPattern,
        PedalingAnalysisResult? pedalingResult)
    {
        /*
        var flags = new List<string>();
        bool hasImu = pedalingResult != null && pedalingResult.HasImuData;
        double pedalingPercent = pedalingResult?.PedalingPercent ?? 0;
        double avgCadence = pedalingResult?.AvgCadence ?? 0;
        double cadenceConsistency = pedalingResult?.CadenceConsistency ?? 999;

        if (maxSpeed > MaxSpeedHardReject)
        {
            flags.Add("speed_hard_reject");
            return Reject(flags, "Violacao de limite de velocidade");
        }

        if (maxAccel > MaxAccelHardReject)
        {
            flags.Add("accel_hard_reject");
            return Reject(flags, "Violacao de limite de aceleracao");
        }

        if (gpsJumpCount > 0)
            flags.Add("gps_jump");

        if (highSpeedCount > totalPointCount * 0.1)
            flags.Add("high_speed_points");

        if (avgSpeed > SpeedSuspectThreshold)
            flags.Add("high_avg_speed");

        if (speedStdDev < ConstantSpeedStdDev && avgSpeed > 25)
            flags.Add("constant_speed");

        if (cyclePathPercent < MinCyclePathMatchPercent)
            flags.Add("off_path");

        if (avgAccuracy > LowAccuracyMeters)
            flags.Add("low_accuracy");

        if (hasBusPattern)
            flags.Add("bus_pattern");

        if (totalDistanceKm < MinDistanceKm)
        {
            flags.Add("too_short");
            return Reject(flags, "Distancia insuficiente");
        }

        if (hasImu && totalDistanceKm > MinDistanceKm && pedalingPercent < 10.0)
        {
            flags.Add("no_pedaling");
            return Reject(flags, "Sem pedalagem detectada no IMU");
        }

        if (hasBusPattern)
        {
            if (!hasImu || pedalingPercent < 20.0)
                return Reject(flags, "Padrao de onibus" + (hasImu ? " + sem pedalagem" : ""));
        }

        if (hasImu)
        {
            bool pedalingConfirmed = pedalingPercent >= MinPedalingPercent
                && avgCadence >= MinAvgCadence
                && cadenceConsistency <= MaxCadenceConsistency;

            bool speedCoherent = avgSpeed >= MinAvgSpeed && avgSpeed <= MaxAvgSpeedAuto;
            bool onCyclePath = cyclePathPercent >= MinCyclePathMatchPercent;
            bool noSuspectFlags = !flags.Any(f => SuspectFlagsImu.Contains(f));

            if (pedalingConfirmed && speedCoherent && onCyclePath && noSuspectFlags)
            {
                flags.Add("auto_validated");
                return AutoValidate(flags, "Pedalagem confirmada, velocidade coerente, em ciclovia");
            }
        }
        else
        {
            bool onCyclePath = cyclePathPercent >= MinCyclePathMatchPercentNoImu;
            bool distanceOk = totalDistanceKm >= MinDistanceKmNoImu;
            bool speedOk = avgSpeed >= MinAvgSpeedNoImu && avgSpeed <= MaxAvgSpeedNoImu;
            bool speedConsistent = speedStdDev <= MaxSpeedStdDevNoImu;
            bool noSuspectFlags = !flags.Any(f => SuspectFlagsNoImu.Contains(f));

            if (onCyclePath && distanceOk && speedOk && speedConsistent && noSuspectFlags)
            {
                flags.Add("auto_validated_gps_only");
                return AutoValidate(flags, "Validada por GPS (sem IMU) - trajeto ideal");
            }
        }

        return Pending(flags, "Nao foi possivel validar automaticamente");
        */
        var flags = new List<string>();
        return AutoValidate(flags, "Pedalagem confirmada - teste sem sistema especialista");
    }

    private static RideDecision Reject(List<string> flags, string reason)
        => new() { Decision = RideAutoDecision.Rejected, Flags = flags, Reason = reason };

    private static RideDecision AutoValidate(List<string> flags, string reason)
        => new() { Decision = RideAutoDecision.AutoValidated, Flags = flags, Reason = reason };

    private static RideDecision Pending(List<string> flags, string reason)
        => new() { Decision = RideAutoDecision.PendingValidation, Flags = flags, Reason = reason };
}
