# [COMPUTE_ESTIMATOR]

Rasm.Compute statistical-learning lane: one `Estimator` `[Union]` carrying a uniform `Fit(Estimator, Design, EstimatorPolicy, IClock) → FittedModel` / `Predict(FittedModel, PredictQuery) → Prediction` contract across regression, reduction, clustering, classification, forecasting, changepoint, and anomaly families, keyed to one `EstimatorModel` fit-result carrier. Contract stays uniform while every row owns its own mechanism, which `[02]` fixes row by row. `Design.Admit` proves raw evidence once; row admission then proves response support, feature, label, history, curve-support, detector range, kernel parameter, and row-count ceiling.

Dense factorizations ride `Tensor/blas#DENSE_ALGEBRA`; descriptive, regression, and distribution surfaces ride `MathNet.Numerics`; criterion sums accumulate exactly through `PeterO.Numbers`; `ComputeReceipt`, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, and `ComparerAccessors.StringOrdinal` arrive settled; NodaTime `IClock` supplies instants — the App-owned `ClockPolicy` stays at composition; `Tensor/blas#DENSE_ALGEBRA` `AtenFloor.Configure` pins the torch default dtype once at boot and no fit re-pins it. Conditioned multi-channel evidence enters detection from `Stats/signal#SIGNAL_LANE`; a fit lands the dedicated `Runtime/receipts#RECEIPT_UNION` `Fit` case; offline deep-training studies cross `GraduationEvidence` by content key, and the published model-asset manifest riding that message envelope is the sole signature authority — the graduation-decode gate reads feature names off the manifest, never off a companion-published tensor signature.

## [01]-[INDEX]

- [02]-[ESTIMATOR_LANE]: Fit/Predict/Validate/Select contract over one `EstimatorModel` carrier and one `Prediction` egress; `TemporalSpec` generates forecasting, CUSUM, Bayesian-online, and correlated-residual rows without reconstructible knobs.
- [02.1]-[HYPOTHESIS_LAW]: `StatisticalTest` axis binding each row's statistic kernel, sample-arity floor, and p-value tail over the matching `Distributions` CDF.

## [02]-[ESTIMATOR_LANE]

- Owner: `EstimatorFamily` is the receipt family axis; `EstimatorKind` rows carry supervised fit behavior; `GlmFamily` rows carry one exponential-family law — exact unit deviance, variance function, canonical link, response support, and the θ-dependent torch deviance kernel; `LinkFunction` rows carry the inverse link in both the scalar and tensor arities; `KernelRow` rows carry one positive-definite kernel and its Gram, `KernelParams` its per-occurrence parameters; `TimeSeriesModel` rows carry forecast or detector fit behavior; `CurveForm` rows carry the coefficient fit and the evaluator for one curve shape; `TemporalSpec` is the parameterized temporal generator and derives its `TimeSeriesModel`, `CurveSpec` the parameterized curve generator deriving its `CurveForm`; `Estimator` types the problem; `ClusterShape` types grouping ingress; `PredictQuery` types prediction ingress as evidence or horizon; `EstimatorModel` carries fitted parameters; `Prediction` types response, projection, assignment, or anomaly egress; `Design` admits evidence once; `EstimatorPolicy` admits family policy and `ScaleCeiling` caps the quadratic and cubic kernels; `FitContext` carries the proven correspondence beside the composition-supplied clock and dense substrate; `IterativeEngine` owns torch-loss fitting; `InformationCriterion` rows score a fitted likelihood, `SelectionPolicy` admits the selection request and `SelectionReport` carries its ranked verdict; `EstimatorFold` owns `Fit`, `Predict`, `Validate`, and `Select`.
- Cases: `EstimatorFamily` regression · reduction · cluster · classify · temporal; `EstimatorKind` owns the supervised/reduction/grouping/classification rows, `glm` being ONE row parameterized by `GlmFamily` and `c-svm` the sparse-dual sibling of the dense `svm`; `GlmFamily` owns gaussian · poisson · binomial · gamma · inverse-gaussian; `LinkFunction` owns identity · logit · log · inverse · inverse-squared; `KernelRow` owns linear · polynomial · rbf · sigmoid; `InformationCriterion` owns aic · bic; `TimeSeriesModel` owns ar · arma · exponential-smoothing · state-space · cusum · bayesian-online · correlated-residual; `CurveForm` owns polynomial · exponential · power · logarithm · combination; `TemporalSpec` and `CurveSpec` each carry one parameter-complete case per row; `Estimator.Curve` reports the regression family so the held-out validator scores it with no second split policy; `EstimatorModel` adds `Curve` and `Detector` beside the fit carriers; `Prediction` adds `Anomaly` beside `Response`/`Projection`/`Assignment`.
- Entry: `Design.Admit(Matrix<double>, Option<Vector<double>>)` proves non-empty, finite, aligned evidence. `Fit` proves family correspondence, policy ranges, estimator support, kernel parameters, row-count ceilings, and `TemporalSpec` history/ranges before dispatch. `Predict` projects, assigns, forecasts, or scores anomalies through the total `EstimatorModel` switch over one `PredictQuery` ingress. `Validate` scores supervised held-out folds and forecasting forward chains; unsupervised detectors do not fabricate validation labels. `Select(Estimator, Design, Seq<EstimatorPolicy>, SelectionPolicy, IClock, DenseSubstrate)` fits each candidate, scores its information criterion where the fit carries a likelihood, folds its validation spread, and returns one ranked `SelectionReport`. Every fold entry takes the composition-selected `DenseSubstrate` beside the clock and seats it on `FitContext`, so the closed-form thin-QR, the LS-SVM KKT solve, and the AR lag solve each declare the dense leg they run on.
- Auto: `Fit` flattens the estimator's typed payloads once into `FitContext` — which carries the policy union member itself, never a zero-filled scalar tail — then dispatches the row kernel. Every GLM cell is ONE composition: the family's θ-dependent unit-deviance kernel evaluated at the link's own tensor inverse, so a `Link` override changes fit and prediction together and no per-(family, link) loss row exists. Curve rows run one shared kernel that captures the library fit, proves the coefficient census and finiteness, and scores `RSquared` and `StandardError` on the original response scale rather than the linearized one the library fits on, so an exponential row's quality compares with a polynomial's. `c-svm` runs SMO with second-order working-set selection under a byte-budgeted column-LRU kernel cache. Temporal forecasting routes AR through thin QR and ARMA/Holt/state-space through `LevenbergMarquardt`; detection fits one admitted multivariate Gaussian baseline through `Admission.Definite`, then CUSUM folds whitened innovation magnitude, Bayesian-online maintains a budget-capped run-length posterior with conjugate known-covariance mean updates, and correlated-residual scoring reads a `ChiSquared.InvCDF` threshold over Mahalanobis evidence. `Validate` derives contiguous, forward-chain, or unsupported behavior from the typed estimator case.
- Receipt: a fit emits the dedicated `Fit` `ComputeReceipt` case `Runtime/receipts#RECEIPT_UNION` declares for this lane (one case row per measured concern, as the FEA `Solver/contract#SOLVE_CONTRACT` `Solve` and the optimizer/sweep/clash/twin/uncertainty cases each own a row rather than overloading a sibling), carrying family, estimator key, carrier parameter count, iteration count, residual, converged flag, the named fit-quality value, the metric label read off the row's `Metric` column (never a per-arm literal), and retained reduction rank; a closed-form fit ALSO emits the blas `Factorization` receipt under the same `CorrelationId`. Fit-quality and rank read back operator-visibly through the receipt stream (a stall through `ReceiptFolds.Nonconverged`) instead of dying write-only on the carrier.
- Packages: MathNet.Numerics, TorchSharp, libtorch-cpu, HyperJet (temporal-fit exact-Jacobian scalar-AD — recurrences authored once over `DDScalar`, the LM hyperdual arm reading `GetGradient()`), PeterO.Numbers (exact criterion accumulation), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, kernel signal capsule), Rasm.Persistence (project), BCL inbox
- Growth: a new exponential family is one `GlmFamily` row and the `glm` kind is untouched; a new link is one `LinkFunction` row carrying both arities; a new kernel is one `KernelRow` row every kernel consumer inherits; a new selection criterion is one `InformationCriterion` row. A new temporal modality is one `TemporalSpec` case deriving one `TimeSeriesModel` row and binding one kernel; a new curve shape is one `CurveSpec` case deriving one `CurveForm` row carrying its coefficient fit and evaluator, with the shared kernel untouched; each spec owns every non-reconstructible parameter. New fitted or prediction shapes extend `EstimatorModel` or `Prediction` only when payload timing differs. Per-model estimator classes, detector DTOs, per-family GLM kinds, per-kernel Gram helpers, and universal temporal knob records are rejected.
- Boundary: each row binds its genuine mechanism; forced SVD or torch-loss routing is rejected. Curve rows are univariate by construction — a multi-column design is a typed refusal, not a silent first-column read — and each row proves the support its own linearization needs, so a non-positive response never reaches an exponential log and a non-positive feature never reaches a power or logarithm log.
- Boundary: the GLM deviance is elementary — the exponential-family normalizer cancels between the fitted and the saturated model, so NO `MathNet.Numerics.Distributions` member enters the deviance, the deviance-explained metric, or the scaled deviance. `Poisson` and `Binomial` expose no `InvCDF` at all, `InverseGaussian` spells its instance quantile `InvCDF(double)` where every sibling spells `InverseCumulativeDistribution`, its `Median` Brent-solves and can throw, and `Gamma` is rate-parameterized (`WithShapeScale` is the scale form) — so a family that reached for a distribution instance would reach for four different contracts.
- Boundary: `LinkFunction.Inverse` is `g(μ)=1/μ` with domain `η > 0`, the statsmodels `InversePower` link; LBFGS steps out of that domain on the way to the optimum, so the Gamma inverse-link arm gates `η > 0` and refuses typed rather than returning a NaN loss — statsmodels ships `safe_links = [Log]` for exactly this reason and the log link stays the canonical route.
- Boundary: the LS-SVM bordered KKT system is symmetric INDEFINITE, so it rides `FactorRoute.SquarePivoting` and `Admission.Definite` never gates it; SMO materializes no factorization at all and gates only its second-order denominator `a_it > τ`. Closed-form estimators reuse `Tensor/blas#DENSE_ALGEBRA`, specialized grouping rows retain their mutation-local kernels, and detector covariance factors through `Admission.Definite`.
- Boundary: quadratic and cubic kernels refuse above their own `ScaleCeiling` instead of running unbounded — agglomerative linkage is cubic in the merge sweep, and the Gram, the DBSCAN region query, and the kNN leave-one-out score are each quadratic — because a building-scale design silently converts an admitted fit into an unkillable one.
- Boundary: `Prediction` is total across response, projection, assignment, and anomaly evidence; neither `Tensor` nor an untyped score array crosses the boundary. `Stats/signal#SIGNAL_LANE` produces conditioned evidence, Stats owns reusable changepoint/anomaly detection, and the digital twin consumes that detector beside its optimizer-owned surrogate. Hypothesis tests validate `Solver/uncertainty#UNCERTAINTY_LANE` samples; offline deep training remains Python-owned behind `GraduationEvidence`.

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

// One row carries the inverse link g⁻¹(η) in BOTH arities — the scalar the prediction fold applies and the
// tensor the fit's loss composes — so a `Link` override moves the fitted objective and the predicted mean
// together. The variance function V(μ) is the FAMILY's, never the link's: identity paired with a binomial
// family still weights by μ(1−μ), and a link-owned variance silently reports the Gaussian one.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LinkFunction {
    public static readonly LinkFunction Identity = new("identity", static eta => eta, static eta => eta, domain: static _ => true);
    public static readonly LinkFunction Logit = new("logit", static eta => 1.0 / (1.0 + Math.Exp(-eta)), static eta => torch.special.expit(eta), domain: static _ => true);
    public static readonly LinkFunction Log = new("log", static eta => Math.Exp(eta), static eta => eta.exp(), domain: static _ => true);
    // g(μ)=1/μ, the statsmodels `InversePower` link: η is the RECIPROCAL mean, so the domain is η > 0 and an
    // unconstrained LBFGS step reaches η ≤ 0 on the way to the optimum — the gate refuses there rather than
    // returning a NaN loss the driver reads as progress. statsmodels ships `safe_links = [Log]` for the same reason.
    public static readonly LinkFunction Inverse = new("inverse", static eta => 1.0 / eta, static eta => eta.pow(-1.0), domain: static eta => eta > 0.0);
    // Inverse-Gaussian's canonical g(μ)=1/μ², so g⁻¹(η)=η^(−1/2) under the same positive-η domain.
    public static readonly LinkFunction InverseSquared = new("inverse-squared", static eta => 1.0 / Math.Sqrt(eta), static eta => eta.pow(-0.5), domain: static eta => eta > 0.0);

    private LinkFunction(string key, Func<double, double> scalar, Func<Tensor, Tensor> tensor, Func<double, bool> domain) : this(key) {
        this.scalar = scalar;
        this.tensor = tensor;
        this.domain = domain;
    }

    private readonly Func<double, double> scalar;
    private readonly Func<Tensor, Tensor> tensor;
    private readonly Func<double, bool> domain;

    public double Mean(double eta) => scalar(eta);

    internal Tensor Mean(Tensor eta) => tensor(eta);

    // The linear predictor's admissible range, proved over the FITTED η before the carrier lands and over the
    // predicted η before a mean leaves; an out-of-domain η is a typed refusal naming the link.
    internal Fin<Unit> Domain(Vector<double> eta) =>
        eta.All(domain) ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create($"<glm-{Key}-link-domain:{eta.Minimum()}>"));
}

// One exponential-family law per row. The EXACT unit deviance d(y,μ) is branch-explicit at its own limits —
// y=0 and y=1 are real points of the support where the y·ln y term has a finite limit, so the branch states
// the limit and a clamp (`Math.Max(1e-9, …)`) that fabricates a nearby value is the deleted form. The torch
// column carries only the θ-DEPENDENT part of that same deviance: the exponential-family normalizer and every
// y-only term are constants in θ that cancel out of the gradient, and ln y is −∞ at the admitted y=0 where a
// `torch.where` would still evaluate both branches into a NaN gradient. `Weight` is the GLM prior weight —
// the binomial trial count m per row, 1.0 for every other family — and both d and V read it.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlmFamily {
    public static readonly GlmFamily Gaussian = new("gaussian", LinkFunction.Identity,
        deviance: static (y, mu, _) => (y - mu) * (y - mu),
        variance: static (_, _) => 1.0,
        support: static y => true,
        kernel: static (mu, y) => mu.sub(y).pow(2));
    public static readonly GlmFamily Poisson = new("poisson", LinkFunction.Log,
        deviance: static (y, mu, w) => y == 0.0 ? 2.0 * w * mu : 2.0 * w * (y * Math.Log(y / mu) - (y - mu)),
        variance: static (mu, _) => mu,
        support: static y => y >= 0.0 && double.IsInteger(y),
        kernel: static (mu, y) => mu.sub(y.mul(mu.log())));
    // y is the observed PROPORTION of m trials, so the endpoints y=0 and y=1 are the ordinary Bernoulli
    // observations and carry the whole deviance in one term each.
    public static readonly GlmFamily Binomial = new("binomial", LinkFunction.Logit,
        deviance: static (y, mu, m) => y == 0.0 ? -2.0 * m * Math.Log(1.0 - mu)
            : y == 1.0 ? -2.0 * m * Math.Log(mu)
            : 2.0 * m * (y * Math.Log(y / mu) + (1.0 - y) * Math.Log((1.0 - y) / (1.0 - mu))),
        variance: static (mu, m) => mu * (1.0 - mu) / m,
        support: static y => y >= 0.0 && y <= 1.0,
        kernel: static (mu, y) => y.mul(mu.log()).add(y.neg().add(1.0).mul(mu.neg().add(1.0).log())).neg());
    public static readonly GlmFamily Gamma = new("gamma", LinkFunction.Log,
        deviance: static (y, mu, w) => 2.0 * w * ((y - mu) / mu - Math.Log(y / mu)),
        variance: static (mu, _) => mu * mu,
        support: static y => y > 0.0,
        kernel: static (mu, y) => y.div(mu).add(mu.log()));
    public static readonly GlmFamily InverseGaussian = new("inverse-gaussian", LinkFunction.InverseSquared,
        deviance: static (y, mu, w) => w * (y - mu) * (y - mu) / (y * mu * mu),
        variance: static (mu, _) => mu * mu * mu,
        support: static y => y > 0.0,
        kernel: static (mu, y) => y.div(mu.pow(2)).sub(mu.pow(-1.0).mul(2.0)));

    private GlmFamily(
        string key, LinkFunction canonical, Func<double, double, double, double> deviance,
        Func<double, double, double> variance, Func<double, bool> support, Func<Tensor, Tensor, Tensor> kernel) : this(key) {
        Canonical = canonical;
        this.deviance = deviance;
        this.variance = variance;
        this.support = support;
        this.kernel = kernel;
    }

    private readonly Func<double, double, double, double> deviance;
    private readonly Func<double, double, double> variance;
    private readonly Func<double, bool> support;
    private readonly Func<Tensor, Tensor, Tensor> kernel;

    public LinkFunction Canonical { get; }

    public double Deviance(double y, double mu, double weight) => deviance(y, mu, weight);

    public double Variance(double mu, double weight) => variance(mu, weight);

    // The loss is ONE composition for every (family, link) cell: the θ-dependent unit deviance evaluated at
    // the link's own tensor inverse. A per-pair loss table re-derives five identities the composition already
    // yields — gaussian∘identity is the squared error, poisson∘log is `e^η − y·η`, binomial∘logit is
    // `log1p(e^η) − y·η`, gamma∘log is `η + y·e^{−η}`, and gamma∘inverse is `y·η − ln η`.
    internal Func<Tensor, Tensor, Tensor> Loss(LinkFunction link) => (eta, y) => kernel(link.Mean(eta), y).mean();

    internal Fin<Unit> Admit(Design design) =>
        design.Targets.Filter(y => y.All(support)).IsSome
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComputeFault.Create($"<glm-response-support:{Key}>"));

    // Deviance explained, 1 − D_resid/D_null, the family's own R² analogue: D_null is the same unit deviance
    // against the intercept-only mean, so the scale-free ratio compares fits within one family and the
    // saturated-model normalizer cancels twice over.
    internal double Explained(Vector<double> y, Vector<double> fitted, double weight) {
        double mean = y.Average();
        double nullDeviance = EstimatorFold.ExactSum(toSeq(Enumerable.Range(0, y.Count).Select(i => deviance(y[i], mean, weight))));
        return nullDeviance > 0.0 ? 1.0 - Total(y, fitted, weight) / nullDeviance : 0.0;
    }

    internal double Total(Vector<double> y, Vector<double> fitted, double weight) =>
        EstimatorFold.ExactSum(toSeq(Enumerable.Range(0, y.Count).Select(i => deviance(y[i], fitted[i], weight))));
}

// One kernel vocabulary every kernel consumer reads — the SMO Gram, the LS-SVM Gram, the decision sum, and
// the kernel-PCA operand are one row's `At`/`Gram`, never four transcriptions of the same RBF exponent.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KernelRow {
    public static readonly KernelRow Linear = new("linear", static (a, b, _) => a.DotProduct(b));
    public static readonly KernelRow Polynomial = new("polynomial", static (a, b, p) => Math.Pow(a.DotProduct(b) / p.Bandwidth + p.Offset, p.Degree));
    public static readonly KernelRow Rbf = new("rbf", static (a, b, p) => Math.Exp(-(a - b).PointwisePower(2.0).Sum() / (2.0 * p.Bandwidth * p.Bandwidth)));
    public static readonly KernelRow Sigmoid = new("sigmoid", static (a, b, p) => Math.Tanh(a.DotProduct(b) / p.Bandwidth + p.Offset));

    private readonly Func<Vector<double>, Vector<double>, KernelParams, double> kernel;

    public double At(Vector<double> left, Vector<double> right, KernelParams parameters) => kernel(left, right, parameters);

    public Matrix<double> Gram(Matrix<double> left, Matrix<double> right, KernelParams parameters) =>
        Matrix<double>.Build.Dense(left.RowCount, right.RowCount, (i, j) => kernel(left.Row(i), right.Row(j), parameters));

    // The diagonal is the SMO second-order denominator's operand and, for `rbf`/`sigmoid` with zero offset,
    // a constant — computing it per pair from the full kernel keeps the row the one authority on its value.
    public Vector<double> Diagonal(Matrix<double> x, KernelParams parameters) =>
        Vector<double>.Build.Dense(x.RowCount, i => kernel(x.Row(i), x.Row(i), parameters));
}

// Sigmoid's `tanh` is conditionally positive-definite only for a narrow parameter range, so the row set is
// admitted on parameter ranges alone and the SMO gate reads the KKT verdict, never a Mercer proof.
public sealed record KernelParams(double Bandwidth, double Degree, double Offset) {
    public static readonly KernelParams Canonical = new(Bandwidth: 1.0, Degree: 3.0, Offset: 1.0);

    internal Fin<Unit> Admit() =>
        Bandwidth > 0.0 && Degree >= 1.0 && double.IsFinite(Bandwidth) && double.IsFinite(Degree) && double.IsFinite(Offset)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComputeFault.Create($"<kernel-params:{Bandwidth}:{Degree}:{Offset}>"));
}

// Quadratic and cubic kernels refuse ABOVE their own row count instead of running unbounded: a building-scale
// design turns an admitted fit into an unkillable one, and the ceiling names which complexity class refused.
public sealed record ScaleCeiling(int Rows, string Gate) {
    public static readonly ScaleCeiling Cubic = new(Rows: 2_048, Gate: "cubic");
    public static readonly ScaleCeiling Quadratic = new(Rows: 32_768, Gate: "quadratic");

    internal Fin<Unit> Admit(int rows) =>
        rows <= Rows ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create($"<estimator-scale-{Gate}:{rows}/{Rows}>"));
}

// AIC and BIC differ only in the penalty per fitted parameter, so the row IS that penalty; a criterion whose
// penalty reads the sample size takes it as an argument rather than growing a second column.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InformationCriterion {
    public static readonly InformationCriterion Akaike = new("aic", static (k, _) => 2.0 * k);
    public static readonly InformationCriterion Bayes = new("bic", static (k, n) => k * Math.Log(n));

    private readonly Func<long, int, double> penalty;

    // Lower is better on both rows, so the ranked verdict orders ascending with no per-row direction column.
    internal double Score(double logLikelihood, long parameters, int samples) =>
        penalty(parameters, samples) - 2.0 * logLikelihood;
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
// ONE `glm` row spans every exponential family: the family is per-occurrence payload on `Estimator.Regression`,
// so poisson, binomial, gamma, and inverse-gaussian fits differ by a row value, never by a kind. A per-family
// kind re-declares the same driver, the same admission shape, and the same loss composition five times and
// hard-codes each one's link into its key, which is exactly what made a `glm-poisson` fit under an overridden
// identity link minimize the log-link deviance and predict through the identity mean.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EstimatorKind {
    public static readonly EstimatorKind Ols = new("ols", EstimatorFamily.Regression, supervised: true, metric: "r2", EstimatorFold.Ordinary, EstimatorFold.RealResponse, OptimDriver.Adam);
    public static readonly EstimatorKind Ridge = new("ridge", EstimatorFamily.Regression, supervised: true, metric: "r2", EstimatorFold.Ridged, EstimatorFold.RegularizedResponse, OptimDriver.Adam);
    public static readonly EstimatorKind Lasso = new("lasso", EstimatorFamily.Regression, supervised: true, metric: "r2", EstimatorFold.Penalized, EstimatorFold.IterativeResponse, OptimDriver.Adam);
    public static readonly EstimatorKind Glm = new("glm", EstimatorFamily.Regression, supervised: true, metric: "deviance-explained",
        static ctx => EstimatorFold.Deviance(ctx, ctx.Family.Loss(ctx.Link)), EstimatorFold.GlmResponse, OptimDriver.LBfgs);
    public static readonly EstimatorKind Pca = new("pca", EstimatorFamily.Reduction, supervised: false, metric: "explained-energy", EstimatorFold.Principal, EstimatorFold.ReductionDesign, OptimDriver.Adam);
    public static readonly EstimatorKind KernelPca = new("kernel-pca", EstimatorFamily.Reduction, supervised: false, metric: "explained-energy", EstimatorFold.KernelPrincipal, EstimatorFold.KernelReductionDesign, OptimDriver.Adam);
    public static readonly EstimatorKind Nmf = new("nmf", EstimatorFamily.Reduction, supervised: false, metric: "reconstruction-error", EstimatorFold.NonNegative, EstimatorFold.NonNegativeReductionDesign, OptimDriver.Adam);
    public static readonly EstimatorKind KMeans = new("kmeans", EstimatorFamily.Cluster, supervised: false, metric: "inertia", EstimatorFold.Lloyd, EstimatorFold.GroupingDesign, OptimDriver.Adam);
    public static readonly EstimatorKind Gmm = new("gmm", EstimatorFamily.Cluster, supervised: false, metric: "log-likelihood", EstimatorFold.ExpectationMaximization, EstimatorFold.MixtureDesign, OptimDriver.Adam);
    public static readonly EstimatorKind Dbscan = new("dbscan", EstimatorFamily.Cluster, supervised: false, metric: "cluster-count", EstimatorFold.Reachability, EstimatorFold.DensityDesign, OptimDriver.Adam);
    public static readonly EstimatorKind Hierarchical = new("hierarchical", EstimatorFamily.Cluster, supervised: false, metric: "inertia", EstimatorFold.Agglomerative, EstimatorFold.LinkageDesign, OptimDriver.Adam);
    public static readonly EstimatorKind Knn = new("knn", EstimatorFamily.Classify, supervised: true, metric: "accuracy", EstimatorFold.Neighborhood, EstimatorFold.NeighborhoodDesign, OptimDriver.Adam);
    public static readonly EstimatorKind Svm = new("svm", EstimatorFamily.Classify, supervised: true, metric: "accuracy", EstimatorFold.MarginMachines, EstimatorFold.MarginDesign, OptimDriver.Adam);
    public static readonly EstimatorKind CSvm = new("c-svm", EstimatorFamily.Classify, supervised: true, metric: "accuracy", EstimatorFold.SequentialMinimal, EstimatorFold.MarginDesign, OptimDriver.Adam);
    public static readonly EstimatorKind NaiveBayes = new("naive-bayes", EstimatorFamily.Classify, supervised: true, metric: "accuracy", EstimatorFold.GaussianBayes, EstimatorFold.ClassLabels, OptimDriver.Adam);

    private EstimatorKind(
        string key, EstimatorFamily family, bool supervised, string metric, Func<FitContext, Fin<FittedModel>> fit,
        Func<FitContext, Fin<Unit>> admit, OptimDriver driver) : this(key) {
        Family = family;
        Supervised = supervised;
        Metric = metric;
        this.fit = fit;
        this.admit = admit;
        Driver = driver;
    }

    private readonly Func<FitContext, Fin<FittedModel>> fit;
    private readonly Func<FitContext, Fin<Unit>> admit;

    public EstimatorFamily Family { get; }
    public bool Supervised { get; }
    public string Metric { get; }
    public OptimDriver Driver { get; }

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

// Curve rows own the linear-in-parameters fits whose design matrix is a transform of one feature column, so the
// library's own log-linearization conventions stay authoritative and no arm re-derives which side gets logged.
// Every row scores identically — `RSquared` is the quality and `StandardError` the residual — because each fits an
// identity link, so the link-weighted Pearson dispersion the GLM rows need has nothing to weight here.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CurveForm {
    public static readonly CurveForm Polynomial = new(
        "polynomial",
        static (spec, x, y) => Fit.Polynomial(x, y, spec.Terms - 1, DirectRegressionMethod.QR),
        static (_, c, x) => MathNet.Numerics.Polynomial.Evaluate(x, [.. c]));
    public static readonly CurveForm Exponential = new(
        "exponential",
        static (_, x, y) => Pair(Fit.Exponential(x, y, DirectRegressionMethod.QR)),
        static (_, c, x) => c[0] * Math.Exp(c[1] * x));
    public static readonly CurveForm Power = new(
        "power",
        static (_, x, y) => Pair(Fit.Power(x, y, DirectRegressionMethod.QR)),
        static (_, c, x) => c[0] * Math.Pow(x, c[1]));
    public static readonly CurveForm Logarithm = new(
        "logarithm",
        static (_, x, y) => Pair(Fit.Logarithm(x, y, DirectRegressionMethod.QR)),
        static (_, c, x) => c[0] + c[1] * Math.Log(x));
    public static readonly CurveForm Combination = new(
        "combination",
        static (spec, x, y) => Fit.LinearCombination(x, y, [.. spec.Functions]),
        static (spec, c, x) => spec.Functions.Map((basis, k) => c[k] * basis(x)).Fold(0.0, static (total, term) => total + term));

    private CurveForm(
        string key,
        Func<CurveSpec, double[], double[], double[]> fit,
        Func<CurveSpec, Vector<double>, double, double> evaluate) : this(key) {
        this.fit = fit;
        this.evaluate = evaluate;
    }

    private readonly Func<CurveSpec, double[], double[], double[]> fit;
    private readonly Func<CurveSpec, Vector<double>, double, double> evaluate;

    public string Metric => "r2";

    internal double[] Fit(CurveSpec spec, double[] x, double[] y) => fit(spec, x, y);

    internal double Evaluate(CurveSpec spec, Vector<double> coefficients, double x) => evaluate(spec, coefficients, x);

    // Two-parameter library fits name their elements differently (`(A, R)` versus `(A, B)`), so positional
    // deconstruction reads both and no row spells a member name the next release could rename.
    private static double[] Pair((double, double) row) => [row.Item1, row.Item2];
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

// `CurveSpec` generates a curve row exactly as `TemporalSpec` generates a temporal one, owning every parameter its
// form cannot reconstruct and deriving the `CurveForm` that binds the kernel.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveSpec {
    private CurveSpec() { }

    public sealed record Polynomial(int Order) : CurveSpec;
    public sealed record Exponential : CurveSpec;
    public sealed record Power : CurveSpec;
    public sealed record Logarithm : CurveSpec;
    public sealed record Combination(Seq<Func<double, double>> Basis) : CurveSpec;

    public CurveForm Form => Switch(
        polynomial: static _ => CurveForm.Polynomial, exponential: static _ => CurveForm.Exponential,
        power: static _ => CurveForm.Power, logarithm: static _ => CurveForm.Logarithm,
        combination: static _ => CurveForm.Combination);

    // Fitted-parameter count, the degrees of freedom `GoodnessOfFit.StandardError` consumes: a polynomial of order n
    // fits n+1 coefficients, the log-linearized rows fit two, and a combination fits one per basis function.
    public int Terms => Switch(
        polynomial: static s => s.Order + 1, exponential: static _ => 2,
        power: static _ => 2, logarithm: static _ => 2,
        combination: static s => s.Basis.Count);

    internal Seq<Func<double, double>> Functions => Switch(
        combination: static s => s.Basis,
        polynomial: static _ => Seq<Func<double, double>>(), exponential: static _ => Seq<Func<double, double>>(),
        power: static _ => Seq<Func<double, double>>(), logarithm: static _ => Seq<Func<double, double>>());

    // `Power` and `Logarithm` linearize through `ln x` and `Exponential` through `ln y`, so each row's admission
    // proves the support its transform needs instead of letting the library return a silent NaN coefficient.
    internal Fin<Unit> Admit(Design design) =>
        design.Columns != 1
            ? Fin.Fail<Unit>(ComputeFault.Create($"<curve-univariate:{design.Columns}>"))
        : design.Rows <= Terms
            ? Fin.Fail<Unit>(ComputeFault.Create($"<curve-underdetermined:{design.Rows}<={Terms}>"))
        : Switch(
            state: design,
            polynomial: static (d, s) => s.Order >= 1
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(ComputeFault.Create($"<curve-order:{s.Order}>")),
            exponential: static (d, _) => Support(d.Targets, "curve-exponential-response"),
            power: static (d, _) => Support(Optional(d.Features.Column(0)), "curve-power-feature")
                .Bind(_ => Support(d.Targets, "curve-power-response")),
            logarithm: static (d, _) => Support(Optional(d.Features.Column(0)), "curve-logarithm-feature"),
            combination: static (_, s) => !s.Basis.IsEmpty && s.Basis.ForAll(static f => f is not null)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(ComputeFault.Create("<curve-basis-empty>")));

    private static Fin<Unit> Support(Option<Vector<double>> values, string gate) =>
        values.Filter(static v => v.All(static value => value > 0.0)).IsSome
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComputeFault.Create($"<{gate}>"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Estimator {
    private Estimator() { }

    // The exponential family is per-occurrence payload beside the kind, and `Link` overrides its canonical
    // link for the whole fit — objective and predicted mean alike. A least-squares row's family is Gaussian
    // and its response support is the real line, so one shape serves every regression row.
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

    // One uniform estimator key and metric label for the receipt — both read the row columns, so no arm
    // re-derives per-kind knowledge through nested ternaries or label literals. The regression key is
    // family-qualified because one `glm` kind spans five families and a bare kind key would report five
    // distinct fits identically; a least-squares row qualifies to its own Gaussian family for the same reason.
    public string Key => Switch(
        regression: static r => $"{r.Kind.Key}:{r.Family.Key}", curve: static c => c.Spec.Form.Key, reduction: static d => d.Kind.Key,
        cluster: static c => c.Kind.Key, classify: static c => c.Kind.Key, temporal: static t => t.Spec.Model.Key);

    public string Metric => Switch(
        regression: static r => r.Kind.Metric, curve: static c => c.Spec.Form.Metric, reduction: static d => d.Kind.Metric,
        cluster: static c => c.Kind.Metric, classify: static c => c.Kind.Metric, temporal: static t => t.Spec.Model.Metric);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimatorModel {
    private EstimatorModel() { }

    // The family rides the carrier beside the link because the predicted mean, the deviance-explained
    // quality, and the scaled deviance `D/φ̂` all read it; scaled deviance is the χ²_{n−p} goodness statistic
    // the Pearson dispersion on `FittedModel.Residual` cannot answer, so it lands here rather than nowhere.
    public sealed record Linear(Vector<double> Coefficients, double Intercept, LinkFunction Link, GlmFamily Family, double ScaledDeviance) : EstimatorModel;
    // Specs ride the carrier because a combination row's basis is the only thing that can evaluate its own
    // coefficients, and a polynomial's order is recoverable from the coefficient count for every other row.
    public sealed record Curve(CurveSpec Spec, Vector<double> Coefficients) : EstimatorModel;
    public sealed record Basis(Matrix<double> Components, Vector<double> Singular, Vector<double> Mean, double EnergyFraction) : EstimatorModel;
    public sealed record KernelBasis(Matrix<double> Training, Matrix<double> Alphas, Vector<double> Eigen, KernelRow Kernel, KernelParams Parameters, Vector<double> RowMean, double GrandMean) : EstimatorModel;
    public sealed record Factors(Matrix<double> Encoder, Matrix<double> Components) : EstimatorModel;
    public sealed record Partition(Matrix<double> Centroids, ImmutableArray<int> Labels) : EstimatorModel;
    public sealed record Density(Matrix<double> Training, ImmutableArray<int> Labels, ImmutableArray<bool> Core, double Radius) : EstimatorModel;
    public sealed record Mixture(Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights) : EstimatorModel;
    // One machine per distinct label (one-vs-rest); binary targets reduce to two symmetric machines and argmax
    // over decisions recovers the sign rule, so multiclass is the same carrier, never a sibling. `Support`
    // carries the machine's own support indices — SMO fills the sparse KKT-active set and LS-SVM every row,
    // because the dense closed form has no zero duals — so the decision sum walks the support alone and the
    // carrier's parameter count is the honest one for both mechanisms.
    public sealed record Margin(
        Matrix<double> Training,
        Seq<(int Label, ImmutableArray<int> Support, Vector<double> Duals, double Bias)> Machines,
        KernelRow Kernel,
        KernelParams Parameters) : EstimatorModel;
    public sealed record Neighbors(Matrix<double> Design, ImmutableArray<int> Labels, int K) : EstimatorModel;
    public sealed record Bayes(Matrix<double> Means, Matrix<double> Variances, Vector<double> Priors) : EstimatorModel;
    public sealed record Lag(Vector<double> ArCoefficients, Vector<double> MaCoefficients, double Variance, Vector<double> Tail, TimeSeriesModel Model) : EstimatorModel;
    public sealed record Detector(Vector<double> Mean, Cholesky<double> Scale, TemporalSpec Spec, int MaxRunLength) : EstimatorModel;

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

    // Retained reduction rank for the receipt's RetainedRank slot — basis/kernel-basis component count, NMF inner
    // dimension; zero on non-reduction carriers so a non-reduction fit reads no retained rank without a family probe.
    public int RetainedRank => Switch(
        basis: static b => b.Singular.Count,
        kernelBasis: static k => k.Eigen.Count,
        factors: static f => f.Encoder.ColumnCount,
        linear: static _ => 0, curve: static _ => 0, partition: static _ => 0, density: static _ => 0, mixture: static _ => 0,
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

// Evidence and horizon are the two ingress shapes a fitted carrier admits, absorbed at ONE parameter by the
// ad-hoc union's implicit conversions, so `model.Predict(features)` and `model.Predict(steps)` are one entry.
// A forecast carrier that took a `Matrix<double>` read only its row count and every temporal caller fabricated
// a zero matrix of that height — a phantom operand the carrier never touched and the reader could not explain.
[Union<Matrix<double>, int>(T1Name = "Evidence", T2Name = "Horizon")]
public readonly partial struct PredictQuery;

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

    // `Weight` is the GLM prior weight — the binomial trial count m, 1.0 for every other family — so the
    // deviance and the variance function read one axis rather than a binomial-only trials column.
    public sealed record Regression(double Regularization, double LearningRate, double Weight, FitBudget Budget) : EstimatorPolicy;
    public sealed record Reduction(double EnergyFraction, KernelRow Kernel, KernelParams Parameters, FitBudget Budget) : EstimatorPolicy;
    public sealed record Grouping(double Radius, int Neighbors, double Ridge, FitBudget Budget) : EstimatorPolicy;
    // `Regularization` is the LS-SVM γ and `Box` the C-SVM box bound C — two mechanisms, two bounds, and the
    // row that ignores the other reads nothing from it. `KktTolerance` is ε in the SMO stop `m(α) − M(α) ≤ ε`.
    public sealed record Classification(
        double Regularization, KernelRow Kernel, KernelParams Parameters, int Neighbors,
        double Box, double KktTolerance, FitBudget Budget, bool Shrinking) : EstimatorPolicy;
    public sealed record Temporal(FitBudget Budget) : EstimatorPolicy;

    public static readonly EstimatorPolicy CanonicalRegression = new Regression(Regularization: 1e-3, LearningRate: 0.05, Weight: 1.0, FitBudget.Canonical);
    public static readonly EstimatorPolicy CanonicalReduction = new Reduction(EnergyFraction: 0.95, KernelRow.Rbf, KernelParams.Canonical, FitBudget.Canonical);
    public static readonly EstimatorPolicy CanonicalGrouping = new Grouping(Radius: 0.5, Neighbors: 4, Ridge: 1e-3, FitBudget.Grouping);
    public static readonly EstimatorPolicy CanonicalClassification = new Classification(
        Regularization: 1e-3, KernelRow.Rbf, KernelParams.Canonical, Neighbors: 5,
        Box: 1.0, KktTolerance: 1e-3, new FitBudget(MaxIterations: 1_000_000, Tolerance: 1e-3), Shrinking: true);
    public static readonly EstimatorPolicy CanonicalTemporal = new Temporal(FitBudget.Canonical);

    public EstimatorFamily Family => Switch(
        regression: static _ => EstimatorFamily.Regression, reduction: static _ => EstimatorFamily.Reduction,
        grouping: static _ => EstimatorFamily.Cluster, classification: static _ => EstimatorFamily.Classify,
        temporal: static _ => EstimatorFamily.Temporal);

    internal Fin<Unit> Admit() => Switch(
        regression: static p => ScalarPolicy(p.Regularization >= 0.0 && p.LearningRate > 0.0 && p.Weight > 0.0, p.Regularization, p.LearningRate).Bind(_ => p.Budget.Admit()),
        reduction: static p => ScalarPolicy(p.EnergyFraction > 0.0 && p.EnergyFraction <= 1.0, p.EnergyFraction, 1.0).Bind(_ => p.Parameters.Admit()).Bind(_ => p.Budget.Admit()),
        grouping: static p => ScalarPolicy(p.Radius > 0.0 && p.Ridge > 0.0 && p.Neighbors >= 1, p.Radius, p.Ridge).Bind(_ => p.Budget.Admit()),
        classification: static p => ScalarPolicy(p.Regularization > 0.0 && p.Box > 0.0 && p.KktTolerance > 0.0 && p.Neighbors >= 1, p.Regularization, p.Box).Bind(_ => p.Parameters.Admit()).Bind(_ => p.Budget.Admit()),
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

// Interior mechanism carrier: the estimator's typed payloads flatten once at the correspondence gate and the
// POLICY rides whole as its own union member. Flattening it to a scalar tail zero-filled every axis the
// family does not own, so a grouping kernel read a `Regularization` of 0.0 that no policy ever set and a
// classification arm silently inherited a canonical budget the caller's policy had already replaced.
// `Clock` and `Substrate` are the two composition-supplied ambients a fit reads: the instant its receipt stamps
// and the dense leg its closed-form solves run on. The substrate is a VALUE here rather than ambient state, so a
// fit declares the leg it asked for and the witnessed carrier reports the one that actually served.
internal sealed record FitContext(
    Estimator Estimator, Design Design, IClock Clock, DenseSubstrate Substrate, EstimatorPolicy Policy, LinkFunction Link, GlmFamily Family,
    OptimDriver Driver, int Rank, Option<ClusterShape> Cluster, Option<TemporalSpec> Temporal, Option<CurveSpec> Curve) {
    // The gate proved `Policy.Family == Estimator.Family` before this record existed, so the narrowing read is
    // that same proof re-projected once per kernel; the fault arm names the gate rather than re-admitting.
    internal Fin<TCase> Case<TCase>() where TCase : EstimatorPolicy =>
        Policy is TCase typed
            ? Fin.Succ(typed)
            : Fin.Fail<TCase>(ComputeFault.Create($"<estimator-policy-case:{Policy.Family.Key}:{Estimator.Key}>"));

    internal int Rows => Design.Rows;
}

// `LogLikelihood` is `None` wherever the row's mechanism defines none — k-means inertia, DBSCAN reachability,
// linkage spread, kNN votes, and margin accuracy are not likelihoods — so an information criterion is
// unavailable rather than fabricated from a quality score that happens to be a number.
public sealed record FittedModel(
    Estimator Estimator, EstimatorModel Carrier, double Quality, double Residual,
    Option<double> LogLikelihood, int Iterations, bool Converged, Instant At) {
    public Fin<Prediction> Predict(PredictQuery query) => EstimatorFold.Predict(this, query);
}

// `Normalizer` declares which convention `Spread` was reduced under; the k folds ARE the population of folds,
// never a sample drawn from a larger one, so the report carries `Population` and a consumer comparing spreads
// across reports reads the convention instead of assuming Bessel's correction either way.
public sealed record ValidationReport(
    Estimator Estimator, Seq<double> FoldQuality, double Mean, double Spread,
    MomentNormalizer Normalizer, int Folds, Instant At);

public sealed record SelectionPolicy(InformationCriterion Criterion, int Folds) {
    public static readonly SelectionPolicy Canonical = new(InformationCriterion.Akaike, Folds: 5);

    internal Fin<Unit> Admit(int candidates) =>
        candidates < 2 || Folds < 2
            ? Fin.Fail<Unit>(ComputeFault.Create($"<selection-policy:{candidates}:{Folds}>"))
            : Fin.Succ(unit);
}

// Both evidence axes ride every candidate: `Criterion` is `None` where the fit carries no likelihood, and the
// validation pair is always present because every admitted candidate crossed the same held-out or
// forward-chain split. Rank is ascending — lower criterion first where one exists, higher validation mean
// otherwise — so the verdict is one ordering and a consumer re-ranks nothing.
public sealed record CandidateEvidence(
    EstimatorPolicy Policy, Option<double> Criterion, double ValidationMean, double ValidationSpread, int Rank);

public sealed record SelectionReport(Seq<CandidateEvidence> Candidates, EstimatorPolicy Chosen, InformationCriterion Criterion, Instant At);

// Torch-loss rows minimize a Tensor loss under torch.autograd + the row's OptimDriver inside one DisposeScope; the LBFGS line-search form re-evaluates the closure per probe.
// Every intermediate is reclaimed at scope exit and stamped by DisposeScopeManager.Statistics; AnomalyMode traps a NaN/inf fit and no Tensor escapes the lane.
// The Float64 default dtype is BOOT state `Tensor/blas#DENSE_ALGEBRA` `AtenFloor.Configure` pins once — a
// per-fit `set_default_dtype` mutates a process-global from inside a lane that may run concurrently with any
// other torch consumer, and every tensor here already names its own `ScalarType.Float64` at construction.
public static class IterativeEngine {
    public static Fin<(Vector<double> Theta, double Loss, int Iterations, bool Converged, ThreadDisposeScopeStatistics Memory)> Minimize(
        Func<Tensor, Tensor, Tensor, Tensor> loss, Matrix<double> design, Vector<double> response, OptimDriver driver, double learningRate, FitBudget budget) {
        using DisposeScope scope = torch.NewDisposeScope();
        using AnomalyMode anomaly = new(enabled: true, check_nan: true);
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

    public static Fin<FittedModel> Fit(Estimator estimator, Design design, EstimatorPolicy policy, IClock clock, DenseSubstrate substrate) =>
        Admitted(estimator, design, policy, clock, substrate).Bind(static ctx => ctx.Estimator.Switch(
            state: ctx,
            regression: static (c, r) => r.Kind.Fit(c),
            curve: static (c, k) => CurveFit(c, k.Spec),
            reduction: static (c, d) => d.Kind.Fit(c),
            cluster: static (c, g) => g.Kind.Fit(c),
            classify: static (c, k) => k.Kind.Fit(c),
            temporal: static (c, t) => t.Spec.Model.Fit(c)));

    // Each carrier names the ONE ingress shape its mechanism consumes and the query's projection refuses the
    // other by name, so a horizon handed to a projection carrier and evidence handed to a forecast carrier are
    // each a typed refusal rather than a silently reinterpreted row count.
    public static Fin<Prediction> Predict(FittedModel model, PredictQuery query) =>
        model.Carrier.Switch<PredictQuery, Fin<Prediction>>(
            state: query,
            linear: static (q, c) => Evidence(q).Bind(x => {
                Vector<double> eta = x.Multiply(c.Coefficients).Add(c.Intercept);
                return c.Link.Domain(eta).Map(_ => (Prediction)new Prediction.Response(eta.Map(c.Link.Mean)));
            }),
            curve: static (q, c) => Evidence(q).Bind(x => x.ColumnCount == 1
                ? Fin.Succ<Prediction>(new Prediction.Response(x.Column(0).Map(value => c.Spec.Form.Evaluate(c.Spec, c.Coefficients, value))))
                : Fin.Fail<Prediction>(ComputeFault.Create($"<curve-univariate:{x.ColumnCount}>"))),
            basis: static (q, b) => Evidence(q).Map(x => (Prediction)new Prediction.Projection(Center(x, b.Mean).Multiply(b.Components.Transpose()))),
            kernelBasis: static (q, k) => Evidence(q).Map(x => (Prediction)new Prediction.Projection(KernelProject(x, k))),
            factors: static (q, f) => Evidence(q).Map(x => (Prediction)new Prediction.Projection(NonNegativeEncode(x, f.Components))),
            partition: static (q, p) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => Nearest(row, p.Centroids))])),
            density: static (q, d) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => DensityAssign(row, d))])),
            mixture: static (q, m) => Evidence(q).Bind(x => Responsibilities(x, m).Map(static labels => (Prediction)new Prediction.Assignment(labels))),
            margin: static (q, m) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => Strongest(row, m))])),
            neighbors: static (q, nbr) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => Vote(row, nbr))])),
            bayes: static (q, b) => Evidence(q).Map(x => (Prediction)new Prediction.Assignment([.. x.EnumerateRows().Select(row => Posterior(row, b))])),
            lag: static (q, l) => Horizon(q).Map(h => (Prediction)new Prediction.Response(Forecast(l, h))),
            detector: static (q, d) => Evidence(q).Bind(x => Detect(d, x)));

    private static Fin<Matrix<double>> Evidence(PredictQuery query) =>
        query.IsEvidence
            ? Fin.Succ(query.AsEvidence)
            : Fin.Fail<Matrix<double>>(ComputeFault.Create("<predict-needs-evidence>"));

    private static Fin<int> Horizon(PredictQuery query) =>
        query.IsHorizon && query.AsHorizon >= 1
            ? Fin.Succ(query.AsHorizon)
            : Fin.Fail<int>(ComputeFault.Create("<predict-needs-horizon>"));

    // Split strategy derives from family: contiguous k-fold scores regression/classification;
    // expanding-window forward chaining scores temporal rows. The spread reduces through the blas
    // `Tensor/blas#PROVIDER_CLAIMS` `OnlineStat` fourth-order stream under an explicit `MomentNormalizer`,
    // never a page-local mean-then-squares pass that leaves its Bessel convention unstated on the report.
    public static Fin<ValidationReport> Validate(Estimator estimator, Design design, EstimatorPolicy policy, int folds, IClock clock, DenseSubstrate substrate) =>
        folds < 2 || folds > design.Rows
            ? Fin.Fail<ValidationReport>(ComputeFault.Create($"<validate-folds:{folds}/{design.Rows}>"))
            : (estimator is Estimator.Temporal { Spec.Forecasts: true }
                ? ForwardChain(estimator, design, policy, folds, clock, substrate)
                : estimator.Family == EstimatorFamily.Regression || estimator.Family == EstimatorFamily.Classify
                    ? HeldOut(estimator, design, policy, folds, clock, substrate)
                    : Fin.Fail<Seq<double>>(ComputeFault.Create($"<validate-no-heldout-score:{estimator.Key}>")))
            .Map(quality => {
                OnlineStat stat = quality.Fold(OnlineStat.Empty, static (acc, q) => acc.Push(q));
                return new ValidationReport(
                    estimator, quality, stat.Mean, Math.Sqrt(stat.Variance(MomentNormalizer.Population)),
                    MomentNormalizer.Population, folds, clock.GetCurrentInstant());
            });

    // One selection fold over a candidate policy set: each candidate fits once for its criterion and validates
    // under the same k-fold or forward-chain axis `Validate` already owns, then both axes fold into ONE
    // ascending rank. A criterion is present only where the fitted row carries a likelihood; a heldout-only
    // family ranks on validation mean alone, so the two evidence axes never fabricate each other.
    public static Fin<SelectionReport> Select(
        Estimator estimator, Design design, Seq<EstimatorPolicy> candidates, SelectionPolicy selection, IClock clock, DenseSubstrate substrate) =>
        selection.Admit(candidates.Count).Bind(_ => candidates
            .TraverseM(policy => Scored(estimator, design, policy, selection, clock, substrate)).As()
            .Map(rows => Ranked(rows, selection, clock.GetCurrentInstant())));

    private static Fin<(EstimatorPolicy Policy, Option<double> Criterion, double Mean, double Spread)> Scored(
        Estimator estimator, Design design, EstimatorPolicy policy, SelectionPolicy selection, IClock clock, DenseSubstrate substrate) =>
        Fit(estimator, design, policy, clock, substrate)
            .Bind(model => Validate(estimator, design, policy, selection.Folds, clock, substrate)
                .Map(report => (
                    policy,
                    model.LogLikelihood.Map(ll => selection.Criterion.Score(ll, model.Carrier.ParameterCount, design.Rows)),
                    report.Mean,
                    report.Spread)));

    // The lane's ONE exact reduction, over every sum whose term count is the row count: a deviance total, a
    // null deviance, and a Gaussian log-likelihood at building-scale n each accumulate ~10⁶ terms whose partial
    // sums dwarf the individual addends, and the criterion gap that decides one candidate over another is
    // smaller than that reduction's own cancellation error. `PeterO.Numbers` `EFloat` sums under the
    // context-free `Add`, which is EXACT at arbitrary precision, and rounds exactly ONCE at the terminal
    // `RoundToPrecision(EContext.Binary64)` — a per-term rounding context re-introduces the drift it replaces.
    internal static double ExactSum(Seq<double> terms) =>
        terms.Fold(EFloat.Zero, static (sum, term) => sum.Add(EFloat.FromDouble(term)))
            .RoundToPrecision(EContext.Binary64)
            .ToDouble();

    // The estimator is FIXED across candidates, so the criterion is present on every row or none and the one
    // ordering key never mixes a criterion scale with a validation one.
    private static SelectionReport Ranked(
        Seq<(EstimatorPolicy Policy, Option<double> Criterion, double Mean, double Spread)> rows, SelectionPolicy selection, Instant at) {
        Seq<CandidateEvidence> ordered = toSeq(rows
            .OrderBy(row => row.Criterion.IfNone(() => -row.Mean))
            .Select(static (row, index) => new CandidateEvidence(row.Policy, row.Criterion, row.Mean, row.Spread, index)));
        return new SelectionReport(ordered, ordered[0].Policy, selection.Criterion, at);
    }

    public static ComputeReceipt Receipt(FittedModel model, CorrelationId correlation, Duration elapsed) =>
        new ComputeReceipt.Fit(model.Estimator.Family.Key, model.Estimator.Key, model.Carrier.ParameterCount, model.Iterations, model.Residual, model.Converged, model.Quality, model.Estimator.Metric, model.Carrier.RetainedRank) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    // --- [ADMISSION] ----------------------------------------------------------------------

    // The regression arm reads the link off the estimator's OWN override first and the family's canonical link
    // second, so `Link` moves the fitted objective and the predicted mean as one decision; a kind-owned link
    // column left the override reachable at predict and unreachable at fit.
    private static Fin<FitContext> Admitted(Estimator estimator, Design design, EstimatorPolicy policy, IClock clock, DenseSubstrate substrate) {
        (Option<EstimatorKind> kind, LinkFunction link, GlmFamily family, int rank, Option<ClusterShape> cluster, Option<TemporalSpec> temporal, Option<CurveSpec> curve) = estimator.Switch(
            regression: static r => (Optional(r.Kind), r.Link.IfNone(r.Family.Canonical), r.Family, 0, Option<ClusterShape>.None, Option<TemporalSpec>.None, Option<CurveSpec>.None),
            curve: static c => (Option<EstimatorKind>.None, LinkFunction.Identity, GlmFamily.Gaussian, 0, Option<ClusterShape>.None, Option<TemporalSpec>.None, Optional(c.Spec)),
            reduction: static d => (Optional(d.Kind), LinkFunction.Identity, GlmFamily.Gaussian, d.Rank, Option<ClusterShape>.None, Option<TemporalSpec>.None, Option<CurveSpec>.None),
            cluster: static c => (Optional(c.Kind), LinkFunction.Identity, GlmFamily.Gaussian, 0, Optional(c.Shape), Option<TemporalSpec>.None, Option<CurveSpec>.None),
            classify: static c => (Optional(c.Kind), LinkFunction.Identity, GlmFamily.Gaussian, 0, Option<ClusterShape>.None, Option<TemporalSpec>.None, Option<CurveSpec>.None),
            temporal: static t => (Option<EstimatorKind>.None, LinkFunction.Identity, GlmFamily.Gaussian, 0, Option<ClusterShape>.None, Optional(t.Spec), Option<CurveSpec>.None));
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
            .Bind(_ => curve.Map(spec => spec.Admit(design)).IfNone(Fin.Succ(unit)))
            .Map(_ => new FitContext(estimator, design, clock, substrate, policy, link, family, driver, rank, cluster, temporal, curve))
            .Bind(ctx => kind.Map(row => row.Admit(ctx))
                .IfNone(() => curve.IsSome ? RealResponse(ctx) : TemporalDesign(ctx))
                .Map(_ => ctx));
    }

    internal static Fin<Unit> RealResponse(FitContext context) =>
        context.Design.Targets.IsSome ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create("<estimator-needs-targets>"));

    internal static Fin<Unit> RegularizedResponse(FitContext context) =>
        RealResponse(context).Bind(_ => context.Case<EstimatorPolicy.Regression>()).Bind(static p => PositiveOrZero(p.Regularization, "regularization"));

    internal static Fin<Unit> IterativeResponse(FitContext context) =>
        RegularizedResponse(context).Bind(_ => context.Case<EstimatorPolicy.Regression>())
            .Bind(static p => Positive(p.LearningRate, "learning-rate").Bind(_ => p.Budget.Admit()));

    // The response support is the FAMILY's — count for poisson, unit-interval proportion for binomial, positive
    // for gamma and inverse-gaussian, the real line for gaussian — so one admission row serves every GLM cell
    // and a per-family gate is the deleted form. Gamma under the inverse link additionally owes η > 0, which is
    // an admission on the FITTED linear predictor and therefore lands after the fit, not here.
    internal static Fin<Unit> GlmResponse(FitContext context) =>
        IterativeResponse(context).Bind(_ => context.Family.Admit(context.Design));

    internal static Fin<Unit> ReductionDesign(FitContext context) =>
        context.Case<EstimatorPolicy.Reduction>().Bind(static p =>
            p.EnergyFraction > 0.0 && p.EnergyFraction <= 1.0 ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create("<reduction-energy>")));

    internal static Fin<Unit> KernelReductionDesign(FitContext context) =>
        ReductionDesign(context)
            .Bind(_ => ScaleCeiling.Quadratic.Admit(context.Rows))
            .Bind(_ => context.Case<EstimatorPolicy.Reduction>()).Bind(static p => p.Parameters.Admit());

    internal static Fin<Unit> NonNegativeReductionDesign(FitContext context) =>
        NonNegativeFeatures(context.Design).Bind(_ => context.Case<EstimatorPolicy.Reduction>()).Bind(static p => p.Budget.Admit());

    internal static Fin<Unit> GroupingDesign(FitContext context) =>
        Groups(context).Bind(_ => context.Case<EstimatorPolicy.Grouping>()).Bind(static p => p.Budget.Admit());

    internal static Fin<Unit> MixtureDesign(FitContext context) =>
        Groups(context).Bind(_ => context.Case<EstimatorPolicy.Grouping>())
            .Bind(static p => Positive(p.Ridge, "mixture-ridge").Bind(_ => p.Budget.Admit()));

    // Average linkage rescans every surviving cluster pair per merge, so the sweep is cubic in the row count
    // and carries no budget of its own — the ceiling IS its budget.
    internal static Fin<Unit> LinkageDesign(FitContext context) =>
        Groups(context).Bind(_ => ScaleCeiling.Cubic.Admit(context.Rows));

    // The region query rescans every row per seed, so reachability is quadratic in both time and the frontier.
    internal static Fin<Unit> DensityDesign(FitContext context) =>
        ScaleCeiling.Quadratic.Admit(context.Rows)
            .Bind(_ => context.Cluster.ToFin(ComputeFault.Create("<density-shape-missing>")))
            .Bind(shape => shape.Switch(
                partitioned: static _ => Fin.Fail<Unit>(ComputeFault.Create("<density-shape-partitioned>")),
                density: _ => context.Case<EstimatorPolicy.Grouping>().Bind(p => Positive(p.Radius, "density-radius")
                    .Bind(_ => p.Neighbors < context.Rows ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create("<density-neighbors>"))))));

    private static Fin<Unit> NonNegativeFeatures(Design design) =>
        TensorPrimitives.IsNegativeAny<double>(design.Features.AsColumnMajorArray() ?? design.Features.ToColumnMajorArray())
            ? Fin.Fail<Unit>(ComputeFault.Create("<estimator-negative-features>"))
            : Fin.Succ(unit);

    internal static Fin<Unit> ClassLabels(FitContext context) =>
        ResponseGate(context.Design, static y =>
            TensorPrimitives.IsIntegerAll<double>(y.AsArray() ?? y.ToArray()) && y.Distinct().Take(2).Count() == 2,
            "class-labels");

    // The leave-one-out training score walks every row against every other, so the FIT is quadratic even
    // though the store itself is lazy.
    internal static Fin<Unit> NeighborhoodDesign(FitContext context) =>
        ClassLabels(context)
            .Bind(_ => ScaleCeiling.Quadratic.Admit(context.Rows))
            .Bind(_ => context.Case<EstimatorPolicy.Classification>())
            .Bind(p => p.Neighbors < context.Rows ? Fin.Succ(unit) : Fin.Fail<Unit>(ComputeFault.Create("<knn-neighbors>")));

    // Both margin rows materialize an n×n Gram — LS-SVM factors it whole, SMO caches its columns — so the
    // quadratic ceiling gates the memory before either mechanism allocates.
    internal static Fin<Unit> MarginDesign(FitContext context) =>
        ClassLabels(context)
            .Bind(_ => ScaleCeiling.Quadratic.Admit(context.Rows))
            .Bind(_ => context.Case<EstimatorPolicy.Classification>())
            .Bind(static p => Positive(p.Regularization, "margin-regularization")
                .Bind(_ => Positive(p.Box, "margin-box"))
                .Bind(_ => Positive(p.KktTolerance, "margin-kkt"))
                .Bind(_ => p.Parameters.Admit()));

    private static Fin<Unit> TemporalDesign(FitContext context) =>
        context.Temporal.ToFin(ComputeFault.Create("<temporal-spec-missing>"))
            .Bind(spec => spec.Admit(context.Design.Rows, context.Design.Columns))
            .Bind(_ => context.Case<EstimatorPolicy.Temporal>())
            .Bind(p => context.Temporal.Map(static spec => spec is TemporalSpec.BayesianOnline).IfNone(false) && p.Budget.MaxIterations < 2
                ? Fin.Fail<Unit>(ComputeFault.Create($"<bayesian-run-length:{p.Budget.MaxIterations}>"))
                : p.Budget.Admit());

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

    internal static Fin<FittedModel> Ridged(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Regression>().Bind(p => ClosedForm(ctx, tikhonov: p.Regularization));

    // OLS/ridge share intercept-augmented thin-QR; ridge stacks the unpenalized-intercept `√λ·I` block.
    // `λ = 0` selects the unstacked identity without a mode flag.
    private static Fin<FittedModel> ClosedForm(FitContext ctx, double tikhonov) =>
        ctx.Case<EstimatorPolicy.Regression>().Bind(p => Supervised(ctx).Bind(y => {
            Matrix<double> design = Intercept(ctx.Design.Features);
            Matrix<double> a = tikhonov > 0.0 ? design.Stack(Tikhonov(design.ColumnCount, tikhonov)) : design;
            Vector<double> b = tikhonov > 0.0 ? Vector<double>.Build.Dense(design.RowCount + design.ColumnCount, i => i < design.RowCount ? y[i] : 0.0) : y;
            return DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), a, b, TolerancePolicy.Derive(a, b), ctx.Substrate)
                .Bind(solved => Build(ctx, p, y, Split(solved.X), 0.0, 1, true));
        }));

    // One kernel serves every curve row: the row supplies its coefficient fit and its evaluator, and the shared body
    // owns capture, finiteness, and the uniform quality pair. `RSquared` scores the surrogate on the ORIGINAL scale
    // rather than the linearized one the library fits on, so an exponential row's quality is comparable with a
    // polynomial's; `StandardError` reads the row's own fitted-parameter count as its degrees of freedom.
    internal static Fin<FittedModel> CurveFit(FitContext ctx, CurveSpec spec) =>
        Supervised(ctx).Bind(y => {
            double[] x = ctx.Design.Features.Column(0).AsArray() ?? ctx.Design.Features.Column(0).ToArray();
            double[] observed = y.AsArray() ?? y.ToArray();
            return Try.lift(() => spec.Form.Fit(spec, x, observed)).Run()
                .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<curve-fit:{error.Message}>"))
                .Bind(coefficients => coefficients.Length == spec.Terms && TensorPrimitives.IsFiniteAll<double>(coefficients)
                    ? Fin.Succ(Vector<double>.Build.DenseOfArray(coefficients))
                    : Fin.Fail<Vector<double>>(ComputeFault.Create($"<curve-coefficients:{coefficients.Length}/{spec.Terms}>")))
                .Map(coefficients => {
                    double[] modelled = [.. x.Select(value => spec.Form.Evaluate(spec, coefficients, value))];
                    return new FittedModel(
                        ctx.Estimator,
                        new EstimatorModel.Curve(spec, coefficients),
                        GoodnessOfFit.RSquared(modelled, observed),
                        GoodnessOfFit.StandardError(modelled, observed, spec.Terms),
                        None,
                        1,
                        true,
                        ctx.Clock.GetCurrentInstant());
                });
        });

    internal static Fin<FittedModel> Penalized(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Regression>().Bind(p => Supervised(ctx).Bind(y =>
            IterativeEngine.Minimize(IterativeEngine.Lasso(p.Regularization), Intercept(ctx.Design.Features), y, ctx.Driver, p.LearningRate, p.Budget)
                .Bind(fit => Build(ctx, p, y, Split(fit.Theta), fit.Loss, fit.Iterations, fit.Converged))));

    internal static Fin<FittedModel> Deviance(FitContext ctx, Func<Tensor, Tensor, Tensor> deviance) =>
        ctx.Case<EstimatorPolicy.Regression>().Bind(p => Supervised(ctx).Bind(y =>
            IterativeEngine.Minimize((theta, x, yy) => deviance(x.matmul(theta), yy), Intercept(ctx.Design.Features), y, ctx.Driver, p.LearningRate, p.Budget)
                .Bind(fit => Build(ctx, p, y, Split(fit.Theta), fit.Loss, fit.Iterations, fit.Converged))));

    // ONE builder for every linear row, closed-form and iterative alike: quality is the family's own
    // deviance-explained (Gaussian∘identity recovers R² exactly, so no row needs a second metric), and the
    // residual is the link-and-family-weighted Pearson dispersion. The linear predictor crosses the link's
    // domain gate BEFORE the carrier lands, so an LBFGS step that walked a Gamma inverse-link fit to η ≤ 0
    // refuses by name instead of publishing a carrier whose every prediction is a signed infinity.
    private static Fin<FittedModel> Build(
        FitContext ctx, EstimatorPolicy.Regression policy, Vector<double> y,
        (double Intercept, Vector<double> Slopes) split, double loss, int iterations, bool converged) {
        Vector<double> eta = ctx.Design.Features.Multiply(split.Slopes).Add(split.Intercept);
        return ctx.Link.Domain(eta).Map(_ => {
            Vector<double> predicted = eta.Map(ctx.Link.Mean);
            // Pearson dispersion √(Σ (yᵢ−μᵢ)²/V(μᵢ)/(n−p)): the FAMILY's V(μ) weights each squared residual, so a
            // binomial fit scores on its μ(1−μ)/m scale and gaussian (V=1) recovers the ordinary standard error.
            // Never an unweighted RMSE blind to the family variance — that residual is the deleted decorative form.
            int dispersionDof = Math.Max(1, y.Count - split.Slopes.Count - 1);
            double dispersion = Math.Sqrt(ExactSum(toSeq(Enumerable.Range(0, y.Count)
                .Select(i => (y[i] - predicted[i]) * (y[i] - predicted[i]) / ctx.Family.Variance(predicted[i], policy.Weight)))) / dispersionDof);
            double deviance = ctx.Family.Total(y, predicted, policy.Weight);
            EstimatorModel.Linear carrier = new(split.Slopes, split.Intercept, ctx.Link, ctx.Family, deviance / Math.Max(1e-12, dispersion * dispersion));
            // The log-likelihood the criterion reads is −D/(2φ̂) with the family normalizer dropped: it is a
            // constant in θ that cancels between candidates of one family, and no candidate set spans two.
            return new FittedModel(
                ctx.Estimator, carrier, ctx.Family.Explained(y, predicted, policy.Weight),
                double.IsFinite(dispersion) ? dispersion : loss,
                Some(-0.5 * carrier.ScaledDeviance), iterations, converged, ctx.Clock.GetCurrentInstant());
        });
    }

    // --- [REDUCTION] ----------------------------------------------------------------------

    internal static Fin<FittedModel> Principal(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Reduction>().Bind(policy => PrincipalAdmitted(ctx, policy));

    private static Fin<FittedModel> PrincipalAdmitted(FitContext ctx, EstimatorPolicy.Reduction policy) {
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
                int rank = Retain(singular, total, Math.Min(ctx.Rank <= 0 ? singular.Count : ctx.Rank, singular.Count), policy.EnergyFraction);
                Matrix<double> components = f.Decomposition.VT.SubMatrix(0, rank, 0, x.ColumnCount);
                double energy = total > 0.0 ? singular.SubVector(0, rank).PointwisePower(2.0).Sum() / total : 0.0;
                EstimatorModel.Basis carrier = new(components, singular.SubVector(0, rank), mean, energy);
                return Fin.Succ(new FittedModel(ctx.Estimator, carrier, energy, 1.0 - energy, None, 1, true, ctx.Clock.GetCurrentInstant()));
            }));
    }

    // Kernel-PCA: the centered Gram of the policy's OWN `KernelRow` is the operand and its top eigenvectors the
    // duals, so a linear, polynomial, or sigmoid kernel-PCA is a policy value rather than a second kernel row;
    // out-of-sample projection double-centers the test/train kernel against the stored row/grand means.
    internal static Fin<FittedModel> KernelPrincipal(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Reduction>().Bind(policy => KernelPrincipalAdmitted(ctx, policy));

    private static Fin<FittedModel> KernelPrincipalAdmitted(FitContext ctx, EstimatorPolicy.Reduction policy) {
        Matrix<double> x = ctx.Design.Features;
        int n = x.RowCount;
        Matrix<double> gram = policy.Kernel.Gram(x, x, policy.Parameters);
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
                EstimatorModel.KernelBasis carrier = new(x, alphas, eigen, policy.Kernel, policy.Parameters, rowMean, grandMean);
                double captured = eigen.Sum() / Math.Max(1e-12, values.Map(Math.Abs).Sum());
                return Fin.Succ(new FittedModel(ctx.Estimator, carrier, captured, 1.0 - captured, None, 1, true, ctx.Clock.GetCurrentInstant()));
            }));
    }

    // NMF (Lee–Seung multiplicative updates): X ≈ W·H, W,H ≥ 0, minimizing the Frobenius reconstruction.
    internal static Fin<FittedModel> NonNegative(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Reduction>().Map(policy => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount, m = x.ColumnCount, k = Math.Max(1, ctx.Rank);
            Matrix<double> w = Matrix<double>.Build.Random(n, k, 1).PointwiseAbs().Add(1e-3);
            Matrix<double> h = Matrix<double>.Build.Random(k, m, 2).PointwiseAbs().Add(1e-3);
            double residual = double.MaxValue;
            int iter = 0;
            bool converged = false;
            for (; iter < policy.Budget.MaxIterations; iter++) {
                h = h.PointwiseMultiply((w.TransposeThisAndMultiply(x)).PointwiseDivide(w.TransposeThisAndMultiply(w).Multiply(h).Add(1e-12)));
                w = w.PointwiseMultiply((x.TransposeAndMultiply(h)).PointwiseDivide(w.Multiply(h.TransposeAndMultiply(h)).Add(1e-12)));
                double next = (x - w.Multiply(h)).FrobeniusNorm();
                converged = Math.Abs(residual - next) < policy.Budget.Tolerance;
                residual = next;
                if (converged) { iter++; break; }
            }
            return new FittedModel(ctx.Estimator, new EstimatorModel.Factors(w, h), -residual, residual, None, iter, converged, ctx.Clock.GetCurrentInstant());
        });

    // --- [CLUSTER] ------------------------------------------------------------------------

    internal static Fin<FittedModel> Lloyd(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Grouping>().Bind(policy => Groups(ctx).Map(k => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount;
            Matrix<double> centroids = Seed(x, k);
            int[] labels = new int[n];
            int iter = 0;
            bool moved = true;
            for (; iter < policy.Budget.MaxIterations && moved; iter++) {
                moved = false;
                for (int i = 0; i < n; i++) {
                    int best = Nearest(x.Row(i), centroids);
                    if (best != labels[i]) { labels[i] = best; moved = true; }
                }
                centroids = Recenter(x, labels, k, centroids);
            }
            double inertia = Inertia(x, labels, centroids);
            return new FittedModel(ctx.Estimator, new EstimatorModel.Partition(centroids, [.. labels]), -inertia, inertia / Math.Max(1, n), None, iter, !moved, ctx.Clock.GetCurrentInstant());
        }));

    // GMM-EM as a Fin-threaded fold: each EmStep computes responsibilities through the gated Cholesky log-density and re-estimates (weights, means, covariances), short-circuiting once the log-likelihood stalls.
    // Cholesky failure on a degenerate covariance aborts the whole fit through the rail.
    internal static Fin<FittedModel> ExpectationMaximization(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Grouping>().Bind(policy => Groups(ctx).Bind(k => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount, dim = x.ColumnCount;
            Fin<(Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights, double LogLik, int Iterations, bool Converged)> seed = Fin.Succ((
                Means: Seed(x, k),
                Covariances: toSeq(Enumerable.Range(0, k).Select(_ => (Matrix<double>)Matrix<double>.Build.DiagonalIdentity(dim))),
                Weights: Vector<double>.Build.Dense(k, 1.0 / k),
                LogLik: double.NegativeInfinity, Iterations: 0, Converged: false));
            return toSeq(Enumerable.Range(0, policy.Budget.MaxIterations))
                .Fold(seed, (acc, _) => acc.Bind(s => s.Converged ? acc : EmStep(x, s, dim, policy.Ridge, policy.Budget.Tolerance)))
                // The EM evidence IS the observed-data log-likelihood, so the mixture row is the one unsupervised
                // carrier an information criterion can score without fabricating a likelihood from a quality score.
                .Map(s => new FittedModel(ctx.Estimator, new EstimatorModel.Mixture(s.Means, s.Covariances, s.Weights), s.LogLik,
                    double.IsFinite(s.LogLik) ? -s.LogLik / Math.Max(1, n) : double.MaxValue,
                    double.IsFinite(s.LogLik) ? Some(s.LogLik) : None, s.Iterations, s.Converged, ctx.Clock.GetCurrentInstant()));
        }));

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

    internal static Fin<FittedModel> Reachability(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Grouping>().Map(policy => ReachabilityAdmitted(ctx, policy));

    private static FittedModel ReachabilityAdmitted(FitContext ctx, EstimatorPolicy.Grouping policy) {
        Matrix<double> x = ctx.Design.Features;
        int n = x.RowCount, minPts = policy.Neighbors;
        double eps = policy.Radius;
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
        return new FittedModel(ctx.Estimator, new EstimatorModel.Density(x, [.. labels], [.. core], eps), cluster + 1, noise, None, 1, true, ctx.Clock.GetCurrentInstant());
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
        return new FittedModel(ctx.Estimator, new EstimatorModel.Partition(centroids, [.. labels]), -spread, spread / Math.Max(1, n), None, n - clusters.Count, true, ctx.Clock.GetCurrentInstant());
    });

    // --- [CLASSIFY] -----------------------------------------------------------------------

    // One-vs-rest LS-SVM runs one regularized-Gram KKT solve per label; binary targets reduce to two machines.
    // The dense closed form yields NO zero duals, so every training row is a support row and the carrier's
    // support set is the full index range — the honest one for this mechanism, where SMO's is genuinely sparse.
    internal static Fin<FittedModel> MarginMachines(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Classification>().Bind(policy => Supervised(ctx).Bind(y => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount;
            int[] classes = [.. y.Select(static v => (int)v).Distinct().Order()];
            Matrix<double> gram = policy.Kernel.Gram(x, x, policy.Parameters);
            return toSeq(classes).TraverseM(label => Machine(gram, y, label, policy.Regularization, ctx.Substrate)).As()
                .Map(machines => {
                    EstimatorModel.Margin carrier = new(x, machines, policy.Kernel, policy.Parameters);
                    double accuracy = Enumerable.Range(0, n).Count(i => Strongest(x.Row(i), carrier) == (int)y[i]) / (double)n;
                    return new FittedModel(ctx.Estimator, carrier, accuracy, 1.0 - accuracy, None, 1, true, ctx.Clock.GetCurrentInstant());
                });
        }));

    // LS-SVM KKT system [[0, sᵀ],[s, K+I/γ]]·[b; α] = [0; 1] for one machine's ±1 indicator s.
    // The bordered matrix is symmetric INDEFINITE — the zero corner alone guarantees one negative eigenvalue —
    // so it rides `FactorRoute.SquarePivoting` and `Admission.Definite` never gates it; routing it to the
    // definite kernel refuses a system that is exactly solvable.
    private static Fin<(int Label, ImmutableArray<int> Support, Vector<double> Duals, double Bias)> Machine(
        Matrix<double> gram, Vector<double> y, int label, double regularization, DenseSubstrate substrate) {
        int n = gram.RowCount;
        Vector<double> signed = Vector<double>.Build.Dense(n, i => (int)y[i] == label ? 1.0 : -1.0);
        Matrix<double> kkt = Matrix<double>.Build.Dense(n + 1, n + 1, (i, j) =>
            i == 0 && j == 0 ? 0.0
            : i == 0 ? signed[j - 1]
            : j == 0 ? signed[i - 1]
            : gram[i - 1, j - 1] + (i == j ? 1.0 / regularization : 0.0));
        Vector<double> rhs = Vector<double>.Build.Dense(n + 1, i => i == 0 ? 0.0 : 1.0);
        return DenseRoute.Solve(new FactorRoute.SquarePivoting(), kkt, rhs, TolerancePolicy.Derive(kkt, rhs), substrate)
            .Map(solved => (label, ImmutableArray.CreateRange(Enumerable.Range(0, n)),
                Vector<double>.Build.Dense(n, i => solved.X[i + 1] * signed[i]), solved.X[0]));
    }

    // kNN is the lazy store; quality is LEAVE-ONE-OUT accuracy (each row voted with itself excluded), because
    // plain training accuracy of a 1-NN-containing vote is unconditionally perfect — evidence, not decoration.
    internal static Fin<FittedModel> Neighborhood(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Classification>().Bind(policy => Supervised(ctx).Map(y => {
            Matrix<double> x = ctx.Design.Features;
            EstimatorModel.Neighbors carrier = new(x, [.. y.Select(static v => (int)v)], policy.Neighbors);
            double accuracy = Enumerable.Range(0, x.RowCount).Count(i => Vote(x.Row(i), carrier, exclude: i) == (int)y[i]) / (double)x.RowCount;
            return new FittedModel(ctx.Estimator, carrier, accuracy, 1.0 - accuracy, None, 0, true, ctx.Clock.GetCurrentInstant());
        }));

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
            return new FittedModel(ctx.Estimator, carrier, accuracy, 1.0 - accuracy, None, 1, true, ctx.Clock.GetCurrentInstant());
        });

    // --- [SEQUENTIAL_MINIMAL_OPTIMIZATION] -------------------------------------------------

    // One-vs-rest C-SVM: per label the dual min ½αᵀQα − eᵀα subject to 0 ≤ αᵢ ≤ C and yᵀα = 0, with
    // Qᵢⱼ = yᵢyⱼK(xᵢ,xⱼ). Where LS-SVM equalizes every constraint into one dense solve and yields a fully dense
    // dual, the box constraint here leaves α sparse — the hard-margin support set — so the carrier's decision
    // sum walks the support alone and a large training set predicts in support-set time, not row-count time.
    internal static Fin<FittedModel> SequentialMinimal(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Classification>().Bind(policy => Supervised(ctx).Bind(y => {
            Matrix<double> x = ctx.Design.Features;
            int[] classes = [.. y.Select(static v => (int)v).Distinct().Order()];
            KernelCache cache = new(x, policy.Kernel, policy.Parameters, ctx.Rows);
            return toSeq(classes).TraverseM(label => Smo(cache, y, label, policy)).As()
                .Map(machines => {
                    EstimatorModel.Margin carrier = new(x, machines.Map(static m => m.Machine), policy.Kernel, policy.Parameters);
                    double accuracy = Enumerable.Range(0, x.RowCount).Count(i => Strongest(x.Row(i), carrier) == (int)y[i]) / (double)x.RowCount;
                    // The receipt reads SMO's own terminal evidence: `Iterations` is the total step count across
                    // the one-vs-rest machines, `Residual` the WORST final gap m(α) − M(α), and `Converged` that
                    // every machine's gap crossed ε. A step count without its gap reports a budget, not a verdict.
                    return new FittedModel(
                        ctx.Estimator, carrier, accuracy,
                        machines.Fold(0.0, static (worst, m) => Math.Max(worst, m.Gap)), None,
                        machines.Fold(0, static (total, m) => total + m.Steps),
                        machines.ForAll(m => m.Gap <= policy.KktTolerance),
                        ctx.Clock.GetCurrentInstant());
                });
        }));

    // A byte-budgeted column LRU: SMO touches exactly two kernel COLUMNS per step and revisits the same working
    // indices for long stretches, so a column cache is the whole memory story and the full n×n Gram is the one
    // allocation the box-constrained route exists to avoid — an n of 32_768 is 8 GiB of Gram, 256 KiB of column.
    private sealed class KernelCache(Matrix<double> x, KernelRow row, KernelParams parameters, int rows) {
        private const long ByteBudget = 512L << 20;

        private readonly int budget = (int)Math.Clamp(ByteBudget / (8L * rows), 2L, rows);
        private readonly Dictionary<int, double[]> columns = [];
        private readonly LinkedList<int> recency = new();
        private readonly Vector<double> diagonal = row.Diagonal(x, parameters);

        internal int Rows => rows;

        internal double Diagonal(int i) => diagonal[i];

        internal double[] Column(int i) {
            if (columns.TryGetValue(i, out double[]? held)) {
                recency.Remove(i);
                recency.AddFirst(i);
                return held;
            }
            while (columns.Count >= budget && recency.Last is { } evicted) {
                columns.Remove(evicted.Value);
                recency.RemoveLast();
            }
            double[] built = [.. Enumerable.Range(0, rows).Select(t => row.At(x.Row(t), x.Row(i), parameters))];
            columns[i] = built;
            recency.AddFirst(i);
            return built;
        }
    }

    // The dual iterate: α, the gradient Gₜ = Σₛ Qₜₛαₛ − 1 maintained incrementally, the ±1 label indicator, and
    // the shrinking working set. `Gap` is the measured KKT violation m(α) − M(α) the terminal verdict reads.
    private sealed record SmoState(double[] Alpha, double[] Gradient, double[] Signed, bool[] Active, int Steps, double Gap);

    private static Fin<((int Label, ImmutableArray<int> Support, Vector<double> Duals, double Bias) Machine, int Steps, double Gap)> Smo(
        KernelCache cache, Vector<double> y, int label, EstimatorPolicy.Classification policy) {
        int n = cache.Rows;
        SmoState seed = new(
            Alpha: new double[n],
            Gradient: [.. Enumerable.Repeat(-1.0, n)],                     // α = 0 seeds G = Q·0 − e = −e.
            Signed: [.. Enumerable.Range(0, n).Select(i => (int)y[i] == label ? 1.0 : -1.0)],
            Active: [.. Enumerable.Repeat(true, n)],
            Steps: 0,
            Gap: double.MaxValue);
        return toSeq(Enumerable.Range(0, policy.Budget.MaxIterations))
            .Fold(Fin.Succ(seed), (acc, _) => acc.Bind(s => s.Gap <= policy.KktTolerance ? acc : Advance(s, cache, policy)))
            .Map(s => (Machine(s, policy.Box, label), s.Steps, s.Gap));
    }

    private static Fin<SmoState> Advance(SmoState state, KernelCache cache, EstimatorPolicy.Classification policy) {
        SmoState measured = Measured(state, cache, policy);
        return measured.Gap <= policy.KktTolerance
            ? Fin.Succ(measured)
            : Step(measured, cache, policy).Map(next => policy.Shrinking
                ? Shrunk(next, policy.Box) with { Steps = measured.Steps + 1 }
                : next with { Steps = measured.Steps + 1 });
    }

    // The stop is the KKT VIOLATION GAP m(α) − M(α) ≤ ε, NEVER a coefficient or objective delta: the dual is flat
    // near its optimum, so a step-size stall certifies an α that still violates the KKT conditions and the machine
    // it publishes misclassifies exactly the rows the stall left violating. Shrinking makes the shrunk gap a
    // LOWER bound only — parked gradients go stale the moment the working set moves — so once that gap closes to
    // 10ε the full gradient RECONSTRUCTS, every index unshrinks, and the gap re-measures over all of them. That
    // reconstruction is mandatory, never an option: without it the verdict is read off stale evidence.
    private static SmoState Measured(SmoState state, KernelCache cache, EstimatorPolicy.Classification policy) {
        (double up, double low) = Violation(state, policy.Box, shrunk: policy.Shrinking);
        if (!policy.Shrinking || up - low > policy.KktTolerance * 10.0) { return state with { Gap = up - low }; }
        SmoState full = Unshrunk(state, cache);
        (double fullUp, double fullLow) = Violation(full, policy.Box, shrunk: false);
        return full with { Gap = fullUp - fullLow };
    }

    // WSS2 second-order working-set selection: i maximizes the up-violation −yᵢGᵢ, and j minimizes the projected
    // objective decrease −b²/a among the low indices strictly below it. First-order selection — the maximal
    // violating PAIR — converges an order of magnitude slower on an RBF kernel for the identical gap.
    private static Fin<SmoState> Step(SmoState state, KernelCache cache, EstimatorPolicy.Classification policy) {
        const double Tau = 1e-12;
        int n = cache.Rows;
        (int i, double best) = (-1, double.NegativeInfinity);
        for (int t = 0; t < n; t++) {
            double score = -state.Signed[t] * state.Gradient[t];
            if (state.Active[t] && Up(state, t, policy.Box) && score > best) { (i, best) = (t, score); }
        }
        if (i < 0) { return Fin.Fail<SmoState>(ComputeFault.Create("<smo-empty-up-set>")); }
        double[] ki = cache.Column(i);
        (int j, double gain) = (-1, double.MaxValue);
        for (int t = 0; t < n; t++) {
            if (!state.Active[t] || !Low(state, t, policy.Box)) { continue; }
            double violation = best + state.Signed[t] * state.Gradient[t];
            if (violation <= 0.0) { continue; }
            // Curvature a_it = Kᵢᵢ + Kₜₜ − 2Kᵢₜ is exactly zero for duplicate rows, so the τ floor is the ONE
            // guard keeping a degenerate pair from an infinite step rather than a NaN one — and it is a floor on
            // the DENOMINATOR alone, never a clamp on the step the box constraint already bounds.
            double curvature = Math.Max(Tau, cache.Diagonal(i) + cache.Diagonal(t) - 2.0 * ki[t]);
            double projected = -violation * violation / curvature;
            if (projected < gain) { (j, gain) = (t, projected); }
        }
        if (j < 0) { return Fin.Fail<SmoState>(ComputeFault.Create($"<smo-no-working-pair:{i}>")); }
        return Fin.Succ(Moved(state, cache, policy, i, j, ki, Math.Max(Tau, cache.Diagonal(i) + cache.Diagonal(j) - 2.0 * ki[j])));
    }

    // The EXACT two-variable step under the equality constraint yᵀα = 0: with s = yᵢ·yⱼ the pair slides along
    // αᵢ + αⱼ (equal labels) or αⱼ − αᵢ (opposite labels), so the clip bounds differ by label PARITY and a bare
    // [0, C] clamp per variable silently leaves the equality violated. αᵢ then recovers from the invariant, which
    // is why it needs no clamp of its own — the [L, H] bounds already place it inside the box.
    private static SmoState Moved(
        SmoState state, KernelCache cache, EstimatorPolicy.Classification policy, int i, int j, double[] ki, double curvature) {
        double si = state.Signed[i], sj = state.Signed[j], s = si * sj;
        double oldI = state.Alpha[i], oldJ = state.Alpha[j];
        double delta = (s * state.Gradient[i] - state.Gradient[j]) / curvature;
        (double lo, double hi) = s > 0.0
            ? (Math.Max(0.0, oldI + oldJ - policy.Box), Math.Min(policy.Box, oldI + oldJ))
            : (Math.Max(0.0, oldJ - oldI), Math.Min(policy.Box, policy.Box + oldJ - oldI));
        double newJ = Math.Clamp(oldJ + delta, lo, hi);
        double newI = oldI + s * (oldJ - newJ);
        double[] alpha = (double[])state.Alpha.Clone();
        (alpha[i], alpha[j]) = (newI, newJ);
        // Gradient maintenance costs exactly the two columns the step already fetched:
        // Gₜ += Qₜᵢ·Δαᵢ + Qₜⱼ·Δαⱼ with Qₜᵤ = yₜyᵤKₜᵤ. Recomputing G from α is O(n²) per step, which turns SMO
        // back into the dense solve it exists to replace.
        double[] kj = cache.Column(j);
        double[] gradient = (double[])state.Gradient.Clone();
        double di = newI - oldI, dj = newJ - oldJ;
        for (int t = 0; t < gradient.Length; t++) { gradient[t] += state.Signed[t] * (si * ki[t] * di + sj * kj[t] * dj); }
        return state with { Alpha = alpha, Gradient = gradient };
    }

    private static bool Up(SmoState state, int t, double box) =>
        (state.Signed[t] > 0.0 && state.Alpha[t] < box) || (state.Signed[t] < 0.0 && state.Alpha[t] > 0.0);

    private static bool Low(SmoState state, int t, double box) =>
        (state.Signed[t] > 0.0 && state.Alpha[t] > 0.0) || (state.Signed[t] < 0.0 && state.Alpha[t] < box);

    // m(α) = max over I_up of −y·G, M(α) = min over I_low of −y·G; the gap between them IS the KKT violation.
    private static (double Up, double Low) Violation(SmoState state, double box, bool shrunk) {
        double up = double.NegativeInfinity, low = double.PositiveInfinity;
        for (int t = 0; t < state.Alpha.Length; t++) {
            if (shrunk && !state.Active[t]) { continue; }
            double score = -state.Signed[t] * state.Gradient[t];
            if (Up(state, t, box)) { up = Math.Max(up, score); }
            if (Low(state, t, box)) { low = Math.Min(low, score); }
        }
        return (up, low);
    }

    // A BOUND index whose violation score falls outside the current [M(α), m(α)] band can be selected as neither
    // i nor j, so it parks. A free index never parks — its own gradient defines the bias — and parking is
    // reversible by construction, because `Unshrunk` restores every index before any verdict is read.
    private static SmoState Shrunk(SmoState state, double box) {
        (double up, double low) = Violation(state, box, shrunk: true);
        bool[] active = (bool[])state.Active.Clone();
        for (int t = 0; t < state.Alpha.Length; t++) {
            if (state.Alpha[t] > 0.0 && state.Alpha[t] < box) { continue; }
            double score = -state.Signed[t] * state.Gradient[t];
            active[t] = Up(state, t, box) ? score > low : score < up;
        }
        return state with { Active = active };
    }

    // Full gradient reconstruction G = Qα − e over the support alone: a parked index's gradient is stale by every
    // step taken since it parked, and the reconstruction is what makes the unshrunk gap a verdict rather than a guess.
    private static SmoState Unshrunk(SmoState state, KernelCache cache) {
        int n = cache.Rows;
        double[] gradient = [.. Enumerable.Repeat(-1.0, n)];
        for (int s = 0; s < n; s++) {
            if (state.Alpha[s] == 0.0) { continue; }
            double[] ks = cache.Column(s);
            for (int t = 0; t < n; t++) { gradient[t] += state.Signed[t] * state.Signed[s] * ks[t] * state.Alpha[s]; }
        }
        return state with { Gradient = gradient, Active = [.. Enumerable.Repeat(true, n)] };
    }

    // The bias reads off the FREE support vectors (0 < α < C), whose KKT conditions hold with equality, as
    // b = −mean(y·G) over that set. An empty free set means every support vector sits at a bound and the only
    // information left is the bracketing interval the final violation pair names: b = −(m(α) + M(α))/2.
    private static (int Label, ImmutableArray<int> Support, Vector<double> Duals, double Bias) Machine(SmoState state, double box, int label) {
        int n = state.Alpha.Length;
        int[] support = [.. Enumerable.Range(0, n).Where(t => state.Alpha[t] > 0.0)];
        int[] free = [.. support.Where(t => state.Alpha[t] < box)];
        (double up, double low) = Violation(state, box, shrunk: false);
        double bias = free.Length > 0
            ? -free.Average(t => state.Signed[t] * state.Gradient[t])
            : -(up + low) / 2.0;
        return (label, [.. support], Vector<double>.Build.Dense(n, t => state.Alpha[t] * state.Signed[t]), bias);
    }

    // --- [TEMPORAL] -----------------------------------------------------------------------

    // The Gaussian conditional log-likelihood at the maximizing σ̂², ℓ = −n/2·(ln 2πσ̂² + 1) — the one form every
    // innovation-variance row shares, so a criterion ranks AR against ARMA against Holt on one scale. `n` is the
    // CONDITIONAL count each row's own warmup leaves, never the raw series length, because a row that discarded
    // more warmup would otherwise score as though it had explained those observations.
    private static Option<double> Gaussian(int conditional, double variance) =>
        conditional > 0 && variance > 0.0 && double.IsFinite(variance)
            ? Some(-0.5 * conditional * (Math.Log(2.0 * Math.PI * variance) + 1.0))
            : None;

    // Pure AR(p): the lag design Y[t] = Σ φₖ·Y[t−k] solved through the dense-algebra thin-QR — the same closed-form route OLS rides, the AR coefficients its solution.
    internal static Fin<FittedModel> AutoRegress(FitContext ctx) {
        Vector<double> series = ctx.Design.Features.Column(0);
        int p = ctx.Temporal.Map(static spec => spec.History).IfNone(1), n = series.Count;
        Matrix<double> design = Matrix<double>.Build.Dense(n - p, p, (i, k) => series[p + i - 1 - k]);
        Vector<double> response = Vector<double>.Build.Dense(n - p, i => series[p + i]);
        return DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), design, response, TolerancePolicy.Derive(design, response), ctx.Substrate)
            .Map(solved => {
                Vector<double> phi = solved.X;
                Vector<double> residual = response - design.Multiply(phi);
                double variance = residual.DotProduct(residual) / Math.Max(1, n - p);
                Vector<double> tail = Vector<double>.Build.Dense(p, k => series[n - 1 - k]);
                return new FittedModel(ctx.Estimator, new EstimatorModel.Lag(phi, Vector<double>.Build.Dense(0), variance, tail, TimeSeriesModel.Ar), -variance, Math.Sqrt(variance), Gaussian(n - p, variance), 1, true, ctx.Clock.GetCurrentInstant());
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
                return new FittedModel(ctx.Estimator, new EstimatorModel.Lag(lm.Parameters.SubVector(0, p), lm.Parameters.SubVector(p, q), variance, tail, TimeSeriesModel.Arma), -variance, lm.Residual, Gaussian(n - p - q, variance), lm.Iterations, lm.Converged, ctx.Clock.GetCurrentInstant());
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
                return new FittedModel(ctx.Estimator, new EstimatorModel.Lag(Vector<double>.Build.DenseOfArray([alpha, beta]), Vector<double>.Build.Dense(0), variance, Vector<double>.Build.DenseOfArray([level, trend]), TimeSeriesModel.ExponentialSmoothing), -variance, lm.Residual, Gaussian(n - 2, variance), lm.Iterations, lm.Converged, ctx.Clock.GetCurrentInstant());
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
                return new FittedModel(ctx.Estimator, new EstimatorModel.Lag(Vector<double>.Build.DenseOfArray([Math.Exp(lm.Parameters[0]), Math.Exp(lm.Parameters[1])]), Vector<double>.Build.Dense(0), variance, Vector<double>.Build.DenseOfArray([level, slope]), TimeSeriesModel.StateSpace), -variance, Math.Sqrt(variance), Gaussian(n - 2, variance), lm.Iterations, lm.Converged, ctx.Clock.GetCurrentInstant());
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

    private static Fin<FittedModel> Baseline(FitContext ctx, TemporalSpec spec) =>
        ctx.Case<EstimatorPolicy.Temporal>().Bind(policy => BaselineAdmitted(ctx, spec, policy.Budget));

    private static Fin<FittedModel> BaselineAdmitted(FitContext ctx, TemporalSpec spec, FitBudget budget) {
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
            // A detector fits a BASELINE over the warmup prefix, not a model of the whole stream, so it carries
            // no series log-likelihood and an information criterion is unavailable rather than misread.
            return new FittedModel(ctx.Estimator, new EstimatorModel.Detector(mean, scale, spec, budget.MaxIterations), -nll, nll, None, 1, true, ctx.Clock.GetCurrentInstant());
        });
    }

    // --- [VALIDATION] ---------------------------------------------------------------------

    // Contiguous k-fold: fit on the complement, score the held-out fold by the family metric — R² for regression,
    // matched fraction for classification — so generalization is measured on rows the fit never saw.
    private static Fin<Seq<double>> HeldOut(Estimator estimator, Design design, EstimatorPolicy policy, int folds, IClock clock, DenseSubstrate substrate) =>
        toSeq(Enumerable.Range(0, folds)).TraverseM(fold => {
            (Design train, Design test) = design.Split(fold, folds);
            return Fit(estimator, train, policy, clock, substrate)
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
    // Negative RMSE preserves higher-is-better quality without future leakage. The forecast request is the step
    // count itself — the forecast carrier reads no evidence at all, so the fold hands it a horizon rather than a
    // zero matrix whose only load-bearing property was its row count.
    private static Fin<Seq<double>> ForwardChain(Estimator estimator, Design design, EstimatorPolicy policy, int folds, IClock clock, DenseSubstrate substrate) {
        Vector<double> series = design.Features.Column(0);
        int n = series.Count;
        return toSeq(Enumerable.Range(1, folds)).TraverseM(fold => {
            int cut = n * fold / (folds + 1), horizon = Math.Max(1, n * (fold + 1) / (folds + 1) - cut);
            return Design.Admit(Matrix<double>.Build.Dense(cut, 1, (i, _) => series[i]), None)
                .Bind(prefix => Fit(estimator, prefix, policy, clock, substrate))
                .Bind(model => Predict(model, horizon))
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

    // The decision walks the machine's OWN support set through the carrier's `KernelRow`, so an SMO machine
    // costs its support count and an LS-SVM machine its row count, and the kernel formula lives in exactly one
    // place — the row — rather than once at the Gram and again here where the two drift apart silently.
    private static double Decision(Vector<double> point, EstimatorModel.Margin margin, (int Label, ImmutableArray<int> Support, Vector<double> Duals, double Bias) machine) =>
        machine.Support.Sum(i => machine.Duals[i] * margin.Kernel.At(point, margin.Training.Row(i), margin.Parameters)) + machine.Bias;

    private static int Strongest(Vector<double> point, EstimatorModel.Margin margin) =>
        margin.Machines.Map(machine => (machine.Label, Score: Decision(point, margin, machine)))
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
        Matrix<double> gram = basis.Kernel.Gram(x, basis.Training, basis.Parameters);
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

- Owner: `StatisticalTest` `[SmartEnum<string>]` rows each bind ONE `Evaluate` kernel computing the statistic, the (possibly fractional) dof, and the p-value from its own tail — collapsing a parallel `Statistic`/`PValue`/`dof` helper trio into row data — with the row's `MinSamples` arity floor, so a two-sample kernel can never receive one sample; `TestResult` carries (statistic, p-value, decision, dof); `Hypothesis` is the static `Test` surface stamping through the injected `IClock`.
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
        string key, int minSamples, Func<Seq<ReadOnlyMemory<double>>, (double Statistic, double PValue, Option<double> Dof)> evaluate,
        Func<Seq<ReadOnlyMemory<double>>, Fin<Unit>> admit) : this(key) {
        MinSamples = minSamples;
        this.evaluate = evaluate;
        this.admit = admit;
    }

    private readonly Func<Seq<ReadOnlyMemory<double>>, (double Statistic, double PValue, Option<double> Dof)> evaluate;
    private readonly Func<Seq<ReadOnlyMemory<double>>, Fin<Unit>> admit;

    public int MinSamples { get; }

    internal (double Statistic, double PValue, Option<double> Dof) Evaluate(Seq<ReadOnlyMemory<double>> samples) => evaluate(samples);
    internal Fin<Unit> Admit(Seq<ReadOnlyMemory<double>> samples) => admit(samples);

    private static (double, double, Option<double>) StudentPooled(Seq<ReadOnlyMemory<double>> samples) {
        (double MeanA, double VarianceA, int CountA, double MeanB, double VarianceB, int CountB) pair = TwoSampleMoments(samples);
        double dof = pair.CountA + pair.CountB - 2;
        double pooled = ((pair.CountA - 1) * pair.VarianceA + (pair.CountB - 1) * pair.VarianceB) / dof;
        double standardError = Math.Sqrt(pooled) * Math.Sqrt(1.0 / pair.CountA + 1.0 / pair.CountB);
        return StudentTail(pair.MeanA - pair.MeanB, standardError, dof);
    }

    private static (double, double, Option<double>) Welch(Seq<ReadOnlyMemory<double>> samples) {
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

    private static (double, double, Option<double>) StudentTail(double difference, double standardError, double dof) {
        double t = difference / standardError;
        return (t, 2.0 * (1.0 - StudentT.CDF(0.0, 1.0, dof, Math.Abs(t))), Some(dof));
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

    private static (double, double, Option<double>) OneWayAnova(Seq<ReadOnlyMemory<double>> samples) {
        double[][] groups = [.. samples.Map(static g => g.ToArray())];
        int total = groups.Sum(static g => g.Length), k = groups.Length;
        double grand = groups.SelectMany(static g => g).Mean();
        double between = groups.Sum(g => g.Length * Math.Pow(g.Mean() - grand, 2)) / (k - 1);
        double within = groups.Sum(g => g.Sum(v => Math.Pow(v - g.Mean(), 2))) / (total - k);
        double f = between / within;
        return (f, 1.0 - FisherSnedecor.CDF(k - 1, total - k, f), Some((double)(k - 1)));
    }

    private static (double, double, Option<double>) ChiSquareGoodness(Seq<ReadOnlyMemory<double>> samples) {
        double[] observed = samples[0].ToArray();
        double[] expected = samples.Count > 1 ? samples[1].ToArray() : [.. observed.Select(_ => observed.Average())];
        double chi = Enumerable.Range(0, observed.Length).Sum(i => Math.Pow(observed[i] - expected[i], 2) / Math.Max(1e-12, expected[i]));
        double dof = observed.Length - 1;
        return (chi, 1.0 - ChiSquared.CDF(dof, chi), Some(dof));
    }

    // Two-sample Kolmogorov–Smirnov: sup gap of the empirical CDFs against the Kolmogorov asymptotic series — MathNet ships no Kolmogorov distribution, so the alternating exponential series is the kernel.
    private static (double, double, Option<double>) KolmogorovSmirnov(Seq<ReadOnlyMemory<double>> samples) {
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
        return (d, Math.Clamp(p, 0.0, 1.0), None);
    }

    // Mann–Whitney U via the tie-corrected, continuity-corrected normal approximation over the rank sum.
    private static (double, double, Option<double>) RankSum(Seq<ReadOnlyMemory<double>> samples) {
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
        return (u, 2.0 * (1.0 - Normal.CDF(0.0, 1.0, Math.Abs(z))), None);
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

// `Dof` is `None` on the distribution-free rows: the Kolmogorov series and the rank-sum normal approximation
// have no degrees of freedom at all, and a `0.0` in that slot reads as a computed zero rather than an absence.
public sealed record TestResult(StatisticalTest Test, double Statistic, double PValue, bool RejectNull, Option<double> Dof, Instant At);

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
        (double statistic, double pValue, Option<double> dof) = test.Evaluate(samples);
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

(none)
