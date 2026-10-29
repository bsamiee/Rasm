using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SurfaceJet(Point3d Point, Vector3d Normal, Seq<Seq<Vector3d>> Orders) {
    public Option<Vector3d> Partial(int du, int dv) =>
        du >= 0 && dv >= 0 && du <= Orders.Count - dv && du + dv is > 0 and var order ? Orders[order - 1].At(dv) : None;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurvatureSource {
    public sealed record OnSurface(Surface Surface, Seq<Point2d> At) : CurvatureSource;

    public sealed record OnSubD(SubD SubD, Seq<SubDComponentParameter> At, SubD.ExtraordinaryVertexCurvature Extraordinary) : CurvatureSource;

    public sealed record OnMesh(Mesh Mesh) : CurvatureSource;

    public IO<Seq<Option<SurfaceCurvature>>> Sample(Tolerances tolerances) =>
        Switch(
            tolerances,
            onSurface: static (at, of) => of.Surface is BrepFace face
                ? Sampled(() => of.At.Map(uv => face.IsPointOnFace(uv.X, uv.Y, at.Absolute) is PointFaceRelation.Exterior ? null : Regular(face, uv)))
                : IO.lift(() => of.At.Traverse(uv => SurfaceEvaluation.InDomain(of.Surface, uv)).As()).Bind(points => Sampled(() => points.Map(uv => Regular(of.Surface, uv)))),
            onSubD: static (_, of) => Sampled(() => of.At.Map(parameter => of.SubD.EvaluateCurvature(parameter, of.Extraordinary, out SurfaceCurvature curvature) ? curvature : null)),
            onMesh: static (_, of) => IO.lift(() => of.Mesh.HasPrincipalCurvatures).Bind(held => held
                ? Sampled(() => toSeq(of.Mesh.PrincipalCurvatures))
                : from gaussian in Approximated(of.Mesh, CurvatureStyle.GaussianCurvature)
                  from mean in Approximated(of.Mesh, CurvatureStyle.MeanCurvature)
                  from sampled in Sampled(() => toSeq(gaussian.Zip(mean, static (k, h) => double.IsNaN(k) || double.IsNaN(h) ? null : SurfaceCurvature.CreateFromGaussianAndMeanCurvatures(k, -h))))
                  select sampled));

    private static IO<double[]> Approximated(Mesh mesh, CurvatureStyle style) =>
        IO.lift(() => RhinoFault.Unless(mesh.ComputeCurvatureApproximation((int)style, out double[] values), RhinoReason.Declined, style).Map(_ => values));

    private static IO<Seq<Option<SurfaceCurvature>>> Sampled(Func<Seq<SurfaceCurvature?>> samples) =>
        from slots in IO.lift(() => samples().Map(static sample => Optional(sample)).Strict())
        from owned in Products.Many(() => slots.Somes(), required: false)
        select slots;

    private static SurfaceCurvature? Regular(Surface surface, Point2d uv) =>
        surface.IsAtSingularity(uv.X, uv.Y, exact: false) ? null : surface.CurvatureAt(uv.X, uv.Y);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceEvaluation {
    // --- [SAMPLES]
    public static IO<SurfaceJet> Jet(Surface surface, Point2d uv, int order) =>
        from at in IO.lift(() => InDomain(surface, uv))
        from jet in IO.lift(() => (
                RhinoFault.Unless(surface.Evaluate(at.X, at.Y, order, out Point3d point, out Vector3d[] derivatives), RhinoReason.Declined, at).Map(_ => (Point: point, Partials: derivatives)),
                surface.NormalAt(at.X, at.Y) switch { var normal => RhinoFault.Unless(normal != Vector3d.Zero, RhinoReason.Invalid, normal).Map(_ => normal) })
            .Apply((sampled, unitNormal) => new SurfaceJet(
                sampled.Point,
                unitNormal,
                toSeq(Range(1, order)).Map(k => toSeq(sampled.Partials.Skip((int)((k * (k + 1L) / 2) - 1)).Take(k + 1))).Strict()))
            .As())
        select jet;

    // --- [CURVES]
    public static IO<Seq<Curve>> IsoCurves(Surface surface, SurfaceDirection constant, double parameter) =>
        from t in IO.lift(() => Conversions.Within(constant.Domain(surface), parameter))
        from curves in surface is BrepFace face
            ? Products.Many(() => face.TrimAwareIsoCurve(constant.Across.Index, t), required: false)
            : Products.Of(() => surface.IsoCurve(constant.Across.Index, t)).Map(static curve => Seq(curve))
        select curves;

    // --- [SCANS]
    public static Seq<double> Discontinuities(Tolerances tolerances, Surface surface, SurfaceDirection direction, Continuity kind) =>
        CurveEvaluation.Breaks(tolerances, kind, surface.IsClosed(direction.Index), direction.Domain(surface),
            (read, t0, t1, cosAngleTolerance, curvatureTolerance) =>
                Conversions.Present(surface.GetNextDiscontinuity(direction.Index, read, t0, t1, cosAngleTolerance, curvatureTolerance, out double t), t));

    // --- [DOMAIN]
    internal static Fin<Point2d> InDomain(Surface surface, Point2d uv) =>
        (Conversions.Within(SurfaceDirection.U.Domain(surface), uv.X), Conversions.Within(SurfaceDirection.V.Domain(surface), uv.Y))
            .Apply(static (u, v) => new Point2d(u, v))
            .As();
}
