using System.Diagnostics;
using Rasm.Rhino.Document;

namespace Arches.Profiles;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Multifoil {
    // --- [LIMITS]
    public static readonly Limits<int> FoilCount = Limits.AtLeast(1);

    // --- [GUIDES]
    private static readonly (double Center, double Sweep) RoundedGuide = (0.0, Math.PI / 2);

    private static readonly (double Center, double Sweep) PointedGuide = (-0.5, Math.PI / 3);

    // --- [PROFILES]
    public static Fin<ArchProfile> Foils(Span span, bool pointed, int count) {
        int divisionCount = DivisionCount(count);
        Arc guide = Guide(span, pointed ? PointedGuide : RoundedGuide, divisionCount, springing: 0);
        Point3d[] points = DivisionPoints(guide, divisionCount);
        return
            from lobes in Range(1, (divisionCount - 2) / 2, 2).ToSeq().Traverse(index => Lobe(span, guide, points[index], points[index + 1], points[index + 2])).As()
            from profile in ArchProfile.Mirrored(span, Springing(span, points[0], points[1]).Cons(lobes) + Seq(Crown(span, points[^1], points[^2])))
            select profile;
    }

    public static Fin<ArchProfile> Cinquefoil(Span span, bool pointed) => pointed ? PointedCinquefoil(span) : RoundedCinquefoil(span);

    public static Fin<ArchProfile> Trefoil(Span span, bool pointed) {
        Arc springingArc = new(span.CircleAt(span.Midpoint + (span.Direction * span.QuarterSpan), span.QuarterSpan), Math.PI / 2);
        Fin<Arc> topArc = pointed
            ? Span.From(springingArc.EndPoint - (span.Direction * span.QuarterSpan * 2), springingArc.EndPoint, span.Normal).Map(Gothic.EquilateralArc)
            : new Arc(span.CircleAt(span.Midpoint + (span.Perpendicular * span.QuarterSpan), span.QuarterSpan), Math.PI / 2);
        return topArc.Bind(top => ArchProfile.Mirrored(span, Seq(springingArc, top)));
    }

    private static Fin<ArchProfile> RoundedCinquefoil(Span span) {
        const int divisionCount = 5;
        return DivisionPoints(Guide(span, RoundedGuide, divisionCount, springing: 1), divisionCount) switch {
            [_, var springing, var first, var center, var last, var crown] =>
                ArchProfile.Mirrored(span, Seq(Springing(span, springing, first), Foil(span, first, center, last), Crown(span, crown, last))),
            _ => throw new UnreachableException(),
        };
    }

    private static Fin<ArchProfile> PointedCinquefoil(Span span) {
        const int divisionCount = 6;
        Arc guide = Guide(span, PointedGuide, divisionCount, springing: 0);
        return DivisionPoints(guide, divisionCount) switch {
            [var springing, var first, var center, var last, ..] =>
                from pointedSpan in Span.From(Transform.Mirror(guide.EndPoint, span.Direction) * last, last, span.Normal)
                from profile in ArchProfile.Mirrored(span, Seq(Springing(span, springing, first), Foil(span, first, center, last), Gothic.EquilateralArc(pointedSpan)))
                select profile,
            _ => throw new UnreachableException(),
        };
    }

    private static int DivisionCount(int count) => count + (count & 1);

    private static Arc Guide(Span span, (double Center, double Sweep) shape, int divisionCount, int springing) {
        double step = shape.Sweep / divisionCount;
        double radius = span.HalfSpan / (shape.Center + Math.Cos(step * springing) + (2 * Math.Sin(step / 2)));
        Point3d center = span.Midpoint + (span.Direction * (shape.Center * radius)) - (span.Perpendicular * (radius * Math.Sin(step * springing)));
        return new Arc(span.CircleAt(center, radius), shape.Sweep);
    }

    private static Point3d[] DivisionPoints(Arc arc, int segmentCount) =>
        [.. Range(0, segmentCount + 1).Select(index => arc.PointAt(arc.AngleDomain.ParameterAt((double)index / segmentCount)))];

    private static Fin<Arc> Lobe(Span span, Arc guide, Point3d start, Point3d middle, Point3d end) {
        Point3d apex = middle + (Vector3d.CrossProduct(guide.TangentAt(guide.ClosestParameter(middle)), span.Normal) * end.DistanceTo(start) / 2);
        return Span.From(start, end, span.Normal).Bind(lobeSpan => Circular.ThroughApex(lobeSpan, apex));
    }

    private static Arc Foil(Span span, Point3d from, Point3d center, Point3d to) => new(from, Vector3d.CrossProduct(span.Normal, from - center), to);

    private static Arc Springing(Span span, Point3d center, Point3d through) =>
        new(span.CircleAt(center, center.DistanceTo(through)), Vector3d.VectorAngle(span.Direction, through - center));

    private static Arc Crown(Span span, Point3d top, Point3d previous) =>
        new(span.CircleAt(top, top.DistanceTo(previous)), new Interval(-Vector3d.VectorAngle(span.Direction, previous - top), Math.PI / 2));
}
