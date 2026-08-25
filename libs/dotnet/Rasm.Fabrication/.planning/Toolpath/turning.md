# [RASM_FABRICATION_TURNING]

`Turning` owns controller-neutral revolved-process generation from one admitted `TurnRequest`. `TurnStep` binds each semantic operation to a spindle side and turret channel; `TurnProgram` preserves channel barriers, spindle state, synchronization, `RemovalEnvelope` rows, and process-load evidence. `Cam` lowers every executable `LatheDirective` onto the S0 `MotionDirective` carrier and every evidence directive onto a `SpecializedToolpathRow`, so no parallel command family exists and a dwell, oriented stop, or spindle synchronization needs no typed refusal.

`LatheOp` closes revolved-operation grammars instead of naming controller cycles. `SweepKind`, `PlungeKind`, and the S0 `AxialKind` generate roughing, finishing, boring, grooving, undercutting, drilling, reaming, counterboring, and countersinking from policy data; `Part`, `Tap`, `Thread`, `Knurl`, and `Handoff` retain distinct payload arity.

`CutSide` is the one owner of the internal-versus-external distinction: radial sign, stock reference, finish-allowance direction, approach and retract polarity, and `RemovalEnvelope` orientation all read that row, so EVERY radial operation carries its side as a column and no body hardcodes an outer radius. The row seats here because its behaviour reaches `TurnStock` and `Loop`, which are Toolpath owners; `Process/atoms#MOTION` names the type on `SpecializedToolpathRow.TurningThread` under the same law its own `FabricationPolicy` cases already hold — S0 names an upper-plane type for payload carriage and reaches no upper-plane behaviour.

The closed vocabularies a specialized row carries — `ThreadForm`, `ThreadHand`, `AxialKind`, `KnurlPattern`, `HandoffKind` — are S0 rows this page CONSUMES; `ThreadProfile` carries the flank, truncation, and depth geometry keyed by the S0 form, so one vocabulary spans the atom and the generator.

## [01]-[INDEX]

- [02]-[VOCABULARY]: row-owned sweep, plunge, thread, spindle, channel, and tool-orientation semantics over the S0 row vocabularies.
- [03]-[DEMAND]: generated owners admit stock, insert, policy, channel steps, and process data once through one accumulating `Admit`.
- [04]-[GENERATE]: `Turning.Generate(TurnRequest)` compensates once, lowers every step, and returns evidence-bearing passes.

## [02]-[VOCABULARY]

- Owner: `CutSide` carries `RadialSign` and its `Target` allowance delegate, so an external sweep leaves stock outward while a bore leaves stock inward; `StockRadius`, `Available`, `Advance`, and `Clear` derive every other side-dependent value from that one row.
- Owner: `ThreadProfile` carries load-flank angle, clearance-flank angle, crest/root truncation, crest/root radius, and pitch-depth factor keyed by the S0 `ThreadForm`. Named standard values are seed data, while `ThreadProfile.Admit` admits custom geometry through the same owner. Buttress geometry remains asymmetric through every pass.
- Owner: `TurnInsert` composes `CutterForm` with insert width, clearance, lead, and semantic tip orientation; no controller tip number enters the process owner.
- Cases: `SweepKind` binds each row to its `CutSide` and its `Emit` generation delegate, so a new sweep is one row rather than a branch; `PlungeKind` distinguishes grooving, undercutting, and forming.
- Law: each `Move.Target.X` is axial machine `Z`, each `Move.Target.Y` is radial machine `X`, and every admitted profile is open.
- Boundary: `SpindleSide` and `TurretChannel` are process facts. `ChannelToken` creates wait/signal barriers without embedding a dialect word.

## [03]-[DEMAND]

- Owner: `TurnStock` admits solid, tubular, and near-net blanks with axial bounds, inner/outer radii, and optional profile evidence.
- Owner: `TurnPolicy` owns approach, retract, overlap, peck, and thread clearances as UnitsNet quantities and the rapid traverse as a `Speed`; no operation body carries a local machining constant and no column spells a unit into a name. Chord and biarc gates are NOT policy columns — the admitted profile carries its own `Context`, so `ToleranceLane.Chord` and `ToleranceLane.Arc` answer them and a project override moves both at once.
- Owner: `SpindleMode` carries the radius FLOOR its own solve needs as a base column each case fills, so a constant-surface mode cannot exist without the floor that keeps its rpm finite at the axis and no body threads one policy column through every rpm site.
- Entry: `TurnDemand.Admit` and `TurnRequest.Admit` accumulate profile, stock, insert, process, spindle, step, operation, synchronization, and numeric defects onto ONE `Validation` rail and land it as `Fin` — the accumulated refusal keeps its arity, so an eleven-defect request reports eleven rows rather than one flattened message.
- Boundary: `TurnDemand` accepts canonical `Loop`, `CutterForm`, `CuttingData`, and `ProcessBudget.Turning` owners. `CuttingData.FeedBasis` must be `FeedBasis.PerRevolution`.

## [04]-[GENERATE]

- Law: tip-radius compensation offsets every profile vertex along its local `ZX` normal, orients that normal by the insert's radial posture so traversal order cannot invert the offset, and reanchors it with the semantic `TipOrientation` vector; clearance-angle gouge admission precedes motion and accumulates every gouging span.
- Law: material crossings are the `PolygonOp.ClipOpen` inside runs of one drive against the closed MATERIAL region the profile and its side's stock rim bound, so a coincident wall hit, a tangency, and an overlapping span are the algebra's verdicts rather than a hand parity check on a scanline that a single degenerate vertex could invert.
- Law: each `SweepKind` row emits its own motion — longitudinal rows require positive radial stock before generating passes, facing roughing reads each interpolated material crossing, pattern roughing shifts the full near-net profile and retracts before repositioning, and the finish rows follow the profile natively; `FinishForm` routes fitted curves through `CurveAlgebra.Apply(CurveOp)` and line-sourced chords through `ArcAlgebra.Densify(ArcProjection.Recover)`, and both lower onto the ONE native bulge walk rather than a second span convention per form.
- Law: explicit axial position, band width, target radius, peck fraction, dwell, and CUT SIDE generate groove, undercut, and form families; `Part` reconstructs width and terminal radius from mounted insert and stock and carries its own side.
- Law: drill, ream, counterbore, and countersink share one depth/diameter/peck/tip-angle generator over the S0 `AxialKind`; a centreline operation states `CutSide.Internal` as a declared fact of boring from the axis, never as a hardcoded rim.
- Law: axial endpoints determine travel, hand remains spindle-synchronization evidence, each start owns pitch indexing, and every pass carries approach, run-in, runout, pullout, asymmetric flank shift, finish, and spring roles.
- Law: main/sub-spindle grip and pull facts are ONE `Handoff` row discriminated by the S0 `HandoffKind`; the cutoff kinds carry their own executed parting span and load evidence. Channel waits and signals preserve twin-turret ordering.
- Law: every generated pass, band, and peck count crosses `Cam.Bounded`, so a degenerate step refuses at its own locus rather than minting a roster no array can hold.
- Law: a directive names the move it follows by the ordinal the trail RECORDED when it emitted that move, so one convention spans plunge, axial, tap, and thread and no body recomputes an index from a stride.
- Law: `Loaded` is the one pass constructor; an operation stating its own load carries it, and every other pass derives `TurnLoad.Cutting` per cutting span through `CuttingData.Evaluate(CutIntent)`. Knurl pressure states `TurnLoad.Forming` instead of impersonating chip-removal force, and a non-removing pass carries no load.
- Law: `CutIntent` admits UnitsNet quantities, so the load boundary converts machining-canonical millimetre, rpm, and feed scalars through `Length.FromMillimeters`, `RotationalSpeed.FromRevolutionsPerMinute`, and `Speed.FromMillimetersPerMinutes` exactly once; radial depth and diameter derive engagement on the admitted intent.
- Entry: `Turning.Generate(TurnRequest)` is the only raw operation.
- Output: `TurnPass` carries moves, directives, load, its `RemovalEnvelope`, and its own measured seconds, so the `SpecializedToolpathEnvelope` the motion lane admits states a real duration; `TurnProgram` carries ordered passes, barriers, residual radial bounds, and physics evidence.
- Packages: `Thinktecture.Runtime.Extensions` owns generated closed families; `LanguageExt.Core` owns accumulated admission and traversal; `System.Numerics.Tensors` owns batch finiteness; `Geometry2D/algebra.md` owns the material-region clip and `Geometry2D/arcs.md` the residual biarc recovery; `ToolAssembly.Snapshot` supplies provider-detached insert width and lead angle through `ToolMeasure`; `UnitsNet` types every admitted policy magnitude and the `CutIntent` load boundary; `Rasm.Domain` `ToleranceLane` supplies the chord and arc gates off the profile's own `Context`; `Rasm.Solving` `FitPolicy.Of` admits the spline fit against that same context under this page's `Op`.
- Boundary: `Turning` owns process geometry and semantic directives; posting admits no typed `TurnProgram` counterpart and reads the lowered `MotionDirective` stream alone.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Numerics.Tensors;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rasm.Parametric;
using Rasm.Solving;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [VOCABULARY] ----------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CutSide {
    public static readonly CutSide External = new("external", radialSign: 1, ExternalTarget);
    public static readonly CutSide Internal = new("internal", radialSign: -1, InternalTarget);

    public int RadialSign { get; }

    [UseDelegateFromConstructor]
    public partial double Target(Loop profile, double radialAllowance);

    private static double ExternalTarget(Loop profile, double radialAllowance) =>
        profile.Vertices.Min(static point => point.Y) + radialAllowance;

    private static double InternalTarget(Loop profile, double radialAllowance) =>
        profile.Vertices.Max(static point => point.Y) - radialAllowance;

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
        finish: false, requiresRadialStock: false, Turning.Facing);
    public static readonly SweepKind NearNet = new("near-net", CutSide.External,
        finish: false, requiresRadialStock: false, Turning.Pattern);
    public static readonly SweepKind ExternalFinish = new("external-finish", CutSide.External,
        finish: true, requiresRadialStock: false, Turning.Native);
    public static readonly SweepKind InternalFinish = new("internal-finish", CutSide.Internal,
        finish: true, requiresRadialStock: false, Turning.Native);

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
public sealed partial class ThreadProfile {
    public ThreadForm Form { get; }
    public double LoadFlankDeg { get; }
    public double ClearanceFlankDeg { get; }
    public double CrestTruncationPitch { get; }
    public double RootTruncationPitch { get; }
    public double CrestRadiusPitch { get; }
    public double RootRadiusPitch { get; }
    public double DepthFactor { get; }

    private static readonly HashMap<ThreadForm, ThreadProfile> Seeds = toHashMap(Seq(
        Seed(ThreadForm.Metric, 30.0, 30.0, 0.125, 0.0, 0.0, 0.1443, 0.6134),
        Seed(ThreadForm.Unified, 30.0, 30.0, 0.125, 0.0, 0.0, 0.1443, 0.61343),
        Seed(ThreadForm.Trapezoidal, 15.0, 15.0, 0.25, 0.25, 0.0, 0.0, 0.5),
        Seed(ThreadForm.Acme, 14.5, 14.5, 0.25, 0.25, 0.0, 0.0, 0.5),
        Seed(ThreadForm.Buttress, 7.0, 45.0, 0.125, 0.25, 0.0, 0.0, 0.6),
        Seed(ThreadForm.Round, 30.0, 30.0, 0.0, 0.0, 0.238, 0.238, 0.55),
        Seed(ThreadForm.Pipe, 27.5, 27.5, 0.0, 0.0, 0.1373, 0.1373, 0.6403))
        .Map(static row => (row.Form, row)));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ThreadForm form,
        ref double loadFlankDeg,
        ref double clearanceFlankDeg,
        ref double crestTruncationPitch,
        ref double rootTruncationPitch,
        ref double crestRadiusPitch,
        ref double rootRadiusPitch,
        ref double depthFactor) {
        if (!(TensorPrimitives.IsFiniteAll<double>([
                loadFlankDeg, clearanceFlankDeg, crestTruncationPitch, rootTruncationPitch,
                crestRadiusPitch, rootRadiusPitch, depthFactor])
            && loadFlankDeg is > 0.0 and < 90.0 && clearanceFlankDeg is > 0.0 and < 90.0
            && crestTruncationPitch >= 0.0 && rootTruncationPitch >= 0.0
            && crestRadiusPitch >= 0.0 && rootRadiusPitch >= 0.0
            && depthFactor is > 0.0 and < 1.0))
            validationError = new ValidationError("thread-profile");
    }

    public static Fin<ThreadProfile> Admit(
        ThreadForm form,
        double loadFlankDeg,
        double clearanceFlankDeg,
        double crestTruncationPitch,
        double rootTruncationPitch,
        double crestRadiusPitch,
        double rootRadiusPitch,
        double depthFactor) =>
        Validate(form, loadFlankDeg, clearanceFlankDeg, crestTruncationPitch, rootTruncationPitch,
            crestRadiusPitch, rootRadiusPitch, depthFactor, out ThreadProfile profile).Admitted(profile);

    public static Fin<ThreadProfile> Of(ThreadForm form) =>
        Seeds.Find(form).ToFin(new KernelFault.InvalidValue("turning", $"thread-profile:unseeded:{form.Key}"));

    private static ThreadProfile Seed(
        ThreadForm form,
        double loadFlankDeg,
        double clearanceFlankDeg,
        double crestTruncationPitch,
        double rootTruncationPitch,
        double crestRadiusPitch,
        double rootRadiusPitch,
        double depthFactor) =>
        new(form, loadFlankDeg, clearanceFlankDeg, crestTruncationPitch, rootTruncationPitch,
            crestRadiusPitch, rootRadiusPitch, depthFactor);
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

// --- [DEMAND] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpindleMode(Length MinimumRadius) {
    public sealed record ConstantSurface(Length MinimumRadius, RotationalSpeed Ceiling) : SpindleMode(MinimumRadius);
    public sealed record ConstantRpm(Length MinimumRadius, RotationalSpeed Held) : SpindleMode(MinimumRadius);

    public double RpmAt(double radiusMm, double surfaceMetersPerMinute) => Switch(
        state: (Radius: radiusMm, Minimum: MinimumRadius.Millimeters, Surface: surfaceMetersPerMinute),
        constantSurface: static (state, mode) => double.Min(
            mode.Ceiling.RevolutionsPerMinute,
            SurfaceSpeed.Rpm(state.Surface, 2.0 * double.Max(state.Radius, state.Minimum))),
        constantRpm: static (_, mode) => mode.Held.RevolutionsPerMinute);
}

[ComplexValueObject]
public sealed partial class TurnStock {
    public StockKind Kind { get; }
    public double OuterRadius { get; }
    public double InnerRadius { get; }
    public double AxialMinimum { get; }
    public double AxialMaximum { get; }
    public Option<Loop> Envelope { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref StockKind kind,
        ref double outerRadius,
        ref double innerRadius,
        ref double axialMinimum,
        ref double axialMaximum,
        ref Option<Loop> envelope) {
        if (!(TensorPrimitives.IsFiniteAll<double>([outerRadius, innerRadius, axialMinimum, axialMaximum])
            && outerRadius > 0.0 && innerRadius >= 0.0 && innerRadius < outerRadius && axialMaximum > axialMinimum
            && (kind != StockKind.Solid || innerRadius == 0.0)
            && (kind != StockKind.Tube || innerRadius > 0.0)
            && (!kind.RequiresEnvelope || envelope.IsSome)))
            validationError = new ValidationError("turn-stock");
    }

    public static Fin<TurnStock> Admit(
        StockKind kind,
        double outerRadius,
        double innerRadius,
        double axialMinimum,
        double axialMaximum,
        Option<Loop> envelope) =>
        Validate(kind, outerRadius, innerRadius, axialMinimum, axialMaximum, envelope, out TurnStock stock)
            .Admitted(stock);
}

[ComplexValueObject]
public sealed partial class TurnInsert {
    public CutterForm Form { get; }
    public double Width { get; }
    public double ClearanceAngleDeg { get; }
    public double LeadAngleDeg { get; }
    public TipOrientation Orientation { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CutterForm form,
        ref double width,
        ref double clearanceAngleDeg,
        ref double leadAngleDeg,
        ref TipOrientation orientation) {
        if (!(TensorPrimitives.IsFiniteAll<double>([width, clearanceAngleDeg, leadAngleDeg])
            && width > 0.0 && clearanceAngleDeg is > 0.0 and < 90.0 && leadAngleDeg is > -90.0 and < 90.0))
            validationError = new ValidationError("turn-insert");
    }

    public static Fin<TurnInsert> Admit(
        CutterForm form,
        ToolAssembly assembly,
        TipOrientation orientation,
        double clearanceAngleDeg) =>
        from width in assembly.Snapshot.Metric(ToolMeasure.InsertWidth)
            .ToFin(new KernelFault.InvalidValue("turning", "turn-insert:width"))
        from lead in assembly.Snapshot.Metric(ToolMeasure.LeadAngle)
            .ToFin(new KernelFault.InvalidValue("turning", "turn-insert:lead-angle"))
        from insert in Validate(form, width, clearanceAngleDeg, lead, orientation, out TurnInsert admitted)
            .Admitted(admitted)
        select insert;
}

[ComplexValueObject]
public sealed partial class TurnPolicy {
    public Length Approach { get; }
    public Length Retract { get; }
    public Ratio WidthOverlap { get; }
    public int BiarcProbeFloor { get; }
    public Length MinimumPeck { get; }
    public Length ThreadRunIn { get; }
    public Length ThreadRunout { get; }
    public Length ThreadPullout { get; }
    public int SpringPasses { get; }

    public Speed Rapid { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length approach,
        ref Length retract,
        ref Ratio widthOverlap,
        ref int biarcProbeFloor,
        ref Length minimumPeck,
        ref Length threadRunIn,
        ref Length threadRunout,
        ref Length threadPullout,
        ref int springPasses,
        ref Speed rapid) {
        double overlap = widthOverlap.DecimalFractions;
        if (!(TensorPrimitives.IsFiniteAll<double>([
                approach.Millimeters, retract.Millimeters, overlap, minimumPeck.Millimeters,
                threadRunIn.Millimeters, threadRunout.Millimeters, threadPullout.Millimeters,
                rapid.MillimetersPerMinutes])
            && approach.Millimeters > 0.0 && retract.Millimeters > 0.0 && overlap is > 0.0 and < 1.0
            && biarcProbeFloor >= 3 && minimumPeck.Millimeters > 0.0
            && threadRunIn.Millimeters >= 0.0 && threadRunout.Millimeters >= 0.0 && threadPullout.Millimeters > 0.0
            && springPasses >= 0 && rapid.MillimetersPerMinutes > 0.0))
            validationError = new ValidationError("turn-policy");
    }

    public static Fin<TurnPolicy> Admit(
        Length approach,
        Length retract,
        Ratio widthOverlap,
        int biarcProbeFloor,
        Length minimumPeck,
        Length threadRunIn,
        Length threadRunout,
        Length threadPullout,
        int springPasses,
        Speed rapid) =>
        Validate(approach, retract, widthOverlap, biarcProbeFloor, minimumPeck, threadRunIn, threadRunout,
            threadPullout, springPasses, rapid, out TurnPolicy policy).Admitted(policy);
}

[ComplexValueObject]
public sealed partial class ThreadSpec {
    public ThreadProfile Profile { get; }
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

    public ThreadForm Form => Profile.Form;
    public double Depth => Profile.DepthFactor * Pitch;
    public double Lead => Pitch * Starts;
    public double CrestFlat => Profile.CrestTruncationPitch * Pitch;
    public double RootFlat => Profile.RootTruncationPitch * Pitch;
    public double CrestRadius => Profile.CrestRadiusPitch * Pitch;
    public double RootRadius => Profile.RootRadiusPitch * Pitch;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ThreadProfile profile,
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
        ref double axialEnd) {
        if (!(TensorPrimitives.IsFiniteAll<double>([
                majorDiameter, pitch, reliefDeg, firstPassMinimum, finalPass, axialStart, axialEnd])
            && majorDiameter > 0.0 && pitch > 0.0 && starts > 0 && roughPasses > 0
            && reliefDeg >= 0.0 && reliefDeg < double.Min(profile.LoadFlankDeg, profile.ClearanceFlankDeg)
            && firstPassMinimum > 0.0 && finalPass > 0.0 && axialStart != axialEnd
            && profile.DepthFactor * pitch < majorDiameter / 2.0
            && finalPass < profile.DepthFactor * pitch
            && firstPassMinimum <= (profile.DepthFactor * pitch) - finalPass))
            validationError = new ValidationError("thread-spec");
    }

    public static Fin<ThreadSpec> Admit(
        ThreadProfile profile,
        ThreadHand hand,
        CutSide side,
        double majorDiameter,
        double pitch,
        int starts,
        int roughPasses,
        InfeedMethod infeed,
        double reliefDeg,
        double firstPassMinimum,
        double finalPass,
        double axialStart,
        double axialEnd) =>
        Validate(profile, hand, side, majorDiameter, pitch, starts, roughPasses, infeed, reliefDeg,
            firstPassMinimum, finalPass, axialStart, axialEnd, out ThreadSpec spec).Admitted(spec);

    public double DepthAt(int pass) => double.Max(
        FirstPassMinimum,
        double.Min(Depth - FinalPass, (Depth - FinalPass) * Math.Sqrt((double)pass / RoughPasses)));

    public double ShiftAt(int pass) =>
        Infeed.Shift(DepthAt(pass), pass, Profile.LoadFlankDeg, Profile.ClearanceFlankDeg, ReliefDeg);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LatheOp {
    private LatheOp() { }

    public sealed record Rough(SweepKind Kind, double Depth, double RadialAllowance, double AxialAllowance) : LatheOp;
    public sealed record Finish(SweepKind Kind, FinishForm Form, double RadialAllowance, double AxialAllowance) : LatheOp;
    public sealed record Plunge(
        PlungeKind Kind, CutSide Side, double AxialPosition, double Width,
        double TargetRadius, double PeckFraction, double DwellRevolutions) : LatheOp;
    public sealed record Part(CutSide Side, double AxialPosition, double PeckFraction) : LatheOp;
    public sealed record Axial(
        AxialKind Kind, double Diameter, double Depth, double PeckDepth,
        double DwellRevolutions, double TipAngleDeg) : LatheOp;
    public sealed record Tap(double Diameter, double Depth, double Pitch, ThreadForm Form, ThreadHand Hand) : LatheOp;
    public sealed record Thread(ThreadSpec Spec) : LatheOp;
    public sealed record Knurl(
        KnurlPattern Pattern, CutSide Side, double AxialStart,
        double AxialEnd, double Radius, double Pressure, double FeedScale) : LatheOp;
    public sealed record Handoff(HandoffKind Kind, CutSide Side, double GripPlane, double GripLength, double PullDistance) : LatheOp;
}

[ValueObject<string>]
public readonly partial struct ChannelToken {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        if (!Witness.Keyed(value))
            validationError = new ValidationError("channel-token");
    }
}

public sealed record TurnStep(
    SpindleSide Spindle,
    TurretChannel Channel,
    LatheOp Operation,
    Seq<ChannelToken> WaitFor,
    Option<ChannelToken> Signal);

[ComplexValueObject]
public sealed partial class TurnDemand {
    public Loop Profile { get; }
    public TurnStock Stock { get; }
    public TurnInsert Insert { get; }
    public SpindleMode Spindle { get; }
    public CuttingData Cutting { get; }
    public ProcessBudget.Turning Budget { get; }
    public TurnPolicy Policy { get; }

    public static Fin<TurnDemand> Admit(
        Loop profile,
        TurnStock stock,
        TurnInsert insert,
        SpindleMode spindle,
        CuttingData cutting,
        ProcessBudget.Turning budget,
        TurnPolicy policy) =>
        AdmissionSlots.Accumulate(Turning.DemandSlots(profile, stock, insert, spindle, cutting, budget))
            .As()
            .ToFin()
            .Bind(_ => Validate(profile, stock, insert, spindle, cutting, budget, policy, out TurnDemand demand)
                .Admitted(demand));
}

[ComplexValueObject]
public sealed partial class TurnRequest {
    public TurnDemand Demand { get; }
    public Seq<TurnStep> Steps { get; }

    public static Fin<TurnRequest> Admit(TurnDemand demand, Seq<TurnStep> steps) =>
        AdmissionSlots.Accumulate(Turning.RequestSlots(demand, steps))
            .As()
            .ToFin()
            .Bind(_ => Validate(demand, steps, out TurnRequest request).Admitted(request));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LatheDirective {
    private LatheDirective() { }

    public sealed record Spindle(SpindleMode Mode, RotationSense Hand, double SurfaceMetersPerMinute, double ResolvedRpm) : LatheDirective;
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
    public sealed record Handoff(HandoffKind Kind, SpindleSide From, SpindleSide To, double GripPlane, double GripLength, double PullDistance) : LatheDirective;
}

public sealed record ChannelBarrier(int Step, TurretChannel Channel, Seq<ChannelToken> WaitFor, Option<ChannelToken> Signal);
public sealed record RemovalEnvelope(double AxialStart, double AxialEnd, double RadiusBefore, double RadiusAfter, CutSide Side, bool RemovesMaterial);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TurnLoad {
    private TurnLoad() { }

    public sealed record Cutting(Seq<CuttingLoad> Spans) : TurnLoad;
    public sealed record Forming(double Pressure, KnurlPattern Pattern) : TurnLoad;
}

public readonly record struct MoveTrail(Seq<Move> Moves, Seq<int> Marks) {
    public static readonly MoveTrail Empty = new(Seq<Move>(), Seq<int>());

    public int Cursor => Moves.Count - 1;

    public MoveTrail Then(Seq<Move> moves) => this with { Moves = Moves + moves };

    public MoveTrail Mark(int back = 0) => this with { Marks = Marks.Add(Cursor - back) };
}

public sealed record TurnPass(
    int Step,
    SpindleSide Spindle,
    TurretChannel Channel,
    LatheOp Operation,
    Seq<Move> Moves,
    Seq<LatheDirective> Directives,
    Option<TurnLoad> Load,
    RemovalEnvelope Removal,
    double DurationSeconds);

public sealed record TurnResidual(double OuterRadius, double InnerRadius, Arr<RemovalEnvelope> Removed);
public sealed record TurnProgram(Seq<TurnPass> Passes, Seq<ChannelBarrier> Barriers, TurnResidual Residual, BudgetEvidence Evidence);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Turning {
    public static Fin<TurnProgram> Generate(TurnRequest request) =>
        from profile in Compensate(request.Demand)
        from passes in request.Steps.Map((step, index) => Emit(index, profile, request.Demand, step)).TraverseM(identity).As()
        let barriers = request.Steps.Map((step, index) => new ChannelBarrier(index, step.Channel, step.WaitFor, step.Signal))
        let removed = passes.Map(static pass => pass.Removal).ToArr()
        let outer = removed.Filter(static span => span.RemovesMaterial && span.Side == CutSide.External)
            .Fold(request.Demand.Stock.OuterRadius, static (radius, span) => double.Min(radius, span.RadiusAfter))
        let inner = removed.Filter(static span => span.RemovesMaterial && span.Side == CutSide.Internal)
            .Fold(request.Demand.Stock.InnerRadius, static (radius, span) => double.Max(radius, span.RadiusAfter))
        select new TurnProgram(passes, barriers, new TurnResidual(outer, inner, removed), request.Demand.Budget.Evidence);

    internal static Seq<K<Validation<Error>, Unit>> DemandSlots(
        Loop profile,
        TurnStock stock,
        TurnInsert insert,
        SpindleMode spindle,
        CuttingData cutting,
        ProcessBudget.Turning budget) =>
        Slots(-1, [
            (!profile.Closed, "profile-open"),
            (profile.Count >= 2, "profile-span"),
            (stock.Envelope.ForAll(static loop => !loop.Closed && loop.Count >= 2), "stock-envelope"),
            (cutting.FeedBasis == FeedBasis.PerRevolution, "feed-basis"),
            (ValidityClaim.Positive(budget.SurfaceSpeed), "surface-speed"),
            (ValidityClaim.Positive(budget.FeedPerRevolution), "feed"),
            (ValidityClaim.Positive(budget.DepthOfCut), "depth"),
            (budget.NoseRadius >= 0.0, "nose-radius-positive"),
            (Math.Abs(budget.NoseRadius - insert.Form.CornerRadius) <= profile.Tolerance.Absolute.Value, "nose-radius-match"),
            (ValidityClaim.Positive(spindle.MinimumRadius.Millimeters), "spindle-minimum-radius")])
        + spindle.Switch(
            constantSurface: static mode => Slots(-1, [(ValidityClaim.Positive(mode.Ceiling.RevolutionsPerMinute), "spindle-css")]),
            constantRpm: static mode => Slots(-1, [(ValidityClaim.Positive(mode.Held.RevolutionsPerMinute), "spindle-rpm")]));

    internal static Seq<K<Validation<Error>, Unit>> RequestSlots(TurnDemand demand, Seq<TurnStep> steps) {
        Seq<(ChannelToken Token, int Step)> signals = steps
            .Map((step, index) => step.Signal.Map(token => (token, index)))
            .Choose(static signal => signal);
        Seq<K<Validation<Error>, Unit>> waits = steps.Bind((step, index) => step.WaitFor.Map(token =>
            AdmissionSlots.Gate(
                signals.Exists(signal => signal.Token == token && signal.Step < index),
                index, "wait-before-signal", Refusal)));
        return Slots(-1, [
                (!steps.IsEmpty, "steps"),
                (signals.Map(static signal => signal.Token).Distinct().Count == signals.Count, "signal-duplicate")])
            + steps.Bind((step, index) => OperationSlots(step.Operation, index, demand))
            + waits;
    }

    private static Seq<K<Validation<Error>, Unit>> OperationSlots(LatheOp operation, int index, TurnDemand demand) =>
        operation.Switch(
            state: (Index: index, Demand: demand),
            rough: static (state, op) => Slots(state.Index, [
                (TensorPrimitives.IsFiniteAll<double>([op.Depth, op.RadialAllowance, op.AxialAllowance]), "sweep-finite"),
                (!op.Kind.Finish, "sweep-rough-kind"),
                (op.Depth > 0.0, "sweep-depth"),
                (op.RadialAllowance >= 0.0, "sweep-radial-allowance"),
                (op.AxialAllowance >= 0.0, "sweep-axial-allowance"),
                (!op.Kind.RequiresRadialStock
                    || op.Kind.Side.Available(state.Demand.Stock,
                        op.Kind.Side.Target(state.Demand.Profile, op.RadialAllowance))
                        > state.Demand.Profile.Tolerance.Absolute.Value, "sweep-available")]),
            finish: static (state, op) => Slots(state.Index, [
                (TensorPrimitives.IsFiniteAll<double>([op.RadialAllowance, op.AxialAllowance]), "finish-finite"),
                (op.Kind.Finish, "finish-kind"),
                (op.RadialAllowance >= 0.0, "finish-radial-allowance"),
                (op.AxialAllowance >= 0.0, "finish-axial-allowance")]),
            plunge: static (state, op) => Slots(state.Index, [
                (TensorPrimitives.IsFiniteAll<double>([op.AxialPosition, op.Width, op.TargetRadius, op.PeckFraction, op.DwellRevolutions]), "plunge-finite"),
                (op.Width > 0.0, "plunge-width"),
                (op.TargetRadius >= state.Demand.Stock.InnerRadius && op.TargetRadius <= state.Demand.Stock.OuterRadius, "plunge-radius"),
                (op.PeckFraction is > 0.0 and <= 1.0, "plunge-peck"),
                (op.DwellRevolutions >= 0.0, "plunge-dwell"),
                (op.AxialPosition >= state.Demand.Stock.AxialMinimum, "plunge-start"),
                (op.AxialPosition + op.Width <= state.Demand.Stock.AxialMaximum, "plunge-end")]),
            part: static (state, op) => Slots(state.Index, [
                (TensorPrimitives.IsFiniteAll<double>([op.AxialPosition, op.PeckFraction]), "part-finite"),
                (op.AxialPosition >= state.Demand.Stock.AxialMinimum, "part-start"),
                (op.AxialPosition + state.Demand.Insert.Width <= state.Demand.Stock.AxialMaximum, "part-end"),
                (op.PeckFraction is > 0.0 and <= 1.0, "part-peck")]),
            axial: static (state, op) => Slots(state.Index, [
                (TensorPrimitives.IsFiniteAll<double>([op.Diameter, op.Depth, op.PeckDepth, op.DwellRevolutions, op.TipAngleDeg]), "axial-finite"),
                (op.Diameter > 0.0 && op.Diameter / 2.0 <= state.Demand.Stock.OuterRadius, "axial-diameter"),
                (op.Depth > 0.0 && op.Depth <= state.Demand.Stock.AxialMaximum - state.Demand.Stock.AxialMinimum, "axial-depth"),
                (op.PeckDepth > 0.0, "axial-peck"),
                (op.DwellRevolutions >= 0.0, "axial-dwell"),
                (op.TipAngleDeg is > 0.0 and <= 180.0, "axial-tip-angle")]),
            tap: static (state, op) => Slots(state.Index, [
                (TensorPrimitives.IsFiniteAll<double>([op.Diameter, op.Depth, op.Pitch]), "tap-finite"),
                (op.Diameter > 0.0 && op.Diameter / 2.0 <= state.Demand.Stock.OuterRadius, "tap-diameter"),
                (op.Depth > 0.0 && op.Depth <= state.Demand.Stock.AxialMaximum - state.Demand.Stock.AxialMinimum, "tap-depth"),
                (op.Pitch > 0.0, "tap-pitch")]),
            thread: static (state, op) => Slots(state.Index, [
                (op.Spec.FinalPass < op.Spec.Depth, "thread-finish"),
                (op.Spec.Side == CutSide.Internal
                    ? op.Spec.MajorDiameter / 2.0 <= state.Demand.Stock.OuterRadius
                        && (op.Spec.MajorDiameter / 2.0) - op.Spec.Depth >= state.Demand.Stock.InnerRadius
                    : op.Spec.MajorDiameter / 2.0 <= state.Demand.Stock.OuterRadius, "thread-diameter"),
                (double.Min(op.Spec.AxialStart, op.Spec.AxialEnd) >= state.Demand.Stock.AxialMinimum, "thread-start"),
                (double.Max(op.Spec.AxialStart, op.Spec.AxialEnd) <= state.Demand.Stock.AxialMaximum, "thread-end")]),
            knurl: static (state, op) => Slots(state.Index, [
                (TensorPrimitives.IsFiniteAll<double>([op.AxialStart, op.AxialEnd, op.Radius, op.Pressure, op.FeedScale]), "knurl-finite"),
                (op.AxialStart != op.AxialEnd, "knurl-span"),
                (op.Radius > 0.0 && op.Radius <= state.Demand.Stock.OuterRadius, "knurl-radius"),
                (op.Pressure > 0.0, "knurl-pressure"),
                (op.FeedScale is > 0.0 and <= 1.0, "knurl-feed"),
                (double.Min(op.AxialStart, op.AxialEnd) >= state.Demand.Stock.AxialMinimum, "knurl-start"),
                (double.Max(op.AxialStart, op.AxialEnd) <= state.Demand.Stock.AxialMaximum, "knurl-end")]),
            handoff: static (state, op) => Slots(state.Index, [
                (TensorPrimitives.IsFiniteAll<double>([op.GripPlane, op.GripLength, op.PullDistance]), "handoff-finite"),
                (op.GripLength > 0.0, "handoff-grip"),
                (op.PullDistance >= 0.0, "handoff-pull"),
                (op.GripPlane >= state.Demand.Stock.AxialMinimum, "handoff-start"),
                (op.GripPlane + double.Max(op.GripLength, Parts(op.Kind) ? state.Demand.Insert.Width : 0.0)
                    <= state.Demand.Stock.AxialMaximum, "handoff-end")]));

    private static Seq<K<Validation<Error>, Unit>> Slots(int step, ReadOnlySpan<(bool Ok, string Axis)> facts) =>
        toSeq(facts.ToArray()).Map(fact => AdmissionSlots.Gate(fact.Ok, step, fact.Axis, Refusal));

    private static FabricationFault Refusal(int step, string axis) =>
        FabricationFault.Inadmissible(
            FabConcern.Toolpath, step < 0 ? $"turning:{axis}" : $"turning:step-{step}:{axis}");

    private static Fin<Loop> Compensate(TurnDemand demand) {
        Seq<Error> gouges = Range(0, demand.Profile.Count - 1)
            .Choose(index => Clearance(demand.Profile.At(index + 1) - demand.Profile.At(index))
                > 90.0 - demand.Insert.ClearanceAngleDeg - Math.Abs(demand.Insert.LeadAngleDeg)
                    ? Some((Error)new FabricationFault.Gouge(demand.Profile.At(index), demand.Insert.Form))
                    : Option<Error>.None)
            .ToSeq();
        return gouges.IsEmpty
            ? Loop.Admit(
                toSeq(demand.Profile.Vertices).Map((point, index) => Compensated(demand.Profile, demand.Insert, point, index)).ToArr(),
                closed: false,
                demand.Profile.Bulges,
                demand.Profile.Tolerance)
            : Fin.Fail<Loop>(Error.Many([.. gouges]));
    }

    private static Point3d Compensated(Loop profile, TurnInsert insert, Point3d point, int index) {
        Vector3d span = profile.At(int.Min(index + 1, profile.Count - 1)) - profile.At(int.Max(index - 1, 0));
        Vector3d tangent = span.Unitize() ? span : Vector3d.XAxis;
        Vector3d normal = new Vector3d(-tangent.Y, tangent.X, 0.0) * (insert.Orientation.Radial < 0 ? -1.0 : 1.0);
        return point + (insert.Form.CornerRadius * normal)
            - (new Vector3d(insert.Orientation.Axial, insert.Orientation.Radial, 0.0) * insert.Form.CornerRadius);
    }

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
        handoff: static (state, op) => Handoff(state.Index, state.Demand, state.Step, op));

    private static Fin<TurnPass> Rough(int index, Loop profile, TurnDemand demand, TurnStep step, LatheOp.Rough op) =>
        from moves in op.Kind.Sweep(profile, demand, new SweepDemand(op.Depth, op.RadialAllowance, op.AxialAllowance))
        from pass in Loaded(index, demand, step, moves, Seq<LatheDirective>(),
            Envelope(profile, demand, op.Kind.Side, op.RadialAllowance, op.AxialAllowance))
        select pass;

    private static Fin<TurnPass> Finish(int index, Loop profile, TurnDemand demand, TurnStep step, LatheOp.Finish op) =>
        from moves in FinishMoves(profile, demand, op)
        from pass in Loaded(index, demand, step, moves, Seq<LatheDirective>(),
            Envelope(profile, demand, op.Kind.Side, op.RadialAllowance, op.AxialAllowance))
        select pass;

    internal static Fin<Seq<Move>> Longitudinal(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) {
        double target = side.Target(profile, sweep.RadialAllowance);
        return from region in Material(profile, demand.Stock, side)
               from passes in Cam.Bounded(side.Available(demand.Stock, target), sweep.Depth, Cam.PassCap, "turning:sweep-passes")
               from moves in Range(1, passes).ToSeq().Traverse(pass => {
                   double radius = side.Advance(demand.Stock, target, pass, sweep.Depth);
                   return Crossings(region, radius, demand).Bind(spans => spans.Traverse(span => Cam.Trail(
                       Move.Rapid.Of(new Point3d(span.Start, side.Clear(radius, demand.Policy.Retract.Millimeters), 0.0)),
                       Move.Linear.Of(new Point3d(span.Start, radius, 0.0), Feed(demand, radius)),
                       Move.Linear.Of(new Point3d(span.End + sweep.AxialAllowance, radius, 0.0), Feed(demand, radius)),
                       Move.Rapid.Of(new Point3d(
                           span.End + sweep.AxialAllowance,
                           side.Clear(radius, demand.Policy.Retract.Millimeters),
                           0.0)))).Map(static rows => rows.Bind(identity)));
               }).As()
               select moves.Bind(identity);
    }

    internal static Fin<Seq<Move>> Facing(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) {
        double target = profile.Vertices.Min(static point => point.X) + sweep.AxialAllowance;
        return from region in Material(profile, demand.Stock, side)
               from passes in Cam.Bounded(demand.Stock.AxialMaximum - target, sweep.Depth, Cam.PassCap, "turning:facing-passes")
               from moves in Range(1, passes).ToSeq().Traverse(pass => {
                   double axial = double.Max(target, demand.Stock.AxialMaximum - (pass * sweep.Depth));
                   return RadiusAt(region, axial, demand, side).Bind(radius => Cam.Trail(
                       Move.Rapid.Of(new Point3d(axial, side.Clear(side.StockRadius(demand.Stock), demand.Policy.Approach.Millimeters), 0.0)),
                       Move.Linear.Of(new Point3d(axial, radius, 0.0), Feed(demand, side.StockRadius(demand.Stock))),
                       Move.Rapid.Of(new Point3d(axial + demand.Policy.Retract.Millimeters, radius, 0.0))));
               }).As()
               select moves.Bind(identity);
    }

    internal static Fin<Seq<Move>> Pattern(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) {
        Loop source = demand.Stock.Envelope.IfNone(profile);
        Point3d park = new(
            demand.Stock.AxialMaximum + demand.Policy.Approach.Millimeters,
            side.Clear(side.StockRadius(demand.Stock), demand.Policy.Approach.Millimeters),
            0.0);
        return Cam.Bounded(
                double.Max(sweep.RadialAllowance, sweep.AxialAllowance), sweep.Depth, Cam.PassCap, "turning:pattern-passes")
            .Bind(passes => Range(1, passes).ToSeq().Traverse(pass => {
                double fraction = (double)(passes - pass) / passes;
                SweepDemand shifted = new(sweep.Depth, sweep.RadialAllowance * fraction, sweep.AxialAllowance * fraction);
                return from entry in Move.Rapid.Of(park)
                       from body in Native(source, demand, shifted, side)
                       from exit in Move.Rapid.Of(new Point3d(
                           source.Vertices.Max(static point => point.X) + shifted.AxialAllowance,
                           side.Clear(side.StockRadius(demand.Stock), demand.Policy.Retract.Millimeters),
                           0.0))
                       select entry.Cons(body).Add(exit);
            }).As().Map(static rows => rows.Bind(identity)));
    }

    internal static Fin<Seq<Move>> Native(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) =>
        from entry in Move.Rapid.Of(new Point3d(
            profile.At(0).X + sweep.AxialAllowance,
            side.Clear(profile.At(0).Y + sweep.RadialAllowance, demand.Policy.Approach.Millimeters),
            0.0))
        from spans in Range(0, profile.Spans).ToSeq().Map(index => {
            Point3d target = profile.At(index + 1);
            Point3d at = new(target.X + sweep.AxialAllowance, target.Y + sweep.RadialAllowance, 0.0);
            return profile.BulgeAt(index) == 0.0
                ? Move.Linear.Of(at, Feed(demand, target.Y))
                : Move.Circular.Of(
                    at,
                    Feed(demand, target.Y),
                    ArcOf(profile, index, sweep.RadialAllowance, sweep.AxialAllowance),
                    4.0 * Math.Atan(profile.BulgeAt(index)));
        }).TraverseM(identity).As()
        select entry.Cons(spans);

    private static Fin<Seq<Move>> FinishMoves(Loop profile, TurnDemand demand, LatheOp.Finish op) {
        SweepDemand sweep = new(0.0, op.RadialAllowance, op.AxialAllowance);
        return op.Form.Switch(
            state: (Profile: profile, Demand: demand, Kind: op.Kind, Sweep: sweep),
            nativeArc: static state => state.Kind.Sweep(state.Profile, state.Demand, state.Sweep),
            spline: static state => Spline(state.Profile, state.Demand, state.Sweep, state.Kind.Side),
            biarc: static state => Biarc(state.Profile, state.Demand, state.Sweep, state.Kind.Side));
    }

    private static Fin<Seq<Move>> Spline(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) =>
        from fit in FitPolicy.Of(context: profile.Tolerance, key: Key)
        let chord = profile.Tolerance.For(ToleranceLane.Chord).Value
        from fitted in CurveAlgebra.Apply(new CurveOp.Admit(
            new CurveSource.Outline(profile, chord, fit),
            Key))
        from curve in fitted is CurveTrace.Fitted admitted
            ? Fin.Succ(admitted.Curve)
            : Fin.Fail<NurbsForm.Curve>(new KernelFault.InvalidValue("turning", "turning:spline-fit"))
        from lowered in CurveAlgebra.Apply(new CurveOp.Lower(
            curve,
            new CurveLowering.Chords(new DivideRule.ByChord(chord)),
            profile.Tolerance,
            Key))
        from loop in lowered is CurveTrace.Lowered result
            ? Fin.Succ(result.Loop)
            : Fin.Fail<Loop>(new KernelFault.InvalidValue("turning", "turning:spline-lower"))
        from moves in Native(loop, demand, sweep, side)
        select moves;

    private static Fin<Seq<Move>> Biarc(Loop profile, TurnDemand demand, SweepDemand sweep, CutSide side) =>
        from trace in ArcAlgebra.Densify(new ArcProjection.Recover(
            profile, profile.Tolerance.For(ToleranceLane.Arc).Value, demand.Policy.BiarcProbeFloor))
        from recovered in trace
            .Recovery(new KernelFault.InvalidValue("turning", "turning:biarc-recover"))
            .Map(static evidence => evidence.Output)
        from moves in Native(recovered, demand, sweep, side)
        select moves;

    private static ArcCenter ArcOf(Loop profile, int index, double radialShift, double axialShift) {
        double bulge = profile.BulgeAt(index);
        Point3d start = profile.At(index);
        Point3d end = profile.At(index + 1);
        Vector3d chord = end - start;
        Vector3d normal = new(-chord.Y, chord.X, 0.0);
        _ = normal.Unitize();
        Point3d center = new Point3d((start.X + end.X) / 2.0, (start.Y + end.Y) / 2.0, 0.0)
            + (normal * chord.Length * (1.0 - (bulge * bulge)) / (4.0 * bulge));
        return new ArcCenter(
            center + new Vector3d(axialShift, radialShift, 0.0),
            bulge < 0.0 ? RotationSense.Clockwise : RotationSense.Counterclockwise);
    }

    private static Fin<TurnPass> Plunge(int index, TurnDemand demand, TurnStep step, LatheOp.Plunge op) {
        double rim = op.Side.StockRadius(demand.Stock);
        double increment = demand.Insert.Width * (1.0 - demand.Policy.WidthOverlap.DecimalFractions);
        return from bands in Cam.Bounded(
                   double.Max(op.Width - demand.Insert.Width, 0.0) + increment, increment, Cam.PassCap, "turning:plunge-bands")
               from pecks in Cam.Bounded(1.0, op.PeckFraction, Cam.PassCap, "turning:plunge-pecks")
               from trail in Range(0, bands).ToSeq().FoldM<Fin, MoveTrail>(MoveTrail.Empty, (walked, band) => {
                   double axial = op.AxialPosition
                       + double.Min(band * increment, double.Max(op.Width - demand.Insert.Width, 0.0));
                   return from approach in Move.Rapid.Of(new Point3d(axial, op.Side.Clear(rim, demand.Policy.Approach.Millimeters), 0.0))
                          from cuts in Range(1, pecks).ToSeq().Traverse(peck => {
                              double radius = rim - ((rim - op.TargetRadius) * peck / pecks);
                              return Cam.Trail(
                                  Move.Linear.Of(new Point3d(axial, radius, 0.0), Feed(demand, radius)),
                                  Move.Rapid.Of(new Point3d(axial, op.Side.Clear(radius, demand.Policy.Retract.Millimeters), 0.0)));
                          }).As().Map(static rows => rows.Bind(identity))
                          select walked.Then(approach.Cons(cuts)).Mark(back: 1);
               }).As()
               from pass in Loaded(index, demand, step, trail.Moves,
                   op.DwellRevolutions > 0.0
                       ? trail.Marks.Map(mark => (LatheDirective)new LatheDirective.Dwell(mark, op.DwellRevolutions))
                       : Seq<LatheDirective>(),
                   new RemovalEnvelope(
                       op.AxialPosition, op.AxialPosition + op.Width, rim, op.TargetRadius, op.Side, RemovesMaterial: true))
               select pass;
    }

    private static Fin<TurnPass> Part(int index, TurnDemand demand, TurnStep step, LatheOp.Part op) {
        double rim = op.Side.StockRadius(demand.Stock);
        double core = op.Side == CutSide.External ? demand.Stock.InnerRadius : demand.Stock.OuterRadius;
        return from pecks in Cam.Bounded(1.0, op.PeckFraction, Cam.PassCap, "turning:part-pecks")
               from approach in Move.Rapid.Of(new Point3d(op.AxialPosition, op.Side.Clear(rim, demand.Policy.Approach.Millimeters), 0.0))
               from cuts in Range(1, pecks).ToSeq().Traverse(peck => {
                   double radius = rim - ((rim - core) * peck / pecks);
                   return Cam.Trail(
                       Move.Linear.Of(new Point3d(op.AxialPosition, radius, 0.0), Feed(demand, radius)),
                       Move.Rapid.Of(new Point3d(op.AxialPosition, op.Side.Clear(radius, demand.Policy.Retract.Millimeters), 0.0)));
               }).As().Map(static rows => rows.Bind(identity))
               from pass in Loaded(index, demand, step, approach.Cons(cuts), Seq<LatheDirective>(),
                   new RemovalEnvelope(
                       op.AxialPosition, op.AxialPosition + demand.Insert.Width, rim, core, op.Side, RemovesMaterial: true))
               select pass;
    }

    private static Fin<TurnPass> Axial(int index, TurnDemand demand, TurnStep step, LatheOp.Axial op) {
        double face = demand.Stock.AxialMaximum;
        double start = face + demand.Policy.Approach.Millimeters;
        return from depths in Depths(op.Kind, op.Depth, double.Max(op.PeckDepth, demand.Policy.MinimumPeck.Millimeters))
               from approach in Move.Rapid.Of(new Point3d(start, 0.0, 0.0))
               from cuts in depths.Traverse(depth => Cam.Trail(
                   Move.Linear.Of(new Point3d(face - depth, 0.0, 0.0), Feed(demand, op.Diameter / 2.0)),
                   Move.Rapid.Of(new Point3d(start, 0.0, 0.0)))).As().Map(static rows => rows.Bind(identity))
               let trail = MoveTrail.Empty.Then(approach.Cons(cuts))
               from pass in Loaded(index, demand, step, trail.Moves,
                   Seq<LatheDirective>(new LatheDirective.AxialShape(
                       0, trail.Cursor, op.Kind, op.Diameter, op.Depth, op.TipAngleDeg))
                   + (op.DwellRevolutions > 0.0
                       ? Seq<LatheDirective>(new LatheDirective.Dwell(trail.Cursor - 1, op.DwellRevolutions))
                       : Seq<LatheDirective>()),
                   new RemovalEnvelope(
                       face - op.Depth, face, demand.Stock.InnerRadius, op.Diameter / 2.0,
                       CutSide.Internal, RemovesMaterial: true))
               select pass;
    }

    private static Fin<Seq<double>> Depths(AxialKind kind, double depth, double peckDepth) => kind.Switch(
        state: (Depth: depth, Peck: peckDepth),
        drill: static state => Pecked(state.Depth, state.Peck),
        peck: static state => Pecked(state.Depth, state.Peck),
        counterbore: static state => Pecked(state.Depth, state.Peck),
        bore: static state => Fin.Succ(Seq(state.Depth)),
        ream: static state => Fin.Succ(Seq(state.Depth)),
        countersink: static state => Fin.Succ(Seq(state.Depth)));

    private static Fin<Seq<double>> Pecked(double depth, double peckDepth) =>
        Cam.Bounded(depth, peckDepth, Cam.PassCap, "turning:axial-pecks").Map(pecks =>
            Range(1, pecks).ToSeq().Map(peck => double.Min(depth, peck * peckDepth)));

    private static Fin<TurnPass> Tap(int index, TurnDemand demand, TurnStep step, LatheOp.Tap op) {
        double face = demand.Stock.AxialMaximum;
        double start = face + demand.Policy.Approach.Millimeters;
        double radius = op.Diameter / 2.0;
        double rpm = demand.Spindle.RpmAt(radius, Surface(demand));
        return from moves in Cam.Trail(
                   Move.Rapid.Of(new Point3d(start, 0.0, 0.0)),
                   Move.Linear.Of(new Point3d(face - op.Depth, 0.0, 0.0), op.Pitch * rpm),
                   Move.Linear.Of(new Point3d(start, 0.0, 0.0), op.Pitch * rpm))
               let trail = MoveTrail.Empty.Then(moves)
               from pass in Loaded(index, demand, step, trail.Moves, Seq<LatheDirective>(
                       new LatheDirective.Spindle(Held(demand, rpm), Hand(op.Hand), Surface(demand), rpm),
                       new LatheDirective.TapShape(1, trail.Cursor, op.Diameter, op.Depth, op.Pitch, op.Form, op.Hand),
                       new LatheDirective.Synchronize(1, trail.Cursor, rpm, op.Pitch, op.Hand, 0, 0, ThreadCutRole.Finish)),
                   new RemovalEnvelope(
                       face - op.Depth, face, demand.Stock.InnerRadius, radius, CutSide.Internal, RemovesMaterial: true))
               select pass;
    }

    private static Fin<TurnPass> Thread(int index, TurnDemand demand, TurnStep step, ThreadSpec spec) {
        Seq<(int Start, int Pass, ThreadCutRole Role, double Depth, double Shift)> cuts =
            Range(0, spec.Starts).ToSeq().Bind(start =>
                Range(1, spec.RoughPasses).ToSeq().Map(pass => (start, pass, ThreadCutRole.Rough, spec.DepthAt(pass), spec.ShiftAt(pass)))
                + Seq((start, spec.RoughPasses + 1, ThreadCutRole.Finish, spec.Depth, 0.0))
                + Range(1, demand.Policy.SpringPasses).ToSeq()
                    .Map(pass => (start, spec.RoughPasses + 1 + pass, ThreadCutRole.Spring, spec.Depth, 0.0)));
        double majorRadius = spec.MajorDiameter / 2.0;
        double rpm = demand.Spindle.RpmAt(majorRadius, Surface(demand));
        double axialDirection = Math.Sign(spec.AxialEnd - spec.AxialStart);
        return from walked in cuts.FoldM<Fin, (MoveTrail Trail, Seq<LatheDirective> Sync)>(
                   (MoveTrail.Empty, Seq<LatheDirective>()),
                   (state, cut) => {
                       double radius = spec.Side == CutSide.Internal
                           ? majorRadius - spec.Depth + cut.Depth
                           : majorRadius - cut.Depth;
                       double indexedStart = spec.AxialStart + (axialDirection * cut.Start * spec.Pitch) + cut.Shift;
                       double indexedEnd = spec.AxialEnd + (axialDirection * cut.Start * spec.Pitch) + cut.Shift;
                       double entry = indexedStart - (axialDirection * demand.Policy.ThreadRunIn.Millimeters);
                       double exit = indexedEnd + (axialDirection * demand.Policy.ThreadRunout.Millimeters);
                       return Cam.Trail(
                           Move.Rapid.Of(new Point3d(entry, spec.Side.Clear(radius, demand.Policy.ThreadPullout.Millimeters), 0.0)),
                           Move.Linear.Of(new Point3d(entry, radius, 0.0), Feed(demand, radius)),
                           Move.Linear.Of(new Point3d(exit, radius, 0.0), spec.Lead * rpm),
                           Move.Rapid.Of(new Point3d(exit, spec.Side.Clear(radius, demand.Policy.ThreadPullout.Millimeters), 0.0)))
                       .Map(moves => {
                           MoveTrail advanced = state.Trail.Then(moves);
                           return (advanced, state.Sync.Add(new LatheDirective.Synchronize(
                               advanced.Cursor - 2, advanced.Cursor - 1, rpm, spec.Lead, spec.Hand,
                               cut.Start, cut.Pass, cut.Role)));
                       });
                   }).As()
               from pass in Loaded(index, demand, step, walked.Trail.Moves, Seq<LatheDirective>(
                       new LatheDirective.Spindle(Held(demand, rpm), Hand(spec.Hand), Surface(demand), rpm),
                       new LatheDirective.ThreadGeometry(
                           spec.Form, spec.Profile.LoadFlankDeg, spec.Profile.ClearanceFlankDeg,
                           spec.CrestFlat, spec.RootFlat, spec.CrestRadius, spec.RootRadius, spec.Side))
                   + walked.Sync,
                   new RemovalEnvelope(
                       double.Min(spec.AxialStart, spec.AxialEnd),
                       double.Max(spec.AxialStart, spec.AxialEnd),
                       spec.Side == CutSide.Internal ? majorRadius - spec.Depth : majorRadius,
                       spec.Side == CutSide.Internal ? majorRadius : majorRadius - spec.Depth,
                       spec.Side,
                       RemovesMaterial: true))
               select pass;
    }

    private static Fin<TurnPass> Knurl(int index, TurnDemand demand, TurnStep step, LatheOp.Knurl op) =>
        from moves in Cam.Trail(
            Move.Rapid.Of(new Point3d(op.AxialStart, op.Side.Clear(op.Radius, demand.Policy.Approach.Millimeters), 0.0)),
            Move.Linear.Of(new Point3d(op.AxialStart, op.Radius, 0.0), Feed(demand, op.Radius) * op.FeedScale),
            Move.Linear.Of(new Point3d(op.AxialEnd, op.Radius, 0.0), Feed(demand, op.Radius) * op.FeedScale),
            Move.Rapid.Of(new Point3d(op.AxialEnd, op.Side.Clear(op.Radius, demand.Policy.Retract.Millimeters), 0.0)))
        from pass in Loaded(index, demand, step, moves,
            Seq<LatheDirective>(new LatheDirective.Knurl(1, 2, op.Pattern, op.Pressure)),
            new RemovalEnvelope(
                double.Min(op.AxialStart, op.AxialEnd),
                double.Max(op.AxialStart, op.AxialEnd),
                op.Radius,
                op.Radius,
                op.Side,
                RemovesMaterial: false),
            Some<TurnLoad>(new TurnLoad.Forming(op.Pressure, op.Pattern)))
        select pass;

    private static Fin<TurnPass> Handoff(int index, TurnDemand demand, TurnStep step, LatheOp.Handoff op) {
        double rim = op.Side.StockRadius(demand.Stock);
        double core = op.Side == CutSide.External ? demand.Stock.InnerRadius : demand.Stock.OuterRadius;
        Seq<LatheDirective> directives = Seq<LatheDirective>(new LatheDirective.Handoff(
            op.Kind, step.Spindle, step.Spindle.Opposite, op.GripPlane, op.GripLength, op.PullDistance));
        return Parts(op.Kind)
            ? from moves in Cam.Trail(
                  Move.Rapid.Of(new Point3d(op.GripPlane, op.Side.Clear(rim, demand.Policy.Approach.Millimeters), 0.0)),
                  Move.Linear.Of(new Point3d(op.GripPlane, core, 0.0), Feed(demand, core)),
                  Move.Rapid.Of(new Point3d(op.GripPlane, op.Side.Clear(rim, demand.Policy.Retract.Millimeters), 0.0)))
              from pass in Loaded(index, demand, step, moves, directives, new RemovalEnvelope(
                  op.GripPlane, op.GripPlane + demand.Insert.Width, rim, core, op.Side, RemovesMaterial: true))
              select pass
            : Loaded(index, demand, step, Seq<Move>(), directives, new RemovalEnvelope(
                op.GripPlane, op.GripPlane + op.GripLength, rim, rim, op.Side, RemovesMaterial: false));
    }

    private static Fin<TurnPass> Loaded(
        int index,
        TurnDemand demand,
        TurnStep step,
        Seq<Move> moves,
        Seq<LatheDirective> directives,
        RemovalEnvelope removal,
        Option<TurnLoad> stated = default) =>
        moves.Traverse(move => Project(move, step.Spindle)).As()
            .Bind(projected => Priced(index, demand, step, projected, directives, removal, stated));

    private static Fin<TurnPass> Priced(
        int index,
        TurnDemand demand,
        TurnStep step,
        Seq<Move> projected,
        Seq<LatheDirective> directives,
        RemovalEnvelope removal,
        Option<TurnLoad> stated) {
        RemovalEnvelope projectedRemoval = Project(removal, step.Spindle);
        Seq<(double Radius, double Feed)> spans = projected.Choose(move => move.Switch(
            state: Math.Abs(projectedRemoval.RadiusAfter),
            rapid: static (_, _) => Option<(double Radius, double Feed)>.None,
            linear: static (minimum, row) => Some((double.Max(minimum, Math.Abs(row.Target.Y)), row.Feed)),
            circular: static (minimum, row) => Some((double.Max(minimum, Math.Abs(row.Target.Y)), row.Feed))));
        double radius = spans.Map(static span => span.Radius).Fold(demand.Spindle.MinimumRadius.Millimeters, double.Max);
        double resolvedRpm = demand.Spindle.RpmAt(radius, Surface(demand));
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
            : new LatheDirective.Spindle(
                demand.Spindle, RotationSense.Clockwise, Surface(demand), resolvedRpm).Cons(directives);
        double seconds = Elapsed(projected, demand);
        return stated.Match(
            Some: load => Fin.Succ(new TurnPass(
                index, step.Spindle, step.Channel, step.Operation, projected, resolved, Some(load),
                projectedRemoval, seconds)),
            None: () => removal.RemovesMaterial
                ? from _ in guard(
                      !spans.IsEmpty,
                      (Error)new KernelFault.InvalidValue("turning", "turning:cutting-span")).ToFin()
                  from loads in spans.Map(span =>
                          from intent in Intent(demand, span, chipWidth, axialDepth, radialDepth)
                          from load in demand.Cutting.Evaluate(intent)
                          select load)
                      .TraverseM(identity)
                      .As()
                  select new TurnPass(
                      index, step.Spindle, step.Channel, step.Operation, projected, resolved,
                      Some<TurnLoad>(new TurnLoad.Cutting(loads)), projectedRemoval, seconds)
                : Fin.Succ(new TurnPass(
                    index, step.Spindle, step.Channel, step.Operation, projected, resolved, None,
                    projectedRemoval, seconds)));
    }

    private static double Elapsed(Seq<Move> moves, TurnDemand demand) => moves
        .Fold(
            (Cursor: Point3d.Origin, Seconds: 0.0),
            (state, move) => (move.Target, state.Seconds + move.Switch(
                state: (state.Cursor, Rapid: demand.Policy.Rapid.MillimetersPerMinutes),
                rapid: static (walk, row) => 60.0 * walk.Cursor.DistanceTo(row.Target) / walk.Rapid,
                linear: static (walk, row) => 60.0 * walk.Cursor.DistanceTo(row.Target) / row.Feed,
                circular: static (walk, row) => 60.0 * row.Radius * Math.Abs(row.SweepRadians) / row.Feed)))
        .Seconds;

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
            Length.FromMillimeters(double.Max(span.Radius * 2.0, demand.Spindle.MinimumRadius.Millimeters * 2.0)),
            teeth: SingleEdge,
            RotationalSpeed.FromRevolutionsPerMinute(
                demand.Spindle.RpmAt(span.Radius, Surface(demand))),
            Speed.FromMillimetersPerMinutes(span.Feed),
            out CutIntent intent).Admitted(intent);

    private static bool Parts(HandoffKind kind) => kind.Switch(
        transfer: static () => false,
        cutoffTransfer: static () => true,
        handoff: static () => false,
        cutoffHandoff: static () => true);

    private static Fin<Move> Project(Move move, SpindleSide side) => move.Switch(
        state: side.AxialSign,
        rapid: static (sign, row) => Move.Rapid.Of(
            new Point3d(row.Target.X * sign, row.Target.Y, row.Target.Z), row.Orientation),
        linear: static (sign, row) => Move.Linear.Of(
            new Point3d(row.Target.X * sign, row.Target.Y, row.Target.Z), row.Feed, row.Orientation),
        circular: static (sign, row) => Move.Circular.Of(
            new Point3d(row.Target.X * sign, row.Target.Y, row.Target.Z),
            row.Feed,
            new ArcCenter(
                new Point3d(row.Arc.Center.X * sign, row.Arc.Center.Y, row.Arc.Center.Z),
                sign > 0 ? row.Arc.Sense : row.Arc.Sense.Flipped),
            sign > 0 ? row.SweepRadians : -row.SweepRadians,
            row.Orientation));

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
        new(profile.Vertices.Min(static point => point.X) + axialAllowance,
            profile.Vertices.Max(static point => point.X) + axialAllowance,
            side.StockRadius(demand.Stock),
            side.Target(profile, radialAllowance),
            side,
            RemovesMaterial: true);

    private static Fin<Loop> Material(Loop profile, TurnStock stock, CutSide side) {
        double rim = side.Clear(side.StockRadius(stock), profile.Tolerance.Absolute.Value);
        Point3d first = profile.At(0);
        Point3d last = profile.At(profile.Count - 1);
        return Loop.Admit(
            profile.Vertices.Add(new Point3d(last.X, rim, 0.0)).Add(new Point3d(first.X, rim, 0.0)),
            closed: true,
            profile.Bulges.Add(0.0).Add(0.0),
            profile.Tolerance);
    }

    private static Fin<Seq<(double Start, double End)>> Crossings(Loop material, double radius, TurnDemand demand) =>
        Runs(material, new Edge3(
                new Point3d(demand.Stock.AxialMinimum - demand.Policy.Approach.Millimeters, radius, 0.0),
                new Point3d(demand.Stock.AxialMaximum + demand.Policy.Approach.Millimeters, radius, 0.0)))
            .Map(static runs => runs.Map(static run => (
                double.Min(run.A.X, run.B.X),
                double.Max(run.A.X, run.B.X))));

    private static Fin<double> RadiusAt(Loop material, double axial, TurnDemand demand, CutSide side) =>
        Runs(material, new Edge3(
                new Point3d(axial, side.Clear(side.StockRadius(demand.Stock), demand.Policy.Approach.Millimeters), 0.0),
                new Point3d(axial, side.RadialSign > 0 ? demand.Stock.InnerRadius : demand.Stock.OuterRadius, 0.0)))
            .Map(runs => runs
                .Bind(static run => Seq(run.A.Y, run.B.Y))
                .Fold(side.StockRadius(demand.Stock), side.RadialSign > 0 ? double.Min : double.Max));

    private static Fin<Seq<Edge3>> Runs(Loop material, Edge3 drive) =>
        PolygonAlgebra.Apply(new PolygonOp.ClipOpen(Seq(Seq(drive)), Seq(material), PolygonFill.NonZero))
            .Bind(static trace => trace
                .Runs(new KernelFault.InvalidValue("turning", "turning:material-clip"))
                .Map(static split => split.Inside.Bind(identity)));

    private static double Feed(TurnDemand demand, double radius) =>
        double.Min(demand.Cutting.Feed, demand.Budget.FeedPerRevolution)
        * demand.Spindle.RpmAt(Math.Abs(radius), Surface(demand));

    private static double Surface(TurnDemand demand) => double.Min(demand.Cutting.SurfaceSpeed, demand.Budget.SurfaceSpeed);

    private static RotationSense Hand(ThreadHand hand) =>
        hand == ThreadHand.Right ? RotationSense.Clockwise : RotationSense.Counterclockwise;

    private static SpindleMode Held(TurnDemand demand, double rpm) => new SpindleMode.ConstantRpm(
        demand.Spindle.MinimumRadius, RotationalSpeed.FromRevolutionsPerMinute(rpm));

    private static readonly Op Key = Op.Of(name: nameof(Turning));

    private const int SingleEdge = 1;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
