using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Ring/polyline shape metrics route to the bridge rail because they invoke RhinoCommon native
// code. The static surface owns metric metadata, pure cluster admission/projection, mass
// normalization, and managed Sinkhorn receipt rails.
internal static class CloudMetricGens {
    public static readonly Context Model = Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters)
        .Match(Succ: static value => value, Fail: static _ => throw new InvalidOperationException(message: "context"));
    public static readonly VectorCloudMetric[] All = [
        VectorCloudMetric.Normal, VectorCloudMetric.Area, VectorCloudMetric.Perimeter, VectorCloudMetric.EdgeAspect,
        VectorCloudMetric.Skewness, VectorCloudMetric.Compactness, VectorCloudMetric.MomentAnisotropy,
        VectorCloudMetric.RadiiOfGyration, VectorCloudMetric.AreaError, VectorCloudMetric.CentroidError,
        VectorCloudMetric.Centroid, VectorCloudMetric.BestFitPlane, VectorCloudMetric.PrincipalAxes,
        VectorCloudMetric.PrincipalFrame, VectorCloudMetric.Shape, VectorCloudMetric.BishopFrames,
        VectorCloudMetric.TangentFlow, VectorCloudMetric.CumulativeArcLength, VectorCloudMetric.EdgeCurvatures,
        VectorCloudMetric.OpenLength, VectorCloudMetric.Covariance, VectorCloudMetric.PrincipalDirection, VectorCloudMetric.Spread,
        VectorCloudMetric.OrientedNormals, VectorCloudMetric.PrincipalCurvature, VectorCloudMetric.Curvedness, VectorCloudMetric.ShapeIndex,
    ];
    public static readonly Gen<VectorCloudMetric> Metric = Gen.OneOfConst(All);
    public static readonly Seq<Point3d> Triangle = Seq(new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 0.0, y: 1.0, z: 0.0));
    public static readonly Gen<Seq<double>> RawMass3 = Gens.Positive.Array[3].Select(static values => toSeq(values));
    public static readonly Gen<Seq<double>> Simplex3 = Gens.Simplex(count: 3);
    public static readonly Type[] RingFamily = [
        typeof(Vector3d), typeof(double), typeof(Point3d), typeof(Plane),
        typeof(Seq<Vector3d>), typeof(VectorCloudShape), typeof(Seq<Plane>),
        typeof(SymmetricMatrix), typeof(Seq<double>), typeof(Seq<(double K1, double K2, Direction E1, Direction E2)>),
    ];
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class VectorCloudMetricLaws {
    [Fact]
    public void KeysAreDistinctAcrossAllCases() =>
        Assert.Equal(
            expected: CloudMetricGens.All.Length,
            actual: CloudMetricGens.All.Select(static (VectorCloudMetric m) => m.Key).Distinct().Count());
    [Fact]
    public void OutputTypeIsRecognisedKind() =>
        Spec.ForAll(CloudMetricGens.Metric, m =>
            Assert.Contains(expected: m.Output, collection: CloudMetricGens.RingFamily));
    [Fact]
    public void ShapeMetricHasShapeOutput() =>
        Assert.Equal(expected: typeof(VectorCloudShape), actual: VectorCloudMetric.Shape.Output);
    [Fact]
    public void CovarianceMetricHasSymmetricMatrixOutput() =>
        Assert.Equal(expected: typeof(SymmetricMatrix), actual: VectorCloudMetric.Covariance.Output);
    [Fact]
    public void TopologicalMetricsHaveScalarOrPlanarOutput() {
        Assert.Equal(expected: typeof(double), actual: VectorCloudMetric.Area.Output);
        Assert.Equal(expected: typeof(double), actual: VectorCloudMetric.Perimeter.Output);
        Assert.Equal(expected: typeof(double), actual: VectorCloudMetric.OpenLength.Output);
        Assert.Equal(expected: typeof(Plane), actual: VectorCloudMetric.BestFitPlane.Output);
        Assert.Equal(expected: typeof(Plane), actual: VectorCloudMetric.PrincipalFrame.Output);
    }
    [Fact]
    public void SequenceMetricsHaveSeqOutput() {
        Assert.Equal(expected: typeof(Seq<Vector3d>), actual: VectorCloudMetric.PrincipalAxes.Output);
        Assert.Equal(expected: typeof(Seq<Plane>), actual: VectorCloudMetric.BishopFrames.Output);
        Assert.Equal(expected: typeof(Seq<Vector3d>), actual: VectorCloudMetric.TangentFlow.Output);
        Assert.Equal(expected: typeof(Seq<Vector3d>), actual: VectorCloudMetric.OrientedNormals.Output);
        Assert.Equal(expected: typeof(Seq<double>), actual: VectorCloudMetric.CumulativeArcLength.Output);
        Assert.Equal(expected: typeof(Seq<double>), actual: VectorCloudMetric.EdgeCurvatures.Output);
        Assert.Equal(expected: typeof(Seq<double>), actual: VectorCloudMetric.Curvedness.Output);
        Assert.Equal(expected: typeof(Seq<double>), actual: VectorCloudMetric.ShapeIndex.Output);
    }
    [Fact]
    public void ClusterMetricsAdmitProjectAndRejectUnsupportedOutputs() {
        VectorCloud cluster = Spec.SuccValue(VectorCloud.Cluster(points: CloudMetricGens.Triangle, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "cluster");
        Assert.True(VectorCloudMetric.Covariance.AdmitsCase(cloud: cluster));
        Assert.False(VectorCloudMetric.Area.AdmitsCase(cloud: cluster));
        Spec.Succ(VectorCloudMetric.Covariance.Project<SymmetricMatrix>(cloud: cluster, key: Op.Of(name: "cloud-test")),
            then: cov => Assert.Equal(expected: 3, actual: cov.Dimension.Value));
        Spec.Fail(VectorCloudMetric.Covariance.Project<double>(cloud: cluster, key: Op.Of(name: "cloud-test")));
        Spec.Fail(VectorCloudMetric.Area.Project<double>(cloud: cluster, key: Op.Of(name: "cloud-test")));
    }
}

public sealed class VectorCloudMassLaws {
    [Fact]
    public void WeightedClusterNormalizesMassRail() =>
        Spec.ForAll(CloudMetricGens.RawMass3, mass => Spec.Succ(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: mass, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), then: cloud => {
            VectorCloud.ClusterCase cluster = Assert.IsType<VectorCloud.ClusterCase>(@object: cloud);
            Spec.Some(cluster.Mass, weights => {
                double total = mass.Fold(initialState: 0.0, f: static (sum, value) => sum + value);
                Spec.EqualWithin(left: weights.Fold(initialState: 0.0, f: static (sum, value) => sum + value), right: 1.0, tolerance: 1.0e-12, what: "mass sum");
                Spec.SeqEqualWithin(left: toSeq(mass.AsIterable().Select(value => value / total)), right: toSeq(weights.AsIterable()), tolerance: 1.0e-12, what: "mass");
            });
        }));
    [Fact]
    public void WeightedClusterRejectsBadMassRails() {
        Spec.Fail(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(1.0, 2.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")));
        Spec.Fail(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(1.0, -1.0, 1.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")));
        Spec.Fail(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(0.0, 0.0, 0.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")));
        Spec.ForAll(Gens.NonFinite, x =>
            Spec.Fail(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(1.0, x, 1.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test"))));
    }
    [Fact]
    public void AlreadyNormalMassRailStaysProportional() =>
        Spec.ForAll(CloudMetricGens.Simplex3, mass => Spec.Succ(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: mass, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), then: cloud =>
            Spec.Some(Assert.IsType<VectorCloud.ClusterCase>(@object: cloud).Mass, weights =>
                Spec.SeqEqualWithin(left: mass, right: toSeq(weights.AsIterable()), tolerance: 1.0e-12, what: "simplex mass"))));
    [Fact]
    public void SinkhornReceiptsUseTypedStopSemantics() {
        VectorCloud source = Spec.SuccValue(VectorCloud.WeightedCluster(points: Seq(CloudMetricGens.Triangle[0]), mass: Seq(2.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "source");
        VectorCloud target = Spec.SuccValue(VectorCloud.WeightedCluster(points: Seq(CloudMetricGens.Triangle[1]), mass: Seq(5.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "target");
        Assert.True(PositiveMagnitude.TryCreate(value: 2.0, obj: out PositiveMagnitude relaxation));
        Spec.Succ(CloudKernel.Sinkhorn<SinkhornReceipt>(source: source, target: target, regularization: 1.0, maxIterations: 32, debiased: false, massRelaxation: Option<PositiveMagnitude>.None, key: Op.Of(name: "cloud-test")), then: receipt =>
            Assert.Equal(expected: SinkhornStopKind.BalancedMarginalsConverged, actual: receipt.Stop));
        Spec.Succ(CloudKernel.Sinkhorn<SinkhornReceipt>(source: source, target: target, regularization: 1.0, maxIterations: 32, debiased: false, massRelaxation: Some(relaxation), key: Op.Of(name: "cloud-test")), then: receipt =>
            Assert.Equal(expected: SinkhornStopKind.RelaxedScalingConverged, actual: receipt.Stop));
        Spec.Succ(CloudKernel.Sinkhorn<double>(source: source, target: target, regularization: 1.0, maxIterations: 32, debiased: false, massRelaxation: Option<PositiveMagnitude>.None, key: Op.Of(name: "cloud-test")),
            then: distance => Assert.True(condition: distance >= 0.0));
        Spec.Fail(CloudKernel.Sinkhorn<double>(source: source, target: target, regularization: 0.0, maxIterations: 32, debiased: false, massRelaxation: Option<PositiveMagnitude>.None, key: Op.Of(name: "cloud-test")));
        Spec.Fail(CloudKernel.Sinkhorn<double>(source: source, target: target, regularization: 1.0, maxIterations: 0, debiased: false, massRelaxation: Option<PositiveMagnitude>.None, key: Op.Of(name: "cloud-test")));
        Spec.Fail(CloudKernel.Sinkhorn<Point3d>(source: source, target: target, regularization: 1.0, maxIterations: 32, debiased: false, massRelaxation: Option<PositiveMagnitude>.None, key: Op.Of(name: "cloud-test")));
    }
}
