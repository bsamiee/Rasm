using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshSplit {
    public sealed record ByMeshes(Seq<Mesh> Meshes, Func<MeshSplitOptions> Options) : MeshSplit;
    public sealed record Self(Func<MeshSplitOptions> Options) : MeshSplit;
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

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
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
    // --- [SPLITS]
    public static IO<(Seq<Mesh> Meshes, Option<Seq<Point3d>> OpenCutEnds)> Split(Tolerances tolerances, Mesh mesh, MeshSplit method, Option<IProgress<double>> progress) =>
        from token in cancelToken
        from split in method.Switch(
            (Mesh: mesh, Tolerances: tolerances, Progress: progress.ValueUnsafe(), Token: token),
            byMeshes: static (state, of) =>
                from options in Running(state.Tolerances, state.Progress, of.Options, state.Token)
                from meshes in Products.Many(() => state.Mesh.Split(of.Meshes, options), required: false)
                select (Meshes: meshes, OpenCutEnds: Optional(options.OpenCutEnds).Map(static ends => toSeq(ends))),
            self: static (state, of) =>
                from options in Running(state.Tolerances, state.Progress, of.Options, state.Token)
                from meshes in Products.Many(() => Unsplit(state.Mesh.SelfSplit(options), state.Token), required: false)
                select (Meshes: meshes, OpenCutEnds: Optional(options.OpenCutEnds).Map(static ends => toSeq(ends))),
            non2Manifolds: static (state, _) => Products.Many(() => Unsplit(state.Mesh.SplitNon2Manifolds(textLog: null, state.Token, state.Progress), state.Token), required: false)
                .Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)),
            byProjectedPolylines: static (state, of) =>
                Products.Many(() => Unsplit(state.Mesh.SplitWithProjectedPolylines(of.Curves, state.Tolerances.Absolute, textLog: null, state.Token, state.Progress), state.Token), required: false)
                    .Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)),
            byPlane: static (state, of) => Products.Many(() => Unsplit(state.Mesh.Split(of.Plane), state.Token), required: false)
                .Map(static meshes => (Meshes: meshes, OpenCutEnds: Option<Seq<Point3d>>.None)),
            disjoint: static (state, _) => Products.Many(state.Mesh.SplitDisjointPieces, required: false)
                .Map(static meshes => (Meshes: meshes.Count is 1 ? Seq<Mesh>() : meshes, OpenCutEnds: Option<Seq<Point3d>>.None)),
            atUnweldedEdges: static (state, _) => Products.Many(state.Mesh.ExplodeAtUnweldedEdges, required: false)
                .Map(static meshes => (Meshes: meshes.Count is 1 ? Seq<Mesh>() : meshes, OpenCutEnds: Option<Seq<Point3d>>.None)))
        select split;

    private static IO<MeshSplitOptions> Running(Tolerances tolerances, IProgress<double>? progress, Func<MeshSplitOptions> options, CancellationToken token) =>
        from fresh in IO.lift(options)
        from running in IO.lift(() => (fresh.Tolerance, fresh.CancellationToken, fresh.ProgressReporter) = (tolerances.MeshIntersection, token, progress))
        select fresh;

    private static Mesh[]? Unsplit(Mesh[]? meshes, CancellationToken token) => meshes ?? (token.IsCancellationRequested ? null : []);

    // --- [EXTRACTION]
    public static IO<(Mesh Remaining, Option<Mesh> Extracted)> Extract(Mesh source, MeshExtraction extraction) =>
        from copy in Products.Of(source.DuplicateMesh)
        from extracted in extraction.Switch(
            copy,
            nonManifoldEdges: static (held, of) => Products.Maybe(() => held.ExtractNonManifoldEdges(of.Selective)),
            duplicateFaces: static (held, _) => Products.Maybe(held.Faces.ExtractDuplicateFaces),
            faces: static (held, of) =>
                from faces in IO.lift(() => held.Within(ComponentIndexType.MeshFace, of.Indices))
                from part in Products.Maybe(() => held.Faces.ExtractFaces(faces))
                select part)
        select (Remaining: copy, Extracted: extracted);

    // --- [MATCHING]
    public static IO<Seq<Mesh>> MatchEdges(Seq<Mesh> inputMeshes, double distance, bool simpleSplits, bool rachet, bool average, bool join) =>
        from copies in inputMeshes.TraverseM(static mesh => Products.Of(mesh.DuplicateMesh)).As()
        from matched in Products.Many(() => Mesh.MatchEdges(copies, distance, simpleSplits, rachet, average, @join))
        select matched;

    // --- [MEASURES]
    public static IO<Seq<MeshThicknessMeasurement>> ComputeThickness(Seq<Mesh> meshes, double maximumThickness, Option<double> sharpAngle) =>
        from token in cancelToken
        from measured in IO.lift(() => Conversions.Required(sharpAngle.Match(
                Some: angle => Mesh.ComputeThickness(meshes, maximumThickness, angle, token),
                None: () => Mesh.ComputeThickness(meshes, maximumThickness, token)))
            .Map(static rows => toSeq(rows)))
        select measured;
}
