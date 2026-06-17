# [APPUI]

`Rasm.AppUi` is the APP-PLATFORM product-UI engine: one Avalonia shell that mounts host-neutrally onto any admitted substrate — Rhino panel and modal, GH2 companion window, standalone desktop, sidecar shell, headless proof surface — through one abstract `SurfaceHost` axis, consuming AppHost ports, Persistence queries, and Compute receipts as settled vocabulary and never referencing Rhino or GH directly. This README routes the `.planning/` design pages and registers every external package the folder draws on; the domain folder-map lives in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the open work in `TASKLOG.md`.

## [1]-[PAGE_ROUTER]

| [INDEX] | [SUB_DOMAIN] | [PAGE]                                                              | [OWNS]                                                              |
| :-----: | :----------- | :------------------------------------------------------------------ | :----------------------------------------------------------------- |
|   [1]   | hosts        | [surface-hosts](.planning/hosts/surface-hosts.md)                   | host axis, mount transaction, embed capsule, scheduler, native assets |
|   [2]   | shell        | [shell-navigation](.planning/shell/shell-navigation.md)             | shell, routing spine, dock layouts, chrome, adaptive breakpoints   |
|   [3]   | screens      | [screens-activation](.planning/screens/screens-activation.md)       | screen catalog, activation scopes, derived state, validation, snapshots |
|   [4]   | commands     | [commands-availability](.planning/commands/commands-availability.md) | intent table, availability algebra, execution receipts, projections |
|   [5]   | livedata     | [live-data](.planning/livedata/live-data.md)                        | data sources, change-set pipelines, binding capsule, aggregation   |
|   [6]   | tables       | [tables-hierarchy](.planning/tables/tables-hierarchy.md)            | column metadata, view-state snapshots, tree-flatten fold, grid commit |
|   [7]   | inspector    | [inspector-editing](.planning/inspector/inspector-editing.md)       | editor factories, commit validation, options inspector, conflicts, code panes |
|   [8]   | charts       | [charts-dashboards](.planning/charts/charts-dashboards.md)          | chart series/axes, stream binding, dashboard tiles, cross-filter brushing |
|   [9]   | charts       | [custom-visuals](.planning/charts/custom-visuals.md)                | Skia diagram/geo kinds, wide-gamut color-space axis                |
|  [10]   | visuals      | [visuals-offscreen](.planning/visuals/visuals-offscreen.md)         | draw capsule, thumbnails, previews, encode identity, document export |
|  [11]   | theme        | [theme-tokens](.planning/theme/theme-tokens.md)                     | token catalog, variant/density axes, control themes, colormaps     |
|  [12]   | typography   | [typography-shaping](.planning/typography/typography-shaping.md)    | typography roles, font admission, shaping rail, markdown projection, text metrics |
|  [13]   | assets       | [icons-assets](.planning/assets/icons-assets.md)                    | asset catalog, icon sourcing, SVG pipeline, raster assets          |
|  [14]   | dialogs      | [dialogs-notifications](.planning/dialogs/dialogs-notifications.md) | dialog intents, session topology, notifications, host-agnostic pickers |
|  [15]   | input        | [input-interaction](.planning/input/input-interaction.md)           | hotkey derivation, behavior rail, pointer gestures, drag/clipboard, input fabric |
|  [16]   | motion       | [motion-tokens](.planning/motion/motion-tokens.md)                  | motion axis, application plans, phase mapping, reduced-motion       |
|  [17]   | access       | [accessibility](.planning/access/accessibility.md)                  | automation peers, keyboard nav, contrast gate, compliance proof, 3D-scene tree |
|  [18]   | localization | [localization-culture](.planning/localization/localization-culture.md) | locale axis, string tables, culture composition, RTL mirroring     |
|  [19]   | evidence     | [diagnostics-evidence](.planning/evidence/diagnostics-evidence.md)  | evidence union, correlation join, capture lanes, headless proof, dev loop |
|  [20]   | viewport     | [viewport-pipeline](.planning/viewport/viewport-pipeline.md)        | render graph, virtualized geometry, residency, path trace, sim, viewpoint codec |
|  [21]   | drafting     | [drafting-sheets](.planning/drafting/drafting-sheets.md)            | sheet set, projection, dimensioning, GD&T, draft emit              |
|  [22]   | notebook     | [notebook-document](.planning/notebook/notebook-document.md)        | cell model, dependency graph, CRDT co-edit, replay bundle          |
|  [23]   | animation    | [animation-timeline](.planning/animation/animation-timeline.md)     | keyframe tracks, timeline playhead, scrub, walkthrough             |

The `realitycapture/` and `coordination/` sub-domains are planned and carry no design page yet; their charters are in `ARCHITECTURE.md` and their work is queued in `TASKLOG.md`.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as one flat registry grouped by domain concern. Versions are centralized in the one C# manifest and never pinned here.

[AVALONIA]:
- Avalonia
- Avalonia.Desktop
- Avalonia.Skia
- Avalonia.Headless
- Avalonia.Headless.XUnit
- Avalonia.Themes.Fluent
- Avalonia.Fonts.Inter
- Avalonia.Controls.DataGrid
- Avalonia.Controls.ColorPicker
- Avalonia.AvaloniaEdit
- AvaloniaEdit.TextMate

[REACTIVE]:
- ReactiveUI
- ReactiveUI.Avalonia
- ReactiveUI.Validation
- Xaml.Behaviors.Avalonia
- System.Reactive
- DynamicData

[DOCKING_DIALOGS]:
- Dock.Avalonia
- Dock.Model.ReactiveUI
- Dock.Serializer.SystemTextJson
- DialogHost.Avalonia

[INSPECTION_CHARTING]:
- bodong.Avalonia.PropertyGrid
- bodong.PropertyModels
- LiveChartsCore.SkiaSharpView.Avalonia

[RENDER_TEXT]:
- SkiaSharp
- SkiaSharp.HarfBuzz
- SkiaSharp.NativeAssets.macOS
- SkiaSharp.NativeAssets.Linux
- SkiaSharp.NativeAssets.Linux.NoDependencies
- HarfBuzzSharp.NativeAssets.macOS
- HarfBuzzSharp.NativeAssets.Linux
- Svg.Controls.Skia.Avalonia
- Svg.Skia

[GPU_BACKEND]:
- VelloSharp

[ASSETS_CONTENT]:
- AsyncImageLoader.Avalonia
- FluentIcons.Avalonia
- FluentIcons.Common
- Markdig
- PanAndZoom
- Wacton.Unicolour

[DOMAIN_RUNTIME]:
- Thinktecture.Runtime.Extensions
- Thinktecture.Runtime.Extensions.Json
- LanguageExt.Core
- NodaTime
- UnitsNet
- System.IO.Hashing

[DRAFTING_EXPORT]:
- ACadSharp
- netDxf
- DocumentFormat.OpenXml

[DEV_TEST]:
- HotAvalonia
- Avalonia.Markup.Xaml.Loader
- Verify.XunitV3
