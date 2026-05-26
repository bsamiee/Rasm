using System;
using System.Linq;
using LanguageExt;
using Rasm.Domain;
using Rasm.RhinoBridge.Protocol;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
using Dimension = Rasm.Vectors.Dimension;

Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");

// --- [SCENARIO: vectors-cloud-shapes] ---------------------------------------------------
{
    const string theme = "vectors-cloud-shapes";
    Op key = Op.Of(name: theme);
    Seq<Point3d> ringPoints = Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 2.0, y: 0.0, z: 0.0),
        new Point3d(x: 2.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0));
    VectorCloud ring = Probe.Expect(VectorCloud.Ring(points: ringPoints, context: context, key: key), "ring");
    VectorCloud cluster = Probe.Expect(VectorCloud.WeightedCluster(
        points: Seq(new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 3.0, y: 0.0, z: 0.0), new Point3d(x: 3.0, y: 3.0, z: 0.0)),
        mass: Seq(2.0, 3.0, 5.0),
        context: context,
        key: key), "weighted cluster");
    VectorCloud shifted = Probe.Expect(VectorCloud.Cluster(
        points: Seq(new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 4.0, y: 0.0, z: 0.0), new Point3d(x: 4.0, y: 3.0, z: 0.0)),
        context: context,
        key: key), "target cluster");
    double area = Probe.Project<double>(intent: VectorIntent.Cloud(cloud: ring, metric: VectorCloudMetric.Area, key: key), context: context, key: key, label: "area");
    VectorCloudShape shape = Probe.Project<VectorCloudShape>(intent: VectorIntent.Cloud(cloud: ring, metric: VectorCloudMetric.Shape, key: key), context: context, key: key, label: "shape");
    Vector3d spread = Probe.Project<Vector3d>(intent: VectorIntent.Cloud(cloud: cluster, metric: VectorCloudMetric.Spread, key: key), context: context, key: key, label: "spread");
    CloudTransportPolicy transportPolicy = Probe.Expect(CloudTransportPolicy.Of(regularization: 0.5, maxIterations: 64, massRelaxation: 1.0, key: key), "transport policy");
    SinkhornReceipt transport = Probe.Expect(Probe.Expect(VectorIntent.Transport(source: cluster, target: shifted, policy: transportPolicy, key: key), "transport intent").Project<SinkhornReceipt>(context: context, key: key), "transport");
    Probe.Require(Math.Abs(area - 2.0) <= 1.0e-6, $"area={area:R}");
    Probe.Require(shape.Area.IsSome && shape.Perimeter.IsSome && shape.Centroid.DistanceTo(new Point3d(x: 1.0, y: 0.5, z: 0.0)) <= 1.0e-6, $"shape.centroid={shape.Centroid}");
    Probe.Require(spread.IsValid && spread.X >= 0.0 && spread.Y >= 0.0 && spread.Z >= 0.0, $"spread={spread}");
    Probe.Require(RhinoMath.IsValidDouble(x: transport.Distance) && transport.Iterations >= 1, $"transport={transport}");
    Probe.Require(transport.Correspondences.CoveredSourceCount == 3 && transport.Correspondences.CoveredTargetCount == 3, $"transport.coverage={transport.Correspondences}");
    Probe.Require(transport.Correspondences.RetainedSourceMass > 0.0 && transport.Correspondences.RetainedTargetMass > 0.0, $"transport.mass={transport.Correspondences}");
    BridgeMarker.EmitScenarioHeader(scenario: theme, capturePath: CAPTURE_PATH);
    BridgeMarker.EmitFact(key: "ring.area", value: area.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
    BridgeMarker.EmitFact(key: "ring.centroid", value: shape.Centroid);
    BridgeMarker.EmitFact(key: "cluster.spread", value: spread);
    BridgeMarker.EmitFact(key: "transport.distance", value: transport.Distance.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
    BridgeMarker.EmitFact(key: "transport.coveredSource", value: transport.Correspondences.CoveredSourceCount);
    BridgeMarker.EmitFact(key: "transport.coveredTarget", value: transport.Correspondences.CoveredTargetCount);
}

// --- [SCENARIO: vectors-cloud-neighborhood] ---------------------------------------------
{
    const string theme = "vectors-cloud-neighborhood";
    Op key = Op.Of(name: theme);
    static Seq<Point3d> Grid(double step, Func<double, double, double> z) =>
        toSeq(from ix in Enumerable.Range(start: -2, count: 5) from iy in Enumerable.Range(start: -2, count: 5) let x = ix * step let y = iy * step select new Point3d(x: x, y: y, z: z(x, y)));
    CloudCurvatureResult CurvatureOf(Seq<Point3d> points, CloudMetricPolicy policy, string label) =>
        Probe.Project<CloudCurvatureResult>(intent: VectorIntent.Cloud(cloud: Probe.Expect(VectorCloud.Cluster(points: points, context: context, key: key), $"{label} cluster"), metric: VectorCloudMetric.PrincipalCurvature, policy: Some(policy), key: key), context: context, key: key, label: label);
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
    CloudCurvatureResult planeCurvature = CurvatureOf(points: Grid(step: 1.0, z: static (_, _) => 0.0), policy: curvaturePolicy, label: "plane curvature");
    CloudCurvatureResult radiusCurvature = CurvatureOf(points: Grid(step: 1.0, z: static (_, _) => 0.0), policy: radiusCurvaturePolicy, label: "radius curvature");
    CloudCurvatureResult sphereCurvature = CurvatureOf(points: Grid(step: 0.5, z: static (x, y) => Math.Sqrt(d: 25.0 - (x * x) - (y * y))), policy: curvedPolicy, label: "sphere curvature");
    CloudCurvatureResult saddleCurvature = CurvatureOf(points: Grid(step: 0.5, z: static (x, y) => 0.25 * ((x * x) - (y * y))), policy: curvedPolicy, label: "saddle curvature");
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
    BridgeMarker.EmitScenarioHeader(scenario: theme, capturePath: CAPTURE_PATH);
    BridgeMarker.EmitFact(key: "normal.count", value: normals.Count);
    BridgeMarker.EmitFact(key: "duplicate.ids", value: string.Join(separator: ",", values: duplicateIds[0]));
    BridgeMarker.EmitFact(key: "curvature.accepted", value: planeCurvature.Receipt.AcceptedSampleCount);
    BridgeMarker.EmitFact(key: "curvature.self", value: planeCurvature.Receipt.SelfNeighborIncluded);
    BridgeMarker.EmitFact(key: "curvature.radiusLimited", value: radiusCurvature.Receipt.RadiusLimited);
    BridgeMarker.EmitFact(key: "sphere.mean", value: sphereMean);
}

// --- [SCENARIO: vectors-cloud-hull] -----------------------------------------------------
{
    const string theme = "vectors-cloud-hull";
    Op key = Op.Of(name: theme);
    VectorCloud tooFew = Probe.Expect(VectorCloud.Cluster(
        points: Seq(
            new Point3d(x: 0.0, y: 0.0, z: 0.0),
            new Point3d(x: 1.0, y: 0.0, z: 0.0),
            new Point3d(x: 0.0, y: 1.0, z: 0.0)),
        context: context,
        key: key), "too few cluster");
    VectorCloud coplanar = Probe.Expect(VectorCloud.Cluster(
        points: Seq(
            new Point3d(x: 0.0, y: 0.0, z: 0.0),
            new Point3d(x: 1.0, y: 0.0, z: 0.0),
            new Point3d(x: 1.0, y: 1.0, z: 0.0),
            new Point3d(x: 0.0, y: 1.0, z: 0.0)),
        context: context,
        key: key), "coplanar cluster");
    VectorCloud cloud = Probe.Expect(VectorCloud.Cluster(
        points: Seq(
            new Point3d(x: 0.0, y: 0.0, z: 0.0),
            new Point3d(x: 1.0, y: 0.0, z: 0.0),
            new Point3d(x: 0.0, y: 1.0, z: 0.0),
            new Point3d(x: 0.0, y: 0.0, z: 1.0),
            new Point3d(x: 0.25, y: 0.25, z: 0.25)),
        context: context,
        key: key), "cluster");
    VectorCloud duplicateFootprint = Probe.Expect(VectorCloud.Cluster(
        points: Seq(
            new Point3d(x: 0.0, y: 0.0, z: 0.0),
            new Point3d(x: 2.0, y: 0.0, z: 0.0),
            new Point3d(x: 2.0, y: 1.0, z: 0.0),
            new Point3d(x: 0.0, y: 1.0, z: 0.0),
            new Point3d(x: 0.0, y: 0.0, z: 0.0),
            new Point3d(x: 1.0, y: 0.5, z: 0.0)),
        context: context,
        key: key), "duplicate footprint cluster");
    VectorCloud collinear = Probe.Expect(VectorCloud.Cluster(
        points: Seq(
            new Point3d(x: 0.0, y: 0.0, z: 0.0),
            new Point3d(x: 1.0, y: 0.0, z: 0.0),
            new Point3d(x: 2.0, y: 0.0, z: 0.0)),
        context: context,
        key: key), "collinear cluster");
    CloudHullReceipt tooFewReceipt = Probe.Project<CloudHullReceipt>(intent: VectorIntent.Hull(source: tooFew, kind: CloudHullKind.Convex3D, key: key), context: context, key: key, label: "too few");
    CloudHullReceipt coplanarReceipt = Probe.Project<CloudHullReceipt>(intent: VectorIntent.Hull(source: coplanar, kind: CloudHullKind.Convex3D, key: key), context: context, key: key, label: "coplanar");
    CloudHullResult result = Probe.Project<CloudHullResult>(intent: VectorIntent.Hull(source: cloud, kind: CloudHullKind.Convex3D, key: key), context: context, key: key, label: "hull");
    Mesh hull = Probe.Expect(result.Mesh.ToFin(Fail: key.InvalidResult()), "hull mesh");
    CloudHullResult footprint = Probe.Project<CloudHullResult>(intent: VectorIntent.Hull(source: duplicateFootprint, kind: CloudHullKind.ConvexFootprint2D, key: key), context: context, key: key, label: "footprint");
    CloudHullResult collinearFootprint = Probe.Project<CloudHullResult>(intent: VectorIntent.Hull(source: collinear, kind: CloudHullKind.ConvexFootprint2D, key: key), context: context, key: key, label: "collinear");
    CloudHullReceipt alphaReceipt = Probe.Project<CloudHullReceipt>(intent: VectorIntent.Hull(source: cloud, kind: CloudHullKind.AlphaShape, key: key), context: context, key: key, label: "alpha");
    Probe.Require(tooFewReceipt.Status.Equals(CloudHullStatus.Rejected) && tooFewReceipt.NativeRouted && tooFewReceipt.ContainmentRejectedCount == 3, $"tooFew={tooFewReceipt}");
    Probe.Require(coplanarReceipt.Status.Equals(CloudHullStatus.Rejected) && coplanarReceipt.CoplanarRejected && coplanarReceipt.NativeRouted, $"coplanar={coplanarReceipt}");
    Probe.Require(result.Receipt.Status.Equals(CloudHullStatus.Completed), $"status={result.Receipt.Status}");
    Probe.Require(result.Receipt.NativeRouted && result.Receipt.NativeFacetCount > 0, $"facets={result.Receipt.NativeFacetCount}");
    Probe.Require(result.Receipt.InputCount == 5 && result.Receipt.OutputVertexCount >= 4, $"receipt={result.Receipt}");
    Probe.Require(hull.IsValid && hull.Faces.Count > 0, $"mesh.faces={hull.Faces.Count}");
    Probe.Require(footprint.Receipt.Status.Equals(CloudHullStatus.Completed) && footprint.Receipt.NativeRouted && footprint.Receipt.InputCount == 6 && footprint.Receipt.OutputVertexCount == 4, $"footprint={footprint.Receipt}");
    Probe.Require(collinearFootprint.Receipt.Status.Equals(CloudHullStatus.Rejected) && collinearFootprint.Mesh.IsNone && collinearFootprint.Receipt.NativeRouted && collinearFootprint.Receipt.ContainmentRejectedCount == 3, $"collinear={collinearFootprint.Receipt}");
    Probe.Require(alphaReceipt.Status.Equals(CloudHullStatus.Unsupported) && !alphaReceipt.NativeRouted, $"alpha={alphaReceipt}");
    BridgeMarker.EmitScenarioHeader(scenario: theme, capturePath: CAPTURE_PATH);
    BridgeMarker.EmitFact(key: "hull.vertices", value: result.Receipt.OutputVertexCount);
    BridgeMarker.EmitFact(key: "hull.facets", value: result.Receipt.NativeFacetCount);
    BridgeMarker.EmitFact(key: "footprint.vertices", value: footprint.Receipt.OutputVertexCount);
    BridgeMarker.EmitFact(key: "collinear.status", value: collinearFootprint.Receipt.Status);
}
