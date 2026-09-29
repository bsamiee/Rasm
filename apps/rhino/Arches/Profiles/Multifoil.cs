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
        Func<int, Point3d> at = Divisions(guide, divisionCount);
        return
            from lobes in Range(1, (divisionCount - 2) / 2, 2).ToSeq().Traverse(index => Lobe(span, guide, at(index), at(index + 1), at(index + 2))).As()
            from profile in ArchProfile.Mirrored(span, Springing(span, at(0), at(1)).Cons(lobes) + Seq(Crown(span, at(divisionCount), at(divisionCount - 1))))
            select profile;
    }

    public static Fin<ArchProfile> Cinquefoil(Span span, bool pointed) => pointed ? PointedCinquefoil(span) : RoundedCinquefoil(span);

    public static Fin<ArchProfile> Trefoil(Span span, bool pointed) {
        Arc springingArc = new(span.CircleAt(span.Midpoint + (span.Frame.XAxis * span.QuarterSpan), span.QuarterSpan), Math.PI / 2);
        Fin<Arc> topArc = pointed
            ? Span.From(springingArc.EndPoint - (span.Frame.XAxis * span.QuarterSpan * 2), springingArc.EndPoint, span.Frame.ZAxis, span.Tolerance).Map(Gothic.EquilateralArc)
            : new Arc(span.CircleAt(span.Midpoint + (span.Frame.YAxis * span.QuarterSpan), span.QuarterSpan), Math.PI / 2);
        return topArc.Bind(top => ArchProfile.Mirrored(span, Seq(springingArc, top)));
    }

    private static Fin<ArchProfile> RoundedCinquefoil(Span span) {
        const int divisionCount = 5;
        Func<int, Point3d> at = Divisions(Guide(span, RoundedGuide, divisionCount, springing: 1), divisionCount);
        return ArchProfile.Mirrored(span, Seq(Springing(span, at(1), at(2)), Foil(span, at(2), at(3), at(4)), Crown(span, at(5), at(4))));
    }

    private static Fin<ArchProfile> PointedCinquefoil(Span span) {
        const int divisionCount = 6;
        Arc guide = Guide(span, PointedGuide, divisionCount, springing: 0);
        Func<int, Point3d> at = Divisions(guide, divisionCount);
        return
            from pointedSpan in Span.From(Transform.Mirror(guide.EndPoint, span.Frame.XAxis) * at(3), at(3), span.Frame.ZAxis, span.Tolerance)
            from profile in ArchProfile.Mirrored(span, Seq(Springing(span, at(0), at(1)), Foil(span, at(1), at(2), at(3)), Gothic.EquilateralArc(pointedSpan)))
            select profile;
    }

    private static int DivisionCount(int count) => count + (count & 1);

    private static Arc Guide(Span span, (double Center, double Sweep) shape, int divisionCount, int springing) {
        double step = shape.Sweep / divisionCount;
        double radius = span.HalfSpan / (shape.Center + Math.Cos(step * springing) + (2 * Math.Sin(step / 2)));
        Point3d center = span.Midpoint + (span.Frame.XAxis * (shape.Center * radius)) - (span.Frame.YAxis * (radius * Math.Sin(step * springing)));
        return new Arc(span.CircleAt(center, radius), shape.Sweep);
    }

    private static Func<int, Point3d> Divisions(Arc arc, int segmentCount) =>
        index => arc.PointAt(arc.AngleDomain.ParameterAt((double)index / segmentCount));

    private static Fin<Arc> Lobe(Span span, Arc guide, Point3d start, Point3d middle, Point3d end) {
        Point3d apex = middle + (Vector3d.CrossProduct(guide.TangentAt(guide.ClosestParameter(middle)), span.Frame.ZAxis) * end.DistanceTo(start) / 2);
        return Span.From(start, end, span.Frame.ZAxis, span.Tolerance).Bind(lobeSpan => Circular.ThroughApex(lobeSpan, apex));
    }

    private static Arc Foil(Span span, Point3d from, Point3d center, Point3d to) => new(from, Vector3d.CrossProduct(span.Frame.ZAxis, from - center), to);

    private static Arc Springing(Span span, Point3d center, Point3d through) =>
        new(span.CircleAt(center, center.DistanceTo(through)), Vector3d.VectorAngle(span.Frame.XAxis, through - center));

    private static Arc Crown(Span span, Point3d top, Point3d previous) =>
        new(span.CircleAt(top, top.DistanceTo(previous)), new Interval(-Vector3d.VectorAngle(span.Frame.XAxis, previous - top), Math.PI / 2));
}
