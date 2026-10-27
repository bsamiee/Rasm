using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ParametricSurface {
    public sealed record Ruled(Func<double, Point3d> Directrix, Func<double, Vector3d> Ruling, Interval U, Interval V) : ParametricSurface;

    public sealed record Function(Func<Point2d, Point3d> Map, Interval U, Interval V) : ParametricSurface;

    public IO<NurbsSurface> ToNurbs(Tolerances tolerances) =>
        Switch(
            tolerances,
            ruled: static (held, ruled) =>
                from edges in Seq(ruled.V.T0, ruled.V.T1).TraverseM(v =>
                    from edge in IO.lift(ParametricCurve.Function.Validated(u => ruled.Directrix(u) + (v * ruled.Ruling(u)), ruled.U, []))
                    from nurbs in edge.ToNurbs(held)
                    select nurbs).As()
                from surface in Products.Made(NurbsSurface.CreateRuledSurface(edges[0], edges[1]))
                select surface,
            function: static (held, function) =>
                from corners in Products.Made(NurbsSurface.CreateFromCorners(
                    new Point3d(function.U.T0, function.V.T0, 0), new Point3d(function.U.T1, function.V.T0, 0),
                    new Point3d(function.U.T1, function.V.T1, 0), new Point3d(function.U.T0, function.V.T1, 0), held.Absolute))
                from plane in Products.Made(Brep.CreateFromSurface(corners))
                from morphed in Morphs.Morph(held, Seq(plane), new MorphKind.Point(p => function.Map(new Point2d(p.X, p.Y))), new MorphSettings(QuickPreview: false, PreserveStructure: false))
                from surface in Products.Made(morphed[0].Faces[0].ToNurbsSurface())
                select surface);
}
