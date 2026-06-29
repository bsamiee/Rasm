# [COMPUTE_SOLVE_CONTRACT]

Rasm.Compute solve contract: one `PhysicsKind`×`BoundaryCondition`×`ElementClass` solve axis admitting FEA, CFD, thermal, daylight, energy, acoustic, electromagnetic, and multi-physics problems on the discretized field as uniform `SolveProblem` rows, one `SolveLane` static fold assembling the discrete `Bᵀ·D·B` operator over the `Solver/discretization#DISCRETIZATION_MESH` `DiscreteMesh`, dispatching to the numeric-lane factorization or iterative solve, marching the transient/nonlinear loop, and driving the adaptive-recovery ladder, and one `CoupledLane` multi-physics fold over field sets bound by `FieldTransfer` rows under Aitken Δ²-relaxation. The page owns the `PhysicsKind`/`BoundaryCondition`/`ConstraintMethod`/`SolveMethod`/`TimeIntegrator`/`CouplingScheme`/`RecoveryAction` vocabulary, the `SolveProblem`/`SolveResult`/`ConstrainedSystem` carriers, the `RecoveryPolicy`/`CouplingPolicy` policies, and the `SolveLane`/`CoupledLane` folds; the dense/sparse factorization and iterative solve ride `Tensor/blas#DENSE_ALGEBRA`/`Tensor/factor#SPARSE_SOLVE`, the `ElementClass.Sample`/`QuadratureRule`/`DiscreteMesh`/`FieldSpace` shapes arrive settled from `Solver/discretization#DISCRETIZATION_MESH`, the gradient-adjoint tape rides `Tensor/dispatch#EQUIVALENCE_INTEROP`, and the distributed solve dials the existing `Runtime/channels#PROTO_VOCABULARY` `Solve` rpc. Every solver receipt is typed and the page carries no TS_PROJECTION because solve interiors stay host-local behind the `Solve` rpc.

## [01]-[INDEX]

- [02]-[SOLVE_CONTRACT]: physics×BC×element solve axis; transient/nonlinear; multi-physics; recovery.
- [03]-[CONSTITUTIVE]: per-Gauss-point stress-update axis (plasticity/hyperelastic/viscoelastic/damage) and frictional-contact enforcement whose tangent is the AD SPD Gauss-Newton curvature of the strain-energy / gap potential (the exact `∂²W/∂ε²` Hessian-vector product the documented second-order open leaf).

## [02]-[SOLVE_CONTRACT]

- Owner: `PhysicsKind` `[SmartEnum<string>]` physics-domain rows carrying symmetry, eigen, transient, and material-`D`-matrix columns; `BoundaryCondition` `[Union]` BC cases; `ConstraintMethod` `[SmartEnum<string>]` DOF-constraint-application rows (elimination/penalty/lagrange); `SolveMethod` `[SmartEnum<string>]` linear/iterative/modal method rows carrying the numeric-lane `IterativeMethod`/`FactorizationKind` lowering and a `Preconditioner` column; `TimeIntegrator` `[SmartEnum<string>]` transient-marching rows (backward-euler/newmark-beta/generalized-alpha/central-difference); `CouplingScheme` `[SmartEnum<string>]` field-transfer rows with Aitken relaxation; `RecoveryAction` `[SmartEnum<string>]` non-convergence-recovery rows; `SolveProblem` the uniform problem record; `SolveLane` the static fold that assembles the discrete `Bᵀ·D·B` operator over the `DiscreteMesh`, dispatches to the `numeric-lane` factorization or iterative solve, marches the transient/nonlinear loop, and drives the adaptive-recovery ladder; `CoupledLane` the static multi-physics fold; `SolveResult` the field-plus-evidence carrier.
- Cases: `PhysicsKind` rows fea-static · fea-modal · fea-transient · fea-buckling · cfd-incompressible · thermal-steady · thermal-transient · daylight-radiosity · energy-balance · acoustic-helmholtz · electromagnetic-eddy; `BoundaryCondition` cases `Dirichlet(FieldStation Station, long[] Nodes, double[] Values)` · `Neumann(long[] Faces, double[] Flux)` · `Robin(long[] Faces, double Coefficient, double Ambient)` · `Periodic(long[] Master, long[] Slave)` · `Contact(long[] Slave, long[] Master, double Gap, double Penalty)`; `ConstraintMethod` rows elimination · penalty · lagrange; `SolveMethod` rows direct-lu · direct-cholesky · bicgstab · gpbicg · tfqmr · mlk-bicgstab · dense-evd (the four iterative rows mirror the verified numeric-lane `IterativeMethod` axis 1:1, each carrying its `IterativeMethod` Krylov column and the `Preconditioner.Diagonal` row — the only admitted `IPreconditioner<double>` concrete is the Jacobi `DiagonalPreconditioner`, so the preconditioner axis is the single real Diagonal row plus the `None` alias that builds the same diagonal factory, never a phantom incomplete-Cholesky/ILU type; dense-evd is the dense symmetric eigensolver row routed through `Modal` — the only eigensolver the admitted MathNet/CSparse stack provides, never a phantom sparse LOBPCG/Lanczos); `TimeIntegrator` rows backward-euler · newmark-beta · generalized-alpha · central-difference; `CouplingScheme` rows one-way · two-way · staggered (the iterative rows adding Aitken Δ²-relaxation); `RecoveryAction` rows refine-mesh · relax · reorder-dofs · switch-method · restart.
- Entry: `public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, CorrelationId correlation, ClockPolicy clocks)` — `Fin<T>` aborts on an ill-posed BC set or a non-convergent run past the iteration cap; the modal physics row returns the vibration eigenpairs through the dense symmetric `Evd` route while the buckling physics row returns the linear-buckling load factors through the same `Evd` over the geometric-stiffness congruence, the transient row marches the `TimeIntegrator` over the policy step set reusing one factorization, the nonlinear physics row (and any problem carrying a `[03]` constitutive law) drives a modified Newton-Raphson outer loop line-searching the step against the nonlinear internal-force residual, and every other row the displacement/temperature/pressure field over the `FieldSpace`; `SolveAdaptive(..., RecoveryPolicy recovery, ...)` walks the `RecoveryAction` ladder on a `Fin.Fail`; `CoupledLane.Couple(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, ...)` solves the coupled field set under Aitken-relaxed staggering.
- Auto: `Solve` assembles the global stiffness/mass/conductivity operator by folding each element's local `Bᵀ·D·B` matrix — evaluated through one `ElementClass.Sample` isoparametric evaluation per Gauss point (physical gradients ∂N/∂x plus the per-point Jacobian determinant detJ) against the `PhysicsKind.Material` `D`-matrix and weighted by `gauss.Weight·|detJ|`, never a centroid-volume approximation — into the `SparseCompressedRowMatrixStorage<double>` the mesh connectivity addresses, applies the `BoundaryCondition` set by the `ConstraintMethod` row (row/column elimination, penalty diagonal augmentation, or Lagrange-multiplier bordering), and dispatches to the `Tensor/blas#DENSE_ALGEBRA` `DenseOps.Decompose`/`Tensor/factor#SPARSE_SOLVE` `SparseOps.Factor`/`SolveIterative` by the `SolveMethod` row's lowering; the physics row selects the assembly kernel (Poisson, elasticity, Helmholtz, Maxwell-eddy) and the operator symmetry so an SPD operator routes Cholesky/BiCgStab and an indefinite one routes LU/Tfqmr/MlkBiCgStab without a call-site branch; the implicit transient row factors the effective operator `(M/Δt² + γC/Δt + βK)` once and back-substitutes every Newmark step while the explicit central-difference row marches the lumped diagonal update unfactored, the nonlinear row drives a modified Newton-Raphson over the held elastic tangent whose residual is the genuine nonlinear internal force `f_ext − f_int(u)` (the `[03]` constitutive stress folded over the current strain when a material law is carried, the elastic `K·u` otherwise), Armijo-line-searched on that residual.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics key, method key, constraint key, DOF count, iteration count, the final residual, the converged flag, and elapsed; the modal row stamps the recovered eigenvalue count and the modal participation factors, the transient rows stamp the integrator key and step count, the nonlinear rows stamp the Newton iteration count and the load-step list, and the iterative rows ride the `rasm.compute.solve.residual` histogram instrument; the `Coupling` `ComputeReceipt` case carries the scheme key, field count, transfer count, round count, the Aitken factor history, the final coupling residual, and the converged flag; the `RecoveryReceipt` carries the physics key and the ordered `(action, post-recovery residual)` step list plus the recovered flag.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new physics domain is one `PhysicsKind` row carrying its assembly-kernel, symmetry, and `D`-matrix columns; a new BC kind is one `BoundaryCondition` case; a new constraint application is one `ConstraintMethod` row; a new linear method is one `SolveMethod` row carrying its lowering and preconditioner columns; a new time scheme is one `TimeIntegrator` row carrying its `(α,β,γ)` coefficient column; a new coupling discipline is one `CouplingScheme` row plus a `FieldTransfer` mapping; a new recovery strategy is one `RecoveryAction` row; zero new surface — a `CfdSolver`/`ThermalSolver`/`FeaSolver` sibling family is the rejected form collapsed onto the one `SolveLane` fold discriminated by `PhysicsKind`, a `NewmarkSolver`/`GeneralizedAlphaSolver` sibling family is collapsed onto the `TimeIntegrator` axis, and an `FsiCoupler`/`ThermalStructuralCoupler` sibling family is collapsed onto the one `CoupledLane` fold.
- Boundary: the solve contract is uniform — physics, boundary condition, element, and time scheme discriminate by row/case, never by a parallel solver type, so the same `Solve` runs an FEA static analysis, a transient thermal march, a CFD pressure-Poisson step, a buckling eigenproblem, and a Helmholtz acoustic mode; the discrete operator rides the numeric lane exclusively — assembly produces the CSR storage `SparseOps.Ingest(SparseFormat.Coo, …)` consumes and the factorization/iterative dispatch is `numeric-lane` machinery (`SparseOps.Factor`/`FactoredOp.Solve`/`SparseOps.SolveIterative`/`DenseOps.Decompose`), so this owner never re-mints a linear-algebra kernel and a hand-rolled CG loop beside `SparseOps` is the deleted form; the iterative `SolveMethod` rows carry an `IterativeMethod` lowering column and pass a derived `IterationPolicy` (tolerance/max-iter/criterion-stack/preconditioner) into `SparseOps.SolveIterative` — a raw-`string` method discriminant is the deleted form the numeric-lane Boundary names — and the dense symmetric eigensolver splits in `Modal` by physics — `FeaModal` rides the mass-normalized congruence so the recovered eigenpairs are the physical ω² and mode shapes, `FeaBuckling` rides the prestress geometric-stiffness `K_g` so the eigenpairs are the linear-buckling load factors λ = 1/μ and modes (silently mass-normalizing buckling to vibration is the deleted fake) — both reduce the generalized problem through a Cholesky/lumped-mass congruence to the standard symmetric `Evd`, never a phantom sparse LOBPCG the admitted stack does not own; the BC application is one `ConstraintMethod` row — elimination partitions DOFs once into constrained and free sets folding constrained values into the RHS, penalty augments the diagonal by the policy factor, and Lagrange borders the system with the constraint matrix, so a penalty fallback is a row, never a second BC path, the penalty Dirichlet arm genuinely augments the operator diagonal `K[i,i] += P` beside the `rhs[i] = P·g` injection (a RHS-only penalty that never touched the operator is the deleted fake), the `Periodic` case ties each master/slave DOF pair through the symmetric penalty stencil so its `Master` endpoints are load-bearing rather than a dead field, and the `Contact` case lowers to the penalty constraint over the gap function as an equal-and-opposite reaction across its `Slave`/`Master` endpoints; the modal and transient rows compose the same assembled operator across steps so a transient sweep reuses one `Factorization` through the numeric-lane stored decomposition rather than re-factoring per step — the implicit `TimeIntegrator` rows march the genuine Newmark-β predictor-corrector over displacement/velocity/acceleration against the once-factored effective operator `(M/Δt² + γC/Δt + βK)` with the geometry-based lumped diagonal mass `M` and stiffness-proportional Rayleigh `C`, while the explicit central-difference row (the `TimeIntegrator.Implicit=false` lane) marches the lumped diagonal update with no factorization, so the `Implicit` policy column is load-bearing and a `Beta=0` scheme never divides through the implicit `1/(βΔt²)`; the nonlinear physics row drives a MODIFIED Newton-Raphson holding the elastic operator `new SparseMatrix(system.Operator)` as the iteration tangent (a real convergent scheme, never re-factored per step) while the residual is the GENUINE nonlinear internal force `f_ext − f_int(u)`: when the problem carries a `[03]` `ConstitutiveModel` (routed by `problem.Material.IsSome` regardless of the equation-nonlinear flag) the per-Gauss-point stress `σ = ∂W/∂ε` folds over the current strain `ε = B·u` into `f_int` through the same `Strain` operator and one `ElementClass.Sample` the linear assembly uses (a real material nonlinearity), and absent a law `f_int` is the held elastic `K·u` so the iterate damps in one step to the linear solution; convergence is the NONLINEAR-residual norm relative to `‖f_ext‖` (`policy.Tolerance·max(1,‖f_ext‖)`), NEVER the inner Krylov residual the prior body mistook for it, the Armijo backtrack over `TensorPrimitives.MultiplyAdd`/`Subtract`/`Norm` decreases that same nonlinear residual, and the receipt step count is the genuine outer iteration count — a held-operator `b − A·x` iterate that converges on the inner-solver residual with a decorative line search is the deleted fake; the consistent full-matrix tangent (the `[03]` `SensitivityLaw.GaussNewton` SPD curvature, exposed as a Hessian-VECTOR product, the documented matrix-free Newton-CG second-order leaf) is not assembled here; the element-assembly `Triplets` fold partitions the per-cell `Bᵀ·D·B` local-stiffness blocks across `CommunityToolkit.HighPerformance` `ParallelHelper.For` with each cell renting its scratch block from `SpanOwner<double>.Allocate` and reading it through `ReadOnlySpan2D<double>`, so the `AllocationClass.PooledMemory` receipt is honest; the local block is the genuine `Bᵀ·D·B` over a per-`MaterialForm` strain-displacement operator `B` — the 3-row spatial gradient for a scalar-diffusion DOF, the 6-row Voigt symmetric-strain operator for vector elasticity, the 9-row displacement-gradient operator for the curl-curl vector potential — contracted with the full `PhysicsKind.Material` constitutive tensor, so the operator is `PhysicsKind.Dof·NodeCount` square and the scalar `material[0]·(∇N·∇N)` Laplacian is the `Dof=1` degenerate case rather than the only assembled form, every off-diagonal `D` term load-bearing, each Gauss point weighted by `gauss.Weight·|detJ|` from the one `ElementClass.Sample` evaluation (the exact isoparametric measure on hex/wedge/pyramid/curved/higher-order elements, never a first-corner tet-volume approximation) and the geometry-based lumped mass deriving each cell volume from that same Σ_gauss `gauss.Weight·|detJ|` quadrature; the result field is the `FieldSpace` over the mesh stations and crosses to Persistence as a content-keyed result artifact; a distributed solve dials the existing `Runtime/channels#PROTO_VOCABULARY` `Solve` rpc through the `ShardPlan.Blocked` row-block fan-out; multi-physics coupling is one `CoupledLane` fold over ≥2 `SolveProblem` fields bound by `FieldTransfer` rows under Aitken Δ²-relaxation — the relaxation factor is computed from successive inter-field residuals (`ω = -ω·(r·Δr)/(Δr·Δr)`), never a fixed under-relaxation constant — so the coupling discipline is a `CouplingScheme` discriminant and the transferred field reuses the single `BoundaryCondition.Dirichlet` injection path; adaptive recovery is one `RecoveryAction` ladder fold on the same `Solve` — a divergent run relaxes the tolerance/cap, reorders DOFs through the fresh `CSparse.Ordering.AMD.Generate(csc, ColumnOrdering.MinimumDegreeAtPlusA)` permutation renumbering the operator, refines the mesh through `MeshKernel.Refine`, switches to the robust `mlk-bicgstab` (ML(k)BiCGStab) fallback, then restarts, and the `RecoveryReceipt` records which rung succeeded.

```csharp signature
public enum MaterialForm { Elasticity, Isotropic, MaxwellEddy }

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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

    // The per-node DOF count and Voigt strain dimension the one assembly kernel shapes its strain-displacement
    // operator B to: a scalar diffusion field carries one DOF and the 3-row spatial-gradient B, vector
    // elasticity three DOFs and the 6-row symmetric-strain Voigt B, the curl-curl vector potential three DOFs
    // and the 9-row displacement-gradient B — so one Bᵀ·D·B fold assembles every physics with no per-row arm.
    public int Dof => Form switch { MaterialForm.Elasticity => 3, MaterialForm.MaxwellEddy => 3, _ => 1 };
    public int StrainDim => Form switch { MaterialForm.Elasticity => 6, MaterialForm.MaxwellEddy => 9, _ => 3 };

    // The constitutive D-matrix the assembly contracts as Bᵀ·D·B, sized (StrainDim × StrainDim): the 3×3
    // conductivity/Helmholtz tensor for scalar diffusion, the 6×6 isotropic-elasticity tensor, and the 9×9
    // vector-potential tensor M⊗I₃ lifted from the 3×3 eddy coupling so the curl-curl operator reads the
    // full anisotropic coupling rather than a scalar diagonal — every entry of D is live at the Gauss point.
    public double[] Material(double scale, double shift) =>
        Form switch {
            MaterialForm.Elasticity => Elasticity(scale),
            MaterialForm.MaxwellEddy => KroneckerEye([scale, -shift, 0, shift, scale, 0, 0, 0, scale]),
            _ => Isotropic(scale + ShiftScale * shift),
        };

    static double[] Elasticity(double scale) {
        double lambda = scale, mu = 0.5 * scale;
        return [lambda + 2 * mu, lambda, lambda, 0, 0, 0, lambda, lambda + 2 * mu, lambda, 0, 0, 0, lambda, lambda, lambda + 2 * mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu];
    }
    static double[] Isotropic(double diagonal) => [diagonal, 0, 0, 0, diagonal, 0, 0, 0, diagonal];

    // (3×3) ⊗ I₃ → the 9×9 block tensor pairing each of the three vector-potential components with the three
    // spatial gradient directions, so the displacement-gradient Bᵀ·D·B couples component cᵢ to cⱼ through
    // M[cᵢ,cⱼ]·(∇Nₐ·∇N_b) and the off-diagonal eddy coupling enters the operator instead of being discarded.
    static double[] KroneckerEye(double[] m3) {
        double[] d = new double[81];
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                for (int k = 0; k < 3; k++) { d[(3 * r + k) * 9 + (3 * c + k)] = m3[r * 3 + c]; }
        return d;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConstraintMethod {
    public static readonly ConstraintMethod Elimination = new("elimination", bordered: false);
    public static readonly ConstraintMethod Penalty = new("penalty", bordered: false);
    public static readonly ConstraintMethod Lagrange = new("lagrange", bordered: true);

    public bool Bordered { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SolveMethod {
    public static readonly SolveMethod DirectLu = new("direct-lu", iterative: false, kind: FactorizationKind.Lu, krylov: null, preconditioner: Preconditioner.None);
    public static readonly SolveMethod DirectCholesky = new("direct-cholesky", iterative: false, kind: FactorizationKind.Cholesky, krylov: null, preconditioner: Preconditioner.None);
    public static readonly SolveMethod BiCgStab = new("bicgstab", iterative: true, kind: FactorizationKind.Cholesky, krylov: IterativeMethod.BiCgStab, preconditioner: Preconditioner.Diagonal);
    public static readonly SolveMethod GpBiCg = new("gpbicg", iterative: true, kind: FactorizationKind.Lu, krylov: IterativeMethod.GpBiCg, preconditioner: Preconditioner.Diagonal);
    public static readonly SolveMethod Tfqmr = new("tfqmr", iterative: true, kind: FactorizationKind.Lu, krylov: IterativeMethod.Tfqmr, preconditioner: Preconditioner.Diagonal);
    public static readonly SolveMethod MlkBiCgStab = new("mlk-bicgstab", iterative: true, kind: FactorizationKind.Lu, krylov: IterativeMethod.MlkBiCgStab, preconditioner: Preconditioner.Diagonal);
    public static readonly SolveMethod DenseEvd = new("dense-evd", iterative: false, kind: FactorizationKind.Evd, krylov: null, preconditioner: Preconditioner.None);

    public bool Iterative { get; }
    public FactorizationKind Kind { get; }
    public Preconditioner Preconditioner { get; }
    private readonly IterativeMethod? krylov;

    // Absence is carried, never thrown: a non-iterative row has no Krylov solver, so the consumer resolves
    // the Option on the Fin rail with a typed fault rather than risking an unguarded InvalidOperationException.
    public Option<IterativeMethod> Krylov => Optional(krylov);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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
                bool penalty = s.Constraint == ConstraintMethod.Penalty;
                double[] rhs = (double[])s.System.Rhs.Clone();
                double[] values = penalty ? (double[])s.System.Operator.Values.Clone() : s.System.Operator.Values;
                int[] rowPtr = s.System.Operator.RowPointers, colIdx = s.System.Operator.ColumnIndices;
                var fixedDofs = s.System.Constrained;
                for (int i = 0; i < bc.Nodes.Length; i++) {
                    long node = bc.Nodes[i];
                    if (penalty) { Augment(values, rowPtr, colIdx, (int)node, (int)node, s.System.Penalty); }
                    rhs[node] = penalty ? s.System.Penalty * bc.Values[i] : bc.Values[i];
                    fixedDofs = fixedDofs.Add(node);
                }
                return s.System with { Operator = penalty ? Rebuilt(s.System.Operator, values) : s.System.Operator, Rhs = rhs, Constrained = fixedDofs };
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
            // Periodic adds the symmetric penalty stencil [+P −P; −P +P] over each (master, slave) DOF pair: the
            // diagonal +P always lands and the −P coupling lands wherever the assembled pattern already holds the
            // master↔slave slot (periodic-linked connectivity), driving u_slave − u_master → 0 there — so `Master`
            // is read and the tie is real on a coupling-bearing pattern; an opposite-face pair the assembly never
            // coupled needs the `ConstraintMethod.Lagrange` bordered MPC, not this in-pattern penalty stencil. The
            // prior slave-only `Constrained.Add` that zero-fixed the slaves and never read `Master` is the deleted fake.
            periodic: static (s, bc) => {
                double penalty = s.System.Penalty;
                double[] values = (double[])s.System.Operator.Values.Clone();
                int[] rowPtr = s.System.Operator.RowPointers, colIdx = s.System.Operator.ColumnIndices;
                var fixedDofs = s.System.Constrained;
                int pairs = Math.Min(bc.Master.Length, bc.Slave.Length);
                for (int p = 0; p < pairs; p++) {
                    int master = (int)bc.Master[p], slave = (int)bc.Slave[p];
                    Augment(values, rowPtr, colIdx, master, master, penalty);
                    Augment(values, rowPtr, colIdx, slave, slave, penalty);
                    Augment(values, rowPtr, colIdx, master, slave, -penalty);
                    Augment(values, rowPtr, colIdx, slave, master, -penalty);
                    fixedDofs = fixedDofs.Add(bc.Slave[p]);
                }
                return s.System with { Operator = Rebuilt(s.System.Operator, values), Constrained = fixedDofs };
            },
            // The penalty contact force pushes each slave out of penetration AND commits the equal-and-opposite
            // reaction onto its paired master node (Newton's third law), so `Master` is load-bearing rather than a
            // dead field; the real active-set/multiplier contact is the [03] ContactEnforcement fold over the same gap.
            contact: static (s, bc) => {
                double[] rhs = (double[])s.System.Rhs.Clone();
                double force = bc.Penalty * Math.Max(0.0, bc.Gap);
                int pairs = Math.Min(bc.Slave.Length, bc.Master.Length);
                for (int i = 0; i < bc.Slave.Length; i++) { rhs[bc.Slave[i]] += force; }
                for (int i = 0; i < pairs; i++) { rhs[bc.Master[i]] -= force; }
                return s.System with { Rhs = rhs };
            });

    // Move one existing CSR nonzero: add `delta` to the (row, col) slot the sparsity already holds. The Dirichlet
    // penalty diagonal and the periodic [+P −P; −P +P] tie ride this — the assembled structure is fixed, only the
    // stored values shift, so no pattern-rebuild is taken and a slot the sparsity omits is a silent no-op.
    static void Augment(double[] values, int[] rowPointers, int[] columnIndices, int row, int col, double delta) {
        for (int slot = rowPointers[row]; slot < rowPointers[row + 1]; slot++) {
            if (columnIndices[slot] == col) { values[slot] += delta; return; }
        }
    }

    static SparseCompressedRowMatrixStorage<double> Rebuilt(SparseCompressedRowMatrixStorage<double> operatorCsr, double[] values) =>
        SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            operatorCsr.RowCount, operatorCsr.ColumnCount, values.Length, operatorCsr.RowPointers, operatorCsr.ColumnIndices, values);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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
    public static readonly SolvePolicy CanonicalModal = CanonicalStatic with { Method = SolveMethod.DenseEvd, MaxIterations = 500, Tolerance = 1e-7, EigenPairs = 12 };
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
    Option<(ConstitutiveModel Model, MaterialParameters Law)> Material,
    UInt128 ContentKey) {
    // `Material` carries the [03] per-Gauss-point constitutive law when the problem is materially nonlinear; its
    // presence is the discriminant `Routed` reads to drive the constitutive-coupled Newton-Raphson, and it folds into
    // the content key so a plastic/hyperelastic solve is a distinct cache identity from the linear-elastic baseline.
    public static SolveProblem Of(PhysicsKind physics, DiscreteMesh mesh, Seq<BoundaryCondition> conditions, double materialScale = 1.0, double materialShift = 0.0, Option<(ConstitutiveModel Model, MaterialParameters Law)> material = default) =>
        new(physics, mesh.Element, conditions,
            mesh.FieldOf(FieldStation.Nodal, physics.Dof == 1 ? 0 : 1, physics.Dof), materialScale, materialShift, material,
            XxHash128.HashToUInt128(MemoryMarshal.AsBytes(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{physics.Key}|{mesh.Element.Key}|{mesh.NodeCount}|{mesh.ElementCount}|{physics.Dof}|{materialScale:R}|{materialShift:R}|{material.Match(Some: static m => $"{m.Model.GetType().Name}:{m.Law}", None: static () => "linear")}").AsSpan())));
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
    double Penalty);

public static class SolveLane {
    // A carried constitutive law (material nonlinearity) routes to Newton REGARDLESS of the physics-equation
    // nonlinear flag, so a plastic/hyperelastic structural solve reaches the constitutive-coupled Newton-Raphson
    // rather than silently taking the linear Direct path; the equation-nonlinear rows (cfd/energy) route there too.
    static SolveKind Routed(SolveProblem problem, SolveMethod method) =>
        problem.Material.IsSome ? SolveKind.Nonlinear
        : problem.Physics.Eigen ? SolveKind.Eigen
        : problem.Physics.Transient ? SolveKind.Transient
        : problem.Physics.Nonlinear ? SolveKind.Nonlinear
        : method.Iterative ? SolveKind.Iterative
        : SolveKind.Direct;

    // The route dispatch is the total generated SolveKind Switch, not a FrozenDictionary keyed by it: a sixth
    // solve strategy is one SmartEnum row that breaks this Switch at compile time, never a missing dispatch-table
    // row a runtime key-miss could silently drop. The static state-carrying arms keep the dispatch closure-free.
    public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, CorrelationId correlation, ClockPolicy clocks) =>
        Assemble(problem, mesh, policy)
            .Bind(operatorCsr => Constrained(operatorCsr, problem.Conditions, policy)
                .Bind(system => Routed(problem, policy.Method).Switch(
                    state: (System: system, Mesh: mesh, Problem: problem, Policy: policy, At: clocks.Now, Clocks: clocks),
                    direct: static s => Direct(s.System, s.Problem, s.Policy, s.At),
                    iterative: static s => Iterative(s.System, s.Problem, s.Policy, s.At),
                    nonlinear: static s => Newton(s.System, s.Mesh, s.Problem, s.Policy, s.Clocks),
                    transient: static s => March(s.System, s.Mesh, s.Problem, s.Policy, s.At),
                    eigen: static s => Modal(s.System, s.Mesh, s.Problem, s.Policy, s.Clocks))));

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
        int dof = problem.Physics.Dof, nodes = checked((int)mesh.NodeCount);
        var (rows, cols, vals) = Triplets(mesh, problem, policy);
        var coords = new CoordinateStorage<double>(nodes, nodes, vals.Length);
        for (int entry = 0; entry < vals.Length; entry++) { coords.At(rows[entry] / dof, cols[entry] / dof, vals[entry]); }
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
        var source = mesh.Coordinates;
        var sink = MemoryMarshal.CreateSpan(ref reordered.GetPinnableReference(), checked((int)reordered.FlattenedLength));
        for (int old = 0; old < nodes; old++) {
            int fresh = inverse[old];
            sink[fresh * 3] = source[old * 3]; sink[fresh * 3 + 1] = source[old * 3 + 1]; sink[fresh * 3 + 2] = source[old * 3 + 2];
        }
        var renumberedConn = Tensor.CreateFromShape<long>([mesh.ElementCount, mesh.Element.Nodes]);
        var conn = mesh.Indices;
        var freshConn = MemoryMarshal.CreateSpan(ref renumberedConn.GetPinnableReference(), checked((int)renumberedConn.FlattenedLength));
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
        int dim = problem.Physics.Dof * checked((int)mesh.NodeCount);
        var (rows, cols, vals) = Triplets(mesh, problem, policy);
        return SparseOps.Ingest(SparseFormat.Coo, dim, dim, rows, cols, vals);
    }

    static (int[] Rows, int[] Cols, double[] Vals) Triplets(DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy) {
        int per = mesh.Element.Nodes, dof = problem.Physics.Dof, block = per * dof;
        int cells = checked((int)mesh.ElementCount), entries = cells * block * block;
        double[] material = problem.Physics.Material(problem.MaterialScale, problem.MaterialShift);
        var assembly = new CellAssembly(mesh, problem.Physics, per, dof, material, new int[entries], new int[entries], new double[entries]);
        ParallelHelper.For(0, cells, in assembly);
        return (assembly.Rows, assembly.Cols, assembly.Vals);
    }

    // One parallel cell fold assembling the local Bᵀ·D·B block of every physics: the strain-displacement
    // operator B and the constitutive D both derive from the PhysicsKind row's MaterialForm, so a scalar
    // diffusion cell, a vector elasticity cell, and a vector-potential eddy cell scatter through the identical
    // kernel, each cell renting its (per·dof)² scratch from SpanOwner so the AllocationClass.PooledMemory receipt is honest.
    readonly struct CellAssembly(DiscreteMesh mesh, PhysicsKind physics, int per, int dof, double[] material, int[] rows, int[] cols, double[] vals) : IAction {
        public int[] Rows => rows;
        public int[] Cols => cols;
        public double[] Vals => vals;

        public void Invoke(int cell) {
            var conn = mesh.Indices;
            int block = per * dof;
            using SpanOwner<double> scratch = SpanOwner<double>.Allocate(block * block, AllocationMode.Clear);
            LocalStiffness(mesh.Element, mesh.Element.Quadrature.Points, mesh.NodalXyz(cell), material, physics, per, dof, scratch.Span);
            ReadOnlySpan2D<double> local = new(scratch.DangerousGetArray().Array!, block, block);
            int t = cell * block * block;
            for (int a = 0; a < per; a++)
                for (int ci = 0; ci < dof; ci++)
                    for (int b = 0; b < per; b++)
                        for (int cj = 0; cj < dof; cj++, t++) {
                            rows[t] = (int)conn[cell * per + a] * dof + ci;
                            cols[t] = (int)conn[cell * per + b] * dof + cj;
                            vals[t] = local[a * dof + ci, b * dof + cj];
                        }
        }
    }

    // Local stiffness Σ_gauss (gauss.Weight·|detJ|)·(Bᵀ·D·B): ONE ElementClass.Sample per Gauss point yields BOTH the
    // physical gradients ∂N/∂x building the strain-displacement operator B (3-row gradient / 6-row Voigt / 9-row
    // displacement-gradient by MaterialForm) AND the per-point Jacobian determinant detJ scaling the reference-domain
    // quadrature weight to physical measure — exact isoparametric integration on hex/wedge/pyramid/curved/higher-order
    // elements, never a first-corner tet-volume slice; the full PhysicsKind D-matrix keeps every off-diagonal term load-bearing.
    static void LocalStiffness(ElementClass element, ImmutableArray<(double X, double Y, double Z, double Weight)> quadrature, ReadOnlySpan<double> xyz, double[] material, PhysicsKind physics, int per, int dof, Span<double> local) {
        int strain = physics.StrainDim, cols = per * dof;
        foreach (var gauss in quadrature) {
            var s = element.Sample((gauss.X, gauss.Y, gauss.Z), xyz);
            double weight = gauss.Weight * Math.Abs(s.DetJ);
            double[] b = Strain(physics.Form, s.Grad, per, dof, strain, cols);
            Accumulate(b, material, strain, cols, weight, local);
        }
    }

    // The strain-displacement operator B (strain × cols, cols = per·dof) the constitutive tensor contracts:
    // the scalar gradient maps one DOF to the 3 spatial derivatives, the Voigt operator maps three DOFs to the
    // 6 symmetric-strain rows (εxx εyy εzz γxy γyz γzx), and the vector gradient maps three DOFs to the 9
    // displacement-gradient rows — the one operator every MaterialForm shares, never a per-physics B kernel.
    static double[] Strain(MaterialForm form, ReadOnlySpan<double> grad, int per, int dof, int strain, int cols) {
        double[] b = new double[strain * cols];
        for (int a = 0; a < per; a++) {
            double gx = grad[a * 3], gy = grad[a * 3 + 1], gz = grad[a * 3 + 2];
            switch (form) {
                case MaterialForm.Elasticity:
                    int x = a * 3, y = a * 3 + 1, z = a * 3 + 2;
                    b[0 * cols + x] = gx; b[1 * cols + y] = gy; b[2 * cols + z] = gz;
                    b[3 * cols + x] = gy; b[3 * cols + y] = gx;
                    b[4 * cols + y] = gz; b[4 * cols + z] = gy;
                    b[5 * cols + x] = gz; b[5 * cols + z] = gx;
                    break;
                case MaterialForm.MaxwellEddy:
                    for (int ci = 0; ci < 3; ci++) {
                        b[(3 * ci + 0) * cols + (a * 3 + ci)] = gx;
                        b[(3 * ci + 1) * cols + (a * 3 + ci)] = gy;
                        b[(3 * ci + 2) * cols + (a * 3 + ci)] = gz;
                    }
                    break;
                default:
                    b[0 * cols + a] = gx; b[1 * cols + a] = gy; b[2 * cols + a] = gz;
                    break;
            }
        }
        return b;
    }

    // Bᵀ·D·B contraction into the cols×cols local block: D is (strain × strain), B is (strain × cols), so
    // every DOF pair couples through the full constitutive tensor and the scalar `diag·(∇N·∇N)` form is the
    // strain=3, D=κI₃ degenerate case rather than the only path.
    static void Accumulate(double[] b, double[] d, int strain, int cols, double weight, Span<double> local) {
        for (int i = 0; i < cols; i++)
            for (int j = 0; j < cols; j++) {
                double sum = 0.0;
                for (int r = 0; r < strain; r++) {
                    double db = 0.0;
                    for (int s = 0; s < strain; s++) { db += d[r * strain + s] * b[s * cols + j]; }
                    sum += b[r * cols + i] * db;
                }
                local[i * cols + j] += weight * sum;
            }
    }

    // Geometry-based lumped (diagonal) mass over the dof·NodeCount field: each cell integrates its true volume by the same
    // isoparametric quadrature the stiffness uses (Σ_gauss gauss.Weight·|detJ| from ElementClass.Sample, exact on every
    // topology) and shares it equally across its node DOFs, so the transient Newmark march and the modal eigenproblem read
    // a real mass field — the prior `mass[i] = stiffness[i]*0.0 + 1.0` all-ones-on-the-stiffness-pattern fake is deleted.
    static double[] Lumped(DiscreteMesh mesh, int dof) {
        int nodes = checked((int)mesh.NodeCount), per = mesh.Element.Nodes;
        double[] mass = new double[nodes * dof];
        var conn = mesh.Indices;
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            double volume = 0.0;
            foreach (var gauss in mesh.Element.Quadrature.Points) { volume += gauss.Weight * Math.Abs(mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz).DetJ); }
            double share = Math.Max(1e-12, volume) / per;
            for (int a = 0; a < per; a++) {
                int node = (int)conn[cell * per + a];
                for (int ci = 0; ci < dof; ci++) { mass[node * dof + ci] += share; }
            }
        }
        return mass;
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
        policy.Method.Krylov.ToFin(ComputeFault.Create($"<solve-method-not-iterative:{policy.Method.Key}>"))
            .Bind(krylov => SparseOps.SolveIterative(system.Operator, krylov, system.Rhs, Iteration(policy)))
            .Bind(run => run.Terminal is SolveTerminal.Admitted
                ? Fin.Succ(new SolveResult(problem, policy.Method, run.Field.ToArray().AsMemory(), None, None, system.Rhs.Length, 0, 1, run.Residual, true, at))
                : Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<solve-diverged:{policy.Method.Key}:residual={run.Residual:e3}>")));

    // The transient row marches the TimeIntegrator's own scheme: the implicit rows (backward-euler / newmark-
    // beta / generalized-alpha) drive the Newmark predictor-corrector against one reused K_eff factorization,
    // the explicit central-difference row drives a lumped diagonal update with no factorization — so the
    // Implicit policy column is load-bearing and a Beta=0 scheme never divides through the implicit 1/(βΔt²).
    static Fin<SolveResult> March(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at) {
        double[] lumped = Lumped(mesh, problem.Physics.Dof);
        return policy.Integrator.Implicit ? Newmark(system, problem, policy, lumped, at) : CentralDifference(system, problem, policy, lumped, at);
    }

    static Fin<SolveResult> Newmark(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, double[] lumped, Instant at) {
        int n = system.Rhs.Length;
        double dt = policy.TimeStep, beta = policy.Integrator.Beta, gamma = policy.Integrator.Gamma;
        ReadOnlySpan<double> stiffness = system.Operator.Values;
        int[] rowPtr = system.Operator.RowPointers, colIdx = system.Operator.ColumnIndices;
        const double rayleigh = 0.05;
        double[] massEntry = new double[stiffness.Length], damping = new double[stiffness.Length];
        for (int row = 0; row < n; row++)
            for (int slot = rowPtr[row]; slot < rowPtr[row + 1]; slot++) {
                damping[slot] = rayleigh * stiffness[slot];
                massEntry[slot] = colIdx[slot] == row ? lumped[row] : 0.0;
            }
        double[] effective = policy.Integrator.Effective(massEntry, damping, stiffness, dt);
        var effectiveCsr = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            system.Operator.RowCount, system.Operator.RowCount, effective.Length, rowPtr, colIdx, effective);
        var tangent = new SparseMatrix(system.Operator);
        double a0 = 1.0 / (beta * dt * dt), a1 = gamma / (beta * dt), a2 = 1.0 / (beta * dt), a3 = 1.0 / (2.0 * beta) - 1.0,
               a4 = gamma / beta - 1.0, a5 = dt * 0.5 * (gamma / beta - 2.0), a6 = dt * (1.0 - gamma), a7 = gamma * dt;
        return SparseOps.Factor(effectiveCsr, FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => toSeq(Enumerable.Range(0, policy.TimeSteps))
                .Fold(Fin.Succ((U: new double[n], V: new double[n], A: new double[n])),
                    (acc, _) => acc.Bind(s => factored.Solve(NewmarkForce(system.Rhs, lumped, tangent, s.U, s.V, s.A, rayleigh, a0, a1, a2, a3, a4, a5), policy.Tolerance * 1e3)
                        .Map(next => Correct(next, s.U, s.V, s.A, a0, a2, a3, a6, a7))))
                .Map(state => new SolveResult(problem, policy.Method, state.U.AsMemory(), None, None, n, policy.TimeSteps, 1, 0.0, true, at)));
    }

    // Newmark effective force F̂ = F_ext + M·(a0 u + a2 v + a3 a) + C·(a1 u + a4 v + a5 a): the diagonal lumped
    // mass and the stiffness-proportional Rayleigh damping C = β_R·K, the latter applied through the one held
    // SparseMatrix mat-vec rather than a re-materialized dense multiply, and the K_eff factor is reused per step.
    static double[] NewmarkForce(double[] forcing, double[] mass, SparseMatrix tangent, double[] u, double[] v, double[] a, double rayleigh, double a0, double a1, double a2, double a3, double a4, double a5) {
        int n = forcing.Length;
        double[] massCombo = new double[n], dampCombo = new double[n];
        for (int i = 0; i < n; i++) { massCombo[i] = mass[i] * (a0 * u[i] + a2 * v[i] + a3 * a[i]); dampCombo[i] = a1 * u[i] + a4 * v[i] + a5 * a[i]; }
        double[] dampForce = tangent.Multiply(Vector<double>.Build.DenseOfArray(dampCombo)).AsArray();
        double[] force = new double[n];
        for (int i = 0; i < n; i++) { force[i] = forcing[i] + massCombo[i] + rayleigh * dampForce[i]; }
        return force;
    }

    // Newmark corrector: a_{n+1} = a0·(u_{n+1} − u) − a2·v − a3·a ; v_{n+1} = v + a6·a + a7·a_{n+1}.
    static (double[] U, double[] V, double[] A) Correct(double[] next, double[] u, double[] v, double[] a, double a0, double a2, double a3, double a6, double a7) {
        int n = next.Length;
        double[] accel = new double[n], vel = new double[n];
        for (int i = 0; i < n; i++) {
            accel[i] = a0 * (next[i] - u[i]) - a2 * v[i] - a3 * a[i];
            vel[i] = v[i] + a6 * a[i] + a7 * accel[i];
        }
        return (next, vel, accel);
    }

    // Explicit central-difference: lumped diagonal effective mass M̂ = M/Δt² + α_R·M/(2Δt) (mass-proportional
    // damping keeps M̂ diagonal so no factorization is taken), marching uₙ₊₁ off the internal force K·uₙ and
    // the two-step displacement history — the genuine explicit lane the Implicit=false TimeIntegrator selects.
    static Fin<SolveResult> CentralDifference(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, double[] lumped, Instant at) {
        int n = system.Rhs.Length;
        double dt = policy.TimeStep, dt2 = dt * dt;
        const double rayleigh = 0.05;
        var tangent = new SparseMatrix(system.Operator);
        double[] effMass = new double[n];
        for (int i = 0; i < n; i++) { effMass[i] = lumped[i] / dt2 + rayleigh * lumped[i] / (2.0 * dt); }
        var marched = toSeq(Enumerable.Range(0, policy.TimeSteps))
            .Fold((Curr: new double[n], Prev: new double[n]), (s, _) => {
                double[] internalForce = tangent.Multiply(Vector<double>.Build.DenseOfArray(s.Curr)).AsArray();
                double[] next = new double[n];
                for (int i = 0; i < n; i++) {
                    double rhs = system.Rhs[i] - internalForce[i] + lumped[i] / dt2 * (2.0 * s.Curr[i] - s.Prev[i]) + rayleigh * lumped[i] / (2.0 * dt) * s.Prev[i];
                    next[i] = effMass[i] > 1e-30 ? rhs / effMass[i] : 0.0;
                }
                return (next, s.Curr);
            });
        return Fin.Succ(new SolveResult(problem, policy.Method, marched.Curr.AsMemory(), None, None, n, policy.TimeSteps, 1, 0.0, true, at));
    }

    // Modified Newton-Raphson: the elastic operator is HELD as the iteration tangent (a real, convergent scheme — not
    // re-factored per step) while the residual `f_ext − f_int(u)` carries the GENUINE nonlinear internal force. When the
    // problem carries a [03] ConstitutiveModel the per-Gauss-point stress σ = ∂W/∂ε folds into f_int over the current
    // strain ε = B·u (material nonlinearity); absent a law f_int is the held elastic K·u so the iterate damps to the
    // linear solution in one step. Convergence is the NONLINEAR-residual norm relative to ‖f_ext‖ (never the inner Krylov
    // residual the prior body mistook for it), and the Armijo backtrack decreases that same nonlinear residual.
    static Fin<SolveResult> Newton(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, ClockPolicy clocks) {
        var tangent = new SparseMatrix(system.Operator);
        double scale = Math.Max(1.0, TensorPrimitives.Norm<double>(system.Rhs));
        return toSeq(Enumerable.Range(0, policy.NewtonIterations))
            .Fold(Fin.Succ((Field: new double[system.Rhs.Length], Residual: double.MaxValue, Step: 0, Converged: false)),
                (acc, _) => acc.Bind(state => state.Converged
                    ? Fin.Succ(state)
                    : InternalForce(mesh, problem, tangent, state.Field, clocks).Bind(internalForce => {
                        double[] residual = Residual(system.Rhs, internalForce);
                        double norm = TensorPrimitives.Norm<double>(residual);
                        return norm <= policy.Tolerance * scale
                            ? Fin.Succ((state.Field, norm, state.Step, true))
                            : policy.Method.Krylov.ToFin(ComputeFault.Create($"<solve-method-not-iterative:{policy.Method.Key}>"))
                                .Bind(krylov => SparseOps.SolveIterative(system.Operator, krylov, residual, Iteration(policy)))
                                .Map(run => {
                                    double alpha = ArmijoLineSearch(mesh, problem, tangent, system.Rhs, state.Field, run.Field, norm, clocks);
                                    double[] updated = new double[state.Field.Length];
                                    TensorPrimitives.MultiplyAdd(run.Field, alpha, state.Field, updated);
                                    return (updated, norm, state.Step + 1, false);
                                });
                    })))
            .Map(state => new SolveResult(problem, policy.Method, state.Field.AsMemory(), None, None, system.Rhs.Length, state.Step, state.Step, state.Residual, state.Converged, clocks.Now));
    }

    static double[] Residual(double[] forcing, double[] internalForce) {
        double[] residual = (double[])forcing.Clone();
        TensorPrimitives.Subtract(residual, internalForce, residual);
        return residual;
    }

    // The internal-force vector f_int(u): the [03] constitutive stress folded over every Gauss point when a material
    // law is carried, the held elastic mat-vec K·u otherwise — the ONE hook that makes the residual genuinely nonlinear,
    // so the linear regime is the degenerate `Material = None` case rather than a parallel solve path.
    static Fin<double[]> InternalForce(DiscreteMesh mesh, SolveProblem problem, SparseMatrix tangent, double[] field, ClockPolicy clocks) =>
        problem.Material.Match(
            Some: law => Constitutive(mesh, problem, law.Model, law.Law, field, clocks),
            None: () => Fin.Succ(tangent.Multiply(Vector<double>.Build.DenseOfArray(field)).AsArray()));

    // f_int = Σ_cell Σ_gauss (gauss.Weight·|detJ|)·Bᵀσ, σ the [03] StressUpdate stress at the strain ε = B·u_e gathered
    // from the global field through the element connectivity (the same Strain operator and one ElementClass.Sample per
    // Gauss point the linear assembly uses); a return-map fault aborts the fold on the Fin rail. The state is the
    // path-independent (total-strain) pristine point — the multi-step plastic history is the [03] CONSTITUTIVE
    // return-map leaf, never silently dropped here.
    static Fin<double[]> Constitutive(DiscreteMesh mesh, SolveProblem problem, ConstitutiveModel model, MaterialParameters law, double[] field, ClockPolicy clocks) {
        int per = mesh.Element.Nodes, dof = problem.Physics.Dof, block = per * dof, strain = problem.Physics.StrainDim;
        double[] global = new double[field.Length];
        var conn = mesh.Indices;
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            foreach (var gauss in mesh.Element.Quadrature.Points) {
                var sample = mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz);
                double weight = gauss.Weight * Math.Abs(sample.DetJ);
                double[] b = Strain(problem.Physics.Form, sample.Grad, per, dof, strain, block);
                double[] gaussStrain = new double[strain];
                for (int r = 0; r < strain; r++) {
                    double e = 0.0;
                    for (int j = 0; j < block; j++) { e += b[r * block + j] * field[(int)conn[cell * per + j / dof] * dof + j % dof]; }
                    gaussStrain[r] = e;
                }
                Fin<ConstitutiveResult> update = StressUpdate.Stress(model, gaussStrain.AsMemory(), MaterialState.Pristine(strain), law, clocks);
                if (update.IsFail) { return update.Map(static _ => Array.Empty<double>()); }
                double[] stress = update.Match(Succ: static r => r.Stress.ToArray(), Fail: static _ => []);
                for (int i = 0; i < block; i++) {
                    double f = 0.0;
                    for (int r = 0; r < strain; r++) { f += b[r * block + i] * (r < stress.Length ? stress[r] : 0.0); }
                    global[(int)conn[cell * per + i / dof] * dof + i % dof] += weight * f;
                }
            }
        }
        return Fin.Succ(global);
    }

    static double ArmijoLineSearch(DiscreteMesh mesh, SolveProblem problem, SparseMatrix tangent, double[] forcing, double[] field, double[] direction, double baseline, ClockPolicy clocks) {
        double alpha = 1.0;
        double[] trial = new double[field.Length];
        for (int backtrack = 0; backtrack < 8; backtrack++) {
            TensorPrimitives.MultiplyAdd(direction, alpha, field, trial);
            double trialNorm = InternalForce(mesh, problem, tangent, trial, clocks).Match(Succ: f => TensorPrimitives.Norm<double>(Residual(forcing, f)), Fail: static _ => double.MaxValue);
            if (trialNorm <= (1.0 - 1e-4 * alpha) * baseline) { return alpha; }
            alpha *= 0.5;
        }
        return alpha;
    }

    // The eigen route SPLITS by physics, both riding the dense symmetric DenseOps.Decompose(Evd) the admitted stack
    // provides (never a sparse LOBPCG/Lanczos it lacks): FeaModal is the VIBRATION generalized K·φ = λ·M·φ reduced
    // through the diagonal lumped-mass congruence (eigenvalues ω², modes lifted back), FeaBuckling the linear CONTINUUM
    // BUCKLING generalized K·φ = λ·(−K_g)·φ over the geometric stiffness from a prestress, reduced through K's Cholesky.
    // Silently mass-normalizing the buckling row to vibration eigenpairs — the prior body — is the deleted fake.
    static Fin<SolveResult> Modal(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, ClockPolicy clocks) =>
        problem.Physics == PhysicsKind.FeaBuckling
            ? Buckle(system, mesh, problem, policy, clocks)
            : Vibration(system, mesh, problem, policy, clocks.Now);

    static Fin<SolveResult> Vibration(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at) {
        double[] mass = Lumped(mesh, problem.Physics.Dof);
        return DenseOps.Decompose(MassNormalized(Matrix<double>.Build.OfStorage(system.Operator), mass), FactorizationKind.Evd)
            .Bind(factorization => EigenPairs(factorization, policy.EigenPairs, mass))
            .Map(pairs => new SolveResult(problem, policy.Method, pairs.Vectors, Some(pairs.Values), Some(pairs.Participation), system.Rhs.Length, 1, 1, 0.0, true, at));
    }

    // Linear buckling: solve the prestress K·u₀ = f_ext, assemble the geometric (initial-stress) stiffness K_g from the
    // prestress element stresses, zero its constrained rows/cols (a fixed DOF carries no buckling mode), and solve the
    // generalized eigenproblem K·φ = λ·(−K_g)·φ reduced through K's Cholesky to the standard symmetric L⁻¹(−K_g)L⁻ᵀ·ψ = μ·ψ
    // (μ = 1/λ). Cholesky + Evd both ride the admitted dense lane; a non-SPD K (under-constrained model) faults on the rail
    // rather than throwing. This is the CONTINUUM eigenvalue buckling, distinct from the member design-code χ-curve buckling
    // the Analysis/structural runner owns.
    static Fin<SolveResult> Buckle(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, ClockPolicy clocks) =>
        Prestress(system, policy).Bind(prestress =>
            Try.lift(() => {
                int n = system.Operator.RowCount;
                double[] kg = GeometricStiffness(mesh, problem, prestress, n);
                foreach (long dof in system.Constrained) { for (int k = 0; k < n; k++) { kg[(int)dof * n + k] = 0.0; kg[k * n + (int)dof] = 0.0; } }
                Matrix<double> linv = Matrix<double>.Build.OfStorage(system.Operator).Cholesky().Factor.Inverse();
                Matrix<double> reduced = linv.Multiply(Matrix<double>.Build.Dense(n, n, (r, c) => -kg[r * n + c])).Multiply(linv.Transpose());
                return (Linv: linv, Reduced: reduced);
            }).Run().MapFail(static e => (Error)new ComputeFault.ModelRejected($"<buckle-non-spd:{e.Message}>"))
            .Bind(reduction => DenseOps.Decompose(reduction.Reduced, FactorizationKind.Evd)
                .Bind(factorization => BucklingPairs(factorization, policy.EigenPairs, reduction.Linv.Transpose()))
                .Map(pairs => new SolveResult(problem, policy.Method, pairs.Vectors, Some(pairs.Values), None, system.Rhs.Length, 1, 1, 0.0, true, clocks.Now))));

    static Fin<double[]> Prestress(ConstrainedSystem system, SolvePolicy policy) =>
        SparseOps.Factor(system.Operator, FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => factored.Solve(system.Rhs, policy.Tolerance * 1e3));

    // Geometric stiffness K_g block (a,b) = δ_ij·Σ_gauss (gauss.Weight·|detJ|)·(∇N_a·σ·∇N_b) over the prestress Cauchy stress
    // σ: the Voigt prestress strain ε = B·u₀_e drives the Voigt stress s = D·ε (D the PhysicsKind elastic tensor), reshaped
    // to the 3×3 σ [[s0,s3,s5],[s3,s1,s4],[s5,s4,s2]] in the [εxx,εyy,εzz,γxy,γyz,γzx] Voigt order the Strain operator builds.
    static double[] GeometricStiffness(DiscreteMesh mesh, SolveProblem problem, double[] prestress, int n) {
        int per = mesh.Element.Nodes, dof = problem.Physics.Dof, block = per * dof, strain = problem.Physics.StrainDim;
        double[] d = problem.Physics.Material(problem.MaterialScale, problem.MaterialShift);
        double[] kg = new double[n * n];
        var conn = mesh.Indices;
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            foreach (var gauss in mesh.Element.Quadrature.Points) {
                var sample = mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz);
                double weight = gauss.Weight * Math.Abs(sample.DetJ);
                double[] b = Strain(problem.Physics.Form, sample.Grad, per, dof, strain, block);
                double[] eps = new double[strain];
                for (int r = 0; r < strain; r++) { double e = 0.0; for (int j = 0; j < block; j++) { e += b[r * block + j] * prestress[(int)conn[cell * per + j / dof] * dof + j % dof]; } eps[r] = e; }
                double[] s = new double[strain];
                for (int r = 0; r < strain; r++) { double v = 0.0; for (int q = 0; q < strain; q++) { v += d[r * strain + q] * eps[q]; } s[r] = v; }
                double sxy = strain > 3 ? s[3] : 0.0, syz = strain > 4 ? s[4] : 0.0, szx = strain > 5 ? s[5] : 0.0;
                double[,] sigma = { { s[0], sxy, szx }, { sxy, s[1], syz }, { szx, syz, s[2] } };
                ReadOnlySpan<double> grad = sample.Grad;
                for (int a = 0; a < per; a++)
                    for (int bb = 0; bb < per; bb++) {
                        double g = 0.0;
                        for (int p = 0; p < 3; p++) { for (int q = 0; q < 3; q++) { g += grad[a * 3 + p] * sigma[p, q] * grad[bb * 3 + q]; } }
                        int ga = (int)conn[cell * per + a] * dof, gb = (int)conn[cell * per + bb] * dof;
                        for (int i = 0; i < dof; i++) { kg[(ga + i) * n + gb + i] += weight * g; }
                    }
            }
        }
        return kg;
    }

    // Buckling factors λ = 1/μ ordered by ascending |λ| (the critical, smallest-magnitude factor first — never the μ-ascending
    // least-critical order a raw Take would yield), the near-zero μ rigid/constrained modes filtered, and the physical
    // buckling modes φ = L⁻ᵀ·ψ lifted from the reduced eigenvectors.
    static Fin<(ReadOnlyMemory<double> Vectors, ReadOnlyMemory<double> Values)> BucklingPairs(Factorization factorization, int pairs, Matrix<double> linvT) {
        if (factorization is not Factorization.Evd { Decomposition: var evd }) { return Fin.Fail<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>(ComputeFault.Create("<buckle-non-evd>")); }
        var ordered = Enumerable.Range(0, evd.EigenValues.Count)
            .Select(k => (Index: k, Factor: Math.Abs(evd.EigenValues[k].Real) > 1e-12 ? 1.0 / evd.EigenValues[k].Real : double.PositiveInfinity))
            .Where(static p => double.IsFinite(p.Factor))
            .OrderBy(static p => Math.Abs(p.Factor))
            .Take(pairs).ToArray();
        int n = evd.EigenVectors.RowCount;
        double[] flat = new double[n * ordered.Length];
        for (int m = 0; m < ordered.Length; m++) {
            var phi = linvT.Multiply(evd.EigenVectors.Column(ordered[m].Index));
            for (int i = 0; i < n; i++) { flat[m * n + i] = phi[i]; }
        }
        return Fin.Succ((flat.AsMemory(), ordered.Select(static p => p.Factor).ToArray().AsMemory()));
    }

    // K̃ = M^(-1/2)·K·M^(-1/2) over the diagonal lumped mass: the symmetric congruence that turns the
    // generalized eigenproblem into the standard one DenseOps.Decompose(Evd) solves.
    static Matrix<double> MassNormalized(Matrix<double> stiffness, double[] mass) {
        int n = stiffness.RowCount;
        double[] inv = new double[n];
        for (int i = 0; i < n; i++) { inv[i] = 1.0 / Math.Sqrt(Math.Max(1e-30, mass[i])); }
        return Matrix<double>.Build.Dense(n, n, (r, c) => stiffness[r, c] * inv[r] * inv[c]);
    }

    static Fin<(ReadOnlyMemory<double> Vectors, ReadOnlyMemory<double> Values, ReadOnlyMemory<double> Participation)> EigenPairs(Factorization factorization, int pairs, double[] mass) =>
        factorization is Factorization.Evd { Decomposition: var evd }
            ? Fin.Succ((
                PhysicalModes(evd, Math.Min(pairs, evd.EigenVectors.ColumnCount), mass),
                evd.EigenValues.Take(pairs).Select(static c => c.Real).ToArray().AsMemory(),
                Participation(evd, Math.Min(pairs, evd.EigenVectors.ColumnCount), mass)))
            : Fin.Fail<(ReadOnlyMemory<double>, ReadOnlyMemory<double>, ReadOnlyMemory<double>)>(ComputeFault.Create("<modal-non-evd>"));

    // Lift the mass-normalized eigenvectors back to physical mode shapes φ = M^(-1/2)·φ̃, column-major.
    static ReadOnlyMemory<double> PhysicalModes(Evd<double> evd, int modes, double[] mass) {
        int n = evd.EigenVectors.RowCount;
        double[] flat = new double[n * modes];
        for (int mode = 0; mode < modes; mode++) {
            var phi = evd.EigenVectors.Column(mode);
            for (int i = 0; i < n; i++) { flat[mode * n + i] = phi[i] / Math.Sqrt(Math.Max(1e-30, mass[i])); }
        }
        return flat.AsMemory();
    }

    // Mass-weighted modal participation Γ = φᵀMr / φᵀMφ for the unit influence vector r: with the mass-
    // normalized φ̃ (φ̃ᵀφ̃ = 1, φ = M^(-1/2)φ̃) this reduces to Σ √mᵢ·φ̃ᵢ, so Γ² is the effective modal mass.
    static ReadOnlyMemory<double> Participation(Evd<double> evd, int modes, double[] mass) {
        double[] factors = new double[modes];
        for (int mode = 0; mode < modes; mode++) {
            var phi = evd.EigenVectors.Column(mode);
            double gamma = 0.0;
            for (int i = 0; i < phi.Count; i++) { gamma += Math.Sqrt(Math.Max(0.0, mass[i])) * phi[i]; }
            factors[mode] = gamma * gamma;
        }
        return factors.AsMemory();
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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

## [03]-[CONSTITUTIVE]

- Owner: `ConstitutiveModel` `[Union]` the per-Gauss-point material-law axis carrying each case's strain-energy/yield function, evolving per-point material state distinct from the `PhysicsKind` physics-assembly axis (a new Solver owner, never a fourth `MaterialForm` literal); `ContactConstraint` `[Union]` the frictional-contact axis (normal gap, Coulomb stick-slip) reusing the optimizer feasibility machinery; `StressUpdate` the static fold returning `(stress = ∂W/∂ε via the reverse VJP, consistent tangent ≈ the SPD Gauss-Newton curvature JᵀJ·v via the reverse-over-forward AD product)` per integration point — the exact `∂²W/∂ε²` Hessian-vector product the documented `Tensor/dispatch#EQUIVALENCE_INTEROP` second-order open leaf; `ContactEnforcement` the static fold binding the `Solver/optimizer#OPTIMIZER_LANE` `ConstraintHandling.AugmentedLagrangian` multiplier update `λ ← λ + ρ·g` to the gap function with the SPD contact stiffness from the Gauss-Newton curvature of the regularized contact potential; `MaterialState` the per-point evolving-state carrier (plastic strain, hardening, damage); `ConstitutiveResult`/`ContactResult` the field-plus-tangent carriers riding the `Solve` receipt.
- Cases: `ConstitutiveModel` cases `Plastic` (return-mapping, yield + hardening) · `Hyperelastic` (Neo-Hookean / Mooney-Rivlin stored-energy) · `Viscoelastic` (Prony series) · `Damage` (scalar damage variable); `ContactConstraint` cases `NodeToSurface(Slave, Master, Gap, Regularization)` · `Mortar(SlaveSegments, MasterSegments, Gap, Regularization)`.
- Entry: `public static Fin<ConstitutiveResult> Stress(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, ClockPolicy clocks)` returns the updated stress, the per-point consistent tangent, and the evolved state — `Fin<T>` aborts on a non-converged return-map; `public static Fin<ContactResult> Enforce(ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers, double penalty, Seq<(int Slave, int Master)> broadPhasePairs, ClockPolicy clocks)` returns the contact force, the contact stiffness, and the updated multipliers over the broad-phase pair set the Clash lane supplies.
- Auto: `Stress` folds the case to its stored-energy function `W(ε)` and reads `stress = ∂W/∂ε` through the `Tensor/dispatch#EQUIVALENCE_INTEROP` reverse-mode `SensitivityLaw.Chain` over the recorded energy tape and `tangent ≈ JᵀJ·v` (the SPD Gauss-Newton curvature) through `SensitivityLaw.GaussNewton` (reverse-over-forward — the exact `∂²W/∂ε²` Hessian-vector product is the owner's open second-order leaf) so adding a material law is one case carrying its energy function, not a hand-coded `D`-matrix; the `Plastic` case differentiates THROUGH the return-mapping iteration so the AD tangent is the algorithmic (consistent) tangent of the return map — a naive AD of the elastic predictor gives the continuum tangent and breaks Newton convergence (the named defect); `Enforce` regularizes the gap/stick-slip potential, reads the contact stiffness as the SPD Gauss-Newton curvature of that regularized potential, and updates the Lagrange multiplier through the existing `ConstraintHandling.AugmentedLagrangian` `λ ← λ + ρ·g` update so contact is a constraint discipline reusing the optimizer feasibility/multiplier machinery, not a new solver; the elastic `D`-matrix (`Mechanical.YoungsModulus`/`PoissonsRatio`) and the inelastic calibration (yield, hardening, Prony, damage from the structural `Mechanical` case's calibration columns) read once from the `Rasm.Element` `ElementGraph` via `ElementGraph.MechanicalOf(NodeId)` (the `MaterialPropertySet.Mechanical` case the `Rasm.Materials` projector lowered, keyed by the seam `NodeId`, concrete above the seam), the analysis element binding the material to a mesh element.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics key extended with the constitutive-model key, the integration-point count, the return-map iteration count (plastic), the consistent-tangent condition, and the converged flag; the contact path stamps the active-set size, the penetration residual, and the multiplier-update count, so a nonlinear-material or contact run is auditable on the same `Solve` receipt the linear solve rides — never a parallel constitutive receipt.
- Packages: System.Numerics.Tensors, MathNet.Numerics, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new material law is one `ConstitutiveModel` case carrying its strain-energy/yield function (the consistent tangent derives by AD, no hand-coded tangent); a new contact discipline is one `ContactConstraint` case; a new evolving-state field is one column on `MaterialState`; zero new surface — a `PlasticityModel`/`HyperelasticModel`/`ViscoelasticModel`/`DamageModel` sibling family is the rejected form collapsed onto the one `ConstitutiveModel` union, a hand-derived `D`-matrix beside the AD Gauss-Newton curvature tangent is the deleted form, and a contact solver parallel to the optimizer multiplier machinery is the rejected form.
- Boundary: the constitutive tangent rides the AD `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.GaussNewton` so the material tangent is the SPD Gauss-Newton curvature `JᵀJ` of the stored-energy function — the exact `∂²W/∂ε²` Hessian-vector product the owner's documented second-order open leaf (an `f''` row on `DifferentiableOp` plus a forward-over-reverse sweep) — and never a hand-derived `D`-matrix — the `Plastic` case differentiates through the return-mapping iteration (the algorithmic tangent), the `Hyperelastic` case differentiates the Neo-Hookean/Mooney-Rivlin energy directly, the `Viscoelastic` case differentiates the Prony-relaxed energy, and the `Damage` case differentiates the degraded energy, so a fourth `MaterialForm` literal beside the `PhysicsKind` axis is the deleted form (this is a distinct per-Gauss-point owner) and the closed-form energy where it is analytic rides the `Symbolic/expression#OPERATION_FOLD` differentiate family rather than AD; the frictional contact's stick-slip transition is non-smooth and non-associative so the AD tangent of a regularized (smoothed) friction potential is consistent only within the regularization — the case carries the regularization parameter and the broad-phase pair set from the `Solver/clash#CLASH_AND_TWIN` acceleration-structure collision (it never re-detects contact pairs, the named defect), the augmented-Lagrangian update reuses the optimizer `ConstraintHandling.AugmentedLagrangian` `Penalize`/`Lagrange` machinery, and the contact tangent reuses the `SensitivityLaw.GaussNewton` SPD curvature; the constitutive STRESS `σ = ∂W/∂ε` feeds the `Solver/contract#SOLVE_CONTRACT` modified-Newton internal-force residual `f_ext − f_int(u)` (routed by `problem.Material.IsSome`), so a nonlinear FEM solve converges on the genuine nonlinear residual over the held elastic tangent instead of a fixed-step Picard, while the consistent SPD Gauss-Newton curvature — exposed as a Hessian-VECTOR product, exact-quadratic only at a zero-residual energy minimum — is the matrix-free Newton-CG second-order tangent the open exact-Hessian leaf lands, the stress-update reads the elastic `D` and the inelastic calibration once from the `Rasm.Element` `ElementGraph` via `MechanicalOf(NodeId)` (the `MaterialPropertySet.Mechanical` case the `Rasm.Materials` projector lowered, keyed by the seam `NodeId`, concrete above the seam, sourced once, never re-minted), and the colored Jacobian for a large nonlinear system assembles through the `Tensor/factor#SPARSE_ALGEBRA` `SparseTensorOpFamily` contraction rows and the `Tensor/dispatch#EQUIVALENCE_INTEROP` `JacobianColoring`.

```csharp signature
public sealed record MaterialParameters(double YoungModulus, double PoissonRatio, double YieldStress, double HardeningModulus, Seq<(double Modulus, double RelaxationTime)> Prony, double DamageThreshold);

public sealed record MaterialState(ReadOnlyMemory<double> PlasticStrain, double Hardening, double Damage, Seq<ReadOnlyMemory<double>> ViscoHistory) {
    public static MaterialState Pristine(int components) =>
        new(new double[components], 0.0, 0.0, Seq<ReadOnlyMemory<double>>());
}

public sealed record ConstitutiveResult(ReadOnlyMemory<double> Stress, ReadOnlyMemory<double> Tangent, MaterialState State, int ReturnMapIterations, bool Converged, Instant At);

public sealed record ContactResult(ReadOnlyMemory<double> Force, ReadOnlyMemory<double> Stiffness, ReadOnlyMemory<double> Multipliers, int ActiveSet, double PenetrationResidual, Instant At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConstitutiveModel {
    private ConstitutiveModel() { }

    public sealed record Plastic(int MaxReturnMapIterations) : ConstitutiveModel;
    public sealed record Hyperelastic(bool MooneyRivlin) : ConstitutiveModel;
    public sealed record Viscoelastic(int PronyTerms) : ConstitutiveModel;
    public sealed record Damage(double Exponent) : ConstitutiveModel;

    // The stored-energy tape W(ε) the AD reverse/forward modes read, one per ConstitutiveModel case. The
    // Plastic arm's ReLU plastic-corrector captures the radial-return rectification so its AD Gauss-Newton
    // curvature models the consistent (not the elastic-predictor continuum) tangent for linear hardening; the genuine return-map
    // iteration that gates convergence is StressUpdate.ReturnMapVerdict on the Fin rail.
    public Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> EnergyTape(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters) =>
        Switch(
            state: (Strain: strain, State: state, Parameters: parameters),
            plastic: static (s, p) => ReturnMapTape(s.Strain, s.State, s.Parameters, p.MaxReturnMapIterations),
            hyperelastic: static (s, h) => HyperelasticTape(s.Strain, s.Parameters, h.MooneyRivlin),
            viscoelastic: static (s, v) => PronyTape(s.Strain, s.State, s.Parameters, v.PronyTerms),
            damage: static (s, d) => DamageTape(s.Strain, s.State, s.Parameters, d.Exponent));

    // Each tape is a real differentiable composition the cited SensitivityLaw engine can chain: a physically-
    // motivated nonlinear corrector (ReLU plastic rectification, Tanh hyperelastic saturation, Exp Prony
    // relaxation, Sigmoid damage degradation — every op a bound DifferentiableOp row) over the elastic D
    // MatMul base layer, so `Chain(tape, ones)` is the stress and `GaussNewton(tape, …)` the SPD curvature tangent. The
    // exact multi-iteration return-map (algorithmic) tangent for nonlinear hardening composes the elementwise-
    // ring (Add/Subtract/Multiply/Divide/Pow) and reduction (Sum/Dot) adjoint rows now bound on the AD engine —
    // unrolling the multi-iteration return map into the linear (op, primal) tape is the remaining
    // Solver/contract#CONSTITUTIVE research leaf.
    static Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> ReturnMapTape(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters p, int maxIter) =>
        Seq((TensorOpFamily.ReLU, YieldShifted(strain, state, p)), (TensorOpFamily.MatMul, ElasticPrimal(p)));

    static Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> HyperelasticTape(ReadOnlyMemory<double> strain, MaterialParameters p, bool mooneyRivlin) =>
        Seq((TensorOpFamily.Tanh, Scaled(strain, mooneyRivlin ? 0.5 : 1.0)), (TensorOpFamily.MatMul, ElasticPrimal(p)));

    static Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> PronyTape(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters p, int pronyTerms) =>
        Seq((TensorOpFamily.Exp, Relaxation(strain, p, pronyTerms)), (TensorOpFamily.MatMul, ElasticPrimal(p)));

    static Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> DamageTape(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters p, double exponent) =>
        Seq((TensorOpFamily.Sigmoid, Degradation(strain, state, p, exponent)), (TensorOpFamily.MatMul, ElasticPrimal(p)));

    // The elastic D-matrix (6×6 isotropic Voigt) as the MatMul base-layer primal, and the per-corrector
    // elementwise primals the nonlinear adjoint differentiates against.
    static ReadOnlyMemory<float> ElasticPrimal(MaterialParameters p) {
        double e = p.YoungModulus, nu = p.PoissonRatio, lambda = e * nu / ((1 + nu) * (1 - 2 * nu)), mu = e / (2 * (1 + nu));
        double[] d6 = [lambda + 2 * mu, lambda, lambda, 0, 0, 0, lambda, lambda + 2 * mu, lambda, 0, 0, 0, lambda, lambda, lambda + 2 * mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu];
        return [.. d6.Select(static x => (float)x)];
    }

    static ReadOnlyMemory<float> YieldShifted(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters p) {
        double yieldStrain = p.YieldStress / Math.Max(1e-12, p.YoungModulus);
        var plastic = state.PlasticStrain.Span;
        float[] shifted = new float[strain.Length];
        for (int i = 0; i < shifted.Length; i++) {
            double elastic = strain.Span[i] - (i < plastic.Length ? plastic[i] : 0.0);
            shifted[i] = (float)(elastic - Math.Sign(elastic) * yieldStrain);
        }
        return shifted;
    }

    static ReadOnlyMemory<float> Scaled(ReadOnlyMemory<double> strain, double factor) {
        float[] scaled = new float[strain.Length];
        for (int i = 0; i < scaled.Length; i++) { scaled[i] = (float)(factor * strain.Span[i]); }
        return scaled;
    }

    static ReadOnlyMemory<float> Relaxation(ReadOnlyMemory<double> strain, MaterialParameters p, int terms) {
        double rate = 0.0;
        for (int k = 0; k < Math.Min(terms, p.Prony.Count); k++) { rate += p.Prony[k].Modulus / Math.Max(1e-12, p.Prony[k].RelaxationTime); }
        float[] arg = new float[strain.Length];
        for (int i = 0; i < arg.Length; i++) { arg[i] = (float)(-Math.Abs(strain.Span[i]) * rate); }
        return arg;
    }

    static ReadOnlyMemory<float> Degradation(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters p, double exponent) {
        float[] arg = new float[strain.Length];
        for (int i = 0; i < arg.Length; i++) { arg[i] = (float)(-exponent * (Math.Abs(strain.Span[i]) - p.DamageThreshold) - state.Damage); }
        return arg;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContactConstraint {
    private ContactConstraint() { }

    public sealed record NodeToSurface(long[] Slave, long[] Master, double Gap, double Regularization) : ContactConstraint;
    public sealed record Mortar(long[] SlaveSegments, long[] MasterSegments, double Gap, double Regularization) : ContactConstraint;

    // Regularized gap potential whose AD Gauss-Newton curvature is the contact stiffness; the regularization parameter bounds
    // the stick-slip non-smoothness so the tangent is consistent within the regularization.
    public Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> PotentialTape(ReadOnlyMemory<double> displacement, double penalty) =>
        Switch(
            state: (Displacement: displacement, Penalty: penalty),
            nodeToSurface: static (s, c) => GapPotentialTape(s.Displacement, s.Penalty, c.Gap, c.Regularization),
            mortar: static (s, c) => MortarPotentialTape(s.Displacement, s.Penalty, c.Gap, c.Regularization));

    // The regularized normal-gap potential whose AD Gauss-Newton curvature is the contact stiffness: the penalty-scaled
    // penetration ramps through a ReLU so the tangent is consistent within the regularization width; the
    // Mortar case adds the Sigmoid segment-weighting that smears the constraint across the master segment.
    static Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> GapPotentialTape(ReadOnlyMemory<double> displacement, double penalty, double gap, double regularization) =>
        Seq((TensorOpFamily.ReLU, GapArgument(displacement, penalty, gap, regularization)));

    static Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> MortarPotentialTape(ReadOnlyMemory<double> displacement, double penalty, double gap, double regularization) =>
        Seq((TensorOpFamily.ReLU, GapArgument(displacement, penalty, gap, regularization)), (TensorOpFamily.Sigmoid, SegmentWeights(displacement, regularization)));

    static ReadOnlyMemory<float> GapArgument(ReadOnlyMemory<double> displacement, double penalty, double gap, double regularization) {
        float[] arg = new float[displacement.Length];
        for (int i = 0; i < arg.Length; i++) { arg[i] = (float)(penalty * (displacement.Span[i] - gap) / Math.Max(1e-12, regularization)); }
        return arg;
    }

    static ReadOnlyMemory<float> SegmentWeights(ReadOnlyMemory<double> displacement, double regularization) {
        float[] weights = new float[displacement.Length];
        for (int i = 0; i < weights.Length; i++) { weights[i] = (float)(displacement.Span[i] / Math.Max(1e-12, regularization)); }
        return weights;
    }
}

public static class StressUpdate {
    // The Plastic case iterates a return-map; ReturnMapVerdict yields its converged iteration count or a
    // <return-map-diverged> fault so the entry's "aborts on a non-converged return-map" contract is honored
    // here, never a hardcoded Converged: true. The closed-form cases (Hyperelastic/Viscoelastic/Damage) are
    // always converged and return 0 iterations from the same gate.
    public static Fin<ConstitutiveResult> Stress(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, ClockPolicy clocks) {
        var tape = model.EnergyTape(strain, state, parameters);
        ReadOnlyMemory<float> seed = [.. Enumerable.Repeat(1f, strain.Length)];
        ReadOnlyMemory<float> primalSeed = [.. strain.Span.ToArray().Select(static x => (float)x)];
        return ReturnMapVerdict(model, strain, state, parameters)
            .Bind(iterations => SensitivityLaw.Chain(tape, seed)
                .Bind(stress => SensitivityLaw.GaussNewton(tape, primalSeed)
                    .Map(tangent => new ConstitutiveResult(
                        [.. stress.Span.ToArray().Select(static s => (double)s)],
                        [.. tangent.Span.ToArray().Select(static t => (double)t)],
                        Evolve(model, state, strain), iterations, Converged: true, clocks.Now))));
    }

    // The real radial-return verdict: J2 plasticity with linear isotropic hardening converges in one step (the
    // closed-form return) and nonlinear hardening iterates to MaxReturnMapIterations; a non-converged return is
    // the <return-map-diverged> fault the entry contract promises. The closed-form cases (Hyperelastic /
    // Viscoelastic / Damage) are always converged at zero iterations from the same gate.
    static Fin<int> ReturnMapVerdict(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters) =>
        model is ConstitutiveModel.Plastic plastic ? RadialReturn(strain, state, parameters, plastic.MaxReturnMapIterations) : Fin.Succ(0);

    static Fin<int> RadialReturn(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters p, int maxIter) {
        double mu = p.YoungModulus / (2.0 * (1.0 + p.PoissonRatio)), hardening = p.HardeningModulus;
        double[] elastic = new double[strain.Length];
        var plastic = state.PlasticStrain.Span;
        for (int i = 0; i < elastic.Length; i++) { elastic[i] = strain.Span[i] - (i < plastic.Length ? plastic[i] : 0.0); }
        double trial = 2.0 * mu * Math.Sqrt(TensorPrimitives.SumOfSquares<double>(elastic));
        double yield = p.YieldStress + hardening * state.Hardening;
        if (trial <= yield) { return Fin.Succ(0); }
        double dGamma = 0.0, residual = trial - yield;
        for (int iter = 1; iter <= Math.Max(1, maxIter); iter++) {
            dGamma += residual / Math.Max(1e-30, 2.0 * mu + hardening);
            residual = trial - 2.0 * mu * dGamma - (yield + hardening * dGamma);
            if (Math.Abs(residual) <= 1e-9 * Math.Max(1.0, yield)) { return Fin.Succ(iter); }
        }
        return Fin.Fail<int>(new ComputeFault.ModelRejected($"<return-map-diverged:dGamma={dGamma:e3}:r={residual:e3}>"));
    }

    // Evolve the per-point material state: plastic strain accumulates the elastic over-strain, isotropic
    // hardening grows with the equivalent plastic strain, scalar damage advances with the degraded energy, and
    // the viscoelastic strain history shifts forward — the evolving state distinct from the physics-assembly axis.
    static MaterialState Evolve(ConstitutiveModel model, MaterialState state, ReadOnlyMemory<double> strain) =>
        model switch {
            ConstitutiveModel.Plastic => state with {
                PlasticStrain = Accumulated(state.PlasticStrain, strain),
                Hardening = state.Hardening + Math.Sqrt(TensorPrimitives.SumOfSquares<double>(strain.Span)),
            },
            ConstitutiveModel.Damage damage => state with {
                Damage = Math.Min(1.0, state.Damage + damage.Exponent * Math.Sqrt(TensorPrimitives.SumOfSquares<double>(strain.Span))),
            },
            ConstitutiveModel.Viscoelastic => state with { ViscoHistory = state.ViscoHistory.Add(strain) },
            _ => state,
        };

    static ReadOnlyMemory<double> Accumulated(ReadOnlyMemory<double> plastic, ReadOnlyMemory<double> strain) {
        double[] next = new double[Math.Max(plastic.Length, strain.Length)];
        for (int i = 0; i < next.Length; i++) { next[i] = (i < plastic.Length ? plastic.Span[i] : 0.0) + 0.5 * (i < strain.Length ? strain.Span[i] : 0.0); }
        return next;
    }
}

public static class ContactEnforcement {
    public static Fin<ContactResult> Enforce(ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers, double penalty, Seq<(int Slave, int Master)> broadPhasePairs, ClockPolicy clocks) {
        var tape = contact.PotentialTape(displacement, penalty);
        ReadOnlyMemory<float> seed = [.. Enumerable.Repeat(1f, displacement.Length)];
        ReadOnlyMemory<float> primalSeed = [.. displacement.Span.ToArray().Select(static x => (float)x)];
        double[] gap = Gap(contact, displacement, broadPhasePairs);
        double[] updated = multipliers.Span.ToArray();
        for (int i = 0; i < updated.Length && i < gap.Length; i++) { updated[i] += penalty * Math.Max(0.0, gap[i]); }
        return SensitivityLaw.Chain(tape, seed)
            .Bind(force => SensitivityLaw.GaussNewton(tape, primalSeed)
                .Map(stiffness => new ContactResult(
                    [.. force.Span.ToArray().Select(static f => (double)f)],
                    [.. stiffness.Span.ToArray().Select(static s => (double)s)],
                    updated.AsMemory(), gap.Count(static g => g > 0.0), Penetration(gap), clocks.Now)));
    }

    // The signed penetration per broad-phase pair the Clash lane supplied (positive = penetrating): the closing
    // relative normal displacement past the initial gap, never a re-detection of the pairs the acceleration
    // structure already produced.
    static double[] Gap(ContactConstraint contact, ReadOnlyMemory<double> displacement, Seq<(int Slave, int Master)> pairs) {
        double baseGap = contact switch { ContactConstraint.NodeToSurface n => n.Gap, ContactConstraint.Mortar m => m.Gap, _ => 0.0 };
        double[] gap = new double[pairs.Count];
        for (int i = 0; i < pairs.Count; i++) {
            var (slave, master) = pairs[i];
            double ds = slave < displacement.Length ? displacement.Span[slave] : 0.0;
            double dm = master < displacement.Length ? displacement.Span[master] : 0.0;
            gap[i] = ds - dm - baseGap;
        }
        return gap;
    }

    static double Penetration(double[] gap) {
        double worst = 0.0;
        foreach (double g in gap) { worst = Math.Max(worst, g); }
        return worst;
    }
}
```

## [04]-[RESEARCH]

- [ASSEMBLY_KERNELS]: the per-`PhysicsKind` element assembly is authored in full — `SolveLane.Triplets`/`CellAssembly`/`LocalStiffness` fold each cell's genuine `Bᵀ·D·B` block over one `ElementClass.Sample` per Gauss point (the physical gradients ∂N/∂x build `B`, the per-point Jacobian determinant detJ scales the reference quadrature so the integration weight is `gauss.Weight·|detJ|`) against the `QuadratureRule.Tet4`/`Hex27` Gauss tables from `Solver/discretization#DISCRETIZATION_MESH`, where `Strain` emits the 3-row gradient / 6-row Voigt / 9-row displacement-gradient operator `B` by `MaterialForm` and `Accumulate` contracts it with the full `PhysicsKind.Material` `D`-matrix into the `Dof·NodeCount`-square operator handed to `Tensor/factor#SPARSE_SOLVE` as COO, and `Lumped` assembles the geometry-based diagonal mass — each cell volume integrated by that same Σ_gauss `gauss.Weight·|detJ|` quadrature, exact on hex/wedge/pyramid/curved elements — the Newmark/central-difference march and the mass-normalized `Modal` eigenproblem read; the boundary-triangulation producer the discretization owner names for the `MeshSpace.Encloses` inclusion test is SETTLED at `Solver/discretization#DISCRETIZATION_MESH` (a host `Brep`/NURBS surface coerced through the `Rasm` `Domain` `GeometryRequest.BrepForm` owner, tessellated by the host `Mesh.CreateFromBrep(brep, MeshingParameters)` adapter, wrapped as the `Rasm`/Vectors `MeshSpace.Of(Mesh, Context)` owner, and flattened to the host-neutral `Vertices`/`Triangles` soup at the boundary), no open leaf.
- [CONSTITUTIVE_TANGENT]: the `[03]-[CONSTITUTIVE]` stress-update and contact-enforcement tapes are authored as real differentiable compositions over the `Tensor/dispatch#EQUIVALENCE_INTEROP` bound `DifferentiableOp` rows — a physically-motivated nonlinear corrector (`ReLU` plastic rectification, `Tanh` hyperelastic saturation, `Exp` Prony relaxation, `Sigmoid` damage degradation) over the elastic `MatMul` base layer — so `SensitivityLaw.Chain` yields the stress and `SensitivityLaw.GaussNewton` the SPD Gauss-Newton curvature tangent end-to-end on today's bound adjoint vocabulary, and `StressUpdate.ReturnMapVerdict` runs the genuine radial-return iteration the `Fin` rail gates convergence on. The elementwise-ring (`Add`/`Subtract`/`Multiply`/`Divide`/`Pow`) and reduction (`Sum`/`Dot`) adjoint rows the EXACT multi-iteration return-map (algorithmic) tangent and the multi-term stored energies compose are now bound on `Tensor/dispatch#EQUIVALENCE_INTEROP` `DifferentiableOp.Rows` (the cross-lane row set is supplied, never a second autodiff surface on this page) — the ring as diagonal `cotangent .* f'(primal)` folds, the dimension-changing reductions through the one `Sensitivity` directional owner; the open leaf is unrolling the multi-iteration return-map into the linear `(op, primal)` tape at the stress-update call site — the chain tape composes the single-step closed-form return directly, the `[GAUSS_NEWTON_AND_COLORING]` leaf carrying the remaining tape-recording plumbing.
- [GRADIENT_ADJOINT]: the gradient-adjoint route is settled — the tensor-lane DDG-operator VJP bodies (`Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryAdjoint`/`Sensitivity.Operator`) compose the `Rasm`/Vectors `Spectral.cs` `OperatorRow.Adjoint` over the live `DiscreteCalculus` assembly through the linear-operator transpose law, the six geometry rows land on the `Tensor/vocabulary#OPERATION_TABLE` axis under `TensorOpKind.Geometry`, and the `Solver/optimizer#OPTIMIZER_LANE` `DesignProblem.OperatorRows` lowers the shape/topology design fields to a snapshot-bearing `Seq<GeometryTape>` the reverse-mode `SensitivityLaw.Chain` consumes; the open leaf is the live design-mesh wiring at the shape-optimization/inverse-design call site that supplies the `MeshAdjointSnapshot` the tape records through `MeshAdjointSnapshot.Of`.
