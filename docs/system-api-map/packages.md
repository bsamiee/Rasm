# [H1][PACKAGES]
>**Dictum:** *Package state follows concrete consumers and central truth.*

<br>

[IMPORTANT] Central package management lives in `Directory.Packages.props`. Project files declare usage without versions.

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

---
## [2][CURRENT]
>**Dictum:** *Current graph truth beats approved intent.*

<br>

| [INDEX] | [PACKAGE] | [STATE] | [OWNER] |
| :-----: | --------- | ------- | ------- |
| [1] | `LanguageExt.Core` | Active shared | Rails, effects, immutable traversal. |
| [2] | `Thinktecture.Runtime.Extensions` | Active shared | Generated value objects, smart enums, unions. |
| [3] | `MathNet.Numerics` | Active direct in `Rasm` | Numerical algorithms. |
| [4] | `MathNet.Symbolics` | Active direct in `Rasm` | Symbolic formulas. |
| [5] | MathNet/F# support closure | Transitive/supporting pins | Symbolics load-context proof, not direct C# API guidance. |
| [6] | `System.Drawing.Common` | Conditioned compile surface | Rhino UI/raster boundary metadata. |
| [7] | `Microsoft.NET.Test.Sdk` | Active shared for runnable tests | VSTest project execution. |
| [8] | `xunit.v3.mtp-off` | Active shared for runnable tests | xUnit v3 framework without MTP. |
| [9] | `xunit.v3.assert` | Active shared for testkit | Assertions and serializer extensibility. |
| [10] | `xunit.runner.visualstudio` | Active shared for runnable tests | VSTest adapter. |
| [11] | `CsCheck` | Active shared for tests/testkit | Property-based generation and shrinking. |
| [12] | `coverlet.msbuild` | Active shared for runnable tests | Opt-in managed coverage. |
| [13] | `dotnet-stryker` | Local tool manifest | Opt-in managed mutation testing. |
| [14] | `Verify.XunitV3` | Active direct in tooling snapshot tests | Snapshot-worthy generated/tooling artifacts only. |
| [15] | `TngTech.ArchUnitNET.xUnitV3` | Active direct in architecture tests | Assembly dependency boundary laws. |
| [16] | `BenchmarkDotNet` | Active direct in benchmark project | Managed hot-path measurement outside xUnit. |
| [17] | `SharpFuzz` | Active direct in fuzz project | Pure managed parser/token fuzz harnesses. |
| [18] | `SharpFuzz.CommandLine` | Local tool manifest | Opt-in fuzz instrumentation. |

---
## [3][ADOPTION]
>**Dictum:** *New packages need consumer, owner, and validation proof.*

<br>

- Add `PackageVersion` only with a concrete consumer or required transitive pin.
- Keep project `PackageReference` entries versionless.
- Use restore/build proof after graph changes.
- Mark candidate packages as out of graph until consumed.
- Reject packages that duplicate LanguageExt rails, Thinktecture shape, MathNet algorithms, or Rhino/GH semantics.

---
## [4][TEST_TOOL_SCOPE]
>**Dictum:** *Test tools belong to the test rail, not product dependency policy.*

<br>

- Raw xUnit, CsCheck, coverlet, and Stryker API guidance lives under `docs/testing-libs`.
- Test project package injection lives in `Directory.Build.props`; versions live in `Directory.Packages.props`.
- Local mutation tooling lives in `.config/dotnet-tools.json` and is invoked by `scripts/mutate-cs.sh`.
- Verify, ArchUnitNET, BenchmarkDotNet, and SharpFuzz each keep one direct `tests/csharp/_*` rail: `_tooling`, `_architecture`, `_benchmarks`, and `_fuzz`.
- Rejected adjacent testing packages stay out of the graph until a concrete test owner proves they add value beyond xUnit, CsCheck, coverlet, Stryker, bridge scenarios, and existing analyzers.
