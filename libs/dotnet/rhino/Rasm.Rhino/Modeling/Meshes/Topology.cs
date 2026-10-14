using Rasm.Rhino.Document.Units;
using Rhino.Commands;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshBoolean {
    public sealed record BooleanUnion(Seq<Mesh> Meshes) : MeshBoolean;
    public sealed record BooleanIntersection(Seq<Mesh> FirstSet, Seq<Mesh> SecondSet) : MeshBoolean;
    public sealed record BooleanDifference(Seq<Mesh> FirstSet, Seq<Mesh> SecondSet) : MeshBoolean;
    public sealed record BooleanSplit(Seq<Mesh> MeshesToSplit, Seq<Mesh> MeshSplitters) : MeshBoolean;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshSplit {
    public sealed record ByMeshes(Seq<Mesh> Meshes, MeshSplitOptions Options) : MeshSplit;
    public sealed record Self(MeshSplitOptions Options) : MeshSplit;
    public sealed record Non2Manifolds : MeshSplit;
    public sealed record ByProjectedPolylines(Seq<PolylineCurve> Curves) : MeshSplit;
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshManifold {
    public static IO<MeshManifold> Of(Mesh mesh) =>
        IO.lift(MeshManifold () => (Manifold: mesh.IsManifold(topologicalTest: true, out bool isOriented, out bool hasBoundary), Oriented: isOriented, Boundary: hasBoundary) switch {
            (Manifold: false, _, _) => new NonManifold(),
            (_, var oriented, Boundary: true) => new Open(oriented),
            (_, Oriented: false, _) => new Closed(),
            _ => new Solid(mesh.Volume()),
        });

    public sealed record NonManifold : MeshManifold;
    public sealed record Open(bool IsOriented) : MeshManifold;
    public sealed record Closed : MeshManifold;
    public sealed record Solid(double Volume) : MeshManifold;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class MeshTopologyMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial MeshBooleanOptions Boolean((double Tolerance, CancellationToken CancellationToken, IProgress<double>? ProgressReporter) source);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapNestedProperties(nameof(MeshSplit.Self.Options))]
    internal static partial MeshSplitOptions Split((MeshSplitOptions Options, double Tolerance, CancellationToken CancellationToken, IProgress<double>? ProgressReporter) source);
}

public static class MeshTopology {
    // --- [BOOLEANS]
    public static IO<Seq<(Mesh Mesh, Seq<int> Inputs)>> Boolean(MeshBoolean method, Tolerances tolerances, Option<IProgress<double>> progress, Action<IDisposable> retain) =>
        from token in cancelToken
        from answer in Booleaned(method, tolerances, progress, retain, token)
        select answer;

    private static IO<Seq<(Mesh Mesh, Seq<int> Inputs)>> Booleaned(MeshBoolean method, Tolerances tolerances, Option<IProgress<double>> progress, Action<IDisposable> retain, CancellationToken token) =>
        Copies.Owned(
            () => method.Switch(
                MeshTopologyMapper.Boolean((tolerances.MeshIntersection, token, progress.ValueUnsafe())),
                booleanUnion: static (options, of) => (Meshes: Mesh.CreateBooleanUnion(of.Meshes, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanUnion)),
                booleanIntersection: static (options, of) => (Meshes: Mesh.CreateBooleanIntersection(of.FirstSet, of.SecondSet, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanIntersection)),
                booleanDifference: static (options, of) => (Meshes: Mesh.CreateBooleanDifference(of.FirstSet, of.SecondSet, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanDifference)),
                booleanSplit: static (options, of) => (Meshes: Mesh.CreateBooleanSplit(of.MeshesToSplit, of.MeshSplitters, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanSplit))),
            made =>
                from verdict in made.Result switch {
                    Result.Nothing => Fin.Succ(unit),
                    Result.Failure when token.IsCancellationRequested => Fin.Fail<Unit>(Errors.Cancelled),
                    var result => Conversions.FromResult(result, made.Member),
                }
                from rows in (Missing.Unless(made.Meshes, made.Member).Bind(meshes => Measurements.Valid([.. meshes], made.Member)).ToValidation(), Missing.Unless(made.Map, made.Member).ToValidation())
                    .Apply(static (meshes, map) => meshes.Zip(toSeq(map), static (mesh, inputs) => (Mesh: mesh, Inputs: toSeq(inputs)))).As().ToFin()
                select rows,
            static made => DisposalOps.Release(Conversions.Rows(made.Meshes)), retain);

    // --- [SPLITS]
    public static IO<Option<Seq<Mesh>>> Split(Mesh mesh, MeshSplit method, Tolerances tolerances, Option<IProgress<double>> progress, Action<IDisposable> retain) =>
        from token in cancelToken
        from split in Copies.Owned(
            () => method.Switch(
                (Mesh: mesh, Tolerances: tolerances, Token: token, Progress: progress.ValueUnsafe()),
                byMeshes: static (state, of) => (Meshes: state.Mesh.Split(of.Meshes, MeshTopologyMapper.Split((of.Options, state.Tolerances.MeshIntersection, state.Token, state.Progress))), Member: nameof(Mesh.Split)),
                self: static (state, of) => (Meshes: state.Mesh.SelfSplit(MeshTopologyMapper.Split((of.Options, state.Tolerances.MeshIntersection, state.Token, state.Progress))), Member: nameof(Mesh.SelfSplit)),
                non2Manifolds: static (state, _) => (Meshes: state.Mesh.SplitNon2Manifolds(textLog: null, state.Token, state.Progress), Member: nameof(Mesh.SplitNon2Manifolds)),
                byProjectedPolylines: static (state, of) => (Meshes: state.Mesh.SplitWithProjectedPolylines(of.Curves, state.Tolerances.Absolute, textLog: null, state.Token, state.Progress), Member: nameof(Mesh.SplitWithProjectedPolylines))),
            made => made.Meshes switch {
                null when token.IsCancellationRequested => Fin.Fail<Option<Seq<Mesh>>>(Errors.Cancelled),
                null when method is MeshSplit.Self or MeshSplit.Non2Manifolds => Option<Seq<Mesh>>.None,
                null => new Missing(made.Member),
                var meshes => Measurements.Valid([.. meshes], made.Member).Map(Some),
            },
            static made => DisposalOps.Release(Conversions.Rows(made.Meshes)), retain)
        select split;

    public static IO<(Mesh Remaining, Option<Mesh> Extracted)> ExtractNonManifoldEdges(Mesh source, bool selective, Action<IDisposable> retain) =>
        DisposalOps.Acquire(
            output =>
                from copy in Copies.Duplicate(source, output)
                from result in Copies.Owned(
                    () => Optional(copy.ExtractNonManifoldEdges(selective)),
                    extracted => (Measurements.Valid(copy, nameof(Mesh.ExtractNonManifoldEdges)).ToValidation(),
                            extracted.Traverse(static mesh => Measurements.Valid(mesh, nameof(Mesh.ExtractNonManifoldEdges))).As().ToValidation())
                        .Apply(static (remaining, extracted) => (Remaining: remaining, Extracted: extracted)).As().ToFin(),
                    static extracted => DisposalOps.Release(extracted.ToSeq()), output)
                select result,
            (_, release) => retain(release));

    // --- [MATCHING]
    public static IO<Seq<Mesh>> MatchEdges(Seq<Mesh> inputMeshes, double distance, bool simpleSplits, bool rachet, bool average, bool join, Action<IDisposable> retain) =>
        DisposalOps.Acquire(
            output => DisposalOps.Within(
                (Action<IDisposable> temporary) =>
                    from copies in inputMeshes.TraverseM(mesh => Copies.Duplicate(mesh, temporary)).As()
                    from matched in Copies.Acquire(() => Mesh.MatchEdges(copies, distance, simpleSplits, rachet, average, @join), nameof(Mesh.MatchEdges), output)
                    select matched,
                DisposalOps.Release),
            (_, release) => retain(release));

    // --- [MEASURES]
    public static IO<Seq<MeshThicknessMeasurement>> ComputeThickness(Seq<Mesh> meshes, double maximumThickness, Option<double> sharpAngle) =>
        from token in cancelToken
        from measured in IO.lift(() => sharpAngle.Match(Some: angle => Mesh.ComputeThickness(meshes, maximumThickness, angle, token), None: () => Mesh.ComputeThickness(meshes, maximumThickness, token)))
        from answer in IO.lift(Fin<Seq<MeshThicknessMeasurement>> () => measured is [] && token.IsCancellationRequested ? Errors.Cancelled : Conversions.Rows(measured))
        select answer;
}
