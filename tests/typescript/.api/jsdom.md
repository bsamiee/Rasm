# [jsdom] — spec-conformant DOM environment where fidelity outranks speed

[PACKAGE_SURFACE]:
- package: `jsdom` · version `29.1.1` · license `MIT`
- module: CommonJS (`type: commonjs`); single entry `main: ./lib/api.js` — `require("jsdom")` / interop-default under ESM; no `exports` map; the barrel exports exactly `{ JSDOM, VirtualConsole, CookieJar, requestInterceptor, toughCookie }`.
- types: NONE bundled and `@types/jsdom` is NOT admitted — the member surface below is SOURCE-VERIFIED against `lib/api.js` (assay resolves the package as `tsdecl` but reports zero declaration paths). The vitest `environment: 'jsdom'` string path needs no types; a directly-typed `new JSDOM(...)` spec must admit the community `@types/jsdom` first or consume it untyped.
- asset: pure JS under `lib/`; the HTML parser is `parse5`, cookies are `tough-cookie`, subresource loading is `undici` — real standards implementations, not approximations.
- runtime: node `^20.19 || ^22.13 || >=24`; single-threaded; a full contextified `vm` global per instance — heavier startup than `happy-dom`.
- plane: `plane:dev` — the fidelity `DOM_ENVIRONMENT` half of the `_testkit` unit lane; the fast counterpart is `happy-dom.md`. The `tests/typescript/_architecture` suite fences it off every runtime graph.
- rail: dom-environment / fidelity-lane.

jsdom is the FIDELITY DOM of the `_testkit` unit lane: real `parse5` parsing, real `tough-cookie` cookie semantics, real `undici`-driven subresource loading, and true in-`vm` script execution — the environment a spec picks when byte-exact WHATWG serialization, `getComputedStyle` cascade, or in-page `<script>` behavior must be correct rather than fast. Two consumption seams — vitest selects it by the `environment: 'jsdom'` string (`test.environmentOptions.jsdom` forwards constructor options), and a spec needing an inspectable instance constructs `new JSDOM(html, options)` and reads `.window` / `.serialize()` / `.nodeLocation()`. The v29 API is authoritative below: the pre-v29 `ResourceLoader` class is GONE — subresource loading is now an `undici` `Dispatcher` under the `resources` option, and `requestInterceptor` + `toughCookie` are new named exports.

## [01]-[CORE]

[PUBLIC_TYPE_SCOPE]: the one construction owner and its instance surface — every access mode hangs off a `JSDOM`.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]        | [CAPABILITY]                                                                 |
| :-----: | :--------------------------- | :------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `JSDOM`                      | class                | the instance; `.window`, `.serialize()`, `.nodeLocation()`, `.reconfigure()` |
|  [02]   | `JSDOM.fromURL` / `fromFile` | static async         | build from a fetched URL / a file path — the fixture-load path               |
|  [03]   | `JSDOM.fragment(html)`       | static               | a bare `DocumentFragment`, no `Window` cost — the cheapest parse             |
|  [04]   | `VirtualConsole`             | class (EventEmitter) | in-DOM console capture / forward; `jsdomError` event carries uncaught errors |
|  [05]   | `CookieJar`                  | class                | a `tough-cookie` `CookieJar` subclass — the shared cookie store              |
|  [06]   | `requestInterceptor`         | function             | build an `undici`-compatible interceptor that inspects/synthesizes requests  |
|  [07]   | `toughCookie`                | namespace            | the re-exported `tough-cookie` module (`Cookie`, `MemoryCookieStore`, …)     |

```ts signature
// SOURCE-VERIFIED shapes (no bundled .d.ts). runScripts absent ⇒ scripts do NOT run (the safe default); getInternalVMContext() throws unless runScripts was set.
declare class JSDOM {
  constructor(html?: string | Buffer, options?: ConstructorOptions)
  readonly window: DOMWindow
  readonly virtualConsole: VirtualConsole
  readonly cookieJar: CookieJar
  serialize(): string                                  // HTML-fragment-serialization-algorithm output — the byte-parity source
  nodeLocation(node: Node): ElementLocation | null     // parse5 source location; requires includeNodeLocations: true
  getInternalVMContext(): unknown                       // the contextified global for node:vm; throws without runScripts
  reconfigure(settings: { windowTop?: DOMWindow; url?: string }): void
  static fromURL(url: string, options?: ConstructorOptions): Promise<JSDOM>
  static fromFile(filename: string, options?: ConstructorOptions): Promise<JSDOM>
  static fragment(html?: string): DocumentFragment
}
interface ConstructorOptions {
  url?: string; referrer?: string; contentType?: string    // contentType "text/html" (default) vs an XML type flips the parser
  includeNodeLocations?: boolean; storageQuota?: number     // storageQuota default 5_000_000 (localStorage/sessionStorage bytes)
  runScripts?: "dangerously" | "outside-only"               // absent ⇒ no execution; "outside-only" gives window.eval but no in-page <script>
  pretendToBeVisual?: boolean                               // requestAnimationFrame / document.hidden fakery for rAF specs
  resources?: "usable" | { userAgent?: string; dispatcher?: Dispatcher; interceptors?: unknown[] }  // see [02]
  virtualConsole?: VirtualConsole; cookieJar?: CookieJar; beforeParse?(window: DOMWindow): void
}
```

## [02]-[RESOURCE_LOADING]

Subresource loading is ONE parameterized option, not a class hierarchy — the pre-v29 `ResourceLoader` subclassing pattern was removed. `resources` discriminates on three shapes: `undefined` (no automatic subresource fetch; XHR still works), `"usable"` (fetch with defaults), or an object `{ userAgent?, dispatcher?, interceptors? }` where `dispatcher` is any `undici` `Dispatcher` (proxy, mock-agent, custom pool). `requestInterceptor(fn)` is the lightweight per-request hook when a full dispatcher is overkill.

```ts signature
import { JSDOM, requestInterceptor } from "jsdom"
// A dispatcher routes real fetches; an interceptor inspects/synthesizes them — compose both to sandbox network in a spec.
new JSDOM(html, {
  runScripts: "dangerously",
  resources: {
    dispatcher: /* undici ProxyAgent | MockAgent | Pool */ agent,
    interceptors: [requestInterceptor((request, context) => {
      // return a synthetic Response to stub, or undefined to pass through; context.element names the requesting node
      return undefined
    })],
  },
})
```

Scripts inside the DOM using synchronous `XMLHttpRequest` bypass all resource customization — a technical `vm` limitation, not a knob. `<script>` loading additionally requires `runScripts: "dangerously"`; event-handler attributes (`onclick=""`) also depend on that flag.

## [03]-[CONSOLE_AND_COOKIES]

| [INDEX] | [SURFACE]                                             | [CAPABILITY]                                                                   |
| :-----: | :---------------------------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new VirtualConsole()` + `.on(method, fn)`            | `EventEmitter`; subscribe to `log`/`warn`/`error`/`info`/… + `jsdomError`      |
|  [02]   | `virtualConsole.forwardTo(console, { jsdomErrors? })` | mirror to a real console; `jsdomErrors`: `undefined` \| `string[]` \| `"none"` |
|  [03]   | `new CookieJar(store?, options?)`                     | `tough-cookie` jar; a shared jar correlates cookies across instances           |

The `jsdomError` event on `VirtualConsole` is where uncaught in-DOM script errors and resource-load failures surface — a fidelity spec subscribes to it rather than letting jsdom default-forward to `console.error`. Match `jsdomError` categories (`"unhandled-exception"`, `"not-implemented"`, `"resource-loading"`) on the category token, never on message text.

## [04]-[INTEGRATION]

[STACK: `jsdom` environment + `@effect/vitest`] — same environment role as `happy-dom`: `environment: 'jsdom'` (config or `// @vitest-environment jsdom` docblock) installs the DOM globals under which `it.effect`/`it.layer` bodies run. The fidelity axis is the whole reason to pay jsdom's startup cost — pick it only when the spec asserts something `happy-dom` approximates.

[STACK: `jsdom.serialize()` + the `tests/contracts/` goldens] — `serialize()` returns the exact WHATWG HTML fragment serialization, and `nodeLocation()` (under `includeNodeLocations`) returns `parse5` source offsets. A spec asserting a rendered/serialized document against a frozen `tests/contracts/` byte fixture uses these two as the reproducer — jsdom's parser is the standards oracle, the frozen bytes are the expectation.

[STACK: `jsdom` + `fast-check`] — a property generating HTML fragments runs each case through `JSDOM.fragment()` (the cheapest parse — no `Window`) and asserts a structural invariant; `Schema`-derived arbitraries from the `_testkit` law/arbitrary source feed the predicate. Full-`Window` specs use `new JSDOM()` per case only when script execution is under test.

[STACK: `resources.dispatcher` + `undici`] — because subresource loading is an `undici` `Dispatcher`, a spec sandboxes network by passing an `undici` `MockAgent` (deterministic canned responses) or a `ProxyAgent` — the same `undici` primitive the runtime data plane uses, so a fixture recorded once drives both. `requestInterceptor` covers the inspect-one-request case without a full agent.

[BOUNDARY vs `happy-dom`] — jsdom executes scripts, implements computed style, and serializes to spec; it costs a full contextified `vm` per instance and is single-threaded. When a spec needs neither script execution nor exact serialization, it is a `happy-dom` spec. When it needs true rendering or a real browser engine, it is a `playwright-test` spec.

## [05]-[RAIL_LAW]

- Owns: a spec-conformant WHATWG DOM for the fidelity unit lane; `parse5` parsing, in-`vm` script execution, `tough-cookie` cookies, `undici`-dispatched subresource loading, exact `serialize()`, and `parse5` `nodeLocation()`.
- Accept: `environment: 'jsdom'` + `environmentOptions.jsdom` for global specs; `new JSDOM(html, { runScripts, resources, includeNodeLocations })` for inspectable specs; `resources.dispatcher` (`undici` `MockAgent`/`ProxyAgent`) or `requestInterceptor` to sandbox network; `VirtualConsole` `jsdomError` subscription for in-DOM error capture.
- Reject: composing a `ResourceLoader` (removed in v29 — use `resources.dispatcher`); typed direct construction without admitting `@types/jsdom` (source ships no declarations); `getInternalVMContext()` without `runScripts` set (it throws); real rendering/cross-browser assertions (route to `playwright-test`); speed-critical DOM specs that need no fidelity (route to `happy-dom`); any import from a `plane:runtime` folder — dev environment only.
- Boundary: single-threaded, no layout geometry, no paint; `pretendToBeVisual` only fakes `requestAnimationFrame`/visibility. Synchronous in-DOM `XMLHttpRequest` bypasses all resource customization.
