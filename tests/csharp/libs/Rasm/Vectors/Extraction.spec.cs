using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;
using Dimension = Rasm.Vectors.Dimension;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class ExtractionGens {
    public static readonly Op Key = Op.Of(name: "extraction-test");
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "extraction context");
    public static readonly Seq<Point3d> Samples = Gens.UnitSegment3;
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class ExtractionProjectionLaws {
    [Fact]
    public void DomainAdmissionRoutesManagedInputsThroughCanonicalOwners() {
        VectorCloud cloud = Spec.SuccValue(VectorCloud.Cluster(points: ExtractionGens.Samples, context: ExtractionGens.Model, key: ExtractionGens.Key), label: "domain cloud");
        Spec.Succ(ExtractionDomain.Of(value: cloud, context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: domain => Assert.IsType<ExtractionDomain.CloudCase>(@object: domain));
        Spec.Succ(ExtractionDomain.Of(value: Point3d.Origin, context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: domain => Assert.IsType<ExtractionDomain.SupportCase>(@object: domain));
        Spec.FailCategory(ExtractionDomain.Of(value: null, context: ExtractionGens.Model, key: ExtractionGens.Key), category: "Input");
        Spec.FailCategory(ContourPolicy.Plane(section: Plane.Unset, key: ExtractionGens.Key), category: "Input");
        Spec.Succ(ContourPolicy.Plane(section: Plane.WorldXY, key: ExtractionGens.Key));
    }
    [Fact]
    public void ProbeKeepsFieldProjectionOnIntentRail() {
        SymmetricMatrix tensor = Spec.SuccValue(SymmetricMatrix.Of(dim: Dimension.Create(value: 2), upper: [1.0, 0.25, 2.0], key: ExtractionGens.Key), label: "tensor");
        Spec.Succ(
            VectorIntent.Probe(source: ExtractionProbe.Scalar(source: ScalarField.Constant(value: 3.0)), sample: Point3d.Origin)
                .Project<double>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: value => Spec.EqualWithin(left: value, right: 3.0, tolerance: 0.0, what: "scalar probe"));
        Spec.Succ(
            VectorIntent.Probe(source: ExtractionProbe.Vector(source: VectorField.Constant(value: Vector3d.XAxis)), sample: Point3d.Origin)
                .Project<Vector3d>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: vector => Spec.NearEqual(left: vector, right: Vector3d.XAxis, tolerance: 0.0));
        Spec.Succ(
            VectorIntent.Probe(source: ExtractionProbe.Tensor(source: TensorField.Constant(value: tensor)), sample: Point3d.Origin)
                .Project<SymmetricMatrix>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: value => Assert.Equal(expected: tensor, actual: value));
        Spec.FailCategory(
            VectorIntent.Probe(source: ExtractionProbe.Tensor(source: TensorField.Constant(value: tensor)), sample: Point3d.Origin)
                .Project<double>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            category: "Unsupported");
    }
    [Fact]
    public void GridsReturnComputedReceipts() {
        ExtractionDomain domain = ExtractionDomain.Cloud(value: Spec.SuccValue(VectorCloud.Cluster(points: ExtractionGens.Samples, context: ExtractionGens.Model, key: ExtractionGens.Key), label: "domain cloud"));
        SampleKind samples = Spec.SuccValue(SampleKind.Explicit(points: ExtractionGens.Samples, key: ExtractionGens.Key), label: "explicit samples");
        VectorIntent grid = VectorIntent.SampleGrid(
            field: ScalarField.Constant(value: 4.0),
            domain: domain,
            policy: new GridPolicy(Kind: samples));
        Spec.Succ(grid.Project<Seq<(Point3d Point, double Value)>>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: samples => {
                Assert.Equal(expected: ExtractionGens.Samples.Count, actual: samples.Count);
                _ = samples.Zip(ExtractionGens.Samples).Iter(pair => {
                    Spec.NearEqual(left: pair.Item1.Point, right: pair.Item2, tolerance: 0.0);
                    Spec.EqualWithin(left: pair.Item1.Value, right: 4.0, tolerance: 0.0, what: "constant grid value");
                });
            });
        Spec.Succ(grid.Project<ExtractionReceipt>(context: ExtractionGens.Model, key: ExtractionGens.Key), then: receipt => {
            Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "grid receipt");
            Assert.Equal(expected: ExtractionStatus.Complete, actual: receipt.Status);
            Assert.Equal(expected: ExtractionGens.Samples.Count, actual: receipt.Attempted);
            Assert.Equal(expected: ExtractionGens.Samples.Count, actual: receipt.Emitted);
            Assert.Equal(expected: 0, actual: receipt.Rejected);
            Assert.False(condition: receipt.NativeRouted);
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
    }
}
