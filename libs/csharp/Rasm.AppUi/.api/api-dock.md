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

| [INDEX] | [SYMBOL]                                    | [RAIL]                          |
| :-----: | :------------------------------------------ | :------------------------------ |
|  [01]   | `DockControl`                               | dock root control (binds graph) |
|  [02]   | `RootDockControl`                           | root view                       |
|  [03]   | `ProportionalDockControl`                   | split view                      |
|  [04]   | `DocumentDockControl`                       | document dock                   |
|  [05]   | `ToolDockControl`                           | tool dock                       |
|  [06]   | `DocumentControl`                           | document host                   |
|  [07]   | `ToolControl`                               | tool host                       |
|  [08]   | `DocumentTabStrip`                          | document tabs                   |
|  [09]   | `ToolTabStrip` / `ToolTabStripItem`         | tool tabs                       |
|  [10]   | `ToolChromeControl`                         | tool chrome                     |
|  [11]   | `PinnedDockControl` / `PinnedDockHostPanel` | pinned dock + host              |
|  [12]   | `ToolPinnedControl`                         | pinned tools                    |

[WINDOW_TARGET_OVERLAY_TYPES]: floating windows, managed-float layer, drop targets, selectors, overlays, MDI
- rail: docking

| [INDEX] | [SYMBOL]                       | [RAIL]                    |
| :-----: | :----------------------------- | :------------------------ |
|  [01]   | `HostWindow`                   | OS floating host          |
|  [02]   | `HostWindowTitleBar`           | floating chrome           |
|  [03]   | `ManagedHostWindow`            | managed float host        |
|  [04]   | `ManagedWindowLayer`           | managed float layer       |
|  [05]   | `ManagedWindowDock`            | managed float dock        |
|  [06]   | `ManagedDockWindowDocument`    | managed float document    |
|  [07]   | `DockTarget`                   | local drop target         |
|  [08]   | `GlobalDockTarget`             | global drop target        |
|  [09]   | `DockableControl`              | dockable region           |
|  [10]   | `DragPreviewControl`           | drag preview              |
|  [11]   | `DockSelectorOverlay`          | selector overlay          |
|  [12]   | `DockSelectorItem`             | selector item             |
|  [13]   | `OverlayHost`                  | overlay host              |
|  [14]   | `DialogOverlayControl`         | dialog overlay            |
|  [15]   | `ConfirmationOverlayControl`   | confirmation overlay      |
|  [16]   | `BusyOverlayControl`           | busy overlay              |
|  [17]   | `OverlayLayerRegistry`         | overlay registry          |
|  [18]   | `OverlayLayerCollection`       | overlay collection        |
|  [19]   | `MdiDocumentControl`           | MDI document control      |
|  [20]   | `MdiDocumentWindow`            | MDI document window       |
|  [21]   | `ClassicMdiLayoutManager`      | classic MDI layout        |
|  [22]   | `DockPresetThemeManagerBase`   | theme preset manager      |
|  [23]   | `DockCommandBarManager`        | command-bar merge         |
|  [24]   | `DefaultDockCommandBarAdapter` | command-bar adapter       |
|  [25]   | `DockControlFactoryService`    | control-factory wiring    |
|  [26]   | `IDockThemeManager`            | theme-manager contract    |
|  [27]   | `IExternalDockSurface`         | external surface contract |
|  [28]   | `DockSelectorMode`             | selector scope            |

[IDOCK_THEME_MANAGER]:
- Namespace: `Dock.Avalonia.Themes`
- Binding: the `Factory` override exposes `Func<IDockThemeManager>`
- Effect: theme-variant changes update dock-owned brushes

[IEXTERNAL_DOCK_SURFACE]:
- Properties: `DockControl? DockControl { get; set; }` and `Control SurfaceControl { get; }`
- Lifecycle: `DockControl.RegisterExternalDockSurface` attaches and `UnregisterExternalDockSurface` detaches the embedded surface

[DOCK_SELECTOR_MODE]:
- Namespace: `Dock.Avalonia.Selectors`
- Values: `Documents`, `Tools`, and `All`
- Consumer: `DockControl.ShowSelector`

[MODEL_TYPES]: `Dock.Model.ReactiveUI` graph + `Dock.Model.Core`/`Controls` contracts
- rail: docking

| [INDEX] | [SYMBOL]                   | [RAIL]                |
| :-----: | :------------------------- | :-------------------- |
|  [01]   | `Factory : FactoryBase`    | model factory         |
|  [02]   | `DockableBase`             | reactive dockable     |
|  [03]   | `DockBase`                 | reactive dock         |
|  [04]   | `RootDock`                 | root model            |
|  [05]   | `ProportionalDock`         | split model           |
|  [06]   | `ProportionalDockSplitter` | splitter model        |
|  [07]   | `DocumentDock`             | document dock model   |
|  [08]   | `ToolDock`                 | tool dock model       |
|  [09]   | `Document`                 | document model        |
|  [10]   | `Tool`                     | tool model            |
|  [11]   | `DockWindow`               | float-window model    |
|  [12]   | `IFactory`                 | factory contract      |
|  [13]   | `IDock`                    | dock-node contract    |
|  [14]   | `IDockable`                | dockable contract     |
|  [15]   | `IRootDock`                | root-dock contract    |
|  [16]   | `IDocumentDock`            | document contract     |
|  [17]   | `IToolDock`                | tool contract         |
|  [18]   | `IProportionalDock`        | split-dock contract   |
|  [19]   | `IDockManager`             | drag/drop contract    |
|  [20]   | `IDockManagerState`        | manager state         |
|  [21]   | `IDockState`               | snapshot contract     |
|  [22]   | `IDockSerializer`          | round-trip contract   |
|  [23]   | `IHostWindow`              | float-host contract   |
|  [24]   | `IDockWindow`              | float-window contract |
|  [25]   | `IDocumentTemplate`        | document template     |
|  [26]   | `IToolTemplate`            | tool template         |
|  [27]   | `IDockDispatcher`          | UI-thread dispatch    |

[IDOCK_MANAGER]:
- Validation: `ValidateTool`, `ValidateDocument`, `ValidateDock`, and `ValidateDockable`
- Inputs: source, target, `DragAction`, `DockOperation`, and execution state
- Visibility: `IsDockTargetVisible`

[IDOCK_STATE]:
- Snapshot: `Save(IDock)`, `Restore(IDock)`, and `Reset()`

[IDOCK_SERIALIZER]:
- Methods: `Serialize<T>(T)`, `Deserialize<T>(string)`, `Load<T>(Stream)`, and `Save<T>(Stream,T)`
- Implementation: `Dock.Serializer.SystemTextJson.DockSerializer` (`api-dock-serializer.md`)

[IDOCK_DISPATCHER]:
- Namespace: `Dock.Model.ReactiveUI.Services`
- Operation: UI-thread post

[MODEL_ENUMS]: `Dock.Model.Core` vocabulary
- rail: docking

| [INDEX] | [SYMBOL]                                                                   | [RAIL]                                 |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `DockMode`                                                                 | `Left`/`Right`/`Top`/`Bottom`/`Center` |
|  [02]   | `DockOperation` / `DockOperationMask`                                      | window/fill/split target operation     |
|  [03]   | `Alignment` / `Orientation`                                                | dock alignment + split orientation     |
|  [04]   | `DragAction`                                                               | copy/move/link drag action             |
|  [05]   | `GripMode`                                                                 | tool grip visibility                   |
|  [06]   | `DocumentTabLayout` / `DocumentLayoutMode` / `DocumentCloseButtonShowMode` | document tab policy                    |
|  [07]   | `PinnedDockDisplayMode`                                                    | pinned-dock reveal policy              |
|  [08]   | `DockFloatingWindowHostMode` / `DockWindowOwnerMode` / `DockWindowState`   | float-window policy                    |
|  [09]   | `MdiWindowState`                                                           | MDI child window state                 |
|  [10]   | `DockCapability` / `DockCapabilityValueSource`                             | per-dockable capability flags          |

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: `DockControl` wiring (`StyledProperty`-backed)
- rail: docking

| [INDEX] | [SURFACE]                                             | [TYPE]                | [RAIL]          |
| :-----: | :---------------------------------------------------- | :-------------------- | :-------------- |
|  [01]   | `Layout`                                              | `IDock?`              | dock graph      |
|  [02]   | `Factory`                                             | `IFactory?`           | factory binding |
|  [03]   | `DockManager`                                         | `IDockManager`        | drag/drop       |
|  [04]   | `InitializeLayout`                                    | `bool`                | layout init     |
|  [05]   | `InitializeFactory`                                   | `bool`                | factory init    |
|  [06]   | `DefaultContext`                                      | `object?`             | fallback        |
|  [07]   | `IsDockingEnabled`                                    | `bool`                | docking gate    |
|  [08]   | `IsDraggingDock`                                      | `bool`                | drag state      |
|  [09]   | `AutoCreateDataTemplates`                             | `bool`                | templates       |
|  [10]   | `HostWindowFactory`                                   | `Func<IHostWindow?>?` | window factory  |
|  [11]   | `EnableManagedWindowLayer`                            | `bool`                | managed floats  |
|  [12]   | `RegisterExternalDockSurface(IExternalDockSurface)`   | method                | attach surface  |
|  [13]   | `UnregisterExternalDockSurface(IExternalDockSurface)` | `bool`                | detach surface  |
|  [14]   | `ShowSelector(DockSelectorMode)`                      | method                | show selector   |
|  [15]   | `HideSelector()`                                      | method                | hide selector   |

[FACTORY_CONSTRUCTION]: `Factory` (`IFactory`) layout construction — every `Create*` returns the typed `Dock.Model` contract
- rail: docking

| [INDEX] | [SURFACE]                           | [RETURNS]                   | [RAIL]          |
| :-----: | :---------------------------------- | :-------------------------- | :-------------- |
|  [01]   | `CreateLayout()`                    | `IRootDock`                 | layout root     |
|  [02]   | `CreateRootDock()`                  | `IRootDock`                 | root dock       |
|  [03]   | `CreateProportionalDock()`          | `IProportionalDock`         | split dock      |
|  [04]   | `CreateProportionalDockSplitter()`  | `IProportionalDockSplitter` | splitter        |
|  [05]   | `CreateDocumentDock()`              | `IDocumentDock`             | document dock   |
|  [06]   | `CreateToolDock()`                  | `IToolDock`                 | tool dock       |
|  [07]   | `CreateDocument()` / `CreateTool()` | `IDocument` / `ITool`       | document / tool |
|  [08]   | `CreateDockWindow()`                | `IDockWindow`               | float window    |
|  [09]   | `CreateList<T>(params T[])`         | `IList<T>`                  | dockable list   |

[FACTORY_OPERATIONS]: `IFactory` docking operations — the layout graph mutates HERE, not through view manipulation
- rail: docking

| [INDEX] | [SURFACE]                                   | [RAIL]           |
| :-----: | :------------------------------------------ | :--------------- |
|  [01]   | `InitLayout(IDockable)`                     | initialize       |
|  [02]   | `AddDockable(IDock, IDockable)`             | append           |
|  [03]   | `InsertDockable(IDock, IDockable, int)`     | indexed insert   |
|  [04]   | `MoveDockable(...)`                         | move             |
|  [05]   | `SwapDockable(...)`                         | swap             |
|  [06]   | `RemoveDockable(IDockable, bool collapse)`  | remove           |
|  [07]   | `CloseDockable(IDockable)`                  | close            |
|  [08]   | `CloseAllDockables(IDock)`                  | close all        |
|  [09]   | `FloatDockable(IDockable)`                  | float            |
|  [10]   | `PinDockable(IDockable)`                    | pin              |
|  [11]   | `UnpinDockable(IDockable)`                  | restore          |
|  [12]   | `CollapseDock(IDock)`                       | collapse         |
|  [13]   | `SetActiveDockable(IDockable)`              | activate         |
|  [14]   | `SetFocusedDockable(IDock, IDockable?)`     | focus            |
|  [15]   | `CreateWindowFrom(IDockable)`               | create window    |
|  [16]   | `AddWindow(IRootDock, IDockWindow)`         | add window       |
|  [17]   | `RemoveWindow(IDockWindow)`                 | remove window    |
|  [18]   | `FindDockable(IDock, Func<IDockable,bool>)` | locate           |
|  [19]   | `RestoreDockable(IDockable)`                | restore object   |
|  [20]   | `RestoreDockable(string id)`                | restore by ID    |
|  [21]   | `OnDockableRestored(IDockable?)`            | restoration hook |
|  [22]   | `DockableRestored`                          | event            |

[RESTORE_DOCKABLE]:
- Resolution: `RestoreDockable(string id)` resolves through `DockableLocator`
- Ownership: the restored dockable rejoins the graph
- Notification: `OnDockableRestored` raises `DockableRestored`
- Counterpart: `IDockState.Restore`

[FACTORY_REGISTRIES]: `IFactory` live `IDictionary`/`IList` registries the runtime maintains
- rail: docking

| [INDEX] | [SURFACE]                                                                    | [RAIL]                                    |
| :-----: | :--------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `DockControls : IList<IDockControl>` / `HostWindows : IList<IHostWindow>`    | active controls + float hosts             |
|  [02]   | `VisibleDockableControls` / `PinnedDockableControls` / `TabDockableControls` | dockable-to-control maps                  |
|  [03]   | `DocumentControls` / `ToolControls`                                          | document/tool content maps                |
|  [04]   | `DockableLocator : IDictionary<string, Func<IDockable?>>?`                   | id-to-dockable resolver (deserialization) |

[MODEL_GRAPH_PROPERTIES]: `Dock.Model.Core`/`Controls` node + leaf properties the `Factory`-built graph assigns (every property is `get; set;`)
- rail: docking

| [INDEX] | [SURFACE]                                                          | [SURFACE_ROOT]      | [RAIL]                        |
| :-----: | :----------------------------------------------------------------- | :------------------ | :---------------------------- |
|  [01]   | `Id` (`string`) / `Title` (`string`) / `Context` (`object?`)       | `IDockable`         | identity, header, view-model  |
|  [02]   | `CanFloat` / `CanPin` / `CanClose` (`bool`)                        | `IDockable`         | per-dockable capability gates |
|  [03]   | `Proportion` (`double`)                                            | `IDockable`         | proportional split size       |
|  [04]   | `VisibleDockables` (`IList<IDockable>?`)                           | `IDock`             | child dockable list           |
|  [05]   | `ActiveDockable` (`IDockable?`) / `FocusedDockable` (`IDockable?`) | `IDock`             | active + focused leaf         |
|  [06]   | `Orientation` (`Orientation`)                                      | `IProportionalDock` | split orientation             |

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
- Graph: `IDockSerializer` round-trips the `IFactory` graph and registries through `Dock.Serializer.SystemTextJson.DockSerializer` (`api-dock-serializer.md`).
- Snapshot: `IDockState.Save(IDock)` and `Restore(IDock)` capture and restore the live layout.
- Discriminator: `DockModelPolymorphicTypeResolver` identifies the `IDockable`, `IDock`, and `IRootDock` graph by `$type`.
- Rehydration: `DockableLocator` resolves id-keyed dockables, and `RestoreDockable(string)` with `OnDockableRestored` re-owns each dockable and raises `DockableRestored`.
- Outcome: the restored workspace retains its dock tree, open documents, and pinned tools.
- Accept: persist through the `IDockSerializer` round-trip — `DockSerializer.Save<IRootDock>(Stream,…)`/`Load<IRootDock>(Stream)` over the `IDockState` snapshot; register the per-assembly source-generated `IJsonTypeInfoResolver` for AOT-safe serialization (`api-dock-serializer.md`).
- Reject: a bespoke layout serializer replicating the polymorphic resolution the `Dock.Serializer` sibling owns; mutating the restored graph by hand instead of the `RestoreDockable`/`DockableLocator` rehydration surface.
