# [RASM_FABRICATION_PROCESS_FAMILY]

The one top-level fabrication discriminant the whole folder de-hardcodes onto: `Process` the `[SmartEnum<string>]` removal-physics axis (`mill`/`turn`/`route`/`laser`/`plasma`/`waterjet`/`additive`/`oxyfuel`/`edm-wire`) and `Machine` the `[SmartEnum<string>]` kinematics-and-holding axis (`mill-3axis`/`mill-5axis`/`router-gantry`/`lathe-2axis`/`lathe-millturn`/`laser-flatbed`/`plasma-table`/`waterjet-5axis`/`fff-cartesian`/`fff-delta`/`robot-6axis`/`robot-7axis`/`cobot`, the three articulated rows whose serial-chain FK/IK is the `Toolpath/kinematics#ROBOT_CELL` `Robots`-cell solve the `FabricationInput.Cell` selects) — two axes over one concept (process = removal physics, machine = kinematics + holding), never a single flattened `mill-3axis-aluminium` enum. Each `Process` row carries the three behavior columns every downstream owner reads as constructor-bound row data: `RemovalModality` (the ONE shared modality vocabulary `Process/physics#CUT_PARAMETER`'s `RemovalBudget` `[Union]` switches on, `Toolpath/motion#CAM_MOTION` cross-products against the `CutStrategy` axis to dispatch `Generate`, and `Posting/program#CUT_PROGRAM` reads for its `PostDialect` default), `KinematicClass` (the motion-topology the `Toolpath/motion#CAM_MOTION` generator reads to select a gantry sweep from a chain-target IK drive), and `PostDialect` (the default controller dialect `Posting/program#CUT_PROGRAM` renders when the job names no override). Each `Machine` row carries `Processes` (the `Process` set the machine admits), `HoldingClass` (the abstract workholding-mechanism `Nesting/workholding#WORKHOLDING`'s `WorkholderKind.ForHolding` keys the concrete device footprint-shape column off, never a second device list beside the placed `Clamp` record), and `AxisCount` (the controlled-axis count the kinematics and posting owners read). Beside the two physics/machine axes the page owns the third orthogonal `CutStrategy` `[SmartEnum<string>]` axis (`boundary-pass`/`pocket-clear`/`peck`/`adaptive`/`radial-sweep`/`plunge-dwell`/`helical`/`layer-walk`) — the process-agnostic engagement-pattern the `Toolpath/motion#CAM_MOTION` `Cam.Generate` reads as the SECOND dispatch coordinate beside `Process.RemovalModality`, the factoring that retires the flat 11-row `ToolpathKind` conflation (a strategy lands once and the modality envelopes its move set) — and the total `RemovalModality.Admits(CutStrategy)` row-relation gating the cross-product the way `Machine.Admits(Process)` gates the process set, routing an inadmissible pair (turning's `radial-sweep` on a `cartesian-gantry` laser modality) to `FabricationFault.InadmissiblePair` rather than a silent empty move set. The axis is the policy data the `Process/owner#FABRICATION_OWNER` `FabricationInput` carries as input state every kernel reads — no new entrypoint, the existing `Run` generated total `Switch` unchanged, never a second discriminant beside `FabricationPolicy`. It composes the Thinktecture `[SmartEnum<string>]` constructor-delegate behavior columns and the generated total `Switch`/`Map` so per-process dispatch is the closed-family fold the row owns; it computes no hash, mints no geometry, and operates on bounded vocabulary at the row.

Wire posture: HOST-LOCAL. The `Process`/`Machine` axis is host-local input state that crosses only the in-process `FabricationInput` seam to the cut-parameter, toolpath, posting, and fixturing kernels — never a browser or peer wire. The `RemovalModality`/`KinematicClass`/`HoldingClass`/`PostDialect` vocabularies and the `Process`/`Machine` axes are host-local `[SmartEnum<string>]` rows that never sit between wire and rail.

## [01]-[INDEX]

- [01]-[PROCESS_FAMILY]: owns the `RemovalModality`/`KinematicClass`/`HoldingClass`/`PostDialect`/`CutStrategy` shared vocabularies, the `Process` removal-physics axis carrying the modality/kinematic/dialect behavior columns, the `Machine` kinematics-and-holding axis carrying the process-set/holding/axis-count columns, the `Machine.Admits(Process)` membership and the `RemovalModality.Admits(CutStrategy)` engagement-cross-product row-relations, and `PostDialect` as the constructor-bound dialect FALLBACK the `Posting/program#CUT_PROGRAM` override resolves against — the one fabrication discriminant `cut-parameter`/`motion`/`posting`/`workholding` read and the `FabricationInput` carries.

## [02]-[PROCESS_FAMILY]

- Owner: `RemovalModality` `[SmartEnum<string>]` the material-removal-physics axis (`subtractive`/`thermal`/`abrasive`/`additive`/`erosion`) carrying a `Contacts` tool-contact column and the `Strategies` admitted-`CutStrategy` `Set<CutStrategy>` column the `Admits` engagement-relation reads — the ONE modality vocabulary the `Process/physics#CUT_PARAMETER` `RemovalBudget` `[Union]` selects its budget case by AND the `Toolpath/motion#CAM_MOTION` `Cam.Generate` cross-products against `CutStrategy`, a second parallel modality enum downstream the deleted form; `KinematicClass` `[SmartEnum<string>]` the motion-topology axis (`cartesian-gantry`/`rotary-spindle`/`articulated-arm`/`delta-parallel`) carrying a `MinAxes` column; `HoldingClass` `[SmartEnum<string>]` the workholding-mechanism axis (`mechanical`/`revolved`/`vacuum`/`magnetic`/`bed`) the `Nesting/workholding#WORKHOLDING` `WorkholderKind.ForHolding` keys the footprint-shape column off (the abstract holding mechanism, NOT a device list — the concrete clamp/vise/chuck device footprint is the `WorkholderKind` axis that maps onto this mechanism class); `PostDialect` `[SmartEnum<string>]` the controller-dialect axis (`linuxcnc`/`grbl`/`fanuc`/`haas`/`marlin`/`reprap`/`hypertherm`/`mazak`) carrying its G/M-code RS-274-vs-Marlin family column, its `Comment`/`LineNumbers` render columns the `Posting/program#CUT_PROGRAM` `Emit` fold reads (the ONE `PostDialect` owner both the modality-compatibility axis and the text render read, never a second `PostDialect` in the posting page), AND its `Admits(RemovalModality)` modality-compatibility relation (a `marlin`/`reprap` non-RS-274 dialect admits ONLY the `additive` modality, an RS-274 dialect rejects `additive`) — the constructor-bound dialect FALLBACK `Posting/program#CUT_PROGRAM` renders when the job's `Option<PostDialect>` override is `None`, NOT a welded constant the override cannot displace; `CutStrategy` `[SmartEnum<string>]` the process-agnostic engagement-pattern axis (`boundary-pass`/`pocket-clear`/`peck`/`adaptive`/`radial-sweep`/`plunge-dwell`/`helical`/`layer-walk`) the `Cam.Generate` reads as the second dispatch coordinate, never an axis re-encoding the modality (a thermal contour and a routing contour are ONE `boundary-pass` strategy the modality envelopes differently, not two rows); `Process` `[SmartEnum<string>]` the removal-physics axis (`mill`/`turn`/`route`/`laser`/`plasma`/`waterjet`/`additive`/`oxyfuel`/`edm-wire`) carrying the `RemovalModality`/`KinematicClass`/`PostDialect`-default behavior columns; `Machine` `[SmartEnum<string>]` the kinematics-and-holding axis (`mill-3axis`/`mill-5axis`/`router-gantry`/`lathe-2axis`/`lathe-millturn`/`laser-flatbed`/`plasma-table`/`waterjet-5axis`/`fff-cartesian`/`fff-delta`/`robot-6axis`/`robot-7axis`/`cobot`) carrying its `Processes` admitted-process set, its `HoldingClass`, and its `AxisCount`, with the `Admits` row-relation testing a `Process` against the machine's set — the three articulated robot rows admitting the `mill`/`route`/`additive` end-effector processes whose articulated kinematics the `Toolpath/kinematics#ROBOT_CELL` `Robots`-cell solve realizes off the `FabricationInput.Cell`, the gantry/lathe/bed rows needing no inverse kinematics.
- Cases: `RemovalModality` rows `subtractive` · `thermal` · `abrasive` · `additive` · `erosion` (5), each binding its admitted-`CutStrategy` set by constructor reference (`subtractive`→{boundary-pass,pocket-clear,peck,adaptive,radial-sweep,plunge-dwell,helical}, `thermal`→{boundary-pass,helical}, `abrasive`→{boundary-pass,helical}, `additive`→{layer-walk}, `erosion`→{boundary-pass,plunge-dwell}); `KinematicClass` rows `cartesian-gantry` · `rotary-spindle` · `articulated-arm` · `delta-parallel` (4); `HoldingClass` rows `mechanical` · `revolved` · `vacuum` · `magnetic` · `bed` (5); `PostDialect` rows `linuxcnc` · `grbl` · `fanuc` · `haas` · `marlin` · `reprap` · `hypertherm` · `mazak` (8); `CutStrategy` rows `boundary-pass` (the constant-offset contour boundary passes, the milling-contour/thermal-contour/routing strategy a modality envelopes) · `pocket-clear` (the inward continuous-spiral clearing) · `peck` (the drill peck-cycle point set) · `adaptive` (the constant-engagement HEM trochoidal walk over the straight-skeleton medial axis) · `radial-sweep` (the lathe 1-axis Z-vs-radius diameter sweep, turning rough/finish/face) · `plunge-dwell` (the groove plunge-and-dwell radial cut) · `helical` (the constant-lead helical sweep, lathe threading and helical-bore entry) · `layer-walk` (the additive per-layer perimeter-and-infill walk over the slice set) (8); `Process` rows `mill` · `turn` · `route` · `laser` · `plasma` · `waterjet` · `additive` · `oxyfuel` · `edm-wire` (9), each binding one `RemovalModality` × one `KinematicClass` × one `PostDialect`-default by constructor reference (`mill`→subtractive/cartesian-gantry/linuxcnc, `turn`→subtractive/rotary-spindle/fanuc, `route`→subtractive/cartesian-gantry/grbl, `laser`→thermal/cartesian-gantry/grbl, `plasma`→thermal/cartesian-gantry/hypertherm, `waterjet`→abrasive/cartesian-gantry/fanuc, `additive`→additive/cartesian-gantry/marlin, `oxyfuel`→thermal/cartesian-gantry/hypertherm, `edm-wire`→erosion/cartesian-gantry/fanuc); `Machine` rows `mill-3axis` · `mill-5axis` · `router-gantry` · `lathe-2axis` · `lathe-millturn` · `laser-flatbed` · `plasma-table` · `waterjet-5axis` · `fff-cartesian` · `fff-delta` · `robot-6axis` · `robot-7axis` · `cobot` (13), each binding its `Processes` set × one `HoldingClass` × one `AxisCount` (`mill-3axis`→{mill,route}/mechanical/3, `lathe-millturn`→{turn,mill}/revolved/5, `laser-flatbed`→{laser}/vacuum/2, `fff-cartesian`→{additive}/bed/3, `robot-6axis`→{mill,route,additive}/mechanical/6), the row the closed cross-product the downstream owners discriminate on; the `(RemovalModality, CutStrategy)` cross-product is PARTIAL (5×8 minus the inadmissible cells), the membership the `RemovalModality.Admits` set-containment owns exactly as `Machine.Admits` owns the process membership.
- Entry: no new entrypoint — the axis is policy data on the existing `Process/owner#FABRICATION_OWNER` `Run(FabricationPolicy, FabricationInput)`; `public bool Machine.Admits(Process process)` is the total row-relation a generator queries before committing a per-process kernel (the machine's `Processes` set the closed membership test), `public bool RemovalModality.Admits(CutStrategy strategy)` is the engagement-cross-product membership the `Toolpath/motion#CAM_MOTION` `Cam.Generate` queries before generating a move set (the modality's `Strategies` set the closed test, an inadmissible pair routing `FabricationFault.InadmissiblePair`), `public bool PostDialect.Admits(RemovalModality modality)` is the dialect-modality compatibility the `Posting/program#CUT_PROGRAM` override resolution reads (a non-RS-274 dialect admits only `additive`), and `public static Fin<Process> Process.Admit(ReadOnlySpan<char> key)` / `public static Fin<Machine> Machine.Admit(ReadOnlySpan<char> key)` / `public static Fin<CutStrategy> CutStrategy.Admit(ReadOnlySpan<char> key)` / `public static Fin<PostDialect> PostDialect.Admit(ReadOnlySpan<char> key)` are the span-keyed boundaries admitting external job text through each axis's generated `Validate`, routing the kernel `GeometryFault.DegenerateInput` on an unknown key, never an exception.
- Auto: each `Process` row binds its `RemovalModality`, `KinematicClass`, and `PostDialect`-default once at construction as readonly columns the generated total `Switch`/`Map` dispatch reads — `Process/physics#CUT_PARAMETER`'s `RemovalBudget Budget(Process, Material, Tool, Operation)` reads `process.Modality` and switches the budget case (a milling `mill` row selects the `SubtractiveBudget`, a `laser` row the `ThermalBudget`, a `waterjet` row the `AbrasiveBudget`, an `additive` row the `AdditiveBudget`, an `edm-wire` row the erosion case); `Toolpath/motion#CAM_MOTION`'s `Generate((process.Modality, CutStrategy), …)` reads the `(RemovalModality, CutStrategy)` pair through the generated total `Switch` — `process.Modality` selecting the move-set envelope and the `CutStrategy` selecting the engagement pattern, the `process.Kinematics` topology distinguishing a `cartesian-gantry` gantry sweep from a `rotary-spindle` radial turning pass, the `articulated-arm` serial-chain drive realized not by a CAM-local solver but by the `Toolpath/kinematics#ROBOT_CELL` `RobotProgram.Solve` the `FabricationInput.Cell` presence selects (the `Robots` cell owning the FK/IK) — and queries `process.Modality.Admits(strategy)` first, routing `FabricationFault.InadmissiblePair` when the modality's `Strategies` set rejects the strategy (a `thermal` laser modality rejects `radial-sweep`); `Posting/program#CUT_PROGRAM`'s `Post` resolves the job's `Option<PostDialect>` override against `process.Dialect` as the FALLBACK when the override is `None`, the resolution gating the override against `process.Modality` through `dialect.Admits(modality)` so a thermal process overridden onto a `marlin` additive-only dialect routes `GeometryFault.DegenerateInput`. Each `Machine` row binds its `Processes` set, `HoldingClass`, and `AxisCount` once at construction — `Machine.Admits(process)` tests the closed membership before a kernel runs (a `lathe-2axis` admits `turn` and rejects `additive`), `Nesting/workholding#WORKHOLDING`'s `WorkholderKind.ForHolding(machine.Holding)` selects the footprint-shape column (a `revolved` holding selects a concentric-jaw footprint, a `vacuum` holding a planar-pad footprint, the exclusion `Offset` unchanged), and the kinematics and posting owners read `machine.AxisCount`. The whole de-hardcode is the `FabricationInput` carrying the selected `Process`/`Machine` plus the per-policy `CutStrategy` and the `Option<PostDialect>` override as input state every kernel reads off the row column, never a per-owner baked milling assumption.
- Receipt: the `Process`/`Machine` rows ARE the typed evidence — `process.Modality`/`process.Kinematics`/`process.Dialect` and `machine.Processes`/`machine.Holding`/`machine.AxisCount` are the self-describing axis columns the downstream owners read directly; no generic process-metadata table, no parallel `ProcessTable`/`MachineTable` keyed lookup, each row carries its own behavior columns and the generated `Switch` is the dispatch.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with constructor-delegate behavior columns binding the modality/kinematic/dialect/holding rows, the generated total `Switch`/`Map`/`Validate` per axis — transitive via the `Rasm` ProjectReference, no csproj row), LanguageExt.Core (`Fin`/`Seq`/`Set` — the boundary rail and the `Processes` membership set, transitive via `Rasm`), BCL inbox.
- Growth: a new process is one `Process` row binding its three behavior columns (a `grind` row → abrasive/rotary-spindle/fanuc), the downstream `RemovalBudget`/`Generate`/`Post` dispatch unchanged; a new machine is one `Machine` row binding its process-set/holding/axis-count, never a second fabrication owner; a new removal modality is one `RemovalModality` row binding its admitted-`CutStrategy` set plus one `RemovalBudget` case at `Process/physics#CUT_PARAMETER`; a new engagement strategy is one `CutStrategy` row plus one `Cam.Generate` `Switch` arm at `Toolpath/motion#CAM_MOTION` and its addition to every modality's `Strategies` set that admits it, the generated dispatch breaking the build until the arm lands — a new process reusing an existing engagement adds ZERO `CutStrategy` rows (a routing contour and a thermal contour both read `boundary-pass`); a new controller dialect is one `PostDialect` row binding its admitted-modality relation plus one `Emit` arm at `Posting/program#CUT_PROGRAM`; a new holding class is one `HoldingClass` row plus one `Clamp` footprint shape at `Nesting/workholding#WORKHOLDING`; a new kinematic topology is one `KinematicClass` row plus one `Generate` arm at `Toolpath/motion#CAM_MOTION`; zero new surface.
- Boundary: the `Process`/`Machine` pair is the ONE fabrication discriminant and a single flattened `mill-3axis-aluminium` enum is the deleted form — process (removal physics) and machine (kinematics + holding) are two axes over one concept, the cross-product the `Machine.Admits` membership owns, never one combinatorial enum; the `RemovalModality` is the ONE shared modality vocabulary `Process/physics#CUT_PARAMETER`'s `RemovalBudget` `[Union]` switches on and a second parallel modality enum minted downstream is the deleted form — one modality axis, one budget union case per row; the `CutStrategy` is the ONE process-agnostic engagement axis the `Cam.Generate` cross-products against the modality and the flat 11-row `ToolpathKind` conflating strategy with modality (turn-rough/turn-finish/face/groove/thread/thermal-contour/slice-layer masquerading as strategy siblings beside contour/pocket/drill/trochoidal) is the deleted form — a strategy lands once and the modality envelopes it, the turning rows collapsing onto `radial-sweep`/`plunge-dwell`/`helical`, the thermal contour onto `boundary-pass`, the additive walk onto `layer-walk`, never an axis re-encoding the modality; the `(RemovalModality, CutStrategy)` cross-product is the `RemovalModality.Admits` set-containment relation and a silent empty move set on an inadmissible pair (turning's `radial-sweep` on a cartesian-gantry laser) is the deleted form — the dispatch queries `Admits` and routes `FabricationFault.InadmissiblePair`, the relation gating the cross-product exactly as `Machine.Admits` gates the process set; the `PostDialect` row is the dialect FALLBACK the `Posting/program#CUT_PROGRAM` `Option<PostDialect>` override resolves against and a process welded to one dialect with no override seam is the deleted form — a `mill` row defaults `linuxcnc` but a `fanuc` shop reposting the same toolpath supplies the input override, the resolution gating modality compatibility through `dialect.Admits(modality)`, never a second dialect enum; the `HoldingClass` is the abstract holding-mechanism the `Nesting/workholding#WORKHOLDING` `WorkholderKind.ForHolding` keys the footprint-shape column off and a flattened device list (`bed-clamp`/`lathe-chuck`) on this axis is the rejected form — the mechanism class (`mechanical`/`revolved`/`vacuum`/`magnetic`/`bed`) is the one axis the concrete `WorkholderKind` device footprint maps onto, the device taxonomy living on the `WorkholderKind` axis the `Clamp` reads (the page's own collapse law: a soft-jaw or vacuum kind is one `Clamp` footprint shape), never a second device enum here; the behavior columns are constructor-bound row data read through the generated total `Switch`/`Map` and an external `FrozenDictionary<Process, …>` lookup is the rejected form — the row carries its own columns, a new `Process` case breaks the build until its columns and every `Switch` arm land; the axis is policy data on the existing `Process/owner#FABRICATION_OWNER` `FabricationInput` and a second discriminant beside `FabricationPolicy` is the deleted form — `Run` is unchanged, the `Process`/`Machine`/`CutStrategy` carried as input state the `FabricationPolicy` case kernels read, never a parallel dispatch key; the axis carries bounded vocabulary and a process selected by a raw `string` magic literal at a call site is the named defect — a `Process`/`Machine`/`RemovalModality`/`HoldingClass`/`CutStrategy`/`PostDialect` is admitted once through `Admit` and travels as the typed row.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.ProcessModel;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CutStrategy {
    public static readonly CutStrategy BoundaryPass = new("boundary-pass");
    public static readonly CutStrategy PocketClear = new("pocket-clear");
    public static readonly CutStrategy Peck = new("peck");
    public static readonly CutStrategy Adaptive = new("adaptive");
    public static readonly CutStrategy RadialSweep = new("radial-sweep");
    public static readonly CutStrategy PlungeDwell = new("plunge-dwell");
    public static readonly CutStrategy Helical = new("helical");
    public static readonly CutStrategy LayerWalk = new("layer-walk");
}

[SmartEnum<string>]
public sealed partial class RemovalModality {
    public static readonly RemovalModality Subtractive = new("subtractive", contacts: true,
        Set(CutStrategy.BoundaryPass, CutStrategy.PocketClear, CutStrategy.Peck, CutStrategy.Adaptive, CutStrategy.RadialSweep, CutStrategy.PlungeDwell, CutStrategy.Helical));
    public static readonly RemovalModality Thermal = new("thermal", contacts: false, Set(CutStrategy.BoundaryPass, CutStrategy.Helical));
    public static readonly RemovalModality Abrasive = new("abrasive", contacts: false, Set(CutStrategy.BoundaryPass, CutStrategy.Helical));
    public static readonly RemovalModality Additive = new("additive", contacts: true, Set(CutStrategy.LayerWalk));
    public static readonly RemovalModality Erosion = new("erosion", contacts: false, Set(CutStrategy.BoundaryPass, CutStrategy.PlungeDwell));

    public bool Contacts { get; }
    public Set<CutStrategy> Strategies { get; }

    public bool Admits(CutStrategy strategy) => Strategies.Contains(strategy);
}

[SmartEnum<string>]
public sealed partial class KinematicClass {
    public static readonly KinematicClass CartesianGantry = new("cartesian-gantry", minAxes: 2);
    public static readonly KinematicClass RotarySpindle = new("rotary-spindle", minAxes: 2);
    public static readonly KinematicClass ArticulatedArm = new("articulated-arm", minAxes: 6);
    public static readonly KinematicClass DeltaParallel = new("delta-parallel", minAxes: 3);

    public int MinAxes { get; }
}

[SmartEnum<string>]
public sealed partial class HoldingClass {
    public static readonly HoldingClass Mechanical = new("mechanical");
    public static readonly HoldingClass Revolved = new("revolved");
    public static readonly HoldingClass Vacuum = new("vacuum");
    public static readonly HoldingClass Magnetic = new("magnetic");
    public static readonly HoldingClass Bed = new("bed");
}

[SmartEnum<string>]
public sealed partial class PostDialect {
    public static readonly PostDialect LinuxCnc = new("linuxcnc", rs274: true, comment: "(", lineNumbers: false);
    public static readonly PostDialect Grbl = new("grbl", rs274: true, comment: "(", lineNumbers: false);
    public static readonly PostDialect Fanuc = new("fanuc", rs274: true, comment: "(", lineNumbers: true);
    public static readonly PostDialect Haas = new("haas", rs274: true, comment: "(", lineNumbers: true);
    public static readonly PostDialect Marlin = new("marlin", rs274: false, comment: ";", lineNumbers: false);
    public static readonly PostDialect Reprap = new("reprap", rs274: false, comment: ";", lineNumbers: false);
    public static readonly PostDialect Hypertherm = new("hypertherm", rs274: true, comment: "(", lineNumbers: true);
    public static readonly PostDialect Mazak = new("mazak", rs274: true, comment: "(", lineNumbers: true);

    public bool Rs274 { get; }
    public string Comment { get; }
    public bool LineNumbers { get; }

    public bool Admits(RemovalModality modality) =>
        Rs274 ? modality != RemovalModality.Additive : modality == RemovalModality.Additive;
}

[SmartEnum<string>]
public sealed partial class Process {
    public static readonly Process Mill = new("mill", RemovalModality.Subtractive, KinematicClass.CartesianGantry, PostDialect.LinuxCnc);
    public static readonly Process Turn = new("turn", RemovalModality.Subtractive, KinematicClass.RotarySpindle, PostDialect.Fanuc);
    public static readonly Process Route = new("route", RemovalModality.Subtractive, KinematicClass.CartesianGantry, PostDialect.Grbl);
    public static readonly Process Laser = new("laser", RemovalModality.Thermal, KinematicClass.CartesianGantry, PostDialect.Grbl);
    public static readonly Process Plasma = new("plasma", RemovalModality.Thermal, KinematicClass.CartesianGantry, PostDialect.Hypertherm);
    public static readonly Process Waterjet = new("waterjet", RemovalModality.Abrasive, KinematicClass.CartesianGantry, PostDialect.Fanuc);
    public static readonly Process Additive = new("additive", RemovalModality.Additive, KinematicClass.CartesianGantry, PostDialect.Marlin);
    public static readonly Process Oxyfuel = new("oxyfuel", RemovalModality.Thermal, KinematicClass.CartesianGantry, PostDialect.Hypertherm);
    public static readonly Process EdmWire = new("edm-wire", RemovalModality.Erosion, KinematicClass.CartesianGantry, PostDialect.Fanuc);

    public RemovalModality Modality { get; }
    public KinematicClass Kinematics { get; }
    public PostDialect Dialect { get; }
}

[SmartEnum<string>]
public sealed partial class Machine {
    public static readonly Machine Mill3Axis = new("mill-3axis", Set(Process.Mill, Process.Route), HoldingClass.Mechanical, axisCount: 3);
    public static readonly Machine Mill5Axis = new("mill-5axis", Set(Process.Mill, Process.Route), HoldingClass.Mechanical, axisCount: 5);
    public static readonly Machine RouterGantry = new("router-gantry", Set(Process.Route, Process.Mill), HoldingClass.Vacuum, axisCount: 3);
    public static readonly Machine Lathe2Axis = new("lathe-2axis", Set(Process.Turn), HoldingClass.Revolved, axisCount: 2);
    public static readonly Machine LatheMillTurn = new("lathe-millturn", Set(Process.Turn, Process.Mill), HoldingClass.Revolved, axisCount: 5);
    public static readonly Machine LaserFlatbed = new("laser-flatbed", Set(Process.Laser), HoldingClass.Vacuum, axisCount: 2);
    public static readonly Machine PlasmaTable = new("plasma-table", Set(Process.Plasma, Process.Oxyfuel), HoldingClass.Bed, axisCount: 2);
    public static readonly Machine Waterjet5Axis = new("waterjet-5axis", Set(Process.Waterjet), HoldingClass.Bed, axisCount: 5);
    public static readonly Machine FffCartesian = new("fff-cartesian", Set(Process.Additive), HoldingClass.Bed, axisCount: 3);
    public static readonly Machine FffDelta = new("fff-delta", Set(Process.Additive), HoldingClass.Bed, axisCount: 3);
    public static readonly Machine Robot6Axis = new("robot-6axis", Set(Process.Mill, Process.Route, Process.Additive), HoldingClass.Mechanical, axisCount: 6);
    public static readonly Machine Robot7Axis = new("robot-7axis", Set(Process.Mill, Process.Route, Process.Additive), HoldingClass.Mechanical, axisCount: 7);
    public static readonly Machine Cobot = new("cobot", Set(Process.Route, Process.Additive), HoldingClass.Mechanical, axisCount: 6);

    public Set<Process> Processes { get; }
    public HoldingClass Holding { get; }
    public int AxisCount { get; }

    public bool Admits(Process process) => Processes.Contains(process);
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
public static class ProcessFamily {
    public static Fin<Process> Admit(ReadOnlySpan<char> key) =>
        Process.Validate(key, null, out var p) is { } f
            ? Fin.Fail<Process>(GeometryFault.DegenerateInput($"process:{f.Message}").ToError())
            : Fin.Succ(p!);

    public static Fin<Machine> AdmitMachine(ReadOnlySpan<char> key) =>
        Machine.Validate(key, null, out var m) is { } f
            ? Fin.Fail<Machine>(GeometryFault.DegenerateInput($"machine:{f.Message}").ToError())
            : Fin.Succ(m!);

    public static Fin<CutStrategy> AdmitStrategy(ReadOnlySpan<char> key) =>
        CutStrategy.Validate(key, null, out var s) is { } f
            ? Fin.Fail<CutStrategy>(GeometryFault.DegenerateInput($"cut-strategy:{f.Message}").ToError())
            : Fin.Succ(s!);

    public static Fin<PostDialect> AdmitDialect(ReadOnlySpan<char> key) =>
        PostDialect.Validate(key, null, out var d) is { } f
            ? Fin.Fail<PostDialect>(GeometryFault.DegenerateInput($"post-dialect:{f.Message}").ToError())
            : Fin.Succ(d!);

    public static Fin<(Process Process, Machine Machine)> AdmitPair(ReadOnlySpan<char> processKey, ReadOnlySpan<char> machineKey) =>
        Admit(processKey).Bind(p =>
            AdmitMachine(machineKey).Bind(m =>
                m.Admits(p)
                    ? Fin.Succ((p, m))
                    : Fin.Fail<(Process, Machine)>(GeometryFault.DegenerateInput($"process-machine:{m.Key} rejects {p.Key}").ToError())));
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Process["Process row mill · turn · laser · waterjet · additive · edm-wire"]
    Machine["Machine row mill-3axis · lathe-millturn · laser-flatbed · fff-cartesian"]
    Strategy["CutStrategy boundary-pass · pocket-clear · adaptive · radial-sweep · layer-walk"]
    Process -->|Modality column| Budget["cut-parameter RemovalBudget union case"]
    Process -->|"Modality.Admits(CutStrategy)"| Strategy
    Strategy -->|"(Modality, CutStrategy) pair"| Motion["motion Cam.Generate cross-product"]
    Process -->|Kinematics column| Motion
    Process -->|Dialect FALLBACK| Resolve["Option<PostDialect> override resolution"]
    Resolve -->|"Dialect.Admits(Modality)"| Post["posting PostDialect render"]
    Machine -->|Holding column| Clamp["workholding Clamp footprint shape"]
    Machine -->|Admits(Process)| Process
    Process -->|input state| Fabrication["Fabrication owner · FabricationInput · Run unchanged"]
    Machine -->|input state| Fabrication
    Strategy -->|policy column| Fabrication
```
