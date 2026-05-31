# [H1][STRYKER_API]
>**Dictum:** *Mutation scores the managed law suite only when explicitly requested.*

<br>

[IMPORTANT] Rasm uses `dotnet-stryker 4.14.2` through `.config/dotnet-tools.json` and `tools.quality test run --mutation changed|full`. Default `tools.quality test run` is unit-only. Stryker runs with the MTP runner against the `Rasm` project/test pair, and zero-test discovery fails the rail.

---
## [1][LOCAL_RAIL]
>**Dictum:** *Mutation has one narrow owner.*

<br>

| [INDEX] | [FACT]                 | [VALUE]                                                                                    |
| :-----: | ---------------------- | ------------------------------------------------------------------------------------------ |
|   [1]   | Tool                   | `dotnet-stryker 4.14.2`                                                                    |
|   [2]   | Tool restore           | `.config/dotnet-tools.json` through `dotnet tool restore`                                  |
|   [3]   | Project under mutation | `libs/csharp/Rasm/Rasm.csproj`                                                             |
|   [4]   | Test project           | `tests/csharp/libs/Rasm/Rasm.Tests.csproj`                                                 |
|   [5]   | Runner                 | `mtp`                                                                                      |
|   [6]   | Output                 | `.artifacts/mutation/<slice>/<run-id>`                                                     |
|   [7]   | Lock                   | `.artifacts/locks/mutation.lock`; lock contention fails fast instead of silent queueing     |

---
## [2][MUTATION_MODES]
>**Dictum:** *Expensive proof must be explicit.*

<br>

| [INDEX] | [MODE]    | [COMMAND]                                                | [POLICY]                                      |
| :-----: | --------- | -------------------------------------------------------- | --------------------------------------------- |
|   [1]   | `off`     | `uv run python -m tools.quality test run`                | Unit-only default.                            |
|   [2]   | `changed` | `uv run python -m tools.quality test run --mutation changed` | Mutate changed managed files under `libs/csharp/Rasm`. |
|   [3]   | `full`    | `uv run python -m tools.quality test run --mutation full` | Full managed mutation with strict thresholds. |

`list` and `coverage` do not mutate. Focused `--target` runs are unit-only unless mutation remains on the default managed Rasm pair.

---
## [3][PARALLELISM]
>**Dictum:** *Unit tests isolate; mutation serializes.*

<br>

| [INDEX] | [RAIL]                         | [POLICY]                                                                     |
| :-----: | ------------------------------ | ---------------------------------------------------------------------------- |
|   [1]   | `quality static`               | Concurrent-safe through run-scoped artifacts.                                 |
|   [2]   | `tools.quality test run`       | MTP unit execution through run-scoped artifacts.                              |
|   [3]   | `--mutation changed|full`      | One mutation process; fail fast when `.artifacts/locks/mutation.lock` is held. |
|   [4]   | `tools.quality bridge`         | Serial — one live Rhino endpoint.                                             |

---
## [4][METRICS]
>**Dictum:** *Survivors are classified before tests change.*

<br>

- Mutation score is meaningful only after non-zero test discovery; zero Stryker discovery fails the rail.
- Target `95%` on the eligible managed slice after runner proof.
- Enforced thresholds are `high 95 / low 90 / break 85`.
- Classify every survivor as missing oracle, equivalent mutant, bridge-owned path, or product bug.
- Do not mutate `libs/csharp/Rasm.Rhino`, `libs/csharp/Rasm.Grasshopper`, plugin apps, bridge tools, or `*.verify.csx`.

---
## [5][CONFIG]
>**Dictum:** *Add config only for config-only value.*

<br>

Keep Python operator-owned settings while mutation targets one project. Add `stryker-config.*` only when config-only options such as coverage analysis, executable excludes, or multi-project orchestration become necessary.

[SOURCE] Stryker config docs: https://stryker-mutator.io/docs/stryker-net/configuration/

---
## [6][THEORY_AS_STRYKER_ENABLER]
>**Dictum:** *PBT hosts are one mutation target; Theory rows are N.*

<br>

Stryker mutates each test method body and asks "does any test fail?". A `Spec.ForAll(Gen.OneOfConst([A,B,C]), ...)` body is ONE method, ONE mutation target. A `[Theory][InlineData(A)][InlineData(B)][InlineData(C)]` becomes THREE separately-tracked entry points.

Convert when:
- Stryker survivor analysis points at a SmartEnum or Union case the PBT host under-samples.
- The CI mutation budget cannot afford to re-run the full PBT body per mutant.

Do not convert when the cases share oracle logic and the PBT body is the more honest representation.

---
## [7][SURVIVOR_TAXONOMY]
>**Dictum:** *Every survivor classifies before any test changes.*

<br>

| [INDEX] | [CLASS]           | [ACTION]                                                                                        |
| :-----: | ----------------- | ----------------------------------------------------------------------------------------------- |
|   [1]   | Missing oracle    | Add a Grade A/B oracle that distinguishes the mutant.                                            |
|   [2]   | Equivalent mutant | Document; do not weaken oracle.                                                                 |
|   [3]   | Bridge-owned path | Add or strengthen `*.verify.csx` scenario; static spec cannot kill it.                          |
|   [4]   | Product bug       | Fix the production code; mutation revealed a real defect.                                       |

Do not chase a survivor by adding an assertion on the mutant's behavior.
