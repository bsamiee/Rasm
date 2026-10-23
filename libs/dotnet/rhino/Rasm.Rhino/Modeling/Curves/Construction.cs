using System.Globalization;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Curves;

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class PanjOHaft {
    public static readonly double CrownDepth = double.Sqrt(0.5);
    public static readonly double HaunchRadius = 1 - CrownDepth;
    public static readonly double Rise = double.Sqrt(((1 + HaunchRadius) * (1 + HaunchRadius)) - 0.25) - CrownDepth;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Grouped<T>(T Value, Seq<int> Inputs);

[Union<Point3d, double, double, Point3d>(T1Name = "ThroughPoint", T2Name = "FromLength", T3Name = "FromParameter", T4Name = "FromApex", MapMethods = SwitchMapMethodsGeneration.None)]
public readonly partial struct CatenaryForm;

public sealed record CatenaryCurve(Curve Curve, Point3d Apex, double Parameter, double Length, double MaxDeviation);

public sealed record ArchSpan {
    private ArchSpan(Line baseline, Plane frame) => (Baseline, Frame) = (baseline, frame);

    public Line Baseline { get; }
    public Plane Frame { get; }
    public Point3d Midpoint => Baseline.PointAt(0.5);

    public static Fin<ArchSpan> Of(Line baseline, Vector3d normal) =>
        new Plane(baseline.From, baseline.Direction, Vector3d.CrossProduct(normal, baseline.Direction)) switch {
            { IsValid: true } frame => new ArchSpan(baseline, frame),
            _ => Error.New(ErrorCodes.ValidationFailed, "Arch baseline and normal span no plane"),
        };

    public double Height(Point3d point) => (point - Midpoint) * Frame.YAxis;

    public Point3d Apex(double height) => Midpoint + (Frame.YAxis * height);
}

[SmartEnum(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SpannedArch {
    public static readonly SpannedArch Semicircular = new();
    public static readonly SpannedArch Equilateral = new();
    public static readonly SpannedArch BasketHandle = new();
    public static readonly SpannedArch Persian = new();
    public static readonly SpannedArch Keel = new();
    public static readonly SpannedArch TentedOgee = new();
}

[SmartEnum(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RaisedArch {
    public static readonly RaisedArch Segmental = new(new Interval(0, 0.5));
    public static readonly RaisedArch Horseshoe = new(new Interval(0.5, double.MaxValue));
    public static readonly RaisedArch Lancet = new(new Interval(double.Sqrt(3) / 2, double.MaxValue));
    public static readonly RaisedArch Drop = new(new Interval(0.5, double.Sqrt(3) / 2));
    public static readonly RaisedArch PointedHorseshoe = new(new Interval(double.Sqrt(5.0 / 12), double.MaxValue));
    public static readonly RaisedArch DepressedThreeCentered = new(new Interval(0, 0.5));
    public static readonly RaisedArch Tudor = new(new Interval(0.25, PanjOHaft.Rise));
    public static readonly RaisedArch ThreeCenteredOgee = new(new Interval(0.5, (1 + double.Sqrt(2)) / 2));
    public static readonly RaisedArch FourCenteredOgee = new(new Interval(0.5, (1 + double.Sqrt(2)) / 2));
    public static readonly RaisedArch ReverseOgee = new(new Interval(0, 0.5));
    public static readonly RaisedArch Parabolic = new(new Interval(0, double.MaxValue));
    public static readonly RaisedArch Elliptical = new(new Interval(0, double.MaxValue));

    public Interval Ratios { get; }

    public Fin<Interval> Admitted(ArchSpan span, Tolerances tolerances) =>
        (tolerances.Absolute / span.Baseline.Length) switch {
            var margin => new Interval(Ratios.T0 + margin, Ratios.T1 - margin) switch {
                { IsIncreasing: true } admitted => admitted,
                _ => Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"Arch span {span.Baseline.Length} admits no rise ratio inside {Ratios}")),
            },
        };
}

[SmartEnum(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class FoiledArch {
    public static readonly FoiledArch Trefoil = new();
    public static readonly FoiledArch Cinquefoil = new();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FoilCount : System.Numerics.IMinMaxValue<FoilCount> {
    public static FoilCount MinValue { get; } = new(7);
    public static FoilCount MaxValue { get; } = new(17);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = int.IsOddInteger(value)
            ? ValidationResults.OutsideRange(value, MinValue, MaxValue)
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"{value} is an even count, a multifoil arch has a key lobe at the crown"));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ArchForm {
    public sealed record Spanned(SpannedArch Kind) : ArchForm;
    public sealed record Raised : ArchForm {
        private Raised(RaisedArch kind, double ratio) => (Kind, Ratio) = (kind, ratio);

        public RaisedArch Kind { get; }
        public double Ratio { get; }

        public static Fin<Raised> Of(RaisedArch kind, ArchSpan span, double height, Tolerances tolerances) =>
            kind.Admitted(span, tolerances).Bind(admitted => (height / span.Baseline.Length) switch {
                var ratio when admitted.IncludesParameter(double.Abs(ratio)) => Fin.Succ(new Raised(kind, ratio)),
                var ratio => Error.New(ErrorCodes.ValidationFailed, admitted.Max is double.MaxValue
                    ? string.Create(CultureInfo.InvariantCulture, $"Arch rise {height:G4} is shorter than the least rise {admitted.Min * span.Baseline.Length:G4} on this span")
                    : string.Create(CultureInfo.InvariantCulture, $"Arch rise {height:G4} lies outside {admitted.Min * span.Baseline.Length:G4} to {admitted.Max * span.Baseline.Length:G4} on this span")),
            });

        public static Fin<Raised> Nearest(RaisedArch kind, ArchSpan span, double height, Tolerances tolerances) =>
            kind.Admitted(span, tolerances).Map(admitted => new Raised(kind, double.CopySign(double.Clamp(double.Abs(height) / span.Baseline.Length, admitted.Min, admitted.Max), height)));

        public Point3d Apex(ArchSpan span) => span.Apex(Ratio * span.Baseline.Length);
    }

    public sealed record Foiled(FoiledArch Kind, bool Pointed) : ArchForm;
    public sealed record Multifoil(FoilCount Count, bool Pointed) : ArchForm;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveConstruction {
    // --- [JOINS_AND_BOOLEANS]
    public static IO<Seq<Grouped<Curve>>> JoinCurves(Seq<Curve> curves, Tolerances tolerances, bool preserveDirection, bool simpleJoin, Action<IDisposable> retain) =>
        Copies.Owned(
            () => (Curves: Curve.JoinCurves(curves, tolerances.Absolute, preserveDirection, simpleJoin, out int[] key), Key: key),
            static answer => Measurements.Valid(toSeq(answer.Curves), nameof(Curve.JoinCurves)).Map(joined => Group(joined, toSeq(answer.Key))),
            static answer => DisposalOps.Release(Conversions.Rows(answer.Curves)), retain);

    public static IO<Seq<Grouped<Curve>>> CreateBooleanUnion(Seq<Curve> curves, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Owned(
            () => (Curves: Curve.CreateBooleanUnion(curves, tolerances.Absolute, out int[] indexMap), IndexMap: indexMap),
            static answer => Measurements.Valid(toSeq(answer.Curves), nameof(Curve.CreateBooleanUnion))
                .Bind(static united => Conversions.NonEmpty(united, nameof(Curve.CreateBooleanUnion)))
                .Map(united => Group(united, toSeq(answer.IndexMap))),
            static answer => DisposalOps.Release(Conversions.Rows(answer.Curves)), retain);

    public static IO<Seq<Grouped<Seq<Curve>>>> CreateBooleanRegions(Seq<Curve> curves, Plane plane, Seq<Point3d> points, bool combineRegions, Tolerances tolerances, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned => DisposalOps.Found(
            () => Curve.CreateBooleanRegions(curves, plane, points, combineRegions, tolerances.Absolute), nameof(Curve.CreateBooleanRegions),
            regions => toSeq(Range(0, regions.RegionCount))
                .TraverseM(index => Copies.AcquireNonEmpty(() => regions.RegionCurves(index), nameof(CurveBooleanRegions.RegionCurves), owned)).As()
                .Map(boundaries => Group(boundaries, toSeq(Range(0, regions.PointCount)).Map(regions.RegionPointIndex)))), retain);

    private static Seq<Grouped<T>> Group<T>(Seq<T> outputs, Seq<int> key) =>
        key.Map(static (owner, input) => (owner, input)).ToLookup(static entry => entry.owner, static entry => entry.input) switch {
            var inputs => outputs.Map((output, index) => new Grouped<T>(output, toSeq(inputs[index]))).Strict(),
        };

    // --- [ANALYTIC]
    public static IO<Seq<Curve>> Arcs(Seq<Arc> arcs, Action<IDisposable> retain) =>
        Callbacks.Each(arcs.Map(arc => Copies.Acquire<Curve>(() => new ArcCurve(arc), nameof(ArcCurve), retain)));

    public static IO<NurbsCurve> EllipticalArc(Ellipse ellipse, Interval angles, Action<IDisposable> retain) =>
        Copies.Owned(
            () => NurbsCurve.CreateFromArc(new Arc(new Circle(ellipse.Plane, 1.0), angles)),
            made => Refused.UnlessFound(made, nameof(NurbsCurve.CreateFromArc))
                .Bind(arc => Refused.Unless(arc.Transform(Transform.Scale(ellipse.Plane, ellipse.Radius1, ellipse.Radius2, 1.0)), arc, nameof(GeometryBase.Transform)))
                .Bind(static scaled => Measurements.Valid(scaled, nameof(GeometryBase.Transform))),
            static made => DisposalOps.Release(made), retain);

    // --- [CATENARIES]
    public static IO<CatenaryCurve> CreateCatenaryCurve(Point3d start, Point3d end, Vector3d axis, CatenaryForm form, bool smooth, int pointCount, Action<IDisposable> retain) =>
        Copies.Owned(
            () => form.Switch(
                (Start: start, End: end, Axis: axis, Smooth: smooth, Count: pointCount),
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
                    Apex: computedApex, Parameter: parameter, Length: length, Deviation: deviation, Member: nameof(Curve.CreateCatenaryCurveFromApex))),
            static made => Measurements.Valid(made.Curve, made.Member).Map(valid => new CatenaryCurve(valid, made.Apex, made.Parameter, made.Length, made.Deviation)),
            static made => DisposalOps.Release(made.Curve), retain);

    // --- [ARCHES]
    public static IO<Seq<Curve>> CreateArch(ArchSpan span, ArchForm form, Tolerances tolerances, Action<IDisposable> retain) =>
        form.Switch(
            (Span: span, Tolerances: tolerances, Retain: retain),
            spanned: static (state, spanned) => spanned.Kind.Switch(
                state,
                semicircular: static arch => Arcs(Seq(new Arc(arch.Span.Baseline.From, arch.Span.Frame.YAxis, arch.Span.Baseline.To)), arch.Retain),
                equilateral: static arch => Symmetric(arch.Span, Seq(new Arc(arch.Span.Frame, arch.Span.Baseline.Length, double.Pi / 3)), arch.Retain),
                basketHandle: static arch => BasketHandle(arch.Span, arch.Retain),
                persian: static arch => FourCentered(arch.Span, arch.Span.Frame, arch.Span.Baseline.Length * PanjOHaft.HaunchRadius, arch.Span.Baseline.From - (arch.Span.Frame.YAxis * (arch.Span.Baseline.Length * PanjOHaft.CrownDepth)), arch.Retain),
                keel: static arch => Keel(arch.Span, arch.Retain),
                tentedOgee: static arch => TentedOgee(arch.Span, arch.Retain)),
            raised: static (state, raised) => raised.Kind.Switch(
                (state.Span, state.Retain, Apex: raised.Apex(state.Span)),
                segmental: static arch => Circular(arch.Span, arch.Apex, arch.Retain),
                horseshoe: static arch => Circular(arch.Span, arch.Apex, arch.Retain),
                lancet: static arch => Gothic(arch.Span, arch.Apex, arch.Retain),
                drop: static arch => Gothic(arch.Span, arch.Apex, arch.Retain),
                pointedHorseshoe: static arch => PointedHorseshoe(arch.Span, arch.Apex, arch.Retain),
                depressedThreeCentered: static arch => DepressedThreeCentered(arch.Span, arch.Apex, arch.Retain),
                tudor: static arch => Tudor(arch.Span, arch.Apex, arch.Retain),
                threeCenteredOgee: static arch => Ogee(arch.Span, arch.Apex, arch.Span.Baseline.Length / 2, arch.Retain),
                fourCenteredOgee: static arch => Ogee(arch.Span, arch.Apex, arch.Span.Baseline.Length / 3, arch.Retain),
                reverseOgee: static arch => ReverseOgee(arch.Span, arch.Apex, arch.Retain),
                parabolic: static arch => Copies.Acquire(() => NurbsCurve.CreateParabolaFromVertex(arch.Apex, arch.Span.Baseline.From, arch.Span.Baseline.To), nameof(NurbsCurve.CreateParabolaFromVertex), arch.Retain)
                    .Map(static curve => Seq<Curve>(curve)),
                elliptical: static arch => EllipticalArc(new Ellipse(new Plane(arch.Span.Midpoint, -arch.Span.Frame.XAxis, arch.Apex - arch.Span.Midpoint), arch.Span.Baseline.Length / 2, arch.Span.Midpoint.DistanceTo(arch.Apex)), new Interval(0, double.Pi), arch.Retain)
                    .Map(static curve => Seq<Curve>(curve))),
            foiled: static (state, foiled) => foiled.Kind.Switch(
                (state.Span, state.Retain, foiled.Pointed),
                trefoil: static arch => Trefoil(arch.Span, arch.Pointed, arch.Retain),
                cinquefoil: static arch => Cinquefoil(arch.Span, arch.Pointed, arch.Retain)),
            multifoil: static (state, multifoil) => Multifoil(state.Span, multifoil.Pointed, multifoil.Count, state.Tolerances, state.Retain));

    // --- [CENTERED_ARCHES]
    private static IO<Seq<Curve>> Circular(ArchSpan span, Point3d apex, Action<IDisposable> retain) =>
        Arcs(Seq(new Arc(span.Baseline.From, apex, span.Baseline.To)), retain);

    private static IO<Seq<Curve>> Gothic(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double height = rise.Length;
        double radius = (height * (height / length)) + (length / 4);
        Point3d center = span.Baseline.To - (span.Frame.XAxis * radius);
        return Symmetric(span, Seq(new Arc(
            new Circle(new Plane(center, span.Frame.XAxis, rise.Direction), radius),
            2 * double.Atan2(length / 2, height))), retain);
    }

    private static IO<Seq<Curve>> PointedHorseshoe(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double height = rise.Length;
        double offset = (height - (5.0 / 12 * length * (length / height))) / 2;
        Point3d center = span.Baseline.PointAt(1.0 / 3) + (rise.UnitTangent * offset);
        Circle circle = new(new Plane(center, span.Frame.XAxis, rise.Direction), center.DistanceTo(span.Baseline.To));
        return Symmetric(span, Seq(new Arc(circle, new Interval(-double.Atan2(offset, length * 2 / 3), Vector3d.VectorAngle(span.Frame.XAxis, apex - center)))), retain);
    }

    private static IO<Seq<Curve>> BasketHandle(ArchSpan span, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Point3d haunch = span.Baseline.PointAt(0.25);
        Point3d crown = span.Midpoint - (span.Frame.YAxis * (span.Baseline.Length * double.Sqrt(3) / 4));
        return Symmetric(span, Seq(
            new Arc(new Circle(span.Frame, haunch, quarter), new Interval(2 * double.Pi / 3, double.Pi)),
            new Arc(new Circle(span.Frame, crown, crown.DistanceTo(haunch) + quarter), new Interval(double.Pi / 2, 2 * double.Pi / 3))), retain);
    }

    private static IO<Seq<Curve>> DepressedThreeCentered(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
        Line rise = new(span.Midpoint, apex);
        double half = span.Baseline.Length / 2;
        double height = rise.Length;
        double ratio = height / half;
        double diagonal = new Line(span.Baseline.From, apex).Length / half;
        double radius = height / 2 * diagonal * (1 + (ratio / (diagonal + 1)));
        Point3d haunch = span.Baseline.From + (span.Frame.XAxis * radius);
        Point3d crown = rise.From + (rise.UnitTangent * ((radius - half) / ratio));
        double angle = Vector3d.VectorAngle(span.Frame.XAxis, (rise.UnitTangent * half) - (span.Frame.XAxis * height));
        Plane frame = new(rise.From, span.Frame.XAxis, rise.Direction);
        return Symmetric(span, Seq(
            new Arc(new Circle(frame, haunch, radius), new Interval(angle, double.Pi)),
            new Arc(new Circle(frame, crown, crown.DistanceTo(apex)), new Interval(double.Pi / 2, angle))), retain);
    }

    private static IO<Seq<Curve>> Keel(ArchSpan span, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Point3d crown = span.Baseline.PointAt(0.25) + (span.Frame.YAxis * quarter);
        Point3d haunch = span.Baseline.To + (span.Frame.YAxis * quarter);
        return Symmetric(span, Seq(
            new Arc(new Circle(new Plane(haunch, span.Frame.XAxis, -span.Frame.YAxis), quarter), new Interval(double.Pi / 2, double.Pi)),
            new Arc(span.Frame, crown, span.Baseline.Length / 2, double.Pi / 3)), retain);
    }

    private static IO<Seq<Curve>> Tudor(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double ratio = rise.Length / length;
        double offset = (2 * ratio * ratio) - 0.5;
        double square = (16 * ratio * ratio) - 1;
        double linear = 8 * offset * ratio;
        double depth = (double.Sqrt((linear * linear) - (4 * square * ((offset * offset) - 0.25))) - linear) / (2 * square);
        return FourCentered(span, new Plane(span.Midpoint, span.Frame.XAxis, rise.Direction), length / 4, span.Baseline.PointAt(0.25) - (rise.UnitTangent * (length * depth)), retain);
    }

    private static IO<Seq<Curve>> FourCentered(ArchSpan span, Plane frame, double haunchRadius, Point3d crown, Action<IDisposable> retain) {
        Point3d haunch = span.Baseline.To - (frame.XAxis * haunchRadius);
        double angle = Vector3d.VectorAngle(frame.XAxis, haunch - crown);
        Circle circle = new(frame, crown, crown.DistanceTo(haunch) + haunchRadius);
        return Symmetric(span, Seq(new Arc(frame, haunch, haunchRadius, angle), new Arc(circle, new Interval(angle, double.Acos((span.Midpoint - crown) * frame.XAxis / circle.Radius)))), retain);
    }

    // --- [FOILED_ARCHES]
    private static IO<Seq<Curve>> Multifoil(ArchSpan span, bool pointed, FoilCount count, Tolerances tolerances, Action<IDisposable> retain) {
        int lobes = (int)count;
        double divisions = lobes - 1;
        Arc guide = FoilGuide(span, pointed, divisions);
        Func<double, Point3d> at = FoilDivisions(guide, divisions);
        return at(0).DistanceTo(at(1)) > tolerances.Absolute
            ? Symmetric(span, FoilSpringing(span.Frame, at(0), at(1)).Cons(Range(1, (lobes - 2) / 2, 2).ToSeq().Map(index => FoilLobe(span.Frame, at(index + 1), at(index), at(index + 2))))
                .Add(FoilCrown(span.Frame, at(divisions), at(divisions - 1))), retain)
            : IO.fail<Seq<Curve>>(Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"Foil count {lobes} makes lobes shorter than the absolute tolerance {tolerances.Absolute} across this span")));
    }

    private static IO<Seq<Curve>> Cinquefoil(ArchSpan span, bool pointed, Action<IDisposable> retain) {
        const int divisions = 4;
        Arc guide = FoilGuide(span, pointed, divisions);
        Func<double, Point3d> at = FoilDivisions(guide, divisions);
        Point3d crownEnd = at(3);
        Line crownLine = new(Transform.Mirror(guide.EndPoint, span.Frame.XAxis) * crownEnd, crownEnd);
        Arc crown = pointed
            ? new Arc(new Plane(crownLine.From, crownLine.Direction, Vector3d.CrossProduct(span.Frame.ZAxis, crownLine.Direction)), crownLine.Length, double.Pi / 3)
            : FoilCrown(span.Frame, at(divisions), crownEnd);
        return Symmetric(span, Seq(FoilSpringing(span.Frame, at(0), at(1)), FoilLobe(span.Frame, at(2), at(1), crownEnd), crown), retain);
    }

    private static IO<Seq<Curve>> Trefoil(ArchSpan span, bool pointed, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Arc springing = new(span.Frame, span.Baseline.PointAt(0.75), quarter, double.Pi / 2);
        Arc crown = pointed
            ? new Arc(span.Frame, springing.EndPoint - (span.Frame.XAxis * (quarter * 2)), quarter * 2, double.Pi / 3)
            : new Arc(span.Frame, span.Midpoint + (span.Frame.YAxis * quarter), quarter, double.Pi / 2);
        return Symmetric(span, Seq(springing, crown), retain);
    }

    private static Arc FoilGuide(ArchSpan span, bool pointed, double divisions) {
        (double centerFactor, double sweep) = pointed ? (-0.5, double.Pi / 3) : (0, double.Pi / 2);
        double radius = span.Baseline.Length / 2 / (centerFactor + 1 + (2 * double.Sin(sweep / divisions / 2)));
        return new Arc(span.Frame, span.Midpoint + (span.Frame.XAxis * (centerFactor * radius)), radius, sweep);
    }

    private static Func<double, Point3d> FoilDivisions(Arc guide, double divisions) =>
        index => guide.PointAt(guide.AngleDomain.ParameterAt(index / divisions));

    private static Arc FoilSpringing(Plane frame, Point3d center, Point3d through) =>
        new(frame, center, center.DistanceTo(through), Vector3d.VectorAngle(frame.XAxis, through - center));

    private static Arc FoilLobe(Plane frame, Point3d center, Point3d from, Point3d to) =>
        new(new Circle(frame, center, center.DistanceTo(from)), new Interval(-Vector3d.VectorAngle(frame.XAxis, from - center), Vector3d.VectorAngle(frame.XAxis, to - center)));

    private static Arc FoilCrown(Plane frame, Point3d top, Point3d previous) =>
        new(new Circle(frame, top, top.DistanceTo(previous)), new Interval(-Vector3d.VectorAngle(frame.XAxis, previous - top), double.Pi / 2));

    // --- [OGEE_ARCHES]
    private static IO<Seq<Curve>> Ogee(ArchSpan span, Point3d apex, double radius, Action<IDisposable> retain) {
        Circle springing = new(new Plane(span.Baseline.To - (span.Frame.XAxis * radius), span.Frame.XAxis, apex - span.Midpoint), radius);
        Vector3d diagonal = apex - span.Baseline.To;
        double reach = radius * span.Baseline.Length / diagonal.SquareLength;
        Point3d tangent = span.Baseline.To + (diagonal * reach);
        Point3d crown = springing.Center + ((tangent - springing.Center) / reach);
        return Symmetric(span, Seq(
            new Arc(springing, Vector3d.VectorAngle(span.Frame.XAxis, tangent - springing.Center)),
            new Arc(new Circle(new Plane(crown, springing.Plane.XAxis, -springing.Plane.YAxis), crown.DistanceTo(apex)), new Interval(Vector3d.VectorAngle(span.Frame.XAxis, tangent - crown), double.Pi))), retain);
    }

    private static IO<Seq<Curve>> ReverseOgee(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double ratio = rise.Length / length;
        double haunchRatio = (ratio / 4) + (1 / (16 * ratio));
        double crownRatio = (3 * ratio / 4) - (1 / (16 * ratio));
        Point3d haunch = span.Baseline.To + (rise.UnitTangent * (length * haunchRatio));
        Point3d crown = span.Midpoint + (rise.UnitTangent * (length * crownRatio));
        Point3d diagonalMidpoint = new Line(span.Baseline.To, apex).PointAt(0.5);
        Plane frame = new(span.Midpoint, span.Frame.XAxis, rise.Direction);
        return Symmetric(span, Seq(
            new Arc(new Circle(new Plane(haunch, frame.XAxis, -frame.YAxis), haunch.DistanceTo(span.Baseline.To)), new Interval(double.Pi / 2, double.Pi - Vector3d.VectorAngle(-frame.XAxis, diagonalMidpoint - haunch))),
            new Arc(new Circle(frame, crown, crown.DistanceTo(apex)), new Interval(Vector3d.VectorAngle(frame.XAxis, diagonalMidpoint - crown), double.Pi / 2))), retain);
    }

    private static IO<Seq<Curve>> TentedOgee(ArchSpan span, Action<IDisposable> retain) {
        double equilateralRise = double.Sqrt(3) / 2;
        double height = span.Baseline.Length * equilateralRise;
        Point3d apex = span.Apex(height);
        Point3d haunchEnd = new Line(span.Baseline.To, apex).PointAtLength(span.Baseline.Length / 2);
        Line haunchLine = new(span.Baseline.To, haunchEnd);
        Plane haunchFrame = new(haunchLine.From, haunchLine.Direction, Vector3d.CrossProduct(span.Frame.ZAxis, haunchLine.Direction));
        Point3d haunch = haunchLine.PointAt(0.5) - (haunchFrame.YAxis * (haunchLine.Length * equilateralRise));
        Point3d crown = span.Baseline.To + (span.Frame.YAxis * height);
        Interval sweep = new(2 * double.Pi / 3, double.Pi);
        return Symmetric(span, Seq(
            new Arc(new Circle(new Plane(haunch, span.Frame.XAxis, -span.Frame.YAxis), haunch.DistanceTo(span.Baseline.To)), sweep),
            new Arc(new Circle(new Plane(crown, span.Frame.XAxis, -span.Frame.YAxis), crown.DistanceTo(apex)), sweep)), retain);
    }

    // --- [ARCH_SYMMETRY]
    private static IO<Seq<Curve>> Symmetric(ArchSpan span, Seq<Arc> arcs, Action<IDisposable> retain) =>
        Transform.Mirror(span.Midpoint, span.Frame.XAxis) switch {
            var mirror => Arcs(arcs.Bind(arc => Seq(
                new Arc(new Circle(new Plane(mirror * arc.Center, mirror * arc.Plane.XAxis, mirror * arc.Plane.YAxis), arc.Radius), arc.AngleDomain),
                new Arc(new Circle(new Plane(arc.Center, arc.Plane.XAxis, -arc.Plane.YAxis), arc.Radius), new Interval(-arc.EndAngle, -arc.StartAngle)))), retain),
        };
}
