using Rasm.Rhino.Document;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RailFrame {
    public sealed record Freeform() : RailFrame;

    public sealed record Roadlike(Vector3d Up) : RailFrame;

    public sealed record AlignWithSurface() : RailFrame;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveFit {
    public sealed record AsIs() : CurveFit;

    public sealed record Rebuild(int PointCount) : CurveFit;

    public sealed record Refit(double Tolerance) : CurveFit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepOneMode {
    public sealed record Direct(Option<Point3d> Start, Option<Point3d> End, SweepBlend Blend, SweepMiter Miter, CurveFit Fit, bool RefitRail) : SweepOneMode;

    public sealed record Segmented(Option<Point3d> Start, Option<Point3d> End, SweepBlend Blend, SweepMiter Miter, CurveFit Fit) : SweepOneMode;

    public sealed record Parameterized(Seq<double> Stations, SweepMiter Miter, bool GlobalShapeBlending, CurveFit Fit, double AngleToleranceRadians) : SweepOneMode;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepTwoMode {
    public sealed record Direct(Option<Point3d> Start, Option<Point3d> End, CurveFit Fit, bool PreserveHeight, bool AutoAdjust) : SweepTwoMode;

    public sealed record Parameterized(Seq<double> Rail1Stations, Seq<double> Rail2Stations, CurveFit Fit, bool MaintainHeight, bool AutoAdjust, double AngleToleranceRadians) : SweepTwoMode;

    public sealed record Partitioned(Seq<Point2d> RailParameters) : SweepTwoMode;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DevelopableMethod {
    public sealed record ByDensity(Curve Curve0, Curve Curve1, bool Reverse0, bool Reverse1, int Density) : DevelopableMethod;

    public sealed record ByRulings(NurbsCurve Rail0, NurbsCurve Rail1, Seq<Point2d> Rulings) : DevelopableMethod;
}

public sealed record VariationalPatchResult(Brep Patch, Option<string> Warning, Option<bool> G0Int, Option<bool> G0, Option<bool> G1, Option<bool> G2);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class LoftMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(SweepOneMode.Parameterized.Miter), nameof(SweepOneRail.MiterType), Use = nameof(MiterType))]
    [MapperIgnoreSource(nameof(SweepOneMode.Parameterized.Stations), Justification = "SweepOneRail.PerformSweep station argument")]
    [MapperIgnoreSource(nameof(SweepOneMode.Parameterized.Fit), Justification = "Selects the PerformSweep overload")]
    internal static partial SweepOneRail ToSweeper(SweepOneMode.Parameterized stations, double sweepTolerance, bool closedSweep);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapValue(nameof(SweepTwoRail.UseLegacySweeper), true)]
    [MapperIgnoreSource(nameof(SweepTwoMode.Parameterized.Rail1Stations), Justification = "SweepTwoRail.PerformSweep station argument")]
    [MapperIgnoreSource(nameof(SweepTwoMode.Parameterized.Rail2Stations), Justification = "SweepTwoRail.PerformSweep station argument")]
    [MapperIgnoreSource(nameof(SweepTwoMode.Parameterized.Fit), Justification = "Selects the PerformSweep overload")]
    internal static partial SweepTwoRail ToSweeper(SweepTwoMode.Parameterized stations, double sweepTolerance, bool closedSweep);

    [UserMapping]
    private static int MiterType(SweepMiter miter) => (int)miter;
}

public static class Lofts {
    // --- [LIMITS]
    private const int MinimumRebuildPoints = 2;

    // --- [SWEEPS]
    public static IO<Seq<Brep>> SweepOne(Curve rail, Seq<Curve> shapes, RailFrame frame, bool closed, SweepOneMode mode, double tolerance) =>
        mode.Switch(
            (Rail: rail,
             Shapes: shapes,
             Frame: frame.Switch<(SweepFrame Frame, Vector3d Up)>(
                 freeform: static _ => (SweepFrame.Freeform, Vector3d.Unset),
                 roadlike: static roadlike => (SweepFrame.Roadlike, roadlike.Up),
                 alignWithSurface: static _ => (SweepFrame.AlignWithSurface, Vector3d.Unset)),
             Closed: closed,
             Tolerance: tolerance),
            direct: static (sweep, direct) => GeometryResults.Acquire(
                () => RebuildArguments(direct.Fit).Map(fit => Brep.CreateFromSweep(
                    sweep.Rail, sweep.Shapes, direct.Start.IfNone(Point3d.Unset), direct.End.IfNone(Point3d.Unset), sweep.Frame.Frame, sweep.Frame.Up, sweep.Closed, direct.Blend, direct.Miter, sweep.Tolerance, fit.Rebuild, fit.PointCount, fit.RefitTolerance, direct.RefitRail)),
                nameof(Brep.CreateFromSweep),
                emptyFails: true),
            segmented: static (sweep, segments) => GeometryResults.Acquire(
                () => RebuildArguments(segments.Fit).Map(fit => Brep.CreateFromSweepSegmented(
                    sweep.Rail, sweep.Shapes, segments.Start.IfNone(Point3d.Unset), segments.End.IfNone(Point3d.Unset), sweep.Frame.Frame, sweep.Frame.Up, sweep.Closed, segments.Blend, segments.Miter, sweep.Tolerance, fit.Rebuild, fit.PointCount, fit.RefitTolerance)),
                nameof(Brep.CreateFromSweepSegmented),
                emptyFails: true),
            parameterized: static (sweep, stations) =>
                from ordered in IO.lift(() =>
                    (Invalid.Unless(!sweep.Shapes.IsEmpty, nameof(shapes)), Invalid.Unless(sweep.Frame.Frame != SweepFrame.AlignWithSurface, nameof(frame)), CountMismatch.Unless(sweep.Shapes.Count, stations.Stations.Count, nameof(SweepOneRail.PerformSweep)))
                        .Apply((_, _, _) => toSeq(from station in sweep.Shapes.Zip(stations.Stations) orderby station.Second select station))
                        .As())
                from sweeper in IO.lift(() => {
                    SweepOneRail created = LoftMapper.ToSweeper(stations, sweep.Tolerance, sweep.Closed);
                    created.SetRoadlikeUpDirection(sweep.Frame.Up);
                    return created;
                })
                from swept in stations.Fit.Switch(
                    (Sweeper: sweeper, sweep.Rail, Shapes: ordered.Map(static station => station.First), Stations: ordered.Map(static station => station.Second)),
                    asIs: static (at, _) => GeometryResults.Acquire(() => at.Sweeper.PerformSweep(at.Rail, at.Shapes, at.Stations), nameof(SweepOneRail.PerformSweep), emptyFails: true),
                    rebuild: static (at, rebuild) => GeometryResults.Acquire(
                        () => from points in RebuildPoints(rebuild) select at.Sweeper.PerformSweepRebuild(at.Rail, at.Shapes, at.Stations, points),
                        nameof(SweepOneRail.PerformSweepRebuild),
                        emptyFails: true),
                    refit: static (at, refit) => GeometryResults.Acquire(() => at.Sweeper.PerformSweepRefit(at.Rail, at.Shapes, at.Stations, refit.Tolerance), nameof(SweepOneRail.PerformSweepRefit), emptyFails: true))
                select swept);

    public static IO<Seq<Brep>> SweepTwo(Curve rail1, Curve rail2, Seq<Curve> shapes, bool closed, SweepTwoMode mode, double tolerance) =>
        mode.Switch(
            (Rail1: rail1, Rail2: rail2, Shapes: shapes, Closed: closed, Tolerance: tolerance),
            direct: static (sweep, direct) => GeometryResults.Acquire(
                () => RebuildArguments(direct.Fit).Map(fit => Brep.CreateFromSweep(
                    sweep.Rail1, sweep.Rail2, sweep.Shapes, direct.Start.IfNone(Point3d.Unset), direct.End.IfNone(Point3d.Unset), sweep.Closed, sweep.Tolerance, fit.Rebuild, fit.PointCount, fit.RefitTolerance, direct.PreserveHeight, direct.AutoAdjust)),
                nameof(Brep.CreateFromSweep),
                emptyFails: true),
            parameterized: static (sweep, parameterized) =>
                from counted in IO.lift(() =>
                    (CountMismatch.Unless(sweep.Shapes.Count, parameterized.Rail1Stations.Count, nameof(SweepTwoRail.PerformSweep)), CountMismatch.Unless(sweep.Shapes.Count, parameterized.Rail2Stations.Count, nameof(SweepTwoRail.PerformSweep)))
                        .Apply(static (_, _) => unit)
                        .As())
                from swept in parameterized.Fit.Switch(
                    (Sweeper: LoftMapper.ToSweeper(parameterized, sweep.Tolerance, sweep.Closed), Sweep: sweep, Stations: parameterized),
                    asIs: static (at, _) => GeometryResults.Acquire(
                        () => at.Sweeper.PerformSweep(at.Sweep.Rail1, at.Sweep.Rail2, at.Sweep.Shapes, at.Stations.Rail1Stations, at.Stations.Rail2Stations),
                        nameof(SweepTwoRail.PerformSweep),
                        emptyFails: true),
                    rebuild: static (at, rebuild) => GeometryResults.Acquire(
                        () => from points in RebuildPoints(rebuild) select at.Sweeper.PerformSweepRebuild(at.Sweep.Rail1, at.Sweep.Rail2, at.Sweep.Shapes, at.Stations.Rail1Stations, at.Stations.Rail2Stations, points),
                        nameof(SweepTwoRail.PerformSweepRebuild),
                        emptyFails: true),
                    refit: static (at, refit) => GeometryResults.Acquire(
                        () => at.Sweeper.PerformSweepRefit(at.Sweep.Rail1, at.Sweep.Rail2, at.Sweep.Shapes, at.Stations.Rail1Stations, at.Stations.Rail2Stations, refit.Tolerance),
                        nameof(SweepTwoRail.PerformSweepRefit),
                        emptyFails: true))
                select swept,
            partitioned: static (sweep, parts) => GeometryResults.Acquire(
                () => CountMismatch.Unless(sweep.Shapes.Count, parts.RailParameters.Count, nameof(Brep.CreateFromSweepInParts))
                    .Map(_ => Brep.CreateFromSweepInParts(sweep.Rail1, sweep.Rail2, sweep.Shapes, parts.RailParameters, sweep.Closed, sweep.Tolerance)),
                nameof(Brep.CreateFromSweepInParts),
                emptyFails: true));

    private static Fin<(SweepRebuild Rebuild, int PointCount, double RefitTolerance)> RebuildArguments(CurveFit fit) =>
        fit.Switch<Fin<(SweepRebuild Rebuild, int PointCount, double RefitTolerance)>>(
            asIs: static _ => (SweepRebuild.None, 0, 0.0),
            rebuild: static rebuild => RebuildPoints(rebuild).Map(static points => (SweepRebuild.Rebuild, points, 0.0)),
            refit: static refit => (SweepRebuild.Refit, 0, refit.Tolerance));

    private static Fin<int> RebuildPoints(CurveFit.Rebuild rebuild) =>
        Limits.AtLeast(MinimumRebuildPoints).Check(rebuild.PointCount, nameof(CurveFit.Rebuild.PointCount));

    // --- [LOFTS]
    public static IO<Seq<Brep>> Loft(Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, LoftType kind, bool closed, CurveFit fit, double angleTolerance) =>
        fit.Switch(
            (Shapes: shapes, Start: start.IfNone(Point3d.Unset), End: end.IfNone(Point3d.Unset), Kind: kind, Closed: closed, Angle: angleTolerance),
            asIs: static (loft, _) => GeometryResults.Acquire(() => Brep.CreateFromLoft(loft.Shapes, loft.Start, loft.End, loft.Kind, loft.Closed, loft.Angle), nameof(Brep.CreateFromLoft), emptyFails: true),
            rebuild: static (loft, rebuild) => GeometryResults.Acquire(
                () => RebuildPoints(rebuild).Map(points => Brep.CreateFromLoftRebuild(loft.Shapes, loft.Start, loft.End, loft.Kind, loft.Closed, loft.Angle, points)),
                nameof(Brep.CreateFromLoftRebuild),
                emptyFails: true),
            refit: static (loft, refit) => GeometryResults.Acquire(
                () => Brep.CreateFromLoftRefit(loft.Shapes, loft.Start, loft.End, loft.Kind, loft.Closed, loft.Angle, refit.Tolerance),
                nameof(Brep.CreateFromLoftRefit),
                emptyFails: true));

    public static IO<Seq<Brep>> LoftTangent(Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, BrepTrim startTrim, BrepTrim endTrim, bool startTangent, bool endTangent, LoftType kind, bool closed) =>
        GeometryResults.Acquire(
            () => Brep.CreateFromLoft(shapes, start.IfNone(Point3d.Unset), end.IfNone(Point3d.Unset), startTangent, endTangent, startTrim, endTrim, kind, closed),
            nameof(Brep.CreateFromLoft),
            emptyFails: true);

    public static IO<Seq<Brep>> Developable(DevelopableMethod method) =>
        method.Switch(
            byDensity: static dense => GeometryResults.Acquire(
                () => Brep.CreateDevelopableLoft(dense.Curve0, dense.Curve1, dense.Reverse0, dense.Reverse1, dense.Density),
                nameof(Brep.CreateDevelopableLoft),
                emptyFails: true),
            byRulings: static ruled => GeometryResults.Acquire(
                () => Invalid.Unless(!ruled.Rulings.IsEmpty, nameof(DevelopableMethod.ByRulings.Rulings)).Map(_ => Brep.CreateDevelopableLoft(ruled.Rail0, ruled.Rail1, ruled.Rulings)),
                nameof(Brep.CreateDevelopableLoft),
                emptyFails: true));

    // --- [PATCHES]
    public static IO<Brep> Patch(
        Seq<GeometryBase> geometry,
        Option<Surface> starting,
        int uSpans,
        int vSpans,
        bool trim,
        bool tangency,
        double pointSpacing,
        double flexibility,
        double surfacePull,
        (bool North, bool East, bool South, bool West) fixedEdges,
        double tolerance) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(!geometry.IsEmpty, nameof(geometry)).Map(_ => Brep.CreatePatch(
                geometry,
                starting.ValueUnsafe(),
                uSpans,
                vSpans,
                trim,
                tangency,
                pointSpacing,
                flexibility,
                surfacePull,
                [fixedEdges.North, fixedEdges.East, fixedEdges.South, fixedEdges.West],
                tolerance)),
            nameof(Brep.CreatePatch));

    public static IO<VariationalPatchResult> Variational(Seq<(Curve Curve, Continuity Continuity)> edges, Seq<(Curve Curve, Continuity Continuity)> interior, Seq<Point3d> points, Brep.VariationalPatchSettings settings, bool multiThreading, Option<IProgress<double>> progress, CancellationToken cancel) =>
        from bounded in DisposalOps.Using(
            IO.lift(() => toSeq(Curve.JoinCurves(edges.Map(static edge => edge.Curve), settings.Tolerance))),
            static loops => IO.lift(() => OpenBoundary.Unless(loops.ForAll(static loop => loop.IsClosed), nameof(edges))))
        from answer in IO.lift(() => (
            Patch: Brep.CreateVariationalPatch(
                edges.Map(static edge => new Brep.CurveConstraint(edge.Curve, edge.Continuity)),
                interior.Map(static edge => new Brep.CurveConstraint(edge.Curve, edge.Continuity)),
                points.Map(static point => new Brep.PointConstraint(point)),
                settings,
                multiThreading,
                cancel,
                progress.ValueUnsafe(),
                out Brep.VariationalPatchResult results),
            Results: results))
        from patch in GeometryResults.Acquire(
            () => Optional(answer.Patch).ToFin(Answers.Present(answer.Results.Error).Match<Error>(Some: static reason => new VariationalPatchFailed(reason), None: () => cancel.IsCancellationRequested ? new Canceled() : new Missing(nameof(Brep.CreateVariationalPatch)))),
            nameof(Brep.CreateVariationalPatch))
        select new VariationalPatchResult(patch, Answers.Present(answer.Results.Warning), Optional(answer.Results.G0Int), Optional(answer.Results.G0), Optional(answer.Results.G1), Optional(answer.Results.G2));
}
