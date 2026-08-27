# 1. Remove unconsumed smart-enum keys without collapsing end semantics

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 44-51 and 93-101.

**From:**

Anchor: `offset.md:44-51`, the keyed `JoinType` declaration and rows.

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JoinType {
    public static readonly JoinType Miter  = new("miter", MiterCorner);
    public static readonly JoinType Round  = new("round", RoundCorner);
    public static readonly JoinType Bevel  = new("bevel", static (_, _, _, _, _) => Seq<Point3d>());
    public static readonly JoinType Square = new("square", SquareCorner);
}
```

Anchor: `offset.md:93-101`, the keyed `EndType` declaration and rows.

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EndType {
    public static readonly EndType Closed = new("closed", static (_, _, _, _) => Seq<Point3d>(), closesRibbon: true);
    public static readonly EndType Joined = new("joined", static (_, _, _, _) => Seq<Point3d>(), closesRibbon: true);
    public static readonly EndType Butt   = new("butt", static (_, _, _, _) => Seq<Point3d>(), closesRibbon: false);
    public static readonly EndType Square = new("square", SquareCap, closesRibbon: false);
    public static readonly EndType Round  = new("round", RoundCap, closesRibbon: false);
}
```

**To:**

```csharp
// JoinType.Key DELETED
[SmartEnum]
public sealed partial class JoinType {
    public static readonly JoinType Miter  = new(corner: MiterCorner);
    public static readonly JoinType Round  = new(corner: RoundCorner);
    public static readonly JoinType Bevel  = new(corner: static (_, _, _, _, _) => Seq<Point3d>());
    public static readonly JoinType Square = new(corner: SquareCorner);
}

// EndType.Key DELETED
[SmartEnum]
public sealed partial class EndType {
    public static readonly EndType Closed = new(cap: static (_, _, _, _) => Seq<Point3d>(), closesRibbon: true);
    public static readonly EndType Joined = new(cap: static (_, _, _, _) => Seq<Point3d>(), closesRibbon: true);
    public static readonly EndType Butt   = new(cap: static (_, _, _, _) => Seq<Point3d>(), closesRibbon: false);
    public static readonly EndType Square = new(cap: SquareCap, closesRibbon: false);
    public static readonly EndType Round  = new(cap: RoundCap, closesRibbon: false);
}
```

**Why:** No `libs/dotnet/` consumer reads either generated key, performs keyed lookup, or converts a row to text. Keyless Thinktecture smart enums retain row identity, `Items`, and generated dispatch while deleting key, lookup, conversion, parsing, formatting, and comparer surface. Keep both `Closed` and `Joined`: `Geometry2D/algebra.md:690` uses `Closed` as a closed-input admission marker, while `Joined` remains the distinct open-path fused-ribbon policy.

# 2. Inline the miter row's sole helper

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 48 and 56-64.

**From:**

Anchor: `offset.md:48,56-64`, `JoinType.Miter` and `MiterCorner`.

```csharp
public static readonly JoinType Miter  = new("miter", MiterCorner);

static Seq<Point3d> MiterCorner(Point3d apex, Vector3d nIn, Vector3d nOut, double distance, OffsetPolicy policy) {
    Vector3d bisector = nIn + nOut;
    double len = bisector.Length;
    if (len <= EpsilonPolicy.ZeroTolerance) { return Seq<Point3d>(); }
    double reach = distance / (0.5 * len);
    return reach <= policy.MiterLimit.Value * Math.Abs(distance)
        ? Seq(apex + (reach / len) * bisector)
        : Seq<Point3d>();
}
```

**To:**

```csharp
public static readonly JoinType Miter = new(corner: static (apex, nIn, nOut, distance, policy) => {
    Vector3d bisector = nIn + nOut;
    double len = bisector.Length;
    if (len <= EpsilonPolicy.ZeroTolerance) return Seq<Point3d>();
    double reach = distance / (0.5 * len);
    return reach <= policy.MiterLimit.Value * Math.Abs(distance)
        ? Seq(apex + (reach / len) * bisector)
        : Seq<Point3d>();
});
// MiterCorner DELETED
```

**Why:** The generated `Corner` delegate is the behavior owner and the only caller. Row-localizing the body removes one private member and one name-resolution hop without weakening the miter-limit gate.

# 3. Inline the round row's sole helper

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 49 and 66-76.

**From:**

Anchor: `offset.md:49,66-76`, `JoinType.Round` and `RoundCorner`.

```csharp
public static readonly JoinType Round  = new("round", RoundCorner);

static Seq<Point3d> RoundCorner(Point3d apex, Vector3d nIn, Vector3d nOut, double distance, OffsetPolicy policy) {
    double cross = (nIn.X * nOut.Y) - (nIn.Y * nOut.X);
    double sweep = Math.Atan2(Math.Abs(cross), (nIn.X * nOut.X) + (nIn.Y * nOut.Y));
    double turn = cross < 0.0 ? -1.0 : 1.0;
    Vector3d perp = new(-turn * nIn.Y, turn * nIn.X, 0.0);
    int steps = int.Max(1, (int)Math.Ceiling(sweep / (2.0 * Math.Acos(double.Clamp(1.0 - (policy.ArcBand / Math.Abs(distance)), -1.0, 1.0)))));
    return toSeq(Enumerable.Range(1, steps - 1).Select(i => {
        double t = sweep * i / steps;
        return apex + (distance * ((Math.Cos(t) * nIn) + (Math.Sin(t) * perp)));
    }));
}
```

**To:**

```csharp
public static readonly JoinType Round = new(corner: static (apex, nIn, nOut, distance, policy) => {
    double cross = (nIn.X * nOut.Y) - (nIn.Y * nOut.X);
    double sweep = Math.Atan2(Math.Abs(cross), (nIn.X * nOut.X) + (nIn.Y * nOut.Y));
    double turn = cross < 0.0 ? -1.0 : 1.0;
    Vector3d perp = new(-turn * nIn.Y, turn * nIn.X, 0.0);
    int steps = int.Max(1, (int)Math.Ceiling(sweep / (2.0 * Math.Acos(double.Clamp(1.0 - (policy.ArcBand / Math.Abs(distance)), -1.0, 1.0)))));
    return toSeq(Enumerable.Range(1, steps - 1).Select(i => apex + distance
        * ((Math.Cos(sweep * i / steps) * nIn) + (Math.Sin(sweep * i / steps) * perp))));
});
// RoundCorner DELETED
```

**Why:** The helper is referenced only by its generated row. Inlining removes that member and its single-use `t` local while retaining the same sample count and arc-band derivation.

# 4. Inline the square row's sole helper

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 51 and 78-90.

**From:**

Anchor: `offset.md:51,78-90`, `JoinType.Square` and `SquareCorner`.

```csharp
public static readonly JoinType Square = new("square", SquareCorner);

static Seq<Point3d> SquareCorner(Point3d apex, Vector3d nIn, Vector3d nOut, double distance, OffsetPolicy policy) {
    Vector3d bisector = nIn + nOut;
    double len = bisector.Length;
    if (len <= EpsilonPolicy.ZeroTolerance) { return Seq<Point3d>(); }
    (double bx, double by) = (bisector.X / len, bisector.Y / len);
    double cos = (nIn.X * bx) + (nIn.Y * by);
    double dotIn = (-nIn.Y * bx) + (nIn.X * by);
    double dotOut = (-nOut.Y * bx) + (nOut.X * by);
    if (Math.Abs(dotIn) <= EpsilonPolicy.ZeroTolerance || Math.Abs(dotOut) <= EpsilonPolicy.ZeroTolerance) { return Seq<Point3d>(); }
    return Seq(
        apex + (distance * nIn) + ((distance * (1.0 - cos) / dotIn) * new Vector3d(-nIn.Y, nIn.X, 0.0)),
        apex + (distance * nOut) + ((distance * (1.0 - cos) / dotOut) * new Vector3d(-nOut.Y, nOut.X, 0.0)));
}
```

**To:**

```csharp
public static readonly JoinType Square = new(corner: static (apex, nIn, nOut, distance, _) => {
    Vector3d bisector = nIn + nOut;
    double len = bisector.Length;
    if (len <= EpsilonPolicy.ZeroTolerance) return Seq<Point3d>();
    (double bx, double by) = (bisector.X / len, bisector.Y / len);
    double cos = (nIn.X * bx) + (nIn.Y * by);
    double dotIn = (-nIn.Y * bx) + (nIn.X * by), dotOut = (-nOut.Y * bx) + (nOut.X * by);
    return Math.Abs(dotIn) <= EpsilonPolicy.ZeroTolerance || Math.Abs(dotOut) <= EpsilonPolicy.ZeroTolerance
        ? Seq<Point3d>()
        : Seq(apex + distance * nIn + distance * (1.0 - cos) / dotIn * new Vector3d(-nIn.Y, nIn.X, 0.0),
              apex + distance * nOut + distance * (1.0 - cos) / dotOut * new Vector3d(-nOut.Y, nOut.X, 0.0));
});
// SquareCorner DELETED
```

**Why:** `SquareCorner` is a one-row implementation and its policy parameter is unused. The row-local delegate removes both the private symbol and the misleading named parameter use.

# 5. Inline the two sole cap helpers

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 100-113.

**From:**

Anchor: `offset.md:100-113`, the square and round cap rows plus their helpers.

```csharp
public static readonly EndType Square = new("square", SquareCap, closesRibbon: false);
public static readonly EndType Round  = new("round", RoundCap, closesRibbon: false);

static Seq<Point3d> SquareCap(Point3d end, Vector3d tangent, double distance, OffsetPolicy policy) =>
    Seq(end + distance * new Vector3d(tangent.Y, -tangent.X, 0.0) + distance * tangent,
        end + distance * new Vector3d(-tangent.Y, tangent.X, 0.0) + distance * tangent);

static Seq<Point3d> RoundCap(Point3d end, Vector3d tangent, double distance, OffsetPolicy policy) =>
    JoinType.Round.Corner(end, new Vector3d(tangent.Y, -tangent.X, 0.0), new Vector3d(-tangent.Y, tangent.X, 0.0), distance, policy);
```

**To:**

```csharp
public static readonly EndType Square = new(cap: static (end, tangent, distance, _) =>
    Seq(end + distance * new Vector3d(tangent.Y, -tangent.X, 0.0) + distance * tangent,
        end + distance * new Vector3d(-tangent.Y, tangent.X, 0.0) + distance * tangent), closesRibbon: false);
public static readonly EndType Round = new(cap: static (end, tangent, distance, policy) =>
    JoinType.Round.Corner(end, new Vector3d(tangent.Y, -tangent.X, 0.0),
        new Vector3d(-tangent.Y, tangent.X, 0.0), distance, policy), closesRibbon: false);
// SquareCap DELETED
// RoundCap DELETED
```

**Why:** Each helper exists only to populate one generated delegate column. Inlining removes two private members and keeps the round cap composed through the one round-corner behavior.

# 6. Derive the fixed tolerance lanes instead of storing them

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 117-132.

**From:**

Anchor: `offset.md:117-132`, the policy's lane fields, factory assignments, and band projections.

```csharp
public sealed record OffsetPolicy(
    Context Context, PositiveMagnitude TimeBudget, Dimension MaxEvents, Dimension SameTimeMultiple,
    ArenaPolicy Arena, Dimension NearestCandidates,
    ToleranceLane Collapse, ToleranceLane Arc, PositiveMagnitude MiterLimit, Arr<double> EdgeSpeed = default) {
    public static OffsetPolicy Of(Context context, Option<PositiveMagnitude> timeBudget = default,
        Option<Dimension> maxEvents = default, Option<ArenaPolicy> arena = default) => new(
        Context: context,
        TimeBudget: timeBudget.IfNone(noneValue: PositiveMagnitude.Create(value: 1e9)),
        MaxEvents: maxEvents.IfNone(noneValue: Dimension.Create(value: 1 << 20)),
        SameTimeMultiple: Dimension.Create(value: 4),
        Arena: arena.IfNone(noneValue: ArenaPolicy.Canonical),
        NearestCandidates: Dimension.Create(value: 16),
        Collapse: ToleranceLane.Collapse, Arc: ToleranceLane.Arc, MiterLimit: PositiveMagnitude.Create(value: 2.0));

    public double CollapseBand => Context.For(Collapse).Value;
    public double ArcBand => Context.For(Arc).Value;
```

**To:**

```csharp
public sealed record OffsetPolicy(
    Context Context, PositiveMagnitude TimeBudget, Dimension MaxEvents, Dimension SameTimeMultiple,
    ArenaPolicy Arena, Dimension NearestCandidates, PositiveMagnitude MiterLimit, Arr<double> EdgeSpeed = default) {
    // OffsetPolicy.Collapse DELETED
    // OffsetPolicy.Arc DELETED
    public static OffsetPolicy Of(Context context, Option<PositiveMagnitude> timeBudget = default,
        Option<Dimension> maxEvents = default, Option<ArenaPolicy> arena = default) => new(
        context, timeBudget.IfNone(PositiveMagnitude.Create(1e9)),
        maxEvents.IfNone(Dimension.Create(1 << 20)), Dimension.Create(4),
        arena.IfNone(ArenaPolicy.Canonical), Dimension.Create(16), PositiveMagnitude.Create(2.0));

    public double CollapseBand => Context.For(ToleranceLane.Collapse).Value;
    public double ArcBand => Context.For(ToleranceLane.Arc).Value;
```

**Why:** Both lane identities are algorithm invariants. Storing them adds two public record members and two caller-overridable `with` slots whose only lawful values are already known. Direct lane reads retain per-context overrides while removing the duplicate axes.

**Ripples:** Replace `OffsetPolicy.Canonical` in `Geometry2D/algebra.md`, `Verify/audit.md`, `Spec/manufacturability.md`, `Toolpath/motion.md`, `Nesting/linking.md`, and `Additive/slicing.md` with `OffsetPolicy.Of(Context.Canonical)`. Where a caller customizes the arc band, bind `Context.Override(ToleranceLane.Arc, value, LengthUnit.Millimeter)` before `OffsetPolicy.Of`. In `Additive/scanpath.md` and `Additive/support.md`, hash `policy.Context.For(ToleranceLane.Collapse).Value` and `.For(ToleranceLane.Arc).Value` directly.

# 7. Keep weighted skeleton semantics but move speeds onto its request

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 117-120, 263-281, and 287-292.

**From:**

Anchor: `offset.md:117-120`, edge speeds stored on the run policy.

```csharp
ArenaPolicy Arena, Dimension NearestCandidates,
ToleranceLane Collapse, ToleranceLane Arc, PositiveMagnitude MiterLimit, Arr<double> EdgeSpeed = default) {
```

Anchor: `offset.md:263-281`, the weighted case carries no weighted evidence while the unconsumed forwarding projection repeats every case.

```csharp
public sealed record Skeleton(Polyline Ring, OffsetPolicy Policy) : OffsetOp;
public sealed record Weighted(Polyline Ring, OffsetPolicy Policy) : OffsetOp;

public OffsetPolicy Policy =>
    Switch(
        skeleton:  static s => s.Policy,
        weighted:  static w => w.Policy,
        offset:    static o => o.Policy,
        medial:    static m => m.Policy,
        minkowski: static k => k.Policy,
        clearance: static c => c.Policy);
```

Anchor: `offset.md:287-292`, separate uniform and weighted dispatch arms.

```csharp
skeleton:  s => AdmitRing(s.Ring, s.Policy, site).Bind(admitted => Propagate(admitted, s.Policy, Arr<double>.Empty)).Map(static t => (OffsetResult)new OffsetResult.Graph(t.Graph)),
weighted:  w => AdmitRing(w.Ring, w.Policy, site)
    .Bind(admitted => AdmitSpeeds(w.Policy.EdgeSpeed, admitted.Edges.Count)
        .Bind(speeds => Propagate(admitted, w.Policy, Orientation(w.Ring) == Sign.Negative ? ReversedSpeeds(speeds) : speeds)))
    .Map(static t => (OffsetResult)new OffsetResult.Graph(t.Graph)),
```

**To:**

```csharp
ArenaPolicy Arena, Dimension NearestCandidates, PositiveMagnitude MiterLimit) {
    // OffsetPolicy.EdgeSpeed DELETED

public sealed record Skeleton(Polyline Ring, OffsetPolicy Policy) : OffsetOp;
public sealed record Weighted(Polyline Ring, Arr<double> EdgeSpeed, OffsetPolicy Policy) : OffsetOp;
// OffsetOp.Policy DELETED

skeleton: s => AdmitRing(s.Ring, s.Policy, site)
    .Bind(admitted => Propagate(admitted, s.Policy, Arr<double>.Empty))
    .Map(static trace => (OffsetResult)new OffsetResult.Graph(trace.Graph)),
weighted: w => AdmitRing(w.Ring, w.Policy, site)
    .Bind(admitted => AdmitSpeeds(w.EdgeSpeed, admitted.Edges.Count)
        .Bind(speeds => Propagate(admitted, w.Policy,
            Orientation(w.Ring) == Sign.Negative ? speeds.Reverse() : speeds)))
    .Map(static trace => (OffsetResult)new OffsetResult.Graph(trace.Graph)),
```

**Why:** `EdgeSpeed` is operation input, not reusable run policy. Keep `Weighted` because it has a genuinely distinct payload and radius semantics: an empty optional speed row on `Skeleton` would erase that discriminant and admit a malformed weighted request as uniform. Moving the table onto the weighted case removes the mutable policy axis and the unused six-arm `Policy` projection without deleting real weighted-skeleton capability.

**Ripples:** Remove `EdgeSpeed` from policy hashing in `Additive/scanpath.md:1306-1308` and `Additive/support.md:1172-1174`. `Geometry2D/algebra.md:453-456` currently sends a variable-offset request through `OffsetOp.Weighted` and then asks `ChainsOf` for curves, but `Weighted` returns a `SkeletonGraph`, so that arm deterministically faults today. Do not rename that call to `Skeleton`; widen the existing `OffsetOp.Offset` curve modality to carry the variable edge-distance evidence and lower this arm through it, while `Weighted` remains the genuine weighted-skeleton request.

# 8. Inline finite weighted-speed admission and use the existing Arr reversal

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 289-292, 302-305, and 396-400.

**From:**

Anchor: `offset.md:289-292`, the weighted arm is the helper's only caller.

```csharp
weighted:  w => AdmitRing(w.Ring, w.Policy, site)
    .Bind(admitted => AdmitSpeeds(w.Policy.EdgeSpeed, admitted.Edges.Count)
        .Bind(speeds => Propagate(admitted, w.Policy, Orientation(w.Ring) == Sign.Negative ? ReversedSpeeds(speeds) : speeds)))
    .Map(static t => (OffsetResult)new OffsetResult.Graph(t.Graph)),
```

Anchor: `offset.md:302-305`, the one-call admission helper accepts positive infinity.

```csharp
static Fin<Arr<double>> AdmitSpeeds(Arr<double> speeds, int edges) =>
    speeds.Count == edges && speeds.ForAll(static speed => speed > 0.0)
        ? Fin.Succ(speeds)
        : Fin.Fail<Arr<double>>(new GeometryFault.DegenerateOffset(speeds.Count));
```

Anchor: `offset.md:396-400`, the manual array reversal.

```csharp
static Arr<double> ReversedSpeeds(Arr<double> speeds) {
    double[] flipped = new double[speeds.Count];
    for (int e = 0; e < flipped.Length; e++) { flipped[e] = speeds[speeds.Count - 1 - e]; }
    return Arr.create<double>(flipped);
}
```

**To:**

```csharp
weighted: w => AdmitRing(w.Ring, w.Policy, site)
    .Bind(admitted => w.EdgeSpeed.Count == admitted.Edges.Count
        && w.EdgeSpeed.ForAll(static speed => double.IsFinite(speed) && speed > 0.0)
            ? Propagate(admitted, w.Policy, Orientation(w.Ring) == Sign.Negative
                ? w.EdgeSpeed.Reverse()
                : w.EdgeSpeed)
            : Fin.Fail<Trace>(new GeometryFault.DegenerateOffset(w.EdgeSpeed.Count)))
    .Map(static trace => (OffsetResult)new OffsetResult.Graph(trace.Graph)),

// AdmitSpeeds DELETED
// ReversedSpeeds DELETED
```

**Why:** Positive infinity passes the old predicate and later contaminates event times and vertices. The weighted arm is the only caller, so keeping `AdmitSpeeds` as a private forwarding gate adds a symbol without owning an independently reusable rule. Inlining the finite-positive predicate at the admission boundary and using LanguageExt's catalogued `Arr.Reverse()` removes that helper and the manual copy loop while preserving the original-edge ordering law.

# 9. Derive weighted-event semantics from admitted speed evidence

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 133 and 420-445.

**From:**

Anchor: `offset.md:133`, the policy projection duplicates the speed row's state.

```csharp
public bool Weighted => EdgeSpeed.Count > 0;
```

Anchor: `offset.md:420-445`, both event handlers consult the policy rather than their admitted speeds.

```csharp
nodes.Add(new ClearanceNode(meet, policy.Weighted ? radius : ev.Time, witness));
```

```csharp
nodes.Add(new ClearanceNode(hit, policy.Weighted ? radius : ev.Time, witness));
```

**To:**

```csharp
// OffsetPolicy.Weighted DELETED

nodes.Add(new ClearanceNode(meet, speeds.Count == 0 ? ev.Time : radius, witness));
```

```csharp
nodes.Add(new ClearanceNode(hit, speeds.Count == 0 ? ev.Time : radius, witness));
```

**Why:** Weightedness is request evidence, not run policy. The admitted propagation row already carries the distinction at both uses, so deriving it removes a public member and prevents a policy flag from drifting away from the actual speeds. `Count` is the catalogue-backed `Arr` observation already used throughout the target; an unverified `IsEmpty` spelling does not enter the replacement.

# 10. Let the result union carry its three real payloads directly

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 188, 252-260, 287-299, 503, and 620-651.

**From:**

Anchor: `offset.md:188`, the curve carrier whose distance is never read.

```csharp
public sealed record OffsetCurves(Seq<Chain> Loops, double Distance);
```

Anchor: `offset.md:252-260`, four wrapper types around three payload shapes.

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetResult {
    private OffsetResult() { }

    public sealed record Graph(SkeletonGraph Skeleton) : OffsetResult;
    public sealed record Axis(SkeletonGraph Medial) : OffsetResult;
    public sealed record Curves(OffsetCurves Offset) : OffsetResult;
    public sealed record Probe(ClearanceNode Node) : OffsetResult;
}
```

Anchor: `offset.md:287-299`, wrapper construction in the operation fold.

```csharp
.Map(static t => (OffsetResult)new OffsetResult.Graph(t.Graph)),
.Map(static axis => (OffsetResult)new OffsetResult.Axis(axis)),
.Map(static loops => (OffsetResult)new OffsetResult.Curves(loops)),
return (OffsetResult)new OffsetResult.Probe(new ClearanceNode(c.Probe, radius, edge));
```

Anchor: `offset.md:503,620-651`, the two curve-producing paths wrap the same `Seq<Chain>` and carry an unconsumed distance.

```csharp
.Map(chains => (OffsetResult)new OffsetResult.Curves(new OffsetCurves(chains, op.Distance)));

static Fin<OffsetCurves> Convolve(Polyline ring, Polyline element, Op key) {
```

**To:**

```csharp
// OffsetCurves DELETED
// OffsetResult.Graph DELETED
// OffsetResult.Axis DELETED
// OffsetResult.Curves DELETED
// OffsetResult.Probe DELETED
[Union<SkeletonGraph, Seq<Chain>, ClearanceNode>(
    T1Name = "Graph", T2Name = "Curves", T3Name = "Probe")]
public readonly partial struct OffsetResult;

.Map(static trace => (OffsetResult)trace.Graph),
.Map(static graph => (OffsetResult)graph),
.Map(static loops => (OffsetResult)loops),
return (OffsetResult)new ClearanceNode(c.Probe, radius, edge);
```

```csharp
.Map(chains => (OffsetResult)chains);

static Fin<Seq<Chain>> Convolve(Polyline ring, Polyline element, Op key) {
    if (element.Count < 4 || !element.IsClosed)
        return new GeometryFault.DegenerateOffset(0);
```

```csharp
if (Predicate.Orient2D(b[(j - 1 + en) % en], b[j], b[(j + 1) % en]) == Sign.Negative)
    return new GeometryFault.DegenerateOffset(j);
```

```csharp
    return Arrangement.Apply(new ArrangementOp.PlanarOverlay(
            Seq(cycle), Seq<Polyline>(), BooleanOp.Union, Axis.Z, ArrangementPolicy.Canonical), key)
        .Bind(static result => result is ArrangementResult.Overlay overlay
            ? Fin.Succ(overlay.Loops)
            : Fin.Fail<Seq<Chain>>(new GeometryFault.DegenerateOffset(0)));
```

**Why:** The nested records add four types without invariants, and `OffsetCurves` adds a fifth wrapper around `Seq<Chain>` plus a `Distance` no consumer reads. `Graph` and `Axis` are request-origin labels over the same `SkeletonGraph`, so their distinction disappears once the caller knows which request it sent. The Thinktecture ad-hoc union keeps exhaustive dispatch over the three genuine result values while deleting all five wrappers.

**Ripples:** Rebind `Geometry2D/algebra.md:694-699` `ChainsOf` to the generated `graph`, `curves`, and `probe` arms, returning the `curves` sequence directly. Replace the partial `OffsetResult.Axis` tests in `Verify/audit.md:1102-1106`, `Spec/manufacturability.md:1165-1173`, and `Toolpath/motion.md:1278-1287` with total generated `Switch` calls: the `graph` arm continues the medial fold and both non-graph arms return the existing invalid-result fault. Update target prose, Mermaid, and density rows from `OffsetCurves`/`Graph`+`Axis` to the three payload arms.

# 11. Return private pipeline state as named tuples

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 184, 239, 307-315, and 349-380.

**From:**

Anchor: `offset.md:184`, the admission transport record.

```csharp
public sealed record AdmittedRing(Polyline Ring, ClearanceProbe Edges);
```

Anchor: `offset.md:239`, the propagation transport record.

```csharp
public readonly record struct Trace(WavefrontStore Store, SkeletonGraph Graph);
```

Anchor: `offset.md:307-315`, admission constructs its one private transport value.

```csharp
static Fin<AdmittedRing> AdmitRing(Polyline ring, OffsetPolicy policy, Op key) {
```

```csharp
return SelfCrossing(edges, oriented, policy, key).Map(_ => new AdmittedRing(oriented, edges));
```

Anchor: `offset.md:349-380`, propagation constructs its one private transport value.

```csharp
static Fin<Trace> Propagate(AdmittedRing admitted, OffsetPolicy policy, Arr<double> speeds, double until = double.PositiveInfinity) {
```

```csharp
return Fin.Succ(new Trace(store, new SkeletonGraph(toSeq(nodes), toSeq(arcs))));
```

**To:**

```csharp
// AdmittedRing DELETED
// Trace DELETED

static Fin<(Polyline Ring, ClearanceProbe Edges)> AdmitRing(
    Polyline ring, OffsetPolicy policy, Op key) {
```

```csharp
return SelfCrossing(edges, oriented, policy, key).Map(_ => (oriented, edges));
```

```csharp
static Fin<(WavefrontStore Store, SkeletonGraph Graph)> Propagate(
    (Polyline Ring, ClearanceProbe Edges) admitted,
    OffsetPolicy policy, Arr<double> speeds, double until = double.PositiveInfinity) {
```

```csharp
return (store, new SkeletonGraph(toSeq(nodes), toSeq(arcs)));
```

**Why:** Both records are local transport shapes with no invariant or consumer outside `Offsetting`. Named tuples preserve the field-oriented call sites while removing two module-level nominal symbols.

# 12. Store clearance geometry once as lines behind a closed constructor

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 139-146 and 318-324.

**From:**

Anchor: `offset.md:139-146`, a public positional record exposes two independently constructible endpoint arrays and index state.

```csharp
public sealed record ClearanceProbe(Arr<Point3d> From, Arr<Point3d> To, Option<SpatialIndex> Index, Dimension Ceiling) {
    public int Count => From.Count;

    public static ClearanceProbe Of(Arr<Point3d> from, Arr<Point3d> to, Dimension ceiling) {
        BoundingBox[] boxes = new BoundingBox[from.Count];
        for (int s = 0; s < boxes.Length; s++) { boxes[s] = new BoundingBox([from[s], to[s]]); }
        return new ClearanceProbe(From: from, To: to, Index: Seated(boxes), Ceiling: ceiling);
    }
```

Anchor: `offset.md:318-324`, ring admission materializes parallel endpoint arrays.

```csharp
static ClearanceProbe EdgesOf(Polyline ring, OffsetPolicy policy) {
    int n = ring.Count - 1;
    return ClearanceProbe.Of(
        from: new Arr<Point3d>([.. Enumerable.Range(0, n).Select(e => ring[e])]),
        to: new Arr<Point3d>([.. Enumerable.Range(0, n).Select(e => ring[e + 1])]),
        ceiling: policy.NearestCandidates);
}
```

**To:**

```csharp
public sealed class ClearanceProbe {
    readonly Arr<Line> segments;
    readonly Option<SpatialIndex> index;
    readonly Dimension ceiling;

    ClearanceProbe(Arr<Line> segments, Option<SpatialIndex> index, Dimension ceiling) =>
        (this.segments, this.index, this.ceiling) = (segments, index, ceiling);
    internal int Count => segments.Count;
    internal Dimension Ceiling => ceiling;
    internal Line this[int segment] => segments[segment];

    public static ClearanceProbe Of(Arr<Line> segments, Dimension ceiling) {
        BoundingBox[] boxes = [.. segments.Map(static segment => segment.BoundingBox)];
        Option<SpatialIndex> index = Spatial.Apply(
                new SpatialOp.Build(SpatialKind.Bvh, boxes, BuildPolicy.Canonical))
            .ToOption()
            .Bind(static answer => answer is SpatialAnswer.Index seated ? Some(seated.Value) : None);
        return new(segments, index, ceiling);
    }

    // ClearanceProbe.Seated DELETED
```

```csharp
internal IEnumerable<(int I, int J)> Overlaps(double tolerance) =>
    index.Bind(seated => Spatial.Apply(new SpatialOp.Query(
        seated, new SpatialQuery.SelfOverlap(tolerance))).ToOption())
        .Bind(static answer => answer is SpatialAnswer.Result { Value: QueryResult.Pairs pairs }
            ? Some(pairs.Overlaps) : None)
        .Map(static pairs => pairs.AsEnumerable().Select(static pair =>
            (int.Min(pair.Left, pair.Right), int.Max(pair.Left, pair.Right))))
        .IfNone(() => from i in Enumerable.Range(0, Count)
            from j in Enumerable.Range(i + 1, Count - i - 1) select (i, j));

IEnumerable<int> Ordered(Point3d probe, int candidates) =>
    index.Bind(seated => Spatial.Apply(new SpatialOp.Query(
        seated, new SpatialQuery.Nearest(probe, candidates))).ToOption())
        .Bind(static answer => answer is SpatialAnswer.Result { Value: QueryResult.Nearest ranked }
            ? Some(ranked.Ordered) : None)
        .Map(static ranked => ranked.AsEnumerable())
        .IfNone(() => Enumerable.Range(0, Count));

double BoxReach(int segment, Point3d probe) =>
    index.Map(seated => seated.Primitives[segment].ClosestPoint(probe).DistanceTo(probe))
        .IfNone(noneValue: 0.0);
```

```csharp
static ClearanceProbe EdgesOf(Polyline ring, OffsetPolicy policy) =>
    ClearanceProbe.Of(new Arr<Line>([.. Enumerable.Range(0, ring.Count - 1)
        .Select(edge => new Line(ring[edge], ring[edge + 1]))]), policy.NearestCandidates);
```

**Why:** A segment is already a Rhino `Line`. Parallel start/end arrays expose mismatched lengths and public index construction as representable invalid states, then reconstruct the same lines in consumers. One private `Arr<Line>` removes four public positional properties and makes `Of` the sole constructor boundary. `Count`, `Ceiling`, and the indexer remain internal observations because the offset kernel and `Meshing/skeleton` genuinely consume them; the spatial index itself remains private. Folding the one-call `Seated` wrapper into that boundary also leaves index construction at the only site that owns its boxes.

**Ripples:** Change `Meshing/skeleton.md:455` to pass its already-paired reach edges as `Arr<Line>` to `ClearanceProbe.Of`. Its `skeleton.Reach.Ceiling` read at line 477 continues through the retained internal `Ceiling` projection; `Nearest` remains the sibling module's only public query.

# 13. Inline candidate ordering and use the stored line for closest-point projection

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 148-177.

**From:**

Anchor: `offset.md:148-160`, nearest search rebuilds vector arithmetic from the two endpoint arrays.

```csharp
foreach (int s in Ordered(probe, ceiling)) {
    if (BoxReach(s, probe) >= best) { closed = true; break; }
    Vector3d d = To[s] - From[s];
    double t = d.SquareLength <= EpsilonPolicy.ZeroTolerance
        ? 0.0
        : Math.Clamp(((probe - From[s]) * d) / d.SquareLength, 0.0, 1.0);
    double dist = probe.DistanceTo(From[s] + (t * d));
    if (dist < best) { (best, at, foot) = (dist, s, t); }
}
```

Anchor: `offset.md:170-177`, the candidate-order and box-reach helpers each have one caller.

```csharp
IEnumerable<int> Ordered(Point3d probe, int ceiling) =>
    Index.Bind(index => Spatial.Apply(new SpatialOp.Query(index, new SpatialQuery.Nearest(probe, ceiling))).ToOption())
        .Bind(static answer => answer is SpatialAnswer.Result { Value: QueryResult.Nearest ranked } ? Some(ranked.Ordered) : None)
        .Map(static ranked => ranked.AsEnumerable())
        .IfNone(() => Enumerable.Range(0, Count));

double BoxReach(int segment, Point3d probe) =>
    Index.Map(index => index.Primitives[segment].ClosestPoint(probe).DistanceTo(probe)).IfNone(noneValue: 0.0);
```

**To:**

```csharp
public (double Distance, int Segment, double Parameter) Nearest(Point3d probe) {
    for (int candidates = int.Min(ceiling.Value, Count); ;
        candidates = int.Min(candidates << 1, Count)) {
        (double best, int at, double foot, bool closed) = (double.PositiveInfinity, 0, 0.0, false);
        IEnumerable<int> ordered = index.Bind(seated => Spatial.Apply(new SpatialOp.Query(
                seated, new SpatialQuery.Nearest(probe, candidates))).ToOption())
            .Bind(static answer => answer is SpatialAnswer.Result { Value: QueryResult.Nearest ranked }
                ? Some(ranked.Ordered) : None)
            .Map(static ranked => ranked.AsEnumerable())
            .IfNone(() => Enumerable.Range(0, Count));
        foreach (int s in ordered) {
            if (index.Map(seated => seated.Primitives[s].ClosestPoint(probe, true).DistanceTo(probe))
                    .IfNone(noneValue: 0.0) >= best) { closed = true; break; }
            Line segment = segments[s];
            double t = segment.Length <= EpsilonPolicy.ZeroTolerance
                ? 0.0 : Math.Clamp(segment.ClosestParameter(probe), 0.0, 1.0);
            double distance = segment.PointAt(t).DistanceTo(probe);
            if (distance < best) (best, at, foot) = (distance, s, t);
        }
        if (closed || candidates >= Count) return (best, at, foot);
    }
}

// ClearanceProbe.Ordered DELETED
// ClearanceProbe.BoxReach DELETED
```

**Why:** Rhino's catalogued `Line.ClosestParameter(Point3d)` and `PointAt(double)` own parameter projection and evaluation, while `BoundingBox.ClosestPoint(Point3d, bool)` with interior inclusion yields the required zero lower bound when the probe lies inside the box. `MinimumDistanceTo` is not valid here: for an interior probe it measures to the box boundary and can exceed the distance to a segment crossing the box, causing an unsound early stop. The zero-length guard remains because `Meshing/skeleton` deliberately represents isolated nodes as degenerate lines. Inlining the sole `Ordered` call and deleting `BoxReach` removes two private members without weakening pruning or parameter evidence.

# 14. Preserve intersection faults separately from crossing evidence

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 315, 326-335, and 579-586.

**From:**

Anchor: `offset.md:315`, admission expects only success/failure from simplicity checking.

```csharp
return SelfCrossing(edges, oriented, policy, key).Map(_ => new AdmittedRing(oriented, edges));
```

Anchor: `offset.md:326-335`, the loop reads `Fin.Case`, treating an intersection failure as no hit.

```csharp
static Fin<Unit> SelfCrossing(ClearanceProbe edges, Polyline ring, OffsetPolicy policy, Op key) {
    int n = edges.Count;
    foreach ((int i, int j) in edges.Overlaps(policy.CollapseBand)) {
        if (j - i < 2 || (i == 0 && j == n - 1)) { continue; }
        Fin<IntersectResult> hit = Intersection.Apply(
            new IntersectOp.SegmentSegment(new Line(ring[i], ring[i + 1]), new Line(ring[j], ring[j + 1]), Axis.Z, IntersectPolicy.Canonical), key);
        if (hit.Case is IntersectResult.Points { Hits.IsEmpty: false }) { return Fail<Unit>(i); }
    }
    return Fin.Succ(unit);
}
```

Anchor: `offset.md:579-586`, loop resolution interprets every failure as overlap.

```csharp
bool overlapping = loops.Exists(loop => SelfCrossing(EdgesOf(loop, policy), loop, policy, key).IsFail);
return overlapping
    ? Arrangement.Apply(new ArrangementOp.PlanarOverlay(loops, Seq<Polyline>(), BooleanOp.Union, Axis.Z, ArrangementPolicy.Canonical), key)
        .Bind(static result => result is ArrangementResult.Overlay overlay
            ? Fin.Succ(overlay.Loops)
            : Fin.Fail<Seq<Chain>>(new GeometryFault.DegenerateOffset(0)))
    : Fin.Succ(loops.Map(static loop => new Chain(loop, Closed: true)));
```

**To:**

```csharp
return SelfCrossing(edges, policy, key).Bind(crossing => crossing.Match(
    Some: vertex => Fail<(Polyline Ring, ClearanceProbe Edges)>(vertex),
    None: () => Fin.Succ((oriented, edges))));
```

```csharp
static Fin<Option<int>> SelfCrossing(ClearanceProbe edges, OffsetPolicy policy, Op key) =>
    toSeq(edges.Overlaps(policy.CollapseBand)
        .Where(pair => pair.J - pair.I >= 2 && (pair.I != 0 || pair.J != edges.Count - 1)))
    .TraverseM(pair => Intersection.Apply(new IntersectOp.SegmentSegment(
            edges[pair.I], edges[pair.J], Axis.Z, IntersectPolicy.Canonical), key)
        .Map(result => result.Switch(
            points: points => points.Hits.IsEmpty ? Option<int>.None : Some(pair.I),
            segments: segments => segments.Crossings.IsEmpty ? Option<int>.None : Some(pair.I),
            chains: chains => chains.Walked.IsEmpty ? Option<int>.None : Some(pair.I))))
    .As()
    .Map(crossings => crossings.Find(static crossing => crossing.IsSome)
        .Bind(static crossing => crossing));
```

```csharp
return loops.TraverseM(loop => SelfCrossing(EdgesOf(loop, policy), policy, key)).As()
    .Bind(crossings => crossings.Exists(static crossing => crossing.IsSome)
        ? Arrangement.Apply(new ArrangementOp.PlanarOverlay(
            loops, Seq<Polyline>(), BooleanOp.Union, Axis.Z, ArrangementPolicy.Canonical), key)
            .Bind(static result => result is ArrangementResult.Overlay overlay
                ? Fin.Succ(overlay.Loops)
                : Fin.Fail<Seq<Chain>>(new GeometryFault.DegenerateOffset(0)))
        : Fin.Succ(loops.Map(static loop => new Chain(loop, Closed: true))));
```

**Why:** `Fin.Case` erases an `Intersection.Apply` failure by letting it fall through as a clear pair, while `Resolve` later treats every failure as proof of overlap. `Fin<Option<int>>` keeps fault and absence distinct, `TraverseM(...).As()` short-circuits the first real fault, and the generated exhaustive `IntersectResult.Switch` removes the partial runtime type test.

# 15. Reject tolerance-collapsed input edges during admission

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 307-315, 337-343, and 494-502.

**From:**

Anchor: `offset.md:307-315`, ring admission checks finiteness but not adjacent edge length.

```csharp
for (int i = 0; i < ring.Count; i++) {
    if (!ValidityClaim.Finite(ring[i])) { return Fail<AdmittedRing>(i); }
}
```

Anchor: `offset.md:337-343`, open-path admission rejects only exact duplicate neighbors and has no policy.

```csharp
static Fin<Polyline> AdmitPath(Polyline path) {
    if (path.Count < 2) { return Fail<Polyline>(0); }
    for (int i = 0; i < path.Count; i++) {
        if (!ValidityClaim.Finite(path[i])) { return Fail<Polyline>(i); }
        if (i > 0 && path[i] == path[i - 1]) { return Fail<Polyline>(i); }
    }
    return Fin.Succ(path);
}
```

Anchor: `offset.md:494-502`, the open branch invokes the policy-free admission helper.

```csharp
: AdmitPath(op.Path).Map(path => Ribbon(op with { Path = path })))
```

**To:**

```csharp
for (int i = 0; i < ring.Count; i++) {
    if (!ValidityClaim.Finite(ring[i])
        || i > 0 && ring[i].DistanceTo(ring[i - 1]) <= policy.CollapseBand)
        return Fail<(Polyline Ring, ClearanceProbe Edges)>(i);
}
```

```csharp
static Fin<Polyline> AdmitPath(Polyline path, OffsetPolicy policy) {
    if (path.Count < 2) return Fail<Polyline>(0);
    for (int i = 0; i < path.Count; i++) {
        if (!ValidityClaim.Finite(path[i])
            || i > 0 && path[i].DistanceTo(path[i - 1]) <= policy.CollapseBand)
            return Fail<Polyline>(i);
    }
    return path;
}
```

```csharp
: AdmitPath(op.Path, op.Policy).Map(path => Ribbon(op with { Path = path })))
```

**Why:** The offset kernel already defines the collapse band as its minimum meaningful separation. Admitting merely non-equal adjacent points allows zero-length-at-tolerance edges into normals, event times, and the spatial index. Applying the existing contextual band at both boundaries rejects that invalid state without another type or helper.

# 16. Enforce the event ceiling without one extra dequeue

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, line 359.

**From:**

Anchor: `offset.md:359`, the post-increment budget guard.

```csharp
if (fired++ > policy.MaxEvents.Value) { return Fin.Fail<Trace>(new GeometryFault.SkeletonStalled(queue.Count, Some(queue.Peek().Time))); }
```

**To:**

```csharp
if (fired++ >= policy.MaxEvents.Value)
    return Fin.Fail<(WavefrontStore Store, SkeletonGraph Graph)>(
        new GeometryFault.SkeletonStalled(queue.Count, Some(queue.Peek().Time)));
```

**Why:** `>` permits `MaxEvents + 1` events to dequeue before failing. `>=` makes the guarded dimension an actual ceiling while retaining the counter's single increment point.

# 17. Nest wavefront-only implementation types under Offsetting

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 190-250 and 284-285.

**From:**

Anchor: `offset.md:190-250`, `WavefrontStore` and `OffsetEvent` are module-level types used only by `Offsetting`.

```csharp
public sealed class WavefrontStore {
```

Anchor: `offset.md:241-249`, the module-level event union.

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetEvent {
    private OffsetEvent() { }

    public sealed record Edge(double Time, int Vertex, int NextVertex) : OffsetEvent;
    public sealed record Split(double Time, int Reflex, int OpposingA, int OpposingB) : OffsetEvent;

    public double Time =>
        Switch(edge: static e => e.Time, split: static s => s.Time);
}
```

Anchor: `offset.md:284-285`, their sole owner.

```csharp
public static class Offsetting {
    public static Fin<OffsetResult> Apply(OffsetOp op, Op? key = null) {
```

**To:**

```csharp
public static partial class Offsetting {
    sealed class WavefrontStore {
```

```csharp
    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    abstract partial record OffsetEvent {
        private OffsetEvent() { }
        public sealed record Edge(double Time, int Vertex, int NextVertex) : OffsetEvent;
        public sealed record Split(double Time, int Reflex, int OpposingA, int OpposingB) : OffsetEvent;

        public double Time =>
            Switch(edge: static e => e.Time, split: static s => s.Time);
    }
```

```csharp
// module-level WavefrontStore DELETED
// module-level OffsetEvent DELETED
```

**Why:** Neither type is a reusable meshing concept; both are private mechanics of one propagation algorithm. Nesting removes two module-level symbols and prevents callers from coupling to mutable wavefront storage or raw queue events while leaving the event algebra and arena implementation intact.

**Ripples:** Update the target's owner prose, Mermaid graph, density table, and indexed notes so both names appear only beneath `Offsetting`, not as public module siblings.

# 18. Remove the one-use collection alias

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 39 and 505-507.

**From:**

Anchor: `offset.md:39`, the module-level alias.

```csharp
using IndexSet = System.Collections.Generic.HashSet<int>;
```

Anchor: `offset.md:505-507`, its only use.

```csharp
static Seq<int[]> Rings(WavefrontStore store) {
    IndexSet seen = new();
    List<int[]> loops = new();
```

**To:**

```csharp
// IndexSet alias DELETED

static Seq<int[]> Rings(WavefrontStore store) {
    HashSet<int> seen = new();
    List<int[]> loops = new();
```

**Why:** The BCL type is already imported and appears once. The alias adds a module symbol without shortening or strengthening the kernel that uses it.

# 19. Emit both sides of every open ribbon

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 543-557.

**From:**

Anchor: `offset.md:543-557`, `Ribbon` suppresses the return side whenever an end row closes without a cap.

```csharp
bool ring = path.IsClosed;
bool fused = ring || op.End.ClosesRibbon;
int n = path.Count - (ring ? 1 : 0);
double d = Math.Abs(op.Distance);
Polyline cycle = new();
Emit(cycle, path, n, closed: ring, d, op);
if (!fused) {
    foreach (Point3d cap in op.End.Cap(path[n - 1], Unit(path[n - 1] - path[n - 2]), d, op.Policy)) { cycle.Add(cap); }
    Emit(cycle, Reversed(path), n, closed: false, d, op);
    foreach (Point3d cap in op.End.Cap(path[0], Unit(path[0] - path[1]), d, op.Policy)) { cycle.Add(cap); }
}
```

**To:**

```csharp
bool ring = path.IsClosed;
int n = path.Count - (ring ? 1 : 0);
double d = Math.Abs(op.Distance);
Polyline cycle = new();
Emit(cycle, path, n, closed: ring, d, op);
if (!ring) {
    if (!op.End.ClosesRibbon)
        foreach (Point3d cap in op.End.Cap(path[n - 1], Unit(path[n - 1] - path[n - 2]), d, op.Policy)) cycle.Add(cap);
    Emit(cycle, Reversed(path), n, closed: false, d, op);
    if (!op.End.ClosesRibbon)
        foreach (Point3d cap in op.End.Cap(path[0], Unit(path[0] - path[1]), d, op.Policy)) cycle.Add(cap);
}
// fused DELETED
```

**Why:** `Closed` and `Joined` suppress end-cap geometry; they do not erase the mirrored side of an open ribbon. The old `fused` branch emits one offset side and closes its endpoints directly, contradicting the page's two-sided-ribbon law. Discriminating first on input closure preserves the single contour for a closed ring while every open path emits both sides; the end row decides only whether cap points join them.

# 20. Revalidate split contact when a queued event fires

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 374-376.

**From:**

Anchor: `offset.md:374-376`, a split event checks only liveness and adjacency after sitting in the staleable queue.

```csharp
if (store.Alive(s.Reflex) && store.Alive(s.OpposingA) && store.Alive(s.OpposingB) && store.Next(s.OpposingA) == s.OpposingB) {
    Divide(store, s, admitted.Edges, nodes, arcs, queue, policy, speeds);
}
```

**To:**

```csharp
if (store.Alive(s.Reflex) && store.Alive(s.OpposingA) && store.Alive(s.OpposingB)
    && store.Next(s.OpposingA) == s.OpposingB
    && new Line(store.At(s.OpposingA, s.Time), store.At(s.OpposingB, s.Time))
        .DistanceTo(store.At(s.Reflex, s.Time), true) <= policy.CollapseBand)
    Divide(store, s, admitted.Edges, nodes, arcs, queue, policy, speeds);
```

**Why:** Rewires can leave all three vertices alive and the opposing pair adjacent while moving the segment away from the scheduled hit. `SplitTime` solves against the then-current supporting line; fire-time contact with the bounded segment is the missing liveness evidence. Rhino `Line.DistanceTo(Point3d, bool)` supplies that bounded check without another helper or type.

# 21. Inline the medial fold's sole centroid helper

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 600 and 679-680.

**From:**

Anchor: `offset.md:600`, the only call to `Centroid`.

```csharp
bool[] interior = [.. x.Tris.Select(tri => Containment.Of(Centroid(tri), ring, Axis.Z, Axis.Y).Equals(Containment.Inside))];
```

Anchor: `offset.md:679-680`, the one-call helper.

```csharp
static Point3d Centroid((Point3d A, Point3d B, Point3d C) tri) =>
    new((tri.A.X + tri.B.X + tri.C.X) / 3.0, (tri.A.Y + tri.B.Y + tri.C.Y) / 3.0, (tri.A.Z + tri.B.Z + tri.C.Z) / 3.0);
```

**To:**

```csharp
bool[] interior = [.. x.Tris.Select(tri => Containment.Of(new Point3d(
    (tri.A.X + tri.B.X + tri.C.X) / 3.0,
    (tri.A.Y + tri.B.Y + tri.C.Y) / 3.0,
    (tri.A.Z + tri.B.Z + tri.C.Z) / 3.0), ring, Axis.Z, Axis.Y).Equals(Containment.Inside))];
// Centroid DELETED
```

**Why:** The centroid expression has one caller and no independent domain meaning on this page. Inlining removes one private module member and keeps the medial interior predicate at the only site that consumes the point.

# 22. Remove the one-hop degenerate-offset failure helper

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 307-346.

**From:**

Anchor: `offset.md:307-343`, both admission paths call the generic helper for their local refusal.

```csharp
if (ring.Count < 4 || !ring.IsClosed) { return Fail<AdmittedRing>(0); }
```

```csharp
if (Orientation(ring) == Sign.Zero) { return Fail<AdmittedRing>(0); }
```

```csharp
if (path.Count < 2) { return Fail<Polyline>(0); }
```

Anchor: `offset.md:346`, the helper only renames `Fin.Fail` plus one fault constructor.

```csharp
static Fin<T> Fail<T>(int vertex) => Fin.Fail<T>(new GeometryFault.DegenerateOffset(vertex));
```

**To:**

```csharp
if (ring.Count < 4 || !ring.IsClosed) return new GeometryFault.DegenerateOffset(0);
```

```csharp
if (Orientation(ring) == Sign.Zero) return new GeometryFault.DegenerateOffset(0);
```

```csharp
return SelfCrossing(edges, policy, key).Bind(crossing => crossing.Match(
    Some: vertex => Fin.Fail<(Polyline Ring, ClearanceProbe Edges)>(
        new GeometryFault.DegenerateOffset(vertex)),
    None: () => Fin.Succ((oriented, edges))));
```

```csharp
if (path.Count < 2) return new GeometryFault.DegenerateOffset(0);
```

```csharp
// Offsetting.Fail<T> DELETED
```

Apply the same target-typed bare-fault return to the finite and collapsed-edge refusals in `AdmitRing` and `AdmitPath`. Keep the explicit `Fin.Fail<(Polyline Ring, ClearanceProbe Edges)>` in the `Option.Match` arm because generic arm inference does not apply the implicit fault-to-`Fin` conversion.

**Why:** `Fail<T>` contributes no domain rule beyond the fault constructor already visible at each admission site and forces a second hop to discover the failure identity. LanguageExt permits a bare `Error`-derived fault in a target-typed `Fin<T>` return; only the generic `Match` arm needs the explicit carrier construction. The change removes one private member without changing the fault family or its vertex evidence.

# 23. Inline arena growth at its sole mutation site

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 216-236.

**From:**

Anchor: `offset.md:216-236`, `Spawn` is the only caller of `WavefrontStore.Grow`.

```csharp
public int Spawn(Point3d at, Vector3d velocity, double time, int fromNode, int outEdge) {
    int v = free.Count > 0 ? free.Pop() : count++;
    Grow(v + 1);
    (px[v], py[v], vx[v], vy[v]) = (at.X, at.Y, velocity.X, velocity.Y);
    (spawnTime[v], node[v], edgeOf[v], dead[v]) = (time, fromNode, outEdge, false);
    return v;
}

void Grow(int needed) {
    if (needed <= px.Length) { return; }
    int extent = int.Max(needed, px.Length << 1);
    Array.Resize(ref px, extent); Array.Resize(ref py, extent);
    Array.Resize(ref vx, extent); Array.Resize(ref vy, extent);
    Array.Resize(ref spawnTime, extent);
    Array.Resize(ref prev, extent); Array.Resize(ref next, extent);
    Array.Resize(ref node, extent); Array.Resize(ref edgeOf, extent);
    Array.Resize(ref dead, extent);
}
```

**To:**

```csharp
public int Spawn(Point3d at, Vector3d velocity, double time, int fromNode, int outEdge) {
    int v = free.Count > 0 ? free.Pop() : count++;
    if (v >= px.Length) {
        int extent = int.Max(v + 1, px.Length << 1);
        Array.Resize(ref px, extent); Array.Resize(ref py, extent);
        Array.Resize(ref vx, extent); Array.Resize(ref vy, extent);
        Array.Resize(ref spawnTime, extent);
        Array.Resize(ref prev, extent); Array.Resize(ref next, extent);
        Array.Resize(ref node, extent); Array.Resize(ref edgeOf, extent);
        Array.Resize(ref dead, extent);
    }
    (px[v], py[v], vx[v], vy[v]) = (at.X, at.Y, velocity.X, velocity.Y);
    (spawnTime[v], node[v], edgeOf[v], dead[v]) = (time, fromNode, outEdge, false);
    return v;
}

// WavefrontStore.Grow DELETED
```

**Why:** Capacity growth is not an independently meaningful arena operation: only `Spawn` can create a needed slot, and every growth must precede that slot's write. Inlining the doubling branch removes one private member and the `needed` forwarding hop while preserving the same allocation threshold and column-resize set.

# 24. Inline reflex classification and let over-budget events reach the typed exhaustion gate

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 356, 361, 411-417, 437-438, 460, and 489-491.

**From:**

Anchor: `offset.md:411-417`, scheduling silently discards every event beyond `TimeBudget`.

```csharp
static void EnqueueAt(WavefrontStore store, PriorityQueue<OffsetEvent, double> queue, int v, double now, OffsetPolicy policy, Arr<double> speeds) {
    if (!store.Alive(v)) { return; }
    int nxt = store.Next(v);
    EdgeCollapseTime(store, v, nxt, now).IfSome(t => { if (t <= policy.TimeBudget.Value) { queue.Enqueue(new OffsetEvent.Edge(t, v, nxt), t); } });
    if (IsReflex(store, v, now)) {
        SplitTime(store, v, now, speeds).IfSome(s => { if (s.Time <= policy.TimeBudget.Value) { queue.Enqueue(new OffsetEvent.Split(s.Time, v, s.A, s.B), s.Time); } });
    }
}
```

Anchor: `offset.md:489-491`, the scheduler's sole `IsReflex` helper repeats the liveness test already performed at its entry.

```csharp
static bool IsReflex(WavefrontStore store, int v, double now) =>
    store.Alive(v)
    && Predicate.Orient2D(store.At(store.Prev(v), now), store.At(v, now), store.At(store.Next(v), now)) == Sign.Negative;
```

Anchor: `offset.md:361`, the declared exhaustion branch can never observe an event filtered out above.

```csharp
if (ev.Time > policy.TimeBudget.Value) { return Fin.Fail<Trace>(new GeometryFault.SkeletonStalled(queue.Count, Some(ev.Time))); }
```

Anchor: `offset.md:356,437-438,460`, every scheduler call forwards `policy` only for that pre-filter.

```csharp
EnqueueAt(store, queue, v, 0.0, policy, speeds);
EnqueueAt(store, queue, before, ev.Time, policy, speeds);
EnqueueAt(store, queue, merged, ev.Time, policy, speeds);
foreach (int v in (ReadOnlySpan<int>)[before, left, ev.OpposingA, right]) { EnqueueAt(store, queue, v, ev.Time, policy, speeds); }
```

**To:**

```csharp
static void EnqueueAt(
    WavefrontStore store, PriorityQueue<OffsetEvent, double> queue,
    int v, double now, Arr<double> speeds) {
    if (!store.Alive(v)) return;
    int next = store.Next(v);
    EdgeCollapseTime(store, v, next, now)
        .IfSome(time => queue.Enqueue(new OffsetEvent.Edge(time, v, next), time));
    if (Predicate.Orient2D(
            store.At(store.Prev(v), now), store.At(v, now), store.At(next, now)) == Sign.Negative)
        SplitTime(store, v, now, speeds).IfSome(split =>
            queue.Enqueue(new OffsetEvent.Split(split.Time, v, split.A, split.B), split.Time));
}
```

```csharp
EnqueueAt(store, queue, v, 0.0, speeds);
EnqueueAt(store, queue, before, ev.Time, speeds);
EnqueueAt(store, queue, merged, ev.Time, speeds);
foreach (int v in (ReadOnlySpan<int>)[before, left, ev.OpposingA, right]) { EnqueueAt(store, queue, v, ev.Time, speeds); }
```

The existing dequeue gate remains:

```csharp
if (ev.Time > policy.TimeBudget.Value)
    return Fin.Fail<(WavefrontStore Store, SkeletonGraph Graph)>(
        new GeometryFault.SkeletonStalled(queue.Count, Some(ev.Time)));
// IsReflex DELETED
// EnqueueAt.policy DELETED
```

**Why:** Pre-filtering makes a time-budget exhaustion indistinguishable from convergence: the queue empties and `Propagate` returns success, while the later `SkeletonStalled` branch is unreachable. Scheduling the analytic event and enforcing the ceiling at the ordered dequeue restores the declared typed failure. `EnqueueAt` already rejects a dead vertex, so inlining its sole reflex predicate also removes the repeated liveness read and one private member. The same rewrite removes the `policy` parameter from the scheduler and every call. `until` still intentionally stops an offset snapshot before a later event; only a dequeued event is work the run attempted.

# 25. Reject non-finite offset distance at the public fold

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 269 and 293.

**From:**

Anchor: `offset.md:269`, the offset request admits an unrestricted raw scalar.

```csharp
public sealed record Offset(Polyline Path, double Distance, JoinType Join, EndType End, OffsetPolicy Policy) : OffsetOp;
```

Anchor: `offset.md:293`, the dispatch enters ribbon and round-corner arithmetic without a distance gate.

```csharp
offset: o => Snapshot(o, site),
```

**To:**

```csharp
offset: o => double.IsFinite(o.Distance)
    ? Snapshot(o, site)
    : Fin.Fail<OffsetResult>(new GeometryFault.DegenerateOffset(0)),
```

**Why:** Distance is signed because its sign selects inward versus outward propagation, so `PositiveMagnitude` is not its domain. It still must be finite: either infinity or NaN contaminates event positions, normals, and arc sampling. The one public operation fold is the admission boundary; no new scalar type, helper, or mode flag is needed, and the existing zero-distance semantics remain available rather than being reclassified without contract evidence.

# 26. Admit both Minkowski rings through the existing ring boundary

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 295 and 620-623.

**From:**

Anchor: `offset.md:295`, only the subject ring receives finite, area, simplicity, and orientation admission.

```csharp
minkowski: k => AdmitRing(k.Ring, k.Policy, site).Bind(admitted => Convolve(admitted.Ring, k.Element, site)).Map(static loops => (OffsetResult)new OffsetResult.Curves(loops)),
```

Anchor: `offset.md:620-623`, `Convolve` checks only the element's count and closure, then repeats orientation.

```csharp
static Fin<OffsetCurves> Convolve(Polyline ring, Polyline element, Op key) {
    if (element.Count < 4 || !element.IsClosed) { return Fin.Fail<OffsetCurves>(new GeometryFault.DegenerateOffset(0)); }
    Polyline b = Oriented(element);
```

**To:**

```csharp
minkowski: k => AdmitRing(k.Ring, k.Policy, site).Bind(ring =>
        AdmitRing(k.Element, k.Policy, site).Bind(element =>
            Convolve(ring.Ring, element.Ring, site)))
    .Map(static curves => (OffsetResult)curves),
```

```csharp
static Fin<Seq<Chain>> Convolve(Polyline ring, Polyline element, Op key) {
    Polyline b = element;
    // Convolve element count/closure guard DELETED
    // Convolve element reorientation DELETED
```

**Why:** The element participates in exact turn predicates and support search just as directly as the subject ring, but the old arm lets non-finite, zero-area, and self-crossing element geometry reach those kernels. Reusing `AdmitRing` applies the existing boundary once to each raw ring, returns both in the same canonical orientation, and deletes `Convolve`'s weaker duplicate guard and reorientation. The convexity loop remains because convexity is specific to convolution rather than general ring admission.

# 27. Inline the remaining one-call orientation normalizer

**Location:** `libs/dotnet/Rasm/.planning/Meshing/offset.md`, lines 307-315 and 672-677.

**From:**

Anchor: `offset.md:307-315`, ring admission is the normalizer's only remaining caller after both Minkowski operands use this boundary.

```csharp
static Fin<AdmittedRing> AdmitRing(Polyline ring, OffsetPolicy policy, Op key) {
    if (ring.Count < 4 || !ring.IsClosed) { return Fail<AdmittedRing>(0); }
    for (int i = 0; i < ring.Count; i++) {
        if (!ValidityClaim.Finite(ring[i])) { return Fail<AdmittedRing>(i); }
    }
    if (Orientation(ring) == Sign.Zero) { return Fail<AdmittedRing>(0); }
    Polyline oriented = Oriented(ring);
    ClearanceProbe edges = EdgesOf(oriented, policy);
    return SelfCrossing(edges, oriented, policy, key).Map(_ => new AdmittedRing(oriented, edges));
}
```

Anchor: `offset.md:672-677`, the private normalizer only conditionally copies and reverses its argument.

```csharp
static Polyline Oriented(Polyline ring) {
    if (Orientation(ring) != Sign.Negative) { return ring; }
    Polyline reversed = new(ring);
    reversed.Reverse();
    return reversed;
}
```

**To:**

```csharp
Sign orientation = Orientation(ring);
if (orientation == Sign.Zero) return new GeometryFault.DegenerateOffset(0);
Polyline oriented = ring;
if (orientation == Sign.Negative) {
    oriented = new(ring);
    oriented.Reverse();
}
ClearanceProbe edges = EdgesOf(oriented, policy);
```

```csharp
// Offsetting.Oriented DELETED
```

**Why:** Once Minkowski routes its element through `AdmitRing`, no second caller needs an orientation-normalization abstraction. Capturing the exact sign once also avoids rescanning the ring immediately after the zero-area check. The conditional copy still preserves the original input when no reversal is needed, while deleting one private member and its forwarding return path.
