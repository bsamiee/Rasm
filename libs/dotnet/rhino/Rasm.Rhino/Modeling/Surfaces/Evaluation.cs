using Rasm.Rhino.Document.Units;
using CurvatureAnalysisSettingsState = Rhino.ApplicationSettings.CurvatureAnalysisSettingsState;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
[Union<BrepFace, Surface>(SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SurfaceTarget;

public sealed record SurfaceJet(Point3d Point, Vector3d Normal, Seq<Seq<Vector3d>> Orders) {
    public Option<Vector3d> Partial(int du, int dv) =>
        du >= 0 && dv >= 0 && du <= Orders.Count - dv && du + dv is > 0 and var order ? Orders[order - 1].At(dv) : None;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurvatureSource {
    public sealed record OnSurface(Surface Surface, Seq<Point2d> At) : CurvatureSource;

    public sealed record OnFace(BrepFace Face, Seq<Point2d> At) : CurvatureSource;

    public sealed record OnSubD(SubD SubD, Seq<SubDComponentParameter> At, SubD.ExtraordinaryVertexCurvature Extraordinary) : CurvatureSource;

    public sealed record OnMesh(Mesh Mesh) : CurvatureSource;

    public IO<Seq<Option<SurfaceCurvature>>> Sample(Tolerances tolerances, Action<IDisposable> retain) =>
        Switch(
            (Tolerances: tolerances, Retain: retain),
            onSurface: static (at, of) => IO.lift(() => of.At.Traverse(uv => SurfaceEvaluation.InDomain(of.Surface, uv, nameof(Surface.CurvatureAt))).As())
                .Bind(points => Copies.Acquire(nameof(Surface.CurvatureAt), _ => Answer.Sparse(points.Map(uv => Regular(of.Surface, uv))), at.Retain)),
            onFace: static (at, of) => Copies.Acquire(nameof(BrepFace.CurvatureAt), _ => Answer.Sparse(of.At.Map(uv =>
                of.Face.IsPointOnFace(uv.X, uv.Y, at.Tolerances.Absolute) is PointFaceRelation.Exterior ? null : Regular(of.Face, uv))), at.Retain),
            onSubD: static (at, of) => Copies.Acquire(nameof(SubD.EvaluateCurvature), _ => Answer.Sparse(of.At.Map(parameter =>
                of.SubD.EvaluateCurvature(parameter, of.Extraordinary, out SurfaceCurvature curvature) ? curvature : null)), at.Retain),
            onMesh: static (at, of) => IO.lift(() => of.Mesh.HasPrincipalCurvatures).Bind(held => held
                ? Copies.Acquire(nameof(Mesh.PrincipalCurvatures), _ => Answer.Sparse(of.Mesh.PrincipalCurvatures), at.Retain)
                : Copies.Acquire(nameof(Mesh.ComputeCurvatureApproximation), _ =>
                    of.Mesh.ComputeCurvatureApproximation((int)CurvatureStyle.GaussianCurvature, out double[] gaussian)
                    && of.Mesh.ComputeCurvatureApproximation((int)CurvatureStyle.MeanCurvature, out double[] mean)
                        ? Answer.Sparse(gaussian.Zip(mean, static (k, h) => double.IsNaN(k) || double.IsNaN(h) ? null : SurfaceCurvature.CreateFromGaussianAndMeanCurvatures(k, -h)))
                        : Answer.Accepted(accepted: false).Map(static _ => Seq<Option<SurfaceCurvature>>()), at.Retain)));

    private static SurfaceCurvature? Regular(Surface surface, Point2d uv) =>
        surface.IsAtSingularity(uv.X, uv.Y, exact: false) ? null : surface.CurvatureAt(uv.X, uv.Y);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceEvaluation {
    // --- [SAMPLES]
    public static IO<SurfaceJet> Jet(Surface surface, Point2d uv, int order) =>
        from at in IO.lift(() => InDomain(surface, uv, nameof(Surface.Evaluate)))
        from normal in IO.lift(() => surface.NormalAt(at.X, at.Y))
        from jet in IO.lift(() => (
                Refused.Unless(surface.Evaluate(at.X, at.Y, order, out Point3d point, out Vector3d[] derivatives), (Point: point, Partials: derivatives), nameof(Surface.Evaluate)),
                Refused.UnlessValid(normal != Vector3d.Zero, normal, nameof(Surface.NormalAt)))
            .Apply((sampled, unitNormal) => new SurfaceJet(
                sampled.Point,
                unitNormal,
                toSeq(Range(1, order)).Map(k => toSeq(sampled.Partials.Skip((int)((k * (k + 1L) / 2) - 1)).Take(k + 1))).Strict()))
            .As())
        select jet;

    // --- [PROXIMITY]
    public static IO<Option<Point2d>> ClosestPoint(SurfaceTarget target, Point3d test, Option<SearchDistance> within) =>
        IO.lift(() => target.Switch(
            (Test: test, Within: within),
            brepFace: static (at, face) => face.ClosestPointOnFace(at.Test, out double u, out double v, at.Within.Match(static distance => (double)distance, 0d))
                ? Some(new Point2d(u, v))
                : at.Within.IsSome ? Option<Point2d>.None : Fin.Fail<Option<Point2d>>(new Refused(nameof(BrepFace.ClosestPointOnFace), Refused.Declined)),
            surface: static (at, surface) => surface.ClosestPoint(at.Test, out double u, out double v)
                ? at.Within.Match(
                    Some: distance => surface.PointAt(u, v).DistanceTo(at.Test) <= distance ? Some(new Point2d(u, v)) : Option<Point2d>.None,
                    None: () => Some(new Point2d(u, v)))
                : new Refused(nameof(Surface.ClosestPoint), Refused.Declined)));

    // --- [CURVES]
    public static IO<Seq<Curve>> IsoCurves(SurfaceTarget target, SurfaceDirection constant, double parameter, Action<IDisposable> retain) =>
        target.Switch(
            (Constant: constant, Parameter: parameter, Retain: retain),
            brepFace: static (at, face) => IO.lift(() => Conversions.Within(face.Domain((int)at.Constant), at.Parameter, nameof(BrepFace.TrimAwareIsoCurve)))
                .Bind(t => Copies.Acquire(nameof(BrepFace.TrimAwareIsoCurve), _ => Answer.Many(face.TrimAwareIsoCurve(Varying(at.Constant), t)), at.Retain)),
            surface: static (at, surface) => IO.lift(() => Conversions.Within(surface.Domain((int)at.Constant), at.Parameter, nameof(Surface.IsoCurve)))
                .Bind(t => Copies.Acquire(nameof(Surface.IsoCurve), _ => Answer.One(surface.IsoCurve(Varying(at.Constant), t)).Map(static curve => Seq(curve)), at.Retain)));

    public static IO<Curve> ShortPath(Tolerances tolerances, Surface surface, Point2d start, Point2d end, Action<IDisposable> retain) =>
        IO.lift(() => (InDomain(surface, start, nameof(Surface.ShortPath)), InDomain(surface, end, nameof(Surface.ShortPath))).Apply(static (origin, target) => (Origin: origin, Target: target)).As())
            .Bind(span => Copies.Acquire(nameof(Surface.ShortPath), _ => Answer.One(surface.ShortPath(span.Origin, span.Target, tolerances.Absolute)), retain));

    // --- [ANALYSIS]
    public static IO<Seq<Mesh>> CurvatureAnalysisMesh(Brep brep, CurvatureAnalysisSettingsState state, MeshingParameters meshingParameters, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Brep.CreateCurvatureAnalysisMesh), _ => Answer.Many(Brep.CreateCurvatureAnalysisMesh(brep, state, meshingParameters)), retain);

    // --- [SCANS]
    public static IO<Seq<double>> Discontinuities(Surface surface, SurfaceDirection direction, Continuity continuity, double cosAngleTolerance, double curvatureTolerance) =>
        IO.lift(() => toSeq(LanguageExt.List.unfold(
                surface.Domain((int)direction),
                span => Callbacks.Found(surface.GetNextDiscontinuity((int)direction, continuity, span.T0, span.T1, cosAngleTolerance, curvatureTolerance, out double at), (at, new Interval(at, span.T1)))))
            .Strict());

    // --- [DOMAIN]
    internal static Fin<Point2d> InDomain(Surface surface, Point2d uv, string member) =>
        (Conversions.Within(surface.Domain((int)SurfaceDirection.U), uv.X, member), Conversions.Within(surface.Domain((int)SurfaceDirection.V), uv.Y, member))
            .Apply(static (u, v) => new Point2d(u, v))
            .As();

    private static int Varying(SurfaceDirection constant) => (int)SurfaceDirection.V - (int)constant;
}
