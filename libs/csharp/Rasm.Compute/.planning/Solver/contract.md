# [COMPUTE_SOLVE_CONTRACT]

Rasm.Compute solve contract: one `PhysicsKind`×`BoundaryCondition`×`ElementClass` axis admits FEA, CFD, thermal, daylight, energy, acoustic, electromagnetic, frame, and multi-physics problems as uniform `SolveProblem` rows on the discretized field. `SolveLane` is the one static fold assembling the discrete operator over the `Solver/discretization#DISCRETIZATION_MESH` `DiscreteMesh` — the isoparametric `Bᵀ·D·B` fold for continuum elements, the closed-form 12-DOF member-stiffness scatter for the frame `ElementClass` rows — then dispatching to numeric-lane factorization or iterative solve, marching the transient/nonlinear loop, and driving the adaptive-recovery ladder; `CoupledLane` is the multi-physics fold over field sets bound by `FieldTransfer` rows under Aitken Δ²-relaxation. Owned vocabulary: `PhysicsKind`/`BoundaryCondition`/`ConstraintMethod`/`SolveMethod`/`TimeIntegrator`/`CouplingScheme`/`RecoveryAction`, the `MaterialField` elasticity carrier (the seam-fed per-cell `(E, ν)` admission — `graph.PropertiesOf(member).Mechanical` read once at admission, never a hardcoded Poisson), the `SolveProblem`/`SolveResult`/`ConstrainedSystem` carriers, the `RecoveryPolicy`/`CouplingPolicy` policies, and the `SolveLane`/`CoupledLane` folds.

Dense/sparse factorization and iterative solve ride `Tensor/blas#DENSE_ALGEBRA`/`Tensor/factor#SPARSE_SOLVE`; the ARPACK shift-invert sparse eigensolver rides the vendored `csparse-interop` driver over the SAME CSparse-assembled `K`/`M` (the `SolveMethod` row lifting `fea-modal`/`fea-buckling`/seismic to 10⁴–10⁶ DOF, dense EVD staying the small-DOF terminal). `ElementClass.Sample`/`QuadratureRule`/`DiscreteMesh`/`FieldSpace` and the frame member-stiffness rows arrive settled from `Solver/discretization#DISCRETIZATION_MESH`, the per-Gauss-point constitutive axis (`ConstitutiveModel`/`StressUpdate`/`MaterialState`) from `Solver/constitutive#CONSTITUTIVE`, the gradient-adjoint tape rides `Tensor/dispatch#EQUIVALENCE_INTEROP`, and a distributed solve dials the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc. `SolveProblem.ContentKey` composes the kernel `ContentHash.Of` seed-zero rail over canonical bytes — never a per-call-site hash over a formatted string. Every solver receipt is typed, and the page carries no TS_PROJECTION because solve interiors stay host-local behind the `Solve` rpc.

## [01]-[INDEX]

- [01]-[SOLVE_CONTRACT]: physics×BC×element solve axis; transient/nonlinear; sparse/dense eigen; frame members; multi-physics; recovery.

## [02]-[SOLVE_CONTRACT]

- Owner: `PhysicsKind` `[SmartEnum<string>]` physics-domain rows carrying symmetry, eigen, transient, and material-`D`-matrix columns; `BoundaryCondition` `[Union]` BC cases; `ConstraintMethod` `[SmartEnum<string>]` DOF-constraint rows (elimination/penalty/lagrange); `SolveMethod` `[SmartEnum<string>]` linear/iterative/modal rows carrying the numeric-lane `IterativeMethod`/`FactorizationKind` lowering and a `Preconditioner` column, plus the `arpack-shift-invert` sparse-eigen row binding the vendored `csparse-interop` `Arpack` driver (RID-claim-gated, fault-at-init when the native is absent); `TimeIntegrator` `[SmartEnum<string>]` transient-marching rows; `CouplingScheme` `[SmartEnum<string>]` field-transfer rows with Aitken relaxation; `RecoveryAction` `[SmartEnum<string>]` recovery rows; `MaterialField` `[Union]` the elasticity carrier discriminating a uniform `(scale, shift, ν)` from a per-cell `(E, ν)` assignment (`MaterialField.OfMechanical` lifts `graph.PropertiesOf(member).Mechanical` onto cells once at admission); `SolveProblem` the uniform problem record; `SolveLane` the static assembly/dispatch/march/recovery fold over the `DiscreteMesh`; `CoupledLane` the static multi-physics fold; `SolveResult` the field-plus-evidence carrier.
- Cases: `PhysicsKind` fea-static · fea-modal · fea-transient · fea-buckling · cfd-incompressible · thermal-steady · thermal-transient · daylight-radiosity · energy-balance · acoustic-helmholtz · electromagnetic-eddy; `BoundaryCondition` `Dirichlet` · `Neumann` · `Robin` · `Periodic` · `Contact`; `ConstraintMethod` elimination · penalty · lagrange; `SolveMethod` direct-lu · direct-cholesky · bicgstab · gpbicg · tfqmr · mlk-bicgstab · dense-evd · arpack-shift-invert (the four iterative rows mirror the numeric-lane `IterativeMethod` axis 1:1, each carrying its Krylov column and the `Preconditioner.Diagonal` Jacobi row — the only admitted concrete, `None` aliasing the same diagonal factory; dense-evd is the small-DOF terminal; arpack-shift-invert is the sparse generalized `K·φ = λ·M·φ` shift-invert Lanczos at 10⁴–10⁶ DOF, never a reach into the kernel `Numerics/matrix.md` LOBPCG); `TimeIntegrator` backward-euler · newmark-beta · generalized-alpha · central-difference; `CouplingScheme` one-way · two-way · staggered; `RecoveryAction` refine-mesh · relax · reorder-dofs · switch-method · restart.
- Entry: `public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, CorrelationId correlation, ClockPolicy clocks)` — `Fin<T>` aborts on an ill-posed BC set or a non-convergent run past the cap; a modal row returns eigenpairs through the `SolveMethod`-selected eigen route (`arpack-shift-invert` the sparse `K·φ = λ·M·φ` at building DOF, `dense-evd` the small-DOF congruence), a buckling row the load factors over the geometric-stiffness pencil, a transient row marches the `TimeIntegrator` over the step set reusing one factorization, a nonlinear row (or any problem carrying a `Solver/constitutive` law) drives a modified Newton-Raphson line-searching the nonlinear internal-force residual, and every other row the field over the `FieldSpace`; `SolveAdaptive(…, RecoveryPolicy recovery, …)` walks the `RecoveryAction` ladder on a `Fin.Fail`; `CoupledLane.Couple(CoupledProblem coupling, Seq<DiscreteMesh> meshes, …)` solves the coupled field set under Aitken-relaxed staggering.
- Auto: `Solve` assembles the global operator by folding each cell's local matrix — for continuum rows the `Bᵀ·D·B` block over one `ElementClass.Sample` per Gauss point against the PER-CELL `PhysicsKind.Material` `D` the `MaterialField` resolves (a two-material model assembles two distinct `D` tensors) weighted by `gauss.Weight·|detJ|`, for a frame row the closed-form 12-DOF member-stiffness block scattered by direction cosines — into the `SparseCompressedRowMatrixStorage<double>` the connectivity addresses, applies the `BoundaryCondition` set by the `ConstraintMethod` row, and dispatches to `DenseOps.Decompose`/`SparseOps.Factor`/`SolveIterative` by the `SolveMethod` lowering; a physics row selects the assembly kernel and operator symmetry, so an SPD operator routes Cholesky/BiCgStab and an indefinite one LU/Tfqmr without a call-site branch; an implicit transient row factors `(M/Δt² + γC/Δt + βK)` once and back-substitutes every Newmark step while central-difference marches the lumped diagonal unfactored, and a nonlinear row drives a modified Newton-Raphson over the held elastic tangent whose residual is `f_ext − f_int(u)` (the constitutive stress folded over the current strain when a law is carried, elastic `K·u` otherwise), Armijo-line-searched.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics/method/constraint keys, DOF count, iteration count, final residual, converged flag, and elapsed; the modal row stamps the recovered eigenvalue count and modal participation factors, the transient rows the integrator key and step count, the nonlinear rows the Newton iteration count and load-step list, and the iterative rows ride the `rasm.compute.solve.residual` histogram; the `Coupling` case carries the scheme key, field/transfer/round counts, Aitken factor history, final coupling residual, and converged flag; the `RecoveryReceipt` carries the physics key and the ordered `(action, post-recovery residual)` step list plus the recovered flag.
- Packages: MathNet.Numerics, CSparse, csparse-interop (vendored — the `Arpack` sparse-eigen driver; natives Forge-provisioned, fault-at-init), Rasm (project — the kernel `ContentHash.Of` identity entry), Rasm.Element (project — the seam `MaterialPropertySet.Mechanical` elasticity read), System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new physics domain is one `PhysicsKind` row carrying its assembly-kernel, symmetry, and `D`-matrix columns; a new BC is one `BoundaryCondition` case; a new constraint application one `ConstraintMethod` row; a new linear method or eigensolver one `SolveMethod` row carrying its lowering and preconditioner columns (the ARPACK row is the proof — one row, no second modal rail); a new material-assignment shape one `MaterialField` case; a new time scheme one `TimeIntegrator` row carrying its `(α,β,γ)` column; a new coupling discipline one `CouplingScheme` row plus a `FieldTransfer` mapping; a new recovery strategy one `RecoveryAction` row; zero new surface — a `CfdSolver`/`ThermalSolver`/`FeaSolver`, a `NewmarkSolver`/`GeneralizedAlphaSolver`, and an `FsiCoupler`/`ThermalStructuralCoupler` sibling family each collapse onto the one `SolveLane`/`TimeIntegrator`/`CoupledLane`.
- Boundary: the solve contract is uniform — physics, boundary condition, element, and time scheme discriminate by row/case, never a parallel solver type, so one `Solve` runs an FEA static, a transient thermal march, a CFD pressure-Poisson step, a buckling eigenproblem, and a Helmholtz mode. Discrete operator rides the numeric lane exclusively — assembly produces the CSR `SparseOps.Ingest(SparseFormat.Coo, …)` consumes and factorization/iterative dispatch is `SparseOps.Factor`/`FactoredOp.Solve`/`SolveIterative`/`DenseOps.Decompose`, so a hand-rolled CG loop beside `SparseOps` is the deleted form; the iterative rows carry an `IterativeMethod` lowering and pass a derived `IterationPolicy` into `SolveIterative`, so a raw-`string` method discriminant is deleted. Eigensolve routes in `Modal` by BOTH physics and `SolveMethod`: `FeaModal` recovers ω² and mode shapes, `FeaBuckling` rides the prestress geometric-stiffness `K_g` so the eigenpairs are the buckling load factors λ = 1/μ (silently mass-normalizing buckling to vibration is the deleted fake); `arpack-shift-invert` solves the generalized pencil through the vendored `csparse-interop` `Arpack` (`new Arpack(K, M)` with `ShiftInvert`/`Shift`, `SolveGeneralized(k, shift, Spectrum.SmallestMagnitude)`, `ConvergedEigenValues` gating a typed under-convergence fault) over Compute's own CSparse `K`/`M` — the native factor is `IDisposable`, factored once and reused, and an absent Forge-provisioned native faults at init, never a silent managed degrade — while `dense-evd` reduces through a Cholesky/lumped-mass congruence; a hand-rolled Lanczos beside the two rows is the deleted form. BC application is one `ConstraintMethod` row — elimination partitions DOFs into constrained/free folding values into the RHS, penalty augments the operator diagonal `K[i,i] += P` beside `rhs[i] = P·g` (an RHS-only penalty that never touched the operator is the deleted fake), and Lagrange borders the system; the `Periodic` case ties each master/slave pair through the symmetric penalty stencil so `Master` is load-bearing, and the `Contact` case lowers to the penalty over the gap as an equal-and-opposite reaction across `Slave`/`Master`. Modal and transient rows compose the same assembled operator across steps, so a transient sweep reuses one `Factorization` rather than re-factoring per step — the implicit rows march the Newmark-β predictor-corrector against the once-factored `(M/Δt² + γC/Δt + βK)` with lumped diagonal `M` and Rayleigh `C`, the explicit central-difference row marches the lumped diagonal unfactored, so the `Implicit` column is load-bearing and a `Beta=0` scheme never divides through `1/(βΔt²)`. Nonlinear physics rows drive a MODIFIED Newton-Raphson holding the elastic operator as the iteration tangent (never re-factored per step) whose residual is the genuine nonlinear internal force `f_ext − f_int(u)`: a carried `Solver/constitutive#CONSTITUTIVE` law (routed by `problem.Material.IsSome`) folds the per-Gauss stress `σ = ∂W/∂ε` over the current strain `ε = B·u` into `f_int` through the same `Strain` operator and one `ElementClass.Sample`, and absent a law `f_int` is the held elastic `K·u` damping to the linear solution in one step; convergence is the nonlinear-residual norm relative to `‖f_ext‖`, NEVER the inner Krylov residual, the Armijo backtrack decreases that same residual, and the receipt step count is the genuine outer iteration count (a held-operator iterate converging on the inner-solver residual with a decorative line search is the deleted fake). Consistent full-matrix tangent (the constitutive `SensitivityLaw.GaussNewton` SPD curvature, a Hessian-VECTOR product) is not assembled here — it is the documented matrix-free Newton-CG second-order leaf. Element assembly partitions the per-cell `Bᵀ·D·B` blocks across `ParallelHelper.For`, each cell renting scratch from `SpanOwner<double>` through `ReadOnlySpan2D<double>` so the `AllocationClass.PooledMemory` receipt is honest; the local block is the genuine `Bᵀ·D·B` over a per-`MaterialForm` strain-displacement `B` (3-row gradient, 6-row Voigt, 9-row displacement-gradient) contracted with the full `PhysicsKind.Material` `D`, so the operator is `PhysicsKind.Dof·NodeCount` square and the scalar `material[0]·(∇N·∇N)` Laplacian is the `Dof=1` degenerate case, each Gauss point weighted by `gauss.Weight·|detJ|` from one `ElementClass.Sample` (exact on hex/wedge/pyramid/curved, never a first-corner tet-volume slice) and the lumped mass derived from that same quadrature. Result field is the `FieldSpace` over the mesh stations and crosses to Persistence content-keyed: `SolveProblem.ContentKey` composes the kernel `ContentHash.Of` over canonical little-endian bytes — physics/element keys as UTF-8, the FULL mesh coordinates and connectivity length-prefixed (never a count-only digest two distinct grids collide on), conditions/members count-prefixed, then a presence-tagged constitutive-law payload — never a reflected `GetType().Name` plus a culture-formatted interpolation (the deleted breach; `Tensor/factor` `ShardPlan.Digest` is the sibling proof shape). Elasticity enters ONCE at admission — `MaterialField.OfMechanical` lifts the seam `graph.PropertiesOf(member).Mechanical` rows (`YoungsModulus.Si`, `PoissonsRatio` — the REAL seam members; a `MechanicalOf` spelling is a phantom) onto per-cell `(E, ν)`, a member with no `Mechanical` case is a typed admission fault, and `GeometricStiffness` inherits the same corrected per-cell `D`. A distributed solve dials the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc through the `ShardPlan.Blocked` row-block fan-out; multi-physics coupling is one `CoupledLane` fold over ≥2 fields bound by `FieldTransfer` rows under Aitken Δ²-relaxation (the factor from successive inter-field residuals `ω = -ω·(r·Δr)/(Δr·Δr)`, never a fixed constant), the transferred field reusing the single `BoundaryCondition.Dirichlet` injection; adaptive recovery is one `RecoveryAction` ladder fold relaxing the tolerance/cap, reordering DOFs through the `CSparse.Ordering.AMD` permutation, refining through `MeshKernel.Refine`, switching to `mlk-bicgstab`, then restarting, the `RecoveryReceipt` recording which rung succeeded.

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

    // The per-node DOF count and Voigt strain dimension the one B is shaped to: scalar diffusion 1 DOF / 3-row
    // gradient, vector elasticity 3 DOF / 6-row Voigt, curl-curl 3 DOF / 9-row displacement-gradient — one
    // Bᵀ·D·B fold assembles every physics with no per-row arm.
    public int Dof => Form switch { MaterialForm.Elasticity => 3, MaterialForm.MaxwellEddy => 3, _ => 1 };
    public int StrainDim => Form switch { MaterialForm.Elasticity => 6, MaterialForm.MaxwellEddy => 9, _ => 3 };

    // The constitutive D-matrix contracted as Bᵀ·D·B, sized (StrainDim × StrainDim): 3×3 conductivity/Helmholtz,
    // 6×6 isotropic elasticity, 9×9 vector-potential M⊗I₃ from the 3×3 eddy coupling — every entry live at the
    // Gauss point, never a scalar diagonal.
    public double[] Material(double scale, double shift, double poisson) =>
        Form switch {
            MaterialForm.Elasticity => Elasticity(scale, poisson),
            MaterialForm.MaxwellEddy => KroneckerEye([scale, -shift, 0, shift, scale, 0, 0, 0, scale]),
            _ => Isotropic(scale + ShiftScale * shift),
        };

    // The ONE two-parameter isotropic elasticity-D: λ = E·ν/((1+ν)(1−2ν)), μ = E/(2(1+ν)) — the same form
    // the constitutive ElasticPrimal proves; a hardcoded ν (the prior μ = scale/2, ν = 1/3 collapse) is deleted.
    static double[] Elasticity(double e, double nu) {
        double lambda = e * nu / ((1 + nu) * (1 - 2 * nu)), mu = e / (2 * (1 + nu));
        return [lambda + 2 * mu, lambda, lambda, 0, 0, 0, lambda, lambda + 2 * mu, lambda, 0, 0, 0, lambda, lambda, lambda + 2 * mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu];
    }
    static double[] Isotropic(double diagonal) => [diagonal, 0, 0, 0, diagonal, 0, 0, 0, diagonal];

    // (3×3) ⊗ I₃ → the 9×9 block tensor pairing each vector-potential component with the three gradient
    // directions, so the off-diagonal eddy coupling enters the operator instead of being discarded.
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
    // The sparse generalized shift-invert Lanczos row: csparse-interop Arpack over the CSparse-assembled
    // K/M pencil — RID-claim-gated, the Forge-provisioned native faulting at init when absent.
    public static readonly SolveMethod ArpackShiftInvert = new("arpack-shift-invert", iterative: false, kind: FactorizationKind.Evd, krylov: null, preconditioner: Preconditioner.None);

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
            // Periodic adds the symmetric penalty stencil [+P −P; −P +P] over each (master, slave) pair: the −P
            // coupling lands only where the assembled pattern already holds the master↔slave slot, so `Master` is
            // read and the tie real; an uncoupled opposite-face pair needs the `Lagrange` bordered MPC, not this
            // in-pattern stencil (a slave-only `Constrained.Add` that never read `Master` is the deleted fake).
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
            // dead field; the real active-set/multiplier contact is the Solver/constitutive ContactEnforcement fold over the same gap.
            contact: static (s, bc) => {
                double[] rhs = (double[])s.System.Rhs.Clone();
                double force = bc.Penalty * Math.Max(0.0, bc.Gap);
                int pairs = Math.Min(bc.Slave.Length, bc.Master.Length);
                for (int i = 0; i < bc.Slave.Length; i++) { rhs[bc.Slave[i]] += force; }
                for (int i = 0; i < pairs; i++) { rhs[bc.Master[i]] -= force; }
                return s.System with { Rhs = rhs };
            });

    // Canonical bytes for the SolveProblem content key: case tag, count-prefixed id arrays, values as raw LE
    // doubles — content-stable across processes, never a record GetHashCode (array fields hash by reference).
    public void WriteCanonical(ArrayBufferWriter<byte> sink) =>
        Switch(
            state: sink,
            dirichlet: static (w, bc) => Digest(w, (byte)'d', bc.Nodes, bc.Values),
            neumann: static (w, bc) => Digest(w, (byte)'n', bc.Faces, bc.Flux),
            robin: static (w, bc) => Digest(w, (byte)'r', bc.Faces, [bc.Coefficient, bc.Ambient]),
            periodic: static (w, bc) => Digest(w, (byte)'p', [.. bc.Master, .. bc.Slave], []),
            contact: static (w, bc) => Digest(w, (byte)'c', [.. bc.Slave, .. bc.Master], [bc.Gap, bc.Penalty]));

    static void Digest(ArrayBufferWriter<byte> sink, byte tag, long[] ids, double[] values) {
        Span<byte> scratch = stackalloc byte[8];
        sink.Write([tag]);
        BinaryPrimitives.WriteInt32LittleEndian(scratch, ids.Length); sink.Write(scratch[..4]);
        foreach (long id in ids) { BinaryPrimitives.WriteInt64LittleEndian(scratch, id); sink.Write(scratch); }
        foreach (double v in values) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, v); sink.Write(scratch); }
    }

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
    public static readonly SolvePolicy CanonicalModalSparse = CanonicalModal with { Method = SolveMethod.ArpackShiftInvert, EigenPairs = 30 };
    public static readonly SolvePolicy CanonicalTransient = CanonicalStatic with { Method = SolveMethod.DirectLu, Integrator = TimeIntegrator.NewmarkBeta, TimeStep = 0.01, TimeSteps = 100 };
    public static readonly SolvePolicy CanonicalNonlinear = CanonicalIterative with { Method = SolveMethod.MlkBiCgStab, NewtonIterations = 25 };
}

// The elasticity/material assignment: ONE owner discriminating a uniform (scale, shift, ν) from a per-cell
// (E, ν) field, resolved once per cell at assembly — the seam-fed two-material admission. `OfMechanical`
// lifts the Rasm.Element `graph.PropertiesOf(member).Mechanical` rows (YoungsModulus.Si, PoissonsRatio —
// the real seam members) onto cells; a member with no Mechanical case is a typed admission fault, never a
// silently-defaulted ν.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialField {
    private MaterialField() { }

    public sealed record Uniform(double Scale, double Shift, double Poisson) : MaterialField;
    public sealed record PerCell(ImmutableArray<double> Scale, ImmutableArray<double> Shift, ImmutableArray<double> Poisson) : MaterialField;

    public static readonly MaterialField Unit = new Uniform(1.0, 0.0, 0.0);

    public static Fin<MaterialField> OfMechanical(Seq<Option<MaterialPropertySet.Mechanical>> perCell) =>
        perCell.Traverse(static row => row.ToFin(new ComputeFault.AssessmentInputMissing("<material-field:member-without-mechanical-case>")))
            .Map(static rows => (MaterialField)new PerCell(
                [.. rows.Map(static m => m.YoungsModulus.Si)],
                [.. rows.Map(static _ => 0.0)],
                [.. rows.Map(static m => m.PoissonsRatio)]));

    public (double Scale, double Shift, double Poisson) At(int cell) =>
        this switch {
            Uniform u => (u.Scale, u.Shift, u.Poisson),
            PerCell p => (p.Scale[cell], p.Shift[cell], p.Poisson[cell]),
            _ => (1.0, 0.0, 0.0),
        };

    // One pre-lowered D-resolver per assembly: the uniform arm closes over ONE array, the per-cell arm
    // derives per cell — so the hot fold never re-derives an unchanged tensor.
    public Func<int, double[]> Lower(PhysicsKind physics) =>
        this switch {
            Uniform u => Cached(physics.Material(u.Scale, u.Shift, u.Poisson)),
            _ => cell => { var (s, h, nu) = At(cell); return physics.Material(s, h, nu); },
        };

    static Func<int, double[]> Cached(double[] d) => _ => d;

    // Canonical bytes for the content key: the case tag plus every scalar as raw LE doubles, count-prefixed.
    public void WriteCanonical(ArrayBufferWriter<byte> sink) {
        Span<byte> scratch = stackalloc byte[8];
        void Write(double v) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, v); sink.Write(scratch); }
        switch (this) {
            case Uniform u: sink.Write("u"u8); Write(u.Scale); Write(u.Shift); Write(u.Poisson); break;
            case PerCell p:
                sink.Write("p"u8);
                BinaryPrimitives.WriteInt32LittleEndian(scratch, p.Scale.Length); sink.Write(scratch[..4]);
                foreach (double v in p.Scale) { Write(v); }
                foreach (double v in p.Shift) { Write(v); }
                foreach (double v in p.Poisson) { Write(v); }
                break;
        }
    }
}

public sealed record SolveProblem(
    PhysicsKind Physics,
    ElementClass Element,
    Seq<BoundaryCondition> Conditions,
    FieldSpace Unknown,
    MaterialField Field,
    ImmutableArray<FrameMember> Members,
    Option<(ConstitutiveModel Model, MaterialParameters Law)> Material,
    UInt128 ContentKey) {
    // A frame member carries 6 DOF per node (translations + rotations); every continuum row reads the
    // PhysicsKind Dof column — the ONE element-aware DOF rule every assembly/march/eigen site composes.
    public int Dof => Element.Family == ShapeFamily.Frame ? 6 : Physics.Dof;

    // `Material` carries the Solver/constitutive per-Gauss-point law when the problem is materially nonlinear; its
    // presence is the discriminant `Routed` reads to drive the constitutive-coupled Newton-Raphson, and it folds into
    // the content key so a plastic/hyperelastic solve is a distinct cache identity from the linear-elastic baseline.
    // `Members` carries the per-member frame section/release/offset rows (empty for continuum problems) the
    // discretization ElementClass.Member closed form reads; a Frame mesh scatters one 12-DOF member block per
    // cell indexing Members[cell], so a frame problem missing a full per-element member set is a typed admission
    // fault at construction here — the Fin rail, never an out-of-range index deep in the parallel assembly scatter.
    public static Fin<SolveProblem> Of(PhysicsKind physics, DiscreteMesh mesh, Seq<BoundaryCondition> conditions, MaterialField field, ImmutableArray<FrameMember> members = default, Option<(ConstitutiveModel Model, MaterialParameters Law)> material = default) {
        ImmutableArray<FrameMember> rows = members.IsDefault ? ImmutableArray<FrameMember>.Empty : members;
        return mesh.Element.Family == ShapeFamily.Frame && rows.Length != mesh.ElementCount
            ? Fin.Fail<SolveProblem>(new ComputeFault.ModelRejected($"<solve-frame-member-count:{rows.Length}≠{mesh.ElementCount}>"))
            : Fin.Succ(new SolveProblem(physics, mesh.Element, conditions,
                mesh.FieldOf(FieldStation.Nodal, physics.Dof == 1 ? 0 : 1, physics.Dof), field,
                rows, material,
                Key(physics, mesh, conditions, field, rows, material)));
    }

    // The content key composes the kernel seed-zero ContentHash.Of over canonical little-endian bytes — physics/
    // element keys as UTF-8, the FULL mesh coordinates and connectivity length-prefixed (never a count-only digest
    // two distinct grids collide on), conditions/members count-prefixed self-delimiting, then a presence-tagged
    // constitutive-law payload — never a reflected GetType().Name + a culture-formatted `{law}` interpolation (the
    // deleted breach; Tensor/factor ShardPlan.Digest is the sibling proof shape).
    static UInt128 Key(PhysicsKind physics, DiscreteMesh mesh, Seq<BoundaryCondition> conditions, MaterialField field, ImmutableArray<FrameMember> members, Option<(ConstitutiveModel Model, MaterialParameters Law)> material) {
        var sink = new ArrayBufferWriter<byte>(256);
        Span<byte> scratch = stackalloc byte[8];
        void WriteLong(long v) { BinaryPrimitives.WriteInt64LittleEndian(scratch, v); sink.Write(scratch); }
        void WriteInt(int v) { BinaryPrimitives.WriteInt32LittleEndian(scratch, v); sink.Write(scratch[..4]); }
        sink.Write(Encoding.UTF8.GetBytes(physics.Key));
        sink.Write(Encoding.UTF8.GetBytes(mesh.Element.Key));
        WriteLong(mesh.NodeCount);
        WriteLong(mesh.ElementCount);
        WriteLong(physics.Dof);
        // Full geometry + topology, each contiguous span length-prefixed: two grids sharing node/element counts but
        // differing in one coordinate or one connectivity index are distinct cache identities, never a collision.
        ReadOnlySpan<float> coordinates = mesh.Coordinates;
        ReadOnlySpan<long> indices = mesh.Indices;
        WriteInt(coordinates.Length);
        foreach (float ordinate in coordinates) { BinaryPrimitives.WriteSingleLittleEndian(scratch, ordinate); sink.Write(scratch[..4]); }
        WriteInt(indices.Length);
        foreach (long node in indices) { WriteLong(node); }
        WriteLong(conditions.Count);
        foreach (BoundaryCondition condition in conditions) { condition.WriteCanonical(sink); }
        field.WriteCanonical(sink);
        WriteLong(members.Length);
        foreach (FrameMember member in members) { member.WriteCanonical(sink); }
        sink.Write([(byte)(material.IsSome ? 1 : 0)]);
        material.IfSome(m => { WriteConstitutive(sink, m.Model); WriteLaw(sink, m.Law); });
        return ContentHash.Of(sink.WrittenSpan);
    }

    // Canonical bytes for the carried constitutive identity: the ConstitutiveModel case discriminant plus its one
    // scalar field through the generated Switch, so Plastic(20) ≠ Plastic(30) ≠ Hyperelastic — never a reflected
    // type name; then every MaterialParameters scalar as raw LE doubles with the Prony pairs count-prefixed.
    static void WriteConstitutive(ArrayBufferWriter<byte> sink, ConstitutiveModel model) =>
        model.Switch(
            state: sink,
            plastic: static (w, m) => Tagged(w, (byte)'P', m.MaxReturnMapIterations),
            hyperelastic: static (w, m) => Tagged(w, (byte)'H', m.MooneyRivlin ? 1 : 0),
            viscoelastic: static (w, m) => Tagged(w, (byte)'V', m.PronyTerms),
            damage: static (w, m) => TaggedReal(w, (byte)'D', m.Exponent));

    static void WriteLaw(ArrayBufferWriter<byte> sink, MaterialParameters law) {
        Span<byte> scratch = stackalloc byte[8];
        void Write(double v) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, v); sink.Write(scratch); }
        Write(law.YoungModulus); Write(law.PoissonRatio); Write(law.YieldStress); Write(law.HardeningModulus); Write(law.DamageThreshold);
        BinaryPrimitives.WriteInt32LittleEndian(scratch, law.Prony.Count); sink.Write(scratch[..4]);
        foreach ((double modulus, double relaxation) in law.Prony) { Write(modulus); Write(relaxation); }
    }

    static void Tagged(ArrayBufferWriter<byte> sink, byte tag, int value) {
        Span<byte> scratch = stackalloc byte[4];
        sink.Write([tag]);
        BinaryPrimitives.WriteInt32LittleEndian(scratch, value); sink.Write(scratch);
    }

    static void TaggedReal(ArrayBufferWriter<byte> sink, byte tag, double value) {
        Span<byte> scratch = stackalloc byte[8];
        sink.Write([tag]);
        BinaryPrimitives.WriteDoubleLittleEndian(scratch, value); sink.Write(scratch);
    }
}

// Participation carries the SIGNED per-mode factors Γ_i (Γ_i² the effective modal mass) beside
// TotalMass = Σmᵢ, so a consumer derives the cumulative effective-mass RATIO ΣΓ²/M_total without
// re-owning the lumped-mass field — both live only on the vibration eigen rows.
public sealed record SolveResult(
    SolveProblem Problem,
    SolveMethod Method,
    ReadOnlyMemory<double> Field,
    Option<ReadOnlyMemory<double>> EigenValues,
    Option<ReadOnlyMemory<double>> Participation,
    Option<double> TotalMass,
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
        int dof = problem.Dof, nodes = checked((int)mesh.NodeCount);
        // A degenerate member surfaced by Triplets leaves the mesh unreordered — the ensuing Solve re-fails through
        // Assemble on the same ComputeFault rail, never an AMD reorder over a half-built operator.
        return Triplets(mesh, problem, policy).Match(
            Succ: t => {
                var coords = new CoordinateStorage<double>(nodes, nodes, t.Vals.Length);
                for (int entry = 0; entry < t.Vals.Length; entry++) { coords.At(t.Rows[entry] / dof, t.Cols[entry] / dof, t.Vals[entry]); }
                var csc = CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
                int[] permutation = AMD.Generate(csc, ColumnOrdering.MinimumDegreeAtPlusA);
                return (problem, Renumbered(mesh, permutation, clocks), policy);
            },
            Fail: _ => (problem, mesh, policy));
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
        int dim = problem.Dof * checked((int)mesh.NodeCount);
        return Triplets(mesh, problem, policy)
            .Bind(t => SparseOps.Ingest(SparseFormat.Coo, dim, dim, t.Rows, t.Cols, t.Vals));
    }

    // The frame ElementClass rows scatter the closed-form 12-DOF member block (2 nodes × 6 DOF); every
    // continuum row rides the isoparametric Bᵀ·D·B fold — one dispatch on the element family, never a
    // parallel FrameSolver beside the one SolveLane.
    static Fin<(int[] Rows, int[] Cols, double[] Vals)> Triplets(DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy) {
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof;
        int cells = checked((int)mesh.ElementCount), entries = cells * block * block;
        var outcomes = new Fin<Unit>[cells];
        var assembly = mesh.Element.Family == ShapeFamily.Frame
            ? new CellAssembly(mesh, problem.Physics, per, dof, MemberOf(mesh, problem), new int[entries], new int[entries], new double[entries], outcomes)
            : new CellAssembly(mesh, problem.Physics, per, dof, ContinuumOf(mesh, problem), new int[entries], new int[entries], new double[entries], outcomes);
        ParallelHelper.For(0, cells, in assembly);
        // The parallel scatter cannot short-circuit a Fin, so each cell records its block-producer outcome (the
        // frame ElementClass.Member degenerate-effective-length guard) and the traverse surfaces the FIRST
        // ComputeFault onto the assembly rail — never a discarded failure over a half-scattered operator.
        return toSeq(outcomes).Traverse(static outcome => outcome)
            .Map(_ => (assembly.Rows, assembly.Cols, assembly.Vals));
    }

    // Per-cell local-block producers: the continuum producer folds Bᵀ·D·B over the MaterialField-resolved
    // per-cell D (two materials assemble two distinct tensors); the frame producer reads the discretization
    // ElementClass.Member closed form — releases condensed, rigid-end offsets transformed, semi-rigid
    // springs applied — at the member's (E, ν) row.
    static LocalBlock ContinuumOf(DiscreteMesh mesh, SolveProblem problem) {
        Func<int, double[]> materialOf = problem.Field.Lower(problem.Physics);
        return (int cell, Span<double> local) => {
            LocalStiffness(mesh.Element, mesh.Element.Quadrature.Points, mesh.NodalXyz(cell), materialOf(cell), problem.Physics, mesh.Element.Nodes, problem.Dof, local);
            return Fin.Succ(unit);
        };
    }

    // The frame producer forwards the ElementClass.Member degenerate-effective-length guard onto the block rail,
    // so a member whose rigid-end offsets consume its span faults here rather than scattering a garbage block.
    static LocalBlock MemberOf(DiscreteMesh mesh, SolveProblem problem) =>
        (int cell, Span<double> local) => {
            var (e, _, nu) = problem.Field.At(cell);
            return mesh.Element.Member(mesh.NodalXyz(cell), problem.Members[cell], e, nu, local);
        };

    delegate Fin<Unit> LocalBlock(int cell, Span<double> local);

    // One parallel cell fold scattering the local block of every physics AND every element family: the
    // per-cell block producer owns the continuum-vs-frame distinction, so a scalar diffusion cell, a vector
    // elasticity cell, a vector-potential eddy cell, and a 12-DOF frame member scatter through the identical
    // kernel, each cell renting its (per·dof)² scratch from SpanOwner so the AllocationClass.PooledMemory receipt is honest.
    readonly struct CellAssembly(DiscreteMesh mesh, PhysicsKind physics, int per, int dof, LocalBlock localBlock, int[] rows, int[] cols, double[] vals, Fin<Unit>[] outcomes) : IAction {
        public int[] Rows => rows;
        public int[] Cols => cols;
        public double[] Vals => vals;

        public void Invoke(int cell) {
            var conn = mesh.Indices;
            int block = per * dof;
            using SpanOwner<double> scratch = SpanOwner<double>.Allocate(block * block, AllocationMode.Clear);
            // The block producer's Fin is recorded per cell for the post-scatter traverse; a faulted cell leaves its
            // zero-cleared triplet slots untouched (the fold discards the whole operator), never a garbage scatter.
            outcomes[cell] = localBlock(cell, scratch.Span);
            if (outcomes[cell].IsFail) { return; }
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
            // A frame member's measure is its length (the section area rides the member row at the caller);
            // a continuum cell integrates its true volume by the same isoparametric quadrature the stiffness uses.
            double volume = 0.0;
            if (mesh.Element.Family == ShapeFamily.Frame) {
                double dx = xyz[3] - xyz[0], dy = xyz[4] - xyz[1], dz = xyz[5] - xyz[2];
                volume = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
            else {
                foreach (var gauss in mesh.Element.Quadrature.Points) { volume += gauss.Weight * Math.Abs(mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz).DetJ); }
            }
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
            .Map(field => new SolveResult(problem, policy.Method, field.AsMemory(), None, None, None, system.Rhs.Length, 1, 1, 0.0, true, at));

    static IterationPolicy Iteration(SolvePolicy policy) =>
        IterationPolicy.Default with { Tolerance = policy.Tolerance, MaxIterations = policy.MaxIterations, Preconditioner = policy.Method.Preconditioner.Build };

    static Fin<SolveResult> Iterative(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, Instant at) =>
        policy.Method.Krylov.ToFin(ComputeFault.Create($"<solve-method-not-iterative:{policy.Method.Key}>"))
            .Bind(krylov => SparseOps.SolveIterative(system.Operator, krylov, system.Rhs, Iteration(policy)))
            .Bind(run => run.Terminal is SolveTerminal.Admitted
                ? Fin.Succ(new SolveResult(problem, policy.Method, run.Field.ToArray().AsMemory(), None, None, None, system.Rhs.Length, 0, 1, run.Residual, true, at))
                : Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<solve-diverged:{policy.Method.Key}:residual={run.Residual:e3}>")));

    // The transient row marches the TimeIntegrator's own scheme: the implicit rows (backward-euler / newmark-
    // beta / generalized-alpha) drive the Newmark predictor-corrector against one reused K_eff factorization,
    // the explicit central-difference row drives a lumped diagonal update with no factorization — so the
    // Implicit policy column is load-bearing and a Beta=0 scheme never divides through the implicit 1/(βΔt²).
    static Fin<SolveResult> March(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at) {
        double[] lumped = Lumped(mesh, problem.Dof);
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
                .Map(state => new SolveResult(problem, policy.Method, state.U.AsMemory(), None, None, None, n, policy.TimeSteps, 1, 0.0, true, at)));
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
        return Fin.Succ(new SolveResult(problem, policy.Method, marched.Curr.AsMemory(), None, None, None, n, policy.TimeSteps, 1, 0.0, true, at));
    }

    // Modified Newton-Raphson: the elastic operator is HELD as the iteration tangent (a real, convergent scheme — not
    // re-factored per step) while the residual `f_ext − f_int(u)` carries the GENUINE nonlinear internal force. When the
    // problem carries a Solver/constitutive ConstitutiveModel the per-Gauss-point stress σ = ∂W/∂ε folds into f_int over the current
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
            .Map(state => new SolveResult(problem, policy.Method, state.Field.AsMemory(), None, None, None, system.Rhs.Length, state.Step, state.Step, state.Residual, state.Converged, clocks.Now));
    }

    static double[] Residual(double[] forcing, double[] internalForce) {
        double[] residual = (double[])forcing.Clone();
        TensorPrimitives.Subtract(residual, internalForce, residual);
        return residual;
    }

    // The internal-force vector f_int(u): the Solver/constitutive stress folded over every Gauss point when a material
    // law is carried, the held elastic mat-vec K·u otherwise — the ONE hook that makes the residual genuinely nonlinear,
    // so the linear regime is the degenerate `Material = None` case rather than a parallel solve path.
    static Fin<double[]> InternalForce(DiscreteMesh mesh, SolveProblem problem, SparseMatrix tangent, double[] field, ClockPolicy clocks) =>
        problem.Material.Match(
            Some: law => Constitutive(mesh, problem, law.Model, law.Law, field, clocks),
            None: () => Fin.Succ(tangent.Multiply(Vector<double>.Build.DenseOfArray(field)).AsArray()));

    // f_int = Σ_cell Σ_gauss (gauss.Weight·|detJ|)·Bᵀσ, σ the Solver/constitutive StressUpdate stress at the strain ε = B·u_e gathered
    // from the global field through the element connectivity (the same Strain operator and one ElementClass.Sample per
    // Gauss point the linear assembly uses); a return-map fault aborts the fold on the Fin rail. The state is the
    // path-independent (total-strain) pristine point — the multi-step plastic history is the Solver/constitutive#CONSTITUTIVE
    // return-map leaf, never silently dropped here.
    static Fin<double[]> Constitutive(DiscreteMesh mesh, SolveProblem problem, ConstitutiveModel model, MaterialParameters law, double[] field, ClockPolicy clocks) {
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof, strain = problem.Physics.StrainDim;
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

    // The eigen route dispatches on the JOINT (physics, method) discriminant: FeaModal is the VIBRATION generalized
    // K·φ = λ·M·φ, FeaBuckling the linear CONTINUUM BUCKLING generalized K·φ = λ·(−K_g)·φ over the geometric
    // stiffness from a prestress (silently mass-normalizing buckling to vibration is the deleted fake); the
    // arpack-shift-invert row solves the pencil SPARSELY through the vendored csparse-interop driver at building
    // DOF, the dense-evd terminal reduces through the Cholesky/lumped-mass congruence at small DOF.
    static Fin<SolveResult> Modal(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, ClockPolicy clocks) =>
        (Buckling: problem.Physics == PhysicsKind.FeaBuckling, Sparse: policy.Method == SolveMethod.ArpackShiftInvert) switch {
            (false, false) => Vibration(system, mesh, problem, policy, clocks.Now),
            (false, true) => ArpackPencil(system, Lumped(mesh, problem.Dof), problem, policy, clocks.Now),
            (true, false) => Buckle(system, mesh, problem, policy, clocks),
            (true, true) => Prestress(system, policy).Bind(prestress =>
                ArpackPencil(system, GeometricTriplets(mesh, problem, prestress, system), None, problem, policy, clocks.Now)),
        };

    // ONE sparse-eigen kernel for both pencils: vibration passes the lumped diagonal mass, buckling the −K_g
    // geometric stiffness — the right-hand matrix is the only varying input, and the Option'd mass carries the
    // participation concern (the vibration leg computes Γ over the B-orthonormal ARPACK modes; the buckling
    // leg has no mass pencil, so its participation stays None by construction). The native Arpack driver factors
    // once and walks the shift-invert Lanczos; an absent Forge-provisioned native or an internal ARPACK error
    // surfaces through the one lifted boundary as a typed fault (fault-at-init, never a silent managed degrade),
    // and an under-converged spectrum is a typed shortfall naming the achieved count — never a silent truncation.
    static Fin<SolveResult> ArpackPencil(ConstrainedSystem system, CSparse.Double.SparseMatrix rhs, Option<double[]> lumpedMass, SolveProblem problem, SolvePolicy policy, Instant at) =>
        Try.lift(() => {
            var stiffness = SparseOps.ToCsc(system.Operator);
            using var arpack = new Arpack(stiffness, rhs) { ComputeEigenVectors = true, ShiftInvert = true, Shift = 0.0, Tolerance = policy.Tolerance, ArnoldiCount = Math.Max(2 * policy.EigenPairs + 1, 20) };
            return arpack.SolveGeneralized(policy.EigenPairs, 0.0, Spectrum.SmallestMagnitude);
        }).Run().MapFail(static e => (Error)new ComputeFault.ModelRejected($"<arpack-native-or-solve:{e.GetType().Name}:{e.Message}>"))
            .Bind(result => result.ConvergedEigenValues < policy.EigenPairs
                ? Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<arpack-under-converged:{result.ConvergedEigenValues}/{policy.EigenPairs}>"))
                : Fin.Succ(new SolveResult(
                    problem, policy.Method,
                    Columns(result.EigenVectors, policy.EigenPairs),
                    Some((ReadOnlyMemory<double>)[.. result.EigenValues.Take(policy.EigenPairs).Select(static c => c.Real)]),
                    lumpedMass.Map(m => Participation(result.EigenVectors, policy.EigenPairs, m)),
                    lumpedMass.Map(static m => m.Sum()),
                    system.Rhs.Length, result.IterationsTaken, 1, 0.0, true, at)));

    static Fin<SolveResult> ArpackPencil(ConstrainedSystem system, double[] lumpedMass, SolveProblem problem, SolvePolicy policy, Instant at) =>
        ArpackPencil(system, SparseOps.Diagonal(lumpedMass), Some(lumpedMass), problem, policy, at);

    // Sparse-leg participation: ARPACK generalized shift-invert returns B-orthonormal modes (φᵀMφ = 1), so
    // Γ_mode = Σᵢ mᵢ·φᵢ directly — the signed factor whose square is the effective modal mass.
    static ReadOnlyMemory<double> Participation(CSparse.Double.DenseMatrix vectors, int pairs, double[] mass) {
        int n = vectors.RowCount, take = Math.Min(pairs, vectors.ColumnCount);
        double[] factors = new double[take];
        for (int mode = 0; mode < take; mode++) {
            double gamma = 0.0;
            for (int i = 0; i < n; i++) { gamma += mass[i] * vectors.At(i, mode); }
            factors[mode] = gamma;
        }
        return factors.AsMemory();
    }

    // The −K_g pencil right-hand side assembled SPARSELY (per-element scatter into COO, never a dense n²
    // buffer at building DOF), constrained rows/cols dropped — a fixed DOF carries no buckling mode.
    static CSparse.Double.SparseMatrix GeometricTriplets(DiscreteMesh mesh, SolveProblem problem, double[] prestress, ConstrainedSystem system) {
        int n = system.Operator.RowCount;
        var coo = new CoordinateStorage<double>(n, n, checked((int)mesh.ElementCount) * mesh.Element.Nodes * mesh.Element.Nodes * problem.Dof);
        GeometricScatter(mesh, problem, prestress, (row, col, value) => {
            if (!system.Constrained.Contains(row) && !system.Constrained.Contains(col)) { coo.At(row, col, -value); }
        });
        return (CSparse.Double.SparseMatrix)CompressedColumnStorage<double>.OfIndexed(coo, inplace: false);
    }

    // Column-major eigenvector flatten off the native result matrix — the boundary projection into the
    // SolveResult field memory.
    static ReadOnlyMemory<double> Columns(CSparse.Double.DenseMatrix vectors, int pairs) {
        int n = vectors.RowCount, take = Math.Min(pairs, vectors.ColumnCount);
        double[] flat = new double[n * take];
        for (int m = 0; m < take; m++) { for (int i = 0; i < n; i++) { flat[m * n + i] = vectors.At(i, m); } }
        return flat.AsMemory();
    }

    static Fin<SolveResult> Vibration(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at) {
        double[] mass = Lumped(mesh, problem.Dof);
        return DenseOps.Decompose(MassNormalized(Matrix<double>.Build.OfStorage(system.Operator), mass), FactorizationKind.Evd)
            .Bind(factorization => EigenPairs(factorization, policy.EigenPairs, mass))
            .Map(pairs => new SolveResult(problem, policy.Method, pairs.Vectors, Some(pairs.Values), Some(pairs.Participation), Some(mass.Sum()), system.Rhs.Length, 1, 1, 0.0, true, at));
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
                .Map(pairs => new SolveResult(problem, policy.Method, pairs.Vectors, Some(pairs.Values), None, None, system.Rhs.Length, 1, 1, 0.0, true, clocks.Now))));

    static Fin<double[]> Prestress(ConstrainedSystem system, SolvePolicy policy) =>
        SparseOps.Factor(system.Operator, FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => factored.Solve(system.Rhs, policy.Tolerance * 1e3));

    // Geometric stiffness K_g block (a,b) = δ_ij·Σ_gauss (gauss.Weight·|detJ|)·(∇N_a·σ·∇N_b) over the prestress Cauchy stress
    // σ: the Voigt prestress strain ε = B·u₀_e drives the Voigt stress s = D·ε (D the PhysicsKind elastic tensor), reshaped
    // to the 3×3 σ [[s0,s3,s5],[s3,s1,s4],[s5,s4,s2]] in the [εxx,εyy,εzz,γxy,γyz,γzx] Voigt order the Strain operator builds.
    static double[] GeometricStiffness(DiscreteMesh mesh, SolveProblem problem, double[] prestress, int n) {
        double[] kg = new double[n * n];
        GeometricScatter(mesh, problem, prestress, (row, col, value) => kg[row * n + col] += value);
        return kg;
    }

    // The one geometric-stiffness scatter both eigen routes consume: the dense small-DOF buffer and the
    // sparse COO pencil compose the identical per-element (∇N·σ·∇N) fold through this callback.
    static void GeometricScatter(DiscreteMesh mesh, SolveProblem problem, double[] prestress, Action<int, int, double> scatter) {
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof, strain = problem.Physics.StrainDim;
        Func<int, double[]> materialOf = problem.Field.Lower(problem.Physics);
        var conn = mesh.Indices;
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            double[] d = materialOf(cell);
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
    // normalized φ̃ (φ̃ᵀφ̃ = 1, φ = M^(-1/2)φ̃) this reduces to Σ √mᵢ·φ̃ᵢ. The stream carries the SIGNED Γ
    // (Γ² the effective modal mass; the sign feeds CQC cross-modal terms) — the cumulative RATIO is the
    // consumer's ΣΓ²/TotalMass fold, never a self-normalized quotient that reads ~1 for any spectrum.
    static ReadOnlyMemory<double> Participation(Evd<double> evd, int modes, double[] mass) {
        double[] factors = new double[modes];
        for (int mode = 0; mode < modes; mode++) {
            var phi = evd.EigenVectors.Column(mode);
            double gamma = 0.0;
            for (int i = 0; i < phi.Count; i++) { gamma += Math.Sqrt(Math.Max(0.0, mass[i])) * phi[i]; }
            factors[mode] = gamma;
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
            ? Seq(double.MaxValue)
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [GRADIENT_ADJOINT_WIRING]-[OPEN]: what live design-mesh at the shape-optimization/inverse-design call site supplies the `MeshAdjointSnapshot` the reverse-mode tape records through `MeshAdjointSnapshot.Of` — the DDG-operator VJP bodies, the `TensorOpKind.Geometry` rows, and the `Solver/optimizer#OPTIMIZER_LANE` `DesignProblem.OperatorRows` lowering are settled on `Tensor/dispatch#EQUIVALENCE_INTEROP`, so only the design-mesh wiring is open; the optimization call site.
