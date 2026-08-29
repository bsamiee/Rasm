# 1. Remove the unused verdict from the carrier algebra

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:616`
```csharp
Sign? Verdict { get; }
```

To

```csharp
// IExact<TSelf>.Verdict DELETED
```

Why

No generic polynomial reads this member. Only the concrete interval filter has an indeterminate sign; forcing the exact carrier to implement nullable escalation adds a second unused protocol.

Change

Delete `Verdict` from `IExact<TSelf>` and delete `IExact<Expansion>.Verdict`; keep the interval verdict on `Interval` itself.

Delta

-2 LOC; -2 module-level members; 0 types.

# 2. Represent interval indeterminacy with Option

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:767-771`
```csharp
public Sign? Verdict =>
    Lo.Sign > 0 ? Sign.Positive
    : Hi.Sign < 0 ? Sign.Negative
    : Lo.IsZero && Hi.IsZero ? Sign.Zero
    : null;
```

To

```csharp
public Option<Sign> Verdict =>
    Lo.Sign > 0 ? Some(Sign.Positive)
    : Hi.Sign < 0 ? Some(Sign.Negative)
    : Lo.IsZero && Hi.IsZero ? Some(Sign.Zero)
    : None;
```

Why

An interval containing zero has ordinary typed absence, not a null result. `Option<Sign>` gives the filter one explicit escalation shape and composes with the module's carrier vocabulary.

Change

Import `LanguageExt` and `LanguageExt.Prelude` in the interior-numerics fence, return `Option<Sign>`, and replace the five nullable verdict probes with `Verdict.Case is Sign sign` probes.

Delta

+2 LOC; 0 members; 0 types; five nullable escalation paths removed.

# 3. Collapse duplicate direct error filters

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:188-201`
```csharp
const double Epsilon = 1.0 / (1L << 53);
const double DoubleDoubleEpsilon = Epsilon * Epsilon * 0.5;

private double Bound(double roundoff) => (alpha + (beta * roundoff)) * roundoff;
public Sign? Of(double det, double permanent) =>
    Math.Abs(det) > Bound(Epsilon) * permanent ? Sign.Of(det) : null;
public Sign? Refine(ddouble det, ddouble permanent) =>
    ddouble.Abs(det) > Bound(DoubleDoubleEpsilon) * permanent ? Sign.Of(ddouble.Sign(det)) : null;
```

To

```csharp
const double DoubleRoundoff = 1.0 / (1L << 53);
const double DoubleDoubleRoundoff = DoubleRoundoff * DoubleRoundoff;

public Option<Sign> Filter<T>(T determinant, T permanent, T roundoff) where T : struct, INumber<T> =>
    T.Abs(determinant) > (T.CreateChecked(alpha) + T.CreateChecked(beta) * roundoff) * roundoff * permanent
        ? Some(Sign.Of(T.Sign(determinant)))
        : None;
```

Why

`double` and `ddouble` both implement `INumber<T>`, so three members repeat one formula. The catalogued 106-bit significand has unit roundoff `2^-106`; another factor of one half understates uncertainty and can accept an unresolved refinement.

Change

Import `System.Numerics`, replace `Bound`, `Of`, and `Refine` with `Filter<T>`, use the typed roundoff at each call, return `Option<Sign>` from the four refinement kernels, and retain lazy escalation with `IfNone(Func<Sign>)`.

Delta

-1 LOC; -2 module-level members; 0 types.

# 4. Replace the expansion engine with exact dyadic arithmetic

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:620-629`
```csharp
internal readonly struct Expansion : IExact<Expansion> {
    internal const double SplitCeiling = double.MaxValue / Splitter;
    const double Splitter = (1 << 27) + 1;

    private readonly double[] components;
    private readonly int length;

    private Expansion(double[] components, int length) { this.components = components; this.length = length; }

    private static Expansion Single(double value) => value == 0.0 ? default : new([value], 1);
```

To

```csharp
internal readonly struct Dyadic : IExact<Dyadic> {
    readonly EFloat? value;
    EFloat Value => value ?? EFloat.Zero;

    Dyadic(EFloat value) => this.value = value;
    public static Dyadic Of(double value) => new(EFloat.FromDouble(value));
    public static Dyadic Diff(double a, double b) => new(EFloat.FromDouble(a).Subtract(EFloat.FromDouble(b)));
    public Dyadic Add(Dyadic other) => new(Value.Add(other.Value));
    public Dyadic Sub(Dyadic other) => new(Value.Subtract(other.Value));
    public Dyadic Mul(Dyadic other) => new(Value.Multiply(other.Value));
    public Dyadic Scale(double exact) => Mul(Of(exact));
    internal Sign Sign => Sign.Of(Value.Sign);
    internal double Quotient(Dyadic denominator) => Value.Divide(denominator.Value, EContext.Binary64).ToDouble();
}
```

Why

`EFloat.FromDouble` is lossless and context-free arithmetic is exact. Arrays, error-free transforms, FMA dispatch, splitting, scaling, merging, sign scan, and estimate duplicate the admitted PeterO arithmetic; the small carrier remains only to supply `IExact<TSelf>` conformance and a valid default zero.

Change

Replace the entire `Expansion` owner with `Dyadic`, instantiate terminal homogeneous folds with it, translate the four direct exact kernels to the same carrier operations, and replace `Expansion.SignOf(value)` with `value.Sign` without retaining a parallel expansion path.

Delta

-82 LOC; -9 module-level members; 0 net types.

# 5. Round homogeneous coordinates after exact division

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:95-101`
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

To

```csharp
public Option<Point3d> Round() {
    if (IsExplicit) return AsExplicit.IsValid ? Some(AsExplicit) : None;
    (Dyadic x, Dyadic y, Dyadic z, Dyadic lambda) = Homogeneous<Dyadic>();
    if (lambda.Sign == Sign.Zero) return None;
    Point3d rounded = new(x.Quotient(lambda), y.Quotient(lambda), z.Quotient(lambda));
    return rounded.IsValid ? Some(rounded) : None;
}
```

Why

Estimating numerator and denominator separately can overflow or underflow although their quotient is representable. `EContext.Binary64` rounds the exact quotient once, and the explicit union arm must pass the same host-validity gate.

Change

Use `Dyadic.Quotient` for constructed coordinates and admit the explicit arm through `Point3d.IsValid` before returning it.

Delta

0 LOC; 0 members; 0 types; one double-rounding path and one unchecked egress removed.

# 6. Remove the obsolete splitter ceiling from axis admission

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:70-75`
```csharp
public static Fin<Axis> DominantOf(Vector3d d) {
    (double x, double y, double z) = (Math.Abs(d.X), Math.Abs(d.Y), Math.Abs(d.Z));
    return d.IsValid && !d.IsZero
        && x <= Expansion.SplitCeiling && y <= Expansion.SplitCeiling && z <= Expansion.SplitCeiling
            ? Fin.Succ(x >= y && x >= z ? X : y >= z ? Y : Z)
            : Fin.Fail<Axis>(new KernelFault.InvalidInput());
}
```

To

```csharp
public static Fin<Axis> DominantOf(Vector3d d) {
    (double x, double y, double z) = (Math.Abs(d.X), Math.Abs(d.Y), Math.Abs(d.Z));
    return d.IsValid && !d.IsZero
        ? Fin.Succ(x >= y && x >= z ? X : y >= z ? Y : Z)
        : Fin.Fail<Axis>(new KernelFault.InvalidInput());
}
```

Why

The magnitude gate exists only for Dekker splitting. Exact `EFloat` arithmetic has an unbounded exponent, so the gate rejects valid host vectors for a deleted implementation constraint.

Change

Keep `Vector3d.IsValid` and `IsZero` admission and remove every reference to `SplitCeiling`.

Delta

-1 LOC; 0 members; 0 types.

# 7. Delete point-based dominant-axis forwarding overloads

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:78-82`
```csharp
public static Fin<Axis> DominantOf(Point3d a, Point3d b, Point3d c) =>
    DominantOf(Vector3d.CrossProduct(b - a, c - a));

public static Fin<Axis> DominantOf(Point3d a, Point3d b, Point3d c, Point3d d) =>
    DominantOf(Vector3d.CrossProduct(c - a, d - b));
```

To

```csharp
// DominantOf(Point3d...) DELETED
```

Why

Both overloads form one vector and forward to the actual admission. The four-point overload also hides a non-canonical diagonal construction that only the quad caller can name correctly.

Change

Delete both overloads and pass the intended `Vector3d.CrossProduct` directly at each caller; replace the stray four-argument repair call with `Vector3d.CrossProduct(pb - pa, pc - pa)`.

Delta

-5 LOC; -2 public members; 0 types.

Ripples

Update `libs/dotnet/Rasm/.planning/Meshing/arrangement.md:293`, `Meshing/intersect.md:215,245,252,270,372,425`, `Meshing/mesh.md:628`, and `Processing/repair.md:193,333`; vector-based callers remain unchanged.

# 8. Localize explicit-point bit identity to the crossing arena

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:65-68`
```csharp
public static (long X, long Y, long Z) BitKey(Point3d p) {
    static long CanonicalBits(double value) => BitConverter.DoubleToInt64Bits(value == 0.0 ? 0.0 : value);
    return (CanonicalBits(p.X), CanonicalBits(p.Y), CanonicalBits(p.Z));
}
```

To

```csharp
// Axis.BitKey DELETED
```

Why

Bitwise interning is not axis behavior and only `CrossingStore` consumes it. Publishing one arena's identity policy on `Axis` creates a false semantic hop and widens the numeric surface.

Change

Delete `Axis.BitKey` and move the signed-zero-canonicalizing body into a static local function inside `CrossingStore.Intern`.

Delta

0 project LOC; -1 public member; 0 types.

Ripples

Replace both calls at `libs/dotnet/Rasm/.planning/Meshing/intersect.md:156,159` and remove `BitKey` from that page's owner, package, and boundary prose.

# 9. Stop publishing scalar precision kernels

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:207,220,240,260`
```csharp
public static Sign Orient2D(double ax, double ay, double bx, double by, double cx, double cy) {
public static Sign Orient3D(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz) {
public static Sign InCircle(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy) {
public static Sign InSphere(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz, double ex, double ey, double ez) {
```

To

```csharp
private static Sign Orient2D(double ax, double ay, double bx, double by, double cx, double cy) {
private static Sign Orient3D(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz) {
private static Sign InCircle(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy) {
private static Sign InSphere(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz, double ex, double ey, double ez) {
```

Why

Every repository consumer enters through `Point3d` or the implicit-point union. The scalar arities are precision-kernel details that expose a second public admission path with no consumer.

Change

Make the four scalar kernels private and keep the host-value and implicit-point entries public.

Delta

0 LOC; -4 public members; 0 total members; 0 types.

# 10. Make affine half-plane side tests exact

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:179-182`
```csharp
affine: static (q, affine) => {
    double offset = (affine.Normal * (Vector3d)q) - affine.Constant;
    return (Sign.Of(offset), offset);
});
```

To

```csharp
affine: static (q, affine) => {
    double offset = (affine.Normal * (Vector3d)q) - affine.Constant;
    Dyadic exact = Dyadic.Of(affine.Normal.X).Mul(Dyadic.Of(q.X))
        .Add(Dyadic.Of(affine.Normal.Y).Mul(Dyadic.Of(q.Y)))
        .Add(Dyadic.Of(affine.Normal.Z).Mul(Dyadic.Of(q.Z)))
        .Sub(Dyadic.Of(affine.Constant));
    return (exact.Sign, offset);
});
```

Why

The affine case decides topology from a rounded dot product while the directed-line case uses exact orientation. The approximate offset is needed only for interpolation and must not become a second side oracle.

Change

Compute `Side` with `Dyadic` and retain the `double` offset only for the emission-side segment parameter.

Delta

+3 LOC; 0 members; 0 types; one inexact topology verdict removed.

# 11. Internalize half-plane evaluation into clipping

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:45,167,351-353,359,380`
```csharp
public Sign Flip => Times(Negative);
public (Sign Side, double Offset) Evaluate(Point3d point) =>
public static Fin<(int Written, int MidpointFallbacks)> ClipHalfplane(
    ReadOnlySpan<Point3d> ring, ReadOnlySpan<int> labels, Halfplane cut, Sign keep, double band, double denomFloor,
    int cutLabel, Span<Point3d> target, Span<int> targetLabels, Span<bool> targetMidpointFallback) {
int written = 0, midpointFallbacks = 0;
return Fin.Succ((written, midpointFallbacks));
```

To

```csharp
public Fin<int> Clip(
    ReadOnlySpan<Point3d> ring, ReadOnlySpan<int> labels, Point3d interior, double band, double denominatorFloor,
    int cutLabel, Span<Point3d> target, Span<int> targetLabels, Span<bool> targetMidpointFallback) {
int written = 0;
return Fin.Succ(written);
```

Why

The only consumer sequences `Evaluate(interior).Side` and `ClipHalfplane`, exposing the clip's internal protocol. The fallback count is derivable from the emitted boolean span, and `Flip` has no other consumer.

Change

Move the clip body to `Halfplane.Clip`, nest `Evaluate` as a static local function, derive `keep` from `interior`, replace `keep.Flip` with `keep.Times(Sign.Negative)`, require the exact maximum `ring.Length + 1`, return only `written`, and delete public `Evaluate`, `Flip`, and `Predicate.ClipHalfplane`.

Delta

-3 LOC; -2 public members; -2 total members; 0 types; one tuple field removed.

Ripples

At `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:821-829`, call `cut.Clip(interior: at, ...)`, bind the returned `int`, and test `targetMidpointFallback[..written].Contains(true)`. Update `Meshing/mesh.md:201,1063-1066` to the same instance surface.

# 12. Inline the projection-only point conversion

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:470`
```csharp
static Point3d Swizzled(Point3d p, Axis axis) => new(axis.U.Read(p), axis.V.Read(p), 0.0);
```

To

```csharp
// Predicate.Swizzled DELETED
```

Why

`Swizzled` constructs a temporary host value only to forward two projected coordinates into a scalar kernel. Both callers already hold the coordinate axes.

Change

Delete `Swizzled` and pass `axis.U.Read(point)` and `axis.V.Read(point)` directly from the explicit `Orient2D` and `InCircle` paths.

Delta

-1 LOC; -1 module-level member; 0 types.

# 13. Make exact-coordinate selection exhaustive

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:62-63`
```csharp
internal T Pick<T>(in (T X, T Y, T Z, T Lambda) h) where T : struct, IExact<T> =>
    Key == 0 ? h.X : Key == 1 ? h.Y : h.Z;
```

To

```csharp
internal T Pick<T>(in (T X, T Y, T Z, T Lambda) h) where T : struct, IExact<T> =>
    Switch(state: h,
        x: static (value, _) => value.X,
        y: static (value, _) => value.Y,
        z: static (value, _) => value.Z);
```

Why

The final `else` silently treats any future `Axis` row as `Z`, contradicting the closed vocabulary used by `U` and `V`. Generated exhaustive dispatch makes a new row break at the owner.

Change

Replace the key ladder with generated state-threaded `Switch`; keep the tuple generic and closure-free.

Delta

+3 LOC; 0 members; 0 types; one non-exhaustive fallback removed.

# 14. Use canonical names for the implicit point and directed boundary

From

`libs/dotnet/Rasm/.planning/Numerics/predicates.md:85-87,160-165`
```csharp
[Union<Point3d, Implicit.SegmentIntersection, Implicit.LinePlaneIntersection, Implicit.ThreePlaneIntersection>(
    T1Name = "Explicit", T2Name = "SegmentIntersection", T3Name = "LinePlaneIntersection", T4Name = "ThreePlaneIntersection")]
public readonly partial struct Implicit {
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Halfplane {
    private Halfplane() { }

    public sealed record Frame(Point3d Origin, Point3d Along, Axis Plane) : Halfplane;
    public sealed record Affine(Vector3d Normal, double Constant) : Halfplane;
```

To

```csharp
[Union<Point3d, ImplicitPoint.SegmentIntersection, ImplicitPoint.LinePlaneIntersection, ImplicitPoint.ThreePlaneIntersection>(
    T1Name = "Explicit", T2Name = "SegmentIntersection", T3Name = "LinePlaneIntersection", T4Name = "ThreePlaneIntersection")]
public readonly partial struct ImplicitPoint {
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HalfPlane {
    private HalfPlane() { }

    public sealed record DirectedLine(Point3d Start, Point3d End, Axis Projection) : HalfPlane;
    public sealed record Affine(Vector3d Normal, double Constant) : HalfPlane;
```

Why

`Implicit` is an adjective where the type carries a point; `ImplicitPoint` is the established computational-geometry term. `Frame` does not carry a coordinate frame but a directed boundary line and projection plane.

Change

Rename declarations, the nested case, payload members, generated references, signatures, constructors, prose, and calls atomically; add no aliases or compatibility members.

Delta

0 LOC; 0 members; 0 types; six vague or malformed public identifiers removed.

Ripples

Rename numeric `Implicit` references in `libs/dotnet/Rasm/.planning/Drawing/hatch.md`, `Meshing/arrangement.md`, `Meshing/delaunay.md`, `Meshing/intersect.md`, `Meshing/offset.md`, `Processing/repair.md`, `libs/dotnet/Rasm/ARCHITECTURE.md`, and `libs/dotnet/Rasm/RULINGS.md`. Rename `Halfplane` in `Meshing/delaunay.md` and `Meshing/mesh.md`, and replace `Halfplane.Frame` with `HalfPlane.DirectedLine` using the renamed payload fields.
