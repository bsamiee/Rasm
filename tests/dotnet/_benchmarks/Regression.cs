using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.Benchmarks;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class GateStat {
    public static readonly GateStat Min = new(static stats => stats.Min);
    public static readonly GateStat Median = new(static stats => stats.Median);
    public static readonly GateStat Mean = new(static stats => stats.Mean);

    [UseDelegateFromConstructor]
    public partial double NanosecondsOf(BdnStatistics stats);
}

[Union]
public abstract partial record GateVerdict {
    public sealed record Pass(string Label, double ObservedMs, double BudgetMs) : GateVerdict;
    public sealed record TooNoisy(string Label, double RelIqr, double MaxRelIqr) : GateVerdict;
    public sealed record Breach(string Label, string Detail) : GateVerdict;
}

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class RegressionPolicy {
    internal const double PottsBeta = 4.0;
    internal const double RegressionTolerance = 0.70;
    internal const double NanosecondsPerMillisecond = 1_000_000.0;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BenchCase(string FullName, double BudgetMs, GateStat GateStat, double MaxRelIqr = 0.25);

// --- [BDN_REPORT]
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Regression {
    public static Seq<GateVerdict> Gate(BdnReport report, Seq<BenchCase> cases) {
        ArgumentNullException.ThrowIfNull(argument: report);
        return cases.Map(row =>
            report.Benchmarks.FirstOrDefault(benchmark => string.Equals(a: benchmark.FullName, b: row.FullName, comparisonType: StringComparison.Ordinal)) switch {
                null => new GateVerdict.Breach(Label: row.FullName, Detail: "no benchmark with this exact FullName in the report"),
                { Statistics: null } => new GateVerdict.Breach(Label: row.FullName, Detail: "benchmark carries no statistics"),
                { Statistics: { } statistics } => Verdict(row: row, statistics: statistics),
            });
    }

    public static Fin<Unit> Sustained(HashMap<string, Seq<double>> seriesByKey) {
        Seq<Error> regressions = seriesByKey.AsIterable().ToSeq().Bind(entry =>
            Segments(entry.Value) is var segments && segments.Count >= 2 && LevelJump(segments) is var ratio && ratio > RegressionPolicy.RegressionTolerance
                ? Seq(Error.New(string.Create(provider: CultureInfo.InvariantCulture, $"sustained benchmark regression: {entry.Key}: +{ratio:P1}")))
                : Seq<Error>());
        return regressions.IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: Error.Many(errors: regressions));
    }

    public static HashMap<string, Seq<double>> SeriesFromReports(params Seq<BdnReport> reports) =>
        reports.Bind(report => toSeq(report.Benchmarks))
            .Filter(static benchmark => benchmark.Statistics is not null)
            .Fold(HashMap<string, Seq<double>>(), static (series, benchmark) =>
                series.AddOrUpdate(
                    key: benchmark.FullName,
                    Some: existing => existing.Add(benchmark.Statistics!.Median),
                    None: () => Seq(benchmark.Statistics!.Median)));

    public static Fin<Unit> RegistryParity(Seq<string> discovered, Seq<BenchCase> cases) {
        static bool Owns(string methodKey, string rowName) =>
            string.Equals(a: rowName, b: methodKey, comparisonType: StringComparison.Ordinal)
            || (rowName.Length > methodKey.Length
                && rowName[index: methodKey.Length] == '('
                && rowName.StartsWith(value: methodKey, comparisonType: StringComparison.Ordinal));
        Seq<Error> gaps =
            discovered.Bind(method => cases.Exists(predicate: row => Owns(methodKey: method, rowName: row.FullName))
                ? Seq<Error>()
                : Seq(Error.New($"ungated benchmark: '{method}' has no BenchCase registry row")))
            + cases.Bind(row => discovered.Exists(predicate: method => Owns(methodKey: method, rowName: row.FullName))
                ? Seq<Error>()
                : Seq(Error.New($"phantom registry row: '{row.FullName}' names no discovered [Benchmark] method")));
        return gaps.IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: Error.Many(errors: gaps));
    }

    public static Fin<BdnReport> ReadReport(string path) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: path);
        return Try.lift(() => JsonSerializer.Deserialize(json: File.ReadAllText(path: path), jsonTypeInfo: BdnContext.Default.BdnReport)
                ?? throw new JsonException($"empty BDN report: {path}"))
            .Run()
            .MapFail(error => Error.New($"BDN report read failed ({path}): {error.Message}"));
    }

    // --- [GATE_VERDICT]
    private static GateVerdict Verdict(BenchCase row, BdnStatistics statistics) {
        double relIqr = statistics.Median > 0.0 ? statistics.InterquartileRange / statistics.Median : double.PositiveInfinity;
        double observedMs = row.GateStat.NanosecondsOf(stats: statistics) / RegressionPolicy.NanosecondsPerMillisecond;
        return (relIqr > row.MaxRelIqr, observedMs > row.BudgetMs) switch {
            (true, _) => new GateVerdict.TooNoisy(Label: row.FullName, RelIqr: relIqr, MaxRelIqr: row.MaxRelIqr),
            (_, true) => new GateVerdict.Breach(Label: row.FullName, Detail: string.Create(provider: CultureInfo.InvariantCulture,
                $"{observedMs:F4}ms exceeds budget {row.BudgetMs:F4}ms (relIqr={relIqr:F3})")),
            _ => new GateVerdict.Pass(Label: row.FullName, ObservedMs: observedMs, BudgetMs: row.BudgetMs),
        };
    }

    // --- [POTTS_SEGMENTATION]
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
