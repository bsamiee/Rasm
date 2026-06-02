using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using Dimension = Rasm.Vectors.Dimension;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class AlignGens {
    public static readonly Op Key = Op.Of(name: "align-test");
    public static readonly Seq<Point3d> Tetra = Seq(
        Point3d.Origin,
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.0, y: 0.0, z: 1.0));
    public static readonly Seq<Point3d> Six = Seq(
        Point3d.Origin,
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0),
        new Point3d(x: 1.0, y: 1.0, z: 0.0),
        new Point3d(x: 2.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 2.0, z: 0.0));
    public static readonly Seq<Point3d> Cube = Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0),
        new Point3d(x: 1.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.0, y: 0.0, z: 1.0),
        new Point3d(x: 1.0, y: 0.0, z: 1.0),
        new Point3d(x: 0.0, y: 1.0, z: 1.0),
        new Point3d(x: 1.0, y: 1.0, z: 1.0));
    public static readonly AlignKind[] Kinds = [
        AlignKind.Point, AlignKind.Plane, AlignKind.Symmetric, AlignKind.Robust, AlignKind.NormalWeightedPointToPlane, AlignKind.Generalized,
    ];
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters).ToFin(), label: "align context");
    public static readonly Vector3d Offset = new(x: 0.02, y: -0.03, z: 0.05);
    public static AlignmentPolicy Policy(int maxIterations = 8, double tolerance = 1.0e-6) =>
        Spec.SuccValue(
            from iterations in Key.AcceptValidated<Dimension>(candidate: maxIterations)
            from convergence in Key.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
            from robust in Key.AcceptValidated<PositiveMagnitude>(candidate: 0.1)
            select new AlignmentPolicy(MaxIterations: iterations, ConvergenceTolerance: convergence, RobustScale: robust),
            label: "alignment policy");
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class AlignKindLaws {
    [Fact]
    public void KeysAreDistinctAndNonClusterInputsFail() {
        Spec.SmartEnumKeysUnique(items: AlignGens.Kinds, key: static k => k.Key);
        Assert.Equal(expected: AlignmentApproximationStatus.SymmetricNormalSumLinearized, actual: AlignKind.Symmetric.Approximation);
        Assert.Equal(expected: AlignmentApproximationStatus.GeneralizedIterativeClosestPoint, actual: AlignKind.Generalized.Approximation);
        Assert.Equal(expected: AlignGens.Kinds.Length, actual: AlignKind.Items.Count);
        VectorCloud polyline = Spec.SuccValue(VectorCloud.Polyline(points: AlignGens.Tetra, context: AlignGens.Model, key: AlignGens.Key), label: "polyline");
        VectorCloud cluster = Spec.SuccValue(VectorCloud.Cluster(points: AlignGens.Tetra, context: AlignGens.Model, key: AlignGens.Key), label: "cluster");
        Spec.ForAll(Gen.OneOfConst(AlignGens.Kinds), kind =>
            Spec.FailCategory(kind.AlignDetailed(source: polyline, target: cluster, key: AlignGens.Key), category: "Input"));
    }
}

public sealed class AlignSolverLaws {
    [Fact]
    public void PointToPlaneGuardsLinearizedRailCountsBeforeNativeNormals() {
        Point3d[] target = [.. AlignGens.Six.AsIterable().Select(static p => p + Vector3d.ZAxis)];
        Vector3d[] normals = [.. Enumerable.Repeat(element: Vector3d.ZAxis, count: AlignGens.Six.Count)];
        double[] rowMass = [.. Enumerable.Repeat(element: 1.0 / AlignGens.Six.Count, count: AlignGens.Six.Count)];
        Spec.FailCategory(AlignKernel.SolvePointToPlane(source: AlignGens.Six, target: target[..^1], normals: normals, rowMass: rowMass, current: Transform.Identity, key: AlignGens.Key), category: "Input");
        Spec.FailCategory(AlignKernel.SolvePointToPlane(source: AlignGens.Six, target: target, normals: normals[..^1], rowMass: rowMass, current: Transform.Identity, key: AlignGens.Key), category: "Input");
    }
    [Fact]
    public void PointToPlaneQrMigrationRecoversPlanarZTranslation() {
        Point3d[] target = [.. AlignGens.Six.AsIterable().Select(static p => p + Vector3d.ZAxis)];
        Vector3d[] normals = [
            new Vector3d(x: 2.0, y: 2.0, z: 0.0),
            new Vector3d(x: -2.0, y: 0.0, z: -1.0),
            new Vector3d(x: -1.0, y: 2.0, z: 2.0),
            new Vector3d(x: 0.0, y: 2.0, z: 0.0),
            new Vector3d(x: 2.0, y: -1.0, z: -1.0),
            new Vector3d(x: 2.0, y: -1.0, z: 1.0),
        ];
        double[] rowMass = [.. Enumerable.Repeat(element: 1.0 / AlignGens.Six.Count, count: AlignGens.Six.Count)];
        Spec.Succ(AlignKernel.SolvePointToPlane(source: AlignGens.Six, target: target, normals: normals, rowMass: rowMass, current: Transform.Identity, key: AlignGens.Key), then: step => {
            Spec.Equal(left: step.Delta[0, 3], right: 0.0, tolerance: 1.0e-10, what: "x translation");
            Spec.Equal(left: step.Delta[1, 3], right: 0.0, tolerance: 1.0e-10, what: "y translation");
            Spec.Equal(left: step.Delta[2, 3], right: 1.0, tolerance: 1.0e-10, what: "z translation");
            Assert.True(condition: step.Solve.IsSome);
        });
    }
    [Fact]
    public void RobustSolverRejectsMismatchedResidualRails() {
        Point3d[] target = [.. AlignGens.Tetra.AsIterable()];
        double[] residuals = [.. Enumerable.Repeat(element: 0.0, count: AlignGens.Tetra.Count)];
        double[] rowMass = [.. Enumerable.Repeat(element: 1.0 / AlignGens.Tetra.Count, count: AlignGens.Tetra.Count)];
        Spec.FailCategory(AlignKernel.SolveRobustProcrustes(source: AlignGens.Tetra, target: target[..^1], residuals: residuals, rowMass: rowMass, current: Transform.Identity, key: AlignGens.Key), category: "Input");
        Spec.FailCategory(AlignKernel.SolveRobustProcrustes(source: AlignGens.Tetra, target: target, residuals: residuals[..^1], rowMass: rowMass, current: Transform.Identity, key: AlignGens.Key), category: "Input");
        Spec.FailCategory(AlignKernel.SolveRobustProcrustes(source: AlignGens.Tetra, target: target, residuals: residuals, rowMass: rowMass, current: Transform.Identity, robustScale: 0.0, key: AlignGens.Key), category: "Input");
    }
}
