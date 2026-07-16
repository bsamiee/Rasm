# [COMPUTE_SOLVE_CONTRACT]

Rasm.Compute solve contract: one `PhysicsKind`×`BoundaryCondition`×`ElementClass` axis admits FEA, CFD, thermal, daylight, energy, acoustic, electromagnetic, frame, and multi-physics problems as uniform `SolveProblem` rows on the discretized field. `SolveLane` is the one static fold assembling the discrete operator over the `Solver/discretization#DISCRETIZATION_MESH` `DiscreteMesh` — the isoparametric `Bᵀ·D·B` fold for continuum elements, the closed-form 12-DOF member-stiffness scatter for the frame `ElementClass` rows — then dispatching to numeric-lane factorization or iterative solve, marching the transient/nonlinear loop, and driving the adaptive-recovery ladder; `CoupledLane` is the multi-physics fold over field sets bound by `FieldTransfer` rows under Aitken Δ²-relaxation. Owned vocabulary: `PhysicsKind`/`BoundaryCondition`/`ConstraintMethod`/`SolveMethod`/`TimeIntegrator`/`CouplingScheme`/`RecoveryAction`, the `MaterialField` elastic-or-scalar coefficient carrier, the `SolveProblem`/`SolveResult`/`ConstrainedSystem` carriers, the `RecoveryPolicy`/`CouplingPolicy` policies, and the `SolveLane`/`CoupledLane` folds.

Dense/sparse factorization and iterative solve ride `Tensor/blas#DENSE_ALGEBRA`/`Tensor/factor#SPARSE_SOLVE`; generalized eigenanalysis reuses the verified dense `Evd` terminal after mass or geometric-stiffness reduction. `ElementClass.Sample`/`QuadratureRule`/`DiscreteMesh`/`FieldSpace` and the frame member-stiffness rows arrive settled from `Solver/discretization#DISCRETIZATION_MESH`, the per-Gauss-point constitutive axis (`ConstitutiveModel`/`StressUpdate`/`MaterialState`) from `Solver/constitutive#CONSTITUTIVE`, the gradient-adjoint tape rides `Tensor/dispatch#EQUIVALENCE_INTEROP`, and a distributed solve dials the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc. `SolveProblem.ContentKey` composes the kernel `ContentHash.Of` seed-zero rail over canonical bytes — never a per-call-site hash over a formatted string. Every solver receipt is typed, and the page carries no TS_PROJECTION because solve interiors stay host-local behind the `Solve` rpc.

## [01]-[INDEX]

- [01]-[SOLVE_CONTRACT]: physics×BC×element solve axis; transient/nonlinear; sparse/dense eigen; frame members; multi-physics; recovery.

## [02]-[SOLVE_CONTRACT]

- Owner: `PhysicsKind` `[SmartEnum<string>]` carries symmetry, lifecycle, `MaterialForm`, and `OperatorForm`; `PhysicsPayload` `[Union]` carries continuum, mixed-flow, radiosity, energy-network, Helmholtz, and eddy-current data; `BoundaryCondition` `[Union]` and `ConstraintMethod` own DOF constraints; `SolveMethod` `[SmartEnum<string>]` carries numeric or continuation lowering; `TimeIntegrator`, `CouplingScheme`, and `RecoveryAction` own their rows; `MaterialField` `[Union]` carries uniform or per-cell coefficients; `SolveProblem` binds every discriminant and payload; `SolveLane` and `CoupledLane` own execution.
- Cases: `PhysicsKind` fea-static · fea-modal · fea-transient · fea-buckling · cfd-incompressible · thermal-steady · thermal-transient · daylight-radiosity · energy-balance · acoustic-helmholtz · electromagnetic-eddy; `BoundaryCondition` `Dirichlet` · `Neumann` · `Robin` · `Periodic` · `Contact`; `ConstraintMethod` elimination · penalty · lagrange; `SolveMethod` direct-lu · direct-cholesky · bicgstab · gpbicg · tfqmr · mlk-bicgstab · arc-length · dense-evd; `TimeIntegrator` backward-euler · newmark-beta · generalized-alpha · central-difference; `CouplingScheme` one-way · two-way · staggered; `RecoveryAction` refine-mesh · relax · reorder-dofs · switch-method · restart.
- Entry: `public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, ClockPolicy clocks)` — `Fin<T>` aborts on an ill-posed BC set or a non-convergent run past the cap; a modal row returns eigenpairs through the verified dense `Evd` route, a buckling row the load factors over the geometric-stiffness pencil, a transient row marches the `TimeIntegrator` over the step set reusing one factorization, a nonlinear row (or any problem carrying a `Solver/constitutive` law) drives a modified Newton-Raphson line-searching the nonlinear internal-force residual, and every other row the field over the `FieldSpace`; `SolveAdaptive(…, RecoveryPolicy recovery, …)` walks the `RecoveryAction` ladder on a `Fin.Fail`; `CoupledLane.Couple(CoupledProblem coupling, Seq<DiscreteMesh> meshes, …)` solves the coupled field set under Aitken-relaxed staggering.
- Auto: `Solve` folds elasticity/diffusion as `Bᵀ·D·B`, mixed incompressible flow as velocity-gradient/pressure coupling plus advective transport, Helmholtz as stiffness minus wave-number mass, and eddy current as doubled-real curl-curl plus conductivity coupling. `Radiosity` lowers `I − diag(ρ)F`, and `EnergyNetwork` lowers its conductance matrix. Second-order structural transients use Newmark/generalized-α/central difference; thermal, flow, and energy rows use a factored first-order capacity march. `ArcLength` enforces the Crisfield displacement/load constraint through predictor-corrector iterations across limit points.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics/method/constraint keys, DOF count, iteration count, final residual, converged flag, and elapsed; the modal row stamps the recovered eigenvalue count and modal participation factors, the transient rows the integrator key and step count, the nonlinear rows the Newton iteration count and load-step list, and the iterative rows ride the `rasm.compute.solve.residual` histogram; the `Coupling` case carries the scheme key, field/transfer/round counts, Aitken factor history, final coupling residual, and converged flag; the `RecoveryReceipt` carries the physics key and the ordered `(action, post-recovery residual)` step list plus the recovered flag.
- Packages: MathNet.Numerics, CSparse, Rasm (project — the kernel `ContentHash.Of` identity entry), Rasm.Element (project — the seam `MaterialPropertySet.Mechanical` elasticity read), System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new physics domain is one `PhysicsKind` row plus one `PhysicsPayload` case only when its operator data differs; a new BC is one `BoundaryCondition` case; a new constraint application is one `ConstraintMethod` row; a new numeric or continuation method is one `SolveMethod` row carrying its lowering and policy; a new material assignment is one `MaterialField` case; a new time scheme is one `TimeIntegrator` row; a new coupling discipline is one `CouplingScheme` row plus a `FieldTransfer`. `CfdSolver`/`ThermalSolver`/`FeaSolver`, `NewmarkSolver`/`GeneralizedAlphaSolver`, `ArcLengthSolver`, and `FsiCoupler`/`ThermalStructuralCoupler` siblings collapse onto `SolveLane`/`TimeIntegrator`/`CoupledLane`.
- Boundary: one `Solve` owns every physics, boundary-condition, element, payload, and time-scheme combination. `PhysicsKind.Operator` must match the `PhysicsPayload` case, and admission rejects cardinality, range, or coefficient failures before assembly. `SparseOps.Ingest` consumes assembled COO data, `SparseOps.Factor`/`FactoredOp.Solve`/`SolveIterative` own sparse solution, and `DenseOps.Decompose` owns modal reduction. `ConstraintMethod` mutates both operator and right-hand side. `SolveProblem.ContentKey` hashes the full mesh, payload, conditions, members, and constitutive law. `CoupledLane` transfers fields under Aitken Δ² relaxation, and `SolveAdaptive` records each recovery rung.

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
    public sealed record Contact(long[] Slave, long[] Master, double Gap, double Penalty) : BoundaryCondition;

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
            contact: static (n, bc) => bc.Master.Length == bc.Slave.Length && bc.Master.Length > 0 && InRange(bc.Master, n) && InRange(bc.Slave, n) && double.IsFinite(bc.Gap) && double.IsFinite(bc.Penalty) && bc.Penalty > 0.0
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
            contact: static (s, bc) => {
                double[] rhs = (double[])s.System.Rhs.Clone();
                double force = bc.Penalty * Math.Max(0.0, bc.Gap);
                int pairs = Math.Min(bc.Slave.Length, bc.Master.Length);
                for (int i = 0; i < bc.Slave.Length; i++) { rhs[bc.Slave[i]] += force; }
                for (int i = 0; i < pairs; i++) { rhs[bc.Master[i]] -= force; }
                return s.System with { Rhs = rhs };
            });

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
    Option<ArcLengthPolicy> Continuation) {
    public static readonly SolvePolicy CanonicalStatic = new(SolveMethod.DirectCholesky, ConstraintMethod.Elimination, TimeIntegrator.BackwardEuler, MaxIterations: 1, Tolerance: 1e-9, EigenPairs: 0, TimeStep: 0.0, TimeSteps: 1, NewtonIterations: 1, PenaltyFactor: 1e12, Continuation: None);
    public static readonly SolvePolicy CanonicalIterative = CanonicalStatic with { Method = SolveMethod.BiCgStab, MaxIterations = 2000, Tolerance = 1e-8 };
    public static readonly SolvePolicy CanonicalModal = CanonicalStatic with { Method = SolveMethod.DenseEvd, MaxIterations = 500, Tolerance = 1e-7, EigenPairs = 12 };
    public static readonly SolvePolicy CanonicalTransient = CanonicalStatic with { Method = SolveMethod.DirectLu, Integrator = TimeIntegrator.NewmarkBeta, TimeStep = 0.01, TimeSteps = 100 };
    public static readonly SolvePolicy CanonicalNonlinear = CanonicalIterative with { Method = SolveMethod.MlkBiCgStab, NewtonIterations = 25 };
    public static readonly SolvePolicy CanonicalArcLength = CanonicalNonlinear with { Method = SolveMethod.ArcLength, Continuation = Some(new ArcLengthPolicy(0.05, 1.0, 40, 1e-7)) };

    public Fin<Unit> Validate(SolveProblem problem) =>
        MaxIterations <= 0 || !double.IsFinite(Tolerance) || Tolerance <= 0.0 || !double.IsFinite(PenaltyFactor) || PenaltyFactor <= 0.0 || Continuation.Exists(static continuation => continuation.Invalid)
            ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-iteration>"))
            : (Method == SolveMethod.ArcLength) != Continuation.IsSome
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<solve-policy-continuation-discriminant>"))
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

    public sealed record UniformElastic(double Young, double Poisson) : MaterialField;
    public sealed record UniformScalar(double Scale) : MaterialField;
    public sealed record PerCellElastic(ImmutableArray<double> Young, ImmutableArray<double> Poisson) : MaterialField;
    public sealed record PerCellScalar(ImmutableArray<double> Scale) : MaterialField;

    public static readonly MaterialField Unit = new UniformScalar(1.0);

    public static Fin<MaterialField> OfMechanical(Seq<Option<MaterialPropertySet.Mechanical>> perCell) =>
        perCell.Traverse(static row => row.ToFin(new ComputeFault.AssessmentInputMissing("<material-field:member-without-mechanical-case>")))
            .Map(static rows => (MaterialField)new PerCellElastic(
                [.. rows.Map(static m => m.YoungsModulus.Si)],
                [.. rows.Map(static m => m.PoissonsRatio)]));

    public Fin<(double Young, double Poisson)> MechanicalAt(int cell) =>
        Switch(
            state: cell,
            uniformElastic: static (_, assignment) => Fin.Succ((assignment.Young, assignment.Poisson)),
            uniformScalar: static (_, _) => Fin.Fail<(double, double)>(new ComputeFault.ModelRejected("<frame-requires-elastic-material>")),
            perCellElastic: static (index, assignment) => Fin.Succ((assignment.Young[index], assignment.Poisson[index])),
            perCellScalar: static (_, _) => Fin.Fail<(double, double)>(new ComputeFault.ModelRejected("<frame-requires-elastic-material>")));

    public Fin<Unit> Validate(long cells, MaterialForm form) {
        bool elastic = form == MaterialForm.Elasticity;
        bool valid = this switch {
            UniformElastic assignment => elastic && Positive(assignment.Young) && PoissonValid(assignment.Poisson),
            UniformScalar assignment => !elastic && Positive(assignment.Scale),
            PerCellElastic assignment => elastic && assignment.Young.Length == cells && assignment.Poisson.Length == cells && assignment.Young.All(Positive) && assignment.Poisson.All(PoissonValid),
            PerCellScalar assignment => !elastic && assignment.Scale.Length == cells && assignment.Scale.All(Positive),
            _ => false,
        };
        return valid ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<material-field:{form}:{cells}>"));
    }

    public Func<int, double[]> Lower(PhysicsKind physics, PhysicsPayload payload) =>
        Switch(
            state: (Physics: physics, Payload: payload),
            uniformElastic: static (row, assignment) => Cached(row.Physics.Material(assignment.Young, assignment.Poisson, row.Payload)),
            uniformScalar: static (row, assignment) => Cached(row.Physics.Material(assignment.Scale, 0.0, row.Payload)),
            perCellElastic: static (row, assignment) => cell => row.Physics.Material(assignment.Young[cell], assignment.Poisson[cell], row.Payload),
            perCellScalar: static (row, assignment) => cell => row.Physics.Material(assignment.Scale[cell], 0.0, row.Payload));

    static Func<int, double[]> Cached(double[] d) => _ => d;

    public void WriteCanonical(ArrayBufferWriter<byte> sink) =>
        Switch(
            state: sink,
            uniformElastic: static (writer, assignment) => {
                writer.Write("ue"u8);
                WriteScalars(writer, [assignment.Young, assignment.Poisson]);
            },
            uniformScalar: static (writer, assignment) => {
                writer.Write("us"u8);
                WriteScalars(writer, [assignment.Scale]);
            },
            perCellElastic: static (writer, assignment) => {
                writer.Write("pe"u8);
                Span<byte> count = stackalloc byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(count, assignment.Young.Length); writer.Write(count);
                WriteScalars(writer, assignment.Young); WriteScalars(writer, assignment.Poisson);
            },
            perCellScalar: static (writer, assignment) => {
                writer.Write("ps"u8);
                Span<byte> count = stackalloc byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(count, assignment.Scale.Length); writer.Write(count);
                WriteScalars(writer, assignment.Scale);
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
        BinaryPrimitives.WriteInt32LittleEndian(scratch, law.Prony.Count); sink.Write(scratch[..4]);
        foreach ((double modulus, double relaxation) in law.Prony) { Write(modulus); Write(relaxation); }
        sink.Write([(byte)(law.Soil.IsSome ? 1 : 0)]);
        law.Soil.IfSome(soil => {
            Write(soil.FrictionAngle); Write(soil.DilationAngle); Write(soil.Cohesion); Write(soil.CriticalStateSlope);
            Write(soil.CompressionIndex); Write(soil.SwellIndex); Write(soil.InitialPreconsolidationPressure); Write(soil.InitialPorePressure);
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
    static SolveKind Routed(SolveProblem problem, SolveMethod method) =>
        method == SolveMethod.ArcLength ? SolveKind.Nonlinear
        : problem.Material.IsSome ? SolveKind.Nonlinear
        : problem.Physics.Eigen ? SolveKind.Eigen
        : problem.Physics.Transient ? SolveKind.Transient
        : problem.Physics.Nonlinear ? SolveKind.Nonlinear
        : method.Iterative ? SolveKind.Iterative
        : SolveKind.Direct;

    public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, ClockPolicy clocks) =>
        from policyValid in policy.Validate(problem)
        from operatorCsr in Assemble(problem, mesh, policy)
        from system in Constrained(operatorCsr, problem.Conditions, policy)
        from result in Routed(problem, policy.Method).Switch(
            state: (System: system, Mesh: mesh, Problem: problem, Policy: policy, At: clocks.Now, Clocks: clocks),
            direct: static state => Direct(state.System, state.Problem, state.Policy, state.At),
            iterative: static state => Iterative(state.System, state.Problem, state.Policy, state.At),
            nonlinear: static state => Newton(state.System, state.Mesh, state.Problem, state.Policy, state.Clocks),
            transient: static state => March(state.System, state.Mesh, state.Problem, state.Policy, state.At),
            eigen: static state => Modal(state.System, state.Mesh, state.Problem, state.Policy, state.Clocks))
        select result;

    public static (Fin<SolveResult> Result, RecoveryReceipt Trace) SolveAdaptive(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, RecoveryPolicy recovery, ClockPolicy clocks) {
        (Fin<SolveResult> Result, SolveProblem Problem, DiscreteMesh Mesh, SolvePolicy Policy, Seq<(string Action, double Residual)> Steps) final = recovery.Ladder.Fold(
            (Result: Solve(problem, mesh, policy, clocks), Problem: problem, Mesh: mesh, Policy: policy, Steps: Seq<(string Action, double Residual)>()),
            (state, action) => {
                if (state.Result.IsSucc) { return state; }
                return Recover(action, state.Problem, state.Mesh, state.Policy, recovery, clocks).Match(
                    Succ: next => {
                        Fin<SolveResult> attempt = Solve(next.Problem, next.Mesh, next.Policy, clocks);
                        return (attempt, next.Problem, next.Mesh, next.Policy, state.Steps.Add((action.Key, Residual(attempt))));
                    },
                    Fail: fault => (Fin.Fail<SolveResult>(fault), state.Problem, state.Mesh, state.Policy, state.Steps.Add((action.Key, double.PositiveInfinity))));
            });
        return (final.Result, new RecoveryReceipt(problem.Physics.Key, final.Steps, final.Result.IsSucc, clocks.Now));
    }

    static Fin<(SolveProblem Problem, DiscreteMesh Mesh, SolvePolicy Policy)> Recover(RecoveryAction action, SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, RecoveryPolicy recovery, ClockPolicy clocks) =>
        action.Switch(
            state: (Problem: problem, Mesh: mesh, Policy: policy, Recovery: recovery, Clocks: clocks),
            refineMesh: static s => MeshKernel.Refine(s.Mesh, s.Recovery.MeshPolicy, RefinementError(s.Mesh), s.Clocks)
                .Map(refined => (s.Problem with { Element = refined.Element }, refined, s.Policy)),
            relax: static s => Fin.Succ((s.Problem, s.Mesh, s.Policy with { Tolerance = s.Policy.Tolerance * s.Recovery.RelaxFactor, MaxIterations = (int)(s.Policy.MaxIterations * s.Recovery.IterationGrowth) })),
            reorderDofs: static s => Reordered(s.Problem, s.Mesh, s.Policy, s.Clocks),
            switchMethod: static s => Fin.Succ((s.Problem, s.Mesh, s.Policy with { Method = s.Recovery.Fallback })),
            restart: static s => Fin.Succ((s.Problem, s.Mesh, s.Policy with { Method = s.Recovery.Fallback, MaxIterations = s.Policy.MaxIterations * 2 })));

    static Fin<(SolveProblem Problem, DiscreteMesh Mesh, SolvePolicy Policy)> Reordered(SolveProblem problem, DiscreteMesh mesh, SolvePolicy policy, ClockPolicy clocks) {
        int dof = problem.Dof, nodes = checked((int)mesh.NodeCount);
        return Triplets(mesh, problem, policy).Map(t => {
                CoordinateStorage<double> coords = new(nodes, nodes, t.Vals.Length);
                for (int entry = 0; entry < t.Vals.Length; entry++) { coords.At(t.Rows[entry] / dof, t.Cols[entry] / dof, t.Vals[entry]); }
                CompressedColumnStorage<double> csc = CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
                int[] permutation = AMD.Generate(csc, ColumnOrdering.MinimumDegreeAtPlusA);
                return (problem, Renumbered(mesh, permutation, clocks), policy);
            });
    }

    static DiscreteMesh Renumbered(DiscreteMesh mesh, int[] permutation, ClockPolicy clocks) {
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
        return mesh with { Nodes = reordered.AsMemory(), Connectivity = renumberedConn.AsMemory(), At = clocks.Now };
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
            .Map(_ => (assembly.Rows, assembly.Cols, assembly.Vals));
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

    static double[] Lumped(DiscreteMesh mesh, int dof) {
        int nodes = checked((int)mesh.NodeCount), per = mesh.Element.Nodes;
        double[] mass = new double[nodes * dof];
        ReadOnlySpan<long> conn = mesh.Indices;
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
            double volume = 0.0;
            if (mesh.Element.Family == ShapeFamily.Frame) {
                double dx = xyz[3] - xyz[0], dy = xyz[4] - xyz[1], dz = xyz[5] - xyz[2];
                volume = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
            else {
                foreach ((double X, double Y, double Z, double Weight) gauss in mesh.Element.Quadrature.Points) {
                    volume += gauss.Weight * Math.Abs(mesh.Element.Sample((gauss.X, gauss.Y, gauss.Z), xyz).DetJ);
                }
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
        conditions.Traverse(condition => condition.Validate(operatorCsr.RowCount))
            .Bind(_ => conditions.Fold(
                Fin.Succ(new ConstrainedSystem(operatorCsr, new double[operatorCsr.RowCount], Prelude.HashSet<long>(), policy.PenaltyFactor)),
                (acc, condition) => acc.Bind(system => condition.Apply(system, policy.Constraint))));

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

    static Fin<SolveResult> March(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at) {
        double[] lumped = Lumped(mesh, problem.Dof);
        return problem.Physics.Form == MaterialForm.Elasticity
            ? policy.Integrator.Implicit ? Newmark(system, problem, policy, lumped, at) : CentralDifference(system, problem, policy, lumped, at)
            : FirstOrder(system, problem, policy, Capacity(problem, lumped), at);
    }

    static double[] Capacity(SolveProblem problem, double[] lumped) => problem.Payload switch {
        PhysicsPayload.EnergyNetwork energy => energy.Capacity.ToArray(),
        PhysicsPayload.Flow => [.. lumped.Select((value, index) => index % 4 == 3 ? 0.0 : value)],
        _ => lumped,
    };

    static Fin<SolveResult> FirstOrder(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, double[] capacity, Instant at) {
        int n = system.Rhs.Length;
        double[] effective = (double[])system.Operator.Values.Clone();
        int[] rows = system.Operator.RowPointers, columns = system.Operator.ColumnIndices;
        foreach (long constrained in system.Constrained) { capacity[(int)constrained] = 0.0; }
        for (int row = 0; row < n; row++)
            for (int slot = rows[row]; slot < rows[row + 1]; slot++)
                if (columns[slot] == row) { effective[slot] += capacity[row] / policy.TimeStep; }
        SparseCompressedRowMatrixStorage<double> effectiveCsr = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(n, n, effective.Length, rows, columns, effective);
        return SparseOps.Factor(effectiveCsr, FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => toSeq(Enumerable.Range(0, policy.TimeSteps))
                .Fold(Fin.Succ(new double[n]), (acc, _) => acc.Bind(previous => {
                    double[] forcing = (double[])system.Rhs.Clone();
                    for (int i = 0; i < n; i++) { forcing[i] += capacity[i] * previous[i] / policy.TimeStep; }
                    return factored.Solve(forcing, policy.Tolerance * 1e3);
                })))
            .Map(field => new SolveResult(problem, policy.Method, field.AsMemory(), None, None, None, n, policy.TimeSteps, 1, 0.0, true, at));
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
        SparseCompressedRowMatrixStorage<double> effectiveCsr = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            system.Operator.RowCount, system.Operator.RowCount, effective.Length, rowPtr, colIdx, effective);
        SparseMatrix tangent = new(system.Operator);
        double a0 = 1.0 / (beta * dt * dt), a1 = gamma / (beta * dt), a2 = 1.0 / (beta * dt), a3 = 1.0 / (2.0 * beta) - 1.0,
               a4 = gamma / beta - 1.0, a5 = dt * 0.5 * (gamma / beta - 2.0), a6 = dt * (1.0 - gamma), a7 = gamma * dt;
        double alphaMass = policy.Integrator.AlphaMass, alphaForce = policy.Integrator.AlphaForce;
        return SparseOps.Factor(effectiveCsr, FactorKind.Lu, ColumnOrdering.MinimumDegreeAtPlusA, 1.0, 0.0)
            .Bind(factored => toSeq(Enumerable.Range(0, policy.TimeSteps))
                .Fold(Fin.Succ((U: new double[n], V: new double[n], A: new double[n])),
                    (acc, _) => acc.Bind(s => factored.Solve(NewmarkForce(system.Rhs, lumped, tangent, s.U, s.V, s.A, rayleigh, alphaMass, alphaForce, a0, a1, a2, a3, a4, a5), policy.Tolerance * 1e3)
                        .Map(next => Correct(next, s.U, s.V, s.A, a0, a2, a3, a6, a7))))
                .Map(state => new SolveResult(problem, policy.Method, state.U.AsMemory(), None, None, None, n, policy.TimeSteps, 1, 0.0, true, at)));
    }

    static double[] NewmarkForce(double[] forcing, double[] mass, SparseMatrix tangent, double[] u, double[] v, double[] a, double rayleigh, double alphaMass, double alphaForce, double a0, double a1, double a2, double a3, double a4, double a5) {
        int n = forcing.Length;
        double[] massCombo = new double[n], dampCombo = new double[n], priorStiffness = tangent.Multiply(Vector<double>.Build.DenseOfArray(u)).AsArray();
        for (int i = 0; i < n; i++) {
            massCombo[i] = mass[i] * ((1.0 - alphaMass) * (a0 * u[i] + a2 * v[i] + a3 * a[i]) - alphaMass * a[i]);
            dampCombo[i] = (1.0 - alphaForce) * (a1 * u[i] + a4 * v[i] + a5 * a[i]) - alphaForce * v[i];
        }
        double[] dampForce = tangent.Multiply(Vector<double>.Build.DenseOfArray(dampCombo)).AsArray();
        double[] force = new double[n];
        for (int i = 0; i < n; i++) { force[i] = forcing[i] + massCombo[i] + rayleigh * dampForce[i] - alphaForce * priorStiffness[i]; }
        return force;
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

    static Fin<SolveResult> CentralDifference(ConstrainedSystem system, SolveProblem problem, SolvePolicy policy, double[] lumped, Instant at) {
        int n = system.Rhs.Length;
        double dt = policy.TimeStep, dt2 = dt * dt;
        const double rayleigh = 0.05;
        SparseMatrix tangent = new(system.Operator);
        double[] effMass = new double[n];
        for (int i = 0; i < n; i++) { effMass[i] = lumped[i] / dt2 + rayleigh * lumped[i] / (2.0 * dt); }
        (double[] Curr, double[] Prev) marched = toSeq(Enumerable.Range(0, policy.TimeSteps))
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

    static Fin<SolveResult> Newton(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, ClockPolicy clocks) =>
        policy.Continuation.Match(
            Some: continuation => ArcLength(system, mesh, problem, policy, continuation, clocks),
            None: () => NewtonLoad(system, mesh, problem, policy, clocks));

    static Fin<SolveResult> NewtonLoad(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, ClockPolicy clocks) {
        SparseMatrix tangent = new(system.Operator);
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
                                .Bind(run => ArmijoLineSearch(mesh, problem, tangent, system.Rhs, state.Field, run.Field, norm, clocks).Map(alpha => {
                                    double[] updated = new double[state.Field.Length];
                                    TensorPrimitives.MultiplyAdd(run.Field, alpha, state.Field, updated);
                                    return (updated, norm, state.Step + 1, false);
                                }));
                    })))
            .Bind(state => state.Converged
                ? Fin.Succ(new SolveResult(problem, policy.Method, state.Field.AsMemory(), None, None, None, system.Rhs.Length, state.Step, state.Step, state.Residual, true, clocks.Now))
                : Fin.Fail<SolveResult>(new ComputeFault.ModelRejected($"<solve-newton-cap:{policy.NewtonIterations}:residual={state.Residual:e3}>")));
    }

    static Fin<SolveResult> ArcLength(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, ArcLengthPolicy path, ClockPolicy clocks) {
        IterationPolicy iteration = Iteration(policy);
        SparseMatrix tangent = new(system.Operator);
        Func<double[], Fin<double[]>> solve = rhs => policy.Method.Krylov
            .ToFin(ComputeFault.Create("<arc-length-inner-method>"))
            .Bind(krylov => SparseOps.SolveIterative(system.Operator, krylov, rhs, iteration))
            .Bind(run => run.Terminal is SolveTerminal.Admitted
                ? Fin.Succ(run.Field.ToArray())
                : Fin.Fail<double[]>(new ComputeFault.ModelRejected($"<arc-length-linear-diverged:{run.Residual:e3}>")));
        return toSeq(Enumerable.Range(0, path.Steps))
            .Fold(
                Fin.Succ((Field: new double[system.Rhs.Length], Load: 0.0, Direction: new double[system.Rhs.Length], Iterations: 0, Residual: double.MaxValue)),
                (outer, _) => outer.Bind(state => solve(system.Rhs).Bind(loadDirection => {
                    double orientation = TensorPrimitives.Dot(loadDirection, state.Direction) < 0.0 ? -1.0 : 1.0;
                    double loadIncrement = orientation * path.Radius / Math.Sqrt(TensorPrimitives.SumOfSquares<double>(loadDirection) + path.LoadScale * path.LoadScale);
                    double[] origin = (double[])state.Field.Clone();
                    double originLoad = state.Load;
                    double[] predicted = new double[state.Field.Length];
                    TensorPrimitives.MultiplyAdd(loadDirection, loadIncrement, state.Field, predicted);
                    return toSeq(Enumerable.Range(0, policy.NewtonIterations))
                        .Fold(Fin.Succ((Field: predicted, Load: state.Load + loadIncrement, Converged: false, Residual: double.MaxValue, Iterations: 0)),
                            (inner, _) => inner.Bind(point => point.Converged
                                ? Fin.Succ(point)
                                : InternalForce(mesh, problem, tangent, point.Field, clocks).Bind(internalForce => {
                                    double[] forcing = new double[system.Rhs.Length];
                                    TensorPrimitives.Multiply(system.Rhs, point.Load, forcing);
                                    double[] residual = Residual(forcing, internalForce);
                                    double norm = TensorPrimitives.Norm<double>(residual);
                                    if (norm <= path.ResidualTolerance * Math.Max(1.0, TensorPrimitives.Norm<double>(forcing))) {
                                        return Fin.Succ((point.Field, point.Load, true, norm, point.Iterations));
                                    }
                                    return from correction in solve(residual)
                                           from response in solve(system.Rhs)
                                           from corrected in ArcCorrect(point.Field, point.Load, origin, originLoad, correction, response, path)
                                           select (corrected.Field, corrected.Load, false, norm, point.Iterations + 1);
                                })))
                        .Bind(point => point.Converged
                            ? Fin.Succ((point.Field, point.Load, ArcDirection(point.Field, origin), state.Iterations + point.Iterations, point.Residual))
                            : Fin.Fail<(double[], double, double[], int, double)>(new ComputeFault.ModelRejected($"<arc-length-corrector-cap:{policy.NewtonIterations}:residual={point.Residual:e3}>")));
                })))
            .Map(state => new SolveResult(problem, policy.Method, state.Field.AsMemory(), None, None, None, system.Rhs.Length, state.Iterations, state.Iterations, state.Residual, true, clocks.Now));
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

    static Fin<double[]> InternalForce(DiscreteMesh mesh, SolveProblem problem, SparseMatrix tangent, double[] field, ClockPolicy clocks) =>
        problem.Material.Match(
            Some: law => Constitutive(mesh, problem, law.Model, law.Law, field, clocks),
            None: () => Fin.Succ(tangent.Multiply(Vector<double>.Build.DenseOfArray(field)).AsArray()));

    static Fin<double[]> Constitutive(DiscreteMesh mesh, SolveProblem problem, ConstitutiveModel model, MaterialParameters law, double[] field, ClockPolicy clocks) {
        int per = mesh.Element.Nodes, dof = problem.Dof, block = per * dof;
        bool finiteStrain = model is ConstitutiveModel.Hyperelastic;
        int components = finiteStrain ? 9 : problem.Physics.StrainDim;
        double[] global = new double[field.Length];
        ReadOnlySpan<long> conn = mesh.Indices;
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            ReadOnlySpan<double> xyz = mesh.NodalXyz(cell);
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
                Fin<ConstitutiveResult> update = StressUpdate.Stress(model, gaussStrain.AsMemory(), MaterialState.Pristine(components), law, clocks);
                Fin<Unit> accumulated = update.Map(result => {
                    ReadOnlySpan<double> stress = result.Stress.Span;
                    for (int i = 0; i < block; i++) {
                        double f = 0.0;
                        for (int r = 0; r < components; r++) { f += b[r * block + i] * (r < stress.Length ? stress[r] : 0.0); }
                        global[(int)conn[cell * per + i / dof] * dof + i % dof] += weight * f;
                    }
                    return unit;
                });
                if (accumulated.IsFail) { return accumulated.Map(static _ => Array.Empty<double>()); }
            }
        }
        return Fin.Succ(global);
    }

    static Fin<double> ArmijoLineSearch(DiscreteMesh mesh, SolveProblem problem, SparseMatrix tangent, double[] forcing, double[] field, double[] direction, double baseline, ClockPolicy clocks) =>
        toSeq(Enumerable.Range(0, 8)).Fold(
            Fin.Succ((Alpha: 1.0, Accepted: false)),
            (acc, _) => acc.Bind(state => state.Accepted
                ? Fin.Succ(state)
                : Fin.Succ(state).Bind(current => {
                    double[] trial = new double[field.Length];
                    TensorPrimitives.MultiplyAdd(direction, current.Alpha, field, trial);
                    return InternalForce(mesh, problem, tangent, trial, clocks)
                        .Map(force => TensorPrimitives.Norm<double>(Residual(forcing, force)))
                        .Map(norm => norm <= (1.0 - 1e-4 * current.Alpha) * baseline ? (current.Alpha, true) : (current.Alpha * 0.5, false));
                })))
        .Bind(state => state.Accepted
            ? Fin.Succ(state.Alpha)
            : Fin.Fail<double>(new ComputeFault.ModelRejected($"<solve-line-search-cap:residual={baseline:e3}>")));

    static Fin<SolveResult> Modal(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, ClockPolicy clocks) =>
        problem.Physics == PhysicsKind.FeaBuckling
            ? Buckle(system, mesh, problem, policy, clocks)
            : Vibration(system, mesh, problem, policy, clocks.Now);

    static Fin<SolveResult> Vibration(ConstrainedSystem system, DiscreteMesh mesh, SolveProblem problem, SolvePolicy policy, Instant at) {
        double[] mass = Lumped(mesh, problem.Dof);
        return DenseOps.Decompose(MassNormalized(Matrix<double>.Build.OfStorage(system.Operator), mass), FactorizationKind.Evd)
            .Bind(factorization => EigenPairs(factorization, policy.EigenPairs, mass))
            .Map(pairs => new SolveResult(problem, policy.Method, pairs.Vectors, Some(pairs.Values), Some(pairs.Participation), Some(mass.Sum()), system.Rhs.Length, 1, 1, 0.0, true, at));
    }

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

    static Matrix<double> MassNormalized(Matrix<double> stiffness, double[] mass) {
        int n = stiffness.RowCount;
        double[] inv = new double[n];
        for (int i = 0; i < n; i++) { inv[i] = 1.0 / Math.Sqrt(Math.Max(1e-30, mass[i])); }
        return Matrix<double>.Build.Dense(n, n, (r, c) => stiffness[r, c] * inv[r] * inv[c]);
    }

    static Fin<(ReadOnlyMemory<double> Vectors, ReadOnlyMemory<double> Values, ReadOnlyMemory<double> Participation)> EigenPairs(Factorization factorization, int pairs, double[] mass) =>
        factorization is Factorization.Evd { Decomposition: Evd<double> evd }
            ? Fin.Succ((
                PhysicalModes(evd, Math.Min(pairs, evd.EigenVectors.ColumnCount), mass),
                evd.EigenValues.Take(pairs).Select(static c => c.Real).ToArray().AsMemory(),
                Participation(evd, Math.Min(pairs, evd.EigenVectors.ColumnCount), mass)))
            : Fin.Fail<(ReadOnlyMemory<double>, ReadOnlyMemory<double>, ReadOnlyMemory<double>)>(ComputeFault.Create("<modal-non-evd>"));

    static ReadOnlyMemory<double> PhysicalModes(Evd<double> evd, int modes, double[] mass) {
        int n = evd.EigenVectors.RowCount;
        double[] flat = new double[n * modes];
        for (int mode = 0; mode < modes; mode++) {
            Vector<double> phi = evd.EigenVectors.Column(mode);
            for (int i = 0; i < n; i++) { flat[mode * n + i] = phi[i] / Math.Sqrt(Math.Max(1e-30, mass[i])); }
        }
        return flat.AsMemory();
    }

    static ReadOnlyMemory<double> Participation(Evd<double> evd, int modes, double[] mass) {
        double[] factors = new double[modes];
        for (int mode = 0; mode < modes; mode++) {
            Vector<double> phi = evd.EigenVectors.Column(mode);
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
    public static Fin<CoupledResult> Couple(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, ClockPolicy clocks) =>
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
            .Fold(SolveRound(coupling, meshes, policy, Seq<SolveResult>(), clocks).Map(fields => (Fields: fields, Round: 1, Residual: double.MaxValue, Omega: coupling.Policy.Relaxation, PriorDelta: Seq<double>(), History: Seq<double>(), Converged: false)),
                (acc, _) => acc.Bind(state => state.Converged
                    ? Fin.Succ(state)
                    : SolveRound(coupling, meshes, policy, state.Fields, clocks).Map(next => {
                        Seq<double> delta = Delta(state.Fields, next);
                        double residual = Math.Sqrt(delta.Sum(d => d * d));
                        double omega = coupling.Policy.Aitken ? Aitken(state.Omega, state.PriorDelta, delta) : coupling.Policy.Relaxation;
                        return (Relax(state.Fields, next, omega), state.Round + 1, residual, omega, delta, state.History.Add(omega), residual <= coupling.Policy.Tolerance);
                    })))
            .Bind(state => state.Converged
                ? Fin.Succ(new CoupledResult(state.Fields, state.Round, state.Residual, state.History, true, clocks.Now))
                : Fin.Fail<CoupledResult>(new ComputeFault.ModelRejected($"<coupling-round-cap:{coupling.Policy.MaxRounds}:residual={state.Residual:e3}>")));

    static Fin<Seq<SolveResult>> SolveRound(CoupledProblem coupling, Seq<DiscreteMesh> meshes, SolvePolicy policy, Seq<SolveResult> prior, ClockPolicy clocks) =>
        toSeq(Enumerable.Range(0, coupling.Fields.Count)).Fold(Fin.Succ(Seq<SolveResult>()), (acc, index) =>
            acc.Bind(solved => {
                SolveProblem field = coupling.Fields[index];
                // Gauss-Seidel staggering: a donor already solved this round transfers its fresh field; an unsolved donor falls back to the prior round
                Seq<BoundaryCondition> injected = coupling.Transfers
                    .Filter(t => t.To == index && (t.From < solved.Count || t.From < prior.Count))
                    .Map(t => t.Lower(t.From < solved.Count ? solved[t.From].Field : prior[t.From].Field));
                SolveProblem stamped = field with { Conditions = field.Conditions + injected };
                return SolveLane.Solve(stamped, meshes[index], policy, clocks).Map(result => solved.Add(result));
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

- [GRADIENT_ADJOINT_WIRING]-[OPEN]: what live design-mesh at the shape-optimization/inverse-design call site supplies the `MeshAdjointSnapshot` the reverse-mode tape records through `MeshAdjointSnapshot.Of` — the DDG-operator VJP bodies, the `TensorOpKind.Geometry` rows, and the `Solver/optimizer#OPTIMIZER_LANE` `DesignProblem.OperatorRows` lowering are settled on `Tensor/dispatch#EQUIVALENCE_INTEROP`, so only the design-mesh wiring is open; the optimization call site.
