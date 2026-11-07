using System.Globalization;
using System.Runtime.CompilerServices;
using Rasm.Rhino;
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
            var frame => ArchFault.Unless(frame.IsValid, ArchReason.Planeless, (baseline, normal)).Map(_ => new ArchSpan(baseline, frame)),
        };

    public double Height(Point3d point) => (point - Midpoint) * Frame.YAxis;

    public Point3d Apex(double height) => Midpoint + (Frame.YAxis * height);
}

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
internal sealed partial class ArchReason {
    public static readonly ArchReason Planeless = new("baseline and normal spanning no plane");
    public static readonly ArchReason SpanNarrow = new("span has no usable rise ratio");
    public static readonly ArchReason RiseOutside = new("rise outside the span's range");
    public static readonly ArchReason LobesOutside = new("lobe count outside the arch's set");

    public string Message { get; }
}

internal sealed record ArchFault(object? Subject, ArchReason Reason, string Expression = "") : Expected(Reason.Message, 0) {
    public object? Subject { get; } = Subject;
    public ArchReason Reason { get; } = Reason;
    public string Expression { get; } = Expression;

    public override string Message => Expression.Length is 0
        ? string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Subject}")
        : string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Expression} = {Subject}");

    public static Fin<Unit> Unless<T>(bool flag, ArchReason reason, T rejected, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new ArchFault(rejected, reason, names);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class ArchGeometry {

    // --- [RISES]

    private static readonly double CrownDepth = double.Sqrt(0.5);
    private static readonly double HaunchRadius = 1 - CrownDepth;
    internal static readonly double PersianRise = double.Sqrt(((1 + HaunchRadius) * (1 + HaunchRadius)) - 0.25) - CrownDepth;
    internal static readonly double EquilateralRise = double.Sqrt(3) / 2;
    internal static readonly double OgeeRise = (1 + double.Sqrt(2)) / 2;
    internal static readonly double BasketRise = (3 - double.Sqrt(3)) / 4;
    internal static readonly double KeelRise = (1 + double.Sqrt(3)) / 4;

    // --- [RISEN_ARCHES]

    internal static IO<Seq<Curve>> Circular(ArchSpan span, Point3d apex) =>
        Products.Many<Curve>(() => [new ArcCurve(new Arc(span.Baseline.From, apex, span.Baseline.To))]);

    internal static IO<Seq<Curve>> Gothic(ArchSpan span, Point3d apex) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double radius = (rise.Length * (rise.Length / length)) + (length / 4);
        return Symmetric(span, Seq(new Arc(new Circle(new Plane(span.Baseline.To - (span.Frame.XAxis * radius), span.Frame.XAxis, rise.Direction), radius), 2 * double.Atan2(length / 2, rise.Length))));
    }

    internal static IO<Seq<Curve>> PointedHorseshoe(ArchSpan span, Point3d apex) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double offset = (rise.Length - (5d / 12 * length * (length / rise.Length))) / 2;
        Point3d center = span.Baseline.PointAt(1d / 3) + (rise.UnitTangent * offset);
        return Symmetric(span, Seq(new Arc(
            new Circle(new Plane(center, span.Frame.XAxis, rise.Direction), center.DistanceTo(span.Baseline.To)),
            new Interval(-double.Atan2(offset, length * 2 / 3), Vector3d.VectorAngle(span.Frame.XAxis, apex - center)))));
    }

    internal static IO<Seq<Curve>> Ogee(ArchSpan span, Point3d apex, double haunch) {
        double radius = span.Baseline.Length * haunch;
        Circle springing = new(new Plane(span.Baseline.To - (span.Frame.XAxis * radius), span.Frame.XAxis, apex - span.Midpoint), radius);
        Vector3d diagonal = apex - span.Baseline.To;
        double reach = radius * span.Baseline.Length / diagonal.SquareLength;
        Point3d tangent = span.Baseline.To + (diagonal * reach);
        Point3d crown = springing.Center + ((tangent - springing.Center) / reach);
        return Symmetric(span, Seq(
            new Arc(springing, Vector3d.VectorAngle(span.Frame.XAxis, tangent - springing.Center)),
            new Arc(new Circle(new Plane(crown, springing.Plane.XAxis, -springing.Plane.YAxis), crown.DistanceTo(apex)), new Interval(Vector3d.VectorAngle(span.Frame.XAxis, tangent - crown), double.Pi))));
    }

    internal static IO<Seq<Curve>> ReverseOgee(ArchSpan span, Point3d apex) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double ratio = rise.Length / length;
        Point3d haunch = span.Baseline.To + (rise.UnitTangent * (length * ((ratio / 4) + (1 / (16 * ratio)))));
        Point3d crown = span.Midpoint + (rise.UnitTangent * (length * ((3 * ratio / 4) - (1 / (16 * ratio)))));
        Point3d diagonal = new Line(span.Baseline.To, apex).PointAt(0.5);
        Plane frame = new(span.Midpoint, span.Frame.XAxis, rise.Direction);
        return Symmetric(span, Seq(
            new Arc(new Circle(new Plane(haunch, frame.XAxis, -frame.YAxis), haunch.DistanceTo(span.Baseline.To)), new Interval(double.Pi / 2, double.Pi - Vector3d.VectorAngle(-frame.XAxis, diagonal - haunch))),
            new Arc(new Circle(frame, crown, crown.DistanceTo(apex)), new Interval(Vector3d.VectorAngle(frame.XAxis, diagonal - crown), double.Pi / 2))));
    }

    internal static IO<Seq<Curve>> Tented(ArchSpan span, Point3d apex) {
        Line haunchLine = new(span.Baseline.To, new Line(span.Baseline.To, apex).PointAtLength(span.Baseline.Length / 2));
        Plane haunchFrame = new(haunchLine.From, haunchLine.Direction, Vector3d.CrossProduct(span.Frame.ZAxis, haunchLine.Direction));
        Point3d haunch = haunchLine.PointAt(0.5) - (haunchFrame.YAxis * (haunchLine.Length * EquilateralRise));
        Point3d crown = span.Baseline.To + (apex - span.Midpoint);
        Interval sweep = new(2 * double.Pi / 3, double.Pi);
        return Symmetric(span, Seq(
            new Arc(new Circle(new Plane(haunch, span.Frame.XAxis, -span.Frame.YAxis), haunch.DistanceTo(span.Baseline.To)), sweep),
            new Arc(new Circle(new Plane(crown, span.Frame.XAxis, -span.Frame.YAxis), crown.DistanceTo(apex)), sweep)));
    }

    internal static IO<Seq<Curve>> BasketHandle(ArchSpan span) {
        double quarter = span.Baseline.Length / 4;
        Point3d haunch = span.Baseline.PointAt(0.25);
        Point3d crown = span.Midpoint - (span.Frame.YAxis * (span.Baseline.Length * EquilateralRise / 2));
        return Symmetric(span, Seq(
            new Arc(new Circle(span.Frame, haunch, quarter), new Interval(2 * double.Pi / 3, double.Pi)),
            new Arc(new Circle(span.Frame, crown, crown.DistanceTo(haunch) + quarter), new Interval(double.Pi / 2, 2 * double.Pi / 3))));
    }

    internal static IO<Seq<Curve>> DepressedThreeCentered(ArchSpan span, Point3d apex) {
        Line rise = new(span.Midpoint, apex);
        double half = span.Baseline.Length / 2;
        double ratio = rise.Length / half;
        double diagonal = new Line(span.Baseline.From, apex).Length / half;
        double radius = rise.Length / 2 * diagonal * (1 + (ratio / (diagonal + 1)));
        Point3d haunch = span.Baseline.From + (span.Frame.XAxis * radius);
        Point3d crown = rise.From + (rise.UnitTangent * ((radius - half) / ratio));
        double angle = Vector3d.VectorAngle(span.Frame.XAxis, (rise.UnitTangent * half) - (span.Frame.XAxis * rise.Length));
        Plane frame = new(rise.From, span.Frame.XAxis, rise.Direction);
        return Symmetric(span, Seq(
            new Arc(new Circle(frame, haunch, radius), new Interval(angle, double.Pi)),
            new Arc(new Circle(frame, crown, crown.DistanceTo(apex)), new Interval(double.Pi / 2, angle))));
    }

    internal static IO<Seq<Curve>> Tudor(ArchSpan span, Point3d apex) {
        double length = span.Baseline.Length;
        Line rise = new(span.Midpoint, apex);
        double ratio = rise.Length / length;
        double offset = (2 * ratio * ratio) - 0.5;
        double square = (16 * ratio * ratio) - 1;
        double linear = 8 * offset * ratio;
        double depth = (double.Sqrt((linear * linear) - (4 * square * ((offset * offset) - 0.25))) - linear) / (2 * square);
        return FourCentered(span, new Plane(span.Midpoint, span.Frame.XAxis, rise.Direction), length / 4, span.Baseline.PointAt(0.25) - (rise.UnitTangent * (length * depth)));
    }

    internal static IO<Seq<Curve>> Persian(ArchSpan span) =>
        FourCentered(span, span.Frame, span.Baseline.Length * HaunchRadius, span.Baseline.From - (span.Frame.YAxis * (span.Baseline.Length * CrownDepth)));

    internal static IO<Seq<Curve>> Keel(ArchSpan span) {
        double quarter = span.Baseline.Length / 4;
        return Symmetric(span, Seq(
            new Arc(new Circle(new Plane(span.Baseline.To + (span.Frame.YAxis * quarter), span.Frame.XAxis, -span.Frame.YAxis), quarter), new Interval(double.Pi / 2, double.Pi)),
            new Arc(span.Frame, span.Baseline.PointAt(0.25) + (span.Frame.YAxis * quarter), span.Baseline.Length / 2, double.Pi / 3)));
    }

    internal static IO<Seq<Curve>> Parabolic(ArchSpan span, Point3d apex) =>
        Products.Many<Curve>(() => [NurbsCurve.CreateParabolaFromVertex(apex, span.Baseline.From, span.Baseline.To)]);

    internal static IO<Seq<Curve>> Elliptical(ArchSpan span, Point3d apex) =>
        new Ellipse(new Plane(span.Midpoint, -span.Frame.XAxis, apex - span.Midpoint), span.Baseline.Length / 2, span.Midpoint.DistanceTo(apex))
            .ToNurbs(new Interval(0d, double.Pi)).Map(static curve => Seq<Curve>(curve));

    private static IO<Seq<Curve>> FourCentered(ArchSpan span, Plane frame, double haunchRadius, Point3d crown) {
        Point3d haunch = span.Baseline.To - (frame.XAxis * haunchRadius);
        double angle = Vector3d.VectorAngle(frame.XAxis, haunch - crown);
        Circle circle = new(frame, crown, crown.DistanceTo(haunch) + haunchRadius);
        return Symmetric(span, Seq(new Arc(frame, haunch, haunchRadius, angle), new Arc(circle, new Interval(angle, double.Acos((span.Midpoint - crown) * frame.XAxis / circle.Radius)))));
    }

    // --- [FOILED_ARCHES]

    internal static IO<Seq<Curve>> Trefoil(ArchSpan span, bool pointed) {
        double quarter = span.Baseline.Length / 4;
        Arc springing = new(span.Frame, span.Baseline.PointAt(0.75), quarter, double.Pi / 2);
        return Symmetric(span, Seq(springing, pointed
            ? new Arc(span.Frame, springing.EndPoint - (span.Frame.XAxis * (quarter * 2)), quarter * 2, double.Pi / 3)
            : new Arc(span.Frame, span.Midpoint + (span.Frame.YAxis * quarter), quarter, double.Pi / 2)));
    }

    internal static IO<Seq<Curve>> Foils(ArchSpan span, bool pointed, int lobes, bool pointedCrown) {
        int divisions = lobes - 1;
        Arc guide = FoilGuide(span, pointed, divisions);
        Seq<Point3d> points = Range(0, divisions).ToSeq().Map(index => guide.PointAt(guide.AngleDomain.ParameterAt((double)index / divisions))).Strict();
        Point3d guideEnd = guide.EndPoint;
        Point3d crownStart = points[^1];
        Line crownLine = new(Transform.Mirror(guideEnd, span.Frame.XAxis) * crownStart, crownStart);
        return Symmetric(span, FoilSpringing(span.Frame, points[0], points[1])
            .Cons(Range(1, (lobes - 2) / 2, 2).ToSeq().Map(index => FoilLobe(span.Frame, points[index + 1], points[index], points[index + 2])))
            .Add(pointedCrown
                ? new Arc(new Plane(crownLine.From, crownLine.Direction, Vector3d.CrossProduct(span.Frame.ZAxis, crownLine.Direction)), crownLine.Length, double.Pi / 3)
                : new Arc(new Circle(span.Frame, guideEnd, guideEnd.DistanceTo(crownStart)), new Interval(-Vector3d.VectorAngle(span.Frame.XAxis, crownStart - guideEnd), double.Pi / 2))));
    }

    private static Arc FoilGuide(ArchSpan span, bool pointed, double divisions) {
        (double centerFactor, double sweep) = pointed ? (-0.5, double.Pi / 3) : (0, double.Pi / 2);
        double radius = span.Baseline.Length / 2 / (centerFactor + 1 + (2 * double.Sin(sweep / divisions / 2)));
        return new Arc(span.Frame, span.Midpoint + (span.Frame.XAxis * (centerFactor * radius)), radius, sweep);
    }

    private static Arc FoilSpringing(Plane frame, Point3d center, Point3d through) =>
        new(frame, center, center.DistanceTo(through), Vector3d.VectorAngle(frame.XAxis, through - center));

    private static Arc FoilLobe(Plane frame, Point3d center, Point3d from, Point3d to) =>
        new(new Circle(frame, center, center.DistanceTo(from)), new Interval(-Vector3d.VectorAngle(frame.XAxis, from - center), Vector3d.VectorAngle(frame.XAxis, to - center)));

    // --- [SYMMETRY]

    private static IO<Seq<Curve>> Symmetric(ArchSpan span, Seq<Arc> arcs) =>
        Transform.Mirror(span.Midpoint, span.Frame.XAxis) switch {
            var mirror => Products.Each(arcs.Map(arc => Products.Accepted(() => {
                Arc reflected = arc;
                return (reflected.Transform(mirror), reflected);
            }).Map(reflected => {
                Arc reversed = arc;
                reversed.Reverse();
                return Seq(reflected, reversed);
            }))).Bind(static pairs => Products.Many(() => pairs.Flatten().Map(static arc => (Curve)new ArcCurve(arc)))),
        };
}
