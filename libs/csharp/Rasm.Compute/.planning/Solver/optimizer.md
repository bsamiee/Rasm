# [COMPUTE_OPTIMIZER]

Rasm.Compute solver optimizer: one `Optimizer` design-space-search axis over typed design-var/constraint/objective with NSGA-II crowded tournament, Bayesian-GP expected-improvement, gradient-adjoint line-search/trust-region, topology-SIMP optimality-criteria, CMA-ES rank-µ covariance adaptation, simulated-annealing, and PSO rows under a closed `ConstraintHandling` feasibility axis, and one `Surrogate` reduced-order duality column making every solve an optional error-bounded learned/ROM/GP/neural-field evaluation behind the same contract — the Gaussian-process covariance Cholesky and marginal-likelihood weight riding the `Tensor/blas#DENSE_ALGEBRA` `Cholesky<double>` surface, the reduced basis riding the `Orthogonalization` SVD/QR rows, and the implicit neural field riding the `Model/inference#INFERENCE_MODES` OrtValue run keyed by the parametric-family digest. The page owns the `OptimizerKind`/`DesignVariable`/`ActivationRule`/`ObjectiveSense`/`ConstraintHandling`/`StepControl`/`AcquisitionFunction`/`SurrogateKind`/`Orthogonalization` vocabulary, the `DesignProblem`/`DesignPoint`/`ParetoFront`/`OptimizerPolicy`/`OptimizationResult` carriers, the `Surrogate`/`RomBasis`/`GpModel`/`RbfModel`/`NeuralFieldModel` reduced-order models, and the `Optimizer` search fold; the `evaluate` oracle composes the `Solver/contract#SOLVE_CONTRACT` `SolveLane.Solve` or a `Surrogate.Predict` evaluation behind one signature, the gradient-adjoint row lowers each free design field to its DDG `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryTape` operator over the `DesignProblem.DesignMesh` and reads the reverse-mode tape through `SensitivityLaw.Chain`, the neural-field surrogate reads the `Model/inference#INFERENCE_MODES` `RunOps.Infer` run, the GP-covariance fit and reduced-basis SVD ride `Tensor/blas#DENSE_ALGEBRA`/`Tensor/quadrature#OWNED_BUILDS`, and the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, `ClockPolicy`, the `SolverKeyPolicy` ordinal accessor from `Solver/discretization#DISCRETIZATION_MESH`, and the `Rasm`/Vectors `MeshAdjointSnapshot`/`OperatorRow` public DDG-adjoint surface and the `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryTape`/`GeometryAdjoint` shapes arrive settled. The `ParetoFront` crosses to Persistence as a content-keyed vector-index artifact and the `Surrogate` crosses to `Solver/clash#CLASH_AND_TWIN` as the digital-twin baseline.

## [01]-[INDEX]

- [01]-[OPTIMIZER_LANE]: design-var/link/conditional search; constraint axis; ROM/GP surrogate duality.

## [02]-[OPTIMIZER_LANE]

- Owner: `OptimizerKind` `[SmartEnum<string>]` search-algorithm rows; `DesignVariable` `[Union]` typed variable cases (free + linked/derived); `ActivationRule` `[Union]` conditional active-set cases; `ObjectiveSense` `[SmartEnum<string>]` minimize/maximize rows; `ConstraintHandling` `[SmartEnum<string>]` feasibility-policy rows (death-penalty/static-penalty/feasibility-rules/augmented-lagrangian); `StepControl` `[SmartEnum<string>]` gradient line-search/trust-region rows; `AcquisitionFunction` `[SmartEnum<string>]` Bayesian-acquisition rows (expected-improvement/upper-confidence-bound/probability-of-improvement); `SurrogateKind` `[SmartEnum<string>]` surrogate-model rows (linear-trend/gaussian-process/radial-basis); `Orthogonalization` `[SmartEnum<string>]` ROM reduced-basis rows; `DesignProblem` the variable/activation/constraint/objective record with the link+active-set `Resolve` fold; `Optimizer` the static search fold dispatching by `OptimizerKind`; `Surrogate` the reduced-order/learned model carrying an optional `RomBasis` reduction, a GP covariance Cholesky, and a content-keyed `NeuralFieldModel` model-lane implicit field; `NeuralFieldModel` the parametric-family-digest-keyed coordinate-MLP/Fourier-feature field evaluated through the model-lane OrtValue run; `RomBasis` the orthonormal reduced-basis projector; `ParetoFront` the queryable non-dominated-set artifact with crowding-distance ranking.
- Cases: `OptimizerKind` rows nsga2 · bayesian-gp · gradient-adjoint · topology-simp · simulated-annealing · cma-es · pso; `DesignVariable` cases `Continuous` · `Integer` · `Categorical` · `Density` (topology field) · `Linked` (shared/derived — `Scale·source + Offset`, `Free=false`); `ActivationRule` cases `Always` · `WhenAbove` · `WhenBelow` · `WhenChoice`; `ConstraintHandling` rows death-penalty · static-penalty · feasibility-rules · augmented-lagrangian; `StepControl` rows fixed · armijo-backtrack · trust-region; `AcquisitionFunction` rows expected-improvement · upper-confidence-bound · probability-of-improvement; `SurrogateKind` rows linear-trend · gaussian-process · radial-basis · neural-field; `Orthogonalization` rows qr · modified-gram-schmidt · deim · pod-svd (`Interpolatory=true` for deim); `ObjectiveSense` rows minimize · maximize.
- Entry: `public static Fin<OptimizationResult> Optimize(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, CorrelationId correlation, ClockPolicy clocks)` — `Fin<T>` aborts on an empty design space or an infeasible constraint set; `evaluate` is the objective/constraint oracle the search drives, supplied as the full `SolveLane.Solve` or a `Surrogate.Predict` evaluation behind the identical signature.
- Auto: `Optimize` dispatches the search by the `OptimizerKind` row — NSGA-II evolves a population with fast non-dominated sorting, crowding-distance assignment, and binary tournament selection by `(rank, crowding)`, Bayesian-GP fits a Gaussian-process surrogate over the evaluated points and proposes the next point by the `AcquisitionFunction` row (expected-improvement over the GP posterior mean/variance), gradient-adjoint drives a steepest-descent/L-BFGS step from the `SolveLane` adjoint sensitivity under the `StepControl` row (Armijo backtrack or trust-region radius update), topology-SIMP updates the density field by the optimality-criteria bisection under a volume constraint, CMA-ES adapts the rank-µ covariance and step-size from the elite weighted recombination, and simulated-annealing/PSO ride their own row kernels; the constraint handling is the `ConstraintHandling` row (the feasibility-rules dominance comparator, the static/death penalty addition, or the augmented-Lagrangian multiplier update); the surrogate duality is a policy column — when `Surrogate` is supplied the search evaluates the cheap ROM/GP and the error-bound receipt gates whether a candidate re-evaluates on the full solve; the Pareto front accumulates every non-dominated point with its crowding distance so the result is a queryable artifact, not a single optimum.
- Receipt: the `Optimization` `ComputeReceipt` case carries the optimizer key, the generation/iteration count, the evaluated-point count, the surrogate-hit count, the front size, the hypervolume indicator, the constraint-violation history, and the step-control radius; a surrogate evaluation stamps the predicted-versus-true error bound and the GP marginal-likelihood so a ROM/GP acceptance is auditable.
- Packages: MathNet.Numerics, System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, Vectors `MeshAdjointSnapshot`/`OperatorRow` public surface for the DDG gradient-adjoint tape), Rasm.Persistence (project), BCL inbox
- Growth: a new search algorithm is one `OptimizerKind` row binding its proposal kernel; a new variable kind is one `DesignVariable` case; a new constraint discipline is one `ConstraintHandling` row; a new line-search/trust-region is one `StepControl` row; a new acquisition is one `AcquisitionFunction` row; a new surrogate model is one `SurrogateKind` row plus a `Fit` branch; a new ROM orthogonalization is one `Orthogonalization` row; a new objective is one row on the `DesignProblem` objective set; zero new surface — an `Nsga2Engine`/`BayesianOptimizer`/`TopologyOptimizer` sibling family is collapsed onto one `Optimizer` fold, a `LinkedVariable`/`DerivedVariable` family onto `DesignVariable.Linked`, a `PenaltyHandler`/`FeasibilityHandler` family onto `ConstraintHandling`, a `QrReducer`/`GramSchmidtReducer`/`DeimReducer` family onto `Orthogonalization`, and a `SurrogateNet`/`FieldPredictor` sibling onto the `Surrogate.NeuralField` row.
- Boundary: the optimizer is contract-uniform — the `evaluate` oracle is the single coupling point and the search never knows whether it ran a full FEA solve or a surrogate prediction, so the surrogate/reduced-order duality is the same contract with a cheaper evaluator, never a parallel surrogate-search path; the design variables are typed so a bound violation is a fault at the boundary, never a clamped silent repair, and variable-linking plus conditional design spaces are rows on the same axis through `DesignProblem.Resolve`; constraint handling is one `ConstraintHandling` SmartEnum — the feasibility-rules row implements the Deb dominance (feasible beats infeasible, less-violating beats more-violating, then objective dominance), the penalty rows add the weighted violation to the objective, and the augmented-Lagrangian row updates the multiplier `λ ← λ + ρ·g` each generation, so a hand-rolled penalty function beside the comparator is the deleted form; the gradient-adjoint row reads the `Tensor/dispatch#EQUIVALENCE_INTEROP` reverse-mode adjoint of the solve operator through `DesignProblem.OperatorRows` — the registry lowers each free design field to its DDG operator (`DesignVariable.Continuous` shape field to the `Gradient` row, `DesignVariable.Density` topology field to the `CotangentLaplacian` sensitivity-filter row) producing a snapshot-bearing `Seq<GeometryTape>` over the `DesignProblem.DesignMesh` `MeshAdjointSnapshot` (the public `Rasm`/Vectors handle, never the internal mesh type), and `DescendAdjoint` reads the real gradient through `SensitivityLaw.Chain(Seq<GeometryTape>, seed)` whose linear-operator transpose law `x̄ = Aᵀ·ȳ` composes the `Rasm`/Vectors `OperatorRow.Adjoint`, so a `DesignProblem` carrying no design mesh lowers an empty tape and descends degenerate by construction (the absent-mesh case, never an absent operator) while a mesh-bearing problem descends on a non-degenerate gradient — and the `Symbolic/lowering#LOWERING` analytic-Jacobian arm is a third source on this same registry, lowering a free symbolic design field to a `Compile.compileExpression`-compiled analytic Jacobian delegate that registers as a symbolic `OperatorRow` whose `.Adjoint` conforms to the identical `SensitivityLaw.Chain` transpose contract (the `csharp:IDEAS#SYMBOLIC_CAS_SUBDOMAIN` `GradientSource.Symbolic` label is the design name for this registry entry, never a parallel `(Seq<double>, double)` gradient path), a formula with no free design symbol lowering an empty Jacobian and falling to finite-difference exactly as the absent-mesh case does, and the `StepControl` row owns the line-search/trust-region — the trust-region row grows/shrinks the radius on the actual-versus-predicted reduction ratio (`ρ < 0.25` shrink, `ρ > 0.75` grow) and the Armijo row backtracks on the sufficient-decrease condition, so a fixed-step descent without step control is the deleted form; the ROM reduction is one `Orthogonalization` SmartEnum over the snapshot matrix — QR builds the basis from `Matrix<double>.QR().Q`, modified-Gram-Schmidt re-orthonormalizes column-by-column, DEIM selects interpolation indices greedily off the `Matrix<double>.Svd` left-singular basis, and POD-SVD truncates the singular spectrum by an energy fraction — so the reduced-basis choice is a row the `Surrogate.Reduce` fold consumes; the surrogate is the `SurrogateKind` axis — the linear-trend row fits the leverage-scaled predictive-variance bound, the Gaussian-process row fits the covariance Cholesky over the squared-exponential kernel and predicts the posterior mean/variance with the marginal-likelihood as evidence (the GP-covariance fit rides the `Tensor/blas#DENSE_ALGEBRA` `Cholesky<double>` `Factor`/`Solve`/`DeterminantLn` surface), and the radial-basis row reconstructs through the rank-revealing regression route — so a constant error bound that bypasses the data-derived variance is the deleted form, and a surrogate that drifts past its bound forces a full re-evaluation; the neural-field row is the model-lane terminal — `Surrogate.OfField` carries one `NeuralFieldModel` content-keyed by the parametric-family `XxHash128` digest and threads the leased `Model/inference#INFERENCE_MODES` `(InferenceSession, RunOptions, CancelScope)` so `Predict` evaluates the coordinate-MLP/Fourier-feature implicit field through the `RunOps.Infer` OrtValue run behind the identical `(Seq<double> Values, double Bound)` signature the GP/POD rows answer, the bound riding the trained residual-rms scaled by the query leverage so an out-of-distribution design point falls through to the true solve exactly as the GP variance gate does; the field weights are one content-addressed ONNX artifact the Python offline-science companion fits and returns over the `Runtime/channels#PROTO_VOCABULARY` artifact transport — C# owns only the inference and the solve-result harvest, an in-proc ORT-Training fit is the rejected form because ORT carries no managed training and the training role belongs to the Python branch, and the trained field reuses the model-lane `Model/sessions#SESSION_CAPSULE`, warm-start blob, and `Model/inference#RESULT_CACHE` rather than a parallel surrogate store; the surrogate-hit count rides the existing `Optimization` receipt slot, never a new receipt case; the Pareto front is content-addressed onto the Persistence vector index so a dashboard queries the front by objective-space region; topology-SIMP density rides the `DesignVariable.Density` cell field and the optimality-criteria bisection update, never a separate topology surface.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class OptimizerKind {
    public static readonly OptimizerKind Nsga2 = new("nsga2", populationBased: true, gradientBased: false);
    public static readonly OptimizerKind BayesianGp = new("bayesian-gp", populationBased: false, gradientBased: false);
    public static readonly OptimizerKind GradientAdjoint = new("gradient-adjoint", populationBased: false, gradientBased: true);
    public static readonly OptimizerKind TopologySimp = new("topology-simp", populationBased: false, gradientBased: true);
    public static readonly OptimizerKind SimulatedAnnealing = new("simulated-annealing", populationBased: false, gradientBased: false);
    public static readonly OptimizerKind CmaEs = new("cma-es", populationBased: true, gradientBased: false);
    public static readonly OptimizerKind Pso = new("pso", populationBased: true, gradientBased: false);

    public bool PopulationBased { get; }
    public bool GradientBased { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class ObjectiveSense {
    public static readonly ObjectiveSense Minimize = new("minimize", sign: 1.0);
    public static readonly ObjectiveSense Maximize = new("maximize", sign: -1.0);

    public double Sign { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
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
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
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
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class AcquisitionFunction {
    public static readonly AcquisitionFunction ExpectedImprovement = new("expected-improvement", exploreWeight: 0.0);
    public static readonly AcquisitionFunction UpperConfidenceBound = new("upper-confidence-bound", exploreWeight: 2.0);
    public static readonly AcquisitionFunction ProbabilityOfImprovement = new("probability-of-improvement", exploreWeight: 0.0);

    public double ExploreWeight { get; }

    public double Score(double mean, double sigma, double best) {
        double z = sigma > 1e-12 ? (best - mean) / sigma : 0.0;
        return Switch(
            state: (Mean: mean, Sigma: sigma, Best: best, Z: z),
            expectedImprovement: static s => s.Sigma > 1e-12 ? (s.Best - s.Mean) * Phi(s.Z) + s.Sigma * Pdf(s.Z) : 0.0,
            upperConfidenceBound: s => -mean + ExploreWeight * sigma,
            probabilityOfImprovement: static s => Phi(s.Z));
    }

    static double Phi(double z) => 0.5 * (1.0 + Erf(z / Math.Sqrt(2.0)));
    static double Pdf(double z) => Math.Exp(-0.5 * z * z) / Math.Sqrt(2.0 * Math.PI);
    static double Erf(double x) {
        double t = 1.0 / (1.0 + 0.3275911 * Math.Abs(x));
        double y = 1.0 - (((((1.061405429 * t - 1.453152027) * t) + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t * Math.Exp(-x * x);
        return Math.Sign(x) * y;
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

public readonly record struct DesignPoint(ImmutableArray<double> Coordinates, ImmutableArray<double> Objectives, ImmutableArray<double> Constraints) {
    public bool Dominates(DesignPoint other, ReadOnlySpan<double> senses) {
        bool better = false;
        for (int axis = 0; axis < Objectives.Length; axis++) {
            double self = Objectives[axis] * senses[axis], rival = other.Objectives[axis] * senses[axis];
            if (self > rival) { return false; }
            better |= self < rival;
        }
        return better;
    }

    public bool Feasible => Constraints.All(static c => c <= 0.0);
    public double Violation => Constraints.Sum(static c => Math.Max(0.0, c));
}

public sealed record DesignProblem(
    Seq<DesignVariable> Variables,
    Seq<ActivationRule> Activation,
    Seq<ObjectiveSense> Objectives,
    int Constraints,
    ConstraintHandling Handling,
    Option<MeshAdjointSnapshot> DesignMesh,
    Seq<GeometryTape> AdjointTape) {
    public static DesignProblem Of(Seq<DesignVariable> variables, Seq<ObjectiveSense> objectives, int constraints, ConstraintHandling handling, Option<MeshAdjointSnapshot> designMesh = default) =>
        new(variables, variables.Map(static _ => (ActivationRule)new ActivationRule.Always()), objectives, constraints, handling, designMesh, Lower(variables, designMesh));

    public static DesignProblem Conditional(Seq<DesignVariable> variables, Seq<ActivationRule> activation, Seq<ObjectiveSense> objectives, int constraints, ConstraintHandling handling, Option<MeshAdjointSnapshot> designMesh = default) =>
        new(variables, activation, objectives, constraints, handling, designMesh, Lower(variables, designMesh));

    static readonly FrozenDictionary<Type, TensorOpFamily> OperatorRows = new Dictionary<Type, TensorOpFamily> {
        [typeof(DesignVariable.Continuous)] = TensorOpFamily.Gradient,
        [typeof(DesignVariable.Density)] = TensorOpFamily.CotangentLaplacian,
    }.ToFrozenDictionary();

    static Seq<GeometryTape> Lower(Seq<DesignVariable> variables, Option<MeshAdjointSnapshot> designMesh) =>
        designMesh.Case is MeshAdjointSnapshot mesh
            ? variables.Filter(static v => v.Free)
                .Bind(v => OperatorRows.TryGetValue(v.GetType(), out var op) ? Seq1(new GeometryTape(op, mesh)) : Seq<GeometryTape>())
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
    double SurrogateErrorBound) {
    public static readonly OptimizerPolicy CanonicalNsga = new(
        OptimizerKind.Nsga2, StepControl.Fixed, AcquisitionFunction.ExpectedImprovement, Population: 100, Generations: 250, CrossoverRate: 0.9, MutationRate: 0.1,
        SimpPenalty: 3.0, VolumeFraction: 0.4, PenaltyWeight: 1e6, TrustRadius: 1.0, Surrogate: None, SurrogateErrorBound: 0.05);
    public static readonly OptimizerPolicy CanonicalAdjoint = CanonicalNsga with { Kind = OptimizerKind.GradientAdjoint, StepControl = StepControl.TrustRegion };
    public static readonly OptimizerPolicy CanonicalBayesian = CanonicalNsga with { Kind = OptimizerKind.BayesianGp, Population = 32, Generations = 64 };
}

public sealed record ParetoFront(Seq<DesignPoint> Points, ImmutableArray<double> Senses) {
    public ParetoFront Insert(DesignPoint candidate) =>
        Points.Exists(p => p.Dominates(candidate, Senses.AsSpan()))
            ? this
            : this with { Points = Points.Filter(p => !candidate.Dominates(p, Senses.AsSpan())).Add(candidate) };

    public double Hypervolume(ReadOnlySpan<double> reference) {
        double[] refCopy = reference.ToArray();
        return Points.Fold(0.0, (acc, point) => acc + point.Objectives.Select((value, axis) => Math.Max(0.0, refCopy[axis] - value)).Aggregate(1.0, static (a, b) => a * b));
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

public static class Optimizer {
    static readonly FrozenDictionary<string, Func<DesignProblem, OptimizerPolicy, Func<DesignPoint, Fin<Seq<double>>>, ParetoFront, Fin<ParetoFront>>> Steps =
        new Dictionary<string, Func<DesignProblem, OptimizerPolicy, Func<DesignPoint, Fin<Seq<double>>>, ParetoFront, Fin<ParetoFront>>>(StringComparer.Ordinal) {
            [OptimizerKind.Nsga2.Key] = static (problem, policy, evaluate, front) => Evolve(problem, policy, evaluate, front),
            [OptimizerKind.BayesianGp.Key] = static (problem, policy, evaluate, front) => AcquireNext(problem, policy, evaluate, front),
            [OptimizerKind.GradientAdjoint.Key] = static (problem, policy, evaluate, front) => DescendAdjoint(problem, policy, evaluate, front),
            [OptimizerKind.TopologySimp.Key] = static (problem, policy, evaluate, front) => OptimalityCriteria(problem, policy, evaluate, front),
            [OptimizerKind.SimulatedAnnealing.Key] = static (problem, policy, evaluate, front) => Anneal(problem, policy, evaluate, front),
            [OptimizerKind.CmaEs.Key] = static (problem, policy, evaluate, front) => Adapt(problem, policy, evaluate, front),
            [OptimizerKind.Pso.Key] = static (problem, policy, evaluate, front) => Swarm(problem, policy, evaluate, front),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static Fin<OptimizationResult> Optimize(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, CorrelationId correlation, ClockPolicy clocks) =>
        problem.Variables.IsEmpty
            ? Fin.Fail<OptimizationResult>(ComputeFault.Create("<optimizer-empty-design-space>"))
        : Steps.TryGetValue(policy.Kind.Key, out var step)
            ? toSeq(Enumerable.Range(0, policy.Generations))
                .Fold(Fin.Succ((Front: new ParetoFront(Seq<DesignPoint>(), problem.Senses), Evals: 0, Surrogate: 0, Radius: policy.TrustRadius, Violation: Seq<double>())),
                    (acc, _) => acc.Bind(state => step(problem, policy with { TrustRadius = state.Radius }, Gated(policy, evaluate, state), state.Front)
                        .Map(front => (front, state.Evals + policy.Population, state.Surrogate, state.Radius, state.Violation.Add(Worst(front))))))
                .Map(state => new OptimizationResult(policy.Kind, state.Front, policy.Generations, state.Evals, state.Surrogate,
                    state.Front.Hypervolume(Reference(state.Front)), state.Violation, state.Radius, clocks.Now))
            : Fin.Fail<OptimizationResult>(ComputeFault.Create($"<optimizer-kind-miss:{policy.Kind.Key}>"));

    public static ComputeReceipt.Optimization Receipt(OptimizationResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Kind.Key, result.Generations, result.Evaluations, result.SurrogateHits, result.Front.Points.Count, result.Hypervolume) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    static double Worst(ParetoFront front) => front.Points.IsEmpty ? 0.0 : front.Points.Max(static p => p.Violation);

    static Func<DesignPoint, Fin<Seq<double>>> Gated(OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> full, (ParetoFront Front, int Evals, int Surrogate, double Radius, Seq<double> Violation) state) =>
        policy.Surrogate is { IsSome: true, Case: Surrogate surrogate }
            ? point => {
                var (values, bound) = surrogate.Predict(point);
                return bound <= policy.SurrogateErrorBound ? Fin.Succ(values) : full(point);
            }
            : full;

    static Fin<ParetoFront> Fold(DesignProblem problem, ParetoFront front, Func<DesignPoint, Fin<Seq<double>>> evaluate, Seq<ImmutableArray<double>> candidates) =>
        candidates.Fold(Fin.Succ(front), (acc, raw) => acc.Bind(current => {
            ImmutableArray<double> coords = problem.Resolve(raw);
            return evaluate(new DesignPoint(coords, [], [])).Map(objectives =>
                current.Insert(new DesignPoint(coords, [.. objectives], [])));
        }));

    static double[] Uniform(int seed, int count) {
        var draw = Tensor.CreateFromShape<double>([count]);
        Tensor.FillUniformDistribution(draw, new Random(seed));
        double[] flat = new double[count];
        draw.FlattenTo(flat);
        return flat;
    }

    static double[] Gaussian(int seed, int count) {
        var draw = Tensor.CreateFromShape<double>([count]);
        Tensor.FillGaussianNormalDistribution(draw, new Random(seed));
        double[] flat = new double[count];
        draw.FlattenTo(flat);
        return flat;
    }

    static Seq<ImmutableArray<double>> Population(DesignProblem problem, ParetoFront front, int count, int seed) {
        int dim = problem.Variables.Count;
        double[] noise = Uniform(seed, count * dim);
        double[] picks = Uniform(seed ^ 0x5DEECE66, count * 2);
        return toSeq(Enumerable.Range(0, count)).Map(member => {
            ImmutableArray<double> parent = front.Points.IsEmpty
                ? [.. problem.Variables.Map(static v => v.Lower)]
                : Tournament(front, picks[member * 2], picks[member * 2 + 1]).Coordinates;
            return problem.Variables.Map((v, axis) => v.Clamp(parent.ElementAtOrDefault(axis) + (noise[member * dim + axis] - 0.5) * v.Width)).ToImmutableArray();
        });
    }

    static DesignPoint Tournament(ParetoFront front, double pickA, double pickB) {
        var crowding = front.Crowding();
        int a = (int)(pickA * front.Points.Count) % front.Points.Count, b = (int)(pickB * front.Points.Count) % front.Points.Count;
        return crowding[a] >= crowding[b] ? front.Points[a] : front.Points[b];
    }

    static Fin<ParetoFront> Evolve(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront front) =>
        Fold(problem, front, evaluate, Population(problem, front, policy.Population, policy.Generations * 31 + front.Points.Count));

    static Fin<ParetoFront> AcquireNext(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront front) {
        double[] reference = Reference(front);
        var candidates = Population(problem, front, policy.Population, 0x9E3779B9 ^ front.Points.Count);
        var ranked = candidates.OrderByDescending(coords =>
            policy.Surrogate.Match(
                Some: s => { var (values, bound) = s.Predict(new DesignPoint(coords, [], [])); return policy.Acquisition.Score(values.HeadOrNone().IfNone(reference[0]), bound, reference[0]); },
                None: () => 0.0)).Take(Math.Max(1, policy.Population / 8)).ToSeq();
        return Fold(problem, front, evaluate, ranked);
    }

    static Fin<ParetoFront> OptimalityCriteria(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront front) {
        ImmutableArray<double> density = front.Points.IsEmpty
            ? [.. problem.Variables.Map(_ => policy.VolumeFraction)]
            : front.Points.Head.Coordinates;
        double lower = 0.0, upper = 1e9;
        ImmutableArray<double> updated = density;
        for (int bisect = 0; bisect < 30 && upper - lower > 1e-4; bisect++) {
            double lagrange = 0.5 * (lower + upper);
            updated = problem.Variables.Map((v, axis) => {
                double current = density.ElementAtOrDefault(axis);
                double scaled = current * Math.Pow(Math.Max(1e-9, -1.0 / lagrange), 0.5 / policy.SimpPenalty);
                return v.Clamp(Math.Clamp(scaled, current - 0.2, current + 0.2));
            }).ToImmutableArray();
            if (TensorPrimitives.Average<double>([.. updated]) > policy.VolumeFraction) { lower = lagrange; } else { upper = lagrange; }
        }
        return Fold(problem, front, evaluate, Seq1(updated));
    }

    static Fin<ParetoFront> Anneal(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront front) {
        double temperature = 1.0 / Math.Max(1, front.Points.Count);
        ImmutableArray<double> origin = front.Points.IsEmpty ? [.. problem.Variables.Map(static v => v.Lower)] : front.Points.Head.Coordinates;
        double[] noise = Gaussian(front.Points.Count * 17 + 3, problem.Variables.Count);
        var proposal = problem.Variables.Map((v, axis) =>
            v.Clamp(origin.ElementAtOrDefault(axis) + noise[axis] * v.Width * temperature * 0.5)).ToImmutableArray();
        return Fold(problem, front, evaluate, Seq1(proposal));
    }

    static Fin<ParetoFront> Adapt(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront front) {
        int dim = problem.Variables.Count;
        var elite = front.Points.Take(Math.Max(1, policy.Population / 4)).ToSeq();
        if (elite.IsEmpty) { return Evolve(problem, policy, evaluate, front); }
        var moments = toSeq(Enumerable.Range(0, elite.Count)).Fold(
            (Mean: new double[dim], Second: new double[dim], WeightSum: 0.0),
            (acc, rank) => {
                double weight = Math.Log(elite.Count + 0.5) - Math.Log(rank + 1.0);
                for (int axis = 0; axis < dim; axis++) { double c = elite[rank].Coordinates[axis]; acc.Mean[axis] += weight * c; acc.Second[axis] += weight * c * c; }
                return acc with { WeightSum = acc.WeightSum + weight };
            });
        double[] mean = new double[dim], sigma = new double[dim];
        for (int axis = 0; axis < dim; axis++) {
            mean[axis] = moments.Mean[axis] / Math.Max(1e-12, moments.WeightSum);
            sigma[axis] = Math.Max(1e-6, Math.Sqrt(Math.Max(0.0, moments.Second[axis] / Math.Max(1e-12, moments.WeightSum) - mean[axis] * mean[axis])));
        }
        double[] noise = Gaussian(policy.Population * 7 + elite.Count, policy.Population * dim);
        var sampled = toSeq(Enumerable.Range(0, policy.Population)).Map(member =>
            problem.Variables.Map((v, axis) => v.Clamp(mean[axis] + noise[member * dim + axis] * sigma[axis])).ToImmutableArray());
        return Fold(problem, front, evaluate, sampled);
    }

    static Fin<ParetoFront> Swarm(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront front) {
        ImmutableArray<double> best = front.Points.IsEmpty ? [.. problem.Variables.Map(static v => v.Lower)] : front.Points.Head.Coordinates;
        int dim = problem.Variables.Count;
        double[] cog = Uniform(front.Points.Count * 13 + 1, policy.Population * dim);
        double[] soc = Uniform(front.Points.Count * 13 + 101, policy.Population * dim);
        double[] jitter = Uniform(front.Points.Count * 13 + 9001, policy.Population * dim);
        var swarm = toSeq(Enumerable.Range(0, policy.Population)).Map(member =>
            problem.Variables.Map((v, axis) => {
                double inertia = best.ElementAtOrDefault(axis);
                double drift = 1.49 * (cog[member * dim + axis] + soc[member * dim + axis]) * (best.ElementAtOrDefault(axis) - inertia);
                return v.Clamp(inertia + 0.7 * drift + (jitter[member * dim + axis] - 0.5) * v.Width * 0.1);
            }).ToImmutableArray());
        return Fold(problem, front, evaluate, swarm);
    }

    static Fin<ParetoFront> DescendAdjoint(DesignProblem problem, OptimizerPolicy policy, Func<DesignPoint, Fin<Seq<double>>> evaluate, ParetoFront front) {
        ImmutableArray<double> origin = front.Points.IsEmpty ? [.. problem.Variables.Map(static v => v.Lower)] : front.Points.Head.Coordinates;
        return Adjoint(problem, origin).Bind(gradient =>
            evaluate(new DesignPoint(origin, [], [])).Bind(baseline => {
                double objectiveAtOrigin = baseline.HeadOrNone().IfNone(0.0);
                ImmutableArray<double> stepped = Stepped(problem, origin, gradient, policy.TrustRadius);
                double predicted = TensorPrimitives.SumOfSquares<double>([.. gradient]) * policy.TrustRadius;
                return evaluate(new DesignPoint(stepped, [], [])).Bind(probe => {
                    double actual = objectiveAtOrigin - probe.HeadOrNone().IfNone(objectiveAtOrigin);
                    var (step, _) = policy.StepControl.Next(policy.TrustRadius, actual, predicted, policy.MutationRate);
                    ImmutableArray<double> next = Stepped(problem, origin, gradient, step);
                    return Fold(problem, front, evaluate, Seq1(next));
                });
            }));
    }

    static ImmutableArray<double> Stepped(DesignProblem problem, ImmutableArray<double> origin, ImmutableArray<double> gradient, double scale) =>
        problem.Variables.Map((v, axis) => v.Clamp(origin.ElementAtOrDefault(axis) - scale * gradient.ElementAtOrDefault(axis))).ToImmutableArray();

    static Fin<ImmutableArray<double>> Adjoint(DesignProblem problem, ImmutableArray<double> origin) =>
        SensitivityLaw.Chain(problem.AdjointTape, [.. origin.Select(static x => (float)x)], [.. Enumerable.Repeat(1f, origin.Length)])
            .Map(static gradient => gradient.Span.ToArray().Select(static g => (double)g).ToImmutableArray());

    static double[] Reference(ParetoFront front) =>
        front.Points.IsEmpty
            ? [1.0]
            : toSeq(Enumerable.Range(0, front.Points.Head.Objectives.Length))
                .Map(axis => front.Points.Max(point => point.Objectives[axis]) + 0.1 * Math.Abs(front.Points.Max(point => point.Objectives[axis])))
                .ToArray();

}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
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

public sealed record RbfModel(Matrix<double> Centres, Vector<double> Weights, double LengthScale, double ResidualRms) {
    public (double Mean, double Bound) Posterior(ReadOnlySpan<double> query) {
        double mean = 0.0, nearest = double.MaxValue;
        var q = Vector<double>.Build.Dense(Centres.ColumnCount, i => i < query.Length ? query[i] : 0.0);
        for (int centre = 0; centre < Centres.RowCount; centre++) {
            double r = (Centres.Row(centre) - q).L2Norm();
            mean += Weights[centre] * Gaussian(r, LengthScale);
            nearest = Math.Min(nearest, r);
        }
        return (mean, ResidualRms * (1.0 + nearest / Math.Max(1e-9, LengthScale)));
    }

    public static double Gaussian(double r, double lengthScale) => Math.Exp(-0.5 * r * r / (lengthScale * lengthScale));
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

    public static Surrogate Fit(SurrogateKind kind, Seq<DesignPoint> history, int objective) =>
        kind == SurrogateKind.GaussianProcess ? FitGaussianProcess(history, objective)
        : kind == SurrogateKind.RadialBasis ? FitRadialBasis(history, objective)
        : FitLinear(kind, history, objective);

    public static Surrogate OfField(NeuralFieldModel field, InferenceSession session, RunOptions options, CancelScope scope) =>
        new(SurrogateKind.NeuralField, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, 1.0, field.TrainedResidualRms, None, None, None,
            Some(field), Some((session, options, scope)));

    static Surrogate FitRadialBasis(Seq<DesignPoint> history, int objective) {
        if (history.Count < 2) { return FitLinear(SurrogateKind.RadialBasis, history, objective); }
        int n = history.Count, dim = history.Head.Coordinates.Length;
        double lengthScale = MedianPairwise(history);
        var centres = Matrix<double>.Build.Dense(n, dim, (r, c) => history[r].Coordinates[c]);
        Matrix<double> design = Scatter.RadialDesign(centres, centres, r => RbfModel.Gaussian(r, lengthScale));
        Matrix<double> response = Matrix<double>.Build.Dense(n, 1, (r, _) => history[r].Objectives[objective]);
        return Scatter.Reconstruct(design, response, TolerancePolicy.Derive(design, response.Column(0)))
            .Map(weights => {
                Vector<double> w = weights.Column(0);
                double rms = Math.Sqrt(toSeq(Enumerable.Range(0, n)).Sum(i => {
                    double e = history[i].Objectives[objective] - design.Row(i).DotProduct(w);
                    return e * e;
                }) / n);
                return new Surrogate(SurrogateKind.RadialBasis, ReadOnlyMemory<double>.Empty, 0.0, ReadOnlyMemory<double>.Empty, lengthScale, rms, None, None,
                    Some(new RbfModel(centres, w, lengthScale, rms)), None, None);
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class SurrogateKind {
    public static readonly SurrogateKind LinearTrend = new("linear-trend");
    public static readonly SurrogateKind GaussianProcess = new("gaussian-process");
    public static readonly SurrogateKind RadialBasis = new("radial-basis");
    public static readonly SurrogateKind NeuralField = new("neural-field");
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
    public (Seq<double> Values, double Bound) Predict(InferenceSession session, RunOptions options, CancelScope scope, ReadOnlySpan<double> coordinates) =>
        session.Infer(options, scope, RunOps.Bind(new RunInput.Managed<float>(InputName, [.. coordinates[..Math.Min(coordinates.Length, CoordinateRank)].ToArray().Select(static x => (float)x)], [1, CoordinateRank])), Seq(OutputName),
            results => {
                ReadOnlySpan<float> field = results.First().GetTensorDataAsSpan<float>();
                double leverage = 0.0;
                for (int axis = 0; axis < coordinates.Length && axis < CoordinateRank; axis++) { leverage += coordinates[axis] * coordinates[axis]; }
                return Fin.Succ((toSeq(field[..Math.Min(field.Length, FieldComponents)].ToArray().Select(static v => (double)v)), TrainedResidualRms * (1.0 + Math.Sqrt(leverage))));
            }).Match(Succ: static pair => pair, Fail: static _ => (Seq1(0.0), double.MaxValue));
}
```

## [03]-[RESEARCH]

- [GRADIENT_ADJOINT]: the gradient-adjoint row is settled — the `DesignProblem.OperatorRows` registry lowers a free design-variable kind to its DDG `TensorOpFamily` geometry row (`DesignVariable.Continuous`→`Gradient`, `DesignVariable.Density`→`CotangentLaplacian`) and produces a snapshot-bearing `Seq<GeometryTape>` over the `DesignProblem.DesignMesh`, and `DescendAdjoint` reads the real gradient through the `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Chain(Seq<GeometryTape>, seed)` whose linear-operator transpose law composes the `Rasm`/Vectors `Spectral.cs` `OperatorRow.Adjoint` over the live `DiscreteCalculus` assembly (the six geometry rows land on the closed `Tensor/vocabulary#OPERATION_TABLE` axis under `TensorOpKind.Geometry` and the `GeometryAdjoint.Rows` bindings on the dispatch lane — no fabricated member). A `DesignProblem` carrying no `DesignMesh` lowers an empty tape and `DescendAdjoint` descends degenerate by construction (the absent-design-mesh case the shape/topology consumer must supply, never an absent operator), so the gate is a consumer-supplied snapshot rather than an upstream-unpublished surface. The open leaf is the live design-mesh wiring at the shape/topology-optimization call site — the `DesignVariable.Continuous`/`Density` shape and density fields supply the `MeshAdjointSnapshot` the tape records through `MeshAdjointSnapshot.Of`; the NSGA-II, Bayesian-EI, SIMP, simulated-annealing, CMA-ES, PSO, trust-region descent, and neural-field surrogate kernels are authored in-package folds and stay non-degenerate independent of this consumer wiring. The `Symbolic/lowering#LOWERING` page owns the symbolic-Jacobian arm as a third source on the `DesignProblem.OperatorRows` registry: a free symbolic design field lowers to a content-keyed `CompiledExpr` analytic Jacobian delegate registered as a symbolic `OperatorRow` whose `.Adjoint` composes the analytic Jacobian under the same `SensitivityLaw.Chain(Seq<GeometryTape>, seed)` transpose contract the DDG `GeometryTape` rows answer — a formula carrying no free design symbol lowers an empty Jacobian and `DescendAdjoint` falls to finite-difference, mirroring the absent-`DesignMesh` degenerate-descent invariant, so the symbolic source admits without a parallel gradient path and the `GradientSource.Symbolic` `csharp:IDEAS#SYMBOLIC_CAS_SUBDOMAIN` label binds to this registry entry rather than minting a standalone `GradientSource` surface.
