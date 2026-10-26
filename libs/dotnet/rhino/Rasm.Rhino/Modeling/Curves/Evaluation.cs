using System.Globalization;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Start", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ArcLength : System.Numerics.IMinMaxValue<ArcLength> {
    public static ArcLength MinValue { get; } = new(0d);
    public static ArcLength MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Start", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct NormalizedLength : System.Numerics.IMinMaxValue<NormalizedLength> {
    public static NormalizedLength MinValue { get; } = new(0d);
    public static NormalizedLength MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PositiveLength : System.Numerics.IMinMaxValue<PositiveLength> {
    public static PositiveLength MinValue { get; } = new(double.BitIncrement(0d));
    public static PositiveLength MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DerivativeCount : System.Numerics.IMinMaxValue<DerivativeCount> {
    public static DerivativeCount MinValue { get; } = new(0);
    public static DerivativeCount MaxValue { get; } = new(int.MaxValue - 1);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
public sealed partial class AscendingParameters {
    public Seq<double> Values { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<double> values) =>
        validationError = values.Count >= 2 && values.Zip(values.Tail).ForAll(static pair => pair.First < pair.Second)
            ? null
            : new ValidationError($"parameters [{string.Join(", ", values.Map(static value => value.ToString(CultureInfo.InvariantCulture)))}] are no strictly increasing set of at least 2");
}

public readonly record struct SegmentParameter(int Segment, double T);

[Union<double, ArcLength, NormalizedLength, double, SegmentParameter>(
    T1Name = "CurveParameter", T2Name = "LengthParameter", T3Name = "NormalizedLengthParameter", T4Name = "NurbsFormParameter", T5Name = "SegmentParameter",
    MapMethods = SwitchMapMethodsGeneration.None)]
public readonly partial struct CurveAddress;

public readonly record struct CurveSample(double Parameter, Point3d Point) {
    public static CurveSample Of(Curve curve, double parameter) => new(parameter, curve.PointAt(parameter));

    public static Seq<CurveSample> Of(double[] parameters, Point3d[] points) =>
        toSeq(parameters).Zip(toSeq(points), static (parameter, point) => new CurveSample(parameter, point)).Strict();
}

public readonly record struct CurveStation(CurveSample At, Vector3d Tangent, Vector3d Curvature, double Torsion);

[SmartEnum]
public sealed partial class CurveFrame {
    public static readonly CurveFrame Frenet = new(nameof(Curve.FrameAt), static (curve, t) => Callbacks.Found(curve.FrameAt(t, out Plane plane), plane));
    public static readonly CurveFrame Perpendicular = new(nameof(Curve.PerpendicularFrameAt), static (curve, t) => Callbacks.Found(curve.PerpendicularFrameAt(t, out Plane plane), plane));

    internal string Member { get; }

    [UseDelegateFromConstructor]
    internal partial Option<Plane> Read(Curve curve, double parameter);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveFrames {
    public sealed record Perpendicular(AscendingParameters Parameters) : CurveFrames;

    public sealed record Frenet(Seq<double> Parameters) : CurveFrames;

    public sealed record Rail(Seq<double> Parameters) : CurveFrames;

    public sealed record PlanarRail(Seq<double> Parameters, Vector3d Normal) : CurveFrames;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurvePointSet {
    public sealed record DivideByCount(int SegmentCount, bool IncludeEnds) : CurvePointSet;

    public sealed record DivideByLength(double SegmentLength, bool IncludeEnds, bool Reverse) : CurvePointSet;

    public sealed record DivideEquidistant(double Distance) : CurvePointSet;

    public sealed record NormalizedLengths(Seq<NormalizedLength> Lengths) : CurvePointSet;

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
            curveParameter: static (state, address) => Conversions.Within(state.Curve.Domain, address, state.Member),
            lengthParameter: static (state, address) => Refused.Unless(state.Curve.LengthParameter(address, out double t), t, nameof(Curve.LengthParameter)),
            normalizedLengthParameter: static (state, address) => Refused.Unless(state.Curve.NormalizedLengthParameter(address, out double t), t, nameof(Curve.NormalizedLengthParameter)),
            nurbsFormParameter: static (state, address) =>
                Refused.Unless(state.Curve.GetCurveParameterFromNurbsFormParameter(address, out double t), t, nameof(Curve.GetCurveParameterFromNurbsFormParameter)),
            segmentParameter: static (state, address) =>
                from segment in Copies.Within(state.Curve, ComponentIndexType.PolycurveSegment, Seq(address.Segment))
                let polycurve = (PolyCurve)state.Curve
                from local in Conversions.Within(polycurve.SegmentDomain(address.Segment), address.T, nameof(PolyCurve.PolyCurveParameter))
                from t in Conversions.Present(polycurve.PolyCurveParameter(address.Segment, local)).ToFin(new Refused(nameof(PolyCurve.PolyCurveParameter), Refused.Declined))
                select t);

    // --- [STATIONS]
    public static Fin<CurveStation> Station(Curve curve, CurveAddress at) =>
        from t in Resolve(curve, at, nameof(Curve.PointAt))
        let read = curve.TangentAt(t)
        from tangent in Refused.UnlessValid(read.IsValid && !read.IsTiny(), read, nameof(Curve.TangentAt))
        select new CurveStation(CurveSample.Of(curve, t), tangent, curve.CurvatureAt(t), curve.TorsionAt(t));

    public static Fin<Seq<Vector3d>> Derivatives(Curve curve, CurveAddress at, DerivativeCount count, CurveEvaluationSide side) =>
        from t in Resolve(curve, at, nameof(Curve.DerivativeAt))
        from jet in Refused.UnlessFound(curve.DerivativeAt(t, count, side), nameof(Curve.DerivativeAt))
        select toSeq(jet);

    public static Fin<double> LengthAt(Curve curve, CurveAddress at) =>
        from t in Resolve(curve, at, nameof(Curve.GetLength))
        let length = curve.GetLength(new Interval(curve.Domain.T0, t))
        from measured in Refused.Unless(length > 0d || t == curve.Domain.T0, length, nameof(Curve.GetLength))
        select measured;

    // --- [FRAMES]
    public static Fin<Plane> FrameAt(Curve curve, CurveAddress at, CurveFrame frame) =>
        from t in Resolve(curve, at, frame.Member)
        from plane in frame.Read(curve, t).ToFin(new Refused(frame.Member, Refused.Declined))
        select plane;

    public static Fin<Seq<Plane>> Frames(Curve curve, CurveFrames frames) =>
        frames.Switch(
            curve,
            perpendicular: static (rail, set) =>
                Refused.UnlessFound(rail.GetPerpendicularFrames(set.Parameters.Values), nameof(Curve.GetPerpendicularFrames)).Map(static planes => toSeq(planes)),
            frenet: static (rail, set) =>
                Callbacks.Each(set.Parameters, (t, index) => Refused.Unless(rail.FrameAt(t, out Plane plane), plane, nameof(Curve.FrameAt), index)),
            rail: static (rail, set) =>
                Railed(rail, nurbs => nurbs.CreateRailFrames(set.Parameters), nameof(NurbsCurve.CreateRailFrames)),
            planarRail: static (rail, set) => set.Normal.IsTiny()
                ? Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{nameof(NurbsCurve.CreatePlanarRailFrames)} needs a nonzero normal, received {set.Normal}"))
                : Railed(rail, nurbs => nurbs.CreatePlanarRailFrames(set.Parameters, set.Normal / set.Normal.Length), nameof(NurbsCurve.CreatePlanarRailFrames)));

    private static Fin<Seq<Plane>> Railed(Curve curve, Func<NurbsCurve, Plane[]> read, string member) {
        using NurbsCurve? converted = curve is NurbsCurve ? null : curve.ToNurbsCurve();
        return from nurbs in Refused.UnlessFound(curve as NurbsCurve ?? converted, nameof(Curve.ToNurbsCurve))
               let planes = read(nurbs)
               from framed in Refused.Unless(planes.Length > 0, toSeq(planes), member)
               select framed;
    }

    // --- [POINTS]
    public static Fin<Seq<CurveSample>> Points(Tolerances tolerances, Curve curve, CurvePointSet set) =>
        set.Switch(
            (Curve: curve, Tolerances: tolerances),
            divideByCount: static (at, rule) =>
                Refused.UnlessFound(at.Curve.DivideByCount(rule.SegmentCount, rule.IncludeEnds, out Point3d[] points), nameof(Curve.DivideByCount)).Map(parameters => CurveSample.Of(parameters, points)),
            divideByLength: static (at, rule) =>
                Refused.UnlessFound(at.Curve.DivideByLength(rule.SegmentLength, rule.IncludeEnds, rule.Reverse, out Point3d[] points), nameof(Curve.DivideByLength)).Map(parameters => CurveSample.Of(parameters, points)),
            divideEquidistant: static (at, rule) =>
                Refused.UnlessFound(at.Curve.DivideEquidistant(rule.Distance, out double[] parameters), nameof(Curve.DivideEquidistant)).Map(points => CurveSample.Of(parameters, points)),
            normalizedLengths: static (at, rule) =>
                Refused.UnlessFound(
                        at.Curve.NormalizedLengthParameters([.. rule.Lengths.Map(static length => (double)length)], at.Tolerances.Absolute, at.Tolerances.Relative),
                        nameof(Curve.NormalizedLengthParameters))
                    .Map(parameters => toSeq(parameters).Map(t => CurveSample.Of(at.Curve, t)).Strict()),
            inflectionPoints: static (at, _) =>
                at.Curve.InflectionPoints(out double[] parameters) is { } points ? CurveSample.Of(parameters, points) : Seq<CurveSample>(),
            maxCurvaturePoints: static (at, _) =>
                at.Curve.MaxCurvaturePoints(out double[] parameters) is { } points ? CurveSample.Of(parameters, points) : Seq<CurveSample>(),
            extremeParameters: static (at, rule) =>
                toSeq(at.Curve.ExtremeParameters(rule.Direction)) switch {
                    var parameters => Refused.Unless(!parameters.IsEmpty || at.Curve.IsPeriodic, parameters.Map(t => CurveSample.Of(at.Curve, t)).Strict(), nameof(Curve.ExtremeParameters)),
                },
            discontinuities: static (at, rule) =>
                toSeq(LanguageExt.List.unfold(at.Curve.Domain.T0, t0 =>
                        t0 < at.Curve.Domain.T1 ? Callbacks.Found(at.Curve.GetNextDiscontinuity(rule.ContinuityType, t0, at.Curve.Domain.T1, rule.CosAngleTolerance, rule.CurvatureTolerance, out double t), (t, t)) : None))
                    .Map(t => CurveSample.Of(at.Curve, t))
                    .Strict());

    // --- [PROXIMITY]
    public static Fin<Option<CurveSample>> ClosestPoint(Curve curve, Point3d test, Option<SearchDistance> within) =>
        curve.ClosestPoint(test, out double t, Conversions.Unset(within.Map(static distance => (double)distance)))
            ? Some(CurveSample.Of(curve, t))
            : within.IsSome ? Option<CurveSample>.None : new Refused(nameof(Curve.ClosestPoint), Refused.Declined);

    public static Fin<Option<Proximity<T>>> ClosestPoints<T>(Curve curve, Seq<T> candidates, Option<SearchDistance> within) where T : GeometryBase =>
        curve.ClosestPoints(candidates, out Point3d onCurve, out Point3d onObject, out int index, Conversions.Unset(within.Map(static distance => (double)distance)))
            ? Some(new Proximity<T>(onCurve, onObject, candidates[index]))
            : within.IsSome ? Option<Proximity<T>>.None : new Refused(nameof(Curve.ClosestPoints), Refused.Declined);

    public static Fin<(CurveDeviation Maximum, CurveDeviation Minimum)> Deviation(Tolerances tolerances, Curve curveA, Curve curveB) =>
        Refused.Unless(
                Curve.GetDistancesBetweenCurves(curveA, curveB, tolerances.Absolute, out double maximum, out double maximumA, out double maximumB, out double minimum, out double minimumA, out double minimumB),
                nameof(Curve.GetDistancesBetweenCurves))
            .Map(_ => (
                Maximum: new CurveDeviation(maximum, CurveSample.Of(curveA, maximumA), CurveSample.Of(curveB, maximumB)),
                Minimum: new CurveDeviation(minimum, CurveSample.Of(curveA, minimumA), CurveSample.Of(curveB, minimumB))));
}
