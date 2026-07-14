# [RASM_FABRICATION_FAULTS]

The fabrication fault rail: `FabricationFault` ONE closed `[Union]` deriving the kernel `Rasm.Domain.Expected` on the federation registry row `FaultBand.Fabrication = 2700` (`Rasm.Element` `Projection/fault#FAULT_TABLES` — band allocation lives in the ONE registry, never a page-local integer). The typed case IS the `Error`: the base threads `(Offset, Detail)` as positional columns every case constructor passes, `Code => FaultBand.Fabrication + Offset` reads the registry row through the generated implicit `SmartEnum`-to-`int` conversion, `Message => Detail` projects the frozen wire text, and a case lifts BARE onto the `Fin<T>`/`Validation<Error,T>` rail through the `Error` base — the payload travels WHOLE on the rail, so recovery discriminates `error.IsType<FabricationFault.Gouge>()` without message parsing; `ToError() => this` survives as the frozen call-site idiom, a zero-cost identity. The band is three layers. Layer 1 — the FROZEN first-instance arms 2701-2710: wire codes retained verbatim (the build-ON-never-re-band law plus the landed Persistence artifact-index persisted-decode constraint), every arm RETYPED from its bare `string Detail` to a typed payload, message prefixes byte-identical; the frozen codes are NON-contiguous per folder (Nesting spans 2701/2709/2710) — the recorded consequence of the freeze, mirroring the kernel's cross-cutting `DegenerateInput` 2400. Layer 2 — the growth arms 2711-2729, folder-grouped across two tiers (2711-2722, then the distinct-mechanism splits 2723-2729). Layer 3 — the widened-map arms 2730-2746, folder-grouped in ONE contiguous run per folder under the 15-cluster partition (Forming and Joining mint their clusters here; a receipts-only page — estimation, audit, report, manufacturability's verdict rows — mints NO arm, its typed receipt IS the verdict). Within every growth tier folder = fault-cluster is mechanically checkable: a code maps to exactly one folder and each folder's arms are a contiguous run per tier. `OpenLoop` 2704 is the ONE cross-cutting fabrication-contract arm (a boundary the kernel admits open but a toolpath/nest/skeleton/program/profile/form concern demands closed — the typed `FabConcern` discriminant names the rejecting concern). Every growth arm obeys the typed-payload law — never a weak-string escape hatch: `IngressTranslation` 2711 carries a `SourceLocus` discriminated union over its four source sites (DXF entity handle · OCCT shape id · DSTV block+line · element-graph node key) so the ONE polymorphic ingress fault stays single-arm with a typed carrier, `VoxelFault` 2715 carries the typed `VoxelBudget` (bounds + resolution + cap), and the simulate envelope arms carry the typed `MachineAxis` row, never a formatted string.

Band-ownership law: a degenerate or non-finite primitive set routes the kernel band-2400 `GeometryFault.DegenerateInput` — never re-cased here; a fabrication CONTRACT rejection (closed-boundary demand, inadmissible pair, capability shortfall) mints its own arm. Both families lower onto the one `Fin<T>` rail — bare, or through the retained `.ToError()` spelling. `Documentation/` holds the reserved EMPTY cluster: projection routes the kernel `GeometryFault.ProjectionFault(EdgeKind, int)` cluster 2436-2439 (`Rasm.Numerics`, raised by `Rasm.Drawing` `View.Apply`) and traveler is content-keyed assembly with no fault producer. Growth law: the next free offset is `+48` (2748); a new concern is one arm on its folder's block, never a new band — 2748-2799 is headroom, and a federation-wide renumber is the escalation path only if the century exhausts.

Wire posture: HOST-LOCAL. `FabricationFault` rides the `Fin<T>` rail every fabrication entrypoint returns; the frozen codes are the persisted-decode contract the Persistence artifact index reads; the union never sits between wire and rail.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: owns the `FabricationFault` `[Union]` deriving `Rasm.Domain.Expected` on `FaultBand.Fabrication` = 2700 — the frozen retyped arms 2701-2710, the folder-grouped growth arms 2711-2729 and 2730-2746 plus the 2747 native-crossing arm, the fault-local payload vocabularies (`FabConcern`/`JointFault`/`JointDiagnostic`/`NestFault`/`SourceKind`/`SourceLocus`/`MachineAxis`/`VoxelBudget`), the base-positional `(Offset, Detail)` columns deriving `Code`/`Message` with zero parallel dispatch sweeps, composing the kernel band-2400 `GeometryFault` for shared degenerate input.

## [02]-[FAULT_BAND]

- Owner: `FabricationFault` the closed `[Union]` band deriving the kernel `Expected` — one case per failure carrying its TYPED payload plus the two base positional columns (`Offset` the band-relative code, `Detail` the stable `fabrication:<kebab>:` message computed once at construction from the payload; 2701-2710 prefixes byte-identical to the frozen wire contract); `Code => FaultBand.Fabrication + Offset` and `Message => Detail` are the ONLY projections — the case declaration IS the code table and the message table, so a new arm is ONE declaration line and an omitted projection is unrepresentable; `Category => "Fabrication"` is the band telemetry column; `ToError() => this` retains the corpus-wide lowering idiom while the case lifts bare through the `Error` base; the fault-local carriers `FabConcern` (the OpenLoop rejecting concern), `JointDiagnostic` (`JointFault` kind + joint index + value), `NestFault` (the malformed cutting-stock job kind), `SourceKind`+`SourceLocus` (the polymorphic ingress site), `MachineAxis` (the ISO 841 NC axis vocabulary — x/y/z primary linear, a/b/c rotary, u/v/w secondary linear with the `Rotary` flag; supersedes the rotary-only `RotaryAxis`, and the wire-EDM upper-guide u/v axes are nameable rows, so singularity and envelope arms share ONE axis vocabulary), and `VoxelBudget` (bounds/voxel-size/cap) — minted HERE because they are fault-payload vocabulary no plane owns; plane-owned payload types (`SurfaceStrategy`, `BevelType`, `DerivationStage`, `EssentialVariable`, `SetupChain`, `FeatureControl`) stay plane-owned and are referenced, never re-minted.
- Cases, folder-clustered: **Process** `OpenLoop` 2704 `(FabConcern, int)` — the one cross-cutting arm. **Nesting** `NoFit` 2701 `(int part, Seq<double> triedRotations)` · `StockOverflow` 2709 `(int unplaced, int sheets)` · `Nest` 2710 `(NestFault, int part)`. **Kinematics** `Unreachable` 2702 `(JointDiagnostic, int target)` · `AxisSingularity` 2714 `(MachineAxis, double angle)` — RTCP/rotary singularity distinct from reach; the payload retypes onto the shared `MachineAxis` vocabulary (codes never move, payloads retype freely). **Geometry2D** `KerfCollision` 2703 `(Point3d at, double kerf)` — re-attributed to the kerf-compensation owner (`arcs`), off Posting. **Toolpath** `InadmissiblePair` 2705 `((ProcessModality, CutStrategy) pair)` · `Gouge` 2706 `(Point3d, CutterForm)` · `Collision` 2707 `(ExclusionZone)` · `SampleStalled` 2713 `(SurfaceStrategy, int iteration)` — OpenCAMLib drop-cutter non-convergence · `PartitionDegenerate` 2723 `(PartitionStrategy, int sites)` — Voronoi tessellation degenerates, distinct from `SampleStalled`'s sampling stall. **Additive** `NonManifoldSlice` 2708 `(int layer, int openChains)` · `VoxelFault` 2715 `(ImplicitOp, VoxelBudget)` · `OrientationInfeasible` 2716 `(int overhangs, double bestScore)` · `Unsupported3mfExtension` 2725 `(ThreeMfExtension, EgressKind)` — the capability-probe verdict alone · `ThreeMfWriteRejected` 2747 `(EgressKind, string native)` — the lib3mf WRITE crossing's own rejection, the `CheckError`-lifted native evidence its payload (the one sanctioned foreign-message arm: the native core owns the code taxonomy, so no local vocabulary can retype it); a CLR defect at the write seam propagates, never rails. **Ingress** `IngressTranslation` 2711 `(SourceKind, SourceLocus)` — the ONE polymorphic ingress fault, profile/solid/steel discriminated by `Kind`, the site by the `SourceLocus` union. **Tooling** `MachinabilityUnknown` 2712 `(Material, Operation)` — no `kc` row and no admissible formula fallback · `NoToolForOp` 2724 `(Operation, CutterForm required)` — the `Schedule` fold exhausts the assembly set, a SCHEDULING failure orthogonal to the missing-DATA failure. **Fixturing** `SetupInfeasible` 2717 `(int operation, int triedSetups)` · `DatumLineageBroken` 2726 `(SetupChain)` — the datum-lineage graph is cyclic or references an un-machined feature, a PRECEDENCE failure distinct from kinematic-reach infeasibility · `ClampOnMachinedFace` 2727 `(int operation, Point3d at)` — the op-N clamp lands on material op-N-1 already cut away, the typed verdict of the `StockSnapshot` keep-out, distinct from the per-move `Collision` 2707. **Posting** `DialectUnsupported` 2718 `(PostDialect, GNodeKind)` · `ProgramParse` 2719 `(int line, ModalGroup)` — G-code ingress modal-group violation · `BlockCapExceeded` 2728 `(PostDialect, int blocks, int cap)` — a program-SIZE overflow carries no offending `GNodeKind`, so it splits off `DialectUnsupported`. **Verify** `ProbeOvertravel` 2720 `(Point3d, double limit)`; a verify-time gouge routes the shared `Gouge` 2706. **Spec** `ToleranceUnsatisfiable` 2721 `(FeatureControl, double achievable)` · `CapabilityShortfall` 2722 `(ProcessKind, double cpk, double demanded)` — the plan-time Cpk gate's fail arm · `StackupExceeded` 2729 `(ToleranceChain, double accumulated, double bound)` — assembly-level accumulation, orthogonal to the per-feature and per-process arms. **Documentation** reserved (empty). Layer 3, folder-grouped: **Process** `RoutingInfeasible` 2730 `(UInt128 componentKey, DerivationStage)` — the `Run(Derive)` orchestrator exhausts routing (no capable process/machine/setup chain closes the plan). **Tooling** `WearEstimateUnfit` 2731 `(Tool, int samples)` — the Taylor/RUL fit lacks admissible decoded-telemetry support; a scheduling exhaustion stays `NoToolForOp` 2724. **Toolpath** `WireTaperExceeded` 2732 `(double angleDeg, double guideLimitDeg)` — the demanded wire-EDM taper exceeds the guide-plane capability · `LinkBlocked` 2733 `(Point3d from, Point3d to)` — the rapid-link router finds no collision-free retract channel · `BevelUnsupported` 2734 `(BevelType, double angleDeg)` — the demanded edge prep exceeds the head's tilt envelope. **Additive** `SupportUnbuildable` 2735 `(int layer, int region)` — the tree/planar support search terminates in the collision state with anchors unreachable. **Nesting** `RemnantStale` 2736 `(ContentKey)` — a reconcile references a remnant whose content key no longer resolves in inventory. **Fixturing** `AssemblyPrecedenceCyclic` 2737 `(int joints, int edges)` — the join-precedence graph is cyclic, orthogonal to `DatumLineageBroken`'s setup lineage. **Verify** `EnvelopeExceeded` 2738 `(MachineAxis, double at, double limit)` — the simulated modal-state walk drives an axis beyond the machine envelope · `SimulatedOvertravel` 2739 `(int block, MachineAxis, double by)` — a rapid crosses the soft-limit at a named block; distinct from the probing-cycle `ProbeOvertravel` 2720. **Forming** `UnfoldInfeasible` 2740 `(int faces, int branches)` — the tangent-face adjacency admits no spanning unfold (non-developable or branched) · `BendSequenceInfeasible` 2741 `(int bend, int triedOrders)` — the best-first sequence search exhausts the feasibility matrix · `TonnageExceeded` 2742 `(double requiredKn, double capacityKn)` · `MinBendRadiusViolated` 2743 `(int bend, double radiusMm, double floorMm)`. **Joining** `WeldAccessBlocked` 2744 `(int joint, double torchAngleDeg)` — no collision-free torch approach reaches the joint · `HeatInputExceeded` 2745 `(int joint, double kjPerMm, double cap)` — the procedure gate's heat-input ceiling fails · `WpsUnqualified` 2746 `(EssentialVariable, double value)` — a demanded weld falls outside the qualified WPS essential-variable range. 47 arms; Geometry2D, Kinematics, Posting, Spec, and Documentation mint NO tier-3 arm (their new pages emit receipts or route existing arms); any "2701-2704 as Process arms" re-attribution is REFUTED on disk.
- Entry: the union cases are the fault constructors the `Fin<T>` rail carries — `FabricationFault.NoFit(part, tried)` IS the band-coded `Error` and lifts bare (`Fin.Fail<T>(fault)`), while the retained `.ToError()` identity keeps every landed `<Fault>.<Case>(...).ToError()` call site verbatim; `GeometryFault.DegenerateInput(detail).ToError()` stays the kernel family's lowering — one rail for both families on the single `Fin<T>` every entrypoint returns.
- Auto: each owner routes its most specific arm — `Nest.Solve` routes `NoFit`/`StockOverflow`/`OpenLoop`; `Cam.Generate` routes `InadmissiblePair`; the cell solve folds `Robots` diagnostics into `Unreachable`'s `JointDiagnostic`; `Guard.Check` routes `Gouge`/`Collision`; the slicer routes `NonManifoldSlice` only for a topologically-broken CLOSED section (a coordinate defect stays kernel `DegenerateInput`); `ArcAlgebra` kerf compensation routes `KerfCollision`; `Ingress.Admit` routes `IngressTranslation` with its site locus; `CuttingData.Of` routes `MachinabilityUnknown`; `ToolMagazine.Schedule` routes `NoToolForOp`; the surface sampler `SampleStalled`; the machine-tool inverse `AxisSingularity`; the voxel lanes `VoxelFault`; orientation `OrientationInfeasible`; the 3MF writer boundary `ThreeMfWriteRejected` (extension probe `Unsupported3mfExtension`); the setup scheduler `SetupInfeasible`/`DatumLineageBroken`/`ClampOnMachinedFace`; posting `DialectUnsupported`/`BlockCapExceeded`, its `Parse` arm `ProgramParse`; probing `ProbeOvertravel`; the spec plane `ToleranceUnsatisfiable`/`CapabilityShortfall`/`StackupExceeded`; the derivation orchestrator `RoutingInfeasible`; the wear model `WearEstimateUnfit`; the wire/link/bevel toolpath arms their taper/routing/prep faults; the support search `SupportUnbuildable`; remnant reconcile `RemnantStale`; assembly sequencing `AssemblyPrecedenceCyclic`; the program simulator `EnvelopeExceeded`/`SimulatedOvertravel`; the unfold/brake folds `UnfoldInfeasible`/`BendSequenceInfeasible`/`TonnageExceeded`/`MinBendRadiusViolated`; the weld planes `WeldAccessBlocked`/`HeatInputExceeded`/`WpsUnqualified`.
- Receipt: `FabricationFault` is the typed fault evidence on the `Fin<T>` failure rail — the case IS the `Error`, so the payload (`SourceLocus`, `JointDiagnostic`, `VoxelBudget`, every typed member) survives lowering and a consumer recovers by `error.IsType<FabricationFault.IngressTranslation>()`, never by message parsing; no generic `IFault`, no error-code abstraction, no string-formatted detail masquerading as structure.
- Packages: `Rasm` (`Rasm.Domain.Expected` — the kernel fault base every federation band derives; `Op`), `Rasm.Element` (`FaultBand` — the federation band-allocation registry, composed through its generated implicit `SmartEnum`-to-`int` conversion), `Rasm.Numerics` (kernel `GeometryFault` band-2400 — composed for shared degenerate input), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`), LanguageExt.Core (`Error`/`Fin`/`Seq`), BCL inbox.
- Growth: a new fabrication failure is ONE case declaration at the next free offset (2748+) on its folder's block, carrying a TYPED payload and its base `(offset, detail)` columns from birth — no second or third sweep exists to forget; a shared geometry failure routes the kernel; a receipts-only concern never mints an arm; a new band never mints here.
- Boundary: `FabricationFault` mints ONLY fabrication-specific cases — a parallel band re-casing `DegenerateInput` or a synthesized kernel case is the deleted form; every arm's payload is typed and a `string Detail`-only arm (frozen or growth) is the dead form this registry retypes (the base `Detail` column is the DERIVED message, never the payload); the frozen 2701-2710 wire codes and message prefixes never renumber — a re-band is the named persisted-decode break; the parallel 47-arm `Code` and `Message` `Switch` sweeps are the deleted form — code and message thread the base positional columns, per the union base-positional-column law; the `Expected(string, int, Option<Error>)` base ctor belongs to `LanguageExt.Common.Expected` and is the named wrong base — the band derives the kernel `Rasm.Domain.Expected` (parameterless, `Category`-bearing) exactly as the Element and Materials bands do; faults route through `Fin`/`Validation` rails and exception-style domain control flow is the named defect; the fault-local carriers live HERE and a plane re-minting `FabConcern`/`SourceLocus`/`VoxelBudget` siblings is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Element;                  // FaultBand — the federation band-allocation registry (Projection/fault#FAULT_TABLES)
using Rasm.Fabrication.Additive;     // ImplicitOp · ThreeMfExtension (Additive plane payload vocabulary)
using Rasm.Fabrication.Fixturing;    // ExclusionZone · SetupChain
using Rasm.Fabrication.Joining;      // EssentialVariable (Joining plane payload vocabulary)
using Rasm.Fabrication.Posting;      // GNodeKind · ModalGroup
using Rasm.Fabrication.Spec;         // FeatureControl · ToleranceChain
using Rasm.Fabrication.Toolpath;     // SurfaceStrategy · PartitionStrategy · BevelType
using Rhino.Geometry;
using Thinktecture;
using Expected = Rasm.Domain.Expected;   // the kernel fault base — NOT LanguageExt.Common.Expected (no Category, ctor-coded)

namespace Rasm.Fabrication.Process;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// Fault-local payload vocabularies: carriers no plane owns, minted once on the band owner.
[SmartEnum<string>]
public sealed partial class FabConcern {
    public static readonly FabConcern Toolpath = new("toolpath");
    public static readonly FabConcern Nest = new("nest");
    public static readonly FabConcern Skeleton = new("skeleton");
    public static readonly FabConcern Program = new("program");
    public static readonly FabConcern Profile = new("profile");
    public static readonly FabConcern Form = new("form");
}

[SmartEnum<string>]
public sealed partial class JointFault {
    public static readonly JointFault JointLimit = new("joint-limit");
    public static readonly JointFault Singularity = new("singularity");
    public static readonly JointFault Reach = new("reach");
}

[SmartEnum<string>]
public sealed partial class NestFault {
    public static readonly NestFault EmptyCutList = new("empty-cut-list");
    public static readonly NestFault PartExceedsStock = new("part-exceeds-stock");
    public static readonly NestFault HeterogeneousMassCut = new("heterogeneous-mass-cut");
}

[SmartEnum<string>]
public sealed partial class SourceKind {
    public static readonly SourceKind Profile = new("profile");
    public static readonly SourceKind Solid = new("solid");
    public static readonly SourceKind Steel = new("steel");
    public static readonly SourceKind Element = new("element");
}

// ONE axis vocabulary for singularity AND envelope arms — the ISO 841 NC axis set (supersedes the rotary-only
// RotaryAxis): x/y/z primary linear, a/b/c rotary, u/v/w secondary linear (wire-EDM upper guide, gantry cross-slides).
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

    public bool Rotary { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct JointDiagnostic(JointFault Kind, int Joint, double Value);

public readonly record struct VoxelBudget(BoundingBox Bounds, double VoxelSize, long VoxelCap);

// The ONE polymorphic ingress site: four typed source loci, never a weak-string escape hatch.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SourceLocus {
    private SourceLocus() { }

    public sealed record DxfEntity(string Handle) : SourceLocus;
    public sealed record OcctShape(int Id) : SourceLocus;
    public sealed record DstvBlock(string Block, int Line) : SourceLocus;
    public sealed record ElementNode(UInt128 NodeKey) : SourceLocus;
}

// --- [ERRORS] -------------------------------------------------------------------------------------------------------------------------------------
// Layer 1 (2701-2710): frozen wire codes, payloads RETYPED, message prefixes byte-identical. Layer 2 (2711-2729) +
// Layer 3 (2730-2746) + 2747: folder-grouped growth arms; next free offset 2748. The case declaration IS the code and
// message table: (Offset, Detail) thread the base positional columns, the typed case IS the Error on the rail.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationFault(int Offset, string Detail) : Expected {
    public sealed record NoFit(int Part, Seq<double> TriedRotations) : FabricationFault(1, $"fabrication:no-fit:{Part}:{TriedRotations.Count}-rotations");
    public sealed record Unreachable(JointDiagnostic Diag, int Target) : FabricationFault(2, $"fabrication:unreachable:{Diag.Kind.Key}:j{Diag.Joint}:{Target}");
    public sealed record KerfCollision(Point3d At, double Kerf) : FabricationFault(3, $"fabrication:kerf-collision:{At.X:0.###},{At.Y:0.###}:{Kerf:0.###}");
    public sealed record OpenLoop(FabConcern Concern, int Primitive) : FabricationFault(4, $"fabrication:open-loop:{Concern.Key}:{Primitive}");
    public sealed record InadmissiblePair((ProcessModality Modality, CutStrategy Strategy) Pair) : FabricationFault(5, $"fabrication:inadmissible-pair:{Pair.Modality.Key}-rejects-{Pair.Strategy.Key}");
    public sealed record Gouge(Point3d Point, CutterForm Tool) : FabricationFault(6, $"fabrication:gouge:{Point.X:0.###},{Point.Y:0.###}:{Tool.Family.Key}");
    public sealed record Collision(ExclusionZone Zone) : FabricationFault(7, $"fabrication:collision:{Zone.Keepout.Count}-gon");
    public sealed record NonManifoldSlice(int Layer, int OpenChains) : FabricationFault(8, $"fabrication:non-manifold-slice:layer-{Layer}:{OpenChains}-open");
    public sealed record StockOverflow(int Unplaced, int Sheets) : FabricationFault(9, $"fabrication:stock-overflow:{Unplaced}-unplaced/{Sheets}-sheets");
    public sealed record Nest(NestFault Kind, int Part) : FabricationFault(10, $"fabrication:nest:{Kind.Key}:{Part}");
    public sealed record IngressTranslation(SourceKind Kind, SourceLocus Locus) : FabricationFault(11, $"fabrication:ingress-translation:{Kind.Key}");
    public sealed record MachinabilityUnknown(Material Material, Operation Op) : FabricationFault(12, $"fabrication:machinability-unknown:{Material.Key}:{Op.Key}");
    public sealed record SampleStalled(SurfaceStrategy Strategy, int Iteration) : FabricationFault(13, $"fabrication:sample-stalled:{Strategy.Key}:iter-{Iteration}");
    public sealed record AxisSingularity(MachineAxis Axis, double Angle) : FabricationFault(14, $"fabrication:axis-singularity:{Axis.Key}:{Angle:0.###}");
    public sealed record VoxelFault(ImplicitOp Op, VoxelBudget Budget) : FabricationFault(15, $"fabrication:voxel-fault:{Budget.VoxelSize:0.###}mm:{Budget.VoxelCap}");
    public sealed record OrientationInfeasible(int Overhangs, double BestScore) : FabricationFault(16, $"fabrication:orientation-infeasible:{Overhangs}:{BestScore:0.###}");
    public sealed record SetupInfeasible(int Operation, int TriedSetups) : FabricationFault(17, $"fabrication:setup-infeasible:op-{Operation}:{TriedSetups}-tried");
    public sealed record DialectUnsupported(PostDialect Dialect, GNodeKind Node) : FabricationFault(18, $"fabrication:dialect-unsupported:{Dialect.Key}");
    public sealed record ProgramParse(int Line, ModalGroup Group) : FabricationFault(19, $"fabrication:program-parse:line-{Line}");
    public sealed record ProbeOvertravel(Point3d At, double Limit) : FabricationFault(20, $"fabrication:probe-overtravel:{At.X:0.###},{At.Y:0.###}:{Limit:0.###}");
    public sealed record ToleranceUnsatisfiable(FeatureControl Frame, double Achievable) : FabricationFault(21, $"fabrication:tolerance-unsatisfiable:{Achievable:0.####}");
    public sealed record CapabilityShortfall(ProcessKind Process, double Cpk, double Demanded) : FabricationFault(22, $"fabrication:capability-shortfall:{Process.Key}:cpk-{Cpk:0.##}<{Demanded:0.##}");
    public sealed record PartitionDegenerate(PartitionStrategy Strategy, int Sites) : FabricationFault(23, $"fabrication:partition-degenerate:{Sites}-sites");
    public sealed record NoToolForOp(Operation Op, CutterForm Required) : FabricationFault(24, $"fabrication:no-tool-for-op:{Op.Key}:{Required.Family.Key}");
    public sealed record Unsupported3mfExtension(ThreeMfExtension Extension, EgressKind Target) : FabricationFault(25, $"fabrication:unsupported-3mf-extension:{Target.Key}");
    public sealed record DatumLineageBroken(SetupChain Chain) : FabricationFault(26, "fabrication:datum-lineage-broken");
    public sealed record ClampOnMachinedFace(int Operation, Point3d At) : FabricationFault(27, $"fabrication:clamp-on-machined-face:op-{Operation}:{At.X:0.###},{At.Y:0.###}");
    public sealed record BlockCapExceeded(PostDialect Dialect, int Blocks, int Cap) : FabricationFault(28, $"fabrication:block-cap-exceeded:{Dialect.Key}:{Blocks}>{Cap}");
    public sealed record StackupExceeded(ToleranceChain Chain, double Accumulated, double Bound) : FabricationFault(29, $"fabrication:stackup-exceeded:{Accumulated:0.####}>{Bound:0.####}");
    public sealed record RoutingInfeasible(UInt128 ComponentKey, DerivationStage Stage) : FabricationFault(30, $"fabrication:routing-infeasible:{Stage.Key}");
    public sealed record WearEstimateUnfit(Tool Tool, int Samples) : FabricationFault(31, $"fabrication:wear-estimate-unfit:{Tool.Key}:{Samples}-samples");
    public sealed record WireTaperExceeded(double AngleDeg, double GuideLimitDeg) : FabricationFault(32, $"fabrication:wire-taper-exceeded:{AngleDeg:0.##}>{GuideLimitDeg:0.##}");
    public sealed record LinkBlocked(Point3d From, Point3d To) : FabricationFault(33, $"fabrication:link-blocked:{From.X:0.###},{From.Y:0.###}->{To.X:0.###},{To.Y:0.###}");
    public sealed record BevelUnsupported(BevelType Bevel, double AngleDeg) : FabricationFault(34, $"fabrication:bevel-unsupported:{Bevel.Key}:{AngleDeg:0.##}");
    public sealed record SupportUnbuildable(int Layer, int Region) : FabricationFault(35, $"fabrication:support-unbuildable:layer-{Layer}:{Region}");
    public sealed record RemnantStale(ContentKey Key) : FabricationFault(36, $"fabrication:remnant-stale:{Key.Kind.Key}");
    public sealed record AssemblyPrecedenceCyclic(int Joints, int Edges) : FabricationFault(37, $"fabrication:assembly-precedence-cyclic:{Joints}-joints:{Edges}-edges");
    public sealed record EnvelopeExceeded(MachineAxis Axis, double At, double Limit) : FabricationFault(38, $"fabrication:envelope-exceeded:{Axis.Key}:{At:0.###}>{Limit:0.###}");
    public sealed record SimulatedOvertravel(int Block, MachineAxis Axis, double By) : FabricationFault(39, $"fabrication:simulated-overtravel:block-{Block}:{Axis.Key}:{By:0.###}");
    public sealed record UnfoldInfeasible(int Faces, int Branches) : FabricationFault(40, $"fabrication:unfold-infeasible:{Faces}-faces:{Branches}-branches");
    public sealed record BendSequenceInfeasible(int Bend, int TriedOrders) : FabricationFault(41, $"fabrication:bend-sequence-infeasible:bend-{Bend}:{TriedOrders}-tried");
    public sealed record TonnageExceeded(double RequiredKn, double CapacityKn) : FabricationFault(42, $"fabrication:tonnage-exceeded:{RequiredKn:0.#}>{CapacityKn:0.#}kn");
    public sealed record MinBendRadiusViolated(int Bend, double RadiusMm, double FloorMm) : FabricationFault(43, $"fabrication:min-bend-radius:bend-{Bend}:{RadiusMm:0.##}<{FloorMm:0.##}");
    public sealed record WeldAccessBlocked(int Joint, double TorchAngleDeg) : FabricationFault(44, $"fabrication:weld-access-blocked:joint-{Joint}:{TorchAngleDeg:0.#}deg");
    public sealed record HeatInputExceeded(int Joint, double KjPerMm, double Cap) : FabricationFault(45, $"fabrication:heat-input-exceeded:joint-{Joint}:{KjPerMm:0.##}>{Cap:0.##}");
    public sealed record WpsUnqualified(EssentialVariable Variable, double Value) : FabricationFault(46, $"fabrication:wps-unqualified:{Variable.Key}:{Value:0.###}");
    public sealed record ThreeMfWriteRejected(EgressKind Target, string Native) : FabricationFault(47, $"fabrication:3mf-write-rejected:{Target.Key}:{Native}");

    public override int Code => FaultBand.Fabrication + Offset;
    public override string Message => Detail;
    public override string Category => "Fabrication";

    // The retained corpus-wide lowering idiom — a zero-cost identity; the case lifts bare through the Error base.
    public Error ToError() => this;
}
```
