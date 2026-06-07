# [STRYKER_API]

Use `dotnet-stryker` through the project mutation rail. The default test run stays unit-only unless the local quality router enables mutation. Zero-test discovery fails the mutation rail.

## [1][LOCAL_RAIL]

| [INDEX] | [FACT]                 | [VALUE]                                                                                   |
| :-----: | ---------------------- | ----------------------------------------------------------------------------------------- |
|   [1]   | Tool                   | `dotnet-stryker`                                                                          |
|   [2]   | Tool restore           | `.config/dotnet-tools.json` through `dotnet tool restore`                                 |
|   [3]   | Project under mutation | `<project-under-mutation>`                                                                |
|   [4]   | Test project           | `<test-project>`                                                                          |
|   [5]   | Runner                 | `mtp`                                                                                     |
|   [6]   | Output                 | `.artifacts/mutation/<slice>/<run-id>`                                                    |
|   [7]   | Lock                   | `.artifacts/locks/mutation.lock`; live advisory contention fails fast                     |
|   [8]   | Timeout                | whole-process guard in the local quality router; Stryker owns per-mutant execution timing |

## [2][MUTATION_MODES]

| [INDEX] | [MODE]    | [COMMAND]                          | [POLICY]                                      |
| :-----: | --------- | ---------------------------------- | --------------------------------------------- |
|   [1]   | `off`     | `<test-runner>`                    | Unit-only default.                            |
|   [2]   | `changed` | `<test-runner> --mutation changed` | Mutate changed eligible managed files.        |
|   [3]   | `full`    | `<test-runner> --mutation full`    | Full managed mutation with strict thresholds. |

`list` and `coverage` do not mutate. Focused `--target` runs are unit-only unless mutation remains on the configured managed pair.

## [3][PARALLELISM]

| [INDEX] | [RAIL]                       | [POLICY]                                                    |
| :-----: | ---------------------------- | ----------------------------------------------------------- |
|   [1]   | `quality static`             | Concurrent-safe through run-scoped artifacts.               |
|   [2]   | local test runner            | MTP unit execution through run-scoped artifacts.            |
|   [3]   | `--mutation changed \| full` | One mutation process; fail fast when advisory lock is held. |
|   [4]   | runtime scenario rail        | Serial when the host exposes one live endpoint.             |

Unlocked lock files are stale and reusable. The quality tool rewrites them before launching Stryker.

## [4][METRICS]

- Mutation score is meaningful only after non-zero test discovery; zero Stryker discovery fails the rail.
- Target `95%` on the eligible managed slice after runner proof.
- Enforced thresholds are `high 95 / low 90 / break 85`.
- Classify every survivor as missing oracle, equivalent mutant, runtime-owned path, or product bug.
- Do not mutate host-runtime projects, plugin apps, runtime bridge tools, or runtime scenario scripts.
- Treat slow runs with selected mutants, timeouts, and non-zero discovery as real mutation results, not hangs.

## [5][CONFIG]

Keep Python operator-owned settings while mutation targets one project. Add `stryker-config.*` only when config-only options such as coverage analysis, executable excludes, or multi-project orchestration become necessary.

Stryker configuration detail belongs to the maintained Stryker.NET documentation; this page owns the project mutation rail posture.

## [6][THEORY_AS_STRYKER_ENABLER]

Stryker mutates each test method body and asks "does any test fail?". A `Spec.ForAll(Gen.OneOfConst([A,B,C]), ...)` body is ONE method, ONE mutation target. A `[Theory][InlineData(A)][InlineData(B)][InlineData(C)]` becomes THREE separately-tracked entry points.

Convert when:
- Stryker survivor analysis points at a SmartEnum or Union case the PBT host under-samples.
- The CI mutation budget cannot afford to re-run the full PBT body per mutant.

Do not convert when the cases share oracle logic and the PBT body is the more honest representation.

## [7][SURVIVOR_TAXONOMY]

| [INDEX] | [CLASS]            | [ACTION]                                                            |
| :-----: | ------------------ | ------------------------------------------------------------------- |
|   [1]   | Missing oracle     | Add a Grade A/B oracle that distinguishes the mutant.               |
|   [2]   | Equivalent mutant  | Document; do not weaken oracle.                                     |
|   [3]   | Runtime-owned path | Add or strengthen the runtime scenario; static spec cannot kill it. |
|   [4]   | Product bug        | Fix the production code; mutation revealed a real defect.           |

Do not chase a survivor by adding an assertion on the mutant's behavior.
