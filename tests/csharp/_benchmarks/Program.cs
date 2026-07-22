using System.Globalization;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace Rasm.Benchmarks;

// --- [CONSTANTS] -----------------------------------------------------------------------
// The gate registry: one row per gated benchmark, keyed by exact BDN FullName. Benchmark classes
// register here as they land; an empty registry still gates visibly through the session receipt.
internal static class BenchRegistry {
    public static readonly Seq<BenchCase> Cases = Seq<BenchCase>();
}

// --- [SERVICES] ------------------------------------------------------------------------
internal static class Program {
    // `gate <report-full.json> [older-reports...]` reads BDN full-JSON reports (newest last),
    // gates the newest against the registry, and runs the sustained segmenter across the series.
    // Any other argv routes to the BenchmarkSwitcher.
    public static int Main(string[] args) =>
        args is ["gate", .. var reportPaths]
            ? Gate(reportPaths: reportPaths)
            : Run(args: args);

    private static int Run(string[] args) {
        _ = BenchmarkSwitcher.FromAssembly(assembly: typeof(Program).Assembly).Run(args: args, config: RasmBenchmarkConfig.Instance);
        return 0;
    }

    private static int Gate(string[] reportPaths) {
        if (reportPaths.Length == 0) {
            Console.Error.WriteLine(value: "gate: at least one BDN *-report-full.json path is required");
            return 1;
        }
        Seq<Fin<BdnReport>> loaded = toSeq(reportPaths).Map(Regression.ReadReport);
        Seq<Error> unreadable = loaded.Bind(static result => result.Match(Succ: static _ => Seq<Error>(), Fail: static error => Seq(error)));
        Seq<BdnReport> reports = loaded.Bind(static result => result.Match(Succ: static report => Seq(report), Fail: static _ => Seq<BdnReport>()));
        _ = unreadable.AsIterable().Iter(error => Console.Error.WriteLine(value: error.Message));
        if (reports.IsEmpty) {
            return 1;
        }
        Seq<GateVerdict> rows = Regression.Gate(report: reports[index: reports.Count - 1], cases: BenchRegistry.Cases);
        _ = rows.AsIterable().Iter(row => Console.WriteLine(value: Render(verdict: row)));
        Seq<Error> sustained = Regression.Sustained(seriesByKey: Regression.SeriesFromReports(reports: reports))
            .Match(Succ: static _ => Seq<Error>(), Fail: static error => error switch { ManyErrors many => toSeq(many.Errors), _ => Seq(error) });
        _ = sustained.AsIterable().Iter(error => Console.WriteLine(value: $"SUSTAINED {error.Message}"));
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture,
            $"gate: cases={rows.Count} pass={rows.Count(static row => row is GateVerdict.Pass)} tooNoisy={rows.Count(static row => row is GateVerdict.TooNoisy)} breach={rows.Count(static row => row is GateVerdict.Breach)} sustained={sustained.Count} unreadable={unreadable.Count}"));
        // TooNoisy is a distinct visible exit, never folded into pass: 1 = breach/regression, 2 = ungateable noise.
        bool breached = !unreadable.IsEmpty || !sustained.IsEmpty || rows.Exists(static row => row is GateVerdict.Breach);
        bool noisy = rows.Exists(static row => row is GateVerdict.TooNoisy);
        return breached ? 1 : noisy ? 2 : 0;
    }

    private static string Render(GateVerdict verdict) => verdict.Switch(
        pass: static row => string.Create(provider: CultureInfo.InvariantCulture, $"PASS     {row.Label}: {row.ObservedMs:F4}ms within {row.BudgetMs:F4}ms"),
        tooNoisy: static row => string.Create(provider: CultureInfo.InvariantCulture, $"TOONOISY {row.Label}: relIqr={row.RelIqr:F3} over ceiling {row.MaxRelIqr:F3} — sample cannot be gated"),
        breach: static row => $"BREACH   {row.Label}: {row.Detail}");
}

internal sealed class RasmBenchmarkConfig : ManualConfig {
    public static readonly RasmBenchmarkConfig Instance = new();

    private RasmBenchmarkConfig() {
        AssemblyMetadataAttribute? root = typeof(Program).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(static attr => string.Equals(a: attr.Key, b: "RasmWorkspaceRoot", comparisonType: StringComparison.Ordinal));
        ArtifactsPath = Path.Combine(path1: root?.Value ?? Directory.GetCurrentDirectory(), path2: ".artifacts", path3: "benchmarks", path4: "rasm");
        // Explicit adaptive-engine ceilings keep every session self-limiting regardless of BDN default drift.
        _ = AddJob(Job.Default.WithId(id: "net10-release").WithMaxWarmupCount(count: 50).WithMaxIterationCount(count: 100));
        _ = AddDiagnoser(MemoryDiagnoser.Default);
        _ = AddExporter(JsonExporter.Full);
        _ = AddValidator(ExecutionValidator.FailOnError);
        _ = AddValidator(JitOptimizationsValidator.FailOnError);
    }
}
