# [W4_RASM_B1_GROUNDING] — Parametric batch: curve/surface/subdivide/develop

Verified primary extracts for `libs/csharp/Rasm/.planning/Parametric/` batch b1: curve.md [rebuild], surface.md [new], subdivide.md [new], develop.md [new]. Every extract carries a `file:line` anchor. Live-decompile confirmations noted `[assay-verified 2026-07-04]`.

## [00]-[INVENTORIES]

Doctrine root `docs/stacks/csharp/` (`ls`): `README.md` `language.md` `shapes.md` `surfaces-and-dispatch.md` `rails-and-effects.md` `boundaries.md` `algorithms.md` `system-apis.md` + `domain/`.
Doctrine `docs/stacks/csharp/domain/` (`ls`): `README.md` `compute.md` `concurrency.md` `data-interchange.md` `diagnostics.md` `durability.md` `interaction.md` `persistence.md` `postgres.md` `resilience.md` `runtime.md` `transport.md` `validation.md` `visuals.md`.
Shared substrate `libs/csharp/.api/` (`ls`, 31 catalogs): `api-csparse` `api-extensions-ai` `api-generator-equals` `api-grpc-*`(6) `api-hashing` `api-highperformance` `api-hybrid-cache` `api-jsonpatch` `api-languageext` `api-mapperly` `api-mathnet-numerics` `api-mathnet-providers` `api-messagepack` `api-nodatime*`(3) `api-protobuf` `api-quikgraph` `api-redaction` `api-system-configuration` `api-tensors` `api-thinktecture-*`(3) `api-unicolour` `api-unitsnet`.
Folder `libs/csharp/Rasm/.api/` (`ls`, 12 catalogs): `api-bigrational` `api-doubledouble` `api-gshark` `api-hashing` `api-kdtree` `api-manifold` `api-mathnet-numerics` `api-miconvexhull` `api-peteronumbers` `api-rhino` `api-tensors`.
Parametric folder on disk (`ls`): `curve.md` (rebuild target), `locate.md` (landed sibling), `projections.md` (landed sibling). surface/subdivide/develop/nurbs/panelize/patternmap are NEW rows not yet on disk.

## [01]-[DECISION_SCOPE] — RASM-CS-GEOMETRY-DECISION.md

R1 ruling head (`RASM-CS-GEOMETRY-DECISION.md:7-11`): "GShark hinge: VENDOR-AND-OWN NOW (whole algorithm set, one motion; the package pin dies)". `:11` "The vendored owner is `Parametric/nurbs.md` (row 20)... the `GShark 2.3.1` pin + `Rasm.csproj:21` reference + kernel `api-gshark.md` DELETE only with the wave-4 `nurbs.md` landing — never a second NURBS engine beside the vendor." → curve/surface/subdivide/develop compose the VENDORED engine (member surface = the GShark algorithm set documented in `api-gshark.md`, extended by R1's G1/G2/G3/G5/G6/G7 additions authored IN nurbs.md).

Parametric namespace cluster (`:42`): "`Rasm.Geometry.Parametric` | nurbs, curve, surface, subdivide, develop, panelize, patternmap (Parametric) | 2448-2449 `parametric`... `ParametricFault` 2448, `DevelopmentFault` 2449".

Row 20 nurbs.md engine (`:98`): "THE VENDORED NURBS ENGINE (ruling R1)... PUBLIC arbitrary-knot construction for both carriers (G1 — vendored-source edit anchor `NurbsSurface.cs:29`), PUBLIC raw `RationalDerivatives` with the #234 order convention FIXED (G2 — anchor `Evaluate/Surface.cs:25`), fundamental forms I/II + principal/Gaussian/mean curvature as kernel projections over those derivatives (G3)... the Piegl-Tiller SURFACE interpolation/approximation lane (G5 — normal-offset refit substrate)... `ClosestParameter` with PARAMETERIZED iteration/tolerance knobs (G7)... `Fin<NurbsForm> Nurbs.Of(NurbsWire)` — ONE polymorphic admission... evaluation members live on the carriers; op rails live in curve/surface | out: predicates... `Rasm.Vectors`... faults; in: curve, surface, develop (strip evaluation)".

Row 21 curve.md [REBUILD] (`:99`): "Host-neutral curve algebra FULL-BODY (the six stubs at `:116-140` die): the STATION/FRAME family lands as first-class cases — arc-length stations via `Divide(maxSegmentLength, equalSegmentLengths)`/`ParameterAtLength`/`PointAtLength` sweeping BATCH `PerpendicularFrames` RMF over the SpineRef `[T0,T1]` window (`SubCurve` + normalized domain), emitting `(station, frame)` PAIRS — the `PathRow`/`Placement` producer; `Offset` PROMOTED to a first-class `ParametricOp` case: vendored seed + kernel deviation-refinement loop + self-intersection trim through the `[V4]` crossing lattice (G8); SSI disposition row states the HOST-DEFERRED TRIPLE... the anomalous `using Rhino.Geometry` (`:33`) dies... parametric canonical-bytes emitted through reconciliation `EncodeOp.Parametric` (row 5); borrowed `ParameterizationFault` ends → `ParametricFault` 2448; `ToMesh` trimmed-fill → arrangement `PlanarOverlay` (kept) | `Fin<ParametricResult> Parametric.Apply(ParametricOp, Op? key = null)` (Evaluate · Measure · Divide · Stations · Split · Reconstruct · Offset · Intersect2D) | out: nurbs (engine — backward), reconciliation (`EncodeOp.Parametric`), intersect (trim lattice), arrangement (`PlanarOverlay`), faults; in: Generation SpineRef... boundary: ⇄ `projections.md` (evaluation, reciprocal), ⇄ `relations.md` (intersection), ⇄ `locate.md` (never a second location algebra)".

Row 22 surface.md [NEW] (`:100`): "Host-neutral surface ops: UV-PROVENANCED tessellation sampler (kernel-owned UV grid over vendored `PointAt` — per-vertex `(u,v)` beside the `MeshSpace`; the tier-wide UV-pullback obligation lands HERE by construction, `ClosestParameter` the parameterized foreign-point fallback); isoline FAMILIES via `KnotVector.Refine` + `IsoCurve` rows; geodesic POLYLINES composing landed `fields.md` `Geodesic` + `geodesics.md` heat/MMP through the UV pullback (results return to the parametric domain); NORMAL-OFFSET surfaces REAL: Greville-abscissae normal offset + vendored Piegl-Tiller surface refit (R1/G5 strong branch); curvature-domain sampling over nurbs fundamental forms (G3)... host-authored surfaces INGRESS through the public arbitrary-knot construction (G1 closed) | `Fin<SurfaceResult> Surfaces.Apply(SurfaceOp, Op? key = null)` (Tessellate · Isolines · Geodesics · NormalOffset · CurvatureSample · Pullback) | out: nurbs (evaluation/derivatives/fitting — backward), `fields.md`/`geodesics.md` (landed), reconciliation (bytes), `mesh.md` (tessellation admission), faults; in: develop, panelize, patternmap (UV-provenanced input law: a world-space polyline with no surface binding cannot feed them), Generation gate".

Row 23 subdivide.md [NEW] (`:101`): "Subdivision surfaces: Catmull-Clark/Loop as STENCIL ROWS over ONE refinement fold (`[GENERATOR_LAW]` — the next scheme is a data row, never a sibling subdivider); limit-surface evaluation author-kernel (Stam eigenbasis rows); crease/boundary rules as row data; OpenSubdiv stays rejected... | `Fin<SubdivisionResult> Subdivision.Apply(SubdivideOp, Op? key = null)` | out: edit (arena), `mesh.md` (substrate), faults; in: Generation gate (region subdivision item)". Charter fault: `DevelopmentFault` 2449 (Subdivision stage).

Row 24 develop.md [NEW] (`:102`): "Guaranteed-ISOMETRIC developable-strip unroll — distinct from flatten's low-distortion conformal maps AND segment.md's host LSCM capture (tier boundary stated, one anchor each): geodesic-strip decomposition + ruling-line extraction + exact unroll with a per-strip ISOMETRY WITNESS receipt (the Fabrication sheet/plywood/fabric acceptance reads the witness) | `Fin<DevelopmentResult> Development.Apply(DevelopOp, Op? key = null)` | out: surface (UV-provenanced strips — backward), flatten (ChartAtlas seam, W3), `geodesics.md` (landed strips), nurbs (strip evaluation), faults; in: Fabrication unroll dry-run, Generation gate (developable item)". Charter fault: 2449 (Strip stage).

Seam ledger (`:117`): "nurbs ← curve·surface·develop; surface ← develop·panelize·patternmap; reconciliation ← pack·curve·surface; intersect ← ...·curve; arrangement ← ...·curve(`ToMesh` fill); flatten ← develop; edit ← ...·subdivide; `geodesics.md`/`segment.md`/`sample.md` ← skeleton·panelize·patternmap·develop·surface".
Folder ruling (`:43`(d)): "the flatten→`Parametric/` move is REJECTED... breaks the develop→flatten W4→W3 wave logic."
Manifest staging (`:113`): "kernel `api-gshark.md` deletes with the WAVE-4 `nurbs.md` landing (the R1 two-stage staging), never in wave 1."

## [02]-[VENDORED_ENGINE_MEMBERS] — api-gshark.md + live decompile [assay-verified 2026-07-04]

Catalog authority `libs/csharp/Rasm/.api/api-gshark.md`. `2.3.1`, `netstandard2.0`, MIT, pure-managed (`:29-35`). Composed at the `NurbsBase`/`NurbsSurface` INSTANCE surface; `GShark.Sampling`/`Analyze`/`Modify` are `internal static` kernels reached only through public instance members (`:6-9,166`).

NurbsBase curve algebra `api-gshark.md:78-92` (table [03]-[CURVE_ALGEBRA]) — live decompile of `GShark.Geometry.NurbsBase` [assay-verified] confirms these EXACT signatures:
```
public virtual double Length => GShark.Analyze.Curve.Length(this);
public virtual Point3 StartPoint/MidPoint/EndPoint => PointAt(0.0/0.5/1.0);
public virtual Point3 PointAt(double t)
public virtual Point3 PointAtLength(double length)
public virtual Vector3 TangentAt(double t)
public List<Vector3> DerivativeAt(double t, int numberOfDerivatives = 1)
public Vector3 CurvatureAt(double t)
public virtual Point3 ClosestPoint(Point3 point)
public virtual double ClosestParameter(Point3 pt)
public virtual double ParameterAtLength(double segmentLength)
public virtual double LengthAt(double t)
public virtual NurbsBase Offset(double distance, Plane pln)
public static PolyCurve Join(IList<NurbsBase> curves)
public List<NurbsBase> DecomposeIntoBeziers()
public NurbsBase ElevateDegree(int desiredDegree)
public NurbsBase ReduceDegree(double tolerance = 0.001)
public (List<Point3> Points, List<double> Parameters) Divide(int numberOfSegments)
public (List<Point3> Points, List<double> Parameters) Divide(double maxSegmentLength, bool equalSegmentLengths = false)
public List<Plane> PerpendicularFrames(List<double> uValues, Vector3? startTangent = null, Vector3? endTangent = null)
public List<NurbsBase> SplitAt(double t) / SplitAt(double[] parameters)
public NurbsBase SubCurve(Interval domain)
```
`PerpendicularFrames` is the Wang-2008 double-reflection BATCH RMF (`RASM-CS-GEOMETRY-DECISION.md:9` "`PerpendicularFrames(List<double>, Vector3?, Vector3?)` as genuine Wang-2008 double-reflection RMF"). `Divide(maxSegmentLength, equalSegmentLengths=false)` over "a genuine Piegl-Tiller Bezier-decomposed Gauss-Legendre arc-length engine at 1e-6" (`:9`). These are the EXACT members curve.md's Stations/Offset family lowers to.

NurbsSurface algebra `api-gshark.md:104-115` (table [04]) — live decompile of `GShark.Geometry.NurbsSurface` [assay-verified]:
```
public class NurbsSurface : IGeometry<NurbsSurface>, IEquatable<NurbsSurface>, ITransformable<NurbsSurface>
public int DegreeU/DegreeV;  public KnotVector KnotsU/KnotsV;  internal Interval DomainU/DomainV;
public List<List<double>> Weights;  public List<List<Point3>> ControlPointLocations;  public List<List<Point4>> ControlPoints;
internal NurbsSurface(int degreeU, int degreeV, KnotVector knotsU, KnotVector knotsV, List<List<Point4>> controlPts)  // G1 WALL: sole ctor internal
public static NurbsSurface FromCorners(Point3 p1,p2,p3,p4)
public static NurbsSurface FromPoints(int degreeU, int degreeV, List<List<Point3>> points, List<List<double>> weight = null)
public static NurbsSurface FromLoft(IList<NurbsBase> curves, LoftType loftType = LoftType.Normal)
public static NurbsSurface FromExtrusion(Vector3 direction, NurbsBase profile)
public static NurbsSurface FromSweep(NurbsBase rail, NurbsBase profile, Vector3? startTangent = null, Vector3? endTangent = null)
public static NurbsSurface Ruled(NurbsBase curveA, NurbsBase curveB)
public static NurbsSurface Revolved(NurbsBase curveProfile, Ray axis, double rotationAngle)
public Point3 PointAt(double u, double v)
public Point3 ClosestPoint(Point3 point)
public (double U, double V) ClosestParameter(Point3 point)
public Vector3 EvaluateAt(double u, double v, EvaluateSurfaceDirection direction)   // G2: metric-BLIND (single unitized vector)
public NurbsCurve IsoCurve(double parameter, SurfaceDirection direction)
public NurbsSurface[] SplitAt(double parameter, SplitDirection direction)  // decompile line 492 → internal GShark.Sampling.Surface.Split
public NurbsBase[] BoundaryEdges()
public bool IsClosed(SurfaceDirection direction)  /  IsPlanar(double tolerance = 1E-10)
public NurbsSurface Reverse(SurfaceDirection surfaceDirection)
```
CONFIRMED G1 (`api-gshark.md:175`): "The wire is READ-only for surfaces: `NurbsSurface`'s sole ctor is `internal` with no arbitrary-knot public construction, so host-authored surface INGRESS through the wire does not exist in the packaged engine — curves construct, surfaces only read out." R1 (`RASM-CS-GEOMETRY-DECISION.md:9`) G2: "surface derivatives are `internal`... the only public exposure unitizes everything — the public surface API is metric-blind"; G3 "surface curvature/fundamental forms are absent engine-wide"; G5 "no surface interpolation/approximation (#132/#205)". → surface.md's CurvatureSample/NormalOffset compose nurbs.md's NEW G2/G3/G5 lanes, NOT the packaged `EvaluateAt` (which the R1 raw-derivative lane supersedes).

Fitting/Intersection/Evaluation `api-gshark.md:126-158`:
```
Curve.Interpolated(List<Point3> pts, int degree, Vector3? startTangent=null, Vector3? endTangent=null, bool centripetal=false)  // :128
Curve.Approximate(List<Point3> pts, int degree, bool centripetal=false)   // :129 least-squares, fewer CPs than pts
Curve.InterpolateBezier(List<Point3> pts) → List<NurbsBase>   // :130
Intersect.CurveCurve(NurbsBase,NurbsBase, double tolerance=1E-06) / CurveLine(NurbsBase,Line) / CurveSelf(NurbsBase, tolerance=1E-06) → List<CurvesIntersectionResult>  // :145 BVH-accelerated
Intersect.CurvePlane(NurbsBase,Plane, tolerance=1E-06) → List<CurvePlaneIntersectionResult>  // :146
Evaluate.RationalDerivatives(NurbsBase curve, double parameter, int numberOfDerivatives=1) → List<Vector3>  // :156
```
`KnotVector.Refine` public both carriers — R1 wave-1 catalog correction (`RASM-CS-GEOMETRY-DECISION.md:11` "`KnotVector.Refine` public both carriers") and `api-gshark.md:166` ("`KnotVector.Refine` (arbitrary knot refinement, serving BOTH carriers)"). surface.md Isolines composes it. `CurveSelf` (`:145`) is the self-intersection member curve.md Offset self-intersection-trim reads.
Stacking laws: `api-gshark.md:178` "a dense closest-point query over many sampled surface points routes through the kd-tree; the single-surface projection stays in GShark"; `:179` "Any GShark result feeding a degeneracy-sensitive predicate... escalates to the kernel's own `double`→`ddouble`→`Expansion`→`Fraction` ladder".

## [03]-[CANONICAL_BYTES_SEAM] — Spatial/reconciliation.md

`EncodeForm.Parametric` (`Spatial/reconciliation.md:55-62`):
```
public sealed record Parametric : EncodeForm {
    internal Parametric(Arr<Direction> directions, Arr<double> weights, Arr<Point3d> controlNet) {...}
    public Arr<Direction> Directions { get; }  public Arr<double> Weights { get; }  public Arr<Point3d> ControlNet { get; }
}
public readonly record struct Direction(int Degree, Arr<double> Knots);   // :64  (1 Direction = curve, 2 = surface U/V)
public static Fin<EncodeForm> Of(Arr<Direction> directions, Arr<double> weights, Arr<Point3d> controlNet, Op? key = null)  // :78
```
R1 one-curve-one-content-key ingress gate LANDED HERE (`Spatial/reconciliation.md:88-92`):
```
// Clamped + normalized + monotone + finite: the R1 one-curve-one-content-key admission gate.
static bool Normalized(int degree, Arr<double> knots) =>
    degree >= 1 && knots.Count >= (2 * degree) + 2
    && Enumerable.Range(0, degree + 1).All(i => knots[i] == 0.0 && knots[knots.Count - 1 - i] == 1.0)
    && Enumerable.Range(1, knots.Count - 1).All(i => knots[i - 1] <= knots[i])
```
`ParametricStream(EncodeForm.Parametric form)` frozen byte stream (`:288`), iterating `form.Directions` (`:292`). Entry `Reconciliation.Apply(ReconcileOp op, Op? key = null)` → `Fin<ReconcileAnswer>` (`:16`), `ReconcileOp.Encode(EncodeForm Form)` (`:44`). → curve.md (1 Direction) + surface.md (2 Directions) extract (control net, weights, per-direction normalized knots) → `EncodeForm.Of(...)` → `Reconciliation.Apply`.

## [04]-[FAULTS] — Numerics/faults.md

```
public static readonly FaultCluster Parametric = new(2448, "parametric", "Rasm.Geometry.Parametric");   // :69
[SmartEnum<string>] ParametricStage: Construction/Evaluation/Station/Offset/Encode   // :83-88
[SmartEnum<string>] DevelopmentStage: Subdivision/Strip/Panel/Pattern   // :94-98
public sealed record ParametricFault(ParametricStage Stage, string Carrier, string Witness) : GeometryFault;   // :146 → 2448
public sealed record DevelopmentFault(DevelopmentStage Stage, int Unit, double Witness) : GeometryFault;   // :147 → 2449
```
`RASM-CS-GEOMETRY-DECISION.md:48`: "`DevelopmentFault`... witness = the per-concern measure: refinement level · isometry error · panel defect · instance defect". → curve.md routes `ParametricFault(Station/Offset/Evaluation/...)`; surface.md `ParametricFault(Evaluation/Construction/Encode)`; subdivide.md `DevelopmentFault(Subdivision, unit, refinementLevel)`; develop.md `DevelopmentFault(Strip, unit, isometryError)`.

## [05]-[KERNEL_OWNER_SEAMS] — composed by the batch (backward + landed)

Numerics/matrix.md (`Rasm.Vectors`, MathNet+CSparse facade — the ONE linear-algebra access path):
```
EigenSolvePath: DenseSymmetricEvd(0) / DenseGeneralEvd(1,isComplex) / SparseLobpcg(2) / SparseHermitianLobpcg(3) / SparseGeneralizedCholeskyCongruence(4)   // matrix.md:46-51
SolvePath.DenseQrLeastSquares = new(key:1, isSparse:false, ...)   // matrix.md:76
Matrix ops: Of · Identity · Transpose · Multiply · Inverse · PseudoInverse · DecomposeEigenDetailed(complex general)   // matrix.md:151
public static Fin<Matrix> Of(Dimension rows, Dimension cols, Arr<double> entries, Op? key = null)   // matrix.md:163
```
→ subdivide.md Stam eigenbasis composes `DenseGeneralEvd`/`DecomposeEigenDetailed` (subdivision matrix is non-symmetric); surface.md NormalOffset refit composes `DenseQrLeastSquares` (or CSparse `SparseQR`).

Meshing/mesh.md: `public static Fin<MeshSpace> Of(Mesh native, Context context, MeshAssemblyPolicy? assembly = null, Op? key = null)` (`mesh.md:129`, entry `:15`) — the tessellation-admission carrier. `MeshSpace` is `[BoundaryAdapter]` defensive snapshot (`:13`).
Meshing/edit.md: `public static MeshEdit Of(MeshSpace space, ArenaPolicy? policy = null)` (`edit.md:71`) AND `Of(ReadOnlySpan<Point3d> vertices, ReadOnlySpan<(int,int,int)> faces)` soup form (`edit.md:16`) — the single-writer SoA build arena, `ToSpace` publishes by freeze (`:46` README). Arena namespace TOTAL, no fault union (`edit.md:5`). → surface.md Tessellate + subdivide.md refinement build via the soup arena → freeze → MeshSpace.

Processing/geodesics.md (landed, over `MeshSpace`/`IntrinsicMesh`):
```
GeodesicKernel.HeatGeodesicAt(MeshSpace space, Seq<int> sources, Point3d sample, Op key) → Fin<double>   // :92
GeodesicKernel.GeodesicTangentAt(space, sources, sample, key) → Fin<Vector3d>   // :96
GeodesicKernel.PropagateWindows(imesh, source, policy) → WindowPropagation  (MMP-EXACT vertex distances)   // :176
GeodesicKernel.TangentLogMapAt / VectorHeatAt (parallel transport)   // fields.md:426,431
```
→ surface.md Geodesics composes HeatGeodesicAt/PropagateWindows through the UV pullback; develop.md geodesic-strip decomposition composes MMP-exact `PropagateWindows`.

Spatial/fields.md (landed):
```
public sealed record GeodesicCase(MeshSpace Space, Seq<int> Sources) : ScalarField;   // :280
public static Fin<ScalarField> Geodesic(MeshSpace space, Seq<int> sources, Op? key = null)   // :298
public sealed record StripeCase(MeshSpace Space, VectorField CrossField, PositiveMagnitude Frequency) : ScalarField;   // :283
VectorField.CrossField → SegmentKernel.CrossFieldAt(space, symmetry, constraints, cones, sample, key) → Fin<Vector3d>   // fields.md:425, segment.md:744
```
→ surface.md Geodesics composes `fields.md Geodesic`.

Processing/flatten.md (develop→flatten W3 seam):
```
public static Fin<ChartAtlas> Flatten.Apply(ParamOp op, Op? key = null)   // :15,196
public sealed record ChartAtlas(MeshSpace Source, Seq<UvIsland> Islands, Seq<FeatureEdge> Seams, DistortionReceipt Receipt)   // :131
public sealed record DistortionReceipt(MaxConformal/MeanConformal, MaxArea/MeanArea, MaxQuasiConformal, Iterations)   // :120
ParamOp: Harmonic(MeshSpace, Option<Polyline>, Policy) / Lscm / Arap / Bff   // :169-172
```
→ develop.md emits unrolled strips as a ChartAtlas (Islands = strips), isometry witness beside/exceeding DistortionReceipt. Distinct tier from flatten's conformal maps (`RASM-CS-GEOMETRY-DECISION.md:102`).

## [06]-[RECIPROCAL_RHINO_RUNTIME] — projections.md + locate.md (landed, `Rasm.Vectors`/`Rasm.Analysis`)

projections.md (`Rasm.Vectors`) reciprocal boundary (`projections.md:3`): "The host-neutral GShark sibling `curve.md` owns the SAME parametric concept for the non-Rhino runtime — the split is runtime, never capability, and the two meet only at the wire." Rhino-side surface differential geometry (the reciprocals of surface.md's G3 lane):
```
SurfaceProjection.ShapeOperator = THE second-fundamental-form owner (Rhino SurfaceCurvature)   // projections.md:87,113-131
SurfaceProjection.Metric = [E F; F G] first-fundamental-form   // :93
SurfaceProjection.Jacobian (3×2 ∂u/∂v) / AreaScale (|∂u×∂v|)   // :92,94
```
Note: surface.md's G3 fundamental-forms owner is the HOST-NEUTRAL runtime counterpart (over vendored RationalDerivatives), NOT a double-owner of projections.md's Rhino `SurfaceCurvature`-based one — the "second `k·d⊗d` assembly is the double-owner defect" clause (`projections.md:21`) is scoped WITHIN the Rhino runtime.

locate.md (`Rasm.Analysis`) reciprocal (`locate.md:22`): "the host-neutral GShark `Parametric/curve` owner carries the same parametric concept for the non-Rhino runtime (division, closest-point, arc-length live in BOTH by decision — the split is runtime, they meet at the wire)". Rhino division `Division.ByCount(int)/ByLength(double)` → `Curve.DivideByCount/DivideByLength` (`locate.md:178-187`). → curve.md's Divide/Stations is the non-Rhino runtime counterpart of locate.md's Division; `⇄ locate.md (never a second location algebra)` = runtime split, meet at the wire.

## [07]-[UNDERUTILIZATION_CATALOG_ANCHORS]

api-mathnet-numerics.md (folder tier) — quadrature + root-finding the concept admits (kernel Numerics/integrate.md is RK-ODE not quadrature; calculus.md is differentiation — parametric-domain quadrature is UNOWNED):
```
Integrate.OnRectangle(f2d, aA, bA, aB, bB[, order])   // :104 — 2D Gauss-Legendre over [a,b]×[c,d] → surface AREA ∫∫|∂u×∂v|
Integrate.OnCuboid(f3d, aA, bA, aB, bB, aC, bC, order=32)   // :105 — 3D
Broyden.FindRoot(Func<double[],double[]> f, double[] initialGuess, accuracy, maxIter) → double[]   // :120 — nonlinear system (ruling-field)
Brent.TryFindRoot(f, lower, upper, accuracy, maxIter, out root) → bool   // :114 — no-throw 1D root (Fin-composable)
Interpolate.CubicSplineMonotone / CubicSplineWithDerivatives(points, values, firstDerivatives)   // :132-133 — arc-length reparam tables
```
api-csparse.md (shared tier, via matrix.md) — least-squares refit + amortized re-solve:
```
CSparse.Double.Factorization.SparseQR.Create(A, ColumnOrdering).Solve   // :57,118 — m≥n least-squares (surface refit, subdivision operators)
SparseCholesky.Create(A, MinimumDegreeAtPlusA)   // :54,115 — SPD
<factorization>.Refactorize(A)   // :120 — reuse symbolic analysis (iterated refinement re-solve)
```
api-quikgraph.md (shared tier) — strip/panel adjacency graphs:
```
MinimumSpanningTreePrim(this IUndirectedGraph<TVertex,TEdge>, Func<TEdge,double> edgeWeights) → IEnumerable<TEdge>   // :132 — strip unroll spanning order
StronglyConnectedComponents / WeaklyConnectedComponents   // :118 — disconnected developable-region separation
SourceFirstTopologicalSort   // :94 — strip/panel ordering
UndirectedGraph<TVertex,TEdge> / SEdge<TVertex>   // :71,57 — transient adjacency (kernel folds Spatial/neighbors MST, Solving/solver islands the same way :148-149)
```
api-doubledouble.md (folder tier) — isometry witness accumulation:
```
ddouble: readonly struct, ~106-bit significand (~31 digits), FMA TwoProduct + Knuth TwoSum   // :3-4,21-22
implements full System.Numerics hierarchy (INumber/IRootFunctions/...)   // :7-16 — a `where T:INumber<T>` reduce binds ddouble directly
```
→ develop.md isometry residual Σ(surfaceArc − planarArc)² in ddouble (same 106-bit accumulation pattern as QEM Quadric / LM Σr², `README.md:88`), narrow to double at DevelopmentFault witness readout.
api-kdtree.md (folder tier) — dense foreign-point Pullback seed:
```
KDTree.Create factory; NearestNeighbors (k-nearest) + RadialSearch; assembly KDTree.dll, namespace SuperClusterKDTree   // :3-6,24-25
```
→ surface.md Pullback for a POINT SET seeds via kd-tree over the tessellation grid then refines with nurbs ClosestParameter (Newton), per api-gshark.md:178.
api-tensors.md (both tiers) — SIMD batch (benchmark-gated, `:236,251`):
```
TensorPrimitives.MultiplyAdd / FusedMultiplyAdd / Dot   // :103-105 — stencil weighted sums (subdivide), batch frame math (curve stations)
TensorPrimitives.Norm / Distance / Sqrt   // :151,155,140 — isometry-error reduction (develop), tessellation grid (surface)
```

## [08]-[STUB_DEATH_SITES] — curve.md current-file anchors (the six stubs the rebuild replaces)

Current curve.md is a curve+surface mega-owner composing EXTERNAL GShark with bodiless entry signatures (`RASM-CS-GEOMETRY-DECISION.md:99` "the six stubs at `:116-140` die"):
```
public static Fin<Parametric> CurveFrom(ReadOnlySpan<Point3d> controlPoints, ReadOnlySpan<double> weights, int degree, Context tolerance);   // curve.md:116 (bodiless)
public static Fin<Parametric> SurfaceFrom(SurfaceFactory factory, Context tolerance);   // :117 (bodiless — MOVES to surface.md)
public Fin<EvalResult> Apply(ParametricOp op) => this switch {...};   // :123 (surface half splits out)
Fin<EvalResult> CurveApply(Curve c, ParametricOp op);   // :130 (bodiless)
Fin<EvalResult> SurfaceApply(Surface s, ParametricOp op);   // :131 (bodiless — MOVES to surface.md)
public Fin<Polyline> ToPolyline(int divisions);   // :138 (bodiless)
public Fin<MeshSpace> ToMesh(int uvResolution);   // :139 (bodiless — surface tessellation MOVES to surface.md)
using Rhino.Geometry;   // :33 anomalous host-coupling — DIES (host-neutral thesis; Ray3d from Rasm.Vectors)
```
Current ParametricOp (`curve.md:94-103`): Evaluate/Measure/Divide/Split/Reconstruct/Intersect (6 cases; NO Stations, NO Offset). Rebuild target = 8 cases (adds Stations, Offset; Intersect→Intersect2D). Current SurfaceFactory union (`:68-79`, 7 cases) + EvalResult.Sample/Span/Division/Pieces/Crossings (`:82-90`) — surface cases migrate to surface.md.
