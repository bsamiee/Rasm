using System.Globalization;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.RhinoCommon.Document.Units;
using Rasm.RhinoCommon.Modeling.Curves;

namespace Rasm.RhinoCommon.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CloudCurves {
    public static readonly CloudCurves Splines = new(createSpline: true, createPolyline: false);
    public static readonly CloudCurves Polylines = new(createSpline: false, createPolyline: true);
    public static readonly CloudCurves Both = new(createSpline: true, createPolyline: true);

    public bool CreateSpline { get; }

    public bool CreatePolyline { get; }
}

public sealed record CloudSlab(Interval Distance, bool OpenCurves, CloudCurves Curves, double FitTolerance);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Points {
    // --- [CLOUDS]
    public static IO<PointCloud> FromMesh(Mesh mesh) => unitIO.Bind(_ => Products.Made(PointCloud.CreateFromMesh(mesh)));

    public static IO<PointCloud> Subsample(PointCloud cloud, uint numberOfPoints, Option<IProgress<double>> progress) =>
        from token in cancelToken
        from sample in Products.Made(cloud.GetRandomSubsample(numberOfPoints, token, progress.ValueUnsafe()))
        select sample;

    public static IO<(PointCloud Above, PointCloud Below)> Split(PointCloud cloud, Plane plane) =>
        from split in IO.lift(() => (Accepted: cloud.Split(plane, out PointCloud? above, out PointCloud? below), Above: above, Below: below))
        from heldAbove in Products.Held(split.Above)
        from heldBelow in Products.Held(split.Below)
        from accepted in IO.lift(() => RhinoFault.Unless(split.Accepted))
        from upper in IO.lift(() => Conversions.Found(heldAbove))
        from lower in IO.lift(() => Conversions.Found(heldBelow))
        select (Above: upper, Below: lower);

    // --- [NORMALS]
    public static IO<PointCloud> Normals(PointCloud cloud, int neighbours) =>
        from count in IO.lift(() => neighbours >= 3 ? Fin.Succ(neighbours) : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"neighbours {neighbours}"), RhinoReason.Outside))
        from estimated in IO.lift(() => Estimated(cloud, count))
        from copy in Copies.Duplicate(cloud)
        from written in IO.lift(() => estimated.Iter((index, normal) => copy[index].Normal = normal))
        select copy;

    private static Fin<Seq<Vector3d>> Estimated(PointCloud cloud, int neighbours) =>
        cloud.GetPoints() switch {
            var locations => toSeq(RTree.PointCloudKNeighbors(cloud, locations, neighbours)).Strict() switch {
                var near => Callbacks.Each(near, (indices, at) =>
                        RhinoFault.Unless(Plane.FitPlaneToPoints(indices.Select(index => locations[index]), out Plane plane) is PlaneFitResult.Success, at).Map(_ => plane.ZAxis))
                    .Map(fitted => Oriented(fitted, near)),
            },
        };

    private static Seq<Vector3d> Oriented(Seq<Vector3d> fitted, Seq<int[]> near) {
        Vector3d[] oriented = [.. fitted];
        UndirectedDepthFirstSearchAlgorithm<int, SEquatableEdge<int>> walk = new(
            near.SelectMany(static (indices, at) => indices.Where(index => index != at).Select(index => new SEquatableEdge<int>(Math.Min(at, index), Math.Max(at, index))))
                .Distinct()
                .ToUndirectedGraph()
                .MinimumSpanningTreeKruskal(edge => 1d - Math.Abs(fitted[edge.Source] * fitted[edge.Target]))
                .ToUndirectedGraph());
        walk.TreeEdge += (_, step) => oriented[step.Target] = oriented[step.Source] * fitted[step.Target] < 0d ? -fitted[step.Target] : fitted[step.Target];
        walk.Compute();
        return toSeq(oriented);
    }

    // --- [SECTIONS]
    public static IO<Seq<Curve>> Section(Tolerances tolerances, PointCloud cloud, CloudSlab slab, Plane plane) =>
        Products.Made(cloud.CreateSectionCurve(
            plane, tolerances.Absolute, slab.Distance.Max, slab.Distance.Min, slab.OpenCurves, slab.Curves.CreateSpline, slab.Curves.CreatePolyline, slab.FitTolerance));

    public static IO<Seq<Curve>> Contours(Tolerances tolerances, PointCloud cloud, CloudSlab slab, Point3d startPoint, Point3d endPoint, double interval) =>
        Products.Made(cloud.CreateContourCurves(
            startPoint, endPoint, interval, tolerances.Absolute, slab.Distance.Max, slab.Distance.Min, slab.OpenCurves, slab.Curves.CreateSpline, slab.Curves.CreatePolyline, slab.FitTolerance));

    // --- [POINTS]
    public static Fin<Seq<Point3d>> CullDuplicates(Tolerances tolerances, IterableNE<Point3d> points) =>
        Conversions.Found(Point3d.CullDuplicates(points, tolerances.Absolute)).Map(static culled => toSeq(culled));

    public static Fin<Seq<Point3d>> SortAndCull(IterableNE<Point3d> points, double minimumDistance) =>
        Conversions.Found(Point3d.SortAndCullPointList(points, minimumDistance)).Map(static sorted => toSeq(sorted));

    public static bool Coplanar(Tolerances tolerances, IterableNE<Point3d> points) =>
        Point3d.ArePointsCoplanar(points, tolerances.Absolute);

    // --- [ENCLOSURES]
    public static IO<(PolylineCurve Hull, Seq<int> Indices)> ConvexHull(Seq<Point2d> points) =>
        unitIO.Bind(_ => Products.Made(PolylineCurve.CreateConvexHull2d([.. points], out int[] hullIndices)).Map(hull => (Hull: hull, Indices: toSeq(hullIndices))));

    public static Fin<(Circle Circle, Seq<int> OnCircle)> EnclosingCircle(Tolerances tolerances, Seq<Point2d> points) =>
        RhinoFault.Unless(Circle.TrySmallestEnclosingCircle(points, tolerances.Absolute, out Circle circle, out int[] indicesOnCircle))
            .Map(_ => (Circle: circle, OnCircle: toSeq(indicesOnCircle)));

    // --- [TANGENCY]
    public static Fin<Circle> TangentCircle(Curve first, CurveAddress nearFirst, Curve second, CurveAddress nearSecond) =>
        (CurveEvaluation.Resolve(first, nearFirst), CurveEvaluation.Resolve(second, nearSecond))
            .Apply((t1, t2) => Circle.TryFitCircleTT(first, second, t1, t2)).As()
            .Bind(static circle => RhinoFault.Unless(circle.IsValid).Map(_ => circle));

    public static Fin<Circle> TangentCircle(Curve first, CurveAddress nearFirst, Curve second, CurveAddress nearSecond, Curve third, CurveAddress nearThird) =>
        (CurveEvaluation.Resolve(first, nearFirst), CurveEvaluation.Resolve(second, nearSecond), CurveEvaluation.Resolve(third, nearThird))
            .Apply((t1, t2, t3) => Circle.TryFitCircleTTT(first, second, third, t1, t2, t3)).As()
            .Bind(static circle => RhinoFault.Unless(circle.IsValid).Map(_ => circle));
}
