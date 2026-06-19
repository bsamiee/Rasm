# [RASM_APPUI_API_DOCK]

`Dock.Avalonia` and `Dock.Model.ReactiveUI` supply docking controls, host windows, dock targets, selector overlays, and the ReactiveUI dock model with factory-created layouts, docks, documents, and tools.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Dock.Avalonia`
- package: `Dock.Avalonia`
- assembly: `Dock.Avalonia`
- namespace: `Dock.Avalonia.Controls`
- namespace: `Dock.Avalonia.Selectors`
- namespace: `Dock.Avalonia.Themes`
- namespace: `Dock.Avalonia.Mdi`
- asset: runtime library
- rail: docking

[PACKAGE_SURFACE]: `Dock.Model.ReactiveUI`
- package: `Dock.Model.ReactiveUI`
- assembly: `Dock.Model.ReactiveUI`
- namespace: `Dock.Model.ReactiveUI`
- namespace: `Dock.Model.ReactiveUI.Core`
- namespace: `Dock.Model.ReactiveUI.Controls`
- asset: runtime library
- rail: docking

## [02]-[PUBLIC_TYPES]

[DOCK_CONTROLS]: docking visual tree
- rail: docking

| [INDEX] | [SYMBOL]                  | [RAIL]            |
| :-----: | :------------------------ | :---------------- |
|  [01]   | `DockControl`             | dock root control |
|  [02]   | `RootDockControl`         | root view         |
|  [03]   | `ProportionalDockControl` | split view        |
|  [04]   | `DocumentDockControl`     | document dock     |
|  [05]   | `ToolDockControl`         | tool dock         |
|  [06]   | `DocumentControl`         | document host     |
|  [07]   | `ToolControl`             | tool host         |
|  [08]   | `DocumentTabStrip`        | document tabs     |
|  [09]   | `ToolTabStrip`            | tool tabs         |
|  [10]   | `ToolChromeControl`       | tool chrome       |
|  [11]   | `PinnedDockControl`       | pinned dock       |
|  [12]   | `ToolPinnedControl`       | pinned tools      |

[WINDOW_AND_TARGET_TYPES]: floating windows, drop targets, and selectors
- rail: docking

| [INDEX] | [SYMBOL]              | [RAIL]             |
| :-----: | :-------------------- | :----------------- |
|  [01]   | `HostWindow`          | floating host      |
|  [02]   | `HostWindowTitleBar`  | host chrome        |
|  [03]   | `DockTarget`          | local drop target  |
|  [04]   | `GlobalDockTarget`    | global drop target |
|  [05]   | `DockableControl`     | dockable region    |
|  [06]   | `DragPreviewControl`  | drag preview       |
|  [07]   | `DockSelectorOverlay` | selector overlay   |
|  [08]   | `DockSelectorMode`    | selector mode      |
|  [09]   | `IDockThemeManager`   | theme contract     |
|  [10]   | `IMdiLayoutManager`   | MDI layout         |

[MODEL_TYPES]: ReactiveUI dock model
- rail: docking

| [INDEX] | [SYMBOL]                   | [RAIL]            |
| :-----: | :------------------------- | :---------------- |
|  [01]   | `Factory`                  | model factory     |
|  [02]   | `DockableBase`             | dockable base     |
|  [03]   | `DockBase`                 | dock base         |
|  [04]   | `DockWindow`               | window model      |
|  [05]   | `RootDock`                 | root model        |
|  [06]   | `ProportionalDock`         | split model       |
|  [07]   | `ProportionalDockSplitter` | splitter model    |
|  [08]   | `DocumentDock`             | document dock     |
|  [09]   | `ToolDock`                 | tool dock         |
|  [10]   | `Document`                 | document model    |
|  [11]   | `Tool`                     | tool model        |
|  [12]   | `IDockDispatcher`          | dispatch contract |

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: dock control wiring
- rail: docking

| [INDEX] | [SURFACE]                       | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :------------------------------ | :------------- | :---------------- |
|  [01]   | `Layout`                        | `DockControl`  | layout input      |
|  [02]   | `Factory`                       | `DockControl`  | factory binding   |
|  [03]   | `InitializeLayout`              | `DockControl`  | auto init         |
|  [04]   | `InitializeFactory`             | `DockControl`  | factory init      |
|  [05]   | `DefaultContext`                | `DockControl`  | context fallback  |
|  [06]   | `IsDockingEnabled`              | `DockControl`  | drag gate         |
|  [07]   | `HostWindowFactory`             | `DockControl`  | float window kind |
|  [08]   | `EnableManagedWindowLayer`      | `DockControl`  | managed floats    |
|  [09]   | `DockManager`                   | `DockControl`  | manager access    |
|  [10]   | `RegisterExternalDockSurface`   | `DockControl`  | external surface  |
|  [11]   | `ShowSelector` / `HideSelector` | `DockControl`  | selector overlay  |

[FACTORY_ENTRYPOINTS]: ReactiveUI model construction
- rail: docking

| [INDEX] | [SURFACE]                        | [SURFACE_ROOT] | [RAIL]          |
| :-----: | :------------------------------- | :------------- | :-------------- |
|  [01]   | `CreateLayout`                   | `Factory`      | layout root     |
|  [02]   | `CreateRootDock`                 | `Factory`      | root dock       |
|  [03]   | `CreateProportionalDock`         | `Factory`      | split dock      |
|  [04]   | `CreateProportionalDockSplitter` | `Factory`      | splitter        |
|  [05]   | `CreateDocumentDock`             | `Factory`      | document dock   |
|  [06]   | `CreateToolDock`                 | `Factory`      | tool dock       |
|  [07]   | `CreateDocument`                 | `Factory`      | document        |
|  [08]   | `CreateTool`                     | `Factory`      | tool            |
|  [09]   | `CreateDockWindow`               | `Factory`      | float window    |
|  [10]   | `CreateList`                     | `Factory`      | dockable list   |
|  [11]   | `DockControls` / `HostWindows`   | `Factory`      | live registries |

## [04]-[IMPLEMENTATION_LAW]

[DOCKING_LAW]:
- Package: `Dock.Avalonia`
- Owns: dock visual tree, drag/drop targets, floating host windows, pinned docks, and selector overlays
- Accept: panel arrangement intent maps to dock layouts bound through `DockControl.Layout`
- Reject: hand-built splitter/tab arrangements for dockable panels

[MODEL_LAW]:
- Package: `Dock.Model.ReactiveUI`
- Owns: the reactive dock model graph and its factory construction
- Accept: layout state lives in factory-created `RootDock`/`DocumentDock`/`ToolDock` models
- Reject: view-layer mutation of dock structure outside the factory surface
