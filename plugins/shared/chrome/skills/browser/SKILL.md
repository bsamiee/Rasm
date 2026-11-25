---
name: browser
description: "Use when work reads or drives the owner's signed-in Chrome, web pages and chrome:// pages alike, covering the chrome-devtools CLI daemon, page commands, and settings, flags, theme, and extension changes through Chrome's own pages"
---

# [CHROME_BROWSER]

Covers the owner's signed-in Chrome, driven through CLI `chrome-devtools`.

## [01]-[CONNECTION]

One `chrome-devtools` daemon per user attaches to running Chrome, and every Claude Code and Codex session shares it through `/tmp/chrome-devtools-mcp-<uid>/server.sock`:

```bash
chrome-devtools status | rg -q -- --auto-connect || chrome-devtools start --autoConnect --categoryExtensions --experimentalIncludeAllPages \
    --experimentalDevtools --workspace=/ --no-usage-statistics --no-performance-crux
```

- Commands with no daemon running start one with default flags on a scratch headless Chrome, so the line above runs before a session's first command
- Pages keep the owner's sign-ins, extensions, and settings, and `--output-format json` prints each result as JSON
- Chrome serves CDP once the owner enables remote debugging at `chrome://inspect/#remote-debugging`
- Chrome asks the owner to Allow each new CDP connection and remembers no answer, so `start` runs only when `status` lacks `--auto-connect`, the spelling `status` prints
- Connections last until the daemon stops, Chrome quits, or `restartBrowser()` runs, and the next command waits on a new Allow dialog
- `Could not connect to Chrome` -> ask the owner to open Chrome and enable remote debugging

## [02]-[PAGES]

Page commands take `pageId` from `list_pages`, and every session sharing the daemon sees every page:
- `new_page <url> --background` opens a tab behind the owner's work, and `close_page <pageId>` closes each tab a session opened before its final message
- `take_snapshot <pageId>` prints element `uid`s that `click <pageId> <uid>`, `fill`, `hover`, and `evaluate_script --args` take
- `evaluate_script '<source>' --pageId <pageId>` takes `async () => ...` source and returns JSON values
- `navigate_page` and `new_page` refuse `chrome:` URLs other than `chrome://newtab` and `chrome://inspect`
- Chrome Web Store pages take `click` on Add to Chrome, and the owner answers Chrome's Add extension dialog
- `install_extension` loads an unpacked build folder and prints its id, and `list_extensions` lists unpacked extensions alone
- `reload_extension` restarts an unpacked extension's service worker from its folder and closes its open pages, and `uninstall_extension` removes it
- Chrome removes an extension `install_extension` loaded at its next start

## [03]-[CHROME_PAGES]

Extensions, Claude in Chrome included, cannot reach `chrome://` pages in Google Chrome, and a DevTools window inspecting a tab navigates and evaluates there with no URL check:
1. `new_page 'about:blank#<task>' --background` opens the tab to drive
2. `new_page 'chrome://inspect/#other' --background` opens the target list
3. `evaluate_script` on `chrome://inspect` presses action `inspect` of the row naming `about:blank#<task>`, which opens a DevTools window
4. `list_pages` prints each DevTools window as a `devtools://` page, where `evaluate_script` drives the tab
5. `close_page` closes DevTools page, inspect tab, and driven tab once work ends

Step 3 matches the row by its URL:

```js
() => [...document.querySelectorAll(".row")]
    .find((row) => row.querySelector(".url")?.textContent.startsWith("about:blank#<task>"))
    ?.querySelector(".action")
    ?.click()
```

Step 4 navigates the tab to `chrome://<page>`, then evaluates `<expression>` in it:

```js
async () => {
    const SDK = await import("./core/sdk/sdk.js");
    const target = SDK.TargetManager.TargetManager.instance().primaryPageTarget();
    await target.pageAgent().invoke_navigate({ url: "chrome://<page>" });
    const { result, exceptionDetails } = await target.runtimeAgent().invoke_evaluate({ expression: "<expression>", returnByValue: true, awaitPromise: true });
    return exceptionDetails ?? result.value;
}
```

When DevTools leaves its page blank, `evaluate_script` removes class `__web-inspector-hide-shortcut__` from `<body>`.

## [04]-[SETTINGS]

As Chrome rewrites `Local State` and `Default/Preferences` on exit, each change calls an API of Chrome's own pages and shows as if clicked, and `jq` over either file only confirms a value.

Expressions on `chrome://settings` read and write prefs:
- `chrome.settingsPrivate.getAllPrefs(cb)` lists each pref with `key` and `value`
- `chrome.settingsPrivate.setPref("<key>", <value>, "", cb)` writes one and reports false for a pref an extension controls or one already at its value
- Prefs keyed `synthetic.*` change through a click on inner `cr-toggle` of the `settings-toggle-button` holding their `pref.key`
- Settings drawn as a `<select>` take its `value` and a dispatched `change` event
- `chrome.send("setMetricsReportingEnabled", [<bool>])` sets usage reports, a setting with no pref

Expressions on `chrome://settings/manageProfile` set profile name and theme:
- `chrome.send("setProfileName", ["<name>"])` renames the profile
- `handler_.setSeedColor({ value: 0xFF<RRGGBB> }, <variant>)` of element `cr-theme-color-picker` sets the theme seed in `browser.theme.user_color2`
- Variants are 1 tonal spot, 2 neutral, 3 vibrant, and 4 expressive
- Theme extensions replace the seed theme and disable glass frame (`glass_frame.enabled`)

## [05]-[FLAGS]

Expressions on `chrome://flags` call its page proxy:
- `(await import("chrome://flags/flags_browser_proxy.js")).FlagsBrowserProxyImpl.getInstance()` returns the page's proxy
- `requestExperimentalFeatures()` lists each flag with `internal_name` and `options`, `options` absent for an on-off flag
- `options[<index>].selected` marks the option a flag holds, `enabled` an on-off flag, and `is_default` reads false on each flag with a choice
- `selectExperimentalFeature("<flag>", <index>)` picks an option by its index in `options`
- `enableExperimentalFeature("<flag>", <bool>)` sets an on-off flag
- Code picks and reads back an option by `options[<index>].description`, as Chrome reorders a flag's options across releases
- `exportFlags()` resolves `{ enabled_flags, customized_flags }`, each held option as `<flag>@<index>`
- `resetAllFlags()` clears every choice, and `importFlags(<object>)` merges into choices already set
- `exportFlags()` and `importFlags()` end the browser process unless flag `import-export-flags` is enabled
- `restartBrowser()` restarts Chrome with its tabs and applies flags, which `Local State` `browser.enabled_labs_experiments` holds from then on
- Flags under an expired milestone appear after `temporary-unexpire-flags-m<milestone>` is enabled and Chrome restarts
- Flags change only a feature `chrome://version/?show-variations-cmd` shows in its rollout state, as Enabled changes nothing on one `--enable-features` lists
- `Default` leaves a feature to Chrome's code and field trials, either of which can enable it, and `Disabled` alone holds one off

## [06]-[EXTENSIONS]

Expressions on `chrome://extensions` and on an extension's own pages change installed extensions:
- `chrome.developerPrivate.updateExtensionConfiguration` on `chrome://extensions` takes `extensionId` with `fileAccess`, `incognitoAccess`, or `pinnedToToolbar`
- `chrome.developerPrivate.loadUnpacked({ failQuietly: true, populateError: true })` opens a folder picker and loads an extension that lasts across starts
- Folder pickers take a sidebar row, then a folder row, then Select, and Chrome resolves a symlink to its target
- `chrome.developerPrivate.updateExtensionCommand({ extensionId, commandName, keybinding })` binds a shortcut a reinstall left inactive
- Extension pages open as `chrome-extension://<id>/<page>`, where `browser.runtime.sendMessage(<message>)` sends what a page's own script sends
- Dashboard of uBlock Origin Lite takes `{ what: "applyRulesets", enabledRulesets }`, `{ what: "setDefaultFilteringMode", level }`, and `{ what: "getOptionsPageData" }`
- Filtering level 3 (Complete) of uBlock Origin Lite holds once `browser.permissions.contains({ origins: ["<all_urls>"] })` returns true

## [07]-[LIMITS]

Changes stay inside the owner's setup:
- `defaults read com.google.Chrome` holds no policy row, as any policy row marks Chrome managed and disables auto browse
- 1Password owns filling and sign-in, and Chrome's password, passkey, autofill, card, wallet, and sign-in features stay disabled, flags included
- Owner signs in to Google and answers each permission grant
