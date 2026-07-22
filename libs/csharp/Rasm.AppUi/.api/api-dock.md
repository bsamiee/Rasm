# [RASM_APPUI_API_DOCK]

`Dock.Avalonia` binds an `IDock` model graph to a live Avalonia docking surface, and `Dock.Model.ReactiveUI` supplies the `ReactiveObject` binding and the `Factory : FactoryBase` constructing that graph. Every layout mutation flows through the `IFactory` operations under `DockControl.DockManager` validation, never a hand-built splitter or tab arrangement. `Dock.Model` owns the host-neutral contracts in the transitively-restored core, and the `Dock.Serializer.SystemTextJson` catalog (`api-dock-serializer.md`) owns the persistence round-trip.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Dock.Avalonia`
- package: `Dock.Avalonia` (MIT)
- assembly: `Dock.Avalonia`
- namespace: `Dock.Avalonia.Controls`, `.Selectors`, `.Themes`, `.Mdi`, `.CommandBars`, `.Services`
- target: `lib/net10.0`
- asset: runtime library
- depends: `Dock.Model`, `Dock.Settings`
- rail: docking

[PACKAGE_SURFACE]: `Dock.Model.ReactiveUI`
- package: `Dock.Model.ReactiveUI` (MIT)
- assembly: `Dock.Model.ReactiveUI`
- namespace: `Dock.Model.ReactiveUI`, `.Core`, `.Controls`, `.Services`
- target: `lib/net10.0`
- asset: runtime library
- depends: `Dock.Model`, `reactiveui`
- rail: docking

## [02]-[PUBLIC_TYPES]

[DOCK_CONTROLS]: `Dock.Avalonia.Controls` visual tree; every row is a `control`.

| [INDEX] | [SYMBOL]                  | [CAPABILITY]          |
| :-----: | :------------------------ | :-------------------- |
|  [01]   | `DockControl`             | dock root binds graph |
|  [02]   | `RootDockControl`         | root view             |
|  [03]   | `ProportionalDockControl` | split view            |
|  [04]   | `DocumentDockControl`     | document dock         |
|  [05]   | `ToolDockControl`         | tool dock             |
|  [06]   | `DocumentControl`         | document host         |
|  [07]   | `ToolControl`             | tool host             |
|  [08]   | `DocumentTabStrip`        | document tabs         |
|  [09]   | `ToolTabStrip`            | tool tabs             |
|  [10]   | `ToolTabStripItem`        | tool tab item         |
|  [11]   | `ToolChromeControl`       | tool chrome           |
|  [12]   | `PinnedDockControl`       | pinned dock           |
|  [13]   | `PinnedDockHostPanel`     | pinned dock host      |
|  [14]   | `ToolPinnedControl`       | pinned tools          |

[WINDOW_TARGET_OVERLAY_TYPES]: floating and managed-float hosts, drop targets, selectors, overlays, MDI, command bars; every row is a `class`.

| [INDEX] | [SYMBOL]                       | [CAPABILITY]           |
| :-----: | :----------------------------- | :--------------------- |
|  [01]   | `HostWindow`                   | OS floating host       |
|  [02]   | `HostWindowTitleBar`           | floating chrome        |
|  [03]   | `ManagedHostWindow`            | managed float host     |
|  [04]   | `ManagedWindowLayer`           | managed float layer    |
|  [05]   | `ManagedWindowDock`            | managed float dock     |
|  [06]   | `ManagedDockWindowDocument`    | managed float document |
|  [07]   | `DockTarget`                   | local drop target      |
|  [08]   | `GlobalDockTarget`             | global drop target     |
|  [09]   | `DockableControl`              | dockable region        |
|  [10]   | `DragPreviewControl`           | drag preview           |
|  [11]   | `DockSelectorOverlay`          | selector overlay       |
|  [12]   | `DockSelectorItem`             | selector item          |
|  [13]   | `OverlayHost`                  | overlay host           |
|  [14]   | `DialogOverlayControl`         | dialog overlay         |
|  [15]   | `ConfirmationOverlayControl`   | confirmation overlay   |
|  [16]   | `BusyOverlayControl`           | busy overlay           |
|  [17]   | `OverlayLayerRegistry`         | overlay registry       |
|  [18]   | `OverlayLayerCollection`       | overlay collection     |
|  [19]   | `MdiDocumentControl`           | MDI document control   |
|  [20]   | `MdiDocumentWindow`            | MDI document window    |
|  [21]   | `ClassicMdiLayoutManager`      | classic MDI layout     |
|  [22]   | `DockPresetThemeManagerBase`   | theme preset manager   |
|  [23]   | `DockCommandBarManager`        | command-bar merge      |
|  [24]   | `DefaultDockCommandBarAdapter` | command-bar adapter    |
|  [25]   | `DockControlFactoryService`    | control-factory wiring |

[IDockThemeManager]: `Dock.Avalonia.Themes` interface; the `Factory` override exposes `Func<IDockThemeManager>`, and theme-variant changes update dock-owned brushes.
[IExternalDockSurface]: `DockControl? DockControl { get; set; }` and `Control SurfaceControl { get; }` — the embedded surface `DockControl.RegisterExternalDockSurface` attaches.
[DockSelectorMode]: `Documents` `Tools` `All` — the `DockControl.ShowSelector` scope.

[MODEL_TYPES]: `Dock.Model.ReactiveUI` concrete graph and `Dock.Model.Core`/`Controls` contracts.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                |
| :-----: | :------------------------- | :------------ | :-------------------------- |
|  [01]   | `Factory : FactoryBase`    | class         | model factory               |
|  [02]   | `DockableBase`             | class         | reactive dockable           |
|  [03]   | `DockBase`                 | class         | reactive dock               |
|  [04]   | `RootDock`                 | class         | root model                  |
|  [05]   | `ProportionalDock`         | class         | split model                 |
|  [06]   | `ProportionalDockSplitter` | class         | splitter model              |
|  [07]   | `DocumentDock`             | class         | document dock model         |
|  [08]   | `ToolDock`                 | class         | tool dock model             |
|  [09]   | `Document`                 | class         | document model              |
|  [10]   | `Tool`                     | class         | tool model                  |
|  [11]   | `DockWindow`               | class         | float-window model          |
|  [12]   | `IFactory`                 | interface     | factory contract            |
|  [13]   | `IDock`                    | interface     | dock-node contract          |
|  [14]   | `IDockable`                | interface     | dockable contract           |
|  [15]   | `IRootDock`                | interface     | root-dock contract          |
|  [16]   | `IDocumentDock`            | interface     | document contract           |
|  [17]   | `IToolDock`                | interface     | tool contract               |
|  [18]   | `IProportionalDock`        | interface     | split-dock contract         |
|  [19]   | `IDockManager`             | interface     | drag/drop contract          |
|  [20]   | `IDockManagerState`        | interface     | manager state               |
|  [21]   | `IDockState`               | interface     | snapshot contract           |
|  [22]   | `IDockSerializer`          | interface     | round-trip contract         |
|  [23]   | `IHostWindow`              | interface     | float-host contract         |
|  [24]   | `IDockWindow`              | interface     | float-window contract       |
|  [25]   | `IDocumentTemplate`        | interface     | document template           |
|  [26]   | `IToolTemplate`            | interface     | tool template               |
|  [27]   | `IDockDispatcher`          | interface     | `Invoke` posts to UI thread |

[IDockManager]: `ValidateTool`/`ValidateDocument`/`ValidateDock`/`ValidateDockable` gate a drag by source, target, `DragAction`, `DockOperation`, and execution state; `IsDockTargetVisible` gates target rendering.
[IDockState]: `Save(IDock)`, `Restore(IDock)`, and `Reset()` capture and restore the live layout.
[IDockSerializer]: `Serialize<T>`/`Deserialize<T>`/`Load<T>`/`Save<T>` — the round-trip contract the `Dock.Serializer.SystemTextJson` catalog implements.

[MODEL_ENUMS]: `Dock.Model.Core` vocabulary; every row is an `enum` or closed enum family.

| [INDEX] | [SYMBOL]                                                                   | [CAPABILITY]                           |
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

[CONTROL_ENTRYPOINTS]: `DockControl` wiring, `StyledProperty`-backed.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `Layout`                                                      | property | bound `IDock?` graph               |
|  [02]   | `Factory`                                                     | property | bound `IFactory?`                  |
|  [03]   | `DockManager`                                                 | property | `IDockManager` drag/drop policy    |
|  [04]   | `InitializeLayout`                                            | property | `bool` layout-init gate            |
|  [05]   | `InitializeFactory`                                           | property | `bool` factory-init gate           |
|  [06]   | `DefaultContext`                                              | property | `object?` fallback view-model      |
|  [07]   | `IsDockingEnabled`                                            | property | `bool` docking gate                |
|  [08]   | `IsDraggingDock`                                              | property | `bool` drag state                  |
|  [09]   | `AutoCreateDataTemplates`                                     | property | `bool` template auto-gen           |
|  [10]   | `HostWindowFactory`                                           | property | `Func<IHostWindow?>?` host factory |
|  [11]   | `EnableManagedWindowLayer`                                    | property | `bool` managed-float gate          |
|  [12]   | `RegisterExternalDockSurface(IExternalDockSurface)`           | method   | attach embedded surface            |
|  [13]   | `UnregisterExternalDockSurface(IExternalDockSurface) -> bool` | method   | detach embedded surface            |
|  [14]   | `ShowSelector(DockSelectorMode)`                              | method   | show selector overlay              |
|  [15]   | `HideSelector()`                                              | method   | hide selector overlay              |

[FACTORY_CONSTRUCTION]: `Factory` (`IFactory`) layout construction; every `Create*` is a factory returning its typed `Dock.Model` contract.

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]  |
| :-----: | :-------------------------------------------------------------- | :------------ |
|  [01]   | `CreateLayout() -> IRootDock`                                   | layout root   |
|  [02]   | `CreateRootDock() -> IRootDock`                                 | root dock     |
|  [03]   | `CreateProportionalDock() -> IProportionalDock`                 | split dock    |
|  [04]   | `CreateProportionalDockSplitter() -> IProportionalDockSplitter` | splitter      |
|  [05]   | `CreateDocumentDock() -> IDocumentDock`                         | document dock |
|  [06]   | `CreateToolDock() -> IToolDock`                                 | tool dock     |
|  [07]   | `CreateDocument() -> IDocument` / `CreateTool() -> ITool`       | document/tool |
|  [08]   | `CreateDockWindow() -> IDockWindow`                             | float window  |
|  [09]   | `CreateList<T>(params T[]) -> IList<T>`                         | dockable list |

[FACTORY_OPERATIONS]: `IFactory` docking operations; the layout graph mutates here, never through view manipulation.

| [INDEX] | [SURFACE]                                   | [CAPABILITY]     |
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
|  [20]   | `RestoreDockable(string id)`                | restore by id    |
|  [21]   | `OnDockableRestored(IDockable?)`            | restoration hook |
|  [22]   | `DockableRestored`                          | event            |

- `RestoreDockable(string)` resolves through `DockableLocator`, `OnDockableRestored` raises `DockableRestored`, and `IDockState.Restore` is the snapshot counterpart.

[FACTORY_REGISTRIES]: `IFactory` live `IDictionary`/`IList` registries the runtime maintains, every one a property.

| [INDEX] | [SURFACE]                                                                    | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `DockControls : IList<IDockControl>` / `HostWindows : IList<IHostWindow>`    | active controls + float hosts             |
|  [02]   | `VisibleDockableControls` / `PinnedDockableControls` / `TabDockableControls` | dockable-to-control maps                  |
|  [03]   | `DocumentControls` / `ToolControls`                                          | document/tool content maps                |
|  [04]   | `DockableLocator : IDictionary<string, Func<IDockable?>>?`                   | id-to-dockable resolver (deserialization) |

[MODEL_GRAPH_PROPERTIES]: `Dock.Model.Core`/`Controls` node and leaf properties the `Factory`-built graph assigns; every property is `get; set;`.

| [INDEX] | [SURFACE]                            | [CAPABILITY]                           |
| :-----: | :----------------------------------- | :------------------------------------- |
|  [01]   | `Id` / `Title` / `Context`           | `IDockable` identity/header/view-model |
|  [02]   | `CanFloat` / `CanPin` / `CanClose`   | `IDockable` capability gates           |
|  [03]   | `Proportion`                         | `IDockable` proportional split size    |
|  [04]   | `VisibleDockables`                   | `IDock` child dockable list            |
|  [05]   | `ActiveDockable` / `FocusedDockable` | `IDock` active + focused leaf          |
|  [06]   | `Orientation`                        | `IProportionalDock` split orientation  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Panel arrangement is an `IDock` graph bound through `DockControl.Layout`; the `Factory` docking operations mutate that graph, `DockControl.DockManager` (`IDockManager`) validates every drag/drop, and data templates resolve `Document`/`Tool` view-models to views, auto-generated under `AutoCreateDataTemplates`. Visual tree, floating hosts, and overlays render the graph, never authoring it.

[STACKING]:
- `Dock.Serializer.SystemTextJson`(`.api/api-dock-serializer.md`): `DockSerializer.Save<IRootDock>(Stream)`/`Load<IRootDock>` round-trips the `IFactory` graph and registries over the `IDockState.Save(IDock)`/`Restore(IDock)` snapshot, discriminating the `IDockable`/`IDock`/`IRootDock` tree by `$type` and rehydrating through `DockableLocator` and `RestoreDockable`.
- Shell composition (`.planning/Shell/`): the AppUi Shell binds one `IDock` graph per screen through `DockControl.Layout`, mounts host surfaces via `IExternalDockSurface`, and marshals graph mutation onto the Avalonia UI thread through `IDockDispatcher.Invoke`.

[LOCAL_ADMISSION]:
- `Dock.Model.ReactiveUI` is the one admitted binding; a `Dock.Model.Avalonia`/`Dock.Model.Mvvm` parallel binding, a hand-rolled `INotifyPropertyChanged` model duplicating `DockableBase`, or a bespoke serializer replicating the `Dock.Serializer` polymorphism is rejected.

[RAIL_LAW]:
- Package: `Dock.Avalonia`
- Owns: the dock visual tree, drag/drop targets, OS and managed floating hosts (`HostWindow`/`ManagedWindowLayer` under `EnableManagedWindowLayer`), pinned docks, selector and dialog/confirmation/busy overlays through `OverlayHost`, command-bar merge, MDI, and theme presets.
- Accept: an `IDock` graph bound through `DockControl.Layout`, mutated by the `IFactory` operations under `IDockManager` validation.
- Reject: hand-built splitter/tab arrangement, graph mutation through view manipulation, a second selector/overlay layer beside `DockSelectorOverlay`/`OverlayHost`.
- Package: `Dock.Model.ReactiveUI`
- Owns: the `ReactiveObject` binding of the dock model graph and the `Factory : FactoryBase` override constructing ReactiveUI-typed `RootDock`/`DocumentDock`/`ToolDock`/`Document`/`Tool` with the inherited `IFactory` operations and registries.
- Accept: layout state in factory-created models; `DocumentDock.CanCreateDocument`/`CreateDocument` drives runtime document spawning; observed-property reactivity drives the bound `DockControl` without manual invalidation.
- Reject: view-layer mutation of dock structure outside the factory surface.
