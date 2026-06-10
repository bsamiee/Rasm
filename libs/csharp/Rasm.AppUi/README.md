# [RASM_APPUI]

`Rasm.AppUi` is the product UI engine for plugins and apps. It integrates Avalonia, ReactiveUI, ReactiveUI.Validation, DynamicData, SkiaSharp, LiveCharts2, official Avalonia controls, property inspection, path/SVG assets, theme tokens, typography roles, dialogs, diagnostics, and host evidence as one retained UI rail above Rhino and GH2 host-boundary UI rails.

## [1]-[PURPOSE]

`Rasm.AppUi` provides one app-surface rail for shell state, panels, screens, commands, input, live projections, charts, tables, hierarchical inspectors, theme, typography, icon assets, custom visuals, dialogs, accessibility, diagnostics, and support views.

It is not an Avalonia wrapper, ReactiveUI wrapper, Skia wrapper, charting wrapper, icon package wrapper, Eto replacement, Rhino panel clone, GH2 canvas abstraction, or collection of independent UI feature systems.

## [2]-[STATUS]

| [INDEX] | [SURFACE]          | [STATE]                                                                                    |
| :-----: | ------------------ | ------------------------------------------------------------------------------------------ |
|   [1]   | Project file       | Present and in `Workspace.slnx`                                                            |
|   [2]   | Production API     | Unified UI rail contract defined                                                           |
|   [3]   | Package references | Active direct, inherited global, central closure, and rejected states classified           |
|   [4]   | Runtime behavior   | Host, companion, and sidecar evidence are part of the implementation contract              |
|   [5]   | Host evidence      | Owner-local receipts required for load, focus, scale, disposal, native assets, screenshots |

`Rasm.AppUi.csproj` anchors the folder for restore/build routing and references the no-cost AppUi matrix versionlessly. Package versions live in `Directory.Packages.props`; paid/pro packages are rejected unless a validated no-cost license route is recorded.

## [3]-[CONSTRAINTS]

- One typed app-surface rail owns shell, screen, command, input, live-view, chart, visual, inspector, table, hierarchy, theme, typography, assets, dialogs, accessibility, and diagnostic concerns.
- AppUi is built as a complete UI package for Rhino panels, GH2 canvas/popup/component/overlay surfaces, companion windows, sidecar shells, downstream app shells, support views, and diagnostics through the same UI rail.
- Toolkit and provider types stay internal behind product concepts.
- One canonical UI scheduler boundary owns Avalonia dispatcher, ReactiveUI scheduler, and host-thread affinity.
- Folder architecture is rail-first: screens, commands, assets, providers, themes, typography roles, inspectors, charts, dialogs, host adapters, and diagnostic evidence add catalog rows, discriminants, receipts, or adapter records to existing shapes instead of adding parallel UI systems or provider-specific public types.
- Shell mode, host surface, resource provider, asset source, theme variant, typography role, command availability, and live projection are parameterized data, not hardcoded implementation branches.
- Avalonia owns retained panels, dialogs, companion-window surfaces, and sidecar app shells through the same shell/screen/command rail; viewport overlays render through Rhino/GH display conduits, never Avalonia.
- AppUi SkiaSharp is retained charts, thumbnails, SVG assets, custom visuals, and offscreen draw only; viewport-overlay SkiaSharp lives in the Rhino/GH display conduit.
- `MacOSPlatformOptions.DisableAvaloniaAppDelegate = true`; retained panels use `CreateEmbeddableTopLevel()`.
- TFM is plain `net10.0`; NSView reparenting stays inside the host embedding boundary.
- No UI code bypasses `Rasm.Rhino/UI` or `Rasm.Grasshopper/UI`.
- No ViewModel rail mixes ReactiveUI with another MVVM idiom.
- Theme is official Avalonia Fluent plus AppUi token/control-theme catalogs.
- Icons and graphics are source-neutral catalog entries keyed by product asset identities, not provider-branded public API.
- Embedded resources, generated icon providers, custom assets, host-provided resources, and runtime-loaded assets use the same asset catalog contract.
- Typography uses role-based font policy, embedded font collections, fallback mappings, and HarfBuzz shaping; it is not a single-font rule.
- Hierarchical inspectors and table details use OSS rails: typed row models, grouping, expansion state, virtualization, DataGrid presentation adapters, and direct `ItemsRepeater` usage under a source owner.
- AppUi carries its own Skia/HarfBuzz native assets and records copied asset identity, loaded native identity, and duplicate-family conflict state in runtime evidence.
- Rhino panels, GH2 canvas/popup/component surfaces, companion windows, sidecar shells, and downstream app shells use one shell/screen/command/live/theme/asset rail.
- GH2 retained UI uses the GH2 host surfaces that exist for canvas, popup, toolbar, component-panel, and overlay behavior. A GH2 dockable retained-panel host is one host adapter over the same rail.
