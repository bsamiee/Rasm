# [H1][RASM_APPUI]
>**Dictum:** *AppUi expresses product intent; Rhino and GH2 own host execution.*

<br>

`Rasm.AppUi` is the advanced product UI platform for Rasm plugins and apps. It integrates Avalonia, ReactiveUI, ReactiveUI.Avalonia, ReactiveUI.Validation, DynamicData, SkiaSharp, LiveCharts2, and the full adjacent-package matrix as one unified surface, lowering typed product intent through the existing `Rasm.Rhino/UI` and `Rasm.Grasshopper/UI` host rails.

---
## [1][PURPOSE]
>**Dictum:** *Compose one app surface, not copy host widgets.*

<br>

`Rasm.AppUi` provides a single app-surface rail for windows, panels, screens, command state, live projections, charts and dashboards, custom visuals, diagnostics, and support views. It aggregates product UI intent and typed receipts, then delegates final host behavior to the `Rasm.Rhino/UI` and `Rasm.Grasshopper/UI` rails.

It is not an Avalonia wrapper, ReactiveUI wrapper, Skia wrapper, charting wrapper, Eto replacement, Rhino panel clone, or GH2 canvas abstraction.

---
## [2][STATUS]
>**Dictum:** *One unified surface, built fully from the foundation.*

<br>

| [INDEX] | [SURFACE]          | [STATE]                                          |
| :-----: | ------------------ | ------------------------------------------------ |
|   [1]   | Project file       | Present and in `Workspace.slnx`                  |
|   [2]   | Production API     | In progress                                      |
|   [3]   | Package references | Active direct; every pinned AppUi package referenced |
|   [4]   | Runtime behavior   | Per host scenario                                |
|   [5]   | Host evidence      | Owner-local receipts                             |

`Rasm.AppUi.csproj` now anchors the folder for restore/build routing and references the AppUi matrix versionlessly. No package versions appear in AppUi docs; versions live exclusively in `Directory.Packages.props`.

---
## [3][MANUAL]
>**Dictum:** *Read the owner manual before inventing UI surface.*

<br>

| [INDEX] | [FILE]             | [READ_FOR]                                                                                                               |
| :-----: | ------------------ | ------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | `_ARCHITECTURE.md` | Rail contract, type shapes, host delegation, package matrix, embedding rules, world-class capabilities, runtime evidence |
|   [2]   | `AGENTS.md`        | Build rules, forbidden duplicates, embedding constraints, package rejections                                             |
|   [3]   | `ROADMAP.md`       | Build sequence, integration surfaces, promotion criteria, Phase-0 verification gates (§6)                                |

---
## [4][CONSTRAINTS]
>**Dictum:** *One rail, one paradigm, host execution delegated.*

<br>

- One typed app-surface rail owns shell, screen, command, live-view, chart, visual, and diagnostic concerns.
- Toolkit types stay internal behind product concepts.
- One canonical UI scheduler boundary (`RasmUiScheduler`); DynamicData change-sets observe on it before binding.
- Avalonia owns panels, dialogs, and companion-window surfaces; viewport overlays render through the Rhino/GH display conduit, never Avalonia.
- AppUi SkiaSharp is thumbnails and offscreen draw only; viewport-overlay SkiaSharp lives in the Rhino/GH display conduit.
- `MacOSPlatformOptions.DisableAvaloniaAppDelegate = true`; use `CreateEmbeddableTopLevel()` — `CreateEmbeddableWindow` is unimplemented on macOS.
- TFM is plain `net10.0`; NSView reparenting via isolated P/Invoke shim (`objc_msgSend`) — no `net10.0-macos`.
- No UI code bypasses `Rasm.Rhino/UI` or `Rasm.Grasshopper/UI`.
- No ViewModel rail mixes ReactiveUI and another MVVM idiom in the same screen stack.
- GH2-Avalonia embedding is [DEFERRED] — the current GH2 SDK has no dockable plugin-panel host API; Rhino-panel embedding is the supported path. Re-evaluate when a GH2 panel-host API ships.
