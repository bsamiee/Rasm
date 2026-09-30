using Rasm.Rhino.Document;
using Rhino.Commands;
using Rhino.Geometry.Intersect;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshBooleanMethod {
    public sealed record Union(Seq<Mesh> Meshes) : MeshBooleanMethod;

    public sealed record Difference(Seq<Mesh> First, Seq<Mesh> Second) : MeshBooleanMethod;

    public sealed record Intersection(Seq<Mesh> First, Seq<Mesh> Second) : MeshBooleanMethod;

    public sealed record Split(Seq<Mesh> Targets, Seq<Mesh> Cutters) : MeshBooleanMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshSplitMethod {
    public sealed record ByPlane(Plane Plane) : MeshSplitMethod;

    public sealed record ByMeshes(Seq<Mesh> Cutters, double AbsoluteTolerance, bool SplitAtCoplanar, bool CreateNgons, Option<IProgress<double>> Progress, CancellationToken Cancel) : MeshSplitMethod;

    public sealed record Disjoint() : MeshSplitMethod;

    public sealed record NonManifold() : MeshSplitMethod;

    public sealed record ByProjectedPolylines(Seq<PolylineCurve> Curves, double Tolerance) : MeshSplitMethod;

    public sealed record UnweldedEdges() : MeshSplitMethod;

    public sealed record ByCount(int MaxCount, bool CountSum, bool CountTriangles) : MeshSplitMethod;

    public sealed record Partition(int MaxVertices, int MaxFaces) : MeshSplitMethod;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshTopology {
    // --- [TOLERANCE]
    internal static double IntersectionTolerance(double absoluteTolerance) =>
        absoluteTolerance * Intersection.MeshIntersectionsTolerancesCoefficient;

    // --- [BOOLEANS]
    public static IO<Seq<(Mesh Mesh, Seq<int> Inputs)>> Boolean(MeshBooleanMethod method, double absoluteTolerance, Option<IProgress<double>> progress, CancellationToken cancel) =>
        from answer in IO.lift(() => method.Switch(
            new MeshBooleanOptions {
                Tolerance = IntersectionTolerance(absoluteTolerance),
                CancellationToken = cancel,
                ProgressReporter = progress.ValueUnsafe(),
            },
            union: static (options, of) => Invalid.Unless(!of.Meshes.IsEmpty, nameof(of.Meshes))
                .Map(_ => (Meshes: Mesh.CreateBooleanUnion(of.Meshes, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanUnion))),
            difference: static (options, of) => GeometryResults.Sets((of.First, nameof(of.First)), (of.Second, nameof(of.Second)))
                .Map(_ => (Meshes: Mesh.CreateBooleanDifference(of.First, of.Second, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanDifference))),
            intersection: static (options, of) => GeometryResults.Sets((of.First, nameof(of.First)), (of.Second, nameof(of.Second)))
                .Map(_ => (Meshes: Mesh.CreateBooleanIntersection(of.First, of.Second, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanIntersection))),
            split: static (options, of) => GeometryResults.Sets((of.Targets, nameof(of.Targets)), (of.Cutters, nameof(of.Cutters)))
                .Map(_ => (Meshes: Mesh.CreateBooleanSplit(of.Targets, of.Cutters, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanSplit)))))
        from succeeded in DisposalOps.OnFailure(IO.lift(() => Answers.FromResult(answer.Result, answer.Member)), GeometryResults.Released(answer.Meshes))
        from rows in GeometryResults.Acquire(() => (answer.Meshes, answer.Map), answer.Member)
        select rows.Map(static row => (Mesh: row.Result, Inputs: toSeq(row.Row)));

    // --- [SPLITS]
    public static IO<Seq<Mesh>> Split(Mesh mesh, MeshSplitMethod method) =>
        method.Switch(
            mesh,
            byPlane: static (source, of) => GeometryResults.Acquire(() => source.Split(of.Plane), nameof(Mesh.Split), emptyFails: false),
            byMeshes: static (source, of) => GeometryResults.Acquire(
                () => Invalid.Unless(!of.Cutters.IsEmpty, nameof(of.Cutters)).Map(_ => source.Split(
                    meshes: of.Cutters,
                    tolerance: IntersectionTolerance(of.AbsoluteTolerance),
                    splitAtCoplanar: of.SplitAtCoplanar,
                    createNgons: of.CreateNgons,
                    textLog: null,
                    cancel: of.Cancel,
                    progress: of.Progress.ValueUnsafe())),
                nameof(Mesh.Split),
                emptyFails: false),
            disjoint: static (source, _) => GeometryResults.Acquire(source.SplitDisjointPieces, nameof(Mesh.SplitDisjointPieces), emptyFails: false),
            nonManifold: static (source, _) => GeometryResults.Acquire(source.SplitNon2Manifolds, nameof(Mesh.SplitNon2Manifolds), emptyFails: false),
            byProjectedPolylines: static (source, of) => GeometryResults.Acquire(() => source.SplitWithProjectedPolylines(of.Curves, of.Tolerance), nameof(Mesh.SplitWithProjectedPolylines), emptyFails: false),
            unweldedEdges: static (source, _) => GeometryResults.Acquire(source.ExplodeAtUnweldedEdges, nameof(Mesh.ExplodeAtUnweldedEdges), emptyFails: false),
            byCount: static (source, of) => GeometryResults.Acquire(() => Mesh.SplitMesh(source, of.MaxCount, of.CountSum, of.CountTriangles), nameof(Mesh.SplitMesh), emptyFails: true),
            partition: static (source, of) => GeometryResults.Acquire(() => Mesh.PartitionMesh(source, of.MaxVertices, of.MaxFaces), nameof(Mesh.PartitionMesh), emptyFails: false));

    public static IO<(Mesh Remaining, Mesh Extracted)> SplitNonManifoldFaces(Mesh source, bool selective) =>
        GeometryResults.EditCopy(source, copy => Missing.Unless(copy.ExtractNonManifoldEdges(selective), nameof(Mesh.ExtractNonManifoldEdges)))
            .Map(static edited => (Remaining: edited.Copy, Extracted: edited.Result));

    public static IO<Seq<Mesh>> MatchEdges(Seq<Mesh> meshes, double distance, bool simpleSplits, bool rachet, bool average, bool join) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(!meshes.IsEmpty, nameof(meshes)).Map(_ => Mesh.MatchEdges(meshes, distance, simpleSplits, rachet, average, @join)),
            nameof(Mesh.MatchEdges),
            emptyFails: false);

    public static IO<Mesh> Append(Seq<Mesh> meshes) =>
        IO.lift(() => Invalid.Unless(!meshes.IsEmpty, nameof(meshes)).Map(_ => {
            Mesh aggregate = new();
            aggregate.Append(meshes);
            return aggregate;
        }));

    // --- [SECTIONS]
    public static IO<Seq<PolylineCurve>> NakedEdges(Mesh mesh) =>
        IO.lift(() => toSeq(mesh.GetNakedEdges()).Map(static edge => edge.ToPolylineCurve()).Strict());

    public static IO<Seq<PolylineCurve>> Outlines(Mesh mesh, Plane plane) =>
        IO.lift(() => Missing.Unless(mesh.GetOutlines(plane), nameof(Mesh.GetOutlines)).Map(static outlines => toSeq(outlines).Map(static outline => outline.ToPolylineCurve()).Strict()));

    // --- [PROJECTIONS]
    public static IO<Seq<(Point3d Point, int Source)>> ProjectPointsToMeshesEx(Seq<Mesh> meshes, Seq<Point3d> points, Vector3d direction, double tolerance) =>
        IO.lift(() =>
            from valid in (Invalid.Unless(!meshes.IsEmpty, nameof(meshes)), Degenerate.Unless(direction, nameof(direction))).Apply(static (_, _) => unit).As()
            let answer = (Points: Intersection.ProjectPointsToMeshesEx(meshes, points, direction, tolerance, out int[] indices), Sources: indices)
            from projected in Missing.Unless(answer.Points, nameof(Intersection.ProjectPointsToMeshesEx))
            select toSeq(projected).Zip(toSeq(answer.Sources), static (point, source) => (Point: point, Source: source)));
}
