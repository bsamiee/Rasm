# [RASM_FABRICATION_CUT_PARAMETER]

The removal-physics policy table: `RemovalParameter` the one frozen `process × material × tool × operation` cross-product owner projecting each row to the modality-discriminated `RemovalBudget` `[Union]` the `Toolpath/motion#CAM_MOTION` and `Toolpath/skeleton#STRAIGHT_SKELETON` generators read as settled scalars. The `Process/family#PROCESS_FAMILY` `Process.RemovalModality` column is the ONE discriminant the `Budget` projection switches on — a subtractive mill row projects a `SubtractiveBudget` (rpm/feed/depth/MRR), a thermal laser/plasma/oxyfuel row a `ThermalBudget` (pierce-time/kerf-width/cut-speed/assist-pressure), an abrasive waterjet row an `AbrasiveBudget` (jet-pressure/abrasive-rate/traverse-speed), and an additive FFF row an `AdditiveBudget` (extrusion-width/layer-height/print-speed/melt-temp). The table is data-driven dispatch keyed by the composite `(Process, Material, Tool, Operation)` smart-enum quad — a new process, material, tool, or operation is one row, never a per-generator magic number, and a new modality is one `RemovalBudget` case plus one projection arm the generated total `Switch` breaks the build until it lands. Each `Material` carries a per-modality `ModalityPhysics` payload column the `RemovalModality` `Switch` reads ONLY for its case (the subtractive surface-speed for milling, the thermal kerf/pierce row for a laser, the abrasive jet row for a waterjet, the additive melt-temp row for an FFF head) so materials own real specific vocabularies without a flat wide record where every modality's columns sit nullable side-by-side. The upstream-owned constitutive scalars — thermal conductivity, specific heat, density — admit as RAW DOUBLES at the `Rasm.Materials/physical-properties#MATERIAL_PROPERTY` AEC-peer boundary (the additive melt-temp and process-only thermal columns are NOT upstream engineering properties and stay in-folder). Quantity-bearing ingress (a feed in `mm/min`, a spindle speed in `rpm`, a depth in `mm`) admits ONCE through the `Rasm.Compute/Symbolic/units#QUANTITIES` UnitsNet boundary; this table stores and projects only the canonical raw doubles the boundary emits, because the toolpath interior operates on raw coordinate/rate doubles and a unit-bearing quantity in a generator signature is the seam violation the Fabrication owner forbids. It composes the `Process/owner#FABRICATION_OWNER` shared vocabulary as the consumer seam; it computes no hash and operates on raw doubles at the interior.

Wire posture: HOST-LOCAL. The `RemovalBudget` scalars cross only the in-process seam to the toolpath generators — never a browser or peer wire. The `Material`/`Tool`/`Operation` vocabularies and the `RemovalBudget` union are host-local types that never sit between wire and rail.

## [01]-[INDEX]

- [01]-[CUT_PARAMETER]: owns the `Material`/`Tool`/`Operation` cross-product axes, the `ModalityPhysics` per-modality payload, the `RemovalBudget` `[Union]` modality-discriminated receipt, and the `RemovalParameter` frozen policy table — the `process × material × tool × operation` rows projecting through the `Process.RemovalModality` switch to the per-modality budget the toolpath generators read.

## [02]-[CUT_PARAMETER]

- Owner: `Material` `[SmartEnum<string>]` the stock-material axis carrying a per-modality `ModalityPhysics` payload column (never a flat 15-column nullable record); `Tool` `[SmartEnum<string>]` the process-discriminated tooling axis (`endmill-3mm`/`endmill-6mm`/`endmill-10mm`/`drill-6mm` milling rows kept, plus `turning-insert`/`laser-head`/`plasma-torch`/`waterjet-nozzle`/`fff-nozzle`) carrying the geometry its process reads as behavior columns; `Operation` `[SmartEnum<string>]` the cut-operation axis (`contour`/`pocket`/`drill`/`trochoidal`) carrying a chip-load and an engagement column; `ModalityPhysics` `[Union]` the closed per-modality material payload (`SubtractivePhysics` surface-speed · `ThermalPhysics` kerf-width/pierce-time/assist-pressure · `AbrasivePhysics` jet-pressure/abrasive-rate · `AdditivePhysics` melt-temp/bond-window) the `RemovalModality` switch reads only for its case; `RemovalBudget` `[Union]` the modality-discriminated raw-scalar receipt (`SubtractiveBudget` rpm/feed/depth/MRR · `ThermalBudget` pierce/kerf/speed/pressure · `AbrasiveBudget` pressure/rate/traverse · `AdditiveBudget` width/layer/speed/temp); `RemovalParameter` the static surface owning the frozen `(Process, Material, Tool, Operation)`-keyed override table and the `Budget` projection from the row columns through the `RemovalModality` switch.
- Cases: `Tool` rows `endmill-3mm` · `endmill-6mm` · `endmill-10mm` · `drill-6mm` · `turning-insert` · `laser-head` · `plasma-torch` · `waterjet-nozzle` · `fff-nozzle` (9); `Operation` rows `contour` · `pocket` · `drill` · `trochoidal` (4); `ModalityPhysics`/`RemovalBudget` cases `Subtractive` · `Thermal` · `Abrasive` · `Additive` (4 each, one-to-one with the `RemovalModality` rows the `Process` carries); the subtractive budget is the cross-product projection `spindle = surfaceSpeed · 1000 / (π · diameter)`, `feed = spindle · flutes · chipLoad`, `depth = engagement · diameter`, `mrr = depth · engagement · diameter · feed` — the existing milling formula UNCHANGED as the `Subtractive` arm; the thermal budget projects pierce-time and assist-pressure off the `ThermalPhysics` row scaled by the cut speed, the abrasive budget the jet-pressure/abrasive-rate off the `AbrasivePhysics` row, the additive budget the extrusion-width/layer-height/print-speed off the `AdditivePhysics` melt-temp row — each a settled formula over the per-modality payload, the table the composite-key admission factory the generators read.
- Entry: `public static RemovalBudget Budget(Process process, Material material, Tool tool, Operation operation)` — the ONE projection discriminating by `Process.RemovalModality` through the generated total `Switch`, total over every modality (each case closed, the projection a pure formula over the payload columns), no rail because a budget is always derivable once the modality and rows are settled; `public static Fin<RemovalBudget> Admit(ReadOnlySpan<char> process, ReadOnlySpan<char> material, ReadOnlySpan<char> tool, ReadOnlySpan<char> operation)` is the span-keyed boundary admitting external text through each axis's generated `Validate`, routing the kernel `GeometryFault.DegenerateInput` on an unknown key.
- Auto: `RemovalParameter.Budget` reads the `Process.RemovalModality` and dispatches the per-modality projection through the generated total `Switch` — the `Subtractive` arm reads the `SubtractivePhysics.SurfaceSpeed` off the `Material` payload, the diameter and flute count off the `Tool` column, the chip load and engagement off the `Operation` column, and projects the four milling scalars exactly as the milling table always did; the `Thermal` arm reads the `ThermalPhysics` kerf-width/pierce-time/assist-pressure and the cut speed; the `Abrasive` arm reads the `AbrasivePhysics` jet-pressure/abrasive-rate; the `Additive` arm reads the `AdditivePhysics` melt-temp/bond-window and the layer geometry. A frozen `(Process, Material, Tool, Operation)`-keyed override is consulted BEFORE the formula so a hand-measured budget defeats the projection. The upstream constitutive scalars (a thermal cut reads its material's conductivity and specific heat to scale the pierce time, an abrasive cut its density) admit as raw doubles through the `Rasm.Materials/physical-properties#MATERIAL_PROPERTY` boundary — `Thermal.ConductivityWMK`/`SpecificHeatJKgK`, `Mechanical.DensityKgM3` — never a `MaterialProperty` type in a generator signature. Quantity text (`"3000 mm/min"`, `"200 m/min"`) admits through the `Rasm.Compute/Symbolic/units#QUANTITIES` boundary canonicalizing to the SI scalar; this table never re-mints a UnitsNet member. The `Toolpath/motion#CAM_MOTION` `Cam.Solve` reads the modality-matched budget case directly — the `SubtractiveBudget.MaterialRemovalRate` is the constant-MRR trochoidal budget, the `ThermalBudget.PierceTime`/`AssistPressure` the laser/plasma pierce conditioning, the `AdditiveBudget.LayerHeight`/`MeltTemp` the FFF slice budget.
- Receipt: the `RemovalBudget` case carries its modality's raw scalars directly — the projected budget IS the evidence the generators read; no generic parameter ledger, no quantity type escaping the boundary, and no budget case carrying a column outside its modality.
- Packages: `Rasm`/Vectors (composed at the consumer seam), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the axes, `[Union]` for `ModalityPhysics`/`RemovalBudget` with the `RemovalModality`-driven total `Switch`), LanguageExt.Core, BCL inbox; UnitsNet composed ONLY through the `Rasm.Compute/Symbolic/units#QUANTITIES` boundary at ingress; the `Rasm.Materials/physical-properties#MATERIAL_PROPERTY` constitutive scalars admit as raw doubles at the peer boundary (a legal upward acyclic AEC-domain-peer read, distinct from the forbidden app-platform `Rasm.Compute` downward edge), never a `MaterialProperty` type in-folder.
- Growth: a new material is one `[SmartEnum<string>]` row plus its `ModalityPhysics` payload column, the cross-product projection unchanged; a new tool/operation is one row plus its behavior columns; a new removal modality is one `RemovalModality` row (in `Process/family`) plus one `ModalityPhysics` case plus one `RemovalBudget` case plus one `Budget` switch arm, the generated dispatch breaking the build until the arm lands; a new budget scalar (coolant pressure, plunge rate) is one field on the modality's budget case plus one projection formula; a fully tabulated per-cell override is one frozen `(Process, Material, Tool, Operation)`-keyed override row consulted before the formula; zero new surface.
- Boundary: `RemovalParameter` is the ONE removal-physics owner and a per-generator magic number is the deleted form — the trochoidal step-over, the contour feed, the laser pierce, the FFF layer all read the `RemovalBudget` case; the budget is the modality-discriminated union and a parallel `ThermalParameter`/`AbrasiveParameter`/`AdditiveParameter` sibling table is the deleted form — one `Budget` projection over the `RemovalModality` switch; the per-modality physics rides the `ModalityPhysics` payload column the switch reads per case and a flat `Material` record with every modality's columns nullable side-by-side is the named density defect (the flag-set deleted form); the table is the cross-product composite-key factory and a `MaterialTable`/`ToolTable`/`OperationTable` sibling triple is the deleted form; the modality discriminant is the `Process.RemovalModality` row from `Process/family#PROCESS_FAMILY` and a second parallel modality enum in this folder is the deleted form — the two pages share the one modality vocabulary; the quantity admission is the one `Rasm.Compute/Symbolic/units#QUANTITIES` boundary and a UnitsNet member spelled in this folder is the seam violation; the constitutive scalars admit as raw doubles at the `Rasm.Materials/physical-properties#MATERIAL_PROPERTY` boundary and a `MaterialProperty` type in a `Cam`/`StraightSkeleton` signature is the named seam violation; the generators read raw `double` budgets and a `Speed`/`RotationalSpeed`/`Length` quantity in a generator signature is the seam violation the Fabrication owner's interior-double law forbids.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.ProcessModel;
using Rasm.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.ProcessPhysics;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Tool {
    public static readonly Tool Endmill3 = new("endmill-3mm", diameter: 3.0, flutes: 2);
    public static readonly Tool Endmill6 = new("endmill-6mm", diameter: 6.0, flutes: 3);
    public static readonly Tool Endmill10 = new("endmill-10mm", diameter: 10.0, flutes: 4);
    public static readonly Tool Drill6 = new("drill-6mm", diameter: 6.0, flutes: 2);
    public static readonly Tool TurningInsert = new("turning-insert", diameter: 0.8, flutes: 1);
    public static readonly Tool LaserHead = new("laser-head", diameter: 0.1, flutes: 0);
    public static readonly Tool PlasmaTorch = new("plasma-torch", diameter: 1.5, flutes: 0);
    public static readonly Tool WaterjetNozzle = new("waterjet-nozzle", diameter: 0.76, flutes: 0);
    public static readonly Tool FffNozzle = new("fff-nozzle", diameter: 0.4, flutes: 0);

    public double Diameter { get; }
    public int Flutes { get; }
}

[SmartEnum<string>]
public sealed partial class Operation {
    public static readonly Operation Contour = new("contour", chipLoad: 0.05, engagement: 1.0);
    public static readonly Operation Pocket = new("pocket", chipLoad: 0.04, engagement: 0.5);
    public static readonly Operation Drill = new("drill", chipLoad: 0.03, engagement: 1.0);
    public static readonly Operation Trochoidal = new("trochoidal", chipLoad: 0.06, engagement: 0.1);

    public double ChipLoad { get; }
    public double Engagement { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModalityPhysics {
    private ModalityPhysics() { }

    public sealed record Subtractive(double SurfaceSpeed) : ModalityPhysics;
    public sealed record Thermal(double KerfWidth, double PierceTime, double AssistPressure, double CutSpeed) : ModalityPhysics;
    public sealed record Abrasive(double JetPressure, double AbrasiveRate, double TraverseSpeed) : ModalityPhysics;
    public sealed record Additive(double MeltTemp, double BondWindow, double ExtrusionWidth) : ModalityPhysics;
}

[SmartEnum<string>]
public sealed partial class Material {
    public static readonly Material Aluminium = new("aluminium", new ModalityPhysics.Subtractive(300.0));
    public static readonly Material MildSteel = new("mild-steel", new ModalityPhysics.Subtractive(90.0));
    public static readonly Material Stainless = new("stainless", new ModalityPhysics.Subtractive(45.0));
    public static readonly Material Acrylic = new("acrylic", new ModalityPhysics.Subtractive(500.0));
    public static readonly Material Plywood = new("plywood", new ModalityPhysics.Subtractive(600.0));
    public static readonly Material MildSteelThermal = new("mild-steel-thermal", new ModalityPhysics.Thermal(KerfWidth: 1.2, PierceTime: 0.5, AssistPressure: 8.0, CutSpeed: 2000.0));
    public static readonly Material StainlessAbrasive = new("stainless-abrasive", new ModalityPhysics.Abrasive(JetPressure: 380.0, AbrasiveRate: 0.45, TraverseSpeed: 150.0));
    public static readonly Material PlaFilament = new("pla-filament", new ModalityPhysics.Additive(MeltTemp: 210.0, BondWindow: 15.0, ExtrusionWidth: 0.45));

    public ModalityPhysics Physics { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RemovalBudget {
    private RemovalBudget() { }

    public sealed record Subtractive(double SpindleRpm, double FeedRate, double DepthOfCut, double MaterialRemovalRate) : RemovalBudget;
    public sealed record Thermal(double PierceTime, double KerfWidth, double CutSpeed, double AssistPressure) : RemovalBudget;
    public sealed record Abrasive(double JetPressure, double AbrasiveRate, double TraverseSpeed) : RemovalBudget;
    public sealed record Additive(double ExtrusionWidth, double LayerHeight, double PrintSpeed, double MeltTemp) : RemovalBudget;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class RemovalParameter {
    static readonly FrozenDictionary<(string Process, string Material, string Tool, string Operation), RemovalBudget> Overrides =
        FrozenDictionary<(string, string, string, string), RemovalBudget>.Empty;

    public static RemovalBudget Budget(Process process, Material material, Tool tool, Operation operation) =>
        Overrides.TryGetValue((process.Key, material.Key, tool.Key, operation.Key), out var measured)
            ? measured
            : process.Modality.Switch(
                state:       (material, tool, operation),
                subtractive: static s => SubtractiveBudget(s.material, s.tool, s.operation),
                thermal:     static s => ThermalBudget(s.material, s.tool),
                abrasive:    static s => AbrasiveBudget(s.material),
                additive:    static s => AdditiveBudget(s.material, s.tool),
                erosion:     static s => ThermalBudget(s.material, s.tool));

    static RemovalBudget SubtractiveBudget(Material material, Tool tool, Operation operation) {
        double surfaceSpeed = material.Physics is ModalityPhysics.Subtractive sub ? sub.SurfaceSpeed : 90.0;
        double spindle = surfaceSpeed * 1000.0 / (Math.PI * tool.Diameter);
        double feed = spindle * tool.Flutes * operation.ChipLoad;
        double depth = operation.Engagement * tool.Diameter;
        double width = operation.Engagement * tool.Diameter;
        return new RemovalBudget.Subtractive(spindle, feed, depth, depth * width * feed);
    }

    static RemovalBudget ThermalBudget(Material material, Tool tool) =>
        material.Physics is ModalityPhysics.Thermal th
            ? new RemovalBudget.Thermal(th.PierceTime, th.KerfWidth, th.CutSpeed, th.AssistPressure)
            : new RemovalBudget.Thermal(PierceTime: 0.5, KerfWidth: tool.Diameter, CutSpeed: 2000.0, AssistPressure: 8.0);

    static RemovalBudget AbrasiveBudget(Material material) =>
        material.Physics is ModalityPhysics.Abrasive ab
            ? new RemovalBudget.Abrasive(ab.JetPressure, ab.AbrasiveRate, ab.TraverseSpeed)
            : new RemovalBudget.Abrasive(JetPressure: 380.0, AbrasiveRate: 0.45, TraverseSpeed: 150.0);

    static RemovalBudget AdditiveBudget(Material material, Tool tool) =>
        material.Physics is ModalityPhysics.Additive ad
            ? new RemovalBudget.Additive(ad.ExtrusionWidth, ad.ExtrusionWidth * 0.5, PrintSpeed: 60.0, ad.MeltTemp)
            : new RemovalBudget.Additive(ExtrusionWidth: tool.Diameter * 1.125, LayerHeight: tool.Diameter * 0.5, PrintSpeed: 60.0, MeltTemp: 210.0);

    // --- [BOUNDARIES] ---------------------------------------------------------------------
    public static Fin<RemovalBudget> Admit(ReadOnlySpan<char> process, ReadOnlySpan<char> material, ReadOnlySpan<char> tool, ReadOnlySpan<char> operation) =>
        Process.Validate(process, null, out var pr) is { } pf ? Fin.Fail<RemovalBudget>(GeometryFault.DegenerateInput($"removal-parameter:process:{pf.Message}").ToError())
        : Material.Validate(material, null, out var m) is { } mf ? Fin.Fail<RemovalBudget>(GeometryFault.DegenerateInput($"removal-parameter:material:{mf.Message}").ToError())
        : Tool.Validate(tool, null, out var t) is { } tf ? Fin.Fail<RemovalBudget>(GeometryFault.DegenerateInput($"removal-parameter:tool:{tf.Message}").ToError())
        : Operation.Validate(operation, null, out var o) is { } of ? Fin.Fail<RemovalBudget>(GeometryFault.DegenerateInput($"removal-parameter:operation:{of.Message}").ToError())
        : Fin.Succ(Budget(pr!, m!, t!, o!));
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Process["Process.RemovalModality"] -->|generated Switch| Budget["RemovalParameter.Budget"]
    Quantity["Quantity text mm/min · m/min"] -->|Compute units boundary| Canonical["Canonical raw double"]
    Material["Material.Physics (ModalityPhysics payload)"] -->|per-case read| Budget
    Constitutive["Materials Thermal/Mechanical scalars"] -->|raw doubles at peer boundary| Budget
    Canonical -->|axis columns| Budget
    Budget -->|Subtractive rpm/feed/depth/MRR| Cam["CAM_MOTION milling"]
    Budget -->|Thermal pierce/kerf/speed| Thermal["CAM_MOTION thermal-contour"]
    Budget -->|Additive width/layer/temp| Additive["Toolpath/slicing layers"]
```
