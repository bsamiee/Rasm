# [PY_DATA_API_RHINO3DM]

`rhino3dm` is the OpenNURBS Python binding that reads, writes, and constructs Rhino `.3dm` model data with no Rhino install: `File3dm` owns the typed component tables and a `CommonObject`/`GeometryBase` hierarchy roots curve, surface, Brep, mesh, SubD, extrusion, annotation, and instance geometry. `Encode`/`Decode` JSON dicts are the host-neutral wire shared with Rhino.Compute and the rhino3dm.js binding, so this binding is the headless 3dm IO and lightweight-construction layer beneath the Rhino-MCP and bridge owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rhino3dm`
- package: `rhino3dm` (MIT)
- module: `import rhino3dm`
- owner: `data`
- rail: aec
- asset: pybind11 native binding over OpenNURBS; each overload carries its concrete signature in `__doc__`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document owner, geometry root, and the value/kernel types a caller dispatches on

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :----------------- | :------------ | :-------------------------------------------- |
|  [01]   | `File3dm`          | class         | 3dm document owning the typed tables          |
|  [02]   | `GeometryBase`     | class         | `CommonObject`/`ModelComponent` geometry root |
|  [03]   | `Intersection`     | class         | static line/plane/sphere intersection kernel  |
|  [04]   | `Transform`        | value-object  | 4x4 affine matrix with static factories       |
|  [05]   | `ObjectAttributes` | class         | per-object layer/material/color/visibility    |
|  [06]   | `File3dmSettings`  | class         | document unit system and tolerances           |
|  [07]   | `DracoCompression` | class         | mesh geometry byte/base64 compression         |

[COMPONENT_TABLES]: `File3dmObjectTable` `File3dmLayerTable` `File3dmMaterialTable` `File3dmGroupTable` `File3dmDimStyleTable` `File3dmLinetypeTable` `File3dmInstanceDefinitionTable` `File3dmBitmapTable` `File3dmViewTable` `File3dmStringTable` `File3dmEmbeddedFileTable` `File3dmRenderContentTable` `File3dmPlugInDataTable`
[CURVES]: `Curve` `NurbsCurve` `LineCurve` `PolylineCurve` `PolyCurve` `ArcCurve` `CurveProxy` `BezierCurve`
[SURFACES_SOLIDS]: `Surface` `NurbsSurface` `PlaneSurface` `RevSurface` `SurfaceProxy` `Brep` `BrepFace` `BrepEdge` `BrepVertex` `Extrusion` `SubD`
[MESHES_POINTS]: `Mesh` `MeshVertexList` `MeshFaceList` `MeshNormalList` `MeshTextureCoordinateList` `MeshVertexColorList` `MeshTopologyEdgeList` `PointCloud` `PointCloudItem` `Point` `Point3dList` `PointGrid`
[ANNOTATION]: `AnnotationBase` `Dimension` `DimLinear` `DimAngular` `DimRadial` `DimOrdinate` `Leader` `Centermark` `Text` `TextDot` `Hatch` `DimensionStyle` `Font` `Arrowhead`
[INSTANCES_DOC]: `InstanceDefinition` `InstanceReference` `Group` `Layer` `Linetype` `Bitmap` `EmbeddedFile` `FileReference` `File3dmObject`
[VALUE_TYPES]: `Point2d` `Point3d` `Point4d` `Point2f` `Point3f` `Vector2d` `Vector3d` `Vector3f` `BoundingBox` `Box` `Plane` `Interval` `Line` `Arc` `Circle` `Ellipse` `Sphere` `Cylinder` `Cone` `Polyline` `ComponentIndex`
[INTERSECTION_RESULTS]: `LineCircleIntersection` `LineCylinderIntersection` `LineSphereIntersection` `PlaneSphereIntersection` `SphereSphereIntersection` `PointContainment` `RegionContainment`
[MATERIALS_RENDER]: `Material` `PhysicallyBasedMaterial` `Texture` `TextureMapping` `RenderContent` `RenderMaterial` `RenderTexture` `RenderEnvironment` `RenderSettings` `RenderChannels` `Light` `Decal` `EarthAnchorPoint` `GroundPlane` `Skylight` `Sun` `Dithering` `LinearWorkflow` `SafeFrame` `PostEffect`
[ENUMS]: `ObjectType` `MeshType` `ActiveSpace` `ObjectColorSource` `ObjectMaterialSource` `ObjectLinetypeSource` `ObjectPlotColorSource` `ObjectPlotWeightSource` `ObjectMode` `ObjectDecoration` `UnitSystem` `LoftType` `ComponentIndexType` `CoordinateSystem` `CurveOrientation` `CurveEvaluationSide` `CurveKnotStyle` `CurveOffsetCornerStyle` `CurveExtensionStyle` `BlendContinuity` `LightStyle` `TextureType` `TextureUvwWrapping` `InstanceDefinitionUpdateType` `TransformRigidType` `TransformSimilarityType` `AnnotationTypes` `ArrowheadTypes`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document IO, typed table mutation, construction factories, evaluation, intersection, and transform

[DOCUMENT_IO]: `File3dm.Read(path)` `File3dm.FromByteArray(buffer)` `f.Write(path, version=0) -> bool` `f.Encode() -> str` `File3dm.Decode(jsonStr)` `File3dm.ReadArchiveVersion(path) -> int` `File3dm.ReadNotes(path) -> str`
[TABLE_MUTATION]: `f.Objects.Add(geometry, attributes=None)` `AddObject(obj)` `AddPoint(...)` `AddLine(...)` `AddPolyline(...)` `AddArc(arc, attrs=None)` `AddCircle(...)` `AddSphere(...)` `AddCurve(...)` `AddSurface(...)` `AddBrep(...)` `AddExtrusion(...)` `AddMesh(...)` `AddPointCloud(...)` `AddHatch(...)` `AddTextDot(text, location, attrs=None)` `AddInstanceObject(...)` `FindId(guid)` `Delete(...)`; sibling tables carry `Add(component)` and iteration
[CURVE_CTORS]: `NurbsCurve.Create(periodic, degree, points)` `NurbsCurve.CreateFromLine/CreateFromArc/CreateFromCircle/CreateFromEllipse(...)` `Curve.CreateControlPointCurve(points, degree=3)`
[SURFACE_SOLID_CTORS]: `NurbsSurface.Create(dimension, isRational, order0, order1, cvCount0, cvCount1)` `NurbsSurface.CreateFromSphere/CreateFromCylinder/CreateFromCone/CreateRuledSurface(...)` `Brep.CreateFromBox/CreateFromBoundingBox/CreateFromSphere/CreateQuadSphere/CreateFromCylinder/CreateFromCone/CreateFromRevSurface/CreateFromSurface/CreateFromMesh/CreateTrimmedPlane/TryConvertBrep(...)` `Extrusion.Create(planarCurve, height, cap)` `Extrusion.CreateWithPlane/CreateBoxExtrusion/CreateCylinderExtrusion/CreatePipeExtrusion(...)` `ext.ToBrep(splitKinkyFaces)` `ext.GetMesh(meshType)`
[MESH_BUILD]: `Mesh()` `mesh.Vertices.Add(x, y, z)` `mesh.Faces.AddFace(a, b, c[, d])` `mesh.Normals.ComputeNormals()` `mesh.Compact()` `mesh.Append(other)` `mesh.IsManifold(topologicalTest) -> tuple` `Mesh.CreateFromSubDControlNet(subd, includeTextureCoordinates)` `mesh.CreatePartitions(maxVertexCount, maxTriangleCount)`
[EVALUATION]: `Curve.PointAt(t)/TangentAt(t)/FrameAt(t)/DerivativeAt(t, count, side)/CurvatureAt(t)/Trim(t0, t1)/Split(t)` `Curve.IsArc/IsCircle/IsLinear/IsPlanar/IsPolyline(tolerance=...)` `Curve.TryGetArc/TryGetCircle/TryGetEllipse/TryGetPolyline(tolerance=...)` `Surface.PointAt(u, v)/NormalAt(u, v)/FrameAt(u, v)/IsoCurve(direction, t)` `Surface.IsCone/IsCylinder/IsSphere/IsPlanar(tolerance=...)` `SubD.Subdivide(count)`
[INTERSECTION]: `Intersection.LineLine/LinePlane/LineCircle/LineSphere/LineCylinder/LineBox/PlanePlane/PlanePlanePlane/PlaneSphere/SphereSphere(...) -> typed result`
[TRANSFORM]: `Transform.Translation(v)` `Scale(...)` `Rotation(angle, axis, center)` `PlaneToPlane(p0, p1)` `Mirror(...)` `Shear(...)` `Identity()/ZeroTransformation()/Unset()` `Multiply(a, b)` `t.Transpose()/TryGetInverse()/Determinant()/ToFloatArray(rowDominant)` `M00..M33`
[COMPRESSION]: `DracoCompression.Compress(geometry, options)` `DecompressByteArray(bytes)` `DecompressBase64String(str)`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `File3dm` is the sole document owner; geometry enters and leaves only through the typed component tables, and `f.Objects.Add(geometry, attributes)` is the polymorphic add folding every `Add*` shortcut.
- Static `Create*` factories signal geometric failure by `None`, never exception, so the caller checks the result before use.
- pybind11 dispatches overloads at the C++ layer; each `Create(*args)`/`Translation(*args)` reads its concrete per-overload argument shape from `__doc__`.
- `Encode()`/`Decode()` round-trip any `CommonObject` or `File3dm` through a JSON-compatible structure — the supported bridge to Rhino.Compute and the rhino3dm.js/WebAssembly binding.
- `File3dmSettings.ModelUnitSystem`/`PageUnitSystem` are `UnitSystem` members read from `f.Settings` for the declared model unit; `ModelAbsoluteTolerance` carries the model tolerance.
- `Mesh` elements are plain tuples: a face is a 4-int tuple where a triangle repeats index 3 (`face[3] != face[2]` probes a quad), a vertex color is an `(R, G, B, A)` int tuple, and a normal is a `Vector3f`.
- `Intersection` and the `Curve`/`Surface` `Is*`/`TryGet*` families own geometric classification and intersection under an explicit `tolerance`.

[STACKING]:
- `trimesh`(`.api/trimesh.md`): a `rhino3dm.Mesh` exposes `Vertices`/`Faces`/`Normals`/`TextureCoordinates`/`VertexColors` arrays crossing to the data-tier `trimesh` load/export edge as the in-memory triangulation, Draco bytes carrying the compressed transport form.
- `meshio`(`libs/python/.api/meshio.md`): unstructured solver-mesh formats leave through meshio while `rhino3dm` owns the `.3dm` leg alone.
- within-lib: the `spatial/mesh` plane consumes `rhino3dm` as the headless `.3dm` identity and topology owner; `Encode`/`Decode` JSON dicts cross the same `File3dm`/`GeometryBase` objects to Rhino.Compute and the rhino3dm.js binding with no re-serialization, and the live Rhino host over MCP and the bridge own kernel operations OpenNURBS-without-Rhino cannot perform.

[LOCAL_ADMISSION]:
- Admit `rhino3dm` as the headless OpenNURBS `.3dm` IO and lightweight-construction owner on the data AEC rail, constructing primitives, Breps-from-primitives, NURBS, meshes, and SubD control nets and reading/writing the document.

[RAIL_LAW]:
- Package: `rhino3dm`
- Owns: OpenNURBS `.3dm` read/write and byte/JSON round-trip, typed document tables, NURBS/Brep/Mesh/SubD/Extrusion construction and evaluation, the `Intersection` kernel, geometry value types and transforms, annotation and instance geometry, object attributes and user strings, render content, meshing parameters, and Draco compression
- Accept: `File3dm` as document owner with typed table mutation, the polymorphic `Objects.Add(geometry, attrs)`, static `Create*` factories under null-result checks, `Encode`/`Decode`/`FromByteArray` for the transport bridge, `ObjectAttributes` alongside geometry on add, user strings for round-trip metadata, the `Intersection` kernel and `Is*`/`TryGet*` predicates, and Draco for mesh transport
- Reject: loose geometry outside the document tables, assuming non-null factory results, hand-rolled 3dm archive parsing where `Read`/`Encode`/`Decode` apply, manual vertex re-encoding where Draco applies, hand-derived intersection or shape detection where the kernel and predicates resolve, and substituting this binding for a live Rhino kernel on boolean solids or advanced filleting
