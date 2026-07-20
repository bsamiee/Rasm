# [TS_TESTS_API_PLAYWRIGHT_TEST]

[PACKAGE_SURFACE]:
- package: `@playwright/test` · version `1.61.1` · license `Apache-2.0`
- module: dual CJS/ESM (`.js` + `.mjs`) with `exports` map `.` / `./cli` / `./reporter` / `./package.json`; the root is a thin re-export of `playwright/test` → `playwright/types/test` (types resolve through it); the custom-reporter SPI is the separate `@playwright/test/reporter` subpath.
- asset: bundled `playwright` + `playwright-core` engines; the actual browser binaries (chromium / firefox / webkit) install out-of-band via `playwright install` — a runner fact, never a JS dependency on any bundle.
- runtime: node `>=18`; each spec runs in a worker process driving a real browser over CDP/the Playwright protocol.
- plane: `plane:dev` — the browser `E2E_GAUGE` of the `tests/typescript/e2e` home, beside the `@types/k6` load driver; `tests/typescript/_architecture` fences it off every runtime graph.
- rail: browser-e2e / visual-and-aria gauge.

`@playwright/test` is the functional + visual e2e driver the `tests/typescript/e2e` home composes: a `test` runner with worker-scoped browser fixtures, a web-first auto-retrying `expect`, a config-as-code `defineConfig` matrix over `devices`, and a reporter SPI that projects results as data. It drives TWO seams — the standalone runner for full cross-browser e2e, and (via `@vitest/browser-playwright`) the browser PROVIDER under vitest browser mode so a browser-runtime unit spec reuses the same engine; load half of the gauge is k6 (`@types/k6`), the two drivers orthogonal rows on the one `e2e` owner.

## [01]-[ENTRY_SURFACE]

[ENTRYPOINT_SCOPE]: the value exports — everything a spec or config file imports from the barrel.

| [INDEX] | [SYMBOL]       | [TYPE]                               | [CAPABILITY]                                                  |
| :-----: | :------------- | :----------------------------------- | :------------------------------------------------------------ |
|  [01]   | `test`         | `TestType<TestArgs, WorkerArgs>`     | the runner; declares tests + owns fixtures ([02])             |
|  [02]   | `expect`       | `Expect<{}>`                         | web-first auto-retrying assertions ([03])                     |
|  [03]   | `devices`      | keyed descriptor map                 | `devices['Desktop Chrome']` → a `use` preset; the device axis |
|  [04]   | `defineConfig` | 6 overloads → `PlaywrightTestConfig` | config-as-code; right-fold merge of later configs             |
|  [05]   | `mergeTests`   | `MergedTestType<List>`               | compose independent fixture sets into one `test`              |
|  [06]   | `mergeExpects` | `MergedExpect<List>`                 | compose independent matcher sets into one `expect`            |
|  [07]   | `_baseTest`    | `TestType<{}, {}>`                   | the fixture-free base to `.extend` from scratch               |

```ts signature
export const test: TestType<PlaywrightTestArgs & PlaywrightTestOptions, PlaywrightWorkerArgs & PlaywrightWorkerOptions>
export const expect: Expect<{}>
export const devices: { [name: string]: DeviceDescriptor }         // e.g. devices['iPhone 15'] → viewport/userAgent/deviceScaleFactor preset
export function defineConfig(config: PlaywrightTestConfig, ...configs: PlaywrightTestConfig[]): PlaywrightTestConfig
export function defineConfig<T, W>(config: PlaywrightTestConfig<T, W>, ...configs: PlaywrightTestConfig<T, W>[]): PlaywrightTestConfig<T, W>
export function mergeTests<List extends any[]>(...tests: List): MergedTestType<List>
export function mergeExpects<List extends any[]>(...expects: List): MergedExpect<List>
```

## [02]-[TEST_AND_FIXTURES]

`TestType` is ONE dispatch surface — `test(title, body)` with modifier and lifecycle members hung off it, never a family of parallel entrypoints. Its type parameters carry the FIXTURE bag: fixtures are a typed DI rail (`.extend<Fixtures>`), and the built-in fixtures below are SEED ROWS on that rail, not a fixed roster — a custom fixture is a new row, `mergeTests` unions two fixture sets.

```ts signature
interface TestType<TestArgs, WorkerArgs> {
  (title: string, body: (args: TestArgs & WorkerArgs, testInfo: TestInfo) => Promise<void> | void): void
  (title: string, details: TestDetails, body: (args: TestArgs & WorkerArgs, testInfo: TestInfo) => Promise<void> | void): void
  only(title: string, body): void                                                    // + (title, details, body) — declares a focused test
  skip(condition?: boolean, description?: string): void                              // guard: skip() unconditional / skip(cond, desc) / skip(title, body) standalone
  fixme(condition?: boolean, description?: string): void                             // known-broken; same overload family as skip
  slow(condition?: boolean, description?: string): void                              // triples the timeout; guarded
  fail: { (): void; (condition: boolean, description?: string): void }               // expect the current test to fail (callable object)
  describe: { (title: string, cb: () => void): void; only; skip; serial; parallel; configure(o): void; fixme }
  beforeEach; afterEach; beforeAll; afterAll                                          // fixture-aware hooks (title-optional)
  step: { <T>(title: string, body: (step: TestStepInfo) => T | Promise<T>, o?: { box?: boolean; location?: Location; timeout?: number }): Promise<T>; skip(...): Promise<void> }
  extend<T extends {}, W extends {} = {}>(fixtures: Fixtures<T, W, TestArgs, WorkerArgs>): TestType<TestArgs & T, WorkerArgs & W>
  use(fixtures: Fixtures<{}, {}, TestArgs, WorkerArgs>): void                         // scope fixture/option overrides to a describe block
  info(): TestInfo; setTimeout(timeout: number): void
}
// Built-in fixtures = SEED ROWS on the DI rail (destructured in the body); test's params are PlaywrightTestArgs & …TestOptions & PlaywrightWorkerArgs & …WorkerOptions.
interface PlaywrightTestArgs { context: BrowserContext; page: Page; request: APIRequestContext }
interface PlaywrightWorkerArgs { playwright: typeof import('playwright-core'); browser: Browser }
// browserName is a PlaywrightWorkerOptions seed row (not a WorkerArg): { browserName; defaultBrowserType; headless; channel; launchOptions; connectOptions }
```

Page/context CONTROL surfaces the fixture rail composes — the four families a hermetic platform fixture rides:

```ts signature
// page.clock — fake-time control: install() AUTO-ADVANCES from its epoch, so pre-jump ticks carry the page-load offset;
//   callbacks fired by fastForward/pauseAt see the exact target instant — exact-text assertions ride the post-jump surface.
interface Clock {
  install(o?: { time?: number | string | Date }): Promise<void>
  pauseAt(time: number | string | Date): Promise<void>; resume(): Promise<void>
  fastForward(ticks: number | string): Promise<void>; runFor(ticks: number | string): Promise<void>
  setFixedTime(time: number | string | Date): Promise<void>; setSystemTime(time: number | string | Date): Promise<void>
}
// page.routeWebSocket — hermetic WS lanes with no server: the handler IS the peer.
page.routeWebSocket(url: string | RegExp | ((url: URL) => boolean), handler: (ws: WebSocketRoute) => any): Promise<void>
interface WebSocketRoute { onMessage(h: (message: string | Buffer) => any): void; send(message: string | Buffer): void; close(o?: { code?: number; reason?: string }): Promise<void>; connectToServer(): WebSocketRoute }
// HAR record/replay: browserContext.routeFromHAR(har, { url?, update?, notFound? }) — notFound: 'abort' | 'fallback'.
// CDP WebAuthn (chromium-only): the virtual-authenticator ceremony rides a raw CDP session.
const cdp = await context.newCDPSession(page)
await cdp.send('WebAuthn.enable')
const { authenticatorId } = await cdp.send('WebAuthn.addVirtualAuthenticator', { options: { protocol: 'ctap2', transport: 'internal', hasResidentKey: true, hasUserVerification: true, isUserVerified: true, automaticPresenceSimulation: true } })
await cdp.send('WebAuthn.removeVirtualAuthenticator', { authenticatorId })   // ceremony teardown = the refutation seed
```

## [03]-[WEB_FIRST_EXPECT]

`expect` is a SEPARATE assertion library from `@effect/vitest`'s — its matchers AUTO-RETRY against a live browser until they pass or time out, which no synchronous matcher can do. Never mix the two `expect` symbols in one spec file.

```ts signature
type Expect<ExtendedMatchers = {}> = {
  <T = unknown>(actual: T, messageOrOptions?: string | { message?: string }): MakeMatchers<void, T, ExtendedMatchers>
  soft: Expect<ExtendedMatchers>                                                       // record failure, keep going
  poll: <T>(actual: () => T | Promise<T>, o?: string | { message?: string; timeout?: number; intervals?: number[] }) => PollMatchers<Promise<void>, T, ExtendedMatchers>
  configure: (o: { message?: string; timeout?: number; soft?: boolean }) => Expect<ExtendedMatchers>   // a pre-configured expect
  extend<M extends Record<string, (this: ExpectMatcherState, actual: any, ...args: any[]) => MatcherReturnType | Promise<MatcherReturnType>>>(matchers: M): Expect<ExtendedMatchers & M>   // register matchers → a NEW typed expect, never void
  not: Omit<AsymmetricMatchers, "any" | "anything">                                     // negation on Locator/Page/API/Generic assertions
} & AsymmetricMatchers                                                                   // expect.any/anything/arrayContaining/objectContaining/stringMatching …
// Auto-retrying web-first matchers (Locator/Page assertions): toBeVisible, toHaveText, toHaveValue, toHaveCount,
//   toBeChecked, toHaveURL, toBeAttached, … ; the visual/aria gauge is toHaveScreenshot() and toMatchAriaSnapshot().
// expect.poll(fn).toBe(x) and expect(fn).toPass() are the two general retry rails for non-DOM assertions.
```

`toHaveScreenshot()` (pixel gauge, golden-image compare) and `toMatchAriaSnapshot()` (accessibility-tree gauge) are the two projection matchers the e2e gauge reads as pass/fail data; both persist a golden under the project's snapshot dir — align with the `tests/contracts/` frozen-fixture discipline.

## [04]-[CONFIG_AND_REPORTER]

`defineConfig` is config-as-code: `projects: Project[]` is the browser × device matrix (each project a `{ name, use }` row where `use` layers `devices[...]` presets), and `use: PlaywrightTestOptions` sets the shared context. Options are ONE bag, split test-scoped vs worker-scoped.

```ts signature
// type PlaywrightTestConfig<T = {}, W = {}> = Config<PlaywrightTestOptions & CustomProperties<T>, PlaywrightWorkerOptions & CustomProperties<W>>  (Config extends TestConfig — the optional input fields live there)
interface TestConfig<T = {}, W = {}> {
  testDir?: string; testMatch?: string | RegExp | (string | RegExp)[]; outputDir?: string        // outputDir defaults to <package.json-dir>/test-results — a ROOT write on a bare run
  projects?: Project<T, W>[]; use?: UseOptions<T, W>
  reporter?: 'list'|'dot'|'line'|'github'|'json'|'junit'|'html'|'blob'|'null' | ReporterDescription[]
  timeout?: number; globalTimeout?: number; retries?: number; workers?: number | string           // workers accepts a '50%' cores string
  maxFailures?: number; forbidOnly?: boolean; failOnFlakyTests?: boolean; fullyParallel?: boolean
  captureGitInfo?: { commit?: boolean; diff?: boolean }; tsconfig?: string
  expect?: { timeout?: number; toHaveScreenshot?: { pathTemplate?; maxDiffPixels?; maxDiffPixelRatio?; threshold? }; toMatchAriaSnapshot?: { pathTemplate? } }
  webServer?: TestConfigWebServer | TestConfigWebServer[]                                          // the ARRAY form boots several servers before the run
  snapshotPathTemplate?: string   // tokens: {testDir} {snapshotDir} {platform} {projectName} {testFileDir} {testFilePath} {testFileName} {arg} {ext}
}
interface Project<T, W> {  // beyond name/use/testMatch: the dependency topology
  dependencies?: string[]   // project names that must pass first — the auth-setup project pattern (storageState written by setup, consumed via use.storageState)
  teardown?: string          // project that runs after this one and its dependents complete
}
// test-scoped (per test): baseURL, viewport, colorScheme, locale, timezoneId, geolocation, permissions, offline,
//   storageState, testIdAttribute, contextOptions, serviceWorkers, actionTimeout, navigationTimeout
// worker-scoped (per worker): browserName, headless, channel, launchOptions, connectOptions, trace, video, screenshot
// CONFIG RESOLUTION IS CWD-ONLY — resolveConfigLocation probes playwright.config.{ts,js,mts,mjs,cts,cjs} in process.cwd(),
//   never upward: only a ROOT-resident config defends a bare root invocation from a tree-wide *.spec.ts sweep.
// CLI rails: --last-failed (+ --last-failed-file, state under outputDir), --only-changed [ref],
//   blob reporter + `playwright merge-reports` for sharded runs, --update-snapshots for golden minting.
```

`@playwright/test/reporter` is the SPI a custom reporter implements to project results as data — the seam that feeds the e2e gauge a machine result rather than console text.

```ts signature
// import type { Reporter, FullConfig, Suite, TestCase, TestResult, TestStep, TestError, WorkerInfo, FullResult } from "@playwright/test/reporter"
interface Reporter {
  onBegin?(config: FullConfig, suite: Suite): void
  onTestBegin?(test: TestCase, result: TestResult): void
  onStepBegin?(test: TestCase, result: TestResult, step: TestStep): void
  onStepEnd?(test: TestCase, result: TestResult, step: TestStep): void                  // symmetric close — per-step timing/status as data
  onTestEnd?(test: TestCase, result: TestResult): void
  onStdOut?(chunk: string | Buffer, test?: TestCase, result?: TestResult): void         // captured output, correlated to the test/result
  onStdErr?(chunk: string | Buffer, test?: TestCase, result?: TestResult): void
  onError?(error: TestError, workerInfo?: WorkerInfo): void
  onEnd?(result: FullResult): Promise<{ status?: FullResult["status"] } | void> | void  // final aggregate; may override the run status
  onExit?(): Promise<void>                                                              // last async flush after onEnd
  printsToStdio?(): boolean                                                             // declare terminal ownership (a data reporter returns false)
}
```

## [05]-[INTEGRATION]

[STACK: `@playwright/test` standalone + `defineConfig` matrix] — the `tests/typescript/e2e` home owns a `playwright.config.ts` whose `projects` fan the same spec set across chromium/firefox/webkit and `devices` presets; `use.trace`/`video`/`screenshot` capture artifacts on retry, and a custom `Reporter` (subpath SPI) folds `TestResult` rows into the gauge's threshold data. `webServer` boots the app under test before the suite.

[STACK: `@playwright/test` engine + `@vitest/browser-playwright`] — the SAME Playwright engine is the browser PROVIDER under vitest browser mode (the sibling `vitest-browser-playwright.md`). A browser-runtime unit spec runs its `@effect/vitest` body inside a real browser page that Playwright drives — reusing the one `playwright install` browser binaries across the e2e gauge and the browser-mode unit lane, each lane keeping its own launch config (`use.launchOptions` here, `playwright({ launchOptions })` there). Playwright's `expect` is NOT used there; the vitest/`@effect/vitest` assertion rail is, because the spec is a vitest spec that merely executes in-browser.

[STACK: `@playwright/test` + `@types/k6`] — the `tests/typescript/e2e` home is one owner over two orthogonal drivers: Playwright is the functional + visual gauge (does the flow work, does it look right), k6 the load gauge (does it hold under concurrency). They never share a runtime; the k6 binary is a runner fact exactly as the browser binaries are.

[BOUNDARY vs the unit lane] — Playwright launches real browser processes: it is the heavy, high-fidelity end of the spectrum whose fast counterparts are `happy-dom`/`jsdom` (in-process DOM, no browser). A flow that needs no real engine belongs in the unit lane; a DOM-only assertion never justifies a browser launch.

[STACK: the embedded test MCP] — the pinned `playwright` package ships `playwright run-test-mcp-server` (tools `test_list`/`test_run`/`test_debug`): agent-driven run and debug of the e2e estate with zero additional admission; the standalone `@playwright/mcp` browser-automation server is machine-plane agent tooling and never enters the repo manifests.

## [06]-[RAIL_LAW]

- Owns: real-browser e2e — worker-scoped browser/context/page fixtures, web-first auto-retrying assertions, config-as-code project matrices over `devices`, visual (`toHaveScreenshot`) and aria (`toMatchAriaSnapshot`) gauges, and a result-projecting reporter SPI.
- Accept: `test.extend<Fixtures>` / `mergeTests` to grow the fixture DI rail; `defineConfig({ projects, use })` for the browser × device matrix; `expect.poll`/`expect(fn).toPass()` for non-DOM retries; `@playwright/test/reporter` `Reporter` for machine-readable results; the Playwright engine as the `@vitest/browser-playwright` provider for browser-mode unit specs.
- Reject: mixing Playwright `expect` with `@effect/vitest`'s in one file (two retry models); asserting through the browser what a DOM environment settles in-process (route to `happy-dom`/`jsdom`); treating the browser binaries as a JS dependency (they are an install-time runner fact); any import from a `plane:runtime` folder — dev gauge only.
- Boundary: each spec is a worker process driving a real browser — the slowest, highest-fidelity lane; artifacts (trace/video/screenshot) are retry-gated. Match `TestResult.status` and reporter events on their typed tokens, never on rendered console text.
