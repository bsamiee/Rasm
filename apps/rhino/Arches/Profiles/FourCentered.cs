using Rasm.Rhino.Modeling.Curves;

namespace Arches.Profiles;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FourCentered {
    public static Fin<ArchProfile> Persian(Span span) {
        Point3d quarterPoint = span.Start + (span.Direction * span.QuarterSpan);
        Point3d threeQuarterPoint = span.Start + (span.Direction * (span.QuarterSpan * 3));
        Point3d triangleApex = span.Midpoint - (span.Perpendicular * (span.EquilateralHeight / 2));
        Circle haunchCircle = span.CircleAt(threeQuarterPoint, span.QuarterSpan);
        Line quarterPerpendicular = new(quarterPoint, span.Perpendicular);
        Line riseLine = new(span.Midpoint, span.Perpendicular, span.Length);
        return
            from crossing in CurveConstruction.LineLine(new Line(threeQuarterPoint, triangleApex), quarterPerpendicular)
            let crownCenter = quarterPerpendicular.PointAt(crossing.B)
            let tangentAngle = Vector3d.VectorAngle(span.Direction, threeQuarterPoint - crownCenter)
            let crownCircle = span.CircleAt(crownCenter, crownCenter.DistanceTo(threeQuarterPoint) + span.QuarterSpan)
            from points in ArchProfile.Secant(riseLine, crownCircle)
            let startArc = new Arc(haunchCircle, tangentAngle)
            let endArc = new Arc(crownCircle, new Interval(tangentAngle, Vector3d.VectorAngle(span.Direction, points.Point2 - crownCenter)))
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }

    public static Fin<ArchProfile> Tudor(Span span) {
        Point3d crownCenter = span.Start - (span.Perpendicular * span.HalfSpan) + (span.Direction * span.QuarterSpan);
        Point3d haunchCenter = span.Start + (span.Direction * (3 * span.QuarterSpan));
        double tangentAngle = Vector3d.VectorAngle(span.Direction, haunchCenter - crownCenter);
        Circle crownCircle = span.CircleAt(crownCenter, crownCenter.DistanceTo(haunchCenter) + span.QuarterSpan);
        Arc startArc = new(span.CircleAt(haunchCenter, span.QuarterSpan), tangentAngle);
        return
            from points in ArchProfile.Secant(new Line(span.Midpoint, span.Perpendicular, span.Length), crownCircle)
            let endArc = new Arc(crownCircle, new Interval(tangentAngle, Vector3d.VectorAngle(span.Direction, points.Point2 - crownCenter)))
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }

    public static Fin<ArchProfile> Keel(Span span) {
        Point3d crownCenter = span.Start + (span.Perpendicular * span.QuarterSpan) + (span.Direction * span.QuarterSpan);
        Arc endArc = new(span.CircleAt(crownCenter, span.HalfSpan), Math.PI / 3);
        Point3d haunchCenter = span.End + (span.Perpendicular * span.QuarterSpan);
        Arc startArc = new(new Circle(new Plane(haunchCenter, span.Direction, -span.Perpendicular), span.QuarterSpan), new Interval(Math.PI / 2, Math.PI));
        return ArchProfile.Mirrored(span, Seq(startArc, endArc));
    }
}
