using Rasm.Rhino.Document.Units;
using Rhino.ApplicationSettings;

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

    public IO<Seq<Option<SurfaceCurvature>>> Sample(Tolerances tolerances) =>
        Switch(
            tolerances,
            onSurface: static (_, of) => IO.lift(() => of.At.Traverse(uv => SurfaceEvaluation.InDomain(of.Surface, uv)).As())
                .Bind(points => Sampled(points.Map(uv => Regular(of.Surface, uv)))),
            onFace: static (at, of) => Sampled(of.At.Map(uv => of.Face.IsPointOnFace(uv.X, uv.Y, at.Absolute) is PointFaceRelation.Exterior ? null : Regular(of.Face, uv))),
            onSubD: static (_, of) => Sampled(of.At.Map(parameter => of.SubD.EvaluateCurvature(parameter, of.Extraordinary, out SurfaceCurvature curvature) ? curvature : null)),
            onMesh: static (_, of) => IO.lift(() => of.Mesh.HasPrincipalCurvatures).Bind(held => held
                ? Sampled(toSeq(of.Mesh.PrincipalCurvatures))
                : from gaussian in IO.lift(() => RhinoFault.Unless(of.Mesh.ComputeCurvatureApproximation((int)CurvatureStyle.GaussianCurvature, out double[] values)).Map(_ => values))
                  from mean in IO.lift(() => RhinoFault.Unless(of.Mesh.ComputeCurvatureApproximation((int)CurvatureStyle.MeanCurvature, out double[] values)).Map(_ => values))
                  from sampled in Sampled(toSeq(gaussian.Zip(mean, static (k, h) => double.IsNaN(k) || double.IsNaN(h) ? null : SurfaceCurvature.CreateFromGaussianAndMeanCurvatures(k, -h))))
                  select sampled));

    private static IO<Seq<Option<SurfaceCurvature>>> Sampled(Seq<SurfaceCurvature?> samples) =>
        samples.Map(static sample => Optional(sample)).Strict() switch {
            var slots => Products.Made(slots.Somes()).Map(_ => slots),
        };

    private static SurfaceCurvature? Regular(Surface surface, Point2d uv) =>
        surface.IsAtSingularity(uv.X, uv.Y, exact: false) ? null : surface.CurvatureAt(uv.X, uv.Y);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceEvaluation {
    // --- [SAMPLES]
    public static IO<SurfaceJet> Jet(Surface surface, Point2d uv, int order) =>
        from at in IO.lift(() => InDomain(surface, uv))
        from normal in IO.lift(() => surface.NormalAt(at.X, at.Y))
        from jet in IO.lift(() => (
                RhinoFault.Unless(surface.Evaluate(at.X, at.Y, order, out Point3d point, out Vector3d[] derivatives)).Map(_ => (Point: point, Partials: derivatives)),
                RhinoFault.Unless(normal != Vector3d.Zero).Map(_ => normal))
            .Apply((sampled, unitNormal) => new SurfaceJet(
                sampled.Point,
                unitNormal,
                toSeq(Range(1, order)).Map(k => toSeq(sampled.Partials.Skip((int)((k * (k + 1L) / 2) - 1)).Take(k + 1))).Strict()))
            .As())
        select jet;

    // --- [PROXIMITY]
    public static IO<Option<Point2d>> ClosestPoint(SurfaceTarget target, Point3d test, Option<double> within) =>
        IO.lift(() => target.Switch(
            (Test: test, Within: within),
            brepFace: static (at, face) => RhinoFault.Unless(face.ClosestPointOnFace(at.Test, out double u, out double v, at.Within.IfNone(0d))).Map(_ => Some(new Point2d(u, v)))
                .BindFail(error => at.Within.IsSome ? Option<Point2d>.None : error),
            surface: static (at, surface) => RhinoFault.Unless(surface.ClosestPoint(at.Test, out double u, out double v)).Map(_ => at.Within.Match(
                Some: distance => surface.PointAt(u, v).DistanceTo(at.Test) <= distance ? Some(new Point2d(u, v)) : Option<Point2d>.None,
                None: () => Some(new Point2d(u, v))))));

    // --- [CURVES]
    public static IO<Seq<Curve>> IsoCurves(SurfaceTarget target, SurfaceDirection constant, double parameter) =>
        target.Switch(
            (Constant: constant, Parameter: parameter),
            brepFace: static (at, face) => IO.lift(() => Conversions.Within(face.Domain((int)at.Constant), at.Parameter))
                .Bind(t => Products.Made(face.TrimAwareIsoCurve(Varying(at.Constant), t))),
            surface: static (at, surface) =>
                from t in IO.lift(() => Conversions.Within(surface.Domain((int)at.Constant), at.Parameter))
                from curve in Products.Made(surface.IsoCurve(Varying(at.Constant), t))
                select Seq(curve));

    public static IO<Curve> ShortPath(Tolerances tolerances, Surface surface, Point2d start, Point2d end) =>
        IO.lift(() => (InDomain(surface, start), InDomain(surface, end)).Apply(static (origin, target) => (Origin: origin, Target: target)).As())
            .Bind(span => Products.Made(surface.ShortPath(span.Origin, span.Target, tolerances.Absolute)));

    // --- [ANALYSIS]
    public static IO<Seq<Mesh>> CurvatureAnalysisMesh(Brep brep, CurvatureAnalysisSettingsState state, MeshingParameters meshingParameters) =>
        Products.Made(Brep.CreateCurvatureAnalysisMesh(brep, state, meshingParameters));

    // --- [SCANS]
    public static IO<Seq<double>> Discontinuities(Surface surface, SurfaceDirection direction, Continuity continuity, double cosAngleTolerance, double curvatureTolerance) =>
        IO.lift(() => toSeq(LanguageExt.List.unfold(
                surface.Domain((int)direction),
                span => Conversions.Found(surface.GetNextDiscontinuity((int)direction, continuity, span.T0, span.T1, cosAngleTolerance, curvatureTolerance, out double at), (at, new Interval(at, span.T1)))))
            .Strict());

    // --- [DOMAIN]
    internal static Fin<Point2d> InDomain(Surface surface, Point2d uv) =>
        (Conversions.Within(surface.Domain((int)SurfaceDirection.U), uv.X), Conversions.Within(surface.Domain((int)SurfaceDirection.V), uv.Y))
            .Apply(static (u, v) => new Point2d(u, v))
            .As();

    private static int Varying(SurfaceDirection constant) => (int)SurfaceDirection.V - (int)constant;
}
