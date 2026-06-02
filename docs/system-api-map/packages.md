# [H1][PACKAGES]
>**Dictum:** *Current graph truth beats approved intent.*

<br>

[IMPORTANT] Central package management lives in `Directory.Packages.props`. Project files declare usage without versions. Local .NET CLI tools live in `.config/dotnet-tools.json` because `dotnet tool restore` owns executable tool restore.

---
## [1][STATE]
>**Dictum:** *A package name without state creates false guidance.*

<br>

| [INDEX] | [STATE]                  | [MEANING]                                                                                |
| :-----: | ------------------------ | ---------------------------------------------------------------------------------------- |
|   [1]   | Active direct            | A project references the package now.                                                    |
|   [2]   | Active shared            | `Directory.Build.props` references the package for a project class.                      |
|   [3]   | Transitive pin           | Central version controls a dependency required by an active package.                     |
|   [4]   | First-consumer candidate | Approved owner-local intent; add central version only with a concrete consumer.          |
|   [5]   | Rejected                 | Creates duplicate paradigm, unsupported host behavior, or unwanted platform direction.   |
|   [6]   | Shared framework         | API in-box on net10.0; no `PackageReference` required.                                   |
|   [7]   | Platform package         | API ships as a platform package; explicit `PackageReference` only on first consumer.     |
|   [8]   | Local tool manifest      | Version lives in `.config/dotnet-tools.json`; not an MSBuild package.                    |

---
## [2][CURRENT]
>**Dictum:** *Only package references and central versions define current graph state.*

<br>

| [INDEX] | [PACKAGE]                                   | [STATE]                  | [OWNER]                                                |
| :-----: | ------------------------------------------- | ------------------------ | ------------------------------------------------------ |
|   [1]   | `LanguageExt.Core`                          | Active shared/direct     | Rails, effects, immutable traversal                    |
|   [2]   | `Thinktecture.Runtime.Extensions`           | Active shared/direct     | Value objects, smart enums, unions                     |
|   [3]   | `MathNet.Numerics`                          | Active direct            | Numerical algorithms                                   |
|   [4]   | `MathNet.Symbolics`                         | Active direct            | Symbolic formulas                                      |
|   [5]   | `CSparse`                                   | Active direct            | Sparse SPD direct factorization                        |
|   [6]   | `FParsec`                                   | Transitive pin           | Symbolics parser closure                               |
|   [7]   | `FSharp.Core`                               | Transitive pin           | Symbolics graph; bridge staged refs                    |
|   [8]   | `MathNet.Numerics.FSharp`                   | Transitive pin           | Symbolics closure; not C# surface                      |
|   [9]   | `System.Drawing.Common`                     | Active conditional       | RhinoWIP host compile surface; `ExcludeAssets=runtime` |
|  [10]   | `xunit.v3.mtp-v2`                           | Active shared            | xUnit v3 MTP runnable test projects                    |
|  [11]   | `xunit.v3.assert`                           | Active shared (testkit)  | Assertions, serializer extensibility                   |
|  [12]   | `xunit.v3.common`                           | Active shared (testkit)  | xUnit v3 common types                                  |
|  [13]   | `xunit.v3.extensibility.core`               | Active shared (testkit)  | xUnit v3 extensibility core                            |
|  [14]   | `Microsoft.Testing.Platform*`               | Transitive pin           | MTP runner/runtime closure                             |
|  [15]   | `CsCheck`                                   | Active shared            | PBT generation, shrinking                              |
|  [16]   | `coverlet.MTP`                              | Active shared            | Opt-in managed coverage                                |
|  [17]   | `dotnet-stryker`                            | Local tool manifest      | Explicit managed mutation                              |
|  [18]   | `Verify.XunitV3`                            | Direct (`_tooling`)      | Generated/tooling snapshots only                       |
|  [19]   | `TngTech.ArchUnitNET.xUnitV3`               | Direct (`_architecture`) | Assembly boundary laws                                 |
|  [20]   | `BenchmarkDotNet`                           | Direct (`_benchmarks`)   | Hot-path measurement                                   |
|  [21]   | `SharpFuzz`                                 | Direct (`_fuzz`)         | Pure managed parser/token fuzz harnesses               |

---
## [3][FIRST_CONSUMER_CANDIDATES]
>**Dictum:** *Approved candidates are not graph truth.*

<br>

Owner-local platform manuals may name future package candidates, but `Directory.Packages.props` must stay unchanged until a concrete project or host bootstrap consumes the package.

| [INDEX] | [OWNER] | [CANDIDATE BAND] |
| :-----: | ------- | ---------------- |
|   [1]   | `Rasm.AppUi` | Avalonia, ReactiveUI, DynamicData, SkiaSharp, ImGui.NET debug overlays, reselected ReactiveUI/Avalonia adapter. |
|   [2]   | `Rasm.AppHost` | Generic Host/DI/Scrutor, NodaTime, FluentValidation, Serilog/OpenTelemetry, HTTP resilience, optional Dataflow. |
|   [3]   | `Rasm.Persistence` | Microsoft.Data.Sqlite, EF Core SQLite, MessagePack compact snapshots. |
|   [4]   | `Rasm.Compute` | System.Numerics.Tensors, ML.NET named-model lane, gRPC client for companion compute. |

[CRITICAL] Candidate does not mean pinned, restored, loaded, or runtime-proven. Refresh latest stable versions immediately before first consumer adoption.

---
## [4][BUILD_AND_ANALYZERS]
>**Dictum:** *Compile and analyzer packages are build contracts, not runtime dependencies.*

<br>

| [INDEX] | [PACKAGE]                                    | [STATE]                | [OWNER]                                 |
| :-----: | -------------------------------------------- | ---------------------- | --------------------------------------- |
|   [1]   | .NET 10 SDK compiler                         | Shared SDK             | C# 14 compiler; no compiler-toolset pkg |
|   [2]   | `Meziantou.Analyzer`                         | Active shared          | Build-time style/architecture analyzer  |
|   [3]   | `Microsoft.VisualStudio.Threading.Analyzers` | Active shared          | Build-time threading analyzer           |
|   [4]   | `Microsoft.CodeAnalysis.CSharp`              | Active direct          | Roslyn analyzer authoring               |
|   [5]   | `Microsoft.CodeAnalysis.Analyzers`           | Active in `CsAnalyzer` | Release tracking for Roslyn rules       |
|   [6]   | `CsAnalyzer` (local)                         | Shared analyzer ref    | CSP####; not a central `PackageVersion` |

---
## [5][GATED_INJECTION]
>**Dictum:** *Plugin projects do not inherit workspace library injection by default.*

<br>

Rhino and Grasshopper plugin projects default `UseWorkspaceLibraries=false`. Tooling and bridge client projects inherit workspace library injection unless they explicitly opt out. Host references, third-party analyzers, and local CsAnalyzer still apply unless explicitly skipped.

---
## [6][TEST_TOOL_SCOPE]
>**Dictum:** *Test tools belong to the test rail, not product dependency policy.*

<br>

- Raw xUnit, CsCheck, coverlet, and Stryker API guidance lives under `../testing-libs`.
- Test project package injection lives in `Directory.Build.props`; versions live in `Directory.Packages.props`.
- Root `global.json` owns .NET 10 MTP runner selection; it does not pin the SDK.
- Local mutation tooling lives in `.config/dotnet-tools.json` and runs through `tools.quality test run --mutation changed|full`.
- Verify, ArchUnitNET, BenchmarkDotNet, and SharpFuzz each keep one direct `tests/csharp/_*` rail: `_tooling`, `_architecture`, `_benchmarks`, and `_fuzz`.
