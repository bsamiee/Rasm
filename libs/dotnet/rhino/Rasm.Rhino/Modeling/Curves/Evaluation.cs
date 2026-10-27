using System.Globalization;
using Rasm.RhinoCommon.Document.Units;

namespace Rasm.RhinoCommon.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class AscendingParameters {
    public Seq<double> Values { get; }

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref Seq<double> values) =>
        validationError = values.Count >= 2 && values.Zip(values.Tail).ForAll(static pair => pair.First < pair.Second)
            ? null
            : new RhinoFault($"parameters [{string.Join(", ", values.Map(static value => value.ToString(CultureInfo.InvariantCulture)))}]", RhinoReason.Outside);
}

public readonly record struct SegmentParameter(int Segment, double T);

[Union<double, double, double, double, SegmentParameter>(
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
    public static readonly CurveFrame Frenet = new(static (curve, t) => RhinoFault.Unless(curve.FrameAt(t, out Plane plane)).Map(_ => plane));
    public static readonly CurveFrame Perpendicular = new(static (curve, t) => RhinoFault.Unless(curve.PerpendicularFrameAt(t, out Plane plane)).Map(_ => plane));

    [UseDelegateFromConstructor]
    internal partial Fin<Plane> Read(Curve curve, double parameter);
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

    public sealed record NormalizedLengths(Seq<double> Lengths) : CurvePointSet;

    public sealed record InflectionPoints : CurvePointSet;

    public sealed record MaxCurvaturePoints : CurvePointSet;

    public sealed record ExtremeParameters(Vector3d Direction) : CurvePointSet;

    public sealed record Discontinuities(Continuity ContinuityType, double CosAngleTolerance, double CurvatureTolerance) : CurvePointSet;
}

public sealed record CurveProximity<T>(Point3d PointOnCurve, Point3d PointOnObject, T Geometry) where T : GeometryBase;

public readonly record struct CurveDeviation(double Distance, CurveSample OnA, CurveSample OnB);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveEvaluation {
    // --- [ADDRESSES]
    public static Fin<double> Resolve(Curve curve, CurveAddress at) =>
        at.Switch(
            curve,
            curveParameter: static (held, parameter) => Conversions.Within(held.Domain, parameter),
            lengthParameter: static (held, length) => RhinoFault.Unless(held.LengthParameter(length, out double t)).Map(_ => t),
            normalizedLengthParameter: static (held, length) => RhinoFault.Unless(held.NormalizedLengthParameter(length, out double t)).Map(_ => t),
            nurbsFormParameter: static (held, parameter) => RhinoFault.Unless(held.GetCurveParameterFromNurbsFormParameter(parameter, out double t)).Map(_ => t),
            segmentParameter: static (held, address) =>
                from segment in Copies.Within(held, ComponentIndexType.PolycurveSegment, Seq(address.Segment))
                let polycurve = (PolyCurve)held
                from local in Conversions.Within(polycurve.SegmentDomain(address.Segment), address.T)
                from t in Conversions.Found(Conversions.Present(polycurve.PolyCurveParameter(address.Segment, local)))
                select t);

    // --- [STATIONS]
    public static Fin<CurveStation> Station(Curve curve, CurveAddress at) =>
        from t in Resolve(curve, at)
        from tangent in Conversions.Found(Conversions.Present(curve.TangentAt(t)).Filter(static tangent => !tangent.IsTiny()))
        select new CurveStation(CurveSample.Of(curve, t), tangent, curve.CurvatureAt(t), curve.TorsionAt(t));

    public static Fin<Seq<Vector3d>> Derivatives(Curve curve, CurveAddress at, int count, CurveEvaluationSide side) =>
        from t in Resolve(curve, at)
        from jet in Conversions.Found(curve.DerivativeAt(t, count, side))
        select toSeq(jet);

    public static Fin<double> LengthAt(Curve curve, CurveAddress at) =>
        from t in Resolve(curve, at)
        let length = curve.GetLength(new Interval(curve.Domain.T0, t))
        from measured in RhinoFault.Unless(length > 0d || t == curve.Domain.T0).Map(_ => length)
        select measured;

    // --- [FRAMES]
    public static Fin<Plane> FrameAt(Curve curve, CurveAddress at, CurveFrame frame) =>
        from t in Resolve(curve, at)
        from plane in frame.Read(curve, t)
        select plane;

    public static Fin<Seq<Plane>> Frames(Curve curve, CurveFrames frames) =>
        frames.Switch(
            curve,
            perpendicular: static (rail, set) => Conversions.Found(rail.GetPerpendicularFrames(set.Parameters.Values)).Map(static planes => toSeq(planes)),
            frenet: static (rail, set) => Callbacks.Each(set.Parameters, (t, index) => RhinoFault.Unless(rail.FrameAt(t, out Plane plane), index).Map(_ => plane)),
            rail: static (rail, set) => Railed(rail, nurbs => nurbs.CreateRailFrames(set.Parameters)),
            planarRail: static (rail, set) => RhinoFault.Unless(!set.Normal.IsTiny()).Bind(_ => Railed(rail, nurbs => nurbs.CreatePlanarRailFrames(set.Parameters, set.Normal / set.Normal.Length))));

    private static Fin<Seq<Plane>> Railed(Curve curve, Func<NurbsCurve, Plane[]> read) {
        using NurbsCurve? converted = curve is NurbsCurve ? null : curve.ToNurbsCurve();
        return from nurbs in Conversions.Found(curve as NurbsCurve ?? converted)
               let planes = read(nurbs)
               from framed in RhinoFault.Unless(planes.Length > 0).Map(_ => toSeq(planes))
               select framed;
    }

    // --- [POINTS]
    public static Fin<Seq<CurveSample>> Points(Tolerances tolerances, Curve curve, CurvePointSet set) =>
        set.Switch(
            (Curve: curve, Tolerances: tolerances),
            divideByCount: static (at, rule) =>
                Conversions.Found(at.Curve.DivideByCount(rule.SegmentCount, rule.IncludeEnds, out Point3d[] points)).Map(parameters => CurveSample.Of(parameters, points)),
            divideByLength: static (at, rule) =>
                Conversions.Found(at.Curve.DivideByLength(rule.SegmentLength, rule.IncludeEnds, rule.Reverse, out Point3d[] points)).Map(parameters => CurveSample.Of(parameters, points)),
            divideEquidistant: static (at, rule) =>
                Conversions.Found(at.Curve.DivideEquidistant(rule.Distance, out double[] parameters)).Map(points => CurveSample.Of(parameters, points)),
            normalizedLengths: static (at, rule) =>
                Conversions.Found(at.Curve.NormalizedLengthParameters([.. rule.Lengths], at.Tolerances.Absolute, at.Tolerances.Relative))
                    .Map(parameters => toSeq(parameters).Map(t => CurveSample.Of(at.Curve, t)).Strict()),
            inflectionPoints: static (at, _) =>
                at.Curve.InflectionPoints(out double[] parameters) is { } points ? CurveSample.Of(parameters, points) : Seq<CurveSample>(),
            maxCurvaturePoints: static (at, _) =>
                at.Curve.MaxCurvaturePoints(out double[] parameters) is { } points ? CurveSample.Of(parameters, points) : Seq<CurveSample>(),
            extremeParameters: static (at, rule) =>
                toSeq(at.Curve.ExtremeParameters(rule.Direction)) switch {
                    var parameters => RhinoFault.Unless(!parameters.IsEmpty || at.Curve.IsPeriodic).Map(_ => parameters.Map(t => CurveSample.Of(at.Curve, t)).Strict()),
                },
            discontinuities: static (at, rule) =>
                toSeq(LanguageExt.List.unfold(at.Curve.Domain.T0, t0 =>
                        t0 < at.Curve.Domain.T1 ? Conversions.Found(at.Curve.GetNextDiscontinuity(rule.ContinuityType, t0, at.Curve.Domain.T1, rule.CosAngleTolerance, rule.CurvatureTolerance, out double t), (t, t)) : None))
                    .Map(t => CurveSample.Of(at.Curve, t))
                    .Strict());

    // --- [PROXIMITY]
    public static Fin<Option<CurveSample>> ClosestPoint(Curve curve, Point3d test, Option<double> within) =>
        RhinoFault.Unless(curve.ClosestPoint(test, out double t, Conversions.Unset(within))).Map(_ => Some(CurveSample.Of(curve, t)))
            .BindFail(error => within.IsSome ? Option<CurveSample>.None : error);

    public static Fin<Option<CurveProximity<T>>> ClosestPoints<T>(Curve curve, Seq<T> candidates, Option<double> within) where T : GeometryBase =>
        RhinoFault.Unless(curve.ClosestPoints(candidates, out Point3d onCurve, out Point3d onObject, out int index, Conversions.Unset(within)))
            .Map(_ => Some(new CurveProximity<T>(onCurve, onObject, candidates[index])))
            .BindFail(error => within.IsSome ? Option<CurveProximity<T>>.None : error);

    public static Fin<(CurveDeviation Maximum, CurveDeviation Minimum)> Deviation(Tolerances tolerances, Curve curveA, Curve curveB) =>
        RhinoFault.Unless(Curve.GetDistancesBetweenCurves(curveA, curveB, tolerances.Absolute, out double maximum, out double maximumA, out double maximumB, out double minimum, out double minimumA, out double minimumB))
            .Map(_ => (
                Maximum: new CurveDeviation(maximum, CurveSample.Of(curveA, maximumA), CurveSample.Of(curveB, maximumB)),
                Minimum: new CurveDeviation(minimum, CurveSample.Of(curveA, minimumA), CurveSample.Of(curveB, minimumB))));
}
