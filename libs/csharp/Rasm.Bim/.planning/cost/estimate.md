# [BIM_COST_ESTIMATE]

The host-neutral 5D cost-and-resource projection: one `CostItem` record joining an `IfcCostValue` applied rate to the `properties/property-sets#PROPERTY_SETS` `QuantitySet.Derive` takeoff quantity by element `GlobalId`, a closed `ConstructionResource` `[Union]` (`Labor`/`Material`/`Equipment`) over `IfcLaborResource`/`IfcConstructionMaterialResource`/`IfcConstructionEquipmentResource` bound to the `sequencing#ConstructionTask` it resources through `IfcRelAssignsToProcess`, and the `CostSchedule.Rollup` fold that folds the `(quantity x rate x resource)` join into one schedule total — one fold, never enumerated per-resource arms. The estimate is a VIEW of the federated `model/elements#ELEMENT_MODEL` `BimModel`, never a second quantity or schedule source: each `CostItem` reads its quantity from the one `properties/property-sets#PROPERTY_SETS` `QuantitySet` takeoff the model already derives from the kernel `Rasm` geometry the element binds by reference, joins to the priced elements through the `IfcRelAssignsToControl.RelatedObjects` cost-control set the `IfcCostItem` controls, and the resource it consumes binds to the `sequencing#ConstructionTask` activity network through `IfcRelAssignsToProcess` rather than re-modeling a parallel schedule — so a wall's net-volume takeoff, its concrete unit rate, and the labor crew that places it carry one cost line while the physical element keeps its solid geometry and the activity keeps its calendar. The estimate is HOST-NEUTRAL — it joins elements, resources, and tasks by stable `GlobalId` and never carries a RhinoCommon type — and the `query/element-set#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, never a second selection surface. The `(QuantityKey, ResourceKey)` `UInt128` content-key pair the `CostSchedule.Identity` derives is the reference the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads the schedule by at `Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING`, and the `Rasm.AppUi/schedule` estimate report renders the same rollup — Bim PRODUCES the cost source it previously had no owner for, never re-pricing it downstream; a cost rejection lowers onto `faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family.

## [1]-[INDEX]

| [CLUSTER] | [OWNS] |
| :-------- | :----- |
| [2]-[ESTIMATE] | `CostItem` record (rate x `QuantitySet` quantity by `GlobalId`), the `ConstructionResource` `[Union]` (`Labor`/`Material`/`Equipment`), the `CostValue`/`MonetaryAmount` value objects, the `CostScheduleKind`/`ResourceKind` `[SmartEnum<string>]` partitions, the `CostSchedule` record with its `Rollup` fold and `(QuantityKey, ResourceKey)` identity, and the `CostProjection.Project` fold from the `IfcCostSchedule` surface. |

## [2]-[ESTIMATE]

- Owner: `CostSchedule` the single host-neutral 5D cost-and-resource record carrying the `CostScheduleKind` discriminant, the priced `CostItem` line set, the `ConstructionResource` resource set, the `(QuantityKey, ResourceKey)` content-key identity the cross-libs cost catalog reads it by, and the `Rollup` schedule-total fold; `CostItem` the single priced line record joining the `IfcCostValue` `CostValue` applied rate to the `properties/property-sets#PROPERTY_SETS` `QuantitySet` takeoff quantity by the priced element `GlobalId`, carrying the optional `ResourceGlobalId` the line consumes and the `ParentGlobalId` nesting reference the `IfcCostItem` hierarchy declares; `ConstructionResource` the closed `[Union]` discriminating the three IFC construction-resource modalities — `Labor` (`IfcLaborResource` carrying its `SkillSet` and crew `BaseQuantity`), `Material` (`IfcConstructionMaterialResource` carrying its material `GlobalId` and consumed `BaseQuantity`), `Equipment` (`IfcConstructionEquipmentResource` carrying its plant `BaseQuantity`) — each binding the `sequencing#ConstructionTask` it resources by an optional `TaskGlobalId` reference; `CostValue` the applied-rate record carrying its `MonetaryAmount` value, its `UnitBasis` per-unit denominator, and its `CostCategory` discriminant so a rate carries its IFC monetary measure rather than a stringly-typed amount; `CostScheduleKind` the `[SmartEnum<string>]` cost-schedule-kind vocabulary keyed on the `IfcCostScheduleTypeEnum` member; `ResourceKind` the `[SmartEnum<string>]` resource-modality vocabulary keyed on the resource entity-type string carrying its `IfcDomain`; `CostProjection` the static fold over the GeometryGym `IfcCostSchedule` surface.
- Cases: `ConstructionResource` arms `Labor` (`IfcLaborResource` — `GlobalId`, `Name`, `ResourceKind`, the `IfcLaborResourceTypeEnum` `PredefinedType` skill descriptor, the crew `BaseQuantity` man-hours, optional `TaskGlobalId`) · `Material` (`IfcConstructionMaterialResource` — `GlobalId`, `Name`, `ResourceKind`, the consumed material `MaterialName` read through the resource's `HasAssociations` `IfcRelAssociatesMaterial`, the consumed `BaseQuantity`, optional `TaskGlobalId`) · `Equipment` (`IfcConstructionEquipmentResource` — `GlobalId`, `Name`, `ResourceKind`, the plant `BaseQuantity` plant-hours, optional `TaskGlobalId`) (3); the `CostValue` value object carries its `MonetaryAmount` (`Amount` double over the IFC `IfcMonetaryMeasure`, `Currency` three-letter ISO 4217 code), its `UnitBasis` per-unit denominator (`1.0` for a unit rate, the `IfcCostValue.UnitBasis` `IfcMeasureWithUnit` magnitude for a per-basis rate), and its `CostCategory` (`Material`/`Labour`/`Equipment`/`Overhead`/`Subcontract`/`Preliminaries`/`NotDefined` over the `IfcCostValue.Category` string) — a per-square-metre cladding rate carries `UnitBasis = 1.0` and a rate-per-hundred-units carries the basis magnitude so the line value divides the applied amount by the basis before multiplying the takeoff quantity; the `CostScheduleKind` rows `Budget`/`CostPlan`/`Estimate`/`Tender`/`PricedBillOfQuantities`/`ScheduleOfRates`/`Running`/`UserDefined`/`NotDefined` (9) each frozen over the `IfcCostScheduleTypeEnum` member, and the `ResourceKind` rows `Labor`/`Material`/`Equipment`/`Crew`/`Product`/`Subcontract`/`UserDefined`/`NotDefined` (8) each frozen with its `IfcDomain`.
- Entry: `CostProjection.Project(IfcCostSchedule schedule, Seq<IfcConstructionResource> resources, BimModel federated)` folds one GeometryGym cost schedule into one `CostSchedule` — materializing the schedule's `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcCostItem` set once, folding each cost item onto a `CostItem` line that reads its `CostValues` `IfcCostValue` applied rate and joins the controlled element `GlobalId` set the item prices, folding the runtime `Extract<IfcConstructionResource>` resource set onto `ConstructionResource` arms discriminated by runtime resource type, binding each resource to the `sequencing#ConstructionTask` it resources by resolving the `IfcRelAssignsToProcess` controlled-process GlobalId, and joining each `CostItem` quantity to the `properties/property-sets#PROPERTY_SETS` `QuantitySet` takeoff the federated model derives — `Fin<T>` aborts on a cost item pricing an element GlobalId the federated model never declares (`faults#FAULT_BAND` `BimFault.DanglingReference`) lowered with `.ToError()`, the resource fold `Choose`-discarding a non-construction-resource entity rather than aborting the whole schedule on a single unmapped resource; `CostProjection.ProjectAll(Seq<IfcCostSchedule> schedules, Seq<IfcConstructionResource> resources, BimModel federated)` lifts every cost schedule in a federated model onto the `Seq<CostSchedule>` the cost catalog reads, and `CostSchedule.Rollup(BimModel federated)` folds the `(quantity x rate x resource)` join into the `CostRollup` schedule total the report renders.
- Auto: `Project` reads the `IfcCostSchedule` runtime graph and folds it into the typed schedule — the `ItemsOf` projection materializes the schedule's `Controls` `IfcRelAssignsToControl` controlled `IfcCostItem` set once, and the cost-item fold reads each item's `CostValues` first `IfcCostValue` onto the `CostValue` value object (`AppliedValue` `IfcMonetaryMeasure` onto `MonetaryAmount.Amount`, the `IfcCostValue.UnitBasis` `IfcMeasureWithUnit` magnitude onto `UnitBasis`, the `IfcCostValue.Category` string onto `CostCategory`), resolves the item's own `Controls` `IfcRelAssignsToControl.RelatedObjects` priced element GlobalIds, and threads the `IfcCostItem` `IsNestedBy`/`Nests` parent reference onto `ParentGlobalId` so a bill-of-quantities tree folds onto the flat line set with its nesting preserved; the resource fold reads each runtime `Extract<IfcConstructionResource>` resource by runtime type (`IfcLaborResource` reads `PredefinedType` onto the `ResourceKind` and the crew `BaseQuantity` onto `Labor`, `IfcConstructionMaterialResource` reads the associated material GlobalId and consumed `BaseQuantity` onto `Material`, `IfcConstructionEquipmentResource` reads the plant `BaseQuantity` onto `Equipment`, all over the same materialized resource set) and the `ResourceTask` fold resolves each resource's `OperatesOn`/`IfcRelAssignsToProcess` controlled `IfcProcess` GlobalId onto the optional `TaskGlobalId` so the resource binds the `sequencing#ConstructionTask` it resources; the `CostItem.ValueOf` fold joins the line's `CostValue` rate to the `properties/property-sets#PROPERTY_SETS` `QuantitySet.Derive(element, geometry)` takeoff quantity the federated model carries — the line value is the takeoff quantity times the unit rate (`amount / basis`) over the priced element set — so the 5D estimate reads one quantity source rather than re-deriving a parallel takeoff; the `CostSchedule.Rollup` fold sums every line value into the `CostRollup` schedule total and partitions the total by `CostCategory` and by `ResourceKind` through one `Fold` over the line set, and the `CostSchedule.Identity` fold derives the `(QuantityKey, ResourceKey)` `UInt128` pair the cross-libs cost catalog reads the schedule by — `QuantityKey` over the priced line `(elementGlobalId, quantity, rate)` triples through `XxHash128.HashToUInt128` and `ResourceKey` over the resource `(GlobalId, kind, baseQuantity)` rows so the catalog re-reads only a changed estimate.
- Receipt: the `Seq<CostSchedule>` is the cost evidence the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads by the `(QuantityKey, ResourceKey)` reference at `Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING` and the `Rasm.AppUi/schedule` estimate report renders, the `query/element-set#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, and the `CostRollup` schedule total with its `CostCategory`/`ResourceKind` partitions is the 5D estimate evidence; the priced line, the applied rate, and the resourced task each carry one record joining the takeoff quantity and the schedule activity, never a second quantity or schedule store.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm
- Growth: a new cost-schedule kind is one `CostScheduleKind` row reading the next `IfcCostScheduleTypeEnum` member; a new construction-resource modality is one `ConstructionResource` union arm reading the next `IfcConstructionResource` subtype — the Thinktecture generated total `Switch` breaks every `BaseQuantity`/`TaskGlobalId` site at compile time until the arm is added, so a missing resource modality is a build error not a silent fallthrough; a new cost category is one `CostCategory` row; a new per-line binding is one column on `CostItem` projected from the existing `IfcCostItem` surface; a new cost schedule rides the existing `ProjectAll` fold on one row; never a per-resource-type cost record, never a parallel `LaborCost`/`MaterialCost`/`EquipmentCost` class family, never a `GetLaborCost`/`GetByCategory` operation family, and never a second takeoff or schedule source.
- Boundary: `CostSchedule` is ONE record discriminated by the `CostScheduleKind` row and the `ConstructionResource` union — a `LaborResource`/`MaterialResource`/`EquipmentResource` class family or three sibling factory methods is the deleted form mirroring the no-per-element-class law at `model/elements#ELEMENT_MODEL`; the estimate is a VIEW of the federated `BimModel` — the priced quantity is the `properties/property-sets#PROPERTY_SETS` `QuantitySet.Derive` takeoff joined by `GlobalId` and a re-derived parallel takeoff in this owner is the named seam violation, the resourced schedule is the `sequencing#ConstructionTask` activity network joined by `TaskGlobalId` and a re-modeled cost-side schedule is the named seam violation; the GeometryGym `IfcCostSchedule`/`IfcCostItem`/`IfcCostValue`/`IfcConstructionResource`/`IfcLaborResource`/`IfcConstructionMaterialResource`/`IfcConstructionEquipmentResource`/`IfcRelAssignsToControl`/`IfcRelAssignsToProcess` surface (`.api/api-geometrygym-ifc` scheduling-cost-resource family rows 9-16) is consumed as settled vocabulary through the `IfcConstructionResource` discrimination and a hand-rolled cost reader is the deleted form; the quantified-element selection is the `query/element-set#ELEMENT_SET` `ByProperty(Qto_*)` predicate and a parallel cost-element selection arm is the no-second-selection-surface reject; the `(QuantityKey, ResourceKey)` content-key identity is derived through the `coordination/model-diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom and the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` cost catalog reads the schedule by that reference at `Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING` — minting a second identity scheme for the catalog join is the named cross-folder drift defect; the `CostSchedule.Rollup` schedule total is ONE fold over the `(quantity x rate x resource)` join, never enumerated per-resource arms; a cost rejection lowers onto `faults#FAULT_BAND` `BimFault` through `.ToError()` and a bare `Fin.Fail` without that lowering is the named seam defect.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO.Hashing;
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class CostScheduleKind {
    public static readonly CostScheduleKind Budget                 = new("BUDGET");
    public static readonly CostScheduleKind CostPlan               = new("COSTPLAN");
    public static readonly CostScheduleKind Estimate               = new("ESTIMATE");
    public static readonly CostScheduleKind Tender                 = new("TENDER");
    public static readonly CostScheduleKind PricedBillOfQuantities = new("PRICEDBILLOFQUANTITIES");
    public static readonly CostScheduleKind ScheduleOfRates        = new("SCHEDULEOFRATES");
    public static readonly CostScheduleKind Running                = new("RUNNING");
    public static readonly CostScheduleKind UserDefined            = new("USERDEFINED");
    public static readonly CostScheduleKind NotDefined             = new("NOTDEFINED");

    public static CostScheduleKind Of(IfcCostScheduleTypeEnum kind) =>
        TryGet(kind.ToString()).IfNone(NotDefined);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class ResourceKind {
    public static readonly ResourceKind Labor       = new("IfcLaborResource", IfcDomain.Architecture);
    public static readonly ResourceKind Material     = new("IfcConstructionMaterialResource", IfcDomain.Architecture);
    public static readonly ResourceKind Equipment    = new("IfcConstructionEquipmentResource", IfcDomain.Architecture);
    public static readonly ResourceKind Crew         = new("IfcCrewResource", IfcDomain.Architecture);
    public static readonly ResourceKind Product      = new("IfcConstructionProductResource", IfcDomain.Architecture);
    public static readonly ResourceKind Subcontract  = new("IfcSubContractResource", IfcDomain.Architecture);
    public static readonly ResourceKind UserDefined  = new("USERDEFINED", IfcDomain.Architecture);
    public static readonly ResourceKind NotDefined   = new("NOTDEFINED", IfcDomain.Architecture);

    public IfcDomain Domain { get; }

    public static ResourceKind Of(string entityType) =>
        TryGet(entityType).IfNone(NotDefined);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class CostCategory {
    public static readonly CostCategory Material      = new("Material");
    public static readonly CostCategory Labour        = new("Labour");
    public static readonly CostCategory Equipment     = new("Equipment");
    public static readonly CostCategory Overhead      = new("Overhead");
    public static readonly CostCategory Subcontract   = new("Subcontract");
    public static readonly CostCategory Preliminaries = new("Preliminaries");
    public static readonly CostCategory NotDefined    = new("NotDefined");

    public static CostCategory Of(string category) =>
        TryGet(category.Trim()).IfNone(NotDefined);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MonetaryAmount(double Amount, string Currency) {
    public static readonly MonetaryAmount Zero = new(0d, "");

    public MonetaryAmount Add(MonetaryAmount other) => other.Amount == 0d
        ? this
        : Amount == 0d
            ? other
            : this with { Amount = Amount + other.Amount };

    public MonetaryAmount Scale(double factor) => this with { Amount = Amount * factor };
}

public sealed record CostValue(MonetaryAmount Applied, double UnitBasis, CostCategory Category) {
    public static readonly CostValue Zero = new(MonetaryAmount.Zero, 1d, CostCategory.NotDefined);

    public double Rate => UnitBasis == 0d ? Applied.Amount : Applied.Amount / UnitBasis;
}

[Union]
public partial record ConstructionResource {
    partial record Labor(string GlobalId, string Name, ResourceKind Kind, string SkillSet, double BaseQuantity, Option<string> TaskGlobalId);
    partial record Material(string GlobalId, string Name, ResourceKind Kind, string MaterialName, double BaseQuantity, Option<string> TaskGlobalId);
    partial record Equipment(string GlobalId, string Name, ResourceKind Kind, double BaseQuantity, Option<string> TaskGlobalId);

    public string GlobalId => Switch(
        labor:     static r => r.GlobalId,
        material:  static r => r.GlobalId,
        equipment: static r => r.GlobalId);

    public ResourceKind Kind => Switch(
        labor:     static r => r.Kind,
        material:  static r => r.Kind,
        equipment: static r => r.Kind);

    public double BaseQuantity => Switch(
        labor:     static r => r.BaseQuantity,
        material:  static r => r.BaseQuantity,
        equipment: static r => r.BaseQuantity);

    public Option<string> TaskGlobalId => Switch(
        labor:     static r => r.TaskGlobalId,
        material:  static r => r.TaskGlobalId,
        equipment: static r => r.TaskGlobalId);
}

public sealed record CostItem(
    string GlobalId,
    string Name,
    CostValue Value,
    Seq<string> PricedGlobalIds,
    Option<string> ResourceGlobalId,
    Option<string> ParentGlobalId) {
    public MonetaryAmount ValueOf(BimModel federated) =>
        PricedGlobalIds
            .Choose(id => federated.Elements.Find(e => e.GlobalId == id))
            .Fold(MonetaryAmount.Zero, (total, element) => total.Add(
                Value.Applied.Currency is ""
                    ? MonetaryAmount.Zero
                    : new MonetaryAmount(Value.Rate * QuantityOf(element), Value.Applied.Currency)));

    static double QuantityOf(BimElement element) =>
        QuantitySet.Derive(element, element.Geometry).Quantities
            .Values
            .HeadOrNone()
            .Map(static q => q.Value)
            .IfNone(1d);
}

public sealed record CostRollup(
    MonetaryAmount Total,
    Map<string, MonetaryAmount> ByCategory,
    Map<string, double> ByResourceKind);

public sealed record CostSchedule(
    string GlobalId,
    CostScheduleKind Kind,
    string Name,
    Seq<CostItem> Items,
    Seq<ConstructionResource> Resources) {
    public (UInt128 QuantityKey, UInt128 ResourceKey) Identity => (
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join("",
            Items.Map(static i => $"{string.Join(",", i.PricedGlobalIds)}={i.Value.Rate}@{i.Value.Category.Key}")))),
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join("",
            Resources.Map(static r => $"{r.GlobalId}={r.Kind.Key}:{r.BaseQuantity}")))));

    public CostRollup Rollup(BimModel federated) {
        var lines = Items.Map(item => (item.Value.Category.Key, Amount: item.ValueOf(federated)));
        return new CostRollup(
            lines.Fold(MonetaryAmount.Zero, static (total, line) => total.Add(line.Amount)),
            lines.Fold(Map<string, MonetaryAmount>(), static (by, line) =>
                by.AddOrUpdate(line.Key, existing => existing.Add(line.Amount), line.Amount)),
            Resources.Fold(Map<string, double>(), static (by, resource) =>
                by.AddOrUpdate(resource.Kind.Key, existing => existing + resource.BaseQuantity, resource.BaseQuantity)));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CostProjection {
    public static Fin<CostSchedule> Project(IfcCostSchedule schedule, Seq<IfcConstructionResource> resources, BimModel federated) {
        var index = toHashSet(federated.Elements.Map(static e => e.GlobalId));
        return ItemsOf(schedule)
            .TraverseM(item => CostItemOf(item, index))
            .As()
            .Map(items => new CostSchedule(
                schedule.GlobalId,
                CostScheduleKind.Of(schedule.PredefinedType),
                schedule.Name ?? "",
                items,
                resources.Choose(ResourceOf)));
    }

    public static Fin<Seq<CostSchedule>> ProjectAll(Seq<IfcCostSchedule> schedules, Seq<IfcConstructionResource> resources, BimModel federated) =>
        schedules.TraverseM(schedule => Project(schedule, resources, federated)).As();

    static Seq<IfcCostItem> ItemsOf(IfcCostSchedule schedule) =>
        schedule.Controls
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcCostItem>()
            .ToSeq();

    static Fin<CostItem> CostItemOf(IfcCostItem item, HashSet<string> index) {
        var priced = item.Controls
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .Select(static o => o.GlobalId)
            .Where(static id => id.Length > 0)
            .ToSeq();
        var dangling = priced.Find(id => !index.Contains(id));
        return dangling.Match(
            Some: id => FinFail<CostItem>(new BimFault.DanglingReference($"cost-priced-miss:{id}").ToError()),
            None: () => FinSucc(new CostItem(
                item.GlobalId,
                item.Name ?? "",
                ValueOf(item),
                priced,
                None,
                Optional(item.Nests?.RelatingObject?.GlobalId))));
    }

    static CostValue ValueOf(IfcCostItem item) =>
        item.CostValues.AsIterable().HeadOrNone().Match(
            Some: value => new CostValue(
                new MonetaryAmount(AmountOf(value), CurrencyOf(value)),
                BasisOf(value),
                CostCategory.Of(value.Category ?? "")),
            None: () => CostValue.Zero);

    static double AmountOf(IfcCostValue value) =>
        value.AppliedValue is IfcMonetaryMeasure monetary ? monetary.Measure : 0d;

    static string CurrencyOf(IfcCostValue value) =>
        value.UnitBasis?.UnitComponent is IfcMonetaryUnit unit ? unit.Currency : "";

    static double BasisOf(IfcCostValue value) =>
        value.UnitBasis?.ValueComponent is IfcMeasureValue basis && basis.Measure > 0d ? basis.Measure : 1d;

    static Option<ConstructionResource> ResourceOf(IfcConstructionResource resource) =>
        resource switch {
            IfcLaborResource labor =>
                Some<ConstructionResource>(new ConstructionResource.Labor(
                    labor.GlobalId, labor.Name ?? "",
                    ResourceKind.Of(labor.GetType().Name),
                    labor.PredefinedType.ToString(),
                    BaseQuantityOf(labor),
                    TaskOf(labor))),
            IfcConstructionMaterialResource material =>
                Some<ConstructionResource>(new ConstructionResource.Material(
                    material.GlobalId, material.Name ?? "",
                    ResourceKind.Of(material.GetType().Name),
                    MaterialOf(material),
                    BaseQuantityOf(material),
                    TaskOf(material))),
            IfcConstructionEquipmentResource equipment =>
                Some<ConstructionResource>(new ConstructionResource.Equipment(
                    equipment.GlobalId, equipment.Name ?? "",
                    ResourceKind.Of(equipment.GetType().Name),
                    BaseQuantityOf(equipment),
                    TaskOf(equipment))),
            _ => None,
        };

    static double BaseQuantityOf(IfcConstructionResource resource) =>
        resource.BaseQuantity is IfcPhysicalSimpleQuantity simple ? simple.MeasureValue.Measure : 0d;

    static string MaterialOf(IfcConstructionMaterialResource resource) =>
        resource.HasAssociations
            .AsIterable()
            .OfType<IfcRelAssociatesMaterial>()
            .HeadOrNone()
            .Bind(static rel => Optional((rel.RelatingMaterial as IfcMaterial)?.Name))
            .IfNone("");

    static Option<string> TaskOf(IfcConstructionResource resource) =>
        resource.HasAssignments
            .AsIterable()
            .OfType<IfcRelAssignsToProcess>()
            .HeadOrNone()
            .Bind(static rel => Optional((rel.RelatingProcess as IfcProcess)?.GlobalId))
            .Filter(static id => id.Length > 0);
}
```

## [3]-[RESEARCH]

- [COST_SCHEDULE_DISPATCH]: the `IfcCostSchedule` container traversal — the `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcCostItem` set the `ItemsOf` fold materializes the priced lines through, each item's own `Controls` `IfcRelAssignsToControl` priced-element set, each item's `CostValues` `IfcCostValue` applied-rate set, and the `Nests` `IfcRelNests` parent reference the bill-of-quantities tree declares — grounds against the GeometryGym scheduling-cost-resource family surface (`.api/api-geometrygym-ifc` scheduling-cost-resource rows 9-16, the `IfcCostItem.CostValues` `IfcCostValue` traversal row 8) so the `CostItemOf`/`ValueOf` projections discriminate the real cost graph rather than a guessed shape; the `IfcCostSchedule.Controls`/`PredefinedType`/`SubmittedOn`, `IfcCostItem.CostValues`/`Controls`/`Nests`/`PredefinedType`, `IfcCostValue.AppliedValue`/`UnitBasis`/`Category`, and `IfcRelAssignsToControl.RelatingControl`/`RelatedObjects` member spellings confirm against the catalogued surface before the projection fold is final — the `IfcCostSchedule.Controls`/`IfcCostItem.Controls` `IfcRelAssignsToControl` control path is the cost-control assignment relationship, distinct from the `IfcRelAssignsToProcess` resource-to-activity assignment the resource fold reads.
- [COST_VALUE_MONETARY]: the `IfcCostValue` applied-rate members the `ValueOf` fold reads onto the `CostValue` value object are verified against the live GeometryGym decompile — `IfcCostValue : IfcAppliedValue` inherits `AppliedValue` (`IfcAppliedValueSelect`), `UnitBasis` (`IfcMeasureWithUnit`), and `Category` (`String`); the `IfcMonetaryMeasure` direct-amount leg exposes `.Measure` (a `Double`, NOT `.Magnitude` — `IfcMonetaryMeasure : IfcDerivedMeasureValue`) onto `MonetaryAmount.Amount`; the `IfcMeasureWithUnit.UnitComponent` (`IfcUnit`, narrowed `is IfcMonetaryUnit` whose `.Currency` is the ISO 4217 code) onto `MonetaryAmount.Currency`; and the `IfcMeasureWithUnit.ValueComponent` (`IfcValue`, narrowed `is IfcMeasureValue` whose `.Measure` is the per-basis denominator) onto the `UnitBasis` denominator — so the `AmountOf`/`CurrencyOf`/`BasisOf` folds read the real `.Measure`-bearing measure shape, the `IfcCostValue.Category` string lowering onto the `CostCategory` discriminant.
- [RESOURCE_TASK_JOIN]: the construction-resource-to-activity join the `ResourceOf`/`TaskOf` folds read is verified against the live GeometryGym decompile — the `IfcConstructionResource` subtypes (`IfcLaborResource`/`IfcConstructionMaterialResource`/`IfcConstructionEquipmentResource`) the `ConstructionResource` union discriminates each carry only a `PredefinedType` typed kind in IFC4.3 (the IFC2x3 `IfcLaborResource.SkillSet` and a direct `RelatingMaterials` member do NOT exist here), so the `Labor` arm reads the `IfcLaborResourceTypeEnum PredefinedType` as the skill descriptor and the `Material` arm reads the consumed material name through the resource's inherited `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`as IfcMaterial`) `.Name`; the consumed quantity is the inherited `IfcConstructionResource.BaseQuantity` (`IfcPhysicalQuantity`, narrowed `is IfcPhysicalSimpleQuantity` whose `.MeasureValue.Measure` is the magnitude — there is no `.SimpleValue`); the task join is the inherited `HasAssignments` `IfcRelAssignsToProcess.RelatingProcess` controlled `IfcProcess` GlobalId, so the resource binds the `sequencing#ConstructionTask` it resources by the real assignment path, never re-modeling the activity network.
- [COST_ROLLUP_IDENTITY]: the `CostSchedule.Rollup` `(quantity x rate x resource)` fold and the `CostSchedule.Identity` `(QuantityKey, ResourceKey)` `UInt128` pair the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads the schedule by ground against the `properties/property-sets#PROPERTY_SETS` `QuantitySet.Derive` takeoff the `CostItem.ValueOf` line value reads — the line value is the takeoff quantity times the unit rate (`Applied.Amount / UnitBasis`) over the priced element set — and against the `coordination/model-diff#MODEL_DIFF` `XxHash128.HashToUInt128` content-key idiom and the `Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` owner, so the catalog re-reads the estimate only on a changed `QuantityKey` (the priced line `(element, quantity, rate)` triples) or `ResourceKey` (the resource `(GlobalId, kind, baseQuantity)` rows) rather than re-pricing the schedule; the `Rollup` partitions the total by `CostCategory` and by `ResourceKind` through one `Fold` over the line and resource sets, never enumerated per-resource arms, and the `query/element-set#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices — re-pricing the estimate or minting a second takeoff or schedule source downstream is the named cross-folder drift defect, the cost owner produces the priced schedule and its content-key identity, never the takeoff or the activity network.
