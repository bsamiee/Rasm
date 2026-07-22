# [RASM_RHINO_API_RHINOCOMMON_MESHING]

`RhinoCommon` owns the host-fidelity mesh and SubD construction boundary: the `MeshingParameters`-driven mesher, quad-remesh and shrink-wrap generation, mesh booleans, the reduce/weld/offset/heal/smooth/split edit family, the `MeshExtruder`, and the `SubD` object model. Every member P/Invokes `rhcommon_c` and returns geometry bit-compatible with Rhino's own commands, standing above the host-neutral `Rasm` remesh, decimate, and subdivision kernel it never re-derives and projecting native outcomes onto the `LanguageExt` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespace: `Rhino.Geometry`, `Rhino.Geometry.Collections`
- rail: mesh-construction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: construction owners

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :------------- | :-------------- | :------------------------------------------------------------------------ |
|  [01]   | `Mesh`         | native geometry | from-surface/primitive mesh, quad-remesh, shrink-wrap, booleans, edit set |
|  [02]   | `MeshExtruder` | disposable      | face-set extrusion with wall-face capture                                 |
|  [03]   | `SubD`         | native geometry | create from mesh/surface/loft/sweep/primitive, subdivide, offset, to-brep |

[PUBLIC_TYPE_SCOPE]: configuration carriers

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                                      |
| :-----: | :------------------------ | :------------- | :---------------------------------------------------------------- |
|  [01]   | `MeshingParameters`       | config carrier | density, edge-length, grid, refine, and tolerance mesh policy     |
|  [02]   | `QuadRemeshParameters`    | config carrier | target quad count, adaptive size, hard edges, symmetry axis       |
|  [03]   | `ReduceMeshParameters`    | config carrier | polygon target, accuracy, distortion, locked components, progress |
|  [04]   | `ShrinkWrapParameters`    | config carrier | target edge length, offset, hole fill, smoothing, optimization    |
|  [05]   | `MeshBooleanOptions`      | config carrier | tolerance, text log, progress, cancellation for mesh booleans     |
|  [06]   | `MeshCheckParameters`     | config carrier | validity-check thresholds for `Mesh.Check`                        |
|  [07]   | `SubDCreationOptions`     | config carrier | corner, crease, texture, and interpolation policy for from-mesh   |
|  [08]   | `SubDToBrepOptions`       | config carrier | face packing and extraordinary-vertex processing for to-brep      |
|  [09]   | `SubDSurfaceInterpolator` | solver         | fixed/interpolated vertex solve for surface-point interpolation   |

[PUBLIC_TYPE_SCOPE]: topology-read collections

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `MeshVertexList` / `MeshFaceList` / `MeshNgonList`  | face lists    | vertex, quad/tri face, and ngon geometry and edits |
|  [02]   | `MeshTopologyVertexList` / `MeshTopologyEdgeList`   | topology      | welded-vertex and edge connectivity reads          |
|  [03]   | `MeshVertexNormalList` / `MeshFaceNormalList`       | normals       | per-vertex and per-face normal vectors             |
|  [04]   | `MeshVertexColorList` / `MeshTextureCoordinateList` | attributes    | per-vertex color and texture-uv coordinates        |
|  [05]   | `SubDVertexList` / `SubDEdgeList` / `SubDFaceList`  | subd lists    | subd component enumeration and tag reads           |

[ENUM_ROSTERS]:
- `Rhino.Geometry.MeshPipeCapStyle` — `None` `Flat` `Box` `Dome`.
- `[Flags] Rhino.Geometry.QuadRemeshSymmetryAxis` — `None=0` `X=1` `Y=2` `Z=4`.
- `Rhino.Geometry.SubDFromSurfaceMethods : byte` — `Unset` `SubDFriendlyFit` `FromNurbsControlNet`.
- `Rhino.Geometry.SubDEndCapStyle` — `Unset` `None` `Triangles` `Quads` `Ngon`.
- `Rhino.Geometry.SubDEdgeTag` — `Unset=0` `Smooth=1` `Crease=2` `SmoothX=4`.
- `Rhino.Geometry.SubDComponentLocation : byte` — `Unset` `ControlNet` `Surface`.
- `Rhino.Geometry.SubDVertexTag : byte` — `Unset` `Smooth` `Crease` `Corner` `Dart`.
- `SubDDisplayParameters.Density : uint` — `UnsetDensity=0` `ExtraCoarseDensity=1` `CoarseDensity=2` `MediumDensity=3` `FineDensity=4` (=`DefaultDensity`) `ExtraFineDensity=5` `MaximumDensity=6`.
- `MeshExtruderParameterMode` — `CoverWalls` `KeepAndStretch`; `MeshExtruderFaceDirectionMode` — `Keep` `OrientClosedFrontOut`.
- `MeshRefinements.LoopFormula` — `Loop` `Warren` `WarrenWeimer`; `MeshRefinements.CreaseEdges` — `NakedFixed` `NakedSmooth` `CornerFixedOtherCreased` `Auto`.
- `SubDCreationOptions.InteriorCreaseOption` — `Unset` `None` `AtMeshDoubleEdge`; `ConvexCornerOption` / `ConcaveCornerOption` — `Unset` `None` `AtMeshCorner`.
- `SubDCreationOptions.TextureCoordinateOption` — `Unset` `None` `Automatic` `Packed` `CopyMapping` `CopyCoordinates`.
- `SubDToBrepOptions.ExtraordinaryVertexProcessOption` — `None` `LocalG1` `LocalG2` `LocalG1x` `LocalG1xx`.

## [03]-[ENTRYPOINTS]

[MESH_FROM_SURFACE]: `Mesh` static mesher over breps, surfaces, subds.

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `CreateFromBrep(Brep, MeshingParameters) -> Mesh[]`                  | static  | mesh per brep face under a policy |
|  [02]   | `CreateFromSurface(Surface, MeshingParameters) -> Mesh`              | static  | single-surface mesh               |
|  [03]   | `CreateFromExtrusion(Extrusion, MeshingParameters) -> Mesh`          | static  | extrusion mesh                    |
|  [04]   | `CreateFromSubD(SubD, int) -> Mesh`                                  | static  | subd mesh at a display density    |
|  [05]   | `CreateFromSubDControlNet(SubD) -> Mesh`                             | static  | subd control-net cage mesh        |
|  [06]   | `CreateFromSubDControlNetWithTextureCoordinates(SubD) -> Mesh`       | static  | control-net cage with texture uv  |
|  [07]   | `CreateFromSurfaceControlNet(Surface) -> Mesh`                       | static  | surface control-net cage mesh     |
|  [08]   | `CreateFromPlanarBoundary(Curve, MeshingParameters, double) -> Mesh` | static  | planar region in a boundary       |

- `CreateFromBrep` returns null on failure; the array carries one mesh per face.
- `CreateFromSubD(SubD, SubDDisplayParameters.Density)` names the density level by enum.

[MESH_FROM_PRIMITIVES]: `Mesh` static tessellation of analytic solids.

| [INDEX] | [SURFACE]                                                                        | [SHAPE] | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `CreateFromBox(Box, int, int, int) -> Mesh`                                      | static  | faceted box                      |
|  [02]   | `CreateFromSphere(Sphere, int, int) -> Mesh`                                     | static  | uv sphere tessellation           |
|  [03]   | `CreateIcoSphere(Sphere, int) -> Mesh` / `CreateQuadSphere(Sphere, int) -> Mesh` | static  | icosahedral, quad sphere         |
|  [04]   | `CreateFromCylinder(Cylinder, int, int, bool, bool, bool, bool) -> Mesh`         | static  | capped cylinder                  |
|  [05]   | `CreateFromCone(Cone, int, int, bool, bool) -> Mesh`                             | static  | capped cone                      |
|  [06]   | `CreateFromTorus(Torus, int, int) -> Mesh`                                       | static  | torus                            |
|  [07]   | `CreateFromPlane(Plane, Interval, Interval, int, int) -> Mesh`                   | static  | gridded plane                    |
|  [08]   | `CreateFromClosedPolyline(Polyline) -> Mesh`                                     | static  | fan-triangulated closed polyline |

- `CreateFromBox` also takes `BoundingBox` and eight-corner overloads.

[MESH_GENERATE]: `Mesh` static generative construction; rows return `Mesh` unless noted.

| [INDEX] | [SURFACE]                                                                                           | [SHAPE] | [CAPABILITY]           |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------ | :--------------------- |
|  [01]   | `QuadRemeshBrep(Brep, QuadRemeshParameters, IEnumerable<Curve>, IProgress<int>, CancellationToken)` | static  | quad-remesh a brep     |
|  [02]   | `ShrinkWrap(IEnumerable<GeometryBase>, ShrinkWrapParameters, MeshingParameters, CancellationToken)` | static  | wrap mixed geometry    |
|  [03]   | `CreateFromCurvePipe(Curve, double, int, int, MeshPipeCapStyle, bool, IEnumerable<Interval>)`       | static  | pipe mesh from a curve |
|  [04]   | `CreateFromCurveExtrusion(Curve, Vector3d, MeshingParameters, BoundingBox)`                         | static  | curve extrusion mesh   |
|  [05]   | `CreateExtrusion(Curve, Vector3d, MeshingParameters)`                                               | static  | profile extrusion mesh |
|  [06]   | `CreateFromIsosurface(Func<Point3d, double>, BoundingBox, int, int)`                                | static  | marching-cubes iso     |
|  [07]   | `CreateFromLines(Curve[], int, double)`                                                             | static  | edge-line network mesh |
|  [08]   | `CreateFromTessellation(IEnumerable<Point3d>, IEnumerable<IEnumerable<Point3d>>, Plane, bool)`      | static  | constrained mesh       |
|  [09]   | `CreateConvexHull3D(IEnumerable<Point3d>, out int[][], double, double)`                             | static  | convex hull + map      |
|  [10]   | `CreatePatch(Polyline, double, Surface, IEnumerable<Curve>×2, IEnumerable<Point3d>, bool, int)`     | static  | mixed-constraint patch |
|  [11]   | `CreateFromIterativeCleanup(IEnumerable<Mesh>, double) -> Mesh[]`                                   | static  | cleanup rebuild        |
|  [12]   | `RebuildMesh(Mesh, bool, bool)`                                                                     | static  | rebuild keep uv/colors |
|  [13]   | `CreateRefinedLoopMesh(Mesh, LoopFormula, RefinementSettings)`                                      | static  | Loop refinement        |
|  [14]   | `CreateRefinedCatmullClarkMesh(Mesh, RefinementSettings)`                                           | static  | Catmull-Clark refine   |

- `QuadRemeshBrepAsync(...)` returns `Task<Mesh>`.
- `ShrinkWrap` also wraps `IEnumerable<Mesh>` and `PointCloud` sources directly.
- `RequireIterativeCleanup(IEnumerable<Mesh>, double) -> bool` probes whether cleanup is needed.
- `MeshRefinements.RefinementSettings` carries `Level : int`, `NakedEdgeMode : CreaseEdges`, `ContinueRequest : CancellationToken`.

[MESH_BOOLEANS]: `Mesh` static booleans, each `-> Mesh[]` with `out Result, out int[][]` diagnostics.

| [INDEX] | [SURFACE]                                                                             | [SHAPE] | [CAPABILITY]      |
| :-----: | :------------------------------------------------------------------------------------ | :------ | :---------------- |
|  [01]   | `CreateBooleanUnion(IEnumerable<Mesh>, MeshBooleanOptions)`                           | static  | union + input map |
|  [02]   | `CreateBooleanDifference(IEnumerable<Mesh>, IEnumerable<Mesh>, MeshBooleanOptions)`   | static  | difference        |
|  [03]   | `CreateBooleanIntersection(IEnumerable<Mesh>, IEnumerable<Mesh>, MeshBooleanOptions)` | static  | intersection      |
|  [04]   | `CreateBooleanSplit(IEnumerable<Mesh>, IEnumerable<Mesh>, MeshBooleanOptions)`        | static  | split             |

- Each arm's `Result` reports the outcome and `int[][]` maps results to inputs; the `(meshes)` and `(meshes, tolerance)` overloads drop the diagnostics.

[MESH_SPLIT_PARTITION]: `Mesh` static split and measure over mesh sets.

| [INDEX] | [SURFACE]                                                                                              | [SHAPE] | [CAPABILITY]          |
| :-----: | :----------------------------------------------------------------------------------------------------- | :------ | :-------------------- |
|  [01]   | `SplitMesh(Mesh, int, bool, bool) -> Mesh[]`                                                           | static  | count-bounded split   |
|  [02]   | `PartitionMesh(Mesh, int, int) -> Mesh[]`                                                              | static  | vertex/face partition |
|  [03]   | `MatchEdges(IEnumerable<Mesh>, double, bool×4) -> Mesh[]`                                              | static  | align naked edges     |
|  [04]   | `ComputeThickness(IEnumerable<Mesh>, double, double, CancellationToken) -> MeshThicknessMeasurement[]` | static  | per-face thickness    |

- `ComputeThickness(meshes, maximumThickness)` and its `cancelToken` overload drop the sharp-angle bound.

[MESH_EDIT]: `Mesh` instance in-place edit.

| [INDEX] | [SURFACE]                                                                                           | [SHAPE]  | [CAPABILITY]            |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `Reduce(ReduceMeshParameters, bool) -> bool`                                                        | instance | decimate in place       |
|  [02]   | `QuadRemesh(QuadRemeshParameters, IEnumerable<Curve>) -> Mesh`                                      | instance | instance quad-remesh    |
|  [03]   | `QuadRemeshAsync(QuadRemeshParameters, IProgress<int>, CancellationToken) -> Task<Mesh>`            | instance | async quad-remesh       |
|  [04]   | `ShrinkWrap(ShrinkWrapParameters, CancellationToken) -> Mesh`                                       | instance | instance shrink-wrap    |
|  [05]   | `Offset(double, bool, Vector3d, out List<int>) -> Mesh`                                             | instance | offset/solidify         |
|  [06]   | `Weld(double) -> void` / `Weld(double, bool) -> void`                                               | instance | weld by angle           |
|  [07]   | `Unweld(double, bool) -> void`                                                                      | instance | unweld all by angle     |
|  [08]   | `UnweldEdge(IEnumerable<int>, bool) -> bool` / `UnweldVertices(IEnumerable<int>, bool) -> bool`     | instance | per-edge/vertex unweld  |
|  [09]   | `Smooth(IEnumerable<int>, double, int, bool×4, SmoothingCoordinateSystem, Plane) -> bool`           | instance | scoped smoothing        |
|  [10]   | `Subdivide(IEnumerable<int>) -> bool`                                                               | instance | mid-edge subdivision    |
|  [11]   | `FillHoles() -> bool` / `FillHole(int) -> bool`                                                     | instance | fill hole(s)            |
|  [12]   | `HealNakedEdges(double) -> bool` / `ExtractNonManifoldEdges(bool) -> Mesh`                          | instance | heal, non-manifold pull |
|  [13]   | `Split(IEnumerable<Mesh>, double, bool×2, TextLog, CancellationToken, IProgress<double>) -> Mesh[]` | instance | split by mesh/plane     |
|  [14]   | `SplitDisjointPieces() -> Mesh[]` / `SplitNon2Manifolds() -> Mesh[]`                                | instance | separate by criteria    |
|  [15]   | `SplitWithProjectedPolylines(IEnumerable<PolylineCurve>, double) -> Mesh[]`                         | instance | split by polylines      |
|  [16]   | `MergeAllCoplanarFaces(double, double) -> bool`                                                     | instance | merge coplanar faces    |
|  [17]   | `MatchEdges(double, bool) -> bool`                                                                  | instance | match naked edges       |
|  [18]   | `Append(IEnumerable<Mesh>) -> void` / `Compact() -> bool`                                           | instance | aggregate, compact      |
|  [19]   | `Flip(bool×4) -> void` / `RebuildNormals() -> void` / `UnifyNormals() -> int`                       | instance | orient, rebuild         |
|  [20]   | `GetNakedEdges() -> Polyline[]` / `GetOutlines(Plane) -> Polyline[]`                                | instance | boundary/silhouette     |

- `CollapseFacesByEdgeLength(bool, double)`, `CollapseFacesByArea(double, double)`, and `CollapseFacesByByAspectRatio(double)` each return `int`, collapsing faces by edge length, area, and aspect ratio; the doubled `By` is the host spelling.
- `Reduce(int, bool, int, bool, CancellationToken, IProgress<double>, out string)` overload exposes cancellation and a diagnostic string.
- `QuadRemesh` also takes a face-block overload that scopes the remesh.
- `Offset` reduced overloads drop the direction and wall-face output; `Smooth` whole-mesh overloads drop the vertex set; `Subdivide` with no face set subdivides the whole mesh.
- `MeshVertexList.GetTopologicalIndenticalVertices` carries the host misspelling.
- `GetOutlines(RhinoViewport)` and `(ViewportInfo, Plane)` overloads project to view.

[MESH_FINISHING]: `Mesh` value-semantic finishing and face-set derivation, each returning a new mesh.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]         | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------ | :-------------- | :------------------------------ |
|  [01]   | `WithEdgeSoftening(double, bool, bool, bool, double) -> Mesh`             | instance        | edge-softened copy              |
|  [02]   | `WithShutLining(bool, double, IEnumerable<ShutLiningCurveInfo>) -> Mesh`  | instance        | shut-lined copy                 |
|  [03]   | `WithDisplacement(MeshDisplacementInfo) -> Mesh`                          | instance        | displaced copy                  |
|  [04]   | `CreateFromPatchSingleFace(Mesh, IEnumerable<ComponentIndex>) -> Mesh`    | static          | single-face patch               |
|  [05]   | `CreateFromFilteredFaceList(Mesh, IEnumerable<bool>) -> Mesh`             | static          | face-filtered copy              |
|  [06]   | `CreateUnweldedMesh(Mesh) -> Mesh` / `ExplodeAtUnweldedEdges() -> Mesh[]` | static/instance | unwelded copy, explode at seams |
|  [07]   | `MeshFaceList.ExtractFaces(IEnumerable<int>) -> Mesh`                     | instance        | extract a face subset           |

- `ShutLiningCurveInfo(Curve, double, int, bool, bool, IEnumerable<Interval>, bool)` carries read-only `Curve` `Enabled` `Radius` `Profile` `Pull` `IsBump` `CurveIntervals`, consumed per-profile by `WithShutLining`.
- `MeshDisplacementInfo(RenderTexture, TextureMapping)` carries read-only `Texture` `Mapping` and get/set `Black` `White` `BlackMove` `WhiteMove` `MappingTransform` `InstanceTransform` `PostWeldAngle` `RefineSensitivity` `SweepPitch` `ChannelNumber` `FaceLimit` `FairingAmount` `RefineStepCount` `MemoryLimit`.

[MESH_EXTRUDER]: `MeshExtruder` face-set extrusion.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `MeshExtruder(Mesh, IEnumerable<ComponentIndex>)`          | ctor     | extruder over a face set        |
|  [02]   | `ExtrudedMesh(out Mesh, out List<ComponentIndex>) -> bool` | instance | extruded result + component map |
|  [03]   | `GetWallFaces() -> List<int>`                              | instance | created wall-face list          |

[EXTRUDER_KNOBS]: `Transform` `UVN` `EdgeBasedUVN` `KeepOriginalFaces` `TextureCoordinateMode` `SurfaceParameterMode` `FaceDirectionMode` `PreviewLines`

[MESH_TOPOLOGY]: `Mesh` in-place geometry and connectivity lists.

| [INDEX] | [SURFACE]                                                                                          | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `Vertices` / `Faces` / `Ngons` / `Normals` / `FaceNormals` / `VertexColors` / `TextureCoordinates` | property | in-place geometry lists |
|  [02]   | `TopologyVertices` / `TopologyEdges`                                                               | property | welded connectivity     |

[MESH_CONFIG]: `MeshingParameters` carriers feeding the mesh ops.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]         | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------- | :-------------- | :----------------------------- |
|  [01]   | `MeshingParameters.DocumentCurrentSetting(RhinoDoc) -> MeshingParameters`        | static          | document's current mesh policy |
|  [02]   | `ToEncodedString() -> string` / `FromEncodedString(string) -> MeshingParameters` | instance/static | round-trip a policy string     |
|  [03]   | `MeshingParameters(double)` / `MeshingParameters(double, double)`                | ctor            | density, min-edge policy       |

[MESH_PRESETS]: `Default` `FastRenderMesh` `QualityRenderMesh` `DefaultAnalysisMesh` `Minimal`
[MESH_PARAM_KNOBS]: `TextureRange` `JaggedSeams` `RefineGrid` `DoublePrecision` `SimplePlanes` `ComputeCurvature` `ClosedObjectPostProcess` `GridMinCount` `GridMaxCount` `GridAngle` `GridAspectRatio` `GridAmplification` `Tolerance` `MinimumTolerance` `RelativeTolerance` `MinimumEdgeLength` `MaximumEdgeLength` `RefineAngle` `RefineAngleInDegrees`
[QUAD_REMESH_KNOBS]: `TargetEdgeLength` `TargetQuadCount` `AdaptiveSize` `AdaptiveQuadCount` `DetectHardEdges` `GuideCurveInfluence` `PreserveMeshArrayEdgesMode` `SymmetryAxis`
[SHRINK_WRAP_KNOBS]: `TargetEdgeLength` `Offset` `SmoothingIterations` `FillHolesInInputObjects` `PolygonOptimization` `InflateVerticesAndPoints` `PreserveColors`
[REDUCE_KNOBS]: `DesiredPolygonCount` `AllowDistortion` `Accuracy` `NormalizeMeshSize` `FaceTags` `LockedComponents` `CancelToken` `ProgressReporter` `Error`
[BOOLEAN_KNOBS]: `Tolerance` `TextLog` `CancellationToken` `ProgressReporter`
- `QuadRemeshParameters`: `TargetEdgeLength` nonzero overrides the count, `AdaptiveSize` ranges 0-100, `GuideCurveInfluence` is 0 approximate / 1 interp-edge-ring / 2 interp-edge-loop, `PreserveMeshArrayEdgesMode` is 0 off / 1 smart / 2 strict, and progress is `IProgress<int>`, never `IProgress<double>`.
- `ShrinkWrapParameters` builds through the static `Mesh.ShrinkWrap`; no `CreateShrinkWrap` spelling exists.
- `ReduceMeshParameters`: `Error : string` is the host-written diagnostic.

[SUBD_BUILD]: `SubD` static construction; rows return `SubD` unless noted.

| [INDEX] | [SURFACE]                                                                                       | [SHAPE] | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------------------------------------------- | :------ | :-------------------- |
|  [01]   | `CreateFromMesh(Mesh, SubDCreationOptions)`                                                     | static  | mesh to subd          |
|  [02]   | `CreateFromSurface(Surface, SubDFromSurfaceMethods, bool)`                                      | static  | subd from a surface   |
|  [03]   | `CreateFromLoft(IEnumerable<NurbsCurve>, bool×3, int)`                                          | static  | loft through curves   |
|  [04]   | `CreateFromSweep(NurbsCurve, IEnumerable<NurbsCurve>, bool×3, Vector3d)`                        | static  | one-rail sweep        |
|  [05]   | `CreateFromCylinder(Cylinder, uint, uint, SubDEndCapStyle, SubDEdgeTag, SubDComponentLocation)` | static  | faceted cylinder      |
|  [06]   | `CreateQuadSphere(Sphere, SubDComponentLocation, uint)`                                         | static  | quad sphere           |
|  [07]   | `CreateTriSphere(Sphere, SubDComponentLocation, uint)`                                          | static  | tri sphere            |
|  [08]   | `CreateGlobeSphere(Sphere, SubDComponentLocation, uint, uint)`                                  | static  | globe sphere          |
|  [09]   | `CreateIcosahedron(Sphere, SubDComponentLocation)`                                              | static  | icosahedron sphere    |
|  [10]   | `JoinSubDs(IEnumerable<SubD>, double, bool, bool) -> SubD[]`                                    | static  | join coincident edges |

- `CreateFromMesh(Mesh)` overload uses defaults.
- `CreateFromSweep(rail1, rail2, shapes, closed, addCorners)` overload is the two-rail sweep.

[SUBD_EDIT]: `SubD` instance edit and conversion.

| [INDEX] | [SURFACE]                                                                                    | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `Subdivide(int) -> bool`                                                                     | instance | global subdivision by level   |
|  [02]   | `Offset(double, bool) -> SubD`                                                               | instance | offset, optionally to a shell |
|  [03]   | `ToBrep(SubDToBrepOptions) -> Brep`                                                          | instance | convert to a brep             |
|  [04]   | `InterpolateSurfacePoints(Point3d[]) -> bool`                                                | instance | fit control points to targets |
|  [05]   | `MergeAllCoplanarFaces(double, double) -> bool` / `PackFaces() -> uint` / `Flip() -> bool`   | instance | merge, pack, orient           |
|  [06]   | `TransformComponents(IEnumerable<ComponentIndex>, Transform, SubDComponentLocation) -> uint` | instance | transform a component subset  |
|  [07]   | `SetVertexSurfacePoint(uint, Point3d) -> bool`                                               | instance | set one surface-point target  |
|  [08]   | `UpdateAllTagsAndSectorCoefficients() -> uint` / `UpdateSurfaceMeshCache(bool) -> uint`      | instance | refresh tags, surface cache   |

- `Subdivide(IEnumerable<int>)` overload scopes it; no `GlobalSubdivide` and no subd quad-remesh exist — route `Mesh.QuadRemesh` then `SubD.CreateFromMesh`.
- `Offset` closes the shell when solidify is true.
- `ToBrep()` overload uses defaults.
- `InterpolateSurfacePoints(uint[], Point3d[])` overload constrains a vertex subset.

[SUBD_TOPOLOGY]: `SubD` component reads and tag authoring.

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------- | :------- | :------------------------------- |
|  [01]   | `Vertices` / `Edges` / `Faces`           | property | component enumeration, tag reads |
|  [02]   | `DuplicateEdgeCurves(bool×6) -> Curve[]` | instance | filtered edge-curve extraction   |

- `SubDVertexList.SetVertexTags(IEnumerable<int>, SubDVertexTag)` and `SubDEdgeList.SetEdgeTags(IEnumerable<int>, SubDEdgeTag)` author vertex and edge tags.

[SUBD_CONFIG]: `SubD` creation, brep, and display carriers; the display factories are on `SubDDisplayParameters`.

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]            |
| :-----: | :-------------------------------------------------------------------------- | :------ | :---------------------- |
|  [01]   | `SubDToBrepOptions(bool, ExtraordinaryVertexProcessOption)`                 | ctor    | to-brep packing options |
|  [02]   | `CreateFromDisplayDensity(uint)` / `CreateFromAbsoluteDisplayDensity(uint)` | static  | display-density carrier |
|  [03]   | `CreateFromMeshDensity(double)`                                             | static  | mesh-density carrier    |

[SUBD_CREATION_PRESETS]: `Smooth` `InteriorCreases` `ConvexCornersAndInteriorCreases` `ConvexAndConcaveCornersAndInteriorCreases`
[SUBD_TOBREP_PRESETS]: `Default` `DefaultPacked` `DefaultUnpacked`
[SUBD_DISPLAY_PRESETS]: `Empty` `ExtraCoarse` `Coarse` `Medium` `Fine` `ExtraFine` `Default`
[SUBD_CREATION_KNOBS]: `InteriorCreaseTest` `ConvexCornerTest` `ConcaveCornerTest` `TextureCoordinateTest` `MaximumConvexCornerEdgeCount` `MaximumConvexCornerAngleRadians` `MinimumConcaveCornerAngleRadians` `MinimumConcaveCornerEdgeCount` `InterpolateMeshVertices`
[SUBD_TOBREP_KNOBS]: `PackFaces` `ExtraordinaryVertexProcess`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Mesh` construction returns `null` for a single result or `null`/empty for an array on failure; booleans carry the terminal `Result` and a `int[][]` result-to-input map, quad-remesh and convex-hull emit their own index maps, and offset reports the created wall faces, so every native side-channel folds into a typed receipt rather than being discarded.
- Parameter carriers drive the mesher: `MeshingParameters` the density, edge-length, grid, refine, and tolerance policy, `QuadRemeshParameters` the quad-count and symmetry policy, `ReduceMeshParameters` the decimation target, and `ShrinkWrapParameters` the wrap policy; the standing `Default`/`FastRenderMesh`/`QualityRenderMesh`/`DefaultAnalysisMesh` presets seed the common cases.
- Editing mutates in place returning a `bool` or count (`Reduce`, `Weld`, `Subdivide`, `FillHoles`, `CollapseFacesByEdgeLength`, `UnifyNormals`) or new pieces (`Split*`, `Offset`, `ExtractNonManifoldEdges`); the component lists are the topology-read and in-place-edit surface, and `SubD` is a distinct model whose `SubDEdgeTag`, cap style, and component location carry crease-and-corner semantics a mesh cannot represent.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): a nullable single build lifts to `Option<Mesh>`/`Option<SubD>`, a null-or-empty array lands as `Seq<Mesh>`, an in-place `bool`/`int` edit folds into a `Fin` keyed to the mutated geometry, the boolean `Result`+`int[][]` map and the hull/offset side-channels fold into a typed mesh receipt, and `QuadRemeshBrepAsync`/`QuadRemeshAsync` project their `Task<Mesh>` onto the effect rail.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): the closed vocabularies wrap as `[SmartEnum<TKey>]`/`[Flags]`-backed owners; the mesh op models as a `[Union]` over the from-source, generate, boolean, split, and edit arms, and the subd op as a `[Union]` over the create, subdivide, offset, and convert arms.
- `Rasm` kernel: host-neutral isotropic and quad remesh, quadric decimation, and stencil subdivision stand at the kernel altitude and the boundary re-derives none of them; densities, counts, tolerances, and offset distances compose the kernel numeric owners before the native call.

[LOCAL_ADMISSION]:
- Construction enters through the mesh or subd op union: each arm binds its native member, projects the outcome and any index-map or `Result` side-channel onto the rail, and disposes the `MeshExtruder` and `SubDSurfaceInterpolator` solvers through a using scope or lease.
- Native `Mesh`, `SubD`, component lists, and configuration carriers stay inside the construction grant; downstream code receives content-hash-keyed duplicated geometry, the typed mesh or subd receipt, or an owned geometry lease, and the component-list reads project into detached topology records before crossing the boundary.

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: parameter-driven meshing from surfaces and primitives, quad-remesh and shrink-wrap generation, mesh booleans, the reduce/weld/offset/heal/smooth/split edit family, the mesh extruder, and the SubD create/subdivide/offset/to-brep object model.
- Accept: native mesh and subd outcomes projected onto `Fin`/`Option`/`Seq` rails, boolean `Result` and `int[][]` index maps folded into typed receipts, async quad-remesh on the effect rail, and disposable solvers leased.
- Reject: re-deriving kernel-altitude remesh and decimation, exception-style handling of null construction results, discarding the boolean and hull index-map side-channels, and leaking host mesh/subd/collection types past the boundary.
