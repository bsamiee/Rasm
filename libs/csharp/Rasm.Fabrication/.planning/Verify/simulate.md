# [RASM_FABRICATION_SIMULATE]

`Simulate.Execute` admits one landed `CutProgram`, evaluates its controller semantics without replanning, and emits the authoritative `SimulationReceipt` clock. `GCommand.Grammar` owns syntactic admission; simulation owns relational motion admission, modal execution, machine-limit evidence, coordinated timing, energy, and terminal state.

`GNode.Switch` derives executable leaves without a second AST. `CommandEffect` is the one admission table keyed by `GCommand`, so a command with a distinct physical effect is one row and every other command inherits its `ModalGroup` behavior. `SimulationSlice` is the sole ledger family, and every `SimulationReceipt` projection folds that ledger.

## [01]-[INDEX]

- [02]-[SIMULATE]: owns `SimulatePolicy`, `ControllerState`, `CommandEffect`, `Instruction`, `SimulationSlice`, `SimulationReceipt`, and `Simulate.Execute`.

## [02]-[SIMULATE]

- Owner: `SimulatePolicy` composes `MotionDynamics`, `AxisMotion`, assessed `MachineMatch` envelope and power truth, power-on modal defaults, work offsets, tool lengths, controller timing, nesting depth, and energy policy. `ControllerState` carries one canonical modal map with physical registers and the active local frame. `SimulationReceipt` carries the derived clock, energy, per-band motion tallies, per-kind delay tallies, ledger, and terminal state.
- Cases: `CommandEffect` reduces a word to motion, dwell, tool change, halt, constant-surface-speed, spindle, auxiliary, thermal, frame, or modal admission. `SimulationSlice` distinguishes motion, controller delay, additive deposition, specialized toolpath evidence, and state evidence. Specialized envelopes retain their typed rows; a direct specialized program contributes envelope duration and machine energy, while evidence attached to realized motion contributes no duplicate clock.
- Entry: `public static Fin<SimulationReceipt> Execute(CutProgram program, SimulatePolicy policy)` admits the aggregate program once, consumes the generated policy admission, folds executable `GNode` leaves through one `ControllerState`, and fails before ledger mutation on malformed inverse-time feed, inconsistent offset- or radius-defined arcs, unbanded motion role, missing rotary truth, envelope breach, nesting beyond the admitted depth, or execution after terminal stop.
- Auto: `GCommand.Grammar.Admit` validates address shape and `GCommand.Role` selects the clock band. `ArcEvidence.Validate` admits start/end radius consistency and derives machine-axis extrema after the active work transform. `AxisMotion` supplies bounded or cyclic rotary truth, per-axis velocity, acceleration, and jerk in radians. `MotionDynamics` supplies rapid, linear, arc, and rotary feed ceilings with acceleration and jerk already stamped by `Posting/program`.
- Receipt: `SimulationReceipt.Cycle` sums `SimulationSlice.Elapsed` over the whole ledger and `EnergyKwh` sums `SimulationSlice.EnergyKwh`, so no projection can disagree with the ledger. `Bands` and `Delays` are folds keyed by `ClockBand` and `DelayKind`, so a new band or delay row reports without a receipt edit.
- Packages: `Posting/program` (`CutProgram`, `GNode`, `GCommand`, `GParam`, `ModalGroup`, `MotionRole`, `FeedMode`); `Kinematics/machine` (`MotionDynamics`, `AxisMotion`, `AxisPeriodicity`); `Kinematics/fleet` (`MachineMatch.PowerKw`, `MachineInstance.Envelope`, `MachineInstance.IdlePowerKw`); `Process/faults` (`FabricationFault.EnvelopeExceeded`, `FabricationFault.SimulatedOvertravel`); `NodaTime` (`Duration`); `UnitsNet` (`Angle`, `Power`, `Duration`, `Energy`); `Rhino.Geometry`; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: controller latency is one `DelayKind` policy row; a command with a distinct physical effect is one `CommandEffect` row and one `Effects` entry; a coordinate-transform command is one `FrameEffect` row; a new AST case extends the generated `GNode.Switch`; a new machine axis is one `AxisMotion` row.
- Boundary: simulation evaluates posted intent and never rewrites feeds, geometry, or sequence. `Posting/program` owns parse, expansion, and look-ahead. `Kinematics/machine` owns dynamics and axis limits. `ArcEvidence.Witnesses`, `ArcEvidence.SweepFor`, `RadiusDefinition`, and `ProfileSeconds` are the numeric-kernel statement exemptions. Machine-less simulation omits envelope and machine-energy gates but retains program, arc, feed, and rotary admission. Every successful ledger sums exactly to the receipt.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using NodaTime;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Posting;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Verify;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ClockBand {
    public static readonly ClockBand Rapid = new("rapid", MotionRole.Control);
    public static readonly ClockBand Cutting = new("cutting", MotionRole.Cutting);
    public static readonly ClockBand Probing = new("probing", MotionRole.Probing);
    public static readonly ClockBand Deposition = new("deposition", MotionRole.Additive);

    public MotionRole Role { get; }

    public static Option<ClockBand> Of(MotionRole role) => toSeq(Items).Find(band => band.Role == role);
}

[SmartEnum<string>]
public sealed partial class DelayKind {
    public static readonly DelayKind Dwell = new("dwell");
    public static readonly DelayKind Pierce = new("pierce");
    public static readonly DelayKind ToolChange = new("tool-change");
    public static readonly DelayKind RequiredStop = new("required-stop");
    public static readonly DelayKind OptionalStop = new("optional-stop");
    public static readonly DelayKind SpindleRamp = new("spindle-ramp");
    public static readonly DelayKind ThermalRamp = new("thermal-ramp");
    public static readonly DelayKind AuxiliaryStabilization = new("auxiliary-stabilization");
}

[SmartEnum]
public sealed partial class ThermalAction {
    public static readonly ThermalAction HotendSet = new(GCommand.HotendTemp, targetsBed: false, waits: false);
    public static readonly ThermalAction HotendWait = new(GCommand.HotendWait, targetsBed: false, waits: true);
    public static readonly ThermalAction BedSet = new(GCommand.BedTemp, targetsBed: true, waits: false);
    public static readonly ThermalAction BedWait = new(GCommand.BedWait, targetsBed: true, waits: true);

    public GCommand Command { get; }
    public bool TargetsBed { get; }
    public bool Waits { get; }

    public static Option<ThermalAction> Of(GCommand command) => toSeq(Items).Find(action => action.Command == command);
}

[SmartEnum]
public sealed partial class FrameEffect {
    public static readonly FrameEffect Shift = new(GCommand.LocalShift, static (state, word) => state with {
        Shift = Transform.Translation(word.P('X').IfNone(0.0), word.P('Y').IfNone(0.0), word.P('Z').IfNone(0.0)),
    });
    public static readonly FrameEffect Rotate = new(GCommand.Rotate, static (state, word) => state with {
        Rotation = state.Rotation * Transform.Rotation(
            UnitsNet.Angle.FromDegrees(word.P('R').IfNone(0.0)).Radians,
            Vector3d.ZAxis,
            new Point3d(word.P('X').IfNone(0.0), word.P('Y').IfNone(0.0), 0.0)),
    });
    public static readonly FrameEffect Scale = new(GCommand.Scale, static (state, word) => state with {
        Scale = state.Scale * Transform.Scale(
            new Plane(new Point3d(word.P('X').IfNone(0.0), word.P('Y').IfNone(0.0), word.P('Z').IfNone(0.0)),
                Vector3d.XAxis, Vector3d.YAxis),
            word.P('P').IfNone(1.0), word.P('P').IfNone(1.0), word.P('P').IfNone(1.0)),
    });
    public static readonly FrameEffect ClearRotation = new(GCommand.RotateCancel, static (state, _) => state with {
        Rotation = Transform.Identity,
    });
    public static readonly FrameEffect ClearScale = new(GCommand.ScaleCancel, static (state, _) => state with {
        Scale = Transform.Identity,
    });

    public GCommand Command { get; }

    [UseDelegateFromConstructor]
    public partial FrameState Apply(FrameState state, GNode.Word word);

    public static Option<FrameEffect> Of(GCommand command) => toSeq(Items).Find(effect => effect.Command == command);
}

public sealed record FrameState(Transform Shift, Transform Rotation, Transform Scale) {
    public static readonly FrameState Identity = new(Transform.Identity, Transform.Identity, Transform.Identity);
    public Transform Combined => Shift * Rotation * Scale;

    public FrameState Apply(FrameEffect effect, GNode.Word word) => effect.Apply(this, word);
}

[ComplexValueObject]
public sealed partial class ControllerTiming {
    public Map<DelayKind, Duration> Fixed { get; }
    public double SpindleRevolutionsPerSecondSquared { get; }
    public double HotendDegreesPerSecond { get; }
    public double BedDegreesPerSecond { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Map<DelayKind, Duration> fixed,
        ref double spindleRevolutionsPerSecondSquared,
        ref double hotendDegreesPerSecond,
        ref double bedDegreesPerSecond) {
        bool complete = toSeq(DelayKind.Items).ForAll(fixed.ContainsKey);
        bool nonnegative = fixed.ForAll(static row => row.Value >= Duration.Zero);
        bool rates = new[] { spindleRevolutionsPerSecondSquared, hotendDegreesPerSecond, bedDegreesPerSecond }
            .ForAll(static value => double.IsFinite(value) && value > 0.0);
        if (!complete || !nonnegative || !rates)
            validationError = new ValidationError(message: "controller timing requires one nonnegative delay per kind and finite positive slew rates");
    }
}

[ComplexValueObject]
public sealed partial class SimulatePolicy {
    public Option<MachineMatch> Machine { get; }
    public Seq<AxisMotion> Axes { get; }
    public MotionDynamics Dynamics { get; }
    public Map<ModalGroup, GCommand> PowerOn { get; }
    public Map<int, Transform> WorkOffsets { get; }
    public Map<int, double> ToolLengthsMm { get; }
    public ControllerTiming Timing { get; }
    public int MaximumNesting { get; }
    public double SoftLimitMarginMm { get; }
    public double ActivePowerFactor { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Option<MachineMatch> machine,
        ref Seq<AxisMotion> axes,
        ref MotionDynamics dynamics,
        ref Map<ModalGroup, GCommand> powerOn,
        ref Map<int, Transform> workOffsets,
        ref Map<int, double> toolLengthsMm,
        ref ControllerTiming timing,
        ref int maximumNesting,
        ref double softLimitMarginMm,
        ref double activePowerFactor) {
        bool axisIdentity = axes.Map(static axis => axis.Axis).Distinct().Count == axes.Count;
        bool defaults = toSeq(ModalGroup.Items)
            .Filter(static group => group != ModalGroup.NonModal && group != ModalGroup.Stop)
            .ForAll(group => powerOn.Find(group).Exists(command => command is not null && command.Group == group));
        bool offsets = workOffsets.Find(1).IsSome
            && workOffsets.ForAll(static row => row.Key > 0 && row.Value.IsValid);
        bool tools = toolLengthsMm.ForAll(static row => row.Key > 0 && double.IsFinite(row.Value) && row.Value >= 0.0);
        bool scalars = maximumNesting > 0 && double.IsFinite(softLimitMarginMm) && softLimitMarginMm >= 0.0
            && double.IsFinite(activePowerFactor) && activePowerFactor is >= 0.0 and <= 1.0;
        bool assessed = machine.ForAll(static value => value is not null && value.Instance is not null && value.Checks.Feasible
            && double.IsFinite(value.PowerKw) && value.PowerKw >= 0.0);
        if (dynamics is null || timing is null || axes.Exists(static axis => axis is null) || !axisIdentity || !defaults
            || !offsets || !tools || !scalars || !assessed)
            validationError = new ValidationError(message: "simulation policy requires coherent dynamics, axes, power-on defaults, offsets, tools, timing, nesting, margins, and power factor");
    }
}

public sealed record ProgramLocus(int Block, Seq<int> Path);

[ComplexValueObject]
public sealed partial class ArcEvidence {
    public Plane Plane { get; }
    public Point3d From { get; }
    public Point3d To { get; }
    public Point3d Center { get; }
    public RotationSense Sense { get; }

    public double RadiusMm => Center.DistanceTo(Plane.ClosestPoint(From));
    public double StartRad => AngleOf(Plane, Center, From);
    public double SweepRad => SweepFor(Plane, Center, From, To, Sense);
    public double HelicalRiseMm => Plane.DistanceTo(To) - Plane.DistanceTo(From);
    public double LengthMm => Math.Sqrt(Math.Pow(RadiusMm * SweepRad, 2.0) + Math.Pow(HelicalRiseMm, 2.0));

    public static double AngleOf(Plane plane, Point3d center, Point3d at) =>
        Math.Atan2((at - center) * plane.YAxis, (at - center) * plane.XAxis);

    public static double Advance(double fromRad, double toRad, RotationSense sense) {
        double raw = sense == RotationSense.Clockwise ? fromRad - toRad : toRad - fromRad;
        return (raw % (2.0 * Math.PI) + 2.0 * Math.PI) % (2.0 * Math.PI);
    }

    public static double SweepFor(Plane plane, Point3d center, Point3d from, Point3d to, RotationSense sense) =>
        Advance(AngleOf(plane, center, from), AngleOf(plane, center, to), sense) switch {
            <= 1e-9 => 2.0 * Math.PI,
            double sweep => sweep,
        };

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Plane plane,
        ref Point3d from,
        ref Point3d to,
        ref Point3d center,
        ref RotationSense sense) {
        double radiusMm = center.DistanceTo(plane.ClosestPoint(from));
        double endRadius = center.DistanceTo(plane.ClosestPoint(to));
        double tolerance = Math.Max(1e-6, radiusMm * 1e-6);
        double centerDistance = plane.DistanceTo(center);
        bool coherent = plane.IsValid && from.IsValid && to.IsValid && center.IsValid
            && double.IsFinite(radiusMm) && radiusMm > 0.0
            && double.IsFinite(centerDistance) && Math.Abs(centerDistance) <= tolerance
            && double.IsFinite(plane.DistanceTo(to)) && Math.Abs(endRadius - radiusMm) <= tolerance
            && sense is not null;
        if (!coherent)
            validationError = new ValidationError(message: "arc evidence requires one plane, positive radius, consistent endpoint radius, bounded sweep, and one rotation sense");
    }

    public Seq<Point3d> Witnesses(Transform offset, double toolLength) {
        Vector3d worldU = offset * Plane.XAxis, worldV = offset * Plane.YAxis, worldW = offset * Plane.ZAxis;
        Vector3d toolAxis = offset * Plane.ZAxis;
        double direction = Sense == RotationSense.Clockwise ? -1.0 : 1.0;
        Seq<double> angles = Seq(
            (U: worldU.X, V: worldV.X, W: worldW.X),
            (U: worldU.Y, V: worldV.Y, W: worldW.Y),
            (U: worldU.Z, V: worldV.Z, W: worldW.Z))
            .Bind(axis => {
                double cosine = direction * RadiusMm * axis.V;
                double sine = -direction * RadiusMm * axis.U;
                double slope = HelicalRiseMm / SweepRad * axis.W;
                double amplitude = Math.Sqrt(cosine * cosine + sine * sine);
                if (amplitude <= 1e-12 || Math.Abs(slope) > amplitude) return Seq<double>();
                double phase = Math.Atan2(sine, cosine), delta = Math.Acos(-slope / amplitude);
                return Seq(phase + delta, phase - delta);
            })
            .Filter(angle => Travel(angle) <= SweepRad + 1e-9)
            .ToSeq();
        return Seq(From, To).Concat(angles.Map(angle => Center
            + RadiusMm * (Math.Cos(angle) * Plane.XAxis + Math.Sin(angle) * Plane.YAxis)
            + HelicalRiseMm * Travel(angle) / SweepRad * Plane.ZAxis))
            .Map(point => offset * point + toolLength * toolAxis);
    }

    private double Travel(double angle) => Advance(StartRad, angle, Sense);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionGeometry {
    private MotionGeometry() { }

    public sealed record Linear(double LengthMm) : MotionGeometry;
    public sealed record Arc(ArcEvidence Evidence) : MotionGeometry;

    public double LengthMm => Switch(
        linear: static value => value.LengthMm,
        arc: static value => value.Evidence.LengthMm);

    public double Ceiling(MotionDynamics dynamics, ClockBand band) => band == ClockBand.Rapid
        ? dynamics.RapidFeed
        : Switch(
            state: dynamics,
            linear: static (law, _) => law.LinearFeed,
            arc: static (law, _) => law.ArcFeed);

    public Seq<Point3d> Witnesses(Point3d end, Transform offset, double toolLength) => Switch(
        state: (End: end, Offset: offset, ToolLength: toolLength),
        linear: static (state, _) => Seq(state.End),
        arc: static (state, value) => value.Evidence.Witnesses(state.Offset, state.ToolLength));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Instruction(GCommand Command) {

    public sealed record Motion(GCommand Command, MotionGeometry Geometry, Point3d ProgramTo, Point3d MachineTo,
        double A, double B, double C, double FeedMmMinute, FeedMode Mode, double RotarySeconds, ClockBand Band) : Instruction(Command);
    public sealed record Delay(GCommand Command, DelayKind Kind, Duration Duration) : Instruction(Command);
    public sealed record Tool(GCommand Command, Option<int> Tool) : Instruction(Command);
    public sealed record Spindle(GCommand Command, double TargetRpm) : Instruction(Command);
    public sealed record Css(GCommand Command, double SurfaceMetersMinute, double MaximumRpm) : Instruction(Command);
    public sealed record Thermal(GCommand Command, ThermalAction Action, double TargetC) : Instruction(Command);
    public sealed record Frame(GCommand Command, FrameEffect Effect) : Instruction(Command);
    public sealed record Modal(GCommand Command) : Instruction(Command);
}

internal readonly record struct WordContext(ProgramLocus Locus, ControllerState State, GNode.Word Word, SimulatePolicy Policy);

[SmartEnum]
internal sealed partial class CommandEffect {
    public static readonly CommandEffect Motion = new(static context =>
        Simulate.AdmitMotion(context).Map(static value => (Instruction)value));

    public static readonly CommandEffect Dwell = new(static context => Simulate
        .PositiveSeconds(context.Word.P('P'), "simulate:dwell")
        .Map(seconds => (Instruction)new Instruction.Delay(context.Word.Command,
            context.State.Active.Find(ModalGroup.Coolant).Exists(static command => command == GCommand.AssistGas)
                ? DelayKind.Pierce : DelayKind.Dwell,
            Duration.FromSeconds(seconds))));

    public static readonly CommandEffect ToolChange = new(static context => Simulate
        .Identifier(context.Word.P('T'), "simulate:tool", static value => value > 0)
        .Bind(value => value.ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:tool").ToError()))
        .Map(value => (Instruction)new Instruction.Tool(context.Word.Command, Some(value))));

    public static readonly CommandEffect Halt = new(static context => Fin.Succ<Instruction>(
        new Instruction.Delay(context.Word.Command,
            context.Word.Command == GCommand.Stop ? DelayKind.RequiredStop : DelayKind.OptionalStop,
            context.Policy.Timing.Fixed[context.Word.Command == GCommand.Stop ? DelayKind.RequiredStop : DelayKind.OptionalStop])));

    public static readonly CommandEffect Css = new(static context =>
        from surface in context.Word.P('S').Filter(static value => double.IsFinite(value) && value > 0.0)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:css-surface").ToError())
        from maximum in context.Word.P('D').Filter(static value => double.IsFinite(value) && value > 0.0)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:css-maximum").ToError())
        select (Instruction)new Instruction.Css(context.Word.Command,
            surface * (context.State.Active.Find(ModalGroup.Units).Exists(static command => command == GCommand.Inch) ? 0.3048 : 1.0),
            maximum));

    public static readonly CommandEffect Spindle = new(static context =>
        (context.Word.Command == GCommand.SpindleStop
            ? Some(0.0)
            : context.Word.P('S').OrElse(context.State.SpindleRpm > 0.0 ? Some(context.State.SpindleRpm) : None))
        .Filter(static value => double.IsFinite(value) && value >= 0.0)
        .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:spindle-target").ToError())
        .Map(value => (Instruction)new Instruction.Spindle(context.Word.Command, value)));

    public static readonly CommandEffect Auxiliary = new(static context => Fin.Succ<Instruction>(
        new Instruction.Delay(context.Word.Command, DelayKind.AuxiliaryStabilization,
            context.Policy.Timing.Fixed[DelayKind.AuxiliaryStabilization])));

    public static readonly CommandEffect Thermal = new(static context =>
        from action in ThermalAction.Of(context.Word.Command)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:thermal-command").ToError())
        from target in context.Word.P('S').Filter(static value => double.IsFinite(value) && value >= 0.0)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:thermal-target").ToError())
        select (Instruction)new Instruction.Thermal(context.Word.Command, action, target));

    public static readonly CommandEffect Frame = new(static context => FrameEffect.Of(context.Word.Command)
        .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:frame-command").ToError())
        .Map(effect => (Instruction)new Instruction.Frame(context.Word.Command, effect)));

    public static readonly CommandEffect Modal = new(static context =>
        Fin.Succ<Instruction>(new Instruction.Modal(context.Word.Command)));

    [UseDelegateFromConstructor]
    public partial Fin<Instruction> Admit(WordContext context);
}

public sealed record ControllerState(
    Map<ModalGroup, GCommand> Active,
    Map<int, Transform> Offsets,
    FrameState Frame,
    Point3d ProgramAt,
    Point3d MachineAt,
    double A,
    double B,
    double C,
    double FeedMmMinute,
    double SpindleRpm,
    Option<double> CssMetersMinute,
    double CssMaximumRpm,
    Option<int> Tool,
    Option<int> LengthOffset,
    int Wcs,
    double HotendC,
    double BedC,
    double HotendTargetC,
    double BedTargetC,
    bool Stopped) {
    public static ControllerState PowerOn(SimulatePolicy policy) => new(
        policy.PowerOn, policy.WorkOffsets, FrameState.Identity, Point3d.Origin, Point3d.Origin,
        0.0, 0.0, 0.0, 0.0, 0.0, None, 0.0, None, None, 1, 0.0, 0.0, 0.0, 0.0, false);
}

public sealed record MotionTally(Duration Elapsed, double LengthMm, double PeakFeedMmMinute, int Blocks) {
    public static MotionTally Empty { get; } = new(Duration.Zero, 0.0, 0.0, 0);

    public MotionTally Add(Duration elapsed, double lengthMm, double feedMmMinute) => new(
        Elapsed + elapsed, LengthMm + lengthMm, Math.Max(PeakFeedMmMinute, feedMmMinute), Blocks + 1);
}

public sealed record DelayTally(Duration Elapsed, int Count) {
    public static DelayTally Empty { get; } = new(Duration.Zero, 0);

    public DelayTally Add(Duration elapsed) => new(Elapsed + elapsed, Count + 1);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SimulationSlice(ProgramLocus Locus) {

    public sealed record Motion(ProgramLocus Locus, GCommand Command, ClockBand Band, Duration Duration,
        double LengthMm, double FeedMmMinute, double EnergyKwh) : SimulationSlice(Locus);
    public sealed record Delay(ProgramLocus Locus, GCommand Command, DelayKind Kind, Duration Duration, double EnergyKwh) : SimulationSlice(Locus);
    public sealed record Deposition(ProgramLocus Locus, Duration Duration, double Amount, double Feed, double EnergyKwh) : SimulationSlice(Locus);
    public sealed record Specialized(ProgramLocus Locus, SpecializedToolpathEnvelope Payload, Duration Duration, double EnergyKwh) : SimulationSlice(Locus);
    public sealed record State(ProgramLocus Locus, GNode Node) : SimulationSlice(Locus);

    public Duration Elapsed => Switch(
        motion: static value => value.Duration,
        delay: static value => value.Duration,
        deposition: static value => value.Duration,
        specialized: static value => value.Duration,
        state: static _ => Duration.Zero);

    public double EnergyKwh => Switch(
        motion: static value => value.EnergyKwh,
        delay: static value => value.EnergyKwh,
        deposition: static value => value.EnergyKwh,
        specialized: static value => value.EnergyKwh,
        state: static _ => 0.0);
}

public sealed record SimulationReceipt(Seq<SimulationSlice> Ledger, ControllerState Final) {
    public Duration Cycle => Ledger.Fold(Duration.Zero, static (total, row) => total + row.Elapsed);
    public double EnergyKwh => Ledger.Fold(0.0, static (total, row) => total + row.EnergyKwh);

    public Map<ClockBand, MotionTally> Bands => Ledger.Fold(Map<ClockBand, MotionTally>(), static (tallies, row) =>
        row is SimulationSlice.Motion value
            ? tallies.AddOrUpdate(value.Band, tallies.Find(value.Band).IfNone(MotionTally.Empty)
                .Add(value.Duration, value.LengthMm, value.FeedMmMinute))
            : tallies);

    public Map<DelayKind, DelayTally> Delays => Ledger.Fold(Map<DelayKind, DelayTally>(), static (tallies, row) =>
        row is SimulationSlice.Delay value
            ? tallies.AddOrUpdate(value.Kind, tallies.Find(value.Kind).IfNone(DelayTally.Empty).Add(value.Duration))
            : tallies);

    public Duration Deposition => Ledger.Fold(Duration.Zero, static (total, row) =>
        row is SimulationSlice.Deposition value ? total + value.Duration : total);

    public Seq<SpecializedToolpathEnvelope> Specialized => Ledger.Choose(static row =>
        row is SimulationSlice.Specialized value ? Some(value.Payload) : None);

    public double DistanceMm => Bands.Fold(0.0, static (total, tally) => total + tally.LengthMm);
}

internal sealed record SimulationFold(ControllerState State, Seq<SimulationSlice> Ledger);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Simulate {
    // One row per command whose physical effect differs from its modal-group default; every unlisted command inherits
    // `CommandEffect.Modal`, and the thermal and frame rows derive from the vocabularies that already own those axes.
    private static readonly Map<GCommand, CommandEffect> Effects = Seq(
        (GCommand.Dwell, CommandEffect.Dwell),
        (GCommand.ToolChange, CommandEffect.ToolChange),
        (GCommand.Stop, CommandEffect.Halt),
        (GCommand.OptionalStop, CommandEffect.Halt),
        (GCommand.Css, CommandEffect.Css),
        (GCommand.Spindle, CommandEffect.Spindle),
        (GCommand.SpindleCcw, CommandEffect.Spindle),
        (GCommand.SpindleStop, CommandEffect.Spindle),
        (GCommand.TorchOn, CommandEffect.Auxiliary),
        (GCommand.Coolant, CommandEffect.Auxiliary),
        (GCommand.CoolantMist, CommandEffect.Auxiliary),
        (GCommand.CoolantOff, CommandEffect.Auxiliary),
        (GCommand.AssistGas, CommandEffect.Auxiliary),
        (GCommand.DustCollect, CommandEffect.Auxiliary))
        .Concat(toSeq(ThermalAction.Items).Map(static action => (action.Command, CommandEffect.Thermal)))
        .Concat(toSeq(FrameEffect.Items).Map(static effect => (effect.Command, CommandEffect.Frame)))
        .Fold(Map<GCommand, CommandEffect>(), static (table, row) => table.AddOrUpdate(row.Item1, row.Item2));

    public static Fin<SimulationReceipt> Execute(CutProgram program, SimulatePolicy policy) =>
        from admittedPolicy in Optional(policy).ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:policy-missing").ToError())
        from steps in AdmitProgram(program, admittedPolicy)
        from folded in steps.FoldM<Fin, SimulationFold>(
            new SimulationFold(ControllerState.PowerOn(admittedPolicy), Seq<SimulationSlice>()),
            (state, step) => ExecuteStep(state, step, admittedPolicy)).As()
        select new SimulationReceipt(folded.Ledger, folded.State);

    private static Fin<Seq<(ProgramLocus Locus, GNode Node)>> AdmitProgram(CutProgram program, SimulatePolicy policy) =>
        Optional(program).ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:program-missing").ToError()).Bind(value =>
            value.Nodes.IsEmpty
                ? Fin.Fail<Seq<(ProgramLocus, GNode)>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:empty-program").ToError())
                : Flatten(value.Nodes, Seq<int>(), policy));

    private static Fin<Seq<(ProgramLocus Locus, GNode Node)>> Flatten(Seq<GNode> nodes, Seq<int> path, SimulatePolicy policy) =>
        path.Count > policy.MaximumNesting
            ? Fin.Fail<Seq<(ProgramLocus, GNode)>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:nesting-depth").ToError())
            : nodes.Map((node, block) => (node, locus: new ProgramLocus(block, path.Add(block))))
                .TraverseM(item => AdmitNode(item.locus, item.node, policy)).As()
                .Map(static groups => groups.Fold(Seq<(ProgramLocus, GNode)>(), static (all, group) => all.Concat(group)));

    private static Fin<Seq<(ProgramLocus Locus, GNode Node)>> AdmitNode(ProgramLocus locus, GNode node, SimulatePolicy policy) => node.Switch(
        state: (Locus: locus, Policy: policy),
        block: static (at, value) => Flatten(value.Body.ToSeq(), at.Locus.Path, at.Policy),
        word: static (at, value) => value.Command.Grammar.Admit(at.Locus.Block, value.Words, value.Command.Group)
            .Map(_ => Seq((at.Locus, (GNode)value))),
        cannedCycle: static (at, value) => value.Repeats > 0
            ? Fin.Succ(Seq((at.Locus, (GNode)value)))
            : Fin.Fail<Seq<(ProgramLocus, GNode)>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:cycle-repeats").ToError()),
        coordinateFrame: static (at, value) => Fin.Succ(Seq((at.Locus, (GNode)value))),
        macro: static (at, value) => Flatten(value.Body.ToSeq(), at.Locus.Path, at.Policy),
        subprogram: static (at, value) => value.Repeats > 0
            ? Range(0, value.Repeats).Map(index => Flatten(value.Body.ToSeq(), at.Locus.Path.Add(index), at.Policy)).TraverseM(identity).As()
                .Map(static groups => groups.Fold(Seq<(ProgramLocus, GNode)>(), static (all, group) => all.Concat(group)))
            : Fin.Fail<Seq<(ProgramLocus, GNode)>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:subprogram-repeats").ToError()),
        additiveLayer: static (at, value) => Fin.Succ(Seq((at.Locus, (GNode)value))),
        nc1: static (_, _) => Fin.Fail<Seq<(ProgramLocus, GNode)>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:nc1-clock-owner-required").ToError()),
        directive: static (at, value) => Fin.Succ(Seq((at.Locus, (GNode)value))));

    private static Fin<SimulationFold> ExecuteStep(SimulationFold fold, (ProgramLocus Locus, GNode Node) step, SimulatePolicy policy) =>
        fold.State.Stopped
            ? Fin.Fail<SimulationFold>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:after-stop").ToError())
            : step.Node.Switch(
                state: (Fold: fold, Locus: step.Locus, Policy: policy),
                block: static (_, _) => Fin.Fail<SimulationFold>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:unflattened-block").ToError()),
                word: static (context, value) => ExecuteWord(context.Fold, context.Locus, value, context.Policy),
                cannedCycle: static (context, value) => ExecuteCycle(context.Fold, context.Locus, value, context.Policy),
                coordinateFrame: static (context, value) => Fin.Succ(new SimulationFold(
                    context.Fold.State with { Offsets = context.Fold.State.Offsets.AddOrUpdate(
                        value.Assignment.Setup, Transform.PlaneToPlane(Plane.WorldXY, value.Frame)) },
                    context.Fold.Ledger.Add(new SimulationSlice.State(context.Locus, value)))),
                macro: static (_, _) => Fin.Fail<SimulationFold>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:unflattened-macro").ToError()),
                subprogram: static (_, _) => Fin.Fail<SimulationFold>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:unflattened-subprogram").ToError()),
                additiveLayer: static (context, value) => ExecuteAdditive(context.Fold, context.Locus, value, context.Policy),
                nc1: static (_, _) => Fin.Fail<SimulationFold>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:nc1-clock-owner-required").ToError()),
                directive: static (context, value) => ExecuteDirective(context.Fold, context.Locus, value.Value, context.Policy));

    private static Fin<SimulationFold> ExecuteDirective(
        SimulationFold fold,
        ProgramLocus locus,
        MotionDirective directive,
        SimulatePolicy policy) => directive.Switch(
        state: (Fold: fold, Locus: locus, Policy: policy),
        spindle: static (context, row) => ApplyDelay(
            context.Fold with { State = context.Fold.State with { SpindleRpm = row.ResolvedRpm } },
            context.Locus,
            GCommand.Spindle,
            DelayKind.SpindleRamp,
            Duration.FromSeconds(Math.Abs(row.ResolvedRpm - context.Fold.State.SpindleRpm)
                / context.Policy.Timing.SpindleRevolutionsPerSecondSquared / 60.0),
            context.Policy),
        dwell: static (context, row) => context.Fold.State.SpindleRpm > 0.0
            ? ApplyDelay(context.Fold, context.Locus, GCommand.Dwell, DelayKind.Dwell,
                Duration.FromSeconds(row.Revolutions * 60.0 / context.Fold.State.SpindleRpm), context.Policy)
            : Fin.Fail<SimulationFold>(new GeometryFault.DegenerateInput(Kind.Curve, row.AfterMove, "simulate:dwell-without-spindle").ToError()),
        synchronize: static (context, row) => Fin.Succ(context.Fold with {
            Ledger = context.Fold.Ledger.Add(new SimulationSlice.State(context.Locus, new GNode.Directive(row))),
        }),
        orientedStop: static (context, row) => Fin.Succ(context.Fold with {
            Ledger = context.Fold.Ledger.Add(new SimulationSlice.State(context.Locus, new GNode.Directive(row))),
        }),
        channelBarrier: static (context, row) => Fin.Succ(context.Fold with {
            Ledger = context.Fold.Ledger.Add(new SimulationSlice.State(context.Locus, new GNode.Directive(row))),
        }),
        specialized: static (context, row) => {
            Duration elapsed = Duration.FromSeconds(row.AfterMove < 0 ? row.Payload.DurationSeconds : 0.0);
            return row.Payload.IsValid
                ? Fin.Succ(context.Fold with {
                    Ledger = context.Fold.Ledger.Add(new SimulationSlice.Specialized(
                        context.Locus,
                        row.Payload,
                        elapsed,
                        context.Policy.Machine.Map(machine => EnergyKwh(
                            machine.PowerKw * context.Policy.ActivePowerFactor, elapsed)).IfNone(0.0))),
                })
                : Fin.Fail<SimulationFold>(new GeometryFault.DegenerateInput(
                    Kind.Curve, row.AfterMove, $"simulate:specialized:{row.Payload.Kind.Key}").ToError());
        });

    // A cycle with no expanded moves is the single-block form the post emits for dwell-shaped cycles; its own words
    // carry the whole effect, so the cycle body is the command word itself rather than an expansion.
    private static Fin<SimulationFold> ExecuteCycle(SimulationFold fold, ProgramLocus locus, GNode.CannedCycle cycle, SimulatePolicy policy) =>
        Range(0, cycle.Repeats).FoldM<Fin, SimulationFold>(fold, (state, _) => cycle.ExpandedMoves.IsEmpty
            ? ExecuteWord(state, locus, new GNode.Word(cycle.Command, cycle.SingleBlockWords, cycle.Mode), policy)
            : cycle.ExpandedMoves.FoldM<Fin, SimulationFold>(state, (nested, move) =>
                GNode.Move(move, nested.State.ProgramAt) is GNode.Word word
                    ? ExecuteWord(nested, locus, word with { Mode = cycle.Mode }, policy)
                    : Fin.Fail<SimulationFold>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:cycle-move").ToError())).As()).As();

    private static Fin<SimulationFold> ExecuteWord(SimulationFold fold, ProgramLocus locus, GNode.Word word, SimulatePolicy policy) =>
        from instruction in AdmitInstruction(new WordContext(locus, fold.State, word, policy))
        from advanced in Apply(fold, locus, word, instruction, policy)
        select advanced;

    private static Fin<Instruction> AdmitInstruction(WordContext context) =>
        (context.Word.Command.Group == ModalGroup.Motion
            ? CommandEffect.Motion
            : Effects.Find(context.Word.Command).IfNone(CommandEffect.Modal)).Admit(context);

    internal static Fin<Instruction.Motion> AdmitMotion(WordContext context) {
        (ControllerState state, GNode.Word word, SimulatePolicy policy) = (context.State, context.Word, context.Policy);
        Map<ModalGroup, GCommand> active = Stamp(state.Active, word);
        double scale = 1.0;
        bool relative = active.Find(ModalGroup.Distance).Exists(static command => command == GCommand.Relative);
        Point3d programTo = Target(state.ProgramAt, word, scale, relative);
        FeedMode mode = word.Command == GCommand.Rapid ? FeedMode.UnitsPerMinute
            : word.Mode.IfNone(active.Find(ModalGroup.Feed).Exists(static command => command == GCommand.FeedInverseTime)
                ? FeedMode.InverseTime : FeedMode.UnitsPerMinute);
        Option<double> feed = word.Command == GCommand.Rapid ? Some(policy.Dynamics.RapidFeed)
            : mode == FeedMode.InverseTime ? word.P('F') : word.P('F').Map(value => value * scale).OrElse(Some(state.FeedMmMinute));
        return from offset in state.Offsets.Find(state.Wcs).ToFin(new GeometryFault.DegenerateInput(
                       Kind.Mesh, context.Locus.Block, $"simulate:wcs-reference:{state.Wcs}").ToError())
               from length in state.LengthOffset.Traverse(value => policy.ToolLengthsMm.Find(value)
                   .ToFin(new GeometryFault.DegenerateInput(
                       Kind.Mesh, context.Locus.Block, $"simulate:length-reference:{value}").ToError())).As()
               let toolLength = length.IfNone(0.0)
               let machineTo = offset * (state.Frame.Combined * programTo) + toolLength * (offset * Vector3d.ZAxis)
               from band in ClockBand.Of(word.Command.Role)
                   .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:motion-band").ToError())
               from admittedFeed in feed.Filter(static value => double.IsFinite(value) && value > 0.0)
                   .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:motion-feed").ToError())
               from geometry in Geometry(state.ProgramAt, programTo, word, active, scale)
               from rotary in RotarySeconds(context.Locus, state, word, relative, policy)
               from _ in Gate(context.Locus, machineTo, geometry, offset * state.Frame.Combined, toolLength, word, policy)
               select new Instruction.Motion(word.Command, geometry, programTo, machineTo,
                   rotary.A, rotary.B, rotary.C, admittedFeed, mode, rotary.Seconds, band);
    }

    private static Fin<MotionGeometry> Geometry(Point3d from, Point3d to, GNode.Word word, Map<ModalGroup, GCommand> active, double scale) =>
        word.Command == GCommand.ArcCw || word.Command == GCommand.ArcCcw
            ? Arc(from, to, word, active, scale).Map(static value => (MotionGeometry)new MotionGeometry.Arc(value))
            : Fin.Succ<MotionGeometry>(new MotionGeometry.Linear(from.DistanceTo(to)));

    private static Fin<ArcEvidence> Arc(Point3d from, Point3d to, GNode.Word word, Map<ModalGroup, GCommand> active, double scale) {
        GCommand planeCommand = active.Find(ModalGroup.Plane).IfNone(GCommand.PlaneXy);
        Plane plane = planeCommand == GCommand.PlaneZx ? new Plane(from, Vector3d.ZAxis, Vector3d.XAxis)
            : planeCommand == GCommand.PlaneYz ? new Plane(from, Vector3d.YAxis, Vector3d.ZAxis)
            : new Plane(from, Vector3d.XAxis, Vector3d.YAxis);
        RotationSense sense = word.Command == GCommand.ArcCw ? RotationSense.Clockwise : RotationSense.Counterclockwise;
        double i = scale * word.P('I').IfNone(0.0), j = scale * word.P('J').IfNone(0.0), k = scale * word.P('K').IfNone(0.0);
        bool absoluteCenter = active.Find(ModalGroup.ArcDistance).Exists(static command => command == GCommand.ArcAbsolute);
        Vector3d offset = planeCommand == GCommand.PlaneZx ? new Vector3d(i, 0.0, k)
            : planeCommand == GCommand.PlaneYz ? new Vector3d(0.0, j, k) : new Vector3d(i, j, 0.0);
        // G90.1 spells I/J/K as absolute centre coordinates on the active plane axes; the out-of-plane ordinate stays
        // on the start point, so the same two words select between an origin-relative and a start-relative centre.
        Point3d absolute = new(
            planeCommand == GCommand.PlaneYz ? from.X : i,
            planeCommand == GCommand.PlaneZx ? from.Y : j,
            planeCommand == GCommand.PlaneXy ? from.Z : k);
        Option<double> radiusWord = word.P('R').Map(value => value * scale);
        bool carriesOffset = word.P('I').IsSome || word.P('J').IsSome || word.P('K').IsSome;
        Fin<Point3d> definition = radiusWord.Match(
            None: () => carriesOffset
                ? Fin.Succ(absoluteCenter ? absolute : from + offset)
                : Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:arc-center").ToError()),
            Some: radius => carriesOffset
                ? Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:arc-definition-conflict").ToError())
                : RadiusDefinition(radius, from, to, plane, sense));
        return definition.Bind(center => {
            ValidationError? error = ArcEvidence.Validate(plane, from, to, center, sense, out ArcEvidence? evidence);
            return error is null && evidence is not null
                ? Fin.Succ(evidence)
                : Fin.Fail<ArcEvidence>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:arc-geometry").ToError());
        });
    }

    private static Fin<Point3d> RadiusDefinition(
        double signedRadius,
        Point3d from,
        Point3d to,
        Plane plane,
        RotationSense sense) {
        Vector3d chord = plane.ClosestPoint(to) - plane.ClosestPoint(from);
        double length = chord.Length, radius = Math.Abs(signedRadius);
        if (!double.IsFinite(radius) || radius <= 0.0 || length <= 1e-9 || length > 2.0 * radius)
            return Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:arc-radius").ToError());
        Point3d midpoint = plane.ClosestPoint(from) + plane.DistanceTo(from) * plane.ZAxis + 0.5 * chord;
        Vector3d normal = Vector3d.CrossProduct(plane.ZAxis, chord) / length;
        double height = Math.Sqrt(Math.Max(0.0, radius * radius - 0.25 * length * length));
        bool major = signedRadius < 0.0;
        return Seq(midpoint + height * normal, midpoint - height * normal)
            .Find(center => major
                ? ArcEvidence.SweepFor(plane, center, from, to, sense) >= Math.PI
                : ArcEvidence.SweepFor(plane, center, from, to, sense) <= Math.PI)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:arc-radius-branch").ToError());
    }

    internal static Fin<double> PositiveSeconds(Option<double> seconds, string locus) =>
        seconds.Filter(static value => double.IsFinite(value) && value >= 0.0)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, locus).ToError());

    internal static Fin<Option<int>> Identifier(Option<double> raw, string locus, Func<int, bool> admitted) =>
        raw.Match(
            None: () => Fin.Succ<Option<int>>(None),
            Some: value => double.IsFinite(value)
                && value >= int.MinValue
                && value <= int.MaxValue
                && value == Math.Truncate(value)
                && admitted((int)value)
                    ? Fin.Succ<Option<int>>(Some((int)value))
                    : Fin.Fail<Option<int>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, locus).ToError()));

    private static Fin<(double Seconds, double A, double B, double C)> RotarySeconds(
        ProgramLocus locus, ControllerState state, GNode.Word word, bool relative, SimulatePolicy policy) {
        return from a in RotaryTarget(state.A, word.P('A'), relative, locus, MachineAxis.A)
               from b in RotaryTarget(state.B, word.P('B'), relative, locus, MachineAxis.B)
               from c in RotaryTarget(state.C, word.P('C'), relative, locus, MachineAxis.C)
               from seconds in Seq((Axis: MachineAxis.A, From: state.A, To: a),
                   (Axis: MachineAxis.B, From: state.B, To: b),
                   (Axis: MachineAxis.C, From: state.C, To: c))
                   .Filter(static row => row.From != row.To)
                   .TraverseM(row => policy.Axes.Find(axis => axis.Axis == row.Axis)
                       .ToFin(FabricationFault.SimulatedOvertravel(
                           locus.Block, row.Axis, Math.Abs(row.To - row.From)).ToError())
                       .Bind(axis => axis.Periodicity.Cyclic || axis.Contains(row.To)
                           ? Fin.Succ(RotaryProfile(axis, policy.Dynamics, Math.Abs(axis.Periodicity.Cyclic
                               ? Math.IEEERemainder(row.To - row.From, 2.0 * Math.PI) : row.To - row.From)))
                           : Fin.Fail<double>(FabricationFault.SimulatedOvertravel(
                               locus.Block, row.Axis, Math.Max(axis.Min - row.To, row.To - axis.Max)).ToError()))).As()
               select (seconds.Fold(0.0, Math.Max), a, b, c);
    }

    // Rotary travel rides the same jerk-limited profile as linear travel; the per-axis limits bound the machine-wide
    // rotary law, so the tighter of the two governs every coordinated block.
    private static double RotaryProfile(AxisMotion axis, MotionDynamics dynamics, double radians) => ProfileSeconds(
        radians,
        Math.Min(axis.MaximumVelocity, UnitsNet.Angle.FromDegrees(dynamics.RotaryFeed).Radians / 60.0),
        axis.MaximumVelocity,
        Math.Min(axis.MaximumAcceleration, dynamics.RotaryAcceleration),
        Math.Min(axis.MaximumJerk, dynamics.RotaryJerk));

    private static Fin<double> RotaryTarget(
        double held,
        Option<double> value,
        bool relative,
        ProgramLocus locus,
        MachineAxis axis) => value.Match(
            None: () => Fin.Succ(held),
            Some: raw => {
                double target = double.IsFinite(raw)
                    ? UnitsNet.Angle.FromDegrees(raw).Radians + (relative ? held : 0.0)
                    : double.NaN;
                return double.IsFinite(target)
                    ? Fin.Succ(target)
                    : Fin.Fail<double>(new GeometryFault.DegenerateInput(
                        Kind.Mesh, locus.Block, $"simulate:rotary-target:{axis.Key}").ToError());
            });

    private static Fin<Unit> Gate(
        ProgramLocus locus,
        Point3d end,
        MotionGeometry geometry,
        Transform offset,
        double toolLength,
        GNode.Word word,
        SimulatePolicy policy) =>
        policy.Machine.Match(
            None: () => Fin.Succ(unit),
            Some: machine => geometry.Witnesses(end, offset, toolLength)
                .TraverseM(point => GatePoint(locus, point, word, machine.Instance.Envelope,
                    word.Command == GCommand.Rapid ? policy.SoftLimitMarginMm : 0.0)).As().Map(static _ => unit));

    private static Fin<Unit> GatePoint(ProgramLocus locus, Point3d point, GNode.Word word, BoundingBox box, double margin) {
        Seq<(MachineAxis Axis, double At, double Min, double Max)> rows = Seq(
            (MachineAxis.X, point.X, box.Min.X + margin, box.Max.X - margin),
            (MachineAxis.Y, point.Y, box.Min.Y + margin, box.Max.Y - margin),
            (MachineAxis.Z, point.Z, box.Min.Z + margin, box.Max.Z - margin));
        return rows.Find(static row => row.At < row.Min || row.At > row.Max).Match(
            None: () => Fin.Succ(unit),
            Some: row => word.Command == GCommand.Rapid
                ? Fin.Fail<Unit>(FabricationFault.SimulatedOvertravel(locus.Block, row.Axis, Math.Max(row.Min - row.At, row.At - row.Max)).ToError())
                : Fin.Fail<Unit>(FabricationFault.EnvelopeExceeded(row.Axis, row.At, row.At < row.Min ? row.Min : row.Max).ToError()));
    }

    private static Fin<SimulationFold> Apply(SimulationFold fold, ProgramLocus locus, GNode.Word word, Instruction instruction, SimulatePolicy policy) =>
        instruction.Switch(
            state: (Fold: fold, Locus: locus, Word: word, Policy: policy),
            motion: static (context, value) => ApplyMotion(context.Fold, context.Locus, context.Word, value, context.Policy),
            delay: static (context, value) => ApplyDelay(context.Fold, context.Locus, value.Command, value.Kind, value.Duration, context.Policy),
            tool: static (context, value) => ApplyTool(context.Fold, context.Locus, value, context.Policy),
            spindle: static (context, value) => ApplySpindle(context.Fold, context.Locus, context.Word, value, context.Policy),
            css: static (context, value) => ApplyCss(context.Fold, context.Locus, context.Word, value, context.Policy),
            thermal: static (context, value) => ApplyThermal(context.Fold, context.Locus, value, context.Policy),
            frame: static (context, value) => ApplyModal(context.Fold, context.Locus, context.Word,
                context.Fold.State.Frame.Apply(value.Effect, context.Word), context.Policy),
            modal: static (context, _) => ApplyModal(
                context.Fold, context.Locus, context.Word, context.Fold.State.Frame, context.Policy));

    private static Fin<SimulationFold> ApplyMotion(SimulationFold fold, ProgramLocus locus, GNode.Word word, Instruction.Motion motion, SimulatePolicy policy) {
        double linearSeconds = motion.Mode == FeedMode.InverseTime
            ? 60.0 / motion.FeedMmMinute
            : ProfileSeconds(motion.Geometry.LengthMm, motion.FeedMmMinute / 60.0,
                motion.Geometry.Ceiling(policy.Dynamics, motion.Band) / 60.0, policy.Dynamics.Acceleration, policy.Dynamics.Jerk);
        double spindleRpm = fold.State.CssMetersMinute
            .Map(surface => CssRpm(surface, fold.State.CssMaximumRpm, motion.ProgramTo.X)).IfNone(fold.State.SpindleRpm);
        double spindleSeconds = Math.Abs(spindleRpm - fold.State.SpindleRpm)
            / policy.Timing.SpindleRevolutionsPerSecondSquared / 60.0;
        double seconds = Math.Max(linearSeconds, Math.Max(motion.RotarySeconds, spindleSeconds));
        Duration elapsed = Duration.FromSeconds(seconds);
        double power = policy.Machine.Map(machine => motion.Band == ClockBand.Rapid ? machine.Instance.IdlePowerKw : machine.PowerKw).IfNone(0.0);
        SimulationSlice row = new SimulationSlice.Motion(locus, motion.Command, motion.Band, elapsed, motion.Geometry.LengthMm,
            motion.FeedMmMinute, EnergyKwh(power * policy.ActivePowerFactor, elapsed));
        ControllerState state = fold.State with {
            Active = Stamp(fold.State.Active, word), ProgramAt = motion.ProgramTo, MachineAt = motion.MachineTo,
            A = motion.A,
            B = motion.B,
            C = motion.C,
            FeedMmMinute = motion.FeedMmMinute,
            SpindleRpm = spindleRpm,
        };
        return Fin.Succ(new SimulationFold(state, fold.Ledger.Add(row)));
    }

    private static Fin<SimulationFold> ApplyDelay(SimulationFold fold, ProgramLocus locus, GCommand command, DelayKind kind, Duration elapsed, SimulatePolicy policy) {
        double energy = policy.Machine.Map(machine => EnergyKwh(
            machine.Instance.IdlePowerKw * policy.ActivePowerFactor, elapsed)).IfNone(0.0);
        return Fin.Succ(new SimulationFold(
            fold.State with { Active = Stamp(fold.State.Active, command), Stopped = Terminal(command) },
            fold.Ledger.Add(new SimulationSlice.Delay(locus, command, kind, elapsed, energy))));
    }

    private static Fin<SimulationFold> ApplyTool(SimulationFold fold, ProgramLocus locus, Instruction.Tool tool, SimulatePolicy policy) =>
        ApplyDelay(fold with { State = fold.State with { Tool = tool.Tool, LengthOffset = None } }, locus, tool.Command,
            DelayKind.ToolChange, policy.Timing.Fixed[DelayKind.ToolChange], policy);

    private static Fin<SimulationFold> ApplySpindle(
        SimulationFold fold,
        ProgramLocus locus,
        GNode.Word word,
        Instruction.Spindle spindle,
        SimulatePolicy policy) {
        Duration elapsed = Duration.FromSeconds(Math.Abs(spindle.TargetRpm - fold.State.SpindleRpm)
            / policy.Timing.SpindleRevolutionsPerSecondSquared / 60.0);
        return ApplyDelay(fold with { State = fold.State with {
            Active = Stamp(fold.State.Active, word),
            SpindleRpm = spindle.TargetRpm,
        } }, locus, spindle.Command, DelayKind.SpindleRamp, elapsed, policy);
    }

    private static Fin<SimulationFold> ApplyCss(
        SimulationFold fold,
        ProgramLocus locus,
        GNode.Word word,
        Instruction.Css css,
        SimulatePolicy policy) {
        double target = CssRpm(css.SurfaceMetersMinute, css.MaximumRpm, fold.State.ProgramAt.X);
        Duration elapsed = Duration.FromSeconds(Math.Abs(target - fold.State.SpindleRpm)
            / policy.Timing.SpindleRevolutionsPerSecondSquared / 60.0);
        return ApplyDelay(fold with { State = fold.State with {
            Active = Stamp(fold.State.Active, word),
            SpindleRpm = target,
            CssMetersMinute = Some(css.SurfaceMetersMinute),
            CssMaximumRpm = css.MaximumRpm,
        } }, locus, css.Command, DelayKind.SpindleRamp, elapsed, policy);
    }

    private static Fin<SimulationFold> ApplyThermal(SimulationFold fold, ProgramLocus locus, Instruction.Thermal thermal, SimulatePolicy policy) {
        double current = thermal.Action.TargetsBed ? fold.State.BedC : fold.State.HotendC;
        double rate = thermal.Action.TargetsBed ? policy.Timing.BedDegreesPerSecond : policy.Timing.HotendDegreesPerSecond;
        Duration elapsed = thermal.Action.Waits ? Duration.FromSeconds(Math.Abs(thermal.TargetC - current) / rate) : Duration.Zero;
        ControllerState state = thermal.Action.TargetsBed
            ? fold.State with { BedTargetC = thermal.TargetC, BedC = thermal.Action.Waits ? thermal.TargetC : current }
            : fold.State with { HotendTargetC = thermal.TargetC, HotendC = thermal.Action.Waits ? thermal.TargetC : current };
        return ApplyDelay(fold with { State = state }, locus, thermal.Command, DelayKind.ThermalRamp, elapsed, policy);
    }

    private static Fin<SimulationFold> ApplyModal(
        SimulationFold fold,
        ProgramLocus locus,
        GNode.Word word,
        FrameState frame,
        SimulatePolicy policy) {
        GCommand command = word.Command;
        Fin<Option<int>> wcsWord = command == GCommand.Wcs
            ? Identifier(word.P('P'), "simulate:wcs", static value => value > 0)
            : Fin.Succ<Option<int>>(None);
        Fin<Option<int>> registerWord = command == GCommand.SetWcs
            ? Identifier(word.P('L'), "simulate:wcs-register", static value => value == 2)
            : Fin.Succ<Option<int>>(None);
        Fin<Option<int>> slotWord = command == GCommand.SetWcs
            ? Identifier(word.P('P'), "simulate:wcs-slot", static value => value > 0)
            : Fin.Succ<Option<int>>(None);
        Fin<Option<int>> lengthWord = command == GCommand.LengthOffset
            ? Identifier(word.P('H'), "simulate:length-offset", static value => value > 0)
            : Fin.Succ<Option<int>>(None);
        return from admittedWcs in wcsWord
               from register in registerWord
               from slot in slotWord
               from length in lengthWord
               let offsets = register.IsSome
                   ? slot.Map(value => fold.State.Offsets.AddOrUpdate(value, Transform.Translation(
                       word.P('X').IfNone(0.0), word.P('Y').IfNone(0.0), word.P('Z').IfNone(0.0))))
                       .IfNone(fold.State.Offsets)
                   : fold.State.Offsets
               from _wcs in admittedWcs.Traverse(value => offsets.Find(value)
                   .ToFin(new GeometryFault.DegenerateInput(
                       Kind.Mesh, locus.Block, $"simulate:wcs-reference:{value}").ToError())).As()
               from _length in length.Traverse(value => policy.ToolLengthsMm.Find(value)
                   .ToFin(new GeometryFault.DegenerateInput(
                       Kind.Mesh, locus.Block, $"simulate:length-reference:{value}").ToError())).As()
               let lengthOffset = command == GCommand.LengthOffset ? length
                   : command == GCommand.LengthCancel ? None : fold.State.LengthOffset
               let state = fold.State with {
                   Active = Stamp(fold.State.Active, word),
                   Offsets = offsets,
                   Frame = frame,
                   Wcs = admittedWcs.IfNone(fold.State.Wcs),
                   LengthOffset = lengthOffset,
                   SpindleRpm = word.P('S').IfNone(fold.State.SpindleRpm),
                   CssMetersMinute = command == GCommand.CssCancel ? None : fold.State.CssMetersMinute,
                   CssMaximumRpm = command == GCommand.CssCancel ? 0.0 : fold.State.CssMaximumRpm,
                   Stopped = Terminal(command),
               }
               select new SimulationFold(state, fold.Ledger.Add(new SimulationSlice.State(locus, word)));
    }

    private static Fin<SimulationFold> ExecuteAdditive(SimulationFold fold, ProgramLocus locus, GNode.AdditiveLayer layer, SimulatePolicy policy) =>
        double.IsFinite(layer.Extrusion.Feed) && layer.Extrusion.Feed > 0.0
        && double.IsFinite(layer.Extrusion.Amount) && layer.Extrusion.Amount >= 0.0
            ? from hotend in ApplyThermal(fold, locus,
                  new Instruction.Thermal(GCommand.HotendWait, ThermalAction.HotendWait, layer.Temperatures.Hotend), policy)
              from bed in ApplyThermal(hotend, locus,
                  new Instruction.Thermal(GCommand.BedWait, ThermalAction.BedWait, layer.Temperatures.Bed), policy)
              let elapsed = Duration.FromSeconds(layer.Extrusion.Amount / layer.Extrusion.Feed * 60.0)
              let power = policy.Machine.Map(static machine => machine.PowerKw).IfNone(0.0)
              select new SimulationFold(bed.State, bed.Ledger.Add(new SimulationSlice.Deposition(
                  locus, elapsed, layer.Extrusion.Amount, layer.Extrusion.Feed,
                  EnergyKwh(power * policy.ActivePowerFactor, elapsed))))
            : Fin.Fail<SimulationFold>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "simulate:additive-layer").ToError());

    private static double EnergyKwh(double powerKw, NodaTime.Duration elapsed) =>
        (UnitsNet.Power.FromKilowatts(powerKw) * UnitsNet.Duration.FromSeconds(elapsed.TotalSeconds)).KilowattHours;

    private static bool Terminal(GCommand command) => command == GCommand.ProgramEnd;

    private static Map<ModalGroup, GCommand> Stamp(Map<ModalGroup, GCommand> active, GNode.Word word) => Stamp(active, word.Command);

    private static Map<ModalGroup, GCommand> Stamp(Map<ModalGroup, GCommand> active, GCommand command) =>
        command.Group == ModalGroup.NonModal ? active : active.AddOrUpdate(command.Group, command);

    private static Point3d Target(Point3d from, GNode.Word word, double scale, bool relative) => new(
        Axis(from.X, word.P('X'), scale, relative), Axis(from.Y, word.P('Y'), scale, relative), Axis(from.Z, word.P('Z'), scale, relative));

    private static double Axis(double held, Option<double> value, double scale, bool relative) =>
        value.Map(raw => relative ? held + scale * raw : scale * raw).IfNone(held);

    private static double CssRpm(double surfaceMetersMinute, double maximumRpm, double diameterMm) =>
        Math.Min(maximumRpm, surfaceMetersMinute * 1000.0 / (Math.PI * Math.Max(Math.Abs(diameterMm), 1e-9)));

    // One jerk-limited seven-segment profile serves linear, arc, and rotary travel; the caller supplies the axis or
    // machine law, so no lane carries its own timing algebra.
    private static double ProfileSeconds(double distance, double target, double ceiling, double acceleration, double jerk) {
        if (distance <= 0.0) return 0.0;
        double velocity = Math.Min(target, ceiling);
        double threshold = acceleration * acceleration / jerk;
        double ramp = velocity * (velocity <= threshold
            ? 2.0 * Math.Sqrt(velocity / jerk)
            : velocity / acceleration + acceleration / jerk);
        if (distance >= ramp)
            return 2.0 * (velocity <= threshold
                ? 2.0 * Math.Sqrt(velocity / jerk)
                : velocity / acceleration + acceleration / jerk) + (distance - ramp) / velocity;
        double jerkPeak = Math.Pow(distance * Math.Sqrt(jerk) * 0.5, 2.0 / 3.0);
        double peak = jerkPeak <= threshold
            ? jerkPeak
            : 0.5 * (-threshold + Math.Sqrt(threshold * threshold + 4.0 * acceleration * distance));
        return 2.0 * (peak <= threshold
            ? 2.0 * Math.Sqrt(peak / jerk)
            : peak / acceleration + acceleration / jerk);
    }

}
```
