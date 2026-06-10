# [RASM_APPUI]

`Rasm.AppUi` is the product UI engine for Rasm plugins and apps. It integrates Avalonia, ReactiveUI, ReactiveUI.Validation, DynamicData, SkiaSharp, LiveCharts2, official Avalonia controls, property inspection, path/SVG assets, theme tokens, typography roles, dialogs, diagnostics, and host evidence as one retained UI rail that lowers host behavior through `Rasm.Rhino/UI` and `Rasm.Grasshopper/UI`.

## [1]-[PURPOSE]

`Rasm.AppUi` provides one app-surface rail for shell state, panels, screens, commands, input, live projections, charts, tables, tree tables, inspectors, theme, typography, icon assets, custom visuals, diagnostics, and support views.

It is not an Avalonia wrapper, ReactiveUI wrapper, Skia wrapper, charting wrapper, icon package wrapper, Eto replacement, Rhino panel clone, or GH2 canvas abstraction.

## [2]-[STATUS]

| [INDEX] | [SURFACE]          | [STATE]                                              |
| :-----: | ------------------ | ---------------------------------------------------- |
|   [1]   | Project file       | Present and in `Workspace.slnx`                      |
|   [2]   | Production API     | Source rails pending                                 |
|   [3]   | Package references | Active direct; every pinned AppUi package referenced |
|   [4]   | Runtime behavior   | Per host scenario                                    |
|   [5]   | Host evidence      | Owner-local receipts                                 |

`Rasm.AppUi.csproj` anchors the folder for restore/build routing and references the AppUi matrix versionlessly. Package versions live in `Directory.Packages.props`.

## [3]-[MANUAL]

| [INDEX] | [FILE]             | [READ_FOR]                                                                                                                      |
| :-----: | ------------------ | ------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `ARCHITECTURE.md` | Rail contract, host delegation, package matrix, type shapes, theme, typography, assets, capabilities, runtime evidence          |
|   [2]   | `ROADMAP.md`       | Build sequence, rail implementation order, integration surfaces, host proof, completion gates                                   |

## [4]-[CONSTRAINTS]

- One typed app-surface rail owns shell, screen, command, input, live-view, chart, visual, inspector, theme, typography, assets, dialogs, accessibility, and diagnostic concerns.
- Toolkit and provider types stay internal behind product concepts.
- One canonical UI scheduler boundary (`RasmUiScheduler`); DynamicData change-sets observe on it before binding.
- Avalonia owns retained panels, dialogs, and companion-window surfaces; viewport overlays render through Rhino/GH display conduit, never Avalonia.
- AppUi SkiaSharp is retained charts, thumbnails, SVG assets, and offscreen draw only; viewport-overlay SkiaSharp lives in the Rhino/GH display conduit.
- `MacOSPlatformOptions.DisableAvaloniaAppDelegate = true`; use `CreateEmbeddableTopLevel()`.
- TFM is plain `net10.0`; NSView reparenting stays inside the host embedding boundary.
- No UI code bypasses `Rasm.Rhino/UI` or `Rasm.Grasshopper/UI`.
- No ViewModel rail mixes ReactiveUI with another MVVM idiom.
- Theme is official Avalonia Fluent plus AppUi token/control-theme catalogs.
- Icons are path/SVG catalog entries keyed by product `IconKey`, not provider-branded public API.
- Typography uses role-based font policy, embedded font collections, fallback mappings, and HarfBuzz shaping; it is not a single-font rule.
- GH2-Avalonia embedding starts only after a GH2 dockable panel-host API exists and the GH2 UI rail owns that extension.
