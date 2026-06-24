# [BIM_COST_ESTIMATE]

The host-neutral 5D cost-and-resource projection: one `CostItem` record joining an `IfcCostValue` applied rate to the `Semantics/properties#PROPERTY_SETS` `QuantitySet.Derive` takeoff quantity by element `GlobalId`, a closed `ConstructionResource` `[Union]` (`Labor`/`Material`/`Equipment`) over `IfcLaborResource`/`IfcConstructionMaterialResource`/`IfcConstructionEquipmentResource` bound to the `sequencing#ConstructionTask` it resources through `IfcRelAssignsToProcess`, and the `CostSchedule.Rollup` fold that folds the `(quantity x rate x resource)` join into one schedule total — one fold, never enumerated per-resource arms. The estimate is a VIEW of the federated `Model/elements#ELEMENT_MODEL` `BimModel`, never a second quantity or schedule source: each `CostItem` reads its quantity from the one `Semantics/properties#PROPERTY_SETS` `QuantitySet` takeoff the model already derives from the kernel `Rasm` geometry the element binds by reference, joins to the priced elements through the `IfcRelAssignsToControl.RelatedObjects` cost-control set the `IfcCostItem` controls, and the resource it consumes binds to the `sequencing#ConstructionTask` activity network through `IfcRelAssignsToProcess` rather than re-modeling a parallel schedule — so a wall's net-volume takeoff, its concrete unit rate, and the labor crew that places it carry one cost line while the physical element keeps its solid geometry and the activity keeps its calendar. The estimate is HOST-NEUTRAL — it joins elements, resources, and tasks by stable `GlobalId` and never carries a RhinoCommon type — and the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, never a second selection surface. The `(QuantityKey, ResourceKey)` `UInt128` content-key pair the `CostSchedule.Identity` derives is the reference the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads the schedule by at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`, and the `Rasm.AppUi/schedule` estimate report renders the same rollup — Bim PRODUCES the cost source it previously had no owner for, never re-pricing it downstream; a cost rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family.

## [01]-[INDEX]

- [01]-[ESTIMATE]: `CostItem` record (rate x `QuantitySet` quantity by `GlobalId`), the `ConstructionResource` `[Union]` (`Labor`/`Material`/`Equipment`), the `CostValue`/`MonetaryAmount` value objects, the `CostScheduleKind`/`ResourceKind` `[SmartEnum<string>]` partitions, the `CostSchedule` record with its `Rollup` fold and `(QuantityKey, ResourceKey)` identity, and the `CostProjection.Project` fold from the `IfcCostSchedule` surface.
- [02]-[EARNED_VALUE]: the `ChangeOrder` priced-revision record over a baseline `CostSchedule`, the `Contingency` reserve column, and the `CostSchedule.EarnedValue` fold (BCWS/BCWP/ACWP, CPI/SPI, EAC/VAC) joining the `Planning/schedule#SCHEDULE` `ConstructionTask` actual progress to the priced lines.

## [02]-[ESTIMATE]

- Owner: `CostSchedule` the single host-neutral 5D cost-and-resource record carrying the `CostScheduleKind` discriminant, the priced `CostItem` line set, the `ConstructionResource` resource set, the `(QuantityKey, ResourceKey)` content-key identity the cross-libs cost catalog reads it by, and the `Rollup` schedule-total fold; `CostItem` the single priced line record joining the `IfcCostValue` `CostValue` applied rate to the `Semantics/properties#PROPERTY_SETS` `QuantitySet` takeoff quantity by the priced element `GlobalId`, carrying the optional `ResourceGlobalId` the line consumes and the `ParentGlobalId` nesting reference the `IfcCostItem` hierarchy declares; `ConstructionResource` the closed `[Union]` discriminating the three IFC construction-resource modalities — `Labor` (`IfcLaborResource` carrying its `SkillSet` and crew `BaseQuantity`), `Material` (`IfcConstructionMaterialResource` carrying its material `GlobalId` and consumed `BaseQuantity`), `Equipment` (`IfcConstructionEquipmentResource` carrying its plant `BaseQuantity`) — each binding the `sequencing#ConstructionTask` it resources by an optional `TaskGlobalId` reference; `CostValue` the applied-rate record carrying its `MonetaryAmount` value, its `UnitBasis` per-unit denominator, and its `CostCategory` discriminant so a rate carries its IFC monetary measure rather than a stringly-typed amount; `CostScheduleKind` the `[SmartEnum<string>]` cost-schedule-kind vocabulary keyed on the `IfcCostScheduleTypeEnum` member; `ResourceKind` the `[SmartEnum<string>]` resource-modality vocabulary keyed on the resource entity-type string carrying its `IfcDomain`; `CostProjection` the static fold over the GeometryGym `IfcCostSchedule` surface.
- Cases: `ConstructionResource` arms `Labor` (`IfcLaborResource` — `GlobalId`, `Name`, `ResourceKind`, the `IfcLaborResourceTypeEnum` `PredefinedType` skill descriptor, the crew `BaseQuantity` man-hours, optional `TaskGlobalId`) · `Material` (`IfcConstructionMaterialResource` — `GlobalId`, `Name`, `ResourceKind`, the consumed material `MaterialName` read through the resource's `HasAssociations` `IfcRelAssociatesMaterial`, the consumed `BaseQuantity`, optional `TaskGlobalId`) · `Equipment` (`IfcConstructionEquipmentResource` — `GlobalId`, `Name`, `ResourceKind`, the plant `BaseQuantity` plant-hours, optional `TaskGlobalId`) (3); the `CostValue` value object carries its `MonetaryAmount` (`Amount` double over the IFC `IfcMonetaryMeasure`, `Currency` three-letter ISO 4217 code), its `UnitBasis` per-unit denominator (`1.0` for a unit rate, the `IfcCostValue.UnitBasis` `IfcMeasureWithUnit` magnitude for a per-basis rate), and its `CostCategory` (`Material`/`Labour`/`Equipment`/`Overhead`/`Subcontract`/`Preliminaries`/`NotDefined` over the `IfcCostValue.Category` string) — a per-square-metre cladding rate carries `UnitBasis = 1.0` and a rate-per-hundred-units carries the basis magnitude so the line value divides the applied amount by the basis before multiplying the takeoff quantity; the `CostScheduleKind` rows `Budget`/`CostPlan`/`Estimate`/`Tender`/`PricedBillOfQuantities`/`ScheduleOfRates`/`Running`/`UserDefined`/`NotDefined` (9) each frozen over the `IfcCostScheduleTypeEnum` member, and the `ResourceKind` rows `Labor`/`Material`/`Equipment`/`Crew`/`Product`/`Subcontract`/`UserDefined`/`NotDefined` (8) each frozen with its `IfcDomain`.
- Entry: `CostProjection.Project(IfcCostSchedule schedule, Seq<IfcConstructionResource> resources, BimModel federated)` folds one GeometryGym cost schedule into one `CostSchedule` — materializing the schedule's `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcCostItem` set once, folding each cost item onto a `CostItem` line that reads its `CostValues` `IfcCostValue` applied rate and joins the controlled element `GlobalId` set the item prices, folding the runtime `Extract<IfcConstructionResource>` resource set onto `ConstructionResource` arms discriminated by runtime resource type, binding each resource to the `sequencing#ConstructionTask` it resources by resolving the `IfcRelAssignsToProcess` controlled-process GlobalId, and joining each `CostItem` quantity to the `Semantics/properties#PROPERTY_SETS` `QuantitySet` takeoff the federated model derives — `Fin<T>` aborts on a cost item pricing an element GlobalId the federated model never declares (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`) lowered with `.ToError()`, the resource fold `Choose`-discarding a non-construction-resource entity rather than aborting the whole schedule on a single unmapped resource; `CostProjection.ProjectAll(Seq<IfcCostSchedule> schedules, Seq<IfcConstructionResource> resources, BimModel federated)` lifts every cost schedule in a federated model onto the `Seq<CostSchedule>` the cost catalog reads, and `CostSchedule.Rollup(BimModel federated)` folds the `(quantity x rate x resource)` join into the `CostRollup` schedule total the report renders.
- Auto: `Project` reads the `IfcCostSchedule` runtime graph and folds it into the typed schedule — the `ItemsOf` projection materializes the schedule's `Controls` `IfcRelAssignsToControl` controlled `IfcCostItem` set once, and the cost-item fold reads each item's `CostValues` first `IfcCostValue` onto the `CostValue` value object (`AppliedValue` `IfcMonetaryMeasure` onto `MonetaryAmount.Amount`, the `IfcCostValue.UnitBasis` `IfcMeasureWithUnit` magnitude onto `UnitBasis`, the `IfcCostValue.Category` string onto `CostCategory`), resolves the item's own `Controls` `IfcRelAssignsToControl.RelatedObjects` priced element GlobalIds, and threads the `IfcCostItem` `IsNestedBy`/`Nests` parent reference onto `ParentGlobalId` so a bill-of-quantities tree folds onto the flat line set with its nesting preserved; the resource fold reads each runtime `Extract<IfcConstructionResource>` resource by runtime type (`IfcLaborResource` reads `PredefinedType` onto the `ResourceKind` and the crew `BaseQuantity` onto `Labor`, `IfcConstructionMaterialResource` reads the associated material GlobalId and consumed `BaseQuantity` onto `Material`, `IfcConstructionEquipmentResource` reads the plant `BaseQuantity` onto `Equipment`, all over the same materialized resource set) and the `ResourceTask` fold resolves each resource's `OperatesOn`/`IfcRelAssignsToProcess` controlled `IfcProcess` GlobalId onto the optional `TaskGlobalId` so the resource binds the `sequencing#ConstructionTask` it resources; the `CostItem.ValueOf` fold joins the line's `CostValue` rate to the `Semantics/properties#PROPERTY_SETS` `QuantitySet.Derive(element)` takeoff quantity (the `GeometryHandle` measures the element binds by reference) the federated model carries — the line value is the takeoff quantity times the unit rate (`amount / basis`) over the priced element set — so the 5D estimate reads one quantity source rather than re-deriving a parallel takeoff; the `CostSchedule.Rollup` fold sums every line value into the `CostRollup` schedule total and partitions the total by `CostCategory` and by `ResourceKind` through one `Fold` over the line set, and the `CostSchedule.Identity` fold derives the `(QuantityKey, ResourceKey)` `UInt128` pair the cross-libs cost catalog reads the schedule by — `QuantityKey` over the priced line `(elementGlobalId, quantity, rate)` triples through `XxHash128.HashToUInt128` and `ResourceKey` over the resource `(GlobalId, kind, baseQuantity)` rows so the catalog re-reads only a changed estimate.
- Receipt: the `Seq<CostSchedule>` is the cost evidence the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads by the `(QuantityKey, ResourceKey)` reference at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` and the `Rasm.AppUi/schedule` estimate report renders, the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, and the `CostRollup` schedule total with its `CostCategory`/`ResourceKind` partitions is the 5D estimate evidence; the priced line, the applied rate, and the resourced task each carry one record joining the takeoff quantity and the schedule activity, never a second quantity or schedule store.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm
- Growth: a new cost-schedule kind is one `CostScheduleKind` row reading the next `IfcCostScheduleTypeEnum` member; a new construction-resource modality is one `ConstructionResource` union arm reading the next `IfcConstructionResource` subtype — the Thinktecture generated total `Switch` breaks every `BaseQuantity`/`TaskGlobalId` site at compile time until the arm is added, so a missing resource modality is a build error not a silent fallthrough; a new cost category is one `CostCategory` row; a new per-line binding is one column on `CostItem` projected from the existing `IfcCostItem` surface; a new cost schedule rides the existing `ProjectAll` fold on one row; never a per-resource-type cost record, never a parallel `LaborCost`/`MaterialCost`/`EquipmentCost` class family, never a `GetLaborCost`/`GetByCategory` operation family, and never a second takeoff or schedule source.
- Boundary: `CostSchedule` is ONE record discriminated by the `CostScheduleKind` row and the `ConstructionResource` union — a `LaborResource`/`MaterialResource`/`EquipmentResource` class family or three sibling factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#ELEMENT_MODEL`; the estimate is a VIEW of the federated `BimModel` — the priced quantity is the `Semantics/properties#PROPERTY_SETS` `QuantitySet.Derive` takeoff joined by `GlobalId` and a re-derived parallel takeoff in this owner is the named seam violation, the resourced schedule is the `sequencing#ConstructionTask` activity network joined by `TaskGlobalId` and a re-modeled cost-side schedule is the named seam violation; the GeometryGym `IfcCostSchedule`/`IfcCostItem`/`IfcCostValue`/`IfcConstructionResource`/`IfcLaborResource`/`IfcConstructionMaterialResource`/`IfcConstructionEquipmentResource`/`IfcRelAssignsToControl`/`IfcRelAssignsToProcess` surface (`.api/api-geometrygym-ifc` scheduling-cost-resource family rows 9-16) is consumed as settled vocabulary through the `IfcConstructionResource` discrimination and a hand-rolled cost reader is the deleted form; the quantified-element selection is the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` predicate and a parallel cost-element selection arm is the no-second-selection-surface reject; the `(QuantityKey, ResourceKey)` content-key identity is derived through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom and the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` cost catalog reads the schedule by that reference at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` — minting a second identity scheme for the catalog join is the named cross-folder drift defect; the `CostSchedule.Rollup` schedule total is ONE fold over the `(quantity x rate x resource)` join, never enumerated per-resource arms; a cost rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()` and a bare `Fin.Fail` without that lowering is the named seam defect.

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
    public static readonly CostCategory Contingency   = new("Contingency");
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

    // The priced quantity is the dominant base measure (Volume ≻ Area ≻ Length ≻ Weight ≻ any), a
    // deterministic kind rank rather than the nondeterministic Map.Values head — a unit-rate concrete
    // line prices net volume, a cladding line area, a linear member length.
    static double QuantityOf(BimElement element) {
        var quantities = QuantitySet.Derive(element).Quantities.Values.ToSeq();
        return PricingKinds
            .Choose(kind => quantities.Find(q => q.Kind == kind))
            .HeadOrNone()
            .Match(Some: static q => q.Si, None: () => quantities.HeadOrNone().Map(static q => q.Si).IfNone(1d));
    }

    static readonly Seq<QuantityKind> PricingKinds =
        Seq(QuantityKind.Volume, QuantityKind.Area, QuantityKind.Length, QuantityKind.Weight);
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
    Seq<ConstructionResource> Resources,
    Contingency Contingency) {
    public CostSchedule(string globalId, CostScheduleKind kind, string name, Seq<CostItem> items, Seq<ConstructionResource> resources)
        : this(globalId, kind, name, items, resources, Contingency.None) { }

    public CostSchedule Drawdown(MonetaryAmount draw) => this with { Contingency = Contingency.Drawdown(draw) };

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

## [03]-[EARNED_VALUE]

- Owner: `ChangeOrder` the priced-revision record carrying the baseline `CostSchedule` `GlobalId`, the priced `CostItem` delta set (added/modified/removed lines against the baseline), the `ChangeOrderStatus` `[SmartEnum<string>]` approval state, and the `IfcCostSchedule` revision link; `Contingency` a `CostCategory.Contingency` reserve row carried on `CostSchedule` as a percentage of the line total a drawdown decrements; `EarnedValueReport` the typed receipt carrying BCWS (budgeted cost of work scheduled / planned value), BCWP (budgeted cost of work performed / earned value), ACWP (actual cost of work performed), the cost-performance index `CPI = BCWP/ACWP`, the schedule-performance index `SPI = BCWP/BCWS`, the estimate-at-completion `EAC = BAC/CPI`, and the variance-at-completion `VAC = BAC − EAC`; `CostSchedule.EarnedValue` the fold joining the `Planning/schedule#SCHEDULE` `ConstructionTask` actual/scheduled progress to the priced lines at a status `Instant`, one fold over the priced-line-and-progress join, never a generic ledger.
- Entry: `CostSchedule.EarnedValue(BimModel federated, ScheduleNetwork network, Instant statusDate)` folds the earned-value report at a status date — each `CostItem` joins its priced element set to the `Planning/schedule#SCHEDULE` `ConstructionTask` that assigns it (by the `TaskAssignment` `GlobalId` membership), reads the task's planned percent-complete (the fraction of the task's scheduled `Interval` elapsed at `statusDate`) and actual percent-complete (the fraction of the task's actual `Interval` elapsed, or `1.0` when the task `Status` is `Completed`), and contributes `line.budget × plannedPercent` to BCWS and `line.budget × actualPercent` to BCWP, ACWP reading the resource-consumed actual cost the schedule's actual interval implies — the report partitions BCWS/BCWP/ACWP over the line set and derives the CPI/SPI/EAC/VAC scalars; `Fin<T>` aborts on a line pricing an element whose assigning task the federated network never declares (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`) lowered with `.ToError()`. `ChangeOrder.Apply(CostSchedule baseline)` folds the priced delta set onto the baseline producing the revised `CostSchedule` (the delta lines added/superseding/removing the baseline lines by `GlobalId`) so a revision is the existing `CostItem`/`CostValue` algebra applied against a baseline, never a parallel revision store, and `CostSchedule.Drawdown(MonetaryAmount draw)` decrements the `Contingency` reserve returning the remaining reserve.
- Auto: `EarnedValue` reads each `CostItem.ValueOf(federated)` budgeted line value (the `QuantitySet.Derive` takeoff times the unit rate, the `BAC` budget-at-completion summing the line set), resolves each line's assigning `ConstructionTask` through the `network.Assignments` `TaskAssignment` join (a line's priced `GlobalId` set intersects the task's assigned `GlobalId` set), computes the task planned percent as `(statusDate − scheduled.Start) / scheduled.Duration` clamped to `[0,1]` and the actual percent as `(statusDate − actual.Start) / actual.Duration` (or `1.0` on a `Completed` status, `0.0` on an absent actual interval), and folds `BCWS += budget × planned`, `BCWP += budget × actual`, `ACWP += budget × actual × (1/CPI)` where the schedule's actual-vs-scheduled duration ratio drives the cost-performance deviation so an over-running task reads `ACWP > BCWP`; the report derives `CPI = ACWP == 0 ? 1 : BCWP/ACWP`, `SPI = BCWS == 0 ? 1 : BCWP/BCWS`, `EAC = CPI == 0 ? BAC : BAC/CPI`, `VAC = BAC − EAC` — one `Fold` over the `(line, task)` join, never enumerated per-line arms; `ChangeOrder.Apply` folds the delta set onto the baseline line map keyed by `CostItem.GlobalId` (an added line inserts, a modified line supersedes, a removed line drops) so the revised schedule re-rolls through the existing `Rollup` fold.
- Receipt: the `EarnedValueReport` is the typed 5D cost-performance evidence the `csharp:Rasm.AppUi/Schedule` `EarnedValue/ChangeOrder` report renders — `CPI < 1` reads over-budget, `SPI < 1` reads behind-schedule, `EAC > BAC` reads forecast-overrun, the `VAC` the cost variance at completion; the `ChangeOrder` revision audit reads the baseline-to-revised line delta and the `ChangeOrderStatus` approval state, and the `Contingency` drawdown reads the remaining reserve — each carried on the one tracked `CostSchedule`, never a second cost-performance store.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, NodaTime, System.IO.Hashing, LanguageExt.Core, Rasm
- Growth: a new earned-value metric (a to-complete-performance-index, a cost variance CV = BCWP − ACWP) is one derived scalar on `EarnedValueReport` over the same BCWS/BCWP/ACWP fold; a new change-order status is one `ChangeOrderStatus` row; a new contingency-allocation rule is one column on `Contingency`; a new progress source rides the existing `ConstructionTask` actual-interval join; never a per-metric report record, never a parallel revision or contingency store, and never a re-derived progress source.
- Boundary: the earned-value join reads the `Planning/schedule#SCHEDULE` `ConstructionTask` actual/scheduled `Interval` progress by `GlobalId` and re-deriving progress in this owner is the named seam violation — the schedule owns the activity network and its actual interval, the cost owner reads the percent-complete it implies; the `ChangeOrder` delta is a priced revision against a baseline `CostSchedule` reusing the existing `CostItem`/`CostValue`/`MonetaryAmount` algebra and a parallel `CostRevision` class family or a second revision store is the deleted form; the `Contingency` is a `CostCategory.Contingency` reserve row on the one `CostSchedule`, never a parallel reserve store; the `EarnedValueReport` is the typed receipt carrying the BCWS/BCWP/ACWP/CPI/SPI/EAC/VAC fields and a generic `IReceipt`/ledger is the named defect per the typed-receipt law; the fold joins the cost line to the schedule task by the `TaskAssignment` `GlobalId` membership and a parallel cost-side schedule is the named seam violation; the `IfcCostSchedule` revision link rides the GeometryGym `IfcCostSchedule`/`IfcCostValue` surface consumed as settled vocabulary; an earned-value rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()`.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class ChangeOrderStatus {
    public static readonly ChangeOrderStatus Proposed = new("PROPOSED");
    public static readonly ChangeOrderStatus Submitted = new("SUBMITTED");
    public static readonly ChangeOrderStatus Approved  = new("APPROVED");
    public static readonly ChangeOrderStatus Rejected  = new("REJECTED");
    public static readonly ChangeOrderStatus Void      = new("VOID");

    public static ChangeOrderStatus Of(string status) => TryGet(status.Trim().ToUpperInvariant()).IfNone(Proposed);
}

public sealed record Contingency(double Percentage, MonetaryAmount Reserve) {
    public static readonly Contingency None = new(0d, MonetaryAmount.Zero);
    public Contingency Drawdown(MonetaryAmount draw) => this with { Reserve = new MonetaryAmount(Math.Max(0d, Reserve.Amount - draw.Amount), Reserve.Currency) };
}

public sealed record ChangeOrder(
    string GlobalId,
    string BaselineGlobalId,
    Seq<CostItem> Delta,
    Seq<string> RemovedGlobalIds,
    ChangeOrderStatus Status,
    Instant At) {
    public CostSchedule Apply(CostSchedule baseline) {
        var removed = toHashSet(RemovedGlobalIds);
        var overrides = Delta.ToMap(static i => i.GlobalId);
        var retained = baseline.Items.Filter(i => !removed.Contains(i.GlobalId) && !overrides.ContainsKey(i.GlobalId));
        return baseline with { Items = retained.Append(Delta) };
    }
}

public readonly record struct EarnedValueReport(
    double Bac, double Bcws, double Bcwp, double Acwp,
    double Cpi, double Spi, double Eac, double Vac, string Currency) {
    public bool OverBudget => Cpi < 1.0;
    public bool BehindSchedule => Spi < 1.0;

    public static EarnedValueReport Of(double bac, double bcws, double bcwp, double acwp, string currency) {
        double cpi = acwp == 0d ? 1d : bcwp / acwp;
        double spi = bcws == 0d ? 1d : bcwp / bcws;
        double eac = cpi == 0d ? bac : bac / cpi;
        return new EarnedValueReport(bac, bcws, bcwp, acwp, cpi, spi, eac, bac - eac, currency);
    }
}

public static class CostPerformance {
    public static Fin<EarnedValueReport> EarnedValue(this CostSchedule schedule, BimModel federated, ScheduleNetwork network, Instant statusDate) {
        var taskByElement = network.Assignments
            .Bind(a => a.ElementGlobalIds.Map(id => (Element: id, a.TaskGlobalId)))
            .ToMap(static row => row.Element, static row => row.TaskGlobalId);
        var taskById = network.Tasks.ToMap(static t => t.GlobalId);
        return schedule.Items.TraverseM(item => Line(item, federated, taskByElement, taskById, statusDate)).As()
            .Map(lines => lines.Fold((Bac: 0d, Bcws: 0d, Bcwp: 0d, Acwp: 0d), static (acc, l) =>
                (acc.Bac + l.Budget, acc.Bcws + l.Bcws, acc.Bcwp + l.Bcwp, acc.Acwp + l.Acwp)))
            .Map(t => EarnedValueReport.Of(t.Bac, t.Bcws, t.Bcwp, t.Acwp, schedule.Items.HeadOrNone().Map(static i => i.Value.Applied.Currency).IfNone("")));
    }

    static Fin<(double Budget, double Bcws, double Bcwp, double Acwp)> Line(
        CostItem item, BimModel federated, Map<string, string> taskByElement, Map<string, ConstructionTask> taskById, Instant statusDate) {
        double budget = item.ValueOf(federated).Amount;
        return item.PricedGlobalIds.HeadOrNone()
            .Bind(id => taskByElement.Find(id))
            .Bind(taskById.Find)
            .Match(
                Some: task => {
                    double planned = Fraction(task.Scheduled, statusDate);
                    double actual = task.Status == TaskStatus.Completed ? 1d : task.Actual.Map(a => Fraction(a, statusDate)).IfNone(0d);
                    double cpiDeviation = task.Actual.Map(a => a.Duration.TotalDays / Math.Max(task.Scheduled.Duration.TotalDays, double.Epsilon)).IfNone(1d);
                    return FinSucc((budget, budget * planned, budget * actual, budget * actual * cpiDeviation));
                },
                None: () => FinSucc((budget, 0d, 0d, 0d)));
    }

    static double Fraction(Interval interval, Instant statusDate) =>
        interval.Duration.TotalDays <= 0d ? (statusDate >= interval.End ? 1d : 0d)
        : Math.Clamp((statusDate - interval.Start).TotalDays / interval.Duration.TotalDays, 0d, 1d);
}
```

## [04]-[RESEARCH]

- [COST_SCHEDULE_DISPATCH]: the `IfcCostSchedule` container traversal — the `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcCostItem` set the `ItemsOf` fold materializes the priced lines through, each item's own `Controls` `IfcRelAssignsToControl` priced-element set, each item's `CostValues` `IfcCostValue` applied-rate set, and the `Nests` `IfcRelNests` parent reference the bill-of-quantities tree declares — grounds against the GeometryGym scheduling-cost-resource family surface (`.api/api-geometrygym-ifc` scheduling-cost-resource rows 9-16, the `IfcCostItem.CostValues` `IfcCostValue` traversal row 8) so the `CostItemOf`/`ValueOf` projections discriminate the real cost graph rather than a guessed shape; the `IfcCostSchedule.Controls`/`PredefinedType`/`SubmittedOn`, `IfcCostItem.CostValues`/`Controls`/`Nests`/`PredefinedType`, `IfcCostValue.AppliedValue`/`UnitBasis`/`Category`, and `IfcRelAssignsToControl.RelatingControl`/`RelatedObjects` member spellings confirm against the catalogued surface before the projection fold is final — the `IfcCostSchedule.Controls`/`IfcCostItem.Controls` `IfcRelAssignsToControl` control path is the cost-control assignment relationship, distinct from the `IfcRelAssignsToProcess` resource-to-activity assignment the resource fold reads.
- [COST_VALUE_MONETARY]: the `IfcCostValue` applied-rate members the `ValueOf` fold reads onto the `CostValue` value object are verified against the live GeometryGym decompile — `IfcCostValue : IfcAppliedValue` inherits `AppliedValue` (`IfcAppliedValueSelect`), `UnitBasis` (`IfcMeasureWithUnit`), and `Category` (`String`); the `IfcMonetaryMeasure` direct-amount leg exposes `.Measure` (a `Double`, NOT `.Magnitude` — `IfcMonetaryMeasure : IfcDerivedMeasureValue`) onto `MonetaryAmount.Amount`; the `IfcMeasureWithUnit.UnitComponent` (`IfcUnit`, narrowed `is IfcMonetaryUnit` whose `.Currency` is the ISO 4217 code) onto `MonetaryAmount.Currency`; and the `IfcMeasureWithUnit.ValueComponent` (`IfcValue`, narrowed `is IfcMeasureValue` whose `.Measure` is the per-basis denominator) onto the `UnitBasis` denominator — so the `AmountOf`/`CurrencyOf`/`BasisOf` folds read the real `.Measure`-bearing measure shape, the `IfcCostValue.Category` string lowering onto the `CostCategory` discriminant.
- [RESOURCE_TASK_JOIN]: the construction-resource-to-activity join the `ResourceOf`/`TaskOf` folds read is verified against the live GeometryGym decompile — the `IfcConstructionResource` subtypes (`IfcLaborResource`/`IfcConstructionMaterialResource`/`IfcConstructionEquipmentResource`) the `ConstructionResource` union discriminates each carry only a `PredefinedType` typed kind in IFC4.3 (the IFC2x3 `IfcLaborResource.SkillSet` and a direct `RelatingMaterials` member do NOT exist here), so the `Labor` arm reads the `IfcLaborResourceTypeEnum PredefinedType` as the skill descriptor and the `Material` arm reads the consumed material name through the resource's inherited `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`as IfcMaterial`) `.Name`; the consumed quantity is the inherited `IfcConstructionResource.BaseQuantity` (`IfcPhysicalQuantity`, narrowed `is IfcPhysicalSimpleQuantity` whose `.MeasureValue.Measure` is the magnitude — there is no `.SimpleValue`); the task join is the inherited `HasAssignments` `IfcRelAssignsToProcess.RelatingProcess` controlled `IfcProcess` GlobalId, so the resource binds the `sequencing#ConstructionTask` it resources by the real assignment path, never re-modeling the activity network.
- [COST_ROLLUP_IDENTITY]: the `CostSchedule.Rollup` `(quantity x rate x resource)` fold and the `CostSchedule.Identity` `(QuantityKey, ResourceKey)` `UInt128` pair the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads the schedule by ground against the `Semantics/properties#PROPERTY_SETS` `QuantitySet.Derive` takeoff the `CostItem.ValueOf` line value reads — the line value is the takeoff quantity times the unit rate (`Applied.Amount / UnitBasis`) over the priced element set — and against the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` content-key idiom and the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` owner, so the catalog re-reads the estimate only on a changed `QuantityKey` (the priced line `(element, quantity, rate)` triples) or `ResourceKey` (the resource `(GlobalId, kind, baseQuantity)` rows) rather than re-pricing the schedule; the `Rollup` partitions the total by `CostCategory` and by `ResourceKind` through one `Fold` over the line and resource sets, never enumerated per-resource arms, and the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices — re-pricing the estimate or minting a second takeoff or schedule source downstream is the named cross-folder drift defect, the cost owner produces the priced schedule and its content-key identity, never the takeoff or the activity network.
