# [@stryker-mutator/vitest-runner] — runs each surviving mutant through the vitest spec runner and reports the kill verdict

[PACKAGE_SURFACE]:
- package: `@stryker-mutator/vitest-runner` · version `9.6.1` · license `Apache-2.0`
- module: ESM (`type: module`); one `.` export → `dist/src/index.js`; peer contract `@stryker-mutator/api` (`9.6.1`) + a `vitest` peer it augments.
- asset: `dist/src/index.d.ts`; augments `declare module 'vitest'` (adds `ProvidedContext` + `TaskMeta` fields) — the in-worker instrumentation channel.
- runtime: node-only; boots `vitest`'s programmatic API in-process, reuses one vitest worker across mutants (no per-mutant process spawn).
- plane: `plane:dev` — a Stryker TestRunner plugin loaded by `@stryker-mutator/core`; a config row, never a value import; the `tests/typescript/_architecture` purity audit holds trivially.
- rail: mutation kill-execution / test-run verdict.

`@stryker-mutator/vitest-runner` is the kill engine of the mutation/coverage gauge and the spine of the checker→runner pipeline. For every mutant that cleared the `typescript-checker` compile gate, Stryker activates the mutant (an env-flagged branch inside the instrumented source) and calls `mutantRun`; the runner executes the folder's `@effect/vitest` specs against that mutant and returns `Killed` (a spec failed — the mutant was caught), `Survived` (all specs passed — a test gap), `Timeout`, or `Error`. It reuses ONE vitest instance across the whole mutant sweep and narrows execution to only the specs that cover each mutant via `perTest` coverage — the difference between a mutation run that finishes and one that never does. This catalog owns the TestRunner kill-execution half of the checker→runner pipeline; the shared plugin-loading ABI (`PluginKind`/`FactoryPlugin`/`commonTokens`) and the canonical config-as-data schema (`StrykerOptions`/`PartialStrykerOptions`) both admitted plugins ride are owned by `stryker-mutator-core.md` [04]/[02], and `stryker-mutator-typescript-checker.md` owns the compile-gate half.

## [01]-[PLUGIN_ENTRY]

[PUBLIC_TYPE_SCOPE]: the two public exports (`strykerPlugins`, `strykerValidationSchema`) typed against the `@stryker-mutator/api/plugin` loading ABI that `stryker-mutator-core.md` [04] owns. `strykerPlugins` is the value `@stryker-mutator/core` discovers; a `FactoryPlugin` is a DI factory tagged by `PluginKind` and injected with `["$injector"]` (`commonTokens.injector`).

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]        | [CAPABILITY / BOUNDARY]                                                  |
| :-----: | :-------------------------------- | :------------------- | :----------------------------------------------------------------------- |
|  [01]   | `strykerPlugins`                  | `FactoryPlugin[]`    | the only value export the host reads — `FactoryPlugin<PluginKind.TestRunner, ["$injector"]>[]` |
|  [02]   | `strykerValidationSchema`         | JSON schema          | `typeof vitest-runner-options.json` — validates the `vitest` option bag   |
|  [03]   | `PluginKind` / `FactoryPlugin<K, Tokens>` | shared ABI   | the plugin-loading types this row is typed with — owned by `stryker-mutator-core.md` [04] |
|  [04]   | `VitestTestRunner`                | internal `TestRunner` impl | the class the `FactoryPlugin` factory yields (not a public export); implements the [02] contract |

```ts contract
// index.d.ts @9.6.1 — the ENTIRE public barrel is two exports; PluginKind/FactoryPlugin import from @stryker-mutator/api/plugin (core [04]).
import { PluginKind, FactoryPlugin } from '@stryker-mutator/api/plugin'
export declare const strykerPlugins: FactoryPlugin<PluginKind.TestRunner, ["$injector"]>[]
export declare const strykerValidationSchema: typeof import('../schema/vitest-runner-options.json')
```

## [02]-[TEST_RUNNER_CONTRACT]

[PUBLIC_TYPE_SCOPE]: the `@stryker-mutator/api/test-runner` contract — one dry run establishes per-mutant coverage, then one `mutantRun` per mutant returns a four-arm verdict union. Both result shapes are discriminated unions on `status`, never boolean pairs.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]        | [CAPABILITY / BOUNDARY]                                                  |
| :-----: | :-------------------------------- | :------------------- | :----------------------------------------------------------------------- |
|  [01]   | `TestRunner`                      | interface            | `capabilities` + `dryRun` + `mutantRun` (+ optional `init`/`dispose`)     |
|  [02]   | `TestRunnerCapabilities`          | interface            | `{ reloadEnvironment: boolean }` — worker-reuse capability advertisement  |
|  [03]   | `DryRunResult`                    | discriminated union  | `Complete{tests, mutantCoverage?} \| Error \| Timeout` on `DryRunStatus`  |
|  [04]   | `MutantRunResult`                 | discriminated union  | `Killed \| Survived \| Timeout \| Error` on `MutantRunStatus`             |
|  [05]   | `MutantRunOptions`                | interface            | `activeMutant` + `sandboxFileName` + `testFilter` + `hitLimit` + activation|
|  [06]   | `MutantActivation`                | union                | `'runtime' \| 'static'` — when the mutant switch flips                    |
|  [07]   | `TestResult`                      | discriminated union  | `Success \| Failed \| Skipped` on `TestStatus` — dry-run per-test rows    |

```ts contract
// test-runner.d.ts / mutant-run-result.d.ts — the kill verdict is a tagged union; `Killed` names the killing tests.
interface TestRunner {
  capabilities(): Promise<TestRunnerCapabilities> | TestRunnerCapabilities
  init?(): Promise<void>
  dryRun(options: DryRunOptions): Promise<DryRunResult>       // baseline: run all specs, collect per-test coverage
  mutantRun(options: MutantRunOptions): Promise<MutantRunResult>
  dispose?(): Promise<void>
}
type MutantRunResult =
  | { status: MutantRunStatus.Killed;   killedBy: string[]; failureMessage: string; nrOfTests: number }
  | { status: MutantRunStatus.Survived; nrOfTests: number }
  | { status: MutantRunStatus.Timeout;  reason?: string }
  | { status: MutantRunStatus.Error;    errorMessage: string }
interface MutantRunOptions {
  activeMutant: Mutant; sandboxFileName: string
  testFilter?: string[]              // only the specs that cover this mutant (from dry-run perTest coverage)
  hitLimit?: number                  // abort a runaway mutant after N instrumentation hits (infinite-loop guard)
  mutantActivation: MutantActivation // 'static' = load-time, 'runtime' = per-test
  timeout: number; disableBail: boolean; reloadEnvironment: boolean
}
```

[INSTRUMENTATION_CHANNEL] — the runner augments `vitest`'s own context to pass mutant state INTO the worker and coverage back OUT, without a side channel. `ProvidedContext` carries the active mutant and hit budget to each test; `TaskMeta` carries the per-test hit count and `MutantCoverage` back — composing onto the host's canonical instrument channel (`INSTRUMENTER_CONSTANTS` / `MutantCoverage`, `stryker-mutator-core.md` [05]).

```ts contract
// stryker-setup.d.ts — `declare module 'vitest'` augmentation; the bridge between Stryker and the vitest worker.
declare module 'vitest' {
  interface ProvidedContext {
    globalNamespace: '__stryker__' | '__stryker2__'; activeMutant: string | undefined
    hitLimit: number | undefined; mutantActivation: MutantActivation; mode: 'mutant' | 'dry-run'
    isGreaterThanVitest4Point1: boolean                      // vitest>=4.1 API-shape switch read inside the worker
  }
  interface TaskMeta { hitCount: number | undefined; mutantCoverage: MutantCoverage | undefined }
}
```

## [03]-[CONFIG_AS_DATA]

The runner and the whole mutation gauge are ONE declarative options object `.config/stryker.config.json` owns — thresholds AS DATA; the assay mutation rail invokes it with `--configFile .config/stryker.config.json`. `testRunner: "vitest"` activates this plugin; `coverageAnalysis: "perTest"` unlocks the `testFilter` narrowing; `thresholds.break` is the CI kill floor. The `vitest` bag is the only plugin-owned surface — a config file pointer that reuses the folder's existing vitest config, so mutants run under the identical `@effect/vitest` setup the specs already use.

| [INDEX] | [CONFIG_ROW]                                    | [OWNER]              | [CAPABILITY]                                                           |
| :-----: | :---------------------------------------------- | :------------------- | :--------------------------------------------------------------------- |
|  [01]   | `mutate: string[]`                              | core                | the mutant-source glob; assay `--mutation changed` scopes it           |
|  [02]   | `testRunner: "vitest"`                          | core                | activates this plugin as the kill engine                               |
|  [03]   | `coverageAnalysis: 'off' \| 'all' \| 'perTest'` | core                | `perTest` → `testFilter` runs only covering specs per mutant           |
|  [04]   | `thresholds: { high; low; break }`              | core                | mutation-score policy; `break` is the CI fail floor (kill-ratio gate)  |
|  [05]   | `reporters: string[]` + `jsonReporter`          | core                | `["json","html","clear-text"]`; the JSON report is the gauge receipt   |
|  [06]   | `concurrency` / `maxTestRunnerReuse`            | core                | worker fan-out and reuse cap across the mutant sweep                   |
|  [07]   | `incremental` / `ignoreStatic` / `timeoutMS`    | core                | incremental cache, static-mutant policy, runaway-mutant timeout        |
|  [08]   | `vitest: { configFile?; dir?; related }`        | plugin              | reuse the folder vitest config; `related` narrows to changed-related   |

```ts contract
import type { PartialStrykerOptions } from "@stryker-mutator/api/core"   // the canonical schema — stryker-mutator-core.md [02]
// StrykerVitestRunnerOptions is the plugin-owned bag; MutationScoreThresholds/CoverageAnalysis are core ([02]).
interface StrykerVitestRunnerOptions { vitest: { dir?: string; related: boolean; configFile?: string } }
// .config/stryker.config.json encodes ONE PartialStrykerOptions (core [02]); both plugins' rows merge onto it (checker rows: typescript-checker.md [03]):
const strykerConfig = {
  mutate: ["src/**/*.ts", "!src/**/*.spec.ts"],
  checkers: ["typescript"], testRunner: "vitest",
  coverageAnalysis: "perTest",
  vitest: { configFile: "vitest.config.ts" },
  thresholds: { high: 90, low: 80, break: 80 },   // CI fails below 80% mutation score
  reporters: ["json", "html", "clear-text"],
} satisfies PartialStrykerOptions & StrykerVitestRunnerOptions
```

## [04]-[INTEGRATION]

[STACK: `vitest-runner` executes the folder's `@effect/vitest` specs] — the runner does not run its own tests; it runs the SAME specs every folder authors with `@effect/vitest` `it.effect` / `it.scoped` / `it.prop` and the `layer(SharedLayer)` combinator (`fast-check.md` [05]). Each mutant is measured by whether those existing specs kill it — so the `_testkit` law combinators (fold identity, merge commutativity, upcast totality via `fast-check`) and the Schema-derived arbitraries ARE the mutation kill force. A weak property that under-constrains its arbitrary shows up here as a `Survived` mutant. `vitest.configFile` MUST be the config those specs already run under, so no divergence exists between the CI test run and the mutant run.

[STACK: shared harness Layers as the mutant-execution environment] — because the runner reuses one vitest worker across mutants (`reloadEnvironment` reported per `TestRunnerCapabilities`), a spec's acquired Layers persist across `mutantRun` calls. A `layer(PgLiteTest)` unit Layer (`electric-sql-pglite.md` [04]) or a `layer(PgContainer)` container Layer (`testcontainers.md` [04]) is built once and re-entered per mutant — so those Layers must be idempotent and leave no cross-mutant state (a mutant must not see another mutant's rows). `hitLimit` + `Effect.timeout` guard a mutant that drives an acquired resource into an infinite loop; `disableBail` keeps a spec block running so `killedBy` names every catching test, not just the first.

[STACK: assay `test --mutation` + the checker pair] — `uv run python -m tools.assay test run --mutation changed|full --typescript` loads `@stryker-mutator/core`, which runs the `typescript-checker` compile gate (`stryker-mutator-typescript-checker.md`) then this runner. The JSON reporter output is the gauge receipt the assay rail scores against its kill floor; `thresholds.break` is that floor expressed as config data. `vitest.related: true` aligns with `--mutation changed` — both narrow to changed-related specs.

## [05]-[RAIL_LAW]

- Owns: the mutant kill-execution — one dry run to collect `perTest` coverage, then a coverage-narrowed `mutantRun` per mutant returning the `Killed`/`Survived`/`Timeout`/`Error` verdict — over the folder's existing `@effect/vitest` specs in one reused worker.
- Accept: `testRunner: "vitest"` + `coverageAnalysis: "perTest"` for the `testFilter` narrowing; `vitest.configFile` pointing at the folder's real vitest config; `thresholds.break` as the enforced kill floor; `hitLimit`/`timeoutMS` as runaway-mutant guards; `incremental` for cross-run caching.
- Reject: a `vitest` config divergent from the specs' own (mutant runs that disagree with CI); `coverageAnalysis: "off"` on a non-trivial suite (every spec runs for every mutant — the run never finishes); stateful shared Layers that leak rows across mutants under worker reuse; importing any symbol from this package into source (config row only — the `tests/typescript/_architecture` suite bans runtime import).
- Boundary: this is a TestRunner plugin — it renders a `MutantRunResult`, never a compile verdict; compile-invalid mutants are filtered upstream by `typescript-checker` and never reach `mutantRun`. `Survived` is a genuine test gap (tighten the arbitrary or add a law); `CompileError` is out of scope by construction and lives in the checker's `CheckResult`, not here.
