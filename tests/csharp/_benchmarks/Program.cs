using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using Rasm.Domain;

namespace Rasm.Benchmarks;

// --- [SERVICES] ---------------------------------------------------------------------------
internal static class Program {
    public static void Main(string[] args) =>
        _ = BenchmarkSwitcher.FromAssembly(assembly: typeof(Program).Assembly).Run(args: args, config: RasmBenchmarkConfig.Instance);
}

internal sealed class RasmBenchmarkConfig : ManualConfig {
    public static readonly RasmBenchmarkConfig Instance = new();

    private RasmBenchmarkConfig() {
        AssemblyMetadataAttribute? root = typeof(Program).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(static attr => string.Equals(a: attr.Key, b: "RasmWorkspaceRoot", comparisonType: StringComparison.Ordinal));
        ArtifactsPath = Path.Combine(path1: root?.Value ?? Directory.GetCurrentDirectory(), path2: ".artifacts", path3: "benchmarks", path4: "rasm");
        _ = AddJob(Job.Default.WithId(id: "net10-release"));
        _ = AddDiagnoser(MemoryDiagnoser.Default);
        _ = AddExporter(JsonExporter.Full);
        _ = AddValidator(ExecutionValidator.FailOnError);
        _ = AddValidator(JitOptimizationsValidator.FailOnError);
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class StatsBenchmarks {
    private double[] values = [];

    [Params(16, 256, 4096)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup() =>
        values = [.. Enumerable.Range(start: 1, count: Count).Select(static value => (double)value)];

    [Benchmark]
    public Stat Summary() =>
        Stat.Of(values: toSeq(values), key: Op.Of(name: "bench.stats")).Match(Succ: static summary => summary, Fail: static error => throw new InvalidOperationException(message: error.Message));
}
