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

public sealed record CurveMatchOptions(bool Reverse0, BlendContinuity Continuity, bool Reverse1, PreserveEnd Preserve, bool Average);

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

public sealed record RailFilletOptions(int RailDegree, int ArcDegree, double Slider0, double Slider1, int BezierSurfaces, bool Extend, FilletSurfaceSplitType Split);

public sealed record RailFilletResult(Seq<Brep> Fillets, Seq<Brep> Trimmed0, Seq<Brep> Trimmed1, Seq<double> FitResults);

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
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveConstruction {
    // --- [LIMITS]
    private const int MinimumPoints = 2;

    private const int QuadraticDegree = 2;

    private const int CubicDegree = 3;

    private const int QuinticDegree = 5;

    private const int MinimumOpenPoints = 4;

    private const int MinimumPeriodicPoints = 6;

    private static readonly Fin<Limits<double>> RailSlider = Limits.AtLeast(-1.0).AtMost(1.0, nameof(RailSlider));

    private static readonly Fin<Limits<double>> ArcBezierSlider = Limits.AtLeast(0.0).AtMost(1.0, nameof(ArcBezierSlider));

    private static readonly Fin<Limits<int>> RailFilletArcDegree = Limits.AtLeast(QuadraticDegree).AtMost(QuinticDegree, nameof(RailFilletArcDegree));

    internal static readonly Fin<Limits<int>> ArcDegree = Limits.AtLeast(CubicDegree).AtMost(QuinticDegree, nameof(ArcDegree));

    // --- [BLENDS]
    public static IO<Curve> Blend(Curve a, Curve b, BlendMethod method) =>
        IO.lift(() => method.Switch(
            (A: a, B: b),
            endToEnd: static (pair, end) => Missing.Unless(end.Bulge.Case is (double bulgeA, double bulgeB)
                    ? Curve.CreateBlendCurve(pair.A, pair.B, end.Continuity, bulgeA, bulgeB)
                    : Curve.CreateBlendCurve(pair.A, pair.B, end.Continuity), nameof(Curve.CreateBlendCurve)),
            atParameters: static (pair, at) =>
                from inside0 in OutOfDomain.Unless(pair.A.Domain, at.T0, nameof(Curve.CreateBlendCurve))
                from inside1 in OutOfDomain.Unless(pair.B.Domain, at.T1, nameof(Curve.CreateBlendCurve))
                from blend in Missing.Unless(Curve.CreateBlendCurve(pair.A, at.T0, at.Reverse0, at.Continuity0, pair.B, at.T1, at.Reverse1, at.Continuity1), nameof(Curve.CreateBlendCurve))
                select blend));

    public static IO<Curve> ArcBlend(Point3d start, Vector3d startDirection, Point3d end, Vector3d endDirection, ArcBlendMethod method) =>
        IO.lift(() => method.Switch(
            (Start: start, StartDirection: startDirection, End: end, EndDirection: endDirection),
            controlPointRatio: static (ends, ratio) => Missing.Unless(Curve.CreateArcBlend(ends.Start, ends.StartDirection, ends.End, ends.EndDirection, ratio.Ratio), nameof(Curve.CreateArcBlend)),
            lineArcRadius: static (ends, radius) =>
                from positive in Limits.Above(0.0).Check(radius.Radius, nameof(ArcBlendMethod.LineArcRadius.Radius))
                from blend in Missing.Unless(Curve.CreateArcLineArcBlend(ends.Start, ends.StartDirection, ends.End, ends.EndDirection, positive), nameof(Curve.CreateArcLineArcBlend))
                select blend));

    public static IO<Seq<Curve>> FilletCurves(Curve curve0, Point3d point0, Curve curve1, Point3d point1, double radius, bool join, bool trim, bool arcExtension, double tolerance, double angleTolerance) =>
        from positive in IO.lift(() => Limits.Above(0.0).Check(radius, nameof(radius)))
        from fillets in GeometryResults.Acquire(() => Curve.CreateFilletCurves(curve0, point0, curve1, point1, positive, @join, trim, arcExtension, tolerance, angleTolerance), nameof(Curve.CreateFilletCurves))
        select fillets;

    public static IO<Curve> FilletCorners(Curve curve, double radius, double tolerance, double angleTolerance) =>
        IO.lift(() => Missing.Unless(Curve.CreateFilletCornersCurve(curve, radius, tolerance, angleTolerance), nameof(Curve.CreateFilletCornersCurve)));

    public static IO<Seq<Curve>> Tween(Curve curve0, Curve curve1, int count, TweenMethod method, double tolerance) =>
        from positive in IO.lift(() => Limits.AtLeast(1).Check(count, nameof(count)))
        from tweens in method.Switch(
            (Curve0: curve0, Curve1: curve1, ItemCount: positive, Tolerance: tolerance),
            plain: static (state, _) => GeometryResults.Acquire(() => Curve.CreateTweenCurves(state.Curve0, state.Curve1, state.ItemCount, state.Tolerance), nameof(Curve.CreateTweenCurves)),
            matched: static (state, _) => GeometryResults.Acquire(() => Curve.CreateTweenCurvesWithMatching(state.Curve0, state.Curve1, state.ItemCount, state.Tolerance), nameof(Curve.CreateTweenCurvesWithMatching)),
            sampled: static (state, sampled) =>
                from samples in IO.lift(() => Limits.AtLeast(1).Check(sampled.Samples, nameof(TweenMethod.Sampled.Samples)))
                from curves in GeometryResults.Acquire(() => Curve.CreateTweenCurvesWithSampling(state.Curve0, state.Curve1, state.ItemCount, samples, state.Tolerance), nameof(Curve.CreateTweenCurvesWithSampling))
                select curves)
        select tweens;

    public static IO<Seq<Curve>> Match(Curve curve0, Curve curve1, CurveMatchOptions options) =>
        GeometryResults.Acquire(() => Curve.CreateMatchCurve(curve0, options.Reverse0, options.Continuity, curve1, options.Reverse1, options.Preserve, options.Average), nameof(Curve.CreateMatchCurve));

    public static IO<Curve> Mean(Curve a, Curve b, double angleToleranceRadians) =>
        IO.lift(() => Missing.Unless(Curve.CreateMeanCurve(a, b, angleToleranceRadians), nameof(Curve.CreateMeanCurve)));

    public static IO<Seq<Curve>> TwoView(Curve a, Curve b, Vector3d vectorA, Vector3d vectorB, double tolerance, double angleTolerance) =>
        GeometryResults.Acquire(() => Curve.CreateCurve2View(a, b, vectorA, vectorB, tolerance, angleTolerance), nameof(Curve.CreateCurve2View));

    public static IO<(Curve A, Curve B)> MakeEndsMeet(Curve a, bool adjustStartA, Curve b, bool adjustStartB) =>
        from handle in DuplicateMode.Duplicate.Acquire(a)
        from copyB in GeometryOps.OnFailure(
            GeometryOps.EditCopy(b, copy => Refused.Unless(Curve.MakeEndsMeet(handle.Value, adjustStartA, copy, adjustStartB), nameof(Curve.MakeEndsMeet))),
            IO.lift(handle.Dispose))
        select (handle.Value, copyB.Copy);

    public static IO<RailFilletResult> RailFillet(Curve rail, BrepFace first, BrepFace second, double u, double v, RailFilletOptions options, double tolerance) =>
        from valid in IO.lift(() =>
            from railDegree in Invalid.Unless(options.RailDegree is CubicDegree or QuinticDegree, nameof(RailFilletOptions.RailDegree))
            from arcDegree in RailFilletArcDegree.Bind(limits => limits.Check(options.ArcDegree, nameof(RailFilletOptions.ArcDegree)))
            from slider0 in RailSlider.Bind(limits => limits.Check(options.Slider0, nameof(RailFilletOptions.Slider0)))
            from slider1 in RailSlider.Bind(limits => limits.Check(options.Slider1, nameof(RailFilletOptions.Slider1)))
            select (ArcDegree: arcDegree, Slider0: slider0, Slider1: slider1))
        from answer in IO.lift(() => {
            List<Brep> fillets = [];
            List<Brep> trimmed0 = [];
            List<Brep> trimmed1 = [];
            bool accepted = rail.FilletSurfaceToRail(first, second, u, v, options.RailDegree, valid.ArcDegree, [valid.Slider0, valid.Slider1], options.BezierSurfaces, options.Extend, options.Split, tolerance, fillets, trimmed0, trimmed1, out double[] fitResults);
            return (Accepted: accepted, Result: new RailFilletResult(toSeq(fillets), toSeq(trimmed0), toSeq(trimmed1), toSeq(fitResults)));
        })
        from accepted in GeometryOps.OnFailure(
            IO.lift(() => Refused.Unless(answer.Accepted, nameof(Curve.FilletSurfaceToRail))),
            Disposal.Release(answer.Result.Fillets + answer.Result.Trimmed0 + answer.Result.Trimmed1))
        select answer.Result;

    // --- [POINTS]
    public static IO<Curve> Interpolated(Seq<Point3d> points, InterpolationMethod method) =>
        IO.lift(() =>
            from enough in Limits.AtLeast(MinimumPoints).Check(points.Count, nameof(points))
            from curve in method.Switch(
                points,
                plain: static (through, plain) => Missing.Unless(Curve.CreateInterpolatedCurve(through, plain.Degree), nameof(Curve.CreateInterpolatedCurve)),
                knotted: static (through, knotted) => Missing.Unless(Curve.CreateInterpolatedCurve(through, knotted.Degree, knotted.Knots), nameof(Curve.CreateInterpolatedCurve)),
                tangent: static (through, tangent) => Missing.Unless(Curve.CreateInterpolatedCurve(through, tangent.Degree, tangent.Knots, tangent.Start, tangent.End), nameof(Curve.CreateInterpolatedCurve)))
            select curve);

    public static IO<Curve> ControlPoints(Seq<Point3d> points, int degree) =>
        IO.lift(() => Missing.Unless(Curve.CreateControlPointCurve(points, degree), nameof(Curve.CreateControlPointCurve)));

    public static IO<NurbsCurve> FitPoints(Seq<Point3d> points, FitPointsMethod method) =>
        IO.lift(() => method.Switch(
            points,
            plain: static (through, plain) => Missing.Unless(NurbsCurve.CreateFromFitPoints(through, plain.Tolerance, plain.Periodic), nameof(NurbsCurve.CreateFromFitPoints)),
            tangent: static (through, tangent) => Missing.Unless(NurbsCurve.CreateFromFitPoints(through, tangent.Tolerance, tangent.Degree, tangent.Periodic, tangent.Start, tangent.End), nameof(NurbsCurve.CreateFromFitPoints))));

    public static IO<NurbsCurve> HSpline(Seq<Point3d> points, Option<(Vector3d Start, Vector3d End)> tangents) =>
        IO.lift(() => Missing.Unless(tangents.Case is (Vector3d start, Vector3d end) ? NurbsCurve.CreateHSpline(points, start, end) : NurbsCurve.CreateHSpline(points), nameof(NurbsCurve.CreateHSpline)));

    public static IO<NurbsCurve> SubDFriendlyPoints(Seq<Point3d> points, bool interpolate, bool periodicClosed) =>
        IO.lift(() => Missing.Unless(NurbsCurve.CreateSubDFriendly(points, interpolate, periodicClosed), nameof(NurbsCurve.CreateSubDFriendly)));

    // --- [REBUILDS]
    public static IO<Curve> SoftEdit(Curve curve, double t, Vector3d delta, double length, bool fixEnds) =>
        IO.lift(() => OutOfDomain.Unless(curve.Domain, t, nameof(Curve.CreateSoftEditCurve))
            .Bind(_ => Missing.Unless(Curve.CreateSoftEditCurve(curve, t, delta, length, fixEnds), nameof(Curve.CreateSoftEditCurve))));

    public static IO<Curve> Periodic(Curve curve, bool smooth) =>
        IO.lift(() => Missing.Unless(Curve.CreatePeriodicCurve(curve, smooth), nameof(Curve.CreatePeriodicCurve)));

    public static IO<NurbsCurve> SubDFriendly(Curve curve, Option<(int PointCount, bool PeriodicClosed)> size) =>
        IO.lift(() =>
            from sized in size.Match(
                Some: static sized => Limits.AtLeast(sized.PeriodicClosed ? MinimumPeriodicPoints : MinimumOpenPoints).Check(sized.PointCount, nameof(sized.PointCount)).Map(pointCount => Some((PointCount: pointCount, sized.PeriodicClosed))),
                None: static () => Fin.Succ(Option<(int PointCount, bool PeriodicClosed)>.None))
            from friendly in Missing.Unless(sized.Match(
                    Some: valid => NurbsCurve.CreateSubDFriendly(curve, valid.PointCount, valid.PeriodicClosed),
                    None: () => NurbsCurve.CreateSubDFriendly(curve)), nameof(NurbsCurve.CreateSubDFriendly))
            select friendly);

    public static IO<Seq<NurbsCurve>> Compatible(Seq<Curve> curves, Option<Point3d> start, Option<Point3d> end, int simplifyMethod, int pointCount, double refitTolerance, double angleTolerance) =>
        from filled in IO.lift(() => Invalid.Unless(!curves.IsEmpty, nameof(curves)))
        from compatible in GeometryResults.Acquire(
            () => NurbsCurve.MakeCompatible(curves, start.IfNone(Point3d.Unset), end.IfNone(Point3d.Unset), simplifyMethod, pointCount, refitTolerance, angleTolerance),
            nameof(NurbsCurve.MakeCompatible))
        select compatible;

    // --- [ANALYTIC]
    public static IO<NurbsCurve> Spiral(SpiralMethod method, Point3d radiusPoint, double pitch, double turnCount, double radius0, double radius1) =>
        IO.lift(() => method.Switch(
            (RadiusPoint: radiusPoint, Pitch: pitch, TurnCount: turnCount, Radius0: radius0, Radius1: radius1),
            aboutAxis: static (state, axis) => Missing.Unless(NurbsCurve.CreateSpiral(axis.AxisStart, axis.AxisDirection, state.RadiusPoint, state.Pitch, state.TurnCount, state.Radius0, state.Radius1), nameof(NurbsCurve.CreateSpiral)),
            alongRail: static (state, rail) =>
                from pointsPerTurn in Limits.AtLeast(1).Check(rail.PointsPerTurn, nameof(SpiralMethod.AlongRail.PointsPerTurn))
                from spiral in Missing.Unless(NurbsCurve.CreateSpiral(rail.Rail, rail.T0, rail.T1, state.RadiusPoint, state.Pitch, state.TurnCount, state.Radius0, state.Radius1, pointsPerTurn), nameof(NurbsCurve.CreateSpiral))
                select spiral));

    public static IO<NurbsCurve> Parabola(ParabolaSource source) =>
        IO.lift(() => source.Switch(
            fromVertex: static vertex => Missing.Unless(NurbsCurve.CreateParabolaFromVertex(vertex.Vertex, vertex.Start, vertex.End), nameof(NurbsCurve.CreateParabolaFromVertex)),
            fromFocus: static focus => Missing.Unless(NurbsCurve.CreateParabolaFromFocus(focus.Focus, focus.Start, focus.End), nameof(NurbsCurve.CreateParabolaFromFocus)),
            fromPoints: static points => Missing.Unless(NurbsCurve.CreateParabolaFromPoints(points.Start, points.InnerPoint, points.End), nameof(NurbsCurve.CreateParabolaFromPoints))));

    public static IO<NurbsCurve> ArcBezier(int degree, Point3d center, Point3d start, Point3d end, double radius, double tanSlider, double midSlider) =>
        IO.lift(() =>
            from cubicToQuintic in ArcDegree.Bind(limits => limits.Check(degree, nameof(degree)))
            from tangent in ArcBezierSlider.Bind(limits => limits.Check(tanSlider, nameof(tanSlider)))
            from mid in ArcBezierSlider.Bind(limits => limits.Check(midSlider, nameof(midSlider)))
            from bezier in Missing.Unless(NurbsCurve.CreateNonRationalArcBezier(cubicToQuintic, center, start, end, radius, tangent, mid), nameof(NurbsCurve.CreateNonRationalArcBezier))
            select bezier);

    public static IO<NurbsCurve> Analytic(AnalyticCurve source) =>
        source.Switch(
            ofLine: static line => IO.lift(() => Missing.Unless(NurbsCurve.CreateFromLine(line.Line), nameof(NurbsCurve.CreateFromLine))),
            ofArc: static arc => IO.lift(() =>
                Missing.Unless(arc.Structure.Case is (int degree, int cvCount) ? NurbsCurve.CreateFromArc(arc.Arc, degree, cvCount) : NurbsCurve.CreateFromArc(arc.Arc), nameof(NurbsCurve.CreateFromArc))),
            ofCircle: static circle => IO.lift(() =>
                Missing.Unless(circle.Structure.Case is (int degree, int cvCount) ? NurbsCurve.CreateFromCircle(circle.Circle, degree, cvCount) : NurbsCurve.CreateFromCircle(circle.Circle), nameof(NurbsCurve.CreateFromCircle))),
            ofEllipse: static ellipse =>
                from full in IO.lift(() => Missing.Unless(NurbsCurve.CreateFromEllipse(ellipse.Ellipse), nameof(NurbsCurve.CreateFromEllipse)))
                from trimmed in ellipse.AngleRadians.Match(
                    Some: angles => GeometryOps.OnFailure(
                        Disposal.Using(
                            static () => new ArcCurve(new Circle(1.0)),
                            circle => IO.lift(
                                from start in NurbsParameter(circle, angles.T0)
                                from end in NurbsParameter(circle, angles.T1)
                                from cut in Refused.Unless(full.TrimInterval(start, end), nameof(Curve.TrimInterval))
                                select full)),
                        IO.lift(full.Dispose)),
                    None: () => IO.pure(full))
                select trimmed);

    private static Fin<double> NurbsParameter(ArcCurve circle, double radians) =>
        Refused.Unless(circle.GetNurbsFormParameterFromCurveParameter(radians, out double parameter), parameter, nameof(Curve.GetNurbsFormParameterFromCurveParameter));

    public static IO<CatenaryResult> Catenary(Point3d start, Point3d end, Vector3d axis, CatenaryMethod method, bool smooth, int pointCount) =>
        IO.lift(() =>
            from enough in Limits.AtLeast(MinimumPoints).Check(pointCount, nameof(pointCount))
            from result in method.Switch(
                (Start: start, End: end, Axis: axis, Smooth: smooth, PointCount: enough),
                throughPoint: static (state, through) => Answered(
                    Curve.CreateCatenaryCurveThroughPoint(state.Start, state.End, state.Axis, through.Point, state.Smooth, state.PointCount, out Point3d apex, out double parameter, out double length, out double deviation),
                    apex, parameter, length, deviation, nameof(Curve.CreateCatenaryCurveThroughPoint)),
                fromLength: static (state, ofLength) => Answered(
                    Curve.CreateCatenaryCurveFromLength(state.Start, state.End, state.Axis, ofLength.Length, state.Smooth, state.PointCount, out Point3d apex, out double parameter, out double length, out double deviation),
                    apex, parameter, length, deviation, nameof(Curve.CreateCatenaryCurveFromLength)),
                fromParameter: static (state, ofParameter) => Answered(
                    Curve.CreateCatenaryCurveFromParameter(state.Start, state.End, state.Axis, ofParameter.Parameter, state.Smooth, state.PointCount, out Point3d apex, out double parameter, out double length, out double deviation),
                    apex, parameter, length, deviation, nameof(Curve.CreateCatenaryCurveFromParameter)),
                fromApex: static (state, ofApex) => Answered(
                    Curve.CreateCatenaryCurveFromApex(state.Start, state.End, state.Axis, ofApex.Apex, state.Smooth, state.PointCount, out Point3d apex, out double parameter, out double length, out double deviation),
                    apex, parameter, length, deviation, nameof(Curve.CreateCatenaryCurveFromApex)))
            select result);

    private static Fin<CatenaryResult> Answered(Curve? curve, Point3d apex, double parameter, double length, double deviation, string member) =>
        Missing.Unless(curve, member).Map(created => new CatenaryResult(created, apex, parameter, length, deviation));

    // --- [TEXT]
    private const int BoldStyle = 1;

    private const int ItalicStyle = 2;

    public static IO<Seq<Curve>> TextOutlines(string text, string font, double height, bool bold, bool italic, bool closeLoops, Plane plane, double smallCapsScale, double tolerance) =>
        from valid in IO.lift(() =>
            from filled in Invalid.Unless(text.Length > 0, nameof(text))
            from planar in Invalid.Unless(plane.IsValid, nameof(Plane.IsValid))
            select unit)
        from outlines in GeometryResults.Acquire(
            () => Curve.CreateTextOutlines(text, font, height, (bold ? BoldStyle : 0) | (italic ? ItalicStyle : 0), closeLoops, plane, smallCapsScale, tolerance),
            nameof(Curve.CreateTextOutlines))
        select outlines;

    // --- [JOINS]
    public static IO<CurveJoinResult> Join(Seq<Curve> curves, double tolerance, bool preserveDirection, bool simpleJoin) =>
        from filled in IO.lift(() => Invalid.Unless(!curves.IsEmpty, nameof(curves)))
        from answer in IO.lift(() => (Curves: Curve.JoinCurves(curves, tolerance, preserveDirection, simpleJoin, out int[] key), Key: key))
        from joined in GeometryResults.Acquire(() => answer.Curves, nameof(Curve.JoinCurves))
        select new CurveJoinResult(joined, toSeq(answer.Key));

    // --- [INTERSECTIONS]
    public static Fin<(double A, double B)> LineLine(Line lineA, Line lineB) =>
        Intersection.LineLine(lineA, lineB, out double a, out double b) ? (a, b) : new ParallelLines(nameof(Intersection.LineLine));

    public static Fin<LineCircleCrossing> LineCircle(Line line, Circle circle) =>
        Intersection.LineCircle(line, circle, out double t1, out Point3d point1, out double t2, out Point3d point2) switch {
            LineCircleIntersection.Single => new LineCircleCrossing.Single(t1, point1),
            LineCircleIntersection.Multiple => new LineCircleCrossing.Multiple(t1, point1, t2, point2),
            _ => new Missing(nameof(Intersection.LineCircle)),
        };

    // --- [BOOLEANS]
    public static IO<Seq<Curve>> Boolean(CurveBooleanMethod method, double tolerance) =>
        method.Switch(
            tolerance,
            union: static (within, union) =>
                from filled in IO.lift(() => Invalid.Unless(!union.Curves.IsEmpty, nameof(CurveBooleanMethod.Union.Curves)))
                from result in GeometryResults.Acquire(() => Curve.CreateBooleanUnion(union.Curves, within), nameof(Curve.CreateBooleanUnion))
                select result,
            intersection: static (within, both) => GeometryResults.Acquire(() => Curve.CreateBooleanIntersection(both.A, both.B, within), nameof(Curve.CreateBooleanIntersection)),
            difference: static (within, difference) =>
                from filled in IO.lift(() => Invalid.Unless(!difference.Subtractors.IsEmpty, nameof(CurveBooleanMethod.Difference.Subtractors)))
                from result in GeometryResults.Acquire(() => Curve.CreateBooleanDifference(difference.A, difference.Subtractors, within), nameof(Curve.CreateBooleanDifference))
                select result);

    public static IO<Seq<Seq<Curve>>> Regions(Seq<Curve> curves, Plane plane, Seq<Point3d> points, bool combine, double tolerance) =>
        Disposal.Using(
            IO.lift(() =>
                from filled in Invalid.Unless(!curves.IsEmpty, nameof(curves))
                from planar in Invalid.Unless(plane.IsValid, nameof(Plane.IsValid))
                from created in Missing.Unless(points.IsEmpty ? Curve.CreateBooleanRegions(curves, plane, combine, tolerance) : Curve.CreateBooleanRegions(curves, plane, points, combine, tolerance), nameof(Curve.CreateBooleanRegions))
                select created),
            static regions =>
                from answers in IO.lift(() => toSeq(Range(0, regions.RegionCount)).Map(index => Optional(regions.RegionCurves(index))).Strict())
                from complete in GeometryResults.Complete(answers.Map(static row => row.Map(toSeq)), static rows => Disposal.Release(rows.Flatten()), nameof(CurveBooleanRegions.RegionCurves))
                select complete);
}
