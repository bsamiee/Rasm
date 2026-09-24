using Rasm.Rhino.Document;

namespace Arches.Profiles;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Circular {
    public static Fin<ArchProfile> Semicircular(Span span) => new ArchProfile.Arcs(span, Seq(new Arc(span.Start, span.Perpendicular, span.End)));

    public static Fin<ArchProfile> SingleCentered(Span span, Point3d apex) => ThroughApex(span, apex).Map<ArchProfile>(arc => new ArchProfile.Arcs(span, Seq(arc)));

    public static Fin<Arc> ThroughApex(Span span, Point3d apex) {
        Arc arc = new(span.Start, apex, span.End);
        return Invalid.Unless(arc.IsValid, arc, nameof(Arc));
    }
}

public static class Gothic {
    // --- [LIMITS]
    public static Fin<Limits<double>> LancetRise(Span span) => Limits.AtLeast(span.EquilateralHeight);

    public static Fin<Limits<double>> DepressedRise(Span span) => Limits.AtLeast(span.HalfSpan).AtMost(span.EquilateralHeight, nameof(DepressedRise));

    // --- [PROFILES]
    public static Fin<ArchProfile> Equilateral(Span span) => ArchProfile.Mirrored(span, Seq(EquilateralArc(span)));

    public static Arc EquilateralArc(Span span) => new(span.CircleAt(span.Start, span.Length), Math.PI / 3);

    public static Fin<ArchProfile> Pointed(Span span, Point3d apex) => ArchProfile.Mirrored(span, Seq(new Arc(span.End, apex - span.Midpoint, apex)));
}

public static class Horseshoe {
    // --- [LIMITS]
    public static Fin<Limits<double>> PointedRise(Span span) => Limits.AtLeast(span.Length * Math.Sqrt(5.0 / 12.0));

    // --- [PROFILES]
    public static Fin<ArchProfile> Pointed(Span span, Point3d apex) {
        double rise = apex.DistanceTo(span.Midpoint);
        Vector3d riseDirection = new Line(span.Midpoint, apex).UnitTangent;
        double baseRadius = (Math.Pow(rise, 2) - (5.0 / 12.0 * Math.Pow(span.Length, 2))) / (2 * rise);
        Point3d sideCenter = span.Start + (span.Direction * (span.Length / 3)) + (riseDirection * baseRadius);
        Circle sideCircle = new(new Plane(sideCenter, span.Direction, riseDirection), sideCenter.DistanceTo(span.End));
        Interval sweep = new(-Math.Atan2(baseRadius, span.Length * 2 / 3), Vector3d.VectorAngle(span.Direction, apex - sideCenter));
        return ArchProfile.Mirrored(span, Seq(new Arc(sideCircle, sweep)));
    }
}
