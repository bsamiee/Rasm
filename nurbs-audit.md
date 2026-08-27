# 1. Remove the copied pool and import accumulated-error support
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L32-L32**
```csharp
using CommunityToolkit.HighPerformance.Buffers;
```
**To**
```csharp
using LanguageExt.Common;
```
**Why**
The sole pooled rental is copied immediately into the returned immutable array; the rebuilt admission instead needs LanguageExt's canonical `Error` carrier.
**Change**
Remove the CommunityToolkit buffer import after task 8 replaces the copied rental, and import `LanguageExt.Common` for task 14's accumulated control admission.
**Delta**
LOC 0; types 0; members 0; package touchpoints -1.

# 2. Make the parameter axis keyless
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L45-L49**
```csharp
[SmartEnum<int>]
public sealed partial class ParametricDirection {
    public static readonly ParametricDirection U = new(0);
    public static readonly ParametricDirection V = new(1);
}
```
**To**
```csharp
[SmartEnum]
public sealed partial class ParametricDirection {
    public static readonly ParametricDirection U = new();
    public static readonly ParametricDirection V = new();
}
```
**Why**
`U` and `V` are process-local exhaustive-dispatch rows, not persisted or wire identities; numeric keys add a second identity plane with no capability.
**Change**
Use Thinktecture's keyless smart enum while retaining generated `Switch` and `Map` dispatch.
**Ripples**
`libs/dotnet/Rasm/.planning/Parametric/surface.md` symbols `IsolinesOf` and `IsoRows` continue to pass `ParametricDirection.U` and `.V`; no consumer may read `.Key` or construct a numeric direction.
**Delta**
LOC 0; types 0; members 0; key literals -2.

# 3. Delete the duplicated knot-form vocabulary
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L51-L57**
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KnotForm {
    public static readonly KnotForm Clamped  = new("clamped");
    public static readonly KnotForm Periodic = new("periodic");
}
```
**To**
```csharp
// KnotForm DELETED
```
**Why**
Knot admission derives a two-state fact with no payload or behavior. A keyed generated type plus caller-supplied and carrier-copied origin fields stores that fact three times and permits disagreement.
**Change**
Represent the admitted state once as `KnotVector.IsPeriodic`; `false` means clamped. Remove every `KnotForm` constructor argument, property, comparer, and caller assertion through tasks 6, 9, 10, 11, and 14.
**Delta**
LOC -6; types -1; members -2.

# 4. Seat fitting directly on matrix solves and keep their evidence
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L59-L81**
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SplineFit {
    public static readonly SplineFit Interpolate = new("interpolate", Collocate);
    public static readonly SplineFit Approximate = new("approximate", Normalize);

    [UseDelegateFromConstructor] public partial Fin<Arr<double>> Solve(SparseMatrix basis, Arr<double> rhs, Op key);

    static Fin<Arr<double>> Collocate(SparseMatrix basis, Arr<double> rhs, Op key);
    static Fin<Arr<double>> Normalize(SparseMatrix basis, Arr<double> rhs, Op key);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChordRule {
    public static readonly ChordRule Uniform     = new("uniform", static _ => 1.0);
    public static readonly ChordRule Chord       = new("chord", static chord => chord);
    public static readonly ChordRule Centripetal = new("centripetal", Math.Sqrt);

    [UseDelegateFromConstructor] public partial double Metric(double chord);
}
```
**To**
```csharp
[SmartEnum]
public sealed partial class SplineFit {
    public static readonly SplineFit Interpolate = new(
        solve: static (basis, rhs, key) => basis.SolveDetailed(rhs, key: key));
    public static readonly SplineFit Approximate = new(
        solve: static (basis, rhs, key) => basis.SolveLeastSquaresDetailed(rhs, key: key));

    [UseDelegateFromConstructor]
    public partial Fin<LinearSolution> Solve(SparseMatrix basis, Arr<double> rhs, Op key);
}

[SmartEnum]
public sealed partial class ChordRule {
    public static readonly ChordRule Uniform     = new(metric: static _ => 1.0);
    public static readonly ChordRule Chord       = new(metric: static chord => chord);
    public static readonly ChordRule Centripetal = new(metric: Math.Sqrt);

    [UseDelegateFromConstructor] public partial double Metric(double chord);
}
```
**Why**
These are process-local behavior rows. The one-call `Collocate` and `Normalize` shells hide the existing matrix owner, and returning only coefficients discards `LinearSolution.Stop`, route, residual, and path evidence.
**Change**
Bind the exact `SparseMatrix.SolveDetailed` and `SolveLeastSquaresDetailed` catalog members directly. In `FitCurve` and `FitSurface`, read `Stop.IsUsable` before consuming `Solution`; propagate an unusable stop as the operation's typed failure.
**Ripples**
`libs/dotnet/Rasm/.planning/Numerics/matrix.md` remains the sole solve owner and gains no NURBS wrapper. The `FitCurve` and `FitSurface` bodies in this target change from `Fin<Arr<double>>` to `Fin<LinearSolution>` consumption.
**Delta**
LOC -6; types 0; members -2; key literals -5.

# 5. Preserve frame behavior while removing string identity
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L83-L91**
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FrameClosure {
    public static readonly FrameClosure Distributed = new("distributed", static (defect, arc, total) => -defect * arc / total);
    public static readonly FrameClosure Raw         = new("raw", static (_, _, _) => 0.0);

    [UseDelegateFromConstructor] public partial double Twist(double defect, double arc, double total);
}
```
**To**
```csharp
[SmartEnum]
public sealed partial class FrameClosure {
    public static readonly FrameClosure Distributed = new(
        twist: static (defect, arc, total) => -defect * arc / total);
    public static readonly FrameClosure Raw = new(
        twist: static (_, _, _) => 0.0);

    [UseDelegateFromConstructor] public partial double Twist(double defect, double arc, double total);
}
```
**Why**
Distributed correction and raw transport are genuinely different algorithms, so collapsing them to a boolean would weaken the behavior owner. Their string keys and comparer attributes still provide no wire identity.
**Change**
Keep the delegate-bearing row and generated dispatch, but make it keyless and name the constructor column explicitly.
**Delta**
LOC -1; types 0; members 0; key literals -2.

# 6. Rebuild knot admission around the active domain
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L123-L169**
```csharp
public readonly record struct KnotVector(int Degree, Arr<double> Knots, KnotForm Form) {
    public int Count => Knots.Count;
    public int ControlCount => Knots.Count - Degree - 1;

    public static Fin<KnotVector> Of(int degree, ReadOnlySpan<double> raw) {
        if (degree < 1 || raw.Length < 2 * degree) { return Fail(degree, raw.Length, "degree under 1 or knot vector under the trimmed floor"); }
        (double lo, double hi) = (raw[0], raw[^1]);
        if (!double.IsFinite(lo) || !double.IsFinite(hi) || hi <= lo) { return Fail(degree, raw.Length, "degenerate knot extent"); }
        double[] knots = new double[raw.Length];
        TensorPrimitives.Subtract(raw, lo, knots);
        TensorPrimitives.Divide<double>(knots, hi - lo, knots);
        if (!TensorPrimitives.IsFiniteAll<double>(knots)) { return Fail(degree, raw.Length, "non-finite knot after normalization"); }
        for (int i = 1; i < knots.Length; i++) {
            if (knots[i] < knots[i - 1]) { return Fail(degree, raw.Length, $"non-monotone knot at {i}"); }
        }
        int head = 0;
        while (head < knots.Length && Coincident(knots[head], 0.0)) { head++; }
        int tail = 0;
        while (tail < knots.Length && Coincident(knots[^(tail + 1)], 1.0)) { tail++; }
        Option<double[]> clamped = (head, tail) switch {
            (int h, int t) when h == degree + 1 && t == degree + 1 => Some(knots),
            (int h, int t) when h == degree && t == degree => Some<double[]>([0.0, .. knots, 1.0]),
            _ => Option<double[]>.None,
        };
        return clamped.Match(
            Some: vector => vector.Length - degree - 1 < degree + 1
                ? Fail(degree, knots.Length, "control extent under degree + 1")
                : Fin.Succ(new KnotVector(degree, new Arr<double>(vector), KnotForm.Clamped)),
            None: () => !PeriodicWrap(knots, degree)
                ? Fail(degree, knots.Length, "unclamped knot vector — neither clamped, trimmed, nor wrap-periodic")
                : knots.Length - degree - 1 < degree + 1
                    ? Fail(degree, knots.Length, "control extent under degree + 1")
                    : Fin.Succ(new KnotVector(degree, new Arr<double>(knots), KnotForm.Periodic)));

        static Fin<KnotVector> Fail(int degree, int knotCount, string detail) =>
            Fin.Fail<KnotVector>(new GeometryFault.InvalidKnotVector(degree, knotCount, detail));
    }

    static bool Coincident(double a, double b) => Math.Abs(a - b) <= EpsilonPolicy.SqrtEpsilon;

    static bool PeriodicWrap(ReadOnlySpan<double> knots, int degree) {
        int n = knots.Length - degree - 1;
        for (int i = 0; i < degree; i++) {
            if (!Coincident(knots[i + n], knots[i] + 1.0)) { return false; }
        }
        return true;
    }
```
**To**
```csharp
internal readonly record struct KnotVector {
    private KnotVector(int degree, Arr<double> knots, bool isPeriodic) =>
        (Degree, Knots, IsPeriodic) = (degree, knots, isPeriodic);

    internal int Degree { get; }
    internal Arr<double> Knots { get; }
    internal bool IsPeriodic { get; }
    internal int ControlCount => Knots.Count - Degree - 1;

    internal static Fin<KnotVector> Of(int degree, ReadOnlySpan<double> raw) {
        if (degree < 1 || raw.Length < 2 * degree) {
            return Fail(degree, raw.Length, "degree under 1 or knot vector under the trimmed floor");
        }
        if (!TensorPrimitives.IsFiniteAll<double>(raw)) {
            return Fail(degree, raw.Length, "non-finite knot");
        }
        for (int i = 1; i < raw.Length; i++) {
            if (raw[i] < raw[i - 1]) { return Fail(degree, raw.Length, $"non-monotone knot at {i}"); }
        }

        int head = 1;
        while (head < raw.Length && Coincident(raw[head], raw[0])) { head++; }
        int tail = 1;
        while (tail < raw.Length && Coincident(raw[^(tail + 1)], raw[^1])) { tail++; }
        return (head, tail) switch {
            (int h, int t) when h == degree + 1 && t == degree + 1 => Normalize(raw.ToArray(), false),
            (int h, int t) when h == degree && t == degree => Normalize([raw[0], .. raw, raw[^1]], false),
            _ => Normalize(raw.ToArray(), true),
        };

        Fin<KnotVector> Normalize(double[] knots, bool periodic) {
            int controls = knots.Length - degree - 1;
            if (controls < degree + 1) { return Fail(degree, knots.Length, "control extent under degree + 1"); }
            (double lo, double hi) = periodic
                ? (knots[degree], knots[controls])
                : (knots[0], knots[^1]);
            if (hi <= lo) { return Fail(degree, knots.Length, "degenerate active knot extent"); }
            TensorPrimitives.Subtract<double>(knots, lo, knots);
            TensorPrimitives.Divide<double>(knots, hi - lo, knots);
            int period = controls - degree;
            if (periodic && !PeriodicWrap(knots, period)) {
                return Fail(degree, knots.Length, "unclamped knot vector is not periodic over its active domain");
            }
            return Fin.Succ(new KnotVector(degree, new Arr<double>(knots), periodic));
        }

        static Fin<KnotVector> Fail(int degree, int knotCount, string detail) =>
            Fin.Fail<KnotVector>(new GeometryFault.InvalidKnotVector(degree, knotCount, detail));
    }

    static bool Coincident(double a, double b) => Math.Abs(a - b) <= EpsilonPolicy.SqrtEpsilon;

    static bool PeriodicWrap(ReadOnlySpan<double> knots, int period) {
        for (int i = 0; i + period < knots.Length; i++) {
            if (!Coincident(knots[i + period], knots[i] + 1.0)) { return false; }
        }
        return true;
    }
```
**Why**
The public positional constructor bypasses every knot invariant. The current periodic path first normalizes the outer extension to `[0,1]`, then incorrectly tests a `+1` wrap using control count as the period; valid periodic extensions are rejected because their active domain is `U[p]..U[n+1]` and their repeating span count is `controlCount - degree`.
**Change**
Seal construction, delete the unused `Count`, admit finiteness and monotonicity once on the input, restore a missing endpoint only for the trimmed clamped spelling, normalize clamped knots by their endpoints and periodic knots by their active domain, then prove every available periodic extension row under the admitted span period. Store only immutable knots and the derived periodic fact.
**Ripples**
Search resolves no `KnotVector` construction or public knot-property consumer outside this target. `Nurbs.AveragedKnots`, fitting, refinement, elevation, ruled construction, and revolution must call `KnotVector.Of`; `NurbsForm.ToEncodeForm` remains the sole public degree/knot projection.
**Delta**
LOC +5; public types -1; public members -5; construction paths -1; duplicated branch carriers -1.

# 7. Restrict span search to the knot kernel
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L171-L184**
```csharp
    public int SpanAt(double t) {
        if (Form == KnotForm.Periodic) {
            (double lo, double hi) = (Knots[Degree], Knots[ControlCount]);
            t = lo + ((((t - lo) % (hi - lo)) + (hi - lo)) % (hi - lo));
        }
        int n = ControlCount - 1;
        if (t >= Knots[n + 1]) { return n; }
        (int lo2, int hi2) = (Degree, n + 1);
        while (hi2 - lo2 > 1) {
            int mid = (lo2 + hi2) >> 1;
            if (t < Knots[mid]) { hi2 = mid; } else { lo2 = mid; }
        }
        return lo2;
    }
```
**To**
```csharp
    internal int SpanAt(double t) {
        if (IsPeriodic) {
            (double lo, double hi) = (Knots[Degree], Knots[ControlCount]);
            t = lo + Reduce.Floored(t - lo, hi - lo);
        }
        int n = ControlCount - 1;
        if (t >= Knots[n + 1]) { return n; }
        (int lo2, int hi2) = (Degree, n + 1);
        while (hi2 - lo2 > 1) {
            int mid = (lo2 + hi2) >> 1;
            if (t < Knots[mid]) { hi2 = mid; } else { lo2 = mid; }
        }
        return lo2;
    }
```
**Why**
Span lookup is internal De Boor machinery, and the handwritten double-modulo expression duplicates `Numerics.Reduce.Floored` exactly.
**Change**
Make the member internal, branch on the admitted boolean, and compose the existing modular-reduction owner while retaining the binary search.
**Delta**
LOC 0; types 0; public members -1; repeated arithmetic expressions -1.

# 8. Merge knots into the final allocation
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L186-L194**
```csharp
    public Arr<double> Merged(ReadOnlySpan<double> inserts) {
        using MemoryOwner<double> staging = MemoryOwner<double>.Allocate(Knots.Count + inserts.Length);
        Span<double> merged = staging.Span;
        (int a, int b, int at) = (0, 0, 0);
        while (a < Knots.Count && b < inserts.Length) { merged[at++] = Knots[a] <= inserts[b] ? Knots[a++] : inserts[b++]; }
        while (a < Knots.Count) { merged[at++] = Knots[a++]; }
        while (b < inserts.Length) { merged[at++] = inserts[b++]; }
        return new Arr<double>([.. merged]);
    }
```
**To**
```csharp
    internal Arr<double> Merge(ReadOnlySpan<double> inserts) {
        double[] merged = new double[Knots.Count + inserts.Length];
        (int a, int b, int at) = (0, 0, 0);
        while (a < Knots.Count && b < inserts.Length) { merged[at++] = Knots[a] <= inserts[b] ? Knots[a++] : inserts[b++]; }
        while (a < Knots.Count) { merged[at++] = Knots[a++]; }
        while (b < inserts.Length) { merged[at++] = inserts[b++]; }
        return new Arr<double>(merged);
    }
```
**Why**
The pool rental is copied immediately into a new array, so it adds ownership and disposal while still paying the result allocation.
**Change**
Allocate the final array once, merge directly into it, use the concise verb `Merge`, and restrict the helper to the refinement kernel.
**Ripples**
Search resolves no `.Merged` consumer outside the declaration. `NurbsKernel.InsertKnot` and refinement implementations call internal `Merge`; task 1 removes the now-unused pooling package.
**Delta**
LOC -1; types 0; public members -1; allocations -1.

# 9. Rename the mixed admission union and pair weighted controls
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L197-L207**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NurbsWire {
    private NurbsWire() { }

    public sealed record Curve(int Degree, Arr<double> Knots, Arr<Point3d> Points, Arr<double> Weights, KnotForm Origin) : NurbsWire;
    public sealed record Surface(int DegreeU, int DegreeV, Arr<double> KnotsU, Arr<double> KnotsV, int CountU, Arr<Point3d> Grid, Arr<double> Weights, KnotForm Origin) : NurbsWire;
    public sealed record CurveThrough(Arr<Point3d> Samples, SplinePolicy Policy) : NurbsWire;
    public sealed record SurfaceThrough(int CountU, Arr<Point3d> Samples, SplinePolicy Policy) : NurbsWire;
    public sealed record Ruled(NurbsForm.Curve Edge, NurbsForm.Curve Opposite) : NurbsWire;
    public sealed record Revolved(NurbsForm.Curve Profile, Line Axis, double AngleRadians) : NurbsWire;
}
```
**To**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NurbsInput {
    private NurbsInput() { }

    public sealed record Curve(Dimension Degree, Arr<double> Knots, Arr<(Point3d Point, double Weight)> Controls) : NurbsInput;
    public sealed record Surface(Dimension DegreeU, Dimension DegreeV, Arr<double> KnotsU, Arr<double> KnotsV, Dimension CountU, Arr<(Point3d Point, double Weight)> Controls) : NurbsInput;
    public sealed record CurveFit(Arr<Point3d> Samples, SplinePolicy Policy) : NurbsInput;
    public sealed record SurfaceFit(Dimension CountU, Arr<Point3d> Samples, SplinePolicy Policy) : NurbsInput;
    public sealed record Ruled(NurbsForm.Curve Edge, NurbsForm.Curve Opposite) : NurbsInput;
    public sealed record Revolved(NurbsForm.Curve Profile, Line Axis, double AngleRadians) : NurbsInput;
}
```
**Why**
The union includes fitting requests and already-admitted constructive carriers, so `Wire` and `Through` are misleading. Parallel point/weight arrays permit arity drift, caller-supplied origin contradicts knot admission, and raw public counts violate the sheet's `Dimension` boundary rule.
**Change**
Rename the owner to `NurbsInput`, use standard `CurveFit` and `SurfaceFit` cases, pair each point with its weight, remove origin assertions, and admit public degree and grid counts as `Dimension`.
**Ripples**
Replace `NurbsWire.CurveThrough` with `NurbsInput.CurveFit` in `libs/dotnet/Rasm/.planning/Parametric/curve.md` symbols `ReconstructOf` and `SeedFit`, and in `libs/dotnet/Rasm.Fabrication/.planning/Geometry2D/curves.md` symbol `CurveAlgebra.Fit`; update the package-law references in both files. Rename this target's generated switch arms to `curveFit` and `surfaceFit`. Raw curve and surface emitters zip points with weights once and construct admitted dimensions before this boundary.
**Delta**
LOC 0; types 0; fields -4; raw public counts -4; misleading case names -2.

# 10. Give curve records immutable homogeneous state
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L215-L230**
```csharp
    public sealed record Curve : NurbsForm {
        internal Curve(KnotVector knots, double[] wx, double[] wy, double[] wz, double[] w, KnotForm origin) {
            (Knots, WX, WY, WZ, W, Origin) = (knots, wx, wy, wz, w, origin);
        }

        public KnotVector Knots { get; }
        public KnotForm Origin { get; }
        internal double[] WX { get; }
        internal double[] WY { get; }
        internal double[] WZ { get; }
        internal double[] W { get; }

        public int ControlCount => W.Length;
        public Arr<Point3d> ControlPoints => new([.. Enumerable.Range(0, W.Length).Select(i => new Point3d(WX[i] / W[i], WY[i] / W[i], WZ[i] / W[i]))]);
        public Arr<double> Weights => new((double[])W.Clone());
        public bool IsClosed(Context context) => PointAt(0.0).DistanceTo(PointAt(1.0)) <= context.For(lane: ToleranceLane.Closure).Value;
```
**To**
```csharp
    public sealed record Curve : NurbsForm {
        internal Curve(KnotVector knots, Arr<double> wx, Arr<double> wy, Arr<double> wz, Arr<double> w) =>
            (Knots, WX, WY, WZ, W) = (knots, wx, wy, wz, w);

        internal KnotVector Knots { get; }
        internal Arr<double> WX { get; }
        internal Arr<double> WY { get; }
        internal Arr<double> WZ { get; }
        internal Arr<double> W { get; }

        public Dimension ControlCount => Dimension.Create(value: W.Count);
        public Arr<Point3d> ControlPoints => new([.. Enumerable.Range(0, W.Count).Select(i => new Point3d(WX[i] / W[i], WY[i] / W[i], WZ[i] / W[i]))]);
        public Arr<double> Weights => W;
        public bool IsClosed(Context context) => PointAt(0.0).DistanceTo(PointAt(1.0)) <= context.For(lane: ToleranceLane.Closure).Value;
```
**Why**
Record equality over mutable array references is not value semantics. `Arr<double>` supplies immutable structural columns and makes the defensive weights clone unnecessary; copied origin and public raw knots are redundant escape surfaces.
**Change**
Store homogeneous columns as `Arr<double>`, remove origin, keep knots internal, return weights directly, and expose the public count as the existing admitted `Dimension`.
**Ripples**
`libs/dotnet/Rasm/.planning/Parametric/curve.md` symbol `Fill` reads `loop.ControlCount.Value` in its integer expression. `NurbsForm.ToEncodeForm` remains inside the owner and can read internal knots directly.
**Delta**
LOC -3; types 0; public members -2; mutable columns -4; defensive copies -1.

# 11. Give surface records immutable homogeneous state
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L262-L278**
```csharp
    public sealed record Surface : NurbsForm {
        internal Surface(KnotVector knotsU, KnotVector knotsV, double[] wx, double[] wy, double[] wz, double[] w, KnotForm origin) {
            (KnotsU, KnotsV, WX, WY, WZ, W, Origin) = (knotsU, knotsV, wx, wy, wz, w, origin);
        }

        public KnotVector KnotsU { get; }
        public KnotVector KnotsV { get; }
        public KnotForm Origin { get; }
        internal double[] WX { get; }
        internal double[] WY { get; }
        internal double[] WZ { get; }
        internal double[] W { get; }

        public int CountU => KnotsU.ControlCount;
        public int CountV => KnotsV.ControlCount;
        public Arr<Point3d> ControlPoints => new([.. Enumerable.Range(0, W.Length).Select(i => new Point3d(WX[i] / W[i], WY[i] / W[i], WZ[i] / W[i]))]);
        public Arr<double> Weights => new((double[])W.Clone());
```
**To**
```csharp
    public sealed record Surface : NurbsForm {
        internal Surface(KnotVector knotsU, KnotVector knotsV, Arr<double> wx, Arr<double> wy, Arr<double> wz, Arr<double> w) =>
            (KnotsU, KnotsV, WX, WY, WZ, W) = (knotsU, knotsV, wx, wy, wz, w);

        internal KnotVector KnotsU { get; }
        internal KnotVector KnotsV { get; }
        internal Arr<double> WX { get; }
        internal Arr<double> WY { get; }
        internal Arr<double> WZ { get; }
        internal Arr<double> W { get; }

        public Dimension CountU => Dimension.Create(value: KnotsU.ControlCount);
        public Dimension CountV => Dimension.Create(value: KnotsV.ControlCount);
        public Arr<Point3d> ControlPoints => new([.. Enumerable.Range(0, W.Count).Select(i => new Point3d(WX[i] / W[i], WY[i] / W[i], WZ[i] / W[i]))]);
        public Arr<double> Weights => W;
```
**Why**
The surface has the same mutable-array equality defect and duplicated origin. Its raw public grid counts also contradict the sheet's admitted-count rule.
**Change**
Store immutable homogeneous columns, remove origin, keep knot vectors internal, return weights without cloning, and expose admitted `Dimension` counts.
**Ripples**
Search resolves no `NurbsForm.Surface.CountU`, `CountV`, `KnotsU`, `KnotsV`, or `Origin` consumer outside this target. `NurbsForm.ToEncodeForm` retains internal access.
**Delta**
LOC -3; types 0; public members -3; mutable columns -4; defensive copies -1.

# 12. Return immutable collections from carriers and kernels
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L234-L449**
```csharp
        public (Point3d Point, Vector3d[] Derivatives) RationalDerivatives(double t, Option<Dimension> order = default);
        public Fin<Plane[]> PerpendicularFrames(ReadOnlySpan<double> parameters, Option<NurbsPolicy> policy = default);
        public Fin<Curve[]> DecomposeIntoBeziers();
        public Vector3d[][] RationalDerivatives(double u, double v, Option<Dimension> order = default);
        public Fin<Surface[]> DecomposeIntoBeziers();
    internal static (Point3d Point, Vector3d[] Ders) CurveRationalDerivatives(NurbsForm.Curve curve, double t, int order);
    internal static Vector3d[][] SurfaceRationalDerivatives(NurbsForm.Surface surface, double u, double v, int order);
    internal static NurbsForm.Curve[] BezierSegments(NurbsForm.Curve curve);
    internal static Fin<Plane[]> DoubleReflectionFrames(NurbsForm.Curve curve, ReadOnlySpan<double> parameters, NurbsPolicy policy);
```
**To**
```csharp
        public (Point3d Point, Arr<Vector3d> Derivatives) RationalDerivatives(double t, Option<Dimension> order = default);
        public Fin<Arr<Plane>> PerpendicularFrames(ReadOnlySpan<double> parameters, Option<NurbsPolicy> policy = default);
        public Fin<Arr<Curve>> DecomposeIntoBeziers();
        public Arr<Arr<Vector3d>> RationalDerivatives(double u, double v, Option<Dimension> order = default);
        public Fin<Arr<Surface>> DecomposeIntoBeziers();
    internal static (Point3d Point, Arr<Vector3d> Ders) CurveRationalDerivatives(NurbsForm.Curve curve, double t, int order);
    internal static Arr<Arr<Vector3d>> SurfaceRationalDerivatives(NurbsForm.Surface surface, double u, double v, int order);
    internal static Arr<NurbsForm.Curve> BezierSegments(NurbsForm.Curve curve);
    internal static Fin<Arr<Plane>> DoubleReflectionFrames(NurbsForm.Curve curve, ReadOnlySpan<double> parameters, NurbsPolicy policy);
```
**Why**
Mutable arrays and a jagged array escape immutable carrier records, forcing consumers to wrap results and allowing result mutation after admission.
**Change**
Return `Arr` at every carrier collection boundary and from the matching kernels. Keep temporary arrays only inside arithmetic bodies and freeze once at return.
**Ripples**
In `libs/dotnet/Rasm/.planning/Parametric/curve.md`, `EvaluateOf` deconstructs `Arr<Vector3d>` and passes it directly to `ParametricResult.Sample`; `StationsOf` uses `frames.Count` and passes `frames` directly to `StationField`. In `libs/dotnet/Rasm/.planning/Parametric/surface.md`, `CurvatureOf` reads `Arr<Arr<Vector3d>>`. `libs/dotnet/Rasm/.planning/Parametric/patternmap.md` and `develop.md` retain indexed derivative reads.
**Delta**
LOC -3 across consumers; types 0; members 0; mutable collection surfaces -9.

# 13. Delete the point-at-length forwarding shell
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L244-L244**
```csharp
        public Fin<Point3d> PointAtLength(double length, Option<NurbsPolicy> policy = default, Op? key = null);
```
**To**
```csharp
// PointAtLength DELETED
```
**Why**
The member is only `ParameterAtLength(...).Map(PointAt)` and has no repository consumer, so it duplicates the existing `Fin.Map` composition.
**Change**
Delete the declaration and body. A future caller composes the retained parameter inversion with `PointAt` directly.
**Delta**
LOC -1; types 0; members -1.

# 14. Accumulate one weighted-control admission
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L318-L377**
```csharp
public static class Nurbs {
    public static Fin<NurbsForm> Of(NurbsWire wire, Op? key = null) =>
        wire.Switch(
            state: key.OrDefault(),
            curve:          static (_, c) => AdmitCurve(c),
            surface:        static (_, s) => AdmitSurface(s),
            curveThrough:   static (k, f) => FitCurve(f.Samples, f.Policy, k),
            surfaceThrough: static (k, f) => FitSurface(f.CountU, f.Samples, f.Policy, k),
            ruled:          static (k, r) => AdmitRuled(r.Edge, r.Opposite, k),
            revolved:       static (k, r) => AdmitRevolved(r.Profile, r.Axis, r.AngleRadians, k));

    static Fin<NurbsForm> AdmitCurve(NurbsWire.Curve wire) =>
        (KnotVector.Of(wire.Degree, [.. wire.Knots]).ToValidation(),
         WeightsPositive(wire.Weights, Kind.Curve).ToValidation(),
         PointsFinite(wire.Points, Kind.Curve).ToValidation())
        .Apply(static (knots, _, _) => knots).As().ToFin()
        .Bind(knots => knots.ControlCount != wire.Points.Count || wire.Weights.Count != wire.Points.Count
            ? Fin.Fail<NurbsForm>(new GeometryFault.DegenerateInput(Kind.Curve, None, "control/weight extent disagrees with the knot vector"))
            : Homogenize(wire.Points, wire.Weights) switch {
                (double[] wx, double[] wy, double[] wz, double[] w) =>
                    Fin.Succ<NurbsForm>(new NurbsForm.Curve(knots, wx, wy, wz, w, wire.Origin)),
            });

    static Fin<NurbsForm> AdmitSurface(NurbsWire.Surface wire) =>
        (KnotVector.Of(wire.DegreeU, [.. wire.KnotsU]).ToValidation(),
         KnotVector.Of(wire.DegreeV, [.. wire.KnotsV]).ToValidation(),
         WeightsPositive(wire.Weights, Kind.Surface).ToValidation(),
         PointsFinite(wire.Grid, Kind.Surface).ToValidation())
        .Apply(static (u, v, _, _) => (U: u, V: v)).As().ToFin()
        .Bind(axes => axes.U.ControlCount != wire.CountU
                || wire.Grid.Count != axes.U.ControlCount * axes.V.ControlCount
                || wire.Weights.Count != wire.Grid.Count
            ? Fin.Fail<NurbsForm>(new GeometryFault.DegenerateInput(Kind.Surface, None, "grid extent disagrees with the knot vectors"))
            : Homogenize(wire.Grid, wire.Weights) switch {
                (double[] wx, double[] wy, double[] wz, double[] w) =>
                    Fin.Succ<NurbsForm>(new NurbsForm.Surface(axes.U, axes.V, wx, wy, wz, w, wire.Origin)),
            });

    static Fin<Arr<double>> WeightsPositive(Arr<double> weights, Kind kind) =>
        toSeq(Enumerable.Range(0, weights.Count)).Find(i => !ValidityClaim.Positive(value: weights[i])).Match(
            Some: at => Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(kind, at, "non-positive weight")),
            None: () => Fin.Succ(weights));

    static Fin<Arr<Point3d>> PointsFinite(Arr<Point3d> points, Kind kind) =>
        toSeq(Enumerable.Range(0, points.Count)).Find(i => !ValidityClaim.Finite(value: points[i])).Match(
            Some: at => Fin.Fail<Arr<Point3d>>(new GeometryFault.DegenerateInput(kind, at, "non-finite control point")),
            None: () => Fin.Succ(points));

    static (double[] WX, double[] WY, double[] WZ, double[] W) Homogenize(Arr<Point3d> points, Arr<double> weights) {
        int n = points.Count;
        (double[] wx, double[] wy, double[] wz, double[] w) = (new double[n], new double[n], new double[n], new double[n]);
        for (int i = 0; i < n; i++) {
            (wx[i], wy[i], wz[i], w[i]) = (weights[i] * points[i].X, weights[i] * points[i].Y, weights[i] * points[i].Z, weights[i]);
        }
        return (wx, wy, wz, w);
    }

    // --- [FITTING]
    static Fin<NurbsForm> FitCurve(Arr<Point3d> samples, SplinePolicy policy, Op key);
    static Fin<NurbsForm> FitSurface(int countU, Arr<Point3d> samples, SplinePolicy policy, Op key);
```
**To**
```csharp
public static class Nurbs {
    public static Fin<NurbsForm> Of(NurbsInput input, Op? key = null) =>
        input.Switch(
            state: key.OrDefault(),
            curve:      static (_, c) => AdmitCurve(c),
            surface:    static (_, s) => AdmitSurface(s),
            curveFit:   static (k, f) => FitCurve(f.Samples, f.Policy, k),
            surfaceFit: static (k, f) => FitSurface(f.CountU, f.Samples, f.Policy, k),
            ruled:      static (k, r) => AdmitRuled(r.Edge, r.Opposite, k),
            revolved:   static (k, r) => AdmitRevolved(r.Profile, r.Axis, r.AngleRadians, k));

    static Fin<NurbsForm> AdmitCurve(NurbsInput.Curve input) =>
        (KnotVector.Of(input.Degree.Value, [.. input.Knots]).ToValidation(),
         AdmitControls(input.Controls, Kind.Curve))
        .Apply(static (knots, _) => knots).As().ToFin()
        .Bind(knots => knots.ControlCount != input.Controls.Count
            ? Fin.Fail<NurbsForm>(new GeometryFault.DegenerateInput(Kind.Curve, None, "control extent disagrees with the knot vector"))
            : Homogenize(input.Controls) switch {
                (Arr<double> wx, Arr<double> wy, Arr<double> wz, Arr<double> w) =>
                    Fin.Succ<NurbsForm>(new NurbsForm.Curve(knots, wx, wy, wz, w)),
            });

    static Fin<NurbsForm> AdmitSurface(NurbsInput.Surface input) =>
        (KnotVector.Of(input.DegreeU.Value, [.. input.KnotsU]).ToValidation(),
         KnotVector.Of(input.DegreeV.Value, [.. input.KnotsV]).ToValidation(),
         AdmitControls(input.Controls, Kind.Surface))
        .Apply(static (u, v, _) => (U: u, V: v)).As().ToFin()
        .Bind(axes => axes.U.ControlCount != input.CountU.Value
                || input.Controls.Count != axes.U.ControlCount * axes.V.ControlCount
            ? Fin.Fail<NurbsForm>(new GeometryFault.DegenerateInput(Kind.Surface, None, "grid extent disagrees with the knot vectors"))
            : Homogenize(input.Controls) switch {
                (Arr<double> wx, Arr<double> wy, Arr<double> wz, Arr<double> w) =>
                    Fin.Succ<NurbsForm>(new NurbsForm.Surface(axes.U, axes.V, wx, wy, wz, w)),
            });

    static Validation<Error, Unit> AdmitControls(Arr<(Point3d Point, double Weight)> controls, Kind kind) =>
        AdmissionSlots.Accumulate(toSeq(Enumerable.Range(0, controls.Count)).Bind(i => {
            (Point3d point, double weight) = controls[i];
            return Seq(
                AdmissionSlots.Gate(ValidityClaim.Finite(point), new GeometryFault.DegenerateInput(kind, i, "non-finite control point")),
                AdmissionSlots.Gate(ValidityClaim.Positive(weight), new GeometryFault.DegenerateInput(kind, i, "non-positive weight")));
        }));

    static (Arr<double> WX, Arr<double> WY, Arr<double> WZ, Arr<double> W) Homogenize(Arr<(Point3d Point, double Weight)> controls) {
        (double[] wx, double[] wy, double[] wz, double[] w) = (new double[controls.Count], new double[controls.Count], new double[controls.Count], new double[controls.Count]);
        for (int i = 0; i < controls.Count; i++) {
            (Point3d point, double weight) = controls[i];
            (wx[i], wy[i], wz[i], w[i]) = (weight * point.X, weight * point.Y, weight * point.Z, weight);
        }
        return (new Arr<double>(wx), new Arr<double>(wy), new Arr<double>(wz), new Arr<double>(w));
    }

    // --- [FITTING]
    static Fin<NurbsForm> FitCurve(Arr<Point3d> samples, SplinePolicy policy, Op key);
    static Fin<NurbsForm> FitSurface(Dimension countU, Arr<Point3d> samples, SplinePolicy policy, Op key);
```
**Why**
The current boundary validates parallel arrays independently, converts first-failure `Fin` values to `Validation`, then separately validates their arity. Pairing deletes the arity invariant; `AdmissionSlots` already owns applicative refusal accumulation.
**Change**
Fold `NurbsInput`, unwrap admitted dimensions only at the raw knot boundary, accumulate every point and weight refusal once, retain only knot-dependent extent gates, homogenize paired controls directly, freeze the homogeneous columns as `Arr`, and delete `WeightsPositive` plus `PointsFinite`.
**Ripples**
`libs/dotnet/Rasm/.planning/Domain/validation.md` remains the sole `AdmissionSlots` owner. `libs/dotnet/Rasm/.planning/Numerics/faults.md` continues to own `GeometryFault.DegenerateInput`. Raw bridge emitters construct paired controls once and do not pre-validate them.
**Delta**
LOC -10; types 0; members -1; independent boundary invariants -1.

# 15. Delete the one-line total projection
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L416-L417**
```csharp
    internal static double TotalLength(ReadOnlySpan<ddouble> cumulative) =>
        cumulative.IsEmpty ? 0.0 : (double)cumulative[^1];
```
**To**
```csharp
// TotalLength DELETED
```
**Why**
The member has no repository consumer outside the target and only names one conditional projection over the cumulative array.
**Change**
Inline `cumulative.IsEmpty ? 0.0 : (double)cumulative[^1]` in the curve length body and delete the kernel member.
**Delta**
LOC -2; types 0; members -1.

# 16. Localize projection functions and return the root verdict directly
**From — libs/dotnet/Rasm/.planning/Parametric/nurbs.md:L429-L444**
```csharp
    internal static Fin<double> NewtonProject(NurbsForm.Curve curve, Point3d probe, double seedLo, double seedHi, NurbsPolicy policy, Op key) =>
        key.Catch(() => Fin.Succ(RobustNewtonRaphson.TryFindRoot(
                t => ProjectionObjective(curve, probe, t),
                t => ProjectionSlope(curve, probe, t),
                seedLo, seedHi, policy.ProjectTolerance, policy.ProjectIterations.Value, policy.ProjectSubdivision.Value,
                out double root)
            ? Some(root)
            : Option<double>.None))
        .Bind(root => root.ToFin(Fail: new GeometryFault.CurveProjectionUnconverged(probe)));

    internal static double ProjectionObjective(NurbsForm.Curve curve, Point3d probe, double t) =>
        Jet(curve, t) switch { var jet => (jet.Point - probe) * jet.Ders[0] };
    internal static double ProjectionSlope(NurbsForm.Curve curve, Point3d probe, double t) =>
        Jet(curve, t) switch { var jet => (jet.Ders[0] * jet.Ders[0]) + ((jet.Point - probe) * jet.Ders[1]) };

    static (Point3d Point, Vector3d[] Ders) Jet(NurbsForm.Curve curve, double t) => CurveRationalDerivatives(curve, t, 2);
```
**To**
```csharp
    internal static Fin<double> NewtonProject(
        NurbsForm.Curve curve, Point3d probe, double seedLo, double seedHi, NurbsPolicy policy, Op key) {
        double Objective(double t) => CurveRationalDerivatives(curve, t, 2) switch
            { var jet => (jet.Point - probe) * jet.Ders[0] };
        double Slope(double t) => CurveRationalDerivatives(curve, t, 2) switch
            { var jet => (jet.Ders[0] * jet.Ders[0]) + ((jet.Point - probe) * jet.Ders[1]) };

        return key.Catch(() => RobustNewtonRaphson.TryFindRoot(
                Objective, Slope, seedLo, seedHi,
                policy.ProjectTolerance, policy.ProjectIterations.Value, policy.ProjectSubdivision.Value,
                out double root)
            ? Fin.Succ(root)
            : Fin.Fail<double>(new GeometryFault.CurveProjectionUnconverged(probe)));
    }
```
**Why**
The current path wraps a boolean/out package result in `Option`, then `Fin`, then unwraps it to `Fin`; three module helpers exist only for this call.
**Change**
Return success or the typed projection failure directly inside `Op.Catch`, keep the exact MathNet robust Newton owner, and localize objective and slope to their only caller.
**Delta**
LOC -5; types 0; members -3; result wrappers -1.
