using LanguageExt.UnsafeValueAccess;
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

    public sealed record ByMeshes(Seq<Mesh> Cutters, double Tolerance, bool SplitAtCoplanar, bool CreateNgons, Option<IProgress<double>> Progress, CancellationToken Cancel) : MeshSplitMethod;

    public sealed record Disjoint() : MeshSplitMethod;

    public sealed record NonManifold() : MeshSplitMethod;

    public sealed record ByProjectedPolylines(Seq<PolylineCurve> Curves, double Tolerance) : MeshSplitMethod;

    public sealed record UnweldedEdges() : MeshSplitMethod;

    public sealed record ByCount(int MaxCount, bool CountSum, bool CountTriangles) : MeshSplitMethod;

    public sealed record Partition(int MaxVertices, int MaxFaces) : MeshSplitMethod;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshTopology {
    // --- [BOOLEANS]
    public static IO<Seq<(Mesh Mesh, Seq<int> Inputs)>> Boolean(MeshBooleanMethod method, UnitsAndTolerances tolerances, Option<IProgress<double>> progress, CancellationToken cancel) =>
        from options in IO.lift(() => new MeshBooleanOptions {
            Tolerance = tolerances.Absolute * Intersection.MeshIntersectionsTolerancesCoefficient,
            CancellationToken = cancel,
            ProgressReporter = progress.ValueUnsafe(),
        })
        from result in method.Switch(
            options,
            union: static (options, of) =>
                from present in IO.lift(() => Invalid.Unless(!of.Meshes.IsEmpty, nameof(of.Meshes)))
                from answer in IO.lift(() => (Meshes: Mesh.CreateBooleanUnion(of.Meshes, options, out Result commandResult, out int[][] map), CommandResult: commandResult, Map: map))
                from result in BooleanResult(answer, nameof(Mesh.CreateBooleanUnion))
                select result,
            difference: static (options, of) =>
                from present in IO.lift(() => GeometryResults.Sets((of.First, nameof(of.First)), (of.Second, nameof(of.Second))))
                from answer in IO.lift(() => (Meshes: Mesh.CreateBooleanDifference(of.First, of.Second, options, out Result commandResult, out int[][] map), CommandResult: commandResult, Map: map))
                from result in BooleanResult(answer, nameof(Mesh.CreateBooleanDifference))
                select result,
            intersection: static (options, of) =>
                from present in IO.lift(() => GeometryResults.Sets((of.First, nameof(of.First)), (of.Second, nameof(of.Second))))
                from answer in IO.lift(() => (Meshes: Mesh.CreateBooleanIntersection(of.First, of.Second, options, out Result commandResult, out int[][] map), CommandResult: commandResult, Map: map))
                from result in BooleanResult(answer, nameof(Mesh.CreateBooleanIntersection))
                select result,
            split: static (options, of) =>
                from present in IO.lift(() => GeometryResults.Sets((of.Targets, nameof(of.Targets)), (of.Cutters, nameof(of.Cutters))))
                from answer in IO.lift(() => (Meshes: Mesh.CreateBooleanSplit(of.Targets, of.Cutters, options, out Result commandResult, out int[][] map), CommandResult: commandResult, Map: map))
                from result in BooleanResult(answer, nameof(Mesh.CreateBooleanSplit))
                select result)
        select result;

    private static IO<Seq<(Mesh Mesh, Seq<int> Inputs)>> BooleanResult((Mesh[]? Meshes, Result CommandResult, int[][]? Map) answer, string member) =>
        answer.CommandResult == Result.Success
            ? GeometryResults.Acquire(() => (answer.Meshes, answer.Map), member).Map(static rows => rows.Map(static row => (Mesh: row.Result, Inputs: toSeq(row.Row))))
            : GeometryOps.OnFailure(IO.fail<Seq<(Mesh Mesh, Seq<int> Inputs)>>(new UnexpectedResult(member, answer.CommandResult)), GeometryResults.Released(answer.Meshes));

    // --- [SPLITS]
    public static IO<Seq<Mesh>> Split(Mesh mesh, MeshSplitMethod method) =>
        method.Switch(
            mesh,
            byPlane: static (source, of) =>
                from planar in IO.lift(() => Invalid.Unless(of.Plane.IsValid, nameof(Plane.IsValid)))
                from pieces in GeometryResults.Acquire(() => source.Split(of.Plane), nameof(Mesh.Split))
                select pieces,
            byMeshes: static (source, of) =>
                from present in IO.lift(() => Invalid.Unless(!of.Cutters.IsEmpty, nameof(of.Cutters)))
                from pieces in GeometryResults.Acquire(
                    () => source.Split(meshes: of.Cutters, tolerance: of.Tolerance, splitAtCoplanar: of.SplitAtCoplanar, createNgons: of.CreateNgons, textLog: null, cancel: of.Cancel, progress: of.Progress.ValueUnsafe()),
                    nameof(Mesh.Split))
                select pieces,
            disjoint: static (source, _) => GeometryResults.Acquire(source.SplitDisjointPieces, nameof(Mesh.SplitDisjointPieces)),
            nonManifold: static (source, _) => GeometryResults.Acquire(source.SplitNon2Manifolds, nameof(Mesh.SplitNon2Manifolds)),
            byProjectedPolylines: static (source, of) => GeometryResults.Acquire(() => source.SplitWithProjectedPolylines(of.Curves, of.Tolerance), nameof(Mesh.SplitWithProjectedPolylines)),
            unweldedEdges: static (source, _) => GeometryResults.Acquire(source.ExplodeAtUnweldedEdges, nameof(Mesh.ExplodeAtUnweldedEdges)),
            byCount: static (source, of) => GeometryResults.Acquire(() => Mesh.SplitMesh(source, of.MaxCount, of.CountSum, of.CountTriangles), nameof(Mesh.SplitMesh)),
            partition: static (source, of) => GeometryResults.Acquire(() => Mesh.PartitionMesh(source, of.MaxVertices, of.MaxFaces), nameof(Mesh.PartitionMesh)));

    public static IO<(Mesh Remaining, Mesh Extracted)> SplitNonManifoldFaces(Mesh source, bool selective) =>
        GeometryOps.EditCopy(source, copy => Missing.Unless(copy.ExtractNonManifoldEdges(selective), nameof(Mesh.ExtractNonManifoldEdges)))
            .Map(static edited => (Remaining: edited.Copy, Extracted: edited.Result));

    public static IO<Seq<Mesh>> MatchEdges(Seq<Mesh> meshes, double distance, bool simpleSplits, bool rachet, bool average, bool join) =>
        from present in IO.lift(() => Invalid.Unless(!meshes.IsEmpty, nameof(meshes)))
        from matched in GeometryResults.Acquire(() => Mesh.MatchEdges(meshes, distance, simpleSplits, rachet, average, @join), nameof(Mesh.MatchEdges))
        select matched;

    public static IO<Mesh> Append(Seq<Mesh> meshes) =>
        from present in IO.lift(() => Invalid.Unless(!meshes.IsEmpty, nameof(meshes)))
        from aggregate in IO.lift(() => {
            Mesh aggregate = new();
            aggregate.Append(meshes);
            return aggregate;
        })
        select aggregate;

    // --- [SECTIONS]
    public static IO<Seq<PolylineCurve>> NakedEdges(Mesh mesh) =>
        IO.lift(() => toSeq(mesh.GetNakedEdges()).Map(static edge => edge.ToPolylineCurve()).Strict());

    public static IO<Seq<PolylineCurve>> Outlines(Mesh mesh, Plane plane) =>
        IO.lift(() =>
            from planar in Invalid.Unless(plane.IsValid, nameof(Plane.IsValid))
            from outlines in Missing.Unless(mesh.GetOutlines(plane), nameof(Mesh.GetOutlines))
            from present in Answers.NonEmpty(toSeq(outlines), nameof(Mesh.GetOutlines))
            select present.Map(static outline => outline.ToPolylineCurve()).Strict());

    // --- [PROJECTIONS]
    public static IO<Seq<(Point3d Point, int Source)>> ProjectPointsToMeshesEx(Seq<Mesh> meshes, Seq<Point3d> points, Vector3d direction, double tolerance) =>
        IO.lift(() =>
            from targets in Invalid.Unless(!meshes.IsEmpty, nameof(meshes))
            from present in Invalid.Unless(!points.IsEmpty, nameof(points))
            from aimed in Degenerate.Unless(direction.IsValid && !direction.IsTiny(), nameof(direction))
            let answer = (Points: Intersection.ProjectPointsToMeshesEx(meshes, points, direction, tolerance, out int[] indices), Sources: indices)
            from answered in Missing.Unless(answer.Points, nameof(Intersection.ProjectPointsToMeshesEx))
            from same in CountMismatch.Unless(answered.Length, answer.Sources.Length, nameof(Intersection.ProjectPointsToMeshesEx))
            select toSeq(answered).Zip(toSeq(answer.Sources), static (point, source) => (Point: point, Source: source)));
}
