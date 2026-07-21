# [RASM_FABRICATION_TURNING]

`Turning` owns controller-neutral revolved-process generation from one admitted `TurnRequest`. `TurnStep` binds each semantic operation to a spindle side and turret channel; `TurnProgram` preserves channel barriers, spindle state, synchronization, removal envelopes, and process-load evidence. `Cam` projects every executable `LatheDirective` and `ChannelBarrier` into the S0 `MotionDirective` carrier beside generated moves.

`LatheOp` closes revolved-operation grammars instead of naming controller cycles. `SweepKind`, `PlungeKind`, and `AxialKind` generate roughing, finishing, boring, grooving, undercutting, drilling, reaming, counterboring, and countersinking from policy data; `Part`, `Tap`, `Thread`, `Knurl`, `Transfer`, and `CutoffTransfer` retain distinct payload arity.

`CutSide` is the one owner of the internal-versus-external distinction: radial sign, stock reference, finish-allowance direction, approach and retract polarity, and removal-envelope orientation all read that row, so no operation carries a side flag and no body re-derives a sign.

## [01]-[INDEX]

- [02]-[VOCABULARY]: row-owned sweep, plunge, axial, thread, spindle, channel, and tool-orientation semantics.
- [03]-[DEMAND]: generated owners admit stock, insert, policy, channel steps, and process data once.
- [04]-[GENERATE]: `Turning.Generate(TurnRequest)` compensates once, lowers every step, and returns evidence-bearing passes.

## [02]-[VOCABULARY]

- Coordinates: each `Move.Target.X` is axial machine `Z`, each `Move.Target.Y` is radial machine `X`, and every admitted profile is open.
- Sides: `CutSide` carries `RadialSign` and its `Target` allowance delegate, so an external sweep leaves stock outward while a bore leaves stock inward; `StockRadius`, `Available`, `Advance`, and `Clear` derive every other side-dependent value from that one row.
- Cycles: `SweepKind` binds each row to its `CutSide` and its `Emit` generation delegate, so a new sweep is one row rather than a branch; `PlungeKind` distinguishes grooving, undercutting, and forming; `AxialKind` binds each row to its `Depths` peck schedule. `Part` and `Tap` remain total cases without ignored target, pitch, or hand fields.
- Threads: `ThreadForm` carries load-flank angle, clearance-flank angle, crest/root truncation, crest/root radius, and pitch-depth factor. Named standard values are seed data, while `ThreadForm.Validate` admits custom geometry through the same owner. Buttress geometry remains asymmetric through every pass.
- Tooling: `TurnInsert` composes `CutterForm` with insert width, clearance, lead, and semantic tip orientation; no controller tip number enters the process owner.
- Channels: `SpindleSide` and `TurretChannel` are process facts. `ChannelToken` creates wait/signal barriers without embedding a dialect word.

## [03]-[DEMAND]

- Stock: `TurnStock` admits solid, tubular, and near-net blanks with axial bounds, inner/outer radii, and optional profile evidence.
- Policy: `TurnPolicy` owns approach, retract, overlap, chord, biarc, peck, thread, and spindle-radius values; no operation body carries a local machining constant.
- Admission: `TurnDemand.ValidateFactoryArguments` and `TurnRequest.ValidateFactoryArguments` accumulate profile, stock, insert, process, spindle, step, operation, synchronization, and numeric defects before construction.
- Ingress: `TurnDemand` accepts canonical `Loop`, `CutterForm`, `CuttingData`, and `ProcessBudget.Turning` owners. `CuttingData.FeedBasis` must be `FeedBasis.PerRevolution`.

## [04]-[GENERATE]

- Entry: `Turning.Generate(TurnRequest?)` is the only raw operation.
- Compensation: tip-radius compensation offsets every profile vertex along its local `ZX` normal, orients that normal by the insert's radial posture so traversal order cannot invert the offset, and reanchors it with the semantic `TipOrientation` vector; clearance-angle gouge admission precedes motion and accumulates every gouging span.
- Sweep: each `SweepKind` row emits its own motion — longitudinal rows require positive radial stock before generating passes, facing roughing derives every interpolated material crossing, pattern roughing shifts the full near-net profile and retracts before repositioning, and the finish rows follow the profile natively; `FinishForm` then routes fitted curves through `CurveAlgebra.Apply(CurveOp)` or fits line-sourced chords through `g3.BiArcFit2` with a measured error fallback.
- Crossings: material crossings dedupe within the profile tolerance before pairing, so coincident wall hits at one pass radius cannot shift every span; odd wall parity fails on the sweep's `Fin` rail.
- Plunge: explicit axial position, band width, target radius, peck fraction, and dwell generate groove, undercut, and form families; `Part` reconstructs width and terminal radius from mounted insert and stock.
- Axial: drill, ream, counterbore, and countersink rows share one depth/diameter/peck/tip-angle generator; `Tap` carries thread form, pitch, hand, and semantic spindle synchronization.
- Thread: axial endpoints determine travel, hand remains spindle-synchronization evidence, each start owns pitch indexing, and every pass carries approach, run-in, runout, pullout, asymmetric flank shift, finish, and spring roles.
- Transfer: main/sub-spindle grip and pull facts remain directives; `CutoffTransfer` carries its own executed parting span and load evidence. Channel waits and signals preserve twin-turret ordering.
- Load: `Loaded` is the one pass constructor; an operation stating its own load carries it, and every other pass derives `TurnLoad.Cutting` per cutting span through `CuttingData.Evaluate(CutIntent)`. Knurl pressure states `TurnLoad.Forming` instead of impersonating chip-removal force, and a non-removing pass carries no load.
- Quantities: `CutIntent` admits UnitsNet quantities, so the load boundary converts machining-canonical millimetre, rpm, and feed scalars through `Length.FromMillimeters`, `RotationalSpeed.FromRevolutionsPerMinute`, and `Speed.FromMillimetersPerMinutes` exactly once; radial depth and diameter derive engagement on the admitted intent.
- Receipt: `TurnPass` carries moves, directives, load, and removal envelope; `TurnProgram` carries ordered passes, barriers, residual radial bounds, and physics evidence.
- Packages: `Thinktecture.Runtime.Extensions` owns generated closed families; `LanguageExt.Core` owns accumulated admission and traversal; `System.Numerics.Tensors` owns batch finiteness; `CavalierContours` remains native through `Loop`; `geometry3Sharp` owns residual line-sourced biarcs; `ToolAssembly.Snapshot` supplies provider-detached insert width and lead angle through `ToolMeasure`; `UnitsNet` admits the `CutIntent` load boundary and remains at process ingress and receipt boundaries.
- Boundary: `Turning` owns process geometry and semantic directives; `motion.md` refuses directive-bearing programs because `Move` has no directive atom, and posting admits no typed `TurnProgram` counterpart.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Numerics.Tensors;
using g3;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rasm.Parametric;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CutSide {
    public static readonly CutSide External = new("external", radialSign: 1, ExternalTarget);
    public static readonly CutSide Internal = new("internal", radialSign: -1, InternalTarget);

    public int RadialSign { get; }

    [UseDelegateFromConstructor]
    public partial double Target(Loop profile, double radialAllowance);

    private static double ExternalTarget(Loop profile, double radialAllowance) =>
        profile.Vertices.Map(static point => point.Y).Min() + radialAllowance;

    private static double InternalTarget(Loop profile, double radialAllowance) =>
        profile.Vertices.Map(static point => point.Y).Max() - radialAllowance;

    public double StockRadius(TurnStock stock) => RadialSign > 0 ? stock.OuterRadius : stock.InnerRadius;

    public double Clear(double radius, double distance) => radius + (RadialSign * distance);

    public double Available(TurnStock stock, double target) => RadialSign * (StockRadius(stock) - target);

    public double Advance(TurnStock stock, double target, int pass, double depth) => RadialSign > 0
        ? double.Max(target, stock.OuterRadius - (pass * depth))
        : double.Min(target, stock.InnerRadius + (pass * depth));
}

public readonly record struct SweepDemand(double Depth, double RadialAllowance, double AxialAllowance);

[SmartEnum<string>]
public sealed partial class SweepKind {
    public static readonly SweepKind ExternalLongitudinal = new("external-longitudinal", CutSide.External,
        finish: false, requiresRadialStock: true, Turning.Longitudinal);
    public static readonly SweepKind InternalLongitudinal = new("internal-longitudinal", CutSide.Internal,
        finish: false, requiresRadialStock: true, Turning.Longitudinal);
    public static readonly SweepKind Facing = new("facing", CutSide.External,
        finish: false, requiresRadialStock: false,
        static (profile, demand, sweep, side) => Fin.Succ(Turning.Facing(profile, demand, sweep, side)));
    public static readonly SweepKind NearNet = new("near-net", CutSide.External,
        finish: false, requiresRadialStock: false,
        static (profile, demand, sweep, side) => Fin.Succ(Turning.Pattern(profile, demand, sweep, side)));
    public static readonly SweepKind ExternalFinish = new("external-finish", CutSide.External,
        finish: true, requiresRadialStock: false,
        static (profile, demand, sweep, side) => Fin.Succ(Turning.Native(profile, demand, sweep, side)));
    public static readonly SweepKind InternalFinish = new("internal-finish", CutSide.Internal,
        finish: true, requiresRadialStock: false,
        static (profile, demand, sweep, side) => Fin.Succ(Turning.Native(profile, demand, sweep, side)));

    public CutSide Side { get; }
    public bool Finish { get; }
    public bool RequiresRadialStock { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Seq<Move>> Emit(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side);

    public Fin<Seq<Move>> Sweep(Loop profile, TurnDemand demand, SweepDemand sweep) =>
        Emit(profile, demand, sweep, Side);
}

[SmartEnum<string>]
public sealed partial class PlungeKind {
    public static readonly PlungeKind Groove = new("groove");
    public static readonly PlungeKind Undercut = new("undercut");
    public static readonly PlungeKind Form = new("form");
}

[SmartEnum<string>]
public sealed partial class AxialKind {
    public static readonly AxialKind Drill = new("drill", Pecked);
    public static readonly AxialKind Ream = new("ream", Once);
    public static readonly AxialKind Counterbore = new("counterbore", Pecked);
    public static readonly AxialKind Countersink = new("countersink", Once);

    [UseDelegateFromConstructor]
    public partial Seq<double> Depths(double depth, double peckDepth);

    private static Seq<double> Pecked(double depth, double peckDepth) =>
        Range(1, int.Max(1, (int)Math.Ceiling(depth / peckDepth)))
            .Map(peck => double.Min(depth, peck * peckDepth))
            .ToSeq();

    private static Seq<double> Once(double depth, double peckDepth) => Seq(depth);
}

[SmartEnum<string>]
public sealed partial class FinishForm {
    public static readonly FinishForm NativeArc = new("native-arc");
    public static readonly FinishForm Spline = new("spline");
    public static readonly FinishForm Biarc = new("biarc");
}

[SmartEnum<string>]
public sealed partial class InfeedMethod {
    public static readonly InfeedMethod Radial = new("radial", RadialShift);
    public static readonly InfeedMethod LoadFlank = new("load-flank", LoadShift);
    public static readonly InfeedMethod Alternating = new("alternating", AlternatingShift);

    [UseDelegateFromConstructor]
    public partial double Shift(double depth, int pass, double loadFlankDeg, double clearanceFlankDeg, double reliefDeg);

    private static double RadialShift(double depth, int pass, double loadFlankDeg, double clearanceFlankDeg, double reliefDeg) => 0.0;

    private static double LoadShift(double depth, int pass, double loadFlankDeg, double clearanceFlankDeg, double reliefDeg) =>
        depth * Math.Tan((loadFlankDeg - reliefDeg) * Math.PI / 180.0);

    private static double AlternatingShift(double depth, int pass, double loadFlankDeg, double clearanceFlankDeg, double reliefDeg) =>
        depth * Math.Tan((((pass & 1) == 0 ? clearanceFlankDeg : loadFlankDeg) - reliefDeg) * Math.PI / 180.0)
        * ((pass & 1) == 0 ? -1.0 : 1.0);
}

[ComplexValueObject]
public sealed partial class ThreadForm {
    public static readonly ThreadForm IsoMetric = new("iso-metric", 30.0, 30.0, 0.125, 0.0, 0.0, 0.1443, 0.6134);
    public static readonly ThreadForm Unified = new("unified", 30.0, 30.0, 0.125, 0.0, 0.0, 0.1443, 0.61343);
    public static readonly ThreadForm Whitworth = new("whitworth", 27.5, 27.5, 0.0, 0.0, 0.1373, 0.1373, 0.6403);
    public static readonly ThreadForm Acme = new("acme", 14.5, 14.5, 0.25, 0.25, 0.0, 0.0, 0.5);
    public static readonly ThreadForm Trapezoidal = new("trapezoidal", 15.0, 15.0, 0.25, 0.25, 0.0, 0.0, 0.5);
    public static readonly ThreadForm Buttress = new("buttress", 7.0, 45.0, 0.125, 0.25, 0.0, 0.0, 0.6);
    public static readonly ThreadForm Round = new("round", 30.0, 30.0, 0.0, 0.0, 0.238, 0.238, 0.55);

    public string Key { get; }
    public double LoadFlankDeg { get; }
    public double ClearanceFlankDeg { get; }
    public double CrestTruncationPitch { get; }
    public double RootTruncationPitch { get; }
    public double CrestRadiusPitch { get; }
    public double RootRadiusPitch { get; }
    public double DepthFactor { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref double loadFlankDeg,
        ref double clearanceFlankDeg,
        ref double crestTruncationPitch,
        ref double rootTruncationPitch,
        ref double crestRadiusPitch,
        ref double rootRadiusPitch,
        ref double depthFactor) {
        key = key?.Trim() ?? string.Empty;
        validationError = key.Length > 0
            && TensorPrimitives.IsFiniteAll<double>([
                loadFlankDeg, clearanceFlankDeg, crestTruncationPitch, rootTruncationPitch,
                crestRadiusPitch, rootRadiusPitch, depthFactor])
            && loadFlankDeg is > 0.0 and < 90.0 && clearanceFlankDeg is > 0.0 and < 90.0
            && crestTruncationPitch >= 0.0 && rootTruncationPitch >= 0.0
            && crestRadiusPitch >= 0.0 && rootRadiusPitch >= 0.0
            && depthFactor is > 0.0 and < 1.0
                ? null
                : new ValidationError("<thread-form-degenerate>");
    }
}

[SmartEnum<string>]
public sealed partial class ThreadHand {
    public static readonly ThreadHand Right = new("right");
    public static readonly ThreadHand Left = new("left");
}

[SmartEnum<string>]
public sealed partial class ThreadCutRole {
    public static readonly ThreadCutRole Rough = new("rough");
    public static readonly ThreadCutRole Finish = new("finish");
    public static readonly ThreadCutRole Spring = new("spring");
}

[SmartEnum<string>]
public sealed partial class TipOrientation {
    public static readonly TipOrientation AxialPositiveRadialPositive = new("axial-positive-radial-positive", 1, 1);
    public static readonly TipOrientation AxialNegativeRadialPositive = new("axial-negative-radial-positive", -1, 1);
    public static readonly TipOrientation AxialNegativeRadialNegative = new("axial-negative-radial-negative", -1, -1);
    public static readonly TipOrientation AxialPositiveRadialNegative = new("axial-positive-radial-negative", 1, -1);
    public static readonly TipOrientation RadialPositive = new("radial-positive", 0, 1);
    public static readonly TipOrientation AxialNegative = new("axial-negative", -1, 0);
    public static readonly TipOrientation RadialNegative = new("radial-negative", 0, -1);
    public static readonly TipOrientation AxialPositive = new("axial-positive", 1, 0);
    public static readonly TipOrientation Center = new("center", 0, 0);

    public int Axial { get; }
    public int Radial { get; }
}

[SmartEnum<string>]
public sealed partial class StockKind {
    public static readonly StockKind Solid = new("solid", requiresEnvelope: false);
    public static readonly StockKind Tube = new("tube", requiresEnvelope: false);
    public static readonly StockKind NearNet = new("near-net", requiresEnvelope: true);

    public bool RequiresEnvelope { get; }
}

[SmartEnum<string>]
public sealed partial class SpindleSide {
    public static readonly SpindleSide Main = new("main", axialSign: 1);
    public static readonly SpindleSide Sub = new("sub", axialSign: -1);

    public int AxialSign { get; }
    public SpindleSide Opposite => Switch(
        main: static () => Sub,
        sub: static () => Main);
}

[SmartEnum<string>]
public sealed partial class TurretChannel {
    public static readonly TurretChannel Upper = new("upper");
    public static readonly TurretChannel Lower = new("lower");
}

[SmartEnum<string>]
public sealed partial class KnurlPattern {
    public static readonly KnurlPattern Straight = new("straight", helixDeg: 0.0);
    public static readonly KnurlPattern Diamond = new("diamond", helixDeg: 30.0);
    public static readonly KnurlPattern LeftHelix = new("left-helix", helixDeg: -30.0);
    public static readonly KnurlPattern RightHelix = new("right-helix", helixDeg: 30.0);

    public double HelixDeg { get; }
}

// --- [DEMAND] -------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpindleMode {
    private SpindleMode() { }

    public sealed record ConstantSurface(double MaximumRpm) : SpindleMode;
    public sealed record ConstantRpm(double Rpm) : SpindleMode;

    public double RpmAt(double radiusMm, double minimumRadiusMm, double surfaceMetersPerMinute) => Switch(
        state: (Radius: radiusMm, Minimum: minimumRadiusMm, Surface: surfaceMetersPerMinute),
        constantSurface: static (state, mode) => double.Min(
            mode.MaximumRpm,
            1000.0 * state.Surface / (2.0 * Math.PI * double.Max(state.Radius, state.Minimum))),
        constantRpm: static (_, mode) => mode.Rpm);
}

[ComplexValueObject]
public sealed partial class TurnStock {
    public StockKind Kind { get; }
    public double OuterRadius { get; }
    public double InnerRadius { get; }
    public double AxialMinimum { get; }
    public double AxialMaximum { get; }
    public Option<Loop> Envelope { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref StockKind kind,
        ref double outerRadius,
        ref double innerRadius,
        ref double axialMinimum,
        ref double axialMaximum,
        ref Option<Loop> envelope) =>
        validationError = kind is not null
            && TensorPrimitives.IsFiniteAll<double>([outerRadius, innerRadius, axialMinimum, axialMaximum])
            && outerRadius > 0.0 && innerRadius >= 0.0 && innerRadius < outerRadius && axialMaximum > axialMinimum
            && (kind != StockKind.Solid || innerRadius == 0.0)
            && (kind != StockKind.Tube || innerRadius > 0.0)
            && (!kind.RequiresEnvelope || envelope.IsSome)
                ? null
                : new ValidationError("<turn-stock-degenerate>");
}

[ComplexValueObject]
public sealed partial class TurnInsert {
    public CutterForm Form { get; }
    public double Width { get; }
    public double ClearanceAngleDeg { get; }
    public double LeadAngleDeg { get; }
    public TipOrientation Orientation { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CutterForm form,
        ref double width,
        ref double clearanceAngleDeg,
        ref double leadAngleDeg,
        ref TipOrientation orientation) =>
        validationError = form is not null && orientation is not null
            && TensorPrimitives.IsFiniteAll<double>([width, clearanceAngleDeg, leadAngleDeg])
            && width > 0.0 && clearanceAngleDeg is > 0.0 and < 90.0 && leadAngleDeg is > -90.0 and < 90.0
                ? null
                : new ValidationError("<turn-insert-degenerate>");

    public static Fin<TurnInsert> Admit(
        CutterForm form,
        ToolAssembly assembly,
        TipOrientation orientation,
        double clearanceAngleDeg) =>
        from source in Optional(assembly)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "turn-insert:assembly").ToError())
        from width in source.Snapshot.Metric(ToolMeasure.InsertWidth)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "turn-insert:width").ToError())
        from lead in source.Snapshot.Metric(ToolMeasure.LeadAngle)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "turn-insert:lead-angle").ToError())
        from insert in Validate(form, width, clearanceAngleDeg, lead, orientation, out TurnInsert admitted) is { } error
            ? Fin.Fail<TurnInsert>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(admitted)
        select insert;
}

[ComplexValueObject]
public sealed partial class TurnPolicy {
    public double Approach { get; }
    public double Retract { get; }
    public double WidthOverlap { get; }
    public double ChordTolerance { get; }
    public double BiarcTolerance { get; }
    public int BiarcProbeFloor { get; }
    public double MinimumPeck { get; }
    public double ThreadRunIn { get; }
    public double ThreadRunout { get; }
    public double ThreadPullout { get; }
    public int SpringPasses { get; }
    public double MinimumSpindleRadius { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double approach,
        ref double retract,
        ref double widthOverlap,
        ref double chordTolerance,
        ref double biarcTolerance,
        ref int biarcProbeFloor,
        ref double minimumPeck,
        ref double threadRunIn,
        ref double threadRunout,
        ref double threadPullout,
        ref int springPasses,
        ref double minimumSpindleRadius) =>
        validationError = TensorPrimitives.IsFiniteAll<double>([
                approach, retract, widthOverlap, chordTolerance, biarcTolerance, minimumPeck,
                threadRunIn, threadRunout, threadPullout, minimumSpindleRadius])
            && approach > 0.0 && retract > 0.0 && widthOverlap is > 0.0 and < 1.0
            && chordTolerance > 0.0 && biarcTolerance > 0.0 && biarcProbeFloor >= 3
            && minimumPeck > 0.0 && threadRunIn >= 0.0 && threadRunout >= 0.0 && threadPullout > 0.0
            && springPasses >= 0 && minimumSpindleRadius > 0.0
                ? null
                : new ValidationError("<turn-policy-degenerate>");
}

[ComplexValueObject]
public sealed partial class ThreadSpec {
    public ThreadForm Form { get; }
    public ThreadHand Hand { get; }
    public CutSide Side { get; }
    public double MajorDiameter { get; }
    public double Pitch { get; }
    public int Starts { get; }
    public int RoughPasses { get; }
    public InfeedMethod Infeed { get; }
    public double ReliefDeg { get; }
    public double FirstPassMinimum { get; }
    public double FinalPass { get; }
    public double AxialStart { get; }
    public double AxialEnd { get; }
    public double Depth => Form.DepthFactor * Pitch;
    public double Lead => Pitch * Starts;
    public double CrestFlat => Form.CrestTruncationPitch * Pitch;
    public double RootFlat => Form.RootTruncationPitch * Pitch;
    public double CrestRadius => Form.CrestRadiusPitch * Pitch;
    public double RootRadius => Form.RootRadiusPitch * Pitch;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ThreadForm form,
        ref ThreadHand hand,
        ref CutSide side,
        ref double majorDiameter,
        ref double pitch,
        ref int starts,
        ref int roughPasses,
        ref InfeedMethod infeed,
        ref double reliefDeg,
        ref double firstPassMinimum,
        ref double finalPass,
        ref double axialStart,
        ref double axialEnd) =>
        validationError = form is not null && hand is not null && infeed is not null && side is not null
            && TensorPrimitives.IsFiniteAll<double>([
                majorDiameter, pitch, reliefDeg, firstPassMinimum, finalPass, axialStart, axialEnd])
            && majorDiameter > 0.0 && pitch > 0.0 && starts > 0 && roughPasses > 0
            && reliefDeg >= 0.0 && reliefDeg < double.Min(form.LoadFlankDeg, form.ClearanceFlankDeg)
            && firstPassMinimum > 0.0 && finalPass > 0.0 && axialStart != axialEnd
            && form.DepthFactor * pitch < majorDiameter / 2.0
            && finalPass < form.DepthFactor * pitch
            && firstPassMinimum <= form.DepthFactor * pitch - finalPass
                ? null
                : new ValidationError("<thread-spec-degenerate>");

    public double DepthAt(int pass) => double.Max(
        FirstPassMinimum,
        double.Min(Depth - FinalPass, (Depth - FinalPass) * Math.Sqrt((double)pass / RoughPasses)));

    public double ShiftAt(int pass) =>
        Infeed.Shift(DepthAt(pass), pass, Form.LoadFlankDeg, Form.ClearanceFlankDeg, ReliefDeg);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LatheOp {
    private LatheOp() { }

    public sealed record Rough(SweepKind Kind, double Depth, double RadialAllowance, double AxialAllowance) : LatheOp;
    public sealed record Finish(SweepKind Kind, FinishForm Form, double RadialAllowance, double AxialAllowance) : LatheOp;
    public sealed record Plunge(PlungeKind Kind, double AxialPosition, double Width, double TargetRadius, double PeckFraction, double DwellRevolutions) : LatheOp;
    public sealed record Part(double AxialPosition, double PeckFraction) : LatheOp;
    public sealed record Axial(AxialKind Kind, double Diameter, double Depth, double PeckDepth, double DwellRevolutions, double TipAngleDeg) : LatheOp;
    public sealed record Tap(double Diameter, double Depth, double Pitch, ThreadForm Form, ThreadHand Hand) : LatheOp;
    public sealed record Thread(ThreadSpec Spec) : LatheOp;
    public sealed record Knurl(KnurlPattern Pattern, double AxialStart, double AxialEnd, double Radius, double Pressure, double FeedScale) : LatheOp;
    public sealed record Transfer(double GripPlane, double GripLength, double PullDistance) : LatheOp;
    public sealed record CutoffTransfer(double GripPlane, double GripLength, double PullDistance) : LatheOp;
}

[ValueObject<string>]
public readonly partial struct ChannelToken {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length == 0 ? new ValidationError("<channel-token-empty>") : null;
    }
}

[ComplexValueObject]
public sealed partial class TurnStep {
    public SpindleSide Spindle { get; }
    public TurretChannel Channel { get; }
    public LatheOp Operation { get; }
    public Seq<ChannelToken> WaitFor { get; }
    public Option<ChannelToken> Signal { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SpindleSide spindle,
        ref TurretChannel channel,
        ref LatheOp operation,
        ref Seq<ChannelToken> waitFor,
        ref Option<ChannelToken> signal) {
        _ = waitFor;
        _ = signal;
        validationError = spindle is not null && channel is not null && operation is not null
            ? null
            : new ValidationError("<turn-step-degenerate>");
    }
}

[ComplexValueObject]
public sealed partial class TurnDemand {
    public Loop Profile { get; }
    public TurnStock Stock { get; }
    public TurnInsert Insert { get; }
    public SpindleMode Spindle { get; }
    public CuttingData Cutting { get; }
    public ProcessBudget.Turning Budget { get; }
    public TurnPolicy Policy { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Loop profile,
        ref TurnStock stock,
        ref TurnInsert insert,
        ref SpindleMode spindle,
        ref CuttingData cutting,
        ref ProcessBudget.Turning budget,
        ref TurnPolicy policy) =>
        validationError = Turning.DemandAdmission(profile, stock, insert, spindle, cutting, budget, policy).Match(
            Fail: static error => new ValidationError(error.Message),
            Succ: static _ => null);
}

[ComplexValueObject]
public sealed partial class TurnRequest {
    public TurnDemand Demand { get; }
    public Seq<TurnStep> Steps { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref TurnDemand demand,
        ref Seq<TurnStep> steps) =>
        validationError = Turning.RequestAdmission(demand, steps).Match(
            Fail: static error => new ValidationError(error.Message),
            Succ: static _ => null);
}

// --- [RECEIPTS] -----------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LatheDirective {
    private LatheDirective() { }

    public sealed record Spindle(SpindleMode Mode, double SurfaceMetersPerMinute, double ResolvedRpm) : LatheDirective;
    public sealed record Dwell(int AfterMove, double Revolutions) : LatheDirective;
    public sealed record Synchronize(int FromMove, int ToMove, double Rpm, double Lead, ThreadHand Hand, int Start, int Pass, ThreadCutRole Role) : LatheDirective;
    public sealed record ThreadGeometry(
        ThreadForm Form,
        double LoadFlankDeg,
        double ClearanceFlankDeg,
        double CrestFlat,
        double RootFlat,
        double CrestRadius,
        double RootRadius,
        CutSide Side) : LatheDirective;
    public sealed record AxialShape(int FromMove, int ToMove, AxialKind Kind, double Diameter, double Depth, double TipAngleDeg) : LatheDirective;
    public sealed record TapShape(int FromMove, int ToMove, double Diameter, double Depth, double Pitch, ThreadForm Form, ThreadHand Hand) : LatheDirective;
    public sealed record Knurl(int FromMove, int ToMove, KnurlPattern Pattern, double Pressure) : LatheDirective;
    public sealed record Handoff(SpindleSide From, SpindleSide To, double GripPlane, double GripLength, double PullDistance) : LatheDirective;
    public sealed record CutoffHandoff(SpindleSide From, SpindleSide To, double GripPlane, double GripLength, double PullDistance) : LatheDirective;
}

public sealed record ChannelBarrier(int Step, TurretChannel Channel, Seq<ChannelToken> WaitFor, Option<ChannelToken> Signal);
public sealed record RemovalEnvelope(double AxialStart, double AxialEnd, double RadiusBefore, double RadiusAfter, CutSide Side, bool RemovesMaterial);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TurnLoad {
    private TurnLoad() { }

    public sealed record Cutting(Seq<CuttingLoad> Spans) : TurnLoad;
    public sealed record Forming(double Pressure, KnurlPattern Pattern) : TurnLoad;
}

public sealed record TurnPass(int Step, SpindleSide Spindle, TurretChannel Channel, LatheOp Operation, Seq<Move> Moves, Seq<LatheDirective> Directives, Option<TurnLoad> Load, RemovalEnvelope Removal);
public sealed record TurnResidual(double OuterRadius, double InnerRadius, Arr<RemovalEnvelope> Removed);
public sealed record TurnProgram(Seq<TurnPass> Passes, Seq<ChannelBarrier> Barriers, TurnResidual Residual, BudgetEvidence Evidence);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Turning {
    public static Fin<TurnProgram> Generate(TurnRequest? request) =>
        from admitted in Optional(request).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "turning:request").ToError())
        from profile in Compensate(admitted.Demand)
        from passes in admitted.Steps.Map((step, index) => Emit(index, profile, admitted.Demand, step)).TraverseM(identity).As()
        let barriers = admitted.Steps.Map((step, index) => new ChannelBarrier(index, step.Channel, step.WaitFor, step.Signal))
        let removed = passes.Map(static pass => pass.Removal).ToArr()
        let outer = removed.Filter(static span => span.RemovesMaterial && span.Side == CutSide.External).Fold(admitted.Demand.Stock.OuterRadius, static (radius, span) => double.Min(radius, span.RadiusAfter))
        let inner = removed.Filter(static span => span.RemovesMaterial && span.Side == CutSide.Internal).Fold(admitted.Demand.Stock.InnerRadius, static (radius, span) => double.Max(radius, span.RadiusAfter))
        select new TurnProgram(passes, barriers, new TurnResidual(outer, inner, removed), admitted.Demand.Budget.Evidence);

    internal static Validation<Error, Unit> DemandAdmission(
        Loop? profile,
        TurnStock? stock,
        TurnInsert? insert,
        SpindleMode? spindle,
        CuttingData? cutting,
        ProcessBudget.Turning? budget,
        TurnPolicy? policy) =>
        (profile, stock, insert, spindle, cutting, budget, policy) switch {
            ({ } admittedProfile, { } admittedStock, { } admittedInsert, { } admittedSpindle,
                { } admittedCutting, { } admittedBudget, { }) =>
                DemandFacts(admittedProfile, admittedStock, admittedInsert, admittedSpindle, admittedCutting, admittedBudget) is var facts
                    && facts.IsEmpty
                        ? Validation<Error, Unit>.Success(unit)
                        : Validation<Error, Unit>.Fail(Error.Many([.. facts])),
            _ => Validation<Error, Unit>.Fail(Error.Many([
                .. Required(profile, "profile"),
                .. Required(stock, "stock"),
                .. Required(insert, "insert"),
                .. Required(spindle, "spindle"),
                .. Required(cutting, "cutting"),
                .. Required(budget, "budget"),
                .. Required(policy, "policy")]))
        };

    internal static Validation<Error, Unit> RequestAdmission(TurnDemand? demand, Seq<TurnStep> steps) {
        Seq<Error> facts = Required(demand, "demand") + StepFacts(-1, [(!steps.IsEmpty, "steps")]);
        Seq<Error> errors = demand is null ? facts : facts + RequestFacts(demand, steps);
        return errors.IsEmpty
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(Error.Many([.. errors]));
    }

    private static Seq<Error> DemandFacts(
        Loop profile,
        TurnStock stock,
        TurnInsert insert,
        SpindleMode spindle,
        CuttingData cutting,
        ProcessBudget.Turning budget) =>
        StepFacts(-1, [
            (!profile.Closed, "profile-open"),
            (profile.Count >= 2, "profile-span"),
            (stock.Envelope.ForAll(static loop => !loop.Closed && loop.Count >= 2), "stock-envelope"),
            (cutting.FeedBasis == FeedBasis.PerRevolution, "feed-basis"),
            (Positive(budget.SurfaceSpeed), "surface-speed"),
            (Positive(budget.FeedPerRevolution), "feed"),
            (Positive(budget.DepthOfCut), "depth"),
            (budget.NoseRadius >= 0.0, "nose-radius-positive"),
            (Math.Abs(budget.NoseRadius - insert.Form.CornerRadius) <= profile.Tolerance.Absolute.Value, "nose-radius-match")])
        + spindle.Switch(
            constantSurface: static mode => StepFacts(-1, [(Positive(mode.MaximumRpm), "spindle-css")]),
            constantRpm: static mode => StepFacts(-1, [(Positive(mode.Rpm), "spindle-rpm")]));

    private static Seq<Error> RequestFacts(TurnDemand demand, Seq<TurnStep> steps) {
        Seq<(ChannelToken Token, int Step)> signals = steps.Map((step, index) => step.Signal.Map(token => (token, index)))
            .Choose(static signal => signal);
        Seq<Error> operations = steps.Map((step, index) => OperationFacts(step.Operation, index, demand)).Bind(identity);
        Seq<Error> waits = steps.Map((step, index) => step.WaitFor.Choose(token =>
                signals.Exists(signal => signal.Token == token && signal.Step < index)
                    ? Option<Error>.None
                    : Some(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"turning:step-{index}:wait-before-signal").ToError())))
            .Bind(identity);
        Seq<Error> signalErrors = StepFacts(-1, [
            (signals.Map(static signal => signal.Token).Distinct().Count == signals.Count, "signal-duplicate")]);
        return operations + waits + signalErrors;
    }

    private static Seq<Error> OperationFacts(LatheOp operation, int index, TurnDemand demand) => operation.Switch(
        state: (Index: index, Demand: demand),
        rough: static (state, op) => StepFacts(state.Index, [
            (TensorPrimitives.IsFiniteAll<double>([op.Depth, op.RadialAllowance, op.AxialAllowance]), "sweep-finite"),
            (op.Kind is not null, "sweep-kind"),
            (op.Kind is not null && !op.Kind.Finish, "sweep-rough-kind"),
            (op.Depth > 0.0, "sweep-depth"),
            (op.RadialAllowance >= 0.0, "sweep-radial-allowance"),
            (op.AxialAllowance >= 0.0, "sweep-axial-allowance"),
            (op.Kind is null || !op.Kind.RequiresRadialStock
                || op.Kind.Side.Available(state.Demand.Stock,
                    op.Kind.Side.Target(state.Demand.Profile, op.RadialAllowance))
                    > state.Demand.Profile.Tolerance.Absolute.Value, "sweep-available")]),
        finish: static (state, op) => StepFacts(state.Index, [
            (TensorPrimitives.IsFiniteAll<double>([op.RadialAllowance, op.AxialAllowance]), "finish-finite"),
            (op.Kind is not null && op.Kind.Finish, "finish-kind"),
            (op.Form is not null, "finish-form"),
            (op.RadialAllowance >= 0.0, "finish-radial-allowance"),
            (op.AxialAllowance >= 0.0, "finish-axial-allowance")]),
        plunge: static (state, op) => StepFacts(state.Index, [
            (TensorPrimitives.IsFiniteAll<double>([op.AxialPosition, op.Width, op.TargetRadius, op.PeckFraction, op.DwellRevolutions]), "plunge-finite"),
            (op.Kind is not null, "plunge-kind"),
            (op.Width > 0.0, "plunge-width"),
            (op.TargetRadius >= state.Demand.Stock.InnerRadius && op.TargetRadius <= state.Demand.Stock.OuterRadius, "plunge-radius"),
            (op.PeckFraction is > 0.0 and <= 1.0, "plunge-peck"),
            (op.DwellRevolutions >= 0.0, "plunge-dwell"),
            (op.AxialPosition >= state.Demand.Stock.AxialMinimum, "plunge-start"),
            (op.AxialPosition + op.Width <= state.Demand.Stock.AxialMaximum, "plunge-end")]),
        part: static (state, op) => StepFacts(state.Index, [
            (TensorPrimitives.IsFiniteAll<double>([op.AxialPosition, op.PeckFraction]), "part-finite"),
            (op.AxialPosition >= state.Demand.Stock.AxialMinimum, "part-start"),
            (op.AxialPosition + state.Demand.Insert.Width <= state.Demand.Stock.AxialMaximum, "part-end"),
            (op.PeckFraction is > 0.0 and <= 1.0, "part-peck")]),
        axial: static (state, op) => StepFacts(state.Index, [
            (TensorPrimitives.IsFiniteAll<double>([op.Diameter, op.Depth, op.PeckDepth, op.DwellRevolutions, op.TipAngleDeg]), "axial-finite"),
            (op.Kind is not null, "axial-kind"),
            (op.Diameter > 0.0 && op.Diameter / 2.0 <= state.Demand.Stock.OuterRadius, "axial-diameter"),
            (op.Depth > 0.0 && op.Depth <= state.Demand.Stock.AxialMaximum - state.Demand.Stock.AxialMinimum, "axial-depth"),
            (op.PeckDepth > 0.0, "axial-peck"),
            (op.DwellRevolutions >= 0.0, "axial-dwell"),
            (op.TipAngleDeg is > 0.0 and <= 180.0, "axial-tip-angle")]),
        tap: static (state, op) => StepFacts(state.Index, [
            (TensorPrimitives.IsFiniteAll<double>([op.Diameter, op.Depth, op.Pitch]), "tap-finite"),
            (op.Diameter > 0.0 && op.Diameter / 2.0 <= state.Demand.Stock.OuterRadius, "tap-diameter"),
            (op.Depth > 0.0 && op.Depth <= state.Demand.Stock.AxialMaximum - state.Demand.Stock.AxialMinimum, "tap-depth"),
            (op.Pitch > 0.0, "tap-pitch"),
            (op.Form is not null, "tap-form"),
            (op.Hand is not null, "tap-hand")]),
        thread: static (state, op) => op.Spec is null
            ? StepFacts(state.Index, [(false, "thread-spec")])
            : StepFacts(state.Index, [
                (op.Spec.FinalPass < op.Spec.Depth, "thread-finish"),
                (op.Spec.Side == CutSide.Internal
                    ? op.Spec.MajorDiameter / 2.0 <= state.Demand.Stock.OuterRadius
                        && op.Spec.MajorDiameter / 2.0 - op.Spec.Depth >= state.Demand.Stock.InnerRadius
                    : op.Spec.MajorDiameter / 2.0 <= state.Demand.Stock.OuterRadius, "thread-diameter"),
                (double.Min(op.Spec.AxialStart, op.Spec.AxialEnd) >= state.Demand.Stock.AxialMinimum, "thread-start"),
                (double.Max(op.Spec.AxialStart, op.Spec.AxialEnd) <= state.Demand.Stock.AxialMaximum, "thread-end")]),
        knurl: static (state, op) => StepFacts(state.Index, [
            (TensorPrimitives.IsFiniteAll<double>([op.AxialStart, op.AxialEnd, op.Radius, op.Pressure, op.FeedScale]), "knurl-finite"),
            (op.Pattern is not null, "knurl-pattern"),
            (op.AxialStart != op.AxialEnd, "knurl-span"),
            (op.Radius > 0.0 && op.Radius <= state.Demand.Stock.OuterRadius, "knurl-radius"),
            (op.Pressure > 0.0, "knurl-pressure"),
            (op.FeedScale is > 0.0 and <= 1.0, "knurl-feed"),
            (double.Min(op.AxialStart, op.AxialEnd) >= state.Demand.Stock.AxialMinimum, "knurl-start"),
            (double.Max(op.AxialStart, op.AxialEnd) <= state.Demand.Stock.AxialMaximum, "knurl-end")]),
        transfer: static (state, op) => StepFacts(state.Index, [
            (TensorPrimitives.IsFiniteAll<double>([op.GripPlane, op.GripLength, op.PullDistance]), "transfer-finite"),
            (op.GripLength > 0.0, "transfer-grip"),
            (op.PullDistance >= 0.0, "transfer-pull"),
            (op.GripPlane >= state.Demand.Stock.AxialMinimum, "transfer-start"),
            (op.GripPlane + op.GripLength <= state.Demand.Stock.AxialMaximum, "transfer-end")]),
        cutoffTransfer: static (state, op) => StepFacts(state.Index, [
            (TensorPrimitives.IsFiniteAll<double>([op.GripPlane, op.GripLength, op.PullDistance]), "cutoff-transfer-finite"),
            (op.GripLength > 0.0, "cutoff-transfer-grip"),
            (op.PullDistance >= 0.0, "cutoff-transfer-pull"),
            (op.GripPlane >= state.Demand.Stock.AxialMinimum, "cutoff-transfer-start"),
            (op.GripPlane + double.Max(op.GripLength, state.Demand.Insert.Width) <= state.Demand.Stock.AxialMaximum, "cutoff-transfer-end") ]));

    private static Seq<Error> Required<T>(T? value, string axis) where T : class => value is null
        ? Seq(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"turning:{axis}").ToError())
        : Seq<Error>();

    private static Seq<Error> StepFacts(int step, ReadOnlySpan<(bool Ok, string Axis)> facts) =>
        facts.ToArray().ToSeq().Choose(fact => fact.Ok
            ? Option<Error>.None
            : Some(new GeometryFault.DegenerateInput(Kind.Curve, -1, step < 0 ? $"turning:{fact.Axis}" : $"turning:step-{step}:{fact.Axis}").ToError()));

    private static Fin<Loop> Compensate(TurnDemand demand) =>
        Range(0, demand.Profile.Count - 1)
            .Choose(index => Clearance(demand.Profile.At(index + 1) - demand.Profile.At(index))
                > 90.0 - demand.Insert.ClearanceAngleDeg - Math.Abs(demand.Insert.LeadAngleDeg)
                    ? Some(FabricationFault.Gouge(demand.Profile.At(index), demand.Insert.Form).ToError())
                    : Option<Error>.None)
            .ToSeq()
            .Apply(gouges => gouges.IsEmpty
                ? Loop.Admit(
                    demand.Profile.Vertices.Map((point, index) => Compensated(demand.Profile, demand.Insert, point, index)).ToArr(),
                    closed: false,
                    demand.Profile.Bulges,
                    demand.Profile.Tolerance)
                : Fin.Fail<Loop>(Error.Many([.. gouges])));

    private static Point3d Compensated(Loop profile, TurnInsert insert, Point3d point, int index) =>
        (profile.At(int.Min(index + 1, profile.Count - 1)) - profile.At(int.Max(index - 1, 0)))
            .Apply(static span => span.Unitize() ? span : Vector3d.XAxis)
            .Apply(tangent => new Vector3d(-tangent.Y, tangent.X, 0.0)
                * (insert.Orientation.Radial < 0 ? -1.0 : 1.0))
            .Apply(normal => point + (insert.Form.CornerRadius * normal)
                - (new Vector3d(insert.Orientation.Axial, insert.Orientation.Radial, 0.0) * insert.Form.CornerRadius));

    private static double Clearance(Vector3d span) => Math.Abs(Math.Atan2(span.Y, Math.Abs(span.X))) * 180.0 / Math.PI;

    private static Fin<TurnPass> Emit(int index, Loop profile, TurnDemand demand, TurnStep step) => step.Operation.Switch(
        state: (Index: index, Profile: profile, Demand: demand, Step: step),
        rough: static (state, op) => Rough(state.Index, state.Profile, state.Demand, state.Step, op),
        finish: static (state, op) => Finish(state.Index, state.Profile, state.Demand, state.Step, op),
        plunge: static (state, op) => Plunge(state.Index, state.Demand, state.Step, op),
        part: static (state, op) => Part(state.Index, state.Demand, state.Step, op),
        axial: static (state, op) => Axial(state.Index, state.Demand, state.Step, op),
        tap: static (state, op) => Tap(state.Index, state.Demand, state.Step, op),
        thread: static (state, op) => Thread(state.Index, state.Demand, state.Step, op.Spec),
        knurl: static (state, op) => Knurl(state.Index, state.Demand, state.Step, op),
        transfer: static (state, op) => Transfer(state.Index, state.Demand, state.Step, op),
        cutoffTransfer: static (state, op) => CutoffTransfer(state.Index, state.Demand, state.Step, op));

    private static Fin<TurnPass> Rough(int index, Loop profile, TurnDemand demand, TurnStep step, LatheOp.Rough op) =>
        from moves in op.Kind.Sweep(profile, demand,
            new SweepDemand(op.Depth, op.RadialAllowance, op.AxialAllowance))
        from pass in Loaded(index, demand, step, moves, Seq<LatheDirective>(),
            Envelope(profile, demand, op.Kind.Side, op.RadialAllowance, op.AxialAllowance))
        select pass;

    private static Fin<TurnPass> Finish(int index, Loop profile, TurnDemand demand, TurnStep step, LatheOp.Finish op) =>
        from moves in FinishMoves(profile, demand, op)
        from pass in Loaded(index, demand, step, moves, Seq<LatheDirective>(),
            Envelope(profile, demand, op.Kind.Side, op.RadialAllowance, op.AxialAllowance))
        select pass;

    internal static Fin<Seq<Move>> Longitudinal(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) =>
        side.Target(profile, sweep.RadialAllowance)
            .Apply(target => Range(1, (int)Math.Ceiling(side.Available(demand.Stock, target) / sweep.Depth))
                .Map(pass => side.Advance(demand.Stock, target, pass, sweep.Depth)
                    .Apply(radius => Crossings(profile, radius, demand.Stock.AxialMaximum + demand.Policy.Approach)
                        .Map(spans => spans.Bind(span => Seq(
                            (Move)new Move.Rapid(new Point3d(span.Start, side.Clear(radius, demand.Policy.Retract), 0.0)),
                            new Move.Linear(new Point3d(span.Start, radius, 0.0), Feed(demand, radius)),
                            new Move.Linear(new Point3d(span.End + sweep.AxialAllowance, radius, 0.0), Feed(demand, radius)),
                            new Move.Rapid(new Point3d(
                                span.End + sweep.AxialAllowance,
                                side.Clear(radius, demand.Policy.Retract),
                                0.0)))))))
                .TraverseM(identity)
                .As()
                .Map(static passes => passes.Bind(identity).ToSeq()));

    internal static Seq<Move> Facing(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) =>
        (Target: profile.Vertices.Map(static point => point.X).Min() + sweep.AxialAllowance)
            .Apply(target => Range(1, int.Max(1, (int)Math.Ceiling((demand.Stock.AxialMaximum - target) / sweep.Depth)))
                .Bind(pass => (Axial: double.Max(target, demand.Stock.AxialMaximum - (pass * sweep.Depth)))
                    .Apply(axial => (Axial: axial, Radius: RadiusAt(profile, axial, demand.Stock.InnerRadius)))
                    .Apply(station => Seq(
                        (Move)new Move.Rapid(new Point3d(
                            station.Axial,
                            side.Clear(side.StockRadius(demand.Stock), demand.Policy.Approach),
                            0.0)),
                        new Move.Linear(new Point3d(station.Axial, station.Radius, 0.0), Feed(demand, side.StockRadius(demand.Stock))),
                        new Move.Rapid(new Point3d(station.Axial + demand.Policy.Retract, station.Radius, 0.0))))));

    internal static Seq<Move> Pattern(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) =>
        (Source: demand.Stock.Envelope.IfNone(profile),
            Passes: int.Max(1, (int)Math.Ceiling(double.Max(sweep.RadialAllowance, sweep.AxialAllowance) / sweep.Depth)),
            Park: new Point3d(
                demand.Stock.AxialMaximum + demand.Policy.Approach,
                side.Clear(side.StockRadius(demand.Stock), demand.Policy.Approach),
                0.0))
            .Apply(state => Range(1, state.Passes).Bind(pass =>
                ((double)(state.Passes - pass) / state.Passes)
                    .Apply(fraction => (Radial: sweep.RadialAllowance * fraction, Axial: sweep.AxialAllowance * fraction))
                    .Apply(shift => Seq((Move)new Move.Rapid(state.Park))
                        + Native(state.Source, demand, new SweepDemand(sweep.Depth, shift.Radial, shift.Axial), side)
                        + Seq((Move)new Move.Rapid(new Point3d(
                            state.Source.Vertices.Map(static point => point.X).Max() + shift.Axial,
                            side.Clear(side.StockRadius(demand.Stock), demand.Policy.Retract),
                            0.0))))));

    internal static Seq<Move> Native(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) =>
        Seq((Move)new Move.Rapid(new Point3d(
            profile.At(0).X + sweep.AxialAllowance,
            side.Clear(profile.At(0).Y + sweep.RadialAllowance, demand.Policy.Approach),
            0.0)))
        + Range(0, profile.Spans).Map(index => profile.At(index + 1)
            .Apply(target => profile.BulgeAt(index) == 0.0
                ? (Move)new Move.Linear(
                    new Point3d(target.X + sweep.AxialAllowance, target.Y + sweep.RadialAllowance, 0.0),
                    Feed(demand, target.Y))
                : new Move.Circular(
                    new Point3d(target.X + sweep.AxialAllowance, target.Y + sweep.RadialAllowance, 0.0),
                    Feed(demand, target.Y),
                    ArcOf(profile, index, sweep.RadialAllowance, sweep.AxialAllowance))));

    private static Fin<Seq<Move>> FinishMoves(Loop profile, TurnDemand demand, LatheOp.Finish op) =>
        new SweepDemand(0.0, op.RadialAllowance, op.AxialAllowance)
            .Apply(sweep => op.Form.Switch(
                state: (Profile: profile, Demand: demand, Kind: op.Kind, Sweep: sweep),
                nativeArc: static state => state.Kind.Sweep(state.Profile, state.Demand, state.Sweep),
                spline: static state => Spline(state.Profile, state.Demand, state.Sweep, state.Kind.Side),
                biarc: static state => Biarc(state.Profile, state.Demand, state.Sweep, state.Kind.Side)));

    private static Fin<Seq<Move>> Spline(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) =>
        from fitted in CurveAlgebra.Apply(new CurveOp.Admit(
            new CurveSource.Outline(profile, demand.Policy.ChordTolerance, FitPolicy.Canonical),
            null))
        from curve in fitted is CurveTrace.Fitted fit
            ? Fin.Succ(fit.Curve)
            : Fin.Fail<NurbsForm.Curve>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "turning:spline-fit").ToError())
        from lowered in CurveAlgebra.Apply(new CurveOp.Lower(
            curve,
            new CurveLowering.Chords(new DivideRule.ByChord(demand.Policy.ChordTolerance)),
            profile.Tolerance,
            null))
        from loop in lowered is CurveTrace.Lowered result
            ? Fin.Succ(result.Loop)
            : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "turning:spline-lower").ToError())
        select Native(loop, demand, sweep, side);

    private static Fin<Seq<Move>> Biarc(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) =>
        from chunks in Range(0, int.Max(1, profile.Count / 2))
            .Map(chunk => BiarcChunk(profile, demand, sweep, chunk))
            .TraverseM(identity)
            .As()
        select Seq((Move)new Move.Rapid(new Point3d(
            profile.At(0).X + sweep.AxialAllowance,
            side.Clear(profile.At(0).Y + sweep.RadialAllowance, demand.Policy.Approach),
            0.0))) + chunks.Bind(identity);

    private static Fin<Seq<Move>> BiarcChunk(Loop profile, TurnDemand demand, SweepDemand sweep, int chunk) {
        int first = int.Min(chunk * 2, profile.Count - 2);
        int last = int.Min(first + 2, profile.Count - 1);
        Point3d p0 = profile.At(first);
        Point3d pm = profile.At(int.Min(first + 1, last));
        Point3d p1 = profile.At(last);
        if (p0.DistanceTo(pm) <= profile.Tolerance.Absolute.Value
            || pm.DistanceTo(p1) <= profile.Tolerance.Absolute.Value) {
            return Fin.Succ(Seq((Move)new Move.Linear(
                new Point3d(p1.X + sweep.AxialAllowance, p1.Y + sweep.RadialAllowance, 0.0),
                Feed(demand, p1.Y))));
        }
        Vector2d t0 = new Vector2d(pm.X - p0.X, pm.Y - p0.Y).Normalized;
        Vector2d t1 = new Vector2d(p1.X - pm.X, p1.Y - pm.Y).Normalized;
        BiArcFit2 fit = new(new Vector2d(p0.X, p0.Y), t0, new Vector2d(p1.X, p1.Y), t1);
        double deviation = Range(0, demand.Policy.BiarcProbeFloor).Map(probe => {
            double parameter = (double)probe / (demand.Policy.BiarcProbeFloor - 1);
            double local = parameter <= 0.5 ? parameter * 2.0 : (parameter - 0.5) * 2.0;
            Point3d source = parameter <= 0.5
                ? p0 + local * (pm - p0)
                : pm + local * (p1 - pm);
            return fit.Distance(new Vector2d(source.X, source.Y));
        }).Max();
        return deviation > demand.Policy.BiarcTolerance
            ? Fin.Succ(Seq(
                (Move)new Move.Linear(new Point3d(pm.X + sweep.AxialAllowance, pm.Y + sweep.RadialAllowance, 0.0), Feed(demand, pm.Y)),
                new Move.Linear(new Point3d(p1.X + sweep.AxialAllowance, p1.Y + sweep.RadialAllowance, 0.0), Feed(demand, p1.Y))))
            : fit.Curves.ToSeq().Map(curve => Span(curve, demand, sweep)).TraverseM(identity).As();
    }

    private static Fin<Move> Span(IParametricCurve2d curve, TurnDemand demand, SweepDemand sweep) => curve switch {
        Segment2d segment => Fin.Succ<Move>(new Move.Linear(
            new Point3d(segment.P1.x + sweep.AxialAllowance, segment.P1.y + sweep.RadialAllowance, 0.0),
            Feed(demand, segment.P1.y))),
        Arc2d arc => ArcSpan(arc, demand, sweep),
        _ => Fin.Fail<Move>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "turning:biarc-span").ToError())
    };

    private static Fin<Move> ArcSpan(Arc2d arc, TurnDemand demand, SweepDemand sweep) {
        Vector2d endpoint = arc.SampleT(1.0);
        return Fin.Succ<Move>(new Move.Circular(
            new Point3d(endpoint.x + sweep.AxialAllowance, endpoint.y + sweep.RadialAllowance, 0.0),
            Feed(demand, endpoint.y),
            new ArcCenter(
                new Point3d(arc.Center.x + sweep.AxialAllowance, arc.Center.y + sweep.RadialAllowance, 0.0),
                arc.IsReversed ? RotationSense.Clockwise : RotationSense.Counterclockwise)));
    }

    private static ArcCenter ArcOf(Loop profile, int index, double radialShift, double axialShift) {
        double bulge = profile.BulgeAt(index);
        Point3d start = profile.At(index);
        Point3d end = profile.At(index + 1);
        Vector3d chord = end - start;
        Vector3d normal = new(-chord.Y, chord.X, 0.0);
        normal.Unitize();
        Point3d center = new Point3d((start.X + end.X) / 2.0, (start.Y + end.Y) / 2.0, 0.0)
            + normal * chord.Length * (1.0 - bulge * bulge) / (4.0 * bulge);
        return new ArcCenter(
            center + new Vector3d(axialShift, radialShift, 0.0),
            bulge < 0.0 ? RotationSense.Clockwise : RotationSense.Counterclockwise);
    }

    private static Fin<TurnPass> Plunge(int index, TurnDemand demand, TurnStep step, LatheOp.Plunge op) {
        double increment = demand.Insert.Width * (1.0 - demand.Policy.WidthOverlap);
        int bands = int.Max(1, (int)Math.Ceiling(double.Max(op.Width - demand.Insert.Width, 0.0) / increment) + 1);
        int pecks = int.Max(1, (int)Math.Ceiling(1.0 / op.PeckFraction));
        Seq<Move> moves = Range(0, bands).Bind(band => {
            double axial = op.AxialPosition + double.Min(band * increment, double.Max(op.Width - demand.Insert.Width, 0.0));
            return Seq((Move)new Move.Rapid(new Point3d(axial, demand.Stock.OuterRadius + demand.Policy.Approach, 0.0)))
            + Range(1, pecks).Bind(peck => {
                double radius = demand.Stock.OuterRadius - (demand.Stock.OuterRadius - op.TargetRadius) * peck / pecks;
                return Seq(
                    (Move)new Move.Linear(new Point3d(axial, radius, 0.0), Feed(demand, radius)),
                    new Move.Rapid(new Point3d(axial, radius + demand.Policy.Retract, 0.0)));
            });
        });
        Seq<LatheDirective> directives = op.DwellRevolutions > 0.0
            ? Range(0, bands).Map(band => (LatheDirective)new LatheDirective.Dwell(
                band * (pecks * 2 + 1) + pecks * 2 - 1,
                op.DwellRevolutions))
            : Seq<LatheDirective>();
        return Loaded(index, demand, step, moves, directives, new RemovalEnvelope(
            op.AxialPosition,
            op.AxialPosition + op.Width,
            demand.Stock.OuterRadius,
            op.TargetRadius,
            CutSide.External,
            RemovesMaterial: true));
    }

    private static Fin<TurnPass> Part(int index, TurnDemand demand, TurnStep step, LatheOp.Part op) {
        int pecks = int.Max(1, (int)Math.Ceiling(1.0 / op.PeckFraction));
        Seq<Move> moves = Seq((Move)new Move.Rapid(new Point3d(
                op.AxialPosition,
                demand.Stock.OuterRadius + demand.Policy.Approach,
                0.0)))
            + Range(1, pecks).Bind(peck => {
                double radius = demand.Stock.OuterRadius
                    - (demand.Stock.OuterRadius - demand.Stock.InnerRadius) * peck / pecks;
                return Seq(
                    (Move)new Move.Linear(new Point3d(op.AxialPosition, radius, 0.0), Feed(demand, radius)),
                    new Move.Rapid(new Point3d(op.AxialPosition, radius + demand.Policy.Retract, 0.0)));
            });
        return Loaded(index, demand, step, moves, Seq<LatheDirective>(), new RemovalEnvelope(
            op.AxialPosition,
            op.AxialPosition + demand.Insert.Width,
            demand.Stock.OuterRadius,
            demand.Stock.InnerRadius,
            CutSide.External,
            RemovesMaterial: true));
    }

    private static Fin<TurnPass> Axial(int index, TurnDemand demand, TurnStep step, LatheOp.Axial op) =>
        (Face: demand.Stock.AxialMaximum, Start: demand.Stock.AxialMaximum + demand.Policy.Approach)
            .Apply(station => (Station: station, Moves: Seq((Move)new Move.Rapid(new Point3d(station.Start, 0.0, 0.0)))
                + op.Kind.Depths(op.Depth, double.Max(op.PeckDepth, demand.Policy.MinimumPeck))
                    .Bind(depth => Seq(
                        (Move)new Move.Linear(new Point3d(station.Face - depth, 0.0, 0.0), Feed(demand, op.Diameter / 2.0)),
                        new Move.Rapid(new Point3d(station.Start, 0.0, 0.0))))))
            .Apply(state => Loaded(index, demand, step, state.Moves,
                Seq<LatheDirective>(new LatheDirective.AxialShape(
                    0, state.Moves.Count - 1, op.Kind, op.Diameter, op.Depth, op.TipAngleDeg))
                + (op.DwellRevolutions > 0.0
                    ? Seq<LatheDirective>(new LatheDirective.Dwell(state.Moves.Count - 2, op.DwellRevolutions))
                    : Seq<LatheDirective>()),
                new RemovalEnvelope(
                    state.Station.Face - op.Depth,
                    state.Station.Face,
                    demand.Stock.InnerRadius,
                    op.Diameter / 2.0,
                    CutSide.Internal,
                    RemovesMaterial: true)));

    private static Fin<TurnPass> Tap(int index, TurnDemand demand, TurnStep step, LatheOp.Tap op) {
        double face = demand.Stock.AxialMaximum;
        double start = face + demand.Policy.Approach;
        double radius = op.Diameter / 2.0;
        double rpm = demand.Spindle.RpmAt(radius, demand.Policy.MinimumSpindleRadius, Surface(demand));
        Seq<Move> moves = Seq(
            (Move)new Move.Rapid(new Point3d(start, 0.0, 0.0)),
            new Move.Linear(new Point3d(face - op.Depth, 0.0, 0.0), op.Pitch * rpm),
            new Move.Linear(new Point3d(start, 0.0, 0.0), op.Pitch * rpm));
        Seq<LatheDirective> directives = Seq<LatheDirective>(
            new LatheDirective.Spindle(new SpindleMode.ConstantRpm(rpm), Surface(demand), rpm),
            new LatheDirective.TapShape(1, 2, op.Diameter, op.Depth, op.Pitch, op.Form, op.Hand),
            new LatheDirective.Synchronize(1, 2, rpm, op.Pitch, op.Hand, 0, 0, ThreadCutRole.Finish));
        return Loaded(index, demand, step, moves, directives, new RemovalEnvelope(
            face - op.Depth,
            face,
            demand.Stock.InnerRadius,
            radius,
            CutSide.Internal,
            RemovesMaterial: true));
    }

    private static Fin<TurnPass> Thread(int index, TurnDemand demand, TurnStep step, ThreadSpec spec) {
        Seq<(int Start, int Pass, ThreadCutRole Role, double Depth, double Shift)> cuts = Range(0, spec.Starts).Bind(start =>
            Range(1, spec.RoughPasses).Map(pass => (start, pass, ThreadCutRole.Rough, spec.DepthAt(pass), spec.ShiftAt(pass)))
            + Seq((start, spec.RoughPasses + 1, ThreadCutRole.Finish, spec.Depth, 0.0))
            + Range(1, demand.Policy.SpringPasses).Map(pass => (start, spec.RoughPasses + 1 + pass, ThreadCutRole.Spring, spec.Depth, 0.0)));
        double majorRadius = spec.MajorDiameter / 2.0;
        double rpm = demand.Spindle.RpmAt(majorRadius, demand.Policy.MinimumSpindleRadius, Surface(demand));
        double axialDirection = Math.Sign(spec.AxialEnd - spec.AxialStart);
        Seq<Move> moves = cuts.Bind(cut => {
            double radius = spec.Side == CutSide.Internal
                ? majorRadius - spec.Depth + cut.Depth
                : majorRadius - cut.Depth;
            double indexedStart = spec.AxialStart + axialDirection * cut.Start * spec.Pitch + cut.Shift;
            double indexedEnd = spec.AxialEnd + axialDirection * cut.Start * spec.Pitch + cut.Shift;
            double entry = indexedStart - axialDirection * demand.Policy.ThreadRunIn;
            double exit = indexedEnd + axialDirection * demand.Policy.ThreadRunout;
            return Seq(
                (Move)new Move.Rapid(new Point3d(entry, spec.Side.Clear(radius, demand.Policy.ThreadPullout), 0.0)),
                new Move.Linear(new Point3d(entry, radius, 0.0), Feed(demand, radius)),
                new Move.Linear(new Point3d(exit, radius, 0.0), spec.Lead * rpm),
                new Move.Rapid(new Point3d(exit, spec.Side.Clear(radius, demand.Policy.ThreadPullout), 0.0)));
        });
        Seq<LatheDirective> directives = Seq<LatheDirective>(
                new LatheDirective.Spindle(new SpindleMode.ConstantRpm(rpm), Surface(demand), rpm),
                new LatheDirective.ThreadGeometry(
                    spec.Form,
                    spec.Form.LoadFlankDeg,
                    spec.Form.ClearanceFlankDeg,
                    spec.CrestFlat,
                    spec.RootFlat,
                    spec.CrestRadius,
                    spec.RootRadius,
                    spec.Side))
            + cuts.Map((cut, pass) => (LatheDirective)new LatheDirective.Synchronize(
            pass * 4 + 1,
            pass * 4 + 2,
            rpm,
            spec.Lead,
            spec.Hand,
            cut.Start,
            cut.Pass,
            cut.Role));
        return Loaded(index, demand, step, moves, directives, new RemovalEnvelope(
            double.Min(spec.AxialStart, spec.AxialEnd),
            double.Max(spec.AxialStart, spec.AxialEnd),
            spec.Side == CutSide.Internal ? majorRadius - spec.Depth : majorRadius,
            spec.Side == CutSide.Internal ? majorRadius : majorRadius - spec.Depth,
            spec.Side,
            RemovesMaterial: true));
    }

    private static Fin<TurnPass> Knurl(int index, TurnDemand demand, TurnStep step, LatheOp.Knurl op) {
        Seq<Move> moves = Seq(
            (Move)new Move.Rapid(new Point3d(op.AxialStart, op.Radius + demand.Policy.Approach, 0.0)),
            new Move.Linear(new Point3d(op.AxialStart, op.Radius, 0.0), Feed(demand, op.Radius) * op.FeedScale),
            new Move.Linear(new Point3d(op.AxialEnd, op.Radius, 0.0), Feed(demand, op.Radius) * op.FeedScale),
            new Move.Rapid(new Point3d(op.AxialEnd, op.Radius + demand.Policy.Retract, 0.0)));
        return Loaded(index, demand, step, moves,
            Seq<LatheDirective>(new LatheDirective.Knurl(1, 2, op.Pattern, op.Pressure)),
            new RemovalEnvelope(
                double.Min(op.AxialStart, op.AxialEnd),
                double.Max(op.AxialStart, op.AxialEnd),
                op.Radius,
                op.Radius,
                CutSide.External,
                RemovesMaterial: false),
            Some<TurnLoad>(new TurnLoad.Forming(op.Pressure, op.Pattern)));
    }

    private static Fin<TurnPass> Transfer(int index, TurnDemand demand, TurnStep step, LatheOp.Transfer op) =>
        Loaded(index, demand, step, Seq<Move>(),
            Seq<LatheDirective>(new LatheDirective.Handoff(
                step.Spindle,
                step.Spindle.Opposite,
                op.GripPlane,
                op.GripLength,
                op.PullDistance)),
            new RemovalEnvelope(
                op.GripPlane,
                op.GripPlane + op.GripLength,
                demand.Stock.OuterRadius,
                demand.Stock.OuterRadius,
                CutSide.External,
                RemovesMaterial: false));

    private static Fin<TurnPass> CutoffTransfer(
        int index,
        TurnDemand demand,
        TurnStep step,
        LatheOp.CutoffTransfer op) {
        Seq<Move> moves = Seq(
            (Move)new Move.Rapid(new Point3d(
                op.GripPlane,
                demand.Stock.OuterRadius + demand.Policy.Approach,
                0.0)),
            new Move.Linear(new Point3d(op.GripPlane, demand.Stock.InnerRadius, 0.0),
                Feed(demand, demand.Stock.InnerRadius)),
            new Move.Rapid(new Point3d(
                op.GripPlane,
                demand.Stock.OuterRadius + demand.Policy.Retract,
                0.0)));
        Seq<LatheDirective> directives = Seq<LatheDirective>(new LatheDirective.CutoffHandoff(
            step.Spindle,
            step.Spindle.Opposite,
            op.GripPlane,
            op.GripLength,
            op.PullDistance));
        return Loaded(index, demand, step, moves, directives, new RemovalEnvelope(
            op.GripPlane,
            op.GripPlane + demand.Insert.Width,
            demand.Stock.OuterRadius,
            demand.Stock.InnerRadius,
            CutSide.External,
            RemovesMaterial: true));
    }

    private static Fin<TurnPass> Loaded(
        int index,
        TurnDemand demand,
        TurnStep step,
        Seq<Move> moves,
        Seq<LatheDirective> directives,
        RemovalEnvelope removal,
        Option<TurnLoad> stated = default) {
        Seq<Move> projected = moves.Map(move => Project(move, step.Spindle));
        RemovalEnvelope projectedRemoval = Project(removal, step.Spindle);
        Seq<(double Radius, double Feed)> spans = projected.Choose(move => move.Switch(
                state: Math.Abs(projectedRemoval.RadiusAfter),
                rapid: static (_, _) => Option<(double Radius, double Feed)>.None,
                linear: static (minimum, row) => Some((double.Max(minimum, Math.Abs(row.Target.Y)), row.Feed)),
                circular: static (minimum, row) => Some((double.Max(minimum, Math.Abs(row.Target.Y)), row.Feed))));
        double radius = spans.Map(static span => span.Radius)
            .Fold(demand.Policy.MinimumSpindleRadius, double.Max);
        double resolvedRpm = demand.Spindle.RpmAt(radius, demand.Policy.MinimumSpindleRadius, Surface(demand));
        double radialDepth = double.Max(
            demand.Profile.Tolerance.Absolute.Value,
            double.Min(Math.Abs(projectedRemoval.RadiusBefore - projectedRemoval.RadiusAfter), demand.Budget.DepthOfCut));
        double axialDepth = double.Max(
            demand.Profile.Tolerance.Absolute.Value,
            double.Min(Math.Abs(projectedRemoval.AxialEnd - projectedRemoval.AxialStart), demand.Insert.Width));
        double chipWidth = double.Max(
            demand.Profile.Tolerance.Absolute.Value,
            double.Min(demand.Insert.Width, double.Max(radialDepth, axialDepth)));
        Seq<LatheDirective> resolved = directives.Exists(static directive => directive is LatheDirective.Spindle)
            ? directives
            : new LatheDirective.Spindle(demand.Spindle, Surface(demand), resolvedRpm).Cons(directives);
        return stated.Match(
            Some: load => Fin.Succ(new TurnPass(
                index, step.Spindle, step.Channel, step.Operation, projected, resolved, Some(load), projectedRemoval)),
            None: () => removal.RemovesMaterial
                ? from _ in guard(!spans.IsEmpty, new GeometryFault.DegenerateInput(Kind.Curve, -1, "turning:cutting-span").ToError()).ToFin()
                  from loads in spans.Map(span =>
                          from intent in Intent(demand, span, chipWidth, axialDepth, radialDepth)
                          from load in demand.Cutting.Evaluate(intent)
                          select load)
                      .TraverseM(identity)
                      .As()
                  select new TurnPass(
                      index, step.Spindle, step.Channel, step.Operation, projected, resolved,
                      Some<TurnLoad>(new TurnLoad.Cutting(loads)), projectedRemoval)
                : Fin.Succ(new TurnPass(
                    index, step.Spindle, step.Channel, step.Operation, projected, resolved, None, projectedRemoval)));
    }

    private static Fin<CutIntent> Intent(
        TurnDemand demand,
        (double Radius, double Feed) span,
        double chipWidth,
        double axialDepth,
        double radialDepth) =>
        CutIntent.Validate(
            Length.FromMillimeters(double.Min(demand.Cutting.Feed, demand.Budget.FeedPerRevolution)),
            Length.FromMillimeters(chipWidth),
            Length.FromMillimeters(axialDepth),
            Length.FromMillimeters(radialDepth),
            Length.FromMillimeters(double.Max(span.Radius * 2.0, demand.Policy.MinimumSpindleRadius * 2.0)),
            teeth: SingleEdge,
            RotationalSpeed.FromRevolutionsPerMinute(
                demand.Spindle.RpmAt(span.Radius, demand.Policy.MinimumSpindleRadius, Surface(demand))),
            Speed.FromMillimetersPerMinutes(span.Feed),
            out CutIntent intent) is { } error
                ? Fin.Fail<CutIntent>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
                : Fin.Succ(intent);

    private static Move Project(Move move, SpindleSide side) => move.Switch(
        state: side.AxialSign,
        rapid: static (sign, row) => new Move.Rapid(new Point3d(row.Target.X * sign, row.Target.Y, row.Target.Z)),
        linear: static (sign, row) => new Move.Linear(new Point3d(row.Target.X * sign, row.Target.Y, row.Target.Z), row.Feed),
        circular: static (sign, row) => new Move.Circular(
            new Point3d(row.Target.X * sign, row.Target.Y, row.Target.Z),
            row.Feed,
            new ArcCenter(
                new Point3d(row.Arc.Center.X * sign, row.Arc.Center.Y, row.Arc.Center.Z),
                sign > 0 ? row.Arc.Sense : row.Arc.Sense == RotationSense.Clockwise
                    ? RotationSense.Counterclockwise
                    : RotationSense.Clockwise)));

    private static RemovalEnvelope Project(RemovalEnvelope removal, SpindleSide side) {
        double start = removal.AxialStart * side.AxialSign;
        double end = removal.AxialEnd * side.AxialSign;
        return removal with { AxialStart = double.Min(start, end), AxialEnd = double.Max(start, end) };
    }

    private static RemovalEnvelope Envelope(
        Loop profile,
        TurnDemand demand,
        CutSide side,
        double radialAllowance,
        double axialAllowance) =>
        new(profile.Vertices.Map(static point => point.X).Min() + axialAllowance,
            profile.Vertices.Map(static point => point.X).Max() + axialAllowance,
            side.StockRadius(demand.Stock),
            side.Target(profile, radialAllowance),
            side,
            RemovesMaterial: true);

    private static Fin<Seq<(double Start, double End)>> Crossings(Loop profile, double radius, double stockFace) =>
        profile.Tolerance.Absolute.Value
            .Apply(epsilon => stockFace.Cons(Range(0, profile.Count - 1)
                .Filter(index => (profile.At(index).Y - radius) * (profile.At(index + 1).Y - radius) <= 0.0
                    && Math.Abs(profile.At(index + 1).Y - profile.At(index).Y) > epsilon)
                .Map(index => profile.At(index).X
                    + ((radius - profile.At(index).Y) / (profile.At(index + 1).Y - profile.At(index).Y)
                    * (profile.At(index + 1).X - profile.At(index).X)))
                .OrderByDescending(static axial => axial)
                .ToSeq()
                .Fold(Seq<double>(), (kept, axial) => kept.Exists(held => Math.Abs(held - axial) <= epsilon)
                    ? kept
                    : kept.Add(axial))))
            .Apply(walls => (walls.Count & 1) == 0
                ? Fin.Succ(Range(0, walls.Count / 2)
                    .Map(pair => (walls[pair * 2], walls[pair * 2 + 1])).ToSeq())
                : Fin.Fail<Seq<(double Start, double End)>>(new GeometryFault.DegenerateInput(
                    Kind.Curve, -1, "turning:crossing-parity").ToError()));

    private static double RadiusAt(Loop profile, double axial, double fallback) => Range(0, profile.Count - 1)
        .Filter(index => (profile.At(index).X - axial) * (profile.At(index + 1).X - axial) <= 0.0
            && Math.Abs(profile.At(index + 1).X - profile.At(index).X) > profile.Tolerance.Absolute.Value)
        .Map(index => profile.At(index).Y
            + (axial - profile.At(index).X) / (profile.At(index + 1).X - profile.At(index).X)
            * (profile.At(index + 1).Y - profile.At(index).Y))
        .Fold(fallback, double.Max);

    private static double Feed(TurnDemand demand, double radius) =>
        double.Min(demand.Cutting.Feed, demand.Budget.FeedPerRevolution)
        * demand.Spindle.RpmAt(Math.Abs(radius), demand.Policy.MinimumSpindleRadius, Surface(demand));

    private static double Surface(TurnDemand demand) => double.Min(demand.Cutting.SurfaceSpeed, demand.Budget.SurfaceSpeed);

    private static bool Positive(double value) => double.IsFinite(value) && value > 0.0;

    private const int SingleEdge = 1;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
