# [COMPUTE_ESTIMATOR]

Rasm.Compute statistical-learning lane: one `Estimator` `[Union]` carrying a uniform `Fit(Estimator, Design, EstimatorPolicy, IClock) → FittedModel` / `Predict(FittedModel, X) → Prediction` contract across regression, reduction, clustering, classification, forecasting, changepoint, and anomaly families, keyed to one `EstimatorModel` fit-result carrier. Contract stays uniform while each row owns its mechanism: closed-form factorization covers OLS/ridge QR, PCA SVD, LS-SVM Gram, and AR lag-QR; `torch.autograd` plus `torch.optim` minimize lasso L1 and canonical-link GLM deviance; specialized folds own k-means, GMM, NMF, DBSCAN, linkage, CUSUM, Bayesian-online run-length inference, and correlated-residual scoring. `Design.Admit` proves raw evidence once; row admission then proves response support, feature support, labels, temporal history, and detector ranges. `Prediction` closes over response, projection, assignment, and anomaly evidence.

Vocabulary owned here: `EstimatorFamily`/`EstimatorKind`/`LinkFunction`/`OptimDriver`/`TimeSeriesModel`, `TemporalSpec`, the `Estimator`/`EstimatorModel`/`Prediction`/`FittedModel` carriers, `Design`, family-shaped `EstimatorPolicy`, `EstimatorFold`, and the `[02.1]-[HYPOTHESIS_LAW]` `StatisticalTest` axis. Dense factorizations ride `Tensor/blas#DENSE_ALGEBRA`; descriptive and distribution surfaces ride `MathNet.Numerics`; `ComputeReceipt`, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, and `ComparerAccessors.StringOrdinal` arrive settled; NodaTime `IClock` supplies instants — the App-owned `ClockPolicy` stays at composition. Conditioned multi-channel evidence enters detection from `Stats/signal#SIGNAL_LANE`; a fit lands the dedicated `Runtime/receipts#RECEIPT_UNION` `Fit` case; offline deep-training studies cross `ONE_GRADUATION_EVIDENCE` by content key.

## [01]-[INDEX]

- [01]-[ESTIMATOR_LANE]: Fit/Predict/Validate contract over one `EstimatorModel` carrier and one `Prediction` egress; `TemporalSpec` generates forecasting, CUSUM, Bayesian-online, and correlated-residual rows without reconstructible knobs.
- [01.1]-[HYPOTHESIS_LAW]: `StatisticalTest` axis binding each row's statistic kernel, sample-arity floor, and p-value tail over the matching `Distributions` CDF.

## [02]-[ESTIMATOR_LANE]

- Owner: `EstimatorFamily` is the receipt family axis; `EstimatorKind` rows carry supervised fit behavior; `TimeSeriesModel` rows carry forecast or detector fit behavior; `TemporalSpec` is the parameterized temporal generator and derives its `TimeSeriesModel`; `Estimator` types the problem; `ClusterShape` types grouping ingress; `EstimatorModel` carries fitted parameters; `Prediction` types response, projection, assignment, or anomaly egress; `Design` admits evidence once; `EstimatorPolicy` admits family policy; `FitContext` carries the proven correspondence; `IterativeEngine` owns torch-loss fitting; `EstimatorFold` owns `Fit`, `Predict`, and `Validate`.
- Cases: `EstimatorFamily` regression · reduction · cluster · classify · temporal; `EstimatorKind` owns the supervised/reduction/grouping/classification rows; `TimeSeriesModel` owns ar · arma · exponential-smoothing · state-space · cusum · bayesian-online · correlated-residual; `TemporalSpec` carries one parameter-complete case for each temporal row; `EstimatorModel` adds `Detector` beside the fit carriers; `Prediction` adds `Anomaly` beside `Response`/`Projection`/`Assignment`.
- Entry: `Design.Admit(Matrix<double>, Option<Vector<double>>)` proves non-empty, finite, aligned evidence. `Fit` proves family correspondence, policy ranges, estimator support, and `TemporalSpec` history/ranges before dispatch. `Predict` projects, assigns, forecasts, or scores anomalies through the total `EstimatorModel` switch. `Validate` scores supervised held-out folds and forecasting forward chains; unsupervised detectors do not fabricate validation labels.
- Auto: `Fit` flattens unions once into `FitContext`, then dispatches the row kernel. Temporal forecasting routes AR through thin QR and ARMA/Holt/state-space through `LevenbergMarquardt`; detection fits one admitted multivariate Gaussian baseline through `Admission.Definite`, then CUSUM folds whitened innovation magnitude, Bayesian-online maintains a budget-capped run-length posterior with conjugate known-covariance mean updates, and correlated-residual scoring reads a `ChiSquared.InvCDF` threshold over Mahalanobis evidence. `Validate` derives contiguous, forward-chain, or unsupported behavior from the typed estimator case.
- Receipt: a fit emits the dedicated `Fit` `ComputeReceipt` case `Runtime/receipts#RECEIPT_UNION` declares for this lane (one case row per measured concern, as the FEA `Solver/contract#SOLVE_CONTRACT` `Solve` and the optimizer/sweep/clash/twin/uncertainty cases each own a row rather than overloading a sibling), carrying family, estimator key, carrier parameter count, iteration count, residual, converged flag, the named fit-quality value, the metric label read off the row's `Metric` column (never a per-arm literal), and retained reduction rank; a closed-form fit ALSO emits the blas `Factorization` receipt under the same `CorrelationId`. Fit-quality and rank read back operator-visibly through the receipt stream (a stall through `ReceiptFolds.Nonconverged`) instead of dying write-only on the carrier.
- Packages: MathNet.Numerics, TorchSharp, libtorch-cpu, HyperJet (temporal-fit exact-Jacobian scalar-AD — recurrences authored once over `DDScalar`, the LM hyperdual arm reading `GetGradient()`), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new temporal modality is one `TemporalSpec` case deriving one `TimeSeriesModel` row and binding one kernel; the spec owns every non-reconstructible parameter. New fitted or prediction shapes extend `EstimatorModel` or `Prediction` only when payload timing differs. Per-model estimator classes, detector DTOs, and universal temporal knob records are rejected.
- Boundary: each row binds its genuine mechanism; forced SVD or torch-loss routing is rejected. Closed-form estimators reuse `Tensor/blas#DENSE_ALGEBRA`, GLM rows minimize canonical-link deviance, specialized grouping rows retain their mutation-local kernels, and detector covariance factors through `Admission.Definite`. `Prediction` is total across response, projection, assignment, and anomaly evidence; neither `Tensor` nor an untyped score array crosses the boundary. `Stats/signal#SIGNAL_LANE` produces conditioned evidence, Stats owns reusable changepoint/anomaly detection, and the digital twin consumes that detector beside its optimizer-owned surrogate. Hypothesis tests validate `Solver/uncertainty#UNCERTAINTY_LANE` samples; offline deep training remains Python-owned behind `ONE_GRADUATION_EVIDENCE`.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EstimatorFamily {
    public static readonly EstimatorFamily Regression = new("regression");
    public static readonly EstimatorFamily Reduction = new("reduction");
    public static readonly EstimatorFamily Cluster = new("cluster");
    public static readonly EstimatorFamily Classify = new("classify");
    public static readonly EstimatorFamily Temporal = new("temporal");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LinkFunction {
    // Mean = the inverse link g⁻¹(η) applied at predict; Variance = the family variance V(μ) the GLM
    // dispersion/working weight reads. Identity is Gaussian (constant V=1) — NOT the Bernoulli μ(1−μ).
    public static readonly LinkFunction Identity = new("identity", static eta => eta, static _ => 1.0);
    public static readonly LinkFunction Logit = new("logit", static eta => 1.0 / (1.0 + Math.Exp(-eta)), static mu => Math.Max(1e-9, mu * (1.0 - mu)));
    public static readonly LinkFunction Log = new("log", static eta => Math.Exp(eta), static mu => Math.Max(1e-9, mu));
    public static readonly LinkFunction Inverse = new("inverse", static eta => 1.0 / eta, static mu => mu * mu);

    private readonly Func<double, double> inverseLink;
    private readonly Func<double, double> variance;

    public double Mean(double eta) => inverseLink(eta);
    public double Variance(double mu) => variance(mu);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OptimDriver {
    public static readonly OptimDriver LBfgs = new("lbfgs", lineSearch: true, static (p, lr) => torch.optim.LBFGS(p, lr, max_iter: 20));
    public static readonly OptimDriver Adam = new("adam", lineSearch: false, static (p, lr) => torch.optim.Adam(p, lr));

    private readonly Func<IEnumerable<Parameter>, double, torch.optim.Optimizer> bind;

    public bool LineSearch { get; }

    public torch.optim.Optimizer Bind(IEnumerable<Parameter> parameters, double lr) => bind(parameters, lr);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EstimatorKind {
    public static readonly EstimatorKind Ols = new("ols", EstimatorFamily.Regression, supervised: true, metric: "r2", EstimatorFold.Ordinary, EstimatorFold.RealResponse, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind Ridge = new("ridge", EstimatorFamily.Regression, supervised: true, metric: "r2", EstimatorFold.Ridged, EstimatorFold.RegularizedResponse, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind Lasso = new("lasso", EstimatorFamily.Regression, supervised: true, metric: "r2", EstimatorFold.Penalized, EstimatorFold.IterativeResponse, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind GlmLogistic = new("glm-logistic", EstimatorFamily.Regression, supervised: true, metric: "r2",
        static ctx => EstimatorFold.Deviance(ctx, static (eta, y) => (eta.exp().log1p() - y * eta).mean()), EstimatorFold.UnitResponse, OptimDriver.LBfgs, LinkFunction.Logit);
    public static readonly EstimatorKind GlmPoisson = new("glm-poisson", EstimatorFamily.Regression, supervised: true, metric: "r2",
        static ctx => EstimatorFold.Deviance(ctx, static (eta, y) => (eta.exp() - y * eta).mean()), EstimatorFold.CountResponse, OptimDriver.LBfgs, LinkFunction.Log);
    public static readonly EstimatorKind GlmGamma = new("glm-gamma", EstimatorFamily.Regression, supervised: true, metric: "r2",
        static ctx => EstimatorFold.Deviance(ctx, static (eta, y) => (eta + y * eta.neg().exp()).mean()), EstimatorFold.PositiveResponse, OptimDriver.LBfgs, LinkFunction.Log);
    public static readonly EstimatorKind Pca = new("pca", EstimatorFamily.Reduction, supervised: false, metric: "explained-energy", EstimatorFold.Principal, EstimatorFold.ReductionDesign, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind KernelPca = new("kernel-pca", EstimatorFamily.Reduction, supervised: false, metric: "explained-energy", EstimatorFold.KernelPrincipal, EstimatorFold.KernelReductionDesign, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind Nmf = new("nmf", EstimatorFamily.Reduction, supervised: false, metric: "reconstruction-error", EstimatorFold.NonNegative, EstimatorFold.NonNegativeReductionDesign, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind KMeans = new("kmeans", EstimatorFamily.Cluster, supervised: false, metric: "inertia", EstimatorFold.Lloyd, EstimatorFold.GroupingDesign, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind Gmm = new("gmm", EstimatorFamily.Cluster, supervised: false, metric: "log-likelihood", EstimatorFold.ExpectationMaximization, EstimatorFold.MixtureDesign, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind Dbscan = new("dbscan", EstimatorFamily.Cluster, supervised: false, metric: "cluster-count", EstimatorFold.Reachability, EstimatorFold.DensityDesign, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind Hierarchical = new("hierarchical", EstimatorFamily.Cluster, supervised: false, metric: "inertia", EstimatorFold.Agglomerative, EstimatorFold.AnyDesign, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind Knn = new("knn", EstimatorFamily.Classify, supervised: true, metric: "accuracy", EstimatorFold.Neighborhood, EstimatorFold.NeighborhoodDesign, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind Svm = new("svm", EstimatorFamily.Classify, supervised: true, metric: "accuracy", EstimatorFold.MarginMachines, EstimatorFold.MarginDesign, OptimDriver.Adam, LinkFunction.Identity);
    public static readonly EstimatorKind NaiveBayes = new("naive-bayes", EstimatorFamily.Classify, supervised: true, metric: "accuracy", EstimatorFold.GaussianBayes, EstimatorFold.ClassLabels, OptimDriver.Adam, LinkFunction.Identity);

    private EstimatorKind(
        string key, EstimatorFamily family, bool supervised, string metric, Func<FitContext, Fin<FittedModel>> fit,
        Func<FitContext, Fin<Unit>> admit, OptimDriver driver, LinkFunction link) : this(key) {
        Family = family;
        Supervised = supervised;
        Metric = metric;
        this.fit = fit;
        this.admit = admit;
        Driver = driver;
        Link = link;
    }

    private readonly Func<FitContext, Fin<FittedModel>> fit;
    private readonly Func<FitContext, Fin<Unit>> admit;

    public EstimatorFamily Family { get; }
    public bool Supervised { get; }
    public string Metric { get; }
    public OptimDriver Driver { get; }
    public LinkFunction Link { get; }

    internal Fin<FittedModel> Fit(FitContext context) => fit(context);
    internal Fin<Unit> Admit(FitContext context) => admit(context);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimeSeriesModel {
    public static readonly TimeSeriesModel Ar = new("ar", "innovation-variance", EstimatorFold.AutoRegress);
    public static readonly TimeSeriesModel Arma = new("arma", "innovation-variance", EstimatorFold.MovingAverage);
    public static readonly TimeSeriesModel ExponentialSmoothing = new("exponential-smoothing", "innovation-variance", EstimatorFold.Holt);
    public static readonly TimeSeriesModel StateSpace = new("state-space", "innovation-variance", EstimatorFold.StateSpace);
    public static readonly TimeSeriesModel Cusum = new("cusum", "baseline-log-likelihood", EstimatorFold.CusumBaseline);
    public static readonly TimeSeriesModel BayesianOnline = new("bayesian-online", "baseline-log-likelihood", EstimatorFold.BayesianBaseline);
    public static readonly TimeSeriesModel CorrelatedResidual = new("correlated-residual", "baseline-log-likelihood", EstimatorFold.CorrelatedBaseline);

    private TimeSeriesModel(string key, string metric, Func<FitContext, Fin<FittedModel>> fit) : this(key) {
        Metric = metric;
        this.fit = fit;
    }

    private readonly Func<FitContext, Fin<FittedModel>> fit;

    public string Metric { get; }

    internal Fin<FittedModel> Fit(FitContext context) => fit(context);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClusterShape {
    private ClusterShape() { }

    public sealed record Partitioned(int Groups) : ClusterShape;
    public sealed record Density : ClusterShape;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TemporalSpec {
    private TemporalSpec() { }

    public sealed record Ar(int Lags) : TemporalSpec;
    public sealed record Arma(int Lags) : TemporalSpec;
    public sealed record ExponentialSmoothing : TemporalSpec;
    public sealed record StateSpace : TemporalSpec;
    public sealed record Cusum(int Warmup, double Drift, double Threshold) : TemporalSpec;
    public sealed record BayesianOnline(int Warmup, double Hazard, double Threshold) : TemporalSpec;
    public sealed record CorrelatedResidual(int Warmup, double FalsePositiveRate, double Ridge) : TemporalSpec;

    public TimeSeriesModel Model => Switch(
        ar: static _ => TimeSeriesModel.Ar, arma: static _ => TimeSeriesModel.Arma,
        exponentialSmoothing: static _ => TimeSeriesModel.ExponentialSmoothing, stateSpace: static _ => TimeSeriesModel.StateSpace,
        cusum: static _ => TimeSeriesModel.Cusum, bayesianOnline: static _ => TimeSeriesModel.BayesianOnline,
        correlatedResidual: static _ => TimeSeriesModel.CorrelatedResidual);

    public bool Forecasts => Switch(
        ar: static _ => true, arma: static _ => true, exponentialSmoothing: static _ => true, stateSpace: static _ => true,
        cusum: static _ => false, bayesianOnline: static _ => false, correlatedResidual: static _ => false);

    internal int History => Switch(
        ar: static s => s.Lags, arma: static s => s.Lags, exponentialSmoothing: static _ => 2, stateSpace: static _ => 3,
        cusum: static s => s.Warmup, bayesianOnline: static s => s.Warmup, correlatedResidual: static s => s.Warmup);

    internal Fin<Unit> Admit(int rows, int columns) => Switch(
        ar: s => Forecast(s.Lags, rows, columns), arma: s => Forecast(s.Lags, rows, columns),
        exponentialSmoothing: _ => Forecast(2, rows, columns), stateSpace: _ => Forecast(3, rows, columns),
        cusum: s => Detection(s.Warmup, rows, columns, s.Drift >= 0.0 && s.Threshold > 0.0, s.Drift, s.Threshold),
        bayesianOnline: s => Detection(s.Warmup, rows, columns, s.Hazard > 0.0 && s.Hazard < 1.0 && s.Threshold > 0.0 && s.Threshold < 1.0, s.Hazard, s.Threshold),
        correlatedResidual: s => Detection(s.Warmup, rows, columns, s.FalsePositiveRate > 0.0 && s.FalsePositiveRate < 1.0 && s.Ridge > 0.0, s.FalsePositiveRate, s.Ridge));

    private static Fin<Unit> Forecast(int history, int rows, int columns) =>
        columns == 1 && history >= 1 && rows > history
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComputeFault.Create($"<temporal-forecast:{rows}x{columns}:history={history}>"));

    private static Fin<Unit> Detection(int warmup, int rows, int columns, bool range, double first, double second) =>
        warmup >= Math.Max(4, columns + 1) && rows > warmup && range && double.IsFinite(first) && double.IsFinite(second)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComputeFault.Create($"<temporal-detection:{rows}x{columns}:warmup={warmup}:{first}:{second}>"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Estimator {
    private Estimator() { }

    public sealed record Regression(EstimatorKind Kind, Option<LinkFunction> Link) : Estimator;
    public sealed record Reduction(EstimatorKind Kind, int Rank) : Estimator;
    public sealed record Cluster(EstimatorKind Kind, ClusterShape Shape) : Estimator;
    public sealed record Classify(EstimatorKind Kind) : Estimator;
    public sealed record Temporal(TemporalSpec Spec) : Estimator;

    public EstimatorFamily Family => Switch(
        regression: static _ => EstimatorFamily.Regression, reduction: static _ => EstimatorFamily.Reduction,
        cluster: static _ => EstimatorFamily.Cluster, classify: static _ => EstimatorFamily.Classify,
        temporal: static _ => EstimatorFamily.Temporal);

    // One uniform estimator key and metric label for the receipt — both read the row columns, so no arm
    // re-derives per-kind knowledge through nested ternaries or label literals.
    public string Key => Switch(
        regression: static r => r.Kind.Key, reduction: static d => d.Kind.Key, cluster: static c => c.Kind.Key,
        classify: static c => c.Kind.Key, temporal: static t => t.Spec.Model.Key);

    public string Metric => Switch(
        regression: static r => r.Kind.Metric, reduction: static d => d.Kind.Metric, cluster: static c => c.Kind.Metric,
        classify: static c => c.Kind.Metric, temporal: static t => t.Spec.Model.Metric);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimatorModel {
    private EstimatorModel() { }

    public sealed record Linear(Vector<double> Coefficients, double Intercept, LinkFunction Link) : EstimatorModel;
    public sealed record Basis(Matrix<double> Components, Vector<double> Singular, Vector<double> Mean, double EnergyFraction) : EstimatorModel;
    public sealed record KernelBasis(Matrix<double> Training, Matrix<double> Alphas, Vector<double> Eigen, double Bandwidth, Vector<double> RowMean, double GrandMean) : EstimatorModel;
    public sealed record Factors(Matrix<double> Encoder, Matrix<double> Components) : EstimatorModel;
    public sealed record Partition(Matrix<double> Centroids, ImmutableArray<int> Labels) : EstimatorModel;
    public sealed record Density(Matrix<double> Training, ImmutableArray<int> Labels, ImmutableArray<bool> Core, double Radius) : EstimatorModel;
    public sealed record Mixture(Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights) : EstimatorModel;
    // One LS-SVM machine per distinct label (one-vs-rest); binary targets reduce to two symmetric machines
    // and argmax over decisions recovers the sign rule, so multiclass is the same carrier, never a sibling.
    public sealed record Margin(Matrix<double> Training, Seq<(int Label, Vector<double> Duals, double Bias)> Machines, double Bandwidth) : EstimatorModel;
    public sealed record Neighbors(Matrix<double> Design, ImmutableArray<int> Labels, int K) : EstimatorModel;
    public sealed record Bayes(Matrix<double> Means, Matrix<double> Variances, Vector<double> Priors) : EstimatorModel;
    public sealed record Lag(Vector<double> ArCoefficients, Vector<double> MaCoefficients, double Variance, Vector<double> Tail, TimeSeriesModel Model) : EstimatorModel;
    public sealed record Detector(Vector<double> Mean, Cholesky<double> Scale, TemporalSpec Spec, int MaxRunLength) : EstimatorModel;

    public long ParameterCount => Switch(
        linear: static c => (long)c.Coefficients.Count,
        basis: static b => (long)b.Components.RowCount,
        kernelBasis: static k => (long)k.Eigen.Count,
        factors: static f => (long)f.Components.RowCount * f.Components.ColumnCount,
        partition: static p => (long)p.Centroids.RowCount,
        density: static d => (long)d.Core.Count(static core => core),
        mixture: static m => (long)m.Weights.Count,
        margin: static m => m.Machines.Fold(0L, static (acc, machine) => acc + machine.Duals.Count),
        neighbors: static n => (long)n.Design.RowCount,
        bayes: static b => (long)b.Priors.Count,
        lag: static l => (long)(l.ArCoefficients.Count + l.MaCoefficients.Count),
        detector: static d => (long)d.Mean.Count + (long)d.Mean.Count * (d.Mean.Count + 1) / 2);

    // Retained reduction rank for the receipt's RetainedRank slot — basis/kernel-basis component count, NMF inner
    // dimension; zero on non-reduction carriers so a non-reduction fit reads no retained rank without a family probe.
    public int RetainedRank => Switch(
        basis: static b => b.Singular.Count,
        kernelBasis: static k => k.Eigen.Count,
        factors: static f => f.Encoder.ColumnCount,
        linear: static _ => 0, partition: static _ => 0, density: static _ => 0, mixture: static _ => 0,
        margin: static _ => 0, neighbors: static _ => 0, bayes: static _ => 0, lag: static _ => 0, detector: static _ => 0);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Prediction {
    private Prediction() { }

    public sealed record Response(Vector<double> Values) : Prediction;
    public sealed record Projection(Matrix<double> Scores) : Prediction;
    public sealed record Assignment(ImmutableArray<int> Labels) : Prediction;
    public sealed record Anomaly(Vector<double> Scores, ImmutableArray<bool> Changes) : Prediction;
}

public sealed record FitBudget(int MaxIterations, double Tolerance) {
    public static readonly FitBudget Canonical = new(MaxIterations: 1000, Tolerance: 1e-8);
    public static readonly FitBudget Grouping = new(MaxIterations: 300, Tolerance: 1e-8);

    internal Fin<Unit> Admit() =>
        MaxIterations < 1 || !double.IsFinite(Tolerance) || Tolerance <= 0.0
            ? Fin.Fail<Unit>(ComputeFault.Create($"<fit-budget:{MaxIterations}:{Tolerance}>"))
            : Fin.Succ(unit);
}

// Family-shaped policy keeps unrelated family values unconstructible; each EstimatorKind admission row consumes
// only its mechanism's subset before the flattened interior context reaches the kernel.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimatorPolicy {
    private EstimatorPolicy() { }

    public sealed record Regression(double Regularization, double LearningRate, FitBudget Budget) : EstimatorPolicy;
    public sealed record Reduction(double EnergyFraction, double Bandwidth, FitBudget Budget) : EstimatorPolicy;
    public sealed record Grouping(double Bandwidth, int Neighbors, double Ridge, FitBudget Budget) : EstimatorPolicy;
    public sealed record Classification(double Regularization, double Bandwidth, int Neighbors) : EstimatorPolicy;
    public sealed record Temporal(FitBudget Budget) : EstimatorPolicy;

    public static readonly EstimatorPolicy CanonicalRegression = new Regression(Regularization: 1e-3, LearningRate: 0.05, FitBudget.Canonical);
    public static readonly EstimatorPolicy CanonicalReduction = new Reduction(EnergyFraction: 0.95, Bandwidth: 1.0, FitBudget.Canonical);
    public static readonly EstimatorPolicy CanonicalGrouping = new Grouping(Bandwidth: 0.5, Neighbors: 4, Ridge: 1e-3, FitBudget.Grouping);
    public static readonly EstimatorPolicy CanonicalClassification = new Classification(Regularization: 1e-3, Bandwidth: 1.0, Neighbors: 5);
    public static readonly EstimatorPolicy CanonicalTemporal = new Temporal(FitBudget.Canonical);

    public EstimatorFamily Family => Switch(
        regression: static _ => EstimatorFamily.Regression, reduction: static _ => EstimatorFamily.Reduction,
        grouping: static _ => EstimatorFamily.Cluster, classification: static _ => EstimatorFamily.Classify,
        temporal: static _ => EstimatorFamily.Temporal);

    internal Fin<Unit> Admit() => Switch(
        regression: static p => ScalarPolicy(p.Regularization >= 0.0 && p.LearningRate > 0.0, p.Regularization, p.LearningRate).Bind(_ => p.Budget.Admit()),
        reduction: static p => ScalarPolicy(p.EnergyFraction > 0.0 && p.EnergyFraction <= 1.0 && p.Bandwidth > 0.0, p.EnergyFraction, p.Bandwidth).Bind(_ => p.Budget.Admit()),
        grouping: static p => ScalarPolicy(p.Bandwidth > 0.0 && p.Ridge > 0.0 && p.Neighbors >= 1, p.Bandwidth, p.Ridge).Bind(_ => p.Budget.Admit()),
        classification: static p => ScalarPolicy(p.Regularization > 0.0 && p.Bandwidth > 0.0 && p.Neighbors >= 1, p.Regularization, p.Bandwidth),
        temporal: static p => p.Budget.Admit());

    private static Fin<Unit> ScalarPolicy(bool range, double first, double second) =>
        range && double.IsFinite(first) && double.IsFinite(second)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComputeFault.Create($"<estimator-policy:{first}:{second}>"));
}

// `Design.Admit` is the sole raw ingress; kernels receive finite, row-aligned evidence.
// `Split` preserves admission while cross-validation projects folds.
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
        features.RowCount < 1 || features.ColumnCount < 1
            ? Fin.Fail<Design>(ComputeFault.Create($"<design-empty:{features.RowCount}x{features.ColumnCount}>"))
        : !TensorPrimitives.IsFiniteAll<double>(features.AsColumnMajorArray() ?? features.ToColumnMajorArray())
            ? Fin.Fail<Design>(ComputeFault.Create("<design-nonfinite-features>"))
        : targets.Match(
            Some: y => y.Count != features.RowCount
                ? Fin.Fail<Design>(ComputeFault.Create($"<design-target-misaligned:{y.Count}!={features.RowCount}>"))
                : !TensorPrimitives.IsFiniteAll<double>(y.AsArray() ?? y.ToArray())
                    ? Fin.Fail<Design>(ComputeFault.Create("<design-nonfinite-targets>"))
                    : Fin.Succ(new Design(features, Optional(y))),
            None: () => Fin.Succ(new Design(features, None)));

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

// Interior mechanism carrier: the estimator and policy union payloads flatten once at the correspondence gate,
// so a kernel reads proven scalars and never re-probes a case or casts a policy.
internal sealed record FitContext(
    Estimator Estimator, Design Design, IClock Clock, LinkFunction Link, OptimDriver Driver,
    int Rank, Option<ClusterShape> Cluster, Option<TemporalSpec> Temporal,
    double Regularization, double LearningRate, double EnergyFraction, double Bandwidth, int Neighbors, double Ridge, FitBudget Budget);

public sealed record FittedModel(Estimator Estimator, EstimatorModel Carrier, double Quality, double Residual, int Iterations, bool Converged, Instant At) {
    public Fin<Prediction> Predict(Matrix<double> features) => EstimatorFold.Predict(this, features);
}

public sealed record ValidationReport(Estimator Estimator, Seq<double> FoldQuality, double Mean, double Spread, int Folds, Instant At);

// Torch-loss rows minimize a Tensor loss under torch.autograd + the row's OptimDriver inside one DisposeScope; the LBFGS line-search form re-evaluates the closure per probe.
// Every intermediate is reclaimed at scope exit and stamped by DisposeScopeManager.Statistics; AnomalyMode traps a NaN/inf fit, set_default_dtype floors at Float64, no Tensor escapes the lane.
public static class IterativeEngine {
    public static Fin<(Vector<double> Theta, double Loss, int Iterations, bool Converged, ThreadDisposeScopeStatistics Memory)> Minimize(
        Func<Tensor, Tensor, Tensor, Tensor> loss, Matrix<double> design, Vector<double> response, OptimDriver driver, double learningRate, FitBudget budget) {
        using DisposeScope scope = torch.NewDisposeScope();
        using AnomalyMode anomaly = new(enabled: true, check_nan: true);
        torch.set_default_dtype(ScalarType.Float64);
        Tensor x = torch.from_array(design.ToColumnMajorArray(), ScalarType.Float64).reshape(design.ColumnCount, design.RowCount).t();
        Tensor y = torch.from_array(response.AsArray() ?? response.ToArray(), ScalarType.Float64).reshape(response.Count);
        Parameter theta = new(torch.zeros(design.ColumnCount, ScalarType.Float64), requires_grad: true);
        torch.optim.Optimizer opt = driver.Bind([theta], learningRate);
        Tensor Step() { opt.zero_grad(); Tensor l = loss(theta, x, y); l.backward(); return l; }
        double last = double.MaxValue;
        int iter = 0;
        bool converged = false;
        for (; iter < budget.MaxIterations; iter++) {
            Tensor value = driver.LineSearch ? ((Modules.LBFGS)opt).step(Step) : Plain(opt, Step);
            double current = value.ReadCpuDouble(0);
            converged = Math.Abs(last - current) < budget.Tolerance;
            last = current;
            if (converged) { iter++; break; }
        }
        Vector<double> fitted = Vector<double>.Build.DenseOfArray(theta.detach().reshape(theta.NumberOfElements).data<double>().ToArray());
        return double.IsFinite(last)
            ? Fin.Succ((fitted, last, iter, converged, DisposeScopeManager.Statistics))
            : Fin.Fail<(Vector<double>, double, int, bool, ThreadDisposeScopeStatistics)>(new ComputeFault.ModelRejected("<estimator-nonfinite-loss>"));
    }

    public static Func<Tensor, Tensor, Tensor, Tensor> Lasso(double lambda) =>
        (theta, x, y) => 0.5 * x.matmul(theta).sub(y).pow(2).mean() + lambda * theta.abs().sum();

    private static Tensor Plain(torch.optim.Optimizer opt, Func<Tensor> closure) { Tensor l = closure(); opt.step(); return l; }
}

public static class EstimatorFold {
    private const double VarianceFloor = 1e-9;

    public static Fin<FittedModel> Fit(Estimator estimator, Design design, EstimatorPolicy policy, IClock clock) =>
        Admitted(estimator, design, policy, clock).Bind(static ctx => ctx.Estimator.Switch(
            state: ctx,
            regression: static (c, r) => r.Kind.Fit(c),
            reduction: static (c, d) => d.Kind.Fit(c),
            cluster: static (c, g) => g.Kind.Fit(c),
            classify: static (c, k) => k.Kind.Fit(c),
            temporal: static (c, t) => t.Spec.Model.Fit(c)));

    public static Fin<Prediction> Predict(FittedModel model, Matrix<double> features) =>
        model.Carrier.Switch<Matrix<double>, Fin<Prediction>>(
            state: features,
            linear: static (x, c) => Fin.Succ<Prediction>(new Prediction.Response(x.Multiply(c.Coefficients).Add(c.Intercept).Map(c.Link.Mean))),
            basis: static (x, b) => Fin.Succ<Prediction>(new Prediction.Projection(Center(x, b.Mean).Multiply(b.Components.Transpose()))),
            kernelBasis: static (x, k) => Fin.Succ<Prediction>(new Prediction.Projection(KernelProject(x, k))),
            factors: static (x, f) => Fin.Succ<Prediction>(new Prediction.Projection(NonNegativeEncode(x, f.Components))),
            partition: static (x, p) => Fin.Succ<Prediction>(new Prediction.Assignment([.. x.EnumerateRows().Select(row => Nearest(row, p.Centroids))])),
            density: static (x, d) => Fin.Succ<Prediction>(new Prediction.Assignment([.. x.EnumerateRows().Select(row => DensityAssign(row, d))])),
            mixture: static (x, m) => Responsibilities(x, m).Map(static labels => (Prediction)new Prediction.Assignment(labels)),
            margin: static (x, m) => Fin.Succ<Prediction>(new Prediction.Assignment([.. x.EnumerateRows().Select(row => Strongest(row, m))])),
            neighbors: static (x, nbr) => Fin.Succ<Prediction>(new Prediction.Assignment([.. x.EnumerateRows().Select(row => Vote(row, nbr))])),
            bayes: static (x, b) => Fin.Succ<Prediction>(new Prediction.Assignment([.. x.EnumerateRows().Select(row => Posterior(row, b))])),
            lag: static (x, l) => Fin.Succ<Prediction>(new Prediction.Response(Forecast(l, x.RowCount))),
            detector: static (x, d) => Detect(d, x));

    // Split strategy derives from family: contiguous k-fold scores regression/classification;
    // expanding-window forward chaining scores temporal rows.
    public static Fin<ValidationReport> Validate(Estimator estimator, Design design, EstimatorPolicy policy, int folds, IClock clock) =>
        folds < 2 || folds > design.Rows
            ? Fin.Fail<ValidationReport>(ComputeFault.Create($"<validate-folds:{folds}/{design.Rows}>"))
            : (estimator is Estimator.Temporal { Spec.Forecasts: true }
                ? ForwardChain(estimator, design, policy, folds, clock)
                : estimator.Family == EstimatorFamily.Regression || estimator.Family == EstimatorFamily.Classify
                    ? HeldOut(estimator, design, policy, folds, clock)
                    : Fin.Fail<Seq<double>>(ComputeFault.Create($"<validate-no-heldout-score:{estimator.Key}>")))
            .Map(quality => {
                double mean = quality.Sum() / quality.Count;
                double spread = Math.Sqrt(quality.Map(q => (q - mean) * (q - mean)).Sum() / quality.Count);
                return new ValidationReport(estimator, quality, mean, spread, folds, clock.GetCurrentInstant());
            });

    public static ComputeReceipt Receipt(FittedModel model, CorrelationId correlation, Duration elapsed) =>
        new ComputeReceipt.Fit(model.Estimator.Family.Key, model.Estimator.Key, model.Carrier.ParameterCount, model.Iterations, model.Residual, model.Converged, model.Quality, model.Estimator.Metric, model.Carrier.RetainedRank) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    // --- [ADMISSION] ----------------------------------------------------------------------

    private static Fin<FitContext> Admitted(Estimator estimator, Design design, EstimatorPolicy policy, IClock clock) {
        (Option<EstimatorKind> kind, LinkFunction link, int rank, Option<ClusterShape> cluster, Option<TemporalSpec> temporal) = estimator.Switch(
            regression: static r => (Optional(r.Kind), r.Link.IfNone(r.Kind.Link), 0, Option<ClusterShape>.None, Option<TemporalSpec>.None),
            reduction: static d => (Optional(d.Kind), LinkFunction.Identity, d.Rank, Option<ClusterShape>.None, Option<TemporalSpec>.None),
            cluster: static c => (Optional(c.Kind), LinkFunction.Identity, 0, Optional(c.Shape), Option<TemporalSpec>.None),
            classify: static c => (Optional(c.Kind), LinkFunction.Identity, 0, Option<ClusterShape>.None, Option<TemporalSpec>.None),
            temporal: static t => (Option<EstimatorKind>.None, LinkFunction.Identity, 0, Option<ClusterShape>.None, Optional(t.Spec)));
        (double Regularization, double LearningRate, double EnergyFraction, double Bandwidth, int Neighbors, double Ridge, FitBudget Budget) knobs = policy.Switch(
            regression: static p => (p.Regularization, p.LearningRate, 0.0, 0.0, 0, 0.0, p.Budget),
            reduction: static p => (0.0, 0.0, p.EnergyFraction, p.Bandwidth, 0, 0.0, p.Budget),
            grouping: static p => (0.0, 0.0, 0.0, p.Bandwidth, p.Neighbors, p.Ridge, p.Budget),
            classification: static p => (p.Regularization, 0.0, 0.0, p.Bandwidth, p.Neighbors, 0.0, FitBudget.Canonical),
            temporal: static p => (0.0, 0.0, 0.0, 0.0, 0, 0.0, p.Budget));
        OptimDriver driver = kind.Map(static row => row.Driver).IfNone(OptimDriver.Adam);
        Fin<Unit> correspondence = kind.Filter(row => row.Family != estimator.Family).IsSome
                ? Fin.Fail<Unit>(ComputeFault.Create($"<estimator-family-miss:{estimator.Key}:{estimator.Family.Key}>"))
            : policy.Family != estimator.Family
                ? Fin.Fail<Unit>(ComputeFault.Create($"<estimator-policy-miss:{policy.Family.Key}:{estimator.Family.Key}>"))
            : kind.Filter(static row => row.Supervised).IsSome && design.Targets.IsNone
                ? Fin.Fail<Unit>(ComputeFault.Create($"<estimator-needs-targets:{estimator.Key}>"))
            : rank < 0 || rank > Math.Min(design.Rows, design.Columns)
                ? Fin.Fail<Unit>(ComputeFault.Create($"<estimator-rank:{rank}/{design.Rows}x{design.Columns}>"))
            : Fin.Succ(unit);
        return correspondence
            .Bind(_ => policy.Admit())
            .Bind(_ => temporal.Map(spec => spec.Admit(design.Rows, design.Columns)).IfNone(Fin.Succ(unit)))
            .Map(_ => new FitContext(estimator, design, clock, link, driver, rank, cluster, temporal,
                knobs.Regularization, knobs.LearningRate, knobs.EnergyFraction, knobs.Bandwidth, knobs.Neighbors, knobs.Ridge, knobs.Budget))
            .Bind(ctx => kind.Map(row => row.Admit(ctx)).IfNone(TemporalDesign(ctx)).Map(_ => ctx));
    }

    internal static Fin<Unit> AnyDesign(FitContext _) => Fin.Succ(unit);

    internal static Fin<Unit> RealResponse(FitContext context) =>
        context.Design.Targets.IsSome ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create("<estimator-needs-targets>"));

    internal static Fin<Unit> RegularizedResponse(FitContext context) =>
        RealResponse(context).Bind(_ => PositiveOrZero(context.Regularization, "regularization"));

    internal static Fin<Unit> IterativeResponse(FitContext context) =>
        RegularizedResponse(context).Bind(_ => Positive(context.LearningRate, "learning-rate")).Bind(_ => context.Budget.Admit());

    internal static Fin<Unit> UnitResponse(FitContext context) =>
        IterativeResponse(context).Bind(_ => ResponseGate(context.Design, static y => y.All(static value => value >= 0.0 && value <= 1.0), "unit-response"));

    internal static Fin<Unit> CountResponse(FitContext context) =>
        IterativeResponse(context).Bind(_ => ResponseGate(context.Design,
            static y => TensorPrimitives.IsIntegerAll<double>(y.AsArray() ?? y.ToArray()) && y.All(static value => value >= 0.0), "count-response"));

    internal static Fin<Unit> PositiveResponse(FitContext context) =>
        IterativeResponse(context).Bind(_ => ResponseGate(context.Design, static y => y.All(static value => value > 0.0), "positive-response"));

    internal static Fin<Unit> ReductionDesign(FitContext context) =>
        context.EnergyFraction > 0.0 && context.EnergyFraction <= 1.0 ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create("<reduction-energy>"));

    internal static Fin<Unit> KernelReductionDesign(FitContext context) =>
        ReductionDesign(context).Bind(_ => Positive(context.Bandwidth, "kernel-bandwidth"));

    internal static Fin<Unit> NonNegativeReductionDesign(FitContext context) =>
        NonNegativeFeatures(context.Design).Bind(_ => context.Budget.Admit());

    internal static Fin<Unit> GroupingDesign(FitContext context) =>
        Groups(context).Bind(_ => context.Budget.Admit());

    internal static Fin<Unit> MixtureDesign(FitContext context) =>
        Groups(context).Bind(_ => Positive(context.Ridge, "mixture-ridge")).Bind(_ => context.Budget.Admit());

    internal static Fin<Unit> DensityDesign(FitContext context) =>
        context.Cluster.ToFin(ComputeFault.Create("<density-shape-missing>")).Bind(shape => shape.Switch(
            partitioned: static _ => Fin.Fail<Unit>(ComputeFault.Create("<density-shape-partitioned>")),
            density: _ => Positive(context.Bandwidth, "density-bandwidth")
                .Bind(_ => context.Neighbors < context.Design.Rows ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create("<density-neighbors>")))));

    private static Fin<Unit> NonNegativeFeatures(Design design) =>
        TensorPrimitives.IsNegativeAny<double>(design.Features.AsColumnMajorArray() ?? design.Features.ToColumnMajorArray())
            ? Fin.Fail<Unit>(ComputeFault.Create("<estimator-negative-features>"))
            : Fin.Succ(unit);

    internal static Fin<Unit> ClassLabels(FitContext context) =>
        ResponseGate(context.Design, static y =>
            TensorPrimitives.IsIntegerAll<double>(y.AsArray() ?? y.ToArray()) && y.Distinct().Take(2).Count() == 2,
            "class-labels");

    internal static Fin<Unit> NeighborhoodDesign(FitContext context) =>
        ClassLabels(context).Bind(_ => context.Neighbors < context.Design.Rows
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComputeFault.Create("<knn-neighbors>")));

    internal static Fin<Unit> MarginDesign(FitContext context) =>
        ClassLabels(context).Bind(_ => Positive(context.Regularization, "margin-regularization")).Bind(_ => Positive(context.Bandwidth, "margin-bandwidth"));

    private static Fin<Unit> TemporalDesign(FitContext context) =>
        context.Temporal.ToFin(ComputeFault.Create("<temporal-spec-missing>"))
            .Bind(spec => spec.Admit(context.Design.Rows, context.Design.Columns))
            .Bind(_ => context.Temporal.Map(static spec => spec is TemporalSpec.BayesianOnline).IfNone(false) && context.Budget.MaxIterations < 2
                ? Fin.Fail<Unit>(ComputeFault.Create($"<bayesian-run-length:{context.Budget.MaxIterations}>"))
                : context.Budget.Admit());

    private static Fin<Unit> Positive(double value, string gate) =>
        double.IsFinite(value) && value > 0.0 ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create($"<estimator-{gate}:{value}>"));

    private static Fin<Unit> PositiveOrZero(double value, string gate) =>
        double.IsFinite(value) && value >= 0.0 ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create($"<estimator-{gate}:{value}>"));

    private static Fin<int> Groups(FitContext context) =>
        context.Cluster.ToFin(ComputeFault.Create("<partition-shape-missing>")).Bind(shape => shape.Switch(
            partitioned: group => group.Groups >= 1 && group.Groups <= context.Design.Rows
                ? Fin.Succ(group.Groups)
                : Fin.Fail<int>(ComputeFault.Create($"<estimator-groups:{group.Groups}/{context.Design.Rows}>")),
            density: static _ => Fin.Fail<int>(ComputeFault.Create("<partition-shape-density>"))));

    private static Fin<Unit> ResponseGate(Design design, Func<Vector<double>, bool> predicate, string gate) =>
        design.Targets.Match(
            Some: response => predicate(response)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(ComputeFault.Create($"<estimator-{gate}>")),
            None: () => Fin.Fail<Unit>(ComputeFault.Create($"<estimator-{gate}-missing>")));

    private static Fin<Vector<double>> Supervised(FitContext ctx) =>
        ctx.Design.Targets.ToFin(ComputeFault.Create($"<estimator-needs-targets:{ctx.Estimator.Key}>"));

    // --- [REGRESSION] ---------------------------------------------------------------------

    internal static Fin<FittedModel> Ordinary(FitContext ctx) => ClosedForm(ctx, tikhonov: 0.0);

    internal static Fin<FittedModel> Ridged(FitContext ctx) => ClosedForm(ctx, tikhonov: ctx.Regularization);

    // OLS/ridge share intercept-augmented thin-QR; ridge stacks the unpenalized-intercept `√λ·I` block.
    // `λ = 0` selects the unstacked identity without a mode flag.
    private static Fin<FittedModel> ClosedForm(FitContext ctx, double tikhonov) =>
        Supervised(ctx).Bind(y => {
            Matrix<double> design = Intercept(ctx.Design.Features);
            Matrix<double> a = tikhonov > 0.0 ? design.Stack(Tikhonov(design.ColumnCount, tikhonov)) : design;
            Vector<double> b = tikhonov > 0.0 ? Vector<double>.Build.Dense(design.RowCount + design.ColumnCount, i => i < design.RowCount ? y[i] : 0.0) : y;
            return DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), a, b, TolerancePolicy.Derive(a, b))
                .Map(theta => Build(ctx, y, Split(theta), 0.0, 1, true));
        });

    internal static Fin<FittedModel> Penalized(FitContext ctx) =>
        Supervised(ctx).Bind(y => IterativeEngine.Minimize(IterativeEngine.Lasso(ctx.Regularization), Intercept(ctx.Design.Features), y, ctx.Driver, ctx.LearningRate, ctx.Budget)
            .Map(fit => Build(ctx, y, Split(fit.Theta), fit.Loss, fit.Iterations, fit.Converged)));

    internal static Fin<FittedModel> Deviance(FitContext ctx, Func<Tensor, Tensor, Tensor> deviance) =>
        Supervised(ctx).Bind(y => IterativeEngine.Minimize((theta, x, yy) => deviance(x.matmul(theta), yy), Intercept(ctx.Design.Features), y, ctx.Driver, ctx.LearningRate, ctx.Budget)
            .Map(fit => Build(ctx, y, Split(fit.Theta), fit.Loss, fit.Iterations, fit.Converged)));

    private static FittedModel Build(FitContext ctx, Vector<double> y, (double Intercept, Vector<double> Slopes) split, double loss, int iterations, bool converged) {
        EstimatorModel.Linear carrier = new(split.Slopes, split.Intercept, ctx.Link);
        Vector<double> predicted = ctx.Design.Features.Multiply(split.Slopes).Add(split.Intercept).Map(ctx.Link.Mean);
        double r2 = GoodnessOfFit.CoefficientOfDetermination(predicted, y);
        // Pearson dispersion √(Σ (yᵢ−μᵢ)²/V(μᵢ)/(n−p)): the LinkFunction V(μ) weights each squared residual, so a logistic fit scores on its Bernoulli scale (V=μ(1−μ)) and Identity (V=1) recovers the Gaussian standard error.
        // Never an unweighted RMSE blind to the link variance — a residual not reading `Link.Variance` is the deleted decorative form.
        int dispersionDof = Math.Max(1, y.Count - split.Slopes.Count - 1);
        double dispersion = Math.Sqrt(Enumerable.Range(0, y.Count).Sum(i => Math.Pow(y[i] - predicted[i], 2) / Math.Max(1e-12, ctx.Link.Variance(predicted[i]))) / dispersionDof);
        return new FittedModel(ctx.Estimator, carrier, r2, double.IsFinite(dispersion) ? dispersion : loss, iterations, converged, ctx.Clock.GetCurrentInstant());
    }

    // --- [REDUCTION] ----------------------------------------------------------------------

    internal static Fin<FittedModel> Principal(FitContext ctx) {
        Matrix<double> x = ctx.Design.Features;
        Vector<double> mean = x.ColumnSums() / x.RowCount;
        Matrix<double> centered = Center(x, mean);
        return DenseOps.Decompose(centered, FactorizationKind.Svd).Bind(factor => factor.Switch(
            lu: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<pca-non-svd>")),
            qr: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<pca-non-svd>")),
            cholesky: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<pca-non-svd>")),
            evd: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<pca-non-svd>")),
            sketched: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<pca-non-svd>")),
            svd: f => {
                Vector<double> singular = f.Decomposition.S;
                double total = singular.PointwisePower(2.0).Sum();
                int rank = Retain(singular, total, Math.Min(ctx.Rank <= 0 ? singular.Count : ctx.Rank, singular.Count), ctx.EnergyFraction);
                Matrix<double> components = f.Decomposition.VT.SubMatrix(0, rank, 0, x.ColumnCount);
                double energy = total > 0.0 ? singular.SubVector(0, rank).PointwisePower(2.0).Sum() / total : 0.0;
                EstimatorModel.Basis carrier = new(components, singular.SubVector(0, rank), mean, energy);
                return Fin.Succ(new FittedModel(ctx.Estimator, carrier, energy, 1.0 - energy, 1, true, ctx.Clock.GetCurrentInstant()));
            }));
    }

    // Kernel-PCA: centered RBF Gram is the operand, its top eigenvectors the duals;
    // out-of-sample projection double-centers the test/train kernel against the stored row/grand means.
    internal static Fin<FittedModel> KernelPrincipal(FitContext ctx) {
        Matrix<double> x = ctx.Design.Features;
        int n = x.RowCount;
        Matrix<double> gram = Gram(x, x, ctx.Bandwidth);
        Vector<double> rowMean = gram.RowSums() / n;
        double grandMean = rowMean.Sum() / n;
        Matrix<double> centered = Matrix<double>.Build.Dense(n, n, (i, j) => gram[i, j] - rowMean[i] - rowMean[j] + grandMean);
        return DenseOps.Decompose(Admission.Symmetrize(centered), FactorizationKind.Evd).Bind(factor => factor.Switch(
            lu: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<kernel-pca-non-evd>")),
            qr: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<kernel-pca-non-evd>")),
            cholesky: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<kernel-pca-non-evd>")),
            svd: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<kernel-pca-non-evd>")),
            sketched: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<kernel-pca-non-evd>")),
            evd: f => {
                Vector<double> values = f.Decomposition.EigenValues.Map(static v => v.Real);
                int rank = Math.Min(ctx.Rank <= 0 ? n : ctx.Rank, n);
                int[] order = [.. Enumerable.Range(0, n).OrderByDescending(i => values[i]).Take(rank)];
                Vector<double> eigen = Vector<double>.Build.DenseOfArray([.. order.Select(i => values[i])]);
                Matrix<double> alphas = Matrix<double>.Build.Dense(n, rank, (i, c) => f.Decomposition.EigenVectors[i, order[c]] / Math.Sqrt(Math.Max(1e-12, eigen[c])));
                EstimatorModel.KernelBasis carrier = new(x, alphas, eigen, ctx.Bandwidth, rowMean, grandMean);
                double captured = eigen.Sum() / Math.Max(1e-12, values.Map(Math.Abs).Sum());
                return Fin.Succ(new FittedModel(ctx.Estimator, carrier, captured, 1.0 - captured, 1, true, ctx.Clock.GetCurrentInstant()));
            }));
    }

    // NMF (Lee–Seung multiplicative updates): X ≈ W·H, W,H ≥ 0, minimizing the Frobenius reconstruction.
    internal static Fin<FittedModel> NonNegative(FitContext ctx) {
        Matrix<double> x = ctx.Design.Features;
        int n = x.RowCount, m = x.ColumnCount, k = Math.Max(1, ctx.Rank);
        Matrix<double> w = Matrix<double>.Build.Random(n, k, 1).PointwiseAbs().Add(1e-3);
        Matrix<double> h = Matrix<double>.Build.Random(k, m, 2).PointwiseAbs().Add(1e-3);
        double residual = double.MaxValue;
        int iter = 0;
        bool converged = false;
        for (; iter < ctx.Budget.MaxIterations; iter++) {
            h = h.PointwiseMultiply((w.TransposeThisAndMultiply(x)).PointwiseDivide(w.TransposeThisAndMultiply(w).Multiply(h).Add(1e-12)));
            w = w.PointwiseMultiply((x.TransposeAndMultiply(h)).PointwiseDivide(w.Multiply(h.TransposeAndMultiply(h)).Add(1e-12)));
            double next = (x - w.Multiply(h)).FrobeniusNorm();
            converged = Math.Abs(residual - next) < ctx.Budget.Tolerance;
            residual = next;
            if (converged) { iter++; break; }
        }
        return Fin.Succ(new FittedModel(ctx.Estimator, new EstimatorModel.Factors(w, h), -residual, residual, iter, converged, ctx.Clock.GetCurrentInstant()));
    }

    // --- [CLUSTER] ------------------------------------------------------------------------

    internal static Fin<FittedModel> Lloyd(FitContext ctx) => Groups(ctx).Map(k => {
        Matrix<double> x = ctx.Design.Features;
        int n = x.RowCount;
        Matrix<double> centroids = Seed(x, k);
        int[] labels = new int[n];
        int iter = 0;
        bool moved = true;
        for (; iter < ctx.Budget.MaxIterations && moved; iter++) {
            moved = false;
            for (int i = 0; i < n; i++) {
                int best = Nearest(x.Row(i), centroids);
                if (best != labels[i]) { labels[i] = best; moved = true; }
            }
            centroids = Recenter(x, labels, k, centroids);
        }
        double inertia = Inertia(x, labels, centroids);
        return new FittedModel(ctx.Estimator, new EstimatorModel.Partition(centroids, [.. labels]), -inertia, inertia / Math.Max(1, n), iter, !moved, ctx.Clock.GetCurrentInstant());
    });

    // GMM-EM as a Fin-threaded fold: each EmStep computes responsibilities through the gated Cholesky log-density and re-estimates (weights, means, covariances), short-circuiting once the log-likelihood stalls.
    // A Cholesky failure on a degenerate covariance aborts the whole fit through the rail.
    internal static Fin<FittedModel> ExpectationMaximization(FitContext ctx) => Groups(ctx).Bind(k => {
        Matrix<double> x = ctx.Design.Features;
        int n = x.RowCount, dim = x.ColumnCount;
        Fin<(Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights, double LogLik, int Iterations, bool Converged)> seed = Fin.Succ((
            Means: Seed(x, k),
            Covariances: toSeq(Enumerable.Range(0, k).Select(_ => (Matrix<double>)Matrix<double>.Build.DiagonalIdentity(dim))),
            Weights: Vector<double>.Build.Dense(k, 1.0 / k),
            LogLik: double.NegativeInfinity, Iterations: 0, Converged: false));
        return toSeq(Enumerable.Range(0, ctx.Budget.MaxIterations))
            .Fold(seed, (acc, _) => acc.Bind(s => s.Converged ? acc : EmStep(x, s, dim, ctx.Ridge, ctx.Budget.Tolerance)))
            .Map(s => new FittedModel(ctx.Estimator, new EstimatorModel.Mixture(s.Means, s.Covariances, s.Weights), s.LogLik, double.IsFinite(s.LogLik) ? -s.LogLik / Math.Max(1, n) : double.MaxValue, s.Iterations, s.Converged, ctx.Clock.GetCurrentInstant()));
    });

    private static Fin<(Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights, double LogLik, int Iterations, bool Converged)> EmStep(
        Matrix<double> x, (Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights, double LogLik, int Iterations, bool Converged) s, int dim, double ridge, double tolerance) =>
        Choleskies(s.Covariances, ridge).Map(chols => {
            int n = x.RowCount, k = s.Weights.Count;
            Matrix<double> gamma = Matrix<double>.Build.Dense(n, k);
            double evidence = 0.0;
            for (int i = 0; i < n; i++) {
                Vector<double> log = Vector<double>.Build.Dense(k, j => Math.Log(Math.Max(1e-300, s.Weights[j])) + LogGaussian(x.Row(i), s.Means.Row(j), chols[j], dim));
                double max = log.Maximum();
                double sum = log.Map(v => Math.Exp(v - max)).Sum();
                evidence += max + Math.Log(sum);
                for (int j = 0; j < k; j++) { gamma[i, j] = Math.Exp(log[j] - max) / sum; }
            }
            Vector<double> nk = gamma.ColumnSums();
            Matrix<double> weighted = gamma.TransposeThisAndMultiply(x);
            Matrix<double> means = Matrix<double>.Build.Dense(k, dim, (j, f) => weighted[j, f] / Math.Max(1e-9, nk[j]));
            Seq<Matrix<double>> covariances = toSeq(Enumerable.Range(0, k).Select(j => WeightedCovariance(x, gamma.Column(j), means.Row(j), nk[j], ridge)));
            return (means, covariances, nk / n, evidence, s.Iterations + 1, Math.Abs(evidence - s.LogLik) < tolerance);
        });

    internal static Fin<FittedModel> Reachability(FitContext ctx) {
        Matrix<double> x = ctx.Design.Features;
        int n = x.RowCount, minPts = ctx.Neighbors;
        double eps = ctx.Bandwidth;
        int[] labels = Enumerable.Repeat(-2, n).ToArray();
        bool[] core = new bool[n];
        int cluster = -1;
        for (int i = 0; i < n; i++) {
            if (labels[i] != -2) { continue; }
            int[] neighbors = Region(x, i, eps);
            if (neighbors.Length < minPts) { labels[i] = -1; continue; }
            core[i] = true;
            cluster++;
            Queue<int> frontier = new(neighbors);
            labels[i] = cluster;
            while (frontier.Count > 0) {
                int q = frontier.Dequeue();
                if (labels[q] == -1) { labels[q] = cluster; }
                if (labels[q] != -2) { continue; }
                labels[q] = cluster;
                int[] reach = Region(x, q, eps);
                if (reach.Length >= minPts) {
                    core[q] = true;
                    foreach (int r in reach) { frontier.Enqueue(r); }
                }
            }
        }
        double noise = labels.Count(static label => label < 0) / (double)n;
        return Fin.Succ(new FittedModel(ctx.Estimator, new EstimatorModel.Density(x, [.. labels], [.. core], eps), cluster + 1, noise, 1, true, ctx.Clock.GetCurrentInstant()));
    }

    internal static Fin<FittedModel> Agglomerative(FitContext ctx) => Groups(ctx).Map(target => {
        Matrix<double> x = ctx.Design.Features;
        int n = x.RowCount;
        int[] labels = Enumerable.Range(0, n).ToArray();
        List<List<int>> clusters = Enumerable.Range(0, n).Select(i => new List<int> { i }).ToList();
        while (clusters.Count > target) {
            (int A, int B, double Best) = (0, 1, double.MaxValue);
            for (int a = 0; a < clusters.Count; a++) {
                for (int b = a + 1; b < clusters.Count; b++) {
                    double linkage = AverageLinkage(x, clusters[a], clusters[b]);
                    if (linkage < Best) { (A, B, Best) = (a, b, linkage); }
                }
            }
            clusters[A].AddRange(clusters[B]);
            clusters.RemoveAt(B);
        }
        for (int g = 0; g < clusters.Count; g++) { foreach (int member in clusters[g]) { labels[member] = g; } }
        Matrix<double> centroids = Recenter(x, labels, clusters.Count, Matrix<double>.Build.Dense(clusters.Count, x.ColumnCount));
        double spread = Inertia(x, labels, centroids);
        return new FittedModel(ctx.Estimator, new EstimatorModel.Partition(centroids, [.. labels]), -spread, spread / Math.Max(1, n), n - clusters.Count, true, ctx.Clock.GetCurrentInstant());
    });

    // --- [CLASSIFY] -----------------------------------------------------------------------

    // One-vs-rest LS-SVM runs one regularized-Gram KKT solve per label; binary targets reduce to two machines.
    internal static Fin<FittedModel> MarginMachines(FitContext ctx) =>
        Supervised(ctx).Bind(y => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount;
            int[] classes = [.. y.Select(static v => (int)v).Distinct().Order()];
            Matrix<double> gram = Gram(x, x, ctx.Bandwidth);
            return toSeq(classes).TraverseM(label => Machine(gram, y, label, ctx.Regularization)).As()
                .Map(machines => {
                    EstimatorModel.Margin carrier = new(x, machines, ctx.Bandwidth);
                    double accuracy = Enumerable.Range(0, n).Count(i => Strongest(x.Row(i), carrier) == (int)y[i]) / (double)n;
                    return new FittedModel(ctx.Estimator, carrier, accuracy, 1.0 - accuracy, 1, true, ctx.Clock.GetCurrentInstant());
                });
        });

    // LS-SVM KKT system [[0, sᵀ],[s, K+I/γ]]·[b; α] = [0; 1] for one machine's ±1 indicator s.
    private static Fin<(int Label, Vector<double> Duals, double Bias)> Machine(Matrix<double> gram, Vector<double> y, int label, double regularization) {
        int n = gram.RowCount;
        Vector<double> signed = Vector<double>.Build.Dense(n, i => (int)y[i] == label ? 1.0 : -1.0);
        Matrix<double> kkt = Matrix<double>.Build.Dense(n + 1, n + 1, (i, j) =>
            i == 0 && j == 0 ? 0.0
            : i == 0 ? signed[j - 1]
            : j == 0 ? signed[i - 1]
            : gram[i - 1, j - 1] + (i == j ? 1.0 / regularization : 0.0));
        Vector<double> rhs = Vector<double>.Build.Dense(n + 1, i => i == 0 ? 0.0 : 1.0);
        return DenseRoute.Solve(new FactorRoute.SquarePivoting(), kkt, rhs, TolerancePolicy.Derive(kkt, rhs))
            .Map(solution => (label, Vector<double>.Build.Dense(n, i => solution[i + 1] * signed[i]), solution[0]));
    }

    // kNN is the lazy store; quality is LEAVE-ONE-OUT accuracy (each row voted with itself excluded), because
    // plain training accuracy of a 1-NN-containing vote is unconditionally perfect — evidence, not decoration.
    internal static Fin<FittedModel> Neighborhood(FitContext ctx) =>
        Supervised(ctx).Map(y => {
            Matrix<double> x = ctx.Design.Features;
            EstimatorModel.Neighbors carrier = new(x, [.. y.Select(static v => (int)v)], ctx.Neighbors);
            double accuracy = Enumerable.Range(0, x.RowCount).Count(i => Vote(x.Row(i), carrier, exclude: i) == (int)y[i]) / (double)x.RowCount;
            return new FittedModel(ctx.Estimator, carrier, accuracy, 1.0 - accuracy, 0, true, ctx.Clock.GetCurrentInstant());
        });

    internal static Fin<FittedModel> GaussianBayes(FitContext ctx) =>
        Supervised(ctx).Map(y => {
            Matrix<double> x = ctx.Design.Features;
            int dim = x.ColumnCount;
            int[] classes = [.. y.Select(static v => (int)v).Distinct().Order()];
            Matrix<double> means = Matrix<double>.Build.Dense(classes.Length, dim);
            Matrix<double> variances = Matrix<double>.Build.Dense(classes.Length, dim);
            Vector<double> priors = Vector<double>.Build.Dense(classes.Length);
            for (int k = 0; k < classes.Length; k++) {
                int[] rows = [.. Enumerable.Range(0, x.RowCount).Where(i => (int)y[i] == classes[k])];
                priors[k] = rows.Length / (double)x.RowCount;
                for (int f = 0; f < dim; f++) {
                    (double Mean, double Variance) moment = rows.Select(i => x[i, f]).MeanVariance();
                    means[k, f] = moment.Mean;
                    variances[k, f] = Math.Max(VarianceFloor, moment.Variance);
                }
            }
            EstimatorModel.Bayes carrier = new(means, variances, priors);
            double accuracy = Enumerable.Range(0, x.RowCount).Count(i => Posterior(x.Row(i), carrier) == (int)y[i]) / (double)x.RowCount;
            return new FittedModel(ctx.Estimator, carrier, accuracy, 1.0 - accuracy, 1, true, ctx.Clock.GetCurrentInstant());
        });

    // --- [TEMPORAL] -----------------------------------------------------------------------

    // Pure AR(p): the lag design Y[t] = Σ φₖ·Y[t−k] solved through the dense-algebra thin-QR — the same closed-form route OLS rides, the AR coefficients its solution.
    internal static Fin<FittedModel> AutoRegress(FitContext ctx) {
        Vector<double> series = ctx.Design.Features.Column(0);
        int p = ctx.Temporal.Map(static spec => spec.History).IfNone(1), n = series.Count;
        Matrix<double> design = Matrix<double>.Build.Dense(n - p, p, (i, k) => series[p + i - 1 - k]);
        Vector<double> response = Vector<double>.Build.Dense(n - p, i => series[p + i]);
        return DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), design, response, TolerancePolicy.Derive(design, response))
            .Map(phi => {
                Vector<double> residual = response - design.Multiply(phi);
                double variance = residual.DotProduct(residual) / Math.Max(1, n - p);
                Vector<double> tail = Vector<double>.Build.Dense(p, k => series[n - 1 - k]);
                return new FittedModel(ctx.Estimator, new EstimatorModel.Lag(phi, Vector<double>.Build.Dense(0), variance, tail, TimeSeriesModel.Ar), -variance, Math.Sqrt(variance), 1, true, ctx.Clock.GetCurrentInstant());
            });
    }

    // ARMA minimizes conditional sum-of-squares through hyperdual `LevenbergMarquardt`; `GetGradient()` supplies its exact Jacobian.
    // Exponential smoothing and state space reuse that solver with distinct Holt-error and Kalman-innovation recurrences.
    internal static Fin<FittedModel> MovingAverage(FitContext ctx) {
        Vector<double> series = ctx.Design.Features.Column(0);
        int p = ctx.Temporal.Map(static spec => spec.History).IfNone(1), q = p, n = series.Count;
        if (n <= p + q) { return Fin.Fail<FittedModel>(ComputeFault.Create($"<arma-short-series:{n}>")); }
        return LevenbergMarquardt.Minimize(theta => ArmaResiduals(series, p, q, theta), Vector<double>.Build.Dense(p + q), LmPolicy.Canonical)
            .Map(lm => {
                Vector<double> resid = Primal(ArmaResiduals(series, p, q, Constants(lm.Parameters)));
                // Tail packs most-recent-first AR observations, then conditional MA residuals;
                // residual slots zero-fill past the warmup.
                Vector<double> tail = Vector<double>.Build.Dense(p + q, k =>
                    k < p ? series[n - 1 - k]
                    : resid.Count - 1 - (k - p) >= 0 ? resid[resid.Count - 1 - (k - p)] : 0.0);
                double variance = lm.Residual * lm.Residual / Math.Max(1, n - p - q);
                return new FittedModel(ctx.Estimator, new EstimatorModel.Lag(lm.Parameters.SubVector(0, p), lm.Parameters.SubVector(p, q), variance, tail, TimeSeriesModel.Arma), -variance, lm.Residual, lm.Iterations, lm.Converged, ctx.Clock.GetCurrentInstant());
            });
    }

    // Holt linear-trend exponential smoothing: a genuinely distinct level+trend recurrence (NOT an ARMA roll); (α, β) is logistic-reparametrized so LevenbergMarquardt searches ℝ² unconstrained while live rates stay in (0,1).
    // Carrier stores the realized (α, β) and the terminal (level, trend) the forecast extrapolates as level + h·trend.
    internal static Fin<FittedModel> Holt(FitContext ctx) {
        Vector<double> series = ctx.Design.Features.Column(0);
        int n = series.Count;
        if (n < 3) { return Fin.Fail<FittedModel>(ComputeFault.Create($"<holt-short-series:{n}>")); }
        return LevenbergMarquardt.Minimize(theta => HoltFilter(series, theta).Errors, Vector<double>.Build.DenseOfArray([0.0, -2.0]), LmPolicy.Canonical)
            .Map(lm => {
                (double alpha, double beta, double level, double trend) = HoltState(series, lm.Parameters);
                double variance = lm.Residual * lm.Residual / Math.Max(1, n - 2);
                return new FittedModel(ctx.Estimator, new EstimatorModel.Lag(Vector<double>.Build.DenseOfArray([alpha, beta]), Vector<double>.Build.Dense(0), variance, Vector<double>.Build.DenseOfArray([level, trend]), TimeSeriesModel.ExponentialSmoothing), -variance, lm.Residual, lm.Iterations, lm.Converged, ctx.Clock.GetCurrentInstant());
            });
    }

    // Local-linear-trend Kalman fitting log-parameterizes `(qLevel, qSlope)` and minimizes raw innovations; standardization by `√F` admits the `q→∞` degeneracy.
    // Carrier stores filtered terminal `(level, slope)` for forecast projection.
    internal static Fin<FittedModel> StateSpace(FitContext ctx) {
        Vector<double> series = ctx.Design.Features.Column(0);
        int n = series.Count;
        if (n < 4) { return Fin.Fail<FittedModel>(ComputeFault.Create($"<state-space-short-series:{n}>")); }
        return LevenbergMarquardt.Minimize(theta => StateSpaceFilter(series, theta).Innovations, Vector<double>.Build.DenseOfArray([0.0, -3.0]), LmPolicy.Canonical)
            .Map(lm => {
                (double level, double slope, double variance) = StateSpaceState(series, lm.Parameters);
                return new FittedModel(ctx.Estimator, new EstimatorModel.Lag(Vector<double>.Build.DenseOfArray([Math.Exp(lm.Parameters[0]), Math.Exp(lm.Parameters[1])]), Vector<double>.Build.Dense(0), variance, Vector<double>.Build.DenseOfArray([level, slope]), TimeSeriesModel.StateSpace), -variance, Math.Sqrt(variance), lm.Iterations, lm.Converged, ctx.Clock.GetCurrentInstant());
            });
    }

    internal static Fin<FittedModel> CusumBaseline(FitContext ctx) => DetectorBaseline(ctx, TimeSeriesModel.Cusum);

    internal static Fin<FittedModel> BayesianBaseline(FitContext ctx) => DetectorBaseline(ctx, TimeSeriesModel.BayesianOnline);

    internal static Fin<FittedModel> CorrelatedBaseline(FitContext ctx) => DetectorBaseline(ctx, TimeSeriesModel.CorrelatedResidual);

    private static Fin<FittedModel> DetectorBaseline(FitContext ctx, TimeSeriesModel model) =>
        ctx.Temporal.ToFin(ComputeFault.Create("<temporal-spec-missing>"))
            .Bind(spec => spec.Model != model
                ? Fin.Fail<FittedModel>(ComputeFault.Create($"<temporal-model-miss:{spec.Model.Key}!={model.Key}>"))
                : Baseline(ctx, spec));

    private static Fin<FittedModel> Baseline(FitContext ctx, TemporalSpec spec) {
        Matrix<double> x = ctx.Design.Features.SubMatrix(0, spec.History, 0, ctx.Design.Columns);
        Vector<double> mean = x.ColumnSums() / x.RowCount;
        Matrix<double> centered = Center(x, mean);
        double ridge = spec.Switch(
            ar: static _ => VarianceFloor, arma: static _ => VarianceFloor,
            exponentialSmoothing: static _ => VarianceFloor, stateSpace: static _ => VarianceFloor,
            cusum: static _ => VarianceFloor, bayesianOnline: static _ => VarianceFloor,
            correlatedResidual: static s => s.Ridge);
        Matrix<double> covariance = centered.TransposeThisAndMultiply(centered) / Math.Max(1, x.RowCount - 1) +
            Matrix<double>.Build.DiagonalIdentity(x.ColumnCount) * ridge;
        return Admission.Definite(covariance).Map(scale => {
            double nll = x.EnumerateRows().Average(row => -LogGaussian(row, mean, scale, x.ColumnCount));
            return new FittedModel(ctx.Estimator, new EstimatorModel.Detector(mean, scale, spec, ctx.Budget.MaxIterations), -nll, nll, 1, true, ctx.Clock.GetCurrentInstant());
        });
    }

    // --- [VALIDATION] ---------------------------------------------------------------------

    // Contiguous k-fold: fit on the complement, score the held-out fold by the family metric — R² for regression,
    // matched fraction for classification — so generalization is measured on rows the fit never saw.
    private static Fin<Seq<double>> HeldOut(Estimator estimator, Design design, EstimatorPolicy policy, int folds, IClock clock) =>
        toSeq(Enumerable.Range(0, folds)).TraverseM(fold => {
            (Design train, Design test) = design.Split(fold, folds);
            return Fit(estimator, train, policy, clock)
                .Bind(model => Predict(model, test.Features))
                .Bind(prediction => Scored(prediction, test.Targets, estimator));
        }).As();

    private static Fin<double> Scored(Prediction prediction, Option<Vector<double>> held, Estimator estimator) =>
        held.ToFin(ComputeFault.Create($"<validate-needs-targets:{estimator.Key}>")).Bind(actual => prediction.Switch(
            response: r => Fin.Succ(GoodnessOfFit.CoefficientOfDetermination(r.Values, actual)),
            projection: _ => Fin.Fail<double>(ComputeFault.Create($"<validate-projection-unscored:{estimator.Key}>")),
            assignment: a => Fin.Succ(Enumerable.Range(0, actual.Count).Count(i => a.Labels[i] == (int)actual[i]) / (double)actual.Count),
            anomaly: _ => Fin.Fail<double>(ComputeFault.Create($"<validate-anomaly-needs-event-labels:{estimator.Key}>"))));

    // Expanding-window folds fit each prefix and forecast the next block.
    // Negative RMSE preserves higher-is-better quality without future leakage.
    private static Fin<Seq<double>> ForwardChain(Estimator estimator, Design design, EstimatorPolicy policy, int folds, IClock clock) {
        Vector<double> series = design.Features.Column(0);
        int n = series.Count;
        return toSeq(Enumerable.Range(1, folds)).TraverseM(fold => {
            int cut = n * fold / (folds + 1), horizon = Math.Max(1, n * (fold + 1) / (folds + 1) - cut);
            return Design.Admit(Matrix<double>.Build.Dense(cut, 1, (i, _) => series[i]), None)
                .Bind(prefix => Fit(estimator, prefix, policy, clock))
                .Bind(model => Predict(model, Matrix<double>.Build.Dense(horizon, 1)))
                .Bind(prediction => prediction.Switch(
                    response: r => Fin.Succ(-Math.Sqrt(Enumerable.Range(0, Math.Min(r.Values.Count, n - cut)).Sum(h => Math.Pow(r.Values[h] - series[cut + h], 2)) / Math.Max(1, Math.Min(r.Values.Count, n - cut)))),
                    projection: static _ => Fin.Fail<double>(ComputeFault.Create("<validate-temporal-shape>")),
                    assignment: static _ => Fin.Fail<double>(ComputeFault.Create("<validate-temporal-shape>")),
                    anomaly: static _ => Fin.Fail<double>(ComputeFault.Create("<validate-temporal-shape>"))));
        }).As();
    }

    // --- [KERNELS] ------------------------------------------------------------------------

    private static Matrix<double> Intercept(Matrix<double> x) =>
        Matrix<double>.Build.Dense(x.RowCount, x.ColumnCount + 1, (i, j) => j == 0 ? 1.0 : x[i, j - 1]);

    private static Matrix<double> Tikhonov(int columns, double lambda) =>
        Matrix<double>.Build.Diagonal(columns, columns, j => j == 0 ? 0.0 : Math.Sqrt(lambda));

    private static (double Intercept, Vector<double> Slopes) Split(Vector<double> theta) =>
        (theta[0], theta.SubVector(1, theta.Count - 1));

    private static Matrix<double> Center(Matrix<double> x, Vector<double> mean) =>
        Matrix<double>.Build.Dense(x.RowCount, x.ColumnCount, (i, j) => x[i, j] - mean[j]);

    private static double Distance(Vector<double> left, Vector<double> right) =>
        TensorPrimitives.Distance<double>(left.ToArray(), right.ToArray());

    private static int Retain(Vector<double> singular, double total, int cap, double fraction) {
        double cumulative = 0.0;
        for (int k = 0; k < cap; k++) {
            cumulative += singular[k] * singular[k];
            if (total > 0.0 && cumulative / total >= fraction) { return k + 1; }
        }
        return Math.Max(1, cap);
    }

    // Farthest-first traversal anchors row `0`; each next centroid maximizes distance from its nearest chosen row.
    private static Matrix<double> Seed(Matrix<double> x, int k) {
        int[] chosen = new int[k];
        double[] nearest = new double[x.RowCount];
        Array.Fill(nearest, double.MaxValue);
        for (int c = 1; c < k; c++) {
            for (int i = 0; i < x.RowCount; i++) { nearest[i] = Math.Min(nearest[i], Distance(x.Row(i), x.Row(chosen[c - 1]))); }
            chosen[c] = Enumerable.Range(0, x.RowCount).MaxBy(i => nearest[i]);
        }
        return Matrix<double>.Build.DenseOfRowVectors([.. chosen.Select(x.Row)]);
    }

    private static int Nearest(Vector<double> point, Matrix<double> centroids) =>
        Enumerable.Range(0, centroids.RowCount).MinBy(c => Distance(point, centroids.Row(c)));

    // Emptied clusters keep their prior centroid — an origin-snapped centroid corrupts inertia and every later assignment.
    private static Matrix<double> Recenter(Matrix<double> x, int[] labels, int k, Matrix<double> prior) {
        Matrix<double> sums = Matrix<double>.Build.Dense(k, x.ColumnCount);
        int[] counts = new int[k];
        for (int i = 0; i < x.RowCount; i++) {
            if (labels[i] < 0 || labels[i] >= k) { continue; }
            sums.SetRow(labels[i], sums.Row(labels[i]) + x.Row(i));
            counts[labels[i]]++;
        }
        return Matrix<double>.Build.Dense(k, x.ColumnCount, (c, j) => counts[c] > 0 ? sums[c, j] / counts[c] : prior[c, j]);
    }

    private static double Inertia(Matrix<double> x, int[] labels, Matrix<double> centroids) =>
        Enumerable.Range(0, x.RowCount).Where(i => labels[i] >= 0 && labels[i] < centroids.RowCount).Sum(i => (x.Row(i) - centroids.Row(labels[i])).PointwisePower(2.0).Sum());

    private static int[] Region(Matrix<double> x, int point, double eps) =>
        [.. Enumerable.Range(0, x.RowCount).Where(i => Distance(x.Row(point), x.Row(i)) <= eps)];

    private static int DensityAssign(Vector<double> point, EstimatorModel.Density density) =>
        Enumerable.Range(0, density.Training.RowCount)
            .Where(i => density.Core[i] && density.Labels[i] >= 0)
            .Select(i => (Index: i, Distance: Distance(point, density.Training.Row(i))))
            .Where(row => row.Distance <= density.Radius)
            .OrderBy(static row => row.Distance)
            .Select(row => density.Labels[row.Index])
            .DefaultIfEmpty(-1)
            .First();

    private static double AverageLinkage(Matrix<double> x, List<int> a, List<int> b) =>
        a.Sum(i => b.Sum(j => Distance(x.Row(i), x.Row(j)))) / (a.Count * b.Count);

    private static Matrix<double> Gram(Matrix<double> left, Matrix<double> right, double bandwidth) =>
        Matrix<double>.Build.Dense(left.RowCount, right.RowCount, (i, j) => Math.Exp(-(left.Row(i) - right.Row(j)).PointwisePower(2.0).Sum() / (2.0 * bandwidth * bandwidth)));

    private static Fin<Cholesky<double>[]> Choleskies(Seq<Matrix<double>> covariances, double ridge) =>
        covariances.TraverseM(covariance =>
            Admission.Definite(covariance + Matrix<double>.Build.DiagonalIdentity(covariance.RowCount) * ridge)).As()
        .Map(static factors => factors.ToArray());

    private static double LogGaussian(Vector<double> x, Vector<double> mean, Cholesky<double> chol, int dim) {
        return -0.5 * (dim * Math.Log(2.0 * Math.PI) + chol.DeterminantLn + Mahalanobis(x, mean, chol));
    }

    private static double PredictiveLogGaussian(
        Vector<double> value, Vector<double> mean, Cholesky<double> scale, int dimensions, double meanPrecision) {
        double inflation = 1.0 + 1.0 / meanPrecision;
        return -0.5 * (dimensions * Math.Log(2.0 * Math.PI) + scale.DeterminantLn + dimensions * Math.Log(inflation) +
            Mahalanobis(value, mean, scale) / inflation);
    }

    private static double Mahalanobis(Vector<double> value, Vector<double> mean, Cholesky<double> scale) {
        Vector<double> delta = value - mean;
        return delta.DotProduct(scale.Solve(delta));
    }

    private static Matrix<double> WeightedCovariance(Matrix<double> x, Vector<double> gamma, Vector<double> mean, double mass, double ridge) {
        Matrix<double> accumulator = Matrix<double>.Build.Dense(x.ColumnCount, x.ColumnCount);
        for (int i = 0; i < x.RowCount; i++) {
            Vector<double> delta = x.Row(i) - mean;
            accumulator += gamma[i] * delta.OuterProduct(delta);
        }
        return accumulator / Math.Max(1e-9, mass) + Matrix<double>.Build.DiagonalIdentity(x.ColumnCount) * ridge;
    }

    private static Fin<ImmutableArray<int>> Responsibilities(Matrix<double> x, EstimatorModel.Mixture mixture) =>
        Choleskies(mixture.Covariances, 1e-9).Map(chols =>
            [.. x.EnumerateRows().Select(row =>
                Enumerable.Range(0, mixture.Weights.Count)
                    .MaxBy(j => Math.Log(Math.Max(1e-300, mixture.Weights[j])) + LogGaussian(row, mixture.Means.Row(j), chols[j], x.ColumnCount)))]);

    private static double Decision(Vector<double> point, Matrix<double> training, Vector<double> duals, double bias, double bandwidth) =>
        Enumerable.Range(0, training.RowCount).Sum(i => duals[i] * Math.Exp(-(point - training.Row(i)).PointwisePower(2.0).Sum() / (2.0 * bandwidth * bandwidth))) + bias;

    private static int Strongest(Vector<double> point, EstimatorModel.Margin margin) =>
        margin.Machines.Map(machine => (machine.Label, Score: Decision(point, margin.Training, machine.Duals, machine.Bias, margin.Bandwidth)))
            .MaxBy(static row => row.Score).Label;

    // Distance-weighted vote (1/d weight, tie-broken by mass); `exclude` carves the row itself out for the
    // leave-one-out training score — a uniform vote over-counts distant neighbors on skewed densities.
    private static int Vote(Vector<double> point, EstimatorModel.Neighbors neighbors, int exclude = -1) =>
        Enumerable.Range(0, neighbors.Design.RowCount)
            .Where(i => i != exclude)
            .Select(i => (Label: neighbors.Labels[i], Distance: Distance(point, neighbors.Design.Row(i))))
            .OrderBy(static row => row.Distance)
            .Take(neighbors.K)
            .GroupBy(static row => row.Label)
            .OrderByDescending(g => g.Sum(static row => 1.0 / Math.Max(1e-12, row.Distance)))
            .First().Key;

    private static int Posterior(Vector<double> point, EstimatorModel.Bayes bayes) =>
        Enumerable.Range(0, bayes.Priors.Count)
            .MaxBy(k => Math.Log(Math.Max(1e-300, bayes.Priors[k])) +
                Enumerable.Range(0, point.Count).Sum(f => -0.5 * (Math.Log(2.0 * Math.PI * bayes.Variances[k, f]) + (point[f] - bayes.Means[k, f]) * (point[f] - bayes.Means[k, f]) / bayes.Variances[k, f])));

    private static Matrix<double> KernelProject(Matrix<double> x, EstimatorModel.KernelBasis basis) {
        Matrix<double> gram = Gram(x, basis.Training, basis.Bandwidth);
        Vector<double> testRowMean = gram.RowSums() / basis.Training.RowCount;
        Matrix<double> centered = Matrix<double>.Build.Dense(x.RowCount, basis.Training.RowCount,
            (i, j) => gram[i, j] - testRowMean[i] - basis.RowMean[j] + basis.GrandMean);
        return centered.Multiply(basis.Alphas);
    }

    // NMF transform of new rows given components H: one non-negative projection step per component (full NNLS is the noted refinement); the encoding stays W ≥ 0.
    private static Matrix<double> NonNegativeEncode(Matrix<double> x, Matrix<double> components) =>
        Matrix<double>.Build.Dense(x.RowCount, components.RowCount, (i, k) =>
            Math.Max(0.0, x.Row(i).DotProduct(components.Row(k)) / Math.Max(1e-12, components.Row(k).DotProduct(components.Row(k)))));

    private static Fin<Prediction> Detect(EstimatorModel.Detector detector, Matrix<double> evidence) =>
        evidence.RowCount < 1 || evidence.ColumnCount != detector.Mean.Count
            ? Fin.Fail<Prediction>(ComputeFault.Create($"<detect-shape:{evidence.RowCount}x{evidence.ColumnCount}:expected={detector.Mean.Count}>"))
        : !TensorPrimitives.IsFiniteAll<double>(evidence.AsColumnMajorArray() ?? evidence.ToColumnMajorArray())
            ? Fin.Fail<Prediction>(ComputeFault.Create("<detect-nonfinite>"))
        : detector.Spec.Switch<Matrix<double>, Fin<Prediction>>(
            state: evidence,
            ar: static (_, _) => Fin.Fail<Prediction>(ComputeFault.Create("<detect-forecast-carrier>")),
            arma: static (_, _) => Fin.Fail<Prediction>(ComputeFault.Create("<detect-forecast-carrier>")),
            exponentialSmoothing: static (_, _) => Fin.Fail<Prediction>(ComputeFault.Create("<detect-forecast-carrier>")),
            stateSpace: static (_, _) => Fin.Fail<Prediction>(ComputeFault.Create("<detect-forecast-carrier>")),
            cusum: (x, spec) => Fin.Succ<Prediction>(Cusum(detector, x, spec)),
            bayesianOnline: (x, spec) => Fin.Succ<Prediction>(BayesianOnline(detector, x, spec)),
            correlatedResidual: (x, spec) => Fin.Succ<Prediction>(CorrelatedResidual(detector, x, spec)));

    private static Prediction.Anomaly Cusum(EstimatorModel.Detector detector, Matrix<double> evidence, TemporalSpec.Cusum spec) {
        double accumulator = 0.0;
        Vector<double> scores = Vector<double>.Build.Dense(evidence.RowCount);
        bool[] changes = new bool[evidence.RowCount];
        for (int i = 0; i < evidence.RowCount; i++) {
            double innovation = Math.Sqrt(Math.Max(0.0, Mahalanobis(evidence.Row(i), detector.Mean, detector.Scale)));
            accumulator = Math.Max(0.0, accumulator + innovation - spec.Drift);
            scores[i] = accumulator;
            changes[i] = accumulator >= spec.Threshold;
            if (changes[i]) { accumulator = 0.0; }
        }
        return new Prediction.Anomaly(scores, [.. changes]);
    }

    private static Prediction.Anomaly BayesianOnline(EstimatorModel.Detector detector, Matrix<double> evidence, TemporalSpec.BayesianOnline spec) {
        int dimensions = evidence.ColumnCount;
        Vector<double> posterior = Vector<double>.Build.DenseOfArray([1.0]);
        Matrix<double> means = Matrix<double>.Build.DenseOfRowVectors([detector.Mean]);
        Vector<double> scores = Vector<double>.Build.Dense(evidence.RowCount);
        bool[] changes = new bool[evidence.RowCount];
        for (int i = 0; i < evidence.RowCount; i++) {
            Vector<double> row = evidence.Row(i);
            double changeLog = Math.Log(spec.Hazard) + PredictiveLogGaussian(row, detector.Mean, detector.Scale, dimensions, meanPrecision: 1.0);
            double[] growthLog = [.. Enumerable.Range(0, posterior.Count).Select(r =>
                Math.Log(Math.Max(1e-300, posterior[r])) + Math.Log(1.0 - spec.Hazard) +
                PredictiveLogGaussian(row, means.Row(r), detector.Scale, dimensions, meanPrecision: r + 1.0))];
            double maximum = Math.Max(changeLog, growthLog.Max());
            double denominator = Math.Exp(changeLog - maximum) + growthLog.Sum(log => Math.Exp(log - maximum));
            int nextLength = Math.Min(posterior.Count + 1, detector.MaxRunLength);
            double[] next = new double[nextLength];
            next[0] = Math.Exp(changeLog - maximum) / denominator;
            scores[i] = next[0];
            changes[i] = next[0] >= spec.Threshold;
            Matrix<double> updated = Matrix<double>.Build.Dense(nextLength, dimensions);
            updated.SetRow(0, (detector.Mean + row) / 2.0);
            double[] mass = new double[nextLength];
            for (int r = 0; r < posterior.Count; r++) {
                double precision = r + 1.0;
                double probability = Math.Exp(growthLog[r] - maximum) / denominator;
                int target = Math.Min(r + 1, nextLength - 1);
                next[target] += probability;
                mass[target] += probability;
                Vector<double> candidate = (means.Row(r) * precision + row) / (precision + 1.0);
                updated.SetRow(target, updated.Row(target) + candidate * probability);
            }
            for (int r = 1; r < nextLength; r++) { updated.SetRow(r, updated.Row(r) / Math.Max(1e-300, mass[r])); }
            posterior = Vector<double>.Build.DenseOfArray(next);
            means = updated;
        }
        return new Prediction.Anomaly(scores, [.. changes]);
    }

    private static Prediction.Anomaly CorrelatedResidual(
        EstimatorModel.Detector detector, Matrix<double> evidence, TemporalSpec.CorrelatedResidual spec) {
        double threshold = ChiSquared.InvCDF(evidence.ColumnCount, 1.0 - spec.FalsePositiveRate);
        Vector<double> scores = Vector<double>.Build.Dense(evidence.RowCount, i => Mahalanobis(evidence.Row(i), detector.Mean, detector.Scale));
        return new Prediction.Anomaly(scores, [.. scores.Select(score => score >= threshold)]);
    }

    // Model-aware forecast: the Lag.Model tag routes extrapolation — AR(+MA) roll for the lag-regression families, level+trend line for Holt, level+slope line for the local-trend SSM.
    private static Vector<double> Forecast(EstimatorModel.Lag lag, int horizon) =>
        lag.Model == TimeSeriesModel.ExponentialSmoothing ? HoltForecast(lag, horizon)
        : lag.Model == TimeSeriesModel.StateSpace ? StateSpaceForecast(lag, horizon)
        : ArmaForecast(lag, horizon);

    // AR(+MA) roll: ŷ[T+h] = Σφₖ·ŷ[T+h−1−k] + Σψₖ·ê[T+h−1−k]; a future shock has zero expectation, so the MA term decays to zero past q steps while the AR feedback continues from the rolled forecasts.
    // Pure-AR (q=0) skips the residual loop — one roll serving both lag-regression rows.
    private static Vector<double> ArmaForecast(EstimatorModel.Lag lag, int horizon) {
        int p = lag.ArCoefficients.Count, q = lag.MaCoefficients.Count;
        double[] obs = new double[p];
        for (int k = 0; k < p; k++) { obs[k] = lag.Tail[k]; }
        double[] res = new double[q];
        for (int k = 0; k < q; k++) { res[k] = lag.Tail.Count > p + k ? lag.Tail[p + k] : 0.0; }
        double[] forecast = new double[Math.Max(1, horizon)];
        for (int h = 0; h < forecast.Length; h++) {
            double next = 0.0;
            for (int k = 0; k < p; k++) { next += lag.ArCoefficients[k] * obs[k]; }
            for (int k = 0; k < q; k++) { next += lag.MaCoefficients[k] * res[k]; }
            forecast[h] = next;
            for (int k = p - 1; k > 0; k--) { obs[k] = obs[k - 1]; }
            if (p > 0) { obs[0] = next; }
            for (int k = q - 1; k > 0; k--) { res[k] = res[k - 1]; }
            if (q > 0) { res[0] = 0.0; }
        }
        return Vector<double>.Build.DenseOfArray(forecast);
    }

    private static Vector<double> HoltForecast(EstimatorModel.Lag lag, int horizon) =>
        Vector<double>.Build.Dense(Math.Max(1, horizon), h => lag.Tail[0] + (h + 1) * lag.Tail[1]);

    private static Vector<double> StateSpaceForecast(EstimatorModel.Lag lag, int horizon) =>
        Vector<double>.Build.Dense(Math.Max(1, horizon), h => lag.Tail[0] + (h + 1) * lag.Tail[1]);

    // Holt one-step level+trend recurrence; (α, β) arrive logistic-mapped from the LM iterate so live rates stay in (0,1).
    // Authored ONCE over the HyperJet DDScalar — the LM hyperdual arm reads the EXACT Jacobian through GetGradient(), the post-fit read the same recursion seeded with constants, zero finite differences.
    private static (DDScalar[] Errors, DDScalar Level, DDScalar Trend) HoltFilter(Vector<double> series, DDScalar[] theta) {
        DDScalar alpha = Logistic(theta[0]), beta = Logistic(theta[1]);
        int n = series.Count;
        DDScalar level = theta[0] * 0.0 + series[0], trend = theta[0] * 0.0 + (series[1] - series[0]);
        DDScalar[] errors = new DDScalar[n - 1];
        for (int i = 1; i < n; i++) {
            DDScalar fitted = level + trend, e = series[i] - fitted;
            errors[i - 1] = e;
            level = fitted + alpha * e;
            trend += alpha * beta * e;
        }
        return (errors, level, trend);
    }

    private static (double Alpha, double Beta, double Level, double Trend) HoltState(Vector<double> series, Vector<double> theta) {
        (DDScalar[] Errors, DDScalar Level, DDScalar Trend) run = HoltFilter(series, Constants(theta));
        return (Logistic(Constants(theta)[0]).Value, Logistic(Constants(theta)[1]).Value, run.Level.Value, run.Trend.Value);
    }

    private static (double Level, double Slope, double Variance) StateSpaceState(Vector<double> series, Vector<double> theta) {
        (DDScalar[] Innovations, DDScalar Level, DDScalar Slope) run = StateSpaceFilter(series, Constants(theta));
        double[] innovations = [.. run.Innovations.Select(static v => v.Value)];
        double variance = innovations.Length > 0 ? innovations.Sum(static v => v * v) / innovations.Length : 0.0;
        return (run.Level.Value, run.Slope.Value, variance);
    }

    // 2-state (level, slope) local-linear-trend Kalman filter: transition T=[[1,1],[0,1]], observation H=[1,0] with unit measurement variance, diffuse covariance start, process variances (qLevel, qSlope); the raw innovation v feeds the LM prediction-error fit.
    // Authored ONCE over the DDScalar so the innovation Jacobian is exact THROUGH the filter recursion — the covariance/gain arithmetic differentiates too, the algorithmic derivative the FD probe could only approximate.
    private static (DDScalar[] Innovations, DDScalar Level, DDScalar Slope) StateSpaceFilter(Vector<double> series, DDScalar[] theta) {
        DDScalar qLevel = HyperJetMath.Exp(theta[0]), qSlope = HyperJetMath.Exp(theta[1]);
        int n = series.Count;
        DDScalar a0 = theta[0] * 0.0 + series[0], a1 = theta[0] * 0.0 + (series[1] - series[0]);
        DDScalar p00 = theta[0] * 0.0 + 1e3, p01 = theta[0] * 0.0, p11 = theta[0] * 0.0 + 1e3;
        DDScalar[] innov = new DDScalar[n - 1];
        for (int t = 1; t < n; t++) {
            DDScalar pred = a0 + a1;
            DDScalar m00 = p00 + 2.0 * p01 + p11 + qLevel, m01 = p01 + p11, m11 = p11 + qSlope;
            DDScalar f = m00 + 1.0, v = series[t] - pred;
            innov[t - 1] = v;
            DDScalar k0 = m00 / f, k1 = m01 / f;
            a0 = pred + k0 * v;
            a1 += k1 * v;
            p00 = m00 - k0 * m00;
            p01 = m01 - k0 * m01;
            p11 = m11 - k1 * m01;
        }
        return (innov, a0, a1);
    }

    private static DDScalar Logistic(DDScalar z) => 1.0 / (1.0 + HyperJetMath.Exp(-z));

    // Conditional residual recursion r[t] = y[t] − (Σφₖ·y[t−1−k] + Σψₖ·r[t−1−k]); residuals before the max(p,q) warmup are zero, the standard conditional-sum-of-squares start.
    // Authored ONCE over DDScalar so the LM gradient is machine-exact — no finite-difference probe exists on this rail.
    private static DDScalar[] ArmaResiduals(Vector<double> series, int p, int q, DDScalar[] theta) {
        int n = series.Count, warmup = Math.Max(p, q);
        DDScalar zero = theta[0] * 0.0;
        DDScalar[] residuals = new DDScalar[n];
        for (int t = 0; t < warmup; t++) { residuals[t] = zero; }
        for (int t = warmup; t < n; t++) {
            DDScalar predicted = zero;
            for (int k = 0; k < p; k++) { predicted += theta[k] * series[t - 1 - k]; }
            for (int k = 0; k < q; k++) { predicted += theta[p + k] * residuals[t - 1 - k]; }
            residuals[t] = series[t] - predicted;
        }
        return residuals[warmup..];
    }

    // Constant seeding for post-fit reads (gradient-free, order-1-compatible) and the primal projection back onto the MathNet vector the model carriers store.
    private static DDScalar[] Constants(Vector<double> theta) => [.. theta.Select(static v => DDScalar.Constant(v, 0, order: 1))];

    private static Vector<double> Primal(DDScalar[] values) => Vector<double>.Build.Dense([.. values.Select(static v => v.Value)]);
}
```

### [02.1]-[HYPOTHESIS_LAW]

- Owner: `StatisticalTest` `[SmartEnum<string>]` rows each bind ONE `Evaluate` kernel computing the statistic, the (possibly fractional) dof, and the p-value from its own tail — collapsing a parallel `Statistic`/`PValue`/`dof` helper trio into row data — plus the row's `MinSamples` arity floor, so a two-sample kernel can never receive one sample; `TestResult` carries (statistic, p-value, decision, dof); `Hypothesis` is the static `Test` surface stamping through the injected `IClock`.
- Cases: `StatisticalTest` t · welch-t · anova · chi-square · ks · mann-whitney.
- Entry: `Hypothesis.Test(StatisticalTest test, Seq<ReadOnlyMemory<double>> samples, double alpha, IClock clock)` admits `0 < alpha < 1`, row arity, finite samples, and the row's support domain before computing the statistic and matching `Distributions` CDF tail. `IClock.GetCurrentInstant()` stamps the result.
- Auto: pooled-variance `t` and Welch `welch-t` read a two-sided `StudentT.CDF` tail (Welch carrying the Welch–Satterthwaite fractional dof); `anova` reads the upper `FisherSnedecor.CDF` tail over the one-way F; `chi-square` reads the upper `ChiSquared.CDF` tail over Σ(O−E)²/E; `ks` reads the two-sample sup-distance against the Kolmogorov series `Q(λ)=2·Σ(−1)^{j−1}·e^{−2j²λ²}` (no MathNet CDF exists); `mann-whitney` reads a tie-corrected, continuity-corrected `Normal.CDF` approximation over the rank-sum U.
- Boundary: each row owns its complete kernel, so a hand-derived error function beside the `Distributions` CDF is the deleted form for t/anova/chi-square/mann-whitney, while the Kolmogorov series and the rank-sum computation are this page's statement-exemption kernels (as the signal lane's direct-form recurrence is exempt); tail direction is row policy (two-sided for t/welch/mann-whitney, upper for anova/chi-square, Kolmogorov complement for ks), so a fixed one-sided tail across the family is the deleted form; a sample-arity break — a two-sample statistic reading `samples[1]` behind a bare non-empty guard — is the deleted gate; the tests validate `Solver/uncertainty#UNCERTAINTY_LANE` response samples without re-sampling.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StatisticalTest {
    public static readonly StatisticalTest Student = new("t", minSamples: 2, StudentPooled, AnySamples);
    public static readonly StatisticalTest WelchT = new("welch-t", minSamples: 2, Welch, AnySamples);
    public static readonly StatisticalTest Anova = new("anova", minSamples: 2, OneWayAnova, AnySamples);
    public static readonly StatisticalTest ChiSquare = new("chi-square", minSamples: 1, ChiSquareGoodness, ChiSquareSamples);
    public static readonly StatisticalTest Ks = new("ks", minSamples: 2, KolmogorovSmirnov, AnySamples);
    public static readonly StatisticalTest MannWhitney = new("mann-whitney", minSamples: 2, RankSum, AnySamples);

    private StatisticalTest(
        string key, int minSamples, Func<Seq<ReadOnlyMemory<double>>, (double Statistic, double PValue, double Dof)> evaluate,
        Func<Seq<ReadOnlyMemory<double>>, Fin<Unit>> admit) : this(key) {
        MinSamples = minSamples;
        this.evaluate = evaluate;
        this.admit = admit;
    }

    private readonly Func<Seq<ReadOnlyMemory<double>>, (double Statistic, double PValue, double Dof)> evaluate;
    private readonly Func<Seq<ReadOnlyMemory<double>>, Fin<Unit>> admit;

    public int MinSamples { get; }

    internal (double Statistic, double PValue, double Dof) Evaluate(Seq<ReadOnlyMemory<double>> samples) => evaluate(samples);
    internal Fin<Unit> Admit(Seq<ReadOnlyMemory<double>> samples) => admit(samples);

    private static (double, double, double) StudentPooled(Seq<ReadOnlyMemory<double>> samples) {
        (double MeanA, double VarianceA, int CountA, double MeanB, double VarianceB, int CountB) pair = TwoSampleMoments(samples);
        double dof = pair.CountA + pair.CountB - 2;
        double pooled = ((pair.CountA - 1) * pair.VarianceA + (pair.CountB - 1) * pair.VarianceB) / dof;
        double standardError = Math.Sqrt(pooled) * Math.Sqrt(1.0 / pair.CountA + 1.0 / pair.CountB);
        return StudentTail(pair.MeanA - pair.MeanB, standardError, dof);
    }

    private static (double, double, double) Welch(Seq<ReadOnlyMemory<double>> samples) {
        (double MeanA, double VarianceA, int CountA, double MeanB, double VarianceB, int CountB) pair = TwoSampleMoments(samples);
        double scaledA = pair.VarianceA / pair.CountA, scaledB = pair.VarianceB / pair.CountB;
        double dof = Math.Pow(scaledA + scaledB, 2) /
            (scaledA * scaledA / (pair.CountA - 1) + scaledB * scaledB / (pair.CountB - 1));
        return StudentTail(pair.MeanA - pair.MeanB, Math.Sqrt(scaledA + scaledB), dof);
    }

    private static (double MeanA, double VarianceA, int CountA, double MeanB, double VarianceB, int CountB) TwoSampleMoments(
        Seq<ReadOnlyMemory<double>> samples) {
        double[] a = samples[0].ToArray(), b = samples[1].ToArray();
        (double ma, double va) = a.MeanVariance();
        (double mb, double vb) = b.MeanVariance();
        return (ma, va, a.Length, mb, vb, b.Length);
    }

    private static (double, double, double) StudentTail(double difference, double standardError, double dof) {
        double t = difference / standardError;
        return (t, 2.0 * (1.0 - StudentT.CDF(0.0, 1.0, dof, Math.Abs(t))), dof);
    }

    private static Fin<Unit> AnySamples(Seq<ReadOnlyMemory<double>> _) => Fin.Succ(unit);

    private static Fin<Unit> ChiSquareSamples(Seq<ReadOnlyMemory<double>> samples) {
        double[] observed = samples[0].ToArray();
        double[] expected = samples.Count > 1 ? samples[1].ToArray() : [];
        bool observedValid = observed.All(static value => value >= 0.0) && TensorPrimitives.Sum<double>(observed) > 0.0;
        bool expectedValid = expected.Length == 0 ||
            (expected.Length == observed.Length && expected.All(static value => value > 0.0) &&
             Math.Abs(TensorPrimitives.Sum<double>(expected) - TensorPrimitives.Sum<double>(observed)) <= 1e-9 * TensorPrimitives.Sum<double>(observed));
        return observedValid && expectedValid
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComputeFault.Create("<chi-square-support>"));
    }

    private static (double, double, double) OneWayAnova(Seq<ReadOnlyMemory<double>> samples) {
        double[][] groups = [.. samples.Map(static g => g.ToArray())];
        int total = groups.Sum(static g => g.Length), k = groups.Length;
        double grand = groups.SelectMany(static g => g).Mean();
        double between = groups.Sum(g => g.Length * Math.Pow(g.Mean() - grand, 2)) / (k - 1);
        double within = groups.Sum(g => g.Sum(v => Math.Pow(v - g.Mean(), 2))) / (total - k);
        double f = between / within;
        return (f, 1.0 - FisherSnedecor.CDF(k - 1, total - k, f), k - 1);
    }

    private static (double, double, double) ChiSquareGoodness(Seq<ReadOnlyMemory<double>> samples) {
        double[] observed = samples[0].ToArray();
        double[] expected = samples.Count > 1 ? samples[1].ToArray() : [.. observed.Select(_ => observed.Average())];
        double chi = Enumerable.Range(0, observed.Length).Sum(i => Math.Pow(observed[i] - expected[i], 2) / Math.Max(1e-12, expected[i]));
        double dof = observed.Length - 1;
        return (chi, 1.0 - ChiSquared.CDF(dof, chi), dof);
    }

    // Two-sample Kolmogorov–Smirnov: sup gap of the empirical CDFs against the Kolmogorov asymptotic series — MathNet ships no Kolmogorov distribution, so the alternating exponential series is the kernel.
    private static (double, double, double) KolmogorovSmirnov(Seq<ReadOnlyMemory<double>> samples) {
        double[] a = [.. samples[0].ToArray().Order()], b = [.. samples[1].ToArray().Order()];
        int na = a.Length, nb = b.Length, i = 0, j = 0;
        double d = 0.0;
        while (i < na && j < nb) {
            double value = Math.Min(a[i], b[j]);
            while (i < na && a[i] <= value) { i++; }
            while (j < nb && b[j] <= value) { j++; }
            d = Math.Max(d, Math.Abs((double)i / na - (double)j / nb));
        }
        double ne = (double)na * nb / (na + nb);
        double lambda = (Math.Sqrt(ne) + 0.12 + 0.11 / Math.Sqrt(ne)) * d;
        double p = 2.0 * Enumerable.Range(1, 100).Sum(t => Math.Pow(-1.0, t - 1) * Math.Exp(-2.0 * t * t * lambda * lambda));
        return (d, Math.Clamp(p, 0.0, 1.0), 0.0);
    }

    // Mann–Whitney U via the tie-corrected, continuity-corrected normal approximation over the rank sum.
    private static (double, double, double) RankSum(Seq<ReadOnlyMemory<double>> samples) {
        double[] a = samples[0].ToArray(), b = samples[1].ToArray();
        int na = a.Length, nb = b.Length;
        double[] pooled = [.. a.Concat(b)];
        double[] ranks = Ranks(pooled);
        double rankSumA = ranks.Take(na).Sum();
        double u = rankSumA - na * (na + 1) / 2.0;
        double mean = na * (double)nb / 2.0;
        double tieTerm = pooled.GroupBy(static v => v).Sum(g => (double)g.Count() * g.Count() * g.Count() - g.Count());
        int n = na + nb;
        double sigma = Math.Sqrt(na * (double)nb / 12.0 * (n + 1 - tieTerm / (n * (n - 1.0))));
        double z = (u - mean - 0.5 * Math.Sign(u - mean)) / sigma;
        return (u, 2.0 * (1.0 - Normal.CDF(0.0, 1.0, Math.Abs(z))), 0.0);
    }

    private static double[] Ranks(double[] values) {
        int[] order = [.. Enumerable.Range(0, values.Length).OrderBy(i => values[i])];
        double[] ranks = new double[values.Length];
        for (int i = 0; i < order.Length;) {
            int j = i;
            while (j < order.Length && values[order[j]] == values[order[i]]) { j++; }
            double average = (i + j + 1) / 2.0;
            for (int t = i; t < j; t++) { ranks[order[t]] = average; }
            i = j;
        }
        return ranks;
    }
}

public sealed record TestResult(StatisticalTest Test, double Statistic, double PValue, bool RejectNull, double Dof, Instant At);

public static class Hypothesis {
    public static Fin<TestResult> Test(StatisticalTest test, Seq<ReadOnlyMemory<double>> samples, double alpha, IClock clock) =>
        !double.IsFinite(alpha) || alpha <= 0.0 || alpha >= 1.0
            ? Fin.Fail<TestResult>(ComputeFault.Create($"<hypothesis-alpha:{alpha}>"))
        : samples.Count < test.MinSamples
            ? Fin.Fail<TestResult>(ComputeFault.Create($"<hypothesis-samples:{samples.Count}<{test.MinSamples}:{test.Key}>"))
        : samples.Exists(static s => s.Length < 2 || !TensorPrimitives.IsFiniteAll<double>(s.Span))
            ? Fin.Fail<TestResult>(ComputeFault.Create($"<hypothesis-sample-admission:{test.Key}>"))
        : test.Admit(samples).Bind(_ => Evaluated(test, samples, alpha, clock));

    private static Fin<TestResult> Evaluated(StatisticalTest test, Seq<ReadOnlyMemory<double>> samples, double alpha, IClock clock) {
        (double statistic, double pValue, double dof) = test.Evaluate(samples);
        return double.IsFinite(statistic) && double.IsFinite(pValue)
            ? Fin.Succ(new TestResult(test, statistic, pValue, pValue < alpha, dof, clock.GetCurrentInstant()))
            : Fin.Fail<TestResult>(new ComputeFault.ModelRejected($"<hypothesis-nonfinite:{test.Key}:stat={statistic}>"));
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [GLM_DEVIANCE]-[OPEN]: exact family deviance over the `MathNet.Numerics.Distributions` `Density`/`CumulativeDistribution` normalizing constant beyond the quasi-likelihood Pearson dispersion, plus the canonical inverse-link Gamma path (`LinkFunction.Inverse` via the `Estimator.Regression.Link` override) as the `−log(−η)−y·η` form where positivity holds; verify against the `MathNet.Numerics.Distributions` surface.
- [LS_SVM_VS_DUAL]-[OPEN]: box-constrained C-SVM dual (hinge/quadratic-margin under `0 ≤ α ≤ C`, `Σαᵢyᵢ = 0`) trading the one-vs-rest LS-SVM dense closed-form for a sparse support set where hard-margin sparsity is required; verify an SMO/projected-gradient kernel with the `Admission.Definite` Cholesky conditioning gate.
- [GRADUATION_MODELS]-[BLOCKED]: offline deep-training model signature (input/output tensor names, feature interleave) the `ONE_GRADUATION_EVIDENCE` graduation-decode gate reads; verify against the Python `compute/graduation` companion-published signature.
