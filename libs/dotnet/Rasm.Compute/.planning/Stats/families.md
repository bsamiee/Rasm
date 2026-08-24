# [COMPUTE_ESTIMATOR_FAMILIES]

Rasm.Compute statistical-learning ROWS: every generated vocabulary the `Stats/estimator#ESTIMATOR_LANE` contract dispatches on, and the mechanism body each row binds. `Stats/estimator` owns the uniform `Fit`/`Predict`/`Validate`/`Select` contract, its carriers, its admission, and its receipt; this page owns which estimators exist, what each one's parameters are, and how each one actually fits — so a new family is a ROW here and the contract page is untouched.

Every row column arrives through `[UseDelegateFromConstructor]`, so a new row must answer every column the family declares before it compiles and no roster carries a hand constructor, a private delegate field, and a forwarder method restating the same three facts. `TemporalSpec` and `CurveSpec` are the two parameterized generators: each case owns exactly the parameters its form cannot reconstruct and DERIVES the `TimeSeriesModel` or `RegressionForm` row that binds its kernel.

Dense factorizations ride `Tensor/blas#DENSE_ALGEBRA` — `DenseRoute.Solve`, `LevenbergMarquardt.Minimize` over `SolveOutcome<Vector<double>>`, `Admission.Definite`, `DenseOps.Decompose`; the bounded folds report the `Solver/contract#SOLVE_REQUEST` `Convergence` verdict; descriptive, regression, and distribution surfaces ride `MathNet.Numerics`; stochastic seeds ride the kernel `Rasm/Domain/identity` `Deterministic.Source`; exact criterion sums accumulate through `EstimatorFold.ExactSum`. Page is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[FAMILY_ROWS]: the metric, family, link, exponential-family, kernel, driver, kind, time-series, and curve rosters with their delegate columns; the `TemporalSpec`/`DetectorSpec`/`CurveSpec` generators; and `EstimatorKernels`, the per-row fit and detection bodies beside the torch-loss `IterativeEngine`.

## [02]-[FAMILY_ROWS]

- Owner: `FitMetric` `[SmartEnum<string>]` is the ONE fit-quality vocabulary — the receipt's `QualityMetric` column is a wire spelling, and twelve independent string literals across three rosters were twelve chances for it to drift; `EstimatorFamily` is the receipt family axis; `EstimatorKind` rows carry supervised fit behavior; `GlmFamily` rows carry one exponential-family law — exact unit deviance, variance function, canonical link, response support, and the θ-dependent torch deviance kernel; `LinkFunction` rows carry the inverse link in both the scalar and tensor arities; `KernelRow` rows carry one positive-definite kernel; `OptimDriver` rows carry one torch optimizer and whether it line-searches; `TimeSeriesModel` rows carry forecast or detector fit behavior beside the forecast projection each one owns; `RegressionForm` rows carry the coefficient fit and the evaluator for one curve shape; `TemporalSpec` is the parameterized temporal generator, `DetectorSpec` the three-case detection payload it carries, and `CurveSpec` the parameterized curve generator; `EstimatorKernels` owns every row-bound fit body and the three detector scorers; `IterativeEngine` owns torch-loss fitting.
- Cases: `FitMetric` r2 · deviance-explained · explained-energy · reconstruction-error · inertia · log-likelihood · cluster-count · accuracy · innovation-variance · baseline-log-likelihood; `EstimatorFamily` regression · reduction · cluster · classify · temporal; `EstimatorKind` owns the supervised/reduction/grouping/classification rows, `glm` being ONE row parameterized by `GlmFamily` and `c-svm` the sparse-dual sibling of the dense `svm`; `GlmFamily` gaussian · poisson · binomial · gamma · inverse-gaussian; `LinkFunction` identity · logit · log · inverse · inverse-squared; `KernelRow` linear · polynomial · rbf · sigmoid; `OptimDriver` lbfgs · adam; `TimeSeriesModel` ar · arma · exponential-smoothing · state-space · cusum · bayesian-online · correlated-residual; `RegressionForm` polynomial · exponential · power · logarithm · combination; `TemporalSpec` ar · arma · exponential-smoothing · state-space · detection; `DetectorSpec` cusum · bayesian-online · correlated-residual; `CurveSpec` polynomial · exponential · power · logarithm · combination.
- Law: `Supervised` is DERIVED, never declared. `Supervised == Family is Regression or Classify` holds for every row in the roster, so the column was fifteen restatements of a projection the family already answers and the sixteenth row could have contradicted it silently.
- Law: detection payload lives at ONE owner. `TemporalSpec.Detection` CARRIES a `DetectorSpec` rather than spelling three sibling cases beside four forecasting ones, so `EstimatorModel.Detector` narrows to the three-case union its scorer actually dispatches on — the widened seven-case carrier gave the `Detect` switch four arms that could not fire and gave `DetectorBaseline` a `spec.Model != model` re-check the derivation already forbade. The three `TimeSeriesModel` detector rows therefore bind ONE `DetectorBaseline` fit column and the three per-row forwarders that stood between them delete.
- Law: each spec case admits its OWN parameters. The deleted shared gate took `(warmup, rows, columns, bool range, double first, double second)` and collapsed three unrelated range violations onto one boolean and one fault slug, so a caller learned that something was out of range and never which knob — `DISCARDED_DISCRIMINANT` in its exact form.
- Entry: rows are reached through their columns alone — `kind.Fit(context)`/`kind.Admit(context)`, `model.Fit(context)`/`model.Forecast(lag, horizon)`, `form.Fit(spec, x, y)`/`form.Evaluate(spec, coefficients, x)`, `family.Deviance`/`Variance`/`Loss`/`Admit`, `link.Mean`/`Domain`, `row.At`/`Gram`/`Diagonal`, `driver.Bind`. `spec.Model`, `spec.Form`, `spec.History`, `spec.Terms`, and `spec.Admit` are the generator projections `Stats/estimator` reads before dispatch.
- Auto: every GLM cell is ONE composition — the family's θ-dependent unit-deviance kernel evaluated at the link's own tensor inverse, so a `Link` override changes fit and prediction together and no per-(family, link) loss row exists. Curve rows run one shared kernel that captures the library fit, proves the coefficient census and finiteness, and scores on the original response scale rather than the linearized one the library fits on. `c-svm` runs SMO with second-order working-set selection under a byte-budgeted column-LRU kernel cache. Temporal forecasting routes AR through thin QR and ARMA/Holt/state-space through `LevenbergMarquardt`; detection fits one admitted multivariate Gaussian baseline through `Admission.Definite`, then CUSUM folds whitened innovation magnitude, Bayesian-online maintains a budget-capped run-length posterior with conjugate known-covariance mean updates, and correlated-residual scoring reads a `ChiSquared.InvCDF` threshold over Mahalanobis evidence.
- Auto: every bounded fold reports the `Solver/contract#SOLVE_REQUEST` `Convergence` verdict and none of them falls through success-shaped. `Prelude.foldWhile` halts the EM and SMO folds at their own settled predicate rather than binding a no-op through the remaining budget — the SMO fold ran up to a million no-op binds past its own KKT gate — and `Convergence.Exhausted(budget)` is what a spent budget answers, so a GLM under a thousand-iteration cap can no longer publish an unconverged carrier as a success.
- Packages: MathNet.Numerics, TorchSharp, libtorch-cpu, HyperJet (temporal-fit exact-Jacobian scalar-AD — recurrences authored once over `DDScalar`, the LM hyperdual arm reading `GetGradient()`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project — `Deterministic.Source`, `Dimension`, `Tolerance`), BCL inbox
- Growth: a new exponential family is one `GlmFamily` row and the `glm` kind is untouched; a new link is one `LinkFunction` row carrying both arities; a new kernel is one `KernelRow` row every kernel consumer inherits; a new fit-quality reading is one `FitMetric` row. A new temporal modality is one `TemporalSpec` case deriving one `TimeSeriesModel` row and binding one kernel; a new detection modality is one `DetectorSpec` case whose scorer switch breaks at compile time; a new curve shape is one `CurveSpec` case deriving one `RegressionForm` row, with the shared kernel untouched. Per-model estimator classes, detector DTOs, per-family GLM kinds, per-kernel Gram helpers, and universal temporal knob records are rejected.
- Boundary: each row binds its genuine mechanism; forced SVD or torch-loss routing is rejected. Curve rows are univariate by construction — a multi-column design is a typed refusal, not a silent first-column read — and each row proves the support its own linearization needs, so a non-positive response never reaches an exponential log and a non-positive feature never reaches a power or logarithm log.
- Boundary: the GLM deviance is elementary — the exponential-family normalizer cancels between the fitted and the saturated model, so NO `MathNet.Numerics.Distributions` member enters the deviance, the deviance-explained metric, or the scaled deviance. `Poisson` and `Binomial` expose no `InvCDF` at all, `InverseGaussian` spells its instance quantile `InvCDF(double)` where every sibling spells `InverseCumulativeDistribution`, its `Median` Brent-solves and can throw, and `Gamma` is rate-parameterized (`WithShapeScale` is the scale form) — so a family that reached for a distribution instance would reach for four different contracts.
- Boundary: `LinkFunction.Inverse` is `g(μ)=1/μ` with domain `η > 0`, the statsmodels `InversePower` link; LBFGS steps out of that domain on the way to the optimum, so the Gamma inverse-link arm gates `η > 0` and refuses typed rather than returning a NaN loss — statsmodels ships `safe_links = [Log]` for exactly this reason and the log link stays the canonical route.
- Boundary: the LS-SVM bordered KKT system is symmetric INDEFINITE, so it rides `FactorRoute.SquarePivoting` and `Admission.Definite` never gates it; SMO materializes no factorization at all and gates only its second-order denominator `a_it > τ`.
- Boundary: stochastic seeds ride the kernel `Deterministic.Source(seed, lane)` and never a bare integer handed to `Matrix<double>.Build.Random`. The NMF factor pair drew from an unseeded process RNG under two magic lane integers, so two runs of one fit answered two factorizations and neither the receipt nor the carrier said why — `Solver/optimizer#OPTIMIZER_LANE` rules the seeded source for every stochastic kernel and this is one.
- Boundary: the SMO loops, the Lloyd assignment sweep, the agglomerative merge scan, the DBSCAN region walk, and the NMF multiplicative update are MEASURED numeric kernels: statement bodies, index arithmetic, and mutation-local scratch stay confined inside them and never reach domain flow, exactly as the wavelet and power-iteration kernels on the sibling signal page are exempted. What is NOT exempt is their terminal — every one of them reports a `Convergence` verdict its caller reads, so the exemption covers the body and never the answer.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// One fit-quality vocabulary the whole lane reads. The receipt's `QualityMetric` column is a WIRE spelling, and
// twelve literals scattered across three rosters meant twelve independent chances for `"r2"` to become `"R2"` on
// one row; a new reading is a ROW here, and a row that reuses an existing reading references it.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FitMetric {
    public static readonly FitMetric R2 = new("r2");
    public static readonly FitMetric DevianceExplained = new("deviance-explained");
    public static readonly FitMetric ExplainedEnergy = new("explained-energy");
    public static readonly FitMetric ReconstructionError = new("reconstruction-error");
    public static readonly FitMetric Inertia = new("inertia");
    public static readonly FitMetric LogLikelihood = new("log-likelihood");
    public static readonly FitMetric ClusterCount = new("cluster-count");
    public static readonly FitMetric Accuracy = new("accuracy");
    public static readonly FitMetric InnovationVariance = new("innovation-variance");
    public static readonly FitMetric BaselineLogLikelihood = new("baseline-log-likelihood");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EstimatorFamily {
    public static readonly EstimatorFamily Regression = new("regression");
    public static readonly EstimatorFamily Reduction = new("reduction");
    public static readonly EstimatorFamily Cluster = new("cluster");
    public static readonly EstimatorFamily Classify = new("classify");
    public static readonly EstimatorFamily Temporal = new("temporal");

    // Supervision is a PROJECTION of the family, so the roster answers it once instead of fifteen kind rows each
    // restating it — and contradicting it becomes unspellable rather than a silent column.
    public bool Supervised => this == Regression || this == Classify;
}

// One row carries the inverse link g⁻¹(η) in BOTH arities — the scalar the prediction fold applies and the
// tensor the fit's loss composes — so a `Link` override moves the fitted objective and the predicted mean
// together. The variance function V(μ) is the FAMILY's, never the link's: identity paired with a binomial
// family still weights by μ(1−μ), and a link-owned variance silently reports the Gaussian one.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LinkFunction {
    public static readonly LinkFunction Identity = new("identity", static eta => eta, static eta => eta, admits: static _ => true);
    public static readonly LinkFunction Logit = new("logit", static eta => 1.0 / (1.0 + Math.Exp(-eta)), static eta => torch.special.expit(eta), admits: static _ => true);
    public static readonly LinkFunction Log = new("log", static eta => Math.Exp(eta), static eta => eta.exp(), admits: static _ => true);
    // g(μ)=1/μ, the statsmodels `InversePower` link: η is the RECIPROCAL mean, so the domain is η > 0 and an
    // unconstrained LBFGS step reaches η ≤ 0 on the way to the optimum — the gate refuses there rather than
    // returning a NaN loss the driver reads as progress. statsmodels ships `safe_links = [Log]` for the same reason.
    public static readonly LinkFunction Inverse = new("inverse", static eta => 1.0 / eta, static eta => eta.pow(-1.0), admits: static eta => eta > 0.0);
    // Inverse-Gaussian's canonical g(μ)=1/μ², so g⁻¹(η)=η^(−1/2) under the same positive-η domain.
    public static readonly LinkFunction InverseSquared = new("inverse-squared", static eta => 1.0 / Math.Sqrt(eta), static eta => eta.pow(-0.5), admits: static eta => eta > 0.0);

    [UseDelegateFromConstructor] public partial double Mean(double eta);
    [UseDelegateFromConstructor] internal partial Tensor Mean(Tensor eta);
    [UseDelegateFromConstructor] private partial bool Admits(double eta);

    // The linear predictor's admissible range, proved over the FITTED η before the carrier lands and over the
    // predicted η before a mean leaves; an out-of-domain η is a typed refusal naming the link.
    internal Fin<Unit> Domain(Vector<double> eta) =>
        eta.All(Admits) ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(eta.Count))));
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
        supports: static y => true,
        kernel: static (mu, y) => mu.sub(y).pow(2));
    public static readonly GlmFamily Poisson = new("poisson", LinkFunction.Log,
        deviance: static (y, mu, w) => y == 0.0 ? 2.0 * w * mu : 2.0 * w * (y * Math.Log(y / mu) - (y - mu)),
        variance: static (mu, _) => mu,
        supports: static y => y >= 0.0 && double.IsInteger(y),
        kernel: static (mu, y) => mu.sub(y.mul(mu.log())));
    // y is the observed PROPORTION of m trials, so the endpoints y=0 and y=1 are the ordinary Bernoulli
    // observations and carry the whole deviance in one term each.
    public static readonly GlmFamily Binomial = new("binomial", LinkFunction.Logit,
        deviance: static (y, mu, m) => y == 0.0 ? -2.0 * m * Math.Log(1.0 - mu)
            : y == 1.0 ? -2.0 * m * Math.Log(mu)
            : 2.0 * m * (y * Math.Log(y / mu) + (1.0 - y) * Math.Log((1.0 - y) / (1.0 - mu))),
        variance: static (mu, m) => mu * (1.0 - mu) / m,
        supports: static y => y >= 0.0 && y <= 1.0,
        kernel: static (mu, y) => y.mul(mu.log()).add(y.neg().add(1.0).mul(mu.neg().add(1.0).log())).neg());
    public static readonly GlmFamily Gamma = new("gamma", LinkFunction.Log,
        deviance: static (y, mu, w) => 2.0 * w * ((y - mu) / mu - Math.Log(y / mu)),
        variance: static (mu, _) => mu * mu,
        supports: static y => y > 0.0,
        kernel: static (mu, y) => y.div(mu).add(mu.log()));
    public static readonly GlmFamily InverseGaussian = new("inverse-gaussian", LinkFunction.InverseSquared,
        deviance: static (y, mu, w) => w * (y - mu) * (y - mu) / (y * mu * mu),
        variance: static (mu, _) => mu * mu * mu,
        supports: static y => y > 0.0,
        kernel: static (mu, y) => y.div(mu.pow(2)).sub(mu.pow(-1.0).mul(2.0)));

    public LinkFunction Canonical { get; }

    [UseDelegateFromConstructor] public partial double Deviance(double y, double mu, double weight);
    [UseDelegateFromConstructor] public partial double Variance(double mu, double weight);
    [UseDelegateFromConstructor] private partial bool Supports(double y);
    [UseDelegateFromConstructor] private partial Tensor Kernel(Tensor mu, Tensor y);

    // The loss is ONE composition for every (family, link) cell: the θ-dependent unit deviance evaluated at
    // the link's own tensor inverse. A per-pair loss table re-derives five identities the composition already
    // yields — gaussian∘identity is the squared error, poisson∘log is `e^η − y·η`, binomial∘logit is
    // `log1p(e^η) − y·η`, gamma∘log is `η + y·e^{−η}`, and gamma∘inverse is `y·η − ln η`.
    internal Func<Tensor, Tensor, Tensor> Loss(LinkFunction link) => (eta, y) => Kernel(link.Mean(eta), y).mean();

    internal Fin<Unit> Admit(Design design) =>
        design.Targets.Filter(y => y.All(Supports)).IsSome
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(Key))));

    // Deviance explained, 1 − D_resid/D_null, the family's own R² analogue: D_null is the same unit deviance
    // against the intercept-only mean, so the scale-free ratio compares fits within one family and the
    // saturated-model normalizer cancels twice over.
    internal double Explained(Vector<double> y, Vector<double> fitted, double weight) {
        double mean = y.Average();
        double nullDeviance = EstimatorFold.ExactSum(toSeq(Enumerable.Range(0, y.Count).Select(i => Deviance(y[i], mean, weight))));
        return nullDeviance > 0.0 ? 1.0 - Total(y, fitted, weight) / nullDeviance : 0.0;
    }

    internal double Total(Vector<double> y, Vector<double> fitted, double weight) =>
        EstimatorFold.ExactSum(toSeq(Enumerable.Range(0, y.Count).Select(i => Deviance(y[i], fitted[i], weight))));
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

    [UseDelegateFromConstructor] public partial double At(Vector<double> left, Vector<double> right, KernelParams parameters);

    public Matrix<double> Gram(Matrix<double> left, Matrix<double> right, KernelParams parameters) =>
        Matrix<double>.Build.Dense(left.RowCount, right.RowCount, (i, j) => At(left.Row(i), right.Row(j), parameters));

    // The diagonal is the SMO second-order denominator's operand and, for `rbf`/`sigmoid` with zero offset,
    // a constant — computing it per pair from the full kernel keeps the row the one authority on its value.
    public Vector<double> Diagonal(Matrix<double> x, KernelParams parameters) =>
        Vector<double>.Build.Dense(x.RowCount, i => At(x.Row(i), x.Row(i), parameters));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OptimDriver {
    public static readonly OptimDriver LBfgs = new("lbfgs", lineSearch: true, static (p, lr) => torch.optim.LBFGS(p, lr, max_iter: 20));
    public static readonly OptimDriver Adam = new("adam", lineSearch: false, static (p, lr) => torch.optim.Adam(p, lr));

    public bool LineSearch { get; }

    [UseDelegateFromConstructor] public partial torch.optim.Optimizer Bind(IEnumerable<Parameter> parameters, double lr);
}

// ONE `glm` row spans every exponential family: the family is per-occurrence payload on `Estimator.Regression`,
// so poisson, binomial, gamma, and inverse-gaussian fits differ by a row value, never by a kind. A per-family
// kind re-declares the same driver, the same admission shape, and the same loss composition five times and
// hard-codes each one's link into its key, which is exactly what made a `glm-poisson` fit under an overridden
// identity link minimize the log-link deviance and predict through the identity mean.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EstimatorKind {
    public static readonly EstimatorKind Ols = new("ols", EstimatorFamily.Regression, FitMetric.R2, OptimDriver.Adam, EstimatorKernels.Ordinary, EstimatorFold.RealResponse);
    public static readonly EstimatorKind Ridge = new("ridge", EstimatorFamily.Regression, FitMetric.R2, OptimDriver.Adam, EstimatorKernels.Ridged, EstimatorFold.RegularizedResponse);
    public static readonly EstimatorKind Lasso = new("lasso", EstimatorFamily.Regression, FitMetric.R2, OptimDriver.Adam, EstimatorKernels.Penalized, EstimatorFold.IterativeResponse);
    public static readonly EstimatorKind Glm = new("glm", EstimatorFamily.Regression, FitMetric.DevianceExplained, OptimDriver.LBfgs,
        static ctx => EstimatorKernels.Deviance(ctx, ctx.Family.Loss(ctx.Link)), EstimatorFold.GlmResponse);
    public static readonly EstimatorKind Pca = new("pca", EstimatorFamily.Reduction, FitMetric.ExplainedEnergy, OptimDriver.Adam, EstimatorKernels.Principal, EstimatorFold.ReductionDesign);
    public static readonly EstimatorKind KernelPca = new("kernel-pca", EstimatorFamily.Reduction, FitMetric.ExplainedEnergy, OptimDriver.Adam, EstimatorKernels.KernelPrincipal, EstimatorFold.KernelReductionDesign);
    public static readonly EstimatorKind Nmf = new("nmf", EstimatorFamily.Reduction, FitMetric.ReconstructionError, OptimDriver.Adam, EstimatorKernels.NonNegative, EstimatorFold.NonNegativeReductionDesign);
    public static readonly EstimatorKind KMeans = new("kmeans", EstimatorFamily.Cluster, FitMetric.Inertia, OptimDriver.Adam, EstimatorKernels.Lloyd, EstimatorFold.GroupingDesign);
    public static readonly EstimatorKind Gmm = new("gmm", EstimatorFamily.Cluster, FitMetric.LogLikelihood, OptimDriver.Adam, EstimatorKernels.ExpectationMaximization, EstimatorFold.MixtureDesign);
    public static readonly EstimatorKind Dbscan = new("dbscan", EstimatorFamily.Cluster, FitMetric.ClusterCount, OptimDriver.Adam, EstimatorKernels.Reachability, EstimatorFold.DensityDesign);
    public static readonly EstimatorKind Hierarchical = new("hierarchical", EstimatorFamily.Cluster, FitMetric.Inertia, OptimDriver.Adam, EstimatorKernels.Agglomerative, EstimatorFold.LinkageDesign);
    public static readonly EstimatorKind Knn = new("knn", EstimatorFamily.Classify, FitMetric.Accuracy, OptimDriver.Adam, EstimatorKernels.Neighborhood, EstimatorFold.NeighborhoodDesign);
    public static readonly EstimatorKind Svm = new("svm", EstimatorFamily.Classify, FitMetric.Accuracy, OptimDriver.Adam, EstimatorKernels.MarginMachines, EstimatorFold.MarginDesign);
    public static readonly EstimatorKind CSvm = new("c-svm", EstimatorFamily.Classify, FitMetric.Accuracy, OptimDriver.Adam, EstimatorKernels.SequentialMinimal, EstimatorFold.MarginDesign);
    public static readonly EstimatorKind NaiveBayes = new("naive-bayes", EstimatorFamily.Classify, FitMetric.Accuracy, OptimDriver.Adam, EstimatorKernels.GaussianBayes, EstimatorFold.ClassLabels);

    public EstimatorFamily Family { get; }
    public FitMetric Metric { get; }
    public OptimDriver Driver { get; }

    [UseDelegateFromConstructor] internal partial Fin<FittedModel> Fit(FitContext context);
    [UseDelegateFromConstructor] internal partial Fin<Unit> Admit(FitContext context);
}

// Forecast projection is a ROW column, not an equality ladder over the carrier's own model tag: the deleted
// `lag.Model == X ? … : lag.Model == Y ? … : …` chain re-decided at the call site what the row already knows,
// and a new forecast row would have routed silently to the ARMA roll. Detector rows carry a typed refusal
// because their carrier is `EstimatorModel.Detector` and never a `Lag` — the arm is unreachable and says so.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimeSeriesModel {
    public static readonly TimeSeriesModel Ar = new("ar", FitMetric.InnovationVariance, EstimatorKernels.AutoRegress, EstimatorKernels.ArmaForecast);
    public static readonly TimeSeriesModel Arma = new("arma", FitMetric.InnovationVariance, EstimatorKernels.MovingAverage, EstimatorKernels.ArmaForecast);
    public static readonly TimeSeriesModel ExponentialSmoothing = new("exponential-smoothing", FitMetric.InnovationVariance, EstimatorKernels.Holt, EstimatorKernels.TrendForecast);
    public static readonly TimeSeriesModel StateSpace = new("state-space", FitMetric.InnovationVariance, EstimatorKernels.StateSpace, EstimatorKernels.TrendForecast);
    // The three detection rows bind ONE baseline fit. Three per-row forwarders each calling the same body with a
    // row argument the spec already derives were three names for one fold plus a `spec.Model != model` re-check
    // that could never fire, because `TemporalSpec.Detection` DERIVES the row from the payload it carries.
    public static readonly TimeSeriesModel Cusum = new("cusum", FitMetric.BaselineLogLikelihood, EstimatorKernels.DetectorBaseline, EstimatorKernels.NoForecast);
    public static readonly TimeSeriesModel BayesianOnline = new("bayesian-online", FitMetric.BaselineLogLikelihood, EstimatorKernels.DetectorBaseline, EstimatorKernels.NoForecast);
    public static readonly TimeSeriesModel CorrelatedResidual = new("correlated-residual", FitMetric.BaselineLogLikelihood, EstimatorKernels.DetectorBaseline, EstimatorKernels.NoForecast);

    public FitMetric Metric { get; }

    [UseDelegateFromConstructor] internal partial Fin<FittedModel> Fit(FitContext context);
    [UseDelegateFromConstructor] internal partial Fin<Vector<double>> Forecast(EstimatorModel.Lag lag, int horizon);
}

// Curve rows own the linear-in-parameters fits whose design matrix is a transform of one feature column, so the
// library's own log-linearization conventions stay authoritative and no arm re-derives which side gets logged.
// Every row scores identically — `R2` is the quality and `StandardError` the residual — because each fits an
// identity link, so the link-weighted Pearson dispersion the GLM rows need has nothing to weight here.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RegressionForm {
    public static readonly RegressionForm Polynomial = new("polynomial",
        static (spec, x, y) => Fit.Polynomial(x, y, spec.Terms - 1, DirectRegressionMethod.QR),
        static (_, c, x) => MathNet.Numerics.Polynomial.Evaluate(x, [.. c]));
    public static readonly RegressionForm Exponential = new("exponential",
        static (_, x, y) => Pair(Fit.Exponential(x, y, DirectRegressionMethod.QR)),
        static (_, c, x) => c[0] * Math.Exp(c[1] * x));
    public static readonly RegressionForm Power = new("power",
        static (_, x, y) => Pair(Fit.Power(x, y, DirectRegressionMethod.QR)),
        static (_, c, x) => c[0] * Math.Pow(x, c[1]));
    public static readonly RegressionForm Logarithm = new("logarithm",
        static (_, x, y) => Pair(Fit.Logarithm(x, y, DirectRegressionMethod.QR)),
        static (_, c, x) => c[0] + c[1] * Math.Log(x));
    public static readonly RegressionForm Combination = new("combination",
        static (spec, x, y) => Fit.LinearCombination(x, y, [.. spec.Functions]),
        static (spec, c, x) => spec.Functions.Map((basis, k) => c[k] * basis(x)).Fold(0.0, static (total, term) => total + term));

    public FitMetric Metric => FitMetric.R2;

    [UseDelegateFromConstructor] internal partial double[] Fit(CurveSpec spec, double[] x, double[] y);
    [UseDelegateFromConstructor] internal partial double Evaluate(CurveSpec spec, Vector<double> coefficients, double x);

    // Two-parameter library fits name their elements differently (`(A, R)` versus `(A, B)`), so positional
    // deconstruction reads both and no row spells a member name the next release could rename.
    private static double[] Pair((double, double) row) => [row.Item1, row.Item2];
}

// --- [MODELS] ---------------------------------------------------------------------------

// The detection payload at ONE owner. The three cases are the closed set `Detect` dispatches on, so its switch
// is total with no unreachable arm — where the widened seven-case temporal carrier gave it four forecast arms
// that could not fire and a caller could not tell an impossible state from a refused one.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DetectorSpec {
    private DetectorSpec() { }

    public sealed record Cusum(int Warmup, double Drift, double Threshold) : DetectorSpec;
    public sealed record BayesianOnline(int Warmup, double Hazard, double Threshold) : DetectorSpec;
    // Ridge is the diagonal loading the baseline covariance takes before factorization; every other detector row
    // loads at the shared variance floor, so the column lives on the ONE case that moves it rather than as a
    // seven-arm switch answering the floor six times.
    public sealed record CorrelatedResidual(int Warmup, double FalsePositiveRate, double Ridge) : DetectorSpec;

    public TimeSeriesModel Model => Switch(
        cusum: static _ => TimeSeriesModel.Cusum,
        bayesianOnline: static _ => TimeSeriesModel.BayesianOnline,
        correlatedResidual: static _ => TimeSeriesModel.CorrelatedResidual);

    internal int Warmup => Switch(
        cusum: static s => s.Warmup, bayesianOnline: static s => s.Warmup, correlatedResidual: static s => s.Warmup);

    internal double Loading => Switch(
        cusum: static _ => EstimatorFold.VarianceFloor,
        bayesianOnline: static _ => EstimatorFold.VarianceFloor,
        correlatedResidual: static s => s.Ridge);

    // Each case admits its OWN parameters under its OWN slug, accumulating across the independent columns so a
    // caller learns every violated band at once. The deleted shared gate took a `bool range` that had already
    // collapsed three unrelated conditions into one bit before the fault was even minted.
    internal Fin<Unit> Admit(int rows, int columns) => Switch(
        state: (Rows: rows, Columns: columns),
        cusum: static (s, spec) => (
            Prefix(spec.Warmup, s.Rows, s.Columns).ToValidation(),
            Band(spec.Drift >= 0.0 && double.IsFinite(spec.Drift), "cusum-drift", spec.Drift).ToValidation(),
            Band(spec.Threshold > 0.0 && double.IsFinite(spec.Threshold), "cusum-threshold", spec.Threshold).ToValidation())
            .Apply(static (_, _, _) => unit).ToFin(),
        bayesianOnline: static (s, spec) => (
            Prefix(spec.Warmup, s.Rows, s.Columns).ToValidation(),
            Band(spec.Hazard is > 0.0 and < 1.0, "bayesian-hazard", spec.Hazard).ToValidation(),
            Band(spec.Threshold is > 0.0 and < 1.0, "bayesian-threshold", spec.Threshold).ToValidation())
            .Apply(static (_, _, _) => unit).ToFin(),
        correlatedResidual: static (s, spec) => (
            Prefix(spec.Warmup, s.Rows, s.Columns).ToValidation(),
            Band(spec.FalsePositiveRate is > 0.0 and < 1.0, "correlated-false-positive", spec.FalsePositiveRate).ToValidation(),
            Band(spec.Ridge > 0.0 && double.IsFinite(spec.Ridge), "correlated-ridge", spec.Ridge).ToValidation())
            .Apply(static (_, _, _) => unit).ToFin());

    // The warmup prefix must both exceed the dimension (a covariance below it is singular by construction) and
    // leave rows behind to score.
    private static Fin<Unit> Prefix(int warmup, int rows, int columns) =>
        warmup >= Math.Max(4, columns + 1) && rows > warmup
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Counts(warmup, rows, columns))));

    private static Fin<Unit> Band(bool holds, string gate, double value) =>
        holds ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
}

// The three DBSCAN assignment states one `int` spelled through the sentinels `-2` and `-1`. `Cluster` collapses
// onto the carrier's own `Option<int>` at the boundary, so the sentinel decoding happens ONCE and never at a read
// — the deleted form re-decoded `label < 0` at four sites, each of which had to know both magic values.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Reach {
    private Reach() { }

    public sealed record Unvisited : Reach;
    public sealed record Noise : Reach;
    public sealed record Member(int Cluster) : Reach;

    internal Option<int> Cluster => Switch(
        unvisited: static _ => Option<int>.None, noise: static _ => Option<int>.None, member: static m => Optional(m.Cluster));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TemporalSpec {
    private TemporalSpec() { }

    public sealed record Ar(int Lags) : TemporalSpec;
    public sealed record Arma(int Lags) : TemporalSpec;
    public sealed record ExponentialSmoothing : TemporalSpec;
    public sealed record StateSpace : TemporalSpec;
    public sealed record Detection(DetectorSpec Spec) : TemporalSpec;

    public TimeSeriesModel Model => Switch(
        ar: static _ => TimeSeriesModel.Ar, arma: static _ => TimeSeriesModel.Arma,
        exponentialSmoothing: static _ => TimeSeriesModel.ExponentialSmoothing, stateSpace: static _ => TimeSeriesModel.StateSpace,
        detection: static s => s.Spec.Model);

    public bool Forecasts => Detector.IsNone;

    // The detection payload projects as an `Option` and every detector consumer reads it through this ONE
    // narrowing, so no downstream switch carries a forecast arm it cannot reach.
    public Option<DetectorSpec> Detector => Switch(
        ar: static _ => Option<DetectorSpec>.None, arma: static _ => Option<DetectorSpec>.None,
        exponentialSmoothing: static _ => Option<DetectorSpec>.None, stateSpace: static _ => Option<DetectorSpec>.None,
        detection: static s => Optional(s.Spec));

    internal int History => Switch(
        ar: static s => s.Lags, arma: static s => s.Lags, exponentialSmoothing: static _ => 2, stateSpace: static _ => 3,
        detection: static s => s.Spec.Warmup);

    internal Fin<Unit> Admit(int rows, int columns) => Switch(
        state: (Rows: rows, Columns: columns),
        ar: static (s, spec) => Forecast(spec.Lags, s.Rows, s.Columns),
        arma: static (s, spec) => Forecast(spec.Lags, s.Rows, s.Columns),
        exponentialSmoothing: static (s, _) => Forecast(2, s.Rows, s.Columns),
        stateSpace: static (s, _) => Forecast(3, s.Rows, s.Columns),
        detection: static (s, spec) => spec.Spec.Admit(s.Rows, s.Columns));

    private static Fin<Unit> Forecast(int history, int rows, int columns) =>
        columns == 1 && history >= 1 && rows > history
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Counts(rows, columns, history))));
}

// `CurveSpec` generates a curve row exactly as `TemporalSpec` generates a temporal one, owning every parameter its
// form cannot reconstruct and deriving the `RegressionForm` that binds the kernel.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveSpec {
    private CurveSpec() { }

    public sealed record Polynomial(int Order) : CurveSpec;
    public sealed record Exponential : CurveSpec;
    public sealed record Power : CurveSpec;
    public sealed record Logarithm : CurveSpec;
    public sealed record Combination(Seq<Func<double, double>> Basis) : CurveSpec;

    public RegressionForm Form => Switch(
        polynomial: static _ => RegressionForm.Polynomial, exponential: static _ => RegressionForm.Exponential,
        power: static _ => RegressionForm.Power, logarithm: static _ => RegressionForm.Logarithm,
        combination: static _ => RegressionForm.Combination);

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
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(design.Columns, 1L))))
        : design.Rows <= Terms
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(design.Rows, Terms))))
        : Switch(
            state: design,
            polynomial: static (d, s) => s.Order >= 1
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(s.Order)))),
            exponential: static (d, _) => Support(d.Targets, "curve-exponential-response"),
            power: static (d, _) => Support(Optional(d.Features.Column(0)), "curve-power-feature")
                .Bind(_ => Support(d.Targets, "curve-power-response")),
            logarithm: static (d, _) => Support(Optional(d.Features.Column(0)), "curve-logarithm-feature"),
            combination: static (_, s) => !s.Basis.IsEmpty && s.Basis.ForAll(static f => f is not null)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input))));

    private static Fin<Unit> Support(Option<Vector<double>> values, string gate) =>
        values.Filter(static v => v.All(static value => value > 0.0)).IsSome
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(gate))));
}

// --- [SERVICES] -------------------------------------------------------------------------

// Torch-loss rows minimize a Tensor loss under torch.autograd + the row's OptimDriver inside one DisposeScope; the LBFGS line-search form re-evaluates the closure per probe.
// Every intermediate is reclaimed at scope exit and AnomalyMode traps a NaN/inf fit, so no Tensor escapes the lane.
// The Float64 default dtype is BOOT state `Tensor/blas#DENSE_ALGEBRA` `AtenFloor.Configure` pins once — a
// per-fit `set_default_dtype` mutates a process-global from inside a lane that may run concurrently with any
// other torch consumer, and every tensor here already names its own `ScalarType.Float64` at construction.
// The `using` scope inside a `Fin`-returning static is DELIBERATE and no `SUCCESS_ARM_RELEASE` violation: the
// disposal frees torch intermediates, and the only value that leaves is a detached MathNet vector already copied
// out of the graph — nothing the success arm returns is owned by either scope.
public static class IterativeEngine {
    public static Fin<(Vector<double> Theta, double Loss, int Steps, Convergence Verdict)> Minimize(
        Func<Tensor, Tensor, Tensor, Tensor> loss, Matrix<double> design, Vector<double> response, OptimDriver driver, double learningRate, FitBudget budget) {
        using DisposeScope scope = torch.NewDisposeScope();
        using AnomalyMode anomaly = new(enabled: true, check_nan: true);
        Tensor x = torch.from_array(design.ToColumnMajorArray(), ScalarType.Float64).reshape(design.ColumnCount, design.RowCount).t();
        Tensor y = torch.from_array(response.AsArray() ?? response.ToArray(), ScalarType.Float64).reshape(response.Count);
        Parameter theta = new(torch.zeros(design.ColumnCount, ScalarType.Float64), requires_grad: true);
        torch.optim.Optimizer opt = driver.Bind([theta], learningRate);
        Tensor Step() { opt.zero_grad(); Tensor l = loss(theta, x, y); l.backward(); return l; }
        // The terminal is a VERDICT, never a flag: a spent budget answers `Exhausted` carrying the budget, so a
        // GLM under a thousand-iteration cap can no longer publish an unconverged carrier through a success arm.
        double last = double.MaxValue;
        int steps = 0;
        Convergence verdict = new Convergence.Exhausted(budget.MaxIterations.Value);
        for (; steps < budget.MaxIterations.Value; steps++) {
            Tensor value = driver.LineSearch ? ((Modules.LBFGS)opt).step(Step) : Plain(opt, Step);
            double current = value.ReadCpuDouble(0);
            double delta = Math.Abs(last - current);
            last = current;
            if (delta < budget.Stop.Value) { verdict = new Convergence.Converged(delta); steps++; break; }
        }
        Vector<double> fitted = Vector<double>.Build.DenseOfArray(theta.detach().reshape(theta.NumberOfElements).data<double>().ToArray());
        return double.IsFinite(last)
            ? Fin.Succ((fitted, last, steps, verdict))
            : Fin.Fail<(Vector<double>, double, int, Convergence)>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(last))));
    }

    public static Func<Tensor, Tensor, Tensor, Tensor> Lasso(double lambda) =>
        (theta, x, y) => 0.5 * x.matmul(theta).sub(y).pow(2).mean() + lambda * theta.abs().sum();

    private static Tensor Plain(torch.optim.Optimizer opt, Func<Tensor> closure) { Tensor l = closure(); opt.step(); return l; }
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// Every body a row column binds. The shared numeric helpers — `Intercept`, `Center`, `Distance`, `Nearest`,
// `LogGaussian`, `Mahalanobis`, `Choleskies`, `ExactSum` — belong to `EstimatorFold` at `Stats/estimator`, which
// is the contract owner both this page's fits and that page's prediction readers compose, so no helper has two homes.
public static class EstimatorKernels {
    // --- [REGRESSION]

    internal static Fin<FittedModel> Ordinary(FitContext ctx) => ClosedForm(ctx, tikhonov: 0.0);

    internal static Fin<FittedModel> Ridged(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Regression>().Bind(p => ClosedForm(ctx, tikhonov: p.Regularization));

    // OLS/ridge share intercept-augmented thin-QR; ridge stacks the unpenalized-intercept `√λ·I` block.
    // `λ = 0` selects the unstacked identity without a mode flag.
    private static Fin<FittedModel> ClosedForm(FitContext ctx, double tikhonov) =>
        ctx.Case<EstimatorPolicy.Regression>().Bind(p => ctx.Supervised().Bind(y => {
            Matrix<double> design = EstimatorFold.Intercept(ctx.Design.Features);
            Matrix<double> a = tikhonov > 0.0 ? design.Stack(Tikhonov(design.ColumnCount, tikhonov)) : design;
            Vector<double> b = tikhonov > 0.0 ? Vector<double>.Build.Dense(design.RowCount + design.ColumnCount, i => i < design.RowCount ? y[i] : 0.0) : y;
            return DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), a, b, TolerancePolicy.Derive(a, b), ctx.Substrate)
                .Bind(solved => Build(ctx, p, y, EstimatorFold.Split(solved.X), 0.0, 1, new Convergence.Converged(solved.Residual)));
        }));

    private static Matrix<double> Tikhonov(int columns, double lambda) =>
        Matrix<double>.Build.Diagonal(columns, columns, j => j == 0 ? 0.0 : Math.Sqrt(lambda));

    // One kernel serves every curve row: the row supplies its coefficient fit and its evaluator, and the shared body
    // owns capture, finiteness, and the uniform quality pair. `RSquared` scores the surrogate on the ORIGINAL scale
    // rather than the linearized one the library fits on, so an exponential row's quality is comparable with a
    // polynomial's; `StandardError` reads the row's own fitted-parameter count as its degrees of freedom.
    internal static Fin<FittedModel> CurveFit(FitContext ctx, CurveSpec spec) =>
        ctx.Supervised().Bind(y => {
            double[] x = ctx.Design.Features.Column(0).AsArray() ?? ctx.Design.Features.Column(0).ToArray();
            double[] observed = y.AsArray() ?? y.ToArray();
            // The ONE foreign-throw boundary on the page preserves MathNet's error when `Fit.*` rejects a singular
            // linearization.
            return Op.Of(name: "stats.curve-fit").Catch(() => Fin.Succ(spec.Form.Fit(spec, x, observed)))
                .Bind(coefficients => coefficients.Length == spec.Terms && TensorPrimitives.IsFiniteAll<double>(coefficients)
                    ? Fin.Succ(Vector<double>.Build.DenseOfArray(coefficients))
                    : Fin.Fail<Vector<double>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(coefficients.Length, spec.Terms)))))
                .Map(coefficients => {
                    double[] modelled = [.. x.Select(value => spec.Form.Evaluate(spec, coefficients, value))];
                    return new FittedModel(
                        ctx.Estimator, new EstimatorModel.Curve(spec, coefficients),
                        GoodnessOfFit.RSquared(modelled, observed),
                        GoodnessOfFit.StandardError(modelled, observed, spec.Terms),
                        None, 1, new Convergence.Converged(0.0), ctx.Clock.GetCurrentInstant());
                });
        });

    internal static Fin<FittedModel> Penalized(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Regression>().Bind(p => ctx.Supervised().Bind(y =>
            IterativeEngine.Minimize(IterativeEngine.Lasso(p.Regularization), EstimatorFold.Intercept(ctx.Design.Features), y, ctx.Driver, p.LearningRate, p.Budget)
                .Bind(fit => Build(ctx, p, y, EstimatorFold.Split(fit.Theta), fit.Loss, fit.Steps, fit.Verdict))));

    internal static Fin<FittedModel> Deviance(FitContext ctx, Func<Tensor, Tensor, Tensor> deviance) =>
        ctx.Case<EstimatorPolicy.Regression>().Bind(p => ctx.Supervised().Bind(y =>
            IterativeEngine.Minimize((theta, x, yy) => deviance(x.matmul(theta), yy), EstimatorFold.Intercept(ctx.Design.Features), y, ctx.Driver, p.LearningRate, p.Budget)
                .Bind(fit => Build(ctx, p, y, EstimatorFold.Split(fit.Theta), fit.Loss, fit.Steps, fit.Verdict))));

    // ONE builder for every linear row, closed-form and iterative alike: quality is the family's own
    // deviance-explained (Gaussian∘identity recovers R² exactly, so no row needs a second metric), and the
    // residual is the link-and-family-weighted Pearson dispersion. The linear predictor crosses the link's
    // domain gate BEFORE the carrier lands, so an LBFGS step that walked a Gamma inverse-link fit to η ≤ 0
    // refuses by name instead of publishing a carrier whose every prediction is a signed infinity.
    private static Fin<FittedModel> Build(
        FitContext ctx, EstimatorPolicy.Regression policy, Vector<double> y,
        (double Intercept, Vector<double> Slopes) split, double loss, int steps, Convergence verdict) {
        Vector<double> eta = ctx.Design.Features.Multiply(split.Slopes).Add(split.Intercept);
        return ctx.Link.Domain(eta).Map(_ => {
            Vector<double> predicted = eta.Map(ctx.Link.Mean);
            // Pearson dispersion √(Σ (yᵢ−μᵢ)²/V(μᵢ)/(n−p)): the FAMILY's V(μ) weights each squared residual, so a
            // binomial fit scores on its μ(1−μ)/m scale and gaussian (V=1) recovers the ordinary standard error.
            // Never an unweighted RMSE blind to the family variance — that residual is the deleted decorative form.
            int dispersionDof = Math.Max(1, y.Count - split.Slopes.Count - 1);
            double dispersion = Math.Sqrt(EstimatorFold.ExactSum(toSeq(Enumerable.Range(0, y.Count)
                .Select(i => (y[i] - predicted[i]) * (y[i] - predicted[i]) / ctx.Family.Variance(predicted[i], policy.Weight)))) / dispersionDof);
            double deviance = ctx.Family.Total(y, predicted, policy.Weight);
            EstimatorModel.Linear carrier = new(split.Slopes, split.Intercept, ctx.Link, ctx.Family, deviance / Math.Max(1e-12, dispersion * dispersion));
            // The log-likelihood the criterion reads is −D/(2φ̂) with the family normalizer dropped: it is a
            // constant in θ that cancels between candidates of one family, and no candidate set spans two.
            return new FittedModel(
                ctx.Estimator, carrier, ctx.Family.Explained(y, predicted, policy.Weight),
                double.IsFinite(dispersion) ? dispersion : loss,
                Some(-0.5 * carrier.ScaledDeviance), steps, verdict, ctx.Clock.GetCurrentInstant());
        });
    }

    // --- [REDUCTION]

    internal static Fin<FittedModel> Principal(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Reduction>().Bind(policy => {
            Matrix<double> x = ctx.Design.Features;
            Vector<double> mean = x.ColumnSums() / x.RowCount;
            return DenseOps.Decompose(EstimatorFold.Center(x, mean), FactorizationKind.Svd).Bind(factor => factor.Switch(
                lu: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                qr: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                cholesky: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                evd: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                sketched: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                svd: f => {
                    Vector<double> singular = f.Decomposition.S;
                    double total = singular.PointwisePower(2.0).Sum();
                    int rank = Retain(singular, total, Math.Min(ctx.Rank <= 0 ? singular.Count : ctx.Rank, singular.Count), policy.EnergyFraction);
                    Matrix<double> components = f.Decomposition.VT.SubMatrix(0, rank, 0, x.ColumnCount);
                    double energy = total > 0.0 ? singular.SubVector(0, rank).PointwisePower(2.0).Sum() / total : 0.0;
                    EstimatorModel.Basis carrier = new(components, singular.SubVector(0, rank), mean, energy);
                    return Fin.Succ(new FittedModel(ctx.Estimator, carrier, energy, 1.0 - energy, None, 1, new Convergence.Converged(1.0 - energy), ctx.Clock.GetCurrentInstant()));
                }));
        });

    // Kernel-PCA: the centered Gram of the policy's OWN `KernelRow` is the operand and its top eigenvectors the
    // duals, so a linear, polynomial, or sigmoid kernel-PCA is a policy value rather than a second kernel row;
    // out-of-sample projection double-centers the test/train kernel against the stored row/grand means.
    internal static Fin<FittedModel> KernelPrincipal(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Reduction>().Bind(policy => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount;
            Matrix<double> gram = policy.Kernel.Gram(x, x, policy.Parameters);
            Vector<double> rowMean = gram.RowSums() / n;
            double grandMean = rowMean.Sum() / n;
            Matrix<double> centered = Matrix<double>.Build.Dense(n, n, (i, j) => gram[i, j] - rowMean[i] - rowMean[j] + grandMean);
            return DenseOps.Decompose(Admission.Symmetrize(centered), FactorizationKind.Evd).Bind(factor => factor.Switch(
                lu: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                qr: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                cholesky: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                svd: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                sketched: static _ => Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Unsupported(ComputeCapability.Factorization))),
                evd: f => {
                    Vector<double> values = f.Decomposition.EigenValues.Map(static v => v.Real);
                    int rank = Math.Min(ctx.Rank <= 0 ? n : ctx.Rank, n);
                    int[] order = [.. Enumerable.Range(0, n).OrderByDescending(i => values[i]).Take(rank)];
                    Vector<double> eigen = Vector<double>.Build.DenseOfArray([.. order.Select(i => values[i])]);
                    Matrix<double> alphas = Matrix<double>.Build.Dense(n, rank, (i, c) => f.Decomposition.EigenVectors[i, order[c]] / Math.Sqrt(Math.Max(1e-12, eigen[c])));
                    EstimatorModel.KernelBasis carrier = new(x, alphas, eigen, policy.Kernel, policy.Parameters, rowMean, grandMean);
                    double captured = eigen.Sum() / Math.Max(1e-12, values.Map(Math.Abs).Sum());
                    return Fin.Succ(new FittedModel(ctx.Estimator, carrier, captured, 1.0 - captured, None, 1, new Convergence.Converged(1.0 - captured), ctx.Clock.GetCurrentInstant()));
                }));
        });

    private static int Retain(Vector<double> singular, double total, int cap, double fraction) {
        double cumulative = 0.0;
        for (int k = 0; k < cap; k++) {
            cumulative += singular[k] * singular[k];
            if (total > 0.0 && cumulative / total >= fraction) { return k + 1; }
        }
        return Math.Max(1, cap);
    }

    // NMF (Lee–Seung multiplicative updates): X ≈ W·H, W,H ≥ 0, minimizing the Frobenius reconstruction. The
    // factor pair seeds through the kernel `Deterministic.Source` on the estimator's own lane, so one design and
    // one policy answer one factorization — the deleted bare integer seeds drew from an unseeded process RNG and
    // gave two runs two answers with nothing on the carrier or the receipt to say why.
    internal static Fin<FittedModel> NonNegative(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Reduction>().Map(policy => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount, m = x.ColumnCount, k = Math.Max(1, ctx.Rank);
            Matrix<double> w = Matrix<double>.Build.Random(n, k, new ContinuousUniform(0.0, 1.0, ctx.Draw.At(0L).Source)).Add(1e-3);
            Matrix<double> h = Matrix<double>.Build.Random(k, m, new ContinuousUniform(0.0, 1.0, ctx.Draw.At(1L).Source)).Add(1e-3);
            double residual = double.MaxValue;
            int steps = 0;
            Convergence verdict = new Convergence.Exhausted(policy.Budget.MaxIterations.Value);
            for (; steps < policy.Budget.MaxIterations.Value; steps++) {
                h = h.PointwiseMultiply(w.TransposeThisAndMultiply(x).PointwiseDivide(w.TransposeThisAndMultiply(w).Multiply(h).Add(1e-12)));
                w = w.PointwiseMultiply(x.TransposeAndMultiply(h).PointwiseDivide(w.Multiply(h.TransposeAndMultiply(h)).Add(1e-12)));
                double next = (x - w.Multiply(h)).FrobeniusNorm();
                double delta = Math.Abs(residual - next);
                residual = next;
                if (delta < policy.Budget.Stop.Value) { verdict = new Convergence.Converged(residual); steps++; break; }
            }
            return new FittedModel(ctx.Estimator, new EstimatorModel.Factors(w, h), -residual, residual, None, steps, verdict, ctx.Clock.GetCurrentInstant());
        });

    // --- [CLUSTER]

    // Lloyd stops when no row changes its assignment, which is CONVERGENCE with a measured residual of zero
    // moves — the deleted `bool moved` was the same fact spelled as a flag the caller could not distinguish from
    // a spent budget.
    internal static Fin<FittedModel> Lloyd(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Grouping>().Bind(policy => ctx.Groups().Map(k => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount;
            Matrix<double> centroids = Seed(x, k);
            int[] labels = new int[n];
            int steps = 0;
            Convergence verdict = new Convergence.Exhausted(policy.Budget.MaxIterations.Value);
            for (; steps < policy.Budget.MaxIterations.Value; steps++) {
                int moved = 0;
                for (int i = 0; i < n; i++) {
                    int best = EstimatorFold.Nearest(x.Row(i), centroids);
                    if (best != labels[i]) { labels[i] = best; moved++; }
                }
                centroids = Recenter(x, labels, k, centroids);
                if (moved == 0) { verdict = new Convergence.Converged(0.0); steps++; break; }
            }
            double inertia = Inertia(x, labels, centroids);
            return new FittedModel(ctx.Estimator, new EstimatorModel.Partition(centroids, [.. labels]), -inertia, inertia / Math.Max(1, n), None, steps, verdict, ctx.Clock.GetCurrentInstant());
        }));

    // GMM-EM as a rail-threaded HALTING fold: `foldWhile` stops at the settled state rather than binding a no-op
    // through the whole remaining budget, and the terminal is `Converged` carrying the log-likelihood delta the
    // stop measured or `Exhausted` carrying the budget it spent. Cholesky failure on a degenerate covariance
    // aborts the whole fit through the rail.
    internal static Fin<FittedModel> ExpectationMaximization(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Grouping>().Bind(policy => ctx.Groups().Bind(k => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount, dim = x.ColumnCount;
            MixtureState seed = new(
                Means: Seed(x, k),
                Covariances: toSeq(Enumerable.Range(0, k).Select(_ => (Matrix<double>)Matrix<double>.Build.DiagonalIdentity(dim))),
                Weights: Vector<double>.Build.Dense(k, 1.0 / k),
                LogLik: double.NegativeInfinity, Steps: 0,
                Verdict: new Convergence.Exhausted(policy.Budget.MaxIterations.Value));
            return foldWhile(
                    (Fin<MixtureState> acc, int _) => acc.Bind(s => EmStep(x, s, dim, policy)),
                    static state => state.State.Map(static s => s.Verdict is not Convergence.Converged).IfFail(static _ => false),
                    Fin.Succ(seed),
                    toSeq(Enumerable.Range(0, policy.Budget.MaxIterations.Value)))
                // The EM evidence IS the observed-data log-likelihood, so the mixture row is the one unsupervised
                // carrier an information criterion can score without fabricating a likelihood from a quality score.
                .Map(s => new FittedModel(ctx.Estimator, new EstimatorModel.Mixture(s.Means, s.Covariances, s.Weights), s.LogLik,
                    double.IsFinite(s.LogLik) ? -s.LogLik / Math.Max(1, n) : double.MaxValue,
                    double.IsFinite(s.LogLik) ? Some(s.LogLik) : None, s.Steps, s.Verdict, ctx.Clock.GetCurrentInstant()));
        }));

    private sealed record MixtureState(
        Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights,
        double LogLik, int Steps, Convergence Verdict);

    private static Fin<MixtureState> EmStep(Matrix<double> x, MixtureState s, int dim, EstimatorPolicy.Grouping policy) =>
        EstimatorFold.Choleskies(s.Covariances, policy.Ridge).Map(chols => {
            int n = x.RowCount, k = s.Weights.Count;
            Matrix<double> gamma = Matrix<double>.Build.Dense(n, k);
            double evidence = 0.0;
            for (int i = 0; i < n; i++) {
                Vector<double> log = Vector<double>.Build.Dense(k, j => Math.Log(Math.Max(1e-300, s.Weights[j])) + EstimatorFold.LogGaussian(x.Row(i), s.Means.Row(j), chols[j], dim));
                double max = log.Maximum();
                double sum = log.Map(v => Math.Exp(v - max)).Sum();
                evidence += max + Math.Log(sum);
                for (int j = 0; j < k; j++) { gamma[i, j] = Math.Exp(log[j] - max) / sum; }
            }
            Vector<double> nk = gamma.ColumnSums();
            Matrix<double> weighted = gamma.TransposeThisAndMultiply(x);
            Matrix<double> means = Matrix<double>.Build.Dense(k, dim, (j, f) => weighted[j, f] / Math.Max(1e-9, nk[j]));
            Seq<Matrix<double>> covariances = toSeq(Enumerable.Range(0, k).Select(j => WeightedCovariance(x, gamma.Column(j), means.Row(j), nk[j], policy.Ridge)));
            double delta = Math.Abs(evidence - s.LogLik);
            return new MixtureState(means, covariances, nk / n, evidence, s.Steps + 1,
                delta < policy.Budget.Stop.Value ? new Convergence.Converged(delta) : s.Verdict);
        });

    // DBSCAN reachability. Labels ride a three-case union rather than the `-2`/`-1` integer sentinels the deleted
    // form carried: `Unvisited`, `Noise`, and `Member(cluster)` are three facts one `int` spelled through two
    // magic values that every read had to re-decode.
    internal static Fin<FittedModel> Reachability(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Grouping>().Map(policy => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount, minPts = policy.Neighbors;
            double eps = policy.Radius;
            Reach[] labels = [.. Enumerable.Repeat((Reach)new Reach.Unvisited(), n)];
            bool[] core = new bool[n];
            int cluster = -1;
            for (int i = 0; i < n; i++) {
                if (labels[i] is not Reach.Unvisited) { continue; }
                int[] neighbors = Region(x, i, eps);
                if (neighbors.Length < minPts) { labels[i] = new Reach.Noise(); continue; }
                core[i] = true;
                cluster++;
                Queue<int> frontier = new(neighbors);
                labels[i] = new Reach.Member(cluster);
                while (frontier.Count > 0) {
                    int q = frontier.Dequeue();
                    if (labels[q] is Reach.Member) { continue; }
                    labels[q] = new Reach.Member(cluster);
                    int[] reach = Region(x, q, eps);
                    if (reach.Length >= minPts) {
                        core[q] = true;
                        foreach (int r in reach) { frontier.Enqueue(r); }
                    }
                }
            }
            double noise = labels.Count(static label => label is not Reach.Member) / (double)n;
            // A density walk visits every row exactly once and terminates by construction — there is no budget to
            // exhaust, so the verdict is `Converged` at the measured noise fraction rather than a fabricated flag.
            return new FittedModel(
                ctx.Estimator, new EstimatorModel.Density(x, [.. labels.Select(static label => label.Cluster)], [.. core], eps),
                cluster + 1, noise, None, 1, new Convergence.Converged(noise), ctx.Clock.GetCurrentInstant());
        });

    internal static Fin<FittedModel> Agglomerative(FitContext ctx) => ctx.Groups().Map(target => {
        Matrix<double> x = ctx.Design.Features;
        int n = x.RowCount;
        int[] labels = [.. Enumerable.Range(0, n)];
        List<List<int>> clusters = [.. Enumerable.Range(0, n).Select(i => new List<int> { i })];
        double best = 0.0;
        while (clusters.Count > target) {
            (int A, int B, double Linkage) merge = (0, 1, double.MaxValue);
            for (int a = 0; a < clusters.Count; a++) {
                for (int b = a + 1; b < clusters.Count; b++) {
                    double linkage = AverageLinkage(x, clusters[a], clusters[b]);
                    if (linkage < merge.Linkage) { merge = (a, b, linkage); }
                }
            }
            best = merge.Linkage;
            clusters[merge.A].AddRange(clusters[merge.B]);
            clusters.RemoveAt(merge.B);
        }
        for (int g = 0; g < clusters.Count; g++) { foreach (int member in clusters[g]) { labels[member] = g; } }
        Matrix<double> centroids = Recenter(x, labels, clusters.Count, Matrix<double>.Build.Dense(clusters.Count, x.ColumnCount));
        double spread = Inertia(x, labels, centroids);
        // The merge sweep runs to the requested group count by construction — the `ScaleCeiling.Cubic` admission
        // IS its budget — so the terminal is the last accepted linkage distance, never a spent iteration count.
        return new FittedModel(
            ctx.Estimator, new EstimatorModel.Partition(centroids, [.. labels]),
            -spread, spread / Math.Max(1, n), None, n - clusters.Count, new Convergence.Converged(best), ctx.Clock.GetCurrentInstant());
    });

    // --- [CLASSIFY]

    // One-vs-rest LS-SVM runs one regularized-Gram KKT solve per label; binary targets reduce to two machines.
    // The dense closed form yields NO zero duals, so every training row is a support row and the carrier's
    // support set is the full index range — the honest one for this mechanism, where SMO's is genuinely sparse.
    internal static Fin<FittedModel> MarginMachines(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Classification>().Bind(policy => ctx.Supervised().Bind(y => {
            Matrix<double> x = ctx.Design.Features;
            int n = x.RowCount;
            int[] classes = [.. y.Select(static v => (int)v).Distinct().Order()];
            Matrix<double> gram = policy.Kernel.Gram(x, x, policy.Parameters);
            return toSeq(classes).TraverseM(label => Machine(gram, y, label, policy.Regularization, ctx.Substrate)).As()
                .Map(machines => {
                    EstimatorModel.Margin carrier = new(x, machines, policy.Kernel, policy.Parameters);
                    double accuracy = Enumerable.Range(0, n).Count(i => EstimatorFold.Strongest(x.Row(i), carrier) == (int)y[i]) / (double)n;
                    return new FittedModel(ctx.Estimator, carrier, accuracy, 1.0 - accuracy, None, 1, new Convergence.Converged(1.0 - accuracy), ctx.Clock.GetCurrentInstant());
                });
        }));

    // LS-SVM KKT system [[0, sᵀ],[s, K+I/γ]]·[b; α] = [0; 1] for one machine's ±1 indicator s.
    // The bordered matrix is symmetric INDEFINITE — the zero corner alone guarantees one negative eigenvalue —
    // so it rides `FactorRoute.SquarePivoting` and `Admission.Definite` never gates it; routing it to the
    // definite kernel refuses a system that is exactly solvable.
    private static Fin<SupportMachine> Machine(Matrix<double> gram, Vector<double> y, int label, double regularization, DenseSubstrate substrate) {
        int n = gram.RowCount;
        Vector<double> signed = Vector<double>.Build.Dense(n, i => (int)y[i] == label ? 1.0 : -1.0);
        Matrix<double> kkt = Matrix<double>.Build.Dense(n + 1, n + 1, (i, j) =>
            i == 0 && j == 0 ? 0.0
            : i == 0 ? signed[j - 1]
            : j == 0 ? signed[i - 1]
            : gram[i - 1, j - 1] + (i == j ? 1.0 / regularization : 0.0));
        Vector<double> rhs = Vector<double>.Build.Dense(n + 1, i => i == 0 ? 0.0 : 1.0);
        return DenseRoute.Solve(new FactorRoute.SquarePivoting(), kkt, rhs, TolerancePolicy.Derive(kkt, rhs), substrate)
            .Map(solved => new SupportMachine(label, [.. Enumerable.Range(0, n)],
                Vector<double>.Build.Dense(n, i => solved.X[i + 1] * signed[i]), solved.X[0]));
    }

    // kNN is the lazy store; quality is LEAVE-ONE-OUT accuracy (each row voted with itself excluded), because
    // plain training accuracy of a 1-NN-containing vote is unconditionally perfect — evidence, not decoration.
    internal static Fin<FittedModel> Neighborhood(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Classification>().Bind(policy => ctx.Supervised().Map(y => {
            Matrix<double> x = ctx.Design.Features;
            EstimatorModel.Neighbors carrier = new(x, [.. y.Select(static v => (int)v)], policy.Neighbors);
            double accuracy = Enumerable.Range(0, x.RowCount).Count(i => EstimatorFold.Vote(x.Row(i), carrier, exclude: i) == (int)y[i]) / (double)x.RowCount;
            return new FittedModel(ctx.Estimator, carrier, accuracy, 1.0 - accuracy, None, 0, new Convergence.Converged(1.0 - accuracy), ctx.Clock.GetCurrentInstant());
        }));

    // Per-class moments ride the kernel `Stat<Scalar>` four-moment fold, so the mean/variance pair a naive
    // sum-of-squares pass computed with catastrophic cancellation now shares the one Welford recurrence the whole
    // corpus reads, and the sentinel screen rides in with it.
    internal static Fin<FittedModel> GaussianBayes(FitContext ctx) =>
        ctx.Supervised().Bind(y => {
            Matrix<double> x = ctx.Design.Features;
            int dim = x.ColumnCount;
            int[] classes = [.. y.Select(static v => (int)v).Distinct().Order()];
            Op key = Op.Of(name: nameof(GaussianBayes));
            Matrix<double> means = Matrix<double>.Build.Dense(classes.Length, dim);
            Matrix<double> variances = Matrix<double>.Build.Dense(classes.Length, dim);
            Vector<double> priors = Vector<double>.Build.Dense(classes.Length);
            return toSeq(Enumerable.Range(0, classes.Length)).TraverseM(k => {
                int[] rows = [.. Enumerable.Range(0, x.RowCount).Where(i => (int)y[i] == classes[k])];
                priors[k] = rows.Length / (double)x.RowCount;
                return toSeq(Enumerable.Range(0, dim)).TraverseM(f =>
                    Stat<Scalar>.Of(toSeq(rows.Select(i => (Scalar)x[i, f])), key).Map(stat => {
                        means[k, f] = stat.Mean;
                        variances[k, f] = Math.Max(EstimatorFold.VarianceFloor, stat.Variance(MomentNormalizer.Sample));
                        return unit;
                    })).As();
            }).As().Map(_ => {
                EstimatorModel.Bayes carrier = new(means, variances, priors);
                double accuracy = Enumerable.Range(0, x.RowCount).Count(i => EstimatorFold.Posterior(x.Row(i), carrier) == (int)y[i]) / (double)x.RowCount;
                return new FittedModel(ctx.Estimator, carrier, accuracy, 1.0 - accuracy, None, 1, new Convergence.Converged(1.0 - accuracy), ctx.Clock.GetCurrentInstant());
            });
        });

    // --- [SEQUENTIAL_MINIMAL_OPTIMIZATION]

    // One-vs-rest C-SVM: per label the dual min ½αᵀQα − eᵀα subject to 0 ≤ αᵢ ≤ C and yᵀα = 0, with
    // Qᵢⱼ = yᵢyⱼK(xᵢ,xⱼ). Where LS-SVM equalizes every constraint into one dense solve and yields a fully dense
    // dual, the box constraint here leaves α sparse — the hard-margin support set — so the carrier's decision
    // sum walks the support alone and a large training set predicts in support-set time, not row-count time.
    internal static Fin<FittedModel> SequentialMinimal(FitContext ctx) =>
        ctx.Case<EstimatorPolicy.Classification>().Bind(policy => ctx.Supervised().Bind(y => {
            Matrix<double> x = ctx.Design.Features;
            int[] classes = [.. y.Select(static v => (int)v).Distinct().Order()];
            KernelCache cache = new(x, policy.Kernel, policy.Parameters, ctx.Rows);
            return toSeq(classes).TraverseM(label => Smo(cache, y, label, policy)).As()
                .Map(runs => {
                    EstimatorModel.Margin carrier = new(x, runs.Map(static m => m.Machine), policy.Kernel, policy.Parameters);
                    double accuracy = Enumerable.Range(0, x.RowCount).Count(i => EstimatorFold.Strongest(x.Row(i), carrier) == (int)y[i]) / (double)x.RowCount;
                    // The receipt reads SMO's own terminal evidence: `Steps` is the total across the one-vs-rest
                    // machines, the residual the WORST final gap m(α) − M(α), and the verdict `Converged` only
                    // where EVERY machine's gap crossed ε — one machine short of its gate exhausts the whole fit.
                    double worst = runs.Fold(0.0, static (gap, m) => Math.Max(gap, m.Gap));
                    return new FittedModel(
                        ctx.Estimator, carrier, accuracy, worst, None,
                        runs.Fold(0, static (total, m) => total + m.Steps),
                        runs.ForAll(static m => m.Verdict is Convergence.Converged)
                            ? new Convergence.Converged(worst)
                            : new Convergence.Exhausted(policy.Budget.MaxIterations.Value),
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

    private sealed record SmoRun(SupportMachine Machine, int Steps, double Gap, Convergence Verdict);

    // The fold HALTS at its own KKT gate. The deleted form bound a no-op through every remaining iteration of a
    // million-step budget once the gap had already closed, so the machine's own stop condition was reached and
    // then ignored for the rest of the budget.
    private static Fin<SmoRun> Smo(KernelCache cache, Vector<double> y, int label, EstimatorPolicy.Classification policy) {
        int n = cache.Rows;
        SmoState seed = new(
            Alpha: new double[n],
            Gradient: [.. Enumerable.Repeat(-1.0, n)],                     // α = 0 seeds G = Q·0 − e = −e.
            Signed: [.. Enumerable.Range(0, n).Select(i => (int)y[i] == label ? 1.0 : -1.0)],
            Active: [.. Enumerable.Repeat(true, n)],
            Steps: 0,
            Gap: double.MaxValue);
        return foldWhile(
                (Fin<SmoState> acc, int _) => acc.Bind(s => Advance(s, cache, policy)),
                state => state.State.Map(s => s.Gap > policy.KktTolerance.Value).IfFail(static _ => false),
                Fin.Succ(seed),
                toSeq(Enumerable.Range(0, policy.Budget.MaxIterations.Value)))
            .Map(s => new SmoRun(
                Machine(s, policy.Box, label), s.Steps, s.Gap,
                s.Gap <= policy.KktTolerance.Value
                    ? new Convergence.Converged(s.Gap)
                    : new Convergence.Exhausted(policy.Budget.MaxIterations.Value)));
    }

    private static Fin<SmoState> Advance(SmoState state, KernelCache cache, EstimatorPolicy.Classification policy) {
        SmoState measured = Measured(state, cache, policy);
        return measured.Gap <= policy.KktTolerance.Value
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
        if (!policy.Shrinking || up - low > policy.KktTolerance.Value * 10.0) { return state with { Gap = up - low }; }
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
        if (i < 0) { return Fin.Fail<SmoState>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input))); }
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
        if (j < 0) { return Fin.Fail<SmoState>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Feasible, new ContractEvidence.Index(i, state.Alpha.Count)))); }
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
    private static SupportMachine Machine(SmoState state, double box, int label) {
        int n = state.Alpha.Length;
        int[] support = [.. Enumerable.Range(0, n).Where(t => state.Alpha[t] > 0.0)];
        int[] free = [.. support.Where(t => state.Alpha[t] < box)];
        (double up, double low) = Violation(state, box, shrunk: false);
        double bias = free.Length > 0
            ? -free.Average(t => state.Signed[t] * state.Gradient[t])
            : -(up + low) / 2.0;
        return new SupportMachine(label, [.. support], Vector<double>.Build.Dense(n, t => state.Alpha[t] * state.Signed[t]), bias);
    }

    // --- [TEMPORAL]

    // The Gaussian conditional log-likelihood at the maximizing σ̂², ℓ = −n/2·(ln 2πσ̂² + 1) — the one form every
    // innovation-variance row shares, so a criterion ranks AR against ARMA against Holt on one scale. `n` is the
    // CONDITIONAL count each row's own warmup leaves, never the raw series length, because a row that discarded
    // more warmup would otherwise score as though it had explained those observations.
    private static Option<double> Gaussian(int conditional, double variance) =>
        conditional > 0 && variance > 0.0 && double.IsFinite(variance)
            ? Some(-0.5 * conditional * (Math.Log(2.0 * Math.PI * variance) + 1.0))
            : None;

    // Pure AR(p): the lag design Y[t] = Σ φₖ·Y[t−k] solved through the dense-algebra thin-QR — the same closed-form route OLS rides, the AR coefficients its solution.
    internal static Fin<FittedModel> AutoRegress(FitContext ctx) =>
        ctx.Temporal.ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input))).Bind(spec => {
            Vector<double> series = ctx.Design.Features.Column(0);
            int p = spec.History, n = series.Count;
            Matrix<double> design = Matrix<double>.Build.Dense(n - p, p, (i, k) => series[p + i - 1 - k]);
            Vector<double> response = Vector<double>.Build.Dense(n - p, i => series[p + i]);
            return DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), design, response, TolerancePolicy.Derive(design, response), ctx.Substrate)
                .Map(solved => {
                    Vector<double> phi = solved.X;
                    Vector<double> residual = response - design.Multiply(phi);
                    double variance = residual.DotProduct(residual) / Math.Max(1, n - p);
                    Vector<double> tail = Vector<double>.Build.Dense(p, k => series[n - 1 - k]);
                    return new FittedModel(
                        ctx.Estimator, new EstimatorModel.Lag(phi, Vector<double>.Build.Dense(0), variance, tail, TimeSeriesModel.Ar),
                        -variance, Math.Sqrt(variance), Gaussian(n - p, variance), 1, new Convergence.Converged(Math.Sqrt(variance)), ctx.Clock.GetCurrentInstant());
                });
        });

    // ARMA minimizes conditional sum-of-squares through hyperdual `LevenbergMarquardt`; `GetGradient()` supplies its exact Jacobian.
    // Exponential smoothing and state space reuse that solver with distinct Holt-error and Kalman-innovation recurrences.
    internal static Fin<FittedModel> MovingAverage(FitContext ctx) =>
        ctx.Temporal.ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input))).Bind(spec => {
            Vector<double> series = ctx.Design.Features.Column(0);
            int p = spec.History, q = p, n = series.Count;
            return n <= p + q
                ? Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(n, p + q + 1))))
                : LevenbergMarquardt.Minimize(theta => ArmaResiduals(series, p, q, theta), Vector<double>.Build.Dense(p + q), LmPolicy.Canonical)
                    .Map(lm => {
                        Vector<double> resid = Primal(ArmaResiduals(series, p, q, Constants(lm.Iterate)));
                        // Tail packs most-recent-first AR observations, then conditional MA residuals;
                        // residual slots zero-fill past the warmup.
                        Vector<double> tail = Vector<double>.Build.Dense(p + q, k =>
                            k < p ? series[n - 1 - k]
                            : resid.Count - 1 - (k - p) >= 0 ? resid[resid.Count - 1 - (k - p)] : 0.0);
                        double variance = lm.Residual * lm.Residual / Math.Max(1, n - p - q);
                        return new FittedModel(
                            ctx.Estimator, new EstimatorModel.Lag(lm.Iterate.SubVector(0, p), lm.Iterate.SubVector(p, q), variance, tail, TimeSeriesModel.Arma),
                            -variance, lm.Residual, Gaussian(n - p - q, variance), lm.Steps, Settled(lm), ctx.Clock.GetCurrentInstant());
                    });
        });

    // Holt linear-trend exponential smoothing: a genuinely distinct level+trend recurrence (NOT an ARMA roll); (α, β) is logistic-reparametrized so LevenbergMarquardt searches ℝ² unconstrained while live rates stay in (0,1).
    // Carrier stores the realized (α, β) and the terminal (level, trend) the forecast extrapolates as level + h·trend.
    internal static Fin<FittedModel> Holt(FitContext ctx) {
        Vector<double> series = ctx.Design.Features.Column(0);
        int n = series.Count;
        return n < 3
            ? Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(n, 3L))))
            : LevenbergMarquardt.Minimize(theta => HoltFilter(series, theta).Errors, Vector<double>.Build.DenseOfArray([0.0, -2.0]), LmPolicy.Canonical)
                .Map(lm => {
                    (double alpha, double beta, double level, double trend) = HoltState(series, lm.Iterate);
                    double variance = lm.Residual * lm.Residual / Math.Max(1, n - 2);
                    return new FittedModel(
                        ctx.Estimator,
                        new EstimatorModel.Lag(Vector<double>.Build.DenseOfArray([alpha, beta]), Vector<double>.Build.Dense(0), variance, Vector<double>.Build.DenseOfArray([level, trend]), TimeSeriesModel.ExponentialSmoothing),
                        -variance, lm.Residual, Gaussian(n - 2, variance), lm.Steps, Settled(lm), ctx.Clock.GetCurrentInstant());
                });
    }

    // Local-linear-trend Kalman fitting log-parameterizes `(qLevel, qSlope)` and minimizes raw innovations; standardization by `√F` admits the `q→∞` degeneracy.
    // Carrier stores filtered terminal `(level, slope)` for forecast projection.
    internal static Fin<FittedModel> StateSpace(FitContext ctx) {
        Vector<double> series = ctx.Design.Features.Column(0);
        int n = series.Count;
        return n < 4
            ? Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(n, 4L))))
            : LevenbergMarquardt.Minimize(theta => StateSpaceFilter(series, theta).Innovations, Vector<double>.Build.DenseOfArray([0.0, -3.0]), LmPolicy.Canonical)
                .Map(lm => {
                    (double level, double slope, double variance) = StateSpaceState(series, lm.Iterate);
                    return new FittedModel(
                        ctx.Estimator,
                        new EstimatorModel.Lag(Vector<double>.Build.DenseOfArray([Math.Exp(lm.Iterate[0]), Math.Exp(lm.Iterate[1])]), Vector<double>.Build.Dense(0), variance, Vector<double>.Build.DenseOfArray([level, slope]), TimeSeriesModel.StateSpace),
                        -variance, Math.Sqrt(variance), Gaussian(n - 2, variance), lm.Steps, Settled(lm), ctx.Clock.GetCurrentInstant());
                });
    }

    // The dense owner's `SolveTermination` is the SAME decision this lane's `Convergence` carries, so the
    // projection lives at ONE site: a truncated sketch never reaches an LM fit, so its arm names the impossible
    // rather than folding onto either neighbour.
    private static Convergence Settled(SolveOutcome<Vector<double>> outcome) => outcome.Termination switch {
        SolveTermination.Converged => new Convergence.Converged(outcome.Residual),
        SolveTermination.Exhausted spent => new Convergence.Exhausted(spent.Budget),
        _ => new Convergence.Stalled(),
    };

    // ONE detector baseline for all three rows: the fitted multivariate Gaussian over the warmup prefix, loaded
    // by the spec's OWN ridge column. The deleted three forwarders each re-checked `spec.Model != model` against
    // a row the spec DERIVES, so the refusal arm could never fire.
    internal static Fin<FittedModel> DetectorBaseline(FitContext ctx) =>
        ctx.Temporal.Bind(static spec => spec.Detector).ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input)))
            .Bind(spec => ctx.Case<EstimatorPolicy.Temporal>().Bind(policy => {
                Matrix<double> x = ctx.Design.Features.SubMatrix(0, spec.Warmup, 0, ctx.Design.Columns);
                Vector<double> mean = x.ColumnSums() / x.RowCount;
                Matrix<double> centered = EstimatorFold.Center(x, mean);
                Matrix<double> covariance = centered.TransposeThisAndMultiply(centered) / Math.Max(1, x.RowCount - 1) +
                    Matrix<double>.Build.DiagonalIdentity(x.ColumnCount) * spec.Loading;
                return Admission.Definite(covariance).Map(scale => {
                    double nll = x.EnumerateRows().Average(row => -EstimatorFold.LogGaussian(row, mean, scale, x.ColumnCount));
                    // A detector fits a BASELINE over the warmup prefix, not a model of the whole stream, so it
                    // carries no series log-likelihood and an information criterion is unavailable rather than misread.
                    return new FittedModel(
                        ctx.Estimator, new EstimatorModel.Detector(mean, scale, spec, policy.Budget.MaxIterations.Value),
                        -nll, nll, None, 1, new Convergence.Converged(nll), ctx.Clock.GetCurrentInstant());
                });
            }));

    // --- [DETECTION] -- the three scorers `EstimatorModel.Detector` dispatches, one per DetectorSpec case.

    internal static Prediction.Anomaly Cusum(EstimatorModel.Detector detector, Matrix<double> evidence, DetectorSpec.Cusum spec) {
        double accumulator = 0.0;
        Vector<double> scores = Vector<double>.Build.Dense(evidence.RowCount);
        bool[] changes = new bool[evidence.RowCount];
        for (int i = 0; i < evidence.RowCount; i++) {
            double innovation = Math.Sqrt(Math.Max(0.0, EstimatorFold.Mahalanobis(evidence.Row(i), detector.Mean, detector.Scale)));
            accumulator = Math.Max(0.0, accumulator + innovation - spec.Drift);
            scores[i] = accumulator;
            changes[i] = accumulator >= spec.Threshold;
            if (changes[i]) { accumulator = 0.0; }
        }
        return new Prediction.Anomaly(scores, [.. changes]);
    }

    internal static Prediction.Anomaly BayesianOnline(EstimatorModel.Detector detector, Matrix<double> evidence, DetectorSpec.BayesianOnline spec) {
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

    internal static Prediction.Anomaly CorrelatedResidual(
        EstimatorModel.Detector detector, Matrix<double> evidence, DetectorSpec.CorrelatedResidual spec) {
        double threshold = ChiSquared.InvCDF(evidence.ColumnCount, 1.0 - spec.FalsePositiveRate);
        Vector<double> scores = Vector<double>.Build.Dense(evidence.RowCount, i => EstimatorFold.Mahalanobis(evidence.Row(i), detector.Mean, detector.Scale));
        return new Prediction.Anomaly(scores, [.. scores.Select(score => score >= threshold)]);
    }

    private static double PredictiveLogGaussian(
        Vector<double> value, Vector<double> mean, Cholesky<double> scale, int dimensions, double meanPrecision) {
        double inflation = 1.0 + 1.0 / meanPrecision;
        return -0.5 * (dimensions * Math.Log(2.0 * Math.PI) + scale.DeterminantLn + dimensions * Math.Log(inflation) +
            EstimatorFold.Mahalanobis(value, mean, scale) / inflation);
    }

    // --- [FORECAST] -- the projection each TimeSeriesModel row binds.

    // AR(+MA) roll: ŷ[T+h] = Σφₖ·ŷ[T+h−1−k] + Σψₖ·ê[T+h−1−k]; a future shock has zero expectation, so the MA term decays to zero past q steps while the AR feedback continues from the rolled forecasts.
    // Pure-AR (q=0) skips the residual loop — one roll serving both lag-regression rows.
    internal static Fin<Vector<double>> ArmaForecast(EstimatorModel.Lag lag, int horizon) {
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
        return Fin.Succ(Vector<double>.Build.DenseOfArray(forecast));
    }

    // Holt and the local-trend state-space row extrapolate the IDENTICAL line off the terminal `(level, slope)`
    // pair their carriers both store, so the two forecast bodies were one body under two names.
    internal static Fin<Vector<double>> TrendForecast(EstimatorModel.Lag lag, int horizon) =>
        Fin.Succ(Vector<double>.Build.Dense(Math.Max(1, horizon), h => lag.Tail[0] + (h + 1) * lag.Tail[1]));

    // A detector carrier is `EstimatorModel.Detector` and never a `Lag`, so this arm names an unreachable request
    // rather than routing a detection row into a forecast roll.
    internal static Fin<Vector<double>> NoForecast(EstimatorModel.Lag lag, int horizon) =>
        Fin.Fail<Vector<double>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(lag.Model.Key))));

    // --- [HYPERDUAL_RECURRENCES] -- authored ONCE over DDScalar so every LM Jacobian is machine-exact.

    // Holt one-step level+trend recurrence; (α, β) arrive logistic-mapped from the LM iterate so live rates stay in (0,1).
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
        DDScalar[] constants = Constants(theta);
        (DDScalar[] Errors, DDScalar Level, DDScalar Trend) run = HoltFilter(series, constants);
        return (Logistic(constants[0]).Value, Logistic(constants[1]).Value, run.Level.Value, run.Trend.Value);
    }

    private static (double Level, double Slope, double Variance) StateSpaceState(Vector<double> series, Vector<double> theta) {
        (DDScalar[] Innovations, DDScalar Level, DDScalar Slope) run = StateSpaceFilter(series, Constants(theta));
        double[] innovations = [.. run.Innovations.Select(static v => v.Value)];
        double variance = innovations.Length > 0 ? innovations.Sum(static v => v * v) / innovations.Length : 0.0;
        return (run.Level.Value, run.Slope.Value, variance);
    }

    // 2-state (level, slope) local-linear-trend Kalman filter: transition T=[[1,1],[0,1]], observation H=[1,0] with unit measurement variance, diffuse covariance start, process variances (qLevel, qSlope); the raw innovation v feeds the LM prediction-error fit.
    // Authored ONCE over the DDScalar so the innovation Jacobian is exact THROUGH the filter recursion — the covariance/gain arithmetic differentiates too, the algorithmic derivative a finite-difference probe could only approximate.
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

    // --- [GROUPING_KERNELS]

    // Farthest-first traversal anchors row `0`; each next centroid maximizes distance from its nearest chosen row.
    private static Matrix<double> Seed(Matrix<double> x, int k) {
        int[] chosen = new int[k];
        double[] nearest = new double[x.RowCount];
        Array.Fill(nearest, double.MaxValue);
        for (int c = 1; c < k; c++) {
            for (int i = 0; i < x.RowCount; i++) { nearest[i] = Math.Min(nearest[i], EstimatorFold.Distance(x.Row(i), x.Row(chosen[c - 1]))); }
            chosen[c] = Enumerable.Range(0, x.RowCount).MaxBy(i => nearest[i]);
        }
        return Matrix<double>.Build.DenseOfRowVectors([.. chosen.Select(x.Row)]);
    }

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
        Enumerable.Range(0, x.RowCount).Where(i => labels[i] >= 0 && labels[i] < centroids.RowCount)
            .Sum(i => (x.Row(i) - centroids.Row(labels[i])).PointwisePower(2.0).Sum());

    private static int[] Region(Matrix<double> x, int point, double eps) =>
        [.. Enumerable.Range(0, x.RowCount).Where(i => EstimatorFold.Distance(x.Row(point), x.Row(i)) <= eps)];

    private static double AverageLinkage(Matrix<double> x, List<int> a, List<int> b) =>
        a.Sum(i => b.Sum(j => EstimatorFold.Distance(x.Row(i), x.Row(j)))) / (a.Count * b.Count);

    private static Matrix<double> WeightedCovariance(Matrix<double> x, Vector<double> gamma, Vector<double> mean, double mass, double ridge) {
        Matrix<double> accumulator = Matrix<double>.Build.Dense(x.ColumnCount, x.ColumnCount);
        for (int i = 0; i < x.RowCount; i++) {
            Vector<double> delta = x.Row(i) - mean;
            accumulator += gamma[i] * delta.OuterProduct(delta);
        }
        return accumulator / Math.Max(1e-9, mass) + Matrix<double>.Build.DiagonalIdentity(x.ColumnCount) * ridge;
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
