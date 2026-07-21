# [RASM_PERSISTENCE_API_BENCHMARKDOTNET]

`BenchmarkDotNet` is a benchmark-substrate-only catalog: the branch benchmark projects consume the harness, and this file records the measurement surface whose RESULT rows project into the durable `Query/cache#BENCHMARK_INDEX` claim index. `BenchmarkRunner` is the process-global static entry a `[Benchmark]`-decorated harness type invokes: a run forks a dedicated child process per benchmark case, writes artifacts to a process-owned results directory, and returns a `Summary` whose `Reports` carry per-case `Statistics` and `GcStats`. Harness binding stays out of the `Rasm.Persistence` package csproj — the runner owns its own process, so a dedicated benchmark project drives it, and only the measured RESULT (`Median`, `P95`, `AllocatedBytes`, `Operations`) crosses into the AppHost `BenchmarkReceipt` custody projection folding into a fingerprint-gated, recency-bounded `BenchmarkRow` the `ModelResultIndex.Claim` resolution admits.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `BenchmarkDotNet`
- package: `BenchmarkDotNet` (version `0.15.8`, MIT)
- assembly: `BenchmarkDotNet` (the package ships `net6.0`/`net8.0`/`netstandard2.0` assets; the `net8.0` asset binds the workspace floor)
- companion: `BenchmarkDotNet.Annotations` (the attribute vocabulary `assembly` a harness references without the full runner)
- namespaces: `BenchmarkDotNet.Running`, `BenchmarkDotNet.Attributes`, `BenchmarkDotNet.Configs`, `BenchmarkDotNet.Jobs`, `BenchmarkDotNet.Reports`, `BenchmarkDotNet.Mathematics`, `BenchmarkDotNet.Exporters.Json`
- scope: process-global — the runner spawns a per-case child process and owns the results directory; a single dedicated benchmark project is the only admissible owner
- rail: benchmark

## [02]-[PUBLIC_TYPES]

[RUNNER_SCOPE]: process-global run entries and the per-run report graph
- rail: benchmark

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]       | [RAIL]                                                           |
| :-----: | :------------------ | :------------------ | :--------------------------------------------------------------- |
|  [01]   | `BenchmarkRunner`   | static run entry    | `Run` over type / assembly / `BenchmarkRunInfo`, mints `Summary` |
|  [02]   | `BenchmarkSwitcher` | multi-type switcher | `FromAssembly`/`FromTypes` then `Run(args)` for a CLI harness    |
|  [03]   | `Summary`           | per-run report      | `Reports`/`BenchmarksCases`/`ResultsDirectoryPath` graph         |
|  [04]   | `BenchmarkReport`   | per-case report     | `ResultStatistics`/`GcStats`/`AllMeasurements`/`Success`         |
|  [05]   | `Statistics`        | distribution owner  | `Median`/`Mean`/`StandardError`/`Percentiles` over result runs   |
|  [06]   | `Measurement`       | one iteration datum | `Operations`/`Nanoseconds`/`IterationStage` per invocation       |
|  [07]   | `GcStats`           | allocation receipt  | the per-operation allocation datum on `BenchmarkReport.GcStats`  |
|  [08]   | `PercentileValues`  | percentile owner    | `P90`/`P95`/`P100` off `Statistics.Percentiles`                  |

[CONFIG_SCOPE]: run-shape configuration composed off `ManualConfig`
- rail: benchmark

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [RAIL]                                                     |
| :-----: | :-------------- | :--------------- | :--------------------------------------------------------- |
|  [01]   | `IConfig`       | config contract  | the `Run(config)` parameter every entry accepts            |
|  [02]   | `ManualConfig`  | fluent config    | `AddJob`/`AddExporter`/`AddDiagnoser`/`AddColumn` builder  |
|  [03]   | `DefaultConfig` | baseline config  | the implicit config when `Run` gets no `IConfig`           |
|  [04]   | `Job`           | run-shape preset | `Dry`/`ShortRun`/`MediumRun`/`LongRun`/`InProcess` presets |
|  [05]   | `JsonExporter`  | machine artifact | `Full`/`FullCompressed`/`Brief` result-file exporter       |

## [03]-[ENTRYPOINTS]

[RUN_ENTRYPOINTS]: process-global run over a harness type, assembly, or pre-built info
- rail: benchmark

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT]      | [RAIL]                         |
| :-----: | :------------------------------------------------- | :------------------ | :----------------------------- |
|  [01]   | `Run<T>(IConfig? = null, string[]? args = null)`   | `BenchmarkRunner`   | generic single-type run        |
|  [02]   | `Run(Type, IConfig? = null, string[]? = null)`     | `BenchmarkRunner`   | reflected single-type run      |
|  [03]   | `Run(Type[], IConfig? = null, string[]? = null)`   | `BenchmarkRunner`   | multi-type run, `Summary[]`    |
|  [04]   | `Run(Assembly, IConfig? = null, string[]? = null)` | `BenchmarkRunner`   | whole-assembly run             |
|  [05]   | `Run(BenchmarkRunInfo / BenchmarkRunInfo[])`       | `BenchmarkRunner`   | pre-resolved run graph         |
|  [06]   | `FromAssembly(Assembly)` / `FromTypes(Type[])`     | `BenchmarkSwitcher` | CLI switcher construction      |
|  [07]   | `Run(string[]? args = null, IConfig? = null)`      | `BenchmarkSwitcher` | argument-filtered switcher run |
|  [08]   | `RunAllJoined(IConfig? = null, string[]? = null)`  | `BenchmarkSwitcher` | one joined `Summary` over all  |

[CONFIG_ENTRYPOINTS]: run-shape assembly off `ManualConfig`
- rail: benchmark

| [INDEX] | [SURFACE]                                 | [SURFACE_ROOT] | [CAPABILITY]                        |
| :-----: | :---------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `CreateEmpty()` / `CreateMinimumViable()` | `ManualConfig` | empty / minimum-viable base config  |
|  [02]   | `AddJob(params Job[])`                    | `ManualConfig` | attach a run-shape preset           |
|  [03]   | `AddExporter(params IExporter[])`         | `ManualConfig` | attach `JsonExporter.Full` artifact |
|  [04]   | `AddDiagnoser(params IDiagnoser[])`       | `ManualConfig` | attach memory / threading diagnoser |
|  [05]   | `AddColumn(params IColumn[])`             | `ManualConfig` | attach a result column              |
|  [06]   | `WithOptions(ConfigOptions)`              | `ManualConfig` | run-option flags                    |

[RESULT_ENTRYPOINTS]: measured-result read off the returned `Summary`
- rail: benchmark

| [INDEX] | [SURFACE]                              | [SURFACE_ROOT]    | [CAPABILITY]                        |
| :-----: | :------------------------------------- | :---------------- | :---------------------------------- |
|  [01]   | `Reports` / `this[BenchmarkCase]`      | `Summary`         | per-case `BenchmarkReport` lookup   |
|  [02]   | `ResultsDirectoryPath` / `LogFilePath` | `Summary`         | process-owned artifact locations    |
|  [03]   | `ResultStatistics`                     | `BenchmarkReport` | `Statistics?` over the result runs  |
|  [04]   | `Median` / `Mean` / `StandardError`    | `Statistics`      | central-tendency + error data       |
|  [05]   | `Percentiles.P95` / `Percentiles.P90`  | `Statistics`      | tail-latency percentile datum       |
|  [06]   | `GcStats`                              | `BenchmarkReport` | per-operation allocation receipt    |
|  [07]   | `AllMeasurements` / `Operations`       | `BenchmarkReport` | raw per-iteration invocation counts |

## [04]-[IMPLEMENTATION_LAW]

[HARNESS_TOPOLOGY]:
- Harness authoring is attribute-driven: a benchmark class marks each measured method `[Benchmark]`, seeds inputs via `[Params]`/`[ParamsSource]`/`[Arguments]`, and stages non-measured setup via `[GlobalSetup]`/`[IterationSetup]` and teardown via `[GlobalCleanup]`/`[IterationCleanup]`; `[MemoryDiagnoser]` on the class turns on the `GcStats` allocation column, and `[SimpleJob]`/`[IterationCount]`/`[WarmupCount]` fix the run shape inline when a `ManualConfig` is not passed.
- `BenchmarkRunner.Run<T>` forks a dedicated optimized child process per benchmark case — the harness is process-global by construction, never an in-process library the app composes; the results directory and log file are process-owned (`Summary.ResultsDirectoryPath`/`Summary.LogFilePath`), so a benchmark run belongs in its own executable project and a single owner drives it.
- `Summary` is the per-run report graph: `Reports` maps each `BenchmarkCase` to a `BenchmarkReport`, `BenchmarkReport.ResultStatistics` is the `Statistics` distribution over the measured (result-stage) runs, and `BenchmarkReport.GcStats` is the allocation receipt; `Statistics.Percentiles` (`PercentileValues.P95`) supplies the tail datum the durable claim row carries as `P95`.
- `Measurement` is one iteration datum — `Operations` is the per-invocation op count and `Nanoseconds` the elapsed time — filtered by `IterationMode`/`IterationStage` so only result-stage measurements feed `ResultStatistics`; the raw stream is `BenchmarkReport.AllMeasurements`.

[STACKING]:
- Measured `Summary` output is the source of the `Query/cache#BENCHMARK_INDEX` durable claim, never a second store: a benchmark run over a `BenchmarkFamily` subject (`Codec`/`StoreAppend`/`Merge`/`Columnar`/`VectorRoute`/`Multipart`) reads `ResultStatistics.Median`, `ResultStatistics.Percentiles.P95`, the `GcStats` allocated-per-operation datum, and `AllMeasurements` op counts. AppHost `BenchmarkReceipt` custody folds exactly those into a `BenchmarkRow` under the `benchmarks` claim-field map (`Median`, `P95`, `AllocatedBytes`, `Operations`, `Corpus`, `ArtifactKey`); only the RESULT row persists, so the `Summary`, its child-process artifacts, and the per-run `Verdict`/`Correlation` never cross the strata boundary.
- `JsonExporter.Full` (attached via `ManualConfig.AddExporter`) writes the machine-readable per-run artifact the `ArtifactKey` column addresses, so the durable row references the full measurement file by content key rather than re-embedding the raw distribution; `JsonExporter.FullCompressed` is the default machine export.
- Every `BenchmarkRow` is fingerprint-gated: the harness records the running host identity (the upstream `HostFingerprint.ToString`/`DeterminismTag` string), and `ModelResultIndex.Claim(rows, fingerprint)` filters to the exact fingerprint and the `RecencyHorizon` bound before folding to the latest-`At` survivor — a measurement claimed under one host or provider never wins a route on a host whose fingerprint drifted. Claim rows retire under `RetentionClass.Cache` because a benchmark row is re-derivable by re-running the equivalence sweep.
- Upstream `Rasm.Compute` numeric and SIMD lanes read this same index by reference (`Tensor/blas#PROVIDER_CLAIMS` resolves the winning provider against the running fingerprint and `ModelResultIndex.RecencyHorizon`, then hands it to `LinearProvider.Select`); the benchmark harness proves the vectorized-vs-scalar win the `libs/csharp/.api/api-tensors.md` receipt rule demands, and this catalog is the harness half of that gate.

[LOCAL_ADMISSION]:
- `BenchmarkDotNet` binds only in a dedicated benchmark executable project, never `Rasm.Persistence.csproj` — the README `[TEST_SUBSTRATE]` row states this, and the process-global fork model enforces it: a library reference drags the runner into the boundary process it must own alone.
- Measured `Summary` output is a transient artifact; the durable surface is the projected `BenchmarkRow`, so a design page cites `Statistics`/`GcStats`/`Measurement` as the measurement source and the `Query/cache` claim row as the persisted truth, never a hand-rolled `Stopwatch` loop or a prose performance assertion.
- A new hot-path claim is a `BenchmarkFamily` row and a `[Benchmark]` method on the matching subject, not a new exporter, profiler add-on, or second index; the gate is one `ModelResultIndex.Claim` resolution.

[RAIL_LAW]:
- Package: `BenchmarkDotNet`
- Owns: statistically-rigorous micro-benchmark measurement for the branch benchmark projects — attribute-driven harness authoring, per-case child-process execution, and the `Summary`/`Statistics`/`GcStats`/`Measurement` result graph feeding the durable claim index.
- Accept: a `[Benchmark]`-decorated harness type run via `BenchmarkRunner.Run<T>`/`BenchmarkSwitcher`, run-shape via inline `[SimpleJob]` attributes or a `ManualConfig` with `AddJob`/`AddExporter`/`AddDiagnoser`, and `Summary.Reports[...].ResultStatistics`/`GcStats` read as the sole source of a `Query/cache` `BenchmarkRow`.
- Reject: a `PackageReference` in a non-benchmark project; a hand-rolled `Stopwatch`/`DateTime` timing loop when the harness owns statistical rigor and warmup; a second durable benchmark store beside `ModelResultIndex`; a persisted `Summary` or per-run `Verdict` masquerading as durable truth; a prose performance claim unbacked by a fingerprint-gated, recency-bounded `BenchmarkRow`.
