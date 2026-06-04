# [H1][RASM_APPUI_AGENTS]

[CRITICAL] Build `Rasm.AppUi` as one unified, fully-integrated platform. The `.csproj` targets `net10.0`, is present in `Workspace.slnx`, and consumes central package pins through versionless references. Scaffold source surfaces in Phase 0 before heavy work. Integrate every package together, never as separate per-package subsystems.

## [1][OWNER_CONTRACT]

- One typed app-surface operation rail owns shell, screen, command, live-view, chart, visual, and diagnostic concerns.
- Keep Avalonia, ReactiveUI, DynamicData, SkiaSharp, and LiveCharts2 types internal behind product concepts.
- Integrate `ReactiveUI.Avalonia` as the ViewModel/Avalonia adapter; `ReactiveUI.Validation` owns the `Screen` validation surface.
- LiveCharts2 owns charts and dashboards on SkiaSharp; AppUi SkiaSharp is thumbnails and offscreen draw only — viewport-overlay SkiaSharp lives in the Rhino/GH display conduit.
- Land `RasmUiScheduler` (one canonical UI scheduler boundary: Avalonia `Dispatcher` + ReactiveUI `RxApp.MainThreadScheduler`) first; observe DynamicData change-sets on it before binding — off-thread observation yields cross-thread mutation.
- Bootstrap order: `PlugIn.OnLoad` constructs `RasmUiScheduler` on the UI thread, passes it into `AppHost.Boot(token, timeProvider, uiScheduler, …)`, receives the runtime record and drain handle, then hands the record to AppUi to activate inbound observables.
- Embed Avalonia as child content of a host `RasmPanel` via `CreateEmbeddableTopLevel()` — never `CreateEmbeddableWindow` (unimplemented on macOS), never a floating `NSWindow`.
- Set `MacOSPlatformOptions.DisableAvaloniaAppDelegate = true` before any Avalonia surface initializes.
- NSView reparenting: isolated P/Invoke shim — `objc_msgSend` over `TryGetPlatformHandle()` `IntPtr` + Eto native handle; TFM stays `net10.0`.
- Validate embedding with Software rendering before Metal.
- On `PanelHidden`: set `Content = null`, resign first responder.
- On `PanelShown`: restore content, return first responder. Eto/Avalonia focus is not auto-coordinated.
- Refresh scale on `NSWindowDidChangeBackingProperties` and `WhenActivated`.
- Disposal: `Content = null` → await `TopLevel.Closed` → base dispose.
- Inbound contracts (`IObservable<AppState>`, `IObservable<ComputeProgress>`, AppHost scheduling) are typed and parameterized, built fully now and fired when the sibling lands; no sibling is required to exist first.
- Compose the packages into one paradigm; no parallel rails or per-package folders.
- `DiagnosticReceipt` is AppUi-owned; AppHost references/correlates but does not define it.

## [2][HOST_BOUNDARY]

| [INDEX] | [DO_NOT_DUPLICATE]                                          | [OWNER]               |
| :-----: | ----------------------------------------------------------- | --------------------- |
|   [1]   | UI-thread dispatch, parent windows, Rhino document affinity | `Rasm.Rhino/UI`       |
|   [2]   | Rhino panels, window specs, repaint, undo grouping          | `Rasm.Rhino/UI`       |
|   [3]   | GH2 canvas/editor/document scope                            | `Rasm.Grasshopper/UI` |
|   [4]   | GH2 subscriptions, repaint requests, paint hooks            | `Rasm.Grasshopper/UI` |
|   [5]   | Runtime scheduling, drain, telemetry correlation            | `Rasm.AppHost`        |
|   [6]   | Durable state, migrations, support artifacts                | `Rasm.Persistence`    |

AppUi emits product UI intent and receipts. Final mutation runs through the owner rails above.

## [3][EMBEDDING_RULES]

| [INDEX] | [RULE]                                          | [SEVERITY]   | [DETAIL]                                                                                   |
| :-----: | ----------------------------------------------- | ------------ | ------------------------------------------------------------------------------------------ |
|   [1]   | `DisableAvaloniaAppDelegate = true`             | [CRITICAL]   | Must be set before any Avalonia surface initializes; omission crashes Rhino on load        |
|   [2]   | Use `CreateEmbeddableTopLevel()`                | [CRITICAL]   | `CreateEmbeddableWindow` is unimplemented on macOS; call will throw                        |
|   [3]   | TFM = `net10.0`, P/Invoke NSView shim           | [CRITICAL]   | Do not use `net10.0-macos`; reparenting is an isolated `objc_msgSend` shim                |
|   [4]   | Software rendering first                        | [IMPORTANT]  | Validate before Metal; Metal + Rhino Metal queue coexistence is unproven                  |
|   [5]   | Embed as child NSView, never own `NSWindow`     | [CRITICAL]   | Independent `NSWindow` competes for the macOS Metal command queue                         |
|   [6]   | Resign first responder on `PanelHidden`         | [CRITICAL]   | Failing leaves keyboard routing broken after panel reopens                                 |
|   [7]   | Restore first responder on `PanelShown`         | [IMPORTANT]  | Eto/Avalonia focus not auto-coordinated                                                    |
|   [8]   | Refresh scale on backing-properties change      | [IMPORTANT]  | Retina scale changes on external display connection                                        |
|   [9]   | `Content = null` before Eto dispose             | [CRITICAL]   | ReactiveUI `WhenActivated` subscriptions leak until finalization if skipped                |
|  [10]   | Await `TopLevel.Closed` before base dispose     | [CRITICAL]   | Disposing Eto parent before Avalonia TopLevel closes causes native handle double-free      |
|  [11]   | GH2 embedding is [DEFERRED]                     | [IMPORTANT]  | No GH2 dockable panel-host API in current RhinoWIP; Rhino-panel embedding is supported. Trigger: `api query gh2 Panel` per WIP drop |

## [4][PACKAGE_RULES]

- Keep AppUi package references versionless; write only NuGet IDs in docs — no version numbers.
- `SkiaSharp.NativeAssets.macOS`: once the Phase-0 gate (`_ARCHITECTURE.md §4.3`) confirms Rhino's `libSkiaSharp` native major matches, reference with `<ExcludeAssets>native</ExcludeAssets>` to share Rhino's loaded copy; a mismatched major is a hard build gate — same-named dylibs cannot co-load.
- `HarfBuzzSharp.NativeAssets.macOS`: carry unconditionally — not bundled by Rhino.
- Unify SkiaSharp version across Avalonia bundled, LiveCharts2, and `Svg.Skia` — version mismatch = native symbol collision at load.
- Use `Xaml.Behaviors.Avalonia` (not `Avalonia.Xaml.Interactions` — deprecated).

## [5][EVIDENCE]

Executable proof comes from source and host scenarios. Evidence categories:

- RhinoWIP macOS load and unload.
- GH2 editor/canvas coexistence [DEFERRED] — no GH2 plugin-panel host API in current RhinoWIP.
- Focus, keyboard, z-order, parent identity.
- Retina/logical-pixel scaling.
- GPU/frame-pacing coexistence with the viewport.
- Native asset layout and disposal.
- Screenshot/support-bundle `DiagnosticReceipt`.
- Accessibility automation peer verification.

## [6][REJECTIONS]

- No Avalonia, ReactiveUI, DynamicData, SkiaSharp, or LiveCharts2 forwarding surface.
- No AppUi-owned Rhino/GH2 dispatch, repaint, undo, or lifecycle rail.
- No Avalonia rendering over the native viewport; viewport overlays delegate to the Rhino/GH display conduit.
- No viewport-overlay SkiaSharp in AppUi; it lives in the Rhino/GH display conduit.
- No mixed MVVM idioms in one screen stack.
- No per-package subsystem; the packages compose into one rail.
- No `FluentAvaloniaUI` (Avalonia 11), `Material.Avalonia` (theme takeover), `Dock.Avalonia` (floats NSWindow), `Avalonia.Controls.TreeDataGrid` (Avalonia 11 + commercial), `ScottPlot.Avalonia` (Avalonia 11 + duplicates LiveCharts2), `MessageBox.Avalonia` (nightly dep), `Avalonia.Xaml.Interactions` (deprecated).
- No version numbers in doc text; IDs only — versions live in `Directory.Packages.props`.
