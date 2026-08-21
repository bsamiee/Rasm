# [RASM_FABRICATION_WELD_SEQUENCE]

`Sequence.Order` turns one admitted weld-and-assembly census into a precedence-safe, thermally feasible, motion-timed schedule. Candidate space derives from parameterized traversal kernels and physical segment bands; tack and deposit events advance one thickness-scaled thermal state, and each candidate carries predicted displacement, thermal excursions, elapsed time, and warnings. The selected `WeldSchedule` preserves the complete ranking evidence.

The scheduling row is `Joining/weld`'s own `DepositSegment`: a station-indexed interval owning its frames and its one commanded move, so ordering, subdivision, and the distortion moment arm read the admitted seam position and never index-join a commanded path against a frame roster the arc program already lengthened. `DepositSegment.Window` is the ONE sub-interval geometry, so subdividing an orbital deposit for a thermal band keeps it circular.

Precedence is a partial order, never a serial rank: `JointPrecedence` folds `AssemblyPlan.Precedence` into a per-joint level through `DagShortestPathAlgorithm` under `DistanceRelaxers.CriticalDistance`, so joints sharing a level interleave freely under the traversal arm while a real precedence path stays ordered, and a cyclic census refuses carrying its strongly-connected component MEMBERS.

`DistortionSource` and `DistortionField` are this page's PRODUCED shapes: thermal shrinkage per pass, clamp preload, and stage release enter one stiffness assembly, and one `CholeskySparse` factor over `SparseMatrix.FromTriplets` serves every candidate solve — `Fixturing/assembly` tolerance chains and `Fixturing/setups` datum-transfer budgets consume the ONE field. Timing evidence enters as the provider-free `CellTiming` census `Kinematics/cell` publishes off its compiled program, keyed here onto the weld pass that compiled it; this page names no robot type and holds no provider crossing.

## [01]-[INDEX]

- [02]-[SEQUENCE_REQUEST]: admission, traversal vocabulary, dimensional policy, the generated candidate space, and the provider-free motion timing.
- [03]-[DISTORTION_FIELD]: the distortion source family, the stiffness assembly over the kernel sparse owners, and the displacement receipt every fixturing consumer reads.
- [04]-[SCHEDULE_FOLD]: precedence levels, segment derivation and subdivision, the thermal-resource fold, candidate ranking, and receipt projection.

## [02]-[SEQUENCE_REQUEST]

- Owner: `SequenceRequest` admits the aggregate correspondence between deposits, assembly nodes, thermal limits, policy, clamp preloads, and optional motion evidence; `ClampPreload` carries the gripped member, its force, and the release step that relaxes it; `SequencePolicy` owns tack, thickness-scaled thermal, action-time, inherent-strain, feasibility-limit, candidate-generation, and multi-objective scoring values; `DistortionOrder` closes the traversal family; `MotionTiming` owns the provider-free timing census.
- Cases: each `DistortionOrder` case carries only the segment band, stride, block, origin, and direction its ordering arm consumes.
- Law: ordering is a COMPARATOR COLUMN, not six bodies. Every arm sorts by precedence level, then by side barrier, then by the case's own primary and secondary projections — so a new traversal primitive supplies two projections rather than a re-spelled `OrderBy` chain whose first two keys drift from its siblings. `CandidateLaw.PreserveSideBarriers` owns the barrier and stamps one value across the whole generated space, so it arrives as the comparator's argument and no case re-declares a choice every candidate shares.
- Law: `CandidateLaw.Ceiling` truncates the generated product BEFORE materialization — the four families concatenate as a lazy stream and `Take` bounds it, so band, stride, block, and origin breadth grows without a full sweep the ceiling then discards.
- Law: `MotionTiming` is provider-free by construction. `MotionTiming.Of` keys the `CellTiming` census a compiled pass published onto the `MotionKey` that requested it, so the station ordinal stays the producer's and the weld identity stays this page's; a compiled program's own error set refuses at its producing boundary, so this page names no provider type at all.
- Exemption: generated admission hooks are boundary statements; `CandidateLaw.Generate` is the measured product kernel.
- Entry: `Sequence.Order` accepts only `SequenceRequest`; decoded or foreign material re-enters through `SequenceRequest.Admit`.
- Packages: Thinktecture.Runtime.Extensions owns admission and closed dispatch; UnitsNet owns length, speed, temperature, energy, angle, and duration; LanguageExt.Core owns accumulated admission and immutable folds; QuikGraph owns precedence; `Rasm.Element` supplies `AdmissionSlots`; `Rasm.Numerics` owns the sparse assembly and factorization; `Rasm.Fabrication.Process` supplies `RunWarning` and the fault band; `Kinematics/cell` supplies `CellTiming` and `CellSpanTiming`.
- Boundary: weld geometry, station, and realized heat input remain `WeldPlan` evidence, assembly remains the precedence authority, and motion compilation remains a kinematics concern.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Linq;
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.ShortestPath;
using Rasm.Domain;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Joining;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DistortionOrder {
    private DistortionOrder() { }

    public sealed record Progression(Length Segment, bool Reverse) : DistortionOrder;
    public sealed record Residue(Length Segment, int Stride, bool Reverse) : DistortionOrder;
    public sealed record CenterOut(Length Segment, Length Origin, bool Reverse) : DistortionOrder;
    public sealed record Block(Length Segment, int Size, bool Reverse) : DistortionOrder;
    public sealed record Cascade(Length Segment, int Stride, bool Reverse) : DistortionOrder;
    public sealed record Wandering(Length Segment, Length Origin, bool Reverse) : DistortionOrder;

    public Length Band => Switch(
        progression: static value => value.Segment,
        residue: static value => value.Segment,
        centerOut: static value => value.Segment,
        block: static value => value.Segment,
        cascade: static value => value.Segment,
        wandering: static value => value.Segment);

    // The two case-owned ordering projections. Precedence level and side barrier lead EVERY arm, so they live on the
    // shared comparator and a new traversal primitive supplies these two columns rather than a fourth sort chain.
    private Func<WeldSegment, double> Primary => Switch(
        progression: static order => segment => order.Reverse ? -segment.Station.Meters : segment.Station.Meters,
        residue: static order => segment => segment.Sequence % order.Stride,
        centerOut: static order => segment => UnitMath.Abs(segment.Station - order.Origin).Meters,
        block: static order => segment => (int)(segment.Station.Meters / order.Segment.Meters) / order.Size,
        cascade: static order => segment => segment.Pass.Layer + (segment.Sequence / (double)order.Stride),
        wandering: static order => segment => UnitMath.Abs(segment.Station - order.Origin).Meters);

    private Func<WeldSegment, double> Secondary => Switch(
        progression: static _ => static _ => 0.0,
        residue: static order => segment => order.Reverse ? -segment.Sequence : segment.Sequence,
        centerOut: static order => segment => (segment.Station < order.Origin ? 0 : 1) ^ (order.Reverse ? 1 : 0),
        block: static order => segment => order.Reverse ? -segment.Sequence : segment.Sequence,
        cascade: static order => segment => order.Reverse ? -segment.Station.Meters : segment.Station.Meters,
        wandering: static order => segment => ((segment.Pass.Layer + segment.Side) % 2) ^ (order.Reverse ? 1 : 0));

    // The side barrier is the POLICY's fact, not the traversal primitive's: `CandidateLaw` stamped one value on
    // every generated order, so a per-case column published a choice no candidate could differ on. It arrives as
    // the comparator's own argument and the six cases carry only what actually varies between them.
    public Seq<WeldSegment> Arrange(Seq<WeldSegment> segments, bool sideBarrier) {
        Func<WeldSegment, double> primary = Primary;
        Func<WeldSegment, double> secondary = Secondary;
        return toSeq(segments.AsEnumerable()
            .OrderBy(static segment => segment.Precedence)
            .ThenBy(segment => sideBarrier ? segment.Side : 0)
            .ThenBy(primary)
            .ThenBy(secondary));
    }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
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
        ref int ceiling) {
        if (segmentBands.IsEmpty || segmentBands.Exists(static value => !ValidityClaim.Positive(value.Meters).Holds)
            || strides.IsEmpty || strides.Exists(static value => value <= 0)
            || blockSizes.IsEmpty || blockSizes.Exists(static value => value <= 0)
            || originFractions.IsEmpty
            || originFractions.Exists(static value => !double.IsFinite(value) || value is < 0.0 or > 1.0)
            || ceiling <= 0)
            validationError = new ValidationError("candidate-law");
    }

    // The ceiling bounds the STREAM: the four families concatenate lazily and `Take` truncates before any candidate
    // materializes, so widening a breadth column never pays for orders the ceiling immediately discards.
    public Fin<Seq<DistortionOrder>> Generate(Length extent) {
        if (!ValidityClaim.Positive(extent.Meters).Holds)
            return Fin.Fail<Seq<DistortionOrder>>(
                new KernelFault.InvalidValue("sequence", "candidate-law:extent"));

        IEnumerable<bool> directions = ReversePairs ? [false, true] : [false];
        IEnumerable<Length> bands = SegmentBands.AsEnumerable().Select(band => UnitMath.Min(band, extent));
        IEnumerable<DistortionOrder> progression =
            from reverse in directions
            from band in bands
            select (DistortionOrder)new DistortionOrder.Progression(band, reverse);
        IEnumerable<DistortionOrder> strided =
            from reverse in directions
            from band in bands
            from stride in Strides.AsEnumerable()
            from order in new DistortionOrder[] {
                new DistortionOrder.Residue(band, stride, reverse),
                new DistortionOrder.Cascade(band, stride, reverse),
            }
            select order;
        IEnumerable<DistortionOrder> centered =
            from reverse in directions
            from band in bands
            from fraction in OriginFractions.AsEnumerable()
            from order in new DistortionOrder[] {
                new DistortionOrder.CenterOut(band, extent * fraction, reverse),
                new DistortionOrder.Wandering(band, extent * fraction, reverse),
            }
            select order;
        IEnumerable<DistortionOrder> blocked =
            from reverse in directions
            from band in bands
            from size in BlockSizes.AsEnumerable()
            select (DistortionOrder)new DistortionOrder.Block(band, size, reverse);
        return Fin.Succ(toSeq(progression.Concat(strided).Concat(centered).Concat(blocked).Take(Ceiling)));
    }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ThermalLaw {
    public Temperature Ambient { get; }
    public Temperature Peak { get; }
    public Temperature MinimumInterpass { get; }
    public NodaTime.Duration TauAtReference { get; }
    public Length ReferenceThickness { get; }
    public NodaTime.Duration ReheatAfter { get; }
    public NodaTime.Duration ReheatDuration { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Temperature ambient,
        ref Temperature peak,
        ref Temperature minimumInterpass,
        ref NodaTime.Duration tauAtReference,
        ref Length referenceThickness,
        ref NodaTime.Duration reheatAfter,
        ref NodaTime.Duration reheatDuration) {
        if (Seq(ambient, peak, minimumInterpass).Exists(static value => !double.IsFinite(value.DegreesCelsius))
            || ambient >= minimumInterpass || minimumInterpass >= peak
            || tauAtReference <= NodaTime.Duration.Zero || referenceThickness <= Length.Zero
            || reheatAfter <= NodaTime.Duration.Zero || reheatDuration <= NodaTime.Duration.Zero)
            validationError = new ValidationError("thermal-law");
    }

    // ONE heating law for every arc event. A deposit and a tack differ in delivered energy, never in the model, so
    // neither arm hard-sets the peak: a short tack under a long pass's peak energy lands below it.
    public Temperature Heated(Temperature initial, Energy delivered, Energy peakEnergy) => new(
        initial.DegreesCelsius
        + (Math.Clamp(delivered.Joules / peakEnergy.Joules, 0.0, 1.0) * (Peak.DegreesCelsius - initial.DegreesCelsius)),
        TemperatureUnit.DegreeCelsius);
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
        ref Energy minimumEnergy) {
        if (maximumThickness <= Length.Zero || pitch <= Length.Zero || !ValidityClaim.Positive(lengthFactor).Holds
            || minimumLength <= Length.Zero || minimumEnergy <= Energy.Zero)
            validationError = new ValidationError("tack-band");
    }
}

// Every joint action the plan emits carries a duration, so preheat and post-weld heat treatment occupy the clock
// they actually consume: an action family with no row here would silently shorten every schedule that stages it.
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ActionDurations {
    public NodaTime.Duration PrepareGroove { get; }
    public NodaTime.Duration InstallBacking { get; }
    public NodaTime.Duration Backgouge { get; }
    public NodaTime.Duration RemoveBacking { get; }
    public NodaTime.Duration Preheat { get; }
    public NodaTime.Duration PostWeldHeatTreatRamp { get; }
    public NodaTime.Duration Inspect { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref NodaTime.Duration prepareGroove,
        ref NodaTime.Duration installBacking,
        ref NodaTime.Duration backgouge,
        ref NodaTime.Duration removeBacking,
        ref NodaTime.Duration preheat,
        ref NodaTime.Duration postWeldHeatTreatRamp,
        ref NodaTime.Duration inspect) {
        if (Seq(prepareGroove, installBacking, backgouge, removeBacking, preheat, postWeldHeatTreatRamp, inspect)
            .Exists(static value => value < NodaTime.Duration.Zero))
            validationError = new ValidationError("action-durations");
    }

    // The soak an action DECLARES is the action's own fact; the policy row carries only the shop's ramp allowance,
    // so a two-hour stress relief occupies two hours plus ramp rather than the ramp alone.
    public NodaTime.Duration Resolve(JointAction action) => action.Switch(
        state: this,
        prepareGroove: static (durations, _) => durations.PrepareGroove,
        installBacking: static (durations, _) => durations.InstallBacking,
        backgouge: static (durations, _) => durations.Backgouge,
        removeBacking: static (durations, _) => durations.RemoveBacking,
        preheat: static (durations, _) => durations.Preheat,
        postWeldHeatTreat: static (durations, value) =>
            durations.PostWeldHeatTreatRamp + NodaTime.Duration.FromMinutes(value.SoakMinutes));
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
        Seq<double> weights = Seq(sweep, camber, twist, angular, time, thermal);
        if (weights.Exists(static value => !double.IsFinite(value) || value < 0.0)
            || !ValidityClaim.Positive(weights.Fold(0.0, static (sum, value) => sum + value)).Holds)
            validationError = new ValidationError("distortion-objective");
    }
}

[ComplexValueObject]
public sealed partial class InherentStrainLaw {
    public Energy ReferenceHeat { get; }
    public Length LongitudinalAtReference { get; }
    public Length TransverseAtReference { get; }
    public Length NormalAtReference { get; }
    public Angle TwistAtReference { get; }
    public Angle AngularAtReference { get; }
    public double SelfStiffness { get; }
    public Map<PrecedenceKind, double> RestraintStiffness { get; }
    public NodaTime.Duration SequenceMemory { get; }

    // A clamp preload and a stage release are load SOURCES on the same stiffness, so their reference magnitudes seat
    // here beside the thermal one and the assembly never mints a second law for a second source family.
    public Length PreloadAtReference { get; }
    public Force ReferencePreload { get; }
    public Length ReleaseAtReference { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Energy referenceHeat,
        ref Length longitudinalAtReference,
        ref Length transverseAtReference,
        ref Length normalAtReference,
        ref Angle twistAtReference,
        ref Angle angularAtReference,
        ref double selfStiffness,
        ref Map<PrecedenceKind, double> restraintStiffness,
        ref NodaTime.Duration sequenceMemory,
        ref Length preloadAtReference,
        ref Force referencePreload,
        ref Length releaseAtReference) {
        if (referenceHeat <= Energy.Zero || referencePreload <= Force.Zero
            || Seq(longitudinalAtReference, transverseAtReference, normalAtReference, preloadAtReference, releaseAtReference)
                .Exists(static value => !double.IsFinite(value.Meters) || value < Length.Zero)
            || Seq(twistAtReference, angularAtReference).Exists(static value => value < Angle.Zero)
            || !ValidityClaim.Positive(selfStiffness).Holds
            || restraintStiffness.IsEmpty
            || restraintStiffness.Values.Exists(static value => !double.IsFinite(value) || value < 0.0)
            || sequenceMemory <= NodaTime.Duration.Zero)
            validationError = new ValidationError("inherent-strain-law");
    }

    public double Coupling(PrecedenceKind kind) => RestraintStiffness.Find(kind).IfNone(0.0);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SequenceLimits {
    public int ConsecutiveDeposits { get; }
    public NodaTime.Duration Elapsed { get; }
    public Length LinearDistortion { get; }
    public Angle AngularDistortion { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int consecutiveDeposits,
        ref NodaTime.Duration elapsed,
        ref Length linearDistortion,
        ref Angle angularDistortion) {
        if (consecutiveDeposits <= 0 || elapsed <= NodaTime.Duration.Zero
            || linearDistortion <= Length.Zero || angularDistortion <= Angle.Zero)
            validationError = new ValidationError("sequence-limits");
    }
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
        ref DistortionObjective objective) {
        if (tackBands.IsEmpty)
            validationError = new ValidationError("sequence-policy");
    }

    public static Fin<SequencePolicy> Admit(
        CandidateLaw candidates,
        ThermalLaw thermal,
        Seq<TackBand> tackBands,
        ActionDurations actions,
        InherentStrainLaw distortion,
        SequenceLimits limits,
        DistortionObjective objective) =>
        Validate(candidates, thermal, tackBands, actions, distortion, limits, objective, out SequencePolicy policy)
            .Admitted(policy);
}

public readonly record struct MotionKey(int Joint, int Pass);

public readonly record struct MotionSpanTiming(MotionKey Key, int Segment, NodaTime.Duration Elapsed);

public readonly record struct MotionCycleTiming(MotionKey Key, NodaTime.Duration Cycle);

// The provider-free timing crossing. `Kinematics/cell` compiles a pass into station-indexed span elapsed times and a
// planned cycle and hands THESE rows; a compiled program's own error set refuses at that boundary, so this page names
// no provider type and the package keeps exactly one alias crossing.
[ComplexValueObject]
public sealed partial class MotionTiming {
    public Map<(int Joint, int Pass, int Segment), NodaTime.Duration> Segments { get; }
    public Map<MotionKey, NodaTime.Duration> Cycles { get; }
    public Seq<RunWarning> Warnings { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Map<(int Joint, int Pass, int Segment), NodaTime.Duration> segments,
        ref Map<MotionKey, NodaTime.Duration> cycles,
        ref Seq<RunWarning> warnings) {
        if (segments.Values.Exists(static value => value < NodaTime.Duration.Zero)
            || cycles.Values.Exists(static value => value <= NodaTime.Duration.Zero)
            || segments.Keys.Exists(key => !cycles.ContainsKey(new MotionKey(key.Joint, key.Pass))))
            validationError = new ValidationError("motion-timing");
    }

    public static Fin<MotionTiming> Admit(
        Seq<MotionSpanTiming> spans,
        Seq<MotionCycleTiming> cycles,
        Seq<RunWarning> warnings) =>
        Validate(
            spans.Fold(
                Map<(int, int, int), NodaTime.Duration>(),
                static (held, row) => held.AddOrUpdate((row.Key.Joint, row.Key.Pass, row.Segment), row.Elapsed)),
            cycles.Fold(
                Map<MotionKey, NodaTime.Duration>(),
                static (held, row) => held.AddOrUpdate(row.Key, row.Cycle)),
            warnings,
            out MotionTiming timing).Admitted(timing);

    // The ONE crossing entry: `Kinematics/cell` compiles a pass and publishes station-ordinal rows, this page attaches
    // the weld identity the compile was requested for. Neither side learns the other's key space.
    public static Fin<MotionTiming> Of(Seq<(MotionKey Key, CellTiming Timing)> compiled) =>
        Admit(
            compiled.Bind(static row => row.Timing.Spans.Map(span => new MotionSpanTiming(row.Key, span.Station, span.Elapsed))),
            compiled.Map(static row => new MotionCycleTiming(row.Key, row.Timing.Cycle)),
            compiled.Bind(static row => row.Timing.Warnings));

    // Span timing is authoritative; a compiled pass whose segment is absent still contributes its planned cycle,
    // pro-rated by the fraction of the pass's own station extent this segment covers.
    public Option<NodaTime.Duration> Elapsed(WeldSegment segment) =>
        Segments.Find((segment.Pass.Joint, segment.Pass.Ordinal, segment.Source.Ordinal))
            .Map(span => span * (segment.Length / Length.FromMillimeters(segment.Source.LengthMm)))
        | Cycles.Find(new MotionKey(segment.Pass.Joint, segment.Pass.Ordinal))
            .Map(cycle => cycle * (segment.Length / PassExtent(segment.Pass)));

    private static Length PassExtent(WeldPass pass) => Length.FromMillimeters(
        pass.Segments.Fold(0.0, static (sum, row) => sum + row.LengthMm));
}

// A clamp is a load SOURCE on the same stiffness the shrinkage loads: the member it grips, the force it holds, and
// the assembly step whose release relaxes it. A schedule modelling shrinkage alone reports the spring-back of a
// part nobody ever let go of, so the two remaining `DistortionSource` families enter here or they never load.
public readonly record struct ClampPreload(
    int Index,
    AssemblyMemberKey Member,
    Force Preload,
    Option<int> ReleaseStep);

[ComplexValueObject]
public sealed partial class SequenceRequest {
    public WeldPlan Plan { get; }
    public AssemblyPlan Assembly { get; }
    public ProcessBudget.Joining Budget { get; }
    public SequencePolicy Policy { get; }
    public Seq<ClampPreload> Clamps { get; }
    public Option<MotionTiming> Motion { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref WeldPlan plan,
        ref AssemblyPlan assembly,
        ref ProcessBudget.Joining budget,
        ref SequencePolicy policy,
        ref Seq<ClampPreload> clamps,
        ref Option<MotionTiming> motion) {
        Set<int> planJoints = toSet(plan.Passes.Map(static pass => pass.Joint));
        Set<int> assemblyJoints = toSet(assembly.Steps
            .Filter(static step => step.Phase == JoinPhase.Final)
            .Map(static step => step.Joint));
        Set<AssemblyMemberKey> members = toSet(assembly.Members.Map(static member => member.Key));
        Set<int> releases = toSet(assembly.Steps
            .Filter(static step => step.Phase == JoinPhase.Release)
            .Map(static step => step.Order));
        Temperature ceiling = new(budget.InterpassTemp, TemperatureUnit.DegreeCelsius);
        if (planJoints.IsEmpty
            || !planJoints.ForAll(assemblyJoints.Contains)
            || !plan.Passes.ForAll(static pass => !pass.Segments.IsEmpty)
            || ceiling < policy.Thermal.MinimumInterpass || ceiling >= policy.Thermal.Peak
            || !Seq(budget.CurrentA, budget.VoltageV).ForAll(static value => ValidityClaim.Positive(value).Holds)
            || clamps.Map(static clamp => clamp.Index).Distinct().Count != clamps.Count
            // A clamp naming a member the assembly never carried grips nothing, and a release naming a step that is
            // not a RELEASE phase relaxes a hold the plan never staged.
            || clamps.Exists(clamp => clamp.Preload <= Force.Zero
                || !members.Contains(clamp.Member)
                || clamp.ReleaseStep.Exists(step => !releases.Contains(step))))
            validationError = new ValidationError("sequence-request");
    }

    public static Fin<SequenceRequest> Admit(
        WeldPlan plan,
        AssemblyPlan assembly,
        ProcessBudget.Joining budget,
        SequencePolicy policy,
        Seq<ClampPreload> clamps,
        Option<MotionTiming> motion) =>
        Validate(plan, assembly, budget, policy, clamps, motion, out SequenceRequest request).Admitted(request);
}
```

## [03]-[DISTORTION_FIELD]

- Owner: `DistortionSource` closes the load-source family; `DisplacementRow` and `DistortionField` own the per-member field this page PRODUCES; `DistortionKernel` owns the one factored stiffness a candidate sweep reuses; `DistortionEvidence` owns the residual field summary every candidate ranks on.
- Cases: `DistortionSource.Thermal` carries the pass ordinal and its inherent shrinkage, `.Preload` the clamp index and its force, `.Release` the assembly step whose unclamping relaxes it — three sources loading ONE stiffness through one solve, so a member the schedule never welds still moves under the clamp that grips it.
- Law: `Fixturing/assembly` tolerance chains and `Fixturing/setups` datum-transfer budgets consume THIS receipt and no second field. A member row names the source that DOMINATES its own load — compared in the operator's own units, never by a per-case reading that would rank newtons against millimetres — so a consumer separating thermal shrinkage from clamp spring-back reads the discriminant rather than re-running the assembly with one family suppressed, and a member moving only through its restraints answers None rather than naming a stage that never ran.
- Law: the stiffness assembles through the kernel sparse owners — `SparseMatrix.FromTriplets` sums duplicate triplets into one SPD operator and `CholeskySparse.Of` factors it once per request, so every candidate pays one solve against a cached symbolic analysis and no raw CSparse type crosses this page.
- Auto: the residual witness rides the kernel's own `SolveReceipt`, so the field summary reports the solver's measured residual and factor fill rather than a re-derived figure.
- Output: `DistortionField` carries one row per assembly member — the three linear displacement components as one `Vector3d` and the dominating source — beside the `DistortionEvidence` summary holding the sweep, camber, twist, and angular extremes, the residual, and the factor fill. It takes no `*Receipt` name: it addresses no content key, names no producing plane, and carries no stamp, because scheduling clocks are DURATIONS from a zero this page never anchors to an instant, and a stamp here forges an evaluation moment nothing measured.
- Packages: `Rasm.Numerics` supplies `SparseMatrix.FromTriplets`, `CholeskySparse.Of`/`SolveDetailed`, `SolveReceipt`, and `Dimension.Create`; `Rasm.Domain` supplies `Op`.
- Boundary: the kernel holds the factor and the member index alone; the graph, the load vector, and every intermediate array stay inside the fold.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DistortionSource {
    private DistortionSource() { }

    public sealed record Thermal(int Pass, double InherentStrainMm) : DistortionSource;
    public sealed record Preload(int ClampIndex, double PreloadN) : DistortionSource;
    public sealed record Release(int StageIndex) : DistortionSource;
}

// `Source` is the load family DOMINATING this member's own displacement. A member that carries no load of its own
// and moves only through its restraints answers None, because naming a zero-magnitude release there would report a
// stage that never ran as the reason a datum moved.
public readonly record struct DisplacementRow(
    AssemblyMemberKey Member,
    Vector3d Displacement,
    Option<DistortionSource> Source);

public readonly record struct DistortionEvidence(
    Length Sweep,
    Length Camber,
    Angle Twist,
    Angle Angular,
    double Residual,
    double SolverWork,
    int FactorNonZeros);

public sealed record DistortionField(Seq<DisplacementRow> Rows, DistortionEvidence Summary);

// The factored stiffness and the member index it was assembled against. One factor serves every candidate solve,
// so a five-hundred-candidate sweep pays one symbolic analysis.
internal sealed record DistortionKernel(
    CholeskySparse Factor,
    Map<AssemblyMemberKey, int> MemberIndex,
    int Degrees);
```

## [04]-[SCHEDULE_FOLD]

- Owner: `Sequence` derives joint precedence levels, segment rows, candidate schedules, the displacement receipt, and the selected schedule through one ordered algebra; `WeldSegment` and `ScheduledWork` carry the pre- and post-clock shapes.
- Law: precedence LEVEL is the critical distance over the joint projection of `AssemblyPlan.Precedence`. One synthetic source edges every root, `DagShortestPathAlgorithm` under `DistanceRelaxers.CriticalDistance` measures the longest path to each joint, and the depths shift down one — so joints sharing a level carry no precedence path between them and stay interleavable. A cyclic census refuses carrying the strongly-connected component MEMBERS the detecting walk labelled, never a vertex-and-edge count a caller cannot act on.
- Law: `Seeds` walks the arrangement itself, so joints at one level alternate under the traversal arm. `JointAction.Stage` decides placement — an OPENING action precedes a joint's first deposit, a GATING action stages against the side `Backgouge.BeforeSide` names, and a CLOSING action follows its last deposit — so preheat opens the joint it heats and post-weld heat treatment occupies the clock after the joint closes.
- Law: each event advances one immutable `ScheduleState`. Work on other joints credits thickness- and position-scaled cooling, reheat occupies the clock, and BOTH arc arms heat through `ThermalLaw.Heated` against their own delivered energy — a deposit that hard-sets the peak makes every interpass reading fiction. Cooling reads the temperature actually held, so a joint already below the ceiling waits no time at all.
- Law: `DepositSegment.Window` is the ONE subdivision geometry. A band split re-cuts the owner's own interval, so an orbital deposit subdivides into arcs and a linear one into lines, and the schedule never straightens a bead to fit a thermal band.
- Auto: candidate rejection is typed — consecutive deposits, elapsed clock, linear distortion, angular distortion — and a fully infeasible space fails carrying the NEAREST-MISS candidate's rejections rather than a bare no-candidate error.
- Exemption: `Sequence.Assemble` and `Sequence.Solve` are the measured sparse kernels; `Sequence.Advance` and its four arms are the immutable state fold.
- Output: `WeldSchedule` carries selected work, dimensional total, the `TotalS` estimation projection, interpass ceiling and maximum, the `DistortionField`, the whole candidate ranking, and typed warnings.
- Packages: QuikGraph supplies `BidirectionalGraph`, `SEdge`, `IsDirectedAcyclicGraph`, `StronglyConnectedComponents`, `DagShortestPathAlgorithm`, and `DistanceRelaxers.CriticalDistance`.
- Boundary: typed infeasibility terminates before scheduling, and a feasible but inferior candidate remains evidence rather than disappearing from the result.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// The scheduling row wraps the weld owner's OWN station-indexed segment: station, length, and the commanded window
// all read `DepositSegment`, so nothing here reconstructs seam position from a move ordinal.
public readonly record struct WeldSegment(
    WeldPass Pass,
    DepositSegment Source,
    int Sequence,
    int Side,
    int Precedence,
    Length Station,
    Length Length,
    double FromFraction,
    double ToFraction,
    Seq<Move> Path,
    Seq<TorchFrame> Frames);

internal readonly record struct JointPrecedence(Map<int, int> Level, Seq<AssemblyEdge> Restraints);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record WorkSeed {
    private WorkSeed() { }

    public sealed record Tack(WeldSegment Segment, TackBand Band, Length Length, Seq<Move> Path) : WorkSeed;
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
        NodaTime.Duration At,
        NodaTime.Duration Run,
        Energy Heat,
        Temperature Start,
        Temperature End) : ScheduledWork;
    public sealed record Preparation(int Rank, JointAction Action, NodaTime.Duration At, NodaTime.Duration Run) : ScheduledWork;
    public sealed record Inspection(int Rank, int Joint, Option<int> Pass, NodaTime.Duration At, NodaTime.Duration Run) : ScheduledWork;
    public sealed record Deposit(
        int Rank,
        WeldPass Pass,
        int Segment,
        int SourceSegment,
        Seq<Move> Path,
        Seq<TorchFrame> Frames,
        NodaTime.Duration At,
        NodaTime.Duration Wait,
        NodaTime.Duration Reheat,
        NodaTime.Duration Run,
        Temperature Start,
        Temperature End) : ScheduledWork;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CandidateRejection {
    private CandidateRejection() { }

    public sealed record ConsecutiveDeposits(int Actual, int Limit) : CandidateRejection;
    public sealed record Elapsed(NodaTime.Duration Actual, NodaTime.Duration Limit) : CandidateRejection;
    public sealed record LinearDistortion(Length Actual, Length Limit) : CandidateRejection;
    public sealed record AngularDistortion(Angle Actual, Angle Limit) : CandidateRejection;

    public string Detail => Switch(
        consecutiveDeposits: static value => $"weld-sequence:consecutive:{value.Actual}/{value.Limit}",
        elapsed: static value => $"weld-sequence:elapsed:{value.Actual.TotalSeconds}/{value.Limit.TotalSeconds}",
        linearDistortion: static value => $"weld-sequence:linear:{value.Actual.Millimeters}/{value.Limit.Millimeters}",
        angularDistortion: static value => $"weld-sequence:angular:{value.Actual.Degrees}/{value.Limit.Degrees}");
}

public sealed record SequenceCandidate(
    DistortionOrder Order,
    Seq<ScheduledWork> Work,
    NodaTime.Duration Total,
    DistortionField Displacement,
    Temperature InterpassCeiling,
    Temperature MaximumInterpass,
    Seq<RunWarning> Warnings,
    Seq<CandidateRejection> Rejections,
    double Score);

public sealed record WeldSchedule(
    Seq<ScheduledWork> Work,
    NodaTime.Duration Total,
    Temperature InterpassCeiling,
    Temperature MaximumInterpass,
    DistortionField Displacement,
    Seq<SequenceCandidate> Candidates,
    Seq<RunWarning> Warnings) {
    public double TotalS => Total.TotalSeconds;
}

internal readonly record struct ScheduleState(
    NodaTime.Duration Clock,
    Map<int, NodaTime.Duration> LastArc,
    Map<int, Temperature> Temperature,
    Temperature MaximumInterpass,
    Seq<ScheduledWork> Work,
    int Rank);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Sequence {
    public static Fin<WeldSchedule> Order(SequenceRequest request) => Schedule(request).Bind(Select);

    private static Fin<Seq<SequenceCandidate>> Schedule(SequenceRequest request) =>
        from precedence in Precedence(request.Assembly)
        from segments in Segments(request.Plan, precedence.Level)
        from _ in guard(!segments.IsEmpty,
                (Error)new KernelFault.InvalidValue("sequence", "weld-sequence:no-deposit-segments"))
            .ToFin()
        let extent = UnitMath.Sum(segments, static segment => segment.Length, LengthUnit.Meter)
        let ceiling = new Temperature(request.Budget.InterpassTemp, TemperatureUnit.DegreeCelsius)
        from kernel in Assemble(request, segments, precedence.Restraints)
        from orders in request.Policy.Candidates.Generate(extent)
        from candidates in orders.Traverse(order => Candidate(request, segments, order, ceiling, kernel)).As()
        select candidates;

    private static Fin<WeldSchedule> Select(Seq<SequenceCandidate> candidates) =>
        toSeq(candidates.Filter(static candidate => candidate.Rejections.IsEmpty)
                .OrderBy(static candidate => candidate.Score))
            .Head
            .ToFin(Infeasible(candidates))
            .Map(selected => new WeldSchedule(
                selected.Work,
                selected.Total,
                selected.InterpassCeiling,
                selected.MaximumInterpass,
                selected.Displacement,
                candidates,
                selected.Warnings));

    // Nearest-miss candidate's typed rejections are the only evidence a caller can act on when nothing is feasible.
    private static Error Infeasible(Seq<SequenceCandidate> candidates) =>
        toSeq(candidates.OrderBy(static candidate => candidate.Rejections.Count)
                .ThenBy(static candidate => candidate.Score))
            .Head
            .Map(static nearest => nearest.Rejections.Fold(
                (Error)new KernelFault.InvalidValue("sequence", "weld-sequence:no-feasible-candidate"),
                static (combined, rejection) => combined
                    + FabricationFault.Inadmissible(FabConcern.Joining, rejection.Detail)))
            .IfNone(() => new KernelFault.InvalidValue("sequence", "weld-sequence:no-candidate-space"));

    private static Fin<SequenceCandidate> Candidate(
        SequenceRequest request,
        Seq<WeldSegment> segments,
        DistortionOrder order,
        Temperature ceiling,
        DistortionKernel kernel) =>
        from subdivided in Subdivide(segments, order.Band)
        let arranged = order.Arrange(subdivided, request.Policy.Candidates.PreserveSideBarriers)
        let scheduled = Seeds(request, arranged).Fold(
            new ScheduleState(
                NodaTime.Duration.Zero,
                Map<int, NodaTime.Duration>(),
                Map<int, Temperature>(),
                request.Policy.Thermal.Ambient,
                Seq<ScheduledWork>(),
                0),
            (state, seed) => Advance(request, state, seed, ceiling))
        from displacement in Solve(request, arranged, scheduled.Work, scheduled.Clock, kernel)
        select new SequenceCandidate(
            order,
            scheduled.Work,
            scheduled.Clock,
            displacement,
            ceiling,
            scheduled.MaximumInterpass,
            request.Motion.Map(static timing => timing.Warnings).IfNone(Seq<RunWarning>()),
            Rejections(request.Policy.Limits, scheduled.Work, scheduled.Clock, displacement.Summary),
            Score(request.Policy.Objective, scheduled.Clock, scheduled.MaximumInterpass, displacement.Summary));

    private static ScheduleState Advance(
        SequenceRequest request,
        ScheduleState state,
        WorkSeed seed,
        Temperature ceiling) => seed.Switch(
        state: (Request: request, State: state, Ceiling: ceiling),
        tack: static (context, tack) => Arc(
            context.Request, context.State, tack.Segment, tack.Path, tack.Length, Some(tack.Band), context.Ceiling),
        preparation: static (context, preparation) => Staged(
            context.State,
            rank => new ScheduledWork.Preparation(
                rank, preparation.Action, context.State.Clock, context.Request.Policy.Actions.Resolve(preparation.Action)),
            context.Request.Policy.Actions.Resolve(preparation.Action)),
        inspection: static (context, inspection) => Staged(
            context.State,
            rank => new ScheduledWork.Inspection(
                rank, inspection.Joint, inspection.Pass, context.State.Clock, context.Request.Policy.Actions.Inspect),
            context.Request.Policy.Actions.Inspect),
        deposit: static (context, deposit) => Arc(
            context.Request, context.State, deposit.Segment, deposit.Segment.Path, deposit.Segment.Length,
            Option<TackBand>.None, context.Ceiling));

    // A clock-only event advances rank and clock and touches no thermal column, so preparation and inspection share
    // one body rather than two that differ by a constructor.
    private static ScheduleState Staged(
        ScheduleState state,
        Func<int, ScheduledWork> work,
        NodaTime.Duration run) =>
        state with {
            Clock = state.Clock + run,
            Work = state.Work.Add(work(state.Rank)),
            Rank = state.Rank + 1,
        };

    // ONE arc-event body. A tack and a deposit differ in delivered energy, in whether an interpass wait applies, and
    // in which case they publish — never in the thermal model, so both heat through `ThermalLaw.Heated`.
    private static ScheduleState Arc(
        SequenceRequest request,
        ScheduleState state,
        WeldSegment segment,
        Seq<Move> path,
        Length length,
        Option<TackBand> band,
        Temperature ceiling) {
        ThermalLaw thermal = request.Policy.Thermal;
        NodaTime.Duration prior = state.LastArc.Find(segment.Pass.Joint).IfNone(NodaTime.Duration.Zero);
        NodaTime.Duration elapsed = state.Clock - prior;
        Temperature priorTemperature = state.Temperature.Find(segment.Pass.Joint).IfNone(thermal.Ambient);
        Temperature held = TemperatureAt(elapsed, segment.Pass, thermal, priorTemperature);
        // A joint already at or below the ceiling waits no time; the wait is what the ACTUAL held temperature owes.
        NodaTime.Duration wait = band.IsSome || held <= ceiling
            ? NodaTime.Duration.Zero
            : Cooling(segment.Pass, held, ceiling, thermal);
        NodaTime.Duration reheat = elapsed >= thermal.ReheatAfter || held < thermal.MinimumInterpass
            ? thermal.ReheatDuration
            : NodaTime.Duration.Zero;
        Temperature start = reheat > NodaTime.Duration.Zero
            ? thermal.MinimumInterpass
            : TemperatureAt(elapsed + wait, segment.Pass, thermal, priorTemperature);
        Energy delivered = band.Match(
            Some: row => UnitMath.Max(Heat(segment.Pass, length), row.MinimumEnergy),
            None: () => Heat(segment.Pass, length));
        Energy peak = Heat(segment.Pass, Length.FromMillimeters(segment.Source.LengthMm));
        Temperature end = thermal.Heated(start, delivered, peak);
        NodaTime.Duration nominal = NodaTime.Duration.FromSeconds(
            60.0 * length.As(LengthUnit.Millimeter) / segment.Pass.TravelMmMin);
        NodaTime.Duration run = band.Match(
            Some: _ => UnitMath.Max(
                nominal,
                NodaTime.Duration.FromSeconds(delivered.Joules / (request.Budget.CurrentA * request.Budget.VoltageV))),
            None: () => request.Motion.Bind(timing => timing.Elapsed(segment)).IfNone(nominal));
        NodaTime.Duration at = state.Clock;
        ScheduledWork work = band.IsSome
            ? new ScheduledWork.Tack(state.Rank, segment.Pass, segment.Sequence, path, at, run, delivered, start, end)
            : new ScheduledWork.Deposit(
                state.Rank, segment.Pass, segment.Sequence, segment.Source.Ordinal, path, segment.Frames,
                at, wait, reheat, run, start, end);
        NodaTime.Duration close = at + wait + reheat + run;
        return new ScheduleState(
            close,
            state.LastArc.AddOrUpdate(segment.Pass.Joint, close),
            state.Temperature.AddOrUpdate(segment.Pass.Joint, end),
            UnitMath.Max(state.MaximumInterpass, start),
            state.Work.Add(work),
            state.Rank + 1);
    }

    private static Energy Heat(WeldPass pass, Length length) =>
        new(pass.HeatInputKjMm * length.As(LengthUnit.Millimeter), EnergyUnit.Kilojoule);

    // Time for the HELD temperature to fall to the ceiling, never a constant band width: a joint that has already
    // cooled owes nothing and a hot one owes exactly its own exponential decay.
    private static NodaTime.Duration Cooling(WeldPass pass, Temperature held, Temperature ceiling, ThermalLaw law) =>
        TimeConstant(pass, law) * Math.Log(
            (held.DegreesCelsius - law.Ambient.DegreesCelsius) / (ceiling.DegreesCelsius - law.Ambient.DegreesCelsius));

    private static Temperature TemperatureAt(
        NodaTime.Duration elapsed,
        WeldPass pass,
        ThermalLaw law,
        Temperature initial) =>
        new(
            law.Ambient.DegreesCelsius
            + ((initial.DegreesCelsius - law.Ambient.DegreesCelsius)
                * Math.Exp(-elapsed.TotalSeconds / TimeConstant(pass, law).TotalSeconds)),
            TemperatureUnit.DegreeCelsius);

    private static NodaTime.Duration TimeConstant(WeldPass pass, ThermalLaw law) =>
        law.TauAtReference * (new Length(pass.ThicknessMm, LengthUnit.Millimeter) / law.ReferenceThickness);

    // Level is the critical distance from ONE synthetic source edged to every root, so a multi-root precedence forest
    // measures in one walk and every level shifts down by that source. A cycle publishes its component MEMBERS.
    private static Fin<JointPrecedence> Precedence(AssemblyPlan assembly) {
        const int Source = -1;
        BidirectionalGraph<int, SEdge<int>> joints = new(allowParallelEdges: false);
        joints.AddVertexRange(assembly.Steps.Map(static step => step.Joint).Distinct());
        assembly.Precedence
            .Filter(static edge => edge.Source.Joint != edge.Target.Joint)
            .Iter(edge => joints.AddVerticesAndEdge(new SEdge<int>(edge.Source.Joint, edge.Target.Joint)));
        if (!joints.IsDirectedAcyclicGraph()) {
            _ = joints.StronglyConnectedComponents(out IDictionary<int, int> components);
            return Fin.Fail<JointPrecedence>(new FabricationFault.AssemblyPrecedenceCyclic(
                toSeq(components)
                    .GroupBy(static row => row.Value)
                    .Filter(static group => group.Count() > 1)
                    .Bind(static group => toSeq(group).Map(static row => row.Key))
                    .ToArr()));
        }

        // The root scan MATERIALIZES before the source edges land: `Roots()` streams off the graph it is about to
        // gain edges into, so a lazy walk would enumerate a collection the same statement mutates.
        Arr<int> roots = joints.Roots().ToArr();
        roots.Iter(root => joints.AddVerticesAndEdge(new SEdge<int>(Source, root)));
        DagShortestPathAlgorithm<int, SEdge<int>> longest = new(joints, static _ => 1.0, DistanceRelaxers.CriticalDistance);
        longest.Compute(Source);
        return Fin.Succ(new JointPrecedence(
            toSeq(longest.Distances)
                .Filter(static row => row.Key != Source && double.IsFinite(row.Value))
                .Fold(Map<int, int>(), static (held, row) => held.AddOrUpdate(row.Key, (int)row.Value - 1)),
            assembly.Precedence));
    }

    // `DepositSegment` supplies station, length, frames, and the commanded window; the schedule row adds precedence
    // level and the sub-interval fractions a band split re-cuts against.
    private static Fin<Seq<WeldSegment>> Segments(WeldPlan plan, Map<int, int> level) =>
        plan.Passes
            .Bind(pass => pass.Segments.Map(segment => (Pass: pass, Segment: segment)))
            .Map(row => row.Segment
                .Window(0.0, 1.0, row.Pass.CommandedFeedMmMin)
                .Map(path => new WeldSegment(
                    row.Pass,
                    row.Segment,
                    Sequence: 0,
                    row.Pass.Side,
                    level.Find(row.Pass.Joint).IfNone(int.MaxValue),
                    Length.FromMillimeters(row.Segment.StartStationMm),
                    Length.FromMillimeters(row.Segment.LengthMm),
                    FromFraction: 0.0,
                    ToFraction: 1.0,
                    path,
                    row.Segment.Frames)))
            .Traverse(identity)
            .As()
            .Map(static rows => toSeq(rows.OrderBy(static segment => segment.Precedence))
                .Map(static (segment, index) => segment with { Sequence = index }));

    // A band split re-cuts the OWNER's interval, so a circular segment subdivides into arcs and a linear one into
    // lines — the geometry stays with `DepositSegment` and the schedule holds only fractions and stations.
    private static Fin<Seq<WeldSegment>> Subdivide(Seq<WeldSegment> segments, Length maximum) =>
        segments
            .Bind(segment => {
                int count = Math.Max(1, (int)Math.Ceiling(segment.Length / maximum));
                return toSeq(Range(0, count)).Map(index => (Segment: segment, Index: index, Count: count));
            })
            .Map(row => {
                double from = row.Segment.FromFraction
                    + ((row.Segment.ToFraction - row.Segment.FromFraction) * row.Index / row.Count);
                double to = row.Segment.FromFraction
                    + ((row.Segment.ToFraction - row.Segment.FromFraction) * (row.Index + 1) / row.Count);
                return row.Segment.Source
                    .Window(from, to, row.Segment.Pass.CommandedFeedMmMin)
                    .Map(path => row.Segment with {
                        Station = row.Segment.Station + (row.Segment.Length * ((double)row.Index / row.Count)),
                        Length = row.Segment.Length / row.Count,
                        FromFraction = from,
                        ToFraction = to,
                        Path = path,
                        Frames = Seq(row.Segment.Source.FrameAt(from), row.Segment.Source.FrameAt(to)),
                    });
            })
            .Traverse(identity)
            .As()
            .Map(static rows => rows.Map(static (segment, index) => segment with { Sequence = index }));

    // Arrangement is the schedule: joint actions stage around the deposits they gate instead of preceding every
    // deposit, and membership rides SETS so a thousand-segment arrangement pays no quadratic rescan.
    private static Seq<WorkSeed> Seeds(SequenceRequest request, Seq<WeldSegment> arranged) {
        Map<int, int> closes = arranged
            .Map(static (segment, index) => (segment.Pass.Joint, Index: index))
            .Fold(Map<int, int>(), static (held, row) => held.AddOrUpdate(row.Joint, row.Index));
        Map<(int Joint, int Pass), int> passCloses = arranged
            .Map(static (segment, index) => (Key: (segment.Pass.Joint, segment.Pass.Ordinal), Index: index))
            .Fold(Map<(int, int), int>(), static (held, row) => held.AddOrUpdate(row.Key, row.Index));
        return arranged.Map(static (segment, index) => (Segment: segment, Index: index)).Fold(
            (Seeds: Seq<WorkSeed>(), Opened: Set<int>(), Gated: Set<(int Joint, int Side)>()),
            (stream, row) => {
                int joint = row.Segment.Pass.Joint;
                (int Joint, int Side) gate = (joint, row.Segment.Side);
                return (
                    stream.Seeds
                        + (stream.Opened.Contains(joint) ? Seq<WorkSeed>() : Opening(request, arranged, joint))
                        + (stream.Gated.Contains(gate) ? Seq<WorkSeed>() : Gating(request, gate))
                        + Seq<WorkSeed>(row.Segment.Pass.Role == PassRole.Tack
                            ? new WorkSeed.Tack(
                                row.Segment, TackFor(request.Policy, row.Segment.Pass), row.Segment.Length, row.Segment.Path)
                            : new WorkSeed.Deposit(row.Segment))
                        + (row.Segment.Pass.Role.HoldForInspection
                                && passCloses.Find((joint, row.Segment.Pass.Ordinal)).Exists(last => last == row.Index)
                            ? Seq<WorkSeed>(new WorkSeed.Inspection(joint, Some(row.Segment.Pass.Ordinal)))
                            : Seq<WorkSeed>())
                        + (closes.Find(joint).Exists(last => last == row.Index) ? Closing(request, joint) : Seq<WorkSeed>()),
                    stream.Opened.TryAdd(joint),
                    stream.Gated.TryAdd(gate));
            }).Seeds;
    }

    // Preheat and groove preparation open the joint; the tack pattern seeds only where the plan itself carries no
    // tack role, so a plan that already programmed tacks is never double-tacked by policy.
    private static Seq<WorkSeed> Opening(SequenceRequest request, Seq<WeldSegment> arranged, int joint) {
        Seq<WeldSegment> deposits = arranged.Filter(segment => segment.Pass.Joint == joint);
        TackBand band = deposits.Head
            .Map(segment => TackFor(request.Policy, segment.Pass))
            .IfNone(() => request.Policy.TackBands[0]);
        return request.Plan.Actions
                .Filter(action => action.Joint == joint && action.Stage == JointStage.Opening)
                .Map(static action => (WorkSeed)new WorkSeed.Preparation(action))
            + (request.Assembly.Steps.Exists(step => step.Joint == joint && step.Phase == JoinPhase.Tack)
                && !deposits.Exists(static segment => segment.Pass.Role == PassRole.Tack)
                ? deposits
                    .Filter(segment => (segment.Station.Meters % band.Pitch.Meters) <= segment.Length.Meters)
                    .Map(segment => (WorkSeed)new WorkSeed.Tack(
                        segment,
                        band,
                        UnitMath.Min(
                            segment.Length,
                            UnitMath.Max(
                                band.MinimumLength,
                                new Length(segment.Pass.ThicknessMm * band.LengthFactor, LengthUnit.Millimeter))),
                        segment.Path))
                : Seq<WorkSeed>());
    }

    private static Seq<WorkSeed> Gating(SequenceRequest request, (int Joint, int Side) gate) =>
        request.Plan.Actions.Bind(action => action is JointAction.Backgouge backgouge
                && backgouge.Joint == gate.Joint && backgouge.BeforeSide == gate.Side
            ? Seq<WorkSeed>(new WorkSeed.Preparation(action), new WorkSeed.Inspection(gate.Joint, Option<int>.None))
            : Seq<WorkSeed>());

    private static Seq<WorkSeed> Closing(SequenceRequest request, int joint) =>
        request.Plan.Actions
            .Filter(action => action.Joint == joint && action.Stage == JointStage.Closing)
            .Map(static action => (WorkSeed)new WorkSeed.Preparation(action));

    private static TackBand TackFor(SequencePolicy policy, WeldPass pass) {
        Seq<TackBand> ordered = toSeq(policy.TackBands.OrderBy(static band => band.MaximumThickness));
        return ordered
            .Find(band => new Length(pass.ThicknessMm, LengthUnit.Millimeter) <= band.MaximumThickness)
            .IfNone(() => ordered[^1]);
    }

    // Consecutive deposits are counted over the EMITTED schedule, never the pre-schedule arrangement the interleave
    // reorders, and each limit that trips names its own measured figure beside the bound it crossed.
    private static Seq<CandidateRejection> Rejections(
        SequenceLimits limits,
        Seq<ScheduledWork> work,
        NodaTime.Duration elapsed,
        DistortionEvidence field) {
        int consecutive = work.Fold(
            (Current: 0, Maximum: 0),
            static (held, row) => row.Switch(
                state: held,
                tack: static (state, _) => (0, state.Maximum),
                preparation: static (state, _) => (0, state.Maximum),
                inspection: static (state, _) => (0, state.Maximum),
                deposit: static (state, _) => (state.Current + 1, Math.Max(state.Maximum, state.Current + 1)))).Maximum;
        Length linear = UnitMath.Max(field.Sweep, field.Camber);
        Angle angular = UnitMath.Max(field.Twist, field.Angular);
        return Seq<Option<CandidateRejection>>(
                consecutive > limits.ConsecutiveDeposits
                    ? Some<CandidateRejection>(new CandidateRejection.ConsecutiveDeposits(consecutive, limits.ConsecutiveDeposits))
                    : Option<CandidateRejection>.None,
                elapsed > limits.Elapsed
                    ? Some<CandidateRejection>(new CandidateRejection.Elapsed(elapsed, limits.Elapsed))
                    : Option<CandidateRejection>.None,
                linear > limits.LinearDistortion
                    ? Some<CandidateRejection>(new CandidateRejection.LinearDistortion(linear, limits.LinearDistortion))
                    : Option<CandidateRejection>.None,
                angular > limits.AngularDistortion
                    ? Some<CandidateRejection>(new CandidateRejection.AngularDistortion(angular, limits.AngularDistortion))
                    : Option<CandidateRejection>.None)
            .Choose(identity);
    }

    // Five degrees of freedom per assembly MEMBER: three linear components the receipt row publishes as one vector,
    // and the twist and angular modes the field summary carries. Duplicate triplets SUM in the kernel assembly, so
    // the self term and every restraint coupling compose into one SPD operator without a hand accumulator.
    private const int MemberDegrees = 5;

    private static Fin<DistortionKernel> Assemble(
        SequenceRequest request,
        Seq<WeldSegment> segments,
        Seq<AssemblyEdge> restraints) {
        InherentStrainLaw law = request.Policy.Distortion;
        Map<int, AssemblyMemberKey> owners = request.Assembly.Joints.Fold(
            Map<int, AssemblyMemberKey>(),
            static (held, joint) => held.AddOrUpdate(joint.Index, joint.Owner));
        // Welded members AND clamped ones: a member the schedule never deposits on still moves under the clamp that
        // grips it, so leaving it out of the index drops its whole load family from the operator.
        Seq<AssemblyMemberKey> members = (segments
                .Bind(segment => owners.Find(segment.Pass.Joint).ToSeq())
            + request.Clamps.Map(static clamp => clamp.Member))
            .Distinct()
            .ToSeq();
        Map<AssemblyMemberKey, int> index = members
            .Map(static (member, ordinal) => (Member: member, Ordinal: ordinal))
            .Fold(Map<AssemblyMemberKey, int>(), static (held, row) => held.AddOrUpdate(row.Member, row.Ordinal));
        int degrees = Math.Max(MemberDegrees, MemberDegrees * members.Count);
        IEnumerable<(int Row, int Col, double Value)> diagonal = Range(0, degrees)
            .Select(slot => (slot, slot, law.SelfStiffness));
        IEnumerable<(int Row, int Col, double Value)> couplings = restraints
            .Filter(edge => edge.Source.Joint != edge.Target.Joint && law.Coupling(edge.Kind) > 0.0)
            .Bind(edge => Pair(owners, index, edge, law.Coupling(edge.Kind)))
            .AsEnumerable();
        return SparseMatrix
            .FromTriplets(Dimension.Create(degrees), Dimension.Create(degrees), diagonal.Concat(couplings), Key)
            .Bind(stiffness => CholeskySparse.Of(stiffness, key: Key))
            .Map(factor => new DistortionKernel(factor, index, degrees));
    }

    // A restraint couples two members on every degree of freedom: the two diagonal terms and the two off-diagonal
    // terms sum into the assembly, so the operator stays symmetric positive-definite under the self term.
    private static Seq<(int Row, int Col, double Value)> Pair(
        Map<int, AssemblyMemberKey> owners,
        Map<AssemblyMemberKey, int> index,
        AssemblyEdge edge,
        double coupling) =>
        (from source in owners.Find(edge.Source.Joint).Bind(index.Find)
         from target in owners.Find(edge.Target.Joint).Bind(index.Find)
         where source != target
         select toSeq(Range(0, MemberDegrees)).Bind(axis =>
             Seq(((MemberDegrees * source) + axis, (MemberDegrees * source) + axis, coupling),
                 ((MemberDegrees * target) + axis, (MemberDegrees * target) + axis, coupling),
                 ((MemberDegrees * source) + axis, (MemberDegrees * target) + axis, -coupling),
                 ((MemberDegrees * target) + axis, (MemberDegrees * source) + axis, -coupling))))
        .IfNone(Seq<(int, int, double)>());

    // Every source family loads ONE stiffness: thermal shrinkage decays by candidate chronology, clamp preload rides
    // its declared force, and a release stage relaxes what it held. The dominating source per member is the row's
    // own discriminant, so a fixturing consumer separates spring-back from shrinkage without a second solve.
    private static Fin<DistortionField> Solve(
        SequenceRequest request,
        Seq<WeldSegment> segments,
        Seq<ScheduledWork> work,
        NodaTime.Duration total,
        DistortionKernel kernel) {
        InherentStrainLaw law = request.Policy.Distortion;
        Map<int, AssemblyMemberKey> owners = request.Assembly.Joints.Fold(
            Map<int, AssemblyMemberKey>(),
            static (held, joint) => held.AddOrUpdate(joint.Index, joint.Owner));
        Map<(int Joint, int Pass, int Segment), NodaTime.Duration> chronology = work.Fold(
            Map<(int, int, int), NodaTime.Duration>(),
            static (held, row) => row.Switch(
                state: held,
                tack: static (map, tack) => map.AddOrUpdate((tack.Pass.Joint, tack.Pass.Ordinal, tack.Segment), tack.At),
                preparation: static (map, _) => map,
                inspection: static (map, _) => map,
                deposit: static (map, value) => map.AddOrUpdate(
                    (value.Pass.Joint, value.Pass.Ordinal, value.Segment), value.At + value.Wait + value.Reheat)));
        Seq<(int Slot, double Load, DistortionSource Source)> loads = segments.Bind(segment =>
            owners.Find(segment.Pass.Joint).Bind(kernel.MemberIndex.Find).Map(member => {
                Energy heat = Heat(segment.Pass, segment.Length);
                NodaTime.Duration at = chronology
                    .Find((segment.Pass.Joint, segment.Pass.Ordinal, segment.Sequence))
                    .IfNone(NodaTime.Duration.Zero);
                double memory = Math.Exp(-(total - at).TotalSeconds / law.SequenceMemory.TotalSeconds);
                double source = (heat / law.ReferenceHeat) * memory;
                double sign = segment.Pass.Side % 2 == 0 ? -1.0 : 1.0;
                double arm = segment.Station / UnitMath.Max(
                    Length.FromMillimeters(segment.Source.LengthMm), segment.Station + segment.Length);
                int offset = MemberDegrees * member;
                DistortionSource discriminant = new DistortionSource.Thermal(
                    segment.Pass.Ordinal, source * law.LongitudinalAtReference.Millimeters);
                return Seq(
                    (offset, source * law.LongitudinalAtReference.Meters, discriminant),
                    (offset + 1, sign * source * law.TransverseAtReference.Meters, discriminant),
                    (offset + 2, sign * source * law.NormalAtReference.Meters, discriminant),
                    (offset + 3, sign * source * law.TwistAtReference.Radians * arm, discriminant),
                    (offset + 4, sign * source * law.AngularAtReference.Radians, discriminant));
            }).IfNone(Seq<(int, double, DistortionSource)>()));
        // A clamp presses its member against the fixture for as long as it holds, and its RELEASE relaxes what it
        // held — two families over one force, so the schedule reports the spring-back a shop actually measures at
        // unclamping rather than a shrinkage field the fixture was still restraining.
        Seq<(int Slot, double Load, DistortionSource Source)> restraint = request.Clamps.Bind(clamp =>
            kernel.MemberIndex.Find(clamp.Member).Map(member => {
                double held = clamp.Preload / law.ReferencePreload;
                int offset = MemberDegrees * member;
                DistortionSource pressing = new DistortionSource.Preload(clamp.Index, clamp.Preload.Newtons);
                Seq<(int, double, DistortionSource)> preload = Seq(
                    (offset + 1, held * law.PreloadAtReference.Meters, pressing),
                    (offset + 2, held * law.PreloadAtReference.Meters, pressing));
                return preload + clamp.ReleaseStep.Map(step => {
                    DistortionSource relaxing = new DistortionSource.Release(step);
                    return Seq<(int, double, DistortionSource)>(
                        (offset + 1, -held * law.ReleaseAtReference.Meters, relaxing),
                        (offset + 2, -held * law.ReleaseAtReference.Meters, relaxing));
                }).IfNone(Seq<(int, double, DistortionSource)>());
            }).IfNone(Seq<(int, double, DistortionSource)>()));
        double[] vector = (loads + restraint).Fold(
            new double[kernel.Degrees],
            static (held, row) => { held[row.Slot] += row.Load; return held; });
        return kernel.Factor
            .SolveDetailed(new Arr<double>(vector), Key)
            .Map(solved => Measured(kernel, loads + restraint, vector, solved));
    }

    // ONE named operation identity for the whole page, so an assembly, a factorization, and a solve refusal all
    // attribute to `Sequence` instead of three anonymous keys a caller cannot correlate across a candidate sweep.
    private static readonly Op Key = Op.Of(name: nameof(Sequence));

    // ONE pass over the solution vector yields every field extreme and the solver work, reading the already-summed
    // load slot rather than re-scanning the source rows per degree of freedom.
    private static DistortionField Measured(
        DistortionKernel kernel,
        Seq<(int Slot, double Load, DistortionSource Source)> loads,
        double[] vector,
        SolveReceipt solved) {
        // Dominance compares the LOAD each row actually placed on the operator, in the operator's own units — a
        // per-case reading compares millimetres of shrinkage against newtons of preload and elects by whichever
        // family happens to be spelled in the larger number.
        Map<int, (double Load, DistortionSource Source)> dominating = loads.Fold(
            Map<int, (double Load, DistortionSource Source)>(),
            static (held, row) => held.AddOrUpdate(
                row.Slot / MemberDegrees,
                existing => existing.Load >= Math.Abs(row.Load) ? existing : (Math.Abs(row.Load), row.Source),
                (Math.Abs(row.Load), row.Source)));
        Seq<DisplacementRow> rows = toSeq(kernel.MemberIndex).Map(entry => new DisplacementRow(
            entry.Key,
            new Vector3d(
                solved.Solution[MemberDegrees * entry.Value],
                solved.Solution[(MemberDegrees * entry.Value) + 1],
                solved.Solution[(MemberDegrees * entry.Value) + 2]),
            dominating.Find(entry.Value).Map(static held => held.Source)));
        (double Sweep, double Camber, double Twist, double Angular, double Work) extremes = toSeq(solved.Solution)
            .Map(static (value, slot) => (Value: value, Slot: slot))
            .Fold(
                (Sweep: 0.0, Camber: 0.0, Twist: 0.0, Angular: 0.0, Work: 0.0),
                (held, row) => (
                    row.Slot % MemberDegrees == 0 ? Math.Max(held.Sweep, Math.Abs(row.Value)) : held.Sweep,
                    row.Slot % MemberDegrees == 1 ? Math.Max(held.Camber, Math.Abs(row.Value)) : held.Camber,
                    row.Slot % MemberDegrees == 3 ? Math.Max(held.Twist, Math.Abs(row.Value)) : held.Twist,
                    row.Slot % MemberDegrees == 4 ? Math.Max(held.Angular, Math.Abs(row.Value)) : held.Angular,
                    held.Work + (row.Value * vector[row.Slot])));
        return new DistortionField(rows, new DistortionEvidence(
            Length.FromMeters(extremes.Sweep),
            Length.FromMeters(extremes.Camber),
            new Angle(extremes.Twist, AngleUnit.Radian),
            new Angle(extremes.Angular, AngleUnit.Radian),
            solved.Residual,
            extremes.Work,
            solved.FactorNonZeros.IfNone(0)));
    }

    private static double Score(
        DistortionObjective objective,
        NodaTime.Duration total,
        Temperature maximum,
        DistortionEvidence field) =>
        (objective.Sweep * field.Sweep.Millimeters)
        + (objective.Camber * field.Camber.Millimeters)
        + (objective.Twist * field.Twist.Degrees)
        + (objective.Angular * field.Angular.Degrees)
        + (objective.Time * total.TotalSeconds)
        + (objective.Thermal * maximum.DegreesCelsius);
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Weld sequencing fold
    accDescr: One admitted sequence request derives joint precedence levels, cuts station-indexed schedule rows off the weld plan's own deposit segments, generates a bounded candidate order space, folds each candidate through one thermal state, and solves every candidate against one cached sparse factor before ranking.
    Request["SequenceRequest — plan, assembly, budget, policy, motion timing"] --> Precedence["Precedence — DagShortestPathAlgorithm + CriticalDistance"]
    Request --> Segments["Segments — DepositSegment.Window(0,1)"]
    Precedence -->|level per joint| Segments
    Segments --> Assemble["Assemble — SparseMatrix.FromTriplets + CholeskySparse.Of"]
    Request -->|CandidateLaw.Generate, lazily bounded| Orders["DistortionOrder stream"]
    Orders --> Candidate["Candidate"]
    Segments --> Candidate
    Candidate -->|"Subdivide — DepositSegment.Window(from,to)"| Sub["band-split rows"]
    Sub -->|"Arrange — precedence, barrier, primary, secondary"| Arranged["ordered rows"]
    Arranged -->|Seeds — JointAction.Stage| Work["WorkSeed stream"]
    Work -->|"Advance — one thermal state"| Scheduled["ScheduledWork + clock"]
    Assemble --> Solve["Solve — one factor per request"]
    Scheduled --> Solve
    Solve --> Field["DistortionField — per-member rows + evidence summary"]
    Field --> Rank["Rejections + Score"]
    Rank --> Schedule["WeldSchedule"]
    Field -->|"the ONE distortion field"| Fixturing["Fixturing/assembly + Fixturing/setups"]
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
