# [COMPUTE_SOLVER_CONSTITUTIVE]

Rasm.Compute per-Gauss-point material law: `ConstitutiveModel` `[Union]` is the stress-update axis (plasticity / hyperelasticity / viscoelasticity / damage) and `ContactConstraint` the regularized normal-contact enforcement, extracted from the solve contract as the per-integration-point owner distinct from `PhysicsKind`. `StressUpdate.Stress` returns the updated stress, exact hyper-dual local tangent, and evolved `MaterialState`; `ContactEnforcement.Enforce` binds `ConstraintHandling.AugmentedLagrangian.Advance` to the normal gap function over the broad-phase pair set `Solver/clash#CLASH_AND_TWIN` supplies.

Stress is `∂W/∂ε` and the algorithmic tangent is `∂²W/∂ε²` through the admitted `HyperJet.DDScalar.Variables(values, order: 2)` energy evaluation and its `GetGradient`/`GetHessian` projections. Every constitutive surface is written ONCE against the `IScalar<T>` seam and instantiated twice — `Real` for the double-valued verdict the return map and the state evolution read, `Dual` for the hyper-dual energy whose derivatives are stress and tangent — so one parameterized energy owner carries regularized pressure-dependent plasticity, finite-strain Neo-Hookean/Mooney-Rivlin hyperelasticity, generalized-Maxwell viscoelastic history, and scalar damage with no second spelling of any surface. Activation proxies and Gauss-Newton curvature are deleted because neither is a constitutive energy Hessian. `Solver/contract#SOLVE_CONTRACT` imports this page through `SolveProblem.Material`, and its modified-Newton residual folds `StressUpdate.Stress` at every Gauss point. Elastic `(E, ν)` and inelastic calibration read once from `graph.PropertiesOf(id).Mechanical`, keyed by `NodeId`.

## [01]-[INDEX]

- [02]-[CONSTITUTIVE]: per-Gauss-point plasticity/hyperelasticity/viscoelasticity/damage energy axis and regularized normal-contact potential with exact local hyper-dual derivatives.

## [02]-[CONSTITUTIVE]

- Owner: `IScalar<T>` is the one evaluation seam (`Real` · `Dual`) every constitutive surface is written against; `ConstitutiveModel` `[Union]` carries one per-Gauss-point energy/state fold; `PlasticPotential` parameterizes `J2`, `DruckerPrager`, `SmoothedMohrCoulomb`, and `ModifiedCamClay` as seed weights over one invariant generator; `HyperelasticLaw` parameterizes invariant-polynomial energies; `MaterialParameters` carries the elastic, hardening, Prony, damage, and pressure-dependent (friction, dilation, cohesion) calibration beside the optional critical-state `SoilParameters`; `ContactConstraint` `[Union]` carries the constraint normal, base gap, and regularization as base properties and reuses optimizer multiplier advancement; `StressUpdate` returns `∂W/∂ε`, `∂²W/∂ε²`, and evolved state; `MaterialState` carries plastic strain, isotropic and volumetric hardening, optional preconsolidation and pore pressure, damage beside its committed driving energy, and Prony history.
- Cases: `ConstitutiveModel` `Plastic(PlasticPotential, Regularization)` · `Hyperelastic(HyperelasticLaw)` · `Viscoelastic(PronyTerms, TimeStep)` · `Damage(Exponent)`; `ContactConstraint` `NodeToSurface(Normal, BaseGap, Regularization)` · `Mortar(Normal, BaseGap, Regularization, Weights)` — the mortar case carries per-pair segment-integration weights scaling each gap, the pointwise case the unit weight, so the two disciplines differ structurally rather than by name. `SoilParameters` supplies the critical-state slope, compression/swell indices, preconsolidation pressure, and pore pressure the cap term alone reads.
- Entry: `public static Fin<ConstitutiveResult> Stress(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, IClock clock)` returns the updated stress, exact per-point tangent, and evolved state; `Fin<T>` rejects non-finite or dimensionally invalid state, parameter, strain, deformation-gradient, and energy-domain inputs. `public static Fin<ContactResult> Enforce(ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers, double penalty, Seq<(int Slave, int Master)> broadPhasePairs, IClock clock)` returns the normal contact force, gap-space stiffness, and updated multipliers over the supplied broad-phase pairs, each pair naming the base dof of a translational triple.
- Auto: `Stress` seeds the active strain/deformation-gradient vector with `DDScalar.Variables(..., order: 2)`, evaluates one `ConstitutiveModel.Energy`, and projects `GetGradient`/`GetHessian`; the plastic energy differentiates the SAME regularized surface the return map measures, hyperelastic rows require a nine-component deformation gradient and positive determinant, viscoelastic rows evolve one history vector per admitted Prony term over their carried `TimeStep`, and damage carries its evolution onto the tape on the loading branch. `Enforce` projects the gap onto the constraint normal, regularizes the normal potential, reads its composed sensitivity, and advances multipliers through `ConstraintHandling.AugmentedLagrangian.Advance`.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics key extended with the constitutive-model key, the integration-point count, the measured return-map iteration count (plastic), the consistent-tangent condition, and the converged flag the consistency test yields; the contact path stamps the active-set size, penetration residual, and multiplier-update count, so a nonlinear-material or contact run is auditable on the same `Solve` receipt — never a parallel constitutive receipt.
- Packages: HyperJet, System.Numerics (`Vector3` — the constraint normal), System.Numerics.Tensors, MathNet.Numerics, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new invariant plastic or hyperelastic law is parameter data on `PlasticPotential` or `HyperelasticLaw`; a genuinely different state evolution is one `ConstitutiveModel` case; a new contact discipline is one `ContactConstraint` case; a new evolving variable is one `MaterialState` field; a new evaluation mode is one `IScalar<T>` implementation, never a second spelling of a surface. No sibling material solver or hand-derived tangent surface appears.
- Boundary: `HyperJet` owns scalar forward hyper-dual differentiation, and this page owns only the constitutive energies and state transitions; `DDScalar.GetGradient` is stress and `DDScalar.GetHessian` is the exact local tangent, both read as the plain arrays the package exports and flattened once at the carrier edge. Every parameter and strain vector is finite-gated before AD, finite-strain energy rejects a non-nine-component deformation gradient at admission and lifts a non-positive determinant failure onto `Fin`, and Prony history is cardinality-bounded by the model row.
- Boundary: ONE yield surface serves both modes. The return-map verdict and the differentiated energy read the same `Yield` body through `IScalar<T>`, so an edit cannot land on one arm — two hand-kept spellings drift the instant either moves and each half stays internally consistent while they disagree, which no test on either arm alone can see. The consistency modulus is likewise one derivation both the increment and the energy's quadratic term read.
- Boundary: the return map reports what it did. A surface linear in the plastic multiplier — J2 with linear hardening, every weight zero — closes in one step and reports one; a pressure-dependent or capped surface is nonlinear in that multiplier and iterates against the regularized residual, reporting its own count and the convergence the test yields. A literal iteration count and an unconditional converged flag report a closed map for an iteration that never ran.
- Boundary: damage is a BRANCH, not a scale factor. Above the committed driving energy the damage variable rides the tape as a function of that energy, so `(1−D)·Ψ` differentiates into the true softening tangent; at or below it the committed value enters as a constant and the branch tangent is the secant elastic one. A frozen `D` on both branches reports a secant stiffness while the material softens, and Newton then converges to the wrong equilibrium.
- Boundary: inelastic strain is subtracted ONCE. The equilibrium spring, every Maxwell branch, the yield surface, and the damage driving energy all read the same elastic strain, so a viscoelastic branch reading total strain cannot count the plastic offset a second time.
- Boundary: an optional state column is `Option<double>`, never a zero sentinel — a genuinely zero pore pressure and an unset one are different facts, and a sentinel makes the fallback silently re-read the initial value on a state the step already advanced.
- Boundary: pressure-dependent plasticity admits without a soil record. Friction, dilation, and cohesion are material columns, so a rock or concrete row runs Drucker-Prager or smoothed Mohr-Coulomb directly; only the cap term reaches into `SoilParameters` for the critical-state slope and the preconsolidation pressure, and the admission gate is exactly that reach.
- Boundary: normal contact consumes only the broad-phase pairs from `Solver/clash#CLASH_AND_TWIN`, projects the gap onto the unit constraint normal over each pair's dof triple, advances multipliers through `ConstraintHandling.AugmentedLagrangian.Advance`, and never claims a Coulomb tangent without tangential kinematics. The returned stiffness is the gap-space Hessian — `Solver/contract#SOLVE_CONTRACT` projects it onto the same normal to reach the dof-space contact block, so the enforcement owner never spells a global index.
- Boundary: `Solver/contract#SOLVE_CONTRACT` consumes stress in `f_ext − f_int(u)` and keeps any global Newton-CG or colored-Jacobian assembly outside this local owner.

```csharp signature
// ONE constitutive surface, two evaluation modes. A yield surface or stored energy is written once against this
// seam: `Real` evaluates it in doubles for the return-map verdict and the state evolution, `Dual` evaluates the same
// body in hyper-duals so its gradient is stress and its Hessian is the exact algorithmic tangent. Constants enter
// through `Like` because a hyper-dual constant must be seeded at the active vector's own arity.
public interface IScalar<T> where T : IScalar<T> {
    double Value { get; }
    T Like(double constant);

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
    public static Real[] Of(ReadOnlyMemory<double> values) {
        Real[] scalars = new Real[values.Length];
        for (int i = 0; i < scalars.Length; i++) { scalars[i] = new Real(values.Span[i]); }
        return scalars;
    }

    public Real Like(double constant) => new(constant);

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

// The active arity travels ON the value: `DDScalar.Constant` needs it for every constant the surface forms, and
// re-reading it off an exported gradient allocates one array per constant.
public readonly record struct Dual(DDScalar Inner, int Size) : IScalar<Dual> {
    public static Dual[] Variables(ReadOnlyMemory<double> values) {
        DDScalar[] active = DDScalar.Variables(values.ToArray(), order: 2);
        Dual[] scalars = new Dual[active.Length];
        for (int i = 0; i < scalars.Length; i++) { scalars[i] = new Dual(active[i], active.Length); }
        return scalars;
    }

    public double Value => Inner.Value;
    public Dual Like(double constant) => new(DDScalar.Constant(constant, Size, order: 2), Size);

    public static Dual operator +(Dual left, Dual right) => new(left.Inner + right.Inner, left.Size);
    public static Dual operator -(Dual left, Dual right) => new(left.Inner - right.Inner, left.Size);
    public static Dual operator *(Dual left, Dual right) => new(left.Inner * right.Inner, left.Size);
    public static Dual operator /(Dual left, Dual right) => new(left.Inner / right.Inner, left.Size);
    public static Dual operator +(Dual left, double right) => new(left.Inner + right, left.Size);
    public static Dual operator -(Dual left, double right) => new(left.Inner - right, left.Size);
    public static Dual operator *(Dual left, double right) => new(left.Inner * right, left.Size);
    public static Dual operator /(Dual left, double right) => new(left.Inner / right, left.Size);
    public static Dual operator -(double left, Dual right) => new(left - right.Inner, right.Size);
    public static Dual operator *(double left, Dual right) => new(left * right.Inner, right.Size);
}

// Critical-state data ALONE. Friction, dilation, and cohesion are material columns, because a rock or concrete row is
// pressure-dependent without carrying a consolidation model, and gating that row on a soil record forced every such
// material to fabricate columns nothing measured for it.
public sealed record SoilParameters(
    double CriticalStateSlope,
    double CompressionIndex,
    double SwellIndex,
    double InitialPreconsolidationPressure,
    double InitialPorePressure) {
    public bool Invalid => !double.IsFinite(CriticalStateSlope) || CriticalStateSlope <= 0.0
        || !double.IsFinite(CompressionIndex) || !double.IsFinite(SwellIndex) || CompressionIndex <= SwellIndex || SwellIndex <= 0.0
        || !double.IsFinite(InitialPreconsolidationPressure) || InitialPreconsolidationPressure <= 0.0 || !double.IsFinite(InitialPorePressure);
}

public sealed record PlasticPotential(double MeridianWeight, double LodeWeight, double CapWeight) {
    public static readonly PlasticPotential J2 = new(0.0, 0.0, 0.0);
    public static readonly PlasticPotential DruckerPrager = new(1.0, 0.0, 0.0);
    public static readonly PlasticPotential SmoothedMohrCoulomb = new(1.0, 0.2, 0.0);
    public static readonly PlasticPotential ModifiedCamClay = new(0.0, 0.0, 1.0);

    // Weights are seed data written exactly, so the linearity test is exact: every term that makes the surface move
    // with the plastic multiplier carries a weight, and a row with none of them closes its return map in one step.
    public bool Linear => MeridianWeight == 0.0 && LodeWeight == 0.0 && CapWeight == 0.0;

    public bool Invalid => !double.IsFinite(MeridianWeight) || MeridianWeight is < 0.0 or > 1.0
        || !double.IsFinite(LodeWeight) || Math.Abs(LodeWeight) >= 1.0 || !double.IsFinite(CapWeight) || CapWeight is < 0.0 or > 1.0;
}

public sealed record MaterialParameters(
    double YoungModulus,
    double PoissonRatio,
    double YieldStress,
    double HardeningModulus,
    Seq<(double Modulus, double RelaxationTime)> Prony,
    double DamageThreshold,
    double FrictionAngle,
    double DilationAngle,
    double Cohesion,
    Option<SoilParameters> Soil);

public sealed record MaterialState(
    ReadOnlyMemory<double> PlasticStrain,
    double Hardening,
    double Damage,
    Seq<ReadOnlyMemory<double>> ViscoHistory,
    double VolumetricPlasticStrain,
    // Both consolidation columns are OPTIONAL: a genuinely zero pore pressure and an unset one are different facts,
    // and a zero sentinel makes the fallback re-read the initial value on a state the step already advanced.
    Option<double> PreconsolidationPressure,
    Option<double> PorePressure,
    // Committed maximum driving energy — the threshold the loading branch tests against, so unloading is a real
    // branch rather than a monotone re-derivation from the current strain.
    double DamageDriving) {
    public static MaterialState Pristine(int components) =>
        new(new double[components], 0.0, 0.0, Seq<ReadOnlyMemory<double>>(), 0.0, None, None, 0.0);
}

public sealed record ConstitutiveResult(ReadOnlyMemory<double> Stress, ReadOnlyMemory<double> Tangent, MaterialState State, int ReturnMapIterations, bool Converged, Instant At);

public sealed record ContactResult(ReadOnlyMemory<double> Force, ReadOnlyMemory<double> Stiffness, ReadOnlyMemory<double> Multipliers, int ActiveSet, double PenetrationResidual, Instant At);

public sealed record HyperelasticLaw(double FirstInvariant, double SecondInvariant, double FirstInvariantSquared, double BulkScale) {
    public static readonly HyperelasticLaw NeoHookean = new(0.5, 0.0, 0.0, 1.0);
    public static readonly HyperelasticLaw MooneyRivlin = new(0.25, 0.25, 0.0, 1.0);
    public static readonly HyperelasticLaw Yeoh = new(0.5, 0.0, 0.1, 1.0);
    public static readonly HyperelasticLaw ArrudaBoyce = new(0.5, 0.05, 0.01, 1.0);

    public bool Invalid => !double.IsFinite(FirstInvariant) || !double.IsFinite(SecondInvariant) || !double.IsFinite(FirstInvariantSquared)
        || !double.IsFinite(BulkScale) || BulkScale <= 0.0;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConstitutiveModel {
    private ConstitutiveModel() { }

    public sealed record Plastic(PlasticPotential Potential, double Regularization) : ConstitutiveModel;
    public sealed record Hyperelastic(HyperelasticLaw Law) : ConstitutiveModel;
    public sealed record Viscoelastic(int PronyTerms, double TimeStep) : ConstitutiveModel;
    public sealed record Damage(double Exponent) : ConstitutiveModel;

    public T Energy<T>(T[] strain, MaterialState state, MaterialParameters parameters) where T : IScalar<T> =>
        Switch(
            state: (Strain: strain, State: state, Parameters: parameters),
            plastic: static (input, model) => PlasticEnergy(input.Strain, input.State, input.Parameters, model.Potential, model.Regularization),
            hyperelastic: static (input, model) => HyperelasticEnergy(input.Strain, input.Parameters, model.Law),
            viscoelastic: static (input, model) => ViscoelasticEnergy(input.Strain, input.State, input.Parameters, model.PronyTerms, model.TimeStep),
            damage: static (input, model) => DamageEnergy(input.Strain, input.State, input.Parameters, model.Exponent));

    // The double-mode reads the return map and the state evolution need, over the SAME generic bodies the tape
    // differentiates — a second double-valued spelling of any of them is the drift this seam exists to foreclose.
    public static double YieldValue(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, PlasticPotential potential, double regularization) =>
        Yield(Elastic(Real.Of(strain), state.PlasticStrain), state, parameters, potential, regularization).Value;

    public static double StoredEnergy(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters) =>
        ElasticEnergy(Elastic(Real.Of(strain), state.PlasticStrain), parameters).Value;

    public static double DamageFraction(double driving, double threshold, double exponent) =>
        Damaged(new Real(driving), threshold, exponent).Value;

    // Consistency modulus `3μ + H + m·K·tan ψ`: the plastic multiplier's denominator, read by the return map's
    // increment and by the energy's quadratic term, so one edit moves both. `μ > 0` holds for every admitted
    // `(E, ν)`, so the modulus needs no floor.
    public static double Consistency(MaterialParameters parameters, PlasticPotential potential) =>
        3.0 * Shear(parameters) + parameters.HardeningModulus
        + potential.MeridianWeight * Bulk(parameters) * Math.Tan(parameters.DilationAngle * Math.PI / 180.0);

    // Elastic strain is the SAME quantity every arm reads: total minus the committed inelastic part. An arm reading
    // total strain adds the shed plastic offset back into the stored energy and reports a stress the material lost.
    static T[] Elastic<T>(T[] strain, ReadOnlyMemory<double> inelastic) where T : IScalar<T> {
        T[] elastic = new T[strain.Length];
        for (int component = 0; component < strain.Length; component++) {
            elastic[component] = strain[component] - (component < inelastic.Length ? inelastic.Span[component] : 0.0);
        }
        return elastic;
    }

    static T ElasticEnergy<T>(T[] elastic, MaterialParameters parameters) where T : IScalar<T> {
        T trace = elastic[0].Like(0.0), normal = elastic[0].Like(0.0), shear = elastic[0].Like(0.0);
        for (int component = 0; component < elastic.Length; component++) {
            if (component < 3) { trace = trace + elastic[component]; normal = normal + elastic[component] * elastic[component]; }
            else { shear = shear + elastic[component] * elastic[component]; }
        }
        double lambda = parameters.YoungModulus * parameters.PoissonRatio / ((1.0 + parameters.PoissonRatio) * (1.0 - 2.0 * parameters.PoissonRatio));
        double mu = Shear(parameters);
        return 0.5 * lambda * (trace * trace) + mu * normal + 0.5 * mu * shear;
    }

    // ONE yield surface. `q` is the regularized von-Mises equivalent, `lode` the third-invariant shape term, and the
    // meridian and cap terms weight in by `PlasticPotential` alone — J2 zeroes both, Drucker-Prager and smoothed
    // Mohr-Coulomb weight the meridian, modified Cam-Clay the cap. Friction and cohesion are material columns, so
    // only the cap term reaches into `SoilParameters`; a soil-free row therefore carries zero cap weight by
    // admission and the absent arm returns the frictional surface exactly rather than a scaled share of it.
    static T Yield<T>(T[] elastic, MaterialState state, MaterialParameters parameters, PlasticPotential potential, double regularization) where T : IScalar<T> {
        T volumetric = elastic[0] + elastic[1] + elastic[2];
        T mean = volumetric / 3.0;
        T equivalentSquared = elastic[0].Like(0.0), thirdInvariant = elastic[0].Like(1.0);
        for (int component = 0; component < elastic.Length; component++) {
            T deviatoric = component < 3 ? elastic[component] - mean : 0.5 * elastic[component];
            equivalentSquared = equivalentSquared + deviatoric * deviatoric;
            if (component < 3) { thirdInvariant = thirdInvariant * deviatoric; }
        }
        double friction = parameters.FrictionAngle * Math.PI / 180.0;
        T q = 2.0 * Shear(parameters) * Sqrt(1.5 * equivalentSquared, regularization);
        T lode = thirdInvariant / (q * q * q + regularization);
        T pressure = Bulk(parameters) * volumetric - PorePressure(state, parameters);
        T frictional = q * (lode * potential.LodeWeight + 1.0)
            + (pressure * Math.Sin(friction) - parameters.Cohesion * Math.Cos(friction)) * potential.MeridianWeight
            - parameters.YieldStress - parameters.HardeningModulus * state.Hardening;
        return parameters.Soil.Match(
            Some: soil => frictional * (1.0 - potential.CapWeight) + Cap(q, pressure, state, soil, regularization) * potential.CapWeight,
            None: () => frictional);
    }

    static T Cap<T>(T q, T pressure, MaterialState state, SoilParameters soil, double regularization) where T : IScalar<T> {
        double preconsolidation = state.PreconsolidationPressure.IfNone(soil.InitialPreconsolidationPressure);
        T ellipse = q * q + pressure * (pressure - preconsolidation) * (soil.CriticalStateSlope * soil.CriticalStateSlope);
        return Sqrt(Positive(ellipse, regularization), regularization);
    }

    // The stored energy differentiates the SAME surface the return map measures: the regularized overstress and its
    // consistency increment both form from `Yield`, so stress and tangent are the exact derivatives of the verdict.
    static T PlasticEnergy<T>(T[] strain, MaterialState state, MaterialParameters parameters, PlasticPotential potential, double regularization) where T : IScalar<T> {
        T[] elastic = Elastic(strain, state.PlasticStrain);
        T overstress = Positive(Yield(elastic, state, parameters, potential, regularization), regularization);
        double modulus = Consistency(parameters, potential);
        T increment = overstress / modulus;
        return ElasticEnergy(elastic, parameters) - overstress * increment + 0.5 * modulus * (increment * increment);
    }

    // Arity is gated at admission, so the body carries no arity guard of its own.
    static T HyperelasticEnergy<T>(T[] deformation, MaterialParameters parameters, HyperelasticLaw law) where T : IScalar<T> {
        T j = deformation[0] * (deformation[4] * deformation[8] - deformation[5] * deformation[7])
            - deformation[1] * (deformation[3] * deformation[8] - deformation[5] * deformation[6])
            + deformation[2] * (deformation[3] * deformation[7] - deformation[4] * deformation[6]);
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
        double mu = Shear(parameters);
        double lambda = parameters.YoungModulus * parameters.PoissonRatio / ((1.0 + parameters.PoissonRatio) * (1.0 - 2.0 * parameters.PoissonRatio));
        T first = i1 - 3.0, second = i2 - 3.0, volume = j - 1.0;
        return (first * law.FirstInvariant + second * law.SecondInvariant + first * first * law.FirstInvariantSquared) * mu
            - volume * (2.0 * mu * (law.FirstInvariant + 2.0 * law.SecondInvariant))
            + volume * volume * (0.5 * lambda * law.BulkScale);
    }

    // Generalized Maxwell over the SAME elastic strain the equilibrium spring reads: the equilibrium branch is the
    // stored energy over `ε_e` and each arm adds `½·E_k·(ε_e − decay·ε_e,prior)²` over that same `ε_e`. An arm
    // reading TOTAL strain counts the plastic offset once in the spring and again in every branch.
    static T ViscoelasticEnergy<T>(T[] strain, MaterialState state, MaterialParameters parameters, int terms, double timeStep) where T : IScalar<T> {
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

    // Damage evolves ON the tape wherever the driving energy exceeds the committed threshold, so `D` is a function of
    // `Ψ` and `(1−D)·Ψ` differentiates into the true SOFTENING tangent; at or below it the committed value enters as
    // a constant and the branch is the secant elastic one — that asymmetry IS unloading.
    static T DamageEnergy<T>(T[] strain, MaterialState state, MaterialParameters parameters, double exponent) where T : IScalar<T> {
        T stored = ElasticEnergy(Elastic(strain, state.PlasticStrain), parameters);
        return stored.Value > Math.Max(parameters.DamageThreshold, state.DamageDriving)
            ? (1.0 - Damaged(stored, parameters.DamageThreshold, exponent)) * stored
            : (1.0 - state.Damage) * stored;
    }

    // Saturating damage in the driving energy: `D = e·(Ψ−κ)/(e·(Ψ−κ) + κ)` is monotone, reaches one only in the
    // limit, and needs no transcendental the hyper-dual scalar does not carry.
    static T Damaged<T>(T driving, double threshold, double exponent) where T : IScalar<T> {
        T excess = (driving - threshold) * exponent;
        return excess / (excess + threshold);
    }

    // The smooth positive part every surface on the page shares, contact included — one regularized ramp, so the
    // contact potential and the plastic overstress round the same corner the same way.
    public static T Positive<T>(T value, double regularization) where T : IScalar<T> => 0.5 * (value + Sqrt(value * value, regularization));

    // ONE regularized root for every surface on the page, contact included: `√(x + r²)` by eight Newton halvings from
    // a value-seeded start. The shift keeps the derivative finite at zero, which is exactly why the return map and
    // the tangent agree at the elastic-plastic corner.
    static T Sqrt<T>(T value, double regularization) where T : IScalar<T> {
        T shifted = value + regularization * regularization;
        T root = shifted.Like(Math.Sqrt(Math.Max(regularization * regularization, shifted.Value)));
        for (int iteration = 0; iteration < 8; iteration++) { root = 0.5 * (root + shifted / root); }
        return root;
    }

    // Pore pressure is state data, not a differentiated quantity: it enters the surface as the committed scalar the
    // step carries, falling back to the soil row's initial value and to zero for a material with no soil record.
    static double PorePressure(MaterialState state, MaterialParameters parameters) =>
        state.PorePressure.IfNone(parameters.Soil.Map(static soil => soil.InitialPorePressure).IfNone(0.0));

    static double Shear(MaterialParameters parameters) => parameters.YoungModulus / (2.0 * (1.0 + parameters.PoissonRatio));
    static double Bulk(MaterialParameters parameters) => parameters.YoungModulus / (3.0 * (1.0 - 2.0 * parameters.PoissonRatio));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContactConstraint {
    private ContactConstraint() { }

    // Normal, base gap, and regularization are BASE properties both cases realize positionally: every case carries
    // them identically, so a fold over identical arms is a dispatch that decides nothing. Only the weight genuinely
    // differs, and it stays a switch.
    public abstract Vector3 Normal { get; }
    public abstract double BaseGap { get; }
    public abstract double Regularization { get; }

    public sealed record NodeToSurface(Vector3 Normal, double BaseGap, double Regularization) : ContactConstraint;
    public sealed record Mortar(Vector3 Normal, double BaseGap, double Regularization, ReadOnlyMemory<double> Weights) : ContactConstraint;

    // Mortar pairs weight the segment-integrated gap; node-to-surface pairs are pointwise and carry the unit weight
    public double Weight(int pair) =>
        Switch(
            state: pair,
            nodeToSurface: static (_, _) => 1.0,
            mortar: static (index, contact) => contact.Weights.Span[index]);

    // The potential sums INDEPENDENT per-pair terms, so its Hessian is diagonal — which is what lets the solve
    // contract project one entry per pair onto the normal instead of a dense gap-space block.
    public Dual Potential(Dual[] penetration, double penalty) {
        Dual energy = penetration[0].Like(0.0);
        foreach (Dual gap in penetration) {
            Dual positive = ConstitutiveModel.Positive(gap, Regularization);
            energy = energy + positive * positive * (0.5 * penalty);
        }
        return energy;
    }
}

public static class StressUpdate {
    public static Fin<ConstitutiveResult> Stress(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, IClock clock) =>
        from valid in Validate(model, strain, state, parameters)
        from verdict in ReturnMapVerdict(model, strain, state, parameters)
        from result in Try.lift(() => {
            Dual energy = model.Energy(Dual.Variables(strain), state, parameters);
            return new ConstitutiveResult(
                energy.Inner.GetGradient().AsMemory(), RowMajor(energy.Inner.GetHessian()),
                Evolve(model, state, strain, parameters, verdict.DGamma), verdict.Iterations, verdict.Converged, clock.GetCurrentInstant());
        }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<constitutive-energy-domain:{error.Message}>"))
        select result;

    // Hyper-dual exports are plain arrays, so the row-major flatten IS the carrier shape both results publish and no
    // dense-matrix lift stands between the tape and the receipt.
    internal static ReadOnlyMemory<double> RowMajor(double[,] hessian) {
        int size = hessian.GetLength(0);
        double[] flat = new double[size * size];
        for (int row = 0; row < size; row++) for (int column = 0; column < size; column++) { flat[row * size + column] = hessian[row, column]; }
        return flat.AsMemory();
    }

    static Fin<Unit> Validate(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters) =>
        strain.IsEmpty || !TensorPrimitives.IsFiniteAll<double>(strain.Span)
            ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<constitutive-strain>"))
            : !double.IsFinite(parameters.YoungModulus) || parameters.YoungModulus <= 0.0 || !double.IsFinite(parameters.PoissonRatio) || parameters.PoissonRatio is <= -1.0 or >= 0.5 || !double.IsFinite(parameters.YieldStress) || parameters.YieldStress < 0.0 || !double.IsFinite(parameters.HardeningModulus) || parameters.HardeningModulus < 0.0 || !double.IsFinite(parameters.DamageThreshold) || parameters.DamageThreshold <= 0.0 || !double.IsFinite(parameters.FrictionAngle) || parameters.FrictionAngle is < 0.0 or >= 90.0 || !double.IsFinite(parameters.DilationAngle) || parameters.DilationAngle is < 0.0 or >= 90.0 || parameters.DilationAngle > parameters.FrictionAngle || !double.IsFinite(parameters.Cohesion) || parameters.Cohesion < 0.0 || parameters.Prony.Exists(static term => !double.IsFinite(term.Modulus) || term.Modulus < 0.0 || !double.IsFinite(term.RelaxationTime) || term.RelaxationTime <= 0.0) || parameters.Soil.Exists(static soil => soil.Invalid)
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<constitutive-parameters>"))
                : model.Switch(
                    state: (Strain: strain, Parameters: parameters),
                    // The cap term is the ONE reach into consolidation data, so it alone demands a soil record;
                    // every other pressure-dependent row runs off the material's own friction and cohesion.
                    plastic: static (input, plastic) => input.Strain.Length == 6 && !plastic.Potential.Invalid && double.IsFinite(plastic.Regularization) && plastic.Regularization > 0.0 && (plastic.Potential.CapWeight <= 0.0 || input.Parameters.Soil.IsSome),
                    hyperelastic: static (input, model) => input.Strain.Length == 9 && !model.Law.Invalid && Determinant(input.Strain.Span) > 0.0,
                    viscoelastic: static (input, visco) => input.Strain.Length == 6 && visco.PronyTerms is > 0 && visco.PronyTerms <= input.Parameters.Prony.Count && double.IsFinite(visco.TimeStep) && visco.TimeStep > 0.0,
                    damage: static (input, damage) => input.Strain.Length == 6 && double.IsFinite(damage.Exponent) && damage.Exponent > 0.0) is false
                    ? Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<constitutive-model-admission:{strain.Length}>"))
                    : state.PlasticStrain.Length != 0 && state.PlasticStrain.Length != strain.Length
                        ? Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<constitutive-state-arity:{state.PlasticStrain.Length}!={strain.Length}>"))
                        : Fin.Succ(unit);

    static double Determinant(ReadOnlySpan<double> deformation) =>
        deformation[0] * (deformation[4] * deformation[8] - deformation[5] * deformation[7])
        - deformation[1] * (deformation[3] * deformation[8] - deformation[5] * deformation[6])
        + deformation[2] * (deformation[3] * deformation[7] - deformation[4] * deformation[6]);

    static Fin<(int Iterations, double DGamma, bool Converged)> ReturnMapVerdict(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters) =>
        model is ConstitutiveModel.Plastic plastic
            ? Fin.Succ(ReturnMap(strain, state, parameters, plastic.Potential, plastic.Regularization))
            : Fin.Succ((0, 0.0, true));

    // A surface LINEAR in the plastic multiplier — J2 with linear hardening, every potential weight zero — closes in
    // exactly one step and reports one. A pressure-dependent or capped surface moves with the volumetric part the
    // flow rule advances, so it is nonlinear in that multiplier: the map iterates against the regularized residual
    // over the state each increment implies, reports the count it took, and reports the convergence the test yields
    // rather than a literal pair the caller cannot distinguish from a closed map.
    const int ReturnMapCap = 25;

    static (int Iterations, double DGamma, bool Converged) ReturnMap(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, PlasticPotential potential, double regularization) {
        double yield = ConstitutiveModel.YieldValue(strain, state, parameters, potential, regularization);
        double overstress = 0.5 * (yield + Math.Sqrt(yield * yield + regularization * regularization));
        if (overstress <= regularization) { return (0, 0.0, true); }
        double modulus = ConstitutiveModel.Consistency(parameters, potential);
        double increment = overstress / modulus;
        if (potential.Linear) { return (1, increment, true); }
        for (int iteration = 1; iteration <= ReturnMapCap; iteration++) {
            double residual = ConstitutiveModel.YieldValue(strain, PlasticEvolution(state, strain, parameters, potential, increment), parameters, potential, regularization);
            if (Math.Abs(residual) <= regularization) { return (iteration, increment, true); }
            increment += residual / modulus;
        }
        return (ReturnMapCap, increment, false);
    }

    static MaterialState Evolve(ConstitutiveModel model, MaterialState state, ReadOnlyMemory<double> strain, MaterialParameters parameters, double dGamma) =>
        model.Switch(
            state: (State: state, Strain: strain, Parameters: parameters, DGamma: dGamma),
            plastic: static (input, model) => PlasticEvolution(input.State, input.Strain, input.Parameters, model.Potential, input.DGamma),
            hyperelastic: static (input, _) => input.State,
            viscoelastic: static (input, model) => input.State with {
                ViscoHistory = toSeq(Enumerable.Range(0, Math.Max(0, model.PronyTerms))).Map(term => RelaxedHistory(input.State, input.Strain, input.Parameters, term, model.TimeStep)),
            },
            damage: static (input, model) => DamageEvolution(input.State, input.Strain, input.Parameters, model.Exponent));

    static MaterialState PlasticEvolution(MaterialState state, ReadOnlyMemory<double> strain, MaterialParameters parameters, PlasticPotential potential, double dGamma) {
        double dilation = potential.MeridianWeight * Math.Tan(parameters.DilationAngle * Math.PI / 180.0);
        double volumetric = dGamma * dilation;
        MaterialState advanced = state with {
            PlasticStrain = Accumulated(state.PlasticStrain, strain, dGamma, dilation),
            Hardening = state.Hardening + dGamma,
            VolumetricPlasticStrain = state.VolumetricPlasticStrain + volumetric,
        };
        // Consolidation advances ONLY where the material carries a critical-state model; a rock or concrete row runs
        // the same frictional flow with no preconsolidation history to move.
        return parameters.Soil.Match(
            Some: soil => advanced with {
                PreconsolidationPressure = Some(state.PreconsolidationPressure.IfNone(soil.InitialPreconsolidationPressure)
                    * Math.Exp(Math.Clamp(volumetric / (soil.CompressionIndex - soil.SwellIndex), -20.0, 20.0))),
                PorePressure = Some(state.PorePressure.IfNone(soil.InitialPorePressure)),
            },
            None: () => advanced);
    }

    // The committed pair advances by the SAME law the tape differentiates, so the next step's branch test reads the
    // energy this step actually reached; an unloading step commits neither column.
    static MaterialState DamageEvolution(MaterialState state, ReadOnlyMemory<double> strain, MaterialParameters parameters, double exponent) {
        double driving = ConstitutiveModel.StoredEnergy(strain, state, parameters);
        return driving <= Math.Max(parameters.DamageThreshold, state.DamageDriving)
            ? state
            : state with {
                Damage = Math.Min(1.0, ConstitutiveModel.DamageFraction(driving, parameters.DamageThreshold, exponent)),
                DamageDriving = driving,
            };
    }

    static ReadOnlyMemory<double> RelaxedHistory(MaterialState state, ReadOnlyMemory<double> strain, MaterialParameters parameters, int term, double timeStep) {
        ReadOnlyMemory<double> prior = term < state.ViscoHistory.Count ? state.ViscoHistory[term] : ReadOnlyMemory<double>.Empty;
        double[] next = new double[strain.Length];
        double decay = Math.Exp(-timeStep / parameters.Prony[term].RelaxationTime);
        for (int component = 0; component < next.Length; component++) {
            next[component] = decay * (component < prior.Length ? prior.Span[component] : 0.0) + (1.0 - decay) * strain.Span[component];
        }
        return next;
    }

    // Flow direction is the normalized elastic strain, so a state with no elastic strain has no direction and
    // accumulates the volumetric part alone.
    static ReadOnlyMemory<double> Accumulated(ReadOnlyMemory<double> plastic, ReadOnlyMemory<double> strain, double dGamma, double dilation) {
        double[] elastic = new double[Math.Max(plastic.Length, strain.Length)];
        for (int i = 0; i < elastic.Length; i++) { elastic[i] = (i < strain.Length ? strain.Span[i] : 0.0) - (i < plastic.Length ? plastic.Span[i] : 0.0); }
        double norm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(elastic));
        double[] next = new double[elastic.Length];
        for (int i = 0; i < next.Length; i++) {
            double volumetric = i < 3 ? dGamma * dilation / 3.0 : 0.0;
            next[i] = (i < plastic.Length ? plastic.Span[i] : 0.0) + (norm > 0.0 ? dGamma * elastic[i] / norm : 0.0) + volumetric;
        }
        return next;
    }
}

public static class ContactEnforcement {
    public static Fin<ContactResult> Enforce(ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers, double penalty, Seq<(int Slave, int Master)> broadPhasePairs, IClock clock) {
        // Each pair names the BASE dof of a translational triple, because the gap projects onto the constraint
        // normal — an index check on the base row alone reads the two rows past the end of the field.
        if (displacement.IsEmpty || !TensorPrimitives.IsFiniteAll<double>(displacement.Span) || broadPhasePairs.IsEmpty
            || broadPhasePairs.Exists(pair => pair.Slave < 0 || pair.Master < 0 || pair.Slave + 2 >= displacement.Length || pair.Master + 2 >= displacement.Length)) {
            return Fin.Fail<ContactResult>(new ComputeFault.ModelRejected("<contact-kinematics>"));
        }
        // A non-unit normal rescales the gap and every force derived from it, so the direction is admitted rather
        // than normalized in place — a caller whose normal drifted is reporting a geometry it has not resolved.
        if (Math.Abs(contact.Normal.LengthSquared() - 1.0) > 1e-5) {
            return Fin.Fail<ContactResult>(new ComputeFault.ModelRejected($"<contact-normal-not-unit:{contact.Normal.LengthSquared()}>"));
        }
        if (contact is ContactConstraint.Mortar mortar && (mortar.Weights.Length != broadPhasePairs.Count || !TensorPrimitives.IsFiniteAll<double>(mortar.Weights.Span) || mortar.Weights.Span.ToArray().Any(static weight => weight <= 0.0))) {
            return Fin.Fail<ContactResult>(new ComputeFault.ModelRejected($"<contact-mortar-weights:{mortar.Weights.Length}!={broadPhasePairs.Count}>"));
        }
        double[] gap = Gap(contact, displacement, broadPhasePairs);
        if (!double.IsFinite(penalty) || penalty <= 0.0 || !double.IsFinite(contact.BaseGap) || !double.IsFinite(contact.Regularization) || contact.Regularization <= 0.0 || multipliers.Length != gap.Length) {
            return Fin.Fail<ContactResult>(new ComputeFault.ModelRejected("<contact-admission>"));
        }
        double[] updated = ConstraintHandling.AugmentedLagrangian.Advance(multipliers.Span.ToArray(), gap, penalty);
        return Try.lift(() => {
            Dual potential = contact.Potential(Dual.Variables(gap.AsMemory()), penalty);
            return new ContactResult(
                potential.Inner.GetGradient().AsMemory(), StressUpdate.RowMajor(potential.Inner.GetHessian()),
                updated.AsMemory(), gap.Count(static value => value > 0.0), Penetration(gap), clock.GetCurrentInstant());
        }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<contact-potential-domain:{error.Message}>"));
    }

    // Gap is the relative displacement PROJECTED onto the constraint normal, less the base gap: `g = (u_s − u_m)·n − g₀`
    // over each pair's dof triple. A scalar difference of two dof rows measures whichever axis those rows happen to
    // be, so an inclined or non-axis-aligned interface reports a penetration it never had.
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

    static double Penetration(double[] gap) {
        double worst = 0.0;
        foreach (double g in gap) { worst = Math.Max(worst, g); }
        return worst;
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
