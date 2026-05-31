# [H1][BENCHMARKDOTNET_API]
>**Dictum:** *Benchmarks measure hot paths outside the unit rail.*

<br>

[IMPORTANT] Rasm uses `BenchmarkDotNet 0.15.8` only in `tests/csharp/_benchmarks`. Benchmarks are executable measurement rails, not xUnit specs and not part of `tools.quality test run`.

---
## [1][PACKAGE]
>**Dictum:** *Benchmark projects own benchmark dependencies.*

<br>

| [INDEX] | [PACKAGE]         | [PIN]    | [USE]                                 |
| :-----: | ----------------- | -------- | ------------------------------------- |
|   [1]   | `BenchmarkDotNet` | `0.15.8` | Reproducible performance measurement. |

[SOURCE] NuGet package page: https://www.nuget.org/packages/BenchmarkDotNet/0.15.8

---
## [2][CONFIG]
>**Dictum:** *Measurement policy lives in C# config.*

<br>

| [INDEX] | [SURFACE]                                         | [RASM_USE]                                             |
| :-----: | ------------------------------------------------- | ------------------------------------------------------ |
|   [1]   | `ManualConfig`                                    | Central benchmark config per benchmark project.        |
|   [2]   | `Job.Default`                                     | Named release job for local repeatability.             |
|   [3]   | `MemoryDiagnoser`                                 | Allocation pressure for hot paths.                     |
|   [4]   | JSON exporters                                    | Machine-readable output under `.artifacts/benchmarks`. |
|   [5]   | `ExecutionValidator`, `JitOptimizationsValidator` | Fail invalid benchmark runs.                           |
|   [6]   | `BenchmarkSwitcher`                               | CLI selection without extra scripts.                   |

[SOURCE] BenchmarkDotNet config docs: https://benchmarkdotnet.org/articles/configs/configs.html

---
## [3][RASM_SCOPE]
>**Dictum:** *Do not benchmark Rhino app-bundle runtime outside RhinoWIP.*

<br>

Use BenchmarkDotNet for pure managed hot paths such as numeric kernels, generated dispatch, parsing, and allocation-sensitive transforms. Do not benchmark RhinoDoc, GH canvas, viewport, UI thread, or bridge scenarios here.

Run the local rail directly:

```bash
dotnet run --project tests/csharp/_benchmarks/Rasm.Benchmarks.csproj --configuration Release -- --list flat
```
