# [RASM_CLOUD]

The point-cloud owner: ONE `VectorCloud` `[Union]` over the three cloud modalities — closed planar ring, open polyline chain, mass-weighted cluster — with tolerance-deduplicating admission, mass renormalization, and a lazy native index on the cluster case; ONE `VectorCloudMetric` `[SmartEnum<int>]` owning all thirty cloud measurements as table rows behind a single `Project<TOut>`; ONE `CloudKernel` operation surface (name frozen — `intent.md` dispatches `ComputeHullDetailed`, `sample.md`/`register.md`/`stats.md` bind its folds by name) carrying the canonical covariance/PCA fold, the ring/polyline metric folds, planar winding, the omni-shape projection, and the hull rail. A cloud capability is a metric row, a hull kind row, or a shape column — never a sibling type, never a per-measurement method family.

Covariance is computed exactly once in the corpus: `CloudKernel.CovarianceOf` composes `Domain/stats.md` `SampleMoment` (weighted mean + upper-triangular second moments) into a `matrix.md` `SymmetricMatrix`, and every PCA consumer — principal axes, principal frames, best-fit spread, the settled `Solving/fit` robust-fit prior (it composes `CloudKernel.CovarianceOf` for its quality-ordered draw and reads the upstream `OrientedNormals` field), the `register.md` GICP precision field (via `NeighborKernel.PcaOf`) — reads THIS fold. Per-point neighborhoods, normal estimation, principal curvature, and rotation-minimizing frames are `neighbors.md`'s substrate; the metric rows for those measurements delegate there and this page re-implements none of them. The concave hull kinds the retired source declared but returned `Unsupported` are REALIZED here over the admitted `MIConvexHull` Delaunay complex — declared capability now computes.

## [01]-[INDEX]

- [02]-[VECTOR_CLOUD]: the Ring/Polyline/Cluster union; tolerance-dedup mass-conserving admission; the lazy cluster index; the closest-vertex and radius probes.
- [03]-[CLOUD_METRICS]: the thirty-row metric vocabulary; the covariance/PCA fold; ring mass-property, polyline, and winding folds; the `VectorCloudShape` omni-projection.
- [04]-[HULL]: the five-kind hull rail — native convex 3D/2D, chi-shape concave outline, alpha-complex — with typed receipts.

## [02]-[VECTOR_CLOUD]

- Owner: `VectorCloud` `[Union]` — `RingCase` (closed planar loop: vertices + validated `Polyline` native + `Context`), `PolylineCase` (open chain: vertices + `Context`), `ClusterCase` (point set + optional normalized mass `Arr<double>` + `CloudAdmissionReceipt`). Weighted clusters are the SAME case with `Mass: Some` — never a fourth case. `CloudAdmissionPolicy` (deduplicate flag + optional merge tolerance) is the admission policy value; `CloudAdmissionReceipt` the mass-conservation evidence.
- Entry: three admitting factories — `Ring(points, context, key)` (drops a coincident closing vertex, requires ≥3 distinct vertices, rebuilds the closed `Polyline`, proves closure within tolerance AND zero self-intersections via `Intersection.CurveSelf` under a `Lease<PolylineCurve>.Owned`), `Polyline(points, context, key)` (≥2 finite vertices), `Cluster(points, context, admission?, mass?, key)` — one factory, mass an `Option<Seq<double>>` so weighted construction is an argument, not a sibling name. `Admit(key)` re-proves a rehydrated cloud through the same three paths with dedup off (already-unique vertices stay stable).
- Auto: cluster admission is the ONE dedup-and-renormalize fold — walk input points, merge coordinate-equivalent points under the policy tolerance, accumulate merged mass, renormalize to unit total, and emit `OriginalToUnique` (the input-index → unique-index map every external per-point array re-indexes through — sampling weights, per-point normals, correspondence indices survive deduplication because of this column). Mass conservation gates the fold: `|Σin − Σout| ≤ ε·max(1,|Σin|)` or the admission faults. `ClusterCase.Indexed` is the lazy native `PointCloud` memo — a `ConditionalWeakTable<ClusterCase, PointCloud>` keyed by case reference identity so the index dies with the case and two structurally-equal clusters never share a native handle. `ClosestVertex` probes `PointCloud.ClosestPoint` and lifts the hit into a `ClosestHit` with its `ComponentIndexType.PointCloudPoint` component; `WithinRadius` probes `RTree.PointCloudClosestPoints` over the same index under `key.Catch`.
- Receipt: `CloudAdmissionReceipt` — input/output counts, duplicate/merged counts, tolerance, `OriginalToUnique`, mass totals — `IValidityEvidence` with `IsValid` spelled as ONE `ValidityClaim.All` fold (`Domain/rails.md` mechanism): count/nonnegativity claims plus the cross-field terms `OutputCount + MergedCoordinateCount == InputCount`, merge-implies-dedup, re-index range, and mass conservation.
- Packages: RhinoCommon (`PointCloud.AddRange`/`ClosestPoint`/`PointAt`, `Polyline.IsClosedWithinTolerance`/`SegmentCount`/`ToPolylineCurve`, `RTree.PointCloudClosestPoints`, `Intersection.CurveSelf`, `Sphere`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new cloud modality is one union case + one factory + its arms in the metric adapters; a new admission rule is one policy column; mass semantics never fork into a parallel `WeightedCloud` type.
- Boundary: admission runs ONCE at the factory — every kernel fold below consumes admitted vertices and never re-validates; the native `PointCloud`/`RTree` probes are the named platform seam (mutating native containers under `key.Catch` inside the memo and probe bodies only); `OriginalToUnique` is the published re-index contract and a consumer re-deriving the merge map by coordinate comparison is the deleted form; the self-intersection preflight leases its `PolylineCurve` (`Lease<T>.Owned.Use`) so no native curve escapes the check.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record VectorCloud {
    private VectorCloud() { }
    public sealed record RingCase : VectorCloud { internal RingCase(Seq<Point3d> Vertices, Polyline Native, Context Tolerance) { this.Vertices = Vertices; this.Native = Native; this.Tolerance = Tolerance; } public Seq<Point3d> Vertices { get; } public Polyline Native { get; } public Context Tolerance { get; } }
    public sealed record PolylineCase : VectorCloud { internal PolylineCase(Seq<Point3d> Vertices, Context Tolerance) { this.Vertices = Vertices; this.Tolerance = Tolerance; } public Seq<Point3d> Vertices { get; } public Context Tolerance { get; } }
    public sealed record ClusterCase : VectorCloud {
        internal ClusterCase(Seq<Point3d> Vertices, Context Tolerance, Option<Arr<double>> Mass, CloudAdmissionReceipt Admission) { this.Vertices = Vertices; this.Tolerance = Tolerance; this.Mass = Mass; this.Admission = Admission; }
        public Seq<Point3d> Vertices { get; }
        public Context Tolerance { get; }
        public Option<Arr<double>> Mass { get; }
        public CloudAdmissionReceipt Admission { get; }

        // Reference-keyed lazy native index: dies with the case, never shared across structural equals.
        private static readonly ConditionalWeakTable<ClusterCase, PointCloud> IndexCache = [];
        internal PointCloud Indexed => IndexCache.GetValue(key: this, createValueCallback: static c => {
            PointCloud native = [];
            native.AddRange(points: c.Vertices.AsIterable());
            return native;
        });

        internal Fin<ClosestHit> ClosestVertex(Point3d sample, Op key) =>
            Indexed.ClosestPoint(testPoint: sample) switch {
                int idx when idx >= 0 && idx < Vertices.Count => key.AcceptValue(value: ClosestHit.At(
                    target: sample, point: Indexed.PointAt(index: idx),
                    component: Some(new ComponentIndex(type: ComponentIndexType.PointCloudPoint, index: idx)))),
                _ => Fin.Fail<ClosestHit>(error: key.InvalidResult()),
            };
        internal Fin<Seq<int>> WithinRadius(Point3d sample, double radius, Op key) =>
            new Sphere(center: sample, radius: radius) switch {
                { IsValid: false } => Fin.Fail<Seq<int>>(error: key.InvalidInput()),
                Sphere ball => key.Catch(() => {
                    IEnumerable<int[]> found = RTree.PointCloudClosestPoints(pointcloud: Indexed, needlePts: [sample], limitDistance: ball.Radius);
                    using IDisposable? lease = found as IDisposable;
                    return key.Accept(values: found.FirstOrDefault(defaultValue: []).Where(i => i >= 0 && i < Vertices.Count));
                }),
            };
    }

    public static Fin<VectorCloud> Ring(Seq<Point3d> points, Context context, Op? key = null) =>
        from admitted in AdmitPoints(points: points, context: context, key: key, minimum: 3)
        let closed = admitted.Points.Count > 1 && admitted.Points[0].EpsilonEquals(other: admitted.Points[^1], epsilon: admitted.Context.Absolute.Value)
        let vertices = closed ? admitted.Points.Init : admitted.Points
        from _ in guard(vertices.Count >= 3, admitted.Key.InvalidInput())
        let native = new Polyline([.. vertices.AsIterable(), vertices[0]])
        from closure in guard(native.IsValid && native.IsClosedWithinTolerance(admitted.Context.Absolute.Value) && native.SegmentCount >= 3, admitted.Key.InvalidInput())
        from simple in Optional(native.ToPolylineCurve()).ToFin(admitted.Key.InvalidResult())
            .Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(state: (admitted.Context, admitted.Key),
                project: static (s, active) => Optional(Intersection.CurveSelf(curve: active, tolerance: s.Context.Absolute.Value))
                    .ToFin(s.Key.InvalidResult())
                    .Bind(events => events.Count == 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(s.Key.InvalidInput()))))
        select (VectorCloud)new RingCase(Vertices: vertices, Native: native, Tolerance: admitted.Context);

    public static Fin<VectorCloud> Polyline(Seq<Point3d> points, Context context, Op? key = null) =>
        AdmitPoints(points: points, context: context, key: key, minimum: 2)
            .Map(static a => (VectorCloud)new PolylineCase(Vertices: a.Points, Tolerance: a.Context));

    public static Fin<VectorCloud> Cluster(Seq<Point3d> points, Context context, Option<CloudAdmissionPolicy> admission = default, Option<Seq<double>> mass = default, Op? key = null) =>
        from admitted in AdmitPoints(points: points, context: context, key: key, minimum: 1)
        let policy = admission.IfNone(CloudAdmissionPolicy.Default)
        from fold in CloudKernel.AdmitCluster(points: admitted.Points, mass: mass.Map(static m => new Arr<double>([.. m.AsIterable()])), policy: policy, key: admitted.Key)
        select (VectorCloud)new ClusterCase(Vertices: fold.Points, Tolerance: admitted.Context, Mass: fold.Mass, Admission: fold.Receipt);

    internal Fin<VectorCloud> Admit(Op key) => this switch {
        RingCase ring => Ring(points: ring.Vertices, context: ring.Tolerance, key: key),
        PolylineCase poly => Polyline(points: poly.Vertices, context: poly.Tolerance, key: key),
        ClusterCase c => Cluster(points: c.Vertices, context: c.Tolerance,
            admission: Some(CloudAdmissionPolicy.Default with { Deduplicate = false }),
            mass: c.Mass.Map(static m => toSeq(m.AsIterable())), key: key),
        _ => Fin.Fail<VectorCloud>(key.InvalidInput()),
    };

    private static Fin<(Seq<Point3d> Points, Context Context, Op Key)> AdmitPoints(Seq<Point3d> points, Context context, Op? key, int minimum) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from _ in points.TraverseM(p => op.AcceptValue(value: p)).As()
               from count in guard(points.Count >= minimum, op.InvalidInput())
               select (Points: points, Context: model, Key: op);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmissionPolicy(bool Deduplicate, Option<PositiveMagnitude> Tolerance) {
    internal static CloudAdmissionPolicy Default => new(Deduplicate: true, Tolerance: None);
    internal bool Equivalent(Point3d left, Point3d right) => Tolerance switch {
        { IsSome: true, Case: PositiveMagnitude t } => left.EpsilonEquals(other: right, epsilon: t.Value),
        _ => left == right,
    };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmissionReceipt(
    int InputCount, int OutputCount, int InputDuplicateCoordinateCount, int MergedCoordinateCount,
    double Tolerance, bool Deduplicated, Arr<int> OriginalToUnique,
    Option<double> MassInputTotal, Option<double> MassOutputTotal) : IValidityEvidence {
    internal const double ConservationEps = 1.0e-8;
    internal static bool MassConserved(double input, double output) =>
        Math.Abs(input - output) <= ConservationEps * Math.Max(1.0, Math.Abs(input));
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: InputCount, floor: 1),
        ValidityClaim.Nonnegative(Tolerance),
        ValidityClaim.Of(InputDuplicateCoordinateCount >= 0 && MergedCoordinateCount >= 0),
        ValidityClaim.CountExactly(count: OutputCount + MergedCoordinateCount, expected: InputCount),
        ValidityClaim.Of(MergedCoordinateCount == 0 || Deduplicated),
        ValidityClaim.CountExactly(count: OriginalToUnique.Count, expected: InputCount),
        ValidityClaim.Of(OriginalToUnique.ForAll(i => i >= 0 && i < OutputCount)),
        ValidityClaim.Of((MassInputTotal.Case, MassOutputTotal.Case) switch {
            (double input, double output) => MassConserved(input: input, output: output),
            _ => MassInputTotal.IsNone && MassOutputTotal.IsNone,
        }));
}
```

## [03]-[CLOUD_METRICS]

- Owner: `VectorCloudMetric` `[SmartEnum<int>]` — thirty measurement rows, each carrying an `AdmitsCase(VectorCloud)` gate, an `Output` type column, and a `Measure(cloud, policy, key)` delegate, projected through ONE `Project<TOut>(cloud, policy?, key)`. Rows are declared through five row builders keyed by case family — `Ring<TOut>` (ring-only), `Poly<TOut>` (polyline-only), `Chain<TOut>` (any ordered case — ring or polyline), `Cluster<TOut>` (cluster-only, policy-aware overload for neighborhood-backed rows), `All<TOut>` (any case) — so a metric declaration is one line naming its fold.
- Cases: 30 — ring mass-property rows `Normal`/`Area`/`Perimeter`/`EdgeAspect`/`Skewness`/`Compactness`/`MomentAnisotropy`/`RadiiOfGyration`/`AreaError`/`CentroidError`; whole-cloud rows `Centroid`/`BestFitPlane`/`PrincipalAxes`/`PrincipalFrame`/`Shape`; chain rows `BishopFrames`/`TangentFlow`/`CumulativeArcLength`/`EdgeCurvatures`/`OpenLength`; cluster rows `Covariance`/`PrincipalDirection`/`Spread`/`OrientedNormals`/`PrincipalCurvature`/`Curvedness`/`ShapeIndex`/`Admission`/`Neighborhood`/`CurvatureReceipt`.
- Entry: `internal Fin<TOut> Project<TOut>(VectorCloud cloud, Op key)` and the policy overload `Project<TOut>(cloud, CloudMetricPolicy, key)` — case gate, output-type gate, then the row fold, faulting in evidence order (case refusal names the cloud type, output refusal names the metric owner — the `support.md` gate law); sequence-valued rows (`Seq<Plane>`/`Seq<Vector3d>`/`Seq<double>`) lift element-wise through `key.AcceptValue` before the typed cast. `CloudMetricPolicy` wraps the `neighbors.md` `NeighborhoodPolicy` — the ONE policy record neighborhood-backed rows thread; `AdmitOrDefault` derives the canonical default when the caller passes none.
- Auto: the covariance spine is `CloudKernel.CovarianceOf` — cluster mass (or unit weights) + vertices fold through `SampleMoment.Of` into a `SymmetricMatrix`; `PrincipalStatsOf` chains `DecomposeEigen` and carries `(Mean, Eigen)` with derived `Axes`/`Spread` columns; `PrincipalFrameOf` builds the frame from the two dominant eigenvectors through `VectorFrame.Of`. Ring rows lease their native carriers — `WithRingCurve` (owned `PolylineCurve`) and `WithMassProperties` (owned `AreaMassProperties` from `AreaMassProperties.Compute(closedPlanarCurve, planarTolerance)`) — so every mass-property read happens inside a `Lease<T>.Owned.Use` window. Ring orientation reads `ClosedCurveOrientation` against the fitted plane and signs the normal CCW-positive; skewness is the worst normalized interior-angle deviation from the regular polygon ideal; compactness is `4πA/P²`; moment anisotropy is the in-plane principal-moment ratio (the two axes most orthogonal to the ring normal); planar winding is the angle-sum integer fold `round(Σ∠(vᵢ−q, vᵢ₊₁−q)/2π)` — the 2D ring-membership test, name-distinct from the mesh generalized winding number `reconstruct.md` owns — exposed as `PlanarWindingOf(ring, planeNormal, query, key)` (query-parameterized, so a kernel entry, never a metric row; the `intent.md` `WindingCase` composes it with the CCW-signed `RingNormalOf` normal — a sign-arbitrary best-fit-plane normal flips the winding integer). Chain rows are pure folds: unitized segment tangents, prefix-sum arc length, turning-angle-per-mean-edge discrete curvature, segment-length sum. `Shape` assembles `VectorCloudShape` — the 17-`Option`-column omni-projection (normal, signed and unsigned area, perimeter, aspect, skewness, planarity deviation, compactness, anisotropy, radii of gyration, error terms, best-fit plane, convexity, orientation, open length, spread) around the always-present centroid/principal-frame/principal-axes core — ONE shape answer per cloud case with absent columns as `None`, never three per-case shape records.
- Receipt: `VectorCloudShape` is the projection carrier (`IValidityEvidence`, `IsValid` one `ValidityClaim.All` fold over the present columns); neighborhood-backed rows return the `neighbors.md` receipts unchanged (`Admission` returns the cluster's own `CloudAdmissionReceipt`).
- Packages: RhinoCommon (`AreaMassProperties.Compute` + `Centroid`/`Area`/`AreaError`/`CentroidError`/`CentroidCoordinatesRadiiOfGyration`/`CentroidCoordinatesPrincipalMomentsOfInertia`, `Plane.FitPlaneToPoints`, `PolylineCurve.TryGetPlane`/`ClosedCurveOrientation`, `Polyline.GetSegments`/`IsConvexLoop`, `Vector3d.VectorAngle`/`CrossProduct`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measurement is ONE row through the matching builder; a new cloud case extends the builders' adapt arms; a policy knob is one column on `CloudMetricPolicy` — the thirty-row table is the proof the shape absorbs, three prior sibling surfaces (per-ring statics, per-cluster statics, ad-hoc shape structs) already collapsed into it.
- Boundary: `BishopFrames`/`OrientedNormals`/`PrincipalCurvature`/`Curvedness`/`ShapeIndex`/`Neighborhood`/`CurvatureReceipt` rows DELEGATE to `neighbors.md` (`NeighborKernel.BishopChain`/`OrientNormals`/`PrincipalCurvatures`/`ReceiptOf`) — the fold lives on the neighborhood substrate, the metric row is its cloud-facing name, and a second RTree/kNN/RMF body here is the killed parallel rail; `SampleMoment` is READ from `Domain/stats.md`, never re-derived as a local weighted-covariance loop; `AreaMassProperties`/`PolylineCurve` natives never escape their lease windows; `PlanarWinding` is the 2D ring fold and naming it `WindingNumber` (colliding with the 3D solid-angle GWN family) is the rejected name.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class VectorCloudMetric {
    public static readonly VectorCloudMetric Normal = Ring(key: 0, measure: static (c, k) => CloudKernel.RingNormalOf(ring: c, key: k)),
        Area = Ring(key: 1, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.Area), key: k)),
        Perimeter = Ring(key: 2, measure: static (c, k) => k.AcceptValue(value: c.Native.Length)),
        EdgeAspect = Ring(key: 3, measure: static (c, k) => CloudKernel.EdgeAspectOf(native: c.Native, context: c.Tolerance, key: k)),
        Skewness = Ring(key: 4, measure: static (c, k) => CloudKernel.RingSkewnessOf(ring: c, key: k)),
        Compactness = Ring(key: 5, measure: static (c, k) => CloudKernel.RingCompactnessOf(ring: c, key: k)),
        MomentAnisotropy = Ring(key: 6, measure: static (c, k) => CloudKernel.RingMomentAnisotropyOf(ring: c, key: k)),
        RadiiOfGyration = Ring(key: 7, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidCoordinatesRadiiOfGyration), key: k)),
        AreaError = Ring(key: 8, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.AreaError), key: k)),
        CentroidError = Ring(key: 9, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidError), key: k));
    public static readonly VectorCloudMetric Centroid = All(key: 10, measure: static (c, k) => CloudKernel.CentroidOf(cloud: c, key: k)),
        BestFitPlane = All(key: 11, measure: static (c, k) => CloudKernel.BestFitPlaneOf(cloud: c, key: k)),
        PrincipalAxes = All(key: 12, measure: static (c, k) => CloudKernel.PrincipalAxesOf(cloud: c, key: k)),
        PrincipalFrame = All(key: 13, measure: static (c, k) => CloudKernel.PrincipalFrameOf(cloud: c, key: k)),
        Shape = All(key: 14, measure: static (c, k) => CloudKernel.ShapeOf(cloud: c, key: k));
    public static readonly VectorCloudMetric BishopFrames = Chain(key: 15, measure: static (c, k) => NeighborKernel.BishopChain(cloud: c, key: k)),
        TangentFlow = Poly(key: 16, measure: static (pts, k) => CloudKernel.TangentFlowOf(points: pts, key: k)),
        CumulativeArcLength = Poly(key: 17, measure: static (pts, k) => CloudKernel.CumulativeArcLengthOf(points: pts, key: k)),
        EdgeCurvatures = Poly(key: 18, measure: static (pts, k) => CloudKernel.EdgeCurvaturesOf(points: pts, key: k)),
        OpenLength = Poly(key: 19, measure: static (pts, k) => CloudKernel.OpenLengthOf(points: pts, key: k));
    public static readonly VectorCloudMetric Covariance = Cluster(key: 20, measure: static (c, k) => CloudKernel.CovarianceOf(cluster: c, key: k).Map(static v => v.Cov)),
        PrincipalDirection = Cluster(key: 21, measure: static (c, k) => CloudKernel.PrincipalStatsOf(cluster: c, key: k).Bind(s => k.AcceptValue(value: CloudKernel.AsVector3d(v: s.Eigen[0].Eigenvector)))),
        Spread = Cluster(key: 22, measure: static (c, k) => CloudKernel.PrincipalStatsOf(cluster: c, key: k).Bind(s => k.AcceptValue(value: s.Spread))),
        OrientedNormals = Cluster(key: 23, measure: static (c, p, k) => NeighborKernel.OrientNormals(cluster: c, policy: p.Neighborhood, key: k)),
        PrincipalCurvature = Cluster(key: 24, measure: static (c, p, k) => NeighborKernel.PrincipalCurvatures(cluster: c, policy: p.Neighborhood, key: k)),
        Curvedness = Cluster(key: 25, measure: static (c, p, k) => NeighborKernel.Curvedness(cluster: c, policy: p.Neighborhood, key: k)),
        ShapeIndex = Cluster(key: 26, measure: static (c, p, k) => NeighborKernel.ShapeIndex(cluster: c, policy: p.Neighborhood, key: k)),
        Admission = Cluster(key: 27, measure: static (c, k) => Fin.Succ(c.Admission)),
        Neighborhood = Cluster(key: 28, measure: static (c, p, k) => NeighborKernel.ReceiptOf(cluster: c, policy: p.Neighborhood, key: k)),
        CurvatureReceipt = Cluster(key: 29, measure: static (c, p, k) => NeighborKernel.PrincipalCurvatures(cluster: c, policy: p.Neighborhood, key: k).Map(static r => r.Receipt));

    public Type Output { get; }
    [UseDelegateFromConstructor] internal partial bool AdmitsCase(VectorCloud cloud);
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorCloud cloud, CloudMetricPolicy policy, Op key);

    internal Fin<TOut> Project<TOut>(VectorCloud cloud, Op key) =>
        CloudMetricPolicy.AdmitOrDefault(policy: None, key: key).Bind(policy => Project<TOut>(cloud: cloud, policy: policy, key: key));
    // Gates fault in evidence order (the support.md gate law): case refusal names the cloud type,
    // output refusal names the metric owner — one Unsupported never masks the other.
    internal Fin<TOut> Project<TOut>(VectorCloud cloud, CloudMetricPolicy policy, Op key) =>
        (AdmitsCase(cloud: cloud), Output == typeof(TOut)) switch {
            (false, _) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(TOut))),
            (_, false) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorCloudMetric), outputType: typeof(TOut))),
            _ => Measure(cloud: cloud, policy: policy, key: key).Bind(value => value switch {
                Seq<Vector3d> vs => ProjectSeq<Vector3d, TOut>(values: vs, key: key),
                Seq<double> ds => ProjectSeq<double, TOut>(values: ds, key: key),
                Seq<Plane> ps => ProjectSeq<Plane, TOut>(values: ps, key: key),
                _ => key.AcceptValue(value: value).Map(static v => (TOut)v),
            }),
        };
    private static Fin<TOut> ProjectSeq<TItem, TOut>(Seq<TItem> values, Op key) =>
        values.TraverseM(v => key.AcceptValue(value: v)).As().Map(static valid => (TOut)(object)valid);
    // Row builders: All/Ring/Poly/Chain/Cluster(×2 arities) — declaration-shaped, one line per metric.
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudMetricPolicy(NeighborhoodPolicy Neighborhood) {
    internal static Fin<CloudMetricPolicy> AdmitOrDefault(Option<CloudMetricPolicy> policy, Op key) =>
        policy.Match(Some: p => p.Neighborhood.Admit(key: key).Map(static n => new CloudMetricPolicy(Neighborhood: n)),
                     None: () => NeighborhoodPolicy.Default(key: key).Map(static n => new CloudMetricPolicy(Neighborhood: n)));
}

// --- [MODELS] -----------------------------------------------------------------------------
// The omni-projection: one shape answer per cloud case; absent columns are None, never a per-case
// sibling record. Analysis/inspect.md embeds it inside MeshFaceShape — the field set is a contract.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorCloudShape(
    Point3d Centroid, Plane PrincipalFrame, Seq<(double Moment, Vector3d Axis)> PrincipalAxes,
    Option<Vector3d> Normal, Option<double> SignedArea, Option<double> Area, Option<double> Perimeter,
    Option<double> EdgeAspect, Option<double> Skewness, Option<double> PlanarityDeviation,
    Option<double> Compactness, Option<double> MomentAnisotropy, Option<Vector3d> RadiiOfGyration,
    Option<double> AreaError, Option<Vector3d> CentroidError, Option<Plane> BestFitPlane,
    Option<bool> Convex, Option<CurveOrientation> Orientation, Option<double> OpenLength,
    Option<Vector3d> Spread) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Centroid),
        ValidityClaim.Of(PrincipalFrame.IsValid),
        ValidityClaim.CountExactly(count: PrincipalAxes.Count, expected: 3),
        ValidityClaim.Of(Area.Map(static a => ValidityClaim.Nonnegative(a).Holds).IfNone(true)),
        ValidityClaim.Of(Perimeter.Map(static p => ValidityClaim.Nonnegative(p).Holds).IfNone(true)),
        ValidityClaim.Of(Compactness.Map(static c => ValidityClaim.UnitInterval(c).Holds).IfNone(true)),
        ValidityClaim.Of(OpenLength.Map(static l => ValidityClaim.Nonnegative(l).Holds).IfNone(true)),
        ValidityClaim.Of(BestFitPlane.Map(static p => p.IsValid).IfNone(true)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class CloudKernel {
    // THE corpus covariance/PCA fold: stats.md SampleMoment -> matrix.md SymmetricMatrix -> eigen.
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Point3d> points, Option<Arr<double>> mass, Op key) =>
        from moment in SampleMoment.Of(
            rows: points.Map(static p => new Arr<double>([p.X, p.Y, p.Z])), dimension: 3, key: key,
            weights: mass) // None rides SampleMoment's own unweighted arm — a unit-weight array here is redundant.
        from cov in SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: moment.UpperCovariance, key: key)
        select (Mean: AsVector3d(v: moment.Mean), Cov: cov);
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(VectorCloud.ClusterCase cluster, Op key) =>
        from mass in MassOf(cluster: cluster, key: key)
        from stats in CovarianceOf(points: cluster.Vertices, mass: Some(mass), key: key)
        select stats;
    internal static Fin<Arr<double>> MassOf(VectorCloud.ClusterCase cluster, Op key) =>
        MassOf(mass: cluster.Mass.IfNone(() => new Arr<double>([.. Enumerable.Repeat(1.0 / cluster.Vertices.Count, cluster.Vertices.Count)])), count: cluster.Vertices.Count, key: key);
    internal static Fin<Arr<double>> MassOf(Arr<double> mass, int count, Op key) =>
        from _ in guard(mass.Count == count && mass.ForAll(static w => double.IsFinite(w) && w > 0.0), key.InvalidInput())
        from total in mass.Fold(0.0, static (s, w) => s + w) switch {
            double sum when double.IsFinite(sum) && sum > 0.0 => Fin.Succ(sum),
            _ => Fin.Fail<double>(key.InvalidInput()),
        }
        select new Arr<double>([.. mass.AsIterable().Select(w => w / total)]);

    internal sealed record PrincipalStats(Vector3d Mean, Seq<(double Eigenvalue, Arr<double> Eigenvector)> Eigen) {
        internal Seq<(double Moment, Vector3d Axis)> Axes => Eigen.Map(static p => (Moment: p.Eigenvalue, Axis: AsVector3d(v: p.Eigenvector)));
        internal Vector3d Spread => new(Eigen[0].Eigenvalue, Eigen[1].Eigenvalue, Eigen[2].Eigenvalue);
    }
    // One covariance fold per call — the compute-once law applies to the call graph, not just the corpus.
    internal static Fin<PrincipalStats> PrincipalStatsOf(VectorCloud.ClusterCase cluster, Op key) =>
        from stats in CovarianceOf(cluster: cluster, key: key)
        from eigen in stats.Cov.DecomposeEigen(key: key)
        from full in eigen.Count >= 3
            ? Fin.Succ(new PrincipalStats(Mean: stats.Mean, Eigen: eigen))
            : Fin.Fail<PrincipalStats>(key.InvalidResult())
        select full;
    internal static Vector3d AsVector3d(Arr<double> v) => new(x: v[0], y: v[1], z: v[2]);

    // Planar ring winding: angle-sum integer fold — the 2D membership test, distinct from the mesh GWN family.
    internal static Fin<int> PlanarWindingOf(Seq<Point3d> ring, Vector3d planeNormal, Point3d query, Op key) =>
        ring.Count < 3
            ? Fin.Fail<int>(key.InvalidInput())
            : key.AcceptValue(value: (int)Math.Round(
                ring.Map((v, i) => (V0: v - query, V1: ring[(i + 1) % ring.Count] - query))
                    .Fold(0.0, (sum, pair) => sum + Vector3d.VectorAngle(v1: pair.V0, v2: pair.V1, vNormal: planeNormal)) / (2.0 * Math.PI),
                MidpointRounding.ToEven));

    // Ring folds lease their natives: WithRingCurve (owned PolylineCurve), WithMassProperties (owned AreaMassProperties).
    // RingNormalOf (CCW-signed — the PlanarWindingOf normal intent.md's WindingCase composes) / RingCompactnessOf /
    // RingSkewnessOf / RingMomentAnisotropyOf / RingShapeOf / EdgeAspectOf,
    // chain folds TangentFlowOf / CumulativeArcLengthOf / EdgeCurvaturesOf / OpenLengthOf,
    // AdmitCluster (dedup + renormalize + OriginalToUnique fold), CentroidOf / BestFitPlaneOf / PrincipalAxesOf /
    // PrincipalFrameOf / ShapeOf case dispatchers — the [03] card fixes each fold's law; every native read
    // runs inside Lease<T>.Owned.Use and every scalar exits through key.AcceptValue.
}
```

## [04]-[HULL]

- Owner: `CloudHullKind` `[SmartEnum<int>]` — `Convex3D` / `ConvexFootprint2D` / `ConcaveOutline` / `AlphaShape` / `FootprintWrapper` (the 2D fallback row a rejected 3D hull degrades to); `CloudHullPolicy` (`Tolerance` + `AngleTolerance` + the concave columns `Lambda` edge-length ceiling for the chi-shape and `Alpha` circumradius bound for the alpha complex, both `Option<PositiveMagnitude>` deriving from the cluster's mean spacing when absent); `CloudHullReceipt` + `CloudHullResult` the typed outcome pair.
- Entry: `internal static Fin<CloudHullResult> ComputeHullDetailed(VectorCloud source, CloudHullKind kind, CloudHullPolicy policy, Op key)` — cluster-only; every kind returns a `CloudHullResult` whose `Status` (`Completed`/`Rejected`) plus rejection evidence (coplanarity, containment failures, planarity deviation) states the outcome; there is NO `Unsupported` status — every declared kind computes.
- Auto: `Convex3D` routes native — coplanar preflight (`Point3d.ArePointsCoplanar`), then `Mesh.CreateConvexHull3D(points, out hullFacets, tolerance, angleTolerance)` under a `using`, duplicated out of the window. `ConvexFootprint2D`/`FootprintWrapper` fit the PCA plane (`Plane.FitPlaneToPoints`), project, run `PolylineCurve.CreateConvexHull2d`, verify containment of every input point within tolerance, and mesh the loop via `Mesh.CreateFromClosedPolyline`. `ConcaveOutline` and `AlphaShape` are TWO POLICY ROWS OVER ONE DELAUNAY FOLD: project the cluster onto the PCA plane, wrap each planar coordinate pair as a `DefaultVertex` (`double[] Position` — the `IVertex` contract `Triangulation.CreateDelaunay<TVertex>` demands; raw `double[]` rows do NOT bind), triangulate through `Triangulation.CreateDelaunay<DefaultVertex>(vertices, PlaneDistanceTolerance)` (cells carry `Vertices` + `Adjacency`; the policy `Tolerance` feeds `PlaneDistanceTolerance`), then filter — the alpha complex keeps every triangle whose circumradius `≤ Alpha` (Edelsbrunner, radius convention: the policy column IS the circumradius ceiling, dimensionally a length exactly like `Lambda`, which is why both derive from mean spacing), the chi-shape iteratively erodes the longest boundary edge while the edge exceeds `Lambda` and removal preserves regularity (no vertex abandoned, boundary stays a single simple cycle — Duckham et al.); both extract the boundary loop as the edges with exactly one surviving incident cell, orient it CCW against the fitted plane, lift back to world, and mesh via `Mesh.CreateFromClosedPolyline`. The Delaunay entry THROWS `ConvexHullGenerationException` on degenerate (collinear/coincident) input — the call runs under `key.Catch` and the captured failure folds to `Rejected` receipt evidence, never an escaping exception.
- Receipt: `CloudHullReceipt` — kind, status, tolerances, input/output/facet counts, planarity deviation, coplanar/containment rejection evidence, native-vs-authored route, fallback flag, and for the concave kinds the surviving-triangle count plus the effective `Alpha`/`Lambda` actually applied (`IValidityEvidence`, `IsValid` one `ValidityClaim.All` fold). `CloudHullResult.Project<TOut>` resolves `CloudHullReceipt` / `Mesh` / `VectorCloud` (re-admitted boundary cluster) through typed `ProjectionRow.Of` rows.
- Packages: RhinoCommon (`Mesh.CreateConvexHull3D`, `PolylineCurve.CreateConvexHull2d`, `Mesh.CreateFromClosedPolyline`, `Point3d.ArePointsCoplanar`, `Plane.ClosestParameter`/`PointAt`), MIConvexHull (`Triangulation.CreateDelaunay<DefaultVertex>` with `PlaneDistanceTolerance` — planar rows enter as `DefaultVertex{ Position }` per the `IVertex` contract; `ConvexHullGenerationException` is the degenerate-input seam, funneled through `key.Catch`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new hull species is one kind row + one arm in the hull fold (or one filter predicate over the shared Delaunay fold); a new concave criterion is one policy column.
- Boundary: the two concave kinds share ONE Delaunay fold with the filter predicate as the only difference — two separate triangulate-filter-extract bodies is the rejected duplication; the Delaunay complex is `MIConvexHull`'s and a hand-rolled Bowyer-Watson here is the deleted form (the first-principles CDT is the settled `Meshing/delaunay.md` owner at a different altitude — this rail is the cloud-facing native-first tier, and the hull concern is two-tier by ruling: this rail owns the host/complex kinds — convex native, concave outline, alpha — while `Meshing/delaunay` `LowerHull` owns the predicate-gated exact fold for robust-grade envelopes; one anchor each side, never a second hull owner); the Delaunay call is the ONE foreign-exception seam on this page — `key.Catch` funnels it, and a bare `try`/`catch` or an unguarded call is the deleted form; declared-but-refusing hull kinds are the deleted anti-form this rail exists to kill — a kind row that cannot compute does not exist.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CloudHullKind {
    public static readonly CloudHullKind Convex3D = new(key: 0);
    public static readonly CloudHullKind ConvexFootprint2D = new(key: 1);
    public static readonly CloudHullKind ConcaveOutline = new(key: 2);
    public static readonly CloudHullKind AlphaShape = new(key: 3);
    public static readonly CloudHullKind FootprintWrapper = new(key: 4);
}

[SmartEnum<int>]
public sealed partial class CloudHullStatus {
    public static readonly CloudHullStatus Completed = new(key: 0);
    public static readonly CloudHullStatus Rejected = new(key: 1);
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullPolicy(
    PositiveMagnitude Tolerance, VectorAngle AngleTolerance,
    Option<PositiveMagnitude> Alpha, Option<PositiveMagnitude> Lambda) {
    internal static Fin<CloudHullPolicy> AdmitOrDefault(Option<CloudHullPolicy> policy, Context context, Op key) =>
        policy.Match(
            Some: p => Fin.Succ(p),
            None: () => from tolerance in key.AcceptValidated<PositiveMagnitude>(candidate: context.Absolute.Value)
                        from angle in key.AcceptValidated<VectorAngle>(candidate: context.Angle.Value)
                        select new CloudHullPolicy(Tolerance: tolerance, AngleTolerance: angle, Alpha: None, Lambda: None));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullReceipt(
    CloudHullKind Kind, CloudHullStatus Status, double Tolerance, double AngleTolerance,
    int InputCount, int OutputVertexCount, int NativeFacetCount, int SurvivingTriangleCount,
    Option<double> PlanarityDeviation, Option<double> EffectiveAlpha, Option<double> EffectiveLambda,
    bool CoplanarRejected, int ContainmentRejectedCount, bool NativeRouted, bool Fallback) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(Tolerance),
        ValidityClaim.Nonnegative(AngleTolerance),
        ValidityClaim.CountAtLeast(count: InputCount, floor: 1),
        ValidityClaim.Of(OutputVertexCount >= 0 && NativeFacetCount >= 0 && SurvivingTriangleCount >= 0 && ContainmentRejectedCount >= 0),
        ValidityClaim.Of(PlanarityDeviation.Map(static d => ValidityClaim.Nonnegative(d).Holds).IfNone(true)),
        ValidityClaim.Of(EffectiveAlpha.Map(static a => ValidityClaim.Positive(a).Holds).IfNone(true)),
        ValidityClaim.Of(EffectiveLambda.Map(static l => ValidityClaim.Positive(l).Holds).IfNone(true)),
        ValidityClaim.Of(!Status.Equals(CloudHullStatus.Completed) || OutputVertexCount >= 3));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullResult(Option<Mesh> Mesh, CloudHullReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Context context, Op key) {
        CloudHullResult self = this;
        return AtomProjection.Rows<CloudHullResult, TOut>(self: self, key: key,
            ProjectionRow.Of<CloudHullReceipt>(() => Fin.Succ(self.Receipt)),
            ProjectionRow.Of<Mesh>(() => self.Mesh.ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(Mesh)))
                .Bind(mesh => key.AcceptValue(value: mesh))),
            ProjectionRow.Of<VectorCloud>(() => self.Mesh.ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(VectorCloud)))
                .Bind(mesh => VectorCloud.Cluster(
                    points: toSeq(mesh.Vertices.AsIterable().Select(static v => (Point3d)v)), context: context, key: key))));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Hull fold (CloudKernel): Convex3D native; ConvexFootprint2D/FootprintWrapper PCA-plane native 2D;
// ConcaveOutline (chi-shape: erode longest boundary edge while > Lambda and regular) and AlphaShape
// (keep circumradius <= Alpha, radius convention) as two filter predicates over ONE MIConvexHull Delaunay fold:
//   project(PCA plane) -> wrap DefaultVertex{Position} -> Triangulation.CreateDelaunay<DefaultVertex>
//   -> filter(cells) -> boundary(one-incident edges) -> orient CCW -> lift ->
//   Mesh.CreateFromClosedPolyline; the ConvexHullGenerationException a degenerate
//   (collinear/coincident) input throws funnels through key.Catch into Rejected evidence.
```

## [05]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]        | [OWNER]                | [KIND]                                                        | [RAIL]                                       | [CASES] |
| :-----: | :-------------------- | :--------------------- | :------------------------------------------------------------ | :------------------------------------------- | :-----: |
|  [01]   | Cloud modality        | `VectorCloud`          | `[Union]` Ring/Polyline/Cluster, mass as `Option` column       | factories → `Fin<VectorCloud>`               |    3    |
|  [02]   | Cloud measurement     | `VectorCloudMetric`    | `[SmartEnum<int>]` thirty rows, five row builders              | `Project<TOut> → Fin<TOut>`                  |   30    |
|  [03]   | Admission evidence    | `CloudAdmissionReceipt`| `IValidityEvidence` receipt + `OriginalToUnique` re-index map  | carried on `ClusterCase`                     |    —    |
|  [04]   | Covariance/PCA        | `CloudKernel`          | one fold over `stats.md` `SampleMoment` + `matrix.md` eigen    | `CovarianceOf/PrincipalStatsOf → Fin<…>`     |    —    |
|  [05]   | Hull species          | `CloudHullKind`        | `[SmartEnum<int>]` native convex + Delaunay-filtered concave   | `ComputeHullDetailed → Fin<CloudHullResult>` |    5    |
|  [06]   | Shape omni-projection | `VectorCloudShape`     | 17-`Option`-column record around the always-present PCA core   | metric row payload                           |    —    |
