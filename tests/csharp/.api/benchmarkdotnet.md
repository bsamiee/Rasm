# [benchmarkdotnet] — the measured console session behind the regression gate

`BenchmarkDotNet` runs the estate's benchmark session as a plain optimized console app — never an MTP/xunit runner. `BenchmarkSwitcher` drives argv selection under the central `RasmBenchmarkConfig`; `JsonExporter.Full` writes the `*-report-full.json` the `gate` verb decodes into `BdnStatistics` rows for median/dispersion regression ceilings; the `_architecture` suite reads the benchmark assembly's types to enforce registry parity without ever running a session.

## [01]-[PACKAGE_SURFACE]

- package: `BenchmarkDotNet` `0.15.8` (+ `BenchmarkDotNet.Annotations`)
- license: `MIT`
- namespace: `BenchmarkDotNet.Running`, `BenchmarkDotNet.Attributes`, `BenchmarkDotNet.Configs`, `BenchmarkDotNet.Jobs`, `BenchmarkDotNet.Reports`, `BenchmarkDotNet.Mathematics`
- asset: `lib/net8.0/BenchmarkDotNet.dll` (net10 consumers bind via compat) + `netstandard2.0/BenchmarkDotNet.Annotations.dll`
- rail: evidence — wall-clock and allocation measurement with statistical reports the regression gate consumes

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                | [KIND]      | [CAPABILITY]                                                                   |
| :-----: | :-------------------------------------- | :---------- | :----------------------------------------------------------------------------- |
|  [01]   | `BenchmarkRunner` / `BenchmarkSwitcher` | entry       | typed runs and argv-driven selection returning `Summary`                       |
|  [02]   | `[Benchmark]`                           | attribute   | benchmark case declaration                                                     |
|  [03]   | `Params` / `Arguments`                  | attribute   | parameter and per-argument case axes                                           |
|  [04]   | `GlobalSetup` / `IterationSetup`        | attribute   | once-per-run and per-iteration lifecycle hooks                                 |
|  [05]   | `ManualConfig : IConfig`                | config      | `Add{Job,Column,Exporter,Diagnoser,Validator}`, `WithOptions`; `ArtifactsPath` |
|  [06]   | `Job` + `JobExtensions.With*`           | job algebra | run/env/accuracy characteristics; presets `Default`/`ShortRun`/`InProcess`/…   |
|  [07]   | `Summary` / `BenchmarkReport`           | report      | per-case `Success`, `ResultStatistics`, measurements, GC stats                 |
|  [08]   | `Statistics`                            | model       | `Min/Q1/Median/Mean/Q3/Max/InterquartileRange/StandardError/…` nanoseconds     |
|  [09]   | `MemoryDiagnoser`                       | policy      | allocation-metric columns                                                      |
|  [10]   | `ExecutionValidator`                    | policy      | refuse failed executions                                                       |
|  [11]   | `JitOptimizationsValidator`             | policy      | refuse Debug/unoptimized assemblies                                            |
|  [12]   | `JsonExporter` / `MarkdownExporter`     | exporter    | `JsonExporter.Full` emits the `-full` report the gate decodes                  |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                       | [KIND]  | [CAPABILITY]                                               |
| :-----: | :-------------------------------------------------------------- | :------ | :--------------------------------------------------------- |
|  [01]   | `BenchmarkSwitcher.FromAssembly(assembly).Run(args, config)`    | session | argv-selected cases under one config                       |
|  [02]   | `Job.Default.With{Id,MaxWarmupCount,MaxIterationCount}(…)`      | job     | ceiling-bounded measurement; out-of-process by default     |
|  [03]   | `config.ArtifactsPath = <workspace>/.artifacts/benchmarks/rasm` | routing | artifact routing; reports under `<ArtifactsPath>/results/` |
|  [04]   | `JsonExporter.Full` / `MemoryDiagnoser.Default`                 | policy  | full-statistics JSON + allocation columns                  |
|  [05]   | `ExecutionValidator.FailOnError`                                | policy  | refuse failed executions                                   |
|  [06]   | `JitOptimizationsValidator.FailOnError`                         | policy  | refuse Debug/unoptimized assemblies                        |
|  [07]   | `summary[case].ResultStatistics`                                | report  | in-memory statistics per case; null when no runs           |

```csharp signature
public sealed class BenchmarkReport {
    public bool Success { get; }
    public Statistics? ResultStatistics { get; }
    public GcStats GcStats { get; }
}
public class Statistics {
    public double Min { get; }
    public double Q1 { get; }
    public double Median { get; }
    public double Mean { get; }
    public double Q3 { get; }
    public double InterquartileRange { get; }
}
```

## [04]-[IMPLEMENTATION_LAW]

[SESSION_SHAPE]: `IsBenchmarkProject` classifies the session out of `IsTestProject`, so no MTP runner, no xunit json, no test packages reach it; the csproj carries `OutputType=Exe` + `Optimize=true`, and the validator pair turns an unoptimized or failing case into a session error. The workspace root resolves from the `RasmWorkspaceRoot` assembly metadata the csproj stamps, keeping artifacts under `.artifacts/benchmarks/rasm` instead of the BDN default folder.

[GATE]: the regression gate is a JSON consumer, never a BDN runtime consumer — `gate` argv decodes `Benchmarks[].Statistics` rows (`Min/Mean/Median/Q1/Q3/InterquartileRange`, nanoseconds, PascalCase) into `BdnStatistics`, projects `GateStat` medians, and applies `relIqr = InterquartileRange / Median` dispersion ceilings against the registry rows.

[REGISTRY_PARITY]: `BenchRegistry.Cases` is the gate's case registry; the `_architecture` suite reflects `[Benchmark]` methods from the benchmark assembly (via `InternalsVisibleTo`) and fails on either an ungated benchmark or a phantom registry row — types are read, sessions never run inside unit suites.

[STACKING]:
- `xunit.v3` / `Microsoft.Testing.Platform` (`xunit-v3.md`, `testing-platform.md`): deliberate non-interaction; the session is a console app.
- `Rasm.TestKit` `Manifests`: workspace-root resolution mirrors the kit's slnx walk; the assembly-metadata route exists because the session runs from artifact output, not the repo tree.

[LOCAL_ADMISSION]:
- Every `[Benchmark]` pairs with a `BenchCase` registry row; the parity law makes a silent unbenchmarked-case impossible.
- Job ceilings (`WithMaxWarmupCount`, `WithMaxIterationCount`) are config rows, never per-benchmark attributes, so run cost stays centrally bounded.

[RAIL_LAW]:
- Package: `BenchmarkDotNet`
- Owns: measured execution, statistical reporting, and the JSON evidence the regression gate reads.
- Accept: switcher-driven sessions under the one config; `[Params]` axes for shape scaling; `MemoryDiagnoser` allocation columns.
- Reject: benchmarks inside test projects, ad hoc `Stopwatch` micro-timing, per-benchmark config attributes that fork the central policy.
