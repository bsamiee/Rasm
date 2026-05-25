using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class IntentGens {
    public static readonly Op Key = Op.Of(name: "intent-test");
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "intent context");
    public static readonly Gen<double> OutsideUnit = Gens.Finite.Where(static t => t is < 0.0 or > 1.0);
    public static readonly Seq<Point3d> ClusterPoints = Gens.UnitTriangle3;
    public static VectorCloud Cluster => Spec.SuccValue(VectorCloud.Cluster(points: ClusterPoints, context: Model, key: Key), label: "cluster");
    public static ExtractionDomain CloudDomain => Spec.SuccValue(ExtractionDomain.Cloud(value: Cluster, key: Key), label: "intent cloud domain");
    public static SampleKind ExplicitSamples => Spec.SuccValue(SampleKind.Explicit(points: ClusterPoints, key: Key), label: "intent explicit samples");
    public static PositiveMagnitude Step => Spec.SuccValue(Key.AcceptValidated<PositiveMagnitude>(candidate: 0.1), label: "step");
    public static Termination Stop => Spec.SuccValue(Termination.Steps(count: 1, key: Key), label: "stop");
    public static Direction X => default;
    public static Direction Y => default;
    public static MeshSpace Space => default;
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class VectorIntentFactoryLaws {
    [Fact]
    public void InterpolationFactoriesValidateUnitInterval() {
        Spec.ForAll(Gens.UnitClosed, t => {
            Spec.Succ(VectorIntent.Lerp(a: Vector3d.XAxis, b: Vector3d.YAxis, t: t, key: IntentGens.Key),
                then: intent => Spec.EqualWithin(left: Assert.IsType<VectorIntent.LerpCase>(@object: intent).Parameter.Value, right: t, tolerance: 0.0, what: "lerp t"));
            Spec.Succ(VectorIntent.Pose(from: Plane.WorldXY, to: Plane.WorldZX, t: t, mode: MotionInterpolation.Linear, key: IntentGens.Key),
                then: intent => Spec.EqualWithin(left: Assert.IsType<VectorIntent.PoseCase>(@object: intent).Parameter.Value, right: t, tolerance: 0.0, what: "pose t"));
            Spec.Succ(VectorIntent.Slerp(a: IntentGens.X, b: IntentGens.Y, t: t, key: IntentGens.Key),
                then: intent => Spec.EqualWithin(left: Assert.IsType<VectorIntent.SlerpCase>(@object: intent).Parameter.Value, right: t, tolerance: 0.0, what: "slerp t"));
        });
        Spec.ForAll(IntentGens.OutsideUnit, t => {
            Spec.Fail(VectorIntent.Lerp(a: Vector3d.XAxis, b: Vector3d.YAxis, t: t, key: IntentGens.Key));
            Spec.Fail(VectorIntent.Pose(from: Plane.WorldXY, to: Plane.WorldZX, t: t, mode: MotionInterpolation.Linear, key: IntentGens.Key));
            Spec.Fail(VectorIntent.Slerp(a: IntentGens.X, b: IntentGens.Y, t: t, key: IntentGens.Key));
        });
    }
    [Fact]
    public void CloudTransportFeatureAndDescriptorFactoriesGateInvalidInputs() {
        Spec.Succ(VectorIntent.Cloud(cloud: IntentGens.Cluster, metric: VectorCloudMetric.Covariance, key: IntentGens.Key), then: intent => Assert.IsType<VectorIntent.CloudCase>(@object: intent));
        Spec.FailCategory(VectorIntent.Cloud(cloud: IntentGens.Cluster, metric: VectorCloudMetric.Area, key: IntentGens.Key), category: "Unsupported");
        Spec.FailCategory(VectorIntent.Winding(cloud: IntentGens.Cluster, query: Point3d.Origin, key: IntentGens.Key), category: "Unsupported");
        Spec.Succ(VectorIntent.Transport(source: IntentGens.Cluster, target: IntentGens.Cluster, regularization: 1.0, maxIterations: 32, massRelaxation: 2.0, key: IntentGens.Key),
            then: intent => Assert.IsType<VectorIntent.TransportCase>(@object: intent));
        Spec.FailCategory(VectorIntent.Transport(source: IntentGens.Cluster, target: IntentGens.Cluster, regularization: 0.0, maxIterations: 32, key: IntentGens.Key), category: "Tolerance");
        Spec.FailCategory(VectorIntent.Transport(source: IntentGens.Cluster, target: IntentGens.Cluster, regularization: 1.0, maxIterations: 0, key: IntentGens.Key), category: "Tolerance");
        Spec.FailCategory(VectorIntent.Features(space: IntentGens.Space, dihedralRadians: 0.1, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.Features(space: IntentGens.Space, dihedralRadians: 0.0, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.Descriptor(space: IntentGens.Space, kind: MeshDescriptor.Spectral(filter: SpectralFilter.Identity), pairs: 4, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.Descriptor(space: IntentGens.Space, kind: MeshDescriptor.Spectral(filter: null!), pairs: 4, key: IntentGens.Key), category: "Input");
    }
}

public sealed class VectorIntentShapeLaws {
    [Fact]
    public void DirectConstructorsPreserveCasePayloads() {
        _ = Assert.IsType<VectorIntent.ComponentsCase>(@object: VectorIntent.Components(anchor: Point3d.Origin, value: Vector3d.XAxis, frame: Plane.WorldXY));
        _ = Assert.IsType<VectorIntent.ProjectOntoCase>(@object: VectorIntent.ProjectOnto(value: Vector3d.ZAxis, target: Plane.WorldXY));
        _ = Assert.IsType<VectorIntent.MirrorCase>(@object: VectorIntent.Mirror(value: Vector3d.XAxis, across: Plane.WorldYZ));
        _ = Assert.IsType<VectorIntent.RayCase>(@object: VectorIntent.Ray(origin: Point3d.Origin, direction: IntentGens.X));
        _ = Assert.IsType<VectorIntent.StreamlineCase>(@object: Spec.SuccValue(VectorIntent.Streamline(field: VectorField.Constant(value: Vector3d.XAxis), seed: Point3d.Origin, initialStep: 0.1, termination: IntentGens.Stop, key: IntentGens.Key), label: "streamline"));
        _ = Assert.IsType<VectorIntent.ExtractionCase>(@object: Spec.SuccValue(VectorIntent.Probe(source: ExtractionProbe.Vector(source: VectorField.Constant(value: Vector3d.XAxis)), sample: Point3d.Origin, key: IntentGens.Key), label: "vector probe"));
        _ = Assert.IsType<VectorIntent.ExtractionCase>(@object: Spec.SuccValue(VectorIntent.Probe(source: ExtractionProbe.Scalar(source: ScalarField.Constant(value: 1.0)), sample: Point3d.Origin, key: IntentGens.Key), label: "scalar probe"));
        _ = Assert.IsType<VectorIntent.ExtractionCase>(@object: Spec.SuccValue(VectorIntent.Contour(
            domain: IntentGens.CloudDomain,
            policy: Spec.SuccValue(ContourPolicy.Plane(section: Plane.WorldXY, key: IntentGens.Key), label: "contour plane"),
            key: IntentGens.Key), label: "contour intent"));
        _ = Assert.IsType<VectorIntent.ExtractionCase>(@object: Spec.SuccValue(VectorIntent.Glyph(
            field: VectorField.Constant(value: Vector3d.XAxis),
            domain: IntentGens.CloudDomain,
            policy: Spec.SuccValue(GlyphPolicy.Of(kind: IntentGens.ExplicitSamples, scale: IntentGens.Step, key: IntentGens.Key), label: "glyph policy"),
            key: IntentGens.Key), label: "glyph intent"));
        _ = Assert.IsType<VectorIntent.ExtractionCase>(@object: Spec.SuccValue(VectorIntent.SampleGrid(
            field: ScalarField.Constant(value: 1.0),
            domain: IntentGens.CloudDomain,
            policy: Spec.SuccValue(GridPolicy.Of(kind: IntentGens.ExplicitSamples, key: IntentGens.Key), label: "grid policy"),
            key: IntentGens.Key), label: "grid intent"));
        _ = Assert.IsType<VectorIntent.ExtractionCase>(@object: Spec.SuccValue(VectorIntent.StreamBundle(
            field: VectorField.Constant(value: Vector3d.XAxis),
            domain: IntentGens.CloudDomain,
            policy: Spec.SuccValue(StreamBundlePolicy.Of(kind: IntentGens.ExplicitSamples, initialStep: IntentGens.Step, integrator: Spec.SuccValue(FieldIntegrator.Fixed(kind: IntegratorKind.RK4, key: IntentGens.Key), label: "fixed rk4"), termination: IntentGens.Stop, key: IntentGens.Key), label: "bundle policy"),
            key: IntentGens.Key), label: "bundle intent"));
    }
    [Fact]
    public void ProjectRequiresContextBeforeDispatch() {
        Spec.FailCategory(VectorIntent.Direction(value: Vector3d.XAxis).Project<Vector3d>(context: null!, key: IntentGens.Key), category: "Operation");
        Spec.FailCategory(VectorIntent.Lerp(a: Vector3d.XAxis, b: Vector3d.YAxis, t: double.NaN, key: IntentGens.Key), category: "Tolerance");
        Spec.FailCategory(VectorIntent.Transport(source: IntentGens.Cluster, target: IntentGens.Cluster, regularization: double.NaN, maxIterations: 16, key: IntentGens.Key), category: "Tolerance");
        Spec.FailCategory(VectorIntent.Transport(source: null!, target: IntentGens.Cluster, regularization: 1.0, maxIterations: 16, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.Transport(source: IntentGens.Cluster, target: null!, regularization: 1.0, maxIterations: 16, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.Streamline(field: null!, seed: Point3d.Origin, initialStep: 0.1, termination: IntentGens.Stop, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.Streamline(field: VectorField.Constant(value: Vector3d.XAxis), seed: Point3d.Origin, initialStep: 0.1, termination: null!, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.IsoSurface(field: null!, bounds: new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0)), resolution: 2, maxRootSteps: 1, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.IsoSurface(field: ScalarField.Constant(value: 0.0), bounds: BoundingBox.Empty, resolution: 1, maxRootSteps: 0, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.Contour(domain: null!, policy: Spec.SuccValue(ContourPolicy.Plane(section: Plane.WorldXY, key: IntentGens.Key), label: "contour policy"), key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.Sample(domain: IntentGens.CloudDomain, kind: null!, key: IntentGens.Key), category: "Input");
        Spec.FailCategory(VectorIntent.SampleGrid(field: ScalarField.Constant(value: 1.0), domain: IntentGens.CloudDomain, policy: default, key: IntentGens.Key), category: "Input");
    }
    [Fact]
    public void DispatchRailPreservesSupportedOutputAndRejectsForeignOutput() {
        VectorIntent sample = Spec.SuccValue(VectorIntent.Sample(domain: IntentGens.CloudDomain, kind: IntentGens.ExplicitSamples, key: IntentGens.Key), label: "sample intent");
        Spec.Succ(sample.Project<SampleReceipt>(context: IntentGens.Model, key: IntentGens.Key), then: receipt =>
            Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "intent sample"));
        Spec.Succ(sample.Project<VectorCloud>(context: IntentGens.Model, key: IntentGens.Key), then: cloud =>
            Assert.Equal(expected: IntentGens.ClusterPoints.Count, actual: Assert.IsType<VectorCloud.ClusterCase>(@object: cloud).Vertices.Count));
        Spec.FailCategory(sample.Project<SymmetricMatrix>(context: IntentGens.Model, key: IntentGens.Key), category: "Unsupported");
        VectorIntent cloudMetric = Spec.SuccValue(VectorIntent.Cloud(cloud: IntentGens.Cluster, metric: VectorCloudMetric.Covariance, key: IntentGens.Key), label: "metric intent");
        Spec.Succ(cloudMetric.Project<SymmetricMatrix>(context: IntentGens.Model, key: IntentGens.Key), then: matrix =>
            Spec.SeqEqualWithin(left: toSeq(matrix.Upper.AsIterable()), right: toSeq(Numeric.CovarianceUpper(points: IntentGens.ClusterPoints).AsIterable()), tolerance: 1.0e-12, what: "intent covariance"));
        Spec.FailCategory(cloudMetric.Project<Point3d>(context: IntentGens.Model, key: IntentGens.Key), category: "Unsupported");
    }
}
