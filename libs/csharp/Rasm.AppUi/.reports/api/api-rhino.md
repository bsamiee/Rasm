# [RASM_APPUI_API_RHINO]

`RhinoCommon` supplies Rhino document, view, geometry, command, and display surfaces used by host-aware AppUi adapters.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `RhinoCommon`
- host_assembly: `RhinoCommon`
- assembly: `RhinoCommon`
- namespace: `Rhino`
- asset: host assembly
- rail: host-rhino

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: host API family
- rail: host-rhino

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]  | [CAPABILITY]                |
| :-----: | :---------------- | :-------------- | :-------------------------- |
|   [1]   | `RhinoDoc`        | rail contract   | anchors host-rhino contract |
|   [2]   | `RhinoApp`        | rail contract   | anchors host-rhino contract |
|   [3]   | `RhinoView`       | UI surface      | renders product surface     |
|   [4]   | `DisplayPipeline` | rail contract   | anchors host-rhino contract |
|   [5]   | `ViewCapture`     | UI surface      | renders product surface     |
|   [6]   | `RhinoObject`     | rail contract   | anchors host-rhino contract |
|   [7]   | `GeometryBase`    | rail contract   | anchors host-rhino contract |
|   [8]   | `Command`         | command surface | executes user intent        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: host operations
- rail: host-rhino

| [INDEX] | [SURFACE]          | [CALL_SHAPE]     | [CAPABILITY]               |
| :-----: | :----------------- | :--------------- | :------------------------- |
|   [1]   | `ActiveDoc`        | member surface   | drives host-rhino behavior |
|   [2]   | `InvokeOnUiThread` | member surface   | drives host-rhino behavior |
|   [3]   | `WriteLine`        | member surface   | drives host-rhino behavior |
|   [4]   | `Views`            | view collection  | selects Rhino views        |
|   [5]   | `Redraw`           | redraw command   | requests viewport draw     |
|   [6]   | `DrawForeground`   | display callback | draws overlay pass         |
|   [7]   | `CaptureToBitmap`  | rendering call   | captures viewport evidence |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: Rhino host adapter surface
- Accept: host APIs stay behind adapters
- Reject: Rhino vocabulary in UI model

