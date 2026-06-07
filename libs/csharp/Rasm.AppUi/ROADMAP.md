# [RASM_APPUI_ROADMAP]

This roadmap sequences the build. The platform integrates all packages together from the foundation; it does not drip capability in behind separate consumers.

## [1]-[PHASE_0]

- Use the existing root `Directory.Packages.props` AppUi matrix. `PackageReference` entries stay versionless. Do not repin or add package-manager aliases in this pass. Coupled version matrix: Avalonia ↔ ReactiveUI.Avalonia ↔ ReactiveUI ↔ DynamicData ↔ System.Reactive ↔ SkiaSharp (Avalonia-bundled, LiveCharts2-aligned). Target `net10.0` so System.Reactive pulls no WPF/WinForms facade.
- SkiaSharp gate (§4.3): confirm Rhino's bundled `libSkiaSharp` native major. If it matches, reference `SkiaSharp.NativeAssets.macOS` with `<ExcludeAssets>native</ExcludeAssets>` to share Rhino's loaded copy; a mismatched major is a hard build gate (same-named dylibs cannot co-load). Carry `HarfBuzzSharp.NativeAssets.macOS` unconditionally (not bundled by Rhino).
- `Rasm.AppUi.csproj` targets `net10.0`, is wired into `Workspace.slnx`, restores/builds, and resolves host assemblies as the sibling libs do.
- Scaffold the unified folder skeleton and canonical section order: `Shell.cs`, `Screen.cs`, `Command.cs`, `Live.cs`, `Visual.cs`, `Chart.cs`, `Diagnostic.cs`.
- Land `RasmUiScheduler` (Avalonia `Dispatcher` + ReactiveUI `RxApp.MainThreadScheduler`) — the one canonical UI scheduler boundary — before any live-projection work.
- Set `MacOSPlatformOptions.DisableAvaloniaAppDelegate = true` in the bootstrap; use `CreateEmbeddableTopLevel()`.
- Validate Avalonia embedding with Software rendering before Metal.

Phase 0 restore/build foundation is complete. Source scaffolding and runtime embedding proof remain.

## [2]-[INTEGRATION]

Build the single typed app-surface rail and compose the packages into one paradigm:

| [INDEX] | [SURFACE]                  | [PACKAGE_ROLE]                                                                                            |
| :-----: | -------------------------- | --------------------------------------------------------------------------------------------------------- |
|   [1]   | Scheduler spine            | `RasmUiScheduler`: Avalonia `Dispatcher` + ReactiveUI scheduler + host-thread affinity                    |
|   [2]   | Shell and screen rail      | Avalonia retained surface, ReactiveUI activation and commands, `ReactiveUI.Validation` Screen surface     |
|   [3]   | ViewModel adapter          | `ReactiveUI.Avalonia`                                                                                     |
|   [4]   | Live projection            | DynamicData change-sets, observed on `RasmUiScheduler`, read-only projection                              |
|   [5]   | Custom visuals             | SkiaSharp thumbnails/offscreen only; viewport overlays via the Rhino/GH display conduit                   |
|   [6]   | Charts and dashboards      | `LiveChartsCore.SkiaSharpView.Avalonia` retained data-viz on SkiaSharp                                    |
|   [7]   | Tabular surfaces           | `Avalonia.Controls.DataGrid`                                                                              |
|   [8]   | Inspector surfaces         | `bodong.Avalonia.PropertyGrid`; custom editors remain the documented fallback (single-maintainer package) |
|   [9]   | Color surfaces             | `Avalonia.Controls.ColorPicker`                                                                           |
|  [10]   | Event triggers / drag-drop | `Xaml.Behaviors.Avalonia`                                                                                 |
|  [11]   | Icon glyphs                | `Projektanker.Icons.Avalonia` + `Projektanker.Icons.Avalonia.MaterialDesign`                              |
|  [12]   | SVG icons / assets         | `Svg.Controls.Skia.Avalonia`                                                                              |
|  [13]   | In-panel dialogs           | `DialogHost.Avalonia` — no NSWindow                                                                       |
|  [14]   | Text shaping               | `SkiaSharp.HarfBuzz` + `HarfBuzzSharp.NativeAssets.macOS`                                                 |

Command outcomes return `CommandReceipt`. Host execution lowers through `Rasm.Rhino/UI` and `Rasm.Grasshopper/UI`. The scheduler spine precedes live projection; DynamicData change-sets observe on `RasmUiScheduler` before binding.

## [3]-[HIGHER_ORDER]

On the integrated foundation, build the higher-order product-UI capabilities a plugin needs — each as parameterized capability on the one rail, not a parallel subsystem:

| [INDEX] | [CAPABILITY]                | [BUILDS_ON]                                                                              |
| :-----: | --------------------------- | ---------------------------------------------------------------------------------------- |
|   [1]   | Multi-screen navigation     | `Shell` route + `NavStack`; `ReactiveUI.Avalonia` routing                                |
|   [2]   | Command palette / shortcuts | `Xaml.Behaviors.Avalonia` key-binding; scoped per `Screen`                               |
|   [3]   | Live dashboards             | `LiveChartsCore.SkiaSharpView.Avalonia`; series updates on `RasmUiScheduler`             |
|   [4]   | Viewport overlays           | Delegated to Rhino/GH display conduit — NOT Avalonia or AppUi SkiaSharp                  |
|   [5]   | Settings / preferences      | `Screen<PreferencesModel>` + custom editor surface; persisted via Persistence            |
|   [6]   | Notifications / toasts      | `DialogHost.Avalonia` in-panel host; no NSWindow                                         |
|   [7]   | Undo-redo surfacing         | Observe host undo availability from `Rasm.Rhino/UI` rail; `CanExecute` on `Command`      |
|   [8]   | Theming                     | `HostUtils.RunningInDarkMode` + reactive `ThemeVariant` on `WhenActivated`               |
|   [9]   | Drag-drop                   | Avalonia drag-drop + `Xaml.Behaviors.Avalonia`; routed through Eto for host-panel drops  |
|  [10]   | Clipboard                   | Avalonia `IClipboard` injected into `Screen`; no static access                           |
|  [11]   | Accessibility               | `AutomationProperties` on every interactive control; `AutomationPeer` for custom visuals |

## [4]-[EMBEDDING_VALIDATION]

| [INDEX] | [STEP]               | [REQUIREMENT]                                                                                                    |
| :-----: | -------------------- | ---------------------------------------------------------------------------------------------------------------- |
|   [1]   | Software rendering   | Validate embedding with Software rendering before Metal path                                                     |
|   [2]   | NSView reparenting   | P/Invoke `objc_msgSend` shim; verify native handle via `TryGetPlatformHandle()`                                  |
|   [3]   | Focus coordination   | Resign first responder on `PanelHidden`; restore on `PanelShown`                                                 |
|   [4]   | Retina scale refresh | Handle `NSWindowDidChangeBackingProperties` + `WhenActivated`                                                    |
|   [5]   | Disposal order       | `Content = null` → await `TopLevel.Closed` → base dispose                                                        |
|   [6]   | GH2 embedding        | Extension point — Rhino-panel embedding is supported; GH2 retained embedding waits for a dockable panel-host API |

## [5]-[RUNTIME_EVIDENCE]

Runtime claims are scoped to proven host scenarios. Owner-local `DiagnosticReceipt` records identify the host, parent handle, focus behavior, command result, disposal path, GPU/frame-pacing coexistence with the viewport, and screenshot/support evidence for RhinoWIP macOS load and GH2 coexistence.

## [6]-[OPEN_WORK]

| [INDEX] | [WORK]                  | [EXIT_STATE]                                                                                                              |
| :-----: | ----------------------- | ------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | Source scaffold         | `Shell`, `Screen`, `Command`, `Live`, `Visual`, `Chart`, and `Diagnostic` rails exist as cohesive AppUi owners            |
|   [2]   | Native Skia boundary    | AppUi either shares a host-compatible native Skia path or routes Skia rendering outside the in-process host path          |
|   [3]   | Embedding runtime proof | `DiagnosticReceipt` records NSView parent, focus, Retina scale, disposal order, and screenshot/support evidence           |
|   [4]   | GH2 retained embedding  | A GH2 dockable panel-host API exists and the GH2 UI rail owns the host extension before AppUi adds a retained GH2 surface |
