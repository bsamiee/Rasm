# `predicates.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Numerics/predicates.md`

Apply the moves in order. Counts refer to authored C# fence members/lines, not generated Thinktecture surface. Repeated consumer renames are listed as ripples instead of compatibility aliases.

Authority: `CLAUDE.md`; the owning `libs/`, `libs/dotnet/`, and `libs/dotnet/Rasm/` architecture and ruling surfaces; `docs/stacks/csharp/`; both checked-in API tiers, especially Thinktecture, LanguageExt, PeterO.Numbers, DoubleDouble, BigRational, and RhinoCommon; all eight `Numerics` pages; exact `libs/dotnet/` symbol consumers; and the refinement-audit history at `f17b2d852`/`0821ec6c4`.

## 1. Keep the settled `Implicit` owner, but nest and name its three construction cases

Location: target anchors `[Union<Point3d, Ssi, Lpi, Tpi>]`, `Ssi`, `Lpi`, `Tpi`, and every `Tpi.Det3` read.

Retain `Implicit`. `Rasm/RULINGS.md`, `Rasm/ARCHITECTURE.md`, the target boundary law, and the tessellation owner all use *implicit* in its computational-geometry sense: coordinates are defined symbolically and evaluated only at emission. Prefixing that established concept with the consumer (`PredicatePoint`) adds context rather than meaning and causes a repo-wide rename with no semantic or symbol reduction.

### 1a. Replace the abbreviations and seat the payloads inside their union owner

From:

```csharp
[Union<Point3d, Ssi, Lpi, Tpi>(T1Name = "Explicit", T2Name = "Ssi", T3Name = "Lpi", T4Name = "Tpi")]
public readonly partial struct Implicit {
```

To:

```csharp
[Union<Point3d, Implicit.SegmentIntersection, Implicit.LinePlaneIntersection, Implicit.ThreePlaneIntersection>(
    T1Name = "Explicit", T2Name = "SegmentIntersection", T3Name = "LinePlaneIntersection", T4Name = "ThreePlaneIntersection")]
public readonly partial struct Implicit {
```

From, the three module-level records:

```csharp
public readonly record struct Ssi(Point3d P, Point3d Q, Point3d R, Point3d S, Axis Plane)
public readonly record struct Lpi(Point3d P, Point3d Q, Point3d A, Point3d B, Point3d C)
public readonly record struct Tpi(
    Point3d P1, Point3d P2, Point3d P3,
    Point3d Q1, Point3d Q2, Point3d Q3,
    Point3d R1, Point3d R2, Point3d R3)
```

To, nested inside `Implicit` after `Round`:

```csharp
public readonly record struct SegmentIntersection(
    Point3d FirstStart, Point3d FirstEnd, Point3d SecondStart, Point3d SecondEnd, Axis Projection)
public readonly record struct LinePlaneIntersection(
    Point3d LineStart, Point3d LineEnd, Point3d PlaneA, Point3d PlaneB, Point3d PlaneC)
public readonly record struct ThreePlaneIntersection(
    Point3d FirstA, Point3d FirstB, Point3d FirstC,
    Point3d SecondA, Point3d SecondB, Point3d SecondC,
    Point3d ThirdA, Point3d ThirdB, Point3d ThirdC)
```

Apply the corresponding parameter substitutions inside the three bodies. `SegmentIntersection` is the standard domain term for the admitted two-segment construction and removes the opaque SSI abbreviation; its four endpoint columns make the two inputs explicit without the stutter of `SegmentSegmentIntersection`. Both live creation paths first prove a segment crossing (`CrossSegments2D` or constrained-edge recovery). `LineIntersection` would erase that admission fact and remain ambiguous about arity and dimension.

Move the multi-construction helper out of the nested case while naming its exact matrix order:

```csharp
internal static T Det3<T>(...) where T : struct, IExact<T> => ...;
```

becomes, on `Predicate` beside `[HOMOGENEOUS_FOLDS]`:

```csharp
internal static T Determinant3x3<T>(...) where T : struct, IExact<T> => ...;
```

The nested `ThreePlaneIntersection.Homogeneous` calls `Predicate.Determinant3x3`; the general orient/in-sphere folds call `Determinant3x3` directly. `Det3` is an abbreviation, while `Determinant3` leaves the order ambiguous; `3x3` is the conventional matrix-size spelling already used by the checked-in C# surfaces. The helper is not owned by three-plane construction once three independent predicate folds consume it.

From, the union fold:

```csharp
ssi: static s => s.Homogeneous<T>(),
lpi: static l => l.Homogeneous<T>(),
tpi: static t => t.Homogeneous<T>());
```

To:

```csharp
segmentIntersection:    static point => point.Homogeneous<T>(),
linePlaneIntersection:  static point => point.Homogeneous<T>(),
threePlaneIntersection: static point => point.Homogeneous<T>());
```

Effect: module-level type symbols `-3`; total type count unchanged; opaque type names `-3`; one multi-use helper moves to its actual operation owner without changing member count; generated case probes/dispatch arms become descriptive. The value-carried ad-hoc union and its allocation profile do not change.

API/consumer proof: Thinktecture ad-hoc unions accept nested concrete member types and derive case/member names from the supplied `TnName`. The three payload shapes remain distinct: two projected lines, one line plus one plane, and three planes. No case adds admission or identity by remaining module-level.

Ripples: replace `Ssi`, `Lpi`, and `Tpi` with `Implicit.SegmentIntersection`, `Implicit.LinePlaneIntersection`, and `Implicit.ThreePlaneIntersection` in `Meshing/intersect.md` and `Meshing/delaunay.md`, including prose and diagrams. `Rasm/.api/api-bigrational.md` is deleted by move 12, so do not spend a transient rename there. No other `libs/dotnet/` file names those symbols. Keep every `Implicit` spelling in the target, Rasm owner docs, Rasm.Compute, Rasm.Fabrication, and kernel consumers.

## 2. Let the generated ad-hoc-union conversion absorb explicit points

Location: the 55 `new Implicit(Point3d)` wrappers in seven consumers.

From, representative calls:

```csharp
Predicate.Orient2D(new Implicit(a), new Implicit(b), new Implicit(probe), plane);
rows.Add(new Implicit(ring[v]));
return new Implicit(new Point3d(at[0], at[1], at[2]));
```

To:

```csharp
Predicate.Orient2D(a, b, probe, plane);
rows.Add(ring[v]);
return new Point3d(at[0], at[1], at[2]);
```

Where a projection must infer one result type, retain only the type-fixing cast:

```csharp
Arr<Implicit> rows = new([.. Enumerable.Range(0, n).Select(i => (Implicit)ring[i])]);
```

Effect: consumer fenced LOC is unchanged; explicit constructor wrappers `-55`; no overload or forwarding factory is added.

API/consumer proof: the checked-in Thinktecture doctrine states that an ad-hoc union's default implicit conversions make it a parameter absorber. Calls that omit an explicit `in` modifier may convert into the compiler-created readonly temporary; mixed collection/projection inference alone needs the explicit cast.

Ripples: `Meshing/intersect.md`, `Meshing/delaunay.md`, `Meshing/arrangement.md`, `Meshing/slice.md`, `Meshing/offset.md`, `Drawing/hatch.md`, and `Rasm.Fabrication/.planning/Geometry2D/algebra.md`. Exact search found no eighth constructor consumer.

## 3. Make the sign algebra primary instead of repeating its cases

Location: target `Sign.Of` overloads and `Sign.Flip`.

From:

```csharp
public static Sign Of(double value) => value < 0.0 ? Negative : value > 0.0 ? Positive : Zero;
public static Sign Of(int value) => value < 0 ? Negative : value > 0 ? Positive : Zero;

public Sign Flip => Switch(negative: static _ => Positive, zero: static _ => Zero, positive: static _ => Negative);
public Sign Times(Sign other) => Of(Key * other.Key);
```

To:

```csharp
public static Sign Of(double value) => value < 0.0 ? Negative : value > 0.0 ? Positive : Zero;

public Sign Flip => Times(Negative);
public Sign Times(Sign other) => Of(Key * other.Key);
```

Effect: public methods `2 -> 1`; sign classifiers `2 -> 1`; the three-arm flip dispatch is deleted and derived from the already-owned multiplication algebra.

API/consumer proof: every `int` passed to `Sign.Of` is already a normalized `-1/0/+1` sign (`CompareTo`, `ddouble.Sign`, or a generated key product), and those three integers convert to `double` exactly. Keeping `Of(double)` therefore absorbs every live call without widening the public method to arbitrary `INumber<T>` implementations. `Flip == Times(Negative)` for all three keyed rows, including zero.

Ripples: none.

## 4. Derive perpendicular axes through generated exhaustive dispatch

Location: target `Axis` rows, `NormalU`, `NormalV`, `U`, and `V`.

From:

```csharp
public static readonly Axis X = new(0, u: static () => Y, v: static () => Z, basis: Vector3d.XAxis, read: static p => p.X, along: static d => d.X);
public static readonly Axis Y = new(1, u: static () => Z, v: static () => X, basis: Vector3d.YAxis, read: static p => p.Y, along: static d => d.Y);
public static readonly Axis Z = new(2, u: static () => X, v: static () => Y, basis: Vector3d.ZAxis, read: static p => p.Z, along: static d => d.Z);

[UseDelegateFromConstructor] private partial Axis NormalU();
[UseDelegateFromConstructor] private partial Axis NormalV();
public Axis U => NormalU();
public Axis V => NormalV();
```

To:

```csharp
public static readonly Axis X = new(0, basis: Vector3d.XAxis, read: static p => p.X, along: static d => d.X);
public static readonly Axis Y = new(1, basis: Vector3d.YAxis, read: static p => p.Y, along: static d => d.Y);
public static readonly Axis Z = new(2, basis: Vector3d.ZAxis, read: static p => p.Z, along: static d => d.Z);

public Axis U => Switch(x: static _ => Y, y: static _ => Z, z: static _ => X);
public Axis V => Switch(x: static _ => Z, y: static _ => X, z: static _ => Y);
```

Effect: fenced LOC `7 -> 5` (`-2`); private members `-2`; generated delegate columns `-2`; constructor delegate arguments `-6`.

API/consumer proof: keyed smart enums generate exhaustive `Switch`. Property evaluation happens after the static rows initialize, preserving the deferred-reference reason for the original delegates; a new axis now breaks both projections at compile time.

Ripples: none.

## 5. Collapse dominance wrappers and the four-point Newell loop

Location: the three `Axis.DominantOf` overloads plus private `Dominant`/`Representable`.

### 5a. Inline the two one-use helpers

From:

```csharp
public static Fin<Axis> DominantOf(Vector3d d, Op? key = null) =>
    d.IsValid && !d.IsZero && Representable(d)
        ? Fin.Succ(Dominant(Math.Abs(d.X), Math.Abs(d.Y), Math.Abs(d.Z)))
        : Fin.Fail<Axis>(key.OrDefault().InvalidInput());

static Axis Dominant(double x, double y, double z) => x >= y && x >= z ? X : y >= z ? Y : Z;
```

To, after move 10 seats the ceiling on `Expansion`:

```csharp
public static Fin<Axis> DominantOf(Vector3d d, Op? key = null) {
    (double x, double y, double z) = (Math.Abs(d.X), Math.Abs(d.Y), Math.Abs(d.Z));
    return d.IsValid && !d.IsZero
        && x <= Expansion.SplitCeiling && y <= Expansion.SplitCeiling && z <= Expansion.SplitCeiling
            ? Fin.Succ(x >= y && x >= z ? X : y >= z ? Y : Z)
            : Fin.Fail<Axis>(key.OrDefault().InvalidInput());
}
```

Delete `Representable` with `Dominant`; its three comparisons are the conjunction above.

Effect: private members `-2`; one-hop calls `-2`; target fenced LOC approximately `-2` after formatting.

### 5b. Use the diagonal identity for four points

From:

```csharp
public static Fin<Axis> DominantOf(Point3d a, Point3d b, Point3d c, Point3d d, Op? key = null) {
    Span<Point3d> ring = [a, b, c, d];
    double nx = 0.0, ny = 0.0, nz = 0.0;
    for (int i = 0; i < 4; i++) {
        (Point3d p, Point3d q) = (ring[i], ring[(i + 1) & 3]);
        nx += (p.Y - q.Y) * (p.Z + q.Z);
        ny += (p.Z - q.Z) * (p.X + q.X);
        nz += (p.X - q.X) * (p.Y + q.Y);
    }
    return DominantOf(new Vector3d(nx, ny, nz), key);
}
```

To:

```csharp
public static Fin<Axis> DominantOf(Point3d a, Point3d b, Point3d c, Point3d d, Op? key = null) =>
    DominantOf(Vector3d.CrossProduct(c - a, d - b), key);
```

Effect: fenced LOC `11 -> 2` (`-9`); loop locals `-7`; stack span `-1`; loop branch `-1`.

API/consumer proof: for four ordered vertices, the Newell sum expands algebraically to `(c-a) × (d-b)`. Floating evaluation order changes, so this is not presented as bit identity; `DominantOf` is explicitly a projection heuristic, and any nonzero dominant component is lawful while exact topology remains on the predicate ladder. RhinoCommon owns the cross product.

Ripples: none.

## 6. Normalize stale consumers onto the existing `Axis` columns

Location: `Meshing/intersect.md`, `Meshing/delaunay.md`, `Meshing/arrangement.md`, `Meshing/slice.md`, and `Processing/repair.md`.

From:

```csharp
Axis.Coord(point, axis)
Axis.Coord(vector, axis.Key)
plane.AlongU
plane.AlongV
new(plane.Key == 0 ? 1.0 : 0.0, plane.Key == 1 ? 1.0 : 0.0, plane.Key == 2 ? 1.0 : 0.0)
(plane.Key == 0 ? normal.X : plane.Key == 1 ? normal.Y : normal.Z) < 0.0
```

To:

```csharp
axis.Read(point)
axis.Along(vector)
plane.U
plane.V
plane.Basis
plane.Along(normal) < 0.0
```

Effect: consumer fenced LOC unchanged; absent spellings `-35` (`Axis.Coord` 32, `AlongU`/`AlongV` 3); ordinal re-dispatches `-2`; compatibility members avoided `-5`.

API/consumer proof: `Read(Point3d)`, `Along(Vector3d)`, `U`, `V`, and `Basis` are the target's generated/declared behavior columns. The target exposes no `Coord`, `AlongU`, or `AlongV`; adding aliases would preserve stale callers by increasing the owner.

Ripples: apply only in the five files above. Locals storing `U`/`V` become `Axis`; `.Key` survives only at actual array indexing.

## 7. Localize one-use helpers and collapse the half-plane evaluation pair

Location: `Axis.BitKey`, `Implicit.Round`, `Implicit.ThreePlaneIntersection.Homogeneous`, `Halfplane.Side`/`Offset`, and their consumers after move 1.

### 7a. Localize signed-zero canonicalization

From:

```csharp
public static (long X, long Y, long Z) BitKey(Point3d p) => (Bits(p.X), Bits(p.Y), Bits(p.Z));
static long Bits(double v) => BitConverter.DoubleToInt64Bits(v == 0.0 ? 0.0 : v);
```

To:

```csharp
public static (long X, long Y, long Z) BitKey(Point3d p) {
    static long CanonicalBits(double value) => BitConverter.DoubleToInt64Bits(value == 0.0 ? 0.0 : value);
    return (CanonicalBits(p.X), CanonicalBits(p.Y), CanonicalBits(p.Z));
}
```

### 7b. Inline rounded materialization

From:

```csharp
public Option<Point3d> Round() =>
    IsExplicit ? Some(AsExplicit) : Materialized(Homogeneous<Expansion>());

static Option<Point3d> Materialized((Expansion X, Expansion Y, Expansion Z, Expansion Lambda) h) {
```

To:

```csharp
public Option<Point3d> Round() {
    if (IsExplicit) return Some(AsExplicit);
    (Expansion x, Expansion y, Expansion z, Expansion lambda) = Homogeneous<Expansion>();
    if (Expansion.SignOf(lambda) == Sign.Zero) return None;
    double scale = lambda.Estimate();
    Point3d rounded = new(x.Estimate() / scale, y.Estimate() / scale, z.Estimate() / scale);
    return rounded.IsValid ? Some(rounded) : None;
}
```

### 7c. Move `PlaneRow` into `ThreePlaneIntersection.Homogeneous<T>`

From:

```csharp
static ((T X, T Y, T Z) Normal, T Offset) PlaneRow<T>(Point3d a, Point3d b, Point3d c)
    where T : struct, IExact<T> {
```

To, a static local after the determinant return expression:

```csharp
static ((T X, T Y, T Z) Normal, T Offset) PlaneRow(Point3d a, Point3d b, Point3d c) {
```

The three calls drop `<T>`; the enclosing generic method supplies the carrier.

### 7d. Evaluate half-plane sign and offset through one dispatch

From:

```csharp
public Sign Side(Point3d p) => Switch(/* exact frame sign or affine sign */);
public double Offset(Point3d p) => Switch(/* frame Cross or affine offset */);
static double Cross(Point3d origin, Point3d along, Point3d q, Axis plane) { /* ... */ }
```

To:

```csharp
public (Sign Side, double Offset) Evaluate(Point3d point) =>
    Switch(
        state: point,
        frame: static (q, frame) => {
            (Axis u, Axis v) = (frame.Plane.U, frame.Plane.V);
            (double originU, double originV) = (u.Read(frame.Origin), v.Read(frame.Origin));
            (double alongU, double alongV) = (u.Read(frame.Along), v.Read(frame.Along));
            (double pointU, double pointV) = (u.Read(q), v.Read(q));
            Sign side = Predicate.Orient2D(originU, originV, alongU, alongV, pointU, pointV);
            double offset = ((originU - pointU) * (alongV - pointV)) - ((originV - pointV) * (alongU - pointU));
            return (side, offset);
        },
        affine: static (q, affine) => {
            double offset = (affine.Normal * (Vector3d)q) - affine.Constant;
            return (Sign.Of(offset), offset);
        });
```

`ClipHalfplane` deconstructs one `Evaluate` result per vertex instead of calling both members. `Meshing/delaunay.md` changes `cut.Side(at)` to `cut.Evaluate(at).Side` at its sole external read.

From, in `ClipHalfplane`:

```csharp
(Sign sidePrev, double offPrev, int labelPrev) = (cut.Side(prev), cut.Offset(prev), labels[ring.Length - 1]);
// ...
(Sign sideCur, double offCur) = (cut.Side(cur), cut.Offset(cur));
```

To:

```csharp
(Sign sidePrev, double offPrev) = cut.Evaluate(prev);
int labelPrev = labels[ring.Length - 1];
// ...
(Sign sideCur, double offCur) = cut.Evaluate(cur);
```

From, the only sign-only consumer in `Meshing/delaunay.md`:

```csharp
front.AsSpan(0, count), frontLabel.AsSpan(0, count), cut, cut.Side(at),
```

To:

```csharp
front.AsSpan(0, count), frontLabel.AsSpan(0, count), cut, cut.Evaluate(at).Side,
```

Effect: type-level members `-5` across move 7 (`Bits`, `Materialized`, `PlaneRow`, `Side`/`Offset`/`Cross -> Evaluate`); one generated union dispatch per clipped vertex removed; target fenced LOC approximately `-4`.

API/consumer proof: exact search finds one owning body for `Bits`, `Materialized`, and `PlaneRow`, while every `Halfplane` consumer that needs an offset also needs its sign. The frame arm reads the six projected coordinates once and still passes those original ordinates to `Orient2D`; only the emission offset subtracts them in `double`, so the exact predicate never consumes the rounded differences. The affine arm computes its shared scalar once.

Ripples: the one `Meshing/delaunay.md` keep-side read above; no compatibility `Side`/`Offset` wrappers.

## 8. Name the midpoint fallback as the exact evidence it carries

Location: target `ClipHalfplane`; `Meshing/delaunay.md`; `Meshing/mesh.md` `PowerFacet`/`PowerCell`.

From:

```csharp
Fin<(int Written, int Fabricated)>
Span<bool> targetFabricated
int written = 0, fabricated = 0;
bool forged = Math.Abs(denom) < denomFloor;
double t = forged ? 0.5 : offPrev / denom;
```

To:

```csharp
Fin<(int Written, int MidpointFallbacks)>
Span<bool> targetMidpointFallback
int written = 0, midpointFallbacks = 0;
bool midpointFallback = Math.Abs(denom) < denomFloor;
double t = midpointFallback ? 0.5 : offPrev / denom;
```

Apply the same substitutions to the written mark, count increment, and return tuple.

Effect: LOC and symbol count unchanged; coined evidence names `-4`; the boolean and count state the exact event the code records.

API/consumer proof: the fallback chooses `t = 0.5`; it neither fabricates topology nor describes every possible approximation. `MidpointFallback` names the actual branch, while `Meshing/delaunay.md` refuses a positive count and `Meshing/mesh.md` carries the per-row contamination evidence.

Ripples: rename the destructured count/working span in `Meshing/delaunay.md`; rename only `PowerCell.Fabricated` and `PowerFacet.Fabricated` to `MidpointFallback` plus their reads and owning prose in `Meshing/mesh.md`. Do not touch unrelated material/fabrication vocabulary.

## 9. Make the interval internal and nest a non-record filter row on its only owner

Location: target `Interval` and `ErrorBound` declarations.

From:

```csharp
public readonly struct Interval : IExact<Interval> {

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ErrorBound {
    public static readonly ErrorBound Orient2D = new("orient-2d", alpha: 3.0, beta: 16.0);
```

To, with `ErrorBound` moved inside `Predicate` before the direct predicate members:

```csharp
internal readonly struct Interval : IExact<Interval> {

public static class Predicate {
    private readonly struct ErrorBound(double alpha, double beta) {
        private static readonly ErrorBound Orient2D = new(3.0, 16.0);
        private static readonly ErrorBound Orient3D = new(7.0, 56.0);
        private static readonly ErrorBound InCircle = new(10.0, 96.0);
        private static readonly ErrorBound InSphere = new(16.0, 224.0);

        private double Bound(double roundoff) => (alpha + (beta * roundoff)) * roundoff;
        private Sign? Of(double det, double permanent) =>
            Math.Abs(det) > Bound(NumericsPolicy.Epsilon) * permanent ? Sign.Of(det) : null;
        private Sign? Refine(ddouble det, ddouble permanent) =>
            ddouble.Abs(det) > Bound(NumericsPolicy.DoubleDoubleEpsilon) * permanent ? Sign.Of(ddouble.Sign(det)) : null;
    }
```

Delete the constructor keys and `Alpha`/`Beta` properties. C# grants the containing `Predicate` access to its nested type's private members, so no `internal` modifier is needed anywhere on this private coefficient row. Move 10 then rewrites the two `NumericsPolicy` reads shown here to the constants seated on this struct.

Effect: public interior types `-2`; module-level types `-1`; authored row properties `-2`; string keys `-4`; one generated smart-enum family deleted; the four static rows become allocation-free values rather than singleton class instances.

API/consumer proof: no `libs/dotnet/` consumer reads an `ErrorBound` key, roster, lookup, conversion, comparison, or exhaustive dispatch; every use is inside `Predicate`, addresses one static row directly, and calls its coefficient behavior. A keyless smart enum would still generate an unused roster and dispatch plane. A `record struct` is also rejected: it would synthesize equality, hashing, deconstruction, and rendering for a private coefficient row no caller compares, deconstructs, or renders. A readonly primary-constructor struct captures exactly the two coefficients and the three behaviors the filter uses. `Interval` is instantiated only inside this target.

Ripples: remove `ComparerAccessors`, smart-enum, key, roster, and string-key claims from target prose. After move 12, remove `using Thinktecture;` from the second fence as well; no generated owner remains there. No code consumer changes.

## 10. Dissolve the generic `NumericsPolicy` shell into the arithmetic owners

Location: target `NumericsPolicy`, `Axis.DominantOf`, `Expansion`, and `ErrorBound`.

From:

```csharp
public static class NumericsPolicy {
    public const double Epsilon = 1.0 / (1L << 53);
    public const double DoubleDoubleEpsilon = Epsilon * Epsilon * 0.5;
    public const double Splitter = (1 << 27) + 1;
    public const double SplitCeiling = double.MaxValue / Splitter;
    public static readonly bool HardwareFma =
        System.Runtime.Intrinsics.X86.Fma.IsSupported || System.Runtime.Intrinsics.Arm.AdvSimd.Arm64.IsSupported;
}
```

To, on `Expansion`:

```csharp
internal const double SplitCeiling = double.MaxValue / Splitter;
const double Splitter = (1 << 27) + 1;
```

To, on `ErrorBound`:

```csharp
const double Epsilon = 1.0 / (1L << 53);
const double DoubleDoubleEpsilon = Epsilon * Epsilon * 0.5;
```

Replace the one hardware-field read at the product kernel with the intrinsic capability expression directly, replace the three admission reads with `Expansion.SplitCeiling`, and replace the two error-filter reads with the local constants.

Effect: module-level types `-1`; public policy members `-5`; cached one-use field `-1`; target fenced LOC approximately `-4`.

API/consumer proof: exact search finds no `NumericsPolicy` consumer outside the target. Splitter/ceiling govern expansion product exactness; roundoff constants govern filter coefficients. Both `IsSupported` properties are JIT constants, so reading them at the one product site preserves tier elimination without a forwarding field.

Ripples: rewrite the target owner/growth/boundary prose around `Expansion` and `ErrorBound`; remove `NumericsPolicy` from the index. No other file changes.

## 11. Localize arithmetic helpers that have one owning member

Location: target `Expansion.Diff`/`TwoSum`/`Pair`, `Difference`/`Negate`, `Scale`/`TwoProductCore`/`Split`, and `Interval.Mul`/`Least`/`Greatest`/`Min`/`Max`.

### 11a. Collapse the one-use `TwoSum` entry and its one-use pair constructor into `Diff`

From:

```csharp
public static Expansion TwoSum(double a, double b) {
    (double hi, double lo) = TwoSumCore(a, b);
    return Pair(lo, hi);
}

internal static Expansion Diff(double a, double b) => TwoSum(a, -b);

static Expansion Pair(double small, double large) =>
    small == 0.0 ? Single(large) : new Expansion(new[] { small, large }, 2);
```

To:

```csharp
internal static Expansion Diff(double a, double b) {
    (double hi, double lo) = TwoSumCore(a, -b);
    return lo == 0.0 ? Single(hi) : new Expansion([lo, hi], 2);
}
```

### 11b. Inline negation into its sole `Difference` consumer

From:

```csharp
public static Expansion Difference(Expansion left, Expansion right) => Sum(left, Negate(right));

public static Expansion Negate(Expansion e) {
    double[] flipped = new double[e.length];
    for (int i = 0; i < e.length; i++) flipped[i] = -e.components[i];
    return new Expansion(flipped, e.length);
}
```

To:

```csharp
public static Expansion Difference(Expansion left, Expansion right) {
    if (right.length == 0) return left;
    double[] flipped = new double[right.length];
    for (int i = 0; i < right.length; i++) flipped[i] = -right.components[i];
    return Sum(left, new Expansion(flipped, right.length));
}
```

### 11c. Move product splitting into `Scale`

Move `TwoProductCore` as a static local named `TwoProduct` inside `Scale`; move `Split` inside that local. The body remains operation-for-operation identical except that the FMA branch reads the final move-10 capability expression and `Split` reads `Expansion.Splitter` directly.

### 11d. Move interval extrema into `Mul`

Move `Least`, `Greatest`, `Min`, and `Max` as static locals after `Mul`'s return. Keep the four directed endpoint products and `CompareTo` reductions byte-for-byte; PeterO exposes no `EFloat.Min`/`Max` or relational operators.

Effect: type members `-9` (`TwoSum`, `Pair`, `Negate`, `TwoProductCore`, `Split`, `Least`, `Greatest`, `Min`, `Max`); public methods `-2`; target fenced LOC approximately `-2`.

API/consumer proof: each deleted helper has exactly one owning member, though some are called repeatedly inside that body. Localization preserves reuse and exact evaluation order while removing module/type surface. `TwoSumCore` remains type-level because `Diff`, `Sum`, and `Scale` independently consume it.

Ripples: remove `Negate` from any target prose; exact search finds no external code call.

## 12. Delete the redundant rational re-signing tier and its dead test hooks

Location: implicit `InCircle`/`InSphere` exact tails; `RationalOracle`; `Expansion.ToFraction`/`Components`/`StackExponents`; the first- and second-fence imports.

### 12a. Read the sign-exact expansion once

From:

```csharp
(Expansion Det, Expansion Lambda) e = InCircleNumerator<Expansion>(a, b, c, in d, axis);
return RationalOracle.InCircum(e.Det, e.Lambda, lambdaDegree: 4);

(Expansion Det, Expansion Lambda) x = InSphereNumerator<Expansion>(a, b, c, d, in e);
return RationalOracle.InCircum(x.Det, x.Lambda, lambdaDegree: 5);
```

To:

```csharp
(Expansion Det, Expansion Lambda) exact = InCircleNumerator<Expansion>(a, b, c, in d, axis);
Sign lambda = Expansion.SignOf(exact.Lambda);
return Expansion.SignOf(exact.Det).Times(lambda).Times(lambda);

(Expansion Det, Expansion Lambda) exact = InSphereNumerator<Expansion>(a, b, c, d, in e);
return Expansion.SignOf(exact.Det).Times(Expansion.SignOf(exact.Lambda));
```

The repeated in-circle lambda factor deliberately preserves the even-degree zero gate; the in-sphere factor preserves the odd degree. No polynomial or degeneracy decision changes.

### 12b. Delete the non-independent oracle surface

From:

```csharp
public static class RationalOracle {
    public static Sign InCircum(Expansion det, Expansion lambda, int lambdaDegree) { /* Fraction signs */ }
    internal static Sign RationalOf(Expansion det, Expansion lambda, int lambdaDegree) { /* ERational sum */ }
    public static Sign? BinaryOf(Expansion e) { /* EFloat sum */ }
}
```

To: delete the class whole.

Also delete `Expansion.ToFraction`, `Expansion.Components`, and `StackExponents`; all three exist only to feed that class. Remove `using ExtendedNumerics;` and `using System.Numerics;` from both fences plus `using CommunityToolkit.HighPerformance.Buffers;` from the second fence. Move 3 deliberately retains the concrete `double` classifier, so neither fence needs generic math after the BigInteger lift disappears.

Effect: module-level types `-1`; declared type members `-6`; target fenced LOC approximately `-50`; imports `-5`; one shipping `BigInteger` allocation tier and three unconsumed test hooks disappear.

API/consumer proof: `Expansion.SignOf` is declared to return the exact sign of the nonoverlapping expansion and `IExact<Expansion>.Verdict` always delegates to it. `InCircum` does not recompute the determinant from original ordinates with `Fraction`; it converts the already-produced `det` and `lambda` expansions and reads their signs again. `RationalOf` and `BinaryOf` likewise sum those same expansion components, so they are representation checks of the terminal sum, not independent predicate oracles. Exact repository search finds no consumer of any of the three methods or of `Components` outside this target. Replacing the manual lift with repeated `new Fraction(component)` would be shorter but weaker: it would allocate and normalize a rational per component while certifying no fact that `SignOf` has not already decided.

Prose correction, target lead.

From:

```text
Every fold is one polynomial instantiated at both the Interval filter and Expansion exact carriers through the IExact<TSelf> algebra.
```

To:

```text
Every constructed-point fold is one polynomial instantiated at both the Interval filter and Expansion exact carriers through IExact<TSelf>; direct predicates retain precision-specific double, ddouble, and Expansion kernels.
```

The original universal claim excludes the four direct predicates' visibly separate kernels. This correction states the actual reuse boundary and prevents the removed rational conversion from being described as a third polynomial tier.

Ripples: remove `ExtendedNumerics.BigRational` from `Rasm/Rasm.csproj`, `Rasm/README.md`, and root `Directory.Packages.props`; delete the now-unowned `Rasm/.api/api-bigrational.md`; correct `Rasm/.api/api-doubledouble.md` and substrate `api-petero-numbers.md` so the runtime ladder ends at `Expansion`, PeterO owns the interval filter only, and no four-way rational differential is claimed. Regenerate every lockfile whose dependency graph currently carries the removed Rasm edge: `Rasm`, `Rasm.AppHost`, `Rasm.AppUi`, `Rasm.Bim`, `Rasm.Compute`, `Rasm.Element`, `Rasm.Fabrication`, `Rasm.Grasshopper`, `Rasm.Materials`, `Rasm.Persistence`, and `Rasm.Rhino` (`11` `packages.lock.json` files total). Remove `RationalOracle`, `Fraction`, `ERational`, `BigInteger`, and exact-rational-tier claims from both target cards. Keep `CommunityToolkit.HighPerformance` in the project/package registries because other Rasm pages consume it; only this fence import disappears.

## 13. Make the complete exact-arithmetic substrate internal

Location: `IExact<TSelf>`, `Expansion`, `Interval`, `Implicit.Homogeneous<T>`, and the three nested construction `Homogeneous<T>` members after moves 1 and 12.

From:

```csharp
public interface IExact<TSelf> where TSelf : struct, IExact<TSelf>
public readonly struct Expansion : IExact<Expansion>
public readonly struct Interval : IExact<Interval>
public (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T>
public static Expansion Single(double value)
public static Expansion Sum(Expansion left, Expansion right)
public readonly EFloat Lo;
public readonly EFloat Hi;
```

To:

```csharp
internal interface IExact<TSelf> where TSelf : struct, IExact<TSelf>
internal readonly struct Expansion : IExact<Expansion>
internal readonly struct Interval : IExact<Interval>
internal (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T>
private static Expansion Single(double value)
internal static Expansion Sum(Expansion left, Expansion right)
private readonly EFloat Lo;
private readonly EFloat Hi;
```

Apply the `internal` substitution to all four `Homogeneous<T>` declarations: the union dispatcher plus `SegmentIntersection`, `LinePlaneIntersection`, and `ThreePlaneIntersection`.

Apply the same accessibility reduction to the rest of `Expansion`: `Difference`, `Multiply`, `Estimate`, and `SignOf` become `internal`; `Scale` becomes `private`; `Diff` is already `internal`. The interface implementations remain explicit. Keep `Interval`'s arithmetic members public because they are its direct implicit implementation of `IExact<Interval>`; the carrier type itself is internal, so those members cannot cross the assembly, while converting them to explicit implementations would add forwarding bodies merely to preserve the target's concrete `f.N.Verdict` reads. Its endpoint fields are private implementation state.

Effect: public types `-2` beyond move 9's already-internal `Interval`; public generic arithmetic entries `-4`; public carrier members `-9` (seven `Expansion` operations plus two interval endpoints); no runtime change.

API/consumer proof: exact `libs/dotnet/` search finds no consumer of `IExact`, `Expansion`, or any `Homogeneous` member outside this target. Once move 12 deletes the asserted test hooks, no friend-assembly exception remains. The page's own law already says interior arithmetic crosses no public signature; this move makes the fence honor that law instead of exposing its implementation algebra and carrier.

Ripples: update the target boundary prose to state that only `Sign`, `Axis`, `Implicit`, `Halfplane`, and `Predicate` cross the module boundary. No consumer edit.

## 14. Canonicalize zero expansions, size sums to their proven bound, and narrow the allocation claim

Location: target `Expansion.Single` and `Sum`, after move 11 deletes `Pair`/`Negate`.

From:

```csharp
private static Expansion Single(double value) => new(new[] { value }, value == 0.0 ? 0 : 1);

internal static Expansion Sum(Expansion left, Expansion right) {
    double[] merged = new double[left.length + right.length + 1];
    // ... merge loop ...
    if (carry != 0.0 || written == 0) merged[written++] = carry;
    return new Expansion(merged, written);
```

To:

```csharp
private static Expansion Single(double value) => value == 0.0 ? default : new([value], 1);

internal static Expansion Sum(Expansion left, Expansion right) {
    if (left.length == 0) return right;
    if (right.length == 0) return left;
    double[] merged = new double[left.length + right.length];
    // ... unchanged merge loop ...
    if (carry != 0.0) merged[written++] = carry;
    return written == 0 ? default : new Expansion(merged, written);
```

Effect: zero singleton arrays `1 -> 0`; sums with an already-empty operand allocate `0` arrays; an exact cancellation returns the canonical default zero; nonempty sum capacity `m+n+1 -> m+n`; target fenced LOC `+2`; no symbols added.

API/consumer proof: `default(Expansion)` already has `length == 0`; every indexing member checks length before touching `components`, and move 12 deletes the only property that exposed the backing span. The first `TwoSumCore(0, next)` cannot emit a low word, so merging `m+n` stored components emits at most `m+n-1` lows plus one carry: `m+n` is the exact worst-case capacity and the old extra slot was unreachable. The early returns prevent empty-plus-value from allocating merely to reproduce an existing value. A nonempty sum that cancels still allocates its required work buffer, but it returns `default`, so later zero arithmetic takes those early exits; the audit does not claim the cancellation itself is allocation-free.

Prose correction, target `[02]-[ROBUST_PREDICATES]` `Auto`.

From:

```text
Every member walks its tiers inline as one ??-chain over the uniform Sign?-or-escalate protocol, allocation-free with no captured thunk.
```

To:

```text
Direct members walk their tiers inline as one ??-chain with no captured thunk; implicit members branch from the allocating EFloat interval filter to expansion arrays only when the bracket is indeterminate.
```

The former sentence is doubly false: implicit members use explicit branches rather than `??`, and both `EFloat` interval operations and nonzero expansion operations allocate. The optimized fact is dispatch shape, not whole-ladder allocation freedom.

Ripples: none.

## Protected non-moves

- Keep `Implicit` as the owner name. It is settled computational-geometry terminology and a Rasm ruling/architecture anchor; `PredicatePoint` or `ImplicitPoint` would add context without reducing surface.
- Keep the ad-hoc readonly-struct union. Explicit, segment-intersection, line-plane, and three-plane payloads are genuinely different, and a regular class union would allocate on the hot predicate path.
- Keep `Sign` and `Axis` keyed. `Sign.Key` is the parity algebra; `Axis.Key` is the coordinate-array ordinal at the final indexing boundary.
- Keep `IExact<TSelf>` as an internal algebra. It is the one static-abstract contract proving that interval and expansion carriers instantiate the same constructed-point polynomial; duplicating those folds to eliminate one internal interface would add logic.
- Delete `RationalOracle` rather than rename or relocate it. Converting an already exact expansion into `Fraction`, `ERational`, or `EFloat` and summing the same components is not independent adjudication; a future differential must recompute from original inputs in the test assembly.
- Keep `Expansion` internal after deleting its dead differential hooks. No consumer is entitled to its backing components, and the public verdict surface is `Predicate -> Sign`.
- Keep `ErrorBound` as one private nested immutable coefficient row. Inlining `(alpha,beta)` into both precision tiers duplicates the published coefficients eight times; a record or generated smart-enum adds machinery no reader uses.
- Keep `Interval`'s interface members implicit on the internal carrier. Explicit implementations would not shrink the reachable surface and would add adapters for the concrete filter-verdict reads; only `Lo`/`Hi` need private storage accessibility.
- Keep direct `double`, `ddouble`, and expansion determinant bodies separate. They are precision-specific kernels with distinct operators and allocation behavior, not superficial copies.
- Keep `Predicate.Determinant3x3`, `Lift2`, `Lift3`, `Minor3`, `Swizzled`, and `TwoSumCore`. Each has multiple independent code calls; localizing or duplicating them would worsen ownership.
- Keep imperative span/expansion loops. They are the measured numeric-kernel exemption; forcing them through LanguageExt would allocate or erase evaluation order without shrinking the owner.
- Keep `Halfplane` as a two-case regular union. `Frame` uses exact orientation while `Affine` accepts an already-owned functional; a nullable bag or delegate erases that semantic difference.
- Keep `ClipHalfplane`'s `ring.Length + 2` target-room bound. Although a strict half-plane clip of a convex ring adds at most one net vertex, the declared `band` deliberately retains an outside near-boundary vertex while both adjacent crossings may also be emitted, so `+2` is reachable.
- Do not replace `Expansion.Estimate` with SIMD/tensor reduction. Its deterministic component order is part of emission behavior.
- Do not change predicate degeneracy, error coefficients, lambda parity, interval rounding contexts, nonzero expansion operation order, or any direct/implicit polynomial. Move 12 removes only a second sign read over the same terminal expansion; move 14 canonicalizes the already-exact zero identity.

## Net effect

Applying all fourteen moves yields approximately 70-80 fewer target fence lines, six fewer module-level types (three construction payloads and `ErrorBound` nested, `NumericsPolicy` and `RationalOracle` deleted), two fewer total types, at least twenty-eight fewer declared type-level members, six fewer public types, nine additional public carrier members narrowed, four public `Homogeneous` entries internalized, two fewer generated delegate columns, one generated smart-enum family deleted, four fewer string keys, and six target imports removed. Consumer ripples delete exactly 55 `new Implicit(Point3d)` wrappers across seven files and exactly 35 stale `Axis` spellings (32 `Axis.Coord` calls plus three code-level `AlongU`/`AlongV` reads) without adding a compatibility surface. The obsolete BigRational package touch-point set disappears whole: one central version row, one `Rasm.csproj` edge, one `Rasm/README.md` registry row, one package API catalogue, the two remaining predicate-ladder catalogue claims, and exactly eleven affected `packages.lock.json` files (`Rasm` plus its ten transitive workspace consumers). The direct ladder remains `double -> ddouble -> Expansion`, the implicit ladder `Interval -> Expansion`, and every exact polynomial, lambda-parity gate, and degeneracy decision remains unchanged.
