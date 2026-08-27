# 1. Derive lattice admission through one generated fold
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L45-L70**
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PatternLattice {
    public static readonly PatternLattice Oblique = new("oblique", static (a, b, orientation, _) =>
        Sine(a, b) > orientation.Value);
    public static readonly PatternLattice Rectangular = new("rectangular", static (a, b, orientation, _) =>
        Sine(a, b) > orientation.Value && Cosine(a, b) <= orientation.Value);
    public static readonly PatternLattice Centered = new("centered", static (a, b, orientation, _) =>
        Sine(a, b) > orientation.Value && Cosine(a, b) <= orientation.Value);
    public static readonly PatternLattice Square = new("square", static (a, b, orientation, fraction) =>
        Disparity(a, b) <= fraction.Value && Cosine(a, b) <= orientation.Value);
    public static readonly PatternLattice Hexagonal = new("hexagonal", static (a, b, orientation, fraction) =>
        Disparity(a, b) <= fraction.Value && Math.Abs(SignedCosine(a, b) + 0.5) <= orientation.Value);

    static double Cross((double U, double V) a, (double U, double V) b) => (a.U * b.V) - (a.V * b.U);
    static double Dot((double U, double V) a, (double U, double V) b) => (a.U * b.U) + (a.V * b.V);
    static double Len((double U, double V) a) => Math.Sqrt((a.U * a.U) + (a.V * a.V));

    static double Sine((double U, double V) a, (double U, double V) b) => Math.Abs(Cross(a, b)) / (Len(a) * Len(b));
    static double SignedCosine((double U, double V) a, (double U, double V) b) => Dot(a, b) / (Len(a) * Len(b));
    static double Cosine((double U, double V) a, (double U, double V) b) => Math.Abs(SignedCosine(a, b));
    static double Disparity((double U, double V) a, (double U, double V) b) => Math.Abs(Len(a) - Len(b)) / Len(a);

    [UseDelegateFromConstructor] public partial bool Admits((double U, double V) a, (double U, double V) b, Tolerance orientation, Tolerance fraction);
}
```
**To**
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PatternLattice {
    public static readonly PatternLattice Oblique     = new("oblique");
    public static readonly PatternLattice Rectangular = new("rectangular");
    public static readonly PatternLattice Centered    = new("centered");
    public static readonly PatternLattice Square      = new("square");
    public static readonly PatternLattice Hexagonal   = new("hexagonal");

    public bool Admits((double U, double V) a, (double U, double V) b, Tolerance angle, Tolerance ratio) {
        (double sine, double cosine, double signedCosine, double disparity) = Metrics(a, b);
        return Map(
            oblique: sine > angle.Value,
            rectangular: sine > angle.Value && cosine <= angle.Value,
            centered: sine > angle.Value && cosine <= angle.Value,
            square: disparity <= ratio.Value && cosine <= angle.Value,
            hexagonal: disparity <= ratio.Value && Math.Abs(signedCosine + 0.5) <= angle.Value);
    }

    static (double Sine, double Cosine, double SignedCosine, double Disparity) Metrics(
        (double U, double V) a, (double U, double V) b) {
        double aLength = Math.Sqrt((a.U * a.U) + (a.V * a.V));
        double bLength = Math.Sqrt((b.U * b.U) + (b.V * b.V));
        double denominator = aLength * bLength;
        double signedCosine = ((a.U * b.U) + (a.V * b.V)) / denominator;
        return (Math.Abs((a.U * b.V) - (a.V * b.U)) / denominator, Math.Abs(signedCosine), signedCosine,
            Math.Abs(aLength - bLength) / Math.Max(aLength, bLength));
    }
}
```
**Why**
Five constructor delegates repeat one small metric projection, six private arithmetic helpers recompute both lengths, and the asymmetric disparity changes when callers swap the two basis vectors. The generated exhaustive `Map` keeps the theorem-closed row dispatch while one symmetric metric calculation owns the proof.
**Change**
Keep only lattice identity on each smart-enum row, compute normalized sine, signed cosine, and symmetric relative length disparity once, and dispatch those values through generated `Map`.
**Delta**
LOC -1; members -6; delegate columns -5

# 2. Store immutable seat sets on their wallpaper rows
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L74-L104**
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WallpaperGroup {
    public static readonly WallpaperGroup P1   = new("p1",   number: 1,  PatternLattice.Oblique,     static () => SeatKernel.Rows(order: 1, mirrorAxis: None, glide: None, centered: false));
    public static readonly WallpaperGroup P2   = new("p2",   number: 2,  PatternLattice.Oblique,     static () => SeatKernel.Rows(order: 2, mirrorAxis: None, glide: None, centered: false));
    public static readonly WallpaperGroup Pm   = new("pm",   number: 3,  PatternLattice.Rectangular, static () => SeatKernel.Rows(order: 1, mirrorAxis: Some(0.0), glide: None, centered: false));
    public static readonly WallpaperGroup Pg   = new("pg",   number: 4,  PatternLattice.Rectangular, static () => SeatKernel.Rows(order: 1, mirrorAxis: None, glide: Some((0.0, (0.5, 0.0))), centered: false));
    public static readonly WallpaperGroup Cm   = new("cm",   number: 5,  PatternLattice.Centered,    static () => SeatKernel.Rows(order: 1, mirrorAxis: Some(0.0), glide: None, centered: true));
    public static readonly WallpaperGroup Pmm  = new("pmm",  number: 6,  PatternLattice.Rectangular, static () => SeatKernel.Rows(order: 2, mirrorAxis: Some(0.0), glide: None, centered: false));
    public static readonly WallpaperGroup Pmg  = new("pmg",  number: 7,  PatternLattice.Rectangular, static () => SeatKernel.Rows(order: 2, mirrorAxis: Some(Math.PI / 2.0), glide: Some((0.0, (0.5, 0.0))), centered: false));
    public static readonly WallpaperGroup Pgg  = new("pgg",  number: 8,  PatternLattice.Rectangular, static () => SeatKernel.Rows(order: 2, mirrorAxis: None, glide: Some((0.0, (0.5, 0.5))), centered: false));
    public static readonly WallpaperGroup Cmm  = new("cmm",  number: 9,  PatternLattice.Centered,    static () => SeatKernel.Rows(order: 2, mirrorAxis: Some(0.0), glide: None, centered: true));
    public static readonly WallpaperGroup P4   = new("p4",   number: 10, PatternLattice.Square,      static () => SeatKernel.Rows(order: 4, mirrorAxis: None, glide: None, centered: false));
    public static readonly WallpaperGroup P4m  = new("p4m",  number: 11, PatternLattice.Square,      static () => SeatKernel.Rows(order: 4, mirrorAxis: Some(0.0), glide: None, centered: false));
    public static readonly WallpaperGroup P4g  = new("p4g",  number: 12, PatternLattice.Square,      static () => SeatKernel.Rows(order: 4, mirrorAxis: Some(Math.PI / 4.0), glide: Some((0.0, (0.5, 0.5))), centered: false));
    public static readonly WallpaperGroup P3   = new("p3",   number: 13, PatternLattice.Hexagonal,   static () => SeatKernel.Rows(order: 3, mirrorAxis: None, glide: None, centered: false));
    public static readonly WallpaperGroup P3m1 = new("p3m1", number: 14, PatternLattice.Hexagonal,   static () => SeatKernel.Rows(order: 3, mirrorAxis: Some(0.0), glide: None, centered: false));
    public static readonly WallpaperGroup P31m = new("p31m", number: 15, PatternLattice.Hexagonal,   static () => SeatKernel.Rows(order: 3, mirrorAxis: Some(Math.PI / 6.0), glide: None, centered: false));
    public static readonly WallpaperGroup P6   = new("p6",   number: 16, PatternLattice.Hexagonal,   static () => SeatKernel.Rows(order: 6, mirrorAxis: None, glide: None, centered: false));
    public static readonly WallpaperGroup P6m  = new("p6m",  number: 17, PatternLattice.Hexagonal,   static () => SeatKernel.Rows(order: 6, mirrorAxis: Some(0.0), glide: None, centered: false));

    public int Number { get; }
    public PatternLattice Lattice { get; }

    [UseDelegateFromConstructor] public partial Arr<PatternSeat> Seats();
}

internal static class SeatKernel {
    internal static Arr<PatternSeat> Rows(int order, Option<double> mirrorAxis, Option<(double Axis, (double U, double V) Shift)> glide, bool centered);
}
```
**To**
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WallpaperGroup {
    public static readonly WallpaperGroup P1   = new("p1",   PatternLattice.Oblique,     Rows(1, None, None, false));
    public static readonly WallpaperGroup P2   = new("p2",   PatternLattice.Oblique,     Rows(2, None, None, false));
    public static readonly WallpaperGroup Pm   = new("pm",   PatternLattice.Rectangular, Rows(1, Some(0.0), None, false));
    public static readonly WallpaperGroup Pg   = new("pg",   PatternLattice.Rectangular, Rows(1, None, Some((0.0, (0.5, 0.0))), false));
    public static readonly WallpaperGroup Cm   = new("cm",   PatternLattice.Centered,    Rows(1, Some(0.0), None, true));
    public static readonly WallpaperGroup Pmm  = new("pmm",  PatternLattice.Rectangular, Rows(2, Some(0.0), None, false));
    public static readonly WallpaperGroup Pmg  = new("pmg",  PatternLattice.Rectangular, Rows(2, Some(Math.PI / 2.0), Some((0.0, (0.5, 0.0))), false));
    public static readonly WallpaperGroup Pgg  = new("pgg",  PatternLattice.Rectangular, Rows(2, None, Some((0.0, (0.5, 0.5))), false));
    public static readonly WallpaperGroup Cmm  = new("cmm",  PatternLattice.Centered,    Rows(2, Some(0.0), None, true));
    public static readonly WallpaperGroup P4   = new("p4",   PatternLattice.Square,      Rows(4, None, None, false));
    public static readonly WallpaperGroup P4m  = new("p4m",  PatternLattice.Square,      Rows(4, Some(0.0), None, false));
    public static readonly WallpaperGroup P4g  = new("p4g",  PatternLattice.Square,      Rows(4, Some(Math.PI / 4.0), Some((0.0, (0.5, 0.5))), false));
    public static readonly WallpaperGroup P3   = new("p3",   PatternLattice.Hexagonal,   Rows(3, None, None, false));
    public static readonly WallpaperGroup P3m1 = new("p3m1", PatternLattice.Hexagonal,   Rows(3, Some(0.0), None, false));
    public static readonly WallpaperGroup P31m = new("p31m", PatternLattice.Hexagonal,   Rows(3, Some(Math.PI / 6.0), None, false));
    public static readonly WallpaperGroup P6   = new("p6",   PatternLattice.Hexagonal,   Rows(6, None, None, false));
    public static readonly WallpaperGroup P6m  = new("p6m",  PatternLattice.Hexagonal,   Rows(6, Some(0.0), None, false));

    public PatternLattice Lattice { get; }
    public Arr<PatternSeat> Seats { get; }

    static Arr<PatternSeat> Rows(int order, Option<double> mirrorAxis, Option<(double Axis, (double U, double V) Shift)> glide, bool centered);
}
```
**Why**
The integer ordinal duplicates the canonical Hermann-Mauguin key and has no search-resolved reader. A nullary delegate recomputes immutable theorem data, while the one-method `SeatKernel` type is only a forwarding shell.
**Change**
Materialize each closed seat set once during row initialization, store it as a direct smart-enum column, remove the unused ordinal, and move the sole generator into its actual owner.
**Ripples**
Change `group.Seats()` at `MaterialSymmetry.Admits` and `plan.Group.Seats()` at `Patterning.Orbit` in this target to `.Seats`. Repository search found no external `WallpaperGroup.Number` read or `Seats()` call.
**Delta**
LOC -4; types -1; members -1; delegate columns -17

# 3. Rename the rotation vocabulary and derive it from its key
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L106-L120**
```csharp
[SmartEnum<int>]
public sealed partial class SymmetryFold {
    public static readonly SymmetryFold Free    = new(key: 0, admits: static (_, _) => true);
    public static readonly SymmetryFold Fixed   = new(key: 1, admits: static (spin, cone) => Congruent(spin: spin, order: 1, cone: cone));
    public static readonly SymmetryFold Half    = new(key: 2, admits: static (spin, cone) => Congruent(spin: spin, order: 2, cone: cone));
    public static readonly SymmetryFold Third   = new(key: 3, admits: static (spin, cone) => Congruent(spin: spin, order: 3, cone: cone));
    public static readonly SymmetryFold Quarter = new(key: 4, admits: static (spin, cone) => Congruent(spin: spin, order: 4, cone: cone));
    public static readonly SymmetryFold Sixth   = new(key: 6, admits: static (spin, cone) => Congruent(spin: spin, order: 6, cone: cone));

    static bool Congruent(double spin, int order, double cone) => Math.Abs(Math.IEEERemainder(spin, Math.Tau / order)) <= cone;

    [UseDelegateFromConstructor] public partial bool Admits(double spin, double cone);

    public bool Admits(double spin) => Admits(spin, EpsilonPolicy.SqrtEpsilon);
}
```
**To**
```csharp
[SmartEnum<int>]
public sealed partial class RotationOrder {
    public static readonly RotationOrder Free      = new(0);
    public static readonly RotationOrder Identity  = new(1);
    public static readonly RotationOrder Twofold   = new(2);
    public static readonly RotationOrder Threefold = new(3);
    public static readonly RotationOrder Fourfold  = new(4);
    public static readonly RotationOrder Sixfold   = new(6);

    public bool Admits(double angle, double tolerance) =>
        Key == 0 || Math.Abs(Math.IEEERemainder(angle, Math.Tau / Key)) <= tolerance;
}
```
**Why**
“Fold,” “Half,” and “Third” do not name crystallographic rotational order. The generated integer `Key` already contains the order, so six delegates, a private forwarder, and a context-free epsilon overload duplicate data or hide the caller's tolerance owner.
**Change**
Rename the owner and rows to standard rotational-order terminology, derive congruence from `Key`, and require the caller-owned angular tolerance explicitly.
**Ripples**
Rename `SymmetryFold` and its rows in `libs/dotnet/Rasm.Materials/.planning/Component/component.md` (`ComponentSymmetry`), `libs/dotnet/Rasm.Fabrication/.planning/Nesting/nfp.md` (`LawKey`, `Nest.Fold`, `Nest.Moves`), and `libs/dotnet/Rasm.Fabrication/.planning/Nesting/stock.md` (`StockFrame` legality). Update the corresponding package and law prose in those files.
**Delta**
LOC -3; members -2; delegate columns -6

# 4. Admit realized symmetry once under the caller tolerance
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L144-L155**
```csharp
public sealed record MaterialSymmetry(SymmetryFold Fold, MirrorGrant Mirror) {
    public static readonly MaterialSymmetry Free = new(SymmetryFold.Free, MirrorGrant.Reflective);

    public bool Admits(WallpaperGroup group) {
        Arr<PatternSeat> seats = group.Seats();
        bool mirrors = seats.Exists(static row => row.Mirror);
        return seats.Filter(static row => !row.Mirror)
                    .ForAll(row => Fold.Admits(spin: Math.Atan2(row.Sin, row.Cos)))
            && (Mirror.Rights.Admits(MirrorRight.Place) || !mirrors)
            && (!Mirror.Rights.Admits(MirrorRight.Pair) || mirrors);
    }
}
```
**To**
```csharp
public sealed record MaterialSymmetry(RotationOrder Rotation, MirrorGrant Mirror) {
    public static readonly MaterialSymmetry Free = new(RotationOrder.Free, MirrorGrant.Reflective);

    public bool Admits(WallpaperGroup group, Tolerance angle) => group.Seats.ForAll(seat =>
        seat.Mirror
            ? Mirror.Rights.Admits(MirrorRight.Place)
            : Rotation.Admits(Math.Atan2(seat.Sin, seat.Cos), angle.Value));
}
```
**Why**
The current final clause incorrectly rejects every rotation-only group when a material carries the `Pair` right; pairing is an obligation on reflected placements, not a requirement that every admitted group contain reflection. The rotation check also bypasses the plan's resolved tolerance by selecting a global epsilon overload.
**Change**
Fold every realized seat once: rotation seats use the caller's angular tolerance and reflection seats require `Place`. Keep `Pair` enforcement in orbit emission, where reflected pairs actually exist.
**Ripples**
Rename `MaterialSymmetry.Fold` to `.Rotation` in `libs/dotnet/Rasm.Materials/.planning/Component/component.md`, `libs/dotnet/Rasm.Fabrication/.planning/Nesting/nfp.md`, and `libs/dotnet/Rasm.Fabrication/.planning/Nesting/stock.md`. `libs/dotnet/Rasm/.planning/Parametric/panelize.md` continues to read only `Mirror.Rights`; no change is required there.
**Delta**
LOC -4; members 0

# 5. Remove the unexecuted density dependency
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L37-L37**
```csharp
using Rasm.Spatial;
```
**To**
```csharp
// using Rasm.Spatial DELETED
```
**Why**
The only fence use is `PatternPlan.Density`, but no operation body samples it. `ScalarField` is also an unbounded fallible scalar, not an admitted probability, so it cannot lawfully drive the promised deterministic unit draw without a normalization and failure fold that do not exist.
**Change**
Delete the import with the dead plan column. A future thinning feature must enter as an admitted unit-interval projection and execute in the orbit fold, including pair-unit sampling and propagated field failures.
**Delta**
LOC -1; symbols -1

# 6. Make the plan an orbit candidate with one complete gate
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L158-L182**
```csharp
public sealed record PatternPlan(
    WallpaperGroup Group, (double U, double V) BasisA, (double U, double V) BasisB,
    Arr<(double U, double V, double Spin)> Anchors, double Extent, (double U, double V) Root,
    LogMapAlgorithm Algorithm, Option<ScalarField> Density = default,
    Option<MaterialSymmetry> Law = default) : IValidityEvidence {
    public static Fin<PatternPlan> Of(
        WallpaperGroup group, (double U, double V) basisA, (double U, double V) basisB,
        Arr<(double U, double V, double Spin)> anchors, double extent, (double U, double V) root,
        LogMapAlgorithm algorithm, Context context, Op key,
        Option<ScalarField> density = default, Option<MaterialSymmetry> law = default) {
        PatternPlan plan = new(group, basisA, basisB, anchors, extent, root, algorithm, density, law);
        return plan.IsValid && group.Lattice.Admits(
                basisA, basisB,
                orientation: context.For(lane: ToleranceLane.Orientation),
                fraction: context.For(lane: ToleranceLane.Fraction))
            ? Fin.Succ(plan)
            : Fin.Fail<PatternPlan>(key.InvalidInput());
    }

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: Extent),
        ValidityClaim.CountAtLeast(count: Anchors.Count, floor: 1),
        Anchors.All(static a => a.U is >= 0.0 and < 1.0 && a.V is >= 0.0 and < 1.0),
        Law.Map(law => law.Admits(group: Group)).IfNone(true));
}
```
**To**
```csharp
public sealed record PatternPlan(
    WallpaperGroup Group, (double U, double V) BasisA, (double U, double V) BasisB,
    Arr<(double U, double V, double Spin)> Anchors, PositiveMagnitude Extent,
    Tolerance AngleTolerance, Tolerance LengthTolerance, MaterialSymmetry Symmetry) : IValidityEvidence {
    public bool IsValid => Group is not null && Symmetry is not null && ValidityClaim.All(
        ValidityClaim.CountAtLeast(Anchors.Count, 1),
        Anchors.All(static anchor =>
            double.IsFinite(anchor.U) && double.IsFinite(anchor.V) && double.IsFinite(anchor.Spin)
            && anchor.U is >= 0.0 and < 1.0 && anchor.V is >= 0.0 and < 1.0),
        Group.Lattice.Admits(BasisA, BasisB, AngleTolerance, LengthTolerance),
        Symmetry.Admits(Group, AngleTolerance));
}
```
**Why**
Both search-resolved callers bypass `Of`, while `Patterning.Apply` checks an `IsValid` that omits the lattice, finite spins, and map root. Root, log algorithm, and density are not orbit data, and a positive extent should not remain a repeatedly guarded scalar.
**Change**
Delete the bypassed factory, retain only orbit data, type extent with the generated positive-magnitude owner, carry the two resolved tolerance lanes required by the lattice and material proof, make material symmetry explicit, and put every remaining claim on the one value `Apply` gates.
**Ripples**
In `libs/dotnet/Rasm.Materials/.planning/Component/masonry.md`, change `BondGeometry.Plan` to return `Fin<PatternPlan>`, admit the computed extent through `Op.AcceptValidated<PositiveMagnitude>`, accept and thread `Context`, pass `ToleranceLane.Orientation` and `ToleranceLane.Fraction`, and supply `MaterialSymmetry.Free`; remove its dummy root and `LogMapAlgorithm.VectorHeat`. Update `Courses` and `Course` to bind that result. In `libs/dotnet/Rasm/.planning/Drawing/hatch.md`, read `motif.Orbit.Extent.Value` in `Motifs`; no search-resolved `PatternPlan` construction exists there.
**Delta**
LOC -13; members -2; fields -1

# 7. Replace request and result unions with typed operation shapes
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L184-L219**
```csharp
public sealed record PatternPolicy(
    double HeatTime, double HeatMultiplier, GeodesicTracePolicy Trace, WindowPropagationPolicy Windows,
    Tolerance FrameBudget) : IValidityEvidence {
    public static PatternPolicy Of(SurfaceResult.UvTessellation source, Context context, double multiplier = 1.0) =>
        source.Mesh.Cache.MeanEdgeLength switch {
            double h => new PatternPolicy(
                HeatTime: multiplier * h * h, HeatMultiplier: multiplier,
                GeodesicTracePolicy.Default, WindowPropagationPolicy.Default,
                FrameBudget: context.For(lane: ToleranceLane.Orientation)),
        };

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: HeatTime), ValidityClaim.Positive(value: HeatMultiplier), FrameBudget.IsValid);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PatternOp {
    private PatternOp() { }

    public sealed record Orbit(PatternPlan Plan) : PatternOp;
    public sealed record Map(SurfaceResult.UvTessellation Source, PatternPlan Plan, PatternPolicy Policy) : PatternOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InstanceStream {
    private InstanceStream() { }

    public sealed record Planar(
        Arr<(double U, double V)> Site, Arr<double> Spin, Arr<bool> Mirrored, Arr<int> Anchor, Arr<int> Seat, Arr<Option<int>> PairOf) : InstanceStream;

    public sealed record Mapped(
        Arr<Point3d> Origin, Arr<(double U, double V)> Uv, Arr<Vector3d> XAxis, Arr<Vector3d> ZAxis,
        Arr<double> Spin, Arr<bool> Mirrored, Arr<int> Anchor, Arr<int> Seat, Arr<Option<int>> PairOf, Arr<int> Face,
        Arr<double> Radius, Arr<double> FrameDefect) : InstanceStream;
}
```
**To**
```csharp
public sealed record PatternPolicy(
    PositiveMagnitude HeatTime, LogMapAlgorithm Algorithm,
    GeodesicTracePolicy Trace, WindowPropagationPolicy Windows, Tolerance FrameTolerance) : IValidityEvidence {
    public static Fin<PatternPolicy> Of(
        SurfaceResult.UvTessellation source, Context context, LogMapAlgorithm algorithm, Op key,
        double multiplier = 1.0) {
        if (source is null || algorithm is null) { return Fin.Fail<PatternPolicy>(key.InvalidInput()); }
        double h = source.Mesh.Cache.MeanEdgeLength;
        return key.AcceptValidated<PositiveMagnitude>(candidate: multiplier * h * h)
            .Map(time => new PatternPolicy(time, algorithm, GeodesicTracePolicy.Default,
                WindowPropagationPolicy.Default, context.For(ToleranceLane.Orientation)));
    }

    public bool IsValid => Algorithm is not null && Band.Positive.Admits(HeatTime.Value) && FrameTolerance.IsValid;
}

public sealed record PatternMap(
    SurfaceResult.UvTessellation Source, PatternPlan Plan,
    (double U, double V) Root, PatternPolicy Policy);

public sealed record InstanceBatch(
    Arr<double> Spin, Arr<bool> Mirrored, Arr<int> Anchor, Arr<int> Seat, Arr<Option<int>> PairOf);

public sealed record PlanarInstances(Arr<(double U, double V)> Site, InstanceBatch Instances);

public sealed record SurfaceInstances(
    Arr<Point3d> Origin, Arr<(double U, double V)> Uv, Arr<Vector3d> XAxis, Arr<Vector3d> ZAxis,
    Arr<int> Face, Arr<double> FrameDefect, InstanceBatch Instances);
```
**Why**
`PatternOp.Orbit` deterministically returns only `InstanceStream.Planar`, and `PatternOp.Map` only `InstanceStream.Mapped`; the two unions therefore force every caller to select a case and immediately inspect the corresponding output case. `HeatMultiplier` survives after deriving heat time without an operational read, five provenance arrays are duplicated, and `Radius` has no input, derivation, or consumer.
**Change**
Keep typed heat time plus the algorithm and solver policy that mapping actually reads, move map-only root and surface into `PatternMap`, replace the shadowing unions with typed request/result records, share genuine placement provenance in `InstanceBatch`, and delete the fabricated radius column.
**Ripples**
Rename planar consumers to `PlanarInstances` in `libs/dotnet/Rasm.Materials/.planning/Component/masonry.md` (`Bands`, `Placed`) and `libs/dotnet/Rasm/.planning/Drawing/hatch.md` (`Motifs`, `Stamp`). Change `libs/dotnet/Rasm.Fabrication/.planning/Nesting/nfp.md` `NestParts.Of` to accept `InstanceBatch`, read its three needed columns directly, and delete `NestParts.Columns`. Replace `InstanceStream` boundary labels in `libs/dotnet/Rasm/ARCHITECTURE.md`, `libs/dotnet/Rasm.Materials/ARCHITECTURE.md`, and `libs/dotnet/Rasm.Fabrication/ARCHITECTURE.md` with the exact `PlanarInstances`/`SurfaceInstances`/`InstanceBatch` shapes each edge consumes.
**Delta**
LOC -4; types -2; members -6

# 8. Dispatch through typed Apply overloads
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L221-L233**
```csharp
public static class Patterning {
    public static Fin<InstanceStream> Apply(PatternOp op, Op? key = null) =>
        op.Switch(
            state: key.OrDefault(),
            orbit: static (k, o) => OrbitOf(o.Plan, k).Map(static planar => (InstanceStream)planar),
            map:   static (k, m) => !m.Policy.IsValid
                ? Fin.Fail<InstanceStream>(k.InvalidInput())
                : OrbitOf(m.Plan, k).Bind(planar => MapOf(m.Source, m.Plan, m.Policy, planar, k)));

    // --- [ORBIT]
    static Fin<InstanceStream.Planar> OrbitOf(PatternPlan plan, Op key) {
        if (!plan.IsValid) { return Fin.Fail<InstanceStream.Planar>(key.InvalidInput()); }
        Arr<PatternSeat> seats = plan.Group.Seats();
```
**To**
```csharp
public static class Patterning {
    public static Fin<PlanarInstances> Apply(PatternPlan plan, Op? key = null) =>
        plan is not null && plan.IsValid
            ? Fin.Succ(Orbit(plan))
            : Fin.Fail<PlanarInstances>(key.OrDefault().InvalidInput());

    public static Fin<SurfaceInstances> Apply(PatternMap map, Op? key = null) {
        Op operation = key.OrDefault();
        if (map is null || !map.Plan.IsValid || !map.Policy.IsValid
            || !double.IsFinite(map.Root.U) || !double.IsFinite(map.Root.V)) {
            return Fin.Fail<SurfaceInstances>(operation.InvalidInput());
        }
        return RootVertex(map.Source, map.Root).Bind(root =>
            LogField(map.Source, root, map.Policy, operation).Bind(log =>
                Instances(map.Source, root, Orbit(map.Plan), log, map.Policy, operation)));
    }

    // --- [ORBIT]
    static PlanarInstances Orbit(PatternPlan plan) {
        Arr<PatternSeat> seats = plan.Group.Seats;
```
**Why**
Generated union dispatch adds no exhaustiveness value when each input case fixes a distinct output type, and it makes both current orbit consumers carry an impossible mapped failure. Typed overloads retain one owner and one verb while the compiler now fixes each result shape.
**Change**
Admit the orbit plan once in its overload; admit the plan, policy, and finite root once in the mapping overload; compose root lookup, log-field construction, and instance mapping directly; and make the admitted orbit kernel a total value function shared by both overloads.
**Ripples**
In `libs/dotnet/Rasm.Materials/.planning/Component/masonry.md`, replace `Patterning.Apply(new PatternOp.Orbit(plan), key)` and its result `Switch` with `Patterning.Apply(plan, key)`. In `libs/dotnet/Rasm/.planning/Drawing/hatch.md`, replace `Patterning.Apply(new PatternOp.Orbit(motif.Orbit), key)` and the impossible mapped-result guard with `Patterning.Apply(motif.Orbit, key)`. Update both files' prose and diagrams to remove `PatternOp.Orbit` and `InstanceStream.Planar`.
**Delta**
LOC +6; public members +1; union dispatches -2

# 9. Freeze the planar result without a success wrapper
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L266-L269**
```csharp
        return Fin.Succ(new InstanceStream.Planar(
            new([.. site]), new([.. spin]), new([.. mirrored]), new([.. anchor]), new([.. seat]),
            new([.. pair.Select(static p => p < 0 ? Option<int>.None : Some(p))])));
    }
```
**To**
```csharp
        return new PlanarInstances(new([.. site]),
            new InstanceBatch(new([.. spin]), new([.. mirrored]), new([.. anchor]), new([.. seat]),
                new([.. pair.Select(static value => value < 0 ? Option<int>.None : Some(value))])));
    }
```
**Why**
After the public boundary admits the plan, the orbit body has no failure arm. Its `Fin.Succ` is a redundant result layer, and the common placement columns belong in the shared batch consumed by both output modes.
**Change**
Return the total planar value directly and freeze the five common provenance columns into `InstanceBatch` once.
**Delta**
LOC 0; wrappers -1

# 10. Delete the one-call map forwarding shell
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L277-L282**
```csharp
    // --- [SURFACE_MAP]
    static Fin<InstanceStream> MapOf(
        SurfaceResult.UvTessellation source, PatternPlan plan, PatternPolicy policy, InstanceStream.Planar planar, Op key) =>
        RootVertex(source, plan.Root).Bind(root =>
            LogField(source, root, plan, policy, key).Bind(log =>
                Instances(source, root, planar, log, plan, policy, key)));
```
**To**
```csharp
    // --- [SURFACE_MAP]
    // MapOf DELETED
```
**Why**
`MapOf` is called once and only forwards values through root lookup, log-field construction, and instance mapping. The mapping `Apply` overload now exposes that result composition without an extra name hop.
**Change**
Delete the shell and retain the three meaningful algorithm boundaries.
**Delta**
LOC -5; members -1

# 11. Narrow map kernels to their operational inputs
**From — libs/dotnet/Rasm/.planning/Parametric/patternmap.md:L284-L290**
```csharp
    static Fin<int> RootVertex(SurfaceResult.UvTessellation source, (double U, double V) rootUv);

    static Fin<Arr<(double U, double V)>> LogField(SurfaceResult.UvTessellation source, int root, PatternPlan plan, PatternPolicy policy, Op key);

    static Fin<InstanceStream> Instances(
        SurfaceResult.UvTessellation source, int root, InstanceStream.Planar planar, Arr<(double U, double V)> log,
        PatternPlan plan, PatternPolicy policy, Op key);
```
**To**
```csharp
    static Fin<int> RootVertex(SurfaceResult.UvTessellation source, (double U, double V) rootUv);

    static Fin<Arr<(double U, double V)>> LogField(
        SurfaceResult.UvTessellation source, int root, PatternPolicy policy, Op key);

    static Fin<SurfaceInstances> Instances(
        SurfaceResult.UvTessellation source, int root, PlanarInstances planar,
        Arr<(double U, double V)> log, PatternPolicy policy, Op key);
```
**Why**
Once root and algorithm leave the orbit plan, neither mapping kernel needs that owner. Retaining it widens dependencies and invites a second read of data the orbit has already projected.
**Change**
Let `LogField` read algorithm and `HeatTime.Value` from `PatternPolicy`; let `Instances` consume only the admitted planar result and mapping policy; emit `SurfaceInstances` with one survivor `InstanceBatch`.
**Delta**
LOC +1; parameters -2; result unions -1
