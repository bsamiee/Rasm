# [BENCHMARKDOTNET_API]

[IMPORTANT] Use `BenchmarkDotNet` only in the project benchmark rail. Benchmarks are executable measurement rails, not unit specs and not part of the default test run.

## [1][CONFIG]

| [INDEX] | [SURFACE]                                         | [PROJECT_USE]                                          |
| :-----: | ------------------------------------------------- | ------------------------------------------------------ |
|   [1]   | `ManualConfig`                                    | Central benchmark config per benchmark project.        |
|   [2]   | `Job.Default`                                     | Named release job for local repeatability.             |
|   [3]   | `MemoryDiagnoser`                                 | Allocation pressure for hot paths.                     |
|   [4]   | JSON exporters                                    | Machine-readable output under `.artifacts/benchmarks`. |
|   [5]   | `ExecutionValidator`, `JitOptimizationsValidator` | Fail invalid benchmark runs.                           |
|   [6]   | `BenchmarkSwitcher`                               | CLI selection without extra scripts.                   |

[SOURCE] BenchmarkDotNet config docs: https://benchmarkdotnet.org/articles/configs/configs.html

## [2][PROJECT_SCOPE]

Use BenchmarkDotNet for pure managed hot paths such as numeric kernels, generated dispatch, parsing, and allocation-sensitive transforms. Do not benchmark host-native documents, UI surfaces, viewport state, UI thread, or runtime scenarios here.

Run the local rail directly:

```bash
dotnet run --project <benchmark-project> --configuration Release -- --list flat
```
