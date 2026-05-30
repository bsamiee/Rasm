# [H1][PACKAGES]
>**Dictum:** *Package state follows concrete consumers and central truth.*

<br>

[IMPORTANT] Central package management lives in `Directory.Packages.props`. Project files declare usage without versions. Build-time packages (compiler toolset, analyzers, local analyzer project) belong in the state table with owner build/analyzer, not product numerics or rails.

---
## [1][STATE]
>**Dictum:** *A package name without state creates false guidance.*

<br>

| [INDEX] | [STATE] | [MEANING] |
| :-----: | ------- | --------- |
| [1] | Active direct | A project references the package now. |
| [2] | Active shared | `Directory.Build.props` references the package for a project class. |
| [3] | Transitive pin | Central version controls a dependency required by an active package. |
| [4] | First-consumer candidate | Approved only when a concrete consumer is added. |
| [5] | Rejected | Creates duplicate paradigm or unsupported host behavior. |
| [6] | Shared framework | API in-box on net10.0; no `PackageReference` required; adoption may still be gated by measurement. |
| [7] | Platform package | Ships with the .NET platform; requires explicit `PackageReference` on first consumer; central pin on adoption. |
| [8] | Local tool manifest | Version pinned in `.config/dotnet-tools.json`; not an MSBuild package. |

---
## [2][CURRENT]
>**Dictum:** *Current graph truth beats approved intent.*

<br>

| [INDEX] | [PACKAGE] | [STATE] | [OWNER] |
| :-----: | --------- | ------- | ------- |
| [1] | `LanguageExt.Core` | Active shared; Active direct in `tools/rhino-bridge/client` when gated | Rails, effects, immutable traversal. |
| [2] | `Thinktecture.Runtime.Extensions` | Active shared; Active direct in `tools/rhino-bridge/client` when gated | Generated value objects, smart enums, unions. |
| [3] | `MathNet.Numerics` | Active direct in `Rasm` | Numerical algorithms. |
| [4] | `MathNet.Symbolics` | Active direct in `Rasm` | Symbolic formulas. |
| [5] | `CSparse` | Active direct in `Rasm` | Sparse SPD direct factorization; API detail in `../external-libs/mathnet/sparse.md`. |
| [6] | `FParsec` | Transitive pin | Symbolics parser closure. |
| [7] | `FSharp.Core` | Transitive pin | `MathNet.Symbolics` graph closure; bridge may load via staged refs. |
| [8] | `MathNet.Numerics.FSharp` | Transitive pin | Symbolics closure; not a C# API surface. |
| [9] | `System.Drawing.Common` | Active shared (conditioned compile) + Active direct in `Rasm.TestKit` | RhinoWIP host runtime; NuGet compile metadata `ExcludeAssets=runtime`. |
| [10] | `System.Numerics.Tensors` | Platform package; not in graph | Span/tensor SIMD; pin on first measured production consumer; see `bcl.md` §4. |
| [11] | `Microsoft.Extensions.Logging.Abstractions` | First-consumer candidate | `[LoggerMessage]` / `ILogger`; not in graph today — see `bcl.md` §5. |
| [12] | `Microsoft.NET.Test.Sdk` | Active shared for runnable tests | VSTest project execution. |
| [13] | `xunit.v3.mtp-off` | Active shared for runnable tests | xUnit v3 framework without MTP. |
| [14] | `xunit.v3.assert` | Active shared for testkit | Assertions and serializer extensibility. |
| [15] | `xunit.v3.common` | Active shared for testkit | xUnit v3 common types. |
| [16] | `xunit.v3.extensibility.core` | Active shared for testkit | xUnit v3 extensibility core. |
| [17] | `xunit.runner.visualstudio` | Active shared for runnable tests | VSTest adapter. |
| [18] | `CsCheck` | Active shared for tests/testkit | Property-based generation and shrinking. |
| [19] | `coverlet.msbuild` | Active shared for runnable tests | Opt-in managed coverage. |
| [20] | `dotnet-stryker` | Local tool manifest 4.14.2 | Opt-in managed mutation testing. |
| [21] | `Verify.XunitV3` | Active direct in `tests/csharp/_tooling` | Snapshot-worthy generated/tooling artifacts only. |
| [22] | `TngTech.ArchUnitNET.xUnitV3` | Active direct in `_architecture` | Assembly dependency boundary laws. |
| [23] | `BenchmarkDotNet` | Active direct in `_benchmarks` | Managed hot-path measurement outside xUnit. |
| [24] | `SharpFuzz` | Active direct in `_fuzz` | Pure managed parser/token fuzz harnesses. |
| [25] | `SharpFuzz.CommandLine` | Local tool manifest 2.2.0 (`sharpfuzz.commandline`) | Opt-in fuzz instrumentation CLI. |

---
## [3][BUILD_AND_ANALYZERS]
>**Dictum:** *Compile and analyzer packages are build contracts, not runtime dependencies.*

<br>

| [INDEX] | [PACKAGE] | [STATE] | [OWNER] |
| :-----: | --------- | ------- | ------- |
| [1] | `Microsoft.Net.Compilers.Toolset` | Active shared | C# 14 / Roslyn 5.3 compile pin — detail in `../external-libs/csharp/language.md`. |
| [2] | `Meziantou.Analyzer` | Active shared | Build-time style/architecture analyzer. |
| [3] | `Microsoft.VisualStudio.Threading.Analyzers` | Active shared | Build-time threading-correctness analyzer. |
| [4] | `Microsoft.CodeAnalysis.CSharp` | Active direct in analyzer projects + `CsAnalyzer.Tests` | Roslyn analyzer authoring. |
| [5] | `Microsoft.CodeAnalysis.Analyzers` | Active direct in `CsAnalyzer` | Release tracking for Roslyn rules. |
| [6] | Local `CsAnalyzer` (`Foundation.CSharp.Analyzers`) | Active shared (project analyzer ref) | CSP#### rule catalog; not a central `PackageVersion` entry. |

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

---
## [6][TEST_TOOL_SCOPE]
>**Dictum:** *Test tools belong to the test rail, not product dependency policy.*

<br>

- Raw xUnit, CsCheck, coverlet, and Stryker API guidance lives under `../testing-libs`.
- Test project package injection lives in `Directory.Build.props`; versions live in `Directory.Packages.props`.
- Local mutation tooling lives in `.config/dotnet-tools.json` and runs through `tools.quality test run` after VSTest on the default managed target.
- Verify, ArchUnitNET, BenchmarkDotNet, and SharpFuzz each keep one direct `tests/csharp/_*` rail: `_tooling`, `_architecture`, `_benchmarks`, and `_fuzz`.
- Rejected adjacent testing packages stay out of the graph until a concrete test owner proves they add value beyond xUnit, CsCheck, coverlet, Stryker, bridge scenarios, and existing analyzers.
