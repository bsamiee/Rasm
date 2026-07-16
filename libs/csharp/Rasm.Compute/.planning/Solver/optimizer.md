# [COMPUTE_OPTIMIZER]

Rasm.Compute solver optimizer: one `Optimizer` design-space-search axis over a typed `DesignVariable`/`ActivationRule`/`ConstraintHandling`/`ObjectiveSense` problem, dispatching one polymorphic `Optimize` entry by `OptimizerKind` row to a per-family kernel that owns its iteration budget and adaptation state — NSGA multi-objective evolution over `GeneticSharp.GeneticAlgorithm`, CMA-ES rank-`μ`/rank-one covariance adaptation, Clerc-constriction PSO, Metropolis simulated annealing, Bayesian-GP acquisition, gradient-adjoint trust-region/Armijo descent, topology-SIMP optimality criteria, OR-Tools CP-SAT/MILP exact solving, `LowDiscrepancy.Sobol` multi-start restart, and robust-minimax/RBDO.

Owned surface: the `OptimizerKind`/`DesignVariable`/`ActivationRule`/`ObjectiveSense`/`ConstraintHandling`/`StepControl`/`AcquisitionFunction`/`SurrogateKind`/`Orthogonalization` vocabulary, the `LinearModel`/`DesignProblem`/`DesignPoint`/`ParetoFront`/`OptimizerPolicy`/`KernelRun`/`OptimizationResult` carriers, the `Surrogate`/`RomBasis`/`GpModel`/`RbfModel`/`NeuralFieldModel` reduced-order models, and the `Optimizer` fold plus the `GeneticEngine` GeneticSharp capsule. `evaluate` is one `Func<DesignPoint, Fin<Seq<double>>>` returning the objective vector concatenated with the constraint vector, split by `problem.Objectives.Count` so `ConstraintHandling` stays reachable; full `Solver/contract#SOLVE_CONTRACT` evaluation and `Surrogate.Predict` both remain on `Fin`, and the surrogate result additionally carries its bound. `MultiFidelity.Fused` is the fidelity axis ON that contract — an `Analysis` closed-form fold as the cheap leg, the full FEA/energy solve as the expensive leg, per-objective additive-correction surrogates fusing both under one `FidelityPolicy` budget — and the composed arrow IS the contract shape, so every `OptimizerKind` row searches it unchanged; the package holds BOTH fidelity tiers in one runtime, and the fusion converts that co-location into thousands of closed-form evaluations per tens of exact solves. Gradient-adjoint reads `SensitivityLaw.Chain` over the `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryTape` lowered from `DesignProblem.DesignMesh`; GP-covariance Cholesky and marginal-likelihood ride `Tensor/blas#DENSE_ALGEBRA` `Cholesky<double>`, the reduced basis the `Orthogonalization` SVD/QR rows, and the neural field the `Model/inference#INFERENCE_MODES` `RunOps.Infer` OrtValue run keyed by the parametric-family `XxHash128` digest. Settled arrivals: the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, `ClockPolicy`, the `ComparerAccessors.StringOrdinal` accessor from `Solver/discretization#DISCRETIZATION_MESH`, the `Rasm.Meshing` `MeshAdjointSnapshot` / `Rasm.Numerics` `DiscreteCalculus` DDG-adjoint surface, and the `GeometryTape` shape. `ParetoFront` crosses to Persistence content-keyed and `Surrogate` crosses to `Solver/clash#CLASH_AND_TWIN` as the digital-twin baseline.

## [01]-[INDEX]

- [01]-[OPTIMIZER_LANE]: design-var/link/conditional search; per-family kernels; constraint axis; ROM/GP/field surrogate duality.

## [02]-[OPTIMIZER_LANE]

- Owner: `OptimizerKind` `[SmartEnum<string>]` search-algorithm rows; `DesignVariable` `[Union]` typed variable cases (free + linked/derived); `ActivationRule` `[Union]` conditional active-set cases; `ObjectiveSense` `[SmartEnum<string>]` minimize/maximize rows; `ConstraintHandling` `[SmartEnum<string>]` feasibility-policy rows (death-penalty/static-penalty/feasibility-rules/augmented-lagrangian) with a live multiplier-advance; `StepControl` `[SmartEnum<string>]` gradient line-search/trust-region rows; `AcquisitionFunction` `[SmartEnum<string>]` Bayesian-acquisition rows (expected-improvement/upper-confidence-bound/probability-of-improvement); `SurrogateKind` `[SmartEnum<string>]` surrogate-model rows (linear-trend/gaussian-process/radial-basis/neural-field); `Orthogonalization` `[SmartEnum<string>]` ROM reduced-basis rows; `LinearModel`/`LinearRow` the typed objective+constraint model the exact `cp-sat`/`milp` rows lower to OR-Tools; `DesignProblem` the variable/activation/constraint/objective record with the link+active-set `Resolve` fold and the optional `LinearModel` for the exact rows; `DesignPoint` the coordinate/objective/constraint sample; `OptimizerPolicy` the per-kind tuning record; `ParetoFront` the queryable non-dominated-set artifact with crowding-distance ranking and exact bi-objective hypervolume; `KernelRun` the per-kernel run result the `Optimize` fold projects onto `OptimizationResult`; `Optimizer` the static search fold dispatching one `Optimize` entry by `OptimizerKind` to its genuine per-family kernel; `GeneticEngine` the `GeneticSharp.GeneticAlgorithm` NSGA-style proposal capsule (genome from the `DesignVariable` cases, the dominance-rank+crowding `FuncFitness` over the `evaluate` oracle, `ParallelTaskExecutor` on the bounded lanes); `Surrogate` the reduced-order/learned model carrying an optional `RomBasis`, a `GpModel` covariance Cholesky, an `RbfModel`, and a content-keyed `NeuralFieldModel`; `RomBasis` the orthonormal reduced-basis projector; `GpModel`/`RbfModel` the scattered-data posteriors; `NeuralFieldModel` the parametric-family-digest-keyed coordinate-MLP/Fourier-feature field evaluated through the model-lane OrtValue run; `MultiFidelity`/`FidelityPolicy`/`FidelityState` the fused low/high oracle composition, its budget policy, and its paired-evaluation correction state.
- Cases: `OptimizerKind` rows nsga2 · bayesian-gp · gradient-adjoint · topology-simp · simulated-annealing · cma-es · pso · cp-sat · milp · multi-start-global · robust-minimax (`exact=true` for cp-sat/milp; `populationBased=true` for nsga2/cma-es/pso/robust-minimax; `gradientBased=true` for gradient-adjoint/topology-simp); `DesignVariable` cases `Continuous` · `Integer` · `Categorical` · `Density` (topology field) · `Linked` (shared/derived — `Scale·source + Offset`, `Free=false`); `ActivationRule` cases `Always` · `WhenAbove` · `WhenBelow` · `WhenChoice`; `ConstraintHandling` rows death-penalty · static-penalty · feasibility-rules · augmented-lagrangian (`multiplierUpdate=true` only for augmented-lagrangian); `StepControl` rows fixed · armijo-backtrack · trust-region; `AcquisitionFunction` rows expected-improvement · upper-confidence-bound · probability-of-improvement; `SurrogateKind` rows linear-trend · gaussian-process · radial-basis · neural-field; `Orthogonalization` rows qr · modified-gram-schmidt · deim · pod-svd (`Interpolatory=true` for deim); `ObjectiveSense` rows minimize · maximize.
- Entry: `public static Fin<OptimizationResult> Optimize(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ClockPolicy clocks)` — `Fin<T>` aborts on an invalid design space, policy, oracle output, or kernel state; `evaluate` returns exactly `Objectives.Count + Constraints` finite values with objectives first. One shared `Atom<(int Evals, int Hits)>` meters the full and surrogate oracles, and a surrogate hit is admitted only when its bound and output cardinality satisfy the problem contract.
- Auto: `Optimize` dispatches each `OptimizerKind` row through one generated total `Switch` (`Invoke`) to its genuine kernel, invoked exactly once, so a NEW row breaks the dispatch at COMPILE time rather than faulting at runtime; `multi-start-global` re-enters the same dispatch for its inner row. Constraint handling is the `ConstraintHandling` row and the surrogate duality is a policy column gating cheap-versus-full evaluation with the surrogate-hit count metered honestly.
- Receipt: the `Optimization` `ComputeReceipt` case carries the optimizer key, the kernel-reported generation count, the metered evaluated-point count, the metered surrogate-hit count, the front size, and the hypervolume indicator (the receipt's six audit slots); the constraint-violation history and the trust-region radius ride the `OptimizationResult` carrier, and the per-evaluation surrogate error bound and GP marginal-likelihood ride the `Surrogate`/`GpModel` so a ROM/GP acceptance is auditable without a receipt slot the `Runtime/receipts#RECEIPT_UNION` owner does not declare.
- Packages: MathNet.Numerics (the dense `Matrix<double>.Evd`/`Cholesky`/`Svd`/`QR` algebra + `Distributions.Normal` reliability/sampling), GeneticSharp (the NSGA-style `GeneticAlgorithm` engine + chromosome/operator/executor catalog behind the `nsga2` row), Google.OrTools (CP-SAT `CpModel`/`CpSolver` + LinearSolver `Solver` behind the `cp-sat`/`milp` rows), System.Numerics.Tensors, Microsoft.ML.OnnxRuntime (the neural-field `OrtValue` run), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, the `MeshAdjointSnapshot`/`DiscreteCalculus` public surface for the DDG gradient-adjoint tape), Rasm.Persistence (project), BCL inbox
- Growth: a new search algorithm is one `OptimizerKind` row plus one arm on the `Optimize` total `Switch` (`Invoke`) — a population-based row binds its own update rule (CMA-ES covariance, PSO velocity, SA Metropolis) or `GeneticEngine` for a genuine GA; an exact OR-Tools row lowers the `LinearModel` to a `CpModel`/`Solver`; a wrapping row composes the inner kernel through the same dispatch — and the generated `Switch` breaks at COMPILE time until the arm is added (never a runtime kind-miss); a new genetic operator is one `GeneticEngine.Crossover`/`Mutation` arm binding the `GeneticSharp` operator; a new variable kind is one `DesignVariable` case carrying its `AdjointOperator`; a new constraint discipline is one `ConstraintHandling` row; a new line-search/trust-region is one `StepControl` row; a new acquisition is one `AcquisitionFunction` row; a new surrogate model is one `SurrogateKind` row plus a `Fit` arm; a new ROM orthogonalization is one `Orthogonalization` row; a new fidelity tier or fusion posture (co-kriging over the delta GP, a three-tier cascade) is one `FidelityPolicy` column or one `FidelityState.Paired` refit arm on the SAME fused contract, never a second oracle shape; a new objective is one row on the `DesignProblem` objective set; zero new surface — an `Nsga2Engine`/`BayesianOptimizer`/`CmaEsSolver`/`ParticleSwarm`/`Annealer`/`TopologyOptimizer`/`CpSatSolver`/`MilpSolver`/`MultiStartRunner` sibling family is collapsed onto the one `Optimize` total `Switch`, a `LinkedVariable`/`DerivedVariable` family onto `DesignVariable.Linked`, a `PenaltyHandler`/`FeasibilityHandler` family onto `ConstraintHandling`, a `QrReducer`/`GramSchmidtReducer`/`DeimReducer` family onto `Orthogonalization`, and a `SurrogateNet`/`FieldPredictor` sibling onto the `Surrogate.NeuralField` row.
- Boundary: contract-uniform — `evaluate` is the single coupling point, so the search composes a full FEA solve or a railed `Surrogate.Predict` without a parallel surrogate-search path. Objective-vector-then-constraint-vector concatenation keeps the `ConstraintHandling` axis reachable; a permanently-empty constraint set silently disabling penalty/feasibility/augmented-Lagrangian handling is rejected. Typed variables make a bound violation a boundary fault, never a clamped silent repair, and variable-linking plus conditional design spaces are rows on the same axis through `DesignProblem.Resolve`. FIVE genuine kernels — `nsga2` routes `GeneticEngine` over the admitted `GeneticSharp.GeneticAlgorithm` (package owns the GA machinery, page owns the dominance-rank+crowding comparator and `ParetoFront`), while `cma-es`, `pso`, and `simulated-annealing` are distinct algorithms no admitted package owns, authored as in-package folds over the `Matrix<double>.Evd`/`Distributions.Normal` substrate; an operator-swap masquerade routing them through one `GeneticAlgorithm` with zero covariance/velocity/temperature state is rejected. `bayesian-gp` FITS a `GpModel` from the running history each iteration and ranks candidates by the acquisition over the posterior; a loop that never fits the GP and ranks by a constant is rejected. Gradient-adjoint reads the VERIFIED two-argument `SensitivityLaw.Chain(problem.AdjointTape, seed)` overload (cotangent the only seed) — a phantom three-argument `Chain(tape, inputs, seed)` call is the API trap; `AdjointOperator` lowers `Continuous`→`Gradient` and `Density`→`CotangentLaplacian` at compile time, and an absent `DesignMesh` lowers an empty `Seq<GeometryTape>` over the `MeshAdjointSnapshot` so the descent is degenerate by construction (the absent-mesh case, never an absent operator). `StepControl` owns the line-search/trust-region; a fixed-step descent without step control is rejected. Topology-SIMP reads the genuine compliance sensitivity from that same adjoint route and bisects the Lagrange multiplier to the volume constraint; a density update whose base ignores the structural sensitivity (a constant `−1/λ` power) is the deleted fake. Augmented-Lagrangian carries a LIVE multiplier advanced `λ ← max(0, λ + ρ·g)` each generation and read by `Penalize`; a `MultiplierUpdate` flag degenerating to static penalty is rejected. ROM reduction is one `Orthogonalization` SmartEnum (QR/modified-Gram-Schmidt/DEIM/POD-SVD) over the snapshot matrix. `SurrogateKind` is the surrogate axis — the neural-field row threads the leased `Model/inference#INFERENCE_MODES` `(InferenceSession, RunOptions, CancelScope)` so `Predict` runs the coordinate-MLP/Fourier field through `RunOps.Infer` behind the same `Fin<(Seq<double> Values, double Bound)>` rail the GP/POD rows answer, its ONNX weights fitted offline by the Python companion and crossed over `Runtime/wire#PROTO_VOCABULARY` (C# owns only inference; an in-proc ORT-Training fit is rejected). A surrogate drifting past its bound forces a full re-evaluation, the surrogate-hit count metered honestly through the shared `Atom`; a receipt slot that stays zero is rejected. `ParetoFront` is content-addressed onto the Persistence vector index and the exact bi-objective hypervolume is the staircase sweep (≥3-objective a Monte-Carlo estimate over the reference box); a Lebesgue-box overcount double-counting dominated overlaps is rejected. Exact `cp-sat`/`milp` lower the typed `DesignProblem.Exact` `LinearModel` to a genuine OR-Tools `CpModel`/`Solver` and fault `<exact-needs-linear-model>` when absent, because an exact solver cannot optimize the black-box FEA objective (a string-parsed or empty model is rejected); CP-SAT solves integer/boolean natively and discretizes continuous through `IntegerStep` under ONE declared coordinate system — coefficients and bounds scale through `q = round(1/IntegerStep)` so the integer model preserves the physical `LinearModel` semantics, and the harvested assignment re-evaluates through the oracle in physical space — while MILP routes the integer part to SCIP and the continuous part through the linear backend with no discretization. `multi-start-global` wraps any inner row (guarded against self-recursion) with a `LowDiscrepancy.Sobol` basin restart rather than a `System.Random` fill. `robust-minimax` reads the `Solver/uncertainty#UNCERTAINTY_LANE` `RandomVariable` scenario set through the SAME `LowDiscrepancy.Sobol`+`RandomVariable.Quantile` inverse-transform the UQ lane uses, scores each candidate worst-case, and appends the reliability chance constraint `β_target − β ≤ 0` (`β = Normal.InvCDF(1 − pf)`) onto the `ConstraintHandling` axis so RBDO is a constraint row and the deep FORM/SORM/PCE stay the uncertainty lane's. OR-Tools native handles enter only through declared `IDisposable` roots (`CpSolver`/`Solver`) released by `Dispose`; a hand-rolled branch-and-bound, simplex, or float-equality feasibility check beside the solver is rejected.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OptimizerKind {
    public static readonly OptimizerKind Nsga2 = new("nsga2", populationBased: true, gradientBased: false, exact: false);
    public static readonly OptimizerKind BayesianGp = new("bayesian-gp", populationBased: false, gradientBased: false, exact: false);
    public static readonly OptimizerKind GradientAdjoint = new("gradient-adjoint", populationBased: false, gradientBased: true, exact: false);
    public static readonly OptimizerKind TopologySimp = new("topology-simp", populationBased: false, gradientBased: true, exact: false);
    public static readonly OptimizerKind SimulatedAnnealing = new("simulated-annealing", populationBased: false, gradientBased: false, exact: false);
    public static readonly OptimizerKind CmaEs = new("cma-es", populationBased: true, gradientBased: false, exact: false);
    public static readonly OptimizerKind Pso = new("pso", populationBased: true, gradientBased: false, exact: false);
    public static readonly OptimizerKind CpSat = new("cp-sat", populationBased: false, gradientBased: false, exact: true);
    public static readonly OptimizerKind Milp = new("milp", populationBased: false, gradientBased: false, exact: true);
    public static readonly OptimizerKind MultiStartGlobal = new("multi-start-global", populationBased: false, gradientBased: false, exact: false);
    public static readonly OptimizerKind RobustMinimax = new("robust-minimax", populationBased: true, gradientBased: false, exact: false);
    public bool PopulationBased { get; }
    public bool GradientBased { get; }
    public bool Exact { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectiveSense {
    public static readonly ObjectiveSense Minimize = new("minimize", sign: 1.0);
    public static readonly ObjectiveSense Maximize = new("maximize", sign: -1.0);

    public double Sign { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConstraintHandling {
    public static readonly ConstraintHandling DeathPenalty = new("death-penalty", multiplierUpdate: false);
    public static readonly ConstraintHandling StaticPenalty = new("static-penalty", multiplierUpdate: false);
    public static readonly ConstraintHandling FeasibilityRules = new("feasibility-rules", multiplierUpdate: false);
    public static readonly ConstraintHandling AugmentedLagrangian = new("augmented-lagrangian", multiplierUpdate: true);

    public bool MultiplierUpdate { get; }

    public double Penalize(double objective, ReadOnlySpan<double> constraints, double weight, ReadOnlySpan<double> multipliers) =>
        Switch(
            state: (Objective: objective, Constraints: Violation(constraints), Weight: weight, Multipliers: Lagrange(constraints, multipliers)),
            deathPenalty: static s => s.Constraints > 0.0 ? double.MaxValue : s.Objective,
            staticPenalty: static s => s.Objective + s.Weight * s.Constraints * s.Constraints,
            feasibilityRules: static s => s.Objective,
            augmentedLagrangian: static s => s.Objective + s.Multipliers + 0.5 * s.Weight * s.Constraints * s.Constraints);

    public bool Dominates(DesignPoint self, DesignPoint other, ReadOnlySpan<double> senses) =>
        this == FeasibilityRules
            ? (self.Feasible, other.Feasible) switch {
                (true, false) => true,
                (false, true) => false,
                (false, false) => Violation(self.Constraints.AsSpan()) < Violation(other.Constraints.AsSpan()),
                _ => self.Dominates(other, senses),
            }
            : self.Dominates(other, senses);

    public double[] Advance(double[] multipliers, ReadOnlySpan<double> constraints, double rho) {
        double[] values = constraints.ToArray();
        return MultiplierUpdate
            ? [.. multipliers.Select((value, axis) => Math.Max(0.0, value + rho * (axis < values.Length ? values[axis] : 0.0)))]
            : multipliers;
    }

    static double Violation(ReadOnlySpan<double> constraints) =>
        constraints.ToArray().Sum(static constraint => Math.Max(0.0, constraint));

    static double Lagrange(ReadOnlySpan<double> constraints, ReadOnlySpan<double> multipliers) =>
        Enumerable.Range(0, Math.Min(constraints.Length, multipliers.Length))
            .Sum(axis => multipliers[axis] * Math.Max(0.0, constraints[axis]));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StepControl {
    public static readonly StepControl Fixed = new("fixed", trustRegion: false);
    public static readonly StepControl ArmijoBacktrack = new("armijo-backtrack", trustRegion: false);
    public static readonly StepControl TrustRegion = new("trust-region", trustRegion: true);

    public bool TrustRegion { get; }

    public (double Step, double Radius) Next(double radius, double actualReduction, double predictedReduction, double baseStep) =>
        Switch(
            state: (Radius: radius, Ratio: predictedReduction > 1e-12 ? actualReduction / predictedReduction : 0.0, Base: baseStep),
            fixed: static s => (s.Base, s.Radius),
            armijoBacktrack: static s => (s.Ratio >= 1e-4 ? s.Base : s.Base * 0.5, s.Radius),
            trustRegion: static s => s.Ratio < 0.25
                ? (Math.Min(s.Base, 0.25 * s.Radius), 0.25 * s.Radius)
                : s.Ratio > 0.75 ? (Math.Min(2.0 * s.Radius, s.Base), 2.0 * s.Radius) : (s.Base, s.Radius));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AcquisitionFunction {
    public static readonly AcquisitionFunction ExpectedImprovement = new("expected-improvement", exploreWeight: 0.0);
    public static readonly AcquisitionFunction UpperConfidenceBound = new("upper-confidence-bound", exploreWeight: 2.0);
    public static readonly AcquisitionFunction ProbabilityOfImprovement = new("probability-of-improvement", exploreWeight: 0.0);

    public double ExploreWeight { get; }

    public double Score(double mean, double sigma, double best) =>
        Switch(
            state: (Mean: mean, Sigma: sigma, Best: best, Z: sigma > 1e-12 ? (best - mean) / sigma : 0.0, Explore: ExploreWeight),
            expectedImprovement: static s => s.Sigma > 1e-12 ? (s.Best - s.Mean) * Normal.CDF(0.0, 1.0, s.Z) + s.Sigma * Normal.PDF(0.0, 1.0, s.Z) : 0.0,
            upperConfidenceBound: static s => -s.Mean + s.Explore * s.Sigma,
            probabilityOfImprovement: static s => Normal.CDF(0.0, 1.0, s.Z));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SurrogateKind {
    public static readonly SurrogateKind LinearTrend = new("linear-trend");
    public static readonly SurrogateKind GaussianProcess = new("gaussian-process");
    public static readonly SurrogateKind RadialBasis = new("radial-basis");
    public static readonly SurrogateKind NeuralField = new("neural-field");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Orthogonalization {
    public static readonly Orthogonalization Qr = new("qr", interpolatory: false);
    public static readonly Orthogonalization ModifiedGramSchmidt = new("modified-gram-schmidt", interpolatory: false);
    public static readonly Orthogonalization Deim = new("deim", interpolatory: true);
    public static readonly Orthogonalization PodSvd = new("pod-svd", interpolatory: false);

    public bool Interpolatory { get; }

    public RomBasis Reduce(Matrix<double> snapshots, int rank) =>
        Switch(
            state: (Snapshots: snapshots, Rank: rank),
            qr: static s => OrthonormalQr(s.Snapshots, s.Rank),
            modifiedGramSchmidt: static s => OrthonormalMgs(s.Snapshots, s.Rank),
            deim: static s => OrthonormalDeim(s.Snapshots, s.Rank),
            podSvd: static s => OrthonormalPod(s.Snapshots, s.Rank));

    static RomBasis OrthonormalQr(Matrix<double> snapshots, int rank) {
        QR<double> qr = snapshots.QR();
        int k = Math.Min(rank, qr.Q.ColumnCount);
        Matrix<double> basis = qr.Q.SubMatrix(0, qr.Q.RowCount, 0, k);
        return new RomBasis(basis, [], k, 1.0);
    }

    static RomBasis OrthonormalMgs(Matrix<double> snapshots, int rank) {
        int rows = snapshots.RowCount, k = Math.Min(rank, snapshots.ColumnCount);
        Matrix<double> basis = Matrix<double>.Build.Dense(rows, k);
        for (int col = 0; col < k; col++) {
            Vector<double> v = snapshots.Column(col);
            for (int prior = 0; prior < col; prior++) {
                Vector<double> q = basis.Column(prior);
                v -= q * q.DotProduct(v);
            }
            double norm = v.L2Norm();
            basis.SetColumn(col, norm > 1e-12 ? v / norm : v);
        }
        return new RomBasis(basis, [], k, 1.0);
    }

    static RomBasis OrthonormalDeim(Matrix<double> snapshots, int rank) {
        Svd<double> svd = snapshots.Svd(computeVectors: true);
        int k = Math.Min(rank, svd.U.ColumnCount);
        Matrix<double> u = svd.U.SubMatrix(0, svd.U.RowCount, 0, k);
        long[] interpolation = new long[k];
        interpolation[0] = MaxAbsRow(u.Column(0));
        for (int j = 1; j < k; j++) {
            Vector<double> uj = u.Column(j);
            Matrix<double> uPrev = u.SubMatrix(0, u.RowCount, 0, j);
            Matrix<double> pT = Matrix<double>.Build.Dense(j, j, (r, c) => uPrev[(int)interpolation[r], c]);
            Vector<double> rhs = Vector<double>.Build.Dense(j, r => uj[(int)interpolation[r]]);
            Vector<double> coeff = pT.Solve(rhs);
            Vector<double> residual = uj - uPrev * coeff;
            interpolation[j] = MaxAbsRow(residual);
        }
        return new RomBasis(u, [.. interpolation], k, EnergyFraction(svd, k));
    }

    static RomBasis OrthonormalPod(Matrix<double> snapshots, int rank) {
        Svd<double> svd = snapshots.Svd(computeVectors: true);
        double total = svd.S.Sum(static s => s * s);
        double accumulated = 0.0;
        int k = 0;
        while (k < svd.U.ColumnCount && k < rank && accumulated / Math.Max(1e-12, total) < 0.999) { accumulated += svd.S[k] * svd.S[k]; k++; }
        Matrix<double> basis = svd.U.SubMatrix(0, svd.U.RowCount, 0, Math.Max(1, k));
        return new RomBasis(basis, [], Math.Max(1, k), accumulated / Math.Max(1e-12, total));
    }

    static double EnergyFraction(Svd<double> svd, int k) {
        double total = svd.S.Sum(static s => s * s), retained = 0.0;
        for (int i = 0; i < k && i < svd.S.Count; i++) { retained += svd.S[i] * svd.S[i]; }
        return retained / Math.Max(1e-12, total);
    }

    static long MaxAbsRow(Vector<double> column) {
        int index = 0;
        double best = -1.0;
        for (int row = 0; row < column.Count; row++) {
            double magnitude = Math.Abs(column[row]);
            if (magnitude > best) { best = magnitude; index = row; }
        }
        return index;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DesignVariable {
    private DesignVariable() { }

    public sealed record Continuous(string Name, double Lower, double Upper) : DesignVariable;
    public sealed record Integer(string Name, long Lower, long Upper) : DesignVariable;
    public sealed record Categorical(string Name, Seq<string> Choices) : DesignVariable;
    public sealed record Density(string Name, long Cells) : DesignVariable;
    public sealed record Linked(string Name, int Source, double Scale, double Offset) : DesignVariable;

    public string VariableName =>
        Switch(continuous: static c => c.Name, integer: static i => i.Name, categorical: static c => c.Name, density: static d => d.Name, linked: static l => l.Name);

    public long Cardinality =>
        Switch(continuous: static _ => 1L, integer: static i => i.Upper - i.Lower + 1, categorical: static c => c.Choices.Count, density: static d => d.Cells, linked: static _ => 0L);

    public bool Free => Switch(continuous: static _ => true, integer: static _ => true, categorical: static _ => true, density: static _ => true, linked: static _ => false);

    public double Lower =>
        Switch(continuous: static c => c.Lower, integer: static i => (double)i.Lower, categorical: static _ => 0.0, density: static _ => 0.0, linked: static l => l.Offset);

    public double Width =>
        Switch(continuous: static c => c.Upper - c.Lower, integer: static i => (double)(i.Upper - i.Lower), categorical: static c => c.Choices.Count, density: static _ => 1.0, linked: static _ => 0.0);

    public double Clamp(double value) =>
        Switch(
            state: value,
            continuous: static (x, c) => Math.Clamp(x, c.Lower, c.Upper),
            integer: static (x, i) => Math.Clamp(Math.Round(x), i.Lower, i.Upper),
            categorical: static (x, c) => Math.Clamp(Math.Round(x), 0, c.Choices.Count - 1),
            density: static (x, _) => Math.Clamp(x, 0.0, 1.0),
            linked: static (x, _) => x);

    public Option<TensorOpFamily> AdjointOperator =>
        Switch(
            continuous: static _ => Some(TensorOpFamily.Gradient),
            integer: static _ => Option<TensorOpFamily>.None,
            categorical: static _ => Option<TensorOpFamily>.None,
            density: static _ => Some(TensorOpFamily.CotangentLaplacian),
            linked: static _ => Option<TensorOpFamily>.None);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ActivationRule {
    private ActivationRule() { }

    public sealed record Always : ActivationRule;
    public sealed record WhenAbove(int Source, double Threshold) : ActivationRule;
    public sealed record WhenBelow(int Source, double Threshold) : ActivationRule;
    public sealed record WhenChoice(int Source, int Choice) : ActivationRule;

    public bool Active(ReadOnlySpan<double> coordinates) =>
        Switch(
            state: coordinates.ToArray(),
            always: static (_, _) => true,
            whenAbove: static (coords, r) => r.Source < coords.Length && coords[r.Source] >= r.Threshold,
            whenBelow: static (coords, r) => r.Source < coords.Length && coords[r.Source] <= r.Threshold,
            whenChoice: static (coords, r) => r.Source < coords.Length && (int)Math.Round(coords[r.Source]) == r.Choice);
}

public sealed record LinearRow(ImmutableArray<double> Coefficients, double Lower, double Upper);

public sealed record LinearModel(ImmutableArray<double> Objective, Seq<LinearRow> Rows);

public readonly record struct DesignPoint(ImmutableArray<double> Coordinates, ImmutableArray<double> Objectives, ImmutableArray<double> Constraints) {
    public bool Dominates(DesignPoint other, ReadOnlySpan<double> senses) {
        bool better = false;
        for (int axis = 0; axis < Objectives.Length && axis < senses.Length; axis++) {
            double self = Objectives[axis] * senses[axis], rival = other.Objectives[axis] * senses[axis];
            if (self > rival) { return false; }
            better |= self < rival;
        }
        return better;
    }

    public bool Feasible => Constraints.IsDefaultOrEmpty || Constraints.All(static c => c <= 0.0);
    public double Violation => Constraints.IsDefaultOrEmpty ? 0.0 : Constraints.Sum(static c => Math.Max(0.0, c));
}

public sealed record DesignProblem(
    Seq<DesignVariable> Variables,
    Seq<ActivationRule> Activation,
    Seq<ObjectiveSense> Objectives,
    int Constraints,
    ConstraintHandling Handling,
    Option<MeshAdjointSnapshot> DesignMesh,
    Seq<GeometryTape> AdjointTape,
    Option<LinearModel> Exact) {
    public static DesignProblem Of(Seq<DesignVariable> variables, Seq<ObjectiveSense> objectives, int constraints, ConstraintHandling handling, Option<MeshAdjointSnapshot> designMesh = default, Option<LinearModel> exact = default) =>
        new(variables, variables.Map(static _ => (ActivationRule)new ActivationRule.Always()), objectives, constraints, handling, designMesh, Lower(variables, designMesh), exact);

    public static DesignProblem Conditional(Seq<DesignVariable> variables, Seq<ActivationRule> activation, Seq<ObjectiveSense> objectives, int constraints, ConstraintHandling handling, Option<MeshAdjointSnapshot> designMesh = default, Option<LinearModel> exact = default) =>
        new(variables, activation, objectives, constraints, handling, designMesh, Lower(variables, designMesh), exact);

    static Seq<GeometryTape> Lower(Seq<DesignVariable> variables, Option<MeshAdjointSnapshot> designMesh) =>
        designMesh.Case is MeshAdjointSnapshot mesh
            ? variables.Filter(static v => v.Free)
                .Bind(v => v.AdjointOperator.Match(Some: op => Seq(new GeometryTape(op, mesh)), None: () => Seq<GeometryTape>()))
            : Seq<GeometryTape>();

    public ImmutableArray<double> Senses => [.. Objectives.Map(static o => o.Sign)];

    public Fin<Unit> Validate() {
        bool variableShape = Variables.IsEmpty
            || Variables.Exists(static variable => variable switch {
                DesignVariable.Continuous c => !double.IsFinite(c.Lower) || !double.IsFinite(c.Upper) || c.Lower >= c.Upper,
                DesignVariable.Integer i => i.Lower > i.Upper,
                DesignVariable.Categorical c => c.Choices.IsEmpty,
                DesignVariable.Density d => d.Cells <= 0,
                DesignVariable.Linked l => l.Source < 0 || !double.IsFinite(l.Scale) || !double.IsFinite(l.Offset),
                _ => true,
            });
        bool links = Variables.Map((variable, axis) => variable is DesignVariable.Linked link && (link.Source >= axis || link.Source >= Variables.Count)).Exists(static invalid => invalid);
        bool activation = Activation.Count != Variables.Count
            || Activation.Exists(rule => rule.Switch(
                always: static _ => false,
                whenAbove: r => r.Source < 0 || r.Source >= Variables.Count || !double.IsFinite(r.Threshold),
                whenBelow: r => r.Source < 0 || r.Source >= Variables.Count || !double.IsFinite(r.Threshold),
                whenChoice: r => r.Source < 0 || r.Source >= Variables.Count || r.Choice < 0));
        bool objectives = Objectives.IsEmpty || Constraints < 0;
        bool exact = Exact.Exists(model => model.Objective.Length != Variables.Count
            || model.Rows.Exists(row => row.Coefficients.Length != Variables.Count || !double.IsFinite(row.Lower) || !double.IsFinite(row.Upper) || row.Lower > row.Upper));
        return variableShape || links || activation || objectives || exact
            ? Fin.Fail<Unit>(ComputeFault.Create("<optimizer-invalid-design-problem>"))
            : Fin.Succ(unit);
    }

    public ImmutableArray<double> Resolve(ImmutableArray<double> raw) {
        double[] resolved = raw.ToArray();
        for (int axis = 0; axis < Variables.Count; axis++) {
            if (Variables[axis] is DesignVariable.Linked link) {
                double source = link.Source < resolved.Length ? resolved[link.Source] : 0.0;
                resolved[axis] = link.Scale * source + link.Offset;
            }
        }
        for (int axis = 0; axis < Variables.Count && axis < Activation.Count; axis++) {
            if (!Activation[axis].Active(resolved)) { resolved[axis] = 0.0; }
        }
        return [.. resolved];
    }
}

public sealed record OptimizerPolicy(
    OptimizerKind Kind,
    StepControl StepControl,
    AcquisitionFunction Acquisition,
    int Population,
    int Generations,
    double CrossoverRate,
    double MutationRate,
    double SimpPenalty,
    double VolumeFraction,
    double PenaltyWeight,
    double TrustRadius,
    Option<Surrogate> Surrogate,
    double SurrogateErrorBound,
    double IntegerStep,
    double SolveSeconds,
    OptimizerKind MultiStartInner,
    int Restarts,
    int Seed,
    Seq<RandomVariable> Uncertainties,
    double ReliabilityTarget) {
    public static readonly OptimizerPolicy CanonicalNsga = new(
        OptimizerKind.Nsga2, StepControl.Fixed, AcquisitionFunction.ExpectedImprovement, Population: 100, Generations: 250, CrossoverRate: 0.9, MutationRate: 0.1,
        SimpPenalty: 3.0, VolumeFraction: 0.4, PenaltyWeight: 1e6, TrustRadius: 1.0, Surrogate: None, SurrogateErrorBound: 0.05,
        IntegerStep: 1.0, SolveSeconds: 30.0, MultiStartInner: OptimizerKind.CmaEs, Restarts: 8, Seed: 0x5EED, Uncertainties: Seq<RandomVariable>(), ReliabilityTarget: 3.0);
    public static readonly OptimizerPolicy CanonicalAdjoint = CanonicalNsga with { Kind = OptimizerKind.GradientAdjoint, StepControl = StepControl.TrustRegion };
    public static readonly OptimizerPolicy CanonicalBayesian = CanonicalNsga with { Kind = OptimizerKind.BayesianGp, Population = 32, Generations = 64 };
    public static readonly OptimizerPolicy CanonicalCma = CanonicalNsga with { Kind = OptimizerKind.CmaEs, Population = 16, Generations = 200, TrustRadius = 0.3 };
    public static readonly OptimizerPolicy CanonicalPso = CanonicalNsga with { Kind = OptimizerKind.Pso, Population = 40, Generations = 200 };
    public static readonly OptimizerPolicy CanonicalAnneal = CanonicalNsga with { Kind = OptimizerKind.SimulatedAnnealing, Population = 8, Generations = 500, MutationRate = 0.2 };
    public static readonly OptimizerPolicy CanonicalCpSat = CanonicalNsga with { Kind = OptimizerKind.CpSat, Generations = 1, IntegerStep = 0.01 };
    public static readonly OptimizerPolicy CanonicalMilp = CanonicalNsga with { Kind = OptimizerKind.Milp, Generations = 1 };
    public static readonly OptimizerPolicy CanonicalMultiStart = CanonicalNsga with { Kind = OptimizerKind.MultiStartGlobal };
    public static readonly OptimizerPolicy CanonicalRobust = CanonicalNsga with { Kind = OptimizerKind.RobustMinimax };

    public Fin<Unit> Validate() =>
        Population <= 0 || Generations <= 0 || Restarts <= 0
        || !double.IsFinite(CrossoverRate) || CrossoverRate is < 0.0 or > 1.0
        || !double.IsFinite(MutationRate) || MutationRate is < 0.0 or > 1.0
        || !double.IsFinite(SimpPenalty) || SimpPenalty <= 0.0
        || !double.IsFinite(VolumeFraction) || VolumeFraction is <= 0.0 or > 1.0
        || !double.IsFinite(PenaltyWeight) || PenaltyWeight <= 0.0
        || !double.IsFinite(TrustRadius) || TrustRadius <= 0.0
        || !double.IsFinite(SurrogateErrorBound) || SurrogateErrorBound < 0.0
        || !double.IsFinite(IntegerStep) || IntegerStep <= 0.0
        || !double.IsFinite(SolveSeconds) || SolveSeconds <= 0.0
        || !double.IsFinite(ReliabilityTarget)
            ? Fin.Fail<Unit>(ComputeFault.Create("<optimizer-invalid-policy>"))
            : Fin.Succ(unit);
}

public sealed record ParetoFront(Seq<DesignPoint> Points, ImmutableArray<double> Senses) {
    public ParetoFront Insert(DesignPoint candidate) =>
        Points.Exists(p => p.Dominates(candidate, Senses.AsSpan()))
            ? this
            : this with { Points = Points.Filter(p => !candidate.Dominates(p, Senses.AsSpan())).Add(candidate) };

    public double Hypervolume(ReadOnlySpan<double> reference) {
        if (Points.IsEmpty) { return 0.0; }
        int objectives = Points[0].Objectives.Length;
        return objectives == 2 ? Hypervolume2D(reference) : HypervolumeEstimate(reference, objectives);
    }

    double Hypervolume2D(ReadOnlySpan<double> reference) {
        double r0 = reference[0], r1 = reference[1];
        (double X, double Y)[] staircase = Points
            .Map(p => (X: p.Objectives[0] * Senses[0], Y: p.Objectives[1] * Senses[1]))
            .OrderByDescending(static p => p.X)
            .ToArray();
        double area = 0.0, prevX = r0;
        foreach ((double X, double Y) point in staircase) {
            if (point.X >= prevX) { continue; }
            area += (prevX - point.X) * Math.Max(0.0, r1 - point.Y);
            prevX = point.X;
        }
        return area;
    }

    double HypervolumeEstimate(ReadOnlySpan<double> reference, int objectives) {
        double[] refCopy = reference.ToArray();
        double[] low = new double[objectives];
        for (int axis = 0; axis < objectives; axis++) { low[axis] = Points.Min(p => p.Objectives[axis] * Senses[axis]); }
        double boxVolume = 1.0;
        for (int axis = 0; axis < objectives; axis++) { boxVolume *= Math.Max(0.0, refCopy[axis] - low[axis]); }
        if (boxVolume <= 0.0) { return 0.0; }
        Random rng = new(0x4D5);
        const int samples = 4096;
        int dominated = 0;
        double[] probe = new double[objectives];
        for (int s = 0; s < samples; s++) {
            for (int axis = 0; axis < objectives; axis++) { probe[axis] = low[axis] + rng.NextDouble() * (refCopy[axis] - low[axis]); }
            if (Points.Exists(p => Encloses(p, probe))) { dominated++; }
        }
        return boxVolume * dominated / samples;
    }

    bool Encloses(DesignPoint point, ReadOnlySpan<double> probe) {
        for (int axis = 0; axis < probe.Length; axis++) {
            if (point.Objectives[axis] * Senses[axis] > probe[axis]) { return false; }
        }
        return true;
    }

    public ImmutableArray<double> Crowding() {
        int n = Points.Count, objectives = Points.IsEmpty ? 0 : Points[0].Objectives.Length;
        double[] distance = new double[n];
        for (int axis = 0; axis < objectives; axis++) {
            int[] order = Enumerable.Range(0, n).OrderBy(i => Points[i].Objectives[axis]).ToArray();
            distance[order[0]] = distance[order[^1]] = double.MaxValue;
            double range = Math.Max(1e-12, Points[order[^1]].Objectives[axis] - Points[order[0]].Objectives[axis]);
            for (int rank = 1; rank < n - 1; rank++) { distance[order[rank]] += (Points[order[rank + 1]].Objectives[axis] - Points[order[rank - 1]].Objectives[axis]) / range; }
        }
        return [.. distance];
    }
}

public sealed record KernelRun(ParetoFront Front, int Generations, double TrustRadius, Seq<double> Violation);

public sealed record OptimizationResult(
    OptimizerKind Kind,
    ParetoFront Front,
    int Generations,
    int Evaluations,
    int SurrogateHits,
    double Hypervolume,
    Seq<double> ViolationHistory,
    double TrustRadius,
    Instant At);

public sealed record RomBasis(Matrix<double> Modes, ImmutableArray<long> Interpolation, int Rank, double EnergyFraction) {
    public ReadOnlyMemory<double> Project(ReadOnlySpan<double> full) =>
        Modes.TransposeThisAndMultiply(Vector<double>.Build.DenseOfArray(full.ToArray())).ToArray().AsMemory();

    public ReadOnlyMemory<double> Lift(ReadOnlySpan<double> reduced) =>
        (Modes * Vector<double>.Build.DenseOfArray(reduced.ToArray())).ToArray().AsMemory();
}

public sealed record GpModel(Cholesky<double> Factor, Vector<double> Alpha, Matrix<double> X, double LengthScale, double SignalVar, double NoiseVar, double LogMarginal) {
    public (double Mean, double Variance) Posterior(ReadOnlySpan<double> query) {
        Vector<double> k = Vector<double>.Build.Dense(X.RowCount, row => Kernel(X.Row(row), query, LengthScale, SignalVar));
        double mean = k.DotProduct(Alpha);
        double variance = SignalVar + NoiseVar - k.DotProduct(Factor.Solve(k));
        return (mean, Math.Max(0.0, variance));
    }

    public static double Kernel(Vector<double> a, ReadOnlySpan<double> b, double lengthScale, double signalVar) {
        double sq = 0.0;
        for (int i = 0; i < a.Count && i < b.Length; i++) { double d = a[i] - b[i]; sq += d * d; }
        return signalVar * Math.Exp(-0.5 * sq / (lengthScale * lengthScale));
    }
}

public sealed record RbfModel(RbfFit Fit, double LengthScale, double ResidualRms) {
    public (double Mean, double Bound) Posterior(ReadOnlySpan<double> query) {
        Vector<double> q = Vector<double>.Build.Dense(Fit.Centres.ColumnCount, i => i < query.Length ? query[i] : 0.0);
        double mean = Fit.Evaluate(Matrix<double>.Build.DenseOfRowVectors(q))[0, 0];
        double nearest = double.MaxValue;
        for (int centre = 0; centre < Fit.Centres.RowCount; centre++) {
            nearest = Math.Min(nearest, (Fit.Centres.Row(centre) - q).L2Norm());
        }
        return (mean, ResidualRms * (1.0 + nearest / Math.Max(1e-9, LengthScale)));
    }
}

public sealed record Surrogate(
    SurrogateKind Kind,
    ReadOnlyMemory<double> Weights,
    double Intercept,
    ReadOnlyMemory<double> Centroid,
    double SpreadScale,
    double ResidualRms,
    Option<RomBasis> Reduction,
    Option<GpModel> Gp,
    Option<RbfModel> Rbf,
    Option<NeuralFieldModel> Field,
    Option<(InferenceSession Session, RunOptions Options, CancelScope Scope)> Lane) {
    public Fin<(Seq<double> Values, double Bound)> Predict(DesignPoint point) =>
        (Field, Lane, Gp, Rbf) switch {
            ({ IsSome: true, Case: NeuralFieldModel field }, { IsSome: true, Case: (InferenceSession session, RunOptions options, CancelScope scope) }, _, _) =>
                field.Predict(session, options, scope, point.Coordinates.AsSpan()),
            (_, _, { IsSome: true, Case: GpModel gp }, _) =>
                gp.Posterior(point.Coordinates.AsSpan()) switch { (double mean, double variance) => Fin.Succ((Seq(mean), Math.Sqrt(variance))) },
            (_, _, _, { IsSome: true, Case: RbfModel rbf }) =>
                rbf.Posterior(point.Coordinates.AsSpan()) switch { (double mean, double bound) => Fin.Succ((Seq(mean), bound)) },
            _ => Fin.Succ(LinearPredict(point)),
        };

    (Seq<double> Values, double Bound) LinearPredict(DesignPoint point) {
        double mean = Intercept;
        for (int axis = 0; axis < Weights.Length && axis < point.Coordinates.Length; axis++) { mean += Weights.Span[axis] * point.Coordinates[axis]; }
        double leverage = 0.0;
        for (int axis = 0; axis < Centroid.Length && axis < point.Coordinates.Length; axis++) {
            double delta = point.Coordinates[axis] - Centroid.Span[axis];
            leverage += delta * delta;
        }
        double bound = ResidualRms * (1.0 + Math.Sqrt(leverage) / Math.Max(1e-9, SpreadScale));
        return (Seq(mean), bound);
    }

    public Surrogate Reduce(Orthogonalization scheme, Matrix<double> snapshots, int rank) {
        RomBasis basis = scheme.Reduce(snapshots, rank);
        double bound = ReconstructionError(basis, snapshots);
        return this with { Reduction = Some(basis with { EnergyFraction = Math.Min(basis.EnergyFraction, 1.0 - bound) }) };
    }

    static double ReconstructionError(RomBasis basis, Matrix<double> snapshots) {
        double residual = 0.0, total = 0.0;
        for (int column = 0; column < snapshots.ColumnCount; column++) {
            Vector<double> full = snapshots.Column(column);
            Vector<double> rebuilt = Vector<double>.Build.DenseOfArray(basis.Lift(basis.Project(full.AsArray()).Span).ToArray());
            residual += (full - rebuilt).L2Norm(); total += full.L2Norm();
        }
        return total > 1e-12 ? residual / total : 0.0;
    }

    public static Fin<Surrogate> Fit(SurrogateKind kind, Seq<DesignPoint> history, int objective) =>
        kind.Switch(
            state: (History: history, Objective: objective),
            linearTrend: static s => Fin.Succ(FitLinear(SurrogateKind.LinearTrend, s.History, s.Objective)),
            gaussianProcess: static s => Fin.Succ(FitGaussianProcess(s.History, s.Objective)),
            radialBasis: static s => FitRadialBasis(s.History, s.Objective),
            neuralField: static s => Fin.Succ(FitLinear(SurrogateKind.NeuralField, s.History, s.Objective)));

    public static Surrogate OfField(NeuralFieldModel field, InferenceSession session, RunOptions options, CancelScope scope) =>
        new(SurrogateKind.NeuralField, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, 1.0, field.TrainedResidualRms, None, None, None,
            Some(field), Some((session, options, scope)));

    // Multi-fidelity fusion over the ONE evaluate contract: the composed arrow IS Func<DesignPoint, Fin<Seq<double>>>,
    // so every OptimizerKind row searches it unchanged — a fidelity discriminant on the contract, never a second oracle.
    // The cheap leg (Analysis closed-form folds) runs every point; the expensive leg (full FEA/energy solve) spends its
    // budget building paired evaluations; per-objective additive-correction surrogates fuse both, and a correction
    // whose bound exceeds the policy forces the expensive leg while budget remains.
    public static Func<DesignPoint, Fin<Seq<double>>> Fused(
        Func<DesignPoint, Fin<Seq<double>>> low,
        Func<DesignPoint, Fin<Seq<double>>> high,
        FidelityPolicy policy,
        Atom<FidelityState> state) =>
        point => low(point).Bind(cheap =>
            Corrected(state.Value, point, cheap, policy).Match(
                Some: Fin.Succ,
                None: () => state.Value.HighSpent >= policy.HighBudget
                    ? Fin.Succ(cheap)                                            // budget spent, correction unproven: the cheap leg answers raw and HighSpent keeps the degradation auditable
                    : high(point).Map(exact => {
                        ignore(state.Swap(held => held.Paired(point, cheap, exact, policy)));
                        return exact;
                    })));

    static Option<Seq<double>> Corrected(FidelityState state, DesignPoint point, Seq<double> cheap, FidelityPolicy policy) =>
        state.Corrections.Count == cheap.Count && !state.Corrections.IsEmpty
            ? state.Corrections
                .Traverse(correction => correction.Predict(point).ToOption()
                    .Filter(row => row.Values.Count == 1 && row.Bound <= policy.CorrectionBound)
                    .Map(static row => row.Values[0]))
                .As()
                .Map(deltas => cheap.Zip(deltas).Map(static pair => pair.First + pair.Second).ToSeq())
            : None;
}

// Fidelity budget policy: the expensive-leg call budget, the refit cadence over accumulated pairs, and the
// correction-bound ceiling above which fusion refuses and the expensive leg runs instead.
public sealed record FidelityPolicy(int HighBudget, int RefitEvery, double CorrectionBound) {
    public static readonly FidelityPolicy Canonical = new(HighBudget: 32, RefitEvery: 8, CorrectionBound: 0.1);

    public bool Invalid => HighBudget < 1 || RefitEvery < 1 || !double.IsFinite(CorrectionBound) || CorrectionBound <= 0.0;
}

// Paired-evaluation state: delta points (coordinates, high − low per objective) and the per-objective correction
// surrogates refit on the policy cadence; one Atom at the composition boundary, never interior mutation.
public sealed record FidelityState(Seq<(DesignPoint Point, Seq<double> Delta)> Pairs, Seq<Surrogate> Corrections, int HighSpent) {
    public static readonly FidelityState Empty = new(Seq<(DesignPoint, Seq<double>)>(), Seq<Surrogate>(), 0);

    public FidelityState Paired(DesignPoint point, Seq<double> cheap, Seq<double> exact, FidelityPolicy policy) {
        Seq<(DesignPoint Point, Seq<double> Delta)> pairs = Pairs.Add((point, exact.Zip(cheap).Map(static pair => pair.First - pair.Second).ToSeq()));
        Seq<Surrogate> corrections = pairs.Count % policy.RefitEvery == 0 && !pairs.IsEmpty
            ? toSeq(Enumerable.Range(0, pairs.Head.Map(static row => row.Delta.Count).IfNone(0))).Choose(objective =>
                Surrogate.Fit(
                    SurrogateKind.GaussianProcess,
                    pairs.Map(pair => new DesignPoint(pair.Point.Coordinates, [.. pair.Delta], [])),
                    objective).ToOption())
            : Corrections;
        return new FidelityState(pairs, corrections, HighSpent + 1);
    }
}

    static Fin<Surrogate> FitRadialBasis(Seq<DesignPoint> history, int objective) {
        if (history.Count < 2) { return Fin.Succ(FitLinear(SurrogateKind.RadialBasis, history, objective)); }
        int n = history.Count, dim = history[0].Coordinates.Length;
        double lengthScale = MedianPairwise(history);
        double shape = 1.0 / (Math.Sqrt(2.0) * Math.Max(1e-9, lengthScale));
        Matrix<double> centres = Matrix<double>.Build.Dense(n, dim, (r, c) => history[r].Coordinates[c]);
        Matrix<double> response = Matrix<double>.Build.Dense(n, 1, (r, _) => history[r].Objectives[objective]);
        return Scatter.Fit(centres, centres, response, RbfKernel.Gaussian, shape, TolerancePolicy.Derive(centres, response.Column(0)))
            .Map(fit => {
                Matrix<double> fitted = fit.Evaluate(centres);
                double rms = Math.Sqrt(toSeq(Enumerable.Range(0, n)).Sum(i => {
                    double e = history[i].Objectives[objective] - fitted[i, 0];
                    return e * e;
                }) / n);
                return new Surrogate(SurrogateKind.RadialBasis, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, lengthScale, rms, None, None,
                    Some(new RbfModel(fit, lengthScale, rms)), None, None);
            });
    }

    static Surrogate FitGaussianProcess(Seq<DesignPoint> history, int objective) {
        if (history.Count < 2) { return FitLinear(SurrogateKind.LinearTrend, history, objective); }
        int n = history.Count, dim = history[0].Coordinates.Length;
        double lengthScale = MedianPairwise(history), signalVar = ObjectiveVariance(history, objective), noiseVar = 1e-6 * Math.Max(1e-9, signalVar);
        Matrix<double> x = Matrix<double>.Build.Dense(n, dim, (r, c) => history[r].Coordinates[c]);
        Vector<double> y = Vector<double>.Build.Dense(n, r => history[r].Objectives[objective]);
        Matrix<double> kMatrix = Matrix<double>.Build.Dense(n, n, (r, c) =>
            GpModel.Kernel(x.Row(r), x.Row(c).AsArray(), lengthScale, signalVar) + (r == c ? noiseVar : 0.0));
        Cholesky<double> chol = kMatrix.Cholesky();
        Vector<double> alpha = chol.Solve(y);
        double logMarginal = -0.5 * y.DotProduct(alpha) - 0.5 * chol.DeterminantLn - 0.5 * n * Math.Log(2.0 * Math.PI);
        return new Surrogate(SurrogateKind.GaussianProcess, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, lengthScale, Math.Sqrt(signalVar), None,
            Some(new GpModel(chol, alpha, x, lengthScale, signalVar, noiseVar, logMarginal)), None, None, None);
    }

    static Surrogate FitLinear(SurrogateKind kind, Seq<DesignPoint> history, int objective) {
        if (history.IsEmpty) { return new(kind, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, 1.0, double.MaxValue, None, None, None, None, None); }
        int dim = history[0].Coordinates.Length;
        double[] centroid = new double[dim];
        history.Iter(point => { for (int axis = 0; axis < dim; axis++) { centroid[axis] += point.Coordinates[axis] / history.Count; } });
        double meanObjective = history.Average(point => point.Objectives[objective]);
        double[] weights = new double[dim];
        double[] variance = new double[dim];
        history.Iter(point => {
            double dy = point.Objectives[objective] - meanObjective;
            for (int axis = 0; axis < dim; axis++) { double dx = point.Coordinates[axis] - centroid[axis]; weights[axis] += dx * dy; variance[axis] += dx * dx; }
        });
        for (int axis = 0; axis < dim; axis++) { weights[axis] /= Math.Max(1e-12, variance[axis]); }
        double intercept = meanObjective - toSeq(Enumerable.Range(0, dim)).Sum(axis => weights[axis] * centroid[axis]);
        double residual = Math.Sqrt(history.Average(point => {
            double prediction = intercept + toSeq(Enumerable.Range(0, dim)).Sum(axis => weights[axis] * point.Coordinates[axis]);
            double e = point.Objectives[objective] - prediction;
            return e * e;
        }));
        double spread = Math.Sqrt(TensorPrimitives.Sum<double>(variance) / Math.Max(1, history.Count));
        return new(kind, weights.AsMemory(), intercept, centroid.AsMemory(), Math.Max(1e-9, spread), residual, None, None, None, None, None);
    }

    static double MedianPairwise(Seq<DesignPoint> history) {
        List<double> distances = [];
        for (int i = 0; i < history.Count; i++)
            for (int j = i + 1; j < history.Count; j++) {
                double sq = 0.0;
                for (int axis = 0; axis < history[i].Coordinates.Length; axis++) { double d = history[i].Coordinates[axis] - history[j].Coordinates[axis]; sq += d * d; }
                distances.Add(Math.Sqrt(sq));
            }
        distances.Sort();
        return distances.Count == 0 ? 1.0 : Math.Max(1e-6, distances[distances.Count / 2]);
    }

    static double ObjectiveVariance(Seq<DesignPoint> history, int objective) {
        double[] objectives = history.Map(p => p.Objectives[objective]).ToArray();
        double sigma = TensorPrimitives.StdDev<double>(objectives);
        return Math.Max(1e-9, sigma * sigma);
    }
}

public sealed record NeuralFieldModel(
    UInt128 FamilyDigest,
    ModelIdentity Model,
    ExecutionProvider Ep,
    string InputName,
    string OutputName,
    int CoordinateRank,
    int FieldComponents,
    double TrainedResidualRms) {
    public Fin<(Seq<double> Values, double Bound)> Predict(InferenceSession session, RunOptions options, CancelScope scope, ReadOnlySpan<double> coordinates) {
        double leverage = 0.0;
        for (int axis = 0; axis < coordinates.Length && axis < CoordinateRank; axis++) { leverage += coordinates[axis] * coordinates[axis]; }
        double bound = TrainedResidualRms * (1.0 + Math.Sqrt(leverage));
        float[] features = [.. coordinates[..Math.Min(coordinates.Length, CoordinateRank)].ToArray().Select(static x => (float)x)];
        return RunOps.Bind(new RunInput.Managed<float>(InputName, features, [1, CoordinateRank])).Bind(inputs =>
            session.Infer(options, scope, inputs, Seq(OutputName),
                results => {
                    ReadOnlySpan<float> field = results.First().GetTensorDataAsSpan<float>();
                    return Fin.Succ((toSeq(field[..Math.Min(field.Length, FieldComponents)].ToArray().Select(static v => (double)v)), bound));
                }));
    }
}

public static class Optimizer {
    public static Fin<OptimizationResult> Optimize(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ClockPolicy clocks) =>
        from _problem in problem.Validate()
        from _policy in policy.Validate()
        from result in Run(problem, policy, evaluate, clocks)
        select result;

    static Fin<KernelRun> Invoke(OptimizerKind kind, DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        kind.Switch(
            state: (Problem: problem, Policy: policy, Oracle: oracle, Seed: seed),
            nsga2: static s => GeneticEngine.Evolve(s.Problem, s.Policy, s.Oracle, s.Seed),
            bayesianGp: static s => AcquireBayesian(s.Problem, s.Policy, s.Oracle, s.Seed),
            gradientAdjoint: static s => DescendAdjoint(s.Problem, s.Policy, s.Oracle, s.Seed),
            topologySimp: static s => OptimalityCriteria(s.Problem, s.Policy, s.Oracle, s.Seed),
            simulatedAnnealing: static s => Anneal(s.Problem, s.Policy, s.Oracle, s.Seed),
            cmaEs: static s => EvolveCma(s.Problem, s.Policy, s.Oracle, s.Seed),
            pso: static s => EvolvePso(s.Problem, s.Policy, s.Oracle, s.Seed),
            cpSat: static s => SolveCpSat(s.Problem, s.Policy, s.Oracle, s.Seed),
            milp: static s => SolveMilp(s.Problem, s.Policy, s.Oracle, s.Seed),
            multiStartGlobal: static s => MultiStart(s.Problem, s.Policy, s.Oracle, s.Seed),
            robustMinimax: static s => RobustMinimax(s.Problem, s.Policy, s.Oracle, s.Seed));

    static Fin<OptimizationResult> Run(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ClockPolicy clocks) {
        Atom<(int Evals, int Hits)> meter = Atom((Evals: 0, Hits: 0));
        Func<DesignPoint, Fin<Seq<double>>> oracle = Gated(problem, policy, evaluate, meter);
        return Invoke(policy.Kind, problem, policy, oracle, new ParetoFront(Seq<DesignPoint>(), problem.Senses))
            .Map(run => new OptimizationResult(policy.Kind, run.Front, run.Generations, meter.Value.Evals, meter.Value.Hits,
                run.Front.Hypervolume(Reference(run.Front)), run.Violation, run.TrustRadius, clocks.Now));
    }

    public static ComputeReceipt.Optimization Receipt(OptimizationResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Kind.Key, result.Generations, result.Evaluations, result.SurrogateHits, result.Front.Points.Count, result.Hypervolume) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    static Func<DesignPoint, Fin<Seq<double>>> Gated(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> full, Atom<(int Evals, int Hits)> meter) =>
        policy.Surrogate is { IsSome: true, Case: Surrogate surrogate }
            ? point => surrogate.Predict(point).Bind(prediction => {
                if (prediction.Bound <= policy.SurrogateErrorBound && prediction.Values.Count == problem.Objectives.Count + problem.Constraints && prediction.Values.ForAll(double.IsFinite)) {
                    meter.Swap(static m => (m.Evals + 1, m.Hits + 1));
                    return Fin.Succ(prediction.Values);
                }
                meter.Swap(static m => (m.Evals + 1, m.Hits));
                return full(point);
            })
            : point => { meter.Swap(static m => (m.Evals + 1, m.Hits)); return full(point); };

    internal static Fin<DesignPoint> Probe(DesignProblem problem, Func<DesignPoint, Fin<Seq<double>>> oracle, ImmutableArray<double> raw) {
        ImmutableArray<double> coords = problem.Resolve(raw);
        return oracle(new DesignPoint(coords, [], [])).Bind(values => {
            int m = problem.Objectives.Count;
            return values.Count != m + problem.Constraints || !values.ForAll(double.IsFinite)
                ? Fin.Fail<DesignPoint>(ComputeFault.Create("<optimizer-oracle-shape>"))
                : Fin.Succ(new DesignPoint(coords, [.. values.Take(m)], [.. values.Skip(m)]));
        });
    }

    static double Fitness(DesignProblem problem, OptimizerPolicy policy, DesignPoint point, ReadOnlySpan<double> multipliers) =>
        problem.Handling.Penalize(
            point.Objectives.IsDefaultOrEmpty ? 0.0 : point.Objectives[0] * problem.Senses[0],
            point.Constraints.AsSpan(), policy.PenaltyWeight, multipliers);

    internal static double Worst(ParetoFront front) => front.Points.IsEmpty ? 0.0 : front.Points.Max(static p => p.Violation);

    static double[] Reference(ParetoFront front) =>
        front.Points.IsEmpty
            ? [1.0]
            : [.. toSeq(Enumerable.Range(0, front.Points[0].Objectives.Length))
                .Map(axis => front.Points.Max(point => point.Objectives[axis] * front.Senses[axis]))
                .Map(static worst => worst + 0.1 * Math.Abs(worst) + 0.1)];

    static Fin<KernelRun> AcquireBayesian(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        toSeq(Enumerable.Range(0, Math.Max(1, policy.Generations)))
            .Fold(Fin.Succ((History: seed.Points, Front: seed, Violation: Seq<double>())), (acc, gen) => acc.Bind(state =>
                Surrogate.Fit(SurrogateKind.GaussianProcess, state.History, 0).Bind(gp =>
                    GeneticEngine.Candidates(problem, policy.Population, policy.Seed + gen).Bind(candidates => {
                        double best = state.History.IsEmpty ? 0.0 : state.History.Min(p => p.Objectives.IsDefaultOrEmpty ? double.MaxValue : p.Objectives[0] * problem.Senses[0]);
                        return candidates.Traverse(raw => gp.Predict(new DesignPoint(problem.Resolve(raw), [], []))
                                .Map(prediction => (Raw: raw, Score: policy.Acquisition.Score(prediction.Values.Head.IfNone(best), prediction.Bound, best))))
                            .Bind(scored => scored.OrderByDescending(static row => row.Score).Take(Math.Max(1, policy.Population / 8)).ToSeq()
                                .Fold(Fin.Succ((state.History, state.Front)), (inner, row) => inner.Bind(carry =>
                                    Probe(problem, oracle, row.Raw).Map(point => (carry.History.Add(point), carry.Front.Insert(point))))))
                            .Map(carry => (carry.History, carry.Front, state.Violation.Add(Worst(carry.Front))));
                    }))))
            .Map(state => new KernelRun(state.Front, policy.Generations, policy.TrustRadius, state.Violation));

    static Fin<KernelRun> DescendAdjoint(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        ImmutableArray<double> start = seed.Points.IsEmpty ? [.. problem.Variables.Map(static v => v.Lower)] : seed.Points[0].Coordinates;
        return toSeq(Enumerable.Range(0, Math.Max(1, policy.Generations)))
            .Fold(Fin.Succ((Origin: start, Radius: policy.TrustRadius, Front: seed, Violation: Seq<double>())), (acc, _) => acc.Bind(state =>
                Adjoint(problem, state.Origin).Bind(gradient =>
                    Probe(problem, oracle, state.Origin).Bind(baseline => {
                        double objectiveAtOrigin = baseline.Objectives.IsDefaultOrEmpty ? 0.0 : baseline.Objectives[0];
                        ImmutableArray<double> stepped = Stepped(problem, state.Origin, gradient, state.Radius);
                        double predicted = TensorPrimitives.SumOfSquares<double>([.. gradient]) * state.Radius;
                        return Probe(problem, oracle, stepped).Map(probe => {
                            double actual = objectiveAtOrigin - (probe.Objectives.IsDefaultOrEmpty ? objectiveAtOrigin : probe.Objectives[0]);
                            (double step, double radius) = policy.StepControl.Next(state.Radius, actual, predicted, Math.Max(1e-6, policy.MutationRate));
                            ImmutableArray<double> next = Stepped(problem, state.Origin, gradient, step);
                            ParetoFront front = state.Front.Insert(baseline).Insert(probe);
                            return (next, radius, front, state.Violation.Add(Worst(front)));
                        });
                    }))))
            .Map(state => new KernelRun(state.Front, policy.Generations, state.Radius, state.Violation));
    }

    static ImmutableArray<double> Stepped(DesignProblem problem, ImmutableArray<double> origin, ImmutableArray<double> gradient, double scale) =>
        [.. problem.Variables.Map((v, axis) => v.Clamp(origin.ElementAtOrDefault(axis) - scale * gradient.ElementAtOrDefault(axis)))];

    static Fin<ImmutableArray<double>> Adjoint(DesignProblem problem, ImmutableArray<double> origin) =>
        SensitivityLaw.Chain(problem.AdjointTape, [.. Enumerable.Repeat(1f, origin.Length)])
            .Map(static gradient => (ImmutableArray<double>)[.. gradient.Span.ToArray().Select(static g => (double)g)]);

    static Fin<KernelRun> OptimalityCriteria(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        toSeq(Enumerable.Range(0, Math.Max(1, policy.Generations)))
            .Fold(Fin.Succ((Density: seed.Points.IsEmpty ? (ImmutableArray<double>)[.. problem.Variables.Map(_ => policy.VolumeFraction)] : seed.Points[0].Coordinates, Front: seed, Violation: Seq<double>())),
                (acc, _) => acc.Bind(state =>
                    Adjoint(problem, state.Density).Bind(sensitivity => {
                        ImmutableArray<double> updated = OcUpdate(problem, policy, state.Density, sensitivity);
                        return Probe(problem, oracle, updated).Map(point => {
                            ParetoFront front = state.Front.Insert(point);
                            return (updated, front, state.Violation.Add(Worst(front)));
                        });
                    })))
            .Map(state => new KernelRun(state.Front, policy.Generations, policy.TrustRadius, state.Violation));

    static ImmutableArray<double> OcUpdate(DesignProblem problem, OptimizerPolicy policy, ImmutableArray<double> density, ImmutableArray<double> sensitivity) {
        double lower = 1e-9, upper = 1e9;
        const double move = 0.2, eta = 0.5;
        double[] updated = density.ToArray();
        for (int bisect = 0; bisect < 50 && (upper - lower) / Math.Max(1e-12, lower + upper) > 1e-4; bisect++) {
            double lagrange = 0.5 * (lower + upper);
            for (int e = 0; e < updated.Length; e++) {
                double x = density[e];
                double raw = e < sensitivity.Length ? sensitivity[e] : 0.0;
                double dc = policy.SimpPenalty * Math.Pow(Math.Max(1e-9, x), policy.SimpPenalty - 1.0) * raw;
                double scaled = x * Math.Pow(Math.Max(1e-12, -dc / lagrange), eta);
                updated[e] = problem.Variables[e].Clamp(Math.Clamp(scaled, Math.Max(0.0, x - move), Math.Min(1.0, x + move)));
            }
            if (TensorPrimitives.Average<double>(updated) > policy.VolumeFraction) { lower = lagrange; } else { upper = lagrange; }
        }
        return [.. updated];
    }

    static Fin<KernelRun> SolveCpSat(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        problem.Exact.Match(
            Some: model => {
                CpModel cp = new();
                long q = Math.Max(1L, (long)Math.Round(1.0 / policy.IntegerStep));
                IntVar[] vars = [.. problem.Variables.Map(v => v switch {
                    DesignVariable.Integer i => cp.NewIntVar(i.Lower, i.Upper, i.Name),
                    DesignVariable.Categorical c => cp.NewIntVar(0, c.Choices.Count - 1, c.Name),
                    DesignVariable.Continuous co => cp.NewIntVar((long)(co.Lower / policy.IntegerStep), (long)(co.Upper / policy.IntegerStep), co.Name),
                    _ => cp.NewConstant(0),
                })];
                long[] Scaled(ImmutableArray<double> coefficients) {
                    long[] scaled = new long[vars.Length];
                    for (int axis = 0; axis < vars.Length && axis < coefficients.Length; axis++) {
                        scaled[axis] = (long)Math.Round(problem.Variables[axis] is DesignVariable.Continuous ? coefficients[axis] : coefficients[axis] * q);
                    }
                    return scaled;
                }
                foreach (LinearRow row in model.Rows) {
                    cp.AddLinearConstraint(LinearExpr.WeightedSum(vars, Scaled(row.Coefficients)), (long)Math.Round(row.Lower * q), (long)Math.Round(row.Upper * q));
                }
                cp.Minimize(LinearExpr.WeightedSum(vars, Scaled(model.Objective)));
                using CpSolver solver = new() { StringParameters = $"max_time_in_seconds:{policy.SolveSeconds}" };
                CpSolverStatus status = solver.Solve(cp);
                return status is CpSolverStatus.Optimal or CpSolverStatus.Feasible
                    ? Harvest(problem, policy, oracle, seed, [.. problem.Variables.Map((v, axis) => DiscreteValue(v, solver.Value(vars[axis]), policy.IntegerStep))])
                    : Fin.Fail<KernelRun>(new ComputeFault.ModelRejected($"<cp-sat-infeasible:{status}>"));
            },
            None: () => Fin.Fail<KernelRun>(ComputeFault.Create("<exact-needs-linear-model:cp-sat>")));

    static Fin<KernelRun> SolveMilp(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        problem.Exact.Match(
            Some: model => {
                using Google.OrTools.LinearSolver.Solver? solver = Google.OrTools.LinearSolver.Solver.CreateSolver("SCIP");
                if (solver is null) { return Fin.Fail<KernelRun>(new ComputeFault.ModelRejected("<milp-backend-unavailable:SCIP>")); }
                Google.OrTools.LinearSolver.Variable[] vars = [.. problem.Variables.Map(v => v switch {
                    DesignVariable.Integer i => solver.MakeIntVar(i.Lower, i.Upper, i.Name),
                    DesignVariable.Categorical c => solver.MakeIntVar(0, c.Choices.Count - 1, c.Name),
                    DesignVariable.Continuous co => solver.MakeNumVar(co.Lower, co.Upper, co.Name),
                    _ => solver.MakeNumVar(0.0, 0.0, v.VariableName),
                })];
                foreach (LinearRow row in model.Rows) {
                    Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(row.Lower, row.Upper);
                    for (int axis = 0; axis < vars.Length && axis < row.Coefficients.Length; axis++) { constraint.SetCoefficient(vars[axis], row.Coefficients[axis]); }
                }
                Google.OrTools.LinearSolver.Objective objective = solver.Objective();
                for (int axis = 0; axis < vars.Length && axis < model.Objective.Length; axis++) { objective.SetCoefficient(vars[axis], model.Objective[axis]); }
                objective.SetMinimization();
                solver.SetTimeLimit((long)(policy.SolveSeconds * 1000.0));
                return solver.Solve() is Google.OrTools.LinearSolver.Solver.ResultStatus.OPTIMAL or Google.OrTools.LinearSolver.Solver.ResultStatus.FEASIBLE
                    ? Harvest(problem, policy, oracle, seed, [.. problem.Variables.Map((v, axis) => v.Clamp(vars[axis].SolutionValue()))])
                    : Fin.Fail<KernelRun>(new ComputeFault.ModelRejected("<milp-infeasible:SCIP>"));
            },
            None: () => Fin.Fail<KernelRun>(ComputeFault.Create("<exact-needs-linear-model:milp>")));

    static Fin<KernelRun> Harvest(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed, ImmutableArray<double> coordinates) =>
        Probe(problem, oracle, coordinates).Map(point => new KernelRun(seed.Insert(point), 1, policy.TrustRadius, Seq(point.Violation)));

    static double DiscreteValue(DesignVariable variable, long raw, double step) =>
        variable is DesignVariable.Continuous ? raw * step : variable.Clamp(raw);

    static Fin<KernelRun> MultiStart(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        OptimizerKind inner = policy.MultiStartInner == OptimizerKind.MultiStartGlobal ? OptimizerKind.CmaEs : policy.MultiStartInner;
        return LowDiscrepancy.Sobol(dimensions: problem.Variables.Count, seed: policy.Seed, Scramble.DigitalShift).Bind(generator =>
            toSeq(Enumerable.Range(0, Math.Max(1, policy.Restarts)))
                .Fold(Fin.Succ((Gen: generator, Front: seed, Violation: Seq<double>())),
                    (acc, _) => acc.Bind(state => {
                        (LowDiscrepancy next, double[] point) = state.Gen.Draw();
                        ImmutableArray<double> start = [.. problem.Variables.Map((v, axis) => v.Clamp(v.Lower + (axis < point.Length ? point[axis] : 0.0) * v.Width))];
                        return Probe(problem, oracle, start).Bind(seeded =>
                            Invoke(inner, problem, policy with { Kind = inner }, oracle, state.Front.Insert(seeded))
                                .Map(run => (next, run.Front, state.Violation + run.Violation)));
                    }))
                .Map(state => new KernelRun(state.Front, policy.Restarts * Math.Max(1, policy.Generations), policy.TrustRadius, state.Violation)));
    }

    static Fin<KernelRun> RobustMinimax(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        int extra = policy.Uncertainties.IsEmpty ? 0 : 1;
        DesignProblem robustProblem = problem with { Constraints = problem.Constraints + extra };
        return Scenarios(policy).Bind(scenarios => {
            Func<DesignPoint, Fin<Seq<double>>> robust = point => ScenarioWorst(problem, policy, oracle, point, scenarios);
            return GeneticEngine.Evolve(robustProblem, policy, robust, new ParetoFront(seed.Points, robustProblem.Senses));
        });
    }

    static Fin<Seq<ImmutableArray<double>>> Scenarios(OptimizerPolicy policy) {
        if (policy.Uncertainties.IsEmpty) { return Fin.Succ(Seq<ImmutableArray<double>>()); }
        int dimensions = policy.Uncertainties.Count;
        return LowDiscrepancy.Sobol(dimensions: dimensions, seed: policy.Seed, Scramble.DigitalShift)
            .Map(generator => toSeq(Enumerable.Range(0, Math.Max(1, policy.Population)))
                .Fold((Gen: generator, Rows: Seq<ImmutableArray<double>>()), (acc, _) => {
                    (LowDiscrepancy next, double[] u) = acc.Gen.Draw();
                    ImmutableArray<double> row = [.. policy.Uncertainties.Map((rv, axis) => rv.Quantile(axis < u.Length ? u[axis] : 0.5) - rv.Quantile(0.5))];
                    return (next, acc.Rows.Add(row));
                }).Rows);
    }

    static Fin<Seq<double>> ScenarioWorst(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, DesignPoint point, Seq<ImmutableArray<double>> scenarios) {
        int m = problem.Objectives.Count, k = problem.Constraints;
        double[] worstObj = [.. Enumerable.Range(0, m).Select(i => problem.Senses[i] > 0 ? double.MinValue : double.MaxValue)];
        double[] worstCon = [.. Enumerable.Repeat(double.MinValue, k)];
        Seq<ImmutableArray<double>> probes = scenarios.IsEmpty ? Seq(ImmutableArray<double>.Empty) : scenarios;
        return probes.Fold(Fin.Succ((Fail: 0, Total: 0)), (acc, scenario) => acc.Bind(carry => {
                ImmutableArray<double> shifted = [.. point.Coordinates.Select((c, axis) => c + (axis < scenario.Length ? scenario[axis] : 0.0))];
                return Probe(problem, oracle, shifted).Map(p => {
                    for (int i = 0; i < m; i++) { worstObj[i] = problem.Senses[i] > 0 ? Math.Max(worstObj[i], p.Objectives[i]) : Math.Min(worstObj[i], p.Objectives[i]); }
                    for (int j = 0; j < k; j++) { worstCon[j] = Math.Max(worstCon[j], j < p.Constraints.Length ? p.Constraints[j] : 0.0); }
                    return (carry.Fail + (!p.Constraints.IsDefaultOrEmpty && p.Constraints.Any(static c => c > 0.0) ? 1 : 0), carry.Total + 1);
                });
            }))
            .Map(carry => {
                Seq<double> result = toSeq(worstObj.Concat(worstCon));
                if (policy.Uncertainties.IsEmpty) { return result; }
                double pf = Math.Clamp(carry.Total == 0 ? 0.0 : (double)carry.Fail / carry.Total, 1e-9, 1.0 - 1e-9);
                return result.Add(policy.ReliabilityTarget - Normal.InvCDF(0.0, 1.0, 1.0 - pf));
            });
    }

    // --- [CMA_ES] ------------------------------------------------------------------------------------------

    sealed record CmaState(Vector<double> Mean, double Sigma, Matrix<double> Covariance, Vector<double> PathSigma, Vector<double> PathC, double[] Multipliers, ParetoFront Front, Seq<double> Violation);

    static Fin<KernelRun> EvolveCma(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        DesignVariable[] free = [.. problem.Variables.Filter(static v => v.Free)];
        int n = free.Length;
        if (n == 0) { return Fin.Fail<KernelRun>(ComputeFault.Create("<cma-no-free-variable>")); }
        int lambda = Math.Max(4, policy.Population), mu = Math.Max(1, lambda / 2);
        double[] weights = [.. Enumerable.Range(0, mu).Select(i => Math.Log(mu + 0.5) - Math.Log(i + 1.0))];
        double weightSum = weights.Sum();
        for (int i = 0; i < mu; i++) { weights[i] /= weightSum; }
        double muEff = 1.0 / weights.Sum(static w => w * w);
        double cSigma = (muEff + 2.0) / (n + muEff + 5.0);
        double dSigma = 1.0 + 2.0 * Math.Max(0.0, Math.Sqrt((muEff - 1.0) / (n + 1.0)) - 1.0) + cSigma;
        double cc = (4.0 + muEff / n) / (n + 4.0 + 2.0 * muEff / n);
        double c1 = 2.0 / ((n + 1.3) * (n + 1.3) + muEff);
        double cMu = Math.Min(1.0 - c1, 2.0 * (muEff - 2.0 + 1.0 / muEff) / ((n + 2.0) * (n + 2.0) + muEff));
        double chiN = Math.Sqrt(n) * (1.0 - 1.0 / (4.0 * n) + 1.0 / (21.0 * n * n));
        Random rng = new(policy.Seed);
        CmaState initial = new(
            Vector<double>.Build.Dense(n, i => free[i].Lower + 0.5 * free[i].Width),
            policy.TrustRadius > 1e-9 ? policy.TrustRadius : 0.3,
            Matrix<double>.Build.DenseIdentity(n),
            Vector<double>.Build.Dense(n), Vector<double>.Build.Dense(n),
            new double[problem.Constraints], seed, Seq<double>());
        return toSeq(Enumerable.Range(0, Math.Max(1, policy.Generations)))
            .Fold(Fin.Succ(initial), (acc, gen) => acc.Bind(state =>
                CmaStep(problem, policy, oracle, free, state, gen, rng, n, lambda, mu, weights, muEff, cSigma, dSigma, cc, c1, cMu, chiN)))
            .Map(state => new KernelRun(state.Front, policy.Generations, state.Sigma, state.Violation));
    }

    static Fin<CmaState> CmaStep(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, DesignVariable[] free, CmaState state, int gen, Random rng,
        int n, int lambda, int mu, double[] weights, double muEff, double cSigma, double dSigma, double cc, double c1, double cMu, double chiN) {
        Matrix<double> symmetric = (state.Covariance + state.Covariance.Transpose()) * 0.5;
        Evd<double> evd = symmetric.Evd(Symmetricity.Symmetric);
        Matrix<double> b = evd.EigenVectors;
        double[] d = [.. Enumerable.Range(0, n).Select(i => Math.Sqrt(Math.Max(1e-20, evd.EigenValues[i].Real)))];
        return toSeq(Enumerable.Range(0, lambda))
            .Fold(Fin.Succ((Front: state.Front, Offspring: Seq<(Vector<double> Y, double Fitness, ImmutableArray<double> Constraints)>())),
                (acc, _) => acc.Bind(carry => {
                    Vector<double> y = b * Vector<double>.Build.Dense(n, i => d[i] * Normal.Sample(rng, 0.0, 1.0));
                    ImmutableArray<double> raw = Embed(problem, state.Mean, y, state.Sigma);
                    return Probe(problem, oracle, raw).Map(point => (
                        carry.Front.Insert(point),
                        carry.Offspring.Add((y, Fitness(problem, policy, point, state.Multipliers), point.Constraints))));
                }))
            .Map(carry => {
                (Vector<double> Y, double Fitness, ImmutableArray<double> Constraints)[] ordered = carry.Offspring.OrderBy(static o => o.Fitness).Take(mu).ToArray();
                Vector<double> yMean = Vector<double>.Build.Dense(n);
                for (int i = 0; i < mu; i++) { yMean += weights[i] * ordered[i].Y; }
                Vector<double> meanNew = state.Mean + state.Sigma * yMean;
                Matrix<double> invSqrtC = b * Matrix<double>.Build.DenseOfDiagonalArray([.. d.Select(static x => 1.0 / x)]) * b.Transpose();
                Vector<double> pathSigma = (1.0 - cSigma) * state.PathSigma + Math.Sqrt(cSigma * (2.0 - cSigma) * muEff) * (invSqrtC * yMean);
                double hsig = pathSigma.L2Norm() / Math.Sqrt(1.0 - Math.Pow(1.0 - cSigma, 2.0 * (gen + 1))) / chiN < 1.4 + 2.0 / (n + 1.0) ? 1.0 : 0.0;
                Vector<double> pathC = (1.0 - cc) * state.PathC + hsig * Math.Sqrt(cc * (2.0 - cc) * muEff) * yMean;
                Matrix<double> rankMu = Matrix<double>.Build.Dense(n, n);
                for (int i = 0; i < mu; i++) { rankMu += weights[i] * ordered[i].Y.OuterProduct(ordered[i].Y); }
                Matrix<double> covariance = (1.0 - c1 - cMu) * state.Covariance
                    + c1 * (pathC.OuterProduct(pathC) + (1.0 - hsig) * cc * (2.0 - cc) * state.Covariance)
                    + cMu * rankMu;
                double sigma = state.Sigma * Math.Exp(cSigma / dSigma * (pathSigma.L2Norm() / chiN - 1.0));
                double[] multipliers = problem.Handling.Advance(state.Multipliers, ordered[0].Constraints.AsSpan(), policy.PenaltyWeight);
                return new CmaState(meanNew, sigma, covariance, pathSigma, pathC, multipliers, carry.Front, state.Violation.Add(Worst(carry.Front)));
            });
    }

    static ImmutableArray<double> Embed(DesignProblem problem, Vector<double> mean, Vector<double> y, double sigma) {
        double[] full = new double[problem.Variables.Count];
        int g = 0;
        for (int axis = 0; axis < problem.Variables.Count; axis++) {
            full[axis] = problem.Variables[axis].Free ? problem.Variables[axis].Clamp(mean[g] + sigma * y[g]) : 0.0;
            if (problem.Variables[axis].Free) { g++; }
        }
        return [.. full];
    }

    // --- [PSO] ---------------------------------------------------------------------------------------------

    sealed record SwarmState(ImmutableArray<double>[] Position, double[][] Velocity, ImmutableArray<double>[] Best, double[] BestFitness, ImmutableArray<double> Global, double GlobalFitness, double[] Multipliers, ParetoFront Front, Seq<double> Violation);

    static Fin<KernelRun> EvolvePso(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        int particles = Math.Max(2, policy.Population);
        if (!problem.Variables.Exists(static v => v.Free)) { return Fin.Fail<KernelRun>(ComputeFault.Create("<pso-no-free-variable>")); }
        const double chi = 0.7298, phi = 2.05;
        Random rng = new(policy.Seed);
        return InitSwarm(problem, policy, oracle, particles, seed)
            .Bind(init => toSeq(Enumerable.Range(0, Math.Max(1, policy.Generations)))
                .Fold(Fin.Succ(init), (acc, _) => acc.Bind(state => PsoStep(problem, policy, oracle, chi, phi, rng, state)))
                .Map(state => new KernelRun(state.Front, policy.Generations, policy.TrustRadius, state.Violation)));
    }

    static Fin<SwarmState> InitSwarm(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, int particles, ParetoFront seed) =>
        LowDiscrepancy.Sobol(dimensions: problem.Variables.Count, seed: policy.Seed, Scramble.DigitalShift).Bind(generator =>
            toSeq(Enumerable.Range(0, particles))
                .Fold(Fin.Succ((Gen: generator, Items: Seq<(ImmutableArray<double> Pos, double Fit, DesignPoint Point)>())),
                    (acc, _) => acc.Bind(carry => {
                        (LowDiscrepancy next, double[] u) = carry.Gen.Draw();
                        ImmutableArray<double> pos = [.. problem.Variables.Map((v, axis) => v.Clamp(v.Lower + (axis < u.Length ? u[axis] : 0.0) * v.Width))];
                        return Probe(problem, oracle, pos).Map(point => (next, carry.Items.Add((pos, Fitness(problem, policy, point, new double[problem.Constraints]), point))));
                    }))
                .Map(carry => {
                    (ImmutableArray<double> Pos, double Fit, DesignPoint Point)[] items = carry.Items.ToArray();
                    int gbest = 0;
                    for (int i = 1; i < items.Length; i++) { if (items[i].Fit < items[gbest].Fit) { gbest = i; } }
                    return new SwarmState(
                        [.. items.Select(static it => it.Pos)],
                        [.. items.Select(_ => new double[problem.Variables.Count])],
                        [.. items.Select(static it => it.Pos)],
                        [.. items.Select(static it => it.Fit)],
                        items[gbest].Pos, items[gbest].Fit,
                        new double[problem.Constraints], items.Aggregate(seed, static (f, it) => f.Insert(it.Point)), Seq<double>());
                }));

    static Fin<SwarmState> PsoStep(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, double chi, double phi, Random rng, SwarmState state) =>
        toSeq(Enumerable.Range(0, state.Position.Length))
            .Fold(Fin.Succ(state), (acc, p) => acc.Bind(s => {
                double[] velocity = s.Velocity[p];
                double[] position = new double[problem.Variables.Count];
                for (int axis = 0; axis < problem.Variables.Count; axis++) {
                    velocity[axis] = chi * (velocity[axis] + phi * rng.NextDouble() * (s.Best[p][axis] - s.Position[p][axis]) + phi * rng.NextDouble() * (s.Global[axis] - s.Position[p][axis]));
                    position[axis] = problem.Variables[axis].Clamp(s.Position[p][axis] + velocity[axis]);
                }
                ImmutableArray<double> pos = [.. position];
                return Probe(problem, oracle, pos).Map(point => {
                    double fit = Fitness(problem, policy, point, s.Multipliers);
                    s.Position[p] = pos;
                    if (fit < s.BestFitness[p]) { s.Best[p] = pos; s.BestFitness[p] = fit; }
                    bool newGlobal = fit < s.GlobalFitness;
                    return s with {
                        Global = newGlobal ? pos : s.Global, GlobalFitness = newGlobal ? fit : s.GlobalFitness,
                        Multipliers = problem.Handling.Advance(s.Multipliers, point.Constraints.AsSpan(), policy.PenaltyWeight),
                        Front = s.Front.Insert(point),
                    };
                });
            }))
            .Map(s => s with { Violation = s.Violation.Add(Worst(s.Front)) });

    // --- [SIMULATED_ANNEALING] -----------------------------------------------------------------------------

    static Fin<KernelRun> Anneal(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        int chains = Math.Max(1, policy.Population), steps = Math.Max(1, policy.Generations);
        double cooling = Math.Pow(1e-3, 1.0 / steps);
        Random rng = new(policy.Seed);
        return LowDiscrepancy.Sobol(dimensions: problem.Variables.Count, seed: policy.Seed, Scramble.DigitalShift).Bind(generator =>
            toSeq(Enumerable.Range(0, chains))
                .Fold(Fin.Succ((Gen: generator, Front: seed, Multipliers: new double[problem.Constraints], Violation: Seq<double>())),
                    (acc, _) => acc.Bind(state => {
                        (LowDiscrepancy next, double[] u) = state.Gen.Draw();
                        ImmutableArray<double> start = [.. problem.Variables.Map((v, axis) => v.Clamp(v.Lower + (axis < u.Length ? u[axis] : 0.0) * v.Width))];
                        return Probe(problem, oracle, start).Bind(startPoint =>
                            AnnealChain(problem, policy, oracle, rng, cooling, steps, state.Front.Insert(startPoint), startPoint, Fitness(problem, policy, startPoint, state.Multipliers), state.Multipliers)
                                .Map(chain => (next, chain.Front, chain.Multipliers, state.Violation.Add(Worst(chain.Front)))));
                    }))
                .Map(state => new KernelRun(state.Front, chains * steps, policy.TrustRadius, state.Violation)));
    }

    static Fin<(ParetoFront Front, double[] Multipliers)> AnnealChain(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, Random rng, double cooling, int steps, ParetoFront front, DesignPoint current, double currentFit, double[] multipliers) =>
        toSeq(Enumerable.Range(0, steps))
            .Fold(Fin.Succ((Front: front, Current: current, Fit: currentFit, Temp: 1.0, Mult: multipliers)), (acc, _) => acc.Bind(s => {
                ImmutableArray<double> proposal = [.. problem.Variables.Map((v, axis) => v.Clamp(s.Current.Coordinates[axis] + policy.MutationRate * v.Width * (rng.NextDouble() * 2.0 - 1.0)))];
                return Probe(problem, oracle, proposal).Map(point => {
                    double fit = Fitness(problem, policy, point, s.Mult);
                    double delta = fit - s.Fit;
                    bool accept = delta <= 0.0 || rng.NextDouble() < Math.Exp(-delta / Math.Max(1e-12, s.Temp));
                    return (s.Front.Insert(point), accept ? point : s.Current, accept ? fit : s.Fit, s.Temp * cooling,
                        problem.Handling.Advance(s.Mult, point.Constraints.AsSpan(), policy.PenaltyWeight));
                });
            }))
            .Map(s => (s.Front, s.Mult));
}

public static class GeneticEngine {
    public static Fin<KernelRun> Evolve(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront seed) {
        RandomizationProvider.Current = new FastRandomRandomization();
        IChromosome adam = Genome(problem);
        Population population = new(Math.Max(2, policy.Population), Math.Max(2, policy.Population), adam) { GenerationStrategy = new PerformanceGenerationStrategy() };
        Atom<ParetoFront> harvest = Atom(seed);
        Atom<double[]> multipliers = Atom(new double[problem.Constraints]);
        Atom<Option<Error>> fault = Atom<Option<Error>>(None);
        FuncFitness fitness = new(chromosome =>
            Optimizer.Probe(problem, evaluate, DecodeRaw(problem, chromosome)).Match(
                Succ: point => {
                    harvest.Swap(f => f.Insert(point));
                    multipliers.Swap(values => problem.Handling.Advance(values, point.Constraints.AsSpan(), policy.PenaltyWeight));
                    ParetoFront front = harvest.Value;
                    int rank = front.Points.Count(p => problem.Handling.Dominates(p, point, problem.Senses.AsSpan()));
                    int slot = front.Points.Count - 1;
                    ImmutableArray<double> crowding = front.Crowding();
                    double diversity = slot >= 0 && slot < crowding.Length && front.Points[slot].Equals(point) ? crowding[slot] : 0.0;
                    double bonus = double.IsFinite(diversity) ? diversity / (1.0 + diversity) : 1.0;
                    double penalty = problem.Handling.Penalize(point.Objectives.IsDefaultOrEmpty ? 0.0 : point.Objectives[0] * problem.Senses[0], point.Constraints.AsSpan(), policy.PenaltyWeight, multipliers.Value);
                    return -rank + 0.5 * bonus - 1e-9 * penalty;
                },
                Fail: error => { fault.Swap(_ => Some(error)); return double.MinValue; }));
        GeneticAlgorithm algorithm = new(population, fitness, Selection(policy), new TwoPointCrossover(), new UniformMutation(true)) {
            CrossoverProbability = (float)policy.CrossoverRate,
            MutationProbability = (float)policy.MutationRate,
            Reinsertion = new ElitistReinsertion(),
            Termination = new OrTermination(new GenerationNumberTermination(Math.Max(1, policy.Generations)), new FitnessStagnationTermination(Math.Max(2, policy.Generations / 4))),
            TaskExecutor = new ParallelTaskExecutor { MaxThreads = Environment.ProcessorCount, Timeout = TimeSpan.FromSeconds(policy.SolveSeconds) },
            OperatorsStrategy = new TplOperatorsStrategy(),
        };
        algorithm.Start();
        return fault.Value.Match(
            Some: error => Fin.Fail<KernelRun>(error),
            None: () => Fin.Succ(new KernelRun(harvest.Value, algorithm.GenerationsNumber, policy.TrustRadius, Seq(Optimizer.Worst(harvest.Value)))));
    }

    public static Fin<Seq<ImmutableArray<double>>> Candidates(DesignProblem problem, int count, int seed) =>
        LowDiscrepancy.Sobol(dimensions: problem.Variables.Count, seed: seed, Scramble.DigitalShift)
            .Map(generator => toSeq(Enumerable.Range(0, Math.Max(1, count)))
                .Fold((Gen: generator, Pool: Seq<ImmutableArray<double>>()), (acc, _) => {
                    (LowDiscrepancy next, double[] u) = acc.Gen.Draw();
                    ImmutableArray<double> raw = [.. problem.Variables.Map((v, axis) => v.Clamp(v.Lower + (axis < u.Length ? u[axis] : 0.0) * v.Width))];
                    return (next, acc.Pool.Add(raw));
                }).Pool);

    static IChromosome Genome(DesignProblem problem) {
        DesignVariable[] free = [.. problem.Variables.Filter(static v => v.Free)];
        return new FloatingPointChromosome(
            [.. free.Map(static v => v.Lower)],
            [.. free.Map(static v => v.Lower + v.Width)],
            [.. free.Map(static _ => 32)],
            [.. free.Map(static v => v is DesignVariable.Continuous or DesignVariable.Density ? 6 : 0)]);
    }

    static ImmutableArray<double> DecodeRaw(DesignProblem problem, IChromosome chromosome) {
        double[] genes = ((FloatingPointChromosome)chromosome).ToFloatingPoints();
        double[] full = new double[problem.Variables.Count];
        int g = 0;
        for (int axis = 0; axis < problem.Variables.Count; axis++) {
            full[axis] = problem.Variables[axis].Free && g < genes.Length ? problem.Variables[axis].Clamp(genes[g++]) : 0.0;
        }
        return [.. full];
    }

    static ISelection Selection(OptimizerPolicy policy) =>
        policy.Kind == OptimizerKind.Nsga2 ? new TournamentSelection(2, allowWinnerCompeteNextTournament: false) : new EliteSelection();
}
```
