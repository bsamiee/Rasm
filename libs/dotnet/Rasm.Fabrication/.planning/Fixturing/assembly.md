# [RASM_FABRICATION_ASSEMBLY]

`AssemblyPlan` owns member admission, join admission, fit-up, precedence, access, load-path stability, temporary support, tolerance closure, joining resources, robot-cell placement, inspection, release, handling, and disassembly evidence. `AssemblyPolicy` admits every component instance, connection specification, datum, fixture, resource, execution policy, and clearance once; the planner consumes typed joints and graph facts only.

`JoinMethod` is the ONE joining-mechanism row table: join class, phase shape, acting phase, and the trait, text, and metric ROSTERS are columns, so seventeen mechanisms share one `JoinProcess` shape and every projection over a process is one expression rather than a parallel twelve-arm fold. `AssemblyJoint.Index` remains the connection-census identity shared with joining plans, `AssemblyPlan.Apply` closes admission, planning, replanning, disassembly, and projection over `AssemblyOp`, and the plan key mints through the `Process/owner#RUN_DISPATCH` `FabricationCanon.Keyed` close, so its refusal rides the same rail the plan does. Scalar admission is `workholding#EVALUATION` `Fixtures`, cyclic component evidence is `setups#SCHEDULE` `Setups.Components`, the tolerance chain consumes the ONE `Joining/sequence` `DistortionField` through `workholding#FIXTURE` `DatumTransfer`, and every preimage frames through `FabricationCanon` over the one `Rasm.Element` `CanonicalWriter`.

## [01]-[INDEX]

- [02]-[JOINS]: the join-method row table, phases, access, fit, resources, and inspection cadence.
- [03]-[PLANNING]: typed precedence, physical connectivity, stability, clearance, scheduling, and results.
- [04]-[PROJECTION]: joining, traveler, inspection, handling, service, and evidence egress.

## [02]-[JOINS]

- Owner: `JoinMethod` owns every joining mechanism as one row carrying its `JoinClass`, `PhaseShape`, acting phase, and its `JoinTrait`, `JoinText`, and `JoinMetric` rosters; `JoinProcess` is the one admitted occurrence carrying that row beside its keyed text and metric streams; `JoinOrdinal` owns a join's position in its own census.
- Owner: `AssemblyExecution` carries `InspectionCadence` and a positive lane ceiling, and `Lanes` bounds allocation by executable demand; named production presets never become domain cases and cadence is never a boolean. `AccessCorridor` carries approach axis, typed cone angle, the cutter and holder swept envelope, standoff, approach, retract, and visibility constraints, and `FitRequirement` carries gap, interference, alignment, surface, temperature, and closure limits as one admitted value.
- Cases: `JoinPhase` covers locate, fit, tack, preheat, apply, dwell, cool, torque, inspect, release, unlock, extract, clean, handle, and final states, each row carrying the `PrecedenceKind` that entering it satisfies.
- Law: a mechanism differs from its siblings in COLUMN VALUES alone. A payload flag that flips behaviour is a ROW, never a boolean — a tacked fusion, an arc-drawn stud, a shrink-fitted interference, and a removable pin are each their own row, so `Reversible` and `Thermal` read the trait roster instead of sniffing an optional energy payload.
- Law: custody traits ride ONE `Set<JoinTrait>` column beside the text and metric rosters this table already carries, so a declaration names the traits it holds instead of position-counting three booleans, and a fourth trait is one row plus the entries that hold it — no row, admission clause, or consumer signature widens.
- Law: service phases DERIVE from the trait roster — a non-reversible mechanism has none, a reversible thermal one unlocks through preheat, and a reversible cold one unlocks directly — so no second twelve-arm fold states what two roster entries already decide.
- Law: inspection is decided at ONE place from a cadence and a `JoinOrdinal`, and no boolean crosses a public phase signature. `Maximal` is the widest run either program admits and is what a duration table must cover; the ordinal carries its own census size, so the last-join test derives once instead of being spelled at each cadence read.
- Law: `JoinClass`, `PrecedenceKind`, and `JoinPhase` carry their own wire `Code` or `Rank`, so canonical bytes read the declaration and a new row cannot inherit its predecessor's code through a trailing ladder arm.
- Law: `JoinSpecification.Durations` is ONE map keyed by program AND phase — a parallel assembly-and-service pair forced every reader to select the map before selecting the row, and a phase present in one and absent in the other silently answered zero.
- Law: `JoinMetric.DepositedEnergy` is the ONE thermal-load quantity, so distortion ordering ranks hot joints against each other by energy instead of collapsing to a hot-before-cold binary.
- Growth: a new join mechanism is one `JoinMethod` row and, where its scalar or identifier is new, one `JoinMetric` or `JoinText` row; phase, edge, scheduler, preimage, and consumer surfaces stay unchanged.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Fixturing;

// --- [TYPES] ---------------------------------------------------------------------------
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

public readonly record struct JoinOrdinal(int Index, int Count) {
    public bool Last => Index == Count - 1;
}

[SmartEnum<string>]
public sealed partial class InspectionCadence {
    public static readonly InspectionCadence Never = new("never", static _ => false);
    public static readonly InspectionCadence EveryJoin = new("every-join", static _ => true);
    public static readonly InspectionCadence EveryOther = new("every-other", static at => at.Index % 2 == 1);
    public static readonly InspectionCadence EveryFifth = new("every-fifth", static at => at.Index % 5 == 4);
    public static readonly InspectionCadence SubassemblyClose = new("subassembly-close", static at => at.Last);

    public Func<JoinOrdinal, bool> Applies { get; }
}

[SmartEnum<string>]
public sealed partial class PhaseShape {
    public static readonly PhaseShape Fusion = new("fusion",
        static action => Seq(JoinPhase.Preheat, action, JoinPhase.Cool));
    public static readonly PhaseShape Heat = new("heat",
        static action => Seq(JoinPhase.Preheat, action, JoinPhase.Dwell, JoinPhase.Cool));
    public static readonly PhaseShape Cure = new("cure",
        static action => Seq(action, JoinPhase.Dwell));
    public static readonly PhaseShape Mechanical = new("mechanical",
        static action => Seq(action));

    public Func<JoinPhase, Seq<JoinPhase>> Core { get; }
}

[SmartEnum<string>]
public sealed partial class JoinTrait {
    public static readonly JoinTrait Thermal = new("thermal");
    public static readonly JoinTrait Reversible = new("reversible");
    public static readonly JoinTrait Tack = new("tack");
}

[SmartEnum<string>]
public sealed partial class JoinText {
    public static readonly JoinText Agent = new("agent");
    public static readonly JoinText Locking = new("locking");
}

[SmartEnum<string>]
public sealed partial class JoinMetric {
    public static readonly JoinMetric HeatInput = new("heat-input", MetricBound.Positive);
    public static readonly JoinMetric DepositedEnergy = new("deposited-energy", MetricBound.Positive);
    public static readonly JoinMetric Preheat = new("preheat", MetricBound.Finite);
    public static readonly JoinMetric Interpass = new("interpass", MetricBound.Finite);
    public static readonly JoinMetric Liquidus = new("liquidus", MetricBound.Finite);
    public static readonly JoinMetric Dwell = new("dwell", MetricBound.Nonnegative);
    public static readonly JoinMetric Bondline = new("bondline", MetricBound.Positive);
    public static readonly JoinMetric ClampPressure = new("clamp-pressure", MetricBound.Nonnegative);
    public static readonly JoinMetric Torque = new("torque", MetricBound.Positive);
    public static readonly JoinMetric Preload = new("preload", MetricBound.Positive);
    public static readonly JoinMetric UpsetForce = new("upset-force", MetricBound.Positive);
    public static readonly JoinMetric HeadHeight = new("head-height", MetricBound.Positive);
    public static readonly JoinMetric InstallForce = new("install-force", MetricBound.Positive);
    public static readonly JoinMetric Interference = new("interference", MetricBound.Positive);
    public static readonly JoinMetric InsertionForce = new("insertion-force", MetricBound.Positive);
    public static readonly JoinMetric TemperatureDelta = new("temperature-delta", MetricBound.Positive);
    public static readonly JoinMetric Button = new("button", MetricBound.Positive);
    public static readonly JoinMetric Engagement = new("engagement", MetricBound.Positive);
    public static readonly JoinMetric Release = new("release", MetricBound.Positive);
    public static readonly JoinMetric MatingForce = new("mating-force", MetricBound.Positive);
    public static readonly JoinMetric Latching = new("latching", MetricBound.Flag);

    public MetricBound Bound { get; }
}

[SmartEnum<string>]
public sealed partial class JoinMethod {
    public static readonly JoinMethod Fusion = Of("fusion", JoinClass.Weld, PhaseShape.Fusion, JoinPhase.Apply,
        traits: [JoinTrait.Thermal],
        metrics: [JoinMetric.HeatInput, JoinMetric.DepositedEnergy, JoinMetric.Preheat, JoinMetric.Interpass]);
    public static readonly JoinMethod FusionTacked = Of("fusion-tacked", JoinClass.Weld, PhaseShape.Fusion, JoinPhase.Apply,
        traits: [JoinTrait.Thermal, JoinTrait.Tack],
        metrics: [JoinMetric.HeatInput, JoinMetric.DepositedEnergy, JoinMetric.Preheat, JoinMetric.Interpass]);
    public static readonly JoinMethod Braze = Of("braze", JoinClass.Braze, PhaseShape.Heat, JoinPhase.Apply,
        traits: [JoinTrait.Thermal], metrics: [JoinMetric.Liquidus, JoinMetric.Dwell, JoinMetric.DepositedEnergy]);
    public static readonly JoinMethod Solder = Of("solder", JoinClass.Solder, PhaseShape.Heat, JoinPhase.Apply,
        traits: [JoinTrait.Thermal, JoinTrait.Reversible],
        metrics: [JoinMetric.Liquidus, JoinMetric.Dwell, JoinMetric.DepositedEnergy]);
    public static readonly JoinMethod Adhesive = Of("adhesive", JoinClass.Adhesive, PhaseShape.Cure, JoinPhase.Apply,
        metrics: [JoinMetric.Bondline, JoinMetric.Dwell, JoinMetric.ClampPressure]);
    public static readonly JoinMethod Bolt = Of("bolt", JoinClass.Bolt, PhaseShape.Mechanical, JoinPhase.Torque,
        traits: [JoinTrait.Reversible], texts: [JoinText.Agent, JoinText.Locking],
        metrics: [JoinMetric.Torque, JoinMetric.Preload]);
    public static readonly JoinMethod Screw = Of("screw", JoinClass.Screw, PhaseShape.Mechanical, JoinPhase.Torque,
        traits: [JoinTrait.Reversible], texts: [JoinText.Agent, JoinText.Locking],
        metrics: [JoinMetric.Torque, JoinMetric.Preload]);
    public static readonly JoinMethod Rivet = Of("rivet", JoinClass.Rivet, PhaseShape.Mechanical, JoinPhase.Apply,
        metrics: [JoinMetric.UpsetForce, JoinMetric.HeadHeight]);
    public static readonly JoinMethod Stud = Of("stud", JoinClass.Stud, PhaseShape.Mechanical, JoinPhase.Apply,
        traits: [JoinTrait.Reversible], metrics: [JoinMetric.InstallForce]);
    public static readonly JoinMethod ArcStud = Of("arc-stud", JoinClass.Stud, PhaseShape.Heat, JoinPhase.Apply,
        traits: [JoinTrait.Thermal], metrics: [JoinMetric.InstallForce, JoinMetric.DepositedEnergy]);
    public static readonly JoinMethod PressFit = Of("press-fit", JoinClass.PressFit, PhaseShape.Mechanical, JoinPhase.Apply,
        texts: [], metrics: [JoinMetric.Interference, JoinMetric.InsertionForce]);
    public static readonly JoinMethod ShrinkFit = Of("shrink-fit", JoinClass.PressFit, PhaseShape.Heat, JoinPhase.Apply,
        traits: [JoinTrait.Thermal], texts: [],
        metrics: [JoinMetric.Interference, JoinMetric.InsertionForce, JoinMetric.TemperatureDelta, JoinMetric.DepositedEnergy]);
    public static readonly JoinMethod Clinch = Of("clinch", JoinClass.Clinch, PhaseShape.Mechanical, JoinPhase.Apply,
        metrics: [JoinMetric.InsertionForce, JoinMetric.Button]);
    public static readonly JoinMethod Pin = Of("pin", JoinClass.Pin, PhaseShape.Mechanical, JoinPhase.Apply,
        metrics: [JoinMetric.InsertionForce]);
    public static readonly JoinMethod RemovablePin = Of("removable-pin", JoinClass.Pin, PhaseShape.Mechanical, JoinPhase.Apply,
        traits: [JoinTrait.Reversible], metrics: [JoinMetric.InsertionForce]);
    public static readonly JoinMethod Snap = Of("snap", JoinClass.Snap, PhaseShape.Mechanical, JoinPhase.Apply,
        traits: [JoinTrait.Reversible], metrics: [JoinMetric.Engagement, JoinMetric.Release]);
    public static readonly JoinMethod Connector = Of("connector", JoinClass.Connector, PhaseShape.Mechanical, JoinPhase.Apply,
        traits: [JoinTrait.Reversible], metrics: [JoinMetric.MatingForce, JoinMetric.Latching]);

    public JoinClass Class { get; }
    public PhaseShape Shape { get; }
    public JoinPhase Action { get; }
    public Set<JoinTrait> Traits { get; }
    public Set<JoinText> Texts { get; }
    public Set<JoinMetric> Metrics { get; }

    public bool Holds(JoinTrait trait) => Traits.Contains(trait);

    public Seq<JoinPhase> Phases(JoinProgram program, InspectionCadence cadence, JoinOrdinal at) =>
        Run(program, cadence.Applies(at));

    public Seq<JoinPhase> Maximal(JoinProgram program) => Run(program, inspect: true);

    private Seq<JoinPhase> Run(JoinProgram program, bool inspect) => program.Switch(
        state: inspect,
        assembly: (held, _) => Seq(JoinPhase.Locate, JoinPhase.Fit)
            + Spliced(Holds(JoinTrait.Tack), JoinPhase.Tack)
            + Shape.Core(Action)
            + Spliced(held, JoinPhase.Inspect)
            + Seq(JoinPhase.Release, JoinPhase.Handle, JoinPhase.Final),
        service: (held, _) => !Holds(JoinTrait.Reversible)
            ? Seq<JoinPhase>()
            : Seq(JoinPhase.Locate) + Spliced(Holds(JoinTrait.Thermal), JoinPhase.Preheat)
                + Seq(JoinPhase.Unlock, JoinPhase.Extract)
                + Spliced(held, JoinPhase.Inspect)
                + Seq(JoinPhase.Clean, JoinPhase.Handle, JoinPhase.Final));

    private static Seq<JoinPhase> Spliced(bool held, JoinPhase phase) => held ? Seq(phase) : Seq<JoinPhase>();

    private static JoinMethod Of(
        string key,
        JoinClass joinClass,
        PhaseShape shape,
        JoinPhase action,
        JoinTrait[]? traits = null,
        JoinText[]? texts = null,
        JoinMetric[]? metrics = null) =>
        new(key, joinClass, shape, action, toSet(traits ?? []), toSet(texts ?? [JoinText.Agent]), toSet(metrics ?? []));
}

[ComplexValueObject]
public sealed partial class AssemblyExecution {
    public InspectionCadence Cadence { get; }
    public int MaxParallel { get; }
    public bool Parallel => MaxParallel > 1;
    public int Lanes(int demand) => Math.Min(MaxParallel, Math.Max(1, demand));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref InspectionCadence cadence,
        ref int maxParallel) {
        if (maxParallel <= 0)
            validationError = new ValidationError("assembly-execution");
    }

    public static Fin<AssemblyExecution> Admit(InspectionCadence cadence, int maxParallel) =>
        Validate(cadence, maxParallel, out AssemblyExecution execution).Admitted(execution);
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

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length gapMin,
        ref Length gapMax,
        ref Length interferenceMax,
        ref Length alignmentMax,
        ref Length closureMax,
        ref Length surfaceRoughnessMax,
        ref Temperature temperatureMin,
        ref Temperature temperatureMax) {
        if (!(Seq(gapMin, gapMax, interferenceMax, alignmentMax, closureMax, surfaceRoughnessMax).ForAll(Fixtures.Nonnegative)
            && double.IsFinite(temperatureMin.As(TemperatureUnit.DegreeCelsius))
            && double.IsFinite(temperatureMax.As(TemperatureUnit.DegreeCelsius))
            && gapMax >= gapMin && temperatureMax >= temperatureMin))
            validationError = new ValidationError("fit-requirement");
    }

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Double(GapMin.As(LengthUnit.Millimeter)).Double(GapMax.As(LengthUnit.Millimeter))
        .Double(InterferenceMax.As(LengthUnit.Millimeter)).Double(AlignmentMax.As(LengthUnit.Millimeter))
        .Double(ClosureMax.As(LengthUnit.Millimeter)).Double(SurfaceRoughnessMax.As(LengthUnit.Millimeter))
        .Double(TemperatureMin.As(TemperatureUnit.DegreeCelsius))
        .Double(TemperatureMax.As(TemperatureUnit.DegreeCelsius));
}

public readonly record struct AccessCorridor(
    Vector3d Axis,
    Angle HalfAngle,
    Length Standoff,
    Length ToolRadius,
    Length HolderRadius,
    Length Approach,
    Length Retract,
    bool LineOfSight) {
    public bool IsValid => ValidityClaim.All(
        Fixtures.Unit(Axis),
        Fixtures.Finite(HalfAngle), HalfAngle.As(AngleUnit.Radian) is > 0.0 and < (Math.PI / 2.0),
        Fixtures.Positive(Standoff),
        Seq(ToolRadius, HolderRadius, Approach, Retract).ForAll(Fixtures.Nonnegative));

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Coords(Axis).Double(HalfAngle.As(AngleUnit.Radian)).Double(Standoff.As(LengthUnit.Millimeter))
        .Double(ToolRadius.As(LengthUnit.Millimeter)).Double(HolderRadius.As(LengthUnit.Millimeter))
        .Double(Approach.As(LengthUnit.Millimeter)).Double(Retract.As(LengthUnit.Millimeter))
        .Bool(LineOfSight);
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct AssemblyMemberKey {
    public UInt128 Representation { get; }
    public int Instance { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UInt128 representation,
        ref int instance) {
        if (representation == UInt128.Zero || !ValidityClaim.Nonnegative(instance).Holds)
            validationError = new ValidationError("assembly-member-key");
    }

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer.U128(Representation).Ordinal(Instance);
}

[ComplexValueObject]
public sealed partial class JoinProcess {
    public JoinMethod Method { get; }
    public Map<JoinText, string> Texts { get; }
    public Map<JoinMetric, double> Metrics { get; }

    public JoinClass Class => Method.Class;
    public bool Reversible => Method.Holds(JoinTrait.Reversible);
    public bool Thermal => Method.Holds(JoinTrait.Thermal);
    public Energy ThermalLoad => Energy.FromJoules(Of(JoinMetric.DepositedEnergy));
    public Duration Dwell => Duration.FromSeconds(Of(JoinMetric.Dwell));
    public double Of(JoinMetric axis) => Metrics.Find(axis).IfNone(0.0);
    public string Text(JoinText axis) => Texts.Find(axis).IfNone(string.Empty);

    public Seq<JoinPhase> Phases(JoinProgram program, AssemblyExecution execution, JoinOrdinal at) =>
        Method.Phases(program, execution.Cadence, at);

    public Seq<JoinPhase> RequiredPhases(JoinProgram program) => Method.Maximal(program);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref JoinMethod method,
        ref Map<JoinText, string> texts,
        ref Map<JoinMetric, double> metrics) {
        if (!(method.Texts.ForAll(axis => texts.Find(axis).Exists(Witness.Keyed))
            && method.Metrics.ForAll(axis => metrics.Find(axis).Exists(axis.Bound.Admits))
            && metrics.ForAll(static row => row.Key.Bound.Admits(row.Value))))
            validationError = new ValidationError("join-process");
    }

    public static Fin<JoinProcess> Admit(JoinMethod method, Map<JoinText, string> texts, Map<JoinMetric, double> metrics) =>
        Validate(method, texts, metrics, out JoinProcess process).Admitted(process);

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Discriminant(Method).Ordinal(Method.Class.Code)
        .Rows(toSeq(Texts).OrderBy(static row => row.Key.Key, StringComparer.Ordinal).ToSeq(),
            static (held, row) => held.Discriminant(row.Key).String(row.Value))
        .Rows(toSeq(Metrics).OrderBy(static row => row.Key.Key, StringComparer.Ordinal).ToSeq(),
            static (held, row) => held.Discriminant(row.Key).Double(row.Value));
}

public sealed record JoinSpecification(
    JoinProcess Process,
    Arr<AssemblyMemberKey> Components,
    Seq<AccessCorridor> Access,
    FitRequirement Fit,
    Seq<string> Resources,
    Seq<int> FixtureKeys,
    Option<Angle> GrooveIncludedAngle,
    Force Capacity,
    Map<(JoinProgram Program, JoinPhase Phase), Duration> Durations,
    double ReleaseStrengthFraction) {
    public Duration DurationOf(JoinProgram program, JoinPhase phase) =>
        Durations.Find((program, phase)).IfNone(() =>
            phase == JoinPhase.Dwell || phase == JoinPhase.Cool ? Process.Dwell : Duration.Zero);

    public bool DurationsValid =>
        Durations.ForAll(static row => Fixtures.Nonnegative(row.Value))
        && Seq(JoinProgram.Assembly, JoinProgram.Service).ForAll(program =>
            Process.RequiredPhases(program).ForAll(phase =>
                Durations.ContainsKey((program, phase)) || phase == JoinPhase.Dwell || phase == JoinPhase.Cool));

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => Process.CanonicalBytes(Fit.CanonicalBytes(writer
        .Rows(Components.ToSeq(), static (held, component) => component.CanonicalBytes(held))
        .Rows(Access, static (held, access) => access.CanonicalBytes(held))
        .Rows(Resources, static (held, resource) => held.String(resource))
        .Rows(FixtureKeys, static (held, fixture) => held.Ordinal(fixture))
        .Rows(toSeq(Durations)
                .OrderBy(static row => row.Key.Program.Key, StringComparer.Ordinal)
                .ThenBy(static row => row.Key.Phase.Rank).ToSeq(),
            static (held, row) => held.Discriminant(row.Key.Program).Ordinal(row.Key.Phase.Rank)
                .Double(row.Value.As(DurationUnit.Second)))
        .Maybe(GrooveIncludedAngle, static (held, angle) => held.Double(angle.As(AngleUnit.Radian)))
        .Double(Capacity.As(ForceUnit.Newton))
        .Double(ReleaseStrengthFraction)));
}

public sealed record AssemblyMember(AssemblyMemberKey Key, AdmittedComponent Component, Transform Pose);

public sealed record AssemblyJoint(
    int Index,
    AssemblyMemberKey Owner,
    ComponentConnection Connection,
    Edge3 At,
    JoinSpecification Specification);
```

## [03]-[PLANNING]

- Owner: `AssemblyPolicy` is raw ingress; `JoinNode` is one executable phase; `AssemblyPlan` is the reduced proof-bearing result; `Assemblies` owns every fold.
- Cases: `PrecedenceKind` closes phase, datum, occlusion, fit, load-path, support, thermal, cure, resource, inspection, handling, and reversible-service reasons.
- Law: `JointIndex` is built ONCE at admission and carries the joint-by-key map, the ordinal each joint holds, its assembly phase run, its first and last node, and the member-to-incident-joint index. Every pairwise fold reads it, so the ordinal scan that ran inside a nested pair loop — turning a quadratic precedence build into a cubic one — has no site left, and shared-member neighbours resolve through the incidence index rather than a component cross-product.
- Law: `IAssemblyEvidenceSource.Evaluate` proves connected support, capacity, center-of-gravity margin, temporary fixture custody, load-path continuity, fit, sight, and robot placement once per join; `JoinMeasure` retains that boundary evidence WHOLE rather than spreading it across columns a construction site re-spells, so a fifth boundary column reaches every consumer with no arm that forgot to carry it.
- Law: the boundary reports sight as `BlockedCorridor` rows — the same shape the analytic corridor kernel publishes — so an occluded sight line names its occluder and the two censuses correlate without a positional decode. `JoinRejection.Visibility` names a MALFORMED census (a row addressing a corridor outside the joint's own roster); `JoinRejection.Sight` names a corridor demanding line of sight that a row occludes.
- Law: fit and datum errors fold along component paths, and a join fails admission when gap, interference, alignment, or accumulated closure exceeds its carried requirement. The `Joining/sequence` `DistortionField` spends the joint's own alignment budget through `workholding#FIXTURE` `DatumTransfer` before that comparison, so a distortion the weld plane measured narrows the fit a joint may claim rather than arriving as a second estimate here.
- Law: every approach and retract corridor composes `Workholding.Apply` at the phase's `FixtureState`; analytic cone occlusion checks every potential neighbour over the full axial interval.
- Law: source-first order respects resource exclusivity, dwell, cool, inspection, and lane policy; each step carries typed start, finish, fixture, resources, and stability evidence, and every result resolves by joint key through a TOTAL read — an absent subassembly label, joint, or result refuses typed rather than throwing out of an indexer on a non-`Fin` path.
- Law: disassembly reverses the proven precedence through `SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward)`, so an occlusion or thermal edge that gated a join gates its removal; a reversed roster and a reversed order sequence both ignore those edges.
- Law: removing a completed or blocked joint re-proves every surviving result against the residual assembly through the same evidence boundary, because removal moves the load path the original results measured.
- Law: transitive reduction takes NO edge factory and returns the ORIGINAL edges, so the surviving node pairs read once into a set and the typed reason edges filter against it in one pass.
- Result: a cyclic precedence graph publishes its strongly-connected COMPONENT MEMBERS on `FabricationFault.AssemblyPrecedenceCyclic`, through the ONE `setups#SCHEDULE` `Setups.Components` grouping both Fixturing precedence graphs read — a vertex-and-edge count names nothing a caller can break.
- Exemption: `Occludes` is the analytic corridor kernel, `Schedule` the bounded lane fold, and `Physical` the connectivity labelling; mutation stays inside admitted graph and fold containers.
- Packages: `BidirectionalGraph<JoinNode, AssemblyEdge>` carries reason payloads directly, while the component `UndirectedGraph` remains the disjoint physical-connectivity projection. Every graph question is a shipped `AlgorithmExtensions` call and no walk is hand-rolled: `IsDirectedAcyclicGraph` rails ahead of the two sorts that throw on cyclic input, `SourceFirstBidirectionalTopologicalSort` answers both directions, `ComputeTransitiveReduction` reduces, `InEdges` reads predecessors inside the lane fold, `ConnectedComponents` labels physical subassemblies, and the cyclic witness composes the ONE `Setups.Components` grouping.
- Boundary: precedence and physical connectivity remain distinct; geometry failure, missing specification, unstable release, and blocked access remain typed failures carrying a `JoinRejection` reason rather than one opaque code.

```csharp
// --- [PLANNING] ------------------------------------------------------------------------
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

public readonly record struct BlockedCorridor(int Joint, int Corridor, int Occluder);

public readonly record struct FixtureStability(
    int Components,
    double CapacityMargin,
    double SupportMargin,
    double LoadPathMargin,
    bool FixtureHeld) {
    public double Minimum => Seq(CapacityMargin, SupportMargin, LoadPathMargin).Min(double.PositiveInfinity);
    public bool Stable => Components > 0 && Minimum >= 1.0;

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Ordinal(Components).Double(CapacityMargin).Double(SupportMargin).Double(LoadPathMargin).Bool(FixtureHeld);
}

public readonly record struct FitTolerance(
    Length Gap,
    Length Interference,
    Length Alignment,
    Length Closure,
    Length SurfaceRoughness,
    Temperature Temperature) {
    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Double(Gap.As(LengthUnit.Millimeter)).Double(Interference.As(LengthUnit.Millimeter))
        .Double(Alignment.As(LengthUnit.Millimeter)).Double(Closure.As(LengthUnit.Millimeter))
        .Double(SurfaceRoughness.As(LengthUnit.Millimeter))
        .Double(Temperature.As(TemperatureUnit.DegreeCelsius));
}

public readonly record struct AssemblyBoundaryEvidence(
    FitTolerance Tolerance,
    FixtureStability Stability,
    Option<CellPlacement> Robot,
    Seq<BlockedCorridor> Sight);

public readonly record struct JoinMeasure(
    int Joint,
    AssemblyBoundaryEvidence Boundary,
    Seq<WorkholdingResult.Clearance> Clearance,
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
    FixtureStability Stability) {
    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => Stability.CanonicalBytes(writer
        .Ordinal(Order).Ordinal(Joint).Ordinal(Phase.Rank).Ordinal(Subassembly)
        .Maybe(Fixture, static (held, fixture) => held.Ordinal(fixture))
        .Rows(Resources, static (held, resource) => held.String(resource))
        .Double(Start.As(DurationUnit.Second)).Double(Finish.As(DurationUnit.Second)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public interface IAssemblyEvidenceSource {
    Fin<AssemblyBoundaryEvidence> Evaluate(
        AssemblyJoint joint,
        Seq<AssemblyMember> members,
        Seq<Fixture> fixtures,
        AssemblyPolicy policy);
}

public sealed record AssemblyPolicy(
    AssemblyExecution Execution,
    Length CorridorClearance,
    Seq<int> DatumJoints,
    FixtureSet Fixtures,
    Map<PropertyName, JoinSpecification> Specifications,
    Force HandlingLoad,
    Option<DistortionField> Distortion,
    IAssemblyEvidenceSource Evidence);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssemblyOp {
    private AssemblyOp() { }

    public sealed record Admit(Seq<AssemblyMember> Members, AssemblyPolicy Policy) : AssemblyOp;
    public sealed record Plan(Seq<AssemblyMember> Members, AssemblyPolicy Policy) : AssemblyOp;
    public sealed record Replan(AssemblyPlan Plan, AssemblyPolicy Policy, Seq<int> Completed, Seq<int> Blocked) : AssemblyOp;
    public sealed record Disassemble(AssemblyPlan Plan, Seq<int> Targets) : AssemblyOp;
    public sealed record Project(AssemblyPlan Plan, AssemblyProjection Projection) : AssemblyOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssemblyResult {
    private AssemblyResult() { }

    public sealed record Admitted(Seq<AssemblyJoint> Joints, AssemblyPolicy Policy) : AssemblyResult;
    public sealed record Planned(AssemblyPlan Plan) : AssemblyResult;
    public sealed record Replanned(AssemblyPlan Plan) : AssemblyResult;
    public sealed record Disassembled(Seq<JoinStep> Steps) : AssemblyResult;
    public sealed record Projected(AssemblyArtifact Artifact) : AssemblyResult;
}

public sealed record AssemblyPlan(
    Seq<AssemblyMember> Members,
    AssemblyExecution Execution,
    Seq<JoinStep> Steps,
    int Subassemblies,
    Seq<AssemblyEdge> Precedence,
    Seq<AssemblyJoint> Joints,
    Seq<JoinMeasure> Results,
    Seq<BlockedCorridor> Blocked,
    Seq<JoinStep> ServiceOrder,
    ContentKey Key) {
    public static Fin<AssemblyResult> Apply(AssemblyOp? op) =>
        Optional(op)
            .ToFin(FabricationFault.Fixture(new FixturingWitness.Absent()))
            .Bind(static operation => operation.Switch(
                admit: static row => Assemblies.Admit(row.Members, row.Policy)
                    .Map<AssemblyResult>(joints => new AssemblyResult.Admitted(joints, row.Policy)),
                plan: static row => Assemblies.Admit(row.Members, row.Policy)
                    .Bind(joints => Assemblies.Ordered(row.Members, joints, row.Policy))
                    .Map<AssemblyResult>(static plan => new AssemblyResult.Planned(plan)),
                replan: static row => Assemblies.Replan(row.Plan, row.Policy, row.Completed, row.Blocked)
                    .Map<AssemblyResult>(static plan => new AssemblyResult.Replanned(plan)),
                disassemble: static row => Assemblies.Disassemble(row.Plan, row.Targets)
                    .Map<AssemblyResult>(static steps => new AssemblyResult.Disassembled(steps)),
                project: static row => Assemblies.Project(row.Plan, row.Projection)
                    .Map<AssemblyResult>(static artifact => new AssemblyResult.Projected(artifact))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Assemblies {
    internal sealed record JointIndex(
        Seq<AssemblyJoint> Joints,
        Map<int, AssemblyJoint> ByKey,
        Map<int, int> Ordinal,
        Map<int, Seq<JoinPhase>> Phases,
        Map<AssemblyMemberKey, Seq<int>> Incident) {
        public JoinNode First(int joint) =>
            new(joint, Phases.Find(joint).Bind(static run => run.Head).IfNone(JoinPhase.Final));

        public JoinNode Last(int joint) => new(joint, JoinPhase.Final);

        public Seq<AssemblyJoint> Neighbours(AssemblyJoint joint) =>
            joint.Specification.Components.ToSeq()
                .Bind(component => Incident.Find(component).IfNone(Seq<int>()))
                .Distinct()
                .Filter(index => index != joint.Index)
                .Choose(ByKey.Find);

        public static JointIndex Of(Seq<AssemblyJoint> joints, AssemblyExecution execution) {
            Map<int, int> ordinals = joints.Fold(Map<int, int>(),
                static (index, joint) => index.Add(joint.Index, index.Count));
            return new JointIndex(
                joints,
                toMap(joints.Map(static joint => (joint.Index, joint))),
                ordinals,
                toMap(joints.Map(joint => (joint.Index, joint.Specification.Process.Phases(
                    JoinProgram.Assembly, execution, new JoinOrdinal(ordinals[joint.Index], joints.Count))))),
                joints
                    .Bind(joint => joint.Specification.Components.ToSeq().Map(component => (component, joint.Index)))
                    .Fold(Map<AssemblyMemberKey, Seq<int>>(), static (index, row) =>
                        index.AddOrUpdate(row.component, held => held.Add(row.Item2), () => Seq(row.Item2))));
        }

        public Seq<JoinPhase> ServicePhases(int joint, AssemblyExecution execution) =>
            ByKey.Find(joint).Map(row => row.Specification.Process.Phases(
                JoinProgram.Service, execution, new JoinOrdinal(Ordinal.Find(joint).IfNone(0), Joints.Count)))
                .IfNone(Seq<JoinPhase>());
    }

    // --- [ADMISSION]
    internal static Fin<Seq<AssemblyJoint>> Admit(Seq<AssemblyMember> members, AssemblyPolicy policy) =>
        (GateMembers(members), GatePolicy(members, policy), Census(members, policy))
            .Apply(static (_, _, joints) => joints)
            .As()
            .ToFin();

    private static K<Validation<Error>, Unit> GateMembers(Seq<AssemblyMember> members) {
        Set<AssemblyMemberKey> keys = toSet(members.Map(static member => member.Key));
        return AdmissionSlots.Gate(
            !members.IsEmpty && keys.Count == members.Count
            && members.ForAll(static member =>
                member.Key.Representation == member.Component.RepresentationKey && member.Pose.IsValid),
            FabricationFault.Fixture(new FixturingWitness.Membership(None, members.Count, keys.Count)));
    }

    private static K<Validation<Error>, Unit> GatePolicy(Seq<AssemblyMember> input, AssemblyPolicy policy) {
        Set<AssemblyMemberKey> members = toSet(input.Map(static member => member.Key));
        Set<PropertyName> realized = toSet(input.Bind(static member => member.Component.Connections.ToSeq()
            .Map(static connection => connection.RealizingKey)));
        return AdmissionSlots.Gate(
            Fixtures.Nonnegative(policy.CorridorClearance) && Fixtures.Nonnegative(policy.HandlingLoad)
            && policy.DatumJoints.Distinct().Count == policy.DatumJoints.Count
            && policy.Fixtures.ByOperation.Values.ForAll(static fixture => fixture.Constraint.Constrained)
            && realized.Count == policy.Specifications.Count
            && realized.ForAll(policy.Specifications.ContainsKey)
            && policy.Specifications.Values.ForAll(specification => Valid(specification, members, policy)),
            FabricationFault.Fixture(
                new FixturingWitness.Membership(None, realized.Count, policy.Specifications.Count)));
    }

    private static bool Valid(JoinSpecification specification, Set<AssemblyMemberKey> members, AssemblyPolicy policy) =>
        specification.Components.Count >= 2
        && specification.Components.Distinct().Count == specification.Components.Count
        && specification.Components.ForAll(members.Contains)
        && specification.Access.ForAll(static access => access.IsValid)
        && specification.FixtureKeys.Distinct().Count == specification.FixtureKeys.Count
        && specification.FixtureKeys.ForAll(policy.Fixtures.ByOperation.ContainsKey)
        && specification.Resources.Distinct().Count == specification.Resources.Count
        && specification.Resources.ForAll(Witness.Keyed)
        && specification.GrooveIncludedAngle.ForAll(static angle =>
            Fixtures.Finite(angle) && angle.As(AngleUnit.Radian) is > 0.0 and <= Math.PI)
        && Fixtures.Positive(specification.Capacity)
        && specification.DurationsValid
        && double.IsFinite(specification.ReleaseStrengthFraction)
        && specification.ReleaseStrengthFraction is > 0.0 and <= 1.0;

    private static K<Validation<Error>, Seq<AssemblyJoint>> Census(Seq<AssemblyMember> input, AssemblyPolicy policy) =>
        toSeq(input
                .Bind(member => member.Component.Connections.ToSeq().Map(connection => (member, connection)))
                .GroupBy(static row => row.connection.RealizingKey)
                .OrderBy(static group => group.Key.Value, StringComparer.Ordinal))
            .Map((group, index) => (Key: group.Key, Rows: toSeq(group
                .OrderBy(static row => row.member.Key.Representation)
                .ThenBy(static row => row.member.Key.Instance)
                .ThenBy(static row => row.connection.DetailKey.Value, StringComparer.Ordinal)), Index: index))
            .Traverse(group => (
                    policy.Specifications
                        .Find(group.Key)
                        .ToFin(FabricationFault.Fixture(
                            new FixturingWitness.Membership(Some(group.Index), group.Rows.Count, 0))),
                    group.Rows.Choose(static row => row.connection.At.Map(at => (row.member, row.connection, at))).Head
                        .ToFin(new GeometryFault.DegenerateInput(
                            Kind.Line, group.Index, nameof(ComponentConnection.At))))
                .Apply((specification, located) => group.Rows.ForAll(row => specification.Components.Contains(row.member.Key))
                    ? Fin.Succ(new AssemblyJoint(group.Index, specification.Components[0], located.connection,
                        new Edge3(located.member.Pose * located.at.A, located.member.Pose * located.at.B), specification))
                    : Fin.Fail<AssemblyJoint>(FabricationFault.Fixture(new FixturingWitness.Membership(
                        Some(group.Index), group.Rows.Count,
                        group.Rows.Count(row => specification.Components.Contains(row.member.Key)))))
                .As().Bind(identity).ToValidation());

    // --- [PLANNING]
    internal static Fin<AssemblyPlan> Ordered(Seq<AssemblyMember> input, Seq<AssemblyJoint> joints, AssemblyPolicy policy) {
        JointIndex index = JointIndex.Of(joints, policy.Execution);
        return policy.DatumJoints.Exists(anchor => !index.ByKey.ContainsKey(anchor))
            ? Fin.Fail<AssemblyPlan>(FabricationFault.Fixture(new FixturingWitness.Membership(
                policy.DatumJoints.Find(anchor => !index.ByKey.ContainsKey(anchor)),
                policy.DatumJoints.Count, joints.Count)))
            : joints.Traverse(joint => Result(joint, index, input, policy).ToValidation()).As().ToFin()
                .Bind(results => Assemble(input, index, results, policy));
    }

    private static Fin<AssemblyPlan> Assemble(
        Seq<AssemblyMember> input,
        JointIndex index,
        Seq<JoinMeasure> results,
        AssemblyPolicy policy) {
        (BidirectionalGraph<JoinNode, AssemblyEdge> graph, Seq<BlockedCorridor> blocked) = Graph(index, policy);
        if (!graph.IsDirectedAcyclicGraph())
            return Fin.Fail<AssemblyPlan>(new FabricationFault.AssemblyPrecedenceCyclic(
                Setups.Components<JoinNode, AssemblyEdge>(graph)
                    .Bind(static component => component.ToSeq().Map(static node => node.Joint))
                    .Distinct().ToArr()));

        (Map<AssemblyMemberKey, int> components, int count) = Physical(input, index.Joints);
        Map<int, JoinMeasure> keyed = toMap(results.Map(static result => (result.Joint, result)));
        Seq<JoinNode> forward = toSeq(graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Forward));
        Seq<JoinNode> backward = toSeq(graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward));
        return (Schedule(graph, forward, index, keyed, components, policy).ToValidation(),
                Service(backward, index, keyed, components, policy.Execution).ToValidation())
            .Apply((steps, service) => (Steps: steps, Service: service, Reduced: Reduced(graph)))
            .As().ToFin()
            .Bind(built => FabricationCanon.Keyed(
                EgressKind.Plan,
                Grid(index.Joints),
                writer => Canonical(writer, input, policy.Execution, built.Steps, count, built.Reduced,
                    index.Joints, results, blocked, built.Service),
                Key)
                .Map(key => new AssemblyPlan(input, policy.Execution, built.Steps, count, built.Reduced,
                    index.Joints, results, blocked, built.Service, key)));
    }

    private static (BidirectionalGraph<JoinNode, AssemblyEdge> Graph, Seq<BlockedCorridor> Blocked) Graph(
        JointIndex index,
        AssemblyPolicy policy) {
        BidirectionalGraph<JoinNode, AssemblyEdge> graph = new(allowParallelEdges: true);
        index.Joints.Iter(joint => {
            Seq<JoinPhase> phases = index.Phases[joint.Index];
            graph.AddVertexRange(phases.Map(phase => new JoinNode(joint.Index, phase)));
            phases.Zip(phases.Tail).Iter(pair => graph.AddEdge(new AssemblyEdge(
                new JoinNode(joint.Index, pair.First), new JoinNode(joint.Index, pair.Second), pair.Second.Entered)));
        });
        Set<int> datum = toSet(policy.DatumJoints);
        policy.DatumJoints.Iter(anchor => index.Joints
            .Filter(joint => !datum.Contains(joint.Index))
            .Iter(joint => graph.AddEdge(new AssemblyEdge(
                index.Last(anchor), index.First(joint.Index), PrecedenceKind.Datum))));
        index.Joints.Filter(static joint => !joint.Specification.Process.Reversible).Iter(fixedJoint =>
            index.Neighbours(fixedJoint).Filter(static joint => joint.Specification.Process.Reversible).Iter(service =>
                graph.AddEdge(new AssemblyEdge(
                    index.Last(fixedJoint.Index), index.First(service.Index), PrecedenceKind.Service))));
        index.Joints.Iter(hot => index.Neighbours(hot)
            .Filter(cold => hot.Specification.Process.ThermalLoad > cold.Specification.Process.ThermalLoad)
            .Iter(cold => graph.AddEdge(new AssemblyEdge(
                index.Last(hot.Index), index.First(cold.Index), PrecedenceKind.Thermal))));
        Seq<BlockedCorridor> blocked = index.Joints
            .Bind(before => before.Specification.Access.Map((access, corridor) => (before, access, corridor)))
            .Bind(row => index.Joints
                .Filter(after => after.Index != row.before.Index && Occludes(row.before.At, row.access,
                    row.before.Specification.GrooveIncludedAngle, after.At, policy.CorridorClearance))
                .Map(after => new BlockedCorridor(row.before.Index, row.corridor, after.Index)));
        blocked.Iter(row => graph.AddEdge(new AssemblyEdge(
            index.Last(row.Joint), index.First(row.Occluder), PrecedenceKind.Occlusion)));
        return (graph, blocked);
    }

    private static Seq<AssemblyEdge> Reduced(BidirectionalGraph<JoinNode, AssemblyEdge> graph) {
        BidirectionalGraph<JoinNode, SEdge<JoinNode>> simple = new(allowParallelEdges: false);
        simple.AddVertexRange(graph.Vertices);
        toSeq(graph.Edges).Iter(edge => {
            if (!simple.ContainsEdge(edge.Source, edge.Target)) simple.AddEdge(new SEdge<JoinNode>(edge.Source, edge.Target));
        });
        Set<(JoinNode Source, JoinNode Target)> kept = toSet(toSeq(simple.ComputeTransitiveReduction().Edges)
            .Map(static edge => (edge.Source, edge.Target)));
        return toSeq(graph.Edges).Filter(edge => kept.Contains((edge.Source, edge.Target))).Distinct();
    }

    // --- [EVIDENCE]
    private static Fin<JoinMeasure> Result(
        AssemblyJoint joint,
        JointIndex index,
        Seq<AssemblyMember> members,
        AssemblyPolicy policy) {
        Duration duration = index.Phases[joint.Index]
            .Map(phase => joint.Specification.DurationOf(JoinProgram.Assembly, phase))
            .Fold(Duration.Zero, static (total, value) => total + value);
        Seq<Fixture> fixtures = joint.Specification.FixtureKeys.Choose(policy.Fixtures.ByOperation.Find);
        return (policy.Evidence.Evaluate(joint,
                    members.Filter(member => joint.Specification.Components.Contains(member.Key)), fixtures, policy).ToValidation(),
                fixtures.Traverse(fixture => joint.Specification.Access.Traverse(access =>
                    Workholding.Apply(new WorkholdingOp.Clear(fixture, FixtureState.Clamp, Corridor(
                            joint.At, access, joint.Specification.GrooveIncludedAngle, policy.CorridorClearance)))
                        .Bind(static result => result switch {
                            WorkholdingResult.Clearance result => Fin.Succ(result),
                            _ => throw new InvalidOperationException("Workholding.Clear returned a non-clearance result."),
                        }).ToValidation())).As())
            .Apply(static (boundary, rows) => (boundary, Clearance: rows.Bind(identity)))
            .As().ToFin()
            .Bind(result => Admissible(result.boundary, result.Clearance, joint, fixtures, policy)
                ? Fin.Succ(new JoinMeasure(joint.Index, result.boundary, result.Clearance,
                    joint.Specification.Resources, duration))
                : Fin.Fail<JoinMeasure>(FabricationFault.Fixture(new FixturingWitness.Join(
                    joint.Index, Rejection(result.boundary, joint, fixtures, policy))));
    }

    private static bool Admissible(
        AssemblyBoundaryEvidence boundary,
        Seq<WorkholdingResult.Clearance> clearance,
        AssemblyJoint joint,
        Seq<Fixture> fixtures,
        AssemblyPolicy policy) =>
        Fits(boundary.Tolerance, joint.Specification.Fit, joint, policy)
        && boundary.Stability.Stable
        && boundary.Stability.Components == joint.Specification.Components.Count
        && (fixtures.IsEmpty || boundary.Stability.FixtureHeld)
        && boundary.Robot.ForAll(static result =>
            result.Selected.Metrics.Find(CellPlacementMetric.Feasibility).Exists(static value => value == 0.0))
        && Sighted(boundary, joint)
        && clearance.ForAll(static result => result.Clear);

    private static bool Sighted(AssemblyBoundaryEvidence boundary, AssemblyJoint joint) =>
        boundary.Sight.ForAll(row => row.Joint == joint.Index
            && row.Corridor >= 0 && row.Corridor < joint.Specification.Access.Count)
        && joint.Specification.Access
            .Map((access, ordinal) => !access.LineOfSight
                || !boundary.Sight.Exists(row => row.Corridor == ordinal))
            .ForAll(identity);

    private static JoinRejection Rejection(
        AssemblyBoundaryEvidence boundary,
        AssemblyJoint joint,
        Seq<Fixture> fixtures,
        AssemblyPolicy policy) =>
        !Fits(boundary.Tolerance, joint.Specification.Fit, joint, policy) ? JoinRejection.Fit
        : !boundary.Stability.Stable ? JoinRejection.Stability
        : boundary.Stability.Components != joint.Specification.Components.Count ? JoinRejection.Components
        : !fixtures.IsEmpty && !boundary.Stability.FixtureHeld ? JoinRejection.Custody
        : boundary.Robot.Exists(static result =>
            !result.Selected.Metrics.Find(CellPlacementMetric.Feasibility).Exists(static value => value == 0.0))
            ? JoinRejection.Robot
        : boundary.Sight.Exists(row => row.Joint != joint.Index
            || row.Corridor < 0 || row.Corridor >= joint.Specification.Access.Count)
            ? JoinRejection.Visibility
        : !Sighted(boundary, joint) ? JoinRejection.Sight : JoinRejection.Access;

    private static bool Fits(
        FitTolerance result,
        FitRequirement requirement,
        AssemblyJoint joint,
        AssemblyPolicy policy) {
        Length alignment = policy.Distortion.Match(
            Some: displacement => DatumTransfer
                .Of(requirement.AlignmentMax, displacement, toSet(joint.Specification.Components.ToSeq()))
                .Remaining,
            None: () => requirement.AlignmentMax);
        return result.Gap >= requirement.GapMin && result.Gap <= requirement.GapMax
            && result.Interference <= requirement.InterferenceMax
            && result.Alignment <= alignment
            && result.Closure <= requirement.ClosureMax
            && result.SurfaceRoughness <= requirement.SurfaceRoughnessMax
            && result.Temperature >= requirement.TemperatureMin
            && result.Temperature <= requirement.TemperatureMax;
    }

    // --- [SCHEDULE]
    private static Fin<Seq<JoinStep>> Schedule(
        BidirectionalGraph<JoinNode, AssemblyEdge> graph,
        Seq<JoinNode> order,
        JointIndex index,
        Map<int, JoinMeasure> results,
        Map<AssemblyMemberKey, int> components,
        AssemblyPolicy policy) =>
        order.Fold(
            Fin.Succ((Steps: Seq<JoinStep>(), Active: Map<string, double>(), Finished: Map<JoinNode, double>(),
                Lanes: toSeq(Enumerable.Repeat(0.0, policy.Execution.Lanes(order.Count))).ToArr())),
            (rail, node) => rail.Bind(state => Seated(index, results, components, node.Joint).Map(seat => {
                int lane = toSeq(Enumerable.Range(0, state.Lanes.Count)).Fold(0,
                    (best, slot) => state.Lanes[slot] < state.Lanes[best] ? slot : best);
                double predecessor = toSeq(graph.InEdges(node))
                    .Map(edge => state.Finished.Find(edge.Source).IfNone(0.0)).Fold(0.0, Math.Max);
                double ready = seat.Joint.Specification.Resources
                    .Map(resource => state.Active.Find(resource).IfNone(0.0))
                    .Fold(Math.Max(state.Lanes[lane], predecessor), Math.Max);
                double finish = ready + seat.Joint.Specification.DurationOf(JoinProgram.Assembly, node.Phase).As(DurationUnit.Second);
                return (
                    state.Steps.Add(new JoinStep(state.Steps.Count, node.Joint, node.Phase, seat.Subassembly,
                        seat.Joint.Specification.FixtureKeys.Head, seat.Joint.Specification.Resources,
                        Duration.FromSeconds(ready), Duration.FromSeconds(finish), seat.Stability)),
                    seat.Joint.Specification.Resources.Fold(state.Active,
                        (held, resource) => held.AddOrUpdate(resource, finish)),
                    state.Finished.AddOrUpdate(node, finish),
                    state.Lanes.SetItem(lane, finish));
            })))
            .Map(static state => state.Steps);

    private static Fin<Seq<JoinStep>> Service(
        Seq<JoinNode> backward,
        JointIndex index,
        Map<int, JoinMeasure> results,
        Map<AssemblyMemberKey, int> components,
        AssemblyExecution execution) =>
        backward.Map(static node => node.Joint).Distinct().Fold(
            Fin.Succ((Steps: Seq<JoinStep>(), Elapsed: Duration.Zero)),
            (rail, joint) => rail.Bind(state => Seated(index, results, components, joint).Map(seat =>
                index.ServicePhases(joint, execution).Fold(state, (held, phase) => {
                    Duration finish = held.Elapsed + seat.Joint.Specification.DurationOf(JoinProgram.Service, phase);
                    return (held.Steps.Add(new JoinStep(held.Steps.Count, joint, phase, seat.Subassembly,
                        seat.Joint.Specification.FixtureKeys.Head, seat.Joint.Specification.Resources,
                        held.Elapsed, finish, seat.Stability)), finish);
                }))))
            .Map(static state => state.Steps);

    private static Fin<(AssemblyJoint Joint, int Subassembly, FixtureStability Stability)> Seated(
        JointIndex index,
        Map<int, JoinMeasure> results,
        Map<AssemblyMemberKey, int> components,
        int joint) =>
        (index.ByKey.Find(joint).ToFin(Absent(joint, index.Joints.Count, 0)).ToValidation(),
         results.Find(joint).ToFin(Absent(joint, index.Joints.Count, results.Count)).ToValidation())
            .Apply(static (row, result) => (row, result))
            .As().ToFin()
            .Bind(pair => components.Find(pair.row.Specification.Components[0])
                .ToFin(Absent(joint, pair.row.Specification.Components.Count, components.Count))
                .Map(label => (pair.row, label, pair.result.Boundary.Stability)));

    private static Error Absent(int joint, int expected, int available) =>
        FabricationFault.Fixture(new FixturingWitness.Membership(Some(joint), expected, available));

    private static (Map<AssemblyMemberKey, int> Labels, int Count) Physical(
        Seq<AssemblyMember> input,
        Seq<AssemblyJoint> joints) {
        UndirectedGraph<AssemblyMemberKey, SEdge<AssemblyMemberKey>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(input.Map(static member => member.Key));
        joints.Iter(joint => joint.Specification.Components.ToSeq().Tail.Iter(component =>
            graph.AddEdge(new SEdge<AssemblyMemberKey>(joint.Specification.Components[0], component))));
        Dictionary<AssemblyMemberKey, int> labels = new();
        int count = graph.ConnectedComponents(labels);
        return (toMap(toSeq(labels).Map(static row => (row.Key, row.Value))), count);
    }

    // --- [RESIDUAL]
    internal static Fin<AssemblyPlan> Replan(AssemblyPlan plan, AssemblyPolicy policy, Seq<int> completed, Seq<int> blocked) {
        if (completed.Distinct().Count != completed.Count || blocked.Distinct().Count != blocked.Count
            || completed.Exists(blocked.Contains)
            || completed.Concat(blocked).Exists(index => !plan.Joints.Exists(joint => joint.Index == index)))
            return Fin.Fail<AssemblyPlan>(FabricationFault.Fixture(
                new FixturingWitness.Residual(completed.Count, blocked.Count, plan.Joints.Count)));
        Set<int> removed = toSet(completed.Concat(blocked));
        Seq<AssemblyJoint> residual = plan.Joints.Filter(joint => !removed.Contains(joint.Index));
        Seq<AssemblyMember> members = plan.Members.Filter(member =>
            residual.Exists(joint => joint.Specification.Components.Contains(member.Key)));
        return Ordered(members, residual, policy);
    }

    internal static Fin<Seq<JoinStep>> Disassemble(AssemblyPlan plan, Seq<int> targets) =>
        !targets.IsEmpty && targets.Distinct().Count == targets.Count
        && targets.ForAll(identity => plan.Joints.Find(joint => joint.Index == identity)
            .Exists(static joint => joint.Specification.Process.Reversible))
            ? Fin.Succ(plan.ServiceOrder.Filter(step => targets.Contains(step.Joint)))
            : Fin.Fail<Seq<JoinStep>>(FabricationFault.Fixture(
                new FixturingWitness.Residual(0, targets.Count, plan.Joints.Count)));

    // --- [CORRIDOR]
    private static bool Occludes(Edge3 joint, AccessCorridor access, Option<Angle> groove, Edge3 obstacle, Length clearance) {
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
        Seq<double> ordered = toSeq(bounds.Distinct().OrderBy(identity));
        return ordered.Zip(ordered.Tail).Exists(interval => {
            double middle = 0.5 * (interval.First + interval.Second);
            double axial = axial0 + (axialRate * middle);
            double radius0 = axial <= 0.0 ? basis : axial >= standoff ? basis + (slope * standoff) : basis + (slope * axial);
            double radiusRate = axial is > 0.0 && axial < standoff ? slope * axialRate : 0.0;
            double a = (radialRate * radialRate) - (radiusRate * radiusRate);
            double b = 2.0 * ((radial0 * radialRate) - (radius0 * radiusRate));
            double c = (radial0 * radial0) - (radius0 * radius0);
            double stationary = Math.Abs(a) > EpsilonPolicy.ZeroTolerance
                ? Math.Clamp(-b / (2.0 * a), interval.First, interval.Second)
                : interval.First;
            return Seq(interval.First, stationary, interval.Second)
                .Min(parameter => (a * parameter * parameter) + (b * parameter) + c) <= 0.0;
        });
    }

    private static ToolCorridor Corridor(Edge3 joint, AccessCorridor access, Option<Angle> groove, Length clearance) {
        Point3d origin = Mid(joint);
        Vector3d axis = access.Axis;
        axis.Unitize();
        double angle = groove.Map(value => Math.Min(access.HalfAngle.As(AngleUnit.Radian), 0.5 * value.As(AngleUnit.Radian)))
            .IfNone(access.HalfAngle.As(AngleUnit.Radian));
        double standoff = access.Standoff.As(LengthUnit.Millimeter);
        double tool = access.ToolRadius.As(LengthUnit.Millimeter) + clearance.As(LengthUnit.Millimeter);
        double holder = access.HolderRadius.As(LengthUnit.Millimeter) + clearance.As(LengthUnit.Millimeter);
        double approach = access.Approach.As(LengthUnit.Millimeter);
        double retract = access.Retract.As(LengthUnit.Millimeter);
        return new ToolCorridor(CorridorKind.Tool, Seq(
            Station(origin - (approach * axis), tool, holder),
            Station(origin, tool, holder),
            Station(origin + (standoff * axis), tool, holder + (Math.Tan(angle) * standoff)),
            Station(origin + ((standoff + retract) * axis), tool, holder)));
    }

    private static CorridorStation Station(Point3d point, double tool, double holder) => new(
        point,
        Length.FromMillimeters(tool),
        Length.FromMillimeters(holder),
        Length.Zero,
        Length.Zero);

    private static (double Lower, double Upper) AxialInterval(double at, double rate, double minimum, double maximum) {
        if (Math.Abs(rate) < EpsilonPolicy.ZeroTolerance)
            return at >= minimum && at <= maximum ? (0.0, 1.0) : (1.0, 0.0);
        double first = (minimum - at) / rate;
        double second = (maximum - at) / rate;
        return (Math.Max(0.0, Math.Min(first, second)), Math.Min(1.0, Math.Max(first, second)));
    }

    private static Seq<double> Crossing(double at, double rate, double target) =>
        Math.Abs(rate) < EpsilonPolicy.ZeroTolerance ? Seq<double>() : Seq((target - at) / rate);

    private static Point3d Mid(Edge3 at) => at.A + (0.5 * (at.B - at.A));

    // --- [CANONICAL]
    private static CanonicalWriter Canonical(
        CanonicalWriter writer,
        Seq<AssemblyMember> members,
        AssemblyExecution execution,
        Seq<JoinStep> steps,
        int subassemblies,
        Seq<AssemblyEdge> precedence,
        Seq<AssemblyJoint> joints,
        Seq<JoinMeasure> results,
        Seq<BlockedCorridor> blocked,
        Seq<JoinStep> service) =>
        writer
            .Discriminant(execution.Cadence).Ordinal(execution.MaxParallel).Ordinal(subassemblies)
            .Rows(members, static (held, member) => member.Key.CanonicalBytes(held.Basis(member.Pose)))
            .Rows(joints, static (held, joint) => joint.Specification.CanonicalBytes(joint.Owner
                .CanonicalBytes(held.Ordinal(joint.Index))
                .String(joint.Connection.DetailKey.Value).String(joint.Connection.RealizingKey.Value)
                .Maybe(joint.Connection.At, static (row, at) => row.Coords(at.A).Coords(at.B))
                .Coords(joint.At.A).Coords(joint.At.B)))
            .Rows(steps, static (held, step) => step.CanonicalBytes(held))
            .Rows(precedence, static (held, edge) => held
                .Ordinal(edge.Source.Joint).Ordinal(edge.Source.Phase.Rank)
                .Ordinal(edge.Target.Joint).Ordinal(edge.Target.Phase.Rank).Ordinal(edge.Kind.Code))
            .Rows(blocked, static (held, row) => held.Ordinal(row.Joint).Ordinal(row.Corridor).Ordinal(row.Occluder))
            .Rows(results, static (held, result) => result.Boundary.Stability
                .CanonicalBytes(result.Boundary.Tolerance
                    .CanonicalBytes(held.Ordinal(result.Joint))
                    .Rows(result.Clearance, static (row, clearance) => row.Maybe(
                        clearance.Blocked, static (sink, zone) => sink.Rows(zone.Keepouts, static (leaf, loop) => loop.CanonicalBytes(leaf)))))
                .Maybe(result.Boundary.Robot, static (row, placement) => Placement(row, placement))
                .Rows(result.Boundary.Sight, static (row, sight) => row
                    .Ordinal(sight.Joint).Ordinal(sight.Corridor).Ordinal(sight.Occluder))
                .Rows(result.Resources, static (row, resource) => row.String(resource))
                .Double(result.Duration.As(DurationUnit.Second)))
            .Rows(service, static (held, step) => step.CanonicalBytes(held));

    private static readonly Op Key = Op.Of(name: nameof(AssemblyPlan));

    private static CanonicalWriter Placement(CanonicalWriter writer, CellPlacement result) =>
        Candidate(writer, result.Selected)
            .Rows(result.Ranked, static (held, candidate) => Candidate(held, candidate));

    private static CanonicalWriter Candidate(CanonicalWriter writer, CellPlacementCandidate candidate) =>
        candidate.Cell.Source.Switch(
            state: writer,
            library: static (held, source) => held.String(nameof(CellSource.Library)).String(source.Name).String(source.Meshes.Key),
            embedded: static (held, source) => held.String(nameof(CellSource.Embedded)).String(source.Xml))
        .Pose(candidate.Cell.BaseFrame).Pose(candidate.Cell.ToolFrame).Pose(candidate.NormalizedBaseFrame)
        .Rows(candidate.Joints, static (held, joints) => held.Rows(joints.ToSeq(), static (row, value) => row.Double(value)))
        .Rows(toSeq(candidate.Metrics).OrderBy(static row => row.Key.Key, StringComparer.Ordinal).ToSeq(),
            static (held, metric) => held.Discriminant(metric.Key).Double(metric.Value))
        .Double(candidate.Score);

    private static CanonicalWriter Pose(this CanonicalWriter writer, Plane frame) =>
        writer.Coords(frame.Origin).Coords(frame.XAxis).Coords(frame.YAxis);

    private static double Grid(Seq<AssemblyJoint> joints) =>
        joints.Map(static joint => joint.Specification.Fit.AlignmentMax)
            .Fold(Option<Length>.None, static (least, row) => least.Filter(held => held <= row).IfNone(row))
            .IfNone(Length.Zero)
            .As(LengthUnit.Millimeter);
}
```

## [04]-[PROJECTION]

- Owner: `AssemblyProjection` selects joining, traveler, inspection, handling, service, and evidence views; `AssemblyArtifact` carries the selected immutable plan.
- Output: projection preserves typed precedence, execution results, service order, and the already-minted plan key.
- Boundary: joining consumes joint and phase identity, handling consumes stability and subassembly identity, and service consumes only reversible steps; no consumer reconstructs those facts from prose or array order.

```csharp
// --- [PROJECTION] ----------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class AssemblyProjection {
    public static readonly AssemblyProjection Joining = new("joining");
    public static readonly AssemblyProjection Traveler = new("traveler");
    public static readonly AssemblyProjection Inspection = new("inspection");
    public static readonly AssemblyProjection Handling = new("handling");
    public static readonly AssemblyProjection Service = new("service");
    public static readonly AssemblyProjection Evidence = new("evidence");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssemblyArtifact {
    private AssemblyArtifact() { }

    public sealed record Joining(ContentKey Key, Seq<AssemblyJoint> Joints, Seq<JoinStep> Steps, Seq<AssemblyEdge> Precedence) : AssemblyArtifact;
    public sealed record Traveler(ContentKey Key, Seq<JoinStep> Steps, Seq<JoinMeasure> Results) : AssemblyArtifact;
    public sealed record Inspection(ContentKey Key, Seq<JoinMeasure> Results) : AssemblyArtifact;
    public sealed record Handling(ContentKey Key, Seq<JoinStep> Steps) : AssemblyArtifact;
    public sealed record Service(ContentKey Key, Seq<JoinStep> Steps) : AssemblyArtifact;
    public sealed record Evidence(ContentKey Key, AssemblyPlan Plan) : AssemblyArtifact;
}

internal static partial class Assemblies {
    internal static Fin<AssemblyArtifact> Project(AssemblyPlan plan, AssemblyProjection projection) =>
        Fin.Succ(projection.Switch<AssemblyArtifact>(
            joining: () => new AssemblyArtifact.Joining(plan.Key, plan.Joints, plan.Steps, plan.Precedence),
            traveler: () => new AssemblyArtifact.Traveler(plan.Key, plan.Steps, plan.Results),
            inspection: () => new AssemblyArtifact.Inspection(plan.Key, plan.Results),
            handling: () => new AssemblyArtifact.Handling(plan.Key,
                plan.Steps.Filter(static step => step.Phase == JoinPhase.Handle)),
            service: () => new AssemblyArtifact.Service(plan.Key, plan.ServiceOrder),
            evidence: () => new AssemblyArtifact.Evidence(plan.Key, plan)));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
