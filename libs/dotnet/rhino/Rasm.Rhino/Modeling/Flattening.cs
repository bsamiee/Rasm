using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public static IO<UnrollResult> Unroll(Tolerances tolerances, UnrollSource source, UnrollOptions options, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots) =>
        from unroller in IO.lift(() => {
            Unroller created = source.Switch(surface: static surface => new Unroller(surface), brep: static brep => new Unroller(brep));
            UnrollMapper.Update(options, created, tolerances.Absolute, tolerances.Relative);
            created.AddFollowingGeometry(curves);
            created.AddFollowingGeometry(points);
            created.AddFollowingGeometry(dots);
            return created;
        })
        from unrolled in IO.lift(() => (
            Breps: unroller.PerformUnroll(out Curve[] curves, out Point3d[] points, out TextDot[] dots), Curves: curves, Points: points, Dots: dots))
        from flat in Products.Made(unrolled.Breps)
        from flatCurves in Products.Made(unrolled.Curves)
        from flatDots in Products.Made(unrolled.Dots)
        from filled in IO.lift(() => RhinoFault.Unless(!flat.IsEmpty))
        select new UnrollResult(
            flat,
            flatCurves.Map(curve => (Curve: curve, Source: Conversions.Present(unroller.FollowingGeometryIndex(curve)))).Strict(),
            Conversions.Rows(unrolled.Points),
            flatDots.Map(dot => (Dot: dot, Source: Conversions.Present(unroller.FollowingGeometryIndex(dot)))).Strict());

    public static IO<SquishResult> Squish(SquishSource source, SquishParameters parameters, Seq<GeometryBase> marks) =>
        from squisher in use(static () => new Squisher())
        from squished in source.Switch(
            (Squisher: squisher, Parameters: parameters, Marks: marks),
            surface: static (run, surface) => new List<GeometryBase>() switch {
                var flatMarks => Marked(run.Squisher.SquishSurface(run.Parameters, surface, run.Marks, flatMarks), flatMarks),
            },
            mesh: static (run, mesh) => new List<GeometryBase>() switch {
                var flatMarks => Marked(run.Squisher.SquishMesh(run.Parameters, mesh, run.Marks, flatMarks), flatMarks),
            })
        from mesh2d in Products.Made(squisher.Get2dMesh())
        from mesh3d in Products.Made(squisher.Get3dMesh())
        select new SquishResult(
            squished.Flat, marks.Zip(squished.Marks), mesh2d, mesh3d,
            Conversions.Rows(squisher.GetLengthConstrained2dLines()).Zip(Conversions.Rows(squisher.GetLengthConstrained3dLines())),
            Conversions.Rows(squisher.GetAreaConstrainedTrianglesIndices()));

    public static IO<Seq<(GeometryBase Flat, Option<GeometryBase> Source)>> SquishBack(GeometryBase squished, Seq<GeometryBase> marks) =>
        from slots in IO.lift(() => Conversions.Found(Squisher.SquishBack2dMarks(squished, marks)).Map(static back => toSeq(back).Map(static mark => Optional(mark)).Strict()))
        from kept in Products.Made(slots.Somes())
        select marks.Zip(slots);

    public static IO<Seq<Mesh>> Unwrap(Seq<Mesh> meshes, MeshUnwrapMethod method, Option<Plane> symmetry) =>
        from copies in meshes.TraverseM(static mesh => Copies.Duplicate(mesh)).As()
        from unwrapped in Unwrapped(copies, method, symmetry)
        select unwrapped;

    private static IO<(GeometryBase Flat, Seq<Option<GeometryBase>> Marks)> Marked(GeometryBase? flat, List<GeometryBase> flatMarks, [CallerArgumentExpression(nameof(flat))] string call = "") =>
        from held in Products.Made(flat, call)
        let slots = toSeq(flatMarks).Map(static mark => Optional(mark)).Strict()
        from kept in Products.Made(slots.Somes())
        select (Flat: held, Marks: slots);

    private static IO<Seq<Mesh>> Unwrapped(Seq<Mesh> copies, MeshUnwrapMethod method, Option<Plane> symmetry) =>
        (from unwrapper in use(() => new MeshUnwrapper(copies))
         from configured in IO.lift(() => symmetry.Iter(plane => unwrapper.SymmetryPlane = plane))
         from applied in IO.lift(() => RhinoFault.Unless(unwrapper.Unwrap(method)).Map(_ => copies))
         select applied).Bracket();
}
