---
name: chrome
description: "Use when work reads or drives the owner's signed-in Chrome, web pages and chrome:// pages alike, covering the chrome-devtools MCP server, the Allow prompt, page ids, and settings, flags, theme, and extension changes through Chrome's own pages"
---

# [TOOL_CHROME]

MCP server `chrome-devtools` attaches to the running Chrome with `--autoConnect`, so pages keep the owner's sign-ins, extensions, and settings, and `mcp-muxer --per user` shares its one connection across every Claude Code and Codex session.

## [01]-[CONNECTION]

Chrome serves CDP once the owner turns on remote debugging at `chrome://inspect/#remote-debugging`:
- Chrome asks the owner to Allow each new CDP connection and remembers no answer, so the first call after a server start waits on that dialog
- One server keeps its connection until it exits, Chrome quits, or `restartBrowser()` runs, and the next call asks again
- `Could not connect to Chrome` -> ask the owner to open Chrome and turn on remote debugging
- CLI `chrome-devtools` starts a daemon with its own connection and a second Allow, so sessions call the MCP tools alone

## [02]-[PAGES]

Page tools take `pageId` from `list_pages`, and every session sharing the server sees every page:
- `new_page` with `background: true` opens a tab behind the owner's work, `close_page` closes each tab a session opened before its final message
- `take_snapshot` prints element `uid`s that `click`, `fill`, `hover`, and `evaluate_script` `args` take
- `evaluate_script` takes `async () => ...` source and returns JSON values
- `navigate_page` and `new_page` refuse `chrome:` URLs other than `chrome://newtab` and `chrome://inspect`
- Extension pages open as `chrome-extension://<id>/<page>`, where `chrome.runtime.sendMessage` reaches the extension's own handlers
- Chrome Web Store pages take `click` on Add to Chrome, and the owner answers Chrome's Add extension dialog

## [03]-[CHROME_PAGES]

Extensions, the Claude in Chrome extension included, cannot reach chrome:// pages in Google Chrome whatever flag `extensions-on-chrome-urls` holds, and a DevTools window inspecting a tab navigates and evaluates there with no URL check:
1. `new_page` `about:blank#<task>` with `background: true` opens the tab to drive
2. `new_page` `chrome://inspect/#other` with `background: true` opens the target list
3. `evaluate_script` on the inspect page presses the `inspect` action of the row naming `about:blank#<task>`, opening a DevTools window
4. `list_pages` prints the window as a `devtools://` page, and `evaluate_script` there drives the tab, removing class `__web-inspector-hide-shortcut__` from `<body>` when DevTools leaves the page blank
5. `close_page` closes the DevTools page, the inspect tab, and the driven tab once work ends

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

## [04]-[SETTINGS]

Chrome writes `Local State` and `Default/Preferences` and rewrites both on exit, so expressions call the APIs Chrome's own pages call, changes show as if clicked, and `jq` over those files confirms a value alone:
- `chrome://settings`: `chrome.settingsPrivate.getAllPrefs(cb)` lists each pref with `key` and `value`, `setPref("<key>", <value>, "", cb)` writes one and reports false for a pref an extension controls or one already at that value
- `chrome://settings`: prefs keyed `synthetic.*` change through a click on the inner `cr-toggle` of the `settings-toggle-button` holding that `pref.key`
- `chrome://settings`: settings drawn as a `<select>` take its `value` and a dispatched `change` event
- `chrome://settings`: `chrome.send("setMetricsReportingEnabled", [<bool>])` sets usage reports, which hold no pref
- `chrome://settings/manageProfile`: `chrome.send("setProfileName", ["<name>"])` renames the profile
- `chrome://settings/manageProfile`: `cr-theme-color-picker` element's `handler_.setSeedColor({ value: 0xFF<RRGGBB> }, <variant>)` sets the theme seed in `browser.theme.user_color2`, variants 1 tonal spot, 2 neutral, 3 vibrant, 4 expressive
- Theme extensions replace the seed theme and turn glass frame (`glass_frame.enabled`) off
- `chrome://flags`: `(await import("chrome://flags/flags_browser_proxy.js")).FlagsBrowserProxyImpl.getInstance()` returns the page's proxy
- `chrome://flags`: `requestExperimentalFeatures()` lists each flag with `internal_name` and `options`, `options` absent for an on-off flag
- `chrome://flags`: `options[<index>].selected` reads false on every option, so `exportFlags()` reads which option a flag holds as `<flag>@<index>`
- `chrome://flags`: `selectExperimentalFeature("<flag>", <index>)` picks an option by its index in `options`, `enableExperimentalFeature("<flag>", <bool>)` sets an on-off flag
- `chrome://flags`: `exportFlags()` resolves `{ enabled_flags, customized_flags }`, `resetAllFlags()` clears every choice, and `importFlags(<object>)` merges into choices already set
- `chrome://flags`: `exportFlags()` and `importFlags()` end the browser process unless flag `import-export-flags` is on
- `chrome://flags`: `restartBrowser()` relaunches Chrome with its tabs and applies flags, `Local State` `browser.enabled_labs_experiments` holding them from that relaunch on
- Flags under an expired milestone appear after `temporary-unexpire-flags-m<milestone>` is enabled and Chrome relaunches
- `chrome://extensions`: `chrome.developerPrivate.updateExtensionConfiguration({ extensionId, fileAccess, incognitoAccess, pinnedToToolbar })` sets an extension's file and incognito access and its toolbar pin
- `chrome-extension://<id>/<page>`: `chrome.runtime.sendMessage(<message>)` sends the messages the page's own script sends, as uBlock Origin Lite's dashboard takes `{ what: "applyRulesets", enabledRulesets }`, `{ what: "setDefaultFilteringMode", level }`, and `{ what: "getOptionsPageData" }`
- Filtering level 3 (Complete) of uBlock Origin Lite holds once `chrome.permissions.contains({ origins: ["<all_urls>"] })` returns true

## [05]-[RULES]

Changes follow Chrome's own state:
- Flags change only a feature `chrome://version/?show-variations-cmd` shows in its rollout state, as Enabled on a feature `--enable-features` already lists changes nothing
- Flag options are picked and read back by `options[<index>].description`, as Chrome reorders a flag's options across releases
- `defaults read com.google.Chrome` lists no policy row, as any policy row marks Chrome managed and turns auto browse off
- 1Password owns filling and sign-in, so Chrome password, passkey, autofill, card, wallet, and sign-in features stay off, flags included
- Owner signs in to Google and answers each permission grant
