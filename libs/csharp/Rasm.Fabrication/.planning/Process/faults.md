# [RASM_FABRICATION_FAULTS]

`FabricationFault` is the sole fabrication failure rail. Its closed `[Union]` derives `Rasm.Domain.Expected`, preserves `FaultBand.Fabrication + Offset` identity, implements `IValidationError<FabricationFault>` so every generated owner in the package mints its refusals on this band, and carries S0 evidence without importing a generator, posting, fixturing, specification, or additive owner.

Each witness family pairs a case-shaped payload with a `[SmartEnum<string>]` predicate vocabulary and admits through `Witness.Admit<TSelf, TKind>` over the `IWitness<TSelf, TKind>` contract. Every fault carrying a witness family mints through `Witness.Carried`, so the predicate runs on the raising path and a payload contradicting its own kind lands as `WitnessMalformed` rather than as a fault whose evidence refutes it. One `Refused` read owns that verdict, so admission and the gated mint never build two refusals for one payload.

`FaultSubject` preserves higher-plane identity as S0 keys and content identities, while `RelationFault` retains native Process axes. Consumers inspect concrete cases through `Error.IsType<T>()`; no identity conversion or message parser exists. A fault payload names its owning plane's own discriminant by TYPE where recovery demands it — `ModalGroup` for a parse position, `FixturingWitness` for a fixture refusal — and the page reaches no plane BEHAVIOR, so the atoms floor stays acyclic while the offset allocation stays whole on one page.

`FabConcern` is the plane census: every case declares the plane that owns its code, its `Folder` namespace, and the stratum that plane occupies, so a receipt partitions faults by owning plane without a second table and a split package states each of its planes truthfully. `OpenLoop`, `PolicyInadmissible`, and `RunAbandoned` thread their caller's plane into the same slot. Degenerate geometry remains `GeometryFault.DegenerateInput` — a geometric primitive that is itself degenerate, named by its real `Kind` with the real element ordinal where one exists and `None` where the whole input is the subject; a POLICY, request, or parameter tuple failing its own admission gate is a contract failure and takes `PolicyInadmissible`, never a kernel-band borrow under a fabricated `Kind` and a sentinel index. Offsets `1` through `66` remain frozen and `67` is the next allocation.

Wire posture: HOST-LOCAL. `FabricationFault` rides `Fin<T>`, while frozen integer codes alone cross persistence receipts.

## [01]-[INDEX]

- [02]-[CONCERN_CENSUS]: `FabConcern` and the seven `IWitnessKind` predicate vocabularies — `EquipmentFault`, `DeriveFault`, `SubjectKind`, `JointFault`, `NestFault`, `SourceKind`, `RelationKind`, `KerfKind` — beside `RangeSide`, `NestDefect`, and `CollisionContact`.
- [03]-[WITNESS_EVIDENCE]: `IWitnessKind`, `IWitness`, `Witness`, and the payload families `FaultSubject`, `EquipmentWitness`, `DeriveWitness`, `JointDiagnostic`, `NestWitness`, `SourceLocus`, `RelationFault`, `KerfWitness`, `CollisionZone`, `VoxelBudget`.
- [04]-[FAULT_BAND]: `FabricationFault`, its frozen offset ledger, and the seven witness-carrying mints.
- [05]-[ADMISSION]: `Admission`, the one generated-owner bridge onto the `Fin` rail beside the `Rasm.Element` `AdmissionSlots` band-blind `Gate` lift and `Accumulate` fold every accumulating policy admission composes.

## [02]-[CONCERN_CENSUS]

- Owner: `FabConcern` owns the plane census every fault case declares, and each `IWitnessKind` vocabulary owns one family's case predicates — the row that names a condition also decides whether a payload can describe it.
- Cases: `FabConcern` carries one row per PLANE, not per folder — `Folder` names the namespace a receipt partitions by, `Stratum` names the position that plane holds, so `Process` states its atoms floor, terminal derivation, and telemetry fan separately, and `Kinematics` states motion apart from its consuming fleet. `RangeSide` carries the comparison a bound violation runs, so one `Range` witness replaces a floor case and a ceiling case that differ only in direction. `NestDefect` is the one failed-check vocabulary the inventory and lineage witnesses both carry, replacing parallel boolean columns a predicate had to sniff.
- Auto: a kind row is `Of<TCase>(key, predicate)` — `Witness.Case` closes the type test, so the row's own predicate reads its case shape directly and a foreign payload fails the test rather than the predicate.
- Boundary: a predicate asserts the CONDITION the row names, never the presence of an admitted operand — a generated union hands non-null cases, so a null clause is refuted ceremony. Correspondence-bearing rows re-run the correspondence they refuse, so a pair that actually corresponds cannot mint as an inadmissible pair.
- Growth: a new failure condition is one row and one payload case; a new plane is one `FabConcern` row carrying its folder and stratum.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using Rasm.Element.Projection;
using Rhino.Geometry;
using Thinktecture;
using Expected = Rasm.Domain.Expected;

namespace Rasm.Fabrication.Process;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// A row is a PLANE, so a split package states each of its planes; `Folder` is what a receipt partitions by and
// `Stratum` is what the strata law reads. Two rows sharing a folder are the split, never a duplicate.
[SmartEnum<string>]
public sealed partial class FabConcern {
    public static readonly FabConcern Process = new("process", folder: "process", stratum: 0);
    public static readonly FabConcern Geometry2D = new("geometry-2d", folder: "geometry-2d", stratum: 1);
    public static readonly FabConcern Ingress = new("ingress", folder: "ingress", stratum: 1);
    public static readonly FabConcern Kinematics = new("kinematics", folder: "kinematics", stratum: 1);
    public static readonly FabConcern Tooling = new("tooling", folder: "tooling", stratum: 2);
    public static readonly FabConcern Nesting = new("nesting", folder: "nesting", stratum: 2);
    public static readonly FabConcern Additive = new("additive", folder: "additive", stratum: 2);
    public static readonly FabConcern Fixturing = new("fixturing", folder: "fixturing", stratum: 3);
    public static readonly FabConcern Forming = new("forming", folder: "forming", stratum: 3);
    public static readonly FabConcern Joining = new("joining", folder: "joining", stratum: 3);
    public static readonly FabConcern Spec = new("spec", folder: "spec", stratum: 3);
    public static readonly FabConcern Fleet = new("fleet", folder: "kinematics", stratum: 3);
    public static readonly FabConcern Toolpath = new("toolpath", folder: "toolpath", stratum: 4);
    public static readonly FabConcern Derivation = new("derivation", folder: "process", stratum: 4);
    public static readonly FabConcern Posting = new("posting", folder: "posting", stratum: 5);
    public static readonly FabConcern Verify = new("verify", folder: "verify", stratum: 5);
    public static readonly FabConcern Documentation = new("documentation", folder: "documentation", stratum: 5);
    public static readonly FabConcern Telemetry = new("telemetry", folder: "process", stratum: 5);

    public string Folder { get; }
    public int Stratum { get; }
}

// One bound violation, one witness: the side owns the comparison, so a floor and a ceiling stop being two cases
// carrying the same three operands under two names for the same limit.
[SmartEnum<string>]
public sealed partial class RangeSide {
    public static readonly RangeSide Floor = new("floor", static (derived, limit) => limit > derived);
    public static readonly RangeSide Ceiling = new("ceiling", static (derived, limit) => derived > limit);

    public Func<double, double, bool> Exceeded { get; }
}

[SmartEnum<string>]
public sealed partial class EquipmentFault : IWitnessKind<EquipmentWitness> {
    public static readonly EquipmentFault Geometry = Of<EquipmentWitness.Geometry>(
        "geometry", static row => Witness.Keyed(row.Axis));
    public static readonly EquipmentFault Spent = Of<EquipmentWitness.Spent>(
        "spent", static row => row.Identity != UInt128.Zero);
    public static readonly EquipmentFault HeadPhysics = Of<EquipmentWitness.HeadPhysics>(
        "head-physics", static row => row.Mounted.Match(head => !head.Admits(row.Required), static () => true));
    public static readonly EquipmentFault Range = Of<EquipmentWitness.Range>(
        "range", static row => Witness.Finite(row.Derived, row.Limit) && row.Side.Exceeded(row.Derived, row.Limit));
    public static readonly EquipmentFault Quantity = Of<EquipmentWitness.Quantity>(
        "quantity", static row => Witness.Keyed(row.Kind));
    public static readonly EquipmentFault Grade = Of<EquipmentWitness.Grade>(
        "grade", static row => Witness.Keyed(row.Locus));

    public Func<EquipmentWitness, bool> Admits { get; }

    private static EquipmentFault Of<TWitness>(string key, Func<TWitness, bool> admits)
        where TWitness : EquipmentWitness => new(key, Witness.Case<EquipmentWitness, TWitness>(admits));
}

[SmartEnum<string>]
public sealed partial class DeriveFault : IWitnessKind<DeriveWitness> {
    public static readonly DeriveFault ComponentMismatch = Of<DeriveWitness.ComponentMismatch>(
        "component-mismatch", static row => row.Requested != row.Assessed);
    public static readonly DeriveFault OperationCycle = Of<DeriveWitness.OperationCycle>(
        "operation-cycle", static row => !row.Cycle.IsEmpty);
    public static readonly DeriveFault DuplicateOperation = Of<DeriveWitness.DuplicateOperation>(
        "duplicate-operation", static row => Witness.Index(row.Id));
    public static readonly DeriveFault UnknownPredecessor = Of<DeriveWitness.UnknownPredecessor>(
        "unknown-predecessor", static row => Witness.Pair(row.Operation, row.Predecessor));
    public static readonly DeriveFault LotInadmissible = Of<DeriveWitness.LotInadmissible>(
        "lot-inadmissible", static row => row.Quantity < 1 || row.BatchSize < 1 || row.BatchSize > row.Quantity
            || row.Due < row.Release || row.TransferBuffer < Duration.Zero
            || row.Predecessors.Distinct().Count != row.Predecessors.Count || row.Predecessors.Contains(UInt128.Zero));
    public static readonly DeriveFault LotOverdue = Of<DeriveWitness.LotOverdue>(
        "lot-overdue", static row => row.Completion > row.Due);
    public static readonly DeriveFault LotUnschedulable = Of<DeriveWitness.LotUnschedulable>(
        "lot-unschedulable", static row => Witness.Index(row.Operation) && row.Effort > Duration.Zero);
    public static readonly DeriveFault CapacityExhausted = Of<DeriveWitness.CapacityExhausted>(
        "capacity-exhausted", static row => Witness.Index(row.Operation) && row.Instances > 0 && row.Effort > Duration.Zero);
    public static readonly DeriveFault PredecessorLotMissing = Of<DeriveWitness.PredecessorLotMissing>(
        "predecessor-lot-missing", static row => row.Lot != UInt128.Zero);
    public static readonly DeriveFault SetupCoverage = Of<DeriveWitness.SetupCoverage>(
        "setup-coverage", static row => Witness.Index(row.Assigned) && row.Required > 0 && row.Assigned != row.Required);
    public static readonly DeriveFault AssemblyMismatch = Of<DeriveWitness.AssemblyMismatch>(
        "assembly-mismatch", static row => row.Component != UInt128.Zero);
    public static readonly DeriveFault AssemblyRequired = Of<DeriveWitness.AssemblyRequired>(
        "assembly-required", static row => row.Connections > 0);
    public static readonly DeriveFault JoinReceiptMissing = Of<DeriveWitness.JoinReceiptMissing>(
        "join-receipt-missing", static row => Witness.Index(row.Joint));
    public static readonly DeriveFault IdentifierExhausted = Of<DeriveWitness.IdentifierExhausted>(
        "identifier-exhausted", static row => row.Requested > 0 && row.Next > (long)int.MaxValue - row.Requested);
    public static readonly DeriveFault OperationAbsent = Of<DeriveWitness.OperationAbsent>(
        "operation-absent", static row => Witness.Index(row.Id));
    public static readonly DeriveFault OperationsEmpty = Of<DeriveWitness.OperationsEmpty>(
        "operations-empty", static row => Witness.Index(row.Joints));
    public static readonly DeriveFault DemandInadmissible = Of<DeriveWitness.DemandInadmissible>(
        "demand-inadmissible", static row => Witness.Index(row.Id));

    public Func<DeriveWitness, bool> Admits { get; }

    private static DeriveFault Of<TWitness>(string key, Func<TWitness, bool> admits)
        where TWitness : DeriveWitness => new(key, Witness.Case<DeriveWitness, TWitness>(admits));
}

// Subject kind is fixed by each fault's own slot TYPE (`FaultSubject.Strategy Strategy`, `FaultSubject.Specification
// Frame`), so these rows carry the key floor alone and no fault mints through them. `FaultSubject.Admit` is the
// boundary gate a page admitting runtime subject text — a media type, a stage name off a provider — calls before it
// seats the subject on a fault; a subject built from a generated owner's own `Key` is already keyed by construction.
[SmartEnum<string>]
public sealed partial class SubjectKind : IWitnessKind<FaultSubject> {
    public static readonly SubjectKind Strategy = Of<FaultSubject.Strategy>("strategy", static row => Witness.Keyed(row.Key));
    public static readonly SubjectKind VoxelOperation = Of<FaultSubject.VoxelOperation>("voxel-operation", static row => Witness.Keyed(row.Key));
    public static readonly SubjectKind ProgramNode = Of<FaultSubject.ProgramNode>("program-node", static row => Witness.Keyed(row.Key));
    public static readonly SubjectKind Specification = Of<FaultSubject.Specification>("specification", static row => row.Key.Digest != UInt128.Zero);
    public static readonly SubjectKind Partition = Of<FaultSubject.Partition>("partition", static row => Witness.Keyed(row.Key));
    public static readonly SubjectKind Extension = Of<FaultSubject.Extension>(
        "extension", static row => Witness.Keyed(row.MediaType) && Witness.Keyed(row.Key));
    public static readonly SubjectKind Lineage = Of<FaultSubject.Lineage>("lineage", static row => row.Key.Digest != UInt128.Zero);
    public static readonly SubjectKind Stage = Of<FaultSubject.Stage>("stage", static row => Witness.Keyed(row.Key));
    public static readonly SubjectKind Bevel = Of<FaultSubject.Bevel>("bevel", static row => Witness.Keyed(row.Key));
    public static readonly SubjectKind Qualification = Of<FaultSubject.Qualification>("qualification", static row => Witness.Keyed(row.Key));

    public Func<FaultSubject, bool> Admits { get; }

    private static SubjectKind Of<TSubject>(string key, Func<TSubject, bool> admits)
        where TSubject : FaultSubject => new(key, Witness.Case<FaultSubject, TSubject>(admits));
}

[SmartEnum<string>]
public sealed partial class JointFault : IWitnessKind<JointDiagnostic> {
    public static readonly JointFault JointLimit = Of<JointDiagnostic.JointLimit>(
        "joint-limit", static row => Witness.Index(row.Joint) && Witness.Finite(row.Position, row.Lower, row.Upper)
            && row.Lower < row.Upper && (row.Position < row.Lower || row.Position > row.Upper));
    public static readonly JointFault Singularity = Of<JointDiagnostic.Singularity>(
        "singularity", static row => Witness.Index(row.Joint) && Witness.Finite(row.ConditionNumber, row.Limit)
            && row.Limit > 0.0 && row.ConditionNumber > row.Limit);
    public static readonly JointFault Reach = Of<JointDiagnostic.Reach>(
        "reach", static row => Witness.Index(row.Target) && Witness.Finite(row.Distance, row.Limit)
            && row.Limit >= 0.0 && row.Distance > row.Limit);
    public static readonly JointFault Collision = Of<JointDiagnostic.Collision>("collision", Touching);
    public static readonly JointFault SelfCollision = Of<JointDiagnostic.SelfCollision>("self-collision", Touching);
    public static readonly JointFault Velocity = Of<JointDiagnostic.Velocity>("velocity", Exceeded);
    public static readonly JointFault Acceleration = Of<JointDiagnostic.Acceleration>("acceleration", Exceeded);
    public static readonly JointFault Jerk = Of<JointDiagnostic.Jerk>("jerk", Exceeded);
    public static readonly JointFault Torque = Of<JointDiagnostic.Torque>("torque", Exceeded);
    public static readonly JointFault Configuration = Of<JointDiagnostic.Configuration>(
        "configuration", static row => Witness.Keyed(row.Requested) && Witness.Keyed(row.Admitted)
            && !string.Equals(row.Requested, row.Admitted, StringComparison.Ordinal));
    public static readonly JointFault Disconnected = Of<JointDiagnostic.Disconnected>(
        "disconnected", static row => Witness.Index(row.Link) && row.ExpectedParent >= -1
            && row.ActualParent >= -1 && row.ExpectedParent != row.ActualParent);

    public Func<JointDiagnostic, bool> Admits { get; }

    private static JointFault Of<TDiagnostic>(string key, Func<TDiagnostic, bool> admits)
        where TDiagnostic : JointDiagnostic => new(key, Witness.Case<JointDiagnostic, TDiagnostic>(admits));

    // The rate family shares one predicate over its shared shape, and the two contact families share another:
    // an arm differing only in which case carries the operands is a row, never a re-spelled body.
    private static bool Exceeded(JointDiagnostic.Rate row) =>
        Witness.Index(row.Joint) && Witness.Finite(row.Required, row.Limit)
        && row.Limit >= 0.0 && Math.Abs(row.Required) > row.Limit;

    private static bool Touching(JointDiagnostic.Contact row) =>
        Witness.Pair(row.FirstLink, row.SecondLink) && double.IsFinite(row.Clearance) && row.Clearance <= 0.0;
}

[SmartEnum<string>]
public sealed partial class CollisionContact {
    public static readonly CollisionContact Cutter = new("cutter");
    public static readonly CollisionContact Shank = new("shank");
    public static readonly CollisionContact Holder = new("holder");
    public static readonly CollisionContact Spindle = new("spindle");
    public static readonly CollisionContact Fixture = new("fixture");
    public static readonly CollisionContact Clamp = new("clamp");
    public static readonly CollisionContact Stock = new("stock");
    public static readonly CollisionContact Part = new("part");
    public static readonly CollisionContact Table = new("table");
    public static readonly CollisionContact Envelope = new("envelope");
}

// One failed-check vocabulary for both inventory and lineage refusals: the witness reports WHICH checks failed as
// data, so a predicate reads one non-empty set instead of sniffing a boolean column per check.
[SmartEnum<string>]
public sealed partial class NestDefect {
    public static readonly NestDefect Kind = new("kind");
    public static readonly NestDefect Revision = new("revision");
    public static readonly NestDefect ClaimCap = new("claim-cap");
    public static readonly NestDefect Identity = new("identity");
    public static readonly NestDefect MaterialMatch = new("material-match");
    public static readonly NestDefect Lifecycle = new("lifecycle");
    public static readonly NestDefect Lease = new("lease");
    public static readonly NestDefect Profile = new("profile");
    public static readonly NestDefect Usable = new("usable");
    public static readonly NestDefect Indexed = new("indexed");
    public static readonly NestDefect Ordered = new("ordered");
    public static readonly NestDefect Parents = new("parents");
    public static readonly NestDefect Acyclic = new("acyclic");
    public static readonly NestDefect Successive = new("successive");
    public static readonly NestDefect Rooted = new("rooted");
}

[SmartEnum<string>]
public sealed partial class NestFault : IWitnessKind<NestWitness> {
    public static readonly NestFault EmptyCutList = Of<NestWitness.EmptyCutList>("empty-cut-list", static _ => true);
    public static readonly NestFault PartExceedsStock = Of<NestWitness.PartExceedsStock>(
        "part-exceeds-stock", static row => Witness.Index(row.Part) && Witness.Positive(row.PartAreaMm2)
            && Witness.Positive(row.StockAreaMm2) && row.PartAreaMm2 > row.StockAreaMm2);
    public static readonly NestFault HeterogeneousMassCut = Of<NestWitness.HeterogeneousMassCut>(
        "heterogeneous-mass-cut", static row => Witness.Pair(row.FirstPart, row.SecondPart)
            && Witness.Positive(row.FirstMass) && Witness.Positive(row.SecondMass) && row.FirstMass != row.SecondMass);
    public static readonly NestFault MaterialMismatch = Of<NestWitness.MaterialMismatch>(
        "material-mismatch", static row => Witness.Index(row.Part) && row.Required != row.Stock);
    public static readonly NestFault ThicknessMismatch = Of<NestWitness.ThicknessMismatch>(
        "thickness-mismatch", static row => Witness.Index(row.Part) && Witness.Positive(row.RequiredMm)
            && Witness.Positive(row.StockMm) && row.RequiredMm != row.StockMm);
    public static readonly NestFault GrainIncompatible = Of<NestWitness.GrainIncompatible>(
        "grain-incompatible", static row => Witness.Index(row.Part) && Angle(row.RequiredDeg)
            && Angle(row.StockDeg) && row.RequiredDeg != row.StockDeg);
    public static readonly NestFault InvalidQuantity = Of<NestWitness.InvalidQuantity>(
        "invalid-quantity", static row => Witness.Index(row.Part) && row.Quantity <= 0);
    public static readonly NestFault RemnantUnavailable = Of<NestWitness.RemnantUnavailable>(
        "remnant-unavailable", static row => row.Remnant.Digest != UInt128.Zero);
    public static readonly NestFault Admission = Of<NestWitness.Admission>(
        "admission", static row => Witness.Keyed(row.Stage.Key) && row.Subject.ForAll(Witness.Index)
            && Witness.Keyed(row.Detail));
    public static readonly NestFault StrategyBudget = Of<NestWitness.StrategyBudget>(
        "strategy-budget", static row => Witness.Keyed(row.Strategy.Key)
            && row.Visited >= 0 && row.Pending >= 0 && row.Depth >= 0
            && (row.CountBudget < 1 || row.DepthBudget < 0 || row.Visited > row.CountBudget
                || row.Pending > row.CountBudget || row.Depth > row.DepthBudget));
    public static readonly NestFault ProviderProof = Of<NestWitness.ProviderProof>(
        "provider-proof", static row => Witness.Keyed(row.Strategy.Key)
            && row.Placements >= 0 && (!row.Contained || !row.Disjoint));
    public static readonly NestFault InventoryRow = Of<NestWitness.InventoryRow>(
        "inventory-row", static row => row.ClaimCap >= 0 && !row.Defects.IsEmpty);
    public static readonly NestFault Lineage = Of<NestWitness.Lineage>(
        "lineage", static row => row.Rows >= 0 && row.Indexed >= 0 && row.Ordered >= 0 && !row.Defects.IsEmpty);

    public Func<NestWitness, bool> Admits { get; }

    private static NestFault Of<TWitness>(string key, Func<TWitness, bool> admits)
        where TWitness : NestWitness => new(key, Witness.Case<NestWitness, TWitness>(admits));

    private static bool Angle(double value) => double.IsFinite(value) && value is >= -360.0 and <= 360.0;
}

[SmartEnum<string>]
public sealed partial class SourceKind : IWitnessKind<SourceLocus> {
    public static readonly SourceKind Profile = Of<SourceLocus.DxfEntity>(
        "profile", static row => Witness.Keyed(row.Handle));
    public static readonly SourceKind Solid = Of<SourceLocus.OcctShape>("solid", static row => Witness.Index(row.Id));
    public static readonly SourceKind Steel = Of<SourceLocus.DstvBlock>(
        "steel", static row => Witness.Keyed(row.Block) && row.Line > 0);
    public static readonly SourceKind Element = Of<SourceLocus.ElementNode>(
        "element", static row => row.NodeKey != UInt128.Zero);
    public static readonly SourceKind ThreeMf = Of<SourceLocus.ThreeMfObject>(
        "three-mf", static row => Witness.Keyed(row.Model) && Witness.Index(row.ObjectId));
    public static readonly SourceKind Rhino = Of<SourceLocus.RhinoObject>(
        "rhino", static row => row.ObjectId != Guid.Empty);
    public static readonly SourceKind Mesh = Of<SourceLocus.MeshFace>("mesh", static row => Witness.Index(row.Face));
    public static readonly SourceKind Program = Of<SourceLocus.ProgramBlock>("program", static row => Witness.Index(row.Block));
    public static readonly SourceKind Exchange = Of<SourceLocus.ExchangeEntity>(
        "exchange", static row => Witness.Keyed(row.Scheme) && Witness.Keyed(row.Entity));

    public Func<SourceLocus, bool> Admits { get; }

    private static SourceKind Of<TLocus>(string key, Func<TLocus, bool> admits)
        where TLocus : SourceLocus => new(key, Witness.Case<SourceLocus, TLocus>(admits));
}

// A relation refusal re-runs the correspondence it names: a pair that DOES correspond cannot describe an
// inadmissible pairing, so it lands as `WitnessMalformed` instead of a fault its own evidence refutes.
[SmartEnum<string>]
public sealed partial class RelationKind : IWitnessKind<RelationFault> {
    public static readonly RelationKind ModalityStrategy = Of<RelationFault.ModalityStrategy>(
        "modality-strategy", static row => !row.Modality.Admits(row.Strategy));
    public static readonly RelationKind ProcessMachine = Of<RelationFault.ProcessMachine>(
        "process-machine", static row => !row.Machine.Admits(row.Process));
    public static readonly RelationKind DialectModality = Of<RelationFault.DialectModality>(
        "dialect-modality", static row => !row.Dialect.Admits(row.Modality));
    public static readonly RelationKind OperationEquipment = Of<RelationFault.OperationEquipment>(
        "operation-equipment", static row => !row.Equipment.Admits(row.Operation));
    public static readonly RelationKind ProcessMaterial = Of<RelationFault.ProcessMaterial>(
        "process-material", static row => !row.Material.Admits(row.Process));

    public Func<RelationFault, bool> Admits { get; }

    private static RelationKind Of<TPair>(string key, Func<TPair, bool> admits)
        where TPair : RelationFault => new(key, Witness.Case<RelationFault, TPair>(admits));
}

[SmartEnum<string>]
public sealed partial class KerfKind : IWitnessKind<KerfWitness> {
    public static readonly KerfKind Vanished = Of<KerfWitness.Vanished>(
        "vanished", static row => Witness.Index(row.Region));
    public static readonly KerfKind Overlapped = Of<KerfWitness.Overlapped>(
        "overlapped", static row => Witness.Pair(row.First, row.Second));

    public Func<KerfWitness, bool> Admits { get; }

    private static KerfKind Of<TWitness>(string key, Func<TWitness, bool> admits)
        where TWitness : KerfWitness => new(key, Witness.Case<KerfWitness, TWitness>(admits));
}
```

## [03]-[WITNESS_EVIDENCE]

- Owner: `Witness` owns case-predicate lifting, the ONE refusal read, the admission fold, and the gated carrier; each witness union owns its plane evidence — `JointDiagnostic` robot and machine, `NestWitness` nesting, `SourceLocus` ingress, `EquipmentWitness` tool and quantity admission, `DeriveWitness` plan derivation, `RelationFault` inadmissible Process-axis pairs, `KerfWitness` kerf topology, `FaultSubject` S0 references to upper-plane subjects.
- Cases: `JointDiagnostic` distinguishes joint bounds, singularity, reach, contact, rate limits, torque, configuration, and disconnected chains, folding the four rate arms onto one `Rate` base and the two contact arms onto one `Contact` base so the shared predicate has one body. `NestWitness` distinguishes demand, fit, mass, material, thickness, grain, quantity, remnant, admission, search-budget, provider-proof, inventory-row, and lineage failures. `EquipmentWitness` distinguishes equipment geometry, spent life, head-physics refusal, one directed range violation, quantity text, and grade admission. `DeriveWitness` distinguishes component, topology, lot, capacity, setup, assembly, and identifier rejections.
- Entry: `Witness.Admit<TSelf, TKind>` is the one admission over every family; each union exposes it as its own `Admit` and supplies `WitnessKey` symbolically through `nameof`. `CollisionZone.Admit` and `VoxelBudget.Admit` ride the `Admission` bridge, so no hand ternary restates the generated `Validate` contract.
- Auto: `Refused` is the one verdict both `Admit` and `Carried` read, so the gated mint reuses the refusal admission already built rather than discarding it and constructing a second.
- Receipt: the concrete case and its evidence remain recoverable without parsing. Native write rejection and ingress unavailability retain provider text because each provider owns that taxonomy.
- Packages: `Rasm.Domain.Expected`, RhinoCommon value geometry, `NodaTime.Instant` and `Duration` on lot evidence, Thinktecture.Runtime.Extensions, LanguageExt.Core, and BCL inbox compose directly.
- Boundary: a witness kind admits only its own payload type, so a cross-family pairing fails admission rather than reporting a foreign condition. A payload arrives from a generated union already non-null, so the contract carries no null arm and no reflected type tag.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public interface IWitnessKind<in TWitness> {
    string Key { get; }
    Func<TWitness, bool> Admits { get; }
}

public interface IWitness<TSelf, out TKind>
    where TSelf : class, IWitness<TSelf, TKind>
    where TKind : IWitnessKind<TSelf> {
    TKind Kind { get; }
    static abstract string WitnessKey { get; }
}

public static class Witness {
    public static Func<TWitness, bool> Case<TWitness, TCase>(Func<TCase, bool> admits)
        where TCase : TWitness => witness => witness is TCase typed && admits(typed);

    public static Fin<TSelf> Admit<TSelf, TKind>(TSelf candidate)
        where TSelf : class, IWitness<TSelf, TKind>
        where TKind : IWitnessKind<TSelf> =>
        Refused<TSelf, TKind>(candidate).Match(
            Some: Fin.Fail<TSelf>,
            None: () => Fin.Succ(candidate));

    // The gated carrier is what puts the predicate on the raising path. A witness whose payload contradicts its own
    // kind cannot describe the condition it names, so the mint substitutes the SAME `WitnessMalformed` admission
    // already decided: a fault is still raised, the cause stays addressable, and the raise site keeps its
    // `FabricationFault`-shaped return with no rail widening. State threads so every carrier arm stays closure-free.
    public static FabricationFault Carried<TSelf, TKind, TState>(
        TSelf candidate,
        TState state,
        Func<TState, TSelf, FabricationFault> carrier)
        where TSelf : class, IWitness<TSelf, TKind>
        where TKind : IWitnessKind<TSelf> =>
        Refused<TSelf, TKind>(candidate).IfNone(() => carrier(state, candidate));

    public static bool Keyed(string value) => !string.IsNullOrWhiteSpace(value);
    public static bool Index(int value) => value >= 0;
    public static bool Pair(int first, int second) => Index(first) && Index(second) && first != second;
    public static bool Positive(double value) => double.IsFinite(value) && value > 0.0;
    public static bool Finite(double first, double second) => double.IsFinite(first) && double.IsFinite(second);
    public static bool Finite(double first, double second, double third) => Finite(first, second) && double.IsFinite(third);

    private static Option<FabricationFault> Refused<TSelf, TKind>(TSelf candidate)
        where TSelf : class, IWitness<TSelf, TKind>
        where TKind : IWitnessKind<TSelf> =>
        candidate.Kind.Admits(candidate)
            ? None
            : Some<FabricationFault>(new FabricationFault.WitnessMalformed(TSelf.WitnessKey, candidate.Kind.Key));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaultSubject(SubjectKind Kind) : IWitness<FaultSubject, SubjectKind> {
    public sealed record Strategy(string Key) : FaultSubject(SubjectKind.Strategy);
    public sealed record VoxelOperation(string Key) : FaultSubject(SubjectKind.VoxelOperation);
    public sealed record ProgramNode(string Key) : FaultSubject(SubjectKind.ProgramNode);
    public sealed record Specification(ContentKey Key) : FaultSubject(SubjectKind.Specification);
    public sealed record Partition(string Key) : FaultSubject(SubjectKind.Partition);
    public sealed record Extension(string MediaType, string Key) : FaultSubject(SubjectKind.Extension);
    public sealed record Lineage(ContentKey Key) : FaultSubject(SubjectKind.Lineage);
    public sealed record Stage(string Key) : FaultSubject(SubjectKind.Stage);
    public sealed record Bevel(string Key) : FaultSubject(SubjectKind.Bevel);
    public sealed record Qualification(string Key) : FaultSubject(SubjectKind.Qualification);

    public static string WitnessKey => nameof(FaultSubject);
    public static Fin<FaultSubject> Admit(FaultSubject candidate) => Witness.Admit<FaultSubject, SubjectKind>(candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EquipmentWitness(EquipmentFault Kind) : IWitness<EquipmentWitness, EquipmentFault> {
    public sealed record Geometry(Tool Candidate, string Axis) : EquipmentWitness(EquipmentFault.Geometry);
    public sealed record Spent(UInt128 Identity, Option<Operation> Operation) : EquipmentWitness(EquipmentFault.Spent);
    public sealed record HeadPhysics(PhysicsKind Required, Option<ToolClass> Mounted) : EquipmentWitness(EquipmentFault.HeadPhysics);
    public sealed record Range(PhysicsQuantity Bound, RangeSide Side, double Derived, double Limit) : EquipmentWitness(EquipmentFault.Range);
    public sealed record Quantity(string Kind, string Text) : EquipmentWitness(EquipmentFault.Quantity);
    public sealed record Grade(string Locus) : EquipmentWitness(EquipmentFault.Grade);

    public static string WitnessKey => nameof(EquipmentWitness);
    public static Fin<EquipmentWitness> Admit(EquipmentWitness candidate) => Witness.Admit<EquipmentWitness, EquipmentFault>(candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeriveWitness(DeriveFault Kind) : IWitness<DeriveWitness, DeriveFault> {
    public sealed record ComponentMismatch(UInt128 Requested, UInt128 Assessed) : DeriveWitness(DeriveFault.ComponentMismatch);
    // A cycle refusal carries the strongly-connected component's MEMBERS: a vertex/edge count pair names no operation
    // a caller can break, and the component labels are what the detecting walk already computed.
    public sealed record OperationCycle(Arr<int> Cycle) : DeriveWitness(DeriveFault.OperationCycle);
    public sealed record DuplicateOperation(int Id) : DeriveWitness(DeriveFault.DuplicateOperation);
    public sealed record UnknownPredecessor(int Operation, int Predecessor) : DeriveWitness(DeriveFault.UnknownPredecessor);
    public sealed record LotInadmissible(
        int Quantity,
        int BatchSize,
        Instant Release,
        Instant Due,
        Duration TransferBuffer,
        Arr<UInt128> Predecessors) : DeriveWitness(DeriveFault.LotInadmissible);
    public sealed record LotOverdue(Instant Completion, Instant Due) : DeriveWitness(DeriveFault.LotOverdue);
    public sealed record LotUnschedulable(int Operation, Instant Ready, Duration Effort) : DeriveWitness(DeriveFault.LotUnschedulable);
    // Finite capacity refuses on the ASSIGNED instance census, so the receipt names how many machine instances the
    // schedule had and what effort could not seat inside their availability.
    public sealed record CapacityExhausted(int Operation, int Instances, Instant Ready, Duration Effort)
        : DeriveWitness(DeriveFault.CapacityExhausted);
    public sealed record PredecessorLotMissing(UInt128 Lot) : DeriveWitness(DeriveFault.PredecessorLotMissing);
    public sealed record SetupCoverage(int Assigned, int Required) : DeriveWitness(DeriveFault.SetupCoverage);
    public sealed record AssemblyMismatch(UInt128 Component) : DeriveWitness(DeriveFault.AssemblyMismatch);
    public sealed record AssemblyRequired(int Connections) : DeriveWitness(DeriveFault.AssemblyRequired);
    public sealed record JoinReceiptMissing(int Joint) : DeriveWitness(DeriveFault.JoinReceiptMissing);
    public sealed record IdentifierExhausted(long Next, int Requested) : DeriveWitness(DeriveFault.IdentifierExhausted);
    public sealed record OperationAbsent(int Id) : DeriveWitness(DeriveFault.OperationAbsent);
    public sealed record OperationsEmpty(int Joints) : DeriveWitness(DeriveFault.OperationsEmpty);
    public sealed record DemandInadmissible(int Id) : DeriveWitness(DeriveFault.DemandInadmissible);

    public static string WitnessKey => nameof(DeriveWitness);
    public static Fin<DeriveWitness> Admit(DeriveWitness candidate) => Witness.Admit<DeriveWitness, DeriveFault>(candidate);
}

// The rate and contact bases carry the shared operand shape, so `JointFault` states one predicate per SHAPE and a
// new rate limit is one leaf case with no predicate body of its own.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record JointDiagnostic(JointFault Kind) : IWitness<JointDiagnostic, JointFault> {
    public abstract record Rate(int Joint, double Required, double Limit, JointFault Fault) : JointDiagnostic(Fault);
    public abstract record Contact(int FirstLink, int SecondLink, double Clearance, JointFault Fault) : JointDiagnostic(Fault);

    public sealed record JointLimit(int Joint, double Position, double Lower, double Upper)
        : JointDiagnostic(JointFault.JointLimit);
    public sealed record Singularity(int Joint, double ConditionNumber, double Limit)
        : JointDiagnostic(JointFault.Singularity);
    public sealed record Reach(int Target, double Distance, double Limit)
        : JointDiagnostic(JointFault.Reach);
    public sealed record Collision(int FirstLink, int SecondLink, double Clearance)
        : Contact(FirstLink, SecondLink, Clearance, JointFault.Collision);
    public sealed record SelfCollision(int FirstLink, int SecondLink, double Clearance)
        : Contact(FirstLink, SecondLink, Clearance, JointFault.SelfCollision);
    public sealed record Velocity(int Joint, double Required, double Limit)
        : Rate(Joint, Required, Limit, JointFault.Velocity);
    public sealed record Acceleration(int Joint, double Required, double Limit)
        : Rate(Joint, Required, Limit, JointFault.Acceleration);
    public sealed record Jerk(int Joint, double Required, double Limit)
        : Rate(Joint, Required, Limit, JointFault.Jerk);
    public sealed record Torque(int Joint, double Required, double Limit)
        : Rate(Joint, Required, Limit, JointFault.Torque);
    public sealed record Configuration(string Requested, string Admitted)
        : JointDiagnostic(JointFault.Configuration);
    public sealed record Disconnected(int Link, int ExpectedParent, int ActualParent)
        : JointDiagnostic(JointFault.Disconnected);

    public static string WitnessKey => nameof(JointDiagnostic);
    public static Fin<JointDiagnostic> Admit(JointDiagnostic candidate) => Witness.Admit<JointDiagnostic, JointFault>(candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NestWitness(NestFault Kind) : IWitness<NestWitness, NestFault> {
    public sealed record EmptyCutList : NestWitness(NestFault.EmptyCutList);
    public sealed record PartExceedsStock(int Part, double PartAreaMm2, double StockAreaMm2)
        : NestWitness(NestFault.PartExceedsStock);
    public sealed record HeterogeneousMassCut(int FirstPart, int SecondPart, double FirstMass, double SecondMass)
        : NestWitness(NestFault.HeterogeneousMassCut);
    public sealed record MaterialMismatch(int Part, Material Required, Material Stock)
        : NestWitness(NestFault.MaterialMismatch);
    public sealed record ThicknessMismatch(int Part, double RequiredMm, double StockMm)
        : NestWitness(NestFault.ThicknessMismatch);
    public sealed record GrainIncompatible(int Part, double RequiredDeg, double StockDeg)
        : NestWitness(NestFault.GrainIncompatible);
    public sealed record InvalidQuantity(int Part, int Quantity) : NestWitness(NestFault.InvalidQuantity);
    public sealed record RemnantUnavailable(ContentKey Remnant) : NestWitness(NestFault.RemnantUnavailable);
    public sealed record Admission(FaultSubject.Stage Stage, Option<int> Subject, string Detail)
        : NestWitness(NestFault.Admission);
    public sealed record StrategyBudget(
        FaultSubject.Strategy Strategy,
        int CountBudget,
        int DepthBudget,
        int Visited,
        int Pending,
        int Depth) : NestWitness(NestFault.StrategyBudget);
    public sealed record ProviderProof(
        FaultSubject.Strategy Strategy,
        int Placements,
        bool Contained,
        bool Disjoint) : NestWitness(NestFault.ProviderProof);
    public sealed record InventoryRow(ContentKey Key, int Revision, int Claims, int ClaimCap, Set<NestDefect> Defects)
        : NestWitness(NestFault.InventoryRow);
    public sealed record Lineage(int Rows, int Indexed, int Ordered, Set<NestDefect> Defects)
        : NestWitness(NestFault.Lineage);

    public static string WitnessKey => nameof(NestWitness);
    public static Fin<NestWitness> Admit(NestWitness candidate) => Witness.Admit<NestWitness, NestFault>(candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SourceLocus(SourceKind Kind) : IWitness<SourceLocus, SourceKind> {
    public sealed record DxfEntity(string Handle) : SourceLocus(SourceKind.Profile);
    public sealed record OcctShape(int Id) : SourceLocus(SourceKind.Solid);
    public sealed record DstvBlock(string Block, int Line) : SourceLocus(SourceKind.Steel);
    public sealed record ElementNode(UInt128 NodeKey) : SourceLocus(SourceKind.Element);
    public sealed record ThreeMfObject(string Model, int ObjectId) : SourceLocus(SourceKind.ThreeMf);
    public sealed record RhinoObject(Guid ObjectId) : SourceLocus(SourceKind.Rhino);
    public sealed record MeshFace(int Face) : SourceLocus(SourceKind.Mesh);
    public sealed record ProgramBlock(int Block) : SourceLocus(SourceKind.Program);
    public sealed record ExchangeEntity(string Scheme, string Entity) : SourceLocus(SourceKind.Exchange);

    public static string WitnessKey => nameof(SourceLocus);
    public static Fin<SourceLocus> Admit(SourceLocus candidate) => Witness.Admit<SourceLocus, SourceKind>(candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RelationFault(RelationKind Kind) : IWitness<RelationFault, RelationKind> {
    public sealed record ModalityStrategy(ProcessModality Modality, CutStrategy Strategy) : RelationFault(RelationKind.ModalityStrategy);
    public sealed record ProcessMachine(ProcessKind Process, Machine Machine) : RelationFault(RelationKind.ProcessMachine);
    public sealed record DialectModality(PostDialect Dialect, ProcessModality Modality) : RelationFault(RelationKind.DialectModality);
    public sealed record OperationEquipment(Operation Operation, Tool Equipment) : RelationFault(RelationKind.OperationEquipment);
    public sealed record ProcessMaterial(ProcessKind Process, Material Material) : RelationFault(RelationKind.ProcessMaterial);

    public static string WitnessKey => nameof(RelationFault);
    public static Fin<RelationFault> Admit(RelationFault candidate) => Witness.Admit<RelationFault, RelationKind>(candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KerfWitness(KerfKind Kind) : IWitness<KerfWitness, KerfKind> {
    public sealed record Vanished(int Region) : KerfWitness(KerfKind.Vanished);
    public sealed record Overlapped(int First, int Second) : KerfWitness(KerfKind.Overlapped);

    public static string WitnessKey => nameof(KerfWitness);
    public static Fin<KerfWitness> Admit(KerfWitness candidate) => Witness.Admit<KerfWitness, KerfKind>(candidate);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class CollisionZone {
    public ContentKey Key { get; }
    public BoundingBox Bounds { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref ContentKey key,
        ref BoundingBox bounds) {
        if (!bounds.IsValid)
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "collision-zone");
    }

    public static Fin<CollisionZone> Admit(ContentKey key, BoundingBox bounds) =>
        Validate(key, bounds, out CollisionZone zone).Admitted(zone);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class VoxelBudget {
    public BoundingBox Bounds { get; }
    public double VoxelSizeMm { get; }
    public long VoxelCap { get; }
    public long RequiredCells { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref BoundingBox bounds,
        ref double voxelSizeMm,
        ref long voxelCap,
        ref long requiredCells) {
        if (!bounds.IsValid || !Witness.Positive(voxelSizeMm)
            || voxelCap <= 0 || requiredCells < 0 || requiredCells > voxelCap)
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Additive, "voxel-budget");
    }

    public static Fin<VoxelBudget> Admit(BoundingBox bounds, double voxelSizeMm, long voxelCap, long requiredCells) =>
        Validate(bounds, voxelSizeMm, voxelCap, requiredCells, out VoxelBudget budget).Admitted(budget);
}
```

## [04]-[FAULT_BAND]

- Owner: `FabricationFault` owns the closed band, its per-case `FabConcern` allocation, and the seven witness-carrying mints every raise site reaches.
- Cases: `PolicyInadmissible` is the witness-free cross-plane arm every folder's own policy/parameter admission gate raises, threading its raising `FabConcern` the way `OpenLoop` does; `RunAbandoned` is its abandonment counterpart, threading the same slot and carrying the declared stage fraction and witness a withdrawn run reached; `FixtureInadmissible` seats the fixturing plane's refusal here, so the offset ledger stays whole on one page and no folder mints a same-named partial in a second namespace. A plane earns a case of its own only where the refusal carries evidence a caller ACTS on — a magazine seat and its occupant, the model and sample count a fit failed on, the station key and effort contention could not seat, the link an inadmissible chain breaks at, the word a parsed block never resolved, the axis an unfabricable geometry failed — and every remaining refusal answers on `PolicyInadmissible` with its own plane and locus.
- Entry: `FabricationFault.Equipment`, `.Derivation`, `.Joint`, `.Nested`, `.Sourced`, `.Unavailable`, `.Pairing`, and `.Kerf` are the witness-carrying mints — each folds its payload through `Witness.Carried` and returns a `FabricationFault`, so a raise site pays no rail change to gate its own evidence and `new FabricationFault.EquipmentInadmissible(...)` beside the mint is the deleted form. Witness-free cases lift directly into `Fin.Fail<T>`.
- Auto: `Code` is `FaultBand.Fabrication + Offset`, `Message` is the invariant key, and `Category` is `Fabrication`. `Create(string)` satisfies `IValidationError<FabricationFault>`, so every generated owner stamped `[ValidationError<FabricationFault>]` mints on this band and the whole package's admission refusals share one taxonomy. No second case-to-code or case-to-message sweep exists.
- Receipt: `MessageKey` is the mechanical kebab of the case name under one `fabrication:` prefix, so the ledger reads as a table and a hand-shortened key is the named defect. A cycle refusal carries the detecting walk's strongly-connected component members, never a vertex-and-edge count a caller cannot act on.
- Growth: a new fabrication failure is one case at the free offset carrying its owning `FabConcern`; higher-plane evidence crosses as the narrow matching `FaultSubject` case or that plane's own witness union, never as an upper-plane behavioural import.
- Boundary: codes never renumber, keys never interpolate runtime values, and each payload retains the discriminants required for recovery. A case whose owning plane later settles its condition as a VERDICT holds its offset without a producer — `StackupExceeded` is the landed one, because `Spec/capability` `StackupReceipt.Pass`, `Spec/tolerance` `ToleranceReceipt.Conforming`, and `Spec/manufacturability` `StackupPrecheck.Pass` all answer an exceeded bound as the study's own result and refusing it would destroy the contribution ranking that names the term worth tightening; re-minting that condition as a fault at any consuming gate is the deleted form, and the frozen code is what stops the offset carrying a second meaning later.

```csharp signature
// --- [ERRORS] -------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationFault(int Offset, string MessageKey, FabConcern Concern)
    : Expected(MessageKey, FaultBand.Fabrication + Offset), IValidationError<FabricationFault> {
    public sealed record NoFit(int Part, Seq<double> TriedRotations) : FabricationFault(1, "fabrication:no-fit", FabConcern.Nesting);
    public sealed record Unreachable(JointDiagnostic Diag, int Target) : FabricationFault(2, "fabrication:unreachable", FabConcern.Kinematics);
    public sealed record KerfCollision(KerfWitness Witness, double Kerf) : FabricationFault(3, "fabrication:kerf-collision", FabConcern.Nesting);
    public sealed record OpenLoop(FabConcern Raised, int Primitive) : FabricationFault(4, "fabrication:open-loop", Raised);
    public sealed record InadmissiblePair(RelationFault Pair) : FabricationFault(5, "fabrication:inadmissible-pair", FabConcern.Process);
    public sealed record Gouge(Point3d Point, CutterForm Tool) : FabricationFault(6, "fabrication:gouge", FabConcern.Toolpath);
    public sealed record Collision(CollisionZone Zone, CollisionContact Contact) : FabricationFault(7, "fabrication:collision", FabConcern.Toolpath);
    public sealed record NonManifoldSlice(int Layer, int OpenChains) : FabricationFault(8, "fabrication:non-manifold-slice", FabConcern.Additive);
    public sealed record StockOverflow(int Unplaced, int Sheets) : FabricationFault(9, "fabrication:stock-overflow", FabConcern.Nesting);
    public sealed record Nest(NestWitness Witness) : FabricationFault(10, "fabrication:nest", FabConcern.Nesting);
    public sealed record IngressTranslation(SourceLocus Locus) : FabricationFault(11, "fabrication:ingress-translation", FabConcern.Ingress);
    public sealed record MachinabilityUnknown(Material Material, Operation Op) : FabricationFault(12, "fabrication:machinability-unknown", FabConcern.Tooling);
    public sealed record SampleStalled(FaultSubject.Strategy Strategy, int Iteration) : FabricationFault(13, "fabrication:sample-stalled", FabConcern.Toolpath);
    public sealed record AxisSingularity(MachineAxis Axis, double Angle) : FabricationFault(14, "fabrication:axis-singularity", FabConcern.Kinematics);
    public sealed record VoxelFault(FaultSubject.VoxelOperation Op, VoxelBudget Budget) : FabricationFault(15, "fabrication:voxel-fault", FabConcern.Additive);
    public sealed record OrientationInfeasible(int Overhangs, double BestScore) : FabricationFault(16, "fabrication:orientation-infeasible", FabConcern.Additive);
    public sealed record SetupInfeasible(int Operation, int TriedSetups) : FabricationFault(17, "fabrication:setup-infeasible", FabConcern.Fixturing);
    public sealed record DialectUnsupported(PostDialect Dialect, FaultSubject.ProgramNode Node) : FabricationFault(18, "fabrication:dialect-unsupported", FabConcern.Posting);
    public sealed record ProgramParse(int Line, ModalGroup Group) : FabricationFault(19, "fabrication:program-parse", FabConcern.Posting);
    public sealed record ProbeOvertravel(Point3d At, double Limit) : FabricationFault(20, "fabrication:probe-overtravel", FabConcern.Verify);
    public sealed record ToleranceUnsatisfiable(FaultSubject.Specification Frame, double Achievable) : FabricationFault(21, "fabrication:tolerance-unsatisfiable", FabConcern.Spec);
    public sealed record CapabilityShortfall(ProcessKind Process, double Cpk, double Demanded) : FabricationFault(22, "fabrication:capability-shortfall", FabConcern.Spec);
    public sealed record PartitionDegenerate(FaultSubject.Partition Strategy, int Sites) : FabricationFault(23, "fabrication:partition-degenerate", FabConcern.Toolpath);
    public sealed record NoToolForOp(Operation Op, CutterForm Required) : FabricationFault(24, "fabrication:no-tool-for-op", FabConcern.Tooling);
    public sealed record UnsupportedThreeMfExtension(FaultSubject.Extension Extension, EgressKind Target) : FabricationFault(25, "fabrication:unsupported-three-mf-extension", FabConcern.Additive);
    public sealed record DatumLineageBroken(FaultSubject.Lineage Chain) : FabricationFault(26, "fabrication:datum-lineage-broken", FabConcern.Fixturing);
    public sealed record ClampOnMachinedFace(int Operation, Point3d At) : FabricationFault(27, "fabrication:clamp-on-machined-face", FabConcern.Fixturing);
    public sealed record BlockCapExceeded(PostDialect Dialect, int Blocks, int Cap) : FabricationFault(28, "fabrication:block-cap-exceeded", FabConcern.Posting);
    public sealed record StackupExceeded(FaultSubject.Specification Chain, double Accumulated, double Bound) : FabricationFault(29, "fabrication:stackup-exceeded", FabConcern.Spec);
    public sealed record RoutingInfeasible(UInt128 ComponentKey, FaultSubject.Stage Stage) : FabricationFault(30, "fabrication:routing-infeasible", FabConcern.Derivation);
    public sealed record WearEstimateUnfit(Tool Tool, int Samples) : FabricationFault(31, "fabrication:wear-estimate-unfit", FabConcern.Tooling);
    public sealed record WireTaperExceeded(double AngleDeg, double GuideLimitDeg) : FabricationFault(32, "fabrication:wire-taper-exceeded", FabConcern.Toolpath);
    public sealed record LinkBlocked(Point3d From, Point3d To) : FabricationFault(33, "fabrication:link-blocked", FabConcern.Toolpath);
    public sealed record BevelUnsupported(FaultSubject.Bevel Bevel, double AngleDeg) : FabricationFault(34, "fabrication:bevel-unsupported", FabConcern.Toolpath);
    public sealed record SupportUnbuildable(int Layer, int Region) : FabricationFault(35, "fabrication:support-unbuildable", FabConcern.Additive);
    public sealed record RemnantStale(ContentKey Key) : FabricationFault(36, "fabrication:remnant-stale", FabConcern.Nesting);
    public sealed record AssemblyPrecedenceCyclic(Arr<int> Cycle) : FabricationFault(37, "fabrication:assembly-precedence-cyclic", FabConcern.Fixturing);
    public sealed record EnvelopeExceeded(MachineAxis Axis, double At, double Limit) : FabricationFault(38, "fabrication:envelope-exceeded", FabConcern.Kinematics);
    public sealed record SimulatedOvertravel(int Block, MachineAxis Axis, double By) : FabricationFault(39, "fabrication:simulated-overtravel", FabConcern.Verify);
    public sealed record UnfoldInfeasible(int Faces, int Branches) : FabricationFault(40, "fabrication:unfold-infeasible", FabConcern.Forming);
    public sealed record BendSequenceInfeasible(int RejectedCandidates, int ExpandedStates) : FabricationFault(41, "fabrication:bend-sequence-infeasible", FabConcern.Forming);
    public sealed record TonnageExceeded(double RequiredKn, double CapacityKn) : FabricationFault(42, "fabrication:tonnage-exceeded", FabConcern.Forming);
    public sealed record MinBendRadiusViolated(int Bend, double RadiusMm, double FloorMm) : FabricationFault(43, "fabrication:min-bend-radius-violated", FabConcern.Forming);
    public sealed record WeldAccessBlocked(int Joint, double TorchAngleDeg) : FabricationFault(44, "fabrication:weld-access-blocked", FabConcern.Joining);
    public sealed record HeatInputExceeded(int Joint, double KjPerMm, double Cap) : FabricationFault(45, "fabrication:heat-input-exceeded", FabConcern.Joining);
    public sealed record WpsUnqualified(FaultSubject.Qualification Variable, double Value) : FabricationFault(46, "fabrication:wps-unqualified", FabConcern.Joining);
    public sealed record ThreeMfWriteRejected(EgressKind Target, string Native) : FabricationFault(47, "fabrication:three-mf-write-rejected", FabConcern.Additive);
    public sealed record UnknownAxis(string Axis, string Key) : FabricationFault(48, "fabrication:unknown-axis", FabConcern.Process);
    public sealed record IngressProviderUnavailable(SourceLocus Locus, string Detail) : FabricationFault(49, "fabrication:ingress-provider-unavailable", FabConcern.Ingress);
    public sealed record WitnessMalformed(string Witness, string Kind) : FabricationFault(50, "fabrication:witness-malformed", FabConcern.Process);
    public sealed record EquipmentInadmissible(EquipmentWitness Witness) : FabricationFault(51, "fabrication:equipment-inadmissible", FabConcern.Process);
    public sealed record DerivationRejected(DeriveWitness Witness, FaultSubject.Stage Stage) : FabricationFault(52, "fabrication:derivation-rejected", FabConcern.Derivation);
    public sealed record BendSearchBudgetExceeded(int ExpandedStates, int PendingStates)
        : FabricationFault(53, "fabrication:bend-search-budget-exceeded", FabConcern.Forming);
    // Offset 54 is the policy/parameter admission refusal every folder's own admission gate raises: a declared
    // policy, request, or parameter tuple that fails its own gate is a CONTRACT failure, so it answers on this band
    // rather than borrowing the kernel geometry band. Raised is threaded (the OpenLoop shape) because every plane
    // raises it, and Locus is the raising gate's own angle-free discriminant (`guard-policy:clearance-plane`).
    public sealed record PolicyInadmissible(FabConcern Raised, string Locus)
        : FabricationFault(54, "fabrication:policy-inadmissible", Raised);

    // The fixturing plane's refusal seats on the band that allocates its offset: a same-named `partial record` in a
    // second namespace is a DISTINCT type whose cases never reach this union's `Switch`, so the case lands here and
    // its folder-domain witness vocabulary stays at `Fixturing/workholding` where its own axes live.
    public sealed record FixtureInadmissible(FixturingWitness Witness)
        : FabricationFault(55, "fabrication:fixture-inadmissible", FabConcern.Fixturing);

    // Abandonment is not a contract failure: nothing about the request was inadmissible, an owner withdrew it
    // mid-flight. It answers on its own case so a caller separates "the run was refused" from "the run was
    // stopped", and it carries the DONE fraction the abandoning stage declared beside that stage's witness —
    // the same evidence shape the kernel `GeometryFault.RunAbandoned` carries for the arrangement lane, which
    // this case never borrows because its `Kind` slot names geometry a fabrication run has no value for.
    public sealed record RunAbandoned(FabConcern Raised, double Done, string Witness)
        : FabricationFault(56, "fabrication:run-abandoned", Raised);

    // Tooling. A magazine slot is a physical seat, so its refusal names the seat, what already occupies it, and what
    // asked for it; a model that cannot be fit names the model and the sample count that failed to determine it.
    public sealed record ToolSlotConflict(int Slot, Option<string> Occupant, string Requested)
        : FabricationFault(57, "fabrication:tool-slot-conflict", FabConcern.Tooling);
    public sealed record ToolAssetInadmissible(string ToolId, string Axis)
        : FabricationFault(58, "fabrication:tool-asset-inadmissible", FabConcern.Tooling);
    public sealed record CuttingModelUnfit(Material Material, Operation Op, string Model, int Samples)
        : FabricationFault(59, "fabrication:cutting-model-unfit", FabConcern.Tooling);
    public sealed record StabilityUnavailable(double RequestedDepthMm, int Lobes)
        : FabricationFault(60, "fabrication:stability-unavailable", FabConcern.Tooling);

    // Fleet. Contention answers on the INSTANCE, so the refusal carries the station key, the moment work became
    // ready, and the effort that could not seat inside its availability — a machine CLASS names no station.
    public sealed record MachineInstanceUnavailable(MachineInstanceKey Instance, Instant From, Duration Effort)
        : FabricationFault(61, "fabrication:machine-instance-unavailable", FabConcern.Fleet);
    public sealed record FleetAssignmentInfeasible(int Demands, int Instances)
        : FabricationFault(62, "fabrication:fleet-assignment-infeasible", FabConcern.Fleet);

    // Kinematics. A chain refusal names the cell, the link, and the axis, because a caller repairs a chain at a
    // link and never at the whole cell.
    public sealed record KinematicChainInadmissible(string Cell, int Link, string Axis)
        : FabricationFault(63, "fabrication:kinematic-chain-inadmissible", FabConcern.Kinematics);

    // Posting. `ProgramParse` answers for a malformed block; an unresolved WORD is a different failure — the block
    // parsed and its word reached no command — and an optimization pass that cannot preserve semantics names the
    // pass rather than reporting a parse it never ran.
    public sealed record ProgramTokenUnresolved(int Line, string Word)
        : FabricationFault(64, "fabrication:program-token-unresolved", FabConcern.Posting);
    public sealed record OptimizationRefused(string Pass, string Locus)
        : FabricationFault(65, "fabrication:optimization-refused", FabConcern.Posting);

    // Ingress. A source the provider read CLEANLY whose geometry cannot be admitted is not a provider outage:
    // separating the two is what lets a caller distinguish a broken reader from an unfabricable model.
    public sealed record IngressGeometryUnfit(SourceLocus Locus, string Axis)
        : FabricationFault(66, "fabrication:ingress-geometry-unfit", FabConcern.Ingress);

    // Every witness-carrying case mints here, so the payload's own kind predicate runs before the fault exists.
    // A raise site names the concern and hands its payload; the carrier arm stays static because the case-specific
    // remainder — the stage, the target ordinal, the provider detail — threads through the state slot.
    public static FabricationFault Equipment(EquipmentWitness witness) =>
        Witness.Carried<EquipmentWitness, EquipmentFault, Unit>(
            witness, unit, static (_, row) => new EquipmentInadmissible(row));

    public static FabricationFault Derivation(DeriveWitness witness, FaultSubject.Stage stage) =>
        Witness.Carried<DeriveWitness, DeriveFault, FaultSubject.Stage>(
            witness, stage, static (at, row) => new DerivationRejected(row, at));

    public static FabricationFault Joint(JointDiagnostic diagnostic, int target) =>
        Witness.Carried<JointDiagnostic, JointFault, int>(
            diagnostic, target, static (at, row) => new Unreachable(row, at));

    public static FabricationFault Nested(NestWitness witness) =>
        Witness.Carried<NestWitness, NestFault, Unit>(witness, unit, static (_, row) => new Nest(row));

    public static FabricationFault Sourced(SourceLocus locus) =>
        Witness.Carried<SourceLocus, SourceKind, Unit>(locus, unit, static (_, row) => new IngressTranslation(row));

    public static FabricationFault Unavailable(SourceLocus locus, string detail) =>
        Witness.Carried<SourceLocus, SourceKind, string>(
            locus, detail, static (text, row) => new IngressProviderUnavailable(row, text));

    public static FabricationFault Pairing(RelationFault pair) =>
        Witness.Carried<RelationFault, RelationKind, Unit>(pair, unit, static (_, row) => new InadmissiblePair(row));

    public static FabricationFault Kerf(KerfWitness witness, double kerf) =>
        Witness.Carried<KerfWitness, KerfKind, double>(
            witness, kerf, static (width, row) => new KerfCollision(row, width));

    // The generated-owner contract: a `[ValidationError<FabricationFault>]` owner refuses through this mint, and the
    // owner's own `ValidateFactoryArguments` names the specific case where its plane is known.
    public static FabricationFault Create(string message) => new PolicyInadmissible(FabConcern.Process, message);

    public override int Code => FaultBand.Fabrication + Offset;
    public override string Message => MessageKey;
    public override string Category => "Fabrication";
}
```

## [05]-[ADMISSION]

- Owner: `Admission` owns the ONE bridge from a generated owner's `Validate` contract to the `Fin` rail, and it sits on this page because the raise sites span every folder plane and `FabConcern` puts `Process` at stratum `0` beneath all of them, so each consumption edge points down. The accumulating combinators are NOT declared here: `Rasm.Element.Projection.AdmissionSlots` owns them for every package that rebinds a `FaultBand` row, `Gate(holds, refusal)` lifting one boolean invariant and the `FabricationFault` its raise site already minted, and `Accumulate(slots)` folding a slot run where the arity outruns the tuple `.Apply`.
- Entry: `Admitted(admitted)` closes a member-ordered `[ComplexValueObject]` admission over EITHER owner kind — the generator hands a reference owner back through a null-annotated slot and a value owner through the value slot itself, so one extension binds both — while `Admission.Of<TOwner, TRaw>` closes a keyed reference owner and `Admission.OfValue<TOwner, TRaw>` its `readonly partial struct` sibling. Constraints are not part of a signature, so the two keyed arities are two names rather than one overload pair. A raise site reaches `AdmissionSlots.Gate` for the one lift and `AdmissionSlots.Accumulate` for the one fold, joining a closed admission through the tuple `.Apply` directly and reaching the fold only past that arity.
- Auto: `Validate` returns null exactly when its `out` slot is populated, so one read decides both arms and the null-forgiving projection is the contract, not a guard. The slot answer is the concrete `Validation<Error, Unit>` — the lift IS a user-defined implicit conversion, which C# cannot target at an interface — and it upcasts to the `K<F,A>` the tuple `.Apply` receiver reads, `.As()` re-anchoring after each join, so a plane wrapper declaring that upcast as its own return feeds the fold's `K`-run arity with no re-cast; the fail arm target-types the bare `FabricationFault` through that lift, so `Fin.Succ(unit).ToValidation()`, `guard(…).ToFin().ToValidation()`, and `Validation<Error, Unit>.Success`/`.Fail` spelled at a raise site are all the deleted ceremony for one expression.
- Receipt: an accumulated refusal is the `ManyErrors` union of every violated invariant, each recoverable through `Error.IsType<T>()` exactly as a single refusal is — accumulation changes the arity of the evidence, never its taxonomy.
- Packages: `Rasm.Element` (`AdmissionSlots.Gate`/`.Accumulate` over the `FaultBand` registry this band's row sits in), LanguageExt.Core (`Validation<Error,_>`, `K<F,A>`, `Seq`, the `Apply` join and `Traverse` fold), Thinktecture.Runtime.Extensions (`IObjectFactory`, `IValidationError`).
- Boundary: the slot mints NO fault — the raise site owns which band answers for its refusal, so the combinator carries no `Kind`, no locus prefix, and no `FabricationFault` case, and a package-local OR per-folder re-declaration of the lift or the fold is the named defect: the copy forks the accumulation law and its type name collides with the `Rasm.Element` owner under `CS0104` at every page importing both namespaces plainly. `Accumulate` folds a slot RUN while the tuple `.Apply` joins a fixed product — a raise site picks by the shape it already holds. A boundary value enters through `Validate` or `TryCreate` and never through the throwing `Create`.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Admission {
    // A `[ValidationError<FabricationFault>]` owner returns the fault itself, so admission is ONE read: a null
    // refusal means the `out` slot is populated by contract. A hand ternary re-spelling this beside a `Validate`
    // call is the deleted form, and so is a lift that re-wraps the refusal in a second case. A struct owner's
    // generated slot is the value itself — never `Nullable<TOwner>` — so a value owner binds this same extension
    // and the `is { } refusal ? Fail : Succ(slot!.Value)` spelling is both the deleted form and uncompilable.
    public static Fin<TOwner> Admitted<TOwner>(this FabricationFault? refusal, TOwner admitted) =>
        refusal is null ? Fin.Succ(admitted) : Fin.Fail<TOwner>(refusal);

    // The keyed arity: a `[ValueObject<T>]` or `[SmartEnum<TKey>]` owner carries its raw admission statically, so a
    // constrained generic dispatches it with no instance and no member roster.
    public static Fin<TOwner> Of<TOwner, TRaw>(TRaw value, IFormatProvider? provider = null)
        where TOwner : class, IObjectFactory<TOwner, TRaw, FabricationFault>
        where TRaw : notnull =>
        TOwner.Validate(value, provider, out TOwner? admitted).Admitted(admitted!);

    // The keyed VALUE arity. `IObjectFactory` declares its slot as an unconstrained `out T?`, which substitutes to
    // the plain struct rather than to `Nullable<TOwner>`, so the struct constraint is what makes the call bind and
    // the two arities cannot share one name — a generic constraint never enters the signature overload resolution
    // reads. A keyed struct owner spelling its own `Validate` ternary is the deleted form.
    public static Fin<TOwner> OfValue<TOwner, TRaw>(TRaw value, IFormatProvider? provider = null)
        where TOwner : struct, IObjectFactory<TOwner, TRaw, FabricationFault>
        where TRaw : notnull =>
        TOwner.Validate(value, provider, out TOwner admitted).Admitted(admitted);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
