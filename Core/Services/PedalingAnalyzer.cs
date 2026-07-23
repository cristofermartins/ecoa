using Ecoa.Core.Entities;

namespace Ecoa.Core.Services;

public class PedalingAnalysisResult
{
    public double AvgCadence { get; set; }
    public double PedalingPercent { get; set; }
    public int TotalWindows { get; set; }
    public int PedalingWindows { get; set; }
    public double CadenceConsistency { get; set; }
    public bool HasImuData { get; set; }
    public bool PedalingDetected => PedalingPercent > 20;
}

public class PedalingAnalyzer
{
    private const double WindowSizeSeconds = 10.0;
    private const double WindowStepSeconds = 5.0;
    private const double MinPedalingFreq = 0.5;
    private const double MaxPedalingFreq = 2.5;
    private const double AutocorrThreshold = 0.3;
    private const int MinSamplesPerWindow = 100;

    public PedalingAnalysisResult Analyze(List<RideImuSample> samples)
    {
        var result = new PedalingAnalysisResult { HasImuData = samples.Count > 0 };

        if (samples.Count < MinSamplesPerWindow)
        {
            result.AvgCadence = 0;
            result.PedalingPercent = 0;
            result.CadenceConsistency = 999;
            return result;
        }

        var ordered = samples.OrderBy(s => s.RecordedAt).ToList();
        double duration = (ordered[^1].RecordedAt - ordered[0].RecordedAt).TotalSeconds;
        if (duration <= 0)
        {
            result.AvgCadence = 0;
            result.PedalingPercent = 0;
            result.CadenceConsistency = 999;
            return result;
        }

        var cadences = new List<double>();
        int totalWindows = 0;

        double windowStartOffset = 0;
        while (windowStartOffset + WindowSizeSeconds <= duration)
        {
            var windowStart = ordered[0].RecordedAt.AddSeconds(windowStartOffset);
            var windowEnd = windowStart.AddSeconds(WindowSizeSeconds);

            var windowSamples = ordered
                .Where(s => s.RecordedAt >= windowStart && s.RecordedAt < windowEnd)
                .ToList();

            if (windowSamples.Count >= MinSamplesPerWindow)
            {
                double sampleRate = windowSamples.Count /
                    (windowSamples[^1].RecordedAt - windowSamples[0].RecordedAt).TotalSeconds;

                if (sampleRate >= 10.0)
                {
                    double cadence = EstimateCadenceFromBestAxis(windowSamples, sampleRate);
                    totalWindows++;
                    if (cadence > 0)
                        cadences.Add(cadence);
                }
                else
                {
                    totalWindows++;
                }
            }

            windowStartOffset += WindowStepSeconds;
        }

        result.TotalWindows = totalWindows;
        result.PedalingWindows = cadences.Count;
        result.PedalingPercent = totalWindows > 0
            ? (double)cadences.Count / totalWindows * 100.0
            : 0;
        result.AvgCadence = cadences.Count > 0 ? cadences.Average() : 0;
        result.CadenceConsistency = cadences.Count > 1 ? CalculateStdDev(cadences) : 999;

        return result;
    }

    private static double EstimateCadenceFromBestAxis(List<RideImuSample> samples, double sampleRate)
    {
        double[] signalX = samples.Select(s => s.AccelX).ToArray();
        double[] signalY = samples.Select(s => s.AccelY).ToArray();
        double[] signalZ = samples.Select(s => s.AccelZ).ToArray();

        double varX = BandPassVariance(signalX);
        double varY = BandPassVariance(signalY);
        double varZ = BandPassVariance(signalZ);

        double[] bestSignal = signalX;
        if (varY >= varX && varY >= varZ) bestSignal = signalY;
        else if (varZ >= varX && varZ >= varY) bestSignal = signalZ;

        return EstimateCadence(bestSignal, sampleRate);
    }

    private static double BandPassVariance(double[] signal)
    {
        double[] filtered = BandPassFilter(signal);
        double mean = filtered.Average();
        return filtered.Sum(v => (v - mean) * (v - mean)) / filtered.Length;
    }

    private static double[] BandPassFilter(double[] signal)
    {
        if (signal.Length < 10) return signal;

        double[] highPass = HighPassFilter(signal, 5);
        double[] lowPass = LowPassFilter(highPass, 3);
        return lowPass;
    }

    private static double[] HighPassFilter(double[] signal, int windowSize)
    {
        double[] result = new double[signal.Length];
        for (int i = 0; i < signal.Length; i++)
        {
            int start = Math.Max(0, i - windowSize / 2);
            int end = Math.Min(signal.Length - 1, i + windowSize / 2);
            int count = end - start + 1;
            double sum = 0;
            for (int j = start; j <= end; j++)
                sum += signal[j];
            result[i] = signal[i] - sum / count;
        }
        return result;
    }

    private static double[] LowPassFilter(double[] signal, int windowSize)
    {
        double[] result = new double[signal.Length];
        for (int i = 0; i < signal.Length; i++)
        {
            int start = Math.Max(0, i - windowSize / 2);
            int end = Math.Min(signal.Length - 1, i + windowSize / 2);
            int count = end - start + 1;
            double sum = 0;
            for (int j = start; j <= end; j++)
                sum += signal[j];
            result[i] = sum / count;
        }
        return result;
    }

    private static double EstimateCadence(double[] signal, double sampleRate)
    {
        double mean = signal.Average();
        double[] normalized = signal.Select(v => v - mean).ToArray();

        double norm = normalized.Sum(v => v * v);
        if (norm == 0) return 0;

        int maxLag = (int)(sampleRate * 2.0);
        int minLag = Math.Max(1, (int)(sampleRate / 2.5));
        int upperBound = Math.Min(maxLag, normalized.Length - 1);

        double bestCorr = 0;
        int bestLag = 0;

        for (int lag = minLag; lag < upperBound; lag++)
        {
            double sum = 0;
            for (int i = 0; i < normalized.Length - lag; i++)
                sum += normalized[i] * normalized[i + lag];
            double corr = sum / norm;

            if (corr > bestCorr)
            {
                bestCorr = corr;
                bestLag = lag;
            }
        }

        if (bestCorr < AutocorrThreshold || bestLag == 0)
            return 0;

        double frequency = sampleRate / bestLag;
        if (frequency < MinPedalingFreq || frequency > MaxPedalingFreq)
            return 0;

        return frequency * 60.0;
    }

    private static double CalculateStdDev(List<double> values)
    {
        if (values.Count == 0) return 0;
        double avg = values.Average();
        double sumSq = values.Sum(v => (v - avg) * (v - avg));
        return Math.Sqrt(sumSq / values.Count);
    }
}