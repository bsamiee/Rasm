# 1. Replace the payload-free seal vocabulary with one policy fact

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:45`, anchor `[SmartEnum] public sealed partial class SealPosture`; `:85`, anchor `public sealed record SlicePolicy`; `:334`, anchor `Seq<Chain> closed`.

From (`slice.md:45`):

```csharp
[SmartEnum]
public sealed partial class SealPosture {
    public static readonly SealPosture Required = new(static (layer, elevation, open) =>
        Fin.Fail<Unit>(new GeometryFault.OpenSection(layer, elevation, open)));
    public static readonly SealPosture Admitted = new(static (_, _, _) => Fin.Succ(unit));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Admit(int layer, double elevation, int openRows);
}
```

To:

```csharp
// SealPosture DELETED
```

From (`slice.md:85`):

```csharp
public sealed record SlicePolicy(SealPosture Seal, Dimension MaxLayers, Dimension FrameBins, Dimension ParallelFloor, IntersectPolicy Intersect) {
    public static readonly SlicePolicy Canonical = new(
        Seal: SealPosture.Admitted, MaxLayers: Dimension.Create(value: 1 << 14),
```

To:

```csharp
public sealed record SlicePolicy(bool AllowOpen, Dimension MaxLayers, Dimension FrameBins, Dimension ParallelFloor, IntersectPolicy Intersect) {
    public static readonly SlicePolicy Canonical = new(
        AllowOpen: true, MaxLayers: Dimension.Create(value: 1 << 14),
```

From (`slice.md:334`):

```csharp
Seq<Chain> closed = walked.Filter(static chain => chain.Closed);
Seq<Chain> openRows = walked.Filter(static chain => !chain.Closed);
return (openRows.IsEmpty ? Fin.Succ(unit) : op.Policy.Seal.Admit(k, family[k], openRows.Count)).Bind(_ => {
```

To:

```csharp
Seq<Chain> closed = walked.Filter(static chain => chain.Closed);
Seq<Chain> openRows = walked.Filter(static chain => !chain.Closed);
return guard(op.Policy.AllowOpen || openRows.IsEmpty,
    new GeometryFault.OpenSection(k, family[k], openRows.Count)).ToFin().Bind(_ => {
```

Why: `Required` and `Admitted` are two payload-free cases whose only distinction is a predicate. The root shape law explicitly collapses such a family to a bool column. `guard(...).ToFin()` is the installed LanguageExt admission operator and preserves the same typed `OpenSection` failure without a module-level type, two generated items, or a delegate member. Update the lead, card, growth text, density bar, and diagram fault label to name `AllowOpen`; no capability is removed.

# 2. Delete the one-use `SliceOp` request wrapper

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:191`, anchor `public sealed record SliceOp`; `:296`, anchor `public static Fin<SliceStack> Apply`; `:303`, anchor `static Fin<Unit> Admit`; `:317`, anchor `static Fin<SliceStack> Fold`.

From (`slice.md:191`):

```csharp
public sealed record SliceOp(MeshSpace Mesh, Plane Datum, LayerPlan Plan, SlicePolicy Policy);
```

To:

```csharp
// SliceOp DELETED
```

From (`slice.md:296`):

```csharp
public static Fin<SliceStack> Apply(SliceOp op, Op? key = null) {
    Op site = key.OrDefault();
    return Admit(op)
        .Bind(_ => SliceFrame.Of(op.Mesh, op.Datum, op.Policy, site))
        .Bind(frame => op.Plan.Elevations(frame, op.Policy).Bind(elevations => Fold(op, frame, elevations, site)));
}

static Fin<Unit> Admit(SliceOp op) =>
    !op.Datum.IsValid ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Plane, None, "non-finite datum plane"))
    : op.Mesh.Native.Faces.Count == 0 ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "empty mesh"))
    : !op.Policy.Intersect.IsValid ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "invalid intersect policy"))
    : Fin.Succ(unit);
```

To:

```csharp
public static Fin<SliceStack> Apply(MeshSpace mesh, Plane datum, LayerPlan plan, SlicePolicy policy, Op? key = null) {
    Op site = key.OrDefault();
    return Admit(mesh, datum, policy)
        .Bind(_ => SliceFrame.Of(mesh, datum, policy, site))
        .Bind(frame => plan.Elevations(frame, policy).Bind(elevations => Fold(mesh, datum, policy, frame, elevations, site)));
}

static Fin<Unit> Admit(MeshSpace mesh, Plane datum, SlicePolicy policy) =>
    !datum.IsValid ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Plane, None, "non-finite datum plane"))
    : mesh.Native.Faces.Count == 0 ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "empty mesh"))
    : !policy.Intersect.IsValid ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "invalid intersect policy"))
    : Fin.Succ(unit);
```

From (`slice.md:317`):

```csharp
static Fin<SliceStack> Fold(SliceOp op, SliceFrame frame, Arr<double> elevations, Op key) {
    int layers = elevations.Count;
    using MemoryOwner<Fin<IntersectResult>> slots = MemoryOwner<Fin<IntersectResult>>.Allocate(layers);
    double[] family = [.. elevations];
    ParallelHelper.For(0, layers, new SectionAction(op.Mesh, op.Datum, family, op.Policy.Intersect, slots.Memory, key), op.Policy.ParallelFloor.Value);
```

To:

```csharp
static Fin<SliceStack> Fold(MeshSpace mesh, Plane datum, SlicePolicy policy, SliceFrame frame, Arr<double> elevations, Op key) {
    int layers = elevations.Count;
    using MemoryOwner<Fin<IntersectResult>> slots = MemoryOwner<Fin<IntersectResult>>.Allocate(layers);
    double[] family = [.. elevations];
    ParallelHelper.For(0, layers, new SectionAction(mesh, datum, family, policy.Intersect, slots.Memory, key), policy.ParallelFloor.Value);
```

Within `Fold`, replace the remaining `op.Policy` reads with `policy` and the task-7 `op.Datum` plane projection with `datum`. No other `SliceOp` field is read there.

Why: `SliceOp` has no case distinction, admission, derived behavior, persistence identity, or consumer beyond the single `Apply` entry. Its two callers construct it only to have `Apply` immediately read all four fields and pass three onward. Moving the essential arguments onto the one entry removes a module-level record and its generated constructor/equality surface without adding a helper, mode flag, or alternate entrypoint.

Ripples: replace `Slicing.Apply(new SliceOp(model, policy.Datum, policy.Layers, policy.Slicing))` with `Slicing.Apply(model, policy.Datum, policy.Layers, policy.Slicing)` in `libs/dotnet/Rasm.Fabrication/.planning/Additive/production.md:1532`; make the equivalent direct-argument replacement in `libs/dotnet/Rasm.Fabrication/.planning/Additive/slicing.md:1196`, and remove `SliceOp` from that page's package-surface roster at `:28`. Update the target lead, owner card, entry line, and density bar to name the direct `Apply` signature.

# 3. Reuse the kernel scalar owners in every layer-plan case

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:144`, anchor `public sealed record Uniform(double Height) : LayerPlan`.

From (`slice.md:144`):

```csharp
public sealed record Uniform(double Height) : LayerPlan;
public sealed record Adaptive(double CuspHeight, double MinHeight, double MaxHeight) : LayerPlan;
public sealed record BySlope(Arr<(double SlopeCeiling, double Height)> Bands) : LayerPlan;
public sealed record SupportInterface(double BaseHeight, double InterfaceHeight, int InterfaceLayers, double OverhangCosine) : LayerPlan;
public sealed record AtElevations(Arr<double> Elevations) : LayerPlan;
```

To:

```csharp
public sealed record Uniform(PositiveMagnitude Height) : LayerPlan;
public sealed record Adaptive(PositiveMagnitude CuspHeight, PositiveMagnitude MinHeight, PositiveMagnitude MaxHeight) : LayerPlan;
public sealed record BySlope(Arr<(UnitInterval SlopeCeiling, PositiveMagnitude Height)> Bands) : LayerPlan;
public sealed record SupportInterface(PositiveMagnitude BaseHeight, PositiveMagnitude InterfaceHeight, Dimension InterfaceLayers, UnitInterval OverhangCosine) : LayerPlan;
public sealed record AtElevations(Arr<double> Elevations) : LayerPlan;
```

From (`slice.md:153`):

```csharp
uniform:          static (s, u) => March(s.Frame, s.Policy, _ => u.Height),
adaptive:         static (s, a) => March(s.Frame, s.Policy, z => Math.Clamp(a.CuspHeight / s.Frame.SteepestSlope(z, a.MaxHeight), a.MinHeight, a.MaxHeight)),
bySlope:          static (s, b) => March(s.Frame, s.Policy, z => BandHeight(b.Bands, s.Frame.SteepestSlope(z, b.Bands.Fold(0.0, static (m, row) => double.Max(m, row.Height))))),
supportInterface: static (s, i) => March(s.Frame, s.Policy, z => s.Frame.NearInterface(z, i.InterfaceLayers * i.InterfaceHeight, i.OverhangCosine) ? i.InterfaceHeight : i.BaseHeight),
```

To:

```csharp
uniform:          static (s, u) => March(s.Frame, s.Policy, _ => u.Height.Value),
adaptive:         static (s, a) => March(s.Frame, s.Policy, z => Math.Clamp(a.CuspHeight.Value / s.Frame.SteepestSlope(z, a.MaxHeight.Value), a.MinHeight.Value, a.MaxHeight.Value)),
bySlope:          static (s, b) => March(s.Frame, s.Policy, z => BandHeight(b.Bands, s.Frame.SteepestSlope(z, b.Bands.Fold(0.0, static (m, row) => double.Max(m, row.Height.Value))))),
supportInterface: static (s, i) => March(s.Frame, s.Policy, z => s.Frame.NearInterface(z, i.InterfaceLayers.Value * i.InterfaceHeight.Value, i.OverhangCosine.Value) ? i.InterfaceHeight.Value : i.BaseHeight.Value),
```

Why: `PositiveMagnitude`, `Dimension`, and `UnitInterval` are the kernel's existing Thinktecture-generated owners for magnitudes above `EpsilonPolicy.ZeroTolerance`, positive counts, and finite normalized values. Reusing them makes sub-kernel-floor heights, non-finite scalars, non-positive counts, and out-of-band cosines unconstructible, removes repeated primitive guards, and adds no slice-local enum or value-object symbol. `AtElevations` remains raw because signed datum-relative coordinates require relational validation over the collection, not a positive scalar owner.

# 4. Fuse plan admission into its exhaustive dispatch and delete two helpers

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:150`, anchor `public Fin<Arr<double>> Elevations`; `:162`, anchor `Fin<Unit> Admit`; `:183`, anchor `static double BandHeight`.

From (`slice.md:150`):

```csharp
public Fin<Arr<double>> Elevations(SliceFrame frame, SlicePolicy policy) =>
    Admit().Bind(_ => Switch(
```

To:

```csharp
internal Fin<Arr<double>> Elevations(SliceFrame frame, SlicePolicy policy) => Switch(
```

From (`slice.md:155`):

```csharp
bySlope:          static (s, b) => March(s.Frame, s.Policy, z => BandHeight(b.Bands, s.Frame.SteepestSlope(z, b.Bands.Fold(0.0, static (m, row) => double.Max(m, row.Height))))),
```

To:

```csharp
bySlope: static (s, b) => b.Bands.Count > 0
    && b.Bands.ForAll(static row => row.SlopeCeiling.Value > 0.0)
    && Enumerable.Range(1, b.Bands.Count - 1)
        .All(i => b.Bands[i - 1].SlopeCeiling.Value < b.Bands[i].SlopeCeiling.Value)
    ? March(s.Frame, s.Policy, z => {
        double slope = s.Frame.SteepestSlope(z, b.Bands.Fold(0.0, static (m, row) => double.Max(m, row.Height.Value)));
        return b.Bands.Find(row => slope <= row.SlopeCeiling.Value)
            .Map(static row => row.Height.Value).IfNone(b.Bands[^1].Height.Value);
    })
    : Reject("empty, zero, or unordered slope bands"),
```

From (`slice.md:154`):

```csharp
adaptive:         static (s, a) => March(s.Frame, s.Policy, z => Math.Clamp(a.CuspHeight / s.Frame.SteepestSlope(z, a.MaxHeight), a.MinHeight, a.MaxHeight)),
```

To:

```csharp
adaptive: static (s, a) => a.MaxHeight.Value >= a.MinHeight.Value
    ? March(s.Frame, s.Policy, z => Math.Clamp(a.CuspHeight.Value / s.Frame.SteepestSlope(z, a.MaxHeight.Value), a.MinHeight.Value, a.MaxHeight.Value))
    : Reject("maximum height below minimum"),
```

From (`slice.md:156`):

```csharp
supportInterface: static (s, i) => March(s.Frame, s.Policy, z => s.Frame.NearInterface(z, i.InterfaceLayers * i.InterfaceHeight, i.OverhangCosine) ? i.InterfaceHeight : i.BaseHeight),
```

To:

```csharp
supportInterface: static (s, i) => i.OverhangCosine.Value > 0.0
    ? March(s.Frame, s.Policy, z => s.Frame.NearInterface(z, i.InterfaceLayers.Value * i.InterfaceHeight.Value, i.OverhangCosine.Value) ? i.InterfaceHeight.Value : i.BaseHeight.Value)
    : Reject("zero overhang cosine"),
```

From (`slice.md:157`):

```csharp
atElevations:     static (s, x) => x.Elevations.ForAll(e => e > s.Frame.Lo && e < s.Frame.Hi)
    && Enumerable.Range(1, int.Max(x.Elevations.Count - 1, 0)).All(i => x.Elevations[i - 1] < x.Elevations[i])
        ? Fin.Succ(x.Elevations)
        : Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(Kind.Plane, None, "explicit elevations out of extent or unsorted"))));
```

To:

```csharp
atElevations: static (s, x) => x.Elevations.Count > 0
    && x.Elevations.ForAll(e => double.IsFinite(e) && e > s.Frame.Lo && e < s.Frame.Hi)
    && Enumerable.Range(1, x.Elevations.Count - 1).All(i => x.Elevations[i - 1] < x.Elevations[i])
        ? Fin.Succ(x.Elevations)
        : Reject("empty, non-finite, out-of-extent, or unsorted elevations"));
```

From (`slice.md:162`):

```csharp
Fin<Unit> Admit() => Switch(
    uniform:          static u => Gate(u.Height > 0.0, "non-positive layer height"),
    adaptive:         static a => Gate(a.CuspHeight > 0.0 && a.MinHeight > 0.0 && a.MaxHeight >= a.MinHeight, "degenerate cusp bounds"),
    bySlope:          static b => Gate(b.Bands.Count > 0 && b.Bands.ForAll(static row => row.Height > 0.0 && row.SlopeCeiling is > 0.0 and <= 1.0), "degenerate slope bands"),
    supportInterface: static i => Gate(i.BaseHeight > 0.0 && i.InterfaceHeight > 0.0 && i.InterfaceLayers > 0 && i.OverhangCosine is > 0.0 and <= 1.0, "degenerate interface plan"),
    atElevations:     static x => Gate(x.Elevations.Count > 0 && x.Elevations.ForAll(static e => double.IsFinite(e)), "empty or non-finite elevation family"));

static Fin<Unit> Gate(bool holds, string witness) =>
    holds ? Fin.Succ(unit) : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Plane, None, witness));
```

To:

```csharp
// LayerPlan.Admit DELETED

static Fin<Arr<double>> Reject(string witness) =>
    Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(Kind.Plane, None, witness));
```

From (`slice.md:183`):

```csharp
static double BandHeight(Arr<(double SlopeCeiling, double Height)> bands, double slope) {
    foreach ((double ceiling, double height) in bands) {
        if (slope <= ceiling) { return height; }
    }
    return bands[bands.Count - 1].Height;
}
```

To:

```csharp
// LayerPlan.BandHeight DELETED
```

Why: the current `Admit` walks the active case, returns `Unit`, then immediately walks the same case again. Generated scalar admission already owns the primitive guards after task 3. Keep only the relations those owners cannot prove: adaptive ordering, nonzero normalized cosine thresholds, a nonempty strictly ascending slope table, and explicit-elevation collection order. Without the ordering clause, `Find` makes a later lower ceiling unreachable and the result depends on malformed row order. `Find(...).Map(...).IfNone(...)` is the installed LanguageExt foldable search and removes the one-call `BandHeight` member while preserving the final-band fallback. Net result: one union walk instead of two and two fewer private members.

# 5. Internalize the run frame under `Slicing`

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:93`, anchor `public sealed record SliceFrame`; `:150`, anchor `LayerPlan.Elevations`; `:295`, anchor `public static class Slicing`.

From (`slice.md:93`):

```csharp
public sealed record SliceFrame(Plane Datum, Axis Vertical, double Lo, double Hi, double[] MaxSlope, double[] OverhangStarts, double[] OverhangCosines) {
    internal static Fin<SliceFrame> Of(MeshSpace mesh, Plane datum, SlicePolicy policy, Op key) {
        using MeshEdit soup = MeshEdit.Of(mesh);
        Vector3d d = datum.Normal;
        d.Unitize();
```

To:

```csharp
// SliceFrame DELETED
```

From (`slice.md:99`):

```csharp
double e = (soup.Position(v) - datum.Origin) * d;
```

To, inside a private `Slicing.Frame` nested immediately after `SectionAction`:

```csharp
sealed record Frame(Axis Vertical, double Lo, double Hi, double[] MaxSlope, double[] OverhangStarts, double[] OverhangCosines) {
    internal static Fin<Frame> Of(MeshSpace mesh, Plane datum, SlicePolicy policy, Op key) {
        using MeshEdit soup = MeshEdit.Of(mesh);
        Vector3d d = datum.Normal;
        (double lo, double hi) = (double.PositiveInfinity, double.NegativeInfinity);
        for (int v = 0; v < soup.VertexCount; v++) {
            double e = datum.DistanceTo(soup.Position(v));
```

From (`slice.md:110`):

```csharp
(double ea, double eb, double ec) = ((soup.Position(a) - datum.Origin) * d, (soup.Position(b) - datum.Origin) * d, (soup.Position(c) - datum.Origin) * d);
```

To:

```csharp
(double ea, double eb, double ec) = (datum.DistanceTo(soup.Position(a)), datum.DistanceTo(soup.Position(b)), datum.DistanceTo(soup.Position(c)));
```

From (`slice.md:119`):

```csharp
return Axis.DominantOf(d, key).Map(vertical => new SliceFrame(datum, vertical, lo, hi, slope, [.. rows.Select(static row => row.Start)], [.. rows.Select(static row => row.Cos)]));
```

To:

```csharp
return Axis.DominantOf(d, key).Map(vertical => new Frame(vertical, lo, hi, slope, [.. rows.Select(static row => row.Start)], [.. rows.Select(static row => row.Cos)]));
```

Move the remainder of the existing `Of` body plus `SteepestSlope` and `NearInterface` unchanged into `Frame`; only the two datum projections and the constructed type change.

From (`slice.md:150`):

```csharp
public Fin<Arr<double>> Elevations(SliceFrame frame, SlicePolicy policy) =>
```

To: move the task-4 `Elevations`, `Reject`, and `March` members unchanged from `LayerPlan` into `Slicing`, immediately after `Frame`, and make the plan an explicit argument.

```csharp
static Fin<Arr<double>> Elevations(LayerPlan plan, Frame frame, SlicePolicy policy) => plan.Switch(
```

Change `Apply` to call `Frame.Of(mesh, datum, policy, site)` and then `Elevations(plan, frame, policy)` before the existing direct-argument `Fold`; change the `Fold`/`Nest` frame parameters to `Frame`. `LayerPlan` then contains only its generated cases.

Why: `SliceFrame` has no consumer outside this operation and its `Datum` column is never read. It is transient algorithm state, not a public model. Nesting it removes one module-level type and moving the height engine removes every hand-authored operation member from the public `LayerPlan` owner. `Plane.DistanceTo` is the catalogued frame projection, deleting the manual origin subtraction and redundant `Unitize` of an admitted orthonormal plane.

# 6. Partition open and closed chains in one pass

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:334`, anchor `Seq<Chain> closed`.

From (`slice.md:334`):

```csharp
Seq<Chain> closed = walked.Filter(static chain => chain.Closed);
Seq<Chain> openRows = walked.Filter(static chain => !chain.Closed);
```

To:

```csharp
(Seq<Chain> openRows, Seq<Chain> closed) = walked.Partition(static chain => chain.Closed);
```

Why: `Seq.Partition` is the LanguageExt one-pass two-way split, and its positional result is `(False, True)`: open rows therefore occupy the first slot and closed rows the second. The two filters traverse the same intersection result twice and duplicate complementary predicates; the replacement preserves order in both partitions and removes one traversal and one fence line without reversing the two branches.

# 7. Replace world XYZ storage with datum-space UV channels

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:193`, anchor `public sealed record SliceStack`; `:210`, anchor `internal static Fin<SliceStack> Of`; `:323`, anchor `using ArrayPoolBufferWriter<double> x`; `:340`, anchor `(Span<double> sx, Span<double> sy, Span<double> sz)`.

From (`slice.md:193`):

```csharp
private SliceStack(Arr<double> elevations, Arr<int> layerPtr, Arr<int> contourPtr, Arr<double> x, Arr<double> y,
    Arr<double> z, Arr<int> parent, Arr<int> childPtr, Arr<int> children, Arr<bool> open) =>
    (Elevations, LayerPtr, ContourPtr, X, Y, Z, Parent, ChildPtr, Children, Open) =
        (elevations, layerPtr, contourPtr, x, y, z, parent, childPtr, children, open);
```

To:

```csharp
private SliceStack(Plane datum, Arr<double> elevations, Arr<int> layerPtr, Arr<int> contourPtr, Arr<double> u,
    Arr<double> v, Arr<int> parent, Arr<int> childPtr, Arr<int> children, Arr<bool> open) =>
    (Datum, Elevations, LayerPtr, ContourPtr, U, V, Parent, ChildPtr, Children, Open) =
        (datum, elevations, layerPtr, contourPtr, u, v, parent, childPtr, children, open);
```

From (`slice.md:199`):

```csharp
public Arr<double> Elevations { get; init; }
public Arr<int> LayerPtr { get; init; }
public Arr<int> ContourPtr { get; init; }
public Arr<double> X { get; init; }
public Arr<double> Y { get; init; }
public Arr<double> Z { get; init; }
```

To:

```csharp
public Plane Datum { get; }
public Arr<double> Elevations { get; }
public Arr<int> LayerPtr { get; }
public Arr<int> ContourPtr { get; }
public Arr<double> U { get; }
public Arr<double> V { get; }
```

Make the remaining `Parent`, `ChildPtr`, `Children`, and `Open` properties get-only in the same edit; a `with` expression must not bypass `SliceStack.Of` by replacing an admitted channel.

From (`slice.md:323`):

```csharp
using ArrayPoolBufferWriter<double> x = new();
using ArrayPoolBufferWriter<double> y = new();
using ArrayPoolBufferWriter<double> z = new();
```

To:

```csharp
using ArrayPoolBufferWriter<double> u = new();
using ArrayPoolBufferWriter<double> v = new();
```

From (`slice.md:340`):

```csharp
(Span<double> sx, Span<double> sy, Span<double> sz) =
    (x.GetSpan(sizeHint: extent), y.GetSpan(sizeHint: extent), z.GetSpan(sizeHint: extent));
for (int v = 0; v < extent; v++) {
    (sx[v], sy[v], sz[v]) = (chain.Points[v].X, chain.Points[v].Y, chain.Points[v].Z);
}
x.Advance(count: extent); y.Advance(count: extent); z.Advance(count: extent);
```

To:

```csharp
(Span<double> su, Span<double> sv) = (u.GetSpan(extent), v.GetSpan(extent));
for (int p = 0; p < extent; p++) {
    datum.RemapToPlaneSpace(chain.Points[p], out Point3d local);
    (su[p], sv[p]) = (local.X, local.Y);
}
u.Advance(extent); v.Advance(extent);
```

Pass `datum`, `u: new Arr<double>(u.WrittenSpan.ToArray())`, and `v: ...` to `SliceStack.Of`; delete the `z:` argument.

From (`slice.md:210`):

```csharp
internal static Fin<SliceStack> Of(Arr<double> elevations, Arr<int> layerPtr, Arr<int> contourPtr, Arr<double> x,
    Arr<double> y, Arr<double> z, Arr<int> parent, Arr<int> childPtr, Arr<int> children, Arr<bool> open, Op key) {
```

To:

```csharp
internal static Fin<SliceStack> Of(Plane datum, Arr<double> elevations, Arr<int> layerPtr, Arr<int> contourPtr, Arr<double> u,
    Arr<double> v, Arr<int> parent, Arr<int> childPtr, Arr<int> children, Arr<bool> open, Op key) {
```

Why: the request admits an arbitrary `Plane`, but the current wire silently treats world X/Y as slice-plane coordinates and carries every point's world Z even though its layer already owns the elevation. Rhino's catalogued `RemapToPlaneSpace` projection makes the frame correspondence explicit, stores each point with two coordinates instead of three, and removes one pooled writer and one frozen channel. Datum plus U/V is the primary representation; world points derive at the read edge in task 8 rather than remaining a contradictory second representation.

Ripples: in `libs/dotnet/Rasm.Fabrication/.planning/Additive/scanpath.md:488,536-538`, replace the `X/Y/Z` channel gate and bound reads with `U/V`, deleting the `Z` cardinality check. In `libs/dotnet/Rasm.Fabrication/.planning/Verify/audit.md:802-816`, validate `Datum.IsValid`, equal U/V lengths, `ContourPtr[^1] == U.Count`, and finite U/V instead of X/Y/Z. Update the target card and diagram from world XYZ channels to datum + U/V; the architecture boundary labels remain `SliceStack` and need no schema narration.

# 8. Derive contour and metric projections from datum plus UV

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:226`, anchor `public Chain ContourAt`; `:234`, anchor `public Seq<Chain> LayerAt`; `:249`, anchor `public double AreaAt`; `:273`, anchor `public Point3d CentroidAt`.

From (`slice.md:226`):

```csharp
public Chain ContourAt(int contour) {
    bool closed = !IsOpen(contour);
    Polyline polyline = new();
    for (int v = ContourPtr[contour]; v < ContourPtr[contour + 1]; v++) { polyline.Add(new Point3d(X[v], Y[v], Z[v])); }
```

To:

```csharp
public Chain ContourAt(int layer, int contour) {
    bool closed = !IsOpen(contour);
    Plane cut = new(Datum.Origin + (Elevations[layer] * Datum.Normal), Datum.XAxis, Datum.YAxis);
    Polyline polyline = new();
    for (int v = ContourPtr[contour]; v < ContourPtr[contour + 1]; v++) { polyline.Add(cut.PointAt(U[v], V[v])); }
```

From (`slice.md:234`):

```csharp
public Seq<Chain> LayerAt(int layer) =>
    toSeq(Enumerable.Range(LayerPtr[layer], LayerPtr[layer + 1] - LayerPtr[layer]).Select(ContourAt));
```

To:

```csharp
public Seq<Chain> LayerAt(int layer) =>
    toSeq(Enumerable.Range(LayerPtr[layer], LayerPtr[layer + 1] - LayerPtr[layer]).Select(contour => ContourAt(layer, contour)));
```

From (`slice.md:249`):

```csharp
area += (X[v] * Y[w]) - (X[w] * Y[v]);
```

To:

```csharp
area += (U[v] * V[w]) - (U[w] * V[v]);
```

From (`slice.md:267`):

```csharp
length += Math.Sqrt(((X[w] - X[v]) * (X[w] - X[v])) + ((Y[w] - Y[v]) * (Y[w] - Y[v])));
```

To:

```csharp
length += Math.Sqrt(((U[w] - U[v]) * (U[w] - U[v])) + ((V[w] - V[v]) * (V[w] - V[v])));
```

From (`slice.md:279`):

```csharp
double cross = (X[v] * Y[w]) - (X[w] * Y[v]);
mx += (X[v] + X[w]) * cross;
my += (Y[v] + Y[w]) * cross;
```

To:

```csharp
double cross = (U[v] * V[w]) - (U[w] * V[v]);
mx += (U[v] + U[w]) * cross;
my += (V[v] + V[w]) * cross;
```

From (`slice.md:285`):

```csharp
if (Math.Abs(area) > EpsilonPolicy.ZeroTolerance) { return new Point3d(mx / (3.0 * area), my / (3.0 * area), Elevations[layer]); }
```

To:

```csharp
Plane cut = new(Datum.Origin + (Elevations[layer] * Datum.Normal), Datum.XAxis, Datum.YAxis);
if (Math.Abs(area) > EpsilonPolicy.ZeroTolerance) { return cut.PointAt(mx / (3.0 * area), my / (3.0 * area)); }
```

From (`slice.md:288`):

```csharp
for (int v = first; v < last; v++) { sx += X[v]; sy += Y[v]; }
```

To:

```csharp
for (int v = first; v < last; v++) { sx += U[v]; sy += V[v]; }
```

From (`slice.md:290`):

```csharp
return new Point3d(sx / count, sy / count, Elevations[layer]);
```

To:

```csharp
return cut.PointAt(sx / count, sy / count);
```

Why: `AreaAt`, `PerimeterAt`, and `CentroidAt` must consume the primary planar channels, not project world X/Y as if every admitted datum were WorldXY. `Plane.PointAt` reconstructs the exact world-space chain and centroid only at the egress that asks for them. Supplying the layer makes the omitted per-vertex elevation derivable from the existing layer column and preserves one authority for elevation.

Ripples: pass `layer` to `ContourAt` in `libs/dotnet/Rasm.Fabrication/.planning/Additive/production.md:817`; at `:1402-1404`, retain `(Layer, Contour)` through the flattened root sequence before calling `ContourAt(row.Layer, row.Contour)`. In `libs/dotnet/Rasm.Fabrication/.planning/Additive/slicing.md:148,225-227`, pass the existing layer into `Ring` and onward to `ContourAt(layer, contour)`.

# 9. Delete the child CSR mirror

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:194`, anchor the `SliceStack` constructor; `:206`, anchor `public Arr<int> ChildPtr`; `:358`, anchor `int contours = contourPtr.Count - 1`.

From (`slice.md:194`):

```csharp
Arr<double> z, Arr<int> parent, Arr<int> childPtr, Arr<int> children, Arr<bool> open) =>
    (Elevations, LayerPtr, ContourPtr, X, Y, Z, Parent, ChildPtr, Children, Open) =
        (elevations, layerPtr, contourPtr, x, y, z, parent, childPtr, children, open);
```

To, applied to the task-7 datum/U/V constructor:

```csharp
Arr<double> v, Arr<int> parent, Arr<bool> open) =>
    (Datum, Elevations, LayerPtr, ContourPtr, U, V, Parent, Open) =
        (datum, elevations, layerPtr, contourPtr, u, v, parent, open);
```

From (`slice.md:206`):

```csharp
public Arr<int> ChildPtr { get; init; }
public Arr<int> Children { get; init; }
```

To:

```csharp
// SliceStack.ChildPtr DELETED
// SliceStack.Children DELETED
```

From (`slice.md:358`):

```csharp
int contours = contourPtr.Count - 1;
int[] childPtr = new int[contours + 1];
foreach (int p in parent) { if (p >= 0) { childPtr[p + 1]++; } }
for (int c = 0; c < contours; c++) { childPtr[c + 1] += childPtr[c]; }
int[] children = new int[parent.Count(static p => p >= 0)];
int[] cursor = (int[])childPtr.Clone();
for (int c = 0; c < contours; c++) { if (parent[c] >= 0) { children[cursor[parent[c]]++] = c; } }
```

To:

```csharp
// Slicing.Fold child CSR construction DELETED
```

Remove `childPtr` and `children` from `SliceStack.Of` and its constructor call.

Why: `Parent` is the authoritative immediate-parent forest. `ChildPtr`/`Children` are a full reverse index derived from that same column, are not read by any consumer, and force seven assembly lines, two public properties, two frozen arrays, and consumer-side mirror validation. Genuine downward traversal remains expressible from `Parent`; freezing its reverse duplicates one fact rather than preserving an independent capability.

Ripples: delete the `ChildPtr`/`Children` cardinality, monotonicity, and range clauses from `libs/dotnet/Rasm.Fabrication/.planning/Verify/audit.md:805,807,810,812`. No other consumer names either column.

# 10. Close `SliceStack` as a class instead of generating record surface

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:193`, anchor `public sealed record SliceStack`.

From (`slice.md:193`):

```csharp
public sealed record SliceStack {
```

To:

```csharp
public sealed class SliceStack {
```

Why: `SliceStack` is admitted only through its private constructor and `Of`; no consumer compares it structurally, deconstructs it, or uses `with`. After task 7 makes every channel get-only, record cloning and synthesized value equality are unused alternate surface over a wire whose authority is its admitted frozen columns. A sealed class preserves the one construction path and removes that generated public surface without adding a member or changing any consumer call.

# 11. Delete the `IsOpen` forwarding member

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:224`, anchor `public bool IsOpen`; `:226`, anchor `public Chain ContourAt`; `:237`, anchor `public IEnumerable<int> RootsOf`; `:249`, anchor `public double AreaAt`.

From (`slice.md:224`):

```csharp
public bool IsOpen(int contour) => Open[contour];
```

To:

```csharp
// SliceStack.IsOpen DELETED
```

Replace the five target-file reads in `ContourAt`, `RootsOf`, `AreaAt`, `PerimeterAt`, and `CentroidAt` with `Open[contour]` or `Open[c]` as appropriate.

Why: `IsOpen` is a one-expression public forwarding shell over an already-public immutable channel. It adds a second name for the same fact and no admission, derivation, or domain behavior. Direct indexing resolves the fact in one hop and removes one public member without removing the typed open-row capability.

Ripples: replace `stack.IsOpen(contour)` with `stack.Open[contour]` in `libs/dotnet/Rasm.Fabrication/.planning/Additive/slicing.md:147` and `libs/dotnet/Rasm.Fabrication/.planning/Verify/audit.md:814`; remove `IsOpen` from the latter page's package-surface roster at `:749`.

# 12. Use QuikGraph's edge-sequence acyclicity overload

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:210`, anchor `internal static Fin<SliceStack> Of`.

From (`slice.md:212`):

```csharp
BidirectionalGraph<int, SEdge<int>> forest = new(allowParallelEdges: false);
forest.AddVertexRange(Enumerable.Range(0, parent.Count));
for (int c = 0; c < parent.Count; c++) {
    if (parent[c] >= 0) { forest.AddEdge(new SEdge<int>(parent[c], c)); }
}
return forest.IsDirectedAcyclicGraph<int, SEdge<int>>()
    ? Fin.Succ(new SliceStack(elevations, layerPtr, contourPtr, x, y, z, parent, childPtr, children, open))
    : Fin.Fail<SliceStack>(key.InvalidResult());
```

To:

```csharp
return Enumerable.Range(0, parent.Count)
    .Where(c => parent[c] >= 0)
    .Select(c => new SEdge<int>(parent[c], c))
    .IsDirectedAcyclicGraph<int, SEdge<int>>()
        ? Fin.Succ(new SliceStack(datum, elevations, layerPtr, contourPtr, u, v, parent, open))
        : Fin.Fail<SliceStack>(key.InvalidResult());
```

Why: the checked-in QuikGraph catalogue exposes `IsDirectedAcyclicGraph(IEnumerable<TEdge>)`; isolated vertices cannot participate in a cycle. Building a mutable `BidirectionalGraph`, adding every vertex, then adding the same parent edges only to ask this predicate is a wrapper around the deeper overload and an avoidable materialization.

# 13. Return the kernel collection from `RootsOf`

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:237`, anchor `public IEnumerable<int> RootsOf`.

From (`slice.md:237`):

```csharp
public IEnumerable<int> RootsOf(int layer) {
    for (int c = LayerPtr[layer]; c < LayerPtr[layer + 1]; c++) {
        if (Parent[c] < 0 && !Open[c]) { yield return c; }
    }
}
```

To:

```csharp
public Seq<int> RootsOf(int layer) => toSeq(Enumerable
    .Range(LayerPtr[layer], LayerPtr[layer + 1] - LayerPtr[layer])
    .Where(contour => Parent[contour] < 0 && !Open[contour]));
```

Why: every consumer immediately calls `toSeq` on this iterator because `Seq<T>` is the branch's public ordered carrier. Returning the carrier once removes the iterator state machine and consumer wrappers without removing the root query.

Ripples: remove `toSeq(...)` around `RootsOf(layer)` in `libs/dotnet/Rasm.Fabrication/.planning/Additive/production.md:816,1403`.

# 14. Use the generated perpendicular-axis projection directly

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:377`, anchor `Axis v = Axis.Get(frame.Vertical.V)`.

From (`slice.md:377`):

```csharp
Axis v = Axis.Get(frame.Vertical.V);
```

To:

```csharp
Axis v = frame.Vertical.V;
```

Why: `Axis.V` already returns the canonical generated `Axis` item. Converting that item back to its key and sending it through the throwing generated `Get` lookup immediately reconstructs the same singleton. Reading the projection directly removes a lookup and uses the Thinktecture owner as generated, with no semantic or consumer change.

# 15. Inline the one-call extremes helper into nesting assembly

Location: `libs/dotnet/Rasm/.planning/Meshing/slice.md:380`, anchor `for (int i = 0; i < n; i++)`; `:402`, anchor `static ((double, double, double, double) Box, Point3d Anchor) Extremes`.

From (`slice.md:380`):

```csharp
for (int i = 0; i < n; i++) {
    (boxes[i], anchors[i]) = Extremes(closed[i].Points, frame.Vertical);
}
```

To:

```csharp
for (int i = 0; i < n; i++) {
    Polyline ring = closed[i].Points;
    (double loU, double hiU, double loV, double hiV) = (double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity, double.NegativeInfinity);
    Point3d anchor = ring[0];
    (double aU, double aV) = (frame.Vertical.U.Read(anchor), frame.Vertical.V.Read(anchor));
    for (int p = 0; p < ring.Count - 1; p++) {
        (double u, double v) = (frame.Vertical.U.Read(ring[p]), frame.Vertical.V.Read(ring[p]));
        (loU, hiU, loV, hiV) = (double.Min(loU, u), double.Max(hiU, u), double.Min(loV, v), double.Max(hiV, v));
        if (u > aU || (u == aU && v > aV)) { (anchor, aU, aV) = (ring[p], u, v); }
    }
    (boxes[i], anchors[i]) = ((loU, hiU, loV, hiV), anchor);
}
```

From (`slice.md:402`):

```csharp
static ((double, double, double, double) Box, Point3d Anchor) Extremes(Polyline ring, Axis vertical) {
    (double loU, double hiU, double loV, double hiV) = (double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity, double.NegativeInfinity);
    Point3d anchor = ring[0];
    (double aU, double aV) = (vertical.U.Read(anchor), vertical.V.Read(anchor));
    for (int i = 0; i < ring.Count - 1; i++) {
        (double pu, double pv) = (vertical.U.Read(ring[i]), vertical.V.Read(ring[i]));
        (loU, hiU, loV, hiV) = (double.Min(loU, pu), double.Max(hiU, pu), double.Min(loV, pv), double.Max(hiV, pv));
        if (pu > aU || (pu == aU && pv > aV)) { (anchor, aU, aV) = (ring[i], pu, pv); }
    }
    return ((loU, hiU, loV, hiV), anchor);
}
```

To:

```csharp
// Slicing.Extremes DELETED
```

Why: `Extremes` has one call site, no independent domain meaning, and exists only to fill the two arrays allocated immediately above it. Inlining keeps the bbox and rightmost-anchor state beside those arrays, removes one private member and tuple-return hop, and leaves the exact containment and QuikGraph reduction unchanged.
