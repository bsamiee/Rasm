# [@vitest/browser-playwright] — vitest browser-mode driver running browser-runtime specs over playwright

[PACKAGE_SURFACE]:
- package: `@vitest/browser-playwright` · version `4.1.9` · license `MIT`
- module: ESM (`type: module`); subpaths `.` (the `playwright()` provider + `PlaywrightBrowserProvider`) and `./context`. Peers: `playwright *` (required — the real browser driver), `vitest 4.1.9`.
- asset: `dist/index.d.ts`. The test-side browser context (`page`/`userEvent`/locators) is `@vitest/browser` (reached via `vitest/browser`), which THIS provider augments with Playwright's real option and `_BrowserNames` shapes.
- runtime: drives a real Chromium/Firefox/WebKit through Playwright's `Browser`/`BrowserContext`/`Page`/`CDPSession`; specs execute IN the browser page, not in Node.
- plane: `plane:dev` — the browser-mode arm of the SPEC_RUNNER group; the `tests/typescript/_architecture` suite asserts no `plane:runtime` graph imports it.
- rail: browser-runtime spec execution — the real-DOM half of the `environment` axis.

`@vitest/browser-playwright` is the v4 per-provider browser package: the runner ships no built-in provider, so browser mode is admitted by importing THIS provider function. The v3 string form (`browser: { provider: 'playwright', name: 'chromium' }`) is GONE — v4 sets `browser.provider = playwright()` (an imported call) and lists a browser MATRIX in `browser.instances[]`. The provider augments the runner's ambient types: `_BrowserNames` gains `"firefox" | "webkit" | "chromium"`, `BrowserCommandContext` gains `page`/`frame`/`iframe`/`context`, and the `UserEvent*Options`/`ScreenshotOptions`/`ToMatchScreenshotOptions` gain Playwright's real shapes. It ENABLES the `vitest/browser` test context — `page`, `userEvent`, locators, `expect(el).toMatchScreenshot`, `cdp()`, `commands` — so a folder's own specs run against a real DOM. This is the browser-runtime unit/component lane, ORTHOGONAL to `@playwright/test` (the `tests/typescript/e2e` sibling, full cross-page e2e over a deployed app): browser mode runs a spec's own modules in a real engine; e2e drives an assembled application.

## [01]-[PROVIDER]

[ENTRYPOINT_SCOPE]: the provider function + its class — one `playwright(options?)` owns launch/connect/context/trace policy; browsers are config rows, not provider variants.

| [INDEX] | [SYMBOL]                                                        | [ENTRY_FAMILY]                       | [CAPABILITY]                                                                                                                                                                                                                 |
| :-----: | :-------------------------------------------------------------- | :----------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `playwright(options?)`                                          | provider factory                     | → `BrowserProviderOption`; the value `browser.provider` takes                                                                                                                                                                |
|  [02]   | `PlaywrightProviderOptions`                                     | options bag                          | `launchOptions`/`connectOptions.wsEndpoint`/`contextOptions`/`actionTimeout`/`persistentContext`                                                                                                                             |
|  [03]   | `PlaywrightBrowserProvider`                                     | class (`BrowserProvider`)            | drives Playwright — `getPage`/`getCommandsContext`/`getCDPSession`; `supportsParallelism`                                                                                                                                    |
|  [04]   | `_BrowserNames` augmentation (`browser.instances[].browser`)    | ambient registration (NOT an export) | registers `"firefox"/"webkit"/"chromium"`; the same `declare module` also augments `BrowserCommandContext`, `UserEvent*Options`, `ScreenshotOptions`, `ToMatchScreenshotOptions`, `CDPSession` with Playwright's real shapes |
|  [05]   | `defineBrowserCommand(fn)` (re-exported from `@vitest/browser`) | command factory                      | a typed server-side command callable from a spec via `commands`                                                                                                                                                              |

```ts contract
// The provider is a function call, NOT a string (v4). One call owns launch/connect/context/trace for every instance.
declare function playwright(options?: PlaywrightProviderOptions): BrowserProviderOption<PlaywrightProviderOptions>
interface PlaywrightProviderOptions {
  launchOptions?: Omit<LaunchOptions, "tracesDir">                          // playwright.launch — headless, args, channel
  connectOptions?: ConnectOptions & { wsEndpoint: string }                 // remote browser over WebSocket
  contextOptions?: Omit<BrowserContextOptions, "ignoreHTTPSErrors" | "serviceWorkers">   // newContext — viewport, locale, permissions
  actionTimeout?: number                                                   // per-userEvent ceiling (0 = none)
  persistentContext?: boolean | string                                     // reuse cookies/localStorage across runs (path or default cache)
}
// Browser vocabulary is REGISTERED by module augmentation, never exported — a spec reaches it through browser.instances[].browser:
declare module "vitest/node" { interface _BrowserNames { playwright: "firefox" | "webkit" | "chromium" } }
export { playwright, PlaywrightBrowserProvider, defineBrowserCommand }      // value exports; + type { PlaywrightProviderOptions, CDPSession }
```

## [02]-[CONFIG]

`browser.instances[]` is the v4 collapse point: a MATRIX of browser rows over one provider, not a provider-per-browser config. Each row is a `BrowserInstanceOption` overriding per-instance `headless`/`viewport`/`locators`/`screenshotFailures`; the provider drives all of them.

| [INDEX] | [FIELD]                                            | [SHAPE]                                                               | [CAPABILITY]                                                                                 |
| :-----: | :------------------------------------------------- | :-------------------------------------------------------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `browser.enabled`                                  | `boolean`                                                             | switch a project into browser mode                                                           |
|  [02]   | `browser.provider`                                 | `BrowserProviderOption`                                               | `playwright()` (or `webdriverio()`/`preview()` — `BrowserBuiltinProvider`)                   |
|  [03]   | `browser.instances[]`                              | `BrowserInstanceOption[]`                                             | the browser matrix — `{ browser: 'chromium' }`, `{ browser: 'firefox', headless: false }`, … |
|  [04]   | `browser.headless`                                 | `boolean` (default `CI`)                                              | headed for debug, headless for CI/parallel                                                   |
|  [05]   | `browser.viewport`                                 | `{ width; height }` (414×896)                                         | default page size; per-instance overridable                                                  |
|  [06]   | `browser.locators.testIdAttribute`                 | `string` (`data-testid`)                                              | the attribute `getByTestId` resolves; `locators.exact`                                       |
|  [07]   | `browser.trace`                                    | `BrowserTraceViewMode \| { mode; tracesDir; screenshots; snapshots }` | Playwright trace for trace.playwright.dev — playwright-only                                  |
|  [08]   | `browser.screenshotFailures`/`screenshotDirectory` | `boolean`/`string`                                                    | auto-capture on failure; artifact location                                                   |
|  [09]   | `browser.ui`                                       | `boolean` (default `!CI`)                                             | embed the `@vitest/ui` dashboard with the live iframe                                        |

```ts contract
type BrowserBuiltinProvider = "webdriverio" | "playwright" | "preview"
interface BrowserInstanceOption {   // Omit<ProjectConfig, unsupported> + per-instance browser-config overrides
  browser: "firefox" | "webkit" | "chromium"   // keyof _BrowserNames — the provider augments this union
  name?: string; provider?: BrowserProviderOption
  headless?: boolean; viewport?: { width: number; height: number }; screenshotFailures?: boolean
}
// defineConfig({ test: { browser: { enabled: true, provider: playwright(), instances: [{ browser: 'chromium' }, { browser: 'firefox' }] } } })
```

## [03]-[TEST_CONTEXT]

[PUBLIC_TYPE_SCOPE]: what the provider ENABLES in a spec — the `vitest/browser` context (`@vitest/browser/context`). A browser spec drives the real page through these; the provider binds each to Playwright under the hood.

| [INDEX] | [SYMBOL]                                             | [FAMILY]          | [CAPABILITY]                                                                                                                      |
| :-----: | :--------------------------------------------------- | :---------------- | :-------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `page: BrowserPage`                                  | page handle       | `viewport(w,h)`/`screenshot(opts)`/`elementLocator(el)`/`mark`/`extend` + all `LocatorSelectors`                                  |
|  [02]   | `LocatorSelectors` on `page`/`Locator`               | locator           | `getByRole`/`getByText`/`getByTestId`/`getByLabelText`/`getByPlaceholder`/`getByAltText`/`getByTitle`                             |
|  [03]   | `Locator`                                            | element handle    | chainable ARIA-first locator; `.click()`/`.screenshot()`/nested `getBy*`                                                          |
|  [04]   | `userEvent: UserEvent`                               | interaction       | `click`/`dblClick`/`tripleClick`/`wheel`(4.1)/`fill`/`keyboard`/`selectOptions`/`hover`/`dragAndDrop`/`upload`; `setup`/`cleanup` |
|  [05]   | `expect(el).toMatchScreenshot(opts?)`                | visual regression | pixel-diff against a stored baseline (`ScreenshotMatcherOptions`, custom `comparators`)                                           |
|  [06]   | `@vitest/browser/matchers`                           | DOM matchers      | retrying element assertions (`toBeVisible`/`toHaveText`/…) over async DOM                                                         |
|  [07]   | `commands: BrowserCommands` · `defineBrowserCommand` | server bridge     | `readFile`/`writeFile`/`removeFile` + custom Node-side commands from a browser spec                                               |
|  [08]   | `cdp(): CDPSession`                                  | devtools          | Chrome DevTools Protocol — chromium/playwright only                                                                               |
|  [09]   | `server` · `utils`                                   | context + debug   | `server` (platform/version/provider/browser/commands/config); `utils.debug`/`prettyDOM`                                           |

```ts contract
import { page, userEvent, commands, cdp, server } from 'vitest/browser'
// ARIA-first locator + real user interaction; every method routes to Playwright via the provider.
declare const page: BrowserPage      // getByRole/getByTestId/…, screenshot(opts), viewport(w,h), elementLocator(el)
interface UserEvent {
  click(el: Element | Locator, o?: UserEventClickOptions): Promise<void>
  fill(el: Element | Locator, text: string, o?: UserEventFillOptions): Promise<void>
  wheel(el: Element | Locator, o: UserEventWheelOptions): Promise<void>              // since 4.1
  selectOptions(el: Element | Locator, values: string | string[] | Locator[]): Promise<void>
  setup(): UserEvent; cleanup(): Promise<void>
}
// expect(page.getByTestId('save')).toBeVisible(); await userEvent.click(page.getByRole('button', { name: 'Save' }))
// await expect(page.getByTestId('canvas')).toMatchScreenshot({ comparators: { … } })   // visual regression
```

## [04]-[INTEGRATION]

[STACK: `@vitest/browser-playwright` ← `vitest` (`browser.provider: playwright()`)] — the mode seam. The provider plugs into `test.browser` (see `vitest.md`); the runner boots a Playwright browser per `instances[]` row and runs each spec inside it. The provider's module augmentation makes `browser` (`"chromium"|"firefox"|"webkit"`), `page`, and the `UserEvent*Options` type-check against Playwright's real capabilities.

[STACK: `@vitest/browser-playwright` + `@effect/vitest`] — the effect-in-browser seam. An `it.effect` body runs in the browser worker with `TestServices`, and its body drives `page`/`userEvent`/locators; a UI-facing kernel invariant is asserted as an effect law against the real DOM. `it.layer` shares a page-setup Layer across a browser `describe` block exactly as it shares a container in the harness lanes.

[STACK: `@vitest/browser-playwright` + `@vitest/coverage-v8` + `@vitest/ui`] — the report seam. `@vitest/coverage-v8/browser` collects coverage inside the browser and folds it into the Node lane's `CoverageMap`; `browser.ui: true` embeds the `@vitest/ui` dashboard with the live iframe so a failing browser spec is inspected in the DOM it ran in (see `vitest-coverage-v8.md`, `vitest-ui.md`).

[STACK: `@vitest/browser-playwright` + `@playwright/test` (one shared `playwright` engine)] — the engine-reuse seam, the mirror of `playwright-test.md`. Orthogonal in PURPOSE (this is the browser-runtime unit lane; `@playwright/test` is the app-e2e gauge) yet SHARED in engine: this provider's required `playwright` peer is the exact package `@playwright/test` bundles, so one `playwright install` of Chromium/Firefox/WebKit backs both lanes while each lane declares its OWN launch config from the same option shape — `PlaywrightProviderOptions.launchOptions`/`connectOptions` here (in `vitest.config.ts`) mirror `@playwright/test`'s worker-scoped `use.launchOptions`/`connectOptions` there (in `playwright.config.ts`), never a single shared config instance. The lanes diverge only at the ASSERTION rail: a browser-mode spec asserts through the `vitest`/`@effect/vitest` rail, NEVER Playwright's auto-retrying `expect` (that belongs to the `tests/typescript/e2e` driver).

[STACK boundary: `@vitest/browser-playwright` vs `@playwright/test` vs `happy-dom`/`jsdom`] — the three DOM modalities. `happy-dom`/`jsdom` (`test.environment`) are simulated DOM in Node — fast, no engine, adequate for logic that touches `document`. `@vitest/browser-playwright` runs the SAME spec in a real engine — real layout, real events, `toMatchScreenshot`. `@playwright/test` (`tests/typescript/e2e`, `playwright-test.md`) drives a deployed multi-page app end-to-end. One `environment` axis, escalating fidelity; the design picks the least-fidelity lane that proves the invariant.

## [05]-[RAIL_LAW]

- Owns: the browser-runtime spec lane — the `playwright()` provider (launch/connect/context/trace/persistent-context policy), the `PlaywrightBrowserProvider` driving real Chromium/Firefox/WebKit, and the `vitest/browser` test context it enables (`page`/`userEvent`/locators/`toMatchScreenshot`/`commands`/`cdp`).
- Accept: `browser.provider: playwright()` with a `browser.instances[]` matrix; ARIA-first `getByRole`/`getByTestId` locators over CSS selectors; `userEvent` real interactions over synthetic events; `expect(el).toMatchScreenshot` for visual regression; `defineBrowserCommand` for a Node-side bridge; `persistentContext`/`connectOptions.wsEndpoint` for reuse/remote.
- Reject: the v3 provider string (`provider: 'playwright'` — v4 imports the `playwright()` function); a provider-per-browser config (browsers are `instances[]` rows on one provider); a browser-mode spec for pure logic (use `happy-dom`/`jsdom` `environment`); a full cross-page app flow here (that is `@playwright/test`/`tests/typescript/e2e`); any `plane:runtime` import.
- Boundary: browser mode requires the `playwright` peer and downloaded browser binaries (a runner fact, not a JS dependency); `cdp()`/`trace`/`persistentContext` are playwright-specific and unavailable under other providers; `persistentContext` is ignored under parallel headless runs. Specs run in a real engine — slower than the DOM environments and non-deterministic in timing, so async assertions use the retrying `@vitest/browser/matchers`, never a bare read.
