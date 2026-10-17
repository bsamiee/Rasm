using Rasm.Rhino.Document.Units;
using Rhino.Geometry.MeshRefinements;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PlaneFaceCount : System.Numerics.IMinMaxValue<PlaneFaceCount> {
    public static PlaneFaceCount MinValue { get; } = new(1);
    public static PlaneFaceCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = Admission.Refusal(value, MinValue._value, MaxValue._value);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SphereFaceCount : System.Numerics.IMinMaxValue<SphereFaceCount> {
    public static SphereFaceCount MinValue { get; } = new(2);
    public static SphereFaceCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = Admission.Refusal(value, MinValue._value, MaxValue._value);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct MeshRadius : System.Numerics.IMinMaxValue<MeshRadius> {
    public static MeshRadius MinValue { get; } = new(double.BitIncrement(0d));
    public static MeshRadius MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct IsosurfaceResolution : System.Numerics.IMinMaxValue<IsosurfaceResolution> {
    public static IsosurfaceResolution MinValue { get; } = new(1);
    public static IsosurfaceResolution MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = Admission.Refusal(value, MinValue._value, MaxValue._value);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct RefinementLevel : System.Numerics.IMinMaxValue<RefinementLevel> {
    public static RefinementLevel MinValue { get; } = new(0);
    public static RefinementLevel MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = Admission.Refusal(value, MinValue._value, MaxValue._value);
}

[Union<Brep, FaceSelection>(T2Name = "Mesh", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class QuadSource {
    public sealed record FaceSelection(Mesh Mesh, Seq<int> FaceBlocks);
}

[Union<Seq<Mesh>, PointCloud, Meshable, Mesh>(T1Name = "Meshes", T2Name = "Cloud", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ShrinkWrapSource {
    public sealed record Meshable(Seq<GeometryBase> Items, MeshingParameters Meshing);
}

[Union<Convex, Seq<Point3d>, PointCloud>(T2Name = "Points", T3Name = "Cloud", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class HullSource {
    public sealed record Convex(Seq<Point3d> Points, Tolerances Tolerances);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PatchBoundary {
    public sealed record Outline(Polyline Boundary, Option<Surface> Pullback) : PatchBoundary;

    public sealed record Perimeter(Surface Surface, bool Trimback, int Divisions) : PatchBoundary;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshConstruction {
    // --- [GENERATED]
    public static IO<Mesh> QuadRemesh(QuadSource source, QuadRemeshParameters parameters, Seq<Curve> guides, Option<IProgress<int>> progress, Action<IDisposable> retain) =>
        source.Switch(
            (Parameters: parameters, Guides: guides, Progress: progress.ValueUnsafe(), Retain: retain),
            brep: static (state, brep) => Copies.Acquire(
                token => Mesh.QuadRemeshBrep(brep, state.Parameters, state.Guides, state.Progress, token), nameof(Mesh.QuadRemeshBrep), state.Retain),
            mesh: static (state, of) => Copies.Acquire(
                token => of.Mesh.QuadRemesh(of.FaceBlocks, state.Parameters, state.Guides, state.Progress, token), nameof(Mesh.QuadRemesh), state.Retain));

    public static IO<Mesh> ShrinkWrap(ShrinkWrapSource source, ShrinkWrapParameters parameters, Action<IDisposable> retain) =>
        Copies.Acquire(
            token => source.Switch(
                (Parameters: parameters, Token: token),
                meshes: static (state, meshes) => Mesh.ShrinkWrap(meshes, state.Parameters, state.Token),
                cloud: static (state, cloud) => Mesh.ShrinkWrap(cloud, state.Parameters, state.Token),
                meshable: static (state, of) => Mesh.ShrinkWrap(of.Items, state.Parameters, of.Meshing, state.Token),
                mesh: static (state, mesh) => mesh.ShrinkWrap(state.Parameters, state.Token)),
            nameof(Mesh.ShrinkWrap), retain);

    public static IO<(Mesh Mesh, Seq<Seq<int>> Facets)> CreateHull(HullSource source, Action<IDisposable> retain) =>
        Copies.Owned(
            () => source.Switch<(Mesh? Mesh, IEnumerable<int[]>? Facets, string Member)>(
                convex: static of => (Mesh.CreateConvexHull3D(of.Points, out int[][] facets, of.Tolerances.Absolute, of.Tolerances.Angle), facets, nameof(Mesh.CreateConvexHull3D)),
                points: static points => (Mesh.CreateQuickHull3D(points, out List<int[]> facets), facets, nameof(Mesh.CreateQuickHull3D)),
                cloud: static cloud => (Mesh.CreateQuickHull3D(cloud, out List<int[]> facets), facets, nameof(Mesh.CreateQuickHull3D))),
            static answer => Measurements.Valid(answer.Mesh, answer.Member)
                .Map(mesh => (mesh, Conversions.Rows(answer.Facets).Map(static facet => toSeq(facet)).Strict())),
            static answer => DisposalOps.Release(answer.Mesh),
            retain);

    public static IO<Mesh> CreatePatch(PatchBoundary boundary, Seq<Curve> innerBoundaries, Seq<Curve> innerBothSides, Seq<Point3d> innerPoints, Tolerances tolerances, Action<IDisposable> retain) =>
        from boundaries in IO.lift(() => Callbacks.Each(innerBoundaries, static curve => curve.IsClosed, nameof(Mesh.CreatePatch)))
        let patch = boundary.Switch(
            outline: static of => (Boundary: Some(of.Boundary), Surface: of.Pullback, Trimback: false, Divisions: 0),
            perimeter: static of => (Boundary: Option<Polyline>.None, Surface: Some(of.Surface), of.Trimback, of.Divisions))
        from mesh in Copies.Acquire(
            () => Mesh.CreatePatch(patch.Boundary.ValueUnsafe(), tolerances.Angle, patch.Surface.ValueUnsafe(), innerBoundaries, innerBothSides, innerPoints, patch.Trimback, patch.Divisions),
            nameof(Mesh.CreatePatch), retain)
        select mesh;

    // --- [REBUILT]
    public static IO<Option<Seq<Option<Mesh>>>> CreateFromIterativeCleanup(Seq<Mesh> meshes, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Owned(
            () => Optional(Mesh.CreateFromIterativeCleanup(meshes, tolerances.Absolute)),
            static products => products.Traverse(static rows => Measurements.Valid(toSeq(rows).Map(Optional), nameof(Mesh.CreateFromIterativeCleanup))).As(),
            static products => DisposalOps.Release(products.ToSeq().Bind(static rows => Conversions.Rows(rows))), retain);

    public static IO<Mesh> CreateFromFilteredFaceList(Mesh mesh, Seq<int> faces, Action<IDisposable> retain) =>
        from count in IO.lift(() => mesh.Faces.Count)
        from filtered in Copies.Acquire(
            () => Copies.InRange(faces, count, nameof(Mesh.CreateFromFilteredFaceList))
                .Map(_ => Mesh.CreateFromFilteredFaceList(mesh, toSeq(Range(0, count)).Map(toHashSet(faces).Contains))),
            nameof(Mesh.CreateFromFilteredFaceList), retain)
        select filtered;

    public static IO<Mesh> Refine(Mesh mesh, Option<LoopFormula> loop, RefinementLevel level, CreaseEdges nakedEdgeMode, Action<IDisposable> retain) =>
        loop.Match(
            Some: formula => Copies.Acquire(
                token => Mesh.CreateRefinedLoopMesh(mesh, formula, new RefinementSettings { Level = level, NakedEdgeMode = nakedEdgeMode, ContinueRequest = token }),
                nameof(Mesh.CreateRefinedLoopMesh), retain),
            None: () => Copies.Acquire(
                token => Mesh.CreateRefinedCatmullClarkMesh(mesh, new RefinementSettings { Level = level, NakedEdgeMode = nakedEdgeMode, ContinueRequest = token }),
                nameof(Mesh.CreateRefinedCatmullClarkMesh), retain));
}
