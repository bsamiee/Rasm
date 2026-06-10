# [RASM_APPUI_ROADMAP]

This roadmap sequences the AppUi build as one product UI engine. Every retained UI capability enters through the same scheduler, screen, command, theme, typography, asset, diagnostic, and host-delegation rails.

## [1]-[FOUNDATION]

| [INDEX] | [WORK]                  | [EXIT_STATE]                                                                                         |
| :-----: | ----------------------- | ---------------------------------------------------------------------------------------------------- |
|   [1]   | Package graph           | AppUi package matrix restores/builds from versionless project references and central package pins     |
|   [2]   | Host references         | Rhino/GH2/Eto assemblies resolve through existing host-aware build owners                             |
|   [3]   | Scheduler spine         | `RasmUiScheduler` owns Avalonia dispatcher, ReactiveUI scheduler, and host-thread affinity            |
|   [4]   | Bootstrap               | `PlugIn.OnLoad` sets Avalonia macOS options and creates the embeddable top level on the UI thread     |
|   [5]   | Native Skia boundary    | AppUi Skia path either shares the host-compatible native major or leaves the in-process host path      |
|   [6]   | Software rendering gate | Retained panel embedding is proven with software rendering before Metal                               |

Foundation restore/build wiring is present. Source rails and host runtime proof remain.

## [2]-[SOURCE_RAILS]

Build these owner files as the public shape of the engine:

| [INDEX] | [FILE]          | [RAIL]                                                                                       |
| :-----: | --------------- | -------------------------------------------------------------------------------------------- |
|   [1]   | `Shell.cs`      | Route identity, nav stack, panel composition, shell mode, visibility                         |
|   [2]   | `Screen.cs`     | ReactiveUI activation, validation, scheduler-bound binding, screen projection                |
|   [3]   | `Command.cs`    | Command identity, icon key, `CanExecute`, host lowering, `CommandReceipt`                    |
|   [4]   | `Live.cs`       | DynamicData-backed read-only projections, snapshots, selection state                         |
|   [5]   | `Visual.cs`     | Thumbnail, preview, HUD intent, overlay intent, offscreen Skia surfaces                      |
|   [6]   | `Chart.cs`      | LiveCharts2 series, axes, legends, gauges, dashboard interaction                             |
|   [7]   | `Inspector.cs`  | Property grid, typed editor slots, validation projection, table/tree inspection              |
|   [8]   | `Theme.cs`      | Official Fluent theme, control themes, token catalogs, host theme synchronization            |
|   [9]   | `Typography.cs` | Font roles, embedded font collections, fallback mappings, numeric/code/log text policy       |
|  [10]   | `Assets.cs`     | Path icon catalog, SVG assets, provider-generated resources, custom asset registry           |
|  [11]   | `Diagnostic.cs` | Embedding, focus, scale, disposal, screenshot, support evidence                              |

## [3]-[INTEGRATION]

| [INDEX] | [SURFACE]                  | [PACKAGE_ROLE]                                                                                       |
| :-----: | -------------------------- | ---------------------------------------------------------------------------------------------------- |
|   [1]   | Retained UI root           | `Avalonia`, `Avalonia.Desktop`, `CreateEmbeddableTopLevel()`                                         |
|   [2]   | Base visual system         | `Avalonia.Themes.Fluent` plus AppUi control-theme and token catalogs                                 |
|   [3]   | ViewModel rail             | `ReactiveUI`, `ReactiveUI.Avalonia`, `ReactiveUI.Validation`                                         |
|   [4]   | Live projection            | `DynamicData` observed on `RasmUiScheduler`, public read-only snapshots                              |
|   [5]   | Tables                     | `Avalonia.Controls.DataGrid`                                                                         |
|   [6]   | Hierarchical tables        | `Avalonia.Controls.TreeDataGrid`                                                                     |
|   [7]   | Inspector                  | `bodong.Avalonia.PropertyGrid` integrated through `Inspector<T>`                                     |
|   [8]   | Color/palette editing      | `Avalonia.Controls.ColorPicker`                                                                      |
|   [9]   | Charts and dashboards      | `LiveChartsCore.SkiaSharpView.Avalonia`                                                             |
|  [10]   | Custom retained visuals    | `SkiaSharp`, `Svg.Controls.Skia.Avalonia`, path/SVG catalogs                                         |
|  [11]   | Text shaping               | `SkiaSharp.HarfBuzz`, `HarfBuzzSharp.NativeAssets.macOS`                                            |
|  [12]   | Event triggers / drag-drop | `Xaml.Behaviors.Avalonia`                                                                            |
|  [13]   | In-panel dialogs           | `DialogHost.Avalonia`                                                                                |
|  [14]   | Icon assets                | Avalonia `PathIcon` and generated path/SVG resources consumed through `IconKey`                      |

Command outcomes return `CommandReceipt`. Host execution lowers through `Rasm.Rhino/UI` and `Rasm.Grasshopper/UI`. The scheduler spine precedes every live projection and retained visual update.

## [4]-[CAPABILITY_BUILD]

| [INDEX] | [CAPABILITY]                | [BUILDS_ON]                                                                                          |
| :-----: | --------------------------- | ---------------------------------------------------------------------------------------------------- |
|   [1]   | Multi-screen navigation     | `Shell` route + nav stack + ReactiveUI activation                                                     |
|   [2]   | Command palette             | `Command` catalog + path icons + scoped key bindings                                                  |
|   [3]   | Shortcuts                   | `Xaml.Behaviors.Avalonia` per `Screen`                                                               |
|   [4]   | Live dashboards             | LiveCharts2 series observed on `RasmUiScheduler`                                                     |
|   [5]   | Tables and tree tables      | DataGrid and TreeDataGrid surfaces under AppUi selection/sort/filter state                           |
|   [6]   | Object inspection           | PropertyGrid integrated through `Inspector<T>`                                                        |
|   [7]   | Settings / preferences      | `Screen<PreferencesModel>` + inspector slots + Persistence store operation                           |
|   [8]   | Notifications / toasts      | `DialogHost.Avalonia` transient in-panel host                                                        |
|   [9]   | Dialogs / confirmations     | `DialogHost.Avalonia` modal in-panel host                                                            |
|  [10]   | Color and palette editing   | ColorPicker + theme token updates                                                                    |
|  [11]   | Typography                  | Font roles, embedded font collections, fallback mappings, HarfBuzz shaping                           |
|  [12]   | Icon system                 | Path/SVG resource catalog with Fluent UI System Icons as primary provider and custom assets as peers |
|  [13]   | Thumbnail/offscreen visuals | SkiaSharp retained surfaces                                                                          |
|  [14]   | Viewport overlays           | Rhino/GH display conduit via `VisualRequest`                                                         |
|  [15]   | Undo-redo surfacing         | Host undo availability -> `Command.CanExecute`                                                       |
|  [16]   | Drag-drop                   | Avalonia drag-drop + behaviors routed through host panel drops                                       |
|  [17]   | Clipboard                   | Avalonia `IClipboard` injected into `Screen`                                                         |
|  [18]   | Accessibility               | Automation properties, keyboard reachability, custom automation peers                                |
|  [19]   | Diagnostics/support         | `DiagnosticReceipt` + screenshot/support artifacts                                                   |

## [5]-[HOST_PROOF]

| [INDEX] | [STEP]               | [REQUIREMENT]                                                                                     |
| :-----: | -------------------- | ------------------------------------------------------------------------------------------------- |
|   [1]   | Software rendering   | Retained panel loads and renders before Metal path                                                 |
|   [2]   | NSView reparenting   | Avalonia platform handle is parented inside the Eto/Rhino panel native handle                      |
|   [3]   | Focus coordination   | `PanelHidden` resigns first responder; `PanelShown` restores responder state                       |
|   [4]   | Retina scale refresh | `NSWindowDidChangeBackingProperties` and activation refresh scale                                  |
|   [5]   | Disposal order       | Content detach -> `TopLevel.Closed` -> base dispose                                                |
|   [6]   | Native assets        | SkiaSharp and HarfBuzz native assets load without duplicate-major host collision                   |
|   [7]   | GPU coexistence      | Retained panel, chart, SVG, and viewport rendering coexist with Rhino viewport frame pacing        |
|   [8]   | GH2 surfaces         | Toolbars, popups, component panels, canvas hooks, and overlays route through GH2 UI owners         |
|   [9]   | Accessibility        | Interactive controls expose names/help text and keyboard paths                                     |
|  [10]   | Screenshots          | Retained panel and viewport overlay captures are recorded separately                               |

## [6]-[COMPLETION_GATES]

| [INDEX] | [GATE]                  | [EXIT_STATE]                                                                                   |
| :-----: | ----------------------- | ---------------------------------------------------------------------------------------------- |
|   [1]   | Source rails            | All owner files compile and expose product concepts instead of toolkit/provider public surfaces |
|   [2]   | Package graph           | No deprecated, vulnerable, or stale AppUi package remains in the active graph                   |
|   [3]   | Native boundary         | Skia/HarfBuzz native load path is recorded in `DiagnosticReceipt`                               |
|   [4]   | Runtime proof           | RhinoWIP and GH2 scenarios record host, focus, scale, disposal, screenshot, and support evidence |
|   [5]   | Visual proof            | Retained panels and viewport overlays have separate image evidence                              |
|   [6]   | Accessibility proof     | Keyboard and automation surfaces are covered by managed specs and runtime scenarios             |
|   [7]   | Downstream UI creation  | A downstream screen can be declared through shell/screen/command/live/theme/asset rails only    |
