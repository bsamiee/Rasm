namespace Arches.Profiles;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ThreeCentered {
    public static Fin<ArchProfile> BasketHandle(Span span) {
        Point3d quarterPoint = span.Start + (span.Direction * span.QuarterSpan);
        Point3d crownCenter = span.Midpoint - (span.Perpendicular * (span.EquilateralHeight / 2));
        Arc startArc = new(span.CircleAt(quarterPoint, span.QuarterSpan), new Interval(2 * Math.PI / 3, Math.PI));
        Circle crownCircle = span.CircleAt(crownCenter, crownCenter.DistanceTo(quarterPoint) + span.QuarterSpan);
        return ArchProfile.Mirrored(span, Seq(startArc, new Arc(crownCircle, new Interval(Math.PI / 2, 2 * Math.PI / 3))));
    }

    public static Fin<ArchProfile> Depressed(Span span, Point3d apex) {
        Line rise = new(span.Midpoint, apex);
        Vector3d riseDirection = rise.UnitTangent;
        Point3d semicircleQuad = rise.PointAtLength(span.HalfSpan);
        Point3d apexCircleQuad = new Line(apex, span.Start).PointAtLength(apex.DistanceTo(semicircleQuad));
        Point3d startToQuadMidpoint = new Line(span.Start, apexCircleQuad).PointAt(0.5);
        Vector3d startToQuad = apexCircleQuad - span.Start;
        double quadAlongSpan = startToQuad * span.Direction;
        Point3d haunchCenter = span.Start + (span.Direction * (startToQuad.SquareLength / (2 * quadAlongSpan)));
        Point3d crownCenter = span.Midpoint + (riseDirection * ((startToQuad.SquareLength - (span.Length * quadAlongSpan)) / (2 * (startToQuad * riseDirection))));
        Circle crownCircle = new(new Plane(crownCenter, span.Direction, riseDirection), crownCenter.DistanceTo(apex));
        double quadAngle = Vector3d.VectorAngle(span.Direction, startToQuadMidpoint - crownCenter);
        Arc endArc = new(crownCircle, new Interval(Math.PI / 2, quadAngle));
        Circle haunchCircle = new(new Plane(haunchCenter, span.Direction, riseDirection), haunchCenter.DistanceTo(span.Start));
        Arc startArc = new(haunchCircle, new Interval(quadAngle, Math.PI));
        return ArchProfile.Mirrored(span, Seq(startArc, endArc));
    }
}
