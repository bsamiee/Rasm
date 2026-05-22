using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
[System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Design", checkId: "CA1515", Justification = "xUnit discovers public test surface.")]
public static class FieldGens {
    public static readonly Op Key = Op.Of(name: "field-test");
    public static readonly Gen<CsgKind> Csg = Gen.OneOfConst(CsgKind.Union, CsgKind.Intersect, CsgKind.Difference);
    public static readonly Gen<KernelKind> Kernel = Gen.OneOfConst(KernelKind.Wendland, KernelKind.Quintic, KernelKind.Cosine, KernelKind.Cubic, KernelKind.Linear, KernelKind.Epanechnikov);
    public static readonly Gen<IntegratorKind> Integrator = Gen.OneOfConst(
        IntegratorKind.Euler, IntegratorKind.Heun, IntegratorKind.Midpoint, IntegratorKind.Ralston, IntegratorKind.RK4, IntegratorKind.RK38,
        IntegratorKind.BogackiShampine, IntegratorKind.CashKarp, IntegratorKind.DormandPrince);
    public static readonly Gen<IntegratorKind> Adaptive = Gen.OneOfConst(
        IntegratorKind.BogackiShampine, IntegratorKind.CashKarp, IntegratorKind.DormandPrince);
    public static readonly Gen<IntegratorKind> NonAdaptive = Gen.OneOfConst(
        IntegratorKind.Euler, IntegratorKind.Heun, IntegratorKind.Midpoint, IntegratorKind.Ralston, IntegratorKind.RK4, IntegratorKind.RK38);
    public static double Sum(Seq<double> xs) => xs.Fold(initialState: 0.0, f: static (acc, x) => acc + x);
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class FieldBlendLaws {
    [Fact]
    public void KeysAreDistinct() =>
        Spec.SmartEnumKeysUnique(items: [FieldBlend.Sum, FieldBlend.Average], key: static b => b.Key);
    [Fact]
    public void ScalarBlendMatchesFoldOracles() =>
        Spec.ForAll(Gens.NonEmptySeq(Gens.Finite, max: 32), xs => {
            Spec.Succ(FieldBlend.Sum.CombineScalar(values: xs, key: FieldGens.Key),
                then: r => Spec.EqualWithin(left: r, right: FieldGens.Sum(xs: xs), tolerance: 1.0e-9, what: "sum"));
            Spec.Succ(FieldBlend.Average.CombineScalar(values: xs, key: FieldGens.Key),
                then: r => Spec.EqualWithin(left: r, right: FieldGens.Sum(xs: xs) / xs.Count, tolerance: 1.0e-9, what: "mean"));
        });
    [Fact]
    public void EmptyScalarCombineFailsForBoth() {
        Spec.Fail(FieldBlend.Sum.CombineScalar(values: Seq<double>(), key: FieldGens.Key));
        Spec.Fail(FieldBlend.Average.CombineScalar(values: Seq<double>(), key: FieldGens.Key));
    }
}

public sealed class CsgKindLaws {
    [Fact]
    public void HardBlendMatchesBooleanAlgebra() =>
        Spec.ForAll(Gens.Finite.Select(Gens.Finite, static (double a, double b) => (A: a, B: b)), p => {
            Assert.Equal(expected: Math.Min(val1: p.A, val2: p.B), actual: CsgKind.Union.Combine(left: p.A, right: p.B, blend: BlendKind.Hard));
            Assert.Equal(expected: Math.Max(val1: p.A, val2: p.B), actual: CsgKind.Intersect.Combine(left: p.A, right: p.B, blend: BlendKind.Hard));
            Assert.Equal(expected: Math.Max(val1: p.A, val2: -p.B), actual: CsgKind.Difference.Combine(left: p.A, right: p.B, blend: BlendKind.Hard));
            Assert.Equal(expected: CsgKind.Union.Combine(left: p.A, right: p.B, blend: BlendKind.Hard), actual: CsgKind.Union.Combine(left: p.B, right: p.A, blend: BlendKind.Hard));
            Assert.Equal(expected: CsgKind.Intersect.Combine(left: p.A, right: p.B, blend: BlendKind.Hard), actual: CsgKind.Intersect.Combine(left: p.B, right: p.A, blend: BlendKind.Hard));
        });
    [Fact]
    public void SymmetricOpsAreIdempotent() =>
        Spec.ForAll(Gens.Finite, x => {
            Assert.Equal(expected: x, actual: CsgKind.Union.Combine(left: x, right: x, blend: BlendKind.Hard));
            Assert.Equal(expected: x, actual: CsgKind.Intersect.Combine(left: x, right: x, blend: BlendKind.Hard));
        });
    [Fact]
    public void KeysAreDistinct() =>
        Spec.SmartEnumKeysUnique(items: [CsgKind.Union, CsgKind.Intersect, CsgKind.Difference], key: static k => k.Key);
}

public sealed class KernelKindLaws {
    [Fact]
    public void WeightsMatchSupportLaws() {
        Spec.ForAll(FieldGens.Kernel, k =>
            Spec.EqualWithin(left: k.Weight(distance: 0.0, radius: 1.0), right: 1.0, tolerance: 1.0e-12, what: "peak"));
        Spec.ForAll(FieldGens.Kernel.Select(Gens.Positive, static (KernelKind k, double r) => (Kind: k, R: r)), p =>
            Assert.Equal(expected: 0.0, actual: p.Kind.Weight(distance: p.R, radius: p.R)));
        Spec.ForAll(
            FieldGens.Kernel.Select(Gens.Positive, Gens.Probability, static (KernelKind k, double r, double t) => (Kind: k, R: r, T: t)),
            p => Assert.InRange(actual: p.Kind.Weight(distance: p.T * p.R, radius: p.R), low: 0.0, high: 1.0));
    }
    [Fact]
    public void KeysAreDistinct() {
        KernelKind[] all = [KernelKind.Wendland, KernelKind.Quintic, KernelKind.Cosine, KernelKind.Cubic, KernelKind.Linear, KernelKind.Epanechnikov];
        Spec.SmartEnumKeysUnique(items: all, key: static k => k.Key);
    }
}

public sealed class FalloffLaws {
    [Fact]
    public void ConstantWeightIsUnit() =>
        Spec.ForAll(Gens.Positive, d =>
            Spec.Succ(Falloff.Constant.Weight(distance: d, tolerance: 1.0e-12, key: FieldGens.Key),
                then: static w => Assert.Equal(expected: 1.0, actual: w)));
    [Fact]
    public void InverseAndSquareMatchClosedForm() =>
        Spec.ForAll(Gens.Positive, d => {
            Spec.Succ(Falloff.Inverse.Weight(distance: d, tolerance: 1.0e-15, key: FieldGens.Key),
                then: w => Spec.EqualWithin(left: w, right: 1.0 / d, tolerance: 1.0e-12, what: "inverse"));
            Spec.Succ(Falloff.InverseSquare.Weight(distance: d, tolerance: 1.0e-15, key: FieldGens.Key),
                then: w => Spec.EqualWithin(left: w, right: 1.0 / (d * d), tolerance: 1.0e-12, what: "inverse-square"));
        });
    [Fact]
    public void InverseFamilyFailsBelowTolerance() {
        Spec.Fail(Falloff.Inverse.Weight(distance: 1.0e-15, tolerance: 1.0e-10, key: FieldGens.Key));
        Spec.Fail(Falloff.InverseSquare.Weight(distance: 1.0e-15, tolerance: 1.0e-10, key: FieldGens.Key));
    }
    [Fact]
    public void FactoriesRejectNonPositive() =>
        Spec.ForAll(Gens.Finite.Where(static x => x <= 0.0), x => {
            Spec.Fail(Falloff.Gaussian(sigma: x, key: FieldGens.Key));
            Spec.Fail(Falloff.Kernel(kind: KernelKind.Wendland, radius: x, key: FieldGens.Key));
        });
}

public sealed class TerminationLaws {
    [Fact]
    public void StepsRejectsNonPositive() =>
        Spec.ForAll(Gen.Int[-1000, 0], n => Spec.Fail(Termination.Steps(count: n, key: FieldGens.Key)));
    [Fact]
    public void ArcLengthRejectsNonPositive() =>
        Spec.ForAll(Gens.Finite.Where(static x => x <= 0.0), l => Spec.Fail(Termination.ArcLength(length: l, key: FieldGens.Key)));
    [Fact]
    public void StepCountAndMagnitudeBoundariesFire() {
        Termination steps = Termination.Steps(count: 5, key: FieldGens.Key).Match(Succ: static t => t, Fail: static _ => throw new InvalidOperationException(message: "setup"));
        Termination arc = Termination.ArcLength(length: 2.0, key: FieldGens.Key).Match(Succ: static t => t, Fail: static _ => throw new InvalidOperationException(message: "setup"));
        Termination mag = Termination.Magnitude(threshold: 1.0, key: FieldGens.Key).Match(Succ: static t => t, Fail: static _ => throw new InvalidOperationException(message: "setup"));
        Assert.True(steps.ShouldStop(state: new(Trail: Seq<Point3d>(), Current: Point3d.Origin, H: 1.0, Arc: 0.0, Steps: 5, Rejects: 0, Done: false), currentSample: Vector3d.Zero, context: null!, key: FieldGens.Key).Match(Succ: static value => value, Fail: static _ => false));
        Assert.False(steps.ShouldStop(state: new(Trail: Seq<Point3d>(), Current: Point3d.Origin, H: 1.0, Arc: 0.0, Steps: 4, Rejects: 0, Done: false), currentSample: Vector3d.Zero, context: null!, key: FieldGens.Key).Match(Succ: static value => value, Fail: static _ => true));
        Assert.True(arc.ShouldStop(state: new(Trail: Seq<Point3d>(), Current: Point3d.Origin, H: 1.0, Arc: 2.0, Steps: 0, Rejects: 0, Done: false), currentSample: Vector3d.Zero, context: null!, key: FieldGens.Key).Match(Succ: static value => value, Fail: static _ => false));
        Assert.True(mag.ShouldStop(state: new(Trail: Seq<Point3d>(), Current: Point3d.Origin, H: 1.0, Arc: 0.0, Steps: 0, Rejects: 0, Done: false), currentSample: new Vector3d(x: 0.5, y: 0.0, z: 0.0), context: null!, key: FieldGens.Key).Match(Succ: static value => value, Fail: static _ => false));
        Assert.False(mag.ShouldStop(state: new(Trail: Seq<Point3d>(), Current: Point3d.Origin, H: 1.0, Arc: 0.0, Steps: 0, Rejects: 0, Done: false), currentSample: new Vector3d(x: 2.0, y: 0.0, z: 0.0), context: null!, key: FieldGens.Key).Match(Succ: static value => value, Fail: static _ => true));
    }
}

public sealed class IntegratorKindLaws {
    [Fact]
    public void TableauMetadataIsCoherent() {
        Spec.ForAll(FieldGens.Adaptive, k => Assert.True(k.IsAdaptive));
        Spec.ForAll(FieldGens.NonAdaptive, k => Assert.False(k.IsAdaptive));
        Spec.ForAll(FieldGens.Integrator, k => Assert.Equal(expected: k.Tableau.Weights.Count, actual: k.Order));
        Spec.ForAll(FieldGens.Integrator, k =>
            Spec.EqualWithin(left: FieldGens.Sum(xs: k.Tableau.Weights), right: 1.0, tolerance: 1.0e-10, what: "weights"));
        Spec.ForAll(FieldGens.Integrator, k => Assert.True(
            toSeq(k.Tableau.Coupling.AsIterable().Select((Seq<double> row, int i) => row.Count <= i)).ForAll(static b => b),
            userMessage: "Butcher coupling[i].Count must satisfy <= i"));
    }
}

public sealed class FieldIntegratorLaws {
    [Fact]
    public void AdaptiveRejectsFixedKind() =>
        Spec.ForAll(FieldGens.NonAdaptive, k => Spec.Fail(FieldIntegrator.Adaptive(kind: k, tolerance: 1.0e-6, key: FieldGens.Key)));
    [Fact]
    public void AdaptiveRejectsNegativeBudget() =>
        Spec.ForAll(Gen.Int[-100, -1], m => Spec.Fail(FieldIntegrator.Adaptive(kind: IntegratorKind.DormandPrince, tolerance: 1.0e-6, maxRejects: m, key: FieldGens.Key)));
    [Fact]
    public void FixedHasZeroRejectBudget() =>
        Assert.Equal(expected: 0, actual: FieldIntegrator.RK4.RejectBudget);
}

public sealed class VectorFieldAlgebraLaws {
    [Fact]
    public void BlendAndScaleCasesAreCanonical() {
        VectorField a = VectorField.Constant(value: Vector3d.XAxis);
        VectorField b = VectorField.Constant(value: Vector3d.YAxis);
        VectorField c = VectorField.Constant(value: Vector3d.ZAxis);
        VectorField leftFolded = a + b + c;
        VectorField rightFolded = a + (b + c);
        Assert.Equal(expected: 3, actual: ((VectorField.BlendCase)leftFolded).Fields.Count);
        Assert.Equal(expected: 3, actual: ((VectorField.BlendCase)rightFolded).Fields.Count);
        VectorField negated = -VectorField.Constant(value: Vector3d.XAxis);
        Assert.Equal(expected: -1.0, actual: ((VectorField.ScaledCase)negated).Scale);
        Spec.ForAll(Gens.Finite, k => {
            VectorField scaled = VectorField.Constant(value: Vector3d.XAxis) * k;
            Assert.Equal(expected: k, actual: ((VectorField.ScaledCase)scaled).Scale);
        });
    }
}
