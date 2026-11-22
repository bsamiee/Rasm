# [PLATFORM]

## [01]-[REACH]

Each web platform capability reaches an extension's users by its status:
- Released at or before `minimum_chrome_version` -> code calls it directly, with no feature test or fallback
- Released after the minimum -> raise it to that release, or leave that capability out
- Flag-gated -> prototype in the owner's Chrome and a test browser launched with its switch, keeping released code off it
- Origin trial -> enable it on extension pages through manifest `trial_tokens`, and in content scripts through a page token

Release dates come from chromiumdash and release status from chromestatus. Flags change the owner's browser alone, and `chrome://version/?show-variations-cmd` shows features a field trial enables by default.

## [02]-[INTERFACE_FEATURES]

Released CSS and DOM features for extension UI, each with its first release and use:

| [INDEX] | [FEATURE]                                         | [CHROME] | [USE]                                                      |
| :-----: | :------------------------------------------------ | :------: | :--------------------------------------------------------- |
|  [01]   | `popover`, top layer                              |   114    | `manual` for injected UI                                   |
|  [02]   | `popover="hint"`                                  |   133    | Page tooltips, joins light dismiss and hint stacks         |
|  [03]   | `@starting-style`                                 |   117    | Entry start state                                          |
|  [04]   | `transition-behavior: allow-discrete`, `overlay`  |   117    | Exit keeps `display` and top layer                         |
|  [05]   | `light-dark()` colors                             |   123    | Scheme-dependent tokens                                    |
|  [06]   | `light-dark()` images                             |   150    | Scheme-dependent icons                                     |
|  [07]   | Anchor positioning, `position-area`               | 125, 129 | Popups anchored to page elements                           |
|  [08]   | `position-visibility: anchor-valid`               |   157    | Anchored UI hidden when anchor leaves                      |
|  [09]   | Invoker commands `command`, `commandfor`          |   135    | Declarative popover and dialog control                     |
|  [10]   | `interestfor` interest invokers                   |   142    | Hover and focus invokers                                   |
|  [11]   | Dialog `closedby`, `:open`                        | 134, 133 | Light dismiss rules, open state selector                   |
|  [12]   | CSS `if()`, range syntax in `if()`                | 137, 142 | `media(prefers-contrast: more)` branches                   |
|  [13]   | `@function` custom functions                      |   139    | Repeated computed values                                   |
|  [14]   | `sibling-index()`, `sibling-count()`              |   138    | Staggered lists                                            |
|  [15]   | `@property`, `animation-play-state`               |  85, 43  | CSS lifetime timers                                        |
|  [16]   | Element-scoped view transitions                   |   147    | Transitions inside one subtree                             |
|  [17]   | Scroll-driven, scroll-triggered animations        | 115, 146 | Motion bound to scroll                                     |
|  [18]   | Customizable `<select>` (`base-select`)           |   134    | Styled native select in extension pages                    |
|  [19]   | `field-sizing`, `interpolate-size`, `calc-size()` | 123, 129 | Inputs and transitions to intrinsic sizes                  |
|  [20]   | `corner-shape`, `superellipse()`                  |   139    | Squircles where Chrome draws them, never on the round pill |
|  [21]   | `contrast-color()`                                |   147    | Black or white alone, never Chrome's tone pairs            |
|  [22]   | `text-box`                                        |   133    | Trimmed text boxes, unused where Chrome centres a line     |
|  [23]   | `reading-flow`, `reading-order`                   |   137    | Focus order across visual layouts                          |
|  [24]   | `moveBefore()`                                    |   133    | Moves that keep element state                              |
|  [25]   | `appendHTML` and positional HTML methods          |   157    | Trusted markup insertion, replacing `setHTML`              |
|  [26]   | `document.ariaNotify`                             |   141    | Announcements with no live region                          |
|  [27]   | `text-overflow: <string>`                         |   157    | Custom ellipsis text                                       |

Typed `attr()`, `progress()`, `@scope`, `@position-try`, `random()`, gap decorations, `::scroll-marker`, `shape()`, and scroll-state container queries release by 157 alike.

## [03]-[SCRIPT_FEATURES]

JavaScript at 157 with no flag: `Temporal`, `Iterator.concat`, `Iterator.zip`, `Math.sumPrecise`, `Map.prototype.getOrInsert`, `Uint8Array.fromBase64`, `using` with `DisposableStack`, `RegExp.escape`, `Error.isError`, `Float16Array`, `Promise.try`, `Array.fromAsync`. `ShadowRealm`, decorators, `Iterator.range`, iterator chunking, and `Math.clamp` stay absent under every flag.

Built-in AI answers in extension pages as in any page, on device with no server:
- `LanguageModel` (148) with image and audio input, `Summarizer` (138), `Translator` and `LanguageDetector` (138)
- `LanguageModel.availability()` returns `unavailable` in Chrome for Testing, as Playwright disables component updates and `OptimizationHints`
- `Writer`, `Rewriter`, `Proofreader`, Prompt API tool use, and embeddings are flag-gated since their origin trials ended

Later releases raise the minimum:
- 158: `<install>` and `navigator.install()`, `::highlight(*)`, media element pseudo-classes, `focus-without-user-activation` policy
- 159: `AccentColor` following `accent-color`, single-axis scroll snap, `CSSImportRule.styleSheet`
- 160: `corner` shorthands, `startViewTransition({ shadowRoots })`, Prompt API `samplingMode`, SVG Path Data API

## [04]-[EXTENSION_FEATURES]

Extension APIs released by 157, none behind a flag:
- `sidePanel` 114, `open()` 116, `getLayout()` 140, `close()` and `onOpened` 141, `onClosed` 142
- `userScripts` behind the per-extension Allow User Scripts toggle from 138, `execute()` 135
- `action.openPopup()` 127, `contextMenus` context `"tab"` 150
- `browser` namespace for every API 148, `"message_serialization": "structured_clone"` 148, `browser.publicSuffix` 153
- `navigator.clipboard` in the service worker 157, for workers holding `clipboardWrite`
- `trial_tokens` 126
- `icon_variants` (light and dark action icons) stays on Canary behind `ExtensionIconVariants`

## [05]-[ORIGIN_TRIALS]

Manifest `trial_tokens` holds tokens registered for `chrome-extension://<id>`, which needs a stable ID from manifest `key`. DevTools shows trial state for extension pages under Application > Frames, never for a service worker. Content scripts run in their page's origin, so a web feature there takes a third-party token injected as `<meta http-equiv="origin-trial">`.

## [06]-[DEVELOPMENT_FLAGS]

Flags worth enabling in the owner's Chrome while building extensions, set through `chrome://flags` with `tool:chrome`:

| [INDEX] | [FLAG]                                        | [EFFECT]                                                       |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `enable-experimental-web-platform-features`   | Every Blink feature at status experimental, for prototypes     |
|  [02]   | `enable-javascript-harmony`                   | V8 `--harmony`: Immutable ArrayBuffer, `import defer`          |
|  [03]   | `enable-webmcp-testing`                       | `document.modelContext` and tool events                        |
|  [04]   | `devtools-protocol-monitor`                   | CDP log and command editor, for trying `Extensions.*` by hand  |
|  [05]   | `devtools-aria-live-recording`                | Live announcement recording with `ariaNotify` call sites       |
|  [06]   | `devtools-source-map-scopes-in-sources-panel` | Original scopes from source maps                               |
|  [07]   | `forced-colors`                               | Forced colors for web content, for high-contrast checks        |
|  [08]   | `page-colors-settings`                        | Page Colors control in Settings, Chrome's forced palettes      |
|  [09]   | `enable-force-dark`                           | Auto dark rendering, for checking UI against forced dark pages |
|  [10]   | `import-export-flags`                         | `exportFlags()` and `importFlags()` without ending the browser |

Flag behavior an extension meets:
- `extensions-on-chrome-urls` does nothing in Google Chrome, as `DisableExtensionsOnChromeUrlsSwitch` is on in branded builds, and works in Chrome for Testing
- `extensions-on-extension-urls` opens other extensions' pages to scripts, and stores never grant it
- Browser UI flags (`glass-frame`, `webui-refresh-2026`, `toolbar-glow-up`) restyle Chrome's frame and the popup and side panel containers, never extension documents
- Flag-gated CSS at 158 open to prototypes: `@mixin` and `@apply`, `:has-slotted`, menu elements, `<input type=checkbox switch>`, `navigator.preferences`, `grid-lanes`, `line-clamp`

## [07]-[TEST_BROWSER_SWITCHES]

Playwright launches Chrome for Testing with its own defaults first, then the caller's `args`:
- Chromium keeps the last `--enable-features` switch alone, so a caller's list restates Playwright's `CDPScreenshotNewSurface,SkipListAccountsWithoutGaiaCookies`
- Caller `--disable-features` restates Playwright's default list the same way
- Playwright's WebMCP methods read the literal `--enable-features=` arguments and throw unless one lists `WebMCP`
- `--disable-field-trial-config` drops field trial states, so a test browser holds defaults and listed switches alone
- Headless runs pass `--hide-scrollbars`, and browser UI and DevTools flags change nothing there

Switches matching the owner's flags, verified in Chrome for Testing:
- `enable-experimental-web-platform-features` -> `--enable-experimental-web-platform-features`
- `enable-javascript-harmony` -> `--javascript-harmony`
- `enable-future-v8-vm-features` -> `--js-flags=--future`
- `enable-webmcp-testing` -> feature `WebMCP`
- `overlay-scrollbars` overlay -> feature `OverlayScrollbar:mode/overlay`
- Built-in AI flags -> features `AIPromptAPI`, `AIPromptAPIParams`, `AIPromptAPIToolUse`, `AIWriterAPI`, `AIRewriterAPI`, `AIProofreadingAPI`, `AIEmbeddingsAPI`, `AISummarizationAPI`
- `local-network-access-check` disabled -> `LocalNetworkAccessChecks,LocalNetworkAccessChecksWebSockets,LocalNetworkAccessChecksWebTransport` in `--disable-features`

Suites pass switches through `use.launchOptions.args` in `playwright.config.ts`, which the `loaded` fixture forwards.
