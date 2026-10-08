using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Grouped<T>(T Value, Seq<int> Inputs);

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct CatenaryPointCount : System.Numerics.IMinMaxValue<CatenaryPointCount> {
    public static CatenaryPointCount MinValue { get; } = new(4);
    public static CatenaryPointCount MaxValue { get; } = new(10000);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record CatenaryForm {
    public sealed record ThroughPoint(Point3d Point) : CatenaryForm;

    public sealed record FromLength(double Length) : CatenaryForm;

    public sealed record FromParameter(double Parameter) : CatenaryForm;

    public sealed record FromApex(Point3d Apex) : CatenaryForm;
}

public sealed record CatenaryCurve(Curve Curve, Point3d Apex, double Parameter, double Length, double MaxDeviation);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveConstruction {
    // --- [JOINS_AND_BOOLEANS]
    public static IO<Seq<Grouped<Curve>>> JoinCurves(Seq<Curve> curves, Tolerances tolerances, bool preserveDirection, bool simpleJoin) =>
        Copies.Owned(
            IO.lift(() => (Curves: Curve.JoinCurves(curves, tolerances.Absolute, preserveDirection, simpleJoin, out int[] key), Key: key)),
            static answer => Measurements.Valid(toSeq<Curve?>(answer.Curves), nameof(Curve.JoinCurves)).Map(joined => Group(joined, toSeq(answer.Key))),
            static answer => DisposalOps.Release(Conversions.Rows(answer.Curves)));

    public static IO<Seq<Grouped<Curve>>> CreateBooleanUnion(Seq<Curve> curves, Tolerances tolerances) =>
        Copies.Owned(
            IO.lift(() => (Curves: Curve.CreateBooleanUnion(curves, tolerances.Absolute, out int[] indexMap), IndexMap: indexMap)),
            static answer => Measurements.Valid(toSeq<Curve?>(answer.Curves), nameof(Curve.CreateBooleanUnion))
                .Bind(static united => Conversions.NonEmpty(united, nameof(Curve.CreateBooleanUnion)))
                .Map(united => Group(united, toSeq(answer.IndexMap))),
            static answer => DisposalOps.Release(Conversions.Rows(answer.Curves)));

    public static IO<Seq<Grouped<Seq<Curve>>>> CreateBooleanRegions(Seq<Curve> curves, Plane plane, Seq<Point3d> points, bool combineRegions, Tolerances tolerances) =>
        (from regions in use(IO.lift(() => Missing.Unless(Curve.CreateBooleanRegions(curves, plane, points, combineRegions, tolerances.Absolute), nameof(Curve.CreateBooleanRegions))))
         from boundaries in DisposalOps.AcquireAll(
             toSeq(Range(0, regions.RegionCount)).Map(index => Copies.AcquireNonEmpty(() => regions.RegionCurves(index), nameof(CurveBooleanRegions.RegionCurves))),
             static held => DisposalOps.Release(held.Flatten()))
         select Group(boundaries, toSeq(Range(0, regions.PointCount)).Map(regions.RegionPointIndex))).Bracket();

    private static Seq<Grouped<T>> Group<T>(Seq<T> outputs, Seq<int> key) =>
        outputs.Map((output, index) => new Grouped<T>(output, key.Choose((input, owner) => Some(input).Filter(_ => owner == index))));

    // --- [ANALYTIC]
    public static IO<NurbsCurve> EllipticalArc(Ellipse ellipse, Interval angles) =>
        Copies.Owned(
            IO.lift(() => Missing.Unless(NurbsCurve.CreateFromArc(new Arc(new Circle(ellipse.Plane, 1.0), angles)), nameof(NurbsCurve.CreateFromArc))),
            arc => Refused.Unless(arc.Transform(Transform.Scale(ellipse.Plane, ellipse.Radius1, ellipse.Radius2, 1.0)), arc, nameof(GeometryBase.Transform))
                .Bind(static scaled => Measurements.Valid(scaled, nameof(GeometryBase.Transform))));

    // --- [CATENARIES]
    public static IO<CatenaryCurve> CreateCatenaryCurve(Point3d start, Point3d end, Vector3d axis, CatenaryForm form, bool smooth, CatenaryPointCount pointCount) =>
        Copies.Owned(
            IO.lift<(Curve? Curve, Point3d Apex, double Parameter, double Length, double Deviation, string Member)>(() => form.Switch(
                (Start: start, End: end, Axis: axis, Smooth: smooth, Count: pointCount.ToValue()),
                throughPoint: static (hung, through) => (
                    Curve.CreateCatenaryCurveThroughPoint(hung.Start, hung.End, hung.Axis, through.Point, hung.Smooth, hung.Count, out Point3d apex, out double parameter, out double length, out double deviation),
                    apex, parameter, length, deviation, nameof(Curve.CreateCatenaryCurveThroughPoint)),
                fromLength: static (hung, of) => (
                    Curve.CreateCatenaryCurveFromLength(hung.Start, hung.End, hung.Axis, of.Length, hung.Smooth, hung.Count, out Point3d apex, out double parameter, out double length, out double deviation),
                    apex, parameter, length, deviation, nameof(Curve.CreateCatenaryCurveFromLength)),
                fromParameter: static (hung, of) => (
                    Curve.CreateCatenaryCurveFromParameter(hung.Start, hung.End, hung.Axis, of.Parameter, hung.Smooth, hung.Count, out Point3d apex, out double parameter, out double length, out double deviation),
                    apex, parameter, length, deviation, nameof(Curve.CreateCatenaryCurveFromParameter)),
                fromApex: static (hung, of) => (
                    Curve.CreateCatenaryCurveFromApex(hung.Start, hung.End, hung.Axis, of.Apex, hung.Smooth, hung.Count, out Point3d apex, out double parameter, out double length, out double deviation),
                    apex, parameter, length, deviation, nameof(Curve.CreateCatenaryCurveFromApex)))),
            static made => Missing.Unless(made.Curve, made.Member)
                .Bind(curve => Measurements.Valid(curve, made.Member))
                .Map(curve => new CatenaryCurve(curve, made.Apex, made.Parameter, made.Length, made.Deviation)),
            static made => DisposalOps.Release(Conversions.Rows(Seq(made.Curve))));
}
