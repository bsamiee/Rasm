using System.Globalization;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[Union<Point3d, double, double, Point3d>(T1Name = "ThroughPoint", T2Name = "FromLength", T3Name = "FromParameter", T4Name = "FromApex", MapMethods = SwitchMapMethodsGeneration.None)]
public readonly partial struct CatenaryForm;

public sealed record CatenaryCurve(Curve Curve, Point3d Apex, double Parameter, double Length, double MaxDeviation);

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<RhinoFault>]
public sealed partial class ConicRho {
    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref double value) =>
        validationError = value is > 0d and < 1d ? null : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"conic rho {value} lies outside (0, 1)"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Conic {
    public sealed record EllipseArc(Ellipse Ellipse, Interval Angles) : Conic;

    public sealed record Rho(Point3d Start, Point3d Apex, Point3d End, ConicRho Ratio) : Conic;

    public sealed record Hyperbola(Plane Plane, double A, double B, Interval U) : Conic;

    public IO<NurbsCurve> ToNurbs(Action<IDisposable> retain) =>
        Switch(
            retain,
            ellipseArc: static (held, arc) => Copies.Acquire(nameof(NurbsCurve.CreateFromArc), _ => NurbsCurve.CreateFromArc(new Arc(new Circle(arc.Ellipse.Plane, 1d), arc.Angles)) switch {
                var unit => (Answer.Accepted(unit?.Transform(Transform.Scale(arc.Ellipse.Plane, arc.Ellipse.Radius1, arc.Ellipse.Radius2, 1d)) is true), Answer.One(unit))
                    .Apply(static (_, scaled) => scaled).As(),
            }, held),
            rho: static (held, rho) => Rational(rho.Start, rho.Apex, rho.End, (double)rho.Ratio / (1d - (double)rho.Ratio), held),
            hyperbola: static (held, hyperbola) => ((hyperbola.U.T0 + hyperbola.U.T1) / 2d, (hyperbola.U.T1 - hyperbola.U.T0) / 2d) switch {
                var (middle, half) => Rational(
                    hyperbola.Plane.PointAt(hyperbola.A * Math.Cosh(hyperbola.U.T0), hyperbola.B * Math.Sinh(hyperbola.U.T0)),
                    hyperbola.Plane.PointAt(hyperbola.A * Math.Cosh(middle) / Math.Cosh(half), hyperbola.B * Math.Sinh(middle) / Math.Cosh(half)),
                    hyperbola.Plane.PointAt(hyperbola.A * Math.Cosh(hyperbola.U.T1), hyperbola.B * Math.Sinh(hyperbola.U.T1)),
                    Math.Cosh(half),
                    held),
            });

    private static IO<NurbsCurve> Rational(Point3d start, Point3d apex, Point3d end, double weight, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(BezierCurve.ToNurbsCurve), _ => Answer.Successor(
            new BezierCurve(new Point4d[] { new(start.X, start.Y, start.Z, 1d), new(apex.X * weight, apex.Y * weight, apex.Z * weight, weight), new(end.X, end.Y, end.Z, 1d) }),
            static bezier => Answer.One(bezier.ToNurbsCurve())), retain);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record HermiteSpline {
    private HermiteSpline(Seq<(Point3d Point, Vector3d In, Vector3d Out)> stations, bool closed) =>
        Spans = (closed ? stations + Seq(stations[0]) : stations) switch {
            var ends => ends.Zip(ends.Tail, static (from, to) => new[] { from.Point, from.Point + (from.Out / 3d), to.Point - (to.In / 3d), to.Point }).Strict(),
        };

    private Seq<Point3d[]> Spans { get; }

    public IO<NurbsCurve> ToNurbs(Action<IDisposable> retain) =>
        DisposalOps.Within<IDisposable, NurbsCurve>(temporary =>
            from pieces in Spans.TraverseM(span => Copies.Acquire(nameof(BezierCurve.ToNurbsCurve), _ =>
                Answer.Successor(new BezierCurve(span), static bezier => Answer.One(bezier.ToNurbsCurve())), temporary)).As()
            from joined in CurveConstruction.Appended(pieces, retain)
            select joined, DisposalOps.Release);

    public sealed record Stations : HermiteSpline {
        private Stations(Seq<(Point3d Point, Vector3d In, Vector3d Out)> points) : base(points, closed: false) => Points = points;

        public Seq<(Point3d Point, Vector3d In, Vector3d Out)> Points { get; }

        public static Fin<Stations> Validated(Seq<(Point3d Point, Vector3d In, Vector3d Out)> points) =>
            points.Count >= 2 ? new Stations(points) : Spanless(points.Count);
    }

    public sealed record Tcb : HermiteSpline {
        private Tcb(Seq<Point3d> points, double tension, double continuity, double bias, bool closed)
            : base(Kochanek(points, tension, continuity, bias, closed), closed) =>
            (Points, Tension, Continuity, Bias, Closed) = (points, tension, continuity, bias, closed);

        public Seq<Point3d> Points { get; }

        public double Tension { get; }

        public double Continuity { get; }

        public double Bias { get; }

        public bool Closed { get; }

        public static Fin<Tcb> Validated(Seq<Point3d> points, double tension, double continuity, double bias, bool closed) =>
            points.Count >= 2 ? new Tcb(points, tension, continuity, bias, closed) : Spanless(points.Count);

        private static Seq<(Point3d Point, Vector3d In, Vector3d Out)> Kochanek(Seq<Point3d> points, double tension, double continuity, double bias, bool closed) =>
            points.Map((point, index) => (
                    Before: point - (index > 0 ? points[index - 1] : closed ? points[points.Count - 1] : point + (point - points[1])),
                    After: (index < points.Count - 1 ? points[index + 1] : closed ? points[0] : point + (point - points[index - 1])) - point,
                    Point: point))
                .Map(chords => (
                    chords.Point,
                    In: ((1d - tension) * (1d + bias) * (1d - continuity) / 2d * chords.Before) + ((1d - tension) * (1d - bias) * (1d + continuity) / 2d * chords.After),
                    Out: ((1d - tension) * (1d + bias) * (1d + continuity) / 2d * chords.Before) + ((1d - tension) * (1d - bias) * (1d - continuity) / 2d * chords.After)))
                .Strict();
    }

    private static RhinoFault Spanless(int count) =>
        new(string.Create(CultureInfo.InvariantCulture, $"a Hermite spline needs at least 2 stations, received {count}"));
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

    // --- [CATENARIES]
    public static IO<CatenaryCurve> CreateCatenaryCurve(Point3d start, Point3d end, Vector3d axis, CatenaryForm form, bool smooth, int pointCount, Action<IDisposable> retain) =>
        form.Switch(
            (Start: start, End: end, Axis: axis, Smooth: smooth, Count: pointCount, Retain: retain),
            throughPoint: static (state, through) => Copies.Acquire(nameof(Curve.CreateCatenaryCurveThroughPoint), _ =>
                Answer.One(Curve.CreateCatenaryCurveThroughPoint(state.Start, state.End, state.Axis, through, state.Smooth, state.Count, out Point3d apex, out double parameter, out double length, out double deviation))
                    .Map(curve => new CatenaryCurve(curve, apex, parameter, length, deviation)), state.Retain),
            fromLength: static (state, length) => Copies.Acquire(nameof(Curve.CreateCatenaryCurveFromLength), _ =>
                Answer.One(Curve.CreateCatenaryCurveFromLength(state.Start, state.End, state.Axis, length, state.Smooth, state.Count, out Point3d apex, out double parameter, out double computedLength, out double deviation))
                    .Map(curve => new CatenaryCurve(curve, apex, parameter, computedLength, deviation)), state.Retain),
            fromParameter: static (state, parameter) => Copies.Acquire(nameof(Curve.CreateCatenaryCurveFromParameter), _ =>
                Answer.One(Curve.CreateCatenaryCurveFromParameter(state.Start, state.End, state.Axis, parameter, state.Smooth, state.Count, out Point3d apex, out double computedParameter, out double length, out double deviation))
                    .Map(curve => new CatenaryCurve(curve, apex, computedParameter, length, deviation)), state.Retain),
            fromApex: static (state, apex) => Copies.Acquire(nameof(Curve.CreateCatenaryCurveFromApex), _ =>
                Answer.One(Curve.CreateCatenaryCurveFromApex(state.Start, state.End, state.Axis, apex, state.Smooth, state.Count, out Point3d computedApex, out double parameter, out double length, out double deviation))
                    .Map(curve => new CatenaryCurve(curve, computedApex, parameter, length, deviation)), state.Retain));
}
