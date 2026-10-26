using System.Globalization;
using Rasm.Rhino.Document.Units;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
public enum NetworkContinuity { Loose = 0, Position = 1, Tangency = 2, Curvature = 3 }

public enum NetworkFailure { None = 0, Sorting = 1, Initialization = 2, Build = 3, Validity = 4 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NetworkCurves(Seq<Curve> Curves, NetworkContinuity Start, NetworkContinuity End);

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NetworkMethod {
    public sealed record AutoSorted(Seq<Curve> Curves, NetworkContinuity Continuity) : NetworkMethod;

    public sealed record Ordered(NetworkCurves U, NetworkCurves V) : NetworkMethod;
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PointCount : System.Numerics.IMinMaxValue<PointCount> {
    public static PointCount MinValue { get; } = new(2);
    public static PointCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject(SkipToString = true, SerializationFrameworks = SerializationFrameworks.None)]
public sealed partial class FixedRulings {
    [MemberEqualityComparer<ComparerAccessors.Default<Seq<Point2d>>, Seq<Point2d>>]
    public Seq<Point2d> Parameters { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<Point2d> parameters) =>
        validationError = parameters.Count >= 3
            ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"{parameters.Count} fixed rulings hold fewer than 3"));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveFit {
    public (SweepRebuild RebuildType, int RebuildPointCount, double RefitTolerance) Parameters => Switch(
        asIs: static _ => (SweepRebuild.None, 0, 0d),
        rebuild: static rebuild => (SweepRebuild.Rebuild, (int)rebuild.Count, 0d),
        refit: static refit => (SweepRebuild.Refit, 0, refit.Tolerance));

    public sealed record AsIs() : CurveFit;
    public sealed record Rebuild(PointCount Count) : CurveFit;
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

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SweepOne {
    public sealed record Unplaced(SweepRail Rail, Seq<Curve> Shapes, Option<Point3d> Start, Option<Point3d> End, bool RefitRail) : SweepOne;
    public sealed record Placed(Curve Rail, Option<Vector3d> Up, IterableNE<(Curve Shape, double Parameter)> Stations) : SweepOne;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SweepTwo {
    public sealed record Unplaced(Seq<Curve> Shapes, Option<Point3d> Start, Option<Point3d> End, bool AutoAdjust) : SweepTwo;
    public sealed record Placed(IterableNE<(Curve Shape, double Rail1, double Rail2)> Stations) : SweepTwo;
}

[Union<Point3d, BrepTrim>(T1Name = "Apex", T2Name = "Tangent", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LoftEnd {
    internal (Point3d Apex, Option<BrepTrim> Trim) Parameters => Switch(
        apex: static apex => (apex, Option<BrepTrim>.None), tangent: static trim => (Point3d.Unset, Some(trim)));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record DevelopableLoft {
    public sealed record Density(bool Reverse0, bool Reverse1, Option<PointCount> Count) : DevelopableLoft;
    public sealed record Rulings(FixedRulings Fixed) : DevelopableLoft;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SweepMapper {
    [MapProperty(nameof(Tolerances.Absolute), nameof(SweepOneRail.SweepTolerance))]
    [MapProperty(nameof(Tolerances.Angle), nameof(SweepOneRail.AngleToleranceRadians))]
    internal static partial SweepOneRail ToSweepOneRail(Tolerances tolerances, bool closedSweep, bool globalShapeBlending, int miterType);

    [MapProperty(nameof(Tolerances.Absolute), nameof(SweepTwoRail.SweepTolerance))]
    [MapProperty(nameof(Tolerances.Angle), nameof(SweepTwoRail.AngleToleranceRadians))]
    [MapValue(nameof(SweepTwoRail.UseLegacySweeper), true)]
    [MapperIgnoreTarget(nameof(SweepTwoRail.AutoAdjust), Justification = "Ignored under UseLegacySweeper")]
    internal static partial SweepTwoRail ToSweepTwoRail(Tolerances tolerances, bool closedSweep, bool maintainHeight);
}

public static class Lofts {
    // --- [SWEEPS]
    public static IO<Seq<Brep>> Sweep(Tolerances tolerances, SweepOne sweep, bool closed, SweepBlend blend, SweepMiter miter, CurveFit fit, Action<IDisposable> retain) =>
        sweep.Switch(
            (Tolerances: tolerances, Closed: closed, Blend: blend, Miter: miter, Fit: fit, Retain: retain),
            unplaced: static (at, of) => (of.Rail.Parameters, at.Fit.Parameters) switch {
                var (frame, fitting) => Copies.Acquire(nameof(Brep.CreateFromSweep), _ => Answer.NonEmpty(
                    Brep.CreateFromSweep(frame.Rail, of.Shapes, Conversions.Unset(of.Start), Conversions.Unset(of.End), frame.Frame, frame.Normal, at.Closed, at.Blend, at.Miter,
                        at.Tolerances.Absolute, fitting.RebuildType, fitting.RebuildPointCount, fitting.RefitTolerance, of.RefitRail)), at.Retain),
            },
            placed: static (at, of) =>
                from sweeper in IO.lift(() => SweepMapper.ToSweepOneRail(at.Tolerances, closedSweep: at.Closed, globalShapeBlending: at.Blend is SweepBlend.Global, miterType: (int)at.Miter))
                from configured in IO.lift(() => of.Up.Iter(sweeper.SetRoadlikeUpDirection))
                let rows = toSeq(of.Stations.OrderBy(static row => row.Parameter))
                from swept in at.Fit.Switch(
                    (Sweeper: sweeper, of.Rail, Shapes: rows.Map(static row => row.Shape), Parameters: rows.Map(static row => row.Parameter), at.Retain),
                    asIs: static (run, _) => Copies.Acquire(nameof(SweepOneRail.PerformSweep), _ => Answer.NonEmpty(
                        run.Sweeper.PerformSweep(run.Rail, run.Shapes, run.Parameters)), run.Retain),
                    rebuild: static (run, rebuild) => Copies.Acquire(nameof(SweepOneRail.PerformSweepRebuild), _ => Answer.NonEmpty(
                        run.Sweeper.PerformSweepRebuild(run.Rail, run.Shapes, run.Parameters, rebuild.Count)), run.Retain),
                    refit: static (run, refit) => Copies.Acquire(nameof(SweepOneRail.PerformSweepRefit), _ => Answer.NonEmpty(
                        run.Sweeper.PerformSweepRefit(run.Rail, run.Shapes, run.Parameters, refit.Tolerance)), run.Retain))
                select swept);

    public static IO<Seq<Brep>> Sweep(Tolerances tolerances, Curve rail1, Curve rail2, SweepTwo sweep, bool closed, CurveFit fit, bool maintainHeight, Action<IDisposable> retain) =>
        sweep.Switch(
            (Tolerances: tolerances, Rail1: rail1, Rail2: rail2, Closed: closed, Fit: fit, MaintainHeight: maintainHeight, Retain: retain),
            unplaced: static (at, of) => at.Fit.Parameters switch {
                var fitting => Copies.Acquire(nameof(Brep.CreateFromSweep), _ => Answer.NonEmpty(
                    Brep.CreateFromSweep(at.Rail1, at.Rail2, of.Shapes, Conversions.Unset(of.Start), Conversions.Unset(of.End), at.Closed, at.Tolerances.Absolute,
                        fitting.RebuildType, fitting.RebuildPointCount, fitting.RefitTolerance, at.MaintainHeight, of.AutoAdjust)), at.Retain),
            },
            placed: static (at, of) =>
                from sweeper in IO.lift(() => SweepMapper.ToSweepTwoRail(at.Tolerances, closedSweep: at.Closed, maintainHeight: at.MaintainHeight))
                let rows = toSeq(of.Stations)
                from swept in at.Fit.Switch(
                    (Sweeper: sweeper, at.Rail1, at.Rail2, Shapes: rows.Map(static row => row.Shape), Parameters1: rows.Map(static row => row.Rail1), Parameters2: rows.Map(static row => row.Rail2), at.Retain),
                    asIs: static (run, _) => Copies.Acquire(nameof(SweepTwoRail.PerformSweep), _ => Answer.NonEmpty(
                        run.Sweeper.PerformSweep(run.Rail1, run.Rail2, run.Shapes, run.Parameters1, run.Parameters2)), run.Retain),
                    rebuild: static (run, rebuild) => Copies.Acquire(nameof(SweepTwoRail.PerformSweepRebuild), _ => Answer.NonEmpty(
                        run.Sweeper.PerformSweepRebuild(run.Rail1, run.Rail2, run.Shapes, run.Parameters1, run.Parameters2, rebuild.Count)), run.Retain),
                    refit: static (run, refit) => Copies.Acquire(nameof(SweepTwoRail.PerformSweepRefit), _ => Answer.NonEmpty(
                        run.Sweeper.PerformSweepRefit(run.Rail1, run.Rail2, run.Shapes, run.Parameters1, run.Parameters2, refit.Tolerance)), run.Retain))
                select swept);

    // --- [LOFTS]
    public static IO<Seq<Brep>> Loft(Tolerances tolerances, Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, LoftType type, bool closed, CurveFit fit, Action<IDisposable> retain) =>
        fit.Switch(
            (Shapes: shapes, Start: Conversions.Unset(start), End: Conversions.Unset(end), Type: type, Closed: closed, tolerances.Angle, Retain: retain),
            asIs: static (loft, _) => Copies.Acquire(nameof(Brep.CreateFromLoft), _ => Answer.NonEmpty(
                Brep.CreateFromLoft(loft.Shapes, loft.Start, loft.End, loft.Type, loft.Closed, loft.Angle)), loft.Retain),
            rebuild: static (loft, rebuild) => Copies.Acquire(nameof(Brep.CreateFromLoftRebuild), _ => Answer.NonEmpty(
                Brep.CreateFromLoftRebuild(loft.Shapes, loft.Start, loft.End, loft.Type, loft.Closed, loft.Angle, rebuild.Count)), loft.Retain),
            refit: static (loft, refit) => Copies.Acquire(nameof(Brep.CreateFromLoftRefit), _ => Answer.NonEmpty(
                Brep.CreateFromLoftRefit(loft.Shapes, loft.Start, loft.End, loft.Type, loft.Closed, loft.Angle, refit.Tolerance)), loft.Retain));

    public static IO<Seq<Brep>> Loft(Seq<Curve> shapes, Option<LoftEnd> start, Option<LoftEnd> end, LoftType type, bool closed, Action<IDisposable> retain) =>
        (start.Map(static at => at.Parameters).IfNone((Point3d.Unset, Option<BrepTrim>.None)), end.Map(static at => at.Parameters).IfNone((Point3d.Unset, Option<BrepTrim>.None))) switch {
            var (first, last) => Copies.Acquire(nameof(Brep.CreateFromLoft), _ => Answer.NonEmpty(
                Brep.CreateFromLoft(shapes, first.Apex, last.Apex, first.Trim.IsSome, last.Trim.IsSome, first.Trim.ValueUnsafe(), last.Trim.ValueUnsafe(), type, closed)), retain),
        };

    public static IO<Seq<Brep>> DevelopableLoft(NurbsCurve rail0, NurbsCurve rail1, DevelopableLoft loft, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Brep.CreateDevelopableLoft), _ => Answer.NonEmpty(loft.Switch(
            (Rail0: rail0, Rail1: rail1),
            density: static (rails, density) => Brep.CreateDevelopableLoft(rails.Rail0, rails.Rail1, density.Reverse0, density.Reverse1, density.Count.Map(static count => (int)count).IfNone(0)),
            rulings: static (rails, fixedRulings) => Brep.CreateDevelopableLoft(rails.Rail0, rails.Rail1, fixedRulings.Fixed.Parameters))), retain);

    // --- [SURFACES]
    public static IO<NurbsSurface> Network(Tolerances tolerances, NetworkMethod method, double interiorTolerance, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(NurbsSurface.CreateNetworkSurface), _ => method.Switch(
            (Interior: interiorTolerance, Tolerances: tolerances),
            autoSorted: static (network, sorted) => Networked(
                NurbsSurface.CreateNetworkSurface(sorted.Curves, (int)sorted.Continuity, network.Tolerances.Absolute, network.Interior, network.Tolerances.Angle, out int error), error),
            ordered: static (network, ordered) => Networked(
                NurbsSurface.CreateNetworkSurface(
                    ordered.U.Curves, (int)ordered.U.Start, (int)ordered.U.End, ordered.V.Curves, (int)ordered.V.Start, (int)ordered.V.End,
                    network.Tolerances.Absolute, network.Interior, network.Tolerances.Angle, out int error), error)), retain);

    public static IO<RevSurface> Revolved(Curve profile, Line axis, Option<Interval> angles, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(RevSurface.Create), _ => Answer.One(angles.Match(
            Some: sweep => RevSurface.Create(profile, axis, sweep.T0, sweep.T1),
            None: () => RevSurface.Create(profile, axis))), retain);

    private static Answer<NurbsSurface> Networked(NurbsSurface? surface, int error) =>
        (Answer.Reported(error is 0 ? null : $"failed at {(NetworkFailure)error}"), Answer.One(surface))
            .Apply(static (_, network) => network).As();

    // --- [PATCHES]
    public static IO<(Brep Patch, Brep.VariationalPatchResult Result)> VariationalPatch(
        Seq<Brep.CurveConstraint> edges, Seq<Brep.CurveConstraint> internalCurves, Seq<Brep.PointConstraint> points, Brep.VariationalPatchSettings settings,
        bool multiThreading, Option<IProgress<double>> progress, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Brep.CreateVariationalPatch), token =>
            Brep.CreateVariationalPatch(edges, internalCurves, points, settings, multiThreading, token, progress.ValueUnsafe(), out Brep.VariationalPatchResult result) switch {
                var patch => (Answer.One(patch), Answer.Reported(patch is null ? result.Error : null)).Apply((held, _) => (Patch: held, Result: result)).As(),
            }, retain);
}
