# [RASM_FABRICATION_FAULTS]

The fabrication fault rail: `FabricationFault` ONE closed `[Union]` on the federation registry row `FaultBand.Fabrication = 2700` (`Rasm.Element` `Projection/fault#FAULT_TABLES` — band allocation lives in the ONE registry, never a page-local integer). The band is three layers. Layer 1 — the FROZEN first-instance arms 2701-2710: wire codes retained verbatim (the build-ON-never-re-band law plus the landed Persistence artifact-index persisted-decode constraint), every arm RETYPED from its bare `string Detail` to a typed payload, message prefixes unchanged; the frozen codes are NON-contiguous per folder (Nesting spans 2701/2709/2710) — the recorded consequence of the freeze, mirroring the kernel's cross-cutting `DegenerateInput` 2400. Layer 2 — the growth arms 2711-2729, folder-grouped across two tiers (2711-2722, then the distinct-mechanism splits 2723-2729). Layer 3 — the widened-map arms 2730-2746, folder-grouped in ONE contiguous run per folder under the 15-cluster partition (Forming and Joining mint their clusters here; a receipts-only page — estimation, audit, report, manufacturability's verdict rows — mints NO arm, its typed receipt IS the verdict). Within every growth tier folder = fault-cluster is mechanically checkable: a code maps to exactly one folder and each folder's arms are a contiguous run per tier. `OpenLoop` 2704 is the ONE cross-cutting fabrication-contract arm (a boundary the kernel admits open but a toolpath/nest/skeleton/program/profile/form concern demands closed — the typed `FabConcern` discriminant names the rejecting concern). Every growth arm obeys the typed-payload law — never a weak-string escape hatch: `IngressTranslation` 2711 carries a `SourceLocus` discriminated union over its four source sites (DXF entity handle · OCCT shape id · DSTV block+line · element-graph node key) so the ONE polymorphic ingress fault stays single-arm with a typed carrier, `VoxelFault` 2715 carries the typed `VoxelBudget` (bounds + resolution + cap), and the simulate envelope arms carry the typed `MachineAxis` row, never a formatted string.

Band-ownership law: a degenerate or non-finite primitive set routes the kernel band-2400 `GeometryFault.DegenerateInput` — never re-cased here; a fabrication CONTRACT rejection (closed-boundary demand, inadmissible pair, capability shortfall) mints its own arm. Both families lower onto the one `Fin<T>` rail through `<Fault>.<Case>(...).ToError()`. `Documentation/` holds the reserved EMPTY cluster: projection routes the kernel `GeometryFault.ProjectionFault(EdgeKind, int)` cluster 2436-2439 (`Rasm.Numerics`, raised by `Rasm.Drawing` `View.Apply`) and traveler is content-keyed assembly with no fault producer. Growth law: the next free offset is `+47` (2747); a new concern is one arm on its folder's block, never a new band — 2747-2799 is headroom, and a federation-wide renumber is the escalation path only if the century exhausts.

Wire posture: HOST-LOCAL. `FabricationFault` rides the `Fin<T>` rail every fabrication entrypoint returns; the frozen codes are the persisted-decode contract the Persistence artifact index reads; the union never sits between wire and rail.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: owns the `FabricationFault` `[Union]` on `FaultBand.Fabrication` = 2700 — the frozen retyped arms 2701-2710, the folder-grouped growth arms 2711-2729 and 2730-2746, the fault-local payload vocabularies (`FabConcern`/`JointFault`/`JointDiagnostic`/`NestFault`/`SourceKind`/`SourceLocus`/`MachineAxis`/`VoxelBudget`), offset-derived `Code`/`Message`/`ToError`, composing the kernel band-2400 `GeometryFault` for shared degenerate input.

## [02]-[FAULT_BAND]

- Owner: `FabricationFault` the closed `[Union]` band — one case per failure carrying its TYPED payload, a `Code` `Switch` deriving `FaultBand.Fabrication + n`, a `Message` `Switch` projecting the payload into the stable `fabrication:<kebab>:` prefix (2701-2710 prefixes byte-identical to the frozen wire contract), and `ToError()` lowering onto the `Fin<T>` failure channel; the fault-local carriers `FabConcern` (the OpenLoop rejecting concern), `JointDiagnostic` (`JointFault` kind + joint index + value), `NestFault` (the malformed cutting-stock job kind), `SourceKind`+`SourceLocus` (the polymorphic ingress site), `MachineAxis` (x/y/z linear + a/b/c rotary with the `Rotary` flag — supersedes the rotary-only `RotaryAxis`; singularity and envelope arms share ONE axis vocabulary), and `VoxelBudget` (bounds/voxel-size/cap) — minted HERE because they are fault-payload vocabulary no plane owns; plane-owned payload types (`SurfaceStrategy`, `BevelType`, `DerivationStage`, `EssentialVariable`, `SetupChain`, `FeatureControl`) stay plane-owned and are referenced, never re-minted.
- Cases, folder-clustered: **Process** `OpenLoop` 2704 `(FabConcern, int)` — the one cross-cutting arm. **Nesting** `NoFit` 2701 `(int part, Seq<double> triedRotations)` · `StockOverflow` 2709 `(int unplaced, int sheets)` · `Nest` 2710 `(NestFault, int part)`. **Kinematics** `Unreachable` 2702 `(JointDiagnostic, int target)` · `AxisSingularity` 2714 `(MachineAxis, double angle)` — RTCP/rotary singularity distinct from reach; the payload retypes onto the shared `MachineAxis` vocabulary (codes never move, payloads retype freely). **Geometry2D** `KerfCollision` 2703 `(Point3d at, double kerf)` — re-attributed to the kerf-compensation owner (`arcs`), off Posting. **Toolpath** `InadmissiblePair` 2705 `((ProcessModality, CutStrategy) pair)` · `Gouge` 2706 `(Point3d, CutterForm)` · `Collision` 2707 `(ExclusionZone)` · `SampleStalled` 2713 `(SurfaceStrategy, int iteration)` — OpenCAMLib drop-cutter non-convergence · `PartitionDegenerate` 2723 `(PartitionStrategy, int sites)` — Voronoi tessellation degenerates, distinct from `SampleStalled`'s sampling stall. **Additive** `NonManifoldSlice` 2708 `(int layer, int openChains)` · `VoxelFault` 2715 `(ImplicitOp, VoxelBudget)` · `OrientationInfeasible` 2716 `(int overhangs, double bestScore)` · `Unsupported3mfExtension` 2725 `(ThreeMfExtension, EgressKind)` — a lib3mf write error lifts to this arm at the boundary. **Ingress** `IngressTranslation` 2711 `(SourceKind, SourceLocus)` — the ONE polymorphic ingress fault, profile/solid/steel discriminated by `Kind`, the site by the `SourceLocus` union. **Tooling** `MachinabilityUnknown` 2712 `(Material, Operation)` — no `kc` row and no admissible formula fallback · `NoToolForOp` 2724 `(Operation, CutterForm required)` — the `Schedule` fold exhausts the assembly set, a SCHEDULING failure orthogonal to the missing-DATA failure. **Fixturing** `SetupInfeasible` 2717 `(int operation, int triedSetups)` · `DatumLineageBroken` 2726 `(SetupChain)` — the datum-lineage graph is cyclic or references an un-machined feature, a PRECEDENCE failure distinct from kinematic-reach infeasibility · `ClampOnMachinedFace` 2727 `(int operation, Point3d at)` — the op-N clamp lands on material op-N-1 already cut away, the typed verdict of the `StockSnapshot` keep-out, distinct from the per-move `Collision` 2707. **Posting** `DialectUnsupported` 2718 `(PostDialect, GNodeKind)` · `ProgramParse` 2719 `(int line, ModalGroup)` — G-code ingress modal-group violation · `BlockCapExceeded` 2728 `(PostDialect, int blocks, int cap)` — a program-SIZE overflow carries no offending `GNodeKind`, so it splits off `DialectUnsupported`. **Verify** `ProbeOvertravel` 2720 `(Point3d, double limit)`; a verify-time gouge routes the shared `Gouge` 2706. **Spec** `ToleranceUnsatisfiable` 2721 `(FeatureControl, double achievable)` · `CapabilityShortfall` 2722 `(ProcessKind, double cpk, double demanded)` — the plan-time Cpk gate's fail arm · `StackupExceeded` 2729 `(ToleranceChain, double accumulated, double bound)` — assembly-level accumulation, orthogonal to the per-feature and per-process arms. **Documentation** reserved (empty). Layer 3, folder-grouped: **Process** `RoutingInfeasible` 2730 `(UInt128 componentKey, DerivationStage)` — the `Run(Derive)` orchestrator exhausts routing (no capable process/machine/setup chain closes the plan). **Tooling** `WearEstimateUnfit` 2731 `(Tool, int samples)` — the Taylor/RUL fit lacks admissible decoded-telemetry support; a scheduling exhaustion stays `NoToolForOp` 2724. **Toolpath** `WireTaperExceeded` 2732 `(double angleDeg, double guideLimitDeg)` — the demanded wire-EDM taper exceeds the guide-plane capability · `LinkBlocked` 2733 `(Point3d from, Point3d to)` — the rapid-link router finds no collision-free retract channel · `BevelUnsupported` 2734 `(BevelType, double angleDeg)` — the demanded edge prep exceeds the head's tilt envelope. **Additive** `SupportUnbuildable` 2735 `(int layer, int region)` — the tree/planar support search terminates in the collision state with anchors unreachable. **Nesting** `RemnantStale` 2736 `(ContentKey)` — a reconcile references a remnant whose content key no longer resolves in inventory. **Fixturing** `AssemblyPrecedenceCyclic` 2737 `(int joints, int edges)` — the join-precedence graph is cyclic, orthogonal to `DatumLineageBroken`'s setup lineage. **Verify** `EnvelopeExceeded` 2738 `(MachineAxis, double at, double limit)` — the simulated modal-state walk drives an axis beyond the machine envelope · `SimulatedOvertravel` 2739 `(int block, MachineAxis, double by)` — a rapid crosses the soft-limit at a named block; distinct from the probing-cycle `ProbeOvertravel` 2720. **Forming** `UnfoldInfeasible` 2740 `(int faces, int branches)` — the tangent-face adjacency admits no spanning unfold (non-developable or branched) · `BendSequenceInfeasible` 2741 `(int bend, int triedOrders)` — the best-first sequence search exhausts the feasibility matrix · `TonnageExceeded` 2742 `(double requiredKn, double capacityKn)` · `MinBendRadiusViolated` 2743 `(int bend, double radiusMm, double floorMm)`. **Joining** `WeldAccessBlocked` 2744 `(int joint, double torchAngleDeg)` — no collision-free torch approach reaches the joint · `HeatInputExceeded` 2745 `(int joint, double kjPerMm, double cap)` — the procedure gate's heat-input ceiling fails · `WpsUnqualified` 2746 `(EssentialVariable, double value)` — a demanded weld falls outside the qualified WPS essential-variable range. 46 arms; Geometry2D, Kinematics, Posting, Spec, and Documentation mint NO tier-3 arm (their new pages emit receipts or route existing arms); any "2701-2704 as Process arms" re-attribution is REFUTED on disk.
- Entry: the union cases are the fault constructors the `Fin<T>` rail carries — `FabricationFault.NoFit(part, tried).ToError()` and `GeometryFault.DegenerateInput(detail).ToError()` build the band-coded `Error`, one lowering idiom for both families on the single rail every entrypoint returns.
- Auto: each owner routes its most specific arm — `Nest.Solve` routes `NoFit`/`StockOverflow`/`OpenLoop`; `Cam.Generate` routes `InadmissiblePair`; the cell solve folds `Robots` diagnostics into `Unreachable`'s `JointDiagnostic`; `Guard.Check` routes `Gouge`/`Collision`; the slicer routes `NonManifoldSlice` only for a topologically-broken CLOSED section (a coordinate defect stays kernel `DegenerateInput`); `ArcAlgebra` kerf compensation routes `KerfCollision`; `Ingress.Admit` routes `IngressTranslation` with its site locus; `CuttingData.Of` routes `MachinabilityUnknown`; `ToolMagazine.Schedule` routes `NoToolForOp`; the surface sampler `SampleStalled`; the machine-tool inverse `AxisSingularity`; the voxel lanes `VoxelFault`; orientation `OrientationInfeasible`; the setup scheduler `SetupInfeasible`/`DatumLineageBroken`/`ClampOnMachinedFace`; posting `DialectUnsupported`/`BlockCapExceeded`, its `Parse` arm `ProgramParse`; probing `ProbeOvertravel`; the spec plane `ToleranceUnsatisfiable`/`CapabilityShortfall`/`StackupExceeded`; the derivation orchestrator `RoutingInfeasible`; the wear model `WearEstimateUnfit`; the wire/link/bevel toolpath arms their taper/routing/prep faults; the support search `SupportUnbuildable`; remnant reconcile `RemnantStale`; assembly sequencing `AssemblyPrecedenceCyclic`; the program simulator `EnvelopeExceeded`/`SimulatedOvertravel`; the unfold/brake folds `UnfoldInfeasible`/`BendSequenceInfeasible`/`TonnageExceeded`/`MinBendRadiusViolated`; the weld planes `WeldAccessBlocked`/`HeatInputExceeded`/`WpsUnqualified`.
- Receipt: `FabricationFault` is the typed fault evidence on the `Fin<T>` failure rail; each arm's payload IS the diagnosis — no generic `IFault`, no error-code abstraction, no string-formatted detail masquerading as structure.
- Packages: `Rasm.Element` (`FaultBand` — the federation band-allocation registry, composed), `Rasm.Numerics` (kernel `GeometryFault` band-2400 — composed for shared degenerate input), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`), LanguageExt.Core (`Error`/`Fin`/`Seq`), BCL inbox.
- Growth: a new fabrication failure is one arm at the next free offset (2747+) on its folder's block, carrying a TYPED payload from birth; a shared geometry failure routes the kernel; a receipts-only concern never mints an arm; a new band never mints here.
- Boundary: `FabricationFault` mints ONLY fabrication-specific cases — a parallel band re-casing `DegenerateInput` or a synthesized kernel case is the deleted form; every arm's payload is typed and a `string Detail` arm (frozen or growth) is the dead form this registry retypes; the frozen 2701-2710 wire codes and message prefixes never renumber — a re-band is the named persisted-decode break; faults route through `Fin`/`Validation` rails and exception-style domain control flow is the named defect; the fault-local carriers live HERE and a plane re-minting `FabConcern`/`SourceLocus`/`VoxelBudget` siblings is the deleted form.

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

// ONE axis vocabulary for singularity AND envelope arms (supersedes the rotary-only RotaryAxis).
[SmartEnum<string>]
public sealed partial class MachineAxis {
    public static readonly MachineAxis X = new("x", rotary: false);
    public static readonly MachineAxis Y = new("y", rotary: false);
    public static readonly MachineAxis Z = new("z", rotary: false);
    public static readonly MachineAxis A = new("a", rotary: true);
    public static readonly MachineAxis B = new("b", rotary: true);
    public static readonly MachineAxis C = new("c", rotary: true);

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
// Layer 1 (2701-2710): frozen wire codes, payloads RETYPED, message prefixes byte-identical.
// Layer 2 (2711-2729) + Layer 3 (2730-2746): folder-grouped growth arms; next free offset 2747.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationFault {
    private FabricationFault() { }

    public sealed record NoFit(int Part, Seq<double> TriedRotations) : FabricationFault;
    public sealed record Unreachable(JointDiagnostic Diag, int Target) : FabricationFault;
    public sealed record KerfCollision(Point3d At, double Kerf) : FabricationFault;
    public sealed record OpenLoop(FabConcern Concern, int Primitive) : FabricationFault;
    public sealed record InadmissiblePair((ProcessModality Modality, CutStrategy Strategy) Pair) : FabricationFault;
    public sealed record Gouge(Point3d Point, CutterForm Tool) : FabricationFault;
    public sealed record Collision(ExclusionZone Zone) : FabricationFault;
    public sealed record NonManifoldSlice(int Layer, int OpenChains) : FabricationFault;
    public sealed record StockOverflow(int Unplaced, int Sheets) : FabricationFault;
    public sealed record Nest(NestFault Kind, int Part) : FabricationFault;
    public sealed record IngressTranslation(SourceKind Kind, SourceLocus Locus) : FabricationFault;
    public sealed record MachinabilityUnknown(Material Material, Operation Op) : FabricationFault;
    public sealed record SampleStalled(SurfaceStrategy Strategy, int Iteration) : FabricationFault;
    public sealed record AxisSingularity(MachineAxis Axis, double Angle) : FabricationFault;
    public sealed record VoxelFault(ImplicitOp Op, VoxelBudget Budget) : FabricationFault;
    public sealed record OrientationInfeasible(int Overhangs, double BestScore) : FabricationFault;
    public sealed record SetupInfeasible(int Operation, int TriedSetups) : FabricationFault;
    public sealed record DialectUnsupported(PostDialect Dialect, GNodeKind Node) : FabricationFault;
    public sealed record ProgramParse(int Line, ModalGroup Group) : FabricationFault;
    public sealed record ProbeOvertravel(Point3d At, double Limit) : FabricationFault;
    public sealed record ToleranceUnsatisfiable(FeatureControl Frame, double Achievable) : FabricationFault;
    public sealed record CapabilityShortfall(ProcessKind Process, double Cpk, double Demanded) : FabricationFault;
    public sealed record PartitionDegenerate(PartitionStrategy Strategy, int Sites) : FabricationFault;
    public sealed record NoToolForOp(Operation Op, CutterForm Required) : FabricationFault;
    public sealed record Unsupported3mfExtension(ThreeMfExtension Extension, EgressKind Target) : FabricationFault;
    public sealed record DatumLineageBroken(SetupChain Chain) : FabricationFault;
    public sealed record ClampOnMachinedFace(int Operation, Point3d At) : FabricationFault;
    public sealed record BlockCapExceeded(PostDialect Dialect, int Blocks, int Cap) : FabricationFault;
    public sealed record StackupExceeded(ToleranceChain Chain, double Accumulated, double Bound) : FabricationFault;
    public sealed record RoutingInfeasible(UInt128 ComponentKey, DerivationStage Stage) : FabricationFault;
    public sealed record WearEstimateUnfit(Tool Tool, int Samples) : FabricationFault;
    public sealed record WireTaperExceeded(double AngleDeg, double GuideLimitDeg) : FabricationFault;
    public sealed record LinkBlocked(Point3d From, Point3d To) : FabricationFault;
    public sealed record BevelUnsupported(BevelType Bevel, double AngleDeg) : FabricationFault;
    public sealed record SupportUnbuildable(int Layer, int Region) : FabricationFault;
    public sealed record RemnantStale(ContentKey Key) : FabricationFault;
    public sealed record AssemblyPrecedenceCyclic(int Joints, int Edges) : FabricationFault;
    public sealed record EnvelopeExceeded(MachineAxis Axis, double At, double Limit) : FabricationFault;
    public sealed record SimulatedOvertravel(int Block, MachineAxis Axis, double By) : FabricationFault;
    public sealed record UnfoldInfeasible(int Faces, int Branches) : FabricationFault;
    public sealed record BendSequenceInfeasible(int Bend, int TriedOrders) : FabricationFault;
    public sealed record TonnageExceeded(double RequiredKn, double CapacityKn) : FabricationFault;
    public sealed record MinBendRadiusViolated(int Bend, double RadiusMm, double FloorMm) : FabricationFault;
    public sealed record WeldAccessBlocked(int Joint, double TorchAngleDeg) : FabricationFault;
    public sealed record HeatInputExceeded(int Joint, double KjPerMm, double Cap) : FabricationFault;
    public sealed record WpsUnqualified(EssentialVariable Variable, double Value) : FabricationFault;

    public int Code =>
        Switch(
            noFit:                   static _ => FaultBand.Fabrication + 1,
            unreachable:             static _ => FaultBand.Fabrication + 2,
            kerfCollision:           static _ => FaultBand.Fabrication + 3,
            openLoop:                static _ => FaultBand.Fabrication + 4,
            inadmissiblePair:        static _ => FaultBand.Fabrication + 5,
            gouge:                   static _ => FaultBand.Fabrication + 6,
            collision:               static _ => FaultBand.Fabrication + 7,
            nonManifoldSlice:        static _ => FaultBand.Fabrication + 8,
            stockOverflow:           static _ => FaultBand.Fabrication + 9,
            nest:                    static _ => FaultBand.Fabrication + 10,
            ingressTranslation:      static _ => FaultBand.Fabrication + 11,
            machinabilityUnknown:    static _ => FaultBand.Fabrication + 12,
            sampleStalled:           static _ => FaultBand.Fabrication + 13,
            axisSingularity:         static _ => FaultBand.Fabrication + 14,
            voxelFault:              static _ => FaultBand.Fabrication + 15,
            orientationInfeasible:   static _ => FaultBand.Fabrication + 16,
            setupInfeasible:         static _ => FaultBand.Fabrication + 17,
            dialectUnsupported:      static _ => FaultBand.Fabrication + 18,
            programParse:            static _ => FaultBand.Fabrication + 19,
            probeOvertravel:         static _ => FaultBand.Fabrication + 20,
            toleranceUnsatisfiable:  static _ => FaultBand.Fabrication + 21,
            capabilityShortfall:     static _ => FaultBand.Fabrication + 22,
            partitionDegenerate:     static _ => FaultBand.Fabrication + 23,
            noToolForOp:             static _ => FaultBand.Fabrication + 24,
            unsupported3mfExtension: static _ => FaultBand.Fabrication + 25,
            datumLineageBroken:      static _ => FaultBand.Fabrication + 26,
            clampOnMachinedFace:     static _ => FaultBand.Fabrication + 27,
            blockCapExceeded:        static _ => FaultBand.Fabrication + 28,
            stackupExceeded:         static _ => FaultBand.Fabrication + 29,
            routingInfeasible:       static _ => FaultBand.Fabrication + 30,
            wearEstimateUnfit:       static _ => FaultBand.Fabrication + 31,
            wireTaperExceeded:       static _ => FaultBand.Fabrication + 32,
            linkBlocked:             static _ => FaultBand.Fabrication + 33,
            bevelUnsupported:        static _ => FaultBand.Fabrication + 34,
            supportUnbuildable:      static _ => FaultBand.Fabrication + 35,
            remnantStale:            static _ => FaultBand.Fabrication + 36,
            assemblyPrecedenceCyclic: static _ => FaultBand.Fabrication + 37,
            envelopeExceeded:        static _ => FaultBand.Fabrication + 38,
            simulatedOvertravel:     static _ => FaultBand.Fabrication + 39,
            unfoldInfeasible:        static _ => FaultBand.Fabrication + 40,
            bendSequenceInfeasible:  static _ => FaultBand.Fabrication + 41,
            tonnageExceeded:         static _ => FaultBand.Fabrication + 42,
            minBendRadiusViolated:   static _ => FaultBand.Fabrication + 43,
            weldAccessBlocked:       static _ => FaultBand.Fabrication + 44,
            heatInputExceeded:       static _ => FaultBand.Fabrication + 45,
            wpsUnqualified:          static _ => FaultBand.Fabrication + 46);

    public Error ToError() => Error.New(Code, Message);

    string Message =>
        Switch(
            noFit:                   static f => $"fabrication:no-fit:{f.Part}:{f.TriedRotations.Count}-rotations",
            unreachable:             static f => $"fabrication:unreachable:{f.Diag.Kind.Key}:j{f.Diag.Joint}:{f.Target}",
            kerfCollision:           static f => $"fabrication:kerf-collision:{f.At.X:0.###},{f.At.Y:0.###}:{f.Kerf:0.###}",
            openLoop:                static f => $"fabrication:open-loop:{f.Concern.Key}:{f.Primitive}",
            inadmissiblePair:        static f => $"fabrication:inadmissible-pair:{f.Pair.Modality.Key}-rejects-{f.Pair.Strategy.Key}",
            gouge:                   static f => $"fabrication:gouge:{f.Point.X:0.###},{f.Point.Y:0.###}:{f.Tool.Family.Key}",
            collision:               static f => $"fabrication:collision:{f.Zone.Keepout.Count}-gon",
            nonManifoldSlice:        static f => $"fabrication:non-manifold-slice:layer-{f.Layer}:{f.OpenChains}-open",
            stockOverflow:           static f => $"fabrication:stock-overflow:{f.Unplaced}-unplaced/{f.Sheets}-sheets",
            nest:                    static f => $"fabrication:nest:{f.Kind.Key}:{f.Part}",
            ingressTranslation:      static f => $"fabrication:ingress-translation:{f.Kind.Key}",
            machinabilityUnknown:    static f => $"fabrication:machinability-unknown:{f.Material.Key}:{f.Op.Key}",
            sampleStalled:           static f => $"fabrication:sample-stalled:{f.Strategy.Key}:iter-{f.Iteration}",
            axisSingularity:         static f => $"fabrication:axis-singularity:{f.Axis.Key}:{f.Angle:0.###}",
            voxelFault:              static f => $"fabrication:voxel-fault:{f.Budget.VoxelSize:0.###}mm:{f.Budget.VoxelCap}",
            orientationInfeasible:   static f => $"fabrication:orientation-infeasible:{f.Overhangs}:{f.BestScore:0.###}",
            setupInfeasible:         static f => $"fabrication:setup-infeasible:op-{f.Operation}:{f.TriedSetups}-tried",
            dialectUnsupported:      static f => $"fabrication:dialect-unsupported:{f.Dialect.Key}",
            programParse:            static f => $"fabrication:program-parse:line-{f.Line}",
            probeOvertravel:         static f => $"fabrication:probe-overtravel:{f.At.X:0.###},{f.At.Y:0.###}:{f.Limit:0.###}",
            toleranceUnsatisfiable:  static f => $"fabrication:tolerance-unsatisfiable:{f.Achievable:0.####}",
            capabilityShortfall:     static f => $"fabrication:capability-shortfall:{f.Process.Key}:cpk-{f.Cpk:0.##}<{f.Demanded:0.##}",
            partitionDegenerate:     static f => $"fabrication:partition-degenerate:{f.Sites}-sites",
            noToolForOp:             static f => $"fabrication:no-tool-for-op:{f.Op.Key}:{f.Required.Family.Key}",
            unsupported3mfExtension: static f => $"fabrication:unsupported-3mf-extension:{f.Target.Key}",
            datumLineageBroken:      static f => $"fabrication:datum-lineage-broken",
            clampOnMachinedFace:     static f => $"fabrication:clamp-on-machined-face:op-{f.Operation}:{f.At.X:0.###},{f.At.Y:0.###}",
            blockCapExceeded:        static f => $"fabrication:block-cap-exceeded:{f.Dialect.Key}:{f.Blocks}>{f.Cap}",
            stackupExceeded:         static f => $"fabrication:stackup-exceeded:{f.Accumulated:0.####}>{f.Bound:0.####}",
            routingInfeasible:       static f => $"fabrication:routing-infeasible:{f.Stage.Key}",
            wearEstimateUnfit:       static f => $"fabrication:wear-estimate-unfit:{f.Tool.Key}:{f.Samples}-samples",
            wireTaperExceeded:       static f => $"fabrication:wire-taper-exceeded:{f.AngleDeg:0.##}>{f.GuideLimitDeg:0.##}",
            linkBlocked:             static f => $"fabrication:link-blocked:{f.From.X:0.###},{f.From.Y:0.###}->{f.To.X:0.###},{f.To.Y:0.###}",
            bevelUnsupported:        static f => $"fabrication:bevel-unsupported:{f.Bevel.Key}:{f.AngleDeg:0.##}",
            supportUnbuildable:      static f => $"fabrication:support-unbuildable:layer-{f.Layer}:{f.Region}",
            remnantStale:            static f => $"fabrication:remnant-stale:{f.Key.Kind.Key}",
            assemblyPrecedenceCyclic: static f => $"fabrication:assembly-precedence-cyclic:{f.Joints}-joints:{f.Edges}-edges",
            envelopeExceeded:        static f => $"fabrication:envelope-exceeded:{f.Axis.Key}:{f.At:0.###}>{f.Limit:0.###}",
            simulatedOvertravel:     static f => $"fabrication:simulated-overtravel:block-{f.Block}:{f.Axis.Key}:{f.By:0.###}",
            unfoldInfeasible:        static f => $"fabrication:unfold-infeasible:{f.Faces}-faces:{f.Branches}-branches",
            bendSequenceInfeasible:  static f => $"fabrication:bend-sequence-infeasible:bend-{f.Bend}:{f.TriedOrders}-tried",
            tonnageExceeded:         static f => $"fabrication:tonnage-exceeded:{f.RequiredKn:0.#}>{f.CapacityKn:0.#}kn",
            minBendRadiusViolated:   static f => $"fabrication:min-bend-radius:bend-{f.Bend}:{f.RadiusMm:0.##}<{f.FloorMm:0.##}",
            weldAccessBlocked:       static f => $"fabrication:weld-access-blocked:joint-{f.Joint}:{f.TorchAngleDeg:0.#}deg",
            heatInputExceeded:       static f => $"fabrication:heat-input-exceeded:joint-{f.Joint}:{f.KjPerMm:0.##}>{f.Cap:0.##}",
            wpsUnqualified:          static f => $"fabrication:wps-unqualified:{f.Variable.Key}:{f.Value:0.###}");
}
```
