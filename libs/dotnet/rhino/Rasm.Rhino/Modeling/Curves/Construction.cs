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

    public IO<NurbsCurve> ToNurbs(Action<IDisposable> retain) =>
        Switch(
            retain,
            ellipseArc: static (held, arc) => Copies.Acquire(nameof(NurbsCurve.CreateFromArc), _ => NurbsCurve.CreateFromArc(new Arc(new Circle(arc.Ellipse.Plane, 1d), arc.Angles)) switch {
                var circle => (Answer.Accepted(circle?.Transform(Transform.Scale(arc.Ellipse.Plane, arc.Ellipse.Radius1, arc.Ellipse.Radius2, 1d)) is true), Answer.One(circle))
                    .Apply(static (_, scaled) => scaled).As(),
            }, held));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveConstruction {
    // --- [JOINS_AND_BOOLEANS]
    public static IO<Seq<Grouped<Curve, int>>> JoinCurves(Tolerances tolerances, Seq<Curve> curves, bool preserveDirection, bool simpleJoin, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.JoinCurves), _ => Answer.Keyed(Answer.Many(Curve.JoinCurves(curves, tolerances.Absolute, preserveDirection, simpleJoin, out int[] key)), key), retain);

    public static IO<Seq<Grouped<Curve, int>>> Boolean(Tolerances tolerances, Boolean<Curve> operation, Action<IDisposable> retain) =>
        operation.Switch(
            (Tolerances: tolerances, Retain: retain),
            union: static (at, union) => Union(at.Tolerances, union.Operands, at.Retain),
            intersection: static (at, intersection) => Booleans.Meet(
                intersection.First,
                intersection.Second,
                temporary => Union(at.Tolerances, intersection.Second, temporary).Bind(static regions => IO.lift(() => Callbacks.Each(regions, static (region, index) => region.From.IsEmpty
                    ? new Refused(nameof(Curve.CreateBooleanIntersection), "cannot take a hole", index)
                    : Fin.Succ(region.Value)))),
                (first, outer) => Copies.Acquire(nameof(Curve.CreateBooleanIntersection), _ => Answer.Many(Curve.CreateBooleanIntersection(first, outer, at.Tolerances.Absolute)), at.Retain)),
            difference: static (at, difference) => Booleans.PerFirst(difference.First, first => Copies.Acquire(nameof(Curve.CreateBooleanDifference), _ => Answer.Many(
                Curve.CreateBooleanDifference(first, difference.Second, at.Tolerances.Absolute)), at.Retain)));

    public static IO<Seq<Grouped<Seq<Curve>, int>>> CreateBooleanRegions(Tolerances tolerances, Seq<Curve> curves, Plane plane, Seq<Point3d> points, bool combineRegions, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.CreateBooleanRegions), _ => Answer.Successor(
            Curve.CreateBooleanRegions(curves, plane, points, combineRegions, tolerances.Absolute),
            static regions => Answer.Keyed(
                toSeq(Range(0, regions.RegionCount)).Traverse(index => Answer.NonEmpty(regions.RegionCurves(index))).As(),
                [.. toSeq(Range(0, regions.PointCount)).Map(regions.RegionPointIndex)])), retain);

    private static IO<Seq<Grouped<Curve, int>>> Union(Tolerances tolerances, Seq<Curve> curves, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.CreateBooleanUnion), _ => Answer.Keyed(Answer.NonEmpty(Curve.CreateBooleanUnion(curves, tolerances.Absolute, out int[] indexMap)), indexMap), retain);

    // --- [ASSEMBLY]
    internal static IO<NurbsCurve> Appended(Seq<NurbsCurve> pieces, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from whole in Copies.Duplicate(pieces[0], owned)
            from joined in Copies.Acquire(nameof(NurbsCurve.Append), _ =>
                (Answer.Accepted(pieces.Tail.ForAll(whole.Append)), Answer.One(whole) with { Leaves = [] }).Apply(static (_, held) => held).As(), owned)
            select joined, retain);

    // --- [LOWERINGS]
    extension(AnalyticCurve curve) {
        public IO<NurbsCurve> ToNurbs(Plane frame, Tolerances tolerances, Action<IDisposable> retain) => ParametricCurve.Traced(t => curve.At(t).ToPoint3d(frame), curve.Joins, tolerances, retain);
    }

    extension(RationalQuadratic conic) {
        public IO<NurbsCurve> ToNurbs(Plane frame, Action<IDisposable> retain) =>
            Copies.Acquire(nameof(BezierCurve.ToNurbsCurve), _ => Answer.Successor(
                new BezierCurve([new Point4d(conic.Start.ToPoint3d(frame)), new Point4d(conic.Apex.ToPoint3d(frame)) * conic.Weight, new Point4d(conic.End.ToPoint3d(frame))]),
                static bezier => Answer.One(bezier.ToNurbsCurve())), retain);
    }

    extension(HermiteSpline spline) {
        public IO<NurbsCurve> ToNurbs(Plane frame, Action<IDisposable> retain) =>
            DisposalOps.Within<IDisposable, NurbsCurve>(temporary =>
                from pieces in spline.Spans.TraverseM(span => Copies.Acquire(nameof(BezierCurve.ToNurbsCurve), _ => Answer.Successor(
                    new BezierCurve([span.Start.ToPoint3d(frame), span.StartControl.ToPoint3d(frame), span.EndControl.ToPoint3d(frame), span.End.ToPoint3d(frame)]),
                    static bezier => Answer.One(bezier.ToNurbsCurve())), temporary)).As()
                from joined in Appended(pieces, retain)
                select joined, DisposalOps.Release);
    }

    // --- [CATENARIES]
    public static IO<CatenaryCurve> CreateCatenaryCurve(CatenaryCall call, Action<IDisposable> retain, [CallerArgumentExpression(nameof(call))] string member = "") =>
        Copies.Acquire(member, _ => Answer.One(call(out Point3d apex, out double parameter, out double length, out double deviation))
            .Map(curve => new CatenaryCurve(curve, apex, parameter, length, deviation)), retain);
}
