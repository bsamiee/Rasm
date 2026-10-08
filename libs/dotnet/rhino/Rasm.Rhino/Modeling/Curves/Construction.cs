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

[Union<Point3d, double, double, Point3d>(T1Name = "ThroughPoint", T2Name = "FromLength", T3Name = "FromParameter", T4Name = "FromApex", MapMethods = SwitchMapMethodsGeneration.None)]
public readonly partial struct CatenaryForm;

public sealed record CatenaryCurve(Curve Curve, Point3d Apex, double Parameter, double Length, double MaxDeviation);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveConstruction {
    // --- [JOINS_AND_BOOLEANS]
    public static IO<Seq<Grouped<Curve>>> JoinCurves(Seq<Curve> curves, Tolerances tolerances, bool preserveDirection, bool simpleJoin) =>
        Copies.Owned(
            IO.lift(() => (Curves: Curve.JoinCurves(curves, tolerances.Absolute, preserveDirection, simpleJoin, out int[] key), Key: key)),
            static answer => Measurements.Valid(toSeq(answer.Curves), nameof(Curve.JoinCurves)).Map(joined => Group(joined, toSeq(answer.Key))),
            static answer => DisposalOps.Release(Conversions.Rows(answer.Curves)));

    public static IO<Seq<Grouped<Curve>>> CreateBooleanUnion(Seq<Curve> curves, Tolerances tolerances) =>
        Copies.Owned(
            IO.lift(() => (Curves: Curve.CreateBooleanUnion(curves, tolerances.Absolute, out int[] indexMap), IndexMap: indexMap)),
            static answer => Measurements.Valid(toSeq(answer.Curves), nameof(Curve.CreateBooleanUnion))
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
        key.Map(static (owner, input) => (owner, input)).ToLookup(static entry => entry.owner, static entry => entry.input) switch {
            var inputs => outputs.Map((output, index) => new Grouped<T>(output, toSeq(inputs[index]))).Strict(),
        };

    // --- [ANALYTIC]
    public static IO<NurbsCurve> EllipticalArc(Ellipse ellipse, Interval angles) =>
        Copies.Owned(
            IO.lift(() => Missing.Unless(NurbsCurve.CreateFromArc(new Arc(new Circle(ellipse.Plane, 1.0), angles)), nameof(NurbsCurve.CreateFromArc))),
            arc => Refused.Unless(arc.Transform(Transform.Scale(ellipse.Plane, ellipse.Radius1, ellipse.Radius2, 1.0)), arc, nameof(GeometryBase.Transform))
                .Bind(static scaled => Measurements.Valid(scaled, nameof(GeometryBase.Transform))));

    // --- [CATENARIES]
    public static IO<CatenaryCurve> CreateCatenaryCurve(Point3d start, Point3d end, Vector3d axis, CatenaryForm form, bool smooth, CatenaryPointCount pointCount) =>
        Copies.Owned(
            IO.lift(() => form.Switch(
                (Start: start, End: end, Axis: axis, Smooth: smooth, Count: pointCount.ToValue()),
                throughPoint: static (state, through) => (
                    Curve: Curve.CreateCatenaryCurveThroughPoint(state.Start, state.End, state.Axis, through, state.Smooth, state.Count, out Point3d apex, out double parameter, out double length, out double deviation),
                    Apex: apex, Parameter: parameter, Length: length, Deviation: deviation, Member: nameof(Curve.CreateCatenaryCurveThroughPoint)),
                fromLength: static (state, length) => (
                    Curve: Curve.CreateCatenaryCurveFromLength(state.Start, state.End, state.Axis, length, state.Smooth, state.Count, out Point3d apex, out double parameter, out double computedLength, out double deviation),
                    Apex: apex, Parameter: parameter, Length: computedLength, Deviation: deviation, Member: nameof(Curve.CreateCatenaryCurveFromLength)),
                fromParameter: static (state, parameter) => (
                    Curve: Curve.CreateCatenaryCurveFromParameter(state.Start, state.End, state.Axis, parameter, state.Smooth, state.Count, out Point3d apex, out double computedParameter, out double length, out double deviation),
                    Apex: apex, Parameter: computedParameter, Length: length, Deviation: deviation, Member: nameof(Curve.CreateCatenaryCurveFromParameter)),
                fromApex: static (state, apex) => (
                    Curve: Curve.CreateCatenaryCurveFromApex(state.Start, state.End, state.Axis, apex, state.Smooth, state.Count, out Point3d computedApex, out double parameter, out double length, out double deviation),
                    Apex: computedApex, Parameter: parameter, Length: length, Deviation: deviation, Member: nameof(Curve.CreateCatenaryCurveFromApex)))),
            static made => made switch {
                var (curve, apex, parameter, length, deviation, member) =>
                    from present in Missing.Unless(curve, member)
                    from valid in Measurements.Valid(present, member)
                    select new CatenaryCurve(valid, apex, parameter, length, deviation),
            },
            static made => DisposalOps.Release(Optional(made.Curve).ToSeq()));
}
