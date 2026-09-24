using Rasm.Rhino.Document;
using Rasm.Rhino.Modeling.Curves;

namespace Arches.Profiles;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Ogee {
    // --- [LIMITS]
    public static Fin<Limits<double>> ThreeCenteredRise(Span span) => Limits.AtLeast(span.HalfSpan).AtMost(span.HalfSpan * (1 + Math.Sqrt(2)), nameof(ThreeCenteredRise));

    // --- [PROFILES]
    public static Fin<ArchProfile> ThreeCentered(Span span, Point3d apex) {
        Vector3d riseDirection = apex - span.Midpoint;
        Circle springingCircle = new(new Plane(span.Midpoint, span.Direction, riseDirection), span.HalfSpan);
        Line apexParallel = new(apex, span.Direction);
        return riseDirection.Length - springingCircle.Radius <= span.RiseTolerance
            ? ArchProfile.Mirrored(span, Seq(new Arc(springingCircle, Math.PI / 2)))
            : from points in ArchProfile.Secant(new Line(span.End, apex), springingCircle)
              from crossing in CurveConstruction.LineLine(new Line(span.Midpoint, points.Point2), apexParallel)
              let crownCenter = apexParallel.PointAt(crossing.B)
              let crownCircle = new Circle(new Plane(crownCenter, span.Direction, -riseDirection), crownCenter.DistanceTo(points.Point2))
              let endArc = new Arc(crownCircle, new Interval(Vector3d.VectorAngle(span.Direction, points.Point2 - crownCenter), Math.PI))
              let startArc = new Arc(springingCircle, Vector3d.VectorAngle(span.Direction, points.Point2 - span.Midpoint))
              from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
              select profile;
    }

    public static Fin<ArchProfile> FourCentered(Span span, Point3d apex) {
        Vector3d riseDirection = apex - span.Midpoint;
        Point3d haunchCenter = span.Midpoint + (span.Direction * (span.Length / 6));
        Circle haunchCircle = new(new Plane(haunchCenter, span.Direction, riseDirection), haunchCenter.DistanceTo(span.End));
        Line apexParallel = new(apex, span.Direction);
        return
            from points in ArchProfile.Secant(new Line(span.End, apex), haunchCircle)
            let startArc = new Arc(haunchCircle, Vector3d.VectorAngle(span.Direction, points.Point2 - haunchCenter))
            from crossing in CurveConstruction.LineLine(apexParallel, new Line(haunchCenter, points.Point2))
            let crownCenter = apexParallel.PointAt(crossing.A)
            let crownCircle = new Circle(new Plane(crownCenter, span.Direction, -riseDirection), crownCenter.DistanceTo(apex))
            let endArc = new Arc(crownCircle, new Interval(Vector3d.VectorAngle(span.Direction, points.Point2 - crownCenter), Math.PI))
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }

    public static Fin<ArchProfile> Reverse(Span span) {
        const double riseRatio = 19.0 / 61.0;
        Point3d apex = span.Midpoint + (span.Perpendicular * (span.Length * riseRatio));
        Line endToApex = new(span.End, apex);
        double endToApexQuarter = endToApex.Length / 4;
        Line endPerpendicular = new(span.End, span.Perpendicular);
        Point3d endToApexMidpoint = endToApex.PointAtLength(endToApexQuarter * 2);
        Vector3d endToApexPerpendicular = Transform.Rotation(Math.PI / 2, span.Normal, Point3d.Origin) * endToApex.Direction;
        Line firstQuarterPerpendicular = new(endToApex.PointAtLength(endToApexQuarter), endToApexPerpendicular);
        Line thirdQuarterPerpendicular = new(endToApex.PointAtLength(endToApexQuarter * 3), endToApexPerpendicular);
        return
            from haunchCrossing in CurveConstruction.LineLine(endPerpendicular, firstQuarterPerpendicular)
            let haunchCenter = endPerpendicular.PointAt(haunchCrossing.A)
            let haunchCircle = new Circle(new Plane(haunchCenter, span.Direction, -span.Perpendicular), haunchCenter.DistanceTo(span.End))
            from crownCrossing in CurveConstruction.LineLine(span.CenterLine, thirdQuarterPerpendicular)
            let crownCenter = span.CenterLine.PointAt(crownCrossing.A)
            let crownCircle = span.CircleAt(crownCenter, crownCenter.DistanceTo(apex))
            let startArc = new Arc(haunchCircle, new Interval(Math.PI / 2, Math.PI - Vector3d.VectorAngle(-span.Direction, endToApexMidpoint - haunchCenter)))
            let endArc = new Arc(crownCircle, new Interval(Vector3d.VectorAngle(span.Direction, endToApexMidpoint - crownCenter), Math.PI / 2))
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }

    public static Fin<ArchProfile> Tented(Span span) {
        Point3d apex = span.Midpoint + (span.Perpendicular * span.EquilateralHeight);
        Point3d haunchEnd = span.End + (new Line(span.End, apex).UnitTangent * span.HalfSpan);
        Point3d crownCenter = span.End + (span.Perpendicular * span.EquilateralHeight);
        Interval sweep = new(2 * Math.PI / 3, Math.PI);
        return
            from haunchSpan in Span.From(span.End, haunchEnd, span.Normal)
            let haunchCenter = haunchSpan.Midpoint - (haunchSpan.Perpendicular * haunchSpan.EquilateralHeight)
            let startArc = new Arc(new Circle(new Plane(haunchCenter, span.Direction, -span.Perpendicular), haunchCenter.DistanceTo(span.End)), sweep)
            let endArc = new Arc(new Circle(new Plane(crownCenter, span.Direction, -span.Perpendicular), crownCenter.DistanceTo(apex)), sweep)
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }
}
