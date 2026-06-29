# [COMPUTE_ESTIMATOR]

Rasm.Compute statistical-learning lane: one `Estimator` `[Union]` carrying a uniform `Fit(X,y) → FittedModel` / `Predict(FittedModel, X) → Prediction` contract over the supervised-regression, GLM, dimensionality-reduction, clustering, classification, and time-series families, and one `EstimatorModel` `[Union]` fit-result carrier keyed by case — every learned model is a row on one Fit/Predict contract, never a per-model class. The contract is uniform but the SOLUTION MECHANISM is a row property partitioned three ways and never forced: a closed-form factorization where one exists (OLS/ridge through QR, PCA through SVD, LS-SVM through the Gram solve, AR through the lag QR) factors through the `Tensor/blas#DENSE_ALGEBRA` `DenseRoute`/`DenseOps`/`Admission`/`Factorization` surface; a smooth-convex objective (lasso L1, the canonical-link GLM deviance) folds to one `Tensor` loss minimized under `torch.autograd` + a `torch.optim` `Driver` inside one `DisposeScope` through `IterativeEngine`; and a specialized iteration that is genuinely NOT a gradient problem (k-means Lloyd, GMM-EM, NMF multiplicative updates, DBSCAN density reachability, agglomerative linkage) plus the lazy/moment fits (kNN store, Gaussian naive-Bayes moments) is its own genuine kernel — forcing k-means or EM through a `torch.optim` loss is the same deleted form as the blas lane's forced-SVD-for-k-means. Descriptive moments fold `MathNet.Numerics.Statistics` (`Mean`/`Variance`/`MeanVariance`/`Quantile`) and never a hand-rolled Welford. The `Prediction` `[Union]` egress (response vector, projection matrix, label assignment) replaces a `Vector<double>`-cramming contract with a typed result whose case matches the family. The page owns the `EstimatorKind`/`LinkFunction`/`OptimDriver`/`TimeSeriesModel` vocabulary, the `Estimator`/`EstimatorModel`/`Prediction`/`FittedModel` carriers, and the `EstimatorFold` Fit/Predict surface; the hypothesis `[2.1]-[HYPOTHESIS_LAW]` sub-section owns the `StatisticalTest` axis returning a typed `TestResult` over the matching `MathNet.Numerics.Distributions` CDF. The dense factorizations ride `Tensor/blas#DENSE_ALGEBRA`, the descriptive/distribution surfaces ride the admitted `MathNet.Numerics` assembly, and the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, `ClockPolicy`, and the `ComparerAccessors.StringOrdinal` accessor arrive settled. The time-series and classification rows CONSUME the `Stats/signal#SIGNAL_LANE` `Transform` spectral features; offline deep-training studies cross the `ONE_GRADUATION_EVIDENCE` seam from Python by content key, with C# owning classical fit plus ONNX inference and Python owning deep training.

## [01]-[INDEX]

- [01]-[ESTIMATOR_LANE]: Fit/Predict contract; closed-form (QR/SVD/Gram) + torch-loss (lasso/GLM) + specialized-iteration (Lloyd/EM/NMF/density/linkage) + lazy/moment (kNN/naive-Bayes) + temporal rows over one `EstimatorModel` carrier and one `Prediction` egress.
- the `[2.1]-[HYPOTHESIS_LAW]` sub-section owns the `StatisticalTest` axis: each row binds its statistic kernel and its p-value tail over the matching `Distributions` CDF (t/welch → `StudentT`, anova → `FisherSnedecor`, chi-square → `ChiSquared`, mann-whitney → `Normal` rank approximation) or the Kolmogorov series (ks).

## [02]-[ESTIMATOR_LANE]

- Owner: `LinkFunction` `[SmartEnum<string>]` the GLM link rows (identity/logit/log/inverse) each carrying the inverse-link `Mean(η)` (applied at predict) and the family `Variance(μ)` (the GLM dispersion weight) — a link enum nothing reads is the rejected decorative form; `OptimDriver` `[SmartEnum<string>]` the `torch.optim` rows (lbfgs/adam) each carrying the optimizer factory delegate and a `LineSearch` flag (the `LBFGS` closure form); `EstimatorKind` `[SmartEnum<string>]` the learning-algorithm rows carrying `Factorized` (a closed-form factorization fit exists, so a non-`Factorized` supervised regression row folds to a `torch` loss instead), `Supervised`, `Driver` (the `torch.optim` row the torch-loss fit minimizes under), and `Link` (the GLM link, `Identity` for the rest), the GLM rows additionally carrying the family `Deviance` torch closure; `TimeSeriesModel` `[SmartEnum<string>]` the temporal rows (ar/arma/exponential-smoothing/state-space) extending the Fit/Predict contract; `Estimator` `[Union]` the typed problem cases; `EstimatorModel` `[Union]` the fit-result carrier keyed by case (coefficients+link, principal basis, kernel dual, factor pair, partition, mixture, margin, neighbors, Bayes moments, lag), never one result type per algorithm; `Prediction` `[Union]` the typed egress (response/projection/assignment) so a reduction returns a projection matrix and a cluster returns a label assignment without cramming both into a `Vector<double>`; `IterativeEngine` the one `torch.autograd`+`torch.optim` loss-minimization owner under a `DisposeScope`; `EstimatorPolicy` the regularization/energy/iteration/tolerance/bandwidth/learning-rate/neighbor record; `FittedModel` the carrier-plus-quality result; `EstimatorFold` the static Fit/Predict surface dispatching by `Estimator` case.
- Cases: `LinkFunction` rows identity · logit · log · inverse (4); `OptimDriver` rows lbfgs · adam (2); `EstimatorKind` rows ols · ridge · lasso · glm-logistic · glm-poisson · glm-gamma · pca · kernel-pca · nmf · kmeans · gmm · dbscan · hierarchical · knn · svm · naive-bayes (16, each carrying `factorized`/`supervised`/`driver`/`link`); `TimeSeriesModel` rows ar · arma · exponential-smoothing · state-space (4); `Estimator` cases `Regression(EstimatorKind Kind, Option<LinkFunction> Link)` · `Reduction(EstimatorKind Kind, int Rank)` · `Cluster(EstimatorKind Kind, int Groups)` · `Classify(EstimatorKind Kind)` · `Temporal(TimeSeriesModel Model, int Lags)`; `EstimatorModel` cases `Linear` · `Basis` · `KernelBasis` · `Factors` · `Partition` · `Mixture` · `Margin` · `Neighbors` · `Bayes` · `Lag` (10); `Prediction` cases `Response(Vector<double>)` · `Projection(Matrix<double>)` · `Assignment(ImmutableArray<int>)` (3).
- Entry: `public static Fin<FittedModel> Fit(Estimator estimator, Matrix<double> features, Option<Vector<double>> targets, EstimatorPolicy policy, CorrelationId correlation, ClockPolicy clocks)` is the one fit fold — an in-package Stats-lane fold (the shape of the sibling `Stats/signal#SIGNAL_LANE` `Apply`, not a `ComputeIntent` admission case) whose outcome lands the dedicated `Runtime/receipts#RECEIPT_UNION` `Fit` receipt case at the sink edge — `Fin<T>` aborts on a rank-deficient design, a missing supervised target, a degenerate group count, or a non-converged iteration; `public static Fin<Prediction> Predict(FittedModel model, Matrix<double> features)` is the one predict fold projecting/assigning/responding through the `EstimatorModel` total `Switch`; the hypothesis `Test` entry lives on the `[2.1]` `Hypothesis` surface.
- Auto: `Fit` dispatches by the `Estimator` case — `Regression` routes OLS/ridge through the `Tensor/blas#DENSE_ALGEBRA` `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR over the intercept-augmented design (ridge stacking the un-penalized-intercept `√λ·I` Tikhonov block), and routes lasso/GLM through `IterativeEngine.Minimize` under the row's `Driver` (lasso the soft-thresholded least-squares objective under `Adam`, the GLM rows the canonical-link family `Deviance` under `LBFGS`, each differentiated by `backward()` inside one `torch.NewDisposeScope()`), folding the effective link `r.Link.IfNone(r.Kind.Link)` onto the carrier; `Reduction` routes PCA through `DenseOps.Decompose(centered, FactorizationKind.Svd)` truncated by `EnergyFraction`, kernel-PCA through the centered-RBF-kernel `DenseOps.Decompose(..., Evd)`, and NMF through the Lee–Seung multiplicative-update kernel; `Cluster` routes k-means through Lloyd, GMM through EM (Gaussian responsibilities via the blas `Admission.Definite` Cholesky log-density), DBSCAN through density reachability, and hierarchical through agglomerative average linkage; `Classify` routes LS-SVM through the regularized Gram `DenseRoute.Solve(FactorRoute.SquarePivoting)`, kNN through a labelled-design store, and naive-Bayes through per-class `Statistics.MeanVariance` moments; `Temporal` splits on the `TimeSeriesModel.Iterative` column — pure-AR (closed-form) through the lag-design thin-QR, and the iterative ARMA, Holt exponential-smoothing, and local-linear-trend Kalman state-space rows each through their OWN distinct conditional recurrence minimized by the one `LevenbergMarquardt` owner; `Predict` answers a `Response` (regression mean through the link, temporal forecast), a `Projection` (reduction scores, NMF encoding), or an `Assignment` (nearest-centroid cluster, posterior-argmax classifier, decision-sign margin) through the `EstimatorModel` total `Switch`.
- Receipt: the estimator fit emits the dedicated `Fit` `ComputeReceipt` case the `Runtime/receipts#RECEIPT_UNION` owner declares for this lane — the one new case row the union's one-case-per-measured-concern Growth law admits, exactly as the FEA `Solver/contract#SOLVE_CONTRACT` `Solve` case and the optimizer/sweep/clash/twin/uncertainty solver-lane cases each own a dedicated row rather than overloading a sibling — carrying the family, the estimator key (`method`), the carrier parameter count, the fit iteration count, the fit residual, the converged flag, the interpretable fit-quality value under its named metric (R²/explained-energy/inertia/log-likelihood/reconstruction-error/accuracy by family), and the retained reduction rank; a closed-form fit ALSO emits the blas `Factorization` receipt under the same `CorrelationId` (two facts, one correlation); the fit-quality value and retained rank the `FittedModel` carrier surfaces now read back operator-visibly through the receipt stream (and a stalled fit through `ReceiptFolds.Nonconverged`) instead of dying write-only on the carrier; a fit through the dense factorization rides the same provider/determinism evidence the blas lane stamps so a regression fit is reproducible.
- Packages: MathNet.Numerics, TorchSharp, libtorch-cpu, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new estimator is one `EstimatorKind` row (its `factorized`/`supervised`/`torchLoss`/`driver`/`link` columns) plus one branch arm (a closed-form row reuses the dense factorization, a torch-loss row supplies a `Tensor` loss closure to `IterativeEngine.Minimize`, a specialized row a genuine kernel) and one `EstimatorModel` case if its fit-result shape is new; a new optimizer is one `OptimDriver` row binding its `torch.optim` factory; a new GLM family is one `EstimatorKind` GLM row carrying its `Link` and `Deviance`; a new link is one `LinkFunction` row; a new time-series model is one `TimeSeriesModel` row; a new prediction shape is one `Prediction` case; a new hypothesis test is one `StatisticalTest` row binding its statistic kernel and p-value tail; zero new surface — an `OlsEstimator`/`PcaEstimator`/`KMeansEstimator` class family is the rejected form collapsed onto the one `EstimatorFold`, a `RegressionModel`/`ClusterModel`/`ReductionModel` result trio is collapsed onto the one `EstimatorModel` union, a hand-rolled `CoordinateDescent`/`Irls`/`Em` loop beside `IterativeEngine` for a row that is genuinely a torch loss is the deleted form, and a `Lloyd`/`EmFit` torch-loss masquerade for a row that is genuinely a specialized iteration is the equal-and-opposite deleted form.
- Boundary: the lane is contract-uniform — every estimator answers the one Fit/Predict contract and the solution mechanism is a row property, so a forced SVD route for k-means is wrong (k-means is density/iterative; the shared contract is Fit/Predict, not a shared factorization) and the symmetric forced-torch-loss route for k-means or EM is equally wrong (Lloyd and EM are not gradient problems — folding them to a `torch.optim` loss is the deleted masquerade); every closed-form estimator factors through the `Tensor/blas#DENSE_ALGEBRA` surface — OLS/ridge through `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR, PCA through `DenseOps.Decompose(FactorizationKind.Svd)`, kernel-PCA through the centered-kernel `Evd`, LS-SVM through the regularized-Gram `DenseRoute.Solve(FactorRoute.SquarePivoting)`, the GMM responsibility log-density through the gated `Admission.Definite` Cholesky `DeterminantLn`/`Solve` — never a hand-rolled normal-equations solve, FFT, or matrix inverse, so the regression fit shares the dense-algebra provider and determinism tag and the same intra-package owner the blas lane custodies (estimator.md and blas.md are one assembly, so this is composition, not a project edge); the OLS/ridge design prepends a ones intercept column so the fitted intercept is real and never a hardcoded `0.0`, and the ridge Tikhonov block zeroes the intercept's penalty row so the bias is un-regularized; lasso has no closed form so the L1 row folds to the `IterativeEngine` `torch` loss under `Adam` while the L2/OLS rows ride the factorization — one Fit fold, two mechanisms by row; the GLM rows fold the canonical-link family `Deviance` carried on the `EstimatorKind` row (the `softplus(η)−y·η` Bernoulli / `exp(η)−y·η` Poisson / `η+y·exp(−η)` log-link Gamma terms, reverse-mode-differentiated by `backward()`) to a `torch` loss minimized under `LBFGS` rather than a hand-rolled IRLS loop, and the GLM `Predict` applies the row's `Link.Mean` so a logistic fit egresses probabilities, not the bare linear predictor — a GLM that predicts the linear predictor without the inverse link, or a `LinkFunction` enum no fit or predict reads, is the deleted decorative form, and the `Identity` link's `Variance(μ)=1` is the Gaussian constant, never the Bernoulli `μ(1−μ)` the row must NOT carry; the `naive-bayes` row fits per-class `Statistics.MeanVariance` moments through the admitted assembly rather than a hand-rolled Gaussian, the `kmeans`/`gmm`/`nmf`/`dbscan`/`hierarchical` rows bind their genuine Lloyd/EM/multiplicative-update/density/linkage kernels (the in-place Lloyd assignment, EM responsibility, NMF update, DBSCAN region-query, and agglomerative-merge loops are this page's statement exemption), and `knn` stores the labelled design for a lazy vote — the shared contract is Fit/Predict, not a shared solution mechanism; every `IterativeEngine` fit runs inside one `torch.NewDisposeScope()` so no native ATen tensor leaks to the GC, the verified `AnomalyMode(enabled: true, check_nan: true)` `IDisposable` traps a NaN/inf fit to a typed `ComputeFault` instead of a silently-diverged coefficient, `set_default_dtype(Float64)` floors the math at double, and only the egressed `Vector<double>` coefficient projection crosses the lane boundary — a `Tensor` escaping onto the wire or a reliance on finalization for native reclamation is the deleted form, the `DisposeScopeManager.Statistics` `ThreadDisposeScopeStatistics` memory evidence riding the fit; the descriptive moments fold `Statistics.Mean`/`Variance`/`MeanVariance` and the quantiles read `Statistics.Quantile`, never a hand-rolled accumulator, and the regression quality reads `GoodnessOfFit.CoefficientOfDetermination` rather than a hand-derived R²; the `Predict` egress is the typed `Prediction` union switched through the `EstimatorModel` total generated `Switch` — the prior `model.Carrier switch { … _ => Fin.Fail }` non-total C# switch with a runtime-silent `_` arm over a union is the deleted form, because a union dispatch must be compile-time exhaustive and a reduction's projection matrix or a cluster's label assignment must not be flattened into a `Vector<double>`; the fit-quality and retained rank ride the `FittedModel` carrier AND the dedicated `Fit` `ComputeReceipt` case the `Runtime/receipts#RECEIPT_UNION` owner declares for this lane — a new measured concern is one case row on the union per its Growth law, exactly the dedicated-case discipline the FEA `Solve` and the optimizer/sweep/clash/twin/uncertainty solver lanes hold, never a semantic overload of the FEA `Solve` case whose `physics`/`dofs` slots a regression family and a carrier parameter count never truly were, so a stalled k-means/GLM/EM fit lands as a `Fit` fact `ReceiptFolds.Nonconverged` reads back apart from the FEA-solver `Diverged` view rather than polluting it; the time-series and classification rows CONSUME the `Stats/signal#SIGNAL_LANE` `Transform` spectral features (the signal lane is the producer, the estimator the consumer — the bidirectional record the signal page states), the hypothesis tests can validate the `Solver/uncertainty#UNCERTAINTY_LANE` response samples, and offline deep-training models cross the `ONE_GRADUATION_EVIDENCE` seam from Python by content key (C# owns inference plus classical fit, Python owns deep training — an in-proc deep-training loop is the rejected form because the training role belongs to the Python branch); the optimizer's `Surrogate` (GP/RBF/POD) and the digital-twin baseline are the `Solver/optimizer#OPTIMIZER_LANE`'s OWN owners fitted in its acquisition loop — this lane does not own or feed them, so a claim that the optimizer and the stats lane share one regression fold is the deleted illusory seam.

```csharp signature
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
    public static readonly EstimatorKind Ols = new("ols", factorized: true, supervised: true);
    public static readonly EstimatorKind Ridge = new("ridge", factorized: true, supervised: true);
    public static readonly EstimatorKind Lasso = new("lasso", supervised: true, driver: OptimDriver.Adam);
    public static readonly EstimatorKind GlmLogistic = new("glm-logistic", supervised: true, driver: OptimDriver.LBfgs, link: LinkFunction.Logit, deviance: static (eta, y) => (eta.exp().log1p() - y * eta).mean());
    public static readonly EstimatorKind GlmPoisson = new("glm-poisson", supervised: true, driver: OptimDriver.LBfgs, link: LinkFunction.Log, deviance: static (eta, y) => (eta.exp() - y * eta).mean());
    public static readonly EstimatorKind GlmGamma = new("glm-gamma", supervised: true, driver: OptimDriver.LBfgs, link: LinkFunction.Log, deviance: static (eta, y) => (eta + y * eta.neg().exp()).mean());
    public static readonly EstimatorKind Pca = new("pca", factorized: true, supervised: false);
    public static readonly EstimatorKind KernelPca = new("kernel-pca", factorized: true, supervised: false);
    public static readonly EstimatorKind Nmf = new("nmf", factorized: false, supervised: false);
    public static readonly EstimatorKind KMeans = new("kmeans", factorized: false, supervised: false);
    public static readonly EstimatorKind Gmm = new("gmm", factorized: false, supervised: false);
    public static readonly EstimatorKind Dbscan = new("dbscan", factorized: false, supervised: false);
    public static readonly EstimatorKind Hierarchical = new("hierarchical", factorized: false, supervised: false);
    public static readonly EstimatorKind Knn = new("knn", factorized: false, supervised: true);
    public static readonly EstimatorKind Svm = new("svm", factorized: true, supervised: true);
    public static readonly EstimatorKind NaiveBayes = new("naive-bayes", factorized: false, supervised: true);

    // factorized rows ride the dense-algebra route; a non-factorized supervised regression row folds to a
    // torch loss under Driver; the remainder bind a specialized kernel selected by the Estimator case. Link
    // is Identity except on the GLM rows; Deviance is the GLM family NLL term (null-forgiving — reached only
    // by the GLM regression branch). The user constructor delegates the key to the generated `this(key)`.
    private EstimatorKind(string key, bool factorized = false, bool supervised = false, OptimDriver? driver = null, LinkFunction? link = null, Func<Tensor, Tensor, Tensor>? deviance = null) : this(key) {
        Factorized = factorized;
        Supervised = supervised;
        Driver = driver ?? OptimDriver.Adam;
        Link = link ?? LinkFunction.Identity;
        this.deviance = deviance;
    }

    private readonly Func<Tensor, Tensor, Tensor>? deviance;

    public bool Factorized { get; }
    public bool Supervised { get; }
    public OptimDriver Driver { get; }
    public LinkFunction Link { get; }

    public Func<Tensor, Tensor, Tensor> Deviance => deviance!;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimeSeriesModel {
    public static readonly TimeSeriesModel Ar = new("ar", iterative: false);
    public static readonly TimeSeriesModel Arma = new("arma", iterative: true);
    public static readonly TimeSeriesModel ExponentialSmoothing = new("exponential-smoothing", iterative: true);
    public static readonly TimeSeriesModel StateSpace = new("state-space", iterative: true);

    public bool Iterative { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Estimator {
    private Estimator() { }

    public sealed record Regression(EstimatorKind Kind, Option<LinkFunction> Link) : Estimator;
    public sealed record Reduction(EstimatorKind Kind, int Rank) : Estimator;
    public sealed record Cluster(EstimatorKind Kind, int Groups) : Estimator;
    public sealed record Classify(EstimatorKind Kind) : Estimator;
    public sealed record Temporal(TimeSeriesModel Model, int Lags) : Estimator;

    public string Family => Switch(
        regression: static _ => "regression", reduction: static _ => "reduction", cluster: static _ => "cluster",
        classify: static _ => "classify", temporal: static _ => "temporal");

    // One uniform estimator key for the receipt — the row key for the learning families, the model key for
    // temporal, never a fabricated EstimatorKind.Ols stand-in for a case that carries no EstimatorKind.
    public string Key => Switch(
        regression: static r => r.Kind.Key, reduction: static d => d.Kind.Key, cluster: static c => c.Kind.Key,
        classify: static c => c.Kind.Key, temporal: static t => t.Model.Key);

    // The fit-quality metric NAME the receipt's QualityMetric label carries so a downstream fold reads R²
    // apart from explained-energy apart from inertia apart from log-likelihood without re-deriving the family;
    // FittedModel.Quality is the higher-is-better VALUE, this names the underlying metric the value scores.
    public string QualityMetric => Switch(
        regression: static _ => "r2",
        reduction: static d => d.Kind == EstimatorKind.Nmf ? "reconstruction-error" : "explained-energy",
        cluster: static c => c.Kind == EstimatorKind.Gmm ? "log-likelihood" : c.Kind == EstimatorKind.Dbscan ? "cluster-count" : "inertia",
        classify: static c => c.Kind == EstimatorKind.Svm ? "accuracy" : "label-fit",
        temporal: static _ => "innovation-variance");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimatorModel {
    private EstimatorModel() { }

    public sealed record Linear(Vector<double> Coefficients, double Intercept, LinkFunction Link) : EstimatorModel;
    public sealed record Basis(Matrix<double> Components, Vector<double> Singular, Vector<double> Mean, double EnergyFraction) : EstimatorModel;
    public sealed record KernelBasis(Matrix<double> Training, Matrix<double> Alphas, Vector<double> Eigen, double Bandwidth, Vector<double> RowMean, double GrandMean) : EstimatorModel;
    public sealed record Factors(Matrix<double> Encoder, Matrix<double> Components) : EstimatorModel;
    public sealed record Partition(Matrix<double> Centroids, ImmutableArray<int> Labels) : EstimatorModel;
    public sealed record Mixture(Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights) : EstimatorModel;
    public sealed record Margin(Matrix<double> Training, Vector<double> Duals, double Bias, double Bandwidth) : EstimatorModel;
    public sealed record Neighbors(Matrix<double> Design, ImmutableArray<int> Labels, int K) : EstimatorModel;
    public sealed record Bayes(Matrix<double> Means, Matrix<double> Variances, Vector<double> Priors) : EstimatorModel;
    public sealed record Lag(Vector<double> ArCoefficients, Vector<double> MaCoefficients, double Variance, Vector<double> Tail, TimeSeriesModel Model) : EstimatorModel;

    public long ParameterCount => Switch(
        linear: static c => (long)c.Coefficients.Count,
        basis: static b => (long)b.Components.RowCount,
        kernelBasis: static k => (long)k.Eigen.Count,
        factors: static f => (long)f.Components.RowCount * f.Components.ColumnCount,
        partition: static p => (long)p.Centroids.RowCount,
        mixture: static m => (long)m.Weights.Count,
        margin: static m => (long)m.Duals.Count,
        neighbors: static n => (long)n.Design.RowCount,
        bayes: static b => (long)b.Priors.Count,
        lag: static l => (long)(l.ArCoefficients.Count + l.MaCoefficients.Count));

    // The retained reduction rank the receipt's RetainedRank slot carries — the basis/kernel-basis component
    // count and the NMF inner dimension; zero on the non-reduction carriers so a non-reduction fit reads
    // "no retained rank" without a family probe.
    public int RetainedRank => Switch(
        basis: static b => b.Singular.Count,
        kernelBasis: static k => k.Eigen.Count,
        factors: static f => f.Encoder.ColumnCount,
        linear: static _ => 0, partition: static _ => 0, mixture: static _ => 0,
        margin: static _ => 0, neighbors: static _ => 0, bayes: static _ => 0, lag: static _ => 0);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Prediction {
    private Prediction() { }

    public sealed record Response(Vector<double> Values) : Prediction;
    public sealed record Projection(Matrix<double> Scores) : Prediction;
    public sealed record Assignment(ImmutableArray<int> Labels) : Prediction;
}

public sealed record EstimatorPolicy(double Regularization, double EnergyFraction, int MaxIterations, double Tolerance, double KernelBandwidth, double LearningRate, int Neighbors) {
    public static readonly EstimatorPolicy CanonicalRegression = new(Regularization: 1e-3, EnergyFraction: 0.99, MaxIterations: 1000, Tolerance: 1e-8, KernelBandwidth: 1.0, LearningRate: 0.05, Neighbors: 5);
    public static readonly EstimatorPolicy CanonicalReduction = CanonicalRegression with { EnergyFraction: 0.95 };
    public static readonly EstimatorPolicy CanonicalCluster = CanonicalRegression with { MaxIterations: 300 };
    public static readonly EstimatorPolicy CanonicalDensity = CanonicalCluster with { KernelBandwidth: 0.5, Neighbors: 4 };
}

public sealed record FittedModel(Estimator Estimator, EstimatorModel Carrier, double Quality, double Residual, int Iterations, bool Converged, Instant At) {
    public Fin<Prediction> Predict(Matrix<double> features) => EstimatorFold.Predict(this, features);
}

// The torch-loss EstimatorKind rows fold to a Tensor loss minimized under torch.autograd + the row's
// OptimDriver inside one DisposeScope: the design and response ingress through torch.from_array, the loss
// closure is differentiated by backward(), the LBFGS line-search form re-evaluates the closure per probe,
// and the surviving coefficient Tensor egresses to a Vector<double> for the EstimatorModel carrier. Every
// intermediate is reclaimed at scope exit and stamped by DisposeScopeManager.Statistics; AnomalyMode traps a
// NaN/inf fit to a typed fault. set_default_dtype floors the math at Float64. No Tensor escapes the lane.
public static class IterativeEngine {
    public static Fin<(Vector<double> Theta, double Loss, int Iterations, bool Converged, ThreadDisposeScopeStatistics Memory)> Minimize(
        Func<Tensor, Tensor, Tensor, Tensor> loss, Matrix<double> design, Vector<double> response, OptimDriver driver, EstimatorPolicy policy) {
        using var scope = torch.NewDisposeScope();
        using var anomaly = new AnomalyMode(enabled: true, check_nan: true);
        torch.set_default_dtype(ScalarType.Float64);
        Tensor x = torch.from_array(design.ToColumnMajorArray(), ScalarType.Float64).reshape(design.ColumnCount, design.RowCount).t();
        Tensor y = torch.from_array(response.AsArray() ?? response.ToArray(), ScalarType.Float64).reshape(response.Count);
        var theta = new Parameter(torch.zeros(design.ColumnCount, ScalarType.Float64), requires_grad: true);
        var opt = driver.Bind([theta], policy.LearningRate);
        Tensor Step() { opt.zero_grad(); Tensor l = loss(theta, x, y); l.backward(); return l; }
        double last = double.MaxValue;
        int iter = 0;
        bool converged = false;
        for (; iter < policy.MaxIterations; iter++) {
            Tensor value = driver.LineSearch ? ((Modules.LBFGS)opt).step(Step) : Plain(opt, Step);
            double current = value.ReadCpuDouble(0);
            converged = Math.Abs(last - current) < policy.Tolerance;
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

    public static Func<Tensor, Tensor, Tensor, Tensor> Glm(EstimatorKind kind) =>
        (theta, x, y) => kind.Deviance(x.matmul(theta), y);

    static Tensor Plain(torch.optim.Optimizer opt, Func<Tensor> closure) { Tensor l = closure(); opt.step(); return l; }
}

public static class EstimatorFold {
    public static Fin<FittedModel> Fit(Estimator estimator, Matrix<double> features, Option<Vector<double>> targets, EstimatorPolicy policy, CorrelationId correlation, ClockPolicy clocks) =>
        estimator.Switch(
            state: (Features: features, Targets: targets, Policy: policy, Clock: clocks),
            regression: static (s, r) => FitRegression(r, s.Features, s.Targets, s.Policy, s.Clock),
            reduction: static (s, d) => FitReduction(d, s.Features, s.Policy, s.Clock),
            cluster: static (s, c) => FitCluster(c, s.Features, s.Policy, s.Clock),
            classify: static (s, c) => FitClassifier(c, s.Features, s.Targets, s.Policy, s.Clock),
            temporal: static (s, t) => FitTemporal(t, s.Features, s.Policy, s.Clock));

    public static Fin<Prediction> Predict(FittedModel model, Matrix<double> features) =>
        model.Carrier.Switch<Matrix<double>, Fin<Prediction>>(
            state: features,
            linear: static (x, c) => Fin.Succ<Prediction>(new Prediction.Response(x.Multiply(c.Coefficients).Add(c.Intercept).Map(c.Link.Mean))),
            basis: static (x, b) => Fin.Succ<Prediction>(new Prediction.Projection(Center(x, b.Mean).Multiply(b.Components.Transpose()))),
            kernelBasis: static (x, k) => Fin.Succ<Prediction>(new Prediction.Projection(KernelProject(x, k))),
            factors: static (x, f) => Fin.Succ<Prediction>(new Prediction.Projection(NonNegativeEncode(x, f.Components))),
            partition: static (x, p) => Fin.Succ<Prediction>(new Prediction.Assignment([.. x.EnumerateRows().Select(row => Nearest(row, p.Centroids))])),
            mixture: static (x, m) => Responsibilities(x, m).Map(static labels => (Prediction)new Prediction.Assignment(labels)),
            margin: static (x, m) => Fin.Succ<Prediction>(new Prediction.Assignment([.. x.EnumerateRows().Select(row => Decision(row, m) >= 0.0 ? 1 : -1)])),
            neighbors: static (x, nbr) => Fin.Succ<Prediction>(new Prediction.Assignment([.. x.EnumerateRows().Select(row => Vote(row, nbr))])),
            bayes: static (x, b) => Fin.Succ<Prediction>(new Prediction.Assignment([.. x.EnumerateRows().Select(row => Posterior(row, b))])),
            lag: static (x, l) => Fin.Succ<Prediction>(new Prediction.Response(Forecast(l, x.RowCount))));

    public static ComputeReceipt Receipt(FittedModel model, CorrelationId correlation, Duration elapsed) =>
        new ComputeReceipt.Fit(model.Estimator.Family, model.Estimator.Key, model.Carrier.ParameterCount, model.Iterations, model.Residual, model.Converged, model.Quality, model.Estimator.QualityMetric, model.Carrier.RetainedRank) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    // --- [REGRESSION] ---------------------------------------------------------------------

    static Fin<FittedModel> FitRegression(Estimator.Regression r, Matrix<double> x, Option<Vector<double>> y, EstimatorPolicy policy, ClockPolicy clocks) =>
        y.ToFin(ComputeFault.Create("<estimator-regression-needs-targets>")).Bind(targets => {
            LinkFunction link = r.Link.IfNone(r.Kind.Link);
            return r.Kind.Factorized
                ? Closed(r, x, targets, policy, link, clocks)
                : IterativeEngine.Minimize(r.Kind == EstimatorKind.Lasso ? IterativeEngine.Lasso(policy.Regularization) : IterativeEngine.Glm(r.Kind), Intercept(x), targets, r.Kind.Driver, policy)
                    .Map(fit => Build(r, x, targets, Split(fit.Theta), link, fit.Loss, fit.Iterations, fit.Converged, clocks));
        });

    // OLS/ridge: thin-QR over the intercept-augmented design through the dense-algebra route; ridge stacks the
    // un-penalized-intercept √λ·I Tikhonov block onto the design and zero-pads the response.
    static Fin<FittedModel> Closed(Estimator.Regression r, Matrix<double> x, Vector<double> y, EstimatorPolicy policy, LinkFunction link, ClockPolicy clocks) {
        Matrix<double> design = Intercept(x);
        bool ridge = r.Kind == EstimatorKind.Ridge;
        Matrix<double> a = ridge ? design.Stack(Tikhonov(design.ColumnCount, policy.Regularization)) : design;
        Vector<double> b = ridge ? Vector<double>.Build.Dense(design.RowCount + design.ColumnCount, i => i < design.RowCount ? y[i] : 0.0) : y;
        return DenseRoute.Solve(new FactorRoute.Orthonormal(a, QRMethod.Thin, Modified: false), b, TolerancePolicy.Derive(a, b))
            .Map(theta => Build(r, x, y, Split(theta), link, 0.0, 1, true, clocks));
    }

    static FittedModel Build(Estimator estimator, Matrix<double> x, Vector<double> y, (double Intercept, Vector<double> Slopes) split, LinkFunction link, double loss, int iterations, bool converged, ClockPolicy clocks) {
        var carrier = new EstimatorModel.Linear(split.Slopes, split.Intercept, link);
        Vector<double> predicted = x.Multiply(split.Slopes).Add(split.Intercept).Map(link.Mean);
        double r2 = GoodnessOfFit.CoefficientOfDetermination(predicted, y);
        // Pearson dispersion √(Σ (yᵢ−μᵢ)²/V(μᵢ) / (n−p)): the family variance V(μ) the LinkFunction row carries
        // weights each squared residual, so a logistic fit scores on its Bernoulli scale (V=μ(1−μ)) and the
        // Identity link (V=1) recovers the Gaussian residual standard error — never an unweighted RMSE blind to
        // the link variance, the residual NOT reading `Link.Variance` being the decorative form this deletes.
        int dispersionDof = Math.Max(1, y.Count - split.Slopes.Count - 1);
        double dispersion = Math.Sqrt(Enumerable.Range(0, y.Count).Sum(i => Math.Pow(y[i] - predicted[i], 2) / Math.Max(1e-12, link.Variance(predicted[i]))) / dispersionDof);
        return new FittedModel(estimator, carrier, r2, double.IsFinite(dispersion) ? dispersion : loss, iterations, converged, clocks.Now);
    }

    // --- [REDUCTION] ----------------------------------------------------------------------

    static Fin<FittedModel> FitReduction(Estimator.Reduction d, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) =>
        d.Kind == EstimatorKind.Pca ? FitPca(d, x, policy, clocks)
        : d.Kind == EstimatorKind.KernelPca ? FitKernelPca(d, x, policy, clocks)
        : d.Kind == EstimatorKind.Nmf ? FitNmf(d, x, policy, clocks)
        : Fin.Fail<FittedModel>(ComputeFault.Create($"<reduction-kind-miss:{d.Kind.Key}>"));

    static Fin<FittedModel> FitPca(Estimator.Reduction d, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) {
        Vector<double> mean = x.ColumnSums() / x.RowCount;
        Matrix<double> centered = Center(x, mean);
        return DenseOps.Decompose(centered, FactorizationKind.Svd).Bind(factor => factor.Switch(
            lu: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<pca-non-svd>")),
            qr: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<pca-non-svd>")),
            cholesky: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<pca-non-svd>")),
            evd: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<pca-non-svd>")),
            svd: f => {
                Vector<double> singular = f.Decomposition.S;
                double total = singular.PointwisePower(2.0).Sum();
                int rank = Retain(singular, total, Math.Min(d.Rank <= 0 ? singular.Count : d.Rank, singular.Count), policy.EnergyFraction);
                Matrix<double> components = f.Decomposition.VT.SubMatrix(0, rank, 0, x.ColumnCount);
                double energy = total > 0.0 ? singular.SubVector(0, rank).PointwisePower(2.0).Sum() / total : 0.0;
                var carrier = new EstimatorModel.Basis(components, singular.SubVector(0, rank), mean, energy);
                return Fin.Succ(new FittedModel(d, carrier, energy, 1.0 - energy, 1, true, clocks.Now));
            }));
    }

    // Kernel-PCA: the centered RBF Gram matrix is the operand, its top eigenvectors the dual coefficients;
    // out-of-sample projection double-centers the test/train kernel against the stored row/grand means.
    static Fin<FittedModel> FitKernelPca(Estimator.Reduction d, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) {
        int n = x.RowCount;
        Matrix<double> gram = Gram(x, x, policy.KernelBandwidth);
        Vector<double> rowMean = gram.RowSums() / n;
        double grandMean = rowMean.Sum() / n;
        Matrix<double> centered = Matrix<double>.Build.Dense(n, n, (i, j) => gram[i, j] - rowMean[i] - rowMean[j] + grandMean);
        return DenseOps.Decompose(Admission.Symmetrize(centered), FactorizationKind.Evd).Bind(factor => factor.Switch(
            lu: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<kernel-pca-non-evd>")),
            qr: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<kernel-pca-non-evd>")),
            cholesky: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<kernel-pca-non-evd>")),
            svd: static _ => Fin.Fail<FittedModel>(ComputeFault.Create("<kernel-pca-non-evd>")),
            evd: f => {
                Vector<double> values = f.Decomposition.EigenValues.Map(static v => v.Real);
                int rank = Math.Min(d.Rank <= 0 ? n : d.Rank, n);
                int[] order = [.. Enumerable.Range(0, n).OrderByDescending(i => values[i]).Take(rank)];
                Vector<double> eigen = Vector<double>.Build.DenseOfArray([.. order.Select(i => values[i])]);
                Matrix<double> alphas = Matrix<double>.Build.Dense(n, rank, (i, c) => f.Decomposition.EigenVectors[i, order[c]] / Math.Sqrt(Math.Max(1e-12, eigen[c])));
                var carrier = new EstimatorModel.KernelBasis(x, alphas, eigen, policy.KernelBandwidth, rowMean, grandMean);
                double captured = eigen.Sum() / Math.Max(1e-12, values.Map(Math.Abs).Sum());
                return Fin.Succ(new FittedModel(d, carrier, captured, 1.0 - captured, 1, true, clocks.Now));
            }));
    }

    // NMF (Lee–Seung multiplicative updates): X ≈ W·H, W,H ≥ 0, minimizing the Frobenius reconstruction.
    static Fin<FittedModel> FitNmf(Estimator.Reduction d, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) {
        int n = x.RowCount, m = x.ColumnCount, k = Math.Max(1, d.Rank);
        Matrix<double> w = Matrix<double>.Build.Random(n, k, 1).PointwiseAbs().Add(1e-3);
        Matrix<double> h = Matrix<double>.Build.Random(k, m, 2).PointwiseAbs().Add(1e-3);
        Matrix<double> shifted = x.PointwiseMaximum(0.0);
        double residual = double.MaxValue;
        int iter = 0;
        bool converged = false;
        for (; iter < policy.MaxIterations; iter++) {
            h = h.PointwiseMultiply((w.TransposeThisAndMultiply(shifted)).PointwiseDivide(w.TransposeThisAndMultiply(w).Multiply(h).Add(1e-12)));
            w = w.PointwiseMultiply((shifted.TransposeAndMultiply(h)).PointwiseDivide(w.Multiply(h.TransposeAndMultiply(h)).Add(1e-12)));
            double next = (shifted - w.Multiply(h)).FrobeniusNorm();
            converged = Math.Abs(residual - next) < policy.Tolerance;
            residual = next;
            if (converged) { iter++; break; }
        }
        return Fin.Succ(new FittedModel(d, new EstimatorModel.Factors(w, h), -residual, residual, iter, converged, clocks.Now));
    }

    // --- [CLUSTER] ------------------------------------------------------------------------

    static readonly FrozenDictionary<EstimatorKind, Func<Estimator.Cluster, Matrix<double>, EstimatorPolicy, ClockPolicy, Fin<FittedModel>>> ClusterKernels =
        new (EstimatorKind Kind, Func<Estimator.Cluster, Matrix<double>, EstimatorPolicy, ClockPolicy, Fin<FittedModel>> Fit)[] {
            (EstimatorKind.KMeans, Lloyd),
            (EstimatorKind.Gmm, ExpectationMaximization),
            (EstimatorKind.Dbscan, Dbscan),
            (EstimatorKind.Hierarchical, Agglomerative),
        }.ToFrozenDictionary(static row => row.Kind, static row => row.Fit);

    static Fin<FittedModel> FitCluster(Estimator.Cluster c, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) =>
        ClusterKernels.TryGetValue(c.Kind, out var kernel)
            ? kernel(c, x, policy, clocks)
            : Fin.Fail<FittedModel>(ComputeFault.Create($"<cluster-kind-miss:{c.Kind.Key}>"));

    static Fin<FittedModel> Lloyd(Estimator.Cluster c, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) {
        int n = x.RowCount, k = c.Groups;
        if (k <= 0 || k > n) { return Fin.Fail<FittedModel>(ComputeFault.Create($"<kmeans-groups:{k}>")); }
        Matrix<double> centroids = Seed(x, k);
        var labels = new int[n];
        int iter = 0;
        bool moved = true;
        for (; iter < policy.MaxIterations && moved; iter++) {
            moved = false;
            for (int i = 0; i < n; i++) {
                int best = Nearest(x.Row(i), centroids);
                if (best != labels[i]) { labels[i] = best; moved = true; }
            }
            centroids = Recenter(x, labels, k);
        }
        double inertia = Inertia(x, labels, centroids);
        return Fin.Succ(new FittedModel(c, new EstimatorModel.Partition(centroids, [.. labels]), -inertia, inertia / Math.Max(1, n), iter, !moved, clocks.Now));
    }

    // GMM-EM as a Fin-threaded fold: each EmStep computes responsibilities through the gated Cholesky
    // log-density and re-estimates (weights, means, covariances), short-circuiting once the log-likelihood
    // stalls — a Cholesky failure on a degenerate covariance aborts the whole fit through the rail.
    static Fin<FittedModel> ExpectationMaximization(Estimator.Cluster c, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) {
        int n = x.RowCount, dim = x.ColumnCount, k = c.Groups;
        if (k <= 0 || k > n) { return Fin.Fail<FittedModel>(ComputeFault.Create($"<gmm-groups:{k}>")); }
        var seed = Fin.Succ((
            Means: Seed(x, k),
            Covariances: toSeq(Enumerable.Range(0, k).Select(_ => (Matrix<double>)Matrix<double>.Build.DiagonalIdentity(dim))),
            Weights: Vector<double>.Build.Dense(k, 1.0 / k),
            LogLik: double.NegativeInfinity, Iterations: 0, Converged: false));
        return toSeq(Enumerable.Range(0, policy.MaxIterations))
            .Fold(seed, (acc, _) => acc.Bind(s => s.Converged ? acc : EmStep(x, s, dim, policy)))
            .Map(s => new FittedModel(c, new EstimatorModel.Mixture(s.Means, s.Covariances, s.Weights), s.LogLik, double.IsFinite(s.LogLik) ? -s.LogLik / Math.Max(1, n) : double.MaxValue, s.Iterations, s.Converged, clocks.Now));
    }

    static Fin<(Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights, double LogLik, int Iterations, bool Converged)> EmStep(
        Matrix<double> x, (Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights, double LogLik, int Iterations, bool Converged) s, int dim, EstimatorPolicy policy) =>
        Choleskies(s.Covariances, policy.Regularization).Map(chols => {
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
            Seq<Matrix<double>> covariances = toSeq(Enumerable.Range(0, k).Select(j => WeightedCovariance(x, gamma.Column(j), means.Row(j), nk[j], policy.Regularization)));
            return (means, covariances, nk / n, evidence, s.Iterations + 1, Math.Abs(evidence - s.LogLik) < policy.Tolerance);
        });

    static Fin<FittedModel> Dbscan(Estimator.Cluster c, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) {
        int n = x.RowCount, minPts = policy.Neighbors;
        double eps = policy.KernelBandwidth;
        var labels = Enumerable.Repeat(-2, n).ToArray();
        int cluster = -1;
        for (int i = 0; i < n; i++) {
            if (labels[i] != -2) { continue; }
            int[] neighbors = Region(x, i, eps);
            if (neighbors.Length < minPts) { labels[i] = -1; continue; }
            cluster++;
            var frontier = new Queue<int>(neighbors);
            labels[i] = cluster;
            while (frontier.Count > 0) {
                int q = frontier.Dequeue();
                if (labels[q] == -1) { labels[q] = cluster; }
                if (labels[q] != -2) { continue; }
                labels[q] = cluster;
                int[] reach = Region(x, q, eps);
                if (reach.Length >= minPts) { foreach (int r in reach) { frontier.Enqueue(r); } }
            }
        }
        Matrix<double> centroids = Recenter(x, labels, Math.Max(1, cluster + 1));
        double spread = Inertia(x, labels, centroids);
        return Fin.Succ(new FittedModel(c, new EstimatorModel.Partition(centroids, [.. labels]), cluster + 1, spread / Math.Max(1, n), 1, true, clocks.Now));
    }

    static Fin<FittedModel> Agglomerative(Estimator.Cluster c, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) {
        int n = x.RowCount, target = Math.Max(1, c.Groups);
        var labels = Enumerable.Range(0, n).ToArray();
        var clusters = Enumerable.Range(0, n).Select(i => new List<int> { i }).ToList();
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
        Matrix<double> centroids = Recenter(x, labels, clusters.Count);
        double spread = Inertia(x, labels, centroids);
        return Fin.Succ(new FittedModel(c, new EstimatorModel.Partition(centroids, [.. labels]), -spread, spread / Math.Max(1, n), n - clusters.Count, true, clocks.Now));
    }

    // --- [CLASSIFY] -----------------------------------------------------------------------

    static Fin<FittedModel> FitClassifier(Estimator.Classify c, Matrix<double> x, Option<Vector<double>> y, EstimatorPolicy policy, ClockPolicy clocks) =>
        y.ToFin(ComputeFault.Create("<estimator-classify-needs-targets>")).Bind(targets =>
            c.Kind == EstimatorKind.Svm ? FitSvm(c, x, targets, policy, clocks)
            : c.Kind == EstimatorKind.Knn ? Fin.Succ(new FittedModel(c, new EstimatorModel.Neighbors(x, [.. targets.Select(static v => (int)v)], policy.Neighbors), 1.0, 0.0, 0, true, clocks.Now))
            : c.Kind == EstimatorKind.NaiveBayes ? FitNaiveBayes(c, x, targets, policy, clocks)
            : Fin.Fail<FittedModel>(ComputeFault.Create($"<classify-kind-miss:{c.Kind.Key}>")));

    // LS-SVM: the regularized-Gram KKT system [[0, yᵀ],[y, K+I/γ]]·[b; α] = [0; 1] solved once through the
    // dense-algebra square route — the closed-form kernel classifier; the box-constrained C-SVM dual (SMO /
    // projected gradient) is the noted alternative where the hard-margin sparsity is required.
    static Fin<FittedModel> FitSvm(Estimator.Classify c, Matrix<double> x, Vector<double> y, EstimatorPolicy policy, ClockPolicy clocks) {
        int n = x.RowCount;
        Vector<double> signed = y.Map(static v => v > 0.0 ? 1.0 : -1.0);
        Matrix<double> gram = Gram(x, x, policy.KernelBandwidth);
        Matrix<double> kkt = Matrix<double>.Build.Dense(n + 1, n + 1, (i, j) =>
            i == 0 && j == 0 ? 0.0
            : i == 0 ? signed[j - 1]
            : j == 0 ? signed[i - 1]
            : gram[i - 1, j - 1] + (i == j ? 1.0 / policy.Regularization : 0.0));
        Vector<double> rhs = Vector<double>.Build.Dense(n + 1, i => i == 0 ? 0.0 : 1.0);
        return DenseRoute.Solve(new FactorRoute.SquarePivoting(kkt), rhs, TolerancePolicy.Derive(kkt, rhs))
            .Map(solution => {
                double bias = solution[0];
                Vector<double> duals = Vector<double>.Build.Dense(n, i => solution[i + 1] * signed[i]);
                Vector<double> decision = gram.Multiply(duals).Add(bias);
                double accuracy = Enumerable.Range(0, n).Count(i => Math.Sign(decision[i]) == Math.Sign(signed[i])) / (double)n;
                return new FittedModel(c, new EstimatorModel.Margin(x, duals, bias, policy.KernelBandwidth), accuracy, 1.0 - accuracy, 1, true, clocks.Now);
            });
    }

    static Fin<FittedModel> FitNaiveBayes(Estimator.Classify c, Matrix<double> x, Vector<double> y, EstimatorPolicy policy, ClockPolicy clocks) {
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
                variances[k, f] = Math.Max(policy.Tolerance, moment.Variance);
            }
        }
        return Fin.Succ(new FittedModel(c, new EstimatorModel.Bayes(means, variances, priors), 1.0, 0.0, 1, true, clocks.Now));
    }

    // --- [TEMPORAL] -----------------------------------------------------------------------

    // The Iterative column splits the closed-form AR (lag-design thin-QR) from the three genuine iterative
    // recurrences, each with its OWN conditional kernel minimized by the shared LevenbergMarquardt owner — never
    // one ARMA(p,1) masquerade standing in for exponential-smoothing and state-space.
    static Fin<FittedModel> FitTemporal(Estimator.Temporal t, Matrix<double> x, EstimatorPolicy policy, ClockPolicy clocks) {
        Vector<double> series = x.Column(0);
        return !t.Model.Iterative ? FitAr(t, series, clocks)
            : t.Model == TimeSeriesModel.Arma ? FitArma(t, series, policy, clocks)
            : t.Model == TimeSeriesModel.ExponentialSmoothing ? FitHolt(t, series, clocks)
            : FitStateSpace(t, series, clocks);
    }

    // Pure AR(p): the lag design Y[t] = Σ φₖ·Y[t−k] solved through the dense-algebra thin-QR least squares —
    // the same closed-form route OLS rides, the AR coefficients its solution.
    static Fin<FittedModel> FitAr(Estimator.Temporal t, Vector<double> series, ClockPolicy clocks) {
        int p = Math.Max(1, t.Lags), n = series.Count;
        if (n <= p) { return Fin.Fail<FittedModel>(ComputeFault.Create($"<ar-short-series:{n}<={p}>")); }
        Matrix<double> design = Matrix<double>.Build.Dense(n - p, p, (i, k) => series[p + i - 1 - k]);
        Vector<double> response = Vector<double>.Build.Dense(n - p, i => series[p + i]);
        return DenseRoute.Solve(new FactorRoute.Orthonormal(design, QRMethod.Thin, Modified: false), response, TolerancePolicy.Derive(design, response))
            .Map(phi => {
                Vector<double> residual = response - design.Multiply(phi);
                double variance = residual.DotProduct(residual) / Math.Max(1, n - p);
                Vector<double> tail = Vector<double>.Build.Dense(p, k => series[n - 1 - k]);
                return new FittedModel(t, new EstimatorModel.Lag(phi, Vector<double>.Build.Dense(0), variance, tail, TimeSeriesModel.Ar), -variance, Math.Sqrt(variance), 1, true, clocks.Now);
            });
    }

    // ARMA: the MA term is nonlinear in the parameters, so the conditional sum-of-squares is a nonlinear least
    // squares minimized through the dense lane's damped Gauss-Newton LevenbergMarquardt owner over the
    // conditional residual recursion with a finite-difference Jacobian (the autodiff-tape-absent fall the
    // optimizer/UQ lanes take for a generic recurrence) — never a torch loss whose graph the in-place residual
    // write and the CPU-scalar read would sever. Exponential-smoothing and state-space ride the SAME LM owner
    // but over their OWN distinct recurrences (Holt errors, Kalman innovations), never this ARMA residual.
    static Fin<FittedModel> FitArma(Estimator.Temporal t, Vector<double> series, EstimatorPolicy policy, ClockPolicy clocks) {
        int p = Math.Max(1, t.Lags), q = p, n = series.Count;
        if (n <= p + q) { return Fin.Fail<FittedModel>(ComputeFault.Create($"<arma-short-series:{n}>")); }
        Func<Vector<double>, Vector<double>> residual = theta => ArmaResiduals(series, p, q, theta);
        return LevenbergMarquardt.Minimize(residual, theta => FiniteJacobian(residual, theta, 1e-6), Vector<double>.Build.Dense(p + q), LmPolicy.Canonical)
            .Map(lm => {
                Vector<double> resid = ArmaResiduals(series, p, q, lm.Parameters);
                // Tail packs the last p observations (AR feed) THEN the last q conditional residuals (MA feed),
                // most-recent first, so the forecast carries the fitted MA term instead of collapsing to a pure-AR
                // roll that drops the ψ coefficients it just fit; the residual slots zero-fill past the warmup.
                Vector<double> tail = Vector<double>.Build.Dense(p + q, k =>
                    k < p ? series[n - 1 - k]
                    : resid.Count - 1 - (k - p) >= 0 ? resid[resid.Count - 1 - (k - p)] : 0.0);
                double variance = lm.Residual * lm.Residual / Math.Max(1, n - p - q);
                return new FittedModel(t, new EstimatorModel.Lag(lm.Parameters.SubVector(0, p), lm.Parameters.SubVector(p, q), variance, tail, TimeSeriesModel.Arma), -variance, lm.Residual, lm.Iterations, lm.Converged, clocks.Now);
            });
    }

    // Holt linear-trend exponential smoothing: a genuinely distinct level+trend recurrence (NOT an ARMA roll).
    // The smoothing pair (α, β) is logistic-reparametrized so LevenbergMarquardt searches ℝ² unconstrained while
    // the live rates stay in (0,1); the carrier stores the realized (α, β) and the terminal (level, trend) the
    // forecast extrapolates as level + h·trend.
    static Fin<FittedModel> FitHolt(Estimator.Temporal t, Vector<double> series, ClockPolicy clocks) {
        int n = series.Count;
        if (n < 3) { return Fin.Fail<FittedModel>(ComputeFault.Create($"<holt-short-series:{n}>")); }
        Func<Vector<double>, Vector<double>> residual = theta => HoltErrors(series, theta);
        return LevenbergMarquardt.Minimize(residual, theta => FiniteJacobian(residual, theta, 1e-6), Vector<double>.Build.DenseOfArray([0.0, -2.0]), LmPolicy.Canonical)
            .Map(lm => {
                (double alpha, double beta, double level, double trend) = HoltState(series, lm.Parameters);
                double variance = lm.Residual * lm.Residual / Math.Max(1, n - 2);
                return new FittedModel(t, new EstimatorModel.Lag(Vector<double>.Build.DenseOfArray([alpha, beta]), Vector<double>.Build.Dense(0), variance, Vector<double>.Build.DenseOfArray([level, trend]), TimeSeriesModel.ExponentialSmoothing), -variance, lm.Residual, lm.Iterations, lm.Converged, clocks.Now);
            });
    }

    // Local-linear-trend structural state-space fit by the Kalman recurrence: the level/slope signal-to-noise
    // pair (qLevel, qSlope) is log-reparametrized for the unconstrained LM search over the RAW one-step
    // prediction-error innovations v (the minimum-prediction-error criterion — standardizing by √F would let LM
    // drive q→∞ degenerately), and the carrier stores the filtered terminal (level, slope) the forecast projects
    // forward — the optimal Kalman gain the AR lag-regression and the Holt smoother both lack.
    static Fin<FittedModel> FitStateSpace(Estimator.Temporal t, Vector<double> series, ClockPolicy clocks) {
        int n = series.Count;
        if (n < 4) { return Fin.Fail<FittedModel>(ComputeFault.Create($"<state-space-short-series:{n}>")); }
        Func<Vector<double>, Vector<double>> residual = theta => StateSpaceInnovations(series, theta);
        return LevenbergMarquardt.Minimize(residual, theta => FiniteJacobian(residual, theta, 1e-6), Vector<double>.Build.DenseOfArray([0.0, -3.0]), LmPolicy.Canonical)
            .Map(lm => {
                (double level, double slope, double variance) = StateSpaceState(series, lm.Parameters);
                return new FittedModel(t, new EstimatorModel.Lag(Vector<double>.Build.DenseOfArray([Math.Exp(lm.Parameters[0]), Math.Exp(lm.Parameters[1])]), Vector<double>.Build.Dense(0), variance, Vector<double>.Build.DenseOfArray([level, slope]), TimeSeriesModel.StateSpace), -variance, Math.Sqrt(variance), lm.Iterations, lm.Converged, clocks.Now);
            });
    }

    // --- [KERNELS] ------------------------------------------------------------------------

    static Matrix<double> Intercept(Matrix<double> x) =>
        Matrix<double>.Build.Dense(x.RowCount, x.ColumnCount + 1, (i, j) => j == 0 ? 1.0 : x[i, j - 1]);

    static Matrix<double> Tikhonov(int columns, double lambda) =>
        Matrix<double>.Build.Diagonal(columns, columns, j => j == 0 ? 0.0 : Math.Sqrt(lambda));

    static (double Intercept, Vector<double> Slopes) Split(Vector<double> theta) =>
        (theta[0], theta.SubVector(1, theta.Count - 1));

    static Matrix<double> Center(Matrix<double> x, Vector<double> mean) =>
        Matrix<double>.Build.Dense(x.RowCount, x.ColumnCount, (i, j) => x[i, j] - mean[j]);

    static int Retain(Vector<double> singular, double total, int cap, double fraction) {
        double cumulative = 0.0;
        for (int k = 0; k < cap; k++) {
            cumulative += singular[k] * singular[k];
            if (total > 0.0 && cumulative / total >= fraction) { return k + 1; }
        }
        return Math.Max(1, cap);
    }

    static Matrix<double> Seed(Matrix<double> x, int k) =>
        Matrix<double>.Build.DenseOfRowVectors([.. Enumerable.Range(0, k).Select(i => x.Row(i * Math.Max(1, x.RowCount / k) % x.RowCount))]);

    static int Nearest(Vector<double> point, Matrix<double> centroids) =>
        Enumerable.Range(0, centroids.RowCount).MinBy(c => (point - centroids.Row(c)).L2Norm());

    static Matrix<double> Recenter(Matrix<double> x, int[] labels, int k) {
        Matrix<double> sums = Matrix<double>.Build.Dense(k, x.ColumnCount);
        var counts = new int[k];
        for (int i = 0; i < x.RowCount; i++) {
            if (labels[i] < 0 || labels[i] >= k) { continue; }
            sums.SetRow(labels[i], sums.Row(labels[i]) + x.Row(i));
            counts[labels[i]]++;
        }
        return Matrix<double>.Build.Dense(k, x.ColumnCount, (c, j) => counts[c] > 0 ? sums[c, j] / counts[c] : 0.0);
    }

    static double Inertia(Matrix<double> x, int[] labels, Matrix<double> centroids) =>
        Enumerable.Range(0, x.RowCount).Where(i => labels[i] >= 0 && labels[i] < centroids.RowCount).Sum(i => (x.Row(i) - centroids.Row(labels[i])).PointwisePower(2.0).Sum());

    static int[] Region(Matrix<double> x, int point, double eps) =>
        [.. Enumerable.Range(0, x.RowCount).Where(i => i != point && (x.Row(point) - x.Row(i)).L2Norm() <= eps)];

    static double AverageLinkage(Matrix<double> x, List<int> a, List<int> b) =>
        a.Sum(i => b.Sum(j => (x.Row(i) - x.Row(j)).L2Norm())) / (a.Count * b.Count);

    static Matrix<double> Gram(Matrix<double> left, Matrix<double> right, double bandwidth) =>
        Matrix<double>.Build.Dense(left.RowCount, right.RowCount, (i, j) => Math.Exp(-(left.Row(i) - right.Row(j)).PointwisePower(2.0).Sum() / (2.0 * bandwidth * bandwidth)));

    static Fin<Cholesky<double>[]> Choleskies(Seq<Matrix<double>> covariances, double ridge) =>
        covariances.Fold(Fin.Succ(Array.Empty<Cholesky<double>>()), (acc, cov) =>
            acc.Bind(built => Admission.Definite(cov + Matrix<double>.Build.DiagonalIdentity(cov.RowCount) * ridge)
                .Map(chol => (Cholesky<double>[])[.. built, chol])));

    static double LogGaussian(Vector<double> x, Vector<double> mean, Cholesky<double> chol, int dim) {
        Vector<double> delta = x - mean;
        double quadratic = delta.DotProduct(chol.Solve(delta));
        return -0.5 * (dim * Math.Log(2.0 * Math.PI) + chol.DeterminantLn + quadratic);
    }

    static Matrix<double> WeightedCovariance(Matrix<double> x, Vector<double> gamma, Vector<double> mean, double mass, double ridge) {
        Matrix<double> accumulator = Matrix<double>.Build.Dense(x.ColumnCount, x.ColumnCount);
        for (int i = 0; i < x.RowCount; i++) {
            Vector<double> delta = x.Row(i) - mean;
            accumulator += gamma[i] * delta.OuterProduct(delta);
        }
        return accumulator / Math.Max(1e-9, mass) + Matrix<double>.Build.DiagonalIdentity(x.ColumnCount) * ridge;
    }

    static Fin<ImmutableArray<int>> Responsibilities(Matrix<double> x, EstimatorModel.Mixture mixture) =>
        Choleskies(mixture.Covariances, 1e-9).Map(chols =>
            [.. x.EnumerateRows().Select(row =>
                Enumerable.Range(0, mixture.Weights.Count)
                    .MaxBy(j => Math.Log(Math.Max(1e-300, mixture.Weights[j])) + LogGaussian(row, mixture.Means.Row(j), chols[j], x.ColumnCount)))]);

    static double Decision(Vector<double> point, EstimatorModel.Margin margin) =>
        Enumerable.Range(0, margin.Training.RowCount).Sum(i => margin.Duals[i] * Math.Exp(-(point - margin.Training.Row(i)).PointwisePower(2.0).Sum() / (2.0 * margin.Bandwidth * margin.Bandwidth))) + margin.Bias;

    static int Vote(Vector<double> point, EstimatorModel.Neighbors neighbors) =>
        Enumerable.Range(0, neighbors.Design.RowCount)
            .OrderBy(i => (point - neighbors.Design.Row(i)).L2Norm())
            .Take(neighbors.K)
            .GroupBy(i => neighbors.Labels[i])
            .MaxBy(g => g.Count())!.Key;

    static int Posterior(Vector<double> point, EstimatorModel.Bayes bayes) =>
        Enumerable.Range(0, bayes.Priors.Count)
            .MaxBy(k => Math.Log(Math.Max(1e-300, bayes.Priors[k])) +
                Enumerable.Range(0, point.Count).Sum(f => -0.5 * (Math.Log(2.0 * Math.PI * bayes.Variances[k, f]) + (point[f] - bayes.Means[k, f]) * (point[f] - bayes.Means[k, f]) / bayes.Variances[k, f])));

    static Matrix<double> KernelProject(Matrix<double> x, EstimatorModel.KernelBasis basis) {
        Matrix<double> gram = Gram(x, basis.Training, basis.Bandwidth);
        Vector<double> testRowMean = gram.RowSums() / basis.Training.RowCount;
        Matrix<double> centered = Matrix<double>.Build.Dense(x.RowCount, basis.Training.RowCount,
            (i, j) => gram[i, j] - testRowMean[i] - basis.RowMean[j] + basis.GrandMean);
        return centered.Multiply(basis.Alphas);
    }

    // NMF transform of new rows given the learned components H: one non-negative projection step per
    // component (the full non-negative least squares is the noted refinement); the encoding stays W ≥ 0.
    static Matrix<double> NonNegativeEncode(Matrix<double> x, Matrix<double> components) =>
        Matrix<double>.Build.Dense(x.RowCount, components.RowCount, (i, k) =>
            Math.Max(0.0, x.Row(i).DotProduct(components.Row(k)) / Math.Max(1e-12, components.Row(k).DotProduct(components.Row(k)))));

    // Model-aware forecast: the Lag.Model tag routes to the matching extrapolation — the AR(+MA) roll for the
    // lag-regression families, the level+trend line for Holt, the level+slope line for the local-trend SSM.
    static Vector<double> Forecast(EstimatorModel.Lag lag, int horizon) =>
        lag.Model == TimeSeriesModel.ExponentialSmoothing ? HoltForecast(lag, horizon)
        : lag.Model == TimeSeriesModel.StateSpace ? StateSpaceForecast(lag, horizon)
        : ArmaForecast(lag, horizon);

    // AR(+MA) roll: ŷ[T+h] = Σφₖ·ŷ[T+h−1−k] + Σψₖ·ê[T+h−1−k]; a future shock has zero expectation, so the MA
    // term decays to zero past q steps while the AR feedback continues from the rolled forecasts. Pure-AR (q=0)
    // skips the residual loop — one roll serving both lag-regression rows.
    static Vector<double> ArmaForecast(EstimatorModel.Lag lag, int horizon) {
        int p = lag.ArCoefficients.Count, q = lag.MaCoefficients.Count;
        var obs = new double[p];
        for (int k = 0; k < p; k++) { obs[k] = lag.Tail[k]; }
        var res = new double[q];
        for (int k = 0; k < q; k++) { res[k] = lag.Tail.Count > p + k ? lag.Tail[p + k] : 0.0; }
        var forecast = new double[Math.Max(1, horizon)];
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

    static Vector<double> HoltForecast(EstimatorModel.Lag lag, int horizon) =>
        Vector<double>.Build.Dense(Math.Max(1, horizon), h => lag.Tail[0] + (h + 1) * lag.Tail[1]);

    static Vector<double> StateSpaceForecast(EstimatorModel.Lag lag, int horizon) =>
        Vector<double>.Build.Dense(Math.Max(1, horizon), h => lag.Tail[0] + (h + 1) * lag.Tail[1]);

    // Holt one-step error e[t] = y[t] − (ℓ[t−1]+b[t−1]); ℓ[t] = ℓ[t−1]+b[t−1]+α·e[t], b[t] = b[t−1]+α·β·e[t].
    // (α, β) arrive logistic-mapped from the unconstrained LM iterate so the live rates stay in (0,1).
    static Vector<double> HoltErrors(Vector<double> series, Vector<double> theta) {
        double alpha = Logistic(theta[0]), beta = Logistic(theta[1]);
        int n = series.Count;
        double level = series[0], trend = series[1] - series[0];
        var errors = new double[n - 1];
        for (int i = 1; i < n; i++) {
            double fitted = level + trend, e = series[i] - fitted;
            errors[i - 1] = e;
            level = fitted + alpha * e;
            trend += alpha * beta * e;
        }
        return Vector<double>.Build.DenseOfArray(errors);
    }

    static (double Alpha, double Beta, double Level, double Trend) HoltState(Vector<double> series, Vector<double> theta) {
        double alpha = Logistic(theta[0]), beta = Logistic(theta[1]);
        int n = series.Count;
        double level = series[0], trend = series[1] - series[0];
        for (int i = 1; i < n; i++) {
            double fitted = level + trend, e = series[i] - fitted;
            level = fitted + alpha * e;
            trend += alpha * beta * e;
        }
        return (alpha, beta, level, trend);
    }

    static Vector<double> StateSpaceInnovations(Vector<double> series, Vector<double> theta) =>
        Vector<double>.Build.DenseOfArray(StateSpaceFilter(series, theta).Innovations);

    static (double Level, double Slope, double Variance) StateSpaceState(Vector<double> series, Vector<double> theta) {
        (double[] innovations, double level, double slope) = StateSpaceFilter(series, theta);
        double variance = innovations.Length > 0 ? innovations.Sum(static v => v * v) / innovations.Length : 0.0;
        return (level, slope, variance);
    }

    // The 2-state (level, slope) local-linear-trend Kalman filter: transition T=[[1,1],[0,1]], observation
    // H=[1,0] with unit measurement variance, diffuse covariance start, and process variances (qLevel, qSlope).
    // P⁻ = T·P·Tᵀ + Q expands to the three symmetric entries; the optimal gain K = P⁻Hᵀ/F updates the state and
    // the Joseph-equivalent (I−KH)P⁻ posterior. The raw innovation v feeds the LM prediction-error fit.
    static (double[] Innovations, double Level, double Slope) StateSpaceFilter(Vector<double> series, Vector<double> theta) {
        double qLevel = Math.Exp(theta[0]), qSlope = Math.Exp(theta[1]);
        int n = series.Count;
        double a0 = series[0], a1 = series[1] - series[0];
        double p00 = 1e3, p01 = 0.0, p11 = 1e3;
        var innov = new double[n - 1];
        for (int t = 1; t < n; t++) {
            double pred = a0 + a1;
            double m00 = p00 + 2.0 * p01 + p11 + qLevel, m01 = p01 + p11, m11 = p11 + qSlope;
            double f = m00 + 1.0, v = series[t] - pred;
            innov[t - 1] = v;
            double k0 = m00 / f, k1 = m01 / f;
            a0 = pred + k0 * v;
            a1 += k1 * v;
            p00 = m00 - k0 * m00;
            p01 = m01 - k0 * m01;
            p11 = m11 - k1 * m01;
        }
        return (innov, a0, a1);
    }

    static double Logistic(double z) => 1.0 / (1.0 + Math.Exp(-z));

    // Conditional residual recursion r[t] = y[t] − (Σφₖ·y[t−1−k] + Σψₖ·r[t−1−k]); residuals before the
    // max(p,q) warmup are zero, the standard conditional-sum-of-squares start. Pure double arithmetic — the
    // LevenbergMarquardt owner differentiates it by finite differences.
    static Vector<double> ArmaResiduals(Vector<double> series, int p, int q, Vector<double> theta) {
        int n = series.Count, warmup = Math.Max(p, q);
        var residuals = new double[n];
        for (int t = warmup; t < n; t++) {
            double predicted = 0.0;
            for (int k = 0; k < p; k++) { predicted += theta[k] * series[t - 1 - k]; }
            for (int k = 0; k < q; k++) { predicted += theta[p + k] * residuals[t - 1 - k]; }
            residuals[t] = series[t] - predicted;
        }
        return Vector<double>.Build.DenseOfArray(residuals[warmup..]);
    }

    static Matrix<double> FiniteJacobian(Func<Vector<double>, Vector<double>> residual, Vector<double> theta, double step) {
        Vector<double> baseline = residual(theta);
        return Matrix<double>.Build.DenseOfColumnVectors([.. Enumerable.Range(0, theta.Count).Select(j => {
            Vector<double> bumped = theta.Clone();
            bumped[j] += step;
            return (residual(bumped) - baseline) / step;
        })]);
    }
}
```

### [2.1]-[HYPOTHESIS_LAW]

- Owner: `StatisticalTest` `[SmartEnum<string>]` the hypothesis rows, each binding ONE `Evaluate` kernel that computes the test statistic, the (possibly fractional) degrees of freedom, and the p-value from its own tail — collapsing the prior parallel `Statistic`/`PValue`/`dof` helper trio into row data; `TestResult` the (statistic, p-value, decision, dof) carrier; `Hypothesis` the static `Test` surface threading the settled `ClockPolicy`.
- Cases: `StatisticalTest` rows t · welch-t · anova · chi-square · ks · mann-whitney (6).
- Entry: `public static Fin<TestResult> Test(StatisticalTest test, Seq<ReadOnlyMemory<double>> samples, double alpha, CorrelationId correlation, ClockPolicy clocks)` computes the row's statistic and reads the p-value from the matching `Distributions` CDF, stamping the instant from `clocks.Now` — never the ambient `SystemClock`, the foreclosed second clock.
- Auto: the pooled-variance `t` and the Welch `welch-t` rows read a two-sided tail from `StudentT.CDF` (Welch carrying the Welch–Satterthwaite fractional dof); `anova` reads the upper tail of `FisherSnedecor.CDF` over the (k−1, N−k) one-way F; `chi-square` reads the upper tail of `ChiSquared.CDF` over the Σ(O−E)²/E goodness-of-fit statistic; `ks` reads the two-sample sup-distance against the Kolmogorov asymptotic series `Q(λ)=2·Σ(−1)^{j−1}·e^{−2j²λ²}` (no MathNet CDF exists, so the series is the row's kernel); `mann-whitney` reads a tie-corrected, continuity-corrected normal approximation through `Normal.CDF` over the rank-sum U.
- Boundary: each test row owns its complete kernel so a hand-derived error function beside the `Distributions` CDF is the deleted form for the t/anova/chi-square/mann-whitney rows, and the Kolmogorov series + the rank-sum computation are this page's statement-exemption numeric kernels exactly as the signal lane's direct-form recurrence is exempt; the tail direction is row policy (two-sided for t/welch/mann-whitney, upper for anova/chi-square, the Kolmogorov complement for ks) so a fixed one-sided tail across the family is the deleted form; the tests validate the `Solver/uncertainty#UNCERTAINTY_LANE` response samples (a KS distribution-fit check or an ANOVA across design regions) so the hypothesis surface consumes the UQ lane's draws without re-sampling.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StatisticalTest {
    public static readonly StatisticalTest Student = new("t", static s => StudentTwoSample(s, pooled: true));
    public static readonly StatisticalTest WelchT = new("welch-t", static s => StudentTwoSample(s, pooled: false));
    public static readonly StatisticalTest Anova = new("anova", OneWayAnova);
    public static readonly StatisticalTest ChiSquare = new("chi-square", ChiSquareGoodness);
    public static readonly StatisticalTest Ks = new("ks", KolmogorovSmirnov);
    public static readonly StatisticalTest MannWhitney = new("mann-whitney", RankSum);

    private readonly Func<Seq<ReadOnlyMemory<double>>, (double Statistic, double PValue, double Dof)> evaluate;

    public (double Statistic, double PValue, double Dof) Evaluate(Seq<ReadOnlyMemory<double>> samples) => evaluate(samples);

    // Pooled (Student) or separate-variance (Welch) two-sample t; Welch carries the fractional
    // Welch–Satterthwaite dof; both read the two-sided StudentT tail.
    static (double, double, double) StudentTwoSample(Seq<ReadOnlyMemory<double>> samples, bool pooled) {
        double[] a = samples[0].ToArray(), b = samples[1].ToArray();
        (double ma, double va) = a.MeanVariance();
        (double mb, double vb) = b.MeanVariance();
        int na = a.Length, nb = b.Length;
        double dof = pooled
            ? na + nb - 2
            : Math.Pow(va / na + vb / nb, 2) / (Math.Pow(va / na, 2) / (na - 1) + Math.Pow(vb / nb, 2) / (nb - 1));
        double standardError = pooled
            ? Math.Sqrt(((na - 1) * va + (nb - 1) * vb) / (na + nb - 2)) * Math.Sqrt(1.0 / na + 1.0 / nb)
            : Math.Sqrt(va / na + vb / nb);
        double t = (ma - mb) / standardError;
        return (t, 2.0 * (1.0 - StudentT.CDF(0.0, 1.0, dof, Math.Abs(t))), dof);
    }

    static (double, double, double) OneWayAnova(Seq<ReadOnlyMemory<double>> samples) {
        double[][] groups = [.. samples.Map(static g => g.ToArray())];
        int total = groups.Sum(static g => g.Length), k = groups.Length;
        double grand = groups.SelectMany(static g => g).Mean();
        double between = groups.Sum(g => g.Length * Math.Pow(g.Mean() - grand, 2)) / (k - 1);
        double within = groups.Sum(g => g.Sum(v => Math.Pow(v - g.Mean(), 2))) / (total - k);
        double f = between / within;
        return (f, 1.0 - FisherSnedecor.CDF(k - 1, total - k, f), k - 1);
    }

    static (double, double, double) ChiSquareGoodness(Seq<ReadOnlyMemory<double>> samples) {
        double[] observed = samples[0].ToArray();
        double[] expected = samples.Count > 1 ? samples[1].ToArray() : [.. observed.Select(_ => observed.Average())];
        double chi = Enumerable.Range(0, observed.Length).Sum(i => Math.Pow(observed[i] - expected[i], 2) / Math.Max(1e-12, expected[i]));
        double dof = observed.Length - 1;
        return (chi, 1.0 - ChiSquared.CDF(dof, chi), dof);
    }

    // Two-sample Kolmogorov–Smirnov: the sup gap of the empirical CDFs against the Kolmogorov asymptotic
    // series — MathNet ships no Kolmogorov distribution, so the alternating exponential series is the kernel.
    static (double, double, double) KolmogorovSmirnov(Seq<ReadOnlyMemory<double>> samples) {
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
    static (double, double, double) RankSum(Seq<ReadOnlyMemory<double>> samples) {
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

    static double[] Ranks(double[] values) {
        int[] order = [.. Enumerable.Range(0, values.Length).OrderBy(i => values[i])];
        var ranks = new double[values.Length];
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
    public static Fin<TestResult> Test(StatisticalTest test, Seq<ReadOnlyMemory<double>> samples, double alpha, CorrelationId correlation, ClockPolicy clocks) =>
        samples.Count < 1
            ? Fin.Fail<TestResult>(ComputeFault.Create($"<hypothesis-empty-samples:{test.Key}>"))
            : test.Evaluate(samples) is var (statistic, pValue, dof) && double.IsFinite(statistic) && double.IsFinite(pValue)
                ? Fin.Succ(new TestResult(test, statistic, pValue, pValue < alpha, dof, clocks.Now))
                : Fin.Fail<TestResult>(new ComputeFault.ModelRejected($"<hypothesis-nonfinite:{test.Key}:stat={statistic}>"));
}
```

## [03]-[RESEARCH]

- [GLM_DEVIANCE]: the GLM fit folds the canonical-link family `Deviance` carried on the `EstimatorKind` GLM row to a `torch` loss minimized under `LBFGS` through `IterativeEngine.Minimize`, reverse-mode-differentiated by `backward()` (`softplus(η)−y·η` Bernoulli / `exp(η)−y·η` Poisson / `η+y·exp(−η)` log-link Gamma), so the second-order quasi-Newton step replaces the explicit IRLS normal-equation re-solve and the `Link.Mean` inverse link reconstructs the predicted mean at egress; the `Build` residual IS the Pearson dispersion √(Σ (y−μ)²/V(μ)/(n−p)) reading the row's `Link.Variance` (V=μ(1−μ) Bernoulli, V=μ Poisson, V=1 the Gaussian Identity floor), so the family variance is load-bearing rather than decorative, and the receipt's fit-quality stamp carries it; the open leaf is the EXACT deviance over the `MathNet.Numerics.Distributions` `Density`/`CumulativeDistribution` normalizing constant where the family needs it beyond the quasi-likelihood dispersion, and the canonical inverse-link Gamma path (`LinkFunction.Inverse`, reachable through the `Estimator.Regression.Link` override) grounds against the `−log(−η)−y·η` form where positivity holds.
- [LS_SVM_VS_DUAL]: the realized SVM is the least-squares formulation — the regularized-Gram KKT system solved once through the `Tensor/blas#DENSE_ALGEBRA` `DenseRoute.Solve(FactorRoute.SquarePivoting)`, the dual coefficients its solution and the RBF bandwidth a policy column; the open leaf is the box-constrained C-SVM dual (the hinge/quadratic-margin objective under `0 ≤ α ≤ C`, `Σαᵢyᵢ = 0`) whose sparse support set the LS-SVM dense solution trades for closed-form conditioning, grounded against an SMO/projected-gradient kernel where hard-margin sparsity is required, the Gram-matrix conditioning riding the `Admission.Definite` Cholesky gate.
- [GRADUATION_MODELS]: the offline deep-training models (gradient-boosted, deep-net) are the Python `compute/graduation` companion's, decoded by content key over the `ONE_GRADUATION_EVIDENCE` seam — C# owns the classical fit plus the ONNX inference, and the model-signature (input/output tensor names, feature interleave) grounds against the companion-published signature at the graduation-decode gate, the fitted classical estimators staying the C# producer; the spectral features the time-series and classification rows consume arrive from the `Stats/signal#SIGNAL_LANE` `Transform` axis, the producer of the frequency-domain evidence this lane reads.