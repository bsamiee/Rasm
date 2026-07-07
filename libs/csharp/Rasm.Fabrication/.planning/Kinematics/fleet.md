# [RASM_FABRICATION_MACHINE_FLEET]

The machine-capability registry: `Fleet` the static surface owning the ONE capability join `Capable(AdmittedComponent, MachineFleet) → Seq<MachineMatch>` — every shop machine instance crossed with its admitted `ProcessKind` set, each pair scored through six typed checks (work-envelope containment, axis-count/rotary-topology admission, spindle band, tool-pocket capacity, material admission, tolerance-grade capability), feasible pairs ranked by the fleet's scoring policy. The INSTANCE-vs-AXIS split is law: `Process/family#PROCESS_FAMILY` `Machine` is the KIND axis (`mill-5axis`, `robot-6axis`, `press-brake-cnc` — kinematics + holding + topology); `MachineInstance` is the shop's PHYSICAL machine binding one `Machine` row plus the measured per-instance columns (envelope travels, spindle power/speed, smallest admitted cutter, magazine + pocket census, material allowlist, declared best IT grade, hourly rate, the optional `RobotCell` descriptor for `articulated-arm` rows) — instance DATA over the family AXIS, never a parallel machine enum. The match row is the routing feed `Process/derivation` consumes: one `MachineMatch` per feasible `(instance, process)` pair carrying the `CapabilityCheck` flag set, the envelope headroom, the grade margin, and the composite score — an EMPTY `Seq<MachineMatch>` is a VALID verdict, never a fault minted here; `Process/derivation` routes `FabricationFault.RoutingInfeasible` 2730 when the empty match exhausts its routing stage.

The join reads real landed surfaces, no speculative columns: material identity resolves from the component's head composition layer (else the `material` property row) through `Material.Validate` and the MATERIAL truth is the physics map itself — a process is material-admissible exactly when `material.Physics.Find(process.Modality)` holds an entry, so the fleet never re-encodes machinability (`Tooling/cuttingdata` owns measured cells); the spindle law projects the subtractive surface-speed floor onto the instance's smallest cutter (`rpm = v·1000/(π·d_min)` must fit `SpindleMaxRpm`) while non-rotating modalities gate on the demanded power row alone; envelope containment is the sorted-extent compare (part and envelope diagonals sorted descending, headroom the minimum per-rank slack — any 90° table re-orientation admitted; the kernel `Bounds.Principal` OBB is the recorded refinement row); the component bound unions profile `Loop.Bound()` boxes with the kernel mesh bound at the ONE `Analyze.Run`/`AnalysisQuery.Bounds(Bounds.AxisAligned)` seam. Demand rows ride the component's `Quantities` bag under the typed `DemandKey` vocabulary (`demand:min-axes`/`demand:distinct-tools`/`demand:spindle-kw`/`demand:it-grade`, each row carrying its fallback) — `Ingress/element` writes the bag, this page owns the reading vocabulary. Grade capability starts from the instance's DECLARED best IT grade: the `Spec/capability` `Capability.Gate` Cpk history defeats the declared column (the `CapabilityVerdict` input-carry on `owner#atoms` is the standing shape); rotary-topology admission gates on the family `Machine.Topology` ROW — the per-move TCP/RTCP inverse truth stays `Kinematics/machine`, and articulated-arm reach truth stays `Kinematics/cell#ROBOT_CELL`'s `Program.Errors` fold — fleet's envelope gate is the coarse plan-time filter, never a re-derived IK.

Wire posture: HOST-LOCAL. `MachineInstance`/`MachineMatch` rows cross only the in-process seam to the derivation orchestrator, the setups scheduler, and the estimation rate read — never a browser or peer wire; no row sits between wire and rail.

## [01]-[INDEX]

- [01]-[MACHINE_FLEET]: owns the `DemandKey` demand-row vocabulary, the `MachineInstance` shop-machine record over the family `Machine` axis, the `FleetPolicy`/`MachineFleet` registry, the `CapabilityCheck` six-flag verdict, the `MachineMatch` scored `(instance, process)` row, and the `Fleet.Capable` join + `AdmitInstance` span-keyed registry boundary — the plan-time machine filter `Process/derivation`'s fleet-match stage consumes.

## [02]-[MACHINE_FLEET]

- Owner: `DemandKey` `[SmartEnum<string>]` the well-known `Quantities`-bag demand rows (`demand:min-axes`/`demand:distinct-tools`/`demand:spindle-kw`/`demand:it-grade`), each carrying its `Fallback` and the polymorphic `Read(Map<string,double>)` projection; `MachineInstance` the physical-machine record — `Id`, the family `Machine` `Kind`, the measured `Envelope` travels, `SpindlePowerKw`/`SpindleMaxRpm`/`MinToolDiameterMm`, the `Tooling/magazine#TOOL_MAGAZINE` `Magazine` row + `Pockets` override (`0` reads `Magazine.SlotCount`), the `Materials` allowlist (`empty` = open, physics decides), the declared `ItGrade`, the `HourlyRate` cost column `Verify/estimation` reads, and the `Option<RobotCell>` cell descriptor; `FleetPolicy` the scoring weights (headroom/grade/parsimony); `MachineFleet` the shop registry (`Instances` + `Policy` — the ranking policy IS shop data); `CapabilityCheck` the six-flag typed verdict with the derived `Feasible` conjunction; `MachineMatch` the scored `(Instance, Process, Checks, EnvelopeHeadroom, GradeMargin, Score)` row; `Fleet` the static surface owning `Capable` and `AdmitInstance`.
- Cases: the six checks — `Envelope` (sorted-extent headroom ≥ 0) · `Topology` (`AxisCount` ≥ demanded axes, a ≥5-axis demand requiring a `Rotary` topology row or a present cell) · `Spindle` (power row + subtractive rpm-band law) · `ToolCapacity` (pocket census ≥ demanded distinct tools) · `Material` (allowlist ∩ physics-map membership) · `Grade` (declared IT ≤ demanded IT); `DemandKey` rows 4; the pair enumeration is `fleet.Instances × instance.Kind.Processes` FILTERED by the physics map before checking — a process whose modality the material never carries produces no row at all, so the match set is physics-true by construction.
- Entry: `public static Fin<Seq<MachineMatch>> Capable(AdmittedComponent component, MachineFleet fleet)` — the ONE join; `Fin<T>` routes `GeometryFault.DegenerateInput` for an unresolvable material key, a material-free component, or a geometry-free component (no profile and no mesh — the bound is unformable); the EMPTY feasible set returns `Succ` (a verdict, not a failure); `public static Fin<MachineInstance> AdmitInstance(...)` is the span-keyed registry boundary admitting shop-configuration text through `ProcessFamily.AdmitMachine` + `ToolMagazine.AdmitMagazine` + `Material.Validate`, never an exception.
- Auto: `Capable` binds the material resolve, the component bound, then folds instances — each admitted process filtered by `material.Physics.Find(process.Modality)`, each surviving pair scored by `Match`: headroom from the sorted-extent compare, demanded axes/grade/tools/power read through `DemandKey.Read` fallbacks, spindle fitted by the surface-speed floor over `MinToolDiameterMm`, the score `HeadroomWeight·headroom + GradeWeight·gradeMargin − ParsimonyWeight·AxisCount` (the parsimony term prefers the SMALLEST capable machine — a 3-axis job never ranks a trunnion first), feasible rows ordered descending. `Process/derivation` consumes the ranked rows at its fleet-match stage and routes `RoutingInfeasible` 2730 on exhaustion; `Fixturing/setups` reads the matched instance's holding/axis context; `Verify/estimation` reads `HourlyRate` off the matched instance.
- Receipt: the `MachineMatch` IS the typed capability evidence — the six-flag `CapabilityCheck`, the headroom/margin scalars, and the composite score; no bare bool, no filtered instance list without its verdict trail, no generic capability ledger.
- Packages: `Process/family#PROCESS_FAMILY` (`Machine`/`ProcessKind`/`ProcessModality` axes — composed), `Process/physics#CUT_PARAMETER` (`Material` identity map + `ModalityPhysics.Subtractive` floor — the material-admission truth), `Tooling/magazine#TOOL_MAGAZINE` (`Magazine` slot axis — the pocket census), `Process/owner#FABRICATION_OWNER` (`AdmittedComponent`/`Loop.Bound` — composed), `Kinematics/cell#ROBOT_CELL` (`RobotCell` descriptor — the articulated-arm column), kernel `Analysis/query` (`Analyze.Run` + `AnalysisQuery.Bounds(Bounds.AxisAligned)` → `BoundingBox` — the ONE mesh-bound seam), `Rhino.Geometry` (`BoundingBox` — composed), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox; deferred-read seams: `Kinematics/machine` (per-move rotary inverse + TCP/RTCP admission — fleet gates on the family `Topology` row only), `Spec/capability` (`Capability.Gate` Cpk history defeats the declared `ItGrade` column).
- Growth: a new capability dimension is one `CapabilityCheck` flag + one `Match` term; a new demand row is one `DemandKey` row with its fallback; per-instance scheduling calendars and load factors are instance columns the derivation scheduler reads; the OBB envelope refinement is one `MeshBound` row onto `Bounds.Principal`; a persisted shop registry is one ingress arm over `AdmitInstance`; zero new surface.
- Boundary: `MachineInstance` is instance DATA over the family `Machine` AXIS and a parallel machine enum, a flattened `mill-5axis-shop2` row, or a fleet-local topology column is the deleted form; the empty match set is a VERDICT and a fleet-minted fault arm is the rejected form — `RoutingInfeasible` 2730 is `Process/derivation`'s; the match is the typed `MachineMatch` and a bare-bool filter is the deleted form; demand reads go through the `DemandKey` rows and a raw `"demand:*"` string at a call site is the named defect; material admissibility is the physics map join and a fleet-local machinability table is the deleted form; the mesh bound is the ONE `Analyze.Run` seam and a hand-rolled vertex fold is the rejected re-derivation; articulated-arm reach truth is the cell's `Program.Errors` fold and grade truth migrates to `Capability.Gate` when it lands — the declared columns are seeds, never second oracles.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Analysis;                      // Analyze.Run + AnalysisQuery.Bounds — the ONE kernel mesh-bound seam
using Rasm.Fabrication.Process;           // AdmittedComponent · Machine/ProcessKind/ProcessModality · Material physics rows
using Rasm.Fabrication.Tooling;           // Magazine — the pocket census axis
using Rasm.Meshing;                       // MeshSpace
using Rasm.Numerics;                      // GeometryFault band-2400
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Kinematics;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// The demand-row vocabulary over the AdmittedComponent Quantities bag: Ingress/element writes the rows,
// this axis owns the keys and fallbacks — a raw "demand:*" string at a call site is the named defect.
[SmartEnum<string>]
public sealed partial class DemandKey {
    public static readonly DemandKey MinAxes = new("demand:min-axes", fallback: 3.0);
    public static readonly DemandKey DistinctTools = new("demand:distinct-tools", fallback: 1.0);
    public static readonly DemandKey SpindleKw = new("demand:spindle-kw", fallback: 0.0);
    public static readonly DemandKey ItGrade = new("demand:it-grade", fallback: 12.0);

    public double Fallback { get; }

    public double Read(Map<string, double> quantities) => quantities.Find(Key).IfNone(Fallback);
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// Instance DATA over the family Machine AXIS: the shop's physical machine with measured envelope, spindle,
// pocket, allowlist, declared-grade, and rate columns; Cell rides only articulated-arm rows.
public sealed record MachineInstance(
    string Id,
    Machine Kind,
    BoundingBox Envelope,
    double SpindlePowerKw,
    double SpindleMaxRpm,
    double MinToolDiameterMm,
    Magazine Magazine,
    int Pockets,
    Set<Material> Materials,
    int ItGrade,
    double HourlyRate,
    Option<RobotCell> Cell) {
    public int PocketCount => Pockets > 0 ? Pockets : Magazine.SlotCount;
}

public sealed record FleetPolicy(double HeadroomWeight, double GradeWeight, double ParsimonyWeight) {
    public static readonly FleetPolicy Canonical = new(HeadroomWeight: 1.0, GradeWeight: 1.0, ParsimonyWeight: 0.5);
}

public sealed record MachineFleet(Seq<MachineInstance> Instances, FleetPolicy Policy);

public readonly record struct CapabilityCheck(bool Envelope, bool Topology, bool Spindle, bool ToolCapacity, bool Material, bool Grade) {
    public bool Feasible => Envelope && Topology && Spindle && ToolCapacity && Material && Grade;
}

// The typed scored verdict: one row per feasible (instance, process) pair — never a bare bool.
public sealed record MachineMatch(
    MachineInstance Instance, ProcessKind Process, CapabilityCheck Checks, double EnvelopeHeadroom, double GradeMargin, double Score);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Fleet {
    const string MaterialProperty = "material";

    // The ONE capability join: instances × admitted processes, physics-filtered, six checks per pair, feasible
    // rows scored and ranked. The EMPTY set is a VALID verdict — derivation routes RoutingInfeasible 2730.
    public static Fin<Seq<MachineMatch>> Capable(AdmittedComponent component, MachineFleet fleet) =>
        DemandMaterial(component).Bind(material => Bound(component).Map(part =>
            fleet.Instances
                .Bind(instance => toSeq(instance.Kind.Processes)
                    .Filter(process => material.Physics.Find(process.Modality).IsSome)
                    .Map(process => Match(component, part, instance, process, material, fleet.Policy)))
                .Filter(static m => m.Checks.Feasible)
                .OrderByDescending(static m => m.Score).ToSeq()));

    static MachineMatch Match(
        AdmittedComponent component, BoundingBox part, MachineInstance instance, ProcessKind process, Material material, FleetPolicy policy) {
        double headroom = Headroom(part, instance.Envelope);
        int demandedAxes = (int)DemandKey.MinAxes.Read(component.Quantities);
        int demandedGrade = (int)DemandKey.ItGrade.Read(component.Quantities);
        var checks = new CapabilityCheck(
            Envelope: headroom >= 0.0,
            Topology: instance.Kind.AxisCount >= demandedAxes && (demandedAxes < 5 || instance.Kind.Topology.Rotary || instance.Cell.IsSome),
            Spindle: SpindleFit(instance, material, process, component),
            ToolCapacity: instance.PocketCount >= (int)DemandKey.DistinctTools.Read(component.Quantities),
            Material: instance.Materials.IsEmpty || instance.Materials.Contains(material),
            Grade: instance.ItGrade <= demandedGrade);
        double gradeMargin = demandedGrade - instance.ItGrade;
        return new MachineMatch(instance, process, checks, headroom, gradeMargin,
            Score: policy.HeadroomWeight * headroom + policy.GradeWeight * gradeMargin - policy.ParsimonyWeight * instance.Kind.AxisCount);
    }

    // Subtractive spindle law: the material's surface-speed floor at the smallest admitted cutter must fit the
    // spindle band (rpm = v·1000/(π·d)); non-rotating modalities gate on the demanded power row alone.
    static bool SpindleFit(MachineInstance instance, Material material, ProcessKind process, AdmittedComponent component) =>
        DemandKey.SpindleKw.Read(component.Quantities) <= instance.SpindlePowerKw
        && (process.Modality != ProcessModality.Subtractive
            || material.Physics.Find(ProcessModality.Subtractive)
                .Map(p => p is ModalityPhysics.Subtractive s
                    && s.SurfaceSpeed * 1000.0 / (Math.PI * Math.Max(instance.MinToolDiameterMm, 0.1)) <= instance.SpindleMaxRpm)
                .IfNone(false));

    // Sorted-extent containment: both diagonals sorted descending, headroom the minimum per-rank slack — any
    // 90-degree table re-orientation admitted; the kernel Bounds.Principal OBB is the recorded refinement row.
    static double Headroom(BoundingBox part, BoundingBox envelope) {
        double[] p = Extents(part), e = Extents(envelope);
        return Enumerable.Range(0, 3).Min(i => e[i] - p[i]);
    }

    static double[] Extents(BoundingBox box) => [.. new[] { box.Diagonal.X, box.Diagonal.Y, box.Diagonal.Z }.OrderByDescending(static v => v)];

    static Fin<BoundingBox> Bound(AdmittedComponent component) {
        BoundingBox box = BoundingBox.Empty;
        foreach (Loop loop in component.Profiles) box.Union(loop.Bound());
        component.Mesh.Bind(MeshBound).IfSome(mb => box.Union(mb));
        return box.IsValid ? Fin.Succ(box) : Fin.Fail<BoundingBox>(GeometryFault.DegenerateInput($"fleet:bound:{component.RepresentationKey}").ToError());
    }

    // The ONE kernel mesh-bound seam: Analyze.Run over AnalysisQuery.Bounds(Bounds.AxisAligned) — never a vertex fold.
    static Option<BoundingBox> MeshBound(MeshSpace mesh) =>
        Analyze.Run<MeshSpace, BoundingBox>(AnalysisQuery.Bounds(Bounds.AxisAligned), Seq1(mesh)).ToOption().Bind(static boxes => boxes.HeadOrNone());

    // The component's material identity: the head composition layer's key, else the material property row —
    // boundary-mapped through Material.Validate; a component with no resolvable material is degenerate input.
    static Fin<Material> DemandMaterial(AdmittedComponent component) =>
        component.Layers.HeadOrNone().Map(static l => l.MaterialKey).BiBind(Some, () => component.Properties.Find(MaterialProperty)).Match(
            Some: key => Material.Validate(key, null, out var m) is { } f
                ? Fin.Fail<Material>(GeometryFault.DegenerateInput($"fleet:material:{f.Message}").ToError())
                : Fin.Succ(m!),
            None: () => Fin.Fail<Material>(GeometryFault.DegenerateInput($"fleet:material:none:{component.RepresentationKey}").ToError()));

    // --- [BOUNDARIES] — shop-configuration text admits ONCE through the span-keyed registry boundary --------------------------------------------------
    public static Fin<MachineInstance> AdmitInstance(ReadOnlySpan<char> machineKey, ReadOnlySpan<char> magazineKey, string id, BoundingBox envelope,
        double spindleKw, double maxRpm, double minToolDiameterMm, int pockets, Seq<string> materialKeys, int itGrade, double hourlyRate,
        Option<RobotCell> cell) =>
        ProcessFamily.AdmitMachine(machineKey).Bind(kind =>
            ToolMagazine.AdmitMagazine(magazineKey).Bind(magazine =>
                Materials(materialKeys).Map(materials =>
                    new MachineInstance(id, kind, envelope, spindleKw, maxRpm, minToolDiameterMm, magazine, pockets, materials, itGrade, hourlyRate, cell))));

    static Fin<Set<Material>> Materials(Seq<string> keys) =>
        keys.Traverse(k => Material.Validate(k, null, out var m) is { } f
            ? Fin.Fail<Material>(GeometryFault.DegenerateInput($"fleet:material:{f.Message}").ToError())
            : Fin.Succ(m!)).Map(toSet);
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Component["AdmittedComponent geometry · layers · quantities bag"] -->|DemandKey rows + material resolve| Capable["Fleet.Capable"]
    Registry["MachineFleet instances × scoring policy"] --> Capable
    Physics["physics Material.Physics map"] -->|"Find(process.Modality) filter"| Capable
    Family["family Machine.Processes · Topology · AxisCount"] --> Capable
    Magazine["Tooling Magazine.SlotCount pocket census"] --> Capable
    Kernel["Analyze.Run Bounds.AxisAligned mesh bound"] -->|ONE seam| Capable
    Capable -->|"six checks + score per (instance, process)"| Matches["Seq&lt;MachineMatch&gt; ranked"]
    Matches -->|fleet-match stage| Derivation["Process/derivation (empty ⇒ RoutingInfeasible 2730)"]
    Matches -->|holding/axis context| Setups["Fixturing/setups"]
    Matches -->|HourlyRate| Estimation["Verify/estimation"]
    Capability["Spec/capability Gate"] -.->|Cpk history defeats declared ItGrade| Registry
```
