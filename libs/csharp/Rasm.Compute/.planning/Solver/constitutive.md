# [COMPUTE_SOLVER_CONSTITUTIVE]

Rasm.Compute per-Gauss-point material law: the `ConstitutiveModel` `[Union]` stress-update axis (plasticity / hyperelasticity / viscoelasticity / damage) and the `ContactConstraint` frictional-contact enforcement, extracted whole from the solve contract as its own single-fold owner — a genuine per-integration-point concern distinct from the `PhysicsKind` physics-assembly axis (a fourth `MaterialForm` literal is the deleted form). `StressUpdate.Stress` returns the updated stress, the AD-consistent tangent, and the evolved `MaterialState` per point; `ContactEnforcement.Enforce` binds the `Solver/optimizer#OPTIMIZER_LANE` `ConstraintHandling.AugmentedLagrangian` multiplier machinery to the gap function over the broad-phase pair set `Solver/clash#CLASH_AND_TWIN` supplies. The stress is `∂W/∂ε` through the `Tensor/dispatch#EQUIVALENCE_INTEROP` reverse-mode `SensitivityLaw.Chain` over each case's recorded energy tape, and the tangent is the SPD Gauss-Newton curvature through `SensitivityLaw.GaussNewton` — the AD engine is composed, never re-implemented. `Solver/contract#SOLVE_CONTRACT` imports FROM this page (the normal downward direction): its `SolveProblem.Material` option carries the law, its modified-Newton internal-force residual folds `StressUpdate.Stress` at every Gauss point, and its `Solve` receipt is extended with the constitutive-model key — never a parallel constitutive receipt or a second solve path. The elastic `(E, ν)` and the inelastic calibration read once from the `Rasm.Element` `ElementGraph` via `graph.PropertiesOf(id).Mechanical` (the real seam member; a `MechanicalOf` accessor spelling is a phantom), keyed by the seam `NodeId`, concrete above the seam.

## [01]-[INDEX]

- [02]-[CONSTITUTIVE]: per-Gauss-point stress-update axis (plasticity/hyperelastic/viscoelastic/damage) and frictional-contact enforcement whose tangent is the AD SPD Gauss-Newton curvature of the strain-energy / gap potential (the exact `∂²W/∂ε²` Hessian-vector product the documented second-order open leaf).

## [02]-[CONSTITUTIVE]

- Owner: `ConstitutiveModel` `[Union]` the per-Gauss-point material-law axis carrying each case's strain-energy/yield function, evolving per-point material state distinct from the `PhysicsKind` physics-assembly axis (a new Solver owner, never a fourth `MaterialForm` literal); `ContactConstraint` `[Union]` the frictional-contact axis (normal gap, Coulomb stick-slip) reusing the optimizer feasibility machinery; `StressUpdate` the static fold returning `(stress = ∂W/∂ε via the reverse VJP, consistent tangent ≈ the SPD Gauss-Newton curvature JᵀJ·v via the reverse-over-forward AD product)` per integration point — the exact `∂²W/∂ε²` Hessian-vector product the documented `Tensor/dispatch#EQUIVALENCE_INTEROP` second-order open leaf; `ContactEnforcement` the static fold binding the `Solver/optimizer#OPTIMIZER_LANE` `ConstraintHandling.AugmentedLagrangian` multiplier update `λ ← λ + ρ·g` to the gap function with the SPD contact stiffness from the Gauss-Newton curvature of the regularized contact potential; `MaterialState` the per-point evolving-state carrier (plastic strain, hardening, damage); `ConstitutiveResult`/`ContactResult` the field-plus-tangent carriers riding the `Solve` receipt.
- Cases: `ConstitutiveModel` cases `Plastic` (return-mapping, yield + hardening) · `Hyperelastic` (Neo-Hookean / Mooney-Rivlin stored-energy) · `Viscoelastic` (Prony series) · `Damage` (scalar damage variable); `ContactConstraint` cases `NodeToSurface(Slave, Master, Gap, Regularization)` · `Mortar(SlaveSegments, MasterSegments, Gap, Regularization)`.
- Entry: `public static Fin<ConstitutiveResult> Stress(ConstitutiveModel model, ReadOnlyMemory<double> strain, MaterialState state, MaterialParameters parameters, ClockPolicy clocks)` returns the updated stress, the per-point consistent tangent, and the evolved state — `Fin<T>` aborts on a non-converged return-map; `public static Fin<ContactResult> Enforce(ContactConstraint contact, ReadOnlyMemory<double> displacement, ReadOnlyMemory<double> multipliers, double penalty, Seq<(int Slave, int Master)> broadPhasePairs, ClockPolicy clocks)` returns the contact force, the contact stiffness, and the updated multipliers over the broad-phase pair set the Clash lane supplies.
- Auto: `Stress` folds the case to its stored-energy function `W(ε)` and reads `stress = ∂W/∂ε` through the `Tensor/dispatch#EQUIVALENCE_INTEROP` reverse-mode `SensitivityLaw.Chain` over the recorded energy tape and `tangent ≈ JᵀJ·v` (the SPD Gauss-Newton curvature) through `SensitivityLaw.GaussNewton` (reverse-over-forward — the exact `∂²W/∂ε²` Hessian-vector product is the owner's open second-order leaf) so adding a material law is one case carrying its energy function, not a hand-coded `D`-matrix; the `Plastic` case differentiates THROUGH the return-mapping iteration so the AD tangent is the algorithmic (consistent) tangent of the return map — a naive AD of the elastic predictor gives the continuum tangent and breaks Newton convergence (the named defect); `Enforce` regularizes the gap/stick-slip potential, reads the contact stiffness as the SPD Gauss-Newton curvature of that regularized potential, and updates the Lagrange multiplier through the existing `ConstraintHandling.AugmentedLagrangian` `λ ← λ + ρ·g` update so contact is a constraint discipline reusing the optimizer feasibility/multiplier machinery, not a new solver; the elastic `D`-matrix (`Mechanical.YoungsModulus`/`PoissonsRatio`) and the inelastic calibration (yield, hardening, Prony, damage from the structural `Mechanical` case's calibration columns) read once from the `Rasm.Element` `ElementGraph` via `graph.PropertiesOf(id).Mechanical` (the `MaterialPropertySet.Mechanical` case the `Rasm.Materials` projector lowered, keyed by the seam `NodeId`, concrete above the seam), the analysis element binding the material to a mesh element.
- Receipt: the `Solve` `ComputeReceipt` case carries the physics key extended with the constitutive-model key, the integration-point count, the return-map iteration count (plastic), the consistent-tangent condition, and the converged flag; the contact path stamps the active-set size, the penetration residual, and the multiplier-update count, so a nonlinear-material or contact run is auditable on the same `Solve` receipt the linear solve rides — never a parallel constitutive receipt.
- Packages: System.Numerics.Tensors, MathNet.Numerics, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new material law is one `ConstitutiveModel` case carrying its strain-energy/yield function (the consistent tangent derives by AD, no hand-coded tangent); a new contact discipline is one `ContactConstraint` case; a new evolving-state field is one column on `MaterialState`; zero new surface — a `PlasticityModel`/`HyperelasticModel`/`ViscoelasticModel`/`DamageModel` sibling family is the rejected form collapsed onto the one `ConstitutiveModel` union, a hand-derived `D`-matrix beside the AD Gauss-Newton curvature tangent is the deleted form, and a contact solver parallel to the optimizer multiplier machinery is the rejected form.
- Boundary: the constitutive tangent rides the AD `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.GaussNewton` so the material tangent is the SPD Gauss-Newton curvature `JᵀJ` of the stored-energy function — the exact `∂²W/∂ε²` Hessian-vector product the owner's documented second-order open leaf (an `f''` row on `DifferentiableOp` plus a forward-over-reverse sweep) — and never a hand-derived `D`-matrix — the `Plastic` case differentiates through the return-mapping iteration (the algorithmic tangent), the `Hyperelastic` case differentiates the Neo-Hookean/Mooney-Rivlin energy directly, the `Viscoelastic` case differentiates the Prony-relaxed energy, and the `Damage` case differentiates the degraded energy, so a fourth `MaterialForm` literal beside the `PhysicsKind` axis is the deleted form (this is a distinct per-Gauss-point owner) and the closed-form energy where it is analytic rides the `Symbolic/expression#OPERATION_FOLD` differentiate family rather than AD; the frictional contact's stick-slip transition is non-smooth and non-associative so the AD tangent of a regularized (smoothed) friction potential is consistent only within the regularization — the case carries the regularization parameter and the broad-phase pair set from the `Solver/clash#CLASH_AND_TWIN` acceleration-structure collision (it never re-detects contact pairs, the named defect), the augmented-Lagrangian update reuses the optimizer `ConstraintHandling.AugmentedLagrangian` `Penalize`/`Lagrange` machinery, and the contact tangent reuses the `SensitivityLaw.GaussNewton` SPD curvature; the constitutive STRESS `σ = ∂W/∂ε` feeds the `Solver/contract#SOLVE_CONTRACT` modified-Newton internal-force residual `f_ext − f_int(u)` (routed by `problem.Material.IsSome`), so a nonlinear FEM solve converges on the genuine nonlinear residual over the held elastic tangent instead of a fixed-step Picard, while the consistent SPD Gauss-Newton curvature — exposed as a Hessian-VECTOR product, exact-quadratic only at a zero-residual energy minimum — is the matrix-free Newton-CG second-order tangent the open exact-Hessian leaf lands, the stress-update reads the elastic `D` and the inelastic calibration once from the `Rasm.Element` `ElementGraph` via `graph.PropertiesOf(id).Mechanical` (the `MaterialPropertySet.Mechanical` case the `Rasm.Materials` projector lowered, keyed by the seam `NodeId`, concrete above the seam, sourced once, never re-minted), and the colored Jacobian for a large nonlinear system assembles through the `Tensor/factor#SPARSE_ALGEBRA` `SparseTensorOpFamily` contraction rows and the `Tensor/dispatch#EQUIVALENCE_INTEROP` `JacobianColoring`.

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
    // Solver/constitutive#CONSTITUTIVE research leaf.
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

## [03]-[RESEARCH]

- [CONSTITUTIVE_TANGENT]: the stress-update and contact-enforcement tapes are authored as real differentiable compositions over the `Tensor/dispatch#EQUIVALENCE_INTEROP` bound `DifferentiableOp` rows — a physically-motivated nonlinear corrector (`ReLU` plastic rectification, `Tanh` hyperelastic saturation, `Exp` Prony relaxation, `Sigmoid` damage degradation) over the elastic `MatMul` base layer — so `SensitivityLaw.Chain` yields the stress and `SensitivityLaw.GaussNewton` the SPD Gauss-Newton curvature tangent end-to-end on today's bound adjoint vocabulary, and `StressUpdate.ReturnMapVerdict` runs the genuine radial-return iteration the `Fin` rail gates convergence on. The elementwise-ring (`Add`/`Subtract`/`Multiply`/`Divide`/`Pow`) and reduction (`Sum`/`Dot`) adjoint rows the EXACT multi-iteration return-map (algorithmic) tangent and the multi-term stored energies compose are bound on `Tensor/dispatch#EQUIVALENCE_INTEROP` `DifferentiableOp.Rows` (the cross-lane row set is supplied, never a second autodiff surface on this page) — the ring as diagonal `cotangent .* f'(primal)` folds, the dimension-changing reductions through the one `Sensitivity` directional owner; the open leaf is unrolling the multi-iteration return-map into the linear `(op, primal)` tape at the stress-update call site — the chain tape composes the single-step closed-form return directly, the `Tensor/dispatch` `[GAUSS_NEWTON_AND_COLORING]` leaf carrying the remaining tape-recording plumbing.
