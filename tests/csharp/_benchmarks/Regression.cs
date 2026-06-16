using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.Benchmarks;

// --- [TYPES] -------------------------------------------------------------------------------
// The robust statistic the budget gates on is the policy, carried as a vocabulary row with its own
// nanosecond projector — never a string knob re-dispatched at the gate. Mean is admitted but
// tail-sensitive; Median is the default robust choice; Min is the contention-free floor.
[SmartEnum]
public sealed partial class GateStat {
    public static readonly GateStat Min = new(static stats => stats.Min);
    public static readonly GateStat Median = new(static stats => stats.Median);
    public static readonly GateStat Mean = new(static stats => stats.Mean);

    [UseDelegateFromConstructor]
    public partial double NanosecondsOf(BdnStatistics stats);
}

// --- [CONSTANTS] ---------------------------------------------------------------------------
internal static class RegressionPolicy {
    // Breakpoints require scale-free BIC gain beyond an ASV-style per-step penalty; the tolerance is
    // the fractional final-segment level jump above which a sustained regression fails the session.
    internal const double PottsBeta = 4.0;
    internal const double RegressionTolerance = 0.70;
    internal const double NanosecondsPerMillisecond = 1_000_000.0;
}

// --- [MODELS] ------------------------------------------------------------------------------
// One registry row: the absolute budget over the gated statistic plus the dispersion ceiling above
// which a sample is too noisy to gate, mirroring bench.py's BenchCase gate fields.
public sealed record BenchCase(string Label, double BudgetMs, GateStat GateStat, double MaxRelIqr = 0.25);

// --- [BDN_REPORT]
// Source-generated projection over BenchmarkDotNet's `*-report-full.json`; only the median-series and
// gate inputs are decoded, and BDN emits nanosecond PascalCase statistics through JsonExporter.Full.
public sealed record BdnStatistics {
    [JsonPropertyName("Min")] public double Min { get; init; }
    [JsonPropertyName("Mean")] public double Mean { get; init; }
    [JsonPropertyName("Median")] public double Median { get; init; }
    [JsonPropertyName("Q1")] public double Q1 { get; init; }
    [JsonPropertyName("Q3")] public double Q3 { get; init; }
    [JsonPropertyName("InterquartileRange")] public double InterquartileRange { get; init; }
}

public sealed record BdnBenchmark {
    [JsonPropertyName("FullName")] public string FullName { get; init; } = "";
    [JsonPropertyName("Statistics")] public BdnStatistics? Statistics { get; init; }
}

public sealed record BdnReport {
    [JsonPropertyName("Benchmarks")] public IReadOnlyList<BdnBenchmark> Benchmarks { get; init; } = [];
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(BdnReport))]
internal sealed partial class BdnContext : JsonSerializerContext;

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Regression {
    // Gate enforces both the dispersion ceiling and the absolute budget per case, partitioning each
    // benchmark into pass, too-noisy-to-gate, or budget-breach, and folding breaches into one
    // ManyErrors body so every failing case is reported in one verdict.
    public static Fin<Unit> Gate(BdnReport report, Seq<BenchCase> cases) {
        ArgumentNullException.ThrowIfNull(argument: report);
        Seq<BdnBenchmark> benchmarks = toSeq(report.Benchmarks);
        Seq<Error> breaches = cases.Bind(row =>
            benchmarks.Filter(benchmark => benchmark.FullName.Contains(value: row.Label, comparisonType: StringComparison.Ordinal))
                .Map(benchmark => benchmark.Statistics)
                .Filter(static statistics => statistics is not null)
                .Bind(statistics => Verdict(row: row, statistics: statistics!).Match(
                    Some: static error => Seq(error),
                    None: static () => Seq<Error>())));
        return breaches.IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: Error.Many(errors: breaches));
    }

    // Sustained segments each per-key median series with the greedy Potts/BIC step criterion and
    // fails on a final-segment level jump above tolerance; a series with no prior segment never fires.
    public static Fin<Unit> Sustained(HashMap<string, Seq<double>> seriesByKey) {
        Seq<Error> regressions = seriesByKey.AsIterable().ToSeq().Bind(entry =>
            Segments(entry.Value) is var segments && segments.Count >= 2 && LevelJump(segments) is var ratio && ratio > RegressionPolicy.RegressionTolerance
                ? Seq(Error.New(string.Create(provider: CultureInfo.InvariantCulture, $"sustained benchmark regression: {entry.Key}: +{ratio:P1}")))
                : Seq<Error>());
        return regressions.IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: Error.Many(errors: regressions));
    }

    // SeriesFromReports reconstructs per-key ordered median series (oldest first) across stored full
    // reports plus the current run, keyed on the benchmark FullName the gate and segmenter share.
    public static HashMap<string, Seq<double>> SeriesFromReports(params Seq<BdnReport> reports) =>
        reports.Bind(report => toSeq(report.Benchmarks))
            .Filter(static benchmark => benchmark.Statistics is not null)
            .Fold(HashMap<string, Seq<double>>(), static (series, benchmark) =>
                series.AddOrUpdate(
                    key: benchmark.FullName,
                    Some: existing => existing.Add(benchmark.Statistics!.Median),
                    None: () => Seq(benchmark.Statistics!.Median)));

    public static Fin<BdnReport> ReadReport(string path) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: path);
        return Try.lift(() => JsonSerializer.Deserialize(json: File.ReadAllText(path: path), jsonTypeInfo: BdnContext.Default.BdnReport)
                ?? throw new JsonException($"empty BDN report: {path}"))
            .Run()
            .MapFail(static error => Error.New($"BDN report read failed: {error.Message}"));
    }

    // --- [GATE_VERDICT]
    private static Option<Error> Verdict(BenchCase row, BdnStatistics statistics) {
        double relIqr = statistics.Median > 0.0 ? statistics.InterquartileRange / statistics.Median : double.PositiveInfinity;
        double observedMs = row.GateStat.NanosecondsOf(stats: statistics) / RegressionPolicy.NanosecondsPerMillisecond;
        return (relIqr > row.MaxRelIqr, observedMs > row.BudgetMs) switch {
            (true, _) => None,
            (_, true) => Some(Error.New(string.Create(provider: CultureInfo.InvariantCulture,
                $"{row.Label}: {observedMs:F4}ms exceeds budget {row.BudgetMs:F4}ms (relIqr={relIqr:F3})"))),
            _ => None,
        };
    }

    // --- [POTTS_SEGMENTATION]
    // Greedy Potts/BIC partition: the best within-segment split is taken only when its scale-free BIC
    // gain clears the per-step penalty, recursing into each side; below two points the series is one
    // segment, matching bench.py's _potts_segments.
    private static Seq<Seq<double>> Segments(Seq<double> series) =>
        series.Count >= 2 ? Split(series, RegressionPolicy.PottsBeta * Math.Log(d: Math.Max(val1: series.Count, val2: 2))) : (series.IsEmpty ? Seq<Seq<double>>() : Seq(series));

    private static Seq<Seq<double>> Split(Seq<double> segment, double penalty) {
        (double Gain, int Index) best = toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: segment.Count - 1)))
            .Map(i => (Gain: Gain(segment, i), Index: i))
            .Fold((Gain: 0.0, Index: 0), static (acc, candidate) => candidate.Gain > acc.Gain ? candidate : acc);
        return segment.Count >= 2 && best.Gain > penalty
            ? Split(segment.Take(best.Index).AsIterable().ToSeq(), penalty) + Split(segment.Skip(best.Index).AsIterable().ToSeq(), penalty)
            : Seq(segment);
    }

    private static double Gain(Seq<double> segment, int index) {
        double full = Sse(segment);
        double split = Sse(segment.Take(index).AsIterable().ToSeq()) + Sse(segment.Skip(index).AsIterable().ToSeq());
        return full > 0.0 && split > 0.0 ? segment.Count * Math.Log(d: full / split) : (full > 0.0 ? double.PositiveInfinity : 0.0);
    }

    private static double Sse(Seq<double> segment) {
        double mean = Mean(segment);
        return segment.Fold(0.0, (acc, value) => acc + ((value - mean) * (value - mean)));
    }

    private static double LevelJump(Seq<Seq<double>> segments) {
        double priorLevel = Mean(segments[index: segments.Count - 2]);
        double lastLevel = Mean(segments[index: segments.Count - 1]);
        return priorLevel > 0.0 ? (lastLevel - priorLevel) / priorLevel : 0.0;
    }

    private static double Mean(Seq<double> values) => values.Fold(0.0, static (acc, value) => acc + value) / values.Count;
}
