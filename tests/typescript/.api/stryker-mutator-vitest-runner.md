# [TS_TESTS_API_STRYKER_MUTATOR_VITEST_RUNNER]

`@stryker-mutator/vitest-runner` is the kill engine of the mutation/coverage gauge and the one admitted Stryker plugin. For every mutant the engine instruments — no checker pre-filters them, so a compile-invalid mutant arrives here too — Stryker activates the mutant (an env-flagged branch inside the instrumented source) and calls `mutantRun`; the runner executes the folder's `@effect/vitest` specs against that mutant and returns `Killed` (a spec failed — the mutant was caught), `Survived` (all specs passed — a test gap), `Timeout`, or `Error`. It reuses ONE vitest instance across the whole mutant sweep and narrows execution to only the specs that cover each mutant via `perTest` coverage — the difference between a mutation run that finishes and one that never does. This catalog owns the TestRunner kill-execution surface; the plugin-loading ABI (`PluginKind`/`FactoryPlugin`/`commonTokens`) and the canonical config-as-data schema (`StrykerOptions`/`PartialStrykerOptions`) this plugin rides are owned by `stryker-mutator-core.md` [04]/[02].

## [01]-[PLUGIN_ENTRY]

[PUBLIC_TYPE_SCOPE]: the two public exports (`strykerPlugins`, `strykerValidationSchema`) typed against the `@stryker-mutator/api/plugin` loading ABI that `stryker-mutator-core.md` [04] owns. `strykerPlugins` is the value `@stryker-mutator/core` discovers; a `FactoryPlugin` is a DI factory tagged by `PluginKind` and injected with `["$injector"]` (`commonTokens.injector`).

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]              | [CAPABILITY]                                               |
| :-----: | :---------------------------------------- | :------------------------- | :--------------------------------------------------------- |
|  [01]   | `strykerPlugins`                          | `FactoryPlugin[]`          | the only value export the host reads                       |
|  [02]   | `strykerValidationSchema`                 | JSON schema                | validates the `vitest` option bag                          |
|  [03]   | `PluginKind` / `FactoryPlugin<K, Tokens>` | shared ABI                 | plugin-loading types; owner `stryker-mutator-core.md` [04] |
|  [04]   | `VitestTestRunner`                        | internal `TestRunner` impl | the `FactoryPlugin` factory yield; implements [02]         |

```ts
import { PluginKind, FactoryPlugin } from '@stryker-mutator/api/plugin'
export declare const strykerPlugins: FactoryPlugin<PluginKind.TestRunner, ["$injector"]>[]
export declare const strykerValidationSchema: typeof import('../schema/vitest-runner-options.json')
```

## [02]-[TEST_RUNNER_CONTRACT]

[PUBLIC_TYPE_SCOPE]: the `@stryker-mutator/api/test-runner` contract — one dry run establishes per-mutant coverage, then one `mutantRun` per mutant returns a four-arm verdict union. Both result shapes are discriminated unions on `status`, never boolean pairs.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]       | [CAPABILITY]                                                                |
| :-----: | :----------------------- | :------------------ | :-------------------------------------------------------------------------- |
|  [01]   | `TestRunner`             | interface           | `capabilities` + `dryRun` + `mutantRun` (+ optional `init`/`dispose`)       |
|  [02]   | `TestRunnerCapabilities` | interface           | `{ reloadEnvironment: boolean }` — worker-reuse capability advertisement    |
|  [03]   | `DryRunResult`           | discriminated union | `Complete{tests, mutantCoverage?} \| Error \| Timeout` on `DryRunStatus`    |
|  [04]   | `MutantRunResult`        | discriminated union | `Killed \| Survived \| Timeout \| Error` on `MutantRunStatus`               |
|  [05]   | `MutantRunOptions`       | interface           | `activeMutant` + `sandboxFileName` + `testFilter` + `hitLimit` + activation |
|  [06]   | `MutantActivation`       | union               | `'runtime' \| 'static'` — when the mutant switch flips                      |
|  [07]   | `TestResult`             | discriminated union | `Success \| Failed \| Skipped` on `TestStatus` — dry-run per-test rows      |

```ts
interface TestRunner {
  capabilities(): Promise<TestRunnerCapabilities> | TestRunnerCapabilities
  init?(): Promise<void>
  dryRun(options: DryRunOptions): Promise<DryRunResult>
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
  testFilter?: string[]
  hitLimit?: number
  mutantActivation: MutantActivation
  timeout: number; disableBail: boolean; reloadEnvironment: boolean
}
```

[INSTRUMENTATION_CHANNEL] — the runner augments `vitest`'s own context to pass mutant state INTO the worker and coverage back OUT, without a side channel. `ProvidedContext` carries the active mutant and hit budget to each test; `TaskMeta` carries the per-test hit count and `MutantCoverage` back — composing onto the host's canonical instrument channel (`INSTRUMENTER_CONSTANTS` / `MutantCoverage`, `stryker-mutator-core.md` [05]).

```ts
declare module 'vitest' {
  interface ProvidedContext {
    globalNamespace: '__stryker__' | '__stryker2__'; activeMutant: string | undefined
    hitLimit: number | undefined; mutantActivation: MutantActivation; mode: 'mutant' | 'dry-run'
    isGreaterThanVitest4Point1: boolean
  }
  interface TaskMeta { hitCount: number | undefined; mutantCoverage: MutantCoverage | undefined }
}
```

## [03]-[CONFIG_AS_DATA]

| [INDEX] | [CONFIG_ROW]                                    | [OWNER] | [CAPABILITY]                                                          |
| :-----: | :---------------------------------------------- | :------ | :-------------------------------------------------------------------- |
|  [01]   | `testRunner: "vitest"`                          | core    | activates this plugin as the kill engine                              |
|  [02]   | `coverageAnalysis: 'off' \| 'all' \| 'perTest'` | core    | `perTest` → `testFilter` runs only covering specs per mutant          |
|  [03]   | `thresholds: { high; low; break }`              | core    | mutation-score policy; `break` is the CI fail floor (kill-ratio gate) |
|  [04]   | `reporters: string[]` + `jsonReporter`          | core    | `["json","html","clear-text"]`; the JSON report is the gauge verdict  |
|  [05]   | `concurrency` / `maxTestRunnerReuse`            | core    | worker fan-out and reuse cap across the mutant sweep                  |
|  [06]   | `incremental` / `ignoreStatic` / `timeoutMS`    | core    | incremental cache, static-mutant policy, runaway-mutant timeout       |
|  [07]   | `vitest: { configFile?; dir?; related }`        | plugin  | reuse the folder vitest config; `related` narrows to changed-related  |

```ts
import type { PartialStrykerOptions } from "@stryker-mutator/api/core"
interface StrykerVitestRunnerOptions { vitest: { dir?: string; related: boolean; configFile?: string } }
const strykerConfig = {
  mutate: ["src/**/*.ts", "!src/**/*.spec.ts"],
  testRunner: "vitest",
  coverageAnalysis: "perTest",
  vitest: { configFile: "vitest.config.ts" },
  thresholds: { high: 90, low: 80, break: 80 },
  reporters: ["json", "html", "clear-text"],
} satisfies PartialStrykerOptions & StrykerVitestRunnerOptions
```

## [04]-[INTEGRATION]

[STACK: `vitest-runner` executes the folder's `@effect/vitest` specs] — the runner does not run its own tests; it runs the SAME specs every folder authors with `@effect/vitest` `it.effect` / `it.scoped` / `it.prop` and the `layer(SharedLayer)` combinator (`fast-check.md` [05]). Each mutant is measured by whether those existing specs kill it — so the `testkit` law combinators (fold identity, merge commutativity, upcast totality via `fast-check`) and the Schema-derived arbitraries ARE the mutation kill force. Weak properties that under-constrain their arbitrary show up here as `Survived` mutants. `vitest.configFile` MUST be the config those specs already run under, so no divergence exists between the CI test run and the mutant run.

[STACK: shared harness Layers as the mutant-execution environment] — because the runner reuses one vitest worker across mutants (`reloadEnvironment` reported per `TestRunnerCapabilities`), a spec's acquired Layers persist across `mutantRun` calls. `layer(PgLiteTest)` unit Layers (`electric-sql-pglite.md` [04]) are built once and re-entered per mutant — so each Layer must be idempotent and leave no cross-mutant state (a mutant must not see another mutant's rows). `hitLimit` + `Effect.timeout` guard a mutant that drives an acquired resource into an infinite loop; `disableBail` keeps a spec block running so `killedBy` names every catching test, not just the first.
