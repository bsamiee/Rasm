# [H1][STRYKER_API]
>**Dictum:** *Mutation tests the tests; it does not replace product oracles.*

<br>

[IMPORTANT] Rasm uses `dotnet-stryker 4.14.2` as an opt-in local tool through `.config/dotnet-tools.json` and `scripts/mutate-cs.sh`. No root `stryker-config.*` file exists in this pass.

---
## [1][PACKAGE_AND_RUNNER]
>**Dictum:** *Keep mutation on the managed rail.*

<br>

| [INDEX] | [FACT] | [VALUE] |
| :-----: | ------ | ------- |
| [1] | Tool | `dotnet-stryker 4.14.2` |
| [2] | Install mode | Local tool manifest |
| [3] | Script | `scripts/mutate-cs.sh` |
| [4] | Project under mutation | `libs/csharp/Rasm/Rasm.csproj` |
| [5] | Test project | `tests/csharp/libs/Rasm/Rasm.Tests.csproj` |
| [6] | Runner | `vstest` |

[SOURCE] NuGet package page: https://www.nuget.org/packages/dotnet-stryker/4.14.2

---
## [2][PROJECT_OPTIONS]
>**Dictum:** *Stryker mutates one project; be precise.*

<br>

| [INDEX] | [OPTION] | [USE] |
| :-----: | -------- | ----- |
| [1] | `--solution` | Helps dependency resolution. |
| [2] | `--project` | Project file name, not a path, when the test project references more than one project. |
| [3] | `--test-project` | Repeatable option for relevant test projects. |
| [4] | `--configuration` | Rasm script uses `Release`. |
| [5] | `--target-framework` | Rasm script uses `net10.0`. |
| [6] | `--test-runner` | `vstest` by default; MTP is preview in Stryker docs. |
| [7] | `--mutate` | Include/exclude globs; only project source files are considered. |

[SOURCE] Stryker configuration docs: https://stryker-mutator.io/docs/stryker-net/configuration/

---
## [3][CONTROL_REPORTS_THRESHOLDS]
>**Dictum:** *Thresholds start as a signal, then become a gate.*

<br>

| [INDEX] | [OPTION] | [RASM_DEFAULT] |
| :-----: | -------- | --------------- |
| [1] | `--mutation-level` | `Standard`; raise to `Advanced`/`Complete` only after runtime is stable. |
| [2] | `--reporter` | `html`, `json`, `progress`. |
| [3] | `--output` | `.artifacts/mutation/rasm`. |
| [4] | `--threshold-high` | `80`. |
| [5] | `--threshold-low` | `70`. |
| [6] | `--break-at` | `60`. |

The docs define `coverage-analysis` as config-only with default `perTest`; the verified CLI help for 4.14.2 does not expose a `--coverage-analysis` flag, so the script does not pass an unsupported option.

Local validation on 2026-05-24 showed `dotnet-stryker 4.14.2` using its bundled VSTest runner detected zero tests for the current `net10.0` + xUnit v3 suite while `dotnet test` discovered and ran the same tests. Treat that as runner compatibility evidence, not as a reason to weaken specs.

---
## [4][RASM_USE]
>**Dictum:** *Survivors are questions, not automatic test edits.*

<br>

- First classify each survivor as missing oracle, equivalent mutant, bridge-owned native path, or product bug.
- Improve existing laws before adding one-off tests.
- Keep mutation opt-in until the static/bridge ownership split is mature enough for stable local runtime.
- Do not mutate Rhino/GH runtime scenarios through Stryker; validate those through `scripts/rhino.sh verify` and `bridge check-source`.
