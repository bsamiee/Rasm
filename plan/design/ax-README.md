# [AX]-[ADOBE_APP_INSPECTORS]

Paths: `<Sources>` is `plan/research/sources/`,
the read-only source tree holding `ax/lib/` and one `ax/<app>/` per application; `<Inputs>` is
`plan/inputs/`, whose `ax/` folder holds the compiled
walker `ax-dump`, the runner `run.sh`, and one regenerated dump `<app>.json` per application.

Read-only accessibility inspectors for Illustrator, Photoshop, InDesign, Acrobat, Creative Cloud
and Typeface. Nothing is clicked but menu items, preference list rows, and Cancel.

Measured on Darwin 25.6 (macOS 26 Tahoe), Apple Silicon, 2026-09-11, with Accessibility and
Screen Recording both granted to the terminal.

In the repository the walker is `apps/creative-cloud/ax-dump/AxDump.xcodeproj`, a Swift
command-line-tool project with one target and one scheme named `AxDump`, built by the workspace's
Swift inference (`architecture.md` [02] row 14); `@rasm/inspection` owns the `inspect` and `verify`
targets (`inspect` depends on `AxDump:build`) and the xa11y roles beside it (`architecture.md [08]`). The sections below
describe the `<Sources>/ax/` scripts and the `<Inputs>/ax/` tools.

## [01]-[COMMANDS]

`<Inputs>/ax/` holds the compiled walker, its runner, and one dump per application; `<Sources>/ax/lib/ax-dump.swift` holds the walker's source, one flat Swift file, until `AxDump` (`architecture.md` [02] row 14) replaces the binary at G10. The applications differ in data, not in traversal.

| File | Role |
| :-- | :-- |
| `<Inputs>/ax/run.sh` | `run.sh <app>` for the six slugs `illustrator`, `photoshop`, `indesign`, `acrobat`, `creative-cloud`, `typeface`: resolves the pid through System Events, runs `ax-dump menus`, `tree`, and `windowlist`, and writes `<Inputs>/ax/<app>.json`. The walker beside it answers no `windowlist` (its usage line reads `ax-dump <menus\|tree\|dialog\|unlock> <pid>`, read 2026-09-15), so a rerun today fails at that third call and writes nothing; the dumps on disk, hashed by `<Inputs>/manifest.sha256`, were written by an earlier build and carry the keys `status` (`ok`, `process`, `pid`, `cpuPercentAtStart`, `capturedAt`), `menus` (`command`, `maxDepth`, `menuBar`, `nodes`, `ok`, `pid`), `tree` (`command`, `maxDepth`, `nodes`, `ok`, `pid`, `windowCount`, `windows`), `windowlist` (`command`, `ok`, `pid`, `windows[]` of `{bounds, layer, name, windowId}`; `jq` over `illustrator.json`); a missing process writes `{ok: false, process, error: "process not running"}`. A pass regenerates the same three keys through `AxDump menus`, `tree`, `windowlist` (G10, `architecture.md` [02] row 14), so every `run.sh <app>` step of a pass is that call |
| `<Inputs>/ax/ax-dump` | The compiled walker (arm64) beside `run.sh`: `menus`, `tree`, `dialog`, `unlock` subcommands, JSON on stdout |
| `.artifacts/creative-cloud/inspection/<app>/` | Every on-demand capture: `ax-dump dialog <pid>` output, `screencapture -x -l <windowId>` window and dialog PNGs, `ax-dump unlock <pid>` output, and `layout.json` (the `tree` key derived by `@rasm/inspection` `layout.ts` under the rules of [05]; `architecture.md` [08]) |
| Menu clicks, dialog rows, dialog close | System Events through AppleScript: `click menu item`, the outline or table row of a preferences dialog, `Cancel`/`Close`/`Done` else `⌘.` after `set frontmost`; never OK, Apply, or Save |

```sh
<Inputs>/ax/ax-dump menus       <pid>   # menu bar tree with shortcuts and check marks
<Inputs>/ax/ax-dump tree        <pid>   # every window's element tree
<Inputs>/ax/ax-dump dialog      <pid>   # sheets and modal windows only
<Inputs>/ax/ax-dump unlock      <pid>   # AXManualAccessibility / AXEnhancedUserInterface probe
```

`AxDump windowlist <pid>` (G10) is the window-server read that works when AX does not: one row per window with `bounds`, `layer`, `name`, and the `windowId` that `screencapture -l` takes, the shape of the on-disk `windowlist` key.

A pass writes its own dumps under `.artifacts/creative-cloud/passes/<app>/` and nothing under `<Sources>` or `<Inputs>`.

`ax-dump` calls `AXUIElementSetMessagingTimeout(app, 5)` before its first read; `AX_TIMEOUT` replaces the 5 s. Every node carries `path` (the index chain from the root), `depth`, and the attributes of one `AXUIElementCopyMultipleAttributeValues` call: role, subrole, title, description, value, help, enabled, focused, selected, role description, identifier, `AXFrame`, and the four `AXMenuItem*` shortcut and mark attributes; `tree` and `dialog` nodes add `actions`. A failed read is an `errors` row on the node with the `AXError` code, never a retry and never a guess.

Outputs: `<Inputs>/ax/<app>.json` (`status`, `menus`, `tree`, `windowlist`), written by `<Inputs>/ax/run.sh <app>`, is the dump the design files cite. A dump the application did not answer is written as `{"ok": false, "command": …, "error": …}`, never left empty. `@rasm/inspection` `layout.ts` derives `.artifacts/creative-cloud/inspection/<app>/layout.json` from the `tree` key ([05]). Dialog captures are produced on demand into `.artifacts/creative-cloud/inspection/<app>/`: open the dialog by the AX drive channel, diff the window list, `ax-dump dialog <pid>` for the AX-visible ones, `screencapture -x -l <windowId>` as `dialog-<label>.png` for every one, close by `Cancel` else `⌘.`, and `ax-unlock.json` from `ax-dump unlock`. Every measurement below is carried by the numbers in this file and by `<Inputs>/ax/<app>.json`.

## [02]-[TOOLING_VERDICTS]

### [02.1]-[READ_REACH]

System Events reaches every attribute raw `AXUIElement` reaches. Its generic accessor returns
them by name: `value of attribute "AXFrame" of window 1` gives `{0, 39, 1800, 1121}`, and
`AXChildrenInNavigationOrder` returns element references. The two names that error through it,
`AXTopLevelUIElement` and `AXEnhancedUserInterface`, fail on text coercion and on being
application-level rather than window-level attributes, not on access.

JXA calls the AX C API once the signature is declared. `ObjC.import('ApplicationServices')`
relies on bridgesupport metadata that does not annotate the `CFTypeRef *` out-parameters, so
`Ref()` unwrapping yields `{}` and `ObjC.castRefToObject` kills the interpreter. Importing
`Cocoa` and binding the function works:

```js
ObjC.import('Cocoa');
ObjC.bindFunction('AXUIElementCopyAttributeNames', ['int', ['id', 'id*']]);
// -> err 0, the full 19-attribute list
```

The repository uses no JXA (`architecture.md [08]`).

### [02.2]-[TIMINGS]

Full Illustrator menu bar, cross-validated: the Swift walker counted 4036 nodes, System Events'
`entire contents` returned 4037 including the root.

| Method | Illustrator menu bar | Photoshop menu bar | Returns |
| :-- | --: | --: | :-- |
| System Events `entire contents` | 5.39 s | 0.76 s | bare references, no attributes |
| Swift, one call per attribute | 4.15 s | 0.60 s | 5 attributes per node |
| Swift, `AXUIElementCopyMultipleAttributeValues` | 3.00 s | — | 5 attributes per node |

Swift is faster by a factor under two and returns the attributes in the same pass. Where System
Events batches with a plural form it is faster still: `{role, description, position, size} of
every UI element of window 1` returned all 346 Illustrator nodes in 0.74 s, because `every`
collapses into one Apple event.

The inspectors are built on Swift for output shape, not speed. AppleScript returns nested
structures as flat comma-mashed lists, `AXFrame` coerced to text arrives as `03918001121`, and the
deliverable is JSON trees with per-node paths. Swift emits that directly, decodes `AXValue`
structs through `AXValueGetValue`, batches attributes in one IPC, registers `AXObserver`
notifications, and is the only one of the three that can attempt the `AXEnhancedUserInterface`
write. System Events does the clicks: a menu item and a list row, where the AppleScript form is
shorter and equivalent.

`xa11y` adds selectors and a driver loop over the same `AXUIElement` calls; the raw tree dump the
measurements needed is about 200 lines of Swift with no dependency, so `<Sources>/ax/` does not
use it. In the repository xa11y owns window-state reads, control clicks, and menu item clicks
beside the Swift walker (`architecture.md [08]`).

### [02.3]-[LIBRARY_SURVEY]

Star counts and dates fetched 2026-09-11.

| Tool | Stars | Last commit | Verdict |
| :-- | --: | :-- | :-- |
| [xa11y](https://github.com/xa11y/xa11y) | 71 | 2026-09-11 | Adopt for the monorepo. Rust core, MIT, created 2026-03-15, a Playwright-style driver over `AXUIElement` with CSS-like selectors; ships PyPI `xa11y` (Python ≥ 3.9) and npm `@crowecawcaw/xa11y` (Node 18+) at the same version (0.14.0, 2026-09-09) plus `pytest-xa11y`; no code signing needed; the one candidate with an answer to AX in CI. Risks: 6 months old, 15 npm versions in 5 months, npm scope does not match the org |
| [pyax](https://github.com/eeejay/pyax) | 15 | 2026-02-28 | Adopt as a devtool. MIT, v0.3.4; `pyax tree <app> --json`, `inspect`, `observe`. The only Python tool exposing `AXObserver` notifications, the one capability System Events lacks |
| pyobjc-framework-ApplicationServices | — | 12.2.2, 2026-08-11 | The floor. Python 3.10–3.15, `arm64-26.5` metadata. Not installed in this repo's `.venv`, and the system `python3` (3.15.0rc2) has no `Quartz`/`AppKit`, so the Python AX path costs an install while Swift and osascript cost nothing |
| [Peekaboo](https://github.com/openclaw/Peekaboo) | 5,144 | 2026-09-08 | Turnkey CLI/MCP, moved from `steipete/`, v4.3.3, signed, Homebrew; `peekaboo see --app <name> --json` emits an AX-derived UI map and `peekaboo action` performs any AX action. A product with a daemon, not a library |
| [atomacos](https://github.com/daveenguyen/atomacos) | 49 | 2021-05-24 | Do not adopt, archived. Classifiers stop at Python 3.7. `pyatom/atomacos` and `pyatom/ATOMac` are 404 |
| [AXSwift](https://github.com/tmandry/AXSwift) | 415 | 2021-11-14 | Dead, as are all forks (`ax-kit` 3★, `axkit` 2★, `AXSwiftExt` 3★) |
| [nut.js](https://github.com/nut-tree/nut.js) | 2,850 | 2024-05-01 | Dead, unpublished from npm, and pixel-based with no AX |
| Hammerspoon `hs.axuielement` | 16,085 | 2026-07-08 | Excellent API, but a GUI app driven over `hs.ipc`, the wrong shape for a monorepo target |

Claude Code skills for macOS AX: none worth adopting. Every hit is 0–2 stars and personal.

### [02.4]-[MACOS_26_GRANT]

xa11y's source states that on macOS 26+ the Accessibility grant alone no longer suffices:
without Screen Recording, "AXChildren returns only self-referencing AXApplication wrappers and
menu bars." This machine is macOS 26 with Screen Recording granted. Illustrator returned 346 real
window children and Creative Cloud 301 through the same terminal, so the grant works, and
Photoshop's 13 nodes and Acrobat's 6 are the applications being empty, not a missing TCC grant.
The grant is the first thing a reader suspects, so the control is stated here.

### [02.5]-[WRITE_PATH]

An AX action's return code proves nothing. mediar-ai deleted 4,368 lines of Rust over `AXPress`
returning success while doing nothing in eight browsers; Peekaboo reports SwiftUI tab presses as
indeterminate rather than retrying. The inspectors click only menu items and list rows, and
`inspect-app.sh` proves a dialog opened by a window-list diff ([03.4]), never by the click's
return value.

## [03]-[WHAT_ADOBE_EXPOSES]

### [03.1]-[SURFACE_RULE]

A native Cocoa surface is fully accessible. A surface Adobe draws itself is invisible.

Adobe's current UI framework is Drover, described in an Adobe job posting (2026-07-30) as "the
framework behind many of Adobe's flagship desktop applications… from Photoshop to Illustrator to
Premiere Pro to After Effects", a "high-performance native framework" whose listed problem space
includes "compositing and rendering": it paints its own widgets. Photoshop was migrated onto it
in 27.9, rolling out from 2026-07-26 (Adobe developer blog, 2026-06: "We're updating the core UI
layer that Photoshop runs on… an infrastructure change"). Its predecessor OWL is the CS-era
toolkit, confirmed by an InDesign CS6 crash log loading `com.adobe.owl`, `AdobeOwlCanvas`, and
`WidgetBinLib.dylib`. OWL's Windows DLL imports `OLEACC.dll` (MSAA), so OWL had a Windows
accessibility bridge; no public evidence shows an NSAccessibility equivalent for either
framework, and none of Adobe's conformance reports mention VoiceOver.

The split is sharp enough to be a working rule:

| Surface | Framework | AX result |
| :-- | :-- | :-- |
| Menu bar | Cocoa `NSMenu` | Full, every app, including shortcuts and check marks |
| Photoshop / Acrobat Preferences | Cocoa dialog | Full: 208 and 25 labelled controls |
| InDesign Preferences, Color Settings | Drover | Invisible: not in the AX window list at all |
| Document window, panels, toolbars | Drover | Empty everywhere except Illustrator |
| Creative Cloud | WebKit | Full, depth 19–27 |

### [03.2]-[PER_APP]

| App | Menu nodes | Window nodes | Depth | Dialogs AX-visible |
| :-- | --: | --: | --: | :-- |
| Illustrator 2026 Beta | 4036 † | 346 † | 3 | not probed ([06]) |
| InDesign 2026 Beta | 3670 | 47 | 3 | none of 4 |
| Photoshop 2026 Beta | 1151 | 13 | 3 | all 6 |
| Acrobat Pro | 382 | 6 | 3 | 1 of 1, 38 panes |
| Creative Cloud | 191 | 159–301 | 19–27 | n/a |
| Typeface-beta | — | — | — | not running |

† Measured with Illustrator idle. A dump taken while the application runs at
100 % CPU returns `-25204` on every node ([06]), so a `tree` or `menus` dump holding `-25204` is a
load reading, not a count; `<Inputs>/ax/illustrator.json` `status` records `cpuPercentAtStart`
`100.0`, so its counts are load readings.

The menu walk decodes `AXMenuItemCmdModifiers` and `AXMenuItemCmdChar` into `⌘⇧⌥⌃` text and
reads `AXMenuItemMarkChar`: 130 shortcuts and 27 check marks in Illustrator (menu depth 11), 106
and 21 in Photoshop (depth 7), 58 and 4 in Acrobat (depth 7), with no menu opened on screen.

Adobe's conformance reports predict these counts. All four rate 4.1.2 Name/Role/Value as Does
Not Support:

| App | Report | Claim | Measured |
| :-- | :-- | :-- | :-- |
| Photoshop | macOS ACR, Oct 2023, v21.1.2 | "User interface components in most product functions do not provide programmatic information"; menus alone are claimed: "A clear hierarchy is represented in menus; however, most information and relationships… cannot be programmatically determined" | 1151 menu nodes, 13 window nodes |
| Illustrator | joint Win/Mac ACR, Apr 2024 | "most, if not all" components unroled, yet it enumerates individual panels (Layers, Properties, Artboards, Status Bar, Discover Panel) and per-control defects such as disabled state not conveyed: reachable but unlabelled elements | 346 nodes, many with a usable `AXDescription` and an empty `AXTitle` |
| InDesign | v19.0.1, Nov 2023 | "Combo boxes convey all possible values instead of the selected value; Checkboxes don't convey their label, role or state" | 47 window nodes idle, `-25204` at the application element under load ([06]) |
| Acrobat Pro | Apr 2025 | 4.1.2 Does Not Support; `Prepare for Accessibility` and `Make accessible` are listed among functions that "cannot be operated through a keyboard interface" | 6 window nodes |

Acrobat's chrome is not accessible; its reputation concerns the PDFs it produces.

### [03.3]-[PER_SURFACE]

- Photoshop's Layers panel rows are not AX children. Its entire window is 13 nodes: window
  chrome, plus a handful of real `NSControl`s that leak through: `AXPopUpButton` values `Layer`,
  `Kind` and `Normal` (blend mode), two `AXTextField`s (opacity and fill) and two scrollbars.
  There is no toolbar, no Options bar, and no layer row. A region capture is the reading.
- Photoshop's Preferences dialog is fully accessible: 208 labelled controls with values, for
  example `AXCheckBox "Export Clipboard" = 1`, `AXPopUpButton = "Bicubic Automatic"`. Each pane
  is its own menu item under `Photoshop (Beta) ▸ Settings…`, so there is no left list to walk.
- InDesign's dialogs open but are invisible to AX. `Edit ▸ Color Settings...` produced a real
  612×731 window at the window server that never entered the AX window list. Its values are
  readable by window-id capture alone. Its main window exposes 47
  nodes: chrome plus the top app bar (`Home`, `Share`, `AI Assistant (beta)`, `What's new in
  Beta`, `Provide feedback`) and a search box whose descriptions leak Drover's internal widget
  names: `UI_SearchBoxBase`, `UI_SearchBoxBasePopupEdit`, `UI_PictureButton`.
- Acrobat's main window is six nodes, three traffic lights and an empty group, while its
  Preferences dialog is a proper `AXOutline` of 38 panes, each selectable by index, yielding
  labelled checkboxes with values. Panes include `Accessibility`, `Color Management`,
  `Generative AI`, `Security (Enhanced)`, `JavaScript`, `Trust Manager`.
- Illustrator is the outlier and the only app whose panels are readable ([05]).
- Creative Cloud is the best citizen: `AXWebArea`, `AXLandmarkNavigation`, `AXHeading`,
  `AXTabButton`, depth 19–27. Node count varied 301 → 159 between runs as its content reloaded,
  so any single capture is a snapshot.

### [03.4]-[DIALOG_MECHANICS]

Every dialog menu path resolves in the menu dump, so `APP_DIALOGS` names the path and no run-time
discovery exists. Illustrator's `Settings…` is a single item; Photoshop's is a submenu with one
item per pane; Acrobat uses the older `Preferences...` wording; InDesign's `Preferences` submenu
carries no ellipsis.

| App | Settings | Color settings | Shortcuts | Toolbar | Workspace |
| :-- | :-- | :-- | :-- | :-- | :-- |
| Illustrator | `Illustrator (Beta) > Settings…` | `Edit > Color Settings...` | `Edit > Keyboard Shortcuts...` | `Window > Toolbars` (`Default Toolbar [✓]`, `Manage Toolbars...`) | `Window > Workspace` (`Default Workspace [✓]`, `Manage Workspaces...`) |
| Photoshop | `Photoshop (Beta) > Settings…` submenu (`General...`, `Interface...`, `Tools...`, `Workspace...`, …) | `Edit > Color Settings...`, `Edit > OpenColorIO Settings...` | `Edit > Keyboard Shortcuts...` | `Edit > Toolbar...` | `Window > Workspace` (no mark set) |
| InDesign | `InDesign (Beta) > Preferences` submenu (`General...`, `Interface...`, …) | `Edit > Color Settings...` | `Edit > Keyboard Shortcuts...` | — | — |
| Acrobat | `Acrobat > Preferences...` | — | — | `View > Show/Hide > Customize Toolbar` | — |

Escape does not close a Drover dialog; Command-period does. InDesign's Color Settings survived
two Escapes with the app verified frontmost, then closed on the first `⌘.`. Such a dialog exposes
no AX Cancel button, so `⌘.` is the only programmatic dismissal. `dialog-close.applescript`
clicks `Cancel`, `Close`, or `Done` when AX exposes one, else sets the process frontmost, polls
up to twenty times at 0.25 s until it is, and sends `⌘.`.

A new dialog is detected at the window server, not through AX. `menu-click.applescript` only
clicks; `inspect-app.sh` diffs `ax-dump windowlist` before and after, polling ten times at 0.4 s
for a fresh `windowId`, and records `axVisible` from a System Events window count by name. This
is what makes InDesign's dialogs recordable at all, and it is the proof of opening that a click's
return code cannot give ([02.5]). Capture uses `screencapture -l <windowId>`, not `-R <rect>`:
the apps sit behind the terminal, and a rect capture returns whatever is on top.

## [04]-[THE_UNLOCK_ATTEMPT]

No published test of these flags against an Adobe application exists: every published test is
Electron, Chromium, or Flutter. The published best practice in agent tooling (trycua/cua
PR #1756, merged 2026-05-31) sets `AXManualAccessibility` first, falls back to
`AXEnhancedUserInterface` on `kAXErrorAttributeUnsupported`, and pumps the run loop about 500 ms
before re-walking; "native Cocoa apps reject the attribute and pay no cost." Flutter shows that
arming is not populating: the flag flips 0 → 1 yet "FlutterView content group stays childless in
every case." `ax-dump unlock` follows that order, pumps the run loop 0.8 s, re-counts, and
restores the prior value in the same process; its JSON (`ax-unlock.json` under
`.artifacts/creative-cloud/inspection/<app>/` when `inspect-app.sh` runs it) records `nodesBefore`, each flag's
`priorValue`, `setError`, `nodesAfter`, and `restoreError`, `appliedFlag`, `nodesAfterRestore`,
and `unlocked`. Result, all five apps:

| App | `AXManualAccessibility` | `AXEnhancedUserInterface` | Nodes before → after |
| :-- | :-- | :-- | :-- |
| Photoshop | `-25205` unsupported | `-25208` not implemented | 13 → 13 |
| InDesign | `-25205` | `-25208` | 47 → 47 |
| Acrobat | `-25205` | `-25208` | 6 → 6 |
| Creative Cloud | `-25205` | `-25208` | 159 → 159 |
| Illustrator | `-25204` cannot complete (app under load) | `-25204` | — |

`AXManualAccessibility` is Electron-specific and correctly rejected. `AXEnhancedUserInterface`
is readable (it returns `false`) but `kAXErrorNotImplemented` on write: Adobe advertises the
attribute and implements no setter. No node count moved, no write persisted, and every app was
left as found. The unlock does not work on Adobe apps: there is no hidden tree waiting to be
switched on.

## [05]-[DERIVED_LAYOUT]

`<Sources>/ax/lib/derive-layout.py` derives `.artifacts/creative-cloud/inspection/<app>/layout.json`
from the `tree` key of `<Inputs>/ax/<app>.json`. Illustrator's tree
is flat: every panel tab, panel control and toolbar button is a direct child of `AXWindow` at
depth 1 carrying an absolute `AXFrame`, with no dock → column → group → panel containment, so
the layout is recovered geometrically, not structurally. Columns are clusters of x separated by
more than `COLUMN_GAP` (60 px); a tab row is chips whose y differ by at most `ROW_TOLERANCE`
(6 px); content is attributed to the nearest visible tab row above it in the same column, and a
panel's height runs from its tab row to the next tab row or the column end. The toolbar is the
button cluster at x < 60 ordered by (y, x), each button with its description, the shortcut parsed
from the trailing parenthesis, and its selected state from the value. The watchlist matches
descriptions against `AI`, `Assistant`, `Generative`, `Discover`, `Learn`, `Comments`, `Share`,
`Home`, `Firefly`, `Whats`, `WhatsNew`.

Three traps are encoded as rules rather than left to a nearest-match guess:

1. An `AXTabGroup` is a single tab chip, not a container. Chips sharing a y form one tab row,
   which is one panel group.
2. Collapsed or scrolled-out groups report negative y or zero size. Observed in Illustrator:
   `Align [1226,-752]`, `Pathfinder [1273,-752]`, `Properties [1226,-70]`,
   `Appearance [1293,-70]`, `Stroke [1225,-71 0x0]`, `Transparency [1225,-71 0x0]`,
   `Swatches [1479,-650]`, `Color [1541,-650]`, `Gradient [1584,-650]`. These go to
   `offscreenTabs`, never plotted as visible.
3. Frames can be garbage. One Photoshop popup reported `[11459,10779 83x16]` where sibling
   geometry implies `~[1459,779]`. Anything outside the window bounds plus a 200 px margin goes
   to `rejected` with its `path`, role, and reason.

Two further rules:

- The window's own close, minimise, and zoom buttons sit in the toolbar's x band, so they are
  excluded by subrole, not by a y threshold. Without the subrole rule Creative Cloud's three
  traffic lights read as a three-tool toolbar.
- A window with fewer than three tab chips is not an Adobe panel dock. `dockModelApplies: false`
  says so instead of reporting invented columns.

Anything that cannot be attributed to a panel lands in `unattributed`. Nothing is assigned to a
nearest guess.

### [05.1]-[PER_APP]

Illustrator is the only real dock. With the app idle: two dock columns at `x≈1226` and
`x≈1479`, a toolbar at `x≈6`/`x≈36` whose buttons carry the shortcut in the description and
selection in the value (`AXButton [6,300 30x24] desc="Blend Tool (W)" val="Selected"`,
`AXButton [36,300 30x24] desc="Mesh Tool (U)" val="Not Selected"`), visible tab rows for
`Artboards`/`Links` at `y≈771` (`x` 1226 and 1292), `Actions`/`History` at `y≈16` (`x` 1479 and
1532) and `Layers` at `[1479,755]`, and panel content such as the Align panel's buttons at
`y≈112–248`. The AI and discovery surface is all present as button descriptions: `AI Assistant`,
`DiscoverBtn`, `Home`, `Share Document`, `Provide feedback`, `New Notifications`,
`This is WhatsNew button`, `Explore starter tutorials in Illustrator`,
`Check your generative credits usage.`, `Switch Workspace`.

Photoshop, InDesign, Acrobat: `dockModelApplies: false`. There are no tab chips because there
are no panels in the tree. The `layout.json` `derive-layout.py` writes under
`.artifacts/creative-cloud/inspection/<app>/` records the window frame, the few real controls, and
the watchlist; the rest is empty.

Creative Cloud: `dockModelApplies: false` (two tab chips). Watchlist hits include `Home`,
`AI Assistant`, `Adobe Firefly`, `Do more with AI Assistant (Beta)`, `Retouch portraits`.

Panel flyout menus: no app exposes an `AXMenuButton` anywhere. Every `layout.json` `derive-layout.py`
writes under `.artifacts/creative-cloud/inspection/<app>/` carries
`"flyoutNote": "no AXMenuButton exposed: panel flyout menus require computer-use"`, the absence
proven by the dumped nodes rather than asserted. This matches Adobe's own guidance: asked how to
run a Swatches panel-menu command, Adobe's community answer was to record an Action and call
`app.doScript(action, set)`.

## [06]-[PROBLEMS_SEEN]

An Adobe app under load stops servicing AX; it does not answer slowly. Illustrator pinned at
88–100 % CPU by a second process driving it returned `-25204` (`kAXErrorCannotComplete`) on every
call, and `AX_TIMEOUT=30` changed nothing. A relaunch under the same load answered the same way,
so the state follows load, not process age. The published complaint that
Photoshop's Export As dialog "does still respond… but very, very slowly" describes a slower
answer; the measured state is no answer. The `status` key of `<Inputs>/ax/<app>.json` records
`cpuPercentAtStart` beside each run so a `-25204` reading stays interpretable.

`-25204` from an Adobe app means busy, not unsupported. InDesign returned `-25204` at its
application element under load (System Events: `count of UI elements` = 0, `count of menu bars` =
0, `menu bar 1` raises "Invalid index", with the process alive and its windows on screen) and 47
window nodes and 3670 menu nodes idle; Illustrator returned 346 window nodes and 4036 menu nodes
idle and `-25204` under load. A dump holding `-25204` is rerun with the app idle before anything
is concluded from it: `<Inputs>/ax/ax-dump` into `.artifacts/creative-cloud/`.

A dialog opened on an app under load is a trap: the menu click lands, AX reads nothing, and the
dialog closes only through `⌘.` after the process is confirmed frontmost, which a loaded Adobe
app takes seconds to become. The close step therefore confirms frontmost before sending `⌘.`.
The Illustrator Color Settings capture of 19:30 is such a capture. The Illustrator dialog probe
has no idle run: its one recorded outcome is `opened: false` against a loaded app, so the
Illustrator pass captures its dialogs itself when the app is idle (idle by
`pgrep` plus CPU).

Node counts drift while another process drives the application: Illustrator 346 → 349, Photoshop
12 → 13, Acrobat 5 → 6 (a document opened mid-run), Creative Cloud 301 → 159 (content reload).
Every count here is a snapshot with about ±3 nodes of noise, and the Creative Cloud figures are
reload-dependent.

### [06.1]-[SCRIPT_TRAPS]

- `container` and `target` are System Events property names. An AppleScript variable named
  either inside `tell process` makes `set` parse as a property assignment and fail with "Can't
  set container of process…"; `menu-click.applescript` names its variable `theMenu`.
- An AppleScript list literal evaluates every element eagerly, so
  `repeat with h in {table 1 of …, outline 1 of …}` throws on the first invalid index before any
  `try` catches it; `dialog-pane.applescript` gives each accessor its own `try` block.
- `AXUIElementCopyMultipleAttributeValues` returns an `AXValue` of type `axError` for every
  unsupported attribute. Rendered naively this fills the JSON with `"<AXValue 5>"`; it means
  absence, and `ax-dump` omits the key.
- A `VAR=x cmd` prefix applies only to the command on that line, not to a later one in the same
  function body; `inspect-app.sh` exports `DIALOG_AX_VISIBLE`, `DIALOG_WINDOW_ID`, `PANE_COUNT`,
  and `DIALOG_CAPTURE` explicitly.

## [07]-[WHAT_STILL_REQUIRES_COMPUTER_USE]

The scripting DOM is the better answer than computer-use wherever it reaches. Adobe's own
recommendation for panel work is ExtendScript/UXP, and the ExtendScript dumps under
`<Sources>/{illustrator,photoshop,indesign}/` took that route. Photoshop's `batchPlay` covers far
more than its typed DOM; InDesign's DOM exposes even dialog widgets as objects (`Widget`,
`DialogColumn`, `RadiobuttonGroup`), the structure AX refuses to give.

Illustrator: panel flyout menus (no `AXMenuButton`); anything nested below the flat depth-3
child list; panel drag and dock rearrangement. Everything else is readable when the app is idle.

Photoshop: the entire document UI: toolbar, Options bar, all panels, Layers rows, swatches,
brushes. Menus and all six dialogs are fully scriptable, so menu-driven automation is viable for
commands and preferences. Panel state needs `batchPlay` or a capture.

InDesign: the entire document UI, and every dialog, since they open invisibly to AX. Dialogs
open and cancel blind (`⌘.`) and capture by window id; the InDesign pass writes each capture as
`dialog-<label>.png` under `.artifacts/creative-cloud/inspection/indesign/`;
reading a value back requires OCR, computer-use, or the DOM. The 3670-node menu tree is fully
readable, so menu-driven automation works.

Acrobat: the entire document UI and all tool panels. Preferences is fully scriptable across 38
panes. JavaScript for Acrobat and Action Wizard cover the rest.

Creative Cloud: nothing. The web tree is complete.

Typeface-beta: not running at the first measurement; `<Inputs>/ax/run.sh typeface` covers it, and
`<Inputs>/ax/typeface.json` `status` records `ok: true` for process `Typeface-beta`.

| [OPEN] | Route for values AX withholds | Deciding experiment |
| :-- | :-- | :-- |
| [01] | InDesign's four dialogs and the Photoshop, Acrobat, and InDesign document UI deliver no AX values. The inspectors write `status`/`error` JSON and the window-id capture and route the surface to this section; whether the reading route for those values is the capture alone, computer-use, or the scripting DOM (`indesign-uxp.md`, `photoshop-uxp.md`) is the user's decision | The InDesign pass opens each of the four dialogs by AX menu click with InDesign idle and diffs `AxDump windowlist` before and after ([03.4]); a dialog whose window enters the AX list moves to the AX column of the table below, four dialogs that never enter it settle the scripting DOM as the reading route |

### [07.1]-[MENU_AND_DIALOG_REACH]

| App | Menu bar read | Menu item click | Preferences dialog | Dialog controls readable |
| :-- | :-- | :-- | :-- | :-- |
| Illustrator | yes, when idle | yes, when idle | opens; Color Settings AX-invisible under load | unconfirmed, no idle run |
| Photoshop | yes, 1151 nodes | yes | opens, AX-visible | yes, 208 controls |
| InDesign | yes, 3670 nodes | yes | opens, AX-invisible | no, capture only |
| Acrobat | yes, 382 nodes | yes | opens, AX-visible | yes, 38 panes |
| Creative Cloud | yes, 191 nodes | yes | n/a | n/a |

Menu-driven automation is sound for all four Adobe apps: the menu bar is `NSMenu` and reads
completely without opening anything visually, including `AXMenuItemCmdChar`,
`AXMenuItemCmdModifiers` and `AXMenuItemMarkChar`. Keyboard Maestro's author states that
cross-platform apps "do not necessarily update or even build their menu bar until the menu is
selected with the mouse"; on these 2026 builds full menu trees came back with no menu opened.
