using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct RebuildCount : System.Numerics.IMinMaxValue<RebuildCount> {
    public static RebuildCount MinValue { get; } = new(2);
    public static RebuildCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct DevelopableDensity : System.Numerics.IMinMaxValue<DevelopableDensity> {
    public static DevelopableDensity MinValue { get; } = new(2);
    public static DevelopableDensity MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveFit {
    public (SweepRebuild RebuildType, int RebuildPointCount, double RefitTolerance) Parameters => Switch(
        asIs: static _ => (SweepRebuild.None, 0, 0d),
        rebuild: static rebuild => (SweepRebuild.Rebuild, (int)rebuild.Count, 0d),
        refit: static refit => (SweepRebuild.Refit, 0, refit.Tolerance));

    public sealed record AsIs() : CurveFit;
    public sealed record Rebuild(RebuildCount Count) : CurveFit;
    public sealed record Refit(double Tolerance) : CurveFit;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SweepRail {
    internal (Curve Rail, SweepFrame Frame, Vector3d Normal) Parameters => Switch(
        freeform: static rail => (rail.Rail, SweepFrame.Freeform, Vector3d.Unset),
        roadlike: static rail => (rail.Rail, SweepFrame.Roadlike, rail.Up),
        alignWithSurface: static rail => (rail.Trim, SweepFrame.AlignWithSurface, Vector3d.Unset));

    public sealed record Freeform(Curve Rail) : SweepRail;
    public sealed record Roadlike(Curve Rail, Vector3d Up) : SweepRail;
    public sealed record AlignWithSurface(BrepTrim Trim) : SweepRail;
}

[Union<Point3d, BrepTrim>(T1Name = "Apex", T2Name = "Tangent", MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LoftEnd {
    internal (Point3d Apex, Option<BrepTrim> Trim) Parameters => Switch(
        apex: static apex => (apex, Option<BrepTrim>.None), tangent: static trim => (Point3d.Unset, Some(trim)));
}

public sealed record PatchReport(Option<string> Warning, Option<bool> G0Int, Option<bool> G0, Option<bool> G1, Option<bool> G2);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SweepMapper {
    [MapProperty(nameof(Tolerances.Absolute), nameof(SweepOneRail.SweepTolerance))]
    [MapProperty(nameof(Tolerances.Angle), nameof(SweepOneRail.AngleToleranceRadians))]
    internal static partial SweepOneRail ToSweepOneRail(Tolerances tolerances, bool closedSweep, bool globalShapeBlending, int miterType);

    [MapProperty(nameof(Tolerances.Absolute), nameof(SweepTwoRail.SweepTolerance))]
    [MapProperty(nameof(Tolerances.Angle), nameof(SweepTwoRail.AngleToleranceRadians))]
    [MapValue(nameof(SweepTwoRail.UseLegacySweeper), true)]
    [MapperIgnoreTarget(nameof(SweepTwoRail.AutoAdjust), Justification = "The legacy sweeper reads no AutoAdjust")]
    internal static partial SweepTwoRail ToSweepTwoRail(Tolerances tolerances, bool closedSweep, bool maintainHeight);
}

public static class Lofts {
    // --- [SWEEPS]
    public static IO<Seq<Brep>> SweepOne(SweepRail rail, Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, bool closed, SweepBlend blend, SweepMiter miter, CurveFit fit, bool refitRail, Tolerances tolerances) =>
        (rail.Parameters, fit.Parameters) switch {
            var (frame, fitting) => Copies.AcquireNonEmpty(
                () => Brep.CreateFromSweep(frame.Rail, shapes, Conversions.Unset(start), Conversions.Unset(end), frame.Frame, frame.Normal, closed, blend, miter,
                    tolerances.Absolute, fitting.RebuildType, fitting.RebuildPointCount, fitting.RefitTolerance, refitRail), nameof(Brep.CreateFromSweep)),
        };

    public static IO<Seq<Brep>> SweepOne(Curve rail, Option<Vector3d> up, IterableNE<(Curve Shape, double Parameter)> stations, bool closed, SweepBlend blend, SweepMiter miter, CurveFit fit, Tolerances tolerances) =>
        from sweeper in IO.lift(() => SweepMapper.ToSweepOneRail(tolerances, closedSweep: closed, globalShapeBlending: blend == SweepBlend.Global, miterType: (int)miter))
        from configured in IO.lift(() => sweeper.SetRoadlikeUpDirection(Conversions.Unset(up)))
        let rows = toSeq(stations.OrderBy(static row => row.Parameter))
        from swept in fit.Switch(
            (Sweeper: sweeper, Rail: rail, Shapes: rows.Map(static row => row.Shape), Parameters: rows.Map(static row => row.Parameter)),
            asIs: static (sweep, _) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweep(sweep.Rail, sweep.Shapes, sweep.Parameters), nameof(SweepOneRail.PerformSweep)),
            rebuild: static (sweep, rebuild) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweepRebuild(sweep.Rail, sweep.Shapes, sweep.Parameters, rebuild.Count), nameof(SweepOneRail.PerformSweepRebuild)),
            refit: static (sweep, refit) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweepRefit(sweep.Rail, sweep.Shapes, sweep.Parameters, refit.Tolerance), nameof(SweepOneRail.PerformSweepRefit)))
        select swept;

    public static IO<Seq<Brep>> SweepTwo(Curve rail1, Curve rail2, Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, bool closed, CurveFit fit, bool preserveHeight, bool autoAdjust, Tolerances tolerances) =>
        fit.Parameters switch {
            var fitting => Copies.AcquireNonEmpty(
                () => Brep.CreateFromSweep(rail1, rail2, shapes, Conversions.Unset(start), Conversions.Unset(end), closed, tolerances.Absolute,
                    fitting.RebuildType, fitting.RebuildPointCount, fitting.RefitTolerance, preserveHeight, autoAdjust), nameof(Brep.CreateFromSweep)),
        };

    public static IO<Seq<Brep>> SweepTwo(Curve rail1, Curve rail2, IterableNE<(Curve Shape, double Rail1, double Rail2)> stations, bool closed, CurveFit fit, bool maintainHeight, Tolerances tolerances) =>
        from sweeper in IO.lift(() => SweepMapper.ToSweepTwoRail(tolerances, closedSweep: closed, maintainHeight: maintainHeight))
        let rows = toSeq(stations)
        from swept in fit.Switch(
            (Sweeper: sweeper, Rail1: rail1, Rail2: rail2, Shapes: rows.Map(static row => row.Shape), Parameters1: rows.Map(static row => row.Rail1), Parameters2: rows.Map(static row => row.Rail2)),
            asIs: static (sweep, _) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweep(sweep.Rail1, sweep.Rail2, sweep.Shapes, sweep.Parameters1, sweep.Parameters2), nameof(SweepTwoRail.PerformSweep)),
            rebuild: static (sweep, rebuild) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweepRebuild(sweep.Rail1, sweep.Rail2, sweep.Shapes, sweep.Parameters1, sweep.Parameters2, rebuild.Count), nameof(SweepTwoRail.PerformSweepRebuild)),
            refit: static (sweep, refit) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweepRefit(sweep.Rail1, sweep.Rail2, sweep.Shapes, sweep.Parameters1, sweep.Parameters2, refit.Tolerance), nameof(SweepTwoRail.PerformSweepRefit)))
        select swept;

    // --- [LOFTS]
    public static IO<Seq<Brep>> Loft(Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, LoftType type, bool closed, CurveFit fit, Tolerances tolerances) =>
        fit.Switch(
            (Shapes: shapes, Start: Conversions.Unset(start), End: Conversions.Unset(end), Type: type, Closed: closed, tolerances.Angle),
            asIs: static (loft, _) => Copies.AcquireNonEmpty(
                () => Brep.CreateFromLoft(loft.Shapes, loft.Start, loft.End, loft.Type, loft.Closed, loft.Angle), nameof(Brep.CreateFromLoft)),
            rebuild: static (loft, rebuild) => Copies.AcquireNonEmpty(
                () => Brep.CreateFromLoftRebuild(loft.Shapes, loft.Start, loft.End, loft.Type, loft.Closed, loft.Angle, rebuild.Count), nameof(Brep.CreateFromLoftRebuild)),
            refit: static (loft, refit) => Copies.AcquireNonEmpty(
                () => Brep.CreateFromLoftRefit(loft.Shapes, loft.Start, loft.End, loft.Type, loft.Closed, loft.Angle, refit.Tolerance), nameof(Brep.CreateFromLoftRefit)));

    public static IO<Seq<Brep>> Loft(Seq<Curve> shapes, Option<LoftEnd> start, Option<LoftEnd> end, LoftType type, bool closed) =>
        (start.Map(static at => at.Parameters).IfNone((Point3d.Unset, Option<BrepTrim>.None)), end.Map(static at => at.Parameters).IfNone((Point3d.Unset, Option<BrepTrim>.None))) switch {
            var (first, last) => Copies.AcquireNonEmpty(
                () => Brep.CreateFromLoft(shapes, first.Apex, last.Apex, first.Trim.IsSome, last.Trim.IsSome, first.Trim.ValueUnsafe(), last.Trim.ValueUnsafe(), type, closed),
                nameof(Brep.CreateFromLoft)),
        };

    public static IO<Seq<Brep>> DevelopableLoft(Curve rail0, Curve rail1, bool reverse0, bool reverse1, Option<DevelopableDensity> density) =>
        Copies.AcquireNonEmpty(
            () => Brep.CreateDevelopableLoft(rail0, rail1, reverse0, reverse1, density.Map(static count => (int)count).IfNone(0)), nameof(Brep.CreateDevelopableLoft));

    // --- [PATCHES]
    public static IO<Brep> Patch(
        Seq<GeometryBase> geometry, Option<Surface> startingSurface, int uSpans, int vSpans, bool trim, bool tangency, double pointSpacing, double flexibility,
        double surfacePull, (bool Left, bool Bottom, bool Right, bool Top) fixEdges, Tolerances tolerances) =>
        Copies.Acquire(
            () => Brep.CreatePatch(
                geometry, startingSurface.ValueUnsafe(), uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull,
                [fixEdges.Left, fixEdges.Bottom, fixEdges.Right, fixEdges.Top], tolerances.Absolute),
            nameof(Brep.CreatePatch));

    public static IO<(Brep Patch, PatchReport Report)> VariationalPatch(
        Seq<Brep.CurveConstraint> edges, Seq<Brep.CurveConstraint> internalCurves, Seq<Brep.PointConstraint> points, Brep.VariationalPatchSettings settings,
        bool multiThreading, Option<IProgress<double>> progress) =>
        from boundary in Copies.Acquire(() => Curve.JoinCurves(edges.Map(static edge => edge.Curve), settings.Tolerance), nameof(Curve.JoinCurves)).Bracket(
            Use: loops => loops.ForAll(static loop => loop.IsClosed) switch {
                var closed => IO.lift(() => OpenBoundary.Unless(loops.IsEmpty == edges.IsEmpty && closed, nameof(Brep.CreateVariationalPatch))),
            },
            Fin: DisposalOps.Release)
        from token in cancelToken
        from solved in Copies.Owned(
            IO.lift(() => (
                Patch: Brep.CreateVariationalPatch(edges, internalCurves, points, settings, multiThreading, token, progress.ValueUnsafe(), out Brep.VariationalPatchResult results),
                Results: results, Cancelled: token.IsCancellationRequested)),
            static made => Optional(made.Patch)
                .ToFin(Conversions.Present(made.Results.Error).Match(
                    Some: static reason => new VariationalPatchFailed(reason),
                    None: () => made.Cancelled ? Errors.Cancelled : new Missing(nameof(Brep.CreateVariationalPatch))))
                .Bind(static patch => Measurements.Valid(patch, nameof(Brep.CreateVariationalPatch)))
                .Map(patch => (Patch: patch, Report: new PatchReport(
                    Conversions.Present(made.Results.Warning), Optional(made.Results.G0Int), Optional(made.Results.G0), Optional(made.Results.G1), Optional(made.Results.G2)))),
            static made => DisposalOps.Release(Optional(made.Patch).ToSeq()))
        select solved;
}
