using Rasm.Rhino.Document.Units;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union<Surface, Brep>(SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class UnrollSource;

public sealed record UnrollOptions(Option<double> Explode);

public sealed record UnrollResult(Seq<Brep> Flat, Seq<(Curve Curve, Option<int> Source)> Curves, Seq<Point3d> Points, Seq<(TextDot Dot, Option<int> Source)> Dots);

[Union<Surface, Mesh>(SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SquishSource;

public sealed record SquishResult(
    GeometryBase Flat,
    Seq<(GeometryBase Source, Option<GeometryBase> Flat)> Marks,
    Mesh Mesh2d,
    Mesh Mesh3d,
    Seq<(Line Flat, Line Source)> LengthConstrainedLines,
    Seq<MeshFace> AreaConstrainedTriangles) {
    public Seq<double> Strain => LengthConstrainedLines.Map(static pair => (pair.Flat.Length / pair.Source.Length) - 1d);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
internal static partial class UnrollMapper {
    [MapProperty(nameof(@UnrollOptions.Explode.IsSome), nameof(Unroller.ExplodeOutput))]
    [MapProperty(nameof(UnrollOptions.Explode), nameof(Unroller.ExplodeSpacing))]
    internal static partial void Update(UnrollOptions options, Unroller unroller, double absoluteTolerance, double relativeTolerance);
}

public static class Flattening {
    public static IO<UnrollResult> Unroll(Tolerances tolerances, UnrollSource source, UnrollOptions options, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots, Action<IDisposable> retain) =>
        from unroller in IO.lift(() => {
            Unroller created = source.Switch(surface: static surface => new Unroller(surface), brep: static brep => new Unroller(brep));
            UnrollMapper.Update(options, created, tolerances.Absolute, tolerances.Relative);
            created.AddFollowingGeometry(curves);
            created.AddFollowingGeometry(points);
            created.AddFollowingGeometry(dots);
            return created;
        })
        from unrolled in Copies.Acquire(nameof(Unroller.PerformUnroll), _ =>
            (Answer.NonEmpty(unroller.PerformUnroll(out Curve[] unrolledCurves, out Point3d[] unrolledPoints, out TextDot[] unrolledDots)), Answer.Many(unrolledCurves), Answer.Many(unrolledDots))
                .Apply((flat, flatCurves, flatDots) => new UnrollResult(
                    flat,
                    flatCurves.Map(curve => (Curve: curve, Source: Conversions.Present(unroller.FollowingGeometryIndex(curve)))).Strict(),
                    Conversions.Rows(unrolledPoints),
                    flatDots.Map(dot => (Dot: dot, Source: Conversions.Present(unroller.FollowingGeometryIndex(dot)))).Strict()))
                .As(), retain)
        select unrolled;

    public static IO<SquishResult> Squish(SquishSource source, SquishParameters parameters, Seq<GeometryBase> marks, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned => DisposalOps.Within(static () => new Squisher(), squisher =>
            from squished in source.Switch(
                (Squisher: squisher, Parameters: parameters, Marks: marks, Owned: owned),
                surface: static (run, surface) => Copies.Acquire(nameof(Squisher.SquishSurface), _ => new List<GeometryBase>() switch {
                    var flatMarks => Marked(run.Squisher.SquishSurface(run.Parameters, surface, run.Marks, flatMarks), flatMarks),
                }, run.Owned),
                mesh: static (run, mesh) => Copies.Acquire(nameof(Squisher.SquishMesh), _ => new List<GeometryBase>() switch {
                    var flatMarks => Marked(run.Squisher.SquishMesh(run.Parameters, mesh, run.Marks, flatMarks), flatMarks),
                }, run.Owned))
            from mesh2d in Copies.Acquire(nameof(Squisher.Get2dMesh), _ => Answer.One(squisher.Get2dMesh()), owned)
            from mesh3d in Copies.Acquire(nameof(Squisher.Get3dMesh), _ => Answer.One(squisher.Get3dMesh()), owned)
            select new SquishResult(
                squished.Flat, marks.Zip(squished.Marks), mesh2d, mesh3d,
                Conversions.Rows(squisher.GetLengthConstrained2dLines()).Zip(Conversions.Rows(squisher.GetLengthConstrained3dLines())),
                Conversions.Rows(squisher.GetAreaConstrainedTrianglesIndices()))), retain);

    public static IO<Seq<(GeometryBase Flat, Option<GeometryBase> Source)>> SquishBack(GeometryBase squished, Seq<GeometryBase> marks, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Squisher.SquishBack2dMarks), _ => Answer.Sparse(Squisher.SquishBack2dMarks(squished, marks)), retain)
            .Map(back => marks.Zip(back));

    public static IO<Seq<Mesh>> Unwrap(Seq<Mesh> meshes, MeshUnwrapMethod method, Option<Plane> symmetry, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from copies in meshes.TraverseM(mesh => Copies.Duplicate(mesh, owned)).As()
            from unwrapped in Unwrapped(copies, method, symmetry)
            select unwrapped, retain);

    private static Answer<(GeometryBase Flat, Seq<Option<GeometryBase>> Marks)> Marked(GeometryBase? flat, List<GeometryBase> flatMarks) =>
        (Answer.One(flat), Answer.Sparse(flatMarks)).Apply(static (held, kept) => (Flat: held, Marks: kept)).As();

    private static IO<Seq<Mesh>> Unwrapped(Seq<Mesh> copies, MeshUnwrapMethod method, Option<Plane> symmetry) =>
        DisposalOps.Within(() => new MeshUnwrapper(copies), unwrapper =>
            from configured in IO.lift(() => symmetry.Iter(plane => unwrapper.SymmetryPlane = plane))
            from applied in IO.lift(() => Refused.Unless(unwrapper.Unwrap(method), copies, nameof(MeshUnwrapper.Unwrap)))
            select applied);
}
