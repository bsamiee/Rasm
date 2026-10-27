using System.Runtime.CompilerServices;
using Rasm.Spatial;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Curves;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate Curve? CatenaryCall(out Point3d apex, out double parameter, out double length, out double deviation);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CatenaryCurve(Curve Curve, Point3d Apex, double Parameter, double Length, double MaxDeviation);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Conic {
    public sealed record EllipseArc(Ellipse Ellipse, Interval Angles) : Conic;

    public IO<NurbsCurve> ToNurbs() =>
        Switch(
            ellipseArc: static arc =>
                from circle in Products.Made(NurbsCurve.CreateFromArc(new Arc(new Circle(arc.Ellipse.Plane, 1d), arc.Angles)))
                from scaled in IO.lift(() => RhinoFault.Unless(circle.Transform(Transform.Scale(arc.Ellipse.Plane, arc.Ellipse.Radius1, arc.Ellipse.Radius2, 1d))).Map(_ => circle))
                select scaled);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveConstruction {
    // --- [JOINS_AND_BOOLEANS]
    public static IO<Seq<Grouped<Curve, int>>> JoinCurves(Tolerances tolerances, Seq<Curve> curves, bool preserveDirection, bool simpleJoin) =>
        Copies.Keyed(Products.Made(Curve.JoinCurves(curves, tolerances.Absolute, preserveDirection, simpleJoin, out int[] key)), key);

    public static IO<Seq<Grouped<Curve, int>>> Boolean(Tolerances tolerances, Boolean<Curve> operation) =>
        operation.Switch(
            tolerances,
            union: static (at, union) => Union(at, union.Operands),
            intersection: static (at, intersection) => Booleans.Meet(
                intersection.First,
                intersection.Second,
                () => Union(at, intersection.Second).Bind(static regions => IO.lift(() =>
                    Callbacks.Each(regions, static (region, index) => RhinoFault.Unless(!region.From.IsEmpty, index).Map(_ => region.Value)))),
                (first, outer) => Products.Made(Curve.CreateBooleanIntersection(first, outer, at.Absolute))),
            difference: static (at, difference) => Booleans.PerFirst(difference.First, first =>
                Products.Made(Curve.CreateBooleanDifference(first, difference.Second, at.Absolute))));

    public static IO<Seq<Grouped<Seq<Curve>, int>>> CreateBooleanRegions(Tolerances tolerances, Seq<Curve> curves, Plane plane, Seq<Point3d> points, bool combineRegions) =>
        from regions in unitIO.Bind(_ => Products.Made(Curve.CreateBooleanRegions(curves, plane, points, combineRegions, tolerances.Absolute)))
        from grouped in Copies.Keyed(
            Callbacks.Each(toSeq(Range(0, regions.RegionCount)).Map(index => Products.Made(regions.RegionCurves(index))
                .Bind(region => IO.lift(() => RhinoFault.Unless(!region.IsEmpty, index).Map(_ => region)))).Strict()),
            [.. toSeq(Range(0, regions.PointCount)).Map(regions.RegionPointIndex)])
        select grouped;

    private static IO<Seq<Grouped<Curve, int>>> Union(Tolerances tolerances, Seq<Curve> curves) =>
        Copies.Keyed(Products.Made(Curve.CreateBooleanUnion(curves, tolerances.Absolute, out int[] indexMap))
            .Bind(static united => IO.lift(() => RhinoFault.Unless(!united.IsEmpty).Map(_ => united))), indexMap);

    // --- [ASSEMBLY]
    internal static IO<NurbsCurve> Appended(Seq<NurbsCurve> pieces) =>
        from whole in Copies.Duplicate(pieces[0])
        from joined in IO.lift(() => RhinoFault.Unless(pieces.Tail.ForAll(whole.Append)))
        from valid in Products.Made(whole)
        select valid;

    // --- [LOWERINGS]
    extension(AnalyticCurve curve) {
        public IO<NurbsCurve> ToNurbs(Plane frame, Tolerances tolerances) => ParametricCurve.Traced(t => curve.At(t).ToPoint3d(frame), curve.Joins, tolerances);
    }

    extension(RationalQuadratic conic) {
        public IO<NurbsCurve> ToNurbs(Plane frame) =>
            from bezier in unitIO.Bind(_ => Products.Made(new BezierCurve([new Point4d(conic.Start.ToPoint3d(frame)), new Point4d(conic.Apex.ToPoint3d(frame)) * conic.Weight, new Point4d(conic.End.ToPoint3d(frame))])))
            from nurbs in Products.Made(bezier.ToNurbsCurve())
            select nurbs;
    }

    extension(HermiteSpline spline) {
        public IO<NurbsCurve> ToNurbs(Plane frame) =>
            from pieces in spline.Spans.TraverseM(span =>
                from bezier in Products.Made(new BezierCurve([span.Start.ToPoint3d(frame), span.StartControl.ToPoint3d(frame), span.EndControl.ToPoint3d(frame), span.End.ToPoint3d(frame)]))
                from nurbs in Products.Made(bezier.ToNurbsCurve())
                select nurbs).As()
            from joined in Appended(pieces)
            select joined;
    }

    // --- [CATENARIES]
    public static IO<CatenaryCurve> CreateCatenaryCurve(CatenaryCall call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        Products.Made(call(out Point3d apex, out double parameter, out double length, out double deviation), member)
            .Map(curve => new CatenaryCurve(curve, apex, parameter, length, deviation));
}
