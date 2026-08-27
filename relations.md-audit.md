# 1. Derive both pair carriers from one typed policy

Location: `relations.md:187-197`, `PairOrder`

From

```csharp
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PairOrder {
    public static readonly PairOrder Ordered = new(key: "ordered",
        orders: static (l, r) => Seq((Left: l, Right: r)),
        attempts: static (l, r) => Seq((Left: l, Right: r)));
    public static readonly PairOrder Unordered = new(key: "unordered",
        orders: static (l, r) => Seq((Left: l, Right: r), (Left: r, Right: l)),
        attempts: static (l, r) => Seq((Left: l, Right: r), (Left: r, Right: l)));
    [UseDelegateFromConstructor] internal partial Seq<(Type Left, Type Right)> Orders(Type left, Type right);
    [UseDelegateFromConstructor] internal partial Seq<(object Left, object Right)> Attempts(object left, object right);
}
```

To

```csharp
[SmartEnum]
internal sealed partial class PairOrder {
    public static readonly PairOrder Ordered = new();
    public static readonly PairOrder Unordered = new();
    internal Seq<(T Left, T Right)> Pairs<T>(T left, T right) => Switch(
        state: (Left: left, Right: right),
        ordered: static pair => Seq(pair),
        unordered: static pair => Seq(pair, (Left: pair.Right, Right: pair.Left)));
}
```

Location: `relations.md:376-387`, build-time and runtime pair folds

From

```csharp
order.Orders(left: left, right: right).Fold(
order.Attempts(left: pair.L, right: pair.R).Fold(
```

To

```csharp
order.Pairs(left: left, right: right).Fold(
order.Pairs(left: pair.L, right: pair.R).Fold(
```

Why

Ordering is a real two-case policy, but its process-local rows need no string identity and its permutation is generic over the carried pair. Replacing the owner with a boolean parameter would reintroduce the forbidden mode knob.

Change

Make `PairOrder` internal and keyless, then derive both the `Type` and runtime pair sequences through one generated exhaustive `Switch`.

Delta

Net -2 LOC and -1 explicit member; type count is neutral. The generated keyed lookup, conversion, parsing, comparison, and formatting surface also disappears.

Ripples

Update the module law that describes two carrier-specific delegate columns and the density row that still labels `PairOrder` as keyed; no direct consumer exists outside this spec.

# 2. Replace solved-posture rows with one acceptance fact

Location: `relations.md:199-204`, `SolvedPosture`

From

```csharp
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SolvedPosture {
    public static readonly SolvedPosture Total = new(key: "total", accepts: static (solved, _) => solved);
    public static readonly SolvedPosture Partial = new(key: "partial", accepts: static (solved, found) => solved || found);
    [UseDelegateFromConstructor] internal partial bool Accepts(bool solved, bool found);
}
```

To

```csharp
// SolvedPosture DELETED
```

Location: `relations.md:327-340`, solved intersection rows

From

```csharp
Solved(posture: SolvedPosture.Total,
Solved(posture: SolvedPosture.Partial,
Solved(posture: SolvedPosture.Total,
Solved(posture: SolvedPosture.Total,
Solved(posture: SolvedPosture.Total,
Solved(posture: SolvedPosture.Total,
```

To

```csharp
Solved(acceptPartial: false,
Solved(acceptPartial: true,
Solved(acceptPartial: false,
Solved(acceptPartial: false,
Solved(acceptPartial: false,
Solved(acceptPartial: false,
```

Location: `relations.md:431-436`, `Solved`

From

```csharp
private static Fin<IntersectionResult> Solved(SolvedPosture posture, bool solved, Option<Curve[]> curves, Option<Point3d[]> points, IntersectionKind kind, Op op, CancellationToken cancel) =>
    (Curves: toSeq(curves.IfNone([])), Points: toSeq(points.IfNone([]))) switch {
        (Seq<Curve> found, Seq<Point3d> hits) => (posture.Accepts(solved: solved, found: !found.IsEmpty || !hits.IsEmpty), cancel.IsCancellationRequested) switch {
```

To

```csharp
private static Fin<IntersectionResult> Solved(bool acceptPartial, bool solved, Option<Curve[]> curves, Option<Point3d[]> points, IntersectionKind kind, Op op, CancellationToken cancel) =>
    (Curves: toSeq(curves.IfNone([])), Points: toSeq(points.IfNone([]))) switch {
        (Seq<Curve> found, Seq<Point3d> hits) => (solved || (acceptPartial && (!found.IsEmpty || !hits.IsEmpty)), cancel.IsCancellationRequested) switch {
```

Location: `relations.md:474-482`, trim-aware ray result

From

```csharp
posture: SolvedPosture.Partial,
```

To

```csharp
acceptPartial: true,
```

Why

The two rows carry only one boolean distinction and have no independent identity, lookup, wire, or behavior axis. The named fact preserves both host-success readings at their owning call sites.

Change

Make partial-result acceptance a `bool` on `Solved` and remove the unnecessary public roster.

Delta

Net -6 LOC, -1 type, and -3 explicit members; the generated keyed roster surface also disappears.

Ripples

Remove `SolvedPosture` from the module owner, solved-array law, growth rule, diagram edge, and density row; describe partial-result acceptance as the local `Solved` fact instead. No direct consumer exists outside this spec.

# 3. Make ray-target policy keyless and domain-named

Location: `relations.md:206-224`, `RayTarget` declaration and rows

From

```csharp
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class RayTarget {
    public static readonly RayTarget MeshCast = new(key: "mesh-cast", reflections: Dimension.Create(value: 1),
    public static readonly RayTarget SurfaceWalk = new(key: "surface-walk", reflections: Dimension.Create(value: 1000),
    public static readonly RayTarget BrepTrim = new(key: "brep-trim", reflections: Dimension.Create(value: 1),
```

To

```csharp
[SmartEnum]
internal sealed partial class RayTarget {
    public static readonly RayTarget MeshCast = new(reflections: Dimension.Create(value: 1),
    public static readonly RayTarget SurfaceCast = new(reflections: Dimension.Create(value: 1000),
    public static readonly RayTarget BrepCast = new(reflections: Dimension.Create(value: 1),
```

Why

Target selection uses the typed `Admits(Type)` column exclusively. No path serializes, persists, or resolves a ray target by string. `SurfaceWalk` and `BrepTrim` name implementation fragments rather than the shared casting operation.

Change

Keep the strategy roster and every behavior column, make its identity process-local and keyless, and name all three rows as casts.

Delta

LOC, explicit member count, and type count neutral; removes three string identities and the generated keyed lookup/conversion surface, with two row renames.

# 4. Make hit projections process-local

Location: `relations.md:135-148`, `HitProjection` declaration and rows

From

```csharp
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class HitProjection {
    public static readonly HitProjection Hits = new(key: "hits", binding: OutputBinding.Of<IntersectionHit>(), transfers: true,
    public static readonly HitProjection Curves = new(key: "curves", binding: OutputBinding.Of<Curve>(), transfers: true,
    public static readonly HitProjection Points = new(key: "points", binding: OutputBinding.Of<Point3d>(), transfers: false,
    public static readonly HitProjection Intervals = new(key: "intervals", binding: OutputBinding.Of<Interval>(), transfers: false,
    public static readonly HitProjection Kinds = new(key: "kinds", binding: OutputBinding.Of<IntersectionKind>(), transfers: false,
    public static readonly HitProjection Tangencies = new(key: "tangencies", binding: OutputBinding.Of<IntersectionTangency>(), transfers: false,
```

To

```csharp
[SmartEnum]
internal sealed partial class HitProjection {
    public static readonly HitProjection Hits = new(binding: OutputBinding.Of<IntersectionHit>(), transfers: true,
    public static readonly HitProjection Curves = new(binding: OutputBinding.Of<Curve>(), transfers: true,
    public static readonly HitProjection Points = new(binding: OutputBinding.Of<Point3d>(), transfers: false,
    public static readonly HitProjection Intervals = new(binding: OutputBinding.Of<Interval>(), transfers: false,
    public static readonly HitProjection Kinds = new(binding: OutputBinding.Of<IntersectionKind>(), transfers: false,
    public static readonly HitProjection Tangencies = new(binding: OutputBinding.Of<IntersectionTangency>(), transfers: false,
```

Why

`Binding.Declared` is the runtime identity used by projection lookup. No path serializes, persists, or resolves a projection by its generated string key. The `Items`-derived frozen index remains the lawful single-axis lookup over that identity.

Change

Make the roster keyless while preserving its derived output-type index.

Delta

LOC, explicit member count, and type count neutral; removes six string identities and the generated keyed lookup/conversion surface.

# 5. Gate hit batches through the validity oracle

Location: `relations.md:119-126`, `IntersectionHit.Project`

From

```csharp
hits.ForAll(static hit => hit.IsValid)
    ? HitProjection.For(output: typeof(TOut))
        .ToFin(key.Unsupported(inputType: typeof(IntersectionHit), outputType: typeof(TOut)))
        .Match(
            Succ: row => Releasing(hits: hits, transfers: row.Transfers, result: row.Binding.Admit<TOut>(values: row.Of(hits: hits), key: key)),
            Fail: cause => DropCurves(hits: hits, result: Fin.Fail<Seq<TOut>>(cause)))
    : DropCurves(hits: hits, result: Fin.Fail<Seq<TOut>>(key.InvalidResult()));
```

To

```csharp
hits.TraverseM(hit => key.AcceptValue(value: hit)).As().BiBind(
    Succ: admitted => HitProjection.For(output: typeof(TOut))
        .ToFin(key.Unsupported(inputType: typeof(IntersectionHit), outputType: typeof(TOut)))
        .Match(
            Succ: row => Releasing(hits: admitted, transfers: row.Transfers, result: row.Binding.Admit<TOut>(values: row.Of(hits: admitted), key: key)),
            Fail: cause => DropCurves(hits: admitted, result: Fin.Fail<Seq<TOut>>(cause))),
    Fail: cause => DropCurves(hits: hits, result: Fin.Fail<Seq<TOut>>(cause)));
```

Why

Directly reading `IsValid` bypasses the registered `OpAcceptance` rail. `TraverseM` short-circuits at the first rejected hit while both failure branches retain curve-release custody.

Change

Admit every hit through `AcceptValue` before facet projection and release the original batch if admission fails.

Delta

LOC, member count, and type count neutral.

# 6. Inline one-use projection probes

Location: `relations.md:118`, `IntersectionHit.CanProjectTo`

From

```csharp
internal static bool CanProjectTo(Type output) => HitProjection.For(output: output).IsSome;
```

To

```csharp
// IntersectionHit.CanProjectTo DELETED
```

Location: `relations.md:264-268`, hit projection arm

From

```csharp
hits: static (o, _) => IntersectionHit.CanProjectTo(output: o));
```

To

```csharp
hits: static (o, _) => HitProjection.For(output: o).IsSome);
```

Location: `relations.md:246-251`, `IntersectionResult.CanProjectAny` and `Supports`

From

```csharp
internal static bool CanProjectAny(Type output) =>
    Seq(LinesShape, PointsShape, IntervalsShape, PolylinesShape, HitsShape).Exists(shape => shape.CanProject(output: output));
internal static bool Supports(Type left, Type right, Type output, PairOrder order) =>
    Capability.Universal(type: left) || Capability.Universal(type: right)
        ? CanProjectAny(output: output)
        : Relations.ShapeOf(left: left, right: right, output: output, order: order).IsSome;
```

To

```csharp
// IntersectionResult.CanProjectAny DELETED
internal static bool Supports(Type left, Type right, Type output, PairOrder order) =>
    Capability.Universal(type: left) || Capability.Universal(type: right)
        ? Seq(LinesShape, PointsShape, IntervalsShape, PolylinesShape, HitsShape).Exists(shape => shape.CanProject(output: output))
        : Relations.ShapeOf(left: left, right: right, output: output, order: order).IsSome;
```

Why

Both probes have one caller and only rename the owning roster or shape expression.

Change

Read the projection roster and shape fold at their sole gates.

Delta

Net -3 LOC, -2 members, neutral type count.

# 7. Inline row predicate forwards

Location: `relations.md:283-288`, `IntersectionCase.CanProject` and `Admits`

From

```csharp
internal bool CanProject(Type left, Type right, Type output) => Supports(arg1: left, arg2: right) && Shape.CanProject(output: output);
internal bool Admits(object left, object right) => Supports(arg1: left.GetType(), arg2: right.GetType());
```

To

```csharp
// IntersectionCase.CanProject DELETED
// IntersectionCase.Admits DELETED
```

Location: `relations.md:376-379`, build-time table scan

From

```csharp
IntersectionCases.Find(predicate: entry => entry.CanProject(left: row.Left, right: row.Right, output: output)).Map(static entry => entry.Shape)
```

To

```csharp
IntersectionCases.Find(predicate: entry => entry.Supports(row.Left, row.Right) && entry.Shape.CanProject(output)).Map(static entry => entry.Shape)
```

Location: `relations.md:400-405`, runtime table scan

From

```csharp
IntersectionCases.Find(predicate: row => row.Admits(left: left, right: right))
```

To

```csharp
IntersectionCases.Find(predicate: row => row.Supports(left.GetType(), right.GetType()))
```

Why

The record already exposes the predicate and shape that define a row. Both one-call methods forward those columns at a single consumer.

Change

Read the row columns directly in the build-time and runtime scans.

Delta

Net -2 LOC, -2 members, neutral type count.

# 8. Inline single-use operation gates

Location: `relations.md:525-527`, deviation build gate

From

```csharp
(CanDeviate(left: typeof(TA), right: typeof(TB)) && typeof(TOut) == typeof(CurveDeviation))
```

To

```csharp
(Capability.CurveForm.Admits(type: typeof(TA)) && Capability.CurveForm.Admits(type: typeof(TB)) && typeof(TOut) == typeof(CurveDeviation))
```

Location: `relations.md:536-538`, self-intersection build gate

From

```csharp
(CanSelfIntersect(geometry: typeof(TGeometry)) && IntersectionResult.HitsShape.CanProject(output: typeof(TOut)))
```

To

```csharp
((typeof(TGeometry) == typeof(object)
    || typeof(Curve).IsAssignableFrom(c: typeof(TGeometry))
    || typeof(Mesh).IsAssignableFrom(c: typeof(TGeometry)))
 && IntersectionResult.HitsShape.CanProject(output: typeof(TOut)))
```

Location: `relations.md:559-562`, `CanDeviate` and `CanSelfIntersect`

From

```csharp
internal static bool CanDeviate(Type left, Type right) =>
    Capability.CurveForm.Admits(type: left) && Capability.CurveForm.Admits(type: right);
internal static bool CanSelfIntersect(Type geometry) =>
    geometry == typeof(object) || typeof(Curve).IsAssignableFrom(c: geometry) || typeof(Mesh).IsAssignableFrom(c: geometry);
```

To

```csharp
// Relations.CanDeviate DELETED
// Relations.CanSelfIntersect DELETED
```

Why

Each gate has one caller and adds no policy beyond the exact capability expression the owning builder needs.

Change

Seat each admission expression at its operation builder and remove the forwarding members.

Delta

Net -1 LOC, -2 members, neutral type count.

# 9. Scan the normalized classification pair directly

Location: `relations.md:388-399`, `ClassifiedOf`

From

```csharp
body: rightCurve => IntersectionOf(left: leftCurve, right: rightCurve, env: env, op: op, order: PairOrder.Unordered)
lines: Unenriched, points: Unenriched, intervals: Unenriched, polylines: Unenriched,
private static Fin<IntersectionResult> Unenriched<TState>(TState _, IntersectionResult shape) => Fin.Succ(shape);
```

To

```csharp
body: rightCurve => Scan(left: leftCurve, right: rightCurve, env: env, op: op)
lines: static (_, shape) => Fin.Succ((IntersectionResult)shape), points: static (_, shape) => Fin.Succ((IntersectionResult)shape), intervals: static (_, shape) => Fin.Succ((IntersectionResult)shape), polylines: static (_, shape) => Fin.Succ((IntersectionResult)shape),
// Relations.Unenriched DELETED
```

Why

Both leases have already normalized the operands to `Curve`, so pair permutation and null admission cannot select another row. The remaining helper only forwards the four exhaustive non-hit arms unchanged.

Change

Enter the exact `Curve`/`Curve` scan directly and keep generated total dispatch with local identity arms.

Delta

Net -1 LOC, -1 member, neutral type count.

# 10. Remove the redundant right-side curve lowering row

Location: `relations.md:363-368`, retained left-side curve lowering row

From

```csharp
Normalization.CurveForm(source: left, key: op).Bind(lease => lease.Use(body: curve => Scan(left: curve, right: right, env: env, op: op), key: op))),
```

To

```csharp
Normalization.CurveForm(source: left, key: op).Bind(lease => lease.Use(body: curve => Scan(left: curve, right: right, env: env, op: op), key: op))));
```

Location: `relations.md:369-374`, final `IntersectionCases` row

From

```csharp
new IntersectionCase(
    Supports: static (l, r) => r != typeof(Curve) && !typeof(Curve).IsAssignableFrom(c: r) && Capability.CurveForm.Admits(type: r)
        && (Capability.CurveForm.Admits(type: l) || l == typeof(Plane) || l == typeof(Line) || typeof(Surface).IsAssignableFrom(l) || typeof(Brep).IsAssignableFrom(l) || typeof(BrepFace).IsAssignableFrom(l)),
    Shape: IntersectionResult.HitsShape,
    Compute: static (left, right, env, op) =>
        Normalization.CurveForm(source: right, key: op).Bind(lease => lease.Use(body: curve => Scan(left: left, right: curve, env: env, op: op), key: op))));
```

To

```csharp
// IntersectionCases right-side CurveForm row DELETED
```

Why

Relation pairs that permit curve-form lowering use the unordered fold. If the curve-form operand arrives on the right, the second permutation presents it to the retained left-side lowering row; ordered ray casting is consumed by the earlier `RayTarget` row and never needs either lowering row.

Change

Keep one curve-form lowering rule and let the existing unordered permutation handle operand symmetry.

Delta

Net -6 LOC and -1 table row; member and type counts neutral.

Ripples

Update the module diagram and density row from 24 intersection rows to 23.

# 11. Trust the admitted ray inside trim-aware casting

Location: `relations.md:468-475`, `TrimAwareRay` branch entry

From

```csharp
return (query.IsValid, box) switch {
    (true, { IsValid: true }) => Solved(
```

To

```csharp
return box.IsValid
    ? Solved(
```

Location: `relations.md:482-485`, `TrimAwareRay` branch exit

From

```csharp
        cancel: env.Cancellation),
    (true, _) => Fin.Fail<IntersectionResult>(op.InvalidResult()),
    _ => Fin.Fail<IntersectionResult>(op.InvalidInput()),
};
```

To

```csharp
        cancel: env.Cancellation)
    : Fin.Fail<IntersectionResult>(op.InvalidResult());
```

Why

The only callers reach this method after `Pair` has admitted the `RayQuery` through `AcceptInput` and the selected `RayTarget` has admitted its reflection ceiling. Re-reading `query.IsValid` is duplicate validation and introduces an unreachable invalid-input branch.

Change

Retain the host bounding-box result gate and remove the second request-validity decision.

Delta

Net -2 LOC; member and type counts neutral.

# 12. Admit curve deviation once through its aggregate evidence

Location: `relations.md:570-576`, successful `DeviationOf` construction

From

```csharp
true => (op.AcceptValue(value: minDistance), op.AcceptValue(value: maxDistance), op.AcceptValue(value: left.PointAt(t: minA)), op.AcceptValue(value: right.PointAt(t: minB)), op.AcceptValue(value: left.PointAt(t: maxA)), op.AcceptValue(value: right.PointAt(t: maxB)))
    .Apply((minValue, maxValue, minPointA, minPointB, maxPointA, maxPointB) => new CurveDeviation(
        MinimumDistance: minValue, MinimumA: minPointA, MinimumB: minPointB,
        MaximumDistance: maxValue, MaximumA: maxPointA, MaximumB: maxPointB,
        Band: band))
    .As()
    .Bind(deviation => deviation.IsValid ? Fin.Succ(deviation) : Fin.Fail<CurveDeviation>(op.InvalidResult())),
```

To

```csharp
true => op.AcceptValue(value: new CurveDeviation(
    MinimumDistance: minDistance, MinimumA: left.PointAt(t: minA), MinimumB: right.PointAt(t: minB),
    MaximumDistance: maxDistance, MaximumA: left.PointAt(t: maxA), MaximumB: right.PointAt(t: maxB),
    Band: band)),
```

Why

The current branch admits six fields separately and then rechecks the same facts through `CurveDeviation.IsValid`. Its existing `Nonnegative` and `Ordered` claims already include finite-number admission, so extra scalar claims would duplicate evidence rather than complete it.

Change

Construct the carrier once and pass it once through `AcceptValue`, which routes its aggregate `IValidityEvidence` through the registered oracle.

Delta

Net -3 LOC; member and type counts neutral. Removes six per-field oracle calls, the applicative tuple, and the direct `IsValid` branch.

# 13. Remove the curve-hit construction forwarder

Location: `relations.md:103,435,587-588`, `IntersectionHit.Along` and its three call shapes.

From

```csharp
public static IntersectionHit Along(Curve curve, IntersectionKind kind) => new CurveCase(Curve: curve, CurveKind: kind);
IntersectionHit.Along(curve: curve, kind: kind)
IntersectionHit.Along(curve: polyline.ToNurbsCurve(), kind: IntersectionKind.Curve)
IntersectionHit.Along(curve: polyline.ToNurbsCurve(), kind: IntersectionKind.Overlap)
```

To

```csharp
// IntersectionHit.Along DELETED
new IntersectionHit.CurveCase(Curve: curve, CurveKind: kind)
new IntersectionHit.CurveCase(Curve: polyline.ToNurbsCurve(), CurveKind: IntersectionKind.Curve)
new IntersectionHit.CurveCase(Curve: polyline.ToNurbsCurve(), CurveKind: IntersectionKind.Overlap)
```

Why

`Along` forwards both arguments unchanged to the public case and supplies no default, admission, ownership rule, or projection behavior.

Change

Construct `CurveCase` directly at the three use shapes. Retain `At` because it canonicalizes absent tangency and retain `Overlap` because it removes repeated five-field construction at two sites.

Delta

-1 LOC and -1 module-level member; call-site LOC and type count neutral.
