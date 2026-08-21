# [COMPUTE_UNCERTAINTY]

Rasm.Compute solver uncertainty: one `UncertaintyMethod` forward-UQ/reliability axis carrying a keyless `UqStrategy` driver, a `SampleDesign` matrix behavior, and its own draw lane. `RandomVariable` owns inverse transforms and orthonormal recurrences; `UncertaintyResult` carries explicit optional response moments beside quantiles, the method-discriminated `SensitivityPayload`, surrogate fit calibration, failure probability, reliability index, and physical most-probable point.

Variance-reduced draws ride `LowDiscrepancy` through inverse transform; every pseudo-random draw on the page — the Monte Carlo matrix, the Morris permutation and step sign, the subset chain — comes from the kernel `Deterministic.Source` keyed on `(UncertaintyMethod.Lane, …)` under `UncertaintyPolicy.Seed`, so one seed reproduces one campaign and two rows never replay each other's stream. Response moments use `MathNet.Numerics.Statistics`; PCE coefficients use dense or sparse QR over a basis built once in the format its own route consumes; structured Saltelli and Morris designs own their estimators. Correlated inputs use one Cholesky-gated Gaussian copula. FORM/SORM uses the one `LimitState` union over exact HyperJet derivatives or typed finite differences, and subset simulation uses the Au-Beck modified Metropolis chain.

## [01]-[INDEX]

- [02]-[UNCERTAINTY_LANE]: forward-UQ MC/LHS/PCE/subset propagation; HLRF FORM + Breitung SORM; Sobol first/total + Morris screen; Gaussian-copula correlation; failure-prob β.

## [02]-[UNCERTAINTY_LANE]

- Owner: `UncertaintyMethod` `[SmartEnum<string>]` propagation-strategy rows carrying a `UqStrategy` driver discriminant, a `SampleDesign` matrix column, and the draw lane every stochastic step keys on; `RandomVariable` `[Union]` input-distribution cases each lowering to an inverse-transform `Quantile`, a `Standardize` map into its own orthogonality coordinate, and a `RecurrenceCoefficients` orthonormal-polynomial row; `RecurrenceCoefficients` the one orthonormal three-term-recurrence owner (the four Askey closed forms with the discretized-Stieltjes arbitrary-PCE construction) with its own admission; `SensitivityPayload` `[Union]` the method-discriminated sensitivity carrier; `UncertaintyResult` the distribution-valued response carrier (moments through kurtosis + quantiles + the sensitivity payload + surrogate `R²` and residual standard error + `pf` + `β` + the physical MPP); `Uncertainty` the static `UqStrategy`-dispatched (total `Switch`) `Propagate` fold driving the `Solver/optimizer#OPTIMIZER_LANE` `evaluate` oracle.
- Cases: `SampleDesign` pseudo-random · space-filling · stratified · Saltelli-AB-AB · Morris-trajectory · analytic; `UncertaintyMethod` monte-carlo · latin-hypercube-mc · polynomial-chaos · first-order-reliability · second-order-reliability · subset-simulation · sobol-saltelli · morris; `SensitivityPayload` `Sobol(First, Total)` · `Morris(MuStar, Sigma)` · `Importance(Alpha)`; `LimitState` `Oracle` · `Smooth` · `SmoothSpan`; `RandomVariable` normal · log-normal · uniform · gamma · exponential · Weibull · Gumbel · beta · triangular · empirical.
- Entry: `public static Fin<UncertaintyResult> Propagate(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock)` validates every input distribution, unique names, policy bounds, method/design compatibility, response component, supplied-limit-state shape, and correlation matrix before dispatch — ACCUMULATING, so a caller composing a campaign reads every broken column at once and each refusal names the column it broke. `Component` faults a short or non-finite response vector; no first-component or zero fallback exists, and an empty sample refuses rather than publishing `pf = 0` as measured perfect reliability.
- Auto: `Propagate` builds the optional Gaussian-copula `Transform` (identity when absent) and dispatches the `UqStrategy` driver off the `UncertaintyMethod.Strategy` row — the matrix-sampling driver draws the `LowDiscrepancy.Sobol` unit matrix, shapes it per `SampleDesign` (space-filling, LHS-stratified, the Saltelli `(2+d)·N` A/B/AB block, or the Morris `(d+1)·r` randomized-permutation trajectory grid), maps each unit row through the copula and the per-axis `Quantile`, evaluates, and reduces to the moment fold with the Saltelli/Morris payload or the composed `SensitivityTornado` first-order; the spectral-fit driver builds the orthonormal basis over the per-input `RecurrenceCoefficients` in the format its route consumes — a dense Vandermonde for thin-QR, COO triplets for the sparse-QR route — and reads mean/variance/Sobol closed-form from the coefficient masses; the reliability-search driver runs HLRF to the standard-normal MPP scoring `β`/`pf`/importance-factors, the SORM row adding the Breitung curvature correction; the subset driver conditions successive populations on intermediate thresholds through the Au-Beck sampler so a `pf~10⁻⁶` rare event resolves in `O(N·log pf)` evaluations. State threads as one immutable fold accumulator, never a per-method mutable loop.
- Receipt: `Receipt` projects the full `UncertaintyResult` onto the widened `Uncertainty` `ComputeReceipt` case — method key, realized sample/evaluation count, nullable mean/variance/skewness/kurtosis (a method that does not estimate a moment carries `null`, never `NaN` or a fabricated failure), quantiles, the three index slots the `SensitivityPayload` case fills (Sobol first and total, Morris μ* and σ, reliability importance factors — each case writing the slots it measured and leaving the rest empty), the physical MPP, the surrogate `R²` and residual standard error the spectral fit calibrates, `pf`, and `β` — under `ReceiptScope.Execution`.
- Packages: PureHDF (reached ONLY through the `Runtime/archive#HDF_ARCHIVE` `ArchiveSession` capsule — this lane declares three slots against their chunk grids and opens no container of its own), MathNet.Numerics (every `Distributions` call FULLY QUALIFIED, because six `RandomVariable` case names shadow the distribution classes inside the union's own scope), HyperJet (the exact-AD FORM/SORM gradient/Hessian leg via `SensitivityLaw`), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core (`Validation<Error,T>` the accumulating admission through the `Solver/optimizer#OPTIMIZER_LANE` `Refusal` clause, `IO` the archive rail), NodaTime, Rasm (project, `Deterministic.Source` as the one draw owner, reached directly at each draw site), Rasm.Persistence (project), BCL inbox
- Boundary: the spectral fit solves on the `Tensor/blas#DENSE_ALGEBRA` route with the `DenseSubstrate` composition selected, threaded as a POLICY VALUE — an ambient substrate cell let two compositions in one process overwrite each other's choice and stamped receipts with whatever the cell held at read time. The returned `SolveOutcome<Vector<double>>` carries the row that SERVED beside its witnessed residual and its own `SolveTermination`, so a native leg declining this operand is a fact the carrier holds rather than an assumption from the row asked for.
- Growth: a new propagation strategy is one `UncertaintyMethod` row binding its `UqStrategy` driver, `SampleDesign`, and draw lane; a new input distribution is one `RandomVariable` case lowering to its `Quantile`/`Standardize`/`Recurrence` — an Askey-family input binds a closed-form `RecurrenceCoefficients` constructor, a non-Askey input falls to the one `Stieltjes` construction with zero new surface; a new sensitivity family is one `SensitivityPayload` case with its own receipt projection arm; a new response statistic is one field on `UncertaintyResult` with one slot on the `Uncertainty` receipt; a `MonteCarloRunner`/`LatinHypercubeSampler`/`PceFitter`/`FormSolver`/`SormSolver`/`SaltelliSobol`/`MorrisScreening`/`SubsetSimulator` sibling family is collapsed onto one `UqStrategy`-dispatched (total `Switch`) `Uncertainty` fold, a `MomentsResult`/`ReliabilityResult`/`SensitivityResult` result trio onto the one `UncertaintyResult` carrier, a `NormalVariable`/`WeibullVariable`/`EmpiricalVariable` class family onto the one `RandomVariable` union, and a `HermiteBasis`/`LegendreBasis`/`LaguerreBasis`/`JacobiBasis` polynomial-evaluator family onto the one `RecurrenceCoefficients` orthonormal recurrence.
- Boundary: `evaluate` is the single solver coupling, and the sample fold is SEQUENTIAL because that contract is the bare synchronous `Fin` — a campaign wanting overlapped evaluation composes `Solver/sweep#SWEEP_AND_BUDGET`, which owns the `IO`-lifted oracle and the chunked fan-out, rather than this lane growing a second oracle shape. Variance-reduced designs use the owned `LowDiscrepancy` generator and every pseudo-random step draws from `Deterministic.Source` on the method's lane; a bare `new Random(seed)` forks replay across runtimes and has no site left.
- Boundary: correlation admission requires a finite symmetric unit-diagonal positive-definite matrix and rejects PCE/Saltelli/Morris until a generalized correlated-sensitivity estimator exists. FORM faults a degenerate gradient or iteration-cap miss. SORM counts curvature evaluations and faults invalid Breitung curvature domains instead of dropping factors. Subset simulation faults a level-cap miss and tallies EVERY oracle call — the seeded population, each chain's own seed probe, and each proposal — so the receipt's evaluation count is the cost the campaign actually paid.
- Boundary: an estimate below its sample floor is ABSENT, never zero — skewness needs three samples and kurtosis four, and a fabricated `0.0` inside a `Some` reads as a measured symmetry the campaign never established. Reliability-only results carry absent moments as `None`, not `NaN` sentinels. Fit calibration belongs to the spectral row alone — a sampling, reliability, or subset run fits no surrogate, so both calibration columns stay `None`, and an exactly-determined basis interpolates with no residual degrees of freedom, so its standard error stays `None` rather than publishing the infinite quotient.
- Boundary: PER-AXIS basis and standardization pair by construction and that pairing IS tensor-product PCE law — each axis is orthogonal with respect to its OWN measure in its OWN coordinate (Hermite over the standardized normal, Legendre over the uniform mapped to `[-1,1]`, Laguerre over the rate-scaled gamma), and the product basis is orthonormal because the joint measure factorizes. A reading that calls the per-axis coordinates "mixed" mistakes a tensor product for a change of basis; the construction is correct and a shared coordinate across axes would be the error.
- Boundary: Morris is a GRID method — each trajectory draws its own axis permutation and step sign from the method's lane, the step is the `p/(2(p−1))` grid delta, and a step leaving `[0,1]` REFLECTS to the other side of the level grid rather than clamping, because a clamped step changes the denominator the elementary effect divides by and reports a distorted sensitivity as a measured one. The screening reads each effect's axis and signed step off the DESIGN it evaluated, so the permutation needs no side channel and cannot drift from the matrix it describes.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// The matrix shapes `Design` materializes, plus `Analytic` for the rows that draw NO design matrix — the reliability
// search walks to its own MPP and the subset chain seeds its own population, so neither reaches `Design` at all.
[SmartEnum]
public sealed partial class SampleDesign {
    public static readonly SampleDesign PseudoRandom = new();
    public static readonly SampleDesign SpaceFilling = new();
    public static readonly SampleDesign Stratified = new();
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
    public static readonly UncertaintyMethod MonteCarlo = new("monte-carlo", UqStrategy.MatrixSampling, SampleDesign.PseudoRandom, lane: 1L);
    public static readonly UncertaintyMethod LatinHypercubeMc = new("latin-hypercube-mc", UqStrategy.MatrixSampling, SampleDesign.Stratified, lane: 2L);
    public static readonly UncertaintyMethod PolynomialChaos = new("polynomial-chaos", UqStrategy.SpectralFit, SampleDesign.Stratified, lane: 3L);
    public static readonly UncertaintyMethod FirstOrderReliability = new("first-order-reliability", UqStrategy.ReliabilitySearch, SampleDesign.Analytic, lane: 4L);
    public static readonly UncertaintyMethod SecondOrderReliability = new("second-order-reliability", UqStrategy.ReliabilitySearch, SampleDesign.Analytic, lane: 5L);
    public static readonly UncertaintyMethod SubsetSimulation = new("subset-simulation", UqStrategy.Subset, SampleDesign.Analytic, lane: 6L);
    public static readonly UncertaintyMethod SobolSaltelli = new("sobol-saltelli", UqStrategy.MatrixSampling, SampleDesign.SaltelliAbAb, lane: 7L);
    public static readonly UncertaintyMethod Morris = new("morris", UqStrategy.MatrixSampling, SampleDesign.MorrisTrajectory, lane: 8L);

    public UqStrategy Strategy { get; }
    public SampleDesign Design { get; }

    // Draw lane: every pseudo-random step keys the kernel `Deterministic` source on `(Lane, …)` so one policy seed
    // yields independent streams per row and a re-run of one method never replays another's draws. The column is
    // DECLARED rather than derived from the key string, so a row rename never silently re-keys a stored campaign.
    public long Lane { get; }
}

// The three-term recurrence `p̂ₖ₊₁ = ((x − Aₖ)p̂ₖ − √Bₖ p̂ₖ₋₁)/√Bₖ₊₁` for an orthoNORMAL family. `Admit` runs once per
// basis before any evaluation: a vanishing `B` divides the normalization by zero, and a degenerate measure reaches
// one through the Stieltjes construction, so the refusal is typed at the basis rather than a NaN column in the fit.
public sealed record RecurrenceCoefficients(ImmutableArray<double> A, ImmutableArray<double> B) {
    public Fin<Unit> Admit(string variable) =>
        A.Any(static value => !double.IsFinite(value)) || B.Any(static value => !double.IsFinite(value) || value <= 1e-300)
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(variable))))
            : Fin.Succ(unit);

    // Trusts `Admit`: every `B` is finite and strictly positive, so the normalizing roots are real and non-zero.
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

    // Every distribution call is FULLY QUALIFIED, and that is load-bearing rather than verbose. Six of this union's
    // own case names — `Normal`, `LogNormal`, `Gamma`, `Exponential`, `Beta`, `Triangular` — are also the names of
    // the MathNet distribution classes this body means, and inside the union's own scope the nested case wins the
    // lookup. Unqualified, `Normal.InvCDF(...)` binds to the CASE RECORD and not to the distribution at all, while
    // reading on the page exactly like the intended call. The nested case names stay: they are the right domain
    // vocabulary, and the binding — not the roster — was the defect.
    public double Quantile(double u) =>
        Switch(
            state: Math.Clamp(u, 1e-12, 1.0 - 1e-12),
            normal: static (p, v) => MathNet.Numerics.Distributions.Normal.InvCDF(v.Mean, v.StdDev, p),
            logNormal: static (p, v) => MathNet.Numerics.Distributions.LogNormal.InvCDF(v.Mu, v.Sigma, p),
            uniform: static (p, v) => v.Lower + (v.Upper - v.Lower) * p,
            gamma: static (p, v) => MathNet.Numerics.Distributions.Gamma.InvCDF(v.Shape, v.Rate, p),
            exponential: static (p, v) => MathNet.Numerics.Distributions.Exponential.InvCDF(v.Rate, p),
            weibull: static (p, v) => v.Scale * Math.Pow(-Math.Log(1.0 - p), 1.0 / v.Shape),
            gumbel: static (p, v) => v.Location - v.Scale * Math.Log(-Math.Log(p)),
            beta: static (p, v) => MathNet.Numerics.Distributions.Beta.InvCDF(v.A, v.B, p),
            triangular: static (p, v) => MathNet.Numerics.Distributions.Triangular.InvCDF(v.Lower, v.Upper, v.Mode, p),
            empirical: static (p, v) => EmpiricalQuantile(v.Support, v.Cdf, p));

    // Each case maps into the coordinate its OWN orthogonal family is defined on — the standardized normal for the
    // Hermite rows, `[-1,1]` for Legendre, the rate-scaled variate for Laguerre — so the per-axis basis and this map
    // are ONE decision. The tensor product of those per-axis families is orthonormal under the factorized joint
    // measure, which is exactly why each axis keeps its own coordinate rather than sharing a global one.
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

    static bool InvalidName(string value) => string.IsNullOrWhiteSpace(value);

    static bool InvalidEmpirical(Seq<double> support, Seq<double> cdf) =>
        support.Count < 2 || support.Count != cdf.Count || !support.ForAll(double.IsFinite) || !cdf.ForAll(double.IsFinite)
        || Enumerable.Range(1, support.Count - 1).Any(index => support[index] <= support[index - 1] || cdf[index] <= cdf[index - 1])
        || cdf[0] <= 0.0 || cdf[cdf.Count - 1] < 1.0 - 1e-12 || cdf[cdf.Count - 1] > 1.0 + 1e-12;
}

// --- [MODELS] ---------------------------------------------------------------------------

public delegate DDScalarSpan SpanLimitState(ReadOnlySpan<double> values, int order, Span<double> storage);

// One `SensitivityPayload` per method family, so a carrier can never hold a Morris σ in a column named for a Sobol
// total. Each case names its OWN measures and the receipt projection reads the case — the prior shared
// `SobolFirst`/`SobolTotal`/`Interaction` triple made every reader re-derive which method wrote which slot.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SensitivityPayload {
    private SensitivityPayload() { }

    public sealed record Sobol(Seq<double> First, Seq<double> Total) : SensitivityPayload;
    public sealed record Morris(Seq<double> MuStar, Seq<double> Sigma) : SensitivityPayload;

    // Reliability importance: αᵢ² is the variance share of input i at the MPP, a genuinely different measure from a
    // variance-based index and named for what it is.
    public sealed record Importance(Seq<double> Alpha) : SensitivityPayload;

    // The three receipt index slots, filled by the case that measured them. A case writes what it took and leaves
    // the rest empty; an empty slot is an unmeasured one, never a zero-effect claim.
    public (Seq<double> First, Seq<double> Total, Seq<double> Interaction) Slots =>
        Switch(
            sobol: static s => (s.First, s.Total, Seq<double>()),
            morris: static m => (Seq<double>(), m.MuStar, m.Sigma),
            importance: static i => (Seq<double>(), Seq<double>(), i.Alpha));
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
    // The dense execution substrate the spectral fit solves on. `Tensor/blas#DENSE_ALGEBRA` `DenseSubstrate.Select`
    // runs ONCE at composition and the chosen row threads as a VALUE, so this column is what composition overwrites
    // — the same discipline `Parallelism` follows from the CPU budget. The canonical row is the managed terminal a
    // refused native floor degrades onto anyway.
    DenseSubstrate Substrate,
    Option<Uncertainty.LimitState> SmoothLimitState) {
    public static readonly UncertaintyPolicy CanonicalMonteCarlo = new(
        UncertaintyMethod.MonteCarlo, Samples: 4096, PceOrder: 3, HyperbolicTruncation: false, MorrisLevels: 4, SubsetLevelProbability: 0.1,
        QuantileTaus: Seq(0.05, 0.5, 0.95), LimitStateObjective: 0, LimitStateThreshold: 0.0, Seed: 0x5DEECE66,
        Correlation: None, FiniteDifferenceStep: 1e-4, ReliabilityIterations: 50, ReliabilityTolerance: 1e-6, SubsetMaxLevels: 12, StieltjesNodes: 256, SparseBasisThreshold: 512,
        Substrate: DenseSubstrate.Managed, SmoothLimitState: None);
    public static readonly UncertaintyPolicy CanonicalLatinHypercube = CanonicalMonteCarlo with { Method = UncertaintyMethod.LatinHypercubeMc };
    public static readonly UncertaintyPolicy CanonicalReliability = CanonicalMonteCarlo with { Method = UncertaintyMethod.FirstOrderReliability, Samples = 512 };
    public static readonly UncertaintyPolicy CanonicalSorm = CanonicalReliability with { Method = UncertaintyMethod.SecondOrderReliability };
    public static readonly UncertaintyPolicy CanonicalChaos = CanonicalMonteCarlo with { Method = UncertaintyMethod.PolynomialChaos, Samples = 1024, HyperbolicTruncation = true };
    public static readonly UncertaintyPolicy CanonicalSaltelli = CanonicalMonteCarlo with { Method = UncertaintyMethod.SobolSaltelli };
    public static readonly UncertaintyPolicy CanonicalMorris = CanonicalMonteCarlo with { Method = UncertaintyMethod.Morris, Samples = 512 };
    public static readonly UncertaintyPolicy CanonicalSubset = CanonicalMonteCarlo with { Method = UncertaintyMethod.SubsetSimulation, Samples = 1000 };

    // Twenty-one INDEPENDENT constraints, and they accumulate. Three bool blocks OR-ed into one
    // `<uncertainty-invalid-admission>` paid for every one of those twenty-one evaluations and then reported which
    // of them broke to nobody — a caller composing a UQ campaign learned about one defect per round trip, and the
    // fault text could not even name the column. The positive-count and unit-interval families are ONE constraint
    // over a column roster, so each states once and a new bounded column joins its roster.
    Seq<(string Name, int Value)> CountColumns => Seq(
        (nameof(PceOrder), PceOrder), (nameof(ReliabilityIterations), ReliabilityIterations),
        (nameof(SubsetMaxLevels), SubsetMaxLevels), (nameof(SparseBasisThreshold), SparseBasisThreshold),
        (nameof(LimitStateObjective), LimitStateObjective + 1));

    Seq<(string Name, double Value)> PositiveColumns => Seq(
        (nameof(FiniteDifferenceStep), FiniteDifferenceStep), (nameof(ReliabilityTolerance), ReliabilityTolerance));

    public Fin<Unit> Validate(Seq<RandomVariable> inputs) =>
        (CountColumns.Map(static row => Refusal.Unless(
                row.Value > 0, ComputeArea.Solver,
                new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(row.Value))))
            + PositiveColumns.Map(static row => Refusal.Unless(
                double.IsFinite(row.Value) && row.Value > 0.0, ComputeArea.Solver,
                new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(row.Value))))
            + Seq(
                Refusal.Unless(Samples >= 2, ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(Samples, 2L))),
                Refusal.Unless(MorrisLevels >= 2, ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(MorrisLevels, 2L))),
                // The admitted band is the one the subset sampler actually runs on — the prior admission opened the
                // whole unit interval and the sampler then re-clamped, so an admitted value became a different one.
                Refusal.Unless(SubsetLevelProbability is >= 0.01 and <= 0.5, ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Interval(SubsetLevelProbability, 0.01, 0.5))),
                Refusal.Unless(!QuantileTaus.IsEmpty, ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(QuantileTaus.Count, 1L))),
                Refusal.Unless(QuantileTaus.ForAll(static tau => double.IsFinite(tau) && tau is > 0.0 and < 1.0), ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(QuantileTaus.Count))),
                Refusal.Unless(toSeq(QuantileTaus).Distinct().Count == QuantileTaus.Count, ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Unique, new ContractEvidence.Count(toSeq(QuantileTaus).Distinct().Count, QuantileTaus.Count))),
                Refusal.Unless(double.IsFinite(LimitStateThreshold), ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(LimitStateThreshold))),
                Refusal.Unless(StieltjesNodes >= PceOrder + 2, ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(StieltjesNodes, PceOrder + 2L))),
                Refusal.Unless(Method != UncertaintyMethod.SobolSaltelli || Samples % 2 == 0, ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Dimensions, new ShapeEvidence.Alignment(Samples, 2L))),
                Refusal.Unless(Method != UncertaintyMethod.Morris || Samples >= inputs.Count + 1, ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(Samples, inputs.Count + 1L))),
                Refusal.Unless(Correlation.IsNone || (Method != UncertaintyMethod.PolynomialChaos && Method != UncertaintyMethod.SobolSaltelli && Method != UncertaintyMethod.Morris), ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Key(Method.Key))),
                // A caller supplies the DIFFERENTIABLE arms alone; the `Oracle` arm is this lane's own construction
                // over the injected evaluate contract, so accepting one here would let a caller replace the oracle.
                Refusal.Unless(!SmoothLimitState.Exists(static state => !state.Differentiable), ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.None())),
                Refusal.Unless(SmoothLimitState.IsNone || Method == UncertaintyMethod.FirstOrderReliability || Method == UncertaintyMethod.SecondOrderReliability, ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Key(Method.Key))),
                Refusal.Unless(!inputs.IsEmpty, ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(inputs.Count, 1L))),
                Refusal.Unless(!inputs.Exists(static input => input.Invalid), ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())),
                Refusal.Unless(inputs.Map(static input => input.VariableName).Distinct().Count == inputs.Count, ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Unique, new ContractEvidence.Count(inputs.Map(static input => input.VariableName).Distinct().Count, inputs.Count)))))
        .Traverse(static claim => claim).As().Map(static _ => unit).ToFin();
}

public sealed record UncertaintyResult(
    UncertaintyMethod Method,
    int Samples,
    Option<double> Mean,
    Option<double> Variance,
    Option<double> Skewness,
    Option<double> Kurtosis,
    Seq<double> Quantiles,
    Option<SensitivityPayload> Sensitivity,
    Seq<double> MostProbablePoint,
    Option<double> FitQuality,
    Option<double> ResidualStandardError,
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
        Seq<RandomVariable> inputs, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock, Option<(Func<Stream> Sink, HdfArchivePolicy Policy)> archive = default) =>
        from _ in policy.Validate(inputs)
        from transform in Copula(inputs.Count, policy)
        from result in policy.Method.Strategy.Switch(
            state: (Inputs: inputs, Policy: policy, Transform: transform, Evaluate: evaluate, Clock: clock, Archive: archive),
            matrixSampling: static s => SampleAndReduce(s.Inputs, s.Policy, s.Transform, s.Evaluate, s.Clock, s.Archive),
            spectralFit: static s => Spectral(s.Inputs, s.Policy, s.Transform, s.Evaluate, s.Clock),
            reliabilitySearch: static s => Reliability(s.Inputs, s.Policy, s.Transform, s.Evaluate, s.Clock),
            subset: static s => Subset(s.Inputs, s.Policy, s.Transform, s.Evaluate, s.Clock))
        select result;

    // The three index slots come from the payload CASE, so a method that took no sensitivity leaves all three empty
    // rather than a reader guessing which of them its row was supposed to have filled.
    public static ComputeReceipt.Uncertainty Receipt(UncertaintyResult result, CorrelationId correlation, Duration elapsed) {
        (Seq<double> first, Seq<double> total, Seq<double> interaction) =
            result.Sensitivity.Map(static payload => payload.Slots).IfNone((Seq<double>(), Seq<double>(), Seq<double>()));
        return new(result.Method.Key, result.Samples,
            result.Mean.ToNullable(), result.Variance.ToNullable(), result.Skewness.ToNullable(), result.Kurtosis.ToNullable(),
            result.Quantiles, first, total, interaction, result.MostProbablePoint,
            result.FitQuality.ToNullable(), result.ResidualStandardError.ToNullable(),
            result.FailureProbability, result.ReliabilityIndex) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };
    }

    static Fin<Transform> Copula(int dim, UncertaintyPolicy policy) =>
        policy.Correlation.Match(
            None: () => Fin.Succ(new Transform(None)),
            Some: r => r.RowCount != dim || r.ColumnCount != dim
                ? Fin.Fail<Transform>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Counts(r.RowCount, r.ColumnCount, dim))))
                : !r.Enumerate().All(double.IsFinite)
                    || !Enumerable.Range(0, dim).All(axis => Math.Abs(r[axis, axis] - 1.0) <= 1e-10
                        && Enumerable.Range(0, dim).All(other => Math.Abs(r[axis, other] - r[other, axis]) <= 1e-10))
                    ? Fin.Fail<Transform>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
                    : Op.Of(name: "uncertainty.correlation-factor").Catch(() => Fin.Succ(r.Cholesky()))
                        .Bind(cholesky => double.IsFinite(cholesky.DeterminantLn)
                            ? Fin.Succ(new Transform(Some(cholesky.Factor)))
                            : Fin.Fail<Transform>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence((long)dim * dim))))));

    static Fin<double> Component(Seq<double> values, UncertaintyPolicy policy) =>
        policy.LimitStateObjective >= values.Count
            ? Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Range(
                RangeRequirement.WithinBounds,
                new ScalarEvidence.Interval(policy.LimitStateObjective, 0, values.Count - 1))))
        : !values.ForAll(double.IsFinite)
            ? Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(
                ComputeSubject.Value,
                new ScalarEvidence.Sequence(values.Count))))
            : Fin.Succ(values[policy.LimitStateObjective]);

    // --- [MATRIX_SAMPLING] ------------------------------------------------------------

    // BOTH coordinate systems survive the design: the physical rows the oracle evaluates and the unit rows the
    // screening reads its own step from. Morris effects are defined on the unit grid, so a physical-only design
    // would divide every effect by a step in the input's own units and rank a millimetre axis against a kilonewton
    // one — the ranking the method exists to produce.
    readonly record struct SampleMatrix(Seq<double[]> Unit, Seq<ImmutableArray<double>> Physical);

    // The archive session is the ONE deferred effect on an otherwise synchronous lane, so it runs at this single
    // named boundary rather than lifting the whole propagation onto `IO`: the card's own posture is a sequential
    // `Fin` fold, and a campaign wanting overlapped evaluation composes `Solver/sweep#SWEEP_AND_BUDGET`, which owns
    // the `IO`-lifted oracle. A write fault fails the propagation — the caller asked for the artifact.
    static Fin<UncertaintyResult> SampleAndReduce(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock, Option<(Func<Stream> Sink, HdfArchivePolicy Policy)> archive) =>
        Design(inputs, policy, transform)
            .Bind(design => Sample(design.Physical, policy, evaluate)
                .Bind(responses => EnsembleSeal(archive, inputs, policy, design, responses).Run()
                    .Bind(_ => Reduce(inputs, policy, design, responses, clock))));

    // Ensemble store: the whole propagation — unit block, physical block, response block — as one create-only
    // container, sample axis outermost, chunk rows on the DESIGN'S OWN block structure (the Saltelli A/B/AB half,
    // the Morris d+1 trajectory leg, else one bounded row band), so a Saltelli half-block or one trajectory reads
    // back as exactly one hyperslab and a rare-event campaign's evidence outlives its terminal scalar. Absent
    // capability, nothing writes; a write fault fails the propagation — the caller asked for the artifact.
    // The seal DECLARES three slots and writes through the ONE `Runtime/archive#HDF_ARCHIVE` session: the container
    // graph, the filter pipeline off the policy's own `Creation()`, the attribute typing, and the release were the
    // same five steps four producers on this branch each spelled for themselves, and this copy also handed a live
    // `Stream` to a library that does not own it while riding a bare `Fin` with no bracket — so a mid-write fault
    // leaked the sink. `ArchiveSession.Write` binds the release to EVERY outcome arm, `ChunkGrid` derives the
    // station-outermost grid the block band names, and the closed `ArchiveAttribute` vocabulary types what the
    // untyped `object` indexer used to box.
    static IO<Fin<Unit>> EnsembleSeal(Option<(Func<Stream> Sink, HdfArchivePolicy Policy)> archive, Seq<RandomVariable> inputs, UncertaintyPolicy policy, SampleMatrix design, Seq<Seq<double>> responses) =>
        archive.Match(
            None: () => IO.pure(Fin.Succ(unit)),
            Some: capability => {
                int rows = design.Physical.Count, dim = inputs.Count;
                int m = responses.IsEmpty ? 0 : responses[0].Count;
                // The chunk band is the DESIGN'S own block structure — the Saltelli A/B/AB half, the Morris `d+1`
                // trajectory leg, else one bounded row band — so a half-block or one trajectory reads back as
                // exactly one hyperslab instead of a stride across chunk boundaries.
                int block = policy.Method == UncertaintyMethod.SobolSaltelli ? Math.Max(1, Math.Max(2, policy.Samples) / 2)
                    : policy.Method == UncertaintyMethod.Morris ? dim + 1
                    : Math.Min(rows, 4096);
                double[] unitBlock = new double[rows * dim], physicalBlock = new double[rows * dim], responseBlock = new double[rows * m];
                for (int row = 0; row < rows; row++) {
                    design.Unit[row].AsSpan(0, Math.Min(dim, design.Unit[row].Length)).CopyTo(unitBlock.AsSpan(row * dim));
                    design.Physical[row].AsSpan()[..Math.Min(dim, design.Physical[row].Length)].CopyTo(physicalBlock.AsSpan(row * dim));
                    for (int objective = 0; objective < m; objective++) { responseBlock[row * m + objective] = responses[row][objective]; }
                }
                ChunkGrid axisGrid = ChunkGrid.Seat(fileDims: [(ulong)rows, (ulong)dim], chunks: [(uint)Math.Min(rows, block), (uint)dim]);
                ChunkGrid responseGrid = ChunkGrid.Seat(fileDims: [(ulong)rows, (ulong)Math.Max(1, m)], chunks: [(uint)Math.Min(rows, block), (uint)Math.Max(1, m)]);
                ArchiveSlot<double> unitSlot = new("unit", axisGrid);
                ArchiveSlot<double> physicalSlot = new("physical", axisGrid);
                ArchiveSlot<double> responseSlot = new("responses", responseGrid);
                return ArchiveSession.Write(
                    capability.Sink(), capability.Policy,
                    Seq<IArchiveSlot>(unitSlot, physicalSlot, responseSlot),
                    Seq(("method", (ArchiveAttribute)new ArchiveAttribute.Text(policy.Method.Key)),
                        ("samples", new ArchiveAttribute.Whole(policy.Samples)),
                        ("seed", new ArchiveAttribute.Whole(policy.Seed)),
                        ("block", new ArchiveAttribute.Whole(block))),
                    session =>
                        IO.pure(from unitCursor in session.Cursor(unitSlot)
                                from physicalCursor in session.Cursor(physicalSlot)
                                from responseCursor in session.Cursor(responseSlot)
                                from _unit in unitCursor.Write(unitBlock)
                                from _physical in physicalCursor.Write(physicalBlock)
                                from _responses in responseCursor.Write(responseBlock)
                                select unit));
            });

    static Fin<SampleMatrix> Design(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform) {
        int count = Math.Max(2, policy.Samples), dim = inputs.Count;
        Fin<Seq<double[]>> unit = policy.Method.Design.Switch(
            state: (Policy: policy, Count: count, Dim: dim),
            pseudoRandom: static s => Fin.Succ(PseudoRandomDraws(s.Dim, s.Count, s.Policy)),
            spaceFilling: static s => SobolDraws(s.Dim, s.Count, s.Policy.Seed),
            stratified: static s => LowDiscrepancy.LatinHypercube(s.Dim, s.Count, s.Policy.Seed, Scramble.DigitalShift).Map(net => toSeq(net)),
            saltelliAbAb: static s => SobolDraws(s.Dim, s.Count, s.Policy.Seed).Map(draws => Saltelli(draws, s.Count, s.Dim)),
            morrisTrajectory: static s => SobolDraws(s.Dim, s.Count, s.Policy.Seed).Map(draws => MorrisTrajectories(draws, s.Count, s.Dim, s.Policy)),
            analytic: static _ => Fin.Succ(Seq<double[]>()));
        return unit.Map(rows => new SampleMatrix(rows, rows.Map(row => transform.Physical(inputs, row))));
    }

    static Fin<Seq<double[]>> SobolDraws(int dim, int count, int seed) =>
        LowDiscrepancy.Sobol(dimensions: dim, seed: seed, Scramble.DigitalShift).Map(generator =>
            toSeq(Enumerable.Range(0, count)).Fold((Gen: generator, Points: Seq<double[]>()), static (acc, _) => {
                (LowDiscrepancy next, double[] point) = acc.Gen.Draw();
                return (next, acc.Points.Add(point));
            }).Points);

    static Seq<double[]> PseudoRandomDraws(int dim, int count, UncertaintyPolicy policy) {
        // The kernel source is reached DIRECTLY at each draw site, with the lane triple spelled where it is read.
        // A local `Source(policy, step, dim)` shim partially applied the same three lanes behind a page-local name,
        // so the one fact a reader needs at a draw — which lanes key this stream — resolved a hop away and each
        // site's own `step` disappeared into an argument list the shim owned.
        Random random = Deterministic.Source(seed: policy.Seed, lanes: [policy.Method.Lane, 0L, dim]);
        double[][] rows = [.. Enumerable.Range(0, count).Select(_ => new double[dim])];
        for (int row = 0; row < count; row++) {
            for (int axis = 0; axis < dim; axis++) { rows[row][axis] = random.NextDouble(); }
        }
        return toSeq(rows);
    }

    static Fin<Seq<Seq<double>>> Sample(Seq<ImmutableArray<double>> design, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate) =>
        design.Fold(Fin.Succ(Seq<Seq<double>>()), (acc, coordinates) =>
            acc.Bind(responses => evaluate(new DesignPoint(coordinates, [], [])).Bind(values => Component(values, policy).Map(_ => responses.Add(values)))));

    // A moment BELOW its sample floor is absent, not zero: skewness needs three samples and kurtosis four, and a
    // fabricated `0.0` inside a `Some` publishes a measured symmetry the campaign never established.
    static Fin<UncertaintyResult> Reduce(Seq<RandomVariable> inputs, UncertaintyPolicy policy, SampleMatrix design, Seq<Seq<double>> responses, IClock clock) {
        double[] qoi = [.. responses.Map(values => values[policy.LimitStateObjective])];
        // An EMPTY sample measures nothing, and `0 / 0` used to publish `pf = 0.0` — a failure probability of zero
        // is the strongest reliability claim this lane can make, and it was exactly what a campaign that evaluated
        // no point reported. The refusal is typed; the moment floors below already treat a thin sample as absent.
        if (qoi.Length == 0) { return Fin.Fail<UncertaintyResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Input))); }
        double mean = Statistics.Mean(qoi), variance = Statistics.Variance(qoi);
        Seq<double> quantiles = policy.QuantileTaus.Map(tau => Statistics.Quantile(qoi, tau));
        Option<SensitivityPayload> sensitivity =
            policy.Method == UncertaintyMethod.SobolSaltelli ? SaltelliIndices(inputs.Count, Math.Max(2, policy.Samples) / 2, qoi, variance)
            : policy.Method == UncertaintyMethod.Morris ? MorrisScreening(inputs.Count, design.Unit, qoi)
            : SobolBinned(inputs, design.Physical, qoi).Map(static first => (SensitivityPayload)new SensitivityPayload.Sobol(first, Seq<double>()));
        double pf = (double)qoi.Count(value => value > policy.LimitStateThreshold) / qoi.Length;
        double beta = pf is > 0.0 and < 1.0 ? -Normal.InvCDF(0.0, 1.0, pf) : pf <= 0.0 ? double.PositiveInfinity : double.NegativeInfinity;
        return Fin.Succ(new UncertaintyResult(policy.Method, qoi.Length, Some(mean), Some(variance), Moment(qoi, 3, Statistics.Skewness), Moment(qoi, 4, Statistics.Kurtosis),
            quantiles, sensitivity, Seq<double>(), None, None, pf, beta, clock.GetCurrentInstant()));
    }

    static Option<double> Moment(double[] qoi, int floor, Func<double[], double> estimate) =>
        qoi.Length >= floor && estimate(qoi) is var value && double.IsFinite(value) ? Some(value) : None;

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

    // Morris is a GRID method (Morris 1991, Campolongo 2007): each trajectory snaps its base point to the p-level
    // grid, walks the axes in ITS OWN random permutation, and steps by Δ = p/(2(p−1)) with a random sign — the
    // permutation and the sign are what make the elementary effects a sample of the input space rather than d
    // repetitions of one corner walk. A step leaving the unit box REFLECTS to the other side of the grid instead of
    // clamping, because a clamped step shortens the denominator the effect divides by and reports a distorted
    // sensitivity as a measured one. Both draws key the method's own lane, so a campaign replays exactly.
    static Seq<double[]> MorrisTrajectories(Seq<double[]> draws, int count, int dim, UncertaintyPolicy policy) {
        int levels = policy.MorrisLevels, paths = Math.Max(1, count / (dim + 1));
        double delta = levels / (2.0 * (levels - 1)), grid = 1.0 / (levels - 1);
        List<double[]> trajectories = [];
        for (int t = 0; t < paths; t++) {
            Random rng = Deterministic.Source(seed: policy.Seed, lanes: [policy.Method.Lane, t, dim]);
            double[] point = [.. draws.At(t % draws.Count).IfNone(() => new double[dim])
                .Select(u => Math.Round(Math.Clamp(u, 0.0, 1.0) / grid) * grid)];
            trajectories.Add((double[])point.Clone());
            foreach (int axis in Permutation(dim, rng)) {
                double[] stepped = (double[])trajectories[^1].Clone();
                double signed = rng.NextDouble() < 0.5 ? -delta : delta;
                double moved = stepped[axis] + signed;
                stepped[axis] = moved is < 0.0 or > 1.0 ? stepped[axis] - signed : moved;
                trajectories.Add(stepped);
            }
        }
        return toSeq(trajectories);
    }

    // Fisher-Yates over the lane's stream: the axis ORDER is part of the design, so it draws from the same source
    // every other stochastic step on the page does.
    static int[] Permutation(int dim, Random rng) {
        int[] order = [.. Enumerable.Range(0, dim)];
        for (int i = dim - 1; i > 0; i--) { int j = rng.Next(i + 1); (order[i], order[j]) = (order[j], order[i]); }
        return order;
    }

    static Option<SensitivityPayload> SaltelliIndices(int dim, int half, double[] y, double variance) {
        // A spreadless response supports no variance share, and a short block supports no estimator at all — both
        // report ABSENT rather than a zero vector a reader takes for measured insensitivity.
        if (y.Length < (2 + dim) * half || variance <= 1e-18 || half <= 0) { return None; }
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
        return Some((SensitivityPayload)new SensitivityPayload.Sobol(toSeq(first), toSeq(total)));
    }

    // The screening reads WHICH axis moved and BY HOW MUCH off the design it evaluated, so the per-trajectory
    // permutation and step sign need no side channel and can never drift from the matrix that produced the
    // responses. μ* is the mean |effect| (the Campolongo revision that stops cancelling a non-monotonic axis to
    // zero) and σ the effect spread that separates an interacting axis from a linear one.
    static Option<SensitivityPayload> MorrisScreening(int dim, Seq<double[]> unit, double[] y) {
        int paths = Math.Max(1, y.Length / (dim + 1));
        double[] absolute = new double[dim], sum = new double[dim], sumSquare = new double[dim];
        int[] counts = new int[dim];
        for (int t = 0; t < paths; t++) {
            int baseRow = t * (dim + 1);
            for (int leg = 0; leg < dim && baseRow + leg + 1 < y.Length && baseRow + leg + 1 < unit.Count; leg++) {
                if (Moved(unit[baseRow + leg], unit[baseRow + leg + 1]).Case is not (int axis, double step)) { continue; }
                double effect = (y[baseRow + leg + 1] - y[baseRow + leg]) / step;
                absolute[axis] += Math.Abs(effect);
                sum[axis] += effect;
                sumSquare[axis] += effect * effect;
                counts[axis]++;
            }
        }
        return counts.Any(static c => c == 0)
            ? None
            : Some((SensitivityPayload)new SensitivityPayload.Morris(
                toSeq(Enumerable.Range(0, dim).Select(axis => absolute[axis] / counts[axis])),
                toSeq(Enumerable.Range(0, dim).Select(axis => {
                    double meanEffect = sum[axis] / counts[axis];
                    return Math.Sqrt(Math.Max(0.0, sumSquare[axis] / counts[axis] - meanEffect * meanEffect));
                }))));
    }

    // Exactly ONE axis changes across a trajectory leg by construction; a leg that moved none or several is not a
    // Morris leg and contributes no effect rather than an effect attributed to a guess. Absence is an `Option`, not
    // a nullable tuple: this answer crosses back into the screening fold, and a `null` past that boundary is the
    // one absence spelling the estate does not admit.
    static Option<(int Axis, double Step)> Moved(double[] from, double[] to) {
        int axis = -1;
        double step = 0.0;
        for (int i = 0; i < from.Length && i < to.Length; i++) {
            double delta = to[i] - from[i];
            if (Math.Abs(delta) <= 1e-12) { continue; }
            if (axis >= 0) { return None; }
            (axis, step) = (i, delta);
        }
        return axis < 0 ? None : Some((axis, step));
    }

    // `Reduce` reads the index vector POSITIONALLY by input, while `SensitivityTornado.Bars` is effect-ordered and
    // omits every axis its method could not rank — so the bars join back by axis NAME and a vector short of one
    // entry per input yields the empty `Seq` this arm already gives total and interaction. Reading the sorted bars
    // by position files each index under the wrong variable, and padding a missing axis with zero publishes an
    // insensitivity the campaign never measured.
    static Option<Seq<double>> SobolBinned(Seq<RandomVariable> inputs, Seq<ImmutableArray<double>> design, double[] qoi) {
        SweepGrid grid = new(
            inputs.Map(static v => (SweepAxis)new SweepAxis.Linear(v.VariableName, 0.0, 1.0, 2)),
            Seq(ObjectiveSense.Minimize),
            SensitivityMethod.SobolVariance);
        Seq<DesignPoint> points = toSeq(Enumerable.Range(0, Math.Min(design.Count, qoi.Length)))
            .Map(i => new DesignPoint(design[i], [qoi[i]], []));
        HashMap<string, double> byAxis = SensitivityTornado.Of(grid, points, 0).Bars
            .Fold(HashMap<string, double>(), static (map, bar) => map.AddOrUpdate(bar.Axis, bar.Effect));
        Seq<double> indices = inputs.Map(v => byAxis.Find(v.VariableName)).Choose(identity);
        return indices.Count == inputs.Count ? Some(indices) : None;
    }

    // --- [SPECTRAL_FIT] ---------------------------------------------------------------

    static Fin<UncertaintyResult> Spectral(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock) =>
        Design(inputs, policy, transform)
            .Bind(design => Sample(design.Physical, policy, evaluate).Bind(responses => Fit(inputs, policy, design.Physical, responses, clock)));

    // The basis is built ONCE in the format its own route consumes: dense for thin-QR, COO triplets for sparse-QR.
    // Materializing a dense Vandermonde only to walk it into triplets costs `rows × terms` doubles for a matrix the
    // sparse route exists because it cannot hold.
    static Fin<UncertaintyResult> Fit(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, Seq<Seq<double>> responses, IClock clock) {
        double[] qoi = [.. responses.Map(values => values[policy.LimitStateObjective])];
        Seq<int[]> multiIndices = MultiIndexSet(inputs.Count, policy.PceOrder, policy.HyperbolicTruncation);
        if (qoi.Length < multiIndices.Count) { return Fin.Fail<UncertaintyResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(qoi.Length, multiIndices.Count)))); }
        Seq<RecurrenceCoefficients> bases = inputs.Map(v => v.Recurrence(policy.PceOrder, policy.StieltjesNodes));
        return inputs.Map((v, axis) => bases[axis].Admit(v.VariableName)).TraverseM(identity).As().Bind(_ => {
            // ONE generator serves the dense build, the COO build, and the calibration GEMV, so the three can never
            // describe different bases.
            double Basis(int row, int col) =>
                multiIndices[col].Select((degree, axis) => bases[axis].Evaluate(degree, inputs[axis].Standardize(design[row][axis]))).Aggregate(1.0, static (a, b) => a * b);
            Fin<Vector<double>> coefficients = policy.HyperbolicTruncation && multiIndices.Count > policy.SparseBasisThreshold
                ? SparseFit(Basis, qoi, multiIndices.Count)
                : DenseFit(Basis, qoi, multiIndices.Count, policy.Substrate);
            return coefficients.Map(c => ReadSpectral(inputs, policy, multiIndices, qoi, c, Calibration(Basis, qoi, multiIndices.Count, c), clock));
        });
    }

    // The substrate is HANDED in, never read from an ambient cell: `DenseRoute.Solve` takes the row composition
    // selected and answers a `SolveOutcome<Vector<double>>` carrying the row that actually SERVED beside the
    // witnessed residual and a total `SolveTermination`, so a native leg that declined this operand is visible in
    // the carrier rather than assumed from the row asked for.
    static Fin<Vector<double>> DenseFit(Func<int, int, double> basis, double[] qoi, int terms, DenseSubstrate substrate) {
        Matrix<double> vandermonde = Matrix<double>.Build.Dense(qoi.Length, terms, (row, col) => basis(row, col));
        Vector<double> rhs = Vector<double>.Build.DenseOfArray(qoi);
        return DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), vandermonde, rhs, TolerancePolicy.Derive(vandermonde, rhs), substrate)
            .Map(static solved => solved.Iterate);
    }

    // Fit calibration reads the surrogate the coefficients already define — one pass over the SAME basis generator
    // the solve consumed, never a second solve and never a retained matrix. An exactly-determined basis interpolates,
    // so it has no residual degrees of freedom and its standard error stays absent rather than reporting the infinity
    // the quotient would publish.
    static (Option<double> Quality, Option<double> StandardError) Calibration(
        Func<int, int, double> basis, double[] qoi, int terms, Vector<double> coefficients) {
        double[] modelled = new double[qoi.Length];
        for (int row = 0; row < qoi.Length; row++) {
            double value = 0.0;
            for (int col = 0; col < terms; col++) { value += basis(row, col) * coefficients[col]; }
            modelled[row] = value;
        }
        return (Some(GoodnessOfFit.RSquared(modelled, qoi)),
            qoi.Length > terms ? Some(GoodnessOfFit.StandardError(modelled, qoi, terms)) : None);
    }

    // COO is built DIRECTLY from the basis generator: the dropped near-zeros are exactly the entries a hyperbolic
    // truncation makes sparse, so the triplet lists carry the retained mass and no dense intermediate ever exists.
    static Fin<Vector<double>> SparseFit(Func<int, int, double> basis, double[] qoi, int terms) {
        List<int> rows = [];
        List<int> cols = [];
        List<double> values = [];
        for (int r = 0; r < qoi.Length; r++) {
            for (int c = 0; c < terms; c++) {
                double entry = basis(r, c);
                if (Math.Abs(entry) > 1e-12) { rows.Add(r); cols.Add(c); values.Add(entry); }
            }
        }
        return SparseOps.Ingest(SparseFormat.Coo, qoi.Length, terms, [.. rows], [.. cols], [.. values])
            .Bind(csr => SparseOps.Factor(csr, FactorKind.Qr, ColumnOrdering.MinimumDegreeAtA, pivotTol: 0.0, dropFloor: 1e-12))
            .Bind(op => op.Solve(qoi, double.PositiveInfinity))
            .Map(static solution => Vector<double>.Build.DenseOfArray(solution));
    }

    static UncertaintyResult ReadSpectral(
        Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<int[]> multiIndices, double[] qoi,
        Vector<double> coefficients, (Option<double> Quality, Option<double> StandardError) calibration, IClock clock) {
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
        Seq<double> quantiles = policy.QuantileTaus.Map(tau => Statistics.Quantile(qoi, tau));
        double standardDeviation = Math.Sqrt(Math.Max(0.0, variance));
        double beta = standardDeviation > 1e-12 ? (policy.LimitStateThreshold - mean) / standardDeviation : double.PositiveInfinity;
        double pf = double.IsFinite(beta) ? Normal.CDF(0.0, 1.0, -beta) : beta > 0.0 ? 0.0 : 1.0;
        // A spreadless spectral fit supports no variance share, so the payload is ABSENT rather than a zero vector.
        Option<SensitivityPayload> sensitivity = variance > 1e-18
            ? Some((SensitivityPayload)new SensitivityPayload.Sobol(
                toSeq(first.Select(m => m / variance)), toSeq(total.Select(m => m / variance))))
            : None;
        return new UncertaintyResult(policy.Method, qoi.Length, Some(mean), Some(variance), Moment(qoi, 3, Statistics.Skewness), Moment(qoi, 4, Statistics.Kurtosis),
            quantiles, sensitivity, Seq<double>(), calibration.Quality, calibration.StandardError, pf, beta, clock.GetCurrentInstant());
    }

    // Hyperbolic truncation prunes DURING the descent on the running q-norm, so a high-dimensional order-5 basis
    // never enumerates the full tensor grid it discards at the leaf — the prior leaf-only test walked `(order+1)^dim`
    // candidates to keep a set two orders of magnitude smaller, which is the whole cost the truncation exists to cut.
    static Seq<int[]> MultiIndexSet(int dim, int order, bool hyperbolic) {
        const double q = 0.5;
        double bound = Math.Pow(order, q);
        List<int[]> indices = [];
        void Recurse(int axis, int[] current, double accumulated) {
            if (axis == dim) { indices.Add((int[])current.Clone()); return; }
            for (int d = 0; d <= order; d++) {
                double next = accumulated + (hyperbolic ? Math.Pow(d, q) : d);
                // The q-norm is monotone in every degree, so a partial index already past the bound can extend to
                // nothing admissible and the whole subtree prunes here.
                if (next > (hyperbolic ? bound : order) + 1e-9) { break; }
                current[axis] = d;
                Recurse(axis + 1, current, next);
            }
            current[axis] = 0;
        }
        Recurse(0, new int[dim], 0.0);
        return toSeq(indices);
    }

    // --- [RELIABILITY_SEARCH] ---------------------------------------------------------

    sealed record MppState(double[] U, double[] Alpha, double[] Grad, double Beta, double FailureProbability, int Evaluations);
    sealed record HlrfAcc(double[] U, double GHere, double[] Grad, double G0, bool Converged, int Evals);

    static Fin<UncertaintyResult> Reliability(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, IClock clock) {
        // ONE union: a caller supplies a differentiable arm or nothing, and the black-box arm closes over the injected
        // oracle here — a parallel `SmoothLimitState` union carrying the same two cases forced every construction to
        // translate between two spellings of one concept.
        LimitState g = policy.SmoothLimitState.IfNone(() =>
            new LimitState.Oracle(u => evaluate(new DesignPoint(transform.FromU(inputs, u), [], []))
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

        // The two hyper-dual arms earn exact derivatives; the oracle arm is the lane's own black-box construction and
        // is what `UncertaintyPolicy.Validate` refuses from a caller.
        public bool Differentiable =>
            Switch(oracle: static _ => false, smooth: static _ => true, smoothSpan: static _ => true);

        public Fin<(double G, double[] Grad, int Evals)> Probe(double[] u, double step) =>
            Switch(
                state: (U: u, Step: step),
                oracle: static (state, source) => FiniteProbe(state.U, state.Step, source.G),
                smooth: static (state, source) => SensitivityLaw.Gradient(source.G, state.U)
                    .Bind(result => double.IsFinite(result.Value) && result.Gradient.All(double.IsFinite)
                        ? Fin.Succ((result.Value, result.Gradient, 1))
                        : Fin.Fail<(double, double[], int)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(state.U.Length + 1L))))),
                smoothSpan: static (state, source) => SpanProbe(source.G, state.U));

        public Fin<(Matrix<double> Hessian, int Evals)> Curvature(double[] u, double step) =>
            Switch(
                state: (U: u, Step: step),
                oracle: static (state, source) => FiniteHessian(state.U, state.Step, source.G).Map(hessian => (hessian, 1 + 2 * state.U.Length * state.U.Length)),
                smooth: static (state, source) => SensitivityLaw.Hessian(source.G, state.U)
                    .Bind(result => result.Hessian.Cast<double>().All(double.IsFinite)
                        ? Fin.Succ((Matrix<double>.Build.DenseOfArray(result.Hessian), 1))
                        : Fin.Fail<(Matrix<double>, int)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence((long)state.U.Length * state.U.Length))))),
                smoothSpan: static (state, source) => SpanCurvature(source.G, state.U));
    }

    static Fin<(double G, double[] Grad, int Evals)> SpanProbe(SpanLimitState source, double[] u) {
        Span<double> storage = stackalloc double[Kernel.GetDataLength(u.Length, order: 1)];
        DDScalarSpan result = source(u, 1, storage);
        double[] gradient = new double[u.Length];
        for (int axis = 0; axis < gradient.Length; axis++) { gradient[axis] = result.G(axis); }
        return double.IsFinite(result.Value) && gradient.All(double.IsFinite)
            ? Fin.Succ((result.Value, gradient, 1))
            : Fin.Fail<(double, double[], int)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(u.Length + 1L))));
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
            : Fin.Fail<(Matrix<double>, int)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence((long)u.Length * u.Length))));
    }

    static UncertaintyResult Assemble(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, MppState mpp, double pf, IClock clock) {
        double beta = pf is > 0.0 and < 1.0 ? -Normal.InvCDF(0.0, 1.0, pf) : mpp.Beta;
        return new UncertaintyResult(policy.Method, mpp.Evaluations, None, None, None, None,
            Seq<double>(), Some(new SensitivityPayload.Importance(toSeq(mpp.Alpha.Select(static a => a * a)))),
            toSeq(transform.FromU(inputs, mpp.U)), None, None, pf, beta, clock.GetCurrentInstant());
    }

    static Fin<MppState> Hlrf(int dim, UncertaintyPolicy policy, LimitState g) =>
        Begin(dim, policy.FiniteDifferenceStep, g).Bind(start =>
            toSeq(Enumerable.Range(0, Math.Max(1, policy.ReliabilityIterations)))
                .Fold(Fin.Succ(start), (acc, _) => acc.Bind(state => state.Converged ? Fin.Succ(state) : Step(state, policy, g)))
                .Bind(state => state.Converged
                    ? Fin.Succ(Finalize(state))
                    : Fin.Fail<MppState>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))));

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
        if (!double.IsFinite(gradNormSquared) || gradNormSquared < 1e-24) { return Fin.Fail<HlrfAcc>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))); }
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
            if (gradNorm < 1e-12) { return Fin.Fail<(double, int)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(gradNorm)))); }
            double[] a = mpp.Alpha;
            Matrix<double> projector = Matrix<double>.Build.Dense(a.Length, a.Length, (i, j) => (i == j ? 1.0 : 0.0) - a[i] * a[j]);
            Matrix<double> curvatureMatrix = projector * curvature.Hessian * projector / gradNorm;
            return DenseOps.Decompose(curvatureMatrix, FactorizationKind.Evd).Bind(factor => {
                double[] kappa = factor is Factorization.Evd evd ? [.. evd.Decomposition.EigenValues.Map(static value => value.Real)] : [];
                if (kappa.Length != a.Length) { return Fin.Fail<(double, int)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(kappa.Length, a.Length)))); }
                int drop = NearestZero(kappa);
                if (kappa.Where((_, index) => index != drop).Any(value => !double.IsFinite(value) || 1.0 + mpp.Beta * value <= 0.0))
                    return Fin.Fail<(double, int)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
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
        // `Validate` already refused a level probability outside `(0,1)`, so a second clamp to `[0.01, 0.5]` here
        // silently widened an ADMITTED value into a different campaign — the admission is the authority, and a
        // narrower band than the one it enforces belongs at the admission or nowhere.
        double p0 = policy.SubsetLevelProbability;
        int keep = Math.Max(1, (int)Math.Round(p0 * n));
        Random rng = Deterministic.Source(seed: policy.Seed, lanes: [policy.Method.Lane, 0L, dim]);
        Func<double[], Fin<double>> lsf = u => evaluate(new DesignPoint(transform.FromU(inputs, u), [], []))
            .Bind(values => Component(values, policy).Map(value => policy.LimitStateThreshold - value));
        return Population(dim, n, rng, lsf).Bind(initial =>
            toSeq(Enumerable.Range(0, policy.SubsetMaxLevels)).Fold(Fin.Succ(new SubsetAcc(initial, 1.0, false, n)),
                (acc, _) => acc.Bind(state => state.Done ? Fin.Succ(state) : Advance(state, dim, n, keep, p0, rng, lsf)))
            .Bind(state => state.Done
                ? Fin.Succ(SubsetResult(policy, state, clock))
                : Fin.Fail<UncertaintyResult>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Converged, new ContractEvidence.None())))));
    }

    static Fin<Seq<(double[] U, double Lsf)>> Population(int dim, int n, Random rng, Func<double[], Fin<double>> lsf) =>
        toSeq(Enumerable.Range(0, n)).Fold(Fin.Succ(Seq<(double[], double)>()), (acc, _) => acc.Bind(population => {
            double[] u = StandardNormal(dim, rng);
            return lsf(u).Map(value => population.Add((u, value)));
        }));

    // The tally counts EVERY oracle call the level spent — each chain's own seed re-evaluation and each proposal —
    // not the returned population size. The two differ by one per chain, and a receipt that reports the smaller
    // number under-states the cost of the exact rare-event estimate the method exists to make affordable.
    static Fin<SubsetAcc> Advance(SubsetAcc state, int dim, int n, int keep, double p0, Random rng, Func<double[], Fin<double>> lsf) {
        (double[] U, double Lsf)[] sorted = state.Population.OrderBy(static p => p.Lsf).ToArray();
        double threshold = sorted[Math.Min(keep, sorted.Length) - 1].Lsf;
        if (threshold <= 0.0) { return Fin.Succ(state with { Done = true }); }
        double[][] seeds = [.. sorted.Take(keep).Select(static p => p.U)];
        return Repopulate(seeds, dim, n, threshold, rng, lsf).Map(level =>
            state with { Population = level.Population, Probability = state.Probability * p0, Evaluations = state.Evaluations + level.Evaluations });
    }

    static Fin<(Seq<(double[] U, double Lsf)> Population, int Evaluations)> Repopulate(double[][] seeds, int dim, int n, double threshold, Random rng, Func<double[], Fin<double>> lsf) {
        int basePerChain = Math.Max(1, n / seeds.Length), remainder = Math.Max(0, n - basePerChain * seeds.Length);
        return toSeq(seeds).Fold(Fin.Succ((Index: 0, Population: Seq<(double[], double)>(), Evaluations: 0)), (acc, seed) =>
            acc.Bind(state => Chain(seed, basePerChain + (state.Index < remainder ? 1 : 0), dim, threshold, rng, lsf)
                .Map(chain => (state.Index + 1, state.Population + chain.Out, state.Evaluations + chain.Evaluations))))
            .Map(static state => (state.Population, state.Evaluations));
    }

    static Fin<(Seq<(double[] U, double Lsf)> Out, int Evaluations)> Chain(double[] seed, int steps, int dim, double threshold, Random rng, Func<double[], Fin<double>> lsf) =>
        lsf(seed).Bind(seedLsf => toSeq(Enumerable.Range(0, steps)).Fold(
            Fin.Succ((State: (U: seed, Lsf: seedLsf), Out: Seq<(double[], double)>(), Evaluations: 1)),   // the seed probe is the chain's first oracle call
            (acc, _) => acc.Bind(step => {
                double[] candidate = Propose(step.State.U, dim, rng);
                return lsf(candidate).Map(candidateLsf => {
                    (double[], double) next = candidateLsf <= threshold ? (candidate, candidateLsf) : step.State;
                    return (State: next, Out: step.Out.Add(next), Evaluations: step.Evaluations + 1);
                });
            })).Map(static result => (result.Out, result.Evaluations)));

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
            Seq<double>(), None, Seq<double>(), None, None, pf, beta, clock.GetCurrentInstant());
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
