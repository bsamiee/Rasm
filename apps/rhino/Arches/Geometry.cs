using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Modeling.Curves;

namespace Arches;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record ArchSpan {
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

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
internal readonly partial struct FoilCount : System.Numerics.IMinMaxValue<FoilCount> {
    public static FoilCount MinValue { get; } = new(7);
    public static FoilCount MaxValue { get; } = new(17);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = int.IsOddInteger(value)
            ? ValidationResults.OutsideRange(value, MinValue, MaxValue)
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"{value} is an even count, a multifoil arch has a key lobe at the crown"));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchGeometry {
    // --- [RISES]
    private static readonly double CrownDepth = double.Sqrt(0.5);
    private static readonly double HaunchRadius = 1 - CrownDepth;
    internal static readonly double PersianRise = double.Sqrt(((1 + HaunchRadius) * (1 + HaunchRadius)) - 0.25) - CrownDepth;
    internal static readonly double EquilateralRise = double.Sqrt(3) / 2;
    internal static readonly double OgeeRise = (1 + double.Sqrt(2)) / 2;

    // --- [SPANNED_ARCHES]
    internal static IO<Seq<Curve>> Semicircular(ArchSpan span, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(ArcCurve), _ => Answer.One<Curve>(new ArcCurve(new Arc(span.Baseline.From, span.Frame.YAxis, span.Baseline.To))).Map(static curve => Seq(curve)), retain);

    internal static IO<Seq<Curve>> Equilateral(ArchSpan span, Action<IDisposable> retain) =>
        Symmetric(span, Seq(new Arc(span.Frame, span.Baseline.Length, double.Pi / 3)), retain);

    internal static IO<Seq<Curve>> Persian(ArchSpan span, Action<IDisposable> retain) =>
        FourCentered(span, span.Frame, span.Baseline.Length * HaunchRadius, span.Baseline.From - (span.Frame.YAxis * (span.Baseline.Length * CrownDepth)), retain);

    internal static IO<Seq<Curve>> BasketHandle(ArchSpan span, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Point3d haunch = span.Baseline.PointAt(0.25);
        Point3d crown = span.Midpoint - (span.Frame.YAxis * (span.Baseline.Length * EquilateralRise / 2));
        return Symmetric(span, Seq(
            new Arc(new Circle(span.Frame, haunch, quarter), new Interval(2 * double.Pi / 3, double.Pi)),
            new Arc(new Circle(span.Frame, crown, crown.DistanceTo(haunch) + quarter), new Interval(double.Pi / 2, 2 * double.Pi / 3))), retain);
    }

    internal static IO<Seq<Curve>> Keel(ArchSpan span, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Point3d crown = span.Baseline.PointAt(0.25) + (span.Frame.YAxis * quarter);
        Point3d haunch = span.Baseline.To + (span.Frame.YAxis * quarter);
        return Symmetric(span, Seq(
            new Arc(new Circle(new Plane(haunch, span.Frame.XAxis, -span.Frame.YAxis), quarter), new Interval(double.Pi / 2, double.Pi)),
            new Arc(span.Frame, crown, span.Baseline.Length / 2, double.Pi / 3)), retain);
    }

    internal static IO<Seq<Curve>> TentedOgee(ArchSpan span, Action<IDisposable> retain) {
        double height = span.Baseline.Length * EquilateralRise;
        Point3d apex = span.Apex(height);
        Point3d haunchEnd = new Line(span.Baseline.To, apex).PointAtLength(span.Baseline.Length / 2);
        Line haunchLine = new(span.Baseline.To, haunchEnd);
        Plane haunchFrame = new(haunchLine.From, haunchLine.Direction, Vector3d.CrossProduct(span.Frame.ZAxis, haunchLine.Direction));
        Point3d haunch = haunchLine.PointAt(0.5) - (haunchFrame.YAxis * (haunchLine.Length * EquilateralRise));
        Point3d crown = span.Baseline.To + (span.Frame.YAxis * height);
        Interval sweep = new(2 * double.Pi / 3, double.Pi);
        return Symmetric(span, Seq(
            new Arc(new Circle(new Plane(haunch, span.Frame.XAxis, -span.Frame.YAxis), haunch.DistanceTo(span.Baseline.To)), sweep),
            new Arc(new Circle(new Plane(crown, span.Frame.XAxis, -span.Frame.YAxis), crown.DistanceTo(apex)), sweep)), retain);
    }

    // --- [RAISED_ARCHES]
    internal static IO<Seq<Curve>> Circular(ArchSpan span, Point3d apex, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(ArcCurve), _ => Answer.One<Curve>(new ArcCurve(new Arc(span.Baseline.From, apex, span.Baseline.To))).Map(static curve => Seq(curve)), retain);

    internal static IO<Seq<Curve>> Gothic(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double height = rise.Length;
        double radius = (height * (height / length)) + (length / 4);
        Point3d center = span.Baseline.To - (span.Frame.XAxis * radius);
        return Symmetric(span, Seq(new Arc(
            new Circle(new Plane(center, span.Frame.XAxis, rise.Direction), radius),
            2 * double.Atan2(length / 2, height))), retain);
    }

    internal static IO<Seq<Curve>> PointedHorseshoe(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double height = rise.Length;
        double offset = (height - (5.0 / 12 * length * (length / height))) / 2;
        Point3d center = span.Baseline.PointAt(1.0 / 3) + (rise.UnitTangent * offset);
        Circle circle = new(new Plane(center, span.Frame.XAxis, rise.Direction), center.DistanceTo(span.Baseline.To));
        return Symmetric(span, Seq(new Arc(circle, new Interval(-double.Atan2(offset, length * 2 / 3), Vector3d.VectorAngle(span.Frame.XAxis, apex - center)))), retain);
    }

    internal static IO<Seq<Curve>> DepressedThreeCentered(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
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

    internal static IO<Seq<Curve>> Tudor(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double ratio = rise.Length / length;
        double offset = (2 * ratio * ratio) - 0.5;
        double square = (16 * ratio * ratio) - 1;
        double linear = 8 * offset * ratio;
        double depth = (double.Sqrt((linear * linear) - (4 * square * ((offset * offset) - 0.25))) - linear) / (2 * square);
        return FourCentered(span, new Plane(span.Midpoint, span.Frame.XAxis, rise.Direction), length / 4, span.Baseline.PointAt(0.25) - (rise.UnitTangent * (length * depth)), retain);
    }

    internal static IO<Seq<Curve>> Ogee(ArchSpan span, Point3d apex, double haunch, Action<IDisposable> retain) {
        double radius = span.Baseline.Length * haunch;
        Circle springing = new(new Plane(span.Baseline.To - (span.Frame.XAxis * radius), span.Frame.XAxis, apex - span.Midpoint), radius);
        Vector3d diagonal = apex - span.Baseline.To;
        double reach = radius * span.Baseline.Length / diagonal.SquareLength;
        Point3d tangent = span.Baseline.To + (diagonal * reach);
        Point3d crown = springing.Center + ((tangent - springing.Center) / reach);
        return Symmetric(span, Seq(
            new Arc(springing, Vector3d.VectorAngle(span.Frame.XAxis, tangent - springing.Center)),
            new Arc(new Circle(new Plane(crown, springing.Plane.XAxis, -springing.Plane.YAxis), crown.DistanceTo(apex)), new Interval(Vector3d.VectorAngle(span.Frame.XAxis, tangent - crown), double.Pi))), retain);
    }

    internal static IO<Seq<Curve>> ReverseOgee(ArchSpan span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double ratio = rise.Length / length;
        Point3d haunch = span.Baseline.To + (rise.UnitTangent * (length * ((ratio / 4) + (1 / (16 * ratio)))));
        Point3d crown = span.Midpoint + (rise.UnitTangent * (length * ((3 * ratio / 4) - (1 / (16 * ratio)))));
        Point3d diagonalMidpoint = new Line(span.Baseline.To, apex).PointAt(0.5);
        Plane frame = new(span.Midpoint, span.Frame.XAxis, rise.Direction);
        return Symmetric(span, Seq(
            new Arc(new Circle(new Plane(haunch, frame.XAxis, -frame.YAxis), haunch.DistanceTo(span.Baseline.To)), new Interval(double.Pi / 2, double.Pi - Vector3d.VectorAngle(-frame.XAxis, diagonalMidpoint - haunch))),
            new Arc(new Circle(frame, crown, crown.DistanceTo(apex)), new Interval(Vector3d.VectorAngle(frame.XAxis, diagonalMidpoint - crown), double.Pi / 2))), retain);
    }

    internal static IO<Seq<Curve>> Parabolic(ArchSpan span, Point3d apex, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(NurbsCurve.CreateParabolaFromVertex), _ => Answer.One<Curve>(NurbsCurve.CreateParabolaFromVertex(apex, span.Baseline.From, span.Baseline.To)).Map(static curve => Seq(curve)), retain);

    internal static IO<Seq<Curve>> Elliptical(ArchSpan span, Point3d apex, Action<IDisposable> retain) =>
        new Conic.EllipseArc(new Ellipse(new Plane(span.Midpoint, -span.Frame.XAxis, apex - span.Midpoint), span.Baseline.Length / 2, span.Midpoint.DistanceTo(apex)), new Interval(0, double.Pi))
            .ToNurbs(retain)
            .Map(static curve => Seq<Curve>(curve));

    private static IO<Seq<Curve>> FourCentered(ArchSpan span, Plane frame, double haunchRadius, Point3d crown, Action<IDisposable> retain) {
        Point3d haunch = span.Baseline.To - (frame.XAxis * haunchRadius);
        double angle = Vector3d.VectorAngle(frame.XAxis, haunch - crown);
        Circle circle = new(frame, crown, crown.DistanceTo(haunch) + haunchRadius);
        return Symmetric(span, Seq(new Arc(frame, haunch, haunchRadius, angle), new Arc(circle, new Interval(angle, double.Acos((span.Midpoint - crown) * frame.XAxis / circle.Radius)))), retain);
    }

    // --- [FOILED_ARCHES]
    internal static IO<Seq<Curve>> Trefoil(ArchSpan span, bool pointed, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Arc springing = new(span.Frame, span.Baseline.PointAt(0.75), quarter, double.Pi / 2);
        Arc crown = pointed
            ? new Arc(span.Frame, springing.EndPoint - (span.Frame.XAxis * (quarter * 2)), quarter * 2, double.Pi / 3)
            : new Arc(span.Frame, span.Midpoint + (span.Frame.YAxis * quarter), quarter, double.Pi / 2);
        return Symmetric(span, Seq(springing, crown), retain);
    }

    internal static IO<Seq<Curve>> Cinquefoil(ArchSpan span, bool pointed, Action<IDisposable> retain) {
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

    internal static IO<Seq<Curve>> Multifoil(ArchSpan span, bool pointed, FoilCount count, Tolerances tolerances, Action<IDisposable> retain) {
        int lobes = (int)count;
        double divisions = lobes - 1;
        Func<double, Point3d> at = FoilDivisions(FoilGuide(span, pointed, divisions), divisions);
        return at(0).DistanceTo(at(1)) > tolerances.Absolute
            ? Symmetric(span, FoilSpringing(span.Frame, at(0), at(1)).Cons(Range(1, (lobes - 2) / 2, 2).ToSeq().Map(index => FoilLobe(span.Frame, at(index + 1), at(index), at(index + 2))))
                .Add(FoilCrown(span.Frame, at(divisions), at(divisions - 1))), retain)
            : IO.fail<Seq<Curve>>(Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"Foil count {lobes} makes lobes shorter than the absolute tolerance {tolerances.Absolute} across this span")));
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

    // --- [SYMMETRY]
    private static IO<Seq<Curve>> Symmetric(ArchSpan span, Seq<Arc> arcs, Action<IDisposable> retain) =>
        Transform.Mirror(span.Midpoint, span.Frame.XAxis) switch {
            var mirror => Copies.Acquire(nameof(ArcCurve), _ => Answer.Many<Curve>(arcs.Bind(arc => Seq(
                new Arc(new Circle(new Plane(mirror * arc.Center, mirror * arc.Plane.XAxis, mirror * arc.Plane.YAxis), arc.Radius), arc.AngleDomain),
                new Arc(new Circle(new Plane(arc.Center, arc.Plane.XAxis, -arc.Plane.YAxis), arc.Radius), new Interval(-arc.EndAngle, -arc.StartAngle)))).Map(static arc => new ArcCurve(arc))), retain),
        };
}
