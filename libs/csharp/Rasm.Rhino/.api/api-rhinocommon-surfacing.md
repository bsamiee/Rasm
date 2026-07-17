# [RASM_RHINO_API_RHINOCOMMON_SURFACING]

This catalog owns the host-fidelity freeform surface and curve construction boundary: `NurbsSurface` network/rail-revolve/through-points/curve-on-surface/ruled/subd-friendly build, the `Surface`/`RevSurface`/`SumSurface`/`PlaneSurface` generation set, and the `Curve` host-op family — offset, offset-on-surface, fair/fit/rebuild/smooth/simplify, extend/trim/split by cutter, pull and project, plus blend/fillet/tween construction and `NurbsCurve` fitting. Every member P/Invokes `rhcommon_c` and returns geometry bit-compatible with Rhino's own commands, standing at the host boundary beside the kernel's host-neutral NURBS algebra in `Rasm/Parametric/curve` and `surface` (evaluate, divide, curvature, tessellate, pullback), which owns a different altitude and is never re-derived here. Curve-surface and curve-brep intersection belongs to `Rasm/Analysis/relations`, iso and contour extraction to `Rasm/Processing/extract`, and native custody to the geometry catalog; this surface projects native `Curve[]`/`Surface`/`bool`+`out` outcomes onto the `LanguageExt` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon surface-and-curve construction surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Geometry`
- kernel: `Rasm` (host-neutral NURBS evaluation and numeric owners composed by altitude, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: surface-construction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: surface owners
- rail: surface-construction

| [INDEX] | [SYMBOL]       | [KIND]          | [CAPABILITY]                                                             |
| :-----: | :------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `NurbsSurface` | native geometry | network, rail-revolve, through-points, curve-on-surface, ruled build     |
|  [02]   | `Surface`      | native base     | extrusion, periodic, soft-edit, rolling-ball fillet, tween-with-sampling |
|  [03]   | `RevSurface`   | native geometry | revolve of a curve or line about an axis with an angle sweep             |
|  [04]   | `SumSurface`   | native geometry | translational-sweep surface from two curves or a curve and direction     |
|  [05]   | `PlaneSurface` | native geometry | bounded plane through a box in a frame                                   |

[PUBLIC_TYPE_SCOPE]: curve owners
- rail: surface-construction

| [INDEX] | [SYMBOL]     | [KIND]          | [CAPABILITY]                                                                |
| :-----: | :----------- | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Curve`      | native base     | offset, fair/fit/rebuild/smooth/simplify, extend/trim/split, pull, project  |
|  [02]   | `NurbsCurve` | native geometry | fit-point, spline, spiral, parabola, arc-bezier, subd-friendly construction |

[ENUM_ROSTERS]:
- `public enum Rhino.Geometry.CurveOffsetCornerStyle` — `None`, `Sharp`, `Round`, `Smooth`, `Chamfer`.
- `public enum Rhino.Geometry.CurveOffsetEndStyle` — `None`, `Flat`, `Round`.
- `public enum Rhino.Geometry.CurveExtensionStyle` — `Line`, `Arc`, `Smooth`.
- `public enum Rhino.Geometry.CurveEnd` — `None = 0`, `Start = 1`, `End = 2`, `Both = 3`.
- `public enum Rhino.Geometry.SmoothingCoordinateSystem` — `World`, `CPlane`, `Object`.
- `[Flags] public enum Rhino.Geometry.CurveSimplifyOptions` — `None = 0`, `SplitAtFullyMultipleKnots = 1`, `RebuildLines = 2`, `RebuildArcs = 4`, `RebuildRationals = 8`, `AdjustG1 = 0x10`, `Merge = 0x20`, `All = 0x3F`.

## [03]-[ENTRYPOINTS]

[NURBSSURFACE_BUILD]:
- `Rhino.Geometry.NurbsSurface.CreateNetworkSurface(IEnumerable<Curve> uCurves, int uContinuityStart, int uContinuityEnd, IEnumerable<Curve> vCurves, int vContinuityStart, int vContinuityEnd, double edgeTolerance, double interiorTolerance, double angleTolerance, out int error) : NurbsSurface` — fits a surface through a u/v curve network with per-boundary continuity; the `out int error` carries the failure code, and the `(curves, continuity, …)` overload auto-sorts one curve set.
- `Rhino.Geometry.NurbsSurface.CreateRailRevolvedSurface(Curve profile, Curve rail, Line axis, bool scaleHeight) : NurbsSurface` — revolves a profile along a rail about an axis.
- `Rhino.Geometry.NurbsSurface.CreateFromPoints(IEnumerable<Point3d> points, int uCount, int vCount, int uDegree, int vDegree) : NurbsSurface` — interpolates a control-point grid; `CreateThroughPoints(points, uCount, vCount, uDegree, vDegree, bool uClosed, bool vClosed)` fits through the points with closure flags.
- `Rhino.Geometry.NurbsSurface.CreateFromCorners(Point3d corner1, Point3d corner2, Point3d corner3, Point3d corner4, double tolerance) : NurbsSurface` — four-corner bilinear surface; the three-corner overload builds a triangle.
- `Rhino.Geometry.NurbsSurface.CreateRuledSurface(Curve curveA, Curve curveB) : NurbsSurface` — a ruled surface between two curves.
- `Rhino.Geometry.NurbsSurface.CreateCurveOnSurface(Surface surface, IEnumerable<Point2d> points, double tolerance, bool periodic) : NurbsCurve` — a geodesic-fit curve through surface uv points; `CreateCurveOnSurfacePoints(surface, fixedPoints, tolerance, periodic, initCount, levels) : Point2d[]` returns the intermediate uv samples.
- `Rhino.Geometry.NurbsSurface.CreateSubDFriendly(Surface surface) : NurbsSurface` — rebuilds a surface as a subd-compatible bicubic form.
- `Rhino.Geometry.NurbsSurface.CreateFromPlane(Plane plane, Interval uInterval, Interval vInterval, int uDegree, int vDegree, int uPointCount, int vPointCount) : NurbsSurface` / `CreateFromCone(Cone)` / `CreateFromCylinder(Cylinder)` / `CreateFromSphere(Sphere)` / `CreateFromTorus(Torus)` — nurbs forms of the analytic primitives.
- `Rhino.Geometry.NurbsSurface.MakeCompatible(Surface surface0, Surface surface1, out NurbsSurface nurb0, out NurbsSurface nurb1) : bool` — reparameterizes two surfaces to a shared knot and degree structure.
- `Rhino.Geometry.NurbsSurface.MatchToCurve(IsoStatus side, Curve targetCurve, double maxEndDistance, double maxInteriorDistance, double matchTolerance, int maxLevel) : NurbsSurface` — matches one iso edge to a target curve.

[SURFACE_BUILD]:
- `Rhino.Geometry.Surface.CreateExtrusion(Curve profile, Vector3d direction) : Surface` / `CreateExtrusionToPoint(Curve profile, Point3d apexPoint) : Surface` — linear and tapered-to-apex extrusion surfaces.
- `Rhino.Geometry.Surface.CreatePeriodicSurface(Surface surface, int direction, bool bSmooth) : Surface` — closes a surface periodically in one direction.
- `Rhino.Geometry.Surface.CreateSoftEditSurface(Surface surface, Point2d uv, Vector3d delta, double uLength, double vLength, double tolerance, bool fixEnds) : Surface` — falloff-weighted local push of a surface point.
- `Rhino.Geometry.Surface.CreateRollingBallFillet(Surface surfaceA, bool flipA, Surface surfaceB, bool flipB, double radius, double tolerance) : Surface[]` — rolling-ball fillet between two surfaces; the `(surfaceA, uvA, surfaceB, uvB, …)` overload seeds by uv and the `(surfaceA, surfaceB, …)` overload picks sides automatically.
- `Rhino.Geometry.Surface.CreateTweenSurfacesWithSampling(Surface surface0, Surface surface1, int numSurfaces, int numSamples, double tolerance) : Surface[]` — intermediate surfaces sampled between two inputs.
- `Rhino.Geometry.RevSurface.Create(Curve revoluteCurve, Line axisOfRevolution, double startAngleRadians, double endAngleRadians) : RevSurface` — revolves a profile curve through an angle sweep; the line-profile and no-angle overloads cover the degenerate and full-revolution cases, and `CreateFromCone(Cone)` / `CreateFromCylinder(Cylinder)` / `CreateFromSphere(Sphere)` / `CreateFromTorus(Torus)` build the analytic revolutions.
- `Rhino.Geometry.SumSurface.Create(Curve curve, Vector3d extrusionDirection) : SumSurface` — translational-sweep surface from a curve and a direction; the `(Curve curveA, Curve curveB)` overload sums two curves into the translational surface.
- `Rhino.Geometry.Surface.Fit(int uDegree, int vDegree, double fitTolerance) : Surface` / `Rebuild(int uDegree, int vDegree, int uPointCount, int vPointCount) : NurbsSurface` / `RebuildOneDirection(int direction, int pointCount, LoftType loftType, double refitTolerance) : NurbsSurface` — the instance fit-and-rebuild family.
- `Rhino.Geometry.Surface.VariableOffset(double uMinvMin, double uMinvMax, double uMaxvMin, double uMaxvMax, double tolerance) : Surface` / `VariableOffset(double uMinvMin, double uMinvMax, double uMaxvMin, double uMaxvMax, IEnumerable<Point2d> interiorParameters, IEnumerable<double> interiorDistances, double tolerance) : Surface` — corner-distance offset construction, null on failure; the interior overload refuses mismatched parameter/distance counts.
- `Rhino.Geometry.PlaneSurface.CreateThroughBox(Plane plane, BoundingBox box) : PlaneSurface` / `CreateThroughBox(Line lineInPlane, Vector3d vectorInPlane, BoundingBox box) : PlaneSurface` — a bounded plane sized to span a box, framed by a plane or a line and in-plane vector.

[CURVE_OFFSET_PULL]:
- `Rhino.Geometry.Curve.Offset(Point3d directionPoint, Vector3d normal, double distance, double tolerance, double angleTolerance, bool loose, CurveOffsetCornerStyle cornerStyle, CurveOffsetEndStyle endStyle) : Curve[]` — planar offset with corner and end policy and a loose flag; the `(Plane, distance, tolerance, cornerStyle)` overload offsets in a plane.
- `Rhino.Geometry.Curve.OffsetOnSurface(BrepFace face, double[] curveParameters, double[] offsetDistances, double fittingTolerance) : Curve[]` — variable-distance offset across a face keyed by parameter/distance parallel arrays; the constant-distance and through-point overloads accept `Surface` or `BrepFace`.
- `Rhino.Geometry.Curve.OffsetNormalToSurface(Surface surface, double height) : Curve` / `OffsetTangentToSurface(Surface surface, double height) : Curve` — lifts a surface-lying curve normal or tangent to the surface.
- `Rhino.Geometry.Curve.PullToBrepFace(BrepFace face, double tolerance) : Curve[]` — projects a curve onto a face along its normals; `Curve.PullToBrepFace(Curve curve, BrepFace face, double tolerance, bool loose) : Curve[]` is the static loose-fit form.
- `Rhino.Geometry.Curve.PullToMesh(Mesh mesh, double tolerance) : PolylineCurve` — pulls a curve onto a mesh as a polyline along the mesh surface; the `(Mesh, tolerance, bool loose) : Curve` overload returns a loose-fit smooth curve.

[CURVE_REFINE]:
- `Rhino.Geometry.Curve.Fair(double distanceTolerance, double angleTolerance, int clampStart, int clampEnd, int iterations) : Curve` — smooths within a deviation budget under clamped ends.
- `Rhino.Geometry.Curve.Fit(int degree, double fitTolerance, double angleTolerance) : Curve` — refits to a lower knot count within tolerance.
- `Rhino.Geometry.Curve.Rebuild(int pointCount, int degree, bool preserveTangents) : NurbsCurve` — rebuilds to a fixed control-point count and degree.
- `Rhino.Geometry.Curve.Smooth(double smoothFactor, bool bXSmooth, bool bYSmooth, bool bZSmooth, bool bFixBoundaries, SmoothingCoordinateSystem coordinateSystem, Plane plane) : Curve` — per-axis smoothing in a coordinate system; the no-plane overload uses world or cplane.
- `Rhino.Geometry.Curve.Simplify(CurveSimplifyOptions options, double distanceTolerance, double angleToleranceRadians) : Curve` — replaces spans with lines/arcs per the option flags; `SimplifyEnd(CurveEnd end, …)` scopes it to one end.
- `Rhino.Geometry.Curve.RemoveShortSegments(double tolerance) : bool` / `MakeClosed(double tolerance) : bool` — in-place short-segment removal and gap closure.

[CURVE_EXTEND_TRIM_SPLIT]:
- `Rhino.Geometry.Curve.Extend(CurveEnd side, double length, CurveExtensionStyle style) : Curve` — extends one end by length with a line, arc, or smooth style; the `(CurveEnd, CurveExtensionStyle, IEnumerable<GeometryBase>)` and `(…, Point3d endPoint)` overloads extend to geometry or a point.
- `Rhino.Geometry.Curve.ExtendByLine(CurveEnd side, IEnumerable<GeometryBase> geometry) : Curve` / `ExtendByArc(CurveEnd side, IEnumerable<GeometryBase> geometry) : Curve` / `ExtendOnSurface(CurveEnd side, BrepFace face) : Curve` / `ExtendOnSurface(CurveEnd side, Surface surface) : Curve` — line, arc, and on-surface extension to bounding geometry; the on-surface pair extends across a trimmed `BrepFace` or an untrimmed `Surface`, `null` on failure.
- `Rhino.Geometry.Curve.Trim(Interval domain) : Curve` / `Trim(CurveEnd side, double length) : Curve` / `TrimInterval(Interval domain) : bool` — sub-domain extraction and in-place interval trim.
- `Rhino.Geometry.Curve.Split(Brep cutter, double tolerance, double angleToleranceRadians) : Curve[]` — splits at intersections with a brep, surface, plane, or parameter set; the `(double t)` and `(IEnumerable<double> t)` overloads split at parameters.

[CURVE_CONSTRUCT]:
- `Rhino.Geometry.Curve.CreateBlendCurve(Curve curve0, double t0, bool reverse0, BlendContinuity continuity0, Curve curve1, double t1, bool reverse1, BlendContinuity continuity1) : Curve` — a blend between two curves at stations with per-end continuity; the `(curveA, curveB, continuity, bulgeA, bulgeB)` overload blends end-to-end with bulge control, and `CreateArcBlend(Point3d startPt, Vector3d startDir, Point3d endPt, Vector3d endDir, double controlPointLengthRatio) : Curve` / `CreateArcLineArcBlend(Point3d, Vector3d, Point3d, Vector3d, double radius) : Curve` are the direction-seeded arc-blend forms.
- `Rhino.Geometry.Curve.CreateMatchCurve(Curve curve0, bool reverse0, BlendContinuity continuity, Curve curve1, bool reverse1, PreserveEnd preserve, bool average) : Curve[]` — matches one curve's end to another under continuity and end-preservation policy; `CreateMeanCurve(Curve curveA, Curve curveB, double angleToleranceRadians) : Curve` averages two curves into a mean curve.
- `Rhino.Geometry.Curve.CreateFilletCurves(Curve curve0, Point3d point0, Curve curve1, Point3d point1, double radius, bool join, bool trim, bool arcExtension, double tolerance, double angleTolerance) : Curve[]` — a fillet arc between two curves picked near seed points; `CreateFillet(Curve, Curve, double radius, double t0Base, double t1Base) : Arc` returns the raw arc and `CreateFilletCornersCurve(Curve, double radius, double tolerance, double angleTolerance) : Curve` fillets every corner of one curve.
- `Rhino.Geometry.Curve.CreateTweenCurves(Curve curve0, Curve curve1, int numCurves, double tolerance) : Curve[]` — intermediate curves between two inputs; `CreateTweenCurvesWithMatching(...)` reparameterizes first and `CreateTweenCurvesWithSampling(..., int numSamples, ...)` samples for dissimilar inputs.
- `Rhino.Geometry.Curve.ProjectToBrep(IEnumerable<Curve> curves, IEnumerable<Brep> breps, Vector3d direction, double tolerance, out int[] curveIndices, out int[] brepIndices) : Curve[]` — projects curves onto breps along a direction with source-index maps; the single-curve/single-brep overloads drop the maps.
- `Rhino.Geometry.Curve.ProjectToMesh(IEnumerable<Curve> curves, IEnumerable<Mesh> meshes, Vector3d direction, double tolerance) : Curve[]` / `ProjectToPlane(Curve curve, Plane plane) : Curve` — projection onto meshes and onto a plane.
- `Rhino.Geometry.Curve.MakeEndsMeet(Curve curveA, bool adjustStartCurveA, Curve curveB, bool adjustStartCurveB) : bool` / `CreatePeriodicCurve(Curve curve, bool smooth) : Curve` — endpoint reconciliation and periodic closure.
- `Rhino.Geometry.Curve.FilletSurfaceToRail(BrepFace faceWithCurve, BrepFace secondFace, double u1, double v1, int railDegree, int arcDegree, IEnumerable<double> arcSliders, int numBezierSrfs, bool extend, FilletSurfaceSplitType split_type, double tolerance, List<Brep> out_fillets, List<Brep> out_breps0, List<Brep> out_breps1, out double[] fitResults) : bool` — an INSTANCE method on the rail `Curve` receiver whose fillet, trimmed-input, and fit outputs land in the caller-owned lists.

[NURBSCURVE_FIT]:
- `Rhino.Geometry.NurbsCurve.CreateFromFitPoints(IEnumerable<Point3d> points, double tolerance, int degree, bool periodic, Vector3d startTangent, Vector3d endTangent) : NurbsCurve` — fits through points within tolerance under tangent constraints; the `(points, tolerance, periodic)` overload drops the tangents.
- `Rhino.Geometry.NurbsCurve.CreateHSpline(IEnumerable<Point3d> points, Vector3d startTangent, Vector3d endTangent) : NurbsCurve` — an interpolating H-spline; the no-tangent overload free-ends.
- `Rhino.Geometry.NurbsCurve.CreateSpiral(Curve railCurve, double t0, double t1, Point3d radiusPoint, double pitch, double turnCount, double radius0, double radius1, int pointsPerTurn) : NurbsCurve` — a spiral along a rail; the `(Point3d axisStart, Vector3d axisDir, …)` overload spirals about an axis.
- `Rhino.Geometry.NurbsCurve.CreateParabolaFromVertex(Point3d vertex, Point3d startPoint, Point3d endPoint) : NurbsCurve` / `CreateNonRationalArcBezier(int degree, Point3d center, Point3d start, Point3d end, double radius, double tanSlider, double midSlider) : NurbsCurve` — parabola and non-rational arc-bezier construction.
- `Rhino.Geometry.NurbsCurve.CreateSubDFriendly(Curve curve) : NurbsCurve` / `CreateSubDFriendly(Curve curve, int pointCount, bool periodicClosedCurve) : NurbsCurve` — subd-compatible rebuild; the bare-curve form picks the point count and open/closed structure from the source, the structured form fixes control-point count and periodic closure (`pointCount >= 6` when periodic, `>= 4` otherwise), and the `(IEnumerable<Point3d>, bool interpolatePoints, bool periodicClosedCurve)` overload builds from points.
- `Rhino.Geometry.NurbsCurve.CreateFromArc(Arc arc, int degree, int cvCount) : NurbsCurve` / `CreateFromLine(Line line) : NurbsCurve` — analytic-to-nurbs conversion with control over span structure.
- `Rhino.Geometry.NurbsCurve.MakeCompatible(IEnumerable<Curve> curves, Point3d startPt, Point3d endPt, int simplifyMethod, int numPoints, double refitTolerance, double angleTolerance) : NurbsCurve[]` — reparameterizes a set to a shared structure for lofting.
- `Rhino.Geometry.NurbsCurve.CreateParabolaFromFocus(Point3d focus, Point3d startPoint, Point3d endPoint) : NurbsCurve` / `CreateParabolaFromPoints(Point3d startPoint, Point3d innerPoint, Point3d endPoint) : NurbsCurve` — the focus-seeded and three-point parabola siblings.
- `Rhino.Geometry.NurbsCurve.CreateFromCircle(Circle circle[, int degree, int cvCount]) : NurbsCurve` / `CreateFromEllipse(Ellipse ellipse) : NurbsCurve` / `Create(bool periodic, int degree, IEnumerable<Point3d> points) : NurbsCurve` — the remaining analytic and control-point constructors.

[CURVE_STATICS_AND_CARRIERS]:
- `Rhino.Geometry.Curve.CreateInterpolatedCurve(IEnumerable<Point3d> points, int degree[, CurveKnotStyle knots[, Vector3d startTangent, Vector3d endTangent]]) : Curve` / `CreateControlPointCurve(IEnumerable<Point3d> points[, int degree]) : Curve` — interpolation and control-point fitting are `Curve` statics, never `NurbsCurve` members; `public enum CurveKnotStyle` — `Uniform`, `Chord`, `ChordSquareRoot`, `UniformPeriodic`, `ChordPeriodic`, `ChordSquareRootPeriodic`.
- `Rhino.Geometry.Curve.CreateSoftEditCurve(Curve curve, double t, Vector3d delta, double length, bool fixEnds) : Curve` — falloff-weighted local push of a curve point.
- `Rhino.Geometry.Curve.JoinCurves(IEnumerable<Curve> inputCurves[, double joinTolerance[, bool preserveDirection[, bool simpleJoin], out int[] key]]) : Curve[]` — joining with an optional result-to-source key map.
- `Rhino.Geometry.Curve.CreateBooleanUnion(IEnumerable<Curve> curves, double tolerance[, out int[] indexMap]) : Curve[]` / `CreateBooleanIntersection(Curve curveA, Curve curveB, double tolerance) : Curve[]` / `CreateBooleanDifference(Curve curveA, Curve curveB, double tolerance) : Curve[]` / `CreateBooleanDifference(Curve curveA, IEnumerable<Curve> subtractors, double tolerance) : Curve[]` — planar curve booleans; every tolerance-less form is `[Obsolete]`.
- `Rhino.Geometry.Curve.CreateBooleanRegions(IEnumerable<Curve> curves, Plane plane[, IEnumerable<Point3d> points], bool combineRegions, double tolerance) : CurveBooleanRegions` — yields the disposable region carrier: `RegionCount`/`PointCount`/`PlanarCurveCount : int`, `RegionCurves(int regionIndex) : Curve[]`, `RegionPointIndex(int pointIndex) : int`, `BoundaryCount(int)`/`SegmentCount(int, int)`/`SegmentDetails(int, int, int, out Interval subDomain, out bool reversed) : int`, `PlanarCurve(int) : Curve`.
- `Rhino.Geometry.Curve.CreateCatenaryCurveThroughPoint / CreateCatenaryCurveFromLength / CreateCatenaryCurveFromParameter / CreateCatenaryCurveFromApex(Point3d catenary_start, Point3d catenary_end, Vector3d axis_dir, <shape argument>, bool bSmooth, int point_count, out Point3d apex_out, out double parameter_out, out double length_out, out double max_deviation_out) : Curve` — mints the hanging-curve family, one static per shape terminal with apex, parameter, length, and deviation evidence.
- `Rhino.Geometry.Curve.CreateCurve2View(Curve curveA, Curve curveB, Vector3d vectorA, Vector3d vectorB, double tolerance, double angleTolerance) : Curve[]` / `CreateTextOutlines(string text, string font, double textHeight, int textStyle, bool closeLoops, Plane plane, double smallCapsScale, double tolerance) : Curve[]` — two-view intersection construction and glyph outlines.
- `Rhino.Geometry.Curve.DoDirectionsMatch(Curve curveA, Curve curveB) : bool` — static direction-agreement predicate; true when both curves travel more or less the same way.
- `Rhino.Geometry.Curve.RibbonOffset(RibbonOffsetParameters ribbonParameters, out Curve[] railCurves, out Curve[] crossSectionCurves, out Brep[] brepSurfaces) : Curve` — runs the carrier-driven ribbon offset; scalar overloads `(distance, blendRadius, directionPoint, normal, tolerance[, out Curve[] crossSections, out Surface[] ruledSurfaces | out double[] outputParameters, out double[] curveParameters])` predate it.
- `Rhino.Geometry.RibbonOffsetParameters` — all `get/set`: `OffsetDistance : double`, `OffsetLocation : Point3d`, `OffsetTolerance : double`, `OffsetPlaneVector3d : Vector3d` (default `Vector3d.Unset`), `BlendRadius : double`, `RebuildPointCount : int` (0 disables), `RefitTolerance : double` (0 disables), `AlignCrossSections : bool`, `RibbonSurfaceGenerationMethod : RibbonOffsetSurfaceMethod`; `public enum RibbonOffsetSurfaceMethod` — `None = 0`, `Sweep2 = 1`, `Sweep2NetworkSrf = 2`.
- `Rhino.Geometry.Curve.CreateNurbsCurveFit(Curve curve, Interval domain, NurbsCurveFitParameters rebuildOptions, out Line maximumSeparation, out double thisSeparationParameter, out double nurbsSeparationParameter) : NurbsCurve` — runs the advanced fit entry over its carrier.
- `Rhino.Geometry.NurbsCurveFitParameters` (`ICloneable`, `IDisposable`) — `get/set`: `TangentMatching : TangentMatch`, `Degree`/`PointCount : int`, `SubDFriendly : bool`, `KinkAngleRadians`/`KinkAngleDegrees : double`, `KinkSplitting : KinkSplit`, `SmoothingIntensity`/`UniformityIntensity`/`CurvatureBiasIntensity : Intensity`, `SmoothingCoefficient`/`UniformityCoefficient`/`CurvatureBiasCoefficient : double`, `Closed`/`ApplyTangentMatchingAtKinks`/`OptimizeCurve : bool`, `PointCountRange : IndexPair`; nested `byte` enums `TangentMatch` (`None`, `AtStart`, `AtEnd`, `AtStartAndEnd`), `KinkSplit` (`None`, `AtG1Changes`, `AtLargeG2Changes`, `AtMediumG2Changes`, `AtSmallG2Changes`), `Intensity` (`None`, `Low`, `Moderate`, `Medium`, `High`, `Extreme`, `Custom`).
- `public enum Rhino.Geometry.PreserveEnd` — `None = 0`, `Position = 1`, `Tangency = 2`, `Curvature = 3` — the `CreateMatchCurve` end-preservation vocabulary.
- `Rhino.Geometry.Curve.Split(Plane plane, double tolerance, double angleToleranceRadians) : Curve[]` — runs the plane-cutter split beside the brep and surface forms; `Rebuild(int pointCount, int degree, bool preserveTangents) : NurbsCurve` and `Fair(double distanceTolerance, double angleTolerance, int clampStart, int clampEnd, int iterations) : Curve` take numerics only — no `RebuildCurveOptions` or `CurveKinkDefinition` carrier exists.

## [04]-[IMPLEMENTATION_LAW]

[SURFACE_TOPOLOGY]:
- `NurbsSurface` construction returns `null` on failure and carries diagnostics through an `out int error` for the network and curve-on-surface builds; `MakeCompatible` and `MatchToCurve` reparameterize to shared or matched structure, and analytic-primitive nurbs forms feed the loft and network builders a compatible representation.
- `Curve` host ops split by intent: `Offset`/`OffsetOnSurface`/`OffsetNormalToSurface` produce parallel geometry, `Fair`/`Fit`/`Rebuild`/`Smooth`/`Simplify` refine in place or return a refined copy, `Extend`/`Trim`/`Split` change extent by parameter or bounding geometry, and `PullToBrepFace`/`ProjectToBrep`/`ProjectToMesh` map a curve onto a target; variable-distance offset and multi-input projection carry parameter/distance and source-index parallel arrays.
- freeform surface generation stays host-fidelity: `CreateNetworkSurface`, `CreateRailRevolvedSurface`, `CreateSoftEditSurface`, and `CreateRollingBallFillet` produce Rhino-command-identical geometry, distinct from the kernel's host-neutral surface algebra that owns evaluation and division at a lower altitude.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a nullable single build lifts to `Option<NurbsSurface>`/`Option<Curve>`, a possibly-null-or-empty `Curve[]`/`Surface[]` lands as `Seq<A>`, a `bool`-with-`out` op folds into a `Fin` keyed to the payload, the `out int error` network code and the projection source-index maps fold into a typed construction receipt, and the rolling-ball and tween arrays project as `Seq<Surface>`.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): the closed policy vocabularies — `CurveOffsetCornerStyle`, `CurveOffsetEndStyle`, `CurveExtensionStyle`, `CurveEnd`, `SmoothingCoordinateSystem`, and the `[Flags]` `CurveSimplifyOptions` — wrap as `[SmartEnum<TKey>]`/`[Flags]`-backed owners; the curve host op models as a `[Union]` over the offset, refine, extend/trim/split, pull/project, and construct arms.
- `Rasm` kernel: host-neutral NURBS evaluation, division, curvature, and tessellation stand at the kernel altitude and the boundary re-derives none of them; degrees, tolerances, angles, and station parameters compose the kernel numeric owners before the native call.

[LOCAL_ADMISSION]:
- construction enters through the surface or curve op union: each arm binds its native member, projects the outcome onto the rail, and pairs parameter/distance or curve/brep parallel arrays into equal-cardinality rows before the native call; the caller-owned fillet output lists for `FilletSurfaceToRail` drain into detached brep records.
- native `Surface`, `NurbsSurface`, `Curve`, and `NurbsCurve` values stay inside the construction grant; downstream code receives duplicated canonical geometry keyed by content hash, the typed construction receipt, or an explicitly owned geometry lease.

[RAIL_LAW]:
- Surface: `Rhino.Geometry` freeform surface and curve host-fidelity construction
- Owns: nurbs-surface network/rail-revolve/through-points/curve-on-surface/ruled build, the surface/rev/sum/plane generation set, and the curve offset, refine, extend/trim/split, pull/project, and blend/fillet/tween/fit host ops.
- Accept: native surface and curve outcomes projected onto `Fin`/`Option`/`Seq` rails, parameter/distance and source-index parallel arrays paired into rows, `out`-error and index maps folded into typed receipts.
- Reject: re-deriving kernel-altitude NURBS evaluation and division, exception-style handling of null construction results, unpaired parallel-array inputs, and leaking host surface/curve types past the boundary.
