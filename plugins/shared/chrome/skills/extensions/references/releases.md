# [RELEASES]

## [01]-[INTERFACE_FEATURES]

Extension UI takes each CSS and DOM feature from its first release, and chromestatus holds each web feature's status:

| [INDEX] | [FEATURE]                                         | [CHROME] | [USE]                                               |
| :-----: | :------------------------------------------------ | :------: | :-------------------------------------------------- |
|  [01]   | `popover`, top layer                              |   114    | UI above page stacking                              |
|  [02]   | `popover="hint"`                                  |   133    | Tooltips, closed by light dismiss                   |
|  [03]   | `@starting-style`                                 |   117    | Entry start state                                   |
|  [04]   | `transition-behavior: allow-discrete`, `overlay`  |   117    | Exit that keeps `display` and top layer             |
|  [05]   | `light-dark()` colors, images                     | 123, 150 | Scheme-dependent tokens and icons                   |
|  [06]   | Anchor positioning, `position-area`               | 125, 129 | UI anchored to an element                           |
|  [07]   | Invoker commands `command`, `commandfor`          |   135    | Popover and dialog control from markup              |
|  [08]   | `interestfor` interest invokers                   |   142    | Hover and focus invokers                            |
|  [09]   | Dialog `closedby`, `:open`                        | 134, 133 | Light dismiss rules, open state selector            |
|  [10]   | CSS `if()`, range syntax in `if()`                | 137, 142 | `media()` and `style()` branches in one declaration |
|  [11]   | `@function` custom functions                      |   139    | Repeated computed values                            |
|  [12]   | `sibling-index()`, `sibling-count()`              |   138    | Staggered lists                                     |
|  [13]   | Element-scoped view transitions                   |   147    | Transitions inside one subtree                      |
|  [14]   | Scroll-driven, scroll-triggered animations        | 115, 146 | Motion bound to scroll                              |
|  [15]   | Customizable `<select>` (`base-select`)           |   134    | Styled native select                                |
|  [16]   | `field-sizing`, `interpolate-size`, `calc-size()` | 123, 129 | Inputs and transitions to intrinsic sizes           |
|  [17]   | `corner-shape`, `superellipse()`                  |   139    | Squircle corners                                    |
|  [18]   | `contrast-color()`                                |   147    | Black or white text over a color                    |
|  [19]   | `text-box`                                        |   133    | Trimmed text boxes                                  |
|  [20]   | `reading-flow`, `reading-order`                   |   137    | Focus order across visual layouts                   |
|  [21]   | `moveBefore()`                                    |   133    | Moves that keep element state                       |
|  [22]   | `Element.ariaNotify`, `document.ariaNotify`       |   141    | Announcements with no live region                   |
|  [23]   | `appendHTML` and positional HTML methods          |   157    | Trusted markup insertion                            |
|  [24]   | `position-visibility: anchor-valid`               |   157    | Anchored UI hidden when its anchor leaves           |
|  [25]   | `text-overflow: <string>`                         |   157    | Custom ellipsis text                                |

Later releases add features:
- Chrome 158 adds `<install>` and `navigator.install()`, `::highlight(*)`, media element pseudo-classes, and policy `focus-without-user-activation`
- Chrome 159 adds `AccentColor` following `accent-color`, single-axis scroll snap, and `CSSImportRule.styleSheet`
- Chrome 160 adds `corner` shorthands, `startViewTransition({ shadowRoots })`, and SVG Path Data API

## [02]-[SCRIPT_FEATURES]

Chrome 157 runs `Temporal`, `Iterator.concat`, `Iterator.zip`, `Math.sumPrecise`, `Map.prototype.getOrInsert`, `Uint8Array.fromBase64`, `using` with `DisposableStack`, `RegExp.escape`, `Error.isError`, `Float16Array`, `Promise.try`, `Array.fromAsync`, and `URL.parse` with no flag. `ShadowRealm`, decorators, `Iterator.range`, and `Math.clamp` stay absent under every flag.

Built-in AI runs on device with no server:
- `LanguageModel` is available in extensions from 138 and in web pages from 148, with image and audio input
- `Summarizer`, `Translator`, and `LanguageDetector` are available from 138
- `LanguageModel.params()`, `topK`, and `temperature` are available in extensions alone
- `Writer`, `Rewriter`, `Proofreader`, Prompt API tool use, and embeddings stay behind flags
- Models need 22 GB of free disk and either more than 4 GB of video memory or 16 GB of memory
- `LanguageModel.availability()` returns `unavailable` in Chrome for Testing

## [03]-[EXTENSION_FEATURES]

Each extension platform change reached stable, with no flag, in its release:

| [INDEX] | [CHROME] | [CHANGE]                                                                                                |
| :-----: | :------: | :------------------------------------------------------------------------------------------------------ |
|  [01]   |   114    | `sidePanel`, port messages restarting a service worker's idle timer                                     |
|  [02]   |   116    | `sidePanel.open()`, `runtime.getContexts()`, WebSocket traffic restarting the idle timer                |
|  [03]   |   119    | `scripting` `matchOriginAsFallback`                                                                     |
|  [04]   |   120    | `readingList`                                                                                           |
|  [05]   |   124    | `tts.onVoicesChanged`                                                                                   |
|  [06]   |   125    | `debugger.DebuggerSession`                                                                              |
|  [07]   |   126    | `trial_tokens`                                                                                          |
|  [08]   |   127    | `action.openPopup()`                                                                                    |
|  [09]   |   128    | `declarativeNetRequest` condition `responseHeaders`                                                     |
|  [10]   |   129    | `devtools.performance`                                                                                  |
|  [11]   |   130    | `action.onUserSettingsChanged`, `storage` `getKeys()`                                                   |
|  [12]   |   132    | Extension storage viewer in DevTools, `tabs.Tab.frozen`, `cookies.getPartitionKey()`                    |
|  [13]   |   133    | `permissions.addHostAccessRequest()`, `userScripts` worlds by `worldId`                                 |
|  [14]   |   135    | `userScripts.execute()`                                                                                 |
|  [15]   |   137    | `tabGroups.TabGroup.shared`                                                                             |
|  [16]   |   138    | Allow User Scripts toggle per extension                                                                 |
|  [17]   |   139    | `--extensions-on-chrome-urls` and `--disable-extensions-except` ignored by Google Chrome                |
|  [18]   |   140    | `sidePanel.getLayout()`, `protocol_handlers`, `tabs` `splitViewId`                                      |
|  [19]   | 141, 142 | `sidePanel.close()` and `onOpened`, then `onClosed`                                                     |
|  [20]   |   143    | `runtime.getVersion()`                                                                                  |
|  [21]   |   145    | `declarativeNetRequest` condition `topDomains`                                                          |
|  [22]   |   146    | Throw or rejection in an `onMessage` listener rejecting its sender                                      |
|  [23]   |   148    | `browser` namespace, promise-returning `onMessage` listeners, `message_serialization`                   |
|  [24]   |   149    | `devtools` calls returning promises, `userScripts.execute()` syntax diagnostics                         |
|  [25]   |   150    | `contextMenus` context `"tab"`, `offscreen.hasDocument()`, alarm `persistAcrossSessions`                |
|  [26]   |   151    | `mimeHandler`                                                                                           |
|  [27]   |   152    | `browser` namespace for extensions declaring `devtools_page`                                            |
|  [28]   |   153    | `publicSuffix`, alarms as often as once a second                                                        |
|  [29]   |   155    | `runtime.onEnabled`, `tabs.createSplit()` and `unsplit()`                                               |
|  [30]   |   157    | `runtime.onExtensionLoaded`, `navigator.clipboard` writes in a service worker, `mimeHandler` `canEmbed` |
|  [31]   |   158    | `_favicon` exposed only where `web_accessible_resources` lists it                                       |

`icon_variants` (light and dark action icons) and `background.async_listener_registration` have not reached stable.

## [04]-[DEVELOPMENT_FLAGS]

Development flags change the owner's Chrome alone, set through `chrome://flags` with `chrome:browser`:

| [INDEX] | [FLAG]                                                        | [EFFECT]                                                      |
| :-----: | :------------------------------------------------------------ | :------------------------------------------------------------ |
|  [01]   | `enable-experimental-web-platform-features`                   | Every Blink feature at status experimental, for prototypes    |
|  [02]   | `enable-javascript-harmony`                                   | V8 `--harmony` features                                       |
|  [03]   | `enable-webmcp-testing`                                       | `document.modelContext` and tool events                       |
|  [04]   | `devtools-protocol-monitor`                                   | CDP log and command editor, for trying `Extensions.*` by hand |
|  [05]   | `devtools-aria-live-recording`                                | Recording of live region and `ariaNotify` announcements       |
|  [06]   | `enable-devtools-deep-link-via-extensibility-api`             | Performance trace entries linking into a DevTools extension   |
|  [07]   | `extension-install-site-access-options`                       | Site access choice in the install prompt                      |
|  [08]   | `web-request-security-info`                                   | Certificate details on `webRequest` events                    |
|  [09]   | `prompt-api`, `writer-api`, `rewriter-api`, `proofreader-api` | Built-in AI APIs behind flags                                 |

Released code calls only what default switches enable, and a suite at default switches proves it.

Flags restyling Chrome (`desktop-glow-up`, `webui-refresh-2026`, `toolbar-glow-up`, `glass-frame`) change its own pages and window frame, and popups, side panel containers, badges, toasts, and extension documents look alike under each one.

DevTools Rendering emulates forced colors, color scheme, contrast, and reduced motion with no flag.
