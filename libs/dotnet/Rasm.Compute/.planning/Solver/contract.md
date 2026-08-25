# [COMPUTE_SOLVE_CONTRACT]

Rasm.Compute solve contract: one `PhysicsKind`×`BoundaryCondition`×`ElementClass` axis admits FEA, CFD, thermal, daylight, energy, acoustic, electromagnetic, frame, and multi-physics problems as uniform `SolveProblem` rows on the discretized field. This page owns the ADMISSION and the DISPATCH — the physics vocabulary, the route algebra, the lane policy, the problem and result carriers, and the one `SolveLane.Solve` fold that admits, assembles through `Solver/assembly#OPERATOR_ASSEMBLY`, and hands the constrained system to the `Solver/route#SOLVE_ROUTES` body its route names.

`SolveRoute` is the one discriminant. The nine-arm ladder that once derived a keyless kind from four physics bools, a method key, a material option, and a contact scan collapses onto a closed union whose case CARRIES its own payload — an eigen route carries its pair count, a transient route its integrator and step grid, a continuation route its arc-length policy, a condensed route its reduction budget — so the policy columns that only some routes read stop being `Option`s a validator cross-checks against a method key, and the three nested route ternaries become arms of one total `Switch`.

`Convergence` is the one bounded-budget verdict. Every iterative fold in the folder — the Newton loop, the arc-length corrector, the coupling rounds, the constitutive return map, and the optimizer and estimator lanes above them — reports `Converged`, `Exhausted`, or `Stalled` rather than a tuple carrying a `bool` a caller cannot distinguish from a success.

The mesh producer is `Solver/discretization#MESH_GENERATION` `MeshLane.Discretize`: the Discretize-solve-optimize-sweep spine runs generation, admission, assembly, and route in that order, so a caller hands a boundary shell and a mesh policy and never hand-builds a mesh past the quality gate. `ElementClass.Sample`/`DiscreteMesh`/`FieldSpace` and the frame member rows arrive settled from `Solver/element#ELEMENT_TOPOLOGY` and `Solver/field#DISCRETE_FIELD`, whose admitted mesh carries the PROVEN `QuadratureRule` its policy elected; the per-Gauss-point constitutive axis (`ConstitutiveModel`/`StressUpdate`/`ConstitutiveState`) comes from `Solver/constitutive#CONSTITUTIVE`; dense and sparse factorization ride `Tensor/blas#DENSE_ALGEBRA` and `Tensor/factor#SPARSE_SOLVE`; a distributed solve dials the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc.

`SolveProblem.ContentKey` composes the kernel `ContentHash.Of<TState>` seed-zero rail through `CanonicalWriter`, never a hand-framed byte walk over a formatted string. Every solver result is typed, and the page carries no TS_PROJECTION because solve interiors stay host-local behind the `Solve` rpc.

## [01]-[INDEX]

- [02]-[SOLVE_REQUEST]: physics×BC×element admission axis, the route algebra, the lane policy, the problem/result carriers, and the one dispatch fold.

## [02]-[SOLVE_REQUEST]

- Owner: `PhysicsKind` `[SmartEnum<string>]` carries its regime, its operator symmetry, its `MaterialForm`, and its `OperatorForm`; `PhysicsRegime` `[SmartEnum<string>]` closes the static/modal/transient/buckling/nonlinear axis the four adjacent bools once spelled as sixteen corners; `OperatorSymmetry` `[SmartEnum<string>]` closes which factorizations an operator admits; `OperatorForm` `[SmartEnum<string>]` owns the payload admission and the constitutive coefficient BOTH, so a payload the row does not admit is unrepresentable past `SolveProblem.Of` and no assembly body re-tests a pairing; `PhysicsPayload` `[Union]` carries continuum, mixed-flow, radiosity, energy-network, Helmholtz, and eddy-current data; `SolveMethod` `[SmartEnum<string>]` carries the numeric lowering; `Preconditioner` and `TimeIntegrator` own their rows; `SolveRoute` `[Union]` is the ONE route discriminant and carries each route's own payload; `Convergence` `[Union]` is the ONE bounded-budget verdict; `RayleighPair` `[ComplexValueObject]` is the damping pair; `MaterialField` `[Union]` carries uniform or per-cell elastic coefficients including density, or a scalar coefficient beside its volumetric heat capacity; `LanePolicy` binds the route-free lane budgets; `SolveSession` is the lane-owned standing factorization; `SolveProblem`/`SolveResult`/`ModalParticipation`/`CondensationEvidence` are the carriers; `SolveLane` owns admission, dispatch, and the result.
- Cases: `PhysicsKind` fea-static · fea-modal · fea-transient · fea-buckling · cfd-incompressible · thermal-steady · thermal-transient · daylight-radiosity · energy-balance · acoustic-helmholtz · electromagnetic-eddy; `PhysicsRegime` static · modal · transient · buckling · nonlinear; `SolveRoute` `Direct` · `Iterative` · `Transient` · `Traced` · `Nonlinear` · `Continuation` · `Vibration` · `Condensed` · `Buckling`; `Convergence` `Converged` · `Exhausted` · `Stalled`; `ConstraintMethod` elimination · penalty · lagrange; `SolveMethod` direct-lu · direct-cholesky · bicgstab · gpbicg · tfqmr · mlk-bicgstab · dense-evd; `TimeIntegrator` backward-euler · newmark-beta · generalized-alpha · central-difference. `Traced` carries a `FieldIntegrator` instead of a `TimeIntegrator` row, because an error-controlled march elects its step rather than reading one off a policy grid.
- Entry: `public static Fin<SolveResult> Solve(SolveProblem problem, DiscreteMesh mesh, LanePolicy policy, SolveRoute route, IClock clock, Option<SolveArchive> archive = default, Option<SolveSession> session = default)` — the policy and the problem arrive ADMITTED, the route is the caller's declared discriminant, and `Fin<T>` aborts on an ill-posed BC set or a route body's own refusal. `SolveLane.Discretized(BoundaryShell, MeshPolicy, …)` is the spine entry that generates the mesh through `MeshLane.Discretize` and folds straight into `Solve`, so the generation half has one consumer and no caller assembles over a mesh that skipped the quality gate.
- Auto: `Solve` validates the policy against the route and the problem on ONE accumulating pass, assembles through `Solver/assembly`, applies the boundary conditions, and dispatches the route through the generated total `Switch` — every arm a body on `Solver/route`, none reached by a nested ternary. A supplied `SolveSession` carries a standing factorization whose PATTERN the assembly reproduces, so a sweep over material combinations pays the symbolic phase once.
- Result: `SolveResult` carries the problem, method, route, field, measured residual, DOF count, iteration and Newton-step counts, convergence verdict, modal values and participation, total mass, and optional condensation and quadrature evidence. Its converged flag derives from the convergence verdict.
- Packages: MathNet.Numerics, CSparse, Rasm (project — kernel `ContentHash.Of<TState>`/`CanonicalWriter`, `Dimension`/`PositiveMagnitude`/`Band`, `Op`), Rasm.Element (project — the seam `MaterialPropertySet.Mechanical` elasticity and density reads beside the `Thermal` `SpecificHeat` read the volumetric capacity composes), System.Numerics (`Vector3` — the contact normal), System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new physics domain is one `PhysicsKind` row with one `PhysicsPayload` case and one `OperatorForm` row only when its operator data differs; a new route is one `SolveRoute` case carrying its own payload plus one arm on the dispatch `Switch`; a new numeric method is one `SolveMethod` row; a new material assignment is one `MaterialField` case; a new time scheme is one `TimeIntegrator` row carrying its own march delegate; a new lane budget is one `LanePolicy` column the criterion stack reads through `IterationPolicy.Of`. `CfdSolver`/`ThermalSolver`/`FeaSolver`, `NewmarkSolver`/`GeneralizedAlphaSolver`, and `ArcLengthSolver` siblings collapse onto `SolveLane` and the route union.
- Boundary: the route is a UNION with payload, never a keyless tag beside a policy bag of `Option`s. The two cross-checks the old validator ran — that an arc-length method carries a continuation policy and a condensed-evd method carries a reduction policy — tested a discriminant against a payload that a union case pairs by construction, so both refusals delete with the shape that made them possible.
- Boundary: `PhysicsKind`'s regime is ONE row, not four adjacent bools. The four-bool product spelled sixteen corners for six legal ones, and the eight-arm ladder that read them derived a keyless kind by testing them in an order nothing declared; the regime row carries the fact and the route derivation reads it beside the material and contact discriminants alone.
- Boundary: `OperatorForm` owns BOTH halves of the payload correspondence — the admission the problem gate runs and the constitutive coefficient the assembly reads. A bare tag beside a six-arm admission switch and a four-arm coefficient switch put one fact in three places, and the two `InvalidOperationException` throws that guarded the mismatch were domain control flow standing in for a pairing the row can state.
- Boundary: `SolveProblem.ContentKey` frames through the kernel `CanonicalWriter`: tolerance is PART OF THE KEY, `-0.0` canonicalizes, and every variable-width run carries its own count, none of which a hand `ArrayBufferWriter<byte>` walk with eight bespoke framing helpers provided. Capacity is in the canonical bytes, so two runs differing only in heat capacity key apart.
- Boundary: a factorization is a LIVE resource, so it rides `SolveSession` and never a policy column — the same law that keeps `SolveArchive` off the policy. The session holds the standing factor for a pattern-stable family and every re-solve re-values it through the `Tensor/factor#SPARSE_SOLVE` owner's own `Edit.Revalue`, which reuses the cached permutation and yields an INDEPENDENT factor; the in-place CSparse `Refactorize` is refused at that owner because it mutates the shared instance the pre-edit value still aliases, and composing it here would reach past a ruling this lane does not own.
- Boundary: one `Solve` owns every physics, boundary-condition, element, payload, and time-scheme combination. `ConstraintMethod` mutates both operator and right-hand side. Dense/sparse factorization and iterative solve ride the `Tensor` funnels; generalized eigenanalysis reuses the verified dense `Evd` terminal after mass, static-condensation, or geometric-stiffness reduction, because reducing a generalized pencil to standard form demands a positive-definite inertia factor a lumped-mass frame lacks on its inertia-free rows.
- Boundary: wall budget and cancellation are `LanePolicy` columns the lane composes onto a canonical row and reach the criterion stack through `IterationPolicy.Of` beside the `Solve` argument clock, so every iterative leg bounds wall time off the one clock the result durations read; a canonical row binding a clock static, a deadline parameter grown onto the entry signature, or a per-leg literal cap is the rejected form.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum]
public sealed partial class MaterialForm {
    public static readonly MaterialForm Elasticity = new(dof: 3, strainDim: 6, ElasticityMatrix);
    public static readonly MaterialForm Isotropic = new(dof: 1, strainDim: 3, IsotropicMatrix);
    public static readonly MaterialForm MixedFlow = new(dof: 4, strainDim: 10, IsotropicMatrix);
    public static readonly MaterialForm MaxwellEddy = new(dof: 6, strainDim: 6, IsotropicMatrix);

    public int Dof { get; }
    public int StrainDim { get; }

    [UseDelegateFromConstructor]
    public partial double[] Continuum(double scale, double poisson);

    static double[] ElasticityMatrix(double e, double nu) {
        double lambda = e * nu / ((1 + nu) * (1 - 2 * nu)), mu = e / (2 * (1 + nu));
        return [lambda + 2 * mu, lambda, lambda, 0, 0, 0, lambda, lambda + 2 * mu, lambda, 0, 0, 0, lambda, lambda, lambda + 2 * mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu, 0, 0, 0, 0, 0, 0, mu];
    }

    static double[] IsotropicMatrix(double diagonal, double _) => [diagonal, 0, 0, 0, diagonal, 0, 0, 0, diagonal];
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
public sealed partial class OperatorForm {
    public static readonly OperatorForm Continuum = new("continuum", dense: false, AdmitContinuum, LowerContinuum);
    public static readonly OperatorForm Flow = new("flow", dense: false, AdmitFlow, LowerFlow);
    public static readonly OperatorForm Radiosity = new("radiosity", dense: true, AdmitRadiosity, LowerNetwork);
    public static readonly OperatorForm EnergyNetwork = new("energy-network", dense: true, AdmitNetwork, LowerNetwork);
    public static readonly OperatorForm Helmholtz = new("helmholtz", dense: false, AdmitHelmholtz, LowerContinuum);
    public static readonly OperatorForm EddyCurrent = new("eddy-current", dense: false, AdmitEddy, LowerEddy);

    public bool Dense { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Admits(PhysicsPayload payload, long cells, int nodes);

    [UseDelegateFromConstructor]
    public partial Fin<double[]> Coefficient(MaterialForm form, double scale, double poisson, PhysicsPayload payload);

    static Fin<Unit> AdmitContinuum(PhysicsPayload payload, long cells, int nodes) =>
        Require(payload is PhysicsPayload.Continuum, payload);
    static Fin<Unit> AdmitFlow(PhysicsPayload payload, long cells, int nodes) =>
        Require(payload is PhysicsPayload.Flow flow && flow.Velocity.Length == cells * 3
            && Positive(flow.Density) && Positive(flow.Viscosity) && Positive(flow.PressureStabilization), payload);
    static Fin<Unit> AdmitRadiosity(PhysicsPayload payload, long cells, int nodes) =>
        Require(payload is PhysicsPayload.Radiosity row && row.Reflectance.Length == nodes && row.ViewFactors.Length == nodes * nodes
            && TensorPrimitives.IsFiniteAll<double>(row.Reflectance.Span) && TensorPrimitives.IsFiniteAll<double>(row.ViewFactors.Span)
            && TensorPrimitives.Min(row.Reflectance.Span) >= 0.0 && TensorPrimitives.Max(row.Reflectance.Span) <= 1.0, payload);
    static Fin<Unit> AdmitNetwork(PhysicsPayload payload, long cells, int nodes) =>
        Require(payload is PhysicsPayload.EnergyNetwork row && row.Capacity.Length == nodes && row.Conductance.Length == nodes * nodes
            && TensorPrimitives.IsFiniteAll<double>(row.Capacity.Span) && TensorPrimitives.IsFiniteAll<double>(row.Conductance.Span), payload);
    static Fin<Unit> AdmitHelmholtz(PhysicsPayload payload, long cells, int nodes) =>
        Require(payload is PhysicsPayload.Helmholtz wave && Positive(wave.WaveNumber), payload);
    static Fin<Unit> AdmitEddy(PhysicsPayload payload, long cells, int nodes) =>
        Require(payload is PhysicsPayload.EddyCurrent eddy && Positive(eddy.Permeability)
            && double.IsFinite(eddy.Conductivity) && eddy.Conductivity >= 0.0 && Positive(eddy.AngularFrequency), payload);

    static Fin<double[]> LowerContinuum(MaterialForm form, double scale, double poisson, PhysicsPayload _) =>
        Fin.Succ(form.Continuum(scale, poisson));
    static Fin<double[]> LowerFlow(MaterialForm form, double scale, double poisson, PhysicsPayload payload) =>
        payload is PhysicsPayload.Flow flow
            ? Fin.Succ(Square(10, (row, column) => row == column && row < 9 ? flow.Viscosity
                : row == 9 && column == 9 ? flow.PressureStabilization
                : row == 9 && column is 0 or 4 or 8 || column == 9 && row is 0 or 4 or 8 ? -1.0 : 0.0))
            : Require(false, payload).Map(static _ => Array.Empty<double>());
    static Fin<double[]> LowerEddy(MaterialForm form, double scale, double poisson, PhysicsPayload payload) =>
        payload is PhysicsPayload.EddyCurrent eddy
            ? Fin.Succ(Square(6, (row, column) => row == column ? 1.0 / eddy.Permeability : 0.0))
            : Require(false, payload).Map(static _ => Array.Empty<double>());
    static Fin<double[]> LowerNetwork(MaterialForm form, double scale, double poisson, PhysicsPayload payload) =>
        Fin.Fail<double[]>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.Type(payload.GetType()))));

    static Fin<Unit> Require(bool held, PhysicsPayload payload) =>
        held ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Type(payload.GetType()))));
    static bool Positive(double value) => double.IsFinite(value) && value > 0.0;

    static double[] Square(int size, Func<int, int, double> cell) {
        double[] values = new double[size * size];
        for (int row = 0; row < size; row++) for (int column = 0; column < size; column++) { values[row * size + column] = cell(row, column); }
        return values;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhysicsRegime {
    public static readonly PhysicsRegime Static = new("static");
    public static readonly PhysicsRegime Modal = new("modal");
    public static readonly PhysicsRegime Transient = new("transient");
    public static readonly PhysicsRegime Buckling = new("buckling");
    public static readonly PhysicsRegime Nonlinear = new("nonlinear");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OperatorSymmetry {
    public static readonly OperatorSymmetry Symmetric = new("symmetric", static _ => true);
    public static readonly OperatorSymmetry General = new("general", static kind => kind != FactorizationKind.Cholesky);

    [UseDelegateFromConstructor]
    public partial bool Admits(FactorizationKind kind);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhysicsKind {
    public static readonly PhysicsKind FeaStatic = new("fea-static", PhysicsRegime.Static, OperatorSymmetry.Symmetric, MaterialForm.Elasticity, OperatorForm.Continuum);
    public static readonly PhysicsKind FeaModal = new("fea-modal", PhysicsRegime.Modal, OperatorSymmetry.Symmetric, MaterialForm.Elasticity, OperatorForm.Continuum);
    public static readonly PhysicsKind FeaTransient = new("fea-transient", PhysicsRegime.Transient, OperatorSymmetry.Symmetric, MaterialForm.Elasticity, OperatorForm.Continuum);
    public static readonly PhysicsKind FeaBuckling = new("fea-buckling", PhysicsRegime.Buckling, OperatorSymmetry.Symmetric, MaterialForm.Elasticity, OperatorForm.Continuum);
    public static readonly PhysicsKind CfdIncompressible = new("cfd-incompressible", PhysicsRegime.Transient, OperatorSymmetry.General, MaterialForm.MixedFlow, OperatorForm.Flow);
    public static readonly PhysicsKind ThermalSteady = new("thermal-steady", PhysicsRegime.Static, OperatorSymmetry.Symmetric, MaterialForm.Isotropic, OperatorForm.Continuum);
    public static readonly PhysicsKind ThermalTransient = new("thermal-transient", PhysicsRegime.Transient, OperatorSymmetry.Symmetric, MaterialForm.Isotropic, OperatorForm.Continuum);
    public static readonly PhysicsKind DaylightRadiosity = new("daylight-radiosity", PhysicsRegime.Static, OperatorSymmetry.General, MaterialForm.Isotropic, OperatorForm.Radiosity);
    public static readonly PhysicsKind EnergyBalance = new("energy-balance", PhysicsRegime.Transient, OperatorSymmetry.Symmetric, MaterialForm.Isotropic, OperatorForm.EnergyNetwork);
    public static readonly PhysicsKind AcousticHelmholtz = new("acoustic-helmholtz", PhysicsRegime.Static, OperatorSymmetry.Symmetric, MaterialForm.Isotropic, OperatorForm.Helmholtz);
    public static readonly PhysicsKind ElectromagneticEddy = new("electromagnetic-eddy", PhysicsRegime.Static, OperatorSymmetry.General, MaterialForm.MaxwellEddy, OperatorForm.EddyCurrent);

    public PhysicsRegime Regime { get; }
    public OperatorSymmetry Symmetry { get; }
    public MaterialForm Form { get; }
    public OperatorForm Operator { get; }

    public int Dof => Form.Dof;
    public int StrainDim => Form.StrainDim;
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

    public Option<IterativeMethod> Krylov => Optional(krylov);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Preconditioner {
    public static readonly Preconditioner None = new("none", static () => new UnitPreconditioner<double>());
    public static readonly Preconditioner Diagonal = new("diagonal", static () => new DiagonalPreconditioner());

    [UseDelegateFromConstructor]
    public partial IPreconditioner<double> Build();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimeIntegrator {
    public static readonly TimeIntegrator BackwardEuler = new("backward-euler", alphaMass: 0.0, alphaForce: 0.0, beta: 1.0, gamma: 1.0, SolveRoutes.Newmark);
    public static readonly TimeIntegrator NewmarkBeta = new("newmark-beta", alphaMass: 0.0, alphaForce: 0.0, beta: 0.25, gamma: 0.5, SolveRoutes.Newmark);
    public static readonly TimeIntegrator GeneralizedAlpha = new("generalized-alpha", alphaMass: 1.0 / 3.0, alphaForce: 4.0 / 9.0, beta: 25.0 / 81.0, gamma: 11.0 / 18.0, SolveRoutes.Newmark);
    public static readonly TimeIntegrator CentralDifference = new("central-difference", alphaMass: 0.0, alphaForce: 0.0, beta: 0.0, gamma: 0.5, SolveRoutes.CentralDifference);

    public double AlphaMass { get; }
    public double AlphaForce { get; }
    public double Beta { get; }
    public double Gamma { get; }

    [UseDelegateFromConstructor]
    public partial Fin<SolveResult> Advance(MarchRequest request);

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
public abstract partial record Convergence {
    private Convergence() { }

    public sealed record Converged(double Residual) : Convergence;
    public sealed record Exhausted(int Budget) : Convergence;
    public sealed record Stalled : Convergence;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolveRoute {
    private SolveRoute() { }

    public sealed record Direct : SolveRoute;
    public sealed record Iterative : SolveRoute;
    public sealed record Transient(TimeIntegrator Integrator, PositiveMagnitude Step, Dimension Steps) : SolveRoute;
    public sealed record Traced(FieldIntegrator Integrator, TrajectoryControl Control, QuadratureControl Accuracy, Seq<double> Stations) : SolveRoute;
    public sealed record Nonlinear(Dimension NewtonIterations) : SolveRoute;
    public sealed record Continuation(Dimension NewtonIterations, ArcLengthPolicy Path) : SolveRoute;
    public sealed record Vibration(Dimension Pairs) : SolveRoute;
    public sealed record Condensed(Dimension Pairs, CondensationPolicy Reduction) : SolveRoute;
    public sealed record Buckling(Dimension Pairs) : SolveRoute;

    public string Key => Switch(
        direct: static _ => "direct", iterative: static _ => "iterative",
        transient: static row => row.Integrator.Key, traced: static row => row.Integrator.Kind.Key,
        nonlinear: static _ => "newton",
        continuation: static _ => "arc-length", vibration: static _ => "dense-evd",
        condensed: static _ => "condensed-evd", buckling: static _ => "buckling");

    public PhysicsRegime Regime => Switch(
        direct: static _ => PhysicsRegime.Static, iterative: static _ => PhysicsRegime.Static,
        transient: static _ => PhysicsRegime.Transient, traced: static _ => PhysicsRegime.Transient,
        nonlinear: static _ => PhysicsRegime.Nonlinear,
        continuation: static _ => PhysicsRegime.Nonlinear, vibration: static _ => PhysicsRegime.Modal,
        condensed: static _ => PhysicsRegime.Modal, buckling: static _ => PhysicsRegime.Buckling);
}

public sealed record ArcLengthPolicy(PositiveMagnitude Radius, PositiveMagnitude LoadScale, Dimension Steps, PositiveMagnitude ResidualTolerance);

public sealed record CondensationPolicy(Dimension MaxRetained, PositiveMagnitude ResidualCap, long MaxTransformBytes) {
    public static readonly CondensationPolicy Canonical = new(
        Dimension.Create(1_024), PositiveMagnitude.Create(1e-8), MaxTransformBytes: 512L * 1024 * 1024);
}

public readonly record struct CondensationEvidence(int Retained, int Condensed, double Residual, double Conditioning);

[ComplexValueObject]
public sealed partial class RayleighPair {
    public static RayleighPair Structural { get; } = Create(mass: 0.0, stiffness: 0.05);

    public double Mass { get; }
    public double Stiffness { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double mass, ref double stiffness) =>
        validationError = Band.Nonnegative.Guard(label: nameof(Mass), value: ref mass)
            ?? Band.Nonnegative.Guard(label: nameof(Stiffness), value: ref stiffness);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record LanePolicy {
    public static readonly LanePolicy CanonicalStatic = new(
        SolveMethod.DirectCholesky, ConstraintMethod.Elimination,
        Dimension.Create(1), PositiveMagnitude.Create(1e-9), PositiveMagnitude.Create(1e12),
        RayleighPair.Structural, Dimension.Create(20_000), Duration.MaxValue, CancellationToken.None);
    public static readonly LanePolicy CanonicalIterative = CanonicalStatic with {
        Method = SolveMethod.BiCgStab, MaxIterations = Dimension.Create(2000), Tolerance = PositiveMagnitude.Create(1e-8) };
    public static readonly LanePolicy CanonicalModal = CanonicalStatic with {
        Method = SolveMethod.DenseEvd, MaxIterations = Dimension.Create(500), Tolerance = PositiveMagnitude.Create(1e-7) };
    public static readonly LanePolicy CanonicalTransient = CanonicalStatic with { Method = SolveMethod.DirectLu };
    public static readonly LanePolicy CanonicalNonlinear = CanonicalIterative with { Method = SolveMethod.MlkBiCgStab };

    private LanePolicy(
        SolveMethod method, ConstraintMethod constraint, Dimension maxIterations, PositiveMagnitude tolerance,
        PositiveMagnitude penaltyFactor, RayleighPair damping, Dimension maxDenseDofs, Duration deadline, CancellationToken cancel) {
        (Method, Constraint, MaxIterations, Tolerance) = (method, constraint, maxIterations, tolerance);
        (PenaltyFactor, Damping, MaxDenseDofs, Deadline, Cancel) = (penaltyFactor, damping, maxDenseDofs, deadline, cancel);
    }

    public SolveMethod Method { get; init; }
    public ConstraintMethod Constraint { get; init; }
    public Dimension MaxIterations { get; init; }
    public PositiveMagnitude Tolerance { get; init; }
    public PositiveMagnitude PenaltyFactor { get; init; }
    public RayleighPair Damping { get; init; }
    public Dimension MaxDenseDofs { get; init; }
    public Duration Deadline { get; init; }
    public CancellationToken Cancel { get; init; }

    public Fin<Unit> Admits(SolveProblem problem, SolveRoute route) =>
        Seq(
            Claim(problem.Physics.Regime == route.Regime, new ComputeViolation.Contract(
                ComputeContract.Compatible,
                new ContractEvidence.Keys(problem.Physics.Regime.Key, route.Regime.Key))),
            Claim(problem.Physics.Symmetry.Admits(Method.Kind), new ComputeViolation.Contract(
                ComputeContract.Supported,
                new ContractEvidence.Keys(problem.Physics.Symmetry.Key, Method.Kind.Key))),
            Claim(Deadline > Duration.Zero, new ComputeViolation.Range(
                RangeRequirement.Positive,
                new ScalarEvidence.DurationValue(Deadline))),
            Claim(route is not (SolveRoute.Nonlinear or SolveRoute.Continuation) || Method.Krylov.IsSome,
                new ComputeViolation.Unsupported(ComputeCapability.IterativeSolver)),
            Claim(Constraint != ConstraintMethod.Lagrange
                || route is SolveRoute.Direct or SolveRoute.Iterative or SolveRoute.Nonlinear && Method.Kind != FactorizationKind.Cholesky,
                new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Keys(Constraint.Key, Method.Kind.Key))))
            .Traverse(static claim => claim).As().Map(static _ => unit).ToFin();

    public IterationPolicy Iteration(IClock clock) =>
        IterationPolicy.Of(clock, Deadline, Cancel)
            with { Tolerance = Tolerance.Value, MaxIterations = MaxIterations.Value, Preconditioner = Method.Preconditioner.Build };

    static Validation<Error, Unit> Claim(bool held, ComputeViolation evidence) =>
        held ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new ComputeFault.Violation(ComputeArea.Solver, evidence));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialField {
    private MaterialField() { }

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

    public Fin<(double Young, double Poisson, double Density)> MechanicalAt(int cell) =>
        Switch(
            state: cell,
            uniformElastic: static (_, assignment) => Fin.Succ((assignment.Young, assignment.Poisson, assignment.Density)),
            uniformScalar: static (_, _) => Fin.Fail<(double, double, double)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.ElasticMaterial))),
            perCellElastic: static (index, assignment) => Fin.Succ((assignment.Young[index], assignment.Poisson[index], assignment.Density[index])),
            perCellScalar: static (_, _) => Fin.Fail<(double, double, double)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.ElasticMaterial))));

    public Fin<double> CapacityAt(int cell) =>
        Switch(
            state: cell,
            uniformElastic: static (_, _) => Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.ScalarMaterial))),
            uniformScalar: static (_, assignment) => Fin.Succ(assignment.Capacity),
            perCellElastic: static (_, _) => Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Unsupported(ComputeCapability.ScalarMaterial))),
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
        return valid ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
    }

    public Fin<Func<int, double[]>> Lower(PhysicsKind physics, PhysicsPayload payload) =>
        Switch(
            state: (Physics: physics, Payload: payload),
            uniformElastic: static (row, assignment) => row.Physics.Operator
                .Coefficient(row.Physics.Form, assignment.Young, assignment.Poisson, row.Payload)
                .Map(static held => (Func<int, double[]>)(_ => held)),
            uniformScalar: static (row, assignment) => row.Physics.Operator
                .Coefficient(row.Physics.Form, assignment.Scale, 0.0, row.Payload)
                .Map(static held => (Func<int, double[]>)(_ => held)),
            perCellElastic: static (row, assignment) => row.Physics.Operator
                .Coefficient(row.Physics.Form, assignment.Young[0], assignment.Poisson[0], row.Payload)
                .Map(_ => (Func<int, double[]>)(cell => row.Physics.Operator
                    .Coefficient(row.Physics.Form, assignment.Young[cell], assignment.Poisson[cell], row.Payload).ThrowIfFail())),
            perCellScalar: static (row, assignment) => row.Physics.Operator
                .Coefficient(row.Physics.Form, assignment.Scale[0], 0.0, row.Payload)
                .Map(_ => (Func<int, double[]>)(cell => row.Physics.Operator
                    .Coefficient(row.Physics.Form, assignment.Scale[cell], 0.0, row.Payload).ThrowIfFail())));

    public void WriteCanonical(CanonicalWriter sink) =>
        Switch(
            state: sink,
            uniformElastic: static (writer, assignment) => writer.String("ue").Double(assignment.Young).Double(assignment.Poisson).Double(assignment.Density),
            uniformScalar: static (writer, assignment) => writer.String("us").Double(assignment.Scale).Double(assignment.Capacity),
            perCellElastic: static (writer, assignment) => writer.String("pe")
                .Doubles(assignment.Young.AsSpan()).Doubles(assignment.Poisson.AsSpan()).Doubles(assignment.Density.AsSpan()),
            perCellScalar: static (writer, assignment) => writer.String("ps")
                .Doubles(assignment.Scale.AsSpan()).Doubles(assignment.Capacity.AsSpan()));

    static bool Positive(double value) => double.IsFinite(value) && value > 0.0;
    static bool PoissonValid(double value) => double.IsFinite(value) && value is > -1.0 and < 0.5;
}

[Equatable]
public sealed partial record SolveProblem(
    PhysicsKind Physics,
    ElementClass Element,
    [property: OrderedEquality] Seq<BoundaryCondition> Conditions,
    FieldSpace Unknown,
    MaterialField Field,
    PhysicsPayload Payload,
    [property: OrderedEquality] ImmutableArray<FrameMember> Members,
    Option<(ConstitutiveModel Model, ConstitutiveParameters Law)> Material,
    UInt128 ContentKey) {
    public static readonly Dimension MaxDenseNetworkNodes = Dimension.Create(4_096);

    public int Dof => Element.Family == ShapeFamily.Frame ? 6 : Physics.Dof;

    public static Fin<SolveProblem> Of(
        PhysicsKind physics, DiscreteMesh mesh, Seq<BoundaryCondition> conditions, MaterialField field,
        PhysicsPayload payload, ImmutableArray<FrameMember> members,
        Option<(ConstitutiveModel Model, ConstitutiveParameters Law)> material) {
        bool frame = mesh.Element.Family == ShapeFamily.Frame;
        int dof = frame ? 6 : physics.Dof, nodes = checked((int)mesh.NodeCount);
        return Seq(
            Claim(!frame || members.Length == mesh.ElementCount, new ComputeViolation.Shape(
                ShapeRequirement.Arity,
                new ShapeEvidence.Count(members.Length, mesh.ElementCount))),
            Claim(!physics.Operator.Dense || nodes <= MaxDenseNetworkNodes.Value, new ComputeViolation.Capacity(
                CapacityRequirement.WithinLimit,
                new CapacityEvidence.Count(nodes, MaxDenseNetworkNodes.Value))),
            physics.Operator.Admits(payload, mesh.ElementCount, nodes).ToValidation(),
            field.Validate(mesh.ElementCount, physics.Form).ToValidation())
            .Traverse(static claim => claim).As().ToFin()
            .Map(_ => new SolveProblem(
                physics, mesh.Element, conditions,
                mesh.FieldOf(FieldStation.Nodal, dof == 1 ? FieldRank.Scalar : FieldRank.Vector, dof),
                field, payload, members, material,
                Key(physics, mesh, conditions, field, payload, members, material)));
    }

    static UInt128 Key(
        PhysicsKind physics, DiscreteMesh mesh, Seq<BoundaryCondition> conditions, MaterialField field,
        PhysicsPayload payload, ImmutableArray<FrameMember> members,
        Option<(ConstitutiveModel Model, ConstitutiveParameters Law)> material) =>
        ContentHash.Of(
            (Physics: physics, Mesh: mesh, Conditions: conditions, Field: field, Payload: payload, Members: members, Material: material),
            static (row, sink) => {
                sink.String(row.Physics.Key).String(row.Mesh.Element.Key)
                    .I64(row.Mesh.NodeCount).I64(row.Mesh.ElementCount).Ordinal(row.Physics.Dof);
                sink.Rows(toSeq(row.Mesh.Coordinates.ToArray()), static (ordinate, writer) => writer.Single(ordinate));
                sink.Rows(toSeq(row.Mesh.Indices.ToArray()), static (node, writer) => writer.I64(node));
                sink.Rows(row.Conditions, static (condition, writer) => condition.WriteCanonical(writer));
                row.Field.WriteCanonical(sink);
                row.Payload.WriteCanonical(sink);
                sink.Rows(toSeq(row.Members), static (member, writer) => member.WriteCanonical(writer));
                sink.Optional(row.Material, static (law, writer) => {
                    law.Model.WriteCanonical(writer);
                    law.Law.WriteCanonical(writer);
                });
            });

    static Validation<Error, Unit> Claim(bool held, ComputeViolation evidence) =>
        held ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new ComputeFault.Violation(ComputeArea.Solver, evidence));
}

public readonly record struct ModalParticipation(double X, double Y, double Z);

public sealed record SolveResult(
    SolveProblem Problem,
    SolveMethod Method,
    SolveRoute Route,
    ReadOnlyMemory<double> Field,
    Option<ReadOnlyMemory<double>> EigenValues,
    Option<ReadOnlyMemory<ModalParticipation>> Participation,
    Option<ModalParticipation> TotalMass,
    long Dofs,
    int Iterations,
    int NewtonSteps,
    double Residual,
    Convergence Verdict,
    Instant At) {
    public Option<CondensationEvidence> Condensation { get; init; }

    public Option<QuadratureEvidence> Evidence { get; init; }

    public bool Converged => Verdict is Convergence.Converged;
}

public sealed class SolveSession : IDisposable {
    readonly Atom<FactoredOp> held;

    SolveSession(FactoredOp seed, UInt128 patternKey, FactorKind kind) {
        held = Atom(seed);
        (PatternKey, Kind) = (patternKey, kind);
    }

    public UInt128 PatternKey { get; }
    public FactorKind Kind { get; }

    public static Fin<SolveSession> Open(SparseCompressedRowMatrixStorage<double> pattern, FactorKind kind, ColumnOrdering ordering) =>
        SparseOps.Factor(pattern, kind, ordering, pivotTol: 1.0, dropFloor: 0.0)
            .Map(factored => new SolveSession(factored, PatternKeyOf(pattern), kind));

    public Fin<double[]> Solve(SparseCompressedRowMatrixStorage<double> csr, double[] rhs, double cap) =>
        PatternKeyOf(csr) != PatternKey
            ? Fin.Fail<double[]>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Consistent, new ContractEvidence.Digest(PatternKey))))
            : SparseOps.Apply(held.Value, new Edit.Revalue(csr.Values), pivotTol: 1.0)
                .Map(refactored => { held.Swap(_ => refactored); return refactored; })
                .Bind(factored => factored.Solve(rhs, cap));

    static UInt128 PatternKeyOf(SparseCompressedRowMatrixStorage<double> csr) =>
        ContentHash.Of(csr, static (storage, sink) => {
            sink.Ordinal(storage.RowCount).Ordinal(storage.ColumnCount);
            sink.Rows(toSeq(storage.RowPointers), static (pointer, writer) => writer.Ordinal(pointer));
            sink.Rows(toSeq(storage.ColumnIndices), static (column, writer) => writer.Ordinal(column));
        });

    public void Dispose() => held.Value.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class SolveLane {
    public static Fin<SolveResult> Discretized(
        BoundaryShell boundary, MeshPolicy meshPolicy, PhysicsKind physics, Seq<BoundaryCondition> conditions,
        MaterialField field, PhysicsPayload payload, ImmutableArray<FrameMember> members,
        Option<(ConstitutiveModel Model, ConstitutiveParameters Law)> material,
        LanePolicy policy, SolveRoute route, IClock clock,
        Option<SolveArchive> archive = default, Option<SolveSession> session = default) =>
        from mesh in MeshLane.Discretize(boundary, meshPolicy, clock)
        from problem in SolveProblem.Of(physics, mesh, conditions, field, payload, members, material)
        from result in Solve(problem, mesh, policy, route, clock, archive, session)
        select result;

    public static Fin<SolveSession> Session(
        SolveProblem problem, DiscreteMesh mesh, LanePolicy policy, FactorKind kind, ColumnOrdering ordering) =>
        from operatorCsr in OperatorAssembly.Assemble(problem, mesh, policy)
        from system in OperatorAssembly.Constrained(operatorCsr, problem.Conditions, policy)
        from session in SolveSession.Open(system.Operator, kind, ordering)
        select session;

    public static Fin<SolveResult> Solve(
        SolveProblem problem, DiscreteMesh mesh, LanePolicy policy, SolveRoute route, IClock clock,
        Option<SolveArchive> archive = default, Option<SolveSession> session = default) =>
        from _ in policy.Admits(problem, route)
        from operatorCsr in OperatorAssembly.Assemble(problem, mesh, policy)
        from system in OperatorAssembly.Constrained(operatorCsr, problem.Conditions, policy)
        from result in route.Switch(
            state: new RouteRequest(system, mesh, problem, policy, route, clock, archive, session),
            direct: static (request, _) => SolveRoutes.Direct(request),
            iterative: static (request, _) => SolveRoutes.Iterative(request),
            transient: static (request, row) => SolveRoutes.March(request, row),
            traced: static (request, row) => SolveRoutes.Traced(request, row),
            nonlinear: static (request, row) => SolveRoutes.NewtonLoad(request, row),
            continuation: static (request, row) => SolveRoutes.ArcLength(request, row),
            vibration: static (request, row) => SolveRoutes.Vibration(request, row),
            condensed: static (request, row) => SolveRoutes.Condensed(request, row),
            buckling: static (request, row) => SolveRoutes.Buckle(request, row))
        select result;

}
```
