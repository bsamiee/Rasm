using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Points {
    // --- [CLOUDS]
    public static IO<(PointCloud Above, PointCloud Below)> Split(PointCloud cloud, Plane plane) =>
        from split in IO.lift(() => (Accepted: cloud.Split(plane, out PointCloud? above, out PointCloud? below), Above: above, Below: below))
        from above in Products.Of(() => split.Above)
        from below in Products.Of(() => split.Below)
        from accepted in Products.Accepted(() => split.Accepted)
        select (above, below);

    // --- [NORMALS]
    public static IO<PointCloud> Normals(PointCloud cloud, int neighbours) =>
        from estimated in IO.lift(() => Estimated(cloud, neighbours))
        from copy in Products.Of(() => cloud.Duplicate() as PointCloud)
        from written in IO.lift(() => estimated.Iter((index, normal) => copy[index].Normal = normal))
        select copy;

    private static Fin<Seq<Vector3d>> Estimated(PointCloud cloud, int neighbours) {
        Point3d[] locations = cloud.GetPoints();
        IEnumerable<int[]> found = RTree.PointCloudKNeighbors(cloud, locations, neighbours);
        using IDisposable? search = found as IDisposable;
        Seq<int[]> near = toSeq(found).Strict();
        return Products.Each(near, (indices, at) => RhinoFault.Unless(Plane.FitPlaneToPoints(indices.Select(index => locations[index]), out Plane plane) is PlaneFitResult.Success, RhinoReason.Declined, indices.Length, at)
                .Map(_ => plane.ZAxis))
            .Map(fitted => Oriented(fitted, near));
    }

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

    // --- [TANGENCY]
    public static Fin<Circle> TangentCircle(Curve first, CurveAddress nearFirst, Curve second, CurveAddress nearSecond) =>
        from circle in (CurveEvaluation.Resolve(first, nearFirst), CurveEvaluation.Resolve(second, nearSecond)).Apply((t1, t2) => Circle.TryFitCircleTT(first, second, t1, t2)).As()
        from valid in RhinoFault.Unless(circle.IsValid, RhinoReason.Declined, circle)
        select circle;

    public static Fin<Circle> TangentCircle(Curve first, CurveAddress nearFirst, Curve second, CurveAddress nearSecond, Curve third, CurveAddress nearThird) =>
        from circle in (CurveEvaluation.Resolve(first, nearFirst), CurveEvaluation.Resolve(second, nearSecond), CurveEvaluation.Resolve(third, nearThird))
            .Apply((t1, t2, t3) => Circle.TryFitCircleTTT(first, second, third, t1, t2, t3)).As()
        from valid in RhinoFault.Unless(circle.IsValid, RhinoReason.Declined, circle)
        select circle;
}
