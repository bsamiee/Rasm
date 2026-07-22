# [RASM_PERSISTENCE_API_BENCHMARKDOTNET]

`BenchmarkDotNet` mints statistically-rigorous micro-benchmark measurement for the branch benchmark projects: `BenchmarkRunner` forks a dedicated child process per `[Benchmark]` case and returns a `Summary` whose per-case `Statistics` and `GcStats` carry the measured distribution. `BenchmarkRunner` owns its own process, so a dedicated benchmark executable drives it and only the projected RESULT crosses into the durable `Query/cache` claim index — never `Rasm.Persistence.csproj`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `BenchmarkDotNet`
- package: `BenchmarkDotNet` (MIT)
- assembly: `BenchmarkDotNet`
- companion: `BenchmarkDotNet.Annotations` owns the `[Benchmark]` attribute vocabulary a harness references without the runner
- namespaces: `BenchmarkDotNet.Running`, `BenchmarkDotNet.Attributes`, `BenchmarkDotNet.Configs`, `BenchmarkDotNet.Jobs`, `BenchmarkDotNet.Reports`, `BenchmarkDotNet.Mathematics`, `BenchmarkDotNet.Exporters.Json`
- rail: benchmark

## [02]-[PUBLIC_TYPES]

[RUNNER_SCOPE]: process-global run entries and the per-run report graph

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `BenchmarkRunner`   | class         | `Run` over type / assembly / `BenchmarkRunInfo`, mints `Summary` |
|  [02]   | `BenchmarkSwitcher` | class         | `FromAssembly`/`FromTypes` then `Run(args)` for a CLI harness    |
|  [03]   | `Summary`           | class         | `Reports`/`BenchmarksCases`/`ResultsDirectoryPath` graph         |
|  [04]   | `BenchmarkReport`   | class         | `ResultStatistics`/`GcStats`/`AllMeasurements`/`Success`         |
|  [05]   | `Statistics`        | class         | `Median`/`Mean`/`StandardError`/`Percentiles` over result runs   |
|  [06]   | `Measurement`       | struct        | `Operations`/`Nanoseconds`/`IterationStage` per iteration        |
|  [07]   | `GcStats`           | struct        | per-op allocation via `GetBytesAllocatedPerOperation`            |
|  [08]   | `PercentileValues`  | class         | `P90`/`P95`/`P100` off `Statistics.Percentiles`                  |

[CONFIG_SCOPE]: run-shape configuration composed off `ManualConfig`

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :-------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `IConfig`       | interface     | the `Run(config)` parameter every entry accepts            |
|  [02]   | `ManualConfig`  | class         | `AddJob`/`AddExporter`/`AddDiagnoser`/`AddColumn` builder  |
|  [03]   | `DefaultConfig` | class         | implicit config when `Run` gets no `IConfig`               |
|  [04]   | `Job`           | class         | `Dry`/`ShortRun`/`MediumRun`/`LongRun`/`InProcess` presets |
|  [05]   | `JsonExporter`  | class         | `Full`/`FullCompressed`/`Brief` result-file exporter       |

## [03]-[ENTRYPOINTS]

[RUN_ENTRYPOINTS]: process-global run over a harness type, assembly, or pre-built info

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `BenchmarkRunner.Run<T>(IConfig?, string[]?)`                     | static   | generic single-type run        |
|  [02]   | `BenchmarkRunner.Run(Type, IConfig?, string[]?)`                  | static   | reflected single-type run      |
|  [03]   | `BenchmarkRunner.Run(Type[], IConfig?, string[]?) -> Summary[]`   | static   | multi-type run                 |
|  [04]   | `BenchmarkRunner.Run(Assembly, IConfig?, string[]?) -> Summary[]` | static   | whole-assembly run             |
|  [05]   | `BenchmarkRunner.Run(BenchmarkRunInfo / BenchmarkRunInfo[])`      | static   | pre-resolved run graph         |
|  [06]   | `BenchmarkSwitcher.FromAssembly(Assembly) / FromTypes(Type[])`    | factory  | CLI switcher construction      |
|  [07]   | `BenchmarkSwitcher.Run(string[]?, IConfig?)`                      | instance | argument-filtered switcher run |
|  [08]   | `BenchmarkSwitcher.RunAllJoined(IConfig?, string[]?)`             | instance | one joined `Summary` over all  |

[CONFIG_ENTRYPOINTS]: run-shape assembly off `ManualConfig`

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `ManualConfig.CreateEmpty() / CreateMinimumViable()` | static   | empty / minimum-viable base config  |
|  [02]   | `ManualConfig.AddJob(Job[])`                         | instance | attach a run-shape preset           |
|  [03]   | `ManualConfig.AddExporter(IExporter[])`              | instance | attach `JsonExporter.Full` artifact |
|  [04]   | `ManualConfig.AddDiagnoser(IDiagnoser[])`            | instance | attach memory / threading diagnoser |
|  [05]   | `ManualConfig.AddColumn(IColumn[])`                  | instance | attach a result column              |
|  [06]   | `ManualConfig.WithOptions(ConfigOptions)`            | instance | run-option flags                    |

[RESULT_ENTRYPOINTS]: measured-result read off the returned `Summary`

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Summary.Reports / this[BenchmarkCase]`                    | property | per-case `BenchmarkReport` lookup   |
|  [02]   | `Summary.ResultsDirectoryPath / LogFilePath`               | property | process-owned artifact locations    |
|  [03]   | `BenchmarkReport.ResultStatistics -> Statistics?`          | property | distribution over result runs       |
|  [04]   | `Statistics.Median / Mean / StandardError`                 | property | central-tendency + error data       |
|  [05]   | `Statistics.Percentiles.P95 / P90`                         | property | tail-latency percentile datum       |
|  [06]   | `BenchmarkReport.GcStats`                                  | property | per-operation allocation receipt    |
|  [07]   | `BenchmarkReport.AllMeasurements / Measurement.Operations` | property | raw per-iteration invocation counts |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Harness authoring is attribute-driven: `[Benchmark]` marks a measured method, `[Params]`/`[ParamsSource]`/`[Arguments]` seed inputs, `[GlobalSetup]`/`[IterationSetup]` and `[GlobalCleanup]`/`[IterationCleanup]` stage non-measured work, `[MemoryDiagnoser]` turns on the `GcStats` column, and `[SimpleJob]`/`[IterationCount]`/`[WarmupCount]` fix the run shape inline absent a `ManualConfig`.
- `BenchmarkRunner.Run<T>` forks a dedicated optimized child process per case and owns the results directory (`Summary.ResultsDirectoryPath`/`LogFilePath`), so a run belongs in its own executable and one owner drives it.
- `Summary.Reports` maps each `BenchmarkCase` to a `BenchmarkReport`; `ResultStatistics` is the `Statistics` distribution over result-stage runs, `GcStats` the allocation receipt, and `Statistics.Percentiles.P95` the tail datum, while `Measurement` (`Operations`, `Nanoseconds`) filters by `IterationStage` so only result-stage data feeds `ResultStatistics`.

[STACKING]:
- `Query/cache`(`.planning/Query/cache.md`): a run over a `BenchmarkFamily` subject (`Codec`/`StoreAppend`/`Merge`/`Columnar`/`VectorRoute`/`Multipart`) reads `ResultStatistics.Median`, `Percentiles.P95`, the `GcStats` allocated-per-operation datum, and `AllMeasurements` op counts, which an AppHost `BenchmarkReceipt` folds into a `BenchmarkRow` (`Median`, `P95`, `AllocatedBytes`, `Operations`, `Corpus`, `ArtifactKey`); only the RESULT row persists, the `Summary` and child-process artifacts never crossing the strata boundary.
- `JsonExporter.Full` attached via `ManualConfig.AddExporter` writes the per-run artifact the `ArtifactKey` addresses by content key, `FullCompressed` the default machine export.
- `api-tensors`(`libs/csharp/.api/api-tensors.md`): every `BenchmarkRow` records the running `HostFingerprint`/`DeterminismTag`, and `ModelResultIndex.Claim(rows, fingerprint)` filters to the exact fingerprint within `RecencyHorizon` before the latest-`At` survivor, so `Rasm.Compute` reads the same index into `LinearProvider.Select` and gets the vectorized-vs-scalar win this harness proves; rows retire under `RetentionClass.Cache`.

[LOCAL_ADMISSION]:
- `BenchmarkDotNet` binds only in a dedicated benchmark executable, never `Rasm.Persistence.csproj` — the process-global fork drags the runner into the boundary process it must own alone.
- A design page cites `Statistics`/`GcStats`/`Measurement` as the transient measurement source and the `Query/cache` `BenchmarkRow` as persisted truth, never a `Stopwatch` loop or a prose performance assertion.
- A new hot-path claim is a `BenchmarkFamily` row and a `[Benchmark]` method on the matching subject resolved through one `ModelResultIndex.Claim`, never a new exporter, profiler, or second index.

[RAIL_LAW]:
- Package: `BenchmarkDotNet`
- Owns: statistically-rigorous micro-benchmark measurement for the branch benchmark projects — attribute-driven authoring, per-case child-process execution, and the `Summary`/`Statistics`/`GcStats`/`Measurement` graph feeding the durable claim index.
- Accept: a `[Benchmark]` harness run via `BenchmarkRunner.Run<T>`/`BenchmarkSwitcher`, run-shape via inline `[SimpleJob]` or `ManualConfig` `AddJob`/`AddExporter`/`AddDiagnoser`, and `Summary.Reports[...].ResultStatistics`/`GcStats` as the sole `BenchmarkRow` source.
- Reject: a `PackageReference` in a non-benchmark project; a hand-rolled `Stopwatch`/`DateTime` timing loop; a second durable benchmark store beside `ModelResultIndex`; a persisted `Summary`/`Verdict` posing as durable truth; a prose performance claim unbacked by a fingerprint-gated `BenchmarkRow`.
