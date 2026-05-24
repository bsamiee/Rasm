using System.Collections.Immutable;
using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class FieldGens {
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "field context");
    public static readonly Op Key = Op.Of(name: "field-test");
    public static readonly Gen<CsgKind> Csg = Gen.OneOfConst(CsgKind.Union, CsgKind.Intersect, CsgKind.Difference);
    public static readonly Gen<KernelKind> Kernel = Gen.OneOfConst(KernelKind.Wendland, KernelKind.Quintic, KernelKind.Cosine, KernelKind.Cubic, KernelKind.Linear, KernelKind.Epanechnikov);
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

public sealed class BlendKindLaws {
    [Fact]
    public void SmoothFactoriesGatePositiveParametersAndNeverReduceBounds() {
        Spec.ForAll(Gens.Positive, k => {
            Spec.Succ(BlendKind.Polynomial(k: k, key: FieldGens.Key), then: b => Assert.True(condition: b.Erode(leftLip: 2.0, rightLip: 1.0) >= 2.0));
            Spec.Succ(BlendKind.Exponential(k: k, key: FieldGens.Key), then: b => Assert.True(condition: b.Erode(leftLip: 2.0, rightLip: 1.0) >= 2.0));
            Spec.Succ(BlendKind.Round(r: k, key: FieldGens.Key), then: b => Assert.True(condition: b.Erode(leftLip: 2.0, rightLip: 1.0) >= 2.0));
            Spec.Succ(BlendKind.Groove(k: k, d: k, key: FieldGens.Key), then: b => Assert.True(condition: b.Erode(leftLip: 2.0, rightLip: 1.0) >= 2.0));
        });
        Spec.ForAll(Gens.NonPositive, k => {
            Spec.Fail(BlendKind.Polynomial(k: k, key: FieldGens.Key));
            Spec.Fail(BlendKind.Groove(k: 1.0, d: k, key: FieldGens.Key));
        });
    }
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
            Spec.Fail(Falloff.Gaussian(spread: x, key: FieldGens.Key));
            Spec.Fail(Falloff.Kernel(kind: KernelKind.Wendland, radius: x, key: FieldGens.Key));
        });
}

public sealed class FieldPolicyAndSamplingLaws {
    [Fact]
    public void RayAndBouncePoliciesGatePositiveParameters() {
        Spec.Succ(RayPolicy.Segment(length: 2.0, sense: BoundarySense.Away, key: FieldGens.Key), then: p => {
            RayPolicy.SegmentCase segment = Assert.IsType<RayPolicy.SegmentCase>(@object: p);
            Assert.Equal(expected: BoundarySense.Away, actual: segment.Sense);
        });
        Spec.Fail(RayPolicy.Segment(length: 0.0, key: FieldGens.Key));
        Spec.Succ(BouncePolicy.Refract(etaIncident: 1.0, etaTransmitted: 1.5, key: FieldGens.Key), then: p => Assert.IsType<BouncePolicy.RefractCase>(@object: p));
        Spec.Fail(BouncePolicy.Refract(etaIncident: 0.0, etaTransmitted: 1.5, key: FieldGens.Key));
    }
    [Fact]
    public void ScalarConstantProjectAndNablaGradientAreStable() {
        ScalarField constant = ScalarField.Constant(value: 7.0);
        Spec.Succ(constant.Project<double>(sample: new Point3d(x: 1.0, y: 2.0, z: 3.0), context: FieldGens.Model, key: FieldGens.Key),
            then: value => Spec.EqualWithin(left: value, right: 7.0, tolerance: 0.0, what: "constant"));
        Spec.Fail(constant.Project<Vector3d>(sample: Point3d.Origin, context: FieldGens.Model, key: FieldGens.Key));
        Spec.Succ(FieldNabla.GradientAt(field: constant, point: Point3d.Origin, eps: 0.01, context: FieldGens.Model, key: FieldGens.Key),
            then: grad => Spec.EqualWithin(left: grad.Length, right: 0.0, tolerance: 1.0e-12, what: "constant gradient"));
    }
    [Fact]
    public void SdfPrimitiveLipschitzAndRequiredKeysAreGated() {
        ImmutableDictionary<string, double> sphere = ImmutableDictionary<string, double>.Empty.Add(key: "r", value: 1.0);
        ImmutableDictionary<string, double> missing = [];
        ScalarField primitive = new ScalarField.PrimitiveCase(Kind: SdfKind.Sphere, Parameters: sphere, Pose: default);
        Spec.Some(primitive.LipschitzBound(), lip => Spec.EqualWithin(left: lip, right: SdfKind.Sphere.Lipschitz, tolerance: 0.0, what: "sphere lip"));
        Assert.True(condition: SdfKind.Sphere.ValidateParameters(parameters: sphere));
        Assert.False(condition: SdfKind.Sphere.ValidateParameters(parameters: missing));
    }
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
