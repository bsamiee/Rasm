# [RASM_APPUI]

`Rasm.AppUi` is the product UI engine for host surfaces, companion windows, sidecar shells, downstream app shells, support views, and diagnostics. It owns shell, screen, command, live projection, chart, visual, inspector, table, hierarchy, theme, typography, asset, dialog, accessibility, and UI evidence as one UI rail.

## [1]-[PURPOSE]

`Rasm.AppUi` defines the retained UI package above Rhino and Grasshopper host-boundary UI rails. Avalonia, ReactiveUI, DynamicData, SkiaSharp, LiveCharts2, controls, behaviors, dialogs, typography, and assets feed product UI concepts instead of becoming public provider vocabularies.

It is not an Avalonia wrapper, ReactiveUI wrapper, charting wrapper, icon package wrapper, Eto replacement, Rhino panel clone, GH2 canvas abstraction, or collection of independent UI feature systems.

## [2]-[STATUS]

This table is a lookup by package surface.

| [INDEX] | [SURFACE]          | [STATE]                                    |
| :-----: | :----------------- | :----------------------------------------- |
|   [1]   | Project file       | present in `Workspace.slnx`                |
|   [2]   | Package manifest   | retained UI packages admitted              |
|   [3]   | Host classification | Rhino/GH/Eto aware                         |
|   [4]   | Lockfile           | restored package closure tracked           |
|   [5]   | Production source  | absent                                     |
|   [6]   | Package law        | documented in this folder                  |

## [3]-[DOCUMENTS]

This table routes package documents by reader action.

| [INDEX] | [READ_FOR]              | [OPEN]                      |
| :-----: | :---------------------- | :-------------------------- |
|   [1]   | current structure       | [architecture](ARCHITECTURE.md) |
|   [2]   | implementation sequence | [roadmap](ROADMAP.md)       |
|   [3]   | package API catalogue   | [.reports/api](.reports/api/README.md) |

## [4]-[CONSTRAINTS]

- AppUi is one app-surface rail for Rhino panels, GH2 canvas/popup/component/overlay surfaces, companion windows, sidecar shells, downstream app shells, support views, and diagnostics.
- Public vocabulary is product vocabulary. Toolkit, provider, host, native-handle, scheduler, asset-provider, and charting types remain internal unless a host boundary requires them.
- One UI scheduler boundary owns Avalonia dispatcher, ReactiveUI scheduler, and host-thread affinity.
- Shell mode, host surface, resource provider, asset source, theme variant, typography role, command availability, and live projection are parameterized data.
- Retained panels, dialogs, companion windows, sidecar shells, and downstream app shells use Avalonia through the same shell/screen/command/live/theme/asset rail.
- Viewport overlays render through Rhino/GH display conduits. AppUi SkiaSharp owns retained charts, thumbnails, SVG assets, custom visuals, and offscreen draw.
- Theme uses official Avalonia Fluent plus AppUi token and control-theme catalogues. Typography uses roles, embedded font collections, fallback mappings, and HarfBuzz shaping.
- Hierarchical inspectors and table details use typed row models, grouping, expansion state, virtualization, DataGrid adapters, and source-owned item projection.
- AppUi carries native asset identity evidence for SkiaSharp and HarfBuzz families.
