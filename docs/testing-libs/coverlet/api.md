# [H1][COVERLET_API]
>**Dictum:** *Coverage is a map, not an oracle.*

<br>

[IMPORTANT] Rasm pins `coverlet.msbuild 10.0.1`. It is injected only for runnable test projects by `Directory.Build.props`; `Rasm.TestKit` remains a library.

---
## [1][PACKAGE]
>**Dictum:** *The package version is central truth.*

<br>

| [INDEX] | [PACKAGE] | [PIN] | [USE] |
| :-----: | --------- | ----- | ----- |
| [1] | `coverlet.msbuild` | `10.0.1` | Opt-in managed coverage from `dotnet test /p:CollectCoverage=true`. |

[SOURCE] NuGet package page: https://www.nuget.org/packages/coverlet.msbuild/10.0.1

---
## [2][MSBUILD_SURFACE]
>**Dictum:** *Prefer central properties over command spam.*

<br>

| [INDEX] | [PROPERTY] | [RASM_POSTURE] |
| :-----: | ---------- | --------------- |
| [1] | `CollectCoverage` | Passed at command time for coverage runs. |
| [2] | `CoverletOutput` | Centralized under test artifact output from `Directory.Build.props`. |
| [3] | `CoverletOutputFormat` | Keep machine-readable formats configured centrally. |
| [4] | `Exclude`, `ExcludeByFile`, `ExcludeByAttribute` | Keep broad exclusions in central props only when source ownership proves they are non-product or generated. |
| [5] | `Include` | Use only for focused coverage audits. |
| [6] | `Threshold`, `ThresholdType`, `ThresholdStat` | Do not hard-enable global thresholds until static-vs-bridge ownership is clean. |
| [7] | `MergeWith` | Future multi-project aggregation; avoid until report aggregation is required. |

---
## [3][RASM_USE]
>**Dictum:** *Native-owned gaps are bridge gaps, not coverage lies.*

<br>

- Static xUnit coverage applies to pure-managed specs.
- Rhino/GH native behavior is covered by `*.verify.csx` scenarios and bridge checks, not coverlet.
- Coverage holes should be classified before action: missing static law, bridge-owned runtime path, generated code, or intentionally unreachable defensive guard.
- Do not add file-local suppression or artificial tests to satisfy a coverage number.
