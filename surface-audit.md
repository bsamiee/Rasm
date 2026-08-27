# 1. Replace the duplicate geodesic grade with exact-lane policy
**From — libs/dotnet/Rasm/.planning/Parametric/surface.md:L60-L74**
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeodesicGrade {
    public static readonly GeodesicGrade Heat  = new("heat");
    public static readonly GeodesicGrade Exact = new("exact");
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record GeodesicPlan(Arr<Point2d> Sources, Arr<double> Levels, GeodesicGrade Grade) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Sources.Count, floor: 1),
        ValidityClaim.CountAtLeast(count: Levels.Count, floor: 1),
        Levels.All(static level => ValidityClaim.Positive(value: level)));
}
```
**To**
```csharp
// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record GeodesicPlan(
    Arr<Point2d> Sources, Arr<double> Levels,
    Option<WindowPropagationPolicy> Windows = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Sources.Count, floor: 1),
        ValidityClaim.CountAtLeast(count: Levels.Count, floor: 1),
        Sources.All(static source => double.IsFinite(source.X) && double.IsFinite(source.Y)),
        Levels.All(static level => ValidityClaim.Positive(value: level)));
}
```
**Why**
`GeodesicGrade` is a two-item label roster with no owned behavior or evidence. The heat lane needs no extra data, while the exact lane already has a real owner, `WindowPropagationPolicy`; `Option<WindowPropagationPolicy>` therefore makes the lane recoverable from the payload and removes the parallel mode vocabulary.
**Change**
Delete `GeodesicGrade`. Treat `Windows.None` as the cached heat-distance lane and `Windows.Some(policy)` as exact MMP propagation under that policy. Reject non-finite UV sources at the existing plan gate, then update `VertexDistances` to dispatch through `Windows.Match`, so exact budgets are caller-visible data instead of a hidden default selected behind `Grade`.
**Ripples**
In `libs/dotnet/Rasm/.planning/Parametric/develop.md`, replace `GeodesicGrade.Exact` in `Development.DecomposeOf` with `Some(WindowPropagationPolicy.Default)`. No other code-fence consumer of `GeodesicGrade` exists.
**Delta**
LOC -4; types -1; members -2.

# 2. Carry executed geodesic policy without a duplicate vocabulary
**From — libs/dotnet/Rasm/.planning/Parametric/surface.md:L110-L110**
```csharp
    public sealed record GeodesicField(Arr<int> Offsets, Arr<Point2d> Uv, Arr<Point3d> World, Arr<double> LevelOf, GeodesicGrade Grade) : SurfaceResult;
```
**To**
```csharp
    public sealed record GeodesicField(Arr<int> Offsets, Arr<Point2d> Uv, Arr<Point3d> World, Arr<double> LevelOf, Option<WindowPropagationPolicy> Windows) : SurfaceResult;
```
**Why**
The emitted field can outlive its request, so the exact executed policy is result evidence even without a current reader. Reusing the request's `Option<WindowPropagationPolicy>` preserves the exact MMP budgets when present and denotes the heat lane when absent; retaining `GeodesicGrade` would instead duplicate the lane vocabulary while discarding the policy that shaped the result.
**Change**
Replace the smart-enum column with `Windows` and copy `plan.Windows` when `ChainContours` constructs the field. Keep the compact offset-plus-column contour layout; replacing it with nested per-contour arrays would add allocations and weaken the dense carrier.
**Delta**
LOC 0; types 0; members 0.

# 3. Replace four isoline columns with one discriminated row stream
**From — libs/dotnet/Rasm/.planning/Parametric/surface.md:L108-L108**
```csharp
    public sealed record Isolines(Arr<double> UParameters, Arr<NurbsForm.Curve> UCurves, Arr<double> VParameters, Arr<NurbsForm.Curve> VCurves) : SurfaceResult;
```
**To**
```csharp
    public sealed record Isolines(Arr<(ParametricDirection Direction, double Parameter, NurbsForm.Curve Curve)> Curves) : SurfaceResult;
```
**Why**
Four independent arrays make parameter-to-curve alignment an unchecked positional convention. Direction is already a closed discriminant, so one row stream carries direction, parameter, and curve together and deletes three exposed columns without losing either axis.
**Change**
Change the result carrier to one direction/parameter/curve row array. Preserve the current U-then-V emission order while making the axis explicit on every row.
**Delta**
LOC 0; types 0; members -3.

# 4. Construct aligned isoline rows inside the existing total operation fold
**From — libs/dotnet/Rasm/.planning/Parametric/surface.md:L156-L163**
```csharp
    // --- [ISOLINES]
    static Fin<SurfaceResult> IsolinesOf(SurfaceOp.Isolines op) =>
        IsoRows(op.Surface, op.Rule).Bind(rows =>
            rows.U.TraverseM(u => op.Surface.IsoCurve(u, ParametricDirection.U)).As().Bind(uCurves =>
                rows.V.TraverseM(v => op.Surface.IsoCurve(v, ParametricDirection.V)).As().Map(vCurves =>
                    (SurfaceResult)new SurfaceResult.Isolines(rows.U, new Arr<NurbsForm.Curve>([.. uCurves]), rows.V, new Arr<NurbsForm.Curve>([.. vCurves])))));

    static Fin<(Arr<double> U, Arr<double> V)> IsoRows(NurbsForm.Surface surface, IsolineRule rule);
```
**To**
```csharp
    // --- [ISOLINES]
    static Fin<SurfaceResult> IsolinesOf(SurfaceOp.Isolines op) =>
        IsoRows(op.Surface, op.Rule).Bind(rows =>
            rows.U.TraverseM(u => op.Surface.IsoCurve(u, ParametricDirection.U)
                .Map(curve => (Direction: ParametricDirection.U, Parameter: u, Curve: curve))).As().Bind(uCurves =>
            rows.V.TraverseM(v => op.Surface.IsoCurve(v, ParametricDirection.V)
                .Map(curve => (Direction: ParametricDirection.V, Parameter: v, Curve: curve))).As().Map(vCurves =>
                (SurfaceResult)new SurfaceResult.Isolines(
                    new Arr<(ParametricDirection Direction, double Parameter, NurbsForm.Curve Curve)>([.. uCurves, .. vCurves])))));

    static Fin<(Arr<double> U, Arr<double> V)> IsoRows(NurbsForm.Surface surface, IsolineRule rule);
```
**Why**
The current traversal proves curve creation but discards that proof before rebuilding four positional arrays. Mapping each successful `Fin<NurbsForm.Curve>` to its originating parameter keeps the correspondence inside the same short-circuiting `TraverseM`.
**Change**
Attach direction and parameter in each `IsoCurve` success arm and materialize the single typed row array at the existing `SurfaceResult` boundary. Retain `SurfaceOp` and the single `Surfaces.Apply` exhaustive switch; typed `Apply` overloads would violate the one-entry verb-family law.
**Delta**
LOC +3; types 0; members 0.

# 5. Rename pullback to closest-point projection across the owner
**From — libs/dotnet/Rasm/.planning/Parametric/surface.md:L76-L119**
```csharp
public sealed record PullbackPolicy(Dimension DenseFloor, Dimension SeedU, Dimension SeedV, NurbsPolicy Projection) : IValidityEvidence {
    public static readonly PullbackPolicy Canonical = new(
        DenseFloor: Dimension.Create(value: 32), SeedU: Dimension.Create(value: 24), SeedV: Dimension.Create(value: 24),
        Projection: NurbsPolicy.Canonical);

    public static PullbackPolicy Of(Context context) => Canonical with { Projection = NurbsPolicy.Of(context: context) };

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: SeedU.Value, floor: 2),
        ValidityClaim.CountAtLeast(count: SeedV.Value, floor: 2),
        Projection.IsValid);
}

    public sealed record Pullback(NurbsForm.Surface Surface, Arr<Point3d> Probes, PullbackPolicy Policy) : SurfaceOp;

    public sealed record Pulled(Arr<Point2d> Uv, Arr<Point3d> Feet, Arr<double> Distances) : SurfaceResult;
```
**To**
```csharp
public sealed record ProjectionPolicy(Dimension SeedThreshold, Dimension SeedU, Dimension SeedV, NurbsPolicy Nurbs) : IValidityEvidence {
    public static readonly ProjectionPolicy Canonical = new(
        SeedThreshold: Dimension.Create(value: 32), SeedU: Dimension.Create(value: 24), SeedV: Dimension.Create(value: 24),
        Nurbs: NurbsPolicy.Canonical);

    public static ProjectionPolicy Of(Context context) => Canonical with { Nurbs = NurbsPolicy.Of(context: context) };

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: SeedU.Value, floor: 2),
        ValidityClaim.CountAtLeast(count: SeedV.Value, floor: 2),
        Nurbs.IsValid);
}

    public sealed record Project(NurbsForm.Surface Surface, Arr<Point3d> Probes, ProjectionPolicy Policy) : SurfaceOp;

    public sealed record Projection(Arr<Point2d> Uv, Arr<Point3d> Feet, Arr<double> Distances) : SurfaceResult;
```
**Why**
This operation computes closest parameters, feet, and distances for world points. A pullback is a differential-geometric transport of covariant data, so the existing vocabulary names a different operation and forces every consumer through a misleading hop. `SeedThreshold` names the count decision that activates the seed index, while `Nurbs` names the policy's actual owner; `DenseFloor` and `ProjectionPolicy.Projection` obscure both facts.
**Change**
Rename the policy, request case, result case, dispatch arm, and private operation helpers to `ProjectionPolicy`, `Project`, `Projection`, and `ProjectOf`. Preserve the three structure-of-arrays result columns because the resolved consumer reads only `Uv`; an array-of-tuples rewrite would increase retained read bandwidth without deleting capability.
**Ripples**
In `libs/dotnet/Rasm/.planning/Parametric/panelize.md`, rename `PanelPolicy.Pullback` to `Projection`, construct it with `ProjectionPolicy.Of(context)`, call `new SurfaceOp.Project(...)` in `Panelization.Reprovenance`, match `SurfaceResult.Projection`, and return its `Uv` column. Remove `Projection.IsValid` from `PanelPolicy.IsValid`; Task 6 moves that admission to the owning surface so panelization does not validate the same policy twice. No other code-fence consumer resolves.
**Delta**
LOC 0; types 0; members 0.

# 6. Admit projection once and use catalogued collection and value-object surfaces
**From — libs/dotnet/Rasm/.planning/Parametric/surface.md:L207-L228**
```csharp
    // --- [PULLBACK]
    static Fin<SurfaceResult> PullbackOf(SurfaceOp.Pullback op, Op key) =>
        Seeds(op, key)
            .Bind(seeds => op.Probes
                .Zip(seeds, static (probe, seed) => (Probe: probe, Seed: seed))
                .TraverseM(row => op.Surface.ClosestParameter(row.Probe, Some(op.Policy.Projection), row.Seed, key)).As())
            .Map(uv => Emit(op, new Arr<(double U, double V)>([.. uv])));

    static Fin<Arr<Option<(double U, double V)>>> Seeds(SurfaceOp.Pullback op, Op key) {
        if (op.Probes.Count < op.Policy.DenseFloor.Value) {
            return Fin.Succ(new Arr<Option<(double U, double V)>>([.. op.Probes.Map(static _ => Option<(double U, double V)>.None)]));
        }
        (Point3d[] seeds, Point2d[] seedUv) = SeedGrid(op.Surface, op.Policy);
        return NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(seeds)), key: key)
            .Bind(index => key.AcceptValidated<Dimension>(candidate: 1).Bind(one => NeighborKernel.GraphOf(
                index: index, needles: [.. op.Probes], count: Some(one), radius: Option<PositiveMagnitude>.None, key: key)))
            .Map(graph => new Arr<Option<(double U, double V)>>([.. graph.Ids.Select(hits =>
                hits.Length > 0 ? Some((seedUv[hits[0]].X, seedUv[hits[0]].Y)) : Option<(double U, double V)>.None)]));
    }

    static (Point3d[] Seeds, Point2d[] SeedUv) SeedGrid(NurbsForm.Surface surface, PullbackPolicy policy);
    static SurfaceResult Emit(SurfaceOp.Pullback op, Arr<(double U, double V)> uv);
```
**To**
```csharp
    // --- [PROJECTION]
    static Fin<SurfaceResult> ProjectOf(SurfaceOp.Project op, Op key) =>
        !op.Policy.IsValid
            ? Fin.Fail<SurfaceResult>(key.InvalidInput())
            : Seeds(op, key)
                .Bind(seeds => toSeq(op.Probes)
                    .Zip(toSeq(seeds), static (probe, seed) => (Probe: probe, Seed: seed))
                    .TraverseM(row => op.Surface.ClosestParameter(row.Probe, Some(op.Policy.Nurbs), row.Seed, key)).As())
                .Map(uv => Emit(op, new Arr<(double U, double V)>([.. uv])));

    static Fin<Arr<Option<(double U, double V)>>> Seeds(SurfaceOp.Project op, Op key) {
        if (op.Probes.Count < op.Policy.SeedThreshold.Value) {
            return Fin.Succ(new Arr<Option<(double U, double V)>>([.. op.Probes.Map(static _ => Option<(double U, double V)>.None)]));
        }
        (Point3d[] seeds, Point2d[] seedUv) = SeedGrid(op.Surface, op.Policy);
        return NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(seeds)), key: key)
            .Bind(index => NeighborKernel.GraphOf(
                index: index, needles: [.. op.Probes], count: Some(Dimension.Create(value: 1)),
                radius: Option<PositiveMagnitude>.None, key: key))
            .Map(graph => new Arr<Option<(double U, double V)>>([.. graph.Ids.Select(hits =>
                hits.Length > 0 ? Some((seedUv[hits[0]].X, seedUv[hits[0]].Y)) : Option<(double U, double V)>.None)]));
    }

    static (Point3d[] Seeds, Point2d[] SeedUv) SeedGrid(NurbsForm.Surface surface, ProjectionPolicy policy);
    static SurfaceResult Emit(SurfaceOp.Project op, Arr<(double U, double V)> uv);
```
**Why**
The LanguageExt catalogue explicitly excludes `Arr.Zip`; `Zip` belongs to `Seq`, so the current pipeline names a nonexistent member. The constant nearest-neighbor count is trusted source data and should use Thinktecture's generated `Dimension.Create`, not enter a runtime validation bind on every projection. The owning operation must reject an invalid policy even when called outside panelization.
**Change**
Gate `ProjectionPolicy.IsValid` once at `ProjectOf`, re-enter both arrays through `toSeq` before `Zip`, keep `TraverseM` as the short-circuiting batch inversion, and construct the invariant neighbor count directly. Keep `ClosestParameter` as the NURBS owner of single-probe projection and `NeighborIndex` as the dense batch seed owner.
**Delta**
LOC +2; types 0; members 0.

# 7. Delegate area evaluation to the NURBS surface owner
**From — libs/dotnet/Rasm/.planning/Parametric/surface.md:L188-L205**
```csharp
    // --- [CURVATURE_SAMPLE]
    static Fin<SurfaceResult> CurvatureOf(SurfaceOp.CurvatureSample op, Op key) {
        NurbsPolicy policy = op.Policy.IfNone(noneValue: NurbsPolicy.Canonical);
        return Quadrature.Integrate(
                new QuadratureDomain.Rectangle(
                    F: (u, v) => {
                        Vector3d[][] skl = op.Surface.RationalDerivatives(u, v);
                        return Vector3d.CrossProduct(skl[1][0], skl[0][1]).Length;
                    },
                    X: new IntegrationInterval(Lower: 0.0, Upper: 1.0),
                    Y: new IntegrationInterval(Lower: 0.0, Upper: 1.0),
                    Order: policy.GaussOrder.Value),
                control: Some(QuadratureControl.Default with { RequireErrorWitness = false }),
                key: key)
            .Bind(area => SweepCurvature(op, area.Value, key));
    }

    static Fin<SurfaceResult> SweepCurvature(SurfaceOp.CurvatureSample op, double area, Op key);
```
**To**
```csharp
    // --- [CURVATURE_SAMPLE]
    static Fin<SurfaceResult> CurvatureOf(SurfaceOp.CurvatureSample op, Op key) =>
        op.Surface.Area(policy: op.Policy, key: key)
            .Bind(area => SweepCurvature(op, area, key));

    static Fin<SurfaceResult> SweepCurvature(SurfaceOp.CurvatureSample op, double area, Op key);
```
**Why**
`NurbsForm.Surface.Area(Option<NurbsPolicy>, Op?)` already owns surface-area evaluation on the same live surface and result carrier. Rebuilding its Jacobian integral here duplicates the engine, hardcodes the unit rectangle, and suppresses the area owner's error witness.
**Change**
Pass the operation's optional policy directly to `NurbsForm.Surface.Area` and bind its `Fin<double>` into the curvature sweep. Keep the curvature structure-of-arrays result: `Stat<Scalar>.Of` consumes its scalar planes directly, so converting the sweep to tuple rows would weaken the vectorized reduction path.
**Delta**
LOC -12; types 0; members 0.
