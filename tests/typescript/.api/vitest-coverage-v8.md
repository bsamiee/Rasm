# [@vitest/coverage-v8] — V8 coverage provider feeding the gauge coverage thresholds

[PACKAGE_SURFACE]:
- package: `@vitest/coverage-v8` · version `4.1.9` · license `MIT`
- module: ESM (`type: module`); subpaths `.` (the default `CoverageProviderModule`), `./provider` (the `V8CoverageProvider` class), `./browser` (the browser-lane instrumenter). Peers: `vitest 4.1.9` (required), `@vitest/browser 4.1.9` (optional — browser-mode coverage).
- asset: `dist/index.d.ts` (`declare const mod: CoverageProviderModule; export default mod`) · `dist/provider.d.ts` (`V8CoverageProvider`); the coverage CONFIG vocabulary is vitest's (`CoverageOptions` on `vitest/node`), keyed by `provider: 'v8'`.
- runtime: Node V8 native coverage via `node:inspector` `Profiler.ScriptCoverage`; v4 remaps to source through AST (`ast-v8-to-istanbul`), NOT source-map guesswork; reports via `istanbul-lib-coverage`; `thresholds.autoUpdate` rewrites the config file through `magicast`.
- plane: `plane:dev` — the coverage half of the mutation/coverage gauge; thresholds live as data — coverage in the root `vitest.config.ts`, mutation in `.config/stryker.config.json`.
- rail: coverage-provider — a resolved module, never a spec import.

`@vitest/coverage-v8` is a PROVIDER MODULE, not a config surface: no spec imports it; the design sets `test.coverage.provider = 'v8'` (the default) and vitest resolves this package's default `CoverageProviderModule`, whose `getProvider()` yields a `V8CoverageProvider`. The entire configuration vocabulary — `CoverageOptions`, `Thresholds`, `CoverageReporter` — is vitest's, and this catalog documents that vocabulary because the root `vitest.config.ts` composes the pass gate ("coverage thresholds as data") on it, plus the provider internals the design must understand. The v4 collapse is load-bearing: `CoverageV8Options`, `CoverageIstanbulOptions`, `BaseCoverageOptions`, and `CustomProviderOptions` are ALL `@deprecated`, empty `extends CoverageOptions {}` — there is ONE options type, discriminated by `provider: "v8" | "istanbul" | "custom"`. A v8-vs-istanbul split in config is a phantom; the difference is one string.

## [01]-[PROVIDER_MODULE]

[PUBLIC_TYPE_SCOPE]: the resolved provider — one class owns collect → remap → report → threshold; you configure it, never call it.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]     | [CAPABILITY / BOUNDARY]                                              |
| :-----: | :------------------------------------------- | :---------------- | :------------------------------------------------------------------- |
|  [01]   | `default: CoverageProviderModule`            | module (`.`)      | `getProvider()` + `start/take/stopCoverage` worker hooks — what `provider:'v8'` loads |
|  [02]   | `V8CoverageProvider` (`./provider`)          | class             | `extends BaseCoverageProvider implements CoverageProvider`; `name:"v8"` |
|  [03]   | `BaseCoverageProvider` (from `vitest/node`)  | abstract base     | shared report/threshold engine both v8 + istanbul extend             |
|  [04]   | `CoverageProvider`/`CoverageProviderModule`  | contract (`vitest/node`) | the interface a `customProviderModule` implements to add a provider row |
|  [05]   | `ScriptCoverageWithOffset`                   | type              | `Profiler.ScriptCoverage` + `startOffset` — the raw V8 source frame  |

```ts contract
// index.d.ts — the default export is the module vitest loads for provider:'v8'; you never construct it.
declare const mod: CoverageProviderModule; export { mod as default }
interface CoverageProviderModule { getProvider(): CoverageProvider | Promise<CoverageProvider>; /* + start/take/stopCoverage worker hooks */ }
// provider.d.ts — V8 native coverage → AST remap → istanbul reports; the private steps are the v4 pipeline.
declare class V8CoverageProvider extends BaseCoverageProvider implements CoverageProvider {
  name: "v8"
  generateCoverage({ allTestsRun }: ReportContext): Promise<CoverageMap>   // from node:inspector Profiler.ScriptCoverage
  generateReports(coverageMap: CoverageMap, allTestsRun?: boolean): Promise<void>
  parseConfigModule(configFilePath: string): Promise<ProxifiedModule<any>>  // magicast — rewrites thresholds on autoUpdate
}
```

[PUBLIC_TYPE_SCOPE]: the shared report/threshold engine `V8CoverageProvider` inherits — the code the gauge's "thresholds as data" runs through.

```ts contract
// vitest/node — BaseCoverageProvider owns the threshold gate; subclasses supply collection + remap only.
declare class BaseCoverageProvider {
  readonly name: "v8" | "istanbul"; options: ResolvedCoverageOptions
  reportCoverage(coverageMap: unknown, ctx: ReportContext): Promise<void>
  reportThresholds(coverageMap: CoverageMap, allTestsRun?: boolean): Promise<void>   // sets exit 1 when unmet
  updateThresholds(args: { thresholds: ResolvedThreshold[]; onUpdate: () => void; configurationFile: unknown }): Promise<void>  // autoUpdate
  getUntestedFiles(testedFiles: string[]): Promise<string[]>; isIncluded(filename: string, root?: string): boolean  // include/exclude glob gate
}
```

## [02]-[COVERAGE_OPTIONS]

`test.coverage` is ONE `CoverageOptions` object — a `provider` discriminant plus include/report/threshold policy. This is the config the design's `defineConfig` binds; `coverageConfigDefaults` (from `vitest/config`) is the spread-in baseline.

| [INDEX] | [FIELD]                                      | [SHAPE]                         | [CAPABILITY / BOUNDARY]                                          |
| :-----: | :------------------------------------------- | :------------------------------ | :--------------------------------------------------------------- |
|  [01]   | `provider`                                   | `"v8"\|"istanbul"\|"custom"`    | the discriminant; `'v8'` default loads THIS package              |
|  [02]   | `enabled`                                     | `boolean`                       | `false` default; `--coverage` overrides                         |
|  [03]   | `include`/`exclude`                          | `string[]` globs                | `exclude` checked after `include`; both source-side             |
|  [04]   | `reporter`                                    | `Arrayable<CoverageReporter>` \| `[name, opts][]` | istanbul reporter roster (`text`/`html`/`html-spa`/`lcov`/`json`/`clover`/`cobertura`…); tuple form carries per-reporter options |
|  [05]   | `reportsDirectory`/`htmlDir`/`reportOnFailure`| `string`/`boolean`              | output location; `htmlDir` is auto-set for `html`/`lcov`; `reportOnFailure` emits even when specs fail |
|  [06]   | `thresholds`                                 | `Thresholds \| { [glob]: … }`   | the pass gate — see [03]                                        |
|  [07]   | `clean`/`cleanOnRerun`/`skipFull`/`allowExternal`/`excludeAfterRemap` | `boolean` | lifecycle + report shaping; `excludeAfterRemap` re-applies excludes after AST remap |
|  [08]   | `instrumenter`                               | `(o: InstrumenterOptions) => CoverageInstrumenter` | v4 experimental — plug a faster instrumenter (oxc/SWC) into the istanbul pipeline |
|  [09]   | `customProviderModule`                       | `string`                        | module path a `provider:'custom'` loads — the extension point   |

```ts contract
interface CoverageOptions {
  provider?: "v8" | "istanbul" | "custom"; enabled?: boolean
  include?: string[]; exclude?: string[]; reportsDirectory?: string
  reporter?: Arrayable<CoverageReporter> | (CoverageReporter | [CoverageReporter] | CoverageReporterWithOptions)[]
  thresholds?: Thresholds | ({ [glob: string]: Pick<Thresholds, 100 | "statements" | "functions" | "branches" | "lines"> } & Thresholds)
  reportOnFailure?: boolean; skipFull?: boolean; allowExternal?: boolean; excludeAfterRemap?: boolean; htmlDir?: string
  instrumenter?: (options: InstrumenterOptions) => CoverageInstrumenter          // v4 experimental — pluggable instrumenter
  customProviderModule?: string                                                  // provider:'custom' loader
}
type CoverageReporter = keyof ReportOptions | (string & {})                       // istanbul reporter names + open string
```

## [03]-[THRESHOLDS]

`Thresholds` is the gauge's core: coverage as a numeric pass gate, per-metric and optionally per-glob, with `autoUpdate` ratcheting the floor. This is what "coverage thresholds as data" means — a config row in the root `vitest.config.ts`, not a script.

```ts contract
interface Thresholds {
  100?: boolean            // set statements/functions/branches/lines all to 100
  perFile?: boolean        // gate each file, not the aggregate
  autoUpdate?: boolean | ((newThreshold: number) => number)   // ratchet the floor up on higher coverage (magicast-rewrites config)
  statements?: number; functions?: number; branches?: number; lines?: number
}
// Per-glob override: tighten a hot path above the global floor — one parameterized map, not a per-file config family.
//   thresholds: { functions: 95, branches: 70, autoUpdate: true, 'src/kernel/**.ts': { lines: 100, statements: 95 } }
```

## [04]-[INTEGRATION]

[STACK: `@vitest/coverage-v8` ← `vitest` (`provider:'v8'`)] — the resolution seam. The design never imports this package; `defineConfig({ test: { coverage: { provider: 'v8', thresholds } } })` makes vitest load the default `CoverageProviderModule`, and `BaseCoverageProvider.reportThresholds` sets exit 1 when a threshold is unmet — the CI gate. `coverageConfigDefaults` (`vitest/config`) supplies the baseline the design overrides.

[STACK: `@vitest/coverage-v8` `Thresholds` + `@stryker-mutator/core` `MutationScoreThresholds` (the mutation/coverage gauge)] — the two-floor "thresholds as data" gate, the mirror of `stryker-mutator-core.md`'s reciprocal STACK. Coverage thresholds are the FIRST gate (did the specs execute the code); the Stryker mutation-score `break` is the SECOND (did they assert on it), and `@stryker-mutator/vitest-runner` runs BOTH floors over the same vitest spec set. The two floors live as data — coverage `Thresholds` (`statements`/`functions`/`branches`/`lines`) in the root `vitest.config.ts`, `MutationScoreThresholds { high, low, break }` in `.config/stryker.config.json` (`stryker-mutator-core.md`). A high coverage number with a low mutation score is the exact defect the two-gate stack catches.

[STACK: `@vitest/coverage-v8` + `@vitest/ui` / html reporter] — the report seam. `reporter: ['html']` (coverage) writes an istanbul HTML tree under `htmlDir`; the `@vitest/ui` dashboard and the `reporters: ['html']` test report (see `vitest-ui.md`) embed it, so a run's coverage is inspectable beside its spec tree. `lcov` output feeds external coverage services.

[STACK: `@vitest/coverage-v8/browser` + `@vitest/browser-playwright`] — the browser-lane seam. When a spec runs in browser mode (see `vitest-browser-playwright.md`), the `./browser` instrumenter collects coverage inside the real browser and folds it into the same `CoverageMap` as the Node lane — one coverage report across both `environment` modalities.

## [05]-[RAIL_LAW]

- Owns: the V8 coverage provider — native `Profiler.ScriptCoverage` collection, AST-to-istanbul remapping, istanbul report generation, and the threshold pass gate (`reportThresholds`/`updateThresholds`); it supplies the module `provider:'v8'` resolves.
- Accept: `test.coverage.provider: 'v8'` (the default); `thresholds` as data — global, per-glob, `perFile`, or `100:true` with `autoUpdate` ratcheting; `reporter` tuples for per-reporter options; `include`/`exclude` globs; `customProviderModule` only to admit a genuinely new provider row.
- Reject: importing this package in a spec (it is resolved, not imported); a per-provider coverage-options type (`CoverageV8Options`/`BaseCoverageOptions` are deprecated empties — use `CoverageOptions`, `provider`-discriminated); hand-rolled coverage math where `Thresholds` is data; a migrator/instrumenter reinvention where the `instrumenter` hook plugs one in; any `plane:runtime` import.
- Boundary: V8 coverage is function/branch-granular from the engine, remapped to source by AST — accurate for transpiled TS but bounded by the source map's fidelity (`excludeAfterRemap` re-applies excludes post-remap). Coverage proves execution, NOT assertion strength — that is the mutation gauge's job; the two are complementary floors, never substitutes. `instrumenter` and `provider:'custom'` are the sanctioned extension points; the built-in v8 path needs no instrumenter.
