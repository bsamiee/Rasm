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
    public static readonly BoundingBox Bounds = new(min: new Point3d(x: -1.0, y: -1.0, z: -1.0), max: new Point3d(x: 1.0, y: 1.0, z: 1.0));
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
                then: r => Spec.Equal(left: r, right: FieldGens.Sum(xs: xs), tolerance: 1.0e-9, what: "sum"));
            Spec.Succ(FieldBlend.Average.CombineScalar(values: xs, key: FieldGens.Key),
                then: r => Spec.Equal(left: r, right: FieldGens.Sum(xs: xs) / xs.Count, tolerance: 1.0e-9, what: "mean"));
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
            Spec.Equal(left: k.Weight(distance: 0.0, radius: 1.0), right: 1.0, tolerance: 1.0e-12, what: "peak"));
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
                then: w => Spec.Equal(left: w, right: 1.0 / d, tolerance: 1.0e-12, what: "inverse"));
            Spec.Succ(Falloff.InverseSquare.Weight(distance: d, tolerance: 1.0e-15, key: FieldGens.Key),
                then: w => Spec.Equal(left: w, right: 1.0 / (d * d), tolerance: 1.0e-12, what: "inverse-square"));
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
        Spec.Succ(ExtractionProbe.Scalar(source: constant).Project<double>(sample: new Point3d(x: 1.0, y: 2.0, z: 3.0), context: FieldGens.Model, key: FieldGens.Key),
            then: value => Spec.Equal(left: value, right: 7.0, tolerance: 0.0, what: "constant"));
        Spec.Fail(ExtractionProbe.Scalar(source: constant).Project<Vector3d>(sample: Point3d.Origin, context: FieldGens.Model, key: FieldGens.Key));
        Spec.Succ(FieldNabla.GradientAt(field: constant, point: Point3d.Origin, eps: 0.01, context: FieldGens.Model, key: FieldGens.Key),
            then: grad => Spec.Equal(left: grad.Length, right: 0.0, tolerance: 1.0e-12, what: "constant gradient"));
    }
    [Fact]
    public void SdfPrimitiveLipschitzAndRequiredKeysAreGated() {
        ImmutableDictionary<string, double> sphere = ImmutableDictionary<string, double>.Empty.Add(key: "r", value: 1.0);
        ImmutableDictionary<string, double> missing = [];
        ScalarField primitive = new ScalarField.PrimitiveCase(Kind: SdfKind.Sphere, Parameters: sphere, Pose: default);
        Spec.Some(primitive.LipschitzBound(), lip => Spec.Equal(left: lip, right: SdfKind.Sphere.Lipschitz, tolerance: 0.0, what: "sphere lip"));
        Assert.True(condition: SdfKind.Sphere.ValidateParameters(parameters: sphere));
        Assert.False(condition: SdfKind.Sphere.ValidateParameters(parameters: missing));
        Spec.Fail(ScalarField.Primitive(kind: SdfKind.Sphere, parameters: missing, pose: Plane.WorldXY, key: FieldGens.Key));
    }
}

public sealed class FieldReconstructionAndSdfLaws {
    [Fact]
    public void MlsReconstructsOrientedPlaneAndReportsQueryFacts() {
        Seq<MlsSample> plane = Seq(
            new MlsSample(Position: new Point3d(x: -1.0, y: -1.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0),
            new MlsSample(Position: new Point3d(x: 1.0, y: -1.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0),
            new MlsSample(Position: new Point3d(x: -1.0, y: 1.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0),
            new MlsSample(Position: new Point3d(x: 1.0, y: 1.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0));
        ReconstructionResult result = Spec.SuccValue(ScalarField.MlsDetailed(samples: plane, kernel: KernelKind.Wendland, radius: 3.0, context: FieldGens.Model, key: FieldGens.Key), label: "mls");
        Spec.None(result.Receipt.Solve);
        Spec.Succ(result.Field.SampleReconstructionDetailed(sample: new Point3d(x: 0.2, y: -0.25, z: 0.5), context: FieldGens.Model, key: FieldGens.Key), then: sample => {
            Spec.Equal(left: sample.Value, right: 0.5, tolerance: 1.0e-12, what: "mls plane value");
            Assert.Equal(expected: ReconstructionMode.MovingLeastSquares, actual: sample.Receipt.Mode);
            Assert.Equal(expected: ReconstructionStatus.ApproximateSdf, actual: sample.Receipt.Status);
            Assert.Equal(expected: plane.Count, actual: sample.Receipt.NeighborhoodCount);
            Assert.True(condition: sample.Receipt.Rank >= 2);
            Assert.True(condition: sample.Receipt.WeightSum > 0.0);
            Spec.Equal(left: sample.Receipt.GradientNorm, right: 1.0, tolerance: 1.0e-12, what: "mls gradient norm");
        });
        Spec.Succ(ExtractionProbe.Scalar(source: result.Field).Project<double>(sample: new Point3d(x: 0.0, y: 0.0, z: -0.25), context: FieldGens.Model, key: FieldGens.Key),
            then: value => Spec.Equal(left: value, right: -0.25, tolerance: 1.0e-12, what: "mls scalar rail"));
        Spec.Fail(result.Field.SampleReconstructionDetailed(sample: new Point3d(x: 10.0, y: 10.0, z: 10.0), context: FieldGens.Model, key: FieldGens.Key));
    }
    [Fact]
    public void MlsRejectsInvalidAndUnsupportedNeighborhoods() {
        Seq<MlsSample> empty = Seq<MlsSample>();
        Seq<MlsSample> zeroNormal = Seq(new MlsSample(Position: Point3d.Origin, Normal: Vector3d.Zero, Value: 0.0));
        Seq<MlsSample> nonfinite = Seq(new MlsSample(Position: new Point3d(x: double.NaN, y: 0.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0));
        Seq<MlsSample> sparse = Seq(
            new MlsSample(Position: new Point3d(x: -1.0, y: 0.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0),
            new MlsSample(Position: new Point3d(x: 1.0, y: 0.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0));
        Seq<Seq<MlsSample>> invalid = Seq(empty, zeroNormal, nonfinite);
        _ = invalid.Iter(samples => Spec.Fail(ScalarField.MlsDetailed(samples: samples, kernel: KernelKind.Wendland, radius: 1.0, context: FieldGens.Model, key: FieldGens.Key)));
        Spec.Fail(ScalarField.MlsDetailed(samples: sparse, kernel: KernelKind.Wendland, radius: 0.0, context: FieldGens.Model, key: FieldGens.Key));
        ReconstructionResult unsupported = Spec.SuccValue(ScalarField.MlsDetailed(samples: sparse, kernel: KernelKind.Wendland, radius: 3.0, context: FieldGens.Model, key: FieldGens.Key), label: "unsupported mls factory");
        Spec.Fail(unsupported.Field.SampleReconstructionDetailed(sample: Point3d.Origin, context: FieldGens.Model, key: FieldGens.Key));
    }
    [Fact]
    public void ProfileExtrusionRejectsMissingContextBeforeNativeSampling() =>
        Spec.FailCategory(ScalarField.ProfileExtrusion(profile: null!, plane: default, halfHeight: 1.0, context: null!, key: FieldGens.Key), category: "Operation");
    [Fact]
    public void IsoSurfaceReceiptCarriesNativeThreadingAndToleranceFacts() {
        BoundingBox bounds = new(min: new Point3d(x: -2.0, y: -1.0, z: -0.5), max: new Point3d(x: 2.0, y: 1.0, z: 0.5));
        IsoSurfaceGrid grid = new(Bounds: bounds, Resolution: 4, XCells: 16, YCells: 8, ZCells: 4, CellSize: 0.25, HexCellCount: 512, CornerSampleCount: 765, CenterSampleCount: 512, InitialSampleCount: 1277);
        IsoSurfaceReceipt receipt = new(
            NativeRouted: true, Status: IsoSurfaceStatus.NativeValid, Grid: grid, MaxRootSteps: 16, ParallelCallback: true, EvaluatorFailures: 0, Valid: true, VertexCount: 4, FaceCount: 4, FixedTolerance: Some(0.001), FixedNormalSampleDistance: Some(1.0e-5), MeshPreflight: Option<SdfMeshReceipt>.None);
        Spec.Succ(ExtractionReceipt.Of(status: ExtractionStatus.Complete, attempted: 1, emitted: 1, nativeRouted: receipt.NativeRouted, toleranceSource: ToleranceSource.RhinoDefault, tolerance: receipt.FixedTolerance, parallelCallback: receipt.ParallelCallback, key: FieldGens.Key, isoSurface: Some(receipt)), then: extraction => {
            Assert.True(condition: extraction.ParallelCallback);
            Assert.Equal(expected: IsoSurfaceStatus.NativeValid, actual: receipt.Status);
            Assert.Equal(expected: 512, actual: receipt.Grid.HexCellCount);
            Assert.Equal(expected: 1277, actual: receipt.Grid.InitialSampleCount);
            Spec.Some(extraction.Tolerance, tolerance => Spec.Equal(left: tolerance, right: 0.001, tolerance: 0.0, what: "iso fixed tolerance"));
            Spec.Some(receipt.FixedNormalSampleDistance, tolerance => Spec.Equal(left: tolerance, right: 1.0e-5, tolerance: 0.0, what: "iso normal sampling"));
            Spec.Some(extraction.IsoSurface, iso => Assert.True(condition: iso.ParallelCallback && iso.FixedTolerance.IsSome && iso.FixedNormalSampleDistance.IsSome));
        });
    }
    [Fact]
    public void IsoSurfaceAdmissionRejectsNonfiniteScalarPayloadBeforeNativeMarching() =>
        Spec.FailCategory(ScalarField.Constant(value: double.NaN).IsoSurfaceDetailed(bounds: FieldGens.Bounds, resolution: 8, maxRootSteps: 16, context: FieldGens.Model, key: FieldGens.Key), category: "Input");
    [Fact]
    public void IsoSurfaceAdmissionRejectsNonfiniteAnalyticCentersAndStrengthBeforeNativeMarching() {
        Seq<ScalarField> invalid = Seq(
            Spec.SuccValue(ScalarField.Density(center: new Point3d(x: double.NaN, y: 0.0, z: 0.0), spread: 1.0, strength: 1.0, key: FieldGens.Key), label: "density center"),
            Spec.SuccValue(ScalarField.Density(center: Point3d.Origin, spread: 1.0, strength: double.NaN, key: FieldGens.Key), label: "density strength"),
            Spec.SuccValue(ScalarField.Morse(center: new Point3d(x: 0.0, y: double.NaN, z: 0.0), depth: 1.0, width: 1.0, key: FieldGens.Key), label: "morse center"),
            Spec.SuccValue(ScalarField.Mollifier(center: new Point3d(x: 0.0, y: 0.0, z: double.NaN), radius: 1.0, key: FieldGens.Key), label: "mollifier center"));
        _ = invalid.Iter(field => Spec.FailCategory(field.IsoSurfaceDetailed(bounds: FieldGens.Bounds, resolution: 8, maxRootSteps: 16, context: FieldGens.Model, key: FieldGens.Key), category: "Input"));
    }
    [Fact]
    public void IsoSurfaceAdmissionRecursesThroughVectorBackedScalarPayloads() {
        VectorField invalid = VectorField.Constant(value: new Vector3d(x: double.NaN, y: 0.0, z: 0.0));
        Seq<ScalarField> fields = Seq(
            ScalarField.Magnitude(source: invalid),
            Spec.SuccValue(ScalarField.Divergence(source: invalid, epsilon: 0.1, key: FieldGens.Key), label: "divergence"),
            Spec.SuccValue(ScalarField.StrainMagnitude(source: invalid, epsilon: 0.1, key: FieldGens.Key), label: "strain"));
        _ = fields.Iter(field => Spec.FailCategory(field.IsoSurfaceDetailed(bounds: FieldGens.Bounds, resolution: 8, maxRootSteps: 16, context: FieldGens.Model, key: FieldGens.Key), category: "Input"));
    }
    [Fact]
    public void IsoSurfaceAdmissionRejectsNonfiniteChargeAndFalloffPayloads() {
        SymmetricMatrix invalidMetric = new(Dimension: Dim.Create(value: 3), Upper: new Arr<double>([double.NaN, 0.0, 0.0, 1.0, 0.0, 1.0]));
        Falloff invalidFalloff = Spec.SuccValue(Falloff.AnisotropicKernel(kind: KernelKind.Wendland, metric: TensorField.Constant(value: invalidMetric), radius: 2.0, key: FieldGens.Key), label: "anisotropic falloff");
        Seq<ScalarField> fields = Seq(
            new ScalarField.PotentialCase(Charges: Seq((new Point3d(x: double.NaN, y: 0.0, z: 0.0), 1.0)), Falloff: Falloff.Constant),
            new ScalarField.PotentialCase(Charges: Seq((Point3d.Origin, double.NaN)), Falloff: Falloff.Constant),
            new ScalarField.PotentialCase(Charges: Seq((Point3d.Origin, 1.0)), Falloff: invalidFalloff),
            ScalarField.Magnitude(source: new VectorField.CoulombCase(Charges: Seq((Point3d.Origin, double.NaN)), Falloff: Falloff.Constant)));
        _ = fields.Iter(field => Spec.FailCategory(field.IsoSurfaceDetailed(bounds: FieldGens.Bounds, resolution: 8, maxRootSteps: 16, context: FieldGens.Model, key: FieldGens.Key), category: "Input"));
    }
    [Fact]
    public void ReconstructionAdmissionKeepsSolveFactsAndRejectsTamperedPayloads() {
        Seq<(Point3d Position, double Value)> samples = Seq(
            (new Point3d(x: -1.0, y: 0.0, z: 0.0), -1.0),
            (Point3d.Origin, 0.0),
            (new Point3d(x: 1.0, y: 0.0, z: 0.0), 1.0));
        ReconstructionResult rbf = Spec.SuccValue(ScalarField.RbfDetailed(samples: samples, kernel: KernelKind.Wendland, radius: 3.0, key: FieldGens.Key), label: "rbf");
        ReconstructionResult approximate = Spec.SuccValue(ScalarField.RbfDetailed(samples: samples, kernel: KernelKind.Wendland, radius: 3.0, smoothing: 0.5, key: FieldGens.Key), label: "rbf approximation");
        Spec.Some(rbf.Receipt.Solve, solve => Assert.Equal(expected: samples.Count, actual: solve.Solution.Count));
        ScalarField.RbfCase rbfCase = Assert.IsType<ScalarField.RbfCase>(@object: rbf.Field);
        ScalarField.RbfCase approximateCase = Assert.IsType<ScalarField.RbfCase>(@object: approximate.Field);
        ScalarField tamperedRbf = new ScalarField.RbfCase(Samples: rbfCase.Samples, Kernel: rbfCase.Kernel, Radius: rbfCase.Radius, Coefficients: new Arr<double>([1.0]), Receipt: rbfCase.Receipt);
        ScalarField tamperedApproximation = new ScalarField.RbfCase(Samples: approximateCase.Samples, Kernel: approximateCase.Kernel, Radius: approximateCase.Radius, Coefficients: approximateCase.Coefficients, Receipt: approximateCase.Receipt with { Solve = rbfCase.Receipt.Solve });
        Spec.FailCategory(tamperedRbf.IsoSurfaceDetailed(bounds: FieldGens.Bounds, resolution: 8, maxRootSteps: 16, context: FieldGens.Model, key: FieldGens.Key), category: "Result");
        Spec.FailCategory(tamperedApproximation.IsoSurfaceDetailed(bounds: FieldGens.Bounds, resolution: 8, maxRootSteps: 16, context: FieldGens.Model, key: FieldGens.Key), category: "Result");
        Seq<MlsSample> plane = Seq(
            new MlsSample(Position: new Point3d(x: -1.0, y: -1.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0),
            new MlsSample(Position: new Point3d(x: 1.0, y: -1.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0),
            new MlsSample(Position: new Point3d(x: -1.0, y: 1.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0),
            new MlsSample(Position: new Point3d(x: 1.0, y: 1.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0));
        ReconstructionResult mls = Spec.SuccValue(ScalarField.MlsDetailed(samples: plane, kernel: KernelKind.Wendland, radius: 3.0, context: FieldGens.Model, key: FieldGens.Key), label: "mls");
        ScalarField.MlsCase mlsCase = Assert.IsType<ScalarField.MlsCase>(@object: mls.Field);
        ScalarField tamperedMls = new ScalarField.MlsCase(
            Samples: Seq(new MlsSample(Position: new Point3d(x: double.NaN, y: 0.0, z: 0.0), Normal: Vector3d.ZAxis, Value: 0.0)),
            Kernel: mlsCase.Kernel, Radius: mlsCase.Radius, Receipt: mlsCase.Receipt);
        Spec.FailCategory(tamperedMls.IsoSurfaceDetailed(bounds: FieldGens.Bounds, resolution: 8, maxRootSteps: 16, context: FieldGens.Model, key: FieldGens.Key), category: "Input");
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
            Spec.Succ(ExtractionProbe.Vector(source: scaled).Project<Vector3d>(sample: Point3d.Origin, context: FieldGens.Model, key: FieldGens.Key),
                then: vector => Spec.Equal(left: vector, right: k * Vector3d.XAxis, tolerance: 1.0e-9));
        });
    }
    [Fact]
    public void BlendProjectionMatchesIndependentVectorFold() {
        VectorField x = VectorField.Constant(value: Vector3d.XAxis);
        VectorField y = VectorField.Constant(value: Vector3d.YAxis);
        Spec.Succ(ExtractionProbe.Vector(source: VectorField.Blend(fields: Seq(x, y), blend: FieldBlend.Sum)).Project<Vector3d>(sample: Point3d.Origin, context: FieldGens.Model, key: FieldGens.Key),
            then: vector => Spec.Equal(left: vector, right: Vector3d.XAxis + Vector3d.YAxis, tolerance: 1.0e-12));
        Spec.Succ(ExtractionProbe.Vector(source: VectorField.Blend(fields: Seq(x, y), blend: FieldBlend.Average)).Project<Vector3d>(sample: Point3d.Origin, context: FieldGens.Model, key: FieldGens.Key),
            then: vector => Spec.Equal(left: vector, right: 0.5 * (Vector3d.XAxis + Vector3d.YAxis), tolerance: 1.0e-12));
    }
}
