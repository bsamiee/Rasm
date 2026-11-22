# [MANIFEST]

## [01]-[FIELDS]

`manifest.json` at a product's root holds what Chrome and the Web Store read, and its build emits it unchanged:

| [INDEX] | [FIELD]                  | [FORM]                                                                                      |
| :-----: | :----------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `manifest_version`       | `3`                                                                                         |
|  [02]   | `name`, `description`    | `__MSG_name__`, `__MSG_description__`, name within 75 and description within 132 characters |
|  [03]   | `version`                | `major.minor.patch`                                                                         |
|  [04]   | `default_locale`         | Locale folder holding every key                                                             |
|  [05]   | `minimum_chrome_version` | String naming the oldest release                                                            |
|  [06]   | `icons`                  | PNG at 16, 32, 48, and 128 rasterized from `icons/mark.svg`                                 |
|  [07]   | `action`                 | `default_title` from `_locales`, no `default_popup` where a click acts                      |
|  [08]   | `background`             | `service_worker` as built file name, `type: "module"`                                       |
|  [09]   | `commands`               | `_execute_action` with `suggested_key` per platform                                         |
|  [10]   | `permissions`            | Least set code calls                                                                        |
|  [11]   | `key`                    | Public key from dashboard after first upload                                                |

Manifest rules Chrome enforces or tests depend on:
- `action` must exist for `chrome.action` to be defined and for CDP `Extensions.triggerAction` to press it
- `default_popup` replaces `action.onClicked`, so an extension whose shortcut acts declares no popup
- `_execute_action` fires `action.onClicked`, one listener serving click, shortcut, and test
- `suggested_key` takes `default`, `mac`, and `linux`, as IBus on Linux takes `Ctrl+Shift+U` for Unicode input
- Users rebind shortcuts at `chrome://extensions/shortcuts`, and `chrome.commands.getAll()` reads a live binding before UI shows it
- Manifest `icons` and `action.default_icon` refuse SVG, and a 128px icon pads 16px around 96px of artwork
- `key` fixes an extension's ID, the first 32 hex digits of its SHA-256 mapped `0-f` to `a-p`, and unpacked builds without it take an ID from their path
- `minimum_chrome_version` takes a full build number (`"157.0.<build>.0"`) where a feature landed mid-release
- `version_name` holds a display label `version` cannot spell
- `"message_serialization": "structured_clone"` carries `Map`, `Set`, and `Date` through runtime messages

## [02]-[PERMISSIONS]

Each permission grants one capability and carries its install warning and review cost:

| [INDEX] | [PERMISSION]     | [GRANTS]                                              | [WARNING]                          |
| :-----: | :--------------- | :---------------------------------------------------- | :--------------------------------- |
|  [01]   | `activeTab`      | Host access to a tab a user acts on, for that page    | None                               |
|  [02]   | `scripting`      | `executeScript`, `insertCSS` on tabs with host access | None                               |
|  [03]   | `clipboardWrite` | Clipboard writes from worker and pages                | Modify data you copy and paste     |
|  [04]   | `storage`        | `chrome.storage` areas                                | None                               |
|  [05]   | `tabs`           | `url`, `title`, `favIconUrl` of every tab             | Read your browsing history         |
|  [06]   | Host permissions | Standing access to matching sites                     | Read and change your data on sites |
|  [07]   | `notifications`  | OS notification center                                | Display notifications              |

Permissions follow code:
- `tabs` and host permissions add review time as sensitive permissions
- `activeTab` with `scripting` covers an action on the current page, so no host permission or `tabs` joins it
- Host access a later feature needs goes in `optional_host_permissions`, requested at a user's gesture
- `chrome.tabs.getZoom` and `chrome.runtime.getPlatformInfo` need no permission

## [03]-[CONTENT_SECURITY]

Extension pages run under a fixed CSP:
- `eval`, `new Function`, string `setTimeout`, inline scripts and handlers, and `javascript:` URLs stay blocked
- Packages hold all code, as remote code fails review, minified code passes, and obfuscated code fails
- Source maps stay out of packages
- Page CSP never restricts code injected into an isolated world
- Sandbox pages run `eval` with no `chrome.*` access
