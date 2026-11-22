# [TESTING]

## [01]-[SUITE]

Suites sit at `tests/products/apps/chrome/<name>` and depend on the product they load:
- `package.json` devDependency `"@rasm/<name>": "workspace:*"` adds a project graph edge, so `nx affected` reaches the suite and `loaded` finds its build
- `playwright.config.ts` merges `config(import.meta.dirname)` with suite options, `contextOptions.permissions: ['clipboard-read']` for reading copies
- `fixtures.ts` extends `loaded` with the suite's fixtures, and specs import `it` from it
- Use `docs/typescript/Testing.md` for Playwright projects, Nx targets, and media output folders

## [02]-[EXTENSION_FIXTURE]

`loaded` from `@rasm/testing/playwright` opens a persistent Chrome for Testing context with the built extension and hands each test an `extension`:
- Channel `chromium` runs full Chrome for Testing, where `--load-extension` works, as branded Chrome ignores it and the headless shell starts no MV3 worker
- `extension.act(page)` presses the action through CDP `Extensions.triggerAction` on its tab target, a path toolbar clicks and `_execute_action` share
- `extension.toolbar(page)` reads badge text and title of its tab in the service worker
- `extension.stop(page)` stops service workers through `ServiceWorker.stopAllWorkers` and fails while that worker runs
- `extension.evaluate(page, expression)` runs in its isolated world on that page, an execution context with origin `chrome-extension://<id>`
- `extension.worker.evaluate` replaces worker APIs (`navigator.clipboard.writeText`, `chrome.runtime.getPlatformInfo`) and drives Chrome (`chrome.tabs.setZoom`)
- Tab targets resolve by URL, so each page in a test takes its own URL
- `Extensions.triggerAction` fails with "No action found" on an extension with no `action` key

## [03]-[PAGES]

Pages come from `context.route` on fake origins (`https://page.test/<path>`), each with the body and headers its case needs:
- Response headers set page policy (`permissions-policy: web-share=()`)
- `page.emulateMedia` sets `colorScheme`, `contrast`, `forcedColors`, and `reducedMotion`
- `setViewportSize` gives narrow windows, and `chrome.tabs.setZoom` page zoom confirmed through `devicePixelRatio`
- Routes that never fulfill a script hold a page painted and still parsing, reached with `waitUntil: 'commit'` and a first-contentful-paint wait
- `chrome://` pages, SVG documents, and new origins before first paint cover refusals
- `file:` tabs need file access through `chrome.developerPrivate.updateExtensionConfiguration({ extensionId, fileAccess: true })` on `chrome://extensions`
- Paths with non-ASCII characters, a query, and a fragment check URL serialization

## [04]-[STUBS]

Page APIs injected code calls get replaced in the isolated world, as page-world overrides never reach it:
- First injection creates an isolated world, so a stub step acts once, waits for UI, stubs, then hides that UI
- Stubs record calls in isolated globals tests read back (`globalThis.shares`) and hold promises tests settle
- Deleting from a prototype (`delete Navigator.prototype.share`) models a platform lacking that API
- Unstubbed cases against real Chromium prove each stub's premise (`canShare` true with no policy, false under `web-share=()`)
- Worker stubs needed before listeners register attach through `Target.setAutoAttach` with `waitForDebuggerOnStart`, then `Runtime.runIfWaitingForDebugger`

## [05]-[ORACLES]

Assertions read what Chrome rendered or the user receives:
- Clipboard through `navigator.clipboard.readText()` in the page, asserting serialized URLs
- `getComputedStyle` font, colors, and border, and `getBoundingClientRect` geometry against Chrome's own values
- Transition timing from `getAnimations()` `CSSTransition` effects at entry and exit
- Lifetime from `toggle` and `animationend` timestamps a `MutationObserver` records, installed before the action
- Pixel samples from 1px `page.screenshot` clips at `scale: 'css'`, within 3 per channel of the expected `color-mix`
- Glyph identity by the smallest mean channel gap against reference renders of each candidate glyph
- Roles and names (`getByRole('status')`, button names from `_locales`) in place of tag selectors
- Hit tests through `elementFromPoint` at a control's center, and page elements beneath the pill counting clicks
- Network failing closed (`--host-resolver-rules="MAP * ~NOTFOUND"` with a dead proxy) proving a no-data listing answer
- Extension pages (options, popup, side panel) scanned through the `axe` fixture in each scheme and at narrow widths
- `expect.poll` until running transitions reach zero, fixed waits standing only where elapsed time is the behavior

## [06]-[CASES]

Extension suites cover each case its product reaches, one test per row of a case table:
- First action after the service worker stopped
- Each refused page, its badge and title, and their reset after the confirmation lifetime
- Light and dark schemes, increased contrast, forced colors, reduced motion, page zoom, and a viewport narrower than the UI
- Page CSS that unsets every element and hides the root, and a page painted while still parsing
- Second action while the UI shows, restarting its lifetime with one host
- Tab, Enter, Space, and Escape on controls, Escape with focus outside, and focus return on every close
- Clicks passing through the pill to page elements beneath, and reaching a control
- Each OS branch through a replaced `chrome.runtime.getPlatformInfo`

Each test is written from the product's specification, and each one fails against a build with its fault injected before it counts.

## [07]-[MEDIA]

`store.media.ts` scenes render the product as a user sees it:
- Scene pages style text with `@fontsource-variable/*` packages served through `hosted()`, so Mac and Linux renders wrap alike
- `shows` names the role an action reveals, and `after` stubs and acts again to show a control before capture
- Scenes stub through the `isolate` operation tests use, with no product switch for media
- Injected UI renders in the capturing machine's `system-ui`, Linux's in CI
- Tiles draw from the built manifest's resolved `__MSG_name__` and `icons/mark.svg` colored by `icon.css`
