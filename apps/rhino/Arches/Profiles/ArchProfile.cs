using Rasm.Rhino.Document;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Modeling.Curves;
using Rhino;
using Rhino.Geometry.Intersect;

namespace Arches.Profiles;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Span {
    private readonly Plane frame;

    private Span(Point3d start, Point3d end, Plane frame) => (Start, End, this.frame) = (start, end, frame);

    public Point3d Start { get; }

    public Point3d End { get; }

    public Vector3d Normal => frame.ZAxis;

    public double Length => Start.DistanceTo(End);

    public Vector3d Direction => frame.XAxis;

    public Point3d Midpoint => (Start + End) / 2;

    public Vector3d Perpendicular => frame.YAxis;

    public Line CenterLine => new(Midpoint - (Perpendicular * Length), Midpoint + (Perpendicular * Length));

    public double HalfSpan => Length / 2;

    public double QuarterSpan => Length / 4;

    public double EquilateralHeight => Length * (Math.Sqrt(3) / 2);

    public double RiseTolerance => Length * RhinoMath.SqrtEpsilon;

    public static Fin<Span> From(Point3d start, Point3d end, Vector3d normal) =>
        new Plane(start, end - start, Vector3d.CrossProduct(normal, end - start)) is { IsValid: true } plane ? new Span(start, end, plane) : new Degenerate(nameof(Span));

    public Circle CircleAt(Point3d center, double radius) => new(new Plane(center, Direction, Perpendicular), radius);

    public Point3d Raised(Point3d candidate, Limits<double> limits) {
        Point3d pulled = CenterLine.ClosestPoint(candidate, limitToFiniteSegment: false);
        Vector3d side = ((pulled - Midpoint) * Perpendicular) < 0 ? -Perpendicular : Perpendicular;
        return Midpoint + (side * limits.Clamp(pulled.DistanceTo(Midpoint), RiseTolerance));
    }
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
        Transform mirror = Transform.Mirror(new Plane(span.Midpoint, span.Direction));
        return towardStart
            .Traverse(arc => Image(arc, mirror).Map(image => Seq(image, Reversed(arc))))
            .As()
            .Map<ArchProfile>(pairs => new Arcs(span, pairs.Flatten()));
    }

    public static Fin<LineCircleCrossing.Multiple> Secant(Line line, Circle circle) =>
        CurveConstruction.LineCircle(line, circle).Bind(static crossing => crossing.Switch(
            single: static _ => Fin.Fail<LineCircleCrossing.Multiple>(new Missing(nameof(Intersection.LineCircle))),
            multiple: static both => Fin.Succ(both)));

    public IO<Seq<Curve>> Joined(double tolerance) =>
        Switch(
            tolerance,
            arcs: static (within, arcs) => Disposal.Using(
                IO.lift(() => arcs.Parts.Map<Curve>(static arc => arc.ToNurbsCurve()).Strict()),
                inputs => CurveConstruction.Join(inputs, within, preserveDirection: true, simpleJoin: false).Map(static joined => joined.Curves)),
            parabolic: static (_, parabolic) => parabolic.ToCurve().Map(static curve => Seq(curve)),
            elliptical: static (_, elliptical) => elliptical.ToCurve().Map(static curve => Seq(curve)));

    private static Fin<Arc> Image(Arc arc, Transform mirror) =>
        !arc.IsValid ? new Invalid(nameof(Arc)) : Refused.Unless(arc.Transform(mirror), arc, nameof(Arc.Transform));

    private static Arc Reversed(Arc arc) {
        arc.Reverse();
        return arc;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RiseLimits {
    public static Fin<Limits<double>> UpToSemicircle(Span span) => Limits.Above(0.0).AtMost(span.HalfSpan, nameof(UpToSemicircle));

    public static Fin<Limits<double>> FromSemicircle(Span span) => Limits.AtLeast(span.HalfSpan);
}
