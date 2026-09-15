# [AX]-[ADOBE_APP_INSPECTORS]

Read-only accessibility inspectors for Illustrator, Photoshop, InDesign, Acrobat, Creative Cloud
and Typeface. Each emits JSON under `<app>/out/`. Nothing is clicked but menu items, preference
list rows, and Cancel.

Measured on Darwin 25.6 (macOS 26 Tahoe), Apple Silicon, 2026-09-11, with Accessibility and
Screen Recording both granted to the terminal.

## [01]-[COMMANDS]

```sh
cd <this directory>
./run.sh                      # build ax-dump, then inspect every application
./run.sh photoshop acrobat    # inspect only the named ones
./illustrator/inspect.sh      # one application directly
AX_TIMEOUT=20 ./run.sh indesign   # raise the AX messaging timeout for a loaded app
```

The Swift tool can be driven on its own once `run.sh` has compiled it to `.bin/ax-dump`:

```sh
./.bin/ax-dump menus       <pid>   # menu bar tree with shortcuts and check marks
./.bin/ax-dump tree        <pid>   # every window's element tree
./.bin/ax-dump dialog      <pid>   # sheets and modal windows only
./.bin/ax-dump windowlist  <pid>   # window-server geometry, works when AX does not
./.bin/ax-dump unlock      <pid>   # AXManualAccessibility / AXEnhancedUserInterface probe
```

Outputs per app: `status.json`, `menus.json`, `tree.json`, `layout.json`, `windowlist.json`,
`ax-unlock.json`, `dialogs.jsonl`, `dialog-<label>.json`, `dialog-<label>-pane-<n>.json`,
`dialog-<label>.png`, `window.png`.

## [02]-[TOOLING_VERDICTS]

### Two things I assumed, measured, and had to retract

**System Events is not missing the exotic attributes.** Its generic accessor returns them by
name: `value of attribute "AXFrame" of window 1` gives `{0, 39, 1800, 1121}`, and
`AXChildrenInNavigationOrder` returns element references. There is no read-reach gap over raw
`AXUIElement`.

**JXA can call the AX C API.** The failure mode is specific: `ObjC.import('ApplicationServices')`
plus bridgesupport metadata does not annotate the `CFTypeRef *` out-parameters, so `Ref()`
unwrapping yields `{}` and `ObjC.castRefToObject` kills the interpreter. Import `Cocoa` and
declare the signature and it works first try:

```js
ObjC.import('Cocoa');
ObjC.bindFunction('AXUIElementCopyAttributeNames', ['int', ['id', 'id*']]);
// -> err 0, the full 19-attribute list
```

### The measured difference is modest

Full Illustrator menu bar, cross-validated: the Swift walker counted 4036 nodes, System Events'
`entire contents` returned 4037 including the root.

| Method | Illustrator menu bar | Photoshop menu bar | Returns |
| :-- | --: | --: | :-- |
| System Events `entire contents` | 5.39 s | 0.76 s | bare references, **no attributes** |
| Swift, one call per attribute | 4.15 s | 0.60 s | 5 attributes per node |
| Swift, `AXUIElementCopyMultipleAttributeValues` | **3.00 s** | — | 5 attributes per node |

Where System Events can batch with a plural form it is excellent:
`{role, description, position, size} of every UI element of window 1` returned all 346 Illustrator
nodes in **0.74 s**, because `every` collapses into one Apple event.

**So the reason these inspectors are built on Swift is output shape, not speed.** AppleScript
returns nested structures as flat comma-mashed lists — `AXFrame` coerced to text arrives as
`03918001121` — and the deliverable is JSON trees with per-node paths. Swift emits that directly,
decodes `AXValue` structs through `AXValueGetValue`, batches attributes in one IPC, and is the
only one of the three that can attempt the `AXEnhancedUserInterface` write. System Events is used
for exactly what it is better at: clicking a menu item and a list row.

`xa11y` was evaluated and **not adopted here**. It adds selectors and a driver loop over the same
AXUIElement calls; this task needed a raw tree dump, which is ~200 lines of Swift with no
dependency. It remains the right choice for the monorepo proper — see below.

### Library survey, verified 2026-09-11

| Tool | Stars | Last commit | Verdict |
| :-- | --: | :-- | :-- |
| [xa11y](https://github.com/xa11y/xa11y) | 71 | 2026-09-11 | **Adopt for the monorepo.** Rust core, MIT, ships PyPI `xa11y` and npm `@crowecawcaw/xa11y` at the same version (0.14.0, 2026-09-09) plus `pytest-xa11y`. No code signing needed. Risks: 6 months old, 15 npm versions in 5 months, npm scope does not match the org. |
| [pyax](https://github.com/eeejay/pyax) | 15 | 2026-02-28 | **Adopt as a devtool.** `pyax tree <app> --json`, `inspect`, `observe`. The only Python tool exposing **AXObserver notifications**, which is the one capability System Events genuinely lacks. |
| pyobjc-framework-ApplicationServices | — | 12.2.2, 2026-08-11 | The floor. Python 3.10–3.15, `arm64-26.5` metadata. **Not installed in this repo's `.venv`,** and the system `python3` (3.15.0rc2) has no `Quartz`/`AppKit` — so the Python AX path costs an install today while Swift and osascript cost nothing. |
| [Peekaboo](https://github.com/openclaw/Peekaboo) | 5,144 | 2026-09-08 | Turnkey CLI/MCP, signed, `see --json` + `action`. A product with a daemon, not a library. |
| [atomacos](https://github.com/daveenguyen/atomacos) | 49 | 2021-05-24 | **Do not adopt — archived.** Classifiers stop at Python 3.7. `pyatom/atomacos` and `pyatom/ATOMac` are 404. |
| [AXSwift](https://github.com/tmandry/AXSwift) | 415 | 2021-11-14 | Dead, as are all forks (`ax-kit` 3★, `axkit` 2★, `AXSwiftExt` 3★). |
| [nut.js](https://github.com/nut-tree/nut.js) | 2,850 | 2024-05-01 | Dead, unpublished from npm, and pixel-based with no AX. |
| Hammerspoon `hs.axuielement` | 16,085 | 2026-07-08 | Excellent API, but a GUI app driven over `hs.ipc` — wrong shape for a monorepo target. |

**Claude Code skills for macOS AX: none worth adopting.** Every hit is 0–2 stars and personal.

### The macOS 26 grant, and why it is not the explanation here

xa11y's source states that on macOS 26+ the Accessibility grant alone no longer suffices:
without Screen Recording, *"AXChildren returns only self-referencing AXApplication wrappers and
menu bars."* This machine is macOS 26 and Screen Recording **is** granted. Illustrator returned
346 real window children and Creative Cloud 301 through the same terminal, so the grant is
working, and **Photoshop's 13 nodes and Acrobat's 6 are the applications being empty, not a
missing TCC grant.** That control matters because it is the first thing a reader will suspect.

## [03]-[WHAT_ADOBE_ACTUALLY_EXPOSES]

### The rule that explains every result

**A native Cocoa surface is fully accessible. A surface Adobe draws itself is invisible.**

Adobe's current UI framework is **Drover** — described in an Adobe job posting (2026-07-30) as
"the framework behind many of Adobe's flagship desktop applications… from Photoshop to
Illustrator to Premiere Pro", a "high-performance native framework" whose listed problem space
includes "compositing and rendering". Photoshop was migrated onto it in **27.9, rolling out from
2026-07-26**. Its predecessor OWL is confirmed by an InDesign CS6 crash log loading
`com.adobe.owl` and `AdobeOwlCanvas`. Drover paints its own widgets and bridges nothing to
NSAccessibility.

The split is sharp enough to be a working rule:

| Surface | Framework | AX result |
| :-- | :-- | :-- |
| Menu bar | Cocoa `NSMenu` | **Full**, every app, including shortcuts and check marks |
| Photoshop / Acrobat Preferences | Cocoa dialog | **Full** — 208 and 25 labelled controls |
| InDesign Preferences, Color Settings | Drover | **Invisible** — not in the AX window list at all |
| Document window, panels, toolbars | Drover | **Empty** everywhere except Illustrator |
| Creative Cloud | WebKit | **Full**, depth 19–27 |

### Measured, per app

| App | Menu nodes | Window nodes | Depth | Dialogs AX-visible |
| :-- | --: | --: | --: | :-- |
| Illustrator 2026 Beta | 4036 † | **346** † | 3 | not probed, see §06 |
| InDesign 2026 Beta | 3670 | 47 | 3 | **none of 4** |
| Photoshop 2026 Beta | 1151 | **13** | 3 | **all 6** |
| Acrobat Pro | 382 | **6** | 3 | **1 of 1, 38 panes** |
| Creative Cloud | 191 | 159–301 | 19–27 | n/a |
| Typeface-beta | — | — | — | not running |

† Illustrator was idle and fully readable early in the session, then was wedged at 100 % CPU by a
concurrent session for the rest of it. Its committed JSON records the wedged state; these two
numbers come from the earlier readable window. See §06.

These match Adobe's own conformance reports, which rate all four apps **4.1.2 Name/Role/Value →
Does Not Support**. The Photoshop macOS report (Oct 2023) claims only menus: *"A clear hierarchy
is represented in menus; however, most information and relationships… cannot be programmatically
determined."* One premise correction: **Acrobat is not the accessible one** — its April 2025
report lists *Prepare for Accessibility* itself among functions that cannot be keyboard-operated.
Its reputation concerns the PDFs it produces, not its chrome.

### Precise claims, per surface

- **Photoshop's Layers panel rows are not AX children.** Its entire window is 13 nodes: window
  chrome, plus a handful of real `NSControl`s that leak through — `AXPopUpButton` values `Layer`,
  `Kind` and `Normal` (blend mode), two `AXTextField`s (opacity and fill) and two scrollbars.
  There is no toolbar, no Options bar, and no layer row. **Capture the region instead.**
- **Photoshop's Preferences dialog is fully accessible** — 208 labelled controls with values,
  e.g. `AXCheckBox "Export Clipboard" = 1`, `AXPopUpButton = "Bicubic Automatic"`. Each pane is
  its own menu item under `Photoshop (Beta) ▸ Settings…`, so there is no left list to walk.
- **InDesign's dialogs open but are invisible to AX.** `Edit ▸ Color Settings...` produced a real
  612×731 window at the window server that never entered the AX window list. Its values are only
  in `indesign/out/dialog-color-settings.png`. Its main window exposes 47 nodes — chrome plus the
  top app bar (`Home`, `Share`, `AI Assistant (beta)`, `What's new in Beta`, `Provide feedback`)
  and a search box whose descriptions leak Drover's internal widget names: `UI_SearchBoxBase`,
  `UI_SearchBoxBasePopupEdit`, `UI_PictureButton`.
- **Acrobat's main window is six nodes** — three traffic lights and an empty group — while its
  **Preferences dialog is a proper `AXOutline` of 38 panes**, each selectable by index, yielding
  labelled checkboxes with values. Panes include `Accessibility`, `Color Management`,
  `Generative AI`, `Security (Enhanced)`, `JavaScript`, `Trust Manager`.
- **Illustrator is the outlier**, and the only app whose panels are readable. See §05.
- **Creative Cloud is the best citizen**: `AXWebArea`, `AXLandmarkNavigation`, `AXHeading`,
  `AXTabButton`, depth 19–27. Node count varied 301 → 159 between runs as its content reloaded,
  so treat any single capture as a snapshot.

### Two mechanisms worth keeping

**Escape does not close a Drover dialog; Command-period does.** InDesign's Color Settings
survived two Escapes with the app verified frontmost, then closed on the first `⌘.`. Since such a
dialog exposes no AX Cancel button, `⌘.` is the only programmatic dismissal. `dialog-close.applescript`
tries the AX Cancel button first and falls back to `⌘.`.

**A new dialog must be detected at the window server, not through AX.** `menu-click.applescript`
therefore only clicks; `inspect-app.sh` diffs `ax-dump windowlist` before and after. This is what
makes InDesign's dialogs recordable at all. Capture uses `screencapture -l <windowId>`, not
`-R <rect>` — the apps sit behind the terminal, and a rect capture returns whatever is on top.

## [04]-[THE_UNLOCK_ATTEMPT]

No published test of these flags against an Adobe application existed. Result, all five apps:

| App | `AXManualAccessibility` | `AXEnhancedUserInterface` | Nodes before → after |
| :-- | :-- | :-- | :-- |
| Photoshop | `-25205` unsupported | `-25208` **not implemented** | 13 → 13 |
| InDesign | `-25205` | `-25208` | 47 → 47 |
| Acrobat | `-25205` | `-25208` | 6 → 6 |
| Creative Cloud | `-25205` | `-25208` | 159 → 159 |
| Illustrator | `-25204` cannot complete (app blocked) | `-25204` | — |

`AXManualAccessibility` is Electron-specific and correctly rejected. `AXEnhancedUserInterface` is
**readable** (it returns `false`) but **`kAXErrorNotImplemented` on write**: Adobe advertises the
attribute and implements no setter. No node count moved, no write persisted, and every app was
left exactly as found. **The unlock does not work on Adobe apps.** This closes a gap that the
literature left open, and it means there is no hidden tree waiting to be switched on.

## [05]-[DERIVED_LAYOUT]

`layout.json` is produced by `lib/derive-layout.py`. Illustrator's tree is **flat** — every panel
tab, panel control and toolbar button is a direct child of `AXWindow` carrying an absolute
`AXFrame` — so the layout is recovered geometrically, not structurally.

Three traps are encoded as rules rather than left to a nearest-match guess:

1. **An `AXTabGroup` is a single tab chip, not a container.** Chips sharing a y form one tab row,
   which is one panel group.
2. **Collapsed groups report negative y or zero size.** Observed in Illustrator:
   `Align [1226,-752]`, `Pathfinder [1273,-752]`, `Properties [1226,-70]`,
   `Stroke [1225,-71 0x0]`, `Transparency [1225,-71 0x0]`. These go to `offscreenTabs`, never
   plotted as visible.
3. **Frames can be garbage.** One Photoshop popup reported `[11459,10779 83x16]` where sibling
   geometry implies `~[1459,779]`. Anything outside the window bounds plus a margin goes to
   `rejected` with a reason.

Two further rules came from mis-attributions caught during the run:

- The window's own close/minimise/zoom buttons sit exactly in the toolbar's x band, so they are
  excluded **by subrole**, not by a y threshold. Before this fix Creative Cloud reported a
  three-tool toolbar that was really the traffic lights.
- A window with fewer than three tab chips is not an Adobe panel dock. `dockModelApplies: false`
  says so explicitly rather than reporting invented columns.

Anything that cannot be attributed to a panel lands in `unattributed`. Nothing is assigned to a
nearest guess.

### Per app

**Illustrator** — the only real dock. From the planning-phase capture: two dock columns at
`x≈1226` and `x≈1479`, a toolbar at `x≈6`/`x≈36` whose buttons carry the shortcut in the
description and selection in the value (`"Blend Tool (W)" = "Selected"`,
`"Mesh Tool (U)" = "Not Selected"`), visible tab rows for `Artboards`/`Links` at `y≈771`,
`Actions`/`History` at `y≈16` and `Layers` at `y≈755`, and panel content such as the Align panel's
buttons at `y≈112–248`. The AI and discovery surface is all present as button descriptions:
`AI Assistant`, `DiscoverBtn`, `Home`, `Share Document`, `Provide feedback`, `New Notifications`,
`This is WhatsNew button`, `Explore starter tutorials in Illustrator`,
`Check your generative credits usage.`, `Switch Workspace`.

**Photoshop, InDesign, Acrobat** — `dockModelApplies: false`. There are no tab chips because
there are no panels in the tree. `layout.json` records the window frame, the few real controls,
and the watchlist; everything else is honestly empty.

**Creative Cloud** — `dockModelApplies: false` (two tab chips). Watchlist hits include `Home`,
`AI Assistant`, `Adobe Firefly`, `Do more with AI Assistant (Beta)`, `Retouch portraits`.

**Panel flyout menus: no app exposes an `AXMenuButton` anywhere.** Every `layout.json` carries
`"flyoutNote": "no AXMenuButton exposed: panel flyout menus require computer-use"`. This is
consistent with Adobe's own guidance — asked how to run a Swatches panel-menu command, Adobe's
community answer was to record an Action and call `app.doScript(action, set)`.

## [06]-[PROBLEMS_SEEN]

**Illustrator stopped answering AX mid-session and never recovered.** It sat pinned at 88–100 %
CPU, driven by a concurrent session on this machine, and every call returned `-25204`
(`kAXErrorCannotComplete`). Raising the messaging timeout to 30 s did not help, which is the
informative part: **a busy Adobe app does not answer slowly, it stops servicing AX entirely.**
The published complaint that Photoshop's Export As dialog "does still respond… but very, very
slowly" describes a gentler failure than what happens here. It was also **relaunched** mid-session
by that other session (pid 61889 → 73941) and the fresh instance was equally wedged, so this is
not a process that degraded over time. `illustrator/out/status.json` records
`cpuPercentAtStart: 100.0` alongside each error so the reading stays interpretable.

Its dialog probe was therefore **not run**, and `illustrator/out/dialogs.jsonl` says so. Opening a
dialog on a wedged app is a trap: the click still reaches the menu, but the dialog cannot be read
and cannot be closed through AX. One Color Settings dialog was opened this way and needed two
attempts and a confirmed `activate` before `⌘.` took effect — captured as
`illustrator/out/dialog-color-settings.png` before it was dismissed. `dialog-close.applescript`
now waits for the process to actually become frontmost, polling up to five seconds, instead of
assuming a fixed delay is enough.

The Illustrator figures quoted in §03 and §05 come from the earlier window in this session when
the app was idle and fully readable (346 window nodes, 4036 menu nodes). They are measurements,
not estimates, but they are not reproduced by the committed `illustrator/out/*.json`, which
honestly record the wedged state instead. **Re-run `./illustrator/inspect.sh` when the app is
idle** to regenerate them.

The same app was fully readable earlier in the session (346 nodes, 4036 menu nodes), and
**InDesign showed the mirror image** — `-25204` on everything at first, then 47 window nodes and
3670 menu nodes an hour later. So `-25204` from an Adobe app means *busy*, not *unsupported*.
Re-run before concluding anything from it.

**Other sessions were driving these same applications throughout.** Node counts drifted between
runs (Illustrator 346 → 349, Photoshop 12 → 13, Acrobat 5 → 6, Creative Cloud 301 → 159) and a
document was opened in Acrobat mid-run. Treat every count here as a snapshot with roughly ±3
nodes of noise, and the Creative Cloud figures as reload-dependent.

**Bugs found and fixed in these scripts, recorded because they are easy to repeat:**

- `container` and `target` are System Events property names. Using either as an AppleScript
  variable inside `tell process` makes `set` parse as a property assignment and fail with
  *"Can't set container of process…"*. Renamed to `theMenu`.
- An AppleScript list literal evaluates every element eagerly, so
  `repeat with h in {table 1 of …, outline 1 of …}` throws on the first invalid index before any
  `try` can catch it. Each accessor now gets its own block.
- `AXUIElementCopyMultipleAttributeValues` returns an `AXValue` of type `axError` for every
  unsupported attribute. Rendered naively this fills the JSON with `"<AXValue 5>"`; it means
  absence and is now emitted as `null`.
- A `VAR=x cmd` prefix applies only to the command on that line, not to a later one in the same
  function body. Exported explicitly.

## [07]-[WHAT_STILL_REQUIRES_COMPUTER_USE]

The scripting DOM is the better answer than computer-use wherever it reaches. Adobe's own
recommendation for panel work is ExtendScript/UXP, and the sibling
`scratchpad/{illustrator,photoshop,indesign}/` dumps already took that route. Photoshop's
`batchPlay` covers far more than its typed DOM; InDesign's DOM exposes even dialog widgets as
objects (`Widget`, `DialogColumn`, `RadiobuttonGroup`).

**Illustrator** — panel flyout menus (no `AXMenuButton`); anything nested below the flat depth-3
child list; panel drag/dock rearrangement. Everything else is readable when the app is idle.

**Photoshop** — the entire document UI: toolbar, Options bar, all panels, Layers rows, swatches,
brushes. Menus and all six dialogs are fully scriptable, so menu-driven automation is viable for
commands and preferences. Panel *state* needs `batchPlay` or a capture.

**InDesign** — the entire document UI, and **every dialog**, since they open invisibly to AX.
Dialogs can still be opened and cancelled blind (`⌘.`) and captured by window id, which is how
`dialog-*.png` were produced; reading a value back requires OCR, computer-use, or the DOM.
The 3670-node menu tree is fully readable, so menu-driven automation works well.

**Acrobat** — the entire document UI and all tool panels. Preferences is fully scriptable across
38 panes. JavaScript for Acrobat and Action Wizard cover the rest.

**Creative Cloud** — nothing. The web tree is complete.

**Typeface-beta** — not running during this session; relaunch and re-run `./typeface/inspect.sh`.

### Menu and dialog reach, for the operators

| App | Menu bar read | Menu item click | Preferences dialog | Dialog controls readable |
| :-- | :-- | :-- | :-- | :-- |
| Illustrator | yes, when idle | yes, when idle | opens; Color Settings was **AX-invisible** | unconfirmed |
| Photoshop | yes, 1151 nodes | yes | opens, AX-visible | **yes, 208 controls** |
| InDesign | yes, 3670 nodes | yes | opens, **AX-invisible** | no — capture only |
| Acrobat | yes, 382 nodes | yes | opens, AX-visible | **yes, 38 panes** |
| Creative Cloud | yes, 191 nodes | yes | n/a | n/a |

Menu-driven automation is sound for all four Adobe apps: the menu bar is `NSMenu` and reads
completely without opening anything visually, including `AXMenuItemCmdChar`,
`AXMenuItemCmdModifiers` and `AXMenuItemMarkChar`. One published claim did **not** hold here —
that cross-platform apps "do not necessarily update or even build their menu bar until the menu
is selected with the mouse". On these 2026 builds full menu trees came back with no menu opened.
