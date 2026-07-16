# [COMPUTE_SOLVER_CONSTITUTIVE]

Rasm.Compute per-Gauss-point material law: `ConstitutiveModel` `[Union]` is the stress-update axis (plasticity / hyperelasticity / viscoelasticity / damage) and `ContactConstraint` the frictional-contact enforcement, extracted whole from the solve contract as its own single-fold owner — a per-integration-point concern distinct from the `PhysicsKind` physics-assembly axis (a fourth `MaterialForm` literal is the deleted form). `StressUpdate.Stress` returns the updated stress, the AD-consistent tangent, and the evolved `MaterialState` per point; `ContactEnforcement.Enforce` binds the `Solver/optimizer#OPTIMIZER_LANE` `ConstraintHandling.AugmentedLagrangian` multiplier machinery to the gap function over the broad-phase pair set `Solver/clash#CLASH_AND_TWIN` supplies.

Stress is `∂W/∂ε` through the `Tensor/dispatch#EQUIVALENCE_INTEROP` reverse-mode `SensitivityLaw.Chain` over each case's recorded energy tape, and the tangent is the SPD Gauss-Newton curvature through `SensitivityLaw.GaussNewton` — the AD engine is composed, never re-implemented. `Solver/contract#SOLVE_CONTRACT` imports FROM this page: its `SolveProblem.Material` option carries the law and its modified-Newton internal-force residual folds `StressUpdate.Stress` at every Gauss point. Elastic `(E, ν)` and inelastic calibration read once from the `Rasm.Element` `ElementGraph` via `graph.PropertiesOf(id).Mechanical` (the real seam member; a `MechanicalOf` accessor spelling is a phantom), keyed by the seam `NodeId`, concrete above the seam.

## [01]-[INDEX]

- [01]-[CONSTITUTIVE]: per-Gauss-point stress-update axis (plasticity/hyperelastic/viscoelastic/damage) and frictional-contact enforcement whose tangent is the AD SPD Gauss-Newton curvature of the strain-energy/gap potential.

## [02]-[CONSTITUTIVE]

- Owner: `ConstitutiveModel` `[Union]` the per-Gauss-point material-law axis, each case carrying its strain-energy/yield function and evolving per-point state, distinct from the `PhysicsKind` physics-assembly axis (a new Solver owner, never a fourth `MaterialForm` literal); `ContactConstraint` `[Union]` the frictional-contact axis (normal gap, Coulomb stick-slip) reusing the optimizer feasibility machinery; `StressUpdate` the static fold returning `(stress = ∂W/∂ε, consistent tangent ≈ the SPD Gauss-Newton curvature JᵀJ·v)` per integration point; `ContactEnforcement` the static fold binding the `Solver/optimizer#OPTIMIZER_LANE` `ConstraintHandling.AugmentedLagrangian` update `λ ← λ + ρ·g` to the gap function with the SPD contact stiffness from the regularized potential's Gauss-Newton curvature; `MaterialState` the per-point evolving-state carrier (plastic strain, hardening, damage); `ConstitutiveResult`/`ContactResult` the field-plus-tangent carriers riding the `Solve` receipt.
- Cases: `ConstitutiveModel` `Plastic` (return-mapping, yield + hardening) · `Hyperelastic` (Neo-Hookean / Mooney-Rivlin) · `Viscoelastic` (Prony series) · `Damage` (scalar damage variable); `ContactConstraint` `NodeToSurface(Slave, Master, Gap, Regularization)` · `Mortar(SlaveSegments, MasterSegments, Gap, Regularization)`.
- Entry: `public static Fin<ConstitutiveResult> Stress(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, ClockPolicy clocks)` returns the updated stress, the per-point consistent tangent, and the evolved state — `Fin<T>` aborts on a non-converged return-map; `public static Fin<ContactResult> Enforce(ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers, double penalty, Seq<(int Slave, int Master)> broadPhasePairs, ClockPolicy clocks)` returns the contact force, stiffness, and updated multipliers over the broad-phase pair set the Clash lane supplies.
- Auto: `Stress` folds the case to its stored-energy `W(ε)`, reads `stress = ∂W/∂ε` through the reverse-mode `SensitivityLaw.Chain` over the recorded tape and `tangent ≈ JᵀJ·v` (SPD Gauss-Newton curvature) through `SensitivityLaw.GaussNewton`, so adding a material law is one case carrying its energy function, never a hand-coded `D`-matrix; the `Plastic` case differentiates THROUGH the return-mapping iteration so the AD tangent is the algorithmic (consistent) tangent — a naive AD of the elastic predictor gives the continuum tangent and breaks Newton convergence (the named defect). `Enforce` regularizes the gap/stick-slip potential, reads the contact stiffness as that regularized potential's SPD Gauss-Newton curvature, and updates the multiplier through the existing `AugmentedLagrangian` `λ ← λ + ρ·g`, so contact reuses the optimizer feasibility machinery, not a new solver.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics key extended with the constitutive-model key, the integration-point count, the return-map iteration count (plastic), the consistent-tangent condition, and the converged flag; the contact path stamps the active-set size, penetration residual, and multiplier-update count, so a nonlinear-material or contact run is auditable on the same `Solve` receipt — never a parallel constitutive receipt.
- Packages: System.Numerics.Tensors, MathNet.Numerics, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new material law is one `ConstitutiveModel` case carrying its strain-energy/yield function (the consistent tangent derives by AD); a new contact discipline is one `ContactConstraint` case; a new evolving-state field is one column on `MaterialState`; zero new surface — a `PlasticityModel`/`HyperelasticModel`/… sibling family, a hand-derived `D`-matrix beside the AD Gauss-Newton tangent, and a contact solver parallel to the optimizer multiplier machinery are the rejected forms.
- Boundary: the constitutive tangent rides the AD `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.GaussNewton` SPD Gauss-Newton curvature `JᵀJ` of the stored-energy function — never a hand-derived `D`-matrix — and the exact `∂²W/∂ε²` Hessian-vector product (an `f''` row on `DifferentiableOp` plus a forward-over-reverse sweep) is the owner's documented second-order open leaf; each case differentiates its own energy, and a closed-form analytic energy rides the `Symbolic/expression#OPERATION_FOLD` differentiate family rather than AD. Frictional stick-slip is non-smooth and non-associative, so the AD tangent of a regularized friction potential is consistent only within the regularization — the case carries the regularization parameter and the broad-phase pair set from `Solver/clash#CLASH_AND_TWIN`, never re-detecting contact pairs (the named defect), the augmented-Lagrangian update reuses the optimizer `ConstraintHandling.AugmentedLagrangian` `Penalize`/`Lagrange` machinery, and the contact tangent reuses `SensitivityLaw.GaussNewton`. Constitutive stress `σ = ∂W/∂ε` feeds the `Solver/contract#SOLVE_CONTRACT` modified-Newton internal-force residual `f_ext − f_int(u)` (routed by `problem.Material.IsSome`), so a nonlinear FEM solve converges on the genuine residual over the held elastic tangent; the consistent SPD Gauss-Newton curvature — a Hessian-VECTOR product, exact-quadratic only at a zero-residual minimum — is the matrix-free Newton-CG second-order tangent the open exact-Hessian leaf lands. A colored Jacobian for a large nonlinear system assembles through the `Tensor/factor#SPARSE_ALGEBRA` `SparseTensorOpFamily` rows and the `Tensor/dispatch#EQUIVALENCE_INTEROP` `JacobianColoring`.

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

    // The stored-energy tape W(ε) the AD modes read, one per ConstitutiveModel case. The Plastic arm's ReLU
    // corrector captures the radial-return rectification so its AD Gauss-Newton curvature is the consistent (not the
    // elastic-predictor continuum) tangent for linear hardening; return-map convergence gates on ReturnMapVerdict.
    public Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> EnergyTape(ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters) =>
        Switch(
            state: (Strain: strain, State: state, Parameters: parameters),
            plastic: static (s, p) => ReturnMapTape(s.Strain, s.State, s.Parameters, p.MaxReturnMapIterations),
            hyperelastic: static (s, h) => HyperelasticTape(s.Strain, s.Parameters, h.MooneyRivlin),
            viscoelastic: static (s, v) => PronyTape(s.Strain, s.State, s.Parameters, v.PronyTerms),
            damage: static (s, d) => DamageTape(s.Strain, s.State, s.Parameters, d.Exponent));

    // Each tape is a differentiable composition SensitivityLaw chains: a nonlinear corrector (ReLU plastic, Tanh
    // hyperelastic, Exp Prony, Sigmoid damage — each a bound DifferentiableOp) over the elastic D MatMul base, so
    // Chain(tape, ones) is the stress and GaussNewton(tape, …) the SPD curvature tangent. Unrolling the multi-
    // iteration return-map into the linear (op, primal) tape is the remaining research leaf (03-RESEARCH).
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

    // Regularized gap potential whose AD Gauss-Newton curvature is the contact stiffness; the regularization
    // parameter bounds the stick-slip non-smoothness so the tangent is consistent within it.
    public Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> PotentialTape(ReadOnlyMemory<double> displacement, double penalty) =>
        Switch(
            state: (Displacement: displacement, Penalty: penalty),
            nodeToSurface: static (s, c) => GapPotentialTape(s.Displacement, s.Penalty, c.Gap, c.Regularization),
            mortar: static (s, c) => MortarPotentialTape(s.Displacement, s.Penalty, c.Gap, c.Regularization));

    // The penalty-scaled penetration ramps through a ReLU (consistent within the regularization width); the Mortar
    // case adds the Sigmoid segment-weighting that smears the constraint across the master segment.
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
    // <return-map-diverged> fault (never a hardcoded Converged: true). The closed-form cases return 0 iterations.
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

    // Radial-return verdict: J2 plasticity with linear isotropic hardening converges in one step, nonlinear
    // hardening iterates to MaxReturnMapIterations; a non-converged return is the <return-map-diverged> fault.
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

    // Evolve the per-point state (plastic strain, hardening, damage, visco history) — the evolving state distinct
    // from the physics-assembly axis.
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

    // The signed penetration per broad-phase pair the Clash lane supplied (positive = penetrating): closing
    // relative normal displacement past the initial gap, never a re-detection of the pairs.
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [CONSTITUTIVE_RETURN_MAP_TAPE]-[OPEN]: how to unroll the multi-iteration return-map into the linear `(op, primal)` tape at the stress-update call site — the chain tape composes the single-step closed-form return today, and the ring/reduction adjoint rows are already bound on `Tensor/dispatch#EQUIVALENCE_INTEROP` `DifferentiableOp.Rows`, so only the multi-iteration unroll is open; `Tensor/dispatch` `[GAUSS_NEWTON_AND_COLORING]` carries the remaining tape-recording plumbing.
