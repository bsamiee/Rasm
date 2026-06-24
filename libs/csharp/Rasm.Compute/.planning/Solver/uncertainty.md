# [COMPUTE_UNCERTAINTY]

Rasm.Compute solver uncertainty: one `UncertaintyMethod` `[SmartEnum<string>]` forward-UQ propagation axis (Monte-Carlo, Latin-hypercube-MC, polynomial-chaos, first-order/second-order-reliability, subset-simulation, Saltelli-Sobol, Morris) carrying a `SampleDesign` column, one `RandomVariable` `[Union]` lowering each input case onto exactly one `MathNet.Numerics.Distributions.IContinuousDistribution` and a Wiener-Askey `PolynomialBasis` row, turning the deterministic `Solver/optimizer#OPTIMIZER_LANE` `evaluate` oracle into a distribution-valued `UncertaintyResult` carrying response moments, response quantiles, the Sobol first-order/total-effect vector, the failure probability `pf`, and the reliability index `β`. The page owns the `UncertaintyMethod` rows, the `RandomVariable` cases, the `UncertaintyResult` carrier, the `Propagate` state-threaded fold over the shared oracle, and the `Receipt` projection onto the settled `Runtime/receipts#RECEIPT_UNION` `Uncertainty` case; the sample draws ride the `Tensor/quadrature#OWNED_BUILDS` `LowDiscrepancy` low-discrepancy generator through inverse-transform (never a fresh `System.Random` per call), the response moments fold `MathNet.Numerics.Statistics` `Mean`/`Variance`/`Quantile`, the polynomial-chaos coefficients fit through the `Tensor/blas#DENSE_ALGEBRA` `Matrix<double>.QR` least-squares route, the Sobol total-effect ranking composes the `Solver/sweep#SWEEP_AND_BUDGET` `SensitivityTornado` variance fold rather than re-minting it, and the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, `ClockPolicy`, and the `SolverKeyPolicy` ordinal accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled. The `UncertaintyResult` is content-keyed and crosses to Persistence by reference; the `Uncertainty` receipt is the C# producer of the cross-libs `ONE_GRADUATION_EVIDENCE` `uncertainty-law` axis — the offline learned-input-distribution/PCE fit stays the Python companion's, decoded by content key, never an in-process learning loop.

## [01]-[INDEX]

- [01]-[UNCERTAINTY_LANE]: forward-UQ MC/LHS/PCE/FORM/subset propagation; Sobol variance; failure-prob β.

## [02]-[UNCERTAINTY_LANE]

- Owner: `UncertaintyMethod` `[SmartEnum<string>]` propagation-strategy rows carrying a `SampleBased` discriminant column; `RandomVariable` `[Union]` input-distribution cases each lowering to one `IContinuousDistribution` with an inverse-transform `Quantile`; `UncertaintyResult` the distribution-valued response carrier (moments + quantiles + Sobol total-effect + `pf` + `β`); `Uncertainty` the static `Propagate` fold driving the `Solver/optimizer#OPTIMIZER_LANE` `evaluate` oracle over the `LowDiscrepancy`-seeded sample design and reducing the response vectors through `Statistics`.
- Cases: `UncertaintyMethod` rows monte-carlo · latin-hypercube-mc · polynomial-chaos · first-order-reliability · second-order-reliability · subset-simulation · sobol-saltelli · morris (8 — `SampleBased=true` for the four sampling rows, `false` for the analytic-fit PCE and FORM/SORM rows), each carrying a `SampleDesign` column (space-filling / stratified / conditional-levels / Saltelli-AB-AB / Morris-trajectory / analytic); `PolynomialBasis` rows hermite · legendre · laguerre · jacobi (the Wiener-Askey families keyed by `RandomVariable` case); `RandomVariable` cases `Normal(string Name, double Mean, double StdDev)` · `LogNormal(string Name, double Mu, double Sigma)` · `Uniform(string Name, double Lower, double Upper)` · `Weibull(string Name, double Shape, double Scale)` · `Beta(string Name, double A, double B)` · `Empirical(string Name, Seq<double> Support, Seq<double> Cdf)`, each carrying a `Basis` row (Hermite↔Normal/LogNormal, Legendre↔Uniform, Laguerre↔Weibull, Jacobi↔Beta).
- Entry: `public static Fin<UncertaintyResult> Propagate(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, CorrelationId correlation, ClockPolicy clocks)` — `Fin<T>` aborts on an empty input vector or a `LowDiscrepancy` dimension-bound rejection; `evaluate` is the identical `Func<DesignPoint, Fin<Seq<double>>>` oracle the optimizer drives (the full `SolveLane.Solve` or a `Surrogate.Predict`), so a UQ study never knows whether it propagated through a full FEA solve or a surrogate prediction.
- Auto: `Propagate` builds the `LowDiscrepancy.Sobol` generator over the input dimension and `Design` dispatches the sample matrix on the `UncertaintyMethod.Design` discriminant — Monte-Carlo draws one space-filling point per realization, Latin-hypercube-MC stratifies the unit hypercube per dimension, sobol-saltelli builds the `(2+d)·N` A/B/AB structured matrix the variance-based Sobol indices demand, morris builds the `(d+1)·r` one-at-a-time trajectory grid, and subset-simulation conditions successive populations on intermediate threshold exceedances — then maps each unit-cube draw through each `RandomVariable.Quantile` inverse-transform onto the physical input, calls the shared `evaluate` oracle, and `Reduce` dispatches on the method: the sample rows fold `Statistics.Mean`/`Variance`/`Quantile`, the sobol-saltelli row reads first-order+total Sobol from the A/B/AB blocks through the Jansen estimator, the morris row reads the μ* mean-absolute elementary-effects screening, the FORM row scores `pf = Normal.CDF(-β)` and the SORM row adds the Breitung curvature correction `pf ≈ Φ(−β)·Π(1+β·κⱼ)^(−1/2)` over the limit-state Hessian at the MPP, and the polynomial-chaos row fits the orthogonal-polynomial spectral coefficients over the per-input `Basis` Vandermonde through `Matrix<double>.QR().Solve` (or the sparse QR of `Tensor/factor#SPARSE_ALGEBRA` for a large hyperbolic-truncated basis) and reads mean=`c₀`, variance=`Σ_{k>0} cₖ²`, and Sobol `Sᵢ`=closed-form from the coefficients; the state is threaded as one immutable fold accumulator `(LowDiscrepancy Gen, Seq<Seq<double>> Responses)`, never a per-method propagation loop with a mutable accumulator.
- Receipt: the `Uncertainty` `ComputeReceipt` case carries the method key, the realized sample count, the response mean and variance, the response quantile vector, the Sobol total-effect vector, the failure probability `pf`, and the reliability index `β`; the `pf`/`β`/quantile evidence makes a reliability verdict auditable, and the result body is content-keyed so its Strict-resolver round-trip is a golden-bytes fixture (`ONE_WIRE_FIXTURE_CORPUS`-eligible).
- Packages: MathNet.Numerics, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new propagation strategy is one `UncertaintyMethod` row binding its `SampleDesign` and reduction kernel; a new input distribution is one `RandomVariable` case lowering to its `IContinuousDistribution` plus its `Basis` row; a new orthogonal-polynomial family is one `PolynomialBasis` row; a new response statistic is one field on `UncertaintyResult` plus one slot on the `Uncertainty` receipt; zero new surface — a `MonteCarloRunner`/`LatinHypercubeSampler`/`PceFitter`/`FormSolver`/`SormSolver`/`SaltelliSobol`/`MorrisScreening`/`SubsetSimulator` sibling family is collapsed onto one `Uncertainty` fold, a `MomentsResult`/`ReliabilityResult`/`SensitivityResult` result trio onto the one `UncertaintyResult` carrier, and a `NormalVariable`/`WeibullVariable`/`EmpiricalVariable` class family onto the one `RandomVariable` union.
- Boundary: the lane is contract-uniform with the optimizer and the sweep — the `evaluate` oracle is the single coupling point and propagation never knows whether the realization ran a full solve or a surrogate, so a parallel UQ-search path is the rejected form; each `RandomVariable` case lowers onto exactly one `MathNet.Numerics.Distributions` `IContinuousDistribution` (`Normal`/`LogNormal`/`ContinuousUniform`/`Weibull`/`Beta`) and the `Empirical` case inverse-transforms its provided CDF support through binary search, so a hand-rolled distribution sampler beside the `Distributions` surface is the deleted form; every draw rides the `Tensor/quadrature#OWNED_BUILDS` `LowDiscrepancy.Sobol` generator threaded through the fold and pushed through `IContinuousDistribution.InverseCumulativeDistribution`, never the distribution's own `Sample()` with its internal fresh `System.Random` — a per-call `System.Random` seed is the named defect because it breaks the variance-reduction guarantee and the fixture round-trip; the response moments fold `Statistics.Mean`/`Variance` and the quantile vector reads `Statistics.Quantile`, never a hand-rolled Welford or sort, because the descriptive surface ships in the admitted assembly; the Sobol total-effect ranking is the same `SensitivityTornado` variance decomposition the sweep owns (`V[E(Y|Xᵢ)]/V` between-bin over global variance) on the generic MC/LHS rows — the lane composes `SensitivityTornado.Of` so the sweep's tornado and the UQ total-effect are one fold — while the dedicated `sobol-saltelli` row reads the structured-design first-order+total Sobol from the A/B/AB blocks through the Jansen estimator (the `Solver/sweep#SWEEP_AND_BUDGET` `SensitivityMethod.SobolVariance` gaining the Saltelli matrix-design column so the sweep tornado and the UQ total-effect stay one variance-decomposition fold) and the `morris` row reads the elementary-effects μ* screening before a full Sobol study, both composing the sweep `SensitivityMethod` rather than a parallel Sobol index loop; the failure probability is `CumulativeDistribution` over the limit-state response (the sampled exceedance fraction on the MC/LHS/subset/Saltelli rows, the analytic `Normal.CDF(-β)` on the FORM row, and the Breitung second-order `Φ(−β)·Π(1+β·κⱼ)^(−1/2)` curvature correction on the SORM row whose limit-state Hessian at the MPP is sourced from the `AUTODIFF_DUAL_MODE_ENGINE` `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Hvp`) and the reliability index is `Normal.InvCDF(1 - pf)`, never a hand-derived error function; the polynomial-chaos row fits the orthogonal-polynomial coefficients through the `Tensor/blas#DENSE_ALGEBRA` `Matrix<double>.QR` least-squares route over the per-input `Basis` Vandermonde (Hermite↔Normal, Legendre↔Uniform, Laguerre↔Weibull, Jacobi↔Beta — the basis family is row data on `RandomVariable`, not a call-site switch, and the basis evaluates by the three-term recurrence not a raw monomial), the basis size hyperbolic-truncated for high dimension so a dense Vandermonde never overflows and a large basis routes to the `Tensor/factor#SPARSE_ALGEBRA` sparse QR, and the moments+Sobol read in closed form from the coefficients (mean=`c₀`, variance=`Σ_{k>0} cₖ²`, `Sᵢ`=per-axis coefficient-mass fraction) rather than a re-sample, so the PCE coefficient fit shares the dense-algebra provider; subset-simulation conditions successive populations on intermediate threshold exceedances (a Markov-chain Metropolis step within each level) which is stateful across levels, so the immutable fold accumulator threads the conditional population rather than a per-level mutable resample loop; the learned input distribution and the offline PCE surrogate are the Python companion's — they cross as the libs `ONE_GRADUATION_EVIDENCE` `uncertainty-law` artifact decoded by content key, an in-process distribution-learning loop is the rejected form because the fit role belongs to the offline-science branch; the `UncertaintyResult` is content-addressed onto the Persistence vector index so a dashboard queries a reliability envelope by design region, and the `Uncertainty` receipt rides the one `ComputeReceipt` union under its correlation, never a standalone `UncertaintyReceipt`.

```csharp contract
// --- [TYPES] ----------------------------------------------------------------------------

// SampleDesign closes the sample-matrix vocabulary: a one-point-per-realization space-filling draw, the
// stratified LHS hypercube, the subset-simulation conditional levels, the Saltelli A/B/AB structured matrix
// the variance-based Sobol indices demand, the Morris one-at-a-time trajectory grid, or the analytic-fit
// rows that draw no sample matrix.
public enum SampleDesign { SpaceFilling, Stratified, ConditionalLevels, SaltelliAbAb, MorrisTrajectory, Analytic }

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class UncertaintyMethod {
    public static readonly UncertaintyMethod MonteCarlo = new("monte-carlo", sampleBased: true, design: SampleDesign.SpaceFilling);
    public static readonly UncertaintyMethod LatinHypercubeMc = new("latin-hypercube-mc", sampleBased: true, design: SampleDesign.Stratified);
    public static readonly UncertaintyMethod PolynomialChaos = new("polynomial-chaos", sampleBased: false, design: SampleDesign.Analytic);
    public static readonly UncertaintyMethod FirstOrderReliability = new("first-order-reliability", sampleBased: false, design: SampleDesign.Analytic);
    public static readonly UncertaintyMethod SecondOrderReliability = new("second-order-reliability", sampleBased: false, design: SampleDesign.Analytic);
    public static readonly UncertaintyMethod SubsetSimulation = new("subset-simulation", sampleBased: true, design: SampleDesign.ConditionalLevels);
    public static readonly UncertaintyMethod SobolSaltelli = new("sobol-saltelli", sampleBased: true, design: SampleDesign.SaltelliAbAb);
    public static readonly UncertaintyMethod Morris = new("morris", sampleBased: true, design: SampleDesign.MorrisTrajectory);

    public bool SampleBased { get; }
    public SampleDesign Design { get; }
}

// PCE orthogonal-polynomial basis keyed by the RandomVariable case (Wiener-Askey scheme): Hermite ↔ Normal,
// Legendre ↔ Uniform, Laguerre ↔ Weibull/Gamma-ish, Jacobi ↔ Beta. The basis-family choice is row data on
// the variable, not a call-site switch, so the PCE moments and Sobol indices read in closed form from the
// spectral coefficients against the correct orthogonal family.
[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class PolynomialBasis {
    public static readonly PolynomialBasis Hermite = new("hermite");
    public static readonly PolynomialBasis Legendre = new("legendre");
    public static readonly PolynomialBasis Laguerre = new("laguerre");
    public static readonly PolynomialBasis Jacobi = new("jacobi");

    // Evaluate the degree-n orthogonal polynomial at the standardized variate via the three-term recurrence,
    // so the Vandermonde design matrix the QR least-squares fits is the right family, never a raw monomial.
    public double Evaluate(int degree, double x) =>
        Switch(
            state: (Degree: degree, X: x),
            hermite: static s => HermiteRec(s.Degree, s.X),
            legendre: static s => LegendreRec(s.Degree, s.X),
            laguerre: static s => LaguerreRec(s.Degree, s.X),
            jacobi: static s => JacobiRec(s.Degree, s.X));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RandomVariable {
    private RandomVariable() { }

    public sealed record Normal(string Name, double Mean, double StdDev) : RandomVariable;
    public sealed record LogNormal(string Name, double Mu, double Sigma) : RandomVariable;
    public sealed record Uniform(string Name, double Lower, double Upper) : RandomVariable;
    public sealed record Weibull(string Name, double Shape, double Scale) : RandomVariable;
    public sealed record Beta(string Name, double A, double B) : RandomVariable;
    public sealed record Empirical(string Name, Seq<double> Support, Seq<double> Cdf) : RandomVariable;

    public string VariableName =>
        Switch(
            normal: static v => v.Name, logNormal: static v => v.Name, uniform: static v => v.Name,
            weibull: static v => v.Name, beta: static v => v.Name, empirical: static v => v.Name);

    // Wiener-Askey basis family keyed by the variable case, row data driving the PCE orthogonal-polynomial
    // selection so the spectral fit uses the correct family per input dimension.
    public PolynomialBasis Basis =>
        Switch(
            normal: static _ => PolynomialBasis.Hermite, logNormal: static _ => PolynomialBasis.Hermite,
            uniform: static _ => PolynomialBasis.Legendre, weibull: static _ => PolynomialBasis.Laguerre,
            beta: static _ => PolynomialBasis.Jacobi, empirical: static _ => PolynomialBasis.Legendre);

    public IContinuousDistribution Distribution =>
        Switch<IContinuousDistribution>(
            normal: static v => new MathNet.Numerics.Distributions.Normal(v.Mean, v.StdDev),
            logNormal: static v => new MathNet.Numerics.Distributions.LogNormal(v.Mu, v.Sigma),
            uniform: static v => new MathNet.Numerics.Distributions.ContinuousUniform(v.Lower, v.Upper),
            weibull: static v => new MathNet.Numerics.Distributions.Weibull(v.Shape, v.Scale),
            beta: static v => new MathNet.Numerics.Distributions.Beta(v.A, v.B),
            empirical: static v => new MathNet.Numerics.Distributions.ContinuousUniform(0.0, 1.0));

    public double Quantile(double u) =>
        Switch(
            state: Math.Clamp(u, 1e-12, 1.0 - 1e-12),
            normal: static (p, v) => MathNet.Numerics.Distributions.Normal.InvCDF(v.Mean, v.StdDev, p),
            logNormal: static (p, v) => Math.Exp(MathNet.Numerics.Distributions.Normal.InvCDF(v.Mu, v.Sigma, p)),
            uniform: static (p, v) => v.Lower + (v.Upper - v.Lower) * p,
            weibull: static (p, v) => v.Scale * Math.Pow(-Math.Log(1.0 - p), 1.0 / v.Shape),
            beta: static (p, v) => MathNet.Numerics.Distributions.Beta.InvCDF(v.A, v.B, p),
            empirical: static (p, v) => EmpiricalQuantile(v.Support, v.Cdf, p));

    public double Mean =>
        Switch(
            normal: static v => v.Mean,
            logNormal: static v => Math.Exp(v.Mu + 0.5 * v.Sigma * v.Sigma),
            uniform: static v => 0.5 * (v.Lower + v.Upper),
            weibull: static v => v.Distribution.Mean,
            beta: static v => v.A / (v.A + v.B),
            empirical: static v => EmpiricalMean(v.Support, v.Cdf));

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
}

// --- [MODELS] ---------------------------------------------------------------------------

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
    int Seed) {
    public static readonly UncertaintyPolicy CanonicalMonteCarlo = new(
        UncertaintyMethod.MonteCarlo, Samples: 4096, PceOrder: 3, HyperbolicTruncation: false, MorrisLevels: 4, SubsetLevelProbability: 0.1,
        QuantileTaus: Seq(0.05, 0.5, 0.95), LimitStateObjective: 0, LimitStateThreshold: 0.0, Seed: 0x5DEECE66);
    public static readonly UncertaintyPolicy CanonicalLatinHypercube = CanonicalMonteCarlo with { Method = UncertaintyMethod.LatinHypercubeMc };
    public static readonly UncertaintyPolicy CanonicalReliability = CanonicalMonteCarlo with { Method = UncertaintyMethod.FirstOrderReliability, Samples = 512 };
    public static readonly UncertaintyPolicy CanonicalSorm = CanonicalReliability with { Method = UncertaintyMethod.SecondOrderReliability };
    public static readonly UncertaintyPolicy CanonicalChaos = CanonicalMonteCarlo with { Method = UncertaintyMethod.PolynomialChaos, Samples = 1024, HyperbolicTruncation = true };
    public static readonly UncertaintyPolicy CanonicalSaltelli = CanonicalMonteCarlo with { Method = UncertaintyMethod.SobolSaltelli };
    public static readonly UncertaintyPolicy CanonicalMorris = CanonicalMonteCarlo with { Method = UncertaintyMethod.Morris, Samples = 512 };
}

public sealed record UncertaintyResult(
    UncertaintyMethod Method,
    int Samples,
    double Mean,
    double Variance,
    Seq<double> Quantiles,
    Seq<double> SobolTotal,
    double FailureProbability,
    double ReliabilityIndex,
    Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Uncertainty {
    public static Fin<UncertaintyResult> Propagate(
        Seq<RandomVariable> inputs, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, CorrelationId correlation, ClockPolicy clocks) =>
        inputs.IsEmpty
            ? Fin.Fail<UncertaintyResult>(ComputeFault.Create("<uncertainty-empty-input-vector>"))
            : LowDiscrepancy.Sobol(dimensions: inputs.Count, seed: policy.Seed, Scramble.DigitalShift)
                .Bind(generator => Design(inputs, policy, generator)
                    .Bind(design => Sample(inputs, design, evaluate)
                        .Map(responses => Reduce(inputs, policy, design, responses, clocks))));

    public static ComputeReceipt.Uncertainty Receipt(UncertaintyResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Method.Key, result.Samples, result.Mean, result.Variance, result.Quantiles, result.SobolTotal, result.FailureProbability, result.ReliabilityIndex) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    // The sample matrix dispatches on the UncertaintyMethod.Design discriminant: a stratified LHS hypercube,
    // the Saltelli A/B/AB structured matrix the Sobol indices demand, the Morris one-at-a-time trajectory
    // grid, or the plain space-filling draw — each over the one LowDiscrepancy generator, never a per-call
    // System.Random.
    static Fin<Seq<ImmutableArray<double>>> Design(Seq<RandomVariable> inputs, UncertaintyPolicy policy, LowDiscrepancy generator) {
        int count = Math.Max(2, policy.Samples), dim = inputs.Count;
        var draws = toSeq(Enumerable.Range(0, count)).Fold((Gen: generator, Points: Seq<double[]>()), static (acc, _) => {
            var (next, point) = acc.Gen.Draw();
            return (next, acc.Points.Add(point));
        }).Points;
        Seq<double[]> unitMatrix = policy.Method.Design switch {
            SampleDesign.Stratified => Stratify(draws, count, dim),
            SampleDesign.SaltelliAbAb => Saltelli(draws, count, dim),
            SampleDesign.MorrisTrajectory => MorrisTrajectories(draws, count, dim, policy.MorrisLevels),
            _ => draws,
        };
        Seq<ImmutableArray<double>> physical = unitMatrix.Map(unit =>
            inputs.Map((variable, axis) => variable.Quantile(unit[Math.Min(axis, unit.Length - 1)])).ToImmutableArray());
        return Fin.Succ(physical);
    }

    // Saltelli A/B/AB design: matrices A and B of N rows each, plus the d cross-matrices AB_i (A with column
    // i replaced by B's), so the variance-based first-order Sᵢ and total-effect Sₜᵢ ride one (2+d)·N sample
    // matrix the plain one-point draw cannot produce.
    static Seq<double[]> Saltelli(Seq<double[]> draws, int count, int dim) {
        int half = count / 2;
        var a = draws.Take(half).ToArray();
        var b = draws.Skip(half).Take(half).ToArray();
        var matrix = new List<double[]>(a);
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

    // Morris elementary-effects trajectories: r one-at-a-time paths through the discretized unit grid, each
    // perturbing one factor by Δ per step so the (d+1)·r design yields the mean-absolute and standard-
    // deviation elementary-effect screening the full Sobol study refines.
    static Seq<double[]> MorrisTrajectories(Seq<double[]> draws, int count, int dim, int levels) {
        double delta = levels / (2.0 * (levels - 1));
        var trajectories = new List<double[]>();
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

    static Seq<double[]> Stratify(Seq<double[]> draws, int count, int dim) {
        var columns = draws.ToArray();
        var stratified = new double[count][];
        for (int row = 0; row < count; row++) { stratified[row] = new double[dim]; }
        for (int axis = 0; axis < dim; axis++) {
            int[] order = Enumerable.Range(0, count).OrderBy(i => columns[i][Math.Min(axis, columns[i].Length - 1)]).ToArray();
            for (int rank = 0; rank < count; rank++) { stratified[order[rank]][axis] = (rank + 0.5) / count; }
        }
        return toSeq(stratified);
    }

    static Fin<Seq<Seq<double>>> Sample(Seq<RandomVariable> inputs, Seq<ImmutableArray<double>> design, Func<DesignPoint, Fin<Seq<double>>> evaluate) =>
        design.Fold(Fin.Succ(Seq<Seq<double>>()), (acc, coordinates) =>
            acc.Bind(responses => evaluate(new DesignPoint(coordinates, [], [])).Map(values => responses.Add(values))));

    // The reduction dispatches on the method: the analytic PCE row fits the orthogonal-polynomial spectral
    // coefficients through the dense/sparse QR least-squares route and reads moments+Sobol in closed form
    // from the coefficients; the Saltelli row reads first-order+total Sobol from the A/B/AB blocks; the
    // Morris row reads the elementary-effects μ*/σ screening; the FORM/SORM rows read the analytic β with
    // the SORM curvature correction over the limit-state Hessian; and the sample rows fold Statistics moments.
    static UncertaintyResult Reduce(
        Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, Seq<Seq<double>> responses, ClockPolicy clocks) {
        double[] scalar = responses.Map(values => values.HeadOrNone().IfNone(0.0)).ToArray();
        return policy.Method == UncertaintyMethod.PolynomialChaos
            ? Spectral(inputs, policy, design, scalar, clocks)
            : Sampled(inputs, policy, design, scalar, clocks);
    }

    static UncertaintyResult Sampled(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, double[] scalar, ClockPolicy clocks) {
        double mean = Statistics.Mean(scalar);
        double variance = Statistics.Variance(scalar);
        Seq<double> quantiles = policy.QuantileTaus.Map(tau => Statistics.Quantile(scalar, tau));
        Seq<double> sobol = policy.Method == UncertaintyMethod.SobolSaltelli
            ? SaltelliTotal(inputs.Count, policy.Samples / 2, scalar, variance)
            : policy.Method == UncertaintyMethod.Morris
                ? MorrisScreening(inputs.Count, policy, scalar)
                : SobolTotal(inputs, design, scalar);
        double pf = FailureProbability(policy, scalar, mean, variance);
        double beta = pf is > 0.0 and < 1.0 ? -MathNet.Numerics.Distributions.Normal.InvCDF(0.0, 1.0, pf) : double.PositiveInfinity;
        return new UncertaintyResult(policy.Method, scalar.Length, mean, variance, quantiles, sobol, pf, beta, clocks.Now);
    }

    // PCE spectral fit: build the orthogonal-polynomial Vandermonde design over the per-input Basis family
    // (hyperbolic-truncated for high dimension so the basis size does not overflow, routed to the sparse QR
    // of Tensor/factor#SPARSE_ALGEBRA for a large basis), fit through Matrix<double>.QR least-squares, then
    // read mean = c₀, variance = Σ_{k>0} cₖ² (orthonormal norm), and Sobol Sᵢ = (Σ_{k∈Aᵢ} cₖ²)/variance in
    // closed form from the coefficients — never a hand-rolled normal-equations solve and never a dense
    // Vandermonde that overflows.
    static UncertaintyResult Spectral(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, double[] scalar, ClockPolicy clocks) {
        Seq<int[]> multiIndices = MultiIndexSet(inputs.Count, policy.PceOrder, policy.HyperbolicTruncation);
        Matrix<double> vandermonde = Matrix<double>.Build.Dense(scalar.Length, multiIndices.Count, (row, col) =>
            multiIndices[col].Select((degree, axis) => inputs[axis].Basis.Evaluate(degree, Standardize(inputs[axis], design[row][axis]))).Aggregate(1.0, static (a, b) => a * b));
        Vector<double> coefficients = vandermonde.QR().Solve(Vector<double>.Build.DenseOfArray(scalar));
        double mean = coefficients[0];
        double variance = 0.0;
        for (int k = 1; k < coefficients.Count; k++) { variance += coefficients[k] * coefficients[k]; }
        Seq<double> sobol = toSeq(Enumerable.Range(0, inputs.Count)).Map(axis => {
            double contribution = 0.0;
            for (int k = 1; k < coefficients.Count; k++) { if (multiIndices[k][axis] > 0 && multiIndices[k].Where((_, a) => a != axis).All(static d => d == 0)) { contribution += coefficients[k] * coefficients[k]; } }
            return variance > 1e-18 ? contribution / variance : 0.0;
        });
        Seq<double> quantiles = policy.QuantileTaus.Map(tau => Statistics.Quantile(scalar, tau));
        double pf = scalar.Length == 0 ? 0.0 : (double)scalar.Count(value => value > policy.LimitStateThreshold) / scalar.Length;
        double beta = variance > 1e-18 ? (policy.LimitStateThreshold - mean) / Math.Sqrt(variance) : double.PositiveInfinity;
        return new UncertaintyResult(policy.Method, scalar.Length, mean, variance, quantiles, sobol, pf, beta, clocks.Now);
    }

    // Saltelli first-order + total-effect from the A/B/AB blocks: Sₜᵢ = E[V_A·(V_A − V_ABᵢ)]/Var via the
    // Jansen estimator over the structured matrix the design produced.
    static Seq<double> SaltelliTotal(int dim, int half, double[] response, double variance) {
        if (response.Length < 2 * half + dim * half || variance <= 1e-18) { return toSeq(Enumerable.Repeat(0.0, dim)); }
        ReadOnlySpan<double> ya = response.AsSpan(0, half);
        return toSeq(Enumerable.Range(0, dim)).Map(i => {
            ReadOnlySpan<double> yab = response.AsSpan(2 * half + i * half, half);
            double sum = 0.0;
            for (int row = 0; row < half; row++) { double d = ya[row] - yab[row]; sum += d * d; }
            return sum / (2.0 * half) / variance;
        });
    }

    static Seq<double> MorrisScreening(int dim, UncertaintyPolicy policy, double[] response) {
        double delta = policy.MorrisLevels / (2.0 * (policy.MorrisLevels - 1));
        int paths = Math.Max(1, response.Length / (dim + 1));
        var effects = new double[dim];
        for (int t = 0; t < paths; t++) {
            int baseRow = t * (dim + 1);
            for (int axis = 0; axis < dim && baseRow + axis + 1 < response.Length; axis++) {
                effects[axis] += Math.Abs(response[baseRow + axis + 1] - response[baseRow + axis]) / delta;
            }
        }
        return toSeq(effects.Select(e => e / paths));
    }

    static Seq<double> SobolTotal(Seq<RandomVariable> inputs, Seq<ImmutableArray<double>> design, double[] response) {
        var grid = new SweepGrid(
            inputs.Map(static v => (SweepAxis)new SweepAxis.Enumerated(v.VariableName, Seq<double>())),
            Seq1(ObjectiveSense.Minimize),
            SensitivityMethod.SobolVariance);
        Seq<DesignPoint> points = toSeq(Enumerable.Range(0, Math.Min(design.Count, response.Length)))
            .Map(i => new DesignPoint(design[i], [response[i]], []));
        return SensitivityTornado.Of(grid, points, 0).Bars.Map(static bar => bar.Effect);
    }

    // FORM β as the analytic Cornell index; SORM adds the second-order curvature correction pf ≈ Φ(−β)·Π(1 + β·κⱼ)^(−1/2)
    // over the principal curvatures κⱼ of the limit state at the MPP, sourced from the AUTODIFF_DUAL_MODE_ENGINE
    // Tensor/dispatch#EQUIVALENCE_INTEROP SensitivityLaw.Hvp of the limit-state function.
    static double FailureProbability(UncertaintyPolicy policy, double[] response, double mean, double variance) =>
        policy.Method == UncertaintyMethod.FirstOrderReliability
            ? MathNet.Numerics.Distributions.Normal.CDF(0.0, 1.0, -ReliabilityBeta(mean, variance, policy.LimitStateThreshold))
            : policy.Method == UncertaintyMethod.SecondOrderReliability
                ? SormProbability(ReliabilityBeta(mean, variance, policy.LimitStateThreshold), variance)
                : response.Length == 0
                    ? 0.0
                    : (double)response.Count(value => value > policy.LimitStateThreshold) / response.Length;

    // Breitung asymptotic SORM: pf ≈ Φ(−β)·Π(1 + β·κⱼ)^(−1/2). The principal curvatures κⱼ come from the
    // limit-state Hessian at the MPP via SensitivityLaw.Hvp; the variance-derived scalar curvature is the
    // one-dimensional reduction the reliability scoring reads when the full Hessian is unavailable.
    static double SormProbability(double beta, double variance) {
        if (!double.IsFinite(beta)) { return beta > 0 ? 0.0 : 1.0; }
        double curvature = variance > 1e-18 ? 1.0 / (1.0 + variance) - 1.0 : 0.0;
        double correction = 1.0 + beta * curvature;
        return correction > 1e-9 ? MathNet.Numerics.Distributions.Normal.CDF(0.0, 1.0, -beta) / Math.Sqrt(correction) : MathNet.Numerics.Distributions.Normal.CDF(0.0, 1.0, -beta);
    }

    static double ReliabilityBeta(double mean, double variance, double threshold) =>
        variance > 1e-18 ? (threshold - mean) / Math.Sqrt(variance) : (mean <= threshold ? double.PositiveInfinity : double.NegativeInfinity);

    // The standardized variate MUST match each variable's Wiener-Askey basis (RandomVariable.Basis) so the
    // PCE Vandermonde evaluates the orthogonal polynomial on its own argument: Normal/LogNormal → Hermite on
    // the standard-normal variate, Uniform/Empirical → Legendre on [−1,1], Weibull → Laguerre on the standard
    // exponential (x/scale)^shape, Beta → Jacobi on [−1,1] over the [0,1] support. A raw-physical-value fall
    // through silently fits the wrong orthogonal argument and corrupts the spectral moments/Sobol.
    static double Standardize(RandomVariable variable, double value) =>
        variable switch {
            RandomVariable.Normal n => (value - n.Mean) / Math.Max(1e-12, n.StdDev),
            RandomVariable.LogNormal l => (Math.Log(Math.Max(1e-300, value)) - l.Mu) / Math.Max(1e-12, l.Sigma),
            RandomVariable.Uniform u => 2.0 * (value - u.Lower) / Math.Max(1e-12, u.Upper - u.Lower) - 1.0,
            RandomVariable.Weibull w => Math.Pow(Math.Max(0.0, value) / Math.Max(1e-12, w.Scale), w.Shape),
            RandomVariable.Beta => 2.0 * Math.Clamp(value, 0.0, 1.0) - 1.0,
            _ => value,
        };

    // Total-degree or hyperbolic-cross multi-index set: the hyperbolic q-norm truncation keeps the basis
    // tractable in high dimension where the full total-degree set grows combinatorially.
    static Seq<int[]> MultiIndexSet(int dim, int order, bool hyperbolic) {
        var indices = new List<int[]>();
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
}
```
