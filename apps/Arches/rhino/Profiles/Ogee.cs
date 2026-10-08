using Rasm.Rhino;
using Rasm.Rhino.Modeling;
using Rhino.Geometry.Intersect;

namespace Arches.Profiles;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Ogee {
    // --- [LIMITS]
    public static Limits<double> ThreeCenteredRise(Span span) => Limits.AtLeast(span.HalfSpan).AtMost(span.HalfSpan * (1 + Math.Sqrt(2)));

    // --- [PROFILES]
    public static Fin<ArchProfile> ThreeCentered(Span span, Point3d apex) {
        Vector3d riseDirection = apex - span.Midpoint;
        Circle springingCircle = new(new Plane(span.Midpoint, span.Frame.XAxis, riseDirection), span.HalfSpan);
        Line apexParallel = new(apex, span.Frame.XAxis);
        return riseDirection.Length - springingCircle.Radius <= span.Tolerance
            ? ArchProfile.Mirrored(span, Seq(new Arc(springingCircle, Math.PI / 2)))
            : from points in Intersections.LineCircle(new Line(span.End, apex), springingCircle).Secant.ToFin(new Missing(nameof(Intersection.LineCircle)))
              from crossing in Intersections.LineLine(new Line(span.Midpoint, points.Second.Point), apexParallel).ToFin(new Missing(nameof(Intersection.LineLine)))
              let crownCenter = apexParallel.PointAt(crossing.B)
              let crownCircle = new Circle(new Plane(crownCenter, span.Frame.XAxis, -riseDirection), crownCenter.DistanceTo(points.Second.Point))
              let endArc = new Arc(crownCircle, new Interval(Vector3d.VectorAngle(span.Frame.XAxis, points.Second.Point - crownCenter), Math.PI))
              let startArc = new Arc(springingCircle, Vector3d.VectorAngle(span.Frame.XAxis, points.Second.Point - span.Midpoint))
              from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
              select profile;
    }

    public static Fin<ArchProfile> FourCentered(Span span, Point3d apex) {
        Vector3d riseDirection = apex - span.Midpoint;
        Point3d haunchCenter = span.Midpoint + (span.Frame.XAxis * (span.Length / 6));
        Circle haunchCircle = new(new Plane(haunchCenter, span.Frame.XAxis, riseDirection), haunchCenter.DistanceTo(span.End));
        Line apexParallel = new(apex, span.Frame.XAxis);
        return
            from points in Intersections.LineCircle(new Line(span.End, apex), haunchCircle).Secant.ToFin(new Missing(nameof(Intersection.LineCircle)))
            let startArc = new Arc(haunchCircle, Vector3d.VectorAngle(span.Frame.XAxis, points.Second.Point - haunchCenter))
            from crossing in Intersections.LineLine(apexParallel, new Line(haunchCenter, points.Second.Point)).ToFin(new Missing(nameof(Intersection.LineLine)))
            let crownCenter = apexParallel.PointAt(crossing.A)
            let crownCircle = new Circle(new Plane(crownCenter, span.Frame.XAxis, -riseDirection), crownCenter.DistanceTo(apex))
            let endArc = new Arc(crownCircle, new Interval(Vector3d.VectorAngle(span.Frame.XAxis, points.Second.Point - crownCenter), Math.PI))
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }

    public static Fin<ArchProfile> Reverse(Span span) {
        const double riseRatio = 19.0 / 61.0;
        Point3d apex = span.Midpoint + (span.Frame.YAxis * (span.Length * riseRatio));
        Line endToApex = new(span.End, apex);
        double endToApexQuarter = endToApex.Length / 4;
        Line endPerpendicular = new(span.End, span.Frame.YAxis);
        Point3d endToApexMidpoint = endToApex.PointAtLength(endToApexQuarter * 2);
        Vector3d endToApexPerpendicular = Vector3d.CrossProduct(span.Frame.ZAxis, endToApex.Direction);
        Line firstQuarterPerpendicular = new(endToApex.PointAtLength(endToApexQuarter), endToApexPerpendicular);
        Line thirdQuarterPerpendicular = new(endToApex.PointAtLength(endToApexQuarter * 3), endToApexPerpendicular);
        return
            from crossings in (Intersections.LineLine(endPerpendicular, firstQuarterPerpendicular).ToFin(new Missing(nameof(Intersection.LineLine))), Intersections.LineLine(span.CenterLine, thirdQuarterPerpendicular).ToFin(new Missing(nameof(Intersection.LineLine))))
                .Apply(static (haunch, crown) => (Haunch: haunch, Crown: crown))
                .As()
            let haunchCenter = endPerpendicular.PointAt(crossings.Haunch.A)
            let haunchCircle = new Circle(new Plane(haunchCenter, span.Frame.XAxis, -span.Frame.YAxis), haunchCenter.DistanceTo(span.End))
            let crownCenter = span.CenterLine.PointAt(crossings.Crown.A)
            let crownCircle = span.CircleAt(crownCenter, crownCenter.DistanceTo(apex))
            let startArc = new Arc(haunchCircle, new Interval(Math.PI / 2, Math.PI - Vector3d.VectorAngle(-span.Frame.XAxis, endToApexMidpoint - haunchCenter)))
            let endArc = new Arc(crownCircle, new Interval(Vector3d.VectorAngle(span.Frame.XAxis, endToApexMidpoint - crownCenter), Math.PI / 2))
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }

    public static Fin<ArchProfile> Tented(Span span) {
        Point3d apex = span.Midpoint + (span.Frame.YAxis * span.EquilateralHeight);
        Point3d haunchEnd = span.End + (new Line(span.End, apex).UnitTangent * span.HalfSpan);
        Point3d crownCenter = span.End + (span.Frame.YAxis * span.EquilateralHeight);
        Interval sweep = new(2 * Math.PI / 3, Math.PI);
        return
            from haunchSpan in Span.From(span.End, haunchEnd, span.Frame.ZAxis, span.Tolerance)
            let haunchCenter = haunchSpan.Midpoint - (haunchSpan.Frame.YAxis * haunchSpan.EquilateralHeight)
            let startArc = new Arc(new Circle(new Plane(haunchCenter, span.Frame.XAxis, -span.Frame.YAxis), haunchCenter.DistanceTo(span.End)), sweep)
            let endArc = new Arc(new Circle(new Plane(crownCenter, span.Frame.XAxis, -span.Frame.YAxis), crownCenter.DistanceTo(apex)), sweep)
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }
}
