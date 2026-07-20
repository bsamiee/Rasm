# [RASM_FABRICATION_WELD_SEQUENCE]

`Sequence.Order` turns one admitted weld-and-assembly census into a precedence-safe, thermally feasible, motion-timed schedule. Candidate space derives from parameterized traversal kernels and physical segment bands; tack and deposit events advance one thickness-scaled thermal state, and each candidate carries predicted sweep, camber, twist, angular distortion, thermal excursions, robot warnings, and elapsed time. Selected `WeldSchedule` preserves the complete ranking evidence.

Precedence is a partial order, never a serial rank: `JointPrecedence` folds `AssemblyPlan.Precedence` into a per-joint depth, so joints sharing a depth interleave freely under the traversal arm while a real precedence path stays ordered. `TorchFrame.StationMm` is the station authority and `JointAction` stages against `Backgouge.BeforeSide`, so backgouging and backing removal land between the sides they gate rather than ahead of every deposit.

`WeldPlan`, `AssemblyPlan`, and `ProcessBudget.Joining` enter once through `SequenceRequest`; `WeldSchedule.TotalS` remains the estimation clock projection. Compiled robot programs contribute target timing, planned cycle duration, and warnings, while `Program.Errors` close aggregate admission without moving kinematics ownership; `QuikGraph` owns precedence and `CSparse` owns the inherent-strain solve over `PrecedenceKind`-weighted restraints.

## [01]-[INDEX]

- [02]-[SEQUENCE_REQUEST]: admission, dimensional policy, generated candidate space, and motion evidence.
- [03]-[SCHEDULE_FOLD]: graph reduction, thermal-resource fold, distortion solve, candidate ranking, and receipt projection.

## [02]-[SEQUENCE_REQUEST]

- Owner: `SequenceRequest` admits the aggregate correspondence between deposits, assembly nodes, thermal limits, policy, and optional compiled motion evidence.
- Cases: `DistortionOrder` is the closed traversal family; each case carries only the segment, stride, block, origin, direction, and side-barrier evidence its ordering arm consumes.
- Policy: `SequencePolicy` owns tack, thickness-scaled thermal, action-time, inherent-strain, feasibility-limit, candidate-generation, and multi-objective scoring values as generated invariant owners.
- Restraint: `InherentStrainLaw.RestraintStiffness` is a `PrecedenceKind`-keyed row map, so a datum constraint and a handling constraint contribute different coupling to the same solve.
- Bound: `CandidateLaw.Ceiling` truncates the generated product deterministically, so band, stride, block, and origin breadth grows without an unbounded candidate sweep.
- Entry: `Sequence.Order` accepts only `SequenceRequest`; decoded or foreign material re-enters through `SequenceRequest.Validate`.
- Packages: `Thinktecture.Runtime.Extensions` owns admission and closed dispatch; `UnitsNet` owns length, speed, temperature, energy, power, angle, and duration; `LanguageExt.Core` owns accumulated admission and immutable folds; `QuikGraph` owns precedence; `CSparse` owns sparse factorization; `Robots` owns compiled target timing.
- Growth: a traversal primitive is one `DistortionOrder` case, while operating breadth grows through parameter ranges on `CandidateLaw` without new named strategies or entrypoints.
- Payload: `WorkSeed` carries unresolved policy input before clock and thermal evaluation; `ScheduledWork` carries public timing and physical evidence after that fold, so their distinct payload timing keeps both shapes.
- Boundary: weld geometry, station, and realized heat input remain `WeldPlan` evidence, assembly remains the precedence authority, and robot compilation remains a kinematics concern.
- Exemption: generated admission and `RobotTiming.From` are boundary statements; `CandidateLaw.Generate` and `Sequence` graph, geometry, scheduling, and sparse folds are measured kernels.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CSparse;
using CSparse.Double.Factorization;
using CSparse.Storage;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Robots;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Joining;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DistortionOrder {
    private DistortionOrder() { }

    public sealed record Progression(Length Segment, bool Reverse, bool SideBarrier) : DistortionOrder;
    public sealed record Residue(Length Segment, int Stride, bool Reverse, bool SideBarrier) : DistortionOrder;
    public sealed record CenterOut(Length Segment, Length Origin, bool Reverse, bool SideBarrier) : DistortionOrder;
    public sealed record Block(Length Segment, int Size, bool Reverse, bool SideBarrier) : DistortionOrder;
    public sealed record Cascade(Length Segment, int Stride, bool Reverse, bool SideBarrier) : DistortionOrder;
    public sealed record Wandering(Length Segment, Length Origin, bool Reverse, bool SideBarrier) : DistortionOrder;

    public Length Band => Switch(
        progression: static value => value.Segment,
        residue: static value => value.Segment,
        centerOut: static value => value.Segment,
        block: static value => value.Segment,
        cascade: static value => value.Segment,
        wandering: static value => value.Segment);

    public Seq<WeldSegment> Arrange(Seq<WeldSegment> segments) => Switch(
        state: segments,
        progression: static (rows, order) => toSeq(rows.AsEnumerable()
            .OrderBy(static segment => segment.Precedence)
            .ThenBy(segment => order.SideBarrier ? segment.Side : 0)
            .ThenBy(segment => order.Reverse ? -segment.Station.Meters : segment.Station.Meters)),
        residue: static (rows, order) => toSeq(rows.AsEnumerable()
            .OrderBy(static segment => segment.Precedence)
            .ThenBy(segment => order.SideBarrier ? segment.Side : 0)
            .ThenBy(segment => segment.Sequence % order.Stride)
            .ThenBy(segment => order.Reverse ? -segment.Sequence : segment.Sequence)),
        centerOut: static (rows, order) => toSeq(rows.AsEnumerable()
            .OrderBy(static segment => segment.Precedence)
            .ThenBy(segment => order.SideBarrier ? segment.Side : 0)
            .ThenBy(segment => UnitMath.Abs(segment.Station - order.Origin))
            .ThenBy(segment => (segment.Station < order.Origin ? 0 : 1) ^ (order.Reverse ? 1 : 0))),
        block: static (rows, order) => toSeq(rows.AsEnumerable()
            .OrderBy(static segment => segment.Precedence)
            .ThenBy(segment => order.SideBarrier ? segment.Side : 0)
            .ThenBy(segment => (int)(segment.Station.Meters / order.Segment.Meters) / order.Size)
            .ThenBy(segment => order.Reverse ? -segment.Sequence : segment.Sequence)),
        cascade: static (rows, order) => toSeq(rows.AsEnumerable()
            .OrderBy(static segment => segment.Precedence)
            .ThenBy(segment => order.SideBarrier ? segment.Side : 0)
            .ThenBy(segment => segment.Pass.Layer + (segment.Sequence / order.Stride))
            .ThenBy(segment => order.Reverse ? -segment.Station.Meters : segment.Station.Meters)),
        wandering: static (rows, order) => toSeq(rows.AsEnumerable()
            .OrderBy(static segment => segment.Precedence)
            .ThenBy(segment => order.SideBarrier ? segment.Side : 0)
            .ThenBy(segment => UnitMath.Abs(segment.Station - order.Origin))
            .ThenBy(segment => ((segment.Pass.Layer + segment.Side) % 2) ^ (order.Reverse ? 1 : 0))));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct CandidateLaw {
    public Seq<Length> SegmentBands { get; }
    public Seq<int> Strides { get; }
    public Seq<int> BlockSizes { get; }
    public Seq<double> OriginFractions { get; }
    public bool ReversePairs { get; }
    public bool PreserveSideBarriers { get; }
    public int Ceiling { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<Length> segmentBands,
        ref Seq<int> strides,
        ref Seq<int> blockSizes,
        ref Seq<double> originFractions,
        ref bool reversePairs,
        ref bool preserveSideBarriers,
        ref int ceiling) =>
        validationError = segmentBands.IsEmpty
            || segmentBands.Exists(static value => !double.IsFinite(value.Meters) || value <= Length.Zero)
            || strides.IsEmpty || strides.Exists(static value => value <= 0)
            || blockSizes.IsEmpty || blockSizes.Exists(static value => value <= 0)
            || originFractions.IsEmpty || originFractions.Exists(static value => !double.IsFinite(value) || value is < 0.0 or > 1.0)
            || ceiling <= 0
            ? new ValidationError(message: "candidate-law")
            : null;

    public Fin<Seq<DistortionOrder>> Generate(Length extent) {
        Seq<bool> directions = ReversePairs ? Seq(false, true) : Seq(false);
        Seq<DistortionOrder> progression =
            from reverse in directions
            from segment in SegmentBands
            select (DistortionOrder)new DistortionOrder.Progression(
                UnitMath.Min(segment, extent),
                reverse,
                PreserveSideBarriers);
        Seq<DistortionOrder> strided =
            from reverse in directions
            from segment in SegmentBands
            from stride in Strides
            from order in Seq<DistortionOrder>(
                new DistortionOrder.Residue(UnitMath.Min(segment, extent), stride, reverse, PreserveSideBarriers),
                new DistortionOrder.Cascade(UnitMath.Min(segment, extent), stride, reverse, PreserveSideBarriers))
            select order;
        Seq<DistortionOrder> centered =
            from reverse in directions
            from segment in SegmentBands
            from origin in OriginFractions
            from order in Seq<DistortionOrder>(
                new DistortionOrder.CenterOut(
                    UnitMath.Min(segment, extent),
                    extent * origin,
                    reverse,
                    PreserveSideBarriers),
                new DistortionOrder.Wandering(
                    UnitMath.Min(segment, extent),
                    extent * origin,
                    reverse,
                    PreserveSideBarriers))
            select order;
        Seq<DistortionOrder> blocked =
            from reverse in directions
            from segment in SegmentBands
            from size in BlockSizes
            select (DistortionOrder)new DistortionOrder.Block(
                UnitMath.Min(segment, extent),
                size,
                reverse,
                PreserveSideBarriers);
        return !double.IsFinite(extent.Meters) || extent <= Length.Zero
            ? Fin.Fail<Seq<DistortionOrder>>(Error.New("candidate-law:extent"))
            : Fin.Succ((progression + strided + centered + blocked).Take(Ceiling).ToSeq());
    }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ThermalLaw {
    public Temperature Ambient { get; }
    public Temperature Peak { get; }
    public Temperature MinimumInterpass { get; }
    public Duration TauAtReference { get; }
    public Length ReferenceThickness { get; }
    public Duration ReheatAfter { get; }
    public Duration ReheatDuration { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Temperature ambient,
        ref Temperature peak,
        ref Temperature minimumInterpass,
        ref Duration tauAtReference,
        ref Length referenceThickness,
        ref Duration reheatAfter,
        ref Duration reheatDuration) =>
        validationError = Seq(ambient, peak, minimumInterpass)
                .Exists(static value => !double.IsFinite(value.DegreesCelsius))
            || Seq(tauAtReference, reheatAfter, reheatDuration)
                .Exists(static value => !double.IsFinite(value.Seconds))
            || !double.IsFinite(referenceThickness.Meters)
            || ambient >= minimumInterpass || minimumInterpass >= peak || tauAtReference <= Duration.Zero
            || referenceThickness <= Length.Zero || reheatAfter <= Duration.Zero || reheatDuration <= Duration.Zero
            ? new ValidationError(message: "thermal-law")
            : null;

    public Temperature Heated(Temperature initial, Energy delivered, Energy peakEnergy) {
        double fraction = Math.Clamp(delivered.Joules / peakEnergy.Joules, 0.0, 1.0);
        return new Temperature(
            initial.DegreesCelsius
            + (fraction * (Peak.DegreesCelsius - initial.DegreesCelsius)),
            TemperatureUnit.DegreeCelsius);
    }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct TackBand {
    public Length MaximumThickness { get; }
    public Length Pitch { get; }
    public double LengthFactor { get; }
    public Length MinimumLength { get; }
    public Energy MinimumEnergy { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length maximumThickness,
        ref Length pitch,
        ref double lengthFactor,
        ref Length minimumLength,
        ref Energy minimumEnergy) =>
        validationError = Seq(maximumThickness, pitch, minimumLength)
                .Exists(static value => !double.IsFinite(value.Meters))
            || !double.IsFinite(minimumEnergy.Joules) || !double.IsFinite(lengthFactor)
            || maximumThickness <= Length.Zero || pitch <= Length.Zero || lengthFactor <= 0.0
            || minimumLength <= Length.Zero || minimumEnergy <= Energy.Zero
            ? new ValidationError(message: "tack-band")
            : null;
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ActionDurations {
    public Duration PrepareGroove { get; }
    public Duration InstallBacking { get; }
    public Duration Backgouge { get; }
    public Duration RemoveBacking { get; }
    public Duration Inspect { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Duration prepareGroove,
        ref Duration installBacking,
        ref Duration backgouge,
        ref Duration removeBacking,
        ref Duration inspect) =>
        validationError = Seq(prepareGroove, installBacking, backgouge, removeBacking, inspect)
            .Exists(static value => !double.IsFinite(value.Seconds) || value < Duration.Zero)
            ? new ValidationError(message: "action-durations")
            : null;

    public Duration Resolve(JointAction action) => action.Switch(
        state: this,
        prepareGroove: static (durations, _) => durations.PrepareGroove,
        installBacking: static (durations, _) => durations.InstallBacking,
        backgouge: static (durations, _) => durations.Backgouge,
        removeBacking: static (durations, _) => durations.RemoveBacking);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DistortionObjective {
    public double Sweep { get; }
    public double Camber { get; }
    public double Twist { get; }
    public double Angular { get; }
    public double Time { get; }
    public double Thermal { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double sweep,
        ref double camber,
        ref double twist,
        ref double angular,
        ref double time,
        ref double thermal) {
        double total = sweep + camber + twist + angular + time + thermal;
        validationError = Seq(sweep, camber, twist, angular, time, thermal)
                .Exists(static value => !double.IsFinite(value) || value < 0.0)
            || !double.IsFinite(total) || total <= 0.0
            ? new ValidationError(message: "distortion-objective")
            : null;
    }
}

[ComplexValueObject]
public sealed partial class InherentStrainLaw {
    public Energy ReferenceHeat { get; }
    public Length LongitudinalAtReference { get; }
    public Length TransverseAtReference { get; }
    public Angle TwistAtReference { get; }
    public Angle AngularAtReference { get; }
    public double SelfStiffness { get; }
    public Map<PrecedenceKind, double> RestraintStiffness { get; }
    public Duration SequenceMemory { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Energy referenceHeat,
        ref Length longitudinalAtReference,
        ref Length transverseAtReference,
        ref Angle twistAtReference,
        ref Angle angularAtReference,
        ref double selfStiffness,
        ref Map<PrecedenceKind, double> restraintStiffness,
        ref Duration sequenceMemory) =>
        validationError = !double.IsFinite(referenceHeat.Joules)
            || Seq(longitudinalAtReference, transverseAtReference)
                .Exists(static value => !double.IsFinite(value.Meters))
            || Seq(twistAtReference, angularAtReference)
                .Exists(static value => !double.IsFinite(value.Radians))
            || !double.IsFinite(sequenceMemory.Seconds)
            || referenceHeat <= Energy.Zero || longitudinalAtReference < Length.Zero
            || transverseAtReference < Length.Zero || twistAtReference < Angle.Zero || angularAtReference < Angle.Zero
            || !double.IsFinite(selfStiffness) || selfStiffness <= 0.0
            || restraintStiffness.IsEmpty
            || restraintStiffness.Values.Exists(static value => !double.IsFinite(value) || value < 0.0)
            || sequenceMemory <= Duration.Zero
            ? new ValidationError(message: "inherent-strain-law")
            : null;

    public double Coupling(PrecedenceKind kind) => RestraintStiffness.Find(kind).IfNone(0.0);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SequenceLimits {
    public int ConsecutiveDeposits { get; }
    public Duration Elapsed { get; }
    public Length LinearDistortion { get; }
    public Angle AngularDistortion { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int consecutiveDeposits,
        ref Duration elapsed,
        ref Length linearDistortion,
        ref Angle angularDistortion) =>
        validationError = !double.IsFinite(elapsed.Seconds) || !double.IsFinite(linearDistortion.Meters)
            || !double.IsFinite(angularDistortion.Radians)
            || consecutiveDeposits <= 0 || elapsed <= Duration.Zero || linearDistortion <= Length.Zero
            || angularDistortion <= Angle.Zero
            ? new ValidationError(message: "sequence-limits")
            : null;
}

[ComplexValueObject]
public sealed partial class SequencePolicy {
    public CandidateLaw Candidates { get; }
    public ThermalLaw Thermal { get; }
    public Seq<TackBand> TackBands { get; }
    public ActionDurations Actions { get; }
    public InherentStrainLaw Distortion { get; }
    public SequenceLimits Limits { get; }
    public DistortionObjective Objective { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CandidateLaw candidates,
        ref ThermalLaw thermal,
        ref Seq<TackBand> tackBands,
        ref ActionDurations actions,
        ref InherentStrainLaw distortion,
        ref SequenceLimits limits,
        ref DistortionObjective objective) =>
        validationError = candidates == default || thermal == default || actions == default || distortion is null
            || limits == default || objective == default || tackBands.IsEmpty || tackBands.Exists(static band => band == default)
            ? new ValidationError(message: "sequence-policy")
            : null;
}

[ComplexValueObject]
public sealed partial class RobotTiming {
    public HashMap<(int Joint, int Pass, int Span), Duration> Segments { get; }
    public HashMap<(int Joint, int Pass), Duration> Cycles { get; }
    public Seq<string> Warnings { get; }
    public Seq<string> Errors { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HashMap<(int Joint, int Pass, int Span), Duration> segments,
        ref HashMap<(int Joint, int Pass), Duration> cycles,
        ref Seq<string> warnings,
        ref Seq<string> errors) =>
        validationError = segments.Values.Exists(static duration => !double.IsFinite(duration.Seconds) || duration < Duration.Zero)
            || cycles.Values.Exists(static duration => !double.IsFinite(duration.Seconds) || duration <= Duration.Zero)
            || segments.Keys.Exists(key => !cycles.ContainsKey((key.Joint, key.Pass)))
            || warnings.Exists(static warning => string.IsNullOrWhiteSpace(warning))
            || errors.Exists(static error => string.IsNullOrWhiteSpace(error))
            ? new ValidationError(message: "robot-timing")
            : null;

    // Program.Duration is the look-ahead planner's own cycle figure; it seeds a pass whose span index falls outside Targets.
    public static Fin<RobotTiming> From(Map<(int Joint, int Pass), Program> programs) {
        if (programs.Values.Exists(static program => program is null)) {
            return Fin.Fail<RobotTiming>(Error.New("robot-timing:null-program"));
        }
        var admitted = toSeq(programs.AsEnumerable()).Fold(
            (Segments: HashMap<(int Joint, int Pass, int Span), Duration>(),
                Cycles: HashMap<(int Joint, int Pass), Duration>(),
                Warnings: Seq<string>(),
                Errors: Seq<string>()),
            static (timing, row) => (
                toSeq(row.Value.Targets)
                    .Map((target, index) => (Key: (row.Key.Joint, row.Key.Pass, index), At: Duration.FromSeconds(target.DeltaTime)))
                    .Fold(timing.Segments, static (held, target) => held.SetItem(target.Key, target.At)),
                timing.Cycles.SetItem(row.Key, Duration.FromSeconds(row.Value.Duration)),
                timing.Warnings + toSeq(row.Value.Warnings),
                timing.Errors + toSeq(row.Value.Errors)));
        return RobotTiming.Validate(admitted.Segments, admitted.Cycles, admitted.Warnings, admitted.Errors, out RobotTiming timing) is { } error
            ? Fin.Fail<RobotTiming>(Error.New(error.Message))
            : Fin.Succ(timing);
    }
}

[ComplexValueObject]
public sealed partial class SequenceRequest {
    public WeldPlan Plan { get; }
    public AssemblyPlan Assembly { get; }
    public ProcessBudget.Joining Budget { get; }
    public SequencePolicy Policy { get; }
    public Option<RobotTiming> Robot { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref WeldPlan plan,
        ref AssemblyPlan assembly,
        ref ProcessBudget.Joining budget,
        ref SequencePolicy policy,
        ref Option<RobotTiming> robot) {
        if (plan is null || assembly is null || budget is null || policy is null) {
            validationError = new ValidationError(message: "sequence-request");
            return;
        }
        Seq<int> planJoints = plan.Passes.Map(static pass => pass.Joint).Distinct().ToSeq();
        Seq<int> assemblyJoints = assembly.Steps.Filter(static step => step.Phase == JoinPhase.Final)
            .Map(static step => step.Joint).Distinct().ToSeq();
        bool stationsValid = plan.Passes.ForAll(static pass => pass.Frames.Count == pass.Path.Count
            && pass.Frames.Count >= 2
            && pass.Frames.Zip(pass.Frames.Tail).ForAll(static pair =>
                double.IsFinite(pair.Item1.StationMm)
                && double.IsFinite(pair.Item2.StationMm)
                && pair.Item2.StationMm > pair.Item1.StationMm));
        Temperature ceiling = new(budget.InterpassTemp, TemperatureUnit.DegreeCelsius);
        validationError = planJoints.IsEmpty || !stationsValid
            || planJoints.Exists(joint => !assemblyJoints.Contains(joint))
            || !double.IsFinite(budget.InterpassTemp)
            || ceiling < policy.Thermal.MinimumInterpass || ceiling >= policy.Thermal.Peak
            || Seq(budget.CurrentA, budget.VoltageV).Exists(static value => !double.IsFinite(value) || value <= 0.0)
            || robot.Exists(timing => timing.Errors.Count > 0)
            ? new ValidationError(message: "sequence-request")
            : null;
    }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct WeldSegment(
    WeldPass Pass,
    int Sequence,
    int SourceSpan,
    Length SourceLength,
    int Side,
    int Precedence,
    Length Station,
    Length Length,
    Seq<Move> Path,
    Seq<TorchFrame> SourceFrames);

internal readonly record struct JointPrecedence(HashMap<int, int> Depth, Seq<AssemblyEdge> Restraints);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record WorkSeed {
    private WorkSeed() { }

    public sealed record Tack(WeldSegment Segment, TackBand Band, Length Length) : WorkSeed;
    public sealed record Preparation(JointAction Action) : WorkSeed;
    public sealed record Inspection(int Joint, Option<int> Pass) : WorkSeed;
    public sealed record Deposit(WeldSegment Segment) : WorkSeed;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScheduledWork {
    private ScheduledWork() { }

    public sealed record Tack(
        int Rank,
        WeldPass Pass,
        int Segment,
        Seq<Move> Path,
        Duration At,
        Duration Run,
        Energy Heat) : ScheduledWork;
    public sealed record Preparation(int Rank, JointAction Action, Duration At, Duration Run) : ScheduledWork;
    public sealed record Inspection(int Rank, int Joint, Option<int> Pass, Duration At, Duration Run) : ScheduledWork;
    public sealed record Deposit(
        int Rank,
        WeldPass Pass,
        int Segment,
        int SourceSpan,
        Seq<Move> Path,
        Seq<TorchFrame> SourceFrames,
        Duration At,
        Duration Wait,
        Duration Reheat,
        Duration Run,
        Temperature Start,
        Temperature End) : ScheduledWork;
}

public readonly record struct DistortionEvidence(
    Length Sweep,
    Length Camber,
    Angle Twist,
    Angle Angular,
    double SolverWork,
    int FactorNonZeros);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CandidateRejection {
    private CandidateRejection() { }

    public sealed record ConsecutiveDeposits(int Actual, int Limit) : CandidateRejection;
    public sealed record Elapsed(Duration Actual, Duration Limit) : CandidateRejection;
    public sealed record LinearDistortion(Length Actual, Length Limit) : CandidateRejection;
    public sealed record AngularDistortion(Angle Actual, Angle Limit) : CandidateRejection;

    public string Detail => Switch(
        consecutiveDeposits: static value => $"weld-sequence:consecutive:{value.Actual}/{value.Limit}",
        elapsed: static value => $"weld-sequence:elapsed:{value.Actual.Seconds}/{value.Limit.Seconds}",
        linearDistortion: static value => $"weld-sequence:linear:{value.Actual.Millimeters}/{value.Limit.Millimeters}",
        angularDistortion: static value => $"weld-sequence:angular:{value.Actual.Degrees}/{value.Limit.Degrees}");
}

public sealed record SequenceCandidate(
    DistortionOrder Order,
    Seq<ScheduledWork> Work,
    Duration Total,
    DistortionEvidence Distortion,
    Temperature InterpassCeiling,
    Temperature MaximumInterpass,
    Seq<string> Warnings,
    Seq<CandidateRejection> Rejections,
    double Score);

public sealed record WeldSchedule(
    Seq<ScheduledWork> Work,
    Duration Total,
    Temperature InterpassCeiling,
    Temperature MaximumInterpass,
    DistortionEvidence Distortion,
    Seq<SequenceCandidate> Candidates,
    Seq<string> Warnings) {
    public double TotalS => Total.Seconds;
}

internal readonly record struct ScheduleState(
    Duration Clock,
    HashMap<int, Duration> LastArc,
    HashMap<int, Temperature> Temperature,
    Temperature MaximumInterpass,
    Seq<ScheduledWork> Work,
    int Rank);

internal sealed record DistortionKernel(
    SparseCholesky Factor,
    HashMap<int, int> JointIndex,
    int Degrees);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Sequence {
    public static Fin<WeldSchedule> Order(SequenceRequest request) =>
        Optional(request).ToFin(Error.New("sequence-request:null"))
            .Bind(Schedule)
            .Bind(Select);

    private static Fin<Seq<SequenceCandidate>> Schedule(SequenceRequest request) =>
        from precedence in Precedence(request.Assembly)
        let segments = Segments(request.Plan, precedence.Depth)
        from _ in guard(!segments.IsEmpty, Error.New("weld-sequence:no-deposit-segments")).ToFin()
        let extent = UnitMath.Sum(segments, static segment => segment.Length, LengthUnit.Meter)
        let ceiling = new Temperature(request.Budget.InterpassTemp, TemperatureUnit.DegreeCelsius)
        from kernel in Factor(segments, precedence.Restraints, request.Policy.Distortion)
        from orders in request.Policy.Candidates.Generate(extent)
        from candidates in orders.Traverse(order => Candidate(request, segments, order, ceiling, kernel)).As()
        select candidates;

    private static Fin<WeldSchedule> Select(Seq<SequenceCandidate> candidates) =>
        candidates.Filter(static candidate => candidate.Rejections.IsEmpty)
            .OrderBy(static candidate => candidate.Score)
            .HeadOrNone()
            .ToFin(Infeasible(candidates))
            .Map(selected => new WeldSchedule(
                selected.Work,
                selected.Total,
                selected.InterpassCeiling,
                selected.MaximumInterpass,
                selected.Distortion,
                candidates,
                selected.Warnings));

    // Nearest-miss candidate's typed rejections are the only evidence a caller can act on when nothing is feasible.
    private static Error Infeasible(Seq<SequenceCandidate> candidates) =>
        candidates.OrderBy(static candidate => candidate.Rejections.Count)
            .ThenBy(static candidate => candidate.Score)
            .HeadOrNone()
            .Map(static nearest => nearest.Rejections.Fold(
                Error.New("weld-sequence:no-feasible-candidate"),
                static (combined, rejection) => combined + Error.New(rejection.Detail)))
            .IfNone(Error.New("weld-sequence:no-candidate-space"));

    private static Fin<SequenceCandidate> Candidate(
        SequenceRequest request,
        Seq<WeldSegment> segments,
        DistortionOrder order,
        Temperature ceiling,
        DistortionKernel kernel) {
        Seq<WeldSegment> arranged = order.Arrange(Subdivide(segments, order.Band));
        Seq<WorkSeed> seeds = Seeds(request, arranged);
        ScheduleState scheduled = seeds.Fold(
            new ScheduleState(
                Duration.Zero,
                HashMap<int, Duration>(),
                HashMap<int, Temperature>(),
                request.Policy.Thermal.Ambient,
                Seq<ScheduledWork>(),
                0),
            (state, seed) => Advance(request, state, seed, ceiling));
        Seq<string> warnings = request.Robot.Map(static timing => timing.Warnings).IfNone(Seq<string>());
        return InherentStrain(
                arranged,
                scheduled.Work,
                scheduled.Clock,
                request.Policy.Distortion,
                kernel)
            .Map(distortion => {
                Seq<CandidateRejection> rejections = Rejections(
                    request.Policy.Limits,
                    scheduled.Work,
                    scheduled.Clock,
                    distortion);
                double score = Score(request.Policy.Objective, scheduled.Clock, scheduled.MaximumInterpass, distortion);
                return new SequenceCandidate(
                    order,
                    scheduled.Work,
                    scheduled.Clock,
                    distortion,
                    ceiling,
                    scheduled.MaximumInterpass,
                    warnings,
                    rejections,
                    score);
            });
    }

    private static ScheduleState Advance(
        SequenceRequest request,
        ScheduleState state,
        WorkSeed seed,
        Temperature ceiling) => seed.Switch(
        state: (Request: request, State: state, Ceiling: ceiling),
        tack: static (context, tack) => Tack(context.Request, context.State, tack),
        preparation: static (context, preparation) => Preparation(context.Request, context.State, preparation),
        inspection: static (context, inspection) => Inspection(context.Request, context.State, inspection),
        deposit: static (context, deposit) => Deposit(context.Request, context.State, deposit.Segment, context.Ceiling));

    private static ScheduleState Tack(SequenceRequest request, ScheduleState state, WorkSeed.Tack tack) {
        Energy heat = UnitMath.Max(
            new Energy(tack.Segment.Pass.HeatInputKjMm * tack.Length.Millimeters, EnergyUnit.Kilojoule),
            tack.Band.MinimumEnergy);
        // A band-floored tack energy is delivered at the admitted arc power, so the floor lengthens the dwell.
        Duration run = UnitMath.Max(
            Duration.FromSeconds(60.0 * tack.Length.Millimeters / tack.Segment.Pass.TravelMmMin),
            Duration.FromSeconds(heat.Joules / (request.Budget.CurrentA * request.Budget.VoltageV)));
        Duration prior = state.LastArc.Find(tack.Segment.Pass.Joint).IfNone(Duration.Zero);
        Temperature priorTemperature = state.Temperature.Find(tack.Segment.Pass.Joint)
            .IfNone(request.Policy.Thermal.Ambient);
        Temperature start = TemperatureAt(
            state.Clock - prior,
            tack.Segment.Pass,
            request.Policy.Thermal,
            priorTemperature);
        Energy peakEnergy = new(
            tack.Segment.Pass.HeatInputKjMm * tack.Segment.Length.Millimeters,
            EnergyUnit.Kilojoule);
        Temperature endTemperature = request.Policy.Thermal.Heated(start, heat, peakEnergy);
        ScheduledWork.Tack work = new(
            state.Rank,
            tack.Segment.Pass,
            tack.Segment.Sequence,
            Window(tack.Segment, 0.0, tack.Length / tack.Segment.Length),
            state.Clock,
            run,
            heat);
        Duration end = state.Clock + run;
        return new ScheduleState(
            end,
            state.LastArc.SetItem(tack.Segment.Pass.Joint, end),
            state.Temperature.SetItem(tack.Segment.Pass.Joint, endTemperature),
            UnitMath.Max(state.MaximumInterpass, endTemperature),
            state.Work.Add(work),
            state.Rank + 1);
    }

    private static ScheduleState Preparation(SequenceRequest request, ScheduleState state, WorkSeed.Preparation preparation) {
        Duration run = request.Policy.Actions.Resolve(preparation.Action);
        ScheduledWork.Preparation work = new(state.Rank, preparation.Action, state.Clock, run);
        return state with { Clock = state.Clock + run, Work = state.Work.Add(work), Rank = state.Rank + 1 };
    }

    private static ScheduleState Inspection(SequenceRequest request, ScheduleState state, WorkSeed.Inspection inspection) {
        Duration run = request.Policy.Actions.Inspect;
        ScheduledWork.Inspection work = new(state.Rank, inspection.Joint, inspection.Pass, state.Clock, run);
        return state with { Clock = state.Clock + run, Work = state.Work.Add(work), Rank = state.Rank + 1 };
    }

    private static ScheduleState Deposit(
        SequenceRequest request,
        ScheduleState state,
        WeldSegment segment,
        Temperature ceiling) {
        Duration prior = state.LastArc.Find(segment.Pass.Joint).IfNone(Duration.Zero);
        Duration elapsed = state.Clock - prior;
        Temperature priorTemperature = state.Temperature.Find(segment.Pass.Joint)
            .IfNone(request.Policy.Thermal.Ambient);
        Temperature held = TemperatureAt(elapsed, segment.Pass, request.Policy.Thermal, priorTemperature);
        Duration cooling = Cooling(segment.Pass, ceiling, request.Policy.Thermal);
        Duration wait = UnitMath.Max(Duration.Zero, cooling - elapsed);
        Duration reheat = elapsed >= request.Policy.Thermal.ReheatAfter || held < request.Policy.Thermal.MinimumInterpass
            ? request.Policy.Thermal.ReheatDuration
            : Duration.Zero;
        Temperature start = reheat > Duration.Zero
            ? request.Policy.Thermal.MinimumInterpass
            : TemperatureAt(elapsed + wait, segment.Pass, request.Policy.Thermal, priorTemperature);
        Duration nominal = Duration.FromSeconds(60.0 * segment.Length.As(LengthUnit.Millimeter) / segment.Pass.TravelMmMin);
        // Span timing is authoritative; a compiled pass whose span index is absent still contributes its planned cycle.
        Duration run = request.Robot.ToSeq()
            .Bind(timing => timing.Segments.Find((segment.Pass.Joint, segment.Pass.Ordinal, segment.SourceSpan))
                    .Map(span => span * (segment.Length / segment.SourceLength)).ToSeq()
                + timing.Cycles.Find((segment.Pass.Joint, segment.Pass.Ordinal))
                    .Map(cycle => cycle * (segment.Length / Length.FromMillimeters(
                        segment.Pass.Frames.Last.StationMm - segment.Pass.Frames.Head.StationMm))).ToSeq())
            .HeadOrNone()
            .IfNone(nominal);
        Duration at = state.Clock;
        Duration end = at + wait + reheat + run;
        ScheduledWork.Deposit work = new(
            state.Rank,
            segment.Pass,
            segment.Sequence,
            segment.SourceSpan,
            segment.Path,
            segment.SourceFrames,
            at,
            wait,
            reheat,
            run,
            start,
            request.Policy.Thermal.Peak);
        return new ScheduleState(
            end,
            state.LastArc.SetItem(segment.Pass.Joint, end),
            state.Temperature.SetItem(segment.Pass.Joint, request.Policy.Thermal.Peak),
            UnitMath.Max(state.MaximumInterpass, start),
            state.Work.Add(work),
            state.Rank + 1);
    }

    private static Duration Cooling(WeldPass pass, Temperature ceiling, ThermalLaw law) {
        double numerator = law.Peak.DegreesCelsius - law.Ambient.DegreesCelsius;
        double denominator = ceiling.DegreesCelsius - law.Ambient.DegreesCelsius;
        Duration tau = TimeConstant(pass, law);
        return tau * Math.Log(numerator / denominator);
    }

    private static Temperature TemperatureAt(
        Duration elapsed,
        WeldPass pass,
        ThermalLaw law,
        Temperature initial) =>
        new(
            law.Ambient.DegreesCelsius
            + ((initial.DegreesCelsius - law.Ambient.DegreesCelsius)
                * Math.Exp(-elapsed.Seconds / TimeConstant(pass, law).Seconds)),
            TemperatureUnit.DegreeCelsius);

    private static Duration TimeConstant(WeldPass pass, ThermalLaw law) =>
        law.TauAtReference
        * (new Length(pass.ThicknessMm, LengthUnit.Millimeter) / law.ReferenceThickness)
        * pass.Position.CoolingScale;

    // Depth is the joint-graph longest-path level: joints sharing a depth carry no precedence path and stay interleavable.
    private static Fin<JointPrecedence> Precedence(AssemblyPlan assembly) {
        BidirectionalGraph<int, SEdge<int>> joints = new(allowParallelEdges: false);
        joints.AddVertexRange(assembly.Steps.Map(static step => step.Joint).Distinct().ToSeq());
        assembly.Precedence
            .Filter(static edge => edge.Source.Joint != edge.Target.Joint)
            .Iter(edge => joints.AddVerticesAndEdge(new SEdge<int>(edge.Source.Joint, edge.Target.Joint)));
        return joints.IsDirectedAcyclicGraph()
            ? Fin.Succ(new JointPrecedence(
                toSeq(joints.SourceFirstBidirectionalTopologicalSort()).Fold(
                    HashMap<int, int>(),
                    (depth, joint) => depth.AddOrUpdate(
                        joint,
                        toSeq(joints.InEdges(joint)).Fold(
                            0,
                            (deepest, edge) => Math.Max(deepest, 1 + depth.Find(edge.Source).IfNone(0))))),
                assembly.Precedence))
            : Fin.Fail<JointPrecedence>(
                new FabricationFault.AssemblyPrecedenceCyclic(joints.VertexCount, joints.EdgeCount));
    }

    // TorchFrame.StationMm is the admitted arc-length station; chord distance between move targets is not the seam position.
    private static Seq<WeldSegment> Segments(WeldPlan plan, HashMap<int, int> depth) =>
        plan.Passes.Bind(pass => pass.Path.Zip(pass.Path.Tail)
                .Map(static (pair, index) => (Start: pair.Item1, End: pair.Item2, Source: index))
                .Filter(static span => span.End is Move.Linear && span.Start is Move.Rapid or Move.Linear)
                .Map((span, index) => {
                    Length station = Length.FromMillimeters(pass.Frames[span.Source].StationMm);
                    Length length = Length.FromMillimeters(pass.Frames[span.Source + 1].StationMm) - station;
                    return new WeldSegment(
                        pass,
                        index,
                        span.Source,
                        length,
                        pass.Side,
                        depth.Find(pass.Joint).IfNone(int.MaxValue),
                        station,
                        length,
                        Seq<Move>(new Move.Rapid(Target(span.Start)), span.End),
                        pass.Frames.Skip(span.Source).Take(2));
                }))
            .OrderBy(static segment => segment.Precedence)
            .ToSeq();

    private static Seq<WeldSegment> Subdivide(Seq<WeldSegment> segments, Length maximum) =>
        segments.Bind(segment => {
            int count = Math.Max(1, (int)Math.Ceiling(segment.Length / maximum));
            return toSeq(Enumerable.Range(0, count)).Map(index => {
                double from = (double)index / count;
                double to = (double)(index + 1) / count;
                Length length = segment.Length / count;
                return segment with {
                    Station = segment.Station + (segment.Length * from),
                    Length = length,
                    Path = Window(segment, from, to),
                };
            });
        }).Map((segment, index) => segment with { Sequence = index });

    private static Seq<Move> Window(WeldSegment segment, double from, double to) {
        Point3d start = Target(segment.Path[0]);
        Point3d end = Target(segment.Path[1]);
        double feed = ((Move.Linear)segment.Path[1]).Feed;
        return Seq<Move>(
            new Move.Rapid(Interpolate(start, end, from)),
            new Move.Linear(Interpolate(start, end, to), feed));
    }

    private static Point3d Interpolate(Point3d start, Point3d end, double fraction) =>
        new(
            start.X + ((end.X - start.X) * fraction),
            start.Y + ((end.Y - start.Y) * fraction),
            start.Z + ((end.Z - start.Z) * fraction));

    // Arrangement is the schedule: joint actions stage around the deposits they gate instead of preceding every deposit.
    private static Seq<WorkSeed> Seeds(SequenceRequest request, Seq<WeldSegment> arranged) {
        HashMap<int, int> closes = arranged.Map(static (segment, index) => (segment.Pass.Joint, Index: index))
            .Fold(HashMap<int, int>(), static (held, row) => held.AddOrUpdate(row.Joint, row.Index));
        HashMap<(int Joint, int Pass), int> passCloses = arranged
            .Map(static (segment, index) => (Key: (segment.Pass.Joint, segment.Pass.Ordinal), Index: index))
            .Fold(HashMap<(int, int), int>(), static (held, row) => held.AddOrUpdate(row.Key, row.Index));
        return arranged.Map(static (segment, index) => (Segment: segment, Index: index)).Fold(
            (Seeds: Seq<WorkSeed>(), Opened: Seq<int>(), Gated: Seq<(int Joint, int Side)>()),
            (stream, row) => {
                int joint = row.Segment.Pass.Joint;
                (int Joint, int Side) gate = (joint, row.Segment.Side);
                return (
                    stream.Seeds
                        + (stream.Opened.Contains(joint) ? Seq<WorkSeed>() : Opening(request, arranged, joint))
                        + (stream.Gated.Contains(gate) ? Seq<WorkSeed>() : Gating(request, gate))
                        + Seq<WorkSeed>(row.Segment.Pass.Role == PassRole.Tack
                            ? new WorkSeed.Tack(row.Segment, TackFor(request.Policy, row.Segment.Pass), row.Segment.Length)
                            : new WorkSeed.Deposit(row.Segment))
                        + (row.Segment.Pass.Role.HoldForInspection
                                && passCloses.Find((joint, row.Segment.Pass.Ordinal))
                                    .Exists(last => last == row.Index)
                            ? Seq<WorkSeed>(new WorkSeed.Inspection(joint, Some(row.Segment.Pass.Ordinal)))
                            : Seq<WorkSeed>())
                        + (closes.Find(joint).Exists(last => last == row.Index) ? Closing(request, joint) : Seq<WorkSeed>()),
                    stream.Opened.Contains(joint) ? stream.Opened : stream.Opened.Add(joint),
                    stream.Gated.Contains(gate) ? stream.Gated : stream.Gated.Add(gate));
            }).Seeds;
    }

    private static Seq<WorkSeed> Opening(SequenceRequest request, Seq<WeldSegment> arranged, int joint) {
        Seq<WeldSegment> deposits = arranged.Filter(segment => segment.Pass.Joint == joint);
        TackBand band = deposits.HeadOrNone().Map(segment => TackFor(request.Policy, segment.Pass))
            .IfNone(request.Policy.TackBands.Head);
        return request.Plan.Actions
                .Filter(action => action.Joint == joint
                    && action is JointAction.PrepareGroove or JointAction.InstallBacking)
                .Map(static action => (WorkSeed)new WorkSeed.Preparation(action))
            + (request.Assembly.Steps.Exists(step => step.Joint == joint && step.Phase == JoinPhase.Tack)
                && !deposits.Exists(static segment => segment.Pass.Role == PassRole.Tack)
                ? deposits.Filter(segment => (segment.Station.Meters % band.Pitch.Meters) <= segment.Length.Meters)
                    .Map(segment => (WorkSeed)new WorkSeed.Tack(
                        segment,
                        band,
                        UnitMath.Min(
                            segment.Length,
                            UnitMath.Max(
                                band.MinimumLength,
                                new Length(segment.Pass.ThicknessMm * band.LengthFactor, LengthUnit.Millimeter)))))
                : Seq<WorkSeed>());
    }

    private static Seq<WorkSeed> Gating(SequenceRequest request, (int Joint, int Side) gate) =>
        request.Plan.Actions.Bind(action => action is JointAction.Backgouge backgouge
                && backgouge.Joint == gate.Joint && backgouge.BeforeSide == gate.Side
            ? Seq<WorkSeed>(new WorkSeed.Preparation(action), new WorkSeed.Inspection(gate.Joint, Option<int>.None))
            : Seq<WorkSeed>());

    private static Seq<WorkSeed> Closing(SequenceRequest request, int joint) =>
        request.Plan.Actions
            .Filter(action => action is JointAction.RemoveBacking && action.Joint == joint)
            .Map(static action => (WorkSeed)new WorkSeed.Preparation(action));

    private static TackBand TackFor(SequencePolicy policy, WeldPass pass) =>
        policy.TackBands.OrderBy(static band => band.MaximumThickness)
            .Find(band => new Length(pass.ThicknessMm, LengthUnit.Millimeter) <= band.MaximumThickness)
            .IfNone(policy.TackBands.OrderBy(static band => band.MaximumThickness).Last);

    private static Point3d Target(Move move) => move.Switch(
        rapid: static value => value.Target,
        linear: static value => value.Target,
        circular: static value => value.Target);

    // Consecutive deposits are counted over the emitted schedule, never the pre-schedule arrangement the interleave reorders.
    private static Seq<CandidateRejection> Rejections(
        SequenceLimits limits,
        Seq<ScheduledWork> work,
        Duration elapsed,
        DistortionEvidence distortion) {
        int consecutive = work.Fold(
            (Current: 0, Maximum: 0),
            static (held, row) => row.Switch(
                state: held,
                tack: static (state, _) => ResetStreak(state),
                preparation: static (state, _) => ResetStreak(state),
                inspection: static (state, _) => ResetStreak(state),
                deposit: static (state, _) => {
                    int current = state.Current + 1;
                    return (current, Math.Max(state.Maximum, current));
                })).Maximum;
        return Seq<Option<CandidateRejection>>(
                consecutive > limits.ConsecutiveDeposits
                    ? Some<CandidateRejection>(new CandidateRejection.ConsecutiveDeposits(
                        consecutive,
                        limits.ConsecutiveDeposits))
                    : Option<CandidateRejection>.None,
                elapsed > limits.Elapsed
                    ? Some<CandidateRejection>(new CandidateRejection.Elapsed(elapsed, limits.Elapsed))
                    : Option<CandidateRejection>.None,
                UnitMath.Max(distortion.Sweep, distortion.Camber) > limits.LinearDistortion
                    ? Some<CandidateRejection>(new CandidateRejection.LinearDistortion(
                        UnitMath.Max(distortion.Sweep, distortion.Camber),
                        limits.LinearDistortion))
                    : Option<CandidateRejection>.None,
                UnitMath.Max(distortion.Twist, distortion.Angular) > limits.AngularDistortion
                    ? Some<CandidateRejection>(new CandidateRejection.AngularDistortion(
                        UnitMath.Max(distortion.Twist, distortion.Angular),
                        limits.AngularDistortion))
                    : Option<CandidateRejection>.None)
            .Choose(identity);
    }

    private static (int Current, int Maximum) ResetStreak((int Current, int Maximum) state) =>
        (0, state.Maximum);

    // COO accumulates duplicate triplets, so the self term and every restraint coupling sum into one SPD stiffness.
    private static Fin<DistortionKernel> Factor(
        Seq<WeldSegment> segments,
        Seq<AssemblyEdge> restraints,
        InherentStrainLaw law) => Try.lift(() => {
            Seq<int> joints = segments.Map(static segment => segment.Pass.Joint).Distinct().ToSeq();
            HashMap<int, int> indexOf = joints.Map((joint, index) => (Joint: joint, Index: index))
                .Fold(HashMap<int, int>(), static (held, row) => held.AddOrUpdate(row.Joint, row.Index));
            int degrees = Math.Max(1, 4 * joints.Count);
            CoordinateStorage<double> coo = new(degrees, degrees, Math.Max(degrees, 9 * segments.Count));
            toSeq(Enumerable.Range(0, degrees)).Iter(index => coo.At(index, index, law.SelfStiffness));
            restraints.Filter(edge => edge.Source.Joint != edge.Target.Joint
                    && indexOf.ContainsKey(edge.Source.Joint) && indexOf.ContainsKey(edge.Target.Joint)
                    && law.Coupling(edge.Kind) > 0.0)
                .Iter(edge => toSeq(Enumerable.Range(0, 4)).Iter(axis => {
                    double coupling = law.Coupling(edge.Kind);
                    int source = (4 * indexOf[edge.Source.Joint]) + axis;
                    int target = (4 * indexOf[edge.Target.Joint]) + axis;
                    coo.At(source, source, coupling);
                    coo.At(target, target, coupling);
                    coo.At(source, target, -coupling);
                    coo.At(target, source, -coupling);
                }));
            return new DistortionKernel(
                SparseCholesky.Create(
                    CompressedColumnStorage<double>.OfIndexed(coo),
                    ColumnOrdering.MinimumDegreeAtPlusA),
                indexOf,
                degrees);
        })
            .Run()
            .MapFail(error => Error.New("weld-sequence:distortion-factorization") + error);

    private static Fin<DistortionEvidence> InherentStrain(
        Seq<WeldSegment> segments,
        Seq<ScheduledWork> work,
        Duration total,
        InherentStrainLaw law,
        DistortionKernel kernel) => Try.lift(() => {
        HashMap<(int Joint, int Pass, int Segment), Duration> chronology = work.Choose(static row => row.Switch(
                state: unit,
                tack: static (_, tack) => Some((
                    Key: (tack.Pass.Joint, tack.Pass.Ordinal, tack.Segment),
                    At: tack.At)),
                preparation: static (_, _) => Option<((int Joint, int Pass, int Segment) Key, Duration At)>.None,
                inspection: static (_, _) => Option<((int Joint, int Pass, int Segment) Key, Duration At)>.None,
                deposit: static (_, deposit) => Some((
                    Key: (deposit.Pass.Joint, deposit.Pass.Ordinal, deposit.Segment),
                    At: deposit.At + deposit.Wait + deposit.Reheat))))
            .Fold(
                HashMap<(int, int, int), Duration>(),
                static (held, row) => held.AddOrUpdate(row.Key, row.At));
        double[] loads = new double[kernel.Degrees];
        segments.Iter(segment => {
            int offset = 4 * kernel.JointIndex[segment.Pass.Joint];
            Energy heat = new(
                segment.Pass.HeatInputKjMm * segment.Length.As(LengthUnit.Millimeter),
                EnergyUnit.Kilojoule);
            Duration at = chronology.Find((segment.Pass.Joint, segment.Pass.Ordinal, segment.Sequence))
                .IfNone(Duration.Zero);
            double memory = Math.Exp(-(total - at).Seconds / law.SequenceMemory.Seconds);
            double source = (heat / law.ReferenceHeat) * memory;
            double sign = segment.Pass.Side % 2 == 0 ? -1.0 : 1.0;
            double arm = segment.Station / UnitMath.Max(segment.SourceLength, segment.Station + segment.Length);
            loads[offset] += source * law.LongitudinalAtReference.Meters;
            loads[offset + 1] += sign * source * law.TransverseAtReference.Meters;
            loads[offset + 2] += sign * source * law.TwistAtReference.Radians * arm;
            loads[offset + 3] += sign * source * law.AngularAtReference.Radians;
        });
        double[] displacement = new double[kernel.Degrees];
        kernel.Factor.Solve(loads, displacement);
        Length sweep = Length.FromMeters(displacement.Where((_, index) => index % 4 == 0).Select(Math.Abs).DefaultIfEmpty().Max());
        Length camber = Length.FromMeters(displacement.Where((_, index) => index % 4 == 1).Select(Math.Abs).DefaultIfEmpty().Max());
        Angle twist = new(displacement.Where((_, index) => index % 4 == 2).Select(Math.Abs).DefaultIfEmpty().Max(), AngleUnit.Radian);
        Angle angular = new(displacement.Where((_, index) => index % 4 == 3).Select(Math.Abs).DefaultIfEmpty().Max(), AngleUnit.Radian);
        double solverWork = displacement.Zip(loads, static (value, load) => value * load).Sum();
        return new DistortionEvidence(sweep, camber, twist, angular, solverWork, kernel.Factor.NonZerosCount);
    })
        .Run()
        .MapFail(error => Error.New("weld-sequence:distortion-solve") + error);

    private static double Score(
        DistortionObjective objective,
        Duration total,
        Temperature maximum,
        DistortionEvidence distortion) =>
        (objective.Sweep * distortion.Sweep.Millimeters)
        + (objective.Camber * distortion.Camber.Millimeters)
        + (objective.Twist * distortion.Twist.Degrees)
        + (objective.Angular * distortion.Angular.Degrees)
        + (objective.Time * total.Seconds)
        + (objective.Thermal * maximum.DegreesCelsius);
}
```

## [03]-[SCHEDULE_FOLD]

- Owner: `Sequence` derives the joint precedence depth, candidate schedules, inherent-strain evidence, and selected receipt through one ordered algebra.
- Graph: `BidirectionalGraph` admits the joint projection of `AssemblyPlan.Precedence`, `IsDirectedAcyclicGraph` gates the fold, and `SourceFirstBidirectionalTopologicalSort` supplies the order the `InEdges` longest-path fold turns into a depth per joint.
- Interleave: `Seeds` walks the arrangement itself, so joints at one depth alternate under the traversal arm; `PrepareGroove` and `InstallBacking` open a joint, `Backgouge` and its root inspection gate the side `BeforeSide` names, and `RemoveBacking` closes the joint after its last deposit.
- Station: `TorchFrame.StationMm` supplies segment station and length, so ordering, subdivision, and the distortion moment arm read the admitted seam position rather than a chord between move targets.
- Thermal: each deposit advances one immutable `ScheduleState`; work on other joints credits thickness- and position-scaled cooling, reheat occupies the clock, and each start, peak, ceiling, and maximum interpass temperature remains dimensioned. A band-floored tack dwells for the time its energy needs at `ProcessBudget.Joining` arc power.
- Motion: `RobotTiming` reads `Program.Targets`, `SystemTarget.DeltaTime`, `Program.Duration`, `Program.Warnings`, and `Program.Errors`; a compiled pass missing a span falls back to its planned cycle, and nominal travel duration remains the non-robot timing row.
- Distortion: policy-scaled longitudinal, transverse, twist, and angular sources couple through `PrecedenceKind`-weighted assembly restraints and decay by candidate chronology; one COO assembly finalizes to CSC and one `SparseCholesky` factor serves every candidate solve, while factor fill and every predicted mode ride each candidate.
- Receipt: `WeldSchedule` carries selected work, dimensional total, the frozen `TotalS` projection, interpass ceiling, distortion evidence, candidate ranking, and warnings.
- Boundary: typed infeasibility terminates before scheduling, and a fully infeasible space fails carrying the nearest-miss candidate's rejections; a feasible but inferior candidate remains evidence rather than disappearing from the result.
