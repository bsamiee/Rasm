# [H1][COVERLET_API]
>**Dictum:** *Coverage maps untested code; it does not prove behavior.*

<br>

[IMPORTANT] Rasm pins `coverlet.msbuild 10.0.1`. It is injected only for runnable test projects; `Rasm.TestKit`, benchmarks, fuzz harnesses, and bridge scenarios are not coverage targets.

---
## [1][PACKAGE]
>**Dictum:** *Coverage stays opt-in until ownership is clean.*

<br>

| [INDEX] | [PACKAGE]          | [PIN]    | [USE]                                                            |
| :-----: | ------------------ | -------- | ---------------------------------------------------------------- |
|   [1]   | `coverlet.msbuild` | `10.0.1` | `dotnet test /p:CollectCoverage=true` for managed test projects. |

[SOURCE] NuGet package page: https://www.nuget.org/packages/coverlet.msbuild/10.0.1

---
## [2][MSBUILD_SURFACE]
>**Dictum:** *Central properties beat flag spam.*

<br>

| [INDEX] | [PROPERTY]                             | [RASM_POSTURE]                                                            |
| :-----: | -------------------------------------- | ------------------------------------------------------------------------- |
|   [1]   | `CollectCoverage`                      | Command-time opt-in.                                                      |
|   [2]   | `CoverletOutput`                       | Centralized under `.artifacts/coverage/<project>/`.                       |
|   [3]   | `CoverletOutputFormat`                 | `json,cobertura` for local and CI-readable output.                        |
|   [4]   | `Include` / `Exclude`                  | Managed assemblies only; avoid bridge/runtime fiction.                    |
|   [5]   | `ExcludeByFile` / `ExcludeByAttribute` | Generated and non-product exclusions.                                     |
|   [6]   | `Threshold*`                           | Do not hard-enable until static-vs-bridge ownership is proven.            |
|   [7]   | `MergeWith`                            | Future aggregation only after per-project maps are clean.                 |
|   [8]   | `ExcludeAssembliesWithoutSources`      | `None`; local proof shows `MissingAny` filters `Rasm` to an empty report. |

---
## [3][RASM_SCOPE]
>**Dictum:** *Native gaps are bridge gaps, not coverage lies.*

<br>

- `Rasm.Tests` is the first managed coverage target.
- `Rasm.Grasshopper.Tests` currently runs but coverlet cannot instrument `Rasm.Grasshopper` without resolving `Grasshopper2`; do not use its coverage number as a threshold.
- Rhino/GH document, UI, viewport, command, and native validity behavior belongs to bridge scenarios.
- Classify each uncovered line as missing static law, bridge-owned runtime path, generated code, or unreachable defensive guard before adding tests.
- Do not write artificial tests only to raise percentages.

Local command:

```bash
uv run python -m tools.quality test coverage
```

---
## [4][SURVIVOR_CLASSIFICATION]
>**Dictum:** *Every uncovered line classifies before any test changes.*

<br>

| [INDEX] | [CLASS]               | [ACTION]                                                                                                       |
| :-----: | --------------------- | -------------------------------------------------------------------------------------------------------------- |
|   [1]   | Missing static law    | Add a Grade A/B/C oracle to the owning spec.                                                                   |
|   [2]   | Bridge-owned runtime  | Strengthen `*.verify.csx`; coverlet cannot reach line. `[ExcludeFromCodeCoverage]` only after scenario exists. |
|   [3]   | Generated source      | `[ExcludeFromCodeCoverage]` on generator or `ExcludeByFile=**/*.g.cs`.                                         |
|   [4]   | Defensive unreachable | One-line comment naming unreachability invariant; do not test the arm.                                         |
|   [5]   | Dead code             | Remove.                                                                                                        |

Anti-patterns:
- Writing an `Assert.True(true)` test only to "cover" the line.
- Bumping `Threshold*` to mask uncovered bridge paths.
- Mutating the test to invoke a private method via reflection — the contract surface should drive coverage.
