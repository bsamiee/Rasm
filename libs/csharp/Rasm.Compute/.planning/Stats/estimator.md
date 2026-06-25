# [COMPUTE_ESTIMATOR]

Rasm.Compute statistical-learning lane: one `Estimator` `[Union]` carrying a uniform `Fit(X,y) → FittedModel` / `Predict(FittedModel, X) → ŷ` contract over the supervised-regression, GLM, dimensionality-reduction, clustering, classification, hypothesis-testing, and time-series families, and one `EstimatorModel` `[Union]` fit-result carrier keyed by the estimator case — every regression/learning model is a row on one Fit/Predict contract, never a per-model class. The lane factors through the `Tensor/blas#DENSE_ALGEBRA` `Factorization` surface where a closed-form factorization exists (OLS/ridge via QR, PCA via SVD, GP via Cholesky) and folds the iterative rows (lasso L1, GLM-IRLS, kernel-SVM, NMF, GMM-EM, k-means, ARMA-MLE) to a single `Tensor` loss minimized under `TorchSharp`'s `torch.autograd` + a `torch.optim` driver (`LBFGS`/`Adam`/`SGD`) inside one `DisposeScope` through the `IterativeEngine`, never a hand-rolled coordinate-descent/IRLS/EM loop, and reduces descriptive moments through `MathNet.Numerics.Statistics` and reads distribution CDFs through `MathNet.Numerics.Distributions` rather than a hand-rolled accumulator. The `StatisticalTest` axis returns a typed `TestResult` (statistic, p-value, decision) over the matching `Distributions` CDF; the `TimeSeriesModel` rows extend the `Estimator` contract. The page owns the `EstimatorKind`/`LinkFunction`/`StatisticalTest`/`TimeSeriesModel` vocabulary, the `Estimator`/`EstimatorModel`/`FittedModel`/`TestResult` carriers, and the `EstimatorFold` Fit/Predict surface; the dense factorizations ride `Tensor/blas#DENSE_ALGEBRA`, the descriptive/distribution surfaces ride the admitted `MathNet.Numerics` assembly, and the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, `ClockPolicy`, and the `SolverKeyPolicy` ordinal accessor arrive settled. Fitted surrogates feed the `Solver/optimizer#OPTIMIZER_LANE` `Surrogate.Predict` oracle so the optimizer and the stats lane share one regression fold; offline deep-training studies cross the `ONE_GRADUATION_EVIDENCE` seam from Python by content key, with C# owning classical fit plus inference and Python owning deep training.

## [01]-[INDEX]

- [01]-[ESTIMATOR_LANE]: Fit/Predict contract; regression + GLM + PCA/clustering/classification + hypothesis + time-series rows over dense factorization and MathNet.

## [02]-[ESTIMATOR_LANE]

- Owner: `EstimatorKind` `[SmartEnum<string>]` the learning-algorithm rows carrying a `Factorized` column (whether a closed-form `Factorization` fit exists), a `Supervised` column, an `Iterative` column (whether the fit folds to a `torch` loss), and the `Driver` `OptimDriver` row naming the `torch.optim` optimizer the iterative fit minimizes under; `OptimDriver` `[SmartEnum<string>]` the optimizer rows (lbfgs/adam/sgd) each carrying the `torch.optim` factory delegate and a `LineSearch` flag (the `LBFGS` closure form); `LinkFunction` `[SmartEnum<string>]` the GLM link rows (identity/logit/log/inverse) carrying the inverse-link and variance-function delegates; `StatisticalTest` `[SmartEnum<string>]` the hypothesis-test rows each binding its statistic kernel and the matching `Distributions` CDF; `TimeSeriesModel` `[SmartEnum<string>]` the temporal rows (ar/arma/exponential-smoothing/state-space) extending the Fit/Predict contract; `Estimator` `[Union]` the typed problem cases; `EstimatorModel` `[Union]` the fit-result carrier keyed by case (coefficients, basis, centroids, mixture, support vectors) mirroring `Factorization`, never one result type per algorithm; `IterativeEngine` the one `torch.autograd`+`torch.optim` loss-minimization owner under a `DisposeScope`; `FittedModel` the coefficients-plus-quality carrier; `TestResult` the (statistic, p-value, decision) carrier; `EstimatorFold` the static Fit/Predict surface dispatching by `EstimatorKind`.
- Cases: `EstimatorKind` rows ols · ridge · lasso · glm-logistic · glm-poisson · glm-gamma · pca · kernel-pca · nmf · kmeans · gmm · dbscan · hierarchical · knn · svm · naive-bayes (16, each carrying `factorized`/`supervised`/`iterative`/`driver`); `OptimDriver` rows lbfgs · adam · sgd (3); `LinkFunction` rows identity · logit · log · inverse (4); `StatisticalTest` rows t · welch-t · anova · chi-square · ks · mann-whitney (6); `TimeSeriesModel` rows ar · arma · exponential-smoothing · state-space (4); `Estimator` cases `Regression(EstimatorKind Kind, Option<LinkFunction> Link)` · `Reduction(EstimatorKind Kind, int Rank)` · `Cluster(EstimatorKind Kind, int Groups)` · `Classify(EstimatorKind Kind)` · `Temporal(TimeSeriesModel Model, int Lags)`.
- Entry: `public static Fin<FittedModel> Fit(Estimator estimator, Matrix<double> features, Option<Vector<double>> targets, EstimatorPolicy policy, CorrelationId correlation, ClockPolicy clocks)` is the one fit fold — `Fin<T>` aborts on a rank-deficient design or a non-converged iteration; `public static Fin<Vector<double>> Predict(FittedModel model, Matrix<double> features)` is the one predict fold projecting/assigning/classifying through the case carrier; `public static Fin<TestResult> Test(StatisticalTest test, Seq<ReadOnlyMemory<double>> samples, double alpha)` computes the statistic and reads the p-value from the matching CDF.
- Auto: `Fit` dispatches by the `EstimatorKind` row — ols/ridge factor through the `Tensor/blas#DENSE_ALGEBRA` `FactorRoute.Orthonormal` QR least-squares (ridge augmenting the design with the `√λ·I` Tikhonov block), the iterative rows (lasso L1, GLM-IRLS, kernel-SVM, NMF, GMM-EM, k-means) fold to one `Tensor` loss minimized by `IterativeEngine.Minimize` under the row's `Driver` `torch.optim` optimizer — lasso the soft-thresholded least-squares objective under `Adam`, the GLM rows the canonical-link family deviance (`(softplus(η) − y·η)`/`(exp(η) − y·η)` from the `IterativeEngine.GlmDeviance` table) under `LBFGS`, NMF/clustering under `SGD` — each loss differentiated by `backward()` inside a `torch.NewDisposeScope()`, pca/kernel-pca factor through the `FactorRoute.RankRevealing` SVD left-singular basis (the same `Orthogonalization.PodSvd` machinery the optimizer ROM uses) and truncate by the `EstimatorPolicy.EnergyFraction`, dbscan runs density reachability, hierarchical runs agglomerative linkage, knn stores the labelled design, and naive-bayes fits per-class `Distributions` moments; `Predict` projects (reduction), assigns nearest (cluster), classifies (supervised), or forecasts (temporal) through the `EstimatorModel` carrier; `Test` computes the test statistic and reads `CumulativeDistribution` from the matching `Distributions` (`StudentT`/`FisherSnedecor`/`ChiSquared`/`Normal`) for the p-value; a `TimeSeriesModel` row fits its lag design through the QR least-squares route (pure AR) or an iterative likelihood maximization (ARMA, nonlinear in the MA term).
- Receipt: the `TensorRun`/`Factorization` `ComputeReceipt` case carries the estimator key, the fit-quality metric (R²/log-likelihood/inertia/silhouette/AIC by family), the iteration count for the iterative rows, the retained rank for the reduction rows, and elapsed; a fit through the dense factorization rides the same `Factorization` provider/determinism evidence the blas lane stamps so a regression fit is reproducible.
- Packages: MathNet.Numerics, TorchSharp, libtorch-cpu, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new estimator is one `EstimatorKind` row (its `factorized`/`supervised`/`iterative`/`driver` columns) plus one `Fit` branch (closed-form rows reuse the dense factorization, iterative rows supply a `torch` loss closure to `IterativeEngine.Minimize`) and one `EstimatorModel` case if its fit-result shape is new; a new optimizer is one `OptimDriver` row binding its `torch.optim` factory; a new GLM family deviance is one `IterativeEngine.GlmDeviance` row; a new link is one `LinkFunction` row; a new hypothesis test is one `StatisticalTest` row binding its statistic and CDF; a new time-series model is one `TimeSeriesModel` row; zero new surface — an `OlsEstimator`/`RidgeEstimator`/`PcaEstimator`/`KMeansEstimator` class family is the rejected form collapsed onto the one `EstimatorFold`, a `RegressionModel`/`ClusterModel`/`ReductionModel` result trio is collapsed onto the one `EstimatorModel` union, a hand-rolled `CoordinateDescent`/`Irls`/`Em` loop beside `IterativeEngine` is the deleted form, and a `TTest`/`AnovaTest`/`KsTest` class family is collapsed onto the one `StatisticalTest` axis.
- Boundary: the lane is contract-uniform — every estimator answers the one `Fit`/`Predict` contract and the solution mechanism is a row property, not a parallel owner, so a forced SVD route for k-means is wrong (k-means is density/iterative, the shared contract is Fit/Predict, not a shared factorization); every closed-form estimator factors through the `Tensor/blas#DENSE_ALGEBRA` `Factorization` route — OLS/ridge through QR least-squares, PCA through the SVD left-singular basis, GP through Cholesky — never a hand-rolled normal-equations solve or a hand-rolled FFT/SVD, so the regression fit shares the dense-algebra provider and determinism tag; lasso has no closed form so the L1 row folds to the `IterativeEngine` `torch` loss under `Adam` while the L2/OLS rows ride the factorization — one Fit fold, two solution mechanisms by row, never a separate Lasso owner; the GLM rows fold the canonical-link family deviance to a `torch` loss minimized under `LBFGS` (the `IterativeEngine.GlmDeviance` table holding the `softplus(η)−y·η` logistic / `exp(η)−y·η` Poisson / Gamma terms, reverse-mode-differentiated by `backward()`) rather than a hand-rolled IRLS loop, and the `naive-bayes` row fits per-class `Distributions` moments through `Statistics` rather than a hand-rolled Gaussian; the iterative density/mixture rows (gmm-EM, k-means, nmf) fold to a `torch` loss under their `Driver` while pca stays on the SVD surface and dbscan/hierarchical bind their density/linkage kernels — the shared contract is Fit/Predict, not a shared solution mechanism; every `IterativeEngine` fit runs inside one `torch.NewDisposeScope()` so no native ATen tensor leaks to the GC, `detect_anomaly(check_nan)` traps a NaN/inf fit to a typed `ComputeFault` instead of a silently-diverged coefficient, and only the egressed `Vector<double>` coefficient projection crosses the lane boundary — a `Tensor` escaping onto the wire or a reliance on finalization for native reclamation is the deleted form, the `DisposeScopeManager.Statistics` memory evidence riding the fit receipt; the descriptive moments fold `Statistics.Mean`/`Variance`/`Covariance`/`Correlation.Pearson` and the quantiles read `Statistics.Quantile`, never a hand-rolled Welford; the hypothesis tests read the p-value from the `Distributions` CDF (`StudentT.CDF`/`FisherSnedecor.CDF`/`ChiSquared.CDF`/`Normal.CDF`) so a hand-derived error function is the deleted form, and the tests validate the `Solver/uncertainty#UNCERTAINTY_LANE` UQ response distributions; ARMA's moving-average term is nonlinear in the parameters so the `TimeSeriesModel` fold routes the pure-AR row to QR least-squares but the ARMA row to an iterative likelihood maximization — distinct fit mechanisms on the one Estimator contract; the fitted regression/GP/RBF surrogates feed the `Solver/optimizer#OPTIMIZER_LANE` `Surrogate` rows so the optimizer and the stats lane share one regression fold, the time-series changepoint feeds the `Solver/clash#CLASH_AND_TWIN` Twin anomaly lane, the spectral features cross to the `Stats/signal#SIGNAL_LANE` `Transform` axis, and offline deep-training models cross the `ONE_GRADUATION_EVIDENCE` seam from Python by content key (C# owns inference plus classical fit, Python owns deep training — an in-proc deep-training loop is the rejected form because the training role belongs to the Python branch).

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class LinkFunction {
    public static readonly LinkFunction Identity = new("identity", static eta => eta, static mu => mu * (1.0 - mu));
    public static readonly LinkFunction Logit = new("logit", static eta => 1.0 / (1.0 + Math.Exp(-eta)), static mu => Math.Max(1e-9, mu * (1.0 - mu)));
    public static readonly LinkFunction Log = new("log", static eta => Math.Exp(eta), static mu => Math.Max(1e-9, mu));
    public static readonly LinkFunction Inverse = new("inverse", static eta => 1.0 / eta, static mu => mu * mu);

    private readonly Func<double, double> inverseLink;
    private readonly Func<double, double> variance;

    public double Mean(double eta) => inverseLink(eta);
    public double Variance(double mu) => variance(mu);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class OptimDriver {
    public static readonly OptimDriver LBfgs = new("lbfgs", lineSearch: true, static (p, lr) => torch.optim.LBFGS(p, lr, max_iter: 20));
    public static readonly OptimDriver Adam = new("adam", lineSearch: false, static (p, lr) => torch.optim.Adam(p, lr));
    public static readonly OptimDriver Sgd = new("sgd", lineSearch: false, static (p, lr) => torch.optim.SGD(p, lr, momentum: 0.9));

    private readonly Func<IEnumerable<Parameter>, double, torch.optim.Optimizer> bind;

    public bool LineSearch { get; }

    public torch.optim.Optimizer Bind(IEnumerable<Parameter> parameters, double lr) => bind(parameters, lr);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class EstimatorKind {
    public static readonly EstimatorKind Ols = new("ols", factorized: true, supervised: true, iterative: false, driver: OptimDriver.LBfgs);
    public static readonly EstimatorKind Ridge = new("ridge", factorized: true, supervised: true, iterative: false, driver: OptimDriver.LBfgs);
    public static readonly EstimatorKind Lasso = new("lasso", factorized: false, supervised: true, iterative: true, driver: OptimDriver.Adam);
    public static readonly EstimatorKind GlmLogistic = new("glm-logistic", factorized: false, supervised: true, iterative: true, driver: OptimDriver.LBfgs);
    public static readonly EstimatorKind GlmPoisson = new("glm-poisson", factorized: false, supervised: true, iterative: true, driver: OptimDriver.LBfgs);
    public static readonly EstimatorKind GlmGamma = new("glm-gamma", factorized: false, supervised: true, iterative: true, driver: OptimDriver.LBfgs);
    public static readonly EstimatorKind Pca = new("pca", factorized: true, supervised: false, iterative: false, driver: OptimDriver.LBfgs);
    public static readonly EstimatorKind KernelPca = new("kernel-pca", factorized: true, supervised: false, iterative: false, driver: OptimDriver.LBfgs);
    public static readonly EstimatorKind Nmf = new("nmf", factorized: false, supervised: false, iterative: true, driver: OptimDriver.Sgd);
    public static readonly EstimatorKind KMeans = new("kmeans", factorized: false, supervised: false, iterative: true, driver: OptimDriver.Sgd);
    public static readonly EstimatorKind Gmm = new("gmm", factorized: false, supervised: false, iterative: true, driver: OptimDriver.LBfgs);
    public static readonly EstimatorKind Dbscan = new("dbscan", factorized: false, supervised: false, iterative: false, driver: OptimDriver.Sgd);
    public static readonly EstimatorKind Hierarchical = new("hierarchical", factorized: false, supervised: false, iterative: false, driver: OptimDriver.Sgd);
    public static readonly EstimatorKind Knn = new("knn", factorized: false, supervised: true, iterative: false, driver: OptimDriver.Sgd);
    public static readonly EstimatorKind Svm = new("svm", factorized: false, supervised: true, iterative: true, driver: OptimDriver.LBfgs);
    public static readonly EstimatorKind NaiveBayes = new("naive-bayes", factorized: false, supervised: true, iterative: false, driver: OptimDriver.LBfgs);

    public bool Factorized { get; }
    public bool Supervised { get; }
    public bool Iterative { get; }
    public OptimDriver Driver { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class StatisticalTest {
    public static readonly StatisticalTest Student = new("t", dof: static n => n[0] + n[1] - 2);
    public static readonly StatisticalTest WelchT = new("welch-t", dof: static n => n[0] + n[1] - 2);
    public static readonly StatisticalTest Anova = new("anova", dof: static n => n.Length - 1);
    public static readonly StatisticalTest ChiSquare = new("chi-square", dof: static n => n[0] - 1);
    public static readonly StatisticalTest Ks = new("ks", dof: static _ => 0);
    public static readonly StatisticalTest MannWhitney = new("mann-whitney", dof: static _ => 0);

    private readonly Func<int[], int> dof;

    public int DegreesOfFreedom(params int[] counts) => dof(counts);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
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

    public EstimatorKind Algorithm =>
        Switch(regression: static r => r.Kind, reduction: static d => d.Kind, cluster: static c => c.Kind, classify: static c => c.Kind, temporal: static _ => EstimatorKind.Ols);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimatorModel {
    private EstimatorModel() { }

    public sealed record Linear(Vector<double> Coefficients, double Intercept) : EstimatorModel;
    public sealed record Basis(Matrix<double> Components, Vector<double> Singular, double EnergyFraction) : EstimatorModel;
    public sealed record Partition(Matrix<double> Centroids, ImmutableArray<int> Labels) : EstimatorModel;
    public sealed record Mixture(Matrix<double> Means, Seq<Matrix<double>> Covariances, Vector<double> Weights) : EstimatorModel;
    public sealed record Margin(Matrix<double> SupportVectors, Vector<double> Duals, double Bias) : EstimatorModel;
    public sealed record Lag(Vector<double> ArCoefficients, Vector<double> MaCoefficients, double Variance) : EstimatorModel;
}

public sealed record EstimatorPolicy(double Regularization, double EnergyFraction, int MaxIterations, double Tolerance, double KernelBandwidth) {
    public static readonly EstimatorPolicy CanonicalRegression = new(Regularization: 1e-3, EnergyFraction: 0.99, MaxIterations: 1000, Tolerance: 1e-8, KernelBandwidth: 1.0);
    public static readonly EstimatorPolicy CanonicalReduction = CanonicalRegression with { EnergyFraction: 0.95 };
    public static readonly EstimatorPolicy CanonicalCluster = CanonicalRegression with { MaxIterations: 300 };
}

public sealed record FittedModel(Estimator Estimator, EstimatorModel Carrier, double Quality, int Iterations, Instant At) {
    public Fin<Vector<double>> Predict(Matrix<double> features) =>
        EstimatorFold.Predict(this, features);
}

public sealed record TestResult(StatisticalTest Test, double Statistic, double PValue, bool RejectNull, int DegreesOfFreedom, Instant At);

// The iterative EstimatorKind rows fold to a Tensor loss minimized under torch.autograd + the row's
// OptimDriver (torch.optim) inside one DisposeScope: the design matrix and response ingress through
// torch.from_array, the loss closure is differentiated by backward(), the LBFGS line-search form
// re-evaluates the closure per probe, and the surviving coefficient Tensor egresses to a Vector<double>
// for the EstimatorModel carrier. Every intermediate is reclaimed at scope exit and stamped by
// DisposeScopeManager.Statistics; detect_anomaly traps a NaN/inf fit to a typed fault. set_default_dtype
// floors the estimator math at Float64. No Tensor escapes the lane — only the fitted double[] crosses.
public static class IterativeEngine {
    static readonly FrozenDictionary<string, Func<Tensor, Tensor, Tensor>> GlmDeviance =
        new Dictionary<string, Func<Tensor, Tensor, Tensor>>(StringComparer.Ordinal) {
            [EstimatorKind.GlmLogistic.Key] = static (eta, y) => (eta.exp().log1p() - y * eta).mean(),
            [EstimatorKind.GlmPoisson.Key]  = static (eta, y) => (eta.exp() - y * eta).mean(),
            [EstimatorKind.GlmGamma.Key]    = static (eta, y) => (eta + y * eta.neg().exp()).mean(),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static Fin<(Vector<double> Theta, double Loss, int Iterations, ThreadDisposeScopeStatistics Memory)> Minimize(
        Func<Tensor, Tensor, Tensor, Tensor> loss, Matrix<double> design, Vector<double> response, OptimDriver driver, EstimatorPolicy policy) {
        using var scope = torch.NewDisposeScope();
        using var anomaly = torch.autograd.detect_anomaly(check_nan: true);
        Tensor x = torch.from_array(design.ToColumnMajorArray(), ScalarType.Float64).reshape(design.ColumnCount, design.RowCount).t();
        Tensor y = torch.from_array(response.AsArray() ?? response.ToArray(), ScalarType.Float64).reshape(response.Count);
        var theta = new Parameter(torch.zeros(design.ColumnCount, ScalarType.Float64), requires_grad: true);
        var opt = driver.Bind([theta], policy.Tolerance is var _ ? Math.Max(1e-3, policy.Regularization) : 0.1);
        Tensor Step() { opt.zero_grad(); Tensor l = loss(theta, x, y); l.backward(); return l; }
        double last = double.MaxValue;
        int iter = 0;
        for (; iter < policy.MaxIterations; iter++) {
            Tensor value = driver.LineSearch ? ((Modules.LBFGS)opt).step(Step) : Plain(opt, Step);
            double current = value.ReadCpuDouble(0);
            bool stalled = Math.Abs(last - current) < policy.Tolerance;
            last = current;
            if (stalled) { iter++; break; }
        }
        Vector<double> fitted = Vector<double>.Build.DenseOfArray(theta.detach().reshape(theta.NumberOfElements).data<double>().ToArray());
        return double.IsFinite(last)
            ? Fin.Succ((fitted, last, iter, DisposeScopeManager.Statistics))
            : Fin.Fail<(Vector<double>, double, int, ThreadDisposeScopeStatistics)>(new ComputeFault.ModelRejected("<estimator-nonfinite-loss>"));
    }

    public static Func<Tensor, Tensor, Tensor, Tensor> Lasso(double lambda) =>
        (theta, x, y) => 0.5 * x.matmul(theta).sub(y).pow(2).mean() + lambda * theta.abs().sum();

    public static Func<Tensor, Tensor, Tensor, Tensor> Glm(EstimatorKind kind) =>
        (theta, x, y) => GlmDeviance[kind.Key](x.matmul(theta), y);

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

    // OLS/ridge factor through the dense QR least-squares route (ridge augments the design with the √λ·I
    // Tikhonov block); the iterative lasso (L1) and GLM (canonical-link NLL) rows fold to one Tensor loss
    // minimized under the row's OptimDriver via IterativeEngine — lasso the soft-thresholded least-squares
    // objective under Adam, the GLM rows the family deviance under LBFGS — never a hand-rolled coordinate
    // descent or IRLS loop beside torch.autograd.
    static Fin<FittedModel> FitRegression(Estimator.Regression r, Matrix<double> x, Option<Vector<double>> y, EstimatorPolicy policy, ClockPolicy clocks) =>
        y.ToFin(ComputeFault.Create("<estimator-regression-needs-targets>")).Bind(targets =>
            r.Kind == EstimatorKind.Ols || r.Kind == EstimatorKind.Ridge
                ? DenseRoute.Solve(new FactorRoute.Orthonormal(Augment(x, r.Kind == EstimatorKind.Ridge ? policy.Regularization : 0.0), QRMethod.Thin, Modified: false), Pad(targets, x.ColumnCount), TolerancePolicy.Derive(x, targets))
                    .Map(coefficients => new FittedModel(r, new EstimatorModel.Linear(coefficients, 0.0), Rsquared(x, targets, coefficients), 1, clocks.Now))
                : IterativeEngine.Minimize(r.Kind == EstimatorKind.Lasso ? IterativeEngine.Lasso(policy.Regularization) : IterativeEngine.Glm(r.Kind), x, targets, r.Kind.Driver, policy)
                    .Map(fit => new FittedModel(r, new EstimatorModel.Linear(fit.Theta, 0.0), -fit.Loss, fit.Iterations, clocks.Now)));

    public static Fin<Vector<double>> Predict(FittedModel model, Matrix<double> features) =>
        model.Carrier switch {
            EstimatorModel.Linear linear => Fin.Succ(features.Multiply(linear.Coefficients) + linear.Intercept),
            EstimatorModel.Basis basis => Fin.Succ((features * basis.Components).ColumnSums()),
            EstimatorModel.Partition partition => Fin.Succ(Assign(features, partition.Centroids)),
            EstimatorModel.Mixture mixture => Fin.Succ(Responsibilities(features, mixture)),
            EstimatorModel.Margin margin => Fin.Succ(Decision(features, margin)),
            EstimatorModel.Lag lag => Fin.Succ(Forecast(features, lag)),
            _ => Fin.Fail<Vector<double>>(ComputeFault.Create("<estimator-predict-miss>")),
        };

    public static Fin<TestResult> Test(StatisticalTest test, Seq<ReadOnlyMemory<double>> samples, double alpha) {
        double statistic = Statistic(test, samples);
        int dof = test.DegreesOfFreedom([.. samples.Map(static s => s.Length)]);
        double p = PValue(test, statistic, dof);
        return Fin.Succ(new TestResult(test, statistic, p, p < alpha, dof, NodaTime.SystemClock.Instance.GetCurrentInstant()));
    }
}
```

## [03]-[RESEARCH]

- [GLM_IRLS]: the GLM fit folds the canonical-link family deviance to a `torch` loss minimized under `LBFGS` through `IterativeEngine.Minimize`, reverse-mode-differentiated by `backward()` over the `IterativeEngine.GlmDeviance` row (`softplus(η)−y·η` logistic / `exp(η)−y·η` Poisson / Gamma), so the second-order quasi-Newton step replaces the explicit IRLS normal-equation re-solve; the open leaf is the per-family deviance the receipt's fit-quality metric reports, grounded against the `MathNet.Numerics.Distributions` `Density`/`CumulativeDistribution` surface at the fit-quality stamp and against the `torch.special.expit`/`gammaln` link/normalizer terms where the family needs the exact normalizing constant.
- [SVM_KERNEL]: the kernel-SVM dual fit (the margin `EstimatorModel.Margin` carrier) folds the hinge/quadratic-margin dual objective to a `torch` loss minimized under `LBFGS` (the row's `Driver`) with `torch.nn.utils.clip_grad_norm_` bounding an ill-conditioned step; the kernel-bandwidth and the dual-coefficient sparsity are policy columns on `EstimatorPolicy`, and the dual solve grounds at the classification fit gate, the Gram-matrix Cholesky riding the `Tensor/blas#DENSE_ALGEBRA` route where the closed-form conditioning holds.
- [GRADUATION_MODELS]: the offline deep-training models (gradient-boosted, deep-net) are the Python `compute/graduation` companion's, decoded by content key over the `ONE_GRADUATION_EVIDENCE` seam — C# owns the classical fit plus the ONNX inference, and the model-signature (input/output tensor names, feature interleave) grounds against the companion-published signature at the graduation-decode gate, the fitted classical estimators staying the C# producer.
