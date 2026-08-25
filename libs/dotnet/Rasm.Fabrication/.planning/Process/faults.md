# [RASM_FABRICATION_FAULTS]

`FabricationFault` is the sole fabrication failure rail. Its closed `[Union]` rides the kernel fault-estate floor — direct `Fault` leaves carry explicit `[FaultCase]` ordinals and generated numeric identity. Generated-owner validation crosses the kernel's default `ValidationError` bridge; no package validation-error family exists. Evidence stays S0: no generator, posting, fixturing, specification, or additive owner is imported.

Each predicate-bearing witness family pairs a case-shaped payload with a `[SmartEnum<string>]` predicate vocabulary and admits through `Witness.Admit<TSelf, TKind>` over the `IWitness<TSelf, TKind>` contract. Every fault carrying that contract mints through `Witness.Carried`, so the predicate runs on the raising path and a payload contradicting its own kind lands as `WitnessMalformed` rather than as a fault whose evidence refutes it. One `Refused` read owns that verdict, so admission and the gated mint never build two refusals for one payload.

`FaultSubject` preserves higher-plane identity as S0 keys and content identities, while `RelationFault` retains native Process axes. Consumers inspect concrete cases through `Error.IsType<T>()`; no identity conversion or message parser exists. A fault payload names its owning plane's own discriminant by TYPE where recovery demands it — `ModalGroup` for a parse position, `FixturingWitness` for a fixture refusal — and the page reaches no plane BEHAVIOR, so the atoms floor stays acyclic while the offset allocation stays whole on one page.

`FabConcern` is the plane census: every case declares the plane that owns its code, its `Folder` namespace, and the stratum that plane occupies, so a fault census partitions by owning plane without a second table and a split package states each of its planes truthfully. `OpenLoop`, `PolicyInadmissible`, and `RunAbandoned` thread their caller's plane into the same slot. Degenerate geometry remains `GeometryFault.DegenerateInput` — a geometric primitive that is itself degenerate, named by its real `Kind` with the real element ordinal where one exists and `None` where the whole input is the subject; a POLICY, request, or parameter tuple failing its own admission gate is a contract failure and takes `PolicyInadmissible`, never a kernel-band borrow under a fabricated `Kind` and a sentinel index. `FaultBand.Fabrication` is a kernel band (`Rasm/Domain/rails#[04]-[FAULT_BAND]` — `2700/66`, `BandKind.Fault`, `TelemetrySource.Fabrication`), whose `Code(offset)` refuses an out-of-span derivation and whose `Disjoint` fold proves the whole code space; this page allocates no band and mirrors no neighbour, and `[FaultCase]` joins the family to that row at case grain under a `generated identity admission` proof at first construction. Offsets `0` through `65` are the complete allocation and `66` is the next free offset.

Wire posture: HOST-LOCAL. `FabricationFault` rides `Fin<T>`, while frozen integer codes alone cross persisted records.

## [01]-[INDEX]

- [02]-[CONCERN_CENSUS]: `FabConcern` and the eight `IWitnessKind` predicate vocabularies — `EquipmentFault`, `DeriveFault`, `SubjectKind`, `JointFault`, `NestFault`, `SourceKind`, `RelationKind`, `KerfKind` — beside `RangeSide`, `NestDefect`, and `CollisionContact`.
- [03]-[WITNESS_EVIDENCE]: `IWitnessKind`, `IWitness`, `Witness` — the case lift, the admission fold, the gated carrier, and the two predicates no `ValidityClaim` row holds — and the payload families `FaultSubject`, `EquipmentWitness`, `DeriveWitness`, `JointDiagnostic`, `NestWitness`, `SourceLocus`, `RelationFault`, `KerfWitness`, `CollisionZone`, `VoxelBudget`.
- [04]-[FAULT_BAND]: `FabricationFault`, its direct `[FaultCase]` leaves and generated identity, and the eight witness-carrying mints.
- [05]-[ADMISSION]: `Admission`, the one generated-owner bridge onto the `Fin` rail beside the `Rasm.Element` `AdmissionSlots` band-blind `Gate` lift and `Accumulate` fold every accumulating policy admission composes.

## [02]-[CONCERN_CENSUS]

- Owner: `FabConcern` owns the plane census every fault case declares, and each `IWitnessKind` vocabulary owns one family's case predicates — the row that names a condition also decides whether a payload can describe it.
- Cases: `FabConcern` carries one row per PLANE, not per folder — `Folder` names the namespace a fault census partitions by, `Stratum` names the position that plane holds, so `Process` states its atoms floor, terminal derivation, and telemetry fan separately, and `Kinematics` states motion apart from its consuming fleet. `RangeSide` carries the comparison a bound violation runs, so one `Range` witness replaces a floor case and a ceiling case that differ only in direction. `NestDefect` is the one failed-check vocabulary the inventory and lineage witnesses both carry, replacing parallel boolean columns a predicate had to sniff.
- Auto: a kind row is `Of<TCase>(key, predicate)` — `Witness.Case` closes the type test, so the row's own predicate reads its case shape directly and a foreign payload fails the test rather than the predicate.
- Boundary: a predicate asserts the CONDITION the row names, never the presence of an admitted operand — a generated union hands non-null cases, so a null clause is refuted ceremony. Correspondence-bearing rows re-run the correspondence they refuse, so a pair that actually corresponds cannot mint as an inadmissible pair.
- Growth: a new failure condition is one row and one payload case; a new plane is one `FabConcern` row carrying its folder and stratum.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rhino.Geometry;
using Thinktecture;

namespace Rasm.Fabrication.Process;

// --- [TYPES] ---------------------------------------------------------------------------
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
        "range", static row => ValidityClaim.All(ValidityClaim.Finite([row.Derived, row.Limit]), row.Side.Exceeded(row.Derived, row.Limit)));
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
        "duplicate-operation", static row => ValidityClaim.Nonnegative(row.Id));
    public static readonly DeriveFault UnknownPredecessor = Of<DeriveWitness.UnknownPredecessor>(
        "unknown-predecessor", static row => Witness.Pair(row.Operation, row.Predecessor));
    public static readonly DeriveFault LotInadmissible = Of<DeriveWitness.LotInadmissible>(
        "lot-inadmissible", static row => row.Quantity < 1 || row.BatchSize < 1 || row.BatchSize > row.Quantity
            || row.Due < row.Release || row.TransferBuffer < Duration.Zero
            || row.Predecessors.Distinct().Count != row.Predecessors.Count || row.Predecessors.Contains(UInt128.Zero));
    public static readonly DeriveFault LotOverdue = Of<DeriveWitness.LotOverdue>(
        "lot-overdue", static row => row.Completion > row.Due);
    public static readonly DeriveFault LotUnschedulable = Of<DeriveWitness.LotUnschedulable>(
        "lot-unschedulable", static row => ValidityClaim.All(ValidityClaim.Nonnegative(row.Operation), row.Effort > Duration.Zero));
    public static readonly DeriveFault CapacityExhausted = Of<DeriveWitness.CapacityExhausted>(
        "capacity-exhausted", static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Operation), row.Instances > 0, row.Effort > Duration.Zero));
    public static readonly DeriveFault PredecessorLotMissing = Of<DeriveWitness.PredecessorLotMissing>(
        "predecessor-lot-missing", static row => row.Lot != UInt128.Zero);
    public static readonly DeriveFault SetupCoverage = Of<DeriveWitness.SetupCoverage>(
        "setup-coverage", static row => ValidityClaim.All(ValidityClaim.Nonnegative(row.Assigned), row.Required > 0, row.Assigned != row.Required));
    public static readonly DeriveFault AssemblyMismatch = Of<DeriveWitness.AssemblyMismatch>(
        "assembly-mismatch", static row => row.Component != UInt128.Zero);
    public static readonly DeriveFault AssemblyRequired = Of<DeriveWitness.AssemblyRequired>(
        "assembly-required", static row => row.Connections > 0);
    public static readonly DeriveFault JoinMeasureMissing = Of<DeriveWitness.JoinMeasureMissing>(
        "join-measure-missing", static row => ValidityClaim.Nonnegative(row.Joint));
    public static readonly DeriveFault IdentifierExhausted = Of<DeriveWitness.IdentifierExhausted>(
        "identifier-exhausted", static row => row.Requested > 0 && row.Next > (long)int.MaxValue - row.Requested);
    public static readonly DeriveFault OperationAbsent = Of<DeriveWitness.OperationAbsent>(
        "operation-absent", static row => ValidityClaim.Nonnegative(row.Id));
    public static readonly DeriveFault OperationsEmpty = Of<DeriveWitness.OperationsEmpty>(
        "operations-empty", static row => ValidityClaim.Nonnegative(row.Joints));
    public static readonly DeriveFault DemandInadmissible = Of<DeriveWitness.DemandInadmissible>(
        "demand-inadmissible", static row => ValidityClaim.Nonnegative(row.Id));

    public Func<DeriveWitness, bool> Admits { get; }

    private static DeriveFault Of<TWitness>(string key, Func<TWitness, bool> admits)
        where TWitness : DeriveWitness => new(key, Witness.Case<DeriveWitness, TWitness>(admits));
}

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
        "joint-limit", static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Joint), ValidityClaim.Finite([row.Position, row.Lower, row.Upper]), row.Lower < row.Upper,
            (row.Position < row.Lower || row.Position > row.Upper)));
    public static readonly JointFault Singularity = Of<JointDiagnostic.Singularity>(
        "singularity", static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Joint), ValidityClaim.Finite([row.ConditionNumber, row.Limit]), row.Limit > 0.0,
            row.ConditionNumber > row.Limit));
    public static readonly JointFault Reach = Of<JointDiagnostic.Reach>(
        "reach", static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Target), ValidityClaim.Finite([row.Distance, row.Limit]), row.Limit >= 0.0, row.Distance > row.Limit));
    public static readonly JointFault Collision = Of<JointDiagnostic.Collision>(
        "collision", static row => Touching(row.FirstLink, row.SecondLink, row.Clearance));
    public static readonly JointFault SelfCollision = Of<JointDiagnostic.SelfCollision>(
        "self-collision", static row => Touching(row.FirstLink, row.SecondLink, row.Clearance));
    public static readonly JointFault Velocity = Of<JointDiagnostic.Velocity>(
        "velocity", static row => Exceeded(row.Joint, row.Required, row.Limit));
    public static readonly JointFault Acceleration = Of<JointDiagnostic.Acceleration>(
        "acceleration", static row => Exceeded(row.Joint, row.Required, row.Limit));
    public static readonly JointFault Jerk = Of<JointDiagnostic.Jerk>(
        "jerk", static row => Exceeded(row.Joint, row.Required, row.Limit));
    public static readonly JointFault Torque = Of<JointDiagnostic.Torque>(
        "torque", static row => Exceeded(row.Joint, row.Required, row.Limit));
    public static readonly JointFault Configuration = Of<JointDiagnostic.Configuration>(
        "configuration", static row => Witness.Keyed(row.Requested) && Witness.Keyed(row.Admitted)
            && !string.Equals(row.Requested, row.Admitted, StringComparison.Ordinal));
    public static readonly JointFault Disconnected = Of<JointDiagnostic.Disconnected>(
        "disconnected", static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Link), row.ExpectedParent >= -1, row.ActualParent >= -1, row.ExpectedParent != row.ActualParent));
    public static readonly JointFault Provider = Of<JointDiagnostic.Provider>(
        "provider", static row => !row.Errors.IsEmpty && row.Errors.ForAll(Witness.Keyed));

    public Func<JointDiagnostic, bool> Admits { get; }

    private static JointFault Of<TDiagnostic>(string key, Func<TDiagnostic, bool> admits)
        where TDiagnostic : JointDiagnostic => new(key, Witness.Case<JointDiagnostic, TDiagnostic>(admits));

    private static bool Exceeded(int joint, double required, double limit) =>
        ValidityClaim.All(
            ValidityClaim.Nonnegative(joint), ValidityClaim.Finite([required, limit]), limit >= 0.0,
            Math.Abs(required) > limit);

    private static bool Touching(int firstLink, int secondLink, double clearance) =>
        Witness.Pair(firstLink, secondLink) && double.IsFinite(clearance) && clearance <= 0.0;
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
        "part-exceeds-stock", static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Part), ValidityClaim.Positive(row.PartAreaMm2), ValidityClaim.Positive(row.StockAreaMm2),
            row.PartAreaMm2 > row.StockAreaMm2));
    public static readonly NestFault HeterogeneousMassCut = Of<NestWitness.HeterogeneousMassCut>(
        "heterogeneous-mass-cut", static row => ValidityClaim.All(
            Witness.Pair(row.FirstPart, row.SecondPart), ValidityClaim.Positive(row.FirstMass), ValidityClaim.Positive(row.SecondMass),
            row.FirstMass != row.SecondMass));
    public static readonly NestFault MaterialMismatch = Of<NestWitness.MaterialMismatch>(
        "material-mismatch", static row => ValidityClaim.All(ValidityClaim.Nonnegative(row.Part), row.Required != row.Stock));
    public static readonly NestFault ThicknessMismatch = Of<NestWitness.ThicknessMismatch>(
        "thickness-mismatch", static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Part), ValidityClaim.Positive(row.RequiredMm), ValidityClaim.Positive(row.StockMm),
            row.RequiredMm != row.StockMm));
    public static readonly NestFault GrainIncompatible = Of<NestWitness.GrainIncompatible>(
        "grain-incompatible", static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Part), Angle(row.RequiredDeg), Angle(row.StockDeg), row.RequiredDeg != row.StockDeg));
    public static readonly NestFault InvalidQuantity = Of<NestWitness.InvalidQuantity>(
        "invalid-quantity", static row => ValidityClaim.All(ValidityClaim.Nonnegative(row.Part), row.Quantity <= 0));
    public static readonly NestFault RemnantUnavailable = Of<NestWitness.RemnantUnavailable>(
        "remnant-unavailable", static row => row.Remnant.Digest != UInt128.Zero);
    public static readonly NestFault Admission = Of<NestWitness.Admission>(
        "admission", static row => Witness.Keyed(row.Stage.Key) && row.Subject.ForAll(static value => ValidityClaim.Nonnegative(value).Holds)
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
    public static readonly SourceKind Profile = Of<SourceLocus.ProfileEntity>(
        "profile", static row => Witness.Keyed(row.Handle));
    public static readonly SourceKind Solid = Of<SourceLocus.SolidSource>("solid", static row => row.Digest != UInt128.Zero);
    public static readonly SourceKind Steel = Of<SourceLocus.DstvBlock>(
        "steel", static row => Witness.Keyed(row.Block) && row.Line > 0);
    public static readonly SourceKind Element = Of<SourceLocus.ElementNode>(
        "element", static row => row.NodeKey != UInt128.Zero);
    public static readonly SourceKind Mesh = Of<SourceLocus.MeshFace>("mesh", static row => ValidityClaim.Nonnegative(row.Face));

    public Func<SourceLocus, bool> Admits { get; }

    private static SourceKind Of<TLocus>(string key, Func<TLocus, bool> admits)
        where TLocus : SourceLocus => new(key, Witness.Case<SourceLocus, TLocus>(admits));
}

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
        "vanished", static row => ValidityClaim.Nonnegative(row.Region));
    public static readonly KerfKind Overlapped = Of<KerfWitness.Overlapped>(
        "overlapped", static row => Witness.Pair(row.First, row.Second));

    public Func<KerfWitness, bool> Admits { get; }

    private static KerfKind Of<TWitness>(string key, Func<TWitness, bool> admits)
        where TWitness : KerfWitness => new(key, Witness.Case<KerfWitness, TWitness>(admits));
}
```

## [03]-[WITNESS_EVIDENCE]

- Owner: `Witness` owns case-predicate lifting, the ONE refusal read, the admission fold, and the gated carrier, plus exactly the two predicates the kernel does not hold — a non-blank key and an ordered pair of distinct ordinals. Positivity, finiteness, and a nonnegative ordinal are `ValidityClaim` rows (`Rasm/Domain/rails`), so a kind row composes `ValidityClaim.All` over those rows rather than restating `double.IsFinite` or `value >= 0` on this page, and a bound stays movable at the kernel row alone. Each witness union owns its plane evidence — `JointDiagnostic` robot and machine, `NestWitness` nesting, `SourceLocus` ingress, `EquipmentWitness` tool and quantity admission, `DeriveWitness` plan derivation, `RelationFault` inadmissible Process-axis pairs, `KerfWitness` kerf topology, `FaultSubject` S0 references to upper-plane subjects.
- Cases: `JointDiagnostic` distinguishes joint bounds, singularity, reach, contact, rate limits, torque, configuration, disconnected chains, and opaque provider diagnostics, folding the four rate arms onto one `Rate` base and the two contact arms onto one `Contact` base so the shared predicate has one body. `NestWitness` distinguishes demand, fit, mass, material, thickness, grain, quantity, remnant, admission, search-budget, provider-proof, inventory-row, and lineage failures. `EquipmentWitness` distinguishes equipment geometry, spent life, head-physics refusal, one directed range violation, quantity text, and grade admission. `DeriveWitness` distinguishes component, topology, lot, capacity, setup, assembly, and identifier rejections.
- Entry: `Witness.Admit<TSelf, TKind>` is the one admission over every family; each union exposes it as its own `Admit` and supplies `WitnessKey` symbolically through `nameof`. `CollisionZone.Admit` and `VoxelBudget.Admit` ride the `Admission` bridge, so no hand ternary restates the generated `Validate` contract.
- Auto: `Refused` is the one verdict both `Admit` and `Carried` read, so the gated mint reuses the refusal admission already built rather than discarding it and constructing a second.
- Output: the concrete case and its evidence remain recoverable without parsing. Native write rejection and ingress unavailability retain provider text because each provider owns that taxonomy.
- Packages: `Rasm.Domain` (`Fault`, `ValidityClaim` rows and the `All` fold every kind predicate composes), RhinoCommon value geometry, `NodaTime.Instant` and `Duration` on lot evidence, Thinktecture.Runtime.Extensions, LanguageExt.Core, and BCL inbox compose directly.
- Boundary: a witness kind admits only its own payload type, so a cross-family pairing fails admission rather than reporting a foreign condition. A payload arrives from a generated union already non-null, so the contract carries no null arm and no reflected type tag.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
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

    public static FabricationFault Carried<TSelf, TKind, TState>(
        TSelf candidate,
        TState state,
        Func<TState, TSelf, FabricationFault> carrier)
        where TSelf : class, IWitness<TSelf, TKind>
        where TKind : IWitnessKind<TSelf> =>
        Refused<TSelf, TKind>(candidate).IfNone(() => carrier(state, candidate));

    public static bool Keyed(string value) => !string.IsNullOrWhiteSpace(value);
    public static bool Pair(int first, int second) =>
        ValidityClaim.All(ValidityClaim.Nonnegative(first), ValidityClaim.Nonnegative(second), first != second);

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
    public sealed record CapacityExhausted(int Operation, int Instances, Instant Ready, Duration Effort)
        : DeriveWitness(DeriveFault.CapacityExhausted);
    public sealed record PredecessorLotMissing(UInt128 Lot) : DeriveWitness(DeriveFault.PredecessorLotMissing);
    public sealed record SetupCoverage(int Assigned, int Required) : DeriveWitness(DeriveFault.SetupCoverage);
    public sealed record AssemblyMismatch(UInt128 Component) : DeriveWitness(DeriveFault.AssemblyMismatch);
    public sealed record AssemblyRequired(int Connections) : DeriveWitness(DeriveFault.AssemblyRequired);
    public sealed record JoinMeasureMissing(int Joint) : DeriveWitness(DeriveFault.JoinMeasureMissing);
    public sealed record IdentifierExhausted(long Next, int Requested) : DeriveWitness(DeriveFault.IdentifierExhausted);
    public sealed record OperationAbsent(int Id) : DeriveWitness(DeriveFault.OperationAbsent);
    public sealed record OperationsEmpty(int Joints) : DeriveWitness(DeriveFault.OperationsEmpty);
    public sealed record DemandInadmissible(int Id) : DeriveWitness(DeriveFault.DemandInadmissible);

    public static string WitnessKey => nameof(DeriveWitness);
    public static Fin<DeriveWitness> Admit(DeriveWitness candidate) => Witness.Admit<DeriveWitness, DeriveFault>(candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record JointDiagnostic(JointFault Kind) : IWitness<JointDiagnostic, JointFault> {
    public sealed record JointLimit(int Joint, double Position, double Lower, double Upper)
        : JointDiagnostic(JointFault.JointLimit);
    public sealed record Singularity(int Joint, double ConditionNumber, double Limit)
        : JointDiagnostic(JointFault.Singularity);
    public sealed record Reach(int Target, double Distance, double Limit)
        : JointDiagnostic(JointFault.Reach);
    public sealed record Collision(int FirstLink, int SecondLink, double Clearance)
        : JointDiagnostic(JointFault.Collision);
    public sealed record SelfCollision(int FirstLink, int SecondLink, double Clearance)
        : JointDiagnostic(JointFault.SelfCollision);
    public sealed record Velocity(int Joint, double Required, double Limit)
        : JointDiagnostic(JointFault.Velocity);
    public sealed record Acceleration(int Joint, double Required, double Limit)
        : JointDiagnostic(JointFault.Acceleration);
    public sealed record Jerk(int Joint, double Required, double Limit)
        : JointDiagnostic(JointFault.Jerk);
    public sealed record Torque(int Joint, double Required, double Limit)
        : JointDiagnostic(JointFault.Torque);
    public sealed record Configuration(string Requested, string Admitted)
        : JointDiagnostic(JointFault.Configuration);
    public sealed record Disconnected(int Link, int ExpectedParent, int ActualParent)
        : JointDiagnostic(JointFault.Disconnected);
    public sealed record Provider(Seq<string> Errors) : JointDiagnostic(JointFault.Provider);

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
    public sealed record ProfileEntity(string Handle) : SourceLocus(SourceKind.Profile);
    public sealed record SolidSource(UInt128 Digest) : SourceLocus(SourceKind.Solid);
    public sealed record DstvBlock(string Block, int Line) : SourceLocus(SourceKind.Steel);
    public sealed record ElementNode(UInt128 NodeKey) : SourceLocus(SourceKind.Element);
    public sealed record MeshFace(int Face) : SourceLocus(SourceKind.Mesh);

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
public sealed partial class CollisionZone {
    public ContentKey Key { get; }
    public BoundingBox Bounds { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ContentKey key,
        ref BoundingBox bounds) {
        if (!bounds.IsValid)
            validationError = new ValidationError("collision-zone");
    }

    public static Fin<CollisionZone> Admit(ContentKey key, BoundingBox bounds) =>
        Validate(key, bounds, out CollisionZone zone).Admitted(zone);
}

[ComplexValueObject]
public sealed partial class VoxelBudget {
    public BoundingBox Bounds { get; }
    public double VoxelSizeMm { get; }
    public long VoxelCap { get; }
    public long RequiredCells { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref BoundingBox bounds,
        ref double voxelSizeMm,
        ref long voxelCap,
        ref long requiredCells) {
        if (!bounds.IsValid || !ValidityClaim.Positive(voxelSizeMm).Holds
            || voxelCap <= 0 || requiredCells < 0 || requiredCells > voxelCap)
            validationError = new ValidationError("voxel-budget");
    }

    public static Fin<VoxelBudget> Admit(BoundingBox bounds, double voxelSizeMm, long voxelCap, long requiredCells) =>
        Validate(bounds, voxelSizeMm, voxelCap, requiredCells, out VoxelBudget budget).Admitted(budget);
}
```

## [04]-[FAULT_BAND]

- Owner: `FabricationFault` owns one direct closed union over `Fault`, one `FaultBand.Fabrication` binding, one explicit ordinal per leaf, its per-case `FabConcern`, and the witness-carrying mints every raise site reaches.
- Cases: `PolicyInadmissible` is the witness-free cross-plane arm every folder's own policy/parameter admission gate raises, threading its raising `FabConcern` the way `OpenLoop` does; `RunAbandoned` is its abandonment counterpart, threading the same slot and carrying the declared stage fraction and witness a withdrawn run reached; `FixtureInadmissible` seats the fixturing plane's refusal here, so the offset ledger stays whole on one page and no folder mints a same-named partial in a second namespace. A plane earns a case of its own only where the refusal carries evidence a caller ACTS on — a magazine seat and its occupant, the model and sample count a fit failed on, the station key and effort contention could not seat, the link an inadmissible chain breaks at, the word a parsed block never resolved, the axis an unfabricable geometry failed — and every remaining refusal answers on `PolicyInadmissible` with its own plane and locus.
- Entry: predicate-bearing witness families mint through `Witness.Carried`; the fixturing plane funnels its closed `FixturingWitness` payload through `FabricationFault.Fixture`. Witness-free cases lift directly into `Fin.Fail<T>`.
- Auto: the generator combines `FamilyBand` with each `[FaultCase]` ordinal into the cached numeric identity and rejects missing, duplicated, negative, or non-leaf annotations; span containment is the RUNTIME's, `FaultBand.Code(offset)` throwing outside the row's bound, because the generator reads no referenced `static readonly` band. Generated value admission uses the kernel default-validation bridge and never mints a fabrication fault from text.
- Law: recovery reads the concrete case through `Error.IsType<T>()` for the arm, `FaultBand.OwnerOf(BandKind.Fault, error.Code)` for band membership, and `Concern` for the owning plane — never a message substring. A cycle refusal carries the detecting walk's strongly-connected component members, never a vertex-and-edge count a caller cannot act on.
- Packages: `Rasm` (`[FaultCase]`/`Fault`/`FaultId`, `FaultBand.Fabrication`, and the default validation bridge), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`, `ValidationError`), LanguageExt.Core (`Error`/`Fin`/`Validation<Error, T>`/`Seq`/`Option`/`Unit`), RhinoCommon value geometry, NodaTime (`Instant`/`Duration` on lot evidence).
- Growth: a new fabrication failure is one `[FaultCase]` ordinal at the free offset and one case naming it and its owning `FabConcern`; higher-plane evidence crosses as the narrow matching `FaultSubject` case or that plane's own witness union, never as an upper-plane behavioural import.
- Boundary: band ownership derives from numeric `Code`, while `Concern` partitions the fabrication plane. No category, message-key, offset-column, or compatibility registry survives. A condition settled as a verdict remains a verdict and never re-mints as a fault.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Fabrication;
    private FabricationFault(FabConcern concern) => Concern = concern;

    public FabConcern Concern { get; }

    public sealed override string Message => Switch(
        noFit: static fault => $"Part {fault.Part} did not fit after rotations [{string.Join(", ", fault.TriedRotations)}].",
        unreachable: static fault => $"Kinematic target {fault.Target} is unreachable: {fault.Diag}.",
        kerfCollision: static fault => $"Kerf {fault.Kerf:R} conflicts with {fault.Witness}.",
        openLoop: static fault => $"{fault.Raised.Key} primitive {fault.Primitive} is open.",
        inadmissiblePair: static fault => $"Fabrication relation is inadmissible: {fault.Pair}.",
        gouge: static fault => $"Tool {fault.Tool} gouged the workpiece at {fault.Point}.",
        collision: static fault => $"{fault.Contact.Key} collides in zone {fault.Zone.Key}.",
        nonManifoldSlice: static fault => $"Layer {fault.Layer} has {fault.OpenChains} open slice chains.",
        stockOverflow: static fault => $"{fault.Unplaced} parts remain after filling {fault.Sheets} stock sheets.",
        nest: static fault => $"Nesting refused: {fault.Witness}.",
        ingressTranslation: static fault => $"Ingress translation failed at {fault.Locus}: {fault.Detail}.",
        machinabilityUnknown: static fault => $"Machinability is unknown for {fault.Material}/{fault.Op}.",
        sampleStalled: static fault => $"Sampling strategy {fault.Strategy.Key} stalled at iteration {fault.Iteration}.",
        axisSingularity: static fault =>
            $"Kinematic rank {fault.Rank}/{fault.RequiredRank} is singular at condition {fault.ConditionNumber:R}/{fault.MaximumConditionNumber:R}.",
        voxelFault: static fault => $"Voxel operation {fault.Op.Key} failed for budget {fault.Budget.RequiredCells}/{fault.Budget.VoxelCap}.",
        orientationInfeasible: static fault => $"All {fault.Rejected} of {fault.Candidates} build orientations were rejected.",
        setupInfeasible: static fault => $"Setup {fault.Operation} is infeasible after {fault.TriedSetups} candidates.",
        dialectUnsupported: static fault => $"Dialect {fault.Dialect} does not support program node {fault.Node.Key}.",
        programParse: static fault => $"Program line {fault.Line} violates modal group {fault.Group}.",
        probeOvertravel: static fault => $"Probe overtravel at {fault.At}; limit={fault.Limit:R}.",
        toleranceUnsatisfiable: static fault =>
            $"Tolerance frame {fault.Frame.Key} cannot achieve better than {fault.Achievable:R}.",
        capabilityShortfall: static fault =>
            $"Process {fault.Process} capability {fault.Cpk:R} is below demand {fault.Demanded:R}.",
        partitionDegenerate: static fault => $"Partition strategy {fault.Strategy.Key} degenerated across {fault.Sites} sites.",
        noToolForOp: static fault => $"No {fault.Required} tool admits operation {fault.Op}.",
        unsupportedThreeMfExtension: static fault =>
            $"3MF extension {fault.Extension.MediaType}/{fault.Extension.Key} is unsupported for {fault.Target}.",
        datumLineageBroken: static fault => $"Datum lineage {fault.Chain.Key} is broken.",
        clampOnMachinedFace: static fault => $"Operation {fault.Operation} clamps a machined face at {fault.At}.",
        blockCapExceeded: static fault => $"Dialect {fault.Dialect} emitted {fault.Blocks} blocks beyond cap {fault.Cap}.",
        stackupExceeded: static fault =>
            $"Tolerance stack {fault.Chain.Key} accumulated {fault.Accumulated:R} beyond {fault.Bound:R}.",
        routingInfeasible: static fault => $"Component {fault.ComponentKey:x32} cannot route through stage {fault.Stage.Key}.",
        wearEstimateUnfit: static fault => $"Tool {fault.Tool} wear estimate is unfit after {fault.Samples} samples.",
        wireTaperExceeded: static fault => $"Wire taper {fault.AngleDeg:R}° exceeds guide limit {fault.GuideLimitDeg:R}°.",
        linkBlocked: static fault => $"Toolpath link from {fault.From} to {fault.To} is blocked.",
        bevelUnsupported: static fault => $"Bevel {fault.Bevel.Key} does not support angle {fault.AngleDeg:R}°.",
        supportUnbuildable: static fault => $"Support region {fault.Region} on layer {fault.Layer} is unbuildable.",
        remnantStale: static fault => $"Remnant {fault.Key} is stale.",
        assemblyPrecedenceCyclic: static fault => $"Assembly precedence contains cycle [{string.Join(", ", fault.Cycle)}].",
        envelopeExceeded: static fault => $"Axis {fault.Axis} position {fault.At:R} exceeds envelope {fault.Limit:R}.",
        simulatedOvertravel: static fault => $"Block {fault.Block} overtravels axis {fault.Axis} by {fault.By:R}.",
        unfoldInfeasible: static fault => $"Unfolding {fault.Faces} faces exhausted {fault.Branches} branches.",
        bendSequenceInfeasible: static fault =>
            $"Bend sequencing rejected {fault.RejectedCandidates} candidates after {fault.ExpandedStates} states.",
        tonnageExceeded: static fault => $"Required tonnage {fault.RequiredKn:R} kN exceeds {fault.CapacityKn:R} kN.",
        minBendRadiusViolated: static fault =>
            $"Bend {fault.Bend} radius {fault.RadiusMm:R} mm is below {fault.FloorMm:R} mm.",
        weldAccessBlocked: static fault => $"Weld joint {fault.Joint} is blocked at torch angle {fault.TorchAngleDeg:R}°.",
        heatInputExceeded: static fault => $"Joint {fault.Joint} heat input {fault.KjPerMm:R} kJ/mm exceeds {fault.Cap:R}.",
        wpsUnqualified: static fault => $"WPS variable {fault.Variable.Key} is unqualified at {fault.Value:R}.",
        threeMfWriteRejected: static fault => $"{fault.Target} rejected the 3MF write: {fault.Native}.",
        unknownAxis: static fault => $"Axis {fault.Axis} is unknown for {fault.Key}.",
        ingressProviderUnavailable: static fault => $"Ingress provider is unavailable at {fault.Locus}: {fault.Detail}.",
        witnessMalformed: static fault => $"{fault.Witness} witness kind {fault.Kind} is malformed.",
        equipmentInadmissible: static fault => $"Equipment is inadmissible: {fault.Witness}.",
        derivationRejected: static fault => $"Derivation at {fault.Stage.Key} rejected {fault.Witness}.",
        bendSearchBudgetExceeded: static fault =>
            $"Bend search budget ended after {fault.ExpandedStates} expanded and {fault.PendingStates} pending states.",
        policyInadmissible: static fault => $"{fault.Raised.Key} policy is inadmissible at {fault.Locus}.",
        fixtureInadmissible: static fault => $"Fixture is inadmissible: {fault.Witness}.",
        runAbandoned: static fault => $"{fault.Raised.Key} run was abandoned at {fault.Done:P2}: {fault.Witness}.",
        toolSlotConflict: static fault =>
            $"Tool slot {fault.Slot} occupied by {fault.Occupant} cannot seat {fault.Requested}.",
        toolAssetInadmissible: static fault => $"Tool asset {fault.Subject} is inadmissible at {fault.Axis}.",
        cuttingModelUnfit: static fault =>
            $"Cutting model {fault.Model} is unfit for {fault.Material}/{fault.Op} after {fault.Samples} samples.",
        stabilityUnavailable: static fault =>
            $"Stability is unavailable at depth {fault.RequestedDepthMm:R} mm across {fault.Lobes} lobes.",
        machineInstanceUnavailable: static fault =>
            $"Machine {fault.Instance} is unavailable from {fault.From} for {fault.Effort}.",
        fleetAssignmentInfeasible: static fault =>
            $"Fleet cannot assign {fault.Demands} demands across {fault.Instances} instances.",
        kinematicChainInadmissible: static fault =>
            $"Cell {fault.Cell} has an inadmissible {fault.Axis} axis at link {fault.Link}.",
        programTokenUnresolved: static fault => $"Program token {fault.Word} on line {fault.Line} is unresolved.",
        optimizationRefused: static fault => $"Optimization pass {fault.Pass} refused at {fault.Locus}.",
        ingressGeometryUnfit: static fault => $"Ingress geometry at {fault.Locus} is unfit on {fault.Axis}.");

    [FaultCase(0)] public sealed partial record NoFit(int Part, Seq<double> TriedRotations) : FabricationFault(FabConcern.Nesting);
    [FaultCase(1)] public sealed partial record Unreachable(JointDiagnostic Diag, Option<int> Target) : FabricationFault(FabConcern.Kinematics);
    [FaultCase(2)] public sealed partial record KerfCollision(KerfWitness Witness, double Kerf) : FabricationFault(FabConcern.Nesting);
    [FaultCase(3)] public sealed partial record OpenLoop(FabConcern Raised, int Primitive) : FabricationFault(Raised);
    [FaultCase(4)] public sealed partial record InadmissiblePair(RelationFault Pair) : FabricationFault(FabConcern.Process);
    [FaultCase(5)] public sealed partial record Gouge(Point3d Point, CutterForm Tool) : FabricationFault(FabConcern.Toolpath);
    [FaultCase(6)] public sealed partial record Collision(CollisionZone Zone, CollisionContact Contact) : FabricationFault(FabConcern.Toolpath);
    [FaultCase(7)] public sealed partial record NonManifoldSlice(int Layer, int OpenChains) : FabricationFault(FabConcern.Additive);
    [FaultCase(8)] public sealed partial record StockOverflow(int Unplaced, int Sheets) : FabricationFault(FabConcern.Nesting);
    [FaultCase(9)] public sealed partial record Nest(NestWitness Witness) : FabricationFault(FabConcern.Nesting);
    [FaultCase(10)] public sealed partial record IngressTranslation(SourceLocus Locus, string Detail) : FabricationFault(FabConcern.Ingress);
    [FaultCase(11)] public sealed partial record MachinabilityUnknown(Material Material, Operation Op) : FabricationFault(FabConcern.Tooling);
    [FaultCase(12)] public sealed partial record SampleStalled(FaultSubject.Strategy Strategy, int Iteration) : FabricationFault(FabConcern.Toolpath);
    [FaultCase(13)] public sealed partial record AxisSingularity(
        int Rank, int RequiredRank, double ConditionNumber, double MaximumConditionNumber)
        : FabricationFault(FabConcern.Kinematics);
    [FaultCase(14)] public sealed partial record VoxelFault(
        FaultSubject.VoxelOperation Op, VoxelBudget Budget, Error Cause)
        : FabricationFault(FabConcern.Additive), ICausedFault;
    [FaultCase(15)] public sealed partial record OrientationInfeasible(int Candidates, int Rejected) : FabricationFault(FabConcern.Additive);
    [FaultCase(16)] public sealed partial record SetupInfeasible(Option<int> Operation, int TriedSetups) : FabricationFault(FabConcern.Fixturing);
    [FaultCase(17)] public sealed partial record DialectUnsupported(PostDialect Dialect, FaultSubject.ProgramNode Node) : FabricationFault(FabConcern.Posting);
    [FaultCase(18)] public sealed partial record ProgramParse(int Line, ModalGroup Group) : FabricationFault(FabConcern.Posting);
    [FaultCase(19)] public sealed partial record ProbeOvertravel(Point3d At, double Limit) : FabricationFault(FabConcern.Verify);
    [FaultCase(20)] public sealed partial record ToleranceUnsatisfiable(FaultSubject.Specification Frame, double Achievable) : FabricationFault(FabConcern.Spec);
    [FaultCase(21)] public sealed partial record CapabilityShortfall(ProcessKind Process, double Cpk, double Demanded) : FabricationFault(FabConcern.Spec);
    [FaultCase(22)] public sealed partial record PartitionDegenerate(FaultSubject.Partition Strategy, int Sites) : FabricationFault(FabConcern.Toolpath);
    [FaultCase(23)] public sealed partial record NoToolForOp(Operation Op, CutterForm Required) : FabricationFault(FabConcern.Tooling);
    [FaultCase(24)] public sealed partial record UnsupportedThreeMfExtension(FaultSubject.Extension Extension, EgressKind Target) : FabricationFault(FabConcern.Additive);
    [FaultCase(25)] public sealed partial record DatumLineageBroken(FaultSubject.Lineage Chain) : FabricationFault(FabConcern.Fixturing);
    [FaultCase(26)] public sealed partial record ClampOnMachinedFace(int Operation, Point3d At) : FabricationFault(FabConcern.Fixturing);
    [FaultCase(27)] public sealed partial record BlockCapExceeded(PostDialect Dialect, int Blocks, int Cap) : FabricationFault(FabConcern.Posting);
    [FaultCase(28)] public sealed partial record StackupExceeded(FaultSubject.Specification Chain, double Accumulated, double Bound) : FabricationFault(FabConcern.Spec);
    [FaultCase(29)] public sealed partial record RoutingInfeasible(UInt128 ComponentKey, FaultSubject.Stage Stage) : FabricationFault(FabConcern.Derivation);
    [FaultCase(30)] public sealed partial record WearEstimateUnfit(Tool Tool, int Samples) : FabricationFault(FabConcern.Tooling);
    [FaultCase(31)] public sealed partial record WireTaperExceeded(double AngleDeg, double GuideLimitDeg) : FabricationFault(FabConcern.Toolpath);
    [FaultCase(32)] public sealed partial record LinkBlocked(Point3d From, Point3d To) : FabricationFault(FabConcern.Toolpath);
    [FaultCase(33)] public sealed partial record BevelUnsupported(FaultSubject.Bevel Bevel, double AngleDeg) : FabricationFault(FabConcern.Toolpath);
    [FaultCase(34)] public sealed partial record SupportUnbuildable(int Layer, int Region) : FabricationFault(FabConcern.Additive);
    [FaultCase(35)] public sealed partial record RemnantStale(ContentKey Key) : FabricationFault(FabConcern.Nesting);
    [FaultCase(36)] public sealed partial record AssemblyPrecedenceCyclic(Arr<int> Cycle) : FabricationFault(FabConcern.Fixturing);
    [FaultCase(37)] public sealed partial record EnvelopeExceeded(MachineAxis Axis, double At, double Limit) : FabricationFault(FabConcern.Kinematics);
    [FaultCase(38)] public sealed partial record SimulatedOvertravel(int Block, MachineAxis Axis, double By) : FabricationFault(FabConcern.Verify);
    [FaultCase(39)] public sealed partial record UnfoldInfeasible(int Faces, int Branches) : FabricationFault(FabConcern.Forming);
    [FaultCase(40)] public sealed partial record BendSequenceInfeasible(int RejectedCandidates, int ExpandedStates) : FabricationFault(FabConcern.Forming);
    [FaultCase(41)] public sealed partial record TonnageExceeded(double RequiredKn, double CapacityKn) : FabricationFault(FabConcern.Forming);
    [FaultCase(42)] public sealed partial record MinBendRadiusViolated(int Bend, double RadiusMm, double FloorMm) : FabricationFault(FabConcern.Forming);
    [FaultCase(43)] public sealed partial record WeldAccessBlocked(int Joint, double TorchAngleDeg) : FabricationFault(FabConcern.Joining);
    [FaultCase(44)] public sealed partial record HeatInputExceeded(int Joint, double KjPerMm, double Cap) : FabricationFault(FabConcern.Joining);
    [FaultCase(45)] public sealed partial record WpsUnqualified(FaultSubject.Qualification Variable, double Value) : FabricationFault(FabConcern.Joining);
    [FaultCase(46)] public sealed partial record ThreeMfWriteRejected(EgressKind Target, string Native, Error Cause)
        : FabricationFault(FabConcern.Additive), ICausedFault;
    [FaultCase(47)] public sealed partial record UnknownAxis(string Axis, string Key) : FabricationFault(FabConcern.Process);
    [FaultCase(48)] public sealed partial record IngressProviderUnavailable(SourceLocus Locus, string Detail, Error Cause)
        : FabricationFault(FabConcern.Ingress), ICausedFault;
    [FaultCase(49)] public sealed partial record WitnessMalformed(string Witness, string Kind) : FabricationFault(FabConcern.Process);
    [FaultCase(50)] public sealed partial record EquipmentInadmissible(EquipmentWitness Witness) : FabricationFault(FabConcern.Process);
    [FaultCase(51)] public sealed partial record DerivationRejected(DeriveWitness Witness, FaultSubject.Stage Stage) : FabricationFault(FabConcern.Derivation);
    [FaultCase(52)] public sealed partial record BendSearchBudgetExceeded(int ExpandedStates, int PendingStates) : FabricationFault(FabConcern.Forming);
    [FaultCase(53)] public sealed partial record PolicyInadmissible(FabConcern Raised, string Locus) : FabricationFault(Raised);

    [FaultCase(54)] public sealed partial record FixtureInadmissible(FixturingWitness Witness) : FabricationFault(FabConcern.Fixturing);

    [FaultCase(55)] public sealed partial record RunAbandoned(FabConcern Raised, double Done, string Witness) : FabricationFault(Raised);

    [FaultCase(56)] public sealed partial record ToolSlotConflict(int Slot, Option<string> Occupant, string Requested) : FabricationFault(FabConcern.Tooling);
    [FaultCase(57)] public sealed partial record ToolAssetInadmissible(Option<string> Subject, string Axis) : FabricationFault(FabConcern.Tooling);
    [FaultCase(58)] public sealed partial record CuttingModelUnfit(Material Material, Operation Op, string Model, int Samples) : FabricationFault(FabConcern.Tooling);
    [FaultCase(59)] public sealed partial record StabilityUnavailable(double RequestedDepthMm, int Lobes) : FabricationFault(FabConcern.Tooling);

    [FaultCase(60)] public sealed partial record MachineInstanceUnavailable(MachineInstanceKey Instance, Instant From, Duration Effort) : FabricationFault(FabConcern.Fleet);
    [FaultCase(61)] public sealed partial record FleetAssignmentInfeasible(int Demands, int Instances) : FabricationFault(FabConcern.Fleet);

    [FaultCase(62)] public sealed partial record KinematicChainInadmissible(string Cell, int Link, string Axis) : FabricationFault(FabConcern.Kinematics);

    [FaultCase(63)] public sealed partial record ProgramTokenUnresolved(int Line, string Word) : FabricationFault(FabConcern.Posting);
    [FaultCase(64)] public sealed partial record OptimizationRefused(string Pass, string Locus) : FabricationFault(FabConcern.Posting);

    [FaultCase(65)] public sealed partial record IngressGeometryUnfit(SourceLocus Locus, string Axis) : FabricationFault(FabConcern.Ingress);

    public static FabricationFault Equipment(EquipmentWitness witness) =>
        Witness.Carried<EquipmentWitness, EquipmentFault, Unit>(
            witness, unit, static (_, row) => new EquipmentInadmissible(row));

    public static FabricationFault Derivation(DeriveWitness witness, FaultSubject.Stage stage) =>
        Witness.Carried<DeriveWitness, DeriveFault, FaultSubject.Stage>(
            witness, stage, static (at, row) => new DerivationRejected(row, at));

    public static FabricationFault Joint(JointDiagnostic diagnostic, Option<int> target) =>
        Witness.Carried<JointDiagnostic, JointFault, Option<int>>(
            diagnostic, target, static (at, row) => new Unreachable(row, at));

    public static FabricationFault Nested(NestWitness witness) =>
        Witness.Carried<NestWitness, NestFault, Unit>(witness, unit, static (_, row) => new Nest(row));

    public static FabricationFault Sourced(SourceLocus locus, string detail) =>
        Witness.Keyed(detail)
            ? Witness.Carried<SourceLocus, SourceKind, string>(
                locus, detail, static (state, row) => new IngressTranslation(row, state))
            : new WitnessMalformed(nameof(IngressTranslation), locus.Kind.Key);

    public static FabricationFault Unavailable(SourceLocus locus, string detail, Error cause) =>
        Witness.Carried<SourceLocus, SourceKind, (string Detail, Error Cause)>(
            locus, (detail, cause), static (state, row) => new IngressProviderUnavailable(row, state.Detail, state.Cause));

    public static FabricationFault Pairing(RelationFault pair) =>
        Witness.Carried<RelationFault, RelationKind, Unit>(pair, unit, static (_, row) => new InadmissiblePair(row));

    public static FabricationFault Kerf(KerfWitness witness, double kerf) =>
        Witness.Carried<KerfWitness, KerfKind, double>(
            witness, kerf, static (width, row) => new KerfCollision(row, width));

    public static FabricationFault Fixture(FixturingWitness witness) =>
        new FixtureInadmissible(witness);

    public static FabricationFault Inadmissible(FabConcern raised, string locus) => new PolicyInadmissible(raised, locus);

}
```

## [05]-[ADMISSION]

- Owner: `Admission` owns the ONE bridge from a generated owner's `Validate` contract to the `Fin` rail, and it sits on this page because the raise sites span every folder plane and `FabConcern` puts `Process` at stratum `0` beneath all of them, so each consumption edge points down. The accumulating combinators are NOT declared here: `Rasm.Element.Projection.AdmissionSlots` owns them for every package that rebinds a `FaultBand` row, `Gate(holds, refusal)` lifting a `FabricationFault` a prior fold already minted and holds as a value, `Gate(holds, concern, detail, refuse)` threading a plane and a locus into `FabricationFault.Inadmissible` on the failing arm alone, and `Accumulate(slots)` folding a slot run where the arity outruns the tuple `.Apply`.
- Entry: `Admitted(admitted)` closes a member-ordered `[ComplexValueObject]` admission over EITHER owner kind — the generator hands a reference owner back through a null-annotated slot and a value owner through the value slot itself, so one extension binds both — while `Admission.Of<TOwner, TRaw>` closes a keyed reference owner and `Admission.OfValue<TOwner, TRaw>` its `readonly partial struct` sibling. Constraints are not part of a signature, so the two keyed arities are two names rather than one overload pair. A raise site reaches `AdmissionSlots.Gate` for the one lift and `AdmissionSlots.Accumulate` for the one fold, joining a closed admission through the tuple `.Apply` directly and reaching the fold only past that arity.
- Auto: `Validate` returns null exactly when its `out` slot is populated, so one read decides both arms and the null-forgiving projection is the contract, not a guard. The slot answer is the concrete `Validation<Error, Unit>` — the lift IS a user-defined implicit conversion, which C# cannot target at an interface — and it upcasts to the `K<F,A>` the tuple `.Apply` receiver reads, `.As()` re-anchoring after each join, so a plane wrapper declaring that upcast as its own return feeds the fold's `K`-run arity with no re-cast; the fail arm target-types the bare `FabricationFault` through that lift, so `Fin.Succ(unit).ToValidation()`, `guard(…).ToFin().ToValidation()`, and `Validation<Error, Unit>.Success`/`.Fail` spelled at a raise site are all the deleted ceremony for one expression.
- Law: an accumulated refusal is the `ManyErrors` union of every violated invariant, each recoverable through `Error.IsType<T>()` exactly as a single refusal is — accumulation changes the arity of the evidence, never its taxonomy.
- Packages: `Rasm.Element` (`AdmissionSlots.Gate`/`.Accumulate`), LanguageExt.Core (`Validation<Error,_>`, `K<F,A>`, `Seq`, the `Apply` join and `Traverse` fold), Thinktecture.Runtime.Extensions (`IObjectFactory`, default `ValidationError`).
- Boundary: the slot mints NO fault — the raise site owns which band answers for its refusal, so the combinator carries no `Kind`, no locus prefix, and no `FabricationFault` case, and a package-local OR per-folder re-declaration of the lift or the fold is the named defect: the copy forks the accumulation law and its type name collides with the `Rasm.Element` owner under `CS0104` at every page importing both namespaces plainly. `Accumulate` folds a slot RUN while the tuple `.Apply` joins a fixed product — a raise site picks by the shape it already holds. A gate spelling `new KernelFault.InvalidValue("faults", locus)` inline builds a refusal the passing arm discards and takes the deferred arity against `FabricationFault.Inadmissible` instead; a `file static class` wrapping that spelling behind a two-argument `Of(admitted, locus)` re-spells the deferred arity under a per-plane name and is the deleted form. A boundary value enters through `Validate` or `TryCreate` and never through the throwing `Create`.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Admission {
    public static Fin<TOwner> Admitted<TOwner>(this ValidationError? refusal, TOwner admitted) =>
        refusal is null
            ? Fin.Succ(admitted)
            : Fin.Fail<TOwner>(new KernelFault.InvalidValue(typeof(TOwner).Name, refusal.Message));

    public static Fin<TOwner> Of<TOwner, TRaw>(TRaw value, IFormatProvider? provider = null)
        where TOwner : class, IObjectFactory<TOwner, TRaw, ValidationError>
        where TRaw : notnull =>
        TOwner.Validate(value, provider, out TOwner? admitted).Admitted(admitted!);

    public static Fin<TOwner> OfValue<TOwner, TRaw>(TRaw value, IFormatProvider? provider = null)
        where TOwner : struct, IObjectFactory<TOwner, TRaw, ValidationError>
        where TRaw : notnull =>
        TOwner.Validate(value, provider, out TOwner admitted).Admitted(admitted);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
