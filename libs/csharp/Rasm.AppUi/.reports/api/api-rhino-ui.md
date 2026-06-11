# [RASM_APPUI_API_RHINO_UI]

`Rhino.UI` supplies Rhino panels, dock bars, Eto integration, and host UI handles.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Rhino.UI`
- host_assembly: `Rhino.UI`
- assembly: `Rhino.UI`
- namespace: `Rhino.UI`
- asset: host assembly
- rail: host-rhino-ui

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: host UI family
- rail: host-rhino-ui

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                   |
| :-----: | :-------------- | :------------- | :----------------------------- |
|   [1]   | `Panels`        | UI surface     | renders product surface        |
|   [2]   | `PanelType`     | UI surface     | renders product surface        |
|   [3]   | `DockBar`       | rail contract  | anchors host-rhino-ui contract |
|   [4]   | `RhinoEtoApp`   | rail contract  | anchors host-rhino-ui contract |
|   [5]   | `EtoExtensions` | rail contract  | anchors host-rhino-ui contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: host UI operations
- rail: host-rhino-ui

| [INDEX] | [SURFACE]          | [CALL_SHAPE]     | [CAPABILITY]                  |
| :-----: | :----------------- | :--------------- | :---------------------------- |
|   [1]   | `RegisterPanel`    | member surface   | drives host-rhino-ui behavior |
|   [2]   | `OpenPanel`        | operation call   | executes operation            |
|   [3]   | `ClosePanel`       | member surface   | drives host-rhino-ui behavior |
|   [4]   | `ShowSemiModal`    | member surface   | drives host-rhino-ui behavior |
|   [5]   | `MainWindow`       | property surface | binds surface state           |
|   [6]   | `InvokeOnUiThread` | member surface   | drives host-rhino-ui behavior |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Rhino.UI`
- Owns: Rhino panel integration
- Accept: panel handles emit diagnostics
- Reject: native handles in public model

