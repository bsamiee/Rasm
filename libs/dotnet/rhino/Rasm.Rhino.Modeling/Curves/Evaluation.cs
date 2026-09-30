using Rasm.Rhino.Document;
using Rhino;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveAddress {
    public sealed record Parameter(double Value) : CurveAddress;

    public sealed record Length(double Value) : CurveAddress;

    public sealed record Normalized(double Value) : CurveAddress;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Division {
    public sealed record Count(int Segments) : Division;

    public sealed record Length(double Segment) : Division;

    public sealed record Chord(double Distance) : Division;
}

public sealed record DivideResult(Seq<double> Parameters, Seq<Point3d> Points);

public sealed record CurveStation(Point3d Point, Vector3d Tangent, Vector3d Curvature, Plane Frame, Plane PerpendicularFrame);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveEvaluation {
    // --- [LIMITS]
    private static readonly Limits<double> Fraction = Limits.AtLeast(0.0).AtMost(1.0);

    // --- [STATIONS]
    public static IO<double> Resolve(Curve curve, CurveAddress address, double fractionalTolerance) =>
        IO.lift(() => address.Switch(
            (Curve: curve, Tolerance: fractionalTolerance),
            parameter: static (state, at) =>
                from inside in OutOfDomain.Unless(state.Curve.Domain, at.Value, nameof(CurveAddress.Parameter))
                select at.Value,
            length: static (state, at) =>
                from length in Limits.AtLeast(0.0).Check(at.Value, nameof(CurveAddress.Length))
                from parameter in Refused.Unless(state.Curve.LengthParameter(length, out double resolved, state.Tolerance), resolved, nameof(Curve.LengthParameter))
                select parameter,
            normalized: static (state, at) =>
                from fraction in Fraction.Check(at.Value, nameof(CurveAddress.Normalized))
                from parameter in Refused.Unless(state.Curve.NormalizedLengthParameter(fraction, out double resolved, state.Tolerance), resolved, nameof(Curve.NormalizedLengthParameter))
                select parameter));

    public static IO<CurveStation> StationAt(Curve curve, double t) =>
        IO.lift(() =>
            from inside in OutOfDomain.Unless(curve.Domain, t, nameof(Curve.PointAt))
            let tangent = curve.TangentAt(t)
            let curvature = curve.CurvatureAt(t)
            from station in (
                    Degenerate.Unless(tangent, nameof(Curve.TangentAt)),
                    Degenerate.Unless(curvature.IsValid, nameof(Curve.CurvatureAt)),
                    Refused.Unless(curve.FrameAt(t, out Plane frame), frame, nameof(Curve.FrameAt)),
                    Refused.Unless(curve.PerpendicularFrameAt(t, out Plane perpendicularFrame), perpendicularFrame, nameof(Curve.PerpendicularFrameAt)))
                .Apply((_, _, framed, perpendicular) => new CurveStation(curve.PointAt(t), tangent, curvature, framed, perpendicular))
                .As()
            select station);

    public static IO<double> ArcLength(Curve curve, double t, double fractionalTolerance) =>
        IO.lift(() =>
            from inside in OutOfDomain.Unless(curve.Domain, t, nameof(Curve.GetLength))
            let length = curve.GetLength(fractionalTolerance, new Interval(curve.Domain.T0, t))
            from valid in Invalid.Unless(RhinoMath.IsValidDouble(length), nameof(Curve.GetLength))
            from measured in Refused.Unless((length > 0.0) || (t == curve.Domain.T0), nameof(Curve.GetLength))
            select length);

    public static IO<Seq<Vector3d>> Derivatives(Curve curve, double t, int order, CurveEvaluationSide side) =>
        IO.lift(() =>
            from nonNegative in (Limits.AtLeast(0).Check(order, nameof(order)), OutOfDomain.Unless(curve.Domain, t, nameof(Curve.DerivativeAt)))
                .Apply(static (valid, _) => valid)
                .As()
            from jet in Missing.Unless(curve.DerivativeAt(t, nonNegative, side), nameof(Curve.DerivativeAt))
            from complete in CountMismatch.Unless(nonNegative + 1, jet.Length, nameof(Curve.DerivativeAt))
            select toSeq(jet));

    public static IO<Seq<Plane>> PerpendicularFrames(Curve curve, Seq<double> parameters) =>
        IO.lift(() =>
                from frames in Missing.Unless(curve.GetPerpendicularFrames(parameters), nameof(Curve.GetPerpendicularFrames))
                from complete in CountMismatch.Unless(parameters.Count, frames.Length, nameof(Curve.GetPerpendicularFrames))
                select toSeq(frames))
            .Catch(static error => error.HasException<InvalidOperationException>(), static _ => IO.fail<Seq<Plane>>(new Invalid(nameof(parameters))));

    // --- [DIVISIONS]
    public static IO<DivideResult> Divide(Curve curve, Division division) =>
        IO.lift(() => division.Switch(
            curve,
            count: static (source, count) => Missing.Unless(source.DivideByCount(count.Segments, includeEnds: true, out Point3d[] points), nameof(Curve.DivideByCount))
                .Map(parameters => new DivideResult(toSeq(parameters), toSeq(points))),
            length: static (source, length) => Missing.Unless(source.DivideByLength(length.Segment, includeEnds: true, out Point3d[] points), nameof(Curve.DivideByLength))
                .Map(parameters => new DivideResult(toSeq(parameters), toSeq(points))),
            chord: static (source, chord) =>
                from distance in Limits.Above(0.0).Check(chord.Distance, nameof(Division.Chord.Distance))
                let answer = (Points: source.DivideEquidistant(distance, out double[] parameters), Parameters: parameters)
                from points in Missing.Unless(answer.Points, nameof(Curve.DivideEquidistant))
                select new DivideResult(toSeq(answer.Parameters), toSeq(points))));

    public static IO<Seq<Point3d>> DivideAsContour(Curve curve, ContourCut.Sweep sweep) =>
        IO.lift(() =>
            from points in Missing.Unless(curve.DivideAsContour(sweep.Start, sweep.End, sweep.Interval), nameof(Curve.DivideAsContour))
            from filled in Answers.NonEmpty(toSeq(points), nameof(Curve.DivideAsContour))
            select filled);

    // --- [PLANAR]
    public static IO<CurveOrientation> Orientation(Curve curve, Plane plane) =>
        IO.lift(() =>
            from planar in Invalid.Unless(plane.IsValid, nameof(plane))
            let orientation = curve.ClosedCurveOrientation(plane)
            from defined in Refused.Unless(orientation != CurveOrientation.Undefined, nameof(Curve.ClosedCurveOrientation))
            select orientation);

    public static IO<PointContainment> Contains(Curve curve, Point3d testPoint, Plane plane, double tolerance) =>
        IO.lift(() =>
            from valid in (Invalid.Unless(plane.IsValid, nameof(plane)), Invalid.Unless(testPoint.IsValid, nameof(testPoint))).Apply(static (_, _) => unit).As()
            let containment = curve.Contains(testPoint, plane, tolerance)
            from set in Refused.Unless(containment != PointContainment.Unset, nameof(Curve.Contains))
            select containment);
}
