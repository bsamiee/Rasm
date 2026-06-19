# [RASM_FABRICATION_PROCESS_FAMILY]

The one top-level fabrication discriminant the whole folder de-hardcodes onto: `Process` the `[SmartEnum<string>]` removal-physics axis (`mill`/`turn`/`route`/`laser`/`plasma`/`waterjet`/`additive`/`oxyfuel`/`edm-wire`) and `Machine` the `[SmartEnum<string>]` kinematics-and-holding axis (`mill-3axis`/`mill-5axis`/`router-gantry`/`lathe-2axis`/`lathe-millturn`/`laser-flatbed`/`plasma-table`/`waterjet-5axis`/`fff-cartesian`/`fff-delta`) — two axes over one concept (process = removal physics, machine = kinematics + holding), never a single flattened `mill-3axis-aluminium` enum. Each `Process` row carries the three behavior columns every downstream owner reads as constructor-bound row data: `RemovalModality` (the ONE shared modality vocabulary `process-physics/cut-parameter#CUT_PARAMETER`'s `RemovalBudget` `[Union]` switches on, `toolpath/motion#CAM_MOTION` dispatches `Generate` by, and `posting/program#CUT_PROGRAM` reads for its `PostDialect` default), `KinematicClass` (the motion-topology the `toolpath/motion#CAM_MOTION` generator reads to select a gantry sweep from a chain-target IK drive), and `PostDialect` (the default controller dialect `posting/program#CUT_PROGRAM` renders when the job names no override). Each `Machine` row carries `Processes` (the `Process` set the machine admits), `HoldingClass` (the abstract workholding-mechanism `fixturing/workholding#WORKHOLDING`'s `WorkholderKind.ForHolding` keys the concrete device footprint-shape column off, never a second device list beside the placed `Clamp` record), and `AxisCount` (the controlled-axis count the kinematics and posting owners read). The axis is the policy data the `frontier/owner#FABRICATION_OWNER` `FrontierInput` carries as input state every kernel reads — no new entrypoint, the existing `Run` generated total `Switch` unchanged, never a second frontier discriminant beside `FrontierPolicy`. It composes the Thinktecture `[SmartEnum<string>]` constructor-delegate behavior columns and the generated total `Switch`/`Map` so per-process dispatch is the closed-family fold the row owns; it computes no hash, mints no geometry, and operates on bounded vocabulary at the row.

Wire posture: HOST-LOCAL. The `Process`/`Machine` axis is host-local input state that crosses only the in-process `FrontierInput` seam to the cut-parameter, toolpath, posting, and fixturing kernels — never a browser or peer wire. The `RemovalModality`/`KinematicClass`/`HoldingClass`/`PostDialect` vocabularies and the `Process`/`Machine` axes are host-local `[SmartEnum<string>]` rows that never sit between wire and rail.

## [1]-[INDEX]

| [CLUSTER] | [OWNS] |
| --------- | ------ |
| `[2]-[PROCESS_FAMILY]` | the `RemovalModality`/`KinematicClass`/`HoldingClass`/`PostDialect` shared vocabularies, the `Process` removal-physics axis carrying the modality/kinematic/dialect behavior columns, the `Machine` kinematics-and-holding axis carrying the process-set/holding/axis-count columns, and the `Admits` row-relation the `Machine` reads against its `Processes` set — the one fabrication discriminant `cut-parameter`/`motion`/`posting`/`workholding` read and the `FrontierInput` carries |

## [2]-[PROCESS_FAMILY]

- Owner: `RemovalModality` `[SmartEnum<string>]` the material-removal-physics axis (`subtractive`/`thermal`/`abrasive`/`additive`/`erosion`) carrying a `Contacts` tool-contact column — the ONE modality vocabulary the `process-physics/cut-parameter#CUT_PARAMETER` `RemovalBudget` `[Union]` selects its budget case by, a second parallel modality enum downstream the deleted form; `KinematicClass` `[SmartEnum<string>]` the motion-topology axis (`cartesian-gantry`/`rotary-spindle`/`articulated-arm`/`delta-parallel`) carrying a `MinAxes` column; `HoldingClass` `[SmartEnum<string>]` the workholding-mechanism axis (`mechanical`/`revolved`/`vacuum`/`magnetic`/`bed`) the `fixturing/workholding#WORKHOLDING` `WorkholderKind.ForHolding` keys the footprint-shape column off (the abstract holding mechanism, NOT a device list — the concrete clamp/vise/chuck device footprint is the `WorkholderKind` axis that maps onto this mechanism class); `PostDialect` `[SmartEnum<string>]` the controller-dialect axis (`linuxcnc`/`grbl`/`fanuc`/`haas`/`marlin`/`reprap`/`hypertherm`/`mazak`) carrying its G/M-code RS-274-vs-Marlin family column, the dialect-default `posting/program#CUT_PROGRAM` reads; `Process` `[SmartEnum<string>]` the removal-physics axis (`mill`/`turn`/`route`/`laser`/`plasma`/`waterjet`/`additive`/`oxyfuel`/`edm-wire`) carrying the `RemovalModality`/`KinematicClass`/`PostDialect`-default behavior columns; `Machine` `[SmartEnum<string>]` the kinematics-and-holding axis (`mill-3axis`/`mill-5axis`/`router-gantry`/`lathe-2axis`/`lathe-millturn`/`laser-flatbed`/`plasma-table`/`waterjet-5axis`/`fff-cartesian`/`fff-delta`) carrying its `Processes` admitted-process set, its `HoldingClass`, and its `AxisCount`, with the `Admits` row-relation testing a `Process` against the machine's set.
- Cases: `RemovalModality` rows `subtractive` · `thermal` · `abrasive` · `additive` · `erosion` (5); `KinematicClass` rows `cartesian-gantry` · `rotary-spindle` · `articulated-arm` · `delta-parallel` (4); `HoldingClass` rows `mechanical` · `revolved` · `vacuum` · `magnetic` · `bed` (5); `PostDialect` rows `linuxcnc` · `grbl` · `fanuc` · `haas` · `marlin` · `reprap` · `hypertherm` · `mazak` (8); `Process` rows `mill` · `turn` · `route` · `laser` · `plasma` · `waterjet` · `additive` · `oxyfuel` · `edm-wire` (9), each binding one `RemovalModality` × one `KinematicClass` × one `PostDialect`-default by constructor reference (`mill`→subtractive/cartesian-gantry/linuxcnc, `turn`→subtractive/rotary-spindle/fanuc, `route`→subtractive/cartesian-gantry/grbl, `laser`→thermal/cartesian-gantry/grbl, `plasma`→thermal/cartesian-gantry/hypertherm, `waterjet`→abrasive/cartesian-gantry/fanuc, `additive`→additive/cartesian-gantry/marlin, `oxyfuel`→thermal/cartesian-gantry/hypertherm, `edm-wire`→erosion/cartesian-gantry/fanuc); `Machine` rows `mill-3axis` · `mill-5axis` · `router-gantry` · `lathe-2axis` · `lathe-millturn` · `laser-flatbed` · `plasma-table` · `waterjet-5axis` · `fff-cartesian` · `fff-delta` (10), each binding its `Processes` set × one `HoldingClass` × one `AxisCount` (`mill-3axis`→{mill,route}/mechanical/3, `lathe-millturn`→{turn,mill}/revolved/5, `laser-flatbed`→{laser}/vacuum/2, `fff-cartesian`→{additive}/bed/3), the row the closed cross-product the downstream owners discriminate on.
- Entry: no new entrypoint — the axis is policy data on the existing `frontier/owner#FABRICATION_OWNER` `Run(FrontierPolicy, FrontierInput)`; `public bool Machine.Admits(Process process)` is the total row-relation a generator queries before committing a per-process kernel (the machine's `Processes` set the closed membership test), and `public static Fin<Process> Process.Admit(ReadOnlySpan<char> key)` / `public static Fin<Machine> Machine.Admit(ReadOnlySpan<char> key)` are the span-keyed boundaries admitting external job text through each axis's generated `Validate`, routing the kernel `GeometryFault.DegenerateInput` on an unknown key, never an exception.
- Auto: each `Process` row binds its `RemovalModality`, `KinematicClass`, and `PostDialect`-default once at construction as readonly columns the generated total `Switch`/`Map` dispatch reads — `process-physics/cut-parameter#CUT_PARAMETER`'s `RemovalBudget Budget(Process, Material, Tool, Operation)` reads `process.Modality` and switches the budget case (a milling `mill` row selects the `SubtractiveBudget`, a `laser` row the `ThermalBudget`, a `waterjet` row the `AbrasiveBudget`, an `additive` row the `AdditiveBudget`, an `edm-wire` row the erosion case); `toolpath/motion#CAM_MOTION`'s `Generate((Process, ToolpathKind), …)` reads `process.Kinematics` to select a `cartesian-gantry` sweep from a `rotary-spindle` radial turning pass or an `articulated-arm` chain-target IK drive; `posting/program#CUT_PROGRAM`'s `Post` reads `process.Dialect` as the controller default when the job names no `PostDialect` override. Each `Machine` row binds its `Processes` set, `HoldingClass`, and `AxisCount` once at construction — `Machine.Admits(process)` tests the closed membership before a kernel runs (a `lathe-2axis` admits `turn` and rejects `additive`), `fixturing/workholding#WORKHOLDING`'s `WorkholderKind.ForHolding(machine.Holding)` selects the footprint-shape column (a `revolved` holding selects a concentric-jaw footprint, a `vacuum` holding a planar-pad footprint, the exclusion `Offset` unchanged), and the kinematics and posting owners read `machine.AxisCount`. The whole de-hardcode is the `FrontierInput` carrying the selected `Process`/`Machine` as input state every kernel reads off the row column, never a per-owner baked milling assumption.
- Receipt: the `Process`/`Machine` rows ARE the typed evidence — `process.Modality`/`process.Kinematics`/`process.Dialect` and `machine.Processes`/`machine.Holding`/`machine.AxisCount` are the self-describing axis columns the downstream owners read directly; no generic process-metadata table, no parallel `ProcessTable`/`MachineTable` keyed lookup, each row carries its own behavior columns and the generated `Switch` is the dispatch.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with constructor-delegate behavior columns binding the modality/kinematic/dialect/holding rows, the generated total `Switch`/`Map`/`Validate` per axis — transitive via the `Rasm` ProjectReference, no csproj row), LanguageExt.Core (`Fin`/`Seq`/`Set` — the boundary rail and the `Processes` membership set, transitive via `Rasm`), BCL inbox.
- Growth: a new process is one `Process` row binding its three behavior columns (a `grind` row → abrasive/rotary-spindle/fanuc), the downstream `RemovalBudget`/`Generate`/`Post` dispatch unchanged; a new machine is one `Machine` row binding its process-set/holding/axis-count, never a second fabrication owner; a new removal modality is one `RemovalModality` row plus one `RemovalBudget` case at `process-physics/cut-parameter#CUT_PARAMETER`; a new controller dialect is one `PostDialect` row plus one `Emit` arm at `posting/program#CUT_PROGRAM`; a new holding class is one `HoldingClass` row plus one `Clamp` footprint shape at `fixturing/workholding#WORKHOLDING`; a new kinematic topology is one `KinematicClass` row plus one `Generate` arm at `toolpath/motion#CAM_MOTION`; zero new surface.
- Boundary: the `Process`/`Machine` pair is the ONE fabrication discriminant and a single flattened `mill-3axis-aluminium` enum is the deleted form — process (removal physics) and machine (kinematics + holding) are two axes over one concept, the cross-product the `Machine.Admits` membership owns, never one combinatorial enum; the `RemovalModality` is the ONE shared modality vocabulary `process-physics/cut-parameter#CUT_PARAMETER`'s `RemovalBudget` `[Union]` switches on and a second parallel modality enum minted downstream is the deleted form — one modality axis, one budget union case per row; the `HoldingClass` is the abstract holding-mechanism the `fixturing/workholding#WORKHOLDING` `WorkholderKind.ForHolding` keys the footprint-shape column off and a flattened device list (`bed-clamp`/`lathe-chuck`) on this axis is the rejected form — the mechanism class (`mechanical`/`revolved`/`vacuum`/`magnetic`/`bed`) is the one axis the concrete `WorkholderKind` device footprint maps onto, the device taxonomy living on the `WorkholderKind` axis the `Clamp` reads (the page's own collapse law: a soft-jaw or vacuum kind is one `Clamp` footprint shape), never a second device enum here; the behavior columns are constructor-bound row data read through the generated total `Switch`/`Map` and an external `FrozenDictionary<Process, …>` lookup is the rejected form — the row carries its own columns, a new `Process` case breaks the build until its columns and every `Switch` arm land; the axis is policy data on the existing `frontier/owner#FABRICATION_OWNER` `FrontierInput` and a second frontier discriminant beside `FrontierPolicy` is the deleted form — `Run` is unchanged, the `Process`/`Machine` carried as input state the `FrontierPolicy` case kernels read, never a parallel dispatch key; the axis carries bounded vocabulary and a process selected by a raw `string` magic literal at a call site is the named defect — a `Process`/`Machine`/`RemovalModality`/`HoldingClass` is admitted once through `Admit` and travels as the typed row.

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
public sealed partial class RemovalModality {
    public static readonly RemovalModality Subtractive = new("subtractive", contacts: true);
    public static readonly RemovalModality Thermal = new("thermal", contacts: false);
    public static readonly RemovalModality Abrasive = new("abrasive", contacts: false);
    public static readonly RemovalModality Additive = new("additive", contacts: true);
    public static readonly RemovalModality Erosion = new("erosion", contacts: false);

    public bool Contacts { get; }
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
    public static readonly PostDialect LinuxCnc = new("linuxcnc", rs274: true);
    public static readonly PostDialect Grbl = new("grbl", rs274: true);
    public static readonly PostDialect Fanuc = new("fanuc", rs274: true);
    public static readonly PostDialect Haas = new("haas", rs274: true);
    public static readonly PostDialect Marlin = new("marlin", rs274: false);
    public static readonly PostDialect Reprap = new("reprap", rs274: false);
    public static readonly PostDialect Hypertherm = new("hypertherm", rs274: true);
    public static readonly PostDialect Mazak = new("mazak", rs274: true);

    public bool Rs274 { get; }
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
    Process -->|Modality column| Budget["cut-parameter RemovalBudget union case"]
    Process -->|Kinematics column| Motion["motion (Process, ToolpathKind) generator"]
    Process -->|Dialect default| Post["posting PostDialect render"]
    Machine -->|Holding column| Clamp["workholding Clamp footprint shape"]
    Machine -->|Admits(Process)| Process
    Process -->|input state| Frontier["frontier FrontierInput · Run unchanged"]
    Machine -->|input state| Frontier
```
