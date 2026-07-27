# [RASM_API_RHINO]

This catalogue is the kernel's `RhinoCommon` partition: the host-ABI surface `Rasm` composes above the branch value substrate — the single-precision mesh carriers, the analytic primitive solids, the curve and mesh reference geometry with its typed topology, the native remesh and unwrap seam, the `Intersection` parametric lattice the `Analysis` layer folds, and the `RTree` point-neighborhood index. Kernel code authors discrete predicate-exact crossing and broad-phase acceleration itself, so host parametric intersection and kernel crossing meet at no interior.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon kernel-crossing surface
- host: Rhino host runtime, in-process (proprietary McNeel SDK)
- assembly: `RhinoCommon`
- namespace: `Rhino.Geometry`, `Rhino.Geometry.Intersect`, `Rhino.Geometry.Collections`
- asset: in-process `RhinoCommon.dll` from the RhinoWIP host bundle; managed structs wrap the `opennurbs` C++ core through `UnsafeNativeMethods` P/Invoke
- abi: value structs marshal blittably, so a struct copy is a managed value copy; reference geometry (`Mesh`/`Curve`/`Brep`/`GeometryBase`) owns native handles as `IDisposable`, so a leaked reference strands the unmanaged buffer
- runtime: net48 in-Rhino ALC; the geometry-only slice below the document stratum is the sole kernel-admitted surface
- rail: host-rhino

- Registers `RhinoCommon` value substrate(`libs/csharp/.api/api-rhinocommon.md`): `Point3d`, `Vector3d`, `Plane`, `Line`, `BoundingBox`, `Transform`, `MeshFace`, `Quaternion`, `Interval`, `Box`, and the `GeometryBase` bounds triple carry their whole member algebra there — the point, vector, frame, segment, extent, affine, and rotor entrypoint families included — and this partition never re-tables them.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: single-precision mesh carriers and the analytic primitive solids the substrate does not carry

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]    | [CAPABILITY]                          |
| :-----: | :--------- | :--------------- | :------------------------------------ |
|  [01]   | `Point3f`  | blittable struct | mesh-vertex point, single precision   |
|  [02]   | `Vector3f` | blittable struct | mesh-normal vector, single precision  |
|  [03]   | `Sphere`   | struct           | analytic sphere                       |
|  [04]   | `Cylinder` | struct           | axis circle with a height             |
|  [05]   | `Cone`     | struct           | apex, axis, and radius                |
|  [06]   | `Torus`    | struct           | major and minor radii on a frame      |
|  [07]   | `Circle`   | struct           | planar circle on a `Plane` and radius |
|  [08]   | `Arc`      | struct           | `Circle` under an angle `Interval`    |
|  [09]   | `Ray3d`    | struct           | origin-and-direction ray              |

- `Point3f`/`Vector3f` widen implicitly to their double-precision substrate twins, so a mesh buffer read crosses into kernel algebra with no explicit conversion and a write back is the narrowing the caller states.
- `Sphere`: `Center` `Radius` `Diameter` `ClosestPoint` `PointAt(lon,lat)`; `Circle`: `ClosestParameter` `PointAt`.
- `Cylinder`, `Cone`, `Torus`, and `Arc` are named-part compositions over the substrate carriers, so each reads its frame, extent, and parameter span from types the substrate already owns.

[PUBLIC_TYPE_SCOPE]: curve, mesh, brep reference geometry and topology accessors

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]           | [CAPABILITY]      |
| :-----: | :-------------- | :---------------------- | :---------------- |
|  [01]   | `Curve`         | abstract reference      | curve geometry    |
|  [02]   | `NurbsCurve`    | reference               | NURBS curve       |
|  [03]   | `PolylineCurve` | reference               | polyline curve    |
|  [04]   | `Polyline`      | `List<Point3d>` value   | polyline geometry |
|  [05]   | `Mesh`          | reference               | mesh geometry     |
|  [06]   | `Brep`          | reference               | boundary geometry |

- Reference geometry descends from the substrate's `GeometryBase`, so identity, duplication, `ObjectType`, disposal, and the bounds triple read there; this partition owns what each concrete type adds.
- `NurbsCurve` exposes control-point and knot access, the densest `Curve` realization; `PolylineCurve` lowers a polyline to `Curve`, bridging `Curve.TryGetPolyline`.
- `Brep`: `Faces` `Edges` — the parametric solid the `Analysis` layer intersects.

[PUBLIC_TYPE_SCOPE]: mesh restructure and unwrap types (the `Processing/segment` host-restructure seam)

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]            | [CAPABILITY]              |
| :-----: | :----------------------- | :----------------------- | :------------------------ |
|  [01]   | `QuadRemeshParameters`   | parameter class          | quad-remesh policy        |
|  [02]   | `QuadRemeshSymmetryAxis` | `[Flags]` enum           | symmetry axes             |
|  [03]   | `ReduceMeshParameters`   | parameter class          | decimation policy         |
|  [04]   | `MeshUnwrapper`          | reference, `IDisposable` | UV unwrap                 |
|  [05]   | `MeshUnwrapMethod`       | enum                     | parameterization selector |

- `QuadRemeshParameters`: `TargetEdgeLength` `TargetQuadCount` `AdaptiveSize` `AdaptiveQuadCount` `DetectHardEdges` `GuideCurveInfluence` `PreserveMeshArrayEdgesMode` `SymmetryAxis`; `TargetQuadCount` defaults to 2000, `AdaptiveSize` rides the native `[0,100]` unit, `AdaptiveQuadCount` and `DetectHardEdges` default true, all read-write so the receipt recovers the full set.
- `QuadRemeshSymmetryAxis`: `None=0` `X=1` `Y=2` `Z=4`, combining under `[Flags]`.
- `ReduceMeshParameters`: `DesiredPolygonCount` `AllowDistortion` `Accuracy` `NormalizeMeshSize` `FaceTags` `LockedComponents` `CancelToken` `ProgressReporter`; the host writes the internally-set `Error`.
- `MeshUnwrapper`: `MeshUnwrapper(Mesh)` and `MeshUnwrapper(IEnumerable<Mesh>)` constructors and the `SymmetryPlane` property.
- `MeshUnwrapMethod`: `LSCM` `ABFPP` `ARAP` — `LSCM` is the `Processing/segment` flatten route.

[PUBLIC_TYPE_SCOPE]: `Rhino.Geometry.Intersect` result types, cardinality enums, and the point index

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]              | [CAPABILITY]              |
| :-----: | :------------------------- | :------------------------- | :------------------------ |
|  [01]   | `Intersection`             | static class               | parametric intersection   |
|  [02]   | `CurveIntersections`       | `IList<IntersectionEvent>` | disposable hit list       |
|  [03]   | `IntersectionEvent`        | class                      | curve hit                 |
|  [04]   | `LineCircleIntersection`   | enum                       | line-circle cardinality   |
|  [05]   | `LineSphereIntersection`   | enum                       | line-sphere cardinality   |
|  [06]   | `LineCylinderIntersection` | enum                       | line-cylinder cardinality |
|  [07]   | `PlaneCircleIntersection`  | enum                       | plane-circle cardinality  |
|  [08]   | `RTree`                    | reference                  | point-neighborhood index  |
|  [09]   | `RTreeEventArgs`           | event args                 | search callback state     |

- `CurveIntersections`: `Count`, indexer, and `CopyTo` over an `IList<IntersectionEvent>` leased disposable for its read window.
- `IntersectionEvent`: `IsPoint` `IsOverlap` `PointA` `PointB` `ParameterA` `ParameterB` `OverlapA` `OverlapB`.
- `LineCircleIntersection` / `LineSphereIntersection` / `LineCylinderIntersection`: `None` `Single` `Multiple` cardinality the `Analysis` switch reads.
- `PlaneCircleIntersection`: `None` `Tangent` `Secant`; `PlaneSphere` and `SphereSphere` return a `Circle` directly via `out`.
- `RTreeEventArgs`: `EventHandler<RTreeEventArgs>` reads `Id`/`IdB` and sets `Cancel`.

[RTREE]: the host point-neighborhood index the `Spatial/neighbors` `NeighborIndex` composes.
- Construction: `RTree.CreateFromPointArray(points)` `CreatePointCloudTree(cloud)` `CreateMeshFaceTree(mesh)` `Insert(box,elementId)`.
- Search: `RTree.Search(box | sphere, callback)` and static `SearchOverlaps(treeA, treeB, tolerance, callback)`.
- Batch: static `RTree.Point3dKNeighbors(hay, needles, amount)` `Point3dClosestPoints(hay, needles, limitDistance)` `PointCloudKNeighbors` `PointCloudClosestPoints` return `IEnumerable<int[]>` leased `as IDisposable` for the read window.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: analytic primitive sampling and ray parameterization; instance members unless the surface names `static`

[PRIMITIVE_SAMPLING]: `Sphere.PointAt(lonRadians, latRadians)` `Sphere.ClosestPoint` `Sphere.ClosestParameter(pt, out lon, out lat)` `Circle.PointAt(t)` `Circle.ClosestPoint` `Circle.ClosestParameter(pt, out t)` `Arc.PointAt(t)` `Arc.ClosestParameter(pt)` — evaluate a primitive at its own parameterization, the `Analysis` cardinality switch reading the result beside the crossing verdict.

- Projection arity forks across the family: `Sphere` and `Circle` return `bool` with the parameters on `out` channels, while `Arc.ClosestParameter` returns the `double` directly and carries no failure verdict — a fold treating the three uniformly drops the `Arc` case or invents a verdict it never gets.
[RAY_SAMPLING]: `Ray3d.PointAt(t)` `Position` `Direction` — parametric ray sampling driving `SpatialQuery.Ray` and `MeshRay`.

[ENTRYPOINT_SCOPE]: curve evaluation, projection, division, classification, and transformation, and the polyline chain surface; instance members

[CURVE_EVALUATION]: `Curve.PointAt(t)` `TangentAt` `CurvatureAt` `FrameAt` `PerpendicularFrameAt` — evaluate the curve and the moving frame `Drawing` and sweep paths read.
[CURVE_PROJECTION]: `Curve.ClosestPoint(pt,out t)` `LocalClosestPoint` — nearest parameter under a max-distance gate, `LocalClosestPoint` seeding from a guess.
[CURVE_DIVISION]: `Curve.DivideByCount` `DivideByLength` `DivideEquidistant` `GetLength` — arc-length sampling for contour and toolpath resamplers.
[CURVE_CLASSIFICATION]: `Curve.TryGetPolyline(out)` `TryGetPlane(out)` `IsClosed` `IsPlanar` — exact lowering to `Polyline` or a supporting `Plane`, the kernel routing the non-exact `bool` through `Fin`.
[CURVE_TRANSFORMATION]: `Curve.Extend` `Simplify` `TrimInterval` `Offset` `ToNurbsCurve` — extend, simplify, trim, offset, and lower to canonical NURBS.
[POLYLINE_EVALUATION]: `Polyline.PointAt(t)` `ClosestPoint` `CenterPoint` `Length` `GetSegments` `IsClosed` — the crossing-chain carrier the `Meshing/intersect` `ToPolylines` projection builds.
[POLYLINE_REDUCTION]: `Polyline.ToNurbsCurve` `ToPolylineCurve` `ReduceSegments` `MergeColinearSegments` `Smooth` `BreakAtAngles` `Trim` — chain-to-curve lowering and section-curve decimation.

[ENTRYPOINT_SCOPE]: mesh storage — vertex, face, normal, topology, n-gon — and the repair, restructuring, query, factory, and boolean operations; the kernel reads the buffers without reminting them

[VERTEX_STORAGE]: `Mesh.Vertices` (`MeshVertexList`) — `Add` `SetVertex` `CombineIdentical` over `Point3f`/`Point3d` storage.
[FACE_STORAGE]: `Mesh.Faces` (`MeshFaceList`) — `MeshFace` connectivity with `GetFaceVertices` `ConvertQuadsToTriangles`.
[NORMAL_STORAGE]: `Mesh.Normals` (`MeshVertexNormalList`) `FaceNormals` (`MeshFaceNormalList`) `ComputeNormals`.
[TOPOLOGY_STORAGE]: `Mesh.TopologyVertices` `TopologyEdges` — `ConnectedTopologyVertices` `ConnectedFaces` `ConnectedEdges` `MeshVertexIndices` `IndicesFromFace`, the welded half-edge adjacency graph the narrow phase walks.
[NGON_STORAGE]: `Mesh.Ngons` (`MeshNgonList`) `GetNgonAndFacesEnumerable`.
[MESH_REPAIR]: `Mesh.RebuildNormals` `UnifyNormals` `Weld` `Unweld` `Compact` `FillHoles` `HealNakedEdges` `MergeAllCoplanarFaces` — the kernel `Processing/repair` composes only `RebuildNormals` at the boundary and authors welding and hole filling itself.
[MESH_RESTRUCTURING]: `Mesh.Reduce` `Offset` `Split` `SelfSplit` `CollapseFacesByArea` `CollapseFacesByEdgeLength` — `Reduce`/`Split`/`SelfSplit` overloads thread `CancellationToken` and `IProgress<double>`.
[MESH_QUERY]: `Mesh.ClosestPoint` `ClosestMeshPoint` `Volume` `GetNakedEdges` `GetSelfIntersections`.
[MESH_FACTORY]: `Mesh.CreateFromBox` `CreateFromSphere` `CreateFromCone` `CreateFromClosedPolyline` `CreateFromTessellation` — the primitive-to-mesh family meshing pages seed from.
[MESH_BOOLEANS]: `Mesh.CreateBooleanUnion` `CreateBooleanDifference` `CreateBooleanIntersection` → `Mesh[]` with an `out Result`; the kernel predicate-exact arrangement does not use this host CSG path.

[ENTRYPOINT_SCOPE]: native remesh, reduce, and unwrap calls (the `Processing/segment` seam)

Instance forms return a new `Mesh`, null on failure — the kernel `Fin`-routes the null and disposes the orphan; `Reduce` mutates in place returning success `bool`; cooperative overloads thread `IProgress<int>`+`CancellationToken`.

[QUAD_REMESH]: `Mesh.QuadRemesh(QuadRemeshParameters)` and `(parameters, IEnumerable<Curve> guideCurves)`.
[BLOCK_REMESH]: `Mesh.QuadRemesh(IEnumerable<int> faceBlocks, QuadRemeshParameters, IEnumerable<Curve> guideCurves, IProgress<int>, CancellationToken)` — the `Processing/segment` quad arm.
[ASYNC_REMESH]: `Mesh.QuadRemeshAsync(parameters, progress, cancelToken)` with the guide-curve and face-block overloads → `Task<Mesh>`.
[BREP_REMESH]: `Mesh.QuadRemeshBrep(Brep, QuadRemeshParameters)` with guide-curve and cooperative overloads and the `QuadRemeshBrepAsync` pair.
[MESH_REDUCTION]: `Mesh.Reduce(ReduceMeshParameters)` `Reduce(parameters, bool threaded)` and the scalar `Reduce(desiredPolygonCount, allowDistortion, accuracy, normalizeSize[, threaded | cancelToken, progress])`, writing failure to `parameters.Error`.
[MESH_UNWRAP]: `new MeshUnwrapper(mesh)` then `Unwrap(MeshUnwrapMethod.LSCM)` → `bool`, writing `Mesh.TextureCoordinates` checked against `Vertices.Count`; the flatten route disposes the unwrapper under `using`.

[ENTRYPOINT_SCOPE]: `Intersection` parametric crossings (the `Analysis` layer's surface); every member is static

| [INDEX] | [SURFACE]                                         | [CAPABILITY]                     |
| :-----: | :------------------------------------------------ | :------------------------------- |
|  [01]   | `Intersection.LineLine(a,b,out ta,out tb)`        | line-line, both parameters       |
|  [02]   | `Intersection.LinePlane(line,plane,out t)`        | line-plane parameter             |
|  [03]   | `Intersection.LineBox(line,box,tol,out Interval)` | line-box parameter interval      |
|  [04]   | `Intersection.LineCircle`                         | line-circle cardinality + points |
|  [05]   | `Intersection.LineSphere`                         | line-sphere cardinality          |
|  [06]   | `Intersection.LineCylinder`                       | line-cylinder cardinality        |
|  [07]   | `Intersection.PlanePlane(out Line)`               | plane-plane line                 |
|  [08]   | `Intersection.PlanePlanePlane(out Point3d)`       | three-plane point                |
|  [09]   | `Intersection.PlaneCircle`                        | plane-circle cardinality         |
|  [10]   | `Intersection.PlaneSphere(out Circle)`            | plane-sphere circle              |
|  [11]   | `Intersection.SphereSphere(out Circle)`           | sphere-sphere circle             |
|  [12]   | `Intersection.CurveCurve(a,b,tol,overlapTol)`     | curve hits, leased list          |
|  [13]   | `Intersection.MeshRay(mesh,ray,out faceIds)`      | ray parameter with face ids      |
|  [14]   | `Intersection.RayShoot(ray,geometry,maxRefl)`     | reflective cast                  |
|  [15]   | `Intersection.ProjectPointsToMeshes(...)`         | directional point drape          |
|  [16]   | `Intersection.ProjectPointsToMeshesEx(out idx)`   | drape with source indices        |

- `Intersection.CurveCurve`: `CurveIntersections` (`IList<IntersectionEvent>`) leases disposable for its read window; the host owns the NURBS and Brep parametric path.
- `Intersection.RayShoot`: `RayShootEvent[]` from a reflective cast against parametric geometry.

[CURVE_INTERSECTION]: `Intersection.CurveLine` `CurvePlane` `CurveSelf` `CurveSurface` `CurveBrep` — curve-vs-primitive parametric hits, each leasing `CurveIntersections`.
[MESH_INTERSECTION]: `Intersection.MeshLine` `MeshLineSorted` `MeshPolyline` — host mesh-vs-line and mesh-vs-polyline point probes.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every kernel op reads a `Rhino.Geometry` value type's full member surface and composes it through `Rasm.Numerics`, never re-deriving the operation nor re-minting the type.

[STACKING]:
- `RhinoCommon` value substrate(`libs/csharp/.api/api-rhinocommon.md`): the point, vector, frame, segment, extent, affine, and rotor algebra resolves there whole; this partition composes it — `Point3f`/`Vector3f` widen into it, every primitive solid is a named-part composition over its carriers, and every reference type inherits its `GeometryBase` bounds and identity.
- `Rasm.Numerics`: substrate value structs cross into the numeric floor as its carriers, and the kernel reads `SquareLength`/`DecomposeAffine`/`Union` beside this partition's `Mesh.TopologyVertices` rather than re-deriving; the structure-of-arrays frame store reads and writes `Plane.OriginX`/`OriginY`/`OriginZ`, and the degeneracy guard reads `Vector3d.SquareLength`.
- `Spatial/index` SAH-BVH: the substrate `BoundingBox.Empty` seeds and `Union` accumulates the broad-phase AABB the `NodeStore` holds.
- `Meshing/intersect` narrow phase: `MeshFace` triangle-soup indices and `Mesh.TopologyVertices` welded adjacency back the Guigue-Devillers predicate-exact straddle, and `Polyline` re-emits the crossing chains.
- `Spatial/neighbors` `NeighborIndex`: composes host `RTree` construction, callback search, and the hay-by-needle batches inside its callback capsule.
- `Analysis/Intersect` lattice: folds `Intersection` parametric primitive-pair crossings and disposes each `CurveIntersections` under a lease.
- `Supercluster.KDTree`(`.api/api-kdtree.md`): the flat kd-tree owns exact k-NN leaf queries while host `RTree` serves the `NeighborIndex` point-neighborhood tier — neither reimplements the kernel SAH-BVH broad-phase.

[LOCAL_ADMISSION]:
- Reference geometry is the kernel's disposable native-handle owner, released at the seam.
- Geometry values cross the seam as `Rasm.Numerics` carriers, the robust core re-emitting `Polyline`/`Point3d`/`Mesh` at the boundary.
- Substrate carriers read their members from the branch catalogue, and a row for one lands there rather than here, so the kernel and both host boundaries meet one spelling.

[RAIL_LAW]:
- Partition: RhinoCommon kernel crossing (`Rhino.Geometry`, `Rhino.Geometry.Intersect`, `Rhino.Geometry.Collections`)
- Owns: the single-precision mesh carriers and analytic primitive solids, the curve, polyline, mesh, and brep reference geometry with its typed topology, the native remesh/reduce/unwrap seam, the host parametric intersection (`Intersection.Curve*`/`Brep*`/`Ray*`) the `Analysis` layer composes, and the host `RTree` point-neighborhood tier the `Spatial/neighbors` `NeighborIndex` composes.
- Accept: reference geometry read through its full member surface over the registered substrate carriers; the `Analysis/Intersect` parametric lattice disposing each `CurveIntersections` under a lease; the geometry-only surface below the document, view, command, and display strata.
- Reject: a kernel-local re-mint of a Rhino value type (a domain `Aabb`/`Ray`/`Vec3` duplicating `BoundingBox`/`Ray3d`/`Vector3d`); a re-tabling of the substrate carrier algebra; an epsilon-snapped coordinate where the robust core owns an exact construction; a kernel discrete crossing or primitive broad-phase routed through host `Intersection.Mesh*` or `RTree` where the predicate-exact straddle is required; a `RhinoDoc`/`RhinoApp`/`RhinoView`/`DisplayConduit`/`ObjectTable` reach from the kernel.
