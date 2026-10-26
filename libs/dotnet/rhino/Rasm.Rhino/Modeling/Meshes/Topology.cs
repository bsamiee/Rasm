using Rasm.Rhino.Document.Units;
using Rhino.Commands;
using Rhino.Geometry.Collections;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshSplit {
    public sealed record ByMeshes(Seq<Mesh> Meshes, MeshSplitOptions Options) : MeshSplit;
    public sealed record Self(MeshSplitOptions Options) : MeshSplit;
    public sealed record Non2Manifolds : MeshSplit;
    public sealed record ByProjectedPolylines(Seq<PolylineCurve> Curves) : MeshSplit;
    public sealed record ByPlane(Plane Plane) : MeshSplit;
    public sealed record Disjoint : MeshSplit;
    public sealed record AtUnweldedEdges : MeshSplit;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshExtraction {
    public sealed record NonManifoldEdges(bool Selective) : MeshExtraction;
    public sealed record DuplicateFaces : MeshExtraction;
    public sealed record Faces(Seq<int> Indices) : MeshExtraction;
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshManifold {
    public static IO<MeshManifold> Of(Mesh mesh) =>
        IO.lift(MeshManifold () => (Manifold: mesh.IsManifold(topologicalTest: true, out bool isOriented, out bool hasBoundary), Oriented: isOriented, Boundary: hasBoundary) switch {
            (Manifold: false, _, _) => new NonManifold(),
            (_, var oriented, Boundary: true) => new Open(oriented),
            _ => mesh.SolidOrientation() switch {
                0 => new Closed(),
                var orientation => new Solid(Outward: orientation > 0),
            },
        });

    public sealed record NonManifold : MeshManifold;
    public sealed record Open(bool IsOriented) : MeshManifold;
    public sealed record Closed : MeshManifold;
    public sealed record Solid(bool Outward) : MeshManifold;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class MeshTopologyMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapNestedProperties(nameof(MeshSplit.Self.Options))]
    [MapperIgnoreTarget(nameof(MeshSplitOptions.OpenCutEnds), Justification = "Booleans write open cut ends into options they run with")]
    [MapperIgnoreTarget(nameof(MeshBooleanOptions.UnmergedResultPairs), Justification = "Booleans write unmerged result pairs into options they run with")]
    [MapperIgnoreTarget(nameof(MeshBooleanOptions.OpenResults), Justification = "Booleans write open results into options they run with")]
    [MapperIgnoreTarget(nameof(MeshBooleanOptions.FailedInputPairs), Justification = "Booleans write failed input pairs into options they run with")]
    internal static partial MeshBooleanOptions Boolean((MeshBooleanOptions Options, double Tolerance, CancellationToken CancellationToken) source);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapNestedProperties(nameof(MeshSplit.Self.Options))]
    [MapperIgnoreTarget(nameof(MeshSplitOptions.OpenCutEnds), Justification = "Split writes open cut ends into options it runs with")]
    internal static partial MeshSplitOptions Split((MeshSplitOptions Options, double Tolerance, CancellationToken CancellationToken, IProgress<double>? ProgressReporter) source);
}

public static class MeshTopology {
    // --- [BOOLEANS]
    public static IO<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> Boolean(Tolerances tolerances, Boolean<Mesh> operation, MeshBooleanOptions options, Action<IDisposable> retain) =>
        operation.Switch(
            (Tolerance: tolerances.MeshIntersection, Options: options, Retain: retain),
            union: static (state, union) => Copies.Acquire(nameof(Mesh.CreateBooleanUnion), token => MeshTopologyMapper.Boolean((state.Options, state.Tolerance, token)) switch {
                var report => Booleaned(Mesh.CreateBooleanUnion(union.Operands, report, out Result result, out int[][] map), result, map, report),
            }, state.Retain),
            intersection: static (state, intersection) => Copies.Acquire(nameof(Mesh.CreateBooleanIntersection), token => MeshTopologyMapper.Boolean((state.Options, state.Tolerance, token)) switch {
                var report => Booleaned(Mesh.CreateBooleanIntersection(intersection.First, intersection.Second, report, out Result result, out int[][] map), result, map, report),
            }, state.Retain),
            difference: static (state, difference) => Copies.Acquire(nameof(Mesh.CreateBooleanDifference), token => MeshTopologyMapper.Boolean((state.Options, state.Tolerance, token)) switch {
                var report => Booleaned(Mesh.CreateBooleanDifference(difference.First, difference.Second, report, out Result result, out int[][] map), result, map, report),
            }, state.Retain));

    public static IO<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> BooleanSplit(
        Tolerances tolerances, Seq<Mesh> meshesToSplit, Seq<Mesh> meshSplitters, MeshBooleanOptions options, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Mesh.CreateBooleanSplit), token => MeshTopologyMapper.Boolean((options, tolerances.MeshIntersection, token)) switch {
            var report => Booleaned(Mesh.CreateBooleanSplit(meshesToSplit, meshSplitters, report, out Result result, out int[][] map), result, map, report),
        }, retain);

    private static Answer<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> Booleaned(Mesh[]? meshes, Result result, int[][]? map, MeshBooleanOptions report) =>
        (Answer.Ended(result), Answer.Mapped(Answer.Many(meshes), map)).Apply((_, groups) => (Groups: groups, Report: report)).As();

    // --- [SPLITS]
    public static IO<(Seq<Mesh> Meshes, Option<Seq<Point3d>> OpenCutEnds)> Split(Tolerances tolerances, Mesh mesh, MeshSplit method, Option<IProgress<double>> progress, Action<IDisposable> retain) =>
        method.Switch(
            (Mesh: mesh, Tolerances: tolerances, Progress: progress.ValueUnsafe(), Retain: retain),
            byMeshes: static (state, of) => Copies.Acquire(nameof(Mesh.Split), token =>
                MeshTopologyMapper.Split((of.Options, state.Tolerances.MeshIntersection, token, state.Progress)) switch {
                    var options => Answer.Many(state.Mesh.Split(of.Meshes, options)).Map(meshes => (Meshes: meshes, OpenCutEnds: Optional(options.OpenCutEnds).Map(static ends => toSeq(ends)))),
                }, state.Retain),
            self: static (state, of) => Copies.Acquire(nameof(Mesh.SelfSplit), token =>
                MeshTopologyMapper.Split((of.Options, state.Tolerances.MeshIntersection, token, state.Progress)) switch {
                    var options => Unsplit(state.Mesh.SelfSplit(options), token).Map(meshes => (Meshes: meshes, OpenCutEnds: Optional(options.OpenCutEnds).Map(static ends => toSeq(ends)))),
                }, state.Retain),
            non2Manifolds: static (state, _) => Copies.Acquire(nameof(Mesh.SplitNon2Manifolds), token =>
                Unsplit(state.Mesh.SplitNon2Manifolds(textLog: null, token, state.Progress), token).Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)), state.Retain),
            byProjectedPolylines: static (state, of) => Copies.Acquire(nameof(Mesh.SplitWithProjectedPolylines), token =>
                Unsplit(state.Mesh.SplitWithProjectedPolylines(of.Curves, state.Tolerances.Absolute, textLog: null, token, state.Progress), token)
                    .Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)), state.Retain),
            byPlane: static (state, of) => Copies.Acquire(nameof(Mesh.Split), token =>
                Unsplit(state.Mesh.Split(of.Plane), token).Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)), state.Retain),
            disjoint: static (state, _) => Copies.Acquire(nameof(Mesh.SplitDisjointPieces), _ =>
                Whole(state.Mesh.SplitDisjointPieces()).Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)), state.Retain),
            atUnweldedEdges: static (state, _) => Copies.Acquire(nameof(Mesh.ExplodeAtUnweldedEdges), _ =>
                Whole(state.Mesh.ExplodeAtUnweldedEdges()).Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)), state.Retain));

    private static Answer<Seq<Mesh>> Unsplit(Mesh[]? meshes, CancellationToken token) =>
        meshes is null && !token.IsCancellationRequested ? Answer.Many<Mesh>([]) : Answer.Many(meshes);

    private static Answer<Seq<Mesh>> Whole(Mesh[]? meshes) =>
        meshes is [var whole] && DisposalOps.Present(whole) ? Answer.Many<Mesh>([]) : Answer.Many(meshes);

    // --- [EXTRACTION]
    public static IO<(Mesh Remaining, Option<Mesh> Extracted)> Extract(Mesh source, MeshExtraction extraction, Action<IDisposable> retain) =>
        DisposalOps.Acquire(
            output =>
                from copy in Copies.Duplicate(source, output)
                from result in extraction.Switch(
                    (Copy: copy, Output: output),
                    nonManifoldEdges: static (at, of) => Extracted(nameof(Mesh.ExtractNonManifoldEdges), at.Copy, mesh => mesh.ExtractNonManifoldEdges(of.Selective), at.Output),
                    duplicateFaces: static (at, _) => Extracted(nameof(MeshFaceList.ExtractDuplicateFaces), at.Copy, static mesh => mesh.Faces.ExtractDuplicateFaces(), at.Output),
                    faces: static (at, of) => IO.lift(() => Copies.Within(at.Copy, ComponentIndexType.MeshFace, of.Indices))
                        .Bind(faces => Extracted(nameof(MeshFaceList.ExtractFaces), at.Copy, mesh => mesh.Faces.ExtractFaces(faces), at.Output)))
                select result,
            retain);

    private static IO<(Mesh Remaining, Option<Mesh> Extracted)> Extracted(string member, Mesh copy, Func<Mesh, Mesh?> extract, Action<IDisposable> retain) =>
        Copies.Acquire(member, _ => (Answer.Optional(extract(copy)), Answer.One(copy) with { Leaves = [] })
            .Apply(static (extracted, remaining) => (Remaining: remaining, Extracted: extracted)).As(), retain);

    // --- [MATCHING]
    public static IO<Seq<Mesh>> MatchEdges(Seq<Mesh> inputMeshes, double distance, bool simpleSplits, bool rachet, bool average, bool join, Action<IDisposable> retain) =>
        DisposalOps.Acquire(
            output => DisposalOps.Within(
                (Action<IDisposable> temporary) =>
                    from copies in inputMeshes.TraverseM(mesh => Copies.Duplicate(mesh, temporary)).As()
                    from matched in Copies.Acquire(nameof(Mesh.MatchEdges), _ => Answer.Many(Mesh.MatchEdges(copies, distance, simpleSplits, rachet, average, @join)), output)
                    select matched,
                DisposalOps.Release),
            retain);

    // --- [MEASURES]
    public static IO<Seq<MeshThicknessMeasurement>> ComputeThickness(Seq<Mesh> meshes, double maximumThickness, Option<double> sharpAngle) =>
        DisposalOps.Within(
            (Action<IDisposable> held) => Copies.Acquire(nameof(Mesh.ComputeThickness), token => Answer.Rows(
                sharpAngle.Match(Some: angle => Mesh.ComputeThickness(meshes, maximumThickness, angle, token), None: () => Mesh.ComputeThickness(meshes, maximumThickness, token)),
                static _ => Option<IDisposable>.None), held),
            DisposalOps.Release);
}
