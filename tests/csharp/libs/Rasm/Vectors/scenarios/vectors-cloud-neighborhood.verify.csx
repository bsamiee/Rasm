using System;
using System.Linq;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
using Dimension = Rasm.Vectors.Dimension;

static T Expect<T>(Fin<T> result, string label) =>
    result.Match(Succ: static value => value, Fail: error => throw new InvalidOperationException($"{label}: {error.Message}"));
static void Require(bool condition, string message) =>
    _ = condition ? true : throw new InvalidOperationException(message);
static Seq<Point3d> Grid(double step, Func<double, double, double> z) =>
    toSeq(from ix in Enumerable.Range(start: -2, count: 5) from iy in Enumerable.Range(start: -2, count: 5) let x = ix * step let y = iy * step select new Point3d(x: x, y: y, z: z(x, y)));
static CloudCurvatureResult CurvatureOf(Seq<Point3d> points, Context context, CloudMetricPolicy policy, Op key, string label) =>
    Expect(Expect(VectorIntent.Cloud(cloud: Expect(VectorCloud.Cluster(points: points, context: context, key: key), $"{label} cluster"), metric: VectorCloudMetric.PrincipalCurvature, policy: Some(policy), key: key), $"{label} intent").Project<CloudCurvatureResult>(context: context, key: key), label);

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
Dimension neighbors = Expect(key.AcceptValidated<Dimension>(candidate: 4), "neighbors");
PositiveMagnitude gap = Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-8), "gap");
PositiveMagnitude residual = Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-3), "residual");
CloudMetricPolicy policy = new(Neighborhood: new CloudNeighborhoodPolicy(NeighborCount: neighbors, Radius: Option<PositiveMagnitude>.None, EigenGapTolerance: gap, FitResidualTolerance: residual));
PositiveMagnitude radius = Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 2.0), "radius");
VectorCloud cloud = Expect(VectorCloud.Cluster(
    points: Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0),
        new Point3d(x: 1.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.5, y: 0.5, z: 0.2)),
    context: context,
    key: key), "cluster");
Seq<Vector3d> normals = Expect(Expect(VectorIntent.Cloud(cloud: cloud, metric: VectorCloudMetric.OrientedNormals, policy: Some(policy), key: key), "normal intent").Project<Seq<Vector3d>>(context: context, key: key), "normals");
Seq<Vector3d> radiusNormals = Expect(Expect(VectorIntent.Cloud(cloud: cloud, metric: VectorCloudMetric.OrientedNormals, policy: Some(policy with { Neighborhood = policy.Neighborhood with { Radius = Some(radius) } }), key: key), "radius normal intent").Project<Seq<Vector3d>>(context: context, key: key), "radius normals");
Dimension curvatureNeighbors = Expect(key.AcceptValidated<Dimension>(candidate: 9), "curvature neighbors");
CloudMetricPolicy curvaturePolicy = policy with { Neighborhood = policy.Neighborhood with { NeighborCount = curvatureNeighbors } };
PositiveMagnitude curvedResidual = Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 0.05), "curved residual");
CloudMetricPolicy curvedPolicy = curvaturePolicy with { Neighborhood = curvaturePolicy.Neighborhood with { FitResidualTolerance = curvedResidual } };
CloudCurvatureResult planeCurvature = CurvatureOf(points: Grid(step: 1.0, z: static (_, _) => 0.0), context: context, policy: curvaturePolicy, key: key, label: "plane curvature");
CloudCurvatureResult sphereCurvature = CurvatureOf(points: Grid(step: 0.5, z: static (x, y) => Math.Sqrt(d: 25.0 - (x * x) - (y * y))), context: context, policy: curvedPolicy, key: key, label: "sphere curvature");
CloudCurvatureResult saddleCurvature = CurvatureOf(points: Grid(step: 0.5, z: static (x, y) => 0.25 * ((x * x) - (y * y))), context: context, policy: curvedPolicy, key: key, label: "saddle curvature");
double sphereMean = System.Linq.Enumerable.Average(sphereCurvature.Samples.AsIterable(), static sample => 0.5 * (Math.Abs(value: sample.K1) + Math.Abs(value: sample.K2)));

Require(normals.Count == 5, $"normal.count={normals.Count}");
Require(radiusNormals.Count == 5, $"radius.normal.count={radiusNormals.Count}");
Require(normals.ForAll(static normal => normal.IsValid && Math.Abs(value: normal.Length - 1.0) <= 1.0e-6), "unit normals");
Require(planeCurvature.Receipt.AcceptedSampleCount == 25 && planeCurvature.Receipt.RejectedSampleCount == 0, $"curvature.receipt={planeCurvature.Receipt}");
Require(planeCurvature.Samples.ForAll(static sample => Math.Abs(value: sample.K1) <= 1.0e-8 && Math.Abs(value: sample.K2) <= 1.0e-8), "plane curvature");
Require(sphereMean is > 0.12 and < 0.35, $"sphere.mean={sphereMean:R}");
Require(saddleCurvature.Samples.AsIterable().Any(static sample => sample.K1 * sample.K2 < -1.0e-3), "saddle signed curvature");

Console.WriteLine($"scenario={SCENARIO_NAME}");
Console.WriteLine($"capture={CAPTURE_PATH}");
Console.WriteLine($"normal.count={normals.Count}");
Console.WriteLine($"curvature.accepted={planeCurvature.Receipt.AcceptedSampleCount}");
Console.WriteLine($"sphere.mean={sphereMean:R}");
