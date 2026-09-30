using Rasm.Rhino.Document;
using Rhino.Geometry.Intersect;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlendMethod {
    public sealed record EndToEnd(BlendContinuity Continuity, Option<(double A, double B)> Bulge) : BlendMethod;

    public sealed record AtParameters(double T0, bool Reverse0, BlendContinuity Continuity0, double T1, bool Reverse1, BlendContinuity Continuity1) : BlendMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArcBlendMethod {
    public sealed record ControlPointRatio(double Ratio) : ArcBlendMethod;

    public sealed record LineArcRadius(double Radius) : ArcBlendMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TweenMethod {
    public sealed record Plain() : TweenMethod;

    public sealed record Matched() : TweenMethod;

    public sealed record Sampled(int Samples) : TweenMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InterpolationMethod {
    public sealed record Plain(int Degree) : InterpolationMethod;

    public sealed record Knotted(int Degree, CurveKnotStyle Knots) : InterpolationMethod;

    public sealed record Tangent(int Degree, CurveKnotStyle Knots, Vector3d Start, Vector3d End) : InterpolationMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FitPointsMethod {
    public sealed record Plain(double Tolerance, bool Periodic) : FitPointsMethod;

    public sealed record Tangent(double Tolerance, int Degree, bool Periodic, Vector3d Start, Vector3d End) : FitPointsMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpiralMethod {
    public sealed record AboutAxis(Point3d AxisStart, Vector3d AxisDirection) : SpiralMethod;

    public sealed record AlongRail(Curve Rail, double T0, double T1, int PointsPerTurn) : SpiralMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParabolaSource {
    public sealed record FromVertex(Point3d Vertex, Point3d Start, Point3d End) : ParabolaSource;

    public sealed record FromFocus(Point3d Focus, Point3d Start, Point3d End) : ParabolaSource;

    public sealed record FromPoints(Point3d Start, Point3d InnerPoint, Point3d End) : ParabolaSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalyticCurve {
    public sealed record OfLine(Line Line) : AnalyticCurve;

    public sealed record OfArc(Arc Arc, Option<(int Degree, int CvCount)> Structure) : AnalyticCurve;

    public sealed record OfCircle(Circle Circle, Option<(int Degree, int CvCount)> Structure) : AnalyticCurve;

    public sealed record OfEllipse(Ellipse Ellipse, Option<Interval> AngleRadians) : AnalyticCurve;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CatenaryMethod {
    public sealed record ThroughPoint(Point3d Point) : CatenaryMethod;

    public sealed record FromLength(double Length) : CatenaryMethod;

    public sealed record FromParameter(double Parameter) : CatenaryMethod;

    public sealed record FromApex(Point3d Apex) : CatenaryMethod;
}

public sealed record CatenaryResult(Curve Curve, Point3d Apex, double Parameter, double Length, double MaxDeviation);

public sealed record CurveJoinResult(Seq<Curve> Curves, Seq<int> Key);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveBooleanMethod {
    public sealed record Union(Seq<Curve> Curves) : CurveBooleanMethod;

    public sealed record Intersection(Curve A, Curve B) : CurveBooleanMethod;

    public sealed record Difference(Curve A, Seq<Curve> Subtractors) : CurveBooleanMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LineCircleCrossing {
    public sealed record Single(double T, Point3d Point) : LineCircleCrossing;

    public sealed record Multiple(double T1, Point3d Point1, double T2, Point3d Point2) : LineCircleCrossing;

    public Fin<Multiple> Secant =>
        Switch<Fin<Multiple>>(single: static _ => new Missing(nameof(Multiple)), multiple: static both => both);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveConstruction {
    // --- [LIMITS]
    private static readonly Limits<double> ArcBezierSlider = Limits.AtLeast(0.0).AtMost(1.0);

    // --- [BLENDS]
    public static IO<Curve> Blend(Curve a, Curve b, BlendMethod method) =>
        method.Switch(
            (A: a, B: b),
            endToEnd: static (pair, end) => GeometryResults.Acquire(
                () => end.Bulge.Case is (double bulgeA, double bulgeB)
                    ? Curve.CreateBlendCurve(pair.A, pair.B, end.Continuity, bulgeA, bulgeB)
                    : Curve.CreateBlendCurve(pair.A, pair.B, end.Continuity),
                nameof(Curve.CreateBlendCurve)),
            atParameters: static (pair, at) => GeometryResults.Acquire(
                () => (OutOfDomain.Unless(pair.A.Domain, at.T0, nameof(Curve.CreateBlendCurve)), OutOfDomain.Unless(pair.B.Domain, at.T1, nameof(Curve.CreateBlendCurve)))
                    .Apply((_, _) => Curve.CreateBlendCurve(pair.A, at.T0, at.Reverse0, at.Continuity0, pair.B, at.T1, at.Reverse1, at.Continuity1))
                    .As(),
                nameof(Curve.CreateBlendCurve)));

    public static IO<Curve> ArcBlend(Point3d start, Vector3d startDirection, Point3d end, Vector3d endDirection, ArcBlendMethod method) =>
        method.Switch(
            (Start: start, StartDirection: startDirection, End: end, EndDirection: endDirection),
            controlPointRatio: static (ends, ratio) => GeometryResults.Acquire(() => Curve.CreateArcBlend(ends.Start, ends.StartDirection, ends.End, ends.EndDirection, ratio.Ratio), nameof(Curve.CreateArcBlend)),
            lineArcRadius: static (ends, radius) => GeometryResults.Acquire(() => Curve.CreateArcLineArcBlend(ends.Start, ends.StartDirection, ends.End, ends.EndDirection, radius.Radius), nameof(Curve.CreateArcLineArcBlend)));

    public static IO<Seq<Curve>> FilletCurves(Curve curve0, Point3d point0, Curve curve1, Point3d point1, double radius, bool join, bool trim, bool arcExtension, double tolerance, double angleTolerance) =>
        GeometryResults.Acquire(() => Curve.CreateFilletCurves(curve0, point0, curve1, point1, radius, @join, trim, arcExtension, tolerance, angleTolerance), nameof(Curve.CreateFilletCurves), emptyFails: false);

    public static IO<Curve> FilletCorners(Curve curve, double radius, double tolerance, double angleTolerance) =>
        GeometryResults.Acquire(() => Curve.CreateFilletCornersCurve(curve, radius, tolerance, angleTolerance), nameof(Curve.CreateFilletCornersCurve));

    public static IO<Seq<Curve>> Tween(Curve curve0, Curve curve1, int count, TweenMethod method, double tolerance) =>
        method.Switch(
            (Curve0: curve0, Curve1: curve1, ItemCount: count, Tolerance: tolerance),
            plain: static (state, _) => GeometryResults.Acquire(() => Curve.CreateTweenCurves(state.Curve0, state.Curve1, state.ItemCount, state.Tolerance), nameof(Curve.CreateTweenCurves), emptyFails: true),
            matched: static (state, _) => GeometryResults.Acquire(() => Curve.CreateTweenCurvesWithMatching(state.Curve0, state.Curve1, state.ItemCount, state.Tolerance), nameof(Curve.CreateTweenCurvesWithMatching), emptyFails: true),
            sampled: static (state, sampled) => GeometryResults.Acquire(
                () => Curve.CreateTweenCurvesWithSampling(state.Curve0, state.Curve1, state.ItemCount, sampled.Samples, state.Tolerance),
                nameof(Curve.CreateTweenCurvesWithSampling),
                emptyFails: true));

    public static IO<Seq<Curve>> Match(Curve curve0, bool reverse0, BlendContinuity continuity, Curve curve1, bool reverse1, PreserveEnd preserve, bool average) =>
        GeometryResults.Acquire(() => Curve.CreateMatchCurve(curve0, reverse0, continuity, curve1, reverse1, preserve, average), nameof(Curve.CreateMatchCurve), emptyFails: true);

    public static IO<Curve> Mean(Curve a, Curve b, double angleToleranceRadians) =>
        GeometryResults.Acquire(() => Curve.CreateMeanCurve(a, b, angleToleranceRadians), nameof(Curve.CreateMeanCurve));

    public static IO<Seq<Curve>> TwoView(Curve a, Curve b, Vector3d vectorA, Vector3d vectorB, double tolerance, double angleTolerance) =>
        GeometryResults.Acquire(() => Curve.CreateCurve2View(a, b, vectorA, vectorB, tolerance, angleTolerance), nameof(Curve.CreateCurve2View), emptyFails: false);

    // --- [POINTS]
    public static IO<Curve> Interpolated(Seq<Point3d> points, InterpolationMethod method) =>
        method.Switch(
                points,
                plain: static (through, plain) => GeometryResults.Acquire(() => Curve.CreateInterpolatedCurve(through, plain.Degree), nameof(Curve.CreateInterpolatedCurve)),
                knotted: static (through, knotted) => GeometryResults.Acquire(() => Curve.CreateInterpolatedCurve(through, knotted.Degree, knotted.Knots), nameof(Curve.CreateInterpolatedCurve)),
                tangent: static (through, tangent) => GeometryResults.Acquire(() => Curve.CreateInterpolatedCurve(through, tangent.Degree, tangent.Knots, tangent.Start, tangent.End), nameof(Curve.CreateInterpolatedCurve)))
            .Catch(static error => error.HasException<InvalidOperationException>(), static _ => IO.fail<Curve>(new Invalid(nameof(points))));

    public static IO<Curve> ControlPoints(Seq<Point3d> points, int degree) =>
        GeometryResults.Acquire(() => Curve.CreateControlPointCurve(points, degree), nameof(Curve.CreateControlPointCurve));

    public static IO<NurbsCurve> FitPoints(Seq<Point3d> points, FitPointsMethod method) =>
        method.Switch(
            points,
            plain: static (through, plain) => GeometryResults.Acquire(() => NurbsCurve.CreateFromFitPoints(through, plain.Tolerance, plain.Periodic), nameof(NurbsCurve.CreateFromFitPoints)),
            tangent: static (through, tangent) => GeometryResults.Acquire(
                () => NurbsCurve.CreateFromFitPoints(through, tangent.Tolerance, tangent.Degree, tangent.Periodic, tangent.Start, tangent.End),
                nameof(NurbsCurve.CreateFromFitPoints)));

    public static IO<NurbsCurve> HSpline(Seq<Point3d> points, Option<(Vector3d Start, Vector3d End)> tangents) =>
        GeometryResults.Acquire(() => tangents.Case is (Vector3d start, Vector3d end) ? NurbsCurve.CreateHSpline(points, start, end) : NurbsCurve.CreateHSpline(points), nameof(NurbsCurve.CreateHSpline));

    public static IO<NurbsCurve> SubDFriendlyPoints(Seq<Point3d> points, bool interpolate, bool periodicClosed) =>
        GeometryResults.Acquire(() => NurbsCurve.CreateSubDFriendly(points, interpolate, periodicClosed), nameof(NurbsCurve.CreateSubDFriendly));

    // --- [ANALYTIC]
    public static IO<NurbsCurve> Spiral(SpiralMethod method, Point3d radiusPoint, double pitch, double turnCount, double radius0, double radius1) =>
        method.Switch(
            (RadiusPoint: radiusPoint, Pitch: pitch, TurnCount: turnCount, Radius0: radius0, Radius1: radius1),
            aboutAxis: static (state, axis) => GeometryResults.Acquire(
                () => NurbsCurve.CreateSpiral(axis.AxisStart, axis.AxisDirection, state.RadiusPoint, state.Pitch, state.TurnCount, state.Radius0, state.Radius1),
                nameof(NurbsCurve.CreateSpiral)),
            alongRail: static (state, rail) => GeometryResults.Acquire(
                () => NurbsCurve.CreateSpiral(rail.Rail, rail.T0, rail.T1, state.RadiusPoint, state.Pitch, state.TurnCount, state.Radius0, state.Radius1, rail.PointsPerTurn),
                nameof(NurbsCurve.CreateSpiral)));

    public static IO<NurbsCurve> Parabola(ParabolaSource source) =>
        source.Switch(
            fromVertex: static vertex => GeometryResults.Acquire(() => NurbsCurve.CreateParabolaFromVertex(vertex.Vertex, vertex.Start, vertex.End), nameof(NurbsCurve.CreateParabolaFromVertex)),
            fromFocus: static focus => GeometryResults.Acquire(() => NurbsCurve.CreateParabolaFromFocus(focus.Focus, focus.Start, focus.End), nameof(NurbsCurve.CreateParabolaFromFocus)),
            fromPoints: static points => GeometryResults.Acquire(() => NurbsCurve.CreateParabolaFromPoints(points.Start, points.InnerPoint, points.End), nameof(NurbsCurve.CreateParabolaFromPoints)));

    public static IO<NurbsCurve> ArcBezier(int degree, Point3d center, Point3d start, Point3d end, double radius, double tanSlider, double midSlider) =>
        GeometryResults.Acquire(
            () => (ArcBezierSlider.Check(tanSlider, nameof(tanSlider)), ArcBezierSlider.Check(midSlider, nameof(midSlider)))
                .Apply((tangent, mid) => NurbsCurve.CreateNonRationalArcBezier(degree, center, start, end, radius, tangent, mid))
                .As(),
            nameof(NurbsCurve.CreateNonRationalArcBezier));

    public static IO<NurbsCurve> Analytic(AnalyticCurve source) =>
        source.Switch(
            ofLine: static line => GeometryResults.Acquire(() => NurbsCurve.CreateFromLine(line.Line), nameof(NurbsCurve.CreateFromLine)),
            ofArc: static arc => GeometryResults.Acquire(
                () => arc.Structure.Case is (int degree, int cvCount) ? NurbsCurve.CreateFromArc(arc.Arc, degree, cvCount) : NurbsCurve.CreateFromArc(arc.Arc),
                nameof(NurbsCurve.CreateFromArc)),
            ofCircle: static circle => GeometryResults.Acquire(
                () => circle.Structure.Case is (int degree, int cvCount) ? NurbsCurve.CreateFromCircle(circle.Circle, degree, cvCount) : NurbsCurve.CreateFromCircle(circle.Circle),
                nameof(NurbsCurve.CreateFromCircle)),
            ofEllipse: static ellipse => ellipse.AngleRadians.Match(
                Some: angles =>
                    from arc in GeometryResults.Acquire(() => NurbsCurve.CreateFromArc(new Arc(new Circle(ellipse.Ellipse.Plane, 1.0), angles)), nameof(NurbsCurve.CreateFromArc))
                    from scaled in DisposalOps.OnFailure(
                        IO.lift(() => Refused.Unless(arc.Transform(Transform.Scale(ellipse.Ellipse.Plane, ellipse.Ellipse.Radius1, ellipse.Ellipse.Radius2, 1.0)), arc, nameof(GeometryBase.Transform))),
                        IO.lift(arc.Dispose))
                    select scaled,
                None: () => GeometryResults.Acquire(() => NurbsCurve.CreateFromEllipse(ellipse.Ellipse), nameof(NurbsCurve.CreateFromEllipse))));

    public static IO<CatenaryResult> Catenary(Point3d start, Point3d end, Vector3d axis, CatenaryMethod method, bool smooth, int pointCount) =>
        from answer in IO.lift(() => method.Switch(
            (Start: start, End: end, Axis: axis, Smooth: smooth, PointCount: pointCount),
            throughPoint: static (state, through) => (
                Curve: Curve.CreateCatenaryCurveThroughPoint(state.Start, state.End, state.Axis, through.Point, state.Smooth, state.PointCount, out Point3d apex, out double parameter, out double length, out double deviation),
                Result: (Apex: apex, Parameter: parameter, Length: length, Deviation: deviation),
                Member: nameof(Curve.CreateCatenaryCurveThroughPoint)),
            fromLength: static (state, ofLength) => (
                Curve: Curve.CreateCatenaryCurveFromLength(state.Start, state.End, state.Axis, ofLength.Length, state.Smooth, state.PointCount, out Point3d apex, out double parameter, out double length, out double deviation),
                Result: (Apex: apex, Parameter: parameter, Length: length, Deviation: deviation),
                Member: nameof(Curve.CreateCatenaryCurveFromLength)),
            fromParameter: static (state, ofParameter) => (
                Curve: Curve.CreateCatenaryCurveFromParameter(state.Start, state.End, state.Axis, ofParameter.Parameter, state.Smooth, state.PointCount, out Point3d apex, out double parameter, out double length, out double deviation),
                Result: (Apex: apex, Parameter: parameter, Length: length, Deviation: deviation),
                Member: nameof(Curve.CreateCatenaryCurveFromParameter)),
            fromApex: static (state, ofApex) => (
                Curve: Curve.CreateCatenaryCurveFromApex(state.Start, state.End, state.Axis, ofApex.Apex, state.Smooth, state.PointCount, out Point3d apex, out double parameter, out double length, out double deviation),
                Result: (Apex: apex, Parameter: parameter, Length: length, Deviation: deviation),
                Member: nameof(Curve.CreateCatenaryCurveFromApex))))
        from curve in GeometryResults.Acquire(() => answer.Curve, answer.Member)
        select new CatenaryResult(curve, answer.Result.Apex, answer.Result.Parameter, answer.Result.Length, answer.Result.Deviation);

    // --- [TEXT]
    private const int BoldStyle = 1;

    private const int ItalicStyle = 2;

    public static IO<Seq<Curve>> TextOutlines(string text, string font, double height, bool bold, bool italic, bool closeLoops, Plane plane, double smallCapsScale, double tolerance) =>
        GeometryResults.Acquire(
            () => (Invalid.Unless(text.Length > 0, nameof(text)), Invalid.Unless(plane.IsValid, nameof(plane)))
                .Apply((_, _) => Curve.CreateTextOutlines(text, font, height, (bold ? BoldStyle : 0) | (italic ? ItalicStyle : 0), closeLoops, plane, smallCapsScale, tolerance))
                .As(),
            nameof(Curve.CreateTextOutlines),
            emptyFails: false);

    // --- [JOINS]
    public static IO<CurveJoinResult> Join(Seq<Curve> curves, double tolerance, bool preserveDirection, bool simpleJoin) =>
        from answer in IO.lift(() => (Curves: Curve.JoinCurves(curves, tolerance, preserveDirection, simpleJoin, out int[] key), Key: key))
        from joined in GeometryResults.Acquire(() => answer.Curves, nameof(Curve.JoinCurves), emptyFails: false)
        select new CurveJoinResult(joined, toSeq(answer.Key));

    // --- [INTERSECTIONS]
    public static Fin<(double A, double B)> LineLine(Line lineA, Line lineB) =>
        Intersection.LineLine(lineA, lineB, out double a, out double b) ? (a, b) : new ParallelLines();

    public static Fin<LineCircleCrossing> LineCircle(Line line, Circle circle) =>
        Intersection.LineCircle(line, circle, out double t1, out Point3d point1, out double t2, out Point3d point2) switch {
            LineCircleIntersection.None => new Missing(nameof(Intersection.LineCircle)),
            LineCircleIntersection.Single => new LineCircleCrossing.Single(t1, point1),
            LineCircleIntersection.Multiple => new LineCircleCrossing.Multiple(t1, point1, t2, point2),
        };

    // --- [BOOLEANS]
    public static IO<Seq<Curve>> Boolean(CurveBooleanMethod method, double tolerance) =>
        method.Switch(
            tolerance,
            union: static (within, union) => GeometryResults.Acquire(() => Curve.CreateBooleanUnion(union.Curves, within), nameof(Curve.CreateBooleanUnion), emptyFails: true),
            intersection: static (within, both) => GeometryResults.Acquire(() => Curve.CreateBooleanIntersection(both.A, both.B, within), nameof(Curve.CreateBooleanIntersection), emptyFails: false),
            difference: static (within, difference) => GeometryResults.Acquire(() => Curve.CreateBooleanDifference(difference.A, difference.Subtractors, within), nameof(Curve.CreateBooleanDifference), emptyFails: false));

    public static IO<Seq<Seq<Curve>>> Regions(Seq<Curve> curves, Plane plane, Seq<Point3d> points, bool combine, double tolerance) =>
        DisposalOps.Using(
            IO.lift(() => Invalid.Unless(plane.IsValid, nameof(plane)).Bind(_ => Missing.Unless(
                points.IsEmpty ? Curve.CreateBooleanRegions(curves, plane, combine, tolerance) : Curve.CreateBooleanRegions(curves, plane, points, combine, tolerance),
                nameof(Curve.CreateBooleanRegions)))),
            static regions => DisposalOps.AcquireAll(
                toSeq(Range(0, regions.RegionCount)).Map(index => GeometryResults.Acquire(() => regions.RegionCurves(index), nameof(CurveBooleanRegions.RegionCurves), emptyFails: true)),
                static held => DisposalOps.Release(held.Flatten())));
}
