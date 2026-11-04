using MathNet.Numerics.LinearAlgebra;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;
using Rasm.Spatial;

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
                from edges in Products.Each(Seq(ruled.V.T0, ruled.V.T1).Map(v =>
                    from edge in IO.lift(ParametricCurve.Function.Validated(u => ruled.Directrix(u) + (v * ruled.Ruling(u)), ruled.U, []))
                    from nurbs in edge.ToNurbs(held)
                    select nurbs))
                from surface in Products.Of(() => NurbsSurface.CreateRuledSurface(edges[0], edges[1]))
                select surface,
            function: static (held, function) =>
                from corners in Products.Of(() => NurbsSurface.CreateFromCorners(
                    new Point3d(function.U.T0, function.V.T0, 0), new Point3d(function.U.T1, function.V.T0, 0),
                    new Point3d(function.U.T1, function.V.T1, 0), new Point3d(function.U.T0, function.V.T1, 0), held.Absolute))
                from plane in Products.Of(() => Brep.CreateFromSurface(corners))
                from morphed in Morphs.Morph(held, Seq(plane), new MorphKind.Point(p => function.Map(new Point2d(p.X, p.Y))), preserveStructure: false, quickPreview: false)
                from surface in Products.Of(() => morphed[0].Faces[0].ToNurbsSurface())
                select surface);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceLowerings {
    // --- [CLOSED_FORMS]
    extension(PolynomialPatch patch) {
        public IO<NurbsSurface> ToNurbs(Plane frame) =>
            from net in IO.lift(() => patch.Net switch { var grid => (grid.X, grid.Y, grid.Z, grid.W) })
            let controls = Controls(net, frame)
            from surface in Products.Of(() => NurbsSurface.CreateFromPoints(controls.Map(static control => control.Place), net.X.RowCount, net.X.ColumnCount, net.X.RowCount - 1, net.X.ColumnCount - 1))
            from shaped in Edits.Apply(Stages.Geometry<NurbsSurface>(), surface, [
                .. controls.ForAll(static control => control.Weight is 1d)
                    ? Seq<Step<NurbsSurface>>()
                    : Seq(Steps.Of<NurbsSurface>(static held => held.MakeRational())) + controls.Map(static control => Steps.Of<NurbsSurface>(held => held.Points.SetPoint(control.U, control.V, control.Place, control.Weight))),
                Steps.Of<NurbsSurface>(held => held.SetDomain(SurfaceDirection.U.Index, new Interval(patch.U.Start, patch.U.End))),
                Steps.Of<NurbsSurface>(held => held.SetDomain(SurfaceDirection.V.Index, new Interval(patch.V.Start, patch.V.End)))])
            select shaped;
    }

    private static Seq<(int U, int V, double Weight, Point3d Place)> Controls((Matrix<double> X, Matrix<double> Y, Matrix<double> Z, Matrix<double> W) net, Plane frame) =>
        (from u in toSeq(Range(0, net.X.RowCount)) from v in toSeq(Range(0, net.X.ColumnCount)) let weight = net.W[u, v] select (U: u, V: v, Weight: weight, Place: (new Vector3D<double>(net.X[u, v], net.Y[u, v], net.Z[u, v]) / weight).ToPoint3d(frame))).Strict();
}
