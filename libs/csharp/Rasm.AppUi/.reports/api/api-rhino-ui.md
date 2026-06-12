# [RASM_APPUI_API_RHINO_UI]

`Rhino.UI` supplies Rhino panels, dock bars, Eto integration, modal host windows, UI-thread dispatch, and panel lifecycle events.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Rhino.UI`
- package: `Rhino.UI`
- assembly: `Rhino.UI`
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
|   [3]   | `DockBar`             | dock container      |
|   [4]   | `RhinoEtoApp`         | Eto application     |
|   [5]   | `EtoExtensions`       | Eto bridge          |
|   [6]   | `IWindow`             | host window         |
|   [7]   | `ICollapsibleSection` | collapsible section |

[PANEL_EVENT_TYPES]: panel lifecycle events
- rail: host-rhino-ui

| [INDEX] | [SYMBOL]                   | [RAIL]           |
| :-----: | :------------------------- | :--------------- |
|   [1]   | `DockBarAdded`             | dockbar add      |
|   [2]   | `DockBarRemoved`           | dockbar remove   |
|   [3]   | `PanelVisibilityChanged`   | panel visibility |
|   [4]   | `CurrentPanelChanged`      | active panel     |
|   [5]   | `WantReturnInPanelChanged` | keyboard policy  |

## [3]-[ENTRYPOINTS]

[PANEL_ENTRYPOINTS]: panel and dockbar operations
- rail: host-rhino-ui

| [INDEX] | [SURFACE]        | [SURFACE_ROOT] | [RAIL]          |
| :-----: | :--------------- | :------------- | :-------------- |
|   [1]   | `RegisterPanel`  | `Panels`       | panel register  |
|   [2]   | `OpenPanel`      | `Panels`       | panel open      |
|   [3]   | `ClosePanel`     | `Panels`       | panel close     |
|   [4]   | `PanelShown`     | `Panels`       | visibility      |
|   [5]   | `DockBarsFromId` | dockbar API    | dockbar lookup  |
|   [6]   | `OpenDockBarTab` | dockbar API    | dockbar tab     |
|   [7]   | `GetPanelDoc`    | panel API      | document lookup |

[ETO_ENTRYPOINTS]: Eto and modal host operations
- rail: host-rhino-ui

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]  | [RAIL]       |
| :-----: | :----------------- | :-------------- | :----------- |
|   [1]   | `MainWindow`       | `RhinoEtoApp`   | host window  |
|   [2]   | `ShowSemiModal`    | `RhinoEtoApp`   | semi-modal   |
|   [3]   | `InvokeOnUiThread` | `RhinoEtoApp`   | UI dispatch  |
|   [4]   | `ToEto`            | `EtoExtensions` | Eto bridge   |
|   [5]   | `ToRhino`          | `EtoExtensions` | Rhino bridge |

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
