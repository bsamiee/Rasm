---
name: extensions
description: "Use when writing or reviewing a Chrome extension, covering manifest and permissions, contexts, service worker, content scripts, messages, storage, UI elements in Chrome's own look, suites, and Web Store release"
---

# [CHROME_EXTENSIONS]

Covers Manifest V3 extensions for Google Chrome.

[REFERENCES]:
- [01]-[APIS](references/apis.md): Extension APIs and manifest keys outside the common set
- [02]-[APPEARANCE](references/appearance.md): Chrome's colors, type, control sizes, and container geometry
- [03]-[RELEASES](references/releases.md): Features by Chrome release and development flags
- [04]-[SUITES](references/suites.md): Test browser behavior under Playwright and CDP
- [05]-[STORE](references/store.md): Chrome Web Store policies, review, API, and listing

Use `typescript:coding` for code design, `web:css` for CSS, and `chrome:browser` for the owner's Chrome.

## [01]-[PROJECT]

Extensions are products at `products/apps/chrome/<name>/`, a private `@rasm/<name>` package with no `project.json`, beside a suite at `tests/products/apps/chrome/<name>/`:
- `manifest.json` holds every value Chrome and the Web Store read, and build, service worker, and suite import it
- `_locales/<locale>/messages.json` holds every string a user reads
- `service-worker.ts` holds listeners, and `<function>.ts` with `<function>.css` holds each function it injects into a page
- `<page>.html` with `<page>.ts` and `<page>.css` holds one extension page (popup, side panel, options)
- `icons/mark.svg` and `icons/mark-16.svg` hold the drawing, and `package.json` `config.brand` its color
- `README.md` holds name, one line per behavior a test proves, and Web Store dashboard answers
- `tsconfig.app.json` takes `types: ["chrome", "vite/client", "web"]`, and `tsconfig.node.json` holds `vite.config.ts`
- Use `docs/typescript/Workspace.md` for `imports` rows, `docs/typescript/Testing.md` for suite targets, and `docs/Icons.md` for marks

Code calls `browser.<api>`, the same objects `chrome.<api>` names, and type names keep `chrome.` (`chrome.runtime.PlatformInfo`).

`vite.config.ts` exports `chrome(import.meta.dirname)` of `@rasm/vite/chrome`, which builds an extension from its `manifest.json`:
- Entries are the service worker `background.service_worker` names and each page `action.default_popup`, `side_panel.default_path`, and `options_ui.page` name
- `build.target` is `chrome<minimum_chrome_version>`, and each entry writes at build root under the name its manifest key holds
- `manifest.json` and each `_locales` file emit unchanged
- `icons` PNGs paint from `icons/mark.svg` and each `icons/mark-<grid>.svg` in `config.brand`, 128px padding 16px around its drawing
- Imported assets inline as data URLs, as injected code reaches no package file
- Chunks shared with the service worker load through static `import`
- `checks()` of `@rasm/vite` fails a build on Rolldown warnings of output that runs wrong and resolves `#/` imports carrying a Vite query

Each change builds, loads, and tests in order:
1. `nx build @rasm/<name>` writes the unpacked extension to `.artifacts/vite/@rasm/<name>`
2. `chrome:browser` installs each first build in the owner's Chrome and reloads it after later builds
3. `chrome://extensions` Errors lists what the service worker and extension pages threw, and a tab's console prints what its content scripts threw
4. `nx test @rasm/test-<name>` runs its suite against the same folder in Chrome for Testing

Manifest, service worker, and `_locales` edits load on an extension reload, content scripts on an extension reload then a page reload, and extension pages on reopening.

## [02]-[MANIFEST]

`manifest.json` states one value per key:

| [INDEX] | [KEY]                                               | [VALUE]                                                                        |
| :-----: | :-------------------------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `manifest_version`                                  | `3`                                                                            |
|  [02]   | `name`, `description`                               | `__MSG_<key>__`, name within 75 characters and description within 132          |
|  [03]   | `version`                                           | 1 to 4 integers from 0 to 65535 joined by dots, rising on each upload          |
|  [04]   | `default_locale`                                    | Locale folder holding every key                                                |
|  [05]   | `minimum_chrome_version`                            | Newest release among features code calls, as a string                          |
|  [06]   | `icons`                                             | PNG at 16, 32, 48, and 128                                                     |
|  [07]   | `action`                                            | `default_title`, and `default_popup` where a click opens a page                |
|  [08]   | `background`                                        | `service_worker` as built file name, `type: "module"`                          |
|  [09]   | `commands`                                          | Command names to `suggested_key` and `description`                             |
|  [10]   | `permissions`, `host_permissions`                   | Names and match patterns granted at install                                    |
|  [11]   | `optional_permissions`, `optional_host_permissions` | Names and patterns code requests at a user gesture                             |
|  [12]   | `side_panel`                                        | `default_path` of a page, with permission `sidePanel`                          |
|  [13]   | `options_ui`                                        | `page`, with `open_in_tab: false` for a page embedded in `chrome://extensions` |
|  [14]   | `content_scripts`                                   | Scripts and stylesheets by match pattern                                       |
|  [15]   | `web_accessible_resources`                          | Files a page or another extension loads, by match pattern or extension ID      |
|  [16]   | `key`                                               | Web Store dashboard public key, fixing one ID for item and unpacked builds     |

Chrome applies each key by its own rule:
- `minimum_chrome_version` at or above a feature's release lets code call it with no feature test, polyfill, or fallback
- Feature tests stay where a capability varies by OS or page policy (Web Share is absent on Linux and under `Permissions-Policy: web-share=()`)
- Raising `minimum_chrome_version` stops updates on older Chrome with no notice to its user
- `action` must exist for `browser.action` to be defined, and `default_popup` keeps `action.onClicked` from firing
- `_execute_action` in `commands` fires `action.onClicked`, and one listener serves click and shortcut
- `suggested_key` takes `default`, `mac`, `windows`, `linux`, and `chromeos`, each shortcut holding `Ctrl`, `Alt`, or `Command` on `mac`
- At most 4 commands suggest a key, and `chrome://extensions/shortcuts` rebinds each one
- `icons` and `action.default_icon` take raster images alone
- Extension IDs are the first 32 hex digits of a SHA-256 over `key`, mapped `0-f` to `a-p`, and an unpacked build with no `key` takes one from its path
- `"message_serialization": "structured_clone"` sends `Map`, `Set`, `Date`, `BigInt`, `Error`, `File`, and `Blob` in messages, where JSON is the default
- `content_security_policy.extension_pages` takes `'self'`, `'wasm-unsafe-eval'` for Wasm, and localhost sources, for `script-src` alone
- Inline scripts, `eval`, `new Function`, and code fetched at runtime never run in an extension page
- Files listed in `web_accessible_resources` let a matching page detect the extension, and `use_dynamic_url` serves them under an ID each session changes

## [03]-[PERMISSIONS]

Each permission grants one capability, and Chrome shows its warning at install:

| [INDEX] | [PERMISSION]              | [GRANTS]                                                 | [WARNING]                                     |
| :-----: | :------------------------ | :------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `activeTab`               | Host access to a tab its user acts on                    | None                                          |
|  [02]   | `scripting`               | Script and CSS injection into tabs with host access      | None                                          |
|  [03]   | `storage`, `alarms`       | Same-named API                                           | None                                          |
|  [04]   | `sidePanel`, `offscreen`  | Same-named API                                           | None                                          |
|  [05]   | `contextMenus`            | Context menu items                                       | None                                          |
|  [06]   | `unlimitedStorage`        | Storage past 10 MB                                       | None                                          |
|  [07]   | `clipboardWrite`          | Clipboard writes from service worker and pages           | Modify data you copy and paste                |
|  [08]   | `clipboardRead`           | Clipboard reads from an offscreen document               | Read data you copy and paste                  |
|  [09]   | `tabs`                    | `url`, `title`, `favIconUrl` of tabs with no host access | Read your browsing history                    |
|  [10]   | `webNavigation`           | Navigation events                                        | Read your browsing history                    |
|  [11]   | Host permissions          | Standing access to matching sites                        | Read and change your data on sites            |
|  [12]   | `declarativeNetRequest`   | Block, allow, and upgrade rules with no host access      | Block content on any page                     |
|  [13]   | `notifications`           | OS notification center                                   | Display notifications                         |
|  [14]   | `nativeMessaging`         | Messages to a native host                                | Communicate with cooperating native apps      |
|  [15]   | `debugger`, `pageCapture` | Same-named API                                           | Read and change all your data on all websites |
|  [16]   | `tabCapture`, `proxy`     | Same-named API                                           | Read and change all your data on all websites |

Permissions follow what code calls:
- Required permissions hold what every use needs, and an update adding a warning disables the extension until its user accepts
- `permissions.request({ permissions, origins })` asks for optional ones inside a user gesture and prompts only when a warning changes
- `debugger`, `declarativeNetRequest`, `devtools`, `geolocation`, `proxy`, `tts`, and `ttsEngine` stay required, as Chrome refuses them as optional
- `activeTab` with `scripting` covers an action on the current page with no host permission or `tabs`
- Action clicks, command shortcuts, context menu clicks, and accepted omnibox suggestions grant `activeTab` for the tab they act on
- `activeTab` grants start once a page painted or finished loading, last across same-origin navigations, and end at a cross-origin one
- Events before first paint and on `chrome://` pages, the Web Store, and other extensions' pages fire with no `tab.url` and no script access
- Clicks inside a popup, side panel, or injected UI grant nothing
- Users can withhold host access per site, and code reads `permissions.contains` before relying on a host permission
- `permissions.addHostAccessRequest({ tabId })` shows a request in the toolbar for a withheld host

## [04]-[CONTEXTS]

Each context holds its own APIs, lifetime, and limit:

| [INDEX] | [CONTEXT]                | [APIS]                                        | [LIFETIME]             | [LIMIT]                              |
| :-----: | :----------------------- | :-------------------------------------------- | :--------------------- | :----------------------------------- |
|  [01]   | Service worker           | Every API its manifest grants                 | Each event, until idle | No DOM, `matchMedia`, or built-in AI |
|  [02]   | Extension page           | Every API its manifest grants, DOM            | While shown            | Extension page CSP                   |
|  [03]   | Offscreen document       | `runtime` messaging, DOM                      | Until closed           | Reasons it declared at creation      |
|  [04]   | Content script, isolated | `runtime` messaging, `storage`, `i18n`, `dom` | Its document           | Own globals                          |
|  [05]   | Content script, main     | None                                          | Its document           | Page CSP, page globals               |
|  [06]   | Sandbox page             | None                                          | While shown            | `postMessage` alone, `eval` allowed  |

Work goes to the context holding its API:
- Service workers hold work that outlives a page (alarms, commands, context menus, action clicks)
- Offscreen documents serve DOM work a service worker lacks: clipboard reads, `DOMParser`, `matchMedia`, audio, media streams, built-in AI
- `offscreen.createDocument({ url, reasons, justification })` opens the one document a profile holds, and `hasDocument()` reads whether it exists
- Offscreen documents playing audio close after 30s of silence
- Main world scripts run only where code needs a page's own objects, as page overrides of `navigator` or prototypes never reach an isolated world

## [05]-[SERVICE_WORKER]

Chrome starts a service worker for an event and stops it when idle:
- Idle workers stop after 30s, one event or API call stops at 5 minutes, and a `fetch` response stops at 30s
- Events, extension API calls, port messages, WebSocket traffic, and offscreen document messages restart the idle timer
- Listeners register synchronously at module top level, as Chrome dispatches a waking event to listeners present after first evaluation
- Top-level `await` anywhere in the worker's module graph fails its registration, and `import()` throws
- State between events sits in `browser.storage`, IndexedDB, or an event's own fields (`tab.url`), as module variables end with the worker
- Waits past an event go to `browser.alarms`, each alarm named by the key it acts on, as `setTimeout` ends with its worker
- Alarms fire no more often than once a second, hold at most 500 per extension, and last across browser sessions
- `runtime.onExtensionLoaded` fires on each load with reason `install`, `update`, `browser_update`, `enable`, `startup`, or `reload`
- `runtime.onInstalled` fires with reason `install`, `update`, `chrome_update`, or `shared_module_update`, and `runtime.onStartup` at profile start
- Context menu items persist, and `runtime.onInstalled` creates them once
- Updates install only while the worker is idle

Each event runs one Effect program through `Effect.runFork`, and its typed failure picks what a user sees:

```ts
class ActionError extends Schema.TaggedError<ActionError, { readonly brand: unique symbol }>()('ActionError', { reason: Schema.Literals(['unreadable', 'refused']), tabId: Schema.Int, cause: Schema.Defect() }) {}

const marked = ({ reason, tabId }: ActionError) =>
    Effect.promise(() => Promise.all([browser.action.setBadgeText({ tabId, text: '!' }), browser.action.setTitle({ tabId, title: browser.i18n.getMessage(reason) }), browser.alarms.create(`${tabId}`, { delayInMinutes: Duration.toMinutes(Duration.seconds(4)) })]));

browser.action.onClicked.addListener(({ id: tabId = browser.tabs.TAB_ID_NONE, url }) => {
    Effect.gen(function* () {
        yield* Effect.mapError(Effect.fromNullishOr(url), (cause) => ActionError.make({ reason: 'unreadable', tabId, cause }));
        return yield* Effect.tryPromise({
            try: () => browser.scripting.executeScript({ target: { tabId }, func: shown, args: [{ text: browser.i18n.getMessage('shown'), lang: browser.i18n.getUILanguage() }], injectImmediately: true }),
            catch: (cause) => ActionError.make({ reason: 'refused', tabId, cause }),
        });
    }).pipe(Effect.catch(marked), Effect.runFork);
});
```

`shown` is a function the worker injects, and `unreadable`, `refused`, and `shown` are `_locales` keys:
- Extension APIs return promises that reject on an API error, entered through `Effect.promise` or `Effect.tryPromise`
- Action work finishes before its UI injects, as a refused injection then loses no result
- `action.setTitle({ tabId, title })` states each reason, as a badge shows only while its action is pinned
- Badge and title set for a tab last until it closes, and an alarm named by tab ID clears them and restores `action.getTitle({})`

## [06]-[CONTENT_SCRIPTS]

Content scripts run in an isolated world unless `world` names `MAIN`, and their injection form decides when they run and what they need:

| [INDEX] | [FORM]       | [DECLARED_BY]                      | [RUNS]                                | [NEEDS]                                |
| :-----: | :----------- | :--------------------------------- | :------------------------------------ | :------------------------------------- |
|  [01]   | Programmatic | `scripting.executeScript`          | Once, at a user's action              | `activeTab` and `scripting`            |
|  [02]   | Dynamic      | `scripting.registerContentScripts` | Each matching load after registration | Host permission                        |
|  [03]   | Static       | Manifest `content_scripts`         | Each matching load from install       | Match patterns with an install warning |

Programmatic injection suits an extension acting at a user gesture, and a dynamic declaration follows an optional host grant:
- Chrome serializes `func` and runs its source alone, and `func` closes over nothing, imports nothing, and takes every input in `args`
- `args` and results are JSON values: CSS as a string from a `?inline` import, text from `browser.i18n`, images as data URLs
- `injectImmediately: true` runs in a page that painted while still parsing
- `target` takes `tabId` with `allFrames`, `frameIds`, or `documentIds`, and results list the main frame first
- `executeScript` rejects on a page with no host access
- `scripting.insertCSS({ target, css, origin })` adds a stylesheet to a page in origin `AUTHOR` or `USER`, and `removeCSS` takes it back
- Isolated worlds share a page's DOM and keep their own globals across injections into one document
- Repeat injections find what an earlier one built (`document.querySelector('<name>-<role>') ?? document.createElement('<name>-<role>')`)
- Handlers set as properties (`onclick`) replace each other, where `addEventListener` adds one per injection
- Content scripts load as classic scripts, and each dynamic or static one builds as a self-contained file
- `content_scripts` rows take `matches`, `exclude_matches`, `js`, `css`, `run_at`, `all_frames`, `match_origin_as_fallback`, and `world`
- `run_at` takes `document_start`, `document_end`, or `document_idle`, the default
- Dynamic declarations last across sessions, and `updateContentScripts` and `unregisterContentScripts` change them by `id`
- Isolated worlds run under their extension's own CSP, which blocks `eval` whatever a page's CSP allows
- `fetch` from a content script follows its page's CORS rules whatever host permission holds, and cross-origin requests run in the service worker
- Content scripts of an earlier load lose `browser.runtime` once their extension reloads

## [07]-[MESSAGES]

Content scripts and offscreen documents reach an API through a message to the context holding it, and extension pages call `browser.*` directly:

```ts
const Request = Schema.TaggedUnion({
    Saved: { url: Schema.URLFromString, note: Schema.String },
    Cleared: {},
});

const applied = Request.match({
    Saved: ({ url, note }) => Effect.promise(() => browser.storage.session.set({ [url.href]: note })),
    Cleared: () => Effect.promise(() => browser.storage.session.clear()),
});

browser.runtime.onMessage.addListener((message: unknown) => Effect.runPromise(Effect.flatMap(Schema.decodeUnknownEffect(Request)(message), applied)));
```

Listeners answer with the promise they return, and its rejection reaches each sender as an `Error`:
- One tagged union per extension decodes every message at its listener, one case per request
- `runtime.sendMessage` reaches the service worker and extension pages, and `tabs.sendMessage(tabId, message, { frameId })` one frame's content scripts
- Ports (`runtime.connect`, `runtime.onConnect`) stream messages from one context to another and close when either end goes away
- Content script messages are untrusted input, and each URL, permission, and script a privileged context runs comes from its own code
- Extension contexts match `sender.id === browser.runtime.id` with `sender.origin` equal to `location.origin`, whatever `sender.tab` holds

## [08]-[STORAGE]

`browser.storage` areas differ in lifetime, size, and which contexts read them:

| [INDEX] | [AREA]    | [LIFETIME]                               | [SIZE]                              | [CONTENT_SCRIPTS] |
| :-----: | :-------- | :--------------------------------------- | :---------------------------------- | :---------------- |
|  [01]   | `session` | Memory, until restart, reload, or update | 10 MB                               | Refused           |
|  [02]   | `local`   | Disk, until uninstall                    | 10 MB, lifted by `unlimitedStorage` | Allowed           |
|  [03]   | `sync`    | Signed-in account, each device           | 100 KB, 8 KB per item, 512 items    | Allowed           |
|  [04]   | `managed` | Policy, read-only                        | Schema in `storage.managed_schema`  | Allowed           |

Storage holds settings a user chose and state a service worker reads back:
- `sync` takes 120 writes a minute and 1800 an hour and rejects a write past either
- `setAccessLevel({ accessLevel: 'TRUSTED_CONTEXTS' })` on `local` or `sync` refuses content scripts, the default of `session`
- Readers decode stored values with `Schema` at each read, as an earlier version or another device wrote them
- `storage.onChanged` tells each open page of a write, and pages render from storage with no message between them
- Drafts a user returns to after a popup closes sit in `session`

## [09]-[UI_ELEMENTS]

Chrome draws a container around each user interface element, and a task picks its element:

| [INDEX] | [TASK]                           | [ELEMENT]                   | [CONTAINER]                                   |
| :-----: | :------------------------------- | :-------------------------- | :-------------------------------------------- |
|  [01]   | Act on a page at one gesture     | `action.onClicked`, command | None                                          |
|  [02]   | Short task with its own controls | Action popup                | Bubble                                        |
|  [03]   | Task beside a page as it changes | Side panel                  | Panel under a Chrome-drawn header             |
|  [04]   | Settings                         | `options_ui` page           | Dialog on `chrome://extensions`               |
|  [05]   | State of a tab or a refusal      | Action badge and title      | Action icon                                   |
|  [06]   | Command on a selection or link   | Context menu item           | Chrome's menu, 6 top-level items on an action |
|  [07]   | Command typed in the address bar | `omnibox` keyword           | Chrome's suggestion rows                      |
|  [08]   | Event after its moment passed    | `notifications`             | OS notification center                        |
|  [09]   | Confirmation on a page           | Injected UI                 | Page bounds below Chrome's toolbar            |

Chrome sets each element's behavior:
- Popups stay hidden until first load ends, size to their content, take focus when shown, and close on losing it or on `window.close()`
- Popups leave Escape to their page
- `action.openPopup()` needs a focused window whose active tab has a popup, and `action.setPopup({ tabId, popup })` sets one tab's popup
- Side panels show one page for every tab until `sidePanel.setOptions({ tabId, path, enabled })` gives a tab its own
- `sidePanel.setPanelBehavior({ openPanelOnActionClick: true })` opens a side panel from the action on every tab and keeps `action.onClicked` from firing
- `sidePanel.open({ tabId })` runs inside a user gesture alone, and `onOpened` and `onClosed` report each change
- Embedded options pages sit outside any tab, with no `tabs` API and no `sender.tab`
- `runtime.openOptionsPage()` opens an options page, in a tab under `open_in_tab: true`
- Chrome's own toast has no extension API, and injected UI confirms an action on a page that takes scripts, badge and title elsewhere

## [10]-[APPEARANCE]

Extension pages match Chrome's own pages by taking its type, baseline colors, and control sizes, as no API exposes a user's theme:
- Chrome styles every extension page with `body { font-family: system-ui, sans-serif; font-size: 75% }`, and a page sets its type on `body`
- Extension pages declare `color-scheme: light dark`, as Chrome sets none and its Light, Dark, or Device setting then decides `light-dark()`
- Form controls take `font: inherit`, as Chrome's default gives them 13.33px Arial
- `chrome://theme/colors.css` and `chrome://resources` refuse an extension page, and no `theme` API exists
- Popups and embedded options pages take Chrome's page tokens, and side panel pages its side panel tokens
- Brand color goes in action icon and store listing, and extension UI takes primary as its one accent
- Icons are 20px Material Symbols in the rounded style Chrome's pages draw, each a CSS `mask` over `currentColor`
- Native `<dialog>`, `popover`, `<details>`, customizable `<select>`, and `field-sizing: content` supply behavior Chrome's own elements build on
- No package publishes Chrome's own elements, and native ones take its values through CSS

## [11]-[INJECTED_UI]

Injected UI shares a document with a page's styles and scripts:
- One host element named `<name>-<role>` under `document.documentElement` holds an open shadow root and one adopted `CSSStyleSheet`
- `:host { all: initial !important }` keeps page rules off its host, and the root element inside declares `color-scheme` again
- `popover="manual"` draws injected UI in the top layer, above page stacking, transforms, and `overflow`
- Sizes take `em` from the initial font size, as `rem` reads a page's root
- `@property` rules register nothing inside a shadow root
- `pointer-events: none` on UI with no control passes clicks to its page, and a control inside sets `auto`
- `style.zoom = 1 / zoom` from `browser.tabs.getZoom(tabId)` holds a fixed size under page zoom, as Chrome's own UI ignores page zoom
- `element.ariaNotify(text)` announces a change, where a live region inserted with its text announces unreliably
- Open shadow roots are readable by a page's own scripts, and secrets and typed input go in an extension page
- SVG and XML documents refuse injected HTML elements

## [12]-[LOCALES]

Each `messages.json` entry holds a `message` and a `description` naming where it shows and its limits:
- Code reads `browser.i18n.getMessage('<key>')`
- Extension pages and injected UI set `lang` from `browser.i18n.getUILanguage()` and `dir` from `getMessage('@@bidi_dir')`
- Wording takes Chrome's own string where one names the same thing
- Code types keys from the default locale (`import type messages from '#/_locales/en/messages.json'`), and a missing key fails type checks

## [13]-[SUITES]

Suites load the built extension in Chrome for Testing and assert what a user receives:
- `it` extends `loaded` of `@rasm/testing/playwright` in a suite's `fixtures.ts`
- `package.json` devDependency `"@rasm/<name>": "workspace:*"` lets `loaded` find its product's build and `nx affected` reach the suite
- `extension.act(page)` presses the action on a page's tab, `toolbar(page)` reads its badge and title, and `stop(page)` stops its service worker
- `extension.worker.evaluate` runs in the service worker, and `extension.evaluate(page, expression)` in a page's isolated world
- Extension pages open as tabs at `chrome-extension://<id>/<page>.html`, and fixture `axe` scans each one in both color schemes
- Each README behavior line has one test, written from its line and seen failing against a build with its fault injected
- Cases cover pages refusing scripts, a first action after a worker stop, both color schemes, more contrast, forced colors, reduced motion, and zoom

## [14]-[RELEASE]

Nx targets build, version, and publish an extension from its manifest:
- `build--chrome` zips the build folder, and `nx release` writes `version` in `manifest.json`
- `nx release publish` uploads each zip to the Chrome Web Store API, waits for its upload state, and submits it for review
- Use `docs/Workflows.md` for release credentials and `docs/typescript/Testing.md` for store media
