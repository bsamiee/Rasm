# [H1][STRYKER_API]
>**Dictum:** *Mutation scores the managed law suite after discovery works.*

<br>

[IMPORTANT] Rasm uses `dotnet-stryker 4.14.2` through `.config/dotnet-tools.json` and `scripts/mutate-cs.sh`. Stryker is currently diagnostic, not a completion gate, because the Stryker VSTest path detects zero tests while plain VSTest discovers managed xUnit v3 tests.

---
## [1][LOCAL_RAIL]
>**Dictum:** *Mutation has one narrow owner.*

<br>

| [INDEX] | [FACT] | [VALUE] |
| :-----: | ------ | ------- |
| [1] | Tool | `dotnet-stryker 4.14.2` |
| [2] | Project under mutation | `libs/csharp/Rasm/Rasm.csproj` |
| [3] | Test project | `tests/csharp/libs/Rasm/Rasm.Tests.csproj` |
| [4] | Default runner | `vstest` |
| [5] | Output | `.artifacts/mutation/<slice>/<run-id>` |

---
## [2][RUNNER_DECISION]
>**Dictum:** *MTP is a migration, not a retry flag.*

<br>

| [INDEX] | [RUNNER] | [STATUS] | [ACTION] |
| :-----: | -------- | -------- | -------- |
| [1] | `vstest` | Current Rasm rail; `dotnet test` works. | Fix/diagnose Stryker zero-test discovery here first. |
| [2] | `mtp` | Stryker preview; xUnit v3 supports it. | Requires replacing `xunit.v3.mtp-off` and proving all test projects. |
| [3] | bridge | Runtime verification rail. | Never routed through Stryker. |

[SOURCE] Stryker config docs: https://stryker-mutator.io/docs/stryker-net/configuration/

---
## [3][METRICS]
>**Dictum:** *Survivors are classified before tests change.*

<br>

- Mutation score is meaningful only after non-zero test discovery.
- Target `95%` on the eligible managed slice after runner proof.
- Use staged thresholds: discovery with the lowest CLI-accepted thresholds, report-only survivor taxonomy, then `high 95 / low 90 / break 85`; move `break-at` toward `95` only after equivalent/runtime-owned mutants are classified.
- Stryker requires `threshold-low >= break-at`; use `1/1/1` for discovery diagnostics when avoiding threshold failure.
- Classify every survivor as missing oracle, equivalent mutant, bridge-owned path, or product bug.
- Do not mutate `libs/csharp/Rasm.Rhino`, `libs/csharp/Rasm.Grasshopper`, plugin apps, bridge tools, or `*.verify.csx`.

---
## [4][CONFIG]
>**Dictum:** *Add config only for config-only value.*

<br>

Keep script-owned settings while mutation targets one project. Add `stryker-config.*` only when config-only options such as `coverage-analysis`, executable excludes, or multi-project orchestration become necessary.
