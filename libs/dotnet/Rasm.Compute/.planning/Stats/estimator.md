# [COMPUTE_ESTIMATOR]

Rasm.Compute statistical-learning CONTRACT: one `Estimator` `[Union]` carrying a uniform `Fit(Estimator, Design, EstimatorPolicy, FitAmbients) → FittedModel` / `Predict(FittedModel, PredictQuery) → Prediction` contract across regression, reduction, clustering, classification, forecasting, and detection families, keyed to one `EstimatorModel` fit-result carrier. The contract stays uniform while every row owns its own mechanism; `Stats/families#FAMILY_ROWS` holds which rows exist and how each one fits, so a new family costs a row there and nothing here. `Design.Admit` proves raw evidence once; row admission then proves response support, feature, label, history, curve-support, detector range, kernel parameter, and row-count ceiling.

Dense factorizations ride `Tensor/blas#DENSE_ALGEBRA`; criterion sums accumulate exactly through `PeterO.Numbers`; every bounded fold answers the `Solver/contract#SOLVE_REQUEST` `Convergence` verdict, so a spent budget is `Exhausted` carrying the budget and never a success-shaped fall-through; `ComputeReceipt`, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, and `ComparerAccessors.StringOrdinal` arrive settled; NodaTime `IClock` supplies the semantic `Instant` a fit receipt stamps while kernel `MonotonicTimeline` supplies its elapsed span, the app-stratum `ClockPolicy` reaching neither; `Tensor/blas#DENSE_ALGEBRA` `AtenFloor.Configure` pins the torch default dtype once at boot and no fit re-pins it. Conditioned multi-channel spectral evidence enters detection from `Stats/signal#SIGNAL_LANE`; a fit lands the dedicated `Runtime/receipts#RECEIPT_UNION` `Fit` case; offline deep-training studies cross `GraduationEvidence` by content key, and the published model-asset manifest riding that message envelope is the sole signature authority — the graduation-decode gate reads feature names off the manifest, never off a companion-published tensor signature.

## [01]-[INDEX]

- [02]-[ESTIMATOR_LANE]: the Fit/Predict/Validate/Select contract over one `EstimatorModel` carrier and one `Prediction` egress; the admitted `Design`, the family-shaped `EstimatorPolicy`, the composition ambients, the bounded `ResidualWindow` every detector consumer pushes into, and the ranked `SelectionReport`.

## [02]-[ESTIMATOR_LANE]

- Owner: `Estimator` types the problem; `ClusterShape` types grouping ingress; `PredictQuery` types prediction ingress as evidence or horizon; `EstimatorModel` carries fitted parameters and `SupportMachine` the per-label margin row; `Prediction` types response, projection, assignment, or anomaly egress; `Design` admits evidence once and `EstimatorPolicy` admits family policy, `ScaleCeiling` capping the quadratic and cubic kernels and `KernelParams` carrying per-occurrence kernel parameters; `FitBudget` binds an iteration `Dimension` to a lane-typed convergence `Tolerance`; `FitAmbients` is the ONE composition carrier — clock, dense substrate, and deterministic draw — every entry takes; `FitContext` carries the proven correspondence beside those ambients; `ResidualWindow` over `WindowCapacity` is the bounded detector evidence ring every online consumer pushes into; `InformationCriterion` rows score a fitted likelihood, `CriterionPolicy` admits the selection request and `SelectionReport` carries its ranked verdict; `EstimatorFold` owns `Fit`, `Predict`, `Validate`, `Select`, the admission gates, the shared numeric helpers, and the prediction readers.
- Cases: `Estimator` regression · curve · reduction · cluster · classify · temporal; `ClusterShape` partitioned · density; `EstimatorModel` linear · curve · basis · kernel-basis · factors · partition · density · mixture · margin · neighbors · bayes · lag · detector; `Prediction` response · projection · assignment · anomaly; `EstimatorPolicy` regression · reduction · grouping · classification · temporal; `InformationCriterion` aic · bic.
- Entry: `Design.Admit(Matrix<double>, Option<Vector<double>>)` proves non-empty, finite, aligned evidence, ACCUMULATING across its independent columns so a caller learns every violated one. `Fit(Estimator, Design, EstimatorPolicy, FitAmbients)` proves family correspondence, policy ranges, estimator support, kernel parameters, row-count ceilings, and spec history/ranges before dispatch. `Predict` projects, assigns, forecasts, or scores anomalies through the total `EstimatorModel` switch over one `PredictQuery` ingress. `Validate(Estimator, Design, EstimatorPolicy, CriterionPolicy, FitAmbients)` scores supervised held-out folds and forecasting forward chains and takes the POLICY that owns the fold count, never a loose int beside it; unsupervised detectors do not fabricate validation labels. `Select(Estimator, Design, Seq<EstimatorPolicy>, CriterionPolicy, FitAmbients)` fits each candidate, scores its information criterion where the fit carries a likelihood, folds its validation spread, and returns one ranked `SelectionReport`.
- Law: composition ambients ride ONE carrier. Clock, dense substrate, and deterministic draw are three composition-supplied values every entry needs and none derives, so they arrive as `FitAmbients` rather than as a growing parameter tail four entrypoints each re-declared and each threaded through to the same `FitContext` slot.
- Law: a bounded fold answers `Convergence`, never `bool`. `FittedModel.Verdict` is the `Solver/contract#SOLVE_REQUEST` union the whole package already reads, so a spent budget is `Exhausted(budget)` carrying what it spent and a settled fit is `Converged(residual)` carrying what it measured — where the deleted `bool Converged` let a GLM under a thousand-iteration cap publish an unconverged carrier through a success arm, and the receipt's own `Converged` column then reported it as a fit.
- Law: absence on the carrier is an `Option`, never a zero. `RetainedRank` answers `None` on the ten non-reduction carriers rather than `0`, because a reduction that genuinely retained no component and a classifier that has no rank at all are two facts one integer collapsed — `FORGED_ZERO` at its exact grain. The receipt's `int RetainedRank` column takes the collapse at the WIRE edge, which is the one place an option is allowed to become a default.
- Law: the selection request is one policy value. `Validate` took a loose `int folds` beside a `CriterionPolicy` that already carried `Folds`, so two authorities named one quantity and a caller could disagree with itself between the two calls `Select` makes.
- Auto: `Fit` flattens the estimator's typed payloads once into `FitContext` — which carries the policy union member itself, never a zero-filled scalar tail — then dispatches the row kernel `Stats/families` binds. `Validate` derives contiguous, forward-chain, or unsupported behavior from the typed estimator case. The validation spread reduces through the kernel `Rasm/Domain/stats#MOMENTS` `Stat<Scalar>` four-moment fold under an explicit `MomentNormalizer`, never a page-local mean-then-squares pass that leaves its Bessel convention unstated on the report.
- Receipt: a fit emits the dedicated `Fit` `ComputeReceipt` case `Runtime/receipts#RECEIPT_UNION` declares for this lane (one case row per measured concern, as the FEA `Solver/contract#SOLVE_REQUEST` `Solve` and the optimizer/sweep/clash/twin/uncertainty cases each own a row rather than overloading a sibling), carrying family, estimator key, carrier parameter count, step count, residual, the convergence flag projected off the verdict, the named fit-quality value, the metric key read off the row's own `FitMetric` column, and retained reduction rank; a closed-form fit ALSO emits the blas `Factorization` receipt under the same `CorrelationId`. Fit-quality and rank read back operator-visibly through the receipt stream (a stall through `ReceiptFolds.Nonconverged`) instead of dying write-only on the carrier.
- Packages: MathNet.Numerics, TorchSharp, PeterO.Numbers (exact criterion accumulation), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, Generator.Equals, LanguageExt.Core, NodaTime, Rasm (project — kernel `Stat<Scalar>`/`MomentNormalizer`, `Tolerance`/`ToleranceLane`, `Dimension`, `Deterministic.Draw`), Rasm.Persistence (project), BCL inbox
- Growth: a new estimator family, link, kernel, criterion reading, temporal modality, or curve shape is a ROW at `Stats/families#FAMILY_ROWS` and this page is untouched. New fitted or prediction shapes extend `EstimatorModel` or `Prediction` here, and only when payload timing differs. A new selection criterion is one `InformationCriterion` row. Per-model estimator classes, detector DTOs, and universal knob records are rejected.
- Boundary: quadratic and cubic kernels refuse above their own `ScaleCeiling` instead of running unbounded — agglomerative linkage is cubic in the merge sweep, and the Gram, the DBSCAN region query, and the kNN leave-one-out score are each quadratic — because a building-scale design silently converts an admitted fit into an unkillable one.
- Boundary: `Prediction` is total across response, projection, assignment, and anomaly evidence; neither `Tensor` nor an untyped score array crosses the boundary. `Stats/signal#SIGNAL_LANE` produces the conditioned multi-channel spectral evidence detection consumes, Stats owns reusable changepoint/anomaly detection, and the digital twin consumes that detector beside its optimizer-owned surrogate. Offline deep training remains Python-owned behind `GraduationEvidence`. NO hypothesis-testing surface lives here: the `StatisticalTest`/`TestResult`/`Hypothesis` cluster was `LAW_WITHOUT_PRODUCER` — census on landing found zero consumers of any of the three across `libs/dotnet`, `libs/python`, `libs/typescript`, and `tests`, and zero seam rows naming this page in `libs/contracts/manifest.json`, while its own asserted consumer (`Solver/uncertainty#UNCERTAINTY_LANE`) named neither the surface nor the type. NAMED LOSS: six row-owned test kernels — pooled and Welch t with Welch–Satterthwaite fractional dof, one-way ANOVA, chi-square goodness of fit, a two-sample Kolmogorov–Smirnov against the Kolmogorov asymptotic series (MathNet ships no such distribution), and a tie-and-continuity-corrected Mann–Whitney rank-sum normal approximation — each with its own arity floor and tail direction.
- Boundary: `ResidualWindow` is the ONE bounded detector evidence ring. `Stats/monitor#MONITOR_LANE` and `Solver/clash#CLASH_AND_TWIN` both push a scalar stream into a fixed-capacity FIFO and both project it as a one-column `Matrix<double>` for `Detect`, under the identical capacity floor and the identical `(Count >= Capacity ? Tail : Held).Add(v)` fold. Seating it beside `EstimatorModel.Detector` is what makes injecting a monitor capsule into a twin score a single windowing rather than two nested ones — the doubly-windowed path re-pushed every row the twin had already pushed and returned the monitor's window's scores rather than the twin's.
- Boundary: the tolerance carrier is the kernel's and its LANE is the semantic. `FitBudget.Stop` rides `ToleranceLane.Convergence` and `EstimatorPolicy.Classification.KktTolerance` rides `ToleranceLane.Kkt`, so the loss-delta stop and the KKT-violation stop are two admitted bands under two named lanes where two bare `double` fields let either one be handed the other's value. The two canonical budgets mint through the type-init refusal idiom — a constant that violates its own lane is a boot break, never a runtime rail.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

public sealed record ScaleCeiling(int Rows, string Gate) {
    public static readonly ScaleCeiling Cubic = new(Rows: 2_048, Gate: "cubic");
    public static readonly ScaleCeiling Quadratic = new(Rows: 32_768, Gate: "quadratic");

    internal Fin<Unit> Admit(int rows) =>
        rows <= Rows ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Capacity(CapacityRequirement.WithinLimit, new CapacityEvidence.Count(rows, Rows))));
}

public sealed record KernelParams(double Bandwidth, double Degree, double Offset) {
    public static readonly KernelParams Canonical = new(Bandwidth: 1.0, Degree: 3.0, Offset: 1.0);

    internal Fin<Unit> Admit() =>
        Bandwidth > 0.0 && Degree >= 1.0 && double.IsFinite(Bandwidth) && double.IsFinite(Degree) && double.IsFinite(Offset)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InformationCriterion {
    public static readonly InformationCriterion Akaike = new("aic", static (k, _) => 2.0 * k);
    public static readonly InformationCriterion Bayes = new("bic", static (k, n) => k * Math.Log(n));

    [UseDelegateFromConstructor] private partial double Penalty(long parameters, int samples);

    internal double Score(double logLikelihood, long parameters, int samples) =>
        Penalty(parameters, samples) - 2.0 * logLikelihood;
}

[ValueObject<int>]
public readonly partial struct WindowCapacity {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 8 ? null : new ValidationError($"<window-capacity:{value}>");

    public static Fin<WindowCapacity> From(int repr) => Validate(repr, null, out WindowCapacity value) is { } fault ? Fin.Fail<WindowCapacity>(fault) : value;
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record ResidualWindow(Seq<double> Values, WindowCapacity Capacity) {
    public static ResidualWindow Of(WindowCapacity capacity) => new(Seq<double>(), capacity);

    public int Count => Values.Count;

    public ResidualWindow Push(double value) =>
        this with { Values = (Values.Count >= Capacity.Value ? Values.Tail : Values).Add(value) };

    public Matrix<double> Evidence => Matrix<double>.Build.Dense(Values.Count, 1, (row, _) => Values[row]);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClusterShape {
    private ClusterShape() { }

    public sealed record Partitioned(int Groups) : ClusterShape;
    public sealed record Density : ClusterShape;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Estimator {
    private Estimator() { }

    public sealed record Regression(EstimatorKind Kind, GlmFamily Family, Option<LinkFunction> Link) : Estimator;
    public sealed record Curve(CurveSpec Spec) : Estimator;
    public sealed record Reduction(EstimatorKind Kind, int Rank) : Estimator;
    public sealed record Cluster(EstimatorKind Kind, ClusterShape Shape) : Estimator;
    public sealed record Classify(EstimatorKind Kind) : Estimator;
    public sealed record Temporal(TemporalSpec Spec) : Estimator;

    public EstimatorFamily Family => Switch(
        regression: static _ => EstimatorFamily.Regression, curve: static _ => EstimatorFamily.Regression,
        reduction: static _ => EstimatorFamily.Reduction,
        cluster: static _ => EstimatorFamily.Cluster, classify: static _ => EstimatorFamily.Classify,
        temporal: static _ => EstimatorFamily.Temporal);

    public string Key => Switch(
        regression: static r => $"{r.Kind.Key}:{r.Family.Key}", curve: static c => c.Spec.Form.Key, reduction: static d => d.Kind.Key,
        cluster: static c => c.Kind.Key, classify: static c => c.Kind.Key, temporal: static t => t.Spec.Model.Key);

    public FitMetric Metric => Switch(
        regression: static r => r.Kind.Metric, curve: static c => c.Spec.Form.Metric, reduction: static d => d.Kind.Metric,
        cluster: static c => c.Kind.Metric, classify: static c => c.Kind.Metric, temporal: static t => t.Spec.Model.Metric);
}

[Equatable]
public sealed partial record SupportMachine(
    int Label, [property: OrderedEquality] ImmutableArray<int> Support, Vector<double> Duals, double Bias);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimatorModel {
    private EstimatorModel() { }

    public sealed record Linear(Vector<double> Coefficients, double Intercept, LinkFunction Link, GlmFamily Family, double ScaledDeviance) : EstimatorModel;
    public sealed record Curve(CurveSpec Spec, Vector<double> Coefficients) : EstimatorModel;
    public sealed record Basis(Matrix<double> Components, Vector<double> Singular, Vector<double> Mean, double EnergyFraction) : EstimatorModel;
    public sealed record KernelBasis(Matrix<double> Training, Matrix<double> Alphas, Vector<double> Eigen, KernelRow Kernel, KernelParams Parameters, Vector<double> RowMean, double GrandMean) : EstimatorModel;
    public sealed record Factors(Matrix<double> Encoder, Matrix<double> Components) : EstimatorModel;
    [Equatable] public sealed partial record Partition(Matrix<double> Centroids, [property: OrderedEquality] ImmutableArray<int> Labels) : EstimatorModel;
    [Equatable] public sealed partial record Density(Matrix<double> Training, [property: OrderedEquality] ImmutableArray<Option<int>> Labels, [property: OrderedEquality] ImmutableArray<bool> Core, double Radius) : EstimatorModel;
    public sealed record Mixture(Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights) : EstimatorModel;
    public sealed record Margin(Matrix<double> Training, Seq<SupportMachine> Machines, KernelRow Kernel, KernelParams Parameters) : EstimatorModel;
    [Equatable] public sealed partial record Neighbors(Matrix<double> Design, [property: OrderedEquality] ImmutableArray<int> Labels, int K) : EstimatorModel;
    public sealed record Bayes(Matrix<double> Means, Matrix<double> Variances, Vector<double> Priors) : EstimatorModel;
    public sealed record Lag(Vector<double> ArCoefficients, Vector<double> MaCoefficients, double Variance, Vector<double> Tail, TimeSeriesModel Model) : EstimatorModel;
    public sealed record Detector(Vector<double> Mean, Cholesky<double> Scale, DetectorSpec Spec, int MaxRunLength) : EstimatorModel;

    public long ParameterCount => Switch(
        linear: static c => (long)c.Coefficients.Count,
        curve: static c => (long)c.Coefficients.Count,
        basis: static b => (long)b.Components.RowCount,
        kernelBasis: static k => (long)k.Eigen.Count,
        factors: static f => (long)f.Components.RowCount * f.Components.ColumnCount,
        partition: static p => (long)p.Centroids.RowCount,
        density: static d => (long)d.Core.Count(static core => core),
        mixture: static m => (long)m.Weights.Count,
        margin: static m => m.Machines.Fold(0L, static (acc, machine) => acc + machine.Support.Length),
        neighbors: static n => (long)n.Design.RowCount,
        bayes: static b => (long)b.Priors.Count,
        lag: static l => (long)(l.ArCoefficients.Count + l.MaCoefficients.Count),
        detector: static d => (long)d.Mean.Count + (long)d.Mean.Count * (d.Mean.Count + 1) / 2);

    public Option<int> RetainedRank => Switch(
        basis: static b => Optional(b.Singular.Count),
        kernelBasis: static k => Optional(k.Eigen.Count),
        factors: static f => Optional(f.Encoder.ColumnCount),
        linear: static _ => Option<int>.None, curve: static _ => Option<int>.None, partition: static _ => Option<int>.None,
        density: static _ => Option<int>.None, mixture: static _ => Option<int>.None, margin: static _ => Option<int>.None,
        neighbors: static _ => Option<int>.None, bayes: static _ => Option<int>.None, lag: static _ => Option<int>.None,
        detector: static _ => Option<int>.None);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Prediction {
    private Prediction() { }

    public sealed record Response(Vector<double> Values) : Prediction;
    public sealed record Projection(Matrix<double> Scores) : Prediction;
    [Equatable] public sealed partial record Assignment([property: OrderedEquality] ImmutableArray<Option<int>> Labels) : Prediction;
    [Equatable] public sealed partial record Anomaly(Vector<double> Scores, [property: OrderedEquality] ImmutableArray<bool> Changes) : Prediction;
}

[Union<Matrix<double>, int>(T1Name = "Evidence", T2Name = "Horizon")]
public readonly partial struct PredictQuery;

public sealed record FitBudget(Dimension MaxIterations, Tolerance Stop) {
    public static readonly FitBudget Canonical = Of(iterations: 1_000, stop: 1e-8);
    public static readonly FitBudget Grouping = Of(iterations: 300, stop: 1e-8);
    public static readonly FitBudget Dual = Of(iterations: 1_000_000, stop: 1e-3);

    public static Fin<FitBudget> Admit(int iterations, double stop) =>
        (Fin<Dimension>.Succ(Dimension.Create(value: Math.Max(1, iterations))).ToValidation(),
         Tolerance.Of(ToleranceLane.Convergence, stop, Op.Of(name: nameof(FitBudget))).ToValidation())
            .Apply(static (cap, band) => new FitBudget(cap, band))
            .ToFin();

    private static FitBudget Of(int iterations, double stop) =>
        new(Dimension.Create(iterations), new Tolerance(ToleranceLane.Convergence, stop));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimatorPolicy {
    private EstimatorPolicy() { }

    public sealed record Regression(double Regularization, double LearningRate, double Weight, FitBudget Budget) : EstimatorPolicy;
    public sealed record Reduction(double EnergyFraction, KernelRow Kernel, KernelParams Parameters, FitBudget Budget) : EstimatorPolicy;
    public sealed record Grouping(double Radius, int Neighbors, double Ridge, FitBudget Budget) : EstimatorPolicy;
    public sealed record Classification(
        double Regularization, KernelRow Kernel, KernelParams Parameters, int Neighbors,
        double Box, Tolerance KktTolerance, FitBudget Budget, bool Shrinking) : EstimatorPolicy;
    public sealed record Temporal(FitBudget Budget) : EstimatorPolicy;

    public static readonly EstimatorPolicy CanonicalRegression = new Regression(Regularization: 1e-3, LearningRate: 0.05, Weight: 1.0, FitBudget.Canonical);
    public static readonly EstimatorPolicy CanonicalReduction = new Reduction(EnergyFraction: 0.95, KernelRow.Rbf, KernelParams.Canonical, FitBudget.Canonical);
    public static readonly EstimatorPolicy CanonicalGrouping = new Grouping(Radius: 0.5, Neighbors: 4, Ridge: 1e-3, FitBudget.Grouping);
    public static readonly EstimatorPolicy CanonicalClassification = new Classification(
        Regularization: 1e-3, KernelRow.Rbf, KernelParams.Canonical, Neighbors: 5,
        Box: 1.0, Kkt(1e-3), FitBudget.Dual, Shrinking: true);
    public static readonly EstimatorPolicy CanonicalTemporal = new Temporal(FitBudget.Canonical);

    public EstimatorFamily Family => Switch(
        regression: static _ => EstimatorFamily.Regression, reduction: static _ => EstimatorFamily.Reduction,
        grouping: static _ => EstimatorFamily.Cluster, classification: static _ => EstimatorFamily.Classify,
        temporal: static _ => EstimatorFamily.Temporal);

    internal Fin<Unit> Admit() => Switch(
        regression: static p => (
            Range(p.Regularization >= 0.0, "regularization", p.Regularization).ToValidation(),
            Range(p.LearningRate > 0.0, "learning-rate", p.LearningRate).ToValidation(),
            Range(p.Weight > 0.0, "weight", p.Weight).ToValidation())
            .Apply(static (_, _, _) => unit).ToFin(),
        reduction: static p => (
            Range(p.EnergyFraction is > 0.0 and <= 1.0, "energy-fraction", p.EnergyFraction).ToValidation(),
            p.Parameters.Admit().ToValidation())
            .Apply(static (_, _) => unit).ToFin(),
        grouping: static p => (
            Range(p.Radius > 0.0, "radius", p.Radius).ToValidation(),
            Range(p.Ridge > 0.0, "ridge", p.Ridge).ToValidation(),
            Range(p.Neighbors >= 1, "neighbors", p.Neighbors).ToValidation())
            .Apply(static (_, _, _) => unit).ToFin(),
        classification: static p => (
            Range(p.Regularization > 0.0, "regularization", p.Regularization).ToValidation(),
            Range(p.Box > 0.0, "box", p.Box).ToValidation(),
            Range(p.Neighbors >= 1, "neighbors", p.Neighbors).ToValidation(),
            p.Parameters.Admit().ToValidation())
            .Apply(static (_, _, _, _) => unit).ToFin(),
        temporal: static _ => Fin.Succ(unit));

    private static Fin<Unit> Range(bool holds, string gate, double value) =>
        holds && double.IsFinite(value) ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));

    private static Tolerance Kkt(double value) => new(ToleranceLane.Kkt, value);
}

public sealed record Design {
    private Design(Matrix<double> features, Option<Vector<double>> targets) {
        Features = features;
        Targets = targets;
    }

    public Matrix<double> Features { get; }
    public Option<Vector<double>> Targets { get; }
    public int Rows => Features.RowCount;
    public int Columns => Features.ColumnCount;

    public static Fin<Design> Admit(Matrix<double> features, Option<Vector<double>> targets) =>
        (Shape(features).ToValidation(),
         Finite(features.AsColumnMajorArray() ?? features.ToColumnMajorArray(), "features").ToValidation(),
         Aligned(features, targets).ToValidation(),
         targets.Match(Some: y => Finite(y.AsArray() ?? y.ToArray(), "targets"), None: static () => Fin.Succ(unit)).ToValidation())
            .Apply((_, _, _, _) => new Design(features, targets))
            .ToFin();

    private static Fin<Unit> Shape(Matrix<double> features) =>
        features.RowCount >= 1 && features.ColumnCount >= 1
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input)));

    private static Fin<Unit> Finite(double[] values, string gate) =>
        TensorPrimitives.IsFiniteAll<double>(values) ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(values.Length))));

    private static Fin<Unit> Aligned(Matrix<double> features, Option<Vector<double>> targets) =>
        targets.Filter(y => y.Count != features.RowCount).Match(
            Some: y => Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(y.Count, features.RowCount)))),
            None: static () => Fin.Succ(unit));

    public (Design Train, Design Test) Split(int fold, int folds) {
        int lo = Rows * fold / folds, hi = Rows * (fold + 1) / folds;
        int[] test = [.. Enumerable.Range(lo, hi - lo)];
        int[] train = [.. Enumerable.Range(0, Rows).Where(i => i < lo || i >= hi)];
        return (Sliced(train), Sliced(test));
    }

    private Design Sliced(int[] rows) => new(
        Matrix<double>.Build.DenseOfRowVectors([.. rows.Select(Features.Row)]),
        Targets.Map(y => Vector<double>.Build.DenseOfArray([.. rows.Select(i => y[i])])));
}

public readonly record struct FitAmbients(IClock Clock, DenseSubstrate Substrate, Deterministic.Draw Draw);

internal sealed record FitContext(
    Estimator Estimator, Design Design, FitAmbients Ambients, EstimatorPolicy Policy, LinkFunction Link, GlmFamily Family,
    OptimDriver Driver, int Rank, Option<ClusterShape> Cluster, Option<TemporalSpec> Temporal, Option<CurveSpec> Curve) {
    internal IClock Clock => Ambients.Clock;
    internal DenseSubstrate Substrate => Ambients.Substrate;
    internal Deterministic.Draw Draw => Ambients.Draw;
    internal int Rows => Design.Rows;

    internal Fin<TCase> Case<TCase>() where TCase : EstimatorPolicy =>
        Policy is TCase typed
            ? Fin.Succ(typed)
            : Fin.Fail<TCase>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Keys(Policy.Family.Key, Estimator.Key))));

    internal Fin<Vector<double>> Supervised() =>
        Design.Targets.ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(Estimator.Key))));

    internal Fin<int> Groups() =>
        Cluster.ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input))).Bind(shape => shape.Switch(
            state: Design.Rows,
            partitioned: static (rows, group) => group.Groups >= 1 && group.Groups <= rows
                ? Fin.Succ(group.Groups)
                : Fin.Fail<int>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(group.Groups, rows)))),
            density: static (_, _) => Fin.Fail<int>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Group)))));
}

public sealed record FittedModel(
    Estimator Estimator, EstimatorModel Carrier, double Quality, double Residual,
    Option<double> LogLikelihood, int Steps, Convergence Verdict, Instant At) {
    public bool Converged => Verdict is Convergence.Converged;

    public Fin<Prediction> Predict(PredictQuery query) => EstimatorFold.Predict(this, query);
}

public sealed record ValidationReport(
    Estimator Estimator, Seq<double> FoldQuality, double Mean, double Spread,
    MomentNormalizer Normalizer, int Folds, Instant At);

public sealed record CriterionPolicy(InformationCriterion Criterion, int Folds) {
    public static readonly CriterionPolicy Canonical = new(InformationCriterion.Akaike, Folds: 5);

    internal Fin<Unit> Admit(int candidates) =>
        candidates < 2 || Folds < 2
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(candidates, Folds))))
            : Fin.Succ(unit);
}

public sealed record CandidateEvidence(
    EstimatorPolicy Policy, Option<double> Criterion, double ValidationMean, double ValidationSpread, int Rank);

public sealed record SelectionReport(Seq<CandidateEvidence> Candidates, EstimatorPolicy Chosen, InformationCriterion Criterion, Instant At);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class EstimatorFold {
    internal const double VarianceFloor = 1e-9;

    public static Fin<FittedModel> Fit(Estimator estimator, Design design, EstimatorPolicy policy, FitAmbients ambients) =>
        Admitted(estimator, design, policy, ambients).Bind(static ctx => ctx.Estimator.Switch(
            state: ctx,
            regression: static (c, r) => r.Kind.Fit(c),
            curve: static (c, k) => EstimatorKernels.CurveFit(c, k.Spec),
            reduction: static (c, d) => d.Kind.Fit(c),
            cluster: static (c, g) => g.Kind.Fit(c),
            classify: static (c, k) => k.Kind.Fit(c),
            temporal: static (c, t) => t.Spec.Model.Fit(c)));

    public static Fin<Prediction> Predict(FittedModel model, PredictQuery query) =>
        model.Carrier.Switch<PredictQuery, Fin<Prediction>>(
            state: query,
            linear: static (q, c) => Evidence(q).Bind(x => {
                Vector<double> eta = x.Multiply(c.Coefficients).Add(c.Intercept);
                return c.Link.Domain(eta).Map(_ => (Prediction)new Prediction.Response(eta.Map(c.Link.Mean)));
            }),
            curve: static (q, c) => Evidence(q).Bind(x => x.ColumnCount == 1
                ? Fin.Succ<Prediction>(new Prediction.Response(x.Column(0).Map(value => c.Spec.Form.Evaluate(c.Spec, c.Coefficients, value))))
                : Fin.Fail<Prediction>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(x.ColumnCount, 1L))))),
            basis: static (q, b) => Evidence(q).Map(x => (Prediction)new Prediction.Projection(Center(x, b.Mean).Multiply(b.Components.Transpose()))),
            kernelBasis: static (q, k) => Evidence(q).Map(x => (Prediction)new Prediction.Projection(KernelProject(x, k))),
            factors: static (q, f) => Evidence(q).Map(x => (Prediction)new Prediction.Projection(NonNegativeEncode(x, f.Components))),
            partition: static (q, p) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => Optional(Nearest(row, p.Centroids)))])),
            density: static (q, d) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => DensityAssign(row, d))])),
            mixture: static (q, m) => Evidence(q).Bind(x => Responsibilities(x, m).Map(static labels => (Prediction)new Prediction.Assignment(labels))),
            margin: static (q, m) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => Optional(Strongest(row, m)))])),
            neighbors: static (q, nbr) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => Optional(Vote(row, nbr)))])),
            bayes: static (q, b) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => Optional(Posterior(row, b)))])),
            lag: static (q, l) => Horizon(q).Bind(h => l.Model.Forecast(l, h)).Map(static values => (Prediction)new Prediction.Response(values)),
            detector: static (q, d) => Evidence(q).Bind(x => Detect(d, x)));

    private static Fin<Matrix<double>> Evidence(PredictQuery query) =>
        query.IsEvidence
            ? Fin.Succ(query.AsEvidence)
            : Fin.Fail<Matrix<double>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));

    private static Fin<int> Horizon(PredictQuery query) =>
        query.IsHorizon && query.AsHorizon >= 1
            ? Fin.Succ(query.AsHorizon)
            : Fin.Fail<int>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));

    public static Fin<ValidationReport> Validate(
        Estimator estimator, Design design, EstimatorPolicy policy, CriterionPolicy selection, FitAmbients ambients) =>
        selection.Folds < 2 || selection.Folds > design.Rows
            ? Fin.Fail<ValidationReport>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(selection.Folds, design.Rows))))
            : (estimator is Estimator.Temporal { Spec.Forecasts: true }
                ? ForwardChain(estimator, design, policy, selection.Folds, ambients)
                : estimator.Family.Supervised
                    ? HeldOut(estimator, design, policy, selection.Folds, ambients)
                    : Fin.Fail<Seq<double>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(estimator.Key)))))
            .Bind(quality => Stat<Scalar>.Of(quality.Map(static q => (Scalar)q), Op.Of(name: nameof(Validate)))
                .Map(stat => new ValidationReport(
                    estimator, quality, stat.Mean, stat.Deviation(MomentNormalizer.Population),
                    MomentNormalizer.Population, selection.Folds, ambients.Clock.GetCurrentInstant())));

    public static Fin<SelectionReport> Select(
        Estimator estimator, Design design, Seq<EstimatorPolicy> candidates, CriterionPolicy selection, FitAmbients ambients) =>
        selection.Admit(candidates.Count).Bind(_ => candidates
            .TraverseM(policy => Scored(estimator, design, policy, selection, ambients)).As()
            .Map(rows => Ranked(rows, selection, ambients.Clock.GetCurrentInstant())));

    private static Fin<(EstimatorPolicy Policy, Option<double> Criterion, double Mean, double Spread)> Scored(
        Estimator estimator, Design design, EstimatorPolicy policy, CriterionPolicy selection, FitAmbients ambients) =>
        Fit(estimator, design, policy, ambients)
            .Bind(model => Validate(estimator, design, policy, selection, ambients)
                .Map(report => (
                    policy,
                    model.LogLikelihood.Map(ll => selection.Criterion.Score(ll, model.Carrier.ParameterCount, design.Rows)),
                    report.Mean,
                    report.Spread)));

    internal static double ExactSum(Seq<double> terms) =>
        terms.Fold(EFloat.Zero, static (sum, term) => sum.Add(EFloat.FromDouble(term)))
            .RoundToPrecision(EContext.Binary64)
            .ToDouble();

    private static SelectionReport Ranked(
        Seq<(EstimatorPolicy Policy, Option<double> Criterion, double Mean, double Spread)> rows, CriterionPolicy selection, Instant at) {
        Seq<CandidateEvidence> ordered = toSeq(rows
            .OrderBy(row => row.Criterion.IfNone(() => -row.Mean))
            .Select(static (row, index) => new CandidateEvidence(row.Policy, row.Criterion, row.Mean, row.Spread, index)));
        return new SelectionReport(ordered, ordered[0].Policy, selection.Criterion, at);
    }

    public static ComputeReceipt Receipt(FittedModel model, CorrelationId correlation, Duration elapsed) =>
        new ComputeReceipt.Fit(
            model.Estimator.Family.Key, model.Estimator.Key, model.Carrier.ParameterCount, model.Steps,
            model.Residual, model.Converged, model.Quality, model.Estimator.Metric.Key,
            model.Carrier.RetainedRank.IfNone(0)) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    // --- [ADMISSION] -------------------------------------------------------------------

    private static Fin<FitContext> Admitted(Estimator estimator, Design design, EstimatorPolicy policy, FitAmbients ambients) {
        (Option<EstimatorKind> kind, LinkFunction link, GlmFamily family, int rank, Option<ClusterShape> cluster, Option<TemporalSpec> temporal, Option<CurveSpec> curve) = estimator.Switch(
            regression: static r => (Optional(r.Kind), r.Link.IfNone(r.Family.Canonical), r.Family, 0, Option<ClusterShape>.None, Option<TemporalSpec>.None, Option<CurveSpec>.None),
            curve: static c => (Option<EstimatorKind>.None, LinkFunction.Identity, GlmFamily.Gaussian, 0, Option<ClusterShape>.None, Option<TemporalSpec>.None, Optional(c.Spec)),
            reduction: static d => (Optional(d.Kind), LinkFunction.Identity, GlmFamily.Gaussian, d.Rank, Option<ClusterShape>.None, Option<TemporalSpec>.None, Option<CurveSpec>.None),
            cluster: static c => (Optional(c.Kind), LinkFunction.Identity, GlmFamily.Gaussian, 0, Optional(c.Shape), Option<TemporalSpec>.None, Option<CurveSpec>.None),
            classify: static c => (Optional(c.Kind), LinkFunction.Identity, GlmFamily.Gaussian, 0, Option<ClusterShape>.None, Option<TemporalSpec>.None, Option<CurveSpec>.None),
            temporal: static t => (Option<EstimatorKind>.None, LinkFunction.Identity, GlmFamily.Gaussian, 0, Option<ClusterShape>.None, Optional(t.Spec), Option<CurveSpec>.None));
        OptimDriver driver = kind.Map(static row => row.Driver).IfNone(OptimDriver.Adam);
        Fin<Unit> correspondence = kind.Filter(row => row.Family != estimator.Family).IsSome
                ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Keys(estimator.Key, estimator.Family.Key))))
            : policy.Family != estimator.Family
                ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Keys(policy.Family.Key, estimator.Family.Key))))
            : estimator.Family.Supervised && kind.IsSome && design.Targets.IsNone
                ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(estimator.Key))))
            : rank < 0 || rank > Math.Min(design.Rows, design.Columns)
                ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Counts(rank, design.Rows, design.Columns))))
            : Fin.Succ(unit);
        return correspondence
            .Bind(_ => policy.Admit())
            .Bind(_ => temporal.Map(spec => spec.Admit(design.Rows, design.Columns)).IfNone(Fin.Succ(unit)))
            .Bind(_ => curve.Map(spec => spec.Admit(design)).IfNone(Fin.Succ(unit)))
            .Map(_ => new FitContext(estimator, design, ambients, policy, link, family, driver, rank, cluster, temporal, curve))
            .Bind(ctx => kind.Map(row => row.Admit(ctx))
                .IfNone(() => curve.IsSome ? RealResponse(ctx) : TemporalDesign(ctx))
                .Map(_ => ctx));
    }

    internal static Fin<Unit> RealResponse(FitContext context) => context.Supervised().Map(static _ => unit);

    internal static Fin<Unit> RegularizedResponse(FitContext context) =>
        RealResponse(context).Bind(_ => context.Case<EstimatorPolicy.Regression>()).Map(static _ => unit);

    internal static Fin<Unit> IterativeResponse(FitContext context) => RegularizedResponse(context);

    internal static Fin<Unit> GlmResponse(FitContext context) =>
        IterativeResponse(context).Bind(_ => context.Family.Admit(context.Design));

    internal static Fin<Unit> ReductionDesign(FitContext context) =>
        context.Case<EstimatorPolicy.Reduction>().Map(static _ => unit);

    internal static Fin<Unit> KernelReductionDesign(FitContext context) =>
        ReductionDesign(context).Bind(_ => ScaleCeiling.Quadratic.Admit(context.Rows));

    internal static Fin<Unit> NonNegativeReductionDesign(FitContext context) =>
        TensorPrimitives.IsNegativeAny<double>(context.Design.Features.AsColumnMajorArray() ?? context.Design.Features.ToColumnMajorArray())
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
            : ReductionDesign(context);

    internal static Fin<Unit> GroupingDesign(FitContext context) =>
        context.Groups().Bind(_ => context.Case<EstimatorPolicy.Grouping>()).Map(static _ => unit);

    internal static Fin<Unit> MixtureDesign(FitContext context) => GroupingDesign(context);

    internal static Fin<Unit> LinkageDesign(FitContext context) =>
        context.Groups().Bind(_ => ScaleCeiling.Cubic.Admit(context.Rows));

    internal static Fin<Unit> DensityDesign(FitContext context) =>
        ScaleCeiling.Quadratic.Admit(context.Rows)
            .Bind(_ => context.Cluster.ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input))))
            .Bind(shape => shape.Switch(
                partitioned: static _ => Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.None()))),
                density: _ => context.Case<EstimatorPolicy.Grouping>().Bind(p =>
                    p.Neighbors < context.Rows ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))))));

    internal static Fin<Unit> ClassLabels(FitContext context) =>
        context.Supervised().Bind(static y =>
            TensorPrimitives.IsIntegerAll<double>(y.AsArray() ?? y.ToArray()) && y.Distinct().Take(2).Count() == 2
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))));

    internal static Fin<Unit> NeighborhoodDesign(FitContext context) =>
        ClassLabels(context)
            .Bind(_ => ScaleCeiling.Quadratic.Admit(context.Rows))
            .Bind(_ => context.Case<EstimatorPolicy.Classification>())
            .Bind(p => p.Neighbors < context.Rows ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))));

    internal static Fin<Unit> MarginDesign(FitContext context) =>
        ClassLabels(context)
            .Bind(_ => ScaleCeiling.Quadratic.Admit(context.Rows))
            .Bind(_ => context.Case<EstimatorPolicy.Classification>())
            .Map(static _ => unit);

    private static Fin<Unit> TemporalDesign(FitContext context) =>
        context.Temporal.ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input)))
            .Bind(spec => spec.Admit(context.Design.Rows, context.Design.Columns))
            .Bind(_ => context.Case<EstimatorPolicy.Temporal>())
            .Bind(p => context.Temporal.Bind(static spec => spec.Detector).Map(static spec => spec is DetectorSpec.BayesianOnline).IfNone(false)
                && p.Budget.MaxIterations.Value < 2
                ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(p.Budget.MaxIterations.Value, 2L))))
                : Fin.Succ(unit));

    // --- [VALIDATION] ------------------------------------------------------------------

    private static Fin<Seq<double>> HeldOut(Estimator estimator, Design design, EstimatorPolicy policy, int folds, FitAmbients ambients) =>
        toSeq(Enumerable.Range(0, folds)).TraverseM(fold => {
            (Design train, Design test) = design.Split(fold, folds);
            return Fit(estimator, train, policy, ambients)
                .Bind(model => Predict(model, test.Features))
                .Bind(prediction => Scored(prediction, test.Targets, estimator));
        }).As();

    private static Fin<double> Scored(Prediction prediction, Option<Vector<double>> held, Estimator estimator) =>
        held.ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(estimator.Key)))).Bind(actual => prediction.Switch(
            response: r => Fin.Succ(GoodnessOfFit.CoefficientOfDetermination(r.Values, actual)),
            projection: _ => Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(estimator.Key)))),
            assignment: a => Fin.Succ(Enumerable.Range(0, actual.Count).Count(i => a.Labels[i] == Optional((int)actual[i])) / (double)actual.Count),
            anomaly: _ => Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(estimator.Key))))));

    private static Fin<Seq<double>> ForwardChain(Estimator estimator, Design design, EstimatorPolicy policy, int folds, FitAmbients ambients) {
        Vector<double> series = design.Features.Column(0);
        int n = series.Count;
        return toSeq(Enumerable.Range(1, folds)).TraverseM(fold => {
            int cut = n * fold / (folds + 1), horizon = Math.Max(1, n * (fold + 1) / (folds + 1) - cut);
            return Design.Admit(Matrix<double>.Build.Dense(cut, 1, (i, _) => series[i]), None)
                .Bind(prefix => Fit(estimator, prefix, policy, ambients))
                .Bind(model => Predict(model, horizon))
                .Bind(prediction => prediction.Switch(
                    response: r => Fin.Succ(-Math.Sqrt(Enumerable.Range(0, Math.Min(r.Values.Count, n - cut)).Sum(h => Math.Pow(r.Values[h] - series[cut + h], 2)) / Math.Max(1, Math.Min(r.Values.Count, n - cut)))),
                    projection: static _ => Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.None()))),
                    assignment: static _ => Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.None()))),
                    anomaly: static _ => Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.None())))));
        }).As();
    }

    // --- [KERNELS]

    internal static Matrix<double> Intercept(Matrix<double> x) =>
        Matrix<double>.Build.Dense(x.RowCount, x.ColumnCount + 1, (i, j) => j == 0 ? 1.0 : x[i, j - 1]);

    internal static (double Intercept, Vector<double> Slopes) Split(Vector<double> theta) =>
        (theta[0], theta.SubVector(1, theta.Count - 1));

    internal static Matrix<double> Center(Matrix<double> x, Vector<double> mean) =>
        Matrix<double>.Build.Dense(x.RowCount, x.ColumnCount, (i, j) => x[i, j] - mean[j]);

    internal static double Distance(Vector<double> left, Vector<double> right) =>
        TensorPrimitives.Distance<double>(left.ToArray(), right.ToArray());

    internal static int Nearest(Vector<double> point, Matrix<double> centroids) =>
        Enumerable.Range(0, centroids.RowCount).MinBy(c => Distance(point, centroids.Row(c)));

    internal static Fin<Cholesky<double>[]> Choleskies(Seq<Matrix<double>> covariances, double ridge) =>
        covariances.TraverseM(covariance =>
            Admission.Definite(covariance + Matrix<double>.Build.DiagonalIdentity(covariance.RowCount) * ridge)).As()
        .Map(static factors => factors.ToArray());

    internal static double LogGaussian(Vector<double> x, Vector<double> mean, Cholesky<double> chol, int dim) =>
        -0.5 * (dim * Math.Log(2.0 * Math.PI) + chol.DeterminantLn + Mahalanobis(x, mean, chol));

    internal static double Mahalanobis(Vector<double> value, Vector<double> mean, Cholesky<double> scale) {
        Vector<double> delta = value - mean;
        return delta.DotProduct(scale.Solve(delta));
    }

    // --- [PREDICTION]

    private static Fin<ImmutableArray<Option<int>>> Responsibilities(Matrix<double> x, EstimatorModel.Mixture mixture) =>
        Choleskies(mixture.Covariances, VarianceFloor).Map(chols =>
            [.. x.EnumerateRows().Select(row => Optional(
                Enumerable.Range(0, mixture.Weights.Count)
                    .MaxBy(j => Math.Log(Math.Max(1e-300, mixture.Weights[j])) + LogGaussian(row, mixture.Means.Row(j), chols[j], x.ColumnCount))))]);

    private static double Decision(Vector<double> point, EstimatorModel.Margin margin, SupportMachine machine) =>
        machine.Support.Sum(i => machine.Duals[i] * margin.Kernel.At(point, margin.Training.Row(i), margin.Parameters)) + machine.Bias;

    internal static int Strongest(Vector<double> point, EstimatorModel.Margin margin) =>
        margin.Machines.Map(machine => (machine.Label, Score: Decision(point, margin, machine)))
            .MaxBy(static row => row.Score).Label;

    internal static int Vote(Vector<double> point, EstimatorModel.Neighbors neighbors, int exclude = -1) =>
        Enumerable.Range(0, neighbors.Design.RowCount)
            .Where(i => i != exclude)
            .Select(i => (Label: neighbors.Labels[i], Distance: Distance(point, neighbors.Design.Row(i))))
            .OrderBy(static row => row.Distance)
            .Take(neighbors.K)
            .GroupBy(static row => row.Label)
            .OrderByDescending(g => g.Sum(static row => 1.0 / Math.Max(1e-12, row.Distance)))
            .First().Key;

    internal static int Posterior(Vector<double> point, EstimatorModel.Bayes bayes) =>
        Enumerable.Range(0, bayes.Priors.Count)
            .MaxBy(k => Math.Log(Math.Max(1e-300, bayes.Priors[k])) +
                Enumerable.Range(0, point.Count).Sum(f => -0.5 * (Math.Log(2.0 * Math.PI * bayes.Variances[k, f]) + (point[f] - bayes.Means[k, f]) * (point[f] - bayes.Means[k, f]) / bayes.Variances[k, f])));

    private static Option<int> DensityAssign(Vector<double> point, EstimatorModel.Density density) =>
        Enumerable.Range(0, density.Training.RowCount)
            .Where(i => density.Core[i] && density.Labels[i].IsSome)
            .Select(i => (Label: density.Labels[i], Distance: Distance(point, density.Training.Row(i))))
            .Where(row => row.Distance <= density.Radius)
            .OrderBy(static row => row.Distance)
            .Select(static row => row.Label)
            .FirstOrDefault(Option<int>.None);

    private static Matrix<double> KernelProject(Matrix<double> x, EstimatorModel.KernelBasis basis) {
        Matrix<double> gram = basis.Kernel.Gram(x, basis.Training, basis.Parameters);
        Vector<double> testRowMean = gram.RowSums() / basis.Training.RowCount;
        Matrix<double> centered = Matrix<double>.Build.Dense(x.RowCount, basis.Training.RowCount,
            (i, j) => gram[i, j] - testRowMean[i] - basis.RowMean[j] + basis.GrandMean);
        return centered.Multiply(basis.Alphas);
    }

    private static Matrix<double> NonNegativeEncode(Matrix<double> x, Matrix<double> components) =>
        Matrix<double>.Build.Dense(x.RowCount, components.RowCount, (i, k) =>
            Math.Max(0.0, x.Row(i).DotProduct(components.Row(k)) / Math.Max(1e-12, components.Row(k).DotProduct(components.Row(k)))));

    private static Fin<Prediction> Detect(EstimatorModel.Detector detector, Matrix<double> evidence) =>
        evidence.RowCount < 1 || evidence.ColumnCount != detector.Mean.Count
            ? Fin.Fail<Prediction>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Counts(evidence.RowCount, evidence.ColumnCount, detector.Mean.Count))))
        : !TensorPrimitives.IsFiniteAll<double>(evidence.AsColumnMajorArray() ?? evidence.ToColumnMajorArray())
            ? Fin.Fail<Prediction>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence((long)evidence.RowCount * evidence.ColumnCount))))
            : Fin.Succ<Prediction>(detector.Spec.Switch<Matrix<double>, Prediction.Anomaly>(
                state: evidence,
                cusum: (x, spec) => EstimatorKernels.Cusum(detector, x, spec),
                bayesianOnline: (x, spec) => EstimatorKernels.BayesianOnline(detector, x, spec),
                correlatedResidual: (x, spec) => EstimatorKernels.CorrelatedResidual(detector, x, spec)));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
