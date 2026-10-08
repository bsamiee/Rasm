using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TrimLength : System.Numerics.IMinMaxValue<TrimLength> {
    public static TrimLength MinValue { get; } = new(double.BitIncrement(0d));
    public static TrimLength MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union]
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
    public static IO<NurbsCurveFit> CreateNurbsCurveFit(Curve curve, Interval domain, NurbsCurveFitParameters rebuildOptions) =>
        from answer in IO.lift(() => (
            Curve: Curve.CreateNurbsCurveFit(curve, domain, rebuildOptions, out Line maximumSeparation, out double thisSeparationParameter, out double nurbsSeparationParameter),
            MaximumSeparation: maximumSeparation,
            Source: thisSeparationParameter,
            Fit: nurbsSeparationParameter))
        from fitted in Copies.Acquire(() => answer.Curve, nameof(Curve.CreateNurbsCurveFit))
        select new NurbsCurveFit(
            fitted,
            Conversions.Present(answer.MaximumSeparation),
            from source in Conversions.Present(answer.Source)
            from fit in Conversions.Present(answer.Fit)
            select (Source: source, Fit: fit));

    public static IO<(NurbsCurve Curve, Option<Line> MaximumDeviation)> RebuildToMatchTemplateCurve(Curve source, Curve templateCurve, bool flipSourceDirection, bool preserveEndTangents, bool makeSubDFriendly) =>
        from answer in IO.lift(() => (
            Curve: source.RebuildToMatchTemplateCurve(templateCurve, flipSourceDirection, preserveEndTangents, makeSubDFriendly, out Line maximumDeviation),
            MaximumDeviation: maximumDeviation))
        from rebuilt in Copies.Acquire(() => answer.Curve, nameof(Curve.RebuildToMatchTemplateCurve))
        select (Curve: rebuilt, MaximumDeviation: Conversions.Present(answer.MaximumDeviation));

    // --- [SPLIT]
    public static IO<Seq<Curve>> Split(Curve source, Seq<double> parameters) =>
        Copies.Acquire(
            () => Callbacks.Each(parameters, (parameter, _) => OutOfDomain.Unless(source.Domain, parameter, nameof(Curve.Split)))
                .Map(_ => source.Split(parameters)),
            nameof(Curve.Split));

    // --- [MATCHING]
    public static IO<Seq<NurbsCurve>> MakeCompatible(Seq<Curve> curves, Option<Point3d> startPt, Option<Point3d> endPt, CurveFit fit, Tolerances tolerances) =>
        Copies.AcquireNonEmpty(
            () => NurbsCurve.MakeCompatible(curves, Conversions.Unset(startPt), Conversions.Unset(endPt), (int)fit.RebuildType, fit.RebuildPointCount, fit.RefitTolerance, tolerances.Angle),
            nameof(NurbsCurve.MakeCompatible));

    public static IO<(Curve A, Curve B)> MakeEndsMeet(Curve curveA, bool adjustStartCurveA, Curve curveB, bool adjustStartCurveB) =>
        from copyA in Copies.Duplicate(curveA)
        from met in DisposalOps.OnFailure(
            Copies.Edit(
                curveB,
                copyB => Refused.Unless(Curve.MakeEndsMeet(copyA, adjustStartCurveA, copyB, adjustStartCurveB), copyA, nameof(Curve.MakeEndsMeet))
                    .Bind(static edited => Measurements.Valid(edited, nameof(Curve.MakeEndsMeet))),
                nameof(Curve.MakeEndsMeet)),
            IO.lift(copyA.Dispose))
        select (A: met.Result, B: met.Copy);
}
