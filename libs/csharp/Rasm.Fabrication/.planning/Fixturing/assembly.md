# [RASM_FABRICATION_ASSEMBLY]

`AssemblyPlan` owns member admission, join admission, fit-up, precedence, access, load-path stability, temporary support, tolerance closure, joining resources, robot-cell placement, inspection, release, handling, and disassembly evidence. `AssemblyPolicy` admits every component instance, connection specification, datum, fixture, resource, execution policy, and clearance once; the planner consumes typed joints and graph facts only.

`AssemblyJoint.Index` remains the connection-census identity shared with joining plans. `AssemblyPlan.Apply` closes admission, planning, replanning, disassembly, and projection over `AssemblyOp`, and every egress carries the content key minted through `ContentKey.Of`.

## [01]-[INDEX]

- [01]-[JOINS]: method payloads, phases, access, fit, resources, and inspection.
- [02]-[PLANNING]: typed precedence, physical connectivity, stability, clearance, scheduling, and receipts.
- [03]-[PROJECTION]: joining, traveler, inspection, handling, service, and evidence egress.

## [02]-[JOINS]

- Owner: `JoinProcess` closes fusion, brazed, soldered, bonded, threaded, riveted, studded, interference, clinched, pinned, snapped, and connector methods with per-occurrence payloads.
- Execution: `AssemblyExecution` carries `InspectionCadence` and a positive lane ceiling, and `Lanes` bounds allocation by executable demand; named production presets never become domain cases and cadence is never a boolean.
- Classification: `JoinClass` and `PrecedenceKind` carry their own wire `Code` column, so canonical bytes read the declaration and a new row cannot inherit its predecessor's code through a trailing ladder arm.
- Program: `JoinProgram` discriminates the joining and service lifecycles, so `Phases` and `DurationOf` each own both modalities on one entrypoint rather than a name-suffixed sibling pair.
- Thermal: `JoinProcess.ThermalLoad` projects deposited energy per method, so distortion ordering ranks hot joints against each other instead of collapsing to one hot-before-cold edge.
- Phases: `JoinPhase` covers locate, fit, tack, preheat, apply, dwell, cool, torque, inspect, release, unlock, extract, clean, handle, and final states, each row carrying the `PrecedenceKind` that entering it satisfies; `JoinSpecification` carries a duration for every phase its program visits, so schedule time is never fiction between the acting phases.
- Access: `AccessCorridor` carries approach axis, typed cone angle, cutter and holder envelope, standoff, approach, retract, and visibility constraints.
- Fit: `FitRequirement` carries gap, interference, alignment, surface, temperature, and tolerance limits as one admitted value.
- Growth: a new join method is one `JoinProcess` case with its total projections; phase, edge, scheduler, and consumer surfaces remain unchanged.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Fixturing;

// --- [JOINS] --------------------------------------------------------------------------------------------------------------------------------------
// Every vocabulary carries its own wire `Code`, so a new row supplies one at declaration and no
// consumer maintains a parallel ordinal ladder whose trailing arm silently reuses the last code.
[SmartEnum<string>]
public sealed partial class JoinClass {
    public static readonly JoinClass Weld = new("weld", code: 0);
    public static readonly JoinClass Braze = new("braze", code: 1);
    public static readonly JoinClass Solder = new("solder", code: 2);
    public static readonly JoinClass Adhesive = new("adhesive", code: 3);
    public static readonly JoinClass Bolt = new("bolt", code: 4);
    public static readonly JoinClass Screw = new("screw", code: 5);
    public static readonly JoinClass Rivet = new("rivet", code: 6);
    public static readonly JoinClass Stud = new("stud", code: 7);
    public static readonly JoinClass PressFit = new("press-fit", code: 8);
    public static readonly JoinClass Clinch = new("clinch", code: 9);
    public static readonly JoinClass Pin = new("pin", code: 10);
    public static readonly JoinClass Snap = new("snap", code: 11);
    public static readonly JoinClass Connector = new("connector", code: 12);

    public int Code { get; }
}

[SmartEnum<string>]
public sealed partial class JoinProgram {
    public static readonly JoinProgram Assembly = new("assembly");
    public static readonly JoinProgram Service = new("service");
}

[SmartEnum<string>]
public sealed partial class JoinPhase {
    public static readonly JoinPhase Locate = new("locate", rank: 0, PrecedenceKind.Phase);
    public static readonly JoinPhase Fit = new("fit", rank: 1, PrecedenceKind.Support);
    public static readonly JoinPhase Tack = new("tack", rank: 2, PrecedenceKind.Fit);
    public static readonly JoinPhase Preheat = new("preheat", rank: 3, PrecedenceKind.Thermal);
    public static readonly JoinPhase Apply = new("apply", rank: 4, PrecedenceKind.Fit);
    public static readonly JoinPhase Dwell = new("dwell", rank: 5, PrecedenceKind.Cure);
    public static readonly JoinPhase Cool = new("cool", rank: 6, PrecedenceKind.Cure);
    public static readonly JoinPhase Torque = new("torque", rank: 7, PrecedenceKind.Fit);
    public static readonly JoinPhase Inspect = new("inspect", rank: 8, PrecedenceKind.Inspection);
    public static readonly JoinPhase Release = new("release", rank: 9, PrecedenceKind.LoadPath);
    public static readonly JoinPhase Handle = new("handle", rank: 10, PrecedenceKind.Handling);
    public static readonly JoinPhase Final = new("final", rank: 11, PrecedenceKind.Phase);
    public static readonly JoinPhase Unlock = new("unlock", rank: 12, PrecedenceKind.Service);
    public static readonly JoinPhase Extract = new("extract", rank: 13, PrecedenceKind.Service);
    public static readonly JoinPhase Clean = new("clean", rank: 14, PrecedenceKind.Service);

    public int Rank { get; }
    public PrecedenceKind Entered { get; }
}

// Cadence is a shop policy over the join ordinal, never a boolean: a line inspecting every fifth
// joint and one inspecting at subassembly close are the same all-or-nothing flag otherwise.
[SmartEnum<string>]
public sealed partial class InspectionCadence {
    public static readonly InspectionCadence Never = new("never", static (_, _) => false);
    public static readonly InspectionCadence EveryJoin = new("every-join", static (_, _) => true);
    public static readonly InspectionCadence EveryOther = new("every-other", static (ordinal, _) => ordinal % 2 == 1);
    public static readonly InspectionCadence EveryFifth = new("every-fifth", static (ordinal, _) => ordinal % 5 == 4);
    public static readonly InspectionCadence SubassemblyClose = new("subassembly-close", static (_, last) => last);

    public Func<int, bool, bool> Applies { get; }
}

[ComplexValueObject]
public sealed partial class AssemblyExecution {
    public InspectionCadence Cadence { get; }
    public int MaxParallel { get; }
    public bool Parallel => MaxParallel > 1;
    public int Lanes(int demand) => Math.Min(MaxParallel, Math.Max(1, demand));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref InspectionCadence cadence,
        ref int maxParallel) =>
        validationError = cadence is not null && maxParallel > 0
            ? null
            : new ValidationError(message: "<invalid-assembly-execution>");
}

[ComplexValueObject]
public readonly partial struct FitRequirement {
    public Length GapMin { get; }
    public Length GapMax { get; }
    public Length InterferenceMax { get; }
    public Length AlignmentMax { get; }
    public Length ClosureMax { get; }
    public Length SurfaceRoughnessMax { get; }
    public Temperature TemperatureMin { get; }
    public Temperature TemperatureMax { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length gapMin,
        ref Length gapMax,
        ref Length interferenceMax,
        ref Length alignmentMax,
        ref Length closureMax,
        ref Length surfaceRoughnessMax,
        ref Temperature temperatureMin,
        ref Temperature temperatureMax) =>
        validationError = new[] { gapMin, gapMax, interferenceMax, alignmentMax, closureMax, surfaceRoughnessMax }
            .Map(static value => value.As(LengthUnit.Millimeter)).ForAll(double.IsFinite)
            && double.IsFinite(temperatureMin.As(TemperatureUnit.DegreeCelsius))
            && double.IsFinite(temperatureMax.As(TemperatureUnit.DegreeCelsius))
            && gapMin.As(LengthUnit.Millimeter) >= 0.0 && gapMax >= gapMin
            && interferenceMax.As(LengthUnit.Millimeter) >= 0.0 && alignmentMax.As(LengthUnit.Millimeter) >= 0.0
            && closureMax.As(LengthUnit.Millimeter) >= 0.0 && surfaceRoughnessMax.As(LengthUnit.Millimeter) >= 0.0
            && temperatureMax >= temperatureMin
                ? null
                : new ValidationError(message: "<invalid-fit-requirement>");
}

public readonly record struct AccessCorridor(
    Vector3d Axis,
    Angle HalfAngle,
    Length Standoff,
    Length ToolRadius,
    Length HolderRadius,
    Length Approach,
    Length Retract,
    bool LineOfSight);

[ComplexValueObject]
public readonly partial struct LinearEnergy {
    public Energy Energy { get; }
    public Length Basis { get; }
    public double JoulesPerMillimeter => Energy.As(EnergyUnit.Joule) / Basis.As(LengthUnit.Millimeter);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Energy energy,
        ref Length basis) =>
        validationError = double.IsFinite(energy.As(EnergyUnit.Joule)) && energy.As(EnergyUnit.Joule) > 0.0
            && double.IsFinite(basis.As(LengthUnit.Millimeter)) && basis.As(LengthUnit.Millimeter) > 0.0
                ? null
                : new ValidationError(message: "<invalid-linear-energy>");
}

[Union]
public abstract partial record JoinProcess {
    private JoinProcess() { }

    public sealed record Fusion(string Procedure, LinearEnergy HeatInput, Temperature Preheat, Temperature Interpass, bool Tackable) : JoinProcess;
    public sealed record Brazed(string Filler, Temperature Liquidus, Duration Dwell, Energy DepositedEnergy) : JoinProcess;
    public sealed record Soldered(string Filler, Temperature Liquidus, Duration Dwell, Energy DepositedEnergy) : JoinProcess;
    public sealed record Bonded(string Adhesive, Length Bondline, Duration Cure, Pressure ClampPressure) : JoinProcess;
    public sealed record Threaded(string Fastener, Torque Torque, Force Preload, string Locking, bool Screw) : JoinProcess;
    public sealed record Riveted(string Rivet, Force UpsetForce, Length HeadHeight) : JoinProcess;
    public sealed record Studded(string Stud, Force InstallationForce, Option<Energy> ArcEnergy) : JoinProcess;
    public sealed record Interference(Length Interference, Force InsertionForce, TemperatureDelta TemperatureDelta, Option<Energy> ConditioningEnergy) : JoinProcess;
    public sealed record Clinched(string Tool, Force Force, Length Button) : JoinProcess;
    public sealed record Pinned(string Pin, Force InsertionForce, bool Removable) : JoinProcess;
    public sealed record Snapped(string Feature, Force Engagement, Force Release) : JoinProcess;
    public sealed record Connector(string Key, Force MatingForce, bool Latching) : JoinProcess;

    public JoinClass Class => Switch(
        fusion: static _ => JoinClass.Weld,
        brazed: static _ => JoinClass.Braze,
        soldered: static _ => JoinClass.Solder,
        bonded: static _ => JoinClass.Adhesive,
        threaded: static row => row.Screw ? JoinClass.Screw : JoinClass.Bolt,
        riveted: static _ => JoinClass.Rivet,
        studded: static _ => JoinClass.Stud,
        interference: static _ => JoinClass.PressFit,
        clinched: static _ => JoinClass.Clinch,
        pinned: static _ => JoinClass.Pin,
        snapped: static _ => JoinClass.Snap,
        connector: static _ => JoinClass.Connector);

    public bool Reversible => Switch(
        fusion: static _ => false,
        brazed: static _ => false,
        soldered: static _ => true,
        bonded: static _ => false,
        threaded: static _ => true,
        riveted: static _ => false,
        studded: static row => row.ArcEnergy.IsNone,
        interference: static _ => false,
        clinched: static _ => false,
        pinned: static row => row.Removable,
        snapped: static _ => true,
        connector: static _ => true);

    public bool Thermal => Switch(
        fusion: static _ => true,
        brazed: static _ => true,
        soldered: static _ => true,
        bonded: static _ => false,
        threaded: static _ => false,
        riveted: static _ => false,
        studded: static row => row.ArcEnergy.IsSome,
        interference: static row => row.ConditioningEnergy.IsSome,
        clinched: static _ => false,
        pinned: static _ => false,
        snapped: static _ => false,
        connector: static _ => false);

    public bool Valid => Switch(
        fusion: static row => !string.IsNullOrWhiteSpace(row.Procedure) && row.HeatInput.JoulesPerMillimeter > 0.0
            && double.IsFinite(row.HeatInput.JoulesPerMillimeter)
            && double.IsFinite(row.Preheat.As(TemperatureUnit.DegreeCelsius))
            && double.IsFinite(row.Interpass.As(TemperatureUnit.DegreeCelsius)),
        brazed: static row => !string.IsNullOrWhiteSpace(row.Filler)
            && Finite(row.Liquidus.As(TemperatureUnit.DegreeCelsius), row.Dwell.As(DurationUnit.Second), row.DepositedEnergy.As(EnergyUnit.Joule))
            && row.Dwell >= Duration.Zero && row.DepositedEnergy > Energy.Zero,
        soldered: static row => !string.IsNullOrWhiteSpace(row.Filler)
            && Finite(row.Liquidus.As(TemperatureUnit.DegreeCelsius), row.Dwell.As(DurationUnit.Second), row.DepositedEnergy.As(EnergyUnit.Joule))
            && row.Dwell >= Duration.Zero && row.DepositedEnergy > Energy.Zero,
        bonded: static row => !string.IsNullOrWhiteSpace(row.Adhesive)
            && Finite(row.Bondline.As(LengthUnit.Millimeter), row.Cure.As(DurationUnit.Second), row.ClampPressure.As(PressureUnit.Kilopascal))
            && row.Bondline.As(LengthUnit.Millimeter) > 0.0 && row.Cure.As(DurationUnit.Second) > 0.0
            && row.ClampPressure.As(PressureUnit.Kilopascal) >= 0.0,
        threaded: static row => !string.IsNullOrWhiteSpace(row.Fastener) && !string.IsNullOrWhiteSpace(row.Locking)
            && Finite(row.Torque.As(TorqueUnit.NewtonMeter), row.Preload.As(ForceUnit.Newton))
            && row.Torque.As(TorqueUnit.NewtonMeter) > 0.0 && row.Preload.As(ForceUnit.Newton) > 0.0,
        riveted: static row => !string.IsNullOrWhiteSpace(row.Rivet)
            && Finite(row.UpsetForce.As(ForceUnit.Newton), row.HeadHeight.As(LengthUnit.Millimeter))
            && row.UpsetForce.As(ForceUnit.Newton) > 0.0 && row.HeadHeight.As(LengthUnit.Millimeter) > 0.0,
        studded: static row => !string.IsNullOrWhiteSpace(row.Stud) && double.IsFinite(row.InstallationForce.As(ForceUnit.Newton))
            && row.InstallationForce.As(ForceUnit.Newton) > 0.0
            && row.ArcEnergy.ForAll(static energy => double.IsFinite(energy.As(EnergyUnit.Joule)) && energy > Energy.Zero),
        interference: static row => Finite(row.Interference.As(LengthUnit.Millimeter), row.InsertionForce.As(ForceUnit.Newton),
                row.TemperatureDelta.As(TemperatureDeltaUnit.DegreeCelsius))
            && row.Interference.As(LengthUnit.Millimeter) > 0.0
            && row.InsertionForce.As(ForceUnit.Newton) > 0.0
            && row.ConditioningEnergy.ForAll(static energy => double.IsFinite(energy.As(EnergyUnit.Joule)) && energy > Energy.Zero)
            && (row.TemperatureDelta.As(TemperatureDeltaUnit.DegreeCelsius) == 0.0) == row.ConditioningEnergy.IsNone,
        clinched: static row => !string.IsNullOrWhiteSpace(row.Tool)
            && Finite(row.Force.As(ForceUnit.Newton), row.Button.As(LengthUnit.Millimeter))
            && row.Force.As(ForceUnit.Newton) > 0.0 && row.Button.As(LengthUnit.Millimeter) > 0.0,
        pinned: static row => !string.IsNullOrWhiteSpace(row.Pin) && double.IsFinite(row.InsertionForce.As(ForceUnit.Newton))
            && row.InsertionForce.As(ForceUnit.Newton) > 0.0,
        snapped: static row => !string.IsNullOrWhiteSpace(row.Feature)
            && Finite(row.Engagement.As(ForceUnit.Newton), row.Release.As(ForceUnit.Newton))
            && row.Engagement.As(ForceUnit.Newton) > 0.0 && row.Release.As(ForceUnit.Newton) > 0.0,
        connector: static row => !string.IsNullOrWhiteSpace(row.Key) && double.IsFinite(row.MatingForce.As(ForceUnit.Newton))
            && row.MatingForce.As(ForceUnit.Newton) > 0.0);

    // JoinProgram selects assembly or service through one phase algebra.
    public Seq<JoinPhase> Phases(JoinProgram program, AssemblyExecution execution, int ordinal, bool last) =>
        program.Switch(
            assembly: () => Assembly(execution.Cadence.Applies(ordinal, last)),
            service: () => Service(execution.Cadence.Applies(ordinal, last)));

    public Seq<JoinPhase> RequiredPhases(JoinProgram program) => program.Switch(
        assembly: () => Assembly(inspect: true),
        service: () => Service(inspect: true));

    Seq<JoinPhase> Assembly(bool inspect) => Switch(
        state: inspect,
        fusion: static (state, row) => Seq(JoinPhase.Locate, JoinPhase.Fit) + (row.Tackable ? Seq(JoinPhase.Tack) : Seq<JoinPhase>())
            + Seq(JoinPhase.Preheat, JoinPhase.Apply, JoinPhase.Cool) + (state ? Seq(JoinPhase.Inspect) : Seq<JoinPhase>())
            + Seq(JoinPhase.Release, JoinPhase.Handle, JoinPhase.Final),
        brazed: static (state, _) => HeatPhases(state),
        soldered: static (state, _) => HeatPhases(state),
        bonded: static (state, _) => DwellPhases(state),
        threaded: static (state, _) => MechanicalPhases(state, JoinPhase.Torque),
        riveted: static (state, _) => MechanicalPhases(state, JoinPhase.Apply),
        studded: static (state, row) => row.ArcEnergy.IsSome ? HeatPhases(state) : MechanicalPhases(state, JoinPhase.Apply),
        interference: static (state, _) => MechanicalPhases(state, JoinPhase.Apply),
        clinched: static (state, _) => MechanicalPhases(state, JoinPhase.Apply),
        pinned: static (state, _) => MechanicalPhases(state, JoinPhase.Apply),
        snapped: static (state, _) => MechanicalPhases(state, JoinPhase.Apply),
        connector: static (state, _) => MechanicalPhases(state, JoinPhase.Apply));

    Seq<JoinPhase> Service(bool inspect) => Switch(
        state: inspect,
        fusion: static (_, _) => Seq<JoinPhase>(),
        brazed: static (_, _) => Seq<JoinPhase>(),
        soldered: static (state, _) => Seq(JoinPhase.Locate, JoinPhase.Preheat, JoinPhase.Unlock, JoinPhase.Extract)
            + (state ? Seq(JoinPhase.Inspect) : Seq<JoinPhase>()) + Seq(JoinPhase.Clean, JoinPhase.Handle, JoinPhase.Final),
        bonded: static (_, _) => Seq<JoinPhase>(),
        threaded: static (state, _) => MechanicalService(state),
        riveted: static (_, _) => Seq<JoinPhase>(),
        studded: static (state, row) => row.ArcEnergy.IsSome ? Seq<JoinPhase>() : MechanicalService(state),
        interference: static (_, _) => Seq<JoinPhase>(),
        clinched: static (_, _) => Seq<JoinPhase>(),
        pinned: static (state, row) => row.Removable ? MechanicalService(state) : Seq<JoinPhase>(),
        snapped: static (state, _) => MechanicalService(state),
        connector: static (state, _) => MechanicalService(state));

    // Thermal load is the sequencing quantity, not a boolean: two hot joints on shared members
    // order by deposited energy, and a cold joint yields to any of them.
    public Energy ThermalLoad => Switch(
        fusion: static row => row.HeatInput.Energy,
        brazed: static row => row.DepositedEnergy,
        soldered: static row => row.DepositedEnergy,
        bonded: static _ => Energy.FromJoules(0.0),
        threaded: static _ => Energy.FromJoules(0.0),
        riveted: static _ => Energy.FromJoules(0.0),
        studded: static row => row.ArcEnergy.IfNone(Energy.Zero),
        interference: static row => row.ConditioningEnergy.IfNone(Energy.Zero),
        clinched: static _ => Energy.FromJoules(0.0),
        pinned: static _ => Energy.FromJoules(0.0),
        snapped: static _ => Energy.FromJoules(0.0),
        connector: static _ => Energy.FromJoules(0.0));

    public Duration Dwell => Switch(
        fusion: static _ => Duration.Zero,
        brazed: static row => row.Dwell,
        soldered: static row => row.Dwell,
        bonded: static row => row.Cure,
        threaded: static _ => Duration.Zero,
        riveted: static _ => Duration.Zero,
        studded: static _ => Duration.Zero,
        interference: static _ => Duration.Zero,
        clinched: static _ => Duration.Zero,
        pinned: static _ => Duration.Zero,
        snapped: static _ => Duration.Zero,
        connector: static _ => Duration.Zero);

    static Seq<JoinPhase> HeatPhases(bool inspect) => Seq(JoinPhase.Locate, JoinPhase.Fit, JoinPhase.Preheat, JoinPhase.Apply, JoinPhase.Dwell, JoinPhase.Cool)
        + (inspect ? Seq(JoinPhase.Inspect) : Seq<JoinPhase>()) + Seq(JoinPhase.Release, JoinPhase.Handle, JoinPhase.Final);
    static Seq<JoinPhase> DwellPhases(bool inspect) => Seq(JoinPhase.Locate, JoinPhase.Fit, JoinPhase.Apply, JoinPhase.Dwell)
        + (inspect ? Seq(JoinPhase.Inspect) : Seq<JoinPhase>()) + Seq(JoinPhase.Release, JoinPhase.Handle, JoinPhase.Final);
    static Seq<JoinPhase> MechanicalPhases(bool inspect, JoinPhase action) => Seq(JoinPhase.Locate, JoinPhase.Fit, action)
        + (inspect ? Seq(JoinPhase.Inspect) : Seq<JoinPhase>()) + Seq(JoinPhase.Release, JoinPhase.Handle, JoinPhase.Final);
    static Seq<JoinPhase> MechanicalService(bool inspect) => Seq(JoinPhase.Locate, JoinPhase.Unlock, JoinPhase.Extract)
        + (inspect ? Seq(JoinPhase.Inspect) : Seq<JoinPhase>()) + Seq(JoinPhase.Clean, JoinPhase.Handle, JoinPhase.Final);

    static bool Finite(params double[] values) => values.All(double.IsFinite);
}

public sealed record JoinSpecification(
    JoinProcess Process,
    Arr<AssemblyMemberKey> Components,
    Seq<AccessCorridor> Access,
    FitRequirement Fit,
    Seq<string> Resources,
    Seq<int> Fixtures,
    Option<Angle> GrooveIncludedAngle,
    Force Capacity,
    HashMap<JoinPhase, Duration> PhaseDurations,
    HashMap<JoinPhase, Duration> ServiceDurations,
    double ReleaseStrengthFraction) {
    // Locate, fit, tack, preheat, cool, release, handle, clean, and unlock consume real floor
    // time; a duration model covering apply, inspect, and dwell alone makes every schedule
    // between them fiction. The process supplies dwell where the routing carries none.
    public Duration DurationOf(JoinProgram program, JoinPhase phase) =>
        (program == JoinProgram.Service ? ServiceDurations : PhaseDurations)
            .Find(phase)
            .IfNone(() => phase == JoinPhase.Dwell || phase == JoinPhase.Cool ? Process.Dwell : Duration.Zero);

    public bool DurationsValid => Seq(JoinProgram.Assembly, JoinProgram.Service).ForAll(program => {
        HashMap<JoinPhase, Duration> durations = program == JoinProgram.Service ? ServiceDurations : PhaseDurations;
        return Process.RequiredPhases(program).ForAll(phase =>
            (durations.ContainsKey(phase) || phase == JoinPhase.Dwell || phase == JoinPhase.Cool)
            && DurationOf(program, phase).As(DurationUnit.Second) is var seconds
            && double.IsFinite(seconds) && seconds >= 0.0);
    });
}

[ComplexValueObject]
public readonly partial struct AssemblyMemberKey {
    public UInt128 Representation { get; }
    public int Instance { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UInt128 representation,
        ref int instance) =>
        validationError = representation != 0 && instance >= 0
            ? null
            : new ValidationError(message: "<invalid-assembly-member-key>");
}

public sealed record AssemblyMember(AssemblyMemberKey Key, AdmittedComponent Component, Transform Pose);

public sealed record AssemblyJoint(int Index, AssemblyMemberKey Owner, ComponentConnection Connection, Edge3 At, JoinSpecification Specification);
```

## [03]-[PLANNING]

- Owner: `AssemblyPolicy` is raw ingress; `JoinNode` is one executable phase; `AssemblyPlan` is the reduced proof-bearing result.
- Precedence: `PrecedenceKind` closes phase, datum, occlusion, fit, load-path, support, thermal, cure, resource, inspection, handling, and reversible-service reasons.
- Graph: `BidirectionalGraph<JoinNode, AssemblyEdge>` carries reason payloads directly, while the component `UndirectedGraph` remains the disjoint physical-connectivity projection.
- Stability: `IAssemblyEvidenceSource.Evaluate` proves connected support, capacity, center-of-gravity margin, temporary fixture custody, load-path continuity, fit, visibility, and robot placement once per join; every phase retains that receipt.
- Tolerance: fit and datum errors fold along component paths, and a join fails admission when gap, interference, alignment, or accumulated closure exceeds its carried requirement.
- Access: every approach and retract corridor composes `Workholding.Apply` at the phase’s `FixtureState`; analytic cone occlusion checks every potential neighbor over the full axial interval.
- Schedule: source-first order respects resource exclusivity, dwell, cool, inspection, and lane policy; each step carries typed start, finish, fixture, resources, and stability evidence, and every receipt resolves by joint key rather than array position.
- Service: disassembly reverses the proven precedence order, so an occlusion or thermal edge that gated a join gates its removal; a roster reversal ignores both.
- Replan: removing a completed or blocked joint re-proves every surviving receipt against the residual assembly through the same evidence boundary, because removal moves the load path the original receipts measured.
- Exemption: QuikGraph construction, component labeling, bounded scheduling folds, analytic corridor kernels, and canonical boundary serialization mutate only their admitted containers; every adjacent collection in the preimage is count-framed.
- Boundary: precedence and physical connectivity remain distinct; cycle evidence retains the cyclic joint and edge census, and geometry failure, missing specification, unstable release, and blocked access remain typed failures carrying a `JoinRejection` reason rather than one opaque code.

```csharp signature
// --- [PLANNING] -----------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PrecedenceKind {
    public static readonly PrecedenceKind Phase = new("phase", code: 0);
    public static readonly PrecedenceKind Datum = new("datum", code: 1);
    public static readonly PrecedenceKind Occlusion = new("occlusion", code: 2);
    public static readonly PrecedenceKind Fit = new("fit", code: 3);
    public static readonly PrecedenceKind LoadPath = new("load-path", code: 4);
    public static readonly PrecedenceKind Support = new("support", code: 5);
    public static readonly PrecedenceKind Thermal = new("thermal", code: 6);
    public static readonly PrecedenceKind Cure = new("cure", code: 7);
    public static readonly PrecedenceKind Resource = new("resource", code: 8);
    public static readonly PrecedenceKind Inspection = new("inspection", code: 9);
    public static readonly PrecedenceKind Handling = new("handling", code: 10);
    public static readonly PrecedenceKind Service = new("service", code: 11);

    public int Code { get; }
}

public readonly record struct JoinNode(int Joint, JoinPhase Phase);

public readonly record struct AssemblyEdge(JoinNode Source, JoinNode Target, PrecedenceKind Kind) : IEdge<JoinNode>;

public readonly record struct StabilityReceipt(
    int Components,
    double CapacityMargin,
    double SupportMargin,
    double LoadPathMargin,
    bool FixtureHeld) {
    public double Minimum => Seq(CapacityMargin, SupportMargin, LoadPathMargin).Min();
    public bool Stable => Components > 0 && Minimum >= 1.0;
}

public readonly record struct ToleranceReceipt(
    Length Gap,
    Length Interference,
    Length Alignment,
    Length Closure,
    Length SurfaceRoughness,
    Temperature Temperature);

public readonly record struct AssemblyBoundaryEvidence(
    ToleranceReceipt Tolerance,
    StabilityReceipt Stability,
    Option<CellPlacementReceipt> Robot,
    Seq<bool> Visibility);

public interface IAssemblyEvidenceSource {
    Fin<AssemblyBoundaryEvidence> Evaluate(
        AssemblyJoint joint,
        Seq<AssemblyMember> members,
        Seq<Fixture> fixtures,
        AssemblyPolicy policy);
}

public readonly record struct JoinReceipt(
    int Joint,
    ToleranceReceipt Tolerance,
    Seq<WorkholdingResult.Clearance> Clearance,
    StabilityReceipt Stability,
    Option<CellPlacementReceipt> Robot,
    Seq<bool> Visibility,
    Seq<string> Resources,
    Duration Duration);

public readonly record struct JoinStep(
    int Order,
    int Joint,
    JoinPhase Phase,
    int Subassembly,
    Option<int> Fixture,
    Seq<string> Resources,
    Duration Start,
    Duration Finish,
    StabilityReceipt Stability);

public readonly record struct BlockedCorridor(int Joint, int Corridor, int Occluder);

public sealed record AssemblyPolicy(
    AssemblyExecution Execution,
    Length CorridorClearance,
    Seq<int> DatumJoints,
    FixtureSet Fixtures,
    Map<string, JoinSpecification> Specifications,
    Force HandlingLoad,
    IAssemblyEvidenceSource Evidence);

[Union]
public abstract partial record AssemblyOp {
    private AssemblyOp() { }

    public sealed record Admit(Seq<AssemblyMember> Members, AssemblyPolicy Policy) : AssemblyOp;
    public sealed record Plan(Seq<AssemblyMember> Members, AssemblyPolicy Policy) : AssemblyOp;
    public sealed record Replan(AssemblyPlan Plan, AssemblyPolicy Policy, Seq<int> Completed, Seq<int> Blocked) : AssemblyOp;
    public sealed record Disassemble(AssemblyPlan Plan, Seq<int> Targets) : AssemblyOp;
    public sealed record Project(AssemblyPlan Plan, AssemblyProjection Projection) : AssemblyOp;
}

[Union]
public abstract partial record AssemblyResult {
    private AssemblyResult() { }

    public sealed record Admitted(Seq<AssemblyJoint> Joints, AssemblyPolicy Policy) : AssemblyResult;
    public sealed record Planned(AssemblyPlan Plan) : AssemblyResult;
    public sealed record Replanned(AssemblyPlan Plan) : AssemblyResult;
    public sealed record Disassembled(Seq<JoinStep> Steps) : AssemblyResult;
    public sealed record Projected(AssemblyArtifact Artifact) : AssemblyResult;
}

public sealed partial record AssemblyPlan(
    Seq<AssemblyMember> Members,
    AssemblyExecution Execution,
    Seq<JoinStep> Steps,
    int Subassemblies,
    Seq<AssemblyEdge> Precedence,
    Seq<AssemblyJoint> Joints,
    Seq<JoinReceipt> Receipts,
    Seq<BlockedCorridor> Blocked,
    Seq<JoinStep> ServiceOrder,
    ContentKey Key) {
    public static Fin<AssemblyResult> Apply(AssemblyOp? op) =>
        Optional(op).ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Absent()).ToError()).Bind(operation => operation.Switch(
            admit: static row => Admit(row.Members, row.Policy).Map<AssemblyResult>(joints => new AssemblyResult.Admitted(joints, row.Policy)),
            plan: static row => Admit(row.Members, row.Policy).Bind(joints => Ordered(row.Members, joints, row.Policy)).Map<AssemblyResult>(static plan => new AssemblyResult.Planned(plan)),
            replan: static row => Replan(row.Plan, row.Policy, row.Completed, row.Blocked).Map<AssemblyResult>(static plan => new AssemblyResult.Replanned(plan)),
            disassemble: static row => Disassemble(row.Plan, row.Targets).Map<AssemblyResult>(static steps => new AssemblyResult.Disassembled(steps)),
            project: static row => Project(row.Plan, row.Projection).Map<AssemblyResult>(static artifact => new AssemblyResult.Projected(artifact))));

    static Fin<Seq<AssemblyJoint>> Admit(Seq<AssemblyMember> members, AssemblyPolicy policy) =>
        (GateMembers(members), GatePolicy(members, policy), Census(members, policy))
            .Apply(static (_, _, joints) => joints)
            .As()
            .ToFin();

    static K<Validation<Error>, Unit> GateMembers(Seq<AssemblyMember> members) {
        if (members.Exists(static member => member is null))
            return Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Membership(-1, members.Count, 0)).ToError());
        Set<AssemblyMemberKey> keys = members.Map(static member => member.Key).ToSet();
        return members.Count > 0 && keys.Count == members.Count
        && members.ForAll(member => member is not null && member.Component is not null
            && member.Key.Representation == member.Component.RepresentationKey && member.Pose.IsValid)
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Membership(-1, members.Count, keys.Count)).ToError());
    }

    static K<Validation<Error>, Unit> GatePolicy(Seq<AssemblyMember> input, AssemblyPolicy policy) {
        if (input.Exists(static member => member is null || member.Component is null) || policy is null || policy.Execution is null
            || policy.Evidence is null || policy.Fixtures is null)
            return Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Membership(-1, input.Count, 0)).ToError());
        Set<AssemblyMemberKey> members = input.Map(static member => member.Key).ToSet();
        Set<string> realized = input.Bind(static member => member.Component.Connections.ToSeq()
            .Map(static connection => connection.RealizingKey)).ToSet();
        return
        policy.CorridorClearance.As(LengthUnit.Millimeter) >= 0.0
        && double.IsFinite(policy.CorridorClearance.As(LengthUnit.Millimeter))
        && double.IsFinite(policy.HandlingLoad.As(ForceUnit.Newton)) && policy.HandlingLoad.As(ForceUnit.Newton) >= 0.0
        && policy.DatumJoints.Distinct().Count() == policy.DatumJoints.Count
        && policy.Fixtures.ByOperation.Values.ForAll(static fixture => fixture.Constraint.Constrained)
        && realized.Count == policy.Specifications.Count
        && realized.ForAll(policy.Specifications.ContainsKey)
        && policy.Specifications.Values.ForAll(specification => specification is not null && specification.Process is not null
            && specification.Components.Count >= 2 && specification.Process.Valid
            && specification.Components.Distinct().Count() == specification.Components.Count
            && specification.Components.ForAll(members.Contains)
            && specification.Access.ForAll(Valid)
            && specification.Fixtures.ForAll(policy.Fixtures.ByOperation.ContainsKey)
            && specification.Fixtures.Distinct().Count() == specification.Fixtures.Count
            && specification.Resources.ForAll(static resource => !string.IsNullOrWhiteSpace(resource))
            && specification.Resources.Distinct().Count() == specification.Resources.Count
            && specification.GrooveIncludedAngle.ForAll(static angle => double.IsFinite(angle.As(AngleUnit.Radian))
                && angle.As(AngleUnit.Radian) is > 0.0 and <= Math.PI)
            && double.IsFinite(specification.Capacity.As(ForceUnit.Newton))
            && specification.Capacity.As(ForceUnit.Newton) > 0.0
            && specification.DurationsValid
            && double.IsFinite(specification.ReleaseStrengthFraction)
            && specification.ReleaseStrengthFraction is > 0.0 and <= 1.0)
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Membership(-1, realized.Count, policy.Specifications.Count)).ToError());
    }

    static K<Validation<Error>, Seq<AssemblyJoint>> Census(Seq<AssemblyMember> input, AssemblyPolicy policy) {
        if (input.Exists(static member => member is null || member.Component is null) || policy is null)
            return Validation<Error, Seq<AssemblyJoint>>.Fail(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Membership(-1, input.Count, 0)).ToError());
        return toSeq(input.Bind(member => member.Component.Connections.ToSeq().Map(connection => (member, connection)))
            .GroupBy(static row => row.connection.RealizingKey)
            .OrderBy(static group => group.Key, StringComparer.Ordinal))
            .Map((group, index) => (Rows: group
                .OrderBy(static row => row.member.Key.Representation)
                .ThenBy(static row => row.member.Key.Instance)
                .ThenBy(static row => row.connection.DetailKey, StringComparer.Ordinal)
                .ToSeq(), Index: index))
            .Traverse(group => (policy.Specifications.Find(group.Rows.Head.Map(static row => row.connection.RealizingKey).IfNone(string.Empty))
                    .ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Membership(group.Index, group.Rows.Count, 0)).ToError()),
                group.Rows.Choose(static row => row.connection.At.Map(at => (row.member, row.connection, at))).Head
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Line, group.Index, nameof(ComponentConnection.At)).ToError()))
                .Apply((specification, located) => specification.Components.Count >= 2
                    && group.Rows.ForAll(row => specification.Components.Contains(row.member.Key))
                    ? Fin.Succ(new AssemblyJoint(group.Index, specification.Components[0], located.connection,
                        new Edge3(located.member.Pose * located.at.A, located.member.Pose * located.at.B), specification))
                    : Fin.Fail<AssemblyJoint>(new FabricationFault.FixtureInadmissible(
                        new FixturingWitness.Membership(group.Index, group.Rows.Count,
                            group.Rows.Count(row => specification.Components.Contains(row.member.Key)))).ToError()))
                .As().Bind(identity).ToValidation());
    }

    static Fin<AssemblyPlan> Ordered(Seq<AssemblyMember> input, Seq<AssemblyJoint> joints, AssemblyPolicy policy) =>
        policy.DatumJoints.Exists(index => !joints.Exists(joint => joint.Index == index))
            ? Fin.Fail<AssemblyPlan>(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Membership(policy.DatumJoints.Find(index => !joints.Exists(joint => joint.Index == index)).IfNone(-1),
                    policy.DatumJoints.Count, joints.Count)).ToError())
            : joints.Traverse(joint => Receipt(joint, joints, input, policy).ToValidation()).As().ToFin().Bind(receipts => {
            Dictionary<int, AssemblyJoint> keyedJoints = joints.ToDictionary(static joint => joint.Index);
            (BidirectionalGraph<JoinNode, AssemblyEdge> graph, Seq<BlockedCorridor> blocked) = Graph(joints, keyedJoints, policy);
            if (!graph.IsDirectedAcyclicGraph()) {
                Dictionary<JoinNode, int> labels = new();
                graph.StronglyConnectedComponents(labels);
                Set<JoinNode> cyclic = labels.GroupBy(static pair => pair.Value).Filter(static group => group.Count() > 1)
                    .Bind(static group => group.Map(static pair => pair.Key)).ToSet();
                return Fin.Fail<AssemblyPlan>(FabricationFault.AssemblyPrecedenceCyclic(
                    cyclic.Map(static node => node.Joint).Distinct().Count(),
                    graph.Edges.Count(edge => cyclic.Contains(edge.Source) && cyclic.Contains(edge.Target))).ToError());
            }
            (Dictionary<AssemblyMemberKey, int> components, int count) = Physical(input, joints);
            Seq<JoinNode> order = graph.SourceFirstBidirectionalTopologicalSort().ToSeq();
            HashMap<int, JoinReceipt> keyed = receipts.Fold(HashMap<int, JoinReceipt>(), static (index, receipt) => index.Add(receipt.Joint, receipt));
            Seq<JoinStep> steps = Schedule(graph, order, joints, keyedJoints, keyed, components, policy);
            BidirectionalGraph<JoinNode, SEdge<JoinNode>> simple = new(allowParallelEdges: false);
            simple.AddVertexRange(graph.Vertices);
            graph.Edges.Iter(edge => {
                if (!simple.ContainsEdge(edge.Source, edge.Target)) simple.AddEdge(new SEdge<JoinNode>(edge.Source, edge.Target));
            });
            Seq<AssemblyEdge> reduced = simple.ComputeTransitiveReduction(static (source, target) => new SEdge<JoinNode>(source, target)).Edges.ToSeq()
                .Bind(edge => graph.Edges.Filter(reason => reason.Source == edge.Source && reason.Target == edge.Target)).Distinct().ToSeq();
            Seq<JoinStep> service = Service(graph, order, joints, keyedJoints, keyed, components, policy.Execution);
            ContentKey key = ContentKey.Of(EgressKind.Plan, Canonical(
                input, policy.Execution, steps, count, reduced, joints, receipts, blocked, service));
            return Fin.Succ(new AssemblyPlan(input, policy.Execution, steps, count, reduced, joints, receipts, blocked, service, key));
        });

    static (BidirectionalGraph<JoinNode, AssemblyEdge> Graph, Seq<BlockedCorridor> Blocked) Graph(
        Seq<AssemblyJoint> joints,
        Dictionary<int, AssemblyJoint> keyedJoints,
        AssemblyPolicy policy) {
        BidirectionalGraph<JoinNode, AssemblyEdge> graph = new(allowParallelEdges: true);
        foreach (AssemblyJoint joint in joints) {
            Seq<JoinPhase> phases = Phases(joint, joints, policy.Execution, JoinProgram.Assembly);
            graph.AddVertexRange(phases.Map(phase => new JoinNode(joint.Index, phase)));
            phases.Zip(phases.Tail).Iter(pair => graph.AddEdge(new AssemblyEdge(
                new JoinNode(joint.Index, pair.First), new JoinNode(joint.Index, pair.Second), pair.Second.Entered)));
        }
        Set<int> datum = policy.DatumJoints.ToSet();
        policy.DatumJoints.Iter(anchor => joints.Filter(joint => !datum.Contains(joint.Index)).Iter(joint =>
            graph.AddEdge(new AssemblyEdge(Last(keyedJoints[anchor]), First(joint, joints, policy), PrecedenceKind.Datum))));
        joints.Filter(static joint => !joint.Specification.Process.Reversible).Iter(fixedJoint =>
            joints.Filter(joint => joint.Specification.Process.Reversible && Shares(fixedJoint, joint)).Iter(service =>
                graph.AddEdge(new AssemblyEdge(Last(fixedJoint), First(service, joints, policy), PrecedenceKind.Service))));
        // Thermal ordering is by deposited energy, not a hot-before-cold binary: two hot joints on
        // shared members order highest-energy first so each later joint sees the distortion the
        // earlier one already imposed, and a cold joint follows every hot neighbour.
        joints.Iter(hot => joints
            .Filter(joint => joint.Index != hot.Index && Shares(hot, joint)
                && hot.Specification.Process.ThermalLoad > joint.Specification.Process.ThermalLoad)
            .Iter(cold => graph.AddEdge(new AssemblyEdge(Last(hot), First(cold, joints, policy), PrecedenceKind.Thermal))));
        Seq<BlockedCorridor> blocked = joints.Bind(before => before.Specification.Access.Map((access, index) => (before, access, index)))
            .Bind(row => joints.Filter(after => after.Index != row.before.Index && Occludes(row.before.At, row.access,
                    row.before.Specification.GrooveIncludedAngle, after.At, policy.CorridorClearance))
                .Map(after => new BlockedCorridor(row.before.Index, row.index, after.Index)));
        blocked.Iter(row => graph.AddEdge(new AssemblyEdge(
            Last(keyedJoints[row.Joint]),
            First(keyedJoints[row.Occluder], joints, policy),
            PrecedenceKind.Occlusion)));
        return (graph, blocked);
    }

    static Seq<JoinPhase> Phases(
        AssemblyJoint joint,
        Seq<AssemblyJoint> joints,
        AssemblyExecution execution,
        JoinProgram program) {
        int ordinal = Ordinal(joints, joint.Index);
        return joint.Specification.Process.Phases(program, execution, ordinal, ordinal == joints.Count - 1);
    }

    static int Ordinal(Seq<AssemblyJoint> joints, int identity) => joints
        .Map((joint, index) => joint.Index == identity ? Some(index) : None)
        .Choose(static index => index)
        .Head
        .IfNone(0);

    static Fin<JoinReceipt> Receipt(AssemblyJoint joint, Seq<AssemblyJoint> joints, Seq<AssemblyMember> members, AssemblyPolicy policy) {
        Duration duration = Phases(joint, joints, policy.Execution, JoinProgram.Assembly)
            .Map(phase => joint.Specification.DurationOf(JoinProgram.Assembly, phase))
            .Fold(Duration.Zero, static (total, value) => total + value);
        Seq<Fixture> fixtures = joint.Specification.Fixtures.Choose(policy.Fixtures.ByOperation.Find);
        return (policy.Evidence.Evaluate(joint,
                    members.Filter(member => joint.Specification.Components.Contains(member.Key)), fixtures, policy).ToValidation(),
                fixtures.Traverse(fixture => joint.Specification.Access.Traverse(access =>
                Workholding.Apply(new WorkholdingOp.Clear(fixture, FixtureState.Clamp, Corridor(joint.At, access, joint.Specification.GrooveIncludedAngle, policy.CorridorClearance)))
                    .Bind(static result => result switch {
                        WorkholdingResult.Clearance receipt => Fin.Succ(receipt),
                        _ => Fin.Fail<WorkholdingResult.Clearance>(new FabricationFault.WitnessMalformed(nameof(WorkholdingResult.Clearance), nameof(ExclusionZone)).ToError()),
                    }).ToValidation()).Map(static rows => rows).ToValidation()).As())
            .Apply(static (boundary, rows) => (boundary, Clearance: rows.Bind(identity)))
            .As().ToFin()
            .Bind(result => Fits(result.boundary.Tolerance, joint.Specification.Fit)
                && result.boundary.Stability.Stable
                && result.boundary.Stability.Components == joint.Specification.Components.Count
                && (fixtures.IsEmpty || result.boundary.Stability.FixtureHeld)
                && result.boundary.Robot.ForAll(static receipt => receipt.Selected.Failures == 0)
                && result.boundary.Visibility.Count == joint.Specification.Access.Count
                && joint.Specification.Access.Map((access, index) => !access.LineOfSight || result.boundary.Visibility[index]).ForAll(identity)
                && result.Clearance.ForAll(static receipt => receipt.Clear)
                ? Fin.Succ(new JoinReceipt(joint.Index, result.boundary.Tolerance, result.Clearance,
                    result.boundary.Stability, result.boundary.Robot, result.boundary.Visibility, joint.Specification.Resources, duration))
                : Fin.Fail<JoinReceipt>(new FabricationFault.FixtureInadmissible(new FixturingWitness.Join(
                    joint.Index, Rejection(result.boundary, result.Clearance, joint, fixtures))).ToError()));
    }

    static Seq<JoinStep> Schedule(
        BidirectionalGraph<JoinNode, AssemblyEdge> graph,
        Seq<JoinNode> order,
        Seq<AssemblyJoint> joints,
        Dictionary<int, AssemblyJoint> keyedJoints,
        HashMap<int, JoinReceipt> receipts,
        Dictionary<AssemblyMemberKey, int> components,
        AssemblyPolicy policy) =>
        order.Fold(
            (Steps: Seq<JoinStep>(), Active: HashMap<string, double>(), Finished: HashMap<JoinNode, double>(),
             Lanes: toSeq(Enumerable.Repeat(0.0, policy.Execution.Lanes(order.Count))).ToArr()),
            (state, node) => {
                AssemblyJoint joint = keyedJoints[node.Joint];
                StabilityReceipt stability = receipts.Find(node.Joint)
                    .Map(static receipt => receipt.Stability).IfNone(default(StabilityReceipt));
                int lane = toSeq(Enumerable.Range(0, state.Lanes.Count)).OrderBy(index => state.Lanes[index]).Head.IfNone(0);
                double predecessor = graph.InEdges(node).Map(edge => state.Finished.Find(edge.Source).IfNone(0.0)).Fold(0.0, Math.Max);
                double ready = joint.Specification.Resources.Map(resource => state.Active.Find(resource).IfNone(0.0))
                    .Fold(Math.Max(state.Lanes[lane], predecessor), Math.Max);
                double duration = joint.Specification.DurationOf(JoinProgram.Assembly, node.Phase).As(DurationUnit.Second);
                double finish = ready + duration;
                HashMap<string, double> active = joint.Specification.Resources.Fold(state.Active, (held, resource) => held.AddOrUpdate(resource, finish));
                JoinStep step = new(state.Steps.Count, node.Joint, node.Phase, components[joint.Specification.Components[0]],
                    joint.Specification.Fixtures.Head, joint.Specification.Resources,
                    Duration.FromSeconds(ready), Duration.FromSeconds(finish), stability);
                return (state.Steps.Add(step), active, state.Finished.AddOrUpdate(node, finish), state.Lanes.SetItem(lane, finish));
            }).Steps;

    static (Dictionary<AssemblyMemberKey, int> Labels, int Count) Physical(Seq<AssemblyMember> input, Seq<AssemblyJoint> joints) {
        UndirectedGraph<AssemblyMemberKey, SEdge<AssemblyMemberKey>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(input.Map(static member => member.Key));
        joints.Iter(joint => joint.Specification.Components.Tail.Iter(component =>
            graph.AddEdge(new SEdge<AssemblyMemberKey>(joint.Specification.Components[0], component))));
        Dictionary<AssemblyMemberKey, int> labels = new();
        return (labels, graph.ConnectedComponents(labels));
    }

    // Disassembly reverses the precedence the plan proved, not the joint roster: a joint whose
    // access an occlusion or thermal edge gated comes apart after the joint that gated it, and a
    // roster order ignores every one of those edges.
    static Seq<JoinStep> Service(
        BidirectionalGraph<JoinNode, AssemblyEdge> graph,
        Seq<JoinNode> order,
        Seq<AssemblyJoint> joints,
        Dictionary<int, AssemblyJoint> keyedJoints,
        HashMap<int, JoinReceipt> receipts,
        Dictionary<AssemblyMemberKey, int> components,
        AssemblyExecution execution) =>
        order.Reverse().Map(static node => node.Joint).Distinct().ToSeq()
            .Fold((Steps: Seq<JoinStep>(), Elapsed: Duration.Zero), (state, index) => {
                AssemblyJoint joint = keyedJoints[index];
                return Phases(joint, joints, execution, JoinProgram.Service).Fold(state, (held, phase) => {
                    Duration finish = held.Elapsed + joint.Specification.DurationOf(JoinProgram.Service, phase);
                    JoinStep step = new(held.Steps.Count, joint.Index, phase, components[joint.Specification.Components[0]],
                        joint.Specification.Fixtures.Head, joint.Specification.Resources, held.Elapsed, finish,
                        receipts.Find(joint.Index).Map(static receipt => receipt.Stability).IfNone(default(StabilityReceipt)));
                    return (held.Steps.Add(step), finish);
                });
            }).Steps;

    static bool Occludes(Edge3 joint, AccessCorridor access, Option<Angle> groove, Edge3 obstacle, Length clearance) {
        Point3d origin = Mid(joint);
        Vector3d axis = access.Axis;
        axis.Unitize();
        double standoff = access.Standoff.As(LengthUnit.Millimeter);
        double halfAngle = access.HalfAngle.As(AngleUnit.Radian);
        double clearanceMm = clearance.As(LengthUnit.Millimeter);
        Vector3d span = obstacle.B - obstacle.A;
        Vector3d at = obstacle.A - origin;
        double axial0 = at * axis;
        double axialRate = span * axis;
        double approach = access.Approach.As(LengthUnit.Millimeter);
        double retract = access.Retract.As(LengthUnit.Millimeter);
        (double lower, double upper) = AxialInterval(axial0, axialRate, -approach, standoff + retract);
        if (lower > upper) return false;
        Vector3d radial0 = at - (axial0 * axis);
        Vector3d radialRate = span - (axialRate * axis);
        double slope = Math.Tan(groove.Map(angle => Math.Min(halfAngle, 0.5 * angle.As(AngleUnit.Radian))).IfNone(halfAngle));
        double basis = Math.Max(access.ToolRadius.As(LengthUnit.Millimeter), access.HolderRadius.As(LengthUnit.Millimeter)) + clearanceMm;
        Seq<double> bounds = Seq(lower, upper)
            + Crossing(axial0, axialRate, 0.0).Filter(value => value > lower && value < upper)
            + Crossing(axial0, axialRate, standoff).Filter(value => value > lower && value < upper);
        Seq<double> ordered = bounds.Distinct().OrderBy(identity).ToSeq();
        return ordered.Zip(ordered.Tail).Exists(interval => {
            double middle = 0.5 * (interval.First + interval.Second);
            double axial = axial0 + (axialRate * middle);
            double radius0 = axial <= 0.0 ? basis : axial >= standoff ? basis + (slope * standoff) : basis + (slope * axial);
            double radiusRate = axial is > 0.0 && axial < standoff ? slope * axialRate : 0.0;
            double a = (radialRate * radialRate) - (radiusRate * radiusRate);
            double b = 2.0 * ((radial0 * radialRate) - (radius0 * radiusRate));
            double c = (radial0 * radial0) - (radius0 * radius0);
            double stationary = Math.Abs(a) > 1e-12 ? Math.Clamp(-b / (2.0 * a), interval.First, interval.Second) : interval.First;
            return Seq(interval.First, stationary, interval.Second).Map(parameter => (a * parameter * parameter) + (b * parameter) + c).Min() <= 0.0;
        });
    }

    static ToolCorridor Corridor(Edge3 joint, AccessCorridor access, Option<Angle> groove, Length clearance) {
        Point3d origin = Mid(joint);
        Vector3d axis = access.Axis;
        axis.Unitize();
        double angle = groove.Map(value => Math.Min(access.HalfAngle.As(AngleUnit.Radian), 0.5 * value.As(AngleUnit.Radian)))
            .IfNone(access.HalfAngle.As(AngleUnit.Radian));
        double standoff = access.Standoff.As(LengthUnit.Millimeter);
        double tool = access.ToolRadius.As(LengthUnit.Millimeter);
        double holder = access.HolderRadius.As(LengthUnit.Millimeter);
        double approach = access.Approach.As(LengthUnit.Millimeter);
        double retract = access.Retract.As(LengthUnit.Millimeter);
        double clearanceMm = clearance.As(LengthUnit.Millimeter);
        return new ToolCorridor(CorridorKind.Tool, Seq(
            new CorridorStation(origin - (approach * axis), Length.FromMillimeters(tool + clearanceMm), Length.FromMillimeters(holder + clearanceMm),
                new Length(0.0, LengthUnit.Millimeter), new Length(0.0, LengthUnit.Millimeter)),
            new CorridorStation(origin, Length.FromMillimeters(tool + clearanceMm), Length.FromMillimeters(holder + clearanceMm),
                new Length(0.0, LengthUnit.Millimeter), new Length(0.0, LengthUnit.Millimeter)),
            new CorridorStation(origin + (standoff * axis), Length.FromMillimeters(tool + clearanceMm),
                Length.FromMillimeters(holder + clearanceMm + (Math.Tan(angle) * standoff)),
                new Length(0.0, LengthUnit.Millimeter), new Length(0.0, LengthUnit.Millimeter)),
            new CorridorStation(origin + ((standoff + retract) * axis), Length.FromMillimeters(tool + clearanceMm),
                Length.FromMillimeters(holder + clearanceMm), new Length(0.0, LengthUnit.Millimeter), new Length(0.0, LengthUnit.Millimeter))));
    }

    static (double Lower, double Upper) AxialInterval(double at, double rate, double minimum, double maximum) {
        if (Math.Abs(rate) < 1e-12) return at >= minimum && at <= maximum ? (0.0, 1.0) : (1.0, 0.0);
        double first = (minimum - at) / rate;
        double second = (maximum - at) / rate;
        return (Math.Max(0.0, Math.Min(first, second)), Math.Min(1.0, Math.Max(first, second)));
    }

    static Seq<double> Crossing(double at, double rate, double target) =>
        Math.Abs(rate) < 1e-12 ? Seq<double>() : Seq((target - at) / rate);

    // Removing a joint changes the load path of every joint that remains, so the surviving
    // stability receipts are re-proven against the residual assembly through the same evidence
    // boundary; carrying the original receipts forward publishes stability the plan no longer has.
    static Fin<AssemblyPlan> Replan(AssemblyPlan plan, AssemblyPolicy policy, Seq<int> completed, Seq<int> blocked) {
        if (completed.Distinct().Count != completed.Count || blocked.Distinct().Count != blocked.Count
            || completed.Exists(blocked.Contains)
            || completed.Concat(blocked).Exists(index => !plan.Joints.Exists(joint => joint.Index == index)))
            return Fin.Fail<AssemblyPlan>(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Residual(completed.Count, blocked.Count, plan.Joints.Count)).ToError());
        Set<int> removed = completed.Concat(blocked).ToSet();
        Seq<AssemblyJoint> residualJoints = plan.Joints.Filter(joint => !removed.Contains(joint.Index));
        Seq<AssemblyMember> members = plan.Members.Filter(member =>
            residualJoints.Exists(joint => joint.Specification.Components.Contains(member.Key)));
        return Ordered(members, residualJoints, policy);
    }

    static Fin<Seq<JoinStep>> Disassemble(AssemblyPlan? plan, Seq<int> targets) =>
        plan is not null && targets.Count > 0 && targets.Distinct().Count == targets.Count
            && targets.ForAll(identity => plan.Joints.Find(joint => joint.Index == identity)
                .Exists(static joint => joint.Specification.Process.Reversible))
            ? Fin.Succ(plan.ServiceOrder.Filter(step => targets.Contains(step.Joint)).ToSeq())
            : Fin.Fail<Seq<JoinStep>>(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Residual(0, targets.Count, plan?.Joints.Count ?? 0)).ToError());

    static JoinNode First(AssemblyJoint joint, Seq<AssemblyJoint> joints, AssemblyPolicy policy) =>
        new(joint.Index, Phases(joint, joints, policy.Execution, JoinProgram.Assembly).Head.IfNone(JoinPhase.Final));
    static JoinNode Last(AssemblyJoint joint) => new(joint.Index, JoinPhase.Final);
    static JoinRejection Rejection(
        AssemblyBoundaryEvidence boundary,
        Seq<WorkholdingResult.Clearance> clearance,
        AssemblyJoint joint,
        Seq<Fixture> fixtures) =>
        !Fits(boundary.Tolerance, joint.Specification.Fit) ? JoinRejection.Fit
        : !boundary.Stability.Stable ? JoinRejection.Stability
        : boundary.Stability.Components != joint.Specification.Components.Count ? JoinRejection.Components
        : !fixtures.IsEmpty && !boundary.Stability.FixtureHeld ? JoinRejection.Custody
        : boundary.Robot.Exists(static receipt => receipt.Selected.Failures != 0) ? JoinRejection.Robot
        : boundary.Visibility.Count != joint.Specification.Access.Count ? JoinRejection.Visibility
        : joint.Specification.Access.Map((access, index) => access.LineOfSight && !boundary.Visibility[index]).Exists(identity) ? JoinRejection.Sight
        : JoinRejection.Access;
    static bool Shares(AssemblyJoint left, AssemblyJoint right) => left.Specification.Components.Exists(right.Specification.Components.Contains);
    static bool Fits(ToleranceReceipt receipt, FitRequirement requirement) =>
        receipt.Gap >= requirement.GapMin && receipt.Gap <= requirement.GapMax
        && receipt.Interference <= requirement.InterferenceMax
        && receipt.Alignment <= requirement.AlignmentMax
        && receipt.Closure <= requirement.ClosureMax
        && receipt.SurfaceRoughness <= requirement.SurfaceRoughnessMax
        && receipt.Temperature >= requirement.TemperatureMin
        && receipt.Temperature <= requirement.TemperatureMax;
    static Point3d Mid(Edge3 at) => at.A + (0.5 * (at.B - at.A));
    static bool Valid(AccessCorridor access) => Finite(access.Axis) && access.Axis.Length > 1e-9
        && double.IsFinite(access.HalfAngle.As(AngleUnit.Radian))
        && access.HalfAngle.As(AngleUnit.Radian) is > 0.0 and < Math.PI / 2.0
        && double.IsFinite(access.Standoff.As(LengthUnit.Millimeter))
        && double.IsFinite(access.ToolRadius.As(LengthUnit.Millimeter))
        && double.IsFinite(access.HolderRadius.As(LengthUnit.Millimeter))
        && double.IsFinite(access.Approach.As(LengthUnit.Millimeter))
        && double.IsFinite(access.Retract.As(LengthUnit.Millimeter))
        && access.Standoff.As(LengthUnit.Millimeter) > 0.0
        && access.ToolRadius.As(LengthUnit.Millimeter) >= 0.0
        && access.HolderRadius.As(LengthUnit.Millimeter) >= 0.0
        && access.Approach.As(LengthUnit.Millimeter) >= 0.0
        && access.Retract.As(LengthUnit.Millimeter) >= 0.0;
    static bool Finite(Vector3d value) => double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);

    static byte[] Canonical(
        Seq<AssemblyMember> members,
        AssemblyExecution execution,
        Seq<JoinStep> steps,
        int subassemblies,
        Seq<AssemblyEdge> precedence,
        Seq<AssemblyJoint> joints,
        Seq<JoinReceipt> receipts,
        Seq<BlockedCorridor> blocked,
        Seq<JoinStep> service) {
        using ArrayPoolBufferWriter<byte> buffer = new();
        Write(buffer, execution.Cadence.Key); Write(buffer, execution.MaxParallel);
        Write(buffer, subassemblies);
        Frame(buffer, members, static (held, member) => {
            Write(held, member.Key.Representation); Write(held, member.Key.Instance); WriteTransform(held, member.Pose);
        });
        Frame(buffer, joints, static (buffer, joint) => {
            Write(buffer, joint.Index); Write(buffer, joint.Owner.Representation); Write(buffer, joint.Owner.Instance);
            Write(buffer, joint.Connection.DetailKey); Write(buffer, joint.Connection.RealizingKey);
            _ = joint.Connection.At.Match(
                Some: at => { Write(buffer, 1); WriteEdge(buffer, at); return unit; },
                None: () => { Write(buffer, 0); return unit; });
            Write(buffer, joint.At.A.X); Write(buffer, joint.At.A.Y); Write(buffer, joint.At.A.Z);
            Write(buffer, joint.At.B.X); Write(buffer, joint.At.B.Y); Write(buffer, joint.At.B.Z);
            WriteProcess(buffer, joint.Specification.Process);
            Write(buffer, joint.Specification.Fit.GapMin.As(LengthUnit.Millimeter));
            Write(buffer, joint.Specification.Fit.GapMax.As(LengthUnit.Millimeter));
            Write(buffer, joint.Specification.Fit.InterferenceMax.As(LengthUnit.Millimeter));
            Write(buffer, joint.Specification.Fit.AlignmentMax.As(LengthUnit.Millimeter));
            Write(buffer, joint.Specification.Fit.ClosureMax.As(LengthUnit.Millimeter));
            Write(buffer, joint.Specification.Fit.SurfaceRoughnessMax.As(LengthUnit.Millimeter));
            Write(buffer, joint.Specification.Fit.TemperatureMin.As(TemperatureUnit.DegreeCelsius));
            Write(buffer, joint.Specification.Fit.TemperatureMax.As(TemperatureUnit.DegreeCelsius));
            Write(buffer, joint.Specification.Capacity.As(ForceUnit.Newton));
            Write(buffer, joint.Specification.ReleaseStrengthFraction);
            Frame(buffer, joint.Specification.Components.ToSeq(), static (held, component) => { Write(held, component.Representation); Write(held, component.Instance); });
            Frame(buffer, joint.Specification.Resources, Write);
            Frame(buffer, joint.Specification.Fixtures, Write);
            Frame(buffer, joint.Specification.Access, static (buffer, access) => {
                Write(buffer, access.Axis.X); Write(buffer, access.Axis.Y); Write(buffer, access.Axis.Z);
                Write(buffer, access.HalfAngle.As(AngleUnit.Radian)); Write(buffer, access.Standoff.As(LengthUnit.Millimeter));
                Write(buffer, access.ToolRadius.As(LengthUnit.Millimeter)); Write(buffer, access.HolderRadius.As(LengthUnit.Millimeter));
                Write(buffer, access.Approach.As(LengthUnit.Millimeter)); Write(buffer, access.Retract.As(LengthUnit.Millimeter));
                Write(buffer, access.LineOfSight ? 1 : 0);
            });
            Frame(buffer, joint.Specification.PhaseDurations.ToSeq().Map(static row => row).OrderBy(static row => row.Key.Rank).ToSeq(),
                static (held, row) => { Write(held, row.Key.Rank); Write(held, row.Value.As(DurationUnit.Second)); });
            Frame(buffer, joint.Specification.ServiceDurations.ToSeq().Map(static row => row).OrderBy(static row => row.Key.Rank).ToSeq(),
                static (held, row) => { Write(held, row.Key.Rank); Write(held, row.Value.As(DurationUnit.Second)); });
            _ = joint.Specification.GrooveIncludedAngle.Match(
                Some: angle => { Write(buffer, 1); Write(buffer, angle.As(AngleUnit.Radian)); return unit; },
                None: () => { Write(buffer, 0); return unit; });
        });
        Frame(buffer, steps, static (buffer, step) => {
            Write(buffer, step.Order); Write(buffer, step.Joint); Write(buffer, step.Phase.Rank); Write(buffer, step.Subassembly);
            WriteOption(buffer, step.Fixture, static (held, fixture) => Write(held, fixture));
            Frame(buffer, step.Resources, Write);
            Write(buffer, step.Start.As(DurationUnit.Second)); Write(buffer, step.Finish.As(DurationUnit.Second));
            WriteStability(buffer, step.Stability);
        });
        Frame(buffer, precedence, static (buffer, edge) => {
            Write(buffer, edge.Source.Joint); Write(buffer, edge.Source.Phase.Rank);
            Write(buffer, edge.Target.Joint); Write(buffer, edge.Target.Phase.Rank); Write(buffer, edge.Kind.Code);
        });
        Frame(buffer, blocked, static (buffer, row) => {
            Write(buffer, row.Joint); Write(buffer, row.Corridor); Write(buffer, row.Occluder);
        });
        Frame(buffer, receipts, static (buffer, receipt) => {
            Write(buffer, receipt.Joint); Write(buffer, receipt.Tolerance.Gap.As(LengthUnit.Millimeter));
            Write(buffer, receipt.Tolerance.Interference.As(LengthUnit.Millimeter));
            Write(buffer, receipt.Tolerance.Alignment.As(LengthUnit.Millimeter));
            Write(buffer, receipt.Tolerance.Closure.As(LengthUnit.Millimeter));
            Write(buffer, receipt.Tolerance.SurfaceRoughness.As(LengthUnit.Millimeter));
            Write(buffer, receipt.Tolerance.Temperature.As(TemperatureUnit.DegreeCelsius));
            Frame(buffer, receipt.Clearance, static (held, clearance) => WriteOption(
                held,
                clearance.Blocked,
                static (sink, zone) => Write(sink, zone.Collision.Key.Digest)));
            WriteStability(buffer, receipt.Stability);
            WriteOption(buffer, receipt.Robot, WritePlacementReceipt);
            Frame(buffer, receipt.Visibility, static (held, visible) => Write(held, visible ? 1 : 0));
            Frame(buffer, receipt.Resources, Write);
            Write(buffer, receipt.Duration.As(DurationUnit.Second));
        });
        Frame(buffer, service, static (held, step) => {
            Write(held, step.Order); Write(held, step.Joint); Write(held, step.Phase.Rank); Write(held, step.Subassembly);
            WriteOption(held, step.Fixture, static (sink, fixture) => Write(sink, fixture));
            Frame(held, step.Resources, Write);
            Write(held, step.Start.As(DurationUnit.Second)); Write(held, step.Finish.As(DurationUnit.Second));
            WriteStability(held, step.Stability);
        });
        return buffer.WrittenSpan.ToArray();
    }

    static void WritePlacementReceipt(ArrayPoolBufferWriter<byte> buffer, CellPlacementReceipt receipt) {
        WritePlacement(buffer, receipt.Selected);
        Frame(buffer, receipt.Ranked, WritePlacement);
    }

    static void WritePlacement(ArrayPoolBufferWriter<byte> buffer, CellPlacementCandidate candidate) {
        candidate.Cell.Source.Switch(
            state: buffer,
            library: static (held, source) => { Write(held, 1); Write(held, source.Name); Write(held, source.Meshes.Key); return unit; },
            embedded: static (held, source) => { Write(held, 2); Write(held, source.Xml); return unit; });
        WritePlane(buffer, candidate.Cell.BaseFrame); WritePlane(buffer, candidate.Cell.ToolFrame);
        WritePlane(buffer, candidate.NormalizedBaseFrame);
        Frame(buffer, candidate.Joints, static (held, joints) => Frame(held, joints.ToSeq(), static (sink, value) => Write(sink, value)));
        Frame(buffer, candidate.Metrics.ToSeq().Map(static row => row).OrderBy(static row => row.Key.Key).ToSeq(),
            static (held, metric) => { Write(held, metric.Key.Key); Write(held, metric.Value); });
        Write(buffer, candidate.Score);
    }

    static void WriteStability(ArrayPoolBufferWriter<byte> buffer, StabilityReceipt receipt) {
        Write(buffer, receipt.Components); Write(buffer, receipt.CapacityMargin); Write(buffer, receipt.SupportMargin);
        Write(buffer, receipt.LoadPathMargin); Write(buffer, receipt.FixtureHeld ? 1 : 0);
    }

    static void WritePlane(ArrayPoolBufferWriter<byte> buffer, Plane plane) {
        WritePoint(buffer, plane.Origin); WriteVector(buffer, plane.XAxis); WriteVector(buffer, plane.YAxis); WriteVector(buffer, plane.ZAxis);
    }

    static void WriteEdge(ArrayPoolBufferWriter<byte> buffer, Edge3 edge) {
        WritePoint(buffer, edge.A); WritePoint(buffer, edge.B);
    }

    static void WritePoint(ArrayPoolBufferWriter<byte> buffer, Point3d point) {
        Write(buffer, point.X); Write(buffer, point.Y); Write(buffer, point.Z);
    }

    static void WriteVector(ArrayPoolBufferWriter<byte> buffer, Vector3d vector) {
        Write(buffer, vector.X); Write(buffer, vector.Y); Write(buffer, vector.Z);
    }

    static void WriteTransform(ArrayPoolBufferWriter<byte> buffer, Transform value) {
        WritePoint(buffer, value * new Point3d(0.0, 0.0, 0.0));
        WritePoint(buffer, value * new Point3d(1.0, 0.0, 0.0));
        WritePoint(buffer, value * new Point3d(0.0, 1.0, 0.0));
        WritePoint(buffer, value * new Point3d(0.0, 0.0, 1.0));
    }

    static void WriteOption<T>(ArrayPoolBufferWriter<byte> buffer, Option<T> value, Action<ArrayPoolBufferWriter<byte>, T> write) =>
        value.Match(
            Some: item => { Write(buffer, 1); write(buffer, item); return unit; },
            None: () => { Write(buffer, 0); return unit; });

    static void Frame<T>(ArrayPoolBufferWriter<byte> buffer, Seq<T> rows, Action<ArrayPoolBufferWriter<byte>, T> write) {
        Write(buffer, rows.Count);
        _ = rows.Iter(row => write(buffer, row));
    }


    static Unit WriteProcess(ArrayPoolBufferWriter<byte> buffer, JoinProcess process) => process.Switch(
        state: buffer,
        fusion: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Procedure); Write(held, row.HeatInput.JoulesPerMillimeter);
            Write(held, row.Preheat.As(TemperatureUnit.DegreeCelsius)); Write(held, row.Interpass.As(TemperatureUnit.DegreeCelsius)); Write(held, row.Tackable ? 1 : 0); return unit; },
        brazed: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Filler); Write(held, row.Liquidus.As(TemperatureUnit.DegreeCelsius)); Write(held, row.Dwell.As(DurationUnit.Second)); Write(held, row.DepositedEnergy.As(EnergyUnit.Joule)); return unit; },
        soldered: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Filler); Write(held, row.Liquidus.As(TemperatureUnit.DegreeCelsius)); Write(held, row.Dwell.As(DurationUnit.Second)); Write(held, row.DepositedEnergy.As(EnergyUnit.Joule)); return unit; },
        bonded: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Adhesive); Write(held, row.Bondline.As(LengthUnit.Millimeter)); Write(held, row.Cure.As(DurationUnit.Second)); Write(held, row.ClampPressure.As(PressureUnit.Pascal)); return unit; },
        threaded: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Fastener); Write(held, row.Torque.As(TorqueUnit.NewtonMeter)); Write(held, row.Preload.As(ForceUnit.Newton)); Write(held, row.Locking); Write(held, row.Screw ? 1 : 0); return unit; },
        riveted: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Rivet); Write(held, row.UpsetForce.As(ForceUnit.Newton)); Write(held, row.HeadHeight.As(LengthUnit.Millimeter)); return unit; },
        studded: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Stud); Write(held, row.InstallationForce.As(ForceUnit.Newton)); WriteOption(held, row.ArcEnergy, static (sink, energy) => Write(sink, energy.As(EnergyUnit.Joule))); return unit; },
        interference: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Interference.As(LengthUnit.Millimeter)); Write(held, row.InsertionForce.As(ForceUnit.Newton)); Write(held, row.TemperatureDelta.As(TemperatureDeltaUnit.DegreeCelsius)); WriteOption(held, row.ConditioningEnergy, static (sink, energy) => Write(sink, energy.As(EnergyUnit.Joule))); return unit; },
        clinched: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Tool); Write(held, row.Force.As(ForceUnit.Newton)); Write(held, row.Button.As(LengthUnit.Millimeter)); return unit; },
        pinned: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Pin); Write(held, row.InsertionForce.As(ForceUnit.Newton)); Write(held, row.Removable ? 1 : 0); return unit; },
        snapped: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Feature); Write(held, row.Engagement.As(ForceUnit.Newton)); Write(held, row.Release.As(ForceUnit.Newton)); return unit; },
        connector: static (held, row) => { Write(held, row.Class.Code); Write(held, row.Key); Write(held, row.MatingForce.As(ForceUnit.Newton)); Write(held, row.Latching ? 1 : 0); return unit; });


    static void Write(ArrayPoolBufferWriter<byte> buffer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(sizeof(int)), value);
        buffer.Advance(sizeof(int));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, UInt128 value) {
        BinaryPrimitives.WriteUInt64LittleEndian(buffer.GetSpan(sizeof(ulong)), (ulong)(value >> 64));
        buffer.Advance(sizeof(ulong));
        BinaryPrimitives.WriteUInt64LittleEndian(buffer.GetSpan(sizeof(ulong)), (ulong)value);
        buffer.Advance(sizeof(ulong));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, double value) {
        BinaryPrimitives.WriteInt64LittleEndian(buffer.GetSpan(sizeof(long)), BitConverter.DoubleToInt64Bits(value));
        buffer.Advance(sizeof(long));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, string value) {
        int length = Encoding.UTF8.GetByteCount(value);
        Write(buffer, length);
        Encoding.UTF8.GetBytes(value, buffer.GetSpan(length));
        buffer.Advance(length);
    }
}
```

## [04]-[PROJECTION]

- Owner: `AssemblyProjection` selects joining, traveler, inspection, handling, service, and evidence views; `AssemblyArtifact` carries the selected immutable plan.
- Egress: projection preserves typed precedence, execution receipts, service order, and the already-minted plan key.
- Boundary: joining consumes joint and phase identity, handling consumes stability and subassembly identity, and service consumes only reversible steps; no consumer reconstructs those facts from prose or array order.

```csharp signature
// --- [PROJECTION] ---------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class AssemblyProjection {
    public static readonly AssemblyProjection Joining = new("joining");
    public static readonly AssemblyProjection Traveler = new("traveler");
    public static readonly AssemblyProjection Inspection = new("inspection");
    public static readonly AssemblyProjection Handling = new("handling");
    public static readonly AssemblyProjection Service = new("service");
    public static readonly AssemblyProjection Evidence = new("evidence");
}

[Union]
public abstract partial record AssemblyArtifact {
    private AssemblyArtifact() { }

    public sealed record Joining(ContentKey Key, Seq<AssemblyJoint> Joints, Seq<JoinStep> Steps, Seq<AssemblyEdge> Precedence) : AssemblyArtifact;
    public sealed record Traveler(ContentKey Key, Seq<JoinStep> Steps, Seq<JoinReceipt> Receipts) : AssemblyArtifact;
    public sealed record Inspection(ContentKey Key, Seq<JoinReceipt> Receipts) : AssemblyArtifact;
    public sealed record Handling(ContentKey Key, Seq<JoinStep> Steps) : AssemblyArtifact;
    public sealed record Service(ContentKey Key, Seq<JoinStep> Steps) : AssemblyArtifact;
    public sealed record Evidence(ContentKey Key, AssemblyPlan Plan) : AssemblyArtifact;
}

public sealed partial record AssemblyPlan {
    static Fin<AssemblyArtifact> Project(AssemblyPlan? plan, AssemblyProjection? projection) =>
        (Optional(plan).ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Absent()).ToError()),
         Optional(projection).ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Absent()).ToError()))
            .Apply((accepted, kind) => kind.Switch<AssemblyArtifact>(
                joining: () => new AssemblyArtifact.Joining(accepted.Key, accepted.Joints, accepted.Steps, accepted.Precedence),
                traveler: () => new AssemblyArtifact.Traveler(accepted.Key, accepted.Steps, accepted.Receipts),
                inspection: () => new AssemblyArtifact.Inspection(accepted.Key, accepted.Receipts),
                handling: () => new AssemblyArtifact.Handling(accepted.Key, accepted.Steps.Filter(static step => step.Phase == JoinPhase.Handle)),
                service: () => new AssemblyArtifact.Service(accepted.Key, accepted.ServiceOrder),
                evidence: () => new AssemblyArtifact.Evidence(accepted.Key, accepted)))
            .As();
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
