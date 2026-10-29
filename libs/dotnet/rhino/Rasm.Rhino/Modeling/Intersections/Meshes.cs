using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Units;
using Rhino.FileIO;
using Rhino.Geometry.Intersect;

namespace Rasm.Rhino.Modeling.Intersections;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Sets<T> {
    private Sets(IEnumerable<T> first, IEnumerable<T>? second) => Operands = (first, second);

    public (IEnumerable<T> First, IEnumerable<T>? Second) Operands { get; }

    public sealed record Within(Seq<T> Items) : Sets<T>(Items, second: null);

    public sealed record Between(Seq<T> A, Seq<T> B) : Sets<T>(A, B);
}

public sealed record MeshCrossings(Seq<Polyline> Polylines, Seq<Polyline> Overlaps, Option<Mesh> OverlapMesh, Option<string> Log);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshIntersections {
    // --- [CROSSINGS]
    public static IO<MeshCrossings> MeshMesh(Tolerances tolerances, Sets<Mesh> meshes, bool overlaps, bool overlapMesh, Option<IProgress<double>> progress) =>
        from token in cancelToken
        from crossed in meshes.Switch(
            (Tolerance: tolerances.MeshIntersection, Overlaps: overlaps, OverlapMesh: overlapMesh, Progress: progress.ValueUnsafe(), Token: token),
            within: static (at, within) => Crossed(log => (
                Intersection.MeshMesh(within.Items, at.Tolerance, out Polyline[] polylines, at.Overlaps, out Polyline[] overlapped, at.OverlapMesh, out Mesh merged, log, at.Token, at.Progress),
                polylines, overlapped, merged)),
            between: static (at, between) => Crossed(log => (
                Intersection.MeshMeshTwoSets(between.A, between.B, at.Tolerance, out Polyline[] polylines, at.Overlaps, out Polyline[] overlapped, at.OverlapMesh, out Mesh merged, log, at.Token, at.Progress),
                polylines, overlapped, merged)))
        select crossed;

    public static IO<Seq<IntersectingMeshPair>> Pairs(Tolerances tolerances, Sets<Mesh> meshes, bool fast) =>
        from token in cancelToken
        from answered in IO.lift(() => (Found: Intersection.MeshMeshPredicate(meshes.Operands.First, meshes.Operands.Second, tolerances.MeshIntersection, fast, out IntersectingMeshPair[] pairs, textLog: null, token), Pairs: pairs))
        from settled in IO.lift(() => token.IsCancellationRequested ? Fin.Fail<Unit>(Errors.Cancelled) : unit)
        select answered.Found ? Conversions.Rows(answered.Pairs) : Seq<IntersectingMeshPair>();

    public static IO<bool> Intersects(Tolerances tolerances, Sets<Mesh> meshes, bool fast) =>
        from token in cancelToken
        from found in IO.lift(() => Intersection.MeshMeshPredicate(meshes.Operands.First, meshes.Operands.Second, tolerances.MeshIntersection, fast, textLog: null, token))
        from settled in IO.lift(() => token.IsCancellationRequested ? Fin.Fail<Unit>(Errors.Cancelled) : unit)
        select found;

    public static IO<Seq<Option<MeshClash>>> Clashes(Seq<Mesh> setA, Seq<Mesh> setB, double distance, int maxEvents) =>
        IO.lift(() => toSeq(MeshClash.Search(setA, setB, distance, maxEvents)).Map(static clash => Optional(clash.MeshA).Map(_ => clash)).Strict());

    private static IO<MeshCrossings> Crossed(Func<TextLog, (bool Accepted, Polyline[] Polylines, Polyline[] Overlaps, Mesh? OverlapMesh)> host, [CallerArgumentExpression(nameof(host))] string call = "") =>
        from crossed in (from log in use(static () => new TextLog())
                         from answered in IO.lift(() => host(log))
                         select (Answer: answered, Log: log.ToString())).Bracket()
        from mesh in Products.Maybe(() => crossed.Answer.OverlapMesh, call)
        from accepted in Products.Accepted(() => crossed.Answer.Accepted, call)
        select new MeshCrossings(Conversions.Rows(crossed.Answer.Polylines), Conversions.Rows(crossed.Answer.Overlaps), mesh, Conversions.Present(crossed.Log));

    // --- [POINTS]
    public static IO<Seq<(Point3d Point, int Index)>> Points(Func<(Point3d[]? Points, int[]? Indices)> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        IO.lift(call).Bind(answered => IO.lift(() =>
            from points in Optional(answered.Points).ToFin(new RhinoFault(member, RhinoReason.Missing))
            from indices in Optional(answered.Indices).ToFin(new RhinoFault(member, RhinoReason.Missing))
            from paired in RhinoFault.Unless(points.Length == indices.Length, RhinoReason.Mismatched, (points.Length, indices.Length))
            select toSeq(points).Zip(toSeq(indices), static (point, index) => (Point: point, Index: index)).Strict()));
}
