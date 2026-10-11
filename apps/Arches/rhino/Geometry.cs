using Rasm.Rhino;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Modeling.Curves;
using Rhino.Geometry.Intersect;

namespace Arches;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchCurves {
    // --- [RISE_DOMAINS]
    public static (Interval Ratios, bool OpenMinimum) PositiveRise => (new Interval(0, double.MaxValue), true);
    public static (Interval Ratios, bool OpenMinimum) UpToSemicircle => (new Interval(PositiveRise.Ratios.T0, 0.5), PositiveRise.OpenMinimum);
    public static (Interval Ratios, bool OpenMinimum) FromSemicircle => (new Interval(UpToSemicircle.Ratios.T1, PositiveRise.Ratios.T1), false);
    public static (Interval Ratios, bool OpenMinimum) LancetRise => (new Interval(double.Sqrt(3) / 2, PositiveRise.Ratios.T1), false);
    public static (Interval Ratios, bool OpenMinimum) DepressedGothicRise => (new Interval(UpToSemicircle.Ratios.T1, LancetRise.Ratios.T0), false);
    public static (Interval Ratios, bool OpenMinimum) PointedHorseshoeRise => (new Interval(double.Sqrt(5.0 / 12), PositiveRise.Ratios.T1), false);
    public static (Interval Ratios, bool OpenMinimum) ThreeCenteredOgeeRise => (new Interval(UpToSemicircle.Ratios.T1, (1 + double.Sqrt(2)) / 2), false);

    // --- [CIRCULAR]
    public static IO<Seq<Curve>> Semicircular((Line Baseline, Plane Frame, Tolerances Tolerances) span, Action<IDisposable> retain) =>
        CurveConstruction.Arcs(Seq(new Arc(span.Baseline.From, span.Frame.YAxis, span.Baseline.To)), retain);

    public static IO<Seq<Curve>> Circular((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d apex, Action<IDisposable> retain) =>
        CurveConstruction.Arcs(Seq(new Arc(span.Baseline.From, apex, span.Baseline.To)), retain);

    public static IO<Seq<Curve>> Equilateral((Line Baseline, Plane Frame, Tolerances Tolerances) span, Action<IDisposable> retain) =>
        Symmetric(span.Baseline, span.Frame, Seq(new Arc(span.Frame, span.Baseline.Length, double.Pi / 3)), retain);

    public static IO<Seq<Curve>> Gothic((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Baseline.PointAt(0.5), apex);
        double height = rise.Length;
        double radius = (height * (height / length)) + (length / 4);
        Point3d center = span.Baseline.To - (span.Frame.XAxis * radius);
        return Symmetric(span.Baseline, span.Frame, Seq(new Arc(
            new Circle(new Plane(center, span.Frame.XAxis, rise.Direction), radius),
            2 * double.Atan2(length / 2, height))), retain);
    }

    public static IO<Seq<Curve>> PointedHorseshoe((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d apex, Action<IDisposable> retain) {
        double length = span.Baseline.Length;
        Line rise = new(span.Baseline.PointAt(0.5), apex);
        double height = rise.Length;
        double offset = (height - (5.0 / 12 * length * (length / height))) / 2;
        Point3d center = span.Baseline.PointAt(1.0 / 3) + (rise.UnitTangent * offset);
        Circle circle = new(new Plane(center, span.Frame.XAxis, rise.Direction), center.DistanceTo(span.Baseline.To));
        return Symmetric(span.Baseline, span.Frame, Seq(new Arc(circle, new Interval(-double.Atan2(offset, length * 2 / 3), Vector3d.VectorAngle(span.Frame.XAxis, apex - center)))), retain);
    }

    // --- [THREE_CENTERED]
    public static IO<Seq<Curve>> BasketHandle((Line Baseline, Plane Frame, Tolerances Tolerances) span, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Point3d haunch = span.Baseline.PointAt(0.25);
        Point3d crown = span.Baseline.PointAt(0.5) - (span.Frame.YAxis * (span.Baseline.Length * double.Sqrt(3) / 4));
        return Symmetric(span.Baseline, span.Frame, Seq(
            new Arc(new Circle(span.Frame, haunch, quarter), new Interval(2 * double.Pi / 3, double.Pi)),
            new Arc(new Circle(span.Frame, crown, crown.DistanceTo(haunch) + quarter), new Interval(double.Pi / 2, 2 * double.Pi / 3))), retain);
    }

    public static IO<Seq<Curve>> DepressedThreeCentered((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d apex, Action<IDisposable> retain) {
        Line rise = new(span.Baseline.PointAt(0.5), apex);
        double half = span.Baseline.Length / 2;
        double height = rise.Length;
        double ratio = height / half;
        double diagonal = new Line(span.Baseline.From, apex).Length / half;
        double radius = height / 2 * diagonal * (1 + (ratio / (diagonal + 1)));
        Point3d haunch = span.Baseline.From + (span.Frame.XAxis * radius);
        Point3d crown = rise.From + (rise.UnitTangent * ((radius - half) / ratio));
        double angle = Vector3d.VectorAngle(span.Frame.XAxis, (rise.UnitTangent * half) - (span.Frame.XAxis * height));
        Plane frame = new(rise.From, span.Frame.XAxis, rise.Direction);
        return Symmetric(span.Baseline, span.Frame, Seq(
            new Arc(new Circle(frame, haunch, radius), new Interval(angle, double.Pi)),
            new Arc(new Circle(frame, crown, crown.DistanceTo(apex)), new Interval(double.Pi / 2, angle))), retain);
    }

    // --- [FOUR_CENTERED]
    public static IO<Seq<Curve>> Persian((Line Baseline, Plane Frame, Tolerances Tolerances) span, Action<IDisposable> retain) {
        Point3d triangle = span.Baseline.PointAt(0.5) - (span.Frame.YAxis * (span.Baseline.Length * double.Sqrt(3) / 4));
        Line perpendicular = new(span.Baseline.PointAt(0.25), span.Frame.YAxis);
        return
            from crossing in IO.lift(Intersections.LineLine(new Line(span.Baseline.PointAt(0.75), triangle), perpendicular).ToFin(new Missing(nameof(Intersection.LineLine))))
            from curves in FourCentered(span, perpendicular.PointAt(crossing.B), retain)
            select curves;
    }

    public static IO<Seq<Curve>> Tudor((Line Baseline, Plane Frame, Tolerances Tolerances) span, Action<IDisposable> retain) =>
        FourCentered(span, span.Baseline.PointAt(0.25) - (span.Frame.YAxis * (span.Baseline.Length / 2)), retain);

    public static IO<Seq<Curve>> Keel((Line Baseline, Plane Frame, Tolerances Tolerances) span, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Point3d crown = span.Baseline.PointAt(0.25) + (span.Frame.YAxis * quarter);
        Point3d haunch = span.Baseline.To + (span.Frame.YAxis * quarter);
        return Symmetric(span.Baseline, span.Frame, Seq(
            new Arc(new Circle(new Plane(haunch, span.Frame.XAxis, -span.Frame.YAxis), quarter), new Interval(double.Pi / 2, double.Pi)),
            new Arc(span.Frame, crown, span.Baseline.Length / 2, double.Pi / 3)), retain);
    }

    private static IO<Seq<Curve>> FourCentered((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d crown, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Point3d haunch = span.Baseline.PointAt(0.75);
        double angle = Vector3d.VectorAngle(span.Frame.XAxis, haunch - crown);
        Circle circle = new(span.Frame, crown, crown.DistanceTo(haunch) + quarter);
        return
            from points in IO.lift(Intersections.LineCircle(new Line(span.Baseline.PointAt(0.5), span.Frame.YAxis), circle).Secant.ToFin(new Missing(nameof(Intersection.LineCircle))))
            from curves in Symmetric(span.Baseline, span.Frame, Seq(
                new Arc(span.Frame, haunch, quarter, angle),
                new Arc(circle, new Interval(angle, Vector3d.VectorAngle(span.Frame.XAxis, points.Second.Point - crown)))), retain)
            select curves;
    }

    // --- [CONIC]
    public static IO<Seq<Curve>> Parabolic((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d apex, Action<IDisposable> retain) =>
        Copies.Acquire(() => NurbsCurve.CreateParabolaFromVertex(apex, span.Baseline.From, span.Baseline.To), nameof(NurbsCurve.CreateParabolaFromVertex), retain)
            .Map(static curve => Seq<Curve>(curve));

    public static IO<Seq<Curve>> Elliptical((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d apex, Action<IDisposable> retain) {
        Line rise = new(span.Baseline.PointAt(0.5), apex);
        return CurveConstruction.EllipticalArc(new Ellipse(new Plane(rise.From, -span.Frame.XAxis, rise.Direction), span.Baseline.Length / 2, rise.Length), new Interval(0, double.Pi), retain)
            .Map(static curve => Seq<Curve>(curve));
    }

    // --- [SYMMETRY]
    private static IO<Seq<Curve>> Symmetric(Line baseline, Plane frame, Seq<Arc> arcs, Action<IDisposable> retain) =>
        Transform.Mirror(baseline.PointAt(0.5), frame.XAxis) switch {
            var mirror => CurveConstruction.Arcs(arcs.Bind(arc => Seq(
                new Arc(new Circle(new Plane(mirror * arc.Center, mirror * arc.Plane.XAxis, mirror * arc.Plane.YAxis), arc.Radius), arc.AngleDomain),
                new Arc(new Circle(new Plane(arc.Center, arc.Plane.XAxis, -arc.Plane.YAxis), arc.Radius), new Interval(-arc.EndAngle, -arc.StartAngle)))), retain),
        };
    // --- [MULTIFOIL]
    public static IO<Seq<Curve>> Multifoil((Line Baseline, Plane Frame, Tolerances Tolerances) span, bool pointed, int count, Action<IDisposable> retain) {
        double divisions = 2.0 * ((count / 2) + (count & 1));
        Arc guide = FoilGuide(span.Baseline, span.Frame, pointed, divisions, springing: 0);
        Func<double, Point3d> at = FoilDivisions(guide, divisions);
        Seq<Arc> lobes = Range(1, (count - 1) / 2, 2).ToSeq().Map(index => {
            Point3d start = at(index);
            double parameter = guide.AngleDomain.ParameterAt((index + 1) / divisions);
            Point3d middle = guide.PointAt(parameter);
            Point3d end = at(index + 2);
            Point3d apex = middle + (Vector3d.CrossProduct(guide.TangentAt(parameter), span.Frame.ZAxis) * end.DistanceTo(start) / 2);
            return new Arc(start, apex, end);
        });
        return Symmetric(span.Baseline, span.Frame, FoilSpringing(span.Frame, at(0), at(1)).Cons(lobes) + Seq(FoilCrown(span.Frame, at(divisions), at(divisions - 1))), retain);
    }

    // --- [FIXED_FOIL]
    public static IO<Seq<Curve>> Cinquefoil((Line Baseline, Plane Frame, Tolerances Tolerances) span, bool pointed, Action<IDisposable> retain) {
        int divisions = pointed ? 6 : 5;
        int springing = pointed ? 0 : 1;
        Arc guide = FoilGuide(span.Baseline, span.Frame, pointed, divisions, springing);
        Func<double, Point3d> at = FoilDivisions(guide, divisions);
        Point3d from = at(springing + 1);
        Point3d center = at(springing + 2);
        Point3d to = at(springing + 3);
        Point3d crownEnd = at(3);
        Point3d crownStart = Transform.Mirror(guide.EndPoint, span.Frame.XAxis) * crownEnd;
        Line crownLine = new(crownStart, crownEnd);
        Arc crown = pointed
            ? new Arc(new Plane(crownStart, crownLine.Direction, Vector3d.CrossProduct(span.Frame.ZAxis, crownLine.Direction)), crownLine.Length, double.Pi / 3)
            : FoilCrown(span.Frame, at(divisions), at(divisions - 1));
        return Symmetric(span.Baseline, span.Frame, Seq(
            FoilSpringing(span.Frame, at(springing), at(springing + 1)),
            new Arc(new Circle(span.Frame, center, center.DistanceTo(from)), new Interval(-Vector3d.VectorAngle(span.Frame.XAxis, from - center), Vector3d.VectorAngle(span.Frame.XAxis, to - center))),
            crown), retain);
    }

    public static IO<Seq<Curve>> Trefoil((Line Baseline, Plane Frame, Tolerances Tolerances) span, bool pointed, Action<IDisposable> retain) {
        double quarter = span.Baseline.Length / 4;
        Point3d midpoint = span.Baseline.PointAt(0.5);
        Arc springing = new(span.Frame, span.Baseline.PointAt(0.75), quarter, double.Pi / 2);
        Arc crown = pointed
            ? new Arc(span.Frame, springing.EndPoint - (span.Frame.XAxis * (quarter * 2)), quarter * 2, double.Pi / 3)
            : new Arc(span.Frame, midpoint + (span.Frame.YAxis * quarter), quarter, double.Pi / 2);
        return Symmetric(span.Baseline, span.Frame, Seq(springing, crown), retain);
    }

    // --- [FOIL_GUIDES]
    private static Arc FoilGuide(Line baseline, Plane frame, bool pointed, double divisions, int springing) {
        (double centerFactor, double sweep) = pointed ? (-0.5, double.Pi / 3) : (0, double.Pi / 2);
        double step = sweep / divisions;
        double radius = baseline.Length / 2 / (centerFactor + double.Cos(step * springing) + (2 * double.Sin(step / 2)));
        Point3d center = baseline.PointAt(0.5) + (frame.XAxis * (centerFactor * radius)) - (frame.YAxis * (radius * double.Sin(step * springing)));
        return new Arc(frame, center, radius, sweep);
    }

    private static Func<double, Point3d> FoilDivisions(Arc guide, double divisions) =>
        index => guide.PointAt(guide.AngleDomain.ParameterAt(index / divisions));

    private static Arc FoilSpringing(Plane frame, Point3d center, Point3d through) =>
        new(frame, center, center.DistanceTo(through), Vector3d.VectorAngle(frame.XAxis, through - center));

    private static Arc FoilCrown(Plane frame, Point3d top, Point3d previous) =>
        new(new Circle(frame, top, top.DistanceTo(previous)), new Interval(-Vector3d.VectorAngle(frame.XAxis, previous - top), double.Pi / 2));
    // --- [RAISED_OGEE]
    public static IO<Seq<Curve>> ThreeCenteredOgee((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d apex, Action<IDisposable> retain) {
        Point3d midpoint = span.Baseline.PointAt(0.5);
        Vector3d rise = apex - midpoint;
        Circle springing = new(new Plane(midpoint, span.Frame.XAxis, rise), span.Baseline.Length / 2);
        return rise.Length - springing.Radius <= span.Tolerances.Absolute
            ? Symmetric(span.Baseline, span.Frame, Seq(new Arc(springing, double.Pi / 2)), retain)
            : Ogee(span, apex, springing, retain);
    }

    public static IO<Seq<Curve>> FourCenteredOgee((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d apex, Action<IDisposable> retain) {
        Point3d haunch = span.Baseline.PointAt(2.0 / 3);
        return Ogee(span, apex, new Circle(new Plane(haunch, span.Frame.XAxis, apex - span.Baseline.PointAt(0.5)), span.Baseline.Length / 3), retain);
    }

    private static IO<Seq<Curve>> Ogee((Line Baseline, Plane Frame, Tolerances Tolerances) span, Point3d apex, Circle springing, Action<IDisposable> retain) {
        Line parallel = new(apex, span.Frame.XAxis);
        return
            from points in IO.lift(Intersections.LineCircle(new Line(span.Baseline.To, apex), springing).Secant.ToFin(new Missing(nameof(Intersection.LineCircle))))
            from crossing in IO.lift(Intersections.LineLine(new Line(springing.Center, points.Second.Point), parallel).ToFin(new Missing(nameof(Intersection.LineLine))))
            let crown = parallel.PointAt(crossing.B)
            from curves in Symmetric(span.Baseline, span.Frame, Seq(
                new Arc(springing, Vector3d.VectorAngle(span.Frame.XAxis, points.Second.Point - springing.Center)),
                new Arc(new Circle(new Plane(crown, springing.Plane.XAxis, -springing.Plane.YAxis), crown.DistanceTo(apex)), new Interval(Vector3d.VectorAngle(span.Frame.XAxis, points.Second.Point - crown), double.Pi))), retain)
            select curves;
    }

    // --- [SPANNED_OGEE]
    public static IO<Seq<Curve>> ReverseOgee((Line Baseline, Plane Frame, Tolerances Tolerances) span, Action<IDisposable> retain) {
        const double riseRatio = 19.0 / 61;
        Point3d midpoint = span.Baseline.PointAt(0.5);
        Point3d apex = midpoint + (span.Frame.YAxis * (span.Baseline.Length * riseRatio));
        Line diagonal = new(span.Baseline.To, apex);
        Line endNormal = new(span.Baseline.To, span.Frame.YAxis);
        Line centerLine = new(midpoint, span.Frame.YAxis);
        Vector3d perpendicular = Vector3d.CrossProduct(span.Frame.ZAxis, diagonal.Direction);
        return
            from crossings in IO.lift((
                Intersections.LineLine(endNormal, new Line(diagonal.PointAt(0.25), perpendicular)).ToValidation<Error>(new Missing(nameof(Intersection.LineLine))),
                Intersections.LineLine(centerLine, new Line(diagonal.PointAt(0.75), perpendicular)).ToValidation<Error>(new Missing(nameof(Intersection.LineLine))))
                .Apply(static (haunch, crown) => (Haunch: haunch, Crown: crown)).As().ToFin())
            let haunch = endNormal.PointAt(crossings.Haunch.A)
            let crown = centerLine.PointAt(crossings.Crown.A)
            let diagonalMidpoint = diagonal.PointAt(0.5)
            from curves in Symmetric(span.Baseline, span.Frame, Seq(
                new Arc(new Circle(new Plane(haunch, span.Frame.XAxis, -span.Frame.YAxis), haunch.DistanceTo(span.Baseline.To)), new Interval(double.Pi / 2, double.Pi - Vector3d.VectorAngle(-span.Frame.XAxis, diagonalMidpoint - haunch))),
                new Arc(new Circle(span.Frame, crown, crown.DistanceTo(apex)), new Interval(Vector3d.VectorAngle(span.Frame.XAxis, diagonalMidpoint - crown), double.Pi / 2))), retain)
            select curves;
    }

    public static IO<Seq<Curve>> TentedOgee((Line Baseline, Plane Frame, Tolerances Tolerances) span, Action<IDisposable> retain) {
        double equilateralRise = double.Sqrt(3) / 2;
        double height = span.Baseline.Length * equilateralRise;
        Point3d apex = span.Baseline.PointAt(0.5) + (span.Frame.YAxis * height);
        Point3d haunchEnd = new Line(span.Baseline.To, apex).PointAtLength(span.Baseline.Length / 2);
        Line haunchLine = new(span.Baseline.To, haunchEnd);
        Plane haunchFrame = new(haunchLine.From, haunchLine.Direction, Vector3d.CrossProduct(span.Frame.ZAxis, haunchLine.Direction));
        Point3d haunch = haunchLine.PointAt(0.5) - (haunchFrame.YAxis * (haunchLine.Length * equilateralRise));
        Point3d crown = span.Baseline.To + (span.Frame.YAxis * height);
        Interval sweep = new(2 * double.Pi / 3, double.Pi);
        return Symmetric(span.Baseline, span.Frame, Seq(
            new Arc(new Circle(new Plane(haunch, span.Frame.XAxis, -span.Frame.YAxis), haunch.DistanceTo(span.Baseline.To)), sweep),
            new Arc(new Circle(new Plane(crown, span.Frame.XAxis, -span.Frame.YAxis), crown.DistanceTo(apex)), sweep)), retain);
    }
}
