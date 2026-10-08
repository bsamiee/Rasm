using Rasm.Rhino.Document.Notation;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Start", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ArcLength : System.Numerics.IMinMaxValue<ArcLength> {
    public static ArcLength MinValue { get; } = new(0d);
    public static ArcLength MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Start", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct NormalizedLength : System.Numerics.IMinMaxValue<NormalizedLength> {
    public static NormalizedLength MinValue { get; } = new(0d);
    public static NormalizedLength MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct DerivativeCount : System.Numerics.IMinMaxValue<DerivativeCount> {
    public static DerivativeCount MinValue { get; } = new(0);
    public static DerivativeCount MaxValue { get; } = new(int.MaxValue - 1);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SearchDistance : System.Numerics.IMinMaxValue<SearchDistance> {
    public static SearchDistance MinValue { get; } = new(double.BitIncrement(0d));
    public static SearchDistance MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union<double, ArcLength, NormalizedLength>(T1Name = "CurveParameter", T2Name = "LengthParameter", T3Name = "NormalizedLengthParameter", MapMethods = SwitchMapMethodsGeneration.None)]
public readonly partial struct CurveAddress;

public readonly record struct CurveSample(double Parameter, Point3d Point) {
    public static CurveSample Of(Curve curve, double parameter) => new(parameter, curve.PointAt(parameter));

    public static Seq<CurveSample> Of(double[] parameters, Point3d[] points) =>
        toSeq(parameters).Zip(toSeq(points), static (parameter, point) => new CurveSample(parameter, point)).Strict();
}

public readonly record struct CurveStation(CurveSample At, Vector3d Tangent, Vector3d Curvature);

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurvePointSet {
    public sealed record DivideByCount(int SegmentCount, bool IncludeEnds) : CurvePointSet;

    public sealed record DivideByLength(double SegmentLength, bool IncludeEnds, bool Reverse) : CurvePointSet;

    public sealed record DivideEquidistant(double Distance) : CurvePointSet;

    public sealed record InflectionPoints : CurvePointSet;

    public sealed record MaxCurvaturePoints : CurvePointSet;

    public sealed record ExtremeParameters(Vector3d Direction) : CurvePointSet;

    public sealed record Discontinuities(Continuity ContinuityType, double CosAngleTolerance, double CurvatureTolerance) : CurvePointSet;
}

public readonly record struct Proximity<T>(Point3d PointOnCurve, Point3d PointOnObject, T Geometry) where T : GeometryBase;

public readonly record struct CurveDeviation(double Distance, CurveSample OnA, CurveSample OnB);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveEvaluation {
    // --- [ADDRESSES]
    public static Fin<double> Resolve(Curve curve, CurveAddress at, string member) =>
        at.Switch(
            (Curve: curve, Member: member),
            curveParameter: static (state, address) => OutOfDomain.Unless(state.Curve.Domain, address, state.Member),
            lengthParameter: static (state, address) => Refused.Unless(state.Curve.LengthParameter(address, out double t), t, nameof(Curve.LengthParameter)),
            normalizedLengthParameter: static (state, address) => Refused.Unless(state.Curve.NormalizedLengthParameter(address, out double t), t, nameof(Curve.NormalizedLengthParameter)));

    // --- [STATIONS]
    public static Fin<CurveStation> Station(Curve curve, CurveAddress at) =>
        from t in Resolve(curve, at, nameof(Curve.PointAt))
        let read = curve.TangentAt(t)
        from tangent in InvalidAnswer.Unless(read.IsValid && !read.IsTiny(), read, nameof(Curve.TangentAt))
        select new CurveStation(CurveSample.Of(curve, t), tangent, curve.CurvatureAt(t));

    public static Fin<Seq<Vector3d>> Derivatives(Curve curve, CurveAddress at, DerivativeCount count, CurveEvaluationSide side) =>
        from t in Resolve(curve, at, nameof(Curve.DerivativeAt))
        from jet in Missing.Unless(curve.DerivativeAt(t, count, side), nameof(Curve.DerivativeAt))
        select toSeq(jet);

    public static Fin<double> LengthAt(Curve curve, CurveAddress at) =>
        from t in Resolve(curve, at, nameof(Curve.GetLength))
        let length = curve.GetLength(new Interval(curve.Domain.T0, t))
        from measured in Refused.Unless(length > 0d || t == curve.Domain.T0, length, nameof(Curve.GetLength))
        select measured;

    // --- [FRAMES]
    public static Fin<Plane> FrameAt(Curve curve, CurveAddress at, bool zeroTwisting) =>
        from t in Resolve(curve, at, zeroTwisting ? nameof(Curve.PerpendicularFrameAt) : nameof(Curve.FrameAt))
        from frame in zeroTwisting
            ? Refused.Unless(curve.PerpendicularFrameAt(t, out Plane perpendicular), perpendicular, nameof(Curve.PerpendicularFrameAt))
            : Refused.Unless(curve.FrameAt(t, out Plane plane), plane, nameof(Curve.FrameAt))
        select frame;

    public static IO<Seq<Plane>> PerpendicularFrames(Curve curve, Seq<double> parameters) =>
        from frames in IO.lift(() => Missing.Unless(curve.GetPerpendicularFrames(parameters), nameof(Curve.GetPerpendicularFrames)))
            .Catch(
                static error => error.HasException<InvalidOperationException>(),
                static _ => IO.fail<Plane[]>(new UnorderedParameters()))
        from complete in IO.lift(CountMismatch.Unless(parameters.Count, frames.Length, nameof(Curve.GetPerpendicularFrames)))
        select toSeq(frames);

    // --- [POINTS]
    public static Fin<Seq<CurveSample>> Points(Curve curve, CurvePointSet set) =>
        set.Switch(
            curve,
            divideByCount: static (source, rule) =>
                Missing.Unless(source.DivideByCount(rule.SegmentCount, rule.IncludeEnds, out Point3d[] points), nameof(Curve.DivideByCount)).Map(parameters => CurveSample.Of(parameters, points)),
            divideByLength: static (source, rule) =>
                Missing.Unless(source.DivideByLength(rule.SegmentLength, rule.IncludeEnds, rule.Reverse, out Point3d[] points), nameof(Curve.DivideByLength)).Map(parameters => CurveSample.Of(parameters, points)),
            divideEquidistant: static (source, rule) =>
                Missing.Unless(source.DivideEquidistant(rule.Distance, out double[] parameters), nameof(Curve.DivideEquidistant)).Map(points => CurveSample.Of(parameters, points)),
            inflectionPoints: static (source, _) =>
                Missing.Unless(source.InflectionPoints(out double[] parameters), nameof(Curve.InflectionPoints)).Map(points => CurveSample.Of(parameters, points)),
            maxCurvaturePoints: static (source, _) =>
                Missing.Unless(source.MaxCurvaturePoints(out double[] parameters), nameof(Curve.MaxCurvaturePoints)).Map(points => CurveSample.Of(parameters, points)),
            extremeParameters: static (source, rule) =>
                toSeq(source.ExtremeParameters(rule.Direction)) switch {
                    var parameters => Refused.Unless(!parameters.IsEmpty || source.IsPeriodic, parameters.Map(t => CurveSample.Of(source, t)).Strict(), nameof(Curve.ExtremeParameters)),
                },
            discontinuities: static (source, rule) =>
                Fin.Succ(toSeq(LanguageExt.List.unfold(source.Domain.T0, t0 =>
                        t0 < source.Domain.T1 ? Callbacks.Found(source.GetNextDiscontinuity(rule.ContinuityType, t0, source.Domain.T1, rule.CosAngleTolerance, rule.CurvatureTolerance, out double t), (t, t)) : None))
                    .Map(t => CurveSample.Of(source, t))
                    .Strict()));

    // --- [PROXIMITY]
    public static Fin<CurveSample> ClosestPoint(Curve curve, Point3d test) =>
        Refused.Unless(curve.ClosestPoint(test, out double t), t, nameof(Curve.ClosestPoint)).Map(parameter => CurveSample.Of(curve, parameter));

    public static Option<CurveSample> ClosestPoint(Curve curve, Point3d test, SearchDistance within) =>
        Callbacks.Found(curve.ClosestPoint(test, out double t, within), t).Map(parameter => CurveSample.Of(curve, parameter));

    public static Fin<Proximity<T>> ClosestPoints<T>(Curve curve, Seq<T> candidates) where T : GeometryBase =>
        Refused.Unless(curve.ClosestPoints(candidates, out Point3d onCurve, out Point3d onObject, out int index), index, nameof(Curve.ClosestPoints))
            .Map(found => new Proximity<T>(onCurve, onObject, candidates[found]));

    public static Option<Proximity<T>> ClosestPoints<T>(Curve curve, Seq<T> candidates, SearchDistance within) where T : GeometryBase =>
        Callbacks.Found(curve.ClosestPoints(candidates, out Point3d onCurve, out Point3d onObject, out int index, within), index)
            .Map(found => new Proximity<T>(onCurve, onObject, candidates[found]));

    public static Fin<(CurveDeviation Maximum, CurveDeviation Minimum)> Deviation(Curve curveA, Curve curveB, Tolerances tolerances) =>
        Refused.Unless(
                Curve.GetDistancesBetweenCurves(curveA, curveB, tolerances.Absolute, out double maximum, out double maximumA, out double maximumB, out double minimum, out double minimumA, out double minimumB),
                nameof(Curve.GetDistancesBetweenCurves))
            .Map(_ => (
                Maximum: new CurveDeviation(maximum, CurveSample.Of(curveA, maximumA), CurveSample.Of(curveB, maximumB)),
                Minimum: new CurveDeviation(minimum, CurveSample.Of(curveA, minimumA), CurveSample.Of(curveB, minimumB))));
}
