# [APPUI]

`Rasm.AppUi` is the product UI engine with zero consumers; the implementation is full-capability with no holding back. The `.planning/` pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. One shell mounts on every admitted host substrate — Rhino panels, Rhino modals, GH2 companion windows, standalone desktop, sidecar shells, and headless proof surfaces — through one `SurfaceHost` axis. The package owns shell, navigation, screens, commands, live data, tables, inspectors, charts, offscreen visuals, theme, typography, icons, dialogs, input, motion, accessibility, localization, evidence, viewport, drafting, notebooks, and animation as one UI rail consuming AppHost ports, Persistence queries, and Compute receipts as settled vocabulary. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                      | [OWNS]                                                                |
| :-----: | :---------------------------------------------------------- | :-------------------------------------------------------------------- |
|   [1]   | [surface-hosts](Hosts/.planning/surface-hosts.md)                 | host axis, mount, marshal rows                                        |
|   [2]   | [shell-navigation](Shell/.planning/shell-navigation.md)           | shell, routing, dock layouts                                          |
|   [3]   | [screens-activation](Screens/.planning/screens-activation.md)       | screen family, activation, lifecycle                                  |
|   [4]   | [commands-availability](Commands/.planning/commands-availability.md) | intent table, availability, invocation                                |
|   [5]   | [live-data](LiveData/.planning/live-data.md)                         | data sources and change-set spine                                     |
|   [6]   | [tables-hierarchy](Tables/.planning/tables-hierarchy.md)           | table projections and tree flattening                                 |
|   [7]   | [inspector-editing](Inspector/.planning/inspector-editing.md)         | editors, validation, options, conflicts                               |
|   [8]   | [charts-dashboards](Charts/.planning/charts-dashboards.md)         | chart rows and dashboard composition                                  |
|   [9]   | [visuals-offscreen](Visuals/.planning/visuals-offscreen.md)         | offscreen render, hash, export                                        |
|  [10]   | [theme-tokens](Theme/.planning/theme-tokens.md)                   | tokens, variants, density                                             |
|  [11]   | [typography-shaping](Typography/.planning/typography-shaping.md)       | typography, shaping, markdown                                         |
|  [12]   | [icons-assets](Assets/.planning/icons-assets.md)                   | icon sources and asset loading                                        |
|  [13]   | [dialogs-notifications](Dialogs/.planning/dialogs-notifications.md) | dialogs, sessions, notifications                                      |
|  [14]   | [input-interaction](Input/.planning/input-interaction.md)         | input rows, gestures, pan-zoom                                        |
|  [15]   | [motion-tokens](Motion/.planning/motion-tokens.md)                 | motion rows and phase mapping                                         |
|  [16]   | [accessibility](Access/.planning/accessibility.md)                 | automation, contrast, motion law                                      |
|  [17]   | [localization-culture](Localization/.planning/localization-culture.md)   | locale rows and culture composition                                   |
|  [18]   | [diagnostics-evidence](Evidence/.planning/diagnostics-evidence.md)   | evidence union, correlation, headless proof                           |
|  [19]   | [custom-visuals](Charts/.planning/custom-visuals.md)               | Skia diagram kinds and wide-gamut axis                                |
|  [20]   | [viewport-pipeline](Viewport/.planning/viewport-pipeline.md)         | GPU render graph, virtualized geometry, path trace, sim, viewpoint    |
|  [21]   | [drafting-sheets](Drafting/.planning/drafting-sheets.md)             | sheet set, title-block, 3D-to-2D projection, dimensioning, GD&T, emit |
|  [22]   | [notebook-document](Notebook/.planning/notebook-document.md)         | pinned-capability cells, dependency recompute, CRDT, replay           |
|  [23]   | [animation-timeline](Animation/.planning/animation-timeline.md)       | keyframe tracks, timeline playhead, scrub, walkthrough export         |

## [2]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. Catalogue keys omit the `api-` prefix and `.md` suffix. `[STATUS]` is one of `admitted`, `catalogue-pending`, `app-root-pending`. A `catalogue-pending` row names a transitive or unpinned surface that earns its catalogue at admission; the consuming-page row names where the surface lands.

| [INDEX] | [PACKAGE]                                   | [PAGE]                      | [CATALOGUE]           | [STATUS]          |
| :-----: | :------------------------------------------ | :-------------------------- | :-------------------- | :---------------- |
|   [1]   | Avalonia                                    | surface-hosts               | avalonia              | admitted          |
|   [2]   | Avalonia.Desktop                            | surface-hosts               | avalonia-desktop      | admitted          |
|   [3]   | Avalonia.Skia                               | visuals-offscreen           | avalonia-skia         | admitted          |
|   [4]   | Avalonia.Controls.ColorPicker               | inspector + accessibility   | avalonia-color        | admitted          |
|   [5]   | Avalonia.Controls.DataGrid                  | tables-hierarchy            | avalonia-grid         | admitted          |
|   [6]   | Avalonia.Fonts.Inter                        | typography-shaping          | avalonia-fonts        | admitted          |
|   [7]   | Avalonia.Themes.Fluent                      | theme-tokens                | avalonia-fluent       | admitted          |
|   [8]   | ReactiveUI                                  | shell + screens + commands  | reactiveui            | admitted          |
|   [9]   | ReactiveUI.Avalonia                         | surface-hosts + shell       | reactiveui-avalonia   | admitted          |
|  [10]   | ReactiveUI.Validation                       | screens + inspector         | reactiveui-validation | admitted          |
|  [11]   | Xaml.Behaviors.Avalonia                     | input + shell               | behaviors             | admitted          |
|  [12]   | System.Reactive                             | live-data + screens         | reactive              | admitted          |
|  [13]   | DynamicData                                 | live-data + tables          | dynamicdata           | admitted          |
|  [14]   | LiveChartsCore.SkiaSharpView.Avalonia       | charts-dashboards           | livecharts            | admitted          |
|  [15]   | SkiaSharp                                   | visuals + icons             | skiasharp             | admitted          |
|  [16]   | SkiaSharp.HarfBuzz                          | typography-shaping          | skia-harfbuzz         | admitted          |
|  [17]   | Svg.Controls.Skia.Avalonia                  | icons-assets                | svg-skia              | admitted          |
|  [18]   | SkiaSharp.NativeAssets.macOS                | surface-hosts               | skia-native           | admitted          |
|  [19]   | SkiaSharp.NativeAssets.Linux.NoDependencies | surface-hosts               | skia-native           | admitted          |
|  [20]   | SkiaSharp.NativeAssets.Linux                | surface-hosts               | skia-native           | admitted          |
|  [21]   | HarfBuzzSharp.NativeAssets.macOS            | surface-hosts               | harfbuzz-native       | admitted          |
|  [22]   | HarfBuzzSharp.NativeAssets.Linux            | surface-hosts               | harfbuzz-native       | admitted          |
|  [23]   | AsyncImageLoader.Avalonia                   | icons + visuals             | asyncimageloader      | admitted          |
|  [24]   | Avalonia.AvaloniaEdit                       | inspector-editing           | avaloniaedit          | admitted          |
|  [25]   | AvaloniaEdit.TextMate                       | inspector-editing           | avaloniaedit          | admitted          |
|  [26]   | bodong.Avalonia.PropertyGrid                | inspector-editing           | propertygrid          | admitted          |
|  [27]   | bodong.PropertyModels                       | inspector-editing           | propertygrid          | admitted          |
|  [28]   | DialogHost.Avalonia                         | dialogs-notifications       | dialoghost            | admitted          |
|  [29]   | Dock.Avalonia                               | shell-navigation            | dock                  | admitted          |
|  [30]   | Dock.Model.ReactiveUI                       | shell-navigation            | dock                  | admitted          |
|  [31]   | Dock.Serializer.SystemTextJson              | shell-navigation            | dock-serializer       | admitted          |
|  [32]   | PanAndZoom                                  | input + charts              | panandzoom            | admitted          |
|  [33]   | FluentIcons.Avalonia                        | icons-assets                | fluenticons           | admitted          |
|  [34]   | Markdig                                     | typography-shaping          | markdig               | admitted          |
|  [35]   | Thinktecture.Runtime.Extensions.Json        | commands + diagnostics      | docs/stacks/csharp    | admitted          |
|  [36]   | HotAvalonia                                 | diagnostics-evidence        | hotavalonia           | admitted          |
|  [37]   | Avalonia.Headless                           | diagnostics + accessibility | headless              | admitted          |
|  [38]   | Avalonia.Headless.XUnit                     | diagnostics-evidence        | headless              | admitted          |
|  [39]   | Verify.XunitV3                              | diagnostics-evidence        | headless              | catalogue-pending |
|  [40]   | FluentIcons.Common                          | icons-assets                | fluenticons           | catalogue-pending |
|  [41]   | TextMateSharp grammar registry              | inspector-editing           | —                     | catalogue-pending |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                     | [RAIL]                           | [EVIDENCE]                                   |
| :-----: | :------------------------- | :------------------------------- | :------------------------------------------- |
|  [G1]   | locked restore             | Assay restore rail               | lockfile regenerated; zero NU1004            |
|  [G2]   | API catalogue resolve      | `assay api` doctor/resolve       | fence members resolve in `.api`              |
|  [G3]   | static plan + build        | Assay static rail                | zero `': error '` lines on the AppUi closure |
|  [G4]   | spec law-matrix            | Assay test rail (AppUi target)   | law specs and ProofEngine matrix pass        |
|  [G5]   | host-seam bridge scenarios | Assay bridge rail                | live RhinoWIP scenario facts pass            |
|  [G6]   | headless render-hash lanes | Assay test rail (headless lanes) | `FrameHash` equality for named visual rows   |
|  [G7]   | page diagram render        | local mermaid-cli                | planning diagrams render locally             |
