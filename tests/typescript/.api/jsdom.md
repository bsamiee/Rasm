# [TS_TESTS_API_JSDOM]

[PACKAGE_SURFACE]:
- package: `jsdom` · license `MIT`
- module: CommonJS (`type: commonjs`); single entry `main: ./lib/api.js` — `require("jsdom")` / interop-default under ESM; no `exports` map; the barrel exports exactly `{ JSDOM, VirtualConsole, CookieJar, requestInterceptor, toughCookie }`.
- types: NONE bundled and `@types/jsdom` is NOT admitted — the member surface below is SOURCE-VERIFIED against `lib/api.js`, since the assay `npm:` rail parses declarations and this package ships none; the vitest `environment: 'jsdom'` string path needs no types, and a directly-typed `new JSDOM(...)` spec admits the community `@types/jsdom` first or consumes it untyped.
- asset: pure JS under `lib/`; `parse5` parses HTML, `tough-cookie` holds the jar, `undici` dispatches subresources, and `css-tree` + `@asamuzakjp/css-color` back the CSSOM — real standards implementations, not approximations.
- runtime: node `^22.22.2 || ^24.15.0 || >=26.0.0` — the tightest floor on the dev plane, so this lane sets the estate's node bar and the runner's own wider range never relaxes it; single-threaded, one contextified `vm` global per instance, heavier startup than `happy-dom`. `canvas ^3.2.3` is the sole optional peer, gating `<canvas>` pixels and `<img>` decode.
- plane: `plane:dev` — the fidelity `DOM_ENVIRONMENT` half of the `_testkit` unit lane; the fast counterpart is `happy-dom.md`; `tests/typescript/_architecture` fences it off every runtime graph.
- rail: dom-environment / fidelity-lane.

jsdom is the FIDELITY DOM of the `_testkit` unit lane: real `parse5` parsing, real `tough-cookie` cookie semantics, real `undici`-driven subresource loading, a `css-tree` CSSOM resolving the cascade down to computed pixels, and true in-`vm` script execution.

Two consumption seams carry it — vitest selects the environment by the `environment: 'jsdom'` string and forwards `test.environmentOptions.jsdom` as constructor options, and a spec needing an inspectable instance constructs `new JSDOM(html, options)` and reads `.window` / `.serialize()` / `.nodeLocation()`.

## [01]-[CORE]

[PUBLIC_TYPE_SCOPE]: the one construction owner and its instance surface — every access mode hangs off a `JSDOM`.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]        | [CAPABILITY]                                                                 |
| :-----: | :--------------------------- | :------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `JSDOM`                      | class                | the instance; `.window`, `.serialize()`, `.nodeLocation()`, `.reconfigure()` |
|  [02]   | `JSDOM.fromURL` / `fromFile` | static async         | build from a fetched URL / a file path — the fixture-load path               |
|  [03]   | `JSDOM.fragment(html)`       | static               | a bare `DocumentFragment`, no `Window` cost — the cheapest parse             |
|  [04]   | `VirtualConsole`             | class (EventEmitter) | in-DOM console capture / forward; `jsdomError` carries every uncaught fault  |
|  [05]   | `CookieJar`                  | class                | a `tough-cookie` `CookieJar` subclass — the shared cookie store              |
|  [06]   | `requestInterceptor`         | function             | build an `undici`-compatible interceptor that inspects/synthesizes requests  |
|  [07]   | `toughCookie`                | namespace            | the re-exported `tough-cookie` module (`Cookie`, `MemoryCookieStore`, …)     |

```ts
declare class JSDOM {
  constructor(html?: string | ArrayBuffer | ArrayBufferView, options?: ConstructorOptions)
  readonly window: DOMWindow
  readonly virtualConsole: VirtualConsole
  readonly cookieJar: CookieJar
  serialize(): string
  nodeLocation(node: Node): ElementLocation
  getInternalVMContext(): unknown
  reconfigure(settings: { windowTop?: DOMWindow; url?: string }): void
  static fromURL(url: string, options?: ConstructorOptions): Promise<JSDOM>
  static fromFile(filename: string, options?: ConstructorOptions): Promise<JSDOM>
  static fragment(html?: string): DocumentFragment
}
interface ConstructorOptions {
  url?: string; referrer?: string; contentType?: string
  includeNodeLocations?: boolean; storageQuota?: number
  runScripts?: "dangerously" | "outside-only"
  pretendToBeVisual?: boolean
  resources?: "usable" | { userAgent?: string; dispatcher?: Dispatcher; interceptors?: unknown[] }
  virtualConsole?: VirtualConsole; cookieJar?: CookieJar; beforeParse?(window: DOMWindow): void
}
```

- `JSDOM.fragment`: every call shares one lazily-built owner document, so generated cases collide on `ownerDocument` identity and never on content.
- `JSDOM.fromURL`: rejects `url` and `contentType` with a `TypeError` — the response's own URL and content type win, and it mints a fresh `CookieJar` when none is passed.
- `new JSDOM(...)`: forwards straight to the process `console` whenever `virtualConsole` is omitted, so a spec asserting on in-DOM output passes its own instance.
- `new JSDOM(bytes)`: byte input sniffs its encoding through `html-encoding-sniffer`, honoring `contentType`'s `charset` parameter; a string input is UTF-8 by construction.

## [02]-[RESOURCE_LOADING]

Subresource loading is ONE parameterized option keyed by shape: `undefined` fetches no subresources while `XMLHttpRequest` still works, `"usable"` fetches with defaults, and an object `{ userAgent?, dispatcher?, interceptors? }` takes any `undici` `Dispatcher` (proxy, mock-agent, custom pool); every other value throws a `TypeError`. `requestInterceptor(fn)` is the lightweight per-request hook when a full dispatcher is overkill.

```ts
import { JSDOM, requestInterceptor } from "jsdom"
new JSDOM(html, {
  runScripts: "dangerously",
  resources: {
    dispatcher:  agent,
    interceptors: [requestInterceptor((request, context) => {
      return undefined
    })],
  },
})
```

jsdom wraps whatever dispatcher it is handed in its own, carrying the instance's `cookieJar`, user agent, and a terminal decompress interceptor — so a `MockAgent` inherits real cookie and content-encoding semantics for free while spec-supplied interceptors run outermost. Scripts inside the DOM using synchronous `XMLHttpRequest` bypass all resource customization — a technical `vm` limitation, not a knob — and `runScripts: "dangerously"` gates `<script>` loading and event-handler attributes (`onclick=""`) alike.

## [03]-[CSSOM]

[ENTRYPOINT_SCOPE]: the cascade surface — the fidelity axis that makes this environment worth its startup cost.

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                                  |
| :-----: | :------------------------------------ | :------- | :------------------------------------------------------------ |
|  [01]   | `window.getComputedStyle(el)`         | instance | read-only `CSSStyleProperties`; indexed access + `length`     |
|  [02]   | `CSS.escape(ident)`                   | static   | CSSOM-conformant identifier escaping for a generated selector |
|  [03]   | `CSS.supports(property, value)`       | static   | declaration support against the real property definitions     |
|  [04]   | `CSS.supports(conditionText)`         | static   | full `@supports` condition text with `not` / `and` / `or`     |
|  [05]   | `cssSupportsRule.matches`             | property | whether the rule's condition holds in this environment        |
|  [06]   | `cssMediaRule.matches`                | property | media-TYPE evaluation; every media feature reports `false`    |
|  [07]   | `styleSheet.ownerNode`/`href`/`title` | property | the sheet's origin node, resolved URL, and title              |

Computed style resolves the cascade into pixels: every length unit folds to `px` against the inherited font size and a 96dpi absolute scale (`10em` → `160px`, `1in` → `96px`, `calc(1em + 4px)` → `20px`, `background-position-x: 3em` → `48px`), while percentages survive as authored (`background-position-y: 25%` → `25%`) since no layout exists to resolve them against. Inline `element.style.getPropertyValue()` returns the SPECIFIED value instead, leaving `calc(2em + 10px)` unresolved — so resolution asserts against the computed declaration and authoring against the inline one.

Both declarations serialize canonically — `rgb(1 2 3 / .5)` renders `rgba(1, 2, 3, 0.5)` and `url(a.png)` renders `url("a.png")` — so a golden compares canonical text, never the authored spelling.

`CSS.supports` gates every style golden, answering from the real property definitions and rejecting an unparseable value: an unsupported property drops to `""` in the declaration and silently weakens the assertion, so a spec proves support first and asserts the value second.

Rule-condition matching splits by kind — `@supports` conditions evaluate for real, while `cssMediaRule.matches` resolves the media TYPE alone (`screen` and `all` hold, `print` does not) and reports `false` for every media FEATURE (`min-width`, `prefers-color-scheme`), the environment carrying neither a laid-out viewport nor user preferences. `window.matchMedia` stays absent under every option combination, so a viewport-conditional law belongs to `happy-dom`.

## [04]-[CONSOLE_AND_COOKIES]

| [INDEX] | [SURFACE]                                             | [CAPABILITY]                                                                   |
| :-----: | :---------------------------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new VirtualConsole()` + `.on(method, fn)`            | `EventEmitter`; subscribe to `log`/`warn`/`error`/`info`/… + `jsdomError`      |
|  [02]   | `virtualConsole.forwardTo(console, { jsdomErrors? })` | mirror to a real console; `jsdomErrors`: `undefined` \| `string[]` \| `"none"` |
|  [03]   | `new CookieJar(store?, options?)`                     | `tough-cookie` jar; a shared jar correlates cookies across instances           |

`jsdomError` on `VirtualConsole` surfaces uncaught in-DOM script errors, unimplemented surfaces, failed resource loads, and unparseable stylesheets alike — a fidelity spec subscribes to it rather than letting jsdom default-forward to `console.error`.

Every payload is an `Error` discriminated by a `type` token (`"unhandled-exception"`, `"not-implemented"`, `"resource-loading"`, `"css-parsing"`) carrying its evidence beside it: `cause` holds the originating error, `url` rides a resource failure, `sheetText` rides a CSS parse failure. Laws match the token and read the field, never a message substring; `forwardTo`'s `jsdomErrors` filters that same token set and `"none"` suppresses the category outright.

`CookieJar` forces `looseMode` on, so a `Set-Cookie` without a domain attribute is accepted rather than dropped — a fixture recorded from a lax server replays faithfully, and a spec proving strict cookie rejection constructs `tough-cookie`'s own jar instead.

## [05]-[INTEGRATION]

[STACK: `jsdom` environment + `@effect/vitest`] — same environment role as `happy-dom`: `environment: 'jsdom'` (config or a `// @vitest-environment jsdom` docblock) installs the DOM globals under which `it.effect`/`it.layer` bodies run; the fidelity axis is the whole reason to pay jsdom's startup cost — pick it only when the spec reaches the object model `happy-dom` leaves absent.

[STACK: `jsdom.serialize()` + the `libs/contracts/conformance/` goldens] — `serialize()` returns the exact WHATWG HTML fragment serialization and lands through `toMatchFileSnapshot` against a frozen corpus asset, while `nodeLocation()` (under `includeNodeLocations`) returns `parse5` source offsets for the diff. jsdom's parser is the standards oracle, the frozen bytes are the expectation, and `CSS.supports()` proves the property preconditions a style golden depends on before the comparison runs.

[STACK: `jsdom` + `effect/FastCheck`] — a property generating HTML fragments runs each case through `JSDOM.fragment()` (the cheapest parse — no `Window`) and asserts a structural invariant; `Arbitrary.make(schema)` from the `_testkit` law/arbitrary source (`fast-check.md`) feeds the predicate and `it.effect.prop` binds it, so the generated markup and the DOM oracle share one engine instance. `CSS.escape()` is the seam for a generated identifier reaching `querySelector` — a raw generated string is a parse failure, never a miss. Full-`Window` specs use `new JSDOM()` per case only when script execution is under test.

[STACK: `resources.dispatcher` + `undici`] — because subresource loading is an `undici` `Dispatcher`, a spec sandboxes network by passing an `undici` `MockAgent` (deterministic canned responses) or a `ProxyAgent` — the same `undici` primitive the runtime data plane uses, so a fixture recorded once drives both — and jsdom threads the instance's `cookieJar` through that dispatcher, so cookie correlation holds across mocked hops. `requestInterceptor` covers the inspect-one-request case without a full agent, and `jsdomError`'s `resource-loading` payload carries the failing `url` when a hop is not stubbed.

[BOUNDARY vs `happy-dom`] — both environments resolve computed lengths to pixels, so the split is the object model beneath them: jsdom executes scripts, answers `CSS.supports` from real property definitions, materializes the rule graph (`cssRule.matches`, `styleSheet.ownerNode`), canonicalizes every serialized value, and implements XPath through `document.evaluate` — each of which `happy-dom` leaves absent or unvalidated. Each instance costs a full contextified `vm` on a single thread, so a spec touching none of those is a `happy-dom` spec, and a viewport-conditional law is one outright since `matchMedia` lives only there. When a spec needs true rendering or a real browser engine, it is a `playwright-test` spec.

## [06]-[RAIL_LAW]

- Owns: a spec-conformant WHATWG DOM for the fidelity unit lane; `parse5` parsing, in-`vm` script execution, `tough-cookie` cookies, `undici`-dispatched subresource loading, a `css-tree` CSSOM resolving computed pixels, exact `serialize()`, and `parse5` `nodeLocation()`.
- Accept: `environment: 'jsdom'` + `environmentOptions.jsdom` for global specs; `new JSDOM(html, { runScripts, resources, includeNodeLocations })` for inspectable specs; `resources.dispatcher` (`undici` `MockAgent`/`ProxyAgent`) or `requestInterceptor` to sandbox network; `getComputedStyle` + `CSS.supports` for cascade proof; `VirtualConsole` `jsdomError` subscription for in-DOM fault capture.
- Reject: asserting an authored value against the computed declaration (computed resolves lengths to pixels and canonicalizes color and url serialization); a style golden that never gated on `CSS.supports` (an unsupported property reads `""` and the assertion proves nothing); a media-feature assertion (`min-width`, `prefers-color-scheme`) here rather than on `happy-dom`'s `matchMedia`; discriminating a `document.evaluate()` failure by `instanceof` or message, since a malformed expression throws an `XPathException`-named `Error` (`code` 51) while an unsupported result type throws a `DOMException` — match `name` and `code`; typed direct construction without admitting `@types/jsdom` (source ships no declarations); `getInternalVMContext()` without `runScripts` set (it throws); real rendering or cross-browser assertions (route to `playwright-test`); speed-critical DOM specs that need no fidelity (route to `happy-dom`); any import from a `plane:runtime` folder — dev environment only.
- Boundary: single-threaded, no layout geometry, no paint; `pretendToBeVisual` grants `requestAnimationFrame` and visibility state alone. Synchronous in-DOM `XMLHttpRequest` bypasses all resource customization.
