using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union<Surface, Brep>(SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class UnrollSource;

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
public static class Flattening {
    // --- [UNROLL]
    public static IO<UnrollResult> Unroll(Tolerances tolerances, UnrollSource source, Option<double> explode, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots) =>
        from unroller in IO.lift(() => {
            Unroller created = source.Switch(surface: static surface => new Unroller(surface), brep: static brep => new Unroller(brep));
            (created.AbsoluteTolerance, created.RelativeTolerance, created.ExplodeOutput) = (tolerances.Absolute, tolerances.Relative, explode.IsSome);
            _ = explode.Iter(spacing => created.ExplodeSpacing = spacing);
            created.AddFollowingGeometry(curves);
            created.AddFollowingGeometry(points);
            created.AddFollowingGeometry(dots);
            return created;
        })
        from unrolled in IO.lift(() => (Breps: unroller.PerformUnroll(out Curve[] curves, out Point3d[] points, out TextDot[] dots), Curves: curves, Points: points, Dots: dots))
        from flat in Products.Many(() => unrolled.Breps)
        from flatCurves in Products.Many(() => unrolled.Curves, required: false)
        from flatDots in Products.Many(() => unrolled.Dots, required: false)
        select new UnrollResult(
            flat,
            flatCurves.Map(curve => (Curve: curve, Source: Conversions.Present(unroller.FollowingGeometryIndex(curve)))).Strict(),
            Conversions.Rows(unrolled.Points),
            flatDots.Map(dot => (Dot: dot, Source: Conversions.Present(unroller.FollowingGeometryIndex(dot)))).Strict());

    // --- [SQUISH]
    public static IO<SquishResult> Squish(SquishSource source, SquishParameters parameters, Seq<GeometryBase> marks) =>
        from squisher in use(static () => new Squisher())
        from squished in source.Switch(
            (Squisher: squisher, Parameters: parameters, Marks: marks),
            surface: static (run, surface) => Marked(flatMarks => run.Squisher.SquishSurface(run.Parameters, surface, run.Marks, flatMarks)),
            mesh: static (run, mesh) => Marked(flatMarks => run.Squisher.SquishMesh(run.Parameters, mesh, run.Marks, flatMarks)))
        from mesh2d in Products.Of(squisher.Get2dMesh)
        from mesh3d in Products.Of(squisher.Get3dMesh)
        select new SquishResult(
            squished.Flat, marks.Zip(squished.Marks), mesh2d, mesh3d,
            Conversions.Rows(squisher.GetLengthConstrained2dLines()).Zip(Conversions.Rows(squisher.GetLengthConstrained3dLines())),
            Conversions.Rows(squisher.GetAreaConstrainedTrianglesIndices()));

    public static IO<Seq<(GeometryBase Flat, Option<GeometryBase> Source)>> SquishBack(GeometryBase squished, Seq<GeometryBase> marks) =>
        from slots in IO.lift(() => Conversions.Required(Squisher.SquishBack2dMarks(squished, marks)).Map(static back => toSeq(back).Map(static mark => Optional(mark)).Strict()))
        from kept in Products.Each(slots.Somes().Map(static mark => Products.Of(() => mark)))
        select marks.Zip(slots);

    private static IO<(GeometryBase Flat, Seq<Option<GeometryBase>> Marks)> Marked(Func<List<GeometryBase>, GeometryBase?> squish, [CallerArgumentExpression(nameof(squish))] string call = "") =>
        from flatMarks in IO.lift(static () => new List<GeometryBase>())
        from flat in Products.Of(() => squish(flatMarks), call)
        let slots = toSeq(flatMarks).Map(static mark => Optional(mark)).Strict()
        from kept in Products.Each(slots.Somes().Map(static mark => Products.Of(() => mark)))
        select (Flat: flat, Marks: slots);

    // --- [UNWRAP]
    public static IO<Seq<Mesh>> Unwrap(Seq<Mesh> meshes, MeshUnwrapMethod method, Option<Plane> symmetry) =>
        from copies in meshes.TraverseM(static mesh => Products.Of(mesh.DuplicateMesh)).As()
        from unwrapped in (from unwrapper in use(() => new MeshUnwrapper(copies))
                           from configured in IO.lift(() => symmetry.Iter(plane => unwrapper.SymmetryPlane = plane))
                           from applied in Products.Accepted(() => unwrapper.Unwrap(method))
                           select copies).Bracket()
        select unwrapped;
}
