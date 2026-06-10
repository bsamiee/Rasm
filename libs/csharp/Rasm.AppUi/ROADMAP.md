# [RASM_APPUI_ROADMAP]

This roadmap sequences AppUi as one product UI engine. Every retained UI capability enters through the same scheduler, screen, command, theme, typography, asset, diagnostic, lifecycle, and host-delegation rails.

## [1]-[FOUNDATION]

| [INDEX] | [WORK]                  | [EXIT_STATE]                                                                                             |
| :-----: | ----------------------- | -------------------------------------------------------------------------------------------------------- |
|   [1]   | Package graph           | AppUi package matrix restores/builds from versionless project references and central package pins        |
|   [2]   | Host references         | Rhino/GH2/Eto assemblies resolve through existing host-aware build owners                                |
|   [3]   | Scheduler spine         | Avalonia dispatcher, ReactiveUI scheduler, and host-thread affinity enter through one boundary           |
|   [4]   | Lifecycle               | Platform, scheduler, surface, binding, visibility, focus, scale, drain, and disposal states are explicit |
|   [5]   | Bootstrap               | Host startup configures Avalonia macOS options and creates the embeddable top level on the UI thread     |
|   [6]   | Native Skia boundary    | AppUi carries Skia/HarfBuzz native assets and records copied/loaded native identity proof                |
|   [7]   | Software rendering gate | Retained panel embedding is proven with software rendering before Metal                                  |

Foundation restore/build wiring is present. Source rails, host runtime proof, companion proof, sidecar proof, and downstream app proof are part of the same implementation contract.

## [2]-[CAPABILITY_RAILS]

| [INDEX] | [RAIL]      | [EXIT_STATE]                                                                                  |
| :-----: | ----------- | --------------------------------------------------------------------------------------------- |
|   [1]   | Shell       | Route identity, nav stack, panel composition, shell mode, visibility                          |
|   [2]   | Screen      | ReactiveUI activation, validation, scheduler-bound binding, screen projection                 |
|   [3]   | Command     | Command identity, icon key, availability, host lowering, command receipt                      |
|   [4]   | Live        | DynamicData-backed read-only projections, snapshots, selection state                          |
|   [5]   | Visual      | Thumbnail, preview, HUD intent, overlay intent, offscreen Skia surfaces                       |
|   [6]   | Chart       | LiveCharts2 series, axes, legends, gauges, dashboard interaction                              |
|   [7]   | Inspector   | Property grid, typed editor slots, validation projection, table/hierarchy inspection          |
|   [8]   | Theme       | Official Fluent theme, control themes, token catalogs, host theme synchronization             |
|   [9]   | Typography  | Font roles, embedded font collections, fallback mappings, numeric/code/log/symbol text policy |
|  [10]   | Assets      | Path icon catalog, SVG assets, provider-generated resources, custom asset registry            |
|  [11]   | Diagnostics | Embedding, focus, scale, disposal, screenshot, support evidence                               |

Rails are conceptual owners, not required filenames. New features deepen the owning rail before adding public surfaces.

## [3]-[INTEGRATION]

| [INDEX] | [SURFACE]                  | [PACKAGE_ROLE]                                                                      |
| :-----: | -------------------------- | ----------------------------------------------------------------------------------- |
|   [1]   | Retained UI root           | `Avalonia`, `Avalonia.Desktop`, embeddable top-level                                |
|   [2]   | Base visual system         | `Avalonia.Themes.Fluent` plus AppUi control-theme and token catalogs                |
|   [3]   | ViewModel rail             | `ReactiveUI`, `ReactiveUI.Avalonia`, `ReactiveUI.Validation`                        |
|   [4]   | Live projection            | `DynamicData` observed on the UI scheduler, public read-only snapshots              |
|   [5]   | Tables                     | `Avalonia.Controls.DataGrid`                                                        |
|   [6]   | Hierarchical tables        | AppUi hierarchical row projections over OSS DataGrid/virtualized presentation rails |
|   [7]   | Inspector                  | `bodong.Avalonia.PropertyGrid` integrated through the inspector rail                |
|   [8]   | Color/palette editing      | `Avalonia.Controls.ColorPicker`                                                     |
|   [9]   | Charts and dashboards      | `LiveChartsCore.SkiaSharpView.Avalonia`                                             |
|  [10]   | Custom retained visuals    | `SkiaSharp`, `Svg.Controls.Skia.Avalonia`, path/SVG catalogs                        |
|  [11]   | Text shaping               | `SkiaSharp.HarfBuzz`, `HarfBuzzSharp.NativeAssets.macOS`                            |
|  [12]   | Event triggers / drag-drop | `Xaml.Behaviors.Avalonia`                                                           |
|  [13]   | In-panel dialogs           | `DialogHost.Avalonia`                                                               |
|  [14]   | Icon assets                | Avalonia path/vector resources consumed through stable icon keys                    |

Command outcomes return typed receipts. Host execution lowers through `Rasm.Rhino/UI` and `Rasm.Grasshopper/UI`. The scheduler spine precedes every live projection and retained visual update.

## [4]-[CAPABILITY_BUILD]

| [INDEX] | [CAPABILITY]                | [BUILDS_ON]                                                                                          |
| :-----: | --------------------------- | ---------------------------------------------------------------------------------------------------- |
|   [1]   | Multi-screen navigation     | Shell route + nav stack + ReactiveUI activation                                                      |
|   [2]   | Command palette             | Command catalog + path icons + scoped key bindings                                                   |
|   [3]   | Shortcuts                   | `Xaml.Behaviors.Avalonia` per screen                                                                 |
|   [4]   | Live dashboards             | LiveCharts2 series observed on the UI scheduler                                                      |
|   [5]   | Tables and hierarchies      | DataGrid plus AppUi row models, grouping, expansion state, virtualization, and presentation adapters |
|   [6]   | Object inspection           | PropertyGrid integrated through the inspector rail                                                   |
|   [7]   | Settings / preferences      | Screen projection + inspector slots + Persistence store operation                                    |
|   [8]   | Notifications / toasts      | `DialogHost.Avalonia` transient in-panel host                                                        |
|   [9]   | Dialogs / confirmations     | `DialogHost.Avalonia` modal in-panel host                                                            |
|  [10]   | Color and palette editing   | ColorPicker + theme token updates                                                                    |
|  [11]   | Typography                  | Font roles, embedded font collections, fallback mappings, HarfBuzz shaping                           |
|  [12]   | Icon system                 | Path/SVG resource catalog with provider-generated and custom assets as peers                         |
|  [13]   | Thumbnail/offscreen visuals | SkiaSharp retained surfaces                                                                          |
|  [14]   | Viewport overlays           | Rhino/GH display conduit via visual intent                                                           |
|  [15]   | Undo-redo surfacing         | Host undo availability drives command availability                                                   |
|  [16]   | Drag-drop                   | Avalonia drag-drop + behaviors routed through host panel drops                                       |
|  [17]   | Clipboard                   | Avalonia clipboard surface injected at the screen boundary                                           |
|  [18]   | Accessibility               | Automation properties, keyboard reachability, custom automation peers                                |
|  [19]   | Diagnostics/support         | UI evidence + screenshot/support artifacts                                                           |

## [5]-[IMPLEMENTATION_DOCTRINE]

- AppUi is one rail, not independent wrapper systems for Avalonia, ReactiveUI, charts, icons, typography, dialogs, and host embedding.
- Public vocabulary is product vocabulary. Toolkit/provider types remain internal unless a host API requires them at a boundary.
- Icons, SVGs, binary assets, custom assets, and provider-generated assets flow through one source-neutral catalog.
- Theme, typography, asset, chart, inspector, and diagnostic variants are data-driven through tokens, roles, catalog rows, and folded receipts.
- Hierarchical table capability is OSS-first through row projection, grouping, expansion, virtualization, and DataGrid presentation adapters.
- Host viewport drawing is delegated to Rhino/GH display conduits. Retained AppUi rendering never claims viewport overlay ownership.
- Lifecycle evidence is required. Load, focus, scale, hidden, visible, drain, dispose, native asset load, and screenshot states are receipt-backed.
- Rhino panels, GH2 canvas/popup/component surfaces, companion windows, sidecar shells, and downstream apps are declared through shell/screen/command/live/theme/asset rails only. They do not compose toolkit/provider APIs directly.

## [6]-[HOST_PROOF]

| [INDEX] | [STEP]               | [REQUIREMENT]                                                                               |
| :-----: | -------------------- | ------------------------------------------------------------------------------------------- |
|   [1]   | Software rendering   | Retained panel loads and renders before Metal path                                          |
|   [2]   | NSView reparenting   | Avalonia platform handle is parented inside the Eto/Rhino panel native handle               |
|   [3]   | Focus coordination   | Hidden panels resign first responder; shown panels restore responder state                  |
|   [4]   | Retina scale refresh | Backing-property and activation events refresh scale                                        |
|   [5]   | Disposal order       | Content detach -> retained surface closed -> host panel dispose                             |
|   [6]   | Native assets        | SkiaSharp and HarfBuzz native assets load without duplicate-major host collision            |
|   [7]   | GPU coexistence      | Retained panel, chart, SVG, and viewport rendering coexist with Rhino viewport frame pacing |
|   [8]   | GH2 surfaces         | Toolbars, popups, component panels, canvas hooks, and overlays route through GH2 UI owners  |
|   [9]   | Accessibility        | Interactive controls expose names/help text and keyboard paths                              |
|  [10]   | Screenshots          | Retained panel and viewport overlay captures are recorded separately                        |

## [7]-[COMPLETION_GATES]

| [INDEX] | [GATE]                 | [EXIT_STATE]                                                                                     |
| :-----: | ---------------------- | ------------------------------------------------------------------------------------------------ |
|   [1]   | Source rails           | All owner rails expose product concepts instead of toolkit/provider public surfaces              |
|   [2]   | Package graph          | No deprecated, vulnerable, stale, paid/pro, or license-key AppUi package remains active          |
|   [3]   | Native boundary        | Skia/HarfBuzz native load path is recorded in UI evidence                                        |
|   [4]   | Runtime proof          | RhinoWIP and GH2 scenarios record host, focus, scale, disposal, screenshot, and support evidence |
|   [5]   | Visual proof           | Retained panels and viewport overlays have separate image evidence                               |
|   [6]   | Accessibility proof    | Keyboard and automation surfaces are covered by managed specs and runtime scenarios              |
|   [7]   | Downstream UI creation | A downstream screen can be declared through shell/screen/command/live/theme/asset rails only     |
