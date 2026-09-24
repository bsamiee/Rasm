using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RailFrame {
    public sealed record Freeform() : RailFrame;

    public sealed record Roadlike(Vector3d Up) : RailFrame;
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

    public sealed record Parameterized(Seq<double> Rail1Stations, Seq<double> Rail2Stations, CurveFit Fit, bool MaintainHeight, bool AutoAdjust, bool UseLegacySweeper, double AngleToleranceRadians) : SweepTwoMode;

    public sealed record Partitioned(Seq<Point2d> RailParameters) : SweepTwoMode;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DevelopableMethod {
    public sealed record ByDensity(Curve Curve0, Curve Curve1, bool Reverse0, bool Reverse1, int Density) : DevelopableMethod;

    public sealed record ByRulings(NurbsCurve Rail0, NurbsCurve Rail1, Seq<Point2d> Rulings) : DevelopableMethod;
}

public sealed record PatchOptions(int USpans, int VSpans, bool Trim, bool Tangency, double PointSpacing, double Flexibility, double SurfacePull, bool FixNorth, bool FixEast, bool FixSouth, bool FixWest);

public sealed record VariationalPatchResult(Brep Patch, Option<string> Warning, Option<bool> G0Int, Option<bool> G0, Option<bool> G1, Option<bool> G2);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lofts {
    // --- [LIMITS]
    private const int MinimumShapes = 2;

    private const int MinimumRebuildPoints = 2;

    // --- [FRAMES]
    internal static (SweepFrame Frame, Vector3d Up) FrameOf(RailFrame frame) =>
        frame.Switch<(SweepFrame Frame, Vector3d Up)>(
            freeform: static _ => (SweepFrame.Freeform, Vector3d.Unset),
            roadlike: static roadlike => (SweepFrame.Roadlike, roadlike.Up));

    // --- [SWEEPS]
    public static IO<Seq<Brep>> SweepOne(Curve rail, Seq<Curve> shapes, RailFrame frame, bool closed, SweepOneMode mode, double tolerance) =>
        from filled in IO.lift(() => Invalid.Unless(!shapes.IsEmpty, nameof(shapes)))
        from breps in mode.Switch(
            (Rail: rail, Shapes: shapes, Frame: FrameOf(frame), Closed: closed, Tolerance: tolerance),
            direct: static (sweep, direct) =>
                from fit in IO.lift(() => RebuildArguments(direct.Fit))
                from swept in GeometryResults.Acquire(() => Brep.CreateFromSweep(sweep.Rail, sweep.Shapes, direct.Start.IfNone(Point3d.Unset), direct.End.IfNone(Point3d.Unset), sweep.Frame.Frame, sweep.Frame.Up, sweep.Closed, direct.Blend, direct.Miter, sweep.Tolerance, fit.Rebuild, fit.PointCount, fit.RefitTolerance, direct.RefitRail), nameof(Brep.CreateFromSweep))
                select swept,
            segmented: static (sweep, segments) =>
                from fit in IO.lift(() => RebuildArguments(segments.Fit))
                from swept in GeometryResults.Acquire(() => Brep.CreateFromSweepSegmented(sweep.Rail, sweep.Shapes, segments.Start.IfNone(Point3d.Unset), segments.End.IfNone(Point3d.Unset), sweep.Frame.Frame, sweep.Frame.Up, sweep.Closed, segments.Blend, segments.Miter, sweep.Tolerance, fit.Rebuild, fit.PointCount, fit.RefitTolerance), nameof(Brep.CreateFromSweepSegmented))
                select swept,
            parameterized: static (sweep, stations) =>
                from counted in IO.lift(() => CountMismatch.Unless(sweep.Shapes.Count, stations.Stations.Count, nameof(SweepOneRail.PerformSweep)))
                from sweeper in IO.lift(() => {
                    SweepOneRail created = new() { SweepTolerance = sweep.Tolerance, AngleToleranceRadians = stations.AngleToleranceRadians, MiterType = (int)stations.Miter, ClosedSweep = sweep.Closed, GlobalShapeBlending = stations.GlobalShapeBlending };
                    if (sweep.Frame.Frame == SweepFrame.Roadlike)
                        created.SetRoadlikeUpDirection(sweep.Frame.Up);
                    return created;
                })
                from swept in stations.Fit.Switch(
                    (Sweeper: sweeper, sweep.Rail, sweep.Shapes, stations.Stations),
                    asIs: static (run, _) => GeometryResults.Acquire(() => run.Sweeper.PerformSweep(run.Rail, run.Shapes, run.Stations), nameof(SweepOneRail.PerformSweep)),
                    rebuild: static (run, rebuild) =>
                        from points in IO.lift(() => RebuildPoints(rebuild))
                        from breps in GeometryResults.Acquire(() => run.Sweeper.PerformSweepRebuild(run.Rail, run.Shapes, run.Stations, points), nameof(SweepOneRail.PerformSweepRebuild))
                        select breps,
                    refit: static (run, refit) => GeometryResults.Acquire(() => run.Sweeper.PerformSweepRefit(run.Rail, run.Shapes, run.Stations, refit.Tolerance), nameof(SweepOneRail.PerformSweepRefit)))
                select swept)
        select breps;

    public static IO<Seq<Brep>> SweepTwo(Curve rail1, Curve rail2, Seq<Curve> shapes, bool closed, SweepTwoMode mode, double tolerance) =>
        from filled in IO.lift(() => Invalid.Unless(!shapes.IsEmpty, nameof(shapes)))
        from breps in mode.Switch(
            (Rail1: rail1, Rail2: rail2, Shapes: shapes, Closed: closed, Tolerance: tolerance),
            direct: static (sweep, direct) =>
                from fit in IO.lift(() => RebuildArguments(direct.Fit))
                from swept in GeometryResults.Acquire(() => Brep.CreateFromSweep(sweep.Rail1, sweep.Rail2, sweep.Shapes, direct.Start.IfNone(Point3d.Unset), direct.End.IfNone(Point3d.Unset), sweep.Closed, sweep.Tolerance, fit.Rebuild, fit.PointCount, fit.RefitTolerance, direct.PreserveHeight, direct.AutoAdjust), nameof(Brep.CreateFromSweep))
                select swept,
            parameterized: static (sweep, parameterized) =>
                from counted in IO.lift(() =>
                    from first in CountMismatch.Unless(sweep.Shapes.Count, parameterized.Rail1Stations.Count, nameof(SweepTwoRail.PerformSweep))
                    from second in CountMismatch.Unless(sweep.Shapes.Count, parameterized.Rail2Stations.Count, nameof(SweepTwoRail.PerformSweep))
                    select unit)
                from sweeper in IO.lift(() => new SweepTwoRail { SweepTolerance = sweep.Tolerance, AngleToleranceRadians = parameterized.AngleToleranceRadians, MaintainHeight = parameterized.MaintainHeight, ClosedSweep = sweep.Closed, AutoAdjust = parameterized.AutoAdjust, UseLegacySweeper = parameterized.UseLegacySweeper })
                from swept in parameterized.Fit.Switch(
                    (Sweeper: sweeper, Sweep: sweep, Stations: parameterized),
                    asIs: static (run, _) => GeometryResults.Acquire(() => run.Sweeper.PerformSweep(run.Sweep.Rail1, run.Sweep.Rail2, run.Sweep.Shapes, run.Stations.Rail1Stations, run.Stations.Rail2Stations), nameof(SweepTwoRail.PerformSweep)),
                    rebuild: static (run, rebuild) =>
                        from points in IO.lift(() => RebuildPoints(rebuild))
                        from breps in GeometryResults.Acquire(() => run.Sweeper.PerformSweepRebuild(run.Sweep.Rail1, run.Sweep.Rail2, run.Sweep.Shapes, run.Stations.Rail1Stations, run.Stations.Rail2Stations, points), nameof(SweepTwoRail.PerformSweepRebuild))
                        select breps,
                    refit: static (run, refit) => GeometryResults.Acquire(() => run.Sweeper.PerformSweepRefit(run.Sweep.Rail1, run.Sweep.Rail2, run.Sweep.Shapes, run.Stations.Rail1Stations, run.Stations.Rail2Stations, refit.Tolerance), nameof(SweepTwoRail.PerformSweepRefit)))
                select swept,
            partitioned: static (sweep, parts) =>
                from counted in IO.lift(() => CountMismatch.Unless(sweep.Shapes.Count, parts.RailParameters.Count, nameof(Brep.CreateFromSweepInParts)))
                from swept in GeometryResults.Acquire(() => Brep.CreateFromSweepInParts(sweep.Rail1, sweep.Rail2, sweep.Shapes, parts.RailParameters, sweep.Closed, sweep.Tolerance), nameof(Brep.CreateFromSweepInParts))
                select swept)
        select breps;

    private static Fin<(SweepRebuild Rebuild, int PointCount, double RefitTolerance)> RebuildArguments(CurveFit fit) =>
        fit.Switch<Fin<(SweepRebuild Rebuild, int PointCount, double RefitTolerance)>>(
            asIs: static _ => (SweepRebuild.None, 0, 0.0),
            rebuild: static rebuild => RebuildPoints(rebuild).Map(static points => (SweepRebuild.Rebuild, points, 0.0)),
            refit: static refit => (SweepRebuild.Refit, 0, refit.Tolerance));

    private static Fin<int> RebuildPoints(CurveFit.Rebuild rebuild) =>
        Limits.AtLeast(MinimumRebuildPoints).Check(rebuild.PointCount, nameof(CurveFit.Rebuild.PointCount));

    // --- [LOFTS]
    public static IO<Seq<Brep>> Loft(Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, LoftType kind, bool closed, CurveFit fit, double angleTolerance) =>
        from filled in IO.lift(() => Limits.AtLeast(MinimumShapes).Check(shapes.Count, nameof(shapes)))
        from breps in fit.Switch(
            (Shapes: shapes, Start: start.IfNone(Point3d.Unset), End: end.IfNone(Point3d.Unset), Kind: kind, Closed: closed, Angle: angleTolerance),
            asIs: static (loft, _) => GeometryResults.Acquire(() => Brep.CreateFromLoft(loft.Shapes, loft.Start, loft.End, loft.Kind, loft.Closed, loft.Angle), nameof(Brep.CreateFromLoft)),
            rebuild: static (loft, rebuild) =>
                from points in IO.lift(() => RebuildPoints(rebuild))
                from lofted in GeometryResults.Acquire(() => Brep.CreateFromLoftRebuild(loft.Shapes, loft.Start, loft.End, loft.Kind, loft.Closed, loft.Angle, points), nameof(Brep.CreateFromLoftRebuild))
                select lofted,
            refit: static (loft, refit) => GeometryResults.Acquire(() => Brep.CreateFromLoftRefit(loft.Shapes, loft.Start, loft.End, loft.Kind, loft.Closed, loft.Angle, refit.Tolerance), nameof(Brep.CreateFromLoftRefit)))
        select breps;

    public static IO<Seq<Brep>> LoftTangent(Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, BrepTrim startTrim, BrepTrim endTrim, bool startTangent, bool endTangent, LoftType kind, bool closed) =>
        from filled in IO.lift(() => Limits.AtLeast(MinimumShapes).Check(shapes.Count, nameof(shapes)))
        from breps in GeometryResults.Acquire(() => Brep.CreateFromLoft(shapes, start.IfNone(Point3d.Unset), end.IfNone(Point3d.Unset), startTangent, endTangent, startTrim, endTrim, kind, closed), nameof(Brep.CreateFromLoft))
        select breps;

    public static IO<Seq<Brep>> Developable(DevelopableMethod method) =>
        method.Switch(
            byDensity: static dense =>
                from density in IO.lift(() => Limits.AtLeast(1).Check(dense.Density, nameof(DevelopableMethod.ByDensity.Density)))
                from breps in GeometryResults.Acquire(() => Brep.CreateDevelopableLoft(dense.Curve0, dense.Curve1, dense.Reverse0, dense.Reverse1, density), nameof(Brep.CreateDevelopableLoft))
                select breps,
            byRulings: static ruled =>
                from filled in IO.lift(() => Invalid.Unless(!ruled.Rulings.IsEmpty, nameof(DevelopableMethod.ByRulings.Rulings)))
                from breps in GeometryResults.Acquire(() => Brep.CreateDevelopableLoft(ruled.Rail0, ruled.Rail1, ruled.Rulings), nameof(Brep.CreateDevelopableLoft))
                select breps);

    // --- [PATCHES]
    public static IO<Brep> Patch(Seq<GeometryBase> geometry, Option<Surface> starting, PatchOptions options, double tolerance) =>
        from valid in IO.lift(() =>
            from filled in Invalid.Unless(!geometry.IsEmpty, nameof(geometry))
            from uSpanned in Limits.AtLeast(1).Check(options.USpans, nameof(PatchOptions.USpans))
            from vSpanned in Limits.AtLeast(1).Check(options.VSpans, nameof(PatchOptions.VSpans))
            select unit)
        from patch in IO.lift(() => Missing.Unless(Brep.CreatePatch(
                geometry,
                starting.ValueUnsafe(),
                options.USpans,
                options.VSpans,
                options.Trim,
                options.Tangency,
                options.PointSpacing,
                options.Flexibility,
                options.SurfacePull,
                [options.FixNorth, options.FixEast, options.FixSouth, options.FixWest],
                tolerance), nameof(Brep.CreatePatch)))
        select patch;

    public static IO<VariationalPatchResult> Variational(Seq<(Curve Curve, Continuity Continuity)> edges, Seq<(Curve Curve, Continuity Continuity)> interior, Seq<Point3d> points, Brep.VariationalPatchSettings settings, bool multiThreading, Option<IProgress<double>> progress, CancellationToken cancel) =>
        from filled in IO.lift(() => Invalid.Unless(!edges.IsEmpty, nameof(edges)))
        from result in IO.lift(() => Optional(Brep.CreateVariationalPatch(
                edges.Map(static edge => new Brep.CurveConstraint(edge.Curve, edge.Continuity)),
                interior.Map(static edge => new Brep.CurveConstraint(edge.Curve, edge.Continuity)),
                points.Map(static point => new Brep.PointConstraint(point)),
                settings,
                multiThreading,
                cancel,
                progress.ValueUnsafe(),
                out Brep.VariationalPatchResult results))
            .ToFin(Answers.Present(results.Error).Match<VariationalPatchFailed>(Some: static reason => new VariationalPatchFailed.WithReason(reason), None: static () => new VariationalPatchFailed.WithoutReason()))
            .Map(created => new VariationalPatchResult(created, Answers.Present(results.Warning), Optional(results.G0Int), Optional(results.G0), Optional(results.G1), Optional(results.G2))))
        select result;
}
