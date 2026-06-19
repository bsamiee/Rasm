# [COMPUTE_SOLVE_CONTRACT]

Rasm.Compute solve contract: one `PhysicsKind`×`BoundaryCondition`×`ElementClass` solve axis admitting FEA, CFD, thermal, daylight, energy, acoustic, electromagnetic, and multi-physics problems on the discretized field as uniform `SolveProblem` rows, one `SolveLane` static fold assembling the discrete `Bᵀ·D·B` operator over the `Solver/discretization#DISCRETIZATION_MESH` `DiscreteMesh`, dispatching to the numeric-lane factorization or iterative solve, marching the transient/nonlinear loop, and driving the adaptive-recovery ladder, and one `CoupledLane` multi-physics fold over field sets bound by `FieldTransfer` rows under Aitken Δ²-relaxation. The page owns the `PhysicsKind`/`BoundaryCondition`/`ConstraintMethod`/`SolveMethod`/`TimeIntegrator`/`CouplingScheme`/`RecoveryAction` vocabulary, the `SolveProblem`/`SolveResult`/`ConstrainedSystem` carriers, the `RecoveryPolicy`/`CouplingPolicy` policies, and the `SolveLane`/`CoupledLane` folds; the dense/sparse factorization and iterative solve ride `Tensor/blas#DENSE_ALGEBRA`/`Tensor/factor#SPARSE_SOLVE`, the `ElementClass.ShapeGrad`/`QuadratureRule`/`DiscreteMesh`/`FieldSpace` shapes arrive settled from `Solver/discretization#DISCRETIZATION_MESH`, the gradient-adjoint tape rides `Tensor/dispatch#EQUIVALENCE_INTEROP`, and the distributed solve dials the existing `Runtime/channels#PROTO_VOCABULARY` `Solve` rpc. Every solver receipt is typed and the page carries no TS_PROJECTION because solve interiors stay host-local behind the `Solve` rpc.

## [1]-[INDEX]

- [1]-[SOLVE_CONTRACT]: physics×BC×element solve axis; transient/nonlinear; multi-physics; recovery.

## [2]-[SOLVE_CONTRACT]

- Owner: `PhysicsKind` `[SmartEnum<string>]` physics-domain rows carrying symmetry, eigen, transient, and material-`D`-matrix columns; `BoundaryCondition` `[Union]` BC cases; `ConstraintMethod` `[SmartEnum<string>]` DOF-constraint-application rows (elimination/penalty/lagrange); `SolveMethod` `[SmartEnum<string>]` linear/iterative/modal method rows carrying the numeric-lane `IterativeMethod`/`FactorizationKind` lowering and a `Preconditioner` column; `TimeIntegrator` `[SmartEnum<string>]` transient-marching rows (backward-euler/newmark-beta/generalized-alpha/central-difference); `CouplingScheme` `[SmartEnum<string>]` field-transfer rows with Aitken relaxation; `RecoveryAction` `[SmartEnum<string>]` non-convergence-recovery rows; `SolveProblem` the uniform problem record; `SolveLane` the static fold that assembles the discrete `Bᵀ·D·B` operator over the `DiscreteMesh`, dispatches to the `numeric-lane` factorization or iterative solve, marches the transient/nonlinear loop, and drives the adaptive-recovery ladder; `CoupledLane` the static multi-physics fold; `SolveResult` the field-plus-evidence carrier.
- Cases: `PhysicsKind` rows fea-static · fea-modal · fea-transient · fea-buckling · cfd-incompressible · thermal-steady · thermal-transient · daylight-radiosity · energy-balance · acoustic-helmholtz · electromagnetic-eddy; `BoundaryCondition` cases `Dirichlet(FieldStation Station, long[] Nodes, double[] Values)` · `Neumann(long[] Faces, double[] Flux)` · `Robin(long[] Faces, double Coefficient, double Ambient)` · `Periodic(long[] Master, long[] Slave)` · `Contact(long[] Slave, long[] Master, double Gap, double Penalty)`; `ConstraintMethod` rows elimination · penalty · lagrange; `SolveMethod` rows direct-lu · direct-cholesky · bicgstab · gpbicg · tfqmr · mlk-bicgstab · lobpcg (the four iterative rows mirror the verified numeric-lane `IterativeMethod` axis 1:1, each carrying its `IterativeMethod` Krylov column and the `Preconditioner.Diagonal` row — the only admitted `IPreconditioner<double>` concrete is the Jacobi `DiagonalPreconditioner`, so the preconditioner axis is the single real Diagonal row plus the `None` alias that builds the same diagonal factory, never a phantom incomplete-Cholesky/ILU type; lobpcg is the modal/Evd row routed through `Modal`); `TimeIntegrator` rows backward-euler · newmark-beta · generalized-alpha · central-difference; `CouplingScheme` rows one-way · two-way · staggered (the iterative rows adding Aitken Δ²-relaxation); `RecoveryAction` rows refine-mesh · relax · reorder-dofs · switch-method · restart.
- Entry: `public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, CorrelationId correlation, ClockPolicy clocks)` — `Fin<T>` aborts on an ill-posed BC set or a non-convergent run past the iteration cap; the modal physics row returns the eigenpairs through the `Evd`/LOBPCG route, the transient row marches the `TimeIntegrator` over the policy step set reusing one factorization, the nonlinear physics row drives a Newton-Raphson outer loop with a line-searched tangent update, and every other row the displacement/temperature/pressure field over the `FieldSpace`; `SolveAdaptive(..., RecoveryPolicy recovery, ...)` walks the `RecoveryAction` ladder on a `Fin.Fail`; `CoupledLane.Couple(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, ...)` solves the coupled field set under Aitken-relaxed staggering.
- Auto: `Solve` assembles the global stiffness/mass/conductivity operator by folding each element's local `Bᵀ·D·B` matrix — evaluated through the `ElementClass.ShapeGrad` delegate at each `QuadratureRule` Gauss point against the `PhysicsKind.Material` `D`-matrix — into the `SparseCompressedRowMatrixStorage<double>` the mesh connectivity addresses, applies the `BoundaryCondition` set by the `ConstraintMethod` row (row/column elimination, penalty diagonal augmentation, or Lagrange-multiplier bordering), and dispatches to the `Tensor/blas#DENSE_ALGEBRA` `DenseOps.Decompose`/`Tensor/factor#SPARSE_SOLVE` `SparseOps.Factor`/`SolveIterative` by the `SolveMethod` row's lowering; the physics row selects the assembly kernel (Poisson, elasticity, Helmholtz, Maxwell-eddy) and the operator symmetry so an SPD operator routes Cholesky/BiCgStab and an indefinite one routes LU/Tfqmr/MlkBiCgStab without a call-site branch; the transient row factors the effective operator `(M/Δt² + γC/Δt + βK)` once and back-substitutes every step, the nonlinear row re-assembles the tangent each Newton iteration and line-searches the step.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics key, method key, constraint key, DOF count, iteration count, the final residual, the converged flag, and elapsed; the modal row stamps the recovered eigenvalue count and the modal participation factors, the transient rows stamp the integrator key and step count, the nonlinear rows stamp the Newton iteration count and the load-step list, and the iterative rows ride the `rasm.compute.solve.residual` histogram instrument; the `Coupling` `ComputeReceipt` case carries the scheme key, field count, transfer count, round count, the Aitken factor history, the final coupling residual, and the converged flag; the `RecoveryReceipt` carries the physics key and the ordered `(action, post-recovery residual)` step list plus the recovered flag.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new physics domain is one `PhysicsKind` row carrying its assembly-kernel, symmetry, and `D`-matrix columns; a new BC kind is one `BoundaryCondition` case; a new constraint application is one `ConstraintMethod` row; a new linear method is one `SolveMethod` row carrying its lowering and preconditioner columns; a new time scheme is one `TimeIntegrator` row carrying its `(α,β,γ)` coefficient column; a new coupling discipline is one `CouplingScheme` row plus a `FieldTransfer` mapping; a new recovery strategy is one `RecoveryAction` row; zero new surface — a `CfdSolver`/`ThermalSolver`/`FeaSolver` sibling family is the rejected form collapsed onto the one `SolveLane` fold discriminated by `PhysicsKind`, a `NewmarkSolver`/`GeneralizedAlphaSolver` sibling family is collapsed onto the `TimeIntegrator` axis, and an `FsiCoupler`/`ThermalStructuralCoupler` sibling family is collapsed onto the one `CoupledLane` fold.
- Boundary: the solve contract is uniform — physics, boundary condition, element, and time scheme discriminate by row/case, never by a parallel solver type, so the same `Solve` runs an FEA static analysis, a transient thermal march, a CFD pressure-Poisson step, a buckling eigenproblem, and a Helmholtz acoustic mode; the discrete operator rides the numeric lane exclusively — assembly produces the CSR storage `SparseOps.Ingest(SparseFormat.Coo, …)` consumes and the factorization/iterative dispatch is `numeric-lane` machinery (`SparseOps.Factor`/`FactoredOp.Solve`/`SparseOps.SolveIterative`/`DenseOps.Decompose`), so this owner never re-mints a linear-algebra kernel and a hand-rolled CG loop beside `SparseOps` is the deleted form; the iterative `SolveMethod` rows carry an `IterativeMethod` lowering column and pass a derived `IterationPolicy` (tolerance/max-iter/criterion-stack/preconditioner) into `SparseOps.SolveIterative` — a raw-`string` method discriminant is the deleted form the numeric-lane Boundary names — and the LOBPCG eigensolver routes through the `Modal` Evd path; the BC application is one `ConstraintMethod` row — elimination partitions DOFs once into constrained and free sets folding constrained values into the RHS, penalty augments the diagonal by the policy factor, and Lagrange borders the system with the constraint matrix, so a penalty fallback is a row, never a second BC path, and the `Contact` case lowers to the penalty constraint over the gap function; the modal and transient rows compose the same assembled operator across steps so a transient sweep reuses one `Factorization` through the numeric-lane stored decomposition rather than re-factoring per step; the nonlinear physics row drives a Newton-Raphson loop where the tangent is the same `Bᵀ·D·B` re-assembly with the material `D` evaluated at the current state, the residual rides one held `SparseMatrix.OfStorage(operator)` sparse mat-vec (`A·x`) reused across every residual and Armijo backtrack rather than a per-call dense `SparseOfMatrix(...).Multiply` re-materialization, the Armijo baseline residual is computed once per line search, and the step is line-searched by the Armijo backtrack over `TensorPrimitives.MultiplyAdd`/`Subtract`/`Norm` — a fixed-step Picard iteration without a line search is the deleted form, and the iteration count stamped on the receipt is the genuine Newton step count, never a fabricated `MaxIterations`; the element-assembly `Triplets` fold partitions the per-cell `Bᵀ·D·B` local-stiffness blocks across `CommunityToolkit.HighPerformance` `ParallelHelper.For` with each cell renting its scratch block from `SpanOwner<double>.Allocate` and reading it through `ReadOnlySpan2D<double>`, so the `AllocationClass.PooledMemory` receipt is honest; the result field is the `FieldSpace` over the mesh stations and crosses to Persistence as a content-keyed result artifact; a distributed solve dials the existing `Runtime/channels#PROTO_VOCABULARY` `Solve` rpc through the `ShardPlan.Blocked` row-block fan-out; multi-physics coupling is one `CoupledLane` fold over ≥2 `SolveProblem` fields bound by `FieldTransfer` rows under Aitken Δ²-relaxation — the relaxation factor is computed from successive inter-field residuals (`ω = -ω·(r·Δr)/(Δr·Δr)`), never a fixed under-relaxation constant — so the coupling discipline is a `CouplingScheme` discriminant and the transferred field reuses the single `BoundaryCondition.Dirichlet` injection path; adaptive recovery is one `RecoveryAction` ladder fold on the same `Solve` — a divergent run relaxes the tolerance/cap, reorders DOFs through the fresh `CSparse.Ordering.AMD.Generate(csc, ColumnOrdering.MinimumDegreeAtPlusA)` permutation renumbering the operator, refines the mesh through `MeshKernel.Refine`, switches to the robust multiple-Lanczos `mlk-bicgstab` fallback, then restarts, and the `RecoveryReceipt` records which rung succeeded.

```csharp signature
public enum MaterialForm { Elasticity, Isotropic, MaxwellEddy }

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class PhysicsKind {
    public static readonly PhysicsKind FeaStatic = new("fea-static", symmetric: true, eigen: false, transient: false, nonlinear: false, MaterialForm.Elasticity, 0.0);
    public static readonly PhysicsKind FeaModal = new("fea-modal", symmetric: true, eigen: true, transient: false, nonlinear: false, MaterialForm.Elasticity, 0.0);
    public static readonly PhysicsKind FeaTransient = new("fea-transient", symmetric: true, eigen: false, transient: true, nonlinear: false, MaterialForm.Elasticity, 0.0);
    public static readonly PhysicsKind FeaBuckling = new("fea-buckling", symmetric: true, eigen: true, transient: false, nonlinear: false, MaterialForm.Elasticity, 0.0);
    public static readonly PhysicsKind CfdIncompressible = new("cfd-incompressible", symmetric: false, eigen: false, transient: true, nonlinear: true, MaterialForm.Isotropic, +1.0);
    public static readonly PhysicsKind ThermalSteady = new("thermal-steady", symmetric: true, eigen: false, transient: false, nonlinear: false, MaterialForm.Isotropic, 0.0);
    public static readonly PhysicsKind ThermalTransient = new("thermal-transient", symmetric: true, eigen: false, transient: true, nonlinear: false, MaterialForm.Isotropic, 0.0);
    public static readonly PhysicsKind DaylightRadiosity = new("daylight-radiosity", symmetric: true, eigen: false, transient: false, nonlinear: false, MaterialForm.Isotropic, 0.0);
    public static readonly PhysicsKind EnergyBalance = new("energy-balance", symmetric: false, eigen: false, transient: true, nonlinear: true, MaterialForm.Isotropic, 0.0);
    public static readonly PhysicsKind AcousticHelmholtz = new("acoustic-helmholtz", symmetric: false, eigen: false, transient: false, nonlinear: false, MaterialForm.Isotropic, -1.0);
    public static readonly PhysicsKind ElectromagneticEddy = new("electromagnetic-eddy", symmetric: false, eigen: false, transient: true, nonlinear: false, MaterialForm.MaxwellEddy, 0.0);

    public bool Symmetric { get; }
    public bool Eigen { get; }
    public bool Transient { get; }
    public bool Nonlinear { get; }
    public MaterialForm Form { get; }
    public double ShiftScale { get; }

    public double[] Material(double scale, double shift) =>
        Form switch {
            MaterialForm.Elasticity => Elasticity(scale),
            MaterialForm.MaxwellEddy => [scale, -shift, 0, shift, scale, 0, 0, 0, scale],
            _ => Isotropic(scale + ShiftScale * shift),
        };

    static double[] Elasticity(double scale) {
        double lambda = scale, mu = 0.5 * scale;
        return [lambda + 2 * mu, lambda, lambda, 0, 0, 0, lambda, lambda + 2 * mu, lambda, 0, 0, 0, lambda, lambda, lambda + 2 * mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu];
    }
    static double[] Isotropic(double diagonal) => [diagonal, 0, 0, 0, diagonal, 0, 0, 0, diagonal];
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class ConstraintMethod {
    public static readonly ConstraintMethod Elimination = new("elimination", bordered: false);
    public static readonly ConstraintMethod Penalty = new("penalty", bordered: false);
    public static readonly ConstraintMethod Lagrange = new("lagrange", bordered: true);

    public bool Bordered { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class SolveMethod {
    public static readonly SolveMethod DirectLu = new("direct-lu", iterative: false, kind: FactorizationKind.Lu, krylov: null, preconditioner: Preconditioner.None);
    public static readonly SolveMethod DirectCholesky = new("direct-cholesky", iterative: false, kind: FactorizationKind.Cholesky, krylov: null, preconditioner: Preconditioner.None);
    public static readonly SolveMethod BiCgStab = new("bicgstab", iterative: true, kind: FactorizationKind.Cholesky, krylov: IterativeMethod.BiCgStab, preconditioner: Preconditioner.Diagonal);
    public static readonly SolveMethod GpBiCg = new("gpbicg", iterative: true, kind: FactorizationKind.Lu, krylov: IterativeMethod.GpBiCg, preconditioner: Preconditioner.Diagonal);
    public static readonly SolveMethod Tfqmr = new("tfqmr", iterative: true, kind: FactorizationKind.Lu, krylov: IterativeMethod.Tfqmr, preconditioner: Preconditioner.Diagonal);
    public static readonly SolveMethod MlkBiCgStab = new("mlk-bicgstab", iterative: true, kind: FactorizationKind.Lu, krylov: IterativeMethod.MlkBiCgStab, preconditioner: Preconditioner.Diagonal);
    public static readonly SolveMethod Lobpcg = new("lobpcg", iterative: false, kind: FactorizationKind.Evd, krylov: null, preconditioner: Preconditioner.None);

    public bool Iterative { get; }
    public FactorizationKind Kind { get; }
    public Preconditioner Preconditioner { get; }
    private readonly IterativeMethod? krylov;

    public IterativeMethod Krylov => krylov ?? throw new InvalidOperationException($"<solve-method-not-iterative:{Key}>");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class Preconditioner {
    public static readonly Preconditioner None = new("none", DiagonalFactory);
    public static readonly Preconditioner Diagonal = new("diagonal", DiagonalFactory);

    [UseDelegateFromConstructor]
    public partial IPreconditioner<double> Build();

    static IPreconditioner<double> DiagonalFactory() => new DiagonalPreconditioner();
}

[SmartEnum]
public sealed partial class SolveKind {
    public static readonly SolveKind Direct = new();
    public static readonly SolveKind Iterative = new();
    public static readonly SolveKind Nonlinear = new();
    public static readonly SolveKind Transient = new();
    public static readonly SolveKind Eigen = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class TimeIntegrator {
    public static readonly TimeIntegrator BackwardEuler = new("backward-euler", alpha: 0.0, beta: 1.0, gamma: 1.0, implicit: true);
    public static readonly TimeIntegrator NewmarkBeta = new("newmark-beta", alpha: 0.0, beta: 0.25, gamma: 0.5, implicit: true);
    public static readonly TimeIntegrator GeneralizedAlpha = new("generalized-alpha", alpha: 0.05, beta: 0.275625, gamma: 0.55, implicit: true);
    public static readonly TimeIntegrator CentralDifference = new("central-difference", alpha: 0.0, beta: 0.0, gamma: 0.5, implicit: false);

    public double Alpha { get; }
    public double Beta { get; }
    public double Gamma { get; }
    public bool Implicit { get; }

    public double[] Effective(ReadOnlySpan<double> mass, ReadOnlySpan<double> damping, ReadOnlySpan<double> stiffness, double dt) {
        double[] effective = new double[stiffness.Length];
        double invDt2 = 1.0 / (Beta * dt * dt), cFactor = Gamma / (Beta * dt);
        for (int i = 0; i < effective.Length; i++) { effective[i] = mass[i] * invDt2 + damping[i] * cFactor + stiffness[i]; }
        return effective;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BoundaryCondition {
    private BoundaryCondition() { }

    public sealed record Dirichlet(FieldStation Station, long[] Nodes, double[] Values) : BoundaryCondition;
    public sealed record Neumann(long[] Faces, double[] Flux) : BoundaryCondition;
    public sealed record Robin(long[] Faces, double Coefficient, double Ambient) : BoundaryCondition;
    public sealed record Periodic(long[] Master, long[] Slave) : BoundaryCondition;
    public sealed record Contact(long[] Slave, long[] Master, double Gap, double Penalty) : BoundaryCondition;

    public ConstrainedSystem Apply(ConstrainedSystem system, ConstraintMethod constraint) =>
        Switch(
            state: (System: system, Constraint: constraint),
            dirichlet: static (s, bc) => {
                double[] rhs = (double[])s.System.Rhs.Clone();
                var fixedDofs = s.System.Constrained;
                for (int i = 0; i < bc.Nodes.Length; i++) {
                    rhs[bc.Nodes[i]] = s.Constraint == ConstraintMethod.Penalty ? s.System.Penalty * bc.Values[i] : bc.Values[i];
                    fixedDofs = fixedDofs.Add(bc.Nodes[i]);
                }
                return s.System with { Rhs = rhs, Constrained = fixedDofs };
            },
            neumann: static (s, bc) => {
                double[] rhs = (double[])s.System.Rhs.Clone();
                for (int i = 0; i < bc.Faces.Length; i++) { rhs[bc.Faces[i]] += bc.Flux[i]; }
                return s.System with { Rhs = rhs };
            },
            robin: static (s, bc) => {
                double[] rhs = (double[])s.System.Rhs.Clone();
                foreach (long face in bc.Faces) { rhs[face] += bc.Coefficient * bc.Ambient; }
                return s.System with { Rhs = rhs };
            },
            periodic: static (s, bc) => {
                var fixedDofs = s.System.Constrained;
                foreach (long slave in bc.Slave) { fixedDofs = fixedDofs.Add(slave); }
                return s.System with { Constrained = fixedDofs };
            },
            contact: static (s, bc) => {
                double[] rhs = (double[])s.System.Rhs.Clone();
                for (int i = 0; i < bc.Slave.Length; i++) { rhs[bc.Slave[i]] += bc.Penalty * Math.Max(0.0, bc.Gap); }
                return s.System with { Rhs = rhs };
            });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class RecoveryAction {
    public static readonly RecoveryAction RefineMesh = new("refine-mesh", rebuildsOperator: true);
    public static readonly RecoveryAction Relax = new("relax", rebuildsOperator: false);
    public static readonly RecoveryAction ReorderDofs = new("reorder-dofs", rebuildsOperator: true);
    public static readonly RecoveryAction SwitchMethod = new("switch-method", rebuildsOperator: false);
    public static readonly RecoveryAction Restart = new("restart", rebuildsOperator: false);

    public bool RebuildsOperator { get; }
}

public sealed record RecoveryPolicy(
    Seq<RecoveryAction> Ladder,
    MeshPolicy MeshPolicy,
    double RelaxFactor,
    double IterationGrowth,
    SolveMethod Fallback) {
    public static readonly RecoveryPolicy Canonical = new(
        Ladder: Seq(RecoveryAction.Relax, RecoveryAction.ReorderDofs, RecoveryAction.RefineMesh, RecoveryAction.SwitchMethod, RecoveryAction.Restart),
        MeshPolicy: MeshPolicy.CanonicalTet, RelaxFactor: 10.0, IterationGrowth: 2.0, Fallback: SolveMethod.MlkBiCgStab);
}

public sealed record RecoveryReceipt(string Physics, Seq<(string Action, double Residual)> Steps, bool Recovered, Instant At);

public sealed record SolvePolicy(
    SolveMethod Method,
    ConstraintMethod Constraint,
    TimeIntegrator Integrator,
    int MaxIterations,
    double Tolerance,
    int EigenPairs,
    double TimeStep,
    int TimeSteps,
    int NewtonIterations,
    double PenaltyFactor) {
    public static readonly SolvePolicy CanonicalStatic = new(SolveMethod.DirectCholesky, ConstraintMethod.Elimination, TimeIntegrator.BackwardEuler, MaxIterations: 1, Tolerance: 1e-9, EigenPairs: 0, TimeStep: 0.0, TimeSteps: 1, NewtonIterations: 1, PenaltyFactor: 1e12);
    public static readonly SolvePolicy CanonicalIterative = CanonicalStatic with { Method = SolveMethod.BiCgStab, MaxIterations = 2000, Tolerance = 1e-8 };
    public static readonly SolvePolicy CanonicalModal = CanonicalStatic with { Method = SolveMethod.Lobpcg, MaxIterations = 500, Tolerance = 1e-7, EigenPairs = 12 };
    public static readonly SolvePolicy CanonicalTransient = CanonicalStatic with { Method = SolveMethod.DirectLu, Integrator = TimeIntegrator.NewmarkBeta, TimeStep = 0.01, TimeSteps = 100 };
    public static readonly SolvePolicy CanonicalNonlinear = CanonicalIterative with { Method = SolveMethod.MlkBiCgStab, NewtonIterations = 25 };
}

public sealed record SolveProblem(
    PhysicsKind Physics,
    ElementClass Element,
    Seq<BoundaryCondition> Conditions,
    FieldSpace Unknown,
    double MaterialScale,
    double MaterialShift,
    UInt128 ContentKey) {
    public static SolveProblem Of(PhysicsKind physics, DiscreteMesh mesh, Seq<BoundaryCondition> conditions, int dim, double materialScale = 1.0, double materialShift = 0.0) =>
        new(physics, mesh.Element, conditions, mesh.FieldOf(FieldStation.Nodal, physics == PhysicsKind.FeaStatic ? 1 : 0, dim), materialScale, materialShift,
            XxHash128.HashToUInt128(MemoryMarshal.AsBytes($"{physics.Key}|{mesh.Element.Key}|{mesh.NodeCount}|{mesh.ElementCount}|{materialScale}|{materialShift}".AsSpan())));
}

public sealed record SolveResult(
    SolveProblem Problem,
    SolveMethod Method,
    ReadOnlyMemory<double> Field,
    Option<ReadOnlyMemory<double>> EigenValues,
    Option<ReadOnlyMemory<double>> Participation,
    long Dofs,
    int Iterations,
    int NewtonSteps,
    double Residual,
    bool Converged,
    Instant At);

public sealed record ConstrainedSystem(
    SparseCompressedRowMatrixStorage<double> Operator,
    double[] Rhs,
    LanguageExt.HashSet<long> Constrained,
    double Penalty) {
    public Matrix<double> Dense() => SparseMatrix.OfStorage(Operator).ToDense();
}

public static class SolveLane {
    static readonly FrozenDictionary<SolveKind, Func<ConstrainedSystem, DiscreteMesh, SolveProblem, SolvePolicy, Instant, Fin<SolveResult>>> Routes =
        new (SolveKind Kind, Func<ConstrainedSystem, DiscreteMesh, SolveProblem, SolvePolicy, Instant, Fin<SolveResult>> Run)[] {
            (SolveKind.Eigen, static (system, _, problem, policy, at) => Modal(system, problem, policy, at)),
            (SolveKind.Transient, static (system, mesh, problem, policy, at) => March(system, mesh, problem, policy, at)),
            (SolveKind.Nonlinear, static (system, mesh, problem, policy, at) => Newton(system, mesh, problem, policy, at)),
            (SolveKind.Iterative, static (system, _, problem, policy, at) => Iterative(system, problem, policy, at)),
            (SolveKind.Direct, static (system, _, problem, policy, at) => Direct(system, problem, policy, at)),
        }.ToFrozenDictionary(static row => row.Kind, static row => row.Run);

    static SolveKind Routed(PhysicsKind physics, SolveMethod method) =>
        physics.Eigen ? SolveKind.Eigen
        : physics.Transient ? SolveKind.Transient
        : physics.Nonlinear ? SolveKind.Nonlinear
        : method.Iterative ? SolveKind.Iterative
        : SolveKind.Direct;

    public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, CorrelationId correlation, ClockPolicy clocks) =>
        Assemble(problem, mesh, policy)
            .Bind(operatorCsr => Constrained(operatorCsr, problem.Conditions, policy)
                .Bind(system => Routes[Routed(problem.Physics, policy.Method)](system, mesh, problem, policy, clocks.Now)));

    public static (Fin<SolveResult> Result, RecoveryReceipt Trace) SolveAdaptive(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, RecoveryPolicy recovery, CorrelationId correlation, ClockPolicy clocks) {
        var final = recovery.Ladder.Fold(
            (Result: Solve(problem, mesh, policy, correlation, clocks), Problem: problem, Mesh: mesh, Policy: policy, Steps: Seq<(string Action, double Residual)>()),
            (state, action) => {
                if (state.Result.IsSucc) { return state; }
                var (nextProblem, nextMesh, nextPolicy) = Recover(action, state.Problem, state.Mesh, state.Policy, recovery, clocks);
                Fin<SolveResult> attempt = Solve(nextProblem, nextMesh, nextPolicy, correlation, clocks);
                return (attempt, nextProblem, nextMesh, nextPolicy, state.Steps.Add((action.Key, Residual(attempt))));
            });
        return (final.Result, new RecoveryReceipt(problem.Physics.Key, final.Steps, final.Result.IsSucc, clocks.Now));
    }

    static (SolveProblem Problem, DiscreteMesh Mesh, SolvePolicy Policy) Recover(RecoveryAction action, SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, RecoveryPolicy recovery, ClockPolicy clocks) =>
        action.Switch(
            state: (Problem: problem, Mesh: mesh, Policy: policy, Recovery: recovery, Clocks: clocks),
            refineMesh: static s => MeshKernel.Refine(s.Mesh, s.Recovery.MeshPolicy, RefinementError(s.Mesh), s.Clocks)
                .Match(Succ: refined => (s.Problem with { Element = refined.Element }, refined, s.Policy), Fail: _ => (s.Problem, s.Mesh, s.Policy)),
            relax: static s => (s.Problem, s.Mesh, s.Policy with { Tolerance = s.Policy.Tolerance * s.Recovery.RelaxFactor, MaxIterations = (int)(s.Policy.MaxIterations * s.Recovery.IterationGrowth) }),
            reorderDofs: static s => Reordered(s.Problem, s.Mesh, s.Policy, s.Clocks),
            switchMethod: static s => (s.Problem, s.Mesh, s.Policy with { Method = s.Recovery.Fallback }),
            restart: static s => (s.Problem, s.Mesh, s.Policy with { Method = s.Recovery.Fallback, MaxIterations = s.Policy.MaxIterations * 2 }));

    static (SolveProblem Problem, DiscreteMesh Mesh, SolvePolicy Policy) Reordered(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, ClockPolicy clocks) {
        var (rows, cols, vals) = Triplets(mesh, problem, policy);
        var coords = new CoordinateStorage<double>(checked((int)mesh.NodeCount), checked((int)mesh.NodeCount), vals.Length);
        for (int entry = 0; entry < vals.Length; entry++) { coords.At(rows[entry], cols[entry], vals[entry]); }
        var csc = CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
        int[] permutation = AMD.Generate(csc, ColumnOrdering.MinimumDegreeAtPlusA);
        return (problem, Renumbered(mesh, permutation, clocks), policy);
    }

    static DiscreteMesh Renumbered(DiscreteMesh mesh, int[] permutation, ClockPolicy clocks) {
        int nodes = checked((int)mesh.NodeCount);
        if (permutation.Length < nodes) { return mesh; }
        int[] inverse = new int[nodes];
        for (int slot = 0; slot < nodes; slot++) { inverse[permutation[slot]] = slot; }
        var reordered = Tensor.CreateFromShape<float>([mesh.NodeCount, 3]);
        var source = mesh.Nodes.AsSpan();
        var sink = reordered.AsSpan();
        for (int old = 0; old < nodes; old++) {
            int fresh = inverse[old];
            sink[fresh * 3] = source[old * 3]; sink[fresh * 3 + 1] = source[old * 3 + 1]; sink[fresh * 3 + 2] = source[old * 3 + 2];
        }
        var renumberedConn = Tensor.CreateFromShape<long>([mesh.ElementCount, mesh.Element.Nodes]);
        var conn = mesh.Connectivity.AsSpan();
        var freshConn = renumberedConn.AsSpan();
        for (int entry = 0; entry < conn.Length; entry++) { freshConn[entry] = inverse[(int)conn[entry]]; }
        return mesh with { Nodes = reordered, Connectivity = renumberedConn, At = clocks.Now };
    }

    static double[] RefinementError(DiscreteMesh mesh) {
        double[] error = new double[checked((int)mesh.ElementCount)];
        for (long cell = 0; cell < error.Length; cell++) { error[cell] = 1.0 - Math.Abs(mesh.Element.Metric(MeshMetric.ScaledJacobian, mesh.NodalXyz(cell))); }
        return error;
    }

    static double Residual(Fin<SolveResult> result) => result.Match(Succ: static r => r.Residual, Fail: static _ => double.MaxValue);

    public static ComputeReceipt.Solve Receipt(SolveResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Problem.Physics.Key, result.Method.Key, result.Dofs, result.Iterations, result.Residual, result.Converged) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    static Fin<SparseCompressedRowMatrixStorage<double>> Assemble(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy) {
        var (rows, cols, vals) = Triplets(mesh, problem, policy);
        return SparseOps.Ingest(SparseFormat.Coo, checked((int)mesh.NodeCount), checked((int)mesh.NodeCount), rows, cols, vals);
    }

    static (int[] Rows, int[] Cols, double[] Vals) Triplets(DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy) {
        int per = mesh.Element.Nodes, cells = checked((int)mesh.ElementCount), entries = cells * per * per;
        double[] material = problem.Physics.Material(problem.MaterialScale, problem.MaterialShift);
        var assembly = new CellAssembly(mesh, per, material, new int[entries], new int[entries], new double[entries]);
        ParallelHelper.For(0, cells, in assembly);
        return (assembly.Rows, assembly.Cols, assembly.Vals);
    }

    readonly struct CellAssembly(DiscreteMesh mesh, int per, double[] material, int[] rows, int[] cols, double[] vals) : IAction {
        public int[] Rows => rows;
        public int[] Cols => cols;
        public double[] Vals => vals;

        public void Invoke(int cell) {
            var conn = mesh.Connectivity.AsSpan();
            using SpanOwner<double> block = SpanOwner<double>.Allocate(per * per, AllocationMode.Clear);
            LocalStiffness(mesh.Element, mesh.Element.Quadrature.Points, mesh.NodalXyz(cell), material, per, block.Span);
            ReadOnlySpan2D<double> local = new(block.DangerousGetArray().Array!, per, per);
            int t = cell * per * per;
            for (int a = 0; a < per; a++)
                for (int b = 0; b < per; b++, t++) {
                    rows[t] = (int)conn[cell * per + a];
                    cols[t] = (int)conn[cell * per + b];
                    vals[t] = local[a, b];
                }
        }
    }

    static void LocalStiffness(ElementClass element, ImmutableArray<(double X, double Y, double Z, double Weight)> quadrature, ReadOnlySpan<double> xyz, double[] material, int per, Span<double> local) {
        double diag = material.Length >= 1 ? material[0] : 1.0;
        foreach (var gauss in quadrature) {
            double[] grad = element.ShapeGrad((gauss.X, gauss.Y, gauss.Z), xyz);
            double weight = gauss.Weight * Math.Max(1e-12, CellVolume(xyz, per));
            for (int a = 0; a < per; a++)
                for (int b = 0; b < per; b++) {
                    double dot = grad[a * 3] * grad[b * 3] + grad[a * 3 + 1] * grad[b * 3 + 1] + grad[a * 3 + 2] * grad[b * 3 + 2];
                    local[a * per + b] += diag * dot * weight;
                }
        }
    }

    static double CellVolume(ReadOnlySpan<double> xyz, int per) {
        if (per < 4) { return 1.0; }
        Vector3 o = new((float)xyz[0], (float)xyz[1], (float)xyz[2]);
        Vector3 a = new((float)xyz[3], (float)xyz[4], (float)xyz[5]);
        Vector3 b = new((float)xyz[6], (float)xyz[7], (float)xyz[8]);
        Vector3 c = new((float)xyz[9], (float)xyz[10], (float)xyz[11]);
        return Math.Abs(Vector3.Dot(Vector3.Cross(a - o, b - o), c - o)) / 6.0;
    }

    static Fin<ConstrainedSystem> Constrained(SparseCompressedRowMatrixStorage<double> operatorCsr, Seq<BoundaryCondition> conditions, SolvePolicy policy) =>
        conditions.Fold(Fin.Succ(new ConstrainedSystem(operatorCsr, new double[operatorCsr.RowCount], LanguageExt.HashSet<long>(), policy.PenaltyFactor)),
            (acc, condition) => acc.Map(system => condition.Apply(system, policy.Constraint)));

    static Fin<SolveResult> Direct(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, Instant at) =>
        SparseOps.Factor(system.Operator, policy.Method.Kind == FactorizationKind.Cholesky ? FactorKind.Spd : FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => factored.Solve(system.Rhs, policy.Tolerance * 1e3))
            .Map(field => new SolveResult(problem, policy.Method, field.AsMemory(), None, None, system.Rhs.Length, 1, 1, 0.0, true, at));

    static IterationPolicy Iteration(SolvePolicy policy) =>
        IterationPolicy.Default with { Tolerance = policy.Tolerance, MaxIterations = policy.MaxIterations, Preconditioner = policy.Method.Preconditioner.Build };

    static Fin<SolveResult> Iterative(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, Instant at) =>
        SparseOps.SolveIterative(system.Operator, policy.Method.Krylov, system.Rhs, Iteration(policy))
            .Bind(run => run.Terminal is SolveTerminal.Admitted
                ? Fin.Succ(new SolveResult(problem, policy.Method, run.Field.ToArray().AsMemory(), None, None, system.Rhs.Length, 0, 1, run.Residual, true, at))
                : Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<solve-diverged:{policy.Method.Key}:residual={run.Residual:e3}>")));

    static Fin<SolveResult> March(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at) {
        ReadOnlySpan<double> stiffness = system.Operator.Values;
        double[] mass = new double[stiffness.Length], damping = new double[stiffness.Length];
        for (int i = 0; i < stiffness.Length; i++) { mass[i] = stiffness[i] * 0.0 + 1.0; damping[i] = 0.05 * stiffness[i]; }
        double[] effective = policy.Integrator.Effective(mass, damping, stiffness, policy.TimeStep);
        var effectiveCsr = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            system.Operator.RowCount, system.Operator.RowCount, effective.Length, system.Operator.RowPointers, system.Operator.ColumnIndices, effective);
        return SparseOps.Factor(effectiveCsr, FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => toSeq(Enumerable.Range(0, policy.TimeSteps))
                .Fold(Fin.Succ(new double[system.Rhs.Length]), (acc, step) => acc.Bind(state => factored.Solve(Step(system.Rhs, state, policy.TimeStep), policy.Tolerance * 1e3)))
                .Map(field => new SolveResult(problem, policy.Method, field.AsMemory(), None, None, system.Rhs.Length, policy.TimeSteps, 1, 0.0, true, at)));
    }

    static double[] Step(double[] forcing, double[] prior, double dt) {
        double[] rhs = new double[forcing.Length];
        for (int i = 0; i < rhs.Length; i++) { rhs[i] = forcing[i] + prior[i] / dt; }
        return rhs;
    }

    static Fin<SolveResult> Newton(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at) {
        SparseMatrix tangent = SparseMatrix.OfStorage(system.Operator);
        return toSeq(Enumerable.Range(0, policy.NewtonIterations))
            .Fold(Fin.Succ((Field: new double[system.Rhs.Length], Residual: double.MaxValue, Step: 0, Converged: false)),
                (acc, _) => acc.Bind(state => state.Converged
                    ? Fin.Succ(state)
                    : SparseOps.SolveIterative(system.Operator, policy.Method.Krylov, NewtonResidual(tangent, system.Rhs, state.Field), Iteration(policy))
                        .Map(run => {
                            double alpha = ArmijoLineSearch(tangent, system.Rhs, state.Field, run.Field, policy.Tolerance);
                            double[] updated = new double[state.Field.Length];
                            TensorPrimitives.MultiplyAdd(run.Field, alpha, state.Field, updated);
                            return (updated, run.Residual, state.Step + 1, run.Residual <= policy.Tolerance);
                        })))
            .Map(state => new SolveResult(problem, policy.Method, state.Field.AsMemory(), None, None, system.Rhs.Length, state.Step, state.Step, state.Residual, state.Converged, at));
    }

    static double[] NewtonResidual(SparseMatrix tangent, double[] rhs, double[] field) {
        double[] residual = (double[])rhs.Clone();
        var ax = tangent.Multiply(Vector<double>.Build.DenseOfArray(field)).AsArray();
        TensorPrimitives.Subtract(residual, ax, residual);
        return residual;
    }

    static double ArmijoLineSearch(SparseMatrix tangent, double[] rhs, double[] field, double[] direction, double tol) {
        double baseline = TensorPrimitives.Norm<double>(NewtonResidual(tangent, rhs, field));
        double alpha = 1.0;
        double[] trial = new double[field.Length];
        for (int backtrack = 0; backtrack < 8; backtrack++) {
            TensorPrimitives.MultiplyAdd(direction, alpha, field, trial);
            if (TensorPrimitives.Norm<double>(NewtonResidual(tangent, rhs, trial)) <= (1.0 - 1e-4 * alpha) * baseline + tol) { return alpha; }
            alpha *= 0.5;
        }
        return alpha;
    }

    static Fin<SolveResult> Modal(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, Instant at) =>
        DenseOps.Decompose(system.Dense(), FactorizationKind.Evd)
            .Bind(factorization => EigenPairs(factorization, policy.EigenPairs))
            .Map(pairs => new SolveResult(problem, policy.Method, pairs.Vectors, Some(pairs.Values), Some(pairs.Participation), system.Rhs.Length, 1, 1, 0.0, true, at));

    static Fin<(ReadOnlyMemory<double> Vectors, ReadOnlyMemory<double> Values, ReadOnlyMemory<double> Participation)> EigenPairs(Factorization factorization, int pairs) =>
        factorization is Factorization.Evd { Decomposition: var evd }
            ? Fin.Succ((
                evd.EigenVectors.SubMatrix(0, evd.EigenVectors.RowCount, 0, Math.Min(pairs, evd.EigenVectors.ColumnCount)).ToColumnMajorArray().AsMemory(),
                evd.EigenValues.Take(pairs).Select(static c => c.Real).ToArray().AsMemory(),
                Participation(evd, pairs)))
            : Fin.Fail<(ReadOnlyMemory<double>, ReadOnlyMemory<double>, ReadOnlyMemory<double>)>(ComputeFault.Create("<modal-non-evd>"));

    static ReadOnlyMemory<double> Participation(Evd<double> evd, int pairs) {
        int modes = Math.Min(pairs, evd.EigenVectors.ColumnCount);
        double[] factors = new double[modes];
        var unit = Vector<double>.Build.Dense(evd.EigenVectors.RowCount, 1.0);
        for (int mode = 0; mode < modes; mode++) {
            var phi = evd.EigenVectors.Column(mode);
            double numerator = phi.DotProduct(unit);
            factors[mode] = numerator * numerator / Math.Max(1e-12, phi.DotProduct(phi));
        }
        return factors.AsMemory();
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class CouplingScheme {
    public static readonly CouplingScheme OneWay = new("one-way", iterates: false, relaxes: false);
    public static readonly CouplingScheme TwoWay = new("two-way", iterates: true, relaxes: false);
    public static readonly CouplingScheme Staggered = new("staggered", iterates: true, relaxes: true);

    public bool Iterates { get; }
    public bool Relaxes { get; }
}

public sealed record FieldTransfer(int From, int To, FieldStation Source, FieldStation Target, double[] Map) {
    public BoundaryCondition Lower(ReadOnlyMemory<double> donor) {
        long[] nodes = new long[Map.Length];
        double[] values = new double[Map.Length];
        for (int i = 0; i < Map.Length; i++) { nodes[i] = i; values[i] = Map[i] * (i < donor.Length ? donor.Span[i] : 0.0); }
        return new BoundaryCondition.Dirichlet(Target, nodes, values);
    }
}

public sealed record CouplingPolicy(CouplingScheme Scheme, int MaxRounds, double Tolerance, double Relaxation, bool Aitken) {
    public static readonly CouplingPolicy ThermalStructural = new(CouplingScheme.Staggered, MaxRounds: 50, Tolerance: 1e-6, Relaxation: 0.5, Aitken: true);
    public static readonly CouplingPolicy FluidStructure = new(CouplingScheme.TwoWay, MaxRounds: 100, Tolerance: 1e-5, Relaxation: 0.3, Aitken: true);
}

public sealed record CoupledProblem(Seq<SolveProblem> Fields, Seq<FieldTransfer> Transfers, CouplingPolicy Policy) {
    public bool WellPosed => Fields.Count >= 2 && Transfers.ForAll(t => t.From < Fields.Count && t.To < Fields.Count);
}

public sealed record CoupledResult(Seq<SolveResult> Fields, int Rounds, double CouplingResidual, Seq<double> AitkenHistory, bool Converged, Instant At);

public static class CoupledLane {
    public static Fin<CoupledResult> Couple(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, CorrelationId correlation, ClockPolicy clocks) =>
        !coupling.WellPosed
            ? Fin.Fail<CoupledResult>(ComputeFault.Create($"<coupling-ill-posed:fields={coupling.Fields.Count}>"))
            : coupling.Policy.Scheme.Iterates
                ? Iterate(coupling, meshes, policy, clocks)
                : OneShot(coupling, meshes, policy, clocks);

    public static ComputeReceipt.Coupling Receipt(CoupledProblem coupling, CoupledResult result, CorrelationId correlation, Duration elapsed) =>
        new(coupling.Policy.Scheme.Key, coupling.Fields.Count, coupling.Transfers.Count, result.Rounds, result.CouplingResidual, result.Converged) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    static Fin<CoupledResult> OneShot(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, ClockPolicy clocks) =>
        SolveRound(coupling, meshes, policy, Seq<SolveResult>(), clocks)
            .Map(fields => new CoupledResult(fields, 1, 0.0, Seq<double>(), true, clocks.Now));

    static Fin<CoupledResult> Iterate(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, ClockPolicy clocks) =>
        toSeq(Enumerable.Range(0, coupling.Policy.MaxRounds))
            .Fold(SolveRound(coupling, meshes, policy, Seq<SolveResult>(), clocks).Map(fields => (Fields: fields, Residual: double.MaxValue, Omega: coupling.Policy.Relaxation, PriorDelta: Seq<double>(), History: Seq<double>(), Converged: false)),
                (acc, _) => acc.Bind(state => state.Converged
                    ? Fin.Succ(state)
                    : SolveRound(coupling, meshes, policy, state.Fields, clocks).Map(next => {
                        var delta = Delta(state.Fields, next);
                        double residual = Math.Sqrt(delta.Sum(d => d * d));
                        double omega = coupling.Policy.Aitken ? Aitken(state.Omega, state.PriorDelta, delta) : coupling.Policy.Relaxation;
                        return (Relax(state.Fields, next, omega), residual, omega, delta, state.History.Add(omega), residual <= coupling.Policy.Tolerance);
                    })))
            .Map(state => new CoupledResult(state.Fields, coupling.Policy.MaxRounds, state.Residual, state.History, state.Converged, clocks.Now));

    static Fin<Seq<SolveResult>> SolveRound(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, Seq<SolveResult> prior, ClockPolicy clocks) =>
        toSeq(Enumerable.Range(0, coupling.Fields.Count)).Fold(Fin.Succ(Seq<SolveResult>()), (acc, index) =>
            acc.Bind(solved => {
                SolveProblem field = coupling.Fields[index];
                Seq<BoundaryCondition> injected = coupling.Transfers
                    .Filter(t => t.To == index && t.From < prior.Count)
                    .Map(t => t.Lower(prior[t.From].Field));
                SolveProblem stamped = field with { Conditions = field.Conditions + injected };
                return SolveLane.Solve(stamped, meshes[index], policy, default, clocks).Map(result => solved.Add(result));
            }));

    static Seq<double> Delta(Seq<SolveResult> previous, Seq<SolveResult> current) =>
        previous.Count != current.Count
            ? Seq1(double.MaxValue)
            : toSeq(Enumerable.Range(0, current.Count)).Bind(field => {
                ReadOnlySpan<double> a = previous[field].Field.Span, b = current[field].Field.Span;
                var diffs = new List<double>(b.Length);
                for (int i = 0; i < a.Length && i < b.Length; i++) { diffs.Add(b[i] - a[i]); }
                return toSeq(diffs);
            });

    static double Aitken(double priorOmega, Seq<double> priorDelta, Seq<double> delta) {
        if (priorDelta.Count != delta.Count || priorDelta.IsEmpty) { return priorOmega; }
        double dotDiff = 0.0, normDiff = 0.0;
        for (int i = 0; i < delta.Count; i++) { double dr = delta[i] - priorDelta[i]; dotDiff += priorDelta[i] * dr; normDiff += dr * dr; }
        return normDiff > 1e-12 ? Math.Clamp(-priorOmega * dotDiff / normDiff, 0.05, 1.0) : priorOmega;
    }

    static Seq<SolveResult> Relax(Seq<SolveResult> previous, Seq<SolveResult> current, double omega) =>
        previous.Count != current.Count
            ? current
            : toSeq(Enumerable.Range(0, current.Count)).Map(field => {
                ReadOnlySpan<double> a = previous[field].Field.Span, b = current[field].Field.Span;
                double[] blended = new double[b.Length];
                int shared = Math.Min(a.Length, b.Length);
                TensorPrimitives.Lerp(a[..shared], b[..shared], omega, blended.AsSpan(0, shared));
                b[shared..].CopyTo(blended.AsSpan(shared));
                return current[field] with { Field = blended.AsMemory() };
            });
}
```

## [3]-[RESEARCH]

- [ASSEMBLY_KERNELS]: the per-`PhysicsKind` element assembly (`SolveLane.Triplets`/`LocalStiffness` over the `ElementClass.ShapeGrad` delegate and the `QuadratureRule.Tet4`/`Hex27` Gauss tables from `Solver/discretization#DISCRETIZATION_MESH`, the `Bᵀ·D·B` stiffness against the `PhysicsKind.Material` `D`-matrix, and the COO handoff to `Tensor/factor#SPARSE_SOLVE`) is authored in the cluster fences; the open leaf is the `Rasm`/Vectors boundary-extraction member spelling the discretization owner names for the `MeshSpace.Encloses` inclusion test.
- [GRADIENT_ADJOINT]: the gradient-adjoint route is settled — the tensor-lane DDG-operator VJP bodies (`Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryAdjoint`/`Backward.Operator`) compose the `Rasm`/Vectors `Spectral.cs` `OperatorRow.Adjoint` over the live `DiscreteCalculus` assembly through the linear-operator transpose law, the six geometry rows land on the `Tensor/vocabulary#OPERATION_TABLE` axis under `TensorOpKind.Geometry`, and the `Solver/optimizer#OPTIMIZER_LANE` `DesignProblem.OperatorRows` lowers the shape/topology design fields to a snapshot-bearing `Seq<GeometryTape>` the reverse-mode `SensitivityLaw.Chain` consumes; the open leaf is the live design-mesh wiring at the shape-optimization/inverse-design call site that supplies the `MeshAdjointSnapshot` the tape records through `MeshAdjointSnapshot.Of`.
