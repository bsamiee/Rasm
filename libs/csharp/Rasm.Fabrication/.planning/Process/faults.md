# [RASM_FABRICATION_FAULTS]

The fabrication fault rail is one closed `FabricationFault` `[Union]` deriving `Rasm.Domain.Expected` on `FaultBand.Fabrication = 2700`. Each case carries its typed evidence, while the base positional columns carry only the stable band-relative offset and invariant message key. `Code`, `Message`, and `Category` project those columns without a parallel dispatch sweep, and `ToError()` remains the zero-cost corpus lowering idiom. `RelationFault` closes every inadmissible cross-axis relation behind the existing `InadmissiblePair` arm, so modality/strategy, process/machine, dialect/modality, and operation/equipment failures retain their native operands without string parsing. `JointDiagnostic.Admit` and `VoxelBudget.Admit` prevent failure evidence from contradicting the condition it reports.

Degenerate or non-finite geometry routes `GeometryFault.DegenerateInput`; fabrication contract failures route the most specific `FabricationFault`. `Documentation/` composes kernel projection failures and adds no fabrication case. The next fabrication offset is `49`, and each admitted failure extends this union with typed evidence and a stable message key.

Wire posture: HOST-LOCAL. `FabricationFault` rides the `Fin<T>` rail every fabrication entrypoint returns; the frozen codes are the persisted-decode contract the Persistence artifact index reads; the union never sits between wire and rail.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: `FabConcern`, `JointFault`, `NestFault`, `CollisionContact`, `SourceKind`, `MachineAxis`, `JointDiagnostic`, `VoxelBudget`, `SourceLocus`, `RelationFault`, `KerfWitness`, and the closed `FabricationFault` band.

## [02]-[FAULT_BAND]

- Owner: `FabricationFault` is the closed fabrication band deriving `Rasm.Domain.Expected`. Each case carries typed domain evidence plus the inherited `Offset` and `MessageKey`. `RelationFault` discriminates cross-axis rejections, and `KerfWitness` discriminates a vanished region from an overlapping survivor pair.
- Cases: frozen offsets `1` through `10` preserve their persisted identity, and folder-owned offsets `11` through `48` extend the band. `JointFault` covers limit, singularity, reach, collision, self-collision, velocity, acceleration, jerk, torque, configuration, and disconnected-chain failures. `NestFault` covers cut-list, stock-envelope, material, thickness, grain, quantity, and remnant failures. `MachineAxis` covers linear, rotary, articulated, and press-brake axes. `SourceKind` and `SourceLocus` cover profile, solid, steel, element, `3MF`, Rhino, mesh, program, and generic exchange ingress. `RelationFault` covers modality/strategy, process/machine, dialect/modality, operation/equipment, and process/material relations. `KerfWitness` covers vanished and overlapping compensated regions; `UnknownAxis` carries generic keyed-vocabulary admission failure without borrowing a geometry fault.
- Entry: each union case lifts directly through `Fin.Fail<T>`, and `ToError()` preserves the established lowering spelling as an identity. `JointDiagnostic.Admit` and `VoxelBudget.Admit` own their raw evidence boundaries. `Code` equals `FaultBand.Fabrication + Offset`, `Message` equals `MessageKey`, and `Category` equals `Fabrication`.
- Auto: cross-axis admission routes one `RelationFault` case; kerf compensation routes one `KerfWitness` case; source translation routes one `SourceLocus` case. Independent admission failures accumulate through `Validation`, while execution failures abort through `Fin<T>` on the first specific `FabricationFault`.
- Receipt: the concrete case remains recoverable through `Error.IsType<T>()`, so consumers never parse `Message`. Native write rejection alone carries provider-owned message text because that provider owns the native error taxonomy.
- Packages: `Rasm.Domain.Expected` supplies the federation fault base; `FaultBand.Fabrication` supplies the code allocation; `GeometryFault` owns shared geometric invalidity; Thinktecture supplies `[Union]` and `[SmartEnum<string>]`; LanguageExt supplies `Error`, `Fin<T>`, and immutable payload collections.
- Growth: a fabrication-specific failure is one typed union case at the next free offset with its stable message key. Shared geometry, arrangement, projection, and parametric failures remain in their kernel bands.
- Boundary: frozen codes never renumber, invariant message keys never interpolate runtime values, and typed payloads retain every discriminant needed for recovery. No plane introduces another fabrication error base or a parallel code/message dispatch.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Element;
using Rasm.Fabrication.Additive;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Posting;
using Rasm.Fabrication.Spec;
using Rasm.Fabrication.Toolpath;
using Rhino.Geometry;
using Thinktecture;
using Expected = Rasm.Domain.Expected;

namespace Rasm.Fabrication.Process;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FabConcern {
    public static readonly FabConcern Toolpath = new("toolpath");
    public static readonly FabConcern Nest = new("nest");
    public static readonly FabConcern Skeleton = new("skeleton");
    public static readonly FabConcern Program = new("program");
    public static readonly FabConcern Profile = new("profile");
    public static readonly FabConcern Form = new("form");
    public static readonly FabConcern Additive = new("additive");
    public static readonly FabConcern Wire = new("wire");
    public static readonly FabConcern Fixture = new("fixture");
    public static readonly FabConcern Geometry2D = new("geometry-2d");
    public static readonly FabConcern Joining = new("joining");
    public static readonly FabConcern Verify = new("verify");
    public static readonly FabConcern Documentation = new("documentation");
}

[SmartEnum<string>]
public sealed partial class JointFault {
    public static readonly JointFault JointLimit = new("joint-limit");
    public static readonly JointFault Singularity = new("singularity");
    public static readonly JointFault Reach = new("reach");
    public static readonly JointFault Collision = new("collision");
    public static readonly JointFault Velocity = new("velocity");
    public static readonly JointFault Acceleration = new("acceleration");
    public static readonly JointFault Jerk = new("jerk");
    public static readonly JointFault Torque = new("torque");
    public static readonly JointFault SelfCollision = new("self-collision");
    public static readonly JointFault Configuration = new("configuration");
    public static readonly JointFault Disconnected = new("disconnected");
}

// The collision-cause vocabulary: whether the swept CUTTER or the swept HOLDER produced the contact. Guard
// mints the cause; the fault case preserves it so recovery can distinguish gouge-risk from holder-clearance work.
[SmartEnum<string>]
public sealed partial class CollisionContact {
    public static readonly CollisionContact Cutter = new("cutter");
    public static readonly CollisionContact Holder = new("holder");
}

[SmartEnum<string>]
public sealed partial class NestFault {
    public static readonly NestFault EmptyCutList = new("empty-cut-list");
    public static readonly NestFault PartExceedsStock = new("part-exceeds-stock");
    public static readonly NestFault HeterogeneousMassCut = new("heterogeneous-mass-cut");
    public static readonly NestFault MaterialMismatch = new("material-mismatch");
    public static readonly NestFault ThicknessMismatch = new("thickness-mismatch");
    public static readonly NestFault GrainIncompatible = new("grain-incompatible");
    public static readonly NestFault InvalidQuantity = new("invalid-quantity");
    public static readonly NestFault RemnantUnavailable = new("remnant-unavailable");
}

[SmartEnum<string>]
public sealed partial class SourceKind {
    public static readonly SourceKind Profile = new("profile");
    public static readonly SourceKind Solid = new("solid");
    public static readonly SourceKind Steel = new("steel");
    public static readonly SourceKind Element = new("element");
    public static readonly SourceKind ThreeMf = new("3mf");
    public static readonly SourceKind Rhino = new("rhino");
    public static readonly SourceKind Mesh = new("mesh");
    public static readonly SourceKind Program = new("program");
    public static readonly SourceKind Exchange = new("exchange");
}

// One axis vocabulary covers ISO 841 machine axes, articulated joints, and press-brake synchronized/backgauge axes.
[SmartEnum<string>]
public sealed partial class MachineAxis {
    public static readonly MachineAxis X = new("x", rotary: false);
    public static readonly MachineAxis Y = new("y", rotary: false);
    public static readonly MachineAxis Z = new("z", rotary: false);
    public static readonly MachineAxis A = new("a", rotary: true);
    public static readonly MachineAxis B = new("b", rotary: true);
    public static readonly MachineAxis C = new("c", rotary: true);
    public static readonly MachineAxis U = new("u", rotary: false);
    public static readonly MachineAxis V = new("v", rotary: false);
    public static readonly MachineAxis W = new("w", rotary: false);
    public static readonly MachineAxis J1 = new("j1", rotary: true);
    public static readonly MachineAxis J2 = new("j2", rotary: true);
    public static readonly MachineAxis J3 = new("j3", rotary: true);
    public static readonly MachineAxis J4 = new("j4", rotary: true);
    public static readonly MachineAxis J5 = new("j5", rotary: true);
    public static readonly MachineAxis J6 = new("j6", rotary: true);
    public static readonly MachineAxis J7 = new("j7", rotary: true);
    public static readonly MachineAxis Y1 = new("y1", rotary: false);
    public static readonly MachineAxis Y2 = new("y2", rotary: false);
    public static readonly MachineAxis R = new("r", rotary: false);
    public static readonly MachineAxis Z1 = new("z1", rotary: false);
    public static readonly MachineAxis Z2 = new("z2", rotary: false);

    public bool Rotary { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record JointDiagnostic {
    private JointDiagnostic(JointFault kind, int joint, double value) => (Kind, Joint, Value) = (kind, joint, value);

    public JointFault Kind { get; }
    public int Joint { get; }
    public double Value { get; }

    public static Fin<JointDiagnostic> Admit(JointFault kind, int joint, double value) =>
        joint >= 0 && double.IsFinite(value)
            ? Fin.Succ(new JointDiagnostic(kind, joint, value))
            : Fin.Fail<JointDiagnostic>(GeometryFault.DegenerateInput("joint-diagnostic").ToError());
}

public sealed record VoxelBudget {
    private VoxelBudget(BoundingBox bounds, double voxelSize, long voxelCap) =>
        (Bounds, VoxelSize, VoxelCap) = (bounds, voxelSize, voxelCap);

    public BoundingBox Bounds { get; }
    public double VoxelSize { get; }
    public long VoxelCap { get; }

    public static Fin<VoxelBudget> Admit(BoundingBox bounds, double voxelSize, long voxelCap) =>
        bounds.IsValid && double.IsFinite(voxelSize) && voxelSize > 0.0 && voxelCap > 0
            ? Fin.Succ(new VoxelBudget(bounds, voxelSize, voxelCap))
            : Fin.Fail<VoxelBudget>(GeometryFault.DegenerateInput("voxel-budget").ToError());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SourceLocus {
    private SourceLocus() { }

    public sealed record DxfEntity(string Handle) : SourceLocus;
    public sealed record OcctShape(int Id) : SourceLocus;
    public sealed record DstvBlock(string Block, int Line) : SourceLocus;
    public sealed record ElementNode(UInt128 NodeKey) : SourceLocus;
    public sealed record ThreeMfObject(string Model, int Object) : SourceLocus;
    public sealed record RhinoObject(Guid Object) : SourceLocus;
    public sealed record MeshFace(int Face) : SourceLocus;
    public sealed record ProgramBlock(int Block) : SourceLocus;
    public sealed record ExchangeEntity(string Scheme, string Entity) : SourceLocus;
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
// Frozen offsets retain persisted identity; each case declaration also owns its message key.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationFault(int Offset, string MessageKey) : Expected {
    public sealed record NoFit(int Part, Seq<double> TriedRotations) : FabricationFault(1, "fabrication:no-fit");
    public sealed record Unreachable(JointDiagnostic Diag, int Target) : FabricationFault(2, "fabrication:unreachable");
    public sealed record KerfCollision(KerfWitness Witness, double Kerf) : FabricationFault(3, "fabrication:kerf-collision");
    public sealed record OpenLoop(FabConcern Concern, int Primitive) : FabricationFault(4, "fabrication:open-loop");
    public sealed record InadmissiblePair(RelationFault Pair) : FabricationFault(5, "fabrication:inadmissible-pair");
    public sealed record Gouge(Point3d Point, CutterForm Tool) : FabricationFault(6, "fabrication:gouge");
    public sealed record Collision(ExclusionZone Zone, CollisionContact Contact) : FabricationFault(7, "fabrication:collision");
    public sealed record NonManifoldSlice(int Layer, int OpenChains) : FabricationFault(8, "fabrication:non-manifold-slice");
    public sealed record StockOverflow(int Unplaced, int Sheets) : FabricationFault(9, "fabrication:stock-overflow");
    public sealed record Nest(NestFault Kind, int Part) : FabricationFault(10, "fabrication:nest");
    public sealed record IngressTranslation(SourceKind Kind, SourceLocus Locus) : FabricationFault(11, "fabrication:ingress-translation");
    public sealed record MachinabilityUnknown(Material Material, Operation Op) : FabricationFault(12, "fabrication:machinability-unknown");
    public sealed record SampleStalled(SurfaceStrategy Strategy, int Iteration) : FabricationFault(13, "fabrication:sample-stalled");
    public sealed record AxisSingularity(MachineAxis Axis, double Angle) : FabricationFault(14, "fabrication:axis-singularity");
    public sealed record VoxelFault(ImplicitOp Op, VoxelBudget Budget) : FabricationFault(15, "fabrication:voxel-fault");
    public sealed record OrientationInfeasible(int Overhangs, double BestScore) : FabricationFault(16, "fabrication:orientation-infeasible");
    public sealed record SetupInfeasible(int Operation, int TriedSetups) : FabricationFault(17, "fabrication:setup-infeasible");
    public sealed record DialectUnsupported(PostDialect Dialect, GNodeKind Node) : FabricationFault(18, "fabrication:dialect-unsupported");
    public sealed record ProgramParse(int Line, ModalGroup Group) : FabricationFault(19, "fabrication:program-parse");
    public sealed record ProbeOvertravel(Point3d At, double Limit) : FabricationFault(20, "fabrication:probe-overtravel");
    public sealed record ToleranceUnsatisfiable(FeatureControl Frame, double Achievable) : FabricationFault(21, "fabrication:tolerance-unsatisfiable");
    public sealed record CapabilityShortfall(ProcessKind Process, double Cpk, double Demanded) : FabricationFault(22, "fabrication:capability-shortfall");
    public sealed record PartitionDegenerate(PartitionStrategy Strategy, int Sites) : FabricationFault(23, "fabrication:partition-degenerate");
    public sealed record NoToolForOp(Operation Op, CutterForm Required) : FabricationFault(24, "fabrication:no-tool-for-op");
    public sealed record Unsupported3mfExtension(ThreeMfExtension Extension, EgressKind Target) : FabricationFault(25, "fabrication:unsupported-3mf-extension");
    public sealed record DatumLineageBroken(SetupChain Chain) : FabricationFault(26, "fabrication:datum-lineage-broken");
    public sealed record ClampOnMachinedFace(int Operation, Point3d At) : FabricationFault(27, "fabrication:clamp-on-machined-face");
    public sealed record BlockCapExceeded(PostDialect Dialect, int Blocks, int Cap) : FabricationFault(28, "fabrication:block-cap-exceeded");
    public sealed record StackupExceeded(ToleranceChain Chain, double Accumulated, double Bound) : FabricationFault(29, "fabrication:stackup-exceeded");
    public sealed record RoutingInfeasible(UInt128 ComponentKey, DerivationStage Stage) : FabricationFault(30, "fabrication:routing-infeasible");
    public sealed record WearEstimateUnfit(Tool Tool, int Samples) : FabricationFault(31, "fabrication:wear-estimate-unfit");
    public sealed record WireTaperExceeded(double AngleDeg, double GuideLimitDeg) : FabricationFault(32, "fabrication:wire-taper-exceeded");
    public sealed record LinkBlocked(Point3d From, Point3d To) : FabricationFault(33, "fabrication:link-blocked");
    public sealed record BevelUnsupported(BevelType Bevel, double AngleDeg) : FabricationFault(34, "fabrication:bevel-unsupported");
    public sealed record SupportUnbuildable(int Layer, int Region) : FabricationFault(35, "fabrication:support-unbuildable");
    public sealed record RemnantStale(ContentKey Key) : FabricationFault(36, "fabrication:remnant-stale");
    public sealed record AssemblyPrecedenceCyclic(int Joints, int Edges) : FabricationFault(37, "fabrication:assembly-precedence-cyclic");
    public sealed record EnvelopeExceeded(MachineAxis Axis, double At, double Limit) : FabricationFault(38, "fabrication:envelope-exceeded");
    public sealed record SimulatedOvertravel(int Block, MachineAxis Axis, double By) : FabricationFault(39, "fabrication:simulated-overtravel");
    public sealed record UnfoldInfeasible(int Faces, int Branches) : FabricationFault(40, "fabrication:unfold-infeasible");
    public sealed record BendSequenceInfeasible(int Bend, int TriedOrders) : FabricationFault(41, "fabrication:bend-sequence-infeasible");
    public sealed record TonnageExceeded(double RequiredKn, double CapacityKn) : FabricationFault(42, "fabrication:tonnage-exceeded");
    public sealed record MinBendRadiusViolated(int Bend, double RadiusMm, double FloorMm) : FabricationFault(43, "fabrication:min-bend-radius");
    public sealed record WeldAccessBlocked(int Joint, double TorchAngleDeg) : FabricationFault(44, "fabrication:weld-access-blocked");
    public sealed record HeatInputExceeded(int Joint, double KjPerMm, double Cap) : FabricationFault(45, "fabrication:heat-input-exceeded");
    public sealed record WpsUnqualified(EssentialVariable Variable, double Value) : FabricationFault(46, "fabrication:wps-unqualified");
    public sealed record ThreeMfWriteRejected(EgressKind Target, string Native) : FabricationFault(47, "fabrication:3mf-write-rejected");
    public sealed record UnknownAxis(string Axis, string Key) : FabricationFault(48, "fabrication:unknown-axis");

    public override int Code => FaultBand.Fabrication + Offset;
    public override string Message => MessageKey;
    public override string Category => "Fabrication";

    public Error ToError() => this;
}
```
