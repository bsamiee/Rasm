using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SurfaceJet(Point3d Point, Vector3d Du, Vector3d Dv, Vector3d Normal, Plane Frame);

public sealed record SurfaceCurvatureState(Point3d Point, Vector3d Normal, double Gaussian, double Mean, double Kappa0, double Kappa1, Vector3d Direction0, Vector3d Direction1, Circle Osculating0, Circle Osculating1);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceEvaluation {
    private const int JetOrder = 1;

    public static IO<SurfaceJet> Jet(Surface surface, Point2d uv) =>
        from inside in IO.lift(() => InDomain(surface, uv, nameof(Surface.Evaluate)))
        from jet in IO.lift(() =>
            from sampled in Refused.Unless(surface.Evaluate(uv.X, uv.Y, JetOrder, out Point3d point, out Vector3d[] derivatives), nameof(Surface.Evaluate))
            let normal = surface.NormalAt(uv.X, uv.Y)
            from sound in Degenerate.Unless(normal.IsValid, nameof(Surface.NormalAt))
            from oriented in Refused.Unless(surface.FrameAt(uv.X, uv.Y, out Plane frame), frame, nameof(Surface.FrameAt))
            select new SurfaceJet(point, derivatives[0], derivatives[1], normal, oriented))
        select jet;

    public static IO<SurfaceCurvatureState> Curvature(Surface surface, Point2d uv) =>
        from inside in IO.lift(() => InDomain(surface, uv, nameof(Surface.CurvatureAt)))
        from state in Disposal.Using(
            IO.lift(() => Missing.Unless(surface.CurvatureAt(uv.X, uv.Y), nameof(Surface.CurvatureAt))),
            static curvature => IO.lift(() =>
                from set in Invalid.Unless(curvature.IsSet, nameof(SurfaceCurvature.IsSet))
                let circles = (Major: curvature.OsculatingCircle(0), Minor: curvature.OsculatingCircle(1))
                from sound in Degenerate.Unless(circles.Major.IsValid && circles.Minor.IsValid, nameof(SurfaceCurvature.OsculatingCircle))
                select new SurfaceCurvatureState(curvature.Point, curvature.Normal, curvature.Gaussian, curvature.Mean, curvature.Kappa(0), curvature.Kappa(1), curvature.Direction(0), curvature.Direction(1), circles.Major, circles.Minor)))
        select state;

    public static IO<Seq<Curve>> IsoCurves(Surface surface, IsoStatus side, Option<double> parameter) =>
        from station in IO.lift(() => (side, parameter.Case) switch {
            (IsoStatus.X, double at) => OutOfDomain.Unless(surface.Domain(SurfaceConstruction.AlongU), at, nameof(Surface.IsoCurve)).Map(_ => (Direction: SurfaceConstruction.AlongV, Parameter: at)),
            (IsoStatus.Y, double at) => OutOfDomain.Unless(surface.Domain(SurfaceConstruction.AlongV), at, nameof(Surface.IsoCurve)).Map(_ => (Direction: SurfaceConstruction.AlongU, Parameter: at)),
            (IsoStatus.West, null) => Fin.Succ((Direction: SurfaceConstruction.AlongV, Parameter: surface.Domain(SurfaceConstruction.AlongU).T0)),
            (IsoStatus.East, null) => Fin.Succ((Direction: SurfaceConstruction.AlongV, Parameter: surface.Domain(SurfaceConstruction.AlongU).T1)),
            (IsoStatus.South, null) => Fin.Succ((Direction: SurfaceConstruction.AlongU, Parameter: surface.Domain(SurfaceConstruction.AlongV).T0)),
            (IsoStatus.North, null) => Fin.Succ((Direction: SurfaceConstruction.AlongU, Parameter: surface.Domain(SurfaceConstruction.AlongV).T1)),
            _ => Fin.Fail<(int Direction, double Parameter)>(new Invalid(nameof(IsoStatus))),
        })
        from curves in surface is BrepFace face
            ? GeometryResults.Acquire(() => face.TrimAwareIsoCurve(station.Direction, station.Parameter), nameof(BrepFace.TrimAwareIsoCurve))
            : IO.lift(() => Missing.Unless(surface.IsoCurve(station.Direction, station.Parameter), nameof(Surface.IsoCurve)).Map(static curve => Seq(curve)))
        select curves;

    public static IO<Curve> ShortPath(Surface surface, Point2d start, Point2d end, double tolerance) =>
        from valid in IO.lift(() =>
            from defined in Invalid.Unless(start.IsValid && end.IsValid, nameof(Point2d.IsValid))
            from startInside in InDomain(surface, start, nameof(Surface.ShortPath))
            from endInside in InDomain(surface, end, nameof(Surface.ShortPath))
            from distinct in Degenerate.Unless(start != end, nameof(Surface.ShortPath))
            select unit)
        from path in IO.lift(() => Missing.Unless(surface.ShortPath(start, end, tolerance), nameof(Surface.ShortPath)))
        select path;

    private static Fin<Unit> InDomain(Surface surface, Point2d uv, string member) =>
        from u in OutOfDomain.Unless(surface.Domain(SurfaceConstruction.AlongU), uv.X, member)
        from v in OutOfDomain.Unless(surface.Domain(SurfaceConstruction.AlongV), uv.Y, member)
        select unit;
}
