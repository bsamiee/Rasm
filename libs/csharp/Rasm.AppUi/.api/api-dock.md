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

[WINDOW_TARGET_OVERLAY_TYPES]: floating and managed-float hosts, drop targets, selectors, overlays, MDI, command bars, theme presets, factory wiring; every row is a `class`.

| [INDEX] | [SYMBOL]                       | [CAPABILITY]             |
| :-----: | :----------------------------- | :----------------------- |
|  [01]   | `HostWindow`                   | OS floating host         |
|  [02]   | `HostWindowTitleBar`           | floating chrome          |
|  [03]   | `ManagedHostWindow`            | managed float host       |
|  [04]   | `ManagedWindowLayer`           | managed float layer      |
|  [05]   | `ManagedWindowDock`            | managed float dock       |
|  [06]   | `ManagedDockWindowDocument`    | managed float document   |
|  [07]   | `DockTarget`                   | local drop target        |
|  [08]   | `GlobalDockTarget`             | global drop target       |
|  [09]   | `DockableControl`              | dockable region          |
|  [10]   | `DragPreviewControl`           | drag preview             |
|  [11]   | `DockSelectorOverlay`          | selector overlay         |
|  [12]   | `DockSelectorItem`             | selector item            |
|  [13]   | `OverlayHost`                  | overlay host             |
|  [14]   | `DialogOverlayControl`         | dialog overlay           |
|  [15]   | `ConfirmationOverlayControl`   | confirmation overlay     |
|  [16]   | `BusyOverlayControl`           | busy overlay             |
|  [17]   | `OverlayLayerRegistry`         | overlay registry         |
|  [18]   | `OverlayLayerCollection`       | overlay collection       |
|  [19]   | `MdiDocumentControl`           | MDI document control     |
|  [20]   | `MdiDocumentWindow`            | MDI document window      |
|  [21]   | `ClassicMdiLayoutManager`      | classic MDI layout       |
|  [22]   | `DockPresetThemeManagerBase`   | `IDockThemeManager` base |
|  [23]   | `DockCommandBarManager`        | command-bar merge        |
|  [24]   | `DefaultDockCommandBarAdapter` | command-bar adapter      |
|  [25]   | `DockControlFactoryService`    | control-factory wiring   |

[IDockThemeManager]: `Dock.Avalonia.Themes` preset contract — `PresetNames` `CurrentPresetIndex` `Switch(int)` `SwitchPreset(int)`; `Switch` moves the app `ThemeVariant`, `SwitchPreset` swaps the merged preset dictionary.
[IExternalDockSurface]: `DockControl? DockControl { get; set; }` and `Control SurfaceControl { get; }` — the embedded surface `DockControl.RegisterExternalDockSurface` attaches.
[DockSelectorMode]: `Documents` `Tools` `All` — the `DockControl.ShowSelector` scope.

[MODEL_TYPES]: `Dock.Model.ReactiveUI` concrete graph and `Dock.Model.Core`/`Controls` contracts.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]            |
| :-----: | :------------------------- | :------------ | :---------------------- |
|  [01]   | `Factory : FactoryBase`    | class         | model factory           |
|  [02]   | `DockableBase`             | class         | reactive dockable       |
|  [03]   | `DockBase`                 | class         | reactive dock           |
|  [04]   | `RootDock`                 | class         | root model              |
|  [05]   | `ProportionalDock`         | class         | split model             |
|  [06]   | `ProportionalDockSplitter` | class         | splitter model          |
|  [07]   | `DocumentDock`             | class         | document dock model     |
|  [08]   | `ToolDock`                 | class         | tool dock model         |
|  [09]   | `Document`                 | class         | document model          |
|  [10]   | `Tool`                     | class         | tool model              |
|  [11]   | `DockWindow`               | class         | float-window model      |
|  [12]   | `IFactory`                 | interface     | factory contract        |
|  [13]   | `IDock`                    | interface     | dock-node contract      |
|  [14]   | `IDockable`                | interface     | dockable contract       |
|  [15]   | `IRootDock`                | interface     | root-dock contract      |
|  [16]   | `IDocumentDock`            | interface     | document contract       |
|  [17]   | `IToolDock`                | interface     | tool contract           |
|  [18]   | `IProportionalDock`        | interface     | split-dock contract     |
|  [19]   | `IDockManager`             | interface     | drag/drop contract      |
|  [20]   | `IDockManagerState`        | interface     | manager state           |
|  [21]   | `IDockState`               | interface     | snapshot contract       |
|  [22]   | `IDockSerializer`          | interface     | round-trip contract     |
|  [23]   | `IHostWindow`              | interface     | float-host contract     |
|  [24]   | `IDockWindow`              | interface     | float-window contract   |
|  [25]   | `IDocumentTemplate`        | interface     | document template       |
|  [26]   | `IToolTemplate`            | interface     | tool template           |
|  [27]   | `IDockDispatcher`          | interface     | UI-thread post contract |

[IDockManager]: `ValidateTool`/`ValidateDocument`/`ValidateDock`/`ValidateDockable` gate a drag by source, target, `DragAction`, `DockOperation`, and execution state; `IsDockTargetVisible` gates target rendering.
[IDockState]: `Save(IDock)`, `Restore(IDock)`, and `Reset()` capture and restore the live layout.
[IDockSerializer]: `Serialize<T>`/`Deserialize<T>`/`Load<T>`/`Save<T>` — the round-trip contract the `Dock.Serializer.SystemTextJson` catalog implements.
[IDockDispatcher]: `Task InvokeAsync(Action)` in `Dock.Model.ReactiveUI.Services` — the framework-free UI-thread seam the ReactiveUI binding marshals graph mutation through.

[MODEL_ENUMS]: `Dock.Model.Core` vocabulary; every row is an `enum` or closed enum family.

| [INDEX] | [SYMBOL]                                                                   | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `DockMode`                                                                 | `Left`/`Right`/`Top`/`Bottom`/`Center`                    |
|  [02]   | `DockOperation` / `DockOperationMask`                                      | window/fill/split target operation                        |
|  [03]   | `Alignment` / `Orientation`                                                | dock alignment + split orientation                        |
|  [04]   | `DragAction`                                                               | copy/move/link drag action                                |
|  [05]   | `GripMode`                                                                 | tool grip visibility                                      |
|  [06]   | `DocumentTabLayout` / `DocumentLayoutMode` / `DocumentCloseButtonShowMode` | document tab policy                                       |
|  [07]   | `PinnedDockDisplayMode`                                                    | `Overlay` floats the preview, `Inline` takes layout space |
|  [08]   | `DockFloatingWindowHostMode` / `DockWindowOwnerMode` / `DockWindowState`   | float-window policy                                       |
|  [09]   | `MdiWindowState`                                                           | MDI child window state                                    |
|  [10]   | `DockCapability` / `DockCapabilityValueSource`                             | per-dockable capability flags                             |

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: `DockControl` wiring — rows [01]-[10] are `StyledProperty`-backed and bind from XAML; the rest answer at runtime.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------------ | :------- | :------------------------------------------------------- |
|  [01]   | `Layout`                                                      | property | bound `IDock?` graph                                     |
|  [02]   | `Factory`                                                     | property | bound `IFactory?`                                        |
|  [03]   | `InitializeLayout`                                            | property | `bool` layout-init gate                                  |
|  [04]   | `InitializeFactory`                                           | property | `bool` factory-init gate                                 |
|  [05]   | `DefaultContext`                                              | property | `object?` fallback view-model                            |
|  [06]   | `IsDockingEnabled`                                            | property | `bool` docking gate                                      |
|  [07]   | `IsDraggingDock`                                              | property | `bool` drag state                                        |
|  [08]   | `AutoCreateDataTemplates`                                     | property | `bool` template auto-gen                                 |
|  [09]   | `HostWindowFactory`                                           | property | `Func<IHostWindow?>?` float-host override                |
|  [10]   | `EnableManagedWindowLayer`                                    | property | `bool` in-app window-layer render gate, default true     |
|  [11]   | `DockManager` / `DockManagerOptions` / `DockControlState`     | property | drag/drop policy, its option record, and live drag state |
|  [12]   | `DragOffsetCalculator`                                        | property | settable `IDragOffsetCalculator` grab-offset policy      |
|  [13]   | `IsOpen`                                                      | property | `bool` selector-overlay state                            |
|  [14]   | `RegisterExternalDockSurface(IExternalDockSurface)`           | method   | attach embedded surface                                  |
|  [15]   | `UnregisterExternalDockSurface(IExternalDockSurface) -> bool` | method   | detach embedded surface                                  |
|  [16]   | `ShowSelector(DockSelectorMode)` / `HideSelector()`           | method   | drive the selector overlay                               |

- `EnableManagedWindowLayer`: renders the layer only when `DockSettings.IsManagedWindowHostingEnabled(root)` also holds; float-host CLASS selection stays in `[FLOATING_HOSTS]`.

[TOOL_CHROME_SLOTS]: `ToolChromeControl : ContentControl` carries the tool header state and the per-button `ControlTheme` seats a skin overrides; every row is a `StyledProperty`.

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `Title`                                                | property | `string` header text                             |
|  [02]   | `IsActive` / `IsFloating` / `IsMaximized` / `IsPinned` | property | `bool` chrome state the template styles against  |
|  [03]   | `ToolFlyout`                                           | property | `FlyoutBase?` opened from the chrome menu button |
|  [04]   | `{Close,Maximize,Menu,Pin}ButtonTheme`                 | property | `ControlTheme?` per-button theme seats           |

- `ToolChromeControl` names its `Title` backing field `TitleProprty`; bind `Title` or spell that exact misspelling, never `TitleProperty`.

[DRAG_PREVIEW]: `DragPreviewControl : TemplatedControl` renders the in-flight drag ghost; every row is a `StyledProperty`.

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `PreviewContent`                               | property | `Control?` live content shown inside the ghost         |
|  [02]   | `PreviewContentWidth` / `PreviewContentHeight` | property | `double`, `NaN` default — ghost size, unset means auto |
|  [03]   | `ShowContent`                                  | property | `bool` content-vs-placeholder gate                     |
|  [04]   | `Status`                                       | property | `string` drop-verdict caption                          |
|  [05]   | `ContentTemplate`                              | property | `IDataTemplate` for the previewed dockable             |
|  [06]   | `ControlRecycling`                             | property | `IControlRecycling?` reusing the dragged view instance |

[FLOATING_HOSTS]: float-window hosting — `HostWindow : Window, IHostWindow` for native OS windows, `ManagedHostWindow : IHostWindow` for in-app ones.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------------------ | :------- | :--------------------------------------------------------- |
|  [01]   | `HostWindow.IsToolWindow`                                     | property | `bool` tool-window chrome mode                             |
|  [02]   | `HostWindow.ToolChromeControlsWholeWindow`                    | property | `bool` — `ToolChromeControl` owns the whole window frame   |
|  [03]   | `HostWindow.DocumentChromeControlsWholeWindow`                | property | `bool` — document chrome owns the whole window frame       |
|  [04]   | `HostWindow.{Attach,Detach}Grip(Control, string)`             | instance | bind a drag grip and pseudo-class; 1-arg chrome overload   |
|  [05]   | `ManagedWindowLayer.Dock` / `.LayoutManager`                  | property | `IDock?` hosted graph, `IMdiLayoutManager?` placement rule |
|  [06]   | `ManagedWindowLayer.ShowOverlay(string, Control, Rect, bool)` | instance | keyed overlay placement; `HideOverlay(string)` clears it   |
|  [07]   | `DockSettings.ResolveFloatingWindowHostMode(IRootDock?)`      | static   | resolve `Native` vs `Managed` for one root                 |

- Host CLASS follows `DockFloatingWindowHostMode` resolved per root: `IRootDock.FloatingWindowHostMode`, else the global `DockSettings.FloatingWindowHostMode`, else `UseManagedWindows ? Managed : Native`.

[FACTORY_CONSTRUCTION]: `Factory` (`IFactory`) layout construction; every `Create*` is a factory returning its typed `Dock.Model` contract.

| [INDEX] | [SURFACE]                                                                         | [CAPABILITY]           |
| :-----: | :-------------------------------------------------------------------------------- | :--------------------- |
|  [01]   | `CreateLayout() -> IRootDock`                                                     | layout root            |
|  [02]   | `CreateRootDock() -> IRootDock`                                                   | root dock              |
|  [03]   | `CreateProportionalDock() -> IProportionalDock`                                   | split dock             |
|  [04]   | `CreateProportionalDockSplitter() -> IProportionalDockSplitter`                   | splitter               |
|  [05]   | `CreateDocumentDock() -> IDocumentDock`                                           | document dock          |
|  [06]   | `CreateToolDock() -> IToolDock`                                                   | tool dock              |
|  [07]   | `CreateDocument() -> IDocument` / `CreateTool() -> ITool`                         | document/tool          |
|  [08]   | `CreateDockWindow() -> IDockWindow`                                               | float window           |
|  [09]   | `CreateList<T>(params T[]) -> IList<T>`                                           | dockable list          |
|  [10]   | `CreateDockDock() -> IDockDock` / `CreateStackDock() -> IStackDock`               | edge-dock / stack dock |
|  [11]   | `CreateGridDock() -> IGridDock` / `CreateGridDockSplitter() -> IGridDockSplitter` | grid dock + splitter   |
|  [12]   | `CreateWrapDock() -> IWrapDock` / `CreateUniformGridDock() -> IUniformGridDock`   | wrap / uniform-grid    |
|  [13]   | `CreateSplitViewDock() -> ISplitViewDock`                                         | split-view dock        |

[FACTORY_OPERATIONS]: `IFactory` docking operations; the layout graph mutates here, never through view manipulation.

| [INDEX] | [SURFACE]                                      | [CAPABILITY]     |
| :-----: | :--------------------------------------------- | :--------------- |
|  [01]   | `InitLayout(IDockable)`                        | initialize       |
|  [02]   | `AddDockable(IDock, IDockable)`                | append           |
|  [03]   | `InsertDockable(IDock, IDockable, int)`        | indexed insert   |
|  [04]   | `MoveDockable(...)`                            | move             |
|  [05]   | `SwapDockable(...)`                            | swap             |
|  [06]   | `RemoveDockable(IDockable, bool collapse)`     | remove           |
|  [07]   | `CloseDockable(IDockable)`                     | close            |
|  [08]   | `CloseAllDockables(IDock)`                     | close all        |
|  [09]   | `FloatDockable(IDockable, DockWindowOptions?)` | float            |
|  [10]   | `PinDockable(IDockable)`                       | pin toggle       |
|  [11]   | `UnpinDockable(IDockable)`                     | unpin            |
|  [12]   | `CollapseDock(IDock)`                          | collapse         |
|  [13]   | `SetActiveDockable(IDockable)`                 | activate         |
|  [14]   | `SetFocusedDockable(IDock, IDockable?)`        | focus            |
|  [15]   | `CreateWindowFrom(IDockable)`                  | create window    |
|  [16]   | `AddWindow(IRootDock, IDockWindow)`            | add window       |
|  [17]   | `RemoveWindow(IDockWindow)`                    | remove window    |
|  [18]   | `FindDockable(IDock, Func<IDockable,bool>)`    | locate           |
|  [19]   | `RestoreDockable(IDockable)`                   | restore object   |
|  [20]   | `RestoreDockable(string id)`                   | restore by id    |
|  [21]   | `OnDockableRestored(IDockable?)`               | restoration hook |
|  [22]   | `DockableRestored`                             | event            |

- `RestoreDockable(string)` resolves through `DockableLocator`, `OnDockableRestored` raises `DockableRestored`, and `IDockState.Restore` is the snapshot counterpart.
- `PinDockable` toggles: it unpins an already-pinned dockable, gates on `DockCapability.Pin`, and takes the pin alignment from `IDockable.OriginalOwner`; `UnpinDockable` delegates back to it under an `IsDockablePinned` guard.

[PINNED_DOCK_RAIL]: pinned-tool state on `IFactory` and the strip/flyout controls rendering it.

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `IFactory.IsDockablePinned(IDockable, IRootDock?)`  | instance | pinned test, root resolved when omitted                        |
|  [02]   | `IFactory.PreviewPinnedDockable(IDockable)`         | instance | reveal one pinned tool without unpinning                       |
|  [03]   | `IFactory.TogglePreviewPinnedDockable(IDockable)`   | instance | flip that reveal                                               |
|  [04]   | `IFactory.HidePreviewingDockables(IRootDock)`       | instance | collapse every previewed tool on one root                      |
|  [05]   | `IFactory.MovePinnedDockable(IDockable, IDockable)` | instance | reorder inside the pinned collection                           |
|  [06]   | `PinnedDockControl.PinnedDockAlignment`             | property | `Alignment` — `Unset`/`Left`/`Bottom`/`Right`/`Top` strip edge |
|  [07]   | `PinnedDockControl.PinnedDockDisplayMode`           | property | `PinnedDockDisplayMode` overlay-vs-inline reveal               |
|  [08]   | `ToolPinnedControl.Orientation`                     | property | `Avalonia.Layout.Orientation`, default `Vertical`              |

- `ToolPinnedControl.Orientation` pushes down onto every `ToolPinItemControl` it creates or prepares, so the strip and its items never diverge.

[FACTORY_REGISTRIES]: `IFactory` live `IDictionary`/`IList` registries the runtime maintains, every one a property.

| [INDEX] | [SURFACE]                                                                    | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `DockControls : IList<IDockControl>` / `HostWindows : IList<IHostWindow>`    | active controls + float hosts             |
|  [02]   | `VisibleDockableControls` / `PinnedDockableControls` / `TabDockableControls` | dockable-to-`IDockableControl` maps       |
|  [03]   | `VisibleRootControls` / `PinnedRootControls` / `TabRootControls`             | dockable-to-root-control maps             |
|  [04]   | `DocumentControls` / `ToolControls`                                          | document/tool content maps                |
|  [05]   | `DockableLocator : IDictionary<string, Func<IDockable?>>?`                   | id-to-dockable resolver (deserialization) |

[MODEL_GRAPH_PROPERTIES]: `Dock.Model.Core`/`Controls` node and leaf properties the `Factory`-built graph assigns; every property is `get; set;`.

| [INDEX] | [SURFACE]                            | [CAPABILITY]                           |
| :-----: | :----------------------------------- | :------------------------------------- |
|  [01]   | `Id` / `Title` / `Context`           | `IDockable` identity/header/view-model |
|  [02]   | `CanFloat` / `CanPin` / `CanClose`   | `IDockable` capability gates           |
|  [03]   | `Proportion`                         | `IDockable` proportional split size    |
|  [04]   | `VisibleDockables`                   | `IDock` child dockable list            |
|  [05]   | `ActiveDockable` / `FocusedDockable` | `IDock` active + focused leaf          |
|  [06]   | `Orientation`                        | `IProportionalDock` split orientation  |

[SKIN_RESOURCE_SLOTS]: `Semi.Avalonia.Dock` (`api-semi.md` owns the package and its chain order) skins the dock roster from one `Controls/_index.axaml` merge over `Themes/{Light,Dark,Shared}` dictionaries; the `Dock*` keys below are the override seats a shell writes.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `DockSurfaceWorkbenchBrush` / `DockSeparatorBrush`                         | workbench fill, tab/strip separator                      |
|  [02]   | `DockTargetIndicatorBackground` / `DockTargetDropAdornerShape*`            | drop-target indicator and adorner fill, border, margin   |
|  [03]   | `DockDialog*Brush`                                                         | dialog/confirmation palette                              |
|  [04]   | `DockDialog{MinWidth,MaxWidth,CornerRadius,Padding,Spacing,TitleFontSize}` | dialog metrics                                           |
|  [05]   | `DockSelectorOverlay*` / `DockConfirmationDialog*`                         | selector-overlay and confirmation metrics                |
|  [06]   | `DockOverlay{Dialog,Confirmation,Busy}LayerTheme` / `DockOverlay*`         | overlay-layer `ControlTheme`s, card and progress metrics |
|  [07]   | `DockCommandBar{Padding,Spacing}` / `DockHeaderContentPadding`             | command-bar and tab-header metrics                       |
|  [08]   | `DockDefaultOverlayLayers` / `Dock{Dialog,Confirmation}RequestTemplate`    | default layer set and request `DataTemplate`s            |

- `DockSurfaceWorkbenchBrush` and `DockSeparatorBrush` resolve as `DynamicResource` yet no shipped dictionary defines them: the shell supplies both or the bound brush stays unset.
- Every other brush key resolves to a `SemiColor*` slot, so an OKLCH palette override re-tints dock chrome with no dock-side edit; `Themes/Light` and `Themes/Dark` redefine the same key set, so a `ThemeVariant` flip re-resolves them whole.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Panel arrangement is an `IDock` graph bound through `DockControl.Layout`; the `Factory` docking operations mutate that graph, `DockControl.DockManager` (`IDockManager`) validates every drag/drop, and data templates resolve `Document`/`Tool` view-models to views, auto-generated under `AutoCreateDataTemplates`. Visual tree, floating hosts, and overlays render the graph, never authoring it.

[STACKING]:
- `Dock.Serializer.SystemTextJson`(`.api/api-dock-serializer.md`): `DockSerializer.Save<IRootDock>(Stream)`/`Load<IRootDock>` round-trips the `IFactory` graph and registries over the `IDockState.Save(IDock)`/`Restore(IDock)` snapshot, discriminating the `IDockable`/`IDock`/`IRootDock` tree by `$type` and rehydrating through `DockableLocator` and `RestoreDockable`.
- `Semi.Avalonia`(`.api/api-semi.md`): `DockSemiTheme : Styles` loads after `SemiTheme` and resolves every `Dock*` brush key against a `SemiColor*` slot, so the `Wacton.Unicolour` OKLCH ramp writing those slots re-tints dock chrome without a dock-side edit.
- Shell composition (`.planning/Shell/`): the AppUi Shell binds one `IDock` graph per screen through `DockControl.Layout`, mounts host surfaces via `IExternalDockSurface`, and marshals graph mutation onto the Avalonia UI thread through `IDockDispatcher.InvokeAsync`.

[LOCAL_ADMISSION]:
- `Dock.Model.ReactiveUI` is the one admitted binding; a `Dock.Model.Avalonia`/`Dock.Model.Mvvm` parallel binding, a hand-rolled `INotifyPropertyChanged` model duplicating `DockableBase`, or a bespoke serializer replicating the `Dock.Serializer` polymorphism is rejected.

[RAIL_LAW]:
- Package: `Dock.Avalonia`
- Owns: the dock visual tree, drag/drop targets and the drag ghost, both float hosts (`HostWindow` native, `ManagedHostWindow` in-app under `ManagedWindowLayer`), pinned strips and their preview reveal, selector and dialog/confirmation/busy overlays through `OverlayHost`, command-bar merge, MDI, and the `IDockThemeManager` preset switch.
- Accept: an `IDock` graph bound through `DockControl.Layout`, mutated by the `IFactory` operations under `IDockManager` validation; chrome and skin variation seated on `ToolChromeControl` `ControlTheme` slots and the `Dock*` resource keys.
- Reject: hand-built splitter/tab arrangement, graph mutation through view manipulation, a second selector/overlay layer beside `DockSelectorOverlay`/`OverlayHost`, a bespoke float-host picker beside `DockFloatingWindowHostMode`.
- Package: `Dock.Model.ReactiveUI`
- Owns: the `ReactiveObject` binding of the dock model graph and the `Factory : FactoryBase` override constructing ReactiveUI-typed `RootDock`/`DocumentDock`/`ToolDock`/`Document`/`Tool` with the inherited `IFactory` operations and registries.
- Accept: layout state in factory-created models; `DocumentDock.CanCreateDocument`/`CreateDocument` drives runtime document spawning; observed-property reactivity drives the bound `DockControl` without manual invalidation.
- Reject: view-layer mutation of dock structure outside the factory surface.
