# [PHOTOSHOP]

Adobe Photoshop Beta 27.11 joins the `creative-cloud` MCP server through one UXP plugin over a local WebSocket, and its application configuration is a GUI pass over the running host. This file owns both: the host facts, the plugin and its job model, the `photoshop_*` tool contracts, the install route, and every setting, panel, preset, and artifact of the pass.

Source classes used in every table: `machine` is a read on this Mac, `adobe` is a developer.adobe.com or helpx.adobe.com page with its date, `types` is `@adobe-uxp-types/photoshop@0.1.8` or `@adobe-uxp-types/uxp@0.1.4`, `forum` is a dated Adobe forum post, `sdk` is `PIStringTerminology.h`. A claim with none of those is a row of [11] and appears nowhere else.

Channels named by every step of the pass:

| [INDEX] | [CHANNEL] | [MEANING] |
| :-----: | :-------- | :-------- |
|  [01]   | `bridge`  | The plugin over the socket: DOM reads, `batchPlay` descriptors, `executeAsModal` writes |
|  [02]   | `osascript` | `tell application id "com.adobe.Photoshop" to do javascript "…"`, ExtendScript, `system_report` alone |
|  [03]   | `AX`      | System Events over the menu bar, the Settings dialog panes, and the Cocoa dialogs |
|  [04]   | `CU`      | computer-use click, drag, and zoomed window-id screenshot on Drover-drawn surfaces |
|  [05]   | `FILE`    | A preference or preset file read or copied with Photoshop quit |

## [01]-[HOST]

| [INDEX] | [FACT] | [SOURCE] |
| :-----: | :----- | :------- |
|  [01]   | Host is `Adobe Photoshop (Beta).app` under `<photoshop.installFolder>`, bundle id `com.adobe.Photoshop`, `app.version` `27.11.0`, `app.build` `27.11.0 (20260909.m.3669 481f204)`, feature access `PublicBeta` | machine, `osascript` read |
|  [02]   | The release build is absent: `/Applications` holds the Beta builds of Photoshop, Illustrator, and InDesign beside Acrobat DC, and the Creative Cloud extension manager logs `Product not found for specifier "PHSP-27.10.0"` | machine, `ls /Applications`, `EMCL.log` |
|  [03]   | The UXP runtime is read at run time from `require('uxp').versions.uxp` and carried in the `hello` frame; on this build the literal `uxp-9.4.1-0` sits in the main binary and in `dynamic-torqnative.framework`, and `app.systemInformation` prints `Unified Extensibility Platform uxp-9.4.1-0 UPIC 2.6.0` | machine, `rg -a` over the bundle, `system-information.txt`; adobe Versions page |
|  [04]   | `dvauxphost.framework`, the UXP host bridge, reports `CFBundleShortVersionString 25.2.0`, a DVA framework version and not a UXP version | machine, `plutil -p` on its `Info.plist` |
|  [05]   | The Photoshop UXP API changelog ends at 27.4 with `UXP v9.2.0 Integration`; UXP 9.3 and 9.4 are described by the Adobe developer blog of 2026-07 and nothing documents 9.4.1, so the runtime runs two UXP minors ahead of the API changelog | adobe changelog; adobe blog 2026-07 |
|  [06]   | UXP 9.4 adds `queueMicrotask`, `featureFlags.uncaughtException`, `featureFlags.unhandledRejection`, CSS `text-decoration`, and WebView navigation status codes; 9.3 stringifies plugin command results by default | adobe blog 2026-07 |
|  [07]   | UXP 9.2 adds the `window` events `sleep` and `awake`; 9.1 adds `URLSearchParams`; 9.0 makes `requiredPermissions.webview.domains` optional; 8.2 adds `fs.createReadStream` | adobe changelog 27.4, 26.10, 26.8 |
|  [08]   | `host.data.apiVersion` `1` is the 2021 model where any plugin mutates at any time and `2` is the modal-scope model; `2` is the default at `host.minVersion` `23.0.0` or later, `1` is deprecated, and `executeAsModal` and `redrawDocument` exist under `2` alone | adobe Photoshop manifest page, photoshopCore page |
|  [09]   | `require('photoshop').core.apiVersion` reports the declared value at run time | adobe photoshopCore page; types `Core.apiVersion` |
|  [10]   | `require('uxp').host` gives `name` (`"photoshop"`), `version`, and `uiLocale`; `require('uxp').versions` gives `uxp` and `plugin` | adobe Host and Versions pages; types |
|  [11]   | The UXP DOM `app` object has no `version` and no `build`: the Photoshop class page lists `actionTree`, `activeDocument`, `backgroundColor`, `currentTool`, `displayDialogs`, `documents`, `fonts`, `foregroundColor`, `preferences`, `typename`, and the typings match | adobe Photoshop class page; types |
|  [12]   | Developer mode is off: `pluginPicker.enablePluginDeveloperMode` is `false`, `/Library/Application Support/Adobe/UXP/Developer/settings.json` is absent, `/Library/Application Support/Adobe/UXP/` holds `extensions/` alone, and no UXP Developer Tool is installed | machine, application descriptor, `ls` |
|  [13]   | Nine TK9 4.0.0 plugins are registered in `PluginsInfo/v1/PS.json`; four reach `Loaded` at launch (Multi-Mask, Combo, Export, My Actions) and five stay `Prepared` (Cx, My Actions-Tab 1 to 4), all labelled `from Plugin Marketplace` | machine, `system-information.txt` |
|  [14]   | The four `Loaded` plugins are the four whose panels sit in the saved workspace dock, two open and two minimized; the five `Prepared` ones have no panel in the dock. A plugin stays loaded while its panel is hidden: `app.panelList` reports Export and My Actions hidden while both report `Loaded` | machine, workspace dock tree and panel visibility of one launch |
|  [15]   | No `psjs` literal exists in the main binary, in `dvauxphost`, in `Contents/Frameworks`, or anywhere under `<photoshop.installFolder>`; `Presets/Scripts` holds `.jsx` alone and the per-user `Presets` tree has no `Scripts` directory. File > Scripts on this build enumerates ExtendScript alone | machine, full-text scan |
|  [16]   | `tell application id "com.adobe.Photoshop" to do javascript "…"` answers on this build in 0.13 s with no dialog and reaches `app.systemInformation`, `app.playbackDisplayDialogs`, `$.writeln`, `ExternalObject`, and `executeActionGet`; it is the second channel and carries `system_report` alone | machine, `osascript` read |
|  [17]   | UXP alone reaches the Unified Text Engine styles of 24.1 (`ParagraphStyle.layoutMode`, `kashidaWidth`, `justification`, `hyphenation`, `CharacterStyle.middleEasternDigitsType` with default `LTRARABIC`, `middleEasternTextDirection`, `kashidas`, `horizontalDiacriticPosition`, `verticalDiacriticPosition`), the twelve `app.preferences` classes, `batchPlay` with per-descriptor `_options`, `executeAsModal` with a named history suspension, and the `imaging` module | adobe ParagraphStyle and CharacterStyle class pages 2026-09-14 |
|  [18]   | Machine: Apple M4 Max, 16 cores, 65,536 MB, macOS 26.6.1, one display at 3024 × 1964 physical and 1800 × 1169 pt logical with a 39 pt menu bar; the Photoshop window frame is `{0, 39, 1800, 1081}` | machine, window list |
|  [19]   | Photoshop's window is accessibility-opaque: a System Events tree of the main window yields 13 nodes and the menu bar yields 1,151, so layout is proven by screenshot and the menu bar by `AXMenuItemMarkChar` | machine, accessibility dump |

Path keys and the read that produces each. The server reads them from the installed bundle at start into its host table and `health` returns the resolved table; no file records them.

| [INDEX] | [KEY] | [VALUE_ON_THIS_MACHINE] | [READ] |
| :-----: | :---- | :---------------------- | :----- |
|  [01]   | `<photoshop.bundleId>` | `com.adobe.Photoshop` | `Info.plist` `CFBundleIdentifier` |
|  [02]   | `<photoshop.installFolder>` | `/Applications/Adobe Photoshop (Beta)` | `app.path` |
|  [03]   | `<photoshop.bundlePath>` | `<photoshop.installFolder>/Adobe Photoshop (Beta).app` | Bundle lookup by id |
|  [04]   | `<photoshop.processName>` | `Adobe Photoshop 2026` | `Info.plist` `CFBundleExecutable` |
|  [05]   | `<photoshop.version>` | `27.11.0` | `Info.plist` `CFBundleShortVersionString` |
|  [06]   | `<photoshop.prefsFolder>` | `~/Library/Preferences/Adobe Photoshop (Beta) Settings` | `app.preferencesFolder` |
|  [07]   | `<photoshop.supportFolder>` | `~/Library/Application Support/Adobe/Adobe Photoshop (Beta)` | Support folder beside the preference folder |
|  [08]   | `<photoshop.pluginsFolder>` | `~/Library/Application Support/Adobe/UXP/Plugins/External` | Fixed UXP location |
|  [09]   | `<photoshop.registry>` | `~/Library/Application Support/Adobe/UXP/PluginsInfo/v1/PS.json` | Fixed UXP location |
|  [10]   | `<photoshop.pluginData>` | `~/Library/Application Support/Adobe/UXP/PluginsStorage/PHSPBETA/27/External` | Machine read |

Typings. The plugin project's `tsconfig` declares `types: ["@adobe-uxp-types/uxp", "@adobe-uxp-types/photoshop"]`. `node` joins `types` only where a file reads `import.meta.dirname` or a `node:` module; the plugin reads neither, and `frames.ts`, the one server module it imports, pulls `effect` alone, whose declarations reference no `node:` module, `NodeJS`, or `Buffer`.

| [INDEX] | [PACKAGE] | [VERSION] | [STATE] |
| :-----: | :-------- | :-------- | :------ |
|  [01]   | `@adobe-uxp-types/photoshop` | 0.1.8, published 2026-05-20, peers `@adobe-uxp-types/uxp` 0.1.4 | Used; declares `module 'photoshop'` exporting `action`, `app`, `constants`, `core`, `imaging` |
|  [02]   | `@adobe-uxp-types/uxp` | 0.1.4, published 2026-05-20 | Used; declares `module 'uxp'` exporting `dialog`, `entrypoints`, `host`, `os`, `shell`, `storage`, `versions` plus the `adobe:` modules |
|  [03]   | `@types/photoshop` | 25.0.4, published 2023-11-21, community | Not used, two years behind the host |
|  [04]   | `@adobe/cc-ext-uxp-types` | 7.3.1, published 2023-10-19 | Not used, platform surface alone |

Gaps in 0.1.8 against the documentation, covered by one local `declare module` augmentation in the plugin: `Core` lacks `getLayerTree`, `getLayerTreeSync`, `getLayerGroupContents`, `historySuspended`, `createTemporaryDocument`, `deleteTemporaryDocument`, `convertGlobalToLocal`, and `removeNotificationListener`; `uxp` lacks `script`, `userInfo`, and `pluginManager`. The `action` module carries both listener methods, and `app` carries every method of the Photoshop class page.

## [02]-[PLUGIN]

`apps/creative-cloud/photoshop-plugin` is a transport plugin: a socket client, a one-second tick, and typed job handlers keyed by job kind. It holds no user interface beyond a status line, no refusal branch, and no clock comparison. Vite builds it through `vite-uxp-plugin`, whose `uxp.config.ts` object is the manifest source, so no `manifest.json` is written by hand.

The `config` object of `uxp.config.ts`, field by field. Values marked `omit` are absent from the emitted file.

| [INDEX] | [FIELD] | [MEANING] | [VALUE] | [SOURCE] |
| :-----: | :------ | :-------- | :------ | :------- |
|  [01]   | `manifestVersion` | `5` gives the permissions model and promise-honouring entrypoints, and needs Photoshop 23.3.0 with UXP 6.0 | `5` | adobe manifest v5 |
|  [02]   | `id` | Unique plugin id; a Developer Console id is needed for Marketplace distribution alone | `rasm.photoshop.bridge` | adobe manifest v4 |
|  [03]   | `name` | 3 to 45 characters | `Rasm Photoshop Bridge` | adobe manifest v4 |
|  [04]   | `version` | `x.y.z`, each segment 0 to 99; v5 accepts semver and zero-fills missing segments | `1.0.0`, bumped per deploy | adobe manifest v4, v5 |
|  [05]   | `main` | Path to the initialisation code; defaults to `main.js` | `index.html` holding one `<script src="plugin-main.js">` and one status line | adobe manifest v4 |
|  [06]   | `host.app` | `"PS"` for Photoshop; an array of hosts is allowed in development alone | `"PS"` | adobe manifest v4 |
|  [07]   | `host.minVersion` | Minimum host version in the documented `x.y` form; the extension manager warns `parts in version 23.3.0 is expected to have 2 parts` on a three-part value | `"27.11"` from `<photoshop.version>` | adobe manifest v4; machine `EMCL.log` |
|  [08]   | `host.maxVersion` | Optional upper bound | omit | adobe manifest v4 |
|  [09]   | `host.data.apiVersion` | Modality model | `2`, the value all nine TK9 manifests declare | adobe Photoshop manifest page; machine |
|  [10]   | `host.data.loadEvent` | `"use"` loads the plugin when its panel becomes visible or a command runs; `"startup"` loads it shortly after launch, named for plugins that talk to a remote server | `"startup"` | adobe Photoshop manifest page |
|  [11]   | `host.data.enableMenuRecording` | Lets `command` entrypoints record as Action steps | omit, no command entrypoint exists | adobe Photoshop manifest page |
|  [12]   | `entrypoints[].type` | `"panel"` or `"command"` | One `"panel"` | adobe EntryPoints page |
|  [13]   | `entrypoints[].id` | Key passed to `entrypoints.setup({panels: {id: …}})` and target of `core.suppressResizeGripper` | `"bridge"` | adobe manifest v4, photoshopCore |
|  [14]   | `entrypoints[].label` | Menu and tab text, string or `{default, <locale>}` with `default` required | `{"default": "Rasm Bridge"}` | adobe manifest v4 |
|  [15]   | `entrypoints[].minimumSize`, `maximumSize`, `preferredDockedSize`, `preferredFloatingSize` | Panel size preferences honoured at the host's discretion | `180×80`, `2000×2000`, `235×120`, `235×120` | adobe manifest v4 |
|  [16]   | `entrypoints[].icons` | 23×23 panel icons per theme with an `@2x` variant through `scale: [1, 2]` | `icons/dark.png` for `darkest`, `dark`, `medium`; `icons/light.png` for `lightest`, `light` | adobe manifest v4 |
|  [17]   | `entrypoints[].shortcut` | Documented as not yet available for plugins | omit | adobe manifest v4 |
|  [18]   | `icons[]` | `IconDefinition {width, height, path, scale, theme, species}`, PNG or JPG at 1 MB each | One 48×48 `icons/plugin.png`, `theme ["all"]`, `species ["pluginList"]` | adobe manifest v4 |
|  [19]   | `requiredPermissions.network.domains` | Origins with a scheme; nothing undeclared is granted, no wildcard in the top-level domain from UXP 7.4, `ws:` and `wss:` are distinct, `localhost` and `127.0.0.1` are distinct | `["ws://localhost:39217"]` against the server's `127.0.0.1` bind | adobe manifest v5, WebSocket page; forum 6066, Adobe staff 2023-03-27 |
|  [20]   | `requiredPermissions.localFileSystem` | `request`, `plugin` (the default), or `fullAccess`; `plugin://`, `plugin-temp://`, and `plugin-data://` are always available | omit, snapshots never touch disk | adobe manifest v5 |
|  [21]   | `requiredPermissions.allowCodeGenerationFromStrings` | Needed for `eval()` and `new Function()` | `true`, the `execute` handler builds an `AsyncFunction` | adobe manifest concept page; all nine TK9 manifests declare it |
|  [22]   | `requiredPermissions.clipboard` | `read` or `readAndWrite` | omit | adobe manifest v5 |
|  [23]   | `requiredPermissions.launchProcess` | `{schemes, extensions}` for `openExternal` and `openPath`, each call showing a consent dialog | omit | adobe manifest v5 |
|  [24]   | `requiredPermissions.webview` | `{domains}`; the `allow` field was removed in UXP 9.1 and WebViews run inside modal dialogs alone | omit | adobe manifest v5 |
|  [25]   | `requiredPermissions.ipc.enablePluginCommunication` | Drives other plugins through `require('uxp').pluginManager` | omit | adobe manifest v5 |
|  [26]   | `requiredPermissions.enableUserInfo` | Exposes `require('uxp').userInfo.userId()` from Photoshop 25.1 | omit | adobe manifest v5 |
|  [27]   | `featureFlags` | `uncaughtException`, `unhandledRejection`, `CSSNextSupport`, `enableSWCSupport`, `enableAlerts`, `enableFillAsCustomAttribute` | `{"uncaughtException": true, "unhandledRejection": true}` | adobe blog 2026-07 |
|  [28]   | `strings`, `addon` | Localisation table; native add-on needing a signed binary, install error `-23` otherwise | omit | adobe manifest concept page; helpx install errors 2026-07-03 |

The emitted manifest:

```json
{
    "manifestVersion": 5,
    "id": "rasm.photoshop.bridge",
    "name": "Rasm Photoshop Bridge",
    "version": "1.0.0",
    "main": "index.html",
    "host": { "app": "PS", "minVersion": "27.11", "data": { "apiVersion": 2, "loadEvent": "startup" } },
    "entrypoints": [
        {
            "type": "panel",
            "id": "bridge",
            "label": { "default": "Rasm Bridge" },
            "minimumSize": { "width": 180, "height": 80 },
            "maximumSize": { "width": 2000, "height": 2000 },
            "preferredDockedSize": { "width": 235, "height": 120 },
            "preferredFloatingSize": { "width": 235, "height": 120 },
            "icons": [
                { "width": 23, "height": 23, "path": "icons/dark.png", "scale": [1, 2], "theme": ["darkest", "dark", "medium"], "species": ["generic"] },
                { "width": 23, "height": 23, "path": "icons/light.png", "scale": [1, 2], "theme": ["lightest", "light"], "species": ["generic"] }
            ]
        }
    ],
    "icons": [
        { "width": 48, "height": 48, "path": "icons/plugin.png", "scale": [1, 2], "theme": ["all"], "species": ["pluginList"] }
    ],
    "requiredPermissions": {
        "network": { "domains": ["ws://localhost:39217"] },
        "allowCodeGenerationFromStrings": true
    },
    "featureFlags": { "uncaughtException": true, "unhandledRejection": true }
}
```

Lifecycle facts the plugin rests on:

| [INDEX] | [FACT] | [SOURCE] |
| :-----: | :----- | :------- |
|  [01]   | With `loadEvent` absent or `"use"` the plugin script runs when its panel becomes visible or one of its commands runs; with `"startup"` it runs shortly after launch. On this build a `"use"` plugin whose panel sits in the saved workspace, minimized or hidden, loads about 1.2 s after launch, and one whose panel is absent from the workspace stays `Prepared` | adobe Photoshop manifest page; machine [01] rows 13, 14 |
|  [02]   | Photoshop loads `index.html` and the `<script>` it references; `require('photoshop')` and `require('uxp')` are host-provided globals | adobe "Creating your first plugin" |
|  [03]   | `require('uxp').entrypoints.setup({plugin: {create, destroy}, panels: {<id>: {create, show, hide, destroy, invokeMenu, menuItems}}, commands: {<id>: {run, cancel}}})` may be called once and throws on a second call or on a data error | adobe EntryPoints page; types |
|  [04]   | Under v5 the panel methods `create(rootNode)`, `show(rootNode, data)`, `hide(rootNode, data)`, and `destroy(rootNode)` honour returned promises and time out at 300 ms, and plugin `destroy` honours a promise; panel `show` is tied to plugin `create` and panel `hide` to plugin `destroy` | adobe manifest v5 "Updates to Entrypoints methods" |
|  [05]   | `commands.<id>.run(executionContext, ...arguments)` under v5, with `cancel` reserved for future use | adobe EntryPoints page |
|  [06]   | The DOM event `uxpcommand` on `document` carries `commandId` `uxpshowpanel` or `uxphidepanel` when the panel opens or closes | adobe "How Do I…" page; types |
|  [07]   | `require('uxp').entrypoints.getPanel(id)` returns `UxpPanelInfo`; `_pluginInfo` exposes `developerPlugin`, `pluginPath`, `privileged`, `source`, `uid` | types |
|  [08]   | Plugin `destroy` runs before unload, and `dvauxphost` carries the scripting object `UXPUnloadEvent` with `DoNeedMoreTime(float)`, `DoFinishedWork()`, `Initiated`, `Cancelled`, the host's unload negotiation. The plugin closes its socket on the `window` `unload` event | adobe EntryPoints page; machine `strings` of `dvauxphost` |
|  [09]   | Photoshop core events: `core.addNotificationListener(group, events, cb)`; group `UI` carries `userIdle`, `minimizeAppWindow`, `panelVisibilityChanged`, `activationChanged`, `workspaceDragStarted`, `workspaceDragCompleted`, `workspaceLayoutCompleted`, `interactiveResizeBegin`, `interactiveResizeEnd`; group `OS` carries `activationChanged`, `displayConfigurationChanged` | adobe Event Codes page |
|  [10]   | Document changes arrive through `action.addNotificationListener([events], cb)` with the string codes of the Event Codes page; the `all` hook works in developer mode alone, and notifications are silenced while any non-interactive `executeAsModal` is active | adobe photoshopAction, batchPlay, executeAsModal pages |
|  [11]   | `core.getPluginInfo()` returns `batchPlayCount`, `mainThreadTimeOutCount`, `numberOfPendingMainThreadTasks`, `pluginLoadTime`, `launchTimeImpact`, `v8HeapSize`, documented as development-only | adobe photoshopCore page; types |
|  [12]   | The `window` events `sleep` and `awake` fire on system sleep and wake from UXP 9.2 | adobe changelog 27.4 |
|  [13]   | `setTimeout`, `clearTimeout`, and `setInterval` run inside UXP; `queueMicrotask` exists from UXP 9.4 | machine, shipping UXP plugin; adobe blog 2026-07 |

Transport facts:

| [INDEX] | [FACT] | [SOURCE] |
| :-----: | :----- | :------- |
|  [01]   | `window.WebSocket(url, protocols?)` is a client alone and throws `Error` on an invalid `url` or `protocols`. UXP has no server socket and no raw TCP, so the server owns the listener | adobe WebSocket page; forum 2091, Adobe staff 2023-02-25 |
|  [02]   | `readyState` is 0 CONNECTING, 1 OPEN, 2 CLOSING, 3 CLOSED; `protocol` is empty until connected; `bufferedAmount` does not reset on close; `binaryType` is `"blob"` or `"arraybuffer"` | adobe WebSocket page |
|  [03]   | `send(data)` accepts `string`, `ArrayBuffer`, and `ArrayBufferView`, and the socket closes itself when data cannot be sent because its buffer is full | adobe WebSocket page |
|  [04]   | `close(code = 1000, reason = "")` throws `Error` on an invalid code or reason, so the plugin wraps every close | adobe WebSocket page |
|  [05]   | `onopen`, `onclose`, `onerror`, `onmessage` and `addEventListener` both work, and `event.data` on a message is the string sent | machine, shipping UXP plugin; forum 6322 |
|  [06]   | A refused connection arrives as `onclose` rather than a constructor throw, the same event a dropped live connection produces, and an abandoned socket can deliver queued `onopen` and `onclose` late, so the plugin nulls all four handlers before abandoning one | machine, shipping UXP plugin source |
|  [07]   | `network.domains` entries carry a scheme: `ws://127.0.0.1:8001/` is valid while `127.0.0.1:8001`, `//127.0.0.1:8001/`, and `localhost` are not; a bare `"all"` string is accepted and alarms users at install | forum 6066, Adobe staff 2023-03-27; forum 6322 |
|  [08]   | A working InDesign manifest on this machine declares `["ws://localhost:6001", "wss://localhost:6001"]` and its plugin dials `ws://localhost:6001` | machine, installed plugin manifest |
|  [09]   | No `ws` npm package is usable: `require` of a node module inside UXP fails, and the global `WebSocket` is the client in every shipping plugin read | forum 2091; machine |
|  [10]   | Two published Photoshop MCP servers confirm the listener rule: one serves HTTP on `127.0.0.1:38452` with a plugin polling every 400 ms, the other runs a socket.io proxy on `ws://localhost:3001` with a 20 s proxy timeout because UXP cannot listen; neither inspects a fulfilled `batchPlay` array for `{_obj: "error"}` elements | deepwiki over both repositories |
|  [11]   | The server binds `127.0.0.1:39217` and the plugin dials `ws://localhost:39217`, one socket carrying JSON text frames. `host: 'localhost'` on the server binds `::1` alone and refuses the IPv4 loopback, so the bind host is the IPv4 literal | Rows 07, 08; machine, Node bind measurement |

Link state machine, one tick per second on each side, the one place a transition happens:

| [INDEX] | [SIDE] | [STATE] | [TRANSITION] |
| :-----: | :----- | :------ | :----------- |
|  [01]   | Server | `listening` | A socket arrives and its first frame decodes as `hello` → `attached{identity, lastBeat}`; any other first frame, or a second socket while not `listening`, closes that socket |
|  [02]   | Server | `attached{identity, lastBeat}` | `beat` refreshes `lastBeat`; a job dispatch → `busy`; `now - lastBeat > 10 s` → close → `listening`; a `CloseEvent` → `listening` |
|  [03]   | Server | `busy{identity, job, lastBeat}` | `done` or `failed` for `job.id` → `attached`; stale or closed → `listening`, and the in-flight job answers `TransportClosed` |
|  [04]   | Plugin | `idle` | Tick → `new WebSocket(url)` → `dialing{since}` |
|  [05]   | Plugin | `dialing{since}` | `onopen` → send `hello` → `attached`; `onclose` → `idle` after 2 s, handlers detached before the socket is abandoned |
|  [06]   | Plugin | `attached` | `beat` every 5 s; a `job` frame → `busy{job}`; 10 s without a server `beat` → guarded `close()` → `idle` |
|  [07]   | Plugin | `busy{job}` | The handler resolves → `done` or `failed` → `attached`; the server sends no second job while `busy`, so the plugin holds no refusal branch |

Frames, one `Schema.Union` on `type` in the server's `frames.ts`, imported by both plugins. Plugin to server: `hello{plugin, version, host: {name, version}, uxp}` reading `require('uxp').host.version` and `require('uxp').versions.uxp` because the UXP DOM `app` has no `version`; `beat`; `done{jobId, value}`; `failed{jobId, rejection: HostRejection}`. Server to plugin: `beat`; `job{jobId, kind, body, suspendHistory, commandName}`. `value` is the handler's JSON-serialisable return and never a DOM object.

## [03]-[JOB_EXECUTION]

One job runs at a time per host. A `Ref<Option<InFlight{jobId, startedAt}>>` on the server answers a second call with `HostBusy`; there is no queue and no plugin-side refusal. The deadline sits on the server as a detached fiber (`Effect.forkDetach`) joined under `Effect.timeoutOrElse` failing with `DeadlineExceeded{host, jobId}` at `timeoutMs` from the input, and the `job` frame carries no deadline. After a deadline the in-flight `Option` stays `Some` until the `done` or `failed` frame arrives; every tool but `health` answers `HostBusy` until then, and a late frame clears the `Ref` and is discarded.

Job handlers are a closed record keyed by job kind inside the plugin, each one typed against its body and its return. `execute` is the one string path: `new AsyncFunction(code)()` under `allowCodeGenerationFromStrings`. A `kind` outside the record answers `UnknownMethod`, and a body that fails to decode answers `MalformedParams`.

Modal scope and history suspension, per job:

| [INDEX] | [RULE] | [SOURCE] |
| :-----: | :----- | :------- |
|  [01]   | `core.executeAsModal<T>(targetFunction, options) => Promise<T>`; from 24.0 the resolved value may be a class instance | types; adobe photoshopCore |
|  [02]   | `options.commandName` is required and titles the progress bar; `descriptor?` passes an object to the target; `interactive?` (23.3) suppresses the blocking progress dialog and allows filter dialogs and workspaces; `timeOut?` (25.10) is the millisecond budget the request keeps retrying while another plugin holds the scope, defaulting to one second | adobe executeAsModal page; types |
|  [03]   | Mutating the document, the user interface state, or preference state requires the modal scope under `apiVersion 2`; reads do not | adobe executeAsModal page |
|  [04]   | A mutating job runs inside `core.executeAsModal(fn, {commandName, timeOut})` with `hostControl.suspendHistory({documentID, name})` opening and `resumeHistory({historySuspensionID, finalName?}, true)` closing, so one job is one history state. The `job` frame carries `suspendHistory: Option<{documentID, name}>` and `commandName: Option<string>`; a read carries `None` for both and runs outside a scope | adobe executeAsModal page; this design |
|  [05]   | Photoshop creates the history state only when the document changed. At scope end an unresumed suspension commits on a normal return and rolls back on an exception, and suspensions may cross nested scopes | adobe executeAsModal page; types |
|  [06]   | `Document.suspendHistory(callback, historyStateName)` wraps `executeAsModal` for one document and is the shorthand for a single-document job | types `ps-internal_dom_Document.d.ts`; adobe changelog |
|  [07]   | `hostControl.registerAutoCloseDocument(documentID)` closes a temporary document without saving at scope end; `core.createTemporaryDocument({documentID})` and `deleteTemporaryDocument` exist from 23.0 and are absent from the typings | adobe executeAsModal, photoshopCore pages |
|  [08]   | Nested `executeAsModal` calls are allowed and share the progress bar | adobe executeAsModal page |
|  [09]   | A rejection while another plugin holds the scope carries `e.number == 9`, and from 25.10 the message names the holding plugin | adobe executeAsModal page |
|  [10]   | `executionContext.isCancelled` is a boolean, `onCancel: (e?: {reason: string}) => void` is assignable, and `reportProgress({value?: 0..1, commandName?})` makes the bar determinate | types; adobe executeAsModal page |
|  [11]   | The progress bar appears after about two seconds titled with the plugin name and carrying a Cancel control, and hides while modal user interface is shown | adobe executeAsModal page |
|  [12]   | User cancellation is Escape, the progress bar Cancel, or Plugins > Cancel Plugin Command in interactive mode. After cancellation every awaited Photoshop call throws, JavaScript is interrupted at an `await` alone, a tight loop must poll `isCancelled`, and a `try/catch` that swallows the `batchPlay` exception defeats automatic termination | adobe executeAsModal page |
|  [13]   | An MCP cancellation interrupts the server handler fiber and answers nothing; the host job runs to completion and the in-flight rule of this section applies. No `cancel` frame exists: Photoshop's modal scope ends on user cancel alone | Rows 10, 12; this design |

## [04]-[BATCH_PLAY]

| [INDEX] | [FACT] | [SOURCE] |
| :-----: | :----- | :------- |
|  [01]   | `action.batchPlay(commands: ActionDescriptor[], options?) => Promise<ActionDescriptor[]>`; `action.batchPlaySync` (23.1) returns the array synchronously, and `app.batchPlay` is the same call | types; adobe photoshopAction, Photoshop pages |
|  [02]   | Typed options are `commandEnablement`, `dialogOptions`, `propagateErrorToDefaultHandler`, `synchronousExecution`, `modalBehavior`, `useMultiGet`, `suppressPlayLevelIncrease`, `continueOnError` (24.5), and `immediateRedraw` | types `BatchPlayCommandOptions` |
|  [03]   | `synchronousExecution` is never set; `continueOnError` defaults to false and stops at the first failing descriptor; `immediateRedraw` is a call-level option applied after every descriptor; `historyStateInfo` is deprecated since Photoshop 2022; `modalBehavior` is unnecessary inside a modal scope | adobe batchPlay page |
|  [04]   | Per-descriptor `_options` carries `dialogOptions` (`"silent"` the default and a scripting error on missing parameters, `"dontDisplay"` user interface on error or missing parameters alone, `"display"`) and `suppressProgressBar` | adobe batchPlay page |
|  [05]   | The promise rejects when the call itself is invalid, with an `Error` such as `Argument 1 has an invalid type. Expected type: array actual type: boolean` | adobe batchPlay page |
|  [06]   | A valid command Photoshop cannot process fulfils with `{_obj: "error", message, result}` at that descriptor's index, index-aligned with the input. `result` `0` is no error, `-128` is user cancel, and other values are internal codes. Adobe's prose spells the `_obj` value `"Error"` and the example spells it `"error"` | adobe batchPlay page |
|  [07]   | `core.setExecutionMode({enableErrorStacktraces: true})` adds `stacktrace` to an error descriptor in developer mode alone; `logRejections: true` logs rejected promises | adobe photoshopCore page |
|  [08]   | Since 23.4.1 `batchPlay` throws more often on bad requests, `move` on layers in particular | adobe Known Issues page |
|  [09]   | `_target` reference forms are `{_ref, _id}`, `{_ref, _index}` 1-based, `{_ref, _name}`, `{_ref, _enum: "ordinal", _value: "targetEnum" \| "first" \| "last" \| "front"}`, and `{_property}`; ids are stable for the session | adobe batchPlay page |
|  [10]   | `multiGet` takes `extendedReference: [[props], {_obj: <class>, index, count}]` with `count` `-1` for all, and `options: {failOnMissingProperty, failOnMissingElement}`; a bare `get` with no property returns every property and is development-only by Adobe's guidance | adobe batchPlay page |
|  [11]   | `action.validateReference(ref)` checks `action`, `document`, `channel`, `layer`, `guide`, `historyState`, `compsClass`, `path`, and `actionSet` references synchronously, and `action.getIDFromString` maps a string to its id | adobe photoshopAction page; types |
|  [12]   | Four-character codes remain usable as `"$Xxxx"` strings, keeping the trailing space where the code has one | adobe batchPlay page |
|  [13]   | Every fulfilled array element is inspected for `_obj: "error"` before the value leaves the plugin; a match becomes `DescriptorFailed{index, result, message}`, and `result === -128` becomes `UserCancelled` | This design; row 06 |

## [05]-[IMAGING]

| [INDEX] | [FACT] | [SOURCE] |
| :-----: | :----- | :------- |
|  [01]   | `require('photoshop').imaging` left beta at 24.4, and every pixel-returning call is asynchronous | adobe Imaging page; changelog 24.4 |
|  [02]   | `getPixels({documentID?, layerID?, historyStateID?, sourceBounds?, targetSize?, colorSpace?, colorProfile?, componentSize?, applyAlpha?}) => Promise<{imageData, sourceBounds, level}>`; a missing or negative `documentID` means the active document, an absent `layerID` means the composite, `sourceBounds` is `{left, top, right, bottom}` or `{left, top, width, height}` trimmed to the region holding pixels, `targetSize` is `{width?, height?}` scaling proportionally when one is given, `colorSpace` is `"RGB"`, `"Grayscale"`, or `"Lab"`, `componentSize` is `-1`, `8`, `16`, or `32`, and `applyAlpha: true` mats RGBA onto white and drops alpha | adobe Imaging page; types |
|  [03]   | A `targetSize` smaller than the region lets Photoshop read a pyramid cache level: `level` 0 is full resolution and 1 is half, and the returned `sourceBounds` sit in that level's canvas rather than the full-resolution canvas. The level count is the Performance preference Cache Levels, 4 on this machine | adobe Imaging page; machine |
|  [04]   | The tool reports `level` and `scale = 1 / 2^level` and converts `sourceBounds` to full-resolution pixels by multiplying with `2^level` | This design; row 03 |
|  [05]   | Valid composite `sourceBounds` start at `(0, 0)` with the document `width` and `height`; a pixel layer's valid bounds are `layer.boundsNoEffects` | adobe Imaging page; types |
|  [06]   | `PhotoshopImageData` carries `width`, `height`, `colorSpace`, `colorProfile`, `hasAlpha`, `components`, `componentSize`, `pixelFormat`, `chunky` in the typings against `isChunky` in the documentation, `type 'image/uncompressed'`, `getData({chunky?, fullRange?})`, and `dispose()`; 16-bit values run 0 to 32768 unless `fullRange` is set | adobe Imaging page; types |
|  [07]   | `encodeImageData({imageData, base64?}) => Promise<number[] \| string>` produces JPEG and requires an RGB `colorSpace`; no quality, size, or format option exists | adobe Imaging page; changelog May 2025 |
|  [08]   | `getSelection({documentID?, sourceBounds?, targetSize?})` returns a single grayscale component; `getLayerMask({documentID?, layerID, kind: 'user' \| 'vector', sourceBounds?, targetSize?})`, `putPixels`, `putLayerMask`, `putSelection`, and `createImageDataFromBuffer` complete the module | adobe Imaging page; types |
|  [09]   | `app.getColorProfiles("RGB" \| "Gray")` lists profile names, and 32-bit documents report `(Linear RGB Profile)` suffixes that must be stripped before a profile is passed | adobe Imaging page; Photoshop class page |
|  [10]   | Memory: the plugin console warns at `Plugin memory usage increased to: 600MB` and then `Plugin exceeds memory limit`, so every call takes the smallest `targetSize` and calls `dispose()` | adobe Imaging page |
|  [11]   | The snapshot path is `getPixels({documentID, layerID?, sourceBounds, targetSize, colorSpace: "RGB", colorProfile: "sRGB IEC61966-2.1", componentSize: 8, applyAlpha: true})`, then `encodeImageData({imageData, base64: true})`, then `imageData.dispose()`, reporting `width` and `height` from `imageData`. It is a read, so no modal scope opens, and the base64 string crosses the socket because the plugin has no disk access | Rows 02, 06, 07; [03] row 03 |
|  [12]   | The pixel budget is `dpi = clamp(round(min(1568 / longEdgeIn, sqrt(1150000 / areaIn))), 48, 600)` for detail and 768 px on the long edge within 300,000 px for preview; Photoshop renders at that size through `targetSize`, so no downscaler exists on the server | This design |
|  [13]   | A success value whose JSON text exceeds 200,000 characters is written to `.artifacts/creative-cloud/photoshop/results/<jobId>.json` and answered as `{kind: 'file', path, bytes, sha256}` | This design |

## [06]-[TOOLS]

Every tool is published as `photoshop_<name>` and declared as one `Tool.make(name, {description, parameters, success, failure})` row: `parameters` is one `Schema.Struct`, `success` one closed `Schema.Union` on `kind`, `failure` the `BridgeError` union. The server layer derives each tool's JSON Schema from the Effect `Schema` and decodes every argument before the handler runs, so a handler receives constructed values and validates nothing again. A field marked `?` is `Schema.optional`. The `{kind: 'error', error: BridgeError}` branch is implied on every output union and omitted below.

| [INDEX] | [TOOL] | [INPUT] | [OUTPUT] | [CALLS_IN_ORDER] | [UNDO_MODAL] | [PROOF] |
| :-----: | :----- | :------ | :------- | :--------------- | :----------- | :------ |
|  [01]   | `health` fields | none | The host's row of `health` with the fields `plan/design/server.md` states, `link.identity` carrying `{plugin, version, host, uxp}` | Link `Ref` read, then `execute {code: "1"}` end to end under a 5 000 ms race | none | A detached plugin answers `probe.ok false` with `HostNotAttached` and no socket traffic |
|  [02]   | `execute` | `{code, commandName?, suspendHistory?: {documentID, name}, timeoutMs?}` | `{kind: 'value', value, tookMs}` | `new AsyncFunction(code)()` inside `executeAsModal` when `commandName` is given, bare otherwise | One state when suspended | A body returning a literal answers it; a throw answers `ScriptThrew` |
|  [03]   | `batch_play` | `{descriptors: [{_obj, _target?, _options?: {dialogOptions: 'silent' \| 'dontDisplay' \| 'display', suppressProgressBar?}, …}], commandName, continueOnError?, suspendHistory?, immediateRedraw?}` | `{kind: 'descriptors', results: [ActionDescriptor], failed: [{index, result, message}], tookMs}` | `action.batchPlay(descriptors, {continueOnError, immediateRedraw})`, then each result element inspected for `_obj: 'error'` | Modal, one state | A descriptor naming a missing layer fills `failed` with its index and `result` |
|  [04]   | `snapshot` | `{target: 'document' \| 'selection' \| 'layer', documentId?, layerId?, region?: NormalizedRegion, budget: PixelBudget}` | `{kind: 'jpeg', base64, widthPx, heightPx, level, scale, sourceBounds, colorProfile}` | `imaging.getPixels(…)` (or `getSelection` for a selection, `layer.boundsNoEffects` for a layer), `imaging.encodeImageData({imageData, base64: true})`, `imageData.dispose()` | Read, no scope | The decoded JPEG's dimensions equal `imageData.width` and `height` |
|  [05]   | `get_document` | `{documentId?, layers?: {limit ≤ 500, cursor?, depth ≤ 8}}` | `{kind: 'document', documents: [{id, name, path, saved}], active: Option<{id, mode, bitsPerChannel, colorProfileName, width, height, resolution, layerTree, layerCount, layerCursor}>}` | `app.documents`, `Document` properties, `core.getLayerTree({documentID})` through the local typings augmentation | Read | The read values equal the Image Size and Color Settings dialogs; a nested group deeper than `depth` shows in `layerCursor` |
|  [06]   | `get_preferences` | `{sections: [one of the twelve classes]}` | `{kind: 'preferences', values: {section: {key: value}}}` | `app.preferences.<section>.<key>` over the typed key list of [08] | Read | Every key of every class answers a value |
|  [07]   | `set_preferences` | `{values: {section: {key: value}}}` | `{kind: 'applied', applied: [{section, key, from, to}], rejected: [{section, key, reason}]}` | Inside `executeAsModal({commandName: 'Apply preferences'})` with no history suspension: read, write, read back per key | Modal, no history | A write to a `notifications` key while `quietMode` is true answers `PreferenceLocked` |
|  [08]   | `list_presets` | `{kind: 'brush' \| 'swatch' \| 'gradient' \| 'style' \| 'pattern' \| 'contour' \| 'tool'}` | `{kind: 'presets', groupIndex, names, count}` | One `batchPlay([{_obj: 'get', _target: [{_ref: 'property', _property: 'presetManager'}, {_ref: 'application', _enum: 'ordinal', _value: 'targetEnum'}]}])` with no `_options`, then the group at the index `preset-groups.json` maps to the kind | Read | The names and counts equal the machine read: brushes 28, swatches 122, gradients 183, styles 21, patterns 10, contours 12, tool presets 21 |
|  [09]   | `run_action` | `{set, action}` | `{kind: 'value', value: null, tookMs}` | `app.actionTree` → `ActionSet.actions` matched by name → `Action.play()` inside `executeAsModal` | Modal, one state | The named action plays and the History panel gains one state |
|  [10]   | `system_report` | none | `{kind: 'report', text}` | `osascript` `do javascript "app.systemInformation"`, answering `HostBusy` while the link is `busy` | none | The text lists the UXP extensions with their load states |

Custom shapes are absent from the `presetManager` descriptor, so `list_presets` has no shapes kind and a custom-shape count is read through a `batch_play` `get` of the same descriptor. Capabilities with documented calls and no tool row are `execute` and `batch_play` bodies named here once: `apply_type_styles`, `apply_rtl_type` (`showTextFeatures = MIDDLEEASTERN`, `middleEasternDigitsType FARSI`, `layoutMode WORLDREADY`, `kashidaWidth`), `place_overlay`, and `build_template` (the `.psdt` builder of [20]). Developer mode is off, so `Copy As JavaScript` cannot record their descriptors and each body is written from the documented call list and proven by its readback.

## [07]-[REJECTIONS]

`HostRejection` is data the host produces: one `Schema.Union` of `Schema.TaggedStruct` members that decodes the plugin's `failed` frame. The server wraps it as `HostThrew{host, rejection}`. Photoshop produces six members, and the plugin maps its own failure shapes to them before the frame leaves.

| [INDEX] | [REJECTION] | [FIELDS] | [TRIGGER] |
| :-----: | :---------- | :------- | :-------- |
|  [01]   | `DescriptorFailed` | `index`, `result`, `message` | A fulfilled `batchPlay` array holds `{_obj: "error", message, result}` at `index` |
|  [02]   | `UserCancelled` | none | `result === -128` in a fulfilled descriptor, or the value thrown by an awaited call after Escape or the progress bar Cancel |
|  [03]   | `PreferenceLocked` | `section`, `key` | A `notifications` setter throws while `quietMode` is true |
|  [04]   | `ModalDenied` | `holder: Option<string>` | An `executeAsModal` rejection with `e.number === 9`, the holder parsed from the message on 25.10 and later |
|  [05]   | `ScriptThrew` | `name`, `message`, `line?`, `fileName?`, `number?` | An `execute` body threw; the frame carries `{name, message, stack}` with the stack trimmed |
|  [06]   | `NoActiveDocument` | none | `app.activeDocument` is absent or `app.documents.length === 0` on a tool that needs a document |
|  [07]   | `UnknownMethod` | `method` | The `job` frame carried a `kind` outside the handler record |
|  [08]   | `MalformedParams` | `reason` | The `job` frame's body failed to decode against the handler's input `Schema` |

Failures that never reach `HostRejection` because the server raises them directly: `HostNotAttached` when the link is `listening` and a job arrives, `HostBusy` when the in-flight `Option` is `Some` (`system_report` included), `TransportClosed` on a `CloseEvent` during a job or when UXP closes the socket over a full send buffer, `DeadlineExceeded` on the timeout, and `ResultNotDecodable` when a frame fails `Schema.decodeUnknownEffect`.

## [08]-[PREFERENCE_CLASSES]

`app.preferences` exposes twelve classes with 43 typed keys, the 38 of the 24.0 addition plus the five `notifications` keys of 26.11. Preference writes are state changes and run inside `executeAsModal`. The application descriptor on this machine holds 130 top-level keys, so most values have no DOM setter and the pass reaches them through the Settings dialog.

| [INDEX] | [CLASS] | [KEYS] | [RULES] |
| :-----: | :------ | :----- | :------ |
|  [01]   | `general` | `colorPicker: {type: 'photoshopPicker' \| 'systemPicker' \| 'pluginPicker', pluginID?}`, `imageInterpolation: Constants.InterpolationMethod`, `exportClipboard`, `autoUpdateOpenDocuments`, `beepWhenDone` | `pluginPicker` requires `pluginID` |
|  [02]   | `interface` | `dynamicColorSliders`, `textFontSize: Constants.FontSize`, `colorChannelsInColor` | `textFontSize` takes effect after restart |
|  [03]   | `tools` | `showToolTips`, `useShiftKeyForToolSwitch`, `keyboardZoomResizesWindows` | |
|  [04]   | `history` | `createFirstSnapshot`, `nonLinearHistory`, `numberOfHistoryStates` [1,1000], `useHistoryLog`, `editLogItems: Constants.EditLogItemsType`, `saveLogItems: Constants.SaveLogItemsType` | Setting `editLogItems` or `saveLogItems` sets `useHistoryLog = true` |
|  [05]   | `fileHandling` | `imagePreviews: Constants.SavePreview`, `useLowerCaseExtension`, `askBeforeSavingLayeredTIFF`, `maximizeCompatibility: Constants.MaximizeCompatibility`, `recentFileListMaximum` [0,100] | |
|  [06]   | `performance` | `imageCacheLevels` [1,8], `maxRAMuse` [2,99] | Both take effect after restart |
|  [07]   | `cursors` | `paintingCursors: Constants.PaintingCursors`, `otherCursors: Constants.OtherCursors` | |
|  [08]   | `transparencyAndGamut` | `gridSize: Constants.GridSize`, `gamutWarningOpacity` [1,100] | |
|  [09]   | `unitsAndRulers` | `rulerUnits: Constants.RulerUnits`, `typeUnits: Constants.TypeUnits`, `pointSize: Constants.PointType` | |
|  [10]   | `guidesGridsAndSlices` | `guideStyle: Constants.GuideLineStyle`, `gridStyle: Constants.GridLineStyle`, `gridSubDivisions` [1,100], `showSliceNumber` | |
|  [11]   | `type` | `showTextFeatures: Constants.TypeInterfaceFeatures` with `MIDDLEEASTERN = 'middleEasternInterface'`, `showEnglishFontNames`, `smartQuotes` | |
|  [12]   | `notifications` | `quietMode`, `showFeatureOnboarding`, `showToolTips`, `showWhatsNew`, `useRichToolTips` | `showFeatureOnboarding`, `showWhatsNew`, and `useRichToolTips` become read-only while `quietMode` is true and a write throws; the class arrived at 26.11 and its `typename` carries `@minVersion 27.2` |

Keys outside the DOM are read through a `batchPlay` `get` against `{_ref: "application", _enum: "ordinal", _value: "targetEnum"}` with a `_property` element, the path the descriptor read uses. Writing them with `set` is undocumented and is a row of [11]. The exception shape of a locked write is documented as "will throw errors" alone, so the plugin maps any throw from a `notifications` setter while `quietMode` is true to `PreferenceLocked{section, key}`.

## [09]-[DATA_FILES]

Each file sits beside the Photoshop module, holds one kind of row, and is decoded once at load through the `Schema` stated here. No consumer re-validates.

| [INDEX] | [FILE] | [SCHEMA] | [READ_BY] |
| :-----: | :----- | :------- | :-------- |
|  [01]   | `apps/creative-cloud/server/indesign/page-sizes.json` | The `PageSize` Schema the typography document states, 59 rows | `build_template` and the New Document preset step, so the Photoshop catalogue cannot drift from the page catalogue |
|  [02]   | `apps/creative-cloud/server/photoshop/preset-groups.json` | `Schema.Array(Schema.Struct({index: Schema.Int, kind: Schema.Literal('brush', 'swatch', 'gradient', 'style', 'pattern', 'contour', 'tool')}))` | `list_presets`, mapping a `presetManager` group index to its kind |
|  [03]   | `apps/creative-cloud/server/photoshop/preferences.json` | `Schema.Struct` with one optional field per class of [08], each a `Schema.Struct` over that class's typed keys | The pass's `set_preferences` step, so every DOM-reachable target value is one call |
|  [04]   | `apps/creative-cloud/server/photoshop/menu-visibility.json` | `Schema.Array(Schema.Struct({path: Schema.String, label: Schema.String, visible: Schema.Boolean}))` | The Edit > Menus step of the pass, `path` being the menu bar path the accessibility reader prints |

## [10]-[INSTALL]

Adobe documents two install routes, the `.ccx` double-click and the Unified Plugin Installer Agent, and neither is documented for Beta builds. On this machine the agent fails: `--install` of a `PS` `.ccx` returns `-411` `EXMAN_FAILED_NO_SUPPORTED_PRODUCT` because the extension manager's product table holds no rows and only Beta builds are installed. The proven route on this machine, for ten plugins, is the folder copy plus the registry row.

| [INDEX] | [FACT] | [SOURCE] |
| :-----: | :----- | :------- |
|  [01]   | A `.ccx` is a zip of the plugin root, and Adobe's tooling for making one is the UXP Developer Tool `Package` action | adobe Packaging page |
|  [02]   | An installed plugin folder holds `manifest.json`, `index.html`, `index.js`, `icons/`, `css/`, and `images/`, with no signature file and no `com.apple.quarantine` attribute | machine, installed TK9 folders |
|  [03]   | Registry row shape, copied from `com.tk.export`: `{"hostMinVersion": "23.3.0", "name": "TK9 Export", "path": "$localPlugins/External/com.tk.export_4.0.0", "pluginId": "com.tk.export", "status": "enabled", "type": "uxp", "versionString": "4.0.0"}` inside `{"plugins": […]}`; `PS.json` is mode `0600` owned by the user, and the InDesign registry `ID.json` sits beside it at `0644` | machine, `PluginsInfo/v1/PS.json` |
|  [04]   | Plugin data lands under `<photoshop.pluginData>/<id>/PluginData/` as flat one-value `.ini` files | machine |
|  [05]   | Nine TK9 folders were placed by hand and `PS.json` written by hand; Photoshop loads all nine on every launch and labels them `from Plugin Marketplace`, so a `status: "enabled"` row is sufficient and the registry is re-read at every launch | machine, folder mtimes, `system-information.txt` |
|  [06]   | No page requires a signature on a plain UXP `.ccx`; packages are unsigned, double-click install warns that the plugin is unverified, install error `-23` applies to hybrid plugins alone, and nine unsigned plugins load on this build | adobe Packaging page; helpx install errors 2026-07-03; machine |
|  [07]   | The UXP Developer Tool needs developer mode and elevated privileges, its `Load` does not survive a host relaunch, and it is distributed inside Creative Cloud desktop alone, so no target calls it. A human opens it to read the plugin console when `hello` never arrives | adobe UXP Developer Tool pages; machine |
|  [08]   | Uninstall is the inverse: delete the folder and the registry row | Row 05 |

Install is one target chain: `build` runs `vite build` through `vite-uxp-plugin`, and `deploy` (`dependsOn ["build"]`, `parallelism false`) removes any previous `rasm.photoshop.bridge_*` folder, copies the built folder to `<photoshop.pluginsFolder>/rasm.photoshop.bridge_<version>/`, decodes `<photoshop.registry>` through `Schema.fromJsonString(Registry)` and `Schema.decodeUnknownEffect`, replaces the row with the same `pluginId`, encodes it back with the file mode unchanged at `0600`, and prints `relaunch photoshop`. The folder name and `versionString` are the `uxp.config.ts` `version`, which bumps per deploy. `HOME` comes from `Config.String('HOME')`. The row written is:

```json
{
    "hostMinVersion": "27.11",
    "name": "Rasm Photoshop Bridge",
    "path": "$localPlugins/External/rasm.photoshop.bridge_1.0.0",
    "pluginId": "rasm.photoshop.bridge",
    "status": "enabled",
    "type": "uxp",
    "versionString": "1.0.0"
}
```

The readback is `jq '.plugins[] | select(.pluginId == "rasm.photoshop.bridge")'` over `PS.json`, `app.systemInformation` listing `Rasm Photoshop Bridge (Loaded)` with a load time and the panel closed, and `health` reporting `attached` within two ticks of the relaunch. No target calls the Unified Plugin Installer Agent and no allow row names it.

## [11]-[UNVERIFIED]

Every claim this file makes that no measurement or Adobe page settles. Each row names the unit whose research step reads the fact against the running host, the read that produces it, and the readback that replaces the row before that unit's builder starts. Unit 8 is the plugin and the tools, unit 12 the application pass.

| [INDEX] | [CLAIM] | [UNIT] | [READ] | [READBACK] |
| :-----: | :------ | :----: | :----- | :--------- |
|  [01]   | `ws://localhost:39217` from UXP reaches the server's `127.0.0.1` listener on 27.11 | 8 | Start the server, relaunch Photoshop with the plugin dialing `ws://localhost:39217`; on refusal retry with `domains: ["ws://127.0.0.1:39217"]` and that dial | The server log's connection line and `hello` frame, or an immediate `onclose` with no server line; the domain spelling that passes is written into `uxp.config.ts` |
|  [02]   | The plugin loads with `loadEvent: "startup"` and its panel never opened | 8 | Deploy a plugin whose script logs `versions.uxp`, relaunch, read `app.systemInformation` | `Rasm Photoshop Bridge (Loaded) 1.0.0.0` with a load time and no panel in the workspace |
|  [03]   | The Vite bundle keeps `require('photoshop')` and `require('uxp')` as host lookups and its ES2022 syntax runs on this UXP runtime | 8 | Deploy the real build whose source uses `??=`, a class field, and a top-level `await` inside an async IIFE; relaunch | `Loaded` with the two `require` results on the status line; a syntax failure shows as an error state in the same list |
|  [04]   | The Effect-carrying bundle's load cost under this UXP runtime | 8 | Read `core.getPluginInfo()` after load | `pluginLoadTime`, `launchTimeImpact`, `v8HeapSize` |
|  [05]   | The UXP WebSocket's maximum message size and its backpressure behaviour beyond the documented self-close | 8 | Send 1 MB and 16 MB JSON text frames each way | The byte count at which the socket closes, recorded as the spill threshold's justification |
|  [06]   | `_obj` casing on a failed descriptor and the `result` value | 8 | `batch_play` with a descriptor naming a nonexistent layer | The fulfilled array element printed verbatim |
|  [07]   | `e.number === 9` with the holder named in the message on 27.11 | 8 | `execute` with a modal body while TK9 Combo holds a scope | `e.number`, `e.message`, and the parsed holder |
|  [08]   | The value thrown after user cancel mid-job | 8 | Escape during a modal `execute` body | `result -128` or the thrown value |
|  [09]   | A redeploy at an unchanged version reloads the replaced folder on relaunch | 8 | Deploy twice at `1.0.0` with a changed status line, relaunch between | The status line of the second build, or the first, recorded as whether the version must bump |
|  [10]   | `presetManager` group indices per kind on 27.11 through UXP `batchPlay` | 8 | `list_presets` for each kind before `preset-groups.json` is filled | The group order and per-group counts against the machine read of brushes 28, swatches 122, gradients 183, styles 21, patterns 10, contours 12, and tool presets 21 |
|  [11]   | An `osascript` call during an open modal scope | 8 | `system_report` sent while an `executeAsModal` scope is open | The osascript error code and text, mapped in the classifier |
|  [12]   | A New Document preset written through `batchPlay` rather than the dialog | 8 | A `set` descriptor against the preset store, then `jq` over `New Doc Sizes.json` | Whether the `user` section gained the entry; the dialog stays the route when it did not |
|  [13]   | `featureFlags.uncaughtException` and `unhandledRejection` capture plugin exceptions on this runtime | 8 | Throw from a timer callback outside any handler | Whether the flagged handler receives it |
|  [14]   | Whether a `get_preferences` read of a locked key throws under Quiet Mode | 8 | Read the `notifications` class after `quietMode` is set | A value, or `PreferenceLocked{section, key}` |
|  [15]   | The value `useRichToolTips` holds while Quiet Mode is on | 8 | The same read plus the descriptor keys `useRichToolTips` and `useRichToolTipsRestore` | Both values, recorded in the Notifications rows of [13] |
|  [16]   | `.psjs` support on this build | 8 | Write a `.psjs` reading `require('uxp').host.version` beside a `.jsx`, open File > Scripts > Browse… | Whether the file is enabled or greyed; no target depends on the answer |
|  [17]   | Accessibility readability of the Settings dialog panes | 12 | System Events dump of the `Preferences` window with each of the 21 panes active | Per pane, every titled checkbox, popup, radio, and field with its value; a pane with no titled control switches its rows to `CU` |
|  [18]   | The four Interface theme swatch buttons' accessibility titles | 12 | The same dump on the Interface pane | The titles or their absence; absence makes the theme step `CU` |
|  [19]   | The current value of every preference row whose readback names the descriptor with no key | 12 | `batch_play` `multiGet` over the application's preference sub-descriptors | The key name and value per row, written into [13] |
|  [20]   | Identity of toolbar Show chips 5 and 6 | 12 | Hover each chip in the Customize Toolbar dialog, zoomed window-id screenshot | Two tooltip strings |
|  [21]   | The toolbar preset file extension | 12 | `Save Preset...` named `Default Toolbar`, then list `<photoshop.supportFolder>/Presets/Custom Toolbars/` | One file whose extension fills the persistence row and the Drive name |
|  [22]   | The `.psw` `size-variant` token of a double-column toolbar | 12 | `strings -n 6` over the saved workspace, matching `size-variant="[a-z-]*"` | One token differing from `vertical-narrow` |
|  [23]   | Whether dragging a sub-tool above its parent makes it the slot's visible tool, and the dialog order of the 72 tools | 12 | Drag Lasso above Selection Brush in the Customize Toolbar dialog, `Done`; scroll the left list with screenshots | The slot icon shows Lasso; every `inToolBar` tool appears once |
|  [24]   | Placement of the Brushes panel flyout view items | 12 | Open the flyout, screenshot | The five items with their check states |
|  [25]   | The group-delete command in the preset panels | 12 | Right-click a group, screenshot the context menu | Whether `Delete Group` appears |
|  [26]   | `⇧A` accepted on the four lasso tools beside the `A` slot | 12 | Type `⇧A` in each of the four Tools rows of the Keyboard Shortcuts dialog | The Shortcut column on the four rows and on Path Selection and Direct Selection, plus any conflict alert text |
|  [27]   | Which workspaces the `Delete Workspace...` popup offers | 12 | Accessibility read of the dialog popup | The listed names and whether `Essentials` is absent or refused |
|  [28]   | The Camera Raw Preferences generative control's pane and label in Camera Raw 18.6 | 12 | Screenshot every pane of the Camera Raw Preferences dialog | The pane list, each pane's controls, and the generative control set off |
|  [29]   | The Camera Raw Workflow Options preset control | 12 | Screenshot the Workflow Options dialog | Its controls; a preset control saves `Default Workflow` |
|  [30]   | The `New Doc Sizes.json` preset object shape on the `user` store | 12 | `jq '.sections[] \| select(.section == "user") \| .presets[0]'` after the first `Save Preset` | The key set against the sibling `MRU New Doc Sizes.json` shape `{name, identifier, group, width, height, units, profile, resolution, resolutionUnits, depth, scale, mode, fill, guides, artboards, lastUsedTime}`, plus the `units` strings for millimetres and pixels and the `mode` string for CMYK |
|  [31]   | Remove tool Options bar labels | 12 | Window-id screenshot with Remove selected | `Remove after each stroke`, `Sample all layers`, and the `Find distractions` menu with its three entries |

## [12]-[PASS_RULES]

The application pass configures the running host through the three channels no tool covers. Its rules, the Photoshop meaning of each, the decision, and the test that proves it:

| [INDEX] | [RULE] | [PHOTOSHOP_MEANING] | [DECISION] | [TEST] |
| :-----: | :----- | :------------------ | :--------- | :----- |
|  [01]   | No top panel | The one top strip is the Options bar (`Window > Options`, panel id `static.options`, control-bar dock anchored top) carrying the Home button at left, tool options in the middle, and the app-bar icons at right | Options bar stays. Adobe's workspace overview of 2026-06-05 states that the Options bar displays settings for the selected tool; Properties carries layer, document, and shape properties and never brush size, opacity, flow, selection mode, or sampling, and the Contextual Task Bar covers the workflows Adobe lists alone. Kept minimal: `Enable Narrow Options Bar` on, `Show AI Assisted Editor` off, Home screen auto-show off | Window > Options checkmark; a screenshot shows one strip |
|  [02]   | No help strip at the bottom, status bar kept | The document-window status bar holds the zoom field and the document readout with its `>` menu; this build has no separate hint strip, and `toggleStatusBar` is the one related binary string with no menu item | Nothing to remove. Status bar readout `Document Sizes`. The Info panel's `Show Tool Hints` is moot with Info closed | A screenshot with a document open shows the zoom field and `Doc:` readout and no hint text |
|  [03]   | Rulers enabled | `View > Rulers` (⌘R) is disabled with no document and its state persists per view | On, units Pixels | With the template open, `AXMenuItemMarkChar` of View > Rulers reads `✓` after relaunch |
|  [04]   | Every tool, two columns, icon-only, in tool-type sequence | Edit > Toolbar; the Tools panel `>>` chevron toggles single and double column; the sequence is Adobe's tools-overview order of selection, crop and slice, measure, retouch, paint, draw and type, navigate | [16]: 21 slots holding all 72 `inToolBar` tools, Extra Tools empty, extras chip hidden, double column, letters per [17] | A screenshot shows two columns over 11 rows; the saved workspace's toolbar `size-variant` token differs from `vertical-narrow`; the Keyboard Shortcuts Tools tab equals [17] |
|  [05]   | One panel structure across the applications | Arrangement group top, properties middle, and pages or artboards bottom in one column; colour group top, automations middle, and the layer list bottom in the next; export and inspection panels fold into the one icon dock when an application has a single dock | [14]: iconic pane A for type, library, and inspection panels; pane B 235 pt for the TK9 modules; pane C 280 pt for Properties and Adjustments alone, since Photoshop has no Align, Pathfinder, Artboards, or Links panel; pane D 340 pt for Swatches/Color/Gradients, then Actions/History, then Layers/Channels/Paths | The Window and Plugins menu checkmark set equals the open panels of [14]; the screenshot's column order matches |
|  [06]   | Low-value panels removed whether open or closed | The Window menu of this build holds 37 panel items and five toggles, and the saved `.psw` records every panel id with `closed` true or false | Every closed verdict of [14] is closed in the saved workspace rather than hidden | A `.psw` decode shows `closed="true"` for each |
|  [07]   | Panels we need added and configured | TK9 Multi-Mask and Combo docked; Channels and Paths grouped with Layers; Brush Settings grouped with Brushes | [14] rows 06, 08, 10, 11, 19, 20, 21, 22 | The Plugins menu checkmark on TK9 Multi-Mask and TK9 Combo; `app.systemInformation` lists both `Loaded` |
|  [08]   | Dark interface, smallest elements | Interface pane: Color Theme, UI Font Size (`Tiny`, `Small`, `Medium`, `Large`), `Scale UI To Font`, and the Technology Preview `Enable Modern User Interface` | Darkest theme, `Tiny`, scale off, Modern User Interface on | An accessibility read of the Interface pane after relaunch |
|  [09]   | Factory presets left in place, ours added beside them | The panels read the resident `.psp` stores; the files under `<photoshop.installFolder>/Presets/` are `root:admin` load-on-demand archives that no panel shows | No preset group is deleted and no library is imported: the pass changes tool presets, actions, type styles, and document presets alone | `list_presets` per kind equals the counts of [06] row 08 after the pass |
|  [10]   | Size catalogue as New Document presets and templates, and default type styles | A `.psdt` opens as an untitled `.psd` instance; `New Doc Sizes.json` holds the `user` presets; `Type > Save Default Type Styles` is a live menu item | [20]: one preset and one `.psdt` per catalogue row, print rows at 300 ppi and the digital master and screen rows at 72 ppi, the module carried as guides; `Default Template.psdt` is the `Digital 3840x2160` file | Opening each `.psdt` yields `Untitled-1` at the row's pixel size, resolution, profile, and guide count; `New Doc Sizes.json` holds one `user` preset per row; Load Default Type Styles lists Body, Heading, Caption |
|  [11]   | Everything saved as `Default *` and copied to Drive | [26] | Every artifact of [26] under `<drive.designLibrary>/05.Software Related Assets/99.Default Profiles/Photoshop/`, the catalogue under its `Sizes/` | The `exports.sha256` rows equal the on-disk hashes |
|  [12]   | Contextual Task Bar kept | `Window > Contextual Task Bar` | On and pinned through the bar menu `Pin bar position`; the bar menu of this build carries `Hide bar`, `Pin bar position`, `Reset bar position` | The Window menu checkmark and a screenshot with a document open |
|  [13]   | No assistant, promo, help, share, comment, learn, Home, or Discover surface in a saved layout, and no tool that edits pixels struck | Panels closed: AI Assistant, Adobe Stock, Beta Feedback, Comments, Content Credentials (Beta), Libraries, Version History, Materials. `Show AI Assisted Editor` off, Home auto-show off, and the matching menu rows hidden through Edit > Menus, which the workspace captures. Tools stay whatever model runs them: Remove with its two on-device bundles, Select > Subject, Select > Sky, Object Selection, Content-Aware Tracing, Camera Raw Denoise | Adobe fixes the Home button on the Options bar, the app-bar icons (Beta flask, Share, Notifications bell, Search, AI Assistant, workspace switcher), and the Discover panel extension: `showAIAssistant` and `showEmbeddedDiscoverPanel` exist as binary strings with no Settings control | A screenshot shows none of the closed panels; Edit > Menus shows the hidden rows; the fixed icons are recorded in [18]; toolbar slot 08 holds Remove |

## [13]-[PREFERENCES]

The Settings submenu of this build holds 21 panes: General, Interface, Workspace, Notifications, Tools, History, File Handling, Export, Performance, Image Processing, Scratch Disks, Cursors, Transparency & Gamut, Units & Rulers, Guides, Grid & Slices, Plugins, Type, Enhanced Controls, Technology Previews, Early Access, Product Improvement, then Camera Raw after a separator, which opens the Camera Raw Preferences dialog instead ([23]).

Each row reads `current → target`. Current values come from the ExtendScript `app.preferences` read, the Action Manager application descriptor, or the Settings pane read of [11] row 17. The channel column names one of the five channels; `descriptor` in the readback column means a `batch_play` `get` over the application's preference sub-descriptors. A row whose readback names the descriptor with no key takes its key name from [11] row 19.

`quietMode`, `showWhatsNew`, and `showFeatureOnboarding` are absent from the descriptor and from every `.psp`, so no persisted value exists and their current value is the `from` field of the `set_preferences` `applied` row. Inside the `notifications` class the write order is `showToolTips`, `useRichToolTips`, `showWhatsNew`, `showFeatureOnboarding`, then `quietMode`, because the first four become read-only once Quiet Mode is on and a later write to one of them answers `PreferenceLocked`.

| [INDEX] | [PANE] | [SETTING] | [CURRENT_TO_TARGET] | [CHANNEL] | [READBACK] |
| :-----: | :----- | :-------- | :------------------ | :-------- | :--------- |
|  [01]   | General | Color Picker | Adobe → Adobe | `general.colorPicker` | `get_preferences` |
|  [02]   | General | HUD Color Picker | Hue Strip (Small) → Hue Strip (Small) | AX popup | AX |
|  [03]   | General | Image Interpolation | Bicubic Automatic → Bicubic Automatic | `general.imageInterpolation` | `get_preferences` |
|  [04]   | General | Auto-Update Open File-based Documents | off → off | `general.autoUpdateOpenDocuments` | `get_preferences` |
|  [05]   | General | Auto show the Home Screen | off → off | AX checkbox | descriptor `autoShowHomeScreen false` |
|  [06]   | General | Use Legacy "New Document" Interface | off → off | AX | descriptor `useClassicFileNewDialog false` |
|  [07]   | General | Skip Transform when Placing | off → off | AX | descriptor `skipTransformSOFromLibrary` |
|  [08]   | General | Use Legacy Free Transform | off → off | AX | descriptor |
|  [09]   | General | Beep When Done | off → off | `general.beepWhenDone` | `get_preferences` |
|  [10]   | General | Export Clipboard | on → on | `general.exportClipboard` | `get_preferences` |
|  [11]   | General | Resize Image During Place | off → off | AX | descriptor `resizePastePlace false` |
|  [12]   | General | Always Create Smart Objects when Placing | on → on | AX | descriptor `placeRasterSmartObject true` |
|  [13]   | General | Create new layer when brushing | off → off | AX | descriptor `nonDestructiveBrushTool false` |
|  [14]   | Interface | Color Theme | Dark, the second of four swatch buttons, descriptor `kuiBrightnessLevel kPanelBrightnessMediumGray` → Darkest, the first swatch | CU click on the first swatch left of `Highlight Color` | descriptor `kuiBrightnessLevel`; screenshot |
|  [15]   | Interface | Highlight Color | Blue → Gray | AX popup | descriptor `highlightColorOption` |
|  [16]   | Interface | Standard Screen Mode canvas color and border | Default, Drop Shadow → same | none | descriptor |
|  [17]   | Interface | Full Screen canvas color | Black → Black | none | descriptor |
|  [18]   | Interface | Neutral Color Mode | off → off | AX | AX |
|  [19]   | Interface | UI Language | English → English | none | descriptor `uiLanguageKey` |
|  [20]   | Interface | UI Font Size | Small, descriptor `paletteEnhancedFontTypeKey preferSmallPaletteFontType` → Tiny (`Constants.FontSize.TINY`, persisted `preferTinyPaletteFontType`) | `interface.textFontSize`, effective after restart | `get_preferences`; descriptor `paletteEnhancedFontTypeKey`; zoomed screenshot |
|  [21]   | Interface | Scale UI To Font | off → off | AX | AX |
|  [22]   | Interface | Show Channels in Color | off → off | `interface.colorChannelsInColor` | `get_preferences` |
|  [23]   | Interface | Show Menu Colors | on → off | AX | descriptor `showMenuColors` |
|  [24]   | Interface | Show AI Assisted Editor | off → off | AX | AX checkbox; the persisted key is `showAIAssistedButton` in `MachinePrefs.psp` and is absent from the descriptor |
|  [25]   | Interface | Dynamic Color Sliders | on → on | `interface.dynamicColorSliders` | `get_preferences` |
|  [26]   | Interface | Show Simplified Right Click And Flyout Menus | off → off | AX | AX |
|  [27]   | Workspace | Auto-Collapse Iconic Panels | on → on | AX | descriptor `autoCollapseDrawers` |
|  [28]   | Workspace | Auto-Show Hidden Panels | off → off | AX | descriptor `autoShowRevealStrips` |
|  [29]   | Workspace | Open Documents as Tabs | on → on | AX | descriptor `openNewDocsAsTabs` |
|  [30]   | Workspace | Enable Floating Document Window Docking | on → on | AX | descriptor |
|  [31]   | Workspace | Large Tabs | off → off | AX | descriptor `enableLargeTabs` |
|  [32]   | Workspace | Enable Narrow Options Bar | on → on | AX | descriptor `enableNarrowOptionBar` |
|  [33]   | Workspace | Enable Native Full Screen | off → off | AX | descriptor |
|  [34]   | Notifications | Enable quiet mode | the `applied` row's `from` → on, written last in the class | `notifications.quietMode` | `get_preferences` over the class after the write; a second `showWhatsNew` write answers `PreferenceLocked` |
|  [35]   | Notifications | Tooltips | on → on | `notifications.showToolTips`, also `tools.showToolTips` | `get_preferences` |
|  [36]   | Notifications | Rich Tooltips | on, with `useRichToolTipsRestore true` holding the app's restore copy → on, written before the quiet write | `notifications.useRichToolTips` | `get_preferences` before the quiet write; descriptor `useRichToolTips` and `useRichToolTipsRestore` after it |
|  [37]   | Notifications | What's new | the `applied` row's `from` → off, written before the quiet write | `notifications.showWhatsNew` | `get_preferences` |
|  [38]   | Notifications | Feature Onboarding | the `applied` row's `from` → off, written before the quiet write | `notifications.showFeatureOnboarding` | `get_preferences` |
|  [39]   | Tools | Enable Gestures | on → on | AX | descriptor |
|  [40]   | Tools | Use Shift Key for Tool Switch | on → on | `tools.useShiftKeyForToolSwitch` | `get_preferences` |
|  [41]   | Tools | Overscroll | on → on | AX | descriptor |
|  [42]   | Tools | Enable Flick Panning | on → on | AX | descriptor `flick` |
|  [43]   | Tools | Double Click Layer Mask Launches Select and Mask | on → on | AX | descriptor |
|  [44]   | Tools | Vary Round Brush Hardness based on HUD vertical movement | on → on | AX | descriptor |
|  [45]   | Tools | Disable brush opacity based on HUD vertical movement | off → off | AX | descriptor |
|  [46]   | Tools | Arrow Keys Rotate Brush Tip | on → on | AX | descriptor |
|  [47]   | Tools | Use Paintbrush Tip to Erase While Using a Stylus Eraser | off → off | AX | descriptor |
|  [48]   | Tools | Snap Vector Tools and Transforms to Pixel Grid | on → on | AX | descriptor |
|  [49]   | Tools | Show Reference Point when using Transform | on → on | AX | descriptor |
|  [50]   | Tools | Spring-loaded Tool Shortcuts | on at 200 ms → same | AX | descriptor |
|  [51]   | Tools | Zoom with Scroll Wheel | off → off | AX | descriptor |
|  [52]   | Tools | Animated Zoom | on → on | AX | descriptor `animationKey` |
|  [53]   | Tools | Zoom Resizes Windows | off → off | `tools.keyboardZoomResizesWindows` | `get_preferences` |
|  [54]   | Tools | Zoom Clicked Point to Center | off → off | AX | AX |
|  [55]   | Tools | Show HUD | Top Right → Top Right | AX popup | descriptor `showHUD` |
|  [56]   | History | History Log | off → off | `history.useHistoryLog` | `get_preferences` |
|  [57]   | History | History States | 50 → 50 | `history.numberOfHistoryStates` | `get_preferences` |
|  [58]   | History | Content Credentials document options | None → None; `Ask when opening` on → off; export None | AX checkbox | descriptor `contentCredentialsDocumentAsk` |
|  [59]   | File Handling | Image Previews | Always Save with Thumbnail on → same | `fileHandling.imagePreviews` | `get_preferences` |
|  [60]   | File Handling | Append Extension | Always, Use Lower Case → same | `fileHandling.useLowerCaseExtension` | `get_preferences` |
|  [61]   | File Handling | Default File Location | On your computer, `defaultCloudSave false` → same | AX popup | descriptor |
|  [62]   | File Handling | Save As to Original Folder | on → on | AX | descriptor `FileSaveToOriginalFolder` |
|  [63]   | File Handling | Save in Background | on → on | AX | descriptor |
|  [64]   | File Handling | Automatically Save Recovery Information | on at 5 min → same | AX | descriptor |
|  [65]   | File Handling | Enable legacy "Save As" | off → off | AX | descriptor `fileLegacySaveAs` |
|  [66]   | File Handling | Do not append "copy" | off → off | AX | descriptor `fileDoNotAppendCopy` |
|  [67]   | File Handling | Prefer Adobe Camera Raw for Supported Raw Files | on → on | AX | descriptor `cameraRaw` |
|  [68]   | File Handling | Use Camera Raw to Convert 32 to 16 or 8 bit | off → off | AX | descriptor `preferACRForHDRToning` |
|  [69]   | File Handling | Ignore EXIF Profile Tag, Ignore Rotation Metadata | off, off → same | AX | descriptor |
|  [70]   | File Handling | Ask Before Saving Layered TIFF | on → on | `fileHandling.askBeforeSavingLayeredTIFF` | `get_preferences` |
|  [71]   | File Handling | Disable Compression of PSD and PSB | off → off | AX | descriptor `disablePSDCompression` |
|  [72]   | File Handling | Maximize PSD and PSB Compatibility | Always → Always | `fileHandling.maximizeCompatibility` | `get_preferences` |
|  [73]   | File Handling | Recent File List Contains | 20 → 20 | `fileHandling.recentFileListMaximum` | `get_preferences` |
|  [74]   | Export | Quick Export Format | PNG, Transparency on, Smaller File off → same | AX | descriptor `exportFileType` |
|  [75]   | Export | Quick Export Location | `exportAsLocationSetting 2` → Ask where to export each time | AX radio | descriptor |
|  [76]   | Export | Export As metadata and Convert to sRGB | None, on → same | AX | descriptor |
|  [77]   | Export | Export Assets location | `exportAssetsLocationSetting 3` → last location specified | AX radio | descriptor |
|  [78]   | Performance | Let Photoshop Use | 70 % → 70 % | `performance.maxRAMuse` | `get_preferences` |
|  [79]   | Performance | Cache Levels and Tile Size | 4, 1024K → same | `performance.imageCacheLevels`; tile through AX | `get_preferences` |
|  [80]   | Performance | Use Graphics Processor | on → on | AX | `app.systemInformation` `useGPU 1` |
|  [81]   | Performance | Advanced: OpenCL, GPU compositing, anti-aliased guides, 30-bit display | on, on, on, on → same | AX | descriptor `openglAdvanced` |
|  [82]   | Performance | Multithreaded compositing, Foreground composite caching, Multithreaded PSD reading | on, on, on → same | AX | descriptor |
|  [83]   | Image Processing | Select Subject and Remove Background | Device → Device | AX popup | descriptor `imageProcessingSelectSubjectPrefs` |
|  [84]   | Image Processing | Selections, Remove, Enhance processing | Faster → Faster | AX | descriptor `imageProcessingRemoveToolProcessingPrefsStr imageProcessingPerformantMode` |
|  [85]   | Scratch Disks | Startup volume alone | on → on | AX | `app.systemInformation` |
|  [86]   | Cursors | Painting and Other | Normal Brush Tip, Precise → same | `cursors.paintingCursors`, `cursors.otherCursors` | `get_preferences` |
|  [87]   | Cursors | Crosshair in tip on; only crosshair while painting off; no cursor while painting off; brush leash off | same → same | AX | descriptor |
|  [88]   | Transparency & Gamut | Grid Size, Colors, Gamut warning | Medium, Light, gray at 100 % → same | `transparencyAndGamut.gridSize`, `.gamutWarningOpacity` | `get_preferences` |
|  [89]   | Units & Rulers | Rulers and Type | Pixels, Pixels → same | `unitsAndRulers.rulerUnits`, `.typeUnits` | `get_preferences` |
|  [90]   | Units & Rulers | Column 180 px, Gutter 12 px | same → same | AX | descriptor |
|  [91]   | Units & Rulers | New Document Preset Resolutions | Print 300, Screen 72 → same | AX | descriptor `newDocPresetPrintResolution 21600`, `newDocPresetScreenResolution 5184` |
|  [92]   | Units & Rulers | Point and Pica Size | PostScript → PostScript | `unitsAndRulers.pointSize` | `get_preferences` |
|  [93]   | Guides, Grid & Slices | Guides | Cyan, solid, 1 px → same | `guidesGridsAndSlices.guideStyle`; colour through AX | `get_preferences` |
|  [94]   | Guides, Grid & Slices | Artboard guides | Light Blue active solid, inactive dashed, inactive hidden → same | AX | descriptor |
|  [95]   | Guides, Grid & Slices | Smart Guides | Magenta → Magenta | AX | descriptor |
|  [96]   | Guides, Grid & Slices | Grid | Custom gray, solid, 64 px, 4 subdivisions → Custom gray, solid, gridline every 45 px, 3 subdivisions, the digital master module and its 15 px third | `guidesGridsAndSlices.gridStyle`, `.gridSubDivisions`; gridline size and unit through AX | `get_preferences`; descriptor grid size |
|  [97]   | Guides, Grid & Slices | Slices | Light Blue, numbers on → same | `guidesGridsAndSlices.showSliceNumber` | `get_preferences` |
|  [98]   | Plugins | Show all Filter Gallery groups and names | off → off | AX | descriptor `pluginPicker.showAllFilterGalleryEntries` |
|  [99]   | Plugins | Enable Developer Mode | off → off | AX | descriptor `pluginPicker.enablePluginDeveloperMode` |
|  [100]  | Plugins | Enable Generator | off → off | AX | descriptor `generatorEnabled` |
|  [101]  | Plugins | Enable Remote Connections | off → off | AX | AX |
|  [102]  | Plugins | Allow Extensions to Connect to the Internet | on → off | AX | AX |
|  [103]  | Plugins | Load Extension Panels | on, `extensionsOn true` → off, effective after restart | AX | descriptor; `app.systemInformation` shows no `CC Libraries Panel` row |
|  [104]  | Type | Use Smart Quotes | on → on | `type.smartQuotes` | `get_preferences` |
|  [105]  | Type | Enable Missing Glyph Protection | on → on | AX | descriptor `enableFontFallback` |
|  [106]  | Type | Show Font Names in English | on → on | `type.showEnglishFontNames` | `get_preferences` |
|  [107]  | Type | Use ESC key to commit text | on → on | AX | descriptor `textToolTreatsESCAsCommit` |
|  [108]  | Type | Set default font size automatically | off → off | AX | descriptor `enableAutoDefaultFontSize` |
|  [109]  | Type | Enable Type layer glyph alternates | on → on | AX | descriptor `enableGlyphAlternate` |
|  [110]  | Type | Automatic detection of lists | off → off | AX | descriptor `autoListDetection` |
|  [111]  | Type | Fill new type layers with placeholder text | off → off | AX | descriptor `enablePlaceHolderText` |
|  [112]  | Type | Language Options | Default Features, descriptor `textComposerChoice defaultTextInterface` → Middle Eastern Features | `type.showTextFeatures = 'middleEasternInterface'` | `get_preferences`; Type > Language Options > Middle Eastern Features checkmark |
|  [113]  | Type | Font Preview Size | Large → Small | `osascript` `app.preferences.fontPreviewSize` | ExtendScript readback; Type > Font Preview Size > Small checkmark |
|  [114]  | Type | Recent fonts | 10 → 10 | none | descriptor `textToolRecentFontDisplayNumber` |
|  [115]  | Enhanced Controls | Show Touch Bar property adjustments | on → off | AX | descriptor `enhancedControlsTouchBarPropertyFeedback` |
|  [116]  | Technology Previews | Preserve Details 2.0 Upscale | on → on | AX | descriptor `expFeatureDeepUpscale` |
|  [117]  | Technology Previews | Content-Aware Tracing Tool | on → on | AX | descriptor `expFeatureContentAwareTracing` |
|  [118]  | Technology Previews | Precise color management for HDR display | on → on | AX | descriptor `expFeaturePreciseHDRColorManagement` |
|  [119]  | Technology Previews | Precise previews for 16-bit documents | on → on | AX | descriptor |
|  [120]  | Technology Previews | Open JPEG without Background layer | on → on | AX | AX |
|  [121]  | Technology Previews | Enable Modern User Interface | on → on | AX | descriptor `DroverUI true` |
|  [122]  | Early Access | No feature listed | empty → empty | none | descriptor `earlyAccessPrefs` empty |
|  [123]  | Product Improvement | Participation | off → off | AX | AX |

Three View menu states belong to the pass rather than to a pane: `View > Rulers` on with the template open, `View > Snap` on, and every `View > Show > Extras` option on, each proven by its `AXMenuItemMarkChar`. Photoshop has no font-activation preference: the Type pane holds eight controls and none of them activates a font.

## [14]-[PANELS]

The Window menu of this build, in menu order: Arrange, Workspace, Actions, Adjustments, Adobe Stock, AI Assistant, Beta Feedback, Brush Settings, Brushes, Channels, Character, Character Styles, Clone Source, Color, Comments, Content Credentials (Beta), Glyphs, Gradients, Histogram, History, Info, Layer Comps, Layers, Libraries, Materials, Measurement Log, Navigator, Notes, Paragraph, Paragraph Styles, Paths, Patterns, Properties, Shapes, Styles, Swatches, Timeline, Tool Presets, Version History, then the five toggles AI Assisted Editor, Application Frame, Options, Tools, Contextual Task Bar. Checked panels before the pass: Color, Layers, Properties. The Plugins menu holds Plugins Panel, Browse Plugins..., Manage Plugins..., TK9 Combo, TK9 Cx, TK9 Export, TK9 Multi-Mask, TK9 My Actions, and TK9 My Actions-Tab 1 to 4, each TK9 module carrying an `info` tooltip panel.

The Window > Workspace submenu holds Core Tools, Editorial Images, Essentials (Default), Motion, Painting, Photography, Graphic and Web, Reset Editorial Images, New Workspace..., Delete Workspace..., Keyboard Shortcuts & Menus..., and Lock Workspace. No restore item sits on the submenu: `Restore Default Workspaces` is a button in the Workspace pane of Settings.

Dock structure, right dock, panes inner to outer: pane A iconic with auto-collapse on holds the type, library, and inspection panels; pane B at 235 pt holds the TK9 modules; pane C at 280 pt holds the properties slot; pane D at 340 pt holds the colour group at top, the automations group in the middle, and Layers at the bottom. The canvas takes the remainder of the 1800 pt window after the two-column toolbar and pane A. The left dock holds the toolbar in two columns and the bottom dock is empty. Heights at the 1081 pt window follow a 220, 220, remainder pattern: pane D gives 220 to the Swatches group, 220 to the Actions group, and the remainder to Layers; pane C gives Properties full height; pane B gives 480 to Multi-Mask, the remainder to Combo, and a minimized tab at the bottom to Export. The measured layout before the pass is one main column of 346 pt and a TK9 column of 232 pt.

| [INDEX] | [PANEL] | [ID] | [CURRENT] | [VERDICT] | [PLACE] |
| :-----: | :------ | :--- | :-------- | :-------- | :------ |
|  [01]   | Swatches | `static.swatches` | Open, main pane group 1 | Docked expanded | D, group 1, order 1, front tab |
|  [02]   | Color | `static.picker` | Open | Docked expanded | D, group 1, order 2 |
|  [03]   | Gradients | `static.gradients` | Open | Docked expanded | D, group 1, order 3 |
|  [04]   | Patterns | `static.patterns` | Open | Docked iconic | A, order 6, a library panel |
|  [05]   | Styles | `static.styles` | Collapsed group | Docked iconic | A, order 7 |
|  [06]   | Shapes | `static.customshapes` | Collapsed group | Docked iconic | A, order 8 |
|  [07]   | Actions | `static.actions` | Collapsed group | Docked expanded | D, group 2, order 1, the automations slot |
|  [08]   | History | `static.history` | Collapsed group | Docked expanded | D, group 2, order 2 |
|  [09]   | Layers | `static.layers` | Open | Docked expanded | D, group 3, order 1, tallest |
|  [10]   | Channels | `static.channels` | Open | Docked expanded | D, group 3, order 2 |
|  [11]   | Paths | `static.paths` | Open | Docked expanded | D, group 3, order 3 |
|  [12]   | Properties | `static.properties` | Open | Docked expanded | C, group 1, order 1, full height |
|  [13]   | Adjustments | `static.create` | Open | Docked expanded | C, group 1, order 2 |
|  [14]   | Character | `static.textcharacter` | Collapsed group | Docked iconic | A, order 1; Properties carries type options for a type layer |
|  [15]   | Paragraph | `static.textparagraph` | Collapsed group | Docked iconic | A, order 2 |
|  [16]   | Glyphs | `static.textglyphspanel` | Collapsed group | Docked iconic | A, order 3 |
|  [17]   | Character Styles | `static.textcharstyle` | Collapsed group | Docked iconic | A, order 4 |
|  [18]   | Paragraph Styles | `static.textparastyle` | Collapsed group | Docked iconic | A, order 5 |
|  [19]   | Brushes | `static.brushpresets` | Collapsed group | Docked iconic | A, order 9 |
|  [20]   | Brush Settings | `static.brushstyler` | Collapsed group | Docked iconic | A, order 10, grouped with Brushes |
|  [21]   | TK9 Multi-Mask | `uxp/com.tk.multimask/tkmultimaskv9` | Open | Docked expanded | B, group 1 |
|  [22]   | TK9 Combo | `uxp/com.tk.comboV8/tkcombocxv9` | Open | Docked expanded | B, group 2 |
|  [23]   | TK9 Export | `uxp/com.tk.export/tkexportv9` | Docked minimized | Docked minimized | B, bottom tab; Photoshop has no flyout panel form |
|  [24]   | Clone Source | `static.clonesource` | Collapsed group | Docked iconic | A, order 11, inspection |
|  [25]   | Layer Comps | `static.comps` | Collapsed group | Docked iconic | A, order 12 |
|  [26]   | Tool Presets | `static.toolpresets` | Collapsed group | Docked iconic | A, order 13 |
|  [27]   | TK9 My Actions | `uxp/com.tk.myactionsV8/tkmyactions` | Docked minimized, unconfigured | Closed | The Actions panel holds `Default Actions`; no second action list exists |
|  [28]   | TK9 My Actions-Tab 1 to 4 | `uxp/com.tk.myactionstab1` to `tab4` | Closed | Closed | Same |
|  [29]   | TK9 Cx | `uxp/com.tk.cxV8/tkcombocxv9` | Closed | Closed | Combo carries the same functions at the same width |
|  [30]   | TK9 `info` tooltip panels, five | `uxp/com.tk.*/toolTips` | Closed | Closed | Help surface |
|  [31]   | Plugins Panel | `uxp/com.adobe.pluginspanel/pluginsPanel` | Closed | Closed | Marketplace surface |
|  [32]   | Info | `static.info` | Closed | Closed | The status bar and Properties carry the same readouts |
|  [33]   | Navigator | `static.navigator` | Collapsed group | Closed | The status bar zoom field and the Hand and Zoom tools replace it |
|  [34]   | Histogram | `static.histogram` | Collapsed group | Closed | Curves and Levels show histograms in Properties |
|  [35]   | Timeline | `static.animation`, `uxp/com.adobe.ccx.timeline/ccxTimeline` | Bottom dock, open | Closed | Not a still-image surface; the bottom dock goes with it |
|  [36]   | Measurement Log | `static.measurement` | Closed | Closed | Same |
|  [37]   | Notes | `static.annotation` | Collapsed group | Closed | Comment surface |
|  [38]   | Comments | `uxp/com.adobe.ccx.comments-webview` | Closed | Closed | Comment surface |
|  [39]   | Libraries | `uxp/com.adobe.cclibrariespanel/ccLibrariesPanel`, `.psw` id 57 | Open, in the Properties group | Closed, and `Load Extension Panels` off so the extension never loads | Promo and sync surface |
|  [40]   | AI Assistant | Window item | Closed | Closed | Assistant surface |
|  [41]   | Adobe Stock | `uxp/com.adobe.stock.unified.content.panel` | Closed | Closed | Promo |
|  [42]   | Beta Feedback | `uxp/com.adobe.bfp.betafeatures` | Closed | Closed | Feedback surface |
|  [43]   | Content Credentials (Beta) | `uxp/com.adobe.cai.uxp/panel` | Closed | Closed | Credentials preferences read `none` |
|  [44]   | Version History | Window item | Closed | Closed | Cloud surface |
|  [45]   | Materials | `uxp/com.adobe.photoshop-material-filters`, three panels | Closed | Closed | Substance surface |
|  [46]   | Photoshop Utility Panel | `uxp/com.adobe.unifiedpanel/panel` | Closed | Closed | Discover and search host |
|  [47]   | Share sheet panels | `uxp/com.adobe.ccx.sharesheet/invite`, `/review` | Closed | Closed | Share surface |
|  [48]   | OCIO, Patch Match, Smart Brush | `static.ocio`, `static.patchmatch*`, `static.smartbrush` | Closed | Closed | Workspace-internal panels, never docked |
|  [49]   | AI Assisted Editor | Window toggle | Off | Off | Set in the Interface pane |
|  [50]   | Application Frame | Window toggle | On | On | One frame across the applications |
|  [51]   | Options | Window toggle | On | On | [12] row 01 |
|  [52]   | Tools | Window toggle | On | On | The toolbar of [16] |
|  [53]   | Contextual Task Bar | Window toggle | On | On and pinned | [12] row 12 |
|  [54]   | Workspace > Lock Workspace | Window toggle | Off | Off | A locked dock refuses the TK9 tab drags and hides nothing |

## [15]-[PANEL_OPTIONS]

| [INDEX] | [PANEL] | [OPTION] | [CURRENT_TO_TARGET] | [CHANNEL] |
| :-----: | :------ | :------- | :------------------ | :-------- |
|  [01]   | Layers | Thumbnail Size | `layerThumbnailSize medium` → Small | CU on the `Panel Options...` flyout; readback descriptor `layerThumbnailSize` |
|  [02]   | Layers | Thumbnail Contents | Layer Bounds → Layer Bounds | Same dialog |
|  [03]   | Layers | Use Default Masks on Fill Layers | on → on | Same dialog |
|  [04]   | Layers | Expand New Effects | on → off | Same dialog |
|  [05]   | Layers | Add "copy" to Copied Layers and Groups | on → off | Same dialog |
|  [06]   | Layers | Filter row | Shown → shown | Fixed |
|  [07]   | Channels, Paths | Thumbnail Size | Medium → Small | CU |
|  [08]   | Swatches | View | Small Thumbnail → Small Thumbnail, Show Recent Colors off | CU flyout |
|  [09]   | Gradients, Styles, Shapes, Patterns | View | Small Thumbnail → Small Thumbnail, Show Recent off | CU flyout, opened from pane A |
|  [10]   | Brushes | View | Brush Tip on, Brush Name on, Brush Stroke off, Show Additional Preset Info off, Show Recent Brushes off, thumbnail slider at minimum | CU flyout, opened from pane A |
|  [11]   | Brush Settings | Live tip preview strip | Default → default | none |
|  [12]   | Actions | Button Mode | off → off, one set expanded | CU flyout |
|  [13]   | History | History Options | `createFirstSnapshot true`, `nonLinearHistory false` → same; Show New Snapshot Dialog off; Make Layer Visibility Changes Undoable off | `set_preferences` for the two class keys, CU for the two dialog-only boxes |
|  [14]   | Character, Paragraph | Show options | Default → expanded | CU flyout, opened from pane A |
|  [15]   | Tool Presets | Flyout view | Show All Tool Presets → `Show Current Tool Presets`, `Small List` | CU flyout |
|  [16]   | Color | Model | Hue Cube → Hue Cube | none |
|  [17]   | Properties, Adjustments | Defaults | Keep | none |
|  [18]   | Clone Source, Layer Comps | Defaults | Keep | none |
|  [19]   | TK9 modules | Button saturation | Combo `colorOpacityValue 1` → 1 | TK9 flyout |

## [16]-[TOOLBAR]

Facts of this build. The Customize Toolbar dialog (`Edit > Toolbar...`) exposes its buttons to the accessibility reader alone (`Done`, `Cancel`, `Restore Defaults`, `Clear Tools`, `Save Preset...`, `Load Preset...`, and the checkbox `Disable Shortcuts for Hidden Toolbar Extras`); both tool lists are Drover-drawn, so every move is a computer-use drag. No `Toolbar Customization.psp` exists in the settings folder, so the current toolbar is the 27.11 factory default and the Extra Tools column is empty. Adobe's page documents dragging tools and groups to reorganize the toolbar, moving less frequently used tools to Extra Tools, a Toggle that shows extra tools in the last toolbar slot, `Save Preset` and `Load Preset` for a custom layout, `Restore Defaults`, `Clear Tools`, `Done`, and `Cancel`, and names no file path. The preset folder is `<photoshop.supportFolder>/Presets/Custom Toolbars/`, present and empty, matching the binary's `DefaultCustomToolbarPresetsDir=Custom Toolbars`. The Show row at the bottom of the dialog holds six chips: the `…` extras slot, foreground and background, Quick Mask, Screen Mode, and two more.

The tool set is the `tools[]` array of the application descriptor: 113 entries, of which 72 carry `inToolBar true` and belong in a slot. The 73rd `inToolBar` entry is `editToolbar`, the `Edit Toolbar...` chip. Entries with `inToolBar false` are workspace-internal (Select and Mask, Content-Aware Fill, Neural Filters, Liquify, the adjustment scrubbers, `removeBrushTool`, `distort`, and the two `place*` tools) and never sit in a slot. Each entry's `toolTip` carries the factory letter in parentheses, as in `Brush Tool (B)`, and the tool names below are those tooltips without the letter.

Two columns fill slots in reading order, two per row: 21 slots over 11 rows, every one of the 72 tools placed once.

| [INDEX] | [ROW.COL] | [SLOT_TOOLS_IN_ORDER] | [KEY] | [GROUP] | [REASON] |
| :-----: | :-------- | :-------------------- | :---- | :------ | :------- |
|  [01]   | 1.1 | Move, Artboard | `V` | Move | Factory group, visible tool Move |
|  [02]   | 1.2 | Rectangular Marquee, Elliptical Marquee, Single Row Marquee, Single Column Marquee | `U` from `M`; Single Row and Single Column unassigned | Select: geometric | Factory group |
|  [03]   | 2.1 | Lasso, Polygonal Lasso, Magnetic Lasso, Selection Brush | `⇧A` from `L` | Select: freehand | Factory group reordered so Lasso is the visible tool |
|  [04]   | 2.2 | Object Selection, Quick Selection, Magic Wand | `W` | Select: automatic | Factory group |
|  [05]   | 3.1 | Crop, Perspective Crop, Slice, Slice Select | `C` | Crop | Factory group |
|  [06]   | 3.2 | Frame | `K` | Frame | Single tool beside Crop, both defining bounds |
|  [07]   | 4.1 | Eyedropper, Color Sampler, Ruler, Note, Count | `I` | Measure and sample | Factory group |
|  [08]   | 4.2 | Spot Healing Brush, Healing Brush, Patch, Content-Aware Move, Red Eye, Remove | `J` | Retouch | Factory group; Remove (`removeTool`) is the sixth tool of the slot and runs on the two on-device bundles |
|  [09]   | 5.1 | Brush, Pencil, Color Replacement, Mixer Brush, Adjustment Brush | `B`; Pencil `N` from `B`; Adjustment Brush unassigned | Paint | Factory group |
|  [10]   | 5.2 | Clone Stamp, Pattern Stamp | `S` | Stamp | Factory group beside Paint |
|  [11]   | 6.1 | History Brush, Art History Brush | `Y` | History paint | Factory group |
|  [12]   | 6.2 | Eraser, Background Eraser, Magic Eraser | `E` | Erase | Factory group |
|  [13]   | 7.1 | Gradient, Paint Bucket | `G` | Fill | Factory group |
|  [14]   | 7.2 | Blur, Sharpen, Smudge | none | Focus | Factory group |
|  [15]   | 8.1 | Dodge, Burn, Sponge | `O` | Tone | Factory group beside Focus |
|  [16]   | 8.2 | Pen, Freeform Pen, Curvature Pen, Content-Aware Tracing, Add Anchor Point, Delete Anchor Point, Convert Point | `P`; the three anchor tools unassigned | Vector: draw | Factory group; Content-Aware Tracing is present because its Technology Preview is on |
|  [17]   | 9.1 | Horizontal Type, Vertical Type, Horizontal Type Mask, Vertical Type Mask | `T` | Type | Factory group |
|  [18]   | 9.2 | Path Selection, Direct Selection | `A` | Vector: select | Factory group beside Type and Shapes |
|  [19]   | 10.1 | Rectangle, Ellipse, Triangle, Polygon, Line, Star, Custom Shape | `M` from `U`; Ellipse `L`; Line `Q` from `U` | Shapes | Factory group; `StarTool` carries `inToolBar true` on this build |
|  [20]   | 10.2 | Hand, Rotate View | `H`, `R` | Navigate | Factory group |
|  [21]   | 11.1 | Zoom | `Z` | Navigate | Factory single |

A slot's tools share their letter and `⇧letter` cycles them, since `Use Shift Key for Tool Switch` is on. Dialog decisions:

| [INDEX] | [DIALOG_ITEM] | [DECISION] | [MECHANICS] |
| :-----: | :------------ | :--------- | :---------- |
|  [22]   | Extra Tools column | Empty, as it is now | No tool loses its function to another; every tool keeps a slot |
|  [23]   | `…` extras chip | Hidden | The extras list is empty; chip click in the Show row through CU |
|  [24]   | Foreground and background chip | Shown | Needed for `D` and `X` feedback |
|  [25]   | Quick Mask chip | Shown | Mask work with the TK9 modules |
|  [26]   | Screen Mode chip | Hidden | `F` cycles the modes |
|  [27]   | Chips 5 and 6 | Hidden | Both are non-tool surfaces added after the four classic chips; their tooltips come from [11] row 20 |
|  [28]   | `Disable Shortcuts for Hidden Toolbar Extras` | Unchecked, as it is now | Nothing is hidden |
|  [29]   | Slot order | Drag whole groups in the left list through CU; reorder inside a slot by dragging the sub-tool to the top | One inner drag, Lasso above Selection Brush; every group keeps its factory membership |
|  [30]   | Two columns | Tools panel chevron `>>` clicked through CU after `Done` | Recorded in the workspace `.psw` as the toolbar dock entry's `size-variant` |
|  [31]   | Persistence | `Toolbar Customization.psp` and `Toolbar Customization Primary.psp` written on quit, plus `Save Preset...` named `Default Toolbar` under `Presets/Custom Toolbars/` | The New Workspace dialog captures Panel Locations, Keyboard Shortcuts, and Menus; the `.psw` holds the toolbar as a dock entry with its `size-variant` and not as a slot list, so both `.psp` files and the preset are saved and copied |

Photoshop's two installed add-ons are on-device model bundles under `AddOnModules/sensei_model_cache/inpainting_ai/`: `super_caf/` at 686 MB holding `clio_multidiffusion`, `CMGAN_Tiny_V3`, `GeneralDistractor`, `MDCuration`, `PeopleDistractorV1`, `TiledCMGANSR_640px`, `WireGlobal`, and `WireLocal`, and `ultra_caf/` at 4.9 GB holding `Nano`, `Nano_data_1`, and `Nano_data_2`, with a 4.1 GB user copy of `ultra_caf/Nano`. Neither is a UXP extension, so `Load Extension Panels` off leaves both untouched.

| [INDEX] | [ADD_ON] | [PANEL] | [TOOL] | [MENU_ITEM] | [PROOF] |
| :-----: | :------- | :------ | :----- | :---------- | :------ |
|  [32]   | Remove Tool components, `super_caf/` | None: the Window menu gains no item and the `.psw` no panel id. The controls sit in the Options bar (`Required/layouts/Painting/Tools/removeBrushToolOptions-4175.exv` with the flyouts `RemoveToolRemovalAreaFlyout.eve`, `RemoveToolFindDistractionsFlyout.eve`, `RemoveToolGenAIModeFlyout.eve`) and, from 27.9.1, in the Contextual Task Bar | Remove (`removeTool`, `inToolBar true`), slot 08 of the retouch group and sixth in that slot's sequence; `removeBrushTool` carries `inToolBar false` and is the internal brush | None: no menu item of this build names the tool, and the Contextual Task Bar `Remove` button stays visible | Listing both `inpainting_ai/` folders; an Options bar screenshot with Remove selected shows Size, `Remove after each stroke`, `Sample all layers`, and the `Find distractions` menu with its three events |
|  [33]   | Remove Tool advanced components, `ultra_caf/` | None, as row 32 | The same Remove tool: the advanced bundle serves the `Find distractions` modes and the on-device fill quality at Image Processing Remove `Faster` (descriptor `imageProcessingRemoveToolProcessingPrefsStr imageProcessingPerformantMode`), and the generative-mode flyout stays at its on-device setting | None | The descriptor value after the pass; one Remove stroke on the template completes with no download prompt while `Allow Extensions to Connect to the Internet` is off |

## [17]-[SHORTCUTS]

One shortcut scheme covers every application: a tool keeps the letter its shape family owns, the line family sits on `Q` and `⇧Q`, a tool more than one application holds carries one letter everywhere (the shared factory letter where the factory sets agree, Illustrator's where they differ), a tool one application alone holds keeps its factory key, and a tool shortcut is a single key or `⇧` plus a key. Photoshop's factory letters are the `toolTip` suffixes of `tools[]` and the `[KEY]` column of [16]; the live list is the Tools category of `Edit > Keyboard Shortcuts`, read before any change. Photoshop's delta under that scheme, everything else factory:

| [INDEX] | [PHOTOSHOP_TOOL] | [FACTORY] | [HOUSE] | [REASON] | [PROOF] |
| :-----: | :--------------- | :-------- | :------ | :------- | :------ |
|  [01]   | `Toggle Standard/Quick Mask Modes`, a Tools-list command | `Q` | none | The line family takes `Q`; the Quick Mask chip stays the entry | The Shortcut cell is empty after `Delete Shortcut` |
|  [02]   | Line | `U`, shared with the shape slot | `Q` | The line family owns `Q` in every application | The Shortcut cell reads `Q`; `Q` selects Line and `⇧U` cycles the other shape tools |
|  [03]   | Lasso, Polygonal Lasso, Magnetic Lasso, Selection Brush | `L` | `⇧A` on all four | The freehand selection family sits beside Direct Selection on `A` | The four Shortcut cells read `⇧A` while Path Selection and Direct Selection keep `A`, with no conflict alert |
|  [04]   | Pencil | `B`, shared with the paint slot | `N` | Pencil is `N` in every application | The Shortcut cell reads `N`; `⇧B` cycles Brush, Color Replacement, Mixer Brush |
|  [05]   | Rectangle, Triangle, Polygon, Star, Custom Shape | `U` | `M` | Rectangle is `M` in every application | The Shortcut cells read `M`; `⇧M` cycles the shape slot |
|  [06]   | Ellipse | `U` | `L` | Ellipse is `L` in every application, and row 03 frees `L` | The Shortcut cell reads `L` |
|  [07]   | Rectangular Marquee, Elliptical Marquee | `M` | `U`, the letter the shape slot frees | The marquee family takes the freed letter | Both Shortcut cells read `U`; `⇧U` cycles the two; Single Row and Single Column Marquee stay unassigned |
|  [08]   | Every other tool | Its factory letter | Unchanged: `V`, `A`, `P`, `T`, `B`, `E`, `G`, `I`, `H`, `Z`, `W`, `C`, `K`, `J`, `S`, `Y`, `O`, `R`, and the Tools-list commands `D`, `X`, `F`, `/`, `[`, `]`, `{`, `}`, `,`, `.`, `<`, `>` | A tool one application alone holds keeps its factory key | The `Summarize` HTML Tools rows equal the `[KEY]` column of [16] with rows 01 to 07 applied |

The set is saved as `Default Keyboard Shortcuts`, and the Legacy Undo and Legacy Channel boxes stay off. A conflict alert on any row is answered with `Accept and Go To Conflict` and the conflicting row's cell is read and recorded.

## [18]-[MENUS_AND_SURFACES]

Edit > Menus hides the rows below through the Visibility button per row, and `Save Set` names the set `Default Menus`. Menus are workspace state and the New Workspace dialog's Menus box captures them; a hidden row returns through `Show All Menu Items` or a ⌘-click. Hidden rows, each an item of the menu bar read:

- File: Invite to Edit..., Share for Review, Export > Send to Firefly Boards, Search Adobe Stock..., Search Adobe Express Templates..., Version History
- Edit: Prompt to Edit..., Generative Fill..., Generate Image..., Reflection Removal..., Sky Replacement...
- Image: Generative Upscale...
- Layer: Harmonize, Layer Mask > Enhance edge
- Type: More from Adobe Fonts...
- Filter: Neural Filters..., Parametric Filters..., AI Denoise..., AI Sharpen...
- Window: AI Assistant, AI Assisted Editor, Adobe Stock, Beta Feedback, Comments, Libraries, Version History, Materials
- Plugins: Browse Plugins...
- Help: Photoshop Help..., Hands-on Tutorials..., What's New..., Learn more about generative credits, Adobe generative AI user guidelines

Kept: Select > Subject, Select > Sky, Plugins > Manage Plugins..., Help > System Info..., GPU Compatibility..., Manage My Account..., Updates.... One hidden row carries a shortcut, `Image > Generative Upscale...` at ⌥⇧⌘U, and no shortcut row and no pass step uses it. The proof is a re-read of the dialog, the presence of `Menu Customization.psp` and `Menu Customization Primary.psp`, and a System Events read showing the rows absent.

Every assistant, promotion, help, share, comment, learn, Home, and Discover surface, with the mechanism that removes it and what remains. Classes: `KEY` is one of the 43 typed preference keys, `AX` a Settings control, `MENU` an Edit > Menus visibility toggle, `PANEL` a Window or Plugins item closed in the workspace, `PLUGIN` an Adobe UXP extension under `Contents/Required/UXP/` or the shared UXP folder that neither the Plugins panel nor Creative Cloud lists as disableable, and `FIXED` a surface with no local control.

| [INDEX] | [SURFACE] | [CLASS] | [MECHANISM] | [RESIDUAL] |
| :-----: | :-------- | :------ | :---------- | :--------- |
|  [01]   | AI Assistant panel | PANEL, MENU | Window > AI Assistant closed and its menu row hidden | The app-bar AI Assistant icon: FIXED, `showAIAssistant` exists as a binary string with no Settings control |
|  [02]   | AI Assisted Editor mode | AX, MENU | Interface pane `Show AI Assisted Editor` off; the Window row hidden | none |
|  [03]   | Generative Fill, Generate Image, Prompt to Edit, Reflection Removal, Sky Replacement | MENU | The five Edit rows hidden | Contextual Task Bar buttons `Generative Fill` and `Generative Expand`, the Prompt to Edit field, and the Remove tool `Find distractions` menu: FIXED inside the bar the rules keep |
|  [04]   | Generative Expand in Crop, generative layer controls in Properties | FIXED | No preference; Properties shows generative controls on a generative layer alone, and our documents hold none | The Crop tool's contextual `Generative Expand` button |
|  [05]   | Properties Quick Actions, Remove Background and Select Subject | FIXED, on device | Image Processing pane: Select Subject and Remove Background `Device`, Selections, Remove, Enhance `Faster` | The buttons stay in Properties; processing never leaves the machine |
|  [06]   | Select > Subject, Select > Sky | Retained on-device | Kept: the TK9 Combo Select Subject and Select Sky buttons call them and honour the Select Subject processing choice | none |
|  [07]   | Generative Upscale, Harmonize, Enhance edge, Neural Filters, AI Denoise, AI Sharpen, Send to Firefly Boards, Search Adobe Stock, Search Adobe Express Templates | MENU | Each row hidden | Neural Filters and the Camera Raw Filter's own Denoise stay installed: PLUGIN, not disableable |
|  [08]   | Invite to Edit, Share for Review, share sheets | MENU, PANEL, FIXED | The two File rows hidden; `ccx.sharesheet/invite` and `/review` closed; `Share Panel` is PLUGIN | The Share icon in the app bar |
|  [09]   | Comments | PANEL, MENU, FIXED | Window > Comments closed and hidden; the commenting webview is PLUGIN | The Comments icon in the app bar |
|  [10]   | Notifications bell, in-app messaging, promotion badges | KEY | `notifications.quietMode` on; the in-app messaging and notification extensions stay PLUGIN | The bell icon |
|  [11]   | What's New | KEY, MENU | `notifications.showWhatsNew` off, written before the quiet write; Help > What's New... hidden | none |
|  [12]   | Feature onboarding coach marks | KEY | `notifications.showFeatureOnboarding` off, written before the quiet write | none |
|  [13]   | Rich tooltips | KEY | `notifications.useRichToolTips` on, written before the quiet write and kept as tool help | Locked while Quiet Mode is on; the held value is read back |
|  [14]   | Beta flask | PANEL, FIXED | Window > Beta Feedback closed and hidden; `Beta Feedback` is PLUGIN and the flask icon has no control | The flask icon in the app bar of the Beta build |
|  [15]   | Home screen | AX, FIXED | General pane `Auto show the Home Screen` off; `homeScreenVisibility false`; `Home Screen` is PLUGIN | The Home button at the left of the Options bar |
|  [16]   | Learn | MENU, FIXED | Help > Photoshop Help... and Hands-on Tutorials... hidden; the Home Learn tab is unreachable with Home off | The Search icon opens the Discover panel |
|  [17]   | Discover panel | FIXED | `Discover Panel` is PLUGIN, `DisableEmbeddedDiscoverPanel` is a binary string with no Settings control, and the Photoshop Utility Panel is closed | The Search icon in the app bar |
|  [18]   | Generative credits rows | MENU | Both Help rows hidden | none |
|  [19]   | Adobe Stock panel | PANEL, MENU | Window > Adobe Stock closed and hidden; the extension is PLUGIN | none visible |
|  [20]   | Substance 3D Materials | PANEL | Window > Materials and the three material-filter panels closed; the Substance viewer is PLUGIN | The Filter > Parametric Filters... row, hidden through Menus |
|  [21]   | Content Credentials | AX, PANEL | History pane: document options None, `Ask when opening` off, export None; Window > Content Credentials (Beta) closed | The Content Credentials extension stays PLUGIN |
|  [22]   | Camera Raw generative features | AX | The generative control in the Camera Raw Preferences dialog, off | The Camera Raw Denoise and Enhance models in `ModelZoo/` stay on disk |
|  [23]   | Suggestion strips inside panels | FIXED | The Layers generative bar appears for generative layers alone; the Adjustments presets are static factory groups | none in our documents |
|  [24]   | Creative Cloud Libraries | PANEL, AX | The panel closed and Plugins pane `Load Extension Panels` off, which stops the extension loading | none |
|  [25]   | Plugins marketplace | MENU, PANEL | Plugins > Browse Plugins... hidden and the Plugins Panel closed | Plugins > Manage Plugins... kept for the TK9 modules |
|  [26]   | Adobe Fonts promotion row | MENU | Type > More from Adobe Fonts... hidden | Font activation has no Photoshop preference |
|  [27]   | Cloud document history | MENU, PANEL | The File > Version History submenu and Window > Version History hidden, the panel closed | none |

## [19]-[COLOR_AND_MASKS]

| [INDEX] | [ITEM] | [VALUE] | [CHANNEL] | [PROOF] |
| :-----: | :----- | :------ | :-------- | :------ |
|  [01]   | Edit > Color Settings, `Default Color Settings` | Working Spaces RGB `Adobe RGB (1998)`, CMYK `GRACoL2013_CRPC6.icc`, Gray `Gray Gamma 2.2`, Spot `Dot Gain 20%`; Policies RGB, CMYK, Gray `Preserve Embedded Profiles`; Profile Mismatches Ask When Opening off and Ask When Pasting on; Missing Profiles Ask When Opening on; Conversion Engine `Adobe (ACE)`, Intent `Relative Colorimetric`, Black Point Compensation on, Dither on, Compensate for Scene-referred Profiles off; Advanced Desaturate Monitor Colors off, Blend RGB Colors Using Gamma off, Blend Text Colors Using Gamma 1.45. Every field is kept | none, already applied | descriptor `colorSettings`; the `.csf` hash row |
|  [02]   | View > Proof Setup > Custom…, `Default Proof` | Opened with no document open so the setup becomes the default for new documents; Device to Simulate `GRACoL2013_CRPC6.icc`; Preserve CMYK Numbers off; Rendering Intent `Relative Colorimetric`; Black Point Compensation on; Simulate Paper Color off; Simulate Black Ink off; `Save` in the default location so the preset appears in the menu | AX on the popups and checkboxes, a window-id screenshot for a control with no title | `Default Proof.psf` in `~/Library/Application Support/Adobe/Color/Proofing/`; a System Events read of View > Proof Setup lists it |
|  [03]   | Proof per document | `View > Proof Colors` (⌘Y) on while preparing a print row and off on digital rows; `View > Gamut Warning` (⇧⌘Y) toggled while correcting, with the gamut colour at gray 100 %; the document title carries the proof name while Proof Colors is on | AX on the View menu | The window title ends in the proof profile; the View menu `AXMenuItemMarkChar` |
|  [04]   | Print conversion | `Edit > Convert to Profile` to `GRACoL2013_CRPC6.icc`, Engine `Adobe (ACE)`, Intent `Relative Colorimetric`, Black Point Compensation on, Dither on, Flatten Image off | Recorded as an action of [22] | The action's step listing |
|  [05]   | Screen conversion | `Image > Mode > 8 Bits/Channel`, then `Edit > Convert to Profile` to `sRGB IEC61966-2.1` with the same engine, intent, compensation, and dither, no flatten | Recorded as an action of [22] | The action's step listing |
|  [06]   | Channels and Paths thumbnails | Medium → Small, and `Show Channels in Color` off | CU flyout; bridge | A screenshot of each panel's option dialog; `get_preferences interface` |
|  [07]   | Quick Mask Options | Color Indicates `Masked Areas`, colour `#FF00FF`, opacity 50 %, replacing Adobe's red at 50 % | CU on the dialog opened by double-clicking the Quick Mask chip | Reopening the dialog after relaunch shows `FF00FF` at 50 |
|  [08]   | Layer Mask Display Options | `#FF00FF` at 50 %, the `\` overlay while a mask is targeted, replacing red at 50 % | CU on the dialog opened by double-clicking a layer-mask channel | A screenshot of the reopened dialog |
|  [09]   | Alpha channel options | Color Indicates `Masked Areas`, colour `#FF00FF`, opacity 50 %; the dialog keeps these values for the next new channel | CU | A screenshot |
|  [10]   | Select and Mask workspace | View `Overlay`, Opacity 50 %, Color `#FF00FF`, Indicates `Masked Areas`; Show Edge off, Show Original off, High Quality Preview off; Refine Mode `Object Aware`; Edge Detection Radius 0 px with Smart Radius off; Global Refinements Smooth 0, Feather 0 px, Contrast 0 %, Shift Edge 0 %; Output `Decontaminate Colors` off, Output To `Layer Mask`; `Remember Settings` on | CU inside the workspace | Reopening the workspace after relaunch, a screenshot of the Properties column |
|  [11]   | Fill and adjustment layer masks | `Use Default Masks on Fill Layers` on; adjustment layers open with a mask | none | A Layers panel screenshot after a Curves adjustment shows a mask thumbnail |
|  [12]   | Gray working space with the TK9 modules | Color Settings Gray stays `Gray Gamma 2.2`; Multi-Mask `autoSetGraySpace true` switches the gray space to Monitor Gray while it computes luminosity masks and restores it | [24] | `PluginData/*.ini`; descriptor `workingGray` unchanged after a TK9 run |
|  [13]   | On-device selection and removal | Select Subject and Remove Background `Device`; Selections, Remove, Enhance `Faster`; the two Remove bundles of [16] rows 32, 33 | bridge, AX | descriptor `imageProcessingPrefs` |
|  [14]   | Apply Image and Calculations | Dialog defaults untouched; channel mathematics runs through the TK9 modules | none | none |

## [20]-[TYPE_AND_DOCUMENTS]

`Default Type Styles.psp` at 39,367 bytes is Adobe's seed: an `8BPS` document whose XMP records `Adobe Photoshop CS6 (13.1 x001)` and a 2012-08-13 create date, holding the paragraph style `Basic Paragraph` and the fonts `MyriadPro-Regular`, `MyriadPro-Bold`, and `TimesNewRomanPSMT`. No user style exists. The pass opens the master template, defines the paragraph and character styles Body, Heading, and Caption through the `apply_type_styles` body, and runs `Type > Save Default Type Styles`, which is enabled with a document open. A new document then shows the three styles, and `Type > Load Default Type Styles` into a scratch document lists them.

`<photoshop.prefsFolder>/New Doc Sizes.json` holds `{"sections":[{"section":"user","presets":[]}]}` at 104 bytes: zero user New Document presets. The pass writes one preset and one `.psdt` per row of `page-sizes.json`, which covers the print families, the technical sheets, the boards, `Digital 3840x2160`, and every named screen format. The `build_template` body reads the same file the page catalogue reads, so the two catalogues cannot drift.

| [INDEX] | [FIELD] | [VALUE] | [PROOF] |
| :-----: | :------ | :------ | :------ |
|  [01]   | Names and count | The catalogue's names, one preset and one file each; `Default Template.psdt` is a second copy of `Digital 3840x2160.psdt` | The `user` section's `presets` length equals the row count of `page-sizes.json`; `ls Sizes/` equals the row count plus one |
|  [02]   | Preset Details per row | Width and Height in the unit the row's name carries (inches on US, ANSI, ARCH, and board rows, millimetres on ISO rows, pixels on digital and screen rows); Orientation as the row; Artboards off; Color Mode `RGB Color` at `16 bit`; Resolution 300 Pixels/Inch on print, technical, and board rows and 72 on `Digital 3840x2160` and every screen row; Background Contents `White`; Color Profile `Adobe RGB (1998)`; Pixel Aspect Ratio `Square Pixels`; the preset name is the row name | Each `user` preset carries `name`, `width`, `height`, `units` (`inchesUnit`, `millimetersUnit`, `pixelsUnit`), `resolution` 300 or 72 with `resolutionUnits inchesUnit`, `mode "RGB"`, `depth 16`, `fill "white"`, `profile "Adobe RGB (1998)"`, `scale 1`, `guides []`, `artboards []`; orientation is width against height |
|  [03]   | `.psdt` content | One white `Background` layer, the guides of row 04, the three default type styles, no artboard; saved through `save` as `.psd` with the Maximize Compatibility preference, then renamed `.psdt` because typing the extension yields `.psdt.psd` | Opening the file gives `Untitled-1`, `saved false`, `layers.length 1`, the row's width, height, and `resolution`, `colorProfileName Adobe RGB (1998)`, `bitsPerChannel 16` |
|  [04]   | Guides carry the module | Per row from its `page-sizes.json` record: the two side margins, the top margin, the column edges with their one-module gutters (12 columns on document rows, 2 to 6 on screen rows), the row tops and row baselines, the last baseline, and the folio or footer line. Print rows convert points at 300 ppi (`px = pt × 300 / 72`) keeping fractional positions, since a guide `position` takes `_unit "pointsUnit"`; digital and screen rows carry the 45 px module with 135 px sides on the master and 45 px gutters. Technical sheets carry their sheet border, 24 pt on inch sheets and the ISO 5457 border on ISO sheets, plus the 108 pt module lines, and no column | A `batch_play` `get` of `{_ref: "document"}` with `_property "guides"` lists the positions, and their count equals the row's line count |
|  [05]   | Build | The `build_template` body runs once per row: `make` `document` with `{width, height, resolution, mode "RGBColorMode", depth 16, fill "white", profile "Adobe RGB (1998)", pixelScaleFactor 1}`, one `make` `guide` per line, `save` to `.artifacts/creative-cloud/photoshop/Sizes/<name>.psd`, then the rename. Every descriptor of a row rides one `batch_play` call with `continueOnError false` | `batch_play` answers an empty `failed`; the per-row readback of row 03 |
|  [06]   | Copies | `~/Documents/Adobe/Photoshop/Sizes/<name>.psdt`, `~/Documents/Adobe/Photoshop/Default Template.psdt`, and the Drive folder's `Sizes/` beside a copy of `New Doc Sizes.json` | The `exports.sha256` rows |
|  [07]   | CMYK preset | One CMYK preset, `Print CMYK Letter 8.5x11`: 8.5 × 11 in portrait, `CMYK Color` at `8 bit`, 300 ppi, White, Color Profile `GRACoL2013_CRPC6.icc`, Square Pixels. Every other print row stays RGB and is proofed through [19] rows 02 to 04 | The preset named `Print CMYK Letter 8.5x11` carries `mode "CMYK"`, `depth 8`, and that profile |
|  [08]   | Screen rows | The named web and mobile formats of the catalogue, each at 72 ppi on the 45 px module with the guides its catalogue row states | As rows 02 to 04 |

Each preset is saved from the New Document dialog: the Preset Details fields of row 02, then the save icon, the row's name, and `Save Preset`, which files it under the Saved tab. The dialog is a UXP surface, so the channel is CU, and the preset object's key set is read once after the first save.

## [21]-[TOOL_PRESETS]

The 21 factory tool presets are deleted together in `Edit > Presets > Preset Manager` with the type set to Tools: select all, `Delete`. Each replacement is created from the Tool Presets panel through `Create New Tool Preset` with its tool active and its Options bar set, `Include Color` off, and the set is saved through the Preset Manager `Save Set...` as `Default Tool Presets.tpl`. Every preset defines its own tip inside the preset, so none depends on a brush group in the Brushes panel.

| [INDEX] | [PRESET] | [TOOL] | [SETTINGS] |
| :-----: | :------- | :----- | :--------- |
|  [01]   | `Mask Soft 300` | Brush | Round tip, Size 300 px, Hardness 0 %, Spacing 25 %, Opacity 100 %, Flow 50 %, pen pressure on Opacity |
|  [02]   | `Mask Hard 60` | Brush | Round tip, Size 60 px, Hardness 100 %, Spacing 25 %, Opacity 100 %, Flow 100 % |
|  [03]   | `Eraser Soft 300` | Eraser | Round tip 300 px, Hardness 0 %, Mode Brush, Opacity 100 %, Flow 100 %, `Erase to History` off |
|  [04]   | `Eraser Hard 60` | Eraser | Round tip 60 px, Hardness 100 %, Mode Brush |
|  [05]   | `Clone Aligned 300` | Clone Stamp | Round tip 300 px, Hardness 0 %, Mode Normal, Opacity 100 %, Flow 100 %, `Aligned` on, Sample `Current & Below`, adjustment layers ignored |
|  [06]   | `Heal Sample Below 60` | Healing Brush | Round tip 60 px, Hardness 100 %, Mode Normal, Source `Sampled`, `Aligned` off, Sample `Current & Below`, Diffusion 5 |
|  [07]   | `Spot Heal Content-Aware 60` | Spot Healing Brush | Round tip 60 px, Mode Normal, Type `Content-Aware`, `Sample All Layers` on |
|  [08]   | `Gradient Foreground to Transparent` | Gradient | A two-stop gradient built in the Options bar Gradient Editor from foreground at 100 % to foreground at 0 %, stored inside the preset; Linear, Mode Normal, Opacity 100 %, `Reverse` off, `Dither` on, `Transparency` on, Method `Perceptual` |
|  [09]   | `Crop Digital 3840x2160 72` | Crop | `W x H x Resolution` 3840 px, 2160 px, 72 px/in; `Delete Cropped Pixels` off; `Content-Aware` off; Overlay Rule of Thirds |
|  [10]   | `Crop Letter 8.5x11 300` | Crop | 8.5 in, 11 in, 300 px/in, the same toggles |
|  [11]   | `Crop A4 210x297 300` | Crop | 210 mm, 297 mm, 300 px/in |
|  [12]   | `Crop A3 297x420 300` | Crop | 297 mm, 420 mm, 300 px/in |
|  [13]   | `Marquee 16:9` | Rectangular Marquee | Style `Fixed Ratio` 16 : 9, Feather 0 px, `Anti-alias` on |

The readback is `list_presets tool` returning these 13 names. The mask painting pair of rows 01 and 02 is the Brush tool with a black or white foreground, swapped by `X`.

## [22]-[ACTIONS]

The Actions panel holds 7 sets and 62 actions before the pass: `TK9 actions` with 26, then Adobe's seeded `Basic Adjustments` 5, `Subject & Background` 5, `Creative Effects` 6, `Guides` 8, `Resize` 6, and `Export` 6. The six Adobe sets are seeded into `Actions Palette.psp` by the application and ship as no `.atn` file in the bundle; the legacy `.atn` sets under `Presets/Actions/` are not loaded.

The pass leaves one set, `Default Actions`, holding 30 actions with no function key, no colour, every step's modal-control box off, and no `Stop`. Accepted action sets are binary `.atn` files that are played and never inspected: their steps are recorded as the vendor wrote them and the text export is the record of what they do. The sequence is `New Set...` named `Default Actions`; `Load Actions...` of the accepted overlay set and the drag of its one action into `Default Actions`; the drag of the 26 TK9 actions in their panel order; the three recordings below; deletion of the six Adobe sets, the emptied `TK9 actions` set, and the emptied overlay set; `Save Actions...` to `Presets/Actions/Default Actions.atn`; and ⌘⌥ `Save Actions...`, which writes the step listing as text to `.artifacts/creative-cloud/passes/photoshop/default-actions.txt`.

| [INDEX] | [ACTION] | [ORIGIN] | [RECORDED_STEPS] |
| :-----: | :------- | :------- | :--------------- |
|  [01]   | `Place Overlay` | The accepted overlay `.atn` | The steps the file records, unedited; the text export lists them |
|  [02]   | The 26 TK9 actions: `B and C Landscape`, `B and C Subject`, `B and C General`, `Zone Colour Grading`, `Tight Landscape`, `Tight Subject`, `Intersect Foreground`, `Intersect Sky`, `Intersect Subject`, `Intersect Background`, `Intersect Selection`, `Vignette No Darks`, `Mask The Mask`, `Modify Fill`, `Midtone Lighten`, `Midtone Contrast`, `Shadows Lighten`, `Colour Dodge Burn`, `Multiply Curve`, `Screen Curve`, `Lift Warm`, `Drop Cool`, `Drop Saturated`, `Paint Out Saturation`, `Lift Unsaturated`, `Close Panels` | The `TK9 actions` set, dragged in its panel order | The steps TK9 recorded, unedited; they drive the TK9 panels through Plugin Action steps |
|  [03]   | `Stamp Visible` | Recorded | `Select > All Layers`; `Layer > Duplicate Layers`; `Layer > Merge Layers`; `Layer > Rename Layer...` to `Stamp` |
|  [04]   | `Convert to sRGB 8-bit` | Recorded | `Image > Duplicate...` named `Web` with `Duplicate Merged Layers Only` on; `Image > Mode > 8 Bits/Channel`; `Edit > Convert to Profile...` to `sRGB IEC61966-2.1`, Engine `Adobe (ACE)`, Intent `Relative Colorimetric`, Black Point Compensation on, Dither on, Flatten Image off |
|  [05]   | `Convert to GRACoL` | Recorded | `Image > Duplicate...` named `Print` with `Duplicate Merged Layers Only` on; `Edit > Convert to Profile...` to `GRACoL2013_CRPC6.icc` with the same engine, intent, compensation, and dither, no flatten |

The readback is `executeActionGet` over `ASet` reporting one set named `Default Actions` and `Actn` reporting 30, with the text export matching this table.

## [23]-[CAMERA_RAW]

The installed plugin is `Camera Raw 18.6 (2698)` with the matching `Camera Raw Filter`. Three stores hold its state: `~/Library/Preferences/Adobe Camera Raw Prefs` at 4,340 bytes, a legacy four-character-code record file of 94 records that `plutil` rejects; `~/Library/Application Support/Adobe/CameraRaw/Defaults/Preferences.xmp` at 659 bytes; and `~/Library/Application Support/Adobe/CameraRaw/Defaults/RawDefaults.xmp` at 458 bytes. `CameraProfiles/`, `Curves/`, and `ImportedSettings/` are empty, so no user preset, profile, or curve exists, and the 550 MB `ModelZoo/CloudDownload/` is Adobe's model cache rather than user content.

| [INDEX] | [ITEM] | [CURRENT_TO_TARGET] | [CHANNEL] | [PROOF] |
| :-----: | :----- | :------------------ | :-------- | :------ |
|  [01]   | Save Image Settings In | Camera Raw Database (`DNGSidecarHandling 0`, `Settings/` holding two index files) → Sidecar ".XMP" Files | AX on the Camera Raw Preferences dialog | `Defaults/Preferences.xmp` re-read |
|  [02]   | DNG File Handling | `Ignore Sidecar ".XMP" Files` off → off | Same dialog | Same |
|  [03]   | JPEG, TIFF, HEIC, AVIF, JXL handling | Open if has settings → same | Same dialog | Same |
|  [04]   | Performance cache | 5.0 GB at the default path, the folder empty → 20 GB at the default path | Same dialog, Performance pane | `NegativeCacheMaximumSize 20.0` |
|  [05]   | GPU | Auto with the quick self-test passed → Auto | none | `Camera Raw GPU Config.txt` |
|  [06]   | Generative features | on → off | AX on the Camera Raw Preferences dialog; the pane and label come from [11] row 28 | A screenshot of the dialog |
|  [07]   | Workflow options | Display P3 (`ClrS DiP3`), 16 bit (`BtDp 16`), Smart Object off (`SmPI 0`) → `Adobe RGB (1998)`, 16 bit, native size, 300 ppi, Sharpen For none, `Open In Photoshop As Smart Objects` on | CU on the underlined text at the bottom of the Camera Raw dialog, which opens Space, Depth, Size, Resolution, Sharpen For, and the Smart Object checkbox | The decoded preference file reads `ClrS` as the Adobe RGB tag, `BtDp 16`, `SmPI 1` |
|  [08]   | Raw defaults | Adobe defaults, master only → same | none | `RawDefaults.xmp` `crs:Defaults="Adobe"`, `crs:MasterOnly="True"` |

The workflow values land raw files in the working RGB space of [19] row 01, so a raw file and a native document share one profile.

## [24]-[TK9]

Four TK9 modules hold configuration under `<photoshop.pluginData>/<id>/PluginData/` as flat one-value `.ini` files, 26 in total with no secret among them. `com.tk.myactionsV8` holds `language` alone and the four tab modules have no `PluginData` directory, so My Actions is unconfigured and stays closed.

| [INDEX] | [MODULE] | [CURRENT_TO_TARGET] | [CHANNEL] | [PROOF] |
| :-----: | :------- | :------------------ | :-------- | :------ |
|  [01]   | Multi-Mask | `language English`, `autoSetGraySpace true`, `autoShowProperties false`, `autoHideSelection false`, `showSelectionIndicator true`, `FXOverlayColor #ff00ff` → same | Module flyout | `com.tk.multimask/PluginData/*.ini` |
|  [02]   | Combo | `autoCloseTKActions true`, `colorOpacityValue 1`, `overlayColor #ff00ff`, `showSelectionIndicator true`, `showSmartObjectIndicator true`, `watermarkEdgeOffsetType PX`, `watermarkPosition CenterCenter`, `watermarkSmartObjectType Embedded`, `webSharpenFileType jpg10`, `language English` → same, with the web-sharpen save folder `~/Pictures/TK9 Web Sharpen` | Module flyout and the web-sharpen interface | `com.tk.comboV8/PluginData/*.ini` |
|  [03]   | Export | `radioOutputLocation SameFolder`, `radioChooseSource CurrentImage`, `radioCropType Centered`, `radioBarType ColorBars`, `radioLogoPosition CenterCenter`, `radioEdgeOffsetType PX`, `saveFileType jpg10`, `loadLastPreset false` → output folder `~/Pictures/TK9 Export`, `loadLastPreset true`, the rest unchanged | Module Save section | `com.tk.export/PluginData/radioOutputLocation.ini` |
|  [04]   | Backups | None → one folder per module under the Drive folder's `TK9 Backups/<module>/` | Each module's flyout `Backup user data`, which replaces the folder's contents | The folder listing |

The Multi-Mask gray-space switch and the Combo buttons honour the Select Subject processing choice, so the on-device setting of [19] row 13 governs them too.

## [25]-[EXPORT_PRESETS]

| [INDEX] | [SURFACE] | [FIELDS] | [CHANNEL] | [PROOF] |
| :-----: | :-------- | :------- | :-------- | :------ |
|  [01]   | Quick Export, in the Export pane | Format `PNG`, `Transparency` on, `Smaller File` off; Location `Ask where to export each time`; Metadata `None`; Color Space `Convert to sRGB` on; Export As Location `Export assets to the last location specified`; `Use legacy "Export As"` off | AX | descriptor `exportAssetsPrefs`: `exportFileType PNG`, `exportPNGTransparency true`, `exportAsLocationSetting` changed from 2, `exportMetaData 0`, `exportConvertToSRGB true`, `exportExportAsLegacy false`, `exportAssetsLocationSetting 3` |
|  [02]   | Export As dialog, ⌥⇧⌘W | File Settings Format `PNG`, `Transparency` on, `Smaller File` off; Image Size Scale `1x`, Resample `Bicubic Automatic`; Canvas Size equal to Image Size; Metadata `None`; Color Space `Convert to sRGB` on with `Embed Color Profile` on; Scale All one row at `1x` with no suffix. The dialog keeps its last values | CU on the UXP dialog with the template open, then `Cancel` | Reopening after relaunch, a screenshot of the right column |
|  [03]   | Image Processor, `File > Scripts > Image Processor...` | Select: `Use Open Images`, `Open first image to apply settings` off; Location `Save in Same Location`; File Type `Save as JPEG` at Quality 10 with `Resize to Fit` on at W 3840 and H 2160 and `Convert Profile to sRGB` on, `Save as PSD` off, `Save as TIFF` off; Preferences `Run Action` off, Copyright Info empty, `Include ICC Profile` on; `Save...` to `Default Image Processor.xml` | AX on the Cocoa script dialog and its save sheet | The XML under `.artifacts/creative-cloud/photoshop/` and on Drive; `Load...` in a fresh session repopulates every field |
|  [04]   | Export pipeline | Print: the `Convert to GRACoL` action, then `File > Save As` as TIFF. Screen: the `Convert to sRGB 8-bit` action, then row 02 | Actions | The action step listing |

## [26]-[PERSISTENCE]

Every artifact the pass produces, the file it writes, and the name it takes in the Drive folder `<drive.designLibrary>/05.Software Related Assets/99.Default Profiles/Photoshop/`, the one Drive write of the pass.

| [INDEX] | [ARTIFACT] | [CAPTURES] | [WHERE] | [DRIVE_NAME] |
| :-----: | :--------- | :--------- | :------ | :----------- |
|  [01]   | Workspace | The New Workspace dialog's three Capture boxes: Panel Locations, Keyboard Shortcuts, Menus. The toolbar slot list is not captured; the toolbar dock entry with its `size-variant` is | `WorkSpaces/Default Workspace.psw` created and `Workspace Prefs.psp` rewritten; edits after the save land in `WorkSpaces (Modified)/Default Workspace.psw` | `Default Workspace.psw` |
|  [02]   | Toolbar | Customize Toolbar `Save Preset...` | `<photoshop.supportFolder>/Presets/Custom Toolbars/Default Toolbar.<ext>`, plus `Toolbar Customization.psp` and `Toolbar Customization Primary.psp` in the settings folder | `Default Toolbar.<ext>`, the extension read at save |
|  [03]   | Keyboard shortcuts | The dialog's `Save Set`, which opens a name sheet when saving over Photoshop Defaults | `Presets/Keyboard Shortcuts/Default Keyboard Shortcuts.kys`, plus `Keyboard Shortcuts.psp` and `Keyboard Shortcuts Primary.psp` | `Default Keyboard Shortcuts.kys` |
|  [04]   | Menus | The dialog's `Save Set` | `Presets/Menu Customization/Default Menus.mnu`, plus `Menu Customization.psp` and `Menu Customization Primary.psp` | `Default Menus.mnu` |
|  [05]   | Type styles | `Type > Save Default Type Styles` | `<photoshop.prefsFolder>/Default Type Styles.psp` | `Default Type Styles.psp` |
|  [06]   | Tool presets | Preset Manager `Save Set...` | `Presets/Tools/Default Tool Presets.tpl` | `Default Tool Presets.tpl` |
|  [07]   | Actions | `Save Actions...` | `Presets/Actions/Default Actions.atn` | `Default Actions.atn` |
|  [08]   | New Document presets | One `Save Preset` per catalogue row | `<photoshop.prefsFolder>/New Doc Sizes.json`, the `user` section's `presets` | `Default New Doc Sizes.json` |
|  [09]   | Templates | The `build_template` body and the rename | `~/Documents/Adobe/Photoshop/Sizes/<name>.psdt` and `~/Documents/Adobe/Photoshop/Default Template.psdt` | `Sizes/<name>.psdt`, `Default Template.psdt` |
|  [10]   | Color settings | Already saved | `~/Library/Application Support/Adobe/Color/Settings/Default Color Settings.csf` | `Default Color Settings.csf` |
|  [11]   | Proof setup | View > Proof Setup > Custom… `Save` | `~/Library/Application Support/Adobe/Color/Proofing/Default Proof.psf` | `Default Proof.psf` |
|  [12]   | Camera Raw | Preferences and defaults | `~/Library/Preferences/Adobe Camera Raw Prefs` and `~/Library/Application Support/Adobe/CameraRaw/Defaults/*.xmp` | `Default Camera Raw Prefs`, `Camera Raw Defaults/` |
|  [13]   | TK9 | Module backups | The Drive folder's `TK9 Backups/<module>/` | `TK9 Backups/` |
|  [14]   | Image Processor settings | The dialog's `Save...` | `.artifacts/creative-cloud/photoshop/Default Image Processor.xml` | `Default Image Processor.xml` |
|  [15]   | Action step listing | ⌘⌥ `Save Actions...` | `.artifacts/creative-cloud/passes/photoshop/default-actions.txt` | none, an evidence file |
|  [16]   | Workspace list | `Window > Workspace > Delete Workspace...` | Delete `Editorial Images`, `Core Tools`, `Motion`, `Painting`, `Photography`, and `Graphic and Web`; `Essentials (Default)` stays; the Workspace pane's `Restore Default Workspaces` button and the submenu's reset item are never used | none |
|  [17]   | Hashes | `shasum -a 256` over every row above | `.artifacts/creative-cloud/exports.sha256` | none, the manifest of the copy |

The preference stores the pass rewrites on quit are `Adobe Photoshop (Beta) Prefs.psp`, `UIPrefs.psp`, `MachinePrefs.psp`, `Workspace Prefs.psp`, `Actions Palette.psp`, and `Default Type Styles.psp` under `<photoshop.prefsFolder>`. The resident preset stores `Brushes.psp`, `Swatches.psp`, `Gradients.psp`, `Styles.psp`, `Patterns.psp`, and `CustomShapes.psp` are rewritten unchanged, since the pass deletes no preset group.

## [27]-[PASS_ORDER]

| [INDEX] | [STEP] | [CHANNEL] | [EVIDENCE] |
| :-----: | :----- | :-------- | :--------- |
|  [01]   | Baseline: the settings folder listing, the `.psw` decodes, `New Doc Sizes.json`, `Adobe Camera Raw Prefs`, the TK9 `PluginData` files, the `PS.json` rows, the Window menu checkmarks, and the `AddOnModules/` and `Color/Proofing/` listings, plus the preference sub-descriptor read of [11] row 19 and the Settings pane dumps of [11] row 17 | FILE, AX, bridge | A `baseline/` folder under `.artifacts/creative-cloud/passes/photoshop/` |
|  [02]   | Install the plugin and relaunch | `nx run @rasm/photoshop-plugin:deploy` | The `PS.json` row reads `enabled` and `health` reports `attached` |
|  [03]   | Preferences with a class key, in the write order of [13] | bridge | The `applied` rows with their `from` and `to`; the `notifications` read after the quiet write; a second `showWhatsNew` write answers `PreferenceLocked` |
|  [04]   | Preferences reachable through ExtendScript alone: Font Preview Size | osascript | The readback string |
|  [05]   | Preferences with an accessibility control, pane by pane: Interface first (theme through CU on the first swatch, Highlight Color, Show Menu Colors), then Guides, Grid & Slices (`Gridline Every` 45 Pixels, Subdivisions 3), Plugins (`Load Extension Panels` off), Enhanced Controls, Technology Previews | AX, CU for the theme swatches | A re-read per pane; the descriptor for the theme and the grid |
|  [06]   | Edit > Menus hidden rows, then `Save Set` as `Default Menus` | AX | A dialog re-read |
|  [07]   | Edit > Keyboard Shortcuts, Tools category: read every Shortcut cell, apply the seven rows of [17], `Save Set` as `Default Keyboard Shortcuts`, `Summarize` | AX | The read list equals the `[KEY]` column of [16] before the changes; the `.kys` file exists; the Summarize Tools rows equal [17] |
|  [08]   | Quit and relaunch so the font size, the extension panels, and the Modern User Interface take effect | AX click on `Quit Photoshop`, then `open -b com.adobe.Photoshop` | `app.version`; `app.systemInformation` shows no `CC Libraries Panel` row |
|  [09]   | Proof setup `Default Proof` with no document open, then `Save` | AX, window-id screenshot | `Default Proof.psf` exists and View > Proof Setup lists it |
|  [10]   | Toolbar: Edit > Toolbar, the group drags and the one inner drag, the Show chips, `Done`, `Save Preset...` as `Default Toolbar`, then the `>>` chevron for two columns | AX for the buttons, CU for the drags | A screenshot; the file under `Custom Toolbars/` |
|  [11]   | Close every panel of [14] marked closed; drag TK9 Multi-Mask and Combo into pane B with Export minimized below; fill pane C with Properties and Adjustments; fill pane D with the three groups; make pane A iconic with its thirteen panels; remove the bottom dock | Window menu through AX, panel drags through CU | A screenshot per pane; the Window and Plugins checkmark set |
|  [12]   | Panel options per [15] | CU flyouts | descriptor `layerThumbnailSize small`; a screenshot |
|  [13]   | Camera Raw preferences and workflow options; the workflow link needs the Camera Raw Filter open on a scratch document | CU, AX | The decoded preference file; [11] rows 28, 29 |
|  [14]   | TK9 preferences per module and the module backups | CU on the module flyouts | The `.ini` files and the backup folders |
|  [15]   | Tool presets: delete the 21 factory presets in Preset Manager, create the 13 of [21], `Save Set...` as `Default Tool Presets.tpl` | CU on the panel and the Options bar, Preset Manager | `list_presets tool` returns the 13 names |
|  [16]   | Actions per [22] | Panel flyout through CU, recording through the menus | One set, 30 actions, and `default-actions.txt` matching [22] |
|  [17]   | Type styles: a scratch document from the `Digital 3840x2160` descriptor, the `apply_type_styles` body for Body, Heading, Caption, `Type > Save Default Type Styles`, close without saving | bridge, AX | `Type > Load Default Type Styles` into a second scratch document lists the three |
|  [18]   | Size catalogue: the `build_template` body once per row, the rename to `.psdt`, the copies to `~/Documents/Adobe/Photoshop/Sizes/` and `Default Template.psdt` | bridge, FILE | The per-row open readback of [20] row 03 |
|  [19]   | New Document presets: one `Save Preset` per catalogue row with the fields of [20] row 02, `Print CMYK Letter 8.5x11` last | CU in the New Document dialog | The `user` section's `presets` length; the key set read after the first save |
|  [20]   | Channels and masks on the master template: Quick Mask Options, Layer Mask Display Options, Channel Options, and the Select and Mask defaults with `Remember Settings` on | CU | Screenshots of the reopened dialogs |
|  [21]   | Export defaults: the Export As dialog state then `Cancel`, and the Image Processor fields then `Save...` | CU for the UXP dialog, AX for the script dialog | A reopened Export As screenshot; `Default Image Processor.xml` |
|  [22]   | Remove tool: the Options bar with Remove selected and one stroke on a scratch copy of the template, offline | CU, screenshot | The Options bar labels; the stroke completes; descriptor `imageProcessingRemoveToolProcessingPrefsStr` |
|  [23]   | Rulers on with the template open, since View > Rulers is disabled with no document; the Contextual Task Bar pinned through `Pin bar position` | AX, CU | The View menu checkmark; the pinned bar in a screenshot |
|  [24]   | `Window > Workspace > New Workspace...` named `Default Workspace` with all three Capture boxes on | AX | `WorkSpaces/Default Workspace.psw`; the `size-variant` token |
|  [25]   | Delete the other user and preset workspaces | AX on the Delete Workspace dialog | A System Events read of Window > Workspace |
|  [26]   | Quit; copy every artifact of [26] to the Drive folder; write the hashes | FILE | `exports.sha256` |
|  [27]   | Relaunch verification per [28] | AX, osascript, bridge, `screencapture` | The evidence files of [28] |

## [28]-[VERIFICATION]

Quit through the application menu item `Quit Photoshop`, then `open -b com.adobe.Photoshop`, and read every row. `app.panelList` and `app.workspaceList` answer `undefined` in ExtendScript on 27.11, so panel and workspace state is read through System Events.

| [INDEX] | [CHECK] | [CHANNEL] | [EXPECTED] |
| :-----: | :------ | :-------- | :--------- |
|  [01]   | Applied workspace | AX | `AXMenuItemMarkChar` of Window > Workspace > Default Workspace reads `✓`, and the submenu lists `Essentials (Default)`, `Default Workspace`, and the four commands alone |
|  [02]   | Open panels | AX | The Window and Plugins checkmark set equals the open panels of [14] |
|  [03]   | Layout | `screencapture -l <id>` with the window id from the window-list reader, read zoomed | Two toolbar columns, four panes, no closed panel visible |
|  [04]   | Extensions | osascript | `app.systemInformation` shows no `CC Libraries Panel` row, lists TK9 Multi-Mask, Combo, and Export as `Loaded`, and lists `Rasm Photoshop Bridge (Loaded)` |
|  [05]   | Preferences | bridge | `get_preferences` over the twelve classes equals [13] |
|  [06]   | Presets | bridge | `list_presets` per kind equals the counts of [06] row 08, and `list_presets tool` returns the 13 names of [21] |
|  [07]   | Actions | osascript | `executeActionGet` reports `ASet` 1 and `Actn` 30 |
|  [08]   | Shortcuts | AX | The Keyboard Shortcuts `Summarize` HTML Tools rows equal [17] |
|  [09]   | Templates | FILE, bridge | `ls Sizes/` equals the catalogue row count and one open per file gives the readback of [20] row 03 |
|  [10]   | Document presets | FILE | `New Doc Sizes.json` holds one `user` preset per catalogue row plus the CMYK row, and its hash is unchanged since the copy |
|  [11]   | Proof setup | AX | View > Proof Setup lists `Default Proof` |
|  [12]   | Workspace file | FILE | A `.psw` decode through `strings -n 6` reads `closed="true"` for every closed row of [14]; the decode yields the `is-closed`, `size-variant`, and `preferred-iconic-length` attributes and no panel ids |
|  [13]   | Rulers | AX | `AXMenuItemMarkChar` of View > Rulers reads `✓` with the template open |
|  [14]   | Type styles | bridge | `Default Type Styles.psp` hash is unchanged since the copy |
|  [15]   | Drive copy | FILE | Every row of `exports.sha256` equals the hash of its file on Drive |

## [29]-[SOURCES]

| [INDEX] | [SOURCE] | [DATE] |
| :-----: | :------- | :----- |
|  [01]   | Adobe UXP for Photoshop: manifest v4, manifest v5, and the Photoshop manifest page | 2026-09-11 |
|  [02]   | Adobe UXP for Photoshop reference: `executeAsModal`, `batchPlay`, `imaging`, `photoshopCore`, `photoshopAction`, Event Codes, Preferences, Photoshop class, changelog, Known Issues | 2026-09-11 |
|  [03]   | Adobe UXP API reference: WebSocket, EntryPoints, Host, Versions, Script | 2026-09-11 |
|  [04]   | Adobe UXP guides: How Do I…, Creating your first plugin, Packaging Your Plugin, Distribution Options, UXP Developer Tool, UXP Scripting | 2026-09-11 |
|  [05]   | Adobe developer blog, UXP changelog and support matrix | 2026-07 |
|  [06]   | Adobe UXP for Photoshop reference: ParagraphStyle and CharacterStyle class pages | 2026-09-14 |
|  [07]   | Adobe helpx: plugin installation errors in apps | 2026-07-03 |
|  [08]   | Adobe helpx: install plugins using the UPIA tool | 2025-09-02 |
|  [09]   | Adobe community: UPIA error codes, `-411 EXMAN_FAILED_NO_SUPPORTED_PRODUCT` | 2022-10-31 |
|  [10]   | Adobe developer forums: required permissions and arrays in manifest v5 (thread 6066), WebSocket in manifest v5 (6322), connecting to a WebSocket server (7347), sockets to external programs (2091), custom installers (4399), installing non-Marketplace plugins (6005) | 2023 to 2025 |
|  [11]   | npm registry and unpkg: `@adobe-uxp-types/photoshop`, `@adobe-uxp-types/uxp`, `@types/photoshop`, `@adobe/cc-ext-uxp-types`, `vite-uxp-plugin`, `bolt-uxp` | 2026-09-15 |
|  [12]   | Adobe helpx Photoshop: customize the toolbar, save custom workspaces, delete workspaces, workspace overview, home screen overview, change text size, Contextual Task Bar, technology preview features | 2026-02-23 to 2026-06-05 |
|  [13]   | Adobe helpx Photoshop: modify preferences (quiet mode, Image Processing Cloud and Device) | 2025-07-29 |
|  [14]   | Adobe helpx Photoshop: preference file functions, names, and locations | 2024-03-01 |
|  [15]   | Adobe helpx Photoshop: tools missing from the toolbar | 2024-10-17 |
|  [16]   | Adobe helpx Photoshop: use presets, manage pattern libraries, use the Color and Swatches panels, panels and menus, customize keyboard shortcuts | 2023-05-24 |
|  [17]   | Adobe helpx Photoshop: create documents, brush presets, create tool presets, play and manage actions, record an action | 2024-02-07 to 2026-02-23 |
|  [18]   | Adobe helpx Photoshop: color settings, proofing colors, create a temporary quick mask | 2023-05-24 |
|  [19]   | Adobe helpx Photoshop: export settings and export location preferences, quick export, convert files with the Image Processor | 2026-02-23 |
|  [20]   | Adobe helpx Camera Raw: manage Camera Raw settings; set up default settings for raw images | 2026-06-09; 2025-08-20 |
|  [21]   | Adobe helpx Photoshop: remove unwanted objects and distractions; Remove tool hardware requirements; what's new on desktop | 2026-01-29; 2026-08-28 |
|  [22]   | Adobe `PIStringTerminology.h`: the panel command names `brushesImport`, `brushesExport`, `brushesDelete`, `newBrushGroup`, and their swatch, gradient, pattern, style, and custom-shape counterparts | 2024-12-20 |
|  [23]   | Tony Kuyper, TK9 v4 instructions manual and release notes | 2026-05 to 2026-06 |
|  [24]   | Adobe community: `.psdt` save behaviour; an `.abr` import creates a group | 2021-03-02; 2018-01-09 |
|  [25]   | Machine reads: `plan/inputs/photoshop/{preferences,application-descriptor,preset-manager}.json`, `plan/inputs/ax/photoshop.json`, `<photoshop.prefsFolder>/New Doc Sizes.json` and `MRU New Doc Sizes.json`, the installed plugin manifests and `PluginsInfo/v1/PS.json`, the `AddOnModules/sensei_model_cache/inpainting_ai/` listings, `strings -n 6` over the Photoshop binary, the `.psw` decodes, and the TK9 `PluginData/*.ini` files | 2026-09-11 |
|  [26]   | Machine reads through `osascript`: `app.version`, `app.build`, `app.colorSettings`, `app.preferences`, `app.systemInformation`, `executeActionGet` over `ASet` and `Actn` | 2026-09-11 |
|  [27]   | Public repositories read for the listener rule: two Photoshop MCP servers and their UXP plugins; `hyperbrew/bolt-uxp` | 2026-09-11 |
