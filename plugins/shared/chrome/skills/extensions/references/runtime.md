# [RUNTIME]

## [01]-[SERVICE_WORKER]

Service workers start for an event and stop when idle, so nothing outlives an event in worker memory:
- Listeners register synchronously at module top level, as Chrome dispatches a waking event to listeners present after first evaluation
- Each event runs one Effect program through `Effect.runFork`, and its typed failure picks the fallback
- State between events lives in `chrome.storage` or derives from the event (`tab.url`, `tab.title`), never in module globals
- Waits inside an event run on `Effect.sleep`, as `chrome.alarms` floors at 30s, and a wait past worker idle time moves to an alarm
- `navigator.clipboard.writeText` writes from the worker for extensions holding `clipboardWrite`, checked per call
- Offscreen documents serve clipboard reads and DOM work alone, and a write needs none
- `chrome.runtime.getPlatformInfo()` picks OS branches, keyed by `chrome.runtime.PlatformOs` members
- Extension APIs return promises, and every call goes through `Effect.promise` or `Effect.tryPromise` with no callback form
- Service worker modules reject top-level `await` and `import()`

## [02]-[ACTIVE_TAB]

`activeTab` grants a tab a user acts on (action click, `_execute_action` shortcut, context menu, omnibox pick) host access for its page:
- Action listeners receive `tab.url` and `tab.title` under the grant, so no `tabs` permission is needed
- Grants arrive only once a page painted or finished loading, and a press before first paint gets no URL and no grant, dropped
- Grants persist across same-origin navigations and end at a cross-origin one
- `chrome://` pages, the Web Store, `view-source:`, other extensions' pages, and policy-blocked hosts take no grant
- Button clicks inside a popup, side panel, or page UI grant nothing

## [03]-[PAGE_SCRIPTING]

`chrome.scripting.executeScript` runs a function in a tab's isolated world:

```ts
chrome.scripting.executeScript({ target: { tabId }, func: toast, args: [{ css, text, lang, dir, zoom, shown }], injectImmediately: true });
```

`toast` is the injected function, `css` a stylesheet from a `?inline` import, and other fields values its worker read:
- Chrome serializes `func` and runs its source alone, so it closes over nothing, imports nothing, and takes every input in `args`
- `args` take JSON-serializable values: CSS as a string, text from `chrome.i18n`, glyphs as data URLs Vite inlines
- `injectImmediately: true` runs on a page that painted while still parsing
- Isolated worlds share the DOM and keep their own globals, which persist across injections into one document
- Page overrides of `navigator` or prototypes never reach an isolated world, and `world: 'MAIN'` stands only where page objects are needed
- Injected source type-checks against `dom` libraries in the app program, apart from worker types
- SVG documents refuse an injected element, and the refusal arrives as a rejected promise
- Declared `content_scripts` and host permissions stay out of an extension acting on user gestures alone

## [04]-[MESSAGES]

Injected functions return results through the `InjectionResult[]` of `executeScript`, and runtime messages serve pages that outlive one call:
- Messages decode at the boundary through one `Schema` tagged union, each tag mapped to its handler
- Trust follows `sender.origin` against `new URL(chrome.runtime.getURL('')).origin`, never whether `sender.tab` is set
- Page-UI messages count from the top frame alone (`sender.frameId === 0`)

## [05]-[FALLBACKS]

Extensions finish their job before drawing UI, and a refused UI never undoes it:
- Distinct failures take distinct `reason` cases of one tagged error (page unreadable, clipboard refused, UI refused)
- Refusals set `action.setBadgeText({ tabId, text })` with no color call, taking Chrome's themed `kColorSysNeutralContainer` and contrast text
- `action.setTitle({ tabId, title })` states each reason from `_locales`, as badges show only while an action is pinned
- Badge and title reset after confirmation lifetime, restoring global title from `action.getTitle({})`, as tab badges otherwise last until close

## [06]-[WEB_SHARE]

Page UI offers the system share sheet through `navigator.share` in its isolated world, under Chrome's rules:
- Share only `http:` and `https:` URLs, as sharing another scheme can end the renderer and `canShare` misses it
- Offer the control where `'share' in navigator` and `navigator.canShare(data)` both hold, as `canShare` can exist without `share`
- `data` holds `url`, with `title` only when a tab has one
- Linux Chrome holds no Web Share, macOS uses `NSSharingServicePicker`, Windows its system share sheet, ChromeOS its sharesheet
- `Permissions-Policy: web-share=()` makes `canShare` false and binds extensions alike
- Calls need an active tab in a visible window and a user activation from the last 5s
- Pending shares pause UI lifetime, and resolve or `AbortError` closes it
- Pickers on macOS open at pointer position with no page coordinates, and Chrome 158 shifts each right by page-area window offset (vertical tabs)
- Chrome 160 converts that point (commit `2a83d01`), and page CSS cannot correct the offset before it
- Glyphs follow the OS: Material Symbols `ios_share` on macOS, `share_windows` on Windows, `share` elsewhere

## [07]-[LOCALES]

`_locales/<lang>/messages.json` holds every string a user reads:
- Each entry takes `message` and a `description` naming its surface and limits for translators
- Manifest fields read `__MSG_<key>__`, and code reads `chrome.i18n.getMessage(key)`
- Injected UI sets `lang` from `chrome.i18n.getUILanguage()` and `dir` from `getMessage('@@bidi_dir')`
- Wording takes Chrome's own string where one exists ("Copy link", "Link copied")
- Code types message keys from JSON (`import type messages from '#/_locales/en/messages.json'`), so a missing key fails type checks

## [08]-[STORAGE]

Storage holds settings a user asks for and state that outlives a worker, as Chrome's own toast keeps none:
- `options_ui` with `open_in_tab: false` embeds a settings page in the extension's details page
- `chrome.storage.sync` holds 100 KB total, 8 KB per item, and 512 items, and counts as data leaving the device
- `chrome.storage.session` holds per-tab state across worker restarts and clears when the browser closes
- `chrome.storage.local.setAccessLevel({ accessLevel: 'TRUSTED_CONTEXTS' })` keeps stored values from isolated-world code
- Service workers read values per event, decode them with `Schema` at that boundary, and pass them into injected functions through `args`
