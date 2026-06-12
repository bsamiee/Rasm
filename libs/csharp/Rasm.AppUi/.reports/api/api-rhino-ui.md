# [RASM_APPUI_API_RHINO_UI]

`Rhino.UI` supplies Rhino panels, dock bars, Eto integration, modal host windows, UI-thread dispatch, and panel lifecycle events.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Rhino.UI`
- package: `Rhino.UI`
- assembly: `Rhino.UI` (Eto bridge surfaces)
- assembly: `RhinoCommon` (panel registry types in the `Rhino.UI` namespace)
- namespace: `Rhino.UI`
- namespace: `Rhino.UI.Controls`
- asset: host assembly
- rail: host-rhino-ui

## [2]-[PUBLIC_TYPES]

[PANEL_TYPES]: panel and dockbar surface
- rail: host-rhino-ui

| [INDEX] | [SYMBOL]              | [RAIL]              |
| :-----: | :-------------------- | :------------------ |
|   [1]   | `Panels`              | panel registry      |
|   [2]   | `PanelType`           | panel identity      |
|   [3]   | `StackedDialogPage`   | stacked page        |
|   [4]   | `RhinoEtoApp`         | Eto application     |
|   [5]   | `EtoExtensions`       | Eto bridge          |
|   [6]   | `IWindow`             | host window         |
|   [7]   | `ICollapsibleSection` | collapsible section |

[PANEL_EVENT_TYPES]: panel lifecycle events
- rail: host-rhino-ui

| [INDEX] | [SYMBOL]             | [RAIL]        |
| :-----: | :------------------- | :------------ |
|   [1]   | `Show`               | show event    |
|   [2]   | `Closed`             | close event   |
|   [3]   | `ShowPanelEventArgs` | show payload  |
|   [4]   | `PanelEventArgs`     | panel payload |
|   [5]   | `ShowPanelReason`    | show reason   |

## [3]-[ENTRYPOINTS]

[PANEL_ENTRYPOINTS]: panel and dockbar operations
- rail: host-rhino-ui

| [INDEX] | [SURFACE]            | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :------------------- | :------------- | :------------- |
|   [1]   | `RegisterPanel`      | `Panels`       | panel register |
|   [2]   | `OpenPanel`          | `Panels`       | panel open     |
|   [3]   | `ClosePanel`         | `Panels`       | panel close    |
|   [4]   | `IsPanelVisible`     | `Panels`       | visibility     |
|   [5]   | `PanelDockBar`       | `Panels`       | dockbar lookup |
|   [6]   | `OpenPanelAsSibling` | `Panels`       | sibling tab    |
|   [7]   | `GetPanel`           | `Panels`       | panel lookup   |

[ETO_ENTRYPOINTS]: Eto and modal host operations
- rail: host-rhino-ui

| [INDEX] | [SURFACE]               | [SURFACE_ROOT]  | [RAIL]          |
| :-----: | :---------------------- | :-------------- | :-------------- |
|   [1]   | `MainWindow`            | `RhinoEtoApp`   | host window     |
|   [2]   | `ShowSemiModal`         | `EtoExtensions` | semi-modal      |
|   [3]   | `MainWindowForDocument` | `RhinoEtoApp`   | document window |
|   [4]   | `ToEto`                 | `EtoExtensions` | Eto bridge      |
|   [5]   | `ToSystemDrawing`       | `EtoExtensions` | drawing bridge  |

## [4]-[IMPLEMENTATION_LAW]

[PANEL_LAW]:
- Package: `Rhino.UI`
- Owns: Rhino panel registration, dockbar placement, Eto host windows, modal handoff, and UI-thread dispatch
- Accept: panel handles emit lifecycle and diagnostics receipts
- Reject: native handles in public model

[HOST_UI_BOUNDARY_LAW]:
- Package: `Rhino.UI`
- Owns: Rhino UI interop only at AppUi host boundaries
- Accept: product screens project into Rhino panels without adopting Rhino UI vocabulary
- Reject: Rhino panel APIs as shell or screen model
