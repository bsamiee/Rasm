# [RASM_FABRICATION_API_RHINO3DM]

`Rhino3dm` (MIT) is the Fabrication 3dm-file boundary. `Rasm.Fabrication` references it with `Aliases="R3"`, so `extern alias R3` keeps Rhino3dm file-transfer types distinct from RhinoCommon host types while the package supplies the pure-managed `File3dm` read/write surface and geometry carriers crossing the `.3dm` boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Rhino3dm`
- package: `Rhino3dm`
- license: MIT
- assembly: `Rhino3dm`
- namespace: `Rhino.FileIO`, `Rhino.Geometry`, `Rhino.DocObjects`
- asset: `lib/net7.0/Rhino3dm.dll` plus package native assets
- rail: fabrication-3dm

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file and document tables
- rail: fabrication-3dm

| [INDEX] | [SYMBOL]                 | [CAPABILITY]                               |
| :-----: | :----------------------- | :---------------------------------------- |
|  [01]   | `Rhino.FileIO.File3dm`   | root 3dm document, read/write table owner  |
|  [02]   | `File3dmWriteOptions`    | versioned write policy                     |
|  [03]   | `File3dmObject`          | geometry plus object attributes            |
|  [04]   | `File3dmObjectTable`     | document object collection and add surface |
|  [05]   | `File3dmLayerTable`      | layer table                                |
|  [06]   | `File3dmMaterialTable`   | material table                             |
|  [07]   | `File3dmDimStyleTable`   | dimension style table                      |
|  [08]   | `ObjectAttributes`       | object metadata, layer/material assignment |

[PUBLIC_TYPE_SCOPE]: geometry carriers crossing the 3dm boundary
- rail: fabrication-3dm

| [INDEX] | [SYMBOL]        | [CAPABILITY]                    |
| :-----: | :-------------- | :------------------------------ |
|  [01]   | `GeometryBase`  | abstract file geometry root      |
|  [02]   | `Curve`         | curve root                       |
|  [03]   | `NurbsCurve`    | NURBS curve carrier              |
|  [04]   | `PolylineCurve` | polyline curve carrier           |
|  [05]   | `Mesh`          | mesh carrier                     |
|  [06]   | `Brep`          | boundary-representation carrier  |
|  [07]   | `Point3d`       | 3D point value                   |
|  [08]   | `Vector3d`      | 3D vector value                  |
|  [09]   | `Plane`         | plane value                      |
|  [10]   | `Transform`     | 4x4 transform value              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file read and write
- rail: fabrication-3dm

| [INDEX] | [SURFACE]                                                          | [CALL_SHAPE] | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------- | :----------- | :-------------------------------- |
|  [01]   | `File3dm.Read(string)`                                             | static read  | opens a 3dm document              |
|  [02]   | `File3dm.Read(string, TableTypeFilter, ObjectTypeFilter)`          | static read  | opens selected tables and objects |
|  [03]   | `File3dm.ReadWithLog(...)`                                         | static read  | opens with diagnostic log output  |
|  [04]   | `File3dm.Write(string, int)`                                       | instance     | writes by target 3dm version      |
|  [05]   | `File3dm.Write(string, File3dmWriteOptions)`                       | instance     | writes by options carrier         |
|  [06]   | `File3dm.WriteWithLog(string, File3dmWriteOptions, out string)`    | instance     | writes with diagnostic log output |
|  [07]   | `File3dm.WriteOneObject(string, GeometryBase)`                     | static write | writes one geometry object        |
|  [08]   | `File3dm.WriteMultipleObjects(string, IEnumerable<GeometryBase>)`  | static write | writes multiple geometry objects  |

[ENTRYPOINT_SCOPE]: object table additions
- rail: fabrication-3dm

| [INDEX] | [SURFACE]                                             | [CALL_SHAPE] | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------- | :----------- | :--------------------------- |
|  [01]   | `File3dmObjectTable.Add(GeometryBase, ObjectAttributes)` | table add | add any geometry with attributes |
|  [02]   | `AddPoint(Point3d)`                                   | table add    | add a point                  |
|  [03]   | `AddPolyline(IEnumerable<Point3d>)`                   | table add    | add a polyline               |
|  [04]   | `AddCurve(Curve)`                                     | table add    | add a curve                  |
|  [05]   | `AddMesh(Mesh)`                                       | table add    | add a mesh                   |
|  [06]   | `AddBrep(Brep)`                                       | table add    | add a BRep                   |
|  [07]   | `AddLine(Point3d, Point3d)`                           | table add    | add a line                   |

## [04]-[IMPLEMENTATION_LAW]

[ALIAS_BOUNDARY]:
- `Aliases="R3"` keeps Rhino3dm symbols behind `extern alias R3`.
- Fabrication treats Rhino3dm as the file-transfer lane; RhinoCommon remains the host lane.
- A type crossing the 3dm boundary is projected through the `R3::Rhino.*` surface at ingress and egress only.

[LOCAL_ADMISSION]:
- `.3dm` import reads `File3dm` and lowers object-table geometry into Fabrication-owned carriers.
- `.3dm` export builds a `File3dm`, adds `GeometryBase` instances through `File3dmObjectTable`, and writes through `File3dm.Write`.
- Host-bound RhinoCommon objects never leak through this catalog; the alias boundary keeps the two type identities explicit.

[RAIL_LAW]:
- Package: `Rhino3dm`
- Owns: pure-managed 3dm file read/write, document tables, object attributes, and Rhino3dm geometry carriers
- Accept: file boundary import/export over `File3dm` and `R3::Rhino.Geometry.*`
- Reject: RhinoCommon host-document lifecycle, live Rhino object identity, or unaliased Rhino3dm references inside Fabrication
