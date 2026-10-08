using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rhino.Geometry.MeshRefinements;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct PlaneFaceCount : System.Numerics.IMinMaxValue<PlaneFaceCount> {
    public static PlaneFaceCount MinValue { get; } = new(1);
    public static PlaneFaceCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SphereFaceCount : System.Numerics.IMinMaxValue<SphereFaceCount> {
    public static SphereFaceCount MinValue { get; } = new(2);
    public static SphereFaceCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct MeshRadius : System.Numerics.IMinMaxValue<MeshRadius> {
    public static MeshRadius MinValue { get; } = new(double.BitIncrement(0d));
    public static MeshRadius MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct IsosurfaceResolution : System.Numerics.IMinMaxValue<IsosurfaceResolution> {
    public static IsosurfaceResolution MinValue { get; } = new(1);
    public static IsosurfaceResolution MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct RefinementLevel : System.Numerics.IMinMaxValue<RefinementLevel> {
    public static RefinementLevel MinValue { get; } = new(0);
    public static RefinementLevel MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 ? null : new InvalidRhinoValue();
}

[Union<Brep, FaceSelection>(T2Name = "Mesh", MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class QuadSource {
    public sealed record FaceSelection(Mesh Mesh, Seq<int> FaceBlocks);
}

[Union<Seq<Mesh>, PointCloud, Meshable, Mesh>(T1Name = "Meshes", T2Name = "Cloud", MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ShrinkWrapSource {
    public sealed record Meshable(Seq<GeometryBase> Items, MeshingParameters Meshing);
}

[Union<Convex, Seq<Point3d>, PointCloud>(T2Name = "Points", T3Name = "Cloud", MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class HullSource {
    public sealed record Convex(Seq<Point3d> Points, Tolerances Tolerances);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PatchBoundary {
    public sealed record Outline(Polyline Boundary, Option<Surface> Pullback) : PatchBoundary;

    public sealed record Perimeter(Surface Surface, bool Trimback, int Divisions) : PatchBoundary;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class RefinementMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial RefinementSettings ToSettings((RefinementLevel Level, CreaseEdges NakedEdgeMode, CancellationToken ContinueRequest) source);

    [UserMapping]
    private static int ToLevel(RefinementLevel level) => level;
}

public static class MeshConstruction {
    // --- [GENERATED]
    public static IO<Mesh> QuadRemesh(QuadSource source, QuadRemeshParameters parameters, Seq<Curve> guides, Option<IProgress<int>> progress) =>
        from token in cancelToken
        from mesh in source.Switch(
            (Parameters: parameters, Guides: guides, Progress: progress.ValueUnsafe(), Token: token),
            brep: static (state, brep) => Copies.Acquire(
                () => Optional(Mesh.QuadRemeshBrep(brep, state.Parameters, state.Guides, state.Progress, state.Token))
                    .ToFin(state.Token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.QuadRemeshBrep))),
                nameof(Mesh.QuadRemeshBrep)),
            mesh: static (state, of) => Copies.Acquire(
                () => Optional(of.Mesh.QuadRemesh(of.FaceBlocks, state.Parameters, state.Guides, state.Progress, state.Token))
                    .ToFin(state.Token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.QuadRemesh))),
                nameof(Mesh.QuadRemesh)))
        select mesh;

    public static IO<Mesh> ShrinkWrap(ShrinkWrapSource source, ShrinkWrapParameters parameters) =>
        from token in cancelToken
        from mesh in Copies.Acquire(
            () => Optional(source.Switch(
                    (Parameters: parameters, Token: token),
                    meshes: static (state, meshes) => Mesh.ShrinkWrap(meshes, state.Parameters, state.Token),
                    cloud: static (state, cloud) => Mesh.ShrinkWrap(cloud, state.Parameters, state.Token),
                    meshable: static (state, of) => Mesh.ShrinkWrap(of.Items, state.Parameters, of.Meshing, state.Token),
                    mesh: static (state, mesh) => mesh.ShrinkWrap(state.Parameters, state.Token)))
                .ToFin(token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.ShrinkWrap))),
            nameof(Mesh.ShrinkWrap))
        select mesh;

    public static IO<(Mesh Mesh, Seq<Seq<int>> Facets)> CreateHull(HullSource source) =>
        from answer in IO.lift(() => source.Switch(
            convex: static of => (Mesh: Mesh.CreateConvexHull3D(of.Points, out int[][] facets, of.Tolerances.Absolute, of.Tolerances.Angle), Facets: toSeq(facets), Member: nameof(Mesh.CreateConvexHull3D)),
            points: static points => (Mesh: Mesh.CreateQuickHull3D(points, out List<int[]> facets), Facets: Conversions.Rows(facets), Member: nameof(Mesh.CreateQuickHull3D)),
            cloud: static cloud => (Mesh: Mesh.CreateQuickHull3D(cloud, out List<int[]> facets), Facets: Conversions.Rows(facets), Member: nameof(Mesh.CreateQuickHull3D))))
        from mesh in Copies.Acquire(() => answer.Mesh, answer.Member)
        select (mesh, answer.Facets.Map(static facet => toSeq(facet)).Strict());

    public static IO<Mesh> CreatePatch(PatchBoundary boundary, Seq<Curve> innerBoundaries, Seq<Curve> innerBothSides, Seq<Point3d> innerPoints, Tolerances tolerances) =>
        from boundaries in IO.lift(() => Callbacks.Each(innerBoundaries, static curve => curve.IsClosed, nameof(Mesh.CreatePatch)))
        let patch = boundary.Switch(
            outline: static of => (Boundary: Some(of.Boundary), Surface: of.Pullback, Trimback: false, Divisions: 0),
            perimeter: static of => (Boundary: Option<Polyline>.None, Surface: Some(of.Surface), of.Trimback, of.Divisions))
        from mesh in Copies.Acquire(
            () => Callbacks.Thrown<ArgumentException, Mesh>(
                () => Mesh.CreatePatch(patch.Boundary.ValueUnsafe(), tolerances.Angle, patch.Surface.ValueUnsafe(), innerBoundaries, innerBothSides, innerPoints, patch.Trimback, patch.Divisions),
                nameof(Mesh.CreatePatch)),
            nameof(Mesh.CreatePatch))
        select mesh;

    // --- [REBUILT]
    public static IO<Option<Seq<Option<Mesh>>>> CreateFromIterativeCleanup(Seq<Mesh> meshes, Tolerances tolerances) =>
        Copies.Owned(
            IO.lift(() => Optional(Mesh.CreateFromIterativeCleanup(meshes, tolerances.Absolute))),
            static products => products.Traverse(static rows => Measurements.Valid(toSeq(rows).Map(Optional), nameof(Mesh.CreateFromIterativeCleanup))).As(),
            static products => DisposalOps.Release(products.ToSeq().Bind(static rows => Conversions.Rows(rows))));

    public static IO<Mesh> CreateFromFilteredFaceList(Mesh mesh, Seq<int> faces) =>
        from count in IO.lift(() => mesh.Faces.Count)
        from filtered in Copies.Acquire(
            () => Copies.InRange(faces, count, nameof(Mesh.CreateFromFilteredFaceList))
                .Map(_ => Mesh.CreateFromFilteredFaceList(mesh, toSeq(Range(0, count)).Map(toHashSet(faces).Contains))),
            nameof(Mesh.CreateFromFilteredFaceList))
        select filtered;

    public static IO<Mesh> Refine(Mesh mesh, Option<LoopFormula> loop, RefinementLevel level, CreaseEdges nakedEdgeMode) =>
        from token in cancelToken
        let settings = RefinementMapper.ToSettings((level, nakedEdgeMode, token))
        from refined in Copies.Owned(
            Try.lift(() => loop.Match(
                Some: formula => (Mesh: Mesh.CreateRefinedLoopMesh(mesh, formula, settings), Member: nameof(Mesh.CreateRefinedLoopMesh)),
                None: () => (Mesh: Mesh.CreateRefinedCatmullClarkMesh(mesh, settings), Member: nameof(Mesh.CreateRefinedCatmullClarkMesh)))).ToIO(),
            static answer => Measurements.Valid(answer.Mesh, answer.Member),
            static answer => IO.lift(answer.Mesh.Dispose))
        select refined;
}
