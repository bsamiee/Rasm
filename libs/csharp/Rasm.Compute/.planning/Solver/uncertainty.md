# [COMPUTE_UNCERTAINTY]

Rasm.Compute solver uncertainty: one `UncertaintyMethod` `[SmartEnum<string>]` forward-UQ propagation axis (Monte-Carlo, Latin-hypercube-MC, polynomial-chaos, first-order-reliability, subset-simulation) and one `RandomVariable` `[Union]` lowering each input case onto exactly one `MathNet.Numerics.Distributions.IContinuousDistribution`, turning the deterministic `Solver/optimizer#OPTIMIZER_LANE` `evaluate` oracle into a distribution-valued `UncertaintyResult` carrying response moments, response quantiles, the Sobol total-effect vector, the failure probability `pf`, and the reliability index `β`. The page owns the `UncertaintyMethod` rows, the `RandomVariable` cases, the `UncertaintyResult` carrier, the `Propagate` state-threaded fold over the shared oracle, and the `Receipt` projection onto the settled `Runtime/receipts#RECEIPT_UNION` `Uncertainty` case; the sample draws ride the `Tensor/quadrature#OWNED_BUILDS` `LowDiscrepancy` low-discrepancy generator through inverse-transform (never a fresh `System.Random` per call), the response moments fold `MathNet.Numerics.Statistics` `Mean`/`Variance`/`Quantile`, the polynomial-chaos coefficients fit through the `Tensor/blas#DENSE_ALGEBRA` `Matrix<double>.QR` least-squares route, the Sobol total-effect ranking composes the `Solver/sweep#SWEEP_AND_BUDGET` `SensitivityTornado` variance fold rather than re-minting it, and the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, `ClockPolicy`, and the `SolverKeyPolicy` ordinal accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled. The `UncertaintyResult` is content-keyed and crosses to Persistence by reference; the `Uncertainty` receipt is the C# producer of the cross-libs `ONE_GRADUATION_EVIDENCE` `uncertainty-law` axis — the offline learned-input-distribution/PCE fit stays the Python companion's, decoded by content key, never an in-process learning loop.

## [1]-[INDEX]

- [1]-[UNCERTAINTY_LANE]: forward-UQ MC/LHS/PCE/FORM/subset propagation; Sobol variance; failure-prob β.

## [2]-[UNCERTAINTY_LANE]

- Owner: `UncertaintyMethod` `[SmartEnum<string>]` propagation-strategy rows carrying a `SampleBased` discriminant column; `RandomVariable` `[Union]` input-distribution cases each lowering to one `IContinuousDistribution` with an inverse-transform `Quantile`; `UncertaintyResult` the distribution-valued response carrier (moments + quantiles + Sobol total-effect + `pf` + `β`); `Uncertainty` the static `Propagate` fold driving the `Solver/optimizer#OPTIMIZER_LANE` `evaluate` oracle over the `LowDiscrepancy`-seeded sample design and reducing the response vectors through `Statistics`.
- Cases: `UncertaintyMethod` rows monte-carlo · latin-hypercube-mc · polynomial-chaos · first-order-reliability · subset-simulation (`SampleBased=true` for monte-carlo/latin-hypercube-mc/subset-simulation, `false` for the analytic-fit polynomial-chaos and first-order-reliability rows); `RandomVariable` cases `Normal(string Name, double Mean, double StdDev)` · `LogNormal(string Name, double Mu, double Sigma)` · `Uniform(string Name, double Lower, double Upper)` · `Weibull(string Name, double Shape, double Scale)` · `Beta(string Name, double A, double B)` · `Empirical(string Name, Seq<double> Support, Seq<double> Cdf)`.
- Entry: `public static Fin<UncertaintyResult> Propagate(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, CorrelationId correlation, ClockPolicy clocks)` — `Fin<T>` aborts on an empty input vector or a `LowDiscrepancy` dimension-bound rejection; `evaluate` is the identical `Func<DesignPoint, Fin<Seq<double>>>` oracle the optimizer drives (the full `SolveLane.Solve` or a `Surrogate.Predict`), so a UQ study never knows whether it propagated through a full FEA solve or a surrogate prediction.
- Auto: `Propagate` builds the `LowDiscrepancy.Sobol` generator over the input dimension, draws the sample design by the `UncertaintyMethod` row (Monte-Carlo draws one space-filling point per realization, Latin-hypercube-MC stratifies the unit hypercube per dimension before the low-discrepancy draw, subset-simulation conditions successive sample populations on intermediate threshold exceedances), maps each unit-cube draw through each `RandomVariable.Quantile` inverse-transform onto the physical input, calls the shared `evaluate` oracle for each realization, folds the response vectors through `Statistics.Mean`/`Variance`/`Quantile`, ranks the per-input Sobol total-effect by composing the `Solver/sweep#SWEEP_AND_BUDGET` `SensitivityTornado.Of` variance fold over the sampled response, scores the failure probability `pf` as the limit-state exceedance fraction over the realizations (or `Normal.CDF(-β)` on the analytic FORM row) and the reliability index `β` as `Normal.InvCDF(0, 1, 1 - pf)`; the polynomial-chaos row instead fits the orthogonal-polynomial response coefficients through `Matrix<double>.QR().Solve` least-squares over the sampled design and reads the moments and total-effect indices in closed form from the spectral coefficients; the state is threaded as one immutable fold accumulator `(LowDiscrepancy Gen, Seq<Seq<double>> Responses)`, never a per-method propagation loop with a mutable accumulator.
- Receipt: the `Uncertainty` `ComputeReceipt` case carries the method key, the realized sample count, the response mean and variance, the response quantile vector, the Sobol total-effect vector, the failure probability `pf`, and the reliability index `β`; the `pf`/`β`/quantile evidence makes a reliability verdict auditable, and the result body is content-keyed so its Strict-resolver round-trip is a golden-bytes fixture (`ONE_WIRE_FIXTURE_CORPUS`-eligible).
- Packages: MathNet.Numerics, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new propagation strategy is one `UncertaintyMethod` row binding its sample-design kernel; a new input distribution is one `RandomVariable` case lowering to its `IContinuousDistribution`; a new response statistic is one field on `UncertaintyResult` plus one slot on the `Uncertainty` receipt; zero new surface — a `MonteCarloRunner`/`LatinHypercubeSampler`/`PceFitter`/`FormSolver`/`SubsetSimulator` sibling family is collapsed onto one `Uncertainty` fold, a `MomentsResult`/`ReliabilityResult`/`SensitivityResult` result trio onto the one `UncertaintyResult` carrier, and a `NormalVariable`/`WeibullVariable`/`EmpiricalVariable` class family onto the one `RandomVariable` union.
- Boundary: the lane is contract-uniform with the optimizer and the sweep — the `evaluate` oracle is the single coupling point and propagation never knows whether the realization ran a full solve or a surrogate, so a parallel UQ-search path is the rejected form; each `RandomVariable` case lowers onto exactly one `MathNet.Numerics.Distributions` `IContinuousDistribution` (`Normal`/`LogNormal`/`ContinuousUniform`/`Weibull`/`Beta`) and the `Empirical` case inverse-transforms its provided CDF support through binary search, so a hand-rolled distribution sampler beside the `Distributions` surface is the deleted form; every draw rides the `Tensor/quadrature#OWNED_BUILDS` `LowDiscrepancy.Sobol` generator threaded through the fold and pushed through `IContinuousDistribution.InverseCumulativeDistribution`, never the distribution's own `Sample()` with its internal fresh `System.Random` — a per-call `System.Random` seed is the named defect because it breaks the variance-reduction guarantee and the fixture round-trip; the response moments fold `Statistics.Mean`/`Variance` and the quantile vector reads `Statistics.Quantile`, never a hand-rolled Welford or sort, because the descriptive surface ships in the admitted assembly; the Sobol total-effect ranking is the same `SensitivityTornado` variance decomposition the sweep owns (`V[E(Y|Xᵢ)]/V` between-bin over global variance) — the lane composes `SensitivityTornado.Of` over the sampled response rather than re-implementing a Sobol index loop, so the sweep's tornado and the UQ total-effect are one fold; the failure probability is `CumulativeDistribution` over the limit-state response (the sampled exceedance fraction on the MC/LHS/subset rows, the analytic `Normal.CDF(-β)` on the FORM row) and the reliability index is `Normal.InvCDF(1 - pf)`, never a hand-derived error function; the polynomial-chaos row fits through the `Tensor/blas#DENSE_ALGEBRA` `Matrix<double>.QR` least-squares route rather than a bespoke normal-equations solve, so the PCE coefficient fit shares the dense-algebra provider; the learned input distribution and the offline PCE surrogate are the Python companion's — they cross as the libs `ONE_GRADUATION_EVIDENCE` `uncertainty-law` artifact decoded by content key, an in-process distribution-learning loop is the rejected form because the fit role belongs to the offline-science branch; the `UncertaintyResult` is content-addressed onto the Persistence vector index so a dashboard queries a reliability envelope by design region, and the `Uncertainty` receipt rides the one `ComputeReceipt` union under its correlation, never a standalone `UncertaintyReceipt`.

```csharp contract
// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class UncertaintyMethod {
    public static readonly UncertaintyMethod MonteCarlo = new("monte-carlo", sampleBased: true);
    public static readonly UncertaintyMethod LatinHypercubeMc = new("latin-hypercube-mc", sampleBased: true);
    public static readonly UncertaintyMethod PolynomialChaos = new("polynomial-chaos", sampleBased: false);
    public static readonly UncertaintyMethod FirstOrderReliability = new("first-order-reliability", sampleBased: false);
    public static readonly UncertaintyMethod SubsetSimulation = new("subset-simulation", sampleBased: true);

    public bool SampleBased { get; }
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
    double SubsetLevelProbability,
    Seq<double> QuantileTaus,
    int LimitStateObjective,
    double LimitStateThreshold,
    int Seed) {
    public static readonly UncertaintyPolicy CanonicalMonteCarlo = new(
        UncertaintyMethod.MonteCarlo, Samples: 4096, PceOrder: 3, SubsetLevelProbability: 0.1,
        QuantileTaus: Seq(0.05, 0.5, 0.95), LimitStateObjective: 0, LimitStateThreshold: 0.0, Seed: 0x5DEECE66);
    public static readonly UncertaintyPolicy CanonicalLatinHypercube = CanonicalMonteCarlo with { Method = UncertaintyMethod.LatinHypercubeMc };
    public static readonly UncertaintyPolicy CanonicalReliability = CanonicalMonteCarlo with { Method = UncertaintyMethod.FirstOrderReliability, Samples = 512 };
    public static readonly UncertaintyPolicy CanonicalChaos = CanonicalMonteCarlo with { Method = UncertaintyMethod.PolynomialChaos, Samples = 1024 };
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

    static Fin<Seq<ImmutableArray<double>>> Design(Seq<RandomVariable> inputs, UncertaintyPolicy policy, LowDiscrepancy generator) {
        int count = Math.Max(2, policy.Samples), dim = inputs.Count;
        var draws = toSeq(Enumerable.Range(0, count)).Fold((Gen: generator, Points: Seq<double[]>()), static (acc, _) => {
            var (next, point) = acc.Gen.Draw();
            return (next, acc.Points.Add(point));
        }).Points;
        Seq<double[]> stratified = policy.Method == UncertaintyMethod.LatinHypercubeMc ? Stratify(draws, count, dim) : draws;
        Seq<ImmutableArray<double>> physical = stratified.Map(unit =>
            inputs.Map((variable, axis) => variable.Quantile(unit[Math.Min(axis, unit.Length - 1)])).ToImmutableArray());
        return Fin.Succ(physical);
    }

    static Seq<double[]> Stratify(Seq<double> draws, int count, int dim) {
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

    static UncertaintyResult Reduce(
        Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, Seq<Seq<double>> responses, ClockPolicy clocks) {
        double[] scalar = responses.Map(values => values.HeadOrNone().IfNone(0.0)).ToArray();
        double mean = Statistics.Mean(scalar);
        double variance = Statistics.Variance(scalar);
        Seq<double> quantiles = policy.QuantileTaus.Map(tau => Statistics.Quantile(scalar, tau));
        Seq<double> sobol = SobolTotal(inputs, policy, design, scalar);
        double pf = FailureProbability(policy, scalar, mean, variance);
        double beta = pf is > 0.0 and < 1.0 ? -MathNet.Numerics.Distributions.Normal.InvCDF(0.0, 1.0, pf) : double.PositiveInfinity;
        return new UncertaintyResult(policy.Method, scalar.Length, mean, variance, quantiles, sobol, pf, beta, clocks.Now);
    }

    static Seq<double> SobolTotal(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, double[] response) {
        var grid = new SweepGrid(
            inputs.Map(static v => (SweepAxis)new SweepAxis.Enumerated(v.VariableName, Seq<double>())),
            Seq1(ObjectiveSense.Minimize),
            SensitivityMethod.SobolVariance);
        Seq<DesignPoint> points = toSeq(Enumerable.Range(0, Math.Min(design.Count, response.Length)))
            .Map(i => new DesignPoint(design[i], [response[i]], []));
        return SensitivityTornado.Of(grid, points, 0).Bars.Map(static bar => bar.Effect);
    }

    static double FailureProbability(UncertaintyPolicy policy, double[] response, double mean, double variance) =>
        policy.Method == UncertaintyMethod.FirstOrderReliability
            ? MathNet.Numerics.Distributions.Normal.CDF(0.0, 1.0, -ReliabilityBeta(mean, variance, policy.LimitStateThreshold))
            : response.Length == 0
                ? 0.0
                : (double)response.Count(value => value > policy.LimitStateThreshold) / response.Length;

    static double ReliabilityBeta(double mean, double variance, double threshold) =>
        variance > 1e-18 ? (threshold - mean) / Math.Sqrt(variance) : (mean <= threshold ? double.PositiveInfinity : double.NegativeInfinity);
}
```
