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

[Union]
public abstract partial record CurveFit {
    public SweepRebuild RebuildType => Map(asIs: SweepRebuild.None, rebuild: SweepRebuild.Rebuild, refit: SweepRebuild.Refit);

    public int RebuildPointCount => Switch(asIs: static _ => 0, rebuild: static rebuild => rebuild.Count, refit: static _ => 0);

    public double RefitTolerance => Switch(asIs: static _ => 0d, rebuild: static _ => 0d, refit: static refit => refit.Tolerance);

    public sealed record AsIs() : CurveFit;

    public sealed record Rebuild(RebuildCount Count) : CurveFit;

    public sealed record Refit(double Tolerance) : CurveFit;
}

[Union]
public abstract partial record SweepRail {
    private SweepRail(Curve rail) => Rail = rail;

    public Curve Rail { get; }

    public SweepFrame Frame => Map(freeform: SweepFrame.Freeform, roadlike: SweepFrame.Roadlike, alignWithSurface: SweepFrame.AlignWithSurface);

    public Option<Vector3d> RoadlikeNormal => Switch<Option<Vector3d>>(freeform: static _ => None, roadlike: static roadlike => roadlike.Up, alignWithSurface: static _ => None);

    public sealed record Freeform(Curve Rail) : SweepRail(Rail);

    public sealed record Roadlike(Curve Rail, Vector3d Up) : SweepRail(Rail);

    public sealed record AlignWithSurface(BrepTrim Trim) : SweepRail(Trim);
}

[Union<Point3d, BrepTrim>(T1Name = "Apex", T2Name = "Tangent")]
public sealed partial class LoftEnd {
    public Option<Point3d> Apex => Switch<Option<Point3d>>(apex: static apex => apex, tangent: static _ => None);

    public Option<BrepTrim> Trim => Switch<Option<BrepTrim>>(apex: static _ => None, tangent: static trim => trim);
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
        Copies.AcquireNonEmpty(
            () => Brep.CreateFromSweep(
                rail.Rail, shapes, Conversions.Unset(start), Conversions.Unset(end), rail.Frame, Conversions.Unset(rail.RoadlikeNormal), closed, blend, miter,
                tolerances.Absolute, fit.RebuildType, fit.RebuildPointCount, fit.RefitTolerance, refitRail),
            nameof(Brep.CreateFromSweep));

    public static IO<Seq<Brep>> SweepOne(Curve rail, Option<Vector3d> up, IterableNE<(Curve Shape, double Parameter)> stations, bool closed, SweepBlend blend, SweepMiter miter, CurveFit fit, Tolerances tolerances) =>
        from sweeper in IO.lift(() => {
            SweepOneRail created = SweepMapper.ToSweepOneRail(tolerances, closedSweep: closed, globalShapeBlending: blend == SweepBlend.Global, miterType: (int)miter);
            created.SetRoadlikeUpDirection(Conversions.Unset(up));
            return created;
        })
        from swept in fit.Switch(
            (Sweeper: sweeper, Rail: rail, Rows: toSeq(stations.AsEnumerable().OrderBy(static row => row.Parameter))),
            asIs: static (sweep, _) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweep(sweep.Rail, sweep.Rows.Map(static row => row.Shape), sweep.Rows.Map(static row => row.Parameter)),
                nameof(SweepOneRail.PerformSweep)),
            rebuild: static (sweep, rebuild) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweepRebuild(sweep.Rail, sweep.Rows.Map(static row => row.Shape), sweep.Rows.Map(static row => row.Parameter), rebuild.Count),
                nameof(SweepOneRail.PerformSweepRebuild)),
            refit: static (sweep, refit) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweepRefit(sweep.Rail, sweep.Rows.Map(static row => row.Shape), sweep.Rows.Map(static row => row.Parameter), refit.Tolerance),
                nameof(SweepOneRail.PerformSweepRefit)))
        select swept;

    public static IO<Seq<Brep>> SweepTwo(Curve rail1, Curve rail2, Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, bool closed, CurveFit fit, bool preserveHeight, bool autoAdjust, Tolerances tolerances) =>
        Copies.AcquireNonEmpty(
            () => Brep.CreateFromSweep(
                rail1, rail2, shapes, Conversions.Unset(start), Conversions.Unset(end), closed, tolerances.Absolute,
                fit.RebuildType, fit.RebuildPointCount, fit.RefitTolerance, preserveHeight, autoAdjust),
            nameof(Brep.CreateFromSweep));

    public static IO<Seq<Brep>> SweepTwo(Curve rail1, Curve rail2, IterableNE<(Curve Shape, double Rail1, double Rail2)> stations, bool closed, CurveFit fit, bool maintainHeight, Tolerances tolerances) =>
        from sweeper in IO.lift(() => SweepMapper.ToSweepTwoRail(tolerances, closedSweep: closed, maintainHeight: maintainHeight))
        from swept in fit.Switch(
            (Sweeper: sweeper, Rail1: rail1, Rail2: rail2, Rows: toSeq(stations.AsEnumerable())),
            asIs: static (sweep, _) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweep(sweep.Rail1, sweep.Rail2, sweep.Rows.Map(static row => row.Shape), sweep.Rows.Map(static row => row.Rail1), sweep.Rows.Map(static row => row.Rail2)),
                nameof(SweepTwoRail.PerformSweep)),
            rebuild: static (sweep, rebuild) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweepRebuild(sweep.Rail1, sweep.Rail2, sweep.Rows.Map(static row => row.Shape), sweep.Rows.Map(static row => row.Rail1), sweep.Rows.Map(static row => row.Rail2), rebuild.Count),
                nameof(SweepTwoRail.PerformSweepRebuild)),
            refit: static (sweep, refit) => Copies.AcquireNonEmpty(
                () => sweep.Sweeper.PerformSweepRefit(sweep.Rail1, sweep.Rail2, sweep.Rows.Map(static row => row.Shape), sweep.Rows.Map(static row => row.Rail1), sweep.Rows.Map(static row => row.Rail2), refit.Tolerance),
                nameof(SweepTwoRail.PerformSweepRefit)))
        select swept;

    // --- [LOFTS]
    public static IO<Seq<Brep>> Loft(Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, LoftType type, bool closed, CurveFit fit, Tolerances tolerances) =>
        fit.Switch(
            (Shapes: shapes, Start: Conversions.Unset(start), End: Conversions.Unset(end), Type: type, Closed: closed, tolerances.Angle),
            asIs: static (loft, _) => Copies.AcquireNonEmpty(
                () => Brep.CreateFromLoft(loft.Shapes, loft.Start, loft.End, loft.Type, loft.Closed, loft.Angle),
                nameof(Brep.CreateFromLoft)),
            rebuild: static (loft, rebuild) => Copies.AcquireNonEmpty(
                () => Brep.CreateFromLoftRebuild(loft.Shapes, loft.Start, loft.End, loft.Type, loft.Closed, loft.Angle, rebuild.Count),
                nameof(Brep.CreateFromLoftRebuild)),
            refit: static (loft, refit) => Copies.AcquireNonEmpty(
                () => Brep.CreateFromLoftRefit(loft.Shapes, loft.Start, loft.End, loft.Type, loft.Closed, loft.Angle, refit.Tolerance),
                nameof(Brep.CreateFromLoftRefit)));

    public static IO<Seq<Brep>> Loft(Seq<Curve> shapes, Option<LoftEnd> start, Option<LoftEnd> end, LoftType type, bool closed) =>
        Copies.AcquireNonEmpty(
            () => Brep.CreateFromLoft(
                shapes, Conversions.Unset(start.Bind(static at => at.Apex)), Conversions.Unset(end.Bind(static at => at.Apex)),
                start.Bind(static at => at.Trim).IsSome, end.Bind(static at => at.Trim).IsSome,
                start.Bind(static at => at.Trim).ValueUnsafe(), end.Bind(static at => at.Trim).ValueUnsafe(), type, closed),
            nameof(Brep.CreateFromLoft));

    public static IO<Seq<Brep>> DevelopableLoft(Curve rail0, Curve rail1, bool reverse0, bool reverse1, Option<DevelopableDensity> density) =>
        Copies.AcquireNonEmpty(
            () => Brep.CreateDevelopableLoft(rail0, rail1, reverse0, reverse1, density.Map(static count => count.ToValue()).IfNone(0)),
            nameof(Brep.CreateDevelopableLoft));

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
        from closed in Copies.Acquire(() => Curve.JoinCurves(edges.Map(static edge => edge.Curve), settings.Tolerance), nameof(Curve.JoinCurves)).Bracket(
            Use: loops => IO.lift(() => OpenBoundary.Unless(loops.IsEmpty == edges.IsEmpty && loops.ForAll(static loop => loop.IsClosed), nameof(Brep.CreateVariationalPatch))),
            Fin: DisposalOps.Release)
        from token in cancelToken
        from solved in Copies.Owned(
            IO.lift(() => (
                Patch: Brep.CreateVariationalPatch(edges, internalCurves, points, settings, multiThreading, token, progress.ValueUnsafe(), out Brep.VariationalPatchResult results),
                Results: results)),
            made => Optional(made.Patch)
                .ToFin(Conversions.Present(made.Results.Error).Match(
                    Some: static reason => new VariationalPatchFailed(reason),
                    None: () => token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(Brep.CreateVariationalPatch))))
                .Bind(static patch => Measurements.Valid(patch, nameof(Brep.CreateVariationalPatch)))
                .Map(patch => (Patch: patch, Report: new PatchReport(
                    Conversions.Present(made.Results.Warning), Optional(made.Results.G0Int), Optional(made.Results.G0), Optional(made.Results.G1), Optional(made.Results.G2)))),
            static made => DisposalOps.Release(Optional(made.Patch).ToSeq()))
        select solved;
}
