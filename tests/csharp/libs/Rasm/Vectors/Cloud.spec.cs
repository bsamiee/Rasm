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
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters).ToFin(), label: "cloud context");
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
    public static readonly Seq<Point3d> Triangle = Gens.UnitTriangle3;
    public static readonly Gen<Seq<double>> RawMass3 = Gens.Positive.Array[3].Select(static values => toSeq(values));
    public static readonly Gen<Seq<double>> Simplex3 = Gens.Simplex(count: 3);
    public static readonly Gen<Seq<double>> ZeroMass3 = Gens.Positive.Select(Gen.Int[start: 0, finish: 2], static (double mass, int zero) =>
        toSeq(Enumerable.Range(start: 0, count: 3).Select(i => i == zero ? 0.0 : mass + i + 1.0)));
    public static readonly Type[] OutputFamily = [
        typeof(Vector3d), typeof(double), typeof(Point3d), typeof(Plane),
        typeof(Seq<Vector3d>), typeof(VectorCloudShape), typeof(Seq<Plane>),
        typeof(SymmetricMatrix), typeof(Seq<double>), typeof(Seq<(double K1, double K2, Direction E1, Direction E2)>),
    ];
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class VectorCloudMetricLaws {
    [Fact]
    public void MetadataCasesAreDistinctAndUseKnownOutputs() =>
        Spec.Cases(items: CloudMetricGens.All, key: static metric => metric.Key, law: static metric =>
            Assert.Contains(expected: metric.Output, collection: CloudMetricGens.OutputFamily));
    [Fact]
    public void OutputFamiliesAreDeclaredByMetricRole() {
        Seq<(VectorCloudMetric Metric, Type Output)> cases = Seq(
            (VectorCloudMetric.Shape, typeof(VectorCloudShape)),
            (VectorCloudMetric.Covariance, typeof(SymmetricMatrix)),
            (VectorCloudMetric.Area, typeof(double)),
            (VectorCloudMetric.Perimeter, typeof(double)),
            (VectorCloudMetric.OpenLength, typeof(double)),
            (VectorCloudMetric.BestFitPlane, typeof(Plane)),
            (VectorCloudMetric.PrincipalFrame, typeof(Plane)),
            (VectorCloudMetric.PrincipalAxes, typeof(Seq<Vector3d>)),
            (VectorCloudMetric.BishopFrames, typeof(Seq<Plane>)),
            (VectorCloudMetric.TangentFlow, typeof(Seq<Vector3d>)),
            (VectorCloudMetric.OrientedNormals, typeof(Seq<Vector3d>)),
            (VectorCloudMetric.CumulativeArcLength, typeof(Seq<double>)),
            (VectorCloudMetric.EdgeCurvatures, typeof(Seq<double>)),
            (VectorCloudMetric.Curvedness, typeof(Seq<double>)),
            (VectorCloudMetric.ShapeIndex, typeof(Seq<double>)));
        _ = cases.Iter(static c => Assert.Equal(expected: c.Output, actual: c.Metric.Output));
    }
    [Fact]
    public void ClusterMetricsAdmitProjectAndRejectUnsupportedOutputs() {
        VectorCloud cluster = Spec.SuccValue(VectorCloud.Cluster(points: CloudMetricGens.Triangle, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "cluster");
        Assert.True(VectorCloudMetric.Covariance.AdmitsCase(cloud: cluster));
        Assert.False(VectorCloudMetric.Area.AdmitsCase(cloud: cluster));
        Spec.Succ(VectorCloudMetric.Covariance.Project<SymmetricMatrix>(cloud: cluster, key: Op.Of(name: "cloud-test")),
            then: cov => {
                Assert.Equal(expected: 3, actual: cov.Dimension.Value);
                Spec.SeqEqualWithin(left: toSeq(cov.Upper.AsIterable()), right: toSeq(Numeric.CovarianceUpper(points: CloudMetricGens.Triangle).AsIterable()), tolerance: 1.0e-12, what: "covariance");
            });
        Spec.Succ(VectorCloudMetric.Centroid.Project<Point3d>(cloud: cluster, key: Op.Of(name: "cloud-test")),
            then: centroid => Spec.NearEqual(left: centroid, right: Numeric.Centroid(points: CloudMetricGens.Triangle), tolerance: 1.0e-12));
        Spec.Fail(VectorCloudMetric.Covariance.Project<double>(cloud: cluster, key: Op.Of(name: "cloud-test")));
        Spec.Fail(VectorCloudMetric.Area.Project<double>(cloud: cluster, key: Op.Of(name: "cloud-test")));
        Assert.Equal(expected: typeof(Seq<(double K1, double K2, Direction E1, Direction E2)>), actual: VectorCloudMetric.PrincipalCurvature.Output);
    }
}

public sealed class VectorCloudMassLaws {
    [Fact]
    public void WeightedClusterNormalizesMassRail() =>
        Spec.ForAll(CloudMetricGens.RawMass3, mass => Spec.Succ(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: mass, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), then: cloud => {
            VectorCloud.ClusterCase cluster = Assert.IsType<VectorCloud.ClusterCase>(@object: cloud);
            Spec.Some(cluster.Mass, weights => {
                double total = Numeric.Sum(values: mass);
                Spec.EqualWithin(left: Numeric.Sum(values: toSeq(weights.AsIterable())), right: 1.0, tolerance: 1.0e-12, what: "mass sum");
                Spec.SeqEqualWithin(left: toSeq(mass.AsIterable().Select(value => value / total)), right: toSeq(weights.AsIterable()), tolerance: 1.0e-12, what: "mass");
            });
        }));
    [Fact]
    public void WeightedClusterRejectsBadMassRails() {
        Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(1.0, 2.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input");
        Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(1.0, -1.0, 1.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input");
        Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(0.0, 0.0, 0.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input");
        Spec.ForAll(CloudMetricGens.ZeroMass3, mass =>
            Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: mass, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input"));
        Spec.ForAll(Gens.NonFinite, x =>
            Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(1.0, x, 1.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input"));
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
            then: distance => Spec.EqualWithin(left: distance, right: 1.0, tolerance: 1.0e-12, what: "one-point OT distance"));
        Spec.FailCategory(CloudKernel.Sinkhorn<double>(source: source, target: target, regularization: 0.0, maxIterations: 32, debiased: false, massRelaxation: Option<PositiveMagnitude>.None, key: Op.Of(name: "cloud-test")), category: "Input");
        Spec.FailCategory(CloudKernel.Sinkhorn<double>(source: source, target: target, regularization: 1.0, maxIterations: 0, debiased: false, massRelaxation: Option<PositiveMagnitude>.None, key: Op.Of(name: "cloud-test")), category: "Input");
        Spec.FailCategory(CloudKernel.Sinkhorn<Point3d>(source: source, target: target, regularization: 1.0, maxIterations: 32, debiased: false, massRelaxation: Option<PositiveMagnitude>.None, key: Op.Of(name: "cloud-test")), category: "Unsupported");
    }
}
