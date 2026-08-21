# [COMPUTE_OPTIMIZER]

Rasm.Compute solver optimizer: one `Optimizer` design-space-search axis over a typed `DesignVariable`/`ActivationRule`/`ConstraintHandling`/`ObjectiveSense` problem, dispatching one polymorphic `Optimize` entry by `OptimizerKind` row to a per-family kernel that owns its iteration budget and adaptation state.

Kernel families: NSGA multi-objective evolution over `GeneticSharp.GeneticAlgorithm`, CMA-ES rank-`μ`/rank-one covariance adaptation, Clerc-constriction PSO, Metropolis simulated annealing, Bayesian-GP acquisition, gradient-adjoint trust-region/Armijo descent, topology-SIMP optimality criteria, OR-Tools CP-SAT/MILP exact solving over the package's own `Domain` set algebra and ConstraintSolver vehicle routing, MathNet box-bounded and limited-memory quasi-Newton with derivative-free simplex refinement, `LowDiscrepancy.Sobol` multi-start restart, and robust-minimax/RBDO.

Owned surface: the `OptimizerKind`/`SearchTrait`/`DesignVariable`/`ActivationRule`/`ConstraintHandling`/`LineSearch`/`SmoothMinimizer`/`AcquisitionFunction`/`SurrogateKind` vocabulary, and the `LinearModel`/`LinearRow`/`DesignProblem`/`DesignPoint`/`ParetoFront`/`OptimizerPolicy`/`SearchContext`/`SearchSpend`/`KernelRun`/`OptimizationResult` carriers. Objective direction is the kernel `Rasm/Solving/solver#LM_FUNCTOR` `ObjectiveSense`, composed and never re-declared. The three OR-Tools engines the `cp-sat`/`milp`/`routing` rows dispatch to — with `ExactEvidence`/`ShadowPrice`/`BoundStream` and the whole routing vocabulary — are `Solver/exact#EXACT_LANE`'s: they share one package rail, one refusal vocabulary, and one evidence carrier with each other and no type at all with a search kernel.

`Surrogate`/`OutputModel`/`GpModel`/`RbfModel`/`NeuralFieldModel` are the reduced-order models, and the `Optimizer` fold rides beside the `GeneticEngine`/`NsgaFitness` GeneticSharp capsule and the `RoutingSearch` lowering. `evaluate` is one `Func<DesignPoint, Fin<Seq<double>>>` returning the objective vector concatenated with the constraint vector, split by `problem.Objectives.Count` so `ConstraintHandling` stays reachable; full `Solver/contract#SOLVE_REQUEST` evaluation and `Surrogate.Predict` both remain on `Fin`, and the surrogate result carries its bound as well.

`Optimizer.Analytic` is the ANALYTIC leg of that one contract: a `Symbolic/lowering#LOWERING` `CompiledExpr` per objective and per constraint, bound to the design space by SYMBOL NAME and invoked in the concatenation order the contract already defines — so a design problem whose objective is a cost or code formula searches its compiled expression at a few nanoseconds per point instead of an FE solve, and the `DesignVariable.Symbolic` case gains the VALUE half of the arm whose gradient half `AdjointTape.Symbolic` already carries. A symbol the design space does not declare is a typed refusal, never a zero fill.

Gradient-adjoint dispatches the closed `AdjointTape` union — the `Geometry` case chains `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Chain` over the tapes lowered from `DesignProblem.DesignMesh`, the `Symbolic` case chains `Symbolic/lowering#SYMBOLIC_JACOBIAN` `SymbolicJacobian.Backward` over one design-point-carrying `SymbolicTape` — under an objective-sense cotangent seed.

GP-covariance Cholesky and marginal-likelihood ride `Tensor/blas#DENSE_ALGEBRA` `Cholesky<double>`, the multivariate trend `MathNet.Numerics.Fit.MultiDim`, and the neural field the `Model/run#RUN_MODES` `RunOps.Infer` OrtValue run keyed by the parametric-family `XxHash128` digest.

Settled arrivals: the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, the kernel `Rasm/Domain/rails#TRANSITION` `Transition<TState>`/`Cell` verdict every lock-free seat answers, the kernel `Rasm/Domain/validation#CAPABILITY` `ICapability`/`CapabilitySet<TCapability>` column, the `Solver/contract#SOLVE_REQUEST` `Convergence` bounded-budget verdict, NodaTime `IClock` for semantic stamps with kernel `MonotonicTimeline` for elapsed spans (the app-stratum `ClockPolicy` stops at the app root), the Thinktecture `ComparerAccessors.StringOrdinal` accessor, the `Rasm.Meshing` `MeshAdjointSnapshot` / `Rasm.Numerics` `DiscreteCalculus` DDG-adjoint surface, and the `GeometryTape` shape. `ParetoFront` crosses to Persistence content-keyed and `Surrogate` crosses to `Solver/clash#CLASH_AND_TWIN` as the digital-twin baseline.

## [01]-[INDEX]

- [02]-[OPTIMIZER_LANE]: design-var/link/conditional search; per-family kernels; constraint axis; ROM/GP/field surrogate duality.

## [02]-[OPTIMIZER_LANE]

- Owner: `OptimizerKind` `[SmartEnum<string>]` search-algorithm rows carrying the draw-lane column and the `CapabilitySet<SearchTrait>` the admission gate reads; `SearchTrait` `[SmartEnum<string>]` the closed search-capability vocabulary (`population` · `gradient` · `exact`) conforming the kernel `ICapability<SearchTrait>` floor; `SearchSpend<TState>` the bounded-budget fold carrier binding a state to its `Convergence` verdict and the generations it spent; `DesignVariable` `[Union]` typed variable cases (free + linked/derived) with the `Width` coordinate-span column, the `Extent` per-slot box, and the `Admissible` `Domain`/`Malformed` shape projections; `ActivationRule` `[Union]` conditional active-set cases with the `Reads`/`Trigger` reification pair; `ConstraintHandling` `[SmartEnum<string>]` feasibility-policy rows (death-penalty/static-penalty/feasibility-rules/augmented-lagrangian) with a live multiplier-advance; `LineSearch` `[SmartEnum<string>]` gradient line-search/trust-region rows; `SmoothMinimizer` `[SmartEnum<string>]` the MathNet minimizer rows the smooth-local kinds drive; `AcquisitionFunction` `[SmartEnum<string>]` Bayesian-acquisition rows (expected-improvement/upper-confidence-bound/probability-of-improvement); `SurrogateKind` `[SmartEnum<string>]` surrogate-model rows (linear-trend/gaussian-process/radial-basis/neural-field); `LinearModel`/`LinearRow` the typed objective+constraint model the exact `cp-sat`/`milp` rows lower to OR-Tools, each row named and carrying its admissible BAND SET; `DesignProblem` the variable/activation/constraint/objective record with the OFFSET TABLE every kernel indexes through, the link+active-set `Resolve` fold, the `Scale`/`Admissible` exact-lowering projections, and the optional `LinearModel`/`RoutingProblem` the `Solver/exact#EXACT_LANE` rows read; `DesignPoint` the coordinate/objective/constraint sample with its indexed `Response` read; `OptimizerPolicy` the per-kind tuning record; `SearchContext` the cooperative-stop-plus-observation capability pair every kernel takes as one argument; `ParetoFront` the queryable non-dominated-set artifact with crowding-distance ranking and exact bi-objective hypervolume; `KernelRun` the per-kernel run result the `Optimize` fold projects onto `OptimizationResult`; `Optimizer` the static search fold dispatching one `Optimize` entry by `OptimizerKind` to its genuine per-family kernel; `GeneticEngine`/`NsgaFitness` the `GeneticSharp.GeneticAlgorithm` capsule and its snapshot-ranked multi-objective `IFitness` (genome from the free coordinate slots, the fast non-dominated sort and crowding comparator over the `evaluate` oracle, `ParallelTaskExecutor` on the bounded lanes); `Surrogate`/`OutputModel` the multi-output reduced-order/learned model carrying one leg per contract output beside a content-keyed `NeuralFieldModel`; `GpModel`/`RbfModel` the scattered-data posteriors; `NeuralFieldModel` the parametric-family-digest-keyed coordinate-MLP/Fourier-feature field evaluated through the model-lane OrtValue run.
- Cases: `SearchTrait` rows population · gradient · exact; `OptimizerKind` rows nsga2 · bayesian-gp · gradient-adjoint · topology-simp · simulated-annealing · cma-es · pso · cp-sat · milp · multi-start-global · robust-minimax · bfgs-box · bfgs-limited · nelder-mead · routing, each declaring the `CapabilitySet<SearchTrait>` its kernel actually needs (`population` for nsga2/cma-es/pso/robust-minimax; `gradient` for gradient-adjoint/topology-simp/bfgs-box/bfgs-limited; `exact` for cp-sat/milp/routing — the three smooth-local rows binding the MathNet minimizer family, routing binding the OR-Tools ConstraintSolver rail); `SmoothMinimizer` rows bfgs-box · bfgs-limited · nelder-mead (`gradient=true` for the two quasi-Newton rows); `DesignVariable` cases `Continuous` · `Integer` · `Categorical` (with its own admissible-ordinal roster) · `Density` (topology field, `Width = Cells`) · `Linked` (shared/derived — `Scale·source + Offset`, `Free=false`) · `Symbolic` (bounded design symbol whose partial arrives on the `AdjointTape.Symbolic` tape); `ActivationRule` cases `Always` · `WhenAbove` · `WhenBelow` · `WhenChoice`; `ConstraintHandling` rows death-penalty · static-penalty · feasibility-rules · augmented-lagrangian (`multiplierUpdate=true` only for augmented-lagrangian); `LineSearch` rows fixed · armijo-backtrack · trust-region; `AcquisitionFunction` rows expected-improvement · upper-confidence-bound · probability-of-improvement; `SurrogateKind` rows linear-trend · gaussian-process · radial-basis · neural-field; `Convergence` (composed) `Converged` · `Exhausted` · `Stalled`.
- Entry: `public static Fin<OptimizationResult> Optimize(DesignProblem problem, OptimizerPolicy policy, CpuBudget budget, Func<DesignPoint, Fin<Seq<double>>> evaluate, SearchContext search, IClock clock)` — entry overwrites `policy.Parallelism` from `budget.Workers` before validation and dispatch, so no caller or ambient processor count can widen evaluation; `search` carries the cooperative stop and the optional observation cell as ONE capability argument, never a token tail. `Fin<T>` aborts on an invalid design space, policy, oracle output, or kernel state; `evaluate` returns exactly `Objectives.Count + Constraints` finite values with objectives first. `DesignProblem.Validate` and `OptimizerPolicy.Validate` both ACCUMULATE — every independent defect the pass already computes reaches the caller as its own typed `ComputeFault` arm through `Validation<Error,Unit>`, never one opaque code standing for sixteen conditions. One shared `Atom<(int Evals, int Hits)>` meters the full and surrogate oracles, and a surrogate hit is admitted only when its bound and output cardinality satisfy the problem contract. `public static Fin<Func<DesignPoint, Fin<Seq<double>>>> Analytic(DesignProblem problem, Seq<CompiledExpr> objectives, Seq<CompiledExpr> constraints)` is the analytic oracle mint over the same contract — it proves the symbol binding ONCE and hands back the oracle, so no evaluation re-resolves a name.
- Auto: `Optimize` dispatches each `OptimizerKind` row through one generated total `Switch` (`Invoke`) to its genuine kernel, invoked exactly once, so a NEW row breaks the dispatch at COMPILE time rather than faulting at runtime; `multi-start-global` re-enters the same dispatch for its inner row carrying the same `SearchContext`. `Admits` is the ONE arm the trait column feeds: a `population` row over a policy that cannot seat two individuals and a `gradient` row over a design space carrying no differentiable tape both refuse BY NAME at admission, where the per-kernel `Math.Max(2, …)` floors silently widened the first and a zero gradient silently froze the second. Every kernel spends its budget through the one `Spend` fold, so each returns a `Convergence` verdict rather than a generation count read off the policy. Constraint handling is the `ConstraintHandling` row and the surrogate duality is a policy column gating cheap-versus-full evaluation with the surrogate-hit count metered honestly. Every stochastic kernel draws from the kernel `Deterministic.Source` keyed on `(OptimizerKind.Lane, …)` under `OptimizerPolicy.Seed`; the `nsga2` row also pins the package-global provider through `FastRandomRandomization.ResetSeed`, since assigning the provider alone leaves an entropy-seeded generator, and ranks each generation against a FROZEN snapshot so evaluation order never enters the fitness. Exact rows lower every coordinate SLOT through `DesignVariable.Admissible` and every row through its band set, reify each conditional axis as one literal, register one assumption literal per row, hint from the incoming front, seal `num_search_workers`/`SetNumThreads` from the same governed parallelism, and register the cooperative stop against the solve handle.
- Receipt: the `Optimization` `ComputeReceipt` case carries the optimizer key, the generations the search actually SPENT, the metered evaluated-point count, the metered surrogate-hit count, the front size, the hypervolume indicator, and `ReferenceDerived` — the flag the receipt owner declares with its own comparability law and this fold is the sole producer of, so an emitted receipt reports the box it measured against rather than publishing `false` as a measured fact. The reference box itself, the `Convergence` verdict, the constraint-violation history, the trust-region radius, the exact-lane `ExactEvidence`, and the routing assignment ride the `OptimizationResult` carrier, and the per-evaluation surrogate error bound and GP marginal-likelihood ride the `Surrogate`/`GpModel`, so an acceptance is auditable without a receipt slot the `Runtime/receipts#RECEIPT_UNION` owner does not declare.
- Packages: MathNet.Numerics (the dense `Matrix<double>.Evd`/`Cholesky` algebra, `Fit.MultiDim` the multivariate least-squares trend, `Statistics.Median` the kernel length-scale order statistic, `GoodnessOfFit.StandardError` the residual standard error, `Distributions.Normal` reliability/sampling, and the `BfgsBMinimizer`/`LimitedMemoryBfgsMinimizer`/`NelderMeadSimplex` smooth-local family behind the three refinement rows), GeneticSharp (the `GeneticAlgorithm` engine + chromosome/operator/executor catalog behind the `nsga2` row, its multi-objective `IFitness` this page's), Google.OrTools (CP-SAT `CpModel`/`CpSolver`/`SolutionCallback` + the `Google.OrTools.Util` `Domain` set algebra + LinearSolver `Solver` + the `Google.OrTools.ConstraintSolver` routing rail behind the `cp-sat`/`milp`/`routing` rows), System.Numerics.Tensors, Microsoft.ML.OnnxRuntime (the neural-field `OrtValue` run), Generator.Equals (`[Equatable]` + `[OrderedEquality]` the generated `DesignPoint` structural identity every front set and genome memo keys on), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Prelude.foldWhileM` the one halt-bearing budget fold, `Validation<Error,T>` the accumulating admission), NodaTime, Rasm (project, the `MeshAdjointSnapshot`/`DiscreteCalculus` public surface for the DDG gradient-adjoint tape, `Deterministic.Source` as the one draw owner, `ObjectiveSense` as the branch objective-direction vocabulary, `ICapability`/`CapabilitySet` as the one capability column, and `Transition`/`Cell` as the one lock-free transition verdict), Rasm.Persistence (project), BCL inbox
- Growth: a new search algorithm is one `OptimizerKind` row carrying its own draw lane, its own `CapabilitySet<SearchTrait>`, and one arm on the `Optimize` total `Switch` (`Invoke`) — a population-based row binds its own update rule (CMA-ES covariance, PSO velocity, SA Metropolis) or `GeneticEngine` for a genuine GA; a smooth-local row is one `SmoothMinimizer` row and one `Invoke` arm over the shared `Smooth` fold, never a fourth minimizer body; an exact OR-Tools row is one lowering at `Solver/exact#EXACT_LANE`; a wrapping row composes the inner kernel through the same dispatch — and the generated `Switch` breaks at COMPILE time until the arm is added (never a runtime kind-miss); a variable case admitting a new shape of set is one `Admissible` arm and one `Malformed` arm the generated dispatch demands together, and a case occupying more than one coordinate is one `Width` arm the offset table then carries everywhere; a new genetic operator is one construction column on the `GeneticEngine.Evolve` assembly binding the `GeneticSharp` `ICrossover`/`IMutation` row, never a per-operator engine arm; a new variable kind is one `DesignVariable` case carrying its `AdjointOperator`; a new constraint discipline is one `ConstraintHandling` row; a new line-search/trust-region is one `LineSearch` row; a new acquisition is one `AcquisitionFunction` row; a new surrogate model is one `SurrogateKind` row and a `Fit` arm; a new search capability is one `SearchTrait` row and one `Admits` clause; a new termination is one `Convergence` case at `Solver/contract`, breaking every consumer's fold loudly; a new objective is one row on the `DesignProblem` objective set; zero new surface — an `Nsga2Engine`/`BayesianOptimizer`/`CmaEsSolver`/`ParticleSwarm`/`Annealer`/`TopologyOptimizer`/`MultiStartRunner` sibling family is collapsed onto the one `Optimize` total `Switch`, a `LinkedVariable`/`DerivedVariable` family onto `DesignVariable.Linked`, a `PenaltyHandler`/`FeasibilityHandler` family onto `ConstraintHandling`, and a `SurrogateNet`/`FieldPredictor` sibling onto the `Surrogate.NeuralField` row.
- Boundary: EVERY bounded budget on this lane terminates through one `Spend` fold and answers a `Convergence`, because a `Range(0, Generations).Fold` with no halt runs its full policy budget whatever the residual and then reports that policy value as the search — a success-shaped fall-through certifying an unconverged search as converged, and the exact reason `Generations` on a receipt used to mean "ran to plan" rather than "settled". The halt is the kernel's OWN settle test (a gradient inside `Tolerance`, a density delta inside it, a front whose best objective stopped improving), the stall floor separates `Stalled` from `Exhausted`, and the smooth-local rows read `MinimizationResult.ReasonForExit` onto the same three cases instead of collapsing `Converged`, `ExceedIterations`, and `ManuallyStopped` into one success. A kernel that cannot state a settle test spends its whole budget and says `Exhausted` — which is the truth, not a defect.
- Boundary: contract-uniform — `evaluate` is the single coupling point, so the search composes a full FEA solve, a railed `Surrogate.Predict`, or an `Analytic` compiled-expression oracle without a parallel search path per source. `Analytic` binds each `CompiledExpr` to the design space by SYMBOL NAME through the expression's own `SymbolOrder`, so an expression authored over `{depth, width}` and a design space declaring `{width, depth, grade}` agree by name rather than by position — positional binding silently evaluates a cost formula with the wrong variable in every slot and returns a finite number no reader can tell from a correct one. A symbol the design space does not declare refuses by name; a design variable the expression never reads is lawful and simply unread. Objective-vector-then-constraint-vector concatenation keeps the `ConstraintHandling` axis reachable; a permanently-empty constraint set silently disabling penalty/feasibility/augmented-Lagrangian handling is rejected. Typed variables make a bound violation a boundary fault, never a clamped silent repair, and variable-linking and conditional design spaces are rows on the same axis through `DesignProblem.Resolve`.
- Boundary: a `DesignPoint` is the CONCATENATION of per-variable coordinate spans under one offset table, so a `Density` field of `Cells` elements is `Cells` design freedoms every kernel searches — the prior one-coordinate-per-variable shape left the topology field searchable only by the SIMP kernel, which expanded it privately while every other row optimized a single scalar for the whole field. Starts, boxes, unit maps, clamps, exact-lane variables, and genome genes all index through that table, so a new multi-slot case is one `Width` arm rather than a per-kernel expansion.
- Boundary: FIVE genuine kernels — `nsga2` routes `GeneticEngine` over the admitted `GeneticSharp.GeneticAlgorithm` (package owns the GA machinery, page owns the fast non-dominated sort, the crowding comparator, and `ParetoFront`), while `cma-es`, `pso`, and `simulated-annealing` are distinct algorithms no admitted package owns, authored as in-package folds over the `Matrix<double>.Evd`/`Distributions.Normal` substrate. NSGA-II fitness ranks against a SNAPSHOT frozen for the whole generation: ranking against a live archive makes a genome's fitness depend on how many peers happened to be scored first, which under a parallel executor is thread interleaving, so one seed produces a different search every run and the content-addressed front keys a campaign nobody re-derives. Reproducibility is the seeded provider AND the snapshot together, and the archive survives as archive alone. `bayesian-gp` FITS a `GpModel` from the running history each iteration and ranks candidates by the acquisition over the posterior; a loop that never fits the GP and ranks by a constant is rejected.
- Boundary: gradient-adjoint dispatches `problem.AdjointTape` — the Geometry arm reads the VERIFIED two-argument `SensitivityLaw.Chain(tapes, seed)` overload (a phantom three-argument `Chain(tape, inputs, seed)` call is the API trap), the Symbolic arm re-points its `SymbolicTape` at the current origin before `SymbolicJacobian.Backward` — and the cotangent seed carries the objective sense, so a maximize row descends the negated direction and two design states yield two symbolic gradients; `AdjointOperator` lowers `Continuous`→`Gradient` and `Density`→`CotangentLaplacian` at compile time, and an absent `DesignMesh` lowers an empty Geometry case so the descent is degenerate by construction (the absent-mesh case, never an absent operator). Gradients whose arity misses the design dimension REFUSE: zero-padding the tail descends the leading coordinates and freezes the rest, which is a silently reduced search wearing the full problem's name. `OptimizerPolicy.StepLength` owns the descent step as its own column — reading a genetic mutation rate as a step length couples two unrelated axes. `LineSearch` owns the line-search/trust-region; a fixed-step descent without step control is rejected. Topology-SIMP reads the genuine compliance sensitivity from that same adjoint route, iterates the density SPAN, and bisects the Lagrange multiplier to the volume constraint; a density update whose base ignores the structural sensitivity (a constant `−1/λ` power) is the deleted fake. Augmented-Lagrangian carries a LIVE multiplier advanced `λ ← max(0, λ + ρ·g)` each generation and read by `Penalize`; a `MultiplierUpdate` flag degenerating to static penalty is rejected.
- Boundary: the surrogate is MULTI-OUTPUT — one `OutputModel` per contract output, objectives then constraints — so `Predict` answers the same vector shape the full oracle answers and the gated cardinality check is satisfiable; a single-output surrogate never clears it, leaving every surrogate hit structurally unreachable and the metered hit count zero. `Bound` takes the WIDEST per-output bound, because a surrogate is admissible only while every component it answers is inside the policy. The linear-trend row is a genuine MULTIVARIATE least-squares fit through `Fit.MultiDim`: the per-axis marginal regression it replaces divided each axis's covariance by that axis's own variance alone, so under correlated design axes — which every coupled AEC design space has — the trend was systematically biased and its `ResidualRms` under-reported the error the gate admits on. Every surrogate drifting past its bound forces a full re-evaluation, the surrogate-hit count metered honestly through the shared `Atom`; a receipt slot that stays zero is rejected.
- Boundary: `SurrogateKind` is the surrogate axis — the neural-field row threads the leased `Model/run#RUN_MODES` `(InferenceSession, RunOptions, CancelScope)` so `Predict` runs the coordinate-MLP/Fourier field through `RunOps.Infer` behind the same `Fin<(Seq<double> Values, double Bound)>` rail the fitted rows answer. Its weights are NOT fitted in this runtime: `Surrogate.Fit` refuses the row outright, and the trained asset arrives by content key through the `Model/identity#MODEL_IDENTITY` `ModelSource.Acquire` admission under its `GraduationEnvelope` evidence key, exported from `Solver/sweep#SWEEP_AND_BUDGET` as a `DoeDataset` corpus and returned by an external training environment this branch neither names nor constrains. `Surrogate.OfField` is the only mint, and a delegation to the linear trend under the neural-field name is the deleted form (C# owns only inference; an in-proc ORT-Training fit is rejected).
- Boundary: `ParetoFront` is content-addressed onto the Persistence vector index and the exact bi-objective hypervolume is the staircase sweep (≥3-objective a Monte-Carlo estimate over the reference box, drawn on the run's own `(Lane, Seed)` so no literal seed survives on the page); a Lebesgue-box overcount double-counting dominated overlaps is rejected. `OptimizerPolicy.Reference` supplies the reference box where a campaign declares one; otherwise it derives from the front's own worst objective per axis, with the derived flag on the result — a hypervolume against a derived box moves with the front, so two runs are comparable only when both declared the same reference and a reader must never have to assume which happened.
- Boundary: the exact rows are `Solver/exact#EXACT_LANE`'s — the `Domain`-algebra admissible sets, the assumption-literal explanation obligation, the measured `ExactEvidence`, and the OR-Tools disposal law all state once at that owner. What stays here is the DISPATCH contract they honour: an exact row answers the same `KernelRun` a stochastic row answers, so `Invoke` needs no arm that knows which kind it called, and `DesignProblem.Exact`/`Routing` are the two optional slots a campaign fills to make one reachable.
- Boundary: SMOOTH-LOCAL rows bind the MathNet minimizer family behind one `Smooth` fold and one `SmoothMinimizer` row set; nonlinear LEAST SQUARES is deliberately not a row here because `Tensor/blas#LEVENBERG_MARQUARDT` is the package's one damped Gauss-Newton owner and a library-bound Levenberg-Marquardt beside it is the twin that owner forecloses. Constrained smooth nonlinear programming is likewise NOT a row: the SLSQP row it would be bound no surface this repository carries — the source it named is absent tree-wide, so every member of its contract was an unverifiable claim, and a row whose engine cannot be read is a capability the page advertises and cannot honour. It returns as one `OptimizerKind` row and one `Invoke` arm the day that source lands under the package's own vendor tree.
- Boundary: `multi-start-global` wraps any inner row (guarded against self-recursion) with a `LowDiscrepancy.Sobol` basin restart rather than a `System.Random` fill; every other stochastic kernel on the page — CMA sampling, PSO velocity, the annealing proposal, and the hypervolume estimator — draws from the kernel `Deterministic.Source` under `(OptimizerKind.Lane, …)`, so one policy seed yields independent per-row streams and a bare `new Random(seed)` has no site left. `robust-minimax` reads the `Solver/uncertainty#UNCERTAINTY_LANE` `RandomVariable` scenario set through the SAME `LowDiscrepancy.Sobol`+`RandomVariable.Quantile` inverse-transform the UQ lane uses, scores each candidate worst-case, and appends the reliability chance constraint `β_target − β ≤ 0` (`β = Normal.InvCDF(1 − pf)`) onto the `ConstraintHandling` axis so RBDO is a constraint row and the deep FORM/SORM/PCE stay the uncertainty lane's.
- Boundary: parallel fitness evaluation binds under the governed budget — `Optimizer.Optimize` overwrites `OptimizerPolicy.Parallelism` from `CpuBudget.Workers`, and `ParallelTaskExecutor.MaxThreads` reads that sealed value. Admitted `TplPopulation(int, int, IChromosome)` takes the `Population` constructor whole and overrides `CreateInitialGeneration` alone, so every other population member reads unchanged; its `Parallel.For` genome mint exposes no `ParallelOptions` seat, so the initial generation is the ONE leg outside `CpuBudget.Workers` — a fan of `CreateNew` mints paid once at start rather than per generation, where the sealed budget binds the fitness evaluation the executor runs and a plain `Population` substituted to close that leg buys a serial start for nothing.

```csharp signature
// What a search row NEEDS of the problem and policy handed to it, as a capability column rather than a pair of
// bools nothing read. `population` demands a policy that can seat more than one individual, `gradient` demands a
// design space carrying a differentiable tape, `exact` demands a lowered model. The kernel `ICapability` floor
// derives `Rank` from declaration order, so the roster carries no ordinal of its own.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SearchTrait : ICapability<SearchTrait> {
    public static readonly SearchTrait Population = new("population");
    public static readonly SearchTrait Gradient = new("gradient");
    public static readonly SearchTrait Exact = new("exact");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OptimizerKind {
    public static readonly OptimizerKind Nsga2 = new("nsga2", lane: 1L, CapabilitySet<SearchTrait>.Of(SearchTrait.Population));
    public static readonly OptimizerKind BayesianGp = new("bayesian-gp", lane: 2L, CapabilitySet<SearchTrait>.None);
    public static readonly OptimizerKind GradientAdjoint = new("gradient-adjoint", lane: 3L, CapabilitySet<SearchTrait>.Of(SearchTrait.Gradient));
    public static readonly OptimizerKind TopologySimp = new("topology-simp", lane: 4L, CapabilitySet<SearchTrait>.Of(SearchTrait.Gradient));
    public static readonly OptimizerKind SimulatedAnnealing = new("simulated-annealing", lane: 5L, CapabilitySet<SearchTrait>.None);
    public static readonly OptimizerKind CmaEs = new("cma-es", lane: 6L, CapabilitySet<SearchTrait>.Of(SearchTrait.Population));
    public static readonly OptimizerKind Pso = new("pso", lane: 7L, CapabilitySet<SearchTrait>.Of(SearchTrait.Population));
    public static readonly OptimizerKind CpSat = new("cp-sat", lane: 8L, CapabilitySet<SearchTrait>.Of(SearchTrait.Exact));
    public static readonly OptimizerKind Milp = new("milp", lane: 9L, CapabilitySet<SearchTrait>.Of(SearchTrait.Exact));
    public static readonly OptimizerKind MultiStartGlobal = new("multi-start-global", lane: 10L, CapabilitySet<SearchTrait>.None);
    public static readonly OptimizerKind RobustMinimax = new("robust-minimax", lane: 11L, CapabilitySet<SearchTrait>.Of(SearchTrait.Population));
    // Smooth local refinement over the MathNet minimizer family — box-bounded quasi-Newton where the design space
    // carries bounds, limited-memory where the dimension is wide, and derivative-free simplex where the objective
    // exposes no gradient at all. Nonlinear LEAST SQUARES is NOT a row here: `Tensor/blas#LEVENBERG_MARQUARDT` is the
    // one damped Gauss-Newton owner in the package, and a second Levenberg-Marquardt bound to the library minimizer
    // would be the twin that owner exists to foreclose.
    public static readonly OptimizerKind BfgsBox = new("bfgs-box", lane: 13L, CapabilitySet<SearchTrait>.Of(SearchTrait.Gradient));
    public static readonly OptimizerKind BfgsLimited = new("bfgs-limited", lane: 14L, CapabilitySet<SearchTrait>.Of(SearchTrait.Gradient));
    public static readonly OptimizerKind NelderMead = new("nelder-mead", lane: 15L, CapabilitySet<SearchTrait>.None);
    // Vehicle routing over the OR-Tools ConstraintSolver rail — the third exact engine beside CP-SAT and the MILP
    // backend, and the only one whose model is a graph rather than a coefficient matrix.
    public static readonly OptimizerKind Routing = new("routing", lane: 16L, CapabilitySet<SearchTrait>.Of(SearchTrait.Exact));

    // Draw lane: every stochastic kernel keys the kernel `Deterministic` source on `(Lane, …)` so one policy seed
    // yields independent streams per row, and a `multi-start-global` wrap re-entering its inner row never replays the
    // outer row's draws. Each row declares the column rather than deriving it from the key string, so a row rename
    // never silently re-keys a reproducible campaign.
    public long Lane { get; }

    public CapabilitySet<SearchTrait> Traits { get; }

    // The ONE arm the trait column feeds, and the reason the column exists at all. A `population` row over a policy
    // seating one individual is a swarm of one, which the per-kernel `Math.Max(2, Population)` floors used to widen
    // silently into a search the caller never asked for; a `gradient` row over a design space whose tape carries no
    // operator descends a zero direction and reports the start point as the optimum. Both refuse BY NAME here, and
    // the refusal carries the missing rows through the kernel's own `Require` door.
    public Fin<Unit> Admits(DesignProblem problem, OptimizerPolicy policy) =>
        Held(problem, policy)
            .Require(Traits, missing => new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Input)))
            .Map(static _ => unit);

    static CapabilitySet<SearchTrait> Held(DesignProblem problem, OptimizerPolicy policy) =>
        CapabilitySet<SearchTrait>.Of([
            .. policy.Population >= 2 ? Seq(SearchTrait.Population) : Seq<SearchTrait>(),
            .. problem.Differentiable ? Seq(SearchTrait.Gradient) : Seq<SearchTrait>(),
            .. problem.Exact.IsSome || problem.Routing.IsSome ? Seq(SearchTrait.Exact) : Seq<SearchTrait>()]);
}

// Which MathNet minimizer a smooth-local row drives and whether it consumes the adjoint gradient. Each row owns
// its own minimizer call, so the shared admission, objective construction, and exit-condition lift live once on
// `Optimizer.Smooth` and a fourth minimizer is one row plus one delegate. The three convergence tolerances are the
// POLICY'S — embedding them as constructor literals put three per-row thresholds where no caller and no receipt
// could reach them, so a campaign tightening its budget could not tighten what the budget was spent against.
public delegate MinimizationResult SmoothDescent(IObjectiveFunction objective, Vector<double> start, Vector<double> lower, Vector<double> upper, OptimizerPolicy policy);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SmoothMinimizer {
    public static readonly SmoothMinimizer BfgsBox = new("bfgs-box", gradient: true,
        static (objective, start, lower, upper, policy) =>
            new BfgsBMinimizer(gradientTolerance: policy.Tolerance, parameterTolerance: policy.StallFloor, functionProgressTolerance: policy.StallFloor, maximumIterations: Math.Max(1, policy.Generations))
                .FindMinimum(objective, lower, upper, start));
    public static readonly SmoothMinimizer BfgsLimited = new("bfgs-limited", gradient: true,
        static (objective, start, _, _, policy) =>
            new LimitedMemoryBfgsMinimizer(gradientTolerance: policy.Tolerance, parameterTolerance: policy.StallFloor, functionProgressTolerance: policy.StallFloor, maximumIterations: Math.Max(1, policy.Generations))
                .FindMinimum(objective, start));
    public static readonly SmoothMinimizer NelderMead = new("nelder-mead", gradient: false,
        static (objective, start, _, _, policy) =>
            NelderMeadSimplex.Minimum(objective, start, convergenceTolerance: policy.Tolerance, maximumIterations: Math.Max(1, policy.Generations)));

    // Gradient-consuming rows build `ObjectiveFunction.Gradient` off the same `AdjointTape` dispatch the descent
    // kernel reads; the derivative-free row builds `ObjectiveFunction.Value` and never touches the tape.
    public bool Gradient { get; }

    [UseDelegateFromConstructor]
    public partial MinimizationResult Minimize(IObjectiveFunction objective, Vector<double> start, Vector<double> lower, Vector<double> upper, OptimizerPolicy policy);

    // The library's own terminal, read onto the branch verdict instead of discarded: `Converged` is the met
    // criterion, an exhausted iteration budget is `Exhausted`, and a manual stop is a search that stopped moving.
    // Collapsing all three into one success is what made a truncated refinement indistinguishable from a settled one.
    public static Convergence Verdict(MinimizationResult result, double residual) =>
        result.ReasonForExit switch {
            ExitCondition.Converged => new Convergence.Converged(residual),
            ExitCondition.ExceedIterations => new Convergence.Exhausted(result.Iterations),
            _ => new Convergence.Stalled(),
        };
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
public sealed partial class LineSearch {
    public static readonly LineSearch Fixed = new("fixed");
    public static readonly LineSearch ArmijoBacktrack = new("armijo-backtrack");
    public static readonly LineSearch TrustRegion = new("trust-region");

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DesignVariable {
    private DesignVariable() { }

    public sealed record Continuous(string Name, double Lower, double Upper) : DesignVariable;
    public sealed record Integer(string Name, long Lower, long Upper) : DesignVariable;

    // `Admissible` carries the ordinal SUBSET a design rule leaves reachable — a product family stocking sizes
    // 2 and 4 but not 3 is one roster, not a range plus a caller-side filter. Empty means every ordinal, so a
    // dense family stays a one-argument construction.
    public sealed record Categorical(string Name, Seq<string> Choices, Seq<int> Admissible) : DesignVariable {
        public Categorical(string name, Seq<string> choices) : this(name, choices, Seq<int>()) { }

        public Seq<int> Codes => Admissible.IsEmpty ? toSeq(Enumerable.Range(0, Choices.Count)) : Admissible;
    }

    public sealed record Density(string Name, long Cells) : DesignVariable;
    public sealed record Linked(string Name, int Source, double Scale, double Offset) : DesignVariable;

    // Symbolic is the `Symbolic/lowering#SYMBOLIC_JACOBIAN` gradient source: a bounded continuous design symbol
    // whose objective partial arrives on the problem's `AdjointTape.Symbolic` tape, never a parallel gradient path.
    public sealed record Symbolic(string Name, double Lower, double Upper) : DesignVariable;

    public string VariableName =>
        Switch(continuous: static c => c.Name, integer: static i => i.Name, categorical: static c => c.Name, density: static d => d.Name, linked: static l => l.Name, symbolic: static s => s.Name);

    // How many COORDINATE SLOTS the variable occupies in a `DesignPoint`. A topology density field is `Cells` slots
    // — one design freedom per element — and every other case is one. The point is the concatenation of these spans
    // under `DesignProblem.Offsets`, so a density field is a first-class variable rather than a single coordinate
    // every kernel then had to expand privately (and the SIMP update was alone in doing so).
    public int Width =>
        Switch(continuous: static _ => 1, integer: static _ => 1, categorical: static _ => 1, density: static d => (int)d.Cells, linked: static _ => 1, symbolic: static _ => 1);

    public bool Free => Switch(continuous: static _ => true, integer: static _ => true, categorical: static _ => true, density: static _ => true, linked: static _ => false, symbolic: static _ => true);

    public double Lower =>
        Switch(continuous: static c => c.Lower, integer: static i => (double)i.Lower, categorical: static _ => 0.0, density: static _ => 0.0, linked: static l => l.Offset, symbolic: static s => s.Lower);

    // `Extent` is the per-slot BOX extent, distinct from `Width`: every slot of a density field spans `[0,1]`, an integer axis
    // spans its range. `Lower + Extent` is the slot's upper bound wherever a kernel needs a box.
    public double Extent =>
        Switch(continuous: static c => c.Upper - c.Lower, integer: static i => (double)(i.Upper - i.Lower), categorical: static c => c.Choices.Count, density: static _ => 1.0, linked: static _ => 0.0, symbolic: static s => s.Upper - s.Lower);

    public double Clamp(double value) =>
        Switch(
            state: value,
            continuous: static (x, c) => Math.Clamp(x, c.Lower, c.Upper),
            integer: static (x, i) => Math.Clamp(Math.Round(x), i.Lower, i.Upper),
            categorical: static (x, c) => Math.Clamp(Math.Round(x), 0, c.Choices.Count - 1),
            density: static (x, _) => Math.Clamp(x, 0.0, 1.0),
            linked: static (x, _) => x,
            symbolic: static (x, s) => Math.Clamp(x, s.Lower, s.Upper));

    public Option<TensorOpFamily> AdjointOperator =>
        Switch(
            continuous: static _ => Some(TensorOpFamily.Gradient),
            integer: static _ => Option<TensorOpFamily>.None,
            categorical: static _ => Option<TensorOpFamily>.None,
            density: static _ => Some(TensorOpFamily.CotangentLaplacian),
            linked: static _ => Option<TensorOpFamily>.None,
            symbolic: static _ => Option<TensorOpFamily>.None);

    // Each variable declares its own admissible SET in the integer coordinate `q = round(1/IntegerStep)` fixes,
    // expressed in the package's own `Domain` algebra rather than a bare `(lower, upper)` pair. A range is
    // `Domain(lo, hi)`; a holed categorical family is `Domain.FromValues(codes)`; a linked axis is the singleton `{0}`
    // because `DesignProblem.Resolve` writes its value from its source and the model must not search it. Every exact-lane
    // lowering reads this ONE member, so a non-contiguous set is expressible and a per-case `NewIntVar(lo, hi)`
    // ladder — which admits every value between its ends — has nowhere to reappear.
    public Domain Admissible(long q) =>
        Switch(
            state: q,
            continuous: static (scale, c) => new Domain((long)Math.Round(c.Lower * scale), (long)Math.Round(c.Upper * scale)),
            integer: static (_, i) => new Domain(i.Lower, i.Upper),
            categorical: static (_, c) => Domain.FromValues([.. c.Codes.Map(static code => (long)code)]),
            density: static (scale, _) => new Domain(0L, scale),
            linked: static (_, _) => Domain.FromValues([0L]),
            symbolic: static (scale, s) => new Domain((long)Math.Round(s.Lower * scale), (long)Math.Round(s.Upper * scale)));

    // Shape admission is the FAMILY's, read once by `DesignProblem.Validate` — the generated total dispatch, so a
    // seventh case breaks the build here rather than falling to a `_` arm that reads every unhandled case as
    // malformed and silently refuses a legal design space.
    public bool Malformed =>
        Switch(
            continuous: static c => !double.IsFinite(c.Lower) || !double.IsFinite(c.Upper) || c.Lower >= c.Upper,
            integer: static i => i.Lower > i.Upper,
            categorical: static c => c.Choices.IsEmpty
                || c.Admissible.Exists(code => code < 0 || code >= c.Choices.Count)
                || c.Admissible.Distinct().Count != c.Admissible.Count,
            density: static d => d.Cells <= 0,
            linked: static l => l.Source < 0 || !double.IsFinite(l.Scale) || !double.IsFinite(l.Offset),
            symbolic: static s => !double.IsFinite(s.Lower) || !double.IsFinite(s.Upper) || s.Lower >= s.Upper);
}

// Closed adjoint carrier the `Symbolic/lowering#SYMBOLIC_JACOBIAN` boundary assigns to this consumer: the
// Geometry case carries the composable DEC tape sequence, the Symbolic case one scalar SymbolicTape — each arm
// keeps its honest arity under the one Adjoint dispatch.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AdjointTape {
    private AdjointTape() { }

    public sealed record Geometry(Seq<GeometryTape> Tapes) : AdjointTape;
    public sealed record Symbolic(SymbolicTape Tape) : AdjointTape;
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

    // Which axis the rule reads; `Always` reads none, so the exact lowering reifies nothing for it.
    public Option<int> Reads =>
        Switch(
            always: static _ => Option<int>.None,
            whenAbove: static r => Some(r.Source),
            whenBelow: static r => Some(r.Source),
            whenChoice: static r => Some(r.Source));

    // Each conditional row spells its SOURCE axis's activating set in the same integer coordinate the variable domains
    // use. `Active` answers the rule for a realized point; `Trigger` answers it for a whole search space, so the
    // exact lane reifies one literal per conditional axis (`lit ⇔ source ∈ Trigger`) instead of optimizing over a range
    // whose inactive states the oracle silently rewrites — the disagreement `DesignProblem.Resolve` would otherwise hide.
    public Option<Domain> Trigger(long q) =>
        Switch(
            state: q,
            always: static (_, _) => Option<Domain>.None,
            whenAbove: static (scale, r) => Some(Domain.GreaterOrEqual((long)Math.Ceiling(r.Threshold * scale))),
            whenBelow: static (scale, r) => Some(Domain.LowerOrEqual((long)Math.Floor(r.Threshold * scale))),
            whenChoice: static (_, r) => Some(Domain.FromValues([(long)r.Choice])));
}

// One linear row: its own tracking NAME, the coefficient vector, and the admissible BAND SET its activity
// occupies. `Bands` is primary because the package's `Domain` carries a union natively — a beam depth admissible
// at 300-450 or 600-750 mm is two bands, and flattening them to the 300-750 hull admits the 500 mm section the
// rule set forbids. `Lower`/`Upper` derive as that hull for the one backend whose constraint face is one interval.
// `[Equatable]`: `ImmutableArray<double>` reference-compares under compiler record equality, so two rows carrying
// identical coefficients read unequal — and the exact lane's own name-uniqueness gate, hint reuse, and model
// comparison all rest on row identity. `Seq` members carry their element-wise equality already.
[Equatable]
public sealed partial record LinearRow(string Name, [property: OrderedEquality] ImmutableArray<double> Coefficients, Seq<(double Lower, double Upper)> Bands) {
    public static LinearRow Of(string name, ImmutableArray<double> coefficients, double lower, double upper) =>
        new(name, coefficients, Seq((lower, upper)));

    public double Lower => Bands.Map(static band => band.Lower).Min(double.PositiveInfinity);

    public double Upper => Bands.Map(static band => band.Upper).Max(double.NegativeInfinity);

    public bool Contiguous => Bands.Count is 1;

    // Ascending flat-interval form `Domain.FromFlatIntervals` takes, scaled into the ONE integer coordinate `q`
    // fixes — so a union crosses the exact seam without a second encoding or a per-band constraint copy.
    public long[] Flattened(long q) =>
        [.. Bands.OrderBy(static band => band.Lower)
            .Bind(band => Seq((long)Math.Round(band.Lower * q), (long)Math.Round(band.Upper * q)))];

    // Bands admit ascending, finite, ordered, and DISJOINT: a touching or overlapping pair is one band written
    // twice, and `Domain.FromFlatIntervals` reads an unsorted or overlapping array as a different set than the
    // author wrote — so the shape gate runs here rather than surfacing as a silently wider feasible region.
    public bool Invalid(int width) =>
        string.IsNullOrWhiteSpace(Name) || Coefficients.Length != width || Bands.IsEmpty
        || Bands.Exists(static band => !double.IsFinite(band.Lower) || !double.IsFinite(band.Upper) || band.Lower > band.Upper)
        || toSeq(Bands.OrderBy(static band => band.Lower))
            .Fold((Prior: Option<double>.None, Broken: false), static (state, band) =>
                (Some(band.Upper), state.Broken || state.Prior.Exists(upper => band.Lower <= upper)))
            .Broken;
}

[Equatable]
public sealed partial record LinearModel([property: OrderedEquality] ImmutableArray<double> Objective, Seq<LinearRow> Rows);

// Structural VALUE identity is generated, bit-exact, and total over the three coordinate blocks:
// `ImmutableArray<double>` reference-compares under compiler record equality, so the front-membership set, the
// genome memo, and every `Seq<DesignPoint>` containment read would silently miss re-materialized rows — the
// generated comparer is the one equality, and a tolerance never enters (a screening grid deliberately places
// distinct points close together).
[Equatable]
public readonly partial record struct DesignPoint(
    [property: OrderedEquality] ImmutableArray<double> Coordinates,
    [property: OrderedEquality] ImmutableArray<double> Objectives,
    [property: OrderedEquality] ImmutableArray<double> Constraints) {
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

    // One indexed read serves the oracle's OUTPUT vector — objectives then constraints, the same concatenation the
    // contract defines — so a surrogate fits every output the contract carries rather than the first objective and a
    // count check no fit could ever satisfy.
    public int Outputs => (Objectives.IsDefaultOrEmpty ? 0 : Objectives.Length) + (Constraints.IsDefaultOrEmpty ? 0 : Constraints.Length);

    public double Response(int index) =>
        index < (Objectives.IsDefaultOrEmpty ? 0 : Objectives.Length)
            ? Objectives[index]
            : Constraints[index - Objectives.Length];
}

public sealed record DesignProblem(
    Seq<DesignVariable> Variables,
    Seq<ActivationRule> Activation,
    Seq<ObjectiveSense> Objectives,
    int Constraints,
    ConstraintHandling Handling,
    Option<MeshAdjointSnapshot> DesignMesh,
    AdjointTape AdjointTape,
    Option<LinearModel> Exact) {
    public static DesignProblem Of(Seq<DesignVariable> variables, Seq<ObjectiveSense> objectives, int constraints, ConstraintHandling handling, Option<MeshAdjointSnapshot> designMesh = default, Option<SymbolicTape> symbolic = default, Option<LinearModel> exact = default) =>
        new(variables, variables.Map(static _ => (ActivationRule)new ActivationRule.Always()), objectives, constraints, handling, designMesh, Lower(variables, designMesh, symbolic), exact);

    public static DesignProblem Conditional(Seq<DesignVariable> variables, Seq<ActivationRule> activation, Seq<ObjectiveSense> objectives, int constraints, ConstraintHandling handling, Option<MeshAdjointSnapshot> designMesh = default, Option<SymbolicTape> symbolic = default, Option<LinearModel> exact = default) =>
        new(variables, activation, objectives, constraints, handling, designMesh, Lower(variables, designMesh, symbolic), exact);

    // Symbolic tapes win the carrier when supplied (the SymbolicJacobian.Lower product for Symbolic variables);
    // otherwise the free variables' DEC operators lower onto the Geometry case.
    static AdjointTape Lower(Seq<DesignVariable> variables, Option<MeshAdjointSnapshot> designMesh, Option<SymbolicTape> symbolic) =>
        symbolic.Match(
            Some: static tape => (AdjointTape)new AdjointTape.Symbolic(tape),
            None: () => new AdjointTape.Geometry(designMesh.Case is MeshAdjointSnapshot mesh
                ? variables.Filter(static v => v.Free)
                    .Bind(v => v.AdjointOperator.Match(Some: op => Seq(new GeometryTape(op, mesh)), None: () => Seq<GeometryTape>()))
                : Seq<GeometryTape>()));

    // This slot carries the routing model the `routing` row lowers, absent for every other row. It rides `init` beside the linear
    // model rather than a ninth positional slot, because a routing campaign and a coefficient campaign declare
    // disjoint halves of the same problem and neither should have to name the other's absence.
    public Option<RoutingProblem> Routing { get; init; } = None;

    public ImmutableArray<double> Senses => [.. Objectives.Map(static o => o.Sign)];

    // One OFFSET TABLE builds once and every kernel indexes through it: `Offsets[axis]` is where a variable's span
    // starts and `Offsets[^1]` is the point's dimension. A `DesignPoint` is the concatenation of the per-variable
    // spans, so a `Density` field of 10⁴ cells is 10⁴ coordinates rather than one the SIMP kernel privately expanded
    // while every other kernel searched a single scalar for the whole field.
    public ImmutableArray<int> Offsets { get; } = [.. Variables.Fold(Seq(0), static (acc, v) => acc.Add(acc.Last + v.Width))];

    public int Dimension => Offsets[^1];

    // Slot → owning variable, resolved by the same table rather than a per-kernel scan.
    public int VariableAt(int slot) {
        int axis = Offsets.AsSpan().BinarySearch(slot);
        return axis >= 0 ? axis : ~axis - 1;
    }

    public ImmutableArray<double> LowerBounds => [.. Slots(static v => v.Lower)];
    public ImmutableArray<double> UpperBounds => [.. Slots(static v => v.Lower + v.Extent)];
    public ImmutableArray<double> Centre => [.. Slots(static v => v.Clamp(v.Lower + 0.5 * v.Extent))];

    // These per-slot projections feed every kernel start, box, and unit map — one fold over the offset table instead
    // of the `Variables.Map((v, axis) => …)` shape that silently assumed one slot per variable.
    Seq<double> Slots(Func<DesignVariable, double> read) =>
        Variables.Bind(v => toSeq(Enumerable.Repeat(read(v), v.Width)));

    public ImmutableArray<double> Clamp(ReadOnlySpan<double> raw) {
        double[] clamped = new double[Dimension];
        for (int slot = 0; slot < clamped.Length; slot++) {
            clamped[slot] = Variables[VariableAt(slot)].Clamp(slot < raw.Length ? raw[slot] : 0.0);
        }
        return [.. clamped];
    }

    // Unit hypercube → design coordinates, the one map every low-discrepancy start composes.
    public ImmutableArray<double> FromUnit(ReadOnlySpan<double> unit) {
        double[] coordinates = new double[Dimension];
        for (int slot = 0; slot < coordinates.Length; slot++) {
            DesignVariable variable = Variables[VariableAt(slot)];
            coordinates[slot] = variable.Clamp(variable.Lower + (slot < unit.Length ? unit[slot] : 0.0) * variable.Extent);
        }
        return [.. coordinates];
    }

    // ONE integer coordinate serves the whole exact lane: variable domains, row bands, and hint values all scale
    // by `q`, so the model a CP-SAT solve searches and the physical `LinearModel` an author wrote are the same
    // program in two units, never two programs.
    public static long Scale(double integerStep) => Math.Max(1L, (long)Math.Round(1.0 / integerStep));

    // Each axis publishes its admissible set as the exact lane searches it: its own domain, WIDENED by the inactive
    // value the activation fold writes. `Resolve` zeroes a deactivated axis, so an axis whose own range excludes zero is
    // reachable at zero and nowhere between — exactly the union a contiguous range cannot spell, and exactly the
    // set whose absence lets an exact solve return an assignment the oracle then rewrites underneath it.
    public Domain Admissible(int slot, long q) {
        int axis = VariableAt(slot);
        return Activation[axis] is ActivationRule.Always
            ? Variables[axis].Admissible(q)
            : Variables[axis].Admissible(q).UnionWith(Domain.FromValues([0L]));
    }

    // A gradient row needs a tape that can ANSWER: an empty Geometry tape set and a degenerate symbolic tape both
    // return a zero direction, which every descent reads as a converged optimum at the start point. The trait gate
    // reads this column, so a gradient row over an underivable design space refuses before the first oracle call.
    public bool Differentiable =>
        AdjointTape.Switch(
            geometry: static tape => !tape.Tapes.IsEmpty,
            symbolic: static tape => !tape.Tape.IsDegenerate);

    // Five INDEPENDENT structural laws, so they accumulate. The prior chain computed all five flags and then
    // published one opaque `<optimizer-invalid-design-problem>`: it paid for full-defect evaluation and reported
    // none of it, so a caller repairing a design space learned about exactly one defect per round trip.
    public Fin<Unit> Validate() =>
        Seq(
            Refusal.Unless(!Variables.IsEmpty, ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(Variables.Count, 1L))),
            Refusal.Unless(!Variables.Exists(static variable => variable.Malformed), ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())),
            Refusal.Unless(!Variables.Exists((variable, axis) => variable is DesignVariable.Linked link && (link.Source >= axis || link.Source >= Variables.Count)), ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Reachable, new ContractEvidence.None())),
            Refusal.Unless(Activation.Count == Variables.Count, ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(Activation.Count, Variables.Count))),
            Refusal.Unless(!Activation.Exists(rule => rule.Switch(
                always: static _ => false,
                whenAbove: r => r.Source < 0 || r.Source >= Variables.Count || !double.IsFinite(r.Threshold),
                whenBelow: r => r.Source < 0 || r.Source >= Variables.Count || !double.IsFinite(r.Threshold),
                whenChoice: r => r.Source < 0 || r.Source >= Variables.Count || r.Choice < 0)), ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())),
            Refusal.Unless(!Objectives.IsEmpty, ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(Objectives.Count, 1L))),
            Refusal.Unless(Constraints >= 0, ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Value(Constraints))),
            // Exact models state in SLOT coordinates, the same system the solver variables inhabit, so a density
            // field contributes one coefficient per cell and never one coefficient standing in for the whole field.
            Refusal.Unless(!Exact.Exists(model => model.Objective.Length != Dimension
                || model.Rows.Exists(row => row.Invalid(Dimension))
                || model.Rows.Map(static row => row.Name).ToFrozenSet(StringComparer.Ordinal).Count != model.Rows.Count), ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
        .Traverse(static claim => claim).As().Map(static _ => unit).ToFin();

    // Linking and activation are per-VARIABLE decisions applied across the variable's whole span: a linked field
    // takes its source's leading slot and a deactivated field zeroes every one of its cells, so a conditional
    // topology axis is on or off as a unit rather than cell by cell.
    public ImmutableArray<double> Resolve(ImmutableArray<double> raw) {
        double[] resolved = new double[Dimension];
        for (int slot = 0; slot < resolved.Length; slot++) { resolved[slot] = slot < raw.Length ? raw[slot] : 0.0; }
        for (int axis = 0; axis < Variables.Count; axis++) {
            if (Variables[axis] is not DesignVariable.Linked link) { continue; }
            double source = resolved[Offsets[link.Source]];
            for (int slot = Offsets[axis]; slot < Offsets[axis + 1]; slot++) { resolved[slot] = link.Scale * source + link.Offset; }
        }
        // Activation reads the LEADING slot of each source axis — the same coordinate the exact lane reifies its
        // trigger literal over, so the two lanes agree about which state activates.
        double[] leading = [.. Enumerable.Range(0, Variables.Count).Select(axis => resolved[Offsets[axis]])];
        for (int axis = 0; axis < Variables.Count && axis < Activation.Count; axis++) {
            if (Activation[axis].Active(leading)) { continue; }
            for (int slot = Offsets[axis]; slot < Offsets[axis + 1]; slot++) { resolved[slot] = 0.0; }
        }
        return [.. resolved];
    }
}

public sealed record OptimizerPolicy(
    OptimizerKind Kind,
    LineSearch LineSearch,
    AcquisitionFunction Acquisition,
    int Population,
    int Generations,
    double CrossoverRate,
    double MutationRate,
    double SimpPenalty,
    double VolumeFraction,
    double PenaltyWeight,
    double TrustRadius,
    // `StepLength` owns the descent STEP as its own policy: a mutation rate is a genetic-operator probability, and
    // reading one as a gradient step length couples two unrelated axes — widening the mutation widens the descent.
    double StepLength,
    // The two thresholds every bounded budget on this lane halts against. `Tolerance` is the residual inside which
    // a search reports `Converged`; `StallFloor` is the step below which it stopped moving and reports `Stalled`.
    // They are COLUMNS because the smooth-local rows used to carry them as constructor literals inside three
    // minimizer delegates, where no campaign could tighten what its own budget was being spent against and no
    // receipt could say which threshold the run actually met.
    double Tolerance,
    double StallFloor,
    // The Monte-Carlo sample count the ≥3-objective hypervolume estimator spends. Every other budget on this page
    // is a policy column; a literal here made the indicator's own precision the one quantity a campaign could not
    // trade against its wall time.
    int HypervolumeSamples,
    // `Reference` is the hypervolume reference point, ABSENT where the campaign declares none — the fold then derives it from the
    // front's own worst objective per axis and the result marks it derived, so a hypervolume compared across two
    // runs is never silently measured against two different boxes.
    Option<ImmutableArray<double>> Reference,
    Option<Surrogate> Surrogate,
    double SurrogateErrorBound,
    double IntegerStep,
    double SolveSeconds,
    int Parallelism,
    OptimizerKind MultiStartInner,
    int Restarts,
    int Seed,
    RoutingPolicy Routing,
    Seq<RandomVariable> Uncertainties,
    double ReliabilityTarget) {
    public static readonly OptimizerPolicy CanonicalNsga = new(
        OptimizerKind.Nsga2, LineSearch.Fixed, AcquisitionFunction.ExpectedImprovement, Population: 100, Generations: 250, CrossoverRate: 0.9, MutationRate: 0.1,
        SimpPenalty: 3.0, VolumeFraction: 0.4, PenaltyWeight: 1e6, TrustRadius: 1.0, StepLength: 1e-2,
        Tolerance: 1e-8, StallFloor: 1e-10, HypervolumeSamples: 4096, Reference: None, Surrogate: None, SurrogateErrorBound: 0.05,
        IntegerStep: 1.0, SolveSeconds: 30.0, Parallelism: 1, MultiStartInner: OptimizerKind.CmaEs, Restarts: 8, Seed: 0x5EED,
        Routing: RoutingPolicy.Canonical, Uncertainties: Seq<RandomVariable>(), ReliabilityTarget: 3.0);
    public static readonly OptimizerPolicy CanonicalAdjoint = CanonicalNsga with { Kind = OptimizerKind.GradientAdjoint, LineSearch = LineSearch.TrustRegion };
    public static readonly OptimizerPolicy CanonicalBayesian = CanonicalNsga with { Kind = OptimizerKind.BayesianGp, Population = 32, Generations = 64 };
    public static readonly OptimizerPolicy CanonicalCma = CanonicalNsga with { Kind = OptimizerKind.CmaEs, Population = 16, Generations = 200, TrustRadius = 0.3 };
    public static readonly OptimizerPolicy CanonicalPso = CanonicalNsga with { Kind = OptimizerKind.Pso, Population = 40, Generations = 200 };
    public static readonly OptimizerPolicy CanonicalAnneal = CanonicalNsga with { Kind = OptimizerKind.SimulatedAnnealing, Population = 8, Generations = 500, MutationRate = 0.2 };
    public static readonly OptimizerPolicy CanonicalCpSat = CanonicalNsga with { Kind = OptimizerKind.CpSat, Generations = 1, IntegerStep = 0.01 };
    public static readonly OptimizerPolicy CanonicalMilp = CanonicalNsga with { Kind = OptimizerKind.Milp, Generations = 1 };
    public static readonly OptimizerPolicy CanonicalMultiStart = CanonicalNsga with { Kind = OptimizerKind.MultiStartGlobal };
    public static readonly OptimizerPolicy CanonicalRobust = CanonicalNsga with { Kind = OptimizerKind.RobustMinimax };

    // Twelve of the sixteen conditions were ONE constraint spelled twelve times, so the constraint states once and
    // the columns it ranges over are DATA — a new bounded scalar joins its roster rather than growing the chain.
    // Every clause ACCUMULATES: the prior `||` chain evaluated all sixteen and then published one opaque
    // `<optimizer-invalid-policy>`, so a caller tuning a policy learned about exactly one defect per round trip and
    // the fault text could not even name which column broke.
    Seq<(string Name, double Value)> PositiveColumns => Seq(
        (nameof(SimpPenalty), SimpPenalty), (nameof(PenaltyWeight), PenaltyWeight), (nameof(TrustRadius), TrustRadius),
        (nameof(StepLength), StepLength), (nameof(IntegerStep), IntegerStep), (nameof(SolveSeconds), SolveSeconds),
        (nameof(Tolerance), Tolerance), (nameof(StallFloor), StallFloor));

    Seq<(string Name, double Value)> UnitColumns => Seq(
        (nameof(CrossoverRate), CrossoverRate), (nameof(MutationRate), MutationRate));

    Seq<(string Name, int Value)> CountColumns => Seq(
        (nameof(Population), Population), (nameof(Generations), Generations), (nameof(Restarts), Restarts),
        (nameof(Parallelism), Parallelism), (nameof(HypervolumeSamples), HypervolumeSamples));

    public Fin<Unit> Validate() =>
        (PositiveColumns.Map(static row => Refusal.Unless(double.IsFinite(row.Value) && row.Value > 0.0, ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(row.Value))))
            + UnitColumns.Map(static row => Refusal.Unless(double.IsFinite(row.Value) && row.Value is >= 0.0 and <= 1.0, ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Interval(row.Value, 0.0, 1.0))))
            + CountColumns.Map(static row => Refusal.Unless(row.Value > 0, ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(row.Value))))
            + Seq(
                // The volume fraction is a HALF-OPEN unit: a topology run at zero fraction has no material to place.
                Refusal.Unless(double.IsFinite(VolumeFraction) && VolumeFraction is > 0.0 and <= 1.0, ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Interval(VolumeFraction, 0.0, 1.0))),
                Refusal.Unless(double.IsFinite(SurrogateErrorBound) && SurrogateErrorBound >= 0.0, ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Value(SurrogateErrorBound))),
                Refusal.Unless(double.IsFinite(ReliabilityTarget), ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(ReliabilityTarget))),
                Refusal.Unless(!Reference.Exists(static point => point.IsDefaultOrEmpty), ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Input)),
                Refusal.Unless(!Reference.Exists(static point => !point.All(double.IsFinite)), ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(Reference.Map(static point => point.Length).IfNone(0)))),
                // A stall floor at or above the convergence tolerance calls every converged run stalled.
                Refusal.Unless(StallFloor < Tolerance, ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Interval(StallFloor, 0.0, Tolerance)))))
        .Traverse(static claim => claim).As().Map(static _ => unit).ToFin();
}

// `[Equatable]`: the front crosses to Persistence content-keyed and re-materializes on the far side, so its
// `ImmutableArray<double>` sense vector must compare by VALUE — under compiler record equality it reference-compares
// and two identical fronts read unequal at every index membership test. `Seq<DesignPoint>` already carries its
// element-wise equality and each element its own generated comparer, so only the array member needs the attribute.
[Equatable]
public sealed partial record ParetoFront(Seq<DesignPoint> Points, [property: OrderedEquality] ImmutableArray<double> Senses) {
    public ParetoFront Insert(DesignPoint candidate) =>
        Points.Exists(p => p.Dominates(candidate, Senses.AsSpan()))
            ? this
            : this with { Points = Points.Filter(p => !candidate.Dominates(p, Senses.AsSpan())).Add(candidate) };

    // Estimator draws key the SAME `(Lane, Seed)` pair every other stochastic step on the page keys, so a
    // hypervolume is reproducible from the run's own policy and never from a literal only this method knew.
    public double Hypervolume(ReadOnlySpan<double> reference, long lane, int seed, int samples) {
        if (Points.IsEmpty) { return 0.0; }
        int objectives = Points[0].Objectives.Length;
        return objectives == 2 ? Hypervolume2D(reference) : HypervolumeEstimate(reference, objectives, lane, seed, samples);
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

    double HypervolumeEstimate(ReadOnlySpan<double> reference, int objectives, long lane, int seed, int samples) {
        double[] refCopy = reference.ToArray();
        double[] low = new double[objectives];
        for (int axis = 0; axis < objectives; axis++) { low[axis] = Points.Min(p => p.Objectives[axis] * Senses[axis]); }
        double boxVolume = 1.0;
        for (int axis = 0; axis < objectives; axis++) { boxVolume *= Math.Max(0.0, refCopy[axis] - low[axis]); }
        if (boxVolume <= 0.0) { return 0.0; }
        // Estimator draw rides the kernel's ONE deterministic source under the RUN's own seed and row lane, so the
        // same front measured in two processes reports the same indicator and no literal seed lives on this page.
        Random rng = Deterministic.Source(seed: seed, lanes: [lane, objectives]);
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

}

// One bounded budget's outcome: the state it advanced to, the `Solver/contract#SOLVE_REQUEST` `Convergence` verdict
// it ended on, and the generations it actually SPENT. A spend seeded `Exhausted(0)` has settled nothing, every step
// either settles it or re-marks it exhausted at the count so far, and the halt predicate reads that verdict — so the
// fold stops at the FIRST settled state and a budget that runs out leaves `Exhausted(spent)` standing as the answer.
// The prior shape had no discriminant at all: every kernel ran its full policy budget whatever the residual and then
// reported that policy value as `Generations`, which published "ran to plan" as if it were "converged".
public readonly record struct SearchSpend<TState>(TState State, Convergence Verdict, int Spent) {
    public static SearchSpend<TState> Seed(TState state) => new(state, new Convergence.Exhausted(Budget: 0), Spent: 0);

    public bool Settled => Verdict is not Convergence.Exhausted;
}

public sealed record KernelRun(ParetoFront Front, int Generations, Convergence Verdict, double TrustRadius, Seq<double> Violation, Option<ExactEvidence> Exact = default, Option<RoutingResult> Routing = default) {
    // The projection every kernel ends on, so no kernel body re-derives the pair: the spend carries both the count
    // and the verdict, and a caller reading `Generations` beside `Verdict` can tell a settled search from a
    // truncated one without re-deriving either from the policy it was handed.
    public static KernelRun Of<TState>(SearchSpend<TState> spend, ParetoFront front, double trustRadius, Seq<double> violation) =>
        new(front, spend.Spent, spend.Verdict, trustRadius, violation);
}

public sealed record OptimizationResult(
    OptimizerKind Kind,
    ParetoFront Front,
    int Generations,
    // The verdict the search ENDED on, beside the count it spent reaching it. `ComputeReceipt.Optimization` declares
    // six audit slots and no termination column, so the discriminant rides this carrier the same way `ExactEvidence`
    // does rather than claiming a slot the receipt owner does not declare.
    Convergence Verdict,
    int Evaluations,
    int SurrogateHits,
    double Hypervolume,
    // This pair carries the reference box the indicator measured against, and whether the fold derived it. A hypervolume without
    // its box is not comparable across runs, and the `Runtime/receipts#RECEIPT_UNION` `Optimization` case declares
    // six audit slots — so the box rides this carrier, the same way `ExactEvidence` does.
    ImmutableArray<double> Reference,
    bool ReferenceDerived,
    Seq<double> ViolationHistory,
    double TrustRadius,
    Option<ExactEvidence> Exact,
    // Routing assignments are GRAPH answers, not design points, so they ride their own slot beside the exact
    // evidence rather than being flattened onto a front whose objective vector they do not have.
    Option<RoutingResult> Routing,
    Instant At);

public sealed record GpModel(Cholesky<double> Factor, Vector<double> Alpha, Matrix<double> X, double LengthScale, double SignalVar, double NoiseVar, double LogMarginal) {
    public (double Mean, double Variance) Posterior(ReadOnlySpan<double> query) {
        Vector<double> k = Vector<double>.Build.Dense(X.RowCount, row => Kernel(X.Row(row), query, LengthScale, SignalVar));
        double mean = k.DotProduct(Alpha);
        double variance = SignalVar + NoiseVar - k.DotProduct(Factor.Solve(k));
        return (mean, Math.Max(0.0, variance));
    }

    // The squared-exponential profile over the catalogued pair reduce: `Distance` is the Euclidean norm of the
    // difference in one vectorized pass, where the hand loop paid a bounds check per axis on the hottest read in
    // the posterior — this runs once per training row per query.
    public static double Kernel(Vector<double> a, ReadOnlySpan<double> b, double lengthScale, double signalVar) {
        double distance = TensorPrimitives.Distance<double>(a.AsArray(), b);
        return signalVar * Math.Exp(-0.5 * distance * distance / (lengthScale * lengthScale));
    }
}

public sealed record RbfModel(RbfFit Fit, double LengthScale, double ResidualRms) {
    public (double Mean, double Bound) Posterior(ReadOnlySpan<double> query) {
        Vector<double> q = Vector<double>.Build.Dense(Fit.Centres.ColumnCount, i => i < query.Length ? query[i] : 0.0);
        double mean = Fit.Evaluate(Matrix<double>.Build.DenseOfRowVectors(q))[0, 0];
        // Extrapolation distance is the nearest CENTRE: one vectorized pair reduce per centre, minimum over them.
        double[] reach = [.. Enumerable.Range(0, Fit.Centres.RowCount).Select(centre => TensorPrimitives.Distance<double>(Fit.Centres.Row(centre).AsArray(), q.AsArray()))];
        double nearest = reach.Length == 0 ? double.MaxValue : TensorPrimitives.Min<double>(reach);
        return (mean, ResidualRms * (1.0 + nearest / Math.Max(1e-9, LengthScale)));
    }
}

public sealed record OutputModel(
    ReadOnlyMemory<double> Weights,
    double Intercept,
    ReadOnlyMemory<double> Centroid,
    double SpreadScale,
    double ResidualRms,
    Option<GpModel> Gp,
    Option<RbfModel> Rbf) {
    // One per-output posterior answers here: the fitted GP or RBF where one was fitted, the linear trend otherwise. One body
    // serves every output, so the multi-output surrogate is a Seq of these rather than a parallel per-output family.
    public (double Mean, double Bound) Posterior(ReadOnlySpan<double> query) =>
        (Gp, Rbf) switch {
            ({ IsSome: true, Case: GpModel gp }, _) => gp.Posterior(query) switch { (double mean, double variance) => (mean, Math.Sqrt(variance)) },
            (_, { IsSome: true, Case: RbfModel rbf }) => rbf.Posterior(query),
            _ => LinearPosterior(query),
        };

    // Both reads are catalogued pair reduces over the SHARED prefix the query and the fitted vector agree on — a
    // query shorter than the fit is a caller error the gate above catches, never a silently truncated prediction.
    (double Mean, double Bound) LinearPosterior(ReadOnlySpan<double> query) {
        int width = Math.Min(Weights.Length, query.Length);
        double mean = Intercept + TensorPrimitives.Dot<double>(Weights.Span[..width], query[..width]);
        int reach = Math.Min(Centroid.Length, query.Length);
        double leverage = TensorPrimitives.Distance<double>(Centroid.Span[..reach], query[..reach]);
        return (mean, ResidualRms * (1.0 + leverage / Math.Max(1e-9, SpreadScale)));
    }
}

// MULTI-OUTPUT by construction: one `OutputModel` per contract output — objectives then constraints — so `Predict`
// answers the SAME vector shape the full oracle answers and the gated surrogate's cardinality check is satisfiable.
// Single-output surrogates never clear that check — every surrogate hit is then structurally unreachable and the
// metered hit count stays zero.
public sealed record Surrogate(
    SurrogateKind Kind,
    Seq<OutputModel> Outputs,
    Option<NeuralFieldModel> Field,
    Option<(InferenceSession Session, RunOptions Options, CancelScope Scope)> Lane) {
    // Vector BOUNDs take the widest per-output bound: a surrogate is admissible only while every component
    // it answers is inside the policy, and taking the mean admits a vector whose constraint row is far outside.
    public Fin<(Seq<double> Values, double Bound)> Predict(DesignPoint point) =>
        (Field, Lane) switch {
            ({ IsSome: true, Case: NeuralFieldModel field }, { IsSome: true, Case: (InferenceSession session, RunOptions options, CancelScope scope) }) =>
                field.Predict(session, options, scope, point.Coordinates.AsSpan()),
            _ => Outputs.IsEmpty
                ? Fin.Fail<(Seq<double>, double)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
                : Fin.Succ(Outputs.Fold((Values: Seq<double>(), Bound: 0.0), (acc, model) =>
                    model.Posterior(point.Coordinates.AsSpan()) is (double mean, double bound)
                        ? (acc.Values.Add(mean), Math.Max(acc.Bound, bound))
                        : acc)),
        };

    // Fits ONE model per contract output over the shared history. The neural-field row REFUSES: its weights are
    // trained outside this runtime and arrive by content key through the `Model/identity#MODEL_IDENTITY` admission,
    // so `OfField` is its only mint and a delegation to the linear trend answers a fit nobody asked for under the
    // neural-field name.
    public static Fin<Surrogate> Fit(SurrogateKind kind, Seq<DesignPoint> history, int outputs) =>
        kind == SurrogateKind.NeuralField
            ? Fin.Fail<Surrogate>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.NeuralField)))
            : toSeq(Enumerable.Range(0, outputs))
                .TraverseM(output => kind.Switch(
                    state: (History: history, Output: output),
                    linearTrend: static s => Fin.Succ(FitLinear(s.History, s.Output)),
                    gaussianProcess: static s => Fin.Succ(FitGaussianProcess(s.History, s.Output)),
                    radialBasis: static s => FitRadialBasis(s.History, s.Output),
                    neuralField: static _ => Fin.Fail<OutputModel>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.NeuralField)))))
                .As()
                .Map(models => new Surrogate(kind, models, None, None));

    public static Surrogate OfField(NeuralFieldModel field, InferenceSession session, RunOptions options, CancelScope scope) =>
        new(SurrogateKind.NeuralField, Seq<OutputModel>(), Some(field), Some((session, options, scope)));

    static Fin<OutputModel> FitRadialBasis(Seq<DesignPoint> history, int output) {
        if (history.Count < 2) { return Fin.Succ(FitLinear(history, output)); }
        int n = history.Count, dim = history[0].Coordinates.Length;
        // Median pairwise distance IS the radius the kernel profile takes; the prior inverse-shape spelling was a
        // second parameterization of the same length, and the kernel row owns the profile's own convention.
        double lengthScale = MedianPairwise(history);
        Matrix<double> centres = Matrix<double>.Build.Dense(n, dim, (r, c) => history[r].Coordinates[c]);
        Matrix<double> response = Matrix<double>.Build.Dense(n, 1, (r, _) => history[r].Response(output));
        return Scatter.Fit(centres, centres, response, KernelKind.Gaussian, Math.Max(1e-9, lengthScale), TolerancePolicy.Derive(centres, response.Column(0)))
            .Map(fit => {
                Matrix<double> fitted = fit.Evaluate(centres);
                double rms = Math.Sqrt(toSeq(Enumerable.Range(0, n)).Sum(i => {
                    double e = history[i].Response(output) - fitted[i, 0];
                    return e * e;
                }) / n);
                return new OutputModel(ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, lengthScale, rms, None, Some(new RbfModel(fit, lengthScale, rms)));
            });
    }

    static OutputModel FitGaussianProcess(Seq<DesignPoint> history, int output) {
        if (history.Count < 2) { return FitLinear(history, output); }
        int n = history.Count, dim = history[0].Coordinates.Length;
        double lengthScale = MedianPairwise(history), signalVar = ResponseVariance(history, output), noiseVar = 1e-6 * Math.Max(1e-9, signalVar);
        Matrix<double> x = Matrix<double>.Build.Dense(n, dim, (r, c) => history[r].Coordinates[c]);
        Vector<double> y = Vector<double>.Build.Dense(n, r => history[r].Response(output));
        Matrix<double> kMatrix = Matrix<double>.Build.Dense(n, n, (r, c) =>
            GpModel.Kernel(x.Row(r), x.Row(c).AsArray(), lengthScale, signalVar) + (r == c ? noiseVar : 0.0));
        Cholesky<double> chol = kMatrix.Cholesky();
        Vector<double> alpha = chol.Solve(y);
        double logMarginal = -0.5 * y.DotProduct(alpha) - 0.5 * chol.DeterminantLn - 0.5 * n * Math.Log(2.0 * Math.PI);
        return new OutputModel(ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, lengthScale, Math.Sqrt(signalVar),
            Some(new GpModel(chol, alpha, x, lengthScale, signalVar, noiseVar, logMarginal)), None);
    }

    // A genuine MULTIVARIATE least-squares trend. The per-axis marginal regression this replaces divided each axis's
    // covariance by that axis's OWN variance and ignored every cross-axis covariance — so under correlated design
    // axes, which every coupled AEC design space has, the weights were systematically biased and the residual the
    // surrogate gate admits on under-reported its own error. `Fit.MultiDim` solves the whole system, the intercept
    // rides the leading coefficient, and the bound is the residual STANDARD error at the fit's own degrees of
    // freedom: an exactly-determined or underdetermined history has none, so it reports a bound no gate can clear
    // rather than the zero a root-mean-square over n observations would publish as a perfect surrogate.
    static OutputModel FitLinear(Seq<DesignPoint> history, int output) {
        if (history.IsEmpty) { return new(ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, 1.0, double.MaxValue, None, None); }
        int dim = history[0].Coordinates.Length, n = history.Count;
        double[][] design = [.. history.Map(static point => point.Coordinates.ToArray())];
        double[] observed = [.. history.Map(point => point.Response(output))];
        double[] fitted = Fit.MultiDim(design, observed, intercept: true, DirectRegressionMethod.QR);
        double intercept = fitted[0];
        double[] weights = fitted[1..];
        double[] modelled = [.. design.Select(row => intercept + TensorPrimitives.Dot<double>(weights, row))];
        double[] centroid = [.. Enumerable.Range(0, dim).Select(axis => history.Average(point => point.Coordinates[axis]))];
        double[] deviation = [.. Enumerable.Range(0, dim).Select(axis => TensorPrimitives.StdDev<double>([.. history.Map(point => point.Coordinates[axis])]))];
        double spread = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(deviation));
        double bound = n > dim + 1 ? GoodnessOfFit.StandardError(modelled, observed, dim + 1) : double.MaxValue;
        return new(weights.AsMemory(), intercept, centroid.AsMemory(), Math.Max(1e-9, spread), bound, None, None);
    }

    // The kernel profile's radius is the MEDIAN pairwise distance and `Statistics.Median` owns that order statistic:
    // the hand fold took `distances[count / 2]`, which on an even count is the upper-middle element and not the
    // median — a length scale biased long on every even-sized history, silently over-smoothing the posterior.
    static double MedianPairwise(Seq<DesignPoint> history) {
        double[] distances = [.. toSeq(Enumerable.Range(0, history.Count)).Bind(i =>
            toSeq(Enumerable.Range(i + 1, Math.Max(0, history.Count - i - 1)))
                .Map(j => TensorPrimitives.Distance<double>(history[i].Coordinates.AsSpan(), history[j].Coordinates.AsSpan())))];
        return distances.Length == 0 ? 1.0 : Math.Max(1e-6, Statistics.Median(distances));
    }

    static double ResponseVariance(Seq<DesignPoint> history, int output) {
        double[] responses = history.Map(p => p.Response(output)).ToArray();
        double sigma = TensorPrimitives.StdDev<double>(responses);
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

// Runtime capabilities orthogonal to the design space: the cooperative stop every long native search registers
// against, and the observation cell an admitted intent minted. They travel as ONE argument because both describe
// HOW a search runs rather than WHICH search it is, so a third capability is one field and no kernel signature
// grows a token tail. `Progress` is absent when the admitting intent requested no observation.
public readonly record struct SearchContext(CancelScope Scope, Option<ProgressCell> Progress);

public static class Optimizer {
    public static Fin<OptimizationResult> Optimize(DesignProblem problem, OptimizerPolicy policy, CpuBudget budget, Func<DesignPoint, Fin<Seq<double>>> evaluate, SearchContext search, IClock clock) =>
        from _problem in problem.Validate()
        let governed = policy with { Parallelism = budget.Workers }
        from _policy in governed.Validate()
        from _traits in governed.Kind.Admits(problem, governed)
        from result in Run(problem, governed, evaluate, search, clock)
        select result;

    // THE bounded-budget fold. Every search kernel on this lane spends its generations here, so the halt is one
    // mechanism rather than twelve `Range(0, N).Fold(Fin.Succ(seed), (acc, _) => acc.Bind(step))` chains with no
    // halt and no exhaustion fault between them. `foldWhileM` runs the step on the `Fin` rail and STOPS at the
    // first settled state, so a converged search pays no further oracle call — the shape it replaces paid for the
    // whole policy budget on every run and then reported that budget as the search it performed.
    static Fin<SearchSpend<TState>> Spend<TState>(TState seed, int budget, Func<TState, int, Fin<(TState State, Option<Convergence> Settled)>> step) =>
        Prelude.foldWhileM(
            (int generation) => (SearchSpend<TState> spend) =>
                step(spend.State, generation).Map(next => new SearchSpend<TState>(
                    next.State,
                    next.Settled.IfNone(() => new Convergence.Exhausted(generation + 1)),
                    generation + 1)),
            static pair => !pair.State.Settled,
            SearchSpend<TState>.Seed(seed),
            toSeq(Enumerable.Range(0, Math.Max(1, budget)))).As();

    // The two settle tests every gradient-shaped kernel shares, stated once: a measure inside the tolerance is a
    // met criterion and a step below the stall floor is a search that stopped moving. Absence means keep spending —
    // the fold, not the kernel, owns what running out of budget means.
    static Option<Convergence> Settle(OptimizerPolicy policy, double measure, double motion) =>
        measure <= policy.Tolerance ? Some((Convergence)new Convergence.Converged(measure))
            : motion <= policy.StallFloor ? Some((Convergence)new Convergence.Stalled())
            : None;

    // The ANALYTIC oracle: one `CompiledExpr` per contract output, bound to the design space by SYMBOL NAME and
    // answered in the objectives-then-constraints order the contract already fixes. Binding by POSITION is the
    // deleted form — an expression authored over `{depth, width}` against a space declaring `{width, depth}`
    // evaluates every formula with the wrong variable in each slot and returns a finite number no reader can tell
    // from a correct one. A symbol the space does not declare refuses BY NAME before the first evaluation, so the
    // binding proves once at mint rather than per point.
    public static Fin<Func<DesignPoint, Fin<Seq<double>>>> Analytic(DesignProblem problem, Seq<CompiledExpr> objectives, Seq<CompiledExpr> constraints) {
        HashMap<string, int> slots = problem.Variables.Fold(
            (Held: HashMap<string, int>(), Axis: 0),
            (acc, variable) => (acc.Held.AddOrUpdate(variable.VariableName, problem.Offsets[acc.Axis]), acc.Axis + 1)).Held;
        Seq<CompiledExpr> rows = objectives + constraints;
        return rows.Count != problem.Objectives.Count + problem.Constraints
            ? Fin.Fail<Func<DesignPoint, Fin<Seq<double>>>>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(rows.Count, problem.Objectives.Count + problem.Constraints))))
            : rows.Traverse(row => row.SymbolOrder
                    .Traverse(symbol => slots.Find(symbol.Value)
                        .ToValidation<Error>(new ComputeFault.SymbolUndefined($"<analytic-unbound:{symbol.Value}>")))
                    .Map(bound => (Row: row, Bound: bound)))
                .As().ToFin()
                // Binding ACCUMULATES (a caller repairing a formula set wants every unbound symbol at once);
                // evaluation ABORTS (one non-finite output makes the whole vector meaningless to the search).
                .Map(bound => (Func<DesignPoint, Fin<Seq<double>>>)(point =>
                    bound.TraverseM(row => row.Row.Invoke([.. row.Bound.Map(slot => point.Coordinates[slot])])).As()));
    }

    static Fin<KernelRun> Invoke(OptimizerKind kind, DesignProblem problem, OptimizerPolicy policy, SearchContext search, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        kind.Switch(
            state: (Problem: problem, Policy: policy, Search: search, Oracle: oracle, Seed: seed),
            nsga2: static s => GeneticEngine.Evolve(s.Problem, s.Policy, s.Oracle, s.Seed),
            bayesianGp: static s => AcquireBayesian(s.Problem, s.Policy, s.Oracle, s.Seed),
            gradientAdjoint: static s => DescendAdjoint(s.Problem, s.Policy, s.Oracle, s.Seed),
            topologySimp: static s => OptimalityCriteria(s.Problem, s.Policy, s.Oracle, s.Seed),
            simulatedAnnealing: static s => Anneal(s.Problem, s.Policy, s.Oracle, s.Seed),
            cmaEs: static s => EvolveCma(s.Problem, s.Policy, s.Oracle, s.Seed),
            pso: static s => EvolvePso(s.Problem, s.Policy, s.Oracle, s.Seed),
            cpSat: static s => ExactLane.SolveCpSat(s.Problem, s.Policy, s.Search, s.Oracle, s.Seed),
            milp: static s => ExactLane.SolveMilp(s.Problem, s.Policy, s.Search, s.Oracle, s.Seed),
            multiStartGlobal: static s => MultiStart(s.Problem, s.Policy, s.Search, s.Oracle, s.Seed),
            robustMinimax: static s => RobustMinimax(s.Problem, s.Policy, s.Oracle, s.Seed),
            // Smooth local refinement over the MathNet minimizer family: one `IObjectiveFunction` built from the
            // same oracle, the row's own minimizer entry, and `ReasonForExit` lifted onto the fault rail. Bounds
            // ride the typed variables, so the box-constrained row never re-derives them at the call site.
            bfgsBox: static s => Smooth(s.Problem, s.Policy, s.Oracle, s.Seed, SmoothMinimizer.BfgsBox),
            bfgsLimited: static s => Smooth(s.Problem, s.Policy, s.Oracle, s.Seed, SmoothMinimizer.BfgsLimited),
            nelderMead: static s => Smooth(s.Problem, s.Policy, s.Oracle, s.Seed, SmoothMinimizer.NelderMead),
            // Vehicle routing lowers the typed `RoutingProblem` onto the OR-Tools ConstraintSolver rail; the design
            // space carries no continuous coordinate, so the row refuses a problem with no routing model attached.
            routing: static s => RoutingSearch.Solve(s.Problem, s.Policy, s.Search, s.Seed));

    // ONE smooth-local fold serves every MathNet minimizer row: the penalized oracle is the value face, the row's
    // own `Gradient` column decides whether the `AdjointTape` supplies the derivative, typed variables supply the
    // box, and the exit condition partitions terminals the way the numeric route already rules — a budget or
    // progress stall keeps its iterate as a legitimate result, and only an invalid-value or never-ran exit faults.
    static Fin<KernelRun> Smooth(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed, SmoothMinimizer row) {
        ImmutableArray<double> start = seed.Points.IsEmpty ? problem.Centre : seed.Points[0].Coordinates;
        Vector<double> lower = Vector<double>.Build.DenseOfArray([.. problem.LowerBounds]);
        Vector<double> upper = Vector<double>.Build.DenseOfArray([.. problem.UpperBounds]);
        double[] flat = new double[problem.Constraints];
        double Value(Vector<double> theta) =>
            Probe(problem, oracle, [.. theta]).Map(point => Fitness(problem, policy, point, flat)).IfFail(double.MaxValue);
        IObjectiveFunction objective = row.Gradient
            ? ObjectiveFunction.Gradient(Value, theta => Vector<double>.Build.DenseOfArray(
                [.. Adjoint(problem, [.. theta]).IfFail([.. theta.Select(static _ => 0.0)])]))
            : ObjectiveFunction.Value(Value);
        return Probe(problem, oracle, start).Bind(baseline =>
            Op.Of(name: "optimizer.smooth-minimize").Catch(() => Fin.Succ(row.Minimize(objective, Vector<double>.Build.DenseOfArray([.. start]), lower, upper, policy)))
                .Bind(result => result.ReasonForExit is ExitCondition.InvalidValues or ExitCondition.None
                    ? Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Converged, new ContractEvidence.Status((int)result.ReasonForExit))))
                    : Probe(problem, oracle, problem.Clamp(result.MinimizingPoint.AsArray()))
                        // The library's own terminal reaches the verdict instead of dying here: this row is the ONE
                        // arm on the page that always knew why it stopped and used to discard it, collapsing a met
                        // criterion, a spent iteration budget, and a manual stop into one indistinguishable success.
                        // The residual the verdict carries is the returned iterate's own feasibility violation.
                        .Map(point => {
                            ParetoFront front = seed.Insert(baseline).Insert(point);
                            return new KernelRun(front, result.Iterations, SmoothMinimizer.Verdict(result, point.Violation), policy.TrustRadius, Seq(Worst(front)));
                        })));
    }

    static Fin<OptimizationResult> Run(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, SearchContext search, IClock clock) {
        Atom<(int Evals, int Hits)> meter = Atom((Evals: 0, Hits: 0));
        Func<DesignPoint, Fin<Seq<double>>> oracle = Gated(problem, policy, evaluate, meter);
        return Invoke(policy.Kind, problem, policy, search, oracle, new ParetoFront(Seq<DesignPoint>(), problem.Senses))
            .Map(run => {
                (ImmutableArray<double> reference, bool derived) = Reference(policy, run.Front);
                return new OptimizationResult(policy.Kind, run.Front, run.Generations, run.Verdict, meter.Value.Evals, meter.Value.Hits,
                    run.Front.Hypervolume(reference.AsSpan(), policy.Kind.Lane, policy.Seed, policy.HypervolumeSamples), reference, derived,
                    run.Violation, run.TrustRadius, run.Exact, run.Routing, clock.GetCurrentInstant());
            });
    }

    // `ReferenceDerived` is WRITTEN here and this is its one producer: the receipt owner declares the column with a
    // cross-run comparability law, and a projection that set `Scope` alone published `false` on every emitted
    // receipt as a measured fact while `Reference` genuinely derived the box on most runs.
    public static ComputeReceipt.Optimization Receipt(OptimizationResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Kind.Key, result.Generations, result.Evaluations, result.SurrogateHits, result.Front.Points.Count, result.Hypervolume) {
            ReferenceDerived = result.ReferenceDerived,
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
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
            return values.Count != m + problem.Constraints
                ? Fin.Fail<DesignPoint>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(
                    ShapeRequirement.Arity,
                    new ShapeEvidence.Count(values.Count, m + problem.Constraints))))
            : !values.ForAll(double.IsFinite)
                ? Fin.Fail<DesignPoint>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(
                    ComputeSubject.Value,
                    new ScalarEvidence.Sequence(values.Count))))
                : Fin.Succ(new DesignPoint(coords, [.. values.Take(m)], [.. values.Skip(m)]));
        });
    }

    static double Fitness(DesignProblem problem, OptimizerPolicy policy, DesignPoint point, ReadOnlySpan<double> multipliers) =>
        problem.Handling.Penalize(
            point.Objectives.IsDefaultOrEmpty ? 0.0 : point.Objectives[0] * problem.Senses[0],
            point.Constraints.AsSpan(), policy.PenaltyWeight, multipliers);

    internal static double Worst(ParetoFront front) => front.Points.IsEmpty ? 0.0 : front.Points.Max(static p => p.Violation);

    // A declared reference point wins; otherwise the fold derives one from the front's own worst objective per axis
    // and says so. The distinction is load-bearing: a hypervolume against a derived box moves with the front, so two
    // runs of the same campaign are comparable only when both declared the same reference — and the result carries
    // the box it used so a reader never has to assume.
    static (ImmutableArray<double> Reference, bool Derived) Reference(OptimizerPolicy policy, ParetoFront front) =>
        policy.Reference.Case is ImmutableArray<double> declared && !declared.IsDefaultOrEmpty
            ? (declared, false)
            : front.Points.IsEmpty
                // An empty front still has the problem's OBJECTIVE ARITY, so the degenerate box carries one unit per
                // objective: the prior one-element box let every `Hypervolume` read index past its own reference on
                // any multi-objective campaign that produced no point.
                ? ([.. Enumerable.Repeat(1.0, Math.Max(1, front.Senses.Length))], true)
                : ([.. toSeq(Enumerable.Range(0, front.Points[0].Objectives.Length))
                    .Map(axis => front.Points.Max(point => point.Objectives[axis] * front.Senses[axis]))
                    .Map(static worst => worst + 0.1 * Math.Abs(worst) + 0.1)], true);

    // The acquisition settles when a whole generation of the best candidates stops improving the incumbent: the
    // improvement IS the search's own residual, so a campaign whose GP has converged stops paying full oracle calls
    // for candidates it already knows the answer to instead of spending every remaining generation on them.
    static Fin<KernelRun> AcquireBayesian(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        Spend((History: seed.Points, Front: seed, Violation: Seq<double>(), Best: double.MaxValue), policy.Generations, (state, gen) =>
                // The acquisition ranks the FIRST objective alone, so the surrogate fits one output — the third
                // argument is an output COUNT, not an output index.
                Surrogate.Fit(SurrogateKind.GaussianProcess, state.History, outputs: 1).Bind(gp =>
                    GeneticEngine.Candidates(problem, policy.Population, policy.Seed + gen).Bind(candidates => {
                        double best = state.History.IsEmpty ? 0.0 : state.History.Min(p => p.Objectives.IsDefaultOrEmpty ? double.MaxValue : p.Objectives[0] * problem.Senses[0]);
                        return candidates.Traverse(raw => gp.Predict(new DesignPoint(problem.Resolve(raw), [], []))
                                .Map(prediction => (Raw: raw, Score: policy.Acquisition.Score(prediction.Values.Head.IfNone(best), prediction.Bound, best))))
                            .Bind(scored => toSeq(scored.OrderByDescending(static row => row.Score).Take(Math.Max(1, policy.Population / 8)))
                                .TraverseM(row => Probe(problem, oracle, row.Raw)).As()
                                .Map(probed => probed.Fold((state.History, state.Front), static (carry, point) => (carry.Item1.Add(point), carry.Item2.Insert(point)))))
                            .Map(carry => {
                                double incumbent = carry.Item1.IsEmpty ? double.MaxValue : carry.Item1.Min(p => p.Objectives.IsDefaultOrEmpty ? double.MaxValue : p.Objectives[0] * problem.Senses[0]);
                                double gain = double.IsFinite(state.Best) ? Math.Abs(state.Best - incumbent) : double.MaxValue;
                                return ((carry.Item1, carry.Item2, state.Violation.Add(Worst(carry.Item2)), incumbent),
                                    gain <= policy.StallFloor ? Some((Convergence)new Convergence.Stalled()) : Option<Convergence>.None);
                            });
                    })))
            .Map(spend => KernelRun.Of(spend, spend.State.Front, policy.TrustRadius, spend.State.Violation));

    static Fin<KernelRun> DescendAdjoint(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        ImmutableArray<double> start = seed.Points.IsEmpty ? problem.LowerBounds : seed.Points[0].Coordinates;
        // The descent's settle test is its OWN two measures: a gradient norm inside the tolerance is a stationary
        // point and a trust radius collapsed below the stall floor is a search that stopped moving. Spending every
        // remaining generation past either is the fall-through this fold exists to delete.
        return Spend((Origin: start, Radius: policy.TrustRadius, Front: seed, Violation: Seq<double>()), policy.Generations, (state, _) =>
                Adjoint(problem, state.Origin).Bind(gradient =>
                    Probe(problem, oracle, state.Origin).Bind(baseline =>
                        Stepped(problem, state.Origin, gradient, state.Radius).Bind(stepped => {
                            double objectiveAtOrigin = baseline.Objectives.IsDefaultOrEmpty ? 0.0 : baseline.Objectives[0];
                            double predicted = TensorPrimitives.SumOfSquares<double>([.. gradient]) * state.Radius;
                            return Probe(problem, oracle, stepped).Bind(probe => {
                                double actual = objectiveAtOrigin - (probe.Objectives.IsDefaultOrEmpty ? objectiveAtOrigin : probe.Objectives[0]);
                                // The base step is `OptimizerPolicy.StepLength`, the descent's OWN column: reading a
                                // genetic mutation rate here coupled the step to an operator probability, so widening
                                // the mutation silently lengthened every gradient step.
                                (double step, double radius) = policy.LineSearch.Next(state.Radius, actual, predicted, policy.StepLength);
                                return Stepped(problem, state.Origin, gradient, step).Map(next => {
                                    ParetoFront front = state.Front.Insert(baseline).Insert(probe);
                                    return ((next, radius, front, state.Violation.Add(Worst(front))),
                                        Settle(policy, Math.Sqrt(TensorPrimitives.SumOfSquares<double>([.. gradient])), radius));
                                });
                            });
                        }))))
            .Map(spend => KernelRun.Of(spend, spend.State.Front, spend.State.Radius, spend.State.Violation));
    }

    // A gradient SHORTER than the design point is not a partial gradient — it is a tape that answered a different
    // problem, and padding the tail with zeros descends the leading coordinates while freezing the rest into a
    // silently reduced search. The length mismatch is a typed refusal.
    static Fin<ImmutableArray<double>> Stepped(DesignProblem problem, ImmutableArray<double> origin, ImmutableArray<double> gradient, double scale) =>
        gradient.Length != problem.Dimension
            ? Fin.Fail<ImmutableArray<double>>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Dimensions, new ShapeEvidence.Count(gradient.Length, problem.Dimension))))
            : Fin.Succ(problem.Clamp([.. Enumerable.Range(0, problem.Dimension).Select(slot => origin[slot] - scale * gradient[slot])]));

    // Objective-tied adjoint: the cotangent seed carries the selected objective's sense (maximize rows seed −1
    // so every kernel descends the returned direction), the Geometry arm chains the DEC tapes, and the Symbolic
    // arm re-points its tape at the CURRENT origin before the reverse sweep — two distinct design states produce
    // distinct symbolic gradients, never one frozen direction.
    static Fin<ImmutableArray<double>> Adjoint(DesignProblem problem, ImmutableArray<double> origin) {
        float sense = problem.Objectives.Head.Map(static o => o == ObjectiveSense.Maximize ? -1f : 1f).IfNone(1f);
        return problem.AdjointTape.Switch(
                state: (Origin: origin, Sense: sense),
                geometry: static (s, tape) => SensitivityLaw.Chain(tape.Tapes, [.. Enumerable.Repeat(s.Sense, s.Origin.Length)]),
                symbolic: static (s, tape) => SymbolicJacobian.Backward(tape.Tape with { DesignPoint = s.Origin }, new[] { s.Sense }))
            .Map(static gradient => (ImmutableArray<double>)[.. gradient.Span.ToArray().Select(static g => (double)g)]);
    }

    // SIMP settles on its own design CHANGE: the largest per-cell density move inside the tolerance is the standard
    // topology-optimization stopping criterion, and a field that stopped moving below the stall floor is stalled.
    static Fin<KernelRun> OptimalityCriteria(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) =>
        Spend((Density: seed.Points.IsEmpty ? (ImmutableArray<double>)[.. Enumerable.Repeat(policy.VolumeFraction, problem.Dimension)] : seed.Points[0].Coordinates,
                Front: seed, Violation: Seq<double>()), policy.Generations,
                (state, _) =>
                    Adjoint(problem, state.Density).Bind(sensitivity => {
                        ImmutableArray<double> updated = OcUpdate(problem, policy, state.Density, sensitivity);
                        double change = updated.Length != state.Density.Length
                            ? double.MaxValue
                            : TensorPrimitives.MaxMagnitude<double>([.. Enumerable.Range(0, updated.Length).Select(slot => updated[slot] - state.Density[slot])]);
                        return Probe(problem, oracle, updated).Map(point => {
                            ParetoFront front = state.Front.Insert(point);
                            return ((updated, front, state.Violation.Add(Worst(front))), Settle(policy, change, change));
                        });
                    }))
            .Map(spend => KernelRun.Of(spend, spend.State.Front, policy.TrustRadius, spend.State.Violation));

    // The optimality-criteria update iterates the density SPAN, one element per coordinate slot, because a topology
    // field IS `Cells` design freedoms — the prior `Variables[e]` index read the slot as a variable ordinal and
    // walked off the roster the moment a field carried more than one cell.
    static ImmutableArray<double> OcUpdate(DesignProblem problem, OptimizerPolicy policy, ImmutableArray<double> density, ImmutableArray<double> sensitivity) {
        double lower = 1e-9, upper = 1e9;
        const double move = 0.2, eta = 0.5;
        double[] updated = density.ToArray();
        for (int bisect = 0; bisect < 50 && (upper - lower) / Math.Max(1e-12, lower + upper) > 1e-4; bisect++) {
            double lagrange = 0.5 * (lower + upper);
            for (int slot = 0; slot < updated.Length; slot++) {
                double x = density[slot];
                double raw = slot < sensitivity.Length ? sensitivity[slot] : 0.0;
                double dc = policy.SimpPenalty * Math.Pow(Math.Max(1e-9, x), policy.SimpPenalty - 1.0) * raw;
                double scaled = x * Math.Pow(Math.Max(1e-12, -dc / lagrange), eta);
                updated[slot] = problem.Variables[problem.VariableAt(slot)].Clamp(Math.Clamp(scaled, Math.Max(0.0, x - move), Math.Min(1.0, x + move)));
            }
            if (TensorPrimitives.Average<double>(updated) > policy.VolumeFraction) { lower = lagrange; } else { upper = lagrange; }
        }
        return [.. updated];
    }

    static Fin<KernelRun> MultiStart(DesignProblem problem, OptimizerPolicy policy, SearchContext search, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        OptimizerKind inner = policy.MultiStartInner == OptimizerKind.MultiStartGlobal ? OptimizerKind.CmaEs : policy.MultiStartInner;
        return LowDiscrepancy.Sobol(dimensions: problem.Dimension, seed: policy.Seed, Scramble.DigitalShift).Bind(generator =>
            toSeq(Enumerable.Range(0, Math.Max(1, policy.Restarts)))
                .Fold(Fin.Succ((Gen: generator, Front: seed, Violation: Seq<double>())),
                    (acc, _) => acc.Bind(state => {
                        (LowDiscrepancy next, double[] point) = state.Gen.Draw();
                        ImmutableArray<double> start = problem.FromUnit(point);
                        return Probe(problem, oracle, start).Bind(seeded =>
                            Invoke(inner, problem, policy with { Kind = inner }, search, oracle, state.Front.Insert(seeded))
                                .Map(run => (next, run.Front, state.Violation + run.Violation)));
                    }))
                // Restarts are a BASIN budget, not a convergence budget: every restart is a fresh basin the wrap has
                // no settle test over, so this row spends its whole roster and says `Exhausted` — the truth, and the
                // reason the inner rows carry their own verdicts the caller can still read off each run.
                .Map(state => new KernelRun(state.Front, policy.Restarts * Math.Max(1, policy.Generations),
                    new Convergence.Exhausted(Math.Max(1, policy.Restarts)), policy.TrustRadius, state.Violation)));
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
        // The search space is the FREE SLOTS, not the free variables: a density field contributes one covariance
        // dimension per cell, which is what makes a CMA run over a topology problem search the field at all.
        int[] free = [.. Enumerable.Range(0, problem.Dimension).Where(slot => problem.Variables[problem.VariableAt(slot)].Free)];
        int n = free.Length;
        if (n == 0) { return Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))); }
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
        // Every stochastic kernel on this page draws from the kernel's ONE deterministic source, lane-keyed by the
        // kernel it feeds so two rows under one policy seed never replay each other's stream; a bare
        // `new Random(seed)` is the fork the owner exists to delete.
        Random rng = Deterministic.Source(seed: policy.Seed, lanes: [OptimizerKind.CmaEs.Lane, n]);
        CmaState initial = new(
            Vector<double>.Build.Dense(n, i => problem.Centre[free[i]]),
            policy.TrustRadius > 1e-9 ? policy.TrustRadius : 0.3,
            Matrix<double>.Build.DenseIdentity(n),
            Vector<double>.Build.Dense(n), Vector<double>.Build.Dense(n),
            new double[problem.Constraints], seed, Seq<double>());
        // CMA-ES settles on its OWN step size: `Sigma` collapsing below the stall floor is the algorithm's native
        // termination criterion (the distribution has contracted onto a point), and a run that never contracts
        // spends its budget and says so.
        return Spend(initial, policy.Generations, (state, gen) =>
                CmaStep(problem, policy, oracle, free, state, gen, rng, n, lambda, mu, weights, muEff, cSigma, dSigma, cc, c1, cMu, chiN)
                    // A contracted distribution is a STALL, never a met residual: CMA-ES has no residual to meet, so
                    // the convergence arm stays unreachable here and the verdict never overclaims.
                    .Map(next => (next, Settle(policy, double.MaxValue, next.Sigma))))
            .Map(spend => KernelRun.Of(spend, spend.State.Front, spend.State.Sigma, spend.State.Violation));
    }

    static Fin<CmaState> CmaStep(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, int[] free, CmaState state, int gen, Random rng,
        int n, int lambda, int mu, double[] weights, double muEff, double cSigma, double dSigma, double cc, double c1, double cMu, double chiN) {
        Matrix<double> symmetric = (state.Covariance + state.Covariance.Transpose()) * 0.5;
        Evd<double> evd = symmetric.Evd(Symmetricity.Symmetric);
        Matrix<double> b = evd.EigenVectors;
        double[] d = [.. Enumerable.Range(0, n).Select(i => Math.Sqrt(Math.Max(1e-20, evd.EigenValues[i].Real)))];
        return toSeq(Enumerable.Range(0, lambda))
            .Fold(Fin.Succ((Front: state.Front, Offspring: Seq<(Vector<double> Y, double Fitness, ImmutableArray<double> Constraints)>())),
                (acc, _) => acc.Bind(carry => {
                    Vector<double> y = b * Vector<double>.Build.Dense(n, i => d[i] * Normal.Sample(rng, 0.0, 1.0));
                    ImmutableArray<double> raw = Embed(problem, free, state.Mean, y, state.Sigma);
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

    // Free-slot vector back into the full point: a non-free slot reads zero and `Resolve` writes its linked value.
    static ImmutableArray<double> Embed(DesignProblem problem, int[] free, Vector<double> mean, Vector<double> y, double sigma) {
        double[] full = new double[problem.Dimension];
        for (int g = 0; g < free.Length; g++) {
            full[free[g]] = problem.Variables[problem.VariableAt(free[g])].Clamp(mean[g] + sigma * y[g]);
        }
        return [.. full];
    }

    // --- [PSO] ---------------------------------------------------------------------------------------------

    sealed record SwarmState(ImmutableArray<double>[] Position, double[][] Velocity, ImmutableArray<double>[] Best, double[] BestFitness, ImmutableArray<double> Global, double GlobalFitness, double[] Multipliers, ParetoFront Front, Seq<double> Violation);

    static Fin<KernelRun> EvolvePso(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, ParetoFront seed) {
        int particles = Math.Max(2, policy.Population);
        if (!problem.Variables.Exists(static v => v.Free)) { return Fin.Fail<KernelRun>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))); }
        const double chi = 0.7298, phi = 2.05;
        Random rng = Deterministic.Source(seed: policy.Seed, lanes: [OptimizerKind.Pso.Lane, particles]);
        return InitSwarm(problem, policy, oracle, particles, seed)
            // The swarm settles when its global best stops improving: the gain across one whole generation inside
            // the stall floor is a swarm that has collapsed onto its incumbent, and paying every remaining
            // generation of full oracle calls past that point is the fall-through this fold deletes.
            .Bind(init => Spend(init, policy.Generations, (state, _) =>
                    PsoStep(problem, policy, oracle, chi, phi, rng, state)
                        .Map(next => (next, Settle(policy, double.MaxValue, Math.Abs(state.GlobalFitness - next.GlobalFitness)))))
                .Map(spend => KernelRun.Of(spend, spend.State.Front, policy.TrustRadius, spend.State.Violation)));
    }

    static Fin<SwarmState> InitSwarm(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, int particles, ParetoFront seed) =>
        LowDiscrepancy.Sobol(dimensions: problem.Dimension, seed: policy.Seed, Scramble.DigitalShift).Bind(generator =>
            toSeq(Enumerable.Range(0, particles))
                .Fold(Fin.Succ((Gen: generator, Items: Seq<(ImmutableArray<double> Pos, double Fit, DesignPoint Point)>())),
                    (acc, _) => acc.Bind(carry => {
                        (LowDiscrepancy next, double[] u) = carry.Gen.Draw();
                        ImmutableArray<double> pos = problem.FromUnit(u);
                        return Probe(problem, oracle, pos).Map(point => (next, carry.Items.Add((pos, Fitness(problem, policy, point, new double[problem.Constraints]), point))));
                    }))
                .Map(carry => {
                    (ImmutableArray<double> Pos, double Fit, DesignPoint Point)[] items = carry.Items.ToArray();
                    int gbest = 0;
                    for (int i = 1; i < items.Length; i++) { if (items[i].Fit < items[gbest].Fit) { gbest = i; } }
                    return new SwarmState(
                        [.. items.Select(static it => it.Pos)],
                        [.. items.Select(_ => new double[problem.Dimension])],
                        [.. items.Select(static it => it.Pos)],
                        [.. items.Select(static it => it.Fit)],
                        items[gbest].Pos, items[gbest].Fit,
                        new double[problem.Constraints], items.Aggregate(seed, static (f, it) => f.Insert(it.Point)), Seq<double>());
                }));

    static Fin<SwarmState> PsoStep(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, double chi, double phi, Random rng, SwarmState state) =>
        toSeq(Enumerable.Range(0, state.Position.Length))
            .Fold(Fin.Succ(state), (acc, p) => acc.Bind(s => {
                double[] velocity = s.Velocity[p];
                double[] position = new double[problem.Dimension];
                for (int slot = 0; slot < problem.Dimension; slot++) {
                    velocity[slot] = chi * (velocity[slot] + phi * rng.NextDouble() * (s.Best[p][slot] - s.Position[p][slot]) + phi * rng.NextDouble() * (s.Global[slot] - s.Position[p][slot]));
                    position[slot] = problem.Variables[problem.VariableAt(slot)].Clamp(s.Position[p][slot] + velocity[slot]);
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
        Random rng = Deterministic.Source(seed: policy.Seed, lanes: [OptimizerKind.SimulatedAnnealing.Lane, chains]);
        return LowDiscrepancy.Sobol(dimensions: problem.Dimension, seed: policy.Seed, Scramble.DigitalShift).Bind(generator =>
            toSeq(Enumerable.Range(0, chains))
                .Fold(Fin.Succ((Gen: generator, Front: seed, Multipliers: new double[problem.Constraints], Violation: Seq<double>())),
                    (acc, _) => acc.Bind(state => {
                        (LowDiscrepancy next, double[] u) = state.Gen.Draw();
                        ImmutableArray<double> start = problem.FromUnit(u);
                        return Probe(problem, oracle, start).Bind(startPoint =>
                            AnnealChain(problem, policy, oracle, rng, cooling, steps, state.Front.Insert(startPoint), startPoint, Fitness(problem, policy, startPoint, state.Multipliers), state.Multipliers)
                                .Map(chain => (next, chain.Front, chain.Multipliers, state.Violation.Add(Worst(chain.Front)))));
                    }))
                // Annealing runs its COOLING SCHEDULE whole — the schedule is the algorithm, and stopping early
                // would abandon the exploration the temperature ladder exists to buy — so every chain spends its
                // full step budget and the row reports the honest `Exhausted` rather than a manufactured success.
                .Map(state => new KernelRun(state.Front, chains * steps, new Convergence.Exhausted(steps), policy.TrustRadius, state.Violation)));
    }

    static Fin<(ParetoFront Front, double[] Multipliers)> AnnealChain(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> oracle, Random rng, double cooling, int steps, ParetoFront front, DesignPoint current, double currentFit, double[] multipliers) =>
        toSeq(Enumerable.Range(0, steps))
            .Fold(Fin.Succ((Front: front, Current: current, Fit: currentFit, Temp: 1.0, Mult: multipliers)), (acc, _) => acc.Bind(s => {
                ImmutableArray<double> proposal = problem.Clamp([.. Enumerable.Range(0, problem.Dimension).Select(slot =>
                    s.Current.Coordinates[slot] + policy.MutationRate * problem.Variables[problem.VariableAt(slot)].Extent * (rng.NextDouble() * 2.0 - 1.0))]);
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
        // Provider assignment picks WHICH generator; `ResetSeed` pins WHAT it draws. Assigning alone leaves the
        // package's `DateTime.Now.Millisecond` global feeding every worker thread, so `nsga2` — alone among the
        // stochastic rows, all of which thread `OptimizerPolicy.Seed` — would publish a `ParetoFront` content-
        // addressed onto the Persistence vector index keying a run nobody can re-derive from its own receipt.
        // `ResetSeed` replaces the package's `ThreadLocal<FastRandom>` and every worker re-seeds from this ONE
        // value, so a seeded parallel evaluation is reproducible and per-thread stream independence is not on
        // offer here; the pair binds inside this capsule because the provider is process-global and a concurrent
        // second search re-pinning it at composition would cut this run's stream mid-generation.
        RandomizationProvider.Current = new FastRandomRandomization();
        FastRandomRandomization.ResetSeed(policy.Seed);
        IChromosome adam = Genome(problem);
        // `TplPopulation` inherits the `Population` constructor whole and overrides `CreateInitialGeneration`
        // alone, so the parallel carrier costs one genome-minting fan at start — the package's own knobless
        // `Parallel.For` over `CreateNew` — while every later generation, and every fitness evaluation on it,
        // runs under the operator strategy and executor the policy already seals from `CpuBudget.Workers`.
        TplPopulation population = new(Math.Max(2, policy.Population), Math.Max(2, policy.Population), adam) { GenerationStrategy = new PerformanceGenerationStrategy(10) };
        NsgaFitness fitness = new(problem, policy, evaluate, seed);
        GeneticAlgorithm algorithm = new(population, fitness, Selection(policy), new TwoPointCrossover(), new UniformMutation(true)) {
            CrossoverProbability = (float)policy.CrossoverRate,
            MutationProbability = (float)policy.MutationRate,
            Reinsertion = new ElitistReinsertion(),
            Termination = new OrTermination(new GenerationNumberTermination(Math.Max(1, policy.Generations)), new FitnessStagnationTermination(Math.Max(2, policy.Generations / 4))),
            TaskExecutor = new ParallelTaskExecutor { MaxThreads = policy.Parallelism, Timeout = TimeSpan.FromSeconds(policy.SolveSeconds) },
            OperatorsStrategy = new TplOperatorsStrategy(),
        };
        // The fitness reads the engine's own generation counter to know when its snapshot rotates; the engine takes
        // the fitness at construction, so the counter binds after and before `Start`.
        fitness.Bind(() => algorithm.GenerationsNumber);
        algorithm.Start();
        return fitness.Fault.Match(
            Some: error => Fin.Fail<KernelRun>(error),
            // The engine's OWN termination decides: an `OrTermination` that fired on fitness stagnation is a search
            // that stopped moving, and one that fired on the generation ceiling spent its budget. Reading the
            // generation count alone reported both as the same run.
            None: () => Fin.Succ(new KernelRun(fitness.Archive, algorithm.GenerationsNumber,
                algorithm.GenerationsNumber < Math.Max(1, policy.Generations)
                    ? new Convergence.Stalled()
                    : new Convergence.Exhausted(Math.Max(1, policy.Generations)),
                policy.TrustRadius, Seq(Optimizer.Worst(fitness.Archive)))));
    }

    public static Fin<Seq<ImmutableArray<double>>> Candidates(DesignProblem problem, int count, int seed) =>
        LowDiscrepancy.Sobol(dimensions: problem.Dimension, seed: seed, Scramble.DigitalShift)
            .Map(generator => toSeq(Enumerable.Range(0, Math.Max(1, count)))
                .Fold((Gen: generator, Pool: Seq<ImmutableArray<double>>()), (acc, _) => {
                    (LowDiscrepancy next, double[] u) = acc.Gen.Draw();
                    return (next, acc.Pool.Add(problem.FromUnit(u)));
                }).Pool);

    // One gene per FREE SLOT: a density field is `Cells` genes, so crossover and mutation act on the field itself
    // rather than on one scalar the decode then had to broadcast.
    static IChromosome Genome(DesignProblem problem) {
        int[] free = FreeSlots(problem);
        return new FloatingPointChromosome(
            [.. free.Select(slot => problem.LowerBounds[slot])],
            [.. free.Select(slot => problem.UpperBounds[slot])],
            [.. free.Select(static _ => 32)],
            [.. free.Select(slot => problem.Variables[problem.VariableAt(slot)] is DesignVariable.Continuous or DesignVariable.Density ? 6 : 0)]);
    }

    internal static int[] FreeSlots(DesignProblem problem) =>
        [.. Enumerable.Range(0, problem.Dimension).Where(slot => problem.Variables[problem.VariableAt(slot)].Free)];

    static ImmutableArray<double> DecodeRaw(DesignProblem problem, IChromosome chromosome) {
        double[] genes = ((FloatingPointChromosome)chromosome).ToFloatingPoints();
        int[] free = FreeSlots(problem);
        double[] full = new double[problem.Dimension];
        for (int g = 0; g < free.Length && g < genes.Length; g++) {
            full[free[g]] = problem.Variables[problem.VariableAt(free[g])].Clamp(genes[g]);
        }
        return [.. full];
    }

    static ISelection Selection(OptimizerPolicy policy) =>
        policy.Kind == OptimizerKind.Nsga2 ? new TournamentSelection(2, allowWinnerCompeteNextTournament: false) : new EliteSelection();
}

// --- [NSGA_II] -----------------------------------------------------------------------------------------

// The custom multi-objective `IFitness` the `api-geneticsharp` rail names for this row: GeneticSharp owns the
// population and operator machinery, and the crowded-comparison operator is this page's.
//
// SNAPSHOT fitness is the whole point. Ranking a genome against a LIVE archive makes its fitness depend on how many
// peers happened to be scored before it — which under a `ParallelTaskExecutor` is thread interleaving — so one seed
// produced a different search on every run and the `ParetoFront` content-addressed onto the Persistence vector index
// keyed a campaign nobody could re-derive. The snapshot freezes for a whole generation: every genome of generation
// `g` ranks against the SAME frozen population, evaluation order stops mattering, and reproducibility follows from
// the seeded `RandomizationProvider` and this snapshot together.
//
// Deb 2002 verbatim: a fast non-dominated sort partitions the snapshot into ranked fronts and crowding distance
// spreads each front; a genome scores `−rank + normalized crowding`, so a lower rank always outranks a higher one
// and a SPARSE member outranks a crowded peer at equal rank — the crowded-comparison order the tournament needs.
public sealed class NsgaFitness : IFitness {
    private readonly DesignProblem problem;
    private readonly OptimizerPolicy policy;
    private readonly Func<DesignPoint, Fin<Seq<double>>> evaluate;
    private readonly Atom<Ranked> snapshot;
    private readonly Atom<ParetoFront> archive;
    private readonly Atom<double[]> multipliers;
    private readonly Atom<Option<Error>> fault = Atom<Option<Error>>(None);
    private readonly Atom<HashMap<DesignPoint, DesignPoint>> cache = Atom(HashMap<DesignPoint, DesignPoint>());
    private Func<int> generation = static () => 0;

    // The frozen generation: its members, their fronts, and their crowding distances. `Number` is the generation the
    // snapshot describes, so a rotation is one comparison rather than a subscription.
    private readonly record struct Ranked(int Number, Seq<DesignPoint> Members, ImmutableArray<int> Rank, ImmutableArray<double> Crowding);

    public NsgaFitness(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront seed) {
        (this.problem, this.policy, this.evaluate) = (problem, policy, evaluate);
        archive = Atom(seed);
        multipliers = Atom(new double[problem.Constraints]);
        snapshot = Atom(Rank(seed.Points, problem, 0));
    }

    public void Bind(Func<int> generations) => generation = generations;

    // The archive survives as ARCHIVE ONLY — the non-dominated set the run publishes — and no longer feeds any
    // fitness, which is what made it order-dependent.
    public ParetoFront Archive => archive.Value;

    public Option<Error> Fault => fault.Value;

    public double Evaluate(IChromosome chromosome) =>
        Probe(chromosome).Match(
            Succ: point => {
                Ranked frozen = Rotate();
                archive.Swap(front => front.Insert(point));
                multipliers.Swap(values => problem.Handling.Advance(values, point.Constraints.AsSpan(), policy.PenaltyWeight));
                // The genome's rank against the FROZEN population: the index of the first front no member of which
                // dominates it. A point dominated by nothing lands at rank 0 whatever the evaluation order.
                int rank = 0;
                for (int i = 0; i < frozen.Members.Count; i++) {
                    if (problem.Handling.Dominates(frozen.Members[i], point, problem.Senses.AsSpan())) { rank = Math.Max(rank, frozen.Rank[i] + 1); }
                }
                double crowding = Spread(frozen, point, rank);
                double penalty = problem.Handling.Penalize(
                    point.Objectives.IsDefaultOrEmpty ? 0.0 : point.Objectives[0] * problem.Senses[0],
                    point.Constraints.AsSpan(), policy.PenaltyWeight, multipliers.Value);
                return -rank + crowding - 1e-9 * penalty;
            },
            Fail: error => { fault.Swap(_ => Some(error)); return double.MinValue; });

    // ONE oracle call per distinct genome per run: the memo keys on the coordinate-only `DesignPoint` — the exact
    // value `Optimizer.Probe` constructs — under the generated structural equality, so a genome an elitist
    // reinsertion carries forward is never re-solved and no hand-derived bucket key or hit-confirming re-compare
    // exists. A full FE evaluation is the expensive thing here.
    Fin<DesignPoint> Probe(IChromosome chromosome) {
        DesignPoint probe = new(DecodeRaw(problem, chromosome), [], []);
        return cache.Value.Find(probe).Match(
            Some: Fin.Succ,
            None: () => Optimizer.Probe(problem, evaluate, probe.Coordinates).Map(point => {
                cache.Swap(held => held.AddOrUpdate(probe, point));
                return point;
            }));
    }

    // Rotation is EDGE-triggered on the engine's generation counter: the first genome scored in a new generation
    // freezes the archive's current non-dominated set as that generation's reference population, and every later
    // genome of the same generation reads it unchanged.
    //
    // A bare `Swap` whose body called `Rank` re-ran an O(N²) dominance sort inside the CAS on every contended
    // retry, unbounded, and answered only the post-state — so a genome that LOST the race could not tell that it
    // had. `Cell.Commit` snapshots, computes against that snapshot, commits by comparison under the kernel's own
    // attempt budget, and returns the VERDICT beside the state: a retry re-reads a cell a peer already advanced to
    // this generation, so the guard short-circuits and the sort runs at most twice however hot the contention.
    // `Current` is the snapshot every genome of this generation then ranks against, whichever caller froze it.
    Ranked Rotate() {
        int number = generation();
        Ranked held = snapshot.Value;
        return held.Number == number
            ? held
            : Cell.Commit(snapshot, state => state.Number == number ? state : Rank(archive.Value.Points, problem, number)).Current;
    }

    // Fast non-dominated sort (Deb 2002): domination counts and dominated sets in one O(N²) pass, then peel fronts.
    static Ranked Rank(Seq<DesignPoint> members, DesignProblem problem, int number) {
        int n = members.Count;
        int[] rank = new int[n], dominatedBy = new int[n];
        List<int>[] dominates = [.. Enumerable.Range(0, n).Select(static _ => new List<int>())];
        ReadOnlySpan<double> senses = problem.Senses.AsSpan();
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                if (i == j) { continue; }
                if (problem.Handling.Dominates(members[i], members[j], senses)) { dominates[i].Add(j); }
                else if (problem.Handling.Dominates(members[j], members[i], senses)) { dominatedBy[i]++; }
            }
        }
        List<int> current = [.. Enumerable.Range(0, n).Where(i => dominatedBy[i] == 0)];
        for (int front = 0; current.Count > 0; front++) {
            List<int> next = [];
            foreach (int i in current) {
                rank[i] = front;
                foreach (int j in dominates[i]) { if (--dominatedBy[j] == 0) { next.Add(j); } }
            }
            current = next;
        }
        return new Ranked(number, members, [.. rank], Crowding(members, rank));
    }

    // Crowding distance PER FRONT, Deb's boundary-infinite convention: each front sorts by every objective and each
    // interior member accumulates its normalized neighbour gap. Computing it over the whole population instead of
    // per front would let a rank-3 neighbour widen a rank-0 member's spread.
    static ImmutableArray<double> Crowding(Seq<DesignPoint> members, int[] rank) {
        int n = members.Count, objectives = n == 0 ? 0 : members[0].Objectives.Length;
        double[] distance = new double[n];
        foreach (int front in rank.Distinct()) {
            int[] group = [.. Enumerable.Range(0, n).Where(i => rank[i] == front)];
            if (group.Length < 3) { foreach (int i in group) { distance[i] = double.MaxValue; } continue; }
            for (int axis = 0; axis < objectives; axis++) {
                int[] order = [.. group.OrderBy(i => members[i].Objectives[axis])];
                distance[order[0]] = distance[order[^1]] = double.MaxValue;
                double range = Math.Max(1e-12, members[order[^1]].Objectives[axis] - members[order[0]].Objectives[axis]);
                for (int k = 1; k < order.Length - 1; k++) {
                    distance[order[k]] += (members[order[k + 1]].Objectives[axis] - members[order[k - 1]].Objectives[axis]) / range;
                }
            }
        }
        return [.. distance];
    }

    // The candidate's own spread against its rank's front: the mean neighbour gap of that front, folded into `(0,1)`
    // so it can never outweigh a whole rank step. A boundary member's infinite distance folds to the ceiling rather
    // than to a non-finite fitness the engine would sort as NaN.
    static double Spread(Ranked frozen, DesignPoint point, int rank) {
        double[] peers = [.. Enumerable.Range(0, frozen.Members.Count)
            .Where(i => frozen.Rank[i] == rank && double.IsFinite(frozen.Crowding[i]))
            .Select(i => frozen.Crowding[i])];
        double spread = peers.Length == 0 ? 1.0 : peers.Average();
        return double.IsFinite(spread) ? spread / (1.0 + spread) : 1.0;
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
