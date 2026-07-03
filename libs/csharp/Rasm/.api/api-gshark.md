# [RASM_API_GSHARK]

`GShark` is the pure-managed C# NURBS engine the kernel composes as its host-neutral
parametric-geometry contract: a non-Rhino runtime evaluates and edits curves/surfaces with
no RhinoCommon, retiring the in-house NURBS-Book hand-roll. The consumed surface is NOT the
`GShark.Sampling`/`GShark.Analyze`/`GShark.Modify`/`GShark.Evaluate` static namespaces — those
are almost entirely `internal static` algorithm kernels (`Sampling.Curve.ByCount`/`ByLength`/
`RegularSample`, `Modify.Curve.KnotRefine`/`DecomposeIntoBeziers`, `Analyze.Curve.Length`/
`Curvature`/`BezierLength` are all `internal`) — the public parametric API is the rich
INSTANCE surface on `NurbsBase` (the shared curve algebra: `PointAt`/`TangentAt`/`DerivativeAt`/
`CurvatureAt`/`PerpendicularFrameAt`, `ClosestPoint`/`ClosestParameter`, `Length`/`LengthAt`/
`ParameterAtLength`, `Divide`/`SplitAt`/`SubCurve`, `ElevateDegree`/`ReduceDegree`/
`DecomposeIntoBeziers`, `Offset`/`Reverse`/`Close`/`Join`) plus the `NurbsCurve`/`NurbsSurface`
concrete types, the surface factory family (`FromCorners`/`FromPoints`/`FromLoft`/`FromExtrusion`/
`FromSweep`/`Ruled`/`Revolved`), and the genuinely-public static operation classes that DON'T
fit an instance — `GShark.Fitting.Curve` (`Approximate`/`Interpolated`/`InterpolateBezier`),
`GShark.Intersection.Intersect` (the typed curve/line/plane/circle intersection family), and
`GShark.Evaluate.Evaluate` (`RationalDerivatives`/`OneBasisFunction`). The engine carries its
OWN value vocabulary — `Point3`/`Point4`/`Vector3`/`Plane`/`Line`/`Arc`/`Circle`/`PolyLine`/
`Polygon`/`PolyCurve`/`Ray`/`BoundingBox`/`Mesh` — so the kernel maps `Rasm.Vectors` ↔ GShark
`Point3`/`Vector3` AT THE BOUNDARY and keeps canonical names internal; GShark is the parametric
EVALUATION contract, never the kernel's primitive vocabulary. It is `netstandard2.0`,
zero-dependency, AnyCPU, pure-managed — the host-neutral curve/surface owner that runs on
osx-arm64 with no native gate.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `GShark`
- package: `GShark`
- version: `2.3.1`
- license: MIT (GShark / Cesar Pinto et al.; `github.com/GSharkDev/GShark`; nuspec `<license type="expression">MIT</license>`)
- assembly: `GShark.dll` (ships `GShark.xml` IDE doc companion)
- namespace: `GShark.Geometry` (the value vocabulary + `NurbsBase`/`NurbsCurve`/`NurbsSurface`), `GShark.Fitting`, `GShark.Intersection`, `GShark.Evaluate`, `GShark.Enumerations`, `GShark.Optimization`, `GShark.Interfaces`, `GShark.Core` (`GSharkMath`/`Interval`/`KnotVector`/`Matrix`/`Transform`/`TransformMatrix`/`Trigonometry`)
- target: single `lib/netstandard2.0` only — NO multi-target fallback ambiguity; the `net10.0` consumer binds the one shipped `netstandard2.0` asset forward
- asset: pure-managed runtime library, AnyCPU, NO native runtime and ZERO package dependencies; double-precision throughout (`Point3`/`Vector3` are `double`-backed)
- abi: `netstandard2.0` so NO `System.Numerics` generic-math, NO nullable-reference annotation enforcement, NO `Span`-first kernels — the curve/surface API is `List<Point3>`/`List<double>`-shaped object models, not span/`INumber<T>` generic math
- rail: host-neutral NURBS curve/surface evaluation + edit + fit + intersect

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the geometry vocabulary + the parametric algebra (`GShark.Geometry`)
- rail: parametric evaluation

The vocabulary types `Point3`/`Point4`/`Vector3`/`Vector` are GShark's OWN value model
(distinct from `Rasm.Vectors`); `NurbsBase` is the abstract curve algebra every concrete curve
(`NurbsCurve`/`Line`/`Arc`/`Circle`/`PolyLine`/`PolyCurve`/`Polygon`) derives, so a polymorphic
consumer evaluates any curve through one `NurbsBase` reference. `NurbsSurface` is the standalone
surface. The internal mesh half-edge family (`Mesh`/`MeshVertex`/`MeshEdge`/`MeshFace`/
`MeshHalfEdge`/`MeshTopology`/`MeshCorner`/`MeshGeometry`) is a secondary discrete-geometry
surface the kernel's own meshing owner supersedes.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]                       | [CAPABILITY]                                                                                          |
| :-----: | :----------------------------- | :----------------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `NurbsBase` (abstract)         | the shared curve algebra             | `Degree`/`Knots`/`Weights`/`ControlPoints`/`ControlPointLocations`; the whole `PointAt`/`TangentAt`/`ClosestPoint`/`Length`/`Divide`/`SplitAt`/`ElevateDegree`/`Offset` instance surface; base of every concrete curve |
|  [02]   | `NurbsCurve : NurbsBase`       | concrete rational B-spline curve     | `new NurbsCurve(List<Point3> points, int degree)` / `(points, List<double> weights, degree)`; `Transform(TransformMatrix)` |
|  [03]   | `NurbsSurface`                 | rational B-spline surface            | `DegreeU`/`DegreeV`/`KnotsU`/`KnotsV`/`ControlPoints`; the `From*`/`Ruled`/`Revolved` factory family; `PointAt(u,v)`/`EvaluateAt`/`IsoCurve`/`SplitAt`/`ClosestPoint` |
|  [04]   | `Point3`                       | 3D point value                       | `class : IEquatable/IComparable`; full operator set, `DistanceTo`/`Interpolate`/`Centroid`/`EpsilonEquals`; `Origin`/`Unset`; implicit → `Point4`/`Vector3`/`Vector` |
|  [05]   | `Point4`                       | homogeneous (weighted) point         | `readonly struct`; the rational control-point representation (`x,y,z,w`) |
|  [06]   | `Vector3`                      | 3D vector value                      | `readonly struct : IEquatable/IComparable`; `Zero`/`XAxis`/`YAxis`/`ZAxis`/`Unset` + the vector operator algebra |
|  [07]   | `Plane` / `Line` / `Ray`       | frame + linear primitives            | `Plane` (origin+axes frame), `Line`/`Ray` (segment / infinite ray) — `Line : NurbsBase` so a line evaluates as a degree-1 curve |
|  [08]   | `Arc` / `Circle`               | conic primitives (rational)          | `Arc : Circle : NurbsBase` — both evaluate as exact rational NURBS curves |
|  [09]   | `PolyLine` / `Polygon` / `PolyCurve` | piecewise primitives           | `PolyLine : NurbsBase`, `Polygon : PolyLine`, `PolyCurve : NurbsBase` — joined multi-segment curve evaluable as one `NurbsBase` |
|  [10]   | `BoundingBox`                  | axis-aligned bounds                  | AABB with min/max + containment/intersection; `NurbsBase.GetBoundingBox()` returns it |
|  [11]   | `GShark.Geometry.ConvexHull`   | engine-local 3D incremental hull     | `GenerateHull(List<Point3>, bool splitVerts, ref List<Point3> verts, ref List<int> tris, ref List<Vector3> normals)` — low-level mutable out-param hull, distinct from `MIConvexHull` (`api-miconvexhull`) typed-result `Create` |
|  [12]   | `Mesh` (+ half-edge family)    | discrete half-edge mesh              | `Mesh(List<Point3>, List<List<int>> faces)`; `EulerCharacteristic`/`GetArea`/`MeshTopology` — secondary surface the kernel meshing owner supersedes |

## [03]-[CURVE_ALGEBRA]

[CURVE_SCOPE]: `NurbsBase` instance evaluation + measure (the public parametric API)
- rail: parametric evaluation

This is the PRIMARY composed surface — every concrete curve evaluates through one `NurbsBase`.
Parameters are the curve's NORMALIZED domain `[0,1]` (`PointAt(0.0)`=`StartPoint`,
`PointAt(1.0)`=`EndPoint`), NOT the raw knot domain. The `internal` `GShark.Analyze.Curve.Length`
Gauss-Legendre length kernel and the `internal` `GShark.Sampling.Curve` divide kernels are
reached ONLY through these public instance members — never call the `internal` static directly.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `PointAt(double t)` / `StartPoint` / `MidPoint` / `EndPoint`               | instance       | point on the curve at normalized `t∈[0,1]`; the three anchor accessors |
|  [02]   | `PointAtLength(double)` / `PointAtNormalizedLength(double)`                | instance       | point at arc-length / normalized arc-length (length-reparameterized) |
|  [03]   | `TangentAt(double t)` / `DerivativeAt(double t, int numberOfDerivatives=1)` | instance      | unit tangent; the first-`n` derivative vectors at `t`                |
|  [04]   | `CurvatureAt(double t)` / `Extrema()`                                      | instance       | curvature vector at `t`; the parameter list of curvature/axis extrema |
|  [05]   | `PerpendicularFrameAt(double t)` / `PerpendicularFrames(List<double> u, …)` | instance      | a `Plane` rotation-minimizing frame at `t` / the frame sequence (sweep rails) |
|  [06]   | `Length` (prop) / `LengthAt(double t)` / `ParameterAtLength(double)` / `ParameterAtChordLength(double t, double chordLength)` | instance | total / partial arc length (Gauss-Legendre); param at a target length / chord length |
|  [07]   | `ClosestPoint(Point3)` / `ClosestParameter(Point3)`                        | instance       | foot-of-perpendicular point / its parameter (Newton projection)      |
|  [08]   | `Divide(int numberOfSegments)` / `Divide(double maxSegmentLength, bool equalSegmentLengths=false)` / `DivideByChordLength(double)` | instance | equal-count / max-length / chord-length subdivision → `(Points, Parameters)` |
|  [09]   | `SplitAt(double t)` / `SplitAt(double[] parameters)` / `SubCurve(Interval domain)` | instance | split into sub-curves at one / many parameters; extract a sub-domain curve |
|  [10]   | `ElevateDegree(int desiredDegree)` / `ReduceDegree(double tolerance=0.001)` / `DecomposeIntoBeziers()` | instance | raise / lower degree; explode into the constituent Bézier segments  |
|  [11]   | `Offset(double distance, Plane pln)` / `Reverse()` / `Close()` / `ClampEnds()` | instance     | planar offset; direction reverse; force-close; clamp end knots       |
|  [12]   | `IsClosed` / `IsPeriodic` (props) / `GetBoundingBox()`                     | instance       | closure / periodicity predicates; the curve AABB                     |
|  [13]   | `Transform(TransformMatrix t)` / `static Join(IList<NurbsBase> curves)`    | instance/static | affine transform; join an ordered curve list into a `PolyCurve`     |

## [04]-[SURFACE_ALGEBRA]

[SURFACE_SCOPE]: `NurbsSurface` factories + bidirectional `(u,v)` evaluation
- rail: parametric evaluation

The factory family is the surface CONSTRUCTION vocabulary — corner/grid interpolation, lofting,
extrusion, sweeping, ruling, revolution — each returning a fully-evaluable `NurbsSurface`.
Evaluation is bidirectional in `(u,v)∈[0,1]²` with a `SurfaceDirection`/`SplitDirection`/
`EvaluateSurfaceDirection` discriminant.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `static FromCorners(Point3 p1,p2,p3,p4)` / `FromPoints(int degreeU, int degreeV, List<List<Point3>> points, List<List<double>> weight=null)` | static factory | four-corner planar / control-grid surface |
|  [02]   | `static FromLoft(IList<NurbsBase> curves, LoftType loftType=LoftType.Normal)` | static factory | loft a section-curve sequence (`LoftType` = `Normal`/`Loose`)        |
|  [03]   | `static FromExtrusion(Vector3 direction, NurbsBase profile)` / `FromSweep(NurbsBase rail, NurbsBase profile, Vector3? startTangent=null, Vector3? endTangent=null)` | static factory | extrude a profile along a direction; sweep a profile along a rail |
|  [04]   | `static Ruled(NurbsBase curveA, NurbsBase curveB)` / `Revolved(NurbsBase curveProfile, Ray axis, double rotationAngle)` | static factory | ruled surface between two curves; surface of revolution |
|  [05]   | `PointAt(double u, double v)` / `EvaluateAt(double u, double v, EvaluateSurfaceDirection direction)` | instance | point at `(u,v)`; directional normal/tangent evaluation |
|  [06]   | `ClosestPoint(Point3)` / `ClosestParameter(Point3)` (→ `(double U, double V)`) | instance     | surface foot-of-perpendicular point / its `(u,v)` parameter          |
|  [07]   | `IsoCurve(double parameter, SurfaceDirection direction)` (→ `NurbsCurve`) / `BoundaryEdges()` (→ `NurbsBase[]`) | instance | extract a constant-`u`/`v` isocurve; the four boundary curves        |
|  [08]   | `SplitAt(double parameter, SplitDirection direction)` (→ `NurbsSurface[]`) / `Reverse(SurfaceDirection)` | instance | split in `U`/`V`/`Both`; reverse a parametric direction              |
|  [09]   | `IsClosed(SurfaceDirection direction)` / `IsPlanar(double tolerance=1E-10)` | instance      | directional closure predicate; planarity test                        |
|  [10]   | `Transform(TransformMatrix transformation)`                               | instance       | affine transform of the whole control net                            |

## [05]-[FITTING_INTERSECTION_EVALUATION]

[FITTING_SCOPE]: `GShark.Fitting.Curve` — interpolation / approximation / least-squares
- rail: parametric reconstruction

The genuinely-public reconstruction statics: build a curve THROUGH points (interpolation) or
NEAR points (least-squares approximation). `centripetal` toggles the chord-length vs centripetal
parameterization. These return `NurbsBase`, fully evaluable through `[03]-[CURVE_ALGEBRA]`.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `Curve.Interpolated(List<Point3> pts, int degree, Vector3? startTangent=null, Vector3? endTangent=null, bool centripetal=false)` | static | global interpolation curve through all points with optional end tangents |
|  [02]   | `Curve.Approximate(List<Point3> pts, int degree, bool centripetal=false)`  | static         | least-squares approximation curve near the points (fewer CPs than pts) |
|  [03]   | `Curve.InterpolateBezier(List<Point3> pts)` (→ `List<NurbsBase>`)          | static         | piecewise cubic-Bézier interpolation segment list                    |

[INTERSECTION_SCOPE]: `GShark.Intersection.Intersect` — the typed intersection family
- rail: parametric intersection

One static class, polymorphic over the primitive pair. Linear/planar pairs return `bool` +
`out` hit data (boundary-kernel C# control flow, acceptable at the wire); curve pairs return a
`List<CurvesIntersectionResult>` / `List<CurvePlaneIntersectionResult>` carrying the parameter +
point at each hit. `tolerance` defaults `1E-06`.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `Intersect.PlanePlane(Plane,Plane, out Line)` / `LinePlane(Line,Plane, out Point3, out double t)` | static (bool+out) | plane∩plane → line; line∩plane → point+parameter |
|  [02]   | `Intersect.LineLine(Line,Line, out Point3 pt0, out Point3 pt1, out double t0, out double t1)` | static (bool+out) | closest-approach point pair + parameters of two lines |
|  [03]   | `Intersect.LineCircle(Circle,Line, out Point3[])` / `PlaneCircle(Plane,Circle, out Point3[])` | static (bool+out) | line/plane ∩ circle → 0–2 points |
|  [04]   | `Intersect.CurveCurve(NurbsBase,NurbsBase, double tolerance=1E-06)` / `CurveLine(NurbsBase,Line)` / `CurveSelf(NurbsBase, double tolerance=1E-06)` (→ `List<CurvesIntersectionResult>`) | static | curve∩curve / curve∩line / self-intersection — BVH-accelerated parameter+point list |
|  [05]   | `Intersect.CurvePlane(NurbsBase,Plane, double tolerance=1E-06)` (→ `List<CurvePlaneIntersectionResult>`) / `PolylinePlane(PolyLine,Plane)` (→ `List<Point3>`) | static | curve / polyline ∩ plane → result list |

[EVALUATION_SCOPE]: `GShark.Evaluate.Evaluate` — public low-level evaluation primitives
- rail: parametric evaluation

The handful of genuinely-public evaluation statics for when the instance API is too coarse —
raw basis-function and rational-derivative evaluation.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `Evaluate.RationalDerivatives(NurbsBase curve, double parameter, int numberOfDerivatives=1)` (→ `List<Vector3>`) | static | the rational (weighted) derivative vectors — the primitive `NurbsBase.DerivativeAt` wraps |
|  [02]   | `Evaluate.OneBasisFunction(int degree, KnotVector knots, int span, double knot)` | static       | a single B-spline basis function value (basis-matrix construction)   |
|  [03]   | `Evaluate.AveragePoint(IList<Point3> pts)`                                | static         | centroid of a point set (parameterization helper)                    |

## [06]-[IMPLEMENTATION_LAW]

[VALUE_PROFILE]:
- representation: GShark carries its OWN `double`-precision value vocabulary — `Point3` (`class`), `Point4`/`Vector3` (`readonly struct`), `Plane`, `Line`/`Ray`, the rational primitives `Arc`/`Circle`, the piecewise `PolyLine`/`Polygon`/`PolyCurve`, `BoundingBox`, and the half-edge `Mesh`. These are NOT `Rasm.Vectors` types; a `Point3` is `double x,y,z` with the full operator/`Interpolate`/`Centroid`/`DistanceTo` algebra and `Origin`/`Unset` sentinels.
- numeric profile: `netstandard2.0` means NO `System.Numerics` generic-math and NO `Span`-first kernels — the API is `List<Point3>`/`List<double>`/`KnotVector : List<double>`/`Matrix : List<List<double>>` object models. Curves carry rational `Point4` control points (`x,y,z,w`); `ControlPointLocations` is the dehomogenized `Point3` view.
- parameter convention: curve parameters are the NORMALIZED domain `[0,1]` (not raw knots); surface parameters are `(u,v)∈[0,1]²`. Arc-length reparameterization is explicit (`PointAtLength`/`ParameterAtLength`/`LengthAt`).
- internal-vs-public boundary: `GShark.Sampling`/`GShark.Analyze`/`GShark.Modify` are dominantly `internal static` algorithm kernels — the public surface is the `NurbsBase`/`NurbsSurface` INSTANCE methods plus `Fitting.Curve`, `Intersection.Intersect`, `Evaluate.Evaluate`. The README/manifest framing of "Sampling/Analyze/Modify/Optimization" as composable namespaces is INACCURATE for the public API; design pages compose the instance algebra.

[LOCAL_ADMISSION]:
- GShark is the kernel's HOST-NEUTRAL parametric contract — the curve/surface evaluation a non-Rhino runtime exposes WITHOUT RhinoCommon (`api-rhino`). RhinoCommon's `NurbsCurve`/`NurbsSurface`/`Brep` own the Rhino-host parametric surface; GShark owns the SAME parametric concept managed-only for the universal runtime, meeting RhinoCommon at the wire — never two owners inside one runtime.
- the kernel maps `Rasm.Vectors` ↔ GShark `Point3`/`Vector3`/`Plane` AT THE BOUNDARY (a `Point3(v.X,v.Y,v.Z)` adapter), keeps `Rasm.Vectors` canonical names internal, and never lets GShark's value vocabulary leak past the parametric-evaluation seam. GShark `Point3`/`Vector3` are an evaluation detail, not the kernel primitive.
- GShark replaces the in-house NURBS-Book hand-roll (basis functions, knot insertion, De Boor, Gauss-Legendre length, Newton closest-point) — admit the engine, do NOT re-mint these from the textbook.
- the engine ships its own `GShark.Geometry.ConvexHull` (3D incremental, mutable out-param) and half-edge `Mesh`; the kernel does NOT consume these — `MIConvexHull` (`api-miconvexhull`) owns typed-result hull/Delaunay/Voronoi and the kernel's own meshing owner supersedes the GShark mesh.

[STACKING_LAW]:
- vs Rhino (`api-rhino`): RhinoCommon owns the Rhino-host NURBS/Brep surface; GShark owns the managed host-neutral curve/surface evaluation for the non-Rhino runtime. The split is RUNTIME (which host consumes the contract), not capability — a curve crosses the GShark↔Rhino seam at the wire as control points + knots + weights, never as a shared live object.
- vs MIConvexHull (`api-miconvexhull`): MIConvexHull owns the typed-result `ConvexHull.Create`/`Create2D`/`Triangulation.CreateDelaunay`/`VoronoiMesh.Create` computational-geometry surface. GShark's internal `ConvexHull.GenerateHull` is NOT used — its `Point3` hull would force a value-vocabulary leak; the kernel feeds `MIConvexHull` directly from `Rasm.Vectors`.
- vs the kernel fill owners (`Meshing/delaunay` + `Meshing/arrangement`): a GShark `PolyLine`/`Polygon` boundary that needs FILL triangulation (messy/holey/self-intersecting winding) feeds the kernel-authored exact fill — sampled loops enter `Tessellation.Build` as `Constraint.Segment`s, and a self-overlapping winding resolves through the `Arrangement` `PlanarOverlay` GWN cell classification; the stack is evaluate→sample→tessellate, GShark never tessellating a filled region itself.
- vs Supercluster.KDTree (`api-kdtree`): GShark `ClosestPoint`/`ClosestParameter` is the PARAMETRIC nearest-point (Newton on one curve/surface); `Supercluster.KDTree` is the DISCRETE nearest-point over a sampled point cloud. A dense closest-point query over many sampled curve points routes through the kd-tree; the single-curve projection stays in GShark.
- vs the exact-predicate ladder (`api-doubledouble`/`api-bigrational`): GShark is `double`-only — it carries NO exact/robust arithmetic. Any GShark result feeding a degeneracy-sensitive predicate (orientation, in-circle) escalates to the kernel's own `double`→`ddouble`→`Expansion`→`Fraction` ladder; GShark's `double` evaluation is the geometry, never the adjudication.

[RAIL_LAW]:
- Package: `GShark`
- Owns: the host-neutral pure-managed NURBS curve/surface contract — the `NurbsBase` instance algebra (`PointAt`/`TangentAt`/`DerivativeAt`/`CurvatureAt`/`PerpendicularFrameAt`/`ClosestPoint`/`ClosestParameter`/`Length`/`LengthAt`/`Divide`/`SplitAt`/`SubCurve`/`ElevateDegree`/`ReduceDegree`/`DecomposeIntoBeziers`/`Offset`/`Reverse`/`Close`/`Join`), the `NurbsCurve`/`NurbsSurface` concrete types with the `From*`/`Ruled`/`Revolved` surface factory family and bidirectional `(u,v)` evaluation, the public reconstruction statics `Fitting.Curve.Interpolated`/`Approximate`/`InterpolateBezier`, the typed `Intersection.Intersect` family, the public `Evaluate.RationalDerivatives`/`OneBasisFunction`, and the supporting `Core` types (`Interval`/`KnotVector`/`Matrix`/`Transform`/`TransformMatrix`/`GSharkMath`).
- Accept: managed host-neutral curve/surface evaluation in a non-Rhino runtime; curve interpolation/approximation from point lists; typed curve/line/plane/circle intersection; surface construction (loft/extrude/sweep/rule/revolve) and `(u,v)` evaluation; degree elevation/reduction and Bézier decomposition; `Rasm.Vectors` ↔ `Point3`/`Vector3` mapped strictly AT THE BOUNDARY.
- Reject: treating `GShark.Sampling`/`Analyze`/`Modify` static namespaces as the composable API (they are `internal` kernels — compose the `NurbsBase` instance surface instead); letting GShark `Point3`/`Vector3`/`Plane` leak past the parametric seam as the kernel's primitive vocabulary; consuming GShark's internal `ConvexHull.GenerateHull` or half-edge `Mesh` (use `api-miconvexhull` / the kernel meshing owner); expecting `System.Numerics` generic-math or `Span`-first kernels (it is `netstandard2.0` object-model double-only); feeding a GShark `double` result into a degeneracy-sensitive predicate without escalating to the exact-arithmetic ladder; running GShark as a second NURBS owner inside the Rhino runtime where RhinoCommon already owns the surface.
