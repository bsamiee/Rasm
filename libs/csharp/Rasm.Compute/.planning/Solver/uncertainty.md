# [COMPUTE_UNCERTAINTY]

Rasm.Compute solver uncertainty: one `UncertaintyMethod` forward-UQ/reliability axis carrying a keyless `UqStrategy` driver and `SampleDesign` matrix behavior. `RandomVariable` owns inverse transforms and orthonormal recurrences; `UncertaintyResult` carries explicit optional response moments beside quantiles, sensitivity indices, failure probability, reliability index, and physical most-probable point.

Variance-reduced draws ride `LowDiscrepancy` through inverse transform; the Monte Carlo row owns one seeded pseudo-random matrix. Response moments use `MathNet.Numerics.Statistics`; PCE coefficients use dense or sparse QR; structured Saltelli and Morris designs own their estimators. Correlated inputs use one Cholesky-gated Gaussian copula. FORM/SORM uses a `LimitState` union over exact HyperJet derivatives or typed finite differences, and subset simulation uses the Au-Beck modified Metropolis chain.

## [01]-[INDEX]

- [01]-[UNCERTAINTY_LANE]: forward-UQ MC/LHS/PCE/subset propagation; HLRF FORM + Breitung SORM; Sobol first/total + Morris screen; Gaussian-copula correlation; failure-prob β.

## [02]-[UNCERTAINTY_LANE]

- Owner: `UncertaintyMethod` `[SmartEnum<string>]` propagation-strategy rows carrying a `UqStrategy` driver discriminant and a `SampleDesign` matrix column; `RandomVariable` `[Union]` input-distribution cases each lowering to an inverse-transform `Quantile`, a closed-form `Mean`, a `Standardize` map, a `PolynomialBasis` Wiener-Askey label, and a `RecurrenceCoefficients` orthonormal-polynomial row; `RecurrenceCoefficients` the one orthonormal three-term-recurrence owner (the four Askey closed forms plus the discretized-Stieltjes arbitrary-PCE construction); `UncertaintyResult` the distribution-valued response carrier (moments through kurtosis + quantiles + first/total Sobol + Morris interaction + `pf` + `β` + the physical MPP); `Uncertainty` the static `UqStrategy`-dispatched (total `Switch`) `Propagate` fold driving the `Solver/optimizer#OPTIMIZER_LANE` `evaluate` oracle.
- Cases: `SampleDesign` pseudo-random · space-filling · stratified · conditional-levels · Saltelli-AB-AB · Morris-trajectory · analytic; `UncertaintyMethod` monte-carlo · latin-hypercube-mc · polynomial-chaos · first-order-reliability · second-order-reliability · subset-simulation · sobol-saltelli · morris; `PolynomialBasis` hermite · legendre · laguerre · jacobi · arbitrary; `RandomVariable` normal · log-normal · uniform · gamma · exponential · Weibull · Gumbel · beta · triangular · empirical.
- Entry: `public static Fin<UncertaintyResult> Propagate(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock)` validates every input distribution, unique names, policy bounds, method/design compatibility, response component, and correlation matrix before dispatch. `Component` faults a short or non-finite response vector; no first-component or zero fallback exists.
- Auto: `Propagate` builds the optional Gaussian-copula `Transform` (identity when absent) and dispatches the `UqStrategy` driver off the `UncertaintyMethod.Strategy` row — the matrix-sampling driver draws the `LowDiscrepancy.Sobol` unit matrix, shapes it per `SampleDesign` (space-filling, LHS-stratified, the Saltelli `(2+d)·N` A/B/AB block, or the Morris `(d+1)·r` trajectory grid), maps each unit row through the copula and the per-axis `Quantile`, evaluates, and reduces to the moment fold plus the Saltelli/Morris indices or the composed `SensitivityTornado` first-order; the spectral-fit driver fits the orthonormal Vandermonde over the per-input `RecurrenceCoefficients` through thin-QR (sparse-QR for a large basis) and reads mean/variance/Sobol closed-form from the coefficient masses; the reliability-search driver runs HLRF to the standard-normal MPP scoring `β`/`pf`/importance-factors, the SORM row adding the Breitung curvature correction; the subset driver conditions successive populations on intermediate thresholds through the Au-Beck sampler so a `pf~10⁻⁶` rare event resolves in `O(N·log pf)` evaluations. State threads as one immutable fold accumulator, never a per-method mutable loop.
- Receipt: `Receipt` projects the full `UncertaintyResult` onto the widened `Uncertainty` `ComputeReceipt` case — method key, realized sample/evaluation count, nullable mean/variance/skewness/kurtosis (a method that does not estimate a moment carries `null`, never `NaN` or a fabricated failure), quantiles, first-order and total-effect Sobol indices, Morris interaction σ, the physical MPP, `pf`, and `β` — under `ReceiptScope.Execution`.
- Packages: MathNet.Numerics, HyperJet (the exact-AD FORM/SORM gradient/Hessian leg via `SensitivityLaw`), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new propagation strategy is one `UncertaintyMethod` row binding its `UqStrategy` driver and `SampleDesign`; a new input distribution is one `RandomVariable` case lowering to its `Quantile`/`Mean`/`Standardize`/`Basis`/`Recurrence` — an Askey-family input binds a closed-form `RecurrenceCoefficients` constructor, a non-Askey input falls to the one `Stieltjes` construction with zero new surface; a new response statistic is one field on `UncertaintyResult` plus one slot on the `Uncertainty` receipt; a `MonteCarloRunner`/`LatinHypercubeSampler`/`PceFitter`/`FormSolver`/`SormSolver`/`SaltelliSobol`/`MorrisScreening`/`SubsetSimulator` sibling family is collapsed onto one `UqStrategy`-dispatched (total `Switch`) `Uncertainty` fold, a `MomentsResult`/`ReliabilityResult`/`SensitivityResult` result trio onto the one `UncertaintyResult` carrier, a `NormalVariable`/`WeibullVariable`/`EmpiricalVariable` class family onto the one `RandomVariable` union, and a `HermiteBasis`/`LegendreBasis`/`LaguerreBasis`/`JacobiBasis` polynomial-evaluator family onto the one `RecurrenceCoefficients` orthonormal recurrence.
- Boundary: `evaluate` is the single solver coupling. Monte Carlo uses one seeded pseudo-random matrix; variance-reduced designs use the owned `LowDiscrepancy` generator; subset simulation uses one seeded conditional chain. Correlation admission requires a finite symmetric unit-diagonal positive-definite matrix and rejects PCE/Saltelli/Morris until a generalized correlated-sensitivity estimator exists. FORM faults a degenerate gradient or iteration-cap miss. SORM counts curvature evaluations and faults invalid Breitung curvature domains instead of dropping factors. Subset simulation faults a level-cap miss. Reliability-only results carry absent moments as `None`, not `NaN` sentinels.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum]
public sealed partial class SampleDesign {
    public static readonly SampleDesign PseudoRandom = new();
    public static readonly SampleDesign SpaceFilling = new();
    public static readonly SampleDesign Stratified = new();
    public static readonly SampleDesign ConditionalLevels = new();
    public static readonly SampleDesign SaltelliAbAb = new();
    public static readonly SampleDesign MorrisTrajectory = new();
    public static readonly SampleDesign Analytic = new();
}

[SmartEnum]
public sealed partial class UqStrategy {
    public static readonly UqStrategy MatrixSampling = new();
    public static readonly UqStrategy SpectralFit = new();
    public static readonly UqStrategy ReliabilitySearch = new();
    public static readonly UqStrategy Subset = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UncertaintyMethod {
    public static readonly UncertaintyMethod MonteCarlo = new("monte-carlo", UqStrategy.MatrixSampling, SampleDesign.PseudoRandom);
    public static readonly UncertaintyMethod LatinHypercubeMc = new("latin-hypercube-mc", UqStrategy.MatrixSampling, SampleDesign.Stratified);
    public static readonly UncertaintyMethod PolynomialChaos = new("polynomial-chaos", UqStrategy.SpectralFit, SampleDesign.Stratified);
    public static readonly UncertaintyMethod FirstOrderReliability = new("first-order-reliability", UqStrategy.ReliabilitySearch, SampleDesign.Analytic);
    public static readonly UncertaintyMethod SecondOrderReliability = new("second-order-reliability", UqStrategy.ReliabilitySearch, SampleDesign.Analytic);
    public static readonly UncertaintyMethod SubsetSimulation = new("subset-simulation", UqStrategy.Subset, SampleDesign.ConditionalLevels);
    public static readonly UncertaintyMethod SobolSaltelli = new("sobol-saltelli", UqStrategy.MatrixSampling, SampleDesign.SaltelliAbAb);
    public static readonly UncertaintyMethod Morris = new("morris", UqStrategy.MatrixSampling, SampleDesign.MorrisTrajectory);

    public UqStrategy Strategy { get; }
    public SampleDesign Design { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PolynomialBasis {
    public static readonly PolynomialBasis Hermite = new("hermite", askey: true);
    public static readonly PolynomialBasis Legendre = new("legendre", askey: true);
    public static readonly PolynomialBasis Laguerre = new("laguerre", askey: true);
    public static readonly PolynomialBasis Jacobi = new("jacobi", askey: true);
    public static readonly PolynomialBasis Arbitrary = new("arbitrary", askey: false);

    public bool Askey { get; }
}

public sealed record RecurrenceCoefficients(ImmutableArray<double> A, ImmutableArray<double> B) {
    public double Evaluate(int degree, double x) {
        double prev = 0.0, cur = 1.0;
        for (int k = 0; k < degree; k++) {
            double next = ((x - A[k]) * cur - Math.Sqrt(B[k]) * prev) / Math.Sqrt(B[k + 1]);
            prev = cur;
            cur = next;
        }
        return cur;
    }

    public static RecurrenceCoefficients Hermite(int order) =>
        Closed(order, static _ => 0.0, static k => k == 0 ? 1.0 : k);

    public static RecurrenceCoefficients Legendre(int order) =>
        Closed(order, static _ => 0.0, static k => k == 0 ? 1.0 : (double)(k * k) / ((2 * k - 1) * (2 * k + 1)));

    public static RecurrenceCoefficients Laguerre(int order, double alpha) =>
        Closed(order, k => 2.0 * k + alpha + 1.0, k => k == 0 ? 1.0 : k * (k + alpha));

    public static RecurrenceCoefficients Jacobi(int order, double alpha, double beta) =>
        Closed(order,
            k => JacobiA(k, alpha, beta),
            k => JacobiB(k, alpha, beta));

    static double JacobiA(int degree, double alpha, double beta) {
        double denominator = (2.0 * degree + alpha + beta) * (2.0 * degree + alpha + beta + 2.0);
        return Math.Abs(denominator) < 1e-15 ? (beta - alpha) / 2.0 : (beta * beta - alpha * alpha) / denominator;
    }

    static double JacobiB(int degree, double alpha, double beta) {
        if (degree == 0) { return 1.0; }
        double sum = alpha + beta;
        double common = 4.0 * degree * (degree + alpha) * (degree + beta);
        double scale = 2.0 * degree + sum;
        return degree == 1 && Math.Abs(sum + 1.0) < 1e-15
            ? common / (scale * scale * (scale + 1.0))
            : common * (degree + sum) / (scale * scale * (scale + 1.0) * (scale - 1.0));
    }

    public static RecurrenceCoefficients Stieltjes(int order, int nodes, Func<double, double> quantile) {
        int m = Math.Max(order + 2, nodes);
        double[] x = [.. Enumerable.Range(0, m).Select(j => quantile((j + 0.5) / m))];
        double w = 1.0 / m;
        double[] a = new double[order + 1], b = new double[order + 2];
        double[] prev = new double[m], cur = [.. Enumerable.Repeat(1.0, m)];
        b[0] = 1.0;
        double normCur = 1.0;
        for (int k = 0; k <= order; k++) {
            double moment = 0.0;
            for (int j = 0; j < m; j++) { moment += w * x[j] * cur[j] * cur[j]; }
            a[k] = normCur > 1e-300 ? moment / normCur : 0.0;
            double[] next = new double[m];
            double normNext = 0.0;
            for (int j = 0; j < m; j++) {
                next[j] = (x[j] - a[k]) * cur[j] - (k == 0 ? 0.0 : b[k]) * prev[j];
                normNext += w * next[j] * next[j];
            }
            b[k + 1] = normCur > 1e-300 ? normNext / normCur : 1.0;
            (prev, cur, normCur) = (cur, next, normNext);
        }
        return new RecurrenceCoefficients([.. a], [.. b]);
    }

    static RecurrenceCoefficients Closed(int order, Func<int, double> a, Func<int, double> b) =>
        new([.. Enumerable.Range(0, order + 1).Select(a)], [.. Enumerable.Range(0, order + 2).Select(b)]);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RandomVariable {
    private RandomVariable() { }

    public sealed record Normal(string Name, double Mean, double StdDev) : RandomVariable;
    public sealed record LogNormal(string Name, double Mu, double Sigma) : RandomVariable;
    public sealed record Uniform(string Name, double Lower, double Upper) : RandomVariable;
    public sealed record Gamma(string Name, double Shape, double Rate) : RandomVariable;
    public sealed record Exponential(string Name, double Rate) : RandomVariable;
    public sealed record Weibull(string Name, double Shape, double Scale) : RandomVariable;
    public sealed record Gumbel(string Name, double Location, double Scale) : RandomVariable;
    public sealed record Beta(string Name, double A, double B) : RandomVariable;
    public sealed record Triangular(string Name, double Lower, double Upper, double Mode) : RandomVariable;
    public sealed record Empirical(string Name, Seq<double> Support, Seq<double> Cdf) : RandomVariable;

    public string VariableName =>
        Switch(
            normal: static v => v.Name, logNormal: static v => v.Name, uniform: static v => v.Name,
            gamma: static v => v.Name, exponential: static v => v.Name, weibull: static v => v.Name,
            gumbel: static v => v.Name, beta: static v => v.Name, triangular: static v => v.Name, empirical: static v => v.Name);

    public bool Invalid =>
        Switch(
            normal: static value => InvalidName(value.Name) || !double.IsFinite(value.Mean) || !double.IsFinite(value.StdDev) || value.StdDev <= 0.0,
            logNormal: static value => InvalidName(value.Name) || !double.IsFinite(value.Mu) || !double.IsFinite(value.Sigma) || value.Sigma <= 0.0,
            uniform: static value => InvalidName(value.Name) || !double.IsFinite(value.Lower) || !double.IsFinite(value.Upper) || value.Lower >= value.Upper,
            gamma: static value => InvalidName(value.Name) || !double.IsFinite(value.Shape) || value.Shape <= 0.0 || !double.IsFinite(value.Rate) || value.Rate <= 0.0,
            exponential: static value => InvalidName(value.Name) || !double.IsFinite(value.Rate) || value.Rate <= 0.0,
            weibull: static value => InvalidName(value.Name) || !double.IsFinite(value.Shape) || value.Shape <= 0.0 || !double.IsFinite(value.Scale) || value.Scale <= 0.0,
            gumbel: static value => InvalidName(value.Name) || !double.IsFinite(value.Location) || !double.IsFinite(value.Scale) || value.Scale <= 0.0,
            beta: static value => InvalidName(value.Name) || !double.IsFinite(value.A) || value.A <= 0.0 || !double.IsFinite(value.B) || value.B <= 0.0,
            triangular: static value => InvalidName(value.Name) || !double.IsFinite(value.Lower) || !double.IsFinite(value.Upper) || !double.IsFinite(value.Mode) || value.Lower >= value.Upper || value.Mode < value.Lower || value.Mode > value.Upper,
            empirical: static value => InvalidName(value.Name) || InvalidEmpirical(value.Support, value.Cdf));

    public PolynomialBasis Basis =>
        Switch(
            normal: static _ => PolynomialBasis.Hermite, logNormal: static _ => PolynomialBasis.Hermite,
            uniform: static _ => PolynomialBasis.Legendre,
            gamma: static _ => PolynomialBasis.Laguerre, exponential: static _ => PolynomialBasis.Laguerre,
            beta: static _ => PolynomialBasis.Jacobi,
            weibull: static _ => PolynomialBasis.Arbitrary, gumbel: static _ => PolynomialBasis.Arbitrary,
            triangular: static _ => PolynomialBasis.Arbitrary, empirical: static _ => PolynomialBasis.Arbitrary);

    public double Quantile(double u) =>
        Switch(
            state: Math.Clamp(u, 1e-12, 1.0 - 1e-12),
            normal: static (p, v) => Normal.InvCDF(v.Mean, v.StdDev, p),
            logNormal: static (p, v) => LogNormal.InvCDF(v.Mu, v.Sigma, p),
            uniform: static (p, v) => v.Lower + (v.Upper - v.Lower) * p,
            gamma: static (p, v) => Gamma.InvCDF(v.Shape, v.Rate, p),
            exponential: static (p, v) => Exponential.InvCDF(v.Rate, p),
            weibull: static (p, v) => v.Scale * Math.Pow(-Math.Log(1.0 - p), 1.0 / v.Shape),
            gumbel: static (p, v) => v.Location - v.Scale * Math.Log(-Math.Log(p)),
            beta: static (p, v) => Beta.InvCDF(v.A, v.B, p),
            triangular: static (p, v) => Triangular.InvCDF(v.Lower, v.Upper, v.Mode, p),
            empirical: static (p, v) => EmpiricalQuantile(v.Support, v.Cdf, p));

    public double Mean =>
        Switch(
            normal: static v => v.Mean,
            logNormal: static v => Math.Exp(v.Mu + 0.5 * v.Sigma * v.Sigma),
            uniform: static v => 0.5 * (v.Lower + v.Upper),
            gamma: static v => v.Shape / v.Rate,
            exponential: static v => 1.0 / v.Rate,
            weibull: static v => v.Scale * SpecialFunctions.Gamma(1.0 + 1.0 / v.Shape),
            gumbel: static v => v.Location + v.Scale * 0.5772156649015329,
            beta: static v => v.A / (v.A + v.B),
            triangular: static v => (v.Lower + v.Upper + v.Mode) / 3.0,
            empirical: static v => EmpiricalMean(v.Support, v.Cdf));

    public double Standardize(double value) =>
        Switch(
            state: value,
            normal: static (x, v) => (x - v.Mean) / Math.Max(1e-12, v.StdDev),
            logNormal: static (x, v) => (Math.Log(Math.Max(1e-300, x)) - v.Mu) / Math.Max(1e-12, v.Sigma),
            uniform: static (x, v) => 2.0 * (x - v.Lower) / Math.Max(1e-12, v.Upper - v.Lower) - 1.0,
            gamma: static (x, v) => v.Rate * x,
            exponential: static (x, v) => v.Rate * x,
            weibull: static (x, _) => x,
            gumbel: static (x, _) => x,
            beta: static (x, _) => 2.0 * Math.Clamp(x, 0.0, 1.0) - 1.0,
            triangular: static (x, _) => x,
            empirical: static (x, _) => x);

    public RecurrenceCoefficients Recurrence(int order, int nodes) =>
        Switch(
            state: (Order: order, Nodes: nodes),
            normal: static (s, _) => RecurrenceCoefficients.Hermite(s.Order),
            logNormal: static (s, _) => RecurrenceCoefficients.Hermite(s.Order),
            uniform: static (s, _) => RecurrenceCoefficients.Legendre(s.Order),
            gamma: static (s, v) => RecurrenceCoefficients.Laguerre(s.Order, v.Shape - 1.0),
            exponential: static (s, _) => RecurrenceCoefficients.Laguerre(s.Order, 0.0),
            beta: static (s, v) => RecurrenceCoefficients.Jacobi(s.Order, v.B - 1.0, v.A - 1.0),
            weibull: static (s, v) => RecurrenceCoefficients.Stieltjes(s.Order, s.Nodes, v.Quantile),
            gumbel: static (s, v) => RecurrenceCoefficients.Stieltjes(s.Order, s.Nodes, v.Quantile),
            triangular: static (s, v) => RecurrenceCoefficients.Stieltjes(s.Order, s.Nodes, v.Quantile),
            empirical: static (s, v) => RecurrenceCoefficients.Stieltjes(s.Order, s.Nodes, v.Quantile));

    static double EmpiricalQuantile(Seq<double> support, Seq<double> cdf, double p) {
        if (support.IsEmpty) { return p; }
        int lo = 0, hi = cdf.Count - 1;
        while (lo < hi) { int mid = (lo + hi) >> 1; if (cdf[mid] < p) { lo = mid + 1; } else { hi = mid; } }
        return support[Math.Min(lo, support.Count - 1)];
    }

    static double EmpiricalMean(Seq<double> support, Seq<double> cdf) {
        double mean = 0.0, prior = 0.0;
        for (int i = 0; i < support.Count && i < cdf.Count; i++) { mean += support[i] * (cdf[i] - prior); prior = cdf[i]; }
        return mean;
    }

    static bool InvalidName(string value) => string.IsNullOrWhiteSpace(value);

    static bool InvalidEmpirical(Seq<double> support, Seq<double> cdf) =>
        support.Count < 2 || support.Count != cdf.Count || !support.ForAll(double.IsFinite) || !cdf.ForAll(double.IsFinite)
        || Enumerable.Range(1, support.Count - 1).Any(index => support[index] <= support[index - 1] || cdf[index] <= cdf[index - 1])
        || cdf[0] <= 0.0 || cdf[cdf.Count - 1] < 1.0 - 1e-12 || cdf[cdf.Count - 1] > 1.0 + 1e-12;
}

// --- [MODELS] ---------------------------------------------------------------------------

public delegate DDScalarSpan SpanLimitState(ReadOnlySpan<double> values, int order, Span<double> storage);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SmoothLimitState {
    private SmoothLimitState() { }

    public sealed record Dynamic(Func<DDScalar[], DDScalar> Evaluate) : SmoothLimitState;
    public sealed record SpanBacked(SpanLimitState Evaluate) : SmoothLimitState;
}

public sealed record UncertaintyPolicy(
    UncertaintyMethod Method,
    int Samples,
    int PceOrder,
    bool HyperbolicTruncation,
    int MorrisLevels,
    double SubsetLevelProbability,
    Seq<double> QuantileTaus,
    int LimitStateObjective,
    double LimitStateThreshold,
    int Seed,
    Option<Matrix<double>> Correlation,
    double FiniteDifferenceStep,
    int ReliabilityIterations,
    double ReliabilityTolerance,
    int SubsetMaxLevels,
    int StieltjesNodes,
    int SparseBasisThreshold,
    Option<SmoothLimitState> SmoothLimitState) {
    public static readonly UncertaintyPolicy CanonicalMonteCarlo = new(
        UncertaintyMethod.MonteCarlo, Samples: 4096, PceOrder: 3, HyperbolicTruncation: false, MorrisLevels: 4, SubsetLevelProbability: 0.1,
        QuantileTaus: Seq(0.05, 0.5, 0.95), LimitStateObjective: 0, LimitStateThreshold: 0.0, Seed: 0x5DEECE66,
        Correlation: None, FiniteDifferenceStep: 1e-4, ReliabilityIterations: 50, ReliabilityTolerance: 1e-6, SubsetMaxLevels: 12, StieltjesNodes: 256, SparseBasisThreshold: 512, SmoothLimitState: None);
    public static readonly UncertaintyPolicy CanonicalLatinHypercube = CanonicalMonteCarlo with { Method = UncertaintyMethod.LatinHypercubeMc };
    public static readonly UncertaintyPolicy CanonicalReliability = CanonicalMonteCarlo with { Method = UncertaintyMethod.FirstOrderReliability, Samples = 512 };
    public static readonly UncertaintyPolicy CanonicalSorm = CanonicalReliability with { Method = UncertaintyMethod.SecondOrderReliability };
    public static readonly UncertaintyPolicy CanonicalChaos = CanonicalMonteCarlo with { Method = UncertaintyMethod.PolynomialChaos, Samples = 1024, HyperbolicTruncation = true };
    public static readonly UncertaintyPolicy CanonicalSaltelli = CanonicalMonteCarlo with { Method = UncertaintyMethod.SobolSaltelli };
    public static readonly UncertaintyPolicy CanonicalMorris = CanonicalMonteCarlo with { Method = UncertaintyMethod.Morris, Samples = 512 };
    public static readonly UncertaintyPolicy CanonicalSubset = CanonicalMonteCarlo with { Method = UncertaintyMethod.SubsetSimulation, Samples = 1000 };

    public Fin<Unit> Validate(Seq<RandomVariable> inputs) {
        bool values = Samples < 2 || PceOrder < 1 || MorrisLevels < 2 || SubsetLevelProbability is <= 0.0 or >= 1.0
            || QuantileTaus.IsEmpty || !QuantileTaus.ForAll(static tau => double.IsFinite(tau) && tau is > 0.0 and < 1.0)
            || QuantileTaus.ToHashSet().Count != QuantileTaus.Count || LimitStateObjective < 0 || !double.IsFinite(LimitStateThreshold)
            || !double.IsFinite(FiniteDifferenceStep) || FiniteDifferenceStep <= 0.0 || ReliabilityIterations < 1
            || !double.IsFinite(ReliabilityTolerance) || ReliabilityTolerance <= 0.0 || SubsetMaxLevels < 1 || StieltjesNodes < PceOrder + 2 || SparseBasisThreshold < 1;
        bool design = Method == UncertaintyMethod.SobolSaltelli && Samples % 2 != 0
            || Method == UncertaintyMethod.Morris && Samples < inputs.Count + 1
            || Correlation.IsSome && (Method == UncertaintyMethod.PolynomialChaos || Method == UncertaintyMethod.SobolSaltelli || Method == UncertaintyMethod.Morris)
            || SmoothLimitState.IsSome && Method != UncertaintyMethod.FirstOrderReliability && Method != UncertaintyMethod.SecondOrderReliability;
        bool variables = inputs.IsEmpty || inputs.Exists(static input => input.Invalid)
            || inputs.Map(static input => input.VariableName).ToHashSet(StringComparer.Ordinal).Count != inputs.Count;
        return values || design || variables
            ? Fin.Fail<Unit>(ComputeFault.Create("<uncertainty-invalid-admission>"))
            : Fin.Succ(unit);
    }
}

public sealed record UncertaintyResult(
    UncertaintyMethod Method,
    int Samples,
    Option<double> Mean,
    Option<double> Variance,
    Option<double> Skewness,
    Option<double> Kurtosis,
    Seq<double> Quantiles,
    Seq<double> SobolFirst,
    Seq<double> SobolTotal,
    Seq<double> Interaction,
    Seq<double> MostProbablePoint,
    double FailureProbability,
    double ReliabilityIndex,
    Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Uncertainty {
    sealed record Transform(Option<Matrix<double>> Factor) {
        public ImmutableArray<double> Physical(Seq<RandomVariable> inputs, double[] unit) =>
            Factor.Match(
                None: () => [.. inputs.Map((v, i) => v.Quantile(unit[Math.Min(i, unit.Length - 1)]))],
                Some: l => FromU(inputs, [.. unit.Select(static u => Normal.InvCDF(0.0, 1.0, Math.Clamp(u, 1e-12, 1.0 - 1e-12)))]));

        public ImmutableArray<double> FromU(Seq<RandomVariable> inputs, double[] u) =>
            Factor.Match(
                None: () => Marginalize(inputs, u),
                Some: l => Marginalize(inputs, (l * Vector<double>.Build.DenseOfArray(u)).ToArray()));

        static ImmutableArray<double> Marginalize(Seq<RandomVariable> inputs, double[] z) =>
            [.. inputs.Map((v, i) => v.Quantile(Normal.CDF(0.0, 1.0, z[Math.Min(i, z.Length - 1)])))];
    }

    public static Fin<UncertaintyResult> Propagate(
        Seq<RandomVariable> inputs, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock) =>
        from _ in policy.Validate(inputs)
        from transform in Copula(inputs.Count, policy)
        from result in policy.Method.Strategy.Switch(
            state: (Inputs: inputs, Policy: policy, Transform: transform, Evaluate: evaluate, Clock: clock),
            matrixSampling: static s => SampleAndReduce(s.Inputs, s.Policy, s.Transform, s.Evaluate, s.Clock),
            spectralFit: static s => Spectral(s.Inputs, s.Policy, s.Transform, s.Evaluate, s.Clock),
            reliabilitySearch: static s => Reliability(s.Inputs, s.Policy, s.Transform, s.Evaluate, s.Clock),
            subset: static s => Subset(s.Inputs, s.Policy, s.Transform, s.Evaluate, s.Clock))
        select result;

    public static ComputeReceipt.Uncertainty Receipt(UncertaintyResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Method.Key, result.Samples,
            result.Mean.ToNullable(), result.Variance.ToNullable(), result.Skewness.ToNullable(), result.Kurtosis.ToNullable(),
            result.Quantiles, result.SobolFirst, result.SobolTotal, result.Interaction, result.MostProbablePoint,
            result.FailureProbability, result.ReliabilityIndex) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    static Fin<Transform> Copula(int dim, UncertaintyPolicy policy) =>
        policy.Correlation.Match(
            None: () => Fin.Succ(new Transform(None)),
            Some: r => r.RowCount != dim || r.ColumnCount != dim
                ? Fin.Fail<Transform>(new ComputeFault.ModelRejected($"<uncertainty-correlation-shape:{r.RowCount}x{r.ColumnCount}!={dim}>"))
                : !r.Enumerate().All(double.IsFinite)
                    || !Enumerable.Range(0, dim).All(axis => Math.Abs(r[axis, axis] - 1.0) <= 1e-10
                        && Enumerable.Range(0, dim).All(other => Math.Abs(r[axis, other] - r[other, axis]) <= 1e-10))
                    ? Fin.Fail<Transform>(ComputeFault.Create("<uncertainty-correlation-invalid>"))
                    : Try.lift(() => r.Cholesky()).Run()
                        .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<uncertainty-correlation-not-positive-definite:{error.GetType().Name}>"))
                        .Bind(cholesky => double.IsFinite(cholesky.DeterminantLn)
                            ? Fin.Succ(new Transform(Some(cholesky.Factor)))
                            : Fin.Fail<Transform>(ComputeFault.Create("<uncertainty-correlation-not-positive-definite>"))));

    static Fin<double> Component(Seq<double> values, UncertaintyPolicy policy) =>
        policy.LimitStateObjective >= values.Count || !values.ForAll(double.IsFinite)
            ? Fin.Fail<double>(ComputeFault.Create("<uncertainty-oracle-shape>"))
            : Fin.Succ(values[policy.LimitStateObjective]);

    // --- [MATRIX_SAMPLING] ------------------------------------------------------------

    static Fin<UncertaintyResult> SampleAndReduce(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock) =>
        Design(inputs, policy, transform)
            .Bind(design => Sample(design, policy, evaluate).Map(responses => Reduce(inputs, policy, design, responses, clock)));

    static Fin<Seq<ImmutableArray<double>>> Design(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform) {
        int count = Math.Max(2, policy.Samples), dim = inputs.Count;
        Fin<Seq<double[]>> unit = policy.Method.Design.Switch(
            state: (Policy: policy, Count: count, Dim: dim),
            pseudoRandom: static s => Fin.Succ(PseudoRandomDraws(s.Dim, s.Count, s.Policy.Seed)),
            spaceFilling: static s => SobolDraws(s.Dim, s.Count, s.Policy.Seed),
            stratified: static s => LowDiscrepancy.LatinHypercube(s.Dim, s.Count, s.Policy.Seed, Scramble.DigitalShift).Map(net => toSeq(net)),
            conditionalLevels: static s => SobolDraws(s.Dim, s.Count, s.Policy.Seed),
            saltelliAbAb: static s => SobolDraws(s.Dim, s.Count, s.Policy.Seed).Map(draws => Saltelli(draws, s.Count, s.Dim)),
            morrisTrajectory: static s => SobolDraws(s.Dim, s.Count, s.Policy.Seed).Map(draws => MorrisTrajectories(draws, s.Count, s.Dim, s.Policy.MorrisLevels)),
            analytic: static _ => Fin.Succ(Seq<double[]>()));
        return unit.Map(rows => rows.Map(row => transform.Physical(inputs, row)));
    }

    static Fin<Seq<double[]>> SobolDraws(int dim, int count, int seed) =>
        LowDiscrepancy.Sobol(dimensions: dim, seed: seed, Scramble.DigitalShift).Map(generator =>
            toSeq(Enumerable.Range(0, count)).Fold((Gen: generator, Points: Seq<double[]>()), static (acc, _) => {
                (LowDiscrepancy next, double[] point) = acc.Gen.Draw();
                return (next, acc.Points.Add(point));
            }).Points);

    static Seq<double[]> PseudoRandomDraws(int dim, int count, int seed) {
        Random random = new(seed);
        double[][] rows = [.. Enumerable.Range(0, count).Select(_ => new double[dim])];
        for (int row = 0; row < count; row++) {
            for (int axis = 0; axis < dim; axis++) { rows[row][axis] = random.NextDouble(); }
        }
        return toSeq(rows);
    }

    static Fin<Seq<Seq<double>>> Sample(Seq<ImmutableArray<double>> design, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate) =>
        design.Fold(Fin.Succ(Seq<Seq<double>>()), (acc, coordinates) =>
            acc.Bind(responses => evaluate(new DesignPoint(coordinates, [], [])).Bind(values => Component(values, policy).Map(_ => responses.Add(values)))));

    static UncertaintyResult Reduce(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, Seq<Seq<double>> responses, IClock clock) {
        double[] qoi = [.. responses.Map(values => values[policy.LimitStateObjective])];
        double mean = Statistics.Mean(qoi), variance = Statistics.Variance(qoi);
        double skewness = qoi.Length > 2 ? Statistics.Skewness(qoi) : 0.0;
        double kurtosis = qoi.Length > 3 ? Statistics.Kurtosis(qoi) : 0.0;
        Seq<double> quantiles = policy.QuantileTaus.Map(tau => Statistics.Quantile(qoi, tau));
        (Seq<double> first, Seq<double> total, Seq<double> interaction) =
            policy.Method == UncertaintyMethod.SobolSaltelli ? SaltelliIndices(inputs.Count, Math.Max(2, policy.Samples) / 2, qoi, variance)
            : policy.Method == UncertaintyMethod.Morris ? MorrisScreening(inputs.Count, policy, qoi)
            : (SobolBinned(inputs, design, qoi), Seq<double>(), Seq<double>());
        double pf = qoi.Length == 0 ? 0.0 : (double)qoi.Count(value => value > policy.LimitStateThreshold) / qoi.Length;
        double beta = pf is > 0.0 and < 1.0 ? -Normal.InvCDF(0.0, 1.0, pf) : pf <= 0.0 ? double.PositiveInfinity : double.NegativeInfinity;
        return new UncertaintyResult(policy.Method, qoi.Length, Some(mean), Some(variance), Some(skewness), Some(kurtosis), quantiles, first, total, interaction, Seq<double>(), pf, beta, clock.GetCurrentInstant());
    }

    static Seq<double[]> Saltelli(Seq<double[]> draws, int count, int dim) {
        int half = count / 2;
        double[][] a = draws.Take(half).ToArray();
        double[][] b = draws.Skip(half).Take(half).ToArray();
        List<double[]> matrix = [.. a];
        matrix.AddRange(b);
        for (int i = 0; i < dim; i++) {
            for (int row = 0; row < half; row++) {
                double[] cross = (double[])a[row].Clone();
                if (i < cross.Length && i < b[row].Length) { cross[i] = b[row][i]; }
                matrix.Add(cross);
            }
        }
        return toSeq(matrix);
    }

    static Seq<double[]> MorrisTrajectories(Seq<double[]> draws, int count, int dim, int levels) {
        double delta = levels / (2.0 * (levels - 1));
        List<double[]> trajectories = [];
        int paths = Math.Max(1, count / (dim + 1));
        for (int t = 0; t < paths; t++) {
            double[] basePoint = draws.At(t % draws.Count).IfNone(() => new double[dim]);
            trajectories.Add((double[])basePoint.Clone());
            for (int axis = 0; axis < dim; axis++) {
                double[] stepped = (double[])trajectories[^1].Clone();
                if (axis < stepped.Length) { stepped[axis] = Math.Clamp(stepped[axis] + delta, 0.0, 1.0); }
                trajectories.Add(stepped);
            }
        }
        return toSeq(trajectories);
    }

    static (Seq<double>, Seq<double>, Seq<double>) SaltelliIndices(int dim, int half, double[] y, double variance) {
        if (y.Length < (2 + dim) * half || variance <= 1e-18 || half <= 0) {
            Seq<double> zero = toSeq(Enumerable.Repeat(0.0, dim));
            return (zero, zero, Seq<double>());
        }
        ReadOnlySpan<double> ya = y.AsSpan(0, half), yb = y.AsSpan(half, half);
        double[] first = new double[dim], total = new double[dim];
        for (int i = 0; i < dim; i++) {
            ReadOnlySpan<double> yab = y.AsSpan((2 + i) * half, half);
            double sumFirst = 0.0, sumTotal = 0.0;
            for (int row = 0; row < half; row++) {
                sumFirst += yb[row] * (yab[row] - ya[row]);
                double residual = ya[row] - yab[row];
                sumTotal += residual * residual;
            }
            first[i] = sumFirst / half / variance;
            total[i] = sumTotal / (2.0 * half) / variance;
        }
        return (toSeq(first), toSeq(total), Seq<double>());
    }

    static (Seq<double>, Seq<double>, Seq<double>) MorrisScreening(int dim, UncertaintyPolicy policy, double[] y) {
        double delta = policy.MorrisLevels / (2.0 * (policy.MorrisLevels - 1));
        int paths = Math.Max(1, y.Length / (dim + 1));
        double[] muStar = new double[dim], sum = new double[dim], sumSquare = new double[dim];
        for (int t = 0; t < paths; t++) {
            int baseRow = t * (dim + 1);
            for (int axis = 0; axis < dim && baseRow + axis + 1 < y.Length; axis++) {
                double effect = (y[baseRow + axis + 1] - y[baseRow + axis]) / delta;
                muStar[axis] += Math.Abs(effect);
                sum[axis] += effect;
                sumSquare[axis] += effect * effect;
            }
        }
        Seq<double> mu = toSeq(muStar.Select(s => s / paths));
        Seq<double> sigma = toSeq(Enumerable.Range(0, dim).Select(axis => {
            double meanEffect = sum[axis] / paths;
            return Math.Sqrt(Math.Max(0.0, sumSquare[axis] / paths - meanEffect * meanEffect));
        }));
        return (Seq<double>(), mu, sigma);
    }

    static Seq<double> SobolBinned(Seq<RandomVariable> inputs, Seq<ImmutableArray<double>> design, double[] qoi) {
        SweepGrid grid = new(
            inputs.Map(static v => (SweepAxis)new SweepAxis.Linear(v.VariableName, 0.0, 1.0, 2)),
            Seq(ObjectiveSense.Minimize),
            SensitivityMethod.SobolVariance);
        Seq<DesignPoint> points = toSeq(Enumerable.Range(0, Math.Min(design.Count, qoi.Length)))
            .Map(i => new DesignPoint(design[i], [qoi[i]], []));
        return SensitivityTornado.Of(grid, points, 0).Bars.Map(static bar => bar.Effect);
    }

    // --- [SPECTRAL_FIT] ---------------------------------------------------------------

    static Fin<UncertaintyResult> Spectral(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock) =>
        Design(inputs, policy, transform)
            .Bind(design => Sample(design, policy, evaluate).Bind(responses => Fit(inputs, policy, design, responses, clock)));

    static Fin<UncertaintyResult> Fit(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, Seq<Seq<double>> responses, IClock clock) {
        double[] qoi = [.. responses.Map(values => values[policy.LimitStateObjective])];
        Seq<int[]> multiIndices = MultiIndexSet(inputs.Count, policy.PceOrder, policy.HyperbolicTruncation);
        if (qoi.Length < multiIndices.Count) { return Fin.Fail<UncertaintyResult>(ComputeFault.Create($"<uncertainty-pce-underdetermined:{qoi.Length}<{multiIndices.Count}>")); }
        Seq<RecurrenceCoefficients> bases = inputs.Map(v => v.Recurrence(policy.PceOrder, policy.StieltjesNodes));
        Matrix<double> vandermonde = Matrix<double>.Build.Dense(qoi.Length, multiIndices.Count, (row, col) =>
            multiIndices[col].Select((degree, axis) => bases[axis].Evaluate(degree, inputs[axis].Standardize(design[row][axis]))).Aggregate(1.0, static (a, b) => a * b));
        Vector<double> rhs = Vector<double>.Build.DenseOfArray(qoi);
        Fin<Vector<double>> coefficients = policy.HyperbolicTruncation && multiIndices.Count > policy.SparseBasisThreshold
            ? SparseFit(vandermonde, qoi)
            : DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), vandermonde, rhs, TolerancePolicy.Derive(vandermonde, rhs));
        return coefficients.Map(c => ReadSpectral(inputs, policy, multiIndices, qoi, c, clock));
    }

    static Fin<Vector<double>> SparseFit(Matrix<double> vandermonde, double[] qoi) {
        List<int> rows = [];
        List<int> cols = [];
        List<double> values = [];
        for (int r = 0; r < vandermonde.RowCount; r++) {
            for (int c = 0; c < vandermonde.ColumnCount; c++) {
                double entry = vandermonde[r, c];
                if (Math.Abs(entry) > 1e-12) { rows.Add(r); cols.Add(c); values.Add(entry); }
            }
        }
        return SparseOps.Ingest(SparseFormat.Coo, vandermonde.RowCount, vandermonde.ColumnCount, [.. rows], [.. cols], [.. values])
            .Bind(csr => SparseOps.Factor(csr, FactorKind.Qr, ColumnOrdering.MinimumDegreeAtA, pivotTol: 0.0, dropFloor: 1e-12))
            .Bind(op => op.Solve(qoi, double.PositiveInfinity))
            .Map(static solution => Vector<double>.Build.DenseOfArray(solution));
    }

    static UncertaintyResult ReadSpectral(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<int[]> multiIndices, double[] qoi, Vector<double> coefficients, IClock clock) {
        double mean = coefficients[0], variance = 0.0;
        for (int k = 1; k < coefficients.Count; k++) { variance += coefficients[k] * coefficients[k]; }
        double[] first = new double[inputs.Count], total = new double[inputs.Count];
        for (int k = 1; k < coefficients.Count; k++) {
            double mass = coefficients[k] * coefficients[k];
            int[] index = multiIndices[k];
            int active = index.Count(static d => d > 0), sole = -1;
            for (int axis = 0; axis < index.Length; axis++) {
                if (index[axis] > 0) { total[axis] += mass; sole = axis; }
            }
            if (active == 1 && sole >= 0) { first[sole] += mass; }
        }
        double inverse = variance > 1e-18 ? 1.0 / variance : 0.0;
        Seq<double> quantiles = policy.QuantileTaus.Map(tau => Statistics.Quantile(qoi, tau));
        double standardDeviation = Math.Sqrt(Math.Max(0.0, variance));
        double beta = standardDeviation > 1e-12 ? (policy.LimitStateThreshold - mean) / standardDeviation : double.PositiveInfinity;
        double pf = double.IsFinite(beta) ? Normal.CDF(0.0, 1.0, -beta) : beta > 0.0 ? 0.0 : 1.0;
        double skewness = qoi.Length > 2 ? Statistics.Skewness(qoi) : 0.0;
        double kurtosis = qoi.Length > 3 ? Statistics.Kurtosis(qoi) : 0.0;
        return new UncertaintyResult(policy.Method, qoi.Length, Some(mean), Some(variance), Some(skewness), Some(kurtosis), quantiles,
            toSeq(first.Select(m => m * inverse)), toSeq(total.Select(m => m * inverse)), Seq<double>(), Seq<double>(), pf, beta, clock.GetCurrentInstant());
    }

    static Seq<int[]> MultiIndexSet(int dim, int order, bool hyperbolic) {
        List<int[]> indices = [];
        void Recurse(int axis, int[] current, int remaining) {
            if (axis == dim) {
                double q = hyperbolic ? Math.Pow(current.Sum(d => Math.Pow(d, 0.5)), 2.0) : current.Sum();
                if (q <= order + 1e-9) { indices.Add((int[])current.Clone()); }
                return;
            }
            for (int d = 0; d <= remaining; d++) { current[axis] = d; Recurse(axis + 1, current, remaining - (hyperbolic ? 0 : d)); }
        }
        Recurse(0, new int[dim], order);
        return toSeq(indices);
    }

    // --- [RELIABILITY_SEARCH] ---------------------------------------------------------

    sealed record MppState(double[] U, double[] Alpha, double[] Grad, double Beta, double FailureProbability, int Evaluations);
    sealed record HlrfAcc(double[] U, double GHere, double[] Grad, double G0, bool Converged, int Evals);

    static Fin<UncertaintyResult> Reliability(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock) {
        LimitState g = policy.SmoothLimitState.Match(
            Some: static smooth => smooth.Switch(
                dynamic: static source => (LimitState)new LimitState.Smooth(source.Evaluate),
                spanBacked: static source => (LimitState)new LimitState.SmoothSpan(source.Evaluate)),
            None: () => new LimitState.Oracle(u => evaluate(new DesignPoint(transform.FromU(inputs, u), [], []))
                .Bind(values => Component(values, policy).Map(value => policy.LimitStateThreshold - value))));
        return Hlrf(inputs.Count, policy, g).Bind(mpp =>
            (policy.Method == UncertaintyMethod.SecondOrderReliability ? Breitung(mpp, policy, g) : Fin.Succ((FailureProbability: mpp.FailureProbability, Evals: 0)))
                .Map(result => Assemble(inputs, policy, transform, mpp with { Evaluations = mpp.Evaluations + result.Evals }, result.FailureProbability, clock)));
    }

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record LimitState {
        private LimitState() { }

        public sealed record Oracle(Func<double[], Fin<double>> G) : LimitState;
        public sealed record Smooth(Func<DDScalar[], DDScalar> G) : LimitState;
        public sealed record SmoothSpan(SpanLimitState G) : LimitState;

        public Fin<(double G, double[] Grad, int Evals)> Probe(double[] u, double step) =>
            Switch(
                state: (U: u, Step: step),
                oracle: static (state, source) => FiniteProbe(state.U, state.Step, source.G),
                smooth: static (state, source) => SensitivityLaw.Gradient(source.G, state.U)
                    .Bind(result => double.IsFinite(result.Value) && result.Gradient.All(double.IsFinite)
                        ? Fin.Succ((result.Value, result.Gradient, 1))
                        : Fin.Fail<(double, double[], int)>(ComputeFault.Create("<limit-state-nonfinite>"))),
                smoothSpan: static (state, source) => SpanProbe(source.G, state.U));

        public Fin<(Matrix<double> Hessian, int Evals)> Curvature(double[] u, double step) =>
            Switch(
                state: (U: u, Step: step),
                oracle: static (state, source) => FiniteHessian(state.U, state.Step, source.G).Map(hessian => (hessian, 1 + 2 * state.U.Length * state.U.Length)),
                smooth: static (state, source) => SensitivityLaw.Hessian(source.G, state.U)
                    .Bind(result => result.Hessian.Cast<double>().All(double.IsFinite)
                        ? Fin.Succ((Matrix<double>.Build.DenseOfArray(result.Hessian), 1))
                        : Fin.Fail<(Matrix<double>, int)>(ComputeFault.Create("<limit-state-curvature-nonfinite>"))),
                smoothSpan: static (state, source) => SpanCurvature(source.G, state.U));
    }

    static Fin<(double G, double[] Grad, int Evals)> SpanProbe(SpanLimitState source, double[] u) {
        Span<double> storage = stackalloc double[Kernel.GetDataLength(u.Length, order: 1)];
        DDScalarSpan result = source(u, 1, storage);
        double[] gradient = new double[u.Length];
        for (int axis = 0; axis < gradient.Length; axis++) { gradient[axis] = result.G(axis); }
        return double.IsFinite(result.Value) && gradient.All(double.IsFinite)
            ? Fin.Succ((result.Value, gradient, 1))
            : Fin.Fail<(double, double[], int)>(ComputeFault.Create("<limit-state-span-nonfinite>"));
    }

    static Fin<(Matrix<double> Hessian, int Evals)> SpanCurvature(SpanLimitState source, double[] u) {
        Span<double> storage = stackalloc double[Kernel.GetDataLength(u.Length, order: 2)];
        DDScalarSpan result = source(u, 2, storage);
        // DDScalarSpan is a ref struct: H copies element-wise before the span dies — a Dense(n, n, result.H) method group would capture the ref struct receiver
        double[] curvature = new double[u.Length * u.Length];
        for (int row = 0; row < u.Length; row++) {
            for (int column = 0; column < u.Length; column++) { curvature[row * u.Length + column] = result.H(row, column); }
        }
        Matrix<double> hessian = Matrix<double>.Build.DenseOfRowMajor(u.Length, u.Length, curvature);
        return hessian.Enumerate().All(double.IsFinite)
            ? Fin.Succ((hessian, 1))
            : Fin.Fail<(Matrix<double>, int)>(ComputeFault.Create("<limit-state-span-curvature-nonfinite>"));
    }

    static UncertaintyResult Assemble(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, MppState mpp, double pf, IClock clock) {
        double beta = pf is > 0.0 and < 1.0 ? -Normal.InvCDF(0.0, 1.0, pf) : mpp.Beta;
        return new UncertaintyResult(policy.Method, mpp.Evaluations, None, None, None, None,
            Seq<double>(), Seq<double>(), toSeq(mpp.Alpha.Select(static a => a * a)), Seq<double>(), toSeq(transform.FromU(inputs, mpp.U)), pf, beta, clock.GetCurrentInstant());
    }

    static Fin<MppState> Hlrf(int dim, UncertaintyPolicy policy, LimitState g) =>
        Begin(dim, policy.FiniteDifferenceStep, g).Bind(start =>
            toSeq(Enumerable.Range(0, Math.Max(1, policy.ReliabilityIterations)))
                .Fold(Fin.Succ(start), (acc, _) => acc.Bind(state => state.Converged ? Fin.Succ(state) : Step(state, policy, g)))
                .Bind(state => state.Converged
                    ? Fin.Succ(Finalize(state))
                    : Fin.Fail<MppState>(ComputeFault.Create("<uncertainty-hlrf-not-converged>"))));

    static Fin<HlrfAcc> Begin(int dim, double step, LimitState g) =>
        g.Probe(new double[dim], step).Map(probe => new HlrfAcc(new double[dim], probe.G, probe.Grad, probe.G, false, probe.Evals));

    static Fin<(double G, double[] Grad, int Evals)> FiniteProbe(double[] u, double step, Func<double[], Fin<double>> g) =>
        g(u).Bind(here => toSeq(Enumerable.Range(0, u.Length)).Fold(
            Fin.Succ((Grad: new double[u.Length], Evals: 1)),
            (acc, i) => acc.Bind(state => {
                double[] up = (double[])u.Clone(); up[i] += step;
                double[] down = (double[])u.Clone(); down[i] -= step;
                return g(up).Bind(plus => g(down).Map(minus => {
                    state.Grad[i] = (plus - minus) / (2.0 * step);
                    return (state.Grad, state.Evals + 2);
                }));
            })).Map(state => (here, state.Grad, state.Evals)));

    static Fin<HlrfAcc> Step(HlrfAcc acc, UncertaintyPolicy policy, LimitState g) {
        double gradNormSquared = TensorPrimitives.SumOfSquares<double>(acc.Grad);
        if (!double.IsFinite(gradNormSquared) || gradNormSquared < 1e-24) { return Fin.Fail<HlrfAcc>(ComputeFault.Create("<uncertainty-hlrf-degenerate-gradient>")); }
        double scale = (TensorPrimitives.Dot<double>(acc.U, acc.Grad) - acc.GHere) / gradNormSquared;
        double[] next = [.. acc.Grad.Select(gi => scale * gi)];
        bool converged = Distance(next, acc.U) < policy.ReliabilityTolerance;
        return g.Probe(next, policy.FiniteDifferenceStep).Map(probe =>
            acc with { U = next, GHere = probe.G, Grad = probe.Grad, Converged = converged && Math.Abs(probe.G) < policy.ReliabilityTolerance, Evals = acc.Evals + probe.Evals });
    }

    static MppState Finalize(HlrfAcc acc) {
        double norm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(acc.U));
        double gradNorm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(acc.Grad));
        double[] alpha = gradNorm > 1e-12 ? [.. acc.Grad.Select(gi => -gi / gradNorm)] : new double[acc.U.Length];
        double beta = (acc.G0 >= 0.0 ? 1.0 : -1.0) * norm;
        return new MppState(acc.U, alpha, acc.Grad, beta, Normal.CDF(0.0, 1.0, -beta), acc.Evals);
    }

    static Fin<(double FailureProbability, int Evals)> Breitung(MppState mpp, UncertaintyPolicy policy, LimitState g) =>
        g.Curvature(mpp.U, policy.FiniteDifferenceStep).Bind(curvature => {
            double gradNorm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(mpp.Grad));
            if (gradNorm < 1e-12) { return Fin.Fail<(double, int)>(ComputeFault.Create("<uncertainty-sorm-degenerate-gradient>")); }
            double[] a = mpp.Alpha;
            Matrix<double> projector = Matrix<double>.Build.Dense(a.Length, a.Length, (i, j) => (i == j ? 1.0 : 0.0) - a[i] * a[j]);
            Matrix<double> curvatureMatrix = projector * curvature.Hessian * projector / gradNorm;
            return DenseOps.Decompose(curvatureMatrix, FactorizationKind.Evd).Bind(factor => {
                double[] kappa = factor is Factorization.Evd evd ? [.. evd.Decomposition.EigenValues.Map(static value => value.Real)] : [];
                if (kappa.Length != a.Length) { return Fin.Fail<(double, int)>(ComputeFault.Create("<uncertainty-sorm-curvature-shape>")); }
                int drop = NearestZero(kappa);
                if (kappa.Where((_, index) => index != drop).Any(value => !double.IsFinite(value) || 1.0 + mpp.Beta * value <= 0.0))
                    return Fin.Fail<(double, int)>(ComputeFault.Create("<uncertainty-sorm-curvature-domain>"));
                double product = 1.0;
                for (int j = 0; j < kappa.Length; j++) {
                    if (j == drop) { continue; }
                    double bracket = 1.0 + mpp.Beta * kappa[j];
                    product *= 1.0 / Math.Sqrt(bracket);
                }
                return Fin.Succ((FailureProbability: Math.Clamp(Normal.CDF(0.0, 1.0, -mpp.Beta) * product, 0.0, 1.0), Evals: curvature.Evals));
            });
        });

    static Fin<Matrix<double>> FiniteHessian(double[] u, double step, Func<double[], Fin<double>> g) {
        int n = u.Length;
        return g(u).Bind(g0 => toSeq(Pairs(n)).Fold(Fin.Succ(Matrix<double>.Build.Dense(n, n)), (acc, pair) =>
            acc.Bind(matrix => Second(u, pair.I, pair.J, step, g0, g).Map(value => {
                matrix[pair.I, pair.J] = value;
                matrix[pair.J, pair.I] = value;
                return matrix;
            }))));
    }

    static IEnumerable<(int I, int J)> Pairs(int n) {
        for (int i = 0; i < n; i++) { for (int j = i; j < n; j++) { yield return (i, j); } }
    }

    static Fin<double> Second(double[] u, int i, int j, double h, double g0, Func<double[], Fin<double>> g) =>
        i == j
            ? g(Shift(u, i, h)).Bind(plus => g(Shift(u, i, -h)).Map(minus => (plus - 2.0 * g0 + minus) / (h * h)))
            : g(Shift(u, i, h, j, h)).Bind(pp => g(Shift(u, i, h, j, -h)).Bind(pm =>
                g(Shift(u, i, -h, j, h)).Bind(mp => g(Shift(u, i, -h, j, -h)).Map(mm => (pp - pm - mp + mm) / (4.0 * h * h)))));

    static double[] Shift(double[] u, int i, double d) { double[] v = (double[])u.Clone(); v[i] += d; return v; }
    static double[] Shift(double[] u, int i, double di, int j, double dj) { double[] v = (double[])u.Clone(); v[i] += di; v[j] += dj; return v; }

    static double Distance(double[] a, double[] b) {
        double sum = 0.0;
        for (int i = 0; i < a.Length && i < b.Length; i++) { double d = a[i] - b[i]; sum += d * d; }
        return Math.Sqrt(sum);
    }

    static int NearestZero(double[] values) {
        int index = 0;
        for (int i = 1; i < values.Length; i++) { if (Math.Abs(values[i]) < Math.Abs(values[index])) { index = i; } }
        return index;
    }

    // --- [SUBSET_SIMULATION] ----------------------------------------------------------

    sealed record SubsetAcc(Seq<(double[] U, double Lsf)> Population, double Probability, bool Done, int Evaluations);

    static Fin<UncertaintyResult> Subset(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock) {
        int dim = inputs.Count, n = Math.Max(4, policy.Samples);
        double p0 = Math.Clamp(policy.SubsetLevelProbability, 0.01, 0.5);
        int keep = Math.Max(1, (int)Math.Round(p0 * n));
        Random rng = new(policy.Seed);
        Func<double[], Fin<double>> lsf = u => evaluate(new DesignPoint(transform.FromU(inputs, u), [], []))
            .Bind(values => Component(values, policy).Map(value => policy.LimitStateThreshold - value));
        return Population(dim, n, rng, lsf).Bind(initial =>
            toSeq(Enumerable.Range(0, policy.SubsetMaxLevels)).Fold(Fin.Succ(new SubsetAcc(initial, 1.0, false, n)),
                (acc, _) => acc.Bind(state => state.Done ? Fin.Succ(state) : Advance(state, dim, n, keep, p0, rng, lsf)))
            .Bind(state => state.Done
                ? Fin.Succ(SubsetResult(policy, state, clock))
                : Fin.Fail<UncertaintyResult>(ComputeFault.Create("<uncertainty-subset-level-cap>"))));
    }

    static Fin<Seq<(double[] U, double Lsf)>> Population(int dim, int n, Random rng, Func<double[], Fin<double>> lsf) =>
        toSeq(Enumerable.Range(0, n)).Fold(Fin.Succ(Seq<(double[], double)>()), (acc, _) => acc.Bind(population => {
            double[] u = StandardNormal(dim, rng);
            return lsf(u).Map(value => population.Add((u, value)));
        }));

    static Fin<SubsetAcc> Advance(SubsetAcc state, int dim, int n, int keep, double p0, Random rng, Func<double[], Fin<double>> lsf) {
        (double[] U, double Lsf)[] sorted = state.Population.OrderBy(static p => p.Lsf).ToArray();
        double threshold = sorted[Math.Min(keep, sorted.Length) - 1].Lsf;
        if (threshold <= 0.0) { return Fin.Succ(state with { Done = true }); }
        double[][] seeds = [.. sorted.Take(keep).Select(static p => p.U)];
        return Repopulate(seeds, dim, n, threshold, rng, lsf).Map(population =>
            state with { Population = population, Probability = state.Probability * p0, Evaluations = state.Evaluations + population.Count });
    }

    static Fin<Seq<(double[] U, double Lsf)>> Repopulate(double[][] seeds, int dim, int n, double threshold, Random rng, Func<double[], Fin<double>> lsf) {
        int basePerChain = Math.Max(1, n / seeds.Length), remainder = Math.Max(0, n - basePerChain * seeds.Length);
        return toSeq(seeds).Fold(Fin.Succ((Index: 0, Population: Seq<(double[], double)>())), (acc, seed) =>
            acc.Bind(state => Chain(seed, basePerChain + (state.Index < remainder ? 1 : 0), dim, threshold, rng, lsf)
                .Map(chain => (state.Index + 1, state.Population + chain))))
            .Map(static state => state.Population);
    }

    static Fin<Seq<(double[] U, double Lsf)>> Chain(double[] seed, int steps, int dim, double threshold, Random rng, Func<double[], Fin<double>> lsf) =>
        lsf(seed).Bind(seedLsf => toSeq(Enumerable.Range(0, steps)).Fold(
            Fin.Succ((State: (U: seed, Lsf: seedLsf), Out: Seq<(double[], double)>())),
            (acc, _) => acc.Bind(step => {
                double[] candidate = Propose(step.State.U, dim, rng);
                return lsf(candidate).Map(candidateLsf => {
                    (double[], double) next = candidateLsf <= threshold ? (candidate, candidateLsf) : step.State;
                    return (State: next, Out: step.Out.Add(next));
                });
            })).Map(static result => result.Out));

    static double[] Propose(double[] u, int dim, Random rng) =>
        [.. Enumerable.Range(0, dim).Select(i => {
            double candidate = u[i] + Normal.InvCDF(0.0, 1.0, Math.Clamp(rng.NextDouble(), 1e-12, 1.0 - 1e-12));
            double ratio = Math.Exp(0.5 * (u[i] * u[i] - candidate * candidate));
            return rng.NextDouble() < ratio ? candidate : u[i];
        })];

    static double[] StandardNormal(int dim, Random rng) =>
        [.. Enumerable.Range(0, dim).Select(_ => Normal.InvCDF(0.0, 1.0, Math.Clamp(rng.NextDouble(), 1e-12, 1.0 - 1e-12)))];

    static UncertaintyResult SubsetResult(UncertaintyPolicy policy, SubsetAcc state, IClock clock) {
        double[] lsf = [.. state.Population.Map(static p => p.Lsf)];
        double finalFraction = lsf.Length == 0 ? 0.0 : (double)lsf.Count(static value => value <= 0.0) / lsf.Length;
        double pf = Math.Clamp(state.Probability * finalFraction, 0.0, 1.0);
        double beta = pf is > 0.0 and < 1.0 ? -Normal.InvCDF(0.0, 1.0, pf) : pf <= 0.0 ? double.PositiveInfinity : double.NegativeInfinity;
        return new UncertaintyResult(policy.Method, state.Evaluations, None, None, None, None,
            Seq<double>(), Seq<double>(), Seq<double>(), Seq<double>(), Seq<double>(), pf, beta, clock.GetCurrentInstant());
    }
}
```
