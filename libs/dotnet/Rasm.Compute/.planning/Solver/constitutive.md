# [COMPUTE_SOLVER_CONSTITUTIVE]

Rasm.Compute per-Gauss-point material law: `ConstitutiveModel` `[Union]` is the stress-update axis (plasticity / hyperelasticity / viscoelasticity / damage) and `ContactConstraint` the regularized normal-contact enforcement, extracted from the solve contract as the per-integration-point owner distinct from `PhysicsKind`. `StressUpdate.Stress` returns the updated stress, exact hyper-dual local tangent, and evolved `ConstitutiveState`; `ContactEnforcement.Enforce` binds `ConstraintHandling.AugmentedLagrangian.Advance` to the normal gap function over the broad-phase pair set `Solver/clash#CLASH_AND_TWIN` supplies.

Stress is `∂W/∂ε` and the algorithmic tangent is `∂²W/∂ε²` through the admitted `HyperJet.DDScalar.Variables(values, order: 2)` energy evaluation and its `GetGradient`/`GetHessian` projections. Every constitutive surface is written ONCE against the `IScalar<T>` interface and instantiated twice — `Real` for the double-valued verdict the return map and the state evolution read, `HyperDual` for the hyper-dual energy whose derivatives are stress and tangent — so one parameterized energy owner carries regularized pressure-dependent plasticity, finite-strain Neo-Hookean/Mooney-Rivlin hyperelasticity, generalized-Maxwell viscoelastic history, and scalar damage with no second spelling of any surface. The interface carries its own transcendentals: `Real` binds `Math.Sqrt` and `HyperDual` binds `HyperJetMath.Sqrt`, both exact, so no surface hand-iterates a root whose convergence it cannot test. Activation proxies and Gauss-Newton curvature are deleted because neither is a constitutive energy Hessian. `Solver/contract#SOLVE_REQUEST` imports this page through `SolveProblem.Material`, and its modified-Newton residual folds `StressUpdate.Stress` at every Gauss point. Elastic `(E, ν)` and inelastic calibration read once from `graph.PropertiesOf(id).Mechanical`, keyed by `NodeId`.

The three page-level names carry their DISCIPLINE rather than the branch's shared vocabulary: kernel `Rasm/Solving/solver#LM_FUNCTOR` freezes `Dual<T>` as the first-order generic-math scalar, `Rasm.Materials/Appearance/graph` owns `MaterialParameters` as the optical row, and `Rasm.Fabrication/Tooling/cuttingdata` owns `MaterialState` as the heat-treatment vocabulary — `Rasm.AppUi` references Compute, Materials, and Fabrication together, so all three collisions are live in one closure and the Compute end moves.

## [01]-[INDEX]

- [02]-[CONSTITUTIVE]: per-Gauss-point plasticity/hyperelasticity/viscoelasticity/damage energy axis and regularized normal-contact potential with exact local hyper-dual derivatives.

## [02]-[CONSTITUTIVE]

- Owner: `IScalar<T>` is the one evaluation interface (`Real` · `HyperDual`) every constitutive surface is written against, carrying the regularized root both modes need; `VoigtLayout` publishes the normal/shear split and the engineering-shear factor every energy body reads; `Regularization` `[ValueObject<double>]` is the admitted smoothing radius the whole page threads; `ConstitutiveModel` `[Union]` carries one per-Gauss-point energy/state fold; `PlasticPotential` `[SmartEnum<string>]` closes the yield-surface roster as seed weights over one invariant generator, carrying `Linear` and `NeedsSoil` as row columns; `HyperelasticLaw` `[SmartEnum<string>]` closes the invariant-polynomial roster with its provenance; `ConstitutiveParameters` `[ComplexValueObject]` carries the elastic, hardening, Prony, damage, and pressure-dependent calibration beside the optional critical-state `SoilParameters` and its own derived moduli; `ContactConstraint` carries the constraint normal, base gap, regularization, and the optional segment weights; `StressUpdate` returns `∂W/∂ε`, `∂²W/∂ε²`, and evolved state; `ConstitutiveState` carries plastic strain, isotropic and volumetric hardening, optional preconsolidation and pore pressure, damage beside its committed driving energy, and Prony history.
- Cases: `ConstitutiveModel` `Plastic(PlasticPotential, Regularization)` · `Hyperelastic(HyperelasticLaw)` · `Viscoelastic(PronyTerms, TimeStep)` · `Damage(Exponent)`; `PlasticPotential` rows j2 · drucker-prager · smoothed-mohr-coulomb · modified-cam-clay; `HyperelasticLaw` rows neo-hookean · mooney-rivlin · yeoh · arruda-boyce. `SoilParameters` supplies the critical-state slope, compression/swell indices, preconsolidation pressure, and pore pressure the cap term alone reads.
- Entry: `public static Fin<ConstitutiveResult> Stress(ConstitutiveModel model, ReadOnlyMemory<double> strain, ConstitutiveState state, ConstitutiveParameters parameters, Instant at)` returns the updated stress, exact per-point tangent, and evolved state; `public static Fin<ContactResult> Enforce(ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers, double penalty, Seq<(int Slave, int Master)> broadPhasePairs, Instant at)` returns the normal contact force, gap-space stiffness, and updated multipliers over the supplied broad-phase pairs, each pair naming the base dof of a translational triple.
- Auto: `Stress` seeds the active strain/deformation-gradient vector with `DDScalar.Variables(..., order: 2)`, evaluates one `ConstitutiveModel.Energy`, and projects `GetGradient`/`GetHessian`; the plastic energy differentiates the SAME regularized surface the return map measures, hyperelastic rows require a nine-component deformation gradient and positive determinant, viscoelastic rows evolve one history vector per admitted Prony term over their carried `TimeStep`, and damage carries its evolution onto the tape on the loading branch. `Enforce` projects the gap onto the constraint normal, regularizes the normal potential, reads its composed sensitivity, and advances multipliers through `ConstraintHandling.AugmentedLagrangian.Advance`.
- Result: `ConstitutiveResult` carries the constitutive-model key, return-map iterations, return-map residual, integration-point count, and consistent-tangent condition. `ContactResult` carries active-set size, penetration residual, and multiplier-update count. The assembly fold forwards these measured values on their native results.
- Packages: HyperJet (`DDScalar` and its complete `in`-taking arithmetic operator set, `DDScalar.Variables`/`Constant`/`Value`/`GetGradient`/`GetHessian`, `HyperJetMath.Sqrt`), System.Numerics (`Vector3` — the constraint normal), System.Numerics.Tensors (`IsFiniteAll`/`Min`/`Max`/`SumOfSquares`), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`, `[ComplexValueObject]`, `[ValueObject<double>]`), LanguageExt.Core (`Fin`/`Option`/`Validation`/`Seq`), NodaTime (`Instant`), Rasm (project — `EpsilonPolicy`, `Band`), BCL inbox (`MemoryMarshal`)
- Growth: a new invariant plastic or hyperelastic law is one `[SmartEnum]` row carrying its weights, its linearity, its soil reach, and its provenance; a genuinely different state evolution is one `ConstitutiveModel` case; a new evolving variable is one `ConstitutiveState` field; a new evaluation mode is one `IScalar<T>` implementation, never a second spelling of a surface. No sibling material solver or hand-derived tangent surface appears.
- Boundary: `HyperJet` owns scalar forward hyper-dual differentiation, and this page owns only the constitutive energies and state transitions; `DDScalar.GetGradient` is stress and `DDScalar.GetHessian` is the exact local tangent, both read as the plain arrays the package exports and flattened once at the carrier edge. Every parameter is admitted at CONSTRUCTION, so the per-call gate carries only what varies per call — strain finiteness, model arity, and state arity — and the fourteen independent parameter invariants that once collapsed to one bit and one message now accumulate and report together.
- Boundary: ONE yield surface serves both modes. The return-map verdict and the differentiated energy read the same `Yield` body through `IScalar<T>`, so an edit cannot land on one arm — two hand-kept spellings drift the instant either moves and each half stays internally consistent while they disagree, which no test on either arm alone can see. The consistency modulus is likewise one derivation both the increment and the energy's quadratic term read, and the smooth positive part is one ramp contact and plasticity share.
- Boundary: the return map REPORTS, and an exhausted map REFUSES. A surface linear in the plastic multiplier closes in one step; a pressure-dependent or capped surface is nonlinear in that multiplier and iterates against the regularized residual to its declared cap. Running the cap out yields `Convergence.Exhausted`, which `Stress` fails as a typed refusal rather than advancing plastic strain by an increment no consistency test accepted — a success-shaped fall-through commits an unconverged state and every later step compounds it.
- Boundary: damage is a BRANCH, not a scale factor. Above the committed driving energy the damage variable rides the tape as a function of that energy, so `(1−D)·Ψ` differentiates into the true softening tangent; at or below it the committed value enters as a constant and the branch tangent is the secant elastic one. A frozen `D` on both branches reports a secant stiffness while the material softens, and Newton then converges to the wrong equilibrium.
- Boundary: inelastic strain is subtracted ONCE. The equilibrium spring, every Maxwell branch, the yield surface, and the damage driving energy all read the same elastic strain, so a viscoelastic branch reading total strain cannot count the plastic offset a second time.
- Boundary: an optional state column is `Option<double>`, never a zero sentinel — a genuinely zero pore pressure and an unset one are different facts, and a sentinel makes the fallback silently re-read the initial value on a state the step already advanced.
- Boundary: pressure-dependent plasticity admits without a soil record. Friction, dilation, and cohesion are material columns, so a rock or concrete row runs Drucker-Prager or smoothed Mohr-Coulomb directly; only the cap term reaches into `SoilParameters`, and the row's own `NeedsSoil` column states that reach so the admission reads one column instead of re-deriving a weight comparison.
- Boundary: the component split is a LAYOUT value, never a bare three. Which components are normal, which are shear, and what factor engineering shear carries are one owner's facts, so a plane-strain or axisymmetric problem is one layout row rather than four bodies that silently miscompute.
- Boundary: normal contact consumes only the broad-phase pairs from `Solver/clash#CLASH_AND_TWIN`, projects the gap onto the unit constraint normal over each pair's dof triple, advances multipliers through `ConstraintHandling.AugmentedLagrangian.Advance`, and never claims a Coulomb tangent without tangential kinematics. NAMED GAP: the whole tangential half of contact therefore has no owner anywhere, and the pair vocabularies do not meet — `ClashPair` carries triangle ids while this entry takes dof base indices, so whoever maps one to the other is unnamed. The returned stiffness is the gap-space Hessian, which `Solver/contract` projects onto the same normal to reach the dof-space contact block, so the enforcement owner never spells a global index.
- Boundary: the update is a PURE fold of `(model, strain, state, parameters)` and takes no clock — the `Instant` is caller metadata the result carries, exactly as `Solver/clash` stamps its result at the result and not at the geometry fold. A clock threaded into a pure numeric kernel is a dead entry parameter every Gauss point pays for.
- Exemption: the twenty component loops across the energy bodies, the return map, and the gap projection are MEASURED span kernels over fixed small arities — a per-Gauss-point stress update is the `ref struct` fold the expression law exempts by name, and every one of them dies with the call that fills it.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

public interface IScalar<T> where T : IScalar<T> {
    double Value { get; }
    T Like(double constant);

    static abstract T Sqrt(T value);

    static abstract T operator +(T left, T right);
    static abstract T operator -(T left, T right);
    static abstract T operator *(T left, T right);
    static abstract T operator /(T left, T right);
    static abstract T operator +(T left, double right);
    static abstract T operator -(T left, double right);
    static abstract T operator *(T left, double right);
    static abstract T operator /(T left, double right);
    static abstract T operator -(double left, T right);
    static abstract T operator *(double left, T right);
}

public readonly record struct Real(double Value) : IScalar<Real> {
    public static ReadOnlySpan<Real> Of(ReadOnlySpan<double> values) => MemoryMarshal.Cast<double, Real>(values);

    public Real Like(double constant) => new(constant);
    public static Real Sqrt(Real value) => new(Math.Sqrt(value.Value));

    public static Real operator +(Real left, Real right) => new(left.Value + right.Value);
    public static Real operator -(Real left, Real right) => new(left.Value - right.Value);
    public static Real operator *(Real left, Real right) => new(left.Value * right.Value);
    public static Real operator /(Real left, Real right) => new(left.Value / right.Value);
    public static Real operator +(Real left, double right) => new(left.Value + right);
    public static Real operator -(Real left, double right) => new(left.Value - right);
    public static Real operator *(Real left, double right) => new(left.Value * right);
    public static Real operator /(Real left, double right) => new(left.Value / right);
    public static Real operator -(double left, Real right) => new(left - right.Value);
    public static Real operator *(double left, Real right) => new(left * right.Value);
}

public readonly record struct HyperDual(DDScalar Inner, int Size) : IScalar<HyperDual> {
    public static HyperDual[] Variables(ReadOnlyMemory<double> values) {
        DDScalar[] active = DDScalar.Variables(values.ToArray(), order: 2);
        HyperDual[] scalars = new HyperDual[active.Length];
        for (int i = 0; i < scalars.Length; i++) { scalars[i] = new HyperDual(active[i], active.Length); }
        return scalars;
    }

    public double Value => Inner.Value;
    public HyperDual Like(double constant) => new(DDScalar.Constant(constant, Size, order: 2), Size);
    public static HyperDual Sqrt(HyperDual value) => new(HyperJetMath.Sqrt(value.Inner), value.Size);

    public static HyperDual operator +(HyperDual left, HyperDual right) => new(left.Inner + right.Inner, left.Size);
    public static HyperDual operator -(HyperDual left, HyperDual right) => new(left.Inner - right.Inner, left.Size);
    public static HyperDual operator *(HyperDual left, HyperDual right) => new(left.Inner * right.Inner, left.Size);
    public static HyperDual operator /(HyperDual left, HyperDual right) => new(left.Inner / right.Inner, left.Size);
    public static HyperDual operator +(HyperDual left, double right) => new(left.Inner + right, left.Size);
    public static HyperDual operator -(HyperDual left, double right) => new(left.Inner - right, left.Size);
    public static HyperDual operator *(HyperDual left, double right) => new(left.Inner * right, left.Size);
    public static HyperDual operator /(HyperDual left, double right) => new(left.Inner / right, left.Size);
    public static HyperDual operator -(double left, HyperDual right) => new(left - right.Inner, right.Size);
    public static HyperDual operator *(double left, HyperDual right) => new(left * right.Inner, right.Size);
}

public sealed record VoigtLayout(int NormalCount, int ShearCount) {
    public static readonly VoigtLayout Solid = new(NormalCount: 3, ShearCount: 3);
    public static readonly VoigtLayout PlaneStrain = new(NormalCount: 3, ShearCount: 1);
    public static readonly VoigtLayout Axisymmetric = new(NormalCount: 3, ShearCount: 1);

    public int Components => NormalCount + ShearCount;
    public bool IsShear(int component) => component >= NormalCount;

    public double ShearFactor => 0.5;
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct Regularization {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = Band.Positive.Guard(label: nameof(Regularization), value: ref value);
}

[ComplexValueObject]
public sealed partial class SoilParameters {
    public double CriticalStateSlope { get; }
    public double CompressionIndex { get; }
    public double SwellIndex { get; }
    public double InitialPreconsolidationPressure { get; }
    public double InitialPorePressure { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double criticalStateSlope, ref double compressionIndex,
        ref double swellIndex, ref double initialPreconsolidationPressure, ref double initialPorePressure) =>
        validationError = Band.Positive.Guard(label: nameof(CriticalStateSlope), value: ref criticalStateSlope)
            ?? Band.Positive.Guard(label: nameof(SwellIndex), value: ref swellIndex)
            ?? Band.Positive.Guard(label: nameof(InitialPreconsolidationPressure), value: ref initialPreconsolidationPressure)
            ?? (double.IsFinite(initialPorePressure) && compressionIndex > swellIndex
                ? null
                : new ValidationError("SoilParameters requires a finite pore pressure and a compression index above its swell index."));

    public double ConsolidationSpan => CompressionIndex - SwellIndex;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlasticPotential {
    public static readonly PlasticPotential J2 = new("j2", meridian: 0.0, lode: 0.0, cap: 0.0);
    public static readonly PlasticPotential DruckerPrager = new("drucker-prager", meridian: 1.0, lode: 0.0, cap: 0.0);
    public static readonly PlasticPotential SmoothedMohrCoulomb = new("smoothed-mohr-coulomb", meridian: 1.0, lode: 0.2, cap: 0.0);
    public static readonly PlasticPotential ModifiedCamClay = new("modified-cam-clay", meridian: 0.0, lode: 0.0, cap: 1.0);

    public double Meridian { get; }
    public double Lode { get; }
    public double Cap { get; }

    public bool Linear => Meridian == 0.0 && Lode == 0.0 && Cap == 0.0;
    public bool NeedsSoil => Cap > 0.0;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HyperelasticLaw {
    public static readonly HyperelasticLaw NeoHookean = new("neo-hookean", first: 0.5, second: 0.0, firstSquared: 0.0, bulk: 1.0, provenance: "exact");
    public static readonly HyperelasticLaw MooneyRivlin = new("mooney-rivlin", first: 0.25, second: 0.25, firstSquared: 0.0, bulk: 1.0, provenance: "exact");
    public static readonly HyperelasticLaw Yeoh = new("yeoh", first: 0.5, second: 0.0, firstSquared: 0.1, bulk: 1.0, provenance: "three-term-fit");
    public static readonly HyperelasticLaw ArrudaBoyce = new("arruda-boyce", first: 0.5, second: 0.05, firstSquared: 0.01, bulk: 1.0, provenance: "three-term-fit");

    public double First { get; }
    public double Second { get; }
    public double FirstSquared { get; }
    public double Bulk { get; }
    public string Provenance { get; }
}

// --- [MODELS] --------------------------------------------------------------------------

[ComplexValueObject]
public sealed partial class ConstitutiveParameters {
    public double YoungModulus { get; }
    public double PoissonRatio { get; }
    public double YieldStress { get; }
    public double HardeningModulus { get; }
    public Seq<(double Modulus, double RelaxationTime)> Prony { get; }
    public double DamageThreshold { get; }
    public double FrictionAngle { get; }
    public double DilationAngle { get; }
    public double Cohesion { get; }
    public Option<SoilParameters> Soil { get; }
    public VoigtLayout Layout { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double youngModulus, ref double poissonRatio, ref double yieldStress,
        ref double hardeningModulus, ref Seq<(double Modulus, double RelaxationTime)> prony, ref double damageThreshold,
        ref double frictionAngle, ref double dilationAngle, ref double cohesion, ref Option<SoilParameters> soil,
        ref VoigtLayout layout) {
        double young = youngModulus, poisson = poissonRatio, yield = yieldStress, hardening = hardeningModulus;
        double threshold = damageThreshold, friction = frictionAngle, dilation = dilationAngle, adhesion = cohesion;
        Seq<(double Modulus, double RelaxationTime)> terms = prony;
        validationError = Seq(
            Claim(double.IsFinite(young) && young > 0.0, nameof(YoungModulus)),
            Claim(double.IsFinite(poisson) && poisson is > -1.0 and < 0.5, nameof(PoissonRatio)),
            Claim(double.IsFinite(yield) && yield >= 0.0, nameof(YieldStress)),
            Claim(double.IsFinite(hardening) && hardening >= 0.0, nameof(HardeningModulus)),
            Claim(double.IsFinite(threshold) && threshold > 0.0, nameof(DamageThreshold)),
            Claim(double.IsFinite(friction) && friction is >= 0.0 and < 90.0, nameof(FrictionAngle)),
            Claim(double.IsFinite(dilation) && dilation is >= 0.0 and < 90.0 && dilation <= friction, nameof(DilationAngle)),
            Claim(double.IsFinite(adhesion) && adhesion >= 0.0, nameof(Cohesion)),
            Claim(terms.ForAll(static term => double.IsFinite(term.Modulus) && term.Modulus >= 0.0
                && double.IsFinite(term.RelaxationTime) && term.RelaxationTime > 0.0), nameof(Prony)))
            .Choose(static claim => claim) is { IsEmpty: false } refused
            ? new ValidationError($"ConstitutiveParameters refuses: {string.Join(", ", refused)}.")
            : null;

        static Option<string> Claim(bool held, string column) => held ? None : Some(column);
    }

    public double Shear => YoungModulus / (2.0 * (1.0 + PoissonRatio));
    public double Bulk => YoungModulus / (3.0 * (1.0 - 2.0 * PoissonRatio));
    public double Lame => YoungModulus * PoissonRatio / ((1.0 + PoissonRatio) * (1.0 - 2.0 * PoissonRatio));

    public double Consistency(PlasticPotential potential) =>
        3.0 * Shear + HardeningModulus + potential.Meridian * Bulk * Math.Tan(DilationAngle * Math.PI / 180.0);

    public double PorePressure(ConstitutiveState state) =>
        state.PorePressure.IfNone(Soil.Map(static soil => soil.InitialPorePressure).IfNone(0.0));
}

[Equatable]
public sealed partial record ConstitutiveState(
    [property: OrderedEquality] ReadOnlyMemory<double> PlasticStrain,
    double Hardening,
    double Damage,
    [property: OrderedEquality] Seq<ReadOnlyMemory<double>> ViscoHistory,
    double VolumetricPlasticStrain,
    Option<double> PreconsolidationPressure,
    Option<double> PorePressure,
    double DamageDriving) {
    public static ConstitutiveState Pristine(int components) =>
        new(new double[components], 0.0, 0.0, Seq<ReadOnlyMemory<double>>(), 0.0, None, None, 0.0);
}

public sealed record ConstitutiveResult(
    ReadOnlyMemory<double> Stress, ReadOnlyMemory<double> Tangent, ConstitutiveState State,
    string ModelKey, int ReturnMapIterations, double ReturnMapResidual, Instant At);

public sealed record ContactResult(
    ReadOnlyMemory<double> Force, ReadOnlyMemory<double> Stiffness, ReadOnlyMemory<double> Multipliers,
    int ActiveSet, double PenetrationResidual, Instant At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConstitutiveModel {
    private ConstitutiveModel() { }

    public sealed record Plastic(PlasticPotential Potential, Regularization Smoothing) : ConstitutiveModel;
    public sealed record Hyperelastic(HyperelasticLaw Law) : ConstitutiveModel;
    public sealed record Viscoelastic(int PronyTerms, double TimeStep) : ConstitutiveModel;
    public sealed record Damage(double Exponent) : ConstitutiveModel;

    public string ModelKey => Switch(
        plastic: static model => model.Potential.Key,
        hyperelastic: static model => model.Law.Key,
        viscoelastic: static _ => "viscoelastic",
        damage: static _ => "damage");

    public T Energy<T>(T[] strain, ConstitutiveState state, ConstitutiveParameters parameters) where T : IScalar<T> =>
        Switch(
            state: (Strain: strain, State: state, Parameters: parameters),
            plastic: static (input, model) => PlasticEnergy(input.Strain, input.State, input.Parameters, model.Potential, model.Smoothing),
            hyperelastic: static (input, model) => HyperelasticEnergy(input.Strain, input.Parameters, model.Law),
            viscoelastic: static (input, model) => ViscoelasticEnergy(input.Strain, input.State, input.Parameters, model.PronyTerms, model.TimeStep),
            damage: static (input, model) => DamageEnergy(input.Strain, input.State, input.Parameters, model.Exponent));

    public Fin<Unit> Admits(ReadOnlyMemory<double> strain, ConstitutiveParameters parameters) =>
        Switch(
            state: (Strain: strain, Parameters: parameters),
            plastic: static (input, model) => Require(
                    input.Strain.Length == input.Parameters.Layout.Components,
                    new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(input.Strain.Length, input.Parameters.Layout.Components)))
                .Bind(_ => Require(
                    !model.Potential.NeedsSoil || input.Parameters.Soil.IsSome,
                    new ComputeViolation.Required(ComputeSubject.Input))),
            hyperelastic: static (input, _) => Require(
                    input.Strain.Length == 9,
                    new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(input.Strain.Length, 9L)))
                .Bind(_ => {
                    double determinant = Determinant(Real.Of(input.Strain.Span)).Value;
                    return Require(determinant > 0.0, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(determinant)));
                }),
            viscoelastic: static (input, model) => Require(
                    input.Strain.Length == input.Parameters.Layout.Components,
                    new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(input.Strain.Length, input.Parameters.Layout.Components)))
                .Bind(_ => Require(
                    model.PronyTerms > 0 && model.PronyTerms <= input.Parameters.Prony.Count,
                    new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Interval(model.PronyTerms, 1, input.Parameters.Prony.Count))))
                .Bind(_ => Require(
                    double.IsFinite(model.TimeStep),
                    new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(model.TimeStep))))
                .Bind(_ => Require(
                    model.TimeStep > 0.0,
                    new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(model.TimeStep)))),
            damage: static (input, model) => Require(
                    input.Strain.Length == input.Parameters.Layout.Components,
                    new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(input.Strain.Length, input.Parameters.Layout.Components)))
                .Bind(_ => Require(
                    double.IsFinite(model.Exponent),
                    new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(model.Exponent))))
                .Bind(_ => Require(
                    model.Exponent > 0.0,
                    new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(model.Exponent)))));

    static Fin<Unit> Require(bool held, ComputeViolation evidence) =>
        held ? Fin.Succ(unit) : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, evidence));

    public static double YieldValue(ReadOnlyMemory<double> strain, ConstitutiveState state, ConstitutiveParameters parameters,
        PlasticPotential potential, Regularization smoothing) =>
        Yield(Elastic(Real.Of(strain.Span).ToArray(), state.PlasticStrain), state, parameters, potential, smoothing).Value;

    public static double StoredEnergy(ReadOnlyMemory<double> strain, ConstitutiveState state, ConstitutiveParameters parameters) =>
        ElasticEnergy(Elastic(Real.Of(strain.Span).ToArray(), state.PlasticStrain), parameters).Value;

    public static double DamageFraction(double driving, double threshold, double exponent) =>
        Damaged(new Real(driving), threshold, exponent).Value;

    static T[] Elastic<T>(T[] strain, ReadOnlyMemory<double> inelastic) where T : IScalar<T> {
        T[] elastic = new T[strain.Length];
        for (int component = 0; component < strain.Length; component++) {
            elastic[component] = strain[component] - (component < inelastic.Length ? inelastic.Span[component] : 0.0);
        }
        return elastic;
    }

    static T ElasticEnergy<T>(T[] elastic, ConstitutiveParameters parameters) where T : IScalar<T> {
        VoigtLayout layout = parameters.Layout;
        T trace = elastic[0].Like(0.0), normal = elastic[0].Like(0.0), shear = elastic[0].Like(0.0);
        for (int component = 0; component < elastic.Length; component++) {
            if (layout.IsShear(component)) { shear = shear + elastic[component] * elastic[component]; }
            else { trace = trace + elastic[component]; normal = normal + elastic[component] * elastic[component]; }
        }
        double mu = parameters.Shear;
        return 0.5 * parameters.Lame * (trace * trace) + mu * normal + layout.ShearFactor * mu * shear;
    }

    static T Yield<T>(T[] elastic, ConstitutiveState state, ConstitutiveParameters parameters,
        PlasticPotential potential, Regularization smoothing) where T : IScalar<T> {
        VoigtLayout layout = parameters.Layout;
        T volumetric = elastic[0] + elastic[1] + elastic[2];
        T mean = volumetric / 3.0;
        T equivalentSquared = elastic[0].Like(0.0), thirdInvariant = elastic[0].Like(1.0);
        for (int component = 0; component < elastic.Length; component++) {
            T deviatoric = layout.IsShear(component) ? layout.ShearFactor * elastic[component] : elastic[component] - mean;
            equivalentSquared = equivalentSquared + deviatoric * deviatoric;
            if (!layout.IsShear(component)) { thirdInvariant = thirdInvariant * deviatoric; }
        }
        double friction = parameters.FrictionAngle * Math.PI / 180.0;
        T q = 2.0 * parameters.Shear * Root(1.5 * equivalentSquared, smoothing);
        T lode = thirdInvariant / (q * q * q + smoothing.Value);
        T pressure = parameters.Bulk * volumetric - parameters.PorePressure(state);
        T frictional = q * (lode * potential.Lode + 1.0)
            + (pressure * Math.Sin(friction) - parameters.Cohesion * Math.Cos(friction)) * potential.Meridian
            - parameters.YieldStress - parameters.HardeningModulus * state.Hardening;
        return parameters.Soil.Match(
            Some: soil => frictional * (1.0 - potential.Cap) + Cap(q, pressure, state, soil, smoothing) * potential.Cap,
            None: () => frictional);
    }

    static T Cap<T>(T q, T pressure, ConstitutiveState state, SoilParameters soil, Regularization smoothing) where T : IScalar<T> {
        double preconsolidation = state.PreconsolidationPressure.IfNone(soil.InitialPreconsolidationPressure);
        T ellipse = q * q + pressure * (pressure - preconsolidation) * (soil.CriticalStateSlope * soil.CriticalStateSlope);
        return Root(Positive(ellipse, smoothing), smoothing);
    }

    static T PlasticEnergy<T>(T[] strain, ConstitutiveState state, ConstitutiveParameters parameters,
        PlasticPotential potential, Regularization smoothing) where T : IScalar<T> {
        T[] elastic = Elastic(strain, state.PlasticStrain);
        T overstress = Positive(Yield(elastic, state, parameters, potential, smoothing), smoothing);
        double modulus = parameters.Consistency(potential);
        T increment = overstress / modulus;
        return ElasticEnergy(elastic, parameters) - overstress * increment + 0.5 * modulus * (increment * increment);
    }

    static T HyperelasticEnergy<T>(T[] deformation, ConstitutiveParameters parameters, HyperelasticLaw law) where T : IScalar<T> {
        T j = Determinant(deformation);
        T[] rightCauchyGreen = new T[9];
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++) {
                rightCauchyGreen[row * 3 + column] = deformation[0].Like(0.0);
                for (int k = 0; k < 3; k++) { rightCauchyGreen[row * 3 + column] = rightCauchyGreen[row * 3 + column] + deformation[k * 3 + row] * deformation[k * 3 + column]; }
            }
        T i1 = rightCauchyGreen[0] + rightCauchyGreen[4] + rightCauchyGreen[8];
        T traceC2 = deformation[0].Like(0.0);
        for (int row = 0; row < 3; row++) for (int column = 0; column < 3; column++) { traceC2 = traceC2 + rightCauchyGreen[row * 3 + column] * rightCauchyGreen[column * 3 + row]; }
        T i2 = 0.5 * (i1 * i1 - traceC2);
        double mu = parameters.Shear;
        T first = i1 - 3.0, second = i2 - 3.0, volume = j - 1.0;
        return (first * law.First + second * law.Second + first * first * law.FirstSquared) * mu
            - volume * (2.0 * mu * (law.First + 2.0 * law.Second))
            + volume * volume * (0.5 * parameters.Lame * law.Bulk);
    }

    internal static T Determinant<T>(ReadOnlySpan<T> f) where T : IScalar<T> =>
        f[0] * (f[4] * f[8] - f[5] * f[7]) - f[1] * (f[3] * f[8] - f[5] * f[6]) + f[2] * (f[3] * f[7] - f[4] * f[6]);

    static T Determinant<T>(T[] f) where T : IScalar<T> => Determinant<T>(f.AsSpan());

    static T ViscoelasticEnergy<T>(T[] strain, ConstitutiveState state, ConstitutiveParameters parameters, int terms, double timeStep) where T : IScalar<T> {
        T[] elastic = Elastic(strain, state.PlasticStrain);
        T energy = ElasticEnergy(elastic, parameters);
        for (int term = 0; term < Math.Min(terms, parameters.Prony.Count); term++) {
            double decay = Math.Exp(-timeStep / parameters.Prony[term].RelaxationTime);
            ReadOnlyMemory<double> history = term < state.ViscoHistory.Count ? state.ViscoHistory[term] : ReadOnlyMemory<double>.Empty;
            for (int component = 0; component < elastic.Length; component++) {
                T branch = elastic[component] - decay * (component < history.Length ? history.Span[component] : 0.0);
                energy = energy + branch * branch * (0.5 * parameters.Prony[term].Modulus);
            }
        }
        return energy;
    }

    static T DamageEnergy<T>(T[] strain, ConstitutiveState state, ConstitutiveParameters parameters, double exponent) where T : IScalar<T> {
        T stored = ElasticEnergy(Elastic(strain, state.PlasticStrain), parameters);
        return stored.Value > Math.Max(parameters.DamageThreshold, state.DamageDriving)
            ? (1.0 - Damaged(stored, parameters.DamageThreshold, exponent)) * stored
            : (1.0 - state.Damage) * stored;
    }

    static T Damaged<T>(T driving, double threshold, double exponent) where T : IScalar<T> {
        T excess = (driving - threshold) * exponent;
        return excess / (excess + threshold);
    }

    public static T Positive<T>(T value, Regularization smoothing) where T : IScalar<T> =>
        0.5 * (value + Root(value * value, smoothing));

    public static T Root<T>(T value, Regularization smoothing) where T : IScalar<T> =>
        T.Sqrt(value + smoothing.Value * smoothing.Value);
}

public sealed record ContactConstraint {
    private ContactConstraint(Vector3 normal, double baseGap, Regularization smoothing, Option<ReadOnlyMemory<double>> weights) =>
        (Normal, BaseGap, Smoothing, Weights) = (normal, baseGap, smoothing, weights);

    public Vector3 Normal { get; }
    public double BaseGap { get; }
    public Regularization Smoothing { get; }
    public Option<ReadOnlyMemory<double>> Weights { get; }

    public static Fin<ContactConstraint> Of(Vector3 normal, double baseGap, Regularization smoothing, Option<ReadOnlyMemory<double>> weights) =>
        Seq(
            Claim(Math.Abs(normal.LengthSquared() - 1.0) <= EpsilonPolicy.SqrtEpsilon,
                new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Scalar(normal.LengthSquared()))),
            Claim(double.IsFinite(baseGap), new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(baseGap))),
            weights.Match(
                Some: static held => Seq(
                        Claim(!held.IsEmpty, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(held.Length, 1L))),
                        Claim(TensorPrimitives.IsFiniteAll<double>(held.Span), new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(held.Length))),
                        Claim(TensorPrimitives.Min(held.Span) > 0.0, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(TensorPrimitives.Min(held.Span)))))
                    .Traverse(static claim => claim).As().Map(static _ => unit),
                None: static () => Success<Error, Unit>(unit)))
            .Traverse(static claim => claim).As()
            .Map(_ => new ContactConstraint(normal, baseGap, smoothing, weights)).ToFin();

    public double Weight(int pair) => Weights.Match(Some: held => held.Span[pair], None: static () => 1.0);

    public HyperDual Potential(HyperDual[] penetration, double penalty) {
        HyperDual energy = penetration[0].Like(0.0);
        foreach (HyperDual gap in penetration) {
            HyperDual positive = ConstitutiveModel.Positive(gap, Smoothing);
            energy = energy + positive * positive * (0.5 * penalty);
        }
        return energy;
    }

    static Validation<Error, Unit> Claim(bool held, ComputeViolation evidence) =>
        held ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new ComputeFault.Violation(ComputeArea.Solver, evidence));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class StressUpdate {
    public static Fin<ConstitutiveResult> Stress(
        ConstitutiveModel model, ReadOnlyMemory<double> strain, ConstitutiveState state, ConstitutiveParameters parameters, Instant at) =>
        from _ in Validate(model, strain, state, parameters)
        from verdict in ReturnMapVerdict(model, strain, state, parameters)
        from result in Op.Of(name: "constitutive.energy-domain").Catch(() => {
            HyperDual energy = model.Energy(HyperDual.Variables(strain), state, parameters);
            return Fin.Succ(new ConstitutiveResult(
                energy.Inner.GetGradient().AsMemory(), RowMajor(energy.Inner.GetHessian()),
                Evolve(model, state, strain, parameters, verdict.DGamma),
                model.ModelKey, verdict.Iterations, verdict.Residual, at));
        })
        select result;

    internal static ReadOnlyMemory<double> RowMajor(double[,] hessian) {
        int size = hessian.GetLength(0);
        double[] flat = new double[size * size];
        for (int row = 0; row < size; row++) for (int column = 0; column < size; column++) { flat[row * size + column] = hessian[row, column]; }
        return flat.AsMemory();
    }

    static Fin<Unit> Validate(ConstitutiveModel model, ReadOnlyMemory<double> strain, ConstitutiveState state, ConstitutiveParameters parameters) =>
        strain.IsEmpty || !TensorPrimitives.IsFiniteAll<double>(strain.Span)
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Input, new ScalarEvidence.Sequence(strain.Length))))
            : state.PlasticStrain.Length != 0 && state.PlasticStrain.Length != strain.Length
                ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(state.PlasticStrain.Length, strain.Length))))
                : model.Admits(strain, parameters);

    static Fin<(int Iterations, double DGamma, double Residual)> ReturnMapVerdict(
        ConstitutiveModel model, ReadOnlyMemory<double> strain, ConstitutiveState state, ConstitutiveParameters parameters) =>
        model is ConstitutiveModel.Plastic plastic
            ? ReturnMap(strain, state, parameters, plastic.Potential, plastic.Smoothing) is var (iterations, increment, verdict)
                ? verdict switch {
                    Convergence.Converged converged => Fin.Succ((iterations, increment, converged.Residual)),
                    Convergence.Exhausted exhausted => Fin.Fail<(int, double, double)>(
                        new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.WithinLimit, new CapacityEvidence.Count(exhausted.Budget, exhausted.Budget)))),
                    _ => Fin.Fail<(int, double, double)>(
                        new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Converged, new ContractEvidence.None()))),
                }
                : Fin.Fail<(int, double, double)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Consistent, new ContractEvidence.None())))
            : Fin.Succ((0, 0.0, 0.0));

    const int ReturnMapCap = 25;

    static (int Iterations, double DGamma, Convergence Verdict) ReturnMap(
        ReadOnlyMemory<double> strain, ConstitutiveState state, ConstitutiveParameters parameters,
        PlasticPotential potential, Regularization smoothing) {
        double yield = ConstitutiveModel.YieldValue(strain, state, parameters, potential, smoothing);
        double overstress = ConstitutiveModel.Positive(new Real(yield), smoothing).Value;
        if (overstress <= smoothing.Value) { return (0, 0.0, new Convergence.Converged(overstress)); }
        double modulus = parameters.Consistency(potential);
        double increment = overstress / modulus;
        if (potential.Linear) { return (1, increment, new Convergence.Converged(0.0)); }
        double residual = overstress;
        for (int iteration = 1; iteration <= ReturnMapCap; iteration++) {
            residual = ConstitutiveModel.YieldValue(
                strain, PlasticEvolution(state, strain, parameters, potential, increment), parameters, potential, smoothing);
            if (Math.Abs(residual) <= smoothing.Value) { return (iteration, increment, new Convergence.Converged(Math.Abs(residual))); }
            increment += residual / modulus;
        }
        return (ReturnMapCap, increment, new Convergence.Exhausted(ReturnMapCap));
    }

    static ConstitutiveState Evolve(
        ConstitutiveModel model, ConstitutiveState state, ReadOnlyMemory<double> strain, ConstitutiveParameters parameters, double dGamma) =>
        model.Switch(
            state: (State: state, Strain: strain, Parameters: parameters, DGamma: dGamma),
            plastic: static (input, model) => PlasticEvolution(input.State, input.Strain, input.Parameters, model.Potential, input.DGamma),
            hyperelastic: static (input, _) => input.State,
            viscoelastic: static (input, model) => input.State with {
                ViscoHistory = toSeq(Enumerable.Range(0, Math.Max(0, model.PronyTerms)))
                    .Map(term => RelaxedHistory(input.State, input.Strain, input.Parameters, term, model.TimeStep)),
            },
            damage: static (input, model) => DamageEvolution(input.State, input.Strain, input.Parameters, model.Exponent));

    static ConstitutiveState PlasticEvolution(
        ConstitutiveState state, ReadOnlyMemory<double> strain, ConstitutiveParameters parameters, PlasticPotential potential, double dGamma) {
        double dilation = potential.Meridian * Math.Tan(parameters.DilationAngle * Math.PI / 180.0);
        double volumetric = dGamma * dilation;
        ConstitutiveState advanced = state with {
            PlasticStrain = Accumulated(state.PlasticStrain, strain, parameters.Layout, dGamma, dilation),
            Hardening = state.Hardening + dGamma,
            VolumetricPlasticStrain = state.VolumetricPlasticStrain + volumetric,
        };
        return parameters.Soil.Match(
            Some: soil => advanced with {
                PreconsolidationPressure = Some(state.PreconsolidationPressure.IfNone(soil.InitialPreconsolidationPressure)
                    * Math.Exp(Math.Clamp(volumetric / soil.ConsolidationSpan, -ExponentDomain, ExponentDomain))),
                PorePressure = Some(state.PorePressure.IfNone(soil.InitialPorePressure)),
            },
            None: () => advanced);
    }

    const double ExponentDomain = 20.0;

    static ConstitutiveState DamageEvolution(
        ConstitutiveState state, ReadOnlyMemory<double> strain, ConstitutiveParameters parameters, double exponent) {
        double driving = ConstitutiveModel.StoredEnergy(strain, state, parameters);
        return driving <= Math.Max(parameters.DamageThreshold, state.DamageDriving)
            ? state
            : state with {
                Damage = Math.Min(1.0, ConstitutiveModel.DamageFraction(driving, parameters.DamageThreshold, exponent)),
                DamageDriving = driving,
            };
    }

    static ReadOnlyMemory<double> RelaxedHistory(
        ConstitutiveState state, ReadOnlyMemory<double> strain, ConstitutiveParameters parameters, int term, double timeStep) {
        ReadOnlyMemory<double> prior = term < state.ViscoHistory.Count ? state.ViscoHistory[term] : ReadOnlyMemory<double>.Empty;
        double[] next = new double[strain.Length];
        double decay = Math.Exp(-timeStep / parameters.Prony[term].RelaxationTime);
        for (int component = 0; component < next.Length; component++) {
            next[component] = decay * (component < prior.Length ? prior.Span[component] : 0.0) + (1.0 - decay) * strain.Span[component];
        }
        return next;
    }

    static ReadOnlyMemory<double> Accumulated(
        ReadOnlyMemory<double> plastic, ReadOnlyMemory<double> strain, VoigtLayout layout, double dGamma, double dilation) {
        double[] elastic = new double[Math.Max(plastic.Length, strain.Length)];
        for (int i = 0; i < elastic.Length; i++) { elastic[i] = (i < strain.Length ? strain.Span[i] : 0.0) - (i < plastic.Length ? plastic.Span[i] : 0.0); }
        double norm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(elastic));
        double[] next = new double[elastic.Length];
        for (int i = 0; i < next.Length; i++) {
            double volumetric = layout.IsShear(i) ? 0.0 : dGamma * dilation / layout.NormalCount;
            next[i] = (i < plastic.Length ? plastic.Span[i] : 0.0) + (norm > 0.0 ? dGamma * elastic[i] / norm : 0.0) + volumetric;
        }
        return next;
    }
}

public static class ContactEnforcement {
    public static Fin<ContactResult> Enforce(
        ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers,
        double penalty, Seq<(int Slave, int Master)> broadPhasePairs, Instant at) =>
        from _ in Admit(contact, displacement, multipliers, penalty, broadPhasePairs)
        let gap = Gap(contact, displacement, broadPhasePairs)
        from result in Op.Of(name: "contact.potential-domain").Catch(() => {
            HyperDual potential = contact.Potential(HyperDual.Variables(gap.AsMemory()), penalty);
            return Fin.Succ(new ContactResult(
                potential.Inner.GetGradient().AsMemory(), StressUpdate.RowMajor(potential.Inner.GetHessian()),
                ConstraintHandling.AugmentedLagrangian.Advance(multipliers.Span.ToArray(), gap, penalty).AsMemory(),
                Active(gap), TensorPrimitives.Max<double>(gap), at));
        })
        select result;

    static Fin<Unit> Admit(
        ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers,
        double penalty, Seq<(int Slave, int Master)> pairs) =>
        Seq(
            Claim(!displacement.IsEmpty, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(displacement.Length, 1L))),
            Claim(TensorPrimitives.IsFiniteAll<double>(displacement.Span), new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(displacement.Length))),
            Claim(!pairs.IsEmpty, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(pairs.Count, 1L))),
            Claim(pairs.ForAll(pair => pair.Slave >= 0 && pair.Master >= 0
                    && pair.Slave + 2 < displacement.Length && pair.Master + 2 < displacement.Length),
                new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Sequence(pairs.Count))),
            Claim(contact.Weights.Map(static held => held.Length).IfNone(pairs.Count) == pairs.Count,
                new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(contact.Weights.Map(static held => held.Length).IfNone(pairs.Count), pairs.Count))),
            Claim(double.IsFinite(penalty), new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(penalty))),
            Claim(penalty > 0.0, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(penalty))),
            Claim(multipliers.Length == pairs.Count, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(multipliers.Length, pairs.Count))))
            .Traverse(static claim => claim).As().Map(static _ => unit).ToFin();

    static Validation<Error, Unit> Claim(bool held, ComputeViolation evidence) =>
        held ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new ComputeFault.Violation(ComputeArea.Solver, evidence));

    static double[] Gap(ContactConstraint contact, ReadOnlyMemory<double> displacement, Seq<(int Slave, int Master)> pairs) {
        double[] gap = new double[pairs.Count];
        ReadOnlySpan<double> field = displacement.Span;
        double nx = contact.Normal.X, ny = contact.Normal.Y, nz = contact.Normal.Z;
        for (int i = 0; i < pairs.Count; i++) {
            (int slave, int master) = pairs[i];
            double projected = (field[slave] - field[master]) * nx
                + (field[slave + 1] - field[master + 1]) * ny
                + (field[slave + 2] - field[master + 2]) * nz;
            gap[i] = contact.Weight(i) * projected - contact.BaseGap;
        }
        return gap;
    }

    static int Active(ReadOnlySpan<double> gap) {
        int count = 0;
        foreach (double value in gap) { if (value > 0.0) { count++; } }
        return count;
    }
}
```
