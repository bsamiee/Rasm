# [happy-dom] — fast DOM environment for the unit lane

[PACKAGE_SURFACE]:
- package: `happy-dom` · version `20.10.6` · license `MIT`
- module: ESM (`type: module`); single barrel `happy-dom` — no `exports` map, so deep paths (`happy-dom/lib/window/Window.js`) resolve but the barrel is canonical; declarations ship co-located (`lib/**/*.d.ts`, implicit `lib/index.d.ts`).
- asset: pure JS + `.d.ts` under `lib/`; zero native, zero wasm — a hand-written WHATWG DOM in TypeScript.
- runtime: node `>=20`; the whole DOM is a plain object graph the spec builds and discards — microsecond startup, no browser process, no layout, no pixels.
- plane: `plane:dev` — the fast `DOM_ENVIRONMENT` half of the `_testkit` unit lane; the fidelity counterpart is `jsdom.md`. The `tests/typescript/_architecture` suite fences it off every runtime graph.
- rail: dom-environment / fast-unit-lane.

happy-dom is the FAST DOM of the `_testkit` unit lane: it renders nothing and runs no layout, trading strict spec-conformance for speed so a DOM-touching unit spec settles in microseconds. Two consumption seams — vitest selects it by the `environment: 'happy-dom'` string (vitest dynamically imports this package and installs its classes as globals; `test.environmentOptions.happyDOM` forwards `Window` construction options), and a spec needing an isolated, directly-controlled DOM constructs `new Window(...)` and drives async settling through `window.happyDOM` (a `DetachedWindowAPI`). The fidelity boundary — in-page `<script>` execution, exact computed-style, byte-exact WHATWG serialization — routes to `jsdom.md`.

## [01]-[ENTRY_WINDOW]

[PUBLIC_TYPE_SCOPE]: the global DOM the unit lane installs — one window owner plus its detached control surface.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY / BOUNDARY]                                                        |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `Window`            | class         | the constructable global; `extends BrowserWindow`; owns the `happyDOM` control API |
|  [02]   | `GlobalWindow`      | class         | `Window` that also mixes Node globals onto itself — the vitest global-install shape |
|  [03]   | `BrowserWindow`     | class         | the DOM/BOM member base both windows extend (no lifecycle control of its own)   |
|  [04]   | `DetachedWindowAPI` | class         | `window.happyDOM`; the async-settle + viewport + settings control rail          |
|  [05]   | `IOptionalBrowserSettings` | interface | the one construction policy bag (see [03])                                    |

```ts contract
// Prefer `new Window(...)` over touching globals when a spec needs an isolated DOM; the vitest 'happy-dom' environment builds a GlobalWindow for you.
declare class Window extends BrowserWindow {
  readonly happyDOM: DetachedWindowAPI
  constructor(options?: { width?: number; height?: number; url?: string; console?: IConsole; settings?: IOptionalBrowserSettings })
}
// The control surface — the ONLY sound way to await DOM async work (timers, fetch, microtasks) before asserting.
declare class DetachedWindowAPI {
  get settings(): IBrowserSettings
  get virtualConsolePrinter(): VirtualConsolePrinter        // buffered console output the spec can readAll()/dump
  waitUntilComplete(): Promise<void>                        // settle all pending async before assertion
  abort(): Promise<void>                                    // cancel outstanding async tasks
  close(): Promise<void>                                    // abort + tear the window down (release the object graph)
  setURL(url: string): void                                 // relocate document.location without navigation
  setViewport(viewport: IOptionalBrowserPageViewport): void // width/height/deviceScaleFactor for media-query specs
}
```

## [02]-[HEADLESS_BROWSER]

[ENTRYPOINT_SCOPE]: multi-page / multi-context navigation without a browser process — the in-process analogue of the `playwright-test` e2e driver, used when an e2e-shaped flow must stay in the fast lane.

| [INDEX] | [SURFACE]                              | [PRODUCES]                  | [CAPABILITY]                                                    |
| :-----: | :------------------------------------- | :-------------------------- | :-------------------------------------------------------------- |
|  [01]   | `new Browser({ settings?, console? })` | `Browser`                   | root; `.defaultContext`, `.contexts`, `.newIncognitoContext()` |
|  [02]   | `browser.newPage()`                    | `BrowserPage`               | a tab; `.mainFrame`, `.content` get/set, `.url` get/set, `.closed` |
|  [03]   | `page.goto(url, IGoToOptions?)`        | `Promise<Response \| null>` | fetch + parse a document into the page (honors nav settings)   |
|  [04]   | `page.evaluate(script)` / `evaluateModule({url,module})` | `any`     | run a `string \| Script` in page scope; module form for ESM    |
|  [05]   | `page.waitUntilComplete()` / `page.abort()` / `page.reload()` | `Promise` | per-page settle / cancel / reload                          |
|  [06]   | `DetachedBrowser*`                     | class family                | reuse an existing global window as the browser root (vitest env case) |

```ts contract
declare class Browser {
  readonly contexts: BrowserContext[]; readonly settings: IBrowserSettings
  get defaultContext(): BrowserContext
  newPage(): BrowserPage; newIncognitoContext(): BrowserContext
  waitUntilComplete(): Promise<void>; abort(): Promise<void>; close(): Promise<void>
}
declare class BrowserPage {
  readonly mainFrame: BrowserFrame; readonly virtualConsolePrinter: VirtualConsolePrinter; readonly closed: boolean
  get content(): string; set content(html: string); get url(): string; set url(url: string)
  goto(url: string, options?: IGoToOptions): Promise<Response | null>
  evaluate(script: string | Script): any; setViewport(v: IOptionalBrowserPageViewport): void
  waitUntilComplete(): Promise<void>; abort(): Promise<void>; close(): Promise<void>
}
```

## [03]-[SETTINGS]

`IOptionalBrowserSettings` is ONE parameterized policy bag threaded through `Window`, `Browser`, and the vitest `environmentOptions.happyDOM` key — never a matrix of constructor flags. It is where a unit spec bounds cost and picks fidelity knobs.

```ts contract
interface IOptionalBrowserSettings {
  disableJavaScriptEvaluation?: boolean; disableJavaScriptFileLoading?: boolean   // the "no script execution" fast default
  disableCSSFileLoading?: boolean; enableImageFileLoading?: boolean
  disableComputedStyleRendering?: boolean                                          // skip the layout-approximating style engine
  handleDisabledFileLoadingAsSuccess?: boolean
  timer?: { maxTimeout?: number; maxIntervalTime?: number; maxIntervalIterations?: number; preventTimerLoops?: boolean }
  fetch?: { disableSameOriginPolicy?: boolean; disableStrictSSL?: boolean; interceptor?: IFetchInterceptor | null; virtualServers?: IVirtualServer[] | null }
  navigation?: { disableMainFrameNavigation?: boolean; disableChildFrameNavigation?: boolean; crossOriginPolicy?: BrowserNavigationCrossOriginPolicyEnum }
  navigator?: { userAgent?: string; maxTouchPoints?: number }; device?: { prefersColorScheme?: string }
  errorCapture?: BrowserErrorCaptureEnum; enableFileSystemHttpRequests?: boolean
}
```

`timer.preventTimerLoops` + `maxIntervalIterations` bound a spec (or a generated `fast-check` case) that schedules a runaway `setInterval`; `fetch.virtualServers` / `fetch.interceptor` stub network without a real socket; `navigator`/`device` drive media-query and pointer specs. `errorCapture` (`BrowserErrorCaptureEnum`) routes uncaught in-DOM errors to `virtualConsolePrinter` instead of the process.

## [04]-[DOM_SURFACE]

The full WHATWG roster — `Document`, the `Element`/`Node` tree, the `Event` family (`CustomEvent`, `KeyboardEvent`, `PointerEvent`, `SubmitEvent`, …), the CSS-rule family (`CSSStyleSheet`, `CSSStyleRule`, `CSSMediaRule`, `CSSContainerRule`, …), the fetch/file family (`Request`, `Response`, `Headers`, `Blob`, `File`, `FormData`), and the observers (`MutationObserver`, `ResizeObserver`, `IntersectionObserver`) — is SEED DATA re-exported by the one barrel, never a list a consumer hand-enumerates. A spec reaches these as globals (vitest env) or off a `Window` instance; the catalog owners are the entry `Window`/`Browser` and the two utility owners below.

| [INDEX] | [SYMBOL]                                    | [CAPABILITY / BOUNDARY]                                                        |
| :-----: | :------------------------------------------ | :----------------------------------------------------------------------------- |
|  [01]   | `VirtualConsole` / `VirtualConsolePrinter`  | capture console output as structured records; `printer.readAll()` drains for assertion — the console-parity seam |
|  [02]   | `DOMParser` / `XMLSerializer`               | parse an HTML/XML string to a document and serialize back — the fragment round-trip a `tests/contracts/` golden byte assertion drives |
|  [03]   | `BrowserErrorCaptureEnum` / `BrowserNavigationCrossOriginPolicyEnum` | the bounded vocabularies `settings.errorCapture` / `navigation.crossOriginPolicy` select |

## [05]-[INTEGRATION]

[STACK: `happy-dom` environment + `@effect/vitest`] — the DOM lane and the effect-spec rail are one runtime. A DOM-touching spec sets `environment: 'happy-dom'` (config or a `// @vitest-environment happy-dom` docblock), then runs `it.effect`/`it.layer` bodies against the installed globals. When the effect under test schedules DOM async work (timers, `fetch`, microtasks), `window.happyDOM.waitUntilComplete()` is the settle point folded into the effect before the assertion — never a bare `await Promise.resolve()`. `layer(SharedLayer)(...)` still shares acquired resources across the block; the DOM environment is orthogonal to the Layer.

[STACK: `happy-dom` + `@electric-sql/pglite`] — both are the FAST HALF of the `_testkit` unit lane: in-process, no server, no external process, microsecond-to-millisecond startup. A spec that needs both a DOM and a database composes the `PGlite` Layer (see `electric-sql-pglite.md`) under the `happy-dom` environment in one `layer(...)` block — the whole verification runs in-process with nothing to tear down but object graphs.

[STACK: `happy-dom` + `fast-check`] — a property that generates DOM inputs (markup fragments, event sequences, viewport dimensions) runs each generated case inside the window; `settings.timer.preventTimerLoops` and `maxIntervalIterations` bound a pathological generated case so shrinking terminates. The `Schema`-derived arbitraries in the `_testkit` law/arbitrary source feed the same predicate.

[BOUNDARY vs `jsdom`] — happy-dom disables script execution and computed-style rendering by default and approximates (never implements) layout. A spec asserting in-page `<script>` side effects, exact `getComputedStyle` cascade, or byte-exact WHATWG fragment serialization is a `jsdom` spec by definition. Both are `plane:dev` DOM environments; neither may be imported from a `plane:runtime` folder.

## [06]-[RAIL_LAW]

- Owns: an in-process, layout-free WHATWG DOM for the fast unit lane; global install via the vitest `happy-dom` environment, direct isolated construction via `new Window(...)`, headless multi-page navigation via `Browser`/`BrowserPage`, and async settling via `DetachedWindowAPI.waitUntilComplete()`.
- Accept: `environment: 'happy-dom'` + `environmentOptions.happyDOM` for global specs; `new Window({ settings })` for isolated specs; `settings.timer`/`settings.fetch.virtualServers` to bound cost and stub network; `VirtualConsolePrinter.readAll()` for console-parity assertions.
- Reject: `await`-ing raw promises instead of `waitUntilComplete()` (async DOM work silently outlives the assertion); in-page `<script>` / exact-computed-style / byte-exact-serialization assertions (route to `jsdom`); real network or a real browser process (that is the `playwright-test` e2e gauge); any import from a `plane:runtime` folder — dev environment only.
- Boundary: no rendering, no real layout (`disableComputedStyleRendering` only toggles the approximation), no pixel output; visual-browser affordances are emulated. When a spec needs true rendering or cross-browser behavior it is a `playwright-test` browser spec, not a DOM-environment spec.
