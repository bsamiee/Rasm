# [RASM_APPUI_API_RHINO]

`RhinoCommon` supplies Rhino document, view, geometry, command, object table, selection, display, and viewport capture surfaces used by host-aware AppUi adapters.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `RhinoCommon`
- package: `RhinoCommon`
- assembly: `RhinoCommon`
- namespace: `Rhino`
- namespace: `Rhino.Commands`
- namespace: `Rhino.Display`
- namespace: `Rhino.DocObjects`
- namespace: `Rhino.Geometry`
- namespace: `Rhino.Input`
- asset: host assembly
- rail: host-rhino

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, app, and command surface
- rail: host-rhino

| [INDEX] | [SYMBOL]    | [CAPABILITY]     |
| :-----: | :---------- | :--------------- |
|   [1]   | `RhinoDoc`  | document root    |
|   [2]   | `RhinoApp`  | application root |
|   [3]   | `Command`   | command base     |
|   [4]   | `RhinoGet`  | input utility    |
|   [5]   | `Result`    | command result   |
|   [6]   | `GetObject` | object selection |
|   [7]   | `GetPoint`  | point selection  |

[PUBLIC_TYPE_SCOPE]: view, viewport, display, and capture surface
- rail: host-rhino

| [INDEX] | [SYMBOL]          | [CAPABILITY]      |
| :-----: | :---------------- | :---------------- |
|   [1]   | `RhinoView`       | viewport control  |
|   [2]   | `RhinoViewport`   | viewport state    |
|   [3]   | `ViewTable`       | view collection   |
|   [4]   | `DisplayPipeline` | display callback  |
|   [5]   | `DisplayConduit`  | display conduit   |
|   [6]   | `DisplayMaterial` | display material  |
|   [7]   | `ViewCapture`     | viewport capture  |
|   [8]   | `ViewportInfo`    | viewport snapshot |

[PUBLIC_TYPE_SCOPE]: object and geometry surface
- rail: host-rhino

| [INDEX] | [SYMBOL]           | [CAPABILITY]         |
| :-----: | :----------------- | :------------------- |
|   [1]   | `RhinoObject`      | document object      |
|   [2]   | `ObjRef`           | object reference     |
|   [3]   | `ObjectAttributes` | object attributes    |
|   [4]   | `ObjectTable`      | object table         |
|   [5]   | `Layer`            | layer record         |
|   [6]   | `GeometryBase`     | geometry root        |
|   [7]   | `Brep`             | boundary geometry    |
|   [8]   | `Curve`            | curve geometry       |
|   [9]   | `Mesh`             | mesh geometry        |
|  [10]   | `SubD`             | subdivision geometry |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document and command operations
- rail: host-rhino

| [INDEX] | [SURFACE]          | [SURFACE_ROOT] | [CAPABILITY]      |
| :-----: | :----------------- | :------------- | :---------------- |
|   [1]   | `ActiveDoc`        | `RhinoDoc`     | active document   |
|   [2]   | `InvokeOnUiThread` | `RhinoApp`     | UI dispatch       |
|   [3]   | `WriteLine`        | `RhinoApp`     | host output       |
|   [4]   | `RunScript`        | `RhinoApp`     | script execution  |
|   [5]   | `RunCommand`       | `Command`      | command execution |
|   [6]   | `Objects`          | `RhinoDoc`     | object table      |
|   [7]   | `Layers`           | `RhinoDoc`     | layer table       |
|   [8]   | `Views`            | `RhinoDoc`     | view table        |

[ENTRYPOINT_SCOPE]: view and display operations
- rail: host-rhino

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]   | [CAPABILITY]    |
| :-----: | :--------------------- | :--------------- | :-------------- |
|   [1]   | `Redraw`               | `RhinoView`      | viewport redraw |
|   [2]   | `ActiveViewport`       | `RhinoView`      | viewport state  |
|   [3]   | `DrawForeground`       | `DisplayConduit` | overlay pass    |
|   [4]   | `DrawOverlay`          | `DisplayConduit` | overlay pass    |
|   [5]   | `CalculateBoundingBox` | `DisplayConduit` | bounds pass     |
|   [6]   | `CaptureToBitmap`      | `ViewCapture`    | bitmap capture  |

[ENTRYPOINT_SCOPE]: object and geometry operations
- rail: host-rhino

| [INDEX] | [SURFACE]           | [SURFACE_ROOT] | [CAPABILITY]      |
| :-----: | :------------------ | :------------- | :---------------- |
|   [1]   | `Find`              | `ObjectTable`  | object lookup     |
|   [2]   | `Add`               | `ObjectTable`  | object add        |
|   [3]   | `Delete`            | `ObjectTable`  | object delete     |
|   [4]   | `ModifyAttributes`  | `ObjectTable`  | attribute update  |
|   [5]   | `Select`            | `RhinoObject`  | object selection  |
|   [6]   | `DuplicateGeometry` | `RhinoObject`  | geometry clone    |
|   [7]   | `Object`            | `ObjRef`       | reference resolve |

## [4]-[IMPLEMENTATION_LAW]

[HOST_BOUNDARY_LAW]:
- Package: `RhinoCommon`
- Owns: Rhino document, view, command, object, geometry, display, and capture adapter surface
- Accept: host APIs stay behind boundary adapters and emit typed UI receipts
- Reject: Rhino vocabulary in UI model

[EVIDENCE_LAW]:
- Package: `RhinoCommon`
- Owns: viewport redraw, capture, object selection, document lookup, and host-thread dispatch proof
- Accept: AppUi host surfaces carry Rhino evidence without exposing Rhino types as product UI contracts
- Reject: direct Rhino API calls from shell, screen, command, theme, or live-state rails
