# [TS_TESTS_API_STRYKER_MUTATOR_CORE]

[PACKAGE_SURFACE]:
- package: `@stryker-mutator/core` · version `9.6.1` · license `Apache-2.0`
- module: ESM (`type: module`); `exports` map `.` → `dist/src/index.js` (types resolve via the co-located `dist/src/index.d.ts`, no explicit types condition). Barrel exports `{ Stryker, StrykerCli }`, default `Stryker`.
- api peer: `@stryker-mutator/api` `9.6.1` (Apache-2.0), seven subpaths — `./core`, `./plugin`, `./report`, `./test-runner`, `./check`, `./ignore`, `./logging`. This catalog is the CANONICAL home for the api surface the plugin HOST owns: `StrykerOptions`/`PartialStrykerOptions` (`./core`), `MutantResult`/`MutantStatus` (`./core`), `Reporter` + the instrument channel (`./report`, `./core`), and the `PluginKind` plugin-loading ABI (`./plugin`); the two SPIs a runner/checker plugin IMPLEMENTS (`TestRunner` in `./test-runner`, `Checker` in `./check`) are owned deeply by the sibling `stryker-mutator-vitest-runner.md` / `stryker-mutator-typescript-checker.md`; the receipt they feed and the ABI they register through live here. `stryker.config.json` encodes THIS object — the plugin catalogs re-document only the partial config rows they contribute.
- receipt schema: `MutantResult.status` and `MutantStatus` re-export from `mutation-testing-report-schema` `3.7.3` (as `@stryker-mutator/api/core`'s `schema.*`); `Reporter.onMutationTestReportReady` folds a `schema.MutationTestResult` + a `MutationTestMetricsResult` (`mutation-testing-metrics` `3.7.3`).
- asset: pure JS; forks OS worker processes (not `worker_threads`) for checkers and test runners.
- runtime: node `>=20`; DI via `typed-inject`; config validated against a JSON schema (`stryker.config.json` / `stryker.conf.js` / `.mjs`).
- plane: `plane:dev` — the `MUTATION_GAUGE` of the mutation/coverage gauge; `stryker.config.json` pairs its thresholds with the `@vitest/coverage-v8` line/branch thresholds as one "thresholds as data" surface; `tests/typescript/_architecture` fences it off every runtime graph.
- rail: mutation-engine / threshold gauge / plugin host.

`@stryker-mutator/core` is the mutation engine the assay mutation rail drives (`--configFile stryker.config.json`) AND the plugin host both admitted Stryker plugins load through: it instruments production files, forks worker processes, runs each surviving mutant through a test runner, and scores the kill ratio against a `MutationScoreThresholds` gate. `stryker.config.json` encodes thresholds, `mutate` globs, reporters, checkers, and the runner as ONE declarative `PartialStrykerOptions` data object; the runner and checker are PLUGINS on one `PluginKind` SPI the host owns — `@stryker-mutator/vitest-runner` and `@stryker-mutator/typescript-checker` are two `strykerPlugins` rows the config selects by string, each contributing partial `StrykerOptions` rows (documented against this catalog's canonical schema) with one plugin-owned option bag.

## [01]-[ENGINE]

[SERVICES]: the two programmatic entrypoints — the engine class the gauge runs and the argv wrapper it never touches.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :----------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `Stryker`    | class         | `new Stryker(cliOptions).runMutationTest()` → `Promise<MutantResult[]>` — the gauge entry |
|  [02]   | `StrykerCli` | class         | `new StrykerCli(argv).run()` — the `stryker run` command wrapper; not the gauge path      |

```ts signature
import { Stryker, StrykerCli } from "@stryker-mutator/core"
import type { PartialStrykerOptions, MutantResult } from "@stryker-mutator/api/core"
import type { createInjector } from "typed-inject"
// Gauge runs Stryker and folds MutantResult[] against the thresholds itself — no CLI, no parsing stdout.
declare class Stryker {
  constructor(cliOptions: PartialStrykerOptions, injectorFactory?: typeof createInjector)  // injectorFactory: tests only
  runMutationTest(): Promise<MutantResult[]>
}
// CLI takes argv at construction; .run() parses + dispatches; the gauge never constructs this.
declare class StrykerCli {
  constructor(argv: string[], program?: unknown, runMutationTest?: unknown, runMutationTestingServer?: unknown)
  run(createInjectorImpl?: typeof createInjector): void
}
export default Stryker
```

## [02]-[OPTIONS_AND_THRESHOLDS]

[PUBLIC_TYPE_SCOPE]: `@stryker-mutator/api/core`'s `StrykerOptions` is the JSON-schema-generated config; `PartialStrykerOptions` is a DEEP-partial (nested keys like `dashboard.project` are optional too) — the exact `Stryker` constructor input; the gauge encodes the whole config as ONE data object, every field below a canonical row the plugin catalogs reference rather than redefine; the one field the gauge exists to own is `thresholds: MutationScoreThresholds` — three data points, not a matrix of flags.

```ts signature
// from @stryker-mutator/api/core (src-generated/stryker-core, JSON-schema generated). The gauge encodes these as data.
interface StrykerOptions {
  mutate: string[]                                   // production files to mutate (glob; `file:startLine-endLine` for a range)
  testRunner: string                                 // "vitest" — selects the TestRunner plugin (vitest-runner.md [02])
  testRunnerNodeArgs: string[]                       // node args passed to the test-runner child process
  checkers: string[]                                 // ["typescript"] — selects Checker plugins that gate mutants pre-run (typescript-checker.md [02])
  checkerNodeArgs: string[]                          // node args passed to the checker child process (heap for large graphs)
  coverageAnalysis: CoverageAnalysis                 // "perTest" only runs a mutant against tests that cover its line — the speed rail
  concurrency?: number | string                      // worker-process fan; assay serializes runs under a mutation-<lang> lease
  reporters: string[]                                // ["html","json","clear-text","progress"] — Reporter plugins ([05])
  thresholds: MutationScoreThresholds
  ignoreStatic: boolean                              // drop mutants only hit during static (module-load) execution
  disableTypeChecks: boolean | string                // glob of files whose `// @ts-nocheck` is injected so instrumented code still type-checks
  incremental: boolean; incrementalFile: string; force: boolean   // reuse prior results to only re-run changed mutants; force = rebuild cache
  ignorePatterns: string[]; tempDirName: string      // sandbox copy filters + temp dir name
  timeoutMS: number; timeoutFactor: number           // runaway-mutant absolute + relative-to-baseline timeout
  tsconfigFile: string                               // the tsconfig the checker's in-memory compiler loads
  plugins: string[]                                  // extra node modules to require; @stryker-mutator/* auto-loaded
  dashboard: DashboardOptions                        // { project?; version?; module?; baseUrl; reportType } — dashboard reporter target
}
type CoverageAnalysis = "off" | "all" | "perTest"
type Percentage = number
interface MutationScoreThresholds { high: Percentage; low: Percentage; break: Percentage | null }  // break = the CI-fail floor
type PartialStrykerOptions = DeepPartial<StrykerOptions>   // deep-optional; the Stryker constructor input
```

`thresholds.break` is the hard gate: a score below it fails the run (`null` = never fail); `high`/`low` only color the report. `coverageAnalysis: "perTest"` is why the kill ratio is fast — it maps each mutant to its covering tests via the runner's dry-run coverage rather than running the whole suite per mutant.

## [03]-[RECEIPT]

[PUBLIC_TYPE_SCOPE]: `MutantResult` is the per-mutant receipt `runMutationTest()` returns and `Reporter.onMutantTested` streams; `MutantStatus` is its bounded verdict vocabulary. Both re-export from `mutation-testing-report-schema` through `@stryker-mutator/api/core` — the status tokens are PascalCase, `MutantResult = Mutant & schema.MutantResult`.

```ts signature
import type { MutantResult, MutantStatus } from "@stryker-mutator/api/core"
// Canonical receipt vocabulary — a PascalCase string union, NOT a message field. Match on the token.
type MutantStatus = "Killed" | "Survived" | "NoCoverage" | "CompileError" | "RuntimeError" | "Timeout" | "Ignored" | "Pending"
// MutantResult = Mutant & schema.MutantResult — the gauge folds an array of these.
interface MutantResult {
  id: string; mutatorName: string; location: Location; replacement: string; fileName: string   // required
  status: MutantStatus
  coveredBy?: string[]; killedBy?: string[]; static?: boolean; statusReason?: string           // detected-by / static-mutant / failure text
  testsCompleted?: number; description?: string; duration?: number                             // bail count / mutation desc / net ms
}
```

Score semantics (the `mutation-testing-metrics` fold the gauge scores against `thresholds.break`) — the tokens partition into three buckets, and the kill ratio is NOT `Killed / total`:
- DETECTED = `Killed + Timeout` (a timed-out mutant is killed-equivalent — it changed behavior enough to hang) — the numerator.
- UNDETECTED = `Survived + NoCoverage` (a test gap: either a covered mutant no spec caught, or an uncovered line) — the rest of the valid denominator.
- INVALID / excluded = `CompileError + RuntimeError` (false positives, out of the score) with `Ignored` / `Pending` (config-excluded / not-yet-run).
- `mutationScore = DETECTED / (DETECTED + UNDETECTED) = (Killed + Timeout) / (Killed + Timeout + Survived + NoCoverage)`; `mutationScoreBasedOnCoveredCode` drops `NoCoverage` from the denominator.

This is exactly why the checker matters: it moves a doomed mutant to `CompileError` (INVALID), out of the denominator, instead of letting it masquerade as `Survived` (UNDETECTED) and depress the score. `@stryker-mutator/api/check` also declares an internal lowercase `enum MutantStatus` (`"killed"`/`"timedOut"`/…), but that enum is NOT exported from the `./check` barrel and is NOT the receipt vocabulary — the checker's public verdict is `CheckStatus` (typescript-checker.md [02]), which the host maps onto `MutantStatus.CompileError`.

## [04]-[PLUGIN_SPI]

[PUBLIC_TYPE_SCOPE]: the `@stryker-mutator/api/plugin` loading ABI the host owns and both admitted plugins register through. A plugin is ONE parameterized descriptor (`PluginKind` × the three `declare*Plugin` forms), never a hardcoded runner/checker set; the sibling packages export `strykerPlugins: FactoryPlugin<PluginKind.*, ["$injector"]>[]` rows the host discovers by convention.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :---------------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `PluginKind`                  | enum          | `Checker \| TestRunner \| Reporter \| Ignore` — the plugin taxonomy               |
|  [02]   | `PluginInterfaces`            | lookup type   | kind → the SPI interface a plugin of that kind implements                         |
|  [03]   | `Plugins`                     | lookup type   | kind → the `Plugin<K>` DESCRIPTOR (class/factory/value), never the interface      |
|  [04]   | `FactoryPlugin<K, Tokens>`    | interface     | `{ kind; name; factory }` — the DI-factory registration both admitted plugins use |
|  [05]   | `ValuePlugin` / `ClassPlugin` | interface     | the value / class descriptor variants of `Plugin<K>`                              |
|  [06]   | `declareFactoryPlugin`        | function      | type-checks a plugin's DI graph and returns a `FactoryPlugin<K, Tokens>`          |
|  [07]   | `commonTokens` / `tokens`     | const / fn    | the DI token constants + the string-literal-tuple helper typing `["$injector"]`   |

```ts signature
// from @stryker-mutator/api/plugin — PluginKind selects the phase; the descriptor carries the DI factory.
enum PluginKind { Checker = "Checker", TestRunner = "TestRunner", Reporter = "Reporter", Ignore = "Ignore" }
interface PluginInterfaces { [PluginKind.Reporter]: Reporter; [PluginKind.TestRunner]: TestRunner; [PluginKind.Checker]: Checker; [PluginKind.Ignore]: Ignorer }
type Plugins = { [K in keyof PluginInterfaces]: Plugin<K> }                    // kind → descriptor (NOT the interface)
type Plugin<K extends PluginKind> = ClassPlugin<K, Tokens> | FactoryPlugin<K, Tokens> | ValuePlugin<K>
interface FactoryPlugin<K extends PluginKind, Tokens extends InjectionToken<PluginContext>[]> {
  readonly kind: K; readonly name: string
  readonly factory: InjectableFunction<PluginContext, PluginInterfaces[K], Tokens>   // typed-inject DI factory
}
interface ValuePlugin<K extends PluginKind> { readonly kind: K; readonly name: string; readonly value: PluginInterfaces[K] }
interface ClassPlugin<K extends PluginKind, Tokens> { readonly kind: K; readonly name: string; readonly injectableClass: InjectableClass<PluginContext, PluginInterfaces[K], Tokens> }
declare function declareFactoryPlugin<K extends PluginKind, Tokens>(kind: K, name: string, factory: InjectableFunction<PluginContext, PluginInterfaces[K], Tokens>): FactoryPlugin<K, Tokens>
declare function declareValuePlugin<K extends PluginKind>(kind: K, name: string, value: PluginInterfaces[K]): ValuePlugin<K>
declare function declareClassPlugin<K extends PluginKind, Tokens>(kind: K, name: string, injectableClass: InjectableClass<PluginContext, PluginInterfaces[K], Tokens>): ClassPlugin<K, Tokens>
declare const commonTokens: Readonly<{ getLogger: "getLogger"; injector: "$injector"; logger: "logger"; options: "options"; fileDescriptions: "fileDescriptions"; target: "$target" }>
declare function tokens<TS extends string[]>(...tokensList: TS): TS       // string-literal tuple, e.g. tokens(commonTokens.injector)
```

Four SPIs a plugin implements, keyed by `PluginKind`: `TestRunner` (`dryRun`/`mutantRun`; owned by `stryker-mutator-vitest-runner.md` [02]), `Checker` (`check`/`group?`; owned by `stryker-mutator-typescript-checker.md` [02]), `Reporter` ([05] below), and `Ignorer` (`shouldIgnore(path): string | undefined` — the Ignore SPI that suppresses mutants in matched code patterns); the mutation gauge registers a `FactoryPlugin<TestRunner>` and a `FactoryPlugin<Checker>`; a custom gauge reporter registers a `ValuePlugin<Reporter>` or `FactoryPlugin<Reporter>`.

## [05]-[REPORTER_AND_INSTRUMENT]

[PUBLIC_TYPE_SCOPE]: `Reporter` (`@stryker-mutator/api/report`) is the streaming/terminal result channel a custom gauge reporter implements — every method optional, all fired by the host; the instrument surface (`@stryker-mutator/api/core`) is the canonical mutant-activation channel the vitest-runner's `declare module 'vitest'` augmentation (vitest-runner.md [02]) composes onto: `INSTRUMENTER_CONSTANTS` names the injected identifiers, `InstrumenterContext` is the per-worker mutation state, and `MutantCoverage` is the `perTest` coverage payload that drives `coverageAnalysis`.

```ts signature
// @stryker-mutator/api/report — the machine result the gauge folds; onMutantTested streams, onMutationTestReportReady is terminal.
interface Reporter {
  onDryRunCompleted?(event: DryRunCompletedEvent): void              // { result; timing:{net,overhead}; capabilities }
  onMutationTestingPlanReady?(event: MutationTestingPlanReadyEvent): void   // { mutantPlans: readonly MutantTestPlan[] }
  onMutantTested?(result: Readonly<MutantResult>): void              // one receipt per mutant as it completes
  onMutationTestReportReady?(report: Readonly<schema.MutationTestResult>, metrics: Readonly<MutationTestMetricsResult>): void
  wrapUp?(): Promise<void> | void                                    // flush async work before Stryker exits
}
// @stryker-mutator/api/core — the instrument channel; the runner augmentation carries these INTO the worker and coverage back OUT.
declare const INSTRUMENTER_CONSTANTS: Readonly<{
  NAMESPACE: "__stryker__"; MUTATION_COVERAGE_OBJECT: "mutantCoverage"; ACTIVE_MUTANT: "activeMutant"
  CURRENT_TEST_ID: "currentTestId"; HIT_COUNT: "hitCount"; HIT_LIMIT: "hitLimit"; ACTIVE_MUTANT_ENV_VARIABLE: "__STRYKER_ACTIVE_MUTANT__"
}>
interface InstrumenterContext { activeMutant?: string; currentTestId?: string; mutantCoverage?: MutantCoverage; hitCount?: number; hitLimit?: number }
interface MutantCoverage { static: CoverageData; perTest: CoveragePerTestId }   // perTest[testId][mutantId] = hit count
type CoverageData = Record<string, number>                                      // mutantId → times hit
type CoveragePerTestId = Record<string, CoverageData>                           // testId → CoverageData
```

## [06]-[INTEGRATION]

[STACK: `Stryker` + `@stryker-mutator/vitest-runner` + `@stryker-mutator/typescript-checker`] — the mutation gauge is this engine with two `strykerPlugins` rows on the ONE ABI ([04]). `testRunner: 'vitest'` runs each mutant through the SAME `vitest.config.ts` specs the unit/e2e lanes already own (no separate mutation spec authoring), and `checkers: ['typescript']` type-checks each mutant first so a mutant that breaks compilation is `MutantStatus.CompileError` — INVALID, out of the score denominator ([03]) instead of masquerading as `Survived`. Both are `FactoryPlugin<PluginKind.TestRunner|Checker, ["$injector"]>` rows the config selects by string; their SPI surfaces are the sibling `stryker-mutator-vitest-runner.md` / `stryker-mutator-typescript-checker.md`, and the config rows they contribute reference THIS catalog's [02] schema.

[STACK: Stryker thresholds + `@vitest/coverage-v8` thresholds] — the mutation/coverage gauge is "thresholds as data": `MutationScoreThresholds { high, low, break }` ([02]) lives in `stryker.config.json` beside the vitest coverage line/branch/function thresholds as ONE gate surface. Coverage answers "is this line executed"; mutation answers "is this line's behavior actually asserted" — the gauge fails when either floor breaks. `coverageAnalysis: 'perTest'` reuses the runner's `MutantCoverage` ([05]) to map mutants to covering tests.

[STACK: Stryker + the assay `test --mutation` rail] — the repo operator owns the invocation: `assay test --mutation changed` scopes Stryker via `--mutate <glob>` over changed files and holds an exclusive `mutation-<lang>` lease while worker processes fan out. This catalog owns the engine/config/receipt/SPI contract the gauge encodes; the operator owns when and how the process is spawned; the gauge folds `runMutationTest()`'s `MutantResult[]` (or a custom `Reporter`, [05]) — never parsed CLI stdout.

[STACK: `Stryker` + `fast-check`] — mutation testing is the meta-gauge on the property suite: a mutant that survives every generated case exposes a law too weak to pin the behavior, so a `Survived` result is the signal to strengthen a `_testkit` law combinator (tighten the invariant or widen the arbitrary), not merely to add an example.

## [07]-[RAIL_LAW]

- Owns: mutation instrumentation, worker-process fan-out, per-mutant test execution, and kill-ratio scoring against `MutationScoreThresholds`; the programmatic `Stryker.runMutationTest()` entry; the canonical `@stryker-mutator/api` surface the host owns — `StrykerOptions`/`PartialStrykerOptions` ([02]), `MutantResult`/`MutantStatus` ([03]), the `PluginKind` plugin-loading ABI ([04]), and `Reporter` + the instrument channel ([05]).
- Accept: `new Stryker({ mutate, testRunner: 'vitest', checkers: ['typescript'], coverageAnalysis: 'perTest', thresholds })` for the gauge; `MutationScoreThresholds.break` as the CI floor; `incremental` + `incrementalFile` to only re-run changed mutants; a custom `Reporter` to project `MutantResult[]` as gauge data; both plugin catalogs referencing THIS [02]/[03]/[04] rather than redefining the config schema, receipt, or ABI.
- Reject: parsing CLI stdout instead of folding `runMutationTest()`'s `MutantResult[]` (use the programmatic engine); scoring `CompileError`/`RuntimeError`/`Ignored`/`Pending` as valid, or treating `Timeout` as undetected (DETECTED = `Killed + Timeout`, UNDETECTED = `Survived + NoCoverage` — [03]); dropping the typescript checker (uncompilable mutants score as `Survived`); matching `MutantStatus`/`CheckStatus`/`MutantRunStatus` on message text rather than the token; documenting the internal `./check` lowercase `MutantStatus` enum as the receipt vocabulary (it is unexported — the receipt is the PascalCase union); any import from a `plane:runtime` folder — dev gauge only.
- Boundary: Stryker forks OS worker processes sized by `concurrency`; runs hold an exclusive per-language mutation lease. It is the slowest gauge — a full-suite mutation run is a scheduled/CI concern, `--mutation changed` the per-change lane.
