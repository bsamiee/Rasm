# [RASM_FABRICATION_FAULTS]

`FabricationFault` is the sole fabrication failure rail. Its closed `[Union]` derives `Rasm.Domain.Expected`, preserves `FaultBand.Fabrication + Offset` identity, and carries S0 evidence without importing a generator, posting, fixturing, specification, or additive owner.

Each witness family pairs a case-shaped payload with a `[SmartEnum<string>]` predicate vocabulary and admits through `Witness.Admit<TSelf, TKind>` over the `IWitness<TSelf, TKind>` contract. Contradictory payload-kind pairs cannot pass admission.

`FaultSubject` preserves higher-plane identity as S0 keys and content identities, while `RelationFault` retains native Process axes. Consumers inspect concrete cases through `Error.IsType<T>()`; no identity conversion or message parser exists.

`FabConcern` is the folder census: every case declares the sub-domain that owns its code and the stratum that plane occupies, so a receipt partitions faults by owning plane without a second table, and `OpenLoop` threads its caller's plane into the same slot. Degenerate geometry remains `GeometryFault.DegenerateInput`; every fabrication contract failure — equipment admission, witness malformation, derivation rejection included — uses the most specific `FabricationFault` case. Offsets `1` through `53` remain frozen and offset `54` is the next allocation.

Wire posture: HOST-LOCAL. `FabricationFault` rides `Fin<T>`, while frozen integer codes alone cross persistence receipts.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: `FabConcern`, `EquipmentFault`, `DeriveFault`, `SubjectKind`, `JointFault`, `CollisionContact`, `NestFault`, `SourceKind`, `AxisKind`, `MachineAxis`, `IWitnessKind`, `IWitness`, `Witness`, `FaultSubject`, `EquipmentWitness`, `DeriveWitness`, `CollisionZone`, `JointDiagnostic`, `NestWitness`, `VoxelBudget`, `SourceLocus`, `RelationFault`, `KerfWitness`, and `FabricationFault`.

## [02]-[FAULT_BAND]

- Owner: `FabricationFault` owns the closed band and its per-case `FabConcern` allocation, `MachineAxis` owns the machine-axis vocabulary every motion and posting plane addresses, `Witness` owns case-predicate lifting and the one admission fold, and each witness union owns its plane evidence — `JointDiagnostic` robot and machine, `NestWitness` nesting, `SourceLocus` ingress, `EquipmentWitness` tool and quantity admission, `DeriveWitness` plan derivation, `FaultSubject` S0 references to upper-plane subjects, `RelationFault` inadmissible Process-axis pairs.
- Cases: `JointDiagnostic` distinguishes joint bounds, singularity, reach, collision, rate limits, torque, self-collision, configuration, and disconnected chains. `NestWitness` distinguishes demand, fit, mass, material, thickness, grain, quantity, remnant, admission, search-budget, provider-proof, inventory-row, and lineage failures. `EquipmentWitness` distinguishes equipment geometry, spent life, head-physics refusal, range floor and ceiling, quantity text, and grade admission. `DeriveWitness` distinguishes component, topology, lot, setup, assembly, and identifier rejections. `MachineAxis` carries `AxisKind`, word `Address`, block `Order`, rotary `Wraps`, and the gantry `Companion` a duplicated axis pairs with.
- Entry: `Witness.Admit<TSelf, TKind>` is the one admission over every witness family; each union exposes it as its own `Admit` and supplies `WitnessKey` symbolically through `nameof`. `VoxelBudget.Create` admits nonnegative required cells within a finite cap. Concrete `FabricationFault` cases lift directly into `Fin.Fail<T>`.
- Auto: `Code` is `FaultBand.Fabrication + Offset`, `Message` is the invariant key, and `Category` is `Fabrication`. No second case-to-code or case-to-message sweep exists.
- Receipt: the concrete case and its evidence remain recoverable without parsing. Native write rejection and ingress unavailability retain provider text because each provider owns that taxonomy.
- Packages: `Rasm.Domain.Expected`, `FaultBand.Fabrication`, RhinoCommon value geometry, `NodaTime.Instant` on lot evidence, Thinktecture.Runtime.Extensions, LanguageExt.Core, and BCL inbox compose directly.
- Growth: a new fabrication failure is one case at the free offset carrying its owning `FabConcern`; a new witness kind is one predicate row with one payload case, admission and lowering untouched. Higher-plane evidence crosses as the narrow matching `FaultSubject` case, never as an upper-plane type import.
- Boundary: codes never renumber, keys never interpolate runtime values, and each payload retains the discriminants required for recovery. A witness kind admits only its own payload type, so a cross-family pairing fails admission rather than reporting a foreign condition. Process faults read no upper stratum.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rhino.Geometry;
using Thinktecture;
using Expected = Rasm.Domain.Expected;

namespace Rasm.Fabrication.Process;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FabConcern {
    public static readonly FabConcern Process = new("process", stratum: 0);
    public static readonly FabConcern Geometry2D = new("geometry-2d", stratum: 1);
    public static readonly FabConcern Ingress = new("ingress", stratum: 1);
    public static readonly FabConcern Kinematics = new("kinematics", stratum: 1);
    public static readonly FabConcern Tooling = new("tooling", stratum: 2);
    public static readonly FabConcern Nesting = new("nesting", stratum: 2);
    public static readonly FabConcern Additive = new("additive", stratum: 2);
    public static readonly FabConcern Fixturing = new("fixturing", stratum: 3);
    public static readonly FabConcern Forming = new("forming", stratum: 3);
    public static readonly FabConcern Joining = new("joining", stratum: 3);
    public static readonly FabConcern Spec = new("spec", stratum: 3);
    public static readonly FabConcern Toolpath = new("toolpath", stratum: 4);
    public static readonly FabConcern Posting = new("posting", stratum: 5);
    public static readonly FabConcern Verify = new("verify", stratum: 5);
    public static readonly FabConcern Documentation = new("documentation", stratum: 5);

    public int Stratum { get; }
}

[SmartEnum<string>]
public sealed partial class EquipmentFault : IWitnessKind<EquipmentWitness> {
    public static readonly EquipmentFault Geometry = Of<EquipmentWitness.Geometry>(
        "geometry", static row => row.Candidate is not null && !string.IsNullOrWhiteSpace(row.Axis));
    public static readonly EquipmentFault Spent = Of<EquipmentWitness.Spent>(
        "spent", static row => row.Identity != UInt128.Zero);
    public static readonly EquipmentFault HeadPhysics = Of<EquipmentWitness.HeadPhysics>(
        "head-physics", static row => row.Required is not null
            && row.Mounted.Match(head => !head.Admits(row.Required), static () => true));
    public static readonly EquipmentFault RangeFloor = Of<EquipmentWitness.RangeFloor>(
        "range-floor", static row => Finite(row.Derived, row.Floor) && row.Floor > row.Derived);
    public static readonly EquipmentFault RangeCeiling = Of<EquipmentWitness.RangeCeiling>(
        "range-ceiling", static row => Finite(row.Derived, row.Ceiling) && row.Derived > row.Ceiling);
    public static readonly EquipmentFault Quantity = Of<EquipmentWitness.Quantity>(
        "quantity", static row => !string.IsNullOrWhiteSpace(row.Kind));
    public static readonly EquipmentFault Grade = Of<EquipmentWitness.Grade>(
        "grade", static row => !string.IsNullOrWhiteSpace(row.Locus));

    public Func<EquipmentWitness, bool> Admits { get; }

    private static EquipmentFault Of<TWitness>(string key, Func<TWitness, bool> admits)
        where TWitness : EquipmentWitness =>
        new(key, Witness.Case<EquipmentWitness, TWitness>(admits));

    private static bool Finite(double first, double second) => double.IsFinite(first) && double.IsFinite(second);
}

[SmartEnum<string>]
public sealed partial class DeriveFault : IWitnessKind<DeriveWitness> {
    public static readonly DeriveFault ComponentMismatch = Of<DeriveWitness.ComponentMismatch>(
        "component-mismatch", static row => row.Requested != row.Assessed);
    public static readonly DeriveFault OperationCycle = Of<DeriveWitness.OperationCycle>(
        "operation-cycle", static row => row.Operations > 0 && row.Edges > 0);
    public static readonly DeriveFault DuplicateOperation = Of<DeriveWitness.DuplicateOperation>(
        "duplicate-operation", static row => row.Id >= 0);
    public static readonly DeriveFault UnknownPredecessor = Of<DeriveWitness.UnknownPredecessor>(
        "unknown-predecessor", static row => row.Operation >= 0 && row.Predecessor >= 0
            && row.Operation != row.Predecessor);
    public static readonly DeriveFault LotInadmissible = Of<DeriveWitness.LotInadmissible>(
        "lot-inadmissible", static row => row.Quantity < 1 || row.BatchSize < 1 || row.BatchSize > row.Quantity
            || row.Due < row.Release || row.TransferBuffer < Duration.Zero
            || row.Predecessors.Distinct().Count != row.Predecessors.Count || row.Predecessors.Contains(UInt128.Zero));
    public static readonly DeriveFault LotOverdue = Of<DeriveWitness.LotOverdue>(
        "lot-overdue", static row => row.Completion > row.Due);
    public static readonly DeriveFault LotUnschedulable = Of<DeriveWitness.LotUnschedulable>(
        "lot-unschedulable", static row => row.Operation >= 0 && row.Effort > Duration.Zero);
    public static readonly DeriveFault PredecessorLotMissing = Of<DeriveWitness.PredecessorLotMissing>(
        "predecessor-lot-missing", static row => row.Lot != UInt128.Zero);
    public static readonly DeriveFault SetupCoverage = Of<DeriveWitness.SetupCoverage>(
        "setup-coverage", static row => row.Assigned >= 0 && row.Required > 0 && row.Assigned != row.Required);
    public static readonly DeriveFault AssemblyMismatch = Of<DeriveWitness.AssemblyMismatch>(
        "assembly-mismatch", static row => row.Component != UInt128.Zero);
    public static readonly DeriveFault AssemblyRequired = Of<DeriveWitness.AssemblyRequired>(
        "assembly-required", static row => row.Connections > 0);
    public static readonly DeriveFault JoinReceiptMissing = Of<DeriveWitness.JoinReceiptMissing>(
        "join-receipt-missing", static row => row.Joint >= 0);
    public static readonly DeriveFault IdentifierExhausted = Of<DeriveWitness.IdentifierExhausted>(
        "identifier-exhausted", static row => row.Requested > 0
            && row.Next > (long)int.MaxValue - row.Requested);
    public static readonly DeriveFault OperationAbsent = Of<DeriveWitness.OperationAbsent>(
        "operation-absent", static row => row.Id >= 0);
    public static readonly DeriveFault OperationsEmpty = Of<DeriveWitness.OperationsEmpty>(
        "operations-empty", static row => row.Joints >= 0);
    public static readonly DeriveFault DemandInadmissible = Of<DeriveWitness.DemandInadmissible>(
        "demand-inadmissible", static row => row.Id >= 0);

    public Func<DeriveWitness, bool> Admits { get; }

    private static DeriveFault Of<TWitness>(string key, Func<TWitness, bool> admits)
        where TWitness : DeriveWitness =>
        new(key, Witness.Case<DeriveWitness, TWitness>(admits));
}

[SmartEnum<string>]
public sealed partial class SubjectKind : IWitnessKind<FaultSubject> {
    public static readonly SubjectKind Strategy = Of<FaultSubject.Strategy>("strategy", static row => Keyed(row.Key));
    public static readonly SubjectKind VoxelOperation = Of<FaultSubject.VoxelOperation>("voxel-operation", static row => Keyed(row.Key));
    public static readonly SubjectKind ProgramNode = Of<FaultSubject.ProgramNode>("program-node", static row => Keyed(row.Key));
    public static readonly SubjectKind Specification = Of<FaultSubject.Specification>("specification", static row => row.Key.Digest != UInt128.Zero);
    public static readonly SubjectKind Partition = Of<FaultSubject.Partition>("partition", static row => Keyed(row.Key));
    public static readonly SubjectKind Extension = Of<FaultSubject.Extension>(
        "extension", static row => Keyed(row.MediaType) && Keyed(row.Key));
    public static readonly SubjectKind Lineage = Of<FaultSubject.Lineage>("lineage", static row => row.Key.Digest != UInt128.Zero);
    public static readonly SubjectKind Stage = Of<FaultSubject.Stage>("stage", static row => Keyed(row.Key));
    public static readonly SubjectKind Bevel = Of<FaultSubject.Bevel>("bevel", static row => Keyed(row.Key));
    public static readonly SubjectKind Qualification = Of<FaultSubject.Qualification>("qualification", static row => Keyed(row.Key));

    public Func<FaultSubject, bool> Admits { get; }

    private static SubjectKind Of<TSubject>(string key, Func<TSubject, bool> admits)
        where TSubject : FaultSubject =>
        new(key, Witness.Case<FaultSubject, TSubject>(admits));

    private static bool Keyed(string value) => !string.IsNullOrWhiteSpace(value);
}

[SmartEnum<string>]
public sealed partial class JointFault : IWitnessKind<JointDiagnostic> {
    public static readonly JointFault JointLimit = Of<JointDiagnostic.JointLimit>(
        "joint-limit", static row => Index(row.Joint) && Finite(row.Position, row.Lower, row.Upper)
            && row.Lower < row.Upper && (row.Position < row.Lower || row.Position > row.Upper));
    public static readonly JointFault Singularity = Of<JointDiagnostic.Singularity>(
        "singularity", static row => Index(row.Joint) && Finite(row.ConditionNumber, row.Limit)
            && row.Limit > 0.0 && row.ConditionNumber > row.Limit);
    public static readonly JointFault Reach = Of<JointDiagnostic.Reach>(
        "reach", static row => Index(row.Target) && Finite(row.Distance, row.Limit)
            && row.Limit >= 0.0 && row.Distance > row.Limit);
    public static readonly JointFault Collision = Of<JointDiagnostic.Collision>(
        "collision", static row => Pair(row.FirstLink, row.SecondLink)
            && double.IsFinite(row.Clearance) && row.Clearance <= 0.0);
    public static readonly JointFault Velocity = Of<JointDiagnostic.Velocity>(
        "velocity", static row => Exceeded(row.Joint, row.Required, row.Limit));
    public static readonly JointFault Acceleration = Of<JointDiagnostic.Acceleration>(
        "acceleration", static row => Exceeded(row.Joint, row.Required, row.Limit));
    public static readonly JointFault Jerk = Of<JointDiagnostic.Jerk>(
        "jerk", static row => Exceeded(row.Joint, row.Required, row.Limit));
    public static readonly JointFault Torque = Of<JointDiagnostic.Torque>(
        "torque", static row => Exceeded(row.Joint, row.Required, row.Limit));
    public static readonly JointFault SelfCollision = Of<JointDiagnostic.SelfCollision>(
        "self-collision", static row => Pair(row.FirstLink, row.SecondLink)
            && double.IsFinite(row.Clearance) && row.Clearance <= 0.0);
    public static readonly JointFault Configuration = Of<JointDiagnostic.Configuration>(
        "configuration", static row => !string.IsNullOrWhiteSpace(row.Requested)
            && !string.IsNullOrWhiteSpace(row.Admitted)
            && !string.Equals(row.Requested, row.Admitted, StringComparison.Ordinal));
    public static readonly JointFault Disconnected = Of<JointDiagnostic.Disconnected>(
        "disconnected", static row => Index(row.Link) && row.ExpectedParent >= -1
            && row.ActualParent >= -1 && row.ExpectedParent != row.ActualParent);

    public Func<JointDiagnostic, bool> Admits { get; }

    private static JointFault Of<TDiagnostic>(string key, Func<TDiagnostic, bool> admits)
        where TDiagnostic : JointDiagnostic =>
        new(key, Witness.Case<JointDiagnostic, TDiagnostic>(admits));

    private static bool Exceeded(int joint, double required, double limit) =>
        Index(joint) && Finite(required, limit) && limit >= 0.0 && Math.Abs(required) > limit;

    private static bool Pair(int first, int second) => Index(first) && Index(second) && first != second;
    private static bool Index(int value) => value >= 0;
    private static bool Finite(double first, double second) => double.IsFinite(first) && double.IsFinite(second);
    private static bool Finite(double first, double second, double third) =>
        double.IsFinite(first) && double.IsFinite(second) && double.IsFinite(third);
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
public sealed partial class NestFault : IWitnessKind<NestWitness> {
    public static readonly NestFault EmptyCutList = Of<NestWitness.EmptyCutList>("empty-cut-list", static _ => true);
    public static readonly NestFault PartExceedsStock = Of<NestWitness.PartExceedsStock>(
        "part-exceeds-stock", static row => row.Part >= 0 && Positive(row.PartAreaMm2)
            && Positive(row.StockAreaMm2) && row.PartAreaMm2 > row.StockAreaMm2);
    public static readonly NestFault HeterogeneousMassCut = Of<NestWitness.HeterogeneousMassCut>(
        "heterogeneous-mass-cut", static row => row.FirstPart >= 0 && row.SecondPart >= 0
            && row.FirstPart != row.SecondPart && Positive(row.FirstMass) && Positive(row.SecondMass)
            && row.FirstMass != row.SecondMass);
    public static readonly NestFault MaterialMismatch = Of<NestWitness.MaterialMismatch>(
        "material-mismatch", static row => row.Part >= 0 && row.Required is not null
            && row.Stock is not null && row.Required != row.Stock);
    public static readonly NestFault ThicknessMismatch = Of<NestWitness.ThicknessMismatch>(
        "thickness-mismatch", static row => row.Part >= 0 && Positive(row.RequiredMm)
            && Positive(row.StockMm) && row.RequiredMm != row.StockMm);
    public static readonly NestFault GrainIncompatible = Of<NestWitness.GrainIncompatible>(
        "grain-incompatible", static row => row.Part >= 0 && Angle(row.RequiredDeg)
            && Angle(row.StockDeg) && row.RequiredDeg != row.StockDeg);
    public static readonly NestFault InvalidQuantity = Of<NestWitness.InvalidQuantity>(
        "invalid-quantity", static row => row.Part >= 0 && row.Quantity <= 0);
    public static readonly NestFault RemnantUnavailable = Of<NestWitness.RemnantUnavailable>(
        "remnant-unavailable", static row => row.Remnant is not null);
    public static readonly NestFault Admission = Of<NestWitness.Admission>(
        "admission", static row => row.Stage is not null && Keyed(row.Stage.Key)
            && row.Subject.ForAll(static value => value >= 0) && Keyed(row.Detail));
    public static readonly NestFault StrategyBudget = Of<NestWitness.StrategyBudget>(
        "strategy-budget", static row => row.Strategy is not null && Keyed(row.Strategy.Key)
            && row.Visited >= 0 && row.Pending >= 0 && row.Depth >= 0
            && (row.CountBudget < 1 || row.DepthBudget < 0 || row.Visited > row.CountBudget
                || row.Pending > row.CountBudget || row.Depth > row.DepthBudget));
    public static readonly NestFault ProviderProof = Of<NestWitness.ProviderProof>(
        "provider-proof", static row => row.Strategy is not null && Keyed(row.Strategy.Key)
            && row.Placements >= 0 && (!row.Contained || !row.Disjoint));
    public static readonly NestFault InventoryRow = Of<NestWitness.InventoryRow>(
        "inventory-row", static row => row.Key is not null && row.ClaimCap >= 0
            && (row.Key.Kind != EgressKind.Remnant || row.Revision < 0 || row.Claims < 0
                || row.Claims > row.ClaimCap || !row.IdentityMatches || !row.MaterialMatches
                || !row.Lifecycle || !row.Lease || !row.Profile || !row.Usable));
    public static readonly NestFault Lineage = Of<NestWitness.Lineage>(
        "lineage", static row => row.Rows >= 0 && row.Indexed >= 0 && row.Ordered >= 0
            && (row.Indexed != row.Rows || row.Ordered != row.Rows || !row.ParentsResolved
                || !row.Acyclic || !row.Successive || !row.Rooted));

    public Func<NestWitness, bool> Admits { get; }

    private static NestFault Of<TWitness>(string key, Func<TWitness, bool> admits)
        where TWitness : NestWitness =>
        new(key, Witness.Case<NestWitness, TWitness>(admits));

    private static bool Positive(double value) => double.IsFinite(value) && value > 0.0;
    private static bool Angle(double value) => double.IsFinite(value) && value is >= -360.0 and <= 360.0;
    private static bool Keyed(string value) => !string.IsNullOrWhiteSpace(value);
}

[SmartEnum<string>]
public sealed partial class SourceKind : IWitnessKind<SourceLocus> {
    public static readonly SourceKind Profile = Of<SourceLocus.DxfEntity>(
        "profile", static row => !string.IsNullOrWhiteSpace(row.Handle));
    public static readonly SourceKind Solid = Of<SourceLocus.OcctShape>("solid", static row => row.Id >= 0);
    public static readonly SourceKind Steel = Of<SourceLocus.DstvBlock>(
        "steel", static row => !string.IsNullOrWhiteSpace(row.Block) && row.Line > 0);
    public static readonly SourceKind Element = Of<SourceLocus.ElementNode>(
        "element", static row => row.NodeKey != UInt128.Zero);
    public static readonly SourceKind ThreeMf = Of<SourceLocus.ThreeMfObject>(
        "3mf", static row => !string.IsNullOrWhiteSpace(row.Model) && row.ObjectId >= 0);
    public static readonly SourceKind Rhino = Of<SourceLocus.RhinoObject>(
        "rhino", static row => row.ObjectId != Guid.Empty);
    public static readonly SourceKind Mesh = Of<SourceLocus.MeshFace>("mesh", static row => row.Face >= 0);
    public static readonly SourceKind Program = Of<SourceLocus.ProgramBlock>("program", static row => row.Block >= 0);
    public static readonly SourceKind Exchange = Of<SourceLocus.ExchangeEntity>(
        "exchange", static row => !string.IsNullOrWhiteSpace(row.Scheme) && !string.IsNullOrWhiteSpace(row.Entity));

    public Func<SourceLocus, bool> Admits { get; }

    private static SourceKind Of<TLocus>(string key, Func<TLocus, bool> admits)
        where TLocus : SourceLocus =>
        new(key, Witness.Case<SourceLocus, TLocus>(admits));
}

[SmartEnum<string>]
public sealed partial class AxisKind {
    public static readonly AxisKind Linear = new("linear");
    public static readonly AxisKind Rotary = new("rotary");
    public static readonly AxisKind Spindle = new("spindle");
    public static readonly AxisKind Auxiliary = new("auxiliary");
}

[SmartEnum<string>]
public sealed partial class MachineAxis {
    public static readonly MachineAxis X = new("x", AxisKind.Linear, address: 'X', order: 0);
    public static readonly MachineAxis Y = new("y", AxisKind.Linear, address: 'Y', order: 1);
    public static readonly MachineAxis Z = new("z", AxisKind.Linear, address: 'Z', order: 2);
    public static readonly MachineAxis A = new("a", AxisKind.Rotary, address: 'A', order: 3, wraps: true);
    public static readonly MachineAxis B = new("b", AxisKind.Rotary, address: 'B', order: 4, wraps: true);
    public static readonly MachineAxis C = new("c", AxisKind.Rotary, address: 'C', order: 5, wraps: true);
    public static readonly MachineAxis U = new("u", AxisKind.Linear, address: 'U', order: 6);
    public static readonly MachineAxis V = new("v", AxisKind.Linear, address: 'V', order: 7);
    public static readonly MachineAxis W = new("w", AxisKind.Linear, address: 'W', order: 8);
    public static readonly MachineAxis R = new("r", AxisKind.Linear, address: 'R', order: 9);
    public static readonly MachineAxis Y1 = new("y1", AxisKind.Linear, address: 'Y', order: 1, companion: "y2");
    public static readonly MachineAxis Y2 = new("y2", AxisKind.Linear, address: 'Y', order: 1, companion: "y1");
    public static readonly MachineAxis Z1 = new("z1", AxisKind.Linear, address: 'Z', order: 2, companion: "z2");
    public static readonly MachineAxis Z2 = new("z2", AxisKind.Linear, address: 'Z', order: 2, companion: "z1");
    public static readonly MachineAxis S1 = new("s1", AxisKind.Spindle, address: 'S', order: 10);
    public static readonly MachineAxis S2 = new("s2", AxisKind.Spindle, address: 'S', order: 11);
    public static readonly MachineAxis J1 = new("j1", AxisKind.Rotary, address: 'J', order: 20, wraps: true);
    public static readonly MachineAxis J2 = new("j2", AxisKind.Rotary, address: 'J', order: 21, wraps: true);
    public static readonly MachineAxis J3 = new("j3", AxisKind.Rotary, address: 'J', order: 22, wraps: true);
    public static readonly MachineAxis J4 = new("j4", AxisKind.Rotary, address: 'J', order: 23, wraps: true);
    public static readonly MachineAxis J5 = new("j5", AxisKind.Rotary, address: 'J', order: 24, wraps: true);
    public static readonly MachineAxis J6 = new("j6", AxisKind.Rotary, address: 'J', order: 25, wraps: true);
    public static readonly MachineAxis J7 = new("j7", AxisKind.Rotary, address: 'J', order: 26, wraps: true);
    public static readonly MachineAxis E1 = new("e1", AxisKind.Auxiliary, address: 'E', order: 30);
    public static readonly MachineAxis E2 = new("e2", AxisKind.Auxiliary, address: 'E', order: 31);

    public AxisKind Kind { get; }
    public char Address { get; }
    public int Order { get; }
    public bool Wraps { get; }
    public string? Companion { get; }

    public bool Rotary => Kind == AxisKind.Rotary;
    public Option<MachineAxis> Paired => Optional(Companion).Bind(static key => TryGet(key, out MachineAxis? axis) ? Some(axis!) : None);
}

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
    internal static Func<TWitness, bool> Case<TWitness, TCase>(Func<TCase, bool> admits)
        where TCase : TWitness =>
        witness => witness is TCase typed && admits(typed);

    public static Fin<TSelf> Admit<TSelf, TKind>(TSelf candidate)
        where TSelf : class, IWitness<TSelf, TKind>
        where TKind : IWitnessKind<TSelf> => Optional(candidate)
        .ToFin(FabricationFault.WitnessMalformed(TSelf.WitnessKey, typeof(TKind).Name))
        .Bind(admitted => admitted.Kind.Admits(admitted)
            ? Fin.Succ(admitted)
            : Fin.Fail<TSelf>(FabricationFault.WitnessMalformed(TSelf.WitnessKey, admitted.Kind.Key)));
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
    public sealed record RangeFloor(PhysicsQuantity Bound, double Derived, double Floor) : EquipmentWitness(EquipmentFault.RangeFloor);
    public sealed record RangeCeiling(PhysicsQuantity Bound, double Derived, double Ceiling) : EquipmentWitness(EquipmentFault.RangeCeiling);
    public sealed record Quantity(string Kind, string Text) : EquipmentWitness(EquipmentFault.Quantity);
    public sealed record Grade(string Locus) : EquipmentWitness(EquipmentFault.Grade);

    public static string WitnessKey => nameof(EquipmentWitness);
    public static Fin<EquipmentWitness> Admit(EquipmentWitness candidate) => Witness.Admit<EquipmentWitness, EquipmentFault>(candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeriveWitness(DeriveFault Kind) : IWitness<DeriveWitness, DeriveFault> {
    public sealed record ComponentMismatch(UInt128 Requested, UInt128 Assessed) : DeriveWitness(DeriveFault.ComponentMismatch);
    public sealed record OperationCycle(int Operations, int Edges) : DeriveWitness(DeriveFault.OperationCycle);
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
    public sealed record Velocity(int Joint, double Required, double Limit)
        : JointDiagnostic(JointFault.Velocity);
    public sealed record Acceleration(int Joint, double Required, double Limit)
        : JointDiagnostic(JointFault.Acceleration);
    public sealed record Jerk(int Joint, double Required, double Limit)
        : JointDiagnostic(JointFault.Jerk);
    public sealed record Torque(int Joint, double Required, double Limit)
        : JointDiagnostic(JointFault.Torque);
    public sealed record SelfCollision(int FirstLink, int SecondLink, double Clearance)
        : JointDiagnostic(JointFault.SelfCollision);
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
    public sealed record InventoryRow(
        ContentKey Key,
        int Revision,
        int Claims,
        int ClaimCap,
        bool IdentityMatches,
        bool MaterialMatches,
        bool Lifecycle,
        bool Lease,
        bool Profile,
        bool Usable) : NestWitness(NestFault.InventoryRow);
    public sealed record Lineage(
        int Rows,
        int Indexed,
        int Ordered,
        bool ParentsResolved,
        bool Acyclic,
        bool Successive,
        bool Rooted) : NestWitness(NestFault.Lineage);

    public static string WitnessKey => nameof(NestWitness);
    public static Fin<NestWitness> Admit(NestWitness candidate) => Witness.Admit<NestWitness, NestFault>(candidate);
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
        if (!bounds.IsValid || !double.IsFinite(voxelSizeMm) || voxelSizeMm <= 0.0
            || voxelCap <= 0 || requiredCells < 0 || requiredCells > voxelCap)
            validationError = new ValidationError("voxel-budget");
    }
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
public abstract partial record RelationFault {
    private RelationFault() { }

    public sealed record ModalityStrategy(ProcessModality Modality, CutStrategy Strategy) : RelationFault;
    public sealed record ProcessMachine(ProcessKind Process, Machine Machine) : RelationFault;
    public sealed record DialectModality(PostDialect Dialect, ProcessModality Modality) : RelationFault;
    public sealed record OperationEquipment(Operation Operation, Tool Equipment) : RelationFault;
    public sealed record ProcessMaterial(ProcessKind Process, Material Material) : RelationFault;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KerfWitness {
    private KerfWitness() { }

    public sealed record Vanished(int Region) : KerfWitness;
    public sealed record Overlapped(int First, int Second) : KerfWitness;
}

// --- [ERRORS] -------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationFault(int Offset, string MessageKey, FabConcern Concern)
    : Expected(MessageKey, FaultBand.Fabrication + Offset) {
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
    public sealed record Unsupported3mfExtension(FaultSubject.Extension Extension, EgressKind Target) : FabricationFault(25, "fabrication:unsupported-3mf-extension", FabConcern.Additive);
    public sealed record DatumLineageBroken(FaultSubject.Lineage Chain) : FabricationFault(26, "fabrication:datum-lineage-broken", FabConcern.Fixturing);
    public sealed record ClampOnMachinedFace(int Operation, Point3d At) : FabricationFault(27, "fabrication:clamp-on-machined-face", FabConcern.Fixturing);
    public sealed record BlockCapExceeded(PostDialect Dialect, int Blocks, int Cap) : FabricationFault(28, "fabrication:block-cap-exceeded", FabConcern.Posting);
    public sealed record StackupExceeded(FaultSubject.Specification Chain, double Accumulated, double Bound) : FabricationFault(29, "fabrication:stackup-exceeded", FabConcern.Spec);
    public sealed record RoutingInfeasible(UInt128 ComponentKey, FaultSubject.Stage Stage) : FabricationFault(30, "fabrication:routing-infeasible", FabConcern.Process);
    public sealed record WearEstimateUnfit(Tool Tool, int Samples) : FabricationFault(31, "fabrication:wear-estimate-unfit", FabConcern.Tooling);
    public sealed record WireTaperExceeded(double AngleDeg, double GuideLimitDeg) : FabricationFault(32, "fabrication:wire-taper-exceeded", FabConcern.Toolpath);
    public sealed record LinkBlocked(Point3d From, Point3d To) : FabricationFault(33, "fabrication:link-blocked", FabConcern.Toolpath);
    public sealed record BevelUnsupported(FaultSubject.Bevel Bevel, double AngleDeg) : FabricationFault(34, "fabrication:bevel-unsupported", FabConcern.Toolpath);
    public sealed record SupportUnbuildable(int Layer, int Region) : FabricationFault(35, "fabrication:support-unbuildable", FabConcern.Additive);
    public sealed record RemnantStale(ContentKey Key) : FabricationFault(36, "fabrication:remnant-stale", FabConcern.Nesting);
    public sealed record AssemblyPrecedenceCyclic(int Joints, int Edges) : FabricationFault(37, "fabrication:assembly-precedence-cyclic", FabConcern.Fixturing);
    public sealed record EnvelopeExceeded(MachineAxis Axis, double At, double Limit) : FabricationFault(38, "fabrication:envelope-exceeded", FabConcern.Kinematics);
    public sealed record SimulatedOvertravel(int Block, MachineAxis Axis, double By) : FabricationFault(39, "fabrication:simulated-overtravel", FabConcern.Verify);
    public sealed record UnfoldInfeasible(int Faces, int Branches) : FabricationFault(40, "fabrication:unfold-infeasible", FabConcern.Forming);
    public sealed record BendSequenceInfeasible(int RejectedCandidates, int ExpandedStates) : FabricationFault(41, "fabrication:bend-sequence-infeasible", FabConcern.Forming);
    public sealed record TonnageExceeded(double RequiredKn, double CapacityKn) : FabricationFault(42, "fabrication:tonnage-exceeded", FabConcern.Forming);
    public sealed record MinBendRadiusViolated(int Bend, double RadiusMm, double FloorMm) : FabricationFault(43, "fabrication:min-bend-radius", FabConcern.Forming);
    public sealed record WeldAccessBlocked(int Joint, double TorchAngleDeg) : FabricationFault(44, "fabrication:weld-access-blocked", FabConcern.Joining);
    public sealed record HeatInputExceeded(int Joint, double KjPerMm, double Cap) : FabricationFault(45, "fabrication:heat-input-exceeded", FabConcern.Joining);
    public sealed record WpsUnqualified(FaultSubject.Qualification Variable, double Value) : FabricationFault(46, "fabrication:wps-unqualified", FabConcern.Joining);
    public sealed record ThreeMfWriteRejected(EgressKind Target, string Native) : FabricationFault(47, "fabrication:3mf-write-rejected", FabConcern.Additive);
    public sealed record UnknownAxis(string Axis, string Key) : FabricationFault(48, "fabrication:unknown-axis", FabConcern.Process);
    public sealed record IngressProviderUnavailable(SourceLocus Locus, string Detail) : FabricationFault(49, "fabrication:ingress-provider-unavailable", FabConcern.Ingress);
    public sealed record WitnessMalformed(string Witness, string Kind) : FabricationFault(50, "fabrication:witness-malformed", FabConcern.Process);
    public sealed record EquipmentInadmissible(EquipmentWitness Witness) : FabricationFault(51, "fabrication:equipment-inadmissible", FabConcern.Process);
    public sealed record DerivationRejected(DeriveWitness Witness, FaultSubject.Stage Stage) : FabricationFault(52, "fabrication:derivation-rejected", FabConcern.Process);
    public sealed record BendSearchBudgetExceeded(int ExpandedStates, int PendingStates)
        : FabricationFault(53, "fabrication:bend-search-budget-exceeded", FabConcern.Forming);

    public override int Code => FaultBand.Fabrication + Offset;
    public override string Message => MessageKey;
    public override string Category => "Fabrication";
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
