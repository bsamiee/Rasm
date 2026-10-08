namespace Arches.Profiles;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Span {
    private Span(Point3d start, Point3d end, Plane frame, double tolerance) => (Start, End, Frame, Tolerance) = (start, end, frame, tolerance);

    public Point3d Start { get; }

    public Point3d End { get; }

    public Plane Frame { get; }

    public double Tolerance { get; }

    public double Length => Start.DistanceTo(End);

    public Point3d Midpoint => (Start + End) / 2;

    public Line CenterLine => new(Midpoint - (Frame.YAxis * Length), Midpoint + (Frame.YAxis * Length));

    public double HalfSpan => Length / 2;

    public double QuarterSpan => Length / 4;

    public double EquilateralHeight => Length * (Math.Sqrt(3) / 2);

    public static Fin<Span> From(Point3d start, Point3d end, Vector3d normal, double tolerance) =>
        new Plane(start, end - start, Vector3d.CrossProduct(normal, end - start)) is { IsValid: true } plane ? new Span(start, end, plane, tolerance) : new Degenerate(nameof(Span));

    public Circle CircleAt(Point3d center, double radius) => new(new Plane(center, Frame.XAxis, Frame.YAxis), radius);

    public Point3d Raised(Point3d candidate, Limits<double> limits) {
        Point3d pulled = CenterLine.ClosestPoint(candidate, limitToFiniteSegment: false);
        Vector3d side = ((pulled - Midpoint) * Frame.YAxis) < 0 ? -Frame.YAxis : Frame.YAxis;
        return Midpoint + (side * Clamped(limits, pulled.DistanceTo(Midpoint), Tolerance));
    }

    private static double Clamped(Limits<double> limits, double value, double inset) =>
        limits.Upper.Fold(
            limits.Lower.Fold(value, (low, bound) => Math.Max(low, bound.Exclusive ? bound.Value + inset : bound.Value)),
            (high, bound) => Math.Min(high, bound.Exclusive ? bound.Value - inset : bound.Value));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchProfile {
    public abstract Span Span { get; init; }

    public sealed record Arcs(Span Span, Seq<Arc> Parts) : ArchProfile;

    public sealed record Parabolic(Span Span, Point3d Vertex) : ArchProfile {
        public IO<Curve> ToCurve() => CurveConstruction.Parabola(new ParabolaSource.FromVertex(Vertex, Span.Start, Span.End)).Map<Curve>(static curve => curve);
    }

    public sealed record Elliptical(Span Span, Ellipse Ellipse) : ArchProfile {
        public IO<Curve> ToCurve() =>
            CurveConstruction.Analytic(new AnalyticCurve.OfEllipse(Ellipse, Some(new Interval(0.0, Math.PI)))).Map<Curve>(static curve => curve);
    }

    public static Fin<ArchProfile> Mirrored(Span span, Seq<Arc> towardStart) {
        Transform mirror = Transform.Mirror(new Plane(span.Midpoint, span.Frame.XAxis));
        return towardStart
            .Traverse(arc => Image(arc, mirror).Map(image => Seq(image, Reversed(arc))))
            .As()
            .Map<ArchProfile>(pairs => new Arcs(span, pairs.Flatten()));
    }

    public IO<Seq<Curve>> Joined(double tolerance) =>
        Switch(
            tolerance,
            arcs: static (within, arcs) => DisposalOps.Using(
                IO.lift(() => arcs.Parts.Map<Curve>(static arc => arc.ToNurbsCurve()).Strict()),
                inputs => CurveConstruction.Join(inputs, within, preserveDirection: true, simpleJoin: false).Map(static joined => joined.Curves)),
            parabolic: static (_, parabolic) => parabolic.ToCurve().Map(static curve => Seq(curve)),
            elliptical: static (_, elliptical) => elliptical.ToCurve().Map(static curve => Seq(curve)));

    private static Fin<Arc> Image(Arc arc, Transform mirror) {
        Arc image = arc;
        bool mirrored = image.Transform(mirror);
        return (Invalid.Unless(arc.IsValid, nameof(arc)), Refused.Unless(mirrored, image, nameof(Arc.Transform))).Apply(static (_, transformed) => transformed).As();
    }

    private static Arc Reversed(Arc arc) {
        arc.Reverse();
        return arc;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RiseLimits {
    public static Limits<double> UpToSemicircle(Span span) => Limits.Above(0.0).AtMost(span.HalfSpan);

    public static Limits<double> FromSemicircle(Span span) => Limits.AtLeast(span.HalfSpan);

    public static Limits<double> Positive(Span _) => Limits.Above(0.0);
}
