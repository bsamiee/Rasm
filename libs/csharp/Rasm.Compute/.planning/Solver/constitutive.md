# [COMPUTE_SOLVER_CONSTITUTIVE]

Rasm.Compute per-Gauss-point material law: `ConstitutiveModel` `[Union]` is the stress-update axis (plasticity / hyperelasticity / viscoelasticity / damage) and `ContactConstraint` the regularized normal-contact enforcement, extracted from the solve contract as the per-integration-point owner distinct from `PhysicsKind`. `StressUpdate.Stress` returns the updated stress, exact hyper-dual local tangent, and evolved `MaterialState`; `ContactEnforcement.Enforce` binds `ConstraintHandling.AugmentedLagrangian.Advance` to the gap function over the broad-phase pair set `Solver/clash#CLASH_AND_TWIN` supplies.

Stress is `∂W/∂ε` and the algorithmic tangent is `∂²W/∂ε²` through the admitted `HyperJet.DDScalar.Variables(values, order: 2)` energy evaluation and its `GetGradient`/`GetHessian` projections. One parameterized energy owner carries regularized J2 plasticity, finite-strain Neo-Hookean/Mooney-Rivlin hyperelasticity, generalized-Maxwell viscoelastic history, and scalar damage; activation proxies and Gauss-Newton curvature are deleted because neither is a constitutive energy Hessian. `Solver/contract#SOLVE_CONTRACT` imports this page through `SolveProblem.Material`, and its modified-Newton residual folds `StressUpdate.Stress` at every Gauss point. Elastic `(E, ν)` and inelastic calibration read once from `graph.PropertiesOf(id).Mechanical`, keyed by `NodeId`.

## [01]-[INDEX]

- [01]-[CONSTITUTIVE]: per-Gauss-point plasticity/hyperelasticity/viscoelasticity/damage energy axis and regularized normal-contact potential with exact local hyper-dual derivatives.

## [02]-[CONSTITUTIVE]

- Owner: `ConstitutiveModel` `[Union]` carries one per-Gauss-point energy/state fold; `PlasticPotential` parameterizes `J2`, `DruckerPrager`, `SmoothedMohrCoulomb`, and `ModifiedCamClay` as seed data over one invariant generator; `HyperelasticLaw` parameterizes invariant-polynomial energies; `ContactConstraint` `[Union]` reuses optimizer multiplier advancement; `StressUpdate` returns `∂W/∂ε`, `∂²W/∂ε²`, and evolved state; `MaterialState` carries plastic strain, isotropic and volumetric hardening, preconsolidation pressure, pore pressure, damage, and Prony history.
- Cases: `ConstitutiveModel` `Plastic(PlasticPotential, Regularization)` · `Hyperelastic(HyperelasticLaw)` · `Viscoelastic(PronyTerms, TimeStep)` · `Damage(Exponent)`; `ContactConstraint` `NodeToSurface(Gap, Regularization)` · `Mortar(Gap, Regularization, Weights)` — the mortar case carries per-pair segment-integration weights scaling each gap, the pointwise case the unit weight, so the two disciplines differ structurally rather than by name. `SoilParameters` supplies friction/dilation, cohesion, critical-state slope, compression/swell indices, preconsolidation pressure, and pore pressure to the pressure-dependent potential.
- Entry: `public static Fin<ConstitutiveResult> Stress(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, IClock clock)` returns the updated stress, exact per-point tangent, and evolved state; `Fin<T>` rejects non-finite or dimensionally invalid state, parameter, strain, deformation-gradient, and energy-domain inputs. `public static Fin<ContactResult> Enforce(ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers, double penalty, Seq<(int Slave, int Master)> broadPhasePairs, IClock clock)` returns the normal contact force, stiffness, and updated multipliers over the supplied broad-phase pairs.
- Auto: `Stress` seeds the active strain/deformation-gradient vector with `DDScalar.Variables(..., order: 2)`, evaluates one `ConstitutiveModel.Energy`, and projects `GetGradient`/`GetHessian`; J2 uses the same smooth positive-part return multiplier in the differentiated incremental potential and state evolution, hyperelastic rows require a nine-component deformation gradient and positive determinant, viscoelastic rows evolve one history vector per admitted Prony term over their carried `TimeStep`, and damage scales the elastic energy by live state. `Enforce` regularizes the normal gap potential, reads its composed sensitivity, and advances multipliers through `ConstraintHandling.AugmentedLagrangian.Advance`.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics key extended with the constitutive-model key, the integration-point count, the return-map iteration count (plastic), the consistent-tangent condition, and the converged flag; the contact path stamps the active-set size, penetration residual, and multiplier-update count, so a nonlinear-material or contact run is auditable on the same `Solve` receipt — never a parallel constitutive receipt.
- Packages: HyperJet, System.Numerics.Tensors, MathNet.Numerics, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new invariant plastic or hyperelastic law is parameter data on `PlasticPotential` or `HyperelasticLaw`; a genuinely different state evolution is one `ConstitutiveModel` case; a new contact discipline is one `ContactConstraint` case; a new evolving variable is one `MaterialState` field. No sibling material solver or hand-derived tangent surface appears.
- Boundary: `HyperJet` owns scalar forward hyper-dual differentiation, and this page owns only the constitutive energies and state transitions; `DDScalar.GetGradient` is stress and `DDScalar.GetHessian` is the exact local tangent. Plastic energy differentiates the regularized return multiplier, finite-strain energy rejects a non-nine-component deformation gradient and lifts a non-positive determinant failure onto `Fin`, Prony history is cardinality-bounded by the model row, and every parameter/strain vector is finite-gated before AD. Normal contact consumes only the broad-phase pairs from `Solver/clash#CLASH_AND_TWIN`, advances multipliers through `ConstraintHandling.AugmentedLagrangian.Advance`, and never claims a Coulomb tangent without tangential kinematics. `Solver/contract#SOLVE_CONTRACT` consumes stress in `f_ext − f_int(u)` and keeps any global Newton-CG or colored-Jacobian assembly outside this local owner.

```csharp signature
public sealed record SoilParameters(
    double FrictionAngle,
    double DilationAngle,
    double Cohesion,
    double CriticalStateSlope,
    double CompressionIndex,
    double SwellIndex,
    double InitialPreconsolidationPressure,
    double InitialPorePressure) {
    public bool Invalid => !double.IsFinite(FrictionAngle) || FrictionAngle is < 0.0 or >= 90.0
        || !double.IsFinite(DilationAngle) || DilationAngle is < 0.0 or >= 90.0 || DilationAngle > FrictionAngle
        || !double.IsFinite(Cohesion) || Cohesion < 0.0 || !double.IsFinite(CriticalStateSlope) || CriticalStateSlope <= 0.0
        || !double.IsFinite(CompressionIndex) || !double.IsFinite(SwellIndex) || CompressionIndex <= SwellIndex || SwellIndex <= 0.0
        || !double.IsFinite(InitialPreconsolidationPressure) || InitialPreconsolidationPressure <= 0.0 || !double.IsFinite(InitialPorePressure);
}

public sealed record PlasticPotential(double MeridianWeight, double LodeWeight, double CapWeight) {
    public static readonly PlasticPotential J2 = new(0.0, 0.0, 0.0);
    public static readonly PlasticPotential DruckerPrager = new(1.0, 0.0, 0.0);
    public static readonly PlasticPotential SmoothedMohrCoulomb = new(1.0, 0.2, 0.0);
    public static readonly PlasticPotential ModifiedCamClay = new(0.0, 0.0, 1.0);

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
    Option<SoilParameters> Soil);

public sealed record MaterialState(
    ReadOnlyMemory<double> PlasticStrain,
    double Hardening,
    double Damage,
    Seq<ReadOnlyMemory<double>> ViscoHistory,
    double VolumetricPlasticStrain,
    double PreconsolidationPressure,
    double PorePressure) {
    public static MaterialState Pristine(int components) =>
        new(new double[components], 0.0, 0.0, Seq<ReadOnlyMemory<double>>(), 0.0, 0.0, 0.0);
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

    public DDScalar Energy(DDScalar[] strain, MaterialState state, MaterialParameters parameters) =>
        Switch(
            state: (Strain: strain, State: state, Parameters: parameters),
            plastic: static (state, model) => PlasticEnergy(state.Strain, state.State, state.Parameters, model.Potential, model.Regularization),
            hyperelastic: static (state, model) => HyperelasticEnergy(state.Strain, state.Parameters, model.Law),
            viscoelastic: static (state, model) => ViscoelasticEnergy(state.Strain, state.State, state.Parameters, model.PronyTerms, model.TimeStep),
            damage: static (state, model) => (1.0 - state.State.Damage) * ElasticEnergy(state.Strain, state.State.PlasticStrain, state.Parameters));

    static DDScalar ElasticEnergy(DDScalar[] strain, ReadOnlyMemory<double> inelastic, MaterialParameters parameters) {
        int size = strain.Length;
        DDScalar trace = Constant(0.0, size), normal = Constant(0.0, size), shear = Constant(0.0, size);
        for (int component = 0; component < strain.Length; component++) {
            DDScalar elastic = strain[component] - (component < inelastic.Length ? inelastic.Span[component] : 0.0);
            if (component < 3) { trace += elastic; normal += elastic * elastic; }
            else { shear += elastic * elastic; }
        }
        double lambda = parameters.YoungModulus * parameters.PoissonRatio / ((1.0 + parameters.PoissonRatio) * (1.0 - 2.0 * parameters.PoissonRatio));
        double mu = parameters.YoungModulus / (2.0 * (1.0 + parameters.PoissonRatio));
        return 0.5 * lambda * trace * trace + mu * normal + 0.5 * mu * shear;
    }

    static DDScalar PlasticEnergy(DDScalar[] strain, MaterialState state, MaterialParameters parameters, PlasticPotential potential, double regularization) {
        int size = strain.Length;
        DDScalar[] elastic = new DDScalar[strain.Length];
        for (int component = 0; component < strain.Length; component++) {
            elastic[component] = strain[component] - (component < state.PlasticStrain.Length ? state.PlasticStrain.Span[component] : 0.0);
        }
        DDScalar mean = (elastic[0] + elastic[1] + elastic[2]) / 3.0;
        DDScalar equivalentSquared = Constant(0.0, size), thirdInvariant = Constant(1.0, size);
        for (int component = 0; component < strain.Length; component++) {
            DDScalar deviatoric = component < 3 ? elastic[component] - mean : 0.5 * elastic[component];
            equivalentSquared += deviatoric * deviatoric;
            if (component < 3) { thirdInvariant *= deviatoric; }
        }
        double mu = parameters.YoungModulus / (2.0 * (1.0 + parameters.PoissonRatio));
        double bulk = parameters.YoungModulus / (3.0 * (1.0 - 2.0 * parameters.PoissonRatio));
        DDScalar q = 2.0 * mu * Sqrt(1.5 * equivalentSquared, regularization);
        DDScalar lode = thirdInvariant / (q * q * q + regularization);
        return parameters.Soil.Match(
            Some: soil => {
                double porePressure = state.PorePressure == 0.0 ? soil.InitialPorePressure : state.PorePressure;
                DDScalar pressure = bulk * (elastic[0] + elastic[1] + elastic[2]) - porePressure;
                double friction = soil.FrictionAngle * Math.PI / 180.0;
                double preconsolidation = state.PreconsolidationPressure > 0.0 ? state.PreconsolidationPressure : soil.InitialPreconsolidationPressure;
                DDScalar frictional = q * (1.0 + potential.LodeWeight * lode)
                    + potential.MeridianWeight * (pressure * Math.Sin(friction) - soil.Cohesion * Math.Cos(friction))
                    - parameters.HardeningModulus * state.Hardening;
                DDScalar cap = Sqrt(SmoothPositive(q * q + soil.CriticalStateSlope * soil.CriticalStateSlope * pressure * (pressure - preconsolidation), regularization), regularization);
                DDScalar yield = (1.0 - potential.CapWeight) * frictional + potential.CapWeight * cap;
                DDScalar overstress = SmoothPositive(yield, regularization);
                double tangent = 3.0 * mu + parameters.HardeningModulus + potential.MeridianWeight * bulk * Math.Tan(soil.DilationAngle * Math.PI / 180.0);
                DDScalar dGamma = overstress / Math.Max(regularization, tangent);
                return ElasticEnergy(strain, state.PlasticStrain, parameters) - overstress * dGamma + 0.5 * tangent * dGamma * dGamma;
            },
            None: () => {
                DDScalar overstress = SmoothPositive(q - parameters.YieldStress - parameters.HardeningModulus * state.Hardening, regularization);
                DDScalar dGamma = overstress / (3.0 * mu + parameters.HardeningModulus);
                return ElasticEnergy(strain, state.PlasticStrain, parameters) - overstress * dGamma + 0.5 * (3.0 * mu + parameters.HardeningModulus) * dGamma * dGamma;
            });
    }

    static DDScalar HyperelasticEnergy(DDScalar[] deformation, MaterialParameters parameters, HyperelasticLaw law) {
        if (deformation.Length != 9) { throw new ArgumentException("<hyperelastic-deformation-gradient-arity>"); }
        int size = deformation.Length;
        DDScalar j = deformation[0] * (deformation[4] * deformation[8] - deformation[5] * deformation[7])
            - deformation[1] * (deformation[3] * deformation[8] - deformation[5] * deformation[6])
            + deformation[2] * (deformation[3] * deformation[7] - deformation[4] * deformation[6]);
        DDScalar[] rightCauchyGreen = new DDScalar[9];
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++) {
                rightCauchyGreen[row * 3 + column] = Constant(0.0, size);
                for (int k = 0; k < 3; k++) { rightCauchyGreen[row * 3 + column] += deformation[k * 3 + row] * deformation[k * 3 + column]; }
            }
        DDScalar i1 = rightCauchyGreen[0] + rightCauchyGreen[4] + rightCauchyGreen[8];
        DDScalar traceC2 = Constant(0.0, size);
        for (int row = 0; row < 3; row++) for (int column = 0; column < 3; column++) { traceC2 += rightCauchyGreen[row * 3 + column] * rightCauchyGreen[column * 3 + row]; }
        DDScalar i2 = 0.5 * (i1 * i1 - traceC2);
        double mu = parameters.YoungModulus / (2.0 * (1.0 + parameters.PoissonRatio));
        double lambda = parameters.YoungModulus * parameters.PoissonRatio / ((1.0 + parameters.PoissonRatio) * (1.0 - 2.0 * parameters.PoissonRatio));
        DDScalar first = i1 - 3.0, second = i2 - 3.0, volume = j - 1.0;
        return mu * (law.FirstInvariant * first + law.SecondInvariant * second + law.FirstInvariantSquared * first * first)
            - 2.0 * mu * (law.FirstInvariant + 2.0 * law.SecondInvariant) * volume
            + 0.5 * lambda * law.BulkScale * volume * volume;
    }

    static DDScalar ViscoelasticEnergy(DDScalar[] strain, MaterialState state, MaterialParameters parameters, int terms, double timeStep) {
        DDScalar energy = ElasticEnergy(strain, state.PlasticStrain, parameters);
        for (int term = 0; term < Math.Min(terms, parameters.Prony.Count); term++) {
            double decay = Math.Exp(-timeStep / Math.Max(1e-12, parameters.Prony[term].RelaxationTime));
            ReadOnlyMemory<double> history = term < state.ViscoHistory.Count ? state.ViscoHistory[term] : ReadOnlyMemory<double>.Empty;
            for (int component = 0; component < strain.Length; component++) {
                double prior = component < history.Length ? history.Span[component] : 0.0;
                DDScalar branch = strain[component] - decay * prior;
                energy += 0.5 * parameters.Prony[term].Modulus * branch * branch;
            }
        }
        return energy;
    }

    static DDScalar SmoothPositive(DDScalar value, double regularization) => 0.5 * (value + Sqrt(value * value, regularization));
    static DDScalar Sqrt(DDScalar value, double regularization) {
        DDScalar shifted = value + regularization * regularization;
        DDScalar seed = Constant(Math.Sqrt(Math.Max(regularization * regularization, shifted.Value)), shifted.GetGradient().Count);
        return toSeq(Enumerable.Range(0, 8)).Fold(seed, (root, _) => 0.5 * (root + shifted / root));
    }
    static DDScalar Constant(double value, int size) => DDScalar.Constant(value, size, order: 2);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContactConstraint {
    private ContactConstraint() { }

    public sealed record NodeToSurface(double Gap, double Regularization) : ContactConstraint;
    public sealed record Mortar(double Gap, double Regularization, ReadOnlyMemory<double> Weights) : ContactConstraint;

    public (double Gap, double Regularization) Parameters() =>
        Switch(
            nodeToSurface: static contact => (contact.Gap, contact.Regularization),
            mortar: static contact => (contact.Gap, contact.Regularization));

    // Mortar pairs weight the segment-integrated gap; node-to-surface pairs are pointwise and carry the unit weight
    public double Weight(int pair) =>
        Switch(
            state: pair,
            nodeToSurface: static (_, _) => 1.0,
            mortar: static (index, contact) => contact.Weights.Span[index]);

    public DDScalar Potential(DDScalar[] penetration, double penalty) {
        double regularization = Parameters().Regularization;
        DDScalar energy = DDScalar.Constant(0.0, penetration.Length, order: 2);
        foreach (DDScalar coordinate in penetration) {
            DDScalar radicand = coordinate * coordinate + regularization * regularization;
            DDScalar seed = DDScalar.Constant(Math.Sqrt(Math.Max(regularization * regularization, radicand.Value)), penetration.Length, order: 2);
            DDScalar root = toSeq(Enumerable.Range(0, 8)).Fold(seed, (current, _) => 0.5 * (current + radicand / current));
            DDScalar positive = 0.5 * (coordinate + root);
            energy += 0.5 * penalty * positive * positive;
        }
        return energy;
    }
}

public static class StressUpdate {
    public static Fin<ConstitutiveResult> Stress(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, IClock clock) =>
        from valid in Validate(model, strain, state, parameters)
        from verdict in ReturnMapVerdict(model, strain, state, parameters)
        from result in Try.lift(() => {
            DDScalar[] active = DDScalar.Variables(strain.ToArray(), order: 2);
            DDScalar energy = model.Energy(active, state, parameters);
            Vector<double> gradient = energy.GetGradient();
            Matrix<double> hessian = energy.GetHessian();
            return new ConstitutiveResult(
                gradient.AsArray().AsMemory(), hessian.ToRowMajorArray().AsMemory(),
                Evolve(model, state, strain, parameters, verdict.DGamma), verdict.Iterations, Converged: true, clock.GetCurrentInstant());
        }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<constitutive-energy-domain:{error.Message}>"))
        select result;

    static Fin<Unit> Validate(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters) =>
        strain.IsEmpty || !TensorPrimitives.IsFiniteAll<double>(strain.Span)
            ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<constitutive-strain>"))
            : !double.IsFinite(parameters.YoungModulus) || parameters.YoungModulus <= 0.0 || !double.IsFinite(parameters.PoissonRatio) || parameters.PoissonRatio is <= -1.0 or >= 0.5 || !double.IsFinite(parameters.YieldStress) || parameters.YieldStress < 0.0 || !double.IsFinite(parameters.HardeningModulus) || parameters.HardeningModulus < 0.0 || !double.IsFinite(parameters.DamageThreshold) || parameters.Prony.Exists(static term => !double.IsFinite(term.Modulus) || term.Modulus < 0.0 || !double.IsFinite(term.RelaxationTime) || term.RelaxationTime <= 0.0) || parameters.Soil.Exists(static soil => soil.Invalid)
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<constitutive-parameters>"))
                : model.Switch(
                    state: (Strain: strain, Parameters: parameters),
                    plastic: static (input, plastic) => input.Strain.Length == 6 && !plastic.Potential.Invalid && double.IsFinite(plastic.Regularization) && plastic.Regularization > 0.0 && (plastic.Potential == PlasticPotential.J2 || input.Parameters.Soil.IsSome),
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

    static Fin<(int Iterations, double DGamma)> ReturnMapVerdict(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters) =>
        model is ConstitutiveModel.Plastic plastic ? RadialReturn(strain, state, parameters, plastic.Potential, plastic.Regularization) : Fin.Succ((0, 0.0));

    static Fin<(int Iterations, double DGamma)> RadialReturn(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters p, PlasticPotential potential, double regularization) {
        double mu = p.YoungModulus / (2.0 * (1.0 + p.PoissonRatio)), hardening = p.HardeningModulus;
        double[] elastic = new double[strain.Length];
        ReadOnlySpan<double> plastic = state.PlasticStrain.Span;
        for (int i = 0; i < elastic.Length; i++) { elastic[i] = strain.Span[i] - (i < plastic.Length ? plastic[i] : 0.0); }
        double yield = Yield(elastic, state, p, potential, regularization);
        double overstress = 0.5 * (yield + Math.Sqrt(yield * yield + regularization * regularization));
        double dilation = p.Soil.Map(static soil => Math.Tan(soil.DilationAngle * Math.PI / 180.0)).IfNone(0.0);
        double bulk = p.YoungModulus / (3.0 * (1.0 - 2.0 * p.PoissonRatio));
        return Fin.Succ((overstress > regularization ? 1 : 0, overstress / Math.Max(1e-30, 3.0 * mu + hardening + potential.MeridianWeight * bulk * dilation)));
    }

    static double Yield(double[] elastic, MaterialState state, MaterialParameters parameters, PlasticPotential potential, double regularization) {
        double mean = (elastic[0] + elastic[1] + elastic[2]) / 3.0, equivalentSquared = 0.0, thirdInvariant = 1.0;
        for (int component = 0; component < elastic.Length; component++) {
            double deviatoric = component < 3 ? elastic[component] - mean : 0.5 * elastic[component];
            equivalentSquared += deviatoric * deviatoric;
            if (component < 3) { thirdInvariant *= deviatoric; }
        }
        double mu = parameters.YoungModulus / (2.0 * (1.0 + parameters.PoissonRatio));
        double q = 2.0 * mu * Math.Sqrt(1.5 * equivalentSquared + regularization * regularization);
        return parameters.Soil.Match(
            Some: soil => {
                double bulk = parameters.YoungModulus / (3.0 * (1.0 - 2.0 * parameters.PoissonRatio));
                double porePressure = state.PorePressure == 0.0 ? soil.InitialPorePressure : state.PorePressure;
                double pressure = bulk * (elastic[0] + elastic[1] + elastic[2]) - porePressure;
                double friction = soil.FrictionAngle * Math.PI / 180.0;
                double preconsolidation = state.PreconsolidationPressure > 0.0 ? state.PreconsolidationPressure : soil.InitialPreconsolidationPressure;
                double lode = thirdInvariant / (q * q * q + regularization);
                double frictional = q * (1.0 + potential.LodeWeight * lode) + potential.MeridianWeight * (pressure * Math.Sin(friction) - soil.Cohesion * Math.Cos(friction)) - parameters.HardeningModulus * state.Hardening;
                double cap = Math.Sqrt(Math.Max(0.0, q * q + soil.CriticalStateSlope * soil.CriticalStateSlope * pressure * (pressure - preconsolidation)) + regularization * regularization);
                return (1.0 - potential.CapWeight) * frictional + potential.CapWeight * cap;
            },
            None: () => q - parameters.YieldStress - parameters.HardeningModulus * state.Hardening);
    }

    static MaterialState Evolve(ConstitutiveModel model, MaterialState state, ReadOnlyMemory<double> strain, MaterialParameters parameters, double dGamma) =>
        model.Switch(
            state: (State: state, Strain: strain, Parameters: parameters, DGamma: dGamma),
            plastic: static (input, model) => PlasticEvolution(input.State, input.Strain, input.Parameters, model.Potential, input.DGamma),
            hyperelastic: static (input, _) => input.State,
            viscoelastic: static (input, model) => input.State with {
                ViscoHistory = toSeq(Enumerable.Range(0, Math.Max(0, model.PronyTerms))).Map(term => RelaxedHistory(input.State, input.Strain, input.Parameters, term, model.TimeStep)),
            },
            damage: static (input, model) => input.State with {
                Damage = Math.Min(1.0, input.State.Damage + model.Exponent * Math.Max(0.0, Math.Sqrt(TensorPrimitives.SumOfSquares<double>(input.Strain.Span)) - input.Parameters.DamageThreshold)),
            });

    static MaterialState PlasticEvolution(MaterialState state, ReadOnlyMemory<double> strain, MaterialParameters parameters, PlasticPotential potential, double dGamma) =>
        parameters.Soil.Match(
            Some: soil => {
                double dilation = potential.MeridianWeight * Math.Tan(soil.DilationAngle * Math.PI / 180.0);
                double volumetric = dGamma * dilation;
                double preconsolidation = state.PreconsolidationPressure > 0.0 ? state.PreconsolidationPressure : soil.InitialPreconsolidationPressure;
                return state with {
                    PlasticStrain = Accumulated(state.PlasticStrain, strain, dGamma, dilation),
                    Hardening = state.Hardening + dGamma,
                    VolumetricPlasticStrain = state.VolumetricPlasticStrain + volumetric,
                    PreconsolidationPressure = preconsolidation * Math.Exp(Math.Clamp(volumetric / (soil.CompressionIndex - soil.SwellIndex), -20.0, 20.0)),
                    PorePressure = state.PorePressure == 0.0 ? soil.InitialPorePressure : state.PorePressure,
                };
            },
            None: () => state with {
                PlasticStrain = Accumulated(state.PlasticStrain, strain, dGamma, 0.0),
                Hardening = state.Hardening + dGamma,
            });

    static ReadOnlyMemory<double> RelaxedHistory(MaterialState state, ReadOnlyMemory<double> strain, MaterialParameters parameters, int term, double timeStep) {
        ReadOnlyMemory<double> prior = term < state.ViscoHistory.Count ? state.ViscoHistory[term] : ReadOnlyMemory<double>.Empty;
        double[] next = new double[strain.Length];
        double decay = Math.Exp(-timeStep / Math.Max(1e-12, parameters.Prony[term].RelaxationTime));
        for (int component = 0; component < next.Length; component++) {
            next[component] = decay * (component < prior.Length ? prior.Span[component] : 0.0) + (1.0 - decay) * strain.Span[component];
        }
        return next;
    }

    static ReadOnlyMemory<double> Accumulated(ReadOnlyMemory<double> plastic, ReadOnlyMemory<double> strain, double dGamma, double dilation) {
        double[] elastic = new double[Math.Max(plastic.Length, strain.Length)];
        for (int i = 0; i < elastic.Length; i++) { elastic[i] = (i < strain.Length ? strain.Span[i] : 0.0) - (i < plastic.Length ? plastic.Span[i] : 0.0); }
        double norm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(elastic));
        double[] next = new double[elastic.Length];
        for (int i = 0; i < next.Length; i++) {
            double volumetric = i < 3 ? dGamma * dilation / 3.0 : 0.0;
            next[i] = (i < plastic.Length ? plastic.Span[i] : 0.0) + (norm > 1e-30 ? dGamma * elastic[i] / norm : 0.0) + volumetric;
        }
        return next;
    }
}

public static class ContactEnforcement {
    public static Fin<ContactResult> Enforce(ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers, double penalty, Seq<(int Slave, int Master)> broadPhasePairs, IClock clock) {
        if (displacement.IsEmpty || !TensorPrimitives.IsFiniteAll<double>(displacement.Span) || broadPhasePairs.IsEmpty || broadPhasePairs.Exists(pair => pair.Slave < 0 || pair.Master < 0 || pair.Slave >= displacement.Length || pair.Master >= displacement.Length)) {
            return Fin.Fail<ContactResult>(new ComputeFault.ModelRejected("<contact-kinematics>"));
        }
        if (contact is ContactConstraint.Mortar mortar && (mortar.Weights.Length != broadPhasePairs.Count || !TensorPrimitives.IsFiniteAll<double>(mortar.Weights.Span) || mortar.Weights.Span.ToArray().Any(static weight => weight <= 0.0))) {
            return Fin.Fail<ContactResult>(new ComputeFault.ModelRejected($"<contact-mortar-weights:{mortar.Weights.Length}!={broadPhasePairs.Count}>"));
        }
        double[] gap = Gap(contact, displacement, broadPhasePairs);
        (double baseGap, double regularization) = contact.Parameters();
        if (!double.IsFinite(penalty) || penalty <= 0.0 || !double.IsFinite(baseGap) || !double.IsFinite(regularization) || regularization <= 0.0 || multipliers.Length != gap.Length) {
            return Fin.Fail<ContactResult>(new ComputeFault.ModelRejected("<contact-admission>"));
        }
        double[] updated = ConstraintHandling.AugmentedLagrangian.Advance(multipliers.Span.ToArray(), gap, penalty);
        return Try.lift(() => {
            DDScalar potential = contact.Potential(DDScalar.Variables(gap, order: 2), penalty);
            return new ContactResult(
                potential.GetGradient().AsArray().AsMemory(), potential.GetHessian().ToRowMajorArray().AsMemory(),
                updated.AsMemory(), gap.Count(static value => value > 0.0), Penetration(gap), clock.GetCurrentInstant());
        }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<contact-potential-domain:{error.Message}>"));
    }

    static double[] Gap(ContactConstraint contact, ReadOnlyMemory<double> displacement, Seq<(int Slave, int Master)> pairs) {
        double baseGap = contact.Parameters().Gap;
        double[] gap = new double[pairs.Count];
        for (int i = 0; i < pairs.Count; i++) {
            (int slave, int master) = pairs[i];
            double ds = displacement.Span[slave], dm = displacement.Span[master];
            gap[i] = contact.Weight(i) * (ds - dm) - baseGap;
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
