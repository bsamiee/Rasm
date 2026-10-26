using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ParametricSurface {
    public sealed record Ruled(Func<double, Point3d> Directrix, Func<double, Vector3d> Ruling, Interval U, Interval V) : ParametricSurface;

    public sealed record Function(Func<Point2d, Point3d> Map, Interval U, Interval V) : ParametricSurface;

    public IO<NurbsSurface> ToNurbs(Tolerances tolerances, Action<IDisposable> retain) =>
        DisposalOps.Within((Action<IDisposable> scratch) => Switch(
            (Tolerances: tolerances, Scratch: scratch, Retain: retain),
            ruled: static (run, ruled) =>
                from edges in Seq(ruled.V.T0, ruled.V.T1).TraverseM(v =>
                    from edge in IO.lift(ParametricCurve.Function.Validated(u => ruled.Directrix(u) + (v * ruled.Ruling(u)), ruled.U, []))
                    from nurbs in edge.ToNurbs(run.Tolerances, run.Scratch)
                    select nurbs).As()
                from surface in Copies.Acquire(nameof(NurbsSurface.CreateRuledSurface), _ => Answer.One(NurbsSurface.CreateRuledSurface(edges[0], edges[1])), run.Retain)
                select surface,
            function: static (run, function) =>
                from plane in Copies.Acquire(nameof(Brep.CreateFromSurface), _ => Answer.Successor(
                    NurbsSurface.CreateFromCorners(
                        new Point3d(function.U.T0, function.V.T0, 0), new Point3d(function.U.T1, function.V.T0, 0),
                        new Point3d(function.U.T1, function.V.T1, 0), new Point3d(function.U.T0, function.V.T1, 0), run.Tolerances.Absolute),
                    static corners => Answer.One(Brep.CreateFromSurface(corners))), run.Scratch)
                from morphed in Morphs.Morph(
                    run.Tolerances, Seq(plane), new MorphKind.Point(p => function.Map(new Point2d(p.X, p.Y))), new MorphSettings(QuickPreview: false, PreserveStructure: false), run.Scratch)
                from surface in Copies.Acquire(nameof(BrepFace.ToNurbsSurface), _ => Answer.One(morphed[0].Faces[0].ToNurbsSurface()), run.Retain)
                select surface), DisposalOps.Release);
}
