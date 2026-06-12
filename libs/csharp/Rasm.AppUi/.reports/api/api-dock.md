# [RASM_APPUI_API_DOCK]

`Dock.Avalonia` and `Dock.Model.ReactiveUI` supply docking controls, host windows, dock targets, selector overlays, and the ReactiveUI dock model with factory-created layouts, docks, documents, and tools.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[DOCK_CONTROLS]: docking visual tree
- rail: docking

| [INDEX] | [SYMBOL]                  | [RAIL]            |
| :-----: | :------------------------ | :---------------- |
|   [1]   | `DockControl`             | dock root control |
|   [2]   | `RootDockControl`         | root view         |
|   [3]   | `ProportionalDockControl` | split view        |
|   [4]   | `DocumentDockControl`     | document dock     |
|   [5]   | `ToolDockControl`         | tool dock         |
|   [6]   | `DocumentControl`         | document host     |
|   [7]   | `ToolControl`             | tool host         |
|   [8]   | `DocumentTabStrip`        | document tabs     |
|   [9]   | `ToolTabStrip`            | tool tabs         |
|  [10]   | `ToolChromeControl`       | tool chrome       |
|  [11]   | `PinnedDockControl`       | pinned dock       |
|  [12]   | `ToolPinnedControl`       | pinned tools      |

[WINDOW_AND_TARGET_TYPES]: floating windows, drop targets, and selectors
- rail: docking

| [INDEX] | [SYMBOL]              | [RAIL]             |
| :-----: | :-------------------- | :----------------- |
|   [1]   | `HostWindow`          | floating host      |
|   [2]   | `HostWindowTitleBar`  | host chrome        |
|   [3]   | `DockTarget`          | local drop target  |
|   [4]   | `GlobalDockTarget`    | global drop target |
|   [5]   | `DockableControl`     | dockable region    |
|   [6]   | `DragPreviewControl`  | drag preview       |
|   [7]   | `DockSelectorOverlay` | selector overlay   |
|   [8]   | `DockSelectorMode`    | selector mode      |
|   [9]   | `IDockThemeManager`   | theme contract     |
|  [10]   | `IMdiLayoutManager`   | MDI layout         |

[MODEL_TYPES]: ReactiveUI dock model
- rail: docking

| [INDEX] | [SYMBOL]                   | [RAIL]            |
| :-----: | :------------------------- | :---------------- |
|   [1]   | `Factory`                  | model factory     |
|   [2]   | `DockableBase`             | dockable base     |
|   [3]   | `DockBase`                 | dock base         |
|   [4]   | `DockWindow`               | window model      |
|   [5]   | `RootDock`                 | root model        |
|   [6]   | `ProportionalDock`         | split model       |
|   [7]   | `ProportionalDockSplitter` | splitter model    |
|   [8]   | `DocumentDock`             | document dock     |
|   [9]   | `ToolDock`                 | tool dock         |
|  [10]   | `Document`                 | document model    |
|  [11]   | `Tool`                     | tool model        |
|  [12]   | `IDockDispatcher`          | dispatch contract |

## [3]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: dock control wiring
- rail: docking

| [INDEX] | [SURFACE]                       | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :------------------------------ | :------------- | :---------------- |
|   [1]   | `Layout`                        | `DockControl`  | layout input      |
|   [2]   | `Factory`                       | `DockControl`  | factory binding   |
|   [3]   | `InitializeLayout`              | `DockControl`  | auto init         |
|   [4]   | `InitializeFactory`             | `DockControl`  | factory init      |
|   [5]   | `DefaultContext`                | `DockControl`  | context fallback  |
|   [6]   | `IsDockingEnabled`              | `DockControl`  | drag gate         |
|   [7]   | `HostWindowFactory`             | `DockControl`  | float window kind |
|   [8]   | `EnableManagedWindowLayer`      | `DockControl`  | managed floats    |
|   [9]   | `DockManager`                   | `DockControl`  | manager access    |
|  [10]   | `RegisterExternalDockSurface`   | `DockControl`  | external surface  |
|  [11]   | `ShowSelector` / `HideSelector` | `DockControl`  | selector overlay  |

[FACTORY_ENTRYPOINTS]: ReactiveUI model construction
- rail: docking

| [INDEX] | [SURFACE]                        | [SURFACE_ROOT] | [RAIL]          |
| :-----: | :------------------------------- | :------------- | :-------------- |
|   [1]   | `CreateLayout`                   | `Factory`      | layout root     |
|   [2]   | `CreateRootDock`                 | `Factory`      | root dock       |
|   [3]   | `CreateProportionalDock`         | `Factory`      | split dock      |
|   [4]   | `CreateProportionalDockSplitter` | `Factory`      | splitter        |
|   [5]   | `CreateDocumentDock`             | `Factory`      | document dock   |
|   [6]   | `CreateToolDock`                 | `Factory`      | tool dock       |
|   [7]   | `CreateDocument`                 | `Factory`      | document        |
|   [8]   | `CreateTool`                     | `Factory`      | tool            |
|   [9]   | `CreateDockWindow`               | `Factory`      | float window    |
|  [10]   | `CreateList`                     | `Factory`      | dockable list   |
|  [11]   | `DockControls` / `HostWindows`   | `Factory`      | live registries |

## [4]-[IMPLEMENTATION_LAW]

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
