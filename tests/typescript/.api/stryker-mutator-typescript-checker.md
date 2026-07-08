# [@stryker-mutator/typescript-checker] — type-checks mutants so compile-error mutants never count against the kill ratio

[PACKAGE_SURFACE]:
- package: `@stryker-mutator/typescript-checker` · version `9.6.1` · license `Apache-2.0`
- module: ESM (`type: module`); one `.` export → `dist/src/index.js`; no deep-import paths; `engines.node >= 20`.
- asset: `dist/src/index.d.ts`; peer contract `@stryker-mutator/api` (`9.6.1`, the shared plugin ABI both admitted Stryker plugins bind).
- runtime: node-only; loads the project `typescript` as an in-memory compiler (no `tsc` subprocess, no emit) — a language-service diagnostic pass over the mutated virtual file system.
- plane: `plane:dev` — a Stryker Checker plugin loaded by `@stryker-mutator/core`; never imported by source and never bundled (the `tests/typescript/_architecture` purity audit holds trivially — it is a config row, not a value import).
- rail: mutation pre-filter / compile-validity gate.

`@stryker-mutator/typescript-checker` is the compile-validity gate of the mutation/coverage gauge. Stryker generates syntactic mutants blindly; a large fraction are type-errors (a `+` flipped to `*` on incompatible operands, a removed `await`, a narrowed return). Without a checker each such mutant burns a full `vitest-runner` execution only to die on a compile error, and — worse — a compile-error mutant that Stryker cannot distinguish from a genuine survivor pollutes the score. This plugin runs BEFORE the runner: it type-checks each mutant group in-memory and reports `CheckStatus.CompileError`, which Stryker classifies as `MutantStatus.CompileError` and EXCLUDES from the kill-ratio denominator — so the mutation score measures only type-valid mutants within the suite's reach. It is one half of the checker+runner pair; `stryker-mutator-vitest-runner.md` owns the kill-execution half, and `stryker-mutator-core.md` [04]/[02] owns the shared plugin-loading ABI and the canonical config-as-data schema both plugins ride. The plugin is pure configuration surface: activated by the `checkers` StrykerOptions row and tuned by four data fields, never a symbol a spec imports.

## [01]-[PLUGIN_ENTRY]

[PUBLIC_TYPE_SCOPE]: the three module exports — the plugin registration Stryker discovers, the standalone factory, and the JSON options schema. `strykerPlugins` is the only member `@stryker-mutator/core` reads; the rest are for embedding hosts.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]           | [CAPABILITY]                                                                                           |
| :-----: | :------------------------ | :---------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `strykerPlugins`          | `FactoryPlugin[]`       | the registration array; `FactoryPlugin<PluginKind.Checker, ["$injector"]>`                             |
|  [02]   | `createTypescriptChecker` | factory                 | `(injector) => TypescriptChecker`; standalone construction for embedders                               |
|  [03]   | `strykerValidationSchema` | JSON schema             | `typeof typescript-checker-options.json` — validates the plugin option bag                             |
|  [04]   | `TypescriptChecker`       | internal `Checker` impl | the class `createTypescriptChecker` returns (not a public export); `init` / `check` / `group` per [02] |

```ts contract
// index.d.ts @9.6.1 — the public barrel is three exports; PluginKind imports from @stryker-mutator/api/plugin (core [04]).
import { PluginKind } from '@stryker-mutator/api/plugin'
export declare const strykerPlugins: FactoryPlugin<PluginKind.Checker, ["$injector"]>[]
export declare const createTypescriptChecker: (injector: Injector<PluginContext>) => TypescriptChecker
export declare const strykerValidationSchema: typeof import('../schema/typescript-checker-options.json')

// The Checker SPI (@stryker-mutator/api/check, core [04]) the returned instance implements — `group` optional; this plugin implements it for batching.
declare class TypescriptChecker implements Checker {   // internal — the createTypescriptChecker return type, never a public import
  init(): Promise<void>                                            // boot the TS compiler, dry-run the unmutated project
  check(mutants: Mutant[]): Promise<Record<string, CheckResult>>   // per-mutant-id compile verdict
  group(mutants: Mutant[]): Promise<string[][]>                    // batch type-independent mutants into one tsc pass
}
```

## [02]-[CHECK_CONTRACT]

[PUBLIC_TYPE_SCOPE]: the `@stryker-mutator/api/check` verdict algebra — a two-arm discriminated union on `CheckStatus`. `check` returns a `Record<mutantId, CheckResult>`; Stryker maps each arm onto a `MutantStatus` and only the passing arm proceeds to the runner.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]         | [CAPABILITY]                                                                                                                      |
| :-----: | :------------- | :-------------------- | :-------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Checker`      | interface             | `init` + `check` + optional `group`; the plugin implements all three                                                              |
|  [02]   | `CheckResult`  | discriminated union   | `PassedCheckResult \| FailedCheckResult`, keyed by `status`                                                                       |
|  [03]   | `CheckStatus`  | enum                  | `Passed = "passed"` \| `CompileError = "compileError"`                                                                            |
|  [04]   | `Mutant`       | interface             | id + location + `mutatorName` + `replacement` + coverage — the check input                                                        |
|  [05]   | `MutantStatus` | union (report-schema) | the receipt vocabulary owned by `stryker-mutator-core.md` [03]; `CompileError` here removes the mutant from the score denominator |

```ts contract
// check-result.d.ts — the pre-filter verdict; a CompileError carries the tsc `reason` for the report.
type CheckResult = PassedCheckResult | FailedCheckResult
interface PassedCheckResult { status: CheckStatus.Passed }
interface FailedCheckResult { status: CheckStatus.CompileError; reason: string }
enum CheckStatus { Passed = "passed", CompileError = "compileError" }

// A CompileError verdict flows to this schema status (the canonical receipt union — stryker-mutator-core.md [03]); INVALID, out of the score denominator (not Survived, not Killed):
type MutantStatus = "Killed" | "Survived" | "NoCoverage" | "CompileError" | "RuntimeError" | "Timeout" | "Ignored" | "Pending"
```

## [03]-[CONFIG_AS_DATA]

The plugin has NO imperative surface — it is four `stryker.config` rows plus one nested option bag. `checkers` activates it; the rest are the compile context. `stryker.config.json` carries these as data on the one `PartialStrykerOptions` object, never as code. `tsconfigFile`, `checkerNodeArgs`, and `disableTypeChecks` are CORE `StrykerOptions` fields the checker reads; `typescriptChecker` is the plugin-owned bag validated by `strykerValidationSchema`.

| [INDEX] | [CONFIG_ROW]                                          | [OWNER] | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------------------- | :------ | :---------------------------------------------------------------------- |
|  [01]   | `checkers: string[]`                                  | core    | `["typescript"]` activates this plugin in the pre-filter phase          |
|  [02]   | `tsconfigFile: string`                                | core    | the project tsconfig the in-memory compiler loads (follows references)  |
|  [03]   | `checkerNodeArgs: string[]`                           | core    | node args for the checker child process (heap for large graphs)         |
|  [04]   | `disableTypeChecks: boolean \| string`                | core    | glob of files whose `// @ts-nocheck` is injected so mutants compile     |
|  [05]   | `typescriptChecker.prioritizePerformanceOverAccuracy` | plugin  | `true` = coarser grouping / faster; `false` = one-mutant groups / exact |

```ts contract
// The generated plugin option bag (src-generated/typescript-checker-options.d.ts) — the ONLY plugin-owned field.
interface TypescriptCheckerPluginOptions {
  typescriptChecker: { prioritizePerformanceOverAccuracy: boolean }
}
// stryker.config.json carries the checker rows on the ONE PartialStrykerOptions (core [02]; merged example: stryker-mutator-vitest-runner.md [03]):
const checkerRows = {
  checkers: ["typescript"],
  tsconfigFile: "tsconfig.json",
  typescriptChecker: { prioritizePerformanceOverAccuracy: true },
} satisfies PartialStrykerOptions & TypescriptCheckerPluginOptions
```

[GROUPING_LAW] — `group()` is the performance mechanism, not a fixed batch size: it partitions mutants so type-INDEPENDENT mutants share one compiler pass and type-COUPLED mutants (same file / referenced module) split into separate passes that cannot mask each other's errors. `prioritizePerformanceOverAccuracy: true` widens groups (fewer, larger passes); `false` narrows to singleton groups (a pass per mutant — exact attribution, slower). The knob is the accuracy/latency trade, parameterized — never a hand-tuned batch count.

## [04]-[INTEGRATION]

[STACK: `typescript-checker` + `vitest-runner` = the checker→runner pipeline] — the two admitted Stryker plugins are one pipeline, not two features. Stryker runs `check()` first: `CheckStatus.Passed` mutants advance to `vitest-runner` `mutantRun`; `CheckStatus.CompileError` mutants short-circuit to `MutantStatus.CompileError` and never reach the runner. The checker's `tsconfigFile` MUST point at the SAME `tsconfig` the folder's `@effect/vitest` specs compile under, so a mutant that type-checks here is exactly a mutant the runner can execute. See `stryker-mutator-vitest-runner.md` [02] for the kill-execution half.

[STACK: config-as-data in `stryker.config.json`] — the checker contributes rows to the one declarative config object `stryker.config.json` owns; the `MutationScoreThresholds` (`{ high, low, break }`) and `mutate` glob live beside them (documented in `stryker-mutator-vitest-runner.md` [03]). This mirrors the branch's config-as-data doctrine: a new checker context is a data row on the shared options object, never a new mechanism — the same shape `@types/k6` uses for load thresholds (`types-k6.md` [03]).

[STACK: assay `test --mutation` rail] — `uv run python -m tools.assay test run --mutation changed|full --typescript` drives `@stryker-mutator/core`, which loads this plugin from `plugins`/`checkers`. `--mutation changed` maps changed `.ts` files to the Stryker `--mutate <glob>`; the checker then compile-filters that scoped mutant set before the runner executes it against an 0.80-class kill floor. The checker keeps that floor honest — compile-error mutants are out of the denominator, so the ratio reflects only test-killable mutants.

## [05]-[RAIL_LAW]

- Owns: the pre-runner compile-validity gate — in-memory TypeScript type-checking of each mutant group, and the `CompileError` classification that excludes type-invalid mutants from the mutation score.
- Accept: `checkers: ["typescript"]` + a `tsconfigFile` matching the spec compile context; `typescriptChecker.prioritizePerformanceOverAccuracy` as the accuracy/latency knob; `disableTypeChecks` for files a mutant must be allowed to break; `checkerNodeArgs` for heap on large project graphs.
- Reject: a `tsconfigFile` that diverges from the `@effect/vitest` spec tsconfig (mutants that pass the checker but fail to load in the runner — a wasted execution); importing any symbol from this package into source (it is a config row — the `tests/typescript/_architecture` suite bans runtime import); treating a `CompileError` mutant as a `Survived` gap (it is out of scope by construction, not an untested path).
- Boundary: this is a Checker plugin — it renders a `CheckResult`, never a test verdict; the `MutantRunResult` (Killed/Survived/Timeout) is `vitest-runner` territory. The checker sees no test, only types; a mutant that compiles yet is behaviorally dead is the runner's `Survived`, not the checker's concern.
