namespace Arches.Profiles;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FourCentered {
    public static Fin<ArchProfile> Persian(Span span) {
        Point3d quarterPoint = span.Start + (span.Frame.XAxis * span.QuarterSpan);
        Point3d threeQuarterPoint = span.Start + (span.Frame.XAxis * (span.QuarterSpan * 3));
        Point3d triangleApex = span.Midpoint - (span.Frame.YAxis * (span.EquilateralHeight / 2));
        Circle haunchCircle = span.CircleAt(threeQuarterPoint, span.QuarterSpan);
        Line quarterPerpendicular = new(quarterPoint, span.Frame.YAxis);
        Line riseLine = new(span.Midpoint, span.Frame.YAxis, span.Length);
        return
            from crossing in CurveConstruction.LineLine(new Line(threeQuarterPoint, triangleApex), quarterPerpendicular)
            let crownCenter = quarterPerpendicular.PointAt(crossing.B)
            let tangentAngle = Vector3d.VectorAngle(span.Frame.XAxis, threeQuarterPoint - crownCenter)
            let crownCircle = span.CircleAt(crownCenter, crownCenter.DistanceTo(threeQuarterPoint) + span.QuarterSpan)
            from points in CurveConstruction.LineCircle(riseLine, crownCircle).Bind(static met => met.Secant)
            let startArc = new Arc(haunchCircle, tangentAngle)
            let endArc = new Arc(crownCircle, new Interval(tangentAngle, Vector3d.VectorAngle(span.Frame.XAxis, points.Point2 - crownCenter)))
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }

    public static Fin<ArchProfile> Tudor(Span span) {
        Point3d crownCenter = span.Start - (span.Frame.YAxis * span.HalfSpan) + (span.Frame.XAxis * span.QuarterSpan);
        Point3d haunchCenter = span.Start + (span.Frame.XAxis * (3 * span.QuarterSpan));
        double tangentAngle = Vector3d.VectorAngle(span.Frame.XAxis, haunchCenter - crownCenter);
        Circle crownCircle = span.CircleAt(crownCenter, crownCenter.DistanceTo(haunchCenter) + span.QuarterSpan);
        Arc startArc = new(span.CircleAt(haunchCenter, span.QuarterSpan), tangentAngle);
        return
            from points in CurveConstruction.LineCircle(new Line(span.Midpoint, span.Frame.YAxis, span.Length), crownCircle).Bind(static met => met.Secant)
            let endArc = new Arc(crownCircle, new Interval(tangentAngle, Vector3d.VectorAngle(span.Frame.XAxis, points.Point2 - crownCenter)))
            from profile in ArchProfile.Mirrored(span, Seq(startArc, endArc))
            select profile;
    }

    public static Fin<ArchProfile> Keel(Span span) {
        Point3d crownCenter = span.Start + (span.Frame.YAxis * span.QuarterSpan) + (span.Frame.XAxis * span.QuarterSpan);
        Arc endArc = new(span.CircleAt(crownCenter, span.HalfSpan), Math.PI / 3);
        Point3d haunchCenter = span.End + (span.Frame.YAxis * span.QuarterSpan);
        Arc startArc = new(new Circle(new Plane(haunchCenter, span.Frame.XAxis, -span.Frame.YAxis), span.QuarterSpan), new Interval(Math.PI / 2, Math.PI));
        return ArchProfile.Mirrored(span, Seq(startArc, endArc));
    }
}
