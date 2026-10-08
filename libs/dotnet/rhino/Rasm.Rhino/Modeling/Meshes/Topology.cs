using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rhino.Commands;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record MeshBoolean {
    public sealed record BooleanUnion(Seq<Mesh> Meshes) : MeshBoolean;

    public sealed record BooleanIntersection(Seq<Mesh> FirstSet, Seq<Mesh> SecondSet) : MeshBoolean;

    public sealed record BooleanDifference(Seq<Mesh> FirstSet, Seq<Mesh> SecondSet) : MeshBoolean;

    public sealed record BooleanSplit(Seq<Mesh> MeshesToSplit, Seq<Mesh> MeshSplitters) : MeshBoolean;
}

public sealed record MeshCuts(bool SplitAtCoplanar, bool CreateNgons, bool CompleteOpenCuts);

[Union]
public abstract partial record MeshSplit {
    public sealed record ByMeshes(Seq<Mesh> Meshes, MeshCuts Cuts) : MeshSplit;

    public sealed record Self(MeshCuts Cuts) : MeshSplit;

    public sealed record Non2Manifolds : MeshSplit;

    public sealed record ByProjectedPolylines(Seq<PolylineCurve> Curves) : MeshSplit;
}

[Union]
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
internal static partial class MeshCutsMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial MeshSplitOptions ToOptions(MeshCuts cuts, double tolerance, CancellationToken cancellationToken, IProgress<double>? progressReporter);
}

public static class MeshTopology {
    // --- [BOOLEANS]
    public static IO<Seq<(Mesh Mesh, Seq<int> Inputs)>> Boolean(MeshBoolean method, Tolerances tolerances, Option<IProgress<double>> progress) =>
        cancelToken.Bind(token => Copies.Owned(
            IO.lift(() => method.Switch(
                new MeshBooleanOptions { Tolerance = tolerances.MeshIntersection, CancellationToken = token, ProgressReporter = progress.ValueUnsafe() },
                booleanUnion: static (options, of) => (Meshes: Mesh.CreateBooleanUnion(of.Meshes, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanUnion)),
                booleanIntersection: static (options, of) => (Meshes: Mesh.CreateBooleanIntersection(of.FirstSet, of.SecondSet, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanIntersection)),
                booleanDifference: static (options, of) => (Meshes: Mesh.CreateBooleanDifference(of.FirstSet, of.SecondSet, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanDifference)),
                booleanSplit: static (options, of) => (Meshes: Mesh.CreateBooleanSplit(of.MeshesToSplit, of.MeshSplitters, options, out Result result, out int[][] map), Result: result, Map: map, Member: nameof(Mesh.CreateBooleanSplit)))),
            made => (made.Result switch {
                Result.Nothing => Fin.Succ(unit),
                Result.Failure when token.IsCancellationRequested => Fin.Fail<Unit>(Errors.Cancelled),
                var result => Conversions.FromResult(result, made.Member),
            })
                .Bind(_ => Measurements.Valid([.. made.Meshes], made.Member))
                .Map(meshes => meshes.Zip(toSeq(made.Map), static (mesh, inputs) => (Mesh: mesh, Inputs: toSeq(inputs)))),
            static made => DisposalOps.Release(Conversions.Rows(made.Meshes))));

    // --- [SPLITS]
    public static IO<Seq<Mesh>> Split(Mesh mesh, MeshSplit method, Tolerances tolerances, Option<IProgress<double>> progress) =>
        cancelToken.Bind(token => method.Switch(
            (Mesh: mesh, Tolerances: tolerances, Token: token, Progress: progress.ValueUnsafe()),
            byMeshes: static (state, of) => Copies.Acquire(
                () => Optional(state.Mesh.Split(of.Meshes, MeshCutsMapper.ToOptions(of.Cuts, state.Tolerances.MeshIntersection, state.Token, state.Progress)))
                    .ToFin(state.Token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.Split))),
                nameof(Mesh.Split)),
            self: static (state, of) => Copies.Acquire(
                    () => Optional(state.Mesh.SelfSplit(MeshCutsMapper.ToOptions(of.Cuts, state.Tolerances.MeshIntersection, state.Token, state.Progress)))
                        .ToFin(state.Token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.SelfSplit))),
                    nameof(Mesh.SelfSplit))
                .Catch(static error => error.IsType<Missing>(), _ => Copies.Duplicate(state.Mesh).Map(static whole => Seq(whole))),
            non2Manifolds: static (state, _) => Copies.Acquire(
                    () => Optional(state.Mesh.SplitNon2Manifolds(textLog: null, state.Token, state.Progress))
                        .ToFin(state.Token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.SplitNon2Manifolds))),
                    nameof(Mesh.SplitNon2Manifolds))
                .Catch(static error => error.IsType<Missing>(), _ => Copies.Duplicate(state.Mesh).Map(static whole => Seq(whole))),
            byProjectedPolylines: static (state, of) => Copies.Acquire(
                () => Optional(state.Mesh.SplitWithProjectedPolylines(of.Curves, state.Tolerances.Absolute, textLog: null, state.Token, state.Progress))
                    .ToFin(state.Token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.SplitWithProjectedPolylines))),
                nameof(Mesh.SplitWithProjectedPolylines))));

    public static IO<(Mesh Remaining, Option<Mesh> Extracted)> ExtractNonManifoldEdges(Mesh source, bool selective) =>
        Copies.Owned(
            Copies.Duplicate(source).Bind(copy => IO.lift(() => (Remaining: copy, Extracted: Optional(copy.ExtractNonManifoldEdges(selective))))),
            static made => (
                    Measurements.Valid(made.Remaining, nameof(Mesh.ExtractNonManifoldEdges)).ToValidation(),
                    made.Extracted.Traverse(static extracted => Measurements.Valid(extracted, nameof(Mesh.ExtractNonManifoldEdges))).As().ToValidation())
                .Apply(static (remaining, extracted) => (Remaining: remaining, Extracted: extracted))
                .As()
                .ToFin(),
            static made => DisposalOps.Release(made.Extracted.ToSeq().Add(made.Remaining)));

    // --- [MATCHING]
    public static IO<Seq<Mesh>> MatchEdges(Seq<Mesh> inputMeshes, double distance, bool simpleSplits, bool rachet, bool average, bool join) =>
        DisposalOps.AcquireAll(inputMeshes.Map(Copies.Duplicate), DisposalOps.Release).Bracket(
            Use: copies => Copies.Acquire(() => Mesh.MatchEdges(copies, distance, simpleSplits, rachet, average, join), nameof(Mesh.MatchEdges)),
            Fin: DisposalOps.Release);

    // --- [MEASURES]
    public static IO<Seq<MeshThicknessMeasurement>> ComputeThickness(Seq<Mesh> meshes, double maximumThickness, Option<double> sharpAngle) =>
        cancelToken.Bind(token => IO.lift(Fin<Seq<MeshThicknessMeasurement>> () => sharpAngle.Match(
                Some: angle => Mesh.ComputeThickness(meshes, maximumThickness, angle, token),
                None: () => Mesh.ComputeThickness(meshes, maximumThickness, token)) switch {
                { Length: 0 } when token.IsCancellationRequested => Errors.Cancelled,
                    var measured => Conversions.Rows(measured),
                }));
}
