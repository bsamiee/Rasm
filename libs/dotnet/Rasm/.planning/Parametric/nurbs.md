# [RASM_PARAMETRIC_NURBS]

`Rasm.Parametric` owns the host-neutral NURBS engine — the whole rational curve and surface algorithm set in-kernel over `Rhino.Geometry` `Point3d`/`Vector3d`/`Plane` native carriers, control nets held in homogeneous form. `Nurbs.Of` is the ONE polymorphic admission for every wire ingress; evaluation members live on the `NurbsForm` carriers, and the op rails compose this engine from `curve.md`/`surface.md`.

Fitting solves compose the `matrix.md` sparse owners through `SplineFit`'s own solve column; arc-length routes its per-Bezier speed integrals through the kernel `Numerics/integrate` `Quadrature.Integrate` owner and composes `MathNet` for length inversion and Newton projection while the Bezier decomposition and `|C′(t)|` speed integrand stay local. `ToEncodeForm()` projects into the reconciliation `EncodeForm` chain for one content key per curve across every ingress spelling, and a degeneracy-sensitive verdict escalates to the `Numerics/predicates` exact ladder at the consumer seam — evaluation is `double`-only geometry, never the adjudication.

## [01]-[INDEX]

- [02]-[NURBS_ENGINE]: `KnotVector` the normalized clamped-or-periodic knot algebra with `KnotForm`/`ParametricDirection` vocabularies; `SplineFit`/`ChordRule`/`FrameClosure` the behavior-bearing rows; `NurbsWire` the admission `[Union]`; `NurbsForm` the curve/surface carrier `[Union]` holding the full evaluation surface; `NurbsPolicy` the engine knob row; `Nurbs.Of` the ONE admission.

## [02]-[NURBS_ENGINE]

- Owner: `Nurbs` mints the static admission surface folding the `NurbsWire` admission `[Union]` into the `NurbsForm` carrier `[Union]`; `KnotVector` owns the knot algebra proving monotone/finite and clamped-or-wrap-periodic, admitting the full, Rhino-trimmed, and periodic spellings at one seam; `NurbsPolicy` registers `IValidityEvidence`; `ParametricDirection`/`KnotForm` are the `[SmartEnum]` axis and closure-origin discriminants; `SplineFit`/`ChordRule`/`FrameClosure` are the `[SmartEnum]` rows carrying the solve, parameterization, and frame-defect laws as delegate columns.
- Cases: `NurbsWire` cases `Curve`, `Surface`, `CurveThrough`, `SurfaceThrough`, `Ruled`, `Revolved` — fitting modality is `SplinePolicy` data, so interpolate-versus-approximate mints no case, and the constructive cases fold admitted curve carriers through the same `Of` (the loft unifies degree and knots then lofts degree-1, the revolution mints exact rational arcs); the surface grid flattens V-inner (`index = u·CountV + v`), the one flattening law the identity projection shares. `NurbsForm` cases `Curve`, `Surface`.
- Entry: `Of` folds every wire shape through one generated `Switch` — explicit wires validate and freeze into the homogeneous columns, fitting wires parameterize, average knots, and solve. No `OfCurve`/`Interpolate` sibling factory — the wire shape discriminates (`MODAL_ARITY`).
- Auto: evaluation is the internalized NURBS-Book kernel set over the columns — point/derivative, arc-length, closest-parameter projection, double-reflection frame, fundamental-form, iso-curve, and Boehm/Oslo refinement machinery re-emitting normalized clamped forms; the fence pins each member to its algorithm number.
- Law: the two fitting regimes are ONE `SplineFit` roster whose `Solve` column carries the linear system each regime forms — interpolation the banded `N·P = Q`, approximation the SPD `NᵀN·P = NᵀQ` — so the fit lane never branches on row equality and a third regime (penalized, tangent-constrained) is one row. `Solve` reads its `SolveReceipt` stop rather than projecting a solution vector past it, the consumer defect `matrix.md [04]` names.
- Law: parameterization is `ChordRule` data — `Uniform`/`Chord`/`Centripetal` are the A9.3 exponents as one `Metric` column, so the deleted `bool Centripetal` knob cannot spell the uniform third the literature carries.
- Law: every count on the engine's PUBLIC surface is `Dimension` — policy orders and budgets, derivative order, and the degree targets `ElevateDegree`/`ReduceDegree` take — so a non-positive order or target is unrepresentable rather than guarded at each read, and no consumer clamps one into range on this engine's behalf. `KnotVector.Of(int degree, …)` stays raw because it IS the boundary admission. The two tolerance columns anchor on `EpsilonPolicy` and `Of(context)` derives them from `ToleranceLane.Length`/`ToleranceLane.Root` wherever a caller holds a model context.
- Law: closure is MODEL-SPACE and reads `ToleranceLane.Closure` off a threaded `Context`, on both carriers. NAMED LOSS: the parameterless `IsClosed` accessor; a dimensionless anchor called essentially nothing closed on a metre model and disagreed with a millimetre one about the same curve.
- Law: knot coincidence has ONE regime — the `Coincident` band both multiplicity walks and the wrap proof read. NAMED LOSS: exact float equality at the ends, which held only on an unstated bit-exactness invariant a later change to the normalization would have broken silently.
- Receipt: none on a dedicated rail — the `NurbsForm` carrier IS the admitted artifact, and `ToEncodeForm()` is its identity seam into the reconciliation `EncodeForm` chain.
- Packages: `MathNet.Numerics` (`Brent.TryFindRoot` length inversion, `RobustNewtonRaphson.TryFindRoot` guarded Newton projection — both no-throw twins, so budget exhaustion lands as a typed fault on the rail); `TYoshimura.DoubleDouble` (`ddouble` + `DoubleDoubleEnumerableExpand.Sum` — the 106-bit arc-length table, narrowed only at public signatures); `Rhino.Geometry` (`Point3d`/`Vector3d`/`Plane` native carriers); `Rasm.Numerics` (`Quadrature.Integrate`/`IntegrationDomain.Line`/`QuadratureRoute.GaussLegendre`/`QuadratureControl` arc-length quadrature, `SparseMatrix.FromTriplets`/`SolveDetailed` and `CholeskySparse.SolveDetailed` fitting solves, `EpsilonPolicy`/`Dimension` atoms, `GeometryFault.ParametricFault`/`ParametricStage`); `Rasm.Spatial` (`EncodeForm`/`EncodeForm.Direction` identity target); `Rasm.Domain` (`Op`/`Op.Catch`, `Context`/`ToleranceLane`, `ValidityClaim`/`IValidityEvidence`); `Thinktecture.Runtime.Extensions`; `LanguageExt.Core` (`Fin`/`Arr`/`Seq`/`Option`, `Validation` + applicative `Apply` the accumulating admission); `System.Numerics.Tensors` (`TensorPrimitives.Subtract`/`Divide`/`IsFiniteAll` — the knot normalization and its one finiteness reduction); `CommunityToolkit.HighPerformance` (`MemoryOwner<double>` the knot-merge staging plane); BCL inbox.
- Growth: a new evaluation member is one projection over the existing derivative kernels; a new fitting scheme is one `SplineFit` row carrying its own `Solve` column, consumers untouched; a further constructive wire (swept, lofted-through-N) is one `NurbsWire` case folded by the same `Of` — `Ruled`/`Revolved` are the executed precedent — zero new entry surfaces, zero new carriers.
- Boundary: evaluation members live on `NurbsForm` and the op rails live in `curve.md`/`surface.md`, so an op union here or an evaluation re-derivation there is the altitude violation; the engine speaks `Point3d`/`Vector3d`/`Plane` natively with no private point vocabulary or marshal layer; parameters are the normalized `[0,1]`/`[0,1]²` domain and knots store clamped-normalized — or wrap-periodic UNCLAMPED under `KnotForm.Periodic`, where the span arm wraps the parameter and closure holds at `C^{p−1}`; weights are strictly positive at admission and a zero-or-negative weight is a `Construction` fault, never a NaN downstream; `ToEncodeForm` re-proves `EncodeForm.Of`'s normalized-CLAMPED gate, so a periodic carrier refuses identity projection until a consumer clamps it — one key per curve is worth the refusal, a second layout is not; every failure routes `GeometryFault.ParametricFault` naming the failing stage over `Fin`, and no exception crosses the public surface; RhinoCommon owns the Rhino-host parametric surface and this engine the host-neutral one — a runtime split, never capability — with the Rhino-trimmed knot spelling extending at the wire under one admission law.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Linq;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance.Buffers;
using DoubleDouble;
using LanguageExt;
using MathNet.Numerics.RootFinding;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Parametric;

// --- [TYPES] ------------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ParametricDirection {
    public static readonly ParametricDirection U = new(0);
    public static readonly ParametricDirection V = new(1);
}

// Storage form, not a label: Clamped stores the normalized clamped vector, Periodic the true unclamped wrap.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KnotForm {
    public static readonly KnotForm Clamped  = new("clamped");
    public static readonly KnotForm Periodic = new("periodic");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SplineFit {
    public static readonly SplineFit Interpolate = new("interpolate", Collocate);
    public static readonly SplineFit Approximate = new("approximate", Normalize);

    [UseDelegateFromConstructor] public partial Fin<Arr<double>> Solve(SparseMatrix basis, Arr<double> rhs, Op key);

    static Fin<Arr<double>> Collocate(SparseMatrix basis, Arr<double> rhs, Op key);  // banded N·P = Q; the receipt Stop gates the return
    static Fin<Arr<double>> Normalize(SparseMatrix basis, Arr<double> rhs, Op key);  // SPD NᵀN·P = NᵀQ through CholeskySparse, same gate
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

// Wang-2008 terminal-defect policy as a row: the twist a closed curve's frame carries at arc s of total.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FrameClosure {
    public static readonly FrameClosure Distributed = new("distributed", static (defect, arc, total) => -defect * arc / total);
    public static readonly FrameClosure Raw         = new("raw", static (_, _, _) => 0.0);

    [UseDelegateFromConstructor] public partial double Twist(double defect, double arc, double total);
}

// --- [CONSTANTS] --------------------------------------------------------------------------------
// Inversion band stays an order looser than the projection root — a length table cannot resolve tighter.
public sealed record NurbsPolicy(
    Dimension GaussOrder, double LengthTolerance, Dimension ProjectIterations, double ProjectTolerance,
    Dimension ProjectSubdivision, FrameClosure Closure) : IValidityEvidence {
    public static readonly NurbsPolicy Canonical = new(
        GaussOrder: Dimension.Create(value: 32),
        LengthTolerance: EpsilonPolicy.SqrtEpsilon * EpsilonPolicy.SubTolerance,
        ProjectIterations: Dimension.Create(value: 64),
        ProjectTolerance: EpsilonPolicy.ZeroTolerance,
        ProjectSubdivision: Dimension.Create(value: 20),
        Closure: FrameClosure.Distributed);

    // Coarse-tolerance documents never pay for a projection root they cannot resolve.
    public static NurbsPolicy Of(Context context) => Canonical with {
        LengthTolerance = context.For(lane: ToleranceLane.Length).Value,
        ProjectTolerance = context.For(lane: ToleranceLane.Root).Value,
    };

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: LengthTolerance),
        ValidityClaim.Positive(value: ProjectTolerance));
}

// ControlCount is read by Approximate alone — Interpolate's control extent IS the sample count, so a
// stated count on the interpolating row is ignored rather than silently re-shaping the collocation system.
public sealed record SplinePolicy(
    SplineFit Fit, Dimension Degree, ChordRule Rule,
    Option<Vector3d> StartTangent = default, Option<Vector3d> EndTangent = default,
    Option<Dimension> ControlCount = default) {
    public static readonly SplinePolicy Canonical = new(SplineFit.Interpolate, Degree: Dimension.Create(value: 3), Rule: ChordRule.Chord);
}

// --- [MODELS] -----------------------------------------------------------------------------------
// Three admitted wire spellings: full clamped (n+p+1), Rhino-trimmed (n+p−1, end knots duplicated at the
// seam), and unclamped-periodic (the degree-window wrap knots[i+n] = knots[i] + 1 within
// EpsilonPolicy.SqrtEpsilon). Periodic stores UNCLAMPED so a closed curve keeps C^{p−1} across the seam.
public readonly record struct KnotVector(int Degree, Arr<double> Knots, KnotForm Form) {
    public int Count => Knots.Count;
    public int ControlCount => Knots.Count - Degree - 1;

    [BoundaryAdapter]
    public static Fin<KnotVector> Of(int degree, ReadOnlySpan<double> raw) {
        if (degree < 1 || raw.Length < 2 * degree) { return Fail("degree under 1 or knot vector under the trimmed floor"); }
        (double lo, double hi) = (raw[0], raw[^1]);
        if (!double.IsFinite(lo) || !double.IsFinite(hi) || hi <= lo) { return Fail("degenerate knot extent"); }
        // Elementwise normalization into ONE destination, then one vectorized finiteness reduction; only the
        // monotone comparison stays scalar, because no span reducer expresses an ordering predicate.
        double[] knots = new double[raw.Length];
        TensorPrimitives.Subtract(raw, lo, knots);
        TensorPrimitives.Divide<double>(knots, hi - lo, knots);
        if (!TensorPrimitives.IsFiniteAll<double>(knots)) { return Fail("non-finite knot after normalization"); }
        for (int i = 1; i < knots.Length; i++) {
            if (knots[i] < knots[i - 1]) { return Fail($"non-monotone knot at {i}"); }
        }
        int head = 0;
        while (head < knots.Length && Coincident(knots[head], 0.0)) { head++; }
        int tail = 0;
        while (tail < knots.Length && Coincident(knots[^(tail + 1)], 1.0)) { tail++; }
        // Two admitted spellings, discriminated by END MULTIPLICITY: full clamped carries degree+1
        // repeats, the Rhino-trimmed wire carries degree and extends by one duplicate per end. An
        // unrecognized spelling is None — an empty array spelled it as a length the reader re-discriminated.
        Option<double[]> clamped = (head, tail) switch {
            (int h, int t) when h == degree + 1 && t == degree + 1 => Some(knots),
            (int h, int t) when h == degree && t == degree => Some<double[]>([0.0, .. knots, 1.0]),
            _ => Option<double[]>.None,
        };
        return clamped.Match(
            Some: vector => vector.Length - degree - 1 < degree + 1
                ? Fail("control extent under degree + 1")
                : Fin.Succ(new KnotVector(degree, new Arr<double>(vector), KnotForm.Clamped)),
            None: () => !PeriodicWrap(knots, degree)
                ? Fail("unclamped knot vector — neither clamped, trimmed, nor wrap-periodic")
                : knots.Length - degree - 1 < degree + 1
                    ? Fail("control extent under degree + 1")
                    : Fin.Succ(new KnotVector(degree, new Arr<double>(knots), KnotForm.Periodic)));

        static Fin<KnotVector> Fail(string witness) =>
            Fin.Fail<KnotVector>(new GeometryFault.ParametricFault(ParametricStage.Construction, ParametricCarrier.Knots, witness));
    }

    // ONE knot-coincidence regime for the whole owner: the multiplicity walks and the wrap proof read this member,
    // so an exact float comparison resting on an unstated bit-exactness invariant cannot desync from the banded one.
    static bool Coincident(double a, double b) => Math.Abs(a - b) <= EpsilonPolicy.SqrtEpsilon;

    // Periodic spelling carries a degree-window law: each of the leading degree spans repeats the
    // trailing one shifted by the unit period, so an unclamped vector proves its own closure.
    static bool PeriodicWrap(ReadOnlySpan<double> knots, int degree) {
        int n = knots.Length - degree - 1;
        for (int i = 0; i < degree; i++) {
            if (!Coincident(knots[i + n], knots[i] + 1.0)) { return false; }
        }
        return true;
    }

    // A2.1 span search over the normalized domain; t == 1 lands on the last non-degenerate span. The
    // periodic form first wraps t into [Knots[Degree], Knots[ControlCount]) — the one periodic-aware
    // arm; basis evaluation is then form-blind because the wrapped span's local knots are ordinary.
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

    // PRECONDITION: `inserts` is ascending. `Knots` is monotone by admission and Oslo insertion (A5.4) hands its
    // insertions sorted, so the merge is LINEAR — the deleted concatenate-then-Order paid O(n log n) plus a second
    // copy of the insert span to reach a value both operands already ordered.
    public Arr<double> Merged(ReadOnlySpan<double> inserts) {
        using MemoryOwner<double> staging = MemoryOwner<double>.Allocate(Knots.Count + inserts.Length);
        Span<double> merged = staging.Span;
        (int a, int b, int at) = (0, 0, 0);
        while (a < Knots.Count && b < inserts.Length) { merged[at++] = Knots[a] <= inserts[b] ? Knots[a++] : inserts[b++]; }
        while (a < Knots.Count) { merged[at++] = Knots[a++]; }
        while (b < inserts.Length) { merged[at++] = inserts[b++]; }
        return new Arr<double>([.. merged]);
    }
}

// Admission wire: explicit nets and sample sets are both raw ingress, fitting modality is SplinePolicy
// data. Grid flattening is V-inner (index = u·CountV + v) — the one law the identity projection shares.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NurbsWire {
    private NurbsWire() { }

    public sealed record Curve(int Degree, Arr<double> Knots, Arr<Point3d> Points, Arr<double> Weights, KnotForm Origin) : NurbsWire;
    public sealed record Surface(int DegreeU, int DegreeV, Arr<double> KnotsU, Arr<double> KnotsV, int CountU, Arr<Point3d> Grid, Arr<double> Weights, KnotForm Origin) : NurbsWire;
    public sealed record CurveThrough(Arr<Point3d> Samples, SplinePolicy Policy) : NurbsWire;
    public sealed record SurfaceThrough(int CountU, Arr<Point3d> Samples, SplinePolicy Policy) : NurbsWire;
    // Constructive wires over already-admitted curve carriers — the same Of folds them, so a loft or a
    // revolution is an admission case, never a sibling factory.
    public sealed record Ruled(NurbsForm.Curve Rail, NurbsForm.Curve Opposite) : NurbsWire;
    public sealed record Revolved(NurbsForm.Curve Profile, Line Axis, double AngleRadians) : NurbsWire;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NurbsForm {
    private NurbsForm() { }

    // --- [CURVE_CARRIER]
    public sealed record Curve : NurbsForm {
        internal Curve(KnotVector knots, double[] wx, double[] wy, double[] wz, double[] w, KnotForm origin) {
            (Knots, WX, WY, WZ, W, Origin) = (knots, wx, wy, wz, w, origin);
        }

        public KnotVector Knots { get; }
        public KnotForm Origin { get; }
        internal double[] WX { get; }  // homogeneous SoA columns
        internal double[] WY { get; }
        internal double[] WZ { get; }
        internal double[] W { get; }

        public int ControlCount => W.Length;
        public Arr<Point3d> ControlPoints => new([.. Enumerable.Range(0, W.Length).Select(i => new Point3d(WX[i] / W[i], WY[i] / W[i], WZ[i] / W[i]))]);
        public Arr<double> Weights => new((double[])W.Clone());
        // Closure is a MODEL-SPACE coincidence: control points are model coordinates, so the gate reads the
        // Closure lane and a millimetre and a metre document agree about which curves are closed.
        public bool IsClosed(Context context) => PointAt(0.0).DistanceTo(PointAt(1.0)) <= context.For(lane: ToleranceLane.Closure).Value;

        // A3.1/A4.1 homogeneous De Boor combination, dehomogenized at the return.
        public Point3d PointAt(double t);

        // A2.3 + A4.2: homogeneous derivatives Leibniz-corrected to rational C', C'', … C^(n).
        public (Point3d Point, Vector3d[] Derivatives) RationalDerivatives(double t, Option<Dimension> order = default);

        public Vector3d TangentAt(double t);
        public Vector3d CurvatureAt(double t);

        // Bezier-decomposed Gauss-Legendre arc-length: DecomposeIntoBeziers once (cached), then the kernel
        // Quadrature.Integrate Line domain at QuadratureRoute.GaussLegendre with LegendreOrder =
        // policy.GaussOrder per segment into a cumulative table — the rail carries the route's finite guard.
        public Fin<double> Length(Option<NurbsPolicy> policy = default, Op? key = null);
        public Fin<double> LengthAt(double t, Option<NurbsPolicy> policy = default, Op? key = null);

        // Monotone inversion: bracket the containing Bezier segment off the cumulative table, then
        // Brent.TryFindRoot(s(t) − target, ...) — the no-throw bool maps straight to the rail.
        public Fin<double> ParameterAtLength(double length, Option<NurbsPolicy> policy = default, Op? key = null);

        public Fin<Point3d> PointAtLength(double length, Option<NurbsPolicy> policy = default, Op? key = null);

        // Chord inversion for DivideByChordLength-class rails: Brent on the monotone-along-curve
        // |C(t) − C(t0)| − chord over the forward bracket, same knobs and rail.
        public Fin<double> ParameterAtChordLength(double t0, double chordLength, Option<NurbsPolicy> policy = default, Op? key = null);

        // Newton projection on g(t) = (C−P)·C' with g' = |C'|² + (C−P)·C'': polygon-sampled seed
        // bracket, RobustNewtonRaphson.FindRoot under the policy knobs, Op.Catch-funnelled to the rail.
        public Fin<double> ClosestParameter(Point3d probe, Option<NurbsPolicy> policy = default, Op? key = null);

        // Wang-2008 double-reflection RMF: coincident samples carry the prior frame forward (NaN guard);
        // a closed curve distributes the terminal defect through policy.Closure.Twist as tangent twists.
        public Fin<Plane[]> PerpendicularFrames(ReadOnlySpan<double> parameters, Option<NurbsPolicy> policy = default);

        public Fin<(Curve Head, Curve Tail)> SplitAt(double t);
        public Fin<Curve> SubCurve(double t0, double t1);
        public Fin<Curve> Refine(ReadOnlySpan<double> insertions);   // Boehm/Oslo insertion (A5.4), ascending
        public Fin<Curve> ElevateDegree(Dimension target);           // A5.9
        // A5.11 Bezier-segment degree reduction with the exact max-deviation bound per segment; a bound
        // breaching policy.LengthTolerance routes the Construction fault, never a silently lossy carrier.
        public Fin<Curve> ReduceDegree(Dimension target, Option<NurbsPolicy> policy = default);
        public Fin<Curve[]> DecomposeIntoBeziers();                  // A5.6 — the length engine's substrate
        public Curve Reverse();
    }

    // --- [SURFACE_CARRIER]
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
        public bool IsClosed(ParametricDirection direction, Context context);

        public Point3d PointAt(double u, double v);

        // PUBLIC, metric-true, nothing unitized: SKL[k][l] = ∂^{k+l}S/∂u^k∂v^l with k the u-order row
        // and l the v-order column (A3.6 + A4.4).
        public Vector3d[][] RationalDerivatives(double u, double v, Option<Dimension> order = default);

        public Fin<Vector3d> NormalAt(double u, double v);

        // First/second fundamental forms off RationalDerivatives(u, v, 2); a degenerate normal
        // (|Su×Sv| under the scale floor) routes the Evaluation fault, never NaN forms.
        public Fin<(double E, double F, double G, double L, double M, double N)> FundamentalForms(double u, double v);

        // Principal curvatures/directions from the closed-form 2×2 shape operator; K Gaussian, H mean.
        public Fin<(double K1, double K2, Vector3d Dir1, Vector3d Dir2, double Gaussian, double Mean)> CurvatureAt(double u, double v);

        // Basis-row contraction at the fixed parameter → a curve-form control net in the other axis.
        public Fin<Curve> IsoCurve(double parameter, ParametricDirection direction);

        // 2-var Newton over the derivative grid under the policy knobs. A supplied seed skips the
        // sampled-polygon bracket — surface.md's dense Pullback amortizes seeding through ONE batch
        // NeighborIndex query and refines HERE, never a parallel projector.
        public Fin<(double U, double V)> ClosestParameter(Point3d probe, Option<NurbsPolicy> policy = default, Option<(double U, double V)> seed = default, Op? key = null);

        public Fin<(Surface Head, Surface Tail)> SplitAt(double parameter, ParametricDirection direction);
        public Fin<Surface> Refine(ReadOnlySpan<double> insertions, ParametricDirection direction);

        // Per-direction runs of the curve members' own kernels — no algorithm is re-derived here.
        public Fin<Surface> SubSurface(double u0, double u1, double v0, double v1);                 // two SplitAt pairs, one region
        public Fin<Surface> ElevateDegree(Dimension target, ParametricDirection direction);        // A5.10 per-direction
        public Fin<Surface[]> DecomposeIntoBeziers();                                              // A5.7 patch decomposition
        // Exact U/V exchange: the net transposes (index = v·CountU + u), knots swap, orientation flips.
        public Surface Transpose();
        // GL cubature of |Su×Sv| per Bezier patch at policy.GaussOrder per axis, 106-bit accumulated
        // through the same DoubleDoubleEnumerableExpand.Sum lane the arc-length table rides.
        public Fin<double> Area(Option<NurbsPolicy> policy = default, Op? key = null);
    }

    // --- [IDENTITY_PROJECTION]
    // Identity seam: degree+knot Direction rows, positive weights, dehomogenized net in the V-inner
    // flattening — EncodeForm.Of re-proves the normalized-clamped gate, so one curve yields ONE content
    // key across every ingress spelling and a periodic carrier refuses rather than minting a second layout.
    [BoundaryAdapter]
    public Fin<EncodeForm> ToEncodeForm(Op? key = null) => Switch(
        state: key.OrDefault(),
        curve: static (k, c) => EncodeForm.Of(
            new Arr<EncodeForm.Direction>([new EncodeForm.Direction(c.Knots.Degree, c.Knots.Knots)]),
            c.Weights, c.ControlPoints, k),
        surface: static (k, s) => EncodeForm.Of(
            new Arr<EncodeForm.Direction>([
                new EncodeForm.Direction(s.KnotsU.Degree, s.KnotsU.Knots),
                new EncodeForm.Direction(s.KnotsV.Degree, s.KnotsV.Knots)]),
            s.Weights, s.ControlPoints, k));
}

public static class Nurbs {
    [BoundaryAdapter]
    public static Fin<NurbsForm> Of(NurbsWire wire, Op? key = null) =>
        wire.Switch(
            state: key.OrDefault(),
            curve:          static (_, c) => AdmitCurve(c),
            surface:        static (_, s) => AdmitSurface(s),
            curveThrough:   static (k, f) => FitCurve(f.Samples, f.Policy, k),
            surfaceThrough: static (k, f) => FitSurface(f.CountU, f.Samples, f.Policy, k),
            ruled:          static (k, r) => AdmitRuled(r.Rail, r.Opposite, k),
            revolved:       static (k, r) => AdmitRevolved(r.Profile, r.Axis, r.AngleRadians, k));

    // Both admissions ACCUMULATE the columns that are genuinely independent — knot admission, weight positivity,
    // control-point finiteness — so a malformed wire reports every defect at once. Control extent alone sequences,
    // because it reads the admitted vector's own ControlCount and cannot be measured before it.
    static Fin<NurbsForm> AdmitCurve(NurbsWire.Curve wire) =>
        (KnotVector.Of(wire.Degree, [.. wire.Knots]).ToValidation(),
         WeightsPositive(wire.Weights, ParametricCarrier.Curve).ToValidation(),
         PointsFinite(wire.Points, ParametricCarrier.Curve).ToValidation())
        .Apply(static (knots, _, _) => knots).As().ToFin()
        .Bind(knots => knots.ControlCount != wire.Points.Count || wire.Weights.Count != wire.Points.Count
            ? Construction<NurbsForm>(ParametricCarrier.Curve, "control/weight extent disagrees with the knot vector")
            : Homogenize(wire.Points, wire.Weights) switch {
                (double[] wx, double[] wy, double[] wz, double[] w) =>
                    Fin.Succ<NurbsForm>(new NurbsForm.Curve(knots, wx, wy, wz, w, wire.Origin)),
            });

    static Fin<NurbsForm> AdmitSurface(NurbsWire.Surface wire) =>
        (KnotVector.Of(wire.DegreeU, [.. wire.KnotsU]).ToValidation(),
         KnotVector.Of(wire.DegreeV, [.. wire.KnotsV]).ToValidation(),
         WeightsPositive(wire.Weights, ParametricCarrier.Surface).ToValidation(),
         PointsFinite(wire.Grid, ParametricCarrier.Surface).ToValidation())
        .Apply(static (u, v, _, _) => (U: u, V: v)).As().ToFin()
        .Bind(axes => axes.U.ControlCount != wire.CountU
                || wire.Grid.Count != axes.U.ControlCount * axes.V.ControlCount
                || wire.Weights.Count != wire.Grid.Count
            ? Construction<NurbsForm>(ParametricCarrier.Surface, "grid extent disagrees with the knot vectors")
            : Homogenize(wire.Grid, wire.Weights) switch {
                (double[] wx, double[] wy, double[] wz, double[] w) =>
                    Fin.Succ<NurbsForm>(new NurbsForm.Surface(axes.U, axes.V, wx, wy, wz, w, wire.Origin)),
            });

    static Fin<Arr<double>> WeightsPositive(Arr<double> weights, ParametricCarrier carrier) =>
        weights.Exists(static w => !ValidityClaim.Positive(value: w))
            ? Construction<Arr<double>>(carrier, "non-positive weight")
            : Fin.Succ(weights);

    static Fin<Arr<Point3d>> PointsFinite(Arr<Point3d> points, ParametricCarrier carrier) =>
        points.Exists(static p => !ValidityClaim.Finite(value: p))
            ? Construction<Arr<Point3d>>(carrier, "non-finite control point")
            : Fin.Succ(points);

    // ONE homogenization for both carriers — the deleted Freeze/FreezeSurface pair was byte-identical and
    // differed only in which case took the four columns.
    static (double[] WX, double[] WY, double[] WZ, double[] W) Homogenize(Arr<Point3d> points, Arr<double> weights) {
        int n = points.Count;
        (double[] wx, double[] wy, double[] wz, double[] w) = (new double[n], new double[n], new double[n], new double[n]);
        for (int i = 0; i < n; i++) {
            (wx[i], wy[i], wz[i], w[i]) = (weights[i] * points[i].X, weights[i] * points[i].Y, weights[i] * points[i].Z, weights[i]);
        }
        return (wx, wy, wz, w);
    }

    // --- [FITTING]
    // Piegl-Tiller: parameterize through policy.Rule.Metric, average knots, hand the assembled SparseMatrix
    // to policy.Fit.Solve — one basis assembly; the surface lane runs the same column twice, rows then columns.
    static Fin<NurbsForm> FitCurve(Arr<Point3d> samples, SplinePolicy policy, Op key);
    static Fin<NurbsForm> FitSurface(int countU, Arr<Point3d> samples, SplinePolicy policy, Op key);

    // --- [CONSTRUCTIVE]
    // Ruled (A10.1): degrees unify through ElevateDegree, knot vectors merge through Refine on both
    // rails, then the loft is the degree-1 V-direction net over the paired control rows. Revolved
    // (A8.1): exact rational arcs per profile control point — full turn is the nine-point double-knot
    // circle, a partial sweep the minimal arc set — so the revolution is algebraically exact, never
    // sampled. Both freeze through Homogenize under the same weight/validity gates.
    static Fin<NurbsForm> AdmitRuled(NurbsForm.Curve rail, NurbsForm.Curve opposite, Op key);
    static Fin<NurbsForm> AdmitRevolved(NurbsForm.Curve profile, Line axis, double angleRadians, Op key);

    internal static Fin<double[]> ParameterizeSamples(Arr<Point3d> samples, ChordRule rule, Op key);              // A9.3 over rule.Metric
    internal static Fin<KnotVector> AveragedKnots(double[] parameters, int degree, int controlCount, Op key);     // A9.1 averaging

    static Fin<T> Construction<T>(ParametricCarrier carrier, string witness) =>
        Fin.Fail<T>(new GeometryFault.ParametricFault(ParametricStage.Construction, carrier, witness));
}

// --- [KERNELS] ------------------------------------------------------------------------------
internal static class NurbsKernel {
    internal static void BasisFunctions(in KnotVector knots, int span, double t, Span<double> basis);                        // A2.2
    internal static void DersBasisFunctions(in KnotVector knots, int span, double t, int order, Span<double> ders);          // A2.3
    internal static Point3d CurvePoint(NurbsForm.Curve curve, double t);                                                     // A3.1 + A4.1
    internal static (Point3d Point, Vector3d[] Ders) CurveRationalDerivatives(NurbsForm.Curve curve, double t, int order);   // A4.2 Leibniz
    internal static Point3d SurfacePoint(NurbsForm.Surface surface, double u, double v);                                     // A3.5 + A4.3
    internal static Vector3d[][] SurfaceRationalDerivatives(NurbsForm.Surface surface, double u, double v, int order);       // A3.6 + A4.4, #234-fixed [k][l]
    internal static NurbsForm.Curve InsertKnot(NurbsForm.Curve curve, double t, int multiplicity);                           // A5.1 Boehm
    internal static NurbsForm.Curve[] BezierSegments(NurbsForm.Curve curve);                                                 // A5.6 full-multiplicity decomposition
    internal static NurbsForm.Curve Elevate(NurbsForm.Curve curve, int target);                                              // A5.9

    // Table accumulates at 106 bits: sequential double prefix sums of n same-sign terms lose ~log2(n)
    // bits, so a 500-span curve cannot hold LengthTolerance in double and InvertLength would invert against
    // a table whose error grows monotonically toward the curve end. The value narrows to double only where
    // a public signature crosses.
    internal static Fin<ddouble[]> CumulativeLengths(NurbsForm.Curve curve, NurbsPolicy policy, Op key) {
        // GaussLegendre carries no error estimate, so the arc-length lane clears the witness gate explicitly
        // rather than reporting a convergence it never measured.
        QuadratureControl control = QuadratureControl.Default with { LegendreOrder = policy.GaussOrder.Value, RequireErrorWitness = false };
        return toSeq(BezierSegments(curve))
            .TraverseM(segment => Quadrature.Integrate(
                new IntegrationDomain.Line(
                    F: t => CurveRationalDerivatives(segment, t, 1).Ders[0].Length,
                    Bounds: new IntervalSpec(Lower: 0.0, Upper: 1.0),
                    Route: QuadratureRoute.GaussLegendre),
                control: control, key: key)).As()
            .Map(static evidence => {
                ddouble[] cumulative = new ddouble[evidence.Count + 1];
                int s = 0;
                foreach (QuadratureEvidence leg in evidence) { cumulative[s + 1] = cumulative[s] + (ddouble)leg.Value; s++; }
                return cumulative;
            });
    }

    // The table is a PREFIX sum with cumulative[0] = 0, so Σ(cᵢ − cᵢ₋₁) ≡ c_last identically — the deleted fold
    // paid n compensated subtractions and additions to recover a value already in the array, re-introducing at
    // every difference the rounding the ddouble table exists to avoid. CumulativeLengths' Count + 1 sizing
    // makes the empty case unreachable, and the guard states that rather than assuming it.
    internal static double TotalLength(ReadOnlySpan<ddouble> cumulative) =>
        cumulative.IsEmpty ? 0.0 : (double)cumulative[^1];

    internal static Fin<double> InvertLength(NurbsForm.Curve curve, ddouble[] cumulative, double target, NurbsPolicy policy) {
        int at = Array.BinarySearch(cumulative, (ddouble)target);
        (double lo, double hi) = SegmentDomain(curve, int.Clamp(at >= 0 ? at : ~at - 1, 0, cumulative.Length - 2));
        return Brent.TryFindRoot(
                t => LengthTo(curve, cumulative, t, policy) - target, lo, hi,
                policy.LengthTolerance, policy.ProjectIterations.Value, out double root)
            ? Fin.Succ(root)
            : Fin.Fail<double>(new GeometryFault.ParametricFault(ParametricStage.Station, ParametricCarrier.Curve, $"length inversion unconverged at {target}"));
    }

    // TryFindRoot is the no-throw twin InvertLength already rides, so budget exhaustion lowers a TYPED Evaluation
    // fault naming the unconverged probe instead of collapsing into whatever Op.Catch mints for a
    // NonConvergenceException; key.Catch stays around the derivative evaluations the objective and slope perform.
    internal static Fin<double> NewtonProject(NurbsForm.Curve curve, Point3d probe, double seedLo, double seedHi, NurbsPolicy policy, Op key) =>
        key.Catch(() => Fin.Succ(RobustNewtonRaphson.TryFindRoot(
                t => ProjectionObjective(curve, probe, t),
                t => ProjectionSlope(curve, probe, t),
                seedLo, seedHi, policy.ProjectTolerance, policy.ProjectIterations.Value, policy.ProjectSubdivision.Value,
                out double root)
            ? Some(root)
            : Option<double>.None))
        .Bind(root => root.ToFin(Fail: new GeometryFault.ParametricFault(
            ParametricStage.Evaluation, ParametricCarrier.Curve, $"newton projection unconverged at {probe}")));

    // g(t) = (C−P)·C′ and g′(t) = |C′|² + (C−P)·C″ — two NAMED members, never one `int form` knob whose call
    // sites passed the bare literals 1 and 2 and whose type said nothing about which body either selected.
    internal static double ProjectionObjective(NurbsForm.Curve curve, Point3d probe, double t) =>
        Jet(curve, t) switch { var jet => (jet.Point - probe) * jet.Ders[0] };
    internal static double ProjectionSlope(NurbsForm.Curve curve, Point3d probe, double t) =>
        Jet(curve, t) switch { var jet => (jet.Ders[0] * jet.Ders[0]) + ((jet.Point - probe) * jet.Ders[1]) };

    static (Point3d Point, Vector3d[] Ders) Jet(NurbsForm.Curve curve, double t) => CurveRationalDerivatives(curve, t, 2);

    internal static double LengthTo(NurbsForm.Curve curve, ddouble[] cumulative, double t, NurbsPolicy policy);
    internal static (double Lo, double Hi) SegmentDomain(NurbsForm.Curve curve, int segment);

    // Wang-2008 double reflection with the coincident-sample guard; the terminal defect leaves through
    // policy.Closure.Twist, so a new closure law is one FrameClosure row and no body changes.
    internal static Fin<Plane[]> DoubleReflectionFrames(NurbsForm.Curve curve, ReadOnlySpan<double> parameters, NurbsPolicy policy);
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: NURBS engine admission and evaluation flow
    accDescr: NurbsWire shapes fold through Nurbs.Of into NurbsForm carriers; De Boor kernels serve evaluation, quadrature rides the kernel Quadrature owner, inversion rides MathNet, and identity leaves through EncodeForm.
    Wire["NurbsWire — curve · surface · curve-through · surface-through · ruled · revolved"] -->|"Nurbs.Of — ONE Switch"| NurbsForm
    Wire -->|"SplineFit.Solve column"| Matrix["matrix.md SparseMatrix · CholeskySparse"]
    NurbsForm -->|"De Boor SoA kernels A2–A5"| NurbsKernel
    NurbsKernel -->|"per-Bezier speed integrand"| Quadrature["Quadrature.Integrate — Line · GaussLegendre route"]
    NurbsKernel -->|"monotone length inversion"| Brent["Brent.TryFindRoot"]
    NurbsKernel -->|"Newton projection — typed exhaustion fault"| Newton["RobustNewtonRaphson.TryFindRoot"]
    NurbsForm -->|"RationalDerivatives #234-fixed"| Forms["fundamental forms · curvature"]
    NurbsForm -->|"ToEncodeForm — normalized bytes"| EncodeForm["reconciliation EncodeForm.Parametric"]
    NurbsForm -->|"evaluation members"| Consumers["curve.md · surface.md · develop.md rails"]
    Wire -.->|"ParametricFault — Construction / Evaluation / Station"| GeometryFault
```

## [03]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or member on the owning carrier, never a sibling surface.

| [INDEX] | [AXIS_CONCERN] | [OWNER]                          | [RAIL]                      | [CASES] |
| :-----: | :------------- | :------------------------------- | :-------------------------- | :-----: |
|  [01]   | Admission      | `NurbsWire` + `Nurbs`            | `Nurbs.Of → Fin<NurbsForm>` |    6    |
|  [02]   | Carrier        | `NurbsForm`                      | member `Fin` rails          |    2    |
|  [03]   | Knot algebra   | `KnotVector`                     | `Of → Fin<KnotVector>`      |    —    |
|  [04]   | Engine knobs   | `NurbsPolicy`                    | `IValidityEvidence`         |    —    |
|  [05]   | Fitting rows   | `SplineFit` + `SplinePolicy`     | `Solve` column              |    2    |
|  [06]   | Vocabularies   | `ParametricDirection`/`KnotForm` | discriminants               |   2·2   |
|  [07]   | Behavior rows  | `ChordRule`/`FrameClosure`       | delegate columns            |   3·2   |

- [01]-[ADMISSION]: `[Union]` wire shapes folded by ONE `Of` (`MODAL_ARITY` — fitting is policy data).
- [02]-[CARRIER]: `[Union]` `Curve`/`Surface` over homogeneous SoA columns, evaluation members ON the cases.
- [03]-[KNOT_ALGEBRA]: normalized clamped-or-periodic vector, form-dispatched span search, and merge, three wire spellings admitted at one seam.
- [04]-[ENGINE_KNOBS]: `Dimension` orders and budgets, `EpsilonPolicy`-anchored tolerances, `Of(context)` the lane derivation.
- [05]-[FITTING_ROWS]: interpolate/approximate rows carrying their own linear system; degree, parameterization, tangents, and control budget are `SplinePolicy` columns.
- [06]-[VOCABULARIES]: `[SmartEnum]` U/V axis rows, clamped/periodic origin rows.
- [07]-[BEHAVIOR_ROWS]: A9.3 parameterization metrics and the frame-defect twist law as `[UseDelegateFromConstructor]` columns — the two deleted bools.

`NurbsKernel` signatures are the owned textbook-arithmetic transcription targets; the page's own bodies are the composed library seams — that split between owned arithmetic and composed depth is the density law.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
