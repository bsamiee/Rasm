# [PY_GEOMETRY_API_RHINO3DM]

`rhino3dm` supplies the headless OpenNURBS surface for the geometry exchange rail: a `File3dm` document with typed object/layer/material tables, the full `GeometryBase` hierarchy (`Mesh`, `Brep`, `NurbsCurve`, `NurbsSurface`, `SubD`, `Extrusion`, `PointCloud`), value primitives (`Point3d`, `Vector3d`, `Plane`, `Transform`, `BoundingBox`), and `CommonObject` JSON serialization that round-trips geometry to and from the `.3dm` archive without a Rhino install. The package owner composes `File3dm.Read`, `File3dmObjectTable.Add*`, and `CommonObject.Encode`/`Decode` into the exchange owner; it never re-implements OpenNURBS tessellation, NURBS evaluation, or the `.3dm` binary archive.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rhino3dm`
- package: `rhino3dm`
- import: `import rhino3dm`
- owner: `geometry`
- rail: exchange
- installed: `8.17.0` reflected via `python -c "import rhino3dm"` on cp313
- entry points: none (library only)
- capability: headless `.3dm` archive read/write, typed document tables, NURBS curve/surface evaluation, mesh and SubD geometry, point clouds, geometry transforms and bounding boxes, and `CommonObject` JSON serialization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and table family
- rail: exchange

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [CAPABILITY]                                          |
| :-----: | :--------------------- | :---------------- | :---------------------------------------------------- |
|  [01]   | `File3dm`              | archive document  | read/write `.3dm`, owns all object and setting tables |
|  [02]   | `File3dmObjectTable`   | object table      | typed `Add*` geometry insertion and `FindId` lookup   |
|  [03]   | `File3dmLayerTable`    | layer table       | `AddLayer`, `FindName`, `FindIndex`, `FindId`         |
|  [04]   | `File3dmMaterialTable` | material table    | render material entries                               |
|  [05]   | `File3dmObject`        | object record     | geometry plus `ObjectAttributes` pair                 |
|  [06]   | `ObjectAttributes`     | object metadata   | layer, color, name, material source, user strings     |
|  [07]   | `Layer`                | layer record      | name, color, visibility, parent, render material      |
|  [08]   | `File3dmSettings`      | document settings | model unit system, tolerances, render settings        |

[PUBLIC_TYPE_SCOPE]: geometry hierarchy (`GeometryBase` subclasses)
- rail: exchange

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [CAPABILITY]                                         |
| :-----: | :------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `GeometryBase` | geometry root   | transform, bounding box, user strings, encode/decode |
|  [02]   | `Mesh`         | triangle mesh   | vertices/faces/normals/colors, manifold and topology |
|  [03]   | `Brep`         | boundary rep    | faces/edges/surfaces, solid and manifold queries     |
|  [04]   | `Extrusion`    | extrusion solid | profile-and-path solid, `ToBrep`/`ToNurbsSurface`    |
|  [05]   | `SubD`         | subdivision     | control net, subdivide, solid query                  |
|  [06]   | `NurbsCurve`   | NURBS curve     | control points, knots, evaluation, conversion        |
|  [07]   | `NurbsSurface` | NURBS surface   | control grid, knots, surface evaluation              |
|  [08]   | `PointCloud`   | point cloud     | points/colors/normals/values, closest-point query    |

[PUBLIC_TYPE_SCOPE]: value primitive family
- rail: exchange

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]    | [CAPABILITY]                         |
| :-----: | :------------ | :--------------- | :----------------------------------- |
|  [01]   | `Point3d`     | 3D point         | XYZ coordinate value                 |
|  [02]   | `Vector3d`    | 3D vector        | direction with unitize and dot/cross |
|  [03]   | `Point3dList` | point list       | resizable point sequence             |
|  [04]   | `Plane`       | oriented plane   | origin plus axes for frames          |
|  [05]   | `Transform`   | 4x4 transform    | affine transform applied to geometry |
|  [06]   | `BoundingBox` | axis-aligned box | min/max corner spatial bound         |
|  [07]   | `Interval`    | scalar interval  | parameter domain `T0`/`T1`           |
|  [08]   | `Polyline`    | polyline         | vertex sequence to `PolylineCurve`   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document read and write (`File3dm`)
- rail: exchange

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :--------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `File3dm.Read(path) -> File3dm`          | read           | open a `.3dm` archive from disk       |
|  [02]   | `File3dm.FromByteArray(buffer)`          | read           | decode a `.3dm` archive from bytes    |
|  [03]   | `File3dm.ReadArchiveVersion(path)`       | read           | archive version without full load     |
|  [04]   | `File3dm.ReadNotes(path)`                | read           | document notes string                 |
|  [05]   | `f.Write(path, version=0) -> bool`       | write          | write archive at OpenNURBS version    |
|  [06]   | `f.Encode() -> dict`                     | serialize      | JSON-encode the whole document        |
|  [07]   | `File3dm.Decode(jsonObject)`             | serialize      | rebuild a document from JSON          |
|  [08]   | `f.Objects` / `f.Layers` / `f.Materials` | table          | typed table accessors on the document |

[ENTRYPOINT_SCOPE]: typed object insertion (`File3dmObjectTable`)
- rail: exchange

Each `Add*` row inserts geometry under the active layer and returns the new object `Guid`; `Add(geometry, attributes)` is the polymorphic insert that takes any `GeometryBase` plus `ObjectAttributes`.

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [CAPABILITY]                |
| :-----: | :--------------------------------- | :------------- | :-------------------------- |
|  [01]   | `Add(geometry, attributes)`        | insert         | polymorphic geometry insert |
|  [02]   | `AddMesh(mesh, attributes)`        | insert         | add a `Mesh`                |
|  [03]   | `AddBrep(brep, attributes)`        | insert         | add a `Brep`                |
|  [04]   | `AddExtrusion(extrusion, attrs)`   | insert         | add an `Extrusion`          |
|  [05]   | `AddCurve(curve, attributes)`      | insert         | add any curve               |
|  [06]   | `AddPoint(point, attributes)`      | insert         | add a point object          |
|  [07]   | `AddPointCloud(cloud, attributes)` | insert         | add a `PointCloud`          |
|  [08]   | `AddSurface(surface, attributes)`  | insert         | add a surface               |
|  [09]   | `AddPolyline(polyline, attrs)`     | insert         | add a polyline              |
|  [10]   | `FindId(id) -> File3dmObject`      | lookup         | resolve an object by `Guid` |
|  [11]   | `Delete(id) -> bool`               | mutate         | remove an object by `Guid`  |

[ENTRYPOINT_SCOPE]: geometry construction and conversion
- rail: exchange

`CreateFrom*`/`Create*` are static constructors on the named class; conversion rows return a denser geometry form.

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [CAPABILITY]               |
| :-----: | :----------------------------------------- | :------------- | :------------------------- |
|  [01]   | `Brep.CreateFromBox(box) -> Brep`          | constructor    | box solid as Brep          |
|  [02]   | `Brep.CreateFromMesh(mesh, trimmed)`       | constructor    | Brep from a mesh           |
|  [03]   | `Brep.CreateFromSphere(sphere) -> Brep`    | constructor    | sphere as Brep             |
|  [04]   | `Mesh.CreateFromSubDControlNet(subd)`      | constructor    | control-net mesh from SubD |
|  [05]   | `NurbsCurve.Create(periodic, degree, pts)` | constructor    | control-point NURBS curve  |
|  [06]   | `NurbsCurve.CreateFromArc(arc)`            | constructor    | arc as NURBS curve         |
|  [07]   | `NurbsCurve.CreateFromCircle(circle)`      | constructor    | circle as NURBS curve      |
|  [08]   | `Extrusion.Create(profile, height, cap)`   | constructor    | extrude a planar profile   |
|  [09]   | `extrusion.ToBrep() -> Brep`               | conversion     | extrusion to boundary rep  |
|  [10]   | `extrusion.GetMesh(meshType) -> Mesh`      | conversion     | render mesh from extrusion |
|  [11]   | `curve.ToNurbsCurve() -> NurbsCurve`       | conversion     | any curve to NURBS form    |

[ENTRYPOINT_SCOPE]: geometry transforms and evaluation (`GeometryBase`)
- rail: exchange

Transform rows mutate the receiver in place and return a success bool; evaluation rows compute values at a curve/surface parameter.

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [CAPABILITY]                  |
| :-----: | :------------------------------------ | :------------- | :---------------------------- |
|  [01]   | `g.Transform(xform) -> bool`          | transform      | apply a 4x4 transform         |
|  [02]   | `g.Translate(vector) -> bool`         | transform      | translate by a vector         |
|  [03]   | `g.Rotate(angle, axis, center)`       | transform      | rotate about an axis          |
|  [04]   | `g.Scale(factor) -> bool`             | transform      | uniform scale                 |
|  [05]   | `g.GetBoundingBox(accurate)`          | query          | axis-aligned bound            |
|  [06]   | `curve.PointAt(t) -> Point3d`         | evaluation     | point at curve parameter      |
|  [07]   | `curve.TangentAt(t) -> Vector3d`      | evaluation     | tangent at curve parameter    |
|  [08]   | `curve.CurvatureAt(t) -> Vector3d`    | evaluation     | curvature vector at parameter |
|  [09]   | `surface.PointAt(u, v) -> Point3d`    | evaluation     | point at surface `(u,v)`      |
|  [10]   | `g.SetUserString(key, value) -> bool` | metadata       | attach a user string          |
|  [11]   | `g.Encode() -> dict`                  | serialize      | JSON-encode the geometry      |
|  [12]   | `CommonObject.Decode(jsonObject)`     | serialize      | rebuild geometry from JSON    |

## [04]-[IMPLEMENTATION_LAW]

[EXCHANGE_TOPOLOGY]:
- document axis: one `File3dm` owns every object, layer, material, group, instance-definition, and view table; geometry never lives outside a `File3dmObject`, and `File3dmObjectTable.Add*` is the only insertion surface, parameterized by geometry type and `ObjectAttributes`.
- geometry axis: every concrete geometry derives from `GeometryBase`; `ObjectType` discriminates the runtime kind, and `g.HasBrepForm`/`g.GetBoundingBox`/`g.Transform` are uniform across the hierarchy. The geometry kind is the runtime type, never a mode flag on a single class.
- mesh axis: `Mesh.Vertices`/`Faces`/`Normals`/`VertexColors`/`TextureCoordinates` are typed list views; `Faces.AddFace` admits triangle or quad indices, and `IsManifold`/`IsClosed`/`TopologyEdges` carry the connectivity receipt.
- NURBS axis: `NurbsCurve`/`NurbsSurface` expose `Points` and `Knots` list views with `PointAt`/`TangentAt`/`CurvatureAt`/`FrameAt` evaluation; rationality is `IsRational`, never a separate rational subtype.
- serialization axis: `CommonObject.Encode` produces a JSON dict and `CommonObject.Decode(jsonObject)` rebuilds the concrete subtype; this is the polymorphic round-trip that crosses the wire to a non-Python consumer, distinct from the binary `File3dm.Read`/`Write` archive path.
- evidence: each archive read captures archive version, object count, and per-table counts; each geometry round-trip captures `ObjectType`, bounding box, and validity (`IsValid`/`IsValidWithLog`) as an exchange receipt.
- boundary: rhino3dm owns headless `.3dm` IO and OpenNURBS geometry; live Rhino document scripting routes to the Rhino-MCP host, mesh interchange to `trimesh`/`meshio`, and CSG boolean kernels to `manifold3d`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `rhino3dm`
- Owns: headless `.3dm` archive read/write, typed document tables, OpenNURBS geometry (mesh, Brep, NURBS, SubD, extrusion, point cloud), geometry transforms, and `CommonObject` JSON serialization
- Accept: `.3dm` exchange and OpenNURBS geometry feeding the geometry and exchange owners
- Reject: wrapper-renames of `File3dm.Read`/`File3dmObjectTable.Add*`; a hand-rolled `.3dm` archive parser or NURBS evaluator where rhino3dm is admitted; an `Add<Geometry>` family that bypasses the polymorphic `Add(geometry, attributes)` row; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: admitted `rhino3dm>=8.17.0` is a CMake C++ source-build; reflection runs on a cp313 companion interpreter where PyPI ships a prebuilt wheel, while the `>=3.15` project venv builds from source and carries no prebuilt path, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp313 distribution; every documented type, table accessor, constructor, and evaluation entrypoint resolves — no phantom
