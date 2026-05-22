using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// VectorCloud factories (Ring/Polyline/Cluster), CloudKernel pipelines, and VectorCloudShape
// invariants all route to the bridge rail because they invoke RhinoCommon native code
// (Polyline.IsValid, Vector3d.IsValid, AcceptValue<Point3d>, etc.). The static surface here is
// limited to VectorCloudMetric introspection: key uniqueness, Output-type integrity, and the
// per-case AdmitsCase predicate domain (testable without instantiating a real VectorCloud).
[System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Design", checkId: "CA1515", Justification = "xUnit discovers public test surface.")]
public static class CloudMetricGens {
    public static readonly VectorCloudMetric[] All = [
        VectorCloudMetric.Normal, VectorCloudMetric.Area, VectorCloudMetric.Perimeter, VectorCloudMetric.EdgeAspect,
        VectorCloudMetric.Skewness, VectorCloudMetric.Compactness, VectorCloudMetric.MomentAnisotropy,
        VectorCloudMetric.RadiiOfGyration, VectorCloudMetric.AreaError, VectorCloudMetric.CentroidError,
        VectorCloudMetric.Centroid, VectorCloudMetric.BestFitPlane, VectorCloudMetric.PrincipalAxes,
        VectorCloudMetric.PrincipalFrame, VectorCloudMetric.Shape, VectorCloudMetric.BishopFrames,
        VectorCloudMetric.TangentFlow, VectorCloudMetric.CumulativeArcLength, VectorCloudMetric.EdgeCurvatures,
        VectorCloudMetric.OpenLength, VectorCloudMetric.Covariance, VectorCloudMetric.PrincipalDirection, VectorCloudMetric.Spread,
    ];
    public static readonly Gen<VectorCloudMetric> Metric = Gen.OneOfConst(All);
    public static readonly Type[] RingFamily = [
        typeof(Vector3d), typeof(double), typeof(Point3d), typeof(Plane),
        typeof(Seq<Vector3d>), typeof(VectorCloudShape), typeof(Seq<Plane>),
        typeof(SymmetricMatrix), typeof(Seq<double>),
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
        Assert.Equal(expected: typeof(Seq<double>), actual: VectorCloudMetric.CumulativeArcLength.Output);
        Assert.Equal(expected: typeof(Seq<double>), actual: VectorCloudMetric.EdgeCurvatures.Output);
    }
}
