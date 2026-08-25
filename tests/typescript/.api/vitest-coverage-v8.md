# [TS_TESTS_API_VITEST_COVERAGE_V8]

`@vitest/coverage-v8` is a PROVIDER MODULE, not a config surface: no spec imports it; the design sets `test.coverage.provider = 'v8'` (the default) and vitest resolves this package's default `CoverageProviderModule`, whose `getProvider()` yields a `V8CoverageProvider`; the entire configuration vocabulary — `CoverageOptions`, `Thresholds`, `CoverageReporter` — is vitest's, and this catalog documents that vocabulary because the root `vitest.config.ts` composes the pass gate ("coverage thresholds as data") on it, with the provider internals the design must understand; the v4 collapse is load-bearing: `CoverageV8Options`, `CoverageIstanbulOptions`, `BaseCoverageOptions`, and `CustomProviderOptions` are ALL `@deprecated`, empty `extends CoverageOptions {}` — there is ONE options type, discriminated by `provider: "v8" | "istanbul" | "custom"`. Splitting config by v8-vs-istanbul is a phantom; the difference is one string.

## [01]-[PROVIDER_MODULE]

[PUBLIC_TYPE_SCOPE]: the resolved provider — one class owns collect → remap → report → threshold; configuration is the whole contract — no spec calls it.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]            | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------ | :----------------------- | :-------------------------------------------------------------- |
|  [01]   | `default: CoverageProviderModule`           | module (`.`)             | `getProvider()` + `start/take/stopCoverage` worker hooks        |
|  [02]   | `V8CoverageProvider` (`./provider`)         | class                    | `extends BaseCoverageProvider`; `name:"v8"`                     |
|  [03]   | `BaseCoverageProvider` (from `vitest/node`) | abstract base            | shared report/threshold engine both v8 + istanbul extend        |
|  [04]   | `CoverageProvider`/`CoverageProviderModule` | contract (`vitest/node`) | implemented by a `customProviderModule` for a new provider row  |
|  [05]   | `ScriptCoverageWithOffset`                  | type                     | `Profiler.ScriptCoverage` + `startOffset` (raw V8 source frame) |

```ts
declare const mod: CoverageProviderModule; export { mod as default }
interface CoverageProviderModule { getProvider(): CoverageProvider | Promise<CoverageProvider>;  }
declare class V8CoverageProvider extends BaseCoverageProvider implements CoverageProvider {
  name: "v8"
  generateCoverage({ allTestsRun }: ReportContext): Promise<CoverageMap>
  generateReports(coverageMap: CoverageMap, allTestsRun?: boolean): Promise<void>
  parseConfigModule(configFilePath: string): Promise<ProxifiedModule<any>>
}
```

[PUBLIC_TYPE_SCOPE]: the shared report/threshold engine `V8CoverageProvider` inherits — the code the gauge's "thresholds as data" runs through.

```ts
declare class BaseCoverageProvider {
  readonly name: "v8" | "istanbul"; options: ResolvedCoverageOptions
  reportCoverage(coverageMap: unknown, ctx: ReportContext): Promise<void>
  reportThresholds(coverageMap: CoverageMap, allTestsRun?: boolean): Promise<void>
  updateThresholds(args: { thresholds: ResolvedThreshold[]; onUpdate: () => void; configurationFile: unknown }): Promise<void>
  getUntestedFiles(testedFiles: string[]): Promise<string[]>; isIncluded(filename: string, root?: string): boolean
}
```

## [02]-[COVERAGE_OPTIONS]

`test.coverage` is ONE `CoverageOptions` object — a `provider` discriminant with include/report/threshold policy. This is the config the design's `defineConfig` binds; `coverageConfigDefaults` (from `vitest/config`) is the spread-in baseline.

| [INDEX] | [FIELD]                    | [SHAPE]                                 | [CAPABILITY]                                                   |
| :-----: | :------------------------- | :-------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `provider`                 | `"v8"\|"istanbul"\|"custom"`            | discriminant; `'v8'` default loads this package                |
|  [02]   | `enabled`                  | `boolean`                               | `false` default; `--coverage` overrides                        |
|  [03]   | `include`/`exclude`        | `string[]` globs                        | `exclude` applied after `include`, both source-side            |
|  [04]   | `reporter`                 | `Arrayable<CoverageReporter>` \| tuples | `text`/`html`/`html-spa`/`lcov`/`json`/`clover`/`cobertura`…   |
|  [05]   | `reportsDirectory`         | `string`                                | report output location                                         |
|  [06]   | `htmlDir`                  | `string`                                | auto-set for `html`/`lcov` reporters                           |
|  [07]   | `reportOnFailure`          | `boolean`                               | emit reports even when specs fail                              |
|  [08]   | `thresholds`               | `Thresholds \| { [glob]: … }`           | the pass gate — see [03]                                       |
|  [09]   | `clean`/`cleanOnRerun`     | `boolean`                               | wipe reports before a run / on watch rerun                     |
|  [10]   | `skipFull`/`allowExternal` | `boolean`                               | hide full-cover files / include files outside root             |
|  [11]   | `excludeAfterRemap`        | `boolean`                               | re-apply excludes after AST remap                              |
|  [12]   | `instrumenter`             | `(opts) => CoverageInstrumenter`        | v4 pluggable instrumenter (oxc/SWC) into the istanbul pipeline |
|  [13]   | `customProviderModule`     | `string`                                | module a `provider:'custom'` loads — the extension point       |

```ts
interface CoverageOptions {
  provider?: "v8" | "istanbul" | "custom"; enabled?: boolean
  include?: string[]; exclude?: string[]; reportsDirectory?: string
  reporter?: Arrayable<CoverageReporter> | (CoverageReporter | [CoverageReporter] | CoverageReporterWithOptions)[]
  thresholds?: Thresholds | ({ [glob: string]: Pick<Thresholds, 100 | "statements" | "functions" | "branches" | "lines"> } & Thresholds)
  reportOnFailure?: boolean; skipFull?: boolean; allowExternal?: boolean; excludeAfterRemap?: boolean; htmlDir?: string
  instrumenter?: (options: InstrumenterOptions) => CoverageInstrumenter
  customProviderModule?: string
}
type CoverageReporter = keyof ReportOptions | (string & {})
```

## [03]-[THRESHOLDS]

`Thresholds` is the gauge's core: coverage as a numeric pass gate, per-metric and optionally per-glob, with `autoUpdate` ratcheting the floor. This is what "coverage thresholds as data" means — a config row in the root `vitest.config.ts`, not a script.

```ts
interface Thresholds {
  100?: boolean
  perFile?: boolean
  autoUpdate?: boolean | ((newThreshold: number) => number)
  statements?: number; functions?: number; branches?: number; lines?: number
}
```

## [04]-[INTEGRATION]

[STACK: `@vitest/coverage-v8` ← `vitest` (`provider:'v8'`)] — the resolution seam; the design never imports this package; `defineConfig({ test: { coverage: { provider: 'v8', thresholds } } })` makes vitest load the default `CoverageProviderModule`, and `BaseCoverageProvider.reportThresholds` sets exit 1 when a threshold is unmet — the CI gate. `coverageConfigDefaults` (`vitest/config`) supplies the baseline the design overrides.

[STACK: `@vitest/coverage-v8` `Thresholds` + `@stryker-mutator/core` `MutationScoreThresholds` (the mutation/coverage gauge)] — the two-floor "thresholds as data" gate, the mirror of `stryker-mutator-core.md`'s reciprocal STACK. Coverage thresholds are the FIRST gate (did the specs execute the code); the Stryker mutation-score `break` is the SECOND (did they assert on it), and `@stryker-mutator/vitest-runner` runs BOTH floors over the same vitest spec set; the two floors live as data — coverage `Thresholds` (`statements`/`functions`/`branches`/`lines`) in the root `vitest.config.ts`, `MutationScoreThresholds { high, low, break }` in `stryker.config.json` (`stryker-mutator-core.md`). High coverage beside a low mutation score is the exact defect the two-gate stack catches.

[STACK: `@vitest/coverage-v8` + html reporter] — the report seam. `reporter: ['html']` (coverage) writes an istanbul HTML tree under `htmlDir`; the `reporters: ['html']` test report embeds it, so a run's coverage is inspectable beside its spec tree. `lcov` output feeds external coverage services.
