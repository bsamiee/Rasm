using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rhino;
using SuperLinq;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class AscendingParameters {
    public Seq<double> Values { get; }

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref Seq<double> values) =>
        validationError = values.Count >= 2 && values.Window(2).All(static pair => pair[0] < pair[1])
            ? null
            : new RhinoFault($"parameters [{string.Join(", ", values.Map(static value => value.ToString(CultureInfo.InvariantCulture)))}]", RhinoReason.Outside);
}

[SmartEnum]
public sealed partial class CurveMeasure {
    public static readonly CurveMeasure Parameter = new(static (curve, t) => Conversions.Within(curve.Domain, t));
    public static readonly CurveMeasure Length = new(static (curve, length) => RhinoFault.Unless(curve.LengthParameter(length, out double t), RhinoReason.Outside, length).Map(_ => t));
    public static readonly CurveMeasure NormalizedLength = new(static (curve, length) => RhinoFault.Unless(curve.NormalizedLengthParameter(length, out double t), RhinoReason.Outside, length).Map(_ => t));
    public static readonly CurveMeasure NurbsForm = new(static (curve, parameter) =>
        RhinoFault.Unless(curve.GetCurveParameterFromNurbsFormParameter(parameter, out double t), RhinoReason.Outside, parameter).Map(_ => t));

    [UseDelegateFromConstructor]
    internal partial Fin<double> Resolve(Curve curve, double value);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveAddress {
    public sealed record Along(CurveMeasure Measure, double Value) : CurveAddress;

    public sealed record OnSegment(int Segment, double T) : CurveAddress;
}

public sealed record CurveSample(double Parameter, Point3d Point) {
    public static CurveSample Of(Curve curve, double parameter) => new(parameter, curve.PointAt(parameter));
}

public sealed record CurveJet(CurveSample At, Vector3d Tangent, Vector3d Curvature, double Torsion, Seq<Vector3d> Derivatives);

public sealed record CurveDeviation(double Distance, CurveSample OnA, CurveSample OnB);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveEvaluation {
    // --- [ADDRESSES]
    public static Fin<double> Resolve(Curve curve, CurveAddress at) =>
        at.Switch(
            curve,
            along: static (held, address) => address.Measure.Resolve(held, address.Value),
            onSegment: static (held, address) =>
                from segment in held.Within(ComponentIndexType.PolycurveSegment, Seq(address.Segment))
                let polycurve = (PolyCurve)held
                from local in Conversions.Within(polycurve.SegmentDomain(address.Segment), address.T)
                from t in Conversions.Required(Conversions.Present(polycurve.PolyCurveParameter(address.Segment, local)))
                select t);

    // --- [JETS]
    public static Fin<CurveJet> Jet(Curve curve, CurveAddress at, int order, CurveEvaluationSide side) =>
        from t in Resolve(curve, at)
        from tangent in Conversions.Required(Conversions.Present(curve.TangentAt(t)).Filter(static tangent => !tangent.IsTiny()))
        from derivatives in Conversions.Required(curve.DerivativeAt(t, order, side))
        select new CurveJet(CurveSample.Of(curve, t), tangent, curve.CurvatureAt(t), curve.TorsionAt(t), toSeq(derivatives).Tail.Strict());

    public static Fin<double> LengthAt(Curve curve, CurveAddress at) =>
        from t in Resolve(curve, at)
        let length = curve.GetLength(new Interval(curve.Domain.T0, t))
        from measured in RhinoFault.Unless(length > 0d || t == curve.Domain.T0, RhinoReason.Declined, t).Map(_ => length)
        select measured;

    // --- [SCANS]
    internal static Seq<double> Kinks(Tolerances tolerances, Curve curve, Continuity kind) =>
        Breaks(tolerances, kind, curve.IsClosed, curve.Domain, (read, t0, t1, cosAngleTolerance, curvatureTolerance) =>
            Conversions.Present(curve.GetNextDiscontinuity(read, t0, t1, cosAngleTolerance, curvatureTolerance, out double t), t));

    internal static Seq<double> Breaks(Tolerances tolerances, Continuity kind, bool closed, Interval domain, Func<Continuity, double, double, double, double, Option<double>> next) =>
        (closed ? kind switch {
            Continuity.C0_continuous => Continuity.C0_locus_continuous,
            Continuity.C1_continuous => Continuity.C1_locus_continuous,
            Continuity.C2_continuous => Continuity.C2_locus_continuous,
            Continuity.G1_continuous => Continuity.G1_locus_continuous,
            Continuity.G2_continuous => Continuity.G2_locus_continuous,
            _ => kind,
        } : kind) switch {
            var read => toSeq(LanguageExt.List.unfold(domain.T0, t0 =>
                t0 < domain.T1 ? next(read, t0, domain.T1, Math.Cos(tolerances.Angle), RhinoMath.SqrtEpsilon).Map(static t => (t, t)) : None)).Strict(),
        };

    // --- [DEVIATION]
    public static Fin<(CurveDeviation Maximum, CurveDeviation Minimum)> Deviation(Tolerances tolerances, Curve curveA, Curve curveB) =>
        RhinoFault.Unless(
                Curve.GetDistancesBetweenCurves(curveA, curveB, tolerances.Absolute, out double maximum, out double maximumA, out double maximumB, out double minimum, out double minimumA, out double minimumB),
                RhinoReason.Declined, tolerances.Absolute)
            .Map(_ => (
                Maximum: new CurveDeviation(maximum, CurveSample.Of(curveA, maximumA), CurveSample.Of(curveB, maximumB)),
                Minimum: new CurveDeviation(minimum, CurveSample.Of(curveA, minimumA), CurveSample.Of(curveB, minimumB))));
}
