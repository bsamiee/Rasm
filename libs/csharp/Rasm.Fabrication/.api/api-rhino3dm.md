# [RASM_FABRICATION_API_RHINO3DM]

`Rhino3dm` (MIT) owns the pure-managed `File3dm` read/write surface and the `Rhino.Geometry.*` carriers crossing the `.3dm` boundary — the Fabrication file-transfer lane, binary-distinct from the RhinoCommon host lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Rhino3dm`
- package: `Rhino3dm` (`MIT`, McNeel)
- assembly: `Rhino3dm` (`lib/net7.0/Rhino3dm.dll` with the RID-keyed native `librhino3dm_native`)
- namespace: `Rhino.FileIO`, `Rhino.Geometry`, `Rhino.DocObjects`
- rail: fabrication-3dm

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file and document tables

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :--------------------- | :------------ | :----------------------------------------- |
|  [01]   | `Rhino.FileIO.File3dm` | class         | root 3dm document, read/write table owner  |
|  [02]   | `File3dmWriteOptions`  | class         | versioned write policy                     |
|  [03]   | `File3dmObject`        | class         | geometry plus object attributes            |
|  [04]   | `File3dmObjectTable`   | class         | document object collection and add surface |
|  [05]   | `File3dmLayerTable`    | class         | layer table                                |
|  [06]   | `File3dmMaterialTable` | class         | material table                             |
|  [07]   | `File3dmDimStyleTable` | class         | dimension style table                      |
|  [08]   | `ObjectAttributes`     | class         | object metadata, layer/material assignment |

[PUBLIC_TYPE_SCOPE]: geometry carriers crossing the 3dm boundary

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :-------------- | :------------ | :------------------------------ |
|  [01]   | `GeometryBase`  | class         | abstract file geometry root     |
|  [02]   | `Curve`         | class         | curve root                      |
|  [03]   | `NurbsCurve`    | class         | NURBS curve carrier             |
|  [04]   | `PolylineCurve` | class         | polyline curve carrier          |
|  [05]   | `Mesh`          | class         | mesh carrier                    |
|  [06]   | `Brep`          | class         | boundary-representation carrier |
|  [07]   | `Point3d`       | struct        | 3D point value                  |
|  [08]   | `Vector3d`      | struct        | 3D vector value                 |
|  [09]   | `Plane`         | struct        | plane value                     |
|  [10]   | `Transform`     | struct        | 4x4 transform value             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file read and write

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `File3dm.Read(string)`                                            | static   | opens a 3dm document              |
|  [02]   | `File3dm.Read(string, TableTypeFilter, ObjectTypeFilter)`         | static   | opens selected tables and objects |
|  [03]   | `File3dm.ReadWithLog(string, out string)`                         | static   | opens with diagnostic log output  |
|  [04]   | `File3dm.Write(string, int)`                                      | instance | writes by target 3dm version      |
|  [05]   | `File3dm.Write(string, File3dmWriteOptions)`                      | instance | writes by options carrier         |
|  [06]   | `File3dm.WriteWithLog(string, File3dmWriteOptions, out string)`   | instance | writes with diagnostic log output |
|  [07]   | `File3dm.WriteOneObject(string, GeometryBase)`                    | static   | writes one geometry object        |
|  [08]   | `File3dm.WriteMultipleObjects(string, IEnumerable<GeometryBase>)` | static   | writes multiple geometry objects  |

[ENTRYPOINT_SCOPE]: object table additions

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `File3dmObjectTable.Add(GeometryBase, ObjectAttributes)` | instance | polymorphic add of any geometry carrier |
|  [02]   | `AddPoint(Point3d)`                                      | instance | add a point                             |
|  [03]   | `AddPointCloud(PointCloud)`                              | instance | add a point cloud                       |
|  [04]   | `AddPoints(IEnumerable<Point3d>)`                        | instance | add a point batch                       |

[ENTRYPOINT_SCOPE]: solid-ingress projection

| [INDEX] | [SURFACE]                          | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------- | :------- | :---------------------------------- |
|  [01]   | `File3dm.Settings.ModelUnitSystem` | property | declared model-unit evidence        |
|  [02]   | `BrepFace.GetMesh(MeshType)`       | instance | stored face-mesh retrieval          |
|  [03]   | `Mesh.Vertices.ToPoint3dArray()`   | instance | copy vertices out of native carrier |
|  [04]   | `Mesh.Faces.ToIntArray(bool)`      | instance | copy triangulated face indices      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Rasm.Fabrication` references `Rhino3dm` under `Aliases="R3"`, seating every symbol behind `extern alias R3`; a type crosses the `.3dm` boundary through `R3::Rhino.*` at ingress and egress alone, and the two `Rhino.Geometry` identities stay binary-distinct.

[STACKING]:
- `Robots`(`.api/api-robots.md`): Rhino3dm's `Rhino.Geometry.*` (`Plane`, `Mesh`, `Point3d`, `Transform`, `Interval`) is the Robots kinematics geometry substrate; `plan-cs` maps a pose to a `Rhino3dm` `Plane`/`double[]` joint vector at the kinematics seam and reads `KinematicSolution` back, never passing a RhinoCommon instance into a `Robots` parameter.
- Fabrication `.3dm` ingress lowers `File3dmObjectTable` geometry into folder-owned carriers; egress folds `GeometryBase` through `File3dmObjectTable.Add` and serializes through `File3dm.Write`.

[LOCAL_ADMISSION]:
- `.3dm` import reads `File3dm` and lowers object-table geometry into Fabrication carriers; solid admission reads `File3dm.Settings.ModelUnitSystem`, detaches native `Mesh` vertices and faces through `ToPoint3dArray` and `ToIntArray(true)`, and admits a `Brep` only through the stored face meshes from `BrepFace.GetMesh(MeshType.Any)`.
- `.3dm` export builds a `File3dm`, adds `GeometryBase` through `File3dmObjectTable.Add`, and writes through `File3dm.Write`.

[RAIL_LAW]:
- Package: `Rhino3dm`
- Owns: pure-managed `File3dm` read/write, the document tables, object attributes, and the `Rhino.Geometry.*` carriers crossing the `.3dm` boundary
- Accept: file boundary import and export over `File3dm` and `R3::Rhino.Geometry.*`
- Reject: RhinoCommon host-document lifecycle, live Rhino object identity, an unaliased Rhino3dm reference inside Fabrication
