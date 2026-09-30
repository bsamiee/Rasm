using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SurfaceJet(Point3d Point, Vector3d Du, Vector3d Dv, Vector3d Normal, Plane Frame);

public sealed record SurfaceCurvatureState(Point3d Point, Vector3d Normal, double Gaussian, double Mean, double Kappa0, double Kappa1, Vector3d Direction0, Vector3d Direction1, Option<Circle> Osculating0, Option<Circle> Osculating1);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceEvaluation {
    private const int JetOrder = 1;

    public static IO<SurfaceJet> Jet(Surface surface, Point2d uv) =>
        IO.lift(() =>
            from inside in InDomain(surface, uv, nameof(Surface.Evaluate))
            let normal = surface.NormalAt(uv.X, uv.Y)
            from jet in (
                    Refused.Unless(surface.Evaluate(uv.X, uv.Y, JetOrder, out Point3d point, out Vector3d[] derivatives), (Point: point, Derivatives: derivatives), nameof(Surface.Evaluate)),
                    Degenerate.Unless(normal, nameof(Surface.NormalAt)),
                    Refused.Unless(surface.FrameAt(uv.X, uv.Y, out Plane frame), frame, nameof(Surface.FrameAt)))
                .Apply((sampled, _, oriented) => new SurfaceJet(sampled.Point, sampled.Derivatives[0], sampled.Derivatives[1], normal, oriented))
                .As()
            select jet);

    public static IO<SurfaceCurvatureState> Curvature(Surface surface, Point2d uv) =>
        from inside in IO.lift(() => InDomain(surface, uv, nameof(Surface.CurvatureAt)))
        from state in DisposalOps.Using(
            IO.lift(() => Missing.Unless(surface.CurvatureAt(uv.X, uv.Y), nameof(Surface.CurvatureAt))),
            static curvature => IO.lift(() =>
                Degenerate.Unless(curvature.IsSet, nameof(Surface.CurvatureAt))
                    .Map(_ => new SurfaceCurvatureState(curvature.Point, curvature.Normal, curvature.Gaussian, curvature.Mean, curvature.Kappa(0), curvature.Kappa(1), curvature.Direction(0), curvature.Direction(1), Osculating(curvature, 0), Osculating(curvature, 1)))))
        select state;

    public static IO<Seq<Curve>> IsoCurves(Surface surface, IsoStatus side, Option<double> parameter) =>
        from station in IO.lift(() => (side, parameter.Case) switch {
            (IsoStatus.X, double at) => OutOfDomain.Unless(surface.Domain((int)SurfaceDirection.U), at, nameof(Surface.IsoCurve)).Map(_ => (Direction: SurfaceDirection.V, Parameter: at)),
            (IsoStatus.Y, double at) => OutOfDomain.Unless(surface.Domain((int)SurfaceDirection.V), at, nameof(Surface.IsoCurve)).Map(_ => (Direction: SurfaceDirection.U, Parameter: at)),
            (IsoStatus.West, null) => Fin.Succ((Direction: SurfaceDirection.V, Parameter: surface.Domain((int)SurfaceDirection.U).T0)),
            (IsoStatus.East, null) => Fin.Succ((Direction: SurfaceDirection.V, Parameter: surface.Domain((int)SurfaceDirection.U).T1)),
            (IsoStatus.South, null) => Fin.Succ((Direction: SurfaceDirection.U, Parameter: surface.Domain((int)SurfaceDirection.V).T0)),
            (IsoStatus.North, null) => Fin.Succ((Direction: SurfaceDirection.U, Parameter: surface.Domain((int)SurfaceDirection.V).T1)),
            _ => Fin.Fail<(SurfaceDirection Direction, double Parameter)>(new Invalid(nameof(IsoStatus))),
        })
        from curves in surface is BrepFace face
            ? GeometryResults.Acquire(() => face.TrimAwareIsoCurve((int)station.Direction, station.Parameter), nameof(BrepFace.TrimAwareIsoCurve), emptyFails: false)
            : GeometryResults.Acquire(() => surface.IsoCurve((int)station.Direction, station.Parameter), nameof(Surface.IsoCurve)).Map(static curve => Seq(curve))
        select curves;

    public static IO<Curve> ShortPath(Surface surface, Point2d start, Point2d end, double tolerance) =>
        GeometryResults.Acquire(() => surface.ShortPath(start, end, tolerance), nameof(Surface.ShortPath));

    private static Option<Circle> Osculating(SurfaceCurvature curvature, int direction) =>
        Some(curvature.OsculatingCircle(direction)).Filter(static circle => circle.IsValid);

    private static Fin<Unit> InDomain(Surface surface, Point2d uv, string member) =>
        (OutOfDomain.Unless(surface.Domain((int)SurfaceDirection.U), uv.X, member), OutOfDomain.Unless(surface.Domain((int)SurfaceDirection.V), uv.Y, member)).Apply(static (_, _) => unit).As();
}
