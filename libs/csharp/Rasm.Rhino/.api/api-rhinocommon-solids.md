# [RASM_RHINO_API_RHINOCOMMON_SOLIDS]

`Rhino.Geometry` `Brep`, `Extrusion`, and `DevelopableSrf` construction P/Invokes `rhcommon_c`, so every static returns geometry bit-compatible with Rhino's own kernel commands and this catalog stands at the host boundary, not the kernel. Host-neutral robust construction, booleans, and offsets live in the `Rasm` kernel and compose by altitude; brep intersection, mass/area measure, contour/iso extraction, and native custody stay with their own catalogs. Every native `bool`/`out`-param/array outcome projects onto the `LanguageExt` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon Brep-construction surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Geometry`
- kernel: `Rasm` (host-neutral robust construction and numeric owners composed by altitude)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: solid-construction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: construction owners
- rail: solid-construction

| [INDEX] | [SYMBOL]         | [KIND]          | [CAPABILITY]                                                                 |
| :-----: | :--------------- | :-------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Brep`           | native geometry | boolean, fillet/blend, offset/shell/pipe, sweep/loft/patch, join/split/match |
|  [02]   | `Extrusion`      | native geometry | profile-and-path lightweight solid, brep conversion, wall/profile reads      |
|  [03]   | `DevelopableSrf` | ruling helper   | developable-loft ruling extraction between two rails                         |

[PUBLIC_TYPE_SCOPE]: configuration carriers
- rail: solid-construction

| [INDEX] | [SYMBOL]                        | [KIND]         | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Brep.FilletSurfaceSettings`    | config carrier | radius, tolerance, degree, trim/extend for surface-fillet build  |
|  [02]   | `Brep.FilletSurfaceResults`     | result carrier | fillet, mid-fillet, and trimmed-input outputs                    |
|  [03]   | `BrepEdgeFilletDistance`        | value struct   | edge parameter paired with fillet distance for variable radius   |
|  [04]   | `MatchSrfSettings`              | config carrier | continuity, averaging, refinement policy for edge-to-curve match |
|  [05]   | `Brep.VariationalPatchSettings` | config carrier | stretching, bending, refinement policy for variational patch     |
|  [06]   | `Brep.VariationalPatchResult`   | result carrier | patch surface and fit-deviation evidence                         |
|  [07]   | `Brep.CurveConstraint`          | value carrier  | curve and `Continuity` row for the variational patch solver      |
|  [08]   | `Brep.PointConstraint`          | value carrier  | point constraint row for the variational patch solver            |
|  [09]   | `SweepOneRail`                  | class engine   | one-rail sweep with station parameters and roadlike up-direction |
|  [10]   | `SweepTwoRail`                  | class engine   | two-rail sweep with per-rail station parameters and height law   |
|  [11]   | `SurfaceFilletBase`             | class engine   | static section-profile factories filling caller-owned lists      |

[CARRIER_MEMBERS]:
- `Brep.FilletSurfaceSettings` — `Radius`, `SecondRadius`, `Tolerance`, `AngleTolerance`, `Trim`, `Chamfer`, `G2Blend`, `NonRational`, `Extend`, `Degree`, `TanSlider`, `InnerSlider` are `get`-only; `ContinueAcrossTangentFaces : bool` is the one settable knob, and the four factories bake the profile via the `Chamfer`/`G2Blend`/`NonRational` flag columns.
- `Brep.FilletSurfaceResults` — `Face0`/`Face1 : BrepFace`, `OutBreps0`/`OutBreps1`/`Fillets : IList<Brep>`, all `get`-only.
- `MatchSrfSettings(Continuity match, Continuity otherEnd)` — settable `Average`, `MatchClosestPoints`, `PreserveIso : PreserveIsoCurveMethod`, `ReverseMatchDirection`, `ReverseAverageTargetDirection`; refinement enters through `EnableRefinement(bool, double positionalTolerance, double angleToleranceRadians, double curvatureTolerance)` backing the read-only `RefineMatch`/`Refine*Tolerance`; `enum PreserveIsoCurveMethod` — `Automatic`, `MatchTarget`, `Perpendicular`, `Preserve`.
- `Brep.VariationalPatchSettings` — all `get/set`: `Tolerance`, `InternalTolerance`, `AngleToleranceRadians`, `CurvatureRelativeTolerance`, `CurvatureZeroTolerance`, `DegreeU`/`DegreeV`, `SpanCountU`/`SpanCountV`, `Domain : RhinoVariationalDomain`, `Stretching`, `Bending`, `RocBending`, `UVRotation`, `MaxRefinements`, `InitialSurface : Surface`, `PreserveEdges`; `enum RhinoVariationalDomain` — `Projected`, `Molded`, `Untrimmed`, `MultiBlend`.
- `Brep.VariationalPatchResult` — `Warning`/`Error : string`, `G0Int`/`G0`/`G1`/`G2 : bool?`, all `get`-only.
- `Brep.CurveConstraint(Curve curve[, Continuity continuity])` with a public `Continuity` field; `Brep.PointConstraint(Point3d point)`.
- `SweepOneRail` — `SweepTolerance`/`AngleToleranceRadians : double`, `MiterType : int`, `ClosedSweep`/`GlobalShapeBlending : bool` all `get/set`; roadlike state via `SetToRoadlikeTop()`/`SetToRoadlikeFront()`/`SetToRoadlikeRight()`/`SetRoadlikeUpDirection(Vector3d)` with `IsFreeform`/`IsRoadlike*` probes; `PerformSweep`/`PerformSweepRebuild(… int rebuildCount)`/`PerformSweepRefit(… double refitTolerance)` over `(rail, crossSection[s][, crossSectionParameter[s]])`, all returning `Brep[]`.
- `SweepTwoRail` — `SweepTolerance`/`AngleToleranceRadians : double`, `MaintainHeight`/`ClosedSweep`/`AutoAdjust`/`UseLegacySweeper : bool` all `get/set`; `PerformSweep`/`PerformSweepRebuild`/`PerformSweepRefit` over `(rail1, rail2, crossSection[s][, crossSectionParametersRail1, crossSectionParametersRail2])`, all returning `Brep[]`.

[ENUM_ROSTERS]:
- `enum Rhino.Geometry.LoftType` — `Normal`, `Loose`, `Tight`, `Straight`, `Uniform`.
- `enum Rhino.Geometry.BlendContinuity` — `Position`, `Tangency`, `Curvature`, `G3`, `G4`.
- `enum Rhino.Geometry.BlendType` — `Chamfer`, `Fillet`, `Blend`.
- `enum Rhino.Geometry.RailType` — `DistanceFromEdge`, `RollingBall`, `DistanceBetweenRails`.
- `enum Rhino.Geometry.PipeCapMode` — `None`, `Flat`, `Round`.
- `enum Rhino.Geometry.ExtrudeCornerType` — `None`, `Sharp`, `Round`, `Smooth`, `Chamfer`.
- `enum Rhino.Geometry.FilletSurfaceSplitType` — `Nothing`, `Trim`, `Split`.
- `enum Rhino.Geometry.SweepFrame` — `Freeform`, `Roadlike`.
- `enum Rhino.Geometry.SweepBlend` — `Local`, `Global`.
- `enum Rhino.Geometry.SweepMiter` — `None`, `Trimmed`, `Untrimmed`.
- `enum Rhino.Geometry.SweepRebuild` — `None`, `Rebuild`, `Refit`.

## [03]-[ENTRYPOINTS]

[BREP_BOOLEANS]:
- `Rhino.Geometry.Brep.CreateBooleanUnion(IEnumerable<Brep> breps, double tolerance, bool manifoldOnly, out Point3d[] nakedEdgePoints, out Point3d[] badIntersectionPoints, out Point3d[] nonManifoldEdgePoints) : Brep[]` — unions a set; a null result signals failure and the three point arrays diagnose it, while the `(breps, tolerance)` and `(breps, tolerance, manifoldOnly)` overloads drop the diagnostics.
- `Rhino.Geometry.Brep.CreateBooleanIntersection(IEnumerable<Brep> firstSet, IEnumerable<Brep> secondSet, double tolerance, bool manifoldOnly) : Brep[]` — set intersection; the `(Brep, Brep, …)` pair intersects two solids.
- `Rhino.Geometry.Brep.CreateBooleanDifference(IEnumerable<Brep> firstSet, IEnumerable<Brep> secondSet, double tolerance, bool manifoldOnly) : Brep[]` — subtracts the second set; the `(Brep, Brep, …)` pair subtracts one solid.
- `Rhino.Geometry.Brep.CreateBooleanDifferenceWithIndexMap(IEnumerable<Brep> firstSet, IEnumerable<Brep> secondSet, double tolerance, bool manifoldOnly, out int[] indexMap) : Brep[]` — subtracts and maps each result to its source first-set index.
- `Rhino.Geometry.Brep.CreateBooleanSplit(IEnumerable<Brep> firstSet, IEnumerable<Brep> secondSet, double tolerance) : Brep[]` — splits the first set by the second; the `(Brep, Brep, tolerance)` pair splits one solid.
- `Rhino.Geometry.Brep.CreatePlanarUnion(IEnumerable<Brep> breps, Plane plane, double tolerance) : Brep[]` / `CreatePlanarDifference(...)` / `CreatePlanarIntersection(...)` — coplanar-region booleans beside the `(Brep b0, Brep b1, Plane, tolerance)` two-region pairs.
- `Rhino.Geometry.Brep.CreateSolid(IEnumerable<Brep> breps, double tolerance) : Brep[]` — assembles closed solids from open input breps.

[BREP_FILLET_CHAMFER_BLEND]:
- `Rhino.Geometry.Brep.CreateFilletEdges(Brep brep, IEnumerable<int> edgeIndices, IEnumerable<double> startRadii, IEnumerable<double> endRadii, BlendType blendType, RailType railType, bool setbackFillets, double tolerance, double angleTolerance) : Brep[]` — fillets edges under a setback policy; `edgeIndices`, `startRadii`, and `endRadii` are equal-cardinality parallel arrays.
- `Rhino.Geometry.Brep.CreateFilletEdgesVariableRadius(Brep brep, IEnumerable<int> edgeIndices, IDictionary<int, IList<BrepEdgeFilletDistance>> edgeDistances, BlendType blendType, RailType railType, bool setbackFillets, double tolerance, double angleTolerance) : Brep[]` — fillets with a per-edge parameter-to-distance profile map.
- `Rhino.Geometry.Brep.CreateFilletSurface(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, FilletSurfaceSettings settings, out FilletSurfaceResults results) : bool` — fillet, chamfer, or blend between two faces under the settings carrier, projecting the fillet and trimmed inputs through `FilletSurfaceResults`; surface chamfer is the `FilletSurfaceSettings.CreateChamferSettings` profile driving this entry.
- `Rhino.Geometry.Brep.CreateFilletSurfaceCurve(BrepFace face, Point2d uv, Curve curve, double t, FilletSurfaceSettings settings, out FilletSurfaceResults results) : bool` — fillets a face against a curve on it.
- `Rhino.Geometry.Brep.CreateBlendSurface(BrepFace face0, BrepEdge edge0, Interval domain0, bool rev0, BlendContinuity continuity0, BrepFace face1, BrepEdge edge1, Interval domain1, bool rev1, BlendContinuity continuity1) : Brep[]` — surface blend across two edges with per-side continuity.
- `Rhino.Geometry.Brep.CreateBlendShape(BrepFace face0, BrepEdge edge0, double t0, bool rev0, BlendContinuity continuity0, BrepFace face1, BrepEdge edge1, double t1, bool rev1, BlendContinuity continuity1) : Curve` — blend cross-section curve at a station.
- `Rhino.Geometry.Brep.FilletSurfaceSettings.CreateRationalArcSettings(double radius, double tolerance, bool trim, bool extend) : FilletSurfaceSettings` / `CreateNonRationalSettings(double radius, double tolerance, int degree, double tanSlider, double innerSlider, bool trim, bool extend)` / `CreateG2BlendSettings(double radius, double tolerance, bool trim, bool extend)` / `CreateChamferSettings(double radius0, double radius1, double tolerance, bool trim, bool extend)` — four `FilletSurfaceSettings` static factories, one per surface-fillet profile shape.

[BREP_OFFSET_SHELL_PIPE]:
- `Rhino.Geometry.Brep.CreateOffsetBrep(Brep brep, double distance, bool solid, bool extend, bool shrink, double tolerance, out Brep[] outBlends, out Brep[] outWalls) : Brep[]` — offsets a brep and returns blend and wall breps in parallel arrays; a no-`shrink` overload omits the shrink flag.
- `Rhino.Geometry.Brep.CreateFromOffsetFace(BrepFace face, double offsetDistance, double offsetTolerance, bool bothSides, bool createSolid) : Brep` — offsets one face, optionally to a closed solid.
- `Rhino.Geometry.Brep.CreateShell(Brep brep, IEnumerable<int> facesToRemove, double distance, double tolerance) : Brep[]` — hollows a solid, removing the named faces as openings.
- `Rhino.Geometry.Brep.CreatePipe(Curve rail, IEnumerable<double> railRadiiParameters, IEnumerable<double> radii, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians) : Brep[]` — variable-radius pipe keyed by parameter/radius parallel lists; the `(rail, radius, …)` overload is constant-radius.
- `Rhino.Geometry.Brep.CreateThickPipe(Curve rail, IEnumerable<double> railRadiiParameters, IEnumerable<double> radii0, IEnumerable<double> radii1, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians) : Brep[]` — hollow variable-radius pipe with inner and outer radius profiles; the `(rail, radius0, radius1, …)` overload is constant-radius.

[BREP_SWEEP_LOFT_PATCH]:
- `Rhino.Geometry.Brep.CreateFromSweep(Curve rail, IEnumerable<Curve> shapes, Point3d startPoint, Point3d endPoint, SweepFrame frameType, Vector3d roadlikeNormal, bool closed, SweepBlend blendType, SweepMiter miterType, double tolerance, SweepRebuild rebuildType, int rebuildPointCount, double refitTolerance, bool refitRail) : Brep[]` — one-rail sweep with full frame/blend/miter/rebuild control; the `(rail, shape, closed, tolerance)` overload is the minimal form.
- `Rhino.Geometry.Brep.CreateFromSweepSegmented(Curve rail, IEnumerable<Curve> shapes, Point3d startPoint, Point3d endPoint, SweepFrame frameType, Vector3d roadlikeNormal, bool closed, SweepBlend blendType, SweepMiter miterType, double tolerance, SweepRebuild rebuildType, int rebuildPointCount, double refitTolerance) : Brep[]` — one sweep surface per rail segment.
- `Rhino.Geometry.Brep.CreateFromSweep(Curve rail1, Curve rail2, IEnumerable<Curve> shapes, Point3d start, Point3d end, bool closed, double tolerance, SweepRebuild rebuild, int rebuildPointCount, double refitTolerance, bool preserveHeight, bool autoAdjust) : Brep[]` — two-rail sweep; the `CreateFromSweepInParts(Curve, Curve, IEnumerable<Curve>, IEnumerable<Point2d> rail_params, bool, double)` variant keys shapes to rail parameters.
- `Rhino.Geometry.Brep.CreateFromLoft(IEnumerable<Curve> curves, Point3d start, Point3d end, LoftType loftType, bool closed) : Brep[]` — lofts through curves; `CreateFromLoftRebuild(..., int rebuildPointCount)` and `CreateFromLoftRefit(..., double refitTolerance)` add span control, and the `(…, bool StartTangent, bool EndTangent, BrepTrim StartTrim, BrepTrim EndTrim, …)` overload seeds tangent conditions from trims.
- `Rhino.Geometry.Brep.CreatePatch(IEnumerable<GeometryBase> geometry, Surface startingSurface, int uSpans, int vSpans, bool trim, bool tangency, double pointSpacing, double flexibility, double surfacePull, bool[] fixEdges, double tolerance) : Brep` — fits a patch through mixed point/curve/mesh constraints; the `(geometry, startingSurface, tolerance)` and `(geometry, uSpans, vSpans, tolerance)` overloads are the minimal forms.
- `Rhino.Geometry.Brep.CreateVariationalPatch(IEnumerable<CurveConstraint> edges, IEnumerable<CurveConstraint> internalCurves, IEnumerable<PointConstraint> points, VariationalPatchSettings settings, bool multiThreading, CancellationToken cancelToken, IProgress<double> progress, out VariationalPatchResult results) : Brep` — energy-minimizing patch honoring cancellation and progress; the `(RhinoDoc, …)` overload seeds document tolerances.
- `Rhino.Geometry.Brep.CreateDevelopableLoft(NurbsCurve rail0, NurbsCurve rail1, IEnumerable<Point2d> fixedRulings) : Brep[]` — developable loft with pinned rulings; the `(Curve, Curve, bool reverse0, bool reverse1, int density)` overload builds from raw curves.

[BREP_FROM_PRIMITIVES]:
- `Rhino.Geometry.Brep.CreateFromBox(Box box) : Brep` / `CreateFromBox(BoundingBox)` / `CreateFromBox(IEnumerable<Point3d> corners)` — box breps from oriented, axis-aligned, or eight-corner input.
- `Rhino.Geometry.Brep.CreateFromCylinder(Cylinder cylinder, bool capBottom, bool capTop) : Brep` / `CreateFromCone(Cone, bool capBottom)` / `CreateFromTorus(Torus)` / `CreateFromSphere(Sphere)` / `CreateQuadSphere(Sphere)` / `CreateBaseballSphere(Point3d center, double radius, double tolerance)` — analytic-primitive brep set with cap policy where the primitive is open.
- `Rhino.Geometry.Brep.CreateFromRevSurface(RevSurface surface, bool capStart, bool capEnd) : Brep` / `CreateFromSurface(Surface) : Brep` / `CreateFromMesh(Mesh mesh, bool trimmedTriangles) : Brep` — brep from a revolved, general, or mesh source.
- `Rhino.Geometry.Brep.CreateEdgeSurface(IEnumerable<Curve> curves) : Brep` / `CreateFromCornerPoints(Point3d, Point3d, Point3d, Point3d, double tolerance) : Brep` — Coons-style edge or corner surface; the three-point corner overload drops the fourth corner.
- `Rhino.Geometry.Brep.CreatePlanarBreps(IEnumerable<Curve> inputLoops, double tolerance) : Brep[]` — planar faces from closed loops; the `(Curve inputLoop, double tolerance)` overload handles a single loop.
- `Rhino.Geometry.Brep.CreateTrimmedPlane(Plane plane, IEnumerable<Curve> curves) : Brep` / `CreateTrimmedSurface(BrepFace trimSource, Surface surfaceSource, double tolerance) : Brep` — a trimmed plane or a surface trimmed by another face's loops.
- `Rhino.Geometry.Brep.CreateFromTaperedExtrude(Curve curveToExtrude, double distance, Vector3d direction, Point3d basePoint, double draftAngleRadians, ExtrudeCornerType cornerType, double tolerance, double angleToleranceRadians) : Brep[]` — draft-tapered extrude with a corner-relief policy; `CreateFromTaperedExtrudeWithRef(Curve, Vector3d, double, double, Plane, double)` extrudes about a reference plane.

[BREP_JOIN_SPLIT_MERGE_MATCH]:
- `Rhino.Geometry.Brep.JoinBreps(IEnumerable<Brep> brepsToJoin, double tolerance, double angleTolerance, out List<int[]> indexMap) : Brep[]` — joins along coincident edges and maps each result to its source indices; the two-argument overloads drop the map.
- `Rhino.Geometry.Brep.CreateFromJoinedEdges(Brep brep0, int edgeIndex0, Brep brep1, int edgeIndex1, double joinTolerance) : Brep` — joins two breps along one named edge pair.
- `Rhino.Geometry.Brep.SplitDisjointPieces(Brep brep) : Brep[]` — separates connected components; the `(Brep, out List<int[]> indexMap)` overload maps pieces to source faces.
- `Rhino.Geometry.Brep.MergeBreps(IEnumerable<Brep> brepsToMerge, double tolerance) : Brep` — merges into one brep without joining edges; `MergeSurfaces(Brep b0, Brep b1, double tolerance, double angleToleranceRadians, Point2d point0, Point2d point1, double roundness, bool smooth)` merges two single-face breps across a shared edge.
- `Rhino.Geometry.Brep.CreateFromMatch(BrepEdge edge, IEnumerable<Curve> targetCurves, MatchSrfSettings settings, out Brep matched, out Brep target) : bool` — matches a face edge to target curves under continuity settings; the single-curve overload targets one curve.
- `Rhino.Geometry.Brep.ExtendBrepFacesToConnect(BrepFace Face0, int edgeIndex0, BrepFace Face1, int edgeIndex1, double tol, double angleTol, out Brep outBrep0, out Brep outBrep1) : bool` — extends two faces to meet; the `(BrepFace, Point3d, BrepFace, Point3d, …)` overload picks by seed point.
- `Rhino.Geometry.Brep.ChangeSeam(BrepFace face, int direction, double parameter, double tolerance) : Brep` / `CopyTrimCurves(BrepFace trimSource, Surface surfaceSource, double tolerance) : Brep` / `CutUpSurface(Surface surface, IEnumerable<Curve> curves, bool flip, double fitTolerance, double keepTolerance) : Brep[]` — seam relocation, trim-curve transfer, and surface subdivision by curves.
- `Rhino.Geometry.Brep.CreateCurvatureAnalysisMesh(Brep brep, CurvatureAnalysisSettingsState state) : Mesh[]` — false-color curvature-analysis meshes for the brep faces.
- `Rhino.Geometry.Brep.TryConvertBrep(GeometryBase geometry) : Brep` — rebuilds a brep with simpler untrimmed surfaces where its faces admit them.

[BREP_EDIT]:
- `Rhino.Geometry.Brep.CapPlanarHoles(double tolerance) : Brep` — returns a new capped brep; a null result signals no cap was produced.
- `Rhino.Geometry.Brep.JoinNakedEdges(double tolerance) : int` — joins naked edges in place and returns the count joined.
- `Rhino.Geometry.Brep.MergeCoplanarFaces(double tolerance, double angleTolerance) : bool` — merges adjacent coplanar faces in place; the `(faceIndex, …)` and `(faceIndex0, faceIndex1, …)` overloads scope the merge.
- `Rhino.Geometry.Brep.Split(IEnumerable<Brep> cutters, double intersectionTolerance) : Brep[]` — splits by brep, curve, or mixed-geometry cutters; the `(Brep, double, out bool toleranceWasRaised)` overload reports tolerance escalation.
- `Rhino.Geometry.Brep.Trim(Brep cutter, double intersectionTolerance) : Brep[]` / `Trim(Plane cutter, double intersectionTolerance) : Brep[]` — keeps the portion on one side of a brep or plane cutter.
- `Rhino.Geometry.Brep.UnjoinEdges(IEnumerable<int> edgesToUnjoin) : Brep[]` — separates faces along the named shared edges.
- `Rhino.Geometry.Brep.RemoveHoles(IEnumerable<ComponentIndex> loops, double tolerance) : Brep` / `RemoveFins() : bool` / `CullUnusedFaces() : bool` / `Repair(double tolerance) : bool` — hole removal, fin culling, unused-face culling, and in-place repair.

[EXTRUSION]:
- `Rhino.Geometry.Extrusion.Create(Curve planarCurve, double height, bool cap) : Extrusion` — extrudes a planar profile; a non-planar or open profile yields null, and the `(Curve, Plane, double, bool)` overload fixes the profile plane.
- `Rhino.Geometry.Extrusion.CreateBoxExtrusion(Box box, bool cap = true) : Extrusion` / `CreateCylinderExtrusion(Cylinder, bool capBottom, bool capTop) : Extrusion` / `CreatePipeExtrusion(Cylinder, double otherRadius, bool capTop, bool capBottom) : Extrusion` — analytic-primitive extrusions.
- `Rhino.Geometry.Extrusion.SetOuterProfile(Curve outerProfile, bool cap) : bool` / `AddInnerProfile(Curve innerProfile) : bool` / `SetPathAndUp(Point3d a, Point3d b, Vector3d up) : bool` — outer/inner profile authoring and path-frame definition.
- `Rhino.Geometry.Extrusion.ToBrep(bool splitKinkyFaces) : Brep` / `GetWireframe() : Curve[]` / `GetMesh(MeshType meshType) : Mesh` — brep, wireframe, and cached-mesh projections.
- `Rhino.Geometry.Extrusion.Profile3d(int profileIndex, double s) : Curve` / `WallEdge(ComponentIndex ci) : Curve` / `WallSurface(ComponentIndex ci) : Surface` / `GetProfilePlane(double s) : Plane` / `GetPathPlane(double s) : Plane` — station-parameterized profile, wall, and frame reads.

[DEVELOPABLE]:
- `Rhino.Geometry.DevelopableSrf.GetLocalDevopableRuling(NurbsCurve rail0, double t0, Interval dom0, NurbsCurve rail1, double t1, Interval dom1, ref double t0_out, ref double t1_out) : int` — local ruling solve; the member name carries the host's own `Devopable` misspelling.
- `Rhino.Geometry.DevelopableSrf.RulingMinTwist(NurbsCurve rail0, double t0, NurbsCurve rail1, double t1, Interval dom1, ref double t1_out, ref double cos_twist_out) : bool` — minimum-twist ruling; the `(…, Interval dom0, …, ref double t0_out, ref double t1_out, ref double cos_twist_out)` overload solves both parameters.
- `Rhino.Geometry.DevelopableSrf.UntwistRulings(NurbsCurve rail0, NurbsCurve rail1, ref IEnumerable<Point2d> rulings) : bool` — adjusts a ruling set toward developability in place, feeding `Brep.CreateDevelopableLoft`'s fixed-ruling overload.
- `Rhino.Geometry.SurfaceFilletBase` — section-profile factory set `CreateRationalArcsFilletSrf`, `CreateNonRationalCubicArcsFilletSrf`, `CreateNonRationalQuarticArcsFilletSrf`, `CreateNonRationalQuinticArcsFilletSrf`, `CreateNonRationalCubicFilletSrf`, `CreateNonRationalQuarticFilletSrf`, `CreateNonRationalQuinticFilletSrf`, and `CreateG2ChordalQuinticFilletSrf`, each `(BrepFace faceA, Point2d uvA, BrepFace faceB, Point2d uvB, double radius, double tolerance, List<Brep> trimmedBrepsA, List<Brep> trimmedBrepsB, int rail_degree, [double TanSlider, double InnerSlider,] bool bTrim, bool bExtend, List<Brep> Fillets) : bool` filling caller-owned lists; the concrete `SurfaceFillet`/`SurfaceToRailFillet` engines are internal.

## [04]-[IMPLEMENTATION_LAW]

[SOLID_TOPOLOGY]:
- `Brep` static construction returns `null` for a single result or `null`/empty for an array on failure, never throws; boolean unions emit the naked-edge, bad-intersection, and non-manifold point arrays as failure diagnostics, and index-map overloads carry the result-to-source correspondence booleans and joins destroy.
- fillet, chamfer, and blend split by dimensionality: `CreateFilletEdges` over solid edges, `CreateFilletSurface`/`CreateFilletSurfaceCurve` between two faces or a face and a curve under a `FilletSurfaceSettings` carrier, and `CreateBlendSurface`/`CreateBlendShape` spanning two edges under `BlendContinuity`; the four `FilletSurfaceSettings` factories fix the rational-arc, non-rational, G2-blend, or chamfer profile, so surface chamfer is a settings profile.
- `Extrusion` is the profile-and-path lightweight solid: an outer profile, optional inner profiles, and a path frame define it, `ToBrep`/`GetMesh` project it to the heavier representations, and station parameters address profiles, walls, and frames.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a nullable single construction lifts to `Option<Brep>`/`Option<Extrusion>`, a possibly-null-or-empty `Brep[]` lands as `Seq<Brep>`, a `bool`-with-`out` result folds into a `Fin` keyed to the payload, and the boolean diagnostic point arrays and join/split index maps fold into one typed construction receipt on the failure branch; `IProgress<double>`/`CancellationToken` patch builds project onto the effect rail.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): the closed construction vocabularies — `LoftType`, `BlendType`, `BlendContinuity`, `RailType`, `PipeCapMode`, `ExtrudeCornerType`, `FilletSurfaceSplitType`, and the `SweepFrame`/`SweepBlend`/`SweepMiter`/`SweepRebuild` axes — wrap as `[SmartEnum<TKey>]` owners, and the construction op models as a `[Union]` over the boolean, fillet, offset, sweep, loft, patch, primitive, and join/split arms with per-arm parameter carriers.
- `Rasm` kernel: host-neutral robust booleans, offsets, and lofts stand at the kernel altitude and the boundary re-derives none of them; tolerances, radii, angles, and station parameters compose the kernel numeric and unit owners before the native call.

[LOCAL_ADMISSION]:
- construction enters through the op union: each arm binds its native static, projects the `Brep[]`/`Brep`/`bool`+`out` outcome onto the rail, and pairs parallel-array inputs (edge indices with radii, rail parameters with radii, geometry with attributes) into one row sequence proving equal cardinality before the native call.
- native `Brep`, `Extrusion`, and configuration carriers stay inside the construction grant; downstream code receives duplicated canonical geometry keyed by content hash, the typed construction receipt, or an explicitly owned geometry lease, and `FilletSurfaceResults`/`VariationalPatchResult` project into detached result records.

[RAIL_LAW]:
- Surface: `Rhino.Geometry` `Brep`/`Extrusion`/`DevelopableSrf` host-fidelity construction
- Owns: solid and polysurface booleans, edge and surface fillet/chamfer/blend, offset/shell/pipe, sweep/loft/patch, from-primitive and from-curve generation, join/split/merge/match editing, and the extrusion lightweight solid.
- Accept: native construction outcomes projected onto `Fin`/`Option`/`Seq` rails, parallel-array inputs paired into equal-cardinality rows, index maps and diagnostic point arrays folded into typed receipts, and cancellation/progress patch builds on the effect rail.
- Reject: re-deriving kernel-altitude robust construction, exception-style handling of null construction results, unpaired parallel-array inputs, and leaking host `Brep`/`Extrusion`/configuration types past the boundary.
