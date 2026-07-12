# [RASM_API_RHINO]

`Rhino.Geometry` is the host-ABI value-type geometry vocabulary the kernel reads, composes through `Rasm.Numerics`, and re-emits at the seam — points, vectors, transforms, intervals, bounding volumes, primitive solids, NURBS curves, meshes with strongly-typed topology lists, and the `Rhino.Geometry.Intersect` parametric-intersection surface — never re-minted as a parallel kernel primitive and never reached through a document, view, command, or display surface. The kernel authors its discrete predicate-exact crossing and broad-phase acceleration FROM FIRST PRINCIPLES over this value vocabulary (`Mesh`/`MeshFace`/`Plane`/`Ray3d`/`BoundingBox`); the host `Intersection` parametric curve/surface surface is the `Analysis` layer's concern alone, and the two meet at no interior.

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

| [INDEX] | [SYMBOL]     | [KIND]           | [CAPABILITY]             |
| :-----: | :----------- | :--------------- | :----------------------- |
|  [01]   | `Point3d`    | blittable struct | double-precision point   |
|  [02]   | `Point3f`    | blittable struct | mesh-vertex point        |
|  [03]   | `Vector3d`   | blittable struct | kernel direction carrier |
|  [04]   | `Vector3f`   | blittable struct | mesh-normal vector       |
|  [05]   | `Transform`  | 4x4 struct       | affine transform         |
|  [06]   | `Quaternion` | struct           | rotation rotor           |
|  [07]   | `Interval`   | struct           | scalar span              |
|  [08]   | `Plane`      | struct           | oriented frame           |

[POINT3D]:

- Members: `X`/`Y`/`Z`, `Origin`, `DistanceTo`/`DistanceToSquared`, full ordered operators, and `Point3f` interop
- Kernel: `Origin` is the `(0,0,0)` anchor that seeds offset and fit folds

[POINT3F]:

- Precision: Single-precision point
- Conversion: Implicitly widens to `Point3d`
- Storage: Mesh-vertex scalar

[VECTOR3D]:

- Precision: Double-precision vector
- Members: `Length`/`SquareLength`, `Zero`, and `XAxis`/`YAxis`/`ZAxis`
- Kernel: `Rasm.Numerics` composes this direction carrier, and the unit and degeneracy guard reads `SquareLength`

[VECTOR3F]:

- Precision: Single-precision vector
- Conversion: Implicitly widens to `Vector3d`
- Storage: Mesh-normal scalar

[TRANSFORM]:

- Members: Row-major `M00`..`M33`, factory family, decomposition, and `operator *` on self and points

[QUATERNION]:

- Members: `GetRotation` to axis-angle, `Plane`, or `Transform`; `CreateFromRotationZYX`/`ZYZ`

[INTERVAL]:

- Members: `Min`/`Max`/`Mid`/`Length`, `ParameterAt`/`NormalizedParameterAt`, and `FromUnion`/`FromIntersection`

[PLANE]:

- Members: `Origin`/`OriginX`/`OriginY`/`OriginZ`, `Normal`, `XAxis`/`YAxis`/`ZAxis`, and `ClosestPoint`/`RemapToPlaneSpace`/`ValueAt`
- Kernel: The flattened structure-of-arrays frame store reads and writes the scalar `OriginX`/`OriginY`/`OriginZ` members

[PUBLIC_TYPE_SCOPE]: bounding, primitive-solid, and ray value structs

- rail: host-rhino

| [INDEX] | [SYMBOL]      | [KIND] | [CAPABILITY]       |
| :-----: | :------------ | :----- | :----------------- |
|  [01]   | `BoundingBox` | struct | axis-aligned box   |
|  [02]   | `Box`         | struct | oriented box       |
|  [03]   | `Sphere`      | struct | sphere primitive   |
|  [04]   | `Cylinder`    | struct | cylinder primitive |
|  [05]   | `Cone`        | struct | cone primitive     |
|  [06]   | `Torus`       | struct | torus primitive    |
|  [07]   | `Circle`      | struct | circle primitive   |
|  [08]   | `Arc`         | struct | arc primitive      |
|  [09]   | `Ray3d`       | struct | ray primitive      |
|  [10]   | `Line`        | struct | segment primitive  |

[BOUNDING_BOX]:

- Members: `Min`/`Max`, `Empty`, and `Union`/`Intersection`/`Contains`/`ClosestPoint`/`Inflate`
- Kernel: `Empty` seeds BVH AABB accumulation with unset inverted extents, and the kernel BVH stores the result as its broad-phase AABB

[BOX]:

- Shape: `Plane` with three `Interval` extents

[SPHERE]:

- Members: `Center`/`Radius`/`Diameter` and `ClosestPoint`/`PointAt(lon,lat)`

[CYLINDER]:

- Shape: Axis circle and height

[CONE]:

- Shape: Apex, axis, and radius

[TORUS]:

- Shape: Major and minor radii

[CIRCLE]:

- Shape: `Plane` and radius
- Members: `ClosestParameter`/`PointAt`

[ARC]:

- Shape: Circle and angle `Interval`

[RAY3D]:

- Members: `Position`/`Direction`/`PointAt(t)`
- Kernel: `SpatialQuery.Ray` walks this ray

[LINE]:

- Members: `From`/`To`/`Length`/`Direction` and `PointAt`/`ClosestPoint`/`MinimumDistanceTo`/`ExtendThroughBox`

[PUBLIC_TYPE_SCOPE]: curve, mesh, brep reference geometry and topology accessors

- rail: host-rhino

| [INDEX] | [SYMBOL]        | [KIND]                  | [CAPABILITY]      |
| :-----: | :-------------- | :---------------------- | :---------------- |
|  [01]   | `GeometryBase`  | abstract reference root | geometry root     |
|  [02]   | `Curve`         | abstract reference      | curve geometry    |
|  [03]   | `NurbsCurve`    | reference               | NURBS curve       |
|  [04]   | `PolylineCurve` | reference               | polyline curve    |
|  [05]   | `Polyline`      | `List<Point3d>` value   | polyline geometry |
|  [06]   | `Mesh`          | reference               | mesh geometry     |
|  [07]   | `MeshFace`      | blittable struct        | mesh-face record  |
|  [08]   | `Brep`          | reference               | boundary geometry |

[GEOMETRY_BASE]:

- Contract: `IDisposable`
- Members: `GetBoundingBox(bool)`/`GetBoundingBox(Plane,out Box)`, `Transform`, `Duplicate`, and `ObjectType`
- Consumer: `RayShoot` consumes this polymorphic root

[CURVE]:

- Members: `PointAt`/`ClosestPoint`/`FrameAt`/`PerpendicularFrameAt`, `TryGetPolyline`/`TryGetPlane`, and `Extend`/`Simplify`/`Offset`

[NURBS_CURVE]:

- Members: Control-point and knot access
- Shape: Densest `Curve` realization

[POLYLINE_CURVE]:

- Shape: Polyline as `Curve`
- Seam: Bridges `Intersection.MeshPolyline` and `Curve.TryGetPolyline`

[POLYLINE]:

- Members: `Length`/`IsClosed`/`PointAt`/`ClosestPoint`/`GetSegments`/`CenterPoint`, `ToNurbsCurve`/`ToPolylineCurve`, and `ReduceSegments`/`MergeColinearSegments`
- Kernel: Re-emits crossing chains through this carrier

[MESH]:

- Members: Topology lists, booleans, repair, reduce, `ClosestPoint`, `Volume`, and `GetNakedEdges`

[MESH_FACE]:

- Members: `A`/`B`/`C`/`D` vertex indices and `IsTriangle`/`IsQuad`
- Kernel: The predicate-exact narrow phase reads this triangle-soup index

[BREP]:

- Members: `Faces`/`Edges`
- Consumer: The `Analysis` layer intersects this parametric solid

[PUBLIC_TYPE_SCOPE]: mesh restructure and unwrap types (the `Processing/segment` host-restructure seam)

- rail: host-rhino

| [INDEX] | [SYMBOL]                 | [KIND]                   | [CAPABILITY]              |
| :-----: | :----------------------- | :----------------------- | :------------------------ |
|  [01]   | `QuadRemeshParameters`   | parameter class          | quad-remesh policy        |
|  [02]   | `QuadRemeshSymmetryAxis` | `[Flags]` enum           | symmetry axes             |
|  [03]   | `ReduceMeshParameters`   | parameter class          | decimation policy         |
|  [04]   | `MeshUnwrapper`          | reference, `IDisposable` | UV unwrap                 |
|  [05]   | `MeshUnwrapMethod`       | enum                     | parameterization selector |

[QUAD_REMESH_PARAMETERS]:

- Properties: `double TargetEdgeLength`, `int TargetQuadCount`, `double AdaptiveSize`, `bool AdaptiveQuadCount`, `bool DetectHardEdges`, `int GuideCurveInfluence`, `int PreserveMeshArrayEdgesMode`, and `QuadRemeshSymmetryAxis SymmetryAxis`
- Defaults: `TargetQuadCount` is 2000; `AdaptiveSize` uses the native `[0,100]` unit; `AdaptiveQuadCount` and `DetectHardEdges` are true
- Receipt: Every property is readable and writable, so the receipt recovers the full parameter set

[QUAD_REMESH_SYMMETRY_AXIS]:

- Values: `None = 0`, `X = 1`, `Y = 2`, and `Z = 4`
- Semantics: The axes combine under `[Flags]`

[REDUCE_MESH_PARAMETERS]:

- Properties: `int DesiredPolygonCount`, `bool AllowDistortion`, `int Accuracy`, `bool NormalizeMeshSize`, `int[] FaceTags`, `ComponentIndex[] LockedComponents`, `CancellationToken CancelToken`, and `IProgress<double> ProgressReporter`
- Failure: The host writes the internally set `string Error`

[MESH_UNWRAPPER]:

- Constructors: `MeshUnwrapper(Mesh mesh)` and `MeshUnwrapper(IEnumerable<Mesh> meshes)`
- Members: `Plane SymmetryPlane` and `bool Unwrap(MeshUnwrapMethod method)`
- Mutation: `Unwrap` writes `Mesh.TextureCoordinates` in place

[MESH_UNWRAP_METHOD]:

- Values: `LSCM`, `ABFPP`, and `ARAP`
- Route: `LSCM` is the `Processing/segment` flatten route

[PUBLIC_TYPE_SCOPE]: `Rhino.Geometry.Intersect` parametric-intersection surface

- rail: host-rhino

| [INDEX] | [SYMBOL]                   | [KIND]                     | [CAPABILITY]              |
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

[INTERSECTION]:

- Surface: `Line*`/`Curve*`/`Plane*`/`Mesh*`/`RayShoot`

[CURVE_INTERSECTIONS]:

- Sources: `CurveCurve`/`CurveLine`/`CurvePlane`/`CurveSurface`
- Members: `Count`, indexer, and `CopyTo`
- Lifetime: The hit list is disposable

[INTERSECTION_EVENT]:

- Members: `IsPoint`/`IsOverlap`, `PointA`/`PointB`, `ParameterA`/`ParameterB`, and `OverlapA`/`OverlapB`

[LINE_CIRCLE_INTERSECTION]:

- Values: `None`/`Single`/`Multiple`
- Consumer: The `Analysis` switch reads this `LineCircle` cardinality

[LINE_SPHERE_INTERSECTION]:

- Values: `None`/`Single`/`Multiple`
- Domain: `LineSphere` cardinality

[LINE_CYLINDER_INTERSECTION]:

- Values: `None`/`Single`/`Multiple`
- Domain: `LineCylinder` cardinality

[PLANE_CIRCLE_INTERSECTION]:

- Values: `None`/`Tangent`/`Secant`
- Domain: `PlaneCircle` cardinality; `PlaneSphere` and `SphereSphere` return `Circle` directly via `out`

[RTREE]:

- Tier: Host R-tree for the point-neighborhood `Spatial/neighbors` `NeighborIndex`
- Construction: `CreateFromPointArray(points)`/`CreatePointCloudTree(cloud)`/`CreateMeshFaceTree(mesh)`/`Insert(box, elementId)`
- Search: `Search(box | sphere, callback)` and static `SearchOverlaps(treeA, treeB, tolerance, callback)`
- Batch: Static `Point3dKNeighbors(hayPoints, needlePts, amount)`/`Point3dClosestPoints(hayPoints, needlePts, limitDistance)`/`PointCloudKNeighbors`/`PointCloudClosestPoints` return `IEnumerable<int[]>`
- Lifetime: The batch result is leased `as IDisposable` for the read window
- Boundary: Primitive triangle, curve, and AABB broad phase stays in the kernel's SAH-BVH and Morton octree, so neither index reimplements the other

[RTREE_EVENT_ARGS]:

- Callback: `EventHandler<RTreeEventArgs>` reads `Id`/`IdB` and sets `Cancel`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: vector, transform, quaternion, and interval operations

- rail: host-rhino

| [INDEX] | [FAMILY]             | [ROOT]       | [CAPABILITY]                    |
| :-----: | :------------------- | :----------- | :------------------------------ |
|  [01]   | vector product       | `Vector3d`   | cross and dot products          |
|  [02]   | vector unitization   | `Vector3d`   | normalized direction            |
|  [03]   | vector orientation   | `Vector3d`   | angular classification          |
|  [04]   | vector angle         | `Vector3d`   | signed or unsigned angle        |
|  [05]   | vector reframing     | `Vector3d`   | perpendicular rotation          |
|  [06]   | affine factory       | `Transform`  | named affine forms              |
|  [07]   | affine composition   | `Transform`  | transform multiplication        |
|  [08]   | affine inverse       | `Transform`  | fallible inversion              |
|  [09]   | affine decomposition | `Transform`  | transform factors               |
|  [10]   | affine invariants    | `Transform`  | classification and conditioning |
|  [11]   | rotor operations     | `Quaternion` | rotation extraction             |
|  [12]   | interval mapping     | `Interval`   | normalized remap                |

[VECTOR_PRODUCT]:

- Surface: `CrossProduct(a,b)` and `operator *(a,b)`
- Behavior: The static surfaces compute cross and dot products, and the kernel composes them through `Rasm.Numerics`

[VECTOR_UNITIZATION]:

- Surface: `Unitize()` and `IsUnitVector`
- Behavior: `Unitize()` normalizes in place and returns success as `bool`; `IsTiny` and `IsZero` guard a degenerate direction

[VECTOR_ORIENTATION]:

- Surface: `IsParallelTo` and `IsPerpendicularTo`
- Behavior: Parallelism returns `int` as -1, 0, or 1; perpendicularity returns `bool` under an angle tolerance; the predicate floor backs these orientation tests

[VECTOR_ANGLE]:

- Surface: `VectorAngle(a,b[,plane | vNormal])`
- Behavior: Returns a signed or unsigned angle that planar-overlay and frame builders read

[VECTOR_REFRAMING]:

- Surface: `PerpendicularTo` and `Rotate`
- Behavior: Builds a perpendicular vector or rotates in place about an axis

[AFFINE_FACTORY]:

- Surface: `Rotation`/`Mirror`/`PlaneToPlane`/`ChangeBasis`/`ProjectAlong`/`PlanarProjection`/`Scale`/`Translation`/`Diagonal`/`RotationZYX`
- Behavior: The static family carries one named constructor per affine kind
- Boundary: The kernel does not remint these constructors

[AFFINE_COMPOSITION]:

- Surface: `Multiply(a,b)` and `operator *`
- Behavior: Both compose transforms, and `operator *(Transform,Point3d)` applies a transform to a point

[AFFINE_INVERSE]:

- Surface: `TryGetInverse(out)`
- Behavior: Returns the inverse as `bool`, and the kernel routes the singular failure through `Fin`

[AFFINE_DECOMPOSITION]:

- Surface: `DecomposeAffine`/`DecomposeSymmetric`/`GetEulerZYZ`/`GetYawPitchRoll`/`GetQuaternion`
- Behavior: Factors a transform into translation, rotation, scale, Euler angles, or a rotor

[AFFINE_INVARIANTS]:

- Surface: `Determinant`/`IsAffine`/`IsUniformlyScaled`/`Orthogonalize`
- Behavior: Classifies transforms and reconditions a drifted basis through Gram-Schmidt re-orthogonalization

[ROTOR_OPERATIONS]:

- Surface: `GetRotation(out angle,out axis | out Plane | out Transform)`, `Conjugate`, `Inverse`, `Unitize`, and `operator *`
- Behavior: Reads the rotor as axis-angle, oriented frame, or matrix and composes rotors

[INTERVAL_MAPPING]:

- Surface: `ParameterAt`/`NormalizedParameterAt`/`IncludesParameter`
- Behavior: Owns parameter-to-normalized remapping and containment for curve-domain and sampling arithmetic

[ENTRYPOINT_SCOPE]: plane, bounds, and ray operations

- rail: host-rhino

| [INDEX] | [FAMILY]           | [ROOT]        | [CAPABILITY]           |
| :-----: | :----------------- | :------------ | :--------------------- |
|  [01]   | plane projection   | `Plane`       | coordinate remap       |
|  [02]   | plane reframing    | `Plane`       | equation and mutation  |
|  [03]   | plane construction | `Plane`       | static frame factory   |
|  [04]   | bounds composition | `BoundingBox` | AABB merge and overlap |
|  [05]   | bounds query       | `BoundingBox` | spatial sampling       |
|  [06]   | bounds mutation    | `BoundingBox` | extent conditioning    |
|  [07]   | ray sampling       | `Ray3d`       | parametric point       |

[PLANE_PROJECTION]:

- Surface: `ClosestPoint`/`RemapToPlaneSpace`/`ClosestParameter` and `ValueAt`/`DistanceTo`
- Behavior: Projects frames, remaps world and plane space, and returns the signed plane distance that the `PlaneMesh` straddle reads

[PLANE_REFRAMING]:

- Surface: `GetPlaneEquation`/`Rotate`/`Transform`/`Translate`/`Flip`
- Behavior: Returns `[a,b,c,d]`, reframes in place, and carries the `ExtendThroughBox` parameter window

[PLANE_CONSTRUCTION]:

- Surface: `CreateFromNormal`/`CreateFromPoints`/`CreateFromFrame`/`FitPlaneToPoints`
- Behavior: The static family constructs frames, and the `Spatial/cloud` `CloudKernel.BestFitPlaneOf` fold composes the `FitPlaneToPoints` least-squares fit

[BOUNDS_COMPOSITION]:

- Surface: `Union(a,b)`/`Union(box,point)`/`Intersection(a,b)`
- Behavior: Static and in-place AABB merge and overlap feed the broad phase that the kernel BVH `NodeStore` accumulates

[BOUNDS_QUERY]:

- Surface: `Contains`/`ClosestPoint`/`FurthestPoint`/`Corner`/`GetCorners`/`PointAt`
- Behavior: Owns point containment, nearest and farthest points, the eight corners, and parametric sampling

[BOUNDS_MUTATION]:

- Surface: `Inflate`/`Transform`/`MakeValid`/`IsDegenerate`/`Volume`/`Area`/`Diagonal`
- Behavior: Grows, shrinks, and transforms the box and classifies validity, degeneracy, and extent measures

[RAY_SAMPLING]:

- Surface: `PointAt(t)`/`Position`/`Direction`
- Behavior: Drives `SpatialQuery.Ray` and `MeshRay` through parametric ray sampling

[ENTRYPOINT_SCOPE]: curve and polyline operations

- rail: host-rhino

| [INDEX] | [FAMILY]             | [ROOT]     | [CAPABILITY]           |
| :-----: | :------------------- | :--------- | :--------------------- |
|  [01]   | curve evaluation     | `Curve`    | point and frame values |
|  [02]   | curve projection     | `Curve`    | nearest parameter      |
|  [03]   | curve division       | `Curve`    | arc-length sampling    |
|  [04]   | curve classification | `Curve`    | exact lowerings        |
|  [05]   | curve transformation | `Curve`    | curve mutation         |
|  [06]   | polyline evaluation  | `Polyline` | chain sampling         |
|  [07]   | polyline reduction   | `Polyline` | chain lowering         |

[CURVE_EVALUATION]:

- Surface: `PointAt(t)`/`TangentAt`/`CurvatureAt`/`FrameAt`/`PerpendicularFrameAt`
- Behavior: Evaluates the curve and the moving frame used by `Drawing` and sweep paths

[CURVE_PROJECTION]:

- Surface: `ClosestPoint(pt,out t[,maxDist])` and `LocalClosestPoint`
- Behavior: Projects to the nearest parameter under a maximum-distance gate, and `LocalClosestPoint` seeds from a guess

[CURVE_DIVISION]:

- Surface: `DivideByCount`/`DivideByLength`/`DivideEquidistant`/`GetLength`
- Behavior: Owns arc-length sampling for contour and toolpath resamplers

[CURVE_CLASSIFICATION]:

- Surface: `TryGetPolyline(out)`/`TryGetPlane(out)`/`IsClosed`/`IsPlanar`
- Behavior: Lowers exactly to `Polyline` or a supporting `Plane`; the kernel routes the non-exact `bool` branch through `Fin`

[CURVE_TRANSFORMATION]:

- Surface: `Extend`/`Simplify`/`TrimInterval`/`Offset`/`ToNurbsCurve`
- Behavior: Extends, simplifies, trims, offsets, and lowers to canonical NURBS

[POLYLINE_EVALUATION]:

- Surface: `PointAt(t)`/`ClosestPoint`/`CenterPoint`/`Length`/`GetSegments`
- Behavior: The `Meshing/intersect` `ToPolylines` projection builds this crossing-chain re-emission carrier

[POLYLINE_REDUCTION]:

- Surface: `ToNurbsCurve`/`ToPolylineCurve`/`ReduceSegments`/`MergeColinearSegments`/`Smooth`/`BreakAtAngles`/`Trim`
- Behavior: Owns chain-to-curve lowering and section-curve segment decimation

[ENTRYPOINT_SCOPE]: mesh topology access and operations

- rail: host-rhino

| [INDEX] | [FAMILY]           | [ROOT] | [CAPABILITY]           |
| :-----: | :----------------- | :----- | :--------------------- |
|  [01]   | vertex storage     | `Mesh` | mesh positions         |
|  [02]   | face storage       | `Mesh` | mesh connectivity      |
|  [03]   | normal storage     | `Mesh` | derived normals        |
|  [04]   | topology storage   | `Mesh` | welded adjacency       |
|  [05]   | n-gon storage      | `Mesh` | face grouping          |
|  [06]   | mesh repair        | `Mesh` | host-native repair     |
|  [07]   | mesh restructuring | `Mesh` | cooperative mutation   |
|  [08]   | mesh query         | `Mesh` | spatial measurements   |
|  [09]   | mesh factory       | `Mesh` | primitive tessellation |
|  [10]   | mesh booleans      | `Mesh` | host CSG               |

[VERTEX_STORAGE]:

- Surface: `Vertices` as `MeshVertexList`
- Members: `Point3f`/`Point3d` storage with `Add`/`SetVertex`/`CombineIdentical`
- Kernel: Reads positions without reminting the buffer

[FACE_STORAGE]:

- Surface: `Faces` as `MeshFaceList`
- Members: `MeshFace` connectivity with `GetFaceVertices`/`ConvertQuadsToTriangles`
- Kernel: The narrow phase indexes this triangle soup

[NORMAL_STORAGE]:

- Surface: `Normals` as `MeshVertexNormalList` and `FaceNormals` as `MeshFaceNormalList`
- Behavior: Stores per-vertex and per-face normals, and `ComputeNormals` derives them

[TOPOLOGY_STORAGE]:

- Surface: `TopologyVertices` as `MeshTopologyVertexList` and `TopologyEdges` as `MeshTopologyEdgeList`
- Members: `ConnectedTopologyVertices`/`ConnectedFaces`/`ConnectedEdges`/`MeshVertexIndices`/`IndicesFromFace`
- Kernel: Walks this welded half-edge adjacency graph

[NGON_STORAGE]:

- Surface: `Ngons` as `MeshNgonList` and `GetNgonAndFacesEnumerable`
- Behavior: Groups n-gons over triangulated faces

[MESH_REPAIR]:

- Surface: `RebuildNormals`/`UnifyNormals`/`Weld`/`Unweld`/`Compact`/`FillHoles`/`HealNakedEdges`/`MergeAllCoplanarFaces`
- Boundary: The kernel `Processing/repair` composes only `RebuildNormals` at the working-set boundary and authors welding and hole filling itself

[MESH_RESTRUCTURING]:

- Surface: `Reduce`/`Offset`/`Split`/`SelfSplit`/`CollapseFacesByArea`/`CollapseFacesByEdgeLength`
- Behavior: Owns decimation, offsetting, and splitting
- Cooperation: `Reduce`/`Split`/`SelfSplit` overloads take `CancellationToken` and `IProgress<double>` for long-running host calls

[MESH_QUERY]:

- Surface: `ClosestPoint(pt[,out pt,out normal,maxDist])`/`ClosestMeshPoint`/`Volume`/`GetNakedEdges`/`GetSelfIntersections`
- Behavior: Returns the nearest point with optional normal, enclosed volume, naked-edge polylines, and self-intersection probes

[MESH_FACTORY]:

- Surface: `CreateFromBox`/`CreateFromSphere`/`CreateFromCone`/`CreateFromClosedPolyline`/`CreateFromTessellation`
- Consumer: Meshing pages seed from this primitive-to-mesh family

[MESH_BOOLEANS]:

- Surface: `CreateBooleanUnion`/`CreateBooleanDifference`/`CreateBooleanIntersection`
- Return: `Mesh[]` with an `out Result`
- Boundary: The kernel's predicate-exact arrangement does not use this host parametric CSG path

[ENTRYPOINT_SCOPE]: mesh restructure operations (the `Processing/segment` `RemeshKind`/flatten host seam)

- rail: host-rhino

Every remesh/reduce entry is a native long-running call; the instance forms return a NEW `Mesh` (null on failure — the kernel `Fin`-routes the null and disposes the orphan), the `Reduce` forms mutate in place returning success `bool`, and the cooperative overloads thread `IProgress<int>`+`CancellationToken`.

| [INDEX] | [FAMILY]       | [ROOT]          | [CAPABILITY]          |
| :-----: | :------------- | :-------------- | :-------------------- |
|  [01]   | quad remesh    | `Mesh`          | whole-mesh remeshing  |
|  [02]   | block remesh   | `Mesh`          | scoped remeshing      |
|  [03]   | async remesh   | `Mesh`          | cooperative remeshing |
|  [04]   | Brep remesh    | `Mesh`          | direct tessellation   |
|  [05]   | mesh reduction | `Mesh`          | in-place decimation   |
|  [06]   | mesh unwrap    | `MeshUnwrapper` | UV parameterization   |

[QUAD_REMESH]:

- Surface: `QuadRemesh(QuadRemeshParameters parameters)`
- Overload: `(parameters, IEnumerable<Curve> guideCurves)` adds guides to whole-mesh quad remeshing

[BLOCK_REMESH]:

- Surface: `QuadRemesh(IEnumerable<int> faceBlocks, QuadRemeshParameters parameters, IEnumerable<Curve> guideCurves, IProgress<int> progress, CancellationToken cancelToken)`
- Route: This cooperative face-block overload is the exact `Processing/segment` quad arm

[ASYNC_REMESH]:

- Surface: `QuadRemeshAsync(parameters, progress, cancelToken)`, `(parameters, guideCurves, progress, cancelToken)`, and `(faceBlocks, parameters, guideCurves, progress, cancelToken)`
- Return: The cooperative async family returns `Task<Mesh>` and mirrors the synchronous family

[BREP_REMESH]:

- Surface: `Mesh.QuadRemeshBrep(Brep brep, QuadRemeshParameters parameters)`
- Overloads: `(…, guideCurves)` and `(…, guideCurves, progress, cancelToken)` widen direct Brep remeshing, and the `QuadRemeshBrepAsync` pair mirrors them asynchronously

[MESH_REDUCTION]:

- Surface: `Reduce(ReduceMeshParameters parameters)` and `Reduce(parameters, bool threaded)`
- Return: In-place decimation returns success as `bool` and writes failure to `parameters.Error`
- Scalar form: `Reduce(desiredPolygonCount, allowDistortion, accuracy, normalizeSize[, threaded | cancelToken, progress])` carries four scalar overloads replaced by the parameter carrier

[MESH_UNWRAP]:

- Surface: `new MeshUnwrapper(mesh)` followed by `Unwrap(MeshUnwrapMethod.LSCM)`
- Behavior: `LSCM`/`ABFPP`/`ARAP` parameterization writes `Mesh.TextureCoordinates`, returns success as `bool`, and checks coordinates against `Vertices.Count`
- Route: The `Processing/segment` flatten route disposes the unwrapper under `using`

[ENTRYPOINT_SCOPE]: `Intersection` parametric-intersection operations (the `Analysis` layer's surface)

- rail: host-rhino

| [INDEX] | [FAMILY]            | [ROOT]         | [CAPABILITY]       |
| :-----: | :------------------ | :------------- | :----------------- |
|  [01]   | line crossing       | `Intersection` | line parameters    |
|  [02]   | line projection     | `Intersection` | plane and box hits |
|  [03]   | line primitives     | `Intersection` | cardinality hits   |
|  [04]   | plane primitives    | `Intersection` | analytic crossings |
|  [05]   | curve intersection  | `Intersection` | parametric hits    |
|  [06]   | mesh intersection   | `Intersection` | host mesh probes   |
|  [07]   | reflective ray cast | `Intersection` | reflection events  |
|  [08]   | mesh projection     | `Intersection` | directional drape  |

[LINE_CROSSING]:

- Surface: `LineLine(a,b,out ta,out tb[,tol,finiteSegments])`
- Return: `bool` crossing with both line parameters
- Route: The `Analysis/Intersect` lattice owns the `Line·Line` case

[LINE_PROJECTION]:

- Surface: `LinePlane(line,plane,out t)` and `LineBox(line,box,tol,out Interval)`
- Return: `bool` with a line-plane parameter or line-box parameter `Interval`
- Route: The `Analysis/Intersect` lattice owns the `Line·Plane` and `Line·Box` cases

[LINE_PRIMITIVES]:

- Surface: `LineCircle`/`LineSphere`/`LineCylinder`
- Return: Cardinality enum and two `out Point3d` values
- Consumer: The `Analysis` switch reads `Single` and `Multiple`

[PLANE_PRIMITIVES]:

- Surface: `PlanePlane(out Line)`/`PlanePlanePlane(out Point3d)`/`PlaneCircle`/`PlaneSphere(out Circle)`/`SphereSphere(out Circle)`
- Consumer: The `Analysis` parametric lattice folds these analytic primitive-pair crossings

[CURVE_INTERSECTION]:

- Surface: `CurveCurve(a,b,tol,overlapTol)`/`CurveLine`/`CurvePlane`/`CurveSelf`/`CurveSurface`/`CurveBrep`
- Return: Disposable `CurveIntersections` as `IList<IntersectionEvent>` under a lease
- Boundary: The host owns this NURBS and Brep parametric path, and the kernel does not reauthor it

[MESH_INTERSECTION]:

- Surface: `MeshRay(mesh,ray[,out faceIds])`/`MeshLine`/`MeshLineSorted`/`MeshPolyline`
- Return: Ray parameter or `Point3d[]` with face identifiers
- Boundary: The kernel owns the discrete `SpatialQuery.Ray` and predicate-exact narrow phase

[REFLECTIVE_RAY_CAST]:

- Surface: `RayShoot(ray,IEnumerable<GeometryBase>,maxReflections)` and `RayShoot(geometry,ray,maxReflections)`
- Return: `RayShootEvent[]`
- Route: Reflective ray casting against parametric geometry is the `Analysis` ray-trace surface

[MESH_PROJECTION]:

- Surface: `ProjectPointsToMeshes` and `ProjectPointsToMeshesEx(out indices)`
- Boundary: The kernel BVH does not replace host directional point-to-mesh draping

## [04]-[IMPLEMENTATION_LAW]

[GEOMETRY_VALUE_LAW]:

- Package: `RhinoCommon` (`Rhino.Geometry`)
- Owns: the value-type geometry vocabulary the kernel reads, composes through `Rasm.Numerics`, and re-emits at the seam (`Point3d`/`Vector3d`/`Plane`/`Line`/`Polyline`/`Mesh`/`MeshFace`/`BoundingBox`/`Ray3d`/`Transform`/`Interval`)
- Accept: `Rhino.Geometry` value types composed through `Rasm.Numerics`; the kernel reads a struct's full member surface (`Vector3d.IsParallelTo`, `Transform.DecomposeAffine`, `BoundingBox.Union`, `Mesh.TopologyVertices`) rather than re-deriving the operation
- Reject: a kernel-local re-mint of a Rhino value type (a domain `Aabb`/`Ray`/`Vec3` duplicating `BoundingBox`/`Ray3d`/`Vector3d`); an epsilon-snapped coordinate where the robust-core owns an exact construction; a thin pass-through wrapper that renames a struct member without adding domain value

[PREDICATE_EXACT_BOUNDARY]:

- Package: `RhinoCommon` (`Rhino.Geometry.Intersect`, `Rhino.Geometry.RTree`)
- Owns: the host parametric curve/surface/solid intersection (`Intersection.Curve*`/`Brep*`/`Ray*`) — the `Analysis` layer's parametric concern — and the host R-tree POINT-NEIGHBORHOOD tier the `Spatial/neighbors` `NeighborIndex` composes
- Accept: the `Analysis/Intersect.cs` parametric lattice composing `Intersection.LineLine`/`LinePlane`/`PlanePlane`/`LineCircle`/`LineSphere`/`LineBox`/`CurveLine`/`CurveCurve`/`CurvePlane` and disposing each `CurveIntersections` under a lease; the `Spatial/neighbors` `NeighborIndex` running `RTree` construction, callback searches, and the hay×needles batches inside its callback capsule; the host owns parametric, the kernel owns discrete
- Reject: a kernel discrete mesh-mesh / plane-mesh / segment-triangle crossing routed through host `Intersection.MeshMesh*` or a kernel PRIMITIVE (triangle/curve/AABB) broad-phase routed through `RTree` — the `Meshing/intersect` owner authors the Guigue-Devillers predicate-exact narrow-phase over `Mesh`/`MeshFace`/`Plane`/`Ray3d` and the `Spatial/index` owner authors the SAH-BVH / Morton-octree over a flat `NodeStore`, and a host parametric crossing where the predicate-exact straddle is required is the named non-robustness defect

[BOUNDARY_LAW]:

- Package: `RhinoCommon` (`Rhino.Geometry`)
- Owns: the geometry-only Rhino surface below the document, view, command, and display strata
- Accept: geometry values cross the seam as `Rasm.Numerics` carriers, the robust-core re-emitting `Polyline`/`Point3d`/`Mesh` results at the boundary; reference geometry (`Mesh`/`Curve`/`Brep`) is the kernel's disposable native-handle owner, released at the seam
- Reject: a `RhinoDoc`/`RhinoApp`/`RhinoView`/`DisplayConduit`/`ObjectTable` reach from the kernel — the document/view/command/display surface is the host-boundary stratum's concern, never the kernel's
