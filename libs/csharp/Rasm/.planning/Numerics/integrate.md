# [RASM_NUMERICS_INTEGRATE]

`Rasm.Numerics` integration is the ODE/Runge-Kutta floor — pure numerics, zero geometric content. Every Butcher tableau admits by VALIDATING its order conditions numerically rather than asserting them, so a mis-transcribed coefficient is a construction-time typed failure, never a silently wrong trajectory; one carrier-generic `Step` serves scalar, vector, and geometric state, and continuous dense output localizes events without re-tracing. Geometry enters only at the `Processing/flow` consumer that supplies the spatial module; this page never names a geometric type.

`IntegrationModule.Combine` is the corpus' single linear-combination site; `FieldIntegrator` factories derive each dense-output receipt once at admission so the moment-fit least-squares never runs inside the step loop, and the reject loop stays the `Processing/flow` consumer's fold while the stepper stays a pure step function. Dense continuous-extension fallback solves its moment fit through the `matrix.md` least-squares route (`Matrix`, `SolveReceipt`); every moment sum accumulates in 106-bit `ddouble`, keeping the fold's rounding far below the residual band it witnesses.

## [01]-[INDEX]

- [02]-[TABLEAU_VOCABULARY]: `IntegratorKind` tableau rows, the `RootedTree` Butcher-tree order algebra, and the order-condition-validated `ButcherTableau` carrier with `ButcherOrderReceipt`.
- [03]-[DENSE_OUTPUT]: exact-rational interpolant families and the `ButcherDenseOutput` derivation, moment-fit fallback via the `matrix.md` least-squares route.
- [04]-[STEPPER]: carrier-generic `IntegrationModule` step algebra, `StepControl`, and the `FieldIntegrator` Fixed/Adaptive union.
- [05]-[QUADRATURE]: accuracy-routed `QuadratureRoute` rows over the `IntegrationDomain` arity union, the `ReferenceElement` row family with its owned Gauss tables, and the finite-guard-then-admit `Quadrature.Integrate` entry with `QuadratureEvidence`.

## [02]-[TABLEAU_VOCABULARY]

- Owner: `IntegratorKind` mints the `[SmartEnum<int>]` whose rows ARE the Butcher tableaux, each a single declaration through the private `Fixed`/`Adaptive` factories; `RootedTree` is the Butcher-tree algebra — order the node count, density `γ(t) = |t|·Πγ(children)`, elementary weight `Φᵢ(t)` off the coupling matrix — whose condition set `Σᵢ bᵢΦᵢ(t) = 1/γ(t)` over every tree of order ≤ p IS the order-p proof, uncapped; `ButcherTableau.IsValid` runs that full walk at the declared order rather than asserting it, so a mis-transcribed coefficient is a construction-time typed failure, never a silently wrong trajectory, and `VerifiedOrder` DERIVES the largest certified order for both weight rows; `ButcherOrderReceipt` carries the walk's evidence.
- Cases: `Euler` · `Heun` · `Midpoint` · `Ralston` · `RK4` · `RK38` fixed; `BogackiShampine` · `CashKarp` · `DormandPrince` embedded-adaptive.
- Entry: `IntegratorKind.<Row>.Tableau` reads the validated carrier; `ButcherTableau.Admit` gates a tableau onto the rail; `IsFunctionalSameAsLast` detects the FSAL structure that fingerprints the method-specific dense-output families.
- Auto: abscissae never enter as data — the factories derive them as coupling row sums, so the consistency condition holds by construction and `IsValid` re-checks it as the transcription witness; `AdaptiveExponent` derives the step-control exponent from the embedded order; the tree pool per order generates once through `OfOrder`, so a fifth-order pair proves seventeen conditions with zero condition code.
- Receipt: `ButcherOrderReceipt` folds its validity through `ValidityClaim.All` under the semantic gate `FailedConditionCount == 0 && MaxResidual <= CoefficientTolerance`; `CheckedConditionCount` is the tree census at the declared order, so an order-5 row proves every one of its seventeen elementary-weight conditions, never the first four moments alone.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Seq`, `Option`, `Fin`), TYoshimura.DoubleDouble (`ddouble` 106-bit moment accumulation).
- Growth: a new integrator is ONE `IntegratorKind` row — the rooted-tree walk generates every order condition at every order, so a higher-order tableau validates with zero new condition code; a per-order condition roster is the deleted form.
- Boundary: `CoefficientTolerance` is the tableau's own order-condition residual band — exact-rational coefficients evaluate near machine epsilon, so the band catches transcription errors, not roundoff; tableau data lives ONLY on the vocabulary rows, and a consumer never spells a coupling coefficient; the recursive tree enumeration and elementary-weight loops are the named statement kernel.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Immutable;
using System.Numerics;
using DoubleDouble;
using Rasm.Domain;

namespace Rasm.Numerics;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class IntegratorKind {
    public static readonly IntegratorKind Euler = Fixed(key: 0, order: 1, coupling: [[]], weights: [1.0]);
    public static readonly IntegratorKind Heun = Fixed(key: 1, order: 2, coupling: [[], [1.0]], weights: [0.5, 0.5]);
    public static readonly IntegratorKind Midpoint = Fixed(key: 2, order: 2, coupling: [[], [0.5]], weights: [0.0, 1.0]);
    public static readonly IntegratorKind Ralston = Fixed(key: 3, order: 2, coupling: [[], [2.0 / 3.0]], weights: [0.25, 0.75]);
    public static readonly IntegratorKind RK4 = Fixed(key: 4, order: 4,
        coupling: [[], [0.5], [0.0, 0.5], [0.0, 0.0, 1.0]],
        weights: [1.0 / 6.0, 1.0 / 3.0, 1.0 / 3.0, 1.0 / 6.0]);
    public static readonly IntegratorKind RK38 = Fixed(key: 5, order: 4,
        coupling: [[], [1.0 / 3.0], [-1.0 / 3.0, 1.0], [1.0, -1.0, 1.0]],
        weights: [1.0 / 8.0, 3.0 / 8.0, 3.0 / 8.0, 1.0 / 8.0]);
    public static readonly IntegratorKind BogackiShampine = Adaptive(key: 6, order: 3, embeddedOrder: 2,
        coupling: [[], [0.5], [0.0, 0.75], [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0]],
        weights: [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0, 0.0],
        errorWeights: [7.0 / 24.0, 0.25, 1.0 / 3.0, 1.0 / 8.0]);
    public static readonly IntegratorKind CashKarp = Adaptive(key: 7, order: 5, embeddedOrder: 4,
        coupling: [[], [0.2], [3.0 / 40.0, 9.0 / 40.0], [0.3, -0.9, 1.2],
            [-11.0 / 54.0, 2.5, -70.0 / 27.0, 35.0 / 27.0],
            [1631.0 / 55296.0, 175.0 / 512.0, 575.0 / 13824.0, 44275.0 / 110592.0, 253.0 / 4096.0]],
        weights: [37.0 / 378.0, 0.0, 250.0 / 621.0, 125.0 / 594.0, 0.0, 512.0 / 1771.0],
        errorWeights: [2825.0 / 27648.0, 0.0, 18575.0 / 48384.0, 13525.0 / 55296.0, 277.0 / 14336.0, 0.25]);
    public static readonly IntegratorKind DormandPrince = Adaptive(key: 8, order: 5, embeddedOrder: 4,
        coupling: [[], [1.0 / 5.0], [3.0 / 40.0, 9.0 / 40.0],
            [44.0 / 45.0, -56.0 / 15.0, 32.0 / 9.0],
            [19372.0 / 6561.0, -25360.0 / 2187.0, 64448.0 / 6561.0, -212.0 / 729.0],
            [9017.0 / 3168.0, -355.0 / 33.0, 46732.0 / 5247.0, 49.0 / 176.0, -5103.0 / 18656.0],
            [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0]],
        weights: [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0, 0.0],
        errorWeights: [5179.0 / 57600.0, 0.0, 7571.0 / 16695.0, 393.0 / 640.0, -92097.0 / 339200.0, 187.0 / 2100.0, 1.0 / 40.0]);
    public ButcherTableau Tableau { get; }
    internal bool IsAdaptive => Tableau.EmbeddedWeights.IsSome;
    internal double AdaptiveExponent => Tableau.EmbeddedOrder.Map(static order => 1.0 / (order + 1.0)).IfNone(0.2);
    private static IntegratorKind Fixed(int key, int order, double[][] coupling, double[] weights) =>
        new(key: key, tableau: new ButcherTableau(Coupling: toSeq(coupling.Select(static r => toSeq(r))), Abscissae: toSeq(coupling.Select(static r => r.Sum())), Weights: toSeq(weights), EmbeddedWeights: Option<Seq<double>>.None, MethodOrder: order, EmbeddedOrder: Option<int>.None));
    private static IntegratorKind Adaptive(int key, int order, int embeddedOrder, double[][] coupling, double[] weights, double[] errorWeights) =>
        new(key: key, tableau: new ButcherTableau(Coupling: toSeq(coupling.Select(static r => toSeq(r))), Abscissae: toSeq(coupling.Select(static r => r.Sum())), Weights: toSeq(weights), EmbeddedWeights: Some(toSeq(errorWeights)), MethodOrder: order, EmbeddedOrder: Some(embeddedOrder)));
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct ButcherTableau(Seq<Seq<double>> Coupling, Seq<double> Abscissae, Seq<double> Weights, Option<Seq<double>> EmbeddedWeights, int MethodOrder, Option<int> EmbeddedOrder) {
    internal const double CoefficientTolerance = 1.0e-9;
    internal int StageCount => Weights.Count;
    // FSAL fingerprint — the structure the method-specific dense-output families key on.
    internal bool IsFunctionalSameAsLast =>
        StageCount > 1
        && Coupling.Count == StageCount
        && Math.Abs(value: Abscissae[StageCount - 1] - 1.0) <= CoefficientTolerance
        && Math.Abs(value: Weights[StageCount - 1]) <= CoefficientTolerance
        && Coupling[StageCount - 1].Count == StageCount - 1
        && Coupling[StageCount - 1].Zip(Weights.Take(StageCount - 1)).ForAll(static pair => Math.Abs(value: pair.First - pair.Second) <= CoefficientTolerance);
    public ButcherOrderReceipt OrderReceipt => OrderReceiptOf(weights: Weights, order: MethodOrder, embeddedOrder: EmbeddedOrder);
    internal bool IsValid =>
        StageCount > 0
        && MethodOrder > 0
        && (EmbeddedOrder is not { IsSome: true, Case: int embedded } || (embedded > 0 && embedded < MethodOrder))
        && Coupling.Count == StageCount
        && Abscissae.Count == StageCount
        && Abscissae.ForAll(double.IsFinite)
        && CoefficientsMatch(values: Weights, expected: 1.0)
        && OrderReceipt.IsValid
        && Coupling.Zip(Abscissae).AsIterable().Select((pair, index) => pair.First.Count <= index
            && CoefficientsMatch(values: pair.First, expected: pair.Second)).All(static ok => ok)
        && (EmbeddedWeights is not { IsSome: true, Case: Seq<double> ew } || (ew.Count == StageCount && CoefficientsMatch(values: ew, expected: 1.0) && OrderReceiptOf(weights: ew, order: EmbeddedOrder.IfNone(1), embeddedOrder: EmbeddedOrder).IsValid));
    // DERIVED order — the largest p every tree condition of order <= p satisfies, uncapped by any condition
    // roster; the declared MethodOrder is validated against this walk, never asserted past it.
    public int VerifiedOrder => RootedTree.VerifiedOrder(a: CouplingMatrix(), b: [.. Weights]);
    internal Fin<ButcherTableau> Admit(Op key) => IsValid ? Fin.Succ(this) : Fin.Fail<ButcherTableau>(key.InvalidInput());
    internal Fin<DenseOutputReceipt> DenseOutputReceipt(Op key) => ButcherDenseOutput.Receipt(tableau: this, key: key);
    internal Fin<Seq<double>> DenseWeightsAt(double theta, Op key) => ButcherDenseOutput.WeightsAt(tableau: this, theta: theta, key: key);
    private static bool CoefficientsMatch(Seq<double> values, double expected) =>
        values.ForAll(double.IsFinite)
        && Math.Abs(value: values.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - expected) <= CoefficientTolerance;
    // VerifiedOrder runs the FULL elementary-weight walk at the declared order: every rooted tree of order <= p contributes one
    // condition Σᵢ bᵢΦᵢ(t) = 1/γ(t), so the moment conditions are the one-chain subfamily and an order-5 pair
    // proves seventeen conditions where a hand-kept Check roster proved four.
    private ButcherOrderReceipt OrderReceiptOf(Seq<double> weights, int order, Option<int> embeddedOrder) {
        double[,] a = CouplingMatrix();
        double[] b = [.. weights];
        (int Count, int Failed, double Max) state = (0, 0, 0.0);
        for (int p = 1; p <= order; p++) {
            foreach (RootedTree tree in RootedTree.OfOrder(order: p)) {
                double[] phi = tree.Weight(a: a, stages: StageCount);
                ddouble lhs = 0.0;
                for (int i = 0; i < StageCount; i++) lhs += (ddouble)b[i] * phi[i];
                double residual = Math.Abs(value: (double)lhs - (1.0 / tree.Density));
                state = (
                    Count: state.Count + 1,
                    Failed: state.Failed + (double.IsFinite(residual) && residual <= CoefficientTolerance ? 0 : 1),
                    Max: Math.Max(val1: state.Max, val2: residual));
            }
        }
        return new ButcherOrderReceipt(StageCount: StageCount, MethodOrder: order, EmbeddedOrder: embeddedOrder, CheckedConditionCount: state.Count, FailedConditionCount: state.Failed, MaxResidual: state.Max);
    }
    // THE one moment fold — Σwᵢ·cᵢ^power; the dense-output moment residuals still read it.
    internal static double MomentSum(Seq<double> weights, Seq<double> against, int power) =>
        (double)weights.Zip(against).Fold(initialState: (ddouble)0.0, f: (sum, pair) => sum + ((ddouble)pair.First * Math.Pow(x: pair.Second, y: power)));
    private double[,] CouplingMatrix() {
        double[,] a = new double[StageCount, StageCount];
        int row = 0;
        foreach (Seq<double> coupling in Coupling) {
            int col = 0;
            foreach (double coefficient in coupling) a[row, col++] = coefficient;
            row++;
        }
        return a;
    }
}

// Butcher-tree algebra: a rooted tree is a multiset of subtrees, its order the node count, its density
// γ(t) = |t|·Π γ(children); the order conditions Σᵢ bᵢΦᵢ(t) = 1/γ(t) over every tree of order ≤ p are the
// verified order. One algebra serves validation and derivation, so no consumer holds a parallel RK proof stack.
public sealed record RootedTree(ImmutableArray<RootedTree> Children) {
    public static readonly RootedTree Leaf = new(ImmutableArray<RootedTree>.Empty);

    public int Order => 1 + Children.Sum(static child => child.Order);
    public double Density => Order * Children.Aggregate(seed: 1.0, func: static (acc, child) => acc * child.Density);

    public static int VerifiedOrder(double[,] a, double[] b) {
        int stages = b.Length;
        int order = 0;
        for (int p = 1; p <= stages; p++) {
            bool holds = OfOrder(order: p).All(tree => {
                double[] phi = tree.Weight(a: a, stages: stages);
                double lhs = 0.0;
                for (int i = 0; i < stages; i++) lhs += b[i] * phi[i];
                return Math.Abs(value: lhs - (1.0 / tree.Density)) <= ButcherTableau.CoefficientTolerance;
            });
            if (!holds) break;
            order = p;
        }
        return order;
    }

    // Elementary weight Φᵢ(t): a single node weights 1 at every stage; a composite multiplies the stage-local
    // gᵢ = Σⱼ aᵢⱼ Φⱼ(child) over its child subtrees — so [τ] yields cᵢ and [[τ]] yields Σⱼ aᵢⱼ cⱼ.
    public double[] Weight(double[,] a, int stages) {
        double[] phi = new double[stages];
        Array.Fill(array: phi, value: 1.0);
        foreach (RootedTree child in Children) {
            double[] childWeight = child.Weight(a: a, stages: stages);
            for (int i = 0; i < stages; i++) {
                double g = 0.0;
                for (int j = 0; j < stages; j++) g += a[i, j] * childWeight[j];
                phi[i] *= g;
            }
        }
        return phi;
    }

    // All rooted trees of exactly `order` nodes: a root over a multiset of subtrees whose orders sum to order − 1,
    // chosen in non-decreasing pool index so each multiset is emitted once.
    public static IEnumerable<RootedTree> OfOrder(int order) {
        if (order <= 1) return [Leaf];
        ImmutableArray<RootedTree> pool = [.. Enumerable.Range(start: 1, count: order - 1).SelectMany(OfOrder)];
        return Forests(pool: pool, remaining: order - 1, start: 0).Select(static forest => new RootedTree(Children: forest));
    }

    private static IEnumerable<ImmutableArray<RootedTree>> Forests(ImmutableArray<RootedTree> pool, int remaining, int start) =>
        remaining == 0
            ? [ImmutableArray<RootedTree>.Empty]
            : Enumerable.Range(start: start, count: pool.Length - start)
                .Where(index => pool[index].Order <= remaining)
                .SelectMany(index => Forests(pool: pool, remaining: remaining - pool[index].Order, start: index).Select(rest => rest.Insert(index: 0, item: pool[index])));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ButcherOrderReceipt(int StageCount, int MethodOrder, Option<int> EmbeddedOrder, int CheckedConditionCount, int FailedConditionCount, double MaxResidual) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(StageCount >= 1 && MethodOrder >= 1 && CheckedConditionCount >= 0),
        ValidityClaim.Of(EmbeddedOrder.Map(static order => order >= 1).IfNone(noneValue: true)),
        ValidityClaim.CountExactly(count: FailedConditionCount, expected: 0),
        ValidityClaim.Nonnegative(value: MaxResidual),
        ValidityClaim.Of(MaxResidual <= ButcherTableau.CoefficientTolerance));
}
```

## [03]-[DENSE_OUTPUT]

- Owner: `DenseOutputCoefficientFamily` mints the `[SmartEnum<int>]` continuous-extension owner — `DormandPrinceShampine` and `BogackiShampine` carry their published EXACT-RATIONAL interpolant tables as row data, and `Identify` matches a tableau to its family by abscissae + FSAL fingerprint, falling to `GenericMomentFit`; `ButcherDenseOutput` proves the interpolant ONCE per integrator admission (moment residuals at θ ∈ {0, ½, 1}, endpoint value/derivative residuals, coefficient consistency against the step weights), and its generic fallback solves the least-squares moment fit through the `matrix.md` route so the interpolant construction leaves a `SolveReceipt` inside the dense-output evidence.
- Cases: `GenericMomentFit` · `DormandPrinceShampine` · `BogackiShampine`.
- Entry: consumers never reach the family directly — `tableau.DenseWeightsAt` and `tableau.DenseOutputReceipt` are the two entries, the family identified from the tableau fingerprint each call.
- Auto: the generic route pins the endpoints exactly and fits only the interior through the `θ(1−θ)`-scaled correction, so endpoint continuity is structural; `DenseOrderFor` caps the generic dense order at the distinct-abscissa count, the Vandermonde rank ceiling.
- Receipt: `DenseOutputReceipt` folds `ValidityClaim.All` coupling every residual to `CoefficientTolerance` and the family to its evidence shape — the endpoint set rides the one `EndpointResiduals` carrier whose `Derivatives` pair is `Some` exactly when the family is method-specific, a method-specific family carries no correction solve, and a generic receipt must carry its correction-solve `SolveReceipt`.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, `matrix.md` owners (`Matrix`, `SolveReceipt`).
- Growth: a new published interpolant is one `DenseOutputCoefficientFamily` row — fingerprint, order, table; a tableau without a published interpolant costs nothing, the generic moment fit covering it at the Vandermonde-rank order.
- Boundary: interpolant tables are exact rationals spelled as ratios, never decimal approximations — the moment validation flags the drift; dense output is the event-localization substrate `Processing/flow` binds for root bisection, and a consumer interpolating trajectories by chord instead of `b(θ)` re-derives a capability this owner already proves.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DenseOutputCoefficientFamily {
    public static readonly DenseOutputCoefficientFamily GenericMomentFit = new(key: 0, methodSpecific: false, fixedDenseOrder: 0, fingerprint: [], table: []);
    public static readonly DenseOutputCoefficientFamily DormandPrinceShampine = new(key: 1, methodSpecific: true, fixedDenseOrder: 4, fingerprint: DormandPrinceAbscissae, table: DormandPrinceTable);
    public static readonly DenseOutputCoefficientFamily BogackiShampine = new(key: 2, methodSpecific: true, fixedDenseOrder: 3, fingerprint: BogackiShampineAbscissae, table: BogackiShampineTable);
    public bool MethodSpecific { get; }
    public int FixedDenseOrder { get; }
    private double[] Fingerprint { get; }
    private double[][] Table { get; }
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
    internal static DenseOutputCoefficientFamily Identify(ButcherTableau tableau) =>
        toSeq(Items).Find(family => family.MethodSpecific && family.Matches(tableau)).IfNone(GenericMomentFit);
    internal Fin<Seq<double>> WeightsAt(double theta, int stageCount, Op key) => Evaluate(theta: theta, stageCount: stageCount, key: key, project: Horner);
    internal Fin<Seq<double>> DerivativeAt(double theta, int stageCount, Op key) => Evaluate(theta: theta, stageCount: stageCount, key: key, project: HornerDerivative);
    private bool Matches(ButcherTableau tableau) =>
        tableau.StageCount == Fingerprint.Length
        && tableau.IsFunctionalSameAsLast
        && Fingerprint.Zip(tableau.Abscissae).All(pair => Math.Abs(value: pair.First - pair.Second) <= ButcherTableau.CoefficientTolerance);
    private Fin<Seq<double>> Evaluate(double theta, int stageCount, Op key, Func<double[], double, double> project) {
        double[][] table = Table;
        return MethodSpecific && table.Length == stageCount
            ? key.Accept(values: table.Select(row => project(row, theta)))
            : Fin.Fail<Seq<double>>(key.InvalidInput());
    }
    private static double Horner(double[] row, double theta) =>
        theta * row.Reverse().Aggregate(seed: 0.0, func: (acc, coefficient) => (acc * theta) + coefficient);
    private static double HornerDerivative(double[] row, double theta) =>
        Enumerable.Range(start: 0, count: row.Length).Reverse().Aggregate(seed: 0.0, func: (acc, k) => (acc * theta) + ((k + 1) * row[k]));
}

// --- [MODELS] -----------------------------------------------------------------------------
// The endpoint-residual set is ONE named carrier, never five positional doubles of one kind on the receipt — a
// consumer reads the axis by name and a mint cannot mis-order the slots. The derivative pair is measured only by
// method-specific interpolants (the generic moment fit pins no endpoint derivative), so it rides ONE Option pair —
// absent measurement spells absence, never a fabricated 0.0 — and the tolerance coupling states itself once here.
[StructLayout(LayoutKind.Auto)]
public readonly record struct EndpointResiduals(double ValueLeft, double ValueRight, Option<(double Left, double Right)> Derivatives, double Coefficient) {
    public bool Nonnegative =>
        ValueLeft >= 0.0 && ValueRight >= 0.0 && Coefficient >= 0.0
        && Derivatives.Map(static pair => pair.Left >= 0.0 && pair.Right >= 0.0).IfNone(noneValue: true);
    public bool WithinTolerance(double tolerance) =>
        ValueLeft <= tolerance && ValueRight <= tolerance && Coefficient <= tolerance
        && Derivatives.Map(pair => pair.Left <= tolerance && pair.Right <= tolerance).IfNone(noneValue: true);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DenseOutputReceipt(int StageCount, int MethodOrder, int DenseOrder, int CheckedThetaCount, int CheckedConditionCount, int FailedConditionCount, double MaxResidual, bool UsesStageDerivatives, EndpointResiduals Endpoints, DenseOutputCoefficientFamily? CoefficientFamily = null, bool GenericCorrectionSolve = false, Option<SolveReceipt> CorrectionSolve = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(StageCount >= 1 && MethodOrder >= 1 && DenseOrder >= 0 && CheckedThetaCount >= 0),
        ValidityClaim.Of(DenseOrder <= MethodOrder),
        ValidityClaim.Of(CheckedConditionCount >= CheckedThetaCount),
        ValidityClaim.CountExactly(count: FailedConditionCount, expected: 0),
        ValidityClaim.Nonnegative(value: MaxResidual),
        ValidityClaim.Of(MaxResidual <= ButcherTableau.CoefficientTolerance),
        ValidityClaim.Of(UsesStageDerivatives),
        ValidityClaim.Of(Endpoints.Nonnegative),
        ValidityClaim.Of(Endpoints.WithinTolerance(ButcherTableau.CoefficientTolerance)),
        ValidityClaim.Of(CoefficientFamily is not null),
        ValidityClaim.Of(CoefficientFamily is null || CoefficientFamily.MethodSpecific == Endpoints.Derivatives.IsSome),
        ValidityClaim.Of(CoefficientFamily is null || GenericCorrectionSolve == CoefficientFamily.Equals(DenseOutputCoefficientFamily.GenericMomentFit)),
        ValidityClaim.Of(CoefficientFamily is null || !CoefficientFamily.MethodSpecific || CorrectionSolve.IsNone),
        ValidityClaim.Of(CoefficientFamily is null || CoefficientFamily.MethodSpecific || CheckedThetaCount < 3 || CorrectionSolve.IsSome),
        ValidityClaim.Of(CorrectionSolve.Map(static solve => solve.IsValid).IfNone(noneValue: true)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class ButcherDenseOutput {
    internal static Fin<DenseOutputReceipt> Receipt(ButcherTableau tableau, Op key) {
        DenseOutputCoefficientFamily family = DenseOutputCoefficientFamily.Identify(tableau: tableau);
        int order = DenseOrderFor(family: family, tableau: tableau);
        return ProbeAt(family: family, tableau: tableau, order: order, theta: 0.0, key: key).Bind(zero =>
            ProbeAt(family: family, tableau: tableau, order: order, theta: 0.5, key: key).Bind(mid =>
                ProbeAt(family: family, tableau: tableau, order: order, theta: 1.0, key: key).Bind(one =>
                    EndpointEvidence(family: family, tableau: tableau, order: order, key: key).Bind(evidence => {
                        DenseOutputReceipt receipt = new(
                            StageCount: tableau.StageCount, MethodOrder: tableau.MethodOrder, DenseOrder: order,
                            CheckedThetaCount: 3,
                            CheckedConditionCount: zero.CheckedConditionCount + mid.CheckedConditionCount + one.CheckedConditionCount,
                            FailedConditionCount: zero.FailedConditionCount + mid.FailedConditionCount + one.FailedConditionCount,
                            MaxResidual: Math.Max(val1: zero.MaxResidual, val2: Math.Max(val1: mid.MaxResidual, val2: one.MaxResidual)),
                            UsesStageDerivatives: true,
                            Endpoints: evidence, CoefficientFamily: family,
                            GenericCorrectionSolve: !family.MethodSpecific, CorrectionSolve: mid.CorrectionSolve);
                        return receipt.IsValid ? Fin.Succ(receipt) : Fin.Fail<DenseOutputReceipt>(key.InvalidResult());
                    }))));
    }
    internal static Fin<Seq<double>> WeightsAt(ButcherTableau tableau, double theta, Op key) {
        if (!double.IsFinite(theta) || theta is < 0.0 or > 1.0) return Fin.Fail<Seq<double>>(key.InvalidInput());
        DenseOutputCoefficientFamily family = DenseOutputCoefficientFamily.Identify(tableau: tableau);
        return Weights(family: family, tableau: tableau, order: DenseOrderFor(family: family, tableau: tableau), theta: theta, key: key).Map(static result => result.Values);
    }
    private static int DenseOrderFor(DenseOutputCoefficientFamily family, ButcherTableau tableau) =>
        family.MethodSpecific
            ? family.FixedDenseOrder
            : Math.Max(val1: 1, val2: Math.Min(val1: tableau.MethodOrder, val2: DistinctAbscissaCount(tableau: tableau)));
    private static int DistinctAbscissaCount(ButcherTableau tableau) {
        List<double> distinct = [];
        foreach (double c in tableau.Abscissae.AsIterable())
            if (!distinct.Exists(active => Math.Abs(value: active - c) <= ButcherTableau.CoefficientTolerance)) distinct.Add(c);
        return distinct.Count;
    }
    // Per-theta probe carrier — ONLY the facts one theta produces. The prior form re-used DenseOutputReceipt as
    // scratch with five endpoint-residual fields hardwired 0.0 and gated its IsValid on those fabricated zeros
    // ([FORGED_ZERO]: a gate passing on measurements nothing took); validity now adjudicates ONCE at the
    // aggregate receipt, where every endpoint field carries the REAL EndpointEvidence value.
    private readonly record struct ThetaProbe(int CheckedConditionCount, int FailedConditionCount, double MaxResidual, Option<SolveReceipt> CorrectionSolve);

    private static Fin<ThetaProbe> ProbeAt(DenseOutputCoefficientFamily family, ButcherTableau tableau, int order, double theta, Op key) =>
        Weights(family: family, tableau: tableau, order: order, theta: theta, key: key).Map(result => {
            Seq<double> weights = result.Values;
            (bool failed, double maxResidual) = MomentResidual(tableau: tableau, weights: weights, theta: theta, order: order);
            double endpoint = theta <= ButcherTableau.CoefficientTolerance
                ? weights.Fold(initialState: 0.0, f: static (max, value) => Math.Max(val1: max, val2: Math.Abs(value: value)))
                : 1.0 - theta <= ButcherTableau.CoefficientTolerance
                    ? weights.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second)))
                    : 0.0;
            return new ThetaProbe(
                CheckedConditionCount: order + ((theta <= ButcherTableau.CoefficientTolerance || 1.0 - theta <= ButcherTableau.CoefficientTolerance) ? tableau.StageCount : 0),
                FailedConditionCount: (failed ? 1 : 0) + (endpoint <= ButcherTableau.CoefficientTolerance ? 0 : 1),
                MaxResidual: Math.Max(val1: maxResidual, val2: endpoint),
                CorrectionSolve: result.Solve);
        });
    private static Fin<EndpointResiduals> EndpointEvidence(DenseOutputCoefficientFamily family, ButcherTableau tableau, int order, Op key) =>
        family.MethodSpecific
            ? from atOne in family.WeightsAt(theta: 1.0, stageCount: tableau.StageCount, key: key)
              from atZero in family.WeightsAt(theta: 0.0, stageCount: tableau.StageCount, key: key)
              from derivOne in family.DerivativeAt(theta: 1.0, stageCount: tableau.StageCount, key: key)
              from derivZero in family.DerivativeAt(theta: 0.0, stageCount: tableau.StageCount, key: key)
              select new EndpointResiduals(
                  ValueLeft: MaxAbs(values: atZero),
                  ValueRight: Math.Abs(value: atOne.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - tableau.Weights.Fold(initialState: 0.0, f: static (sum, value) => sum + value)),
                  Derivatives: Some((Left: MaxDeviation(values: derivZero, target: 0), Right: MaxDeviation(values: derivOne, target: tableau.StageCount - 1))),
                  Coefficient: atOne.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second))))
            : Weights(family: family, tableau: tableau, order: order, theta: 1.0, key: key).Bind(atOne =>
                Weights(family: family, tableau: tableau, order: order, theta: 0.0, key: key).Map(atZero => new EndpointResiduals(
                    ValueLeft: MaxAbs(values: atZero.Values),
                    ValueRight: Math.Abs(value: atOne.Values.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - tableau.Weights.Fold(initialState: 0.0, f: static (sum, value) => sum + value)),
                    Derivatives: Option<(double Left, double Right)>.None,
                    Coefficient: atOne.Values.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second))))));
    private static double MaxAbs(Seq<double> values) =>
        values.Fold(initialState: 0.0, f: static (max, value) => Math.Max(val1: max, val2: Math.Abs(value: value)));
    private static double MaxDeviation(Seq<double> values, int target) =>
        values.AsIterable().Select((value, index) => Math.Abs(value: value - (index == target ? 1.0 : 0.0))).Aggregate(seed: 0.0, func: static (max, deviation) => Math.Max(val1: max, val2: deviation));
    private static Fin<(Seq<double> Values, Option<SolveReceipt> Solve)> Weights(DenseOutputCoefficientFamily family, ButcherTableau tableau, int order, double theta, Op key) =>
        family.MethodSpecific
            ? family.WeightsAt(theta: theta, stageCount: tableau.StageCount, key: key).Map(static values => (Values: values, Solve: Option<SolveReceipt>.None))
            : theta <= ButcherTableau.CoefficientTolerance
                ? Fin.Succ((Values: toSeq(Enumerable.Repeat(element: 0.0, count: tableau.StageCount)), Solve: Option<SolveReceipt>.None))
                : 1.0 - theta <= ButcherTableau.CoefficientTolerance
                    ? Fin.Succ((Values: tableau.Weights, Solve: Option<SolveReceipt>.None))
                    : Correction(tableau: tableau, theta: theta, order: order, key: key).Map(correction => {
                        double endpointScale = theta * (1.0 - theta);
                        Seq<double> baseWeights = tableau.Weights.Map(weight => theta * weight);
                        return (Values: toSeq(baseWeights.Zip(toSeq(correction.Correction)).Select(pair => pair.First + (endpointScale * pair.Second))), Solve: Some(correction.Solve));
                    });
    private static Fin<(double[] Correction, SolveReceipt Solve)> Correction(ButcherTableau tableau, double theta, int order, Op key) {
        int stages = tableau.StageCount;
        double endpointScale = theta * (1.0 - theta);
        double[] design = MomentDesign(tableau: tableau, stages: stages, order: order);
        double[] rhs = [.. Enumerable.Range(start: 0, count: order).Select(m => (Math.Pow(x: theta, y: m + 1) - theta) / ((m + 1.0) * endpointScale))];
        return MomentPreimage(tableau: tableau, stages: stages, order: order, rhs: new Arr<double>(rhs), key: key).Bind(preimage =>
            Matrix.Of(rows: Dimension.Create(value: stages), cols: Dimension.Create(value: order), entries: new Arr<double>(design), key: key)
                .Bind(matrix => matrix.LeastSquaresDetailed(rhs: preimage, key: key))
                .Map(solved => (Correction: Enumerable.Range(start: 0, count: stages)
                    .Select(stage => Enumerable.Range(start: 0, count: order).Sum(row => design[(stage * order) + row] * solved.Solution[row]))
                    .ToArray(), Solve: solved)));
    }
    private static double[] MomentDesign(ButcherTableau tableau, int stages, int order) {
        double[] design = new double[stages * order];
        for (int stage = 0; stage < stages; stage++)
            for (int power = 0; power < order; power++) design[(stage * order) + power] = Math.Pow(x: tableau.Abscissae[stage], y: power);
        return design;
    }
    private static Fin<Arr<double>> MomentPreimage(ButcherTableau tableau, int stages, int order, Arr<double> rhs, Op key) {
        List<int> anchors = [];
        for (int stage = 0; stage < stages && anchors.Count < order; stage++)
            if (!anchors.Exists(existing => Math.Abs(value: tableau.Abscissae[existing] - tableau.Abscissae[stage]) <= ButcherTableau.CoefficientTolerance)) anchors.Add(stage);
        if (anchors.Count < order) return Fin.Fail<Arr<double>>(key.InvalidInput());
        double[] vandermonde = new double[order * order];
        for (int row = 0; row < order; row++)
            for (int col = 0; col < order; col++) vandermonde[(row * order) + col] = Math.Pow(x: tableau.Abscissae[anchors[col]], y: row);
        return Matrix.Of(rows: Dimension.Create(value: order), cols: Dimension.Create(value: order), entries: new Arr<double>(vandermonde), key: key)
            .Bind(matrix => matrix.SolveDetailed(rhs: rhs, key: key))
            .Map(solved => {
                double[] preimage = new double[stages];
                for (int index = 0; index < order; index++) preimage[anchors[index]] = solved.Solution[index];
                return new Arr<double>(preimage);
            });
    }
    private static (bool Failed, double Max) MomentResidual(ButcherTableau tableau, Seq<double> weights, double theta, int order) =>
        Enumerable.Range(start: 0, count: order)
            .Select(m => Math.Abs(value: ButcherTableau.MomentSum(weights: weights, against: tableau.Abscissae, power: m) - (Math.Pow(x: theta, y: m + 1) / (m + 1.0))))
            .Aggregate(seed: (Failed: false, Max: 0.0), func: static (state, residual) => (
                Failed: state.Failed || !double.IsFinite(residual) || residual > ButcherTableau.CoefficientTolerance,
                Max: Math.Max(val1: state.Max, val2: residual)));
}
```

## [04]-[STEPPER]

- Owner: `IntegrationModule<TState, TDelta>` mints the additive-module policy record — the four operations one Runge-Kutta step needs and its `Zero` delta, carrying `Combine` as the corpus' single linear-combination fold and the `Scalar`/`ComplexScalar` canonical instances for `double` and `Complex` state; `StepControl` mints the adaptive-control policy row whose `Rescale` applies the proportional step law; `FieldIntegrator` mints the `[Union]` Fixed/Adaptive integrator whose factories derive the dense-output receipt once and carry it on the case, and whose generic `Step` folds the coupling rows into stages, forms the primary and embedded combinations, applies the error control, and mints the dense-output span; `IntegrationStep<TState, TDelta>` carries the Accepted/Rejected outcome; `DenseOutputSpan<TState, TDelta>` carries the per-step continuous extension, its construction re-verifying the θ=1 weight combination against the tableau's declared weights.
- Cases: `FieldIntegrator` `FixedCase` · `AdaptiveCase`; `IntegrationStep` `AcceptedCase` · `RejectedCase`.
- Entry: `FieldIntegrator.Fixed` and `Adaptive` admit — re-validating the tableau, enforcing kind/case agreement (a fixed integrator over an embedded kind, or the reverse, fails typed), and deriving the carried receipt; `Step` takes the derivative field as a `sample` function, so the one stepper integrates a scalar ODE, a spatial streamline, or any admitted carrier; `AdmitOrFixed` defaults an absent integrator to `Fixed(RK4)`.
- Auto: stage computation is one fold over the coupling rows; the adaptive arm reads the error from the delta between the primary and embedded combinations, rescales through `StepControl`, and returns `RejectedCase` with the shrunk suggestion rather than looping.
- Receipt: the dense span carries the `DenseOutputReceipt`; step error and suggested step ride the outcome cases.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core; zero geometry — `Rasm.Domain` `Op` only.
- Growth: a new state carrier is one `IntegrationModule` instance at its consumer; a new control law (PI, Gustafsson) is one `StepControl` field set — the stepper body never changes.
- Boundary: no state difference ever appears — only deltas subtract, so the module needs no `TState` subtraction, and the error is measured between the two weight combinations before adding to the state; `MaxRejects` is consumer policy carried on the `AdaptiveCase`, never a hidden kernel constant.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record IntegrationModule<TState, TDelta>(
    Func<TState, double, TDelta, TState> Add,      // state + h * delta
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

    // ComplexScalar is the canonical Complex carrier beside Scalar: a frequency-domain or Hermitian state integrates on the same
    // stepper with no consumer-side module mint, the error norm the modulus.
    public static IntegrationModule<Complex, Complex> ComplexScalar { get; } = new(
        Add: static (state, h, delta) => state + (h * delta),
        Scale: static (factor, delta) => factor * delta,
        Sum: static (left, right) => left + right,
        Norm: static delta => Complex.Abs(value: delta),
        Zero: Complex.Zero);
}

public sealed record StepControl(double SafetyFactor, double MinScale, double MaxScale) {
    public static readonly StepControl Default = new(SafetyFactor: 0.9, MinScale: 0.2, MaxScale: 10.0);
    internal double Rescale(double error, double tolerance, double exponent) =>
        error > EpsilonPolicy.ZeroTolerance
            ? Math.Clamp(value: SafetyFactor * Math.Pow(x: tolerance / error, y: exponent), min: MinScale, max: MaxScale)
            : MaxScale;
}

[Union]
public abstract partial record IntegrationStep<TState, TDelta> {
    public sealed record AcceptedCase(TState Next, double SuggestedStep, Option<double> Error, DenseOutputSpan<TState, TDelta> Dense) : IntegrationStep<TState, TDelta>;
    public sealed record RejectedCase(double SuggestedStep, Option<double> Error) : IntegrationStep<TState, TDelta>;
    private IntegrationStep() { }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DenseOutputSpan<TState, TDelta>(TState Start, TState End, double Step, Seq<TDelta> Stages, ButcherTableau Tableau, DenseOutputReceipt Receipt, IntegrationModule<TState, TDelta> Module) {
    // Receipt arrives already derived at admission; construction proves only the per-step fact — θ=1
    // reproduces the declared weights within a SCALE-RELATIVE band on the max stage norm, drift being a
    // coefficient-error combination of the stages: an absolute gate fails large fields, a combined-norm gate near-cancelling ones.
    internal static Fin<DenseOutputSpan<TState, TDelta>> Of(IntegrationModule<TState, TDelta> module, TState start, TState end, double step, Seq<TDelta> stages, ButcherTableau tableau, DenseOutputReceipt receipt, Op key) =>
        tableau.DenseWeightsAt(theta: 1.0, key: key).Bind(weights => {
            TDelta reconstructed = module.Combine(coefficients: weights, deltas: stages);
            TDelta declared = module.Combine(coefficients: tableau.Weights, deltas: stages);
            double drift = module.Norm(arg: module.Sum(arg1: reconstructed, arg2: module.Scale(arg1: -1.0, arg2: declared)));
            double stageScale = stages.Fold(initialState: 0.0, f: (max, delta) => Math.Max(val1: max, val2: module.Norm(arg: delta)));
            return double.IsFinite(drift) && drift <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: stageScale)
                && stages.Count == tableau.StageCount && Math.Abs(value: step) > EpsilonPolicy.ZeroTolerance && receipt.IsValid
                ? Fin.Succ(new DenseOutputSpan<TState, TDelta>(Start: start, End: end, Step: step, Stages: stages, Tableau: tableau, Receipt: receipt, Module: module))
                : Fin.Fail<DenseOutputSpan<TState, TDelta>>(key.InvalidResult());
        });
    public Fin<TState> PointAt(double theta, Op key) {
        if (!double.IsFinite(theta) || theta is < 0.0 or > 1.0) return Fin.Fail<TState>(key.InvalidInput());
        DenseOutputSpan<TState, TDelta> self = this;
        return Tableau.DenseWeightsAt(theta: theta, key: key)
            .Map(weights => self.Module.Add(arg1: self.Start, arg2: self.Step, arg3: self.Module.Combine(coefficients: weights, deltas: self.Stages)));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union]
public abstract partial record FieldIntegrator {
    public sealed record FixedCase : FieldIntegrator { internal FixedCase(IntegratorKind kind, DenseOutputReceipt dense) { Kind = kind; Dense = dense; } public IntegratorKind Kind { get; } public DenseOutputReceipt Dense { get; } }
    public sealed record AdaptiveCase : FieldIntegrator {
        internal AdaptiveCase(IntegratorKind kind, PositiveMagnitude tolerance, int maxRejects, StepControl control, DenseOutputReceipt dense) { Kind = kind; Tolerance = tolerance; MaxRejects = maxRejects; Control = control; Dense = dense; }
        public IntegratorKind Kind { get; }
        public PositiveMagnitude Tolerance { get; }
        public int MaxRejects { get; }
        public StepControl Control { get; }
        public DenseOutputReceipt Dense { get; }
    }
    private FieldIntegrator() { }
    public static Fin<FieldIntegrator> Fixed(IntegratorKind kind, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(kind).ToFin(op.InvalidInput())
               from tableau in active.Tableau.Admit(key: op)
               from fixedKind in guard(!active.IsAdaptive, op.Unsupported(geometryType: active.GetType(), outputType: typeof(FixedCase)))
               from dense in tableau.DenseOutputReceipt(key: op)
               select (FieldIntegrator)new FixedCase(kind: active, dense: dense);
    }
    public static Fin<FieldIntegrator> Adaptive(IntegratorKind kind, double tolerance, int maxRejects = 3, StepControl? control = null, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(kind).ToFin(op.InvalidInput())
               from tableau in active.Tableau.Admit(key: op)
               from adaptiveKind in guard(active.IsAdaptive, op.Unsupported(geometryType: active.GetType(), outputType: typeof(AdaptiveCase)))
               from rejects in guard(maxRejects >= 0, op.InvalidInput())
               from validated in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               from dense in tableau.DenseOutputReceipt(key: op)
               select (FieldIntegrator)new AdaptiveCase(kind: active, tolerance: validated, maxRejects: maxRejects, control: control ?? StepControl.Default, dense: dense);
    }
    public int RejectBudget => Switch(state: 0, fixedCase: static (s, _) => s, adaptiveCase: static (_, c) => c.MaxRejects);
    internal ButcherTableau Tableau => Switch(state: default(ButcherTableau), fixedCase: static (_, c) => c.Kind.Tableau, adaptiveCase: static (_, c) => c.Kind.Tableau);
    internal DenseOutputReceipt Dense => Switch(state: default(DenseOutputReceipt), fixedCase: static (_, c) => c.Dense, adaptiveCase: static (_, c) => c.Dense);
    public int MethodOrder => Tableau.MethodOrder;
    public Option<int> EmbeddedOrder => Tableau.EmbeddedOrder;
    internal Fin<FieldIntegrator> Admit(Op key) =>
        Switch(
            state: key,
            fixedCase: static (op, integrator) =>
                from kind in Optional(integrator.Kind).ToFin(op.InvalidInput())
                from tableau in kind.Tableau.Admit(key: op)
                from fixedKind in guard(!kind.IsAdaptive, op.Unsupported(geometryType: kind.GetType(), outputType: typeof(FixedCase)))
                from dense in guard(integrator.Dense.IsValid, op.InvalidInput())
                select (FieldIntegrator)integrator,
            adaptiveCase: static (op, integrator) =>
                from kind in Optional(integrator.Kind).ToFin(op.InvalidInput())
                from tableau in kind.Tableau.Admit(key: op)
                from rejects in guard(integrator.MaxRejects >= 0, op.InvalidInput())
                from adaptiveKind in guard(kind.IsAdaptive, op.Unsupported(geometryType: kind.GetType(), outputType: typeof(AdaptiveCase)))
                from dense in guard(integrator.Dense.IsValid, op.InvalidInput())
                select (FieldIntegrator)integrator);
    public static Fin<FieldIntegrator> Admit(FieldIntegrator value, Op key) =>
        Optional(value).ToFin(key.InvalidInput()).Bind(integrator => integrator.Admit(key: key));
    public static Fin<FieldIntegrator> AdmitOrFixed(FieldIntegrator? value, Op key) =>
        value is null ? Fixed(kind: IntegratorKind.RK4, key: key) : Admit(value: value, key: key);
    public Fin<IntegrationStep<TState, TDelta>> Step<TState, TDelta>(IntegrationModule<TState, TDelta> module, Func<TState, Fin<TDelta>> sample, TState state, double h, Op key) => Switch(
        state: (Module: module, Sample: sample, State: state, H: h, Key: key),
        fixedCase: static (s, c) =>
            from ks in Stages(module: s.Module, sample: s.Sample, tableau: c.Kind.Tableau, state: s.State, h: s.H, key: s.Key)
            let next = s.Module.Add(arg1: s.State, arg2: s.H, arg3: s.Module.Combine(coefficients: c.Kind.Tableau.Weights, deltas: ks))
            from dense in DenseOutputSpan<TState, TDelta>.Of(module: s.Module, start: s.State, end: next, step: s.H, stages: ks, tableau: c.Kind.Tableau, receipt: c.Dense, key: s.Key)
            select (IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.AcceptedCase(Next: next, SuggestedStep: s.H, Error: Option<double>.None, Dense: dense),
        adaptiveCase: static (s, c) =>
            from embeddedWeights in c.Kind.Tableau.EmbeddedWeights.ToFin(Fail: s.Key.InvalidInput())
            from ks in Stages(module: s.Module, sample: s.Sample, tableau: c.Kind.Tableau, state: s.State, h: s.H, key: s.Key)
            let primary = s.Module.Combine(coefficients: c.Kind.Tableau.Weights, deltas: ks)
            let secondary = s.Module.Combine(coefficients: embeddedWeights, deltas: ks)
            let err = Math.Abs(value: s.H) * s.Module.Norm(arg: s.Module.Sum(arg1: primary, arg2: s.Module.Scale(arg1: -1.0, arg2: secondary)))
            let scale = c.Control.Rescale(error: err, tolerance: c.Tolerance.Value, exponent: c.Kind.AdaptiveExponent)
            from result in err <= c.Tolerance.Value
                ? DenseOutputSpan<TState, TDelta>.Of(module: s.Module, start: s.State, end: s.Module.Add(arg1: s.State, arg2: s.H, arg3: primary), step: s.H, stages: ks, tableau: c.Kind.Tableau, receipt: c.Dense, key: s.Key)
                    .Map(dense => (IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.AcceptedCase(Next: s.Module.Add(arg1: s.State, arg2: s.H, arg3: primary), SuggestedStep: s.H * scale, Error: Some(err), Dense: dense))
                : Fin.Succ((IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.RejectedCase(SuggestedStep: s.H * scale, Error: Some(err)))
            select result);
    private static Fin<Seq<TDelta>> Stages<TState, TDelta>(IntegrationModule<TState, TDelta> module, Func<TState, Fin<TDelta>> sample, ButcherTableau tableau, TState state, double h, Op key) =>
        tableau.Coupling.Fold(
            initialState: Fin.Succ((Seq<TDelta>)[]),
            f: (acc, row) => acc.Bind(ks =>
                sample(arg: module.Add(arg1: state, arg2: h, arg3: module.Combine(coefficients: row, deltas: ks)))
                    .Map(k => ks.Add(k))));
}
```

## [05]-[QUADRATURE]

- Owner: `QuadratureRoute` the `[SmartEnum<string>]` accuracy axis carrying each kernel's `KernelOutcome` delegate and `InfiniteBounds` capability; `IntegrationDomain` the `[Union]` arity axis over genuine 1-D/2-D/3-D integrands, the Smolyak sparse grid, and the reference-element simplex; `ReferenceElement` the `[SmartEnum<string>]` row family whose rows carry the owned-build Gauss tables per reference domain — triangle/tet area-volume coordinates, `[-1,1]` cube tensor Gauss, triangle⊗line prism, conical pyramid; `IntervalSpec` the bound value whose values alone encode finite or infinite extent; `Quadrature.Integrate` the one entry whose finite-guard-then-admit combinator reads the delegate column once.
- Cases: `QuadratureRoute` rows `DoubleExponential` · `GaussLegendre` · `GaussKronrod`; `IntegrationDomain` cases `Line` · `Rectangle` · `Cuboid` · `SparseGrid` · `Simplex`; `ReferenceElement` rows `Line` · `Tri` · `Tet` · `Quad` · `Hex` · `Wedge` · `Pyramid`, each electing the smallest owned rule at or above the requested order.
- Entry: `Quadrature.Integrate(IntegrationDomain domain, QuadratureControl? control = null, Op? key = null)` — arity is the case, accuracy the route row, and the reference-element table the `Simplex` case's election; no sibling integrator entry exists.
- Auto: each arm wraps its integrand in a skip-counting guard because no MathNet route inspects returns and a pole poisons the weighted sum silently — `QuadratureControl.MaxSkipped` makes that loss budget explicit and defaults to zero; one `Try.lift<Fin<QuadratureEvidence>>` captures integrand and MathNet exceptions onto the typed rail; a `Line` arm faults a route lacking `InfiniteBounds` against an infinite `IntervalSpec` rather than feeding infinity to a finite-only kernel; only `GaussKronrod` returns the `error`/`L1Norm` channel, so the error-budget and cancellation gates bind only where the channels are `Some`; `SparseGrid` folds the nested Clenshaw-Curtis combination formula through `SmolyakCubature.Integrate` under `MaxSparseLevel`; `Simplex` folds the elected reference-element table — the reference-domain integral, the consumer weighting each point by its own Jacobian at the isoparametric map it owns.
- Receipt: `QuadratureEvidence` carries `Option<double>` error, L1, and ratio so a non-adaptive route reports honest absence, never a fabricated `NaN`; the skip count rides the receipt, never silently as coverage; the gate is three-tier — non-finite rejects, an adaptive error estimate over `max(AbsoluteError, RelativeError·|value|)` rejects, and a cancellation ratio breaching the floor rejects — never a rejection on slow convergence alone.
- Packages: MathNet.Numerics (`Integrate.DoubleExponential`/`GaussLegendre`/`GaussKronrod`/`OnRectangle`/`OnCuboid`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new accuracy kernel is one `QuadratureRoute` row with its delegate and infinite-bound capability; a new arity is one `IntegrationDomain` case; a new reference domain or a higher-order rule is one `ReferenceElement` row or one entry on its rule ladder; a new sparse-grid 1-D rule family or dimension-adaptive refinement is a policy row on `SmolyakCubature` — zero new surface.
- Boundary: accuracy is the primary decision with order secondary — the three MathNet kernels bind as route rows, never sibling factories, and the finite-guard-then-admit combinator applies once over the uniform `KernelOutcome` column; infinite bounds route only into `DoubleExponential`/`GaussLegendre`, whose MathNet entries substitute infinity through a baked-in abscissa transform, so `InfiniteBounds` is load-bearing and any 1-D delegate forced through a 2-D rule integrates `(b−a)·∫f` and is rejected; `error`/`L1Norm`/`Ratio` are `Option<double>` because only the adaptive Kronrod row yields them; the reference-element tables integrate the REFERENCE domain — the physical mapping, its Jacobian, and the isoparametric basis stay the consuming element's, so this owner never learns an element topology; a consumer calling `Integrate.GaussLegendre` raw skips the finite guard, the skip budget, and the typed evidence and is the deleted form.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
public readonly record struct IntervalSpec(double Lower, double Upper) {
    public bool Infinite => double.IsInfinity(Lower) || double.IsInfinity(Upper);
}

public readonly record struct KernelOutcome(double Value, Option<double> Error, Option<double> L1Norm);

[SmartEnum<string>]
public sealed partial class QuadratureRoute {
    // InfiniteBounds is true for DoubleExponential/GaussLegendre — the MathNet abscissa transform is baked in —
    // and false for GaussKronrod, which integrates the raw interval and is finite-only.
    public static readonly QuadratureRoute DoubleExponential = new("double-exponential", infiniteBounds: true,
        kernel: static (f, lo, hi, c) => new KernelOutcome(Value: Integrate.DoubleExponential(f, lo, hi, targetAbsoluteError: c.AbsoluteError), Error: None, L1Norm: None));
    public static readonly QuadratureRoute GaussLegendre = new("gauss-legendre", infiniteBounds: true,
        kernel: static (f, lo, hi, c) => new KernelOutcome(Value: Integrate.GaussLegendre(f, lo, hi, order: c.LegendreOrder), Error: None, L1Norm: None));
    public static readonly QuadratureRoute GaussKronrod = new("gauss-kronrod", infiniteBounds: false,
        kernel: static (f, lo, hi, c) => {
            double value = Integrate.GaussKronrod(f, lo, hi, out double error, out double l1Norm, c.RelativeError, c.MaximumDepth, c.KronrodPoints);
            return new KernelOutcome(Value: value, Error: Some(error), L1Norm: Some(l1Norm));
        });

    private readonly Func<Func<double, double>, double, double, QuadratureControl, KernelOutcome> kernel;

    public bool InfiniteBounds { get; }

    public KernelOutcome Run(Func<double, double> f, double lower, double upper, QuadratureControl control) => kernel(f, lower, upper, control);
}

[Union]
public abstract partial record IntegrationDomain {
    private IntegrationDomain() { }

    public sealed record Line(Func<double, double> F, IntervalSpec Bounds, QuadratureRoute Route) : IntegrationDomain;
    public sealed record Rectangle(Func<double, double, double> F, IntervalSpec X, IntervalSpec Y, int Order) : IntegrationDomain;
    public sealed record Cuboid(Func<double, double, double, double> F, IntervalSpec X, IntervalSpec Y, IntervalSpec Z, int Order) : IntegrationDomain;
    public sealed record SparseGrid(Func<double[], double> F, IntervalSpec[] Bounds, int Level) : IntegrationDomain;
    // Reference-domain integral over the elected element table; the physical Jacobian is the consumer's own
    // isoparametric weight folded into F.
    public sealed record Simplex(Func<double, double, double, double> F, ReferenceElement Element, int Order) : IntegrationDomain;
}

// ReferenceElement owns the built Gauss table per reference domain; rules compose from the canonical 1-D Gauss-Legendre nodes
// as compile-time constants, so a per-call runtime rule construction is the avoided allocation.
public readonly record struct QuadratureRule(int Order, int Dimension, ImmutableArray<(double X, double Y, double Z, double Weight)> Points) {
    internal static readonly ImmutableArray<(double Node, double Weight)> Gauss1 = [(0.0, 2.0)];
    internal static readonly ImmutableArray<(double Node, double Weight)> Gauss2 = [(-1.0 / Math.Sqrt(3.0), 1.0), (1.0 / Math.Sqrt(3.0), 1.0)];
    internal static readonly ImmutableArray<(double Node, double Weight)> Gauss3 = [(-Math.Sqrt(0.6), 5.0 / 9.0), (0.0, 8.0 / 9.0), (Math.Sqrt(0.6), 5.0 / 9.0)];

    public static readonly QuadratureRule Line2 = new(2, 1, [.. Gauss2.Select(static g => (g.Node, 0.0, 0.0, g.Weight))]);
    public static readonly QuadratureRule Tri1 = new(1, 2, [(1.0 / 3.0, 1.0 / 3.0, 0.0, 0.5)]);
    public static readonly QuadratureRule Tri3 = new(2, 2, [
        (1.0 / 6.0, 1.0 / 6.0, 0.0, 1.0 / 6.0), (2.0 / 3.0, 1.0 / 6.0, 0.0, 1.0 / 6.0), (1.0 / 6.0, 2.0 / 3.0, 0.0, 1.0 / 6.0)]);
    public static readonly QuadratureRule Tet1 = new(1, 3, [(0.25, 0.25, 0.25, 1.0 / 6.0)]);
    public static readonly QuadratureRule Tet4 = new(2, 3, [.. Simplex3(ab: [0.5854101966249685, 0.1381966011250105])]);
    public static readonly QuadratureRule Quad1 = TensorCube(dim: 2, line: Gauss1);
    public static readonly QuadratureRule Quad4 = TensorCube(dim: 2, line: Gauss2);
    public static readonly QuadratureRule Quad9 = TensorCube(dim: 2, line: Gauss3);
    public static readonly QuadratureRule Hex1 = TensorCube(dim: 3, line: Gauss1);
    public static readonly QuadratureRule Hex8 = TensorCube(dim: 3, line: Gauss2);
    public static readonly QuadratureRule Hex27 = TensorCube(dim: 3, line: Gauss3);
    public static readonly QuadratureRule Wedge6 = PrismProduct(tri: Tri3, line: Gauss2);
    public static readonly QuadratureRule Wedge18 = PrismProduct(tri: Tri3, line: Gauss3);
    public static readonly QuadratureRule Pyramid5 = Conical(n: 2);

    private static IEnumerable<(double, double, double, double)> Simplex3(double[] ab) {
        (double a, double b) = (ab[0], ab[1]);
        yield return (a, b, b, 1.0 / 24.0);
        yield return (b, a, b, 1.0 / 24.0);
        yield return (b, b, a, 1.0 / 24.0);
        yield return (b, b, b, 1.0 / 24.0);
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
        return new(2 * n - 1, dim, [.. rows]);
    }

    private static QuadratureRule PrismProduct(QuadratureRule tri, ImmutableArray<(double Node, double Weight)> line) {
        List<(double, double, double, double)> rows = [];
        foreach ((double X, double Y, double Z, double Weight) point in tri.Points)
            foreach ((double node, double weight) in line) rows.Add((point.X, point.Y, node, point.Weight * weight));
        return new(tri.Order + line.Length, 3, [.. rows]);
    }

    // Conical product for the pyramid: the (1−ζ)² collapse factor rides the weight, so the rational apex basis
    // never evaluates at the singular apex.
    private static QuadratureRule Conical(int n) {
        ImmutableArray<(double Node, double Weight)> baseLine = n == 2 ? Gauss2 : Gauss3;
        (double, double)[] zeta = [(0.1127016653792583, 0.2777777777777778), (0.5, 0.4444444444444444), (0.8872983346207417, 0.2777777777777778)];
        List<(double, double, double, double)> rows = [];
        foreach ((double z, double wz) in zeta) {
            double scale = 1.0 - z;
            foreach ((double bj, double wj) in baseLine)
                foreach ((double bi, double wi) in baseLine) rows.Add((bi * scale, bj * scale, z, wi * wj * wz * scale * scale));
        }
        return new(5, 3, [.. rows]);
    }
}

// Reference-element row family: each row carries its ordered rule ladder and elects the smallest owned rule at
// or above the requested order, clamped to its highest — so a 2-D element can never index a 3-D rule and a
// consumer never constructs a Gauss table.
[SmartEnum<string>]
public sealed partial class ReferenceElement {
    public static readonly ReferenceElement Line = new("line", rules: [QuadratureRule.Line2]);
    public static readonly ReferenceElement Tri = new("tri", rules: [QuadratureRule.Tri1, QuadratureRule.Tri3]);
    public static readonly ReferenceElement Tet = new("tet", rules: [QuadratureRule.Tet1, QuadratureRule.Tet4]);
    public static readonly ReferenceElement Quad = new("quad", rules: [QuadratureRule.Quad1, QuadratureRule.Quad4, QuadratureRule.Quad9]);
    public static readonly ReferenceElement Hex = new("hex", rules: [QuadratureRule.Hex1, QuadratureRule.Hex8, QuadratureRule.Hex27]);
    public static readonly ReferenceElement Wedge = new("wedge", rules: [QuadratureRule.Wedge6, QuadratureRule.Wedge18]);
    public static readonly ReferenceElement Pyramid = new("pyramid", rules: [QuadratureRule.Pyramid5]);

    private readonly ImmutableArray<QuadratureRule> rules;

    public QuadratureRule Rule(int order) =>
        rules.FirstOrDefault(rule => rule.Order >= order, rules[^1]);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record QuadratureControl(double AbsoluteError, double RelativeError, double CancellationFloor, int MaxSkipped, int LegendreOrder, int KronrodPoints, int MaximumDepth, int MaxSparseLevel) {
    public static readonly QuadratureControl Default = new(AbsoluteError: 1e-8, RelativeError: 1e-8, CancellationFloor: 1e-10, MaxSkipped: 0, LegendreOrder: 128, KronrodPoints: 15, MaximumDepth: 15, MaxSparseLevel: 8);
    internal bool IsValid =>
        double.IsFinite(AbsoluteError) && AbsoluteError > 0.0
        && double.IsFinite(RelativeError) && RelativeError > 0.0
        && double.IsFinite(CancellationFloor) && CancellationFloor is >= 0.0 and <= 1.0
        && MaxSkipped >= 0 && LegendreOrder > 0 && KronrodPoints > 0 && MaximumDepth > 0
        && MaxSparseLevel is > 0 and < 31;
}

public sealed record QuadratureEvidence(double Value, Option<double> Error, Option<double> L1Norm, Option<double> Ratio, int Skipped);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Quadrature {
    public static Fin<QuadratureEvidence> Integrate(IntegrationDomain domain, QuadratureControl? control = null, Op? key = null) {
        Op op = key.OrDefault();
        QuadratureControl ctl = control ?? QuadratureControl.Default;
        if (domain is null) return Fin.Fail<QuadratureEvidence>(op.InvalidInput());
        if (!ctl.IsValid) return Fin.Fail<QuadratureEvidence>(new Fault.InvalidValue(Label: nameof(QuadratureControl), Requirement: "finite positive budgets, unit-bounded floor, positive orders"));

        return Try.lift<Fin<QuadratureEvidence>>(() => domain.Switch(
            line: l => !Ordered(bounds: l.Bounds)
                ? Fin.Fail<QuadratureEvidence>(new Fault.InvalidValue(Label: "quadrature-bounds", Requirement: "NaN-free ascending interval", Key: op))
                : l.Bounds.Infinite && !l.Route.InfiniteBounds
                    ? Fin.Fail<QuadratureEvidence>(new Fault.InvalidValue(Label: l.Route.Key, Requirement: "a route carrying InfiniteBounds", Key: op))
                    : Guarded1(f: l.F, run: (guarded, skipped) => Admit(outcome: l.Route.Run(f: guarded, lower: l.Bounds.Lower, upper: l.Bounds.Upper, control: ctl), skipped: skipped(), ctl: ctl, op: op)),
            rectangle: r => !FiniteOrdered(bounds: r.X) || !FiniteOrdered(bounds: r.Y) || r.Order <= 0
                ? Fin.Fail<QuadratureEvidence>(new Fault.InvalidValue(Label: "cubature-domain", Requirement: "finite ascending intervals and a positive order", Key: op))
                : Guarded2(f: r.F, run: (guarded, skipped) => Admit(outcome: new KernelOutcome(Value: Integrate.OnRectangle(guarded, r.X.Lower, r.X.Upper, r.Y.Lower, r.Y.Upper, r.Order), Error: None, L1Norm: None), skipped: skipped(), ctl: ctl, op: op)),
            cuboid: c => !FiniteOrdered(bounds: c.X) || !FiniteOrdered(bounds: c.Y) || !FiniteOrdered(bounds: c.Z) || c.Order <= 0
                ? Fin.Fail<QuadratureEvidence>(new Fault.InvalidValue(Label: "cubature-domain", Requirement: "finite ascending intervals and a positive order", Key: op))
                : Guarded3(f: c.F, run: (guarded, skipped) => Admit(outcome: new KernelOutcome(Value: Integrate.OnCuboid(guarded, c.X.Lower, c.X.Upper, c.Y.Lower, c.Y.Upper, c.Z.Lower, c.Z.Upper, c.Order), Error: None, L1Norm: None), skipped: skipped(), ctl: ctl, op: op)),
            sparseGrid: s => s.Bounds is null || s.Bounds.Length is < 2 or > 20 || s.Level <= 0 || s.Level > ctl.MaxSparseLevel || toSeq(s.Bounds).Exists(static b => !FiniteOrdered(bounds: b))
                ? Fin.Fail<QuadratureEvidence>(new Fault.InvalidValue(Label: "sparse-grid-domain", Requirement: "2-20 finite dimensions inside the level budget", Key: op))
                : GuardedN(f: s.F, run: (guarded, skipped) => Admit(outcome: SmolyakCubature.Integrate(f: guarded, bounds: s.Bounds, level: s.Level), skipped: skipped(), ctl: ctl, op: op)),
            simplex: x => x.Order <= 0
                ? Fin.Fail<QuadratureEvidence>(new Fault.InvalidValue(Label: "simplex-order", Requirement: "a positive rule order", Key: op))
                : Guarded3(f: x.F, run: (guarded, skipped) => {
                    double sum = 0.0;
                    foreach ((double px, double py, double pz, double weight) in x.Element.Rule(order: x.Order).Points) sum += weight * guarded(px, py, pz);
                    return Admit(outcome: new KernelOutcome(Value: sum, Error: None, L1Norm: None), skipped: skipped(), ctl: ctl, op: op);
                })))
            .Run()
            .MapFail(error => (Error)new Fault.ComputationFailed($"quadrature-kernel:{error.Message}"))
            .Bind(identity);
    }

    // One skip-counting guard serves every arity — a non-finite sample counts and contributes zero, and the count
    // rides the receipt through the admission gate.
    private static Fin<QuadratureEvidence> Guarded1(Func<double, double> f, Func<Func<double, double>, Func<int>, Fin<QuadratureEvidence>> run) {
        int skipped = 0;
        return run(x => f(x) is var y && double.IsFinite(y) ? y : (skipped++, 0.0).Item2, () => skipped);
    }
    private static Fin<QuadratureEvidence> Guarded2(Func<double, double, double> f, Func<Func<double, double, double>, Func<int>, Fin<QuadratureEvidence>> run) {
        int skipped = 0;
        return run((x, y) => f(x, y) is var z && double.IsFinite(z) ? z : (skipped++, 0.0).Item2, () => skipped);
    }
    private static Fin<QuadratureEvidence> Guarded3(Func<double, double, double, double> f, Func<Func<double, double, double, double>, Func<int>, Fin<QuadratureEvidence>> run) {
        int skipped = 0;
        return run((x, y, z) => f(x, y, z) is var w && double.IsFinite(w) ? w : (skipped++, 0.0).Item2, () => skipped);
    }
    private static Fin<QuadratureEvidence> GuardedN(Func<double[], double> f, Func<Func<double[], double>, Func<int>, Fin<QuadratureEvidence>> run) {
        int skipped = 0;
        return run(x => f(x) is var y && double.IsFinite(y) ? y : (skipped++, 0.0).Item2, () => skipped);
    }

    // Admission rejects non-finite value, skip-budget breach, adaptive error beyond max(abs, rel·|value|), and
    // cancellation below floor where L1 exists; absent evidence channels remain None.
    private static Fin<QuadratureEvidence> Admit(KernelOutcome outcome, int skipped, QuadratureControl ctl, Op op) =>
        !double.IsFinite(outcome.Value)
            ? Fin.Fail<QuadratureEvidence>(new Fault.OutOfRange(Label: "quadrature-value", Scalar: outcome.Value, Requirement: "finite", Key: op))
        : skipped > ctl.MaxSkipped
            ? Fin.Fail<QuadratureEvidence>(new Fault.OutOfRange(Label: "integrand-loss", Scalar: skipped, Requirement: $"<= {ctl.MaxSkipped} skipped samples", Key: op))
        : outcome.Error is { IsSome: true, Case: double estimate }
          && estimate > Math.Max(val1: ctl.AbsoluteError, val2: ctl.RelativeError * Math.Abs(value: outcome.Value))
            ? Fin.Fail<QuadratureEvidence>(new Fault.OutOfRange(Label: "error-over-budget", Scalar: estimate, Requirement: $"<= {Math.Max(val1: ctl.AbsoluteError, val2: ctl.RelativeError * Math.Abs(value: outcome.Value)):e3}", Key: op))
        : outcome.L1Norm.Map(l1 => l1 == 0.0 && outcome.Value == 0.0 ? 1.0 : Math.Abs(value: outcome.Value / l1)).Match(
            Some: ratio => double.IsFinite(ratio) && ratio >= ctl.CancellationFloor
                ? Fin.Succ(new QuadratureEvidence(Value: outcome.Value, Error: outcome.Error, L1Norm: outcome.L1Norm, Ratio: Some(ratio), Skipped: skipped))
                : Fin.Fail<QuadratureEvidence>(new Fault.OutOfRange(Label: "cancellation-breach", Scalar: ratio, Requirement: $">= {ctl.CancellationFloor:e3}", Key: op)),
            None: () => Fin.Succ(new QuadratureEvidence(Value: outcome.Value, Error: outcome.Error, L1Norm: None, Ratio: None, Skipped: skipped)));

    private static bool Ordered(IntervalSpec bounds) => !double.IsNaN(bounds.Lower) && !double.IsNaN(bounds.Upper) && bounds.Lower < bounds.Upper;

    private static bool FiniteOrdered(IntervalSpec bounds) => !bounds.Infinite && Ordered(bounds: bounds);
}

// Smolyak combines nested Clenshaw-Curtis rules across q−d+1 ≤ |ℓ|₁ ≤ q for 5-20-dimensional projection and
// moment integrals; nested Chebyshev extrema retain lower-level evaluations without a full tensor product.
public static class SmolyakCubature {
    public static KernelOutcome Integrate(Func<double[], double> f, IntervalSpec[] bounds, int level) {
        int d = bounds.Length;
        double sum = 0.0;
        foreach ((int[] multi, int coefficient) in CombinationLevels(dimensions: d, level: level)) {
            double block = 0.0;
            foreach ((double[] node, double weight) in TensorNodes(multi: multi, bounds: bounds)) block += weight * f(node);
            sum += coefficient * block;
        }
        return new KernelOutcome(Value: sum, Error: None, L1Norm: None);
    }

    // Multi-indices with q−d+1 ≤ |ℓ|₁ ≤ q and the Smolyak coefficient (−1)^(q−|ℓ|₁)·C(d−1, q−|ℓ|₁), generated
    // DIRECTLY per band total — the prior form enumerated every composition of the whole d-simplex up to q and
    // filtered (≈10¹³ candidates at the admitted d=20, ℓ=8, to keep Σ C(s−1,d−1) members); the coefficient hoists
    // per total because every composition of one total shares it.
    private static IEnumerable<(int[] Multi, int Coefficient)> CombinationLevels(int dimensions, int level) {
        int q = level + dimensions - 1;
        for (int total = q - dimensions + 1; total <= q; total++) {
            int coefficient = (int)(Math.Pow(x: -1, y: q - total) * Binomial(n: dimensions - 1, k: q - total));
            foreach (int[] multi in PositiveCompositions(total: total, slots: dimensions)) yield return (multi, coefficient);
        }
    }

    // Positive compositions of `total` into exactly `slots` parts by bounded descent — each emitted array is a
    // band member by construction, so nothing is generated to be discarded; the head range keeps one unit for
    // every remaining slot, and the last slot takes the exact remainder.
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

    private static IEnumerable<(double[] Node, double Weight)> TensorNodes(int[] multi, IntervalSpec[] bounds) {
        (double[] Nodes, double[] Weights)[] axes = [.. multi.Select((l, k) => ClenshawCurtis(level: l, bounds: bounds[k]))];
        return Enumerable.Range(start: 0, count: axes.Aggregate(seed: 1, func: static (acc, axis) => acc * axis.Nodes.Length)).Select(flat => {
            double[] node = new double[axes.Length];
            double weight = 1.0;
            int rest = flat;
            for (int k = axes.Length - 1; k >= 0; k--) {
                int i = rest % axes[k].Nodes.Length;
                rest /= axes[k].Nodes.Length;
                node[k] = axes[k].Nodes[i];
                weight *= axes[k].Weights[i];
            }
            return (node, weight);
        });
    }

    // Nested Chebyshev-extrema rule at level ℓ: n = 2^(ℓ−1)+1 points (ℓ=1 the midpoint), classic
    // Clenshaw-Curtis cosine-sum weights mapped onto [lower, upper].
    private static (double[] Nodes, double[] Weights) ClenshawCurtis(int level, IntervalSpec bounds) {
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

    private static double Binomial(int n, int k) =>
        k < 0 || k > n ? 0.0 : Enumerable.Range(start: 0, count: k).Aggregate(seed: 1.0, func: (acc, i) => acc * (n - i) / (i + 1));
}
```

## [06]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or member on the owning carrier, never a sibling surface. Each `[OWNER]` cell names the canonical carrier; sibling carriers and the per-axis kind ride the indexed notes below.

| [INDEX] | [AXIS_CONCERN]       | [OWNER]                            | [CASES] |
| :-----: | :------------------- | :--------------------------------- | :-----: |
|  [01]   | Integrator rows      | `IntegratorKind`                   |    9    |
|  [02]   | Coefficient carrier  | `ButcherTableau`                   |    1    |
|  [03]   | Continuous extension | `ButcherDenseOutput`               |    3    |
|  [04]   | Step algebra         | `IntegrationModule<TState,TDelta>` |   2·2   |

- [01]-[INTEGRATOR_ROWS]: `[SmartEnum<int>]` — the tableau IS the row.
- [02]-[COEFFICIENT_CARRIER]: order-condition-validated record + `ButcherOrderReceipt` `ValidityClaim.All` receipt over the `RootedTree` walk.
- [03]-[CONTINUOUS_EXTENSION]: exact-rational tables + moment-fit fallback via `matrix.md`; carriers `DenseOutputCoefficientFamily` · `DenseOutputReceipt` · `ButcherDenseOutput`.
- [04]-[STEP_ALGEBRA]: carrier-generic policy records + `[Union]` stepper — `IntegrationModule<TState,TDelta>` (THE `Combine`) with `StepControl` · `FieldIntegrator` · `IntegrationStep` · `DenseOutputSpan`.

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
