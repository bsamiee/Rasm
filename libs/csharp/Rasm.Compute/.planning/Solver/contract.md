# [COMPUTE_SOLVE_CONTRACT]

Rasm.Compute solve contract: one `PhysicsKind`×`BoundaryCondition`×`ElementClass` axis admits FEA, CFD, thermal, daylight, energy, acoustic, electromagnetic, frame, and multi-physics problems as uniform `SolveProblem` rows on the discretized field.

`SolveLane` is the one static fold assembling the discrete operator over the `Solver/discretization#DISCRETIZATION_MESH` `DiscreteMesh` — the isoparametric `Bᵀ·D·B` fold for continuum elements, the closed-form 12-DOF member-stiffness scatter for the frame `ElementClass` rows — then dispatching to numeric-lane factorization or iterative solve, marching the transient/nonlinear loop, and driving the adaptive-recovery ladder; `CoupledLane` is the multi-physics fold over field sets bound by `FieldTransfer` rows under Aitken Δ²-relaxation.

Owned vocabulary: `PhysicsKind`/`BoundaryCondition`/`ConstraintMethod`/`SolveMethod`/`TimeIntegrator`/`CouplingScheme`/`RecoveryAction`, the `MaterialField` elastic-or-scalar coefficient carrier, the `SolveProblem`/`SolveResult`/`ConstrainedSystem`/`CondensationEvidence` carriers, the `SolvePolicy`/`RecoveryPolicy`/`CouplingPolicy`/`CondensationPolicy` policies, and the `SolveLane`/`CoupledLane` folds.

Dense/sparse factorization and iterative solve ride `Tensor/blas#DENSE_ALGEBRA`/`Tensor/factor#SPARSE_SOLVE`; generalized eigenanalysis reuses the verified dense `Evd` terminal after mass, static-condensation, or geometric-stiffness reduction. Reducing a generalized pencil to standard form demands a positive-definite inertia factor, which a lumped-mass frame lacks on its inertia-free rows, so static condensation is what makes any modal terminal reachable at building order.

`ElementClass.Sample`/`DiscreteMesh`/`FieldSpace` and the frame member-stiffness rows arrive settled from `Solver/discretization#DISCRETIZATION_MESH`, whose `ElementClass.Quadrature` elects its rule off the kernel `Rasm/Numerics/integrate#QUADRATURE` `ReferenceElement` ladder, the per-Gauss-point constitutive axis (`ConstitutiveModel`/`StressUpdate`/`MaterialState`) from `Solver/constitutive#CONSTITUTIVE`, the gradient-adjoint tape rides `Tensor/dispatch#EQUIVALENCE_INTEROP`, and a distributed solve dials the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc.

`SolveProblem.ContentKey` composes the kernel `ContentHash.Of` seed-zero rail over canonical bytes — never a per-call-site hash over a formatted string. Every solver receipt is typed, and the page carries no TS_PROJECTION because solve interiors stay host-local behind the `Solve` rpc.

## [01]-[INDEX]

- [02]-[SOLVE_CONTRACT]: physics×BC×element solve axis; transient/nonlinear; sparse/dense eigen; frame members; multi-physics; recovery.

## [02]-[SOLVE_CONTRACT]

- Owner: `PhysicsKind` `[SmartEnum<string>]` carries symmetry, lifecycle, `MaterialForm`, and `OperatorForm`; `PhysicsPayload` `[Union]` carries continuum, mixed-flow, radiosity, energy-network, Helmholtz, and eddy-current data; `BoundaryCondition` `[Union]` and `ConstraintMethod` own DOF constraints; `SolveMethod` `[SmartEnum<string>]` carries numeric or continuation lowering; `TimeIntegrator`, `CouplingScheme`, and `RecoveryAction` own their rows; `MaterialField` `[Union]` carries uniform or per-cell elastic coefficients including density, or a scalar coefficient beside its volumetric heat capacity; `ModalParticipation` carries the per-axis factor both modal routes report and the excitable mass they report it against; `CondensationPolicy` carries the retained-set ceiling, the reduction residual cap, and the transformation byte ceiling; `SolveProblem` binds every discriminant and payload; `SolvePolicy` binds the method/constraint/integrator rows beside the iteration cap, tolerance, time grid, penalty factor, Rayleigh damping pair, dense-terminal ceiling, continuation, condensation, and the lane's wall deadline and cooperative token; `SolveArchive`/`SolveArchiveKind` the route-borne HDF5 archive capability with its three container classes, `SolveHistory`/`SolveModes`/`SolveCheckpoint` the per-class sessions over the `Runtime/codecs#HDF_ARCHIVE` owner; `SolveLane` and `CoupledLane` own execution.
- Cases: `PhysicsKind` fea-static · fea-modal · fea-transient · fea-buckling · cfd-incompressible · thermal-steady · thermal-transient · daylight-radiosity · energy-balance · acoustic-helmholtz · electromagnetic-eddy; `BoundaryCondition` `Dirichlet` · `Neumann` · `Robin` · `Periodic` · `Contact`; `ConstraintMethod` elimination · penalty · lagrange; `SolveMethod` direct-lu · direct-cholesky · bicgstab · gpbicg · tfqmr · mlk-bicgstab · arc-length · dense-evd · condensed-evd; `TimeIntegrator` backward-euler · newmark-beta · generalized-alpha · central-difference; `CouplingScheme` one-way · two-way · staggered; `RecoveryAction` refine-mesh · relax · reorder-dofs · switch-method · restart.
- Entry: `public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, IClock clock, Option<SolveArchive> archive = default)` — `Fin<T>` aborts on an ill-posed BC set or a non-convergent run past the cap; a supplied `SolveArchive` lands the transient response history ([steps, dofs] one chunk per accepted step), the modal mode-shape container ([pairs, dofs] mode-outermost with eigenvalues, participation, and condensation attributes), and one create-only checkpoint container per committed arc step, each through the caller's per-class sink and an archive write fault FAULTING the run; a modal row returns eigenpairs through the verified dense `Evd` route over the whole operator, or — where the policy carries a `CondensationPolicy` — over the statically condensed pencil the inertia-bearing degrees of freedom carry, a buckling row the load factors over the geometric-stiffness pencil, a transient row marches the `TimeIntegrator` over the step set reusing one factorization, a nonlinear row (any problem carrying a `Solver/constitutive` law or a `Contact` condition) drives a Newton-Raphson whose per-iteration operator is the consistent tangent assembled from the SAME trial state, line-searching the internal-force residual over a committed per-(cell, gauss) `MaterialState` ledger — trial evolutions read the committed rows, probes discard theirs, and only a converged step commits its trial ledger and contact multipliers — and every other row the field over the `FieldSpace`; `SolveAdaptive(…, RecoveryPolicy recovery, …)` walks the `RecoveryAction` ladder on a `Fin.Fail`; `CoupledLane.Couple(CoupledProblem coupling, Seq<DiscreteMesh> meshes, …)` solves the coupled field set under Aitken-relaxed staggering.
- Auto: `Solve` folds elasticity/diffusion as `Bᵀ·D·B`, mixed incompressible flow as velocity-gradient/pressure coupling with advective transport, Helmholtz as stiffness minus wave-number mass, and eddy current as doubled-real curl-curl under conductivity coupling. `Radiosity` lowers `I − diag(ρ)F`, and `EnergyNetwork` lowers its conductance matrix. Second-order structural transients use Newmark/generalized-α/central difference; thermal, flow, and energy rows use a factored first-order capacity march. `ArcLength` enforces the Crisfield displacement/load constraint through predictor-corrector iterations across limit points. `CondensedEvd` elects the retained set off the lumped inertia the model itself carries, factors the condensed block once through the CSparse SPD owner, folds the slave inertia onto the retained pencil, and returns full-length mode shapes with the condensed rows recovered.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics/method/constraint keys, DOF count, iteration count, final residual, converged flag, and elapsed; the modal rows alone fill its `ParticipationX`/`ParticipationY`/`ParticipationZ` columns, projected at the mint site as the per-axis effective-mass fraction of `SolveResult.Participation` against `SolveResult.TotalMass`, the condensed modal row adds its measured `CondensationEvidence` (retained and condensed counts, reduction residual, pencil conditioning) beside the block eigen-defect it reports as the residual, the transient rows the integrator key and step count, the nonlinear rows the Newton iteration count and load-step list, and the iterative rows ride the `rasm.compute.solve.residual` histogram; the `Coupling` case carries the scheme key, field/transfer/round counts, final coupling residual, and converged flag (the Aitken factor history rides `CoupledResult.History`); the `RecoveryReceipt` carries the physics key and the ordered `(action, post-recovery residual)` step list and the recovered flag.
- Packages: MathNet.Numerics, CSparse, PureHDF (`H5File` graph assignment, `H5Dataset<T>(ulong[] fileDims, uint[] chunks, …)` deferred slots, and `H5Dataset(object, fileDims:)` explicit datasets behind the three archive sessions — opens, filters, cursors, and writer mechanics stay the `Runtime/codecs#HDF_ARCHIVE` owner's `HdfArchive`/`HdfWriter`/`HdfArchivePolicy`), Rasm (project — the kernel `ContentHash.Of` identity entry), Rasm.Element (project — the seam `MaterialPropertySet.Mechanical` elasticity and density reads beside the `Thermal` `SpecificHeat` read the volumetric capacity composes), System.Numerics (`Vector3` — the contact normal), System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new physics domain is one `PhysicsKind` row with one `PhysicsPayload` case only when its operator data differs; a new BC is one `BoundaryCondition` case; a new constraint application is one `ConstraintMethod` row; a new numeric, continuation, or reduction method is one `SolveMethod` row carrying its lowering and policy; a new material assignment is one `MaterialField` case; a new time scheme is one `TimeIntegrator` row; a new coupling discipline is one `CouplingScheme` row with a `FieldTransfer`; a new lane budget is one `SolvePolicy` column the criterion stack reads through `IterationPolicy.Of`; a new reduction budget is one `CondensationPolicy` column. `CfdSolver`/`ThermalSolver`/`FeaSolver`, `NewmarkSolver`/`GeneralizedAlphaSolver`, `ArcLengthSolver`, and `FsiCoupler`/`ThermalStructuralCoupler` siblings collapse onto `SolveLane`/`TimeIntegrator`/`CoupledLane`.
- Boundary: one `Solve` owns every physics, boundary-condition, element, payload, and time-scheme combination. `PhysicsKind.Operator` must match the `PhysicsPayload` case, and admission rejects cardinality, range, or coefficient failures before assembly. `ConstraintMethod` mutates both operator and right-hand side. `SolveProblem.ContentKey` hashes the full mesh, payload, conditions, members, and constitutive law. `SparseOps.Ingest` consumes assembled COO data, `SparseOps.Factor`/`FactoredOp.Solve`/`SolveIterative` own sparse solution, and `DenseOps.Decompose` owns the modal terminal.
- Boundary: static condensation is the modal lane's ONE sparse lowering and it composes owners rather than minting an eigensolver: the condensed block factors through `FactorKind.Spd`, the transformation columns ride `FactoredOp.Solve` under its own true-residual witness, the block sweeps ride the held `SparseTensorOps.Spmv` in both directions so no transposed storage materializes, and the retained pencil terminates on `Admission.Definite`, `DenseOps.Decompose`, and `SpectralOps.Decompose` — the dense funnel the two-funnel adjudication assigns to this lane, reached with no storage round-trip because the reduced blocks are already `Matrix<double>`. Kernel `Numerics/matrix` generalized-eigen rivals none of that: it densifies both operands at full order and factors the WHOLE inertia operator, so it neither scales at building order nor admits a singular lumped mass, and stays the small-model owner it was built as.
- Boundary: lumped inertia is REAL mass for the elastic forms and a lumped-mass frame carries no rotary inertia, so the rotational degrees of freedom hold exact zeros: those zeros are what make their condensation spectrally exact, they are what the retained-set election reads, and they are why the mass-normalizing full-operator route and the inertia-dividing explicit march each refuse a mass-singular free row by name instead of scaling or freezing it. Under a named refusal every surviving row is inertia-bearing, so the reciprocal-root scaling, the explicit division, and the cell extent each stand unfloored — a rescue floor beneath a refusal fabricates the quantity the refusal exists to catch.
- Boundary: the scalar forms carry the dual storage fact. `MaterialField.CapacityAt` is the ONE per-cell volumetric-heat-capacity read the lumped scatter scales by, the seam supplying `ρ` from its mechanical case and `c_p` from its thermal case, while an energy network overrides the whole vector with the measured nodal capacity its payload carries; capacity never enters the `Lower` coefficient, because storage rides the mass fold and a capacity folded into `D` adds a term to the steady-state operator that has no business there. Capacity is in the canonical bytes, so two runs differing only in heat capacity key apart.
- Boundary: modal participation is ONE formula both routes call — `Γ_d = Σ_i m_i·φ_i·r_{d,i}` over the translational rows of axis `d` against mass-normalized modes, so `Γ_d²` is the effective mass in that direction and `ExcitableMass` is the completeness bound it divides by. Both routes deliver full-length mass-normalized modes, so neither the whole-operator `1/√m` back-scaling nor the condensed `L⁻ᵀ` back-map leaves a route-dependent scale on the reported factor; buckling reports none, because a load factor has no modal mass.
- Boundary: contact is nonlinear-only. Its ids name the base dof of a translational triple, the gap projects onto the constraint normal, and ONE `ContactEnforcement.Enforce` per residual evaluation returns the force, the gap-space stiffness, and the advanced multipliers. Both derivative legs project through `∂g/∂u = ±w·n`: the force scatters over the triple and the stiffness scatters as `h·n⊗n` over the pair's four blocks through a re-ingest, because the elastic sparsity holds no slot for a coupling no element makes. Residuals that augment force while their tangent stays elastic converge at first order under a receipt that reports Newton.
- Boundary: every dense terminal is ceilinged and every ceiling refuses by name with its measured quantity — `SolvePolicy.MaxDenseDofs` over the whole-operator modal and buckling routes (the modal refusal naming the condensed route that does serve the model), `CondensationPolicy.MaxTransformBytes` over the `retained × condensed` transformation the reduction cannot stream, and `SolveProblem.MaxDenseNetworkNodes` over the quadratic radiosity and network payloads at admission. Allocations the machine answers with an out-of-memory leave a receipt that explains nothing.
- Boundary: `CoupledLane` transfers fields under Aitken Δ² relaxation, and `SolveAdaptive` records each recovery rung. `FieldTransfer` binds explicit `(donor, receiver)` index pairs range-checked at both ends against independently discretized fields, so a positional map that silently re-homes a boundary value and a zero-fill that publishes an untransferred slot as measured are both refusals.
- Boundary: `MeshAdjointSnapshot.Of(MeshSpace)` runs at the composing call site and `DesignProblem.DesignMesh` receives the snapshot pre-built, because kernel `MeshSpace` never enters an interior Compute signature; the operator rows are `DesignVariable.AdjointOperator` lowered against `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryAdjoint.Rows`, so this lane composes a tape and never records one.
- Boundary: wall budget and cancellation are `SolvePolicy` columns the lane composes onto a canonical row and reach the criterion stack through `IterationPolicy.Of` beside the `Solve` argument clock, so every iterative leg bounds wall time off the one clock the receipt durations read; a canonical row binding a clock static, a deadline parameter grown onto the entry signature, or a per-leg literal cap is the rejected form, and each attempt re-anchors its own window so a relax rung retries against a fresh budget rather than an expired one.

```csharp signature
[SmartEnum]
public sealed partial class MaterialForm {
    public static readonly MaterialForm Elasticity = new(dof: 3, strainDim: 6);
    public static readonly MaterialForm Isotropic = new(dof: 1, strainDim: 3);
    public static readonly MaterialForm MixedFlow = new(dof: 4, strainDim: 10);
    public static readonly MaterialForm MaxwellEddy = new(dof: 6, strainDim: 6);

    public int Dof { get; }
    public int StrainDim { get; }
}

[SmartEnum]
public sealed partial class OperatorForm {
    public static readonly OperatorForm Continuum = new();
    public static readonly OperatorForm Flow = new();
    public static readonly OperatorForm Radiosity = new();
    public static readonly OperatorForm EnergyNetwork = new();
    public static readonly OperatorForm Helmholtz = new();
    public static readonly OperatorForm EddyCurrent = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PhysicsPayload {
    private PhysicsPayload() { }

    public sealed record Continuum : PhysicsPayload;
    public sealed record Flow(ReadOnlyMemory<double> Velocity, double Density, double Viscosity, double PressureStabilization) : PhysicsPayload;
    public sealed record Radiosity(ReadOnlyMemory<double> ViewFactors, ReadOnlyMemory<double> Reflectance) : PhysicsPayload;
    public sealed record EnergyNetwork(ReadOnlyMemory<double> Capacity, ReadOnlyMemory<double> Conductance) : PhysicsPayload;
    public sealed record Helmholtz(double WaveNumber) : PhysicsPayload;
    public sealed record EddyCurrent(double Permeability, double Conductivity, double AngularFrequency) : PhysicsPayload;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhysicsKind {
    public static readonly PhysicsKind FeaStatic = new("fea-static", symmetric: true, eigen: false, transient: false, nonlinear: false, MaterialForm.Elasticity, OperatorForm.Continuum);
    public static readonly PhysicsKind FeaModal = new("fea-modal", symmetric: true, eigen: true, transient: false, nonlinear: false, MaterialForm.Elasticity, OperatorForm.Continuum);
    public static readonly PhysicsKind FeaTransient = new("fea-transient", symmetric: true, eigen: false, transient: true, nonlinear: false, MaterialForm.Elasticity, OperatorForm.Continuum);
    public static readonly PhysicsKind FeaBuckling = new("fea-buckling", symmetric: true, eigen: true, transient: false, nonlinear: false, MaterialForm.Elasticity, OperatorForm.Continuum);
    public static readonly PhysicsKind CfdIncompressible = new("cfd-incompressible", symmetric: false, eigen: false, transient: true, nonlinear: false, MaterialForm.MixedFlow, OperatorForm.Flow);
    public static readonly PhysicsKind ThermalSteady = new("thermal-steady", symmetric: true, eigen: false, transient: false, nonlinear: false, MaterialForm.Isotropic, OperatorForm.Continuum);
    public static readonly PhysicsKind ThermalTransient = new("thermal-transient", symmetric: true, eigen: false, transient: true, nonlinear: false, MaterialForm.Isotropic, OperatorForm.Continuum);
    public static readonly PhysicsKind DaylightRadiosity = new("daylight-radiosity", symmetric: false, eigen: false, transient: false, nonlinear: false, MaterialForm.Isotropic, OperatorForm.Radiosity);
    public static readonly PhysicsKind EnergyBalance = new("energy-balance", symmetric: true, eigen: false, transient: true, nonlinear: false, MaterialForm.Isotropic, OperatorForm.EnergyNetwork);
    public static readonly PhysicsKind AcousticHelmholtz = new("acoustic-helmholtz", symmetric: true, eigen: false, transient: false, nonlinear: false, MaterialForm.Isotropic, OperatorForm.Helmholtz);
    public static readonly PhysicsKind ElectromagneticEddy = new("electromagnetic-eddy", symmetric: false, eigen: false, transient: false, nonlinear: false, MaterialForm.MaxwellEddy, OperatorForm.EddyCurrent);

    public bool Symmetric { get; }
    public bool Eigen { get; }
    public bool Transient { get; }
    public bool Nonlinear { get; }
    public MaterialForm Form { get; }
    public OperatorForm Operator { get; }

    public int Dof => Form.Dof;
    public int StrainDim => Form.StrainDim;

    public double[] Material(double scale, double poisson, PhysicsPayload payload) =>
        Form.Switch(
            state: (Scale: scale, Poisson: poisson, Payload: payload),
            elasticity: static state => Elasticity(state.Scale, state.Poisson),
            isotropic: static state => Isotropic(state.Scale),
            mixedFlow: static state => FlowMaterial(state.Payload),
            maxwellEddy: static state => EddyMaterial(state.Payload));

    static double[] Elasticity(double e, double nu) {
        double lambda = e * nu / ((1 + nu) * (1 - 2 * nu)), mu = e / (2 * (1 + nu));
        return [lambda + 2 * mu, lambda, lambda, 0, 0, 0, lambda, lambda + 2 * mu, lambda, 0, 0, 0, lambda, lambda, lambda + 2 * mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu];
    }
    static double[] Isotropic(double diagonal) => [diagonal, 0, 0, 0, diagonal, 0, 0, 0, diagonal];

    static double[] FlowMaterial(PhysicsPayload payload) => payload is PhysicsPayload.Flow flow
        ? Matrix(10, (row, column) => row == column && row < 9 ? flow.Viscosity
            : row == 9 && column == 9 ? flow.PressureStabilization
            : row == 9 && column is 0 or 4 or 8 || column == 9 && row is 0 or 4 or 8 ? -1.0 : 0.0)
        : throw new InvalidOperationException("<physics-flow-payload>");

    static double[] EddyMaterial(PhysicsPayload payload) => payload is PhysicsPayload.EddyCurrent eddy
        ? Matrix(6, (row, column) => row == column ? 1.0 / eddy.Permeability : 0.0)
        : throw new InvalidOperationException("<physics-eddy-payload>");

    static double[] Matrix(int size, Func<int, int, double> cell) {
        double[] values = new double[size * size];
        for (int row = 0; row < size; row++) for (int column = 0; column < size; column++) { values[row * size + column] = cell(row, column); }
        return values;
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
    public static readonly SolveMethod ArcLength = new("arc-length", iterative: true, kind: FactorizationKind.Lu, krylov: IterativeMethod.MlkBiCgStab, preconditioner: Preconditioner.Diagonal);
    public static readonly SolveMethod DenseEvd = new("dense-evd", iterative: false, kind: FactorizationKind.Evd, krylov: null, preconditioner: Preconditioner.None);
    // Both eigen rows terminate on the SAME dense decomposition, so `Kind` agrees; what the condensed row names is
    // its LOWERING — a sparse static reduction ahead of that terminal — which the receipt method key must report
    // because a run that reduced its pencil and one that solved the whole operator are different derivations.
    public static readonly SolveMethod CondensedEvd = new("condensed-evd", iterative: false, kind: FactorizationKind.Evd, krylov: null, preconditioner: Preconditioner.None);

    public bool Iterative { get; }
    public FactorizationKind Kind { get; }
    public Preconditioner Preconditioner { get; }
    private readonly IterativeMethod? krylov;

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
    public static readonly TimeIntegrator BackwardEuler = new("backward-euler", alphaMass: 0.0, alphaForce: 0.0, beta: 1.0, gamma: 1.0, implicit: true);
    public static readonly TimeIntegrator NewmarkBeta = new("newmark-beta", alphaMass: 0.0, alphaForce: 0.0, beta: 0.25, gamma: 0.5, implicit: true);
    public static readonly TimeIntegrator GeneralizedAlpha = new("generalized-alpha", alphaMass: 1.0 / 3.0, alphaForce: 4.0 / 9.0, beta: 25.0 / 81.0, gamma: 11.0 / 18.0, implicit: true);
    public static readonly TimeIntegrator CentralDifference = new("central-difference", alphaMass: 0.0, alphaForce: 0.0, beta: 0.0, gamma: 0.5, implicit: false);

    public double AlphaMass { get; }
    public double AlphaForce { get; }
    public double Beta { get; }
    public double Gamma { get; }
    public bool Implicit { get; }

    public double[] Effective(ReadOnlySpan<double> mass, ReadOnlySpan<double> damping, ReadOnlySpan<double> stiffness, double dt) {
        double[] effective = new double[stiffness.Length];
        double massFactor = (1.0 - AlphaMass) / (Beta * dt * dt), dampingFactor = (1.0 - AlphaForce) * Gamma / (Beta * dt);
        for (int i = 0; i < effective.Length; i++) {
            effective[i] = mass[i] * massFactor + damping[i] * dampingFactor + stiffness[i] * (1.0 - AlphaForce);
        }
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
    // Contact carries the constitutive-owned ContactConstraint discriminant beside its dof pairing — the normal,
    // base gap, regularization, and mortar weights live on the constraint, and enforcement is the ONE
    // ContactEnforcement.Enforce fold consumed per nonlinear residual evaluation; a contract-local constant
    // penalty force is the deleted split-brain form. Each id names the BASE dof of a node's translational triple
    // because the gap projects onto the constraint normal, so a pair binds six rows and not two.
    public sealed record Contact(ContactConstraint Constraint, long[] Slave, long[] Master, double Penalty) : BoundaryCondition;

    public Fin<Unit> Validate(int dofs) =>
        Switch(
            state: dofs,
            dirichlet: static (n, bc) => bc.Nodes.Length == bc.Values.Length && bc.Nodes.Length > 0 && InRange(bc.Nodes, n) && bc.Values.All(double.IsFinite)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.ModelRejected("<boundary-dirichlet-shape>")),
            neumann: static (n, bc) => bc.Faces.Length == bc.Flux.Length && bc.Faces.Length > 0 && InRange(bc.Faces, n) && bc.Flux.All(double.IsFinite)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.ModelRejected("<boundary-neumann-shape>")),
            robin: static (n, bc) => bc.Faces.Length > 0 && InRange(bc.Faces, n) && double.IsFinite(bc.Coefficient) && double.IsFinite(bc.Ambient)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.ModelRejected("<boundary-robin-shape>")),
            periodic: static (n, bc) => bc.Master.Length == bc.Slave.Length && bc.Master.Length > 0 && InRange(bc.Master, n) && InRange(bc.Slave, n)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.ModelRejected("<boundary-periodic-shape>")),
            contact: static (n, bc) => bc.Master.Length == bc.Slave.Length && bc.Master.Length > 0 && Triples(bc.Master, n) && Triples(bc.Slave, n) && double.IsFinite(bc.Penalty) && bc.Penalty > 0.0
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.ModelRejected("<boundary-contact-shape>")));

    public Fin<ConstrainedSystem> Apply(ConstrainedSystem system, ConstraintMethod constraint) =>
        constraint == ConstraintMethod.Lagrange ? ApplyBordered(system) : Fin.Succ(ApplyFixed(system, constraint));

    ConstrainedSystem ApplyFixed(ConstrainedSystem system, ConstraintMethod constraint) =>
        Switch(
            state: (System: system, Constraint: constraint),
            dirichlet: static (s, bc) => {
                bool penalty = s.Constraint == ConstraintMethod.Penalty;
                double[] rhs = (double[])s.System.Rhs.Clone();
                double[] values = (double[])s.System.Operator.Values.Clone();
                int[] rowPtr = s.System.Operator.RowPointers, colIdx = s.System.Operator.ColumnIndices;
                LanguageExt.HashSet<long> fixedDofs = s.System.Constrained;
                int n = s.System.Operator.RowCount;
                for (int i = 0; i < bc.Nodes.Length; i++) {
                    int node = (int)bc.Nodes[i];
                    if (penalty) {
                        Augment(values, rowPtr, colIdx, node, node, s.System.Penalty);
                        rhs[node] = s.System.Penalty * bc.Values[i];
                    } else {
                        for (int row = 0; row < n; row++)
                            for (int slot = rowPtr[row]; slot < rowPtr[row + 1]; slot++) {
                                if (row == node) { values[slot] = colIdx[slot] == node ? 1.0 : 0.0; }
                                else if (colIdx[slot] == node) { rhs[row] -= values[slot] * bc.Values[i]; values[slot] = 0.0; }
                            }
                        rhs[node] = bc.Values[i];
                    }
                    fixedDofs = fixedDofs.Add(bc.Nodes[i]);
                }
                return s.System with { Operator = Rebuilt(s.System.Operator, values), Rhs = rhs, Constrained = fixedDofs };
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
                double penalty = s.System.Penalty;
                double[] values = (double[])s.System.Operator.Values.Clone();
                int[] rowPtr = s.System.Operator.RowPointers, colIdx = s.System.Operator.ColumnIndices;
                LanguageExt.HashSet<long> fixedDofs = s.System.Constrained;
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
            // Contact contributes NOTHING to the linear system: it is nonlinear-only, enforced per residual
            // evaluation through the constitutive ContactEnforcement owner with current kinematics and the
            // step's committed multipliers — a precomputed constant force is the deleted form.
            contact: static (s, _) => s.System);

    Fin<ConstrainedSystem> ApplyBordered(ConstrainedSystem system) =>
        Switch(
            state: system,
            dirichlet: static (current, bc) => Border(current, bc.Nodes, [], bc.Values),
            neumann: static (current, bc) => Fin.Succ(((BoundaryCondition)bc).ApplyFixed(current, ConstraintMethod.Elimination)),
            robin: static (current, bc) => Fin.Succ(((BoundaryCondition)bc).ApplyFixed(current, ConstraintMethod.Elimination)),
            periodic: static (current, bc) => Border(current, bc.Master, bc.Slave, new double[bc.Master.Length]),
            contact: static (current, bc) => Fin.Succ(((BoundaryCondition)bc).ApplyFixed(current, ConstraintMethod.Elimination)));

    static Fin<ConstrainedSystem> Border(ConstrainedSystem system, long[] positive, long[] negative, double[] prescribed) {
        int original = system.Operator.RowCount, constraints = positive.Length, dimension = original + constraints;
        List<int> rows = new(system.Operator.Values.Length + constraints * (negative.Length == 0 ? 2 : 4));
        List<int> columns = new(rows.Capacity);
        List<double> values = new(rows.Capacity);
        for (int row = 0; row < original; row++) {
            for (int slot = system.Operator.RowPointers[row]; slot < system.Operator.RowPointers[row + 1]; slot++) {
                rows.Add(row); columns.Add(system.Operator.ColumnIndices[slot]); values.Add(system.Operator.Values[slot]);
            }
        }
        for (int constraint = 0; constraint < constraints; constraint++) {
            int multiplier = original + constraint, plus = checked((int)positive[constraint]);
            rows.Add(plus); columns.Add(multiplier); values.Add(1.0);
            rows.Add(multiplier); columns.Add(plus); values.Add(1.0);
            if (negative.Length != 0) {
                int minus = checked((int)negative[constraint]);
                rows.Add(minus); columns.Add(multiplier); values.Add(-1.0);
                rows.Add(multiplier); columns.Add(minus); values.Add(-1.0);
            }
        }
        double[] rhs = new double[dimension];
        system.Rhs.CopyTo(rhs, 0);
        prescribed.CopyTo(rhs, original);
        return SparseOps.Ingest(SparseFormat.Coo, dimension, dimension, [.. rows], [.. columns], [.. values])
            .Map(operatorCsr => system with { Operator = operatorCsr, Rhs = rhs });
    }

    static bool InRange(long[] indices, int dofs) => indices.All(index => index >= 0 && index < dofs);

    // Contact ids name the base dof of a translational triple, so the whole triple must lie in range rather than the
    // one row the id spells — a pair passing an index check on its base alone scatters its normal components
    // off the end of the operator.
    static bool Triples(long[] bases, int dofs) => bases.All(index => index >= 0 && index + 2 < dofs);

    public void WriteCanonical(ArrayBufferWriter<byte> sink) =>
        Switch(
            state: sink,
            dirichlet: static (w, bc) => Digest(w, (byte)'d', bc.Nodes, bc.Values),
            neumann: static (w, bc) => Digest(w, (byte)'n', bc.Faces, bc.Flux),
            robin: static (w, bc) => Digest(w, (byte)'r', bc.Faces, [bc.Coefficient, bc.Ambient]),
            periodic: static (w, bc) => Digest(w, (byte)'p', [.. bc.Master, .. bc.Slave], []),
            contact: static (w, bc) => Digest(w, (byte)'c', [.. bc.Slave, .. bc.Master],
                [bc.Constraint.Normal.X, bc.Constraint.Normal.Y, bc.Constraint.Normal.Z, bc.Constraint.BaseGap, bc.Constraint.Regularization, bc.Penalty]));

    static void Digest(ArrayBufferWriter<byte> sink, byte tag, long[] ids, double[] values) {
        Span<byte> scratch = stackalloc byte[8];
        sink.Write([tag]);
        BinaryPrimitives.WriteInt32LittleEndian(scratch, ids.Length); sink.Write(scratch[..4]);
        foreach (long id in ids) { BinaryPrimitives.WriteInt64LittleEndian(scratch, id); sink.Write(scratch); }
        foreach (double v in values) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, v); sink.Write(scratch); }
    }

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

public sealed record ArcLengthPolicy(double Radius, double LoadScale, int Steps, double ResidualTolerance) {
    public bool Invalid => !double.IsFinite(Radius) || Radius <= 0.0 || !double.IsFinite(LoadScale) || LoadScale <= 0.0 || Steps < 1 || !double.IsFinite(ResidualTolerance) || ResidualTolerance <= 0.0;
}

// Static-condensation budget: the retained-set ceiling above which the reduction has bought nothing the dense
// terminal can afford (the terminal is cubic in the retained count, so the ceiling is a real budget, not a taste), the
// relative-residual cap the per-column slave equilibrium and the block reduction witness both gate on, and the
// byte ceiling on the dense `Ψ` transformation — `retained × condensed × sizeof(double)` is the one allocation the
// reduction cannot stream, and a model that clears the retained cap can still ask for a transform larger than the
// machine. The RETAINED SET itself is no column here — it derives from the inertia the model carries, so a caller
// enumerating a master set is the deleted form and the three rows below are the only reduction knobs that survive.
public sealed record CondensationPolicy(int MaxRetained, double ResidualCap, long MaxTransformBytes) {
    public static readonly CondensationPolicy Canonical = new(MaxRetained: 1_024, ResidualCap: 1e-8, MaxTransformBytes: 512L * 1024 * 1024);

    public bool Invalid => MaxRetained < 1 || !double.IsFinite(ResidualCap) || ResidualCap <= 0.0 || MaxTransformBytes < 1;
}

// Measured reduction receipt: the retained and condensed counts, the slave-equilibrium block residual
// `‖K_ss·Ψ + K_sm‖_F / ‖K_sm‖_F` recomputed against the ORIGINAL blocks, and the retained pencil's condition number
// off one held `Svd(false)` handle. Every column is measured where the reduction happens — a route that could not
// measure one of them refuses rather than stamping a zero a consumer would read as evidence.
public readonly record struct CondensationEvidence(int Retained, int Condensed, double Residual, double Conditioning);

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
    double PenaltyFactor,
    // Rayleigh damping is the PAIR `C = αM + βK`, never a single stiffness-proportional constant: the mass term
    // damps the low modes a building's own inertia carries and the stiffness term the high modes, so a lane
    // holding one of them silently changes which end of the spectrum decays. Both marches read the same pair — the
    // implicit scheme folds it whole into the effective operator, the explicit scheme keeps the mass leg in
    // its diagonal operator and evaluates the stiffness leg at the lagged half-step velocity, so its own operator
    // never leaves the diagonal.
    double DampingAlpha,
    double DampingBeta,
    // Dense-terminal ceiling in degrees of freedom: the whole-operator modal and buckling routes both densify at
    // full order, so the allocation is quadratic and the factorization cubic in this number. The ceiling refuses
    // by NAME above it rather than attempting an allocation the machine answers with an out-of-memory the receipt
    // cannot explain, and the modal refusal names the condensed route that does serve the model.
    int MaxDenseDofs,
    Option<ArcLengthPolicy> Continuation,
    Option<CondensationPolicy> Condensation,
    // Wall budget and cooperative token are policy VALUES the lane composes onto a canonical row, because the
    // iterative criterion stack reads clock, deadline, and token together through `IterationPolicy.Of`. Canonical
    // rows declare the unbounded wall and the never-cancelled token so the numeric canon stays lane-free, while
    // CLOCK stays the `Solve` argument: a canonical row binding a clock static mints a second clock the receipt
    // durations never read.
    Duration Deadline,
    CancellationToken Cancel) {
    public static readonly SolvePolicy CanonicalStatic = new(SolveMethod.DirectCholesky, ConstraintMethod.Elimination, TimeIntegrator.BackwardEuler, MaxIterations: 1, Tolerance: 1e-9, EigenPairs: 0, TimeStep: 0.0, TimeSteps: 1, NewtonIterations: 1, PenaltyFactor: 1e12, DampingAlpha: 0.0, DampingBeta: 0.05, MaxDenseDofs: 20_000, Continuation: None, Condensation: None, Deadline: Duration.MaxValue, Cancel: CancellationToken.None);
    public static readonly SolvePolicy CanonicalIterative = CanonicalStatic with { Method = SolveMethod.BiCgStab, MaxIterations = 2000, Tolerance = 1e-8 };
    public static readonly SolvePolicy CanonicalModal = CanonicalStatic with { Method = SolveMethod.DenseEvd, MaxIterations = 500, Tolerance = 1e-7, EigenPairs = 12 };
    // Condensed modal is the SAME canonical modal row plus its reduction — every eigen column (pair count,
    // tolerance) is inherited so the two routes are comparable, and the reduction is the one thing that differs.
    public static readonly SolvePolicy CanonicalModalCondensed = CanonicalModal with { Method = SolveMethod.CondensedEvd, Condensation = Some(CondensationPolicy.Canonical) };
    public static readonly SolvePolicy CanonicalTransient = CanonicalStatic with { Method = SolveMethod.DirectLu, Integrator = TimeIntegrator.NewmarkBeta, TimeStep = 0.01, TimeSteps = 100 };
    public static readonly SolvePolicy CanonicalNonlinear = CanonicalIterative with { Method = SolveMethod.MlkBiCgStab, NewtonIterations = 25 };
    public static readonly SolvePolicy CanonicalArcLength = CanonicalNonlinear with { Method = SolveMethod.ArcLength, Continuation = Some(new ArcLengthPolicy(0.05, 1.0, 40, 1e-7)) };

    public Fin<Unit> Validate(SolveProblem problem) =>
        MaxIterations <= 0 || !double.IsFinite(Tolerance) || Tolerance <= 0.0 || !double.IsFinite(PenaltyFactor) || PenaltyFactor <= 0.0 || Continuation.Exists(static continuation => continuation.Invalid) || Condensation.Exists(static condensation => condensation.Invalid)
            ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-iteration>"))
            : !double.IsFinite(DampingAlpha) || DampingAlpha < 0.0 || !double.IsFinite(DampingBeta) || DampingBeta < 0.0
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<solve-policy-damping:{DampingAlpha}:{DampingBeta}>"))
            : MaxDenseDofs < 1
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<solve-policy-dense-ceiling:{MaxDenseDofs}>"))
            : Deadline <= Duration.Zero
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<solve-policy-budget:{Deadline}>"))
            : (Method == SolveMethod.ArcLength) != Continuation.IsSome
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-continuation-discriminant>"))
            // Reduction rides the same method-versus-payload discriminant the continuation rides: the row and its
            // policy travel together, so a `condensed-evd` key can never label a run that carried no reduction and a
            // reduction can never run under a method key the receipt reports as the whole-operator route.
            : (Method == SolveMethod.CondensedEvd) != Condensation.IsSome
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-condensation-discriminant>"))
            : problem.Physics.Eigen && EigenPairs <= 0
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-eigen-pairs>"))
                : problem.Physics.Transient && (TimeSteps <= 0 || !double.IsFinite(TimeStep) || TimeStep <= 0.0)
                    ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-time-grid>"))
                    : (problem.Physics.Nonlinear || problem.Material.IsSome) && (NewtonIterations <= 0 || Method.Krylov.IsNone)
                        ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-newton-inner-method>"))
                        : Method.Kind == FactorizationKind.Cholesky && !problem.Physics.Symmetric
                            ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-symmetry-method>"))
                            : Constraint == ConstraintMethod.Lagrange && (problem.Physics.Eigen || problem.Physics.Transient || Method.Kind == FactorizationKind.Cholesky)
                                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-lagrange-method>"))
                                : Fin.Succ(unit);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialField {
    private MaterialField() { }

    // Elastic cases carry DENSITY beside the two elastic constants because inertia is a material fact the seam
    // already supplies: `Mechanical` holds it, so dropping it here forces every dynamic and modal arm to fabricate a
    // mass from geometry alone — a lumped vector in metres reported as kilogrammes. Scalar cases carry the dual
    // fact: `Capacity` is the volumetric heat capacity `ρ·c_p` in J/(m³·K), the seam `Thermal` case supplying the
    // specific heat and `Mechanical` the density, because a first-order march reading a bare geometric volume as a
    // capacity advances every diffusion problem at the wrong rate and reports the wrong steady state as converged.
    public sealed record UniformElastic(double Young, double Poisson, double Density) : MaterialField;
    public sealed record UniformScalar(double Scale, double Capacity) : MaterialField;
    public sealed record PerCellElastic(ImmutableArray<double> Young, ImmutableArray<double> Poisson, ImmutableArray<double> Density) : MaterialField;
    public sealed record PerCellScalar(ImmutableArray<double> Scale, ImmutableArray<double> Capacity) : MaterialField;

    public static readonly MaterialField Unit = new UniformScalar(1.0, 1.0);

    public static Fin<MaterialField> OfMechanical(Seq<Option<MaterialPropertySet.Mechanical>> perCell) =>
        perCell.Traverse(static row => row.ToFin(new ComputeFault.AssessmentInputMissing("<material-field:member-without-mechanical-case>")))
            .Map(static rows => (MaterialField)new PerCellElastic(
                [.. rows.Map(static m => m.YoungsModulus.Si)],
                [.. rows.Map(static m => m.PoissonsRatio)],
                [.. rows.Map(static m => m.Density.Si)]))
            .As();

    // ONE read answers every per-cell constitutive question the assembly and the inertia fold both ask, so a second
    // density-only accessor beside it is the deleted hop.
    public Fin<(double Young, double Poisson, double Density)> MechanicalAt(int cell) =>
        Switch(
            state: cell,
            uniformElastic: static (_, assignment) => Fin.Succ((assignment.Young, assignment.Poisson, assignment.Density)),
            uniformScalar: static (_, _) => Fin.Fail<(double, double, double)>(new ComputeFault.ModelRejected("<frame-requires-elastic-material>")),
            perCellElastic: static (index, assignment) => Fin.Succ((assignment.Young[index], assignment.Poisson[index], assignment.Density[index])),
            perCellScalar: static (_, _) => Fin.Fail<(double, double, double)>(new ComputeFault.ModelRejected("<frame-requires-elastic-material>")));

    // `CapacityAt` is the scalar dual of `MechanicalAt`: ONE read answers the volumetric-heat-capacity question the
    // lumped scatter asks per cell. The elastic cases refuse rather than returning a substitute, because their storage term is
    // density and `MechanicalAt` already carries it — a case answering both reads makes the two indistinguishable.
    public Fin<double> CapacityAt(int cell) =>
        Switch(
            state: cell,
            uniformElastic: static (_, _) => Fin.Fail<double>(new ComputeFault.ModelRejected("<capacity-requires-scalar-material>")),
            uniformScalar: static (_, assignment) => Fin.Succ(assignment.Capacity),
            perCellElastic: static (_, _) => Fin.Fail<double>(new ComputeFault.ModelRejected("<capacity-requires-scalar-material>")),
            perCellScalar: static (index, assignment) => Fin.Succ(assignment.Capacity[index]));

    public Fin<Unit> Validate(long cells, MaterialForm form) {
        bool elastic = form == MaterialForm.Elasticity;
        bool valid = this switch {
            UniformElastic assignment => elastic && Positive(assignment.Young) && PoissonValid(assignment.Poisson) && Positive(assignment.Density),
            UniformScalar assignment => !elastic && Positive(assignment.Scale) && Positive(assignment.Capacity),
            PerCellElastic assignment => elastic && assignment.Young.Length == cells && assignment.Poisson.Length == cells && assignment.Density.Length == cells && assignment.Young.All(Positive) && assignment.Poisson.All(PoissonValid) && assignment.Density.All(Positive),
            PerCellScalar assignment => !elastic && assignment.Scale.Length == cells && assignment.Capacity.Length == cells && assignment.Scale.All(Positive) && assignment.Capacity.All(Positive),
            _ => false,
        };
        return valid ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<material-field:{form}:{cells}>"));
    }

    // Lowering answers the OPERATOR coefficient alone: the scalar arms lower their conductance and the capacity
    // never enters, because storage rides the mass fold rather than the stiffness fold and a capacity folded into
    // `D` would add a term to the steady-state operator that has no business there.
    public Func<int, double[]> Lower(PhysicsKind physics, PhysicsPayload payload) =>
        Switch(
            state: (Physics: physics, Payload: payload),
            uniformElastic: static (row, assignment) => Cached(row.Physics.Material(assignment.Young, assignment.Poisson, row.Payload)),
            uniformScalar: static (row, assignment) => Cached(row.Physics.Material(assignment.Scale, 0.0, row.Payload)),
            perCellElastic: static (row, assignment) => cell => row.Physics.Material(assignment.Young[cell], assignment.Poisson[cell], row.Payload),
            perCellScalar: static (row, assignment) => cell => row.Physics.Material(assignment.Scale[cell], 0.0, row.Payload));

    static Func<int, double[]> Cached(double[] d) => _ => d;

    // Capacity is IN the canonical bytes, so the scalar cases re-key: two runs differing only in heat capacity are
    // different derivations with different transient answers, and a key blind to the column would serve one run's
    // cached result to the other.
    public void WriteCanonical(ArrayBufferWriter<byte> sink) =>
        Switch(
            state: sink,
            uniformElastic: static (writer, assignment) => {
                writer.Write("ue"u8);
                WriteScalars(writer, [assignment.Young, assignment.Poisson, assignment.Density]);
            },
            uniformScalar: static (writer, assignment) => {
                writer.Write("us"u8);
                WriteScalars(writer, [assignment.Scale, assignment.Capacity]);
            },
            perCellElastic: static (writer, assignment) => {
                writer.Write("pe"u8);
                Span<byte> count = stackalloc byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(count, assignment.Young.Length); writer.Write(count);
                WriteScalars(writer, assignment.Young); WriteScalars(writer, assignment.Poisson); WriteScalars(writer, assignment.Density);
            },
            perCellScalar: static (writer, assignment) => {
                writer.Write("ps"u8);
                Span<byte> count = stackalloc byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(count, assignment.Scale.Length); writer.Write(count);
                WriteScalars(writer, assignment.Scale); WriteScalars(writer, assignment.Capacity);
            });

    static bool Positive(double value) => double.IsFinite(value) && value > 0.0;
    static bool PoissonValid(double value) => double.IsFinite(value) && value is > -1.0 and < 0.5;
    static void WriteScalars(ArrayBufferWriter<byte> sink, IEnumerable<double> values) {
        Span<byte> scratch = stackalloc byte[8];
        foreach (double value in values) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, value); sink.Write(scratch); }
    }
}

public sealed record SolveProblem(
    PhysicsKind Physics,
    ElementClass Element,
    Seq<BoundaryCondition> Conditions,
    FieldSpace Unknown,
    MaterialField Field,
    PhysicsPayload Payload,
    ImmutableArray<FrameMember> Members,
    Option<(ConstitutiveModel Model, MaterialParameters Law)> Material,
    UInt128 ContentKey) {
    // Radiosity view factors and network conductance are DENSE `n × n` payloads and the operator they lower is a
    // dense `n × n` triplet fill, so both the payload and the assembly are quadratic in the node count and the
    // ceiling is the honest bound on the dense form rather than a taste. A model above it needs a clustered or
    // hierarchical view-factor route this lane does not own, so it refuses by name with its measured node count
    // instead of attempting an allocation two orders of magnitude past the machine.
    public const int MaxDenseNetworkNodes = 4_096;

    public int Dof => Element.Family == ShapeFamily.Frame ? 6 : Physics.Dof;

    public static Fin<SolveProblem> Of(PhysicsKind physics, DiscreteMesh mesh, Seq<BoundaryCondition> conditions, MaterialField field, PhysicsPayload payload, ImmutableArray<FrameMember> members, Option<(ConstitutiveModel Model, MaterialParameters Law)> material) {
        ImmutableArray<FrameMember> rows = members;
        bool frame = mesh.Element.Family == ShapeFamily.Frame;
        int dof = frame ? 6 : physics.Dof;
        return frame && rows.Length != mesh.ElementCount
            ? Fin.Fail<SolveProblem>(new ComputeFault.ModelRejected($"<solve-frame-member-count:{rows.Length}≠{mesh.ElementCount}>"))
            : PayloadValid(physics, payload, mesh).Bind(_ => field.Validate(mesh.ElementCount, physics.Form)).Map(_ => new SolveProblem(
                physics, mesh.Element, conditions, mesh.FieldOf(FieldStation.Nodal, dof == 1 ? 0 : 1, dof), field, payload, rows, material,
                Key(physics, mesh, conditions, field, payload, rows, material)));
    }

    static UInt128 Key(PhysicsKind physics, DiscreteMesh mesh, Seq<BoundaryCondition> conditions, MaterialField field, PhysicsPayload payload, ImmutableArray<FrameMember> members, Option<(ConstitutiveModel Model, MaterialParameters Law)> material) {
        ArrayBufferWriter<byte> sink = new(256);
        Span<byte> scratch = stackalloc byte[8];
        void WriteLong(long v) { BinaryPrimitives.WriteInt64LittleEndian(scratch, v); sink.Write(scratch); }
        void WriteInt(int v) { BinaryPrimitives.WriteInt32LittleEndian(scratch, v); sink.Write(scratch[..4]); }
        sink.Write(Encoding.UTF8.GetBytes(physics.Key));
        sink.Write(Encoding.UTF8.GetBytes(mesh.Element.Key));
        WriteLong(mesh.NodeCount);
        WriteLong(mesh.ElementCount);
        WriteLong(physics.Dof);
        ReadOnlySpan<float> coordinates = mesh.Coordinates;
        ReadOnlySpan<long> indices = mesh.Indices;
        WriteInt(coordinates.Length);
        foreach (float ordinate in coordinates) { BinaryPrimitives.WriteSingleLittleEndian(scratch, ordinate); sink.Write(scratch[..4]); }
        WriteInt(indices.Length);
        foreach (long node in indices) { WriteLong(node); }
        WriteLong(conditions.Count);
        foreach (BoundaryCondition condition in conditions) { condition.WriteCanonical(sink); }
        field.WriteCanonical(sink);
        WritePayload(sink, payload);
        WriteLong(members.Length);
        foreach (FrameMember member in members) { member.WriteCanonical(sink); }
        sink.Write([(byte)(material.IsSome ? 1 : 0)]);
        material.IfSome(m => { WriteConstitutive(sink, m.Model); WriteLaw(sink, m.Law); });
        return ContentHash.Of(sink.WrittenSpan);
    }

    static Fin<Unit> PayloadValid(PhysicsKind physics, PhysicsPayload payload, DiscreteMesh mesh) {
        int nodes = checked((int)mesh.NodeCount);
        bool dense = physics.Operator == OperatorForm.Radiosity || physics.Operator == OperatorForm.EnergyNetwork;
        if (dense && nodes > MaxDenseNetworkNodes) {
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<physics-dense-network-nodes:{physics.Key}:{nodes}:ceiling={MaxDenseNetworkNodes}>"));
        }
        bool valid = (physics.Operator, payload) switch {
            ({ } form, PhysicsPayload.Continuum) when form == OperatorForm.Continuum => true,
            ({ } form, PhysicsPayload.Flow flow) when form == OperatorForm.Flow => flow.Velocity.Length == mesh.ElementCount * 3 && double.IsFinite(flow.Density) && flow.Density > 0.0 && double.IsFinite(flow.Viscosity) && flow.Viscosity > 0.0 && double.IsFinite(flow.PressureStabilization) && flow.PressureStabilization > 0.0,
            ({ } form, PhysicsPayload.Radiosity radiosity) when form == OperatorForm.Radiosity => radiosity.Reflectance.Length == nodes && radiosity.ViewFactors.Length == nodes * nodes && TensorPrimitives.IsFiniteAll<double>(radiosity.Reflectance.Span) && TensorPrimitives.IsFiniteAll<double>(radiosity.ViewFactors.Span) && radiosity.Reflectance.Span.ToArray().All(static value => value is >= 0.0 and <= 1.0),
            ({ } form, PhysicsPayload.EnergyNetwork energy) when form == OperatorForm.EnergyNetwork => energy.Capacity.Length == nodes && energy.Conductance.Length == nodes * nodes && TensorPrimitives.IsFiniteAll<double>(energy.Capacity.Span) && TensorPrimitives.IsFiniteAll<double>(energy.Conductance.Span),
            ({ } form, PhysicsPayload.Helmholtz wave) when form == OperatorForm.Helmholtz => double.IsFinite(wave.WaveNumber) && wave.WaveNumber > 0.0,
            ({ } form, PhysicsPayload.EddyCurrent eddy) when form == OperatorForm.EddyCurrent => double.IsFinite(eddy.Permeability) && eddy.Permeability > 0.0 && double.IsFinite(eddy.Conductivity) && eddy.Conductivity >= 0.0 && double.IsFinite(eddy.AngularFrequency) && eddy.AngularFrequency > 0.0,
            _ => false,
        };
        return valid ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<physics-payload:{physics.Key}:{payload.GetType().Name}>"));
    }

    static void WritePayload(ArrayBufferWriter<byte> sink, PhysicsPayload payload) {
        Span<byte> scratch = stackalloc byte[8];
        void Write(double value) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, value); sink.Write(scratch); }
        void WriteAll(ReadOnlySpan<double> values) { BinaryPrimitives.WriteInt32LittleEndian(scratch, values.Length); sink.Write(scratch[..4]); foreach (double value in values) { Write(value); } }
        payload.Switch(
            state: sink,
            continuum: static (writer, _) => writer.Write("c"u8),
            flow: (writer, row) => { writer.Write("f"u8); WriteAll(row.Velocity.Span); Write(row.Density); Write(row.Viscosity); Write(row.PressureStabilization); },
            radiosity: (writer, row) => { writer.Write("r"u8); WriteAll(row.ViewFactors.Span); WriteAll(row.Reflectance.Span); },
            energyNetwork: (writer, row) => { writer.Write("e"u8); WriteAll(row.Capacity.Span); WriteAll(row.Conductance.Span); },
            helmholtz: (writer, row) => { writer.Write("h"u8); Write(row.WaveNumber); },
            eddyCurrent: (writer, row) => { writer.Write("m"u8); Write(row.Permeability); Write(row.Conductivity); Write(row.AngularFrequency); });
    }

    static void WriteConstitutive(ArrayBufferWriter<byte> sink, ConstitutiveModel model) =>
        model.Switch(
            state: sink,
            plastic: static (w, m) => WritePlastic(w, m),
            hyperelastic: static (w, m) => WriteHyperelastic(w, m.Law),
            viscoelastic: static (w, m) => TaggedPair(w, (byte)'V', m.PronyTerms, m.TimeStep),
            damage: static (w, m) => TaggedReal(w, (byte)'D', m.Exponent));

    static void WriteLaw(ArrayBufferWriter<byte> sink, MaterialParameters law) {
        Span<byte> scratch = stackalloc byte[8];
        void Write(double v) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, v); sink.Write(scratch); }
        Write(law.YoungModulus); Write(law.PoissonRatio); Write(law.YieldStress); Write(law.HardeningModulus); Write(law.DamageThreshold);
        // Friction, dilation, and cohesion are MATERIAL columns, so they key on every pressure-dependent run rather
        // than only on the runs that carry a critical-state record.
        Write(law.FrictionAngle); Write(law.DilationAngle); Write(law.Cohesion);
        BinaryPrimitives.WriteInt32LittleEndian(scratch, law.Prony.Count); sink.Write(scratch[..4]);
        foreach ((double modulus, double relaxation) in law.Prony) { Write(modulus); Write(relaxation); }
        sink.Write([(byte)(law.Soil.IsSome ? 1 : 0)]);
        law.Soil.IfSome(soil => {
            Write(soil.CriticalStateSlope); Write(soil.CompressionIndex); Write(soil.SwellIndex);
            Write(soil.InitialPreconsolidationPressure); Write(soil.InitialPorePressure);
        });
    }

    static void WritePlastic(ArrayBufferWriter<byte> sink, ConstitutiveModel.Plastic model) {
        Span<byte> scratch = stackalloc byte[8];
        void Write(double value) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, value); sink.Write(scratch); }
        sink.Write([(byte)'P']);
        Write(model.Regularization); Write(model.Potential.MeridianWeight); Write(model.Potential.LodeWeight); Write(model.Potential.CapWeight);
    }

    static void WriteHyperelastic(ArrayBufferWriter<byte> sink, HyperelasticLaw law) {
        Span<byte> scratch = stackalloc byte[8];
        void Write(double value) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, value); sink.Write(scratch); }
        sink.Write([(byte)'H']);
        Write(law.FirstInvariant); Write(law.SecondInvariant); Write(law.FirstInvariantSquared); Write(law.BulkScale);
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

    static void TaggedPair(ArrayBufferWriter<byte> sink, byte tag, int count, double value) {
        Tagged(sink, tag, count);
        Span<byte> scratch = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(scratch, value); sink.Write(scratch);
    }
}

// Modal participation is PER AXIS: `Γ_d = Σ_i m_i·φ_i·r_{d,i}` over the translational degrees of freedom of axis
// `d`, the influence vector `r_d` being one on that axis's translational rows and zero elsewhere. A scalar factor
// summing every axis at once cannot answer the question a seismic check asks — which fraction of the excitable
// mass in THIS direction the retained modes carry — and a frame whose modes are strongly directional reports a
// healthy total while one axis is unrepresented.
public readonly record struct ModalParticipation(double X, double Y, double Z);

public sealed record SolveResult(
    SolveProblem Problem,
    SolveMethod Method,
    ReadOnlyMemory<double> Field,
    Option<ReadOnlyMemory<double>> EigenValues,
    Option<ReadOnlyMemory<ModalParticipation>> Participation,
    Option<ModalParticipation> TotalMass,
    long Dofs,
    int Iterations,
    int NewtonSteps,
    double Residual,
    bool Converged,
    Instant At) {
    // Reduction evidence rides an init member because it is route-borne, not universal: `Option<T>` is total over
    // `default` so no other construction site in the lane changes, and a route that reduced nothing carries `None`
    // rather than a zeroed record every consumer would have to disbelieve.
    public Option<CondensationEvidence> Condensation { get; init; }
}

public sealed record ConstrainedSystem(
    SparseCompressedRowMatrixStorage<double> Operator,
    double[] Rhs,
    LanguageExt.HashSet<long> Constrained,
    double Penalty);

// Route-borne archive CAPABILITY, never a policy value — a sink is a live resource no value record carries.
// Its caller (a scheduling job node or the app root) supplies one fresh pooled stream per requested container, the
// producing route lands its corpus through the one `Runtime/codecs#HDF_ARCHIVE` owner, and the emitted
// artifact rides `SolveResult.Archive` as evidence. Absent, every route runs unchanged. Each container class is
// its own session because their write laws differ: a HISTORY writes [steps, dofs] kinematic datasets one chunk
// per accepted step (monotone by construction — a march is strictly step-ordered); MODES writes [pairs, dofs]
// mode-outermost as recovery produces them; a CHECKPOINT is one whole-graph `H5File.Write` per COMMITTED step
// keyed (ContentKey, step) — create-only forbids a growing checkpoint file, so each commit is its own container
// and `H5Constants.Unlimited` never enters. An archive write fault FAULTS the run: the caller asked for the
// artifact, and a partial container published as evidence is worse than a refused solve.
[SmartEnum<string>]
public sealed partial class SolveArchiveKind {
    public static readonly SolveArchiveKind History = new("history");
    public static readonly SolveArchiveKind Modes = new("modes");
    public static readonly SolveArchiveKind Checkpoint = new("checkpoint");
}

public sealed record SolveArchive(Func<SolveArchiveKind, Stream> Sink, HdfArchivePolicy Policy);

// Null-object session: `Open` over an absent capability returns the inert instance, so integrator folds thread
// ONE call shape with no per-step Option branch on the hot loop.
public sealed class SolveHistory : IDisposable {
    static readonly SolveHistory Inert = new(null, null, null, null, 0, 0);

    readonly HdfWriter? writer;
    readonly H5Dataset<double[]>? u, v, a;
    readonly int steps, dofs;

    SolveHistory(HdfWriter? writer, H5Dataset<double[]>? u, H5Dataset<double[]>? v, H5Dataset<double[]>? a, int steps, int dofs) =>
        (this.writer, this.u, this.v, this.a, this.steps, this.dofs) = (writer, u, v, a, steps, dofs);

    // `kinematic: false` declares the first-order roster — one field dataset, no velocity or acceleration slot —
    // so an absent kinematic is an absent dataset, never a fill-value plane a reader mistakes for stillness.
    public static Fin<SolveHistory> Open(Option<SolveArchive> archive, SolveProblem problem, SolvePolicy policy, int dofs, bool kinematic = true) =>
        archive.Match(
            None: () => Fin.Succ(Inert),
            Some: capability => Try.lift(() => {
                ulong[] dims = [(ulong)policy.TimeSteps, (ulong)dofs];
                uint[] chunks = [1U, (uint)dofs];
                H5DatasetCreation creation = capability.Policy.Creation();
                H5Dataset<double[]> u = new(dims, chunks, datasetCreation: creation);
                H5Dataset<double[]>? v = kinematic ? new(dims, chunks, datasetCreation: creation) : null;
                H5Dataset<double[]>? a = kinematic ? new(dims, chunks, datasetCreation: creation) : null;
                H5File graph = new() { ["u"] = u };
                if (v is not null) { graph["v"] = v; }
                if (a is not null) { graph["a"] = a; }
                graph.Attributes["content-key"] = $"{problem.ContentKey:x32}";
                graph.Attributes["physics"] = problem.Physics.Key;
                graph.Attributes["integrator"] = policy.Integrator.Key;
                graph.Attributes["dt"] = policy.TimeStep;
                return new SolveHistory(
                    HdfArchive.Begin(graph, capability.Sink(SolveArchiveKind.History), capability.Policy),
                    u, v, a, policy.TimeSteps, dofs);
            }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<solve-archive-open:{error.Message}>")));

    // First-order marches carry no independent velocity/acceleration state: the same step writes with the
    // kinematic pair absent, and the container's dataset roster states which kinematics the route produced.
    // Ordering is the archive writer's own cursor law — a repeated or skipped step ordinal refuses there.
    public Fin<Unit> Step(int step, double[] field, double[]? velocity = null, double[]? acceleration = null) =>
        writer is null
            ? Fin.Succ(unit)
            : Try.lift(() => {
                ReadOnlySpan<int> grid = [steps, 1];
                ReadOnlySpan<uint> chunk = [1U, (uint)dofs];
                writer.WriteChunk(u!, field, step, grid, chunk);
                if (velocity is not null) { writer.WriteChunk(v!, velocity, step, grid, chunk); }
                if (acceleration is not null) { writer.WriteChunk(a!, acceleration, step, grid, chunk); }
                return unit;
            }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<solve-archive-step:{error.Message}>"));

    public void Dispose() => writer?.Dispose();
}

// Mode-shape archive: [pairs, dofs] mode-outermost so one hyperslab reads one mode, eigenvalues and per-axis
// participation as plain datasets, condensation evidence as attributes beside the identity — the single
// most-inspected artifact of a seismic or vibration run, readable in h5py/ParaView with no bridge. A mode is
// 8·dofs bytes, so a building-order read-back passes a slab-sized `SimpleReadingChunkCache` — the 1 MiB default
// re-decompresses every miss past ~131k DOFs.
public static class SolveModes {
    public static Fin<Unit> Seal(Option<SolveArchive> archive, SolveProblem problem, SolveResult result) =>
        archive.Match(
            None: () => Fin.Succ(unit),
            Some: capability => result.EigenValues.Match(
                None: () => Fin.Succ(unit),
                Some: values => Try.lift(() => {
                    int pairs = values.Length, dofs = checked((int)result.Dofs);
                    H5Dataset<double[]> modes = new([(ulong)pairs, (ulong)dofs], [1U, (uint)dofs], datasetCreation: capability.Policy.Creation());
                    H5File graph = new() { ["modes"] = modes, ["eigenvalues"] = values.ToArray() };
                    result.Participation.Iter(rows => graph["participation"] = rows.ToArray().SelectMany(static row => new[] { row.X, row.Y, row.Z }).ToArray());
                    graph.Attributes["content-key"] = $"{problem.ContentKey:x32}";
                    graph.Attributes["physics"] = problem.Physics.Key;
                    result.Condensation.Iter(evidence => {
                        graph.Attributes["retained"] = evidence.Retained;
                        graph.Attributes["condensed"] = evidence.Condensed;
                        graph.Attributes["residual"] = evidence.Residual;
                        graph.Attributes["conditioning"] = evidence.Conditioning;
                    });
                    using HdfWriter writer = HdfArchive.Begin(graph, capability.Sink(SolveArchiveKind.Modes), capability.Policy);
                    ReadOnlySpan<double> flat = result.Field.Span;
                    for (int mode = 0; mode < pairs; mode++) {
                        writer.WriteChunk(modes, flat.Slice(mode * dofs, dofs).ToArray(), mode, [pairs, 1], [1U, (uint)dofs]);
                    }
                    return unit;
                }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<solve-archive-modes:{error.Message}>"))));
}

// Per-commit checkpoint: one whole-graph create-only container per COMMITTED arc step, keyed (ContentKey, step)
// through the caller's sink factory — never a growing file. The optional consolidation columns write as datasets
// only where the ledger carries them (uniform per law), so an absent column is an absent dataset, never a zero
// plane; ragged contact multipliers land as one concatenated values dataset beside its offsets dataset.
public static class SolveCheckpoint {
    public static Fin<Unit> Commit(Option<SolveArchive> archive, SolveProblem problem, int step, double[] field, double load, MaterialState[] committed, Seq<double[]> multipliers) =>
        archive.Match(
            None: () => Fin.Succ(unit),
            Some: capability => Try.lift(() => {
                H5File graph = new() { ["field"] = field };
                if (committed.Length > 0) {
                    int components = committed[0].PlasticStrain.Length;
                    double[] plastic = new double[committed.Length * components];
                    double[] hardening = new double[committed.Length], damage = new double[committed.Length];
                    double[] volumetric = new double[committed.Length], driving = new double[committed.Length];
                    for (int row = 0; row < committed.Length; row++) {
                        committed[row].PlasticStrain.Span.CopyTo(plastic.AsSpan(row * components, components));
                        hardening[row] = committed[row].Hardening;
                        damage[row] = committed[row].Damage;
                        volumetric[row] = committed[row].VolumetricPlasticStrain;
                        driving[row] = committed[row].DamageDriving;
                    }
                    graph["plastic-strain"] = new H5Dataset(plastic, fileDims: [(ulong)committed.Length, (ulong)components]);
                    graph["hardening"] = hardening;
                    graph["damage"] = damage;
                    graph["volumetric"] = volumetric;
                    graph["damage-driving"] = driving;
                    if (committed[0].PreconsolidationPressure.IsSome) {
                        graph["preconsolidation"] = committed.Map(static row => row.PreconsolidationPressure.IfNone(0.0)).ToArray();
                    }
                    if (committed[0].PorePressure.IsSome) {
                        graph["pore-pressure"] = committed.Map(static row => row.PorePressure.IfNone(0.0)).ToArray();
                    }
                }
                if (!multipliers.IsEmpty) {
                    graph["multipliers"] = multipliers.Bind(static row => row.AsIterable()).ToArray();
                    graph["multiplier-offsets"] = multipliers.Fold(Seq(0), static (offsets, row) => offsets.Add(offsets.Last + row.Length)).ToArray();
                }
                graph.Attributes["content-key"] = $"{problem.ContentKey:x32}";
                graph.Attributes["step"] = step;
                graph.Attributes["load"] = load;
                graph.Write(capability.Sink(SolveArchiveKind.Checkpoint));
                return unit;
            }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<solve-archive-checkpoint:{error.Message}>")));
}

public static class SolveLane {
    static SolveKind Routed(SolveProblem problem, SolveMethod method) =>
        method == SolveMethod.ArcLength ? SolveKind.Nonlinear
        : problem.Material.IsSome ? SolveKind.Nonlinear
        // Contact is nonlinear-only: its enforcement lives in the residual, never the linear system.
        : problem.Conditions.Exists(static bc => bc is BoundaryCondition.Contact) ? SolveKind.Nonlinear
        : problem.Physics.Eigen ? SolveKind.Eigen
        : problem.Physics.Transient ? SolveKind.Transient
        : problem.Physics.Nonlinear ? SolveKind.Nonlinear
        : method.Iterative ? SolveKind.Iterative
        : SolveKind.Direct;

    public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, IClock clock, Option<SolveArchive> archive = default) =>
        from policyValid in policy.Validate(problem)
        from operatorCsr in Assemble(problem, mesh, policy)
        from system in Constrained(operatorCsr, problem.Conditions, policy)
        from result in Routed(problem, policy.Method).Switch(
            state: (System: system, Mesh: mesh, Problem: problem, Policy: policy, At: clock.GetCurrentInstant(), Clock: clock, Archive: archive),
            direct: static state => Direct(state.System, state.Problem, state.Policy, state.At),
            iterative: static state => Iterative(state.System, state.Problem, state.Policy, state.Clock),
            nonlinear: static state => Newton(state.System, state.Mesh, state.Problem, state.Policy, state.Clock, state.Archive),
            transient: static state => March(state.System, state.Mesh, state.Problem, state.Policy, state.At, state.Archive),
            eigen: static state => Modal(state.System, state.Mesh, state.Problem, state.Policy, state.Clock, state.Archive))
        select result;

    // Each recovery rung's attempt calls the archive sink factory afresh, so every rung that runs an archiving
    // route emits its OWN create-only containers — a rung never appends into a prior rung's artifact.
    public static (Fin<SolveResult> Result, RecoveryReceipt Trace) SolveAdaptive(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, RecoveryPolicy recovery, IClock clock, Option<SolveArchive> archive = default) {
        (Fin<SolveResult> Result, SolveProblem Problem, DiscreteMesh Mesh, SolvePolicy Policy, Seq<(string Action, double Residual)> Steps) final = recovery.Ladder.Fold(
            (Result: Solve(problem, mesh, policy, clock, archive), Problem: problem, Mesh: mesh, Policy: policy, Steps: Seq<(string Action, double Residual)>()),
            (state, action) => {
                if (state.Result.IsSucc) { return state; }
                return Recover(action, state.Problem, state.Mesh, state.Policy, recovery, clock).Match(
                    Succ: next => {
                        Fin<SolveResult> attempt = Solve(next.Problem, next.Mesh, next.Policy, clock, archive);
                        return (attempt, next.Problem, next.Mesh, next.Policy, state.Steps.Add((action.Key, Residual(attempt))));
                    },
                    Fail: fault => (Fin.Fail<SolveResult>(fault), state.Problem, state.Mesh, state.Policy, state.Steps.Add((action.Key, double.PositiveInfinity))));
            });
        return (final.Result, new RecoveryReceipt(problem.Physics.Key, final.Steps, final.Result.IsSucc, clock.GetCurrentInstant()));
    }

    static Fin<(SolveProblem Problem, DiscreteMesh Mesh, SolvePolicy Policy)> Recover(RecoveryAction action, SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, RecoveryPolicy recovery, IClock clock) =>
        action.Switch(
            state: (Problem: problem, Mesh: mesh, Policy: policy, Recovery: recovery, Clock: clock),
            refineMesh: static s => MeshKernel.Refine(s.Mesh, s.Recovery.MeshPolicy, RefinementError(s.Mesh), s.Clock)
                .Map(refined => (s.Problem with { Element = refined.Element }, refined, s.Policy)),
            relax: static s => Fin.Succ((s.Problem, s.Mesh, s.Policy with { Tolerance = s.Policy.Tolerance * s.Recovery.RelaxFactor, MaxIterations = (int)(s.Policy.MaxIterations * s.Recovery.IterationGrowth) })),
            reorderDofs: static s => Reordered(s.Problem, s.Mesh, s.Policy, s.Clock),
            switchMethod: static s => Fin.Succ((s.Problem, s.Mesh, s.Policy with { Method = s.Recovery.Fallback })),
            restart: static s => Fin.Succ((s.Problem, s.Mesh, s.Policy with { Method = s.Recovery.Fallback, MaxIterations = s.Policy.MaxIterations * 2 })));

    static Fin<(SolveProblem Problem, DiscreteMesh Mesh, SolvePolicy Policy)> Reordered(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, IClock clock) {
        int dof = problem.Dof, nodes = checked((int)mesh.NodeCount);
        return Triplets(mesh, problem, policy).Map(t => {
                CoordinateStorage<double> coords = new(nodes, nodes, t.Vals.Length);
                for (int entry = 0; entry < t.Vals.Length; entry++) { coords.At(t.Rows[entry] / dof, t.Cols[entry] / dof, t.Vals[entry]); }
                CompressedColumnStorage<double> csc = CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
                int[] permutation = AMD.Generate(csc, ColumnOrdering.MinimumDegreeAtPlusA);
                return (problem, Renumbered(mesh, permutation, clock), policy);
            });
    }

    static DiscreteMesh Renumbered(DiscreteMesh mesh, int[] permutation, IClock clock) {
        int nodes = checked((int)mesh.NodeCount);
        if (permutation.Length < nodes) { return mesh; }
        int[] inverse = new int[nodes];
        for (int slot = 0; slot < nodes; slot++) { inverse[permutation[slot]] = slot; }
        float[] reordered = new float[nodes * 3];
        ReadOnlySpan<float> source = mesh.Coordinates;
        Span<float> sink = reordered;
        for (int old = 0; old < nodes; old++) {
            int fresh = inverse[old];
            sink[fresh * 3] = source[old * 3]; sink[fresh * 3 + 1] = source[old * 3 + 1]; sink[fresh * 3 + 2] = source[old * 3 + 2];
        }
        long[] renumberedConn = new long[checked((int)mesh.ElementCount) * mesh.Element.Nodes];
        ReadOnlySpan<long> conn = mesh.Indices;
        Span<long> freshConn = renumberedConn;
        for (int entry = 0; entry < conn.Length; entry++) { freshConn[entry] = inverse[(int)conn[entry]]; }
        return mesh with { Nodes = reordered.AsMemory(), Connectivity = renumberedConn.AsMemory(), At = clock.GetCurrentInstant() };
    }

    static double[] RefinementError(DiscreteMesh mesh) {
        double[] error = new double[checked((int)mesh.ElementCount)];
        for (long cell = 0; cell < error.Length; cell++) { error[cell] = 1.0 - Math.Abs(mesh.Element.Metric(MeshMetric.ScaledJacobian, mesh.NodalXyz(cell))); }
        return error;
    }

    static double Residual(Fin<SolveResult> result) => result.Match(Succ: static r => r.Residual, Fail: static _ => double.MaxValue);

    public static ComputeReceipt.Solve Receipt(SolveResult result, CorrelationId correlation, Duration elapsed) {
        Option<ModalParticipation> share = EffectiveMassShare(result);
        return new(result.Problem.Physics.Key, result.Method.Key, result.Dofs, result.Iterations, result.Residual, result.Converged) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
            ParticipationX = share.Case is ModalParticipation x ? x.X : null,
            ParticipationY = share.Case is ModalParticipation y ? y.Y : null,
            ParticipationZ = share.Case is ModalParticipation z ? z.Z : null,
        };
    }

    // Receipt columns carry the effective-mass FRACTION per axis — `Σ_modes Γ_d² / (rᵀ·M·r)_d` — because that ratio
    // is what a seismic floor gates on, while the raw factors scale with the model's mass. Both modal producers fill the
    // factor set and the excitable mass together, so the three columns stay null exactly on the routes that
    // report no modes. An axis whose translational rows are all prescribed has no excitable mass, so its share is
    // exactly zero rather than a division.
    static Option<ModalParticipation> EffectiveMassShare(SolveResult result) {
        if (result.Participation.Case is not ReadOnlyMemory<ModalParticipation> factors || result.TotalMass.Case is not ModalParticipation excitable) { return None; }
        double x = 0.0, y = 0.0, z = 0.0;
        foreach (ModalParticipation factor in factors.Span) { x += factor.X * factor.X; y += factor.Y * factor.Y; z += factor.Z * factor.Z; }
        return Some(new ModalParticipation(Share(x, excitable.X), Share(y, excitable.Y), Share(z, excitable.Z)));
    }

    static double Share(double effective, double excitable) => excitable > 0.0 ? effective / excitable : 0.0;

    static Fin<SparseCompressedRowMatrixStorage<double>> Assemble(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy) {
        int dim = problem.Dof * checked((int)mesh.NodeCount);
        return problem.Payload switch {
            PhysicsPayload.Radiosity radiosity => Network(dim, (row, column) => row == column ? 1.0 - radiosity.Reflectance.Span[row] * radiosity.ViewFactors.Span[row * dim + column] : -radiosity.Reflectance.Span[row] * radiosity.ViewFactors.Span[row * dim + column]),
            PhysicsPayload.EnergyNetwork energy => Network(dim, (row, column) => energy.Conductance.Span[row * dim + column]),
            _ => Triplets(mesh, problem, policy).Bind(t => SparseOps.Ingest(SparseFormat.Coo, dim, dim, t.Rows, t.Cols, t.Vals)),
        };
    }

    static Fin<SparseCompressedRowMatrixStorage<double>> Network(int size, Func<int, int, double> coefficient) {
        int entries = checked(size * size);
        int[] rows = new int[entries], columns = new int[entries];
        double[] values = new double[entries];
        for (int row = 0, slot = 0; row < size; row++)
            for (int column = 0; column < size; column++, slot++) {
                rows[slot] = row; columns[slot] = column; values[slot] = coefficient(row, column);
            }
        return SparseOps.Ingest(SparseFormat.Coo, size, size, rows, columns, values);
    }

    static Fin<(int[] Rows, int[] Cols, double[] Vals)> Triplets(DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy) {
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof;
        int cells = checked((int)mesh.ElementCount), entries = cells * block * block;
        Fin<Unit>[] outcomes = new Fin<Unit>[cells];
        CellAssembly assembly = mesh.Element.Family == ShapeFamily.Frame
            ? new CellAssembly(mesh, problem.Physics, per, dof, MemberOf(mesh, problem), new int[entries], new int[entries], new double[entries], outcomes)
            : new CellAssembly(mesh, problem.Physics, per, dof, ContinuumOf(mesh, problem), new int[entries], new int[entries], new double[entries], outcomes);
        ParallelHelper.For(0, cells, in assembly);
        return toSeq(outcomes).Traverse(static outcome => outcome)
            .Map(_ => (assembly.Rows, assembly.Cols, assembly.Vals))
            .As();
    }

    static LocalBlock ContinuumOf(DiscreteMesh mesh, SolveProblem problem) {
        Func<int, double[]> materialOf = problem.Field.Lower(problem.Physics, problem.Payload);
        return (int cell, Span<double> local) => {
            LocalStiffness(mesh.Element, mesh.Element.Quadrature.Points, mesh.NodalXyz(cell), materialOf(cell), problem.Physics, problem.Payload, cell, mesh.Element.Nodes, problem.Dof, local);
            return Fin.Succ(unit);
        };
    }

    static LocalBlock MemberOf(DiscreteMesh mesh, SolveProblem problem) =>
        (int cell, Span<double> local) => {
            return problem.Field.MechanicalAt(cell).Bind(properties => mesh.Element.Member(mesh.NodalXyz(cell), problem.Members[cell], properties.Young, properties.Poisson, local));
        };

    delegate Fin<Unit> LocalBlock(int cell, Span<double> local);

    readonly struct CellAssembly(DiscreteMesh mesh, PhysicsKind physics, int per, int dof, LocalBlock localBlock, int[] rows, int[] cols, double[] vals, Fin<Unit>[] outcomes) : IAction {
        public int[] Rows => rows;
        public int[] Cols => cols;
        public double[] Vals => vals;

        public void Invoke(int cell) {
            ReadOnlySpan<long> conn = mesh.Indices;
            int block = per * dof;
            using SpanOwner<double> scratch = SpanOwner<double>.Allocate(block * block, AllocationMode.Clear);
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

    static void LocalStiffness(ElementClass element, ImmutableArray<(double X, double Y, double Z, double Weight)> quadrature, ReadOnlySpan<double> xyz, double[] material, PhysicsKind physics, PhysicsPayload payload, int cell, int per, int dof, Span<double> local) {
        int strain = physics.StrainDim, cols = per * dof;
        foreach ((double X, double Y, double Z, double Weight) gauss in quadrature) {
            ShapeSample sample = element.Sample((gauss.X, gauss.Y, gauss.Z), xyz);
            double weight = gauss.Weight * Math.Abs(sample.DetJ);
            double[] b = Strain(physics.Form, sample.Grad, per, dof, strain, cols);
            Accumulate(b, material, strain, cols, weight, local);
            switch (payload) {
                case PhysicsPayload.Flow flow:
                    FlowAdvection(sample, flow, cell, per, dof, weight, local);
                    break;
                case PhysicsPayload.Helmholtz wave:
                    ScalarMass(sample, per, dof, -wave.WaveNumber * wave.WaveNumber, weight, local);
                    break;
                case PhysicsPayload.EddyCurrent eddy:
                    EddyMass(sample, per, dof, eddy.AngularFrequency * eddy.Conductivity, weight, local);
                    break;
            }
        }
    }

    static void FlowAdvection(ShapeSample sample, PhysicsPayload.Flow flow, int cell, int per, int dof, double weight, Span<double> local) {
        ReadOnlySpan<double> velocity = flow.Velocity.Span.Slice(cell * 3, 3);
        for (int a = 0; a < per; a++)
            for (int b = 0; b < per; b++) {
                double convection = flow.Density * sample.Shape[a] * (velocity[0] * sample.Grad[b * 3] + velocity[1] * sample.Grad[b * 3 + 1] + velocity[2] * sample.Grad[b * 3 + 2]);
                for (int component = 0; component < 3; component++) { local[(a * dof + component) * (per * dof) + b * dof + component] += weight * convection; }
            }
    }

    static void ScalarMass(ShapeSample sample, int per, int dof, double coefficient, double weight, Span<double> local) {
        int cols = per * dof;
        for (int a = 0; a < per; a++)
            for (int b = 0; b < per; b++)
                for (int component = 0; component < dof; component++) { local[(a * dof + component) * cols + b * dof + component] += weight * coefficient * sample.Shape[a] * sample.Shape[b]; }
    }

    static void EddyMass(ShapeSample sample, int per, int dof, double coefficient, double weight, Span<double> local) {
        int cols = per * dof;
        for (int a = 0; a < per; a++)
            for (int b = 0; b < per; b++)
                for (int component = 0; component < 3; component++) {
                    double value = weight * coefficient * sample.Shape[a] * sample.Shape[b];
                    local[(a * dof + component) * cols + b * dof + component + 3] -= value;
                    local[(a * dof + component + 3) * cols + b * dof + component] += value;
                }
    }

    static double[] Strain(MaterialForm form, double[] grad, int per, int dof, int strain, int cols) {
        double[] b = new double[strain * cols];
        for (int a = 0; a < per; a++) {
            double gx = grad[a * 3], gy = grad[a * 3 + 1], gz = grad[a * 3 + 2];
            form.Switch(
                state: (A: a, Cols: cols, Gx: gx, Gy: gy, Gz: gz, B: b),
                elasticity: static state => {
                    int x = state.A * 3, y = state.A * 3 + 1, z = state.A * 3 + 2;
                    state.B[0 * state.Cols + x] = state.Gx; state.B[1 * state.Cols + y] = state.Gy; state.B[2 * state.Cols + z] = state.Gz;
                    state.B[3 * state.Cols + x] = state.Gy; state.B[3 * state.Cols + y] = state.Gx;
                    state.B[4 * state.Cols + y] = state.Gz; state.B[4 * state.Cols + z] = state.Gy;
                    state.B[5 * state.Cols + x] = state.Gz; state.B[5 * state.Cols + z] = state.Gx;
                },
                isotropic: static state => {
                    state.B[0 * state.Cols + state.A] = state.Gx;
                    state.B[1 * state.Cols + state.A] = state.Gy;
                    state.B[2 * state.Cols + state.A] = state.Gz;
                },
                mixedFlow: static state => {
                    for (int component = 0; component < 3; component++) {
                        int column = state.A * 4 + component;
                        state.B[(3 * component + 0) * state.Cols + column] = state.Gx;
                        state.B[(3 * component + 1) * state.Cols + column] = state.Gy;
                        state.B[(3 * component + 2) * state.Cols + column] = state.Gz;
                    }
                    state.B[9 * state.Cols + state.A * 4 + 3] = 1.0;
                },
                maxwellEddy: static state => {
                    for (int field = 0; field < 2; field++) {
                        int offset = state.A * 6 + field * 3, row = field * 3;
                        state.B[(row + 0) * state.Cols + offset + 1] = -state.Gz; state.B[(row + 0) * state.Cols + offset + 2] = state.Gy;
                        state.B[(row + 1) * state.Cols + offset + 0] = state.Gz; state.B[(row + 1) * state.Cols + offset + 2] = -state.Gx;
                        state.B[(row + 2) * state.Cols + offset + 0] = -state.Gy; state.B[(row + 2) * state.Cols + offset + 1] = state.Gx;
                    }
                });
        }
        return b;
    }

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

    // Peak-relative inertia floor is the ONE scale-derived threshold the mass-singularity refusals and the
    // condensation partition all read, so masslessness is a fraction of the model's own peak inertia and never an
    // absolute kilogramme literal. The fraction exists only to absorb assembly round-off: the frame idealization
    // writes EXACT zeros on its inertia-free rows, so nothing near the floor is a judgement call.
    const double InertiaFraction = 1e-12;
    static double InertiaFloor(double[] mass, double fraction) => TensorPrimitives.Max<double>(mass) * fraction;

    // First free row whose inertia sits at or under the floor — the row an inertia-dividing march cannot advance and
    // a mass-normalizing pencil cannot scale. Prescribed rows are excluded because they carry no free amplitude.
    static Option<long> MassSingular(double[] mass, LanguageExt.HashSet<long> constrained, double floor) =>
        toSeq(Enumerable.Range(0, mass.Length)).Find(dof => !constrained.Contains(dof) && mass[dof] <= floor).Map(static dof => (long)dof);

    // Lumped inertia is REAL mass for the elastic forms: `ρ·A·L` over a frame cell's two joints and `ρ·∫|detJ|` over
    // a continuum cell's nodes, the density read off the ONE `MechanicalAt` per-cell constitutive accessor. A
    // lumped-mass frame carries NO rotary inertia — the textbook idealization — so the rotational slots stay exactly
    // zero rather than taking a translational share whose kilogrammes are not kilogramme-metres-squared; those exact
    // zeros are what make static condensation of those rows spectrally exact, so a floor that lifts a degenerate
    // cell's extent off zero fabricates the one quantity the reduction reads. For the scalar forms the coefficient
    // is the volumetric heat capacity the ONE `CapacityAt` read supplies, so the vector a first-order march divides
    // by is a real storage in J/K rather than a bare geometric volume.
    // Exemption: the per-cell scatter is the measured-kernel statement seam.
    static Fin<double[]> Lumped(DiscreteMesh mesh, SolveProblem problem) {
        int nodes = checked((int)mesh.NodeCount), per = mesh.Element.Nodes, dof = problem.Dof;
        bool frame = mesh.Element.Family == ShapeFamily.Frame, elastic = problem.Physics.Form == MaterialForm.Elasticity;
        int inertial = frame ? 3 : dof;
        double[] mass = new double[nodes * dof];
        ReadOnlySpan<long> conn = mesh.Indices;
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            double extent = 0.0;
            if (frame) {
                double dx = xyz[3] - xyz[0], dy = xyz[4] - xyz[1], dz = xyz[5] - xyz[2];
                extent = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
            else {
                foreach ((double X, double Y, double Z, double Weight) gauss in mesh.Element.Quadrature.Points) {
                    extent += gauss.Weight * Math.Abs(mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz).DetJ);
                }
            }
            Fin<double> weight = elastic
                ? problem.Field.MechanicalAt(cell).Map(row => row.Density * (frame ? problem.Members[cell].Area : 1.0))
                : problem.Field.CapacityAt(cell);
            if (weight.Case is not double scale) { return weight.Map(static _ => default(double[])!); }
            double share = extent * scale / per;
            for (int a = 0; a < per; a++) {
                int node = (int)conn[cell * per + a];
                for (int ci = 0; ci < inertial; ci++) { mass[node * dof + ci] += share; }
            }
        }
        return Fin.Succ(mass);
    }

    static Fin<ConstrainedSystem> Constrained(SparseCompressedRowMatrixStorage<double> operatorCsr, Seq<BoundaryCondition> conditions, SolvePolicy policy) =>
        conditions.Traverse(condition => condition.Validate(operatorCsr.RowCount))
            .Bind(_ => conditions.Fold(
                Fin.Succ(new ConstrainedSystem(operatorCsr, new double[operatorCsr.RowCount], Prelude.HashSet<long>(), policy.PenaltyFactor)),
                (acc, condition) => acc.Bind(system => condition.Apply(system, policy.Constraint))))
            .As();

    // Terminal residual is MEASURED at the returned state, never stamped: one sparse product against the same
    // right-hand side the route solved, relative to `‖b‖` so the scalar compares across models, and `Converged` is the
    // tolerance test on that measurement. A route stamping `(0.0, true)` publishes a verdict nothing computed,
    // and a direct factorization on an ill-conditioned operator returns exactly the field that verdict would bless.
    static (double Residual, bool Converged) Terminal(SparseCompressedRowMatrixStorage<double> csr, double[] rhs, double[] field, double tolerance) {
        double[] applied = new SparseMatrix(csr).Multiply(Vector<double>.Build.DenseOfArray(field)).AsArray();
        double residual = TensorPrimitives.Norm<double>(Residual(rhs, applied)) / Math.Max(1.0, TensorPrimitives.Norm<double>(rhs));
        return (residual, residual <= tolerance);
    }

    static Fin<SolveResult> Direct(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, Instant at) =>
        SparseOps.Factor(system.Operator, policy.Method.Kind == FactorizationKind.Cholesky ? FactorKind.Spd : FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => factored.Solve(system.Rhs, policy.Tolerance * 1e3))
            .Map(field => Terminal(system.Operator, system.Rhs, field, policy.Tolerance) is var terminal
                ? new SolveResult(problem, policy.Method, field.AsMemory(), None, None, None, system.Rhs.Length, 1, 1, terminal.Residual, terminal.Converged, at)
                : throw new UnreachableException());

    // `IterationPolicy.Of` is the numeric record's one ambient-free ingress and takes clock, wall budget, and token
    // together: the lane's own clock crosses here and the budget half rides the policy row, so the criterion stack
    // bounds wall time and cancellation off one clock and no canonical row carries a second.
    static IterationPolicy Iteration(SolvePolicy policy, IClock clock) =>
        IterationPolicy.Of(clock, policy.Deadline, policy.Cancel)
            with { Tolerance = policy.Tolerance, MaxIterations = policy.MaxIterations, Preconditioner = policy.Method.Preconditioner.Build };

    static Fin<SolveResult> Iterative(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, IClock clock) =>
        policy.Method.Krylov.ToFin(ComputeFault.Create($"<solve-method-not-iterative:{policy.Method.Key}>"))
            .Bind(krylov => SparseOps.SolveIterative(system.Operator, krylov, system.Rhs, Iteration(policy, clock)))
            .Bind(run => run.Terminal is SolveTerminal.Admitted
                ? Fin.Succ(new SolveResult(problem, policy.Method, run.Field.ToArray().AsMemory(), None, None, None, system.Rhs.Length, 0, 1, run.Residual, true, clock.GetCurrentInstant()))
                : Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<solve-diverged:{policy.Method.Key}:residual={run.Residual:e3}>")));

    // Implicit schemes add stiffness and damping to the effective operator, so an inertia-free row still carries a
    // solvable diagonal; the explicit scheme DIVIDES by inertia, so the same row is unmarchable and its guard would
    // freeze it at zero and publish that as motion. A frame's rotational rows are exactly those rows, so the explicit
    // integrator refuses the model by naming the row instead of returning frozen rotations.
    static Fin<SolveResult> March(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at, Option<SolveArchive> archive) =>
        Lumped(mesh, problem).Bind(lumped =>
            problem.Physics.Form != MaterialForm.Elasticity
                ? FirstOrder(system, problem, policy, Capacity(problem, lumped), at, archive)
                : policy.Integrator.Implicit
                    ? Newmark(system, problem, policy, lumped, at, archive)
                    : MassSingular(lumped, system.Constrained, InertiaFloor(lumped, InertiaFraction)).Match(
                        Some: dof => Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<explicit-mass-singular:dof={dof}:integrator={policy.Integrator.Key}>")),
                        None: () => CentralDifference(system, problem, policy, lumped, at, archive)));

    // Storage per row by payload: an energy network carries its measured nodal capacity vector, a mixed-flow row
    // zeroes the pressure slot because the pressure equation has no storage term, and the continuum-scalar row
    // reads the capacity-scaled lumped vector `Lumped` already built from `CapacityAt` — no arm re-derives a
    // capacity the material owner already answered.
    static double[] Capacity(SolveProblem problem, double[] lumped) => problem.Payload switch {
        PhysicsPayload.EnergyNetwork energy => energy.Capacity.ToArray(),
        PhysicsPayload.Flow => [.. lumped.Select((value, index) => index % 4 == 3 ? 0.0 : value)],
        _ => lumped,
    };

    static Fin<SolveResult> FirstOrder(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, double[] capacity, Instant at, Option<SolveArchive> archive) {
        int n = system.Rhs.Length;
        double[] effective = (double[])system.Operator.Values.Clone();
        int[] rows = system.Operator.RowPointers, columns = system.Operator.ColumnIndices;
        foreach (long constrained in system.Constrained) { capacity[(int)constrained] = 0.0; }
        for (int row = 0; row < n; row++)
            for (int slot = rows[row]; slot < rows[row + 1]; slot++)
                if (columns[slot] == row) { effective[slot] += capacity[row] / policy.TimeStep; }
        SparseCompressedRowMatrixStorage<double> effectiveCsr = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(n, n, effective.Length, rows, columns, effective);
        return SparseOps.Factor(effectiveCsr, FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => SolveHistory.Open(archive, problem, policy, n, kinematic: false).Bind(history => { using (history) { return toSeq(Enumerable.Range(0, policy.TimeSteps))
                .Fold(Fin.Succ((Field: new double[n], Forcing: (double[])system.Rhs.Clone())), (acc, step) => acc.Bind(state => {
                    double[] forcing = (double[])system.Rhs.Clone();
                    for (int i = 0; i < n; i++) { forcing[i] += capacity[i] * state.Field[i] / policy.TimeStep; }
                    return factored.Solve(forcing, policy.Tolerance * 1e3)
                        .Bind(field => history.Step(step, field).Map(_ => (Field: field, Forcing: forcing)));
                })); } }))
            // Evidence is the LAST step measured: one product of the effective operator against the returned field
            // relative to the forcing that step solved, so a march that drifted reports it.
            .Map(state => Terminal(effectiveCsr, state.Forcing, state.Field, policy.Tolerance) is var terminal
                ? new SolveResult(problem, policy.Method, state.Field.AsMemory(), None, None, None, n, policy.TimeSteps, 1, terminal.Residual, terminal.Converged, at)
                : throw new UnreachableException());
    }

    static Fin<SolveResult> Newmark(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, double[] lumped, Instant at, Option<SolveArchive> archive) {
        int n = system.Rhs.Length;
        double dt = policy.TimeStep, beta = policy.Integrator.Beta, gamma = policy.Integrator.Gamma;
        double dampingAlpha = policy.DampingAlpha, dampingBeta = policy.DampingBeta;
        ReadOnlySpan<double> stiffness = system.Operator.Values;
        int[] rowPtr = system.Operator.RowPointers, colIdx = system.Operator.ColumnIndices;
        double[] massEntry = new double[stiffness.Length], damping = new double[stiffness.Length];
        for (int row = 0; row < n; row++)
            for (int slot = rowPtr[row]; slot < rowPtr[row + 1]; slot++) {
                massEntry[slot] = colIdx[slot] == row ? lumped[row] : 0.0;
                // Implicit marching folds the WHOLE pair into the effective operator, so both ends of the
                // spectrum decay at the rates the policy declares.
                damping[slot] = dampingAlpha * massEntry[slot] + dampingBeta * stiffness[slot];
            }
        double[] effective = policy.Integrator.Effective(massEntry, damping, stiffness, dt);
        SparseCompressedRowMatrixStorage<double> effectiveCsr = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            system.Operator.RowCount, system.Operator.RowCount, effective.Length, rowPtr, colIdx, effective);
        SparseMatrix tangent = new(system.Operator);
        double a0 = 1.0 / (beta * dt * dt), a1 = gamma / (beta * dt), a2 = 1.0 / (beta * dt), a3 = 1.0 / (2.0 * beta) - 1.0,
               a4 = gamma / beta - 1.0, a5 = dt * 0.5 * (gamma / beta - 2.0), a6 = dt * (1.0 - gamma), a7 = gamma * dt;
        double alphaMass = policy.Integrator.AlphaMass, alphaForce = policy.Integrator.AlphaForce;
        return SparseOps.Factor(effectiveCsr, FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => SolveHistory.Open(archive, problem, policy, n).Bind(history => {
                using (history) {
                    // Every ACCEPTED step lands its kinematic triple before the next solve — the march is strictly
                    // step-ordered, so the chunk-once law holds by construction and the whole response history
                    // survives the run instead of only its terminal state.
                    return toSeq(Enumerable.Range(0, policy.TimeSteps))
                        .Fold(Fin.Succ((U: new double[n], V: new double[n], A: new double[n])),
                            (acc, step) => acc.Bind(s => factored.Solve(NewmarkForce(system.Rhs, lumped, tangent, s.U, s.V, s.A, dampingAlpha, dampingBeta, alphaMass, alphaForce, a0, a1, a2, a3, a4, a5), policy.Tolerance * 1e3)
                                .Map(next => Correct(next, s.U, s.V, s.A, a0, a2, a3, a6, a7))
                                .Bind(corrected => history.Step(step, corrected.U, corrected.V, corrected.A).Map(_ => corrected))))
                        .Map(state => Equilibrium(system.Rhs, tangent, lumped, dampingAlpha, dampingBeta, state.U, state.V, state.A, policy.Tolerance) is var terminal
                            ? new SolveResult(problem, policy.Method, state.U.AsMemory(), None, None, None, n, policy.TimeSteps, 1, terminal.Residual, terminal.Converged, at)
                            : throw new UnreachableException());
                }
            }));
    }

    static double[] NewmarkForce(double[] forcing, double[] mass, SparseMatrix tangent, double[] u, double[] v, double[] a, double dampingAlpha, double dampingBeta, double alphaMass, double alphaForce, double a0, double a1, double a2, double a3, double a4, double a5) {
        int n = forcing.Length;
        double[] massCombo = new double[n], dampCombo = new double[n], priorStiffness = tangent.Multiply(Vector<double>.Build.DenseOfArray(u)).AsArray();
        for (int i = 0; i < n; i++) {
            massCombo[i] = mass[i] * ((1.0 - alphaMass) * (a0 * u[i] + a2 * v[i] + a3 * a[i]) - alphaMass * a[i]);
            dampCombo[i] = (1.0 - alphaForce) * (a1 * u[i] + a4 * v[i] + a5 * a[i]) - alphaForce * v[i];
        }
        // `C·(…)` is the SAME pair the effective operator folded: the mass leg scales the lumped diagonal in place, the
        // stiffness leg rides one product against the elastic operator.
        double[] stiffnessLeg = tangent.Multiply(Vector<double>.Build.DenseOfArray(dampCombo)).AsArray();
        double[] force = new double[n];
        for (int i = 0; i < n; i++) {
            force[i] = forcing[i] + massCombo[i] + dampingAlpha * mass[i] * dampCombo[i] + dampingBeta * stiffnessLeg[i] - alphaForce * priorStiffness[i];
        }
        return force;
    }

    // Terminal march evidence is the equilibrium residual at the state the march ends on —
    // `‖f_ext − M·a − C·v − K·u‖ / ‖f_ext‖` under the same `C = αM + βK` pair the march integrated. A march
    // stamping `(0.0, true)` reports a step set that went unstable in its last decade exactly as it reports a
    // settled one.
    static (double Residual, bool Converged) Equilibrium(double[] forcing, SparseMatrix stiffness, double[] mass, double dampingAlpha, double dampingBeta, double[] u, double[] v, double[] a, double tolerance) {
        double[] elastic = stiffness.Multiply(Vector<double>.Build.DenseOfArray(u)).AsArray();
        double[] viscous = stiffness.Multiply(Vector<double>.Build.DenseOfArray(v)).AsArray();
        double[] residual = new double[forcing.Length];
        for (int i = 0; i < residual.Length; i++) {
            residual[i] = forcing[i] - mass[i] * a[i] - dampingAlpha * mass[i] * v[i] - dampingBeta * viscous[i] - elastic[i];
        }
        double norm = TensorPrimitives.Norm<double>(residual) / Math.Max(1.0, TensorPrimitives.Norm<double>(forcing));
        return (norm, norm <= tolerance);
    }

    static (double[] U, double[] V, double[] A) Correct(double[] next, double[] u, double[] v, double[] a, double a0, double a2, double a3, double a6, double a7) {
        int n = next.Length;
        double[] accel = new double[n], vel = new double[n];
        for (int i = 0; i < n; i++) {
            accel[i] = a0 * (next[i] - u[i]) - a2 * v[i] - a3 * a[i];
            vel[i] = v[i] + a6 * a[i] + a7 * accel[i];
        }
        return (next, vel, accel);
    }

    static Fin<SolveResult> CentralDifference(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, double[] lumped, Instant at, Option<SolveArchive> archive) {
        int n = system.Rhs.Length;
        double dt = policy.TimeStep, dt2 = dt * dt, dampingAlpha = policy.DampingAlpha, dampingBeta = policy.DampingBeta;
        SparseMatrix tangent = new(system.Operator);
        // This pair's mass leg keeps the explicit operator DIAGONAL, so it folds into the effective inertia; the
        // stiffness leg evaluates at the lagged half-step velocity and moves to the forcing, because folding
        // `βK` into the operator demands the factorization this march exists to avoid. Every row is inertia-bearing
        // here — the mass-singular refusal upstream already named any row that is not — so the division is total.
        double[] effMass = new double[n];
        for (int i = 0; i < n; i++) { effMass[i] = lumped[i] / dt2 + dampingAlpha * lumped[i] / (2.0 * dt); }
        // This explicit march archives DISPLACEMENT alone per step — the three-level stencil resolves velocity and
        // acceleration one step behind, so per-step kinematic writes would publish a pair the scheme has not yet
        // centred; the resolved terminal pair rides the result as before.
        return SolveHistory.Open(archive, problem, policy, n, kinematic: false).Bind(history => {
            using (history) {
                return toSeq(Enumerable.Range(0, policy.TimeSteps))
                    .Fold(Fin.Succ((Curr: new double[n], Prev: new double[n], Prior: new double[n])), (acc, step) => acc.Bind(s => {
                        double[] internalForce = tangent.Multiply(Vector<double>.Build.DenseOfArray(s.Curr)).AsArray();
                        double[] lagged = new double[n];
                        for (int i = 0; i < n; i++) { lagged[i] = (s.Curr[i] - s.Prev[i]) / dt; }
                        double[] viscous = tangent.Multiply(Vector<double>.Build.DenseOfArray(lagged)).AsArray();
                        double[] next = new double[n];
                        for (int i = 0; i < n; i++) {
                            double rhs = system.Rhs[i] - internalForce[i] - dampingBeta * viscous[i]
                                + lumped[i] / dt2 * (2.0 * s.Curr[i] - s.Prev[i]) + dampingAlpha * lumped[i] / (2.0 * dt) * s.Prev[i];
                            next[i] = rhs / effMass[i];
                        }
                        return history.Step(step, next).Map(_ => (next, s.Curr, s.Prev));
                    }))
                    .Map(marched => {
                        double[] velocity = new double[n], acceleration = new double[n];
                        for (int i = 0; i < n; i++) {
                            velocity[i] = (marched.Curr[i] - marched.Prior[i]) / (2.0 * dt);
                            acceleration[i] = (marched.Curr[i] - 2.0 * marched.Prev[i] + marched.Prior[i]) / dt2;
                        }
                        // Equilibrium is measured at the step the central difference CENTRES on — the last state whose
                        // velocity and acceleration the three-level stencil actually resolves — so the reported residual
                        // is the scheme's own rather than an extrapolation past the end of the step set.
                        (double Residual, bool Converged) terminal = Equilibrium(system.Rhs, tangent, lumped, dampingAlpha, dampingBeta, marched.Prev, velocity, acceleration, policy.Tolerance);
                        return new SolveResult(problem, policy.Method, marched.Curr.AsMemory(), None, None, None, n, policy.TimeSteps, 1, terminal.Residual, terminal.Converged, at);
                    });
            }
        });
    }

    // NewtonLoad commits ONCE at convergence, so its terminal result IS its artifact and no checkpoint fires; the
    // arc-length path commits per outer step and checkpoints each commit.
    static Fin<SolveResult> Newton(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, IClock clock, Option<SolveArchive> archive) =>
        policy.Continuation.Match(
            Some: continuation => ArcLength(system, mesh, problem, policy, continuation, clock, archive),
            None: () => NewtonLoad(system, mesh, problem, policy, clock));

    // Committed material history: one MaterialState row per (cell, gauss point). Residual evaluations evolve
    // TRIAL rows from the committed ledger — line-search probes and rejected iterations never advance history —
    // and only a converged load/arc step commits its trial ledger, so plasticity, damage, and consolidation stay
    // path-dependent across steps instead of re-minting Pristine per evaluation.
    static MaterialState[] Pristine(DiscreteMesh mesh, SolveProblem problem, ConstitutiveModel model) {
        int components = model is ConstitutiveModel.Hyperelastic ? 9 : problem.Physics.StrainDim;
        return [.. Enumerable.Range(0, mesh.ElementCount * mesh.Element.Quadrature.Points.Length).Select(_ => MaterialState.Pristine(components))];
    }

    static Fin<SolveResult> NewtonLoad(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, IClock clock) {
        SparseMatrix tangent = new(system.Operator);
        MaterialState[] committed = problem.Material.Match(Some: law => Pristine(mesh, problem, law.Model), None: static () => []);
        double scale = Math.Max(1.0, TensorPrimitives.Norm<double>(system.Rhs));
        return toSeq(Enumerable.Range(0, policy.NewtonIterations))
            .Fold(Fin.Succ((Field: new double[system.Rhs.Length], Residual: double.MaxValue, Step: 0, Converged: false, Committed: committed, Multipliers: Seq<double[]>())),
                (acc, _) => acc.Bind(state => state.Converged
                    ? Fin.Succ(state)
                    : InternalForce(mesh, problem, tangent, state.Field, state.Committed, state.Multipliers, clock).Bind(evaluation => {
                        double[] residual = Residual(system.Rhs, evaluation.InternalForce);
                        double norm = TensorPrimitives.Norm<double>(residual);
                        return norm <= policy.Tolerance * scale
                            // Converged: the last trial ledger and contact multipliers COMMIT — the accepted step
                            // advances history once.
                            ? Fin.Succ((state.Field, norm, state.Step, true, evaluation.Trial, evaluation.ContactMultipliers))
                            : policy.Method.Krylov.ToFin(ComputeFault.Create($"<solve-method-not-iterative:{policy.Method.Key}>"))
                                // Consistent tangent from the SAME trial state governs the Newton operator.
                                .Bind(krylov => SparseOps.SolveIterative(evaluation.Tangent, krylov, residual, Iteration(policy, clock)))
                                .Bind(run => ArmijoLineSearch(mesh, problem, tangent, system.Rhs, state.Field, run.Field, norm, state.Committed, state.Multipliers, clock).Map(alpha => {
                                    double[] updated = new double[state.Field.Length];
                                    TensorPrimitives.MultiplyAdd(run.Field, alpha, state.Field, updated);
                                    return (updated, norm, state.Step + 1, false, state.Committed, state.Multipliers);
                                }));
                    })))
            .Bind(state => state.Converged
                ? Fin.Succ(new SolveResult(problem, policy.Method, state.Field.AsMemory(), None, None, None, system.Rhs.Length, state.Step, state.Step, state.Residual, true, clock.GetCurrentInstant()))
                : Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<solve-newton-cap:{policy.NewtonIterations}:residual={state.Residual:e3}>")));
    }

    static Fin<SolveResult> ArcLength(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, ArcLengthPolicy path, IClock clock, Option<SolveArchive> archive) {
        IterationPolicy iteration = Iteration(policy, clock);
        SparseMatrix tangent = new(system.Operator);
        MaterialState[] pristine = problem.Material.Match(Some: law => Pristine(mesh, problem, law.Model), None: static () => []);
        // Modified-Newton arc-length: the corrector's linear solves keep the initial-stiffness operator while the
        // residual reads the committed ledger per step — a converged outer step COMMITS its trial ledger, so the
        // load path is genuinely history-dependent across steps.
        Func<double[], Fin<double[]>> solve = rhs => policy.Method.Krylov
            .ToFin(ComputeFault.Create("<arc-length-inner-method>"))
            .Bind(krylov => SparseOps.SolveIterative(system.Operator, krylov, rhs, iteration))
            .Bind(run => run.Terminal is SolveTerminal.Admitted
                ? Fin.Succ(run.Field.ToArray())
                : Fin.Fail<double[]>(new ComputeFault.ModelRejected($"<arc-length-linear-diverged:{run.Residual:e3}>")));
        return toSeq(Enumerable.Range(0, path.Steps))
            .Fold(
                Fin.Succ((Field: new double[system.Rhs.Length], Load: 0.0, Direction: new double[system.Rhs.Length], Iterations: 0, Residual: double.MaxValue, Committed: pristine, Multipliers: Seq<double[]>())),
                (outer, arcStep) => outer.Bind(state => solve(system.Rhs).Bind(loadDirection => {
                    double orientation = TensorPrimitives.Dot(loadDirection, state.Direction) < 0.0 ? -1.0 : 1.0;
                    double loadIncrement = orientation * path.Radius / Math.Sqrt(TensorPrimitives.SumOfSquares<double>(loadDirection) + path.LoadScale * path.LoadScale);
                    double[] origin = (double[])state.Field.Clone();
                    double originLoad = state.Load;
                    double[] predicted = new double[state.Field.Length];
                    TensorPrimitives.MultiplyAdd(loadDirection, loadIncrement, state.Field, predicted);
                    return toSeq(Enumerable.Range(0, policy.NewtonIterations))
                        .Fold(Fin.Succ((Field: predicted, Load: state.Load + loadIncrement, Converged: false, Residual: double.MaxValue, Iterations: 0, Trial: state.Committed, TrialMultipliers: state.Multipliers)),
                            (inner, _) => inner.Bind(point => point.Converged
                                ? Fin.Succ(point)
                                : InternalForce(mesh, problem, tangent, point.Field, state.Committed, state.Multipliers, clock).Bind(evaluation => {
                                    double[] forcing = new double[system.Rhs.Length];
                                    TensorPrimitives.Multiply(system.Rhs, point.Load, forcing);
                                    double[] residual = Residual(forcing, evaluation.InternalForce);
                                    double norm = TensorPrimitives.Norm<double>(residual);
                                    if (norm <= path.ResidualTolerance * Math.Max(1.0, TensorPrimitives.Norm<double>(forcing))) {
                                        return Fin.Succ((point.Field, point.Load, true, norm, point.Iterations, evaluation.Trial, evaluation.ContactMultipliers));
                                    }
                                    // Corrector iterates carry the step's COMMITTED ledger and multipliers
                                    // forward unchanged: history advances only where a step commits, so an
                                    // iterate that has not converged evolves its trial rows and discards them.
                                    return from correction in solve(residual)
                                           from response in solve(system.Rhs)
                                           from corrected in ArcCorrect(point.Field, point.Load, origin, originLoad, correction, response, path)
                                           select (corrected.Field, corrected.Load, false, norm, point.Iterations + 1, state.Committed, state.Multipliers);
                                })))
                        .Bind(point => point.Converged
                            ? SolveCheckpoint.Commit(archive, problem, arcStep, point.Field, point.Load, point.Trial, point.TrialMultipliers)
                                .Map(_ => (point.Field, point.Load, ArcDirection(point.Field, origin), state.Iterations + point.Iterations, point.Residual, point.Trial, point.TrialMultipliers))
                            : Fin.Fail<(double[], double, double[], int, double, MaterialState[], Seq<double[]>)>(new ComputeFault.ModelRejected($"<arc-length-corrector-cap:{policy.NewtonIterations}:residual={point.Residual:e3}>")));
                })))
            .Map(state => new SolveResult(problem, policy.Method, state.Field.AsMemory(), None, None, None, system.Rhs.Length, state.Iterations, state.Iterations, state.Residual, true, clock.GetCurrentInstant()));
    }

    static Fin<(double[] Field, double Load)> ArcCorrect(double[] field, double load, double[] origin, double originLoad, double[] correction, double[] response, ArcLengthPolicy path) {
        double[] displacement = new double[field.Length];
        TensorPrimitives.Subtract(field, origin, displacement);
        double loadDelta = load - originLoad;
        double constraint = TensorPrimitives.SumOfSquares<double>(displacement) + path.LoadScale * path.LoadScale * loadDelta * loadDelta - path.Radius * path.Radius;
        double denominator = TensorPrimitives.Dot(displacement, response) + path.LoadScale * path.LoadScale * loadDelta;
        if (Math.Abs(denominator) <= 1e-14) { return Fin.Fail<(double[], double)>(new ComputeFault.ModelRejected("<arc-length-limit-point-singular-corrector>")); }
        double loadCorrection = (-0.5 * constraint - TensorPrimitives.Dot(displacement, correction)) / denominator;
        double[] increment = new double[field.Length], next = new double[field.Length];
        TensorPrimitives.MultiplyAdd(response, loadCorrection, correction, increment);
        TensorPrimitives.Add(field, increment, next);
        return Fin.Succ((next, load + loadCorrection));
    }

    static double[] ArcDirection(double[] field, double[] origin) {
        double[] direction = new double[field.Length];
        TensorPrimitives.Subtract(field, origin, direction);
        return direction;
    }

    static double[] Residual(double[] forcing, double[] internalForce) {
        double[] residual = (double[])forcing.Clone();
        TensorPrimitives.Subtract(residual, internalForce, residual);
        return residual;
    }

    // One residual evaluation: internal force, TRIAL state ledger, TRIAL contact multipliers, and the consistent
    // tangent assembled from the same trial state. The elastic path passes the committed ledger through unchanged
    // and keeps the elastic operator; contact augments every path through the ONE constitutive enforcement owner.
    sealed record ResidualEvaluation(double[] InternalForce, MaterialState[] Trial, Seq<double[]> ContactMultipliers, SparseCompressedRowMatrixStorage<double> Tangent);

    static Fin<ResidualEvaluation> InternalForce(DiscreteMesh mesh, SolveProblem problem, SparseMatrix elastic, double[] field, MaterialState[] committed, Seq<double[]> multipliers, IClock clock) =>
        problem.Material.Match(
            Some: law => Constitutive(mesh, problem, law.Model, law.Law, field, committed, elastic, clock),
            None: () => Fin.Succ(new ResidualEvaluation(
                elastic.Multiply(Vector<double>.Build.DenseOfArray(field)).AsArray(),
                committed,
                Seq<double[]>(),
                (SparseCompressedRowMatrixStorage<double>)elastic.Storage)))
        .Bind(evaluation => ContactAugment(problem, field, evaluation, multipliers, clock));

    // Contact enforcement per residual evaluation: current kinematics and the step's committed multipliers hand
    // to ContactEnforcement.Enforce; the returned per-pair force and gap-space stiffness project onto the constraint
    // normal at the slave/master triples, and the advanced multipliers ride the evaluation as TRIAL — a converged
    // step commits them (the augmented-Lagrangian outer update), a probe discards them. The projection `∂g/∂u = ±w·n`
    // carries BOTH legs: the force scatters `f·n` and the Hessian scatters `h·n⊗n` over the four blocks of the pair,
    // so the Newton operator gains the contact stiffness the enforcement measured. A residual that augments the
    // force while the tangent stays elastic converges at first order and reports Newton iterations that never were.
    static Fin<ResidualEvaluation> ContactAugment(SolveProblem problem, double[] field, ResidualEvaluation evaluation, Seq<double[]> multipliers, IClock clock) {
        Seq<BoundaryCondition.Contact> contacts = problem.Conditions.Choose(static bc => bc is BoundaryCondition.Contact contact ? Some(contact) : None);
        if (contacts.IsEmpty) { return Fin.Succ(evaluation); }
        double[] force = (double[])evaluation.InternalForce.Clone();
        Dictionary<(int Row, int Column), double> block = [];
        Seq<double[]> advanced = Seq<double[]>();
        for (int c = 0; c < contacts.Count; c++) {
            BoundaryCondition.Contact contact = contacts[c];
            Seq<(int Slave, int Master)> pairs = toSeq(contact.Slave.Zip(contact.Master, static (s, m) => ((int)s, (int)m)));
            double[] lambda = c < multipliers.Count ? multipliers[c] : new double[pairs.Count];
            Fin<ContactResult> enforced = ContactEnforcement.Enforce(contact.Constraint, field.AsMemory(), lambda.AsMemory(), contact.Penalty, pairs, clock);
            if (enforced.Case is not ContactResult result) { return enforced.Map(static _ => default(ResidualEvaluation)!); }
            double[] normal = [contact.Constraint.Normal.X, contact.Constraint.Normal.Y, contact.Constraint.Normal.Z];
            for (int i = 0; i < pairs.Count; i++) {
                double weight = contact.Constraint.Weight(i);
                double f = result.Force.Span[i] * weight;
                // This gap potential sums independent per-pair terms, so its Hessian is diagonal and the pair's block
                // reads one entry; the weight enters squared because it rides both derivative legs.
                double h = result.Stiffness.Span[i * pairs.Count + i] * weight * weight;
                (int slave, int master) = pairs[i];
                for (int k = 0; k < 3; k++) {
                    force[slave + k] += f * normal[k];
                    force[master + k] -= f * normal[k];
                    for (int l = 0; l < 3; l++) {
                        double coupled = h * normal[k] * normal[l];
                        Accumulate(block, slave + k, slave + l, coupled);
                        Accumulate(block, master + k, master + l, coupled);
                        Accumulate(block, slave + k, master + l, -coupled);
                        Accumulate(block, master + k, slave + l, -coupled);
                    }
                }
            }
            advanced = advanced.Add(result.Multipliers.ToArray());
        }
        return Augmented(evaluation.Tangent, block)
            .Map(tangent => evaluation with { InternalForce = force, ContactMultipliers = advanced, Tangent = tangent });
    }

    static void Accumulate(Dictionary<(int Row, int Column), double> block, int row, int column, double value) =>
        block[(row, column)] = block.TryGetValue((row, column), out double held) ? held + value : value;

    // Contact pairs bind rows no element connects, so the block enters through a RE-INGEST rather than the
    // pattern-preserving scatter: the elastic sparsity holds no slot for a slave/master coupling, and an in-pattern
    // add drops exactly the off-diagonal terms that make the tangent consistent. The merge sums into keyed entries
    // before ingest and hands over a coordinate set with no repeated key, so the result never depends on whether the
    // admission seam's coordinate factory sums or overwrites a duplicate. The re-ingest also leaves the elastic
    // operator itself untouched, which is what lets the elastic path share one storage across every iteration.
    static Fin<SparseCompressedRowMatrixStorage<double>> Augmented(SparseCompressedRowMatrixStorage<double> tangent, Dictionary<(int Row, int Column), double> block) {
        for (int row = 0; row < tangent.RowCount; row++) {
            for (int slot = tangent.RowPointers[row]; slot < tangent.RowPointers[row + 1]; slot++) {
                Accumulate(block, row, tangent.ColumnIndices[slot], tangent.Values[slot]);
            }
        }
        int[] rows = new int[block.Count], columns = new int[block.Count];
        double[] values = new double[block.Count];
        int entry = 0;
        foreach (((int Row, int Column) key, double value) in block) { rows[entry] = key.Row; columns[entry] = key.Column; values[entry] = value; entry++; }
        return SparseOps.Ingest(SparseFormat.Coo, tangent.RowCount, tangent.ColumnCount, rows, columns, values);
    }

    static Fin<ResidualEvaluation> Constitutive(DiscreteMesh mesh, SolveProblem problem, ConstitutiveModel model, MaterialParameters law, double[] field, MaterialState[] committed, SparseMatrix elastic, IClock clock) {
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof;
        int gaussCount = mesh.Element.Quadrature.Points.Length;
        bool finiteStrain = model is ConstitutiveModel.Hyperelastic;
        int components = finiteStrain ? 9 : problem.Physics.StrainDim;
        double[] global = new double[field.Length];
        MaterialState[] trial = (MaterialState[])committed.Clone();
        // Consistent tangent reuses the elastic operator's CSR pattern (same connectivity, same sparsity);
        // values re-assemble as Σ w·Bᵀ·D·B from each trial state's exact per-point tangent.
        SparseCompressedRowMatrixStorage<double> tangent = (SparseCompressedRowMatrixStorage<double>)new SparseMatrix((SparseCompressedRowMatrixStorage<double>)elastic.Storage).Storage;
        Array.Clear(tangent.Values);
        ReadOnlySpan<long> conn = mesh.Indices;
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            int point = 0;
            foreach ((double X, double Y, double Z, double Weight) gauss in mesh.Element.Quadrature.Points) {
                ShapeSample sample = mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz);
                double weight = gauss.Weight * Math.Abs(sample.DetJ);
                double[] b = finiteStrain ? new double[components * block] : Strain(problem.Physics.Form, sample.Grad, per, dof, components, block);
                if (finiteStrain) {
                    for (int node = 0; node < per; node++)
                        for (int displacement = 0; displacement < 3; displacement++)
                            for (int derivative = 0; derivative < 3; derivative++) { b[(displacement * 3 + derivative) * block + node * dof + displacement] = sample.Grad[node * 3 + derivative]; }
                }
                double[] gaussStrain = new double[components];
                for (int r = 0; r < components; r++) {
                    double e = 0.0;
                    for (int j = 0; j < block; j++) { e += b[r * block + j] * field[(int)conn[cell * per + j / dof] * dof + j % dof]; }
                    gaussStrain[r] = e + (finiteStrain && r is 0 or 4 or 8 ? 1.0 : 0.0);
                }
                Fin<ConstitutiveResult> update = StressUpdate.Stress(model, gaussStrain.AsMemory(), trial[cell * gaussCount + point], law, clock);
                Fin<Unit> accumulated = update.Map(result => {
                    trial[cell * gaussCount + point] = result.State;
                    ReadOnlySpan<double> stress = result.Stress.Span;
                    ReadOnlySpan<double> d = result.Tangent.Span;
                    for (int i = 0; i < block; i++) {
                        double f = 0.0;
                        for (int r = 0; r < components; r++) { f += b[r * block + i] * (r < stress.Length ? stress[r] : 0.0); }
                        global[(int)conn[cell * per + i / dof] * dof + i % dof] += weight * f;
                        int rowDof = (int)conn[cell * per + i / dof] * dof + i % dof;
                        for (int j = 0; j < block; j++) {
                            double k = 0.0;
                            for (int r = 0; r < components; r++)
                                for (int s = 0; s < components; s++) { k += b[r * block + i] * d[r * components + s] * b[s * block + j]; }
                            AddAt(tangent, rowDof, (int)conn[cell * per + j / dof] * dof + j % dof, weight * k);
                        }
                    }
                    return unit;
                });
                if (accumulated.IsFail) { return accumulated.Map(static _ => default(ResidualEvaluation)!); }
                point++;
            }
        }
        // Multipliers are the CONTACT augmentation's product alone, so the constitutive leg carries the empty set
        // exactly as the elastic leg does — advancing them here would commit an update no contact evaluated.
        return Fin.Succ(new ResidualEvaluation(global, trial, Seq<double[]>(), tangent));
    }

    // Pattern-preserving scatter: the CSR row slice binary-searches the column; connectivity guarantees presence.
    static void AddAt(SparseCompressedRowMatrixStorage<double> csr, int row, int column, double value) {
        int index = Array.BinarySearch(csr.ColumnIndices, csr.RowPointers[row], csr.RowPointers[row + 1] - csr.RowPointers[row], column);
        if (index >= 0) { csr.Values[index] += value; }
    }

    static Fin<double> ArmijoLineSearch(DiscreteMesh mesh, SolveProblem problem, SparseMatrix tangent, double[] forcing, double[] field, double[] direction, double baseline, MaterialState[] committed, Seq<double[]> multipliers, IClock clock) =>
        toSeq(Enumerable.Range(0, 8)).Fold(
            Fin.Succ((Alpha: 1.0, Accepted: false)),
            (acc, _) => acc.Bind(state => state.Accepted
                ? Fin.Succ(state)
                : Fin.Succ(state).Bind(current => {
                    double[] trial = new double[field.Length];
                    TensorPrimitives.MultiplyAdd(direction, current.Alpha, field, trial);
                    // Probes evolve trial state from the SAME committed ledger and discard it — a probe never commits.
                    return InternalForce(mesh, problem, tangent, trial, committed, multipliers, clock)
                        .Map(evaluation => TensorPrimitives.Norm<double>(Residual(forcing, evaluation.InternalForce)))
                        .Map(norm => norm <= (1.0 - 1e-4 * current.Alpha) * baseline ? (current.Alpha, true) : (current.Alpha * 0.5, false));
                })))
        .Bind(state => state.Accepted
            ? Fin.Succ(state.Alpha)
            : Fin.Fail<double>(new ComputeFault.ModelRejected($"<solve-line-search-cap:residual={baseline:e3}>")));

    // Vibration is dispatched by the POLICY VALUE, not by a method-key comparison: the reduction travels as
    // `Condensation` and the discriminant validation already tied it to its method row, so the arm reads the value.
    // Mode archiving seals at ONE site off the finished result — both modal routes return the identical
    // column-major (n × k) field, so the seal reads the result rather than threading a writer through either.
    static Fin<SolveResult> Modal(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, IClock clock, Option<SolveArchive> archive) =>
        (problem.Physics == PhysicsKind.FeaBuckling
            ? Buckle(system, mesh, problem, policy, clock)
            : policy.Condensation.Match(
                Some: condensation => Condensed(system, mesh, problem, policy, condensation, clock),
                None: () => Vibration(system, mesh, problem, policy, clock.GetCurrentInstant())))
        .Bind(result => SolveModes.Seal(archive, problem, result).Map(_ => result));

    // Whole-operator modal densifies at full order and normalizes by `1/√m` per row, so it refuses twice by name:
    // above the dense ceiling the allocation is quadratic and the factorization cubic in the dof count, and an
    // inertia-free free row would scale its column by the reciprocal of nothing and return a spurious mode. Both
    // refusals name the condensed route, because a frame idealization ALWAYS reaches one of them and the reduced
    // pencil is where those models belong.
    static Fin<SolveResult> Vibration(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at) =>
        system.Operator.RowCount > policy.MaxDenseDofs
            ? Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<modal-dense-ceiling:dofs={system.Operator.RowCount}:ceiling={policy.MaxDenseDofs}:route={SolveMethod.CondensedEvd.Key}>"))
            : Lumped(mesh, problem).Bind(mass =>
                MassSingular(mass, system.Constrained, InertiaFloor(mass, InertiaFraction)).Match(
                    Some: dof => Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<modal-mass-singular:dof={dof}:route={SolveMethod.CondensedEvd.Key}>")),
                    None: () => DenseOps.Decompose(MassNormalized(Matrix<double>.Build.OfStorage(system.Operator), mass), FactorizationKind.Evd)
                        .Bind(factorization => EigenPairs(factorization, policy.EigenPairs, mass))
                        .Map(pairs => new SolveResult(
                            problem, policy.Method, pairs.Vectors, Some(pairs.Values),
                            Some(Participated(pairs.Vectors, mass, problem.Dof, pairs.Count)), Some(ExcitableMass(mass, problem.Dof)),
                            system.Rhs.Length, 1, 1, 0.0, true, at))));

    // Static (Guyan) condensation is the modal lane's sparse lowering. `Ψ = −K_ss⁻¹·K_sm` carries the condensed rows'
    // static response to a unit retained displacement, and because those rows carry NO inertia the reduction drops no
    // mass term at all — the retained pencil (`K_r = K_mm + K_ms·Ψ`, `M_r = M_mm + Ψᵀ·M_ss·Ψ`) reproduces the full
    // model's lower spectrum rather than approximating it. The condensed block factors ONCE through the CSparse SPD
    // owner and every column solves against that standing factor; the retained pencil terminates on the SAME dense
    // `Evd` the whole-operator route uses, so this row adds a lowering and never a second eigensolver.
    static Fin<SolveResult> Condensed(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, CondensationPolicy condensation, IClock clock) =>
        Lumped(mesh, problem).Bind(mass =>
            Partition(mass, system.Constrained, InertiaFloor(mass, InertiaFraction)).Bind(split =>
                split.Masters.Length < policy.EigenPairs
                    ? Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<condensation-retained-below-pairs:{split.Masters.Length}:pairs={policy.EigenPairs}>"))
                    : split.Masters.Length > condensation.MaxRetained
                        ? Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<condensation-retained-cap:{split.Masters.Length}:cap={condensation.MaxRetained}>"))
                        // `Ψ` is the one allocation the reduction cannot stream, so the refusal carries the MEASURED
                        // product rather than the two counts a reader would have to multiply back.
                        : TransformBytes(split) > condensation.MaxTransformBytes
                        ? Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<condensation-transform-bytes:{TransformBytes(split)}:cap={condensation.MaxTransformBytes}>"))
                        : Reduce(system.Operator, mass, split, condensation.ResidualCap).Bind(reduced =>
                            Spectrum(reduced, policy.EigenPairs).Map(spectrum => Result(problem, policy, split, mass, reduced, spectrum, clock.GetCurrentInstant())))));

    // Full-length modes and a full-mass participation set make the condensed result INDISTINGUISHABLE in shape
    // from the whole-operator one, so every downstream station recovery reads one field layout and one
    // participation convention regardless of which lowering ran; the evidence member alone reports a reduction.
    static SolveResult Result(SolveProblem problem, SolvePolicy policy, DofSplit split, double[] mass, Reduced reduced, (Matrix<double> Modes, ReadOnlyMemory<double> Values, double Defect) spectrum, Instant at) {
        ReadOnlyMemory<double> modes = Recovered(reduced, split, mass.Length, spectrum.Modes, spectrum.Values.Length);
        return new SolveResult(
            problem, policy.Method, modes, Some(spectrum.Values),
            Some(Participated(modes, mass, problem.Dof, spectrum.Values.Length)), Some(ExcitableMass(mass, problem.Dof)),
            mass.Length, 1, 1, spectrum.Defect, true, at) {
            Condensation = Some(new CondensationEvidence(split.Masters.Length, split.Slaves.Length, reduced.Residual, reduced.Conditioning)),
        };
    }

    // Dense `Ψ` is `retained × condensed` doubles held whole; the bound is measured here so both the guard and its
    // refusal read one derivation.
    static long TransformBytes(DofSplit split) => (long)split.Masters.Length * split.Slaves.Length * sizeof(double);

    // Partition carries both directions of the correspondence: the ordered retained and condensed rows, and the
    // per-row position within its own side so the block walk resolves a triplet's home in two array reads.
    readonly record struct DofSplit(int[] Masters, int[] Slaves, int[] MasterOf, int[] SlaveOf);

    // Retained/condensed election DERIVES from the inertia the model carries: a prescribed row leaves the pencil
    // entirely because it has no free amplitude, a free row above the floor is retained, and a free row at or under it
    // is condensed. An election that condenses nothing refuses by name so the method key never labels a run that
    // reduced nothing, and one that retains nothing refuses because a pencil needs a coordinate.
    // Exemption: the two-way index scatter is the measured-kernel statement seam.
    static Fin<DofSplit> Partition(double[] mass, LanguageExt.HashSet<long> constrained, double floor) {
        int[] masterOf = new int[mass.Length], slaveOf = new int[mass.Length];
        List<int> masters = new(mass.Length), slaves = new(mass.Length);
        for (int dof = 0; dof < mass.Length; dof++) {
            masterOf[dof] = -1;
            slaveOf[dof] = -1;
            if (constrained.Contains(dof)) { continue; }
            if (mass[dof] > floor) { masterOf[dof] = masters.Count; masters.Add(dof); }
            else { slaveOf[dof] = slaves.Count; slaves.Add(dof); }
        }
        return masters.Count == 0
            ? Fin.Fail<DofSplit>(new ComputeFault.ModelRejected($"<condensation-no-retained-dof:floor={floor:e3}>"))
            : slaves.Count == 0
                ? Fin.Fail<DofSplit>(new ComputeFault.ModelRejected($"<condensation-no-condensed-dof:retained={masters.Count}:floor={floor:e3}>"))
                : Fin.Succ(new DofSplit([.. masters], [.. slaves], masterOf, slaveOf));
    }

    // Retained pencil carrier: the two dense blocks the terminal consumes, the transformation columns the row
    // recovery re-reads, and the two measured evidence scalars.
    readonly record struct Reduced(Matrix<double> Stiffness, Matrix<double> Mass, double[][] Transform, double Residual, double Conditioning);

    // Three sub-blocks, never four: the condensed-condensed block feeds the sparse SPD factorization, the
    // condensed-retained block feeds BOTH the transformation right-hand sides and the `K_ms·Ψ` sweep (held as CSC,
    // so its adjoint leg binds `TransposeMultiply` and allocates no transposed storage), and the retained-retained
    // block is dense because the retained count is capped. `K_ms` never materializes — it IS `K_smᵀ`.
    // Exemption: the CSR row walk is the measured-kernel statement seam.
    static Fin<(SparseCompressedRowMatrixStorage<double> Condensed, CompressedColumnStorage<double> Coupling, Matrix<double> Retained)> Blocks(
        SparseCompressedRowMatrixStorage<double> csr, DofSplit split) {
        int slaves = split.Slaves.Length, masters = split.Masters.Length;
        List<int> ssRows = new(csr.ValueCount), ssCols = new(csr.ValueCount), smRows = new(csr.ValueCount), smCols = new(csr.ValueCount);
        List<double> ssVals = new(csr.ValueCount), smVals = new(csr.ValueCount);
        Matrix<double> retained = Matrix<double>.Build.Dense(masters, masters);
        for (int row = 0; row < csr.RowCount; row++) {
            for (int slot = csr.RowPointers[row]; slot < csr.RowPointers[row + 1]; slot++) {
                int column = csr.ColumnIndices[slot];
                double value = csr.Values[slot];
                if (split.SlaveOf[row] >= 0 && split.SlaveOf[column] >= 0) { ssRows.Add(split.SlaveOf[row]); ssCols.Add(split.SlaveOf[column]); ssVals.Add(value); }
                else if (split.SlaveOf[row] >= 0 && split.MasterOf[column] >= 0) { smRows.Add(split.SlaveOf[row]); smCols.Add(split.MasterOf[column]); smVals.Add(value); }
                else if (split.MasterOf[row] >= 0 && split.MasterOf[column] >= 0) { retained[split.MasterOf[row], split.MasterOf[column]] += value; }
            }
        }
        return SparseOps.Ingest(SparseFormat.Coo, slaves, slaves, [.. ssRows], [.. ssCols], [.. ssVals])
            .Bind(condensed => SparseOps.Ingest(SparseFormat.Coo, slaves, masters, [.. smRows], [.. smCols], [.. smVals])
                .Map(coupling => (condensed, (CompressedColumnStorage<double>)SparseOps.ToCsc(coupling), retained)));
    }

    static Fin<Reduced> Reduce(SparseCompressedRowMatrixStorage<double> csr, double[] mass, DofSplit split, double cap) =>
        Blocks(csr, split).Bind(blocks =>
            SparseOps.Factor(blocks.Condensed, FactorKind.Spd, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
                .Bind(condensedOp => Transform(condensedOp, blocks.Coupling, split.Slaves.Length, split.Masters.Length, cap)
                    .Bind(transform => Pencil(blocks.Coupling, blocks.Retained, transform.Columns, mass, split, transform.Residual))));

    // One sparse solve per retained column against the STANDING factor carries the condensed rows' static response,
    // and each column crosses `FactoredOp.Solve`'s own true-residual witness at the policy cap. The block defect
    // `K_ss·Ψ + K_sm` accumulates in the SAME fold through one held GEMV seeded with the coupling column itself:
    // `GemvForm.Accumulate(1.0, 1.0)` computes it in one call with no temporary and no second sweep, so the reported
    // reduction residual is measured against the original blocks rather than inferred from a per-column gate.
    // Exemption: the column march over the standing factor is the measured-kernel statement seam.
    static Fin<(double[][] Columns, double Residual)> Transform(FactoredOp condensedOp, CompressedColumnStorage<double> coupling, int slaves, int masters, double cap) {
        double[][] columns = new double[masters][];
        double[] rhs = new double[slaves], defect = new double[slaves];
        double defectMass = 0.0, couplingMass = 0.0;
        for (int column = 0; column < masters; column++) {
            coupling.Column(column, rhs);
            couplingMass += TensorPrimitives.SumOfSquares<double>(rhs);
            rhs.AsSpan().CopyTo(defect);
            TensorPrimitives.Multiply<double>(rhs, -1.0, rhs);
            Fin<double[]> solved = condensedOp.Solve(rhs, cap);
            if (solved.Case is not double[] response) { return solved.Map(static _ => default((double[][], double))); }
            columns[column] = response;
            Fin<Unit> swept = SparseTensorOps.Spmv(condensedOp.A, GemvForm.Accumulate(1.0, 1.0), response, defect);
            if (swept.IsFail) { return swept.Map(static _ => default((double[][], double))); }
            defectMass += TensorPrimitives.SumOfSquares<double>(defect);
        }
        return Math.Sqrt(defectMass) / Math.Max(1.0, Math.Sqrt(couplingMass)) is var residual && residual <= cap
            ? Fin.Succ((columns, residual))
            : Fin.Fail<(double[][], double)>(new ComputeFault.ModelRejected($"<condensation-residual:{residual:e3}:cap={cap:e3}>"));
    }

    // Retained pencil: `K_r = K_mm + K_ms·Ψ` through one adjoint GEMV per column over the SAME coupling storage, and
    // `M_r = M_mm + Ψᵀ·M_ss·Ψ` over the condensed inertia as a diagonal CSC, so condensed mass folds ONTO the
    // retained set instead of vanishing. Both blocks force symmetry before the terminal because `IsSymmetric()`
    // compares by exact `!=` and an accumulated block fails it, and `M_mm` contributes on the diagonal alone because
    // a lumped inertia operator has no off-diagonal term to partition.
    // Exemption: the column sweep over the coupling block is the measured-kernel statement seam.
    static Fin<Reduced> Pencil(CompressedColumnStorage<double> coupling, Matrix<double> retained, double[][] transform, double[] mass, DofSplit split, double residual) {
        int masters = split.Masters.Length, slaves = split.Slaves.Length;
        double[] condensedMass = new double[slaves];
        for (int row = 0; row < slaves; row++) { condensedMass[row] = mass[split.Slaves[row]]; }
        CompressedColumnStorage<double> inertia = SparseOps.Diagonal(condensedMass);
        Matrix<double> reducedMass = Matrix<double>.Build.Dense(masters, masters);
        double[] adjoint = new double[masters], weighted = new double[slaves];
        for (int column = 0; column < masters; column++) {
            Fin<Unit> stiffness = SparseTensorOps.Spmv(coupling, GemvForm.Transposed, transform[column], adjoint);
            if (stiffness.IsFail) { return stiffness.Map(static _ => default(Reduced)); }
            for (int row = 0; row < masters; row++) { retained[row, column] += adjoint[row]; }
            Fin<Unit> weighting = SparseTensorOps.Spmv(inertia, GemvForm.Apply, transform[column], weighted);
            if (weighting.IsFail) { return weighting.Map(static _ => default(Reduced)); }
            reducedMass[column, column] = mass[split.Masters[column]] + TensorPrimitives.Dot<double>(transform[column], weighted);
            for (int row = 0; row < column; row++) {
                double coupled = TensorPrimitives.Dot<double>(transform[row], weighted);
                reducedMass[row, column] = coupled;
                reducedMass[column, row] = coupled;
            }
        }
        Matrix<double> pencil = Admission.Symmetrize(retained);
        return Conditioned(pencil).Map(conditioning => new Reduced(pencil, Admission.Symmetrize(reducedMass), transform, residual, conditioning));
    }

    // One held `Svd(false)` handle answers the conditioning, and a non-finite reading is not laundered into a large
    // number: `ConditionNumber` is `+Inf` exactly when the pencil is rank-deficient, which is a refusal, and a
    // downstream evidence fact would reject the non-finite value at its own admission anyway.
    static Fin<double> Conditioned(Matrix<double> pencil) =>
        pencil.Svd(computeVectors: false).ConditionNumber is var conditioning && double.IsFinite(conditioning)
            ? Fin.Succ(conditioning)
            : Fin.Fail<double>(new ComputeFault.ModelRejected("<condensation-pencil-rank-deficient>"));

    // Retained generalized pencil terminates on the lane's OWN dense route: `Admission.Definite` gates the reduced
    // inertia (a retained row the reduction cannot supply inertia for fails HERE, named), `chol.Factor.LU()` is the
    // sanctioned application of the factor's inverse — `Factor.Transpose()` alone is the silently-wrong half-factor —
    // and the congruence `Ã = L⁻¹·K_r·L⁻ᵀ` (spelled as `L⁻¹·(L⁻¹·K_r)ᵀ`, exact for a symmetric `K_r`) turns the pencil
    // into the symmetric standard problem `DenseOps.Decompose` already owns. `SpectralOps.Decompose` is the one
    // spectral return and carries the block defect, so the reported residual is measured. Back-mapping `φ = L⁻ᵀ·y` IS
    // mass normalization — `φᵀ·M_r·φ = yᵀ·y = 1` by construction — so a second normalize pass is deleted ceremony.
    static Fin<(Matrix<double> Modes, ReadOnlyMemory<double> Values, double Defect)> Spectrum(Reduced reduced, int pairs) =>
        Admission.Definite(reduced.Mass).Bind(chol => {
            LU<double> lower = chol.Factor.LU(), upper = chol.Factor.Transpose().LU();
            Matrix<double> congruent = Admission.Symmetrize(lower.Solve(lower.Solve(reduced.Stiffness).Transpose()));
            return DenseOps.Decompose(congruent, FactorizationKind.Evd).Bind(factorization =>
                factorization is Factorization.Evd { Decomposition: Evd<double> evd }
                && SpectralOps.Decompose(congruent, evd, Symmetricity.Symmetric) is SpectralResult.Symmetric spectrum
                    ? Fin.Succ((upper.Solve(spectrum.Vectors), spectrum.Values.Take(Math.Min(pairs, spectrum.Values.Count)).ToArray().AsMemory(), spectrum.Defect))
                    : Fin.Fail<(Matrix<double>, ReadOnlyMemory<double>, double)>(ComputeFault.Create("<condensation-non-symmetric-evd>")));
        });

    // Full-length modes: a retained row carries its reduced amplitude directly, a condensed row recovers `u_s = Ψ·φ_r`
    // so the returned shape is a real displacement everywhere rather than a hole the consumer reads as zero motion,
    // and a prescribed row is exactly zero because it has no free amplitude. The layout is the same column-major
    // `(n × k)` field the whole-operator route returns — a reduced-length field would silently mis-index every
    // station recovery that slices it by DOF count.
    // Exemption: the scatter over the partition is the measured-kernel statement seam.
    static ReadOnlyMemory<double> Recovered(Reduced reduced, DofSplit split, int dofs, Matrix<double> modes, int pairs) {
        double[] flat = new double[dofs * pairs];
        for (int mode = 0; mode < pairs; mode++) {
            for (int master = 0; master < split.Masters.Length; master++) { flat[mode * dofs + split.Masters[master]] = modes[master, mode]; }
            for (int slave = 0; slave < split.Slaves.Length; slave++) {
                double response = 0.0;
                for (int master = 0; master < split.Masters.Length; master++) { response += reduced.Transform[master][slave] * modes[master, mode]; }
                flat[mode * dofs + split.Slaves[slave]] = response;
            }
        }
        return flat.AsMemory();
    }

    // ONE participation formula both modal routes call: `Γ_d = Σ_i m_i·φ_i·r_{d,i}` over the TRANSLATIONAL degrees
    // of freedom of axis `d`, against mass-normalized modes (`φᵀ·M·φ = 1`), so `Γ_d²` is the effective modal mass in
    // that direction and the seismic gate compares it to `ExcitableMass` on the same axis. Both routes deliver
    // full-length mass-normalized modes — the whole-operator route through its `1/√m` back-scaling, the condensed
    // route through the `L⁻ᵀ` back-map — so neither carries a route-dependent normalization. The modal lane's dof
    // stride is three (continuum) or six (frame) and translations lead each node block, so the axis rows are the
    // first three slots of every stride.
    static ReadOnlyMemory<ModalParticipation> Participated(ReadOnlyMemory<double> modes, double[] mass, int dof, int pairs) {
        ModalParticipation[] factors = new ModalParticipation[pairs];
        int n = mass.Length;
        for (int mode = 0; mode < pairs; mode++) {
            ReadOnlySpan<double> phi = modes.Span.Slice(mode * n, n);
            double x = 0.0, y = 0.0, z = 0.0;
            for (int node = 0; node < n; node += dof) {
                x += mass[node] * phi[node];
                y += mass[node + 1] * phi[node + 1];
                z += mass[node + 2] * phi[node + 2];
            }
            factors[mode] = new ModalParticipation(x, y, z);
        }
        return factors.AsMemory();
    }

    // `rᵀ·M·r` per axis — the total translational inertia an excitation in that direction can reach, and the
    // denominator of the effective-mass fraction the receipt publishes.
    static ModalParticipation ExcitableMass(double[] mass, int dof) {
        double x = 0.0, y = 0.0, z = 0.0;
        for (int node = 0; node < mass.Length; node += dof) { x += mass[node]; y += mass[node + 1]; z += mass[node + 2]; }
        return new ModalParticipation(x, y, z);
    }

    // Geometric-stiffness pencils materialize `n × n` dense and invert a full-order Cholesky factor, so the
    // buckling route reads the SAME dense ceiling the whole-operator modal route reads. No reduced route serves
    // buckling, so the refusal names the ceiling alone.
    static Fin<SolveResult> Buckle(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, IClock clock) =>
        system.Operator.RowCount > policy.MaxDenseDofs
            ? Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<buckle-dense-ceiling:dofs={system.Operator.RowCount}:ceiling={policy.MaxDenseDofs}>"))
            : Prestress(system, policy).Bind(prestress =>
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
                .Map(pairs => new SolveResult(problem, policy.Method, pairs.Vectors, Some(pairs.Values), None, None, system.Rhs.Length, 1, 1, 0.0, true, clock.GetCurrentInstant()))));

    static Fin<double[]> Prestress(ConstrainedSystem system, SolvePolicy policy) =>
        SparseOps.Factor(system.Operator, FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => factored.Solve(system.Rhs, policy.Tolerance * 1e3));

    static double[] GeometricStiffness(DiscreteMesh mesh, SolveProblem problem, double[] prestress, int n) {
        double[] kg = new double[n * n];
        GeometricScatter(mesh, problem, prestress, (row, col, value) => kg[row * n + col] += value);
        return kg;
    }

    static void GeometricScatter(DiscreteMesh mesh, SolveProblem problem, double[] prestress, Action<int, int, double> scatter) {
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof, strain = problem.Physics.StrainDim;
        Func<int, double[]> materialOf = problem.Field.Lower(problem.Physics, problem.Payload);
        ReadOnlySpan<long> conn = mesh.Indices;
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            double[] d = materialOf(cell);
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            foreach ((double X, double Y, double Z, double Weight) gauss in mesh.Element.Quadrature.Points) {
                ShapeSample sample = mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz);
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
                        for (int i = 0; i < dof; i++) { scatter(ga + i, gb + i, weight * g); }
                    }
            }
        }
    }

    static Fin<(ReadOnlyMemory<double> Vectors, ReadOnlyMemory<double> Values)> BucklingPairs(Factorization factorization, int pairs, Matrix<double> linvT) {
        if (factorization is not Factorization.Evd { Decomposition: Evd<double> evd }) { return Fin.Fail<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>(ComputeFault.Create("<buckle-non-evd>")); }
        (int Index, double Factor)[] ordered = Enumerable.Range(0, evd.EigenValues.Count)
            .Select(k => (Index: k, Factor: Math.Abs(evd.EigenValues[k].Real) > 1e-12 ? 1.0 / evd.EigenValues[k].Real : double.PositiveInfinity))
            .Where(static p => double.IsFinite(p.Factor))
            .OrderBy(static p => Math.Abs(p.Factor))
            .Take(pairs).ToArray();
        int n = evd.EigenVectors.RowCount;
        double[] flat = new double[n * ordered.Length];
        for (int m = 0; m < ordered.Length; m++) {
            Vector<double> phi = linvT.Multiply(evd.EigenVectors.Column(ordered[m].Index));
            for (int i = 0; i < n; i++) { flat[m * n + i] = phi[i]; }
        }
        return Fin.Succ((flat.AsMemory(), ordered.Select(static p => p.Factor).ToArray().AsMemory()));
    }

    // Congruence by `M^(-1/2)` on both sides: every row is inertia-bearing here because the mass-singular refusal
    // already named any row that is not, so the reciprocal root is total and needs no floor.
    static Matrix<double> MassNormalized(Matrix<double> stiffness, double[] mass) {
        int n = stiffness.RowCount;
        double[] inv = new double[n];
        for (int i = 0; i < n; i++) { inv[i] = 1.0 / Math.Sqrt(mass[i]); }
        return Matrix<double>.Build.Dense(n, n, (r, c) => stiffness[r, c] * inv[r] * inv[c]);
    }

    // This route carries the retained count, never re-derives it: values and vectors truncate to the same number here, and
    // a consumer inferring it from either length alone reads a different roster the moment the decomposition returns
    // fewer columns than the policy asked for.
    static Fin<(ReadOnlyMemory<double> Vectors, ReadOnlyMemory<double> Values, int Count)> EigenPairs(Factorization factorization, int pairs, double[] mass) =>
        factorization is Factorization.Evd { Decomposition: Evd<double> evd }
            ? Fin.Succ(Retained(evd, Math.Min(pairs, evd.EigenVectors.ColumnCount), mass))
            : Fin.Fail<(ReadOnlyMemory<double>, ReadOnlyMemory<double>, int)>(ComputeFault.Create("<modal-non-evd>"));

    static (ReadOnlyMemory<double> Vectors, ReadOnlyMemory<double> Values, int Count) Retained(Evd<double> evd, int modes, double[] mass) =>
        (PhysicalModes(evd, modes, mass), evd.EigenValues.Take(modes).Select(static c => c.Real).ToArray().AsMemory(), modes);

    // Back-scaling by `1/√m` IS mass normalization for the congruent problem — `φᵀ·M·φ = yᵀ·y = 1` — so the returned
    // modes are the ones the shared participation formula reads, with no second normalize pass.
    static ReadOnlyMemory<double> PhysicalModes(Evd<double> evd, int modes, double[] mass) {
        int n = evd.EigenVectors.RowCount;
        double[] flat = new double[n * modes];
        for (int mode = 0; mode < modes; mode++) {
            Vector<double> phi = evd.EigenVectors.Column(mode);
            for (int i = 0; i < n; i++) { flat[mode * n + i] = phi[i] / Math.Sqrt(mass[i]); }
        }
        return flat.AsMemory();
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

// Transfer rows are EXPLICIT (donor, receiver) index pairs per weight, never a positional map: two coupled fields
// are discretized independently, so binding donor slot `k` to receiver dof `k` injects one field's boundary values
// onto unrelated degrees of freedom and the staggering converges on an answer nothing asked for. Both ends of every
// pair are range-checked against the field they index — a donor row past the end of the donor field is a malformed
// coupling, and zero-filling it publishes a transferred zero indistinguishable from a measured one.
public sealed record FieldTransfer(int From, int To, FieldStation Source, FieldStation Target, ImmutableArray<(long Donor, long Receiver)> Pairs, ImmutableArray<double> Weights) {
    public Fin<BoundaryCondition> Lower(ReadOnlyMemory<double> donor, int receiverDofs) {
        if (Pairs.Length != Weights.Length || Pairs.IsEmpty) {
            return Fin.Fail<BoundaryCondition>(new ComputeFault.ModelRejected($"<transfer-pair-weight-shape:{Pairs.Length}≠{Weights.Length}>"));
        }
        long[] nodes = new long[Pairs.Length];
        double[] values = new double[Pairs.Length];
        for (int i = 0; i < Pairs.Length; i++) {
            (long from, long to) = Pairs[i];
            if (from < 0 || from >= donor.Length) {
                return Fin.Fail<BoundaryCondition>(new ComputeFault.ModelRejected($"<transfer-donor-index:{from}:donor={donor.Length}>"));
            }
            if (to < 0 || to >= receiverDofs) {
                return Fin.Fail<BoundaryCondition>(new ComputeFault.ModelRejected($"<transfer-receiver-index:{to}:dofs={receiverDofs}>"));
            }
            nodes[i] = to;
            values[i] = Weights[i] * donor.Span[(int)from];
        }
        return Fin.Succ<BoundaryCondition>(new BoundaryCondition.Dirichlet(Target, nodes, values));
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
    public static Fin<CoupledResult> Couple(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, IClock clock) =>
        !coupling.WellPosed
            ? Fin.Fail<CoupledResult>(ComputeFault.Create($"<coupling-ill-posed:fields={coupling.Fields.Count}>"))
            : coupling.Policy.Scheme.Iterates
                ? Iterate(coupling, meshes, policy, clock)
                : OneShot(coupling, meshes, policy, clock);

    public static ComputeReceipt.Coupling Receipt(CoupledProblem coupling, CoupledResult result, CorrelationId correlation, Duration elapsed) =>
        new(coupling.Policy.Scheme.Key, coupling.Fields.Count, coupling.Transfers.Count, result.Rounds, result.CouplingResidual, result.Converged) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    static Fin<CoupledResult> OneShot(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, IClock clock) =>
        SolveRound(coupling, meshes, policy, Seq<SolveResult>(), clock)
            .Map(fields => new CoupledResult(fields, 1, 0.0, Seq<double>(), true, clock.GetCurrentInstant()));

    static Fin<CoupledResult> Iterate(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, IClock clock) =>
        toSeq(Enumerable.Range(0, coupling.Policy.MaxRounds))
            .Fold(SolveRound(coupling, meshes, policy, Seq<SolveResult>(), clock).Map(fields => (Fields: fields, Round: 1, Residual: double.MaxValue, Omega: coupling.Policy.Relaxation, PriorDelta: Seq<double>(), History: Seq<double>(), Converged: false)),
                (acc, _) => acc.Bind(state => state.Converged
                    ? Fin.Succ(state)
                    : SolveRound(coupling, meshes, policy, state.Fields, clock).Map(next => {
                        Seq<double> delta = Delta(state.Fields, next);
                        double residual = Math.Sqrt(delta.Sum(d => d * d));
                        double omega = coupling.Policy.Aitken ? Aitken(state.Omega, state.PriorDelta, delta) : coupling.Policy.Relaxation;
                        return (Relax(state.Fields, next, omega), state.Round + 1, residual, omega, delta, state.History.Add(omega), residual <= coupling.Policy.Tolerance);
                    })))
            .Bind(state => state.Converged
                ? Fin.Succ(new CoupledResult(state.Fields, state.Round, state.Residual, state.History, true, clock.GetCurrentInstant()))
                : Fin.Fail<CoupledResult>(new ComputeFault.ModelRejected($"<coupling-round-cap:{coupling.Policy.MaxRounds}:residual={state.Residual:e3}>")));

    static Fin<Seq<SolveResult>> SolveRound(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, Seq<SolveResult> prior, IClock clock) =>
        toSeq(Enumerable.Range(0, coupling.Fields.Count)).Fold(Fin.Succ(Seq<SolveResult>()), (acc, index) =>
            acc.Bind(solved => {
                SolveProblem field = coupling.Fields[index];
                int receiverDofs = checked((int)meshes[index].NodeCount) * field.Dof;
                // Gauss-Seidel staggering: a donor already solved this round transfers its fresh field; an unsolved donor falls back to the prior round
                return coupling.Transfers
                    .Filter(t => t.To == index && (t.From < solved.Count || t.From < prior.Count))
                    .Traverse(t => t.Lower(t.From < solved.Count ? solved[t.From].Field : prior[t.From].Field, receiverDofs))
                    .As()
                    .Bind(injected => SolveLane.Solve(field with { Conditions = field.Conditions + injected }, meshes[index], policy, clock))
                    .Map(result => solved.Add(result));
            }));

    static Seq<double> Delta(Seq<SolveResult> previous, Seq<SolveResult> current) =>
        previous.Count != current.Count
            ? Seq(double.MaxValue)
            : toSeq(Enumerable.Range(0, current.Count)).Bind(field => {
                ReadOnlySpan<double> a = previous[field].Field.Span, b = current[field].Field.Span;
                List<double> diffs = new(b.Length);
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

(none)
