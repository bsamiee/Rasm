using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rhino.Geometry.MeshRefinements;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct PlaneFaceCount : System.Numerics.IMinMaxValue<PlaneFaceCount> {
    public static PlaneFaceCount MinValue { get; } = new(1);
    public static PlaneFaceCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SphereFaceCount : System.Numerics.IMinMaxValue<SphereFaceCount> {
    public static SphereFaceCount MinValue { get; } = new(2);
    public static SphereFaceCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
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
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct RefinementLevel : System.Numerics.IMinMaxValue<RefinementLevel> {
    public static RefinementLevel MinValue { get; } = new(0);
    public static RefinementLevel MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record QuadSource {
    public sealed record OfBrep(Brep Brep) : QuadSource;

    public sealed record OfMesh(Mesh Mesh, Seq<int> FaceBlocks) : QuadSource;
}

[Union]
public abstract partial record ShrinkWrapSource {
    public sealed record OfMeshes(Seq<Mesh> Meshes) : ShrinkWrapSource;

    public sealed record OfCloud(PointCloud Cloud) : ShrinkWrapSource;

    public sealed record OfGeometry(Seq<GeometryBase> Geometry, MeshingParameters Meshing) : ShrinkWrapSource;

    public sealed record OfMesh(Mesh Mesh) : ShrinkWrapSource;
}

[Union]
public abstract partial record HullSource {
    public sealed record OfPoints(Seq<Point3d> Points) : HullSource;

    public sealed record OfCloud(PointCloud Cloud) : HullSource;
}

public sealed record ConvexHull(Mesh Mesh, Seq<Seq<int>> Facets);

[Union]
public abstract partial record PatchBoundary {
    public sealed record Outline(Polyline Boundary, Option<Surface> Pullback) : PatchBoundary;

    public sealed record Perimeter(Surface Surface, bool Trimback, int Divisions) : PatchBoundary;
}

[Union]
public abstract partial record RefineMethod {
    public sealed record Loop(LoopFormula Formula) : RefineMethod;

    public sealed record CatmullClark : RefineMethod;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshConstruction {
    // --- [GENERATED]
    public static IO<Mesh> QuadRemesh(QuadSource source, QuadRemeshParameters parameters, Seq<Curve> guides, Option<IProgress<int>> progress) =>
        from token in cancelToken
        from mesh in source.Switch(
            (Parameters: parameters, Guides: guides, Progress: progress.ValueUnsafe(), Token: token),
            ofBrep: static (state, of) => Copies.Acquire(
                () => Optional(Mesh.QuadRemeshBrep(of.Brep, state.Parameters, state.Guides, state.Progress, state.Token))
                    .ToFin(state.Token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.QuadRemeshBrep))),
                nameof(Mesh.QuadRemeshBrep)),
            ofMesh: static (state, of) => Copies.Acquire(
                () => Optional(of.Mesh.QuadRemesh(of.FaceBlocks, state.Parameters, state.Guides, state.Progress, state.Token))
                    .ToFin(state.Token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.QuadRemesh))),
                nameof(Mesh.QuadRemesh)))
        select mesh;

    public static IO<Mesh> ShrinkWrap(ShrinkWrapSource source, ShrinkWrapParameters parameters) =>
        from token in cancelToken
        from mesh in Copies.Acquire(
            () => Optional(source.Switch(
                    (Parameters: parameters, Token: token),
                    ofMeshes: static (state, of) => Mesh.ShrinkWrap(of.Meshes, state.Parameters, state.Token),
                    ofCloud: static (state, of) => Mesh.ShrinkWrap(of.Cloud, state.Parameters, state.Token),
                    ofGeometry: static (state, of) => Mesh.ShrinkWrap(of.Geometry, state.Parameters, of.Meshing, state.Token),
                    ofMesh: static (state, of) => of.Mesh.ShrinkWrap(state.Parameters, state.Token)))
                .ToFin(token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Mesh.ShrinkWrap))),
            nameof(Mesh.ShrinkWrap))
        select mesh;

    public static IO<ConvexHull> CreateConvexHull3D(Seq<Point3d> points, Tolerances tolerances) =>
        Hull(() => (Mesh.CreateConvexHull3D(points, out int[][] facets, tolerances.Absolute, tolerances.Angle), toSeq(facets)), nameof(Mesh.CreateConvexHull3D));

    public static IO<ConvexHull> CreateQuickHull3D(HullSource source) =>
        Hull(
            () => source.Switch(
                ofPoints: static of => (Mesh.CreateQuickHull3D(of.Points, out List<int[]> facets), Conversions.Rows(facets)),
                ofCloud: static of => (Mesh.CreateQuickHull3D(of.Cloud, out List<int[]> facets), Conversions.Rows(facets))),
            nameof(Mesh.CreateQuickHull3D));

    public static IO<Mesh> CreatePatch(PatchBoundary boundary, Seq<Curve> innerBoundaries, Seq<Curve> innerBothSides, Seq<Point3d> innerPoints, Tolerances tolerances) =>
        Copies.Acquire(
            () => Callbacks.Each(innerBoundaries, static curve => curve.IsClosed, nameof(Mesh.CreatePatch)).Bind(_ => Callbacks.Thrown<ArgumentException, Mesh>(
                () => boundary.Switch(
                    (Inner: innerBoundaries, BothSides: innerBothSides, Points: innerPoints, tolerances.Angle),
                    outline: static (state, of) => Mesh.CreatePatch(of.Boundary, state.Angle, of.Pullback.ValueUnsafe(), state.Inner, state.BothSides, state.Points, trimback: false, divisions: 0),
                    perimeter: static (state, of) => Mesh.CreatePatch(outerBoundary: null, state.Angle, of.Surface, state.Inner, state.BothSides, state.Points, of.Trimback, of.Divisions)),
                nameof(Mesh.CreatePatch))),
            nameof(Mesh.CreatePatch));

    private static IO<ConvexHull> Hull(Func<(Mesh? Mesh, Seq<int[]> Facets)> host, string member) =>
        Copies.Owned(
            IO.lift(() => host() switch {
                var (mesh, facets) => Missing.Unless(mesh, member).Map(found => new ConvexHull(found, facets.Map(static facet => toSeq(facet)).Strict())),
            }),
            hull => Measurements.Valid(hull.Mesh, member).Map(_ => hull),
            static hull => IO.lift(hull.Mesh.Dispose));

    // --- [REBUILT]
    public static IO<Seq<Option<Mesh>>> CreateFromIterativeCleanup(Seq<Mesh> meshes, Tolerances tolerances) =>
        Copies.AcquireSparse(() => Mesh.CreateFromIterativeCleanup(meshes, tolerances.Absolute), nameof(Mesh.CreateFromIterativeCleanup))
            .Catch(static error => error.IsType<Missing>(), _ => IO.pure(meshes.Map(static _ => Option<Mesh>.None).Strict()));

    public static IO<Mesh> CreateFromFilteredFaceList(Mesh mesh, Seq<int> faces) =>
        Copies.Acquire(
            () => Copies.InRange(faces, mesh.Faces.Count, nameof(Mesh.CreateFromFilteredFaceList))
                .Map(_ => Mesh.CreateFromFilteredFaceList(mesh, toSeq(Range(0, mesh.Faces.Count)).Map(toHashSet(faces).Contains))),
            nameof(Mesh.CreateFromFilteredFaceList));

    public static IO<Mesh> Refine(Mesh mesh, RefineMethod method, RefinementLevel level, CreaseEdges nakedEdgeMode) =>
        method.Switch(
            (Mesh: mesh, Settings: new RefinementSettings { Level = level, NakedEdgeMode = nakedEdgeMode }),
            loop: static (state, of) => Copies.Acquire(() => Mesh.CreateRefinedLoopMesh(state.Mesh, of.Formula, state.Settings), nameof(Mesh.CreateRefinedLoopMesh)),
            catmullClark: static (state, _) => Copies.Acquire(() => Mesh.CreateRefinedCatmullClarkMesh(state.Mesh, state.Settings), nameof(Mesh.CreateRefinedCatmullClarkMesh)));
}
