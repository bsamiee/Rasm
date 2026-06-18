# [PY_GEOMETRY_API_RHINO3DM]

`rhino3dm` supplies the headless OpenNURBS surface for the geometry exchange rail: a `File3dm` document with typed object/layer/material tables, the full `GeometryBase` hierarchy (`Mesh`, `Brep`, `NurbsCurve`, `NurbsSurface`, `SubD`, `Extrusion`, `PointCloud`), value primitives (`Point3d`, `Vector3d`, `Plane`, `Transform`, `BoundingBox`), and `CommonObject` JSON serialization that round-trips geometry to and from the `.3dm` archive without a Rhino install. The package owner composes `File3dm.Read`, `File3dmObjectTable.Add*`, and `CommonObject.Encode`/`Decode` into the exchange owner; it never re-implements OpenNURBS tessellation, NURBS evaluation, or the `.3dm` binary archive.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rhino3dm`
- package: `rhino3dm`
- import: `import rhino3dm`
- owner: `geometry`
- rail: exchange
- installed: `8.17.0` reflected via `python -c "import rhino3dm"` on cp313
- entry points: none (library only)
- capability: headless `.3dm` archive read/write, typed document tables, NURBS curve/surface evaluation, mesh and SubD geometry, point clouds, geometry transforms and bounding boxes, and `CommonObject` JSON serialization

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and table family
- rail: exchange

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [CAPABILITY]                                          |
| :-----: | :--------------------- | :---------------- | :---------------------------------------------------- |
|   [1]   | `File3dm`              | archive document  | read/write `.3dm`, owns all object and setting tables |
|   [2]   | `File3dmObjectTable`   | object table      | typed `Add*` geometry insertion and `FindId` lookup   |
|   [3]   | `File3dmLayerTable`    | layer table       | `AddLayer`, `FindName`, `FindIndex`, `FindId`         |
|   [4]   | `File3dmMaterialTable` | material table    | render material entries                               |
|   [5]   | `File3dmObject`        | object record     | geometry plus `ObjectAttributes` pair                 |
|   [6]   | `ObjectAttributes`     | object metadata   | layer, color, name, material source, user strings     |
|   [7]   | `Layer`                | layer record      | name, color, visibility, parent, render material      |
|   [8]   | `File3dmSettings`      | document settings | model unit system, tolerances, render settings        |

[PUBLIC_TYPE_SCOPE]: geometry hierarchy (`GeometryBase` subclasses)
- rail: exchange

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [CAPABILITY]                                         |
| :-----: | :------------- | :-------------- | :--------------------------------------------------- |
|   [1]   | `GeometryBase` | geometry root   | transform, bounding box, user strings, encode/decode |
|   [2]   | `Mesh`         | triangle mesh   | vertices/faces/normals/colors, manifold and topology |
|   [3]   | `Brep`         | boundary rep    | faces/edges/surfaces, solid and manifold queries     |
|   [4]   | `Extrusion`    | extrusion solid | profile-and-path solid, `ToBrep`/`ToNurbsSurface`    |
|   [5]   | `SubD`         | subdivision     | control net, subdivide, solid query                  |
|   [6]   | `NurbsCurve`   | NURBS curve     | control points, knots, evaluation, conversion        |
|   [7]   | `NurbsSurface` | NURBS surface   | control grid, knots, surface evaluation              |
|   [8]   | `PointCloud`   | point cloud     | points/colors/normals/values, closest-point query    |

[PUBLIC_TYPE_SCOPE]: value primitive family
- rail: exchange

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]    | [CAPABILITY]                         |
| :-----: | :------------ | :--------------- | :----------------------------------- |
|   [1]   | `Point3d`     | 3D point         | XYZ coordinate value                 |
|   [2]   | `Vector3d`    | 3D vector        | direction with unitize and dot/cross |
|   [3]   | `Point3dList` | point list       | resizable point sequence             |
|   [4]   | `Plane`       | oriented plane   | origin plus axes for frames          |
|   [5]   | `Transform`   | 4x4 transform    | affine transform applied to geometry |
|   [6]   | `BoundingBox` | axis-aligned box | min/max corner spatial bound         |
|   [7]   | `Interval`    | scalar interval  | parameter domain `T0`/`T1`           |
|   [8]   | `Polyline`    | polyline         | vertex sequence to `PolylineCurve`   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document read and write (`File3dm`)
- rail: exchange

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :--------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `File3dm.Read(path) -> File3dm`          | read           | open a `.3dm` archive from disk       |
|   [2]   | `File3dm.FromByteArray(buffer)`          | read           | decode a `.3dm` archive from bytes    |
|   [3]   | `File3dm.ReadArchiveVersion(path)`       | read           | archive version without full load     |
|   [4]   | `File3dm.ReadNotes(path)`                | read           | document notes string                 |
|   [5]   | `f.Write(path, version=0) -> bool`       | write          | write archive at OpenNURBS version    |
|   [6]   | `f.Encode() -> dict`                     | serialize      | JSON-encode the whole document        |
|   [7]   | `File3dm.Decode(jsonObject)`             | serialize      | rebuild a document from JSON          |
|   [8]   | `f.Objects` / `f.Layers` / `f.Materials` | table          | typed table accessors on the document |

[ENTRYPOINT_SCOPE]: typed object insertion (`File3dmObjectTable`)
- rail: exchange

Each `Add*` row inserts geometry under the active layer and returns the new object `Guid`; `Add(geometry, attributes)` is the polymorphic insert that takes any `GeometryBase` plus `ObjectAttributes`.

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [CAPABILITY]                |
| :-----: | :--------------------------------- | :------------- | :-------------------------- |
|   [1]   | `Add(geometry, attributes)`        | insert         | polymorphic geometry insert |
|   [2]   | `AddMesh(mesh, attributes)`        | insert         | add a `Mesh`                |
|   [3]   | `AddBrep(brep, attributes)`        | insert         | add a `Brep`                |
|   [4]   | `AddExtrusion(extrusion, attrs)`   | insert         | add an `Extrusion`          |
|   [5]   | `AddCurve(curve, attributes)`      | insert         | add any curve               |
|   [6]   | `AddPoint(point, attributes)`      | insert         | add a point object          |
|   [7]   | `AddPointCloud(cloud, attributes)` | insert         | add a `PointCloud`          |
|   [8]   | `AddSurface(surface, attributes)`  | insert         | add a surface               |
|   [9]   | `AddPolyline(polyline, attrs)`     | insert         | add a polyline              |
|  [10]   | `FindId(id) -> File3dmObject`      | lookup         | resolve an object by `Guid` |
|  [11]   | `Delete(id) -> bool`               | mutate         | remove an object by `Guid`  |

[ENTRYPOINT_SCOPE]: geometry construction and conversion
- rail: exchange

`CreateFrom*`/`Create*` are static constructors on the named class; conversion rows return a denser geometry form.

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [CAPABILITY]               |
| :-----: | :----------------------------------------- | :------------- | :------------------------- |
|   [1]   | `Brep.CreateFromBox(box) -> Brep`          | constructor    | box solid as Brep          |
|   [2]   | `Brep.CreateFromMesh(mesh, trimmed)`       | constructor    | Brep from a mesh           |
|   [3]   | `Brep.CreateFromSphere(sphere) -> Brep`    | constructor    | sphere as Brep             |
|   [4]   | `Mesh.CreateFromSubDControlNet(subd)`      | constructor    | control-net mesh from SubD |
|   [5]   | `NurbsCurve.Create(periodic, degree, pts)` | constructor    | control-point NURBS curve  |
|   [6]   | `NurbsCurve.CreateFromArc(arc)`            | constructor    | arc as NURBS curve         |
|   [7]   | `NurbsCurve.CreateFromCircle(circle)`      | constructor    | circle as NURBS curve      |
|   [8]   | `Extrusion.Create(profile, height, cap)`   | constructor    | extrude a planar profile   |
|   [9]   | `extrusion.ToBrep() -> Brep`               | conversion     | extrusion to boundary rep  |
|  [10]   | `extrusion.GetMesh(meshType) -> Mesh`      | conversion     | render mesh from extrusion |
|  [11]   | `curve.ToNurbsCurve() -> NurbsCurve`       | conversion     | any curve to NURBS form    |

[ENTRYPOINT_SCOPE]: geometry transforms and evaluation (`GeometryBase`)
- rail: exchange

Transform rows mutate the receiver in place and return a success bool; evaluation rows compute values at a curve/surface parameter.

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [CAPABILITY]                  |
| :-----: | :------------------------------------ | :------------- | :---------------------------- |
|   [1]   | `g.Transform(xform) -> bool`          | transform      | apply a 4x4 transform         |
|   [2]   | `g.Translate(vector) -> bool`         | transform      | translate by a vector         |
|   [3]   | `g.Rotate(angle, axis, center)`       | transform      | rotate about an axis          |
|   [4]   | `g.Scale(factor) -> bool`             | transform      | uniform scale                 |
|   [5]   | `g.GetBoundingBox(accurate)`          | query          | axis-aligned bound            |
|   [6]   | `curve.PointAt(t) -> Point3d`         | evaluation     | point at curve parameter      |
|   [7]   | `curve.TangentAt(t) -> Vector3d`      | evaluation     | tangent at curve parameter    |
|   [8]   | `curve.CurvatureAt(t) -> Vector3d`    | evaluation     | curvature vector at parameter |
|   [9]   | `surface.PointAt(u, v) -> Point3d`    | evaluation     | point at surface `(u,v)`      |
|  [10]   | `g.SetUserString(key, value) -> bool` | metadata       | attach a user string          |
|  [11]   | `g.Encode() -> dict`                  | serialize      | JSON-encode the geometry      |
|  [12]   | `CommonObject.Decode(jsonObject)`     | serialize      | rebuild geometry from JSON    |

## [4]-[IMPLEMENTATION_LAW]

[EXCHANGE_TOPOLOGY]:
- document axis: one `File3dm` owns every object, layer, material, group, instance-definition, and view table; geometry never lives outside a `File3dmObject`, and `File3dmObjectTable.Add*` is the only insertion surface, parameterized by geometry type and `ObjectAttributes`.
- geometry axis: every concrete geometry derives from `GeometryBase`; `ObjectType` discriminates the runtime kind, and `g.HasBrepForm`/`g.GetBoundingBox`/`g.Transform` are uniform across the hierarchy. The geometry kind is the runtime type, never a mode flag on a single class.
- mesh axis: `Mesh.Vertices`/`Faces`/`Normals`/`VertexColors`/`TextureCoordinates` are typed list views; `Faces.AddFace` admits triangle or quad indices, and `IsManifold`/`IsClosed`/`TopologyEdges` carry the connectivity receipt.
- NURBS axis: `NurbsCurve`/`NurbsSurface` expose `Points` and `Knots` list views with `PointAt`/`TangentAt`/`CurvatureAt`/`FrameAt` evaluation; rationality is `IsRational`, never a separate rational subtype.
- serialization axis: `CommonObject.Encode` produces a JSON dict and `CommonObject.Decode(jsonObject)` rebuilds the concrete subtype; this is the polymorphic round-trip that crosses the wire to a non-Python consumer, distinct from the binary `File3dm.Read`/`Write` archive path.
- evidence: each archive read captures archive version, object count, and per-table counts; each geometry round-trip captures `ObjectType`, bounding box, and validity (`IsValid`/`IsValidWithLog`) as an exchange receipt.
- boundary: rhino3dm owns headless `.3dm` IO and OpenNURBS geometry; live Rhino document scripting routes to the Rhino-MCP host, mesh interchange to `trimesh`/`meshio`, and CSG boolean kernels to `manifold3d`.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `rhino3dm`
- Owns: headless `.3dm` archive read/write, typed document tables, OpenNURBS geometry (mesh, Brep, NURBS, SubD, extrusion, point cloud), geometry transforms, and `CommonObject` JSON serialization
- Accept: `.3dm` exchange and OpenNURBS geometry feeding the geometry and exchange owners
- Reject: wrapper-renames of `File3dm.Read`/`File3dmObjectTable.Add*`; a hand-rolled `.3dm` archive parser or NURBS evaluator where rhino3dm is admitted; an `Add<Geometry>` family that bypasses the polymorphic `Add(geometry, attributes)` row; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: admitted `rhino3dm>=8.17.0` is a CMake C++ source-build; reflection runs on a cp313 companion interpreter where PyPI ships a prebuilt wheel, while the `>=3.15` project venv builds from source and carries no prebuilt path, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp313 distribution; every documented type, table accessor, constructor, and evaluation entrypoint resolves — no phantom
