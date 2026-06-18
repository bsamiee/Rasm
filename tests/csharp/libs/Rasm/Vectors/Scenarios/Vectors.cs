using System.Collections.Immutable;
using Rasm.Domain;
using Rasm.TestKit.Scenarios;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using Dimension = Rasm.Vectors.Dimension;

namespace Rasm.Tests.Vectors.Scenarios;

// --- [OPERATIONS] ---------------------------------------------------------------------------

// Ownership: the Vectors theme — cloud metrics, scalar fields, sampling, and spectral rails over
// the VectorIntent projection surface. Each entrypoint mirrors one legacy verify scenario name;
// fixtures are boundary helpers, every projection is fact-fused through the scenario context.
internal static class VectorsScenarios {
    [RhinoScenario(theme: "vectors")]
    internal static Fin<Unit> CloudShapes(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        from context in ctx.Expect(label: "context", projection: Context.Of(units: UnitSystem.Millimeters).ToFin())
        let op = Op.Of()
        from ring in ctx.Expect(label: "ring", projection: VectorCloud.Ring(
            points: Seq(
                new Point3d(x: 0.0, y: 0.0, z: 0.0),
                new Point3d(x: 2.0, y: 0.0, z: 0.0),
                new Point3d(x: 2.0, y: 1.0, z: 0.0),
                new Point3d(x: 0.0, y: 1.0, z: 0.0)),
            context: context, key: op))
        from cluster in ctx.Expect(label: "weighted cluster", projection: VectorCloud.WeightedCluster(
            points: Seq(new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 3.0, y: 0.0, z: 0.0), new Point3d(x: 3.0, y: 3.0, z: 0.0)),
            mass: Seq(2.0, 3.0, 5.0),
            context: context, key: op))
        from shifted in ctx.Expect(label: "target cluster", projection: VectorCloud.Cluster(
            points: Seq(new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 4.0, y: 0.0, z: 0.0), new Point3d(x: 4.0, y: 3.0, z: 0.0)),
            context: context, key: op))
        from area in Project<double>(ctx: ctx, intent: VectorIntent.Cloud(cloud: ring, metric: VectorCloudMetric.Area, key: op), context: context, key: op, label: "area")
        from shape in Project<VectorCloudShape>(ctx: ctx, intent: VectorIntent.Cloud(cloud: ring, metric: VectorCloudMetric.Shape, key: op), context: context, key: op, label: "shape")
        from spread in Project<Vector3d>(ctx: ctx, intent: VectorIntent.Cloud(cloud: cluster, metric: VectorCloudMetric.Spread, key: op), context: context, key: op, label: "spread")
        from transportPolicy in ctx.Expect(label: "transport policy", projection: CloudTransportPolicy.Of(regularization: 0.5, maxIterations: 64, massRelaxation: 1.0, key: op))
        from transportIntent in ctx.Expect(label: "transport intent", projection: VectorIntent.Transport(source: cluster, target: shifted, policy: transportPolicy, key: op))
        from transport in ctx.Expect(label: "transport", projection: transportIntent.Project<SinkhornReceipt>(context: context, key: op))
        from areaLaw in ctx.Require(label: "ring area", observed: Math.Abs(value: area - 2.0) <= 1.0e-6)
        from shapeLaw in ctx.Require(label: "shape centroid", observed: shape.Area.IsSome && shape.Perimeter.IsSome && shape.Centroid.DistanceTo(other: new Point3d(x: 1.0, y: 0.5, z: 0.0)) <= 1.0e-6)
        from spreadLaw in ctx.Require(label: "spread valid", observed: spread.IsValid && spread.X >= 0.0 && spread.Y >= 0.0 && spread.Z >= 0.0)
        from transportLaw in ctx.Require(label: "transport receipt", observed: RhinoMath.IsValidDouble(x: transport.Distance) && transport.Iterations >= 1)
        from coverageLaw in ctx.Require(label: "transport coverage", observed: transport.Correspondences.CoveredSourceCount == 3 && transport.Correspondences.CoveredTargetCount == 3)
        from massLaw in ctx.Require(label: "transport mass", observed: transport.Correspondences.RetainedSourceMass > 0.0 && transport.Correspondences.RetainedTargetMass > 0.0)
        let areaFact = Note(ctx: ctx, key: "ring.area", value: area.ToString(format: "F6", provider: System.Globalization.CultureInfo.InvariantCulture))
        let centroidFact = Note(ctx: ctx, key: "ring.centroid", value: Text(value: shape.Centroid))
        let spreadFact = Note(ctx: ctx, key: "cluster.spread", value: Text(value: spread))
        let distanceFact = Note(ctx: ctx, key: "transport.distance", value: transport.Distance.ToString(format: "F6", provider: System.Globalization.CultureInfo.InvariantCulture))
        let coveredSourceFact = Note(ctx: ctx, key: "transport.coveredSource", value: transport.Correspondences.CoveredSourceCount)
        let coveredTargetFact = Note(ctx: ctx, key: "transport.coveredTarget", value: transport.Correspondences.CoveredTargetCount)
        select Done(scope: scope);

    [RhinoScenario(theme: "vectors")]
    internal static Fin<Unit> CloudNeighborhood(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        from context in ctx.Expect(label: "context", projection: Context.Of(units: UnitSystem.Millimeters).ToFin())
        let op = Op.Of()
        from neighbors in ctx.Expect(label: "neighbors", projection: op.AcceptValidated<Dimension>(candidate: 4))
        from gap in ctx.Expect(label: "gap", projection: op.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-8))
        from residual in ctx.Expect(label: "residual", projection: op.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-3))
        let policy = new CloudMetricPolicy(Neighborhood: new CloudNeighborhoodPolicy(NeighborCount: neighbors, Radius: Option<PositiveMagnitude>.None, EigenGapTolerance: gap, FitResidualTolerance: residual))
        from radius in ctx.Expect(label: "radius", projection: op.AcceptValidated<PositiveMagnitude>(candidate: 2.0))
        from cloud in ctx.Expect(label: "cluster", projection: VectorCloud.Cluster(
            points: Seq(
                new Point3d(x: 0.0, y: 0.0, z: 0.0),
                new Point3d(x: 1.0, y: 0.0, z: 0.0),
                new Point3d(x: 0.0, y: 1.0, z: 0.0),
                new Point3d(x: 1.0, y: 1.0, z: 0.0),
                new Point3d(x: 0.5, y: 0.5, z: 0.2)),
            context: context, key: op))
        let duplicate = DuplicateProbe()
        from duplicateVector in ctx.Expect(label: "dedup cloud", projection: VectorCloud.WeightedCluster(points: toSeq(duplicate.Points), mass: Seq(2.0, 3.0, 5.0), context: context, key: op))
        from duplicateAdmission in Project<CloudAdmissionReceipt>(ctx: ctx, intent: VectorIntent.Cloud(cloud: duplicateVector, metric: VectorCloudMetric.Admission, policy: Some(policy), key: op), context: context, key: op, label: "dedup admission")
        from neighborhood in Project<CloudNeighborhoodReceipt>(ctx: ctx, intent: VectorIntent.Cloud(cloud: cloud, metric: VectorCloudMetric.Neighborhood, policy: Some(policy), key: op), context: context, key: op, label: "neighborhood receipt")
        from normals in Project<Seq<Vector3d>>(ctx: ctx, intent: VectorIntent.Cloud(cloud: cloud, metric: VectorCloudMetric.OrientedNormals, policy: Some(policy), key: op), context: context, key: op, label: "normals")
        from radiusNormals in Project<Seq<Vector3d>>(ctx: ctx, intent: VectorIntent.Cloud(cloud: cloud, metric: VectorCloudMetric.OrientedNormals, policy: Some(policy with { Neighborhood = policy.Neighborhood with { Radius = Some(radius) } }), key: op), context: context, key: op, label: "radius normals")
        from curvatureNeighbors in ctx.Expect(label: "curvature neighbors", projection: op.AcceptValidated<Dimension>(candidate: 9))
        let curvaturePolicy = policy with { Neighborhood = policy.Neighborhood with { NeighborCount = curvatureNeighbors } }
        let radiusCurvaturePolicy = curvaturePolicy with { Neighborhood = curvaturePolicy.Neighborhood with { Radius = Some(radius) } }
        from curvedResidual in ctx.Expect(label: "curved residual", projection: op.AcceptValidated<PositiveMagnitude>(candidate: 0.05))
        let curvedPolicy = curvaturePolicy with { Neighborhood = curvaturePolicy.Neighborhood with { FitResidualTolerance = curvedResidual } }
        from planeCurvature in Curvature(ctx: ctx, context: context, key: op, points: Grid(step: 1.0, z: static (_, _) => 0.0), policy: curvaturePolicy, label: "plane curvature")
        from radiusCurvature in Curvature(ctx: ctx, context: context, key: op, points: Grid(step: 1.0, z: static (_, _) => 0.0), policy: radiusCurvaturePolicy, label: "radius curvature")
        from sphereCurvature in Curvature(ctx: ctx, context: context, key: op, points: Grid(step: 0.5, z: static (x, y) => Math.Sqrt(d: 25.0 - (x * x) - (y * y))), policy: curvedPolicy, label: "sphere curvature")
        from saddleCurvature in Curvature(ctx: ctx, context: context, key: op, points: Grid(step: 0.5, z: static (x, y) => 0.25 * ((x * x) - (y * y))), policy: curvedPolicy, label: "saddle curvature")
        let sphereMean = Enumerable.Average(source: sphereCurvature.Samples.AsIterable(), selector: static sample => 0.5 * (Math.Abs(value: sample.K1) + Math.Abs(value: sample.K2)))
        from normalCount in ctx.Require(label: "normal count", observed: normals.Count == 5)
        from radiusNormalCount in ctx.Require(label: "radius normal count", observed: radiusNormals.Count == 5)
        from unitNormals in ctx.Require(label: "unit normals", observed: normals.ForAll(static normal => normal.IsValid && Math.Abs(value: normal.Length - 1.0) <= 1.0e-6))
        from duplicateIdLaw in ctx.Require(label: "duplicate ids", observed: duplicate.Ids.Length == 3 && duplicate.Ids[0].Contains(value: 0) && duplicate.Ids[0].Contains(value: 1))
        from duplicateRoundTrip in ctx.Require(label: "duplicate PointAt round trip", observed: duplicate.Cloud.PointAt(index: duplicate.Ids[0][0]).DistanceTo(other: duplicate.Points[0]) <= context.Absolute.Value)
        from admissionLaw in ctx.Require(label: "dedup admission", observed: duplicateAdmission.InputCount == 3 && duplicateAdmission.OutputCount == 2 && duplicateAdmission.InputDuplicateCoordinateCount == 1 && duplicateAdmission.MergedCoordinateCount == 1 && duplicateAdmission.Deduplicated && duplicateAdmission.Tolerance >= 0.0)
        from neighborhoodLaw in ctx.Require(label: "neighborhood receipt", observed: neighborhood.NativeIndexRouted && neighborhood.QueryCount == 5 && neighborhood.InputCount == 5 && neighborhood.RequestedNeighborCount == 4 && neighborhood.SearchBackend.Equals(other: CloudNeighborhoodSearchBackend.RhinoPointCloudKnn))
        from curvatureCounts in ctx.Require(label: "curvature receipt", observed: planeCurvature.Receipt.AcceptedSampleCount == 25 && planeCurvature.Receipt.RejectedSampleCount == 0)
        from knnReceipt in ctx.Require(label: "knn receipt", observed: planeCurvature.Receipt.SelfNeighborIncluded && planeCurvature.Receipt.NativeIndexRouted && !planeCurvature.Receipt.RadiusLimited && planeCurvature.Receipt.SearchBackend.Equals(other: CloudNeighborhoodSearchBackend.RhinoPointCloudKnn))
        from radiusReceipt in ctx.Require(label: "radius receipt", observed: radiusCurvature.Receipt.SelfNeighborIncluded && radiusCurvature.Receipt.NativeIndexRouted && radiusCurvature.Receipt.RadiusLimited && radiusCurvature.Receipt.SearchBackend.Equals(other: CloudNeighborhoodSearchBackend.RhinoPointCloudRadius))
        from planeRange in ctx.Require(label: "plane range", observed: planeCurvature.Receipt.Range.Kind.Equals(other: CloudCurvatureRangeKind.Plane) && planeCurvature.Receipt.Range.PlaneLikeCount == 25)
        from saddleRange in ctx.Require(label: "saddle range", observed: saddleCurvature.Receipt.Range.SaddleLikeCount > 0 && (saddleCurvature.Receipt.Range.Kind.Equals(other: CloudCurvatureRangeKind.Saddle) || saddleCurvature.Receipt.Range.Kind.Equals(other: CloudCurvatureRangeKind.Mixed)))
        from planeFlat in ctx.Require(label: "plane curvature", observed: planeCurvature.Samples.ForAll(static sample => Math.Abs(value: sample.K1) <= 1.0e-8 && Math.Abs(value: sample.K2) <= 1.0e-8))
        from sphereLaw in ctx.Require(label: "sphere mean", observed: sphereMean is > 0.12 and < 0.35)
        from saddleSign in ctx.Require(label: "saddle signed curvature", observed: saddleCurvature.Samples.AsIterable().Any(static sample => sample.K1 * sample.K2 < -1.0e-3))
        let normalFact = Note(ctx: ctx, key: "normal.count", value: normals.Count)
        let idsFact = Note(ctx: ctx, key: "duplicate.ids", value: string.Join(separator: ',', values: duplicate.Ids[0]))
        let outputFact = Note(ctx: ctx, key: "duplicate.output", value: duplicateAdmission.OutputCount)
        let backendFact = Note(ctx: ctx, key: "neighborhood.backend", value: neighborhood.SearchBackend.Key)
        let acceptedFact = Note(ctx: ctx, key: "curvature.accepted", value: planeCurvature.Receipt.AcceptedSampleCount)
        let selfFact = Note(ctx: ctx, key: "curvature.self", value: planeCurvature.Receipt.SelfNeighborIncluded)
        let radiusFact = Note(ctx: ctx, key: "curvature.radiusLimited", value: radiusCurvature.Receipt.RadiusLimited)
        let rangeFact = Note(ctx: ctx, key: "curvature.range", value: planeCurvature.Receipt.Range.Kind.Key)
        let meanFact = Note(ctx: ctx, key: "sphere.mean", value: sphereMean)
        select Done(scope: scope);

    [RhinoScenario(theme: "vectors")]
    internal static Fin<Unit> CloudHull(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        from context in ctx.Expect(label: "context", projection: Context.Of(units: UnitSystem.Millimeters).ToFin())
        let op = Op.Of()
        from tooFew in ctx.Expect(label: "too few cluster", projection: VectorCloud.Cluster(
            points: Seq(new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 0.0, y: 1.0, z: 0.0)),
            context: context, key: op))
        from coplanar in ctx.Expect(label: "coplanar cluster", projection: VectorCloud.Cluster(
            points: Seq(
                new Point3d(x: 0.0, y: 0.0, z: 0.0),
                new Point3d(x: 1.0, y: 0.0, z: 0.0),
                new Point3d(x: 1.0, y: 1.0, z: 0.0),
                new Point3d(x: 0.0, y: 1.0, z: 0.0)),
            context: context, key: op))
        from cloud in ctx.Expect(label: "cluster", projection: VectorCloud.Cluster(
            points: Seq(
                new Point3d(x: 0.0, y: 0.0, z: 0.0),
                new Point3d(x: 1.0, y: 0.0, z: 0.0),
                new Point3d(x: 0.0, y: 1.0, z: 0.0),
                new Point3d(x: 0.0, y: 0.0, z: 1.0),
                new Point3d(x: 0.25, y: 0.25, z: 0.25)),
            context: context, key: op))
        from duplicateFootprint in ctx.Expect(label: "duplicate footprint cluster", projection: VectorCloud.Cluster(
            points: Seq(
                new Point3d(x: 0.0, y: 0.0, z: 0.0),
                new Point3d(x: 2.0, y: 0.0, z: 0.0),
                new Point3d(x: 2.0, y: 1.0, z: 0.0),
                new Point3d(x: 0.0, y: 1.0, z: 0.0),
                new Point3d(x: 0.0, y: 0.0, z: 0.0),
                new Point3d(x: 1.0, y: 0.5, z: 0.0)),
            context: context, key: op))
        from collinear in ctx.Expect(label: "collinear cluster", projection: VectorCloud.Cluster(
            points: Seq(new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 2.0, y: 0.0, z: 0.0)),
            context: context, key: op))
        from tooFewReceipt in Project<CloudHullReceipt>(ctx: ctx, intent: VectorIntent.Hull(source: tooFew, kind: CloudHullKind.Convex3D, key: op), context: context, key: op, label: "too few")
        from coplanarReceipt in Project<CloudHullReceipt>(ctx: ctx, intent: VectorIntent.Hull(source: coplanar, kind: CloudHullKind.Convex3D, key: op), context: context, key: op, label: "coplanar")
        from result in Project<CloudHullResult>(ctx: ctx, intent: VectorIntent.Hull(source: cloud, kind: CloudHullKind.Convex3D, key: op), context: context, key: op, label: "hull")
        from hull in ctx.Expect(label: "hull mesh", projection: result.Mesh.ToFin(Fail: op.InvalidResult()))
        from footprint in Project<CloudHullResult>(ctx: ctx, intent: VectorIntent.Hull(source: duplicateFootprint, kind: CloudHullKind.ConvexFootprint2D, key: op), context: context, key: op, label: "footprint")
        from collinearFootprint in Project<CloudHullResult>(ctx: ctx, intent: VectorIntent.Hull(source: collinear, kind: CloudHullKind.ConvexFootprint2D, key: op), context: context, key: op, label: "collinear")
        from alphaReceipt in Project<CloudHullReceipt>(ctx: ctx, intent: VectorIntent.Hull(source: cloud, kind: CloudHullKind.AlphaShape, key: op), context: context, key: op, label: "alpha")
        from tooFewLaw in ctx.Require(label: "too few rejected", observed: tooFewReceipt.Status.Equals(other: CloudHullStatus.Rejected) && tooFewReceipt.NativeRouted && tooFewReceipt.ContainmentRejectedCount == 3)
        from coplanarLaw in ctx.Require(label: "coplanar rejected", observed: coplanarReceipt.Status.Equals(other: CloudHullStatus.Rejected) && coplanarReceipt.CoplanarRejected && coplanarReceipt.NativeRouted)
        from statusLaw in ctx.Require(label: "hull completed", observed: result.Receipt.Status.Equals(other: CloudHullStatus.Completed))
        from facetLaw in ctx.Require(label: "hull facets", observed: result.Receipt.NativeRouted && result.Receipt.NativeFacetCount > 0)
        from receiptLaw in ctx.Require(label: "hull receipt", observed: result.Receipt.InputCount == 5 && result.Receipt.OutputVertexCount >= 4)
        from meshLaw in ctx.Require(label: "hull mesh faces", observed: hull.IsValid && hull.Faces.Count > 0)
        from footprintLaw in ctx.Require(label: "footprint receipt", observed: footprint.Receipt.Status.Equals(other: CloudHullStatus.Completed) && footprint.Receipt.NativeRouted && footprint.Receipt.InputCount == 5 && footprint.Receipt.OutputVertexCount == 4)
        from collinearLaw in ctx.Require(label: "collinear rejected", observed: collinearFootprint.Receipt.Status.Equals(other: CloudHullStatus.Rejected) && collinearFootprint.Mesh.IsNone && collinearFootprint.Receipt.NativeRouted && collinearFootprint.Receipt.ContainmentRejectedCount == 3)
        from alphaLaw in ctx.Require(label: "alpha unsupported", observed: alphaReceipt.Status.Equals(other: CloudHullStatus.Unsupported) && !alphaReceipt.NativeRouted)
        let verticesFact = Note(ctx: ctx, key: "hull.vertices", value: result.Receipt.OutputVertexCount)
        let facetsFact = Note(ctx: ctx, key: "hull.facets", value: result.Receipt.NativeFacetCount)
        let footprintFact = Note(ctx: ctx, key: "footprint.vertices", value: footprint.Receipt.OutputVertexCount)
        let collinearFact = Note(ctx: ctx, key: "collinear.status", value: Text(value: collinearFootprint.Receipt.Status))
        select Done(scope: scope);

    [RhinoScenario(theme: "vectors")]
    internal static Fin<Unit> AtomsFrame(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        from context in ctx.Expect(label: "context", projection: Context.Of(units: UnitSystem.Millimeters).ToFin())
        let op = Op.Of()
        from x in ctx.Expect(label: "direction x", projection: Direction.Of(value: new Vector3d(x: 4.0, y: 0.0, z: 0.0), context: context, key: op))
        from axisIntent in ctx.Expect(label: "axis intent", projection: VectorIntent.Axis(axis: SignedAxis.PositiveX, key: op))
        from axisX in ctx.Expect(label: "axis x", projection: axisIntent.Project<Vector3d>(context: context, key: op))
        from frame in ctx.Expect(label: "frame", projection: VectorIntent.Frame(origin: Point3d.Origin, normal: Vector3d.ZAxis, xHint: Some(Vector3d.XAxis)).Project<Plane>(context: context, key: op))
        from components in ctx.Expect(label: "components projection", projection: VectorIntent.Components(anchor: Point3d.Origin, value: new Vector3d(x: 3.0, y: 4.0, z: 0.0), frame: Plane.WorldXY).Project<(double ComponentX, double ComponentY)>(context: context, key: op))
        from cone in ctx.Expect(label: "cone", projection: VectorCone.Of(apex: Point3d.Origin, axis: Vector3d.ZAxis, halfAngleRadians: Math.PI / 6.0, context: context, key: op))
        from coneIntent in ctx.Expect(label: "cone intent", projection: VectorIntent.Cone(cone: cone, mode: ConeProjection.Axis, key: op))
        from coneAxis in ctx.Expect(label: "cone axis", projection: coneIntent.Project<Vector3d>(context: context, key: op))
        from containsAxis in ctx.Expect(label: "cone contains axis", projection: cone.Contains(query: Vector3d.ZAxis, context: context, key: op))
        from rejectsOpposite in ctx.Expect(label: "cone rejects opposite", projection: cone.Contains(query: -Vector3d.ZAxis, context: context, key: op))
        let angleFrameAccepted = VectorIntent.Angular(a: Vector3d.XAxis, b: Vector3d.YAxis, pivot: AnglePivot.Frame(frame: Plane.WorldXY)).Project<double>(context: context, key: op) switch {
            Fin<double>.Succ(double angle) => RhinoMath.IsValidDouble(x: angle),
            _ => false,
        }
        let angleFrameRejected = VectorIntent.Angular(a: Vector3d.XAxis, b: Vector3d.YAxis, pivot: AnglePivot.Frame(frame: Plane.Unset)).Project<double>(context: context, key: op).IsFail
        let contourPlaneAccepted = ContourPolicy.Plane(section: Plane.WorldXY, key: op).IsSucc
        let contourPlaneRejected = ContourPolicy.Plane(section: Plane.Unset, key: op).IsFail
        from directionLaw in ctx.Require(label: "direction unit length", observed: Math.Abs(value: x.Value.Length - 1.0) <= 1.0e-12)
        from axisLaw in ctx.Require(label: "axis x parallel", observed: axisX.IsParallelTo(other: Vector3d.XAxis) == 1)
        from frameLaw in ctx.Require(label: "frame orthonormal", observed: frame.IsValid && Vector3d.AreOrthonormal(x: frame.XAxis, y: frame.YAxis, z: frame.ZAxis))
        from componentsLaw in ctx.Require(label: "components", observed: Math.Abs(value: components.ComponentX - 3.0) <= RhinoMath.ZeroTolerance && Math.Abs(value: components.ComponentY - 4.0) <= RhinoMath.ZeroTolerance)
        from coneAxisLaw in ctx.Require(label: "cone axis parallel", observed: coneAxis.IsParallelTo(other: Vector3d.ZAxis) == 1)
        from containmentLaw in ctx.Require(label: "cone containment rail", observed: containsAxis && !rejectsOpposite)
        from admissionLaw in ctx.Require(label: "canonical plane admission", observed: angleFrameAccepted && angleFrameRejected && contourPlaneAccepted && contourPlaneRejected)
        let directionFact = Note(ctx: ctx, key: "direction", value: Text(value: x.Value))
        let axisFact = Note(ctx: ctx, key: "axis.x", value: Text(value: axisX))
        let frameFact = Note(ctx: ctx, key: "frame.z", value: Text(value: frame.ZAxis))
        let componentsFact = Note(ctx: ctx, key: "components", value: string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{components.ComponentX:R},{components.ComponentY:R}"))
        let coneFact = Note(ctx: ctx, key: "cone.axis", value: Text(value: coneAxis))
        let angleFact = Note(ctx: ctx, key: "angle.frameAccepted", value: angleFrameAccepted)
        let contourFact = Note(ctx: ctx, key: "contourPlaneAccepted", value: contourPlaneAccepted)
        select Done(scope: scope);

    [RhinoScenario(theme: "vectors")]
    internal static Fin<Unit> SpaceProjection(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        from context in ctx.Expect(label: "context", projection: Context.Of(units: UnitSystem.Millimeters).ToFin())
        let op = Op.Of()
        from sphere in ctx.Expect(label: "sphere support", projection: SupportSpace.Of(value: new Sphere(Point3d.Origin, 5.0), key: op))
        let sample = new Point3d(x: 7.0, y: 0.0, z: 0.0)
        from closest in Project<Point3d>(ctx: ctx, intent: VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Closest, key: op), context: context, key: op, label: "closest point")
        from distance in Project<double>(ctx: ctx, intent: VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Distance, key: op), context: context, key: op, label: "distance")
        from direction in Project<Direction>(ctx: ctx, intent: VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Direction, key: op), context: context, key: op, label: "support direction")
        from inward in Project<Vector3d>(ctx: ctx, intent: VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Span, key: op), context: context, key: op, label: "toward span")
        from outward in Project<Vector3d>(ctx: ctx, intent: VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.SignedSpanAway, key: op), context: context, key: op, label: "away span")
        let closestVectorRejected = VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Closest, key: op)
            .Bind(intent => intent.Project<Vector3d>(context: context, key: op))
            .IsFail
        from uv in Project<Point2d>(ctx: ctx, intent: VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Uv, key: op), context: context, key: op, label: "uv")
        let planeSupportAccepted = SupportSpace.Of(value: Plane.WorldXY, key: op).IsSucc
        let planeSupportRejected = SupportSpace.Of(value: Plane.Unset, key: op).IsFail
        from closestLaw in ctx.Require(label: "closest point", observed: closest.DistanceTo(other: new Point3d(x: 5.0, y: 0.0, z: 0.0)) <= 1.0e-6)
        from distanceLaw in ctx.Require(label: "distance", observed: Math.Abs(value: distance - 2.0) <= 1.0e-6)
        from directionLaw in ctx.Require(label: "support direction", observed: direction.Value.X < 0.0 && Math.Abs(value: direction.Value.Length - 1.0) <= 1.0e-6)
        from inwardLaw in ctx.Require(label: "toward span", observed: inward.X < 0.0 && Math.Abs(value: inward.Length - 2.0) <= 1.0e-6)
        from outwardLaw in ctx.Require(label: "away span", observed: outward.X > 0.0 && Math.Abs(value: outward.Length - 2.0) <= 1.0e-6)
        from rejectionLaw in ctx.Require(label: "closest vector rejected", observed: closestVectorRejected)
        from uvLaw in ctx.Require(label: "uv valid", observed: RhinoMath.IsValidDouble(x: uv.X) && RhinoMath.IsValidDouble(x: uv.Y))
        from planeLaw in ctx.Require(label: "support plane admission", observed: planeSupportAccepted && planeSupportRejected)
        let closestFact = Note(ctx: ctx, key: "closest", value: Text(value: closest))
        let distanceFact = Note(ctx: ctx, key: "distance", value: distance.ToString(format: "F6", provider: System.Globalization.CultureInfo.InvariantCulture))
        let directionFact = Note(ctx: ctx, key: "direction", value: Text(value: direction.Value))
        let towardFact = Note(ctx: ctx, key: "span.toward", value: Text(value: inward))
        let awayFact = Note(ctx: ctx, key: "span.away", value: Text(value: outward))
        let uvFact = Note(ctx: ctx, key: "uv", value: Text(value: uv))
        let planeFact = Note(ctx: ctx, key: "planeSupportAccepted", value: planeSupportAccepted)
        select Done(scope: scope);

    [RhinoScenario(theme: "vectors")]
    internal static Fin<Unit> SampleDworkContinuous(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        from context in ctx.Expect(label: "context", projection: Context.Of(units: UnitSystem.Millimeters).ToFin())
        let op = Op.Of()
        let native = OpenGrid()
        from space in ctx.Expect(label: "mesh space", projection: MeshSpace.Of(native: native, context: context, key: op))
        from domain in ctx.Expect(label: "mesh domain", projection: ExtractionDomain.Mesh(value: space, key: op))
        from kind in ctx.Expect(label: "dwork kind", projection: SampleKind.DworkVariableDensity(radius: ScalarField.Constant(value: 0.30), count: 5, minRadius: 0.25, attempts: 30, seed: 42, key: op))
        from intent in ctx.Expect(label: "dwork intent", projection: VectorIntent.Sample(domain: domain, kind: kind, key: op))
        from first in ctx.Expect(label: "first points", projection: intent.Project<Seq<Point3d>>(context: context, key: op))
        from second in ctx.Expect(label: "second points", projection: intent.Project<Seq<Point3d>>(context: context, key: op))
        from receipt in ctx.Expect(label: "receipt", projection: intent.Project<SampleReceipt>(context: context, key: op))
        from algorithm in ctx.Expect(label: "algorithm", projection: receipt.Algorithm.ToFin(Fail: Error.New(message: "algorithm receipt missing")))
        from dwork in ctx.Expect(label: "dwork receipt", projection: algorithm.Dwork.ToFin(Fail: Error.New(message: "dwork receipt missing")))
        from countLaw in ctx.Require(label: "counts", observed: first.Count == second.Count && first.Count == receipt.Emitted && receipt.Emitted > 1)
        from determinismLaw in ctx.Require(label: "determinism + mesh projection", observed: Enumerable.Range(start: 0, count: first.Count).All(predicate: index =>
            first[index].DistanceTo(other: second[index]) <= context.Absolute.Value
            && native.ClosestMeshPoint(testPoint: first[index], maximumDistance: context.Absolute.Value) is { FaceIndex: >= 0 }))
        from domainLaw in ctx.Require(label: "dwork domain", observed: dwork.Domain.Equals(other: DworkSamplingDomain.ContinuousMesh) && dwork.ContinuousMesh && !dwork.CandidateOnly)
        from gridLaw in ctx.Require(label: "dwork grid", observed: receipt.CandidateCount.IsNone && dwork.BackgroundCellSize.IsSome && dwork.BackgroundGridCells.IfNone(0) > 0)
        from countsLaw in ctx.Require(label: "dwork counts", observed: dwork.GeneratedCandidates == receipt.Attempted && receipt.Rejected == dwork.RejectedTooClose + dwork.RejectedDomain)
        from spacingLaw in ctx.Require(label: "spacing", observed: receipt.MinSpacing.Exists(min => min + context.Absolute.Value >= 0.30))
        from validationLaw in ctx.Require(label: "algorithm validation flags", observed: !algorithm.TransportAssignmentValidated && !algorithm.CapacityResidualValidated)
        let emittedFact = Note(ctx: ctx, key: "dwork.emitted", value: receipt.Emitted)
        let tooCloseFact = Note(ctx: ctx, key: "dwork.rejectedTooClose", value: dwork.RejectedTooClose)
        let rejectedDomainFact = Note(ctx: ctx, key: "dwork.rejectedDomain", value: dwork.RejectedDomain)
        let generatedFact = Note(ctx: ctx, key: "dwork.generated", value: dwork.GeneratedCandidates)
        let gridCellsFact = Note(ctx: ctx, key: "dwork.gridCells", value: dwork.BackgroundGridCells.IfNone(0))
        let spectrumFact = Note(ctx: ctx, key: "dwork.spectrumValidated", value: algorithm.MeshSpectrumValidated)
        select Done(scope: scope);

    [RhinoScenario(theme: "vectors")]
    internal static Fin<Unit> SpectralDec(ScenarioContext ctx) {
        using Mesh native = Tetrahedron();
        using Mesh openNative = OpenSquare();
        using Mesh degenerateNative = DegenerateFaceMesh();
        using Mesh torusNative = TorusMesh(uCount: 8, vCount: 6);
        using Mesh annulusNative = Annulus(radialCount: 12, innerRadius: 1.0, outerRadius: 3.0);
        return SpectralDecRail(ctx: ctx, native: native, openNative: openNative, degenerateNative: degenerateNative, torusNative: torusNative, annulusNative: annulusNative);
    }

    [RhinoScenario(theme: "vectors")]
    internal static Fin<Unit> SpectralDescriptor(ScenarioContext ctx) {
        using Mesh native = Tetrahedron();
        return SpectralDescriptorRail(ctx: ctx, native: native);
    }

    [RhinoScenario(theme: "vectors")]
    internal static Fin<Unit> SpectralEdgeConnection(ScenarioContext ctx) {
        using Mesh native = OpenSquare();
        return SpectralEdgeConnectionRail(ctx: ctx, native: native);
    }

    private static Fin<CloudCurvatureResult> Curvature(ScenarioContext ctx, Context context, Op key, Seq<Point3d> points, CloudMetricPolicy policy, string label) =>
        ctx.Expect(label: $"{label} cluster", projection: VectorCloud.Cluster(points: points, context: context, key: key))
            .Bind(cloud => Project<CloudCurvatureResult>(ctx: ctx, intent: VectorIntent.Cloud(cloud: cloud, metric: VectorCloudMetric.PrincipalCurvature, policy: Some(policy), key: key), context: context, key: key, label: label));

    private static Mesh DegenerateFaceMesh() {
        Mesh mesh = new();
        _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 0.0, y: 1.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 1.0);
        _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 1.0, y: 1.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 1.0);
        _ = mesh.Vertices.Add(x: 2.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 2.0, y: 1.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 2.0, y: 0.0, z: 1.0);
        _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 2);
        _ = mesh.Faces.AddFace(vertex1: 3, vertex2: 4, vertex3: 5);
        _ = mesh.Faces.AddFace(vertex1: 6, vertex2: 7, vertex3: 8);
        _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 3, vertex3: 6);
        _ = mesh.Normals.ComputeNormals();
        return mesh;
    }

    private static (Point3d[] Points, PointCloud Cloud, int[][] Ids) DuplicateProbe() {
        Point3d[] points = [
            new Point3d(x: 0.0, y: 0.0, z: 0.0),
            new Point3d(x: 0.0, y: 0.0, z: 0.0),
            new Point3d(x: 1.0, y: 0.0, z: 0.0),
        ];
        PointCloud cloud = new(points);
        int[][] ids = [.. RTree.PointCloudKNeighbors(pointcloud: cloud, needlePts: points, amount: 3)];
        return (Points: points, Cloud: cloud, Ids: ids);
    }

    private static Seq<Point3d> Grid(double step, Func<double, double, double> z) =>
        toSeq(
            from ix in Enumerable.Range(start: -2, count: 5)
            from iy in Enumerable.Range(start: -2, count: 5)
            let px = ix * step
            let py = iy * step
            select new Point3d(x: px, y: py, z: z(px, py)));

    private static Unit Done(DocumentScope scope) {
        scope.Dispose();
        return unit;
    }

    private static T Note<T>(ScenarioContext ctx, string key, T value) {
        ctx.Fact(key: key, value: value);
        return value;
    }

    private static string Text(object? value) =>
        Convert.ToString(value: value, provider: System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

    private static Mesh OpenGrid() {
        Mesh mesh = Mesh.CreateFromPlane(
            plane: Plane.WorldXY,
            xInterval: new Interval(t0: -1.0, t1: 1.0),
            yInterval: new Interval(t0: -1.0, t1: 1.0),
            xCount: 8,
            yCount: 8);
        _ = mesh.Normals.ComputeNormals();
        _ = mesh.Compact();
        return mesh;
    }

    private static Mesh OpenSquare() {
        Mesh mesh = Mesh.CreateFromPlane(
            plane: Plane.WorldXY,
            xInterval: new Interval(t0: -1.0, t1: 1.0),
            yInterval: new Interval(t0: -1.0, t1: 1.0),
            xCount: 1,
            yCount: 1);
        _ = mesh.Normals.ComputeNormals();
        _ = mesh.Compact();
        return mesh;
    }

    private static Fin<T> Project<T>(ScenarioContext ctx, Fin<VectorIntent> intent, Context context, Op key, string label) =>
        ctx.Expect(label: $"{label}: intent", projection: intent)
            .Bind(admitted => ctx.Expect(label: $"{label}: project", projection: admitted.Project<T>(context: context, key: key)));

    private static Fin<Unit> SpectralDecRail(ScenarioContext ctx, Mesh native, Mesh openNative, Mesh degenerateNative, Mesh torusNative, Mesh annulusNative) =>
        from scope in DocumentScope.Open(ctx: ctx)
        from context in ctx.Expect(label: "context", projection: Context.Of(units: UnitSystem.Millimeters).ToFin())
        let op = Op.Of()
        from space in ctx.Expect(label: "space", projection: MeshSpace.Of(native: native, context: context, key: op))
        from calculus in Project<DiscreteCalculus>(ctx: ctx, intent: VectorIntent.DiscreteCalculus(space: space, key: op), context: context, key: op, label: "dec")
        let receipt = calculus.Receipt
        from openSpace in ctx.Expect(label: "open space", projection: MeshSpace.Of(native: openNative, context: context, key: op))
        from openReceipt in Project<SpectralAssemblyReceipt>(ctx: ctx, intent: VectorIntent.DiscreteCalculus(space: openSpace, key: op), context: context, key: op, label: "open dec")
        from flatten in Project<FlattenResult>(ctx: ctx, intent: VectorIntent.Flatten(space: openSpace, key: op), context: context, key: op, label: "flatten result")
        from flattenReceipt in Project<FlattenReceipt>(ctx: ctx, intent: VectorIntent.Flatten(space: openSpace, key: op), context: context, key: op, label: "flatten receipt")
        from simplify in ctx.Expect(label: "simplify kind", projection: RemeshKind.Simplify(parameters: new ReduceMeshParameters { DesiredPolygonCount = 1 }, key: op))
        from remesh in Project<RemeshResult>(ctx: ctx, intent: VectorIntent.Remesh(space: openSpace, kind: simplify, key: op), context: context, key: op, label: "remesh result")
        from remeshReceipt in Project<RemeshReceipt>(ctx: ctx, intent: VectorIntent.Remesh(space: openSpace, kind: simplify, key: op), context: context, key: op, label: "remesh receipt")
        from degenerateSpace in ctx.Expect(label: "degenerate space", projection: MeshSpace.Of(native: degenerateNative, context: context, key: op))
        from degenerateReceipt in Project<SpectralAssemblyReceipt>(ctx: ctx, intent: VectorIntent.DiscreteCalculus(space: degenerateSpace, key: op), context: context, key: op, label: "degenerate dec")
        from torusSpace in ctx.Expect(label: "torus space", projection: MeshSpace.Of(native: torusNative, context: context, key: op))
        from torusTopology in Project<TopologyReceipt>(ctx: ctx, intent: VectorIntent.Topology(space: torusSpace, key: op), context: context, key: op, label: "torus topology")
        from torusReceipt in Project<SpectralAssemblyReceipt>(ctx: ctx, intent: VectorIntent.DiscreteCalculus(space: torusSpace, key: op), context: context, key: op, label: "torus dec")
        from annulusSpace in ctx.Expect(label: "annulus space", projection: MeshSpace.Of(native: annulusNative, context: context, key: op))
        from annulusTopology in Project<TopologyReceipt>(ctx: ctx, intent: VectorIntent.Topology(space: annulusSpace, key: op), context: context, key: op, label: "annulus topology")
        from annulusCalculus in Project<DiscreteCalculus>(ctx: ctx, intent: VectorIntent.DiscreteCalculus(space: annulusSpace, key: op), context: context, key: op, label: "annulus dec")
        from annulusBasis in Project<HarmonicOneFormBasis>(ctx: ctx, intent: VectorIntent.DiscreteCalculus(space: annulusSpace, key: op), context: context, key: op, label: "annulus harmonic basis")
        from annulusHarmonic in Project<HarmonicOneFormReceipt>(ctx: ctx, intent: VectorIntent.DiscreteCalculus(space: annulusSpace, key: op), context: context, key: op, label: "annulus harmonic receipt")
        let genusPositiveHodgeProjected = VectorField.Hodge(source: VectorField.Constant(value: Vector3d.XAxis), space: torusSpace, key: op)
            .Bind(field => VectorIntent.Probe(source: ExtractionProbe.Vector(source: field), sample: torusNative.Vertices[0], key: op))
            .Bind(intent => intent.Project<Vector3d>(context: context, key: op)) switch {
                Fin<Vector3d>.Succ(Vector3d projected) => projected.IsValid,
                _ => false,
            }
        from featureAngle in ctx.Expect(label: "feature angle", projection: op.AcceptValidated<VectorAngle>(candidate: 0.1))
        from curvatureThreshold in ctx.Expect(label: "feature curvature", projection: op.AcceptValidated<PositiveMagnitude>(candidate: 0.01))
        from featureScale in ctx.Expect(label: "feature scale", projection: op.AcceptValidated<PositiveMagnitude>(candidate: 1.0))
        let curvaturePolicy = new MeshFeaturePolicy(DihedralThreshold: featureAngle, CurvatureThreshold: curvatureThreshold, SmoothingScale: featureScale, FaceRegions: Option<Arr<int>>.None)
        let regionPolicy = curvaturePolicy with { FaceRegions = Some(new Arr<int>([0, 1, 0, 1])) }
        from curvatureFeatures in Project<FeatureReceipt>(ctx: ctx, intent: VectorIntent.Features(space: space, policy: curvaturePolicy, key: op), context: context, key: op, label: "curvature features")
        from regionFeatures in Project<FeatureReceipt>(ctx: ctx, intent: VectorIntent.Features(space: space, policy: regionPolicy, key: op), context: context, key: op, label: "region features")
        let faceValues = new Arr<double>([0.0, 0.25, 1.0, 1.25])
        let identityDescriptor = MeshDescriptor.Spectral(filter: SpectralFilter.Identity)
        from thresholdKind in ctx.Expect(label: "threshold segmentation", projection: MeshSegmentation.ScalarThreshold(values: faceValues, threshold: 0.5, key: op))
        from bandsKind in ctx.Expect(label: "band segmentation", projection: MeshSegmentation.ScalarBands(values: faceValues, bandCount: 2, key: op))
        from growKind in ctx.Expect(label: "grow segmentation", projection: MeshSegmentation.SeededRegionGrow(values: faceValues, seedFaces: Seq(0, 2), tolerance: 0.4, maxIterations: 16, key: op))
        from clustersKind in ctx.Expect(label: "descriptor segmentation", projection: MeshSegmentation.DescriptorClusters(descriptor: identityDescriptor, eigenpairs: 3, regionCount: 2, maxIterations: 16, tolerance: 1.0e-9, key: op))
        from watershedKind in ctx.Expect(label: "watershed segmentation", projection: MeshSegmentation.Watershed(values: faceValues, mergeTolerance: 0.1, key: op))
        from ncutKind in ctx.Expect(label: "ncut segmentation", projection: MeshSegmentation.NormalizedCut(values: faceValues, regionCount: 2, eigenpairs: 2, maxIterations: 16, tolerance: 1.0e-9, key: op))
        from thresholdReceipt in Project<MeshSegmentationReceipt>(ctx: ctx, intent: VectorIntent.Segmentation(space: space, kind: thresholdKind, key: op), context: context, key: op, label: "threshold receipt")
        from bandsReceipt in Project<MeshSegmentationReceipt>(ctx: ctx, intent: VectorIntent.Segmentation(space: space, kind: bandsKind, key: op), context: context, key: op, label: "bands receipt")
        from growReceipt in Project<MeshSegmentationReceipt>(ctx: ctx, intent: VectorIntent.Segmentation(space: space, kind: growKind, key: op), context: context, key: op, label: "grow receipt")
        from clustersReceipt in Project<MeshSegmentationReceipt>(ctx: ctx, intent: VectorIntent.Segmentation(space: space, kind: clustersKind, key: op), context: context, key: op, label: "clusters receipt")
        from watershedReceipt in Project<MeshSegmentationReceipt>(ctx: ctx, intent: VectorIntent.Segmentation(space: space, kind: watershedKind, key: op), context: context, key: op, label: "watershed receipt")
        from ncutReceipt in Project<MeshSegmentationReceipt>(ctx: ctx, intent: VectorIntent.Segmentation(space: space, kind: ncutKind, key: op), context: context, key: op, label: "ncut receipt")
        let segmentationReceipts = Seq(thresholdReceipt, bandsReceipt, growReceipt, clustersReceipt, watershedReceipt, ncutReceipt)
        from meshDomain in ctx.Expect(label: "mesh domain", projection: ExtractionDomain.Mesh(value: space, key: op))
        from meshSampleKind in ctx.Expect(label: "mesh sample kind", projection: SampleKind.Capacity(count: 2, capacity: 4, iterations: 2, tolerance: 1.0, key: op))
        from meshSample in Project<SampleReceipt>(ctx: ctx, intent: VectorIntent.Sample(domain: meshDomain, kind: meshSampleKind, key: op), context: context, key: op, label: "mesh sample receipt")
        from sampleAlgorithm in ctx.Expect(label: "mesh sample algorithm", projection: meshSample.Algorithm.ToFin(Fail: Error.New(message: "mesh sample algorithm missing")))
        from sampleSpectrum in ctx.Expect(label: "mesh sample spectrum", projection: sampleAlgorithm.Spectrum.ToFin(Fail: Error.New(message: "mesh sample spectrum missing")))
        from logField in ctx.Expect(label: "tangent log map field", projection: VectorField.TangentLogMap(space: space, source: 0, time: 0.05, key: op))
        from logReceipt in Project<TangentLogMapReceipt>(ctx: ctx, intent: VectorIntent.Probe(source: ExtractionProbe.Vector(source: logField), sample: native.Vertices[1], key: op), context: context, key: op, label: "tangent log receipt")
        from validDec in ctx.Require(label: "valid DEC", observed: calculus.IsValid)
        from countsLaw in ctx.Require(label: "dec counts", observed: receipt.VertexCount == 4 && receipt.EdgeCount == 6 && receipt.FaceCount == 4)
        from compositionLaw in ctx.Require(label: "d1d0 residual", observed: receipt.BoundaryCompositionResidual <= RhinoMath.SqrtEpsilon)
        from starsLaw in ctx.Require(label: "positive stars", observed: receipt.PositiveStar0Count == 4 && receipt.PositiveStar2Count == 4)
        from genusLaw in ctx.Require(label: "genus zero", observed: receipt.Genus.Exists(static genus => genus == 0))
        from closedTopologyLaw in ctx.Require(label: "closed topology", observed: receipt.BoundaryEdgeCount == 0 && receipt.BoundaryComponentCount == 0 && receipt.NonManifoldEdgeCount == 0 && receipt.EulerCharacteristic == 2 && receipt.TopologyEulerValidated)
        from openLaw in ctx.Require(label: "open receipt", observed: openReceipt.BoundaryEdgeCount > 0 && openReceipt.BoundaryComponentCount > 0 && openReceipt.HarmonicDimension == 0)
        from flattenLaw in ctx.Require(label: "flatten valid", observed: flatten.Mesh.IsValid && flatten.Receipt.Valid && flattenReceipt.Valid)
        from distortionLaw in ctx.Require(label: "flatten distortion", observed: flattenReceipt.EdgeLengthDistortionRms.Exists(static rms => RhinoMath.IsValidDouble(x: rms) && rms >= 0.0))
        from remeshLaw in ctx.Require(label: "remesh completed", observed: remesh.Mesh.IsValid && remesh.Receipt.Status.Equals(other: RemeshStatus.Completed) && remeshReceipt.Status.Equals(other: RemeshStatus.Completed))
        from degenerateLaw in ctx.Require(label: "degenerate receipt", observed: degenerateReceipt.Kind.Equals(other: SpectralAssemblyKind.Dec) && degenerateReceipt.SkippedDegenerateFaces > 0)
        from torusLaw in ctx.Require(label: "torus topology", observed: torusTopology.Genus.Exists(static genus => genus > 0) && torusReceipt.Genus.Exists(static genus => genus == 1) && torusReceipt.HarmonicDimension == 2 && torusReceipt.IsValid && genusPositiveHodgeProjected)
        from annulusTopologyLaw in ctx.Require(label: "annulus topology", observed: annulusTopology.Genus.Exists(static genus => genus == 0) && annulusTopology.BoundaryComponents == 2 && annulusCalculus.Receipt.BoundaryComponentCount == 2 && annulusCalculus.Receipt.HarmonicDimension == 1)
        from annulusHarmonicLaw in ctx.Require(label: "annulus harmonic forms", observed: annulusBasis.Receipt.ExpectedDimension == 1 && annulusBasis.Forms.Count == 1 && annulusBasis.Forms.AsIterable().All(form => form.Count == annulusCalculus.Receipt.EdgeCount) && annulusBasis.IsValid)
        from annulusOrthonormalLaw in ctx.Require(label: "annulus star1 orthonormal", observed: annulusHarmonic.BoundaryComponentCount == 2 && annulusHarmonic.ExpectedDimension == 1 && annulusHarmonic.BasisCount == 1 && annulusHarmonic.Star1OrthonormalResidual <= Math.Max(val1: 1.0e-7, val2: annulusHarmonic.SvdTolerance * 1.0e3) && annulusHarmonic.MaxClosedResidual <= Math.Max(val1: 1.0e-7, val2: annulusHarmonic.SvdTolerance * 1.0e3) && annulusHarmonic.MaxCoClosedResidual <= Math.Max(val1: 1.0e-7, val2: annulusHarmonic.SvdTolerance * 1.0e3))
        from curvatureFeatureLaw in ctx.Require(label: "curvature features", observed: curvatureFeatures.Algorithm?.Equals(other: MeshFeatureAlgorithm.DihedralProxy) == true && curvatureFeatures.Edges.Count > 0 && curvatureFeatures.Edges.AsIterable().Any(static edge => edge.SignedDihedralRadians.IsSome))
        from regionFeatureLaw in ctx.Require(label: "region features", observed: regionFeatures.Algorithm?.Equals(other: MeshFeatureAlgorithm.DihedralProxy) == true && regionFeatures.RegionBoundaryEdges > 0 && regionFeatures.Edges.AsIterable().Any(static edge => edge.Kind.Equals(other: MeshFeatureKind.RegionBoundary)))
        from segmentationLaw in ctx.Require(label: "segmentation rails", observed: segmentationReceipts.Map(f: static row => row.Algorithm.Key).Distinct().Count == 6 && segmentationReceipts.ForAll(static row => row.AssignedFaceCount > 0 && (row.Status.Equals(other: MeshSegmentationStatus.Completed) || row.Status.Equals(other: MeshSegmentationStatus.MaxIterationsExhausted))))
        from sampleLaw in ctx.Require(label: "sample spectrum", observed: sampleAlgorithm.Kind.Equals(other: SampleAlgorithmKind.CapacityLimitedLloydCandidate) && sampleAlgorithm.CapacityResidual.Exists(static r => RhinoMath.IsValidDouble(x: r) && r >= 0.0) && sampleAlgorithm.CapacityAssignedCandidates.Exists(static a => a > 0) && !sampleAlgorithm.TransportAssignmentValidated && sampleAlgorithm.MeshSpectrumValidated == sampleSpectrum.Validated && sampleSpectrum.Algorithm?.Equals(other: MeshSamplingSpectrumAlgorithm.CandidateSpectrum) == true && sampleSpectrum.SuppressionRatio is >= 0.0 and <= 1.0 && sampleSpectrum.SampleCount == meshSample.Emitted)
        from logLaw in ctx.Require(label: "tangent log receipt", observed: logReceipt.VectorHeatBacked && logReceipt.RejectsFlippedIntrinsic && logReceipt.Algorithm?.Equals(other: TangentLogMapAlgorithm.VectorHeatApproximate) == true && logReceipt.FiniteLogCount == 1)
        let nonZerosFact = Note(ctx: ctx, key: "dec.nonzeros", value: receipt.NonZeros)
        let residualFact = Note(ctx: ctx, key: "dec.d1d0", value: receipt.BoundaryCompositionResidual)
        let boundaryFact = Note(ctx: ctx, key: "open.boundaryEdges", value: openReceipt.BoundaryEdgeCount)
        let distortionFact = Note(ctx: ctx, key: "flatten.distortion", value: flattenReceipt.EdgeLengthDistortionRms.IfNone(-1.0))
        let remeshFact = Note(ctx: ctx, key: "remesh.faces", value: remeshReceipt.PostFaceCount)
        let genusFact = Note(ctx: ctx, key: "torus.genus", value: torusTopology.Genus.IfNone(-1))
        let annulusBoundaryFact = Note(ctx: ctx, key: "annulus.boundaryComponents", value: annulusTopology.BoundaryComponents)
        let annulusDimensionFact = Note(ctx: ctx, key: "annulus.harmonicDimension", value: annulusCalculus.Receipt.HarmonicDimension)
        let annulusOrthonormalFact = Note(ctx: ctx, key: "annulus.star1Orthonormal", value: annulusHarmonic.Star1OrthonormalResidual)
        let regionFact = Note(ctx: ctx, key: "features.regionBoundary", value: regionFeatures.RegionBoundaryEdges)
        let railsFact = Note(ctx: ctx, key: "segmentation.rails", value: segmentationReceipts.Count)
        let spectrumFact = Note(ctx: ctx, key: "sample.spectrumValidated", value: sampleSpectrum.Validated)
        let logFact = Note(ctx: ctx, key: "log.vectorHeatBacked", value: logReceipt.VectorHeatBacked)
        select Done(scope: scope);

    private static Fin<Unit> SpectralDescriptorRail(ScenarioContext ctx, Mesh native) =>
        from scope in DocumentScope.Open(ctx: ctx)
        from context in ctx.Expect(label: "context", projection: Context.Of(units: UnitSystem.Millimeters).ToFin())
        let op = Op.Of()
        from space in ctx.Expect(label: "space", projection: MeshSpace.Of(native: native, context: context, key: op))
        let policy = new SpectralDescriptorPolicy(
            ScaleNormalization: SpectralScaleNormalization.FirstNonZeroEigenvalue,
            EnergyNormalization: SpectralEnergyNormalization.UnitL2,
            ZeroModePolicy: SpectralZeroModePolicy.Drop,
            CropCount: Some(Dimension.Create(value: 2)))
        let descriptor = MeshDescriptor.Spectral(filter: SpectralFilter.Identity, sources: Option<Seq<int>>.None, policy: policy)
        from intent in ctx.Expect(label: "descriptor intent", projection: VectorIntent.Descriptor(space: space, kind: descriptor, pairs: 3, key: op))
        from result in ctx.Expect(label: "descriptor result", projection: intent.Project<DescriptorResult>(context: context, key: op))
        from meshReceipt in ctx.Expect(label: "descriptor receipt", projection: intent.Project<DescriptorReceipt>(context: context, key: op))
        from spectral in ctx.Expect(label: "spectral descriptor", projection: intent.Project<SpectralDescriptor>(context: context, key: op))
        from spectralReceipt in ctx.Expect(label: "spectral receipt", projection: intent.Project<SpectralDescriptorReceipt>(context: context, key: op))
        from values in ctx.Expect(label: "descriptor values", projection: intent.Project<Arr<double>>(context: context, key: op))
        from waveEnergy in ctx.Expect(label: "wave energy", projection: op.AcceptValidated<PositiveMagnitude>(candidate: 2.0))
        from waveBandwidth in ctx.Expect(label: "wave bandwidth", projection: op.AcceptValidated<PositiveMagnitude>(candidate: 0.75))
        let waveDescriptor = MeshDescriptor.Spectral(filter: SpectralFilter.Wave(energy: waveEnergy, bandwidth: waveBandwidth), sources: Option<Seq<int>>.None, policy: SpectralDescriptorPolicy.Raw)
        from waveIntent in ctx.Expect(label: "wave descriptor intent", projection: VectorIntent.Descriptor(space: space, kind: waveDescriptor, pairs: 3, key: op))
        from waveReceipt in ctx.Expect(label: "wave spectral receipt", projection: waveIntent.Project<SpectralDescriptorReceipt>(context: context, key: op))
        from wave in ctx.Expect(label: "wave receipt", projection: waveReceipt.Wave.ToFin(Fail: Error.New(message: "wave receipt missing")))
        from valueCountLaw in ctx.Require(label: "value count", observed: result.Values.Count == native.Vertices.Count && spectral.Values.Count == result.Values.Count && values.Count == result.Values.Count)
        from finiteLaw in ctx.Require(label: "finite descriptor values", observed: result.Values.ForAll(static value => RhinoMath.IsValidDouble(x: value)))
        from meshReceiptLaw in ctx.Require(label: "mesh receipt", observed: meshReceipt.RequestedEigenpairs == 3 && meshReceipt.ReturnedEigenpairs > 0 && meshReceipt.Eigen.IsUsable)
        from assemblyLaw in ctx.Require(label: "assembly facts", observed: meshReceipt.Assembly.IsSome)
        from policyLaw in ctx.Require(label: "mesh policy", observed: meshReceipt.Spectral.Policy.ScaleNormalization.Equals(other: policy.ScaleNormalization) && meshReceipt.Spectral.Policy.EnergyNormalization.Equals(other: policy.EnergyNormalization))
        from spectralLaw in ctx.Require(label: "spectral receipt", observed: spectralReceipt.Policy.ZeroModePolicy.Equals(other: policy.ZeroModePolicy) && spectralReceipt.ComparisonReady && spectralReceipt.EnergyNormalized && spectralReceipt.ScaleNormalized && spectralReceipt.Wave.IsNone)
        from spectralCountLaw in ctx.Require(label: "spectral counts", observed: spectralReceipt.VertexCount == native.Vertices.Count && spectralReceipt.EigenpairCount == meshReceipt.Spectral.EigenpairCount)
        from waveLaw in ctx.Require(label: "wave receipt laws", observed: !waveReceipt.ScaleNormalized && wave.WksNormalized && wave.NormalizedWeightSum is > 0.999999999 and < 1.000000001 && wave.NonZeroEigenpairCount > 0)
        let valuesFact = Note(ctx: ctx, key: "descriptor.values", value: result.Values.Count)
        let eigenpairsFact = Note(ctx: ctx, key: "descriptor.returnedEigenpairs", value: meshReceipt.ReturnedEigenpairs)
        let cacheFact = Note(ctx: ctx, key: "descriptor.cacheHit", value: meshReceipt.SpectralCacheHit)
        let assemblyFact = Note(ctx: ctx, key: "descriptor.hasAssembly", value: meshReceipt.Assembly.IsSome)
        let comparisonFact = Note(ctx: ctx, key: "spectral.comparisonReady", value: spectralReceipt.ComparisonReady)
        let waveNormalizedFact = Note(ctx: ctx, key: "spectral.waveNormalized", value: wave.WksNormalized)
        let waveSumFact = Note(ctx: ctx, key: "spectral.waveWeightSum", value: wave.NormalizedWeightSum)
        select Done(scope: scope);

    private static Fin<Unit> SpectralEdgeConnectionRail(ScenarioContext ctx, Mesh native) =>
        from scope in DocumentScope.Open(ctx: ctx)
        from context in ctx.Expect(label: "context", projection: Context.Of(units: UnitSystem.Millimeters).ToFin())
        let op = Op.Of()
        from space in ctx.Expect(label: "space", projection: MeshSpace.Of(native: native, context: context, key: op))
        from solver in ctx.Expect(label: "solver policy", projection: VolumeSolverPolicy.SparseCholesky(residualTolerance: 1.0e-4, key: op))
        from policy in ctx.Expect(label: "boundary policy", projection: SdfMeshPolicy.BoundarySignedHeat(solver: solver, key: op))
        from field in ctx.Expect(label: "boundary signed heat field", projection: ScalarField.SignedDistanceFromMesh(space: space, policy: policy, key: op))
        from sample in ctx.Expect(label: "boundary signed heat sample", projection: field.SampleSdfDetailed(sample: new Point3d(x: 0.25, y: 0.25, z: 0.1), context: context, key: op))
        from mesh in ctx.Expect(label: "mesh receipt", projection: sample.Receipt.Mesh.ToFin(Fail: Error.New(message: "mesh receipt missing")))
        from signed in ctx.Expect(label: "signed heat receipt", projection: mesh.SignedHeat.ToFin(Fail: Error.New(message: "signed heat receipt missing")))
        from edge in ctx.Expect(label: "edge assembly receipt", projection: signed.EdgeAssembly.ToFin(Fail: Error.New(message: "edge assembly receipt missing")))
        from heat in ctx.Expect(label: "heat solve", projection: signed.HeatSolve.ToFin(Fail: Error.New(message: "heat solve missing")))
        from spaceLaw in ctx.Require(label: "edge space", observed: edge.Kind == SpectralAssemblyKind.EdgeConnection && edge.EdgeCount > 0 && edge.ComponentCount == 2)
        from shapeLaw in ctx.Require(label: "edge shape", observed: edge.MatrixRows == edge.EdgeCount * edge.ComponentCount && edge.MatrixCols == edge.MatrixRows)
        from facesLaw in ctx.Require(label: "edge faces", observed: edge.AdmittedFaceCount + edge.SkippedDegenerateFaces == edge.FaceCount)
        from massLaw in ctx.Require(label: "edge mass", observed: edge.PositiveMassCount > 0 && edge.PositiveMassCount <= edge.EdgeCount)
        from factorLaw in ctx.Require(label: "edge factor", observed: edge.SymmetryResidual <= RhinoMath.SqrtEpsilon && edge.FactorNonZeros.IsSome)
        from solveLaw in ctx.Require(label: "solves usable", observed: heat.IsUsable && signed.PoissonSolve.IsUsable)
        let dofsFact = Note(ctx: ctx, key: "edgeDofs", value: edge.EdgeCount)
        let rowsFact = Note(ctx: ctx, key: "edgeRows", value: edge.MatrixRows)
        let nonZerosFact = Note(ctx: ctx, key: "edgeNonZeros", value: edge.NonZeros)
        let factorFact = Note(ctx: ctx, key: "edgeFactorNonZeros", value: edge.FactorNonZeros.IfNone(0))
        let toleranceFact = Note(ctx: ctx, key: "solverTolerance", value: solver.ResidualTolerance.Value)
        select Done(scope: scope);

    private static Mesh Annulus(int radialCount, double innerRadius, double outerRadius) {
        Mesh mesh = new();
        foreach (int i in Enumerable.Range(start: 0, count: radialCount)) {
            double theta = 2.0 * Math.PI * i / radialCount;
            _ = mesh.Vertices.Add(x: innerRadius * Math.Cos(d: theta), y: innerRadius * Math.Sin(a: theta), z: 0.0);
            _ = mesh.Vertices.Add(x: outerRadius * Math.Cos(d: theta), y: outerRadius * Math.Sin(a: theta), z: 0.0);
        }
        foreach (int i in Enumerable.Range(start: 0, count: radialCount)) {
            int inner = 2 * i;
            int outer = inner + 1;
            int nextInner = 2 * ((i + 1) % radialCount);
            int nextOuter = nextInner + 1;
            _ = mesh.Faces.AddFace(vertex1: inner, vertex2: outer, vertex3: nextOuter, vertex4: nextInner);
        }
        _ = mesh.Normals.ComputeNormals();
        _ = mesh.Compact();
        return mesh;
    }

    private static Mesh TorusMesh(int uCount, int vCount) {
        Mesh mesh = new();
        const double major = 3.0;
        const double minor = 0.75;
        foreach (int u in Enumerable.Range(start: 0, count: uCount)) {
            double theta = 2.0 * Math.PI * u / uCount;
            foreach (int v in Enumerable.Range(start: 0, count: vCount)) {
                double phi = 2.0 * Math.PI * v / vCount;
                double radius = major + (minor * Math.Cos(d: phi));
                _ = mesh.Vertices.Add(x: radius * Math.Cos(d: theta), y: radius * Math.Sin(a: theta), z: minor * Math.Sin(a: phi));
            }
        }
        foreach (int u in Enumerable.Range(start: 0, count: uCount)) {
            foreach (int v in Enumerable.Range(start: 0, count: vCount)) {
                int a = (u * vCount) + v;
                int b = ((u + 1) % uCount * vCount) + v;
                int c = ((u + 1) % uCount * vCount) + ((v + 1) % vCount);
                int d = (u * vCount) + ((v + 1) % vCount);
                _ = mesh.Faces.AddFace(vertex1: a, vertex2: b, vertex3: c, vertex4: d);
            }
        }
        _ = mesh.Normals.ComputeNormals();
        _ = mesh.Compact();
        return mesh;
    }

    private static Mesh Tetrahedron() {
        Mesh mesh = new();
        _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 0.0, y: 1.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 1.0);
        _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 2, vertex3: 1);
        _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 3);
        _ = mesh.Faces.AddFace(vertex1: 1, vertex2: 2, vertex3: 3);
        _ = mesh.Faces.AddFace(vertex1: 2, vertex2: 0, vertex3: 3);
        _ = mesh.Normals.ComputeNormals();
        _ = mesh.Compact();
        return mesh;
    }
}
