# [RASM_FABRICATION_SETUPS]

`SetupSchedule` owns operation precedence, datum lineage, fixture and machine assignment, physical reorientation, carrier loading, per-instance work-offset allocation, transfer, probing, stock continuity, and schedule evidence. `SetupPlan` admits every operation, relation, fixture candidate, carrier station, part instance, resource row, and controller slot once; the search consumes only admitted identities and typed evidence.

`Fixture`, `FixtureSet`, `ExclusionZone`, `Setup`, `WcsSlot`, and `SetupSchedule` remain the in-process wire vocabulary. `SetupSchedule.Apply` closes admission, scheduling, rebasing, and projection over `SetupOp`, and every egress addresses through the `Process/owner#RUN_DISPATCH` `FabricationCanon.Keyed` close, so a preimage that never retained its bytes refuses on the error channel instead of forging an address. Scalar admission predicates, the `DatumTransfer` fold over the `Joining/sequence` `DistortionField`, and the restraint and clearance entries all compose `workholding#EVALUATION`, so this page re-declares no quantity guard and no holding algebra. Cost is one `SetupAxis` row table over `CostTerms`, so an objective weight, its scale, and its term travel together and the branch-and-bound bound reads the same rows the incumbent cost does. Every preimage composes `Process/owner#RUN_DISPATCH` `FabricationCanon` over the one `Rasm.Element` `CanonicalWriter`.

## [01]-[INDEX]

- [02]-[VOCABULARY]: operation, relation, cost-axis, mounting, work-offset, carrier, and evidence owners.
- [03]-[SCHEDULE]: graph admission, the admissible assignment bound, bounded partitioning, datum transfer, rebasing, and WCS allocation.
- [04]-[PROJECTION]: machine, probing, posting, traveler, inspection, and evidence egress.

## [02]-[VOCABULARY]

- Owner: `SetupOperation` carries one physical operation instance over a `SetupRoster` of admissible resources and a `SetupDemand` of dimensioned requirements; `SetupRelation` carries precedence, datum, stock, probe, and resource edges without collapsing them into an untyped pair.
- Owner: `SetupAxis` is the cost algebra — one row per objective axis carrying the term it reads off `CostTerms` under `SetupScales`, so `SetupObjective` is a weight per row and a scale bundle rather than twelve parallel columns, and adding an axis changes no cost expression.
- Owner: `WcsSlot` is a closed payload family for base, extended, dynamic, rotary, and local offsets; controller syntax remains posting-owned. `Carrier`, `CarrierStation`, and `PartInstance` model pallet and tombstone occupancy, station frames, derived local offsets, and amortized tool-change cost without cloning operations.
- Law: this `PartInstance` is a part MOUNTED at a carrier station for one operation, and it absorbs nothing from `Nesting/nfp`. That page's same-named pair is a part id beside a COPY ORDINAL inside a nest — its second column counts copies, not operations — and it keys a genetic-algorithm ordering. Merging would push a carrier key, a station index, and a mounting plane onto rows nesting cannot supply and would rename a copy count as an operation key; the two live in different namespaces and share no column by meaning.
- Cases: `Mounting` closes table, pallet, tombstone face, rotary index, trunnion, spindle, robot positioner, and floor-cell mounting.
- Result: `SetupEvidence` retains compatibility, `MachineReach` kinematics with optional robot-cell placement, workholding, clearance, guard, probe, stock, `DatumLineage`, and `ResourceHold` rows; `SetupBoundaryEvidence.Key` fingerprints every provider-owned field before admission.
- Law: `DatumGrade` is the ONE datum-knowledge row — measured, traceable, or both — so a landed rebase takes the roster's own `AfterProbe` transition instead of poking one of two independent booleans and leaving the other to say whatever it already said.
- Growth: a new scheduling concern lands as one relation case, one `SetupAxis` row, one mounting case, or one evidence field; no delegate column or entrypoint appears beside the owner.
- Boundary: scalar admission is `workholding#EVALUATION` `Fixtures`, so a `As(unit) >= 0 && double.IsFinite(...)` clause spelled at this page is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Assignment;
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
using System.Threading;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Fixturing;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WcsSlot {
    private WcsSlot() { }

    public sealed record Base(int Ordinal) : WcsSlot;
    public sealed record Extended(int Ordinal) : WcsSlot;
    public sealed record Dynamic(int Ordinal) : WcsSlot;
    public sealed record Rotary(int Ordinal, int Axis) : WcsSlot;
    public sealed record Local(int Ordinal, int Parent) : WcsSlot;

    public Option<int> Controller => Switch<Option<int>>(
        @base: static row => Some(row.Ordinal),
        extended: static row => Some(row.Ordinal),
        dynamic: static row => Some(row.Ordinal),
        rotary: static row => Some(row.Ordinal),
        local: static _ => None);

    public bool Valid(Seq<WcsSlot> roster) => Switch(
        state: roster,
        @base: static (_, row) => ValidityClaim.Nonnegative(row.Ordinal),
        extended: static (_, row) => ValidityClaim.Nonnegative(row.Ordinal),
        dynamic: static (_, row) => ValidityClaim.Nonnegative(row.Ordinal),
        rotary: static (_, row) => ValidityClaim.All(ValidityClaim.Nonnegative(row.Ordinal), ValidityClaim.Nonnegative(row.Axis)),
        local: static (slots, row) => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Ordinal), ValidityClaim.Nonnegative(row.Parent), row.Parent != row.Ordinal,
            slots.Count(slot => slot.Controller.Contains(row.Parent)) == 1));

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => Switch(
        state: writer,
        @base: static (held, row) => held.String(nameof(Base)).Ordinal(row.Ordinal),
        extended: static (held, row) => held.String(nameof(Extended)).Ordinal(row.Ordinal),
        dynamic: static (held, row) => held.String(nameof(Dynamic)).Ordinal(row.Ordinal),
        rotary: static (held, row) => held.String(nameof(Rotary)).Ordinal(row.Ordinal).Ordinal(row.Axis),
        local: static (held, row) => held.String(nameof(Local)).Ordinal(row.Ordinal).Ordinal(row.Parent));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
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

    public bool IsValid => Frame.IsValid && Switch(
        table: static _ => true,
        pallet: static row => Witness.Keyed(row.Key),
        tombstone: static row => ValidityClaim.All(Witness.Keyed(row.Key), ValidityClaim.Nonnegative(row.Face)),
        rotary: static row => ValidityClaim.All(ValidityClaim.Nonnegative(row.Axis), Fixtures.Finite(row.Angle)),
        trunnion: static row => Fixtures.Finite(row.A) && Fixtures.Finite(row.C),
        spindle: static _ => true,
        positioner: static row => Witness.Keyed(row.Key),
        cell: static _ => true);

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => Switch(
        state: writer,
        table: static (held, _) => held.String(nameof(Table)),
        pallet: static (held, row) => held.String(nameof(Pallet)).String(row.Key),
        tombstone: static (held, row) => held.String(nameof(Tombstone)).String(row.Key).Ordinal(row.Face),
        rotary: static (held, row) => held.String(nameof(Rotary)).Ordinal(row.Axis).Double(row.Angle.As(AngleUnit.Radian)),
        trunnion: static (held, row) => held.String(nameof(Trunnion))
            .Double(row.A.As(AngleUnit.Radian)).Double(row.C.As(AngleUnit.Radian)),
        spindle: static (held, _) => held.String(nameof(Spindle)),
        positioner: static (held, row) => held.String(nameof(Positioner)).String(row.Key),
        cell: static (held, _) => held.String(nameof(Cell)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
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

    public bool IsValid => Switch(
        precedes: static _ => true,
        datum: static _ => true,
        stock: static _ => true,
        probe: static _ => true,
        resource: static row => Witness.Keyed(row.Key),
        sameFixture: static _ => true,
        sameOrientation: static _ => true);

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => Switch(
        state: writer,
        precedes: static (held, _) => held.String(nameof(Precedes)),
        datum: static (held, _) => held.String(nameof(Datum)),
        stock: static (held, _) => held.String(nameof(Stock)),
        probe: static (held, _) => held.String(nameof(Probe)),
        resource: static (held, row) => held.String(nameof(Resource)).String(row.Key),
        sameFixture: static (held, _) => held.String(nameof(SameFixture)),
        sameOrientation: static (held, _) => held.String(nameof(SameOrientation)));
}

public readonly record struct SetupEdge(int Source, int Target, SetupRelation Relation) : IEdge<int>;

public readonly record struct CostTerms(
    bool Extended,
    Duration Changeover,
    Duration Reorientation,
    Length Travel,
    Length DatumError,
    Angle AngularDatumError,
    double RigidityMargin,
    double JacobianCondition,
    Duration Operation,
    Duration ToolChange,
    int Instances);

public readonly record struct SetupScales(
    Duration Time,
    Length Travel,
    Length Datum,
    Angle DatumAngle,
    double Condition) {
    public bool IsValid => ValidityClaim.All(
        Fixtures.Positive(Time), Fixtures.Positive(Travel), Fixtures.Positive(Datum),
        Fixtures.Positive(DatumAngle), ValidityClaim.Positive(Condition));
}

[SmartEnum<string>]
public sealed partial class SetupAxis {
    public static readonly SetupAxis Setup = new("setup", bounding: false,
        static (terms, _) => terms.Extended ? 0.0 : 1.0);
    public static readonly SetupAxis Change = new("change", bounding: false,
        static (terms, scales) => (terms.Changeover.As(DurationUnit.Second)
            + (terms.ToolChange.As(DurationUnit.Second) / Math.Max(terms.Instances, 1)))
            / scales.Time.As(DurationUnit.Second));
    public static readonly SetupAxis Orient = new("orient", bounding: false,
        static (terms, scales) => terms.Reorientation.As(DurationUnit.Second) / scales.Time.As(DurationUnit.Second));
    public static readonly SetupAxis Travel = new("travel", bounding: false,
        static (terms, scales) => terms.Travel.As(LengthUnit.Millimeter) / scales.Travel.As(LengthUnit.Millimeter));
    public static readonly SetupAxis Datum = new("datum", bounding: false,
        static (terms, scales) => (terms.DatumError.As(LengthUnit.Millimeter) / scales.Datum.As(LengthUnit.Millimeter))
            + (terms.AngularDatumError.As(AngleUnit.Radian) / scales.DatumAngle.As(AngleUnit.Radian)));
    public static readonly SetupAxis Rigidity = new("rigidity", bounding: true,
        static (terms, _) => 1.0 / Math.Max(terms.RigidityMargin, EpsilonPolicy.SqrtEpsilon));
    public static readonly SetupAxis Risk = new("risk", bounding: true,
        static (terms, scales) => (terms.JacobianCondition / scales.Condition)
            + (terms.Operation.As(DurationUnit.Second) / scales.Time.As(DurationUnit.Second)));

    public bool Bounding { get; }
    public Func<CostTerms, SetupScales, double> Term { get; }
}

[ComplexValueObject]
public sealed partial class SetupObjective {
    public Map<SetupAxis, double> Weights { get; }
    public SetupScales Scales { get; }

    public double Weight(SetupAxis axis) => Weights.Find(axis).IfNone(0.0);
    public double Total => Weights.Values.Sum();

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Map<SetupAxis, double> weights,
        ref SetupScales scales) {
        if (!(ValidityClaim.All(
            toSeq(SetupAxis.Items).ForAll(axis => weights.Find(axis).Exists(static weight => double.IsFinite(weight) && weight >= 0.0)),
            ValidityClaim.Positive(weights.Values.Sum()), scales.IsValid)))
            validationError = new ValidationError("setup-objective");
    }

    public static Fin<SetupObjective> Admit(Map<SetupAxis, double> weights, SetupScales scales) =>
        Validate(weights, scales, out SetupObjective objective).Admitted(objective);

    public double Cost(CostTerms terms) =>
        toSeq(SetupAxis.Items).Sum(axis => Weight(axis) * axis.Term(terms, Scales));

    public double Bound(CostTerms terms) =>
        toSeq(SetupAxis.Items).Filter(static axis => axis.Bounding).Sum(axis => Weight(axis) * axis.Term(terms, Scales));
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct CarrierKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        if (!Witness.Keyed(value))
            validationError = new ValidationError("carrier-key");
    }

    public static Fin<CarrierKey> Admit(string value) => Admission.OfValue<CarrierKey, string>(value);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct CarrierStation(int Index, Plane Frame, WcsSlot Wcs);

public sealed record Carrier(CarrierKey Key, Mounting Mounting, Seq<CarrierStation> Stations, Duration ToolChange);

public readonly record struct PartInstance(int Key, int Operation, CarrierKey Carrier, int Station, Plane LocalFrame);

public readonly record struct InstanceWcs(int Instance, WcsSlot Slot, Transform Frame);

public sealed record SetupRoster(
    Seq<int> Features,
    Seq<Mounting> Mountings,
    Seq<int> FixtureKeys,
    Seq<int> MachineKeys,
    Seq<string> Resources);

public readonly record struct SetupDemand(
    Duration Duration,
    Length DatumTolerance,
    Angle DatumAngularTolerance,
    double RigidityDemand,
    Ratio SafetyFactor,
    bool RequiresProbe);

public sealed record SetupOperation(
    int Key,
    int Process,
    SetupRoster Roster,
    Seq<LoadCase> Loads,
    Seq<ToolCorridor> Corridors,
    SetupDemand Demand);

public readonly record struct MachineReach(
    int Machine,
    Arr<Angle> Axes,
    double JacobianCondition,
    Length Clearance,
    Length Travel,
    Duration Reorientation,
    Option<CellPlacement> Robot) {
    public bool Reachable => ValidityClaim.All(
        !Axes.IsEmpty, Axes.ForAll(Fixtures.Finite),
        ValidityClaim.Positive(JacobianCondition),
        Fixtures.Nonnegative(Clearance), Fixtures.Nonnegative(Travel),
        Robot.ForAll(static result => double.IsFinite(result.Selected.Score)));
}

[SmartEnum<string>]
public sealed partial class DatumGrade {
    public static readonly DatumGrade Nominal = new("nominal", measured: false, traceable: false);
    public static readonly DatumGrade Probed = new("probed", measured: true, traceable: false);
    public static readonly DatumGrade Chained = new("chained", measured: false, traceable: true);
    public static readonly DatumGrade Certified = new("certified", measured: true, traceable: true);

    public bool Measured { get; }
    public bool Traceable { get; }

    public DatumGrade AfterProbe => Traceable ? Certified : Probed;
}

public readonly record struct DatumLineage(
    int Anchor,
    Seq<int> Lineage,
    Length TransferError,
    Angle AngularTransferError,
    Length ProbeCorrection,
    Angle AngularProbeCorrection,
    DatumGrade Grade) {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Nonnegative(Anchor),
        Lineage.ForAll(static value => ValidityClaim.Nonnegative(value).Holds), Lineage.Distinct().Count == Lineage.Count,
        Fixtures.Nonnegative(TransferError), Fixtures.Nonnegative(AngularTransferError),
        Fixtures.Nonnegative(ProbeCorrection), Fixtures.Nonnegative(AngularProbeCorrection));

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Ordinal(Anchor).Discriminant(Grade)
        .Rows(Lineage, static (held, anchor) => held.Ordinal(anchor))
        .Double(TransferError.As(LengthUnit.Millimeter))
        .Double(AngularTransferError.As(AngleUnit.Radian))
        .Double(ProbeCorrection.As(LengthUnit.Millimeter))
        .Double(AngularProbeCorrection.As(AngleUnit.Radian));
}

public readonly record struct ResourceHold(Seq<string> Held, Duration Changeover, bool Available) {
    public bool IsValid => ValidityClaim.All(Held.ForAll(Witness.Keyed), Fixtures.Nonnegative(Changeover));
}

public readonly record struct SetupBoundaryEvidence(
    MachineReach Reach,
    bool Guarded,
    DatumLineage Datum,
    ResourceHold Resources,
    ContentKey Key);

// --- [SERVICES] ------------------------------------------------------------------------
public interface ISetupEvidenceSource {
    Fin<SetupBoundaryEvidence> Evaluate(int machine, SetupOperation operation, Fixture fixture, Mounting mounting);
}

public sealed record SetupEvidence(
    int Operation,
    bool Compatible,
    MachineReach Reach,
    RestraintProof Holding,
    Seq<WorkholdingResult.Clearance> Clearance,
    bool Guarded,
    Option<Point3d> MachinedHit,
    DatumLineage Datum,
    ResourceHold Resources,
    double RigidityMargin,
    ContentKey Key) {
    public bool IsValid => ValidityClaim.All(Key.Kind == EgressKind.Plan, Datum.IsValid, Resources.IsValid);
}
```

## [03]-[SCHEDULE]

- Owner: `SetupPlan` is raw ingress over one `SetupCatalog` of shop resources and one `SearchBudget`; `Setup` is one admitted physical orientation and resource custody interval; `SetupSchedule` is the proof-bearing ordered result; `Setups` owns every fold.
- Law: the branch-and-bound bound is column-DEPENDENT and admissible. `SearchSpace` memoizes one pair bound per operation-and-fixture — the `SetupAxis.Bounding` rows alone, whose rigidity term reads that fixture's own restraint margin — so an operation's remainder is the minimum over its admissible fixtures and no relaxation exceeds the cost it bounds. A pair-independent seed makes every remainder equal to the incumbent's own accumulated cost, which prunes the root and refuses every plan; that form is deleted.
- Law: `HungarianAlgorithm` over that same pair-bound matrix is the ROOT lower bound and an infeasibility oracle in one solve — no perfect operation-to-fixture matching means no schedule exists, so `SetupInfeasible` answers before any node expands. The search itself opens with no incumbent, `Cut` records the least bound it refused, and `ProvenLowerBound` publishes the stronger of the root bound and that cut, clipped to the incumbent cost — refusing nothing proves the incumbent optimal.
- Law: `Search` reads its exact execution token at every node and lowers `Errors.Cancelled` when requested; cancellation is neither a policy refusal nor a thrown control path.
- Law: controller and carrier-station WCS rows come from the unconsumed admitted roster remainder; setup indices never derive controller syntax, array position, or offset availability, and makespan accumulates per machine so setups on distinct machines do not serialize.
- Law: transitive reduction takes NO edge factory and returns the ORIGINAL edges, so the surviving pair set is a `Set<(int, int)>` read once and the original relation rows filter against it in one pass — the per-edge rescan of the whole edge list is quadratic in the relation count and is deleted.
- Law: a measured frame re-enters through the same evidence boundary that admitted the setup, and a correction exceeding the tightest datum tolerance the setup's operations carry rejects rather than stamping traceability. The `Joining/sequence` `DistortionField` narrows that tolerance through `workholding#FIXTURE` `DatumTransfer` before the comparison, so a distortion the weld plane measured consumes the datum budget rather than being re-estimated.
- Exemption: `RootBound` fills the rectangular integer cost matrix `HungarianAlgorithm` binds, and `Search` threads the bounded scheduling recursion. NO shipped operator covers a cost-bounded assignment search over a precedence order with per-node evidence admission — QuikGraph ships the two questions this fold CAN delegate and both are delegated, `HungarianAlgorithm` for the root bound and infeasibility oracle and `ComputeTransitiveReduction` for the result, so the hand-threaded part is the branch-and-bound recursion alone. Mutation stays inside admitted graph, draft-state, and `Atom` containers; `SetupDraft` is the search's own partial schedule and `SetupSchedule` the proof-bearing result `Finalize` mints from it.
- Entry: identity, relation references, WCS slots, carrier stations, part instances, machine keys, fixture keys, mounting frames, objective values, and operation payloads accumulate before graph construction.
- Auto: one applicative evidence fan-in composes machine or robot-cell reach, rebuilt workholding restraint and corridor checks, guard, machined-stock, datum transfer, probing, and resource availability.
- Result: the scheduled arm writes the decision count through the optional mounted instrument set `Apply` accepts. A cyclic precedence graph publishes its strongly-connected COMPONENT MEMBERS on `SetupChain.Components`, so the refusal names the operations a caller must break rather than a count.
- Packages: `BidirectionalGraph<int, SetupEdge>` preserves isolated operations, typed edge payloads, source-first order, strongly connected cycle evidence, and transitive reduction; `HungarianAlgorithm` at `QuikGraph.Algorithms.Assignment` binds the rectangular `int[,]` cost matrix alone.
- Boundary: ordinary infeasibility prunes one candidate as `Option.None`; malformed input, failed geometry, exhausted budget, withdrawn run, or boundary failure remains a typed `Fin` failure.

```csharp
// --- [SCHEDULE] ------------------------------------------------------------------------
public sealed record SetupCatalog(
    FixtureSet Fixtures,
    Seq<int> Machines,
    Seq<WcsSlot> Wcs,
    Seq<Carrier> Carriers,
    Seq<PartInstance> Instances);

public readonly record struct SearchBudget(int MaxSetups, int NodeBudget);

public sealed record SetupPlan(
    Seq<SetupOperation> Operations,
    Seq<SetupEdge> Relations,
    SetupCatalog Catalog,
    SetupObjective Objective,
    SearchBudget Budget,
    Option<DistortionField> Distortion,
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
    DatumLineage Datum,
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

public sealed record SetupDraft(
    Arr<Setup> Setups,
    Seq<SetupDecision> Decisions,
    Set<int> Placed,
    double Cost) {
    public static SetupDraft Empty => new(Arr<Setup>(), Seq<SetupDecision>(), Set<int>(), 0.0);
}

public readonly record struct WcsAssignment(int Setup, WcsSlot Slot);

public sealed record SetupChain(Seq<int> Operations, Seq<Arr<int>> Components, Seq<(int Before, int After)> Lineage) {
    public Fin<ContentKey> Keyed(double toleranceMm, Op key) => FabricationCanon.Keyed(
        EgressKind.Plan,
        toleranceMm,
        writer => writer
            .Rows(Operations, static (held, operation) => held.Ordinal(operation))
            .Rows(Components, static (held, component) => held.Rows(component.ToSeq(), static (row, member) => row.Ordinal(member)))
            .Rows(Lineage, static (held, edge) => held.Ordinal(edge.Before).Ordinal(edge.After)),
        key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SetupOp {
    private SetupOp() { }

    public sealed record Admit(SetupPlan Plan) : SetupOp;
    public sealed record Schedule(SetupPlan Plan) : SetupOp;
    public sealed record Rebase(SetupSchedule Schedule, int Setup, Plane Measured) : SetupOp;
    public sealed record Project(SetupSchedule Schedule, SetupProjection Projection) : SetupOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SetupResult {
    private SetupResult() { }

    public sealed record Admitted(SetupPlan Plan) : SetupResult;
    public sealed record Scheduled(SetupSchedule Schedule) : SetupResult;
    public sealed record Rebased(SetupSchedule Schedule) : SetupResult;
    public sealed record Projected(SetupArtifact Artifact) : SetupResult;
}

public sealed record SetupSchedule(
    SetupPlan Plan,
    Arr<Setup> Setups,
    Seq<WcsAssignment> Wcs,
    Seq<SetupEdge> Precedence,
    Seq<SetupDecision> Decisions,
    double Cost,
    Option<double> ProvenLowerBound,
    ContentKey Key) {
    public static Fin<SetupResult> Apply(SetupOp? candidate, Option<InstrumentSet> set = default, CancellationToken cancel = default) =>
        Optional(candidate)
            .ToFin(FabricationFault.Fixture(new FixturingWitness.Absent()))
            .Bind(op => op.Switch(
                state: (Set: set, Cancel: cancel),
                admit: static (_, row) => Setups.Admit(row.Plan).Map<SetupResult>(static plan => new SetupResult.Admitted(plan)),
                schedule: static (state, row) => Setups.Admit(row.Plan)
                    .Bind(plan => Setups.Solve(plan, state.Cancel))
                    .Bind(schedule =>
                        state.Set.Steps((EnginePhase.Decisions, schedule.Decisions.Count))
                            .Map<SetupResult>(_ => new SetupResult.Scheduled(schedule))),
                rebase: static (_, row) => Setups.Rebase(row.Schedule, row.Setup, row.Measured)
                    .Map<SetupResult>(static schedule => new SetupResult.Rebased(schedule)),
                project: static (_, row) => Setups.Project(row.Schedule, row.Projection)
                    .Map<SetupResult>(static artifact => new SetupResult.Projected(artifact))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Setups {
    private const double BoundScale = 1e4;
    private const int Blocked = int.MaxValue / 4;

    // --- [ADMISSION]
    internal static Fin<SetupPlan> Admit(SetupPlan? candidate) =>
        Optional(candidate)
            .ToFin(FabricationFault.Fixture(new FixturingWitness.Absent()))
            .Bind(static plan =>
                (GatePlan(plan), GateOperations(plan), GateRelations(plan), GateWcs(plan), GateCarriers(plan))
                    .Apply(static (accepted, _, _, _, _) => accepted)
                    .As()
                    .ToFin());

    private static K<Validation<Error>, SetupPlan> GatePlan(SetupPlan plan) =>
        AdmissionSlots.Gate(
            plan.Budget.MaxSetups > 0 && plan.Budget.MaxSetups <= plan.Catalog.Wcs.Count && plan.Budget.NodeBudget > 0
            && !plan.Catalog.Machines.IsEmpty && plan.Catalog.Machines.ForAll(static value => ValidityClaim.Nonnegative(value).Holds)
            && plan.Catalog.Machines.Distinct().Count == plan.Catalog.Machines.Count
            && !plan.Catalog.Fixtures.Fixtures.IsEmpty
            && plan.Catalog.Fixtures.Fixtures.ForAll(static fixture => fixture.Constraint.Constrained),
            FabricationFault.Fixture(new FixturingWitness.Plan(
                plan.Operations.Count,
                plan.Catalog.Machines.Count,
                plan.Catalog.Fixtures.Fixtures.Count,
                plan.Budget.MaxSetups)))
            .Map(_ => plan);

    private static K<Validation<Error>, Unit> GateOperations(SetupPlan plan) {
        Set<int> keys = toSet(plan.Operations.Map(static operation => operation.Key));
        return AdmissionSlots.Gate(
            keys.Count == plan.Operations.Count && plan.Operations.ForAll(operation => Valid(operation, plan)),
            FabricationFault.Fixture(new FixturingWitness.Operation(
                plan.Operations.Find(operation => !Valid(operation, plan)).Map(static operation => operation.Key),
                nameof(SetupOperation))));
    }

    private static K<Validation<Error>, Unit> GateRelations(SetupPlan plan) {
        Set<int> keys = toSet(plan.Operations.Map(static operation => operation.Key));
        return AdmissionSlots.Gate(
            plan.Relations.Distinct().Count == plan.Relations.Count
            && plan.Relations.ForAll(edge => keys.Contains(edge.Source) && keys.Contains(edge.Target)
                && edge.Source != edge.Target && edge.Relation.IsValid),
            Broken(
                new SetupChain(keys.ToSeq(), Seq<Arr<int>>(), plan.Relations.Map(static edge => (edge.Source, edge.Target))),
                Grid(plan.Operations)));
    }

    private static K<Validation<Error>, Unit> GateWcs(SetupPlan plan) =>
        AdmissionSlots.Gate(
            plan.Catalog.Wcs.Count >= plan.Budget.MaxSetups
            && plan.Catalog.Wcs.ForAll(slot => slot.Valid(plan.Catalog.Wcs))
            && plan.Catalog.Wcs.Distinct().Count == plan.Catalog.Wcs.Count,
            FabricationFault.Fixture(new FixturingWitness.Offsets(
                plan.Catalog.Wcs.Count, plan.Catalog.Wcs.Distinct().Count, plan.Budget.MaxSetups)));

    private static K<Validation<Error>, Unit> GateCarriers(SetupPlan plan) {
        HashMap<CarrierKey, Carrier> carriers = plan.Catalog.Carriers.Fold(
            HashMap<CarrierKey, Carrier>(), static (index, row) => index.Add(row.Key, row));
        Set<int> operations = toSet(plan.Operations.Map(static row => row.Key));
        Set<int> instances = toSet(plan.Catalog.Instances.Map(static row => row.Key));
        Option<PartInstance> orphan = plan.Catalog.Instances.Find(instance => !carriers.ContainsKey(instance.Carrier));
        return AdmissionSlots.Gate(
            carriers.Count == plan.Catalog.Carriers.Count && instances.Count == plan.Catalog.Instances.Count
            && plan.Catalog.Carriers.ForAll(carrier => Valid(carrier, plan.Catalog.Wcs))
            && plan.Catalog.Instances.ForAll(instance => ValidityClaim.All(
                ValidityClaim.Nonnegative(instance.Key), operations.Contains(instance.Operation), carriers.ContainsKey(instance.Carrier),
                carriers[instance.Carrier].Stations.Exists(station => station.Index == instance.Station), instance.LocalFrame.IsValid))
            && toSeq(plan.Catalog.Instances.GroupBy(static instance => instance.Operation))
                .ForAll(static group => toSeq(group).Map(static instance => instance.Carrier).Distinct().Count == 1),
            FabricationFault.Fixture(new FixturingWitness.Roster(
                orphan.Map(static instance => instance.Carrier),
                orphan.Map(static instance => instance.Station),
                plan.Catalog.Instances.Count)));
    }

    private static bool Valid(SetupOperation operation, SetupPlan plan) =>
        ValidityClaim.All(
            ValidityClaim.Nonnegative(operation.Key), Distinct(operation.Roster.Mountings),
            operation.Roster.Mountings.ForAll(static mounting => mounting.IsValid), Distinct(operation.Roster.FixtureKeys),
            operation.Roster.FixtureKeys.ForAll(plan.Catalog.Fixtures.ByOperation.ContainsKey), Distinct(operation.Roster.MachineKeys),
            operation.Roster.MachineKeys.ForAll(plan.Catalog.Machines.Contains), Distinct(operation.Roster.Features),
            operation.Roster.Features.ForAll(static value => ValidityClaim.Nonnegative(value).Holds), Distinct(operation.Roster.Resources),
            operation.Roster.Resources.ForAll(Witness.Keyed), !operation.Loads.IsEmpty, operation.Loads.ForAll(static load => load.IsValid),
            operation.Corridors.ForAll(Valid), Fixtures.Nonnegative(operation.Demand.Duration), Fixtures.Nonnegative(operation.Demand.DatumTolerance),
            Fixtures.Nonnegative(operation.Demand.DatumAngularTolerance), Fixtures.AtLeastUnit(operation.Demand.SafetyFactor),
            double.IsFinite(operation.Demand.RigidityDemand), operation.Demand.RigidityDemand >= 0.0);

    private static bool Valid(Carrier carrier, Seq<WcsSlot> roster) =>
        !carrier.Stations.IsEmpty && Witness.Keyed(carrier.Key.Value) && carrier.Mounting.IsValid
        && Fixtures.Nonnegative(carrier.ToolChange)
        && toSet(carrier.Stations.Map(static station => station.Index)).Count == carrier.Stations.Count
        && carrier.Stations.Map(static station => station.Wcs).Distinct().Count == carrier.Stations.Count
        && carrier.Stations.ForAll(station => ValidityClaim.All(
            ValidityClaim.Nonnegative(station.Index), station.Frame.IsValid, station.Wcs.Valid(roster), roster.Contains(station.Wcs)));

    private static bool Valid(ToolCorridor corridor) =>
        corridor.Stations.Count >= 2
        && corridor.Stations.ForAll(static station => Fixtures.Finite(station.Point)
            && Seq(station.Cutter, station.Holder, station.Chip, station.Coolant).ForAll(Fixtures.Nonnegative));

    private static bool Distinct<T>(Seq<T> rows) => rows.Distinct().Count == rows.Count;

    private static Error Broken(SetupChain chain, double toleranceMm) =>
        chain.Keyed(toleranceMm, Key).Match(
            Succ: static key => (Error)new FabricationFault.DatumLineageBroken(new FaultSubject.Lineage(key)),
            Fail: static fault => fault);

    private static readonly Op Key = Op.Of(name: nameof(SetupSchedule));

    // --- [SEARCH]
    internal sealed record SearchSpace(
        Arr<int> Order,
        HashMap<int, SetupOperation> Operations,
        HashMap<(int Operation, int Fixture), double> PairBounds,
        HashMap<int, double> Remainders,
        SetupPlan Plan,
        Atom<int> Budget,
        Atom<double> Cut,
        Atom<int> Deepest,
        CancellationToken Cancel) {
        public double Remaining(int cursor) => Order.Skip(cursor).Sum(key => Remainders[key]);
    }

    internal static Fin<SetupSchedule> Solve(SetupPlan plan, CancellationToken cancel) {
        BidirectionalGraph<int, SetupEdge> graph = Graph(plan);
        return !graph.IsDirectedAcyclicGraph()
            ? Fin.Fail<SetupSchedule>(Broken(Cycles(graph), Grid(plan.Operations)))
            : Space(plan, toSeq(graph.SourceFirstTopologicalSort()).ToArr(), cancel)
                .Bind(space => RootBound(space).Bind(root => Search(space, cursor: 0, SetupDraft.Empty, double.PositiveInfinity)
                    .Bind(result => result.ToFin(new FabricationFault.SetupInfeasible(
                        space.Order.At(space.Deepest.Value), plan.Budget.MaxSetups)))
                    .Bind(state => Finalize(plan, graph, state, Math.Max(root, space.Cut.Value)))));
    }

    private static BidirectionalGraph<int, SetupEdge> Graph(SetupPlan plan) {
        BidirectionalGraph<int, SetupEdge> graph = new(allowParallelEdges: true);
        graph.AddVertexRange(plan.Operations.Map(static operation => operation.Key));
        graph.AddEdgeRange(plan.Relations.Filter(static edge => edge.Relation.Orders));
        return graph;
    }

    private static Fin<SearchSpace> Space(SetupPlan plan, Arr<int> order, CancellationToken cancel) {
        HashMap<int, SetupOperation> operations = plan.Operations.Fold(
            HashMap<int, SetupOperation>(), static (index, row) => index.Add(row.Key, row));
        return order
            .ToSeq()
            .Bind(key => operations[key].Roster.FixtureKeys.Map(fixture => (Operation: key, Fixture: fixture)))
            .Traverse(pair => PairBound(operations[pair.Operation], plan.Catalog.Fixtures.ByOperation[pair.Fixture], plan.Objective)
                .Map(bound => (pair, bound))
                .ToValidation())
            .As().ToFin()
            .Map(rows => {
                HashMap<(int, int), double> bounds = rows.Fold(HashMap<(int, int), double>(),
                    static (index, row) => index.Add(row.pair, row.bound));
                return new SearchSpace(
                    order,
                    operations,
                    bounds,
                    order.ToSeq().Fold(HashMap<int, double>(), (index, key) => index.Add(key,
                        operations[key].Roster.FixtureKeys
                            .Map(fixture => bounds[(key, fixture)])
                            .Min(double.PositiveInfinity))),
                    plan,
                    Atom(plan.Budget.NodeBudget),
                    Atom(double.PositiveInfinity),
                    Atom(0),
                    cancel);
            });
    }

    private static Fin<double> PairBound(SetupOperation operation, Fixture fixture, SetupObjective objective) =>
        Holding(operation, fixture).Map(holding => objective.Bound(new CostTerms(
            Extended: false,
            Changeover: Duration.Zero,
            Reorientation: Duration.Zero,
            Travel: Length.Zero,
            DatumError: Length.Zero,
            AngularDatumError: Angle.Zero,
            RigidityMargin: holding.MinimumMargin / Math.Max(operation.Demand.RigidityDemand, EpsilonPolicy.SqrtEpsilon),
            JacobianCondition: 0.0,
            Operation: operation.Demand.Duration,
            ToolChange: Duration.Zero,
            Instances: 1)));

    private static Fin<double> RootBound(SearchSpace space) {
        Arr<int> fixtures = toSeq(space.Plan.Catalog.Fixtures.ByOperation.Keys.Order()).ToArr();
        int[,] costs = new int[space.Order.Count, fixtures.Count];
        for (int row = 0; row < space.Order.Count; row++) {
            for (int column = 0; column < fixtures.Count; column++) {
                costs[row, column] = space.PairBounds
                    .Find((space.Order[row], fixtures[column]))
                    .Map(static bound => (int)Math.Ceiling(bound * BoundScale))
                    .IfNone(Blocked);
            }
        }
        int[] allocation = new HungarianAlgorithm(costs).Compute();
        return allocation.Length != space.Order.Count
            || Range(0, allocation.Length).Exists(row => allocation[row] < 0 || costs[row, allocation[row]] == Blocked)
                ? Fin.Fail<double>(new FabricationFault.SetupInfeasible(
                    space.Order.At(0), space.Plan.Budget.MaxSetups))
                : Fin.Succ(Range(0, allocation.Length).Sum(row => costs[row, allocation[row]]) / BoundScale);
    }

    private static Fin<Option<SetupDraft>> Search(SearchSpace space, int cursor, SetupDraft state, double bound) {
        if (space.Cancel.IsCancellationRequested)
            return Fin.Fail<Option<SetupDraft>>(Errors.Cancelled);
        double remaining = space.Remaining(cursor);
        _ = space.Deepest.Swap(held => Math.Max(held, cursor));
        if (space.Budget.Swap(static held => Math.Max(0, held - 1)) == 0)
            return Fin.Fail<Option<SetupDraft>>(FabricationFault.Fixture(new FixturingWitness.Plan(
                space.Order.Count, cursor, state.Setups.Count, space.Plan.Budget.NodeBudget)));
        if (state.Cost + remaining >= bound) {
            _ = space.Cut.Swap(held => Math.Min(held, state.Cost + remaining));
            return Fin.Succ(Option<SetupDraft>.None);
        }
        return cursor == space.Order.Count
            ? Fin.Succ(Some(state))
            : Candidates(state, space.Operations[space.Order[cursor]], space.Plan).Fold(
                Fin.Succ(Option<SetupDraft>.None),
                (result, candidate) => result.Bind(best => Place(space, state, space.Operations[space.Order[cursor]], candidate)
                    .Bind(next => next.Match(
                        Some: admitted => Search(space, cursor + 1, admitted,
                            best.Match(Some: held => Math.Min(bound, held.Cost), None: () => bound)),
                        None: static () => Fin.Succ(Option<SetupDraft>.None)))
                    .Map(found => Better(best, found))));
    }

    private static Seq<(Option<int> Setup, int Machine, Fixture Fixture, Mounting Mounting, Option<Carrier> Carrier)> Candidates(
        SetupDraft state,
        SetupOperation operation,
        SetupPlan plan) {
        Set<CarrierKey> carriers = toSet(plan.Catalog.Instances
            .Filter(instance => instance.Operation == operation.Key)
            .Map(static instance => instance.Carrier));
        Seq<(Option<int>, int, Fixture, Mounting, Option<Carrier>)> existing = state.Setups
            .Filter(setup => operation.Roster.MachineKeys.Contains(setup.Machine)
                && operation.Roster.FixtureKeys.Contains(setup.Fixture.Operation)
                && (carriers.IsEmpty || setup.Carrier.Exists(carrier => carriers.Contains(carrier.Key))))
            .Map(setup => (Some(setup.Index), setup.Machine, setup.Fixture, setup.Mounting, setup.Carrier));
        Seq<(Option<int>, int, Fixture, Mounting, Option<Carrier>)> opened = operation.Roster.MachineKeys.Bind(machine =>
            operation.Roster.FixtureKeys.Bind(fixture =>
                (carriers.IsEmpty
                    ? operation.Roster.Mountings.Map(mounting =>
                        (Option<int>.None, machine, plan.Catalog.Fixtures.ByOperation[fixture], mounting, Option<Carrier>.None))
                    : Seq<(Option<int>, int, Fixture, Mounting, Option<Carrier>)>())
                + plan.Catalog.Carriers.Filter(carrier => carriers.Contains(carrier.Key))
                    .Map(carrier => (Option<int>.None, machine, plan.Catalog.Fixtures.ByOperation[fixture], carrier.Mounting, Some(carrier)))));
        return (state.Setups.Count >= plan.Budget.MaxSetups ? existing : existing + opened)
            .Filter(candidate => operation.Roster.Mountings.Exists(mounting => mounting.Frame == candidate.Mounting.Frame))
            .Filter(candidate => FitsRelations(state, operation, candidate.Fixture, candidate.Mounting, plan));
    }

    private static bool FitsRelations(SetupDraft state, SetupOperation operation, Fixture fixture, Mounting mounting, SetupPlan plan) =>
        plan.Relations.Filter(edge => edge.Source == operation.Key || edge.Target == operation.Key).ForAll(edge => {
            int other = edge.Source == operation.Key ? edge.Target : edge.Source;
            Option<Setup> placed = state.Setups.Find(setup => setup.Operations.Contains(other));
            return edge.Relation.Switch(
                state: (Placed: placed, Fixture: fixture, Mounting: mounting),
                precedes: static (_, _) => true,
                datum: static (_, _) => true,
                stock: static (_, _) => true,
                probe: static (_, _) => true,
                resource: static (_, _) => true,
                sameFixture: static (held, _) => held.Placed.ForAll(setup => setup.Fixture.Operation == held.Fixture.Operation),
                sameOrientation: static (held, _) => held.Placed.ForAll(setup => setup.Mounting.Frame == held.Mounting.Frame));
        });

    private static Fin<Option<SetupDraft>> Place(
        SearchSpace space,
        SetupDraft state,
        SetupOperation operation,
        (Option<int> Setup, int Machine, Fixture Fixture, Mounting Mounting, Option<Carrier> Carrier) candidate) =>
        Evidence(operation, candidate.Machine, candidate.Fixture, candidate.Mounting, space.Plan).Bind(evidence => evidence.Match(
            Some: accepted => Commit(space, state, operation, candidate, accepted).Map(Some),
            None: static () => Fin.Succ(Option<SetupDraft>.None)));

    // --- [EVIDENCE]
    internal static Fin<Option<SetupEvidence>> Evidence(
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
                operation.Roster.MachineKeys.Contains(machine),
                boundary.Reach,
                holding,
                clearance,
                boundary.Guarded,
                hit,
                boundary.Datum,
                boundary.Resources,
                holding.MinimumMargin / Math.Max(operation.Demand.RigidityDemand, EpsilonPolicy.SqrtEpsilon),
                boundary.Key))
            .Map(evidence => Admissible(operation, evidence, plan) ? Some(evidence) : Option<SetupEvidence>.None)
            .As()
            .ToFin();

    private static bool Admissible(SetupOperation operation, SetupEvidence evidence, SetupPlan plan) {
        Length tolerance = plan.Distortion.Match(
            Some: result => DatumTransfer
                .Of(operation.Demand.DatumTolerance, result, toSet(operation.Roster.Features))
                .Remaining,
            None: () => operation.Demand.DatumTolerance);
        return evidence.IsValid && evidence.Compatible && evidence.Reach.Reachable && evidence.Holding.Holds
            && evidence.Clearance.ForAll(static row => row.Clear) && evidence.Guarded && evidence.MachinedHit.IsNone
            && evidence.Datum.Grade.Traceable && (!operation.Demand.RequiresProbe || evidence.Datum.Grade.Measured)
            && evidence.Datum.TransferError <= tolerance && evidence.Resources.Available
            && evidence.Datum.AngularTransferError <= operation.Demand.DatumAngularTolerance
            && evidence.RigidityMargin >= 1.0 && RelationEvidence(operation, evidence, plan);
    }

    private static bool RelationEvidence(SetupOperation operation, SetupEvidence evidence, SetupPlan plan) =>
        plan.Relations.Filter(edge => edge.Target == operation.Key).ForAll(edge => edge.Relation.Switch(
            state: (Source: edge.Source, Evidence: evidence),
            precedes: static (_, _) => true,
            datum: static (held, _) => held.Evidence.Datum.Lineage.Contains(held.Source),
            stock: static (held, _) => held.Evidence.MachinedHit.IsNone,
            probe: static (held, _) => held.Evidence.Datum.Grade.Measured,
            resource: static (held, row) => held.Evidence.Resources.Held.Contains(row.Key),
            sameFixture: static (_, _) => true,
            sameOrientation: static (_, _) => true));

    private static Fin<RestraintProof> Holding(SetupOperation operation, Fixture fixture) =>
        Workholding.Apply(new WorkholdingOp.Restrain(fixture, operation.Loads, operation.Demand.SafetyFactor))
            .Bind(static result => result switch {
                WorkholdingResult.Restrained(var result) => Fin.Succ(result),
                _ => throw new InvalidOperationException("Workholding.Restrain returned a non-restraint result."),
            });

    private static Fin<Seq<WorkholdingResult.Clearance>> Clearance(SetupOperation operation, Fixture fixture) =>
        operation.Corridors.Traverse(corridor =>
            Workholding.Apply(new WorkholdingOp.Clear(fixture, FixtureState.Cut, corridor))
                .Bind(static result => result switch {
                    WorkholdingResult.Clearance result => Fin.Succ(result),
                    _ => throw new InvalidOperationException("Workholding.Clear returned a non-clearance result."),
                }).ToValidation()).As().ToFin();

    private static Fin<Option<Point3d>> Machined(Fixture fixture) =>
        fixture.Spec.Current.Match(
            Some: stock => Workholding.Apply(new WorkholdingOp.Machined(fixture, stock)).Bind(static result => result switch {
                WorkholdingResult.MachinedHit(var point) => Fin.Succ(point),
                _ => throw new InvalidOperationException("Workholding.Machined returned a non-machining result."),
            }),
            None: static () => Fin.Succ(Option<Point3d>.None));

    // --- [COMMIT]
    private static Fin<SetupDraft> Commit(
        SearchSpace space,
        SetupDraft state,
        SetupOperation operation,
        (Option<int> Setup, int Machine, Fixture Fixture, Mounting Mounting, Option<Carrier> Carrier) candidate,
        SetupEvidence evidence) {
        SetupPlan plan = space.Plan;
        bool extended = candidate.Setup.IsSome;
        int index = candidate.Setup.IfNone(state.Setups.Count);
        int position = candidate.Setup.Map(identity => state.Setups.TakeWhile(setup => setup.Index != identity).Count).IfNone(-1);
        if (extended && (position < 0 || position >= state.Setups.Count))
            return Fin.Fail<SetupDraft>(FabricationFault.Fixture(
                new FixturingWitness.Offsets(index, state.Setups.Count, plan.Budget.MaxSetups)));
        Option<WcsSlot> slot = plan.Catalog.Wcs.Find(row => !state.Setups.Exists(setup => setup.Wcs == row));
        if (!extended && slot.IsNone)
            return Fin.Fail<SetupDraft>(FabricationFault.Fixture(
                new FixturingWitness.Offsets(state.Setups.Count + 1, plan.Catalog.Wcs.Count, plan.Budget.MaxSetups)));
        Duration start = Duration.FromSeconds(state.Setups
            .Filter(setup => setup.Machine == candidate.Machine)
            .Map(static setup => setup.Finish.As(DurationUnit.Second)).Fold(0.0, Math.Max));
        Seq<PartInstance> instances = candidate.Carrier.Match(
            Some: carrier => plan.Catalog.Instances.Filter(instance =>
                instance.Operation == operation.Key && instance.Carrier == carrier.Key),
            None: static () => Seq<PartInstance>());
        Seq<InstanceWcs> offsets = candidate.Carrier.Match(
            Some: carrier => instances.Bind(instance => carrier.Stations
                .Filter(station => station.Index == instance.Station)
                .Map(station => new InstanceWcs(instance.Key, station.Wcs,
                    Transform.PlaneToPlane(Plane.WorldXY, station.Frame) * Transform.PlaneToPlane(Plane.WorldXY, instance.LocalFrame)))),
            None: static () => Seq<InstanceWcs>());
        Setup next = extended
            ? state.Setups[position] with {
                Operations = state.Setups[position].Operations.Add(operation.Key),
                Instances = state.Setups[position].Instances + instances,
                InstanceWcs = state.Setups[position].InstanceWcs + offsets,
                Finish = state.Setups[position].Finish + operation.Demand.Duration,
                Resources = state.Setups[position].Resources.Union(operation.Roster.Resources),
            }
            : new Setup(index, candidate.Machine, candidate.Fixture, candidate.Mounting,
                slot.IfNone(() => plan.Catalog.Wcs[0]), candidate.Carrier, instances, offsets, evidence.Datum,
                Arr(operation.Key), toSet(operation.Roster.Resources), start, start + operation.Demand.Duration);
        double increment = plan.Objective.Cost(Terms(operation, next, evidence, extended));
        SetupDecision decision = new(operation.Key, index, extended, increment,
            space.Remainders[operation.Key], evidence);
        return Fin.Succ(state with {
            Setups = extended ? state.Setups.SetItem(position, next) : state.Setups.Add(next),
            Decisions = state.Decisions.Add(decision),
            Placed = state.Placed.Add(operation.Key),
            Cost = state.Cost + increment,
        });
    }

    private static CostTerms Terms(SetupOperation operation, Setup setup, SetupEvidence evidence, bool extended) => new(
        extended,
        evidence.Resources.Changeover,
        evidence.Reach.Reorientation,
        evidence.Reach.Travel,
        evidence.Datum.TransferError,
        evidence.Datum.AngularTransferError,
        evidence.RigidityMargin,
        evidence.Reach.JacobianCondition,
        operation.Demand.Duration,
        setup.Carrier.Map(static carrier => carrier.ToolChange).IfNone(Duration.Zero),
        setup.Instances.Count);

    private static Option<SetupDraft> Better(Option<SetupDraft> current, Option<SetupDraft> candidate) =>
        current.Match(
            Some: best => candidate.Match(Some: next => next.Cost < best.Cost ? candidate : current, None: () => current),
            None: () => candidate);

    private static Fin<SetupSchedule> Finalize(
        SetupPlan plan,
        BidirectionalGraph<int, SetupEdge> graph,
        SetupDraft state,
        double lowerBound) {
        Set<(int Source, int Target)> kept = toSet(toSeq(graph.ComputeTransitiveReduction().Edges)
            .Map(static edge => (edge.Source, edge.Target)));
        Seq<SetupEdge> reduced = plan.Relations
            .Filter(edge => edge.Relation.Orders && kept.Contains((edge.Source, edge.Target)));
        Option<double> proof = Some(Math.Min(state.Cost, double.IsPositiveInfinity(lowerBound) ? state.Cost : lowerBound));
        return Keyed(state.Setups, state.Decisions, reduced, state.Cost, proof, Grid(plan.Operations))
            .Map(key => new SetupSchedule(
                plan,
                state.Setups,
                state.Setups.Map(static setup => new WcsAssignment(setup.Index, setup.Wcs)).ToSeq(),
                reduced,
                state.Decisions,
                state.Cost,
                proof,
                key));
    }

    internal static Fin<SetupSchedule> Rebase(SetupSchedule schedule, int setup, Plane measured) {
        int position = schedule.Setups.TakeWhile(row => row.Index != setup).Count;
        if (position >= schedule.Setups.Count || !measured.IsValid)
            return Fin.Fail<SetupSchedule>(FabricationFault.Fixture(
                new FixturingWitness.Rebase(setup, Length.Zero, Angle.Zero, Length.Zero)));
        Setup held = schedule.Setups[position];
        Transform correction = Transform.PlaneToPlane(held.Mounting.Frame, measured);
        Length offset = Length.FromMillimeters(held.Mounting.Frame.Origin.DistanceTo(measured.Origin));
        Angle rotation = Rotation(correction);
        Seq<SetupOperation> operations = schedule.Plan.Operations.Filter(row => held.Operations.Contains(row.Key));
        Option<Length> tolerance = Least(operations.Map(static row => row.Demand.DatumTolerance));
        Option<Angle> angular = Least(operations.Map(static row => row.Demand.DatumAngularTolerance));
        if (tolerance.Exists(bound => offset > bound) || angular.Exists(bound => rotation > bound))
            return Fin.Fail<SetupSchedule>(FabricationFault.Fixture(
                new FixturingWitness.Rebase(setup, offset, rotation, tolerance.IfNone(Length.Zero))));
        DatumLineage datum = held.Datum with {
            ProbeCorrection = offset,
            AngularProbeCorrection = rotation,
            Grade = held.Datum.Grade.AfterProbe,
        };
        Mounting reframed = held.Mounting.Reframed(measured);
        return operations
            .Traverse(row => Evidence(row, held.Machine, held.Fixture, reframed, schedule.Plan)
                .Bind(evidence => evidence.ToFin(new FabricationFault.SetupInfeasible(Some(row.Key), schedule.Setups.Count)))
                .ToValidation())
            .As().ToFin().Bind(proven => {
                HashMap<int, SetupEvidence> reproven = proven.Fold(HashMap<int, SetupEvidence>(),
                    static (index, evidence) => index.Add(evidence.Operation, evidence));
                HashMap<int, SetupOperation> byKey = operations.Fold(HashMap<int, SetupOperation>(),
                    static (index, operation) => index.Add(operation.Key, operation));
                Setup next = held with {
                    Mounting = reframed,
                    InstanceWcs = held.InstanceWcs.Map(row => row with { Frame = correction * row.Frame }),
                    Datum = datum,
                };
                Seq<SetupDecision> decisions = schedule.Decisions.Map(decision => decision.Setup != setup
                    ? decision
                    : Reproven(decision, reproven[decision.Operation] with { Datum = datum },
                        byKey[decision.Operation], next, schedule.Plan.Objective));
                SetupSchedule draft = schedule with {
                    Setups = schedule.Setups.SetItem(position, next),
                    Decisions = decisions,
                    Cost = decisions.Sum(static decision => decision.IncrementalCost),
                    ProvenLowerBound = None,
                };
                return Keyed(draft.Setups, draft.Decisions, draft.Precedence, draft.Cost, draft.ProvenLowerBound,
                        Grid(schedule.Plan.Operations))
                    .Map(key => draft with { Key = key });
            });
    }

    private static SetupDecision Reproven(
        SetupDecision decision,
        SetupEvidence evidence,
        SetupOperation operation,
        Setup setup,
        SetupObjective objective) =>
        decision with {
            Evidence = evidence,
            IncrementalCost = objective.Cost(Terms(operation, setup, evidence, decision.Extended)),
        };

    // --- [GRAPH_EVIDENCE]
    internal static Seq<Arr<TVertex>> Components<TVertex, TEdge>(IVertexListGraph<TVertex, TEdge> graph)
        where TEdge : IEdge<TVertex> {
        Dictionary<TVertex, int> labels = new();
        _ = graph.StronglyConnectedComponents(labels);
        return toSeq(labels.GroupBy(static row => row.Value))
            .Filter(static group => group.Count() > 1)
            .Map(static group => toSeq(group).Map(static row => row.Key).ToArr());
    }

    private static SetupChain Cycles(BidirectionalGraph<int, SetupEdge> graph) {
        Seq<Arr<int>> components = Components<int, SetupEdge>(graph);
        Set<int> cyclic = toSet(components.Bind(static component => component.ToSeq()));
        return new SetupChain(
            cyclic.ToSeq(),
            components,
            toSeq(graph.Edges)
                .Filter(edge => cyclic.Contains(edge.Source) && cyclic.Contains(edge.Target))
                .Map(static edge => (edge.Source, edge.Target)));
    }

    // --- [CANONICAL]
    private static Fin<ContentKey> Keyed(
        Arr<Setup> setups,
        Seq<SetupDecision> decisions,
        Seq<SetupEdge> precedence,
        double cost,
        Option<double> provenLowerBound,
        double toleranceMm) =>
        FabricationCanon.Keyed(EgressKind.Plan, toleranceMm, writer => writer
            .Rows(setups.ToSeq(), static (held, setup) => setup.Datum.CanonicalBytes(setup.Wcs
                .CanonicalBytes(setup.Mounting.CanonicalBytes(held
                    .Ordinal(setup.Index).Ordinal(setup.Machine).Ordinal(setup.Fixture.Operation)))
                .Pose(setup.Mounting.Frame)
                .Rows(setup.Operations.ToSeq(), static (row, operation) => row.Ordinal(operation))
                .Rows(setup.Instances, static (row, instance) => row
                    .Ordinal(instance.Key).Ordinal(instance.Operation)
                    .String(instance.Carrier.Value).Ordinal(instance.Station)
                    .Pose(instance.LocalFrame))
                .Rows(setup.InstanceWcs, static (row, instance) => instance.Slot.CanonicalBytes(row.Ordinal(instance.Instance)))
                .Double(setup.Start.As(DurationUnit.Second)).Double(setup.Finish.As(DurationUnit.Second))))
            .Rows(decisions, static (held, decision) => decision.Evidence.Key.CanonicalBytes(held
                .Ordinal(decision.Operation).Ordinal(decision.Setup).Bool(decision.Extended)
                .Double(decision.IncrementalCost).Double(decision.Bound)))
            .Rows(precedence, static (held, edge) => edge.Relation.CanonicalBytes(
                held.Ordinal(edge.Source).Ordinal(edge.Target)))
            .Double(cost)
            .Maybe(provenLowerBound, static (held, bound) => held.Double(bound)),
            Key);

    private static CanonicalWriter Pose(this CanonicalWriter writer, Plane frame) =>
        writer.Coords(frame.Origin).Coords(frame.XAxis).Coords(frame.YAxis);

    private static double Grid(Seq<SetupOperation> operations) =>
        Least(operations.Map(static row => row.Demand.DatumTolerance)).IfNone(Length.Zero).As(LengthUnit.Millimeter);

    private static Option<T> Least<T>(Seq<T> rows) where T : IComparable<T> =>
        rows.Fold(Option<T>.None, static (least, row) => least.Filter(held => held.CompareTo(row) <= 0).IfNone(row));

    private static Angle Rotation(Transform correction) => new(Math.Acos(Math.Clamp(
        0.5 * ((correction.M00 + correction.M11 + correction.M22) - 1.0), -1.0, 1.0)), AngleUnit.Radian);
}
```

## [04]-[PROJECTION]

- Owner: `SetupProjection` selects machine, probing, posting, traveler, inspection, and evidence views; `SetupArtifact` carries the selected view without reopening the schedule.
- Output: projection preserves the keyed schedule result, WCS, precedence, datum lineage, evidence, cost, and proven bound; raw search policy and evidence-provider capabilities remain ingress-only.
- Boundary: posting receives WCS identity and values, probing receives datum and correction targets, and documentation receives immutable schedule evidence; no consumer derives setup order from array position alone.

```csharp
// --- [PROJECTION] ----------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SetupProjection {
    public static readonly SetupProjection Machine = new("machine");
    public static readonly SetupProjection Probing = new("probing");
    public static readonly SetupProjection Posting = new("posting");
    public static readonly SetupProjection Traveler = new("traveler");
    public static readonly SetupProjection Inspection = new("inspection");
    public static readonly SetupProjection Evidence = new("evidence");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SetupArtifact {
    private SetupArtifact() { }

    public sealed record Machine(ContentKey Key, Arr<Setup> Setups, Seq<WcsAssignment> Wcs) : SetupArtifact;
    public sealed record Probing(ContentKey Key, Seq<DatumLineage> Datums) : SetupArtifact;
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

internal static partial class Setups {
    internal static Fin<SetupArtifact> Project(SetupSchedule schedule, SetupProjection projection) =>
        Fin.Succ(projection.Switch<SetupArtifact>(
            machine: () => new SetupArtifact.Machine(schedule.Key, schedule.Setups, schedule.Wcs),
            probing: () => new SetupArtifact.Probing(schedule.Key, schedule.Setups.Map(static setup => setup.Datum).ToSeq()),
            posting: () => new SetupArtifact.Posting(schedule.Key, schedule.Wcs, schedule.Precedence),
            traveler: () => new SetupArtifact.Traveler(schedule.Key, schedule.Setups, schedule.Decisions),
            inspection: () => new SetupArtifact.Inspection(schedule.Key,
                schedule.Decisions.Map(static decision => decision.Evidence)),
            evidence: () => new SetupArtifact.Evidence(schedule.Key, schedule.Setups, schedule.Wcs,
                schedule.Precedence, schedule.Decisions, schedule.Cost, schedule.ProvenLowerBound)));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
