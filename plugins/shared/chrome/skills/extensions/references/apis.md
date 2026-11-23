# [APIS]

## [01]-[NAMESPACES]

Each `browser.<namespace>` is available in a service worker and in extension pages once its permission or manifest key is declared:

| [INDEX] | [NAMESPACE]             | [DOES]                                                              | [NEEDS]                         |
| :-----: | :---------------------- | :------------------------------------------------------------------ | :------------------------------ |
|  [01]   | `action`                | Toolbar button: icon, title, badge, popup                           | Manifest `action`               |
|  [02]   | `alarms`                | Timed events that wake a service worker                             | `alarms`                        |
|  [03]   | `commands`              | Keyboard shortcuts, `onCommand` passing its tab                     | Manifest `commands`             |
|  [04]   | `contextMenus`          | Context menu items                                                  | `contextMenus`                  |
|  [05]   | `cookies`               | Cookie reads and writes                                             | `cookies`, host permissions     |
|  [06]   | `debugger`              | CDP sessions on tabs, frames, and workers                           | `debugger`                      |
|  [07]   | `declarativeNetRequest` | Request rules Chrome evaluates                                      | `declarativeNetRequest`         |
|  [08]   | `devtools.*`            | DevTools panels, inspected page, network                            | Manifest `devtools_page`        |
|  [09]   | `dom`                   | `openOrClosedShadowRoot(element)`                                   | None                            |
|  [10]   | `downloads`             | Download start and control, UI through `setUiOptions`               | `downloads`                     |
|  [11]   | `i18n`                  | Messages, UI language, language detection                           | None                            |
|  [12]   | `identity`              | OAuth tokens, `launchWebAuthFlow`                                   | `identity`                      |
|  [13]   | `mimeHandler`           | Handling `application/pdf` streams in place                         | Manifest `mime_types_handler`   |
|  [14]   | `notifications`         | OS notifications, `requireInteraction` and `silent` among options   | `notifications`                 |
|  [15]   | `offscreen`             | One hidden document with a DOM                                      | `offscreen`                     |
|  [16]   | `omnibox`               | Address bar keyword and suggestions, `onDeleteSuggestion`           | Manifest `omnibox`              |
|  [17]   | `permissions`           | Optional permission requests                                        | None                            |
|  [18]   | `publicSuffix`          | `getDomain`, `getKnownSuffix`, `isKnownSuffix`                      | `publicSuffix`                  |
|  [19]   | `readingList`           | Reading List entries                                                | `readingList`                   |
|  [20]   | `runtime`               | Lifecycle, messages, manifest, contexts                             | None                            |
|  [21]   | `scripting`             | Script and CSS injection, dynamic content scripts                   | `scripting`                     |
|  [22]   | `sidePanel`             | Side panel page                                                     | `sidePanel`                     |
|  [23]   | `storage`               | `session`, `local`, `sync`, `managed` areas                         | `storage`                       |
|  [24]   | `tabCapture`            | Media stream of a tab, `getMediaStreamId` for an offscreen document | `tabCapture`                    |
|  [25]   | `tabGroups`             | Tab groups                                                          | `tabGroups`                     |
|  [26]   | `tabs`                  | Tabs, zoom, messages to content scripts                             | None for most calls             |
|  [27]   | `tts`                   | Speech output                                                       | `tts`                           |
|  [28]   | `userScripts`           | User-supplied code in its own world                                 | `userScripts`, host permissions |
|  [29]   | `webNavigation`         | Navigation events by frame, `documentId`, and `documentLifecycle`   | `webNavigation`                 |
|  [30]   | `webRequest`            | Request observation, blocking for policy installs alone             | `webRequest`, host permissions  |
|  [31]   | `windows`               | Browser windows                                                     | None                            |

Namespace members hold their own conditions:
- `runtime.getContexts({ contextTypes })` lists an extension's open contexts, offscreen documents and side panels included
- `userScripts` code runs once a user enables Allow User Scripts for its extension
- `identity.getAccounts`, `processes`, and `system.network` are available on the dev channel alone
- Extension namespace `modelContext` does not exist

`chrome-types` tags each API member with its first release, permission, and channel.

`offscreen.createDocument` takes each reason its document serves: `AUDIO_PLAYBACK`, `BATTERY_STATUS`, `BLOBS`, `CLIPBOARD`, `DISPLAY_MEDIA`, `DOM_PARSER`, `DOM_SCRAPING`, `GEOLOCATION`, `IFRAME_SCRIPTING`, `LOCAL_STORAGE`, `MATCH_MEDIA`, `TESTING`, `USER_MEDIA`, `WEB_RTC`, `WORKERS`. Its page is a packaged HTML file that takes no focus.

## [02]-[MANIFEST_KEYS]

Manifest keys outside the common set take one value each:

| [INDEX] | [KEY]                                                        | [VALUE]                                                              |
| :-----: | :----------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `chrome_url_overrides`                                       | `newtab`, `history`, or `bookmarks` to a page                        |
|  [02]   | `cross_origin_embedder_policy`, `cross_origin_opener_policy` | `{ value }` headers of extension pages, both for `SharedArrayBuffer` |
|  [03]   | `declarative_net_request`                                    | `rule_resources` rows of `id`, `enabled`, and `path`                 |
|  [04]   | `devtools_page`                                              | Page that registers DevTools panels                                  |
|  [05]   | `externally_connectable`                                     | `ids` and `matches` allowed to send, received on `onMessageExternal` |
|  [06]   | `export`, `import`                                           | Shared module resources and modules an extension reads               |
|  [07]   | `homepage_url`                                               | URL, store page by default                                           |
|  [08]   | `incognito`                                                  | `spanning`, `split`, or `not_allowed`                                |
|  [09]   | `mime_types_handler`                                         | `application/pdf` to `handler_url` and `can_embed`                   |
|  [10]   | `oauth2`                                                     | `client_id` and `scopes` of `identity.getAuthToken`                  |
|  [11]   | `omnibox`                                                    | `keyword`                                                            |
|  [12]   | `protocol_handlers`                                          | Rows of `protocol`, `name`, and `uriTemplate`                        |
|  [13]   | `sandbox`                                                    | `pages` served with a unique origin                                  |
|  [14]   | `short_name`                                                 | Name within 12 characters                                            |
|  [15]   | `storage`                                                    | `managed_schema` path of `storage.managed`                           |
|  [16]   | `trial_tokens`                                               | Origin trial tokens registered for `chrome-extension://<id>`         |
|  [17]   | `version_name`                                               | Display label `version` cannot spell                                 |

Chrome applies each key by its own rule:
- `chrome_url_overrides` takes one page per extension, and a new tab page gives first focus to the address bar
- `trial_tokens` apply to service worker and extension pages, and a content script takes its trial from a token registered for its page's origin
- `file_handlers`, `file_browser_handlers`, and `input_components` are available on ChromeOS alone

## [03]-[REQUEST_RULES]

`declarativeNetRequest` evaluates rules in Chrome with no extension code running per request:

| [INDEX] | [LIMIT]                          | [VALUE]                                        |
| :-----: | :------------------------------- | :--------------------------------------------- |
|  [01]   | Static rulesets, enabled at once | 100, 50                                        |
|  [02]   | Static rules guaranteed          | 30000, more from a pool every extension shares |
|  [03]   | Dynamic rules, unsafe among them | 30000, 5000                                    |
|  [04]   | Session rules                    | 5000                                           |
|  [05]   | Regex rules per kind             | 1000, each compiling under 2 KB                |

Rules pair one action with its conditions:
- Actions `block`, `allow`, `allowAllRequests`, and `upgradeScheme` are safe, and `redirect` and `modifyHeaders` unsafe
- Conditions take `urlFilter` or `regexFilter`, `initiatorDomains`, `requestDomains`, `topDomains`, `resourceTypes`, `requestMethods`, and `responseHeaders`
- `updateDynamicRules` rules last across sessions, `updateSessionRules` rules until restart, and `updateEnabledRulesets` switches static sets
- `declarativeNetRequestWithHostAccess` shows no warning and acts on hosts an extension holds
- `testMatchOutcome` and `onRuleMatchedDebug` are available for an unpacked extension alone

## [04]-[MESSAGES]

Message fields and limits depend on serialization format and receiver:
- `MessageSender` holds `id`, `url`, `origin`, `documentId`, `documentLifecycle`, `frameId`, and `tab`
- JSON turns `undefined` into `null` and holds 64 MiB
- Structured clone ignores `toJSON()`, transfers no `ArrayBuffer`, and fails between extensions with different formats
- Native hosts exchange JSON alone over `runtime.connectNative`, 1 MB from host to Chrome and 64 MiB from Chrome to host
- Host manifests on macOS sit in `~/Library/Application Support/Google/Chrome/NativeMessagingHosts/<name>.json`
- Host manifests name an absolute `path` and exact `allowed_origins`
