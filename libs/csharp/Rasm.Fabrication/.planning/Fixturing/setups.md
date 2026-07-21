# [RASM_FABRICATION_SETUPS]

`SetupSchedule` owns operation precedence, datum lineage, fixture and machine assignment, physical reorientation, carrier loading, per-instance work-offset allocation, transfer, probing, stock continuity, and schedule evidence. `SetupPlan` admits every operation, relation, fixture candidate, carrier station, part instance, resource row, and controller slot once; the search consumes only admitted identities and typed evidence.

`Fixture`, `FixtureSet`, `ExclusionZone`, `Setup`, `WcsSlot`, and `SetupSchedule` remain the in-process wire vocabulary. `SetupSchedule.Apply` closes admission, scheduling, rebasing, and projection over `SetupOp`, and every egress carries the content key minted through `ContentKey.Of`.

## [01]-[INDEX]

- [01]-[VOCABULARY]: operation, relation, objective, mounting, work-offset, and evidence owners.
- [02]-[SCHEDULE]: graph admission, candidate evidence, bounded partitioning, datum transfer, and WCS allocation.
- [03]-[PROJECTION]: machine, probing, posting, traveler, and evidence egress.

## [02]-[VOCABULARY]

- Owner: `SetupOperation` carries one physical operation instance; `SetupRelation` carries precedence, datum, stock, probe, and resource edges without collapsing them into an untyped pair.
- Placement: `Mounting` closes table, pallet, tombstone face, rotary index, trunnion, spindle, robot positioner, and floor-cell mounting.
- Objective: `SetupObjective` carries setup, changeover, reorientation, travel, linear and angular datum-transfer, rigidity, and risk weights with their unit scales as one admitted policy value.
- WCS: `WcsSlot` is a closed payload family for base, extended, dynamic, rotary, and local offsets; controller syntax remains posting-owned.
- Carrier: `Carrier`, `CarrierStation`, and `PartInstance` model pallet and tombstone occupancy, station frames, derived local offsets, and amortized tool-change cost without cloning operations.
- Evidence: `SetupEvidence` retains compatibility, typed kinematics, optional robot-cell placement, workholding, clearance, guard, probe, stock, datum, and resource receipts; `SetupBoundaryEvidence.Key` fingerprints every provider-owned field before admission.
- Growth: a new scheduling concern lands as one relation case, objective column, mounting case, or evidence field; no delegate column or entrypoint appears beside the owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Fixturing;

// --- [VOCABULARY] --------------------------------------------------------------------------------------------------------------------------------
[Union]
public abstract partial record WcsSlot {
    private WcsSlot() { }

    public sealed record Base(int Ordinal) : WcsSlot;
    public sealed record Extended(int Ordinal) : WcsSlot;
    public sealed record Dynamic(int Ordinal) : WcsSlot;
    public sealed record Rotary(int Ordinal, int Axis) : WcsSlot;
    public sealed record Local(int Ordinal, int Parent) : WcsSlot;
}

[Union]
public abstract partial record Mounting {
    private Mounting() { }

    public sealed record Table(Plane Frame) : Mounting;
    public sealed record Pallet(string Key, Plane Frame) : Mounting;
    public sealed record Tombstone(string Key, int Face, Plane Frame) : Mounting;
    public sealed record Rotary(int Axis, Angle Angle, Plane Frame) : Mounting;
    public sealed record Trunnion(Angle A, Angle C, Plane Frame) : Mounting;
    public sealed record Spindle(Plane Frame) : Mounting;
    public sealed record Positioner(string Key, Plane Frame) : Mounting;
    public sealed record Cell(Plane Frame) : Mounting;

    public Plane Frame => Switch(
        table: static row => row.Frame,
        pallet: static row => row.Frame,
        tombstone: static row => row.Frame,
        rotary: static row => row.Frame,
        trunnion: static row => row.Frame,
        spindle: static row => row.Frame,
        positioner: static row => row.Frame,
        cell: static row => row.Frame);

    public Mounting Reframed(Plane frame) => Switch(
        state: frame,
        table: static (next, _) => new Table(next),
        pallet: static (next, row) => new Pallet(row.Key, next),
        tombstone: static (next, row) => new Tombstone(row.Key, row.Face, next),
        rotary: static (next, row) => new Rotary(row.Axis, row.Angle, next),
        trunnion: static (next, row) => new Trunnion(row.A, row.C, next),
        spindle: static (next, _) => new Spindle(next),
        positioner: static (next, row) => new Positioner(row.Key, next),
        cell: static (next, _) => new Cell(next));
}

[ComplexValueObject]
public sealed partial class SetupObjective {
    public double SetupWeight { get; }
    public double ChangeWeight { get; }
    public double OrientWeight { get; }
    public double TravelWeight { get; }
    public double DatumWeight { get; }
    public double RigidityWeight { get; }
    public double RiskWeight { get; }
    public Duration TimeScale { get; }
    public Length TravelScale { get; }
    public Length DatumScale { get; }
    public Angle DatumAngleScale { get; }
    public double ConditionScale { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double setupWeight,
        ref double changeWeight,
        ref double orientWeight,
        ref double travelWeight,
        ref double datumWeight,
        ref double rigidityWeight,
        ref double riskWeight,
        ref Duration timeScale,
        ref Length travelScale,
        ref Length datumScale,
        ref Angle datumAngleScale,
        ref double conditionScale) {
        double totalWeight = setupWeight + changeWeight + orientWeight + travelWeight + datumWeight + rigidityWeight + riskWeight;
        validationError = Seq(setupWeight, changeWeight, orientWeight, travelWeight, datumWeight, rigidityWeight, riskWeight)
                .ForAll(static weight => double.IsFinite(weight) && weight >= 0.0)
            && double.IsFinite(totalWeight) && totalWeight > 0.0
            && timeScale.As(DurationUnit.Second) > 0.0 && double.IsFinite(timeScale.As(DurationUnit.Second))
            && travelScale.As(LengthUnit.Millimeter) > 0.0 && double.IsFinite(travelScale.As(LengthUnit.Millimeter))
            && datumScale.As(LengthUnit.Millimeter) > 0.0 && double.IsFinite(datumScale.As(LengthUnit.Millimeter))
            && datumAngleScale.As(AngleUnit.Radian) > 0.0 && double.IsFinite(datumAngleScale.As(AngleUnit.Radian))
            && conditionScale > 0.0 && double.IsFinite(conditionScale)
                ? null
                : new ValidationError(message: "<invalid-setup-objective>");
    }
}

[Union]
public abstract partial record SetupRelation {
    private SetupRelation() { }

    public sealed record Precedes : SetupRelation;
    public sealed record Datum : SetupRelation;
    public sealed record Stock : SetupRelation;
    public sealed record Probe : SetupRelation;
    public sealed record Resource(string Key) : SetupRelation;
    public sealed record SameFixture : SetupRelation;
    public sealed record SameOrientation : SetupRelation;

    public bool Orders => Switch(
        precedes: static _ => true,
        datum: static _ => true,
        stock: static _ => true,
        probe: static _ => true,
        resource: static _ => true,
        sameFixture: static _ => false,
        sameOrientation: static _ => false);
}

public readonly record struct SetupEdge(int Source, int Target, SetupRelation Relation) : IEdge<int>;

[ValueObject<string>]
public readonly partial struct CarrierKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value) ? new ValidationError(message: "<invalid-carrier-key>") : null;
}

public readonly record struct CarrierStation(int Index, Plane Frame, WcsSlot Wcs);

public sealed record Carrier(
    CarrierKey Key,
    Mounting Mounting,
    Seq<CarrierStation> Stations,
    Duration ToolChange);

public readonly record struct PartInstance(
    int Key,
    int Operation,
    CarrierKey Carrier,
    int Station,
    Plane LocalFrame);

public readonly record struct InstanceWcs(int Instance, WcsSlot Slot, Transform Frame);

public sealed record SetupOperation(
    int Key,
    int Process,
    Seq<int> Features,
    Seq<Mounting> Mountings,
    Seq<int> FixtureKeys,
    Seq<int> MachineKeys,
    Seq<LoadCase> Loads,
    Seq<ToolCorridor> Corridors,
    Seq<string> Resources,
    Duration Duration,
    Length DatumTolerance,
    Angle DatumAngularTolerance,
    double RigidityDemand,
    Ratio SafetyFactor,
    bool RequiresProbe);

public readonly record struct KinematicReceipt(
    int Machine,
    Arr<Angle> Axes,
    double JacobianCondition,
    Length Clearance,
    Length Travel,
    Duration Reorientation,
    Option<CellPlacementReceipt> Robot) {
    public bool Reachable => Axes.Count > 0
        && Axes.ForAll(static axis => double.IsFinite(axis.As(AngleUnit.Radian)))
        && JacobianCondition > 0.0 && double.IsFinite(JacobianCondition)
        && Clearance.As(LengthUnit.Millimeter) >= 0.0 && double.IsFinite(Clearance.As(LengthUnit.Millimeter))
        && Travel.As(LengthUnit.Millimeter) >= 0.0 && double.IsFinite(Travel.As(LengthUnit.Millimeter))
        && Robot.ForAll(static receipt => double.IsFinite(receipt.Selected.Score));
}

public readonly record struct DatumReceipt(
    int Anchor,
    Seq<int> Lineage,
    Length TransferError,
    Angle AngularTransferError,
    Length ProbeCorrection,
    Angle AngularProbeCorrection,
    bool Probed,
    bool Traceable);

public readonly record struct ResourceReceipt(Seq<string> Held, Duration Changeover, bool Available);

public readonly record struct SetupBoundaryEvidence(
    KinematicReceipt Reach,
    bool Guarded,
    DatumReceipt Datum,
    ResourceReceipt Resources,
    ContentKey Key);

public interface ISetupEvidenceSource {
    Fin<SetupBoundaryEvidence> Evaluate(int machine, SetupOperation operation, Fixture fixture, Mounting mounting);
}

public sealed record SetupEvidence(
    int Operation,
    bool Compatible,
    KinematicReceipt Reach,
    HoldingReceipt Holding,
    Seq<WorkholdingResult.Clearance> Clearance,
    bool Guarded,
    Option<Point3d> MachinedHit,
    DatumReceipt Datum,
    ResourceReceipt Resources,
    double RigidityMargin,
    ContentKey Key);
```

## [03]-[SCHEDULE]

- Owner: `SetupPlan` is raw ingress; `Setup` is one admitted physical orientation and resource custody interval; `SetupSchedule` is the proof-bearing ordered result.
- Graph: `BidirectionalGraph<int, SetupEdge>` preserves isolated operations, typed edge payloads, source-first order, strongly connected cycle evidence, and transitive reduction.
- Admission: identity, relation references, WCS slots, carrier stations, part instances, machine keys, fixture keys, mounting frames, objective values, and operation payloads accumulate before graph construction.
- Search: branch-and-bound expands existing and new setups over one `SearchSpace` carrying the admission-derived operation index, and `BoundOf` derives each admissible remainder from the same nonnegative duration term used by `Cost`; ingress cannot falsify optimality.
- Proof: `Cut` records the least bound the search refused, so present `ProvenLowerBound` states the real optimality gap. Search without a refused branch proves its incumbent optimal; rebase clears proof presence, and `NodeBudget` exhaustion fails typed.
- Receipt: the scheduled arm fires the `FabricationFact.Engine.Of` decision-count row through the `FabricationTap` `Apply` accepts, defaulting silent for headless callers, so branch-and-bound cost attribution rides the telemetry rail with zero kernel writes.
- Candidate: one applicative evidence fan-in composes machine or robot-cell reach, rebuilt workholding restraint and corridor checks, guard, machined-stock, datum transfer, probing, and resource availability.
- Allocation: controller and carrier-station WCS rows come from the unconsumed admitted roster remainder; setup indices never derive controller syntax, array position, or offset availability, and makespan accumulates per machine so setups on distinct machines do not serialize.
- Rebase: a measured frame re-enters through the same evidence boundary that admitted the setup, and a correction exceeding the tightest datum tolerance the setup's operations carry rejects rather than stamping traceability.
- Exemption: QuikGraph construction with `Search`, `Candidates`, and `Commit` forms the bounded scheduling kernel; mutation remains inside admitted graph, schedule-state, and `Atom` containers.
- Boundary: ordinary infeasibility prunes one candidate as `Option.None`; malformed input, failed geometry, exhausted budget, or boundary failure remains a typed `Fin` failure.

```csharp signature
// --- [SCHEDULE] -----------------------------------------------------------------------------------------------------------------------------------
public sealed record SetupPlan(
    Seq<SetupOperation> Operations,
    Seq<SetupEdge> Relations,
    FixtureSet Fixtures,
    Seq<int> Machines,
    Seq<WcsSlot> Wcs,
    Seq<Carrier> Carriers,
    Seq<PartInstance> Instances,
    SetupObjective Objective,
    int MaxSetups,
    int NodeBudget,
    ISetupEvidenceSource Evidence);

public sealed record Setup(
    int Index,
    int Machine,
    Fixture Fixture,
    Mounting Mounting,
    WcsSlot Wcs,
    Option<Carrier> Carrier,
    Seq<PartInstance> Instances,
    Seq<InstanceWcs> InstanceWcs,
    DatumReceipt Datum,
    Arr<int> Operations,
    Set<string> Resources,
    Duration Start,
    Duration Finish);

public readonly record struct SetupDecision(
    int Operation,
    int Setup,
    bool Extended,
    double IncrementalCost,
    double Bound,
    SetupEvidence Evidence);

public sealed record ScheduleState(
    Arr<Setup> Setups,
    Seq<SetupDecision> Decisions,
    Set<int> Placed,
    double Cost,
    double LowerBound) {
    public static ScheduleState Empty => new(Arr<Setup>(), Seq<SetupDecision>(), Set<int>(), 0.0, 0.0);
}

public readonly record struct WcsAssignment(int Setup, WcsSlot Slot);

[Union]
public abstract partial record SetupOp {
    private SetupOp() { }

    public sealed record Admit(SetupPlan Plan) : SetupOp;
    public sealed record Schedule(SetupPlan Plan) : SetupOp;
    public sealed record Rebase(SetupSchedule Schedule, int Setup, Plane Measured) : SetupOp;
    public sealed record Project(SetupSchedule Schedule, SetupProjection Projection) : SetupOp;
}

[Union]
public abstract partial record SetupResult {
    private SetupResult() { }

    public sealed record Admitted(SetupPlan Plan) : SetupResult;
    public sealed record Scheduled(SetupSchedule Schedule) : SetupResult;
    public sealed record Rebased(SetupSchedule Schedule) : SetupResult;
    public sealed record Projected(SetupArtifact Artifact) : SetupResult;
}

public sealed partial record SetupSchedule(
    SetupPlan Plan,
    Arr<Setup> Setups,
    Seq<WcsAssignment> Wcs,
    Seq<SetupEdge> Precedence,
    Seq<SetupDecision> Decisions,
    double Cost,
    Option<double> ProvenLowerBound,
    ContentKey Key) {
    public static Fin<SetupResult> Apply(SetupOp? candidate, FabricationTap? tap = null) =>
        Optional(candidate).ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Absent()).ToError()).Bind(op => op.Switch(
            state: tap ?? FabricationTap.Silent,
            admit: static (_, row) => Admit(row.Plan).Map<SetupResult>(static plan => new SetupResult.Admitted(plan)),
            schedule: static (port, row) => Admit(row.Plan).Bind(Solve).Map<SetupResult>(schedule =>
                (FabricationFact.Engine.Of(schedule).Map(port.Fire).Strict(), new SetupResult.Scheduled(schedule)).Item2),
            rebase: static (_, row) => Rebase(row.Schedule, row.Setup, row.Measured).Map<SetupResult>(static schedule => new SetupResult.Rebased(schedule)),
            project: static (_, row) => Project(row.Schedule, row.Projection).Map<SetupResult>(static artifact => new SetupResult.Projected(artifact))));

    static Fin<SetupPlan> Admit(SetupPlan? candidate) =>
        Optional(candidate).ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Absent()).ToError()).Bind(plan =>
        (GatePlan(plan), GateOperations(plan), GateRelations(plan), GateWcs(plan), GateCarriers(plan))
            .Apply(static (accepted, _, _, _, _) => accepted)
            .As()
            .ToFin());

    static K<Validation<Error>, SetupPlan> GatePlan(SetupPlan plan) =>
        plan.MaxSetups > 0 && plan.MaxSetups <= plan.Wcs.Count && plan.NodeBudget > 0
        && plan.Objective is not null && plan.Evidence is not null
        && plan.Machines.Count > 0 && plan.Machines.ForAll(static machine => machine >= 0)
        && plan.Machines.Distinct().Count == plan.Machines.Count
        && plan.Fixtures is not null && plan.Fixtures.Fixtures.Count > 0
        && plan.Fixtures.Fixtures.ForAll(static fixture => fixture.Constraint.Constrained)
            ? Validation<Error, SetupPlan>.Success(plan)
            : Validation<Error, SetupPlan>.Fail(new FabricationFault.FixtureInadmissible(new FixturingWitness.Plan(
                plan.Operations.Count, plan.Machines.Count, plan.Fixtures.Fixtures.Count, plan.MaxSetups)).ToError());

    static K<Validation<Error>, Unit> GateOperations(SetupPlan plan) {
        if (plan.Fixtures is null || !plan.Operations.ForAll(static operation => operation is not null))
            return Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Operation(-1, nameof(SetupPlan.Operations))).ToError());
        Set<int> keys = plan.Operations.Map(static operation => operation.Key).ToSet();
        return keys.Count == plan.Operations.Count && plan.Operations.ForAll(operation => Valid(operation, plan))
                ? Validation<Error, Unit>.Success(unit)
                : Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(new FixturingWitness.Operation(
                    plan.Operations.Find(operation => !Valid(operation, plan)).Map(static operation => operation.Key).IfNone(-1),
                    nameof(SetupOperation))).ToError());
    }

    static K<Validation<Error>, Unit> GateRelations(SetupPlan plan) {
        if (!plan.Operations.ForAll(static operation => operation is not null))
            return Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Operation(-1, nameof(SetupPlan.Relations))).ToError());
        Set<int> keys = plan.Operations.Map(static operation => operation.Key).ToSet();
        return plan.Relations.Distinct().Count == plan.Relations.Count
            && plan.Relations.ForAll(edge => edge.Relation is not null && keys.Contains(edge.Source) && keys.Contains(edge.Target) && edge.Source != edge.Target
            && edge.Relation.Switch(
                precedes: static _ => true,
                datum: static _ => true,
                stock: static _ => true,
                probe: static _ => true,
                resource: static row => !string.IsNullOrWhiteSpace(row.Key),
                sameFixture: static _ => true,
                sameOrientation: static _ => true))
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(Broken(new SetupChain(keys.ToSeq(), plan.Relations.Map(static edge => (edge.Source, edge.Target)))));

    // Lineage faults carry a content-keyed subject, so the readable chain rides the
    // schedule while the fault carries its key.
    static Error Broken(SetupChain chain) =>
        new FabricationFault.DatumLineageBroken(new FaultSubject.Lineage(
            ContentKey.Of(EgressKind.Plan, chain.Canonical))).ToError();
    static K<Validation<Error>, Unit> GateWcs(SetupPlan plan) =>
        plan.Wcs.Count >= plan.MaxSetups && plan.Wcs.ForAll(slot => Valid(slot, plan.Wcs))
        && plan.Wcs.Distinct().Count == plan.Wcs.Count
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(new FixturingWitness.Offsets(
                plan.Wcs.Count, plan.Wcs.Distinct().Count, plan.MaxSetups)).ToError());

    static bool Valid(SetupOperation operation, SetupPlan plan) =>
        operation.Key >= 0 && operation.Mountings.Count > 0 && operation.FixtureKeys.Count > 0 && operation.MachineKeys.Count > 0
        && operation.MachineKeys.ForAll(plan.Machines.Contains) && operation.FixtureKeys.ForAll(plan.Fixtures.ByOperation.ContainsKey)
        && operation.Mountings.ForAll(Valid) && operation.Mountings.Distinct().Count == operation.Mountings.Count
        && operation.Features.ForAll(static feature => feature >= 0) && operation.Features.Distinct().Count == operation.Features.Count
        && operation.FixtureKeys.Distinct().Count == operation.FixtureKeys.Count
        && operation.MachineKeys.Distinct().Count == operation.MachineKeys.Count
        && operation.Loads.Count > 0 && operation.Loads.ForAll(static load => load is not null)
        && operation.Corridors.ForAll(Valid)
        && operation.Resources.ForAll(static resource => !string.IsNullOrWhiteSpace(resource))
        && operation.Resources.Distinct().Count == operation.Resources.Count
        && operation.Duration.As(DurationUnit.Second) >= 0.0 && double.IsFinite(operation.Duration.As(DurationUnit.Second))
        && operation.DatumTolerance.As(LengthUnit.Millimeter) >= 0.0 && double.IsFinite(operation.DatumTolerance.As(LengthUnit.Millimeter))
        && operation.DatumAngularTolerance.As(AngleUnit.Radian) >= 0.0 && double.IsFinite(operation.DatumAngularTolerance.As(AngleUnit.Radian))
        && operation.SafetyFactor.As(RatioUnit.DecimalFraction) >= 1.0 && double.IsFinite(operation.SafetyFactor.As(RatioUnit.DecimalFraction))
        && operation.RigidityDemand >= 0.0 && double.IsFinite(operation.RigidityDemand);

    static bool Valid(WcsSlot slot, Seq<WcsSlot> roster) => slot is not null && slot.Switch(
        state: roster,
        @base: static (_, row) => row.Ordinal >= 0,
        extended: static (_, row) => row.Ordinal >= 0,
        dynamic: static (_, row) => row.Ordinal >= 0,
        rotary: static (_, row) => row.Ordinal >= 0 && row.Axis >= 0,
        local: static (slots, row) => row.Ordinal >= 0 && row.Parent >= 0 && row.Parent != row.Ordinal
            && slots.Filter(slot => ControllerOrdinal(slot).Contains(row.Parent)).Count == 1);

    static Option<int> ControllerOrdinal(WcsSlot? slot) => Optional(slot).Bind(value => value.Switch<Option<int>>(
        @base: static row => Some(row.Ordinal),
        extended: static row => Some(row.Ordinal),
        dynamic: static row => Some(row.Ordinal),
        rotary: static row => Some(row.Ordinal),
        local: static _ => None));

    static bool Valid(Mounting mounting) => mounting is not null && mounting.Switch(
        table: static row => row.Frame.IsValid,
        pallet: static row => !string.IsNullOrWhiteSpace(row.Key) && row.Frame.IsValid,
        tombstone: static row => !string.IsNullOrWhiteSpace(row.Key) && row.Face >= 0 && row.Frame.IsValid,
        rotary: static row => row.Axis >= 0 && double.IsFinite(row.Angle.As(AngleUnit.Radian)) && row.Frame.IsValid,
        trunnion: static row => double.IsFinite(row.A.As(AngleUnit.Radian)) && double.IsFinite(row.C.As(AngleUnit.Radian)) && row.Frame.IsValid,
        spindle: static row => row.Frame.IsValid,
        positioner: static row => !string.IsNullOrWhiteSpace(row.Key) && row.Frame.IsValid,
        cell: static row => row.Frame.IsValid);

    static bool Valid(ToolCorridor corridor) => corridor is not null && corridor.Kind is not null && corridor.Stations.Count >= 2
        && corridor.Stations.ForAll(static station => double.IsFinite(station.Point.X) && double.IsFinite(station.Point.Y)
            && double.IsFinite(station.Point.Z)
            && Seq(station.Cutter, station.Holder, station.Chip, station.Coolant)
                .Map(static radius => radius.As(LengthUnit.Millimeter))
                .ForAll(static radius => double.IsFinite(radius) && radius >= 0.0));

    static K<Validation<Error>, Unit> GateCarriers(SetupPlan plan) {
        if (!plan.Carriers.ForAll(static carrier => carrier is not null)
            || !plan.Operations.ForAll(static operation => operation is not null)
            || !plan.Instances.ForAll(static instance => instance is not null))
            return Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Roster(CarrierKey.Create(nameof(SetupPlan.Carriers)), -1, plan.Instances.Count)).ToError());
        HashMap<CarrierKey, Carrier> carriers = plan.Carriers.Fold(HashMap<CarrierKey, Carrier>(), static (index, row) => index.Add(row.Key, row));
        Set<int> operations = plan.Operations.Map(static row => row.Key).ToSet();
        Set<int> instances = plan.Instances.Map(static row => row.Key).ToSet();
        return carriers.Count == plan.Carriers.Count && instances.Count == plan.Instances.Count
            && plan.Carriers.ForAll(static carrier => carrier.Stations.Count > 0
                && !string.IsNullOrWhiteSpace(carrier.Key.Value)
                && carrier.Stations.Map(static station => station.Index).ToSet().Count == carrier.Stations.Count
                && carrier.Stations.Map(static station => station.Wcs).Distinct().Count == carrier.Stations.Count
                && Valid(carrier.Mounting) && carrier.ToolChange.As(DurationUnit.Second) >= 0.0
                && double.IsFinite(carrier.ToolChange.As(DurationUnit.Second))
                && carrier.Stations.ForAll(station => station.Index >= 0 && station.Frame.IsValid && Valid(station.Wcs, plan.Wcs)
                    && plan.Wcs.Contains(station.Wcs)))
            && plan.Instances.ForAll(instance => instance.Key >= 0 && !string.IsNullOrWhiteSpace(instance.Carrier.Value)
                && operations.Contains(instance.Operation) && carriers.ContainsKey(instance.Carrier)
                && carriers[instance.Carrier].Stations.Exists(station => station.Index == instance.Station) && instance.LocalFrame.IsValid)
            && plan.Instances.GroupBy(static instance => instance.Operation)
                .ForAll(static group => group.Map(static instance => instance.Carrier).Distinct().Count == 1)
                ? Validation<Error, Unit>.Success(unit)
                : Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(new FixturingWitness.Roster(
                    plan.Instances.Find(instance => !carriers.ContainsKey(instance.Carrier))
                        .Map(static instance => instance.Carrier).IfNone(() => CarrierKey.Create(nameof(Carrier))),
                    plan.Instances.Find(instance => !carriers.ContainsKey(instance.Carrier)).Map(static instance => instance.Station).IfNone(-1),
                    plan.Instances.Count)).ToError());
    }

    static Fin<SetupSchedule> Solve(SetupPlan plan) {
        BidirectionalGraph<int, SetupEdge> graph = Graph(plan);
        if (!graph.IsDirectedAcyclicGraph()) return Fin.Fail<SetupSchedule>(Broken(Cycles(graph)));
        Arr<int> order = graph.SourceFirstBidirectionalTopologicalSort().ToArr();
        SearchSpace space = new(
            order,
            plan.Operations.Fold(HashMap<int, SetupOperation>(), static (index, row) => index.Add(row.Key, row)),
            plan,
            Atom(plan.NodeBudget),
            Atom(double.PositiveInfinity),
            Atom(0));
        return Search(space, cursor: 0, ScheduleState.Empty, bound: double.PositiveInfinity)
            .Bind(result => result.ToFin(FabricationFault.SetupInfeasible(
                order.At(space.Deepest.Value).IfNone(-1), plan.MaxSetups).ToError()))
            .Map(state => Finalize(plan, graph, state, space.Cut.Value));
    }

    static BidirectionalGraph<int, SetupEdge> Graph(SetupPlan plan) {
        BidirectionalGraph<int, SetupEdge> graph = new(allowParallelEdges: true);
        graph.AddVertexRange(plan.Operations.Map(static operation => operation.Key));
        graph.AddEdgeRange(plan.Relations.Filter(static edge => edge.Relation.Orders));
        return graph;
    }

    static SetupChain Cycles(BidirectionalGraph<int, SetupEdge> graph) {
        Dictionary<int, int> labels = new();
        graph.StronglyConnectedComponents(labels);
        Set<int> cyclic = labels.GroupBy(static pair => pair.Value)
            .Filter(static group => group.Count() > 1)
            .Bind(static group => group.Map(static pair => pair.Key))
            .ToSet();
        return new SetupChain(cyclic.ToSeq(), graph.Edges.Filter(edge => cyclic.Contains(edge.Source) && cyclic.Contains(edge.Target))
            .Map(static edge => (edge.Source, edge.Target)).ToSeq());
    }

    // Operation indexing is an admission-time derivation, not a per-node fold; `Cut` records the
    // least bound the search refused so `ProvenLowerBound` states a real optimality gap, and
    // `Budget` bounds a tree whose candidate fan is machines by fixtures by mountings per level.
    public sealed record SearchSpace(
        Arr<int> Order,
        HashMap<int, SetupOperation> Operations,
        SetupPlan Plan,
        Atom<int> Budget,
        Atom<double> Cut,
        Atom<int> Deepest);

    static Fin<Option<ScheduleState>> Search(SearchSpace space, int cursor, ScheduleState state, double bound) {
        double remaining = space.Order.Skip(cursor).Sum(key => BoundOf(space.Operations[key], space.Plan.Objective));
        _ = space.Deepest.Swap(held => Math.Max(held, cursor));
        if (space.Budget.Swap(static held => Math.Max(0, held - 1)) == 0)
            return Fin.Fail<Option<ScheduleState>>(new FabricationFault.FixtureInadmissible(new FixturingWitness.Plan(
                space.Order.Count, cursor, state.Setups.Count, space.Plan.NodeBudget)).ToError());
        if (state.Cost + remaining >= bound) {
            _ = space.Cut.Swap(held => Math.Min(held, state.Cost + remaining));
            return Fin.Succ(Option<ScheduleState>.None);
        }
        return cursor == space.Order.Count
            ? Fin.Succ(Some(state with { LowerBound = state.Cost }))
            : Candidates(state, space.Operations[space.Order[cursor]], space.Plan).Fold(
                Fin.Succ(Option<ScheduleState>.None),
                (rail, candidate) => rail.Bind(best => Place(state, space.Operations[space.Order[cursor]], candidate, space.Plan)
                    .Bind(next => next.Match(
                        Some: admitted => Search(space, cursor + 1, admitted, best.Match(Some: held => Math.Min(bound, held.Cost), None: () => bound)),
                        None: static () => Fin.Succ(Option<ScheduleState>.None)))
                    .Map(found => Better(best, found))));
    }

    static Seq<(Option<int> Setup, int Machine, Fixture Fixture, Mounting Mounting, Option<Carrier> Carrier)> Candidates(
        ScheduleState state,
        SetupOperation operation,
        SetupPlan plan) {
        Set<CarrierKey> carriers = plan.Instances.Filter(instance => instance.Operation == operation.Key).Map(static instance => instance.Carrier).ToSet();
        Seq<(Option<int>, int, Fixture, Mounting, Option<Carrier>)> existing = state.Setups
            .Filter(setup => operation.MachineKeys.Contains(setup.Machine) && operation.FixtureKeys.Contains(setup.Fixture.Operation))
            .Filter(setup => operation.Mountings.Exists(mounting => mounting.Frame == setup.Mounting.Frame))
            .Filter(setup => carriers.IsEmpty || setup.Carrier.Exists(carrier => carriers.Contains(carrier.Key)))
            .Map(setup => (Some(setup.Index), setup.Machine, setup.Fixture, setup.Mounting, setup.Carrier));
        Seq<(Option<int>, int, Fixture, Mounting, Option<Carrier>)> opened = operation.MachineKeys.Bind(machine =>
            operation.FixtureKeys.Bind(fixture =>
                (carriers.IsEmpty
                    ? operation.Mountings.Map(mounting => (Option<int>.None, machine, plan.Fixtures.ByOperation[fixture], mounting, Option<Carrier>.None))
                    : Seq<(Option<int>, int, Fixture, Mounting, Option<Carrier>)>())
                + plan.Carriers.Filter(carrier => carriers.Contains(carrier.Key))
                    .Map(carrier => (Option<int>.None, machine, plan.Fixtures.ByOperation[fixture], carrier.Mounting, Some(carrier)))));
        return (state.Setups.Count >= plan.MaxSetups ? existing : existing + opened)
            .Filter(candidate => operation.Mountings.Exists(mounting => mounting.Frame == candidate.Mounting.Frame))
            .Filter(candidate => FitsRelations(state, operation, candidate.Fixture, candidate.Mounting, plan));
    }

    static bool FitsRelations(ScheduleState state, SetupOperation operation, Fixture fixture, Mounting mounting, SetupPlan plan) =>
        plan.Relations.Filter(edge => edge.Source == operation.Key || edge.Target == operation.Key).ForAll(edge => {
            int other = edge.Source == operation.Key ? edge.Target : edge.Source;
            Option<Setup> placed = state.Setups.Find(setup => setup.Operations.Contains(other));
            return edge.Relation.Switch(
                precedes: static (_, _) => true,
                datum: static (_, _) => true,
                stock: static (_, _) => true,
                probe: static (_, _) => true,
                resource: static (_, _) => true,
                sameFixture: static (held, _) => held.Placed.ForAll(setup => setup.Fixture.Operation == held.Fixture.Operation),
                sameOrientation: static (held, _) => held.Placed.ForAll(setup => setup.Mounting.Frame == held.Mounting.Frame),
                state: (Placed: placed, Fixture: fixture, Mounting: mounting));
        });

    static Fin<Option<ScheduleState>> Place(
        ScheduleState state,
        SetupOperation operation,
        (Option<int> Setup, int Machine, Fixture Fixture, Mounting Mounting, Option<Carrier> Carrier) candidate,
        SetupPlan plan) =>
        Evidence(operation, candidate.Machine, candidate.Fixture, candidate.Mounting, plan).Bind(evidence => evidence.Match(
            Some: accepted => Commit(state, operation, candidate, accepted, plan).Map(Some),
            None: static () => Fin.Succ(Option<ScheduleState>.None)));

    static Fin<Option<SetupEvidence>> Evidence(
        SetupOperation operation,
        int machine,
        Fixture fixture,
        Mounting mounting,
        SetupPlan plan) =>
        (plan.Evidence.Evaluate(machine, operation, fixture, mounting).ToValidation(),
         Holding(operation, fixture).ToValidation(),
         Clearance(operation, fixture).ToValidation(),
         Machined(fixture).ToValidation())
            .Apply((boundary, holding, clearance, hit) => new SetupEvidence(
                operation.Key,
                operation.MachineKeys.Contains(machine),
                boundary.Reach,
                holding,
                clearance,
                boundary.Guarded,
                hit,
                boundary.Datum,
                boundary.Resources,
                holding.MinimumMargin / Math.Max(operation.RigidityDemand, 1e-9),
                boundary.Key))
            .Map(evidence => Valid(evidence) && evidence.Compatible && evidence.Reach.Reachable && evidence.Holding.Holds
                && evidence.Clearance.ForAll(static row => row.Clear) && evidence.Guarded && evidence.MachinedHit.IsNone
                && evidence.Datum.Traceable && (!operation.RequiresProbe || evidence.Datum.Probed)
                && evidence.Datum.TransferError <= operation.DatumTolerance && evidence.Resources.Available
                && evidence.Datum.AngularTransferError <= operation.DatumAngularTolerance
                && evidence.RigidityMargin >= 1.0 && RelationEvidence(operation, evidence, plan)
                    ? Some(evidence)
                    : Option<SetupEvidence>.None)
            .As()
            .ToFin();

    static bool Valid(SetupEvidence evidence) => evidence.Key is not null && evidence.Key.Kind == EgressKind.Plan
        && evidence.Datum.Anchor >= 0
        && evidence.Datum.Lineage.ForAll(static operation => operation >= 0)
        && evidence.Datum.Lineage.Distinct().Count == evidence.Datum.Lineage.Count
        && evidence.Datum.TransferError.As(LengthUnit.Millimeter) >= 0.0
        && double.IsFinite(evidence.Datum.TransferError.As(LengthUnit.Millimeter))
        && evidence.Datum.AngularTransferError.As(AngleUnit.Radian) >= 0.0
        && double.IsFinite(evidence.Datum.AngularTransferError.As(AngleUnit.Radian))
        && evidence.Datum.ProbeCorrection.As(LengthUnit.Millimeter) >= 0.0
        && double.IsFinite(evidence.Datum.ProbeCorrection.As(LengthUnit.Millimeter))
        && evidence.Datum.AngularProbeCorrection.As(AngleUnit.Radian) >= 0.0
        && double.IsFinite(evidence.Datum.AngularProbeCorrection.As(AngleUnit.Radian))
        && evidence.Resources.Held.ForAll(static resource => !string.IsNullOrWhiteSpace(resource))
        && evidence.Resources.Changeover.As(DurationUnit.Second) >= 0.0
        && double.IsFinite(evidence.Resources.Changeover.As(DurationUnit.Second));

    static bool RelationEvidence(SetupOperation operation, SetupEvidence evidence, SetupPlan plan) =>
        plan.Relations.Filter(edge => edge.Target == operation.Key).ForAll(edge => edge.Relation.Switch(
            precedes: static (_, _) => true,
            datum: static (held, _) => held.Evidence.Datum.Lineage.Contains(held.Source),
            stock: static (held, _) => held.Evidence.MachinedHit.IsNone,
            probe: static (held, _) => held.Evidence.Datum.Probed,
            resource: static (held, row) => held.Evidence.Resources.Held.Contains(row.Key),
            sameFixture: static (_, _) => true,
            sameOrientation: static (_, _) => true,
            state: (Source: edge.Source, Evidence: evidence)));

    static Fin<HoldingReceipt> Holding(SetupOperation operation, Fixture fixture) =>
        Workholding.Apply(new WorkholdingOp.Restrain(fixture, operation.Loads, operation.SafetyFactor)).Bind(static result => result switch {
            WorkholdingResult.Restrained(var receipt) => Fin.Succ(receipt),
            _ => Fin.Fail<HoldingReceipt>(new FabricationFault.WitnessMalformed(nameof(WorkholdingResult.Restrained), nameof(HoldingReceipt)).ToError()),
        });

    static Fin<Seq<WorkholdingResult.Clearance>> Clearance(SetupOperation operation, Fixture fixture) =>
        operation.Corridors.Traverse(corridor => Workholding.Apply(new WorkholdingOp.Clear(fixture, FixtureState.Cut, corridor))
            .Bind(static result => result switch {
                WorkholdingResult.Clearance receipt => Fin.Succ(receipt),
                _ => Fin.Fail<WorkholdingResult.Clearance>(new FabricationFault.WitnessMalformed(nameof(WorkholdingResult.Clearance), nameof(ExclusionZone)).ToError()),
            }).ToValidation()).As().ToFin();

    static Fin<Option<Point3d>> Machined(Fixture fixture) =>
        fixture.Spec.Current.Match(
            Some: stock => Workholding.Apply(new WorkholdingOp.Machined(fixture, stock)).Bind(static result => result switch {
                WorkholdingResult.MachinedHit(var point) => Fin.Succ(point),
                _ => Fin.Fail<Option<Point3d>>(new FabricationFault.WitnessMalformed(nameof(WorkholdingResult.MachinedHit), nameof(StockSnapshot)).ToError()),
            }),
            None: static () => Fin.Succ(Option<Point3d>.None));

    static Fin<ScheduleState> Commit(
        ScheduleState state,
        SetupOperation operation,
        (Option<int> Setup, int Machine, Fixture Fixture, Mounting Mounting, Option<Carrier> Carrier) candidate,
        SetupEvidence evidence,
        SetupPlan plan) {
        bool extended = candidate.Setup.IsSome;
        int index = candidate.Setup.IfNone(state.Setups.Count);
        int position = candidate.Setup.Map(identity => state.Setups.TakeWhile(setup => setup.Index != identity).Count).IfNone(-1);
        if (extended && (position < 0 || position >= state.Setups.Count))
            return Fin.Fail<ScheduleState>(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Offsets(index, state.Setups.Count, plan.MaxSetups)).ToError());
        // Offsets come from the unconsumed roster remainder; array position is not an allocator,
        // and two setups on different machines do not serialize against each other.
        Option<WcsSlot> slot = plan.Wcs.Find(row => !state.Setups.Exists(setup => setup.Wcs == row));
        if (candidate.Setup.IsNone && slot.IsNone)
            return Fin.Fail<ScheduleState>(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Offsets(state.Setups.Count + 1, plan.Wcs.Count, plan.MaxSetups)).ToError());
        Duration start = Duration.FromSeconds(state.Setups
            .Filter(setup => setup.Machine == candidate.Machine)
            .Map(static setup => setup.Finish.As(DurationUnit.Second)).Fold(0.0, Math.Max));
        Seq<PartInstance> instances = candidate.Carrier.Match(
            Some: carrier => plan.Instances.Filter(instance => instance.Operation == operation.Key && instance.Carrier == carrier.Key),
            None: static () => Seq<PartInstance>());
        Seq<InstanceWcs> offsets = candidate.Carrier.Match(
            Some: carrier => instances.Bind(instance => carrier.Stations
                .Filter(station => station.Index == instance.Station)
                .Map(station => new InstanceWcs(instance.Key, station.Wcs,
                    Transform.PlaneToPlane(Plane.WorldXY, station.Frame) * Transform.PlaneToPlane(Plane.WorldXY, instance.LocalFrame)))),
            None: static () => Seq<InstanceWcs>());
        Setup next = candidate.Setup.Match(
            Some: _ => state.Setups[position] with {
                Operations = state.Setups[position].Operations.Add(operation.Key),
                Instances = state.Setups[position].Instances + instances,
                InstanceWcs = state.Setups[position].InstanceWcs + offsets,
                Finish = state.Setups[position].Finish + operation.Duration,
                Resources = state.Setups[position].Resources.Union(operation.Resources),
            },
            None: () => new Setup(index, candidate.Machine, candidate.Fixture, candidate.Mounting, slot.IfNone(plan.Wcs[0]), candidate.Carrier,
                instances, offsets, evidence.Datum, Arr(operation.Key), operation.Resources.ToSet(), start, start + operation.Duration));
        double increment = Cost(operation, next, evidence, extended, plan.Objective);
        double lowerBound = BoundOf(operation, plan.Objective);
        SetupDecision decision = new(operation.Key, index, extended, increment, lowerBound, evidence);
        return Fin.Succ(state with {
            Setups = extended ? state.Setups.SetItem(position, next) : state.Setups.Add(next),
            Decisions = state.Decisions.Add(decision),
            Placed = state.Placed.Add(operation.Key),
            Cost = state.Cost + increment,
            LowerBound = state.LowerBound + lowerBound,
        });
    }

    static double Cost(SetupOperation operation, Setup setup, SetupEvidence evidence, bool extended, SetupObjective objective) =>
        (extended ? 0.0 : objective.SetupWeight)
        + (evidence.Resources.Changeover.As(DurationUnit.Second) / objective.TimeScale.As(DurationUnit.Second) * objective.ChangeWeight)
        + (evidence.Reach.Reorientation.As(DurationUnit.Second) / objective.TimeScale.As(DurationUnit.Second) * objective.OrientWeight)
        + (evidence.Reach.Travel.As(LengthUnit.Millimeter) / objective.TravelScale.As(LengthUnit.Millimeter) * objective.TravelWeight)
        + ((evidence.Datum.TransferError.As(LengthUnit.Millimeter) / objective.DatumScale.As(LengthUnit.Millimeter)
            + evidence.Datum.AngularTransferError.As(AngleUnit.Radian) / objective.DatumAngleScale.As(AngleUnit.Radian)) * objective.DatumWeight)
        + ((1.0 / Math.Max(evidence.RigidityMargin, 1e-9)) * objective.RigidityWeight)
        + (((evidence.Reach.JacobianCondition / objective.ConditionScale)
            + (operation.Duration.As(DurationUnit.Second) / objective.TimeScale.As(DurationUnit.Second))) * objective.RiskWeight)
        + setup.Carrier.Match(
            Some: carrier => carrier.ToolChange.As(DurationUnit.Second) / objective.TimeScale.As(DurationUnit.Second)
                / Math.Max(setup.Instances.Count, 1) * objective.ChangeWeight,
            None: static () => 0.0);

    static double BoundOf(SetupOperation operation, SetupObjective objective) =>
        operation.Duration.As(DurationUnit.Second) / objective.TimeScale.As(DurationUnit.Second) * objective.RiskWeight;

    static Option<ScheduleState> Better(Option<ScheduleState> current, Option<ScheduleState> candidate) =>
        current.Match(
            Some: best => candidate.Match(Some: next => next.Cost < best.Cost ? candidate : current, None: () => current),
            None: () => candidate);

    // A search that refused nothing proved its incumbent optimal, so the bound is the cost itself;
    // a search that cut a branch proves only the least bound it refused.
    static SetupSchedule Finalize(SetupPlan plan, BidirectionalGraph<int, SetupEdge> graph, ScheduleState state, double cut) {
        Seq<SetupEdge> reduced = graph.ComputeTransitiveReduction(static (source, target) => new SetupEdge(source, target, new SetupRelation.Precedes())).Edges.ToSeq()
            .Bind(edge => graph.Edges.Filter(reason => reason.Source == edge.Source && reason.Target == edge.Target)).Distinct().ToSeq();
        double lowerBound = double.IsPositiveInfinity(cut) ? state.Cost : Math.Min(cut, state.Cost);
        Option<double> proof = Some(lowerBound);
        ContentKey key = ContentKey.Of(EgressKind.Plan, Canonical(state.Setups, state.Decisions, reduced, state.Cost, proof));
        return new SetupSchedule(plan, state.Setups, state.Setups.Map(static setup => new WcsAssignment(setup.Index, setup.Wcs)).ToSeq(),
            reduced, state.Decisions, state.Cost, proof, key);
    }

    // Measured frames move the part, so planned-frame reach, clearance, and stability evidence
    // no longer describe this setup: correction re-enters through the same
    // evidence boundary that admitted it, and a correction past the operation's own datum
    // tolerance is a rejection, never a traceable rebase.
    static Fin<SetupSchedule> Rebase(SetupSchedule schedule, int setup, Plane measured) {
        int position = schedule.Setups.TakeWhile(row => row.Index != setup).Count;
        if (position >= schedule.Setups.Count || !measured.IsValid)
            return Fin.Fail<SetupSchedule>(new FabricationFault.FixtureInadmissible(new FixturingWitness.Rebase(
                setup, Length.Zero, Angle.Zero, Length.Zero)).ToError());
        Setup held = schedule.Setups[position];
        Transform correction = Transform.PlaneToPlane(held.Mounting.Frame, measured);
        Length offset = Length.FromMillimeters(held.Mounting.Frame.Origin.DistanceTo(measured.Origin));
        Angle rotation = Rotation(correction);
        Seq<SetupOperation> operations = schedule.Plan.Operations.Filter(row => held.Operations.Contains(row.Key));
        Option<Length> tolerance = operations.Map(static row => row.DatumTolerance).Fold(Option<Length>.None,
            static (least, row) => least.Filter(held => held <= row).IfNone(row));
        Option<Angle> angular = operations.Map(static row => row.DatumAngularTolerance).Fold(Option<Angle>.None,
            static (least, row) => least.Filter(held => held <= row).IfNone(row));
        if (tolerance.Exists(bound => offset > bound) || angular.Exists(bound => rotation > bound))
            return Fin.Fail<SetupSchedule>(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Rebase(setup, offset, rotation, tolerance.IfNone(Length.Zero))).ToError());
        DatumReceipt datum = held.Datum with {
            ProbeCorrection = offset,
            AngularProbeCorrection = rotation,
            Probed = true,
            Traceable = held.Datum.Traceable,
        };
        Mounting reframed = held.Mounting.Reframed(measured);
        return operations.Traverse(row =>
            Evidence(row, held.Machine, held.Fixture, reframed, schedule.Plan)
                .Bind(evidence => evidence.ToFin(FabricationFault.SetupInfeasible(row.Key, schedule.Setups.Count).ToError()))
                .ToValidation())
            .As().ToFin().Map(proven => {
                HashMap<int, SetupEvidence> reproven = proven.Fold(HashMap<int, SetupEvidence>(),
                    static (index, evidence) => index.Add(evidence.Operation, evidence));
                Setup next = held with {
                    Mounting = reframed,
                    InstanceWcs = held.InstanceWcs.Map(row => row with { Frame = correction * row.Frame }),
                    Datum = datum,
                };
                HashMap<int, SetupOperation> operationIndex = operations.Fold(HashMap<int, SetupOperation>(),
                    static (index, operation) => index.Add(operation.Key, operation));
                Seq<SetupDecision> decisions = schedule.Decisions.Map(decision => {
                    if (decision.Setup != setup) return decision;
                    SetupEvidence evidence = reproven[decision.Operation] with { Datum = datum };
                    double incremental = Cost(operationIndex[decision.Operation], next, evidence, decision.Extended, schedule.Plan.Objective);
                    return decision with { Evidence = evidence, IncrementalCost = incremental };
                });
                SetupSchedule draft = schedule with {
                    Setups = schedule.Setups.SetItem(position, next),
                    Decisions = decisions,
                    Cost = decisions.Sum(static decision => decision.IncrementalCost),
                    ProvenLowerBound = None,
                };
                return draft with { Key = ContentKey.Of(EgressKind.Plan,
                    Canonical(draft.Setups, draft.Decisions, draft.Precedence, draft.Cost, draft.ProvenLowerBound)) };
            });
    }

    static Angle Rotation(Transform correction) => new(Math.Acos(Math.Clamp(
        0.5 * ((correction.M00 + correction.M11 + correction.M22) - 1.0), -1.0, 1.0)), AngleUnit.Radian);

    // Every adjacent collection is count-framed and every discriminant rides its own ordinal with
    // framed payload: separator-joined keys let a colon inside a pallet name shift two field
    // splits onto one digest, and round-trip angle text keys a value the buffer already holds
    // exactly.
    static ReadOnlySpan<byte> Canonical(
        Arr<Setup> setups,
        Seq<SetupDecision> decisions,
        Seq<SetupEdge> precedence,
        double cost,
        Option<double> provenLowerBound) {
        using ArrayPoolBufferWriter<byte> buffer = new();
        Frame(buffer, setups.ToSeq(), static (held, setup) => {
            Write(held, setup.Index); Write(held, setup.Machine); Write(held, setup.Fixture.Operation);
            WriteMounting(held, setup.Mounting); WriteWcs(held, setup.Wcs);
            Write(held, setup.Mounting.Frame);
            Frame(held, setup.Operations.ToSeq(), Write);
            Frame(held, setup.Instances, static (inner, instance) => {
                Write(inner, instance.Key); Write(inner, instance.Operation); Write(inner, instance.Carrier.Value); Write(inner, instance.Station);
                Write(inner, instance.LocalFrame);
            });
            Frame(held, setup.InstanceWcs, static (inner, instance) => { Write(inner, instance.Instance); WriteWcs(inner, instance.Slot); });
            Write(held, setup.Start.As(DurationUnit.Second)); Write(held, setup.Finish.As(DurationUnit.Second));
            Write(held, setup.Datum.Anchor); Write(held, setup.Datum.Probed ? 1 : 0); Write(held, setup.Datum.Traceable ? 1 : 0);
            Frame(held, setup.Datum.Lineage, Write);
            Write(held, setup.Datum.TransferError.As(LengthUnit.Millimeter));
            Write(held, setup.Datum.AngularTransferError.As(AngleUnit.Radian));
            Write(held, setup.Datum.ProbeCorrection.As(LengthUnit.Millimeter));
            Write(held, setup.Datum.AngularProbeCorrection.As(AngleUnit.Radian));
        });
        Frame(buffer, decisions, static (held, decision) => {
            Write(held, decision.Operation); Write(held, decision.Setup); Write(held, decision.Extended ? 1 : 0);
            Write(held, decision.IncrementalCost); Write(held, decision.Bound);
            Write(held, decision.Evidence.Key);
        });
        Frame(buffer, precedence, static (held, edge) => {
            Write(held, edge.Source); Write(held, edge.Target); WriteRelation(held, edge.Relation);
        });
        Write(buffer, cost);
        Write(buffer, provenLowerBound.IsSome ? 1 : 0);
        provenLowerBound.Iter(value => Write(buffer, value));
        return buffer.WrittenSpan.ToArray();
    }

    static Unit WriteMounting(ArrayPoolBufferWriter<byte> buffer, Mounting mounting) => mounting.Switch(
        state: buffer,
        table: static (held, _) => Tag(held, 0),
        pallet: static (held, row) => Tag(held, 1, row.Key),
        tombstone: static (held, row) => Tag(held, 2, row.Key, row.Face),
        rotary: static (held, row) => Tag(held, 3, row.Axis, row.Angle.As(AngleUnit.Radian)),
        trunnion: static (held, row) => Tag(held, 4, row.A.As(AngleUnit.Radian), row.C.As(AngleUnit.Radian)),
        spindle: static (held, _) => Tag(held, 5),
        positioner: static (held, row) => Tag(held, 6, row.Key),
        cell: static (held, _) => Tag(held, 7));

    static Unit WriteWcs(ArrayPoolBufferWriter<byte> buffer, WcsSlot slot) => slot.Switch(
        state: buffer,
        @base: static (held, row) => Tag(held, 0, row.Ordinal),
        extended: static (held, row) => Tag(held, 1, row.Ordinal),
        dynamic: static (held, row) => Tag(held, 2, row.Ordinal),
        rotary: static (held, row) => Tag(held, 3, row.Ordinal, row.Axis),
        local: static (held, row) => Tag(held, 4, row.Ordinal, row.Parent));

    static Unit WriteRelation(ArrayPoolBufferWriter<byte> buffer, SetupRelation relation) => relation.Switch(
        state: buffer,
        precedes: static (held, _) => Tag(held, 0),
        datum: static (held, _) => Tag(held, 1),
        stock: static (held, _) => Tag(held, 2),
        probe: static (held, _) => Tag(held, 3),
        resource: static (held, row) => Tag(held, 4, row.Key),
        sameFixture: static (held, _) => Tag(held, 5),
        sameOrientation: static (held, _) => Tag(held, 6));

    static Unit Tag(ArrayPoolBufferWriter<byte> buffer, int discriminant) { Write(buffer, discriminant); return unit; }
    static Unit Tag(ArrayPoolBufferWriter<byte> buffer, int discriminant, int first) { Write(buffer, discriminant); Write(buffer, first); return unit; }
    static Unit Tag(ArrayPoolBufferWriter<byte> buffer, int discriminant, int first, int second) { Write(buffer, discriminant); Write(buffer, first); Write(buffer, second); return unit; }
    static Unit Tag(ArrayPoolBufferWriter<byte> buffer, int discriminant, int first, double second) { Write(buffer, discriminant); Write(buffer, first); Write(buffer, second); return unit; }
    static Unit Tag(ArrayPoolBufferWriter<byte> buffer, int discriminant, double first, double second) { Write(buffer, discriminant); Write(buffer, first); Write(buffer, second); return unit; }
    static Unit Tag(ArrayPoolBufferWriter<byte> buffer, int discriminant, string first) { Write(buffer, discriminant); Write(buffer, first); return unit; }
    static Unit Tag(ArrayPoolBufferWriter<byte> buffer, int discriminant, string first, int second) { Write(buffer, discriminant); Write(buffer, first); Write(buffer, second); return unit; }

    static void Frame<T>(ArrayPoolBufferWriter<byte> buffer, Seq<T> rows, Action<ArrayPoolBufferWriter<byte>, T> write) {
        Write(buffer, rows.Count);
        _ = rows.Iter(row => write(buffer, row));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, Plane frame) {
        Write(buffer, frame.Origin.X); Write(buffer, frame.Origin.Y); Write(buffer, frame.Origin.Z);
        Write(buffer, frame.XAxis.X); Write(buffer, frame.XAxis.Y); Write(buffer, frame.XAxis.Z);
        Write(buffer, frame.YAxis.X); Write(buffer, frame.YAxis.Y); Write(buffer, frame.YAxis.Z);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(sizeof(int)), value);
        buffer.Advance(sizeof(int));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, double value) {
        BinaryPrimitives.WriteInt64LittleEndian(buffer.GetSpan(sizeof(long)), BitConverter.DoubleToInt64Bits(value));
        buffer.Advance(sizeof(long));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, ContentKey value) {
        Write(buffer, value.Kind.Key);
        UInt128 digest = value.Digest;
        BinaryPrimitives.WriteUInt64LittleEndian(buffer.GetSpan(sizeof(ulong)), (ulong)digest);
        buffer.Advance(sizeof(ulong));
        BinaryPrimitives.WriteUInt64LittleEndian(buffer.GetSpan(sizeof(ulong)), (ulong)(digest >> 64));
        buffer.Advance(sizeof(ulong));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, string value) {
        int length = Encoding.UTF8.GetByteCount(value);
        Write(buffer, length);
        Encoding.UTF8.GetBytes(value, buffer.GetSpan(length));
        buffer.Advance(length);
    }
}

public sealed record SetupChain(Seq<int> Operations, Seq<(int Before, int After)> Lineage) {
    public ReadOnlySpan<byte> Canonical {
        get {
            using ArrayPoolBufferWriter<byte> buffer = new();
            BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(sizeof(int)), Operations.Count);
            buffer.Advance(sizeof(int));
            _ = Operations.Iter(operation => {
                BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(sizeof(int)), operation);
                buffer.Advance(sizeof(int));
            });
            BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(sizeof(int)), Lineage.Count);
            buffer.Advance(sizeof(int));
            _ = Lineage.Iter(edge => {
                BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(sizeof(int)), edge.Before);
                buffer.Advance(sizeof(int));
                BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(sizeof(int)), edge.After);
                buffer.Advance(sizeof(int));
            });
            return buffer.WrittenSpan.ToArray();
        }
    }
}
```

## [04]-[PROJECTION]

- Owner: `SetupProjection` selects machine, probing, posting, traveler, inspection, and evidence views; `SetupArtifact` carries the selected view without reopening the schedule.
- Egress: projection preserves the keyed schedule result, WCS, precedence, datum lineage, evidence, cost, and proven bound; raw search policy and evidence-provider capabilities remain ingress-only.
- Exemption: `Canonical`, `Frame`, `Tag`, and the three union writers are the boundary serialization kernel, and `Rebase` is the bounded frame-correction kernel; statements remain inside those seams, every adjacent collection count-framed and every discriminant an ordinal beside its framed payload.
- Boundary: posting receives WCS identity and values, probing receives datum and correction targets, and documentation receives immutable schedule evidence; no consumer derives setup order from array position alone.

```csharp signature
// --- [PROJECTION] ---------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SetupProjection {
    public static readonly SetupProjection Machine = new("machine");
    public static readonly SetupProjection Probing = new("probing");
    public static readonly SetupProjection Posting = new("posting");
    public static readonly SetupProjection Traveler = new("traveler");
    public static readonly SetupProjection Inspection = new("inspection");
    public static readonly SetupProjection Evidence = new("evidence");
}

[Union]
public abstract partial record SetupArtifact {
    private SetupArtifact() { }

    public sealed record Machine(ContentKey Key, Arr<Setup> Setups, Seq<WcsAssignment> Wcs) : SetupArtifact;
    public sealed record Probing(ContentKey Key, Seq<DatumReceipt> Datums) : SetupArtifact;
    public sealed record Posting(ContentKey Key, Seq<WcsAssignment> Wcs, Seq<SetupEdge> Precedence) : SetupArtifact;
    public sealed record Traveler(ContentKey Key, Arr<Setup> Setups, Seq<SetupDecision> Decisions) : SetupArtifact;
    public sealed record Inspection(ContentKey Key, Seq<SetupEvidence> Evidence) : SetupArtifact;
    public sealed record Evidence(
        ContentKey Key,
        Arr<Setup> Setups,
        Seq<WcsAssignment> Wcs,
        Seq<SetupEdge> Precedence,
        Seq<SetupDecision> Decisions,
        double Cost,
        Option<double> ProvenLowerBound) : SetupArtifact;
}

public sealed partial record SetupSchedule {
    static Fin<SetupArtifact> Project(SetupSchedule? schedule, SetupProjection? projection) =>
        (Optional(schedule).ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Absent()).ToError()),
         Optional(projection).ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Absent()).ToError()))
            .Apply((accepted, kind) => kind.Switch<SetupArtifact>(
                machine: () => new SetupArtifact.Machine(accepted.Key, accepted.Setups, accepted.Wcs),
                probing: () => new SetupArtifact.Probing(accepted.Key, accepted.Setups.Map(static setup => setup.Datum).ToSeq()),
                posting: () => new SetupArtifact.Posting(accepted.Key, accepted.Wcs, accepted.Precedence),
                traveler: () => new SetupArtifact.Traveler(accepted.Key, accepted.Setups, accepted.Decisions),
                inspection: () => new SetupArtifact.Inspection(accepted.Key, accepted.Decisions.Map(static decision => decision.Evidence)),
                evidence: () => new SetupArtifact.Evidence(accepted.Key, accepted.Setups, accepted.Wcs, accepted.Precedence,
                    accepted.Decisions, accepted.Cost, accepted.ProvenLowerBound)))
            .As().ToFin();
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
