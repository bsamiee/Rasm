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
    public static readonly Seq<Point3d> Samples = Seq(Point3d.Origin, new Point3d(x: 1.0, y: 0.0, z: 0.0));
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class ExtractionProjectionLaws {
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
            then: vector => Spec.EqualWithin(left: vector.Length, right: 1.0, tolerance: 0.0, what: "vector probe"));
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
        VectorIntent grid = VectorIntent.SampleGrid(
            field: ScalarField.Constant(value: 4.0),
            domain: domain,
            policy: new GridPolicy(Samples: ExtractionGens.Samples));
        Spec.Succ(grid.Project<Seq<(Point3d Point, double Value)>>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: samples => Assert.Equal(expected: ExtractionGens.Samples.Count, actual: samples.Count));
        Spec.Succ(grid.Project<ExtractionReceipt>(context: ExtractionGens.Model, key: ExtractionGens.Key),
            then: receipt => Assert.Equal(expected: ExtractionGens.Samples.Count, actual: receipt.Emitted));
    }
    [Fact]
    public void IsoContourPolicyAdmitsOnlyNativeSurfaceDirections() {
        Spec.Succ(ContourPolicy.Iso(direction: 0, parameter: 0.5, key: ExtractionGens.Key));
        Spec.Succ(ContourPolicy.Iso(direction: 1, parameter: 0.5, key: ExtractionGens.Key));
        Spec.FailCategory(ContourPolicy.Iso(direction: 2, parameter: 0.5, key: ExtractionGens.Key), category: "Input");
        Spec.FailCategory(ContourPolicy.Iso(direction: 0, parameter: double.NaN, key: ExtractionGens.Key), category: "Input");
    }
}
