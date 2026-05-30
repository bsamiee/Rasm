# [H1][PACKAGES]
>**Dictum:** *Package state follows concrete consumers and central truth.*

<br>

[IMPORTANT] Central package management lives in `Directory.Packages.props`. Project files declare usage without versions. Build-time packages (compiler toolset, analyzers, local analyzer project) belong in the state table with owner build/analyzer, not product numerics or rails.

---
## [1][STATE]
>**Dictum:** *A package name without state creates false guidance.*

<br>

| [INDEX] | [STATE]                  | [MEANING]                                                                                          |
| :-----: | ------------------------ | -------------------------------------------------------------------------------------------------- |
|   [1]   | Active direct            | A project references the package now.                                                              |
|   [2]   | Active shared            | `Directory.Build.props` references the package for a project class.                                |
|   [3]   | Transitive pin           | Central version controls a dependency required by an active package.                               |
|   [4]   | First-consumer candidate | Approved only when a concrete consumer is added.                                                   |
|   [5]   | Rejected                 | Creates duplicate paradigm or unsupported host behavior.                                           |
|   [6]   | Shared framework         | API in-box on net10.0; no `PackageReference` required; adoption may still be gated by measurement. |
|   [7]   | Platform package         | Ships with .NET; explicit `PackageReference` on first consumer; central pin on adoption.           |
|   [8]   | Local tool manifest      | Version pinned in `.config/dotnet-tools.json`; not an MSBuild package.                             |

---
## [2][CURRENT]
>**Dictum:** *Current graph truth beats approved intent.*

<br>

| [INDEX] | [PACKAGE]                                   | [STATE]                  | [OWNER]                                                               |
| :-----: | ------------------------------------------- | ------------------------ | --------------------------------------------------------------------- |
|   [1]   | `LanguageExt.Core`                          | Active                   | Rails, effects, immutable traversal                                   |
|   [2]   | `Thinktecture.Runtime.Extensions`           | Active                   | Value objects, smart enums, unions                                    |
|   [3]   | `MathNet.Numerics`                          | Active                   | Numerical algorithms                                                  |
|   [4]   | `MathNet.Symbolics`                         | Active                   | Symbolic formulas                                                     |
|   [5]   | `CSparse`                                   | Active                   | Sparse SPD direct factorization; `../external-libs/mathnet/sparse.md` |
|   [6]   | `FParsec`                                   | Transitive               | Symbolics parser closure                                              |
|   [7]   | `FSharp.Core`                               | Transitive               | Symbolics graph; bridge staged refs                                   |
|   [8]   | `MathNet.Numerics.FSharp`                   | Transitive               | Symbolics closure; not C# surface                                     |
|   [9]   | `System.Drawing.Common`                     | Active (cond.)           | RhinoWIP host; `ExcludeAssets=runtime`                                |
|  [10]   | `System.Numerics.Tensors`                   | Platform (not in graph)  | Span/tensor SIMD; pin on adopt                                        |
|  [11]   | `Microsoft.Extensions.Logging.Abstractions` | First-consumer           | `[LoggerMessage]`/ILogger; not in graph                               |
|  [12]   | `Microsoft.NET.Test.Sdk`                    | Shared (runnable tests)  | VSTest execution                                                      |
|  [13]   | `xunit.v3.mtp-off`                          | Shared (runnable tests)  | xUnit v3 without MTP                                                  |
|  [14]   | `xunit.v3.assert`                           | Shared (testkit)         | Assertions, serializer extensibility                                  |
|  [15]   | `xunit.v3.common`                           | Shared (testkit)         | xUnit v3 common types                                                 |
|  [16]   | `xunit.v3.extensibility.core`               | Shared (testkit)         | xUnit v3 extensibility core                                           |
|  [17]   | `xunit.runner.visualstudio`                 | Shared (runnable tests)  | VSTest adapter                                                        |
|  [18]   | `CsCheck`                                   | Shared (tests+testkit)   | PBT generation, shrinking                                             |
|  [19]   | `coverlet.msbuild`                          | Shared (runnable tests)  | Opt-in managed coverage                                               |
|  [20]   | `dotnet-stryker`                            | Local tool 4.14.2        | Opt-in mutation (managed)                                             |
|  [21]   | `Verify.XunitV3`                            | Direct (`_tooling`)      | Generated/tooling snapshots only                                      |
|  [22]   | `TngTech.ArchUnitNET.xUnitV3`               | Direct (`_architecture`) | Assembly boundary laws                                                |
|  [23]   | `BenchmarkDotNet`                           | Direct (`_benchmarks`)   | Hot-path measurement (non-xUnit)                                      |
|  [24]   | `SharpFuzz`                                 | Direct (`_fuzz`)         | Pure managed parser/token fuzz harnesses                              |
|  [25]   | `SharpFuzz.CommandLine`                     | Local tool 2.2.0         | Fuzz CLI (`sharpfuzz.commandline`)                                    |

[STATE_KEY]
- Direct (`…`) — Active direct in named consumer; `_*` paths under `tests/csharp/`.
- Shared (…) — Active shared for runnable tests, testkit, or tests+testkit scope.
- (+gated bridge) — bridge client when workspace libs gated (§4).

---
## [3][BUILD_AND_ANALYZERS]
>**Dictum:** *Compile and analyzer packages are build contracts, not runtime dependencies.*

<br>

| [INDEX] | [PACKAGE]                                    | [STATE]                | [OWNER]                                                 |
| :-----: | -------------------------------------------- | ---------------------- | ------------------------------------------------------- |
|   [1]   | `Microsoft.Net.Compilers.Toolset`            | Active                 | C# 14/Roslyn 5.3; `../external-libs/csharp/language.md` |
|   [2]   | `Meziantou.Analyzer`                         | Active                 | Build-time style/architecture analyzer                  |
|   [3]   | `Microsoft.VisualStudio.Threading.Analyzers` | Active                 | Build-time threading-correctness analyzer               |
|   [4]   | `Microsoft.CodeAnalysis.CSharp`              | Active                 | Roslyn analyzer authoring                               |
|   [5]   | `Microsoft.CodeAnalysis.Analyzers`           | Active in `CsAnalyzer` | Release tracking for Roslyn rules                       |
|   [6]   | `CsAnalyzer` (local)                         | Shared (analyzer ref)  | CSP####; not a central `PackageVersion`                 |

---
## [4][GATED_INJECTION]
>**Dictum:** *Plugin and bridge projects do not inherit workspace library injection by default.*

<br>

Rhino plugin, Grasshopper plugin, and rhino-bridge projects default `UseWorkspaceLibraries=false`. They do not receive global LanguageExt/Thinktecture package injection or prelude globals from `Directory.Build.props`. Opt-in per `.csproj` when needed; never assume prelude globals in plugin boundaries. Host references, compiler toolset, third-party analyzers, and local CsAnalyzer still apply unless explicitly skipped.

---
## [5][ADOPTION]
>**Dictum:** *New packages need consumer, owner, and validation proof.*

<br>

- Add `PackageVersion` only with a concrete consumer or required transitive pin.
- Keep project `PackageReference` entries versionless.
- Use restore/build proof after graph changes.
- Mark candidate packages as out of graph until consumed.
- Reject packages that duplicate LanguageExt rails, Thinktecture shape, MathNet algorithms, or Rhino/GH semantics.
- Platform packages such as `System.Numerics.Tensors` need explicit `PackageReference`, central pin, and measured consumer before production guidance treats them as active.
- First-consumer candidates such as `Microsoft.Extensions.Logging.Abstractions` stay out of graph until a concrete owner needs `[LoggerMessage]` / `ILogger`.
- Host composition packages (Scrutor, FluentValidation, NodaTime, EF Core, Serilog, OpenTelemetry, Microsoft.Extensions.Http.Resilience) stay out of graph until a bootstrap or persistence host exists — adoption triggers in `../host-libraries.md` §8.

---
## [6][TEST_TOOL_SCOPE]
>**Dictum:** *Test tools belong to the test rail, not product dependency policy.*

<br>

- Raw xUnit, CsCheck, coverlet, and Stryker API guidance lives under `../testing-libs`.
- Test project package injection lives in `Directory.Build.props`; versions live in `Directory.Packages.props`.
- Local mutation tooling lives in `.config/dotnet-tools.json` and runs through `tools.quality test run` after VSTest on the default managed target.
- Verify, ArchUnitNET, BenchmarkDotNet, and SharpFuzz each keep one direct `tests/csharp/_*` rail: `_tooling`, `_architecture`, `_benchmarks`, and `_fuzz`.
- Rejected adjacent testing packages stay out of the graph until a concrete test owner proves they add value beyond xUnit, CsCheck, coverlet, Stryker, bridge scenarios, and existing analyzers.
