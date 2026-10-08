using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SurfaceJet(Point3d Point, Vector3d Normal, Seq<Seq<Vector3d>> Orders) {
    public Option<Vector3d> Partial(int du, int dv) =>
        du >= 0 && dv >= 0 && du <= Orders.Count - dv && du + dv is > 0 and var order ? Orders[order - 1].At(dv) : None;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceEvaluation {
    // --- [SAMPLES]
    public static IO<SurfaceJet> Jet(Surface surface, Point2d uv, int order) =>
        from at in IO.lift(() => InDomain(surface, uv, nameof(Surface.Evaluate)).ToFin())
        from normal in IO.lift(() => surface.NormalAt(at.X, at.Y))
        from jet in IO.lift(() => (
                Refused.Unless(surface.Evaluate(at.X, at.Y, order, out Point3d point, out Vector3d[] derivatives), (Point: point, Partials: derivatives), nameof(Surface.Evaluate)).ToValidation(),
                InvalidAnswer.Unless(normal != Vector3d.Zero, normal, nameof(Surface.NormalAt)).ToValidation())
            .Apply((sampled, unitNormal) => new SurfaceJet(
                sampled.Point,
                unitNormal,
                toSeq(Range(1, order)).Map(k => toSeq(sampled.Partials.Skip((int)((k * (k + 1L) / 2) - 1)).Take(k + 1))).Strict()))
            .As().ToFin())
        select jet;

    public static IO<SurfaceCurvature> Curvature(Surface surface, Point2d uv) =>
        from at in IO.lift(() => InDomain(surface, uv, nameof(Surface.CurvatureAt)).ToFin())
        from curvature in Copies.Owned(
            IO.lift(Fin<SurfaceCurvature> () =>
                surface.IsAtSingularity(at.X, at.Y, exact: false)
                    ? new Degenerate(nameof(Surface.CurvatureAt))
                    : Missing.Unless(surface.CurvatureAt(at.X, at.Y), nameof(Surface.CurvatureAt))),
            static answer => InvalidAnswer.Unless(answer.IsSet, answer, nameof(Surface.CurvatureAt)))
        select curvature;

    // --- [CURVES]
    public static IO<Seq<Curve>> IsoCurves(BrepFace face, SurfaceDirection constant, double parameter) =>
        Copies.Acquire(
            () => OutOfDomain.Unless(face.Domain((int)constant), parameter, nameof(BrepFace.TrimAwareIsoCurve)).Map(at => face.TrimAwareIsoCurve(Varying(constant), at)),
            nameof(BrepFace.TrimAwareIsoCurve));

    public static IO<Seq<Curve>> IsoCurves(Surface surface, SurfaceDirection constant, double parameter) =>
        Copies.Acquire(
            () => OutOfDomain.Unless(surface.Domain((int)constant), parameter, nameof(Surface.IsoCurve)).Map(at => surface.IsoCurve(Varying(constant), at)),
            nameof(Surface.IsoCurve)).Map(static curve => Seq(curve));

    public static IO<Curve> ShortPath(Surface surface, Point2d start, Point2d end, Tolerances tolerances) =>
        Copies.Acquire(
            () => (InDomain(surface, start, nameof(Surface.ShortPath)), InDomain(surface, end, nameof(Surface.ShortPath)))
                .Apply((origin, target) => surface.ShortPath(origin, target, tolerances.Absolute))
                .As()
                .ToFin(),
            nameof(Surface.ShortPath));

    // --- [SCANS]
    public static IO<Seq<double>> Discontinuities(Surface surface, SurfaceDirection direction, Continuity continuity, double cosAngleTolerance, double curvatureTolerance) =>
        IO.lift(() => toSeq(LanguageExt.List.unfold(
                surface.Domain((int)direction),
                span => Callbacks.Found(surface.GetNextDiscontinuity((int)direction, continuity, span.T0, span.T1, cosAngleTolerance, curvatureTolerance, out double at), (at, new Interval(at, span.T1)))))
            .Strict());

    // --- [DOMAIN]
    private static Validation<Error, Point2d> InDomain(Surface surface, Point2d uv, string member) =>
        (OutOfDomain.Unless(surface.Domain((int)SurfaceDirection.U), uv.X, member).ToValidation(), OutOfDomain.Unless(surface.Domain((int)SurfaceDirection.V), uv.Y, member).ToValidation())
            .Apply(static (u, v) => new Point2d(u, v))
            .As();

    private static int Varying(SurfaceDirection constant) => (int)SurfaceDirection.V - (int)constant;
}
