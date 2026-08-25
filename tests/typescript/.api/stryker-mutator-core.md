# [TS_TESTS_API_STRYKER_MUTATOR_CORE]

## [01]-[ENGINE]

[SERVICES]: the two programmatic entrypoints — the engine class the gauge runs and the argv wrapper it never touches.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :----------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `Stryker`    | class         | `new Stryker(cliOptions).runMutationTest()` → `Promise<MutantResult[]>` — the gauge entry |
|  [02]   | `StrykerCli` | class         | `new StrykerCli(argv).run()` — the `stryker run` command wrapper; not the gauge path      |

```ts
import { Stryker, StrykerCli } from "@stryker-mutator/core"
import type { PartialStrykerOptions, MutantResult } from "@stryker-mutator/api/core"
import type { createInjector } from "typed-inject"
declare class Stryker {
  constructor(cliOptions: PartialStrykerOptions, injectorFactory?: typeof createInjector)
  runMutationTest(): Promise<MutantResult[]>
}
declare class StrykerCli {
  constructor(argv: string[], program?: unknown, runMutationTest?: unknown, runMutationTestingServer?: unknown)
  run(createInjectorImpl?: typeof createInjector): void
}
export default Stryker
```

## [02]-[OPTIONS_AND_THRESHOLDS]

[PUBLIC_TYPE_SCOPE]: `@stryker-mutator/api/core`'s `StrykerOptions` is the JSON-schema-generated config; `PartialStrykerOptions` is a DEEP-partial (nested keys like `dashboard.project` are optional too) — the exact `Stryker` constructor input; the gauge encodes the whole config as ONE data object, every field below a canonical row the plugin catalogs reference rather than redefine; the one field the gauge exists to own is `thresholds: MutationScoreThresholds` — three data points, not a matrix of flags.

```ts
interface StrykerOptions {
  mutate: string[]
  testRunner: string
  testRunnerNodeArgs: string[]
  checkers: string[]
  checkerNodeArgs: string[]
  coverageAnalysis: CoverageAnalysis
  concurrency?: number | string
  reporters: string[]
  thresholds: MutationScoreThresholds
  ignoreStatic: boolean
  disableTypeChecks: boolean | string
  incremental: boolean; incrementalFile: string; force: boolean
  ignorePatterns: string[]; tempDirName: string
  timeoutMS: number; timeoutFactor: number
  tsconfigFile: string
  plugins: string[]
  dashboard: DashboardOptions
}
type CoverageAnalysis = "off" | "all" | "perTest"
type Percentage = number
interface MutationScoreThresholds { high: Percentage; low: Percentage; break: Percentage | null }
type PartialStrykerOptions = DeepPartial<StrykerOptions>
```

`thresholds.break` is the hard gate: a score below it fails the run (`null` = never fail); `high`/`low` only color the report. `coverageAnalysis: "perTest"` is why the kill ratio is fast — it maps each mutant to its covering tests via the runner's dry-run coverage rather than running the whole suite per mutant.

## [03]-[MUTANT_RESULT]

[PUBLIC_TYPE_SCOPE]: `MutantResult` is the per-mutant result `runMutationTest()` returns and `Reporter.onMutantTested` streams; `MutantStatus` is its bounded verdict vocabulary. Both re-export from `mutation-testing-report-schema` through `@stryker-mutator/api/core` — the status tokens are PascalCase, `MutantResult = Mutant & schema.MutantResult`.

```ts
import type { MutantResult, MutantStatus } from "@stryker-mutator/api/core"
type MutantStatus = "Killed" | "Survived" | "NoCoverage" | "CompileError" | "RuntimeError" | "Timeout" | "Ignored" | "Pending"
interface MutantResult {
  id: string; mutatorName: string; location: Location; replacement: string; fileName: string
  status: MutantStatus
  coveredBy?: string[]; killedBy?: string[]; static?: boolean; statusReason?: string
  testsCompleted?: number; description?: string; duration?: number
}
```

Score semantics (the `mutation-testing-metrics` fold the gauge scores against `thresholds.break`) — the tokens partition into three buckets, and the kill ratio is NOT `Killed / total`:
- DETECTED = `Killed + Timeout` (a timed-out mutant is killed-equivalent — it changed behavior enough to hang) — the numerator.
- UNDETECTED = `Survived + NoCoverage` (a test gap: either a covered mutant no spec caught, or an uncovered line) — the rest of the valid denominator.
- INVALID / excluded = `CompileError + RuntimeError` (false positives, out of the score) with `Ignored` / `Pending` (config-excluded / not-yet-run).
- `mutationScore = DETECTED / (DETECTED + UNDETECTED) = (Killed + Timeout) / (Killed + Timeout + Survived + NoCoverage)`; `mutationScoreBasedOnCoveredCode` drops `NoCoverage` from the denominator.

Uncheckered, a doomed mutant masquerades as `Survived` (UNDETECTED) instead of landing `CompileError` (INVALID) outside the denominator, so the kill floor prices that depression in rather than filtering it out. `@stryker-mutator/api/check` also declares an internal lowercase `enum MutantStatus` (`"killed"`/`"timedOut"`/…), but that enum is NOT exported from the `./check` barrel and is NOT the result vocabulary — a checker's public verdict is `CheckStatus`, which the host maps onto `MutantStatus.CompileError`.

## [04]-[PLUGIN_SPI]

[PUBLIC_TYPE_SCOPE]: the `@stryker-mutator/api/plugin` loading ABI the host owns and the admitted runner registers through. Plugins are ONE parameterized descriptor each (`PluginKind` × the three `declare*Plugin` forms), never a hardcoded runner/checker set; a plugin package exports `strykerPlugins: FactoryPlugin<PluginKind.*, ["$injector"]>[]` rows the host discovers by convention.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :---------------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `PluginKind`                  | enum          | `Checker \| TestRunner \| Reporter \| Ignore` — the plugin taxonomy               |
|  [02]   | `PluginInterfaces`            | lookup type   | kind → the SPI interface a plugin of that kind implements                         |
|  [03]   | `Plugins`                     | lookup type   | kind → the `Plugin<K>` DESCRIPTOR (class/factory/value), never the interface      |
|  [04]   | `FactoryPlugin<K, Tokens>`    | interface     | `{ kind; name; factory }` — the DI-factory registration both admitted plugins use |
|  [05]   | `ValuePlugin` / `ClassPlugin` | interface     | the value / class descriptor variants of `Plugin<K>`                              |
|  [06]   | `declareFactoryPlugin`        | function      | type-checks a plugin's DI graph and returns a `FactoryPlugin<K, Tokens>`          |
|  [07]   | `commonTokens` / `tokens`     | const / fn    | the DI token constants + the string-literal-tuple helper typing `["$injector"]`   |

```ts
enum PluginKind { Checker = "Checker", TestRunner = "TestRunner", Reporter = "Reporter", Ignore = "Ignore" }
interface PluginInterfaces { [PluginKind.Reporter]: Reporter; [PluginKind.TestRunner]: TestRunner; [PluginKind.Checker]: Checker; [PluginKind.Ignore]: Ignorer }
type Plugins = { [K in keyof PluginInterfaces]: Plugin<K> }
type Plugin<K extends PluginKind> = ClassPlugin<K, Tokens> | FactoryPlugin<K, Tokens> | ValuePlugin<K>
interface FactoryPlugin<K extends PluginKind, Tokens extends InjectionToken<PluginContext>[]> {
  readonly kind: K; readonly name: string
  readonly factory: InjectableFunction<PluginContext, PluginInterfaces[K], Tokens>
}
interface ValuePlugin<K extends PluginKind> { readonly kind: K; readonly name: string; readonly value: PluginInterfaces[K] }
interface ClassPlugin<K extends PluginKind, Tokens> { readonly kind: K; readonly name: string; readonly injectableClass: InjectableClass<PluginContext, PluginInterfaces[K], Tokens> }
declare function declareFactoryPlugin<K extends PluginKind, Tokens>(kind: K, name: string, factory: InjectableFunction<PluginContext, PluginInterfaces[K], Tokens>): FactoryPlugin<K, Tokens>
declare function declareValuePlugin<K extends PluginKind>(kind: K, name: string, value: PluginInterfaces[K]): ValuePlugin<K>
declare function declareClassPlugin<K extends PluginKind, Tokens>(kind: K, name: string, injectableClass: InjectableClass<PluginContext, PluginInterfaces[K], Tokens>): ClassPlugin<K, Tokens>
declare const commonTokens: Readonly<{ getLogger: "getLogger"; injector: "$injector"; logger: "logger"; options: "options"; fileDescriptions: "fileDescriptions"; target: "$target" }>
declare function tokens<TS extends string[]>(...tokensList: TS): TS
```

Four SPIs a plugin implements, keyed by `PluginKind`: `TestRunner` (`dryRun`/`mutantRun`; owned by `stryker-mutator-vitest-runner.md` [02]), `Checker` (`check`/`group?`; no admitted package implements it), `Reporter` ([05] below), and `Ignorer` (`shouldIgnore(path): string | undefined` — the Ignore SPI that suppresses mutants in matched code patterns); the mutation gauge registers a `FactoryPlugin<TestRunner>` alone; a custom gauge reporter registers a `ValuePlugin<Reporter>` or `FactoryPlugin<Reporter>`.

## [05]-[REPORTER_AND_INSTRUMENT]

[PUBLIC_TYPE_SCOPE]: `Reporter` (`@stryker-mutator/api/report`) is the streaming/terminal result channel a custom gauge reporter implements — every method optional, all fired by the host; the instrument surface (`@stryker-mutator/api/core`) is the canonical mutant-activation channel the vitest-runner's `declare module 'vitest'` augmentation (vitest-runner.md [02]) composes onto: `INSTRUMENTER_CONSTANTS` names the injected identifiers, `InstrumenterContext` is the per-worker mutation state, and `MutantCoverage` is the `perTest` coverage payload that drives `coverageAnalysis`.

```ts
interface Reporter {
  onDryRunCompleted?(event: DryRunCompletedEvent): void
  onMutationTestingPlanReady?(event: MutationTestingPlanReadyEvent): void
  onMutantTested?(result: Readonly<MutantResult>): void
  onMutationTestReportReady?(report: Readonly<schema.MutationTestResult>, metrics: Readonly<MutationTestMetricsResult>): void
  wrapUp?(): Promise<void> | void
}
declare const INSTRUMENTER_CONSTANTS: Readonly<{
  NAMESPACE: "__stryker__"; MUTATION_COVERAGE_OBJECT: "mutantCoverage"; ACTIVE_MUTANT: "activeMutant"
  CURRENT_TEST_ID: "currentTestId"; HIT_COUNT: "hitCount"; HIT_LIMIT: "hitLimit"; ACTIVE_MUTANT_ENV_VARIABLE: "__STRYKER_ACTIVE_MUTANT__"
}>
interface InstrumenterContext { activeMutant?: string; currentTestId?: string; mutantCoverage?: MutantCoverage; hitCount?: number; hitLimit?: number }
interface MutantCoverage { static: CoverageData; perTest: CoveragePerTestId }
type CoverageData = Record<string, number>
type CoveragePerTestId = Record<string, CoverageData>
```

## [06]-[INTEGRATION]

[STACK: `Stryker` + `@stryker-mutator/vitest-runner`] — the mutation gauge is this engine with one `strykerPlugins` row on the ONE ABI ([04]). `testRunner: 'vitest'` runs each mutant through the SAME `vitest.config.ts` specs the unit lane already owns (no separate mutation spec authoring). `checkers` stays empty — the TypeScript checker cannot boot against the TS7 API stub — so a mutant that breaks compilation reaches the runner and scores `Survived` rather than `MutantStatus.CompileError`, and the kill floor absorbs that noise ([03]). `@stryker-mutator/vitest-runner` registers one `FactoryPlugin<PluginKind.TestRunner, ["$injector"]>` row the config selects by string; its SPI surface is the sibling `stryker-mutator-vitest-runner.md`, and the config rows it contributes reference THIS catalog's [02] schema.

[STACK: Stryker thresholds + `@vitest/coverage-v8` thresholds] — the mutation/coverage gauge is "thresholds as data": `MutationScoreThresholds { high, low, break }` ([02]) lives in `stryker.config.json` beside the vitest coverage line/branch/function thresholds as ONE gate surface. Coverage answers "is this line executed"; mutation answers "is this line's behavior actually asserted" — the gauge fails when either floor breaks. `coverageAnalysis: 'perTest'` reuses the runner's `MutantCoverage` ([05]) to map mutants to covering tests.

[STACK: `Stryker` + `effect/FastCheck`] — mutation testing is the meta-gauge on the property suite: a mutant that survives every generated case exposes a law too weak to pin the behavior, so a `Survived` result is the signal to strengthen a `testkit` law combinator (tighten the invariant or widen the arbitrary), not merely to add an example.
