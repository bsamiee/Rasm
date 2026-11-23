# [SUITES]

## [01]-[TEST_BROWSER]

`loaded` of `@rasm/testing/playwright` starts one Chrome for Testing per worker and loads the built extension fresh for each test:
- `channel: 'chromium'` runs full Chrome for Testing, as the headless shell has no `Extensions` CDP domain and starts no extension service worker
- `ignoreDefaultArgs: ['--disable-extensions']` drops a Playwright default under which `Extensions.loadUnpacked` returns an ID and loads nothing
- Google Chrome ignores `--load-extension`, and a CDP client loads a folder there through `Extensions.loadUnpacked`
- Playwright pins a Chrome for Testing build of its own that can run a release ahead of stable
- Each browser launch shows a Dock tile on macOS, headless included, and one launch serves every test of a worker
- Tab targets resolve by URL, and each page of a test takes its own URL

## [02]-[SWITCHES]

Playwright starts Chrome for Testing with its own switches first, then a caller's `args`:
- Chromium keeps the last `--enable-features` and `--disable-features` switch alone, and a caller's value restates Playwright's default list
- Playwright enables `CDPScreenshotNewSurface,SkipListAccountsWithoutGaiaCookies` by default
- `--disable-field-trial-config` drops field trial states, and a test browser holds code defaults and listed switches alone
- `--disable-sync` and no Google sign-in leave `storage.sync` on one device and `identity.getAuthToken` with no account
- `--extensions-on-chrome-urls` works in Chrome for Testing
- Playwright's WebMCP methods read the literal `--enable-features=` arguments and throw unless one lists `WebMCP`
- Suites pass switches through `use.launchOptions.args` in `playwright.config.ts`, which `loaded` forwards

Each flag of the owner's Chrome maps to a test browser switch:
- `enable-experimental-web-platform-features` -> `--enable-experimental-web-platform-features`
- `enable-javascript-harmony` -> `--javascript-harmony`
- `enable-future-v8-vm-features` -> `--js-flags=--future`
- `enable-webmcp-testing` -> feature `WebMCP`
- Built-in AI flags -> features `AIPromptAPI`, `AIPromptAPIToolUse`, `AIWriterAPI`, `AIRewriterAPI`, `AIProofreadingAPI`, `AISummarizationAPI`
- `web-request-security-info` -> feature `WebRequestSecurityInfo`
- `web-machine-learning-neural-network` -> feature `WebMachineLearningNeuralNetwork`
- `enable-isolated-web-apps`, `enable-isolated-web-app-dev-mode` -> features `IsolatedWebApps`, `IsolatedWebAppDevMode`

## [03]-[CDP]

CDP methods run on a session that reaches their target:

| [INDEX] | [METHOD]                             | [TAKES]                            | [DOES]                                                    |
| :-----: | :----------------------------------- | :--------------------------------- | :-------------------------------------------------------- |
|  [01]   | `Extensions.loadUnpacked`            | `path`                             | Loads a folder and returns its `id`                       |
|  [02]   | `Extensions.uninstall`               | `id`                               | Removes an unpacked extension                             |
|  [03]   | `Extensions.triggerAction`           | `id`, `targetId` of a tab target   | Presses an action as a click or shortcut does             |
|  [04]   | `Extensions.getExtensions`           | None                               | Lists unpacked extensions                                 |
|  [05]   | `Extensions.getStorageItems`         | `id`, `storageArea`, `keys`        | Reads `browser.storage` values                            |
|  [06]   | `Extensions.setStorageItems`         | `id`, `storageArea`, `values`      | Merges values into an area                                |
|  [07]   | `Extensions.clearStorageItems`       | `id`, `storageArea`                | Empties an area                                           |
|  [08]   | `ServiceWorker.stopAllWorkers`       | None, after `ServiceWorker.enable` | Stops every service worker                                |
|  [09]   | `ServiceWorker.startWorker`          | `scopeURL`                         | Starts a stopped worker                                   |
|  [10]   | `Animation.setPlaybackRate`          | `playbackRate`                     | Scales every animation's clock                            |
|  [11]   | `Page.setFontSizes`                  | `fontSizes.standard`               | Sets Chrome's Font size setting for one page              |
|  [12]   | `Emulation.setTouchEmulationEnabled` | `enabled`                          | Sets `(hover: none)`, mouse events still setting `:hover` |

Sessions differ by method:
- `Extensions` storage methods need a page or service worker session reaching the extension, and its other methods run on a browser session
- `Extensions.triggerAction` refuses a page target ID and takes the tab target holding its page
- `context.newCDPSession` takes a page or frame alone, and `ServiceWorker` methods go through a page session
- Playwright keeps one `Worker` object across a stop and start and fires no `serviceworker` event, and a wait on one hangs
- Calls on a stopped worker stall until it starts, and a read after `stopAllWorkers` calls `ServiceWorker.startWorker` first
- `Emulation.setEmulatedMedia` emulates no `hover` feature
- Worker stubs needed before listeners register attach through `Target.setAutoAttach` with `waitForDebuggerOnStart`

## [04]-[PAGES]

Pages come from `context.route` on `.test` origins (`https://page.test/<path>`), each case fulfilling its own body and headers:
- Response headers set page policy (`permissions-policy: <feature>=()`)
- `page.emulateMedia` sets `colorScheme`, `contrast`, `forcedColors`, and `reducedMotion`
- `browser.tabs.setZoomSettings(tabId, { scope: 'per-tab' })` before `setZoom` keeps zoom off other tests of its origin
- Page zoom scales `devicePixelRatio` and leaves computed `font-size` alone
- Routes that never fulfill a script hold a page painted and still parsing, reached with `waitUntil: 'commit'` and a first-contentful-paint wait
- `chrome://` pages, SVG documents, and new origins before first paint cover pages that refuse scripts
- Paths with non-ASCII characters, a query, and a fragment check URL serialization
- `file:` tabs need file access through `chrome.developerPrivate.updateExtensionConfiguration({ extensionId, fileAccess: true })` on `chrome://extensions`

## [05]-[STUBS]

Stubs replace an API in the world that calls it, and a page API injected code calls sits in its isolated world:
- First injection creates an isolated world, and a stub step acts once, waits for UI, stubs, then hides it
- Stubs record calls in isolated globals a test reads back and hold promises a test settles
- Deleting from a prototype (`delete Navigator.prototype.share`) models a platform with no such API
- Unstubbed cases against real Chromium prove each stub's premise
- `extension.worker.evaluate` replaces service worker APIs (`navigator.clipboard.writeText`, `browser.runtime.getPlatformInfo`)

## [06]-[ORACLES]

Assertions read what Chrome rendered, in place of a selector or notation the product spells:
- Clipboard through `navigator.clipboard.readText()` in a page, under `contextOptions.permissions: ['clipboard-read']`
- Colors by painting a computed color to a canvas and comparing bytes with Chrome's hex
- Geometry from `getBoundingClientRect`, and type from `getComputedStyle`
- Transition timing from `getAnimations()` effects, and lifetime by setting a `CSSAnimation`'s `currentTime`
- Time through Web Animations (`currentTime`, `pause()`) and CDP `Animation.setPlaybackRate`, a fixed wait only where elapsed time is the behavior
- `:popover-open` for a popover's state, as `toBeHidden` waits for `display: none` at the end of its exit
- Custom properties holding a `url("data:image/svg+xml,...")` read back percent-encoded with single-quoted attributes
- Roles and accessible names (`getByRole('status')`) in place of tag selectors
- Hit tests through `elementFromPoint` at a control's center, and page elements beneath UI set `pointer-events: none` counting clicks
- Requests failing closed (`--host-resolver-rules="MAP * ~NOTFOUND"`) proving an extension sends nothing
- `expect.poll` until running transitions reach zero

## [07]-[MEDIA]

`store.media.ts` scenes capture store screenshots from the built extension:
- Scene pages style text with `@fontsource-variable/*` packages served through `hosted()`, and each machine wraps lines alike
- `shows` names the role an action reveals, and `after` stubs and acts again before capture
- Scenes stub through operations a suite's tests use, with no switch in the product for media
- Text snaps to a whole device pixel before capture, as glyph raster at a fractional position differs between runs
- Use `docs/typescript/Testing.md` for `media(scenes)` and promo tiles
