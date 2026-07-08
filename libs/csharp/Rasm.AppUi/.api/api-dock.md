# [RASM_APPUI_API_DOCK]

`Dock.Avalonia` is the Avalonia control layer (dock visual tree, host windows, drop targets, selector/overlay surfaces, MDI, command bars, managed float layer) and `Dock.Model.ReactiveUI` is the ReactiveUI binding of the host-neutral `Dock.Model` core: both depend on `Dock.Model`, where the real contract surface lives — `IFactory` owns layout construction AND the docking operations (add/insert/move/swap/float/pin/close), `IDock`/`IDockable` the model graph, `IDockManager` the drag/drop policy, and `IDockState` the persistence handshake the `Dock.Serializer.SystemTextJson` (`api-dock-serializer.md`) reads/writes. `DockControl.Layout` binds an `IDock` graph through a `Factory`; the visual tree, drag targets, floating `HostWindow`s, and overlays mutate that graph through the factory operations, never by hand-built splitter/tab arrangement.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Dock.Avalonia`
- package: `Dock.Avalonia`
- license: MIT (expression)
- assembly: `Dock.Avalonia`
- namespace: `Dock.Avalonia.Controls` (dock visual tree, host windows, targets, overlays, managed-window layer, MDI controls)
- namespace: `Dock.Avalonia.Selectors` (`DockSelectorItem`)
- namespace: `Dock.Avalonia.Themes` (`DockPresetThemeManagerBase`)
- namespace: `Dock.Avalonia.Mdi` (`ClassicMdiLayoutManager`)
- namespace: `Dock.Avalonia.CommandBars` (`DockCommandBarManager`, `DefaultDockCommandBarAdapter`)
- namespace: `Dock.Avalonia.Services` (`DockControlFactoryService`)
- target: `lib/net10.0` (bound) + `lib/net8.0`
- asset: runtime library
- depends: `Dock.Model`, `Dock.Settings`, `Dock.Controls.DeferredContentControl`, `Dock.Controls.ProportionalStackPanel`, `Dock.Controls.Recycling`, `Dock.MarkupExtension`
- rail: docking

[PACKAGE_SURFACE]: `Dock.Model.ReactiveUI`
- package: `Dock.Model.ReactiveUI`
- license: MIT (expression)
- assembly: `Dock.Model.ReactiveUI`
- namespace: `Dock.Model.ReactiveUI` (`Factory`)
- namespace: `Dock.Model.ReactiveUI.Core` (`DockBase`, `DockableBase`, `DockWindow`)
- namespace: `Dock.Model.ReactiveUI.Controls` (`RootDock`, `ProportionalDock`, `DocumentDock`, `ToolDock`, `Document`, `Tool`, ...)
- namespace: `Dock.Model.ReactiveUI.Services` (`IDockDispatcher`)
- target: `lib/net10.0` (bound) + `lib/net8.0` + `lib/net6.0`
- asset: runtime library
- depends: `Dock.Model` (host-neutral contracts), `reactiveui`
- rail: docking

> The host-neutral contracts (`IFactory`, `IDock`, `IDockable`, `IDockManager`, `IDockState`, `IHostWindow`, the `Dock.Model.Controls` interfaces, and the `Dock.Model.Core` enums below) live in the transitively-restored `Dock.Model` core assembly, not in `Dock.Avalonia` or `Dock.Model.ReactiveUI`.

## [02]-[PUBLIC_TYPES]

[DOCK_CONTROLS]: `Dock.Avalonia.Controls` visual tree
- rail: docking

| [INDEX] | [SYMBOL]                  | [RAIL]                          |
| :-----: | :------------------------ | :------------------------------ |
|  [01]   | `DockControl`             | dock root control (binds graph) |
|  [02]   | `RootDockControl`         | root view                       |
|  [03]   | `ProportionalDockControl` | split view                      |
|  [04]   | `DocumentDockControl`     | document dock                   |
|  [05]   | `ToolDockControl`         | tool dock                       |
|  [06]   | `DocumentControl`         | document host                   |
|  [07]   | `ToolControl`             | tool host                       |
|  [08]   | `DocumentTabStrip`        | document tabs                   |
|  [09]   | `ToolTabStrip` / `ToolTabStripItem` | tool tabs             |
|  [10]   | `ToolChromeControl`       | tool chrome                     |
|  [11]   | `PinnedDockControl` / `PinnedDockHostPanel` | pinned dock + host |
|  [12]   | `ToolPinnedControl`       | pinned tools                    |

[WINDOW_TARGET_OVERLAY_TYPES]: floating windows, managed-float layer, drop targets, selectors, overlays, MDI
- rail: docking

| [INDEX] | [SYMBOL]                                        | [RAIL]                          |
| :-----: | :---------------------------------------------- | :------------------------------ |
|  [01]   | `HostWindow` / `HostWindowTitleBar`             | OS floating host + chrome       |
|  [02]   | `ManagedHostWindow` / `ManagedWindowLayer` / `ManagedWindowDock` / `ManagedDockWindowDocument` | in-window managed float layer |
|  [03]   | `DockTarget` / `GlobalDockTarget`               | local + global drop target      |
|  [04]   | `DockableControl`                               | dockable region                 |
|  [05]   | `DragPreviewControl`                            | drag preview                    |
|  [06]   | `DockSelectorOverlay` / `DockSelectorItem`      | selector overlay + item         |
|  [07]   | `OverlayHost` / `DialogOverlayControl` / `ConfirmationOverlayControl` / `BusyOverlayControl` | overlay layer (dialog/confirm/busy) |
|  [08]   | `OverlayLayerRegistry` / `OverlayLayerCollection` | overlay layer registry        |
|  [09]   | `MdiDocumentControl` / `MdiDocumentWindow` / `ClassicMdiLayoutManager` | MDI surface + classic layout |
|  [10]   | `DockPresetThemeManagerBase`                    | theme preset manager base       |
|  [11]   | `DockCommandBarManager` / `DefaultDockCommandBarAdapter` | command-bar merge surface |
|  [12]   | `DockControlFactoryService`                     | control-to-factory wiring service |
|  [13]   | `IDockThemeManager` (`Dock.Avalonia.Themes`)    | dock theme-manager contract bound at composition (the `Factory` override exposes a `Func<IDockThemeManager>`); the theme-variant subscription flips dock-owned brushes |
|  [14]   | `IExternalDockSurface` (`DockControl? DockControl { get; set; }` / `Control SurfaceControl { get; }`) | embedded external-dock surface contract `DockControl.RegisterExternalDockSurface`/`UnregisterExternalDockSurface` attach/detach |
|  [15]   | `DockSelectorMode` (`Dock.Avalonia.Selectors`)  | `Documents`/`Tools`/`All` selector-overlay scope enum (`DockControl.ShowSelector` argument) |

[MODEL_TYPES]: `Dock.Model.ReactiveUI` graph + `Dock.Model.Core`/`Controls` contracts
- rail: docking

| [INDEX] | [SYMBOL]                       | [RAIL]                                 |
| :-----: | :----------------------------- | :------------------------------------- |
|  [01]   | `Factory : FactoryBase`        | model factory (construct + operate)    |
|  [02]   | `DockableBase` / `DockBase`    | dockable + dock base (ReactiveObject)  |
|  [03]   | `RootDock` / `ProportionalDock` / `ProportionalDockSplitter` | root + split + splitter model |
|  [04]   | `DocumentDock` / `ToolDock`    | document + tool dock model             |
|  [05]   | `Document` / `Tool`            | document + tool model                  |
|  [06]   | `DockWindow`                   | float-window model                     |
|  [07]   | `IFactory`                     | construction + operation contract      |
|  [08]   | `IDock` / `IDockable`          | dock graph node + leaf contract        |
|  [09]   | `IRootDock` / `IDocumentDock` / `IToolDock` / `IProportionalDock` | typed dock contracts |
|  [10]   | `IDockManager` / `IDockManagerState` | `ValidateTool`/`…Document`/`…Dock`/`…Dockable`(src, tgt, `DragAction`, `DockOperation`, exec) + `IsDockTargetVisible` |
|  [11]   | `IDockState`                   | `Save(IDock)` / `Restore(IDock)` / `Reset()` snapshot |
|  [12]   | `IDockSerializer`              | `Serialize<T>(T)` / `Deserialize<T>(string)` / `Load<T>(Stream)` / `Save<T>(Stream,T)` round-trip contract — `Dock.Serializer.SystemTextJson.DockSerializer` (`api-dock-serializer.md`) is the admitted impl |
|  [13]   | `IHostWindow` / `IDockWindow`  | float-host contracts                   |
|  [14]   | `IDocumentTemplate` / `IToolTemplate` | item-source template contracts  |
|  [15]   | `IDockDispatcher`              | `Dock.Model.ReactiveUI.Services` UI-thread post |

[MODEL_ENUMS]: `Dock.Model.Core` vocabulary
- rail: docking

| [INDEX] | [SYMBOL]                                   | [RAIL]                             |
| :-----: | :----------------------------------------- | :--------------------------------- |
|  [01]   | `DockMode`                                 | `Left`/`Right`/`Top`/`Bottom`/`Center` |
|  [02]   | `DockOperation` / `DockOperationMask`      | window/fill/split target operation |
|  [03]   | `Alignment` / `Orientation`                | dock alignment + split orientation |
|  [04]   | `DragAction`                               | copy/move/link drag action         |
|  [05]   | `GripMode`                                 | tool grip visibility               |
|  [06]   | `DocumentTabLayout` / `DocumentLayoutMode` / `DocumentCloseButtonShowMode` | document tab policy |
|  [07]   | `PinnedDockDisplayMode`                     | pinned-dock reveal policy          |
|  [08]   | `DockFloatingWindowHostMode` / `DockWindowOwnerMode` / `DockWindowState` | float-window policy |
|  [09]   | `MdiWindowState`                            | MDI child window state             |
|  [10]   | `DockCapability` / `DockCapabilityValueSource` | per-dockable capability flags  |

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: `DockControl` wiring (`StyledProperty`-backed)
- rail: docking

| [INDEX] | [SURFACE]                                  | [TYPE]                   | [RAIL]                  |
| :-----: | :----------------------------------------- | :----------------------- | :---------------------- |
|  [01]   | `Layout`                                   | `IDock?`                 | bound dock graph        |
|  [02]   | `Factory`                                  | `IFactory?`              | factory binding         |
|  [03]   | `DockManager`                              | `IDockManager`           | drag/drop manager       |
|  [04]   | `InitializeLayout` / `InitializeFactory`   | `bool`                   | auto-init on attach     |
|  [05]   | `DefaultContext`                           | `object?`                | context fallback        |
|  [06]   | `IsDockingEnabled` / `IsDraggingDock`      | `bool`                   | drag gate + drag state  |
|  [07]   | `AutoCreateDataTemplates`                  | `bool`                   | auto template gen       |
|  [08]   | `HostWindowFactory`                        | `Func<IHostWindow?>?`    | float-window kind       |
|  [09]   | `EnableManagedWindowLayer`                 | `bool`                   | managed in-window floats |
|  [10]   | `RegisterExternalDockSurface(IExternalDockSurface)` / `UnregisterExternalDockSurface(IExternalDockSurface) -> bool` | method | external surface attach / detach |
|  [11]   | `ShowSelector(DockSelectorMode)` / `HideSelector()` | method          | selector overlay        |

[FACTORY_CONSTRUCTION]: `Factory` (`IFactory`) layout construction — every `Create*` returns the typed `Dock.Model` contract
- rail: docking

| [INDEX] | [SURFACE]                                          | [RETURNS]                  | [RAIL]          |
| :-----: | :------------------------------------------------- | :------------------------- | :-------------- |
|  [01]   | `CreateLayout()`                                   | `IRootDock`                | layout root     |
|  [02]   | `CreateRootDock()`                                 | `IRootDock`                | root dock       |
|  [03]   | `CreateProportionalDock()`                         | `IProportionalDock`        | split dock      |
|  [04]   | `CreateProportionalDockSplitter()`                 | `IProportionalDockSplitter` | splitter       |
|  [05]   | `CreateDocumentDock()`                             | `IDocumentDock`            | document dock   |
|  [06]   | `CreateToolDock()`                                 | `IToolDock`                | tool dock       |
|  [07]   | `CreateDocument()` / `CreateTool()`                | `IDocument` / `ITool`      | document / tool |
|  [08]   | `CreateDockWindow()`                               | `IDockWindow`              | float window    |
|  [09]   | `CreateList<T>(params T[])`                        | `IList<T>`                 | dockable list   |

[FACTORY_OPERATIONS]: `IFactory` docking operations — the layout graph mutates HERE, not through view manipulation
- rail: docking

| [INDEX] | [SURFACE]                                                       | [RAIL]                       |
| :-----: | :-------------------------------------------------------------- | :--------------------------- |
|  [01]   | `InitLayout(IDockable)`                                         | wire owners + active/focused |
|  [02]   | `AddDockable(IDock, IDockable)` / `InsertDockable(IDock, IDockable, int)` | add at end / at index |
|  [03]   | `MoveDockable(...)` / `SwapDockable(...)`                       | reorder / swap within or across docks |
|  [04]   | `RemoveDockable(IDockable, bool collapse)` / `CloseDockable(IDockable)` / `CloseAllDockables(IDock)` | remove / close |
|  [05]   | `FloatDockable(IDockable)`                                      | tear out into a float window |
|  [06]   | `PinDockable(IDockable)` / `UnpinDockable(IDockable)`           | pin to edge / restore        |
|  [07]   | `CollapseDock(IDock)`                                           | collapse an emptied dock     |
|  [08]   | `SetActiveDockable(IDockable)` / `SetFocusedDockable(IDock, IDockable?)` | activate / focus    |
|  [09]   | `CreateWindowFrom(IDockable)` / `AddWindow(IRootDock, IDockWindow)` / `RemoveWindow(IDockWindow)` | float-window lifecycle |
|  [10]   | `FindDockable(IDock, Func<IDockable,bool>)`                     | predicate locate             |
|  [11]   | `RestoreDockable(IDockable)` / `RestoreDockable(string id)` / `OnDockableRestored(IDockable?)` + `event DockableRestored` | load-side rehydration: resolves an id through `DockableLocator`, re-owns it into the graph, and raises `DockableRestored` — the `IDockState.Restore` counterpart to `DockableLocator` |

[FACTORY_REGISTRIES]: `IFactory` live `IDictionary`/`IList` registries the runtime maintains
- rail: docking

| [INDEX] | [SURFACE]                                                       | [RAIL]                       |
| :-----: | :-------------------------------------------------------------- | :--------------------------- |
|  [01]   | `DockControls : IList<IDockControl>` / `HostWindows : IList<IHostWindow>` | active controls + float hosts |
|  [02]   | `VisibleDockableControls` / `PinnedDockableControls` / `TabDockableControls` | dockable-to-control maps |
|  [03]   | `DocumentControls` / `ToolControls`                            | document/tool content maps   |
|  [04]   | `DockableLocator : IDictionary<string, Func<IDockable?>>?`     | id-to-dockable resolver (deserialization) |

[MODEL_GRAPH_PROPERTIES]: `Dock.Model.Core`/`Controls` node + leaf properties the `Factory`-built graph assigns (every property is `get; set;`)
- rail: docking

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT]       | [RAIL]                       |
| :-----: | :-------------------------------------------------------------- | :------------------- | :--------------------------- |
|  [01]   | `Id` (`string`) / `Title` (`string`) / `Context` (`object?`)    | `IDockable`          | identity, header, view-model  |
|  [02]   | `CanFloat` / `CanPin` / `CanClose` (`bool`)                     | `IDockable`          | per-dockable capability gates |
|  [03]   | `Proportion` (`double`)                                         | `IDockable`          | proportional split size       |
|  [04]   | `VisibleDockables` (`IList<IDockable>?`)                        | `IDock`              | child dockable list           |
|  [05]   | `ActiveDockable` (`IDockable?`) / `FocusedDockable` (`IDockable?`) | `IDock`           | active + focused leaf         |
|  [06]   | `Orientation` (`Orientation`)                                   | `IProportionalDock`  | split orientation             |

## [04]-[IMPLEMENTATION_LAW]

[DOCKING_LAW]:
- Package: `Dock.Avalonia`
- Owns: the dock visual tree, drag/drop targets (`DockTarget`/`GlobalDockTarget`), OS and managed-in-window floating hosts (`HostWindow` / `ManagedWindowLayer` gated by `EnableManagedWindowLayer`), pinned docks, selector overlays, the overlay layer (dialog/confirmation/busy via `OverlayHost`), command-bar merge (`DockCommandBarManager`), MDI (`ClassicMdiLayoutManager`/`MdiDocumentControl`), and theme presets (`DockPresetThemeManagerBase`).
- Accept: panel arrangement intent expressed as an `IDock` graph bound through `DockControl.Layout`; the `DockControl.DockManager` (`IDockManager`) validates every drag/drop and the `Factory` operations mutate the graph; data templates resolve `Document`/`Tool` view-models to views (auto-generated when `AutoCreateDataTemplates`).
- Reject: hand-built splitter/tab arrangements for dockable panels; mutating the dock graph through view manipulation instead of the `IFactory` operations; a second selector/overlay layer beside `DockSelectorOverlay`/`OverlayHost`.

[MODEL_LAW]:
- Package: `Dock.Model.ReactiveUI` (over the `Dock.Model` host-neutral core)
- Owns: the ReactiveUI binding of the dock model graph (`DockBase`/`DockableBase` are `ReactiveObject`s) and the `Factory : FactoryBase` override that constructs ReactiveUI-typed `RootDock`/`DocumentDock`/`ToolDock`/`Document`/`Tool` and carries the inherited `IFactory` operations and registries.
- Accept: layout state lives in factory-created models; `DocumentDock.CanCreateDocument`/`CreateDocument` drives runtime document spawning; `IDockDispatcher.Invoke` (the `Dock.Model.ReactiveUI.Services` surface) marshals graph mutation onto the Avalonia UI thread; observed-property reactivity drives the bound `DockControl` without manual invalidation.
- Reject: view-layer mutation of dock structure outside the factory surface; a hand-rolled `INotifyPropertyChanged` model duplicating `DockableBase`; a `Dock.Model.Avalonia`/`Dock.Model.Mvvm` parallel binding when the ReactiveUI binding is the admitted one.

[PERSISTENCE_LAW]:
- The `IFactory` graph + registries are the object the `IDockSerializer` contract round-trips, with `Dock.Serializer.SystemTextJson.DockSerializer` (`api-dock-serializer.md`) as the admitted impl: `IDockState.Save(IDock)`/`Restore(IDock)` snapshots the live layout, the serializer's `DockModelPolymorphicTypeResolver` discriminates the `IDockable`/`IDock`/`IRootDock` graph by `$type`, and on load `DockableLocator` resolves id-keyed dockables while `RestoreDockable(string)`/`OnDockableRestored` re-own each one into the graph and raise `DockableRestored` — so a saved workspace layout restores the exact dock tree, the open documents, and the pinned tools.
- Accept: persist through the `IDockSerializer` round-trip — `DockSerializer.Save<IRootDock>(Stream,…)`/`Load<IRootDock>(Stream)` over the `IDockState` snapshot; register the per-assembly source-generated `IJsonTypeInfoResolver` for AOT-safe serialization (`api-dock-serializer.md`).
- Reject: a bespoke layout serializer replicating the polymorphic resolution the `Dock.Serializer` sibling owns; mutating the restored graph by hand instead of the `RestoreDockable`/`DockableLocator` rehydration surface.
