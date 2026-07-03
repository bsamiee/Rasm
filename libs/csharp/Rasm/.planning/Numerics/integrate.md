# [RASM_NUMERICS_INTEGRATE]

The ODE/Runge-Kutta integration floor of `Rasm.Vectors` — pure numerics with zero geometric content. The page owns `IntegratorKind`, the nine-tableau data-driven integrator vocabulary (Euler through Dormand-Prince, fixed and embedded-adaptive); `ButcherTableau`, the coefficient carrier whose admission VALIDATES the Runge-Kutta order conditions numerically (row-sum consistency plus the moment conditions through order four) rather than asserting them; `DenseOutputCoefficientFamily`, the continuous-extension coefficient owner carrying the exact-rational Dormand-Prince/Shampine and Bogacki-Shampine interpolant tables with Horner evaluation, plus the generic least-squares moment-fit fallback solved through the `matrix.md` owners; `ButcherDenseOutput`, the dense-output receipt derivation proving endpoint, derivative, and moment residuals at construction; and the stepper — `IntegrationModule<TState, TDelta>`, the additive-module policy record that makes ONE step function serve scalar, vector, and geometric state carriers, `StepControl`, the adaptive step-control policy row, `FieldIntegrator`, the Fixed/Adaptive union whose generic `Step` computes the RK stages, applies the embedded-pair PI error control, and mints a `DenseOutputSpan` for event localization.

The one `Combine(coefficients, deltas)` linear-combination fold lives on the module — the mature corpus carried it twice verbatim; both copies collapse here. Geometry enters only at the `Processing/flow` consumer, which supplies the spatial `IntegrationModule` instance over `Point3d`/`Vector3d` and folds accepted steps into streamline state; this page never names a geometric type.

## [01]-[INDEX]

- [02]-[TABLEAU_VOCABULARY]: `IntegratorKind` nine-row tableau vocabulary + `ButcherTableau` order-condition-validated coefficient carrier + `ButcherMomentReceipt`.
- [03]-[DENSE_OUTPUT]: `DenseOutputCoefficientFamily` exact-rational interpolant tables + `DenseOutputReceipt` + `ButcherDenseOutput` moment-fit derivation via `matrix.md`.
- [04]-[STEPPER]: `IntegrationModule<TState,TDelta>` additive-module policy (THE one `Combine` fold) + `StepControl` + `FieldIntegrator` Fixed/Adaptive union + `IntegrationStep<TState,TDelta>` + `DenseOutputSpan<TState,TDelta>`.

## [02]-[TABLEAU_VOCABULARY]

- Owner: `IntegratorKind` the `[SmartEnum<int>]` whose nine rows ARE the Butcher tableaux — coupling matrix, abscissae (derived as row sums at declaration), weights, and for embedded pairs the error weights and embedded order — constructed through the two private `Fixed`/`Adaptive` factories so a row is one declaration; `ButcherTableau` the coefficient carrier (`Coupling`/`Abscissae`/`Weights`/`EmbeddedWeights`/`MethodOrder`/`EmbeddedOrder`) whose `IsValid` runs the REAL order-condition mathematics: weights sum to one, each coupling row sums to its abscissa, the moment conditions `Σbᵢcᵢ = 1/2`, `Σbᵢcᵢ² = 1/3`, `Σbᵢcᵢ³ = 1/4`, `Σbᵢ(Ac)ᵢ = 1/6` hold through the declared order, and the embedded weights independently satisfy their own order — a mis-transcribed coefficient is a construction-time typed failure, never a silently wrong trajectory; `ButcherMomentReceipt` the moment-validation evidence (checked/failed condition counts + max residual).
- Cases: `Euler(1)` · `Heun(2)` · `Midpoint(2)` · `Ralston(2)` · `RK4(4)` · `RK38(4)` fixed; `BogackiShampine(3/2)` · `CashKarp(5/4)` · `DormandPrince(5/4)` adaptive (9).
- Entry: `IntegratorKind.<Row>.Tableau` reads the validated carrier; `ButcherTableau.Admit(Op key)` gates a tableau onto the rail; `Tableau.MomentReceipt` re-derives the moment evidence on demand; `IsFunctionalSameAsLast` detects the FSAL structure (last stage equals the weight row) that fingerprints the method-specific dense-output families.
- Auto: abscissae never enter as data — `Fixed`/`Adaptive` derive them as coupling row sums, so the consistency condition `cᵢ = Σaᵢⱼ` is true by construction and re-checked by `IsValid` as the transcription witness; `AdaptiveExponent` derives the PI-control exponent `1/(q+1)` from the embedded order.
- Receipt: `ButcherMomentReceipt(StageCount, MethodOrder, EmbeddedOrder, CheckedConditionCount, FailedConditionCount, MaxResidual)` — on the `[ValidityEvidence]` fold with the `FailedConditionCount == 0 && MaxResidual <= CoefficientTolerance` semantic gate.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Seq`, `Option`, `Fin`), TYoshimura.DoubleDouble (`DDouble` — the 106-bit compensated lane for moment-condition folds when high-stage tableaux push residual sums past binary64 fold precision).
- Growth: a new integrator (Tsitouras 5(4), Verner 6(5)) is ONE `IntegratorKind` row — coupling, weights, error weights — with the order conditions validating the transcription automatically; a new order condition (order-5 moment rows) is one `Check` line in `MomentReceiptOf` tightening every row at once.
- Boundary: `CoefficientTolerance = 1.0e-9` is the tableau's own documented order-condition residual band — exact-rational coefficients evaluate to residuals near machine epsilon, so the band catches transcription errors, not roundoff; tableau data lives ONLY on the vocabulary rows — a consumer never spells a coupling coefficient.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
namespace Rasm.Vectors;

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
    // FSAL fingerprint: last abscissa 1, last weight 0, last coupling row equal to the weight prefix —
    // the structure the method-specific dense-output families key on.
    internal bool IsFunctionalSameAsLast =>
        StageCount > 1
        && Coupling.Count == StageCount
        && Math.Abs(value: Abscissae[StageCount - 1] - 1.0) <= CoefficientTolerance
        && Math.Abs(value: Weights[StageCount - 1]) <= CoefficientTolerance
        && Coupling[StageCount - 1].Count == StageCount - 1
        && Coupling[StageCount - 1].Zip(Weights.Take(StageCount - 1)).ForAll(static pair => Math.Abs(value: pair.First - pair.Second) <= CoefficientTolerance);
    public ButcherMomentReceipt MomentReceipt => MomentReceiptOf(weights: Weights, order: MethodOrder, embeddedOrder: EmbeddedOrder);
    internal bool IsValid =>
        StageCount > 0
        && MethodOrder > 0
        && (EmbeddedOrder is not { IsSome: true, Case: int embedded } || (embedded > 0 && embedded < MethodOrder))
        && Coupling.Count == StageCount
        && Abscissae.Count == StageCount
        && Abscissae.ForAll(double.IsFinite)
        && CoefficientsMatch(values: Weights, expected: 1.0)
        && MomentReceipt.IsValid
        && Coupling.Zip(Abscissae).AsIterable().Select((pair, index) => pair.First.Count <= index
            && CoefficientsMatch(values: pair.First, expected: pair.Second)).All(static ok => ok)
        && (EmbeddedWeights is not { IsSome: true, Case: Seq<double> ew } || (ew.Count == StageCount && CoefficientsMatch(values: ew, expected: 1.0) && MomentReceiptOf(weights: ew, order: EmbeddedOrder.IfNone(1), embeddedOrder: EmbeddedOrder).IsValid));
    internal Fin<ButcherTableau> Admit(Op key) => IsValid ? Fin.Succ(this) : Fin.Fail<ButcherTableau>(key.InvalidInput());
    internal Fin<DenseOutputReceipt> DenseOutputReceipt(Op key) => ButcherDenseOutput.Receipt(tableau: this, key: key);
    internal Fin<Seq<double>> DenseWeightsAt(double theta, Op key) => ButcherDenseOutput.WeightsAt(tableau: this, theta: theta, key: key);
    private static bool CoefficientsMatch(Seq<double> values, double expected) =>
        values.ForAll(double.IsFinite)
        && Math.Abs(value: values.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - expected) <= CoefficientTolerance;
    private ButcherMomentReceipt MomentReceiptOf(Seq<double> weights, int order, Option<int> embeddedOrder) {
        (int Count, int Failed, double Max) state = (0, 0, 0.0);
        state = Check(state: state, actual: weights.Zip(Abscissae).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second)), expected: 0.5, active: order >= 2);
        state = Check(state: state, actual: weights.Zip(Abscissae).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second * pair.Second)), expected: 1.0 / 3.0, active: order >= 3);
        state = Check(state: state, actual: weights.Zip(Abscissae).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second * pair.Second * pair.Second)), expected: 0.25, active: order >= 4);
        state = Check(state: state, actual: weights.Zip(Ac(coupling: Coupling, abscissae: Abscissae)).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second)), expected: 1.0 / 6.0, active: order >= 3);
        return new ButcherMomentReceipt(StageCount: StageCount, MethodOrder: order, EmbeddedOrder: embeddedOrder, CheckedConditionCount: state.Count, FailedConditionCount: state.Failed, MaxResidual: state.Max);
    }
    private static Seq<double> Ac(Seq<Seq<double>> coupling, Seq<double> abscissae) =>
        coupling.Map(row => row.Zip(abscissae).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second)));
    private static (int Count, int Failed, double Max) Check((int Count, int Failed, double Max) state, double actual, double expected, bool active) {
        if (!active) return state;
        double residual = Math.Abs(value: actual - expected);
        return (Count: state.Count + 1, Failed: state.Failed + (double.IsFinite(residual) && residual <= CoefficientTolerance ? 0 : 1), Max: Math.Max(val1: state.Max, val2: residual));
    }
}

[ValidityEvidence, StructLayout(LayoutKind.Auto)]
public readonly partial record struct ButcherMomentReceipt(int StageCount, int MethodOrder, Option<int> EmbeddedOrder, int CheckedConditionCount, int FailedConditionCount, double MaxResidual) : IValidityEvidence {
    private bool ValidityGate() => FailedConditionCount == 0 && MaxResidual <= ButcherTableau.CoefficientTolerance;
}
```

## [03]-[DENSE_OUTPUT]

- Owner: `DenseOutputCoefficientFamily` the `[SmartEnum<int>]` continuous-extension owner — `DormandPrinceShampine` (dense order 4) and `BogackiShampine` (dense order 3) carry their published EXACT-RATIONAL interpolant coefficient tables as row data with an abscissae + FSAL fingerprint (`Identify` matches a tableau to its family, falling to `GenericMomentFit`); `WeightsAt`/`DerivativeAt` evaluate the polynomial rows by Horner and Horner-derivative; `ButcherDenseOutput` the derivation kernel — `Receipt` proves the interpolant at construction (moment residuals at θ ∈ {0, ½, 1}, endpoint value/derivative residuals, coefficient-consistency against the step weights), `WeightsAt` yields the stage weights of the continuous extension `b(θ)` for any θ ∈ [0,1], and the generic fallback solves the least-squares moment fit — a Vandermonde preimage (`MomentPreimage`) plus a moment-design least-squares correction (`Correction`) — through `Matrix.SolveDetailed`/`LeastSquaresDetailed`, so the interpolant construction itself leaves a `SolveReceipt` inside the dense-output evidence.
- Cases: `GenericMomentFit` · `DormandPrinceShampine` · `BogackiShampine` (3).
- Entry: consumers never reach the family directly — `tableau.DenseWeightsAt(theta, key)` and `tableau.DenseOutputReceipt(key)` are the two entries, with the family identified from the tableau fingerprint each time.
- Auto: the generic route pins the endpoints exactly — `b(0) = 0`, `b(1) = weights` — and fits only the interior through the `θ(1−θ)`-scaled correction, so endpoint continuity is structural; `DenseOrderFor` caps the generic dense order by the count of distinct abscissae (the Vandermonde rank ceiling).
- Receipt: `DenseOutputReceipt` — stage/order/dense-order + per-θ moment evidence + endpoint value/derivative residuals + coefficient residual + the identified family + the optional generic-fit `SolveReceipt` — on the `[ValidityEvidence]` fold with the semantic gate coupling every residual to `CoefficientTolerance` and the family to its evidence shape (method-specific families carry no correction solve; the generic family must).
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, `matrix.md` owners (`Matrix`, `SolveReceipt`) — the moment fit is the first in-corpus consumer of the dense least-squares route.
- Growth: a new published interpolant (Tsitouras dense output) is one family row — fingerprint, order, table; a tableau without a published interpolant costs nothing — the generic moment fit covers it at the Vandermonde-rank order.
- Boundary: interpolant tables are exact rationals spelled as ratios (`-8048581381.0 / 2820520608.0`), never decimal approximations — the moment validation would flag the drift; dense output is the event-localization substrate `Processing/flow` binds for root bisection, and a consumer interpolating trajectories by chord instead of `b(θ)` re-derives a capability this owner already proves.

```csharp
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
[ValidityEvidence, StructLayout(LayoutKind.Auto)]
public readonly partial record struct DenseOutputReceipt(int StageCount, int MethodOrder, int DenseOrder, int CheckedThetaCount, int CheckedConditionCount, int FailedConditionCount, double MaxResidual, bool UsesStageDerivatives, double EndpointValueResidualLeft, double EndpointValueResidualRight, double EndpointDerivResidualLeft, double EndpointDerivResidualRight, double CoefficientResidual, DenseOutputCoefficientFamily? CoefficientFamily = null, bool GenericCorrectionSolve = false, Option<SolveReceipt> CorrectionSolve = default) : IValidityEvidence {
    private bool ValidityGate() =>
        DenseOrder <= MethodOrder
        && CheckedConditionCount >= CheckedThetaCount
        && FailedConditionCount == 0
        && MaxResidual <= ButcherTableau.CoefficientTolerance
        && UsesStageDerivatives
        && EndpointValueResidualLeft <= ButcherTableau.CoefficientTolerance
        && EndpointValueResidualRight <= ButcherTableau.CoefficientTolerance
        && CoefficientResidual <= ButcherTableau.CoefficientTolerance
        && CoefficientFamily is not null
        && (!CoefficientFamily.MethodSpecific || EndpointDerivResidualLeft <= ButcherTableau.CoefficientTolerance)
        && GenericCorrectionSolve == CoefficientFamily.Equals(DenseOutputCoefficientFamily.GenericMomentFit)
        && (!CoefficientFamily.MethodSpecific || CorrectionSolve.IsNone);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class ButcherDenseOutput {
    internal static Fin<DenseOutputReceipt> Receipt(ButcherTableau tableau, Op key) {
        DenseOutputCoefficientFamily family = DenseOutputCoefficientFamily.Identify(tableau: tableau);
        int order = DenseOrderFor(family: family, tableau: tableau);
        return ReceiptAt(family: family, tableau: tableau, order: order, theta: 0.0, key: key).Bind(zero =>
            ReceiptAt(family: family, tableau: tableau, order: order, theta: 0.5, key: key).Bind(mid =>
                ReceiptAt(family: family, tableau: tableau, order: order, theta: 1.0, key: key).Bind(one =>
                    EndpointEvidence(family: family, tableau: tableau, order: order, key: key).Bind(evidence => {
                        DenseOutputReceipt receipt = new(
                            StageCount: tableau.StageCount, MethodOrder: tableau.MethodOrder, DenseOrder: order,
                            CheckedThetaCount: 3,
                            CheckedConditionCount: zero.CheckedConditionCount + mid.CheckedConditionCount + one.CheckedConditionCount,
                            FailedConditionCount: zero.FailedConditionCount + mid.FailedConditionCount + one.FailedConditionCount,
                            MaxResidual: Math.Max(val1: zero.MaxResidual, val2: Math.Max(val1: mid.MaxResidual, val2: one.MaxResidual)),
                            UsesStageDerivatives: true,
                            EndpointValueResidualLeft: evidence.ValueLeft, EndpointValueResidualRight: evidence.ValueRight,
                            EndpointDerivResidualLeft: evidence.DerivLeft, EndpointDerivResidualRight: evidence.DerivRight,
                            CoefficientResidual: evidence.Coefficient, CoefficientFamily: family,
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
    // Scratch per-theta carrier: Receipt aggregates only condition counts, max residual, and the
    // correction solve from this; the surfaced receipt fills the endpoint slots from EndpointEvidence.
    private static Fin<DenseOutputReceipt> ReceiptAt(DenseOutputCoefficientFamily family, ButcherTableau tableau, int order, double theta, Op key) =>
        Weights(family: family, tableau: tableau, order: order, theta: theta, key: key).Bind(result => {
            Seq<double> weights = result.Values;
            (bool failed, double maxResidual) = MomentResidual(tableau: tableau, weights: weights, theta: theta, order: order);
            double endpoint = theta <= ButcherTableau.CoefficientTolerance
                ? weights.Fold(initialState: 0.0, f: static (max, value) => Math.Max(val1: max, val2: Math.Abs(value: value)))
                : 1.0 - theta <= ButcherTableau.CoefficientTolerance
                    ? weights.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second)))
                    : 0.0;
            DenseOutputReceipt receipt = new(
                StageCount: tableau.StageCount, MethodOrder: tableau.MethodOrder, DenseOrder: order,
                CheckedThetaCount: 1,
                CheckedConditionCount: order + ((theta <= ButcherTableau.CoefficientTolerance || 1.0 - theta <= ButcherTableau.CoefficientTolerance) ? tableau.StageCount : 0),
                FailedConditionCount: (failed ? 1 : 0) + (endpoint <= ButcherTableau.CoefficientTolerance ? 0 : 1),
                MaxResidual: Math.Max(val1: maxResidual, val2: endpoint),
                UsesStageDerivatives: true,
                EndpointValueResidualLeft: 0.0, EndpointValueResidualRight: 0.0,
                EndpointDerivResidualLeft: 0.0, EndpointDerivResidualRight: 0.0,
                CoefficientResidual: 0.0, CoefficientFamily: family,
                GenericCorrectionSolve: !family.MethodSpecific, CorrectionSolve: result.Solve);
            return receipt.IsValid ? Fin.Succ(receipt) : Fin.Fail<DenseOutputReceipt>(key.InvalidResult());
        });
    private static Fin<(double ValueLeft, double ValueRight, double DerivLeft, double DerivRight, double Coefficient)> EndpointEvidence(DenseOutputCoefficientFamily family, ButcherTableau tableau, int order, Op key) =>
        family.MethodSpecific
            ? from atOne in family.WeightsAt(theta: 1.0, stageCount: tableau.StageCount, key: key)
              from atZero in family.WeightsAt(theta: 0.0, stageCount: tableau.StageCount, key: key)
              from derivOne in family.DerivativeAt(theta: 1.0, stageCount: tableau.StageCount, key: key)
              from derivZero in family.DerivativeAt(theta: 0.0, stageCount: tableau.StageCount, key: key)
              select (
                  ValueLeft: MaxAbs(values: atZero),
                  ValueRight: Math.Abs(value: atOne.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - tableau.Weights.Fold(initialState: 0.0, f: static (sum, value) => sum + value)),
                  DerivLeft: MaxDeviation(values: derivZero, target: 0),
                  DerivRight: MaxDeviation(values: derivOne, target: tableau.StageCount - 1),
                  Coefficient: atOne.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second))))
            : Weights(family: family, tableau: tableau, order: order, theta: 1.0, key: key).Bind(atOne =>
                Weights(family: family, tableau: tableau, order: order, theta: 0.0, key: key).Map(atZero => (
                    ValueLeft: MaxAbs(values: atZero.Values),
                    ValueRight: Math.Abs(value: atOne.Values.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - tableau.Weights.Fold(initialState: 0.0, f: static (sum, value) => sum + value)),
                    DerivLeft: 0.0,
                    DerivRight: 0.0,
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
            .Select(m => Math.Abs(value: weights.Zip(tableau.Abscissae).Fold(initialState: 0.0, f: (sum, pair) => sum + (pair.First * Math.Pow(x: pair.Second, y: m))) - (Math.Pow(x: theta, y: m + 1) / (m + 1.0))))
            .Aggregate(seed: (Failed: false, Max: 0.0), func: static (state, residual) => (
                Failed: state.Failed || !double.IsFinite(residual) || residual > ButcherTableau.CoefficientTolerance,
                Max: Math.Max(val1: state.Max, val2: residual)));
}
```

## [04]-[STEPPER]

- Owner: `IntegrationModule<TState, TDelta>` the additive-module policy record — the four operations one Runge-Kutta step needs (`Add: (TState, h, TDelta) → TState`, `Scale: (double, TDelta) → TDelta`, `Sum: (TDelta, TDelta) → TDelta`, `Norm: TDelta → double`, plus the `Zero` delta) with `Combine(coefficients, deltas)` as THE one linear-combination fold in the corpus and the `Scalar` canonical instance for `double`/`double` state; `StepControl` the adaptive-control policy row (`SafetyFactor`/`MinScale`/`MaxScale`, `Default = (0.9, 0.2, 10.0)`) whose `Rescale` applies the PI step law `h·clamp(safety·(tol/err)^(1/(q+1)))`; `FieldIntegrator` the `[Union]` Fixed/Adaptive integrator whose generic `Step` computes the RK stages by folding the coupling rows through the derivative sampler, forms the primary (and for adaptive pairs the embedded) combination, applies the error control, and mints the dense-output span; `IntegrationStep<TState, TDelta>` the Accepted/Rejected step outcome union; `DenseOutputSpan<TState, TDelta>` the per-step continuous extension (`PointAt(θ)` through the dense weights) whose construction re-verifies the θ=1 endpoint against the step result.
- Cases: `FieldIntegrator` `FixedCase(kind)` · `AdaptiveCase(kind, tolerance, maxRejects, control)` (2); `IntegrationStep` `AcceptedCase(next, suggestedStep, error, dense)` · `RejectedCase(suggestedStep, error)` (2).
- Entry: `FieldIntegrator.Fixed(kind)` / `Adaptive(kind, tolerance, maxRejects = 3)` — admission re-validates the tableau and the kind/case agreement (a fixed integrator over an embedded kind or the reverse fails typed); `Step(module, sample, state, h, key)` with `sample: Func<TState, Fin<TDelta>>` — the derivative field abstracted as a sampler, so the SAME stepper integrates a scalar ODE, a spatial streamline, or any admitted carrier; `AdmitOrFixed(value, key)` defaults an absent integrator to `Fixed(RK4)`.
- Auto: stage computation is one fold over the coupling rows — each stage samples at `state + h·Combine(row, stages)`; the adaptive arm reads the error as `module.Norm(primary − secondary)` via the delta between combinations, rescales through `StepControl`, and returns `RejectedCase` with the shrunk suggestion instead of looping — the REJECT LOOP BELONGS TO THE CONSUMER's fold (`Processing/flow` owns the reject budget and iteration policy as state), so the stepper stays a pure step function.
- Receipt: the dense span carries the [03] `DenseOutputReceipt`; step error and suggested step ride the outcome cases.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core; zero geometry — `Rasm.Domain` `Op` only.
- Growth: a new state carrier is one `IntegrationModule` instance at its consumer (the spatial `Point3d`/`Vector3d` module is `Processing/flow`'s one declaration); a new control law (PI with memory, Gustafsson) is one `StepControl` field set — the stepper body does not change.
- Boundary: the mature corpus duplicated `Combine` verbatim in the stepper and the dense-output state and hard-wired both to `Vector3d` — the module collapse deletes both; the difference of two states never appears (only deltas subtract), so the module needs no `TState` subtraction — the error is measured between the two weight combinations BEFORE adding to the state, keeping the algebra minimal; `MaxRejects` is consumer policy carried on the Adaptive case, never a hidden kernel constant.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
// The additive-module policy: one RK stepper serves every state carrier that supplies these four
// operations. THE one Combine fold — the corpus' single linear-combination site.
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
    internal static Fin<DenseOutputSpan<TState, TDelta>> Of(IntegrationModule<TState, TDelta> module, TState start, TState end, double step, Seq<TDelta> stages, ButcherTableau tableau, Op key) =>
        tableau.DenseOutputReceipt(key: key).Bind(receipt =>
            tableau.DenseWeightsAt(theta: 1.0, key: key).Bind(weights => {
                TState endpoint = module.Add(arg1: start, arg2: step, arg3: module.Combine(coefficients: weights, deltas: stages));
                // theta = 1 must reproduce the step result within the norm band, or the interpolant is rejected.
                return double.IsFinite(module.Norm(arg: module.Combine(coefficients: weights, deltas: stages)))
                    && stages.Count == tableau.StageCount && Math.Abs(value: step) > EpsilonPolicy.ZeroTolerance && receipt.IsValid
                    ? EndpointAgrees(module: module, left: endpoint, right: end, start: start, step: step, stages: stages, tableau: tableau, key: key)
                        .Map(_ => new DenseOutputSpan<TState, TDelta>(Start: start, End: end, Step: step, Stages: stages, Tableau: tableau, Receipt: receipt, Module: module))
                    : Fin.Fail<DenseOutputSpan<TState, TDelta>>(key.InvalidResult());
            }));
    public Fin<TState> PointAt(double theta, Op key) {
        if (!double.IsFinite(theta) || theta is < 0.0 or > 1.0) return Fin.Fail<TState>(key.InvalidInput());
        DenseOutputSpan<TState, TDelta> self = this;
        return Tableau.DenseWeightsAt(theta: theta, key: key)
            .Map(weights => self.Module.Add(arg1: self.Start, arg2: self.Step, arg3: self.Module.Combine(coefficients: weights, deltas: self.Stages)));
    }
    private static Fin<Unit> EndpointAgrees(IntegrationModule<TState, TDelta> module, TState left, TState right, TState start, double step, Seq<TDelta> stages, ButcherTableau tableau, Op key) =>
        tableau.DenseWeightsAt(theta: 1.0, key: key).Bind(weights => {
            TDelta reconstructed = module.Combine(coefficients: weights, deltas: stages);
            TDelta declared = module.Combine(coefficients: tableau.Weights, deltas: stages);
            double drift = module.Norm(arg: module.Sum(arg1: reconstructed, arg2: module.Scale(arg1: -1.0, arg2: declared)));
            return guard(drift * Math.Abs(value: step) <= EpsilonPolicy.SqrtEpsilon, key.InvalidResult()).ToFin().Map(static _ => unit);
        });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union]
public abstract partial record FieldIntegrator {
    public sealed record FixedCase : FieldIntegrator { internal FixedCase(IntegratorKind kind) => Kind = kind; public IntegratorKind Kind { get; } }
    public sealed record AdaptiveCase : FieldIntegrator {
        internal AdaptiveCase(IntegratorKind kind, PositiveMagnitude tolerance, int maxRejects, StepControl control) { Kind = kind; Tolerance = tolerance; MaxRejects = maxRejects; Control = control; }
        public IntegratorKind Kind { get; }
        public PositiveMagnitude Tolerance { get; }
        public int MaxRejects { get; }
        public StepControl Control { get; }
    }
    private FieldIntegrator() { }
    public static Fin<FieldIntegrator> Fixed(IntegratorKind kind, Op? key = null) =>
        Admit(value: new FixedCase(kind: kind), key: key.OrDefault());
    public static Fin<FieldIntegrator> Adaptive(IntegratorKind kind, double tolerance, int maxRejects = 3, StepControl? control = null, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
            .Bind(validated => Admit(value: new AdaptiveCase(kind: kind, tolerance: validated, maxRejects: maxRejects, control: control ?? StepControl.Default), key: op));
    }
    public int RejectBudget => Switch(state: 0, fixedCase: static (s, _) => s, adaptiveCase: static (_, c) => c.MaxRejects);
    internal ButcherTableau Tableau => Switch(state: default(ButcherTableau), fixedCase: static (_, c) => c.Kind.Tableau, adaptiveCase: static (_, c) => c.Kind.Tableau);
    public int MethodOrder => Tableau.MethodOrder;
    public Option<int> EmbeddedOrder => Tableau.EmbeddedOrder;
    internal Fin<FieldIntegrator> Admit(Op key) =>
        Switch(
            state: key,
            fixedCase: static (op, integrator) =>
                from kind in Optional(integrator.Kind).ToFin(op.InvalidInput())
                from tableau in kind.Tableau.Admit(key: op)
                from fixedKind in guard(!kind.IsAdaptive, op.Unsupported(geometryType: kind.GetType(), outputType: typeof(FixedCase)))
                select (FieldIntegrator)integrator,
            adaptiveCase: static (op, integrator) =>
                from kind in Optional(integrator.Kind).ToFin(op.InvalidInput())
                from tableau in kind.Tableau.Admit(key: op)
                from rejects in guard(integrator.MaxRejects >= 0, op.InvalidInput())
                from adaptiveKind in guard(kind.IsAdaptive, op.Unsupported(geometryType: kind.GetType(), outputType: typeof(AdaptiveCase)))
                select (FieldIntegrator)integrator);
    public static Fin<FieldIntegrator> Admit(FieldIntegrator value, Op key) =>
        Optional(value).ToFin(key.InvalidInput()).Bind(integrator => integrator.Admit(key: key));
    public static Fin<FieldIntegrator> AdmitOrFixed(FieldIntegrator? value, Op key) =>
        value is null ? Fixed(kind: IntegratorKind.RK4, key: key) : Admit(value: value, key: key);
    // One generic step: stage fold -> combination(s) -> error control -> dense span. The reject loop
    // is the CONSUMER's fold (Processing/flow owns budget and iteration state); this stays a pure step.
    public Fin<IntegrationStep<TState, TDelta>> Step<TState, TDelta>(IntegrationModule<TState, TDelta> module, Func<TState, Fin<TDelta>> sample, TState state, double h, Op key) => Switch(
        state: (Module: module, Sample: sample, State: state, H: h, Key: key),
        fixedCase: static (s, c) =>
            from ks in Stages(module: s.Module, sample: s.Sample, tableau: c.Kind.Tableau, state: s.State, h: s.H, key: s.Key)
            let next = s.Module.Add(arg1: s.State, arg2: s.H, arg3: s.Module.Combine(coefficients: c.Kind.Tableau.Weights, deltas: ks))
            from dense in DenseOutputSpan<TState, TDelta>.Of(module: s.Module, start: s.State, end: next, step: s.H, stages: ks, tableau: c.Kind.Tableau, key: s.Key)
            select (IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.AcceptedCase(Next: next, SuggestedStep: s.H, Error: Option<double>.None, Dense: dense),
        adaptiveCase: static (s, c) =>
            from embeddedWeights in c.Kind.Tableau.EmbeddedWeights.ToFin(Fail: s.Key.InvalidInput())
            from ks in Stages(module: s.Module, sample: s.Sample, tableau: c.Kind.Tableau, state: s.State, h: s.H, key: s.Key)
            let primary = s.Module.Combine(coefficients: c.Kind.Tableau.Weights, deltas: ks)
            let secondary = s.Module.Combine(coefficients: embeddedWeights, deltas: ks)
            let err = Math.Abs(value: s.H) * s.Module.Norm(arg: s.Module.Sum(arg1: primary, arg2: s.Module.Scale(arg1: -1.0, arg2: secondary)))
            let scale = c.Control.Rescale(error: err, tolerance: c.Tolerance.Value, exponent: c.Kind.AdaptiveExponent)
            from result in err <= c.Tolerance.Value
                ? DenseOutputSpan<TState, TDelta>.Of(module: s.Module, start: s.State, end: s.Module.Add(arg1: s.State, arg2: s.H, arg3: primary), step: s.H, stages: ks, tableau: c.Kind.Tableau, key: s.Key)
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

## [05]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]      | [OWNER]                          | [KIND]                                                          | [CASES] |
| :-----: | :------------------ | :-------------------------------- | :--------------------------------------------------------------- | :-----: |
|  [01]   | Integrator rows     | `IntegratorKind`                 | `[SmartEnum<int>]` — the tableau IS the row                     |    9    |
|  [02]   | Coefficient carrier | `ButcherTableau` + `ButcherMomentReceipt` | order-condition-validated record + `[ValidityEvidence]` receipt |    1    |
|  [03]   | Continuous extension | `DenseOutputCoefficientFamily` · `DenseOutputReceipt` · `ButcherDenseOutput` | exact-rational tables + moment-fit fallback via `matrix.md`     |    3    |
|  [04]   | Step algebra        | `IntegrationModule<TState,TDelta>` (THE `Combine`) · `StepControl` · `FieldIntegrator` · `IntegrationStep` · `DenseOutputSpan` | carrier-generic policy records + `[Union]` stepper              |   2·2   |
