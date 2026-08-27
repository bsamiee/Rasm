# [RASM_FABRICATION_SIMULATE]

`Simulate.Execute` admits one `SimulatePolicy`, evaluates the motion source it carries without replanning, and emits the authoritative `SimulationLedger` clock. `MotionSource` closes that source: a posted `CutProgram` folds through controller semantics, and a `RobotCell` folds through the sampled pose census `Kinematics/cell` resolves. `GCommand.Grammar` owns syntactic admission; simulation owns relational motion admission, modal execution, machine-limit evidence, coordinated timing, energy, and terminal state.

Every block settles at admission into ONE feed and ONE duration, so a commanded feed and a commanded block time never share a column and the peak-feed population holds millimetres per minute alone. `Move.Circular.SweepRadians` carries the signed arc sweep, admitted by the S0 atom against its own sense and its `(0, Tau]` band — this page derives it once at the G-code decode boundary where the program carries no sweep at all, hands it to `Move.Circular.Of`, and every reader here and at `Verify/removal` reads that one column.

Spindle speed changes cost the ramp the declared operating envelope implies wherever they arrive, tool changes cost the `ToolChangeEvidence.Elapsed` `Tooling/magazine` derived against a REAL ordinal pair, and constant surface speed resolves against a modal diameter rather than whichever block happens to execute.

`CommandEffect` is the one admission table keyed by `GCommand`, so a command with a distinct physical effect is one row and every other command inherits its `ModalGroup` behaviour. `SimulationSlice` is the sole ledger family and every `SimulationLedger` projection folds that ledger; `ProgramLocus` and `ProgramPathStep` arrive from `Posting/program`, so a ledger row and a program event address one execution locus in one spelling.

## [01]-[INDEX]

- [02]-[EXECUTION_SOURCE]: `MotionSource`, `SimulatePolicy`, `ControllerTiming`, `ClockBand`, `DelayKind`, `ThermalAction`, `FrameEffect`, `FrameState`, `ControllerState`.
- [03]-[BLOCK_ADMISSION]: `ArcEvidence`, `MotionGeometry`, `Instruction`, `ModalSlot`, `CommandEffect`, and the rate, geometry, and operating envelope gates that run before any modal register moves.
- [04]-[LEDGER]: `SimulationSlice`, `MotionTally`, `DelayTally`, `SimulationLedger`.
- [05]-[MODAL_CLOCK]: `Simulate.Execute`, the posted and cell folds, and the one spindle-ramp, tool-change, and jerk-limited profile charges.

## [02]-[EXECUTION_SOURCE]

- Owner: `MotionSource` closes the posted-program and robot-cell lanes as the policy's own discriminant. `SimulatePolicy` composes that source with `MotionDynamics`, `AxisMotion`, assessed `MachineMatch` operating envelope and power truth, the magazine's tool-change census, power-on modal defaults, work offsets, tool-length compensation, controller timing, nesting depth, and energy policy. `ControllerTiming` owns the fixed delays the CONTROLLER times and the slew rates the ramps read. `ControllerState` carries one canonical modal map with physical registers, the constant-surface-speed diameter axis, and the active local frame.
- Cases: `ClockBand` maps a `MotionRole` onto the band its motion tallies under. `DelayKind.ControllerTimed` is the declared `CapabilitySet<DelayKind>` naming which rows the controller itself times — a dwell reads its own program word, a tool change reads the magazine's evidence, and the two ramps derive from slew rates, so only the halts and the auxiliary stabilization demand a fixed duration. `ThermalAction` rosters one row per heated register, each carrying the pair of vocabulary commands that address it.
- Law: the policy admits every quantity a clock later divides by. `MotionDynamics` accelerations and jerks, `AxisMotion` per-axis accelerations and jerks, and the `ControllerTiming` slew rates are all proved finite and strictly positive HERE — the slew rates by their `PositiveMagnitude` carrier alone — so the jerk-limited profile divides with no in-body guard and no NaN can reach the authoritative clock.
- Law: typed carriers own every scalar band the policy states. `Dimension` refuses a non-positive nesting depth, `UnitInterval` refuses a power factor outside its band, `PositiveMagnitude` refuses a non-positive slew rate, and `Length` carries the compensation and soft-limit millimetres, so four invariant rows disappear into construction. `ControllerState` holds canonical millimetres, revolutions per minute, radians, and degrees Celsius read off those carriers ONCE, so the interior clock re-derives no unit.
- Law: the invariant ROSTER is one authority read twice. `Slots` states each independent gate beside the locus it refuses under, `Admit` accumulates every row applicatively so a caller reads all violated invariants at once, and the generated backstop seats the same accumulation folded into the one fault slot its contract publishes — neither close can drift from the other, because neither owns a row.
- Entry: `SimulatePolicy.Admit` is the ONE admission — a lane never re-tests a missing or empty source mid-execution.
- Auto: `ControllerState.PowerOn` seats the policy's declared defaults, so a program that states no mode still executes against one canonical modal map. Its thermal and rotary registers materialize from the `ThermalAction` and addressable-rotary rosters, so both maps are TOTAL by construction and every read is an indexer.
- Packages: `Posting/program` (`CutProgram`, `GNode`, `GCommand`, `GParam`, `ModalGroup`, `MotionRole`, `FeedMode`); `Process/family` (`MachineAxis`); `Kinematics/machine` (`MotionDynamics`, `AxisMotion`, `AxisPeriodicity`); `Kinematics/cell` (`RobotCell`, `CellPolicy`, `CellClock`, `CellPosedStation`, `CellAnimation`); `Kinematics/fleet` (`MachineMatch`, `MachineInstance`); `Tooling/magazine` (`ToolChangeEvidence`, `MagazineLayout.Park`); `Process/physics` (`SurfaceSpeed`); `Process/faults` (`Witness`, `Admission`, `AdmissionSlots`); `Rasm.Domain` (`CapabilitySet`, `ICapability`); `Rasm.Numerics` (`Dimension`, `PositiveMagnitude`, `UnitInterval`); UnitsNet (`Length`, `Angle`, `RotationalSpeed`, `Speed`, `Power`, `Energy`); NodaTime; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: a motion modality is one `MotionSource` case and one `Execute` arm; a controller latency is one `DelayKind` row and its membership in the timed set; a heated register is one `ThermalAction` row carrying its ramp-rate entry, with no state, timing, or apply edit; a coordinate-transform command is one `FrameEffect` row; a machine axis is one `AxisMotion` row the policy declares.
- Boundary: simulation evaluates planned intent and never rewrites feeds, geometry, or sequence. `Posting/program` owns parse, expansion, and look-ahead. `Kinematics/machine` owns dynamics and axis limits. `Kinematics/cell` owns every `Robots` member, so the cell lane consumes a provider-free station census and this page names no provider type. `Tooling/magazine` owns tool-change timing and mints every `ToolChangeEvidence` through its ONE derivation, so this page reads the census whole and re-tests no column of it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Projection;
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

// --- [TYPES] ---------------------------------------------------------------------------
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
public sealed partial class DelayKind : ICapability<DelayKind> {
    public static readonly DelayKind Dwell = new("dwell");
    public static readonly DelayKind Pierce = new("pierce");
    public static readonly DelayKind ToolChange = new("tool-change");
    public static readonly DelayKind RequiredStop = new("required-stop");
    public static readonly DelayKind OptionalStop = new("optional-stop");
    public static readonly DelayKind SpindleRamp = new("spindle-ramp");
    public static readonly DelayKind ThermalRamp = new("thermal-ramp");
    public static readonly DelayKind AuxiliaryStabilization = new("auxiliary-stabilization");

    public static CapabilitySet<DelayKind> ControllerTimed => Timed.Value;

    private static readonly Lazy<CapabilitySet<DelayKind>> Timed = new(
        static () => CapabilitySet<DelayKind>.Of(RequiredStop, OptionalStop, AuxiliaryStabilization),
        LazyThreadSafetyMode.ExecutionAndPublication);
}

[SmartEnum<string>]
public sealed partial class ThermalAction : ICapability<ThermalAction> {
    public static readonly ThermalAction Hotend = new("hotend", GCommand.HotendTemp, GCommand.HotendWait);
    public static readonly ThermalAction Bed = new("bed", GCommand.BedTemp, GCommand.BedWait);

    public GCommand Set { get; }
    public GCommand Wait { get; }

    public bool Blocks(GCommand command) => command == Wait;

    public Seq<GCommand> Commands => Seq(Set, Wait);

    public static Option<ThermalAction> Of(GCommand command) =>
        toSeq(Items).Find(row => row.Set == command || row.Wait == command);
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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FrameState(Transform Shift, Transform Rotation, Transform Scale) {
    public static readonly FrameState Identity = new(Transform.Identity, Transform.Identity, Transform.Identity);
    public Transform Combined => Shift * Rotation * Scale;

    public FrameState Apply(FrameEffect effect, GNode.Word word) => effect.Apply(this, word);
}

[ComplexValueObject]
public sealed partial class ControllerTiming {
    public Map<DelayKind, Duration> Spans { get; }

    public PositiveMagnitude SpindleRevolutionsPerSecondSquared { get; }

    public Map<ThermalAction, PositiveMagnitude> Ramp { get; }

    private static Seq<K<Validation<Error>, Unit>> Slots(
        Map<DelayKind, Duration> spans, Map<ThermalAction, PositiveMagnitude> ramp) => Seq(
        Simulate.Demand(CapabilitySet<DelayKind>.Of([.. spans.Keys]), DelayKind.ControllerTimed, "controller-timing:spans"),
        Simulate.Demand(CapabilitySet<ThermalAction>.Of([.. ramp.Keys]), CapabilitySet<ThermalAction>.All, "controller-timing:ramp"),
        AdmissionSlots.Gate(spans.ForAll(static row => row.Value >= Duration.Zero),
            FabConcern.Verify, "controller-timing:negative", FabricationFault.Inadmissible));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Map<DelayKind, Duration> spans,
        ref PositiveMagnitude spindleRevolutionsPerSecondSquared,
        ref Map<ThermalAction, PositiveMagnitude> ramp) =>
        validationError = Simulate.Folded(Slots(spans, ramp));

    public static Fin<ControllerTiming> Admit(
        Map<DelayKind, Duration> spans,
        PositiveMagnitude spindleRevolutionsPerSecondSquared,
        Map<ThermalAction, PositiveMagnitude> ramp) =>
        AdmissionSlots.Accumulate(Slots(spans, ramp))
            .ToFin()
            .Bind(_ => Validate(spans, spindleRevolutionsPerSecondSquared, ramp, out ControllerTiming timing)
                .Admitted(timing));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionSource {
    private MotionSource() { }

    public sealed record Posted(CutProgram Program) : MotionSource;
    public sealed record Cell(RobotCell Cell, Seq<Move> Moves, CellPolicy Policy, CellClock Clock) : MotionSource;
}

[ComplexValueObject]
public sealed partial class SimulatePolicy {
    public MotionSource Source { get; }
    public Option<MachineMatch> Machine { get; }
    public Seq<AxisMotion> Axes { get; }
    public MotionDynamics Dynamics { get; }

    public Seq<ToolChangeEvidence> ToolChanges { get; }

    public int Park { get; }

    public Map<ModalGroup, GCommand> PowerOn { get; }
    public Map<int, Transform> WorkOffsets { get; }
    public Map<int, Length> ToolLengths { get; }
    public ControllerTiming Timing { get; }
    public Dimension MaximumNesting { get; }
    public Length SoftLimitMargin { get; }
    public UnitInterval ActivePowerFactor { get; }

    public Map<(int From, int To), ToolChangeEvidence> Changes => ToolChanges.Fold(
        Map<(int, int), ToolChangeEvidence>(), static (index, row) => index.AddOrUpdate((row.FromSlot, row.ToSlot), row));

    public Option<AxisMotion> Axis(MachineAxis axis) => Axes.Find(row => row.Axis == axis);

    private static Seq<K<Validation<Error>, Unit>> Slots(
        MotionSource source,
        Option<MachineMatch> machine,
        Seq<AxisMotion> axes,
        MotionDynamics dynamics,
        Seq<ToolChangeEvidence> toolChanges,
        int park,
        Map<ModalGroup, GCommand> powerOn,
        Map<int, Transform> workOffsets,
        Map<int, Length> toolLengths,
        Length softLimitMargin) => Seq(
        AdmissionSlots.Gate(axes.Map(static axis => axis.Axis).Distinct().Count == axes.Count,
            FabConcern.Verify, "simulate:axis-identity", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(
            axes.Filter(static axis => axis.Axis.Rotary).Map(static axis => axis.Axis.Address).Distinct().Count
            == axes.Count(static axis => axis.Axis.Rotary), FabConcern.Verify, "simulate:rotary-address", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(
            ValidityClaim.All(
                ValidityClaim.Positive(dynamics.Acceleration), ValidityClaim.Positive(dynamics.Jerk),
                ValidityClaim.Positive(dynamics.RotaryAcceleration), ValidityClaim.Positive(dynamics.RotaryJerk),
                Seq(
                    dynamics.RapidFeed, dynamics.LinearFeed, dynamics.ArcFeed, dynamics.RotaryFeed)
                    .ForAll(static value => ValidityClaim.Positive(value).Holds),
                axes.ForAll(static axis => ValidityClaim.All(
                    ValidityClaim.Positive(axis.MaximumVelocity), ValidityClaim.Positive(axis.MaximumAcceleration),
                    ValidityClaim.Positive(axis.MaximumJerk)))),
                    FabConcern.Verify, "simulate:dynamics", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(
            toSeq(ModalGroup.Items)
                .Filter(static group => group != ModalGroup.NonModal && group != ModalGroup.Stop)
                .ForAll(group => powerOn.Find(group).Exists(command => command.Group == group)),
                    FabConcern.Verify, "simulate:power-on", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(
            workOffsets.Find(1).IsSome && workOffsets.ForAll(static row => row.Key > 0 && row.Value.IsValid),
                FabConcern.Verify, "simulate:work-offsets", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(
            toolLengths.ForAll(static row => row.Key > 0 && double.IsFinite(row.Value.Millimeters)
                && row.Value >= Length.Zero), FabConcern.Verify, "simulate:tool-lengths", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(ValidityClaim.Nonnegative(park), FabConcern.Verify, "simulate:park-ordinal", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(
            toolChanges.Map(static row => (row.FromSlot, row.ToSlot)).Distinct().Count == toolChanges.Count,
                FabConcern.Verify, "simulate:tool-change-census", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(
            machine.ForAll(static value => value.Checks.Feasible
                && double.IsFinite(value.PowerKw) && value.PowerKw >= 0.0),
                    FabConcern.Verify, "simulate:machine-assessment", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(
            double.IsFinite(softLimitMargin.Millimeters) && softLimitMargin >= Length.Zero,
                FabConcern.Verify, "simulate:soft-limit-margin", FabricationFault.Inadmissible),
        AdmissionSlots.Gate(
            source.Switch(
                posted: static row => !row.Program.Nodes.IsEmpty,
                cell: static row => !row.Moves.IsEmpty), FabConcern.Verify, "simulate:source", FabricationFault.Inadmissible));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MotionSource source,
        ref Option<MachineMatch> machine,
        ref Seq<AxisMotion> axes,
        ref MotionDynamics dynamics,
        ref Seq<ToolChangeEvidence> toolChanges,
        ref int park,
        ref Map<ModalGroup, GCommand> powerOn,
        ref Map<int, Transform> workOffsets,
        ref Map<int, Length> toolLengths,
        ref ControllerTiming timing,
        ref Dimension maximumNesting,
        ref Length softLimitMargin,
        ref UnitInterval activePowerFactor) =>
        validationError = Simulate.Folded(
            Slots(source, machine, axes, dynamics, toolChanges, park, powerOn, workOffsets, toolLengths, softLimitMargin));

    public static Fin<SimulatePolicy> Admit(
        MotionSource source,
        Option<MachineMatch> machine,
        Seq<AxisMotion> axes,
        MotionDynamics dynamics,
        Seq<ToolChangeEvidence> toolChanges,
        int park,
        Map<ModalGroup, GCommand> powerOn,
        Map<int, Transform> workOffsets,
        Map<int, Length> toolLengths,
        ControllerTiming timing,
        Dimension maximumNesting,
        Length softLimitMargin,
        UnitInterval activePowerFactor) =>
        AdmissionSlots.Accumulate(
            Slots(source, machine, axes, dynamics, toolChanges, park, powerOn, workOffsets, toolLengths, softLimitMargin))
            .ToFin()
            .Bind(_ => Validate(source, machine, axes, dynamics, toolChanges, park, powerOn, workOffsets, toolLengths,
                timing, maximumNesting, softLimitMargin, activePowerFactor, out SimulatePolicy policy).Admitted(policy));
}

public sealed record ControllerState(
    Map<ModalGroup, GCommand> Active,
    Map<int, Transform> Offsets,
    FrameState Frame,
    Point3d ProgramAt,
    Point3d MachineAt,

    Map<MachineAxis, double> Rotary,

    double FeedMmMinute,
    double SpindleRpm,

    Option<(double SurfaceMetersMinute, double MaximumRpm)> Css,

    double CssDiameterMm,

    int Tool,

    Option<int> LengthOffset,
    int Wcs,

    Map<ThermalAction, (double At, double Target)> Thermal,

    Option<GCommand> Ended) {

    public static ControllerState PowerOn(SimulatePolicy policy) => new(
        policy.PowerOn, policy.WorkOffsets, FrameState.Identity, Point3d.Origin, Point3d.Origin,
        toMap(Simulate.Addressable.Map(static axis => (axis, 0.0))),
        0.0, 0.0, None, 0.0, policy.Park, None, 1,
        toMap(toSeq(ThermalAction.Items).Map(static row => (row, (At: 0.0, Target: 0.0)))),
        None);
}
```

## [03]-[BLOCK_ADMISSION]

- Owner: `ArcEvidence` owns the working plane, the plane-projected radius, the helical rise, the arc-command sense roster, and the admitted `Move.Circular` every sweep read resolves through. `MotionGeometry` closes the span shape. `Instruction` closes the physical effect a word settles into. `ModalSlot` owns the address a modal command reads and the ordinal law it admits under. `CommandEffect` is the one admission table keyed by `GCommand`.
- Cases: `CommandEffect` reduces a word to motion, dwell, tool change, halt, constant-surface-speed, spindle, auxiliary, thermal, frame, or modal admission.
- Law: `Move.Circular.SweepRadians` carries the signed arc sweep. G-code carries no sweep, so this page derives it ONCE at the decode boundary — I/J/K or R fix the centre, `ArcEvidence.SenseOf` fixes the sense, and coincident plane-projected endpoints are the full turn a G2/G3 block spells that way — then hands it to `Move.Circular.Of`, which admits sign-against-sense and the `(0, Tau]` magnitude band before the instruction exists. Every reader here and at `Verify/removal` reads that one column, so no second sweep convention, sign rule, or angular epsilon exists on either page.
- Law: `ArcEvidence.SenseOf` is the arc DISCRIMINANT and it answers an option: a command the roster does not name is not an arc at all, so the span decode carries the sense it resolved rather than a boolean shadowing the same fact, and the feed ceiling reads the decode's own shape.
- Law: every block commands either a FEED or a block TIME, and the two are different dimensions. Both settle at admission into one millimetres-per-minute reading and one duration, so the peak-feed population never mixes reciprocal minutes with real feeds and the modal feed register is written only by the blocks that command one. Units-per-minute blocks clamp to the machine ceiling, which is what the machine physically does; an inverse-time block whose commanded duration demands a feed above that ceiling is INFEASIBLE and refuses, because clamping it silently reports a cycle time the machine cannot achieve.
- Law: `Simulate.Addressable` names the rotary addresses a posted WORD can carry — ISO 6983 spells rotation about X/Y/Z as `A`/`B`/`C`, while the joint rows share their address with the arc-centre offset word and reach no posted block. Commanding a rotary the policy never declared answers `SimulatedOvertravel` rather than executing as pure linear travel.
- Exemption: `ArcEvidence.Witnesses`, `ArcEvidence.Sweep`, and `Simulate.RadiusDefinition` are the numeric-kernel statement exemptions.
- Entry: `Simulate.AdmitMotion` settles geometry, rate, rotary travel, and the operating envelope gate before returning, so every refusal lands before a modal register moves.
- Auto: `GCommand.Grammar.Admit` validates address shape and `GCommand.Role` selects the clock band. Every dimensioned address arrives CANONICAL — the parse boundary folds the active `ModalGroup.Units` row through `ProgramUnits.Canonical`, so X/Y/Z, U/V/W, I/J/K, an arc or cycle R, and a per-minute F are millimetres before simulation reads them, and a unit scale re-applied here double-converts an inch program.
- Boundary: `Relative` is the one relative tolerance band on this cluster, and it governs both the endpoint-radius admission and the witness-amplitude degeneracy test, so the two cannot drift. `SpecializedToolpathEnvelope` proved kind correspondence, non-empty rows, and finite non-negative duration once at `Process/owner`, so no arm here revalidates a payload it was handed.

```csharp
[ComplexValueObject]
public sealed partial class ArcEvidence {
    public Plane Plane { get; }
    public Point3d From { get; }

    public Move.Circular Motion { get; }

    public double RadiusMm { get; }
    public double RiseMm { get; }

    private const double Relative = 1e-6;

    private static readonly Map<GCommand, RotationSense> Senses = Map(
        (GCommand.ArcCw, RotationSense.Clockwise),
        (GCommand.ArcCcw, RotationSense.Counterclockwise));

    public static Option<RotationSense> SenseOf(GCommand command) => Senses.Find(command);

    public Point3d Center => Motion.Arc.Center;
    public RotationSense Sense => Motion.Arc.Sense;
    public double SweepRadians => Motion.SweepRadians;
    public Point3d To => Motion.Target;
    public double LengthMm => Math.Sqrt(Math.Pow(RadiusMm * Math.Abs(SweepRadians), 2.0) + (RiseMm * RiseMm));

    private double StartRad => AngleOf(Plane, Center, From);

    public static double AngleOf(Plane plane, Point3d center, Point3d at) =>
        Math.Atan2((at - center) * plane.YAxis, (at - center) * plane.XAxis);

    public static double Sweep(Plane plane, Point3d center, Point3d from, Point3d to, RotationSense sense, double radiusMm) {
        double turn = sense == RotationSense.Clockwise ? -Math.Tau : Math.Tau;
        double direction = Math.Sign(turn);
        double advance = direction * (AngleOf(plane, center, to) - AngleOf(plane, center, from));
        double wrapped = advance - (Math.Floor(advance / Math.Tau) * Math.Tau);
        return plane.ClosestPoint(from).DistanceTo(plane.ClosestPoint(to)) <= radiusMm * Relative
            ? turn
            : direction * wrapped;
    }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Plane plane,
        ref Point3d from,
        ref Move.Circular motion,
        ref double radiusMm,
        ref double riseMm) {
        double endRadius = motion.Arc.Center.DistanceTo(plane.ClosestPoint(motion.Target));
        double tolerance = radiusMm * Relative;
        if (!(ValidityClaim.All(
            plane.IsValid, from.IsValid, ValidityClaim.Positive(radiusMm), double.IsFinite(riseMm),
            Math.Abs(plane.DistanceTo(motion.Arc.Center)) <= tolerance, Math.Abs(endRadius - radiusMm) <= tolerance,
            Math.Abs(plane.ClosestPoint(from).DistanceTo(motion.Arc.Center) - radiusMm) <= tolerance)))
            validationError = new ValidationError(string.Join(" | ", new object?[] { Kind.Arc, None, "arc-evidence:radius" }));
    }

    public static Fin<ArcEvidence> Admit(Plane plane, Point3d from, Move.Circular motion, double radiusMm, double riseMm) =>
        Validate(plane, from, motion, radiusMm, riseMm, out ArcEvidence evidence).Admitted(evidence);

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Instruction(GCommand Command) {

    public sealed record Motion(GCommand command, MotionGeometry Geometry, Point3d ProgramTo, Point3d MachineTo,
        Map<MachineAxis, double> Rotary, double FeedMmMinute, Duration Linear, FeedMode Mode,
        Option<double> Diameter, Duration Rotation, ClockBand Band) : Instruction(command);
    public sealed record Delay(GCommand command, DelayKind Kind, Duration Duration) : Instruction(command);
    public sealed record Tool(GCommand command, int Tool) : Instruction(command);
    public sealed record Spindle(GCommand command, double TargetRpm) : Instruction(command);
    public sealed record Css(GCommand command, double SurfaceMetersMinute, double MaximumRpm) : Instruction(command);
    public sealed record Thermal(GCommand command, ThermalAction Action, double TargetC) : Instruction(command);
    public sealed record Frame(GCommand command, FrameEffect Effect) : Instruction(command);
    public sealed record Modal(GCommand command) : Instruction(command);
}

internal readonly record struct WordContext(ProgramLocus Locus, ControllerState State, GNode.Word Word, SimulatePolicy Policy);

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
            context.Policy.Timing.Spans[context.Word.Command == GCommand.Stop ? DelayKind.RequiredStop : DelayKind.OptionalStop])));

    public static readonly CommandEffect Css = new(static context =>
        from surface in context.Word.P('S').Filter(static value => ValidityClaim.Positive(value).Holds)
            .ToFin(new KernelFault.InvalidValue("simulate", "simulate:css-surface"))
        from maximum in context.Word.P('D').Filter(static value => ValidityClaim.Positive(value).Holds)
            .ToFin(new KernelFault.InvalidValue("simulate", "simulate:css-maximum"))
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
        .ToFin(new KernelFault.InvalidValue("simulate", "simulate:spindle-target"))
        .Map(value => (Instruction)new Instruction.Spindle(context.Word.Command, value)));

    public static readonly CommandEffect Auxiliary = new(static context => Fin.Succ<Instruction>(
        new Instruction.Delay(context.Word.Command, DelayKind.AuxiliaryStabilization,
            context.Policy.Timing.Spans[DelayKind.AuxiliaryStabilization])));

    public static readonly CommandEffect Thermal = new(static context =>
        from action in ThermalAction.Of(context.Word.Command)
            .ToFin(new KernelFault.InvalidValue("simulate", "simulate:thermal-command"))
        from target in context.Word.P('S').Filter(static value => double.IsFinite(value) && value >= 0.0)
            .ToFin(new KernelFault.InvalidValue("simulate", "simulate:thermal-target"))
        select (Instruction)new Instruction.Thermal(context.Word.Command, action, target));

    public static readonly CommandEffect Frame = new(static context => FrameEffect.Of(context.Word.Command)
        .ToFin(new KernelFault.InvalidValue("simulate", "simulate:frame-command"))
        .Map(effect => (Instruction)new Instruction.Frame(context.Word.Command, effect)));

    public static readonly CommandEffect Modal = new(static context =>
        Fin.Succ<Instruction>(new Instruction.Modal(context.Word.Command)));

    [UseDelegateFromConstructor]
    public partial Fin<Instruction> Admit(WordContext context);
}
```

## [04]-[LEDGER]

- Owner: `SimulationSlice` is the sole ledger family and `SimulationLedger` accumulates it; `MotionTally` and `DelayTally` own the per-band and per-kind aggregates.
- Cases: `SimulationSlice` distinguishes motion, controller delay, additive deposition, specialized toolpath evidence, posed cell stations, and state evidence.
- Law: `SimulationLedger` is both the in-flight accumulator and the final output because both carry the slice run and final controller state.
- Law: `MotionDirective.Specialized` rows the walk executes carry its ADMITTED `SpecializedToolpathEnvelope` onto the ledger and out through `SimulationLedger.Specialized`, so wire, bevel, link, inspection, and turning rows survive the modal walk instead of being consumed and dropped. Direct specialized programs — those attached to no realized move — charge the duration their own `SpecializedToolpathEnvelope` carries; evidence attached to realized motion contributes no duplicate clock, because the moves it annotates already charged theirs.
- Output: `Cycle` sums `SimulationSlice.Elapsed` over the whole ledger and `EnergyKwh` sums `SimulationSlice.EnergyKwh`, so no projection can disagree with the ledger. `Bands`, `Delays`, and `Poses` are folds keyed by `ClockBand`, `DelayKind`, and the posed payload, so a new band, delay row, or cell station reports with no projection edit, and `DistanceMm` sums banded length beside posed travel so a cell cycle never reports a band-only zero. `MotionTally.PeakFeedMmMinute` aggregates one dimension, because both feed modes settled into millimetres per minute at admission.
- Boundary: `MotionTally` and `DelayTally` stay separate carriers because a delay row HAS no length and no feed — folding them into one tally seats two zero columns on every delay and lets a consumer read a travel distance off a dwell. `Execute` writes `Cycle`, `EnergyKwh`, and `DistanceMm` through `FabricationInstruments.CycleDuration`, `CycleEnergy`, and `CycleDistance`, so the authoritative cycle-time owner is the one histogram source; those three names are the frozen read and never move.

```csharp
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

public sealed record SimulationLedger(Seq<SimulationSlice> Slices, ControllerState State) {
    public static SimulationLedger PowerOn(SimulatePolicy policy) =>
        new(Seq<SimulationSlice>(), ControllerState.PowerOn(policy));

    public SimulationLedger Add(SimulationSlice slice) => this with { Slices = Slices.Add(slice) };

    public SimulationLedger Add(ControllerState state, SimulationSlice slice) =>
        new(Slices.Add(slice), state);

    public Duration Cycle => Slices.Fold(Duration.Zero, static (total, row) => total + row.Elapsed);
    public double EnergyKwh => Slices.Fold(0.0, static (total, row) => total + row.EnergyKwh);

    public Map<ClockBand, MotionTally> Bands => Slices.Fold(Map<ClockBand, MotionTally>(), static (tallies, row) =>
        row is SimulationSlice.Motion value
            ? tallies.AddOrUpdate(value.Band, tallies.Find(value.Band).IfNone(MotionTally.Empty)
                .Add(value.Duration, value.LengthMm, value.FeedMmMinute))
            : tallies);

    public Map<DelayKind, DelayTally> Delays => Slices.Fold(Map<DelayKind, DelayTally>(), static (tallies, row) =>
        row is SimulationSlice.Delay value
            ? tallies.AddOrUpdate(value.Kind, tallies.Find(value.Kind).IfNone(DelayTally.Empty).Add(value.Duration))
            : tallies);

    public Duration Deposition => Slices.Fold(Duration.Zero, static (total, row) =>
        row is SimulationSlice.Deposition value ? total + value.Duration : total);

    public Seq<SpecializedToolpathEnvelope> Specialized => Slices.Choose(static row =>
        row is SimulationSlice.Specialized value ? Some(value.Payload) : None);

    public Seq<CellPosedStation> Poses => Slices.Choose(static row =>
        row is SimulationSlice.Posed value ? Some(value.Station) : None);

    public double DistanceMm => Bands.Fold(0.0, static (total, tally) => total + tally.LengthMm)
        + Poses.Fold(0.0, static (total, station) => total + station.TravelMm);
}
```

## [05]-[MODAL_CLOCK]

- Owner: `Simulate` owns the execution fold, the ONE spindle-ramp charge, the ONE tool-change charge, the ONE jerk-limited profile every linear, arc, and rotary span times through, and the `Gate`/`Demand`/`Folded` admission slots every owner on this page reads.
- Entry: `public static Fin<SimulationLedger> Execute(SimulatePolicy policy, Option<InstrumentSet> set = default, Option<SpanBand> band = default)` folds executable `GNode` leaves through one `ControllerState` or folds the cell census, and fails before ledger mutation on a malformed inverse-time feed, an infeasible commanded block time, inconsistent offset- or radius-defined arcs, an unbanded motion role, a commanded rotary the policy never declared, an operating envelope breach, nesting beyond the admitted depth, execution after program end, a tool change the magazine census does not carry, or a cell whose compiled program carries no simulation. Execution runs inside the `FabricationEngine.Simulate` bracket the supplied band opens, and the settled ledger writes cycle duration, energy, and distance onto their `FabricationInstruments` rows through the supplied set — band and set both default absent, so a headless caller runs untraced and unmeasured with no branch of its own.
- Law: every commanded spindle speed costs its ramp WHEREVER it arrives — an `S` word riding a modal block, an `M03` target, a constant-surface-speed resolution, or a posted spindle directive all route through one charge, so no arrival path changes the speed for free. `RotationalSpeed` carries the ramp arithmetic, so the per-minute basis rides the quantity library rather than a transcribed factor.
- Law: tool changes cost the row the magazine measured for the ordered pair, and the spindle ALWAYS holds a real ordinal — an empty spindle sits at `MagazineLayout.Park`, whose index distance is zero by that layout's own definition, so the load collapses to arm swing through the magazine's own arithmetic and this page reconstructs nothing. Pairs the census does not carry are the magazine's gap and refuse.
- Law: `GCommand.ProgramEnd` is the only terminal row the program vocabulary carries. `Stop` and `OptionalStop` share its `ModalGroup.Stop` membership but are RESUMABLE halts — the operator restarts them and execution continues — so they charge their controller-timed delay and leave the run live. Adding a second terminal word is a vocabulary row, not a predicate edit here.
- Exemption: `Simulate.ProfileSeconds` and `Simulate.RadiusDefinition` are the numeric-kernel statement exemptions; `Simulate.ApplyModal` and `Simulate.ExecuteCell` are the fold-shaped statement exemptions.
- Auto: `CommandEffect` stays a dispatch TABLE — its rows carry admission bodies rather than a shape correspondence, so no generated projection replaces it. `MotionDynamics` supplies rapid, linear, arc, and rotary ceilings with acceleration and jerk already stamped by `Posting/program`, and the cell lane reads its clock from the look-ahead planner instead, because a serial chain resolves no `ClockBand` and no axis-limit profile.
- Law: `ExecuteCell` proves the posed ledger sums to `CellAnimation.Cycle`, which IS the look-ahead planner's `Program.Duration`, so the cell ledger reports the planner's own clock rather than a sampler census that drifted from it.
- Boundary: a policy or parameter failing its own admission gate answers `FabricationFault.PolicyInadmissible` on its raising plane; only genuinely degenerate geometry answers the kernel `GeometryFault.DegenerateInput` band, so a missing work offset or an unresolvable tool length never borrows a fabricated `Kind`. Machine-less simulation omits the operating envelope and machine-energy gates but retains program, arc, feed, and rotary admission. `ExecuteCell` carries the power-on controller state unchanged because a serial chain has no modal controller. Every successful ledger sums exactly to its own projections.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Simulate {
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
        .Concat(toSeq(ThermalAction.Items).Bind(static action => action.Commands.Map(command => (command, CommandEffect.Thermal))))
        .Concat(toSeq(FrameEffect.Items).Map(static effect => (effect.Command, CommandEffect.Frame)))
        .Fold(Map<GCommand, CommandEffect>(), static (table, row) => table.AddOrUpdate(row.Item1, row.Item2));

    internal static readonly Seq<MachineAxis> Addressable = Seq(MachineAxis.A, MachineAxis.B, MachineAxis.C);

    public static Fin<SimulationLedger> Execute(SimulatePolicy policy, Option<InstrumentSet> set = default, Option<SpanBand> band = default) =>
        band.Traced(FabricationEngine.Simulate, _ =>
            from ledger in policy.Source.Switch(
                state: policy,
                posted: static (law, row) => ExecutePosted(law, row.Program),
                cell: static (law, row) => ExecuteCell(law, row))
            from _duration in set.Write(FabricationInstruments.CycleDuration, ledger.Cycle.TotalSeconds)
            from _energy in set.Write(FabricationInstruments.CycleEnergy, ledger.EnergyKwh)
            from _distance in set.Write(FabricationInstruments.CycleDistance, ledger.DistanceMm)
            select ledger);

    private static Fin<SimulationLedger> ExecutePosted(SimulatePolicy policy, CutProgram program) =>
        from steps in Flatten(program.Nodes, Seq<ProgramPathStep>(), policy)
        from folded in steps.FoldM<Fin, SimulationLedger>(
            SimulationLedger.PowerOn(policy),
            (state, step) => ExecuteStep(state, step, policy)).As()
        select folded;

    private static Fin<SimulationLedger> ExecuteCell(SimulatePolicy policy, MotionSource.Cell source) =>
        RobotProgram.Run(source.Cell, source.Moves, new CellProgramRequest.Animation(source.Policy, source.Clock))
            .Bind(outcome => outcome.Switch(
                    motion: static _ => Option<CellAnimation>.None,
                    placement: static _ => Option<CellAnimation>.None,
                    animation: static row => Some(row.Value))
                .ToFin(new KernelFault.InvalidValue("simulate", "simulate:cell-modality")))
            .Bind(animation => {
                Seq<SimulationSlice> slices = animation.Stations.Map(station => (SimulationSlice)new SimulationSlice.Posed(
                    new ProgramLocus(station.Station, Seq(new ProgramPathStep(station.Station, None))),
                    station,
                    policy.Machine.Map(machine => EnergyKwh(machine.PowerKw * policy.ActivePowerFactor.Value, station.Elapsed))
                        .IfNone(0.0)));
                return slices.Fold(Duration.Zero, static (total, row) => total + row.Elapsed) == animation.Cycle
                    ? Fin.Succ(new SimulationLedger(slices, ControllerState.PowerOn(policy)))
                    : Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", "simulate:cell-clock"));
            });

    private static Fin<Seq<(ProgramLocus Locus, GNode Node)>> Flatten(Seq<GNode> nodes, Seq<ProgramPathStep> path, SimulatePolicy policy) =>
        path.Count > policy.MaximumNesting.Value
            ? Fin.Fail<Seq<(ProgramLocus, GNode)>>(new KernelFault.InvalidValue("simulate", "simulate:nesting-depth"))
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
            : Fin.Fail<Seq<(ProgramLocus, GNode)>>(new KernelFault.InvalidValue("simulate", "simulate:cycle-repeats")),
        coordinateFrame: static (at, value) => Fin.Succ(Seq((at.Locus, (GNode)value))),
        macro: static (at, value) => Flatten(value.Body.ToSeq(), at.Locus.Path, at.Policy),
        subprogram: static (at, value) => value.Repeats > 0
            ? Range(0, value.Repeats).ToSeq()
                .TraverseM(index => Flatten(
                    value.Body.ToSeq(),
                    at.Locus.Path.Init.Add(new ProgramPathStep(at.Locus.Block, Some(index))),
                    at.Policy))
                .As()
                .Map(static groups => groups.Fold(Seq<(ProgramLocus, GNode)>(), static (all, group) => all.Concat(group)))
            : Fin.Fail<Seq<(ProgramLocus, GNode)>>(new KernelFault.InvalidValue("simulate", "simulate:subprogram-repeats")),
        additiveLayer: static (at, value) => Fin.Succ(Seq((at.Locus, (GNode)value))),
        nc1: static (_, _) => Fin.Fail<Seq<(ProgramLocus, GNode)>>(new KernelFault.InvalidValue("simulate", "simulate:nc1-clock-owner-required")),
        directive: static (at, value) => Fin.Succ(Seq((at.Locus, (GNode)value))));

    private static Fin<SimulationLedger> ExecuteStep(SimulationLedger fold, (ProgramLocus Locus, GNode Node) step, SimulatePolicy policy) =>
        fold.State.Ended.Match(
            Some: ended => Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", $"simulate:after-end:{ended.Key}")),
            None: () => step.Node.Switch(
                state: (Fold: fold, Locus: step.Locus, Policy: policy),
                block: static (_, _) => Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", "simulate:unflattened-block")),
                word: static (context, value) => ExecuteWord(context.Fold, context.Locus, value, context.Policy),
                cannedCycle: static (context, value) => ExecuteCycle(context.Fold, context.Locus, value, context.Policy),
                coordinateFrame: static (context, value) => Fin.Succ(context.Fold.Add(
                    context.Fold.State with { Offsets = context.Fold.State.Offsets.AddOrUpdate(
                        value.Assignment.Setup, Transform.PlaneToPlane(Plane.WorldXY, value.Frame)) },
                    new SimulationSlice.State(context.Locus, value))),
                macro: static (_, _) => Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", "simulate:unflattened-macro")),
                subprogram: static (_, _) => Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", "simulate:unflattened-subprogram")),
                additiveLayer: static (context, value) => ExecuteAdditive(context.Fold, context.Locus, value, context.Policy),
                nc1: static (_, _) => Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", "simulate:nc1-clock-owner-required")),
                directive: static (context, value) => ExecuteDirective(context.Fold, context.Locus, value.Value, context.Policy)));

    private static Fin<SimulationLedger> ExecuteDirective(
        SimulationLedger fold,
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
                : Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", "simulate:dwell-without-spindle")),
        synchronize: static (context, row) => Fin.Succ(context.Fold.Add(
            new SimulationSlice.State(context.Locus, new GNode.Directive(row)))),
        orientedStop: static (context, row) => Fin.Succ(context.Fold.Add(
            new SimulationSlice.State(context.Locus, new GNode.Directive(row)))),
        channelBarrier: static (context, row) => Fin.Succ(context.Fold.Add(
            new SimulationSlice.State(context.Locus, new GNode.Directive(row)))),
        specialized: static (context, row) => {
            Duration elapsed = row.AfterMove < 0 ? Duration.FromSeconds(row.Payload.DurationSeconds) : Duration.Zero;
            return Fin.Succ(context.Fold.Add(new SimulationSlice.Specialized(
                context.Locus,
                row.Payload,
                elapsed,
                context.Policy.Machine.Map(machine => EnergyKwh(
                    machine.PowerKw * context.Policy.ActivePowerFactor.Value, elapsed)).IfNone(0.0))));
        });

    private static Fin<SimulationLedger> ExecuteCycle(SimulationLedger fold, ProgramLocus locus, GNode.CannedCycle cycle, SimulatePolicy policy) =>
        Range(0, cycle.Repeats).FoldM<Fin, SimulationLedger>(fold, (state, _) => cycle.ExpandedMoves.IsEmpty
            ? ExecuteWord(state, locus, new GNode.Word(cycle.Command, cycle.SingleBlockWords, cycle.Mode), policy)
            : cycle.ExpandedMoves.FoldM<Fin, SimulationLedger>(state, (nested, move) =>
                GNode.Move(move, nested.State.ProgramAt) is GNode.Word word
                    ? ExecuteWord(nested, locus, word with { Mode = cycle.Mode }, policy)
                    : Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", "simulate:cycle-move"))).As()).As();

    private static Fin<SimulationLedger> ExecuteWord(SimulationLedger fold, ProgramLocus locus, GNode.Word word, SimulatePolicy policy) =>
        from instruction in AdmitInstruction(new WordContext(locus, fold.State, word, policy))
        from advanced in Apply(fold, locus, word, instruction, policy)
        select advanced;

    private static Fin<Instruction> AdmitInstruction(WordContext context) =>
        (context.Word.Command.Group == ModalGroup.Motion
            ? CommandEffect.Motion
            : Effects.Find(context.Word.Command).IfNone(CommandEffect.Modal)).Admit(context);

    // --- [MOTION_ADMISSION]
    internal static Fin<Instruction.Motion> AdmitMotion(WordContext context) {
        (ControllerState state, GNode.Word word, SimulatePolicy policy) = (context.State, context.Word, context.Policy);
        Map<ModalGroup, GCommand> active = Stamp(state.Active, word);
        GCommand distance = active.Find(ModalGroup.Distance).IfNone(GCommand.Absolute);
        Point3d programTo = Target(state.ProgramAt, word, distance);
        FeedMode mode = word.Command == GCommand.Rapid ? FeedMode.UnitsPerMinute
            : word.Mode.IfNone(active.Find(ModalGroup.Feed).Exists(static command => command == GCommand.FeedInverseTime)
                ? FeedMode.InverseTime : FeedMode.UnitsPerMinute);
        return from offset in state.Offsets.Find(state.Wcs).ToFin(
                       new KernelFault.InvalidValue("simulate", "simulate:wcs-reference"))
               from length in state.LengthOffset.Traverse(value => policy.ToolLengths.Find(value)
                   .ToFin(new KernelFault.InvalidValue("simulate", "simulate:length-reference"))).As()
               let toolLength = length.Map(static row => row.Millimeters).IfNone(0.0)
               let machineTo = (offset * (state.Frame.Combined * programTo)) + (toolLength * (offset * Vector3d.ZAxis))
               from band in ClockBand.Of(word.Command.Role)
                   .ToFin(new KernelFault.InvalidValue("simulate", "simulate:motion-band"))
               from span in SpanLength(state.ProgramAt, programTo, word, active)
               from rate in Rate(word, mode, state, span.LengthMm, span.Ceiling(policy.Dynamics, band), policy)
               from geometry in span.Admitted(programTo, rate.FeedMmMinute)
               from rotary in RotarySpan(context.Locus, state, word, distance, policy)
               from _ in Envelope(context.Locus, machineTo, geometry, offset * state.Frame.Combined, toolLength, word, policy)
               select new Instruction.Motion(word.Command, geometry, programTo, machineTo,
                   rotary.Targets, rate.FeedMmMinute, rate.Linear, mode,
                   word.P('X').Map(_ => Math.Abs(programTo.X)), rotary.Elapsed, band);
    }

    private static Fin<(double FeedMmMinute, Duration Linear)> Rate(
        GNode.Word word,
        FeedMode mode,
        ControllerState state,
        double lengthMm,
        double ceilingMmMinute,
        SimulatePolicy policy) => mode == FeedMode.InverseTime
        ? word.P('F').Filter(static value => ValidityClaim.Positive(value).Holds)
            .ToFin(new KernelFault.InvalidValue("simulate", "simulate:inverse-time-word"))
            .Map(static reciprocal => SecondsPerMinute / reciprocal)
            .Bind(seconds => double.IsFinite(seconds)
                ? Fin.Succ((Derived: lengthMm * SecondsPerMinute / seconds, Block: Duration.FromSeconds(seconds)))
                : Fin.Fail<(double Derived, Duration Block)>(
                    new KernelFault.InvalidValue("simulate", "simulate:inverse-time-word")))
            .Bind(row => row.Derived <= ceilingMmMinute
                ? Fin.Succ((row.Derived, row.Block))
                : Fin.Fail<(double, Duration)>(
                    new KernelFault.InvalidValue("simulate", "simulate:inverse-time-infeasible")))
        : (word.Command == GCommand.Rapid ? Some(policy.Dynamics.RapidFeed) : word.P('F').OrElse(Some(state.FeedMmMinute)))
            .Filter(static value => ValidityClaim.Positive(value).Holds)
            .ToFin(new KernelFault.InvalidValue("simulate", "simulate:motion-feed"))
            .Map(feed => (feed, Duration.FromSeconds(ProfileSeconds(
                lengthMm, feed / SecondsPerMinute, ceilingMmMinute / SecondsPerMinute,
                policy.Dynamics.Acceleration, policy.Dynamics.Jerk))));

    private readonly record struct SpanDecode(
        double LengthMm,
        Option<(Plane Plane, Point3d Center, RotationSense Sense, double SweepRadians, double RadiusMm, double RiseMm)> Arc) {

        public double Ceiling(MotionDynamics dynamics, ClockBand band) =>
            band == ClockBand.Rapid ? dynamics.RapidFeed : Arc.IsSome ? dynamics.ArcFeed : dynamics.LinearFeed;

        public Fin<MotionGeometry> Admitted(Point3d target, double feedMmMinute) => Arc.Match(
            None: () => Fin.Succ<MotionGeometry>(new MotionGeometry.Linear(LengthMm)),
            Some: row => from move in Move.Circular.Of(target, feedMmMinute, new ArcCenter(row.Center, row.Sense), row.SweepRadians)
                         from circular in move.CircularGeometry.ToFin(
                             new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-atom"))
                         from evidence in ArcEvidence.Admit(row.Plane, row.Plane.Origin, circular, row.RadiusMm, row.RiseMm)
                         select (MotionGeometry)new MotionGeometry.Arc(evidence));
    }

    private static Fin<SpanDecode> SpanLength(
        Point3d from, Point3d to, GNode.Word word, Map<ModalGroup, GCommand> active) =>
        ArcEvidence.SenseOf(word.Command).Match(
            None: () => Fin.Succ(new SpanDecode(from.DistanceTo(to), None)),
            Some: sense => Arc(from, to, word, active, sense).Map(static row => new SpanDecode(
                Math.Sqrt(Math.Pow(row.RadiusMm * Math.Abs(row.SweepRadians), 2.0) + (row.RiseMm * row.RiseMm)),
                Some(row))));

    private static Fin<(Plane Plane, Point3d Center, RotationSense Sense, double SweepRadians, double RadiusMm, double RiseMm)> Arc(
        Point3d from, Point3d to, GNode.Word word, Map<ModalGroup, GCommand> active, RotationSense sense) {
        GCommand planeCommand = active.Find(ModalGroup.Plane).IfNone(GCommand.PlaneXy);
        Plane plane = planeCommand == GCommand.PlaneZx ? new Plane(from, Vector3d.ZAxis, Vector3d.XAxis)
            : planeCommand == GCommand.PlaneYz ? new Plane(from, Vector3d.YAxis, Vector3d.ZAxis)
            : new Plane(from, Vector3d.XAxis, Vector3d.YAxis);
        double i = word.P('I').IfNone(0.0), j = word.P('J').IfNone(0.0), k = word.P('K').IfNone(0.0);
        Vector3d offset = planeCommand == GCommand.PlaneZx ? new Vector3d(i, 0.0, k)
            : planeCommand == GCommand.PlaneYz ? new Vector3d(0.0, j, k) : new Vector3d(i, j, 0.0);
        Point3d absolute = new(
            planeCommand == GCommand.PlaneYz ? from.X : i,
            planeCommand == GCommand.PlaneZx ? from.Y : j,
            planeCommand == GCommand.PlaneXy ? from.Z : k);
        Option<double> radiusWord = word.P('R');
        bool carriesOffset = word.P('I').IsSome || word.P('J').IsSome || word.P('K').IsSome;
        Fin<Point3d> definition = radiusWord.Match(
            None: () => carriesOffset
                ? Fin.Succ(active.Find(ModalGroup.ArcDistance).Exists(static command => command == GCommand.ArcAbsolute)
                    ? absolute
                    : from + offset)
                : Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-center")),
            Some: radius => carriesOffset
                ? Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-definition-conflict"))
                : RadiusDefinition(radius, from, to, plane, sense));
        return definition.Bind(center => {
            double radiusMm = center.DistanceTo(plane.ClosestPoint(from));
            return ValidityClaim.Positive(radiusMm) ? Fin.Succ((plane, center, sense,
                    ArcEvidence.Sweep(plane, center, from, to, sense, radiusMm),
                    radiusMm,
                    plane.DistanceTo(to) - plane.DistanceTo(from)))
                : Fin.Fail<(Plane, Point3d, RotationSense, double, double, double)>(
                    new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-radius"));
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
        if (!ValidityClaim.Positive(radius).Holds || length <= 0.0 || length > 2.0 * radius)
            return Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-radius"));
        Point3d midpoint = plane.ClosestPoint(from) + (plane.DistanceTo(from) * plane.ZAxis) + (0.5 * chord);
        Vector3d normal = Vector3d.CrossProduct(plane.ZAxis, chord) / length;
        double height = Math.Sqrt(Math.Max(0.0, (radius * radius) - (0.25 * length * length)));
        return Seq(midpoint + (height * normal), midpoint - (height * normal))
            .Find(center => signedRadius < 0.0
                ? Math.Abs(ArcEvidence.Sweep(plane, center, from, to, sense, radius)) >= Math.PI
                : Math.Abs(ArcEvidence.Sweep(plane, center, from, to, sense, radius)) <= Math.PI)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Arc, None, "simulate:arc-radius-branch"));
    }

    internal static Fin<double> NonNegativeSeconds(Option<double> seconds, string locus) =>
        seconds.Filter(static value => double.IsFinite(value) && value >= 0.0)
            .ToFin(new KernelFault.InvalidValue("simulate", locus));

    internal static Fin<int> Ordinal(Option<double> raw, string locus, Func<int, bool> admitted) =>
        raw.Filter(value => double.IsFinite(value) && value >= int.MinValue && value <= int.MaxValue
                && value == Math.Truncate(value) && admitted((int)value))
            .Map(static value => (int)value)
            .ToFin(new KernelFault.InvalidValue("simulate", locus));

    internal static Fin<Option<int>> OptionalOrdinal(Option<double> raw, string locus, Func<int, bool> admitted) =>
        raw.TraverseM(value => Ordinal(Some(value), locus, admitted)).As();

    // --- [ROTARY_SPAN]
    private static Fin<(Duration Elapsed, Map<MachineAxis, double> Targets)> RotarySpan(
        ProgramLocus locus, ControllerState state, GNode.Word word, GCommand distance, SimulatePolicy policy) =>
        from rows in Addressable.TraverseM(axis => RotaryTarget(locus, state, word, distance, policy, axis)).As()
        from spans in rows.Filter(row => row.At != state.Rotary[row.Axis])
            .TraverseM(row => policy.Axis(row.Axis)
                .ToFin(new FabricationFault.SimulatedOvertravel(locus.Block, row.Axis, Math.Abs(row.At - state.Rotary[row.Axis])))
                .Bind(motion => RotaryTravel(locus, motion, state.Rotary[row.Axis], row.At, policy.Dynamics))).As()
        select (Duration.FromSeconds(spans.Fold(0.0, Math.Max)), toMap(rows.Map(static row => (row.Axis, row.At))));

    private static Fin<(MachineAxis Axis, double At)> RotaryTarget(
        ProgramLocus locus, ControllerState state, GNode.Word word, GCommand distance, SimulatePolicy policy, MachineAxis axis) =>
        word.P(axis.Address).Match(
            None: () => Fin.Succ((axis, state.Rotary[axis])),
            Some: raw => double.IsFinite(raw)
                ? policy.Axis(axis)
                    .ToFin(new FabricationFault.SimulatedOvertravel(locus.Block, axis, UnitsNet.Angle.FromDegrees(raw).Radians))
                    .Map(_ => (axis, UnitsNet.Angle.FromDegrees(raw).Radians
                        + (distance == GCommand.Relative ? state.Rotary[axis] : 0.0)))
                : Fin.Fail<(MachineAxis, double)>(new KernelFault.InvalidValue("simulate", $"simulate:rotary-target:{axis.Key}")));

    private static Fin<double> RotaryTravel(
        ProgramLocus locus, AxisMotion axis, double from, double to, MotionDynamics dynamics) =>
        axis.Periodicity.Cyclic || axis.Contains(to)
            ? Fin.Succ(ProfileSeconds(
                axis.Periodicity.Period.Match(
                    Some: period => Math.Abs(Math.IEEERemainder(to - from, period)),
                    None: () => Math.Abs(to - from)),
                Math.Min(axis.MaximumVelocity, UnitsNet.Angle.FromDegrees(dynamics.RotaryFeed).Radians / SecondsPerMinute),
                axis.MaximumVelocity,
                Math.Min(axis.MaximumAcceleration, dynamics.RotaryAcceleration),
                Math.Min(axis.MaximumJerk, dynamics.RotaryJerk)))
            : Fin.Fail<double>(new FabricationFault.SimulatedOvertravel(
                locus.Block, axis.Axis, Math.Max(axis.Min - to, to - axis.Max)));

    // --- [ENVELOPE_GATE]
    private static Fin<Unit> Envelope(
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
                .TraverseM(point => EnvelopePoint(locus, point, word, machine.Instance.Envelope,
                    word.Command == GCommand.Rapid ? policy.SoftLimitMargin.Millimeters : 0.0)).As().Map(static _ => unit));

    private static Fin<Unit> EnvelopePoint(ProgramLocus locus, Point3d point, GNode.Word word, BoundingBox box, double margin) {
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
    private static Fin<SimulationLedger> Apply(SimulationLedger fold, ProgramLocus locus, GNode.Word word, Instruction instruction, SimulatePolicy policy) =>
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

    private static Fin<SimulationLedger> ApplyMotion(SimulationLedger fold, ProgramLocus locus, GNode.Word word, Instruction.Motion motion, SimulatePolicy policy) {
        double diameter = motion.Diameter.IfNone(fold.State.CssDiameterMm);
        double spindleRpm = fold.State.Css
            .Map(css => CssRpm(css.SurfaceMetersMinute, css.MaximumRpm, diameter))
            .IfNone(word.P('S').Filter(static value => double.IsFinite(value) && value >= 0.0).IfNone(fold.State.SpindleRpm));
        Duration elapsed = Seq(motion.Linear, motion.Rotation, RampSeconds(fold.State.SpindleRpm, spindleRpm, policy))
            .Fold(Duration.Zero, static (longest, row) => row > longest ? row : longest);
        double power = policy.Machine.Map(machine =>
            motion.Band == ClockBand.Rapid ? machine.Instance.IdlePowerKw : machine.PowerKw).IfNone(0.0);
        return Fin.Succ(fold.Add(
            fold.State with {
                Active = Stamp(fold.State.Active, word),
                ProgramAt = motion.ProgramTo,
                MachineAt = motion.MachineTo,
                Rotary = motion.Rotary,
                FeedMmMinute = motion.Mode == FeedMode.UnitsPerMinute ? motion.FeedMmMinute : fold.State.FeedMmMinute,
                SpindleRpm = spindleRpm,
                CssDiameterMm = diameter,
            },
            new SimulationSlice.Motion(locus, motion.Command, motion.Band, elapsed, motion.Geometry.LengthMm,
                motion.FeedMmMinute, EnergyKwh(power * policy.ActivePowerFactor.Value, elapsed))));
    }

    private static Fin<SimulationLedger> ApplyDelay(SimulationLedger fold, ProgramLocus locus, GCommand command, DelayKind kind, Duration elapsed, SimulatePolicy policy) =>
        Fin.Succ(fold.Add(
            fold.State with { Active = Stamp(fold.State.Active, command), Ended = Terminal(command) },
            new SimulationSlice.Delay(locus, command, kind, elapsed,
                policy.Machine.Map(machine => EnergyKwh(
                    machine.Instance.IdlePowerKw * policy.ActivePowerFactor.Value, elapsed)).IfNone(0.0))));

    private static Fin<SimulationLedger> ApplyTool(SimulationLedger fold, ProgramLocus locus, Instruction.Tool tool, SimulatePolicy policy) =>
        policy.Changes.Find((fold.State.Tool, tool.Tool))
            .ToFin(new KernelFault.InvalidValue("simulate", $"simulate:tool-change-evidence:{fold.State.Tool}:{tool.Tool}"))
            .Bind(row => ApplyDelay(
                fold with { State = fold.State with { Tool = tool.Tool, LengthOffset = None } },
                locus, tool.Command, DelayKind.ToolChange, row.Elapsed, policy));

    private static Fin<SimulationLedger> ApplyCss(
        SimulationLedger fold,
        ProgramLocus locus,
        GNode.Word word,
        Instruction.Css css,
        SimulatePolicy policy) =>
        ApplyModal(
            fold with { State = fold.State with { Css = Some((css.SurfaceMetersMinute, css.MaximumRpm)) } },
            locus, word, fold.State.Frame, policy)
            .Bind(next => ChargeSpindle(next, locus, css.Command,
                CssRpm(css.SurfaceMetersMinute, css.MaximumRpm, next.State.CssDiameterMm), policy));

    private static Fin<SimulationLedger> ApplyThermal(SimulationLedger fold, ProgramLocus locus, Instruction.Thermal thermal, SimulatePolicy policy) {
        (double at, double _) = fold.State.Thermal[thermal.Action];
        Duration elapsed = thermal.Action.Blocks(thermal.Command)
            ? Duration.FromSeconds(Math.Abs(thermal.TargetC - at) / policy.Timing.Ramp[thermal.Action].Value)
            : Duration.Zero;
        return ApplyDelay(
            fold with { State = fold.State with {
                Thermal = fold.State.Thermal.AddOrUpdate(thermal.Action,
                    (At: elapsed == Duration.Zero ? at : thermal.TargetC, Target: thermal.TargetC)),
            } },
            locus, thermal.Command, DelayKind.ThermalRamp, elapsed, policy);
    }

    private static Fin<SimulationLedger> ApplyModal(
        SimulationLedger fold,
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
            .ToFin(new KernelFault.InvalidValue("simulate", "simulate:wcs-reference"))).As()
        from _length in admitted.Find(ModalSlot.LengthOffset).Traverse(value => policy.ToolLengths.Find(value)
            .ToFin(new KernelFault.InvalidValue("simulate", "simulate:length-reference"))).As()
        let lengthOffset = word.Command == GCommand.LengthOffset ? admitted.Find(ModalSlot.LengthOffset)
            : word.Command == GCommand.LengthCancel ? None : fold.State.LengthOffset
        let state = fold.State with {
            Active = Stamp(fold.State.Active, word),
            Offsets = offsets,
            Frame = frame,
            Wcs = admitted.Find(ModalSlot.Wcs).IfNone(fold.State.Wcs),
            LengthOffset = lengthOffset,
            Css = word.Command == GCommand.CssCancel ? None : fold.State.Css,
            Ended = Terminal(word.Command),
        }
        select fold.Add(state, new SimulationSlice.State(locus, word));

    private static Fin<SimulationLedger> Spun(SimulationLedger fold, ProgramLocus locus, GNode.Word word, SimulatePolicy policy) =>
        word.P('S').Match(
            None: () => Fin.Succ(fold),
            Some: raw => double.IsFinite(raw) && raw >= 0.0
                ? raw == fold.State.SpindleRpm
                    ? Fin.Succ(fold)
                    : ChargeSpindle(fold, locus, word.Command, raw, policy)
                : Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", "simulate:spindle-target")));

    private static Fin<SimulationLedger> ChargeSpindle(
        SimulationLedger fold, ProgramLocus locus, GCommand command, double targetRpm, SimulatePolicy policy) =>
        ApplyDelay(
            fold with { State = fold.State with { SpindleRpm = targetRpm } },
            locus, command, DelayKind.SpindleRamp, RampSeconds(fold.State.SpindleRpm, targetRpm, policy), policy);

    private static Duration RampSeconds(double fromRpm, double toRpm, SimulatePolicy policy) => Duration.FromSeconds(
        Math.Abs(RotationalSpeed.FromRevolutionsPerMinute(toRpm - fromRpm).RevolutionsPerSecond)
        / policy.Timing.SpindleRevolutionsPerSecondSquared.Value);

    private static Fin<SimulationLedger> ExecuteAdditive(SimulationLedger fold, ProgramLocus locus, GNode.AdditiveLayer layer, SimulatePolicy policy) =>
        ValidityClaim.All(
            ValidityClaim.Positive(layer.Extrusion.Feed),
            double.IsFinite(layer.Extrusion.Amount), layer.Extrusion.Amount >= 0.0)
            ? from hotend in ApplyThermal(fold, locus,
                  new Instruction.Thermal(GCommand.HotendWait, ThermalAction.Hotend, layer.Temperatures.Hotend), policy)
              from bed in ApplyThermal(hotend, locus,
                  new Instruction.Thermal(GCommand.BedWait, ThermalAction.Bed, layer.Temperatures.Bed), policy)
              let elapsed = Duration.FromSeconds(layer.Extrusion.Amount / layer.Extrusion.Feed * SecondsPerMinute)
              let power = policy.Machine.Map(static machine => machine.PowerKw).IfNone(0.0)
              select bed.Add(new SimulationSlice.Deposition(
                  locus, elapsed, layer.Extrusion.Amount, layer.Extrusion.Feed,
                  EnergyKwh(power * policy.ActivePowerFactor.Value, elapsed)))
            : Fin.Fail<SimulationLedger>(new KernelFault.InvalidValue("simulate", "simulate:additive-layer"));

    // --- [ADMISSION_SLOTS]
    internal static K<Validation<Error>, Unit> Demand<TRow>(
        CapabilitySet<TRow> held, CapabilitySet<TRow> demanded, string locus)
        where TRow : notnull, ICapability<TRow> =>
        held.Require(demanded, missing => new KernelFault.InvalidValue("simulate", $"{locus}:{missing.Wire}"))
            .Map(static _ => unit)
            .ToValidation();

    internal static ValidationError? Folded(Seq<K<Validation<Error>, Unit>> slots) =>
        AdmissionSlots.Accumulate(slots).Match(
            Fail: static _ => new ValidationError("simulate:admission"),
            Succ: static _ => (ValidationError?)null);

    // --- [CLOCK_KERNEL]
    private const double SecondsPerMinute = 60.0;

    private static double CssRpm(double surfaceMetersMinute, double maximumRpm, double diameterMm) =>
        diameterMm <= 0.0
            ? maximumRpm
            : Math.Min(maximumRpm, SurfaceSpeed.Rpm(surfaceMetersMinute, diameterMm));

    private static double EnergyKwh(double powerKw, Duration elapsed) =>
        (UnitsNet.Power.FromKilowatts(powerKw) * UnitsNet.Duration.FromSeconds(elapsed.TotalSeconds)).KilowattHours;

    private static Option<GCommand> Terminal(GCommand command) =>
        command == GCommand.ProgramEnd ? Some(command) : None;

    private static Map<ModalGroup, GCommand> Stamp(Map<ModalGroup, GCommand> active, GNode.Word word) => Stamp(active, word.Command);

    private static Map<ModalGroup, GCommand> Stamp(Map<ModalGroup, GCommand> active, GCommand command) =>
        command.Group == ModalGroup.NonModal ? active : active.AddOrUpdate(command.Group, command);

    private static Point3d Target(Point3d from, GNode.Word word, GCommand distance) => new(
        Advance(from.X, word.P('X'), distance),
        Advance(from.Y, word.P('Y'), distance),
        Advance(from.Z, word.P('Z'), distance));

    private static double Advance(double held, Option<double> commanded, GCommand distance) =>
        commanded.Map(raw => distance == GCommand.Relative ? held + raw : raw).IfNone(held);

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
-->

(none)
