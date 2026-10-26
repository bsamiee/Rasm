using System.Globalization;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SearchDistance : System.Numerics.IMinMaxValue<SearchDistance> {
    public static SearchDistance MinValue { get; } = new(double.BitIncrement(0d));
    public static SearchDistance MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

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
    public static IO<PointCloud> FromMesh(Mesh mesh, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(PointCloud.CreateFromMesh), _ => Answer.One(PointCloud.CreateFromMesh(mesh)), retain);

    public static IO<PointCloud> Subsample(PointCloud cloud, uint numberOfPoints, Option<IProgress<double>> progress, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(PointCloud.GetRandomSubsample), token => Answer.One(cloud.GetRandomSubsample(numberOfPoints, token, progress.ValueUnsafe())), retain);

    public static IO<(PointCloud Above, PointCloud Below)> Split(PointCloud cloud, Plane plane, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(PointCloud.Split), _ =>
            (Answer.Accepted(cloud.Split(plane, out PointCloud? above, out PointCloud? below)), Answer.One(above), Answer.One(below))
                .Apply(static (_, upper, lower) => (Above: upper, Below: lower)).As(), retain);

    // --- [NORMALS]
    public static IO<PointCloud> Normals(PointCloud cloud, int neighbours, Action<IDisposable> retain) =>
        from count in IO.lift(() => neighbours >= 3
            ? Fin.Succ(neighbours)
            : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{nameof(Plane.FitPlaneToPoints)} needs 3 or more neighbours per point, received {neighbours}")))
        from estimated in IO.lift(() => Estimated(cloud, count))
        from copy in Copies.Duplicate(cloud, retain)
        from written in IO.lift(() => estimated.Iter((index, normal) => copy[index].Normal = normal))
        select copy;

    private static Fin<Seq<Vector3d>> Estimated(PointCloud cloud, int neighbours) =>
        cloud.GetPoints() switch {
            var locations => toSeq(RTree.PointCloudKNeighbors(cloud, locations, neighbours)).Strict() switch {
                var near => Callbacks.Each(near, (indices, at) => Refused.Unless(
                        Plane.FitPlaneToPoints(indices.Select(index => locations[index]), out Plane plane) is PlaneFitResult.Success,
                        plane.ZAxis,
                        nameof(Plane.FitPlaneToPoints),
                        at))
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
    public static IO<Seq<Curve>> Section(Tolerances tolerances, PointCloud cloud, CloudSlab slab, Plane plane, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(PointCloud.CreateSectionCurve), _ => Answer.Many(cloud.CreateSectionCurve(
            plane, tolerances.Absolute, slab.Distance.Max, slab.Distance.Min, slab.OpenCurves, slab.Curves.CreateSpline, slab.Curves.CreatePolyline, slab.FitTolerance)), retain);

    public static IO<Seq<Curve>> Contours(Tolerances tolerances, PointCloud cloud, CloudSlab slab, Point3d startPoint, Point3d endPoint, double interval, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(PointCloud.CreateContourCurves), _ => Answer.Many(cloud.CreateContourCurves(
            startPoint, endPoint, interval, tolerances.Absolute, slab.Distance.Max, slab.Distance.Min, slab.OpenCurves, slab.Curves.CreateSpline, slab.Curves.CreatePolyline, slab.FitTolerance)), retain);

    // --- [POINTS]
    public static Fin<Seq<Point3d>> CullDuplicates(Tolerances tolerances, IterableNE<Point3d> points) =>
        Refused.UnlessFound(Point3d.CullDuplicates(points, tolerances.Absolute), nameof(Point3d.CullDuplicates)).Map(static culled => toSeq(culled));

    public static Fin<Seq<Point3d>> SortAndCull(IterableNE<Point3d> points, double minimumDistance) =>
        Refused.UnlessFound(Point3d.SortAndCullPointList(points, minimumDistance), nameof(Point3d.SortAndCullPointList)).Map(static sorted => toSeq(sorted));

    public static bool Coplanar(Tolerances tolerances, IterableNE<Point3d> points) =>
        Point3d.ArePointsCoplanar(points, tolerances.Absolute);

    // --- [ENCLOSURES]
    public static IO<(PolylineCurve Hull, Seq<int> Indices)> ConvexHull(Seq<Point2d> points, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(PolylineCurve.CreateConvexHull2d), _ =>
            Answer.One(PolylineCurve.CreateConvexHull2d([.. points], out int[] hullIndices)).Map(hull => (Hull: hull, Indices: toSeq(hullIndices))), retain);

    public static Fin<(Circle Circle, Seq<int> OnCircle)> EnclosingCircle(Tolerances tolerances, Seq<Point2d> points) =>
        Refused.Unless(
            Circle.TrySmallestEnclosingCircle(points, tolerances.Absolute, out Circle circle, out int[] indicesOnCircle),
            (Circle: circle, OnCircle: toSeq(indicesOnCircle)),
            nameof(Circle.TrySmallestEnclosingCircle));

    // --- [TANGENCY]
    public static Fin<Circle> TangentCircle(Curve first, CurveAddress nearFirst, Curve second, CurveAddress nearSecond) =>
        (CurveEvaluation.Resolve(first, nearFirst, nameof(Circle.TryFitCircleTT)), CurveEvaluation.Resolve(second, nearSecond, nameof(Circle.TryFitCircleTT)))
            .Apply((t1, t2) => Circle.TryFitCircleTT(first, second, t1, t2)).As()
            .Bind(static circle => Refused.Unless(circle.IsValid, circle, nameof(Circle.TryFitCircleTT)));

    public static Fin<Circle> TangentCircle(Curve first, CurveAddress nearFirst, Curve second, CurveAddress nearSecond, Curve third, CurveAddress nearThird) =>
        (CurveEvaluation.Resolve(first, nearFirst, nameof(Circle.TryFitCircleTTT)), CurveEvaluation.Resolve(second, nearSecond, nameof(Circle.TryFitCircleTTT)), CurveEvaluation.Resolve(third, nearThird, nameof(Circle.TryFitCircleTTT)))
            .Apply((t1, t2, t3) => Circle.TryFitCircleTTT(first, second, third, t1, t2, t3)).As()
            .Bind(static circle => Refused.Unless(circle.IsValid, circle, nameof(Circle.TryFitCircleTTT)));
}
