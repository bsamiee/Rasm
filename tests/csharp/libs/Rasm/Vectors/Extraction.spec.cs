using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;
using Dimension = Rasm.Vectors.Dimension;

namespace Rasm.Tests.Vectors;

// --- [MODELS] ----------------------------------------------------------------------------
internal static class ExtractionGens {
    public static readonly Op Key = Op.Of(name: "extraction-test");
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "extraction context");
    public static readonly Seq<Point3d> Samples = Gens.UnitSegment3;
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class ExtractionProjectionLaws {
    [Fact]
    public void DomainAdmissionRoutesManagedInputsThroughCanonicalOwners() {
        VectorCloud cloud = Spec.SuccValue(VectorCloud.Cluster(points: ExtractionGens.Samples, context: ExtractionGens.Model, key: ExtractionGens.Key), label: "domain cloud");
        Spec.Succ(ExtractionDomain.Of(value: cloud, context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: domain => Assert.IsType<ExtractionDomain.CloudCase>(@object: domain));
        Spec.Succ(ExtractionDomain.Of(value: Point3d.Origin, context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: domain => Assert.IsType<ExtractionDomain.SupportCase>(@object: domain));
        Spec.FailCategory(ExtractionDomain.Of(value: null, context: ExtractionGens.Model, key: ExtractionGens.Key), category: "Input");
        Spec.FailCategory(ExtractionDomain.Cloud(value: null!, key: ExtractionGens.Key), category: "Input");
        SampleKind samples = Spec.SuccValue(SampleKind.Explicit(points: ExtractionGens.Samples, key: ExtractionGens.Key), label: "explicit samples");
        Spec.Succ(GridPolicy.Of(kind: samples, key: ExtractionGens.Key));
        Spec.FailCategory(GridPolicy.Of(kind: null!, key: ExtractionGens.Key), category: "Input");
    }
    [Fact]
    public void ProbeKeepsFieldProjectionOnIntentRail() {
        SymmetricMatrix tensor = Spec.SuccValue(SymmetricMatrix.Of(dim: Dimension.Create(value: 2), upper: [1.0, 0.25, 2.0], key: ExtractionGens.Key), label: "tensor");
        Spec.Succ(
            Spec.SuccValue(VectorIntent.Probe(source: ExtractionProbe.Scalar(source: ScalarField.Constant(value: 3.0)), sample: Point3d.Origin, key: ExtractionGens.Key), label: "scalar probe")
                .Project<double>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: value => Spec.Equal(left: value, right: 3.0, tolerance: 0.0, what: "scalar probe"));
        Spec.Succ(
            Spec.SuccValue(VectorIntent.Probe(source: ExtractionProbe.Vector(source: VectorField.Constant(value: Vector3d.XAxis)), sample: Point3d.Origin, key: ExtractionGens.Key), label: "vector probe")
                .Project<Vector3d>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: vector => Spec.Equal(left: vector, right: Vector3d.XAxis, tolerance: 0.0));
        Spec.Succ(
            Spec.SuccValue(VectorIntent.Probe(source: ExtractionProbe.Tensor(source: TensorField.Constant(value: tensor)), sample: Point3d.Origin, key: ExtractionGens.Key), label: "tensor probe")
                .Project<SymmetricMatrix>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: value => Assert.Equal(expected: tensor, actual: value));
        Spec.FailCategory(
            Spec.SuccValue(VectorIntent.Probe(source: ExtractionProbe.Tensor(source: TensorField.Constant(value: tensor)), sample: Point3d.Origin, key: ExtractionGens.Key), label: "unsupported tensor probe")
                .Project<double>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            category: "Unsupported");
    }
    [Fact]
    public void GridsReturnComputedReceipts() {
        ExtractionDomain domain = Spec.SuccValue(ExtractionDomain.Cloud(value: Spec.SuccValue(VectorCloud.Cluster(points: ExtractionGens.Samples, context: ExtractionGens.Model, key: ExtractionGens.Key), label: "domain cloud"), key: ExtractionGens.Key), label: "extraction domain");
        SampleKind samples = Spec.SuccValue(SampleKind.Explicit(points: ExtractionGens.Samples, key: ExtractionGens.Key), label: "explicit samples");
        VectorIntent grid = Spec.SuccValue(VectorIntent.SampleGrid(
            field: ScalarField.Constant(value: 4.0),
            domain: domain,
            policy: Spec.SuccValue(GridPolicy.Of(kind: samples, key: ExtractionGens.Key), label: "grid policy"),
            key: ExtractionGens.Key), label: "grid intent");
        Spec.Succ(grid.Project<Seq<(Point3d Point, double Value)>>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: samples => {
                Assert.Equal(expected: ExtractionGens.Samples.Count, actual: samples.Count);
                _ = samples.Zip(ExtractionGens.Samples).Iter(pair => {
                    Spec.Equal(left: pair.First.Point, right: pair.Second, tolerance: 0.0);
                    Spec.Equal(left: pair.First.Value, right: 4.0, tolerance: 0.0, what: "constant grid value");
                });
            });
        Spec.Succ(grid.Project<ExtractionReceipt>(context: ExtractionGens.Model, key: ExtractionGens.Key), then: receipt => {
            Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "grid receipt");
            Assert.Equal(expected: ExtractionStatus.Complete, actual: receipt.Status);
            Assert.Equal(expected: ExtractionGens.Samples.Count, actual: receipt.Attempted);
            Assert.Equal(expected: ExtractionGens.Samples.Count, actual: receipt.Emitted);
            Assert.Equal(expected: 0, actual: receipt.Rejected);
            Assert.False(condition: receipt.NativeRouted);
            Spec.Some(receipt.Sample, sample => {
                Assert.Equal(expected: ExtractionGens.Samples.Count, actual: sample.Attempted);
                Assert.Equal(expected: ExtractionGens.Samples.Count, actual: sample.Emitted);
            });
        });
        Spec.FailCategory(grid.Project<Point3d>(context: ExtractionGens.Model, key: ExtractionGens.Key), category: "Unsupported");
    }
    [Fact]
    public void IsoContourPolicyAdmitsOnlyNativeSurfaceStatuses() {
        Spec.SmartEnumKeysUnique(items: [ExtractionStatus.Complete, ExtractionStatus.Approximate], key: static status => status.Key);
        Spec.Succ(ContourPolicy.SurfaceIso(status: IsoStatus.X, parameter: 0.5, key: ExtractionGens.Key));
        Spec.Succ(ContourPolicy.SurfaceIso(status: IsoStatus.Y, parameter: 0.5, key: ExtractionGens.Key));
        Spec.Succ(ContourPolicy.SurfaceIso(status: IsoStatus.North, parameter: 0.5, key: ExtractionGens.Key));
        Spec.Succ(ContourPolicy.SurfaceIso(status: IsoStatus.East, parameter: 0.5, key: ExtractionGens.Key));
        Spec.Succ(ContourPolicy.SurfaceIso(status: IsoStatus.South, parameter: 0.5, key: ExtractionGens.Key));
        Spec.Succ(ContourPolicy.SurfaceIso(status: IsoStatus.West, parameter: 0.5, key: ExtractionGens.Key));
        Spec.Succ(ContourPolicy.SurfaceIso(status: IsoStatus.North, parameter: double.NaN, key: ExtractionGens.Key));
        Spec.FailCategory(ContourPolicy.SurfaceIso(status: IsoStatus.None, parameter: 0.5, key: ExtractionGens.Key), category: "Input");
        Spec.FailCategory(ContourPolicy.SurfaceIso(status: IsoStatus.X, parameter: double.NaN, key: ExtractionGens.Key), category: "Input");
        Spec.FailCategory(new ContourPolicy.MeshScalarCase(Values: [double.NaN], Levels: Seq(0.0)).Admit(key: ExtractionGens.Key), category: "Input");
        Spec.FailCategory(new ContourPolicy.AxisCase(Start: Point3d.Origin, End: Point3d.Origin, Interval: Spec.SuccValue(ExtractionGens.Key.AcceptValidated<PositiveMagnitude>(candidate: 1.0), label: "axis interval")).Admit(key: ExtractionGens.Key), category: "Input");
    }
}
