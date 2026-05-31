using Rasm.TestKit;
using Rhino.Geometry;
using Xunit.Sdk;

namespace Rasm.Tests.TestKit;

// --- [LAWS] ---------------------------------------------------------------------------------
public sealed class NumericLaws {
    [Fact]
    public void CentroidEqualsClosedFormAverageForAsymmetricTriangle() {
        Seq<Point3d> points = Seq(new Point3d(x: 2.0, y: 3.0, z: 5.0), new Point3d(x: 7.0, y: 11.0, z: 13.0), new Point3d(x: 17.0, y: 19.0, z: 23.0));
        Spec.Equal(left: Numeric.Centroid(points: points), right: new Point3d(x: 26.0 / 3.0, y: 11.0, z: 41.0 / 3.0), tolerance: 1.0e-12);
    }

    [Fact]
    public void CovarianceUpperMatchesIndependentWeightedMomentRows() {
        Seq<Point3d> points = Seq(new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 2.0, y: 0.0, z: 0.0), new Point3d(x: 0.0, y: 4.0, z: 0.0));
        Seq<double> weights = Seq(0.2, 0.3, 0.5);
        Arr<double> actual = Numeric.CovarianceUpper(points: points, weights: Some(weights));
        Point3d mean = new(x: 0.6, y: 2.0, z: 0.0);
        double Moment(Func<Point3d, double> left, Func<Point3d, double> right) =>
            Enumerable.Range(start: 0, count: points.Count).Sum(i => weights[index: i] * (left(points[index: i]) - left(mean)) * (right(points[index: i]) - right(mean)));
        Spec.Equal(left: actual, right: new Arr<double>([
            Moment(static p => p.X, static p => p.X),
            Moment(static p => p.X, static p => p.Y),
            Moment(static p => p.X, static p => p.Z),
            Moment(static p => p.Y, static p => p.Y),
            Moment(static p => p.Y, static p => p.Z),
            Moment(static p => p.Z, static p => p.Z),
        ]), tolerance: 1.0e-12, what: "covariance upper");
    }

    [Fact]
    public void CovarianceUpperRejectsMismatchedWeightsInsteadOfTruncating() {
        Seq<Point3d> points = Seq(Point3d.Origin, new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 0.0, y: 1.0, z: 0.0));
        _ = Assert.ThrowsAny<XunitException>(testCode: () => Numeric.CovarianceUpper(points: points, weights: Some(Seq(0.5, 0.5))));
    }

    [Fact]
    public void TinyVectorPredicatesAvoidNativeValidityCalls() {
        Vector3d tiny = new(x: 1.0e-16, y: -1.0e-16, z: 1.0e-16);
        Assert.True(condition: Numeric.IsTiny(value: tiny));
        Assert.False(condition: Numeric.IsTiny(value: Vector3d.XAxis));
        Spec.Equal(left: Numeric.AngularDistance(a: tiny, b: Vector3d.XAxis), right: 0.0, tolerance: 0.0, what: "tiny angular distance");
        double[][] rotation = Numeric.RotationMatrix(axis: tiny, angle: Math.PI / 2.0);
        Spec.Equal(left: rotation[0][0], right: 0.0, tolerance: 1.0e-12, what: "z rotation xx");
        Spec.Equal(left: rotation[0][1], right: -1.0, tolerance: 1.0e-12, what: "z rotation xy");
        Spec.Equal(left: rotation[1][0], right: 1.0, tolerance: 1.0e-12, what: "z rotation yx");
    }
}
