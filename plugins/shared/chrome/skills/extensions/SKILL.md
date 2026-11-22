---
name: extensions
description: "Use when writing or reviewing a Chrome extension, covering manifest and permissions, service worker, page UI in Chrome's own look, Playwright suites, and Web Store release"
---

# [CHROME_EXTENSIONS]

Covers Manifest V3 extensions for Google Chrome, from manifest to Chrome Web Store release.

Features follow the release `minimum_chrome_version` names, page UI values follow Chrome's own UI source, text comes from `_locales`, and every other value comes from an event or its caller.

[REFERENCES]:
- [01]-[MANIFEST](references/manifest.md): Manifest fields, permissions, and content security
- [02]-[RUNTIME](references/runtime.md): Service worker, activeTab, page scripting, fallbacks, sharing, locales, and storage
- [03]-[INTERFACE](references/interface.md): Page UI matching Chrome's toast in size, color, motion, and accessibility
- [04]-[PLATFORM](references/platform.md): Web and extension features by Chrome release, flags, and test browser switches
- [05]-[TESTING](references/testing.md): Playwright suites driving a built extension
- [06]-[DELIVERY](references/delivery.md): Project layout, build, release, and store listing

Use `typescript:coding` for code design, and `tool:chrome` for the owner's Chrome, its flags, and loading a build there.

## [01]-[PLATFORM]

Extensions use every feature their floor release includes:
- `minimum_chrome_version` names the floor, and Vite `build.target` reads `chrome${minimum_chrome_version}` from an imported manifest
- Features released by the floor run with no feature test, polyfill, or fallback, and a later feature raises it
- Feature tests stay where a capability varies by OS or page policy at the floor (Web Share on Linux, `web-share=()`)
- Code calls `chrome.*` typed by `@types/chrome`, as `browser.*` answers alike from 148

Forms Chrome replaced stay out:

| [INDEX] | [REPLACED]                                    | [CURRENT]                                            |
| :-----: | :-------------------------------------------- | :--------------------------------------------------- |
|  [01]   | Background page, `persistent`                 | Module service worker                                |
|  [02]   | `browserAction`, `pageAction`                 | `action`                                             |
|  [03]   | `webextension-polyfill`, callback APIs        | Promise-returning `chrome.*`                         |
|  [04]   | Offscreen document for a clipboard write      | `navigator.clipboard.writeText` in the worker        |
|  [05]   | `tabs.executeScript`, declared content script | `scripting.executeScript` under activeTab            |
|  [06]   | Keepalive ports, storage writes, alarm loops  | Event-scoped work, state in `chrome.storage`         |
|  [07]   | Live region shown with text                   | `document.ariaNotify`                                |
|  [08]   | Script timer and `remove()` for transient UI  | `popover`, `@starting-style`, CSS lifetime animation |
|  [09]   | `innerHTML`, `DOMParser` markup               | `createElement`, `replaceChildren`, `appendHTML`     |
|  [10]   | Popover and anchor polyfills, CDN imports     | Native top layer and anchor positioning              |

## [02]-[SURFACES]

Chrome draws each extension surface, and a job's nature picks its surface:

| [INDEX] | [JOB]                           | [SURFACE]                              | [CONDITION]                                  |
| :-----: | :------------------------------ | :------------------------------------- | :------------------------------------------- |
|  [01]   | Confirm a page action           | Page UI injected under activeTab       | Page takes scripts                           |
|  [02]   | Report state or a refusal       | Action badge and per-tab title         | Page refuses scripts or job fails            |
|  [03]   | Show text where pages refuse UI | `action.openPopup()` bubble at toolbar | Takes keyboard focus, needs an active window |
|  [04]   | Ongoing task beside the page    | `sidePanel`                            | Persistent, opened by a gesture              |
|  [05]   | Settings a user asks for        | `options_ui` page                      | Values in `chrome.storage.sync`              |
|  [06]   | Event after its moment passed   | `notifications`                        | OS notification center, warning at install   |

Chrome's own toast has no extension API, so page UI reproduces it inside page bounds, below the toolbar edge it never crosses.

## [03]-[REVIEW]

Reviews read an extension in this order, each answer from code or Chrome source:
1. Manifest: floor, permissions against calls, `action`, commands, locales, icons
2. Service worker: top-level listeners, no worker globals across events, one fallback per typed failure
3. Page scripting: self-contained function, idempotent host, isolated-world assumptions
4. Page UI: Chrome's values, environment colors, CSS motion and lifetime, contrast, forced colors, reduced motion, zoom
5. Accessibility: announcement, keyboard reach, focus return, pointer pass-through
6. Suite: real oracles, a case per refusal and media condition, cold-start action, a fault each test catches
7. Delivery: build values from manifest, media from suite, listing answers matching code
