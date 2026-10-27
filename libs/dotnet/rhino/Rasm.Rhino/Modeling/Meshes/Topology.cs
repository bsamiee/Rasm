using System.Runtime.CompilerServices;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Units;
using Rhino.Commands;

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
public static class MeshTopology {
    // --- [RUNS]
    private static MeshBooleanOptions Running(MeshBooleanOptions options, double tolerance, CancellationToken token) {
        (options.Tolerance, options.CancellationToken) = (tolerance, token);
        return options;
    }

    private static MeshSplitOptions Running(MeshSplitOptions options, double tolerance, CancellationToken token, IProgress<double>? progress) {
        (options.Tolerance, options.CancellationToken, options.ProgressReporter) = (tolerance, token, progress);
        return options;
    }

    // --- [BOOLEANS]
    public static IO<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> Boolean(Tolerances tolerances, Boolean<Mesh> operation, MeshBooleanOptions options) =>
        from token in cancelToken
        from booleaned in operation.Switch(
            Running(options, tolerances.MeshIntersection, token),
            union: static (report, union) => Booleaned(Mesh.CreateBooleanUnion(union.Operands, report, out Result result, out int[][] map), result, map, report),
            intersection: static (report, intersection) => Booleaned(Mesh.CreateBooleanIntersection(intersection.First, intersection.Second, report, out Result result, out int[][] map), result, map, report),
            difference: static (report, difference) => Booleaned(Mesh.CreateBooleanDifference(difference.First, difference.Second, report, out Result result, out int[][] map), result, map, report))
        select booleaned;

    public static IO<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> BooleanSplit(Tolerances tolerances, Seq<Mesh> meshesToSplit, Seq<Mesh> meshSplitters, MeshBooleanOptions options) =>
        from token in cancelToken
        from booleaned in Running(options, tolerances.MeshIntersection, token) switch {
            var report => Booleaned(Mesh.CreateBooleanSplit(meshesToSplit, meshSplitters, report, out Result result, out int[][] map), result, map, report),
        }
        select booleaned;

    private static IO<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> Booleaned(
        Mesh[]? meshes, Result result, int[][]? map, MeshBooleanOptions report, [CallerArgumentExpression(nameof(meshes))] string call = "") =>
        from groups in Copies.Mapped(Products.Made(meshes, call), map, call)
        from ended in IO.lift(() => Results.Ended(result, call))
        select (Groups: groups, Report: report);

    // --- [SPLITS]
    public static IO<(Seq<Mesh> Meshes, Option<Seq<Point3d>> OpenCutEnds)> Split(Tolerances tolerances, Mesh mesh, MeshSplit method, Option<IProgress<double>> progress) =>
        from token in cancelToken
        from split in method.Switch(
            (Mesh: mesh, Tolerances: tolerances, Progress: progress.ValueUnsafe(), Token: token),
            byMeshes: static (state, of) => Running(of.Options, state.Tolerances.MeshIntersection, state.Token, state.Progress) switch {
                var options => Products.Made(state.Mesh.Split(of.Meshes, options)).Map(meshes => (Meshes: meshes, OpenCutEnds: Optional(options.OpenCutEnds).Map(static ends => toSeq(ends)))),
            },
            self: static (state, of) => Running(of.Options, state.Tolerances.MeshIntersection, state.Token, state.Progress) switch {
                var options => Unsplit(state.Mesh.SelfSplit(options), state.Token).Map(meshes => (Meshes: meshes, OpenCutEnds: Optional(options.OpenCutEnds).Map(static ends => toSeq(ends)))),
            },
            non2Manifolds: static (state, _) => Unsplit(state.Mesh.SplitNon2Manifolds(textLog: null, state.Token, state.Progress), state.Token)
                .Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)),
            byProjectedPolylines: static (state, of) => Unsplit(state.Mesh.SplitWithProjectedPolylines(of.Curves, state.Tolerances.Absolute, textLog: null, state.Token, state.Progress), state.Token)
                .Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)),
            byPlane: static (state, of) => Unsplit(state.Mesh.Split(of.Plane), state.Token).Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)),
            disjoint: static (state, _) => Whole(state.Mesh.SplitDisjointPieces()).Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)),
            atUnweldedEdges: static (state, _) => Whole(state.Mesh.ExplodeAtUnweldedEdges()).Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)))
        select split;

    private static IO<Seq<Mesh>> Unsplit(Mesh[]? meshes, CancellationToken token, [CallerArgumentExpression(nameof(meshes))] string call = "") =>
        Products.Made(meshes ?? (token.IsCancellationRequested ? null : []), call);

    private static IO<Seq<Mesh>> Whole(Mesh[]? meshes, [CallerArgumentExpression(nameof(meshes))] string call = "") =>
        meshes is [{ } whole]
            ? IO.lift(() => {
                using Mesh discarded = whole;
                return Seq<Mesh>();
            })
            : Products.Made(meshes, call);

    // --- [EXTRACTION]
    public static IO<(Mesh Remaining, Option<Mesh> Extracted)> Extract(Mesh source, MeshExtraction extraction) =>
        from copy in Copies.Duplicate(source)
        from result in extraction.Switch(
            copy,
            nonManifoldEdges: static (held, of) => Extracted(held, held.ExtractNonManifoldEdges(of.Selective)),
            duplicateFaces: static (held, _) => Extracted(held, held.Faces.ExtractDuplicateFaces()),
            faces: static (held, of) => IO.lift(() => Copies.Within(held, ComponentIndexType.MeshFace, of.Indices))
                .Bind(faces => Extracted(held, held.Faces.ExtractFaces(faces))))
        select result;

    private static IO<(Mesh Remaining, Option<Mesh> Extracted)> Extracted(Mesh copy, Mesh? extracted, [CallerArgumentExpression(nameof(extracted))] string call = "") =>
        from held in Products.Held(extracted, call)
        from remaining in Products.Made(copy, call)
        select (Remaining: remaining, Extracted: held);

    // --- [MATCHING]
    public static IO<Seq<Mesh>> MatchEdges(Seq<Mesh> inputMeshes, double distance, bool simpleSplits, bool rachet, bool average, bool join) =>
        from copies in inputMeshes.TraverseM(static mesh => Copies.Duplicate(mesh)).As()
        from matched in Products.Made(Mesh.MatchEdges(copies, distance, simpleSplits, rachet, average, @join))
        select matched;

    // --- [MEASURES]
    public static IO<Seq<MeshThicknessMeasurement>> ComputeThickness(Seq<Mesh> meshes, double maximumThickness, Option<double> sharpAngle) =>
        from token in cancelToken
        from measured in IO.lift(() => Conversions.Found(sharpAngle.Match(
                Some: angle => Mesh.ComputeThickness(meshes, maximumThickness, angle, token),
                None: () => Mesh.ComputeThickness(meshes, maximumThickness, token)))
            .Map(static rows => toSeq(rows)))
        select measured;
}
