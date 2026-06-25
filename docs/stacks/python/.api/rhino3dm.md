# [PY_GEOMETRY_API_RHINO3DM]

`rhino3dm` supplies the headless OpenNURBS surface for the geometry exchange rail: a `File3dm` document with the full typed table family (objects, layers, materials, groups, instance-definitions, dimension styles, linetypes, bitmaps, embedded files, views, render content, plug-in data, document strings), the complete `GeometryBase` hierarchy (`Mesh`, `Brep`, `NurbsCurve`/`NurbsSurface`, `SubD`, `Extrusion`, `PointCloud`, `PolyCurve`, `RevSurface`, `Hatch`, annotations), a transform algebra (`Transform` static constructors), an analytic primitive-intersection kernel (`Intersection`), Draco mesh compression (`DracoCompression`), render-mesh control (`MeshingParameters`), and `CommonObject` JSON serialization that round-trips any geometry to and from the `.3dm` archive without a Rhino install. The package owner composes `File3dm.Read`, `File3dmObjectTable.Add(geometry, attributes)`, `CommonObject.Encode`/`Decode`, and `Extrusion.GetMesh(meshingParameters)` into the exchange owner; it never re-implements OpenNURBS tessellation, NURBS evaluation, the analytic intersection solver, the Draco codec, or the `.3dm` binary archive.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rhino3dm`
- package: `rhino3dm`
- import: `import rhino3dm`
- owner: `geometry`
- rail: exchange
- license: `MIT` (Robert McNeel & Associates; dist-info `licenses/LICENSE`, OpenNURBS MIT) — no copyleft obligation; the native extension `_rhino3dm` is a self-contained pybind11 build of the OpenNURBS C++ kernel with no transitive copyleft dependency
- installed: `8.17.0` (module `rhino3dm.Version == '8.17.25066.07000'`), reflected against the cp315 Forge scientific env where it loaded a locally built native wheel `cp315-cp315-macosx_14_0_arm64`
- entry points: none (library only)
- capability: headless `.3dm` archive read/write at any OpenNURBS version, the full typed document-table family, complete `GeometryBase` geometry (mesh, Brep, NURBS curve/surface, SubD, extrusion, point cloud, poly-curve, surfaces of revolution, hatch, annotation), transform algebra, analytic primitive intersection, Draco mesh compression, render-mesh meshing parameters, and polymorphic `CommonObject` JSON serialization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and table family (`File3dm` + `File3dm*Table`)
- rail: exchange

One `File3dm` owns the whole archive; every table is a typed accessor property on it. The document carries authorship metadata (`ApplicationName`/`Created`/`Revision`/`ArchiveVersion`) and `StartSectionComments` alongside the tables.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [CAPABILITY]                                                                |
| :-----: | :----------------------------- | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | `File3dm`                      | archive document   | read/write `.3dm`, owns every object and setting table, authorship metadata |
|  [02]   | `File3dmObjectTable`           | object table       | polymorphic `Add` plus typed `Add*` insertion, `FindId`, `Delete`           |
|  [03]   | `File3dmLayerTable`            | layer table        | `AddLayer`, `FindName`/`FindIndex`/`FindId`                                  |
|  [04]   | `File3dmMaterialTable`         | material table     | render-material entries                                                     |
|  [05]   | `File3dmInstanceDefinitionTable` | block table      | `InstanceDefinition` blocks referenced by `InstanceReference`               |
|  [06]   | `File3dmGroupTable`            | group table        | grouping of object ids                                                      |
|  [07]   | `File3dmDimStyleTable`         | dimension styles   | `DimensionStyle` entries for annotation                                     |
|  [08]   | `File3dmLinetypeTable`         | linetype table     | `Linetype` dash patterns                                                    |
|  [09]   | `File3dmEmbeddedFileTable`     | embedded files     | `EmbeddedFile` payloads, `GetEmbeddedFileAsBase64`                          |
|  [10]   | `File3dmStringTable`           | document strings   | document-scoped user-string key/value store                                |
|  [11]   | `File3dmObject`                | object record      | `Geometry` plus `Attributes` (`ObjectAttributes`) pair                      |
|  [12]   | `ObjectAttributes`             | object metadata    | layer, color/source, name, material source, user strings, visibility        |
|  [13]   | `Layer`                        | layer record       | name, color, visibility, parent, render material                            |
|  [14]   | `File3dmSettings`              | document settings   | model unit system, tolerances, render settings                              |
|  [15]   | `File3dmWriteOptions`          | write options       | target archive `Version` and `SaveUserData` toggle for `Write`              |

[PUBLIC_TYPE_SCOPE]: geometry hierarchy (`GeometryBase` subclasses)
- rail: exchange

Every concrete geometry derives from `GeometryBase`; `ObjectType` discriminates the runtime kind and `Transform`/`Translate`/`Rotate`/`Scale`/`GetBoundingBox`/`Encode`/`Decode`/`SetUserString`/`IsValid` are uniform across the hierarchy.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]      | [CAPABILITY]                                                       |
| :-----: | :-------------- | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `GeometryBase`  | geometry root      | transform/translate/rotate/scale, bounding box, user strings, codec |
|  [02]   | `Mesh`          | triangle/quad mesh | vertices/faces/normals/colors/texcoords, `Append`/`Compact`, topology |
|  [03]   | `Brep`          | boundary rep       | faces/edges/surfaces/vertices, `IsSolid`/`IsManifold`, `TryConvertBrep` |
|  [04]   | `Extrusion`     | extrusion solid    | profile-and-path solid, inner profiles, mitering, `ToBrep`/`GetMesh` |
|  [05]   | `SubD`          | subdivision        | control net, `Subdivide`, `IsSolid`, evaluation-cache management    |
|  [06]   | `Curve`         | curve root         | evaluation, `Split`/`Trim`/`Reverse`, `TryGetArc/Circle/Ellipse/Polyline` |
|  [07]   | `NurbsCurve`    | NURBS curve        | control points/knots, degree-raise, Bezier spans, rational toggle   |
|  [08]   | `PolyCurve`     | composite curve    | segment chain unified under the `Curve` evaluation contract         |
|  [09]   | `NurbsSurface`  | NURBS surface      | control grid/knots, iso-curves, `NormalAt`, rational toggle, ruled  |
|  [10]   | `RevSurface`    | surface of revolution | profile-curve revolve consumed by `Brep.CreateFromRevSurface`    |
|  [11]   | `PointCloud`    | point cloud        | points/colors/normals/values, `ClosestPoint`, `Merge`, hidden flags |
|  [12]   | `Hatch`         | hatch fill         | boundary loops plus pattern, annotation-family geometry             |

[PUBLIC_TYPE_SCOPE]: value, transform, and kernel family
- rail: exchange

`Transform` is the full affine algebra; `Intersection` is the analytic primitive-intersection kernel; `DracoCompression` is the mesh codec. Value primitives carry the coordinate/frame vocabulary.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                          |
| :-----: | :------------------------ | :----------------- | :-------------------------------------------------------------------- |
|  [01]   | `Point3d` / `Vector3d`    | coordinate value   | XYZ point and direction (`Vector3d` unitize/dot/cross)                |
|  [02]   | `Point2d` / `Point3f` / `Vector3f` | packed value | float/2D coordinate variants for mesh and texture buffers         |
|  [03]   | `Point4d`                 | homogeneous point  | rational control-point weight carrier                                 |
|  [04]   | `Point3dList`             | point list         | resizable point sequence feeding curve/cloud builders                 |
|  [05]   | `Plane`                   | oriented plane     | origin plus axes; the frame argument for `Transform.PlaneToPlane`     |
|  [06]   | `Transform`               | 4x4 transform      | `Translation`/`Rotation`/`Scale`/`Mirror`/`Shear`/`PlaneToPlane`, `Multiply`, `TryGetInverse`, `Determinant` |
|  [07]   | `BoundingBox` / `Box`     | bounds             | axis-aligned and oriented box; `Brep.CreateFromBox` source            |
|  [08]   | `Interval`                | scalar interval    | parameter domain `T0`/`T1`                                            |
|  [09]   | `Line` / `Arc` / `Circle` / `Ellipse` / `Sphere` / `Cone` / `Cylinder` | analytic primitive | closed-form shapes feeding `NurbsCurve.CreateFrom*` and `Intersection` |
|  [10]   | `Polyline` / `PolylineCurve` | polyline        | vertex sequence and its `Curve` form                                  |
|  [11]   | `Intersection`            | intersection kernel | static `LineLine`/`LinePlane`/`LineSphere`/`PlanePlane`/`SphereSphere`/`LineBox`/`LineCylinder`/`PlanePlanePlane` analytic solvers |
|  [12]   | `DracoCompression` / `DracoCompressionOptions` | mesh codec | Draco `Compress`/`DecompressByteArray`/`DecompressBase64String` with quantization/level options |
|  [13]   | `MeshingParameters`       | render-mesh control | density/angle/edge-length controls for `Extrusion.GetMesh`/render meshes; `Default`/`FastRenderMesh`/`QualityRenderMesh` presets |
|  [14]   | `UnitSystem` / `ObjectType` / `MeshType` | enum         | unit, runtime-geometry-kind, and mesh-purpose discriminants           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document read and write (`File3dm`)
- rail: exchange

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [CAPABILITY]                                  |
| :-----: | :--------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `File3dm.Read(path) -> File3dm`          | read           | open a `.3dm` archive from disk               |
|  [02]   | `File3dm.FromByteArray(buffer)`          | read           | decode a `.3dm` archive from bytes            |
|  [03]   | `File3dm.ReadArchiveVersion(path)`       | read           | archive version without full load             |
|  [04]   | `File3dm.ReadNotes(path)`                | read           | document notes string                         |
|  [05]   | `f.Write(path, version=0) -> bool`       | write          | write archive at OpenNURBS `version` (0 = current) |
|  [06]   | `f.Encode() -> dict`                     | serialize      | JSON-encode the whole document                |
|  [07]   | `File3dm.Decode(jsonObject)`             | serialize      | rebuild a document from JSON                  |
|  [08]   | `f.Objects`/`f.Layers`/`f.Materials`/`f.Groups`/`f.InstanceDefinitions`/`f.DimStyles`/`f.Strings` | table | typed table accessors on the document |
|  [09]   | `f.ArchiveVersion`/`f.ApplicationName`/`f.Revision` | metadata | authorship and version receipt off a loaded document |

[ENTRYPOINT_SCOPE]: typed object insertion (`File3dmObjectTable`)
- rail: exchange

`Add(geometry, attributes=None)` is the polymorphic insert taking any `GeometryBase` plus optional `ObjectAttributes` and returning the new object `Guid`; the typed `Add*` rows are convenience overloads over analytic primitives that build the geometry inline. Prefer `Add` for the canonical path; the analytic `Add*` rows avoid pre-constructing a `Curve`/`Surface`.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [CAPABILITY]                                |
| :-----: | :----------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Add(geometry, attributes=None)`     | insert         | polymorphic geometry insert (canonical row) |
|  [02]   | `AddMesh`/`AddBrep`/`AddExtrusion`/`AddSurface`/`AddPointCloud` | insert | typed geometry inserts |
|  [03]   | `AddCurve`/`AddPolyline`             | insert         | curve and polyline inserts                  |
|  [04]   | `AddPoint`/`AddTextDot`              | insert         | point and text-dot annotation inserts       |
|  [05]   | `AddLine`/`AddArc`/`AddCircle`/`AddEllipse`/`AddSphere` | insert | analytic-primitive inserts (build geometry inline) |
|  [06]   | `AddInstanceObject`/`AddObject`/`AddHatch` | insert   | block reference, generic object, and hatch  |
|  [07]   | `FindId(id) -> File3dmObject`        | lookup         | resolve an object by `Guid`                 |
|  [08]   | `Delete(id) -> bool`                 | mutate         | remove an object by `Guid`                  |

[ENTRYPOINT_SCOPE]: geometry construction, conversion, and meshing
- rail: exchange

`CreateFrom*`/`Create*` are static constructors on the named class; conversion rows return a denser geometry form; `GetMesh(MeshingParameters)` tessellates a smooth body to a render mesh.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `Brep.CreateFromBox`/`CreateFromBoundingBox`/`CreateFromSphere`/`CreateFromCone`/`CreateFromCylinder` | constructor | analytic-solid Breps |
|  [02]   | `Brep.CreateFromMesh(mesh, trimmedTriangles)`      | constructor    | Brep from a mesh                           |
|  [03]   | `Brep.CreateFromSurface`/`CreateFromRevSurface`/`CreateQuadSphere`/`CreateTrimmedPlane` | constructor | Brep from surface forms |
|  [04]   | `Mesh.CreateFromSubDControlNet(subd, includeTextureCoordinates)` | constructor | control-net mesh from SubD     |
|  [05]   | `NurbsCurve.Create(periodic, degree, points)`      | constructor    | control-point NURBS curve                  |
|  [06]   | `NurbsCurve.CreateFromArc/CreateFromCircle/CreateFromEllipse/CreateFromLine` | constructor | analytic curve to NURBS form |
|  [07]   | `NurbsSurface.Create`/`CreateFromSphere`/`CreateFromCone`/`CreateFromCylinder`/`CreateRuledSurface` | constructor | NURBS surface forms |
|  [08]   | `Extrusion.Create(planarCurve, height, cap)`       | constructor    | extrude a planar profile                   |
|  [09]   | `Extrusion.CreateBoxExtrusion`/`CreateCylinderExtrusion`/`CreatePipeExtrusion`/`CreateWithPlane` | constructor | parametric extrusion solids |
|  [10]   | `extrusion.AddInnerProfile(curve)` / `SetOuterProfile`/`SetPathAndUp` | builder | inner voids and path orientation on an extrusion |
|  [11]   | `extrusion.ToBrep()` / `surface.ToNurbsSurface()` / `curve.ToNurbsCurve()` | conversion | promote to a denser geometry form    |
|  [12]   | `extrusion.GetMesh(meshType)` / `MeshingParameters.QualityRenderMesh` | tessellate | render mesh from a smooth body under meshing controls |

[ENTRYPOINT_SCOPE]: transforms, intersection, evaluation, and codec
- rail: exchange

`Transform.*` build a 4x4 transform applied via `g.Transform(xform)`; `Intersection.*` solve analytic primitive intersections (return a tuple); evaluation rows compute values at a curve/surface parameter.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `Transform.Translation`/`Rotation`/`Scale`/`Mirror`/`Shear`/`PlaneToPlane` | transform | static affine-transform constructors |
|  [02]   | `Transform.Multiply(a, b)` / `t.TryGetInverse()` / `t.Determinant` | transform | compose, invert, and inspect a transform |
|  [03]   | `g.Transform(xform)`/`Translate`/`Rotate`/`Scale` (-> bool) | transform | apply a transform to any `GeometryBase` in place |
|  [04]   | `Intersection.LineLine`/`LinePlane`/`LineSphere`/`LineBox`/`LineCylinder` | intersect | analytic line-vs-primitive solvers (tuple result) |
|  [05]   | `Intersection.PlanePlane`/`PlanePlanePlane`/`PlaneSphere`/`SphereSphere` | intersect | analytic plane/sphere solvers (tuple result) |
|  [06]   | `curve.PointAt(t)`/`TangentAt(t)`/`CurvatureAt(t)`/`DerivativeAt(t,n)`/`FrameAt(t)` | evaluate | curve evaluation and Frenet frame |
|  [07]   | `curve.Split(t)`/`Trim(domain)`/`Reverse()` / `curve.TryGetArc()`/`TryGetCircle()`/`TryGetPolyline()` | curve algebra | sub-curve extraction and analytic recovery |
|  [08]   | `surface.PointAt(u,v)`/`NormalAt(u,v)`/`IsoCurve(dir,c)` | evaluate | surface evaluation and iso-curve            |
|  [09]   | `cloud.ClosestPoint(point)` / `cloud.Merge(other)` / `cloud.Add(point)` | cloud | nearest-point query and cloud assembly      |
|  [10]   | `g.SetUserString(key, value)` / `g.GetUserStrings()` | metadata     | attach and read object-scoped user strings  |
|  [11]   | `g.Encode() -> dict` / `CommonObject.Decode(jsonObject)` | serialize | polymorphic per-geometry JSON round-trip    |
|  [12]   | `DracoCompression.Compress(mesh, options)` / `DracoCompression.DecompressByteArray(bytes)` | codec | Draco mesh compression round-trip with `DracoCompressionOptions` |

## [04]-[IMPLEMENTATION_LAW]

[EXCHANGE_TOPOLOGY]:
- document axis: one `File3dm` owns every object, layer, material, group, instance-definition, dimension-style, linetype, embedded-file, string, and view table; geometry never lives outside a `File3dmObject`, and `File3dmObjectTable.Add(geometry, attributes)` is the canonical insertion surface. The typed `Add*` rows are analytic-primitive convenience overloads, not a parallel insert family — the owner threads `Add` and treats `Add*` as the inline-primitive shortcut, never minting a per-type insert path.
- geometry axis: every concrete geometry derives from `GeometryBase`; `ObjectType` discriminates the runtime kind, and `Transform`/`Translate`/`Rotate`/`Scale`/`GetBoundingBox`/`Encode`/`Decode`/`IsValid`/`HasBrepForm` are uniform. The geometry kind is the runtime subtype, never a mode flag on one class.
- transform axis: `Transform` is the affine algebra — `Translation`/`Rotation`/`Scale`/`Mirror`/`Shear`/`PlaneToPlane` build a 4x4, `Multiply` composes, `TryGetInverse`/`Determinant`/`IsRotation`/`IsAffine` inspect it, and `g.Transform(xform)` applies it across every geometry kind. The owner composes a transform algebra here rather than hand-rolling matrix math.
- mesh axis: `Mesh.Vertices`/`Faces`/`Normals`/`VertexColors`/`TextureCoordinates` are typed list views; `Faces.AddFace` admits triangle or quad indices; `Append`/`Compact`/`IsManifold`/`IsClosed`/`TopologyEdges` carry the connectivity receipt; `DracoCompression` is the compressed-mesh transport codec for the wire.
- NURBS axis: `NurbsCurve`/`NurbsSurface` expose `Points`/`Knots` list views with `PointAt`/`TangentAt`/`CurvatureAt`/`FrameAt`/`NormalAt` evaluation, `IncreaseDegree`/`MakePiecewiseBezier`/`IsoCurve` refinement; rationality is `IsRational`/`MakeRational`/`MakeNonRational`, never a separate rational subtype.
- intersection axis: `Intersection` is the analytic primitive-intersection kernel (`LineLine`, `LinePlane`, `LineSphere`, `PlanePlane`, `SphereSphere`, etc.) returning a tuple; it owns closed-form primitive intersections so the owner never re-derives them. It does NOT own mesh-mesh boolean CSG (that routes to `manifold3d`).
- serialization axis: `CommonObject.Encode` produces a JSON dict and `CommonObject.Decode(jsonObject)` rebuilds the concrete subtype; this is the polymorphic round-trip that crosses the wire to a non-Python consumer, distinct from the binary `File3dm.Read`/`Write` archive path.
- evidence: each archive read captures `ArchiveVersion`, object count, and per-table counts; each geometry round-trip captures `ObjectType`, bounding box, and validity (`IsValid`/`IsValidWithLog`) as an exchange receipt.

[INTEGRATION_STACK]:
- mesh interchange seam: a `Mesh` decoded from `.3dm` feeds `trimesh.Trimesh(vertices, faces)` over the in-memory vertices/faces arrays (the geometry mesh owner never serializes to a file at this seam); `meshio` owns multi-format mesh files, `manifold3d` owns watertight CSG boolean, and `rhino3dm` round-trips OpenNURBS-native geometry the others cannot represent. The single dense rail is `File3dm.Read -> Objects.Add/FindId -> Mesh -> trimesh -> manifold3d` with `rhino3dm` the OpenNURBS-fidelity endpoint and Draco the compressed-mesh transport.
- BREP seam: `cadquery-ocp` owns OCCT solid modeling and `rhino3dm` owns OpenNURBS Brep; the two meet at the wire via tessellated `Mesh`/`Brep` JSON (`Encode`/`Decode`), never by sharing a kernel handle. `Brep.CreateFromMesh`/`Mesh.CreateFromSubDControlNet` bridge between the mesh and Brep/SubD tiers inside `rhino3dm`.
- point-cloud seam: a `rhino3dm.PointCloud` carries colors/normals/values for the `.3dm` archive; `laspy`/`pdal`/`pye57` own LAS/E57 point-cloud IO and `small_gicp`/`open3d` own registration — `rhino3dm` is the OpenNURBS-document point-cloud carrier, not the scan-IO or registration owner.
- IFC seam: IFC semantic identity and geometry stay in `ifcopenshell`; `rhino3dm` is the OpenNURBS exchange endpoint. A geometry graduating from IFC to a `.3dm` archive flows `ifcopenshell` tessellation -> `Mesh` -> `File3dmObjectTable.Add`, never a shared semantic model.
- boundary: rhino3dm owns headless `.3dm` IO, OpenNURBS geometry, the affine transform algebra, the analytic primitive-intersection kernel, and Draco mesh compression; live Rhino document scripting routes to the Rhino-MCP host, mesh-mesh boolean CSG to `manifold3d`, multi-format mesh files to `meshio`/`trimesh`, point-cloud scan IO to `laspy`/`pdal`/`pye57`, and registration to `small_gicp`/`open3d`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `rhino3dm`
- Owns: headless `.3dm` archive read/write, the full typed document-table family, OpenNURBS geometry (mesh, Brep, NURBS curve/surface, SubD, extrusion, point cloud, poly-curve, surfaces of revolution, hatch, annotation), the `Transform` affine algebra, the `Intersection` analytic primitive kernel, `DracoCompression` mesh codec, `MeshingParameters` render-mesh control, and polymorphic `CommonObject` JSON serialization
- Accept: `.3dm` exchange and OpenNURBS geometry feeding the geometry and exchange owners
- Reject: wrapper-renames of `File3dm.Read`/`File3dmObjectTable.Add`; a hand-rolled `.3dm` archive parser, NURBS evaluator, affine-transform matrix kernel, analytic primitive-intersection solver, or Draco codec where rhino3dm is admitted; a per-type `Add<Geometry>` family treated as the canonical insert instead of polymorphic `Add(geometry, attributes)`; mesh-mesh boolean CSG re-routed here when `manifold3d` is admitted; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: PyPI publishes no cp315 wheel for `rhino3dm 8.17.0` (its CMake C++ build is the source-build path); the project cp315 venv therefore resolves it only when the Forge scientific env builds the native extension locally. Reflection here ran against exactly such a build — a locally produced `cp315-cp315-macosx_14_0_arm64` wheel installed in the cp315 Forge scientific env — so cp315 is a build-from-source admission, not a no-wheel exclusion. The `assay api` resolver finds no source in the project venv site-packages because the extension is built in the companion scientific env, not the project venv.
- members: every documented type, table accessor, static constructor, transform, intersection, codec, and evaluation entrypoint resolves against the live cp315 `_rhino3dm` pybind surface (193 top-level classes; the catalog documents the high-value exchange subset) — no phantom
