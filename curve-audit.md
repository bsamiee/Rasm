# 1. Remove imports whose only consumers are deleted diagnostics
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L29-L30**
```csharp
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance.Buffers;
```
**To**
```csharp
// Tensor and staging-buffer imports DELETED
```
**Why**
The only uses implement the frame-defect reduction and staging plane removed below; keeping either package import would advertise an algorithmic dependency this module no longer has.
**Change**
Delete both imports with the `FrameDefect` computation. Retain `System`, `System.Linq`, MathNet, LanguageExt, and the Rasm/Rhino namespaces because the rebuilt module still consumes them.
**Delta**
LOC -2; types 0; members 0; imports -2.

# 2. Admit division payloads once
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L44-L52**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DivideRule {
    private DivideRule() { }

    public sealed record ByCount(int Count) : DivideRule;
    public sealed record ByLength(double MaxSegment) : DivideRule;
    public sealed record ByEqualLength(double MaxSegment) : DivideRule;
    public sealed record ByChord(double Chord) : DivideRule;
}
```
**To**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DivideRule {
    private DivideRule() { }

    public sealed record ByCount(Dimension Count) : DivideRule;
    public sealed record ByLength(PositiveMagnitude Maximum) : DivideRule;
    public sealed record ByEqualLength(PositiveMagnitude Maximum) : DivideRule;
    public sealed record ByChord(PositiveMagnitude Chord) : DivideRule;
}
```
**Why**
Raw counts and lengths force `ArcTargets` to repeat positivity checks already owned by `Dimension` and `PositiveMagnitude`.
**Change**
Change each payload to its admitted atom, delete scalar positivity branches from `ArcTargets`, and unwrap `.Value` only at the NURBS call. Derived internal counts use `Dimension.Create` only after their arithmetic proves positivity; foreign lengths enter through `AcceptValidated<PositiveMagnitude>`.
**Ripples**
`libs/dotnet/Rasm.Fabrication/.planning/Toolpath/turning.md:876-889` must admit the chord from `ToleranceLane.Chord` once before constructing `DivideRule.ByChord`; `libs/dotnet/Rasm.Fabrication/.planning/Geometry2D/curves.md:73-77,194-201` continues carrying the rule without inspecting its payload.
**Delta**
LOC 0; types 0; members 0; raw scalar payloads -4.

# 3. Compress measure addressing without losing whole-length semantics
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L54-L61**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeasureProbe {
    private MeasureProbe() { }

    public sealed record Whole : MeasureProbe;
    public sealed record AtParameter(double T) : MeasureProbe;
    public sealed record NearPoint(Point3d P) : MeasureProbe;
}
```
**To**
```csharp
[Union<Unit, UnitInterval, Point3d>(T1Name = "Whole", T1IsStateless = true, T2Name = "Parameter", T3Name = "Point")]
public readonly partial struct MeasureAt;
```
**Why**
The regular union spends a root and three nested record types on one stateless choice and two existing payload types; an ad-hoc union preserves the distinct total-length arm without that hierarchy.
**Change**
Rename the address to `MeasureAt`, represent whole length with the stateless `Unit` arm, admit explicit parameters as `UnitInterval`, and dispatch with the generated exhaustive `Switch`.
**Delta**
LOC -6; types -3; members 0.

# 4. Delete the nested intersection discriminant
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L63-L69**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IntersectTarget {
    private IntersectTarget() { }

    public sealed record Curve2d(NurbsForm.Curve Other, Axis Plane) : IntersectTarget;
    public sealed record SectionPlane(Plane Cut) : IntersectTarget;
}
```
**To**
```csharp
// IntersectTarget DELETED
```
**Why**
`IntersectTarget` is a second operation roster nested under `ParametricOp.Intersect2D`; both discriminants are available together and select different kernels.
**Change**
Delete the target hierarchy and promote curve crossing and plane section to direct `ParametricOp` cases, completed by the operation and dispatch tasks below.
**Delta**
LOC -6; types -3; members 0.

# 5. Name and type planar primitives by the geometry they carry
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L71-L78**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlanarPrimitive {
    private PlanarPrimitive() { }

    public sealed record Segment(Point2d From, Point2d To) : PlanarPrimitive;
    public sealed record Sweep(Point2d Center, double Radius, double Start, double Angle) : PlanarPrimitive;
    public sealed record Cubic(Point2d Start, Point2d Control1, Point2d Control2, Point2d End) : PlanarPrimitive;
}
```
**To**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlanarPrimitive {
    private PlanarPrimitive() { }

    public sealed record Segment(Point2d From, Point2d To) : PlanarPrimitive;
    public sealed record Arc(Point2d Center, PositiveMagnitude Radius, VectorAngle Start, VectorAngle Sweep) : PlanarPrimitive;
    public sealed record Bezier(Point2d Start, Point2d Control1, Point2d Control2, Point2d End) : PlanarPrimitive;
}
```
**Why**
`Sweep` and `Cubic` obscure standard arc and cubic-Bezier geometry, while raw radius and angle fields make the rendering boundary validate values the construction kernel already proved.
**Change**
Rename the cases to `Arc` and `Bezier`; admit a positive radius and bounded radian angles when `RoundedOf` emits an arc, so every consumer reads evidence instead of rebuilding it.
**Ripples**
`libs/dotnet/Rasm.Rhino/.planning/Display/draw.md:385-392` must rename the generated arms to `arc`/`bezier`, pass `row.Radius.Value`, `row.Start`, and `row.Sweep` directly, and delete both `VectorAngle.Create` calls.
**Delta**
LOC 0; types 0; members 0; renamed symbols 4.

# 6. Make station windows construction-valid and kernel-neutral
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L81-L91**
```csharp
public sealed record StationPlan(double T0, double T1, DivideRule Rule, Dimension TableFloor) : IValidityEvidence {
    public static readonly Dimension TableCeiling = Dimension.Create(value: 256);
    public static readonly Dimension TableSeed = Dimension.Create(value: 16);

    public static StationPlan Of(double t0, double t1, DivideRule rule) => new(t0, t1, rule, TableSeed);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.UnitInterval(value: T0), ValidityClaim.UnitInterval(value: T1),
        T0 < T1,
        ValidityClaim.CountAtLeast(count: TableFloor.Value, floor: 2));
}
```
**To**
```csharp
[ComplexValueObject]
public sealed partial class StationPlan {
    public UnitInterval Start { get; }
    public UnitInterval End { get; }
    public DivideRule Rule { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UnitInterval start,
        ref UnitInterval end,
        ref DivideRule rule) =>
        validationError = start.Value < end.Value && rule is not null
            ? null
            : new ValidationError("StationPlan requires an ordered unit-domain window and a division rule.");
}
```
**Why**
The public record constructor bypasses the window invariant, `Of` only forwards, and `TableFloor` exposes an inversion implementation threshold as request policy.
**Change**
Use generated complex-value admission for the ordered window, remove `IValidityEvidence`, `Of`, both table statics, and `TableFloor`, and move one private inversion threshold to `Stationize`.
**Delta**
LOC +2; types 0; members -5.

# 7. Consolidate refinement policy, execution, and evidence
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L93-L129**
```csharp
public sealed record RefinePolicy(Tolerance Band, Tolerance Slack, Dimension Rounds, Dimension Seed) : IValidityEvidence {
    public static RefinePolicy Of(Context context, Option<Tolerance> slack = default) =>
        context.For(lane: ToleranceLane.Deviation) switch {
            Tolerance band => new RefinePolicy(
                Band: band, Slack: slack.IfNone(band),
                Rounds: Dimension.Create(value: 6), Seed: Dimension.Create(value: 24)),
        };

    public bool IsValid => ValidityClaim.All(
        Band.IsValid, Slack.IsValid,
        Slack.Value >= Band.Value,
        ValidityClaim.CountAtLeast(count: Seed.Value, floor: 4));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Refinement(Option<double> Target, Option<double> Applied, double Achieved, int Rounds, int Samples);

public readonly record struct RefineRound<TFit, TStation>(TFit Fit, Arr<TStation> Stations, Arr<TStation> Breaching, double Deviation, int Round);

public static class Refine {
    public static Fin<(TFit Fit, Refinement Refinement)> Fold<TFit, TStation>(
        RefinePolicy policy,
        Arr<TStation> seed,
        Func<Arr<TStation>, int, Fin<RefineRound<TFit, TStation>>> fit,
        Func<Arr<TStation>, Arr<TStation>, Arr<TStation>> densify,
        Func<double, Error> unconverged) =>
        Range(0, policy.Rounds.Value).FoldUntil(
            initialState: fit(seed, 0),
            f: (state, _) => state.Bind(s => fit(densify(s.Stations, s.Breaching), s.Round + 1)),
            predicate: static state => state.Match(Succ: static s => s.Breaching.Count == 0, Fail: static _ => true))
        .Bind(final => !double.IsFinite(final.Deviation) || final.Deviation < 0.0
            || (final.Breaching.Count > 0 && final.Deviation > policy.Slack.Value)
            ? Fin.Fail<(TFit Fit, Refinement Refinement)>(unconverged(final.Deviation))
            : Fin.Succ((final.Fit, new Refinement(
                Target: Some(policy.Band.Value), Applied: Some(policy.Slack.Value),
                Achieved: final.Deviation, Rounds: final.Round, Samples: final.Stations.Count))));
}
```
**To**
```csharp
[ComplexValueObject]
public sealed partial class RefinePolicy {
    public Tolerance Target { get; }
    public Tolerance Limit { get; }
    public Dimension Rounds { get; }
    public Dimension Seed { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Tolerance target,
        ref Tolerance limit,
        ref Dimension rounds,
        ref Dimension seed) =>
        validationError = target.IsValid
            && limit.IsValid
            && target.Lane == ToleranceLane.Deviation
            && limit.Lane == target.Lane
            && limit.Value >= target.Value
            && seed.Value >= 4
                ? null
                : new ValidationError("Refinement requires one deviation lane, an ordered limit, and at least four seeds.");

    public static Fin<RefinePolicy> Of(Context context, Option<Tolerance> limit = default, Op? key = null) {
        Tolerance target = context.For(lane: ToleranceLane.Deviation);
        Tolerance accepted = limit.IfNone(target);
        return key.OrDefault().AcceptValidated<RefinePolicy>(
            Validate(
                target, accepted,
                Dimension.Create(value: 6), Dimension.Create(value: 24),
                out RefinePolicy? policy),
            policy);
    }

    internal Fin<(TFit Fit, Refinement Evidence)> Run<TFit, TStation>(
        Arr<TStation> seed,
        Func<Arr<TStation>, int, Fin<RefineRound<TFit, TStation>>> fit,
        Func<Arr<TStation>, Arr<TStation>, Arr<TStation>> densify,
        Func<double, Error> unconverged) =>
        Range(0, Rounds.Value).FoldUntil(
            initialState: fit(seed, 0),
            f: (state, _) => state.Bind(current => current.Breaching.Count == 0
                ? state
                : fit(densify(current.Stations, current.Breaching), current.Index + 1)),
            predicate: static step => step.State.Match(
                Succ: static current => current.Breaching.Count == 0,
                Fail: static _ => true))
        .Bind(final => !double.IsFinite(final.Deviation)
            || final.Deviation < 0.0
            || (final.Breaching.Count > 0 && final.Deviation > Limit.Value)
                ? Fin.Fail<(TFit Fit, Refinement Evidence)>(unconverged(final.Deviation))
                : Fin.Succ((final.Fit, new Refinement(
                    Target, Limit, final.Deviation,
                    Dimension.Create(value: final.Index + 1),
                    Dimension.Create(value: final.Stations.Count)))));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Refinement(Tolerance Target, Tolerance Limit, double Deviation, Dimension Rounds, Dimension Samples);

internal readonly record struct RefineRound<TFit, TStation>(
    TFit Fit, Arr<TStation> Stations, Arr<TStation> Breaching, double Deviation, int Index);

// Refine DELETED
```
**Why**
The record constructors bypass both policies, the two independent options admit half-present evidence, raw counts violate the page law, the one-member `Refine` shell separates behavior from its selecting policy, and the catalogued pure `FoldUntil` predicate receives `(State, Value)` rather than the `Fin` state directly.
**Change**
Admit policy once through generated construction and the existing `AcceptValidated` bridge; put `Run` on the policy; correct the bounded-fold predicate to read `step.State`; stop re-fitting an already settled state; publish typed target, limit, round count, and sample count; keep the zero-based algorithm index internal; delete `Refine`.
**Ripples**
`libs/dotnet/Rasm/.planning/Parametric/surface.md:97,112,175-185` must retain `RefinePolicy`/`Refinement`, call `op.Refine.Run`, read `final.Evidence`, and rename the `RefineRound` constructor's final member from `Round` to `Index`.
**Delta**
LOC +17; types -1; members -1; impossible evidence states -2.

# 8. Admit operation payloads and flatten intersection cases
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L136-L147**
```csharp
    public sealed record Evaluate(NurbsForm.Curve Curve, double T, Dimension Order) : ParametricOp;
    public sealed record Measure(NurbsForm.Curve Curve, MeasureProbe Probe, Context Model) : ParametricOp;
    public sealed record Divide(NurbsForm.Curve Curve, DivideRule Rule) : ParametricOp;
    public sealed record Stations(NurbsForm.Curve Curve, StationPlan Plan) : ParametricOp;
    public sealed record Split(NurbsForm.Curve Curve, Arr<double> At) : ParametricOp;
    public sealed record Reconstruct(NurbsForm.Curve Curve, SplinePolicy Fit, int Samples) : ParametricOp;
    public sealed record Offset(NurbsForm.Curve Curve, Plane Frame, double Distance, RefinePolicy Refine) : ParametricOp;
    public sealed record Blend(NurbsForm.Curve A, double TA, NurbsForm.Curve B, double TB, int Continuity, RefinePolicy Refine) : ParametricOp;
    public sealed record Project(NurbsForm.Curve Curve, Plane Frame, SplinePolicy Fit, RefinePolicy Refine) : ParametricOp;
    public sealed record Intersect2D(NurbsForm.Curve Curve, IntersectTarget Target) : ParametricOp;
    public sealed record RoundedRectangle(Plane Frame, Interval X, Interval Y, double NW, double NE, double SE, double SW) : ParametricOp;
    public sealed record CardinalSpline(Plane Frame, Arr<Point2d> Points, double Tension, bool Closed) : ParametricOp;
```
**To**
```csharp
    public sealed record Evaluate(NurbsForm.Curve Curve, UnitInterval Parameter, Dimension Order) : ParametricOp;
    public sealed record Measure(NurbsForm.Curve Curve, MeasureAt At, Context Model) : ParametricOp;
    public sealed record Divide(NurbsForm.Curve Curve, DivideRule Rule) : ParametricOp;
    public sealed record Stations(NurbsForm.Curve Curve, StationPlan Plan) : ParametricOp;
    public sealed record Split(NurbsForm.Curve Curve, Arr<UnitInterval> At) : ParametricOp;
    public sealed record Reconstruct(NurbsForm.Curve Curve, SplinePolicy Fit, Dimension Samples) : ParametricOp;
    public sealed record Offset(NurbsForm.Curve Curve, Plane Frame, double Distance, RefinePolicy Refine) : ParametricOp;
    public sealed record Blend(NurbsForm.Curve A, UnitInterval AtA, NurbsForm.Curve B, UnitInterval AtB, Dimension Continuity, RefinePolicy Refine) : ParametricOp;
    public sealed record Project(NurbsForm.Curve Curve, Plane Frame, SplinePolicy Fit, RefinePolicy Refine) : ParametricOp;
    public sealed record Intersect(NurbsForm.Curve Curve, NurbsForm.Curve Other, Axis Plane) : ParametricOp;
    public sealed record Section(NurbsForm.Curve Curve, Plane Cut) : ParametricOp;
    public sealed record RoundedRectangle(Plane Frame, Interval X, Interval Y, double NW, double NE, double SE, double SW) : ParametricOp;
    public sealed record CardinalSpline(Plane Frame, Arr<Point2d> Points, UnitInterval Tension, bool Closed) : ParametricOp;
```
**Why**
Unit-domain parameters and public counts arrive raw and are rechecked in bodies, while the nested intersection target causes a second dispatch under the request dispatch.
**Change**
Carry admitted parameters and counts in the request cases, rename terse parameter fields, and replace `Intersect2D(Target)` with direct `Intersect` and `Section` cases. Retain signed offset distance and raw rectangle radii because direction and zero-radius corners are valid states.
**Ripples**
`libs/dotnet/Rasm.Rhino/.planning/Display/draw.md:358-362` passes `row.Tension` directly to `CardinalSpline`; `libs/dotnet/Rasm.Fabrication/.planning/Geometry2D/curves.md:194-201` remains unchanged after its `DivideRule` is admitted.
**Delta**
LOC +1; types +1; members +2; validation branches -4.

# 9. Keep result cases dense without optional pseudo-evidence
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L154-L165**
```csharp
    public sealed record Sample(Point3d Point, Vector3d Tangent, Arr<Vector3d> Derivatives, Plane Frame, Vector3d Curvature) : ParametricResult;
    public sealed record Measured(double Length, double Parameter, Point3d Point, Vector3d Curvature, bool Closed) : ParametricResult;
    public sealed record Division(Arr<double> Parameters, Arr<Point3d> Points) : ParametricResult;

    public sealed record StationField(Arr<double> Arcs, Arr<double> Parameters, Arr<Point3d> Points, Arr<Plane> Frames, double FrameDefect) : ParametricResult;

    public sealed record Pieces(Arr<NurbsForm.Curve> Curves) : ParametricResult;
    public sealed record Refit(NurbsForm.Curve Curve, Refinement Refinement) : ParametricResult;
    public sealed record Offsets(Arr<NurbsForm.Curve> Curves, Refinement Refinement, int TrimmedCrossings, int KeptSegments) : ParametricResult;
    public sealed record Crossings(Arr<(double TA, double TB, Point3d At)> Hits) : ParametricResult;

    public sealed record Outline(Arr<PlanarPrimitive> Run, Plane Frame, bool Closed) : ParametricResult;
```
**To**
```csharp
    public sealed record Sample(Point3d Point, Vector3d Tangent, Arr<Vector3d> Derivatives, Plane Frame, Vector3d Curvature) : ParametricResult;
    public sealed record Measured(double Length, double Parameter, Point3d Point, Vector3d Curvature, bool Closed) : ParametricResult;
    public sealed record Division(Arr<double> Parameters, Arr<Point3d> Points) : ParametricResult;
    public sealed record StationField(Arr<double> Arcs, Arr<double> Parameters, Arr<Point3d> Points, Arr<Plane> Frames) : ParametricResult;
    public sealed record Pieces(Arr<NurbsForm.Curve> Curves) : ParametricResult;
    public sealed record Refit(NurbsForm.Curve Curve, double Deviation, Dimension Samples) : ParametricResult;
    public sealed record Offsets(Arr<NurbsForm.Curve> Curves, Refinement Refinement, Dimension Trimmed, Dimension Kept) : ParametricResult;
    public sealed record Crossings(Arr<(double TA, double TB, Point3d At)> Hits) : ParametricResult;
    public sealed record Outline(Arr<PlanarPrimitive> Run, Plane Frame, bool Closed) : ParametricResult;
```
**Why**
Reconstruction performs no iterative refinement and should not fabricate two absent tolerance fields; frame orthonormality is guaranteed by the frame engine rather than re-certified on every result; offset tallies are counts.
**Change**
Keep the useful fused sample and measurement facts, narrow `Refit` to its measured deviation and sample count, remove `FrameDefect`, and use `Dimension` for offset tallies.
**Delta**
LOC -3; types 0; members 0; duplicated validity witnesses -1.

# 10. Remove the orphaned frame benchmark
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L168-L173**
```csharp
public static class Parametric {
    public static readonly BenchClaim FrameDefectClaim = new(
        Claim: Op.Of(name: nameof(ParametricResult.StationField)),
        VectorizedLane: "TensorPrimitives.Max<double> over the filled |X̂·Ŷ| plane",
        ReferenceLane: "scalar LINQ Max fold over the frame batch",
        SpeedupFloor: 1.0);
```
**To**
```csharp
public static class Parametric {
```
**Why**
The claim gates a computation removed with `FrameDefect`, has no `BenchLedger` composition consumer, and benchmarks duplicated frame validation rather than a distinct algorithm lane.
**Change**
Delete `FrameDefectClaim` and remove all frame-defect card/diagram language; the import deletion is owned by task 1.
**Delta**
LOC -5; types 0; members -1.

# 11. Keep top-level dispatch total after the intersection collapse
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L175-L192**
```csharp
    public static Fin<ParametricResult> Apply(ParametricOp op, Op? key = null) =>
        op.Switch(
            state: key.OrDefault(),
            evaluate:    static (k, e) => EvaluateOf(e, k),
            measure:     static (k, m) => MeasureOf(m, k),
            divide:      static (k, d) => Stationize(d.Curve, d.Rule, StationPlan.TableCeiling, k).Map(static rows =>
                (ParametricResult)new ParametricResult.Division(rows.Parameters, new Arr<Point3d>([.. rows.Parameters.Select(rows.Curve.PointAt)]))),
            stations:    static (k, s) => StationsOf(s, k),
            split:       static (k, s) => SplitOf(s, k),
            reconstruct: static (k, r) => ReconstructOf(r, k),
            offset:      static (k, o) => OffsetOf(o, k),
            blend:       static (k, b) => BlendOf(b, k),
            project:     static (k, p) => ProjectOf(p, k),
            intersect2D: static (k, i) => i.Target.Switch(
                curve2d:      c => CrossingsOf(i.Curve, c, k),
                sectionPlane: p => SectionOf(i.Curve, p.Cut, k)),
            roundedRectangle: static (_, r) => RoundedOf(r),
            cardinalSpline:   static (_, c) => CardinalOf(c));
```
**To**
```csharp
    public static Fin<ParametricResult> Apply(ParametricOp op, Op? key = null) =>
        op.Switch(
            state: key.OrDefault(),
            evaluate:         static (k, e) => EvaluateOf(e, k),
            measure:          static (k, m) => MeasureOf(m, k),
            divide:           static (k, d) => Stationize(d.Curve, d.Rule, k).Map(rows =>
                (ParametricResult)new ParametricResult.Division(
                    rows.Parameters,
                    new Arr<Point3d>([.. rows.Parameters.Select(d.Curve.PointAt)]))),
            stations:         static (k, s) => StationsOf(s, k),
            split:            static (k, s) => SplitOf(s, k),
            reconstruct:      static (k, r) => ReconstructOf(r, k),
            offset:           static (k, o) => OffsetOf(o, k),
            blend:            static (k, b) => BlendOf(b, k),
            project:          static (k, p) => ProjectOf(p, k),
            intersect:        static (k, i) => CrossingsOf(i.Curve, i.Other, i.Plane, k),
            section:          static (k, s) => SectionOf(s.Curve, s.Cut, k),
            roundedRectangle: static (k, r) => RoundedOf(r, k),
            cardinalSpline:   static (k, c) => CardinalOf(c, k));
```
**Why**
The old intersection arm nests generated dispatch, and the divide arm depends on a wrapper field that merely repeats the request curve.
**Change**
Dispatch `Intersect` and `Section` directly, remove the table-threshold argument, close over the request curve only where division projects points, and carry the supplied operation key into both planar constructors.
**Delta**
LOC 0; types 0; members 0; nested dispatches -1.

# 12. Delete the region-forwarding shell
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L194-L204**
```csharp
    public static Fin<ArrangementResult> Fill(Arr<NurbsForm.Curve> loops, Axis plane, Context model, Option<ArrangementPolicy> policy = default, Op? key = null) {
        Op site = key.OrDefault();
        return loops.Exists(loop => !loop.IsClosed(model))
            ? Fin.Fail<ArrangementResult>(site.InvalidInput())
            : loops.TraverseM(loop => Stationize(loop, new DivideRule.ByCount(int.Max(16, 4 * loop.ControlCount)), StationPlan.TableCeiling, site)
                    .Map(static rows => new Polyline(rows.Parameters.Select(rows.Curve.PointAt))))
                .As()
                .Bind(rings => Arrangement.Apply(
                    new ArrangementOp.PlanarOverlay(toSeq(rings), Seq<Polyline>(), BooleanOp.Union, plane,
                        policy.IfNone(noneValue: ArrangementPolicy.Canonical)), site));
    }
```
**To**
```csharp
    // Fill DELETED
```
**Why**
`Fill` is unconsumed, selects an undocumented sampling density, repeats closure admission, and immediately forwards the lowered rings and policy to the actual `Arrangement` owner.
**Change**
Delete `Fill` and its card, diagram, and density-bar claims. A region owner lowers curves under its own admitted fidelity and calls `Arrangement.Apply(PlanarOverlay)` directly; no current repository consumer changes.
**Delta**
LOC -10; types 0; members -1.

# 13. Evaluate only admitted parameters
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L207-L214**
```csharp
    static Fin<ParametricResult> EvaluateOf(ParametricOp.Evaluate op, Op key) =>
        op.T is < 0.0 or > 1.0
            ? Fin.Fail<ParametricResult>(key.InvalidInput())
            : op.Curve.PerpendicularFrames([op.T]).Map(frames => {
                (Point3d point, Vector3d[] ders) = op.Curve.RationalDerivatives(op.T, Some(op.Order));
                return (ParametricResult)new ParametricResult.Sample(
                    point, op.Curve.TangentAt(op.T), new Arr<Vector3d>(ders), frames[0], op.Curve.CurvatureAt(op.T));
            });
```
**To**
```csharp
    static Fin<ParametricResult> EvaluateOf(ParametricOp.Evaluate op, Op key) =>
        op.Curve.PerpendicularFrames([op.Parameter.Value]).Map(frames => {
            (Point3d point, Vector3d[] derivatives) = op.Curve.RationalDerivatives(op.Parameter.Value, Some(op.Order));
            return (ParametricResult)new ParametricResult.Sample(
                point, op.Curve.TangentAt(op.Parameter.Value), new Arr<Vector3d>(derivatives),
                frames[0], op.Curve.CurvatureAt(op.Parameter.Value));
        });
```
**Why**
`UnitInterval` already owns finiteness and range admission, so the branch is duplicate validation; the output retains the useful engine-owned derivative, tangent, frame, and curvature facts.
**Change**
Delete the raw scalar guard, unwrap the admitted parameter only at NURBS calls, and spell the derivative local in full.
**Delta**
LOC -1; types 0; members 0; validation branches -1.

# 14. Express measurement as one result pipeline
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L216-L226**
```csharp
    static Fin<ParametricResult> MeasureOf(ParametricOp.Measure op, Op key) =>
        op.Probe.Switch<(Fin<double> Parameter, Func<double, Fin<double>> Measure)>(
            whole:       _ => (Fin.Succ(1.0), _ => op.Curve.Length(key: key)),
            atParameter: a => (Fin.Succ(a.T), t => op.Curve.LengthAt(t, key: key)),
            nearPoint:   n => (op.Curve.ClosestParameter(n.P, key: key), t => op.Curve.LengthAt(t, key: key)))
        switch {
            (Fin<double> parameter, Func<double, Fin<double>> measure) => parameter.Bind(t => t is < 0.0 or > 1.0
                ? Fin.Fail<ParametricResult>(key.InvalidInput())
                : measure(t).Map(measured => (ParametricResult)new ParametricResult.Measured(
                    measured, t, op.Curve.PointAt(t), op.Curve.CurvatureAt(t), op.Curve.IsClosed(op.Model)))),
        };
```
**To**
```csharp
    static Fin<ParametricResult> MeasureOf(ParametricOp.Measure op, Op key) =>
        op.At.Switch<Fin<(double Parameter, double Length)>>(
                whole: _ => op.Curve.Length(key: key)
                    .Map(static length => (Parameter: 1.0, Length: length)),
                parameter: value => op.Curve.LengthAt(value.Value, key: key)
                    .Map(length => (Parameter: value.Value, Length: length)),
                point: value => op.Curve.ClosestParameter(value, key: key)
                    .Bind(parameter => op.Curve.LengthAt(parameter, key: key)
                        .Map(length => (Parameter: parameter, Length: length))))
            .Bind(row => Fin.Succ((ParametricResult)new ParametricResult.Measured(
                row.Length, row.Parameter, op.Curve.PointAt(row.Parameter),
                op.Curve.CurvatureAt(row.Parameter), op.Curve.IsClosed(op.Model))));
```
**Why**
The tuple of a result and a deferred function is a staging shell, and its range branch repeats parameter admission; direct generated dispatch can return the complete carrier while preserving the optimized whole-length member.
**Change**
Have each address arm produce `Fin<(Parameter, Length)>`, bind the dependent point projection once, and construct the unchanged fused measurement result after the carrier rejoins.
**Delta**
LOC -1; types 0; members 0; delegate shells -1; validation branches -1.

# 15. Replace station forwarding state with its two columns
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L228-L238**
```csharp
    // --- [STATION_KERNEL]
    internal readonly record struct StationRows(NurbsForm.Curve Curve, Arr<double> Arcs, Arr<double> Parameters);

    internal static Fin<StationRows> Stationize(NurbsForm.Curve curve, DivideRule rule, Dimension tableFloor, Op key) =>
        ArcTargets(curve, rule).Bind(arcs => arcs.Count < tableFloor.Value
            ? arcs.TraverseM(arc => curve.ParameterAtLength(arc, key: key)).As()
                .Map(ts => new StationRows(curve, arcs, new Arr<double>([.. ts])))
            : InvertByTable(curve, arcs, key));

    static Fin<Arr<double>> ArcTargets(NurbsForm.Curve curve, DivideRule rule);
    static Fin<StationRows> InvertByTable(NurbsForm.Curve curve, Arr<double> arcs, Op key);
```
**To**
```csharp
    // --- [STATION_KERNEL]
    private static readonly Dimension TableThreshold = Dimension.Create(value: 256);

    static Fin<(Arr<double> Arcs, Arr<double> Parameters)> Stationize(NurbsForm.Curve curve, DivideRule rule, Op key) =>
        ArcTargets(curve, rule).Bind(arcs => arcs.Count < TableThreshold.Value
            ? arcs.TraverseM(arc => curve.ParameterAtLength(arc, key: key)).As()
                .Map(parameters => (Arcs: arcs, Parameters: new Arr<double>([.. parameters])))
            : InvertByTable(curve, arcs, key));

    static Fin<Arr<double>> ArcTargets(NurbsForm.Curve curve, DivideRule rule);
    static Fin<(Arr<double> Arcs, Arr<double> Parameters)> InvertByTable(NurbsForm.Curve curve, Arr<double> arcs, Op key);
```
**Why**
`StationRows` only forwards the input curve beside two outputs, `Stationize` has no external consumer, and the inversion threshold is an algorithm constant rather than a caller parameter.
**Change**
Return a named tuple, keep each request curve in its existing closure, make the kernel private, and own one typed threshold beside the branch that reads it.
**Delta**
LOC 0; types -1; members +1; parameters -1; internal surface -1.

# 16. Project station fields without revalidating or staging diagnostics
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L240-L256**
```csharp
    static Fin<ParametricResult> StationsOf(ParametricOp.Stations op, Op key) =>
        !op.Plan.IsValid
            ? Fin.Fail<ParametricResult>(key.InvalidInput())
            : op.Curve.SubCurve(op.Plan.T0, op.Plan.T1)
                .Bind(window => Stationize(window, op.Plan.Rule, op.Plan.TableFloor, key))
                .Bind(rows => rows.Curve.PerpendicularFrames([.. rows.Parameters]).Bind(frames => {
                    if (frames.Length == 0) {
                        return Fin.Fail<ParametricResult>(key.InvalidResult());
                    }
                    Arr<double> parent = new([.. rows.Parameters.Select(t => op.Plan.T0 + (t * (op.Plan.T1 - op.Plan.T0)))]);
                    Arr<Point3d> points = new([.. frames.Select(static f => f.Origin)]);
                    using MemoryOwner<double> staging = MemoryOwner<double>.Allocate(frames.Length);
                    Span<double> dots = staging.Span;
                    for (int i = 0; i < dots.Length; i++) { dots[i] = Math.Abs(frames[i].XAxis * frames[i].YAxis); }
                    return Fin.Succ((ParametricResult)new ParametricResult.StationField(
                        rows.Arcs, parent, points, new Arr<Plane>(frames), TensorPrimitives.Max<double>(dots)));
                }));
```
**To**
```csharp
    static Fin<ParametricResult> StationsOf(ParametricOp.Stations op, Op key) =>
        op.Curve.SubCurve(op.Plan.Start.Value, op.Plan.End.Value)
            .Bind(window => Stationize(window, op.Plan.Rule, key).Bind(rows =>
                window.PerpendicularFrames([.. rows.Parameters]).Bind(frames => frames.Length == 0
                    ? Fin.Fail<ParametricResult>(key.InvalidResult())
                    : Fin.Succ((ParametricResult)new ParametricResult.StationField(
                        rows.Arcs,
                        new Arr<double>([.. rows.Parameters.Select(parameter => op.Plan.Start.Value
                            + (parameter * (op.Plan.End.Value - op.Plan.Start.Value)))]),
                        new Arr<Point3d>([.. frames.Select(static frame => frame.Origin)]),
                        new Arr<Plane>(frames))))));
```
**Why**
Generated plan admission makes the `IsValid` branch redundant, while the pooled buffer, mutable loop, tensor reduction, and result field only recheck orthogonality promised by `PerpendicularFrames`.
**Change**
Trust admitted plan fields, retain the window curve in the carrier closure, derive the parent parameter column once, and emit only the station data.
**Delta**
LOC -7; types 0; members 0; validation branches -1; mutable staging owners -1.

# 17. Use the monadic fold for dependent curve splitting
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L259-L266**
```csharp
    static Fin<ParametricResult> SplitOf(ParametricOp.Split op, Op key) =>
        op.At.Exists(static t => !double.IsFinite(t))
            ? Fin.Fail<ParametricResult>(key.InvalidInput())
            : toSeq(op.At.Filter(static t => t is > 0.0 and < 1.0).Distinct().OrderBy(static t => t)).Fold(
                Fin.Succ((Head: op.Curve, Done: Seq<NurbsForm.Curve>(), Consumed: 0.0)),
                (state, t) => state.Bind(s => s.Head.SplitAt((t - s.Consumed) / (1.0 - s.Consumed))
                    .Map(pair => (pair.Tail, s.Done.Add(pair.Head), t))))
            .Map(static s => (ParametricResult)new ParametricResult.Pieces(new Arr<NurbsForm.Curve>([.. s.Done.Add(s.Head)])));
```
**To**
```csharp
    static Fin<ParametricResult> SplitOf(ParametricOp.Split op, Op key) =>
        toSeq(toSeq(op.At)
                .Map(static parameter => parameter.Value)
                .Filter(static parameter => parameter is > 0.0 and < 1.0)
                .Distinct()
                .OrderBy(static parameter => parameter))
            .FoldM(
                (Head: op.Curve, Done: Seq<NurbsForm.Curve>(), Consumed: 0.0),
                (split, parameter) => split.Head.SplitAt((parameter - split.Consumed) / (1.0 - split.Consumed))
                    .Map(pair => (pair.Tail, split.Done.Add(pair.Head), parameter)))
            .As()
            .Map(static split => (ParametricResult)new ParametricResult.Pieces(
                new Arr<NurbsForm.Curve>([.. split.Done.Add(split.Head)])));
```
**Why**
The request already carries finite unit parameters, and a `Fold` whose state is itself `Fin` hand-rolls the dependence that catalogued `FoldM` owns.
**Change**
Project admitted values, exclude endpoints, canonicalize the ordered distinct run with the required `toSeq` re-entry after LINQ ordering, and let `FoldM` thread the failing split state.
**Delta**
LOC +1; types 0; members 0; validation branches -1; nested result states -1.

# 18. Return reconstruction evidence directly
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L268-L275**
```csharp
    static Fin<ParametricResult> ReconstructOf(ParametricOp.Reconstruct op, Op key) =>
        Stationize(op.Curve, new DivideRule.ByCount(int.Max(op.Fit.Degree.Value + 1, op.Samples)), StationPlan.TableCeiling, key)
            .Bind(rows => Nurbs.Of(new NurbsWire.CurveThrough(new Arr<Point3d>([.. rows.Parameters.Select(rows.Curve.PointAt)]), op.Fit), key))
            .Bind(form => form is NurbsForm.Curve refit
                ? DeviationAgainst(op.Curve, refit, 2 * op.Samples).Map(deviation =>
                    (ParametricResult)new ParametricResult.Refit(refit, new Refinement(
                        Target: None, Applied: None, Achieved: deviation, Rounds: 1, Samples: op.Samples)))
                : Fin.Fail<ParametricResult>(key.InvalidResult()));
```
**To**
```csharp
    static Fin<ParametricResult> ReconstructOf(ParametricOp.Reconstruct op, Op key) =>
        Stationize(
                op.Curve,
                new DivideRule.ByCount(Dimension.Create(value: int.Max(op.Fit.Degree.Value + 1, op.Samples.Value))),
                key)
            .Bind(rows => Nurbs.Of(new NurbsWire.CurveThrough(
                new Arr<Point3d>([.. rows.Parameters.Select(op.Curve.PointAt)]), op.Fit), key))
            .Bind(form => form is NurbsForm.Curve refit
                ? DeviationAgainst(op.Curve, refit, 2 * op.Samples.Value).Map(deviation =>
                    (ParametricResult)new ParametricResult.Refit(refit, deviation, op.Samples))
                : Fin.Fail<ParametricResult>(key.InvalidResult()));
```
**Why**
The old body rebuilds count admission, reads a forwarded curve, and fabricates absent refinement bands for a one-pass reconstruction.
**Change**
Use the admitted sample count, keep the request curve, and return the measured deviation and sample count directly on `Refit`.
**Delta**
LOC 0; types 0; members -2; fabricated options -2.

# 19. Inline the one-call offset fit shell
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L283-L297**
```csharp
    static Fin<ParametricResult> OffsetOf(ParametricOp.Offset op, Op key) =>
        Stationize(op.Curve, new DivideRule.ByCount(op.Refine.Seed.Value), StationPlan.TableCeiling, key)
            .Bind(rows => Refine.Fold(
                op.Refine, rows.Parameters,
                fit: (stations, round) => SeedFit(op, stations, round, key),
                densify: Densified,
                unconverged: deviation => new GeometryFault.OffsetUnconverged(Kind.Curve, deviation)))
            .Bind(final => TrimLoops(op, final.Fit, final.Refinement, key));

    static Fin<RefineRound<NurbsForm.Curve, double>> SeedFit(ParametricOp.Offset op, Arr<double> stations, int round, Op key) =>
        stations.TraverseM(t => OffsetLocus(op.Curve, op.Frame, op.Distance, t)).As()
            .Bind(samples => Nurbs.Of(new NurbsWire.CurveThrough(new Arr<Point3d>([.. samples]), SplinePolicy.Canonical), key))
            .Bind(form => form is NurbsForm.Curve fit
                ? Fin.Succ(Probed(op, fit, stations, round))
                : Fin.Fail<RefineRound<NurbsForm.Curve, double>>(key.InvalidResult()));
```
**To**
```csharp
    static Fin<ParametricResult> OffsetOf(ParametricOp.Offset op, Op key) =>
        Stationize(op.Curve, new DivideRule.ByCount(op.Refine.Seed), key)
            .Bind(rows => op.Refine.Run(
                seed: rows.Parameters,
                fit: (stations, index) => stations.TraverseM(
                        parameter => OffsetLocus(op.Curve, op.Frame, op.Distance, parameter))
                    .As()
                    .Bind(samples => Nurbs.Of(new NurbsWire.CurveThrough(
                        new Arr<Point3d>([.. samples]), SplinePolicy.Canonical), key))
                    .Bind(form => form is NurbsForm.Curve fit
                        ? Fin.Succ(Probed(op, fit, stations, index))
                        : Fin.Fail<RefineRound<NurbsForm.Curve, double>>(key.InvalidResult())),
                densify: Densified,
                unconverged: deviation => new GeometryFault.OffsetUnconverged(Kind.Curve, deviation)))
            .Bind(final => TrimLoops(op, final.Fit, final.Evidence, key));
```
**Why**
`SeedFit` has one caller and only forwards the offset request through traversal, NURBS fitting, and `Probed`.
**Change**
Inline the fit lambda at the policy-owned run, pass the admitted seed count directly, rename the zero-based ordinal to `index`, and delete `SeedFit`.
**Delta**
LOC -1; types 0; members -1.

# 20. Pass crossing payloads directly to the kernel
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L305-L308**
```csharp
    // --- [PLANAR_CROSSINGS]
    static Fin<ParametricResult> SectionOf(NurbsForm.Curve curve, Plane cut, Op key);

    static Fin<ParametricResult> CrossingsOf(NurbsForm.Curve a, IntersectTarget.Curve2d target, Op key);
```
**To**
```csharp
    // --- [PLANAR_CROSSINGS]
    static Fin<ParametricResult> SectionOf(NurbsForm.Curve curve, Plane cut, Op key);

    static Fin<ParametricResult> CrossingsOf(NurbsForm.Curve a, NurbsForm.Curve b, Axis plane, Op key);
```
**Why**
After the request union owns the intersection case, retaining its deleted nested payload in the kernel signature is an indirection-only wrapper.
**Change**
Pass the second curve and plane directly, and update the signature-pinned implementation body to use `b` and `plane` without reconstructing a target value.
**Delta**
LOC 0; types 0; members 0; wrapper parameters -1.

# 21. Preserve the operation key through planar construction kernels
**From — libs/dotnet/Rasm/.planning/Parametric/curve.md:L310-L312**
```csharp
    // --- [PLANAR_CONSTRUCTION]
    static Fin<ParametricResult> RoundedOf(ParametricOp.RoundedRectangle op);
    static Fin<ParametricResult> CardinalOf(ParametricOp.CardinalSpline op);
```
**To**
```csharp
    // --- [PLANAR_CONSTRUCTION]
    static Fin<ParametricResult> RoundedOf(ParametricOp.RoundedRectangle op, Op key);
    static Fin<ParametricResult> CardinalOf(ParametricOp.CardinalSpline op, Op key);
```
**Why**
Both constructors return `Fin`, so dropping the caller's operation key leaves input and kernel failures without the request identity already carried by every other operation arm.
**Change**
Add `Op key` to both signatures and their signature-pinned bodies, and use it for admission and propagated failures. Task 11 supplies the existing `Apply` state; do not mint a second default key inside either kernel.
**Delta**
LOC 0; types 0; members 0; discarded operation keys -2.
