# [RASM_NUMERICS_INTEGRATE]

`Rasm.Numerics` integration is the ODE/Runge-Kutta floor — pure numerics, zero geometric content. Every Butcher tableau admits by VALIDATING its order conditions numerically rather than asserting them, so a mis-transcribed coefficient is a construction-time typed failure carrying the failed-condition census, never a silently wrong trajectory; one carrier-generic `Step` serves scalar, vector, and geometric state, and continuous dense output localizes events without re-tracing. Geometry enters only at the `Processing/flow` consumer that supplies the spatial module; this page never names a geometric type.

`IntegrationModule.Combine` is the corpus' single linear-combination site; each `RungeKuttaMethod` row derives its `DenseConditions` and theta-independent correction basis once at static construction, so the moment-fit least-squares never runs inside the step loop — the correction is linear in a right-hand side that is itself a polynomial in theta, which is what makes the solve hoistable at all. `Step` is a PURE step function returning one closed `IntegrationStep` Accepted or Rejected: the kernel owns no reject loop, no run-level terminal partition, no step-underflow floor, and no total-step budget — each is the driver's, and the boundary states itself at both ends (`Rasm.Compute/Tensor/quadrature.md` `[03]-[TRAJECTORY_DRIVER]` `TrajectoryPhase`, `Processing/flow.md` `FlowKernel.Trace`), so a driver distinguishes budget exhaustion from underflow from a refused field where a kernel-side loop hands every one of them back as the same best-so-far. Dense continuous-extension fallback solves every moment basis column together through MathNet's multi-right-hand-side LU and thin-QR surfaces; every moment sum accumulates in 106-bit `ddouble` AND raises its terms there — the elementary weight folds through a `ddouble` coupling matrix and each residual raises through `ddouble.Pow` inside its 106-bit fold — so the claim rests on the summands, not on an accumulator widened after they already rounded.

## [01]-[INDEX]

- [02]-[TABLEAU_VOCABULARY]: `RungeKuttaMethod` tableau rows, the `RootedTree` Butcher-tree order algebra, and the order-condition-validated `ButcherTableau` carrier with `OrderConditions`.
- [03]-[DENSE_OUTPUT]: exact-rational continuous extensions carried by their method rows, the `DenseOutput` derivation, and the direct MathNet moment-fit fallback.
- [04]-[STEPPER]: carrier-generic `IntegrationModule` step algebra, the one `StepControl` adaptive-control policy row, and the `RungeKuttaIntegrator` record carrying optional adaptive `Policy`.
- [05]-[QUADRATURE]: accuracy-routed `QuadratureRoute` rows over the `QuadratureDomain` arity union, the `ReferenceElement` row family with its owned Gauss tables and typed ladder-exhaustion refusal, and the finite-guard-then-admit `Quadrature.Integrate` entry with `QuadratureEvidence`, whose `Estimate` presence is the convergence witness.
- [06]-[DENSITY_BAR]: one owner per axis across both bands.

## [02]-[TABLEAU_VOCABULARY]

- Owner: `RungeKuttaMethod` mints the `[SmartEnum<string>]` whose canonical keys identify rows that ARE admitted Butcher tableaux, each a single declaration through the ONE private `Of` factory whose one optional embedded `(Weights, Order)` pair discriminates fixed from embedded — the two halves share one lifecycle, so a weight row without its order is unrepresentable — and whose optional continuous-extension coefficients, interpolant, and `DenseConditions` stay on that same row; `RootedTree` is the Butcher-tree algebra — order the node count, density `γ(t) = |t|·Πγ(children)`, elementary weight `Φᵢ(t)` off the coupling matrix — whose condition set `Σᵢ bᵢΦᵢ(t) = 1/γ(t)` over every tree of order ≤ p IS the order-p proof, its pool memoized to the tableau's private `OrderCeiling`; nested `OrderConditions` carries the walk's tally beside `VerifiedOrder` — the largest certified prefix, DERIVED inside the same 106-bit walk, never a second lower-precision pass.
- Cases: `Euler` · `Heun` · `Midpoint` · `Ralston` · `RK4` · `RK38` fixed; `BogackiShampine` · `CashKarp` · `DormandPrince` embedded-adaptive. Rows stay DISTINCT-BY-DESIGN over the closed Butcher-tableau literature — the upstream is the published tableau of each named method (Butcher's classical fourth-order pair and the 3/8 rule; Bogacki-Shampine 3(2); Cash-Karp 5(4); Dormand-Prince 5(4)) — and each row carries its coupling matrix, weight row, and embedded weight row as DATA, so no row re-derives a coefficient in a body and the order/stage witness folds from that data through the tree walk rather than a declared number.
- Entry: `RungeKuttaMethod.<Row>.Tableau` reads the validated carrier; the private `Of` gate admits static tableau data once and derives dense output before the row becomes available.
- Auto: abscissae never enter as data — the factories derive them as coupling row sums, so the consistency condition holds by construction and the validity fold re-checks it as the transcription witness; the adaptive step reads its exponent `1/(q+1)` off the embedded pair's order only after the adaptive factory has proved the pair present, so no fixed row silently inherits Dormand-Prince's 0.2 and no exponent falls back to `1.0`; the tree pool per order generates once behind an accessor-forced lazy, so a fifth-order pair proves seventeen conditions with zero condition code and no re-derivation per level; order conditions, continuous extension, interpolant, and dense evidence derive at ROW construction, so one admission runs the tree walk once.
- Law: `Admit` returns an order-carrying tableau or a TYPED structural fault that carries the walk's own census — failed and checked condition counts, the max residual, and the derived `VerifiedOrder` beside the declared `MethodOrder` — so a mis-transcribed coefficient names which claim broke instead of collapsing to a bare invalid-input token.
- Law: `OrderConditions` folds its validity under the semantic gate `FailedConditionCount == 0 && MaxResidual <= CoefficientTolerance`; `CheckedConditionCount` is the tree census at the declared order, so an order-5 row proves every one of its seventeen elementary-weight conditions, never the first four moments alone.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Seq`, `Option`, `Fin`), TYoshimura.DoubleDouble (`ddouble` 106-bit moment accumulation).
- Growth: a new integrator is ONE `RungeKuttaMethod` row through the one `Of` factory, the one optional embedded pair discriminating fixed from embedded — the rooted-tree walk generates every order condition up to `ButcherTableau.OrderCeiling`, so a higher-order tableau validates with zero new condition code; a per-order condition roster and a second factory twin are the deleted forms.
- Boundary: `CoefficientTolerance` is the tableau's own order-condition RESIDUAL band and `ThetaEndpointBand` the separate parameter-domain band the interpolant reads, because one constant serving two unrelated concepts let a change to the transcription band move where an interpolant thinks its endpoints are; the residual band stays a row on the carrier that owns it — exact-rational coefficients evaluate near machine epsilon, so the band catches transcription errors rather than roundoff, and seating it as an `EpsilonPolicy` row or a `ToleranceLane` puts a coefficient-transcription band in the geometry epsilon vocabulary this page carries no `Context` to read; tableau data lives ONLY on the vocabulary rows, and a consumer never spells a coupling coefficient; the recursive tree enumeration and elementary-weight loops are the named statement kernel.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DoubleDouble;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class RungeKuttaMethod {
    public static readonly RungeKuttaMethod Euler = Of(key: "euler", order: 1, coupling: [[]], weights: [1.0]);
    public static readonly RungeKuttaMethod Heun = Of(key: "heun", order: 2, coupling: [[], [1.0]], weights: [0.5, 0.5]);
    public static readonly RungeKuttaMethod Midpoint = Of(key: "midpoint", order: 2, coupling: [[], [0.5]], weights: [0.0, 1.0]);
    public static readonly RungeKuttaMethod Ralston = Of(key: "ralston", order: 2, coupling: [[], [2.0 / 3.0]], weights: [0.25, 0.75]);
    public static readonly RungeKuttaMethod RK4 = Of(key: "rk4", order: 4,
        coupling: [[], [0.5], [0.0, 0.5], [0.0, 0.0, 1.0]],
        weights: [1.0 / 6.0, 1.0 / 3.0, 1.0 / 3.0, 1.0 / 6.0]);
    public static readonly RungeKuttaMethod RK38 = Of(key: "rk38", order: 4,
        coupling: [[], [1.0 / 3.0], [-1.0 / 3.0, 1.0], [1.0, -1.0, 1.0]],
        weights: [1.0 / 8.0, 3.0 / 8.0, 3.0 / 8.0, 1.0 / 8.0]);
    public static readonly RungeKuttaMethod BogackiShampine = Of(key: "bogacki-shampine", order: 3,
        coupling: [[], [0.5], [0.0, 0.75], [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0]],
        weights: [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0, 0.0],
        embedded: Some<(double[] Weights, int Order)>((Weights: [7.0 / 24.0, 0.25, 1.0 / 3.0, 1.0 / 8.0], Order: 2)),
        continuousExtension: Some<(int Order, double[][] Coefficients)>((Order: 3, Coefficients: [
            [1.0, -4.0 / 3.0, 5.0 / 9.0],
            [0.0, 1.0, -2.0 / 3.0],
            [0.0, 4.0 / 3.0, -8.0 / 9.0],
            [0.0, -1.0, 1.0]])));
    public static readonly RungeKuttaMethod CashKarp = Of(key: "cash-karp", order: 5,
        coupling: [[], [0.2], [3.0 / 40.0, 9.0 / 40.0], [0.3, -0.9, 1.2],
            [-11.0 / 54.0, 2.5, -70.0 / 27.0, 35.0 / 27.0],
            [1631.0 / 55296.0, 175.0 / 512.0, 575.0 / 13824.0, 44275.0 / 110592.0, 253.0 / 4096.0]],
        weights: [37.0 / 378.0, 0.0, 250.0 / 621.0, 125.0 / 594.0, 0.0, 512.0 / 1771.0],
        embedded: Some<(double[] Weights, int Order)>((Weights: [2825.0 / 27648.0, 0.0, 18575.0 / 48384.0, 13525.0 / 55296.0, 277.0 / 14336.0, 0.25], Order: 4)));
    public static readonly RungeKuttaMethod DormandPrince = Of(key: "dormand-prince", order: 5,
        coupling: [[], [1.0 / 5.0], [3.0 / 40.0, 9.0 / 40.0],
            [44.0 / 45.0, -56.0 / 15.0, 32.0 / 9.0],
            [19372.0 / 6561.0, -25360.0 / 2187.0, 64448.0 / 6561.0, -212.0 / 729.0],
            [9017.0 / 3168.0, -355.0 / 33.0, 46732.0 / 5247.0, 49.0 / 176.0, -5103.0 / 18656.0],
            [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0]],
        weights: [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0, 0.0],
        embedded: Some<(double[] Weights, int Order)>((Weights: [5179.0 / 57600.0, 0.0, 7571.0 / 16695.0, 393.0 / 640.0, -92097.0 / 339200.0, 187.0 / 2100.0, 1.0 / 40.0], Order: 4)),
        continuousExtension: Some<(int Order, double[][] Coefficients)>((Order: 4, Coefficients: [
            [1.0, -8048581381.0 / 2820520608.0, 8663915743.0 / 2820520608.0, -12715105075.0 / 11282082432.0],
            [0.0, 0.0, 0.0, 0.0],
            [0.0, 131558114200.0 / 32700410799.0, -68118460800.0 / 10900136933.0, 87487479700.0 / 32700410799.0],
            [0.0, -1754552775.0 / 470086768.0, 14199869525.0 / 1410260304.0, -10690763975.0 / 1880347072.0],
            [0.0, 127303824393.0 / 49829197408.0, -318862633887.0 / 49829197408.0, 701980252875.0 / 199316789632.0],
            [0.0, -282668133.0 / 205662961.0, 2019193451.0 / 616988883.0, -1453857185.0 / 822651844.0],
            [0.0, 40617522.0 / 29380423.0, -110615467.0 / 29380423.0, 69997945.0 / 29380423.0]])));
    internal ButcherTableau Tableau { get; }
    internal Option<(int Order, double[][] Coefficients)> ContinuousExtension { get; }
    internal DenseConditions Dense { get; }
    internal DenseOutput.DenseInterpolant Interpolant { get; }
    private static RungeKuttaMethod Of(
        string key, int order, double[][] coupling, double[] weights,
        Option<(double[] Weights, int Order)> embedded = default,
        Option<(int Order, double[][] Coefficients)> continuousExtension = default) {
        ButcherTableau candidate = ButcherTableau.Of(
            toSeq(coupling.Select(static row => toSeq(row))), toSeq(weights),
            embedded.Map(static pair => (toSeq(pair.Weights), pair.Order)), order);
        ButcherTableau tableau = candidate
            .Admit(candidate.ConditionsOf(candidate.Weights, order))
            .ThrowIfFail();
        DenseOutput.DenseInterpolant interpolant = DenseOutput.Interpolant(tableau, continuousExtension).ThrowIfFail();
        DenseConditions dense = DenseOutput.Conditions(tableau, continuousExtension, interpolant).ThrowIfFail();
        return new(key: key, tableau: tableau, continuousExtension: continuousExtension,
            dense: dense, interpolant: interpolant);
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
internal readonly record struct ButcherTableau {
    private ButcherTableau(
        Seq<Seq<double>> coupling, Seq<double> abscissae, Seq<double> weights,
        Option<(Seq<double> Weights, int Order)> embedded, int methodOrder) =>
        (Coupling, Abscissae, Weights, Embedded, MethodOrder) =
            (coupling, abscissae, weights, embedded, methodOrder);
    public Seq<Seq<double>> Coupling { get; }
    public Seq<double> Abscissae { get; }
    public Seq<double> Weights { get; }
    internal Option<(Seq<double> Weights, int Order)> Embedded { get; }
    public int MethodOrder { get; }

    internal static ButcherTableau Of(Seq<Seq<double>> coupling, Seq<double> weights, Option<(Seq<double> Weights, int Order)> embedded, int order) =>
        new(coupling, coupling.Map(static row => row.Fold(0.0, static (sum, value) => sum + value)), weights, embedded, order);

    internal const double CoefficientTolerance = 1.0e-9;
    internal const double ThetaEndpointBand = EpsilonPolicy.ZeroTolerance;
    private const int OrderCeiling = 10;
    internal int StageCount => Weights.Count;
    internal bool Valid(OrderConditions conditions) => ValidityClaim.All(
        StageCount > 0,
        MethodOrder is > 0 and <= OrderCeiling,
        Coupling.Count == StageCount,
        Abscissae.Count == StageCount,
        Abscissae.ForAll(double.IsFinite),
        CoefficientsMatch(values: Weights, expected: 1.0),
        conditions.IsValid,
        Coupling.Zip(Abscissae).AsIterable().Select((pair, index) => pair.First.Count <= index
            && CoefficientsMatch(values: pair.First, expected: pair.Second)).All(static ok => ok),
        Embedded.Map(pair => pair.Order is > 0 and < MethodOrder
            && pair.Order <= OrderCeiling
            && pair.Weights.Count == StageCount
            && CoefficientsMatch(pair.Weights, 1.0)
            && ConditionsOf(pair.Weights, pair.Order).IsValid).IfNone(true));
    internal Fin<ButcherTableau> Admit(OrderConditions conditions) =>
        Valid(conditions)
            ? Fin.Succ(this)
            : Fin.Fail<ButcherTableau>(new KernelFault.InvalidValue(
                Label: $"butcher-tableau:stages={StageCount}:order={MethodOrder}",
                Requirement: $"every order condition within {CoefficientTolerance:e1} — failed {conditions.FailedConditionCount} of {conditions.CheckedConditionCount}, max residual {conditions.MaxResidual:e3}, verified order {conditions.VerifiedOrder}"));
    private static bool CoefficientsMatch(Seq<double> values, double expected) =>
        values.ForAll(double.IsFinite)
        && Math.Abs(value: values.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - expected) <= CoefficientTolerance;
    internal OrderConditions ConditionsOf(Seq<double> weights, int order) {
        ddouble[,] aWide = new ddouble[StageCount, StageCount];
        int row = 0;
        foreach (Seq<double> coupling in Coupling) {
            int column = 0;
            foreach (double coefficient in coupling) aWide[row, column++] = coefficient;
            row++;
        }
        double[] b = [.. weights];
        (int Count, int Failed, double Max, int Verified) state = (0, 0, 0.0, 0);
        for (int p = 1; p <= order; p++) {
            int failedBefore = state.Failed;
            foreach (RootedTree tree in RootedTree.OfOrder(p)) {
                ddouble[] phi = tree.Weight(aWide, StageCount);
                ddouble lhs = 0.0;
                for (int i = 0; i < StageCount; i++) lhs += b[i] * phi[i];
                double residual = (double)ddouble.Abs(lhs - ((ddouble)1.0 / tree.Density));
                state = (state.Count + 1,
                    state.Failed + (double.IsFinite(residual) && residual <= CoefficientTolerance ? 0 : 1),
                    Math.Max(state.Max, residual), state.Verified);
            }
            if (state.Failed == failedBefore && state.Verified == p - 1) state.Verified = p;
        }
        return new(StageCount, order, state.Verified, state.Count, state.Failed, state.Max);
    }

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct OrderConditions(
        int StageCount, int MethodOrder, int VerifiedOrder,
        int CheckedConditionCount, int FailedConditionCount, double MaxResidual) : IValidityEvidence {
        public bool IsValid => ValidityClaim.All(
            StageCount >= 1 && MethodOrder >= 1 && VerifiedOrder == MethodOrder && CheckedConditionCount >= 0,
            ValidityClaim.CountExactly(FailedConditionCount, 0),
            ValidityClaim.Nonnegative(MaxResidual),
            MaxResidual <= ButcherTableau.CoefficientTolerance);
    }

    private sealed record RootedTree {
        public RootedTree(ImmutableArray<RootedTree> children) {
            Children = children;
            Order = 1 + children.Sum(static child => child.Order);
            Density = Order * children.Aggregate(seed: 1.0, func: static (acc, child) => acc * child.Density);
        }
        public static readonly RootedTree Leaf = new(ImmutableArray<RootedTree>.Empty);

        public ImmutableArray<RootedTree> Children { get; }
        public int Order { get; }
        public double Density { get; }

        public ddouble[] Weight(ddouble[,] a, int stages) {
            ddouble[] phi = new ddouble[stages];
            System.Array.Fill(array: phi, value: (ddouble)1.0);
            foreach (RootedTree child in Children) {
                ddouble[] childWeight = child.Weight(a: a, stages: stages);
                for (int i = 0; i < stages; i++) {
                    ddouble g = (ddouble)0.0;
                    for (int j = 0; j < stages; j++) { g += a[i, j] * childWeight[j]; }
                    phi[i] *= g;
                }
            }
            return phi;
        }

        public static ImmutableArray<RootedTree> OfOrder(int order) =>
            order <= 1 ? [Leaf] : Pool.Value.TryGetValue(key: order, value: out ImmutableArray<RootedTree> held) ? held : [];

        private static readonly Lazy<FrozenDictionary<int, ImmutableArray<RootedTree>>> Pool =
            new(Generate, LazyThreadSafetyMode.ExecutionAndPublication);

        private static FrozenDictionary<int, ImmutableArray<RootedTree>> Generate() {
            Dictionary<int, ImmutableArray<RootedTree>> built = new() { [1] = [Leaf] };
            for (int order = 2; order <= OrderCeiling; order++) {
                ImmutableArray<RootedTree> pool = [.. Enumerable.Range(start: 1, count: order - 1).SelectMany(lower => built[lower])];
                built[order] = [.. Forests(pool: pool, remaining: order - 1, start: 0).Select(static forest => new RootedTree(children: forest))];
            }
            return built.ToFrozenDictionary();
        }

        private static IEnumerable<ImmutableArray<RootedTree>> Forests(ImmutableArray<RootedTree> pool, int remaining, int start) =>
            remaining == 0
                ? [ImmutableArray<RootedTree>.Empty]
                : Enumerable.Range(start: start, count: pool.Length - start)
                    .Where(index => pool[index].Order <= remaining)
                    .SelectMany(index => Forests(pool: pool, remaining: remaining - pool[index].Order, start: index).Select(rest => rest.Insert(index: 0, item: pool[index])));
    }
}
```

## [03]-[DENSE_OUTPUT]

- Owner: each `RungeKuttaMethod` row carries `Option<(Order, Coefficients)>` for its published continuous extension — `Some` on Bogacki-Shampine and Dormand-Prince, `None` on the generic moment-fit rows; `DenseOutput` derives the interpolant and proves it ONCE at row construction through independent moment probes at θ ∈ {0, ½, 1} and endpoint evidence.
- Cases: the method row's continuous-extension option selects a published exact-rational table or the generic moment-fit fallback; no second identity vocabulary exists.
- Entry: `DenseOutput.Interpolant`, `Conditions`, and `Weights` take the tableau and its row-owned continuous extension; a span reads those already-derived values from its method.
- Auto: the generic route pins the endpoints exactly and fits only the interior through the `θ(1−θ)`-scaled correction, so endpoint continuity is structural; one Vandermonde LU solves every anchor preimage and one thin QR solves every correction column together, leaving `DenseInterpolant` with only its order and basis columns; `DistinctAnchors` is the ONE distinct-abscissa derivation and caps the generic dense order at the Vandermonde rank ceiling.
- Law: `DenseConditions` carries only irreducible `DenseOrder` and `MaxResidual`; failed-condition rejection remains inside `Conditions`, while endpoint residuals fold immediately to `(Failed, MaxResidual)` and never escape as a carrier.
- Law: the four independent probes accumulate applicatively before one `Fin` egress, so later failures survive an earlier refusal; every moment target and subtraction remains in `ddouble` until the nonnegative residual narrows.
- Packages: LanguageExt.Core, MathNet.Numerics, TYoshimura.DoubleDouble.
- Growth: a new published interpolant is coefficient data on its `RungeKuttaMethod` row; a tableau without one automatically takes the generic multi-column moment fit.
- Boundary: interpolant tables are exact rationals spelled as ratios, never decimal approximations — the moment validation flags the drift; dense output is the event-localization substrate `Processing/flow` binds for root bisection, and a consumer interpolating trajectories by chord instead of `b(θ)` re-derives a capability this owner already proves.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DoubleDouble;
using LanguageExt;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DenseConditions(int DenseOrder, double MaxResidual) : IValidityEvidence {
    public bool IsValid => DenseOrder >= 0
        && double.IsFinite(MaxResidual)
        && MaxResidual is >= 0.0 and <= ButcherTableau.CoefficientTolerance;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DenseOutput {
    internal static Fin<DenseConditions> Conditions(ButcherTableau tableau, Option<(int Order, double[][] Coefficients)> extension, DenseInterpolant interpolant) {
        int order = DenseOrderFor(tableau, extension);
        return (
            ProbeAt(tableau, extension, interpolant, order, 0.0).ToValidation(),
            ProbeAt(tableau, extension, interpolant, order, 0.5).ToValidation(),
            ProbeAt(tableau, extension, interpolant, order, 1.0).ToValidation(),
            EndpointEvidence(tableau, extension).ToValidation())
            .Apply((zero, mid, one, endpoint) => (
                Failed: zero.Failed + mid.Failed + one.Failed + endpoint.Failed,
                MaxResidual: Math.Max(endpoint.MaxResidual,
                    Math.Max(zero.MaxResidual, Math.Max(mid.MaxResidual, one.MaxResidual)))))
            .As().ToFin()
            .Bind(result => {
                DenseConditions conditions = new(order, result.MaxResidual);
                return result.Failed == 0 && conditions.IsValid
                    ? Fin.Succ(conditions)
                    : Fin.Fail<DenseConditions>(new KernelFault.InvalidResult());
            });
    }
    private static int DenseOrderFor(ButcherTableau tableau, Option<(int Order, double[][] Coefficients)> extension) =>
        extension.Match(
            Some: static held => held.Order,
            None: () =>
                Math.Max(val1: 1, val2: Math.Min(val1: tableau.MethodOrder, val2: DistinctAnchors(tableau: tableau).Count)));
    private static Seq<int> DistinctAnchors(ButcherTableau tableau) =>
        tableau.Abscissae.AsIterable().Select((c, stage) => (Stage: stage, C: c)).Aggregate(
            seed: Seq<int>(),
            func: (anchors, row) => anchors.Exists(seen => Math.Abs(value: tableau.Abscissae[seen] - row.C) <= ButcherTableau.CoefficientTolerance)
                ? anchors
                : anchors.Add(row.Stage));
    private static Fin<(int Failed, double MaxResidual)> ProbeAt(ButcherTableau tableau, Option<(int Order, double[][] Coefficients)> extension, DenseInterpolant interpolant, int order, double theta) =>
        Weights(tableau, extension, interpolant, order, theta).Map(weights => {
            (bool failed, double maxResidual) = MomentResidual(tableau: tableau, weights: weights, theta: theta, order: order);
            double endpoint = theta <= ButcherTableau.ThetaEndpointBand
                ? weights.Fold(initialState: 0.0, f: static (max, value) => Math.Max(val1: max, val2: Math.Abs(value: value)))
                : 1.0 - theta <= ButcherTableau.ThetaEndpointBand
                    ? weights.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second)))
                    : 0.0;
            return (
                Failed: (failed ? 1 : 0) + (double.IsFinite(endpoint) && endpoint <= ButcherTableau.CoefficientTolerance ? 0 : 1),
                MaxResidual: Math.Max(val1: maxResidual, val2: endpoint));
        });
    private static Fin<(int Failed, double MaxResidual)> EndpointEvidence(ButcherTableau tableau, Option<(int Order, double[][] Coefficients)> extension) {
        static (int Failed, double MaxResidual) Fold(params double[] values) =>
            values.Select(static value => Math.Abs(value)).Aggregate(
                seed: (Failed: 0, MaxResidual: 0.0),
                func: static (state, residual) => (
                    state.Failed + (double.IsFinite(residual) && residual <= ButcherTableau.CoefficientTolerance ? 0 : 1),
                    Math.Max(state.MaxResidual, residual)));

        return extension.IsSome
            ? from atOne in Published(extension, 1.0, tableau.StageCount, Horner)
              from atZero in Published(extension, 0.0, tableau.StageCount, Horner)
              from derivOne in Published(extension, 1.0, tableau.StageCount, HornerDerivative)
              from derivZero in Published(extension, 0.0, tableau.StageCount, HornerDerivative)
              select Fold(
                  atZero.Fold(0.0, static (max, value) => Math.Max(max, Math.Abs(value))),
                  atOne.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - tableau.Weights.Fold(initialState: 0.0, f: static (sum, value) => sum + value),
                  MaxDeviation(values: derivZero, target: 0),
                  MaxDeviation(values: derivOne, target: tableau.StageCount - 1),
                  atOne.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second))))
            : Fin.Succ((Failed: 0, MaxResidual: 0.0));
    }
    private static double MaxDeviation(Seq<double> values, int target) =>
        values.AsIterable().Select((value, index) => Math.Abs(value: value - (index == target ? 1.0 : 0.0))).Aggregate(seed: 0.0, func: static (max, deviation) => Math.Max(val1: max, val2: deviation));
    internal static Fin<Seq<double>> Weights(ButcherTableau tableau, Option<(int Order, double[][] Coefficients)> extension, DenseInterpolant interpolant, int order, double theta) =>
        extension.IsSome
            ? Published(extension, theta, tableau.StageCount, Horner)
            : theta <= ButcherTableau.ThetaEndpointBand
                ? Fin.Succ(toSeq(Enumerable.Repeat(element: 0.0, count: tableau.StageCount)))
                : 1.0 - theta <= ButcherTableau.ThetaEndpointBand
                    ? Fin.Succ(tableau.Weights)
                    : Fin.Succ(toSeq(tableau.Weights.Map(weight => theta * weight)
                            .Zip(toSeq(interpolant.At(theta: theta, stages: tableau.StageCount)))
                            .Select(pair => pair.First + (theta * (1.0 - theta) * pair.Second))));
    internal readonly record struct DenseInterpolant(int Order, ImmutableArray<double[]> Basis) {
        internal double[] At(double theta, int stages) {
            double endpointScale = theta * (1.0 - theta);
            double[] correction = new double[stages];
            double power = 1.0;
            for (int m = 0; m < Order; m++) {
                power *= theta;
                double coefficient = (power - theta) / ((m + 1.0) * endpointScale);
                for (int stage = 0; stage < stages; stage++) { correction[stage] += coefficient * Basis[m][stage]; }
            }
            return correction;
        }
    }

    internal static Fin<DenseInterpolant> Interpolant(ButcherTableau tableau, Option<(int Order, double[][] Coefficients)> extension) {
        int order = DenseOrderFor(tableau, extension);
        if (extension.IsSome) return Fin.Succ(new DenseInterpolant(order, []));
        return Try.lift(() => {
            int stages = tableau.StageCount;
            int[] anchors = [.. DistinctAnchors(tableau).Take(order)];
            var design = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(
                stages, order, (stage, power) => Math.Pow(tableau.Abscissae[stage], power));
            var vandermonde = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(
                order, order, (power, column) => Math.Pow(tableau.Abscissae[anchors[column]], power));
            var identity = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseIdentity(order);
            var preimages = vandermonde.LU().Solve(identity);
            var samples = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(stages, order);
            for (int column = 0; column < order; column++)
                for (int row = 0; row < order; row++) samples[anchors[row], column] = preimages[row, column];
            var correction = design.Multiply(design.QR(
                MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Thin).Solve(samples));
            return new DenseInterpolant(order,
                [.. Enumerable.Range(0, order).Select(column => correction.Column(column).ToArray())]);
        }).Run();
    }
    private static (bool Failed, double Max) MomentResidual(
        ButcherTableau tableau, Seq<double> weights, double theta, int order) =>
        Enumerable.Range(0, order)
            .Select(moment => {
                ddouble actual = weights.Zip(tableau.Abscissae).Fold((ddouble)0.0,
                    (sum, pair) => sum + (pair.First * ddouble.Pow(pair.Second, moment)));
                return (double)ddouble.Abs(actual - (ddouble.Pow(theta, moment + 1) / (moment + 1)));
            })
            .Aggregate(seed: (Failed: false, Max: 0.0), func: static (state, residual) => (
                Failed: state.Failed || !double.IsFinite(residual) || residual > ButcherTableau.CoefficientTolerance,
                Max: Math.Max(state.Max, residual)));

    private static Fin<Seq<double>> Published(
        Option<(int Order, double[][] Coefficients)> extension,
        double theta,
        int stageCount,
        Func<double[], double, double> project) =>
        extension.Filter(held => held.Coefficients.Length == stageCount)
            .Map(held => Acceptance.Rows(values: held.Coefficients.Select(row => project(row, theta))))
            .IfNone(() => Fin.Fail<Seq<double>>(new KernelFault.InvalidInput()));

    private static double Horner(double[] row, double theta) {
        double accumulated = 0.0;
        for (int index = row.Length - 1; index >= 0; index--) accumulated = (accumulated * theta) + row[index];
        return theta * accumulated;
    }

    private static double HornerDerivative(double[] row, double theta) {
        double accumulated = 0.0;
        for (int index = row.Length - 1; index >= 0; index--) accumulated = (accumulated * theta) + ((index + 1) * row[index]);
        return accumulated;
    }
}
```

## [04]-[STEPPER]

- Owner: `IntegrationModule<TState, TDelta>` mints the additive-module policy record — the four operations one Runge-Kutta step needs and its `Zero` delta, carrying `Combine` as the corpus' single linear-combination fold and the `Scalar`/`ComplexScalar` canonical instances for `double` and `Complex` state; `StepControl` mints the ONE adaptive-control policy row — safety factor, step-ratio clamps, reject budget, and the keyless `StepController` row (`Integral` · `ProportionalIntegral` · `Gustafsson`, Hairer-Wanner's I, PI, and Gustafsson controllers) travelling together as its `Controller` column — whose `Rescale` reads the driver-threaded `Option<StepHistory>`; `RungeKuttaIntegrator` is one record carrying its method and optional adaptive `Policy`, and its generic `Step` computes stages and the primary combination once before policy-only error control; `IntegrationStep<TState, TDelta>` is the closed accepted-or-rejected outcome; `DenseOutputSpan<TState, TDelta>` stores the method and accepted finite stages and reconstructs any θ from `start + h·Σbᵢ(θ)kᵢ`.
- Cases: `RungeKuttaIntegrator.Policy` is `None` for fixed stepping or `Some(Tolerance, Control)` for adaptive stepping; `IntegrationStep` carries `AcceptedCase` and `RejectedCase`.
- Entry: `RungeKuttaIntegrator.Fixed` and `Adaptive` enforce method/policy agreement over the already-admitted static method row; `Step` takes the derivative field as a `sample` function, so the one stepper integrates a scalar ODE, a spatial streamline, or any admitted carrier. Required callers null-gate directly, and the optional extraction caller spells its RK4 default in place.
- Auto: stage computation is one fold over the coupling rows and refuses every non-finite stage norm at production; the adaptive policy binds the tableau's embedded formula, refuses a non-finite error before controller arithmetic, rescales against driver-threaded history at exponent `1/(q+1)`, and returns `RejectedCase` with the suggestion. Accepted spans construct directly because static method admission already proved endpoint and continuous-extension conditions.
- Law: `Step` is a PURE step function and the DRIVE is the consumer's. Kernel owns no reject loop, no run-level terminal partition, no step-underflow floor, and no total-step budget — a kernel-side loop collapses budget exhaustion, step underflow, and a non-finite error into one indistinguishable best-so-far, so each driver owns its closed continue-or-done fold over `IntegrationStep` and names its typed terminal (`Rasm.Compute/Tensor/quadrature.md` `TrajectoryPhase`/`TerminalDisposition.Relaxable(RelaxAxis.Steps)`, `Processing/flow.md` `FlowKernel.Trace` through `Range.FoldUntil` with `StreamlineStopKind.RejectBudgetExhausted`). `StepControl.RejectBudget` is the budget those drivers read; the kernel publishes it and never spends it.
- Exemption: the error-norm choice rides `IntegrationModule.Norm` rather than the control row — the norm is a property of the STATE CARRIER, so a large-magnitude slab supplies the scaled two-pass norm that doctrine's boundary calls for while `Scalar`/`ComplexScalar` supply the modulus; a norm column on `StepControl` lets one control row claim a norm its carrier cannot compute.
- Output: the dense span carries its `DenseConditions`; step error and the suggested next step ride the outcome cases, which is where a driver reads them — a rejection's error is mandatory because only the adaptive arm can reject, an acceptance's is `Some` exactly when the arm was adaptive, and the DRIVER mints, stores, and threads `StepHistory(Error, Scale)` from that error and the next step it actually selected after its own caps, so the stateless stepper returns error plus a suggestion and never run state.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core; zero geometry.
- Growth: a new state carrier is one `IntegrationModule` instance at its consumer; a new controller (PI, Gustafsson) is one `StepControl` field set — the stepper body never changes.
- Boundary: no state difference ever appears — only deltas subtract, so the module needs no `TState` subtraction, and the error is measured between the two weight combinations before adding to the state.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IntegrationModule<TState, TDelta>(
    Func<TState, double, TDelta, TState> Add,
    Func<double, TDelta, TDelta> Scale,
    Func<TDelta, TDelta, TDelta> Sum,
    Func<TDelta, double> Norm,
    TDelta Zero) {
    public TDelta Combine(Seq<double> coefficients, Seq<TDelta> deltas) =>
        coefficients.Zip(deltas).Fold(initialState: Zero, f: (sum, pair) => Sum(arg1: sum, arg2: Scale(arg1: pair.First, arg2: pair.Second)));
    public static IntegrationModule<double, double> Scalar { get; } = new(
        Add: static (state, h, delta) => state + (h * delta),
        Scale: static (factor, delta) => factor * delta,
        Sum: static (left, right) => left + right,
        Norm: Math.Abs,
        Zero: 0.0);

    public static IntegrationModule<Complex, Complex> ComplexScalar { get; } = new(
        Add: static (state, h, delta) => state + (h * delta),
        Scale: static (factor, delta) => factor * delta,
        Sum: static (left, right) => left + right,
        Norm: static delta => Complex.Abs(value: delta),
        Zero: Complex.Zero);
}

public readonly record struct StepHistory(double Error, double Scale);

[SmartEnum]
public sealed partial class StepController {
    public static readonly StepController Integral = new(
        rescale: static (control, history, error, tolerance, exponent) => control.Safety * Math.Pow(x: tolerance / error, y: exponent));
    public static readonly StepController ProportionalIntegral = new(
        rescale: static (control, history, error, tolerance, exponent) => control.Safety
            * Math.Pow(x: tolerance / error, y: 0.7 * exponent)
            * history.Map(previous => Math.Pow(x: previous.Error / tolerance, y: 0.4 * exponent)).IfNone(noneValue: 1.0));
    public static readonly StepController Gustafsson = new(
        rescale: static (control, history, error, tolerance, exponent) => control.Safety
            * Math.Pow(x: tolerance / error, y: exponent)
            * history.Map(previous => previous.Scale * Math.Pow(x: previous.Error / error, y: exponent)).IfNone(noneValue: 1.0));

    [UseDelegateFromConstructor] internal partial double Rescale(StepControl control, Option<StepHistory> history, double error, double tolerance, double exponent);
}

public sealed record StepControl(double Safety, double MinScale, double MaxScale, int RejectBudget, StepController Controller) : IValidityEvidence {
    public static readonly StepControl Default = new(Safety: 0.9, MinScale: 0.2, MaxScale: 10.0, RejectBudget: 3, Controller: StepController.Integral);
    public bool IsValid => ValidityClaim.All(
        double.IsFinite(Safety) && Safety > 0.0,
        double.IsFinite(MinScale) && double.IsFinite(MaxScale) && MinScale > 0.0 && MinScale <= MaxScale,
        RejectBudget >= 0,
        Controller is not null);
    internal double Rescale(Option<StepHistory> history, double error, double tolerance, double exponent) =>
        error > EpsilonPolicy.ZeroTolerance
            ? Math.Clamp(value: Controller.Rescale(control: this, history: history, error: error, tolerance: tolerance, exponent: exponent), min: MinScale, max: MaxScale)
            : MaxScale;
}

[Union]
public abstract partial record IntegrationStep<TState, TDelta> {
    public sealed record AcceptedCase(TState Next, double SuggestedStep, Option<double> Error, DenseOutputSpan<TState, TDelta> Dense) : IntegrationStep<TState, TDelta>;
    public sealed record RejectedCase(double SuggestedStep, double Error) : IntegrationStep<TState, TDelta>;
    private IntegrationStep() { }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DenseOutputSpan<TState, TDelta> {
    private readonly TState start;
    private readonly double step;
    private readonly Seq<TDelta> stages;
    private readonly RungeKuttaMethod method;
    private readonly IntegrationModule<TState, TDelta> module;
    public DenseConditions Conditions => method.Dense;

    internal DenseOutputSpan(
        TState start, double step, Seq<TDelta> stages,
        RungeKuttaMethod method, IntegrationModule<TState, TDelta> module) =>
        (this.start, this.step, this.stages, this.method, this.module) =
            (start, step, stages, method, module);

    public Fin<TState> PointAt(double theta) {
        if (!double.IsFinite(theta) || theta is < 0.0 or > 1.0) return Fin.Fail<TState>(new KernelFault.InvalidInput());
        DenseOutputSpan<TState, TDelta> self = this;
        return DenseOutput.Weights(
                self.method.Tableau, self.method.ContinuousExtension,
                self.method.Interpolant, self.method.Dense.DenseOrder, theta)
            .Map(weights => self.module.Add(arg1: self.start, arg2: self.step, arg3: self.module.Combine(coefficients: weights, deltas: self.stages)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed record RungeKuttaIntegrator {
    private RungeKuttaIntegrator(
        RungeKuttaMethod method,
        Option<(PositiveMagnitude Tolerance, StepControl Control)> adaptive) =>
        (Method, Policy) = (method, adaptive);

    public RungeKuttaMethod Method { get; }
    private Option<(PositiveMagnitude Tolerance, StepControl Control)> Policy { get; }

    public static Fin<RungeKuttaIntegrator> Fixed(RungeKuttaMethod method) =>
        from active in Optional(method).ToFin(new KernelFault.InvalidInput())
        from fixedKind in guard(!active.Tableau.Embedded.IsSome,
            new KernelFault.InvalidValue(Label: nameof(method), Requirement: "a Runge–Kutta method without embedded weights"))
        select new RungeKuttaIntegrator(active, None);

    public static Fin<RungeKuttaIntegrator> Adaptive(RungeKuttaMethod method, double tolerance, Option<StepControl> control = default) {
        StepControl controller = control.IfNone(StepControl.Default);
        return from active in Optional(method).ToFin(new KernelFault.InvalidInput())
               from adaptiveKind in guard(active.Tableau.Embedded.IsSome,
                   new KernelFault.InvalidValue(Label: nameof(method), Requirement: "a Runge–Kutta method with embedded weights"))
               from admitted in guard(controller.IsValid, new KernelFault.InvalidValue(Label: nameof(StepControl), Requirement: "finite positive safety factor, ordered positive scale clamps, nonnegative reject budget, a step controller"))
               from validated in FactoryBridge.Accept<PositiveMagnitude>(candidate: tolerance)
               select new RungeKuttaIntegrator(active, Some((Tolerance: validated, Control: controller)));
    }
    public int RejectBudget => Policy.Map(static policy => policy.Control.RejectBudget).IfNone(0);
    public int MethodOrder => Method.Tableau.MethodOrder;
    public Option<int> EmbeddedOrder => Method.Tableau.Embedded.Map(static pair => pair.Order);
    public Fin<IntegrationStep<TState, TDelta>> Step<TState, TDelta>(IntegrationModule<TState, TDelta> module, Func<TState, Fin<TDelta>> sample, TState state, double h, Option<StepHistory> history = default) =>
        from finiteStep in guard(double.IsFinite(h) && Math.Abs(h) > EpsilonPolicy.ZeroTolerance, new KernelFault.InvalidInput())
        from ks in Stages(module, sample, Method.Tableau, state, h)
        let primary = module.Combine(Method.Tableau.Weights, ks)
        from result in Policy.Match(
            Some: policy =>
                from embedded in Method.Tableau.Embedded.ToFin(new KernelFault.InvalidInput())
                let secondary = module.Combine(embedded.Weights, ks)
                let err = Math.Abs(h) * module.Norm(module.Sum(primary, module.Scale(-1.0, secondary)))
                from finite in guard(double.IsFinite(err), new KernelFault.InvalidResult())
                let scale = policy.Control.Rescale(history, err, policy.Tolerance.Value, 1.0 / (embedded.Order + 1.0))
                let next = module.Add(state, h, primary)
                from step in err <= policy.Tolerance.Value
                    ? Fin.Succ((IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.AcceptedCase(
                        next, h * scale, Some(err), new(state, h, ks, Method, module)))
                    : Fin.Succ((IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.RejectedCase(h * scale, err))
                select step,
            None: () => Fin.Succ((IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.AcceptedCase(
                module.Add(state, h, primary), h, None, new(state, h, ks, Method, module))))
        select result;
    private static Fin<Seq<TDelta>> Stages<TState, TDelta>(IntegrationModule<TState, TDelta> module, Func<TState, Fin<TDelta>> sample, ButcherTableau tableau, TState state, double h) =>
        tableau.Coupling.Fold(
            initialState: Fin.Succ((Seq<TDelta>)[]),
            f: (acc, row) => acc.Bind(ks =>
                sample(arg: module.Add(arg1: state, arg2: h, arg3: module.Combine(coefficients: row, deltas: ks)))
                    .Bind(k => double.IsFinite(module.Norm(k))
                        ? Fin.Succ(ks.Add(k))
                        : Fin.Fail<Seq<TDelta>>(new KernelFault.InvalidResult()))));
}
```

## [05]-[QUADRATURE]

- Owner: `QuadratureRoute` the `[SmartEnum<string>]` accuracy axis carrying each kernel's estimate delegate — value beside the optional error and L1 channels; `QuadratureDomain` the `[Union]` arity axis over genuine 1-D/2-D/3-D integrands, the Smolyak sparse grid, and the reference element — line, simplex, cube, prism, and pyramid rows alike; `ReferenceElement` the `[SmartEnum<string>]` row family whose rows carry the owned-build Gauss tables per reference domain — triangle/tet area-volume coordinates, `[-1,1]` cube tensor Gauss, triangle⊗line prism, conical pyramid; `IntegrationInterval` the bound value whose values alone encode finite or infinite extent; `Quadrature.Integrate` the one entry whose finite-guard-then-admit combinator reads the generated `Evaluate` delegate column once.
- Cases: `QuadratureRoute` rows `DoubleExponential` · `GaussLegendre` · `GaussKronrod`; `QuadratureDomain` cases `Line` · `Rectangle` · `Cuboid` · `SparseGrid` · `Reference`; `ReferenceElement` rows `Line` · `Tri` · `Tet` · `Quad` · `Hex` · `Wedge` · `Pyramid`, each electing the smallest owned rule at or above a POSITIVE requested order over a roster the generated `ValidateConstructorArguments` hook sorts ascending and de-duplicates by exactness once at construction, and each row's declared order the TRUE exactness of its construction — `Order` is polynomial EXACTNESS, never node count — an `n`-point Gauss-Legendre leg carries `2n−1` — a prism is exact to `min(triangle degree, 2n−1)`, never the sum of its legs, and a conical product to the weaker of its base leg `2n−1` and its height leg `2m−3`, the `(1−z)²` collapse Jacobian costing the height leg two degrees; `Pyramid12` therefore carries three, and a second rung raising only the base leg earns no exactness and is the deleted form. `QuadratureRule` rows stay DISTINCT-BY-DESIGN over the classical quadrature families — the upstream is the Gauss-Legendre nodes and the published symmetric simplex rules, and each row carries its point/weight table as DATA built once at type init, the tensor, prism, and conical rows deriving from the three canonical 1-D Gauss node sets and the simplex rows from their published closed forms spelled as exact expressions.
- Entry: `Quadrature.Integrate(QuadratureDomain domain, Option<QuadratureControl> control = default)` — arity is the case, accuracy the route row, and the reference-element table the `Reference` case's election; no sibling integrator entry exists.
- Auto: each arm wraps its integrand in a skip-counting guard because no MathNet route inspects returns and a pole poisons the weighted sum silently — `QuadratureControl.MaxSkipped` makes that loss budget explicit and defaults to zero; `Try.lift` is the one inbound exception funnel over the whole dispatch, so an integrand or MathNet raise surfaces on the typed result with `KernelFault.Cancelled` keeping its own identity; the `Line` arm admits any NaN-free ascending interval, signed infinities included, because every MathNet facade it routes through substitutes an infinite limit itself, while rectangle, cuboid, and sparse-grid bounds refuse non-finite endpoints through `FiniteOrdered`; only `GaussKronrod` returns the `error`/`L1Norm` channel, so the error-budget and cancellation gates bind only where the channels are `Some`; `SparseGrid` folds the nested Clenshaw-Curtis combination formula — its signed coefficients through `SpecialFunctions.Binomial`, never a hand-rolled fold — through the private nested `Quadrature.Smolyak.Integrate` under `MaxSparseLevel`; `Reference` folds the elected reference-element table — the reference-domain integral, the consumer weighting each point by its own Jacobian at the isoparametric map it owns.
- Law: a rule ladder that runs out REFUSES. `ReferenceElement.Rule` returns `Fin<QuadratureRule>` and faults typed when the requested order is nonpositive or exceeds every owned rule, computing the normalized ladder's terminal order locally and carrying it in the `KernelFault.OutOfRange` it returns, so no success-shaped under-integration ever reaches a caller who asked for a higher order; the constructor hook normalizes and never throws, so exhaustion — an accidentally empty roster included — stays inside the `Fin` carrier. The sixteen `QuadratureRule` tables are internal: `Rule` is the one election and no consumer bypasses its typed exhaustion proof.
- Exemption: the Gauss/simplex table builders hold `List<(double, double, double, double)>` accumulators and raw `double[]` node/weight pairs inside the nested `Smolyak`; both are statement kernels — the first runs once at type init and freezes into `ImmutableArray`, the second is the per-call tensor-node walk the sparse fold streams — and neither escapes its owner.
- Output: `QuadratureEvidence` carries one optional `(Error, CancellationRatio)` estimate, so the adaptive channels appear together and the private L1 preimage never escapes; the skip count rides it, never silently as coverage. The gate is three-tier — non-finite rejects, an adaptive error estimate over `max(AbsoluteError, RelativeError·|value|)` rejects, and a cancellation ratio breaching the floor rejects.
- Packages: MathNet.Numerics (`Integrate.DoubleExponential`/`GaussLegendre`/`GaussKronrod`/`OnRectangle`/`OnCuboid`, `SpecialFunctions.Binomial` for the Smolyak combination coefficients), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new accuracy kernel is one `QuadratureRoute` row with its delegate; a new arity is one `QuadratureDomain` case; a new reference domain or a higher-order rule is one `ReferenceElement` row or one entry on its rule ladder, and the ladder entry lifts every consumer already declaring that order — a prism or conical rung climbs only through its WEAKER leg (a degree-five pyramid rung raises the height leg to `Gauss4` at least), since the derived `min`/base-direction order makes a longer strong leg buy nothing the construction-time sort-and-distinct would rank; a new sparse-grid 1-D rule family or dimension-adaptive refinement is a policy row on the nested `Smolyak` — zero new surface.
- Boundary: accuracy is the primary decision with order secondary — the three MathNet kernels bind as route rows, never sibling factories, and `Measure` evaluates one guarded uniform `(Value, Error, L1Norm)` tuple then admits it once; L1 stays private and the public `Estimate` couples error with its derived cancellation ratio. The line domain owns infinite extent STRUCTURALLY — `Integrate.DoubleExponential`, `GaussLegendre`, and `GaussKronrod` are public facades that each substitute either or both infinite limits, so no route column could discriminate and none exists; any 1-D delegate forced through a 2-D rule integrates `(b−a)·∫f` and is rejected; `Estimate` presence is the convergence witness, so `RequireErrorWitness` defaults true and an unwitnessed route is an explicit opt-out. The reference-element tables integrate the REFERENCE domain — the physical mapping, its Jacobian, and the isoparametric basis stay the consuming element's; a consumer calling `Integrate.GaussLegendre` raw skips the finite guard, skip budget, and typed evidence and is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using LanguageExt;
using MathNet.Numerics;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct IntegrationInterval(double Lower, double Upper);

[SmartEnum<string>]
public sealed partial class QuadratureRoute {
    public static readonly QuadratureRoute DoubleExponential = new("double-exponential",
        evaluate: static (f, lo, hi, c) => (Value: Integrate.DoubleExponential(f, lo, hi, targetAbsoluteError: c.AbsoluteError), Error: Option<double>.None, L1Norm: Option<double>.None));
    public static readonly QuadratureRoute GaussLegendre = new("gauss-legendre",
        evaluate: static (f, lo, hi, c) => (Value: Integrate.GaussLegendre(f, lo, hi, order: c.LegendreOrder), Error: Option<double>.None, L1Norm: Option<double>.None));
    public static readonly QuadratureRoute GaussKronrod = new("gauss-kronrod",
        evaluate: static (f, lo, hi, c) => {
            double value = Integrate.GaussKronrod(f, lo, hi, out double error, out double l1Norm, c.RelativeError, c.MaximumDepth, c.KronrodPoints);
            return (Value: value, Error: Some(error), L1Norm: Some(l1Norm));
        });

    [UseDelegateFromConstructor]
    internal partial (double Value, Option<double> Error, Option<double> L1Norm) Evaluate(
        Func<double, double> integrand, double lower, double upper, QuadratureControl control);
}

[Union]
public abstract partial record QuadratureDomain {
    private QuadratureDomain() { }

    public sealed record Line(Func<double, double> F, IntegrationInterval Bounds, QuadratureRoute Route) : QuadratureDomain;
    public sealed record Rectangle(Func<double, double, double> F, IntegrationInterval X, IntegrationInterval Y, int Order) : QuadratureDomain;
    public sealed record Cuboid(Func<double, double, double, double> F, IntegrationInterval X, IntegrationInterval Y, IntegrationInterval Z, int Order) : QuadratureDomain;
    public sealed record SparseGrid(Func<double[], double> F, Arr<IntegrationInterval> Bounds, int Level) : QuadratureDomain;
    public sealed record Reference(Func<double, double, double, double> F, ReferenceElement Element, int Order) : QuadratureDomain;
}

public readonly record struct QuadratureRule(
    int Order, ImmutableArray<(double X, double Y, double Z, double Weight)> Points) {
    internal static readonly ImmutableArray<(double Node, double Weight)> Gauss1 = [(0.0, 2.0)];
    internal static readonly ImmutableArray<(double Node, double Weight)> Gauss2 = [(-1.0 / Math.Sqrt(3.0), 1.0), (1.0 / Math.Sqrt(3.0), 1.0)];
    internal static readonly ImmutableArray<(double Node, double Weight)> Gauss3 = [(-Math.Sqrt(0.6), 5.0 / 9.0), (0.0, 8.0 / 9.0), (Math.Sqrt(0.6), 5.0 / 9.0)];

    internal static readonly QuadratureRule Line2 = new(3, [.. Gauss2.Select(static g => (g.Node, 0.0, 0.0, g.Weight))]);
    internal static readonly QuadratureRule Line3 = new(5, [.. Gauss3.Select(static g => (g.Node, 0.0, 0.0, g.Weight))]);
    internal static readonly QuadratureRule Tri1 = new(1, [(1.0 / 3.0, 1.0 / 3.0, 0.0, 0.5)]);
    internal static readonly QuadratureRule Tri3 = new(2, [
        (1.0 / 6.0, 1.0 / 6.0, 0.0, 1.0 / 6.0), (2.0 / 3.0, 1.0 / 6.0, 0.0, 1.0 / 6.0), (1.0 / 6.0, 2.0 / 3.0, 0.0, 1.0 / 6.0)]);
    internal static readonly QuadratureRule Tri7 = new(5, [.. TriDegree5()]);
    internal static readonly QuadratureRule Tet1 = new(1, [(0.25, 0.25, 0.25, 1.0 / 6.0)]);
    internal static readonly QuadratureRule Tet4 = new(2, [.. Simplex3(a: (5.0 + (3.0 * Math.Sqrt(5.0))) / 20.0, b: (5.0 - Math.Sqrt(5.0)) / 20.0)]);
    internal static readonly QuadratureRule Quad1 = TensorCube(dim: 2, line: Gauss1);
    internal static readonly QuadratureRule Quad4 = TensorCube(dim: 2, line: Gauss2);
    internal static readonly QuadratureRule Quad9 = TensorCube(dim: 2, line: Gauss3);
    internal static readonly QuadratureRule Hex1 = TensorCube(dim: 3, line: Gauss1);
    internal static readonly QuadratureRule Hex8 = TensorCube(dim: 3, line: Gauss2);
    internal static readonly QuadratureRule Hex27 = TensorCube(dim: 3, line: Gauss3);
    internal static readonly QuadratureRule Wedge6 = PrismProduct(tri: Tri3, line: Gauss2);
    internal static readonly QuadratureRule Wedge21 = PrismProduct(tri: Tri7, line: Gauss3);
    internal static readonly QuadratureRule Pyramid12 = Conical(baseLine: Gauss2, heightLine: Gauss3);

    private static IEnumerable<(double, double, double, double)> Simplex3(double a, double b) {
        yield return (a, b, b, 1.0 / 24.0);
        yield return (b, a, b, 1.0 / 24.0);
        yield return (b, b, a, 1.0 / 24.0);
        yield return (b, b, b, 1.0 / 24.0);
    }

    private static IEnumerable<(double, double, double, double)> TriDegree5() {
        yield return (1.0 / 3.0, 1.0 / 3.0, 0.0, 9.0 / 80.0);
        foreach ((double a, double w) in new[] {
            ((6.0 - Math.Sqrt(15.0)) / 21.0, (155.0 - Math.Sqrt(15.0)) / 2400.0),
            ((6.0 + Math.Sqrt(15.0)) / 21.0, (155.0 + Math.Sqrt(15.0)) / 2400.0) }) {
            yield return (a, a, 0.0, w);
            yield return (1.0 - (2.0 * a), a, 0.0, w);
            yield return (a, 1.0 - (2.0 * a), 0.0, w);
        }
    }

    private static QuadratureRule TensorCube(int dim, ImmutableArray<(double Node, double Weight)> line) {
        List<(double, double, double, double)> rows = [];
        int n = line.Length;
        for (int k = 0; k < (dim == 3 ? n : 1); k++)
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++) {
                    double w = line[i].Weight * line[j].Weight * (dim == 3 ? line[k].Weight : 1.0);
                    rows.Add((line[i].Node, line[j].Node, dim == 3 ? line[k].Node : 0.0, w));
                }
        return new(2 * n - 1, [.. rows]);
    }

    private static QuadratureRule PrismProduct(QuadratureRule tri, ImmutableArray<(double Node, double Weight)> line) {
        List<(double, double, double, double)> rows = [];
        foreach ((double X, double Y, double Z, double Weight) point in tri.Points)
            foreach ((double node, double weight) in line) rows.Add((point.X, point.Y, node, point.Weight * weight));
        return new(Math.Min(val1: tri.Order, val2: (2 * line.Length) - 1), [.. rows]);
    }

    private static QuadratureRule Conical(
        ImmutableArray<(double Node, double Weight)> baseLine,
        ImmutableArray<(double Node, double Weight)> heightLine) {
        (double Node, double Weight)[] zeta = [.. heightLine.Select(static g =>
            (Node: (g.Node + 1.0) * 0.5, Weight: g.Weight * 0.5))];
        List<(double, double, double, double)> rows = [];
        foreach ((double z, double wz) in zeta) {
            double scale = 1.0 - z;
            foreach ((double bj, double wj) in baseLine)
                foreach ((double bi, double wi) in baseLine) rows.Add((bi * scale, bj * scale, z, wi * wj * wz * scale * scale));
        }
        int baseOrder = (2 * baseLine.Length) - 1;
        int heightOrder = (2 * heightLine.Length) - 3;
        return new(Math.Min(baseOrder, heightOrder), [.. rows]);
    }
}

[SmartEnum<string>]
public sealed partial class ReferenceElement {
    public static readonly ReferenceElement Line = new("line", rules: [QuadratureRule.Line2, QuadratureRule.Line3]);
    public static readonly ReferenceElement Tri = new("tri", rules: [QuadratureRule.Tri1, QuadratureRule.Tri3, QuadratureRule.Tri7]);
    public static readonly ReferenceElement Tet = new("tet", rules: [QuadratureRule.Tet1, QuadratureRule.Tet4]);
    public static readonly ReferenceElement Quad = new("quad", rules: [QuadratureRule.Quad1, QuadratureRule.Quad4, QuadratureRule.Quad9]);
    public static readonly ReferenceElement Hex = new("hex", rules: [QuadratureRule.Hex1, QuadratureRule.Hex8, QuadratureRule.Hex27]);
    public static readonly ReferenceElement Wedge = new("wedge", rules: [QuadratureRule.Wedge6, QuadratureRule.Wedge21]);
    public static readonly ReferenceElement Pyramid = new("pyramid", rules: [QuadratureRule.Pyramid12]);

    private readonly ImmutableArray<QuadratureRule> rules;

    public Fin<QuadratureRule> Rule(int order) {
        int ceiling = rules.IsDefaultOrEmpty ? 0 : rules[^1].Order;
        return toSeq(rules).Find(rule => order > 0 && rule.Order >= order).ToFin(
            new KernelFault.OutOfRange(
                Label: $"reference-rule:{Key}", Scalar: order,
                Requirement: $"a positive order with an owned rule at or above it, ceiling {ceiling}"));
    }

    static partial void ValidateConstructorArguments(
        ref string key, ref ImmutableArray<QuadratureRule> rules) =>
        rules = rules.IsDefaultOrEmpty ? [] : [.. rules
            .OrderBy(static rule => rule.Order)
            .DistinctBy(static rule => rule.Order)];
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record QuadratureControl(double AbsoluteError, double RelativeError, double CancellationFloor, int MaxSkipped, int LegendreOrder, int KronrodPoints, int MaximumDepth, int MaxSparseLevel, bool RequireErrorWitness = true) : IValidityEvidence {
    public static readonly QuadratureControl Default = new(AbsoluteError: 1e-8, RelativeError: 1e-8, CancellationFloor: 1e-10, MaxSkipped: 0, LegendreOrder: 128, KronrodPoints: 15, MaximumDepth: 15, MaxSparseLevel: 8);
    public bool IsValid => ValidityClaim.All(
        double.IsFinite(AbsoluteError) && AbsoluteError > 0.0,
        double.IsFinite(RelativeError) && RelativeError > 0.0,
        double.IsFinite(CancellationFloor) && CancellationFloor is >= 0.0 and <= 1.0,
        MaxSkipped >= 0 && LegendreOrder > 0 && KronrodPoints > 0 && MaximumDepth > 0,
        MaxSparseLevel is > 0 and < 31);
}

public sealed record QuadratureEvidence(
    double Value, Option<(double Error, double CancellationRatio)> Estimate, int Skipped);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Quadrature {
    private const string FiniteOrderedRequirement = "finite ascending intervals and a positive order";

    public static Fin<QuadratureEvidence> Integrate(QuadratureDomain domain, Option<QuadratureControl> control = default) {
        QuadratureControl ctl = control.IfNone(QuadratureControl.Default);
        return from active in Optional(domain).ToFin(new KernelFault.InvalidInput())
               from admitted in guard(ctl.IsValid, new KernelFault.InvalidValue(Label: nameof(QuadratureControl), Requirement: "finite positive budgets, unit-bounded floor, positive orders"))
               from evidence in Try.lift(() => active.Switch(
                   line: l => !(l.Bounds.Lower < l.Bounds.Upper)
                       ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(
                           Label: nameof(QuadratureDomain.Line),
                           Requirement: "a NaN-free ascending interval"))
                       : Measure(evaluate: guard => l.Route.Evaluate(x => guard.Finite(l.F(x)), l.Bounds.Lower, l.Bounds.Upper, ctl), control: ctl),
                   rectangle: r => !FiniteOrdered(bounds: r.X) || !FiniteOrdered(bounds: r.Y) || r.Order <= 0
                       ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(Label: nameof(QuadratureDomain.Rectangle), Requirement: FiniteOrderedRequirement))
                       : Measure(evaluate: guard => (Value: MathNet.Numerics.Integrate.OnRectangle((x, y) => guard.Finite(r.F(x, y)), r.X.Lower, r.X.Upper, r.Y.Lower, r.Y.Upper, r.Order), Error: Option<double>.None, L1Norm: Option<double>.None), control: ctl),
                   cuboid: c => !FiniteOrdered(bounds: c.X) || !FiniteOrdered(bounds: c.Y) || !FiniteOrdered(bounds: c.Z) || c.Order <= 0
                       ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(Label: nameof(QuadratureDomain.Cuboid), Requirement: FiniteOrderedRequirement))
                       : Measure(evaluate: guard => (Value: MathNet.Numerics.Integrate.OnCuboid((x, y, z) => guard.Finite(c.F(x, y, z)), c.X.Lower, c.X.Upper, c.Y.Lower, c.Y.Upper, c.Z.Lower, c.Z.Upper, c.Order), Error: Option<double>.None, L1Norm: Option<double>.None), control: ctl),
                   sparseGrid: s => s.Bounds.Count is < 2 or > 20 || s.Level <= 0 || s.Level > ctl.MaxSparseLevel || s.Bounds.Exists(static b => !FiniteOrdered(bounds: b))
                       ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(Label: nameof(QuadratureDomain.SparseGrid), Requirement: "2-20 finite dimensions inside the level budget"))
                       : Measure(evaluate: guard => Smolyak.Integrate(f: x => guard.Finite(s.F(x)), bounds: s.Bounds, level: s.Level), control: ctl),
                   reference: x => x.Order <= 0
                       ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(Label: nameof(QuadratureDomain.Reference), Requirement: "a positive rule order"))
                       : x.Element.Rule(order: x.Order).Bind(rule =>
                           Measure(evaluate: guard => {
                               double sum = 0.0;
                               foreach ((double px, double py, double pz, double weight) in rule.Points) sum += weight * guard.Finite(x.F(px, py, pz));
                               return (Value: sum, Error: Option<double>.None, L1Norm: Option<double>.None);
                           }, control: ctl)))).Run().Bind(static inner => inner)
               select evidence;
    }

    private static Fin<QuadratureEvidence> Measure(
        Func<SkipCounter, (double Value, Option<double> Error, Option<double> L1Norm)> evaluate,
        QuadratureControl control) {
        SkipCounter counter = new();
        return Admit(evaluate(counter), counter.Skipped, control);
    }

    private sealed class SkipCounter {
        internal int Skipped { get; private set; }
        internal double Finite(double value) {
            if (double.IsFinite(value)) { return value; }
            Skipped++;
            return 0.0;
        }
    }

    private static Fin<QuadratureEvidence> Admit((double Value, Option<double> Error, Option<double> L1Norm) outcome, int skipped, QuadratureControl ctl) =>
        !double.IsFinite(outcome.Value)
            ? Fin.Fail<QuadratureEvidence>(new KernelFault.OutOfRange(Label: "quadrature-value", Scalar: outcome.Value, Requirement: "finite"))
        : ctl.RequireErrorWitness && !outcome.Error.IsSome
            ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(Label: "convergence-witness", Requirement: "a route carrying its own error estimate, or RequireErrorWitness cleared"))
        : skipped > ctl.MaxSkipped
            ? Fin.Fail<QuadratureEvidence>(new KernelFault.OutOfRange(Label: "integrand-loss", Scalar: skipped, Requirement: $"<= {ctl.MaxSkipped} skipped samples"))
        : outcome.Error is { IsSome: true, Case: double reportedError }
          && reportedError > Math.Max(val1: ctl.AbsoluteError, val2: ctl.RelativeError * Math.Abs(value: outcome.Value))
            ? Fin.Fail<QuadratureEvidence>(new KernelFault.OutOfRange(Label: "error-over-budget", Scalar: reportedError, Requirement: $"<= {Math.Max(val1: ctl.AbsoluteError, val2: ctl.RelativeError * Math.Abs(value: outcome.Value)):e3}"))
        : (outcome.Error, outcome.L1Norm) switch {
            ({ IsSome: true, Case: double error }, { IsSome: true, Case: double l1 }) =>
                l1 == 0.0 && outcome.Value == 0.0
                    ? Fin.Succ(new QuadratureEvidence(outcome.Value, Some((error, 1.0)), skipped))
                    : Math.Abs(outcome.Value / l1) is double ratio
                      && double.IsFinite(ratio) && ratio >= ctl.CancellationFloor
                        ? Fin.Succ(new QuadratureEvidence(outcome.Value, Some((error, ratio)), skipped))
                        : Fin.Fail<QuadratureEvidence>(new KernelFault.OutOfRange(Label: "cancellation-breach", Scalar: ratio, Requirement: $">= {ctl.CancellationFloor:e3}")),
            ({ IsSome: false }, { IsSome: false }) =>
                Fin.Succ(new QuadratureEvidence(outcome.Value, None, skipped)),
            _ => Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidResult())
        };

    private static bool FiniteOrdered(IntegrationInterval bounds) =>
        double.IsFinite(bounds.Lower) && double.IsFinite(bounds.Upper) && bounds.Lower < bounds.Upper;

    private static class Smolyak {
        private static (double Value, Option<double> Error, Option<double> L1Norm) Integrate(
            Func<double[], double> f, Arr<IntegrationInterval> bounds, int level) {
            int d = bounds.Count;
            double sum = 0.0;
            Dictionary<(int Level, int Axis), (double[] Nodes, double[] Weights)> built = [];
            for (int axis = 0; axis < d; axis++) {
                for (int rule = 1; rule <= level + d - 1; rule++) { built[(rule, axis)] = ClenshawCurtis(level: rule, bounds: bounds[axis]); }
            }
            FrozenDictionary<(int Level, int Axis), (double[] Nodes, double[] Weights)> axes = built.ToFrozenDictionary();
            foreach ((int[] multi, int coefficient) in CombinationLevels(dimensions: d, level: level)) {
                double block = 0.0;
                foreach ((double[] node, double weight) in TensorNodes(multi: multi, axes: axes)) block += weight * f(node);
                sum += coefficient * block;
            }
            return (Value: sum, Error: Option<double>.None, L1Norm: Option<double>.None);
        }

        private static IEnumerable<(int[] Multi, int Coefficient)> CombinationLevels(int dimensions, int level) {
            int q = level + dimensions - 1;
            for (int total = q - dimensions + 1; total <= q; total++) {
                int coefficient = (((q - total) & 1) == 0 ? 1 : -1)
                    * (int)SpecialFunctions.Binomial(dimensions - 1, q - total);
                foreach (int[] multi in PositiveCompositions(total: total, slots: dimensions)) yield return (multi, coefficient);
            }
        }

        private static IEnumerable<int[]> PositiveCompositions(int total, int slots) {
            int[] parts = new int[slots];
            return Descend(slot: 0, remaining: total);
            IEnumerable<int[]> Descend(int slot, int remaining) {
                if (slot == slots - 1) { parts[slot] = remaining; yield return (int[])parts.Clone(); yield break; }
                for (int head = 1; head <= remaining - (slots - slot - 1); head++) {
                    parts[slot] = head;
                    foreach (int[] done in Descend(slot: slot + 1, remaining: remaining - head)) yield return done;
                }
            }
        }

        private static IEnumerable<(double[] Node, double Weight)> TensorNodes(int[] multi, FrozenDictionary<(int Level, int Axis), (double[] Nodes, double[] Weights)> axes) {
            (double[] Nodes, double[] Weights)[] rules = [.. multi.Select((level, axis) => axes[(level, axis)])];
            return Enumerable.Range(start: 0, count: rules.Aggregate(seed: 1, func: static (acc, rule) => acc * rule.Nodes.Length)).Select(flat => {
                double[] node = new double[rules.Length];
                double weight = 1.0;
                int rest = flat;
                for (int k = rules.Length - 1; k >= 0; k--) {
                    int i = rest % rules[k].Nodes.Length;
                    rest /= rules[k].Nodes.Length;
                    node[k] = rules[k].Nodes[i];
                    weight *= rules[k].Weights[i];
                }
                return (node, weight);
            });
        }

        private static (double[] Nodes, double[] Weights) ClenshawCurtis(int level, IntegrationInterval bounds) {
            int n = level == 1 ? 1 : (1 << (level - 1)) + 1;
            double half = 0.5 * (bounds.Upper - bounds.Lower);
            double mid = 0.5 * (bounds.Upper + bounds.Lower);
            if (n == 1) return ([mid], [2.0 * half]);
            double[] nodes = new double[n];
            double[] weights = new double[n];
            for (int i = 0; i < n; i++) {
                nodes[i] = mid - (half * Math.Cos(d: Math.PI * i / (n - 1)));
                double w = 1.0;
                for (int j = 1; j <= (n - 1) / 2; j++) {
                    double b = 2 * j == n - 1 ? 1.0 : 2.0;
                    w -= b * Math.Cos(d: 2.0 * j * Math.PI * i / (n - 1)) / ((4.0 * j * j) - 1.0);
                }
                weights[i] = half * 2.0 * w / (n - 1) * (i == 0 || i == n - 1 ? 0.5 : 1.0);
            }
            return (nodes, weights);
        }
    }
}
```

## [06]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or member on the owning carrier, never a sibling surface. Each `[OWNER]` cell names the canonical carrier; sibling carriers and the per-axis kind ride the indexed notes below.

| [INDEX] | [AXIS_CONCERN]       | [OWNER]                            | [CASES] |
| :-----: | :------------------- | :--------------------------------- | :-----: |
|  [01]   | Integrator rows      | `RungeKuttaMethod`                 |    9    |
|  [02]   | Coefficient carrier  | `ButcherTableau`                   |    1    |
|  [03]   | Continuous extension | `DenseOutput`                      |    3    |
|  [04]   | Step algebra         | `IntegrationModule<TState,TDelta>` |   2·2   |
|  [05]   | Accuracy route       | `QuadratureRoute`                  |    3    |
|  [06]   | Integration arity    | `QuadratureDomain`                 |    5    |
|  [07]   | Reference domain     | `ReferenceElement`                 |  7·16   |

- [01]-[INTEGRATOR_ROWS]: `[SmartEnum<string>]` — the canonical method key and admitted tableau share one row.
- [02]-[COEFFICIENT_CARRIER]: order-condition-validated record + `OrderConditions` `ValidityClaim.All` tally over the `RootedTree` walk.
- [03]-[CONTINUOUS_EXTENSION]: method-row exact-rational tables + direct MathNet multi-column moment fit; carriers `DenseConditions` · `DenseOutput` (`Interpolant` · `Conditions` · `Weights`).
- [04]-[STEP_ALGEBRA]: carrier-generic policy records + optional adaptive policy — `IntegrationModule<TState,TDelta>` (THE `Combine`) with `StepControl` · `RungeKuttaIntegrator` · `IntegrationStep` · `DenseOutputSpan`.
- [05]-[ACCURACY_ROUTE]: `[SmartEnum<string>]` rows carrying the MathNet facade delegate, every row admitting infinite line limits through the facade substitution; the private `(Value, Error, L1Norm)` tuple is the uniform transit and public `Estimate` presence is its witness.
- [06]-[INTEGRATION_ARITY]: `[Union]` cases over 1-D/2-D/3-D integrands, the Smolyak sparse grid, and the reference element; `IntegrationInterval` and `QuadratureControl`/`QuadratureEvidence` ride the same axis.
- [07]-[REFERENCE_DOMAIN]: seven `[SmartEnum<string>]` rows over sixteen `QuadratureRule` tables, each row electing the smallest owned rule at or above a positive request and refusing typed past the normalized ladder's terminal order `Rule` computes locally; the private nested `Quadrature.Smolyak` is the nested-rule algorithm the `SparseGrid` case alone reaches.

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
