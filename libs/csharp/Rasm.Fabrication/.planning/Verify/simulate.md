# [RASM_FABRICATION_SIMULATE]

`Simulate.Execute` admits one `SimulatePolicy`, evaluates the motion source it carries without replanning, and emits the authoritative `SimulationReceipt` clock. `MotionSource` closes that source: a posted `CutProgram` folds through controller semantics, and a `RobotCell` folds through the sampled pose census `Kinematics/cell` resolves. `GCommand.Grammar` owns syntactic admission; simulation owns relational motion admission, modal execution, machine-limit evidence, coordinated timing, energy, and terminal state.

Every block settles at admission into ONE feed and ONE duration, so a commanded feed and a commanded block time never share a column and the peak-feed population holds millimetres per minute alone. The signed arc sweep is `Move.Circular.SweepRadians`, admitted by the S0 atom against its own sense and its `(0, Tau]` band — this page derives it once at the G-code decode seam where the program carries no sweep at all, hands it to `Move.Circular.Of`, and every reader here and at `Verify/removal` reads that one column. Spindle speed changes cost the ramp the declared envelope implies wherever they arrive, tool changes cost the `ToolChangeEvidence.Elapsed` `Tooling/magazine` derived, and constant surface speed resolves against a modal diameter rather than whichever block happens to execute. `CommandEffect` is the one admission table keyed by `GCommand`, so a command with a distinct physical effect is one row and every other command inherits its `ModalGroup` behaviour. `SimulationSlice` is the sole ledger family and every `SimulationReceipt` projection folds that ledger; `ProgramLocus` and `ProgramPathStep` arrive from `Posting/program`, so a ledger row and a program event address one execution locus in one spelling.

## [01]-[INDEX]

- [02]-[EXECUTION_SOURCE]: `MotionSource`, `SimulatePolicy`, `ControllerTiming`, `ClockBand`, `DelayKind`, `ThermalAction`, `FrameEffect`, `FrameState`, `ControllerState`.
- [03]-[BLOCK_ADMISSION]: `ArcEvidence`, `MotionGeometry`, `Instruction`, `ModalSlot`, `CommandEffect`, and the rate, geometry, and envelope gates that run before any modal register moves.
- [04]-[LEDGER]: `SimulationSlice`, `MotionTally`, `DelayTally`, `SimulationReceipt`.
- [05]-[MODAL_CLOCK]: `Simulate.Execute`, the posted and cell folds, and the one spindle-ramp, tool-change, and jerk-limited profile charges.

## [02]-[EXECUTION_SOURCE]

- Owner: `MotionSource` closes the posted-program and robot-cell lanes as the policy's own discriminant. `SimulatePolicy` composes that source with `MotionDynamics`, `AxisMotion`, assessed `MachineMatch` envelope and power truth, the magazine's tool-change census, power-on modal defaults, work offsets, tool lengths, controller timing, nesting depth, and energy policy. `ControllerTiming` owns the fixed delays the CONTROLLER times and the slew rates the ramps read. `ControllerState` carries one canonical modal map with physical registers, the constant-surface-speed diameter axis, and the active local frame.
- Cases: `ClockBand` maps a `MotionRole` onto the band its motion tallies under. `DelayKind` carries `ControllerTimed`, the column deciding whether the controller owns a delay's duration or an external producer does — a dwell reads its own program word, a tool change reads the magazine's evidence, and the two ramps derive from slew rates, so only the halts and the auxiliary stabilization demand a fixed row.
- Law: the policy admits every quantity a clock later divides by. `MotionDynamics` accelerations and jerks, `AxisMotion` per-axis accelerations and jerks, and the `ControllerTiming` slew rates are all proved finite and strictly positive HERE, so the jerk-limited profile divides with no in-body guard and no NaN can reach the authoritative clock.
- Entry: `SimulatePolicy` admits through its generated `Validate`, so a lane never re-tests a missing or empty source mid-execution.
- Auto: `ControllerState.PowerOn` seats the policy's declared defaults, so a program that states no mode still executes against one canonical modal map.
- Packages: `Posting/program` (`CutProgram`, `GNode`, `GCommand`, `GParam`, `ModalGroup`, `MotionRole`, `FeedMode`); `Kinematics/machine` (`MotionDynamics`, `AxisMotion`, `AxisPeriodicity`); `Kinematics/cell` (`RobotCell`, `CellPolicy`, `CellClock`, `CellPosedStation`, `CellAnimation`); `Kinematics/fleet` (`MachineMatch`, `MachineInstance`); `Tooling/magazine` (`ToolChangeEvidence`); `Process/physics` (`SurfaceSpeed`); `Process/faults`; `NodaTime`; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: a motion modality is one `MotionSource` case and one `Execute` arm; a controller latency is one `DelayKind` row carrying its own timing ownership; a coordinate-transform command is one `FrameEffect` row; a new machine axis is one `AxisMotion` row.
- Boundary: simulation evaluates planned intent and never rewrites feeds, geometry, or sequence. `Posting/program` owns parse, expansion, and look-ahead. `Kinematics/machine` owns dynamics and axis limits. `Kinematics/cell` owns every `Robots` member and the `Rhino3dm` alias crossing, so the cell lane consumes a provider-free station census and this page names no provider type. `Tooling/magazine` owns tool-change timing; this page consumes its rows and derives none.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Posting;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
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

// `ControllerTimed` names WHO owns a delay's duration. A dwell states its amount in its own program word, a tool
// change reads the magazine's measured evidence, and the two ramps derive from declared slew rates; only the rows
// the controller itself times demand a `ControllerTiming.Fixed` entry, so the completeness gate stops requiring
// four durations no arm ever reads.
[SmartEnum<string>]
public sealed partial class DelayKind {
    public static readonly DelayKind Dwell = new("dwell", controllerTimed: false);
    public static readonly DelayKind Pierce = new("pierce", controllerTimed: false);
    public static readonly DelayKind ToolChange = new("tool-change", controllerTimed: false);
    public static readonly DelayKind RequiredStop = new("required-stop", controllerTimed: true);
    public static readonly DelayKind OptionalStop = new("optional-stop", controllerTimed: true);
    public static readonly DelayKind SpindleRamp = new("spindle-ramp", controllerTimed: false);
    public static readonly DelayKind ThermalRamp = new("thermal-ramp", controllerTimed: false);
    public static readonly DelayKind AuxiliaryStabilization = new("auxiliary-stabilization", controllerTimed: true);

    public bool ControllerTimed { get; }
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

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record FrameState(Transform Shift, Transform Rotation, Transform Scale) {
    public static readonly FrameState Identity = new(Transform.Identity, Transform.Identity, Transform.Identity);
    public Transform Combined => Shift * Rotation * Scale;

    public FrameState Apply(FrameEffect effect, GNode.Word word) => effect.Apply(this, word);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ControllerTiming {
    public Map<DelayKind, Duration> Fixed { get; }
    public double SpindleRevolutionsPerSecondSquared { get; }
    public double HotendDegreesPerSecond { get; }
    public double BedDegreesPerSecond { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Map<DelayKind, Duration> fixed,
        ref double spindleRevolutionsPerSecondSquared,
        ref double hotendDegreesPerSecond,
        ref double bedDegreesPerSecond) {
        // Completeness runs over the CONTROLLER-timed rows alone, so the indexer every halt and stabilization arm
        // reads is total by admission while the externally timed rows demand nothing they never supply.
        bool complete = toSeq(DelayKind.Items).Filter(static row => row.ControllerTimed).ForAll(fixed.ContainsKey);
        bool nonnegative = fixed.ForAll(static row => row.Value >= Duration.Zero);
        bool rates = Seq(spindleRevolutionsPerSecondSquared, hotendDegreesPerSecond, bedDegreesPerSecond)
            .ForAll(Witness.Positive);
        if (!complete || !nonnegative || !rates)
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "controller-timing");
    }

    public static Fin<ControllerTiming> Admit(
        Map<DelayKind, Duration> fixed,
        double spindleRevolutionsPerSecondSquared,
        double hotendDegreesPerSecond,
        double bedDegreesPerSecond) =>
        Validate(fixed, spindleRevolutionsPerSecondSquared, hotendDegreesPerSecond, bedDegreesPerSecond,
            out ControllerTiming timing).Admitted(timing);
}

// `MotionSource` is the policy's own discriminant, not a second parameter beside it: the posted lane folds G-code
// leaves, the cell lane folds the sampled poses `Kinematics/cell` resolves, and one entry serves both.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionSource {
    private MotionSource() { }

    public sealed record Posted(CutProgram Program) : MotionSource;
    public sealed record Cell(RobotCell Cell, Seq<Move> Moves, CellPolicy Policy, CellClock Clock) : MotionSource;
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class SimulatePolicy {
    public MotionSource Source { get; }
    public Option<MachineMatch> Machine { get; }
    public Seq<AxisMotion> Axes { get; }
    public MotionDynamics Dynamics { get; }

    // The magazine's measured change census, keyed by the ordered slot pair the program traverses. A change this
    // page cannot find is a census gap the magazine owns and refuses here rather than collapsing onto a flat delay.
    public Map<(int From, int To), ToolChangeEvidence> ToolChanges { get; }

    public Map<ModalGroup, GCommand> PowerOn { get; }
    public Map<int, Transform> WorkOffsets { get; }
    public Map<int, double> ToolLengthsMm { get; }
    public ControllerTiming Timing { get; }
    public int MaximumNesting { get; }
    public double SoftLimitMarginMm { get; }
    public double ActivePowerFactor { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref MotionSource source,
        ref Option<MachineMatch> machine,
        ref Seq<AxisMotion> axes,
        ref MotionDynamics dynamics,
        ref Map<(int From, int To), ToolChangeEvidence> toolChanges,
        ref Map<ModalGroup, GCommand> powerOn,
        ref Map<int, Transform> workOffsets,
        ref Map<int, double> toolLengthsMm,
        ref ControllerTiming timing,
        ref int maximumNesting,
        ref double softLimitMarginMm,
        ref double activePowerFactor) {
        bool axisIdentity = axes.Map(static axis => axis.Axis).Distinct().Count == axes.Count;
        // Every quantity the jerk-limited profile divides by is proved here, so the profile carries no in-body
        // guard and no unguarded divisor can put a NaN into the authoritative clock.
        bool laws = Witness.Positive(dynamics.Acceleration) && Witness.Positive(dynamics.Jerk)
            && Witness.Positive(dynamics.RotaryAcceleration) && Witness.Positive(dynamics.RotaryJerk)
            && Seq(dynamics.RapidFeed, dynamics.LinearFeed, dynamics.ArcFeed, dynamics.RotaryFeed).ForAll(Witness.Positive)
            && axes.ForAll(static axis => Witness.Positive(axis.MaximumVelocity)
                && Witness.Positive(axis.MaximumAcceleration) && Witness.Positive(axis.MaximumJerk));
        bool defaults = toSeq(ModalGroup.Items)
            .Filter(static group => group != ModalGroup.NonModal && group != ModalGroup.Stop)
            .ForAll(group => powerOn.Find(group).Exists(command => command.Group == group));
        bool offsets = workOffsets.Find(1).IsSome
            && workOffsets.ForAll(static row => row.Key > 0 && row.Value.IsValid);
        bool tools = toolLengthsMm.ForAll(static row => row.Key > 0 && double.IsFinite(row.Value) && row.Value >= 0.0);
        // Key and payload state one fact; the magazine owns the `Elapsed = Traverse + ArmSwing` derivation and this
        // gate never re-runs it.
        bool changes = toolChanges.ForAll(static row =>
            row.Key.From == row.Value.FromSlot && row.Key.To == row.Value.ToSlot && row.Value.IndexSteps >= 0
            && row.Value.Traverse >= Duration.Zero && row.Value.ArmSwing >= Duration.Zero
            && row.Value.Elapsed >= Duration.Zero);
        bool scalars = maximumNesting > 0 && double.IsFinite(softLimitMarginMm) && softLimitMarginMm >= 0.0
            && double.IsFinite(activePowerFactor) && activePowerFactor is >= 0.0 and <= 1.0;
        bool assessed = machine.ForAll(static value => value.Checks.Feasible
            && double.IsFinite(value.PowerKw) && value.PowerKw >= 0.0);
        // The source is gated ONCE here, so no fold re-tests a missing or empty program mid-execution.
        bool sourced = source.Switch(
            posted: static row => !row.Program.Nodes.IsEmpty,
            cell: static row => !row.Moves.IsEmpty);
        if (!axisIdentity || !laws || !defaults || !offsets || !tools || !changes || !scalars || !assessed || !sourced)
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate-policy");
    }

    public static Fin<SimulatePolicy> Admit(
        MotionSource source,
        Option<MachineMatch> machine,
        Seq<AxisMotion> axes,
        MotionDynamics dynamics,
        Map<(int From, int To), ToolChangeEvidence> toolChanges,
        Map<ModalGroup, GCommand> powerOn,
        Map<int, Transform> workOffsets,
        Map<int, double> toolLengthsMm,
        ControllerTiming timing,
        int maximumNesting,
        double softLimitMarginMm,
        double activePowerFactor) =>
        Validate(source, machine, axes, dynamics, toolChanges, powerOn, workOffsets, toolLengthsMm, timing,
            maximumNesting, softLimitMarginMm, activePowerFactor, out SimulatePolicy policy).Admitted(policy);
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

    // The turned diameter constant surface speed resolves against. It is MODAL: a Z-only block leaves it untouched,
    // so that block commands no speed change and charges no ramp, and the resolution never reads whichever target
    // happens to execute.
    double CssDiameterMm,

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
        0.0, 0.0, 0.0, 0.0, 0.0, None, 0.0, 0.0, None, None, 1, 0.0, 0.0, 0.0, 0.0, false);
}
```

## [03]-[BLOCK_ADMISSION]

- Owner: `ArcEvidence` owns the working plane, the plane-projected radius, the helical rise, and the admitted `Move.Circular` every sweep read resolves through. `MotionGeometry` closes the span shape. `Instruction` closes the physical effect a word settles into. `ModalSlot` owns the address a modal command reads and the ordinal law it admits under. `CommandEffect` is the one admission table keyed by `GCommand`.
- Cases: `CommandEffect` reduces a word to motion, dwell, tool change, halt, constant-surface-speed, spindle, auxiliary, thermal, frame, or modal admission.
- Law: the signed arc sweep is `Move.Circular.SweepRadians`. G-code carries no sweep, so this page derives it ONCE at the decode seam — I/J/K or R fix the centre, the command fixes the sense, and coincident plane-projected endpoints are the full turn a G2/G3 block spells that way — then hands it to `Move.Circular.Of`, which admits sign-against-sense and the `(0, Tau]` magnitude band before the instruction exists. Every reader here and at `Verify/removal` reads that one column, so no second sweep convention, sign rule, or angular epsilon exists on either page.
- Law: a block commands either a FEED or a block TIME, and the two are different dimensions. Both settle at admission into one millimetres-per-minute reading and one duration, so the peak-feed population never mixes reciprocal minutes with real feeds and the modal feed register is written only by the blocks that command one. A units-per-minute block clamps to the machine ceiling, which is what the machine physically does; an inverse-time block whose commanded duration demands a feed above that ceiling is INFEASIBLE and refuses, because clamping it would silently report a cycle time the machine cannot achieve.
- Exemption: `ArcEvidence.Witnesses`, `ArcEvidence.Sweep`, and `Simulate.RadiusDefinition` are the numeric-kernel statement exemptions.
- Entry: `Simulate.AdmitMotion` settles geometry, rate, rotary travel, and the envelope gate before returning, so every refusal lands before a modal register moves.
- Auto: `GCommand.Grammar.Admit` validates address shape and `GCommand.Role` selects the clock band. Every dimensioned address arrives CANONICAL — the parse seam folds the active `ModalGroup.Units` row through `ProgramUnits.Canonical`, so X/Y/Z, U/V/W, I/J/K, an arc or cycle R, and a per-minute F are millimetres before simulation reads them, and a unit scale re-applied here would double-convert an inch program.
- Boundary: `Relative` is the one relative tolerance band on this cluster, and it governs both the endpoint-radius admission and the witness-amplitude degeneracy test, so the two cannot drift. `SpecializedToolpathEnvelope` proved kind correspondence, non-empty rows, and finite non-negative duration once at `Process/owner`, so no arm here revalidates a payload it was handed.

```csharp signature
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ArcEvidence {
    public Plane Plane { get; }
    public Point3d From { get; }

    // The admitted S0 atom. It carries the SIGNED sweep, the rotation sense, the centre, and the target, so this
    // page reads one sweep column and the removal verifier reads the same one.
    public Move.Circular Motion { get; }

    // The PLANE-PROJECTED radius, which the admission proves equal at both endpoints. `Motion.Radius` is the
    // centre-to-target distance and leaves the arc plane on a helix, so the two are distinct facts and the helical
    // lane reads this one.
    public double RadiusMm { get; }
    public double RiseMm { get; }

    // One relative band serves the endpoint-radius admission and the witness-amplitude degeneracy test, so a
    // ground arc cannot pass one and fail the other.
    private const double Relative = 1e-6;

    public Point3d Center => Motion.Arc.Center;
    public RotationSense Sense => Motion.Arc.Sense;
    public double SweepRadians => Motion.SweepRadians;
    public Point3d To => Motion.Target;
    public double LengthMm => Math.Sqrt(Math.Pow(RadiusMm * Math.Abs(SweepRadians), 2.0) + (RiseMm * RiseMm));

    private double StartRad => AngleOf(Plane, Center, From);

    public static double AngleOf(Plane plane, Point3d center, Point3d at) =>
        Math.Atan2((at - center) * plane.YAxis, (at - center) * plane.XAxis);

    // The ONE sweep derivation, at the decode seam. Its result feeds `Move.Circular.Of`, whose admission is what
    // makes the value law; a coincident plane-projected endpoint pair is the full turn, decided structurally
    // against the same relative band the radius admission uses rather than by an epsilon on a computed angle.
    public static double Sweep(Plane plane, Point3d center, Point3d from, Point3d to, RotationSense sense, double radiusMm) {
        double turn = sense == RotationSense.Clockwise ? -Math.Tau : Math.Tau;
        double direction = Math.Sign(turn);
        double advance = direction * (AngleOf(plane, center, to) - AngleOf(plane, center, from));
        double wrapped = advance - (Math.Floor(advance / Math.Tau) * Math.Tau);
        return plane.ClosestPoint(from).DistanceTo(plane.ClosestPoint(to)) <= radiusMm * Relative
            ? turn
            : direction * wrapped;
    }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Plane plane,
        ref Point3d from,
        ref Move.Circular motion,
        ref double radiusMm,
        ref double riseMm) {
        double endRadius = motion.Arc.Center.DistanceTo(plane.ClosestPoint(motion.Target));
        double tolerance = radiusMm * Relative;
        bool coherent = plane.IsValid && from.IsValid
            && Witness.Positive(radiusMm) && double.IsFinite(riseMm)
            && Math.Abs(plane.DistanceTo(motion.Arc.Center)) <= tolerance
            && Math.Abs(endRadius - radiusMm) <= tolerance
            && Math.Abs(plane.ClosestPoint(from).DistanceTo(motion.Arc.Center) - radiusMm) <= tolerance;
        if (!coherent)
            validationError = new GeometryFault.DegenerateInput(Kind.Arc, None, "arc-evidence:radius").ToFabrication();
    }

    public static Fin<ArcEvidence> Admit(Plane plane, Point3d from, Move.Circular motion, double radiusMm, double riseMm) =>
        Validate(plane, from, motion, radiusMm, riseMm, out ArcEvidence evidence).Admitted(evidence);

    // Machine-axis extrema over the swept arc after the active work transform, so the envelope gate tests the real
    // excursion rather than the two endpoints. Progress rides the atom's own sign, so one comparison bounds both
    // senses and an extremum landing exactly on an endpoint is already in the seed pair.
    public Seq<Point3d> Witnesses(Transform offset, double toolLength) {
        Vector3d worldU = offset * Plane.XAxis, worldV = offset * Plane.YAxis, worldW = offset * Plane.ZAxis;
        Vector3d toolAxis = offset * Plane.ZAxis;
        double span = Math.Abs(SweepRadians), direction = Math.Sign(SweepRadians), floor = RadiusMm * Relative;
        Seq<double> angles = Seq(
            (U: worldU.X, V: worldV.X, W: worldW.X),
            (U: worldU.Y, V: worldV.Y, W: worldW.Y),
            (U: worldU.Z, V: worldV.Z, W: worldW.Z))
            .Bind(axis => {
                double cosine = direction * RadiusMm * axis.V;
                double sine = -direction * RadiusMm * axis.U;
                double slope = RiseMm / span * axis.W;
                double amplitude = Math.Sqrt((cosine * cosine) + (sine * sine));
                if (amplitude <= floor || Math.Abs(slope) > amplitude) return Seq<double>();
                double phase = Math.Atan2(sine, cosine), delta = Math.Acos(-slope / amplitude);
                return Seq(phase + delta, phase - delta);
            })
            .Filter(angle => Travel(angle) < span)
            .ToSeq();
        return Seq(From, To).Concat(angles.Map(angle => Center
                + (RadiusMm * ((Math.Cos(angle) * Plane.XAxis) + (Math.Sin(angle) * Plane.YAxis)))
                + (RiseMm * Travel(angle) / span * Plane.ZAxis)))
            .Map(point => (offset * point) + (toolLength * toolAxis));
    }

    private double Travel(double angle) {
        double raw = Math.Sign(SweepRadians) * (angle - StartRad);
        return raw - (Math.Floor(raw / Math.Tau) * Math.Tau);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionGeometry {
    private MotionGeometry() { }

    public sealed record Linear(double LengthMm) : MotionGeometry;
    public sealed record Arc(ArcEvidence Evidence) : MotionGeometry;

    public double LengthMm => Switch(
        linear: static value => value.LengthMm,
        arc: static value => value.Evidence.LengthMm);

    public Seq<Point3d> Witnesses(Point3d end, Transform offset, double toolLength) => Switch(
        state: (End: end, Offset: offset, ToolLength: toolLength),
        linear: static (state, _) => Seq(state.End),
        arc: static (state, value) => value.Evidence.Witnesses(state.Offset, state.ToolLength));
}

// `Command` is the universal column and the ROOT owns its property; each case takes it as a plain constructor
// argument spelled `command` and threads it to the base. A case re-declaring the base property's own name
// synthesizes nothing, silently drops the argument, and leaves `Instruction.Command` reading a member the case
// never wrote.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Instruction(GCommand Command) {

    // `FeedMmMinute` is the millimetres per minute the block COMMANDS in either feed mode, and `Linear` is the
    // travel duration that settles with it, so the tally aggregates one dimension and the clock reads one
    // duration. `Mode` survives because only a units-per-minute block writes the modal feed register: an inverse
    // time word states a block duration, which is not a feed and leaves the register alone.
    public sealed record Motion(GCommand command, MotionGeometry Geometry, Point3d ProgramTo, Point3d MachineTo,
        double A, double B, double C, double FeedMmMinute, Duration Linear, FeedMode Mode,
        bool CommandsDiameter, double RotarySeconds, ClockBand Band) : Instruction(command);
    public sealed record Delay(GCommand command, DelayKind Kind, Duration Duration) : Instruction(command);
    public sealed record Tool(GCommand command, int Tool) : Instruction(command);
    public sealed record Spindle(GCommand command, double TargetRpm) : Instruction(command);
    public sealed record Css(GCommand command, double SurfaceMetersMinute, double MaximumRpm) : Instruction(command);
    public sealed record Thermal(GCommand command, ThermalAction Action, double TargetC) : Instruction(command);
    public sealed record Frame(GCommand command, FrameEffect Effect) : Instruction(command);
    public sealed record Modal(GCommand command) : Instruction(command);
}

internal readonly record struct WordContext(ProgramLocus Locus, ControllerState State, GNode.Word Word, SimulatePolicy Policy);

// Every address a modal command reads is a ROW carrying its own ordinal law and refusal locus. A row produces only
// when its own command is active, so the modal fold reads its results by SLOT and tests no command of its own.
[SmartEnum<string>]
internal sealed partial class ModalSlot {
    public static readonly ModalSlot Wcs = new("wcs", GCommand.Wcs, 'P', static value => value > 0);
    public static readonly ModalSlot WcsRegister = new("wcs-register", GCommand.SetWcs, 'L', static value => value == 2);
    public static readonly ModalSlot WcsSlot = new("wcs-slot", GCommand.SetWcs, 'P', static value => value > 0);
    public static readonly ModalSlot LengthOffset = new("length-offset", GCommand.LengthOffset, 'H', static value => value > 0);

    public GCommand Command { get; }
    public char Address { get; }
    public Func<int, bool> Admits { get; }

    public string Locus => $"simulate:{Key}";

    public static Seq<ModalSlot> For(GCommand command) => toSeq(Items).Filter(row => row.Command == command);
}

[SmartEnum]
internal sealed partial class CommandEffect {
    public static readonly CommandEffect Motion = new(static context =>
        Simulate.AdmitMotion(context).Map(static value => (Instruction)value));

    public static readonly CommandEffect Dwell = new(static context => Simulate
        .NonNegativeSeconds(context.Word.P('P'), "simulate:dwell")
        .Map(seconds => (Instruction)new Instruction.Delay(context.Word.Command,
            context.State.Active.Find(ModalGroup.Coolant).Exists(static command => command == GCommand.AssistGas)
                ? DelayKind.Pierce : DelayKind.Dwell,
            Duration.FromSeconds(seconds))));

    public static readonly CommandEffect ToolChange = new(static context => Simulate
        .Ordinal(context.Word.P('T'), "simulate:tool", static value => value > 0)
        .Map(value => (Instruction)new Instruction.Tool(context.Word.Command, value)));

    public static readonly CommandEffect Halt = new(static context => Fin.Succ<Instruction>(
        new Instruction.Delay(context.Word.Command,
            context.Word.Command == GCommand.Stop ? DelayKind.RequiredStop : DelayKind.OptionalStop,
            context.Policy.Timing.Fixed[context.Word.Command == GCommand.Stop ? DelayKind.RequiredStop : DelayKind.OptionalStop])));

    // `S` is the one motion-adjacent address the parse seam does NOT canonicalize — it dimensions lengths and
    // per-minute feed alone — so surface speed still arrives in program units and an inch program states feet per
    // minute. The quantity library owns that conversion; no factor is transcribed.
    public static readonly CommandEffect Css = new(static context =>
        from surface in context.Word.P('S').Filter(Witness.Positive)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:css-surface"))
        from maximum in context.Word.P('D').Filter(Witness.Positive)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:css-maximum"))
        select (Instruction)new Instruction.Css(context.Word.Command,
            context.State.Active.Find(ModalGroup.Units).Exists(static command => command == GCommand.Inch)
                ? UnitsNet.Speed.FromFeetPerMinute(surface).MetersPerMinutes
                : surface,
            maximum));

    public static readonly CommandEffect Spindle = new(static context =>
        (context.Word.Command == GCommand.SpindleStop
            ? Some(0.0)
            : context.Word.P('S').OrElse(context.State.SpindleRpm > 0.0 ? Some(context.State.SpindleRpm) : None))
        .Filter(static value => double.IsFinite(value) && value >= 0.0)
        .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:spindle-target"))
        .Map(value => (Instruction)new Instruction.Spindle(context.Word.Command, value)));

    public static readonly CommandEffect Auxiliary = new(static context => Fin.Succ<Instruction>(
        new Instruction.Delay(context.Word.Command, DelayKind.AuxiliaryStabilization,
            context.Policy.Timing.Fixed[DelayKind.AuxiliaryStabilization])));

    public static readonly CommandEffect Thermal = new(static context =>
        from action in ThermalAction.Of(context.Word.Command)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:thermal-command"))
        from target in context.Word.P('S').Filter(static value => double.IsFinite(value) && value >= 0.0)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:thermal-target"))
        select (Instruction)new Instruction.Thermal(context.Word.Command, action, target));

    public static readonly CommandEffect Frame = new(static context => FrameEffect.Of(context.Word.Command)
        .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:frame-command"))
        .Map(effect => (Instruction)new Instruction.Frame(context.Word.Command, effect)));

    public static readonly CommandEffect Modal = new(static context =>
        Fin.Succ<Instruction>(new Instruction.Modal(context.Word.Command)));

    [UseDelegateFromConstructor]
    public partial Fin<Instruction> Admit(WordContext context);
}
```

## [04]-[LEDGER]

- Owner: `SimulationSlice` is the sole ledger family and `SimulationReceipt` folds it; `MotionTally` and `DelayTally` own the per-band and per-kind aggregates.
- Cases: `SimulationSlice` distinguishes motion, controller delay, additive deposition, specialized toolpath evidence, posed cell stations, and state evidence.
- Law: a `MotionDirective.Specialized` executed by the walk carries its ADMITTED envelope onto the ledger and out through `SimulationReceipt.Specialized`, so wire, bevel, link, inspection, and turning rows survive the modal walk instead of being consumed and dropped. A direct specialized program — one attached to no realized move — contributes its own envelope duration; evidence attached to realized motion contributes no duplicate clock, because the moves it annotates already charged theirs.
- Receipt: `Cycle` sums `SimulationSlice.Elapsed` over the whole ledger and `EnergyKwh` sums `SimulationSlice.EnergyKwh`, so no projection can disagree with the ledger. `Bands`, `Delays`, and `Poses` are folds keyed by `ClockBand`, `DelayKind`, and the posed payload, so a new band, delay row, or cell station reports with no receipt edit, and `DistanceMm` sums banded length beside posed travel so a cell cycle never reports a band-only zero. `MotionTally.PeakFeedMmMinute` aggregates one dimension, because both feed modes settled into millimetres per minute at admission.
- Boundary: `FabricationFact.Cycle.Of` projects `Cycle`, `EnergyKwh`, and `DistanceMm` onto `rasm.fabrication.cycle.duration`, `rasm.fabrication.cycle.energy`, and `rasm.fabrication.cycle.distance` through `Process/telemetry#FACT_PROJECTION` as kind `cycle`, so the authoritative cycle-time owner is the one histogram source; those three names are the frozen read and never move.

```csharp signature
public sealed record MotionTally(Duration Elapsed, double LengthMm, double PeakFeedMmMinute, int Blocks) {
    public static MotionTally Empty { get; } = new(Duration.Zero, 0.0, 0.0, 0);

    public MotionTally Add(Duration elapsed, double lengthMm, double feedMmMinute) => new(
        Elapsed + elapsed, LengthMm + lengthMm, Math.Max(PeakFeedMmMinute, feedMmMinute), Blocks + 1);
}

public sealed record DelayTally(Duration Elapsed, int Count) {
    public static DelayTally Empty { get; } = new(Duration.Zero, 0);

    public DelayTally Add(Duration elapsed) => new(Elapsed + elapsed, Count + 1);
}

// `Locus` is the universal column the root owns; each case threads it as the plain argument `locus` and never
// re-declares the base property's name.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SimulationSlice(ProgramLocus Locus) {

    public sealed record Motion(ProgramLocus locus, GCommand Command, ClockBand Band, Duration Duration,
        double LengthMm, double FeedMmMinute, double EnergyKwh) : SimulationSlice(locus);
    public sealed record Delay(ProgramLocus locus, GCommand Command, DelayKind Kind, Duration Duration, double EnergyKwh) : SimulationSlice(locus);
    public sealed record Deposition(ProgramLocus locus, Duration Duration, double Amount, double Feed, double EnergyKwh) : SimulationSlice(locus);
    public sealed record Specialized(ProgramLocus locus, SpecializedToolpathEnvelope Payload, Duration Duration, double EnergyKwh) : SimulationSlice(locus);
    public sealed record Posed(ProgramLocus locus, CellPosedStation Station, double EnergyKwh) : SimulationSlice(locus);
    public sealed record State(ProgramLocus locus, GNode Node) : SimulationSlice(locus);

    public Duration Elapsed => Switch(
        motion: static value => value.Duration,
        delay: static value => value.Duration,
        deposition: static value => value.Duration,
        specialized: static value => value.Duration,
        posed: static value => value.Station.Elapsed,
        state: static _ => Duration.Zero);

    public double EnergyKwh => Switch(
        motion: static value => value.EnergyKwh,
        delay: static value => value.EnergyKwh,
        deposition: static value => value.EnergyKwh,
        specialized: static value => value.EnergyKwh,
        posed: static value => value.EnergyKwh,
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

    // The specialized egress: every envelope the walk executed leaves whole, so a posting or estimation consumer
    // reads the wire, bevel, link, inspection, and turning rows the program carried rather than re-deriving them.
    public Seq<SpecializedToolpathEnvelope> Specialized => Ledger.Choose(static row =>
        row is SimulationSlice.Specialized value ? Some(value.Payload) : None);

    public Seq<CellPosedStation> Poses => Ledger.Choose(static row =>
        row is SimulationSlice.Posed value ? Some(value.Station) : None);

    // Travel sums both sources: a posted lane bands its motion by role, a cell lane measures flange advance between
    // posed stations, so the distance a cycle fact reports is never the zero a band-only fold reads for a cell.
    public double DistanceMm => Bands.Fold(0.0, static (total, tally) => total + tally.LengthMm)
        + Poses.Fold(0.0, static (total, station) => total + station.TravelMm);
}

internal sealed record SimulationFold(ControllerState State, Seq<SimulationSlice> Ledger);
```

## [05]-[MODAL_CLOCK]

- Owner: `Simulate` owns the execution fold, the ONE spindle-ramp charge, the ONE tool-change charge, and the ONE jerk-limited profile every linear, arc, and rotary span times through.
- Entry: `public static Fin<SimulationReceipt> Execute(SimulatePolicy policy, FabricationTap? tap = null, SpanBand? band = null)` folds executable `GNode` leaves through one `ControllerState` or folds the cell census, and fails before ledger mutation on a malformed inverse-time feed, an infeasible commanded block time, inconsistent offset- or radius-defined arcs, an unbanded motion role, missing rotary truth, an envelope breach, nesting beyond the admitted depth, execution after terminal stop, a tool change the magazine census does not carry, or a cell whose compiled program carries no simulation. The whole fold runs inside the `FabricationEngine.Simulate` bracket the supplied `SpanBand` opens, and the settled receipt fires `FabricationFact.Cycle.Of` through the supplied tap — band and tap both default absent, so a headless caller runs untraced and silent with no branch of its own.
- Law: a commanded spindle speed costs its ramp WHEREVER it arrives — an `S` word riding a modal block, an `M03` target, a constant-surface-speed resolution, or a posted spindle directive all route through one charge, so no arrival path changes the speed for free. A tool change costs `ToolChangeEvidence.Elapsed`, which `Tooling/magazine` derived from layout index distance and arm-swing policy; a change out of an empty spindle has no origin slot and therefore no index traverse, so it charges the destination row's arm swing alone.
- Law: `GCommand.ProgramEnd` is the only terminal row the program vocabulary carries. `Stop` and `OptionalStop` share its `ModalGroup.Stop` membership but are RESUMABLE halts — the operator restarts them and execution continues — so they charge their controller-timed delay and leave the run live. A second terminal word is a vocabulary row, not a predicate edit here.
- Exemption: `Simulate.ProfileSeconds` and `Simulate.RadiusDefinition` are the numeric-kernel statement exemptions; `Simulate.ApplyModal` and `Simulate.ExecuteCell` are the fold-shaped statement exemptions.
- Auto: `CommandEffect` stays a dispatch TABLE — its rows carry admission bodies rather than a shape correspondence, so no generated projection replaces it. `MotionDynamics` supplies rapid, linear, arc, and rotary ceilings with acceleration and jerk already stamped by `Posting/program`, and the cell lane reads its clock from the look-ahead planner instead, because a serial chain resolves no `ClockBand` and no axis-limit profile.
- Receipt: `ExecuteCell` proves the posed ledger sums to `CellAnimation.Cycle`, which IS the look-ahead planner's `Program.Duration`, so the cell receipt reports the planner's own clock rather than a sampler census that drifted from it.
- Boundary: a policy or parameter failing its own admission gate answers `FabricationFault.PolicyInadmissible` on its raising plane; only genuinely degenerate geometry answers the kernel `GeometryFault.DegenerateInput` band, so a missing work offset or an unresolvable tool length never borrows a fabricated `Kind`. Machine-less simulation omits envelope and machine-energy gates but retains program, arc, feed, and rotary admission. `ExecuteCell` carries the power-on controller state unchanged because a serial chain has no modal controller. Every successful ledger sums exactly to the receipt.

```csharp signature
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

    public static Fin<SimulationReceipt> Execute(SimulatePolicy policy, FabricationTap? tap = null, SpanBand? band = null) =>
        band.Traced(FabricationEngine.Simulate, Op.Of(), _ =>
            from folded in policy.Source.Switch(
                state: policy,
                posted: static (law, row) => ExecutePosted(law, row.Program),
                cell: static (law, row) => ExecuteCell(law, row))
            let receipt = new SimulationReceipt(folded.Ledger, folded.State)
            let _fact = (tap ?? FabricationTap.Silent).Fire(FabricationFact.Cycle.Of(receipt))
            select receipt);

    private static Fin<SimulationFold> ExecutePosted(SimulatePolicy policy, CutProgram program) =>
        from steps in Flatten(program.Nodes, Seq<ProgramPathStep>(), policy)
        from folded in steps.FoldM<Fin, SimulationFold>(
            new SimulationFold(ControllerState.PowerOn(policy), Seq<SimulationSlice>()),
            (state, step) => ExecuteStep(state, step, policy)).As()
        select folded;

    // `CellAnimation.Cycle` IS the look-ahead planner's `Program.Duration`, and every station measures its elapsed
    // and travel against the PRIOR posed station, so the posed ledger sums to that clock exactly. The gate proves
    // it: a sampler census that does not close on the planner's own duration would leave the receipt reporting a
    // cycle no planner produced. `Kinematics/cell` owns every `Robots` member the census is built from.
    private static Fin<SimulationFold> ExecuteCell(SimulatePolicy policy, MotionSource.Cell source) =>
        RobotProgram.Run(source.Cell, source.Moves, new CellProgramRequest.Animation(source.Policy, source.Clock))
            .Bind(receipt => receipt.Switch(
                    motion: static _ => Option<CellAnimation>.None,
                    placement: static _ => Option<CellAnimation>.None,
                    animation: static row => Some(row.Result))
                .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:cell-modality")))
            .Bind(animation => {
                Seq<SimulationSlice> ledger = animation.Stations.Map(station => (SimulationSlice)new SimulationSlice.Posed(
                    new ProgramLocus(station.Station, Seq(new ProgramPathStep(station.Station, None))),
                    station,
                    policy.Machine.Map(machine => EnergyKwh(machine.PowerKw * policy.ActivePowerFactor, station.Elapsed))
                        .IfNone(0.0)));
                return ledger.Fold(Duration.Zero, static (total, row) => total + row.Elapsed) == animation.Cycle
                    ? Fin.Succ(new SimulationFold(ControllerState.PowerOn(policy), ledger))
                    : Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:cell-clock"));
            });

    private static Fin<Seq<(ProgramLocus Locus, GNode Node)>> Flatten(Seq<GNode> nodes, Seq<ProgramPathStep> path, SimulatePolicy policy) =>
        path.Count > policy.MaximumNesting
            ? Fin.Fail<Seq<(ProgramLocus, GNode)>>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:nesting-depth"))
            : nodes.Map((node, block) => (node, locus: new ProgramLocus(block, path.Add(new ProgramPathStep(block, None)))))
                .TraverseM(item => AdmitNode(item.locus, item.node, policy)).As()
                .Map(static groups => groups.Fold(Seq<(ProgramLocus, GNode)>(), static (all, group) => all.Concat(group)));

    private static Fin<Seq<(ProgramLocus Locus, GNode Node)>> AdmitNode(ProgramLocus locus, GNode node, SimulatePolicy policy) => node.Switch(
        state: (Locus: locus, Policy: policy),
        block: static (at, value) => Flatten(value.Body.ToSeq(), at.Locus.Path, at.Policy),
        word: static (at, value) => value.Command.Grammar.Admit(at.Locus.Block, value.Words, value.Command.Group)
            .Map(_ => Seq((at.Locus, (GNode)value))),
        cannedCycle: static (at, value) => value.Repeats > 0
            ? Fin.Succ(Seq((at.Locus, (GNode)value)))
            : Fin.Fail<Seq<(ProgramLocus, GNode)>>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:cycle-repeats")),
        coordinateFrame: static (at, value) => Fin.Succ(Seq((at.Locus, (GNode)value))),
        macro: static (at, value) => Flatten(value.Body.ToSeq(), at.Locus.Path, at.Policy),
        subprogram: static (at, value) => value.Repeats > 0
            ? Range(0, value.Repeats).ToSeq()
                .Map(index => Flatten(value.Body.ToSeq(), at.Locus.Path.Init.Add(new ProgramPathStep(at.Locus.Block, Some(index))), at.Policy))
                .TraverseM(identity).As()
                .Map(static groups => groups.Fold(Seq<(ProgramLocus, GNode)>(), static (all, group) => all.Concat(group)))
            : Fin.Fail<Seq<(ProgramLocus, GNode)>>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:subprogram-repeats")),
        additiveLayer: static (at, value) => Fin.Succ(Seq((at.Locus, (GNode)value))),
        nc1: static (_, _) => Fin.Fail<Seq<(ProgramLocus, GNode)>>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:nc1-clock-owner-required")),
        directive: static (at, value) => Fin.Succ(Seq((at.Locus, (GNode)value))));

    private static Fin<SimulationFold> ExecuteStep(SimulationFold fold, (ProgramLocus Locus, GNode Node) step, SimulatePolicy policy) =>
        fold.State.Stopped
            ? Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:after-stop"))
            : step.Node.Switch(
                state: (Fold: fold, Locus: step.Locus, Policy: policy),
                block: static (_, _) => Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:unflattened-block")),
                word: static (context, value) => ExecuteWord(context.Fold, context.Locus, value, context.Policy),
                cannedCycle: static (context, value) => ExecuteCycle(context.Fold, context.Locus, value, context.Policy),
                coordinateFrame: static (context, value) => Fin.Succ(new SimulationFold(
                    context.Fold.State with { Offsets = context.Fold.State.Offsets.AddOrUpdate(
                        value.Assignment.Setup, Transform.PlaneToPlane(Plane.WorldXY, value.Frame)) },
                    context.Fold.Ledger.Add(new SimulationSlice.State(context.Locus, value)))),
                macro: static (_, _) => Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:unflattened-macro")),
                subprogram: static (_, _) => Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:unflattened-subprogram")),
                additiveLayer: static (context, value) => ExecuteAdditive(context.Fold, context.Locus, value, context.Policy),
                nc1: static (_, _) => Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:nc1-clock-owner-required")),
                directive: static (context, value) => ExecuteDirective(context.Fold, context.Locus, value.Value, context.Policy));

    // The specialized arm REVALIDATES NOTHING: `SpecializedToolpathEnvelope.Admit` proved kind correspondence,
    // non-empty rows, and a finite non-negative duration once at the atoms floor. An envelope attached to no
    // realized move — the atom spells that as a negative `AfterMove` — is a direct specialized program and charges
    // its own duration; one annotating realized motion charges nothing, because those moves already paid.
    private static Fin<SimulationFold> ExecuteDirective(
        SimulationFold fold,
        ProgramLocus locus,
        MotionDirective directive,
        SimulatePolicy policy) => directive.Switch(
        state: (Fold: fold, Locus: locus, Policy: policy),
        spindle: static (context, row) => ChargeSpindle(
            context.Fold, context.Locus, GCommand.Spindle, row.ResolvedRpm, context.Policy),
        dwell: static (context, row) => row.Basis == DwellBasis.Seconds
            ? ApplyDelay(context.Fold, context.Locus, GCommand.Dwell, DelayKind.Dwell,
                Duration.FromSeconds(row.Amount), context.Policy)
            : context.Fold.State.SpindleRpm > 0.0
                ? ApplyDelay(context.Fold, context.Locus, GCommand.Dwell, DelayKind.Dwell,
                    Duration.FromSeconds(row.Amount * SecondsPerMinute / context.Fold.State.SpindleRpm), context.Policy)
                : Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(
                    FabConcern.Verify, "simulate:dwell-without-spindle")),
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
            Duration elapsed = row.AfterMove < 0 ? Duration.FromSeconds(row.Payload.DurationSeconds) : Duration.Zero;
            return Fin.Succ(context.Fold with {
                Ledger = context.Fold.Ledger.Add(new SimulationSlice.Specialized(
                    context.Locus,
                    row.Payload,
                    elapsed,
                    context.Policy.Machine.Map(machine => EnergyKwh(
                        machine.PowerKw * context.Policy.ActivePowerFactor, elapsed)).IfNone(0.0))),
            });
        });

    // A cycle with no expanded moves is the single-block form the post emits for dwell-shaped cycles; its own words
    // carry the whole effect, so the cycle body is the command word itself rather than an expansion.
    private static Fin<SimulationFold> ExecuteCycle(SimulationFold fold, ProgramLocus locus, GNode.CannedCycle cycle, SimulatePolicy policy) =>
        Range(0, cycle.Repeats).FoldM<Fin, SimulationFold>(fold, (state, _) => cycle.ExpandedMoves.IsEmpty
            ? ExecuteWord(state, locus, new GNode.Word(cycle.Command, cycle.SingleBlockWords, cycle.Mode), policy)
            : cycle.ExpandedMoves.FoldM<Fin, SimulationFold>(state, (nested, move) =>
                GNode.Move(move, nested.State.ProgramAt) is GNode.Word word
                    ? ExecuteWord(nested, locus, word with { Mode = cycle.Mode }, policy)
                    : Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:cycle-move"))).As()).As();

    private static Fin<SimulationFold> ExecuteWord(SimulationFold fold, ProgramLocus locus, GNode.Word word, SimulatePolicy policy) =>
        from instruction in AdmitInstruction(new WordContext(locus, fold.State, word, policy))
        from advanced in Apply(fold, locus, word, instruction, policy)
        select advanced;

    private static Fin<Instruction> AdmitInstruction(WordContext context) =>
        (context.Word.Command.Group == ModalGroup.Motion
            ? CommandEffect.Motion
            : Effects.Find(context.Word.Command).IfNone(CommandEffect.Modal)).Admit(context);

    // --- [MOTION_ADMISSION]
    // Every refusal below lands BEFORE `Apply` touches a modal register, so a rejected block leaves the controller
    // state and the ledger exactly as it found them.
    internal static Fin<Instruction.Motion> AdmitMotion(WordContext context) {
        (ControllerState state, GNode.Word word, SimulatePolicy policy) = (context.State, context.Word, context.Policy);
        Map<ModalGroup, GCommand> active = Stamp(state.Active, word);
        bool relative = active.Find(ModalGroup.Distance).Exists(static command => command == GCommand.Relative);
        Point3d programTo = Target(state.ProgramAt, word, relative);
        bool arc = word.Command == GCommand.ArcCw || word.Command == GCommand.ArcCcw;
        FeedMode mode = word.Command == GCommand.Rapid ? FeedMode.UnitsPerMinute
            : word.Mode.IfNone(active.Find(ModalGroup.Feed).Exists(static command => command == GCommand.FeedInverseTime)
                ? FeedMode.InverseTime : FeedMode.UnitsPerMinute);
        return from offset in state.Offsets.Find(state.Wcs).ToFin(
                       new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:wcs-reference"))
               from length in state.LengthOffset.Traverse(value => policy.ToolLengthsMm.Find(value)
                   .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:length-reference"))).As()
               let toolLength = length.IfNone(0.0)
               let machineTo = (offset * (state.Frame.Combined * programTo)) + (toolLength * (offset * Vector3d.ZAxis))
               from band in ClockBand.Of(word.Command.Role)
                   .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:motion-band"))
               let ceiling = Ceiling(policy.Dynamics, band, arc)
               from spanLength in SpanLength(state.ProgramAt, programTo, word, active, arc)
               from rate in Rate(word, mode, state, spanLength.LengthMm, ceiling, policy)
               from geometry in spanLength.Admitted(programTo, rate.FeedMmMinute)
               from rotary in RotarySeconds(context.Locus, state, word, relative, policy)
               from _ in Gate(context.Locus, machineTo, geometry, offset * state.Frame.Combined, toolLength, word, policy)
               select new Instruction.Motion(word.Command, geometry, programTo, machineTo,
                   rotary.A, rotary.B, rotary.C, rate.FeedMmMinute, rate.Linear, mode,
                   word.P('X').IsSome, rotary.Seconds, band);
    }

    // The ceiling a band commands: a rapid rides the rapid law regardless of span shape, and a fed block rides the
    // law its own span shape declares.
    private static double Ceiling(MotionDynamics dynamics, ClockBand band, bool arc) =>
        band == ClockBand.Rapid ? dynamics.RapidFeed : arc ? dynamics.ArcFeed : dynamics.LinearFeed;

    // The two feed modes settle into ONE pair. A units-per-minute block clamps to the machine ceiling inside the
    // profile, which is what the machine physically does. An inverse-time block commands a DURATION, so its feed is
    // derived — and a derived feed above the ceiling is a block the machine cannot execute in the time commanded,
    // which refuses here rather than silently clamping and reporting a cycle time no machine achieves.
    private static Fin<(double FeedMmMinute, Duration Linear)> Rate(
        GNode.Word word,
        FeedMode mode,
        ControllerState state,
        double lengthMm,
        double ceilingMmMinute,
        SimulatePolicy policy) => mode == FeedMode.InverseTime
        ? word.P('F').Filter(Witness.Positive)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:inverse-time-word"))
            .Map(static reciprocal => SecondsPerMinute / reciprocal)
            .Bind(seconds => double.IsFinite(seconds)
                ? Fin.Succ((Derived: lengthMm * SecondsPerMinute / seconds, Block: Duration.FromSeconds(seconds)))
                : Fin.Fail<(double Derived, Duration Block)>(
                    new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:inverse-time-word")))
            .Bind(row => row.Derived <= ceilingMmMinute
                ? Fin.Succ((row.Derived, row.Block))
                : Fin.Fail<(double, Duration)>(
                    new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:inverse-time-infeasible")))
        : (word.Command == GCommand.Rapid ? Some(policy.Dynamics.RapidFeed) : word.P('F').OrElse(Some(state.FeedMmMinute)))
            .Filter(Witness.Positive)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:motion-feed"))
            .Map(feed => (feed, Duration.FromSeconds(ProfileSeconds(
                lengthMm, feed / SecondsPerMinute, ceilingMmMinute / SecondsPerMinute,
                policy.Dynamics.Acceleration, policy.Dynamics.Jerk))));

    // The span settles in two stages because the rate needs the LENGTH while the arc atom needs the FEED: the
    // decode resolves centre, sense, signed sweep, radius, and rise, and `Admitted` seats the S0 `Move.Circular`
    // once the feed is known. The decoded sweep and the admitted sweep are the same number, so no convention forks.
    private readonly record struct SpanDecode(
        double LengthMm,
        Option<(Plane Plane, Point3d Center, RotationSense Sense, double SweepRadians, double RadiusMm, double RiseMm)> Arc) {

        public Fin<MotionGeometry> Admitted(Point3d target, double feedMmMinute) => Arc.Match(
            None: () => Fin.Succ<MotionGeometry>(new MotionGeometry.Linear(LengthMm)),
            // The decode seats the working plane ON the arc start, so the plane origin IS the start point and no
            // second column restates it.
            Some: row => from move in Move.Circular.Of(target, feedMmMinute, new ArcCenter(row.Center, row.Sense), row.SweepRadians)
                         from circular in move.CircularGeometry.ToFin(
                             new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-atom").ToError())
                         from evidence in ArcEvidence.Admit(row.Plane, row.Plane.Origin, circular, row.RadiusMm, row.RiseMm)
                         select (MotionGeometry)new MotionGeometry.Arc(evidence));
    }

    private static Fin<SpanDecode> SpanLength(
        Point3d from, Point3d to, GNode.Word word, Map<ModalGroup, GCommand> active, bool arc) =>
        arc
            ? Arc(from, to, word, active).Map(static row => new SpanDecode(
                Math.Sqrt(Math.Pow(row.RadiusMm * Math.Abs(row.SweepRadians), 2.0) + (row.RiseMm * row.RiseMm)),
                Some(row)))
            : Fin.Succ(new SpanDecode(from.DistanceTo(to), None));

    private static Fin<(Plane Plane, Point3d Center, RotationSense Sense, double SweepRadians, double RadiusMm, double RiseMm)> Arc(
        Point3d from, Point3d to, GNode.Word word, Map<ModalGroup, GCommand> active) {
        GCommand planeCommand = active.Find(ModalGroup.Plane).IfNone(GCommand.PlaneXy);
        Plane plane = planeCommand == GCommand.PlaneZx ? new Plane(from, Vector3d.ZAxis, Vector3d.XAxis)
            : planeCommand == GCommand.PlaneYz ? new Plane(from, Vector3d.YAxis, Vector3d.ZAxis)
            : new Plane(from, Vector3d.XAxis, Vector3d.YAxis);
        RotationSense sense = word.Command == GCommand.ArcCw ? RotationSense.Clockwise : RotationSense.Counterclockwise;
        double i = word.P('I').IfNone(0.0), j = word.P('J').IfNone(0.0), k = word.P('K').IfNone(0.0);
        bool absoluteCenter = active.Find(ModalGroup.ArcDistance).Exists(static command => command == GCommand.ArcAbsolute);
        Vector3d offset = planeCommand == GCommand.PlaneZx ? new Vector3d(i, 0.0, k)
            : planeCommand == GCommand.PlaneYz ? new Vector3d(0.0, j, k) : new Vector3d(i, j, 0.0);
        // G90.1 spells I/J/K as absolute centre coordinates on the active plane axes; the out-of-plane ordinate stays
        // on the start point, so the same two words select between an origin-relative and a start-relative centre.
        Point3d absolute = new(
            planeCommand == GCommand.PlaneYz ? from.X : i,
            planeCommand == GCommand.PlaneZx ? from.Y : j,
            planeCommand == GCommand.PlaneXy ? from.Z : k);
        Option<double> radiusWord = word.P('R');
        bool carriesOffset = word.P('I').IsSome || word.P('J').IsSome || word.P('K').IsSome;
        Fin<Point3d> definition = radiusWord.Match(
            None: () => carriesOffset
                ? Fin.Succ(absoluteCenter ? absolute : from + offset)
                : Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-center").ToError()),
            Some: radius => carriesOffset
                ? Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-definition-conflict").ToError())
                : RadiusDefinition(radius, from, to, plane, sense));
        return definition.Bind(center => {
            double radiusMm = center.DistanceTo(plane.ClosestPoint(from));
            return Witness.Positive(radiusMm)
                ? Fin.Succ((plane, center, sense,
                    ArcEvidence.Sweep(plane, center, from, to, sense, radiusMm),
                    radiusMm,
                    plane.DistanceTo(to) - plane.DistanceTo(from)))
                : Fin.Fail<(Plane, Point3d, RotationSense, double, double, double)>(
                    new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-radius").ToError());
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
        if (!Witness.Positive(radius) || length <= 0.0 || length > 2.0 * radius)
            return Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-radius").ToError());
        Point3d midpoint = plane.ClosestPoint(from) + (plane.DistanceTo(from) * plane.ZAxis) + (0.5 * chord);
        Vector3d normal = Vector3d.CrossProduct(plane.ZAxis, chord) / length;
        double height = Math.Sqrt(Math.Max(0.0, (radius * radius) - (0.25 * length * length)));
        // A negative R word spells the MAJOR arc, so the branch is selected by the sweep magnitude the atom will
        // admit rather than by a second geometric convention.
        bool major = signedRadius < 0.0;
        return Seq(midpoint + (height * normal), midpoint - (height * normal))
            .Find(center => major
                ? Math.Abs(ArcEvidence.Sweep(plane, center, from, to, sense, radius)) >= Math.PI
                : Math.Abs(ArcEvidence.Sweep(plane, center, from, to, sense, radius)) <= Math.PI)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-radius-branch").ToError());
    }

    internal static Fin<double> NonNegativeSeconds(Option<double> seconds, string locus) =>
        seconds.Filter(static value => double.IsFinite(value) && value >= 0.0)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, locus));

    // One integral read for every address that names a slot, a register, or a tool: the value is an ordinal by
    // contract, so a fractional or out-of-range word refuses rather than truncating into a neighbouring slot.
    internal static Fin<int> Ordinal(Option<double> raw, string locus, Func<int, bool> admitted) =>
        raw.Filter(value => double.IsFinite(value) && value >= int.MinValue && value <= int.MaxValue
                && value == Math.Truncate(value) && admitted((int)value))
            .Map(static value => (int)value)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, locus));

    internal static Fin<Option<int>> OptionalOrdinal(Option<double> raw, string locus, Func<int, bool> admitted) =>
        raw.Match(
            None: () => Fin.Succ<Option<int>>(None),
            Some: _ => Ordinal(raw, locus, admitted).Map(Some));

    private static Fin<(double Seconds, double A, double B, double C)> RotarySeconds(
        ProgramLocus locus, ControllerState state, GNode.Word word, bool relative, SimulatePolicy policy) =>
        from a in RotaryTarget(state.A, word.P('A'), relative, MachineAxis.A)
        from b in RotaryTarget(state.B, word.P('B'), relative, MachineAxis.B)
        from c in RotaryTarget(state.C, word.P('C'), relative, MachineAxis.C)
        from seconds in Seq(
                (Axis: MachineAxis.A, From: state.A, To: a),
                (Axis: MachineAxis.B, From: state.B, To: b),
                (Axis: MachineAxis.C, From: state.C, To: c))
            .Filter(static row => row.From != row.To)
            .TraverseM(row => policy.Axes.Find(axis => axis.Axis == row.Axis)
                .ToFin(new FabricationFault.SimulatedOvertravel(
                    locus.Block, row.Axis, Math.Abs(row.To - row.From)))
                .Bind(axis => axis.Periodicity.Cyclic || axis.Contains(row.To)
                    ? Fin.Succ(RotaryProfile(axis, policy.Dynamics, Math.Abs(axis.Periodicity.Cyclic
                        ? Math.IEEERemainder(row.To - row.From, Math.Tau) : row.To - row.From)))
                    : Fin.Fail<double>(new FabricationFault.SimulatedOvertravel(
                        locus.Block, row.Axis, Math.Max(axis.Min - row.To, row.To - axis.Max))))).As()
        select (seconds.Fold(0.0, Math.Max), a, b, c);

    // Rotary travel rides the same jerk-limited profile as linear travel; the per-axis limits bound the machine-wide
    // rotary law, so the tighter of the two governs every coordinated block.
    private static double RotaryProfile(AxisMotion axis, MotionDynamics dynamics, double radians) => ProfileSeconds(
        radians,
        Math.Min(axis.MaximumVelocity, UnitsNet.Angle.FromDegrees(dynamics.RotaryFeed).Radians / SecondsPerMinute),
        axis.MaximumVelocity,
        Math.Min(axis.MaximumAcceleration, dynamics.RotaryAcceleration),
        Math.Min(axis.MaximumJerk, dynamics.RotaryJerk));

    private static Fin<double> RotaryTarget(double held, Option<double> value, bool relative, MachineAxis axis) =>
        value.Match(
            None: () => Fin.Succ(held),
            Some: raw => double.IsFinite(raw)
                ? Fin.Succ(UnitsNet.Angle.FromDegrees(raw).Radians + (relative ? held : 0.0))
                : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(
                    FabConcern.Verify, $"simulate:rotary-target:{axis.Key}")));

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
                ? Fin.Fail<Unit>(new FabricationFault.SimulatedOvertravel(
                    locus.Block, row.Axis, Math.Max(row.Min - row.At, row.At - row.Max)))
                : Fin.Fail<Unit>(new FabricationFault.EnvelopeExceeded(
                    row.Axis, row.At, row.At < row.Min ? row.Min : row.Max)));
    }

    // --- [MODAL_APPLICATION]
    private static Fin<SimulationFold> Apply(SimulationFold fold, ProgramLocus locus, GNode.Word word, Instruction instruction, SimulatePolicy policy) =>
        instruction.Switch(
            state: (Fold: fold, Locus: locus, Word: word, Policy: policy),
            motion: static (context, value) => ApplyMotion(context.Fold, context.Locus, context.Word, value, context.Policy),
            delay: static (context, value) => ApplyDelay(context.Fold, context.Locus, value.Command, value.Kind, value.Duration, context.Policy),
            tool: static (context, value) => ApplyTool(context.Fold, context.Locus, value, context.Policy),
            spindle: static (context, value) => ApplyModal(context.Fold, context.Locus, context.Word, context.Fold.State.Frame, context.Policy)
                .Bind(next => ChargeSpindle(next, context.Locus, value.Command, value.TargetRpm, context.Policy)),
            css: static (context, value) => ApplyCss(context.Fold, context.Locus, context.Word, value, context.Policy),
            thermal: static (context, value) => ApplyThermal(context.Fold, context.Locus, value, context.Policy),
            frame: static (context, value) => ApplyModal(context.Fold, context.Locus, context.Word,
                    context.Fold.State.Frame.Apply(value.Effect, context.Word), context.Policy)
                .Bind(next => Spun(next, context.Locus, context.Word, context.Policy)),
            modal: static (context, _) => ApplyModal(
                    context.Fold, context.Locus, context.Word, context.Fold.State.Frame, context.Policy)
                .Bind(next => Spun(next, context.Locus, context.Word, context.Policy)));

    private static Fin<SimulationFold> ApplyMotion(SimulationFold fold, ProgramLocus locus, GNode.Word word, Instruction.Motion motion, SimulatePolicy policy) {
        // Constant surface speed resolves against the MODAL diameter, so a Z-only block reads the diameter it
        // already held, commands no speed change, and charges no ramp.
        double diameter = motion.CommandsDiameter ? Math.Abs(motion.ProgramTo.X) : fold.State.CssDiameterMm;
        double spindleRpm = fold.State.CssMetersMinute
            .Map(surface => CssRpm(surface, fold.State.CssMaximumRpm, diameter))
            .IfNone(word.P('S').Filter(static value => double.IsFinite(value) && value >= 0.0).IfNone(fold.State.SpindleRpm));
        // The three costs run CONCURRENTLY on a coordinated block, so the longest governs.
        Duration elapsed = Seq(
                motion.Linear,
                Duration.FromSeconds(motion.RotarySeconds),
                RampSeconds(fold.State.SpindleRpm, spindleRpm, policy))
            .Fold(Duration.Zero, static (longest, row) => row > longest ? row : longest);
        double power = policy.Machine.Map(machine =>
            motion.Band == ClockBand.Rapid ? machine.Instance.IdlePowerKw : machine.PowerKw).IfNone(0.0);
        SimulationSlice row = new SimulationSlice.Motion(locus, motion.Command, motion.Band, elapsed, motion.Geometry.LengthMm,
            motion.FeedMmMinute, EnergyKwh(power * policy.ActivePowerFactor, elapsed));
        ControllerState state = fold.State with {
            Active = Stamp(fold.State.Active, word),
            ProgramAt = motion.ProgramTo,
            MachineAt = motion.MachineTo,
            A = motion.A,
            B = motion.B,
            C = motion.C,
            // An inverse-time word states a block DURATION, not a feed, so it leaves the modal feed register alone.
            FeedMmMinute = motion.Mode == FeedMode.UnitsPerMinute ? motion.FeedMmMinute : fold.State.FeedMmMinute,
            SpindleRpm = spindleRpm,
            CssDiameterMm = diameter,
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

    // A tool change costs what the magazine MEASURED. A change between two seated tools reads the ordered-pair row;
    // a load into an empty spindle has no origin slot and therefore no index traverse, so it charges the
    // destination row's arm swing alone. A change the census does not carry is the magazine's gap and refuses.
    private static Fin<SimulationFold> ApplyTool(SimulationFold fold, ProgramLocus locus, Instruction.Tool tool, SimulatePolicy policy) =>
        fold.State.Tool
            .Bind(origin => policy.ToolChanges.Find((origin, tool.Tool)).Map(static row => row.Elapsed))
            .OrElse(() => fold.State.Tool.IsNone
                ? toSeq(policy.ToolChanges).Find(row => row.Key.To == tool.Tool).Map(static row => row.Value.ArmSwing)
                : None)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:tool-change-evidence"))
            .Bind(elapsed => ApplyDelay(
                fold with { State = fold.State with { Tool = Some(tool.Tool), LengthOffset = None } },
                locus, tool.Command, DelayKind.ToolChange, elapsed, policy));

    private static Fin<SimulationFold> ApplyCss(
        SimulationFold fold,
        ProgramLocus locus,
        GNode.Word word,
        Instruction.Css css,
        SimulatePolicy policy) =>
        ApplyModal(fold with { State = fold.State with {
                CssMetersMinute = Some(css.SurfaceMetersMinute),
                CssMaximumRpm = css.MaximumRpm,
            } }, locus, word, fold.State.Frame, policy)
            .Bind(next => ChargeSpindle(next, locus, css.Command,
                CssRpm(css.SurfaceMetersMinute, css.MaximumRpm, next.State.CssDiameterMm), policy));

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
        SimulatePolicy policy) =>
        from reads in ModalSlot.For(word.Command)
            .TraverseM(slot => OptionalOrdinal(word.P(slot.Address), slot.Locus, slot.Admits)
                .Map(value => (Slot: slot, Value: value))).As()
        let admitted = toMap(reads.Choose(static row => row.Value.Map(value => (row.Slot, value))))
        let offsets = admitted.Find(ModalSlot.WcsRegister).IsSome
            ? admitted.Find(ModalSlot.WcsSlot)
                .Map(value => fold.State.Offsets.AddOrUpdate(value, Transform.Translation(
                    word.P('X').IfNone(0.0), word.P('Y').IfNone(0.0), word.P('Z').IfNone(0.0))))
                .IfNone(fold.State.Offsets)
            : fold.State.Offsets
        from _wcs in admitted.Find(ModalSlot.Wcs).Traverse(value => offsets.Find(value)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:wcs-reference"))).As()
        from _length in admitted.Find(ModalSlot.LengthOffset).Traverse(value => policy.ToolLengthsMm.Find(value)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:length-reference"))).As()
        let lengthOffset = word.Command == GCommand.LengthOffset ? admitted.Find(ModalSlot.LengthOffset)
            : word.Command == GCommand.LengthCancel ? None : fold.State.LengthOffset
        let state = fold.State with {
            Active = Stamp(fold.State.Active, word),
            Offsets = offsets,
            Frame = frame,
            Wcs = admitted.Find(ModalSlot.Wcs).IfNone(fold.State.Wcs),
            LengthOffset = lengthOffset,
            CssMetersMinute = word.Command == GCommand.CssCancel ? None : fold.State.CssMetersMinute,
            CssMaximumRpm = word.Command == GCommand.CssCancel ? 0.0 : fold.State.CssMaximumRpm,
            Stopped = Terminal(word.Command),
        }
        select new SimulationFold(state, fold.Ledger.Add(new SimulationSlice.State(locus, word)));

    // An `S` word riding a modal block is a real speed command and charges the same ramp an explicit spindle
    // command does; an unreadable one refuses rather than being silently ignored.
    private static Fin<SimulationFold> Spun(SimulationFold fold, ProgramLocus locus, GNode.Word word, SimulatePolicy policy) =>
        word.P('S').Match(
            None: () => Fin.Succ(fold),
            Some: raw => double.IsFinite(raw) && raw >= 0.0
                ? raw == fold.State.SpindleRpm
                    ? Fin.Succ(fold)
                    : ChargeSpindle(fold, locus, word.Command, raw, policy)
                : Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(
                    FabConcern.Verify, "simulate:spindle-target")));

    // The ONE spindle-ramp charge. Every arrival path routes here, so no path changes the speed for free.
    private static Fin<SimulationFold> ChargeSpindle(
        SimulationFold fold, ProgramLocus locus, GCommand command, double targetRpm, SimulatePolicy policy) =>
        ApplyDelay(
            fold with { State = fold.State with { SpindleRpm = targetRpm } },
            locus, command, DelayKind.SpindleRamp, RampSeconds(fold.State.SpindleRpm, targetRpm, policy), policy);

    private static Duration RampSeconds(double fromRpm, double toRpm, SimulatePolicy policy) =>
        Duration.FromSeconds(Math.Abs(toRpm - fromRpm) / policy.Timing.SpindleRevolutionsPerSecondSquared / SecondsPerMinute);

    private static Fin<SimulationFold> ExecuteAdditive(SimulationFold fold, ProgramLocus locus, GNode.AdditiveLayer layer, SimulatePolicy policy) =>
        // The extrusion feed is a DIVISOR, so its positivity is the gate rather than decoration.
        Witness.Positive(layer.Extrusion.Feed)
        && double.IsFinite(layer.Extrusion.Amount) && layer.Extrusion.Amount >= 0.0
            ? from hotend in ApplyThermal(fold, locus,
                  new Instruction.Thermal(GCommand.HotendWait, ThermalAction.HotendWait, layer.Temperatures.Hotend), policy)
              from bed in ApplyThermal(hotend, locus,
                  new Instruction.Thermal(GCommand.BedWait, ThermalAction.BedWait, layer.Temperatures.Bed), policy)
              let elapsed = Duration.FromSeconds(layer.Extrusion.Amount / layer.Extrusion.Feed * SecondsPerMinute)
              let power = policy.Machine.Map(static machine => machine.PowerKw).IfNone(0.0)
              select new SimulationFold(bed.State, bed.Ledger.Add(new SimulationSlice.Deposition(
                  locus, elapsed, layer.Extrusion.Amount, layer.Extrusion.Feed,
                  EnergyKwh(power * policy.ActivePowerFactor, elapsed))))
            : Fin.Fail<SimulationFold>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "simulate:additive-layer"));

    // --- [CLOCK_KERNEL]
    // The one per-minute basis every feed, ramp, and deposition rate converts through.
    private const double SecondsPerMinute = 60.0;

    // The cutting-speed relation is `Process/physics#BUDGET_FOLD` `SurfaceSpeed.Rpm` and it lives there alone, so
    // this lane composes it rather than restating the metre basis a second time. At or through the turning centre
    // the demand is unbounded and the controller holds its declared ceiling — the clamp IS the physical behaviour,
    // and taking it before the composition keeps the singularity out of the shared law.
    private static double CssRpm(double surfaceMetersMinute, double maximumRpm, double diameterMm) =>
        diameterMm <= 0.0
            ? maximumRpm
            : Math.Min(maximumRpm, SurfaceSpeed.Rpm(surfaceMetersMinute, diameterMm));

    private static double EnergyKwh(double powerKw, Duration elapsed) =>
        (UnitsNet.Power.FromKilowatts(powerKw) * UnitsNet.Duration.FromSeconds(elapsed.TotalSeconds)).KilowattHours;

    // `ProgramEnd` is the vocabulary's only terminal row; `Stop` and `OptionalStop` share its modal group but
    // resume under the operator, so they charge their delay and leave the run live.
    private static bool Terminal(GCommand command) => command == GCommand.ProgramEnd;

    private static Map<ModalGroup, GCommand> Stamp(Map<ModalGroup, GCommand> active, GNode.Word word) => Stamp(active, word.Command);

    private static Map<ModalGroup, GCommand> Stamp(Map<ModalGroup, GCommand> active, GCommand command) =>
        command.Group == ModalGroup.NonModal ? active : active.AddOrUpdate(command.Group, command);

    private static Point3d Target(Point3d from, GNode.Word word, bool relative) => new(
        Axis(from.X, word.P('X'), relative), Axis(from.Y, word.P('Y'), relative), Axis(from.Z, word.P('Z'), relative));

    private static double Axis(double held, Option<double> value, bool relative) =>
        value.Map(raw => relative ? held + raw : raw).IfNone(held);

    // One jerk-limited seven-segment profile serves linear, arc, and rotary travel; the caller supplies the axis or
    // machine law, and `SimulatePolicy` proved every acceleration and jerk strictly positive, so no divisor here is
    // guarded and no NaN reaches the clock.
    private static double ProfileSeconds(double distance, double target, double ceiling, double acceleration, double jerk) {
        if (distance <= 0.0) return 0.0;
        double velocity = Math.Min(target, ceiling);
        double threshold = acceleration * acceleration / jerk;
        double ramp = velocity * (velocity <= threshold
            ? 2.0 * Math.Sqrt(velocity / jerk)
            : (velocity / acceleration) + (acceleration / jerk));
        if (distance >= ramp)
            return (2.0 * (velocity <= threshold
                ? 2.0 * Math.Sqrt(velocity / jerk)
                : (velocity / acceleration) + (acceleration / jerk))) + ((distance - ramp) / velocity);
        double jerkPeak = Math.Pow(distance * Math.Sqrt(jerk) * 0.5, 2.0 / 3.0);
        double peak = jerkPeak <= threshold
            ? jerkPeak
            : 0.5 * (-threshold + Math.Sqrt((threshold * threshold) + (4.0 * acceleration * distance)));
        return 2.0 * (peak <= threshold
            ? 2.0 * Math.Sqrt(peak / jerk)
            : (peak / acceleration) + (acceleration / jerk));
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
