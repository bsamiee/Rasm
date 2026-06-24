# [RASM_API_RHINO]

`Rhino.Geometry` is the host-ABI value-type geometry vocabulary the kernel reads, composes through `Rasm.Vectors`, and re-emits at the seam — points, vectors, transforms, intervals, bounding volumes, primitive solids, NURBS curves, meshes with strongly-typed topology lists, and the `Rhino.Geometry.Intersect` parametric-intersection surface — never re-minted as a parallel kernel primitive and never reached through a document, view, command, or display surface. The kernel authors its discrete predicate-exact crossing and broad-phase acceleration FROM FIRST PRINCIPLES over this value vocabulary (`Mesh`/`MeshFace`/`Plane`/`Ray3d`/`BoundingBox`); the host `Intersection` parametric curve/surface surface is the `Analysis` layer's concern alone, and the two meet at no interior.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `RhinoCommon`
- package: `RhinoCommon` (Rhino.Inside / RhinoWIP host bundle; not a NuGet pin — the in-process `RhinoCommon.dll` is the resolved asset)
- assembly: `RhinoCommon`
- namespace: `Rhino.Geometry`
- namespace: `Rhino.Geometry.Intersect`
- namespace: `Rhino.Geometry.Collections` (`Mesh*List` topology accessors)
- asset: host assembly (native-backed; the managed structs wrap an `opennurbs` C++ core through `UnsafeNativeMethods` P/Invoke)
- abi: value structs marshal blittably; reference geometry (`Mesh`/`Curve`/`Brep`/`GeometryBase`) owns native handles and is `IDisposable` — a long-lived kernel must not leak the unmanaged buffer, and a struct copy is a managed-side value copy, never a native alias
- runtime: net48 in-Rhino ALC for plugin assemblies; the geometry-only surface below the document stratum is the sole kernel-admitted slice
- rail: host-rhino

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: point, vector, frame, and rotation value structs
- rail: host-rhino

| [INDEX] | [SYMBOL]     | [KIND]            | [CAPABILITY]                                                                                              |
| :-----: | :----------- | :---------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Point3d`    | blittable struct  | double point; `X`/`Y`/`Z`, `DistanceTo`/`DistanceToSquared`, full ordered operators, `Point3f` interop    |
|  [02]   | `Point3f`    | blittable struct  | single point; implicit-widens to `Point3d`, mesh-vertex storage scalar                                    |
|  [03]   | `Vector3d`   | blittable struct  | double vector; the kernel's settled direction carrier composed through `Rasm.Vectors`                    |
|  [04]   | `Vector3f`   | blittable struct  | single vector; mesh-normal storage scalar, implicit-widens to `Vector3d`                                 |
|  [05]   | `Transform`  | 4x4 struct        | affine transform; `M00`..`M33` row-major, factory family, decomposition, `operator *` on self and points |
|  [06]   | `Quaternion` | struct            | rotation rotor; `GetRotation` to axis-angle / `Plane` / `Transform`, `CreateFromRotationZYX`/`ZYZ`        |
|  [07]   | `Interval`   | struct            | scalar span; `Min`/`Max`/`Mid`/`Length`, `ParameterAt`/`NormalizedParameterAt`, `FromUnion`/`FromIntersection` |
|  [08]   | `Plane`      | struct            | oriented frame; `Origin`/`Normal`/`XAxis`/`YAxis`/`ZAxis`, `ClosestPoint`/`RemapToPlaneSpace`/`ValueAt`   |

[PUBLIC_TYPE_SCOPE]: bounding, primitive-solid, and ray value structs
- rail: host-rhino

| [INDEX] | [SYMBOL]      | [KIND]           | [CAPABILITY]                                                                                  |
| :-----: | :------------ | :--------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `BoundingBox` | struct           | axis-aligned box; `Min`/`Max`, `Union`/`Intersection`/`Contains`/`ClosestPoint`/`Inflate`, the broad-phase AABB the kernel BVH stores |
|  [02]   | `Box`         | struct           | oriented box; `Plane` + three `Interval` extents                                              |
|  [03]   | `Sphere`      | struct           | sphere primitive; `Center`/`Radius`/`Diameter`, `ClosestPoint`/`PointAt(lon,lat)`             |
|  [04]   | `Cylinder`    | struct           | cylinder primitive; axis circle + height                                                      |
|  [05]   | `Cone`        | struct           | cone primitive; apex/axis/radius                                                               |
|  [06]   | `Torus`       | struct           | torus primitive; major/minor radii                                                            |
|  [07]   | `Circle`      | struct           | circle primitive; `Plane` + radius, `ClosestParameter`/`PointAt`                              |
|  [08]   | `Arc`         | struct           | arc primitive; circle + angle `Interval`                                                       |
|  [09]   | `Ray3d`       | struct           | ray primitive; `Position`/`Direction`/`PointAt(t)` — the ray the kernel `SpatialQuery.Ray` walks |
|  [10]   | `Line`        | struct           | segment primitive; `From`/`To`/`Length`/`Direction`, `PointAt`/`ClosestPoint`/`MinimumDistanceTo`/`ExtendThroughBox` |

[PUBLIC_TYPE_SCOPE]: curve, mesh, brep reference geometry and topology accessors
- rail: host-rhino

| [INDEX] | [SYMBOL]               | [KIND]                  | [CAPABILITY]                                                                                      |
| :-----: | :--------------------- | :---------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `GeometryBase`         | abstract reference root | `IDisposable`; `GetBoundingBox(bool)`/`GetBoundingBox(Plane,out Box)`, `Transform`, `Duplicate`, `ObjectType` — the polymorphic root `RayShoot` consumes |
|  [02]   | `Curve`                | abstract reference      | curve geometry; `PointAt`/`ClosestPoint`/`FrameAt`/`PerpendicularFrameAt`, `TryGetPolyline`/`TryGetPlane`, `Extend`/`Simplify`/`Offset` |
|  [03]   | `NurbsCurve`           | reference               | nurbs curve; control-point / knot access, the densest `Curve` realization                        |
|  [04]   | `PolylineCurve`        | reference               | polyline-as-`Curve`; the `Intersection.MeshPolyline` and `Curve.TryGetPolyline` bridge            |
|  [05]   | `Polyline`             | `List<Point3d>` value   | polyline geometry; `Length`/`PointAt`/`ClosestPoint`/`GetSegments`/`CenterPoint`, `ToNurbsCurve`/`ToPolylineCurve`, `ReduceSegments`/`MergeColinearSegments` — the kernel's crossing-chain re-emit carrier |
|  [06]   | `Mesh`                 | reference               | mesh geometry; topology lists below, the booleans/repair/reduce surface, `ClosestPoint`/`Volume`/`GetNakedEdges` |
|  [07]   | `MeshFace`             | blittable struct        | mesh face record; `A`/`B`/`C`/`D` vertex indices, `IsTriangle`/`IsQuad` — the triangle-soup index the predicate-exact narrow-phase reads |
|  [08]   | `Brep`                 | reference               | boundary geometry; `Faces`/`Edges`, the parametric solid the `Analysis` layer intersects          |

[PUBLIC_TYPE_SCOPE]: `Rhino.Geometry.Intersect` parametric-intersection surface
- rail: host-rhino

| [INDEX] | [SYMBOL]                   | [KIND]                 | [CAPABILITY]                                                                                          |
| :-----: | :------------------------- | :--------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `Intersection`             | static class           | the host parametric-intersection surface; `Line*`/`Curve*`/`Plane*`/`Mesh*`/`RayShoot` (entrypoints below) |
|  [02]   | `CurveIntersections`       | `IList<IntersectionEvent>` | disposable hit list from `CurveCurve`/`CurveLine`/`CurvePlane`/`CurveSurface`; `Count`/indexer/`CopyTo` |
|  [03]   | `IntersectionEvent`        | class                  | one curve-intersection hit; `IsPoint`/`IsOverlap`, `PointA`/`PointB`, `ParameterA`/`ParameterB`, `OverlapA`/`OverlapB` |
|  [04]   | `LineCircleIntersection`   | enum                   | `None`/`Single`/`Multiple` — the `LineCircle` cardinality the `Analysis` switch reads                  |
|  [05]   | `LineSphereIntersection`   | enum                   | `None`/`Single`/`Multiple` — the `LineSphere` cardinality                                              |
|  [06]   | `LineCylinderIntersection` | enum                   | `None`/`Single`/`Multiple` — the `LineCylinder` cardinality                                            |
|  [07]   | `PlaneCircleIntersection`  | enum                   | `None`/`Tangent`/`Secant` — the `PlaneCircle` cardinality (`PlaneSphere`/`SphereSphere` return `Circle` directly via `out`) |
|  [08]   | `RTree`                    | reference (NOT used)   | host R-tree broad-phase; the kernel authors its OWN SAH-BVH / Morton-octree over a flat `NodeStore`, so `RTree` is the deleted host-acceleration form, documented only as the boundary the kernel does not cross |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: vector, transform, quaternion, and interval operations
- rail: host-rhino

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT] | [CALL_SHAPE / NOTE]                                                                                    |
| :-----: | :----------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `CrossProduct(a,b)` / `operator *(a,b)`    | `Vector3d`     | cross product; `operator *` is the dot product — both static, the kernel composes through `Rasm.Vectors` |
|  [02]   | `Unitize()` / `IsUnitVector`               | `Vector3d`     | in-place normalize returning success `bool`; `IsTiny`/`IsZero` guard the degenerate direction          |
|  [03]   | `IsParallelTo` / `IsPerpendicularTo`       | `Vector3d`     | `int` (-1/0/1) parallelism and `bool` perpendicularity under an angle tolerance — the orientation tests the predicate floor backs |
|  [04]   | `VectorAngle(a,b[,plane\|vNormal])`        | `Vector3d`     | signed/unsigned angle; the planar-overlay and frame builders read it                                   |
|  [05]   | `PerpendicularTo` / `Rotate`               | `Vector3d`     | builds a perpendicular vector / rotates in place about an axis                                          |
|  [06]   | `Rotation` / `Mirror` / `PlaneToPlane` / `ChangeBasis` / `ProjectAlong` / `PlanarProjection` / `Scale` / `Translation` / `Diagonal` / `RotationZYX` | `Transform`    | the static affine-factory family — one named constructor per affine kind, never re-minted             |
|  [07]   | `Multiply(a,b)` / `operator *`             | `Transform`    | composes two transforms; `operator *(Transform,Point3d)` applies to a point                            |
|  [08]   | `TryGetInverse(out)`                       | `Transform`    | `bool` inverse with the singular case as the failure branch — the kernel routes it through `Fin`       |
|  [09]   | `DecomposeAffine` / `DecomposeSymmetric` / `GetEulerZYZ` / `GetYawPitchRoll` / `GetQuaternion` | `Transform`    | factors a transform into translation+rotation+scale, Euler angles, or a rotor                          |
|  [10]   | `Determinant` / `IsAffine` / `IsUniformlyScaled` / `Orthogonalize` | `Transform`    | invariants and the Gram-Schmidt re-orthogonalization that re-conditions a drifted basis                |
|  [11]   | `GetRotation(out angle,out axis \| out Plane \| out Transform)` | `Quaternion`   | reads the rotor as axis-angle / oriented frame / matrix; `Conjugate`/`Inverse`/`Unitize`, `operator *` rotor compose |
|  [12]   | `ParameterAt` / `NormalizedParameterAt` / `IncludesParameter` | `Interval`     | parameter <-> normalized remap and containment — the curve-domain and sampling arithmetic              |

[ENTRYPOINT_SCOPE]: plane, bounds, and ray operations
- rail: host-rhino

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT] | [CALL_SHAPE / NOTE]                                                                              |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `ClosestPoint` / `RemapToPlaneSpace` / `ClosestParameter` | `Plane`        | frame projection and world<->plane-space remap; `ValueAt`/`DistanceTo` give the signed plane distance the `PlaneMesh` straddle reads |
|  [02]   | `GetPlaneEquation` / `Rotate` / `Transform` / `Translate` / `Flip` | `Plane`        | the plane equation `[a,b,c,d]`, in-place reframing, and the `ExtendThroughBox` parameter window  |
|  [03]   | `CreateFromNormal` / `CreateFromPoints` / `CreateFromFrame` / `FitPlaneToPoints` | `Plane`        | the static frame-construction family; `FitPlaneToPoints` is the least-squares fit the `Processing/fit` page composes |
|  [04]   | `Union(a,b)` / `Union(box,point)` / `Intersection(a,b)` | `BoundingBox`  | static and in-place AABB merge / overlap — the broad-phase the kernel BVH `NodeStore` accumulates |
|  [05]   | `Contains` / `ClosestPoint` / `FurthestPoint` / `Corner` / `GetCorners` / `PointAt` | `BoundingBox`  | point-in-box, nearest/farthest on the box, the eight corners, parametric sampling               |
|  [06]   | `Inflate` / `Transform` / `MakeValid` / `IsDegenerate` / `Volume` / `Area` / `Diagonal` | `BoundingBox`  | grow/shrink, transform the box, validity and degeneracy classification, extent measures          |
|  [07]   | `PointAt(t)` / `Position` / `Direction`            | `Ray3d`        | parametric ray sampling — the `SpatialQuery.Ray` and `MeshRay` driver                            |

[ENTRYPOINT_SCOPE]: curve and polyline operations
- rail: host-rhino

| [INDEX] | [SURFACE]                                       | [SURFACE_ROOT] | [CALL_SHAPE / NOTE]                                                                              |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `PointAt(t)` / `TangentAt` / `CurvatureAt` / `FrameAt` / `PerpendicularFrameAt` | `Curve`        | curve evaluation and the moving frame the `Drawing` and sweep paths ride                         |
|  [02]   | `ClosestPoint(pt,out t[,maxDist])` / `LocalClosestPoint` | `Curve`        | nearest-parameter projection with a maximum-distance gate; `LocalClosestPoint` seeds from a guess |
|  [03]   | `DivideByCount` / `DivideByLength` / `DivideEquidistant` / `GetLength` | `Curve`        | arc-length sampling — the contour and toolpath resamplers                                        |
|  [04]   | `TryGetPolyline(out)` / `TryGetPlane(out)` / `IsClosed` / `IsPlanar` | `Curve`        | lowers a curve to its `Polyline` / supporting `Plane` when exact, else fails the `bool` — the kernel `Fin`-routes the non-exact branch |
|  [05]   | `Extend` / `Simplify` / `TrimInterval` / `Offset` / `ToNurbsCurve` | `Curve`        | extend/simplify/trim/offset and the canonical NURBS lowering                                     |
|  [06]   | `PointAt(t)` / `ClosestPoint` / `CenterPoint` / `Length` / `GetSegments` | `Polyline`     | the crossing-chain re-emit carrier the `Meshing/intersect` `ToPolylines` projection builds        |
|  [07]   | `ToNurbsCurve` / `ToPolylineCurve` / `ReduceSegments` / `MergeColinearSegments` / `Smooth` / `BreakAtAngles` / `Trim` | `Polyline`     | chain-to-curve lowering and the segment-decimation the section curve emits                        |

[ENTRYPOINT_SCOPE]: mesh topology access and operations
- rail: host-rhino

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT] | [CALL_SHAPE / NOTE]                                                                              |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `Vertices` (`MeshVertexList`)                      | `Mesh`         | vertex `Point3f`/`Point3d` store with `Add`/`SetVertex`/`CombineIdentical` — the kernel reads positions, never re-mints the buffer |
|  [02]   | `Faces` (`MeshFaceList`)                           | `Mesh`         | `MeshFace` connectivity with `GetFaceVertices`/`ConvertQuadsToTriangles` — the triangle soup the narrow-phase indexes |
|  [03]   | `Normals` (`MeshVertexNormalList`) / `FaceNormals` (`MeshFaceNormalList`) | `Mesh`         | per-vertex and per-face normals; `ComputeNormals` derives them                                   |
|  [04]   | `TopologyVertices` (`MeshTopologyVertexList`) / `TopologyEdges` (`MeshTopologyEdgeList`) | `Mesh`         | the welded adjacency graph — `ConnectedTopologyVertices`/`ConnectedFaces`/`ConnectedEdges`/`MeshVertexIndices`/`IndicesFromFace`, the half-edge surface the kernel walks |
|  [05]   | `Ngons` (`MeshNgonList`) / `GetNgonAndFacesEnumerable` | `Mesh`         | n-gon grouping over the triangulated faces                                                       |
|  [06]   | `RebuildNormals` / `UnifyNormals` / `Weld` / `Unweld` / `Compact` / `FillHoles` / `HealNakedEdges` / `MergeAllCoplanarFaces` | `Mesh`         | the repair surface the `Processing/repair` page composes — orientation, welding, hole-filling     |
|  [07]   | `Reduce` / `Offset` / `Split` / `SelfSplit` / `CollapseFacesByArea` / `CollapseFacesByEdgeLength` | `Mesh`         | decimation/offset/splitting; the `Reduce`/`Split`/`SelfSplit` overloads take `CancellationToken`+`IProgress<double>` — the host's long-running cooperative form |
|  [08]   | `ClosestPoint(pt[,out pt,out normal,maxDist])` / `ClosestMeshPoint` / `Volume` / `GetNakedEdges` / `GetSelfIntersections` | `Mesh`         | nearest-point with optional normal, enclosed volume, naked-edge polylines, self-intersection probe |
|  [09]   | `CreateFromBox` / `CreateFromSphere` / `CreateFromCone` / `CreateFromClosedPolyline` / `CreateFromTessellation` | `Mesh`         | the primitive-to-mesh factory family the meshing pages seed from                                  |
|  [10]   | `CreateBooleanUnion` / `CreateBooleanDifference` / `CreateBooleanIntersection` | `Mesh`         | host CSG booleans (return `Mesh[]` with an `out Result`) — the host parametric path the kernel's predicate-exact arrangement does NOT use, documented as the boundary |

[ENTRYPOINT_SCOPE]: `Intersection` parametric-intersection operations (the `Analysis` layer's surface)
- rail: host-rhino

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT] | [CALL_SHAPE / NOTE]                                                                              |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `LineLine(a,b,out ta,out tb[,tol,finiteSegments])` | `Intersection` | `bool` line-line crossing with the two parameters; the `Analysis/Intersect` `Line·Line` case      |
|  [02]   | `LinePlane(line,plane,out t)` / `LineBox(line,box,tol,out Interval)` | `Intersection` | `bool` line-vs-plane parameter / line-vs-box parameter `Interval` — the `Line·Plane` / `Line·Box` cases |
|  [03]   | `LineCircle` / `LineSphere` / `LineCylinder`       | `Intersection` | returns the cardinality enum + two `out Point3d`; the `Analysis` switch reads `Single`/`Multiple` |
|  [04]   | `PlanePlane(out Line)` / `PlanePlanePlane(out Point3d)` / `PlaneCircle` / `PlaneSphere(out Circle)` / `SphereSphere(out Circle)` | `Intersection` | the analytic primitive-pair crossings the `Analysis` parametric lattice folds                    |
|  [05]   | `CurveCurve(a,b,tol,overlapTol)` / `CurveLine` / `CurvePlane` / `CurveSelf` / `CurveSurface` / `CurveBrep` | `Intersection` | returns `CurveIntersections` (`IList<IntersectionEvent>`, disposable — lease it); the NURBS/Brep parametric path host-owned, never re-authored by the kernel |
|  [06]   | `MeshRay(mesh,ray[,out faceIds])` / `MeshLine` / `MeshLineSorted` / `MeshPolyline`         | `Intersection` | host mesh-ray/line probes returning the parameter or `Point3d[]` + face ids — available, but the kernel `SpatialQuery.Ray` + predicate-exact narrow-phase is the owned discrete path |
|  [07]   | `RayShoot(ray,IEnumerable<GeometryBase>,maxReflections)` / `RayShoot(geometry,ray,maxReflections)` (`RayShootEvent[]`) | `Intersection` | reflective ray casting against parametric geometry — the `Analysis` ray-trace surface             |
|  [08]   | `ProjectPointsToMeshes` / `ProjectPointsToMeshesEx(out indices)` | `Intersection` | host point-to-mesh projection along a direction — the parametric draping the kernel's BVH does not replace |

## [04]-[IMPLEMENTATION_LAW]

[GEOMETRY_VALUE_LAW]:
- Package: `RhinoCommon` (`Rhino.Geometry`)
- Owns: the value-type geometry vocabulary the kernel reads, composes through `Rasm.Vectors`, and re-emits at the seam (`Point3d`/`Vector3d`/`Plane`/`Line`/`Polyline`/`Mesh`/`MeshFace`/`BoundingBox`/`Ray3d`/`Transform`/`Interval`)
- Accept: `Rhino.Geometry` value types composed through `Rasm.Vectors`; the kernel reads a struct's full member surface (`Vector3d.IsParallelTo`, `Transform.DecomposeAffine`, `BoundingBox.Union`, `Mesh.TopologyVertices`) rather than re-deriving the operation
- Reject: a kernel-local re-mint of a Rhino value type (a domain `Aabb`/`Ray`/`Vec3` duplicating `BoundingBox`/`Ray3d`/`Vector3d`); an epsilon-snapped coordinate where the robust-core owns an exact construction; a thin pass-through wrapper that renames a struct member without adding domain value

[PREDICATE_EXACT_BOUNDARY]:
- Package: `RhinoCommon` (`Rhino.Geometry.Intersect`, `Rhino.Geometry.RTree`)
- Owns: the host parametric curve/surface/solid intersection (`Intersection.Curve*`/`Brep*`/`Ray*`) and the host R-tree broad-phase — the `Analysis` layer's parametric concern
- Accept: the `Analysis/Intersect.cs` parametric lattice composing `Intersection.LineLine`/`LinePlane`/`PlanePlane`/`LineCircle`/`LineSphere`/`LineBox`/`CurveLine`/`CurveCurve`/`CurvePlane` and disposing each `CurveIntersections` under a lease; the host owns parametric, the kernel owns discrete
- Reject: a kernel discrete mesh-mesh / plane-mesh / segment-triangle crossing routed through host `Intersection.MeshMesh*` or a kernel broad-phase routed through `RTree` — the `Meshing/intersect` owner authors the Guigue-Devillers predicate-exact narrow-phase over `Mesh`/`MeshFace`/`Plane`/`Ray3d` and the `Spatial/index` owner authors the SAH-BVH / Morton-octree over a flat `NodeStore`, and a host parametric crossing where the predicate-exact straddle is required is the named non-robustness defect

[BOUNDARY_LAW]:
- Package: `RhinoCommon` (`Rhino.Geometry`)
- Owns: the geometry-only Rhino surface below the document, view, command, and display strata
- Accept: geometry values cross the seam as `Rasm.Vectors` carriers, the robust-core re-emitting `Polyline`/`Point3d`/`Mesh` results at the boundary; reference geometry (`Mesh`/`Curve`/`Brep`) is the kernel's disposable native-handle owner, released at the seam
- Reject: a `RhinoDoc`/`RhinoApp`/`RhinoView`/`DisplayConduit`/`ObjectTable` reach from the kernel — the document/view/command/display surface is the host-boundary stratum's concern, never the kernel's
