# [COMPUTE_UNCERTAINTY]

Rasm.Compute solver uncertainty: one `UncertaintyMethod` `[SmartEnum<string>]` forward-UQ/reliability axis (Monte-Carlo, Latin-hypercube-MC, polynomial-chaos, first-order/second-order-reliability, subset-simulation, Saltelli-Sobol, Morris) carrying a `UqStrategy` driver discriminant and a `SampleDesign` matrix column, one `RandomVariable` `[Union]` lowering each input case onto its inverse-transform `Quantile` and an orthonormal `RecurrenceCoefficients` row, turning the deterministic `Solver/optimizer#OPTIMIZER_LANE` `evaluate` oracle into a distribution-valued `UncertaintyResult` carrying response moments through the fourth cumulant, response quantiles, the first-order and total-effect Sobol vectors, the Morris interaction screen, the failure probability `pf`, the reliability index `β`, and the physical-space most-probable point. The page owns the `UncertaintyMethod`/`UqStrategy` vocabulary, the `PolynomialBasis` Wiener-Askey rows, the `RandomVariable` cases with the one `RecurrenceCoefficients` orthonormal-polynomial owner, the `UncertaintyResult` carrier, the `UqStrategy`-dispatched `Propagate` total `Switch` over the shared oracle, and the `Receipt` projection onto the settled `Runtime/receipts#RECEIPT_UNION` `Uncertainty` case; the sample draws ride the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy` low-discrepancy generator through inverse-transform (never a fresh per-call `System.Random` in the variance-reduced lanes), the response moments fold `MathNet.Numerics.Statistics` `Mean`/`Variance`/`Skewness`/`Kurtosis`/`Quantile`, the polynomial-chaos coefficients fit through the `Tensor/blas#DENSE_ALGEBRA` `DenseRoute.Solve` thin-QR route (or the `Tensor/factor#SPARSE_SOLVE` sparse-QR route for a large hyperbolic-cross basis), the Sobol total-effect ranking composes the `Solver/sweep#SWEEP_AND_BUDGET` `SensitivityTornado` variance fold for the generic rows and the structured Saltelli-A/B/AB estimators for the dedicated row, and the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, `ClockPolicy`, and the `ComparerAccessors.StringOrdinal` accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled. Correlated inputs ride a Gaussian-copula `Transform` whose standard-normal correlation factor is the `Cholesky<double>` of the policy correlation matrix; the FORM/SORM most-probable point is found by the HLRF iteration in standard-normal space with a `LimitState` dual-source gradient — the exact hyperdual arm (`SensitivityLaw.Gradient`/`Hessian` over a `SmoothLimitState`-authored g, one evaluation) beside the honest finite-difference arm for the black-box oracle, the SORM curvature correction reading the principal curvatures off the `Tensor/blas#DENSE_ALGEBRA` `Evd` of the limit-state Hessian at the design point, and subset-simulation conditions successive populations on intermediate thresholds through the Au-Beck Modified Metropolis sampler. In-place numeric kernels — the HLRF MPP iteration, the finite-difference Hessian, the discretized-Stieltjes recurrence walk, the orthonormal three-term evaluation, and the Saltelli/Morris/subset design folds — are this page's statement exemption. The `UncertaintyResult` is content-keyed and crosses to Persistence by reference; the `Uncertainty` receipt is the C# producer of the cross-libs `ONE_GRADUATION_EVIDENCE` `uncertainty-law` axis — the offline learned-input-distribution/PCE fit stays the Python companion's, decoded by content key, never an in-process learning loop.

## [01]-[INDEX]

- [01]-[UNCERTAINTY_LANE]: forward-UQ MC/LHS/PCE/subset propagation; HLRF FORM + Breitung SORM; Sobol first/total + Morris screen; Gaussian-copula correlation; failure-prob β.

## [02]-[UNCERTAINTY_LANE]

- Owner: `UncertaintyMethod` `[SmartEnum<string>]` propagation-strategy rows carrying a `UqStrategy` driver discriminant and a `SampleDesign` matrix column; `RandomVariable` `[Union]` input-distribution cases each lowering to an inverse-transform `Quantile`, a closed-form `Mean`, a `Standardize` map, a `PolynomialBasis` Wiener-Askey label, and a `RecurrenceCoefficients` orthonormal-polynomial row; `RecurrenceCoefficients` the one orthonormal three-term-recurrence owner (the four Askey closed forms plus the discretized-Stieltjes arbitrary-PCE construction); `UncertaintyResult` the distribution-valued response carrier (moments through kurtosis + quantiles + first/total Sobol + Morris interaction + `pf` + `β` + the physical MPP); `Uncertainty` the static `UqStrategy`-dispatched (total `Switch`) `Propagate` fold driving the `Solver/optimizer#OPTIMIZER_LANE` `evaluate` oracle.
- Cases: `UncertaintyMethod` rows monte-carlo · latin-hypercube-mc · polynomial-chaos · first-order-reliability · second-order-reliability · subset-simulation · sobol-saltelli · morris (8), each binding one of four `UqStrategy` drivers (matrix-sampling / spectral-fit / reliability-search / subset) and one `SampleDesign` (space-filling / stratified / conditional-levels / Saltelli-AB-AB / Morris-trajectory / analytic); `PolynomialBasis` rows hermite · legendre · laguerre · jacobi · arbitrary (the Wiener-Askey families plus the data-driven arbitrary-PCE label, `Askey=false` only for arbitrary); `RandomVariable` cases `Normal(Name, Mean, StdDev)` · `LogNormal(Name, Mu, Sigma)` · `Uniform(Name, Lower, Upper)` · `Gamma(Name, Shape, Rate)` · `Exponential(Name, Rate)` · `Weibull(Name, Shape, Scale)` · `Gumbel(Name, Location, Scale)` · `Beta(Name, A, B)` · `Triangular(Name, Lower, Upper, Mode)` · `Empirical(Name, Support, Cdf)` (10), each mapping to its Askey basis (Hermite↔Normal/LogNormal, Legendre↔Uniform, Laguerre↔Gamma/Exponential, Jacobi↔Beta) or the arbitrary basis (Weibull/Gumbel/Triangular/Empirical, where no classical orthogonal family exists).
- Entry: `public static Fin<UncertaintyResult> Propagate(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, CorrelationId correlation, ClockPolicy clocks)` — `Fin<T>` aborts on an empty input vector, a non-positive-definite or mis-shaped correlation matrix, a `LowDiscrepancy` dimension-bound rejection, or a divergent QR/Evd; `evaluate` is the identical `Func<DesignPoint, Fin<Seq<double>>>` oracle the optimizer drives (the full `SolveLane.Solve` or a `Surrogate.Predict`), so a UQ study never knows whether it propagated through a full FEA solve or a surrogate prediction, and the analyzed scalar is the `policy.LimitStateObjective` response component, never silently the first.
- Auto: `Propagate` builds the optional Gaussian-copula `Transform` (the `Cholesky<double>` factor of `policy.Correlation` over the standard-normal space, identity when absent) and dispatches the `UqStrategy` driver off the `UncertaintyMethod.Strategy` row: the matrix-sampling driver draws the `LowDiscrepancy.Sobol` unit matrix, shapes it per `SampleDesign` (Monte-Carlo one space-filling point per realization, Latin-hypercube ranks the columns into a stratified hypercube, sobol-saltelli builds the `(2+d)·N` A/B/AB structured matrix, morris builds the `(d+1)·r` one-at-a-time trajectory grid), maps each unit row through the copula and the per-axis `Quantile` onto the physical input, calls `evaluate`, and reduces — the sample rows fold `Statistics.Mean`/`Variance`/`Skewness`/`Kurtosis`/`Quantile`, the sobol-saltelli row reads first-order (Saltelli-2010) and total-effect (Jansen) from the A/B/AB blocks, the morris row reads the μ* mean-absolute and σ standard-deviation elementary-effects screen, and the generic rows read the first-order main effect through the composed `SensitivityTornado`; the spectral-fit driver fits the orthonormal-polynomial Vandermonde over the per-input `RecurrenceCoefficients` through the dense thin-QR route (the sparse-QR route for a large hyperbolic-cross basis) and reads mean=`c₀`, variance=`Σ_{k>0} cₖ²`, and Sobol first/total in closed form from the coefficient masses; the reliability-search driver runs the HLRF iteration to the most-probable point `u*` in standard-normal space (finite-difference limit-state gradient over `evaluate`), scoring `β=±|u*|`, `pf=Φ(−β)`, and the FORM importance factors `αᵢ²`, with the SORM row adding the Breitung curvature correction `pf≈Φ(−β)·Π(1+β·κⱼ)^(−1/2)` whose principal curvatures `κⱼ` are the `Evd` eigenvalues of the tangent-projected limit-state Hessian at `u*`; the subset driver conditions successive populations on intermediate thresholds through the Au-Beck Modified Metropolis sampler so a `pf~10⁻⁶` rare event resolves in `O(N·log pf)` evaluations rather than the `O(1/pf)` a flat Monte-Carlo demands; the state threads as one immutable fold accumulator, never a per-method propagation loop with a mutable accumulator.
- Receipt: the `Uncertainty` `ComputeReceipt` case carries the method key, the realized sample/evaluation count, the response mean and variance, the response quantile vector, the total-effect Sobol vector (or the FORM importance factors / Morris μ* on the reliability/screening rows), the failure probability `pf`, and the reliability index `β`; the richer evidence (first-order Sobol, skewness/kurtosis, Morris interaction σ, the physical MPP) rides the content-keyed `UncertaintyResult` body, so a `pf`/`β`/quantile reliability verdict is auditable and the result round-trips as a golden-bytes fixture (`ONE_WIRE_FIXTURE_CORPUS`-eligible).
- Packages: MathNet.Numerics, HyperJet (the exact-AD FORM/SORM gradient/Hessian leg via `SensitivityLaw`), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new propagation strategy is one `UncertaintyMethod` row binding its `UqStrategy` driver and `SampleDesign`; a new input distribution is one `RandomVariable` case lowering to its `Quantile`/`Mean`/`Standardize`/`Basis`/`Recurrence` — an Askey-family input binds a closed-form `RecurrenceCoefficients` constructor, a non-Askey input falls to the one `Stieltjes` construction with zero new surface; a new response statistic is one field on `UncertaintyResult` plus one slot on the `Uncertainty` receipt; a `MonteCarloRunner`/`LatinHypercubeSampler`/`PceFitter`/`FormSolver`/`SormSolver`/`SaltelliSobol`/`MorrisScreening`/`SubsetSimulator` sibling family is collapsed onto one `UqStrategy`-dispatched (total `Switch`) `Uncertainty` fold, a `MomentsResult`/`ReliabilityResult`/`SensitivityResult` result trio onto the one `UncertaintyResult` carrier, a `NormalVariable`/`WeibullVariable`/`EmpiricalVariable` class family onto the one `RandomVariable` union, and a `HermiteBasis`/`LegendreBasis`/`LaguerreBasis`/`JacobiBasis` polynomial-evaluator family onto the one `RecurrenceCoefficients` orthonormal recurrence.
- Boundary: the lane is contract-uniform with the optimizer and the sweep — the `evaluate` oracle is the single coupling point and propagation never knows whether the realization ran a full solve or a surrogate, so a parallel UQ-search path is the rejected form; each `RandomVariable` case lowers onto exactly one inverse-CDF (`Normal`/`LogNormal`/`Gamma`/`Exponential`/`Beta`/`Triangular` through the `MathNet.Numerics.Distributions` static `InvCDF` family, `Weibull`/`Gumbel` through their closed-form quantiles, `Empirical` through a binary search of its provided CDF support), so a hand-rolled distribution sampler beside the `Distributions` surface is the deleted form; every variance-reduced draw rides the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy.Sobol` generator threaded through the fold and pushed through the per-axis `Quantile`, never the distribution's own `Sample()` with its internal fresh `System.Random` — a per-call `System.Random` seed in the QMC lanes is the named defect because it breaks the variance-reduction guarantee and the fixture round-trip, while the subset MCMC chain draws a single explicitly-seeded `Random(policy.Seed)` so the conditional walk is replayable; the response moments fold `Statistics.Mean`/`Variance`/`Skewness`/`Kurtosis` and the quantile vector reads `Statistics.Quantile`, never a hand-rolled Welford or sort, because the descriptive surface ships in the admitted assembly; the generic MC/LHS first-order index is the same `SensitivityTornado` between-bin variance decomposition the sweep owns (`V[E(Y|Xᵢ)]/V` over global variance) — the lane composes `SensitivityTornado.Of` so the sweep's tornado and the UQ first-order are one fold, while the dedicated `sobol-saltelli` row reads first-order (Saltelli-2010 `E[Y_B·(Y_ABᵢ−Y_A)]/V`) and total-effect (Jansen `E[(Y_A−Y_ABᵢ)²]/2V`) from the structured A/B/AB design that the binned estimator cannot separate, and the `morris` row reads the elementary-effects μ*/σ screen before a full Sobol study, both composing the sweep `SensitivityMethod` rather than a parallel Sobol loop; the failure probability is the sampled exceedance fraction on the MC/LHS rows, the analytic `Φ(−β)` on the FORM row, the Breitung curvature-corrected `Φ(−β)·Π(1+β·κⱼ)^(−1/2)` on the SORM row whose principal curvatures are the tangent-space `Evd` eigenvalues of the finite-difference limit-state Hessian at the MPP, and the conditional-product `p₀^(m−1)·P(F_m|F_{m−1})` on the subset row — the FORM/SORM gradient and Hessian come from finite differences over the same black-box oracle ONLY where the limit state is a genuine black box — a hyperdual-authored `SmoothLimitState` reads the EXACT `SensitivityLaw.Gradient`/`Hessian` leg instead (the absent-tape fall now bounded to the honest oracle arm; the fall, exactly as the optimizer's finite-difference gradient when no `MeshAdjointSnapshot` is supplied), never a fabricated mean-value Cornell index dressed as FORM nor a variance-derived scalar dressed as a Hessian curvature; the polynomial-chaos row fits the orthonormal-polynomial coefficients through the owned `Tensor/blas#DENSE_ALGEBRA` `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR least-squares route over the per-input `RecurrenceCoefficients` Vandermonde — the basis family is row data on `RandomVariable` (the correct Wiener-Askey correspondence: Gamma/Exponential→Laguerre, Beta→Jacobi, and Weibull/Gumbel/Triangular/Empirical→the discretized-Stieltjes arbitrary-PCE basis, never the mathematically-wrong Weibull→Laguerre Askey assignment) and the polynomial evaluates by the one orthonormal three-term recurrence, not a per-family kernel and not a raw monomial — the basis hyperbolic-truncated for high dimension and a large basis routed to the `Tensor/factor#SPARSE_SOLVE` `FactoredOp` sparse-QR rather than a dense Vandermonde that overflows, and the moments+Sobol read in closed form from the coefficients (mean=`c₀`, variance=`Σ_{k>0} cₖ²`, first-order `Sᵢ`=single-axis coefficient mass, total `Sₜᵢ`=any-axis-nonzero mass) rather than a re-sample, so the PCE fit shares the dense/sparse provider and a hand-rolled normal-equations solve beside `DenseRoute` is the deleted form (PCE and the variance-based Saltelli/Morris sensitivity rows assume independent inputs — the Gaussian copula serves the moment, quantile, and reliability lanes, and correlated variance-based sensitivity requires the generalized Kucherenko indices, the noted out-of-scope refinement); correlated inputs ride one Gaussian-copula `Transform` whose standard-normal correlation factor is `policy.Correlation.Cholesky().Factor` (validated positive-definite through `double.IsFinite(DeterminantLn)`, faulting on a mis-shaped or indefinite matrix), applied uniformly across the sampling, reliability, and subset lanes so a correlated reliability problem maps `u→z=L·u→x=Quantileᵢ(Φ(zᵢ))` with one transform owner, never a per-lane correlation path; subset-simulation conditions successive populations on intermediate threshold exceedances through the Au-Beck per-coordinate Modified Metropolis sampler (the standard-normal acceptance ratio `exp(½(uᵢ²−u'ᵢ²))` then the failure-region indicator), stateful across levels, so the immutable fold accumulator threads the conditional population and the accumulated `p₀` product rather than a per-level mutable resample loop; the learned input distribution and the offline PCE surrogate are the Python companion's — they cross as the libs `ONE_GRADUATION_EVIDENCE` `uncertainty-law` artifact decoded by content key, an in-process distribution-learning loop is the rejected form because the fit role belongs to the offline-science branch; the `UncertaintyResult` is content-addressed onto the Persistence vector index so a dashboard queries a reliability envelope by design region, and the `Uncertainty` receipt rides the one `ComputeReceipt` union under its correlation, never a standalone `UncertaintyReceipt`.

```csharp contract
// --- [TYPES] ----------------------------------------------------------------------------

// SampleDesign closes the sample-matrix vocabulary: a one-point-per-realization space-filling draw, the
// stratified LHS hypercube, the subset-simulation conditional levels, the Saltelli A/B/AB structured matrix
// the variance-based Sobol indices demand, the Morris one-at-a-time trajectory grid, or the analytic-search
// rows that draw no pre-built matrix.
public enum SampleDesign { SpaceFilling, Stratified, ConditionalLevels, SaltelliAbAb, MorrisTrajectory, Analytic }

// UqStrategy is the driver discriminant carried as row data on the method: the matrix-sampling driver folds a
// pre-drawn design, the spectral-fit driver regresses an orthonormal-polynomial surrogate, the reliability
// driver runs the HLRF most-probable-point search, and the subset driver runs the level-conditioned MCMC.
// The four drivers exhaust the propagation shapes; a method row selects one rather than re-spelling a runner.
public enum UqStrategy { MatrixSampling, SpectralFit, ReliabilitySearch, Subset }

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UncertaintyMethod {
    public static readonly UncertaintyMethod MonteCarlo = new("monte-carlo", UqStrategy.MatrixSampling, SampleDesign.SpaceFilling);
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

// PolynomialBasis is the Wiener-Askey correspondence vocabulary keyed onto each RandomVariable case: Hermite ↔
// Normal/LogNormal, Legendre ↔ Uniform, Laguerre ↔ Gamma/Exponential, Jacobi ↔ Beta — the four classical
// families carrying Askey=true closed-form recurrence coefficients, and Arbitrary (Askey=false) the
// data-driven discretized-Stieltjes basis every non-Askey distribution (Weibull/Gumbel/Triangular/Empirical)
// resolves to. The label is evidence; the coefficients live on RecurrenceCoefficients.
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

// The one orthonormal-polynomial owner: monic three-term-recurrence coefficients (A_k, B_k) over a
// prob-normalized measure (B_0 = 1), evaluated through the single orthonormal recurrence
// p̂_{k+1} = ((x − A_k)·p̂_k − √B_k·p̂_{k−1}) / √B_{k+1}, p̂_0 = 1. The four Askey families ship closed-form
// coefficient constructors; every non-Askey measure builds its coefficients through the discretized-Stieltjes
// procedure over equiprobable quantile nodes — one evaluator and one type collapse the per-family polynomial
// kernels and admit arbitrary-PCE without a wrong Askey assignment.
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

    // Generalized Laguerre over x^α·e^{−x} (Gamma shape−1 / Exponential 0): monic A_k = 2k+α+1, B_k = k(k+α).
    public static RecurrenceCoefficients Laguerre(int order, double alpha) =>
        Closed(order, k => 2.0 * k + alpha + 1.0, k => k == 0 ? 1.0 : k * (k + alpha));

    // Jacobi over (1−x)^α·(1+x)^β on [−1,1] (Beta b−1, a−1): the standard monic recurrence coefficients,
    // guarded at the k=0 degeneracy where the (α+β)=0 denominator vanishes.
    public static RecurrenceCoefficients Jacobi(int order, double alpha, double beta) =>
        Closed(order,
            k => (2.0 * k + alpha + beta) * (2.0 * k + alpha + beta + 2.0) is var t && Math.Abs(t) < 1e-15 ? (beta - alpha) / 2.0 : (beta * beta - alpha * alpha) / t,
            k => k == 0
                ? 1.0
                : 4.0 * k * (k + alpha) * (k + beta) * (k + alpha + beta)
                    / ((2.0 * k + alpha + beta) * (2.0 * k + alpha + beta) * (2.0 * k + alpha + beta + 1.0) * (2.0 * k + alpha + beta - 1.0)));

    // Discretized Stieltjes: equiprobable quantile nodes x_j = quantile((j+½)/m) carry the empirical measure;
    // the monic recurrence A_k = ⟨x·p_k,p_k⟩/⟨p_k,p_k⟩, B_k = ⟨p_k,p_k⟩/⟨p_{k−1},p_{k−1}⟩ builds the orthogonal
    // family for ANY distribution where no classical Askey family applies — the arbitrary-PCE construction.
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

    // Wiener-Askey family keyed by the variable case: the four classical families for the measures that admit
    // a closed-form orthogonal polynomial, Arbitrary for every other measure (resolved through Stieltjes).
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

    // Map the physical value onto the orthogonal-polynomial argument: the standard-normal variate for
    // Hermite, [−1,1] for Legendre/Jacobi, the rate-scaled exponential argument for Laguerre, and the physical
    // value unchanged for the arbitrary basis (the Stieltjes coefficients are built for the physical measure).
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

    // The orthonormal recurrence row: the Askey cases bind the closed-form coefficients (parameterized by the
    // measure's shape — Laguerre by Gamma shape−1, Jacobi by Beta b−1/a−1), every non-Askey case falls to the
    // one discretized-Stieltjes construction over its own quantile nodes. One owner, no per-family evaluator.
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
    int Seed,
    Option<Matrix<double>> Correlation,
    double FiniteDifferenceStep,
    int ReliabilityIterations,
    double ReliabilityTolerance,
    int StieltjesNodes,
    int SparseBasisThreshold,
    // The exact-AD FORM/SORM gradient source: a hyperdual-authored limit state g(u) in standard-normal
    // space. Present, the reliability descent reads exact SensitivityLaw.Gradient/Hessian; absent, the
    // honest finite-difference probe over the black-box oracle stands — never a silent wrong gradient.
    Option<Func<DDScalar[], DDScalar>> SmoothLimitState = default) {
    public static readonly UncertaintyPolicy CanonicalMonteCarlo = new(
        UncertaintyMethod.MonteCarlo, Samples: 4096, PceOrder: 3, HyperbolicTruncation: false, MorrisLevels: 4, SubsetLevelProbability: 0.1,
        QuantileTaus: Seq(0.05, 0.5, 0.95), LimitStateObjective: 0, LimitStateThreshold: 0.0, Seed: 0x5DEECE66,
        Correlation: None, FiniteDifferenceStep: 1e-4, ReliabilityIterations: 50, ReliabilityTolerance: 1e-6, StieltjesNodes: 256, SparseBasisThreshold: 512);
    public static readonly UncertaintyPolicy CanonicalLatinHypercube = CanonicalMonteCarlo with { Method = UncertaintyMethod.LatinHypercubeMc };
    public static readonly UncertaintyPolicy CanonicalReliability = CanonicalMonteCarlo with { Method = UncertaintyMethod.FirstOrderReliability, Samples = 512 };
    public static readonly UncertaintyPolicy CanonicalSorm = CanonicalReliability with { Method = UncertaintyMethod.SecondOrderReliability };
    public static readonly UncertaintyPolicy CanonicalChaos = CanonicalMonteCarlo with { Method = UncertaintyMethod.PolynomialChaos, Samples = 1024, HyperbolicTruncation = true };
    public static readonly UncertaintyPolicy CanonicalSaltelli = CanonicalMonteCarlo with { Method = UncertaintyMethod.SobolSaltelli };
    public static readonly UncertaintyPolicy CanonicalMorris = CanonicalMonteCarlo with { Method = UncertaintyMethod.Morris, Samples = 512 };
    public static readonly UncertaintyPolicy CanonicalSubset = CanonicalMonteCarlo with { Method = UncertaintyMethod.SubsetSimulation, Samples = 1000 };
}

public sealed record UncertaintyResult(
    UncertaintyMethod Method,
    int Samples,
    double Mean,
    double Variance,
    double Skewness,
    double Kurtosis,
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
    // The Gaussian-copula transform: the standard-normal correlation factor L (Cholesky of policy.Correlation,
    // None for independent inputs) maps the quasi-random unit draw onto the physical input. The independent
    // path applies the per-axis Quantile directly (preserving the QMC equidistribution); the correlated path
    // routes the draw through the standard-normal space, correlates with L, and inverts each marginal.
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

    // Total dispatch over the closed UqStrategy driver set: a NEW driver value breaks this Switch expression at
    // COMPILE time (no `_` arm, no runtime strategy-unmapped fallback). The four drivers exhaust the propagation
    // shapes; the method row selects one through UncertaintyMethod.Strategy rather than re-spelling a runner.
    public static Fin<UncertaintyResult> Propagate(
        Seq<RandomVariable> inputs, UncertaintyPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, CorrelationId correlation, ClockPolicy clocks) =>
        inputs.IsEmpty
            ? Fin.Fail<UncertaintyResult>(new ComputeFault.ModelRejected("<uncertainty-empty-input-vector>"))
            : Copula(inputs.Count, policy).Bind(transform => policy.Method.Strategy switch {
                UqStrategy.MatrixSampling => SampleAndReduce(inputs, policy, transform, evaluate, clocks),
                UqStrategy.SpectralFit => Spectral(inputs, policy, transform, evaluate, clocks),
                UqStrategy.ReliabilitySearch => Reliability(inputs, policy, transform, evaluate, clocks),
                UqStrategy.Subset => Subset(inputs, policy, transform, evaluate, clocks),
            });

    public static ComputeReceipt.Uncertainty Receipt(UncertaintyResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Method.Key, result.Samples, result.Mean, result.Variance, result.Quantiles, result.SobolTotal, result.FailureProbability, result.ReliabilityIndex) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    // The copula factor: validate the correlation matrix shape, Cholesky-factor it once, and reject an
    // indefinite matrix through the same DeterminantLn-finiteness gate the dense-algebra owner uses for SPD
    // admission. None means independent inputs and the identity transform.
    static Fin<Transform> Copula(int dim, UncertaintyPolicy policy) =>
        policy.Correlation.Match(
            None: () => Fin.Succ(new Transform(None)),
            Some: r => r.RowCount == dim && r.ColumnCount == dim
                ? r.Cholesky() is var chol && double.IsFinite(chol.DeterminantLn)
                    ? Fin.Succ(new Transform(Some(chol.Factor)))
                    : Fin.Fail<Transform>(new ComputeFault.ModelRejected("<uncertainty-correlation-not-positive-definite>"))
                : Fin.Fail<Transform>(new ComputeFault.ModelRejected($"<uncertainty-correlation-shape:{r.RowCount}x{r.ColumnCount}!={dim}>")));

    static double Component(Seq<double> values, UncertaintyPolicy policy) =>
        values.At(policy.LimitStateObjective).IfNone(() => values.Head.IfNone(0.0));

    // --- [MATRIX_SAMPLING] ------------------------------------------------------------

    static Fin<UncertaintyResult> SampleAndReduce(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, ClockPolicy clocks) =>
        Design(inputs, policy, transform)
            .Bind(design => Sample(design, evaluate).Map(responses => Reduce(inputs, policy, design, responses, clocks)));

    // The sample matrix dispatches on the UncertaintyMethod.Design discriminant — the stratified LHS hypercube
    // composed from the Tensor/sampling#OWNED_BUILDS LowDiscrepancy.LatinHypercube owner, else the raw Sobol net
    // shaped into the Saltelli A/B/AB structured matrix the Sobol indices demand, the Morris one-at-a-time
    // trajectory grid, or the plain space-filling draw — then routes each unit row through the Gaussian-copula
    // transform onto the physical input, never a per-call System.Random.
    static Fin<Seq<ImmutableArray<double>>> Design(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform) {
        int count = Math.Max(2, policy.Samples), dim = inputs.Count;
        Fin<Seq<double[]>> unit = policy.Method.Design == SampleDesign.Stratified
            ? LowDiscrepancy.LatinHypercube(dim, count, policy.Seed, Scramble.DigitalShift).Map(net => toSeq(net))
            : LowDiscrepancy.Sobol(dimensions: dim, seed: policy.Seed, Scramble.DigitalShift).Map(generator => {
                Seq<double[]> draws = toSeq(Enumerable.Range(0, count)).Fold((Gen: generator, Points: Seq<double[]>()), static (acc, _) => {
                    var (next, point) = acc.Gen.Draw();
                    return (next, acc.Points.Add(point));
                }).Points;
                return policy.Method.Design switch {
                    SampleDesign.SaltelliAbAb => Saltelli(draws, count, dim),
                    SampleDesign.MorrisTrajectory => MorrisTrajectories(draws, count, dim, policy.MorrisLevels),
                    _ => draws,
                };
            });
        return unit.Map(rows => rows.Map(row => transform.Physical(inputs, row)));
    }

    static Fin<Seq<Seq<double>>> Sample(Seq<ImmutableArray<double>> design, Func<DesignPoint, Fin<Seq<double>>> evaluate) =>
        design.Fold(Fin.Succ(Seq<Seq<double>>()), (acc, coordinates) =>
            acc.Bind(responses => evaluate(new DesignPoint(coordinates, [], [])).Map(values => responses.Add(values))));

    static UncertaintyResult Reduce(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, Seq<Seq<double>> responses, ClockPolicy clocks) {
        double[] qoi = [.. responses.Map(values => Component(values, policy))];
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
        return new UncertaintyResult(policy.Method, qoi.Length, mean, variance, skewness, kurtosis, quantiles, first, total, interaction, Seq<double>(), pf, beta, clocks.Now);
    }

    // Saltelli A/B/AB design: matrices A and B of N rows each, plus the d cross-matrices AB_i (A with column i
    // replaced by B's), so the variance-based first-order Sᵢ and total-effect Sₜᵢ ride one (2+d)·N sample
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
    // perturbing one factor by Δ per step so the (d+1)·r design yields the μ* mean-absolute and σ standard-
    // deviation elementary-effect screen the full Sobol study refines.
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

    // First-order (Saltelli 2010) and total-effect (Jansen) Sobol indices from the A/B/AB blocks: the design
    // lays A at [0,N), B at [N,2N), and each AB_i at [(2+i)·N, (3+i)·N), so Sᵢ = E[Y_B·(Y_ABᵢ−Y_A)]/V and
    // Sₜᵢ = E[(Y_A−Y_ABᵢ)²]/2V ride one fold over the structured matrix.
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

    // Morris screen: μ* the mean absolute elementary effect (importance, returned in the total slot) and σ the
    // standard deviation of the elementary effects (nonlinearity/interaction, returned in the interaction slot).
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

    // Generic first-order Sobol main effect through the composed sweep tornado: the between-bin variance
    // V[E(Y|Xᵢ)]/V over the global variance is the available index from a plain MC/LHS sample; the separated
    // total-effect needs the dedicated sobol-saltelli design, so the matrix-sampling rows report first-order.
    static Seq<double> SobolBinned(Seq<RandomVariable> inputs, Seq<ImmutableArray<double>> design, double[] qoi) {
        var grid = new SweepGrid(
            inputs.Map(static v => (SweepAxis)new SweepAxis.Enumerated(v.VariableName, Seq<double>())),
            Seq(ObjectiveSense.Minimize),
            SensitivityMethod.SobolVariance);
        Seq<DesignPoint> points = toSeq(Enumerable.Range(0, Math.Min(design.Count, qoi.Length)))
            .Map(i => new DesignPoint(design[i], [qoi[i]], []));
        return SensitivityTornado.Of(grid, points, 0).Bars.Map(static bar => bar.Effect);
    }

    // --- [SPECTRAL_FIT] ---------------------------------------------------------------

    static Fin<UncertaintyResult> Spectral(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, ClockPolicy clocks) =>
        Design(inputs, policy, transform)
            .Bind(design => Sample(design, evaluate).Bind(responses => Fit(inputs, policy, design, responses, clocks)));

    // PCE spectral fit: build the orthonormal-polynomial Vandermonde over the per-input RecurrenceCoefficients
    // (hyperbolic-truncated for high dimension), fit through the owned dense thin-QR route (or the sparse-QR
    // route for a large hyperbolic-cross basis), then read mean/variance/Sobol in closed form from the
    // orthonormal coefficients — never a hand-rolled normal-equations solve and never a dense Vandermonde that
    // overflows. PCE assumes independent inputs; a correlated PCE requires a decorrelating basis transform.
    static Fin<UncertaintyResult> Fit(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<ImmutableArray<double>> design, Seq<Seq<double>> responses, ClockPolicy clocks) {
        double[] qoi = [.. responses.Map(values => Component(values, policy))];
        Seq<int[]> multiIndices = MultiIndexSet(inputs.Count, policy.PceOrder, policy.HyperbolicTruncation);
        Seq<RecurrenceCoefficients> bases = inputs.Map(v => v.Recurrence(policy.PceOrder, policy.StieltjesNodes));
        Matrix<double> vandermonde = Matrix<double>.Build.Dense(qoi.Length, multiIndices.Count, (row, col) =>
            multiIndices[col].Select((degree, axis) => bases[axis].Evaluate(degree, inputs[axis].Standardize(design[row][axis]))).Aggregate(1.0, static (a, b) => a * b));
        Vector<double> rhs = Vector<double>.Build.DenseOfArray(qoi);
        Fin<Vector<double>> coefficients = policy.HyperbolicTruncation && multiIndices.Count > policy.SparseBasisThreshold
            ? SparseFit(vandermonde, qoi)
            : DenseRoute.Solve(new FactorRoute.Orthonormal(vandermonde, QRMethod.Thin, Modified: false), rhs, TolerancePolicy.Derive(vandermonde, rhs));
        return coefficients.Map(c => ReadSpectral(inputs, policy, multiIndices, qoi, c, clocks));
    }

    // A large hyperbolic-cross basis routes the overdetermined fit to the sparse-QR FactoredOp least-squares,
    // dropping the below-floor Vandermonde entries into a COO triplet ingest so the sparse PCE coefficient fit
    // never densifies to Matrix<double>.QR.
    static Fin<Vector<double>> SparseFit(Matrix<double> vandermonde, double[] qoi) {
        var rows = new List<int>();
        var cols = new List<int>();
        var values = new List<double>();
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

    static UncertaintyResult ReadSpectral(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Seq<int[]> multiIndices, double[] qoi, Vector<double> coefficients, ClockPolicy clocks) {
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
        return new UncertaintyResult(policy.Method, qoi.Length, mean, variance, skewness, kurtosis, quantiles,
            toSeq(first.Select(m => m * inverse)), toSeq(total.Select(m => m * inverse)), Seq<double>(), Seq<double>(), pf, beta, clocks.Now);
    }

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

    // --- [RELIABILITY_SEARCH] ---------------------------------------------------------

    sealed record MppState(double[] U, double[] Alpha, double[] Grad, double Beta, double FailureProbability, int Evaluations);
    sealed record HlrfAcc(double[] U, double GHere, double[] Grad, double G0, bool Converged, int Evals);

    // FORM/SORM: find the most-probable point in standard-normal space by the HLRF iteration, scoring the
    // first-order β = ±|u*| and pf = Φ(−β) with the FORM importance factors αᵢ², then add the Breitung
    // second-order curvature correction on the SORM row. The limit state is ONE LimitState owner with two
    // gradient sources: the exact-AD Smooth arm (a hyperdual-authored g — SensitivityLaw.Gradient/Hessian,
    // ONE evaluation, exact derivatives) and the honest finite-difference Oracle arm for the black-box
    // solve/surrogate evaluator no hyper-dual instantiation can reach — the leg law the Sensitivity family
    // seals, never a silent wrong gradient and never a fourth mechanism.
    static Fin<UncertaintyResult> Reliability(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, ClockPolicy clocks) {
        LimitState g = policy.SmoothLimitState.Match(
            Some: static smooth => (LimitState)new LimitState.Smooth(smooth),
            None: () => new LimitState.Oracle(u => evaluate(new DesignPoint(transform.FromU(inputs, u), [], []))
                .Map(values => policy.LimitStateThreshold - Component(values, policy))));
        return Hlrf(inputs.Count, policy, g).Bind(mpp =>
            (policy.Method == UncertaintyMethod.SecondOrderReliability ? Breitung(mpp, policy, g) : Fin.Succ(mpp.FailureProbability))
                .Map(pf => Assemble(inputs, policy, transform, mpp, pf, clocks)));
    }

    // The limit-state carrier: value, gradient-probe, and Hessian internalize the source dispatch so the
    // HLRF descent and the Breitung curvature never re-switch the gradient axis.
    public abstract record LimitState {
        private LimitState() { }

        public sealed record Oracle(Func<double[], Fin<double>> G) : LimitState;
        public sealed record Smooth(Func<DDScalar[], DDScalar> G) : LimitState;

        public Fin<double> Value(double[] u) =>
            this switch {
                Oracle oracle => oracle.G(u),
                Smooth smooth => SensitivityLaw.Gradient(smooth.G, u).Map(static r => r.Value),
                _ => Fin.Fail<double>(ComputeFault.Create("<limit-state-unmapped>")),
            };

        // Value-and-gradient in one shot: the Smooth arm is ONE hyperdual evaluation (exact, Evals = 1);
        // the Oracle arm is the central-difference probe (2·dim+1 calls) threaded through the Fin rail.
        public Fin<(double G, double[] Grad, int Evals)> Probe(double[] u, double step) =>
            this switch {
                Smooth smooth => SensitivityLaw.Gradient(smooth.G, u).Map(static r => (r.Value, r.Gradient, 1)),
                Oracle oracle => FiniteProbe(u, step, oracle.G),
                _ => Fin.Fail<(double, double[], int)>(ComputeFault.Create("<limit-state-unmapped>")),
            };

        public Fin<Matrix<double>> Curvature(double[] u, double step) =>
            this switch {
                Smooth smooth => SensitivityLaw.Hessian(smooth.G, u).Map(static r => Matrix<double>.Build.DenseOfArray(r.Hessian)),
                Oracle oracle => FiniteHessian(u, step, oracle.G),
                _ => Fin.Fail<Matrix<double>>(ComputeFault.Create("<limit-state-unmapped>")),
            };
    }

    static UncertaintyResult Assemble(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, MppState mpp, double pf, ClockPolicy clocks) {
        double beta = pf is > 0.0 and < 1.0 ? -Normal.InvCDF(0.0, 1.0, pf) : mpp.Beta;
        return new UncertaintyResult(policy.Method, mpp.Evaluations, double.NaN, double.NaN, double.NaN, double.NaN,
            Seq<double>(), Seq<double>(), toSeq(mpp.Alpha.Select(static a => a * a)), Seq<double>(), toSeq(transform.FromU(inputs, mpp.U)), pf, beta, clocks.Now);
    }

    static Fin<MppState> Hlrf(int dim, UncertaintyPolicy policy, LimitState g) =>
        Begin(dim, policy.FiniteDifferenceStep, g).Bind(start =>
            toSeq(Enumerable.Range(0, Math.Max(1, policy.ReliabilityIterations)))
                .Fold(Fin.Succ(start), (acc, _) => acc.Bind(state => state.Converged ? Fin.Succ(state) : Step(state, policy, g)))
                .Map(Finalize));

    static Fin<HlrfAcc> Begin(int dim, double step, LimitState g) =>
        g.Probe(new double[dim], step).Map(probe => new HlrfAcc(new double[dim], probe.G, probe.Grad, probe.G, false, probe.Evals));

    // Central-difference value-and-gradient probe of a BLACK-BOX limit state at u: 2·dim+1 oracle calls
    // threaded through the Fin rail so any evaluation failure aborts the descent — the honest Oracle arm;
    // the Smooth arm never reaches here (one exact hyperdual evaluation).
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
        if (gradNormSquared < 1e-24) { return Fin.Succ(acc with { Converged = true }); }
        double scale = (TensorPrimitives.Dot<double>(acc.U, acc.Grad) - acc.GHere) / gradNormSquared;
        double[] next = [.. acc.Grad.Select(gi => scale * gi)];
        bool converged = Distance(next, acc.U) < policy.ReliabilityTolerance;
        return g.Probe(next, policy.FiniteDifferenceStep).Map(probe =>
            acc with { U = next, GHere = probe.G, Grad = probe.Grad, Converged = converged, Evals = acc.Evals + probe.Evals });
    }

    static MppState Finalize(HlrfAcc acc) {
        double norm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(acc.U));
        double gradNorm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(acc.Grad));
        double[] alpha = gradNorm > 1e-12 ? [.. acc.Grad.Select(gi => -gi / gradNorm)] : new double[acc.U.Length];
        double beta = (acc.G0 >= 0.0 ? 1.0 : -1.0) * norm;
        return new MppState(acc.U, alpha, acc.Grad, beta, Normal.CDF(0.0, 1.0, -beta), acc.Evals);
    }

    // Breitung asymptotic SORM: pf ≈ Φ(−β)·Π(1 + β·κⱼ)^(−1/2) over the principal curvatures κⱼ of the limit
    // state at the MPP. The curvatures are the Evd eigenvalues of the tangent-projected scaled Hessian
    // A = (I − ααᵀ)·(H/|∇g|)·(I − ααᵀ); the eigenvalue along α is ~0 and dropped, leaving the n−1 curvatures.
    static Fin<double> Breitung(MppState mpp, UncertaintyPolicy policy, LimitState g) =>
        g.Curvature(mpp.U, policy.FiniteDifferenceStep).Bind(hessian => {
            double gradNorm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(mpp.Grad));
            if (gradNorm < 1e-12) { return Fin.Succ(mpp.FailureProbability); }
            double[] a = mpp.Alpha;
            Matrix<double> projector = Matrix<double>.Build.Dense(a.Length, a.Length, (i, j) => (i == j ? 1.0 : 0.0) - a[i] * a[j]);
            Matrix<double> curvatureMatrix = projector * hessian * projector / gradNorm;
            return DenseRoute.Decompose(curvatureMatrix, FactorizationKind.Evd).Map(factor => {
                double[] kappa = factor is Factorization.Evd evd ? [.. evd.Decomposition.EigenValues.Map(static value => value.Real)] : [];
                int drop = NearestZero(kappa);
                double product = 1.0;
                for (int j = 0; j < kappa.Length; j++) {
                    if (j == drop) { continue; }
                    double bracket = 1.0 + mpp.Beta * kappa[j];
                    product *= bracket > 1e-9 ? 1.0 / Math.Sqrt(bracket) : 1.0;
                }
                return Math.Clamp(Normal.CDF(0.0, 1.0, -mpp.Beta) * product, 0.0, 1.0);
            });
        });

    // Symmetric finite-difference Hessian of a BLACK-BOX limit state at u: central second differences on
    // the diagonal and the four-point central mixed differences off-diagonal, threaded through the Fin
    // rail — the honest Oracle arm; the Smooth arm reads the exact hyperdual GetHessian().
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

    // Subset simulation conditions successive populations on intermediate limit-state thresholds so a rare
    // failure pf ~ 10⁻⁶ resolves in O(N·log pf) evaluations: level 0 is a plain standard-normal MC population,
    // each subsequent level keeps the p₀-quantile failure seeds and repopulates through the Au-Beck Modified
    // Metropolis sampler, and pf = p₀^(m−1)·P(F_m | F_{m−1}). The limit state LSF(u) = threshold − response,
    // failure LSF ≤ 0; the MCMC chain draws one explicitly-seeded Random so the conditional walk is replayable.
    static Fin<UncertaintyResult> Subset(Seq<RandomVariable> inputs, UncertaintyPolicy policy, Transform transform, Func<DesignPoint, Fin<Seq<double>>> evaluate, ClockPolicy clocks) {
        int dim = inputs.Count, n = Math.Max(4, policy.Samples);
        double p0 = Math.Clamp(policy.SubsetLevelProbability, 0.01, 0.5);
        int keep = Math.Max(1, (int)Math.Round(p0 * n));
        var rng = new Random(policy.Seed);
        Func<double[], Fin<double>> lsf = u => evaluate(new DesignPoint(transform.FromU(inputs, u), [], []))
            .Map(values => policy.LimitStateThreshold - Component(values, policy));
        return Population(dim, n, rng, lsf).Bind(initial =>
            toSeq(Enumerable.Range(0, 8)).Fold(Fin.Succ(new SubsetAcc(initial, 1.0, false, n)),
                (acc, _) => acc.Bind(state => state.Done ? Fin.Succ(state) : Advance(state, dim, n, keep, p0, policy, rng, lsf)))
            .Map(state => SubsetResult(policy, state, clocks)));
    }

    static Fin<Seq<(double[] U, double Lsf)>> Population(int dim, int n, Random rng, Func<double[], Fin<double>> lsf) =>
        toSeq(Enumerable.Range(0, n)).Fold(Fin.Succ(Seq<(double[], double)>()), (acc, _) => acc.Bind(population => {
            double[] u = StandardNormal(dim, rng);
            return lsf(u).Map(value => population.Add((u, value)));
        }));

    static Fin<SubsetAcc> Advance(SubsetAcc state, int dim, int n, int keep, double p0, UncertaintyPolicy policy, Random rng, Func<double[], Fin<double>> lsf) {
        var sorted = state.Population.OrderBy(static p => p.Lsf).ToArray();
        double threshold = sorted[Math.Min(keep, sorted.Length) - 1].Lsf;
        if (threshold <= 0.0) { return Fin.Succ(state with { Done = true }); }
        double[][] seeds = [.. sorted.Take(keep).Select(static p => p.U)];
        return Repopulate(seeds, dim, n, threshold, rng, lsf).Map(population =>
            state with { Population = population, Probability = state.Probability * p0, Evaluations = state.Evaluations + n });
    }

    static Fin<Seq<(double[] U, double Lsf)>> Repopulate(double[][] seeds, int dim, int n, double threshold, Random rng, Func<double[], Fin<double>> lsf) {
        int perChain = Math.Max(1, n / seeds.Length);
        return toSeq(seeds).Fold(Fin.Succ(Seq<(double[], double)>()), (acc, seed) =>
            acc.Bind(population => Chain(seed, perChain, dim, threshold, rng, lsf).Map(chain => population + chain)));
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

    // Au-Beck Modified Metropolis: each coordinate proposes a unit random-walk step accepted by the
    // standard-normal ratio exp(½(uᵢ²−u'ᵢ²)); the candidate enters the failure region only if its LSF holds,
    // checked once on the assembled candidate by the caller.
    static double[] Propose(double[] u, int dim, Random rng) =>
        [.. Enumerable.Range(0, dim).Select(i => {
            double candidate = u[i] + Normal.InvCDF(0.0, 1.0, Math.Clamp(rng.NextDouble(), 1e-12, 1.0 - 1e-12));
            double ratio = Math.Exp(0.5 * (u[i] * u[i] - candidate * candidate));
            return rng.NextDouble() < ratio ? candidate : u[i];
        })];

    static double[] StandardNormal(int dim, Random rng) =>
        [.. Enumerable.Range(0, dim).Select(_ => Normal.InvCDF(0.0, 1.0, Math.Clamp(rng.NextDouble(), 1e-12, 1.0 - 1e-12)))];

    static UncertaintyResult SubsetResult(UncertaintyPolicy policy, SubsetAcc state, ClockPolicy clocks) {
        double[] lsf = [.. state.Population.Map(static p => p.Lsf)];
        double finalFraction = lsf.Length == 0 ? 0.0 : (double)lsf.Count(static value => value <= 0.0) / lsf.Length;
        double pf = Math.Clamp(state.Probability * finalFraction, 0.0, 1.0);
        double beta = pf is > 0.0 and < 1.0 ? -Normal.InvCDF(0.0, 1.0, pf) : pf <= 0.0 ? double.PositiveInfinity : double.NegativeInfinity;
        return new UncertaintyResult(policy.Method, state.Evaluations, double.NaN, double.NaN, double.NaN, double.NaN,
            Seq<double>(), Seq<double>(), Seq<double>(), Seq<double>(), Seq<double>(), pf, beta, clocks.Now);
    }
}
```
