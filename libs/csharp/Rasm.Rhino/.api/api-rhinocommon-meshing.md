# [RASM_RHINO_API_RHINOCOMMON_MESHING]

This catalog owns the host-fidelity mesh and SubD construction boundary: the native `MeshingParameters`-driven mesher from breps, surfaces, subds, and extrusions, quad-remesh and shrink-wrap generation, mesh booleans, the reduce/weld/offset/heal/smooth/split editing family, the `MeshExtruder` helper, and the `SubD` object model — creation from mesh/surface/loft/sweep/primitives, subdivision, offset, brep conversion, and surface-point interpolation. Every member P/Invokes `rhcommon_c` and returns geometry bit-compatible with Rhino's own commands, standing at the host boundary beside the kernel's host-neutral mesh engines in `Rasm` (isotropic and quad remesh, quadric decimate, stencil subdivision), which own a different altitude and are never re-derived here. Mesh-mesh intersection and clash belong to `Rasm/Analysis/relations`, contour and section extraction to `Rasm/Processing/extract`, and native custody to the geometry catalog; the `Rhino.Geometry.Collections` component lists ride here as the topology-read surface, and every native outcome projects onto the `LanguageExt` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon mesh-and-SubD construction surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Geometry`, `Rhino.Geometry.Collections`
- kernel: `Rasm` (host-neutral remesh, decimate, and subdivision owners composed by altitude, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: mesh-construction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: construction owners
- rail: mesh-construction

| [INDEX] | [SYMBOL]       | [KIND]          | [CAPABILITY]                                                              |
| :-----: | :------------- | :-------------- | :------------------------------------------------------------------------ |
|  [01]   | `Mesh`         | native geometry | from-surface/primitive mesh, quad-remesh, shrink-wrap, booleans, edit set |
|  [02]   | `MeshExtruder` | disposable      | face-set extrusion with wall-face capture                                 |
|  [03]   | `SubD`         | native geometry | create from mesh/surface/loft/sweep/primitive, subdivide, offset, to-brep |

[PUBLIC_TYPE_SCOPE]: configuration carriers
- rail: mesh-construction

| [INDEX] | [SYMBOL]                  | [KIND]         | [CAPABILITY]                                                      |
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
- rail: mesh-construction

| [INDEX] | [SYMBOL]                                            | [KIND]     | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------- | :--------- | :------------------------------------------------- |
|  [01]   | `MeshVertexList` / `MeshFaceList` / `MeshNgonList`  | face lists | vertex, quad/tri face, and ngon geometry and edits |
|  [02]   | `MeshTopologyVertexList` / `MeshTopologyEdgeList`   | topology   | welded-vertex and edge connectivity reads          |
|  [03]   | `MeshVertexNormalList` / `MeshFaceNormalList`       | normals    | per-vertex and per-face normal vectors             |
|  [04]   | `MeshVertexColorList` / `MeshTextureCoordinateList` | attributes | per-vertex color and texture-uv coordinates        |
|  [05]   | `SubDVertexList` / `SubDEdgeList` / `SubDFaceList`  | subd lists | subd component enumeration and tag reads           |

[CARRIER_MEMBERS]:
- `MeshingParameters` (`IDisposable`) — presets `Minimal`, `Default`, `FastRenderMesh`, `QualityRenderMesh`, `DefaultAnalysisMesh`, `DocumentCurrentSetting(RhinoDoc doc)`; `Coarse`/`Smooth` are `[Obsolete]` for the render presets; ctors `()`, `(MeshingParameters source)`, `(double density[, double minimumEdgeLength])`; `get/set` — `TextureRange : MeshingParameterTextureRange`, `JaggedSeams`, `RefineGrid`, `DoublePrecision`, `SimplePlanes`, `ComputeCurvature`, `ClosedObjectPostProcess : bool`, `GridMinCount`/`GridMaxCount : int`, `GridAngle`/`GridAspectRatio`/`GridAmplification : double`, `Tolerance`/`MinimumTolerance`/`RelativeTolerance`/`MinimumEdgeLength`/`MaximumEdgeLength`/`RefineAngle`/`RefineAngleInDegrees : double`; round-trip via `ToEncodedString()`/`FromEncodedString(string)`.
- `QuadRemeshParameters` — `get/set`: `TargetEdgeLength : double` (nonzero overrides the count), `TargetQuadCount : int` (2000), `AdaptiveSize : double` (50, range 0-100), `AdaptiveQuadCount : bool`, `DetectHardEdges : bool`, `GuideCurveInfluence : int` (0 approximate, 1 interp-edge-ring, 2 interp-edge-loop), `PreserveMeshArrayEdgesMode : int` (0 off, 1 smart, 2 strict), `SymmetryAxis : QuadRemeshSymmetryAxis`; quad-remesh progress is `IProgress<int>`, never `IProgress<double>`.
- `ShrinkWrapParameters` — `get/set`: `TargetEdgeLength : double` (1.0), `Offset : double`, `SmoothingIterations : int`, `FillHolesInInputObjects : bool`, `PolygonOptimization : int` (10), `InflateVerticesAndPoints : bool`, `PreserveColors : bool`; the builder is the static `Mesh.ShrinkWrap`, and no `CreateShrinkWrap` spelling exists.
- `ReduceMeshParameters` — `get/set`: `DesiredPolygonCount : int`, `AllowDistortion : bool`, `Accuracy : int`, `NormalizeMeshSize : bool`, `FaceTags : int[]`, `LockedComponents : ComponentIndex[]`, `CancelToken : CancellationToken`, `ProgressReporter : IProgress<double>`; `Error : string` is the host-written diagnostic.
- `MeshBooleanOptions` — `get/set`: `Tolerance : double`, `TextLog : TextLog`, `CancellationToken : CancellationToken`, `ProgressReporter : IProgress<double>`.
- `MeshExtruder(Mesh inputMesh, IEnumerable<ComponentIndex> componentIndices)` — `get/set`: `Transform : Transform`, `UVN`/`EdgeBasedUVN`/`KeepOriginalFaces : bool`, `TextureCoordinateMode`/`SurfaceParameterMode : MeshExtruderParameterMode` (`CoverWalls`, `KeepAndStretch`), `FaceDirectionMode : MeshExtruderFaceDirectionMode` (`Keep`, `OrientClosedFrontOut`); `PreviewLines : Line[]`; `ExtrudedMesh(out Mesh extrudedMeshOut[, out List<ComponentIndex> componentIndicesOut]) : bool`; `GetWallFaces() : List<int>`.
- `SubDCreationOptions` (`IDisposable`) — presets `Smooth`, `InteriorCreases`, `ConvexCornersAndInteriorCreases`, `ConvexAndConcaveCornersAndInteriorCreases`; `get/set`: `InteriorCreaseTest : InteriorCreaseOption` (`Unset`, `None`, `AtMeshDoubleEdge`), `ConvexCornerTest : ConvexCornerOption` (`Unset`, `None`, `AtMeshCorner`), `ConcaveCornerTest : ConcaveCornerOption` (`Unset`, `None`, `AtMeshCorner`), `TextureCoordinateTest : TextureCoordinateOption` (`Unset`, `None`, `Automatic`, `Packed`, `CopyMapping`, `CopyCoordinates`), `MaximumConvexCornerEdgeCount : uint`, `MaximumConvexCornerAngleRadians : double`, `MinimumConcaveCornerAngleRadians : double`, `MinimumConcaveCornerEdgeCount : uint`, `InterpolateMeshVertices : bool`.
- `SubDToBrepOptions` (`IDisposable`) — presets `Default`, `DefaultPacked`, `DefaultUnpacked`; ctor `(bool packFaces, ExtraordinaryVertexProcessOption vertexProcess)`; `get/set`: `PackFaces : bool`, `ExtraordinaryVertexProcess : ExtraordinaryVertexProcessOption` (`None`, `LocalG1`, `LocalG2`, `LocalG1x`, `LocalG1xx`).
- `SubDDisplayParameters` (`IDisposable`) — statics `Empty()`/`ExtraCoarse()`/`Coarse()`/`Medium()`/`Fine()`/`ExtraFine()`/`Default()`, `CreateFromDisplayDensity(uint)`, `CreateFromAbsoluteDisplayDensity(uint)`, `CreateFromMeshDensity(double)`; nested `enum Density : uint` — `UnsetDensity = 0` through `MaximumDensity = 6` with `ExtraCoarseDensity = 1`, `CoarseDensity = 2`, `MediumDensity = 3`, `FineDensity = 4` (= `DefaultDensity`), `ExtraFineDensity = 5`.
- `Rhino.Geometry.MeshRefinements` is the nested namespace owning `RefinementSettings` (`Level : int`, `NakedEdgeMode : CreaseEdges`, `ContinueRequest : CancellationToken`), `LoopFormula` (`Loop`, `Warren`, `WarrenWeimer`), and `CreaseEdges` (`NakedFixed`, `NakedSmooth`, `CornerFixedOtherCreased`, `Auto`) consumed by `CreateRefinedLoopMesh`/`CreateRefinedCatmullClarkMesh`.
- spelling truths — `Mesh.CreateFromIsosurface` (lowercase `s`), `Mesh.CollapseFacesByByAspectRatio` (doubled `By` in the assembly), `MeshVertexList.GetTopologicalIndenticalVertices` (host misspelling), and `Mesh.CollapseFacesByArea(double lessThanArea, double greaterThanArea) : int` beside the edge-length collapse.
- edit additions — `Weld(double angleToleranceRadians, bool preserveSurfaceParameters)`, instance `MatchEdges(double distance, bool rachet) : bool`, `ExplodeAtUnweldedEdges() : Mesh[]`, `CreateUnweldedMesh(Mesh) : Mesh`, `CreateFromPatchSingleFace(Mesh, IEnumerable<ComponentIndex>) : Mesh`, `CreateFromFilteredFaceList(Mesh, IEnumerable<bool>) : Mesh`, `MeshFaceList.ExtractFaces(IEnumerable<int>) : Mesh`; the value-semantic finishing family returns a new mesh — `WithEdgeSoftening(double softeningRadius, bool chamfer, bool faceted, bool force, double angleThreshold) : Mesh`, `WithShutLining(bool faceted, double tolerance, IEnumerable<ShutLiningCurveInfo>) : Mesh`, `WithDisplacement(MeshDisplacementInfo) : Mesh`.
- SubD truths — no quad-remesh entry exists on `SubD` (route: `Mesh.QuadRemesh` then `SubD.CreateFromMesh`); subdivision is `Subdivide()`/`Subdivide(int count)`/`Subdivide(IEnumerable<int> faceIndices)` with no `GlobalSubdivide`; `SetVertexSurfacePoint(uint vertexIndex, Point3d surfacePoint) : bool`, `UpdateAllTagsAndSectorCoefficients() : uint`, and `UpdateSurfaceMeshCache(bool lazyUpdate) : uint` ride the edit surface; `SubDVertexList.SetVertexTags(IEnumerable<int>, SubDVertexTag)` and `SubDEdgeList.SetEdgeTags(IEnumerable<int>, SubDEdgeTag)` author tags; `public enum SubDVertexTag : byte` — `Unset`, `Smooth`, `Crease`, `Corner`, `Dart`.

[ENUM_ROSTERS]:
- `public enum Rhino.Geometry.MeshPipeCapStyle` — `None`, `Flat`, `Box`, `Dome`.
- `[Flags] public enum Rhino.Geometry.QuadRemeshSymmetryAxis` — `None = 0`, `X = 1`, `Y = 2`, `Z = 4`.
- `public enum Rhino.Geometry.SubDFromSurfaceMethods : byte` — `Unset`, `SubDFriendlyFit`, `FromNurbsControlNet`.
- `public enum Rhino.Geometry.SubDEndCapStyle` — `Unset`, `None`, `Triangles`, `Quads`, `Ngon`.
- `public enum Rhino.Geometry.SubDEdgeTag` — `Unset = 0`, `Smooth = 1`, `Crease = 2`, `SmoothX = 4`.
- `public enum Rhino.Geometry.SubDComponentLocation : byte` — `Unset`, `ControlNet`, `Surface`.

## [03]-[ENTRYPOINTS]

[MESH_FROM_SURFACE]:
- `Rhino.Geometry.Mesh.CreateFromBrep(Brep brep, MeshingParameters meshingParameters) : Mesh[]` — meshes each brep face under a parameter policy; a null return signals failure and the array carries one mesh per face.
- `Rhino.Geometry.Mesh.CreateFromSurface(Surface surface, MeshingParameters meshingParameters) : Mesh` / `CreateFromExtrusion(Extrusion extrusion, MeshingParameters meshingParameters) : Mesh` — single-surface and extrusion meshing.
- `Rhino.Geometry.Mesh.CreateFromSubD(SubD subd, int displayDensity) : Mesh` — meshes a subd at a display density; the `(SubD, SubDDisplayParameters.Density)` overload names the density level.
- `Rhino.Geometry.Mesh.CreateFromSubDControlNet(SubD subd) : Mesh` / `CreateFromSubDControlNetWithTextureCoordinates(SubD subd) : Mesh` / `CreateFromSurfaceControlNet(Surface surface) : Mesh` — control-net cage meshes.
- `Rhino.Geometry.Mesh.CreateFromPlanarBoundary(Curve boundary, MeshingParameters parameters, double tolerance) : Mesh` — meshes a planar region inside a boundary curve.

[MESH_FROM_PRIMITIVES]:
- `Rhino.Geometry.Mesh.CreateFromBox(Box box, int xCount, int yCount, int zCount) : Mesh` — a faceted box; the `BoundingBox` and eight-corner overloads frame it differently.
- `Rhino.Geometry.Mesh.CreateFromSphere(Sphere sphere, int xCount, int yCount) : Mesh` / `CreateIcoSphere(Sphere, int subdivisions) : Mesh` / `CreateQuadSphere(Sphere, int subdivisions) : Mesh` — uv, icosahedral, and quad sphere tessellations.
- `Rhino.Geometry.Mesh.CreateFromCylinder(Cylinder cylinder, int vertical, int around, bool capBottom, bool capTop, bool circumscribe, bool quadCaps) : Mesh` / `CreateFromCone(Cone, int vertical, int around, bool solid, bool quadCaps) : Mesh` / `CreateFromTorus(Torus, int vertical, int around) : Mesh` — capped analytic solids.
- `Rhino.Geometry.Mesh.CreateFromPlane(Plane plane, Interval xInterval, Interval yInterval, int xCount, int yCount) : Mesh` / `CreateFromClosedPolyline(Polyline polyline) : Mesh` — a gridded plane and a fan-triangulated closed polyline.

[MESH_GENERATE]:
- `Rhino.Geometry.Mesh.QuadRemeshBrep(Brep brep, QuadRemeshParameters parameters, IEnumerable<Curve> guideCurves, IProgress<int> progress, CancellationToken cancelToken) : Mesh` — quad-remeshes a brep along guide curves with progress and cancellation; `QuadRemeshBrepAsync(...)` returns `Task<Mesh>`.
- `Rhino.Geometry.Mesh.ShrinkWrap(IEnumerable<GeometryBase> geometryBases, ShrinkWrapParameters parameters, MeshingParameters meshingParameters, CancellationToken token) : Mesh` — wraps mixed geometry; the `IEnumerable<Mesh>` and `PointCloud` overloads wrap those sources directly.
- `Rhino.Geometry.Mesh.CreateFromCurvePipe(Curve curve, double radius, int segments, int accuracy, MeshPipeCapStyle capType, bool faceted, IEnumerable<Interval> intervals = null) : Mesh` / `CreateFromCurveExtrusion(Curve, Vector3d direction, MeshingParameters, BoundingBox) : Mesh` / `CreateExtrusion(Curve profile, Vector3d direction, MeshingParameters parameters) : Mesh` — pipe and extrusion meshes directly from a curve.
- `Rhino.Geometry.Mesh.CreateFromIsosurface(Func<Point3d, double> scalarFieldEvaluator, BoundingBox box, int resolution, int RootFindingMaxSteps) : Mesh` — marching-cubes isosurface of a scalar field.
- `Rhino.Geometry.Mesh.CreateFromLines(Curve[] lines, int maxFaceValence, double tolerance) : Mesh` / `CreateFromTessellation(IEnumerable<Point3d> points, IEnumerable<IEnumerable<Point3d>> edges, Plane plane, bool allowNewVertices) : Mesh` — mesh from an edge-line network or a constrained tessellation.
- `Rhino.Geometry.Mesh.CreateConvexHull3D(IEnumerable<Point3d> points, out int[][] hullFacets, double tolerance, double angleTolerance) : Mesh` — a convex hull with the facet index map.
- `Rhino.Geometry.Mesh.CreatePatch(Polyline outerBoundary, double angleToleranceRadians, Surface pullbackSurface, IEnumerable<Curve> innerBoundaryCurves, IEnumerable<Curve> innerBothSideCurves, IEnumerable<Point3d> innerPoints, bool trimback, int divisions) : Mesh` — a patch mesh over mixed constraints.
- `Rhino.Geometry.Mesh.CreateFromIterativeCleanup(IEnumerable<Mesh> meshes, double tolerance) : Mesh[]` / `RebuildMesh(Mesh mesh, bool preserveTextureCoordinates, bool preserveVertexColors) : Mesh` — cleanup rebuild; `RequireIterativeCleanup(meshes, tolerance) : bool` probes whether cleanup is needed.
- `Rhino.Geometry.Mesh.CreateRefinedLoopMesh(Mesh mesh, LoopFormula formula = LoopFormula.WarrenWeimer, RefinementSettings settings = null) : Mesh` / `CreateRefinedCatmullClarkMesh(Mesh mesh, RefinementSettings settings = null) : Mesh` — Loop and Catmull-Clark subdivision refinement of a control mesh under a `LoopFormula` weighting and `RefinementSettings` policy.

[MESH_BOOLEANS]:
- `Rhino.Geometry.Mesh.CreateBooleanUnion(IEnumerable<Mesh> meshes, MeshBooleanOptions options, out Result commandResult, out int[][] inputMap) : Mesh[]` — unions with the terminal `Result` and a result-to-input map; the `(meshes)` and `(meshes, tolerance)` overloads drop the diagnostics.
- `Rhino.Geometry.Mesh.CreateBooleanDifference(IEnumerable<Mesh> firstSet, IEnumerable<Mesh> secondSet, MeshBooleanOptions options, out Result result, out int[][] inputMap) : Mesh[]` / `CreateBooleanIntersection(...) : Mesh[]` / `CreateBooleanSplit(IEnumerable<Mesh> meshesToSplit, IEnumerable<Mesh> meshSplitters, MeshBooleanOptions options, out Result result, out int[][] inputMap) : Mesh[]` — the difference, intersection, and split arms carrying the same diagnostics.

[MESH_SPLIT_PARTITION]:
- `Rhino.Geometry.Mesh.SplitMesh(Mesh mesh, int maxCount, bool countSum, bool countTriangles) : Mesh[]` / `PartitionMesh(Mesh mesh, int maxVertexCount, int maxFaceCount) : Mesh[]` — count-bounded splitting and partitioning.
- `Rhino.Geometry.Mesh.MatchEdges(IEnumerable<Mesh> inputMeshes, double distance, bool simpleSplits, bool rachet, bool average, bool join) : Mesh[]` — aligns coincident naked edges across meshes with split, ratchet, average, and join policy.
- `Rhino.Geometry.Mesh.ComputeThickness(IEnumerable<Mesh> meshes, double maximumThickness, double sharpAngle, CancellationToken cancelToken) : MeshThicknessMeasurement[]` — per-face wall-thickness measurements; the `(meshes, maximumThickness)` and `(meshes, maximumThickness, cancelToken)` overloads drop the sharp-angle bound.

[MESH_EDIT]:
- `Rhino.Geometry.Mesh.Reduce(ReduceMeshParameters parameters, bool threaded) : bool` — decimates in place under a parameter carrier; the `(desiredPolygonCount, allowDistortion, accuracy, normalizeSize, CancellationToken, IProgress<double>, out string problemDescription)` overload exposes cancellation and a diagnostic string.
- `Rhino.Geometry.Mesh.QuadRemesh(QuadRemeshParameters parameters, IEnumerable<Curve> guideCurves) : Mesh` / `QuadRemeshAsync(QuadRemeshParameters parameters, IProgress<int> progress, CancellationToken cancelToken) : Task<Mesh>` — instance quad-remesh with a face-block overload for scoped remeshing.
- `Rhino.Geometry.Mesh.ShrinkWrap(ShrinkWrapParameters parameters, CancellationToken token) : Mesh` — instance shrink-wrap of this mesh.
- `Rhino.Geometry.Mesh.Offset(double distance, bool solidify, Vector3d direction, out List<int> wallFacesOut) : Mesh` — offsets and optionally solidifies, reporting the wall faces; the reduced overloads drop the direction and wall-face output.
- `Rhino.Geometry.Mesh.Weld(double angleToleranceRadians) : void` / `Unweld(double angleToleranceRadians, bool modifyNormals) : void` / `UnweldEdge(IEnumerable<int> edgeIndices, bool modifyNormals) : bool` / `UnweldVertices(IEnumerable<int> topologyVertexIndices, bool modifyNormals) : bool` — vertex welding and per-edge/per-vertex unwelding.
- `Rhino.Geometry.Mesh.Smooth(IEnumerable<int> vertexIndices, double smoothFactor, int numSteps, bool bXSmooth, bool bYSmooth, bool bZSmooth, bool bFixBoundaries, SmoothingCoordinateSystem coordinateSystem, Plane plane) : bool` — scoped multi-step smoothing; the whole-mesh overloads drop the vertex set.
- `Rhino.Geometry.Mesh.Subdivide(IEnumerable<int> faceIndices) : bool` — mid-edge subdivision, scoped or whole-mesh.
- `Rhino.Geometry.Mesh.FillHoles() : bool` / `FillHole(int topologyEdgeIndex) : bool` / `HealNakedEdges(double distance) : bool` / `ExtractNonManifoldEdges(bool selective) : Mesh` — hole and naked-edge repair, and non-manifold-edge extraction.
- `Rhino.Geometry.Mesh.Split(IEnumerable<Mesh> meshes, double tolerance, bool splitAtCoplanar, bool createNgons, TextLog textLog, CancellationToken cancel, IProgress<double> progress) : Mesh[]` — splits by other meshes or a plane; `SplitDisjointPieces() : Mesh[]`, `SplitNon2Manifolds() : Mesh[]`, and `SplitWithProjectedPolylines(IEnumerable<PolylineCurve> curves, double tolerance) : Mesh[]` separate along the named criteria.
- `Rhino.Geometry.Mesh.CollapseFacesByEdgeLength(bool bGreaterThan, double edgeLength) : int` / `MergeAllCoplanarFaces(double tolerance, double angleTolerance) : bool` — edge-length face collapse and coplanar-face merge.
- `Rhino.Geometry.Mesh.Append(IEnumerable<Mesh> meshes) : void` / `Flip(bool vertexNormals, bool faceNormals, bool faceOrientation, bool ngonsBoundaryDirection) : void` / `RebuildNormals() : void` / `UnifyNormals() : int` / `Compact() : bool` — aggregation, orientation, normal rebuild, and slot compaction.
- `Rhino.Geometry.Mesh.GetNakedEdges() : Polyline[]` / `GetOutlines(Plane plane) : Polyline[]` — boundary and silhouette polylines; the `(RhinoViewport)` and `(ViewportInfo, Plane)` overloads project to view.

[MESH_EXTRUDER]:
- `Rhino.Geometry.MeshExtruder(Mesh inputMesh, IEnumerable<ComponentIndex> componentIndices)` — constructs an extruder over a face set; the instance carries the extrusion transform state.
- `Rhino.Geometry.MeshExtruder.ExtrudedMesh(out Mesh extrudedMeshOut, out List<ComponentIndex> componentIndicesOut) : bool` / `GetWallFaces() : List<int>` — the extruded result with created-component indices and the wall-face list.

[MESH_TOPOLOGY]:
- `Rhino.Geometry.Mesh.Vertices : MeshVertexList` / `Faces : MeshFaceList` / `Ngons : MeshNgonList` / `Normals : MeshVertexNormalList` / `FaceNormals : MeshFaceNormalList` / `VertexColors : MeshVertexColorList` / `TextureCoordinates : MeshTextureCoordinateList` — the geometry and attribute lists edited in place.
- `Rhino.Geometry.Mesh.TopologyVertices : MeshTopologyVertexList` / `TopologyEdges : MeshTopologyEdgeList` — the welded-connectivity read surface distinct from the raw vertex list.

[SUBD_BUILD]:
- `Rhino.Geometry.SubD.CreateFromMesh(Mesh mesh, SubDCreationOptions options) : SubD` — converts a mesh to a subd under corner/crease/texture policy; the `(Mesh)` overload uses defaults.
- `Rhino.Geometry.SubD.CreateFromSurface(Surface surface, SubDFromSurfaceMethods method, bool corners) : SubD` — subd from a surface by fit or control-net method.
- `Rhino.Geometry.SubD.CreateFromLoft(IEnumerable<NurbsCurve> curves, bool closed, bool addCorners, bool addCreases, int divisions) : SubD` / `CreateFromSweep(NurbsCurve rail1, IEnumerable<NurbsCurve> shapes, bool closed, bool addCorners, bool roadlikeFrame, Vector3d roadlikeNormal) : SubD` — loft and one-rail sweep; the `(rail1, rail2, shapes, closed, addCorners)` overload is the two-rail sweep.
- `Rhino.Geometry.SubD.CreateFromCylinder(Cylinder cylinder, uint circumferenceFaceCount, uint heightFaceCount, SubDEndCapStyle endCapStyle, SubDEdgeTag endCapEdgeTag, SubDComponentLocation radiusLocation) : SubD` — a faceted subd cylinder with cap and edge-tag policy.
- `Rhino.Geometry.SubD.CreateQuadSphere(Sphere sphere, SubDComponentLocation vertexLocation, uint quadSubdivisionLevel) : SubD` / `CreateGlobeSphere(Sphere, SubDComponentLocation, uint axialFaceCount, uint equatorialFaceCount) : SubD` / `CreateTriSphere(Sphere, SubDComponentLocation, uint triSubdivisionLevel) : SubD` / `CreateIcosahedron(Sphere, SubDComponentLocation) : SubD` — the four subd sphere topologies.
- `Rhino.Geometry.SubD.JoinSubDs(IEnumerable<SubD> subdsToJoin, double tolerance, bool joinedEdgesAreCreases, bool preserveSymmetry) : SubD[]` — joins subds along coincident edges.

[SUBD_EDIT]:
- `Rhino.Geometry.SubD.Subdivide(int count) : bool` — global subdivision by level; the `(IEnumerable<int> faceIndices)` overload scopes it.
- `Rhino.Geometry.SubD.Offset(double distance, bool solidify) : SubD` — offsets, optionally to a closed shell.
- `Rhino.Geometry.SubD.ToBrep(SubDToBrepOptions options) : Brep` — converts to a brep under packing and vertex-processing policy; the `()` overload uses defaults.
- `Rhino.Geometry.SubD.InterpolateSurfacePoints(Point3d[] surfacePoints) : bool` — solves control points so the subd limit surface passes through the targets; the `(uint[] vertexIndices, Point3d[] surfacePoints)` overload constrains a vertex subset.
- `Rhino.Geometry.SubD.MergeAllCoplanarFaces(double tolerance, double angleTolerance) : bool` / `PackFaces() : uint` / `Flip() : bool` — coplanar-face merge, face packing, and orientation flip.
- `Rhino.Geometry.SubD.TransformComponents(IEnumerable<ComponentIndex> components, Transform xform, SubDComponentLocation componentLocation) : uint` — transforms a component subset at the control-net or surface location.

[SUBD_TOPOLOGY]:
- `Rhino.Geometry.SubD.Vertices : SubDVertexList` / `Edges : SubDEdgeList` / `Faces : SubDFaceList` — component enumeration carrying per-edge `SubDEdgeTag` reads.
- `Rhino.Geometry.SubD.DuplicateEdgeCurves(bool boundaryOnly, bool interiorOnly, bool smoothOnly, bool sharpOnly, bool creaseOnly, bool clampEnds) : Curve[]` — filtered edge-curve extraction.

## [04]-[IMPLEMENTATION_LAW]

[MESH_TOPOLOGY]:
- `Mesh` construction returns `null` for a single result or `null`/empty for an array on failure; booleans additionally carry the terminal `Result` and a `int[][]` result-to-input map, quad-remesh and convex-hull emit their own index maps, and offset reports the created wall faces, so every diagnostic native side-channel folds into a typed receipt rather than being discarded.
- parameter carriers drive the mesher: `MeshingParameters` carries the density, edge-length, grid, refine, and tolerance policy from breps, surfaces, subds, and extrusions, `QuadRemeshParameters` the quad-count and symmetry policy, `ReduceMeshParameters` the decimation target, and `ShrinkWrapParameters` the wrap policy; the standing `MeshingParameters.Default`/`FastRenderMesh`/`QualityRenderMesh`/`DefaultAnalysisMesh` presets seed the common cases.
- editing mutates in place and returns a `bool` or a count (`Reduce`, `Weld`, `Subdivide`, `FillHoles`, `CollapseFacesByEdgeLength`, `UnifyNormals`) or returns new pieces (`Split*`, `Offset`, `ExtractNonManifoldEdges`); the component lists (`Vertices`, `Faces`, `Ngons`, `TopologyVertices`, `TopologyEdges`, attribute lists) are the topology-read and in-place-edit surface, and `SubD` is a distinct object model whose `SubDEdgeTag`, cap style, and component location carry the crease-and-corner semantics a mesh has no representation for.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a nullable single build lifts to `Option<Mesh>`/`Option<SubD>`, a possibly-null-or-empty array lands as `Seq<Mesh>`, an in-place `bool`/`int` edit folds into a `Fin` keyed to the mutated geometry, the boolean `Result`+`int[][]` map and the convex-hull/offset side-channels fold into a typed mesh receipt, and `QuadRemeshBrepAsync`/`QuadRemeshAsync` project their `Task<Mesh>` onto the effect rail.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): the closed vocabularies — `MeshPipeCapStyle`, the `[Flags]` `QuadRemeshSymmetryAxis`, `SubDFromSurfaceMethods`, `SubDEndCapStyle`, `SubDEdgeTag`, and `SubDComponentLocation` — wrap as `[SmartEnum<TKey>]`/`[Flags]`-backed owners; the mesh op models as a `[Union]` over the from-source, generate, boolean, split, and edit arms, and the subd op as a `[Union]` over the create, subdivide, offset, and convert arms.
- `Rasm` kernel: host-neutral isotropic and quad remesh, quadric decimation, and stencil subdivision stand at the kernel altitude and the boundary re-derives none of them; densities, counts, tolerances, and offset distances compose the kernel numeric owners before the native call.

[LOCAL_ADMISSION]:
- construction enters through the mesh or subd op union: each arm binds its native member, projects the outcome and any index-map or `Result` side-channel onto the rail, and disposes the `MeshExtruder` and `SubDSurfaceInterpolator` solvers through a using scope or lease.
- native `Mesh`, `SubD`, component lists, and configuration carriers stay inside the construction grant; downstream code receives duplicated canonical geometry keyed by content hash, the typed mesh or subd receipt, or an explicitly owned geometry lease, and the component-list reads project into detached topology records before crossing the boundary.

[RAIL_LAW]:
- Surface: `Rhino.Geometry` + `Rhino.Geometry.Collections` mesh and SubD host-fidelity construction
- Owns: parameter-driven meshing from surfaces and primitives, quad-remesh and shrink-wrap generation, mesh booleans, the reduce/weld/offset/heal/smooth/split edit family, the mesh extruder, and the SubD create/subdivide/offset/to-brep object model.
- Accept: native mesh and subd outcomes projected onto `Fin`/`Option`/`Seq` rails, boolean `Result` and `int[][]` index maps folded into typed receipts, async quad-remesh on the effect rail, and disposable solvers leased.
- Reject: re-deriving kernel-altitude remesh and decimation, exception-style handling of null construction results, discarding the boolean and hull index-map side-channels, and leaking host mesh/subd/collection types past the boundary.
