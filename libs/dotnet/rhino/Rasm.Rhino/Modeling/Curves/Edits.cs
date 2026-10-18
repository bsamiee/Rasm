using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TrimLength : System.Numerics.IMinMaxValue<TrimLength> {
    public static TrimLength MinValue { get; } = new(double.BitIncrement(0d));
    public static TrimLength MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SmoothFrame {
    public sealed record World : SmoothFrame;

    public sealed record ObjectUvn : SmoothFrame;

    public sealed record CPlane(Plane Plane) : SmoothFrame;
}

public sealed record SmoothOptions(double Factor, bool X, bool Y, bool Z, bool FixBoundaries, SmoothFrame Frame) {
    public (SmoothingCoordinateSystem System, Plane Plane) Coordinates =>
        Frame.Switch(
            world: static _ => (SmoothingCoordinateSystem.World, Plane.WorldXY),
            objectUvn: static _ => (SmoothingCoordinateSystem.Object, Plane.WorldXY),
            cPlane: static frame => (SmoothingCoordinateSystem.CPlane, frame.Plane));
}

public sealed record NurbsCurveFit(NurbsCurve Curve, Option<Line> MaximumSeparation, Option<(double Source, double Fit)> SeparationParameters);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveEdits {
    // --- [FITS]
    public static IO<NurbsCurveFit> CreateNurbsCurveFit(Curve curve, Interval domain, NurbsCurveFitParameters rebuildOptions, Action<IDisposable> retain) =>
        Copies.Owned(
            () => (
                Curve: Curve.CreateNurbsCurveFit(curve, domain, rebuildOptions, out Line maximumSeparation, out double source, out double fit),
                MaximumSeparation: maximumSeparation, Source: source, Fit: fit),
            static answer => Measurements.Valid(answer.Curve, nameof(Curve.CreateNurbsCurveFit))
                .Map(fitted => new NurbsCurveFit(fitted, Conversions.Present(answer.MaximumSeparation),
                    from source in Conversions.Present(answer.Source)
                    from fit in Conversions.Present(answer.Fit)
                    select (Source: source, Fit: fit))),
            static answer => DisposalOps.Release(answer.Curve), retain);

    public static IO<(NurbsCurve Curve, Option<Line> MaximumDeviation)> RebuildToMatchTemplateCurve(Curve source, Curve templateCurve, bool flipSourceDirection, bool preserveEndTangents, bool makeSubDFriendly, Action<IDisposable> retain) =>
        Copies.Owned(
            () => (
                Curve: source.RebuildToMatchTemplateCurve(templateCurve, flipSourceDirection, preserveEndTangents, makeSubDFriendly, out Line maximumDeviation),
                MaximumDeviation: maximumDeviation),
            static answer => Measurements.Valid(answer.Curve, nameof(Curve.RebuildToMatchTemplateCurve))
                .Map(rebuilt => (Curve: rebuilt, MaximumDeviation: Conversions.Present(answer.MaximumDeviation))),
            static answer => DisposalOps.Release(answer.Curve), retain);

    // --- [SPLIT]
    public static IO<Seq<Curve>> Split(Curve source, Seq<double> parameters, Action<IDisposable> retain) =>
        Copies.Acquire(
            () => Callbacks.Each(parameters, (parameter, _) => Conversions.Within(source.Domain, parameter, nameof(Curve.Split)))
                .Map(_ => source.Split(parameters)),
            nameof(Curve.Split), retain);

    // --- [MATCHING]
    public static IO<Seq<NurbsCurve>> MakeCompatible(Seq<Curve> curves, Option<Point3d> startPt, Option<Point3d> endPt, CurveFit fit, Tolerances tolerances, Action<IDisposable> retain) =>
        fit.Parameters switch {
            var fitting => Copies.AcquireNonEmpty(
                () => NurbsCurve.MakeCompatible(curves, Conversions.Unset(startPt), Conversions.Unset(endPt), (int)fitting.RebuildType, fitting.RebuildPointCount, fitting.RefitTolerance, tolerances.Angle),
                nameof(NurbsCurve.MakeCompatible), retain),
        };

    public static IO<(Curve A, Curve B)> MakeEndsMeet(Curve curveA, bool adjustStartCurveA, Curve curveB, bool adjustStartCurveB, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from copyA in Copies.Duplicate(curveA, owned)
            from copyB in Copies.Duplicate(curveB, owned)
            from met in IO.lift(() => Refused.Unless(Curve.MakeEndsMeet(copyA, adjustStartCurveA, copyB, adjustStartCurveB), nameof(Curve.MakeEndsMeet)))
            from valid in IO.lift(() => (Measurements.Valid(copyA, nameof(Curve.MakeEndsMeet)), Measurements.Valid(copyB, nameof(Curve.MakeEndsMeet)))
                .Apply(static (a, b) => (A: a, B: b)).As())
            select valid, retain);
}
