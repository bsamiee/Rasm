# [PY_DATA_API_RHINO3DM]

`rhino3dm` supplies an OpenNURBS binding for reading, writing, and constructing Rhino `.3dm` model data without Rhino installed. It provides `File3dm` as the document owner over typed component tables, a `GeometryBase` hierarchy (`Curve`, `Surface`, `Brep`, `Mesh`, `SubD`, `Extrusion`, `PointCloud`), and value-type primitives (`Point3d`, `Vector3d`, `Transform`, `BoundingBox`, `Plane`).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rhino3dm`
- package: `rhino3dm`
- import: `import rhino3dm`
- owner: `data`
- rail: aec
- capability: OpenNURBS 3dm read/write, document component tables, NURBS curve/surface/Brep/Mesh/SubD/Extrusion construction, geometry value types and transforms, object attributes and user strings, and Draco mesh compression

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `rhino3dm.File3dm` — in-memory 3dm document; holds component tables and read/write entry; `File3dm.Read(path)`, `File3dm.FromByteArray(bytes)`, `Write(path, version)`, `Encode()`/`Decode()`, `ReadArchiveVersion(path)`.
- component tables: `File3dmObjectTable` (`Objects`), `File3dmLayerTable` (`Layers`), `File3dmMaterialTable` (`Materials`), `File3dmGroupTable` (`Groups`), `File3dmDimStyleTable`, `File3dmLinetypeTable`, `File3dmInstanceDefinitionTable`, `File3dmBitmapTable`, `File3dmViewTable`, `File3dmStringTable` (`Strings`), `File3dmEmbeddedFileTable`; `File3dmSettings` via `Settings`.
- `rhino3dm.GeometryBase` — geometry root; `Transform(xform)`, `Translate`, `Rotate`, `Scale`, `GetBoundingBox`, `Duplicate`, `Encode`/`Decode`, `GetUserString`/`SetUserString`, `ObjectType`.
- curves: `Curve`, `NurbsCurve`, `LineCurve`, `PolylineCurve`, `PolyCurve`, `ArcCurve`, `CurveProxy`, `BezierCurve`.
- surfaces and solids: `Surface`, `NurbsSurface`, `PlaneSurface`, `RevSurface`, `SurfaceProxy`, `Brep` (with `BrepFace`, `BrepEdge`, `BrepVertex` and their `*List` collections), `Extrusion`, `SubD`.
- meshes and points: `Mesh` (with `MeshVertexList`, `MeshFaceList`, `MeshNormalList`, `MeshTextureCoordinateList`, `MeshVertexColorList`, `MeshTopologyEdgeList`), `PointCloud`, `Point`, `Point3dList`, `PointGrid`.
- value types: `Point2d`, `Point3d`, `Point4d`, `Point3f`, `Vector2d`, `Vector3d`, `Vector3f`, `Transform`, `BoundingBox`, `Box`, `Plane`, `Interval`, `Line`, `Arc`, `Circle`, `Ellipse`, `Sphere`, `Cylinder`, `Cone`, `Polyline`.
- attributes and enums: `ObjectAttributes`, `Layer`, `Material`, `PhysicallyBasedMaterial`, `ObjectType`, `MeshType`, `ActiveSpace`, `ObjectColorSource`, `ObjectMaterialSource`, `UnitSystem`, `LoftType`, `ComponentIndex`.

[ENTRYPOINTS]:
- document IO: `File3dm.Read(path) -> File3dm`, `File3dm.FromByteArray(b) -> File3dm`, `f.Write(path, version)`, `f.Encode() -> dict`, `File3dm.Decode(dict) -> File3dm`, `File3dm.ReadArchiveVersion(path) -> int`, `File3dm.ReadNotes(path) -> str`.
- table mutation: `f.Objects.AddPoint(pt)`, `f.Objects.AddLine(p0, p1)`, `f.Objects.AddCurve(curve, attrs=None)`, `f.Objects.AddMesh(mesh, attrs=None)`, `f.Objects.AddBrep(brep, attrs=None)`, `f.Objects.AddSurface(srf, attrs=None)`, `f.Objects.AddExtrusion(ext, attrs=None)`, `f.Layers.Add(layer)`, `f.Materials.Add(material)`.
- curve construction: `NurbsCurve.Create(periodic, degree, points)`, `NurbsCurve.CreateFromLine(line)`, `NurbsCurve.CreateFromArc(arc)`, `NurbsCurve.CreateFromCircle(circle)`, `NurbsCurve.CreateFromEllipse(ellipse)`, `Curve.CreateControlPointCurve(points, degree)`.
- surface and solid construction: `NurbsSurface.Create(dimension, isRational, order0, order1, cvCount0, cvCount1)`, `NurbsSurface.CreateFromSphere(sphere)`, `NurbsSurface.CreateFromCylinder(cyl)`, `NurbsSurface.CreateRuledSurface(curveA, curveB)`, `Brep.CreateFromBox(box)`, `Brep.CreateFromSphere(sphere)`, `Brep.CreateFromCylinder(cyl, capBottom, capTop)`, `Brep.CreateFromCone(cone, capBottom)`, `Brep.CreateFromMesh(mesh, trimmedTriangles)`, `Brep.CreateFromSurface(srf)`, `Extrusion.Create(planarCurve, height, cap)`, `Extrusion.CreateBoxExtrusion(box)`, `Extrusion.CreateCylinderExtrusion(cyl, capBottom, capTop)`.
- mesh build: `Mesh()`, `mesh.Vertices.Add(x, y, z)`, `mesh.Faces.AddFace(a, b, c[, d])`, `mesh.Normals.ComputeNormals()`, `mesh.Compact()`, `Mesh.CreateFromSubDControlNet(subd)`, `mesh.CreatePartitions(maxVerts, maxFaces)`.
- geometry queries: `Curve.PointAt(t)`, `Curve.PointAtStart`, `Curve.PointAtEnd`, `Curve.FrameAt(t)`, `Curve.DerivativeAt(t, derivativeCount, side)`, `Curve.CurvatureAt(t)`, `Curve.IsClosed`, `Curve.IsPlanar()`, `Curve.IsLinear()`, `GeometryBase.GetBoundingBox(plane=None)`, `Mesh.IsClosed`, `Mesh.IsManifold(topologicalTest)`, `SubD.IsSolid`.
- transforms: `Transform.Translation(v)`, `Transform.Scale(plane, x, y, z)`, `Transform.Rotation(angle, axis, center)`, `Transform.PlaneToPlane(from, to)`, `Transform.Mirror(plane)`, `geom.Transform(xform)`, `Point3d.DistanceTo(other)`.
- attributes and metadata: `ObjectAttributes()` with `LayerIndex`, `MaterialIndex`, `Name`, `ObjectColor`, `ColorSource`; `geom.SetUserString(key, value)`, `geom.GetUserStrings()`, `f.Strings` document string table.
- compression: `DracoCompression.Compress(geometry, options)`, `DracoCompression.DecompressByteArray(bytes)`, `DracoCompressionOptions`.

[IMPLEMENTATION_LAW]:
- `File3dm` is the single document owner; geometry enters and leaves only through typed component tables (`Objects`, `Layers`, `Materials`), never as loose globals.
- `File3dm.Write(path, version)` selects the 3dm archive version; pass the target Rhino version explicitly and use `ReadArchiveVersion` to inspect an unknown file before reading.
- Geometry construction uses static `Create*` factories that return `None` on invalid input; check the result rather than assuming success, since this binding signals failure by null rather than exception.
- `Encode()`/`Decode()` round-trip geometry and documents through JSON-compatible dicts; this is the supported serialization bridge for transport to and from the Rhino compute and JavaScript bindings.
- Object identity in tables is index- and GUID-based; `ObjectAttributes` carries layer, material, color source, and name, and is passed alongside geometry on `Add*` calls.
- User strings (`SetUserString`/`GetUserStrings`) attach arbitrary key/value metadata to geometry and persist through 3dm round-trips; the document `Strings` table holds document-scoped key/value pairs.
- `DracoCompression` compresses `Mesh` and other geometry to a compact byte array; use it for mesh transport rather than re-encoding vertex arrays manually.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `rhino3dm`
- Owns: OpenNURBS 3dm read/write, document component tables, NURBS curve/surface/Brep/Mesh/SubD/Extrusion construction, geometry value types and transforms, object attributes and user strings, Draco compression
- Accept: `File3dm` as the document owner with typed table mutation, static `Create*` factories with null-result checks, `Encode`/`Decode` for the JSON serialization bridge, `ObjectAttributes` alongside geometry on add, user strings for round-trip metadata
- Reject: loose geometry outside the document tables, assuming non-null factory results, hand-rolled 3dm archive parsing, and manual vertex re-encoding where Draco compression applies
