# [COMPUTE_OPTIMIZER]

Rasm.Compute solver optimizer: one `Optimizer` design-space-search axis over a typed `DesignVariable`/`ActivationRule`/`ConstraintHandling`/`ObjectiveSense` problem, dispatching one polymorphic `Optimize` entry by `OptimizerKind` row to a GENUINE per-family search kernel that owns its full iteration budget and adaptation state — NSGA-style multi-objective evolution over the admitted `GeneticSharp.GeneticAlgorithm` engine (the non-dominated rank + crowding comparator and the `ParetoFront` stay the page's), a covariance-matrix-adaptation evolution strategy whose rank-`μ`/rank-one covariance update eigendecomposes through the `Tensor/blas#DENSE_ALGEBRA` `Matrix<double>.Evd(Symmetricity.Symmetric)`, a Clerc-constriction particle swarm carrying real velocity/personal-best/global-best state, a Metropolis simulated-annealing ensemble with geometric cooling, Bayesian optimization that FITS a `GpModel` Gaussian-process surrogate over the running evaluation history every iteration and proposes by the `AcquisitionFunction` row over the GP posterior, gradient-adjoint trust-region/Armijo descent reading the reverse-mode `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Chain(Seq<GeometryTape>, seed)` tape, topology-SIMP optimality-criteria driven by that same adjoint compliance sensitivity (never a constant base), OR-Tools CP-SAT/MILP exact integer/MILP solving over a typed `LinearModel` objective+constraint set, low-discrepancy multi-start basin restart over the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy.Sobol` generator, and a robust min-max/RBDO row sharing the `Solver/uncertainty#UNCERTAINTY_LANE` `RandomVariable` inputs and feeding a reliability chance constraint `β_target − β ≤ 0` onto the `ConstraintHandling` axis — so the five population/metaheuristic kinds are FIVE distinct algorithmic kernels, never one `GeneticEngine` masquerade with swapped operators. The page owns the `OptimizerKind`/`DesignVariable`/`ActivationRule`/`ObjectiveSense`/`ConstraintHandling`/`StepControl`/`AcquisitionFunction`/`SurrogateKind`/`Orthogonalization` vocabulary, the `LinearModel`/`DesignProblem`/`DesignPoint`/`ParetoFront`/`OptimizerPolicy`/`KernelRun`/`OptimizationResult` carriers, the `Surrogate`/`RomBasis`/`GpModel`/`RbfModel`/`NeuralFieldModel` reduced-order models, and the `Optimizer` search fold plus the `GeneticEngine` GeneticSharp capsule. The `evaluate` oracle is one `Func<DesignPoint, Fin<Seq<double>>>` returning the objective vector FOLLOWED BY the constraint vector (the optimizer splits it by `problem.Objectives.Count`, so the `ConstraintHandling` axis — death/static penalty, the Deb feasibility-rules dominance, and the augmented-Lagrangian with a LIVE `λ ← max(0, λ + ρ·g)` multiplier update — is reachable, never a permanently-empty constraint set), the oracle being either the full `Solver/contract#SOLVE_CONTRACT` `SolveLane.Solve` or a `Surrogate.Predict` evaluation behind the identical signature so the surrogate/reduced-order duality is the same contract with a cheaper evaluator gated by the error bound and audited by an HONEST surrogate-hit count on the receipt. The gradient-adjoint row lowers each free design field to its DDG `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryTape` operator over the `DesignProblem.DesignMesh` and reads the reverse-mode tape through `SensitivityLaw.Chain`; the GP-covariance Cholesky and marginal-likelihood ride `Tensor/blas#DENSE_ALGEBRA` `Cholesky<double>`, the reduced basis the `Orthogonalization` SVD/QR rows, the implicit neural field the `Model/inference#INFERENCE_MODES` `RunOps.Infer` OrtValue run keyed by the parametric-family `XxHash128` digest; the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, `ClockPolicy`, the `ComparerAccessors.StringOrdinal` accessor from `Solver/discretization#DISCRETIZATION_MESH`, the `Rasm`/Vectors `MeshAdjointSnapshot`/`OperatorRow` public DDG-adjoint surface, and the `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryTape` shape arrive settled. The `ParetoFront` crosses to Persistence as a content-keyed vector-index artifact and the `Surrogate` crosses to `Solver/clash#CLASH_AND_TWIN` as the digital-twin baseline.

## [01]-[INDEX]

- [01]-[OPTIMIZER_LANE]: design-var/link/conditional search; per-family kernels; constraint axis; ROM/GP/field surrogate duality.

## [02]-[OPTIMIZER_LANE]

- Owner: `OptimizerKind` `[SmartEnum<string>]` search-algorithm rows; `DesignVariable` `[Union]` typed variable cases (free + linked/derived); `ActivationRule` `[Union]` conditional active-set cases; `ObjectiveSense` `[SmartEnum<string>]` minimize/maximize rows; `ConstraintHandling` `[SmartEnum<string>]` feasibility-policy rows (death-penalty/static-penalty/feasibility-rules/augmented-lagrangian) with a live multiplier-advance; `StepControl` `[SmartEnum<string>]` gradient line-search/trust-region rows; `AcquisitionFunction` `[SmartEnum<string>]` Bayesian-acquisition rows (expected-improvement/upper-confidence-bound/probability-of-improvement); `SurrogateKind` `[SmartEnum<string>]` surrogate-model rows (linear-trend/gaussian-process/radial-basis/neural-field); `Orthogonalization` `[SmartEnum<string>]` ROM reduced-basis rows; `LinearModel`/`LinearRow` the typed objective+constraint model the exact `cp-sat`/`milp` rows lower to OR-Tools; `DesignProblem` the variable/activation/constraint/objective record with the link+active-set `Resolve` fold and the optional `LinearModel` for the exact rows; `DesignPoint` the coordinate/objective/constraint sample; `OptimizerPolicy` the per-kind tuning record; `ParetoFront` the queryable non-dominated-set artifact with crowding-distance ranking and exact bi-objective hypervolume; `KernelRun` the per-kernel run result the `Optimize` fold projects onto `OptimizationResult`; `Optimizer` the static search fold dispatching one `Optimize` entry by `OptimizerKind` to its genuine per-family kernel; `GeneticEngine` the `GeneticSharp.GeneticAlgorithm` NSGA-style proposal capsule (genome from the `DesignVariable` cases, the dominance-rank+crowding `FuncFitness` over the `evaluate` oracle, `ParallelTaskExecutor` on the bounded lanes); `Surrogate` the reduced-order/learned model carrying an optional `RomBasis`, a `GpModel` covariance Cholesky, an `RbfModel`, and a content-keyed `NeuralFieldModel`; `RomBasis` the orthonormal reduced-basis projector; `GpModel`/`RbfModel` the scattered-data posteriors; `NeuralFieldModel` the parametric-family-digest-keyed coordinate-MLP/Fourier-feature field evaluated through the model-lane OrtValue run.
- Cases: `OptimizerKind` rows nsga2 · bayesian-gp · gradient-adjoint · topology-simp · simulated-annealing · cma-es · pso · cp-sat · milp · multi-start-global · robust-minimax (`exact=true` for cp-sat/milp; `populationBased=true` for nsga2/cma-es/pso/robust-minimax; `gradientBased=true` for gradient-adjoint/topology-simp); `DesignVariable` cases `Continuous` · `Integer` · `Categorical` · `Density` (topology field) · `Linked` (shared/derived — `Scale·source + Offset`, `Free=false`); `ActivationRule` cases `Always` · `WhenAbove` · `WhenBelow` · `WhenChoice`; `ConstraintHandling` rows death-penalty · static-penalty · feasibility-rules · augmented-lagrangian (`multiplierUpdate=true` only for augmented-lagrangian); `StepControl` rows fixed · armijo-backtrack · trust-region; `AcquisitionFunction` rows expected-improvement · upper-confidence-bound · probability-of-improvement; `SurrogateKind` rows linear-trend · gaussian-process · radial-basis · neural-field; `Orthogonalization` rows qr · modified-gram-schmidt · deim · pod-svd (`Interpolatory=true` for deim); `ObjectiveSense` rows minimize · maximize.
- Entry: `public static Fin<OptimizationResult> Optimize(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, CorrelationId correlation, ClockPolicy clocks)` — `Fin<T>` aborts on an empty design space, an `OptimizerKind` miss, or a kernel-internal infeasibility; `evaluate` returns the objective vector concatenated with the constraint vector (length `Objectives.Count + Constraints`), supplied as the full `SolveLane.Solve` or a `Surrogate.Predict` evaluation behind the identical signature. `Optimize` validates, builds one shared `Atom<(int Evals, int Hits)>` meter and wraps `evaluate` in a counting+gating oracle, calls the ONE kernel for `policy.Kind` exactly once (the kernel owns its full generation/iteration loop), and projects the `KernelRun` onto `OptimizationResult` with the real evaluation/surrogate-hit counts.
- Auto: the `Optimize` entry dispatches each `OptimizerKind` row through one generated total `Switch` (`Invoke`) to its genuine kernel — `nsga2` drives `GeneticEngine.Evolve` (a `DesignVariable`-mapped `FloatingPointChromosome` genome, a `FuncFitness` whose score is the candidate's non-dominated rank against the running front plus a crowding bonus under the `ConstraintHandling.Dominates` feasibility-aware comparator, a row-selected operator set, a `GenerationNumberTermination`+`FitnessStagnationTermination` `OrTermination`, and a deadline-bounded `ParallelTaskExecutor`), `cma-es` runs a faithful CMA-ES (sample `λ` offspring `x = m + σ·B·(D ⊙ z)` from the `Matrix<double>.Evd` factor of the symmetrized covariance, select the `μ` best by penalized fitness, update the mean by weighted recombination, the evolution paths `p_σ`/`p_c`, the rank-one+rank-`μ` covariance, and the step size by CSA), `pso` runs a Clerc-constriction swarm (`v ← χ(v + φ₁r₁(p−x) + φ₂r₂(g−x))`, `x ← clamp(x+v)`), `simulated-annealing` runs a Metropolis ensemble of `Population` chains under geometric cooling, `bayesian-gp` FITS a `GpModel` over the accumulated history each iteration and ranks `GeneticEngine.Candidates` by the `AcquisitionFunction` row over the GP posterior, `gradient-adjoint` descends the reverse-mode adjoint gradient under the `StepControl` line-search/trust-region, `topology-simp` runs the optimality-criteria bisection over the `DesignVariable.Density` field driven by the adjoint compliance sensitivity, `cp-sat`/`milp` lower the typed `LinearModel` to a genuine `CpModel`/`Solver` objective+constraint set and harvest the optimum, `multi-start-global` restarts any inner row from `LowDiscrepancy.Sobol` space-filling seeds, and `robust-minimax` scores the worst-case over `RandomVariable`-sampled scenarios and appends the reliability chance constraint. The constraint handling is the `ConstraintHandling` row (the feasibility-rules dominance, the static/death penalty, or the augmented-Lagrangian multiplier advance), and the surrogate duality is a policy column gating cheap-vs-full evaluation with the surrogate-hit count metered honestly.
- Receipt: the `Optimization` `ComputeReceipt` case carries the optimizer key, the kernel-reported generation count, the metered evaluated-point count, the metered surrogate-hit count, the front size, and the hypervolume indicator (the receipt's six audit slots); the constraint-violation history and the trust-region radius ride the `OptimizationResult` carrier, and the per-evaluation surrogate error bound and GP marginal-likelihood ride the `Surrogate`/`GpModel` so a ROM/GP acceptance is auditable without a receipt slot the `Runtime/receipts#RECEIPT_UNION` owner does not declare.
- Packages: MathNet.Numerics (the dense `Matrix<double>.Evd`/`Cholesky`/`Svd`/`QR` algebra + `Distributions.Normal` reliability/sampling), GeneticSharp (the NSGA-style `GeneticAlgorithm` engine + chromosome/operator/executor catalog behind the `nsga2` row), Google.OrTools (CP-SAT `CpModel`/`CpSolver` + LinearSolver `Solver` behind the `cp-sat`/`milp` rows), System.Numerics.Tensors, Microsoft.ML.OnnxRuntime (the neural-field OrtValue run), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, Vectors `MeshAdjointSnapshot`/`OperatorRow` public surface for the DDG gradient-adjoint tape), Rasm.Persistence (project), BCL inbox
- Growth: a new search algorithm is one `OptimizerKind` row plus one arm on the `Optimize` total `Switch` (`Invoke`) — a population-based row binds its own update rule (CMA-ES covariance, PSO velocity, SA Metropolis) or `GeneticEngine` for a genuine GA; an exact OR-Tools row lowers the `LinearModel` to a `CpModel`/`Solver`; a wrapping row composes the inner kernel through the same dispatch — and the generated `Switch` breaks at COMPILE time until the arm is added (never a runtime kind-miss); a new genetic operator is one `GeneticEngine.Crossover`/`Mutation` arm binding the `GeneticSharp` operator; a new variable kind is one `DesignVariable` case carrying its `AdjointOperator`; a new constraint discipline is one `ConstraintHandling` row; a new line-search/trust-region is one `StepControl` row; a new acquisition is one `AcquisitionFunction` row; a new surrogate model is one `SurrogateKind` row plus a `Fit` arm; a new ROM orthogonalization is one `Orthogonalization` row; a new objective is one row on the `DesignProblem` objective set; zero new surface — an `Nsga2Engine`/`BayesianOptimizer`/`CmaEsSolver`/`ParticleSwarm`/`Annealer`/`TopologyOptimizer`/`CpSatSolver`/`MilpSolver`/`MultiStartRunner` sibling family is collapsed onto the one `Optimize` total `Switch`, a `LinkedVariable`/`DerivedVariable` family onto `DesignVariable.Linked`, a `PenaltyHandler`/`FeasibilityHandler` family onto `ConstraintHandling`, a `QrReducer`/`GramSchmidtReducer`/`DeimReducer` family onto `Orthogonalization`, and a `SurrogateNet`/`FieldPredictor` sibling onto the `Surrogate.NeuralField` row.
- Boundary: the optimizer is contract-uniform — the `evaluate` oracle is the single coupling point and the search never knows whether it ran a full FEA solve or a surrogate prediction, so the surrogate/reduced-order duality is the same contract with a cheaper evaluator, never a parallel surrogate-search path; the oracle returns the objective vector followed by the constraint vector so the `ConstraintHandling` axis is genuinely reachable — a permanently-empty constraint set that silently disables death/penalty/feasibility/augmented-Lagrangian handling is the deleted form; the design variables are typed so a bound violation is a fault at the boundary, never a clamped silent repair, and variable-linking plus conditional design spaces are rows on the same axis through `DesignProblem.Resolve`; the FIVE population/metaheuristic kinds are FIVE genuine kernels — `nsga2` routes the `GeneticEngine` over the admitted `GeneticSharp.GeneticAlgorithm` (the package owns the GA machinery; the dominance-rank+crowding comparator and the `ParetoFront` stay the page's), while `cma-es` (covariance-adaptation evolution strategy), `pso` (constriction velocity swarm), and `simulated-annealing` (Metropolis ensemble with cooling) are distinct algorithms no admitted package owns, authored as genuine in-package folds over the `Matrix<double>.Evd`/`Distributions.Normal` numeric substrate — an operator-swap masquerade that routes cma-es/pso/sa through one `GeneticAlgorithm` with a swapped `Crossover`/`Mutation` and zero covariance/velocity/temperature state is the deleted form; the `bayesian-gp` row FITS a `GpModel` from the running history each iteration and ranks candidates by the acquisition over the GP posterior mean/variance — a Bayesian loop that never fits the GP (relying on a pre-supplied surrogate) and ranks by a constant is the deleted form; the gradient-adjoint row reads the reverse-mode adjoint through `SensitivityLaw.Chain(problem.AdjointTape, seed)` (the verified two-argument geometry-tape overload, the cotangent the only seed — a phantom three-argument `Chain(tape, inputs, seed)` call is the deleted form), the registry lowering each free design field to its DDG operator (`DesignVariable.Continuous` → `Gradient`, `DesignVariable.Density` → `CotangentLaplacian`) producing a snapshot-bearing `Seq<GeometryTape>` over the `DesignProblem.DesignMesh` `MeshAdjointSnapshot`, so a `DesignProblem` carrying no design mesh lowers an empty tape and descends degenerate by construction (the absent-mesh case, never an absent operator) and the `StepControl` row owns the line-search/trust-region (the trust-region row grows/shrinks the radius on the actual-versus-predicted reduction ratio, `ρ < 0.25` shrink / `ρ > 0.75` grow; the Armijo row backtracks on the sufficient-decrease condition) so a fixed-step descent without step control is the deleted form; the topology-SIMP optimality-criteria update reads the genuine compliance sensitivity from the SAME adjoint route and bisects the Lagrange multiplier to the volume constraint — a density update whose base ignores the structural sensitivity (a constant `−1/λ` power) is the deleted fake; the augmented-Lagrangian row carries a LIVE multiplier vector advanced `λ ← max(0, λ + ρ·g)` each generation and read by `Penalize` so the `MultiplierUpdate` flag drives a real update, never a flag that degenerates to static penalty; the ROM reduction is one `Orthogonalization` SmartEnum over the snapshot matrix (QR from `Matrix<double>.QR().Q`, modified-Gram-Schmidt column-by-column, DEIM greedy interpolation indices off the `Matrix<double>.Svd` left-singular basis, POD-SVD energy-fraction truncation); the surrogate is the `SurrogateKind` axis — the linear-trend row fits the leverage-scaled predictive-variance bound, the Gaussian-process row fits the covariance Cholesky over the squared-exponential kernel and predicts the posterior mean/variance with the marginal-likelihood as evidence (riding the `Tensor/blas#DENSE_ALGEBRA` `Cholesky<double>` surface), the radial-basis row reconstructs through the rank-revealing regression route, and the neural-field row threads the leased `Model/inference#INFERENCE_MODES` `(InferenceSession, RunOptions, CancelScope)` so `Predict` evaluates the coordinate-MLP/Fourier-feature implicit field through the `RunOps.Infer` OrtValue run behind the identical `(Seq<double> Values, double Bound)` signature the GP/POD rows answer (the field weights one content-addressed ONNX artifact the Python offline-science companion fits and returns over the `Runtime/channels#PROTO_VOCABULARY` artifact transport — C# owns only inference; an in-proc ORT-Training fit is the rejected form); a surrogate that drifts past its bound forces a full re-evaluation and the surrogate-hit count is metered honestly through the shared `Atom` meter, never a receipt slot that stays zero; the Pareto front is content-addressed onto the Persistence vector index and the exact bi-objective hypervolume is the staircase sweep (a Lebesgue-box overcount that double-counts dominated overlaps is the deleted form, the ≥3-objective case a documented Monte-Carlo estimate over the reference box); the exact `cp-sat`/`milp` rows lower the typed `DesignProblem.Exact` `LinearModel` to a genuine OR-Tools `CpModel`/`Solver` objective+constraint set through the typed model-builder API (`CpModel.NewIntVar`/`LinearExpr.WeightedSum`/`AddLinearConstraint`/`Minimize` and `Solver.MakeIntVar`/`MakeNumVar`/`MakeConstraint`/`Objective().SetCoefficient`, never a string-parsed model and never an empty model whose objective the solver never sees) — CP-SAT solves integer/boolean natively and discretizes continuous through the `IntegerStep`, MILP routes the integer part to SCIP exactly and the continuous part through the linear backend, the row faulting `<exact-needs-linear-model>` when no `LinearModel` is supplied (an exact solver cannot optimize a black-box FEA objective, the named impedance) and the harvested assignment evaluating through the `evaluate` oracle into the one `ParetoFront`; the `multi-start-global` row wraps any inner local `OptimizerKind` (`MultiStartInner`, guarded against self-recursion) with a `LowDiscrepancy.Sobol`-seeded basin restart so global escape is a row reusing the genuine low-discrepancy generator rather than a plain `System.Random` fill; the `robust-minimax` row reads the `Solver/uncertainty#UNCERTAINTY_LANE` `RandomVariable` scenario set, samples scenarios through the SAME `LowDiscrepancy.Sobol`+`RandomVariable.Quantile` inverse-transform the UQ lane uses, scores each candidate as the worst-case over the scenarios, and appends a reliability chance constraint `β_target − β ≤ 0` (the failure probability the sampled limit-state exceedance fraction, `β = Normal.InvCDF(1 − pf)`) onto the `ConstraintHandling` axis so robust optimization and forward UQ genuinely share inputs and RBDO is a constraint row, the inner scenario evaluation riding the gated `Surrogate.Predict` where the error bound holds rather than a full inner solve per outer candidate; OR-Tools native solver handles enter only through declared `IDisposable` roots (`CpSolver`/`Solver`) released by `Dispose`, and a hand-rolled branch-and-bound, simplex, or float-equality feasibility check beside the solver is the rejected form.

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

    // The augmented-Lagrangian estimate advances each generation only on the multiplier-update row; the other
    // rows return the vector unchanged so a static/death/feasibility run never accumulates a phantom multiplier.
    public double[] Advance(double[] multipliers, ReadOnlySpan<double> constraints, double rho) {
        if (!MultiplierUpdate) { return multipliers; }
        double[] next = new double[multipliers.Length];
        for (int i = 0; i < next.Length; i++) { next[i] = Math.Max(0.0, multipliers[i] + rho * (i < constraints.Length ? constraints[i] : 0.0)); }
        return next;
    }

    static double Violation(ReadOnlySpan<double> constraints) {
        double sum = 0.0;
        foreach (double c in constraints) { sum += Math.Max(0.0, c); }
        return sum;
    }

    static double Lagrange(ReadOnlySpan<double> constraints, ReadOnlySpan<double> multipliers) {
        double sum = 0.0;
        for (int i = 0; i < constraints.Length && i < multipliers.Length; i++) { sum += multipliers[i] * Math.Max(0.0, constraints[i]); }
        return sum;
    }
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

    // mean/sigma are the GP posterior the bayesian-gp kernel fits each iteration; best is the incumbent
    // minimization objective. Higher Score = more promising; the kernel ranks candidates by descending Score.
    // Φ/φ are the admitted MathNet.Numerics.Distributions.Normal statics (the same surface the reliability and
    // sampling rows drive), never a hand-rolled Abramowitz-Stegun erf polynomial. EI = (best−mean)·Φ(z) + σ·φ(z).
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

    // The DDG adjoint operator each free design field lowers to over the design mesh — Continuous descends the
    // Gradient, Density the CotangentLaplacian; Integer/Categorical/Linked carry None (a discrete or derived
    // field is not a differentiable design field), so a NEW DesignVariable case must DECIDE its operator at
    // COMPILE time rather than silently lowering no tape through a Type-keyed dictionary miss.
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

// The typed linear objective+constraint model the exact cp-sat/milp rows lower to an OR-Tools CpModel/Solver.
// A row coefficient set indexes the DesignProblem.Variables positionally; the exact rows REQUIRE this model
// because an exact solver cannot optimize the black-box `evaluate` FEA objective the metaheuristic rows drive.
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
                .Bind(v => v.AdjointOperator.Match(Some: op => Seq1(new GeometryTape(op, mesh)), None: () => Seq<GeometryTape>()))
            : Seq<GeometryTape>();

    public ImmutableArray<double> Senses => [.. Objectives.Map(static o => o.Sign)];

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
}

public sealed record ParetoFront(Seq<DesignPoint> Points, ImmutableArray<double> Senses) {
    public ParetoFront Insert(DesignPoint candidate) =>
        Points.Exists(p => p.Dominates(candidate, Senses.AsSpan()))
            ? this
            : this with { Points = Points.Filter(p => !candidate.Dominates(p, Senses.AsSpan())).Add(candidate) };

    // Exact bi-objective hypervolume (the staircase sweep over the sense-applied non-dominated set); the
    // >=3-objective case is a Monte-Carlo estimate over the reference box. A per-point box sum that double-
    // counts dominated overlaps is the deleted overcount form.
    public double Hypervolume(ReadOnlySpan<double> reference) {
        if (Points.IsEmpty) { return 0.0; }
        int objectives = Points.Head.Objectives.Length;
        return objectives == 2 ? Hypervolume2D(reference) : HypervolumeEstimate(reference, objectives);
    }

    double Hypervolume2D(ReadOnlySpan<double> reference) {
        double r0 = reference[0], r1 = reference[1];
        var staircase = Points
            .Map(p => (X: p.Objectives[0] * Senses[0], Y: p.Objectives[1] * Senses[1]))
            .OrderByDescending(static p => p.X)
            .ToArray();
        double area = 0.0, prevX = r0;
        foreach (var p in staircase) {
            if (p.X >= prevX) { continue; }
            area += (prevX - p.X) * Math.Max(0.0, r1 - p.Y);
            prevX = p.X;
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
        var rng = new Random(0x4D5);
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
        int n = Points.Count, objectives = Points.IsEmpty ? 0 : Points.Head.Objectives.Length;
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

// The per-kernel run result the Optimize fold projects onto OptimizationResult; the evaluation/surrogate-hit
// counts ride the shared Atom meter, never re-derived here.
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
    // The fitted field is the canonical Tensor/sampling#SCATTER_RECONSTRUCTION RbfFit (RbfKernel.Gaussian over the
    // held-SVD reconstruction); the surrogate adds only the acquisition bound — the residual RMS inflated by the
    // query's distance to the nearest centre so an extrapolating step reads a wider uncertainty.
    public (double Mean, double Bound) Posterior(ReadOnlySpan<double> query) {
        var q = Vector<double>.Build.Dense(Fit.Centres.ColumnCount, i => i < query.Length ? query[i] : 0.0);
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
    public (Seq<double> Values, double Bound) Predict(DesignPoint point) =>
        (Field, Lane, Gp, Rbf) switch {
            ({ IsSome: true, Case: NeuralFieldModel field }, { IsSome: true, Case: var (session, options, scope) }, _, _) =>
                field.Predict(session, options, scope, point.Coordinates.AsSpan()),
            (_, _, { IsSome: true, Case: GpModel gp }, _) =>
                gp.Posterior(point.Coordinates.AsSpan()) is var (mean, variance) ? (Seq1(mean), Math.Sqrt(variance)) : (Seq1(0.0), double.MaxValue),
            (_, _, _, { IsSome: true, Case: RbfModel rbf }) =>
                rbf.Posterior(point.Coordinates.AsSpan()) is var (mean, bound) ? (Seq1(mean), bound) : (Seq1(0.0), double.MaxValue),
            _ => LinearPredict(point),
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
        return (Seq1(mean), bound);
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

    // Total dispatch over the closed SurrogateKind family — a NEW surrogate row must DECIDE its fit at COMPILE
    // time rather than slipping through a ternary catch-all. GaussianProcess fits the covariance Cholesky,
    // RadialBasis the held-SVD RBF, LinearTrend the leverage-scaled trend; NeuralField carries no in-proc fit
    // (its ONNX weights arrive offline through OfField), so a Fit() call degenerates to the linear-trend
    // exploration bound rather than an in-proc train.
    public static Surrogate Fit(SurrogateKind kind, Seq<DesignPoint> history, int objective) =>
        kind.Switch(
            state: (History: history, Objective: objective),
            linearTrend: static s => FitLinear(SurrogateKind.LinearTrend, s.History, s.Objective),
            gaussianProcess: static s => FitGaussianProcess(s.History, s.Objective),
            radialBasis: static s => FitRadialBasis(s.History, s.Objective),
            neuralField: static s => FitLinear(SurrogateKind.NeuralField, s.History, s.Objective));

    public static Surrogate OfField(NeuralFieldModel field, InferenceSession session, RunOptions options, CancelScope scope) =>
        new(SurrogateKind.NeuralField, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, 1.0, field.TrainedResidualRms, None, None, None,
            Some(field), Some((session, options, scope)));

    static Surrogate FitRadialBasis(Seq<DesignPoint> history, int objective) {
        if (history.Count < 2) { return FitLinear(SurrogateKind.RadialBasis, history, objective); }
        int n = history.Count, dim = history.Head.Coordinates.Length;
        double lengthScale = MedianPairwise(history);
        double shape = 1.0 / (Math.Sqrt(2.0) * Math.Max(1e-9, lengthScale));
        var centres = Matrix<double>.Build.Dense(n, dim, (r, c) => history[r].Coordinates[c]);
        Matrix<double> response = Matrix<double>.Build.Dense(n, 1, (r, _) => history[r].Objectives[objective]);
        // Compose the canonical Tensor/sampling#SCATTER_RECONSTRUCTION Scatter.Fit (centres == samples, the square
        // interpolation case) over the RbfKernel.Gaussian vocabulary row — the held-SVD RbfFit owns the design and
        // solve, so the surrogate keeps only the exploration bound and a local design matrix is the rejected form.
        return Scatter.Fit(centres, centres, response, RbfKernel.Gaussian, shape, TolerancePolicy.Derive(centres, response.Column(0)))
            .Map(fit => {
                Matrix<double> fitted = fit.Evaluate(centres);
                double rms = Math.Sqrt(toSeq(Enumerable.Range(0, n)).Sum(i => {
                    double e = history[i].Objectives[objective] - fitted[i, 0];
                    return e * e;
                }) / n);
                return new Surrogate(SurrogateKind.RadialBasis, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, lengthScale, rms, None, None,
                    Some(new RbfModel(fit, lengthScale, rms)), None, None);
            })
            .IfFail(FitLinear(SurrogateKind.RadialBasis, history, objective));
    }

    static Surrogate FitGaussianProcess(Seq<DesignPoint> history, int objective) {
        if (history.Count < 2) { return FitLinear(SurrogateKind.LinearTrend, history, objective); }
        int n = history.Count, dim = history.Head.Coordinates.Length;
        double lengthScale = MedianPairwise(history), signalVar = ObjectiveVariance(history, objective), noiseVar = 1e-6 * Math.Max(1e-9, signalVar);
        var x = Matrix<double>.Build.Dense(n, dim, (r, c) => history[r].Coordinates[c]);
        var y = Vector<double>.Build.Dense(n, r => history[r].Objectives[objective]);
        var kMatrix = Matrix<double>.Build.Dense(n, n, (r, c) =>
            GpModel.Kernel(x.Row(r), x.Row(c).AsArray(), lengthScale, signalVar) + (r == c ? noiseVar : 0.0));
        Cholesky<double> chol = kMatrix.Cholesky();
        Vector<double> alpha = chol.Solve(y);
        double logMarginal = -0.5 * y.DotProduct(alpha) - 0.5 * chol.DeterminantLn - 0.5 * n * Math.Log(2.0 * Math.PI);
        return new Surrogate(SurrogateKind.GaussianProcess, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, lengthScale, Math.Sqrt(signalVar), None,
            Some(new GpModel(chol, alpha, x, lengthScale, signalVar, noiseVar, logMarginal)), None, None, None);
    }

    static Surrogate FitLinear(SurrogateKind kind, Seq<DesignPoint> history, int objective) {
        if (history.IsEmpty) { return new(kind, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, 1.0, double.MaxValue, None, None, None, None, None); }
        int dim = history.Head.Coordinates.Length;
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
        var distances = new List<double>();
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
    public (Seq<double> Values, double Bound) Predict(InferenceSession session, RunOptions options, CancelScope scope, ReadOnlySpan<double> coordinates) {
        double leverage = 0.0;
        for (int axis = 0; axis < coordinates.Length && axis < CoordinateRank; axis++) { leverage += coordinates[axis] * coordinates[axis]; }
        double bound = TrainedResidualRms * (1.0 + Math.Sqrt(leverage));
        float[] features = [.. coordinates[..Math.Min(coordinates.Length, CoordinateRank)].ToArray().Select(static x => (float)x)];
        return RunOps.Bind(new RunInput.Managed<float>(InputName, features, [1, CoordinateRank])).Bind(inputs =>
            session.Infer(options, scope, inputs, Seq(OutputName),
                results => {
                    ReadOnlySpan<float> field = results.First().GetTensorDataAsSpan<float>();
                    return Fin.Succ((toSeq(field[..Math.Min(field.Length, FieldComponents)].ToArray().Select(static v => (double)v)), bound));
                })).Match(Succ: static pair => pair, Fail: static _ => (Seq1(0.0), double.MaxValue));
    }
}
public static class Optimizer {
    public static Fin<OptimizationResult> Optimize(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, CorrelationId correlation, ClockPolicy clocks) =>
        problem.Variables.IsEmpty
            ? Fin.Fail<OptimizationResult>(ComputeFault.Create("<optimizer-empty-design-space>"))
            : Run(problem, policy, evaluate, clocks);

    // Total dispatch over the closed OptimizerKind family: every row routes to its genuine per-family kernel
    // through the generated Switch, so a NEW OptimizerKind row breaks this dispatch at COMPILE time rather than
    // silently faulting at runtime, and the kernel owns its full generation/iteration loop (invoked once).
    // MultiStart re-enters here for its inner row, so the global-restart wrap reuses the one total dispatch
    // (a string-keyed dispatch table with a runtime kind-miss fallback is the rejected form).
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

    // One kernel call (the kernel owns its full generation/iteration loop and adaptation state) projected onto
    // the result with the metered evaluation/surrogate-hit counts — never the per-generation re-run of a full
    // inner search the legacy fold-over-Generations shape forced.
    static Fin<OptimizationResult> Run(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ClockPolicy clocks) {
        var meter = Atom((Evals: 0, Hits: 0));
        Func<DesignPoint, Fin<Seq<double>>> oracle = Gated(policy, evaluate, meter);
        return Invoke(policy.Kind, problem, policy, oracle, new ParetoFront(Seq<DesignPoint>(), problem.Senses))
            .Map(run => new OptimizationResult(policy.Kind, run.Front, run.Generations, meter.Value.Evals, meter.Value.Hits,
                run.Front.Hypervolume(Reference(run.Front)), run.Violation, run.TrustRadius, clocks.Now));
    }

    public static ComputeReceipt.Optimization Receipt(OptimizationResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Kind.Key, result.Generations, result.Evaluations, result.SurrogateHits, result.Front.Points.Count, result.Hypervolume) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    // Every oracle call bumps the evaluation meter; a surrogate prediction within the error bound bumps the
    // surrogate-hit meter and short-circuits the full solve, so the receipt's hit count is the genuine number
    // of cheap evaluations rather than a slot that stays zero.
    static Func<DesignPoint, Fin<Seq<double>>> Gated(OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> full, Atom<(int Evals, int Hits)> meter) =>
        policy.Surrogate is { IsSome: true, Case: Surrogate surrogate }
            ? point => {
                var (values, bound) = surrogate.Predict(point);
                if (bound <= policy.SurrogateErrorBound) { meter.Swap(static m => (m.Evals + 1, m.Hits + 1)); return Fin.Succ(values); }
                meter.Swap(static m => (m.Evals + 1, m.Hits));
                return full(point);
            }
            : point => { meter.Swap(static m => (m.Evals + 1, m.Hits)); return full(point); };

    // Splits the oracle output into objectives (the first Objectives.Count entries) and constraints (the
    // remainder), so the ConstraintHandling axis is genuinely reachable instead of a permanently-empty set.
    internal static Fin<DesignPoint> Probe(DesignProblem problem, Func<DesignPoint, Fin<Seq<double>>> oracle, ImmutableArray<double> raw) {
        ImmutableArray<double> coords = problem.Resolve(raw);
        return oracle(new DesignPoint(coords, [], [])).Map(values => {
            int m = problem.Objectives.Count;
            return new DesignPoint(coords, [.. values.Take(m)], [.. values.Skip(m).Take(problem.Constraints)]);
        });
    }

    static double Fitness(DesignProblem problem, OptimizerPolicy policy, DesignPoint point, ReadOnlySpan<double> multipliers) =>
        problem.Handling.Penalize(
            point.Objectives.IsDefaultOrEmpty ? 0.0 : point.Objectives[0] * problem.Senses[0],
            point.Constraints.AsSpan(), policy.PenaltyWeight, multipliers);

    internal static double Worst(ParetoFront front) => front.Points.IsEmpty ? 0.0 : front.Points.Max(static p => p.Violation);

    // Nadir reference dominating every point in the minimize sense-space (always >= each point so the
    // hypervolume box is non-negative for positive- and negative-valued objectives alike).
    static double[] Reference(ParetoFront front) =>
        front.Points.IsEmpty
            ? [1.0]
            : [.. toSeq(Enumerable.Range(0, front.Points.Head.Objectives.Length))
                .Map(axis => front.Points.Max(point => point.Objectives[axis] * front.Senses[axis]) is var worst ? worst + 0.1 * Math.Abs(worst) + 0.1 : 0.1)];

    // Bayesian optimization that FITS a GpModel over the running history EVERY iteration and ranks the genome-
    // decoded candidate pool by the AcquisitionFunction over the GP posterior mean/variance — a loop that never
    // fits the GP (relying on a pre-supplied surrogate) and ranks by a constant is the deleted form.
    static Fin<KernelRun> AcquireBayesian(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        toSeq(Enumerable.Range(0, Math.Max(1, policy.Generations)))
            .Fold(Fin.Succ((History: seed.Points, Front: seed, Violation: Seq<double>())), (acc, gen) => acc.Bind(state => {
                Surrogate gp = Surrogate.Fit(SurrogateKind.GaussianProcess, state.History, 0);
                double best = state.History.IsEmpty ? 0.0 : state.History.Min(p => p.Objectives.IsDefaultOrEmpty ? double.MaxValue : p.Objectives[0] * problem.Senses[0]);
                Seq<ImmutableArray<double>> ranked = GeneticEngine.Candidates(problem, policy.Population, policy.Seed + gen)
                    .OrderByDescending(raw => {
                        var (values, bound) = gp.Predict(new DesignPoint(problem.Resolve(raw), [], []));
                        return policy.Acquisition.Score(values.Head.IfNone(best), bound, best);
                    })
                    .Take(Math.Max(1, policy.Population / 8))
                    .ToSeq();
                return ranked.Fold(Fin.Succ((state.History, state.Front)), (inner, raw) => inner.Bind(carry =>
                        Probe(problem, oracle, raw).Map(point => (carry.History.Add(point), carry.Front.Insert(point)))))
                    .Map(carry => (carry.History, carry.Front, state.Violation.Add(Worst(carry.Front))));
            }))
            .Map(state => new KernelRun(state.Front, policy.Generations, policy.TrustRadius, state.Violation));

    // First-order adjoint descent under the StepControl line-search/trust-region: each step reads the reverse-
    // mode gradient, probes a candidate step, and lets StepControl.Next size the next step on the actual-vs-
    // predicted reduction ratio. A fixed-step descent without step control is the deleted form.
    static Fin<KernelRun> DescendAdjoint(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        ImmutableArray<double> start = seed.Points.IsEmpty ? [.. problem.Variables.Map(static v => v.Lower)] : seed.Points.Head.Coordinates;
        return toSeq(Enumerable.Range(0, Math.Max(1, policy.Generations)))
            .Fold(Fin.Succ((Origin: start, Radius: policy.TrustRadius, Front: seed, Violation: Seq<double>())), (acc, _) => acc.Bind(state =>
                Adjoint(problem, state.Origin).Bind(gradient =>
                    Probe(problem, oracle, state.Origin).Bind(baseline => {
                        double objectiveAtOrigin = baseline.Objectives.IsDefaultOrEmpty ? 0.0 : baseline.Objectives[0];
                        ImmutableArray<double> stepped = Stepped(problem, state.Origin, gradient, state.Radius);
                        double predicted = TensorPrimitives.SumOfSquares<double>([.. gradient]) * state.Radius;
                        return Probe(problem, oracle, stepped).Map(probe => {
                            double actual = objectiveAtOrigin - (probe.Objectives.IsDefaultOrEmpty ? objectiveAtOrigin : probe.Objectives[0]);
                            var (step, radius) = policy.StepControl.Next(state.Radius, actual, predicted, Math.Max(1e-6, policy.MutationRate));
                            ImmutableArray<double> next = Stepped(problem, state.Origin, gradient, step);
                            ParetoFront front = state.Front.Insert(baseline).Insert(probe);
                            return (next, radius, front, state.Violation.Add(Worst(front)));
                        });
                    }))))
            .Map(state => new KernelRun(state.Front, policy.Generations, state.Radius, state.Violation));
    }

    static ImmutableArray<double> Stepped(DesignProblem problem, ImmutableArray<double> origin, ImmutableArray<double> gradient, double scale) =>
        [.. problem.Variables.Map((v, axis) => v.Clamp(origin.ElementAtOrDefault(axis) - scale * gradient.ElementAtOrDefault(axis)))];

    // Reads the reverse-mode adjoint through the VERIFIED two-argument SensitivityLaw.Chain(Seq<GeometryTape>,
    // seed) overload — the cotangent is the only seed, a phantom three-argument Chain(tape, inputs, seed) call
    // is the deleted form. The unit cotangent is the default the shape/topology call site overrides with a real
    // objective sensitivity; a problem carrying no DesignMesh lowers an empty tape and the gradient is
    // degenerate by construction (the absent-mesh case, never an absent operator).
    static Fin<ImmutableArray<double>> Adjoint(DesignProblem problem, ImmutableArray<double> origin) =>
        SensitivityLaw.Chain(problem.AdjointTape, [.. Enumerable.Repeat(1f, origin.Length)])
            .Map(static gradient => (ImmutableArray<double>)[.. gradient.Span.ToArray().Select(static g => (double)g)]);

    // Topology-SIMP optimality criteria driven by the GENUINE adjoint compliance sensitivity (dc/dx_e <= 0),
    // bisecting the Lagrange multiplier to the volume constraint each generation. A density update whose base
    // ignores the structural sensitivity (a constant -1/lambda power) is the deleted fake.
    static Fin<KernelRun> OptimalityCriteria(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        toSeq(Enumerable.Range(0, Math.Max(1, policy.Generations)))
            .Fold(Fin.Succ((Density: seed.Points.IsEmpty ? (ImmutableArray<double>)[.. problem.Variables.Map(_ => policy.VolumeFraction)] : seed.Points.Head.Coordinates, Front: seed, Violation: Seq<double>())),
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
                double dc = policy.SimpPenalty * Math.Pow(Math.Max(1e-9, x), policy.SimpPenalty - 1.0) * raw;   // SIMP material-penalized compliance sensitivity dc/dx_e
                double scaled = x * Math.Pow(Math.Max(1e-12, -dc / lagrange), eta);
                updated[e] = problem.Variables[e].Clamp(Math.Clamp(scaled, Math.Max(0.0, x - move), Math.Min(1.0, x + move)));
            }
            if (TensorPrimitives.Average<double>(updated) > policy.VolumeFraction) { lower = lagrange; } else { upper = lagrange; }
        }
        return [.. updated];
    }
    // CP-SAT lowers the typed LinearModel to a genuine integer CpModel objective+constraint set (never an empty
    // model whose objective the solver never sees), solves to a classified status, and harvests the assignment
    // into the ParetoFront; the row faults <exact-needs-linear-model> when no LinearModel is supplied because an
    // exact solver cannot optimize the black-box evaluate FEA objective. Continuous variables discretize through
    // the IntegerStep at the variable level; coefficients apply to the integer-variable values directly.
    static Fin<KernelRun> SolveCpSat(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        problem.Exact.Match(
            Some: model => {
                using var cp = new CpModel();
                IntVar[] vars = [.. problem.Variables.Map(v => v switch {
                    DesignVariable.Integer i => cp.NewIntVar(i.Lower, i.Upper, i.Name),
                    DesignVariable.Categorical c => cp.NewIntVar(0, c.Choices.Count - 1, c.Name),
                    DesignVariable.Continuous co => cp.NewIntVar((long)(co.Lower / policy.IntegerStep), (long)(co.Upper / policy.IntegerStep), co.Name),
                    _ => cp.NewConstant(0),
                })];
                foreach (LinearRow row in model.Rows) {
                    cp.AddLinearConstraint(LinearExpr.WeightedSum(vars, [.. row.Coefficients.Select(static c => (long)Math.Round(c))]), (long)Math.Round(row.Lower), (long)Math.Round(row.Upper));
                }
                cp.Minimize(LinearExpr.WeightedSum(vars, [.. model.Objective.Select(static c => (long)Math.Round(c))]));
                using var solver = new CpSolver { StringParameters = $"max_time_in_seconds:{policy.SolveSeconds}" };
                CpSolverStatus status = solver.Solve(cp);
                return status is CpSolverStatus.Optimal or CpSolverStatus.Feasible
                    ? Harvest(problem, policy, oracle, seed, [.. problem.Variables.Map((v, axis) => DiscreteValue(v, solver.Value(vars[axis]), policy.IntegerStep))])
                    : Fin.Fail<KernelRun>(new ComputeFault.ModelRejected($"<cp-sat-infeasible:{status}>"));
            },
            None: () => Fin.Fail<KernelRun>(ComputeFault.Create("<exact-needs-linear-model:cp-sat>")));

    // MILP builds the OR-Tools SCIP LinearSolver, declares the typed variables, the bounded linear constraints,
    // and the objective coefficients through the typed builder API, and solves to a classified ResultStatus — a
    // mixed continuous+integer problem routes the integer part exactly and the continuous part through the
    // linear backend rather than discretizing.
    static Fin<KernelRun> SolveMilp(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        problem.Exact.Match(
            Some: model => {
                using var solver = Google.OrTools.LinearSolver.Solver.CreateSolver("SCIP");
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
        Probe(problem, oracle, coordinates).Map(point => new KernelRun(seed.Insert(point), 1, policy.TrustRadius, Seq1(point.Violation)));

    static double DiscreteValue(DesignVariable variable, long raw, double step) =>
        variable is DesignVariable.Continuous ? raw * step : variable.Clamp(raw);

    // Multi-start wraps any inner local row with a LowDiscrepancy.Sobol-seeded basin restart (the genuine low-
    // discrepancy generator, never a plain System.Random fill) so global escape is a row; the inner kind is
    // guarded against self-recursion.
    static Fin<KernelRun> MultiStart(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        OptimizerKind inner = policy.MultiStartInner == OptimizerKind.MultiStartGlobal ? OptimizerKind.CmaEs : policy.MultiStartInner;
        return LowDiscrepancy.Sobol(dimensions: problem.Variables.Count, seed: policy.Seed, Scramble.DigitalShift).Bind(generator =>
            toSeq(Enumerable.Range(0, Math.Max(1, policy.Restarts)))
                .Fold(Fin.Succ((Gen: generator, Front: seed, Violation: Seq<double>())),
                    (acc, _) => acc.Bind(state => {
                        var (next, point) = state.Gen.Draw();
                        ImmutableArray<double> start = [.. problem.Variables.Map((v, axis) => v.Clamp(v.Lower + (axis < point.Length ? point[axis] : 0.0) * v.Width))];
                        return Probe(problem, oracle, start).Bind(seeded =>
                            Invoke(inner, problem, policy with { Kind = inner }, oracle, state.Front.Insert(seeded))
                                .Map(run => (next, run.Front, state.Violation + run.Violation)));
                    }))
                .Map(state => new KernelRun(state.Front, policy.Restarts * Math.Max(1, policy.Generations), policy.TrustRadius, state.Violation)));
    }

    // Robust min-max/RBDO: the outer GA scores each candidate as the worst-case over RandomVariable-sampled
    // scenarios (shared UQ inputs) and appends a reliability chance constraint beta_target - beta <= 0 (pf the
    // sampled limit-state exceedance, beta = Normal.InvCDF(1 - pf)) onto the ConstraintHandling axis.
    static Fin<KernelRun> RobustMinimax(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        Seq<ImmutableArray<double>> scenarios = Scenarios(policy);
        int extra = policy.Uncertainties.IsEmpty ? 0 : 1;
        DesignProblem robustProblem = problem with { Constraints = problem.Constraints + extra };
        Func<DesignPoint, Fin<Seq<double>>> robust = point => ScenarioWorst(problem, policy, oracle, point, scenarios);
        return GeneticEngine.Evolve(robustProblem, policy, robust, new ParetoFront(seed.Points, robustProblem.Senses));
    }

    // Centered tolerance perturbations sampled from the RandomVariable set through the SAME LowDiscrepancy.Sobol
    // + inverse-transform Quantile route the UQ lane uses, so robust optimization and forward UQ genuinely
    // share inputs rather than a parallel raw double[] scenario family.
    static Seq<ImmutableArray<double>> Scenarios(OptimizerPolicy policy) {
        if (policy.Uncertainties.IsEmpty) { return Seq<ImmutableArray<double>>(); }
        int dimensions = policy.Uncertainties.Count;
        return LowDiscrepancy.Sobol(dimensions: dimensions, seed: policy.Seed, Scramble.DigitalShift)
            .Map(generator => toSeq(Enumerable.Range(0, Math.Max(1, policy.Population)))
                .Fold((Gen: generator, Rows: Seq<ImmutableArray<double>>()), (acc, _) => {
                    var (next, u) = acc.Gen.Draw();
                    ImmutableArray<double> row = [.. policy.Uncertainties.Map((rv, axis) => rv.Quantile(axis < u.Length ? u[axis] : 0.5) - rv.Quantile(0.5))];
                    return (next, acc.Rows.Add(row));
                }).Rows)
            .IfFail(Seq<ImmutableArray<double>>());
    }

    static Fin<Seq<double>> ScenarioWorst(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, DesignPoint point, Seq<ImmutableArray<double>> scenarios) {
        int m = problem.Objectives.Count, k = problem.Constraints;
        double[] worstObj = [.. Enumerable.Range(0, m).Select(i => problem.Senses[i] > 0 ? double.MinValue : double.MaxValue)];
        double[] worstCon = [.. Enumerable.Repeat(double.MinValue, k)];
        Seq<ImmutableArray<double>> probes = scenarios.IsEmpty ? Seq1(ImmutableArray<double>.Empty) : scenarios;
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

    // --- CMA-ES (covariance-matrix-adaptation evolution strategy; no admitted package owns it) ---------------

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
        var rng = new Random(policy.Seed);
        var initial = new CmaState(
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
                var ordered = carry.Offspring.OrderBy(static o => o.Fitness).Take(mu).ToArray();
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

    // --- PSO (Clerc-constriction particle swarm; no admitted package owns it) ------------------------------

    sealed record SwarmState(ImmutableArray<double>[] Position, double[][] Velocity, ImmutableArray<double>[] Best, double[] BestFitness, ImmutableArray<double> Global, double GlobalFitness, double[] Multipliers, ParetoFront Front, Seq<double> Violation);

    static Fin<KernelRun> EvolvePso(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        int particles = Math.Max(2, policy.Population);
        if (!problem.Variables.Exists(static v => v.Free)) { return Fin.Fail<KernelRun>(ComputeFault.Create("<pso-no-free-variable>")); }
        const double chi = 0.7298, phi = 2.05;
        var rng = new Random(policy.Seed);
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
                        var (next, u) = carry.Gen.Draw();
                        ImmutableArray<double> pos = [.. problem.Variables.Map((v, axis) => v.Clamp(v.Lower + (axis < u.Length ? u[axis] : 0.0) * v.Width))];
                        return Probe(problem, oracle, pos).Map(point => (next, carry.Items.Add((pos, Fitness(problem, policy, point, new double[problem.Constraints]), point))));
                    }))
                .Map(carry => {
                    var items = carry.Items.ToArray();
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

    // --- Simulated annealing (Metropolis ensemble with geometric cooling; no admitted package owns it) -----

    static Fin<KernelRun> Anneal(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        int chains = Math.Max(1, policy.Population), steps = Math.Max(1, policy.Generations);
        double cooling = Math.Pow(1e-3, 1.0 / steps);
        var rng = new Random(policy.Seed);
        return LowDiscrepancy.Sobol(dimensions: problem.Variables.Count, seed: policy.Seed, Scramble.DigitalShift).Bind(generator =>
            toSeq(Enumerable.Range(0, chains))
                .Fold(Fin.Succ((Gen: generator, Front: seed, Multipliers: new double[problem.Constraints], Violation: Seq<double>())),
                    (acc, _) => acc.Bind(state => {
                        var (next, u) = state.Gen.Draw();
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

// The NSGA-style proposal capsule: GeneticSharp.GeneticAlgorithm drives the `nsga2` row over a DesignVariable-
// mapped genome, a FuncFitness bound to the one evaluate oracle, a row-selected operator set, and a deadline-
// bounded ParallelTaskExecutor — the GA machinery is the admitted package's. The multi-objective selection
// pressure is the candidate's non-dominated rank against the running front, with the ParetoFront crowding
// distance breaking within-rank ties toward the more-diverse region (the NSGA-II crowded comparison), under
// the feasibility-aware ConstraintHandling.Dominates comparator; the ParetoFront accumulation is the optimizer
// page's, not the package's. The engine runs its full generation budget once (never re-run per outer generation).
public static class GeneticEngine {
    public static Fin<KernelRun> Evolve(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront seed) {
        RandomizationProvider.Current = new FastRandomRandomization();
        IChromosome adam = Genome(problem);
        var population = new Population(Math.Max(2, policy.Population), Math.Max(2, policy.Population), adam) { GenerationStrategy = new PerformanceGenerationStrategy() };
        var harvest = Atom(seed);
        double[] multipliers = new double[problem.Constraints];
        var fitness = new FuncFitness(chromosome =>
            Optimizer.Probe(problem, evaluate, DecodeRaw(problem, chromosome)).Match(
                Succ: point => {
                    harvest.Swap(f => f.Insert(point));
                    ParetoFront front = harvest.Value;
                    int rank = front.Points.Count(p => problem.Handling.Dominates(p, point, problem.Senses.AsSpan()));
                    // NSGA-II crowded comparison: rank is the primary pressure, the candidate's ParetoFront crowding
                    // distance breaks ties WITHIN a rank toward the less-crowded (more diverse) region. ParetoFront.Insert
                    // appends a surviving (non-dominated) candidate last, so its crowding slot is the final index; a
                    // dominated candidate (rank > 0, not in the front) takes no diversity bonus. The bonus is capped
                    // strictly below the rank quantum (1.0) so crowding never crosses a rank boundary; a boundary point
                    // (infinite crowding) saturates to the maximal in-rank bonus.
                    int slot = front.Points.Count - 1;
                    ImmutableArray<double> crowding = front.Crowding();
                    double diversity = slot >= 0 && slot < crowding.Length && front.Points[slot].Equals(point) ? crowding[slot] : 0.0;
                    double bonus = double.IsFinite(diversity) ? diversity / (1.0 + diversity) : 1.0;
                    double penalty = problem.Handling.Penalize(point.Objectives.IsDefaultOrEmpty ? 0.0 : point.Objectives[0] * problem.Senses[0], point.Constraints.AsSpan(), policy.PenaltyWeight, multipliers);
                    return -rank + 0.5 * bonus - 1e-9 * penalty;
                },
                Fail: static _ => double.MinValue));
        var algorithm = new GeneticAlgorithm(population, fitness, Selection(policy), new TwoPointCrossover(), new UniformMutation(true)) {
            CrossoverProbability = (float)policy.CrossoverRate,
            MutationProbability = (float)policy.MutationRate,
            Reinsertion = new ElitistReinsertion(),
            Termination = new OrTermination(new GenerationNumberTermination(Math.Max(1, policy.Generations)), new FitnessStagnationTermination(Math.Max(2, policy.Generations / 4))),
            TaskExecutor = new ParallelTaskExecutor { MaxThreads = Environment.ProcessorCount, Timeout = TimeSpan.FromSeconds(policy.SolveSeconds) },
        };
        algorithm.Start();
        return Fin.Succ(new KernelRun(harvest.Value, algorithm.GenerationsNumber, policy.TrustRadius, Seq1(Optimizer.Worst(harvest.Value))));
    }

    // The space-filling candidate pool the Bayesian acquisition ranks: a seed-keyed LowDiscrepancy.Sobol net
    // lowered onto the free design box — the SAME owned generator every stochastic kernel draws, so the pool is
    // reproducible from `seed` (the acquisition advances it per generation) rather than an unseeded global RNG.
    public static Seq<ImmutableArray<double>> Candidates(DesignProblem problem, int count, int seed) =>
        LowDiscrepancy.Sobol(dimensions: problem.Variables.Count, seed: seed, Scramble.DigitalShift)
            .Map(generator => toSeq(Enumerable.Range(0, Math.Max(1, count)))
                .Fold((Gen: generator, Pool: Seq<ImmutableArray<double>>()), (acc, _) => {
                    var (next, u) = acc.Gen.Draw();
                    ImmutableArray<double> raw = [.. problem.Variables.Map((v, axis) => v.Clamp(v.Lower + (axis < u.Length ? u[axis] : 0.0) * v.Width))];
                    return (next, acc.Pool.Add(raw));
                }).Pool)
            .IfFail(Seq<ImmutableArray<double>>());

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

## [03]-[RESEARCH]

- [METAHEURISTIC_KERNELS]: the five population/metaheuristic kinds are five GENUINE kernels, not one engine with swapped operators. `nsga2` is the only one an admitted package owns — `GeneticSharp.GeneticAlgorithm` supplies the GA loop/operators/executor and the page supplies the non-dominated rank + crowding comparator and the `ParetoFront`. `cma-es` (rank-`μ`/rank-one covariance adaptation with CSA step control, eigendecomposing the symmetrized covariance through `Matrix<double>.Evd(Symmetricity.Symmetric)` and sampling `Normal.Sample`), `pso` (Clerc-constriction velocity/personal-best/global-best swarm), and `simulated-annealing` (Metropolis acceptance over a geometric cooling schedule across a `Population`-wide chain ensemble) are distinct algorithms NO admitted package owns, so they are authored as in-package folds over the admitted MathNet/`Distributions` numeric substrate exactly as the closed-form physics, SIMP optimality criteria, and GP kernel already are — the doctrine forbids hand-rolling what a package owns, and no package owns these. The settled leaf is the orchestration: each kernel owns its full generation/iteration loop and adaptation state and the `Optimize` entry calls it once, so the legacy per-generation re-run of a full inner GA (`Generations²` GA generations) is removed; the open leaf is none — the kernels are non-degenerate independent of consumer wiring.
- [CONSTRAINT_AND_RELIABILITY]: the `evaluate` oracle returns the objective vector concatenated with the constraint vector and `Optimizer.Probe` splits it by `Objectives.Count`, so the `ConstraintHandling` axis (death/static penalty, Deb feasibility-rules dominance, and the augmented-Lagrangian with a live `λ ← max(0, λ + ρ·g)` advance read by `Penalize`) is genuinely reachable rather than a permanently-empty constraint set; the `robust-minimax` row samples scenarios from the `Solver/uncertainty#UNCERTAINTY_LANE` `RandomVariable` set through the same `LowDiscrepancy.Sobol` + inverse-transform `Quantile` route the UQ lane uses and appends the reliability chance constraint `β_target − β ≤ 0` (`pf` the sampled limit-state exceedance, `β = Normal.InvCDF(1 − pf)`) so robust optimization and forward UQ share inputs and RBDO is a constraint row. The deep reliability methods (FORM/SORM/PCE) stay the uncertainty lane's; the optimizer composes only the inputs and the simulation-based `pf`/`β`.
- [GRADIENT_ADJOINT]: the gradient-adjoint and topology-SIMP rows read the genuine reverse-mode sensitivity through the VERIFIED two-argument `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Chain(Seq<GeometryTape>, seed)` overload (a phantom three-argument `Chain(tape, inputs, seed)` call is removed), the `DesignVariable.AdjointOperator` total `Switch` lowering each free design field to its DDG `TensorOpFamily` (`Continuous` → `Gradient`, `Density` → `CotangentLaplacian`, the discrete/linked cases to `None`, so a new case decides its operator at compile time) and producing a snapshot-bearing `Seq<GeometryTape>` over the `DesignProblem.DesignMesh`; the SIMP optimality-criteria update is driven by that genuine compliance sensitivity `dc/dx_e` (never a constant `−1/λ` base) and bisects the Lagrange multiplier to the volume constraint. A `DesignProblem` carrying no `DesignMesh` lowers an empty tape and descends degenerate by construction (the absent-mesh case, never an absent operator); the open leaf is the live design-mesh wiring at the shape/topology call site that supplies the `MeshAdjointSnapshot` through `MeshAdjointSnapshot.Of` and the real objective cotangent the unit-seed default stands in for.
- [EXACT_AND_LOWDISCREPANCY]: the exact `cp-sat`/`milp` rows lower the typed `DesignProblem.Exact` `LinearModel` to a genuine OR-Tools `CpModel`/`Solver` objective+constraint set (CP-SAT integer-native, MILP routing the integer part to SCIP and the continuous part through the linear backend) and fault `<exact-needs-linear-model>` when absent, because an exact solver cannot optimize the black-box `evaluate` FEA objective the metaheuristic rows drive — the open leaf is the consumer supplying the typed `LinearModel` for a genuinely combinatorial sizing/selection problem. The `multi-start-global`, `pso`, `simulated-annealing`, and `robust-minimax` rows seed from the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy.Sobol(dimensions, seed, Scramble.DigitalShift)` generator threaded through `.Draw()`, identical to the uncertainty lane; the `LowDiscrepancy` owner is settled in `Tensor/sampling.md` (the seed-explicit state-serializable `LowDiscrepancy` carrier, the `Sobol`/`Halton` factories, and the `Scramble` policy under the owned-build sampler lane), shared verbatim by this page, the `Solver/uncertainty#UNCERTAINTY_LANE`, and the `Solver/sweep#SWEEP_AND_BUDGET` lanes.
