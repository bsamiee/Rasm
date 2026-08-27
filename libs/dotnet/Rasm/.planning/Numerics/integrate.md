# [RASM_NUMERICS_INTEGRATE]

`Rasm.Numerics` integration is the ODE/Runge-Kutta floor — pure numerics, zero geometric content. Every Butcher tableau admits by VALIDATING its order conditions numerically rather than asserting them, so a mis-transcribed coefficient is a construction-time typed failure carrying the failed-condition census, never a silently wrong trajectory; one carrier-generic `Step` serves scalar, vector, and geometric state, and continuous dense output localizes events without re-tracing. Geometry enters only at the `Processing/flow` consumer that supplies the spatial module; this page never names a geometric type.

`IntegrationModule.Combine` is the corpus' single linear-combination site; `RungeKuttaIntegrator` factories derive each interpolant's `DenseConditions` AND its theta-independent correction basis once at admission, so the moment-fit least-squares never runs inside the step loop — the correction is linear in a right-hand side that is itself a polynomial in theta, which is what makes the solve hoistable at all. `Step` is a PURE step function returning one closed `IntegrationStep` Accepted or Rejected: the kernel owns no reject loop, no run-level terminal partition, no step-underflow floor, and no total-step budget — each is the driver's, and the boundary states itself at both ends (`Rasm.Compute/Tensor/quadrature.md` `[03]-[TRAJECTORY_DRIVER]` `TrajectoryPhase`, `Processing/flow.md` `FlowKernel.Trace`), so a driver distinguishes budget exhaustion from underflow from a refused field where a kernel-side loop hands every one of them back as the same best-so-far. Dense continuous-extension fallback solves its moment fit through the `matrix.md` least-squares route (`Matrix`, `LinearSolution`); every moment sum accumulates in 106-bit `ddouble` AND raises its terms there — the elementary weight folds through a `ddouble` coupling matrix and each residual raises through `ddouble.Pow` inside its 106-bit fold — so the claim rests on the summands, not on an accumulator widened after they already rounded.

## [01]-[INDEX]

- [02]-[TABLEAU_VOCABULARY]: `RungeKuttaMethod` tableau rows, the `RootedTree` Butcher-tree order algebra, and the order-condition-validated `ButcherTableau` carrier with `OrderConditions`.
- [03]-[DENSE_OUTPUT]: exact-rational interpolant families whose published-table option is the one source discriminant, the `DenseOutput` derivation, and the moment-fit fallback via the `matrix.md` least-squares route.
- [04]-[STEPPER]: carrier-generic `IntegrationModule` step algebra, the one `StepControl` adaptive-control policy row, and the `RungeKuttaIntegrator` Fixed/Adaptive union.
- [05]-[QUADRATURE]: accuracy-routed `QuadratureRoute` rows over the `QuadratureDomain` arity union, the `ReferenceElement` row family with its owned Gauss tables and typed ladder-exhaustion refusal, and the finite-guard-then-admit `Quadrature.Integrate` entry with `QuadratureEvidence`, whose `Error` presence is the convergence witness.
- [06]-[DENSITY_BAR]: one owner per axis across both bands.

## [02]-[TABLEAU_VOCABULARY]

- Owner: `RungeKuttaMethod` mints the `[SmartEnum<int>]` whose rows ARE the Butcher tableaux, each a single declaration through the ONE private `Of` factory whose one optional embedded `(Weights, Order)` pair discriminates fixed from embedded — the two halves share one lifecycle, so a weight row without its order is unrepresentable — and each carrying its derived `OrderConditions` and dense-output family as columns; `RootedTree` is the Butcher-tree algebra — order the node count, density `γ(t) = |t|·Πγ(children)`, elementary weight `Φᵢ(t)` off the coupling matrix — whose condition set `Σᵢ bᵢΦᵢ(t) = 1/γ(t)` over every tree of order ≤ p IS the order-p proof, its pool memoized to the tableau's private `OrderCeiling`; `ButcherTableau` registers `IValidityEvidence` and runs that full walk at the declared order rather than asserting it, and its nested `OrderConditions` carries the walk's tally beside `VerifiedOrder` — the largest certified prefix, DERIVED inside the same 106-bit walk, never a second lower-precision pass.
- Cases: `Euler` · `Heun` · `Midpoint` · `Ralston` · `RK4` · `RK38` fixed; `BogackiShampine` · `CashKarp` · `DormandPrince` embedded-adaptive. Rows stay DISTINCT-BY-DESIGN over the closed Butcher-tableau literature — the upstream is the published tableau of each named method (Butcher's classical fourth-order pair and the 3/8 rule; Bogacki-Shampine 3(2); Cash-Karp 5(4); Dormand-Prince 5(4)) — and each row carries its coupling matrix, weight row, and embedded weight row as DATA, so no row re-derives a coefficient in a body and the order/stage witness folds from that data through the tree walk rather than a declared number.
- Entry: `RungeKuttaMethod.<Row>.Tableau` reads the validated carrier; `ButcherTableau.Admit` gates a tableau into `Fin`; `IsFunctionalSameAsLast` detects the FSAL structure that fingerprints the method-specific dense-output families.
- Auto: abscissae never enter as data — the factories derive them as coupling row sums, so the consistency condition holds by construction and the validity fold re-checks it as the transcription witness; the adaptive step reads its exponent `1/(q+1)` off the embedded pair's order only after the adaptive factory has proved the pair present, so no fixed row silently inherits Dormand-Prince's 0.2 and no exponent falls back to `1.0`; the tree pool per order generates once behind an accessor-forced lazy, so a fifth-order pair proves seventeen conditions with zero condition code and no re-derivation per level; the order conditions (verified prefix included) and the dense-output family derive at ROW construction and ride as columns, so one admission runs the tree walk once.
- Law: `Admit` returns an order-carrying tableau or a TYPED structural fault that carries the walk's own census — failed and checked condition counts, the max residual, and the derived `VerifiedOrder` beside the declared `MethodOrder` — so a mis-transcribed coefficient names which claim broke instead of collapsing to a bare invalid-input token (`docs/stacks/csharp/algorithms.md [INTEGRATOR_TABLEAU]`).
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
[SmartEnum<int>]
public sealed partial class RungeKuttaMethod {
    public static readonly RungeKuttaMethod Euler = Of(key: 0, order: 1, coupling: [[]], weights: [1.0]);
    public static readonly RungeKuttaMethod Heun = Of(key: 1, order: 2, coupling: [[], [1.0]], weights: [0.5, 0.5]);
    public static readonly RungeKuttaMethod Midpoint = Of(key: 2, order: 2, coupling: [[], [0.5]], weights: [0.0, 1.0]);
    public static readonly RungeKuttaMethod Ralston = Of(key: 3, order: 2, coupling: [[], [2.0 / 3.0]], weights: [0.25, 0.75]);
    public static readonly RungeKuttaMethod RK4 = Of(key: 4, order: 4,
        coupling: [[], [0.5], [0.0, 0.5], [0.0, 0.0, 1.0]],
        weights: [1.0 / 6.0, 1.0 / 3.0, 1.0 / 3.0, 1.0 / 6.0]);
    public static readonly RungeKuttaMethod RK38 = Of(key: 5, order: 4,
        coupling: [[], [1.0 / 3.0], [-1.0 / 3.0, 1.0], [1.0, -1.0, 1.0]],
        weights: [1.0 / 8.0, 3.0 / 8.0, 3.0 / 8.0, 1.0 / 8.0]);
    public static readonly RungeKuttaMethod BogackiShampine = Of(key: 6, order: 3,
        coupling: [[], [0.5], [0.0, 0.75], [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0]],
        weights: [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0, 0.0],
        embedded: Some<(double[] Weights, int Order)>((Weights: [7.0 / 24.0, 0.25, 1.0 / 3.0, 1.0 / 8.0], Order: 2)));
    public static readonly RungeKuttaMethod CashKarp = Of(key: 7, order: 5,
        coupling: [[], [0.2], [3.0 / 40.0, 9.0 / 40.0], [0.3, -0.9, 1.2],
            [-11.0 / 54.0, 2.5, -70.0 / 27.0, 35.0 / 27.0],
            [1631.0 / 55296.0, 175.0 / 512.0, 575.0 / 13824.0, 44275.0 / 110592.0, 253.0 / 4096.0]],
        weights: [37.0 / 378.0, 0.0, 250.0 / 621.0, 125.0 / 594.0, 0.0, 512.0 / 1771.0],
        embedded: Some<(double[] Weights, int Order)>((Weights: [2825.0 / 27648.0, 0.0, 18575.0 / 48384.0, 13525.0 / 55296.0, 277.0 / 14336.0, 0.25], Order: 4)));
    public static readonly RungeKuttaMethod DormandPrince = Of(key: 8, order: 5,
        coupling: [[], [1.0 / 5.0], [3.0 / 40.0, 9.0 / 40.0],
            [44.0 / 45.0, -56.0 / 15.0, 32.0 / 9.0],
            [19372.0 / 6561.0, -25360.0 / 2187.0, 64448.0 / 6561.0, -212.0 / 729.0],
            [9017.0 / 3168.0, -355.0 / 33.0, 46732.0 / 5247.0, 49.0 / 176.0, -5103.0 / 18656.0],
            [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0]],
        weights: [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0, 0.0],
        embedded: Some<(double[] Weights, int Order)>((Weights: [5179.0 / 57600.0, 0.0, 7571.0 / 16695.0, 393.0 / 640.0, -92097.0 / 339200.0, 187.0 / 2100.0, 1.0 / 40.0], Order: 4)));
    internal ButcherTableau Tableau { get; }
    internal ButcherTableau.OrderConditions Conditions { get; }
    internal DenseFormula Formula { get; }
    private static RungeKuttaMethod Of(
        int key, int order, double[][] coupling, double[] weights,
        Option<(double[] Weights, int Order)> embedded = default) {
        ButcherTableau tableau = ButcherTableau.Of(
            toSeq(coupling.Select(static row => toSeq(row))), toSeq(weights),
            embedded.Map(static pair => (toSeq(pair.Weights), pair.Order)), order);
        ButcherTableau.OrderConditions conditions = tableau.ConditionsOf(tableau.Weights, order);
        return new(tableau: tableau, conditions: conditions,
            formula: DenseFormula.Identify(tableau));
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
internal readonly record struct ButcherTableau : IValidityEvidence {
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
    internal bool IsFunctionalSameAsLast =>
        StageCount > 1
        && Coupling.Count == StageCount
        && Math.Abs(value: Abscissae[StageCount - 1] - 1.0) <= CoefficientTolerance
        && Math.Abs(value: Weights[StageCount - 1]) <= CoefficientTolerance
        && Coupling[StageCount - 1].Count == StageCount - 1
        && Coupling[StageCount - 1].Zip(Weights.Take(StageCount - 1)).ForAll(static pair => Math.Abs(value: pair.First - pair.Second) <= CoefficientTolerance);
    public bool IsValid => Valid(ConditionsOf(Weights, MethodOrder));
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
                double residual = Math.Abs((double)lhs - (1.0 / tree.Density));
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

- Owner: `DenseFormula` mints the keyless `[SmartEnum]` implementation-only continuous-extension owner whose `Published` table option IS the source discriminant — `Some` for a tabulated continuous extension, `None` for the least-squares fallback — where `DormandPrinceShampine` and `BogackiShampine` carry their published EXACT-RATIONAL interpolant tables as row data and `Identify` matches a tableau to its family by abscissae + FSAL fingerprint, falling to `GenericMomentFit`; `DenseOutput` proves the interpolant ONCE per integrator admission (moment residuals at θ ∈ {0, ½, 1}, endpoint value/derivative residuals, coefficient consistency against the step weights), and its generic fallback solves the least-squares moment fit through the `matrix.md` route so the interpolant construction leaves a `LinearSolution` inside `DenseConditions`.
- Cases: `DenseFormula` rows `GenericMomentFit` · `DormandPrinceShampine` · `BogackiShampine`.
- Entry: consumers never reach the family directly — `DenseOutput.Conditions` and `DenseOutput.WeightsAt` are the two internal operations, and both take the tableau, the family, and the correction basis the INTEGRATOR ROW already derived, so no interpolant evaluation re-identifies a family or re-runs a matrix solve.
- Auto: the generic route pins the endpoints exactly and fits only the interior through the `θ(1−θ)`-scaled correction, so endpoint continuity is structural; that correction is LINEAR in its moment right-hand side and the right-hand side is a polynomial in θ, so `DenseInterpolant` carries one solved basis column per moment and `WeightsAt` evaluates a polynomial where it once ran two matrix solves per θ; `DistinctAnchors` is the ONE distinct-abscissa derivation — the generic dense order caps at its count (the Vandermonde rank ceiling) and the moment preimage solves at those same anchor indices, one scan under one accumulator.
- Law: `DenseConditions` carries no family column — its evidence shape IS the discriminant: the endpoint derivative pair is `Some` exactly for a published table and the correction `LinearSolution` is `Some` exactly for a moment-fit interpolant, coupled as one exclusive-or, so a published/fitted disagreement is unrepresentable; `Conditions` always probes θ ∈ {0, ½, 1}, so the census floor of three is the algorithm's own fact and rides no stored count that could disagree with it.
- Law: `DenseConditions` folds `ValidityClaim.All` coupling every residual to `CoefficientTolerance` and the family to its evidence shape — the endpoint set rides the one `EndpointResiduals` carrier whose `Derivatives` pair is `Some` exactly when the family's `Published` table is, so an unmeasured endpoint derivative spells absence and never a fabricated `0.0`.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, System.Numerics.Tensors (`TensorPrimitives.Dot` — the design-plane product), CommunityToolkit.HighPerformance (`Span2D<double>` over the row-major design), `matrix.md` owners (`Matrix`, `LinearSolution`).
- Growth: a new published interpolant is one `DenseFormula` row — fingerprint, order, and table inside its `Published` option; a tableau without a published interpolant costs nothing, the generic moment fit covering it at the Vandermonde-rank order.
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
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
internal sealed partial class DenseFormula {
    public static readonly DenseFormula GenericMomentFit = new(fixedDenseOrder: 0, published: None);
    public static readonly DenseFormula DormandPrinceShampine = new(fixedDenseOrder: 4, published: Some((Fingerprint: DormandPrinceAbscissae, Table: DormandPrinceTable)));
    public static readonly DenseFormula BogackiShampine = new(fixedDenseOrder: 3, published: Some((Fingerprint: BogackiShampineAbscissae, Table: BogackiShampineTable)));
    internal int FixedDenseOrder { get; }
    internal Option<(double[] Fingerprint, double[][] Table)> Published { get; }
    private static double[] DormandPrinceAbscissae => [0.0, 1.0 / 5.0, 3.0 / 10.0, 4.0 / 5.0, 8.0 / 9.0, 1.0, 1.0];
    private static double[] BogackiShampineAbscissae => [0.0, 1.0 / 2.0, 3.0 / 4.0, 1.0];
    private static double[][] DormandPrinceTable => [
        [1.0, -8048581381.0 / 2820520608.0, 8663915743.0 / 2820520608.0, -12715105075.0 / 11282082432.0],
        [0.0, 0.0, 0.0, 0.0],
        [0.0, 131558114200.0 / 32700410799.0, -68118460800.0 / 10900136933.0, 87487479700.0 / 32700410799.0],
        [0.0, -1754552775.0 / 470086768.0, 14199869525.0 / 1410260304.0, -10690763975.0 / 1880347072.0],
        [0.0, 127303824393.0 / 49829197408.0, -318862633887.0 / 49829197408.0, 701980252875.0 / 199316789632.0],
        [0.0, -282668133.0 / 205662961.0, 2019193451.0 / 616988883.0, -1453857185.0 / 822651844.0],
        [0.0, 40617522.0 / 29380423.0, -110615467.0 / 29380423.0, 69997945.0 / 29380423.0]];
    private static double[][] BogackiShampineTable => [
        [1.0, -4.0 / 3.0, 5.0 / 9.0],
        [0.0, 1.0, -2.0 / 3.0],
        [0.0, 4.0 / 3.0, -8.0 / 9.0],
        [0.0, -1.0, 1.0]];
    internal static DenseFormula Identify(ButcherTableau tableau) =>
        toSeq(Items).Find(family => family.Published.Exists(held =>
            tableau.StageCount == held.Fingerprint.Length && tableau.IsFunctionalSameAsLast
            && held.Fingerprint.Zip(tableau.Abscissae).All(pair =>
                Math.Abs(pair.First - pair.Second) <= ButcherTableau.CoefficientTolerance)))
            .IfNone(GenericMomentFit);
    internal Fin<Seq<double>> WeightsAt(double theta, int stageCount) => Evaluate(theta: theta, stageCount: stageCount, project: Horner);
    internal Fin<Seq<double>> DerivativeAt(double theta, int stageCount) => Evaluate(theta: theta, stageCount: stageCount, project: HornerDerivative);
    private Fin<Seq<double>> Evaluate(double theta, int stageCount, Func<double[], double, double> project) =>
        Published.Filter(held => held.Table.Length == stageCount)
            .Map(held => Acceptance.Rows(values: held.Table.Select(row => project(row, theta))))
            .IfNone(() => Fin.Fail<Seq<double>>(new KernelFault.InvalidInput()));
    private static double Horner(double[] row, double theta) {
        double accumulated = 0.0;
        for (int index = row.Length - 1; index >= 0; index--) { accumulated = (accumulated * theta) + row[index]; }
        return theta * accumulated;
    }
    private static double HornerDerivative(double[] row, double theta) {
        double accumulated = 0.0;
        for (int index = row.Length - 1; index >= 0; index--) { accumulated = (accumulated * theta) + ((index + 1) * row[index]); }
        return accumulated;
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct EndpointResiduals {
    internal EndpointResiduals(double valueLeft, double valueRight, Option<(double Left, double Right)> derivatives, double coefficient) =>
        (ValueLeft, ValueRight, Derivatives, Coefficient) =
            (Math.Abs(value: valueLeft), Math.Abs(value: valueRight),
             derivatives.Map(static pair => (Left: Math.Abs(value: pair.Left), Right: Math.Abs(value: pair.Right))), Math.Abs(value: coefficient));
    public double ValueLeft { get; }
    public double ValueRight { get; }
    public Option<(double Left, double Right)> Derivatives { get; }
    public double Coefficient { get; }
    public bool WithinTolerance(double tolerance) =>
        ValueLeft <= tolerance && ValueRight <= tolerance && Coefficient <= tolerance
        && Derivatives.Map(pair => pair.Left <= tolerance && pair.Right <= tolerance).IfNone(noneValue: true);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DenseConditions(int StageCount, int MethodOrder, int DenseOrder, int CheckedConditionCount, int FailedConditionCount, double MaxResidual, EndpointResiduals Endpoints, Option<LinearSolution> CorrectionSolve = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        StageCount >= 1 && MethodOrder >= 1 && DenseOrder >= 0,
        DenseOrder <= MethodOrder,
        CheckedConditionCount >= 3,
        ValidityClaim.CountExactly(count: FailedConditionCount, expected: 0),
        ValidityClaim.Nonnegative(value: MaxResidual),
        MaxResidual <= ButcherTableau.CoefficientTolerance,
        Endpoints.WithinTolerance(ButcherTableau.CoefficientTolerance),
        Endpoints.Derivatives.IsSome ^ CorrectionSolve.IsSome,
        CorrectionSolve.Map(static solve => solve.IsValid).IfNone(noneValue: true));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DenseOutput {
    internal static Fin<DenseConditions> Conditions(ButcherTableau tableau, DenseFormula family, DenseInterpolant interpolant) {
        int order = DenseOrderFor(family: family, tableau: tableau);
        return ProbeAt(family: family, tableau: tableau, interpolant: interpolant, order: order, theta: 0.0).Bind(zero =>
            ProbeAt(family: family, tableau: tableau, interpolant: interpolant, order: order, theta: 0.5).Bind(mid =>
                ProbeAt(family: family, tableau: tableau, interpolant: interpolant, order: order, theta: 1.0).Bind(one =>
                    EndpointEvidence(family: family, tableau: tableau, interpolant: interpolant, order: order).Bind(evidence => {
                        DenseConditions conditions = new(
                            StageCount: tableau.StageCount, MethodOrder: tableau.MethodOrder, DenseOrder: order,
                            CheckedConditionCount: zero.CheckedConditionCount + mid.CheckedConditionCount + one.CheckedConditionCount,
                            FailedConditionCount: zero.FailedConditionCount + mid.FailedConditionCount + one.FailedConditionCount,
                            MaxResidual: Math.Max(val1: zero.MaxResidual, val2: Math.Max(val1: mid.MaxResidual, val2: one.MaxResidual)),
                            Endpoints: evidence, CorrectionSolve: mid.CorrectionSolve);
                        return conditions.IsValid ? Fin.Succ(conditions) : Fin.Fail<DenseConditions>(new KernelFault.InvalidResult());
                    }))));
    }
    internal static Fin<Seq<double>> WeightsAt(ButcherTableau tableau, DenseFormula family, DenseInterpolant interpolant, double theta) =>
        !double.IsFinite(theta) || theta is < 0.0 or > 1.0
            ? Fin.Fail<Seq<double>>(new KernelFault.InvalidInput())
            : Weights(family: family, tableau: tableau, interpolant: interpolant, order: DenseOrderFor(family: family, tableau: tableau), theta: theta)
                .Map(static result => result.Values);
    private static int DenseOrderFor(DenseFormula family, ButcherTableau tableau) =>
        family.Published.IsSome
            ? family.FixedDenseOrder
            : Math.Max(val1: 1, val2: Math.Min(val1: tableau.MethodOrder, val2: DistinctAnchors(tableau: tableau).Count));
    private static Seq<int> DistinctAnchors(ButcherTableau tableau) =>
        tableau.Abscissae.AsIterable().Select((c, stage) => (Stage: stage, C: c)).Aggregate(
            seed: Seq<int>(),
            func: (anchors, row) => anchors.Exists(seen => Math.Abs(value: tableau.Abscissae[seen] - row.C) <= ButcherTableau.CoefficientTolerance)
                ? anchors
                : anchors.Add(row.Stage));
    private readonly record struct ThetaProbe(int CheckedConditionCount, int FailedConditionCount, double MaxResidual, Option<LinearSolution> CorrectionSolve);

    private static Fin<ThetaProbe> ProbeAt(DenseFormula family, ButcherTableau tableau, DenseInterpolant interpolant, int order, double theta) =>
        Weights(family: family, tableau: tableau, interpolant: interpolant, order: order, theta: theta).Map(result => {
            Seq<double> weights = result.Values;
            (bool failed, double maxResidual) = MomentResidual(tableau: tableau, weights: weights, theta: theta, order: order);
            double endpoint = theta <= ButcherTableau.ThetaEndpointBand
                ? weights.Fold(initialState: 0.0, f: static (max, value) => Math.Max(val1: max, val2: Math.Abs(value: value)))
                : 1.0 - theta <= ButcherTableau.ThetaEndpointBand
                    ? weights.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second)))
                    : 0.0;
            return new ThetaProbe(
                CheckedConditionCount: order + ((theta <= ButcherTableau.ThetaEndpointBand || 1.0 - theta <= ButcherTableau.ThetaEndpointBand) ? tableau.StageCount : 0),
                FailedConditionCount: (failed ? 1 : 0) + (endpoint <= ButcherTableau.CoefficientTolerance ? 0 : 1),
                MaxResidual: Math.Max(val1: maxResidual, val2: endpoint),
                CorrectionSolve: result.Solve);
        });
    private static Fin<EndpointResiduals> EndpointEvidence(DenseFormula family, ButcherTableau tableau, DenseInterpolant interpolant, int order) =>
        family.Published.IsSome
            ? from atOne in family.WeightsAt(theta: 1.0, stageCount: tableau.StageCount)
              from atZero in family.WeightsAt(theta: 0.0, stageCount: tableau.StageCount)
              from derivOne in family.DerivativeAt(theta: 1.0, stageCount: tableau.StageCount)
              from derivZero in family.DerivativeAt(theta: 0.0, stageCount: tableau.StageCount)
              select new EndpointResiduals(
                  ValueLeft: atZero.Fold(0.0, static (max, value) => Math.Max(max, Math.Abs(value))),
                  ValueRight: Math.Abs(value: atOne.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - tableau.Weights.Fold(initialState: 0.0, f: static (sum, value) => sum + value)),
                  Derivatives: Some((Left: MaxDeviation(values: derivZero, target: 0), Right: MaxDeviation(values: derivOne, target: tableau.StageCount - 1))),
                  Coefficient: atOne.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second))))
            : Weights(family: family, tableau: tableau, interpolant: interpolant, order: order, theta: 1.0).Bind(atOne =>
                Weights(family: family, tableau: tableau, interpolant: interpolant, order: order, theta: 0.0).Map(atZero => new EndpointResiduals(
                    ValueLeft: atZero.Values.Fold(0.0, static (max, value) => Math.Max(max, Math.Abs(value))),
                    ValueRight: Math.Abs(value: atOne.Values.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - tableau.Weights.Fold(initialState: 0.0, f: static (sum, value) => sum + value)),
                    Derivatives: Option<(double Left, double Right)>.None,
                    Coefficient: atOne.Values.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second))))));
    private static double MaxDeviation(Seq<double> values, int target) =>
        values.AsIterable().Select((value, index) => Math.Abs(value: value - (index == target ? 1.0 : 0.0))).Aggregate(seed: 0.0, func: static (max, deviation) => Math.Max(val1: max, val2: deviation));
    private static Fin<(Seq<double> Values, Option<LinearSolution> Solve)> Weights(DenseFormula family, ButcherTableau tableau, DenseInterpolant interpolant, int order, double theta) =>
        family.Published.IsSome
            ? family.WeightsAt(theta: theta, stageCount: tableau.StageCount).Map(static values => (Values: values, Solve: Option<LinearSolution>.None))
            : theta <= ButcherTableau.ThetaEndpointBand
                ? Fin.Succ((Values: toSeq(Enumerable.Repeat(element: 0.0, count: tableau.StageCount)), Solve: Option<LinearSolution>.None))
                : 1.0 - theta <= ButcherTableau.ThetaEndpointBand
                    ? Fin.Succ((Values: tableau.Weights, Solve: Option<LinearSolution>.None))
                    : Fin.Succ((
                        Values: toSeq(tableau.Weights.Map(weight => theta * weight)
                            .Zip(toSeq(interpolant.At(theta: theta, stages: tableau.StageCount)))
                            .Select(pair => pair.First + (theta * (1.0 - theta) * pair.Second))),
                        Solve: interpolant.Solve));
    internal readonly record struct DenseInterpolant(int Order, ImmutableArray<double[]> Basis, Option<LinearSolution> Solve) {
        internal double[] At(double theta, int stages) {
            double endpointScale = theta * (1.0 - theta);
            double[] correction = new double[stages];
            double power = theta;
            for (int m = 0; m < Order; m++) {
                power *= theta;
                double coefficient = (power - theta) / ((m + 1.0) * endpointScale);
                for (int stage = 0; stage < stages; stage++) { correction[stage] += coefficient * Basis[m][stage]; }
            }
            return correction;
        }
    }

    internal static Fin<DenseInterpolant> Interpolant(ButcherTableau tableau, DenseFormula family) {
        int order = DenseOrderFor(family: family, tableau: tableau);
        if (family.Published.IsSome) { return Fin.Succ(new DenseInterpolant(Order: order, Basis: [], Solve: None)); }
        int stages = tableau.StageCount;
        double[] design = MomentDesign(tableau: tableau, stages: stages, order: order);
        return Range(0, order).ToSeq()
            .TraverseM(moment => BasisColumn(tableau: tableau, design: design, stages: stages, order: order, moment: moment)).As()
            .Map(columns => new DenseInterpolant(
                Order: order,
                Basis: [.. columns.Map(static column => column.Correction)],
                Solve: columns.Last.Map(static column => column.Solve)));
    }

    private static Fin<(double[] Correction, LinearSolution Solve)> BasisColumn(ButcherTableau tableau, double[] design, int stages, int order, int moment) {
        double[] unit = new double[order];
        unit[moment] = 1.0;
        return from preimage in MomentPreimage(tableau: tableau, stages: stages, order: order, rhs: new Arr<double>(unit))
               from rows in FactoryBridge.Accept<Dimension>(stages)
               from cols in FactoryBridge.Accept<Dimension>(order)
               from matrix in Matrix.Of(rows: rows, cols: cols, entries: new Arr<double>(design))
               from solved in matrix.LeastSquaresDetailed(rhs: preimage)
               select (Correction: DesignProduct(design: design, solution: solved.Solution, stages: stages, order: order), Solve: solved);
    }

    private static double[] DesignProduct(double[] design, Arr<double> solution, int stages, int order) {
        Span2D<double> rows = design.AsSpan2D(height: stages, width: order);
        double[] correction = new double[stages];
        for (int stage = 0; stage < stages; stage++) { correction[stage] = TensorPrimitives.Dot<double>(rows.GetRowSpan(stage), solution.AsSpan()); }
        return correction;
    }
    private static double[] MomentDesign(ButcherTableau tableau, int stages, int order) {
        double[] design = new double[stages * order];
        for (int stage = 0; stage < stages; stage++) {
            double raised = 1.0;
            for (int power = 0; power < order; power++) { design[(stage * order) + power] = raised; raised *= tableau.Abscissae[stage]; }
        }
        return design;
    }
    private static Fin<Arr<double>> MomentPreimage(ButcherTableau tableau, int stages, int order, Arr<double> rhs) {
        Seq<int> anchors = DistinctAnchors(tableau: tableau).Take(order);
        if (anchors.Count < order) return Fin.Fail<Arr<double>>(new KernelFault.InvalidInput());
        double[] vandermonde = new double[order * order];
        for (int col = 0; col < order; col++) {
            double raised = 1.0;
            for (int row = 0; row < order; row++) { vandermonde[(row * order) + col] = raised; raised *= tableau.Abscissae[anchors[col]]; }
        }
        return FactoryBridge.Accept<Dimension>(order).Bind(dim =>
            Matrix.Of(rows: dim, cols: dim, entries: new Arr<double>(vandermonde)))
            .Bind(matrix => matrix.SolveDetailed(rhs: rhs))
            .Map(solved => {
                double[] preimage = new double[stages];
                for (int index = 0; index < order; index++) preimage[anchors[index]] = solved.Solution[index];
                return new Arr<double>(preimage);
            });
    }
    private static (bool Failed, double Max) MomentResidual(
        ButcherTableau tableau, Seq<double> weights, double theta, int order) =>
        Enumerable.Range(0, order)
            .Select(moment => {
                ddouble actual = weights.Zip(tableau.Abscissae).Fold((ddouble)0.0,
                    (sum, pair) => sum + (pair.First * ddouble.Pow(pair.Second, moment)));
                return Math.Abs((double)actual - (Math.Pow(theta, moment + 1) / (moment + 1.0)));
            })
            .Aggregate(seed: (Failed: false, Max: 0.0), func: static (state, residual) => (
                Failed: state.Failed || !double.IsFinite(residual) || residual > ButcherTableau.CoefficientTolerance,
                Max: Math.Max(state.Max, residual)));
}
```

## [04]-[STEPPER]

- Owner: `IntegrationModule<TState, TDelta>` mints the additive-module policy record — the four operations one Runge-Kutta step needs and its `Zero` delta, carrying `Combine` as the corpus' single linear-combination fold and the `Scalar`/`ComplexScalar` canonical instances for `double` and `Complex` state; `StepControl` mints the ONE adaptive-control policy row — safety factor, step-ratio clamps, reject budget, and the keyless `StepController` row (`Integral` · `ProportionalIntegral` · `Gustafsson`, Hairer-Wanner's I, PI, and Gustafsson controllers) travelling together as its `Controller` column (`docs/stacks/csharp/algorithms.md [INTEGRATOR_TABLEAU]`) — whose `Rescale` reads the driver-threaded `Option<StepHistory>` — the previous step's error beside the scale the driver actually selected, present exactly when the previous step was adaptive — so an I, PI, or Gustafsson controller is a ROW and the stepper body never changes (Hairer-Wanner II §IV.2); `RungeKuttaIntegrator` mints the `[Union]` Fixed/Adaptive integrator whose factories derive the interpolant's `DenseConditions` once and carry them on the case, and whose generic `Step` folds the coupling rows into stages, forms the primary and embedded combinations, applies the error control, and mints the dense-output span; `IntegrationStep<TState, TDelta>` is the closed continue-or-done outcome; `DenseOutputSpan<TState, TDelta>` carries the per-step continuous extension behind one public `Conditions` column and `PointAt`, its construction re-verifying the θ=1 weight combination against the tableau's declared weights; `AcceptedCase.Next` is the one terminal state and the span reconstructs any θ from `start + h·Σbᵢ(θ)kᵢ`.
- Cases: `RungeKuttaIntegrator` `FixedCase` · `AdaptiveCase`; `IntegrationStep` `AcceptedCase` · `RejectedCase`.
- Entry: `RungeKuttaIntegrator.Fixed` and `Adaptive` admit — re-validating the tableau, enforcing method/case agreement (a fixed integrator over an embedded method, or the reverse, fails typed), and deriving the carried `DenseConditions`; `Step` takes the derivative field as a `sample` function, so the one stepper integrates a scalar ODE, a spatial streamline, or any admitted carrier; `AdmitOrFixed` defaults an absent integrator to `Fixed(RK4)`.
- Auto: stage computation is one fold over the coupling rows; the adaptive arm binds the tableau's one optional embedded formula, reads the error from the delta between the primary and embedded combinations, rescales through the `StepControl` controller against the history the driver threads at the exponent `1/(q+1)` the embedded order fixes, and returns `RejectedCase` with the shrunk suggestion. `Method`, `Dense`, and `Interp` are ABSTRACT columns both cases carry, so no dispatch discriminates on a fold whose arms do not differ, and `Admit` returns the value — a `RungeKuttaIntegrator` in hand IS admitted, its case constructors internal and its factories having proved everything once.
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
    private readonly ButcherTableau tableau;
    private readonly DenseFormula family;
    private readonly DenseOutput.DenseInterpolant interpolant;
    private readonly IntegrationModule<TState, TDelta> module;
    public DenseConditions Conditions { get; }

    private DenseOutputSpan(
        TState start, double step, Seq<TDelta> stages, ButcherTableau tableau,
        DenseFormula family, DenseOutput.DenseInterpolant interpolant,
        DenseConditions conditions, IntegrationModule<TState, TDelta> module) =>
        (this.start, this.step, this.stages, this.tableau, this.family, this.interpolant, Conditions, this.module) =
            (start, step, stages, tableau, family, interpolant, conditions, module);
    internal static Fin<DenseOutputSpan<TState, TDelta>> Of(IntegrationModule<TState, TDelta> module, TState start, double step, Seq<TDelta> stages, ButcherTableau tableau, DenseFormula family, DenseOutput.DenseInterpolant interpolant, DenseConditions conditions) =>
        DenseOutput.WeightsAt(tableau, family, interpolant, 1.0).Bind(weights => {
            TDelta reconstructed = module.Combine(coefficients: weights, deltas: stages);
            TDelta declared = module.Combine(coefficients: tableau.Weights, deltas: stages);
            double drift = module.Norm(arg: module.Sum(arg1: reconstructed, arg2: module.Scale(arg1: -1.0, arg2: declared)));
            double stageScale = stages.Fold(initialState: 0.0, f: (max, delta) => Math.Max(val1: max, val2: module.Norm(arg: delta)));
            return double.IsFinite(drift) && drift <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: stageScale)
                && stages.Count == tableau.StageCount && Math.Abs(value: step) > EpsilonPolicy.ZeroTolerance && conditions.IsValid
                ? Fin.Succ(new DenseOutputSpan<TState, TDelta>(start, step, stages, tableau, family, interpolant, conditions, module))
                : Fin.Fail<DenseOutputSpan<TState, TDelta>>(new KernelFault.InvalidResult());
        });
    public Fin<TState> PointAt(double theta) {
        if (!double.IsFinite(theta) || theta is < 0.0 or > 1.0) return Fin.Fail<TState>(new KernelFault.InvalidInput());
        DenseOutputSpan<TState, TDelta> self = this;
        return DenseOutput.WeightsAt(self.tableau, self.family, self.interpolant, theta, key)
            .Map(weights => self.module.Add(arg1: self.start, arg2: self.step, arg3: self.module.Combine(coefficients: weights, deltas: self.stages)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union]
public abstract partial record RungeKuttaIntegrator {
    public sealed record FixedCase : RungeKuttaIntegrator {
        internal FixedCase(RungeKuttaMethod method, DenseConditions dense, DenseOutput.DenseInterpolant interp) { Method = method; Dense = dense; Interp = interp; }
        public override RungeKuttaMethod Method { get; }
        internal override DenseConditions Dense { get; }
        internal override DenseOutput.DenseInterpolant Interp { get; }
    }
    public sealed record AdaptiveCase : RungeKuttaIntegrator {
        internal AdaptiveCase(RungeKuttaMethod method, PositiveMagnitude tolerance, StepControl control, DenseConditions dense, DenseOutput.DenseInterpolant interp) {
            Method = method; Tolerance = tolerance; Control = control; Dense = dense; Interp = interp;
        }
        public override RungeKuttaMethod Method { get; }
        public PositiveMagnitude Tolerance { get; }
        public StepControl Control { get; }
        internal override DenseConditions Dense { get; }
        internal override DenseOutput.DenseInterpolant Interp { get; }
    }
    private RungeKuttaIntegrator() { }
    public static Fin<RungeKuttaIntegrator> Fixed(RungeKuttaMethod method) =>
        from active in Optional(method).ToFin(new KernelFault.InvalidInput())
        from tableau in active.Tableau.Admit(active.Conditions, key)
        from fixedKind in guard(!active.Tableau.Embedded.IsSome, new KernelFault.Unsupported(InputType: active.GetType(), OutputType: typeof(FixedCase)))
        from interp in DenseOutput.Interpolant(tableau: tableau, family: active.Formula)
        from dense in DenseOutput.Conditions(tableau, active.Formula, interp, key)
        select (RungeKuttaIntegrator)new FixedCase(method: active, dense: dense, interp: interp);
    public static Fin<RungeKuttaIntegrator> Adaptive(RungeKuttaMethod method, double tolerance, Option<StepControl> control = default) {
        StepControl controller = control.IfNone(StepControl.Default);
        return from active in Optional(method).ToFin(new KernelFault.InvalidInput())
               from tableau in active.Tableau.Admit(active.Conditions, key)
               from adaptiveKind in guard(active.Tableau.Embedded.IsSome, new KernelFault.Unsupported(InputType: active.GetType(), OutputType: typeof(AdaptiveCase)))
               from admitted in guard(controller.IsValid, new KernelFault.InvalidValue(Label: nameof(StepControl), Requirement: "finite positive safety factor, ordered positive scale clamps, nonnegative reject budget, a step controller"))
               from validated in FactoryBridge.Accept<PositiveMagnitude>(candidate: tolerance)
               from interp in DenseOutput.Interpolant(tableau: tableau, family: active.Formula)
               from dense in DenseOutput.Conditions(tableau, active.Formula, interp, key)
               select (RungeKuttaIntegrator)new AdaptiveCase(method: active, tolerance: validated, control: controller, dense: dense, interp: interp);
    }
    public abstract RungeKuttaMethod Method { get; }
    internal abstract DenseConditions Dense { get; }
    internal abstract DenseOutput.DenseInterpolant Interp { get; }
    public int RejectBudget => Switch(state: 0, fixedCase: static (s, _) => s, adaptiveCase: static (_, c) => c.Control.RejectBudget);
    public int MethodOrder => Method.Tableau.MethodOrder;
    public Option<int> EmbeddedOrder => Method.Tableau.Embedded.Map(static pair => pair.Order);
    public static Fin<RungeKuttaIntegrator> Admit(RungeKuttaIntegrator value) =>
        Optional(value).ToFin(new KernelFault.InvalidInput());
    public static Fin<RungeKuttaIntegrator> AdmitOrFixed(Option<RungeKuttaIntegrator> value) =>
        value.Match(Some: Fin.Succ, None: () => Fixed(method: RungeKuttaMethod.RK4));
    public Fin<IntegrationStep<TState, TDelta>> Step<TState, TDelta>(IntegrationModule<TState, TDelta> module, Func<TState, Fin<TDelta>> sample, TState state, double h, Option<StepHistory> history = default) => Switch(
        state: (Module: module, Sample: sample, State: state, H: h, History: history),
        fixedCase: static (s, c) =>
            from ks in Stages(module: s.Module, sample: s.Sample, tableau: c.Method.Tableau, state: s.State, h: s.H)
            let next = s.Module.Add(arg1: s.State, arg2: s.H, arg3: s.Module.Combine(coefficients: c.Method.Tableau.Weights, deltas: ks))
            from dense in DenseOutputSpan<TState, TDelta>.Of(module: s.Module, start: s.State, step: s.H, stages: ks, tableau: c.Method.Tableau, family: c.Method.Formula, interpolant: c.Interp, conditions: c.Dense)
            select (IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.AcceptedCase(Next: next, SuggestedStep: s.H, Error: Option<double>.None, Dense: dense),
        adaptiveCase: static (s, c) =>
            from embedded in c.Method.Tableau.Embedded.ToFin(new KernelFault.InvalidInput())
            from ks in Stages(module: s.Module, sample: s.Sample, tableau: c.Method.Tableau, state: s.State, h: s.H)
            let primary = s.Module.Combine(coefficients: c.Method.Tableau.Weights, deltas: ks)
            let secondary = s.Module.Combine(embedded.Weights, ks)
            let err = Math.Abs(value: s.H) * s.Module.Norm(arg: s.Module.Sum(arg1: primary, arg2: s.Module.Scale(arg1: -1.0, arg2: secondary)))
            let scale = c.Control.Rescale(s.History, err, c.Tolerance.Value, 1.0 / (embedded.Order + 1.0))
            let next = s.Module.Add(s.State, s.H, primary)
            from result in err <= c.Tolerance.Value
                ? DenseOutputSpan<TState, TDelta>.Of(module: s.Module, start: s.State, step: s.H, stages: ks, tableau: c.Method.Tableau, family: c.Method.Formula, interpolant: c.Interp, conditions: c.Dense)
                    .Map(dense => (IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.AcceptedCase(next, s.H * scale, Some(err), dense))
                : Fin.Succ((IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.RejectedCase(s.H * scale, err))
            select result);
    private static Fin<Seq<TDelta>> Stages<TState, TDelta>(IntegrationModule<TState, TDelta> module, Func<TState, Fin<TDelta>> sample, ButcherTableau tableau, TState state, double h) =>
        tableau.Coupling.Fold(
            initialState: Fin.Succ((Seq<TDelta>)[]),
            f: (acc, row) => acc.Bind(ks =>
                sample(arg: module.Add(arg1: state, arg2: h, arg3: module.Combine(coefficients: row, deltas: ks)))
                    .Map(k => ks.Add(k))));
}
```

## [05]-[QUADRATURE]

- Owner: `QuadratureRoute` the `[SmartEnum<string>]` accuracy axis carrying each kernel's estimate delegate — value beside the optional error and L1 channels; `QuadratureDomain` the `[Union]` arity axis over genuine 1-D/2-D/3-D integrands, the Smolyak sparse grid, and the reference element — line, simplex, cube, prism, and pyramid rows alike; `ReferenceElement` the `[SmartEnum<string>]` row family whose rows carry the owned-build Gauss tables per reference domain — triangle/tet area-volume coordinates, `[-1,1]` cube tensor Gauss, triangle⊗line prism, conical pyramid; `IntegrationInterval` the bound value whose values alone encode finite or infinite extent; `Quadrature.Integrate` the one entry whose finite-guard-then-admit combinator reads the generated `Evaluate` delegate column once.
- Cases: `QuadratureRoute` rows `DoubleExponential` · `GaussLegendre` · `GaussKronrod`; `QuadratureDomain` cases `Line` · `Rectangle` · `Cuboid` · `SparseGrid` · `Reference`; `ReferenceElement` rows `Line` · `Tri` · `Tet` · `Quad` · `Hex` · `Wedge` · `Pyramid`, each electing the smallest owned rule at or above a POSITIVE requested order over a roster the generated `ValidateConstructorArguments` hook sorts ascending and de-duplicates by exactness once at construction, and each row's declared order the TRUE exactness of its construction — `Order` is polynomial EXACTNESS, never node count — an `n`-point Gauss-Legendre leg carries `2n−1` — a prism is exact to `min(triangle degree, 2n−1)`, never the sum of its legs, and a conical product to the weaker of its base leg `2n−1` and its height leg `2m−3`, the `(1−z)²` collapse Jacobian costing the height leg two degrees; `Pyramid12` therefore carries three, and a second rung raising only the base leg earns no exactness and is the deleted form. `QuadratureRule` rows stay DISTINCT-BY-DESIGN over the classical quadrature families — the upstream is the Gauss-Legendre nodes and the published symmetric simplex rules, and each row carries its point/weight table as DATA built once at type init, the tensor, prism, and conical rows deriving from the three canonical 1-D Gauss node sets and the simplex rows from their published closed forms spelled as exact expressions.
- Entry: `Quadrature.Integrate(QuadratureDomain domain, Option<QuadratureControl> control = default)` — arity is the case, accuracy the route row, and the reference-element table the `Reference` case's election; no sibling integrator entry exists.
- Auto: each arm wraps its integrand in a skip-counting guard because no MathNet route inspects returns and a pole poisons the weighted sum silently — `QuadratureControl.MaxSkipped` makes that loss budget explicit and defaults to zero; `Try.lift` is the one inbound exception funnel over the whole dispatch, so an integrand or MathNet raise surfaces on the typed result with `KernelFault.Cancelled` keeping its own identity; the `Line` arm admits any NaN-free ascending interval, signed infinities included, because every MathNet facade it routes through substitutes an infinite limit itself, while rectangle, cuboid, and sparse-grid bounds refuse non-finite endpoints through `FiniteOrdered`; only `GaussKronrod` returns the `error`/`L1Norm` channel, so the error-budget and cancellation gates bind only where the channels are `Some`; `SparseGrid` folds the nested Clenshaw-Curtis combination formula — its signed coefficients through `SpecialFunctions.Binomial`, never a hand-rolled fold — through the private nested `Quadrature.Smolyak.Integrate` under `MaxSparseLevel`; `Reference` folds the elected reference-element table — the reference-domain integral, the consumer weighting each point by its own Jacobian at the isoparametric map it owns.
- Law: a rule ladder that runs out REFUSES. `ReferenceElement.Rule` returns `Fin<QuadratureRule>` and faults typed when the requested order is nonpositive or exceeds every owned rule, computing the normalized ladder's terminal order locally and carrying it in the `KernelFault.OutOfRange` it returns, so no success-shaped under-integration ever reaches a caller who asked for a higher order; the constructor hook normalizes and never throws, so exhaustion — an accidentally empty roster included — stays inside the `Fin` carrier. The sixteen `QuadratureRule` tables are internal: `Rule` is the one election and no consumer bypasses its typed exhaustion proof.
- Exemption: the Gauss/simplex table builders hold `List<(double, double, double, double)>` accumulators and raw `double[]` node/weight pairs inside the nested `Smolyak`; both are statement kernels — the first runs once at type init and freezes into `ImmutableArray`, the second is the per-call tensor-node walk the sparse fold streams — and neither escapes its owner.
- Output: `QuadratureEvidence` carries `Option<double>` error, L1, and ratio so a non-adaptive route reports honest absence, never a fabricated `NaN`; the skip count rides it, never silently as coverage; the gate is three-tier — non-finite rejects, an adaptive error estimate over `max(AbsoluteError, RelativeError·|value|)` rejects, and a cancellation ratio breaching the floor rejects — never a rejection on slow convergence alone.
- Packages: MathNet.Numerics (`Integrate.DoubleExponential`/`GaussLegendre`/`GaussKronrod`/`OnRectangle`/`OnCuboid`, `SpecialFunctions.Binomial` for the Smolyak combination coefficients), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new accuracy kernel is one `QuadratureRoute` row with its delegate; a new arity is one `QuadratureDomain` case; a new reference domain or a higher-order rule is one `ReferenceElement` row or one entry on its rule ladder, and the ladder entry lifts every consumer already declaring that order — a prism or conical rung climbs only through its WEAKER leg (a degree-five pyramid rung raises the height leg to `Gauss4` at least), since the derived `min`/base-direction order makes a longer strong leg buy nothing the construction-time sort-and-distinct would rank; a new sparse-grid 1-D rule family or dimension-adaptive refinement is a policy row on the nested `Smolyak` — zero new surface.
- Boundary: accuracy is the primary decision with order secondary — the three MathNet kernels bind as route rows, never sibling factories, and the finite-guard-then-admit combinator applies once over the uniform `(Value, Error, L1Norm)` estimate tuple — a transient transit shape that never crosses `Quadrature.Integrate`, where `QuadratureEvidence` is the one named result; the line domain owns infinite extent STRUCTURALLY — `Integrate.DoubleExponential`, `GaussLegendre`, and `GaussKronrod` are public facades that each substitute either or both infinite limits, so no route column could discriminate and none exists, and a capability flag whose rows all agree was the deleted form; any 1-D delegate forced through a 2-D rule integrates `(b−a)·∫f` and is rejected; `error`/`L1Norm`/`Ratio` are `Option<double>` because only the adaptive Kronrod row yields them, and the presence of `Error` IS the convergence witness the admission gate reads — no parallel claim column can contradict the estimate it would classify — so `RequireErrorWitness` defaults true and an unwitnessed route is an explicit opt-out rather than a success indistinguishable from a converged one; the reference-element tables integrate the REFERENCE domain — the physical mapping, its Jacobian, and the isoparametric basis stay the consuming element's, so this owner never learns an element topology; a consumer calling `Integrate.GaussLegendre` raw skips the finite guard, the skip budget, and the typed evidence and is the deleted form.

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
    internal static readonly QuadratureRule Tet4 = new(2, [.. Simplex3(ab: [(5.0 + (3.0 * Math.Sqrt(5.0))) / 20.0, (5.0 - Math.Sqrt(5.0)) / 20.0])]);
    internal static readonly QuadratureRule Quad1 = TensorCube(dim: 2, line: Gauss1);
    internal static readonly QuadratureRule Quad4 = TensorCube(dim: 2, line: Gauss2);
    internal static readonly QuadratureRule Quad9 = TensorCube(dim: 2, line: Gauss3);
    internal static readonly QuadratureRule Hex1 = TensorCube(dim: 3, line: Gauss1);
    internal static readonly QuadratureRule Hex8 = TensorCube(dim: 3, line: Gauss2);
    internal static readonly QuadratureRule Hex27 = TensorCube(dim: 3, line: Gauss3);
    internal static readonly QuadratureRule Wedge6 = PrismProduct(tri: Tri3, line: Gauss2);
    internal static readonly QuadratureRule Wedge21 = PrismProduct(tri: Tri7, line: Gauss3);
    internal static readonly QuadratureRule Pyramid12 = Conical(baseLine: Gauss2, heightLine: Gauss3);

    private static IEnumerable<(double, double, double, double)> Simplex3(double[] ab) {
        (double a, double b) = (ab[0], ab[1]);
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
    double Value, Option<double> Error, Option<double> L1Norm, Option<double> Ratio, int Skipped);

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
                       : Counted(run: guard => Admit(outcome: l.Route.Evaluate(x => guard.Finite(l.F(x)), l.Bounds.Lower, l.Bounds.Upper, ctl), skipped: guard.Skipped, ctl: ctl)),
                   rectangle: r => !FiniteOrdered(bounds: r.X) || !FiniteOrdered(bounds: r.Y) || r.Order <= 0
                       ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(Label: nameof(QuadratureDomain.Rectangle), Requirement: FiniteOrderedRequirement))
                       : Counted(run: guard => Admit(outcome: (Value: Integrate.OnRectangle((x, y) => guard.Finite(r.F(x, y)), r.X.Lower, r.X.Upper, r.Y.Lower, r.Y.Upper, r.Order), Error: Option<double>.None, L1Norm: Option<double>.None), skipped: guard.Skipped, ctl: ctl)),
                   cuboid: c => !FiniteOrdered(bounds: c.X) || !FiniteOrdered(bounds: c.Y) || !FiniteOrdered(bounds: c.Z) || c.Order <= 0
                       ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(Label: nameof(QuadratureDomain.Cuboid), Requirement: FiniteOrderedRequirement))
                       : Counted(run: guard => Admit(outcome: (Value: Integrate.OnCuboid((x, y, z) => guard.Finite(c.F(x, y, z)), c.X.Lower, c.X.Upper, c.Y.Lower, c.Y.Upper, c.Z.Lower, c.Z.Upper, c.Order), Error: Option<double>.None, L1Norm: Option<double>.None), skipped: guard.Skipped, ctl: ctl)),
                   sparseGrid: s => s.Bounds.Count is < 2 or > 20 || s.Level <= 0 || s.Level > ctl.MaxSparseLevel || s.Bounds.Exists(static b => !FiniteOrdered(bounds: b))
                       ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(Label: nameof(QuadratureDomain.SparseGrid), Requirement: "2-20 finite dimensions inside the level budget"))
                       : Counted(run: guard => Admit(outcome: Smolyak.Integrate(f: x => guard.Finite(s.F(x)), bounds: s.Bounds, level: s.Level), skipped: guard.Skipped, ctl: ctl)),
                   reference: x => x.Order <= 0
                       ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(Label: nameof(QuadratureDomain.Reference), Requirement: "a positive rule order"))
                       : x.Element.Rule(order: x.Order).Bind(rule =>
                           Counted(run: guard => {
                               double sum = 0.0;
                               foreach ((double px, double py, double pz, double weight) in rule.Points) sum += weight * guard.Finite(x.F(px, py, pz));
                               return Admit(outcome: (Value: sum, Error: Option<double>.None, L1Norm: Option<double>.None), skipped: guard.Skipped, ctl: ctl);
                           })))).Run().Bind(static inner => inner)
               select evidence;
    }

    private static Fin<QuadratureEvidence> Counted(Func<SkipCounter, Fin<QuadratureEvidence>> run) => run(arg: new SkipCounter());

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
        : outcome.Error is { IsSome: true, Case: double estimate }
          && estimate > Math.Max(val1: ctl.AbsoluteError, val2: ctl.RelativeError * Math.Abs(value: outcome.Value))
            ? Fin.Fail<QuadratureEvidence>(new KernelFault.OutOfRange(Label: "error-over-budget", Scalar: estimate, Requirement: $"<= {Math.Max(val1: ctl.AbsoluteError, val2: ctl.RelativeError * Math.Abs(value: outcome.Value)):e3}"))
        : outcome.L1Norm.Map(l1 => l1 == 0.0 && outcome.Value == 0.0 ? 1.0 : Math.Abs(value: outcome.Value / l1)).Match(
            Some: ratio => double.IsFinite(ratio) && ratio >= ctl.CancellationFloor
                ? Fin.Succ(new QuadratureEvidence(Value: outcome.Value, Error: outcome.Error, L1Norm: outcome.L1Norm, Ratio: Some(ratio), Skipped: skipped))
                : Fin.Fail<QuadratureEvidence>(new KernelFault.OutOfRange(Label: "cancellation-breach", Scalar: ratio, Requirement: $">= {ctl.CancellationFloor:e3}")),
            None: () => Fin.Succ(new QuadratureEvidence(Value: outcome.Value, Error: outcome.Error, L1Norm: None, Ratio: None, Skipped: skipped)));

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

- [01]-[INTEGRATOR_ROWS]: `[SmartEnum<int>]` — the tableau IS the row.
- [02]-[COEFFICIENT_CARRIER]: order-condition-validated record + `OrderConditions` `ValidityClaim.All` tally over the `RootedTree` walk.
- [03]-[CONTINUOUS_EXTENSION]: exact-rational tables + moment-fit fallback via `matrix.md`; carriers `DenseFormula` (its `Published` option the source discriminant) · `DenseConditions` · `DenseOutput` (`Conditions` · `WeightsAt`).
- [04]-[STEP_ALGEBRA]: carrier-generic policy records + `[Union]` stepper — `IntegrationModule<TState,TDelta>` (THE `Combine`) with `StepControl` · `RungeKuttaIntegrator` · `IntegrationStep` · `DenseOutputSpan`.
- [05]-[ACCURACY_ROUTE]: `[SmartEnum<string>]` rows carrying the MathNet facade delegate, every row admitting infinite line limits through the facade substitution; the `(Value, Error, L1Norm)` estimate tuple is the uniform transit the admission gate reads, and `Error` presence is its witness.
- [06]-[INTEGRATION_ARITY]: `[Union]` cases over 1-D/2-D/3-D integrands, the Smolyak sparse grid, and the reference element; `IntegrationInterval` and `QuadratureControl`/`QuadratureEvidence` ride the same axis.
- [07]-[REFERENCE_DOMAIN]: seven `[SmartEnum<string>]` rows over sixteen `QuadratureRule` tables, each row electing the smallest owned rule at or above a positive request and refusing typed past the normalized ladder's terminal order `Rule` computes locally; the private nested `Quadrature.Smolyak` is the nested-rule algorithm the `SparseGrid` case alone reaches.

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
