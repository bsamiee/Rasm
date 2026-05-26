using System;
using System.Linq;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
using Dimension = Rasm.Vectors.Dimension;

static Seq<Point3d> Grid(double step, Func<double, double, double> z) =>
    toSeq(from ix in Enumerable.Range(start: -2, count: 5) from iy in Enumerable.Range(start: -2, count: 5) let x = ix * step let y = iy * step select new Point3d(x: x, y: y, z: z(x, y)));
static CloudCurvatureResult CurvatureOf(Seq<Point3d> points, Context context, CloudMetricPolicy policy, Op key, string label) =>
    Probe.Project<CloudCurvatureResult>(intent: VectorIntent.Cloud(cloud: Probe.Expect(VectorCloud.Cluster(points: points, context: context, key: key), $"{label} cluster"), metric: VectorCloudMetric.PrincipalCurvature, policy: Some(policy), key: key), context: context, key: key, label: label);

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
Dimension neighbors = Probe.Expect(key.AcceptValidated<Dimension>(candidate: 4), "neighbors");
PositiveMagnitude gap = Probe.Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-8), "gap");
PositiveMagnitude residual = Probe.Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-3), "residual");
CloudMetricPolicy policy = new(Neighborhood: new CloudNeighborhoodPolicy(NeighborCount: neighbors, Radius: Option<PositiveMagnitude>.None, EigenGapTolerance: gap, FitResidualTolerance: residual));
PositiveMagnitude radius = Probe.Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 2.0), "radius");
VectorCloud cloud = Probe.Expect(VectorCloud.Cluster(
    points: Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0),
        new Point3d(x: 1.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.5, y: 0.5, z: 0.2)),
    context: context,
    key: key), "cluster");
Point3d[] duplicatePoints = new[] {
    new Point3d(x: 0.0, y: 0.0, z: 0.0),
    new Point3d(x: 0.0, y: 0.0, z: 0.0),
    new Point3d(x: 1.0, y: 0.0, z: 0.0)
};
PointCloud duplicateCloud = new();
duplicateCloud.AddRange(points: duplicatePoints);
int[][] duplicateIds = RTree.PointCloudKNeighbors(pointcloud: duplicateCloud, needlePts: duplicatePoints, amount: 3).ToArray();
Seq<Vector3d> normals = Probe.Project<Seq<Vector3d>>(intent: VectorIntent.Cloud(cloud: cloud, metric: VectorCloudMetric.OrientedNormals, policy: Some(policy), key: key), context: context, key: key, label: "normals");
Seq<Vector3d> radiusNormals = Probe.Project<Seq<Vector3d>>(intent: VectorIntent.Cloud(cloud: cloud, metric: VectorCloudMetric.OrientedNormals, policy: Some(policy with { Neighborhood = policy.Neighborhood with { Radius = Some(radius) } }), key: key), context: context, key: key, label: "radius normals");
Dimension curvatureNeighbors = Probe.Expect(key.AcceptValidated<Dimension>(candidate: 9), "curvature neighbors");
CloudMetricPolicy curvaturePolicy = policy with { Neighborhood = policy.Neighborhood with { NeighborCount = curvatureNeighbors } };
CloudMetricPolicy radiusCurvaturePolicy = curvaturePolicy with { Neighborhood = curvaturePolicy.Neighborhood with { Radius = Some(radius) } };
PositiveMagnitude curvedResidual = Probe.Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 0.05), "curved residual");
CloudMetricPolicy curvedPolicy = curvaturePolicy with { Neighborhood = curvaturePolicy.Neighborhood with { FitResidualTolerance = curvedResidual } };
CloudCurvatureResult planeCurvature = CurvatureOf(points: Grid(step: 1.0, z: static (_, _) => 0.0), context: context, policy: curvaturePolicy, key: key, label: "plane curvature");
CloudCurvatureResult radiusCurvature = CurvatureOf(points: Grid(step: 1.0, z: static (_, _) => 0.0), context: context, policy: radiusCurvaturePolicy, key: key, label: "radius curvature");
CloudCurvatureResult sphereCurvature = CurvatureOf(points: Grid(step: 0.5, z: static (x, y) => Math.Sqrt(d: 25.0 - (x * x) - (y * y))), context: context, policy: curvedPolicy, key: key, label: "sphere curvature");
CloudCurvatureResult saddleCurvature = CurvatureOf(points: Grid(step: 0.5, z: static (x, y) => 0.25 * ((x * x) - (y * y))), context: context, policy: curvedPolicy, key: key, label: "saddle curvature");
double sphereMean = System.Linq.Enumerable.Average(sphereCurvature.Samples.AsIterable(), static sample => 0.5 * (Math.Abs(value: sample.K1) + Math.Abs(value: sample.K2)));

Probe.Require(normals.Count == 5, $"normal.count={normals.Count}");
Probe.Require(radiusNormals.Count == 5, $"radius.normal.count={radiusNormals.Count}");
Probe.Require(normals.ForAll(static normal => normal.IsValid && Math.Abs(value: normal.Length - 1.0) <= 1.0e-6), "unit normals");
Probe.Require(duplicateIds.Length == 3 && duplicateIds[0].Contains(0) && duplicateIds[0].Contains(1), $"duplicate.ids={string.Join(separator: ",", values: duplicateIds[0])}");
Probe.Require(duplicateCloud.PointAt(index: duplicateIds[0][0]).DistanceTo(other: duplicatePoints[0]) <= context.Absolute.Value, "duplicate PointAt round trip failed");
Probe.Require(planeCurvature.Receipt.AcceptedSampleCount == 25 && planeCurvature.Receipt.RejectedSampleCount == 0, $"curvature.receipt={planeCurvature.Receipt}");
Probe.Require(planeCurvature.Receipt.SelfNeighborIncluded && planeCurvature.Receipt.NativeIndexRouted && !planeCurvature.Receipt.RadiusLimited && planeCurvature.Receipt.SearchBackend.Equals(CloudNeighborhoodSearchBackend.RhinoPointCloudKnn), $"knn.receipt={planeCurvature.Receipt}");
Probe.Require(radiusCurvature.Receipt.SelfNeighborIncluded && radiusCurvature.Receipt.NativeIndexRouted && radiusCurvature.Receipt.RadiusLimited && radiusCurvature.Receipt.SearchBackend.Equals(CloudNeighborhoodSearchBackend.RhinoPointCloudRadius), $"radius.receipt={radiusCurvature.Receipt}");
Probe.Require(planeCurvature.Samples.ForAll(static sample => Math.Abs(value: sample.K1) <= 1.0e-8 && Math.Abs(value: sample.K2) <= 1.0e-8), "plane curvature");
Probe.Require(sphereMean is > 0.12 and < 0.35, $"sphere.mean={sphereMean:R}");
Probe.Require(saddleCurvature.Samples.AsIterable().Any(static sample => sample.K1 * sample.K2 < -1.0e-3), "saddle signed curvature");

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.Emit("normal.count", normals.Count);
Evidence.Emit("duplicate.ids", string.Join(separator: ",", values: duplicateIds[0]));
Evidence.Emit("curvature.accepted", planeCurvature.Receipt.AcceptedSampleCount);
Evidence.Emit("curvature.self", planeCurvature.Receipt.SelfNeighborIncluded);
Evidence.Emit("curvature.radiusLimited", radiusCurvature.Receipt.RadiusLimited);
Evidence.Emit("sphere.mean", sphereMean);
