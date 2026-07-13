# [vitest] — the dev-plane spec runner core the harness lanes and gauges drive

[PACKAGE_SURFACE]:
- package: `vitest` · version `4.1.9` · license `MIT`
- module: ESM (`type: module`); one barrel per concern — `.` (test API), `./config` (config builders), `./node` (programmatic runner + every config-option type), `./reporters`, `./runners`, `./snapshot`, `./browser`, `./environments`, `./worker`, `./globals` (ambient `types`). No deep-`dist` reach; the config-option interfaces re-export from `./node`.
- asset: `dist/index.d.ts` (test API) · `dist/config.d.ts` (`defineConfig`/`defineProject`) · `dist/node.d.ts` (programmatic `Vitest` + `CoverageOptions`/`BrowserConfigOptions`/reporters); bin `vitest`.
- runtime: Node `^20 || ^22 || >=24`; executes on Vite — `vite ^6 || ^7 || ^8` is the one REQUIRED peer (the runner IS a Vite plugin graph). Every other peer is optional and admitted à la carte: `@vitest/{browser-playwright,coverage-v8,ui}` `4.1.9`, `happy-dom`, `jsdom`, `@opentelemetry/api ^1.9`, `@edge-runtime/vm`.
- plane: `plane:dev` — the SPEC_RUNNER core; the `tests/typescript/_architecture` suite asserts no `plane:runtime` graph imports it.
- rail: spec execution — the runner every folder's specs and every gauge terminate in.

`vitest` is the substrate the whole TS spec estate stands on: `@effect/vitest` is a thin binding that re-exports this entire surface (`expect`/`describe`/`vi`/lifecycle) and adds `it.effect`/`it.layer`/`it.prop`, so an effect spec never imports raw `test`/`expect` — yet the config, coverage, browser, reporter, and programmatic surfaces ARE vitest's, and this catalog owns them because the gauges compose on them. The v4 surface below is authoritative for `4.1.9`: the `workspace` config is GONE (replaced by `projects` + `defineProject`); per-provider coverage option types collapsed into one `provider`-discriminated `CoverageOptions` (see `vitest-coverage-v8.md`); the browser provider is an imported `playwright()` function, not a string (see `vitest-browser-playwright.md`); and v4 adds `aroundAll`/`aroundEach` hooks, `recordArtifact` test annotations, `TestTags`, `vi.mockObject`, tinybench `bench`, the Reported-Tasks API (`TestModule`/`TestCase`/`TestSuite`), and the `AgentReporter`.

## [01]-[TEST_API]

[ENTRYPOINT_SCOPE]: the collector + assertion surface (re-exported from `@vitest/runner` + `@vitest/expect`) — specs reach all of it THROUGH `@effect/vitest`, never a raw import in an effect spec.

| [INDEX] | [SYMBOL]                                            | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `test` (`it`) · `TestAPI`                           | collector      | chainable modifiers (roster [01])                         |
|  [02]   | `describe` (`suite`) · `SuiteAPI`                   | grouping       | same modifiers; nests; `TestContext`/`TestOptions` config |
|  [03]   | `beforeEach`/`afterEach`/`beforeAll`/`afterAll`     | lifecycle      | teardown returned from a `before*` hook; per-hook timeout |
|  [04]   | `aroundEach`/`aroundAll`                            | v4 wrap hook   | one hook: setup AND teardown around a test / the suite    |
|  [05]   | `onTestFailed`/`onTestFinished`                     | in-test hook   | register cleanup/diagnostics from a running test          |
|  [06]   | `recordArtifact` · `TestArtifact`/`TestAnnotation`  | v4 annotation  | attach files/notes to a test — surfaces in reporters + UI |
|  [07]   | `bench` · `BenchmarkAPI`/`BenchOptions`/`BenchTask` | tinybench      | micro-benchmark collector; `BenchmarkReporter` folds it   |
|  [08]   | `expect`/`createExpect`/`assert`/`should`           | assertion      | chai+jest matchers; `expect.soft`/`expect.poll`           |
|  [09]   | `assertType`/`expectTypeOf` (`expect-type`)         | type assertion | compile-time assertions the `typecheck` pool runs         |
|  [10]   | `inject<K extends keyof ProvidedContext>(key)`      | injection      | typed value from `globalSetup`/`provide`, cross-worker    |
|  [11]   | `Snapshots` · `SnapshotSerializer`                  | snapshot       | composable custom matchers incl. `toMatchDomainSnapshot`  |
|  [12]   | `TestTags` · `TestTagDefinition`                    | v4 tagging     | declarative test tags for filtered runs / reporting       |

- [01]-[MODIFIERS]: `.skip`/`.only`/`.each`/`.concurrent`/`.sequential`/`.fails`/`.skipIf`/`.runIf`/`.extend`.

```ts signature
// Collectors + hooks re-exported from '@vitest/runner'; bench from tinybench; inject is the typed globalSetup channel.
declare const test: TestAPI; declare const describe: SuiteAPI    // aliases: it, suite
declare function aroundEach(fn: (test: TaskPopulated, use: () => Promise<void>) => Awaitable<void>): void  // v4 — setup+teardown in one
declare function recordArtifact(artifact: TestArtifact): void     // v4 — attach an artifact to the running test
declare const bench: BenchmarkAPI                                 // tinybench-backed; folds a Benchmark result
declare function inject<T extends keyof ProvidedContext & string>(key: T): ProvidedContext[T]
```

## [02]-[VI_UTILITY]

`vi` (`vitest`) is one `VitestUtils` object owning timers, module/object mocking, and global/env stubbing — a single surface, discriminated by method, never a helper family. Effect specs prefer `TestClock`/`TestRandom` (from `@effect/vitest`'s `TestServices`) over `vi`'s timers; `vi` owns module/global mocking, which `TestServices` does not.

| [INDEX] | [SURFACE]                                                      | [FAMILY]       | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `useFakeTimers`/`advanceTimersByTime`/`runAllTimers`(+`Async`) | fake timers    | virtual time; `setSystemTime`/`getMockedSystemTime`   |
|  [02]   | `setTimerTickMode("manual"\|"nextTimerAsync"\|"interval")`     | v4 tick mode   | auto-advance policy for fake timers                   |
|  [03]   | `fn`/`spyOn`/`mocked`/`isMockFunction` · `Mock`/`MockInstance` | spy            | typed spies; `mocked<T>` narrows a mocked module      |
|  [04]   | `mockObject(value, opts?)`                                     | v4 deep mock   | deep-mock methods/props (`{ spy: true }` passthrough) |
|  [05]   | `mock`/`unmock`/`doMock`/`doUnmock`/`hoisted`                  | module mock    | `doMock` → `Disposable`; `hoisted` pre-import factory |
|  [06]   | `importActual`/`importMock`                                    | partial mock   | unwrap the real module inside a `mock` factory        |
|  [07]   | `stubGlobal`/`stubEnv`/`unstubAllGlobals`/`unstubAllEnvs`      | stub           | scoped global/`import.meta.env` overrides             |
|  [08]   | `waitFor`/`waitUntil` · `defineHelper`                         | async wait     | poll to a deadline; `defineHelper` re-anchors stacks  |
|  [09]   | `setConfig`/`resetConfig`/`resetModules`                       | runtime config | mutate `RuntimeOptions` mid-run; `resetModules`       |

```ts signature
interface VitestUtils {
  useFakeTimers(config?: FakeTimerInstallOpts): VitestUtils; advanceTimersByTime(ms: number): VitestUtils
  setSystemTime(time: number | string | Date): VitestUtils; setTimerTickMode(mode: "manual" | "nextTimerAsync"): VitestUtils
  fn: typeof fn; spyOn: typeof spyOn; mockObject<T>(v: T, o?: ModuleMockOptions): MaybeMockedDeep<T>   // v4
  mock(path: string, factory?: ModuleMockFactoryWithHelper | ModuleMockOptions): void
  doMock(path: string, factory?: ModuleMockFactoryWithHelper): Disposable                              // v4 — Disposable
  stubEnv<T extends string>(name: T, value: string | undefined): VitestUtils; unstubAllEnvs(): VitestUtils
  waitFor<T>(cb: () => T | Promise<T>, o?: number | WaitForOptions): Promise<T>; defineHelper<F extends (...a: any) => any>(fn: F): F
}
declare const vi: VitestUtils
```

## [03]-[CONFIG]

[ENTRYPOINT_SCOPE]: `vitest/config` — the config builders. The `test` key is module-augmented onto Vite's `UserConfig`, so ONE `defineConfig` owns Vite + Vitest; a monorepo lists sub-projects via `defineProject` (v4 — the retired `workspace` file).

| [INDEX] | [SYMBOL]                                  | [FAMILY]        | [CAPABILITY]                                                               |
| :-----: | :---------------------------------------- | :-------------- | :------------------------------------------------------------------------- |
|  [01]   | `defineConfig(config)`                    | config builder  | 5 overloads (object/promise/env-fn); injects `test?: InlineConfig`         |
|  [02]   | `defineProject(config)`                   | project builder | v4 — one project in a `test.projects[]` matrix; `TestProjectConfiguration` |
|  [03]   | `mergeConfig` (from `vite`)               | config merge    | deep-merge a base config with an override                                  |
|  [04]   | `configDefaults`/`coverageConfigDefaults` | defaults        | baselines; `defaultInclude`/`defaultExclude`/`defaultBrowserPort` (63315)  |
|  [05]   | `configureVitest` (plugin hook)           | v4 plugin hook  | a Vite plugin mutates resolved Vitest config via `VitestPluginContext`     |
|  [06]   | `InlineConfig` fields                     | config surface  | the whole run policy (roster [06]); option types live on `./node`          |

- [06]-[INLINE_CONFIG]: `pool`/`environment`/`coverage`/`browser`/`typecheck`/`projects`/`setupFiles`/`globalSetup`/`sequence`/`reporters`.

```ts signature
declare function defineConfig(config: UserConfig): UserConfig                              // + fn/promise overloads
declare function defineProject(config: UserWorkspaceConfig): UserWorkspaceConfig           // v4 — replaces `workspace`
declare const coverageConfigDefaults: Required<Pick<CoverageOptions, FieldsWithDefaultValues>>
declare const configDefaults: Readonly<{ environment: "node"; include: string[]; exclude: string[]; isolate: boolean; /* … */ }>
declare module "vite" { interface UserConfig { test?: InlineConfig }; interface Plugin { configureVitest?: HookHandler<(c: VitestPluginContext) => void> } }
```

## [04]-[PROGRAMMATIC_AND_REPORT]

[ENTRYPOINT_SCOPE]: `vitest/node` — the programmatic runner, the Reported-Tasks tree, the reporters, and every config-option TYPE the design's `defineConfig` binds. A CI gauge boots the runner here, reads the typed task tree, and folds a reporter — never scrapes stdout.

| [INDEX] | [SYMBOL]                                                            | [FAMILY]          | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------ | :---------------- | :--------------------------------------------- |
|  [01]   | `startVitest`/`createVitest`/`parseCLI`/`resolveConfig`             | boot              | programmatic run; `Vitest` + `VitestRunMode`   |
|  [02]   | `TestModule`/`TestSuite`/`TestCase`/`TestCollection`                | reported tasks    | `TestResult`/`TestState`/`TestDiagnostic` tree |
|  [03]   | `TestProject`/`TestSpecification`                                   | project handle    | per-project config + `projects[]` face         |
|  [04]   | `Reporter`/`BuiltinReporters`/`ReportersMap`/`BaseReporter`         | reporter contract | `reporters` vocabulary + custom-reporter base  |
|  [05]   | builtin `*Reporter` classes                                         | builtin reporters | roster [05]; `AgentReporter` = AI-agent stream |
|  [06]   | pool workers · `BaseSequencer`                                      | pool workers      | roster [06]; `PoolOptions`; shards specs       |
|  [07]   | `BuiltinEnvironment = "node"\|"jsdom"\|"happy-dom"\|"edge-runtime"` | environment       | `environment` axis; `EnvironmentOptions`       |
|  [08]   | config-option types (roster [08])                                   | config types      | shapes `InlineConfig` resolves to (`.api`)     |

- [05]-[REPORTERS]: `Default`/`Dot`/`Verbose`/`JUnit`/`Json`/`Tap`/`TapFlat`/`GithubActions`/`HangingProcess`/`Minimal`/`Agent` `Reporter` (`html` ships in `@vitest/ui`).
- [06]-[POOL_WORKERS]: `Forks`/`Threads`/`VmForks`/`VmThreads`/`Typecheck` `PoolWorker` (`@experimental`).
- [08]-[CONFIG_TYPES]: `CoverageOptions`/`BrowserConfigOptions`/`TypecheckConfig`/`ApiConfig`.

```ts signature
declare function startVitest(mode: VitestRunMode, cliFilters?: string[], options?: CliOptions, viteOverrides?: ViteUserConfig, vitestOptions?: VitestOptions): Promise<Vitest>
declare function createVitest(mode: VitestRunMode, options: CliOptions, ...): Promise<Vitest>
declare function parseCLI(argv: string | string[], config?: CliParseOptions): { filter: string[]; options: CliOptions }
// Reported-Tasks read model (post-run): TestModule ⊇ TestSuite ⊇ TestCase, each carrying TestResult (passed|failed|skipped) + TestDiagnostic.
interface TestRunResult { testModules: TestModule[]; unhandledErrors: unknown[] }
```

## [05]-[INTEGRATION]

[STACK: `vitest` ← `@effect/vitest`] — the binding. `@effect/vitest` (this tier, `effect-vitest.md`) re-exports this whole surface (`export * from "vitest"`) and layers `it.effect`/`it.scoped`/`it.layer`/`it.prop`/`it.flakyTest` over the collectors, running each body as an `Effect` under `TestServices` (`TestClock`/`TestRandom`). Every spec is a vitest test wearing an effect body; `addEqualityTesters()` swaps vitest's `toEqual` for `Equal.equals`. Peer note: `@effect/vitest@0.29.0` declares `vitest@^3.2.0`, one major behind the admitted `vitest@4.1.9`; the binding resolves and runs, but the declared range trails the runtime.

[STACK: `vitest` + `@vitest/coverage-v8` + `@stryker-mutator/vitest-runner`] — the gauge stack. `test.coverage.provider: 'v8'` resolves the `@vitest/coverage-v8` `CoverageProviderModule` (see `vitest-coverage-v8.md`); `test.coverage.thresholds` express the pass gate as data. `@stryker-mutator/vitest-runner` reuses THIS runner in-process so mutation testing runs the identical spec set — the root `vitest.config.ts` owns the coverage thresholds, `stryker.config.json` the mutation thresholds.

[STACK: `vitest` + `@vitest/browser-playwright` + `happy-dom`/`jsdom`] — the DOM-lane axis. `test.environment` selects a simulated DOM (`happy-dom` fast / `jsdom` fidelity — `BuiltinEnvironment`) for a Node-hosted DOM spec, while `test.browser.provider: playwright()` runs the SAME spec in a real browser (see `vitest-browser-playwright.md`). One `environment` axis, three modalities; the design picks per folder.

[STACK: `vitest` + `@vitest/ui`] — inspection. `test.ui: true` / `--ui` mounts the `@vitest/ui` dashboard plugin; `reporters: ['html']` (its `HtmlReporter`) writes the durable static report — both read the Reported-Tasks tree above (see `vitest-ui.md`).

[STACK: `vitest` + `fast-check`] — property law. `it.prop`/`it.effect.prop` (`@effect/vitest`) accept `fast-check` `Arbitrary`s beside `Schema`s; the `_testkit` law/arbitrary source folds them into the three reusable law combinators (see `fast-check.md`). The `fastCheck` option forwards `FC.Parameters` (seed, `numRuns`) into the vitest test.

## [06]-[RAIL_LAW]

- Owns: the spec-run engine — the collector/hook/assertion API, `vi` timers/mocks/stubs, the `defineConfig`/`defineProject` config vocabulary, the programmatic `Vitest` + Reported-Tasks tree, the builtin reporters and pool workers, and every config-option TYPE (`CoverageOptions`/`BrowserConfigOptions`/`EnvironmentOptions`/`PoolOptions`) the provider packages key into.
- Accept: `@effect/vitest`'s `it.effect`/`it.layer`/`it.prop` as the spec entry (never raw `test`); `defineProject` for the projects matrix; `test.coverage`/`test.browser`/`test.environment`/`test.pool` as data; the programmatic `startVitest`/Reported-Tasks for a CI gauge; `vi` for module/global mocking that `TestServices` cannot express.
- Reject: raw `test(async () => await Effect.runPromise(...))` (loses `TestServices`/typed `Exit` — use `it.effect`); the retired `workspace` config (use `test.projects` + `defineProject`); a per-provider coverage-options type (deprecated — one `CoverageOptions`, `provider`-discriminated); `vi.useFakeTimers` where `TestClock.adjust` is deterministic; any import from a `plane:runtime` folder — dev subpath only.
- Boundary: the required Vite peer means the run graph is a Vite build — Vite plugins/resolve/aliases apply, and the `configLoader` (`bundle`/`runner`) governs config evaluation. `pool` workers and `TypecheckPoolWorker` are `@experimental`; the `AgentReporter`/`recordArtifact`/`configureVitest`/`instrumenter` surfaces are v4-current and may still shift. `expect.poll`/`waitFor` are wall-clock, not virtual — a deterministic effect assertion uses `TestClock`, not a real poll.
