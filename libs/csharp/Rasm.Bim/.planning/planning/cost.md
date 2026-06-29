# [BIM_COST_ESTIMATE]

The host-neutral 5D cost-and-resource projection over the `Rasm.Element` seam graph: one `CostItem` line joining an `IfcCostValue` applied rate to the takeoff `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` the line resolves at projection from the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Element.Quantities` bag (or the cost line's own `IfcCostItem.CostQuantities`), one `ConstructionResource` record discriminated by the `ResourceKind` `[SmartEnum<string>]` over all six `IfcConstructionResource` modalities, and the `CostSchedule.Rollup` `Money`-fold that folds the resolved `(quantity x rate)` lines into one schedule total — one fold, never enumerated per-resource arms. The priced scalar is the `NodaMoney` `Money` `readonly struct` over a `decimal Amount` and an ISO 4217 `Currency`, never a `(double Amount, string Currency)` pair anywhere on the page: a `CostItem` value is `Money`, a unit rate is `Money / decimal`, the quantity x rate join is `Money * (decimal)quantity.Si`, a composite IFC `IfcCostValue.Components` rate folds through the `IfcArithmeticOperatorEnum` operator into one `Money`, the `CostSchedule.Rollup` is a `Money` sum over the additive operator (`Money.AdditiveIdentity` the no-currency anchor), a cross-currency rollup composes `NodaMoney.Exchange.ExchangeRate.Convert`, a lump-sum or contingency allocation across packages is the lossless `MoneyExtensions.Split` penny distribution rather than a remainder-losing `total / n` multiply, and the EARNED-VALUE metrics (BCWS/BCWP/ACWP/EAC/VAC) are `Money` with `CPI`/`SPI` the dimensionless `Money / Money → decimal` ratio — never a `(double, string Currency)` carrier. The whole estimate folds under one `NodaMoney.Context.MoneyContext` rounding/precision policy (the `CostMoney.Context` banker's-rounding scope the composition root installs) so a single rounding rule governs every line rather than a `MidpointRounding` argument threaded through every construction.

The estimate is a VIEW of the federated `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, never a second quantity or schedule source: each `CostItem` reads its takeoff from the one seam `QuantitySet` bag the `Semantics/properties#BASE_QUANTITIES` `QuantityDerivation` already derived from the kernel geometry the `Node.Object` references by content key, joins to the priced elements through the `IfcRelAssignsToControl.RelatedObjects` cost-control set resolved against the seam graph by the `Node.Object.ExternalId` (the 1:1 IFC `GlobalId` projection attribute [H6]), and binds the resource it consumes to the `Planning/schedule#SCHEDULE` `ConstructionTask` activity network through `IfcRelAssignsToProcess` rather than re-modeling a parallel schedule — so a wall's net-volume takeoff, its concrete unit rate, and the labor crew that places it carry one cost line while the seam keeps the element's content-keyed geometry and the schedule keeps the activity's calendar. The estimate is HOST-NEUTRAL — it reads the in-process GeometryGym cost graph and joins to the seam by stable `GlobalId`/`NodeId`, never a RhinoCommon type — and the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, never a second selection surface. The `(QuantityKey, ResourceKey)` `UInt128` content-key pair `CostSchedule.Identity` derives over the resolved `(globalIds, quantity, rate)` line triples is the reference the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads the schedule by at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`, and the `csharp:Rasm.AppUi/Charts` estimate report renders the same rollup — Bim PRODUCES the cost source, never re-pricing it downstream. A foreign IFC amount enters through the railed `CostMoney.Of((decimal)amount, iso4217, key)` boundary trapping the `InvalidCurrencyException` onto `Model/faults#FAULT_BAND` `BimFault.CodecReject`, and every cost rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail (band 2600 IS the `Expected` `Code`), never a `.ToError()` hop, a new fault family, or a thrown currency exception in domain logic.

## [01]-[INDEX]

- [01]-[ESTIMATE]: `CostItem` line (rate x resolved `MeasureValue` takeoff by `GlobalId`), `ConstructionResource` the ONE record discriminated by the `ResourceKind` `[SmartEnum<string>]` over the six `IfcConstructionResource` modalities, the `CostValue` rate carrier over the `NodaMoney` `Money`/`Currency`/`ExchangeRate` algebra with the `IfcCostValue.Components` aggregation fold, the `CostScheduleKind`/`ResourceKind`/`CostCategory` `[SmartEnum]` partitions, the `CostSchedule` record with its RAILED `Fin<CostRollup>` `Money`-fold `Rollup`, lossless `Apportion` `Split`, and CANONICALLY-ORDERED `(QuantityKey, ResourceKey)` identity, the `CostMoney` boundary lift + `MoneyContext` policy, and the `CostProjection.Project` fold from the GeometryGym `IfcCostSchedule` surface over the seam graph.
- [02]-[EARNED_VALUE]: the `ChangeOrder` priced-revision record over a baseline `CostSchedule`, the `Contingency` `Money` reserve column, and the total `CostSchedule.EarnedValue` fold (BCWS/BCWP/ACWP as `Money`, CPI/SPI as `decimal`, EAC/VAC as `Money`) joining the `Planning/schedule#SCHEDULE` `ConstructionTask` actual progress to the self-contained priced lines.

## [02]-[ESTIMATE]

- Owner: `CostSchedule` the single host-neutral 5D cost-and-resource record carrying the `CostScheduleKind` discriminant, the self-contained priced `CostItem` line set (each line carrying its resolved takeoff `MeasureValue`), the `ConstructionResource` resource set, the `Contingency` reserve, and the `(QuantityKey, ResourceKey)` content-key identity the cross-libs cost catalog reads it by, with the railed `Fin<CostRollup>` `Money`-fold `Rollup` schedule-total and the lossless `Apportion` lump-sum distribution; `CostItem` the single priced line record joining the `IfcCostValue` `CostValue` applied rate to the resolved `MeasureValue Quantity` (the seam takeoff or the explicit `IfcCostItem.CostQuantities`) by the priced element `GlobalId` set, carrying the optional `ResourceGlobalId` it consumes and the `ParentGlobalId` nesting reference the `IfcCostItem` `Nests` hierarchy declares — its `ValueOf` the pure `Rate * (decimal)Quantity.Si` join needing no graph; `ConstructionResource` the ONE record discriminated by the `ResourceKind` `[SmartEnum<string>]` over the six `IfcConstructionResource` modalities (`Labor`/`Material`/`Equipment`/`Crew`/`Product`/`Subcontract`), carrying its `MeasureValue BaseQuantity`, an optional `Money BaseCost` from `IfcConstructionResource.BaseCosts`, the optional `Skill` (the `IfcLaborResourceTypeEnum`/`IfcCrewResourceTypeEnum` `PredefinedType` descriptor), the optional consumed `Material` (read through the resource's `HasAssociations` `IfcRelAssociatesMaterial`), and the optional `TaskGlobalId` it resources through `IfcRelAssignsToProcess` — never a per-subtype class family; `CostValue` the applied-rate record carrying its `Money` value (the fold of the `IfcCostValue` direct amount OR its `Components` tree through the `ArithmeticOperator`), its `UnitBasis` per-unit `decimal` denominator, and its `CostCategory` discriminant, the per-basis rate the native `Money / decimal` divide; `CostScheduleKind`/`ResourceKind`/`CostCategory` the cost-schedule-kind / resource-modality / cost-category `[SmartEnum<string>]` vocabularies; `CostMoney` the boundary lift folding a foreign IFC `(double amount, string iso4217)` into a typed `Money` trapping the `InvalidCurrencyException` onto `BimFault.CodecReject` plus the `MoneyContext` rounding policy; `CostProjection` the static fold over the GeometryGym `IfcCostSchedule` surface and the seam `ElementGraph`.
- Cases: `ConstructionResource` is ONE record whose `ResourceKind` row discriminates the modality — `Labor` (`IfcLaborResource`, `Skill` = `IfcLaborResourceTypeEnum` `PredefinedType`, `BaseQuantity` crew man-hours), `Material` (`IfcConstructionMaterialResource`, `Material` = the consumed material name through `HasAssociations`, `BaseQuantity` consumed volume/mass), `Equipment` (`IfcConstructionEquipmentResource`, `BaseQuantity` plant-hours), `Crew` (`IfcCrewResource`, `Skill` = `IfcCrewResourceTypeEnum`), `Product` (`IfcConstructionProductResource`, `Material` = the product), `Subcontract` (`IfcSubContractResource`) — each carrying the optional `BaseCost` and `TaskGlobalId`, a seventh modality being one `ResourceKind` row with zero new surface (6 modalities, the `[SmartEnum]` partition the discriminant); the `CostValue` value object carries its `Money` `Applied` rate (the `IfcCostValue.AppliedValue` `IfcMonetaryMeasure.Measure` lifted with the `IfcMonetaryUnit.Currency` ISO 4217 code TOGETHER into one `Money`, or the `IfcAppliedValue.Components` sub-value tree folded through the `IfcArithmeticOperatorEnum` — never a `double` amount beside a bare currency string), its `UnitBasis` per-unit `decimal` denominator (`1m` for a unit rate, the `IfcCostValue.UnitBasis` `IfcMeasureValue` magnitude for a per-basis rate), and its `CostCategory` (`Material`/`Labour`/`Equipment`/`Overhead`/`Subcontract`/`Preliminaries`/`Contingency`/`NotDefined` over the `IfcCostValue.Category` string) — the line rate the native `Applied / UnitBasis` (`Money / decimal → Money`) before multiplying the takeoff quantity; the `CostScheduleKind` rows `Budget`/`CostPlan`/`Estimate`/`Tender`/`PricedBillOfQuantities`/`UnpricedBillOfQuantities`/`ScheduleOfRates`/`UserDefined`/`NotDefined` (9) each frozen over the verified `IfcCostScheduleTypeEnum` member, the `ResourceKind` rows `Labor`/`Material`/`Equipment`/`Crew`/`Product`/`Subcontract`/`UserDefined`/`NotDefined` (8) each frozen with its `IfcDomain`, and the `CostCategory` rows (8) over the IFC cost category string.
- Entry: `CostProjection.Project(IfcCostSchedule schedule, Seq<IfcConstructionResource> resources, ElementGraph graph, Op key)` folds one GeometryGym cost schedule into one self-contained `CostSchedule` — materializing the schedule's `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcCostItem` set once, building the `ExternalId → NodeId` index over `graph.ObjectNodes` once, folding each cost item onto a `CostItem` line that resolves its `CostValues` applied rate (the `Components` tree included), resolves the priced element `GlobalId` set against the index (`BimFault.DanglingReference` BARE on a priced `GlobalId` the seam graph never declares as a `Node.Object.ExternalId`), and resolves the line takeoff `MeasureValue` (the explicit `IfcCostItem.CostQuantities` when present, else the dominant base quantity off the priced elements' seam `Bake`d `Element.Quantities`, else a unit lump-sum), and folding the resource set onto `ConstructionResource` rows discriminated by `ResourceKind.Of(resource.GetType().Name)` (a non-construction-resource entity `Choose`-discarded rather than aborting the whole schedule); `CostProjection.ProjectAll` lifts every cost schedule in a federated graph onto the `Seq<CostSchedule>` the catalog reads, `CostSchedule.Rollup(Op key, Option<ExchangeRate> fx = default)` folds the resolved lines into the `Fin<CostRollup>` schedule total (railing `Model/faults#FAULT_BAND` `BimFault.CodecReject` on a cross-currency line carrying no matching `ExchangeRate`, never a thrown `Money + Money` mismatch in domain logic), and `CostSchedule.Apportion(Money lumpSum)` distributes a lump-sum (overhead, contingency) across the lines by their value-weight ratios through the lossless `MoneyExtensions.Split`.
- Auto: `Project` reads the `IfcCostSchedule` runtime graph and folds it into the self-contained schedule — the `ItemsOf` projection materializes the schedule's `Controls` `IfcRelAssignsToControl` controlled `IfcCostItem` set once; `ValueOf` folds each item's first `IfcCostValue` onto the `CostValue` value object (`AmountOf` reading the `IfcMonetaryMeasure.Measure` and the `UnitBasis.UnitComponent` `IfcMonetaryUnit.Currency` ISO 4217 code TOGETHER into one `Money` through `CostMoney.Of`, OR recursively folding the `IfcAppliedValue.Components` sub-value tree through the `IfcArithmeticOperatorEnum` `ADD`/`SUBTRACT`/`MULTIPLY`/`DIVIDE` operator, the `IfcCostValue.UnitBasis.ValueComponent` `IfcMeasureValue.Measure` onto the `decimal UnitBasis`, the `IfcCostValue.Category` onto `CostCategory`); `QuantityOf` resolves the line takeoff once at projection (the explicit `IfcCostItem.CostQuantities` `IfcPhysicalSimpleQuantity` set decoded through the owned `Projection/semantic#VALUE_NARROWING` `PropertyLowering.Measure`, else `graph.Bake(node, key)` over each priced element and the dominant `MeasureValue` by `Dimension` rank `Volume ≻ Area ≻ Length ≻ Mass ≻ Duration` summed through `MeasureValue.Sum`, else the `Dimensionless` unit lump-sum), so the line value is the pure `CostValue.Rate * (decimal)Quantity.Si` and the schedule needs no graph after projection; `CostItemOf` threads the `IfcCostItem` `Nests.RelatingObject.GlobalId` parent onto `ParentGlobalId` so a bill-of-quantities tree folds onto the flat line set with its nesting preserved; `ResourceOf` is ONE fold reading each `IfcConstructionResource` by runtime subtype onto a `ConstructionResource` row — `ResourceKind.Of(GetType().Name)` the discriminant, `MeasureOf(resource.BaseQuantity)` the `MeasureValue BaseQuantity`, the first `IfcConstructionResource.BaseCosts` `IfcAppliedValue` onto the optional `Money BaseCost`, the `IfcLaborResource`/`IfcCrewResource` `PredefinedType` onto `Skill`, the `IfcConstructionMaterialResource`/`IfcConstructionProductResource` associated `IfcMaterial.Name` onto `Material`, and the `OperatesOn`/`HasAssignments` `IfcRelAssignsToProcess.RelatingProcess` `IfcProcess.GlobalId` onto `TaskGlobalId` so the resource binds the `Planning/schedule#SCHEDULE` `ConstructionTask` it resources; `CostSchedule.Rollup` reprices every line `ValueOf()` `Money` into the reporting currency (`ExchangeRate.Convert`, railing `BimFault.CodecReject` when no matching rate exists rather than throwing a `Money + Money` mismatch) then sums them into the `Fin<CostRollup>` schedule total through the additive operator, partitions the total by `CostCategory` and the resource cost (`BaseCost * BaseQuantity`) by `ResourceKind` through one `Fold`; `CostSchedule.Identity` derives the `(QuantityKey, ResourceKey)` `UInt128` pair through `XxHash128.HashToUInt128` over the CANONICALLY-ORDERED priced line `(globalIds, quantity.Si, rate)` triples and resource `(GlobalId, kind, baseQuantity.Si)` rows (the line set by `GlobalId`, each priced-id and resource set sorted ordinally) so the key is invariant to the unstable `IfcSet` iteration order a re-parse yields and the catalog re-reads only a genuinely changed estimate.
- Receipt: the `Seq<CostSchedule>` is the cost evidence the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads by the `(QuantityKey, ResourceKey)` reference at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` and the `csharp:Rasm.AppUi/Charts` estimate report renders, the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, and the `CostRollup` schedule total with its `CostCategory`/`ResourceKind` partitions is the 5D estimate evidence; the priced line carries its resolved takeoff, applied rate, and resourced task on one self-contained record joining the seam quantity and the schedule activity by reference, never a second quantity or schedule store.
- Packages: GeometryGymIFC_Core, NodaMoney, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new cost-schedule kind is one `CostScheduleKind` row reading the next `IfcCostScheduleTypeEnum` member; a new construction-resource modality is one `ResourceKind` row with zero new surface (the ONE `ConstructionResource` record absorbs it on the discriminant — never a new union arm or class); a new cost category is one `CostCategory` row; a composite rate rides the existing `IfcAppliedValue.Components` fold; a new per-line binding is one column on `CostItem`; a regional or custom currency is one `CurrencyRegistry.TryAdd(CurrencyInfo)` registration; a new rounding rule (cash-denomination, exact) is one `CostMoney.Context` `MoneyContext` policy (`CashDenominationRounding`/`NoRounding`), never a `MidpointRounding` argument; a new cost schedule rides the existing `ProjectAll` fold on one row; never a per-resource-type cost record, never a parallel `LaborCost`/`MaterialCost` class family, never a `GetLaborCost`/`GetByCategory` operation family, never a hand-rolled `MonetaryAmount` `(double, string)` carrier beside `Money`, and never a second takeoff or schedule source.
- Boundary: `CostSchedule` is ONE record discriminated by the `CostScheduleKind` row, and `ConstructionResource` is ONE record discriminated by the `ResourceKind` `[SmartEnum]` over all six IFC modalities — a `Labor`/`Material`/`Equipment` `[Union]` slicing only three of six subtypes, a `LaborResource`/`MaterialResource` class family, or three sibling factory methods is the deleted form (the collapse to one record keyed by the smart enum removes the repeated `Switch` accessors and covers every modality), mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`; the priced scalar is the `NodaMoney` `Money` `readonly struct` and a hand-rolled `MonetaryAmount` `(double, string)` record, a `double` cost-arithmetic helper, a naive `total / n` allocation where `MoneyExtensions.Split` is lossless, a stringly currency field validated by hand where the `Money(decimal, string)` ctor resolves the ISO 4217 registry, a thrown `InvalidCurrencyException` in domain code instead of the railed `CostMoney.Of`, and a second rounding policy threaded as a `MidpointRounding` argument where the one `CostMoney.Context` `MoneyContext` governs are the RETIRED forms; the estimate is a VIEW of the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` — the priced quantity is the seam `QuantitySet` `MeasureValue` takeoff joined by `Node.Object.ExternalId` (or the explicit `IfcCostItem.CostQuantities`), a re-derived parallel takeoff or a re-tessellation in this owner being the named seam violation, and the resourced schedule is the `Planning/schedule#SCHEDULE` `ConstructionTask` network joined by `TaskGlobalId`, a re-modeled cost-side schedule being the named seam violation; the retired `BimModel`/`BimElement` collection is GONE — a `federated.Elements` scan over a second stored element record is the deleted form, the cost reading the seam graph the `Bake` fold derives the consumer `Element` from; the GeometryGym `IfcCostSchedule`/`IfcCostItem`/`IfcCostValue`/`IfcAppliedValue`/`IfcConstructionResource` and its six subtypes / `IfcRelAssignsToControl`/`IfcRelAssignsToProcess`/`IfcRelAssociatesMaterial` surface (`.api/api-geometrygym-ifc` scheduling-cost-resource rows 8-16) is consumed as settled vocabulary and a hand-rolled cost reader is the deleted form, the `IfcPhysicalSimpleQuantity`->`MeasureValue` decode composing the owned `Projection/semantic#VALUE_NARROWING` `PropertyLowering.Measure` (one decode owner) rather than a duplicate dimension/value switch; the `NodaMoney` `Money`/`Currency` cross the `Exchange/wire` boundary through `MoneyJsonConverter`/`CurrencyJsonConverter` or the integer `ToMinorUnits` form and never leak past the cost owner; the quantified-element selection is the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` predicate and a parallel cost-element selection arm is the no-second-selection-surface reject; the `(QuantityKey, ResourceKey)` identity is derived through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom over canonically-ordered line/resource rows (invariant to the unstable `IfcSet` iteration order) and minting a second identity scheme for the catalog join is the named cross-folder drift defect; the `CostSchedule.Rollup` total is ONE RAILED `Money` fold over the resolved lines (a cross-currency line lacking a matching `ExchangeRate` lifting `BimFault.CodecReject` BARE, never a thrown `Money + Money` mismatch in domain logic), never enumerated per-resource arms; a cost rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail and a `.ToError()` lowering hop or a bare `Fin.Fail` without the typed case is the named defect the rebuilt `Model/faults#FAULT_BAND` band closes.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO.Hashing;
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using NodaMoney;
using NodaMoney.Context;
using NodaMoney.Exchange;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class CostScheduleKind {
    public static readonly CostScheduleKind Budget                   = new("BUDGET");
    public static readonly CostScheduleKind CostPlan                 = new("COSTPLAN");
    public static readonly CostScheduleKind Estimate                 = new("ESTIMATE");
    public static readonly CostScheduleKind Tender                   = new("TENDER");
    public static readonly CostScheduleKind PricedBillOfQuantities   = new("PRICEDBILLOFQUANTITIES");
    public static readonly CostScheduleKind UnpricedBillOfQuantities = new("UNPRICEDBILLOFQUANTITIES");
    public static readonly CostScheduleKind ScheduleOfRates          = new("SCHEDULEOFRATES");
    public static readonly CostScheduleKind UserDefined              = new("USERDEFINED");
    public static readonly CostScheduleKind NotDefined               = new("NOTDEFINED");

    public static CostScheduleKind Of(IfcCostScheduleTypeEnum kind) =>
        TryGet(kind.ToString()).IfNone(NotDefined);
}

// The resource-modality discriminant: ONE [SmartEnum] keyed on the IfcConstructionResource subtype name,
// each row carrying its IfcDomain. The ConstructionResource record reads the row through Of(GetType().Name),
// so a seventh modality is one row with zero new surface — never a per-subtype union arm.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
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
// The NodaMoney boundary lift + the ambient rounding policy. A foreign IFC (double measure, ISO 4217 code)
// folds into the typed `Money` once at the projection edge through the RAILED Of: the `Money(decimal, string)`
// ctor resolves the embedded ISO 4217 registry, an unknown code's thrown InvalidCurrencyException trapping onto
// BimFault.CodecReject (lifted BARE — band 2600 IS the Expected Code, no .ToError() hop) so domain code never
// sees a thrown currency fault and never calls a non-railed Of. An empty code yields a Money over
// Currency.NoCurrency (the additive identity the rollup seeds). `Context` is the one MoneyContext rounding/
// precision policy the static initializer installs AS THE PROCESS DEFAULT via MoneyContext.CreateAndSetDefault, so
// every Money operator folds under one banker's-rounding rule AND the EnforceZeroCurrencyMatching=false relaxation
// lets the Currency.NoCurrency additive identity seed a currency-bearing sum — never a MidpointRounding argument
// threaded through each construction; a growth rounding rule (CashDenominationRounding/NoRounding) swaps this one
// policy and TAKES EFFECT, where a non-installed Create would leave the swap inert (the dead-field defect).
public static class CostMoney {
    public static readonly MoneyContext Context = MoneyContext.CreateAndSetDefault(static options => {
        options.RoundingStrategy = new StandardRounding();
        options.EnforceZeroCurrencyMatching = false;
    }, name: "rasm-cost");

    public static Fin<Money> Of(decimal amount, string iso4217, Op key) =>
        iso4217.Trim() is { Length: > 0 } code
            ? Try.lift(() => new Money(amount, code)).Run()
                .MapFail(error => new BimFault.CodecReject(key, $"cost-currency:{error.Message}"))
            : Fin.Succ(new Money(amount, Currency.NoCurrency));
}

// The applied rate is a `Money` over a `decimal` amount + a resolved `Currency` — never a `double` amount beside
// a bare currency string. The per-basis rate is the native `Money / decimal` divide (UnitBasis the IfcMeasureValue
// magnitude, 1m for a unit rate), so the line value arithmetic stays in the decimal-precision operator set.
public sealed record CostValue(Money Applied, decimal UnitBasis, CostCategory Category) {
    public static readonly CostValue Zero = new(new Money(0m, Currency.NoCurrency), 1m, CostCategory.NotDefined);

    public Money Rate => UnitBasis == 0m ? Applied : Applied / UnitBasis;
}

// ONE construction-resource record discriminated by the ResourceKind row over the six IFC modalities — the
// migration source's 3-arm Labor/Material/Equipment [Union] (a naive slice dropping Crew/Product/Subcontract,
// with four Switch accessors re-projecting shared fields) is COLLAPSED here: the kind drives the modality, the
// kind-specific data (Skill for Labor/Crew, Material for Material/Product) rides Option fields, BaseQuantity is a
// seam MeasureValue (never a bare double), and BaseCost lifts IfcConstructionResource.BaseCosts so the resource
// contributes Cost = BaseCost x BaseQuantity to the by-resource rollup partition.
public sealed record ConstructionResource(
    string GlobalId,
    string Name,
    ResourceKind Kind,
    MeasureValue BaseQuantity,
    Option<Money> BaseCost,
    Option<string> Skill,
    Option<string> Material,
    Option<string> TaskGlobalId) {
    public Money Cost => BaseCost.Map(c => c * (decimal)BaseQuantity.Si).IfNone(Money.AdditiveIdentity);
}

public sealed record CostItem(
    string GlobalId,
    string Name,
    CostValue Value,
    MeasureValue Quantity,
    Seq<string> PricedGlobalIds,
    Option<string> ResourceGlobalId,
    Option<string> ParentGlobalId) {
    // The line value is the unit rate times the takeoff resolved ONCE at projection — pure decimal-precision Money
    // (Money * decimal -> Money), no graph: a no-currency rate (Currency.NoCurrency) contributes the additive
    // identity so an unpriced line never poisons the sum, and the cross-multiply is never a `double`.
    public Money ValueOf() => Value.Rate * (decimal)Quantity.Si;
}

public sealed record CostRollup(
    Money Total,
    Map<string, Money> ByCategory,
    Map<string, Money> ByResourceKind);

public sealed record CostSchedule(
    string GlobalId,
    CostScheduleKind Kind,
    string Name,
    Seq<CostItem> Items,
    Seq<ConstructionResource> Resources,
    Contingency Contingency) {
    public CostSchedule(string globalId, CostScheduleKind kind, string name, Seq<CostItem> items, Seq<ConstructionResource> resources)
        : this(globalId, kind, name, items, resources, Contingency.None) { }

    public CostSchedule Drawdown(Money draw) => this with { Contingency = Contingency.Drawdown(draw) };

    // The identity hashes the RESOLVED (globalIds, quantity.Si, rate) line triples and the resource rows under a
    // CANONICAL ordering (the line set by GlobalId, each priced-id set and the resource set sorted ordinally) so the
    // content key is INVARIANT to the unstable IfcSet iteration order a re-parse yields — the QuantityKey changes
    // only when a takeoff genuinely changes (the catalog re-reads only a changed estimate), never on an incidental
    // reorder and never the rate-only key the migration source hashed while its prose claimed the quantity was keyed.
    public (UInt128 QuantityKey, UInt128 ResourceKey) Identity => (
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join("",
            Items.OrderBy(static i => i.GlobalId, StringComparer.Ordinal).Select(static i =>
                $"{string.Join(",", i.PricedGlobalIds.OrderBy(static g => g, StringComparer.Ordinal))}={i.Quantity.Si:R}x{i.Value.Rate.Amount}{i.Value.Rate.Currency.Code}@{i.Value.Category.Key}")))),
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join("",
            Resources.OrderBy(static r => r.GlobalId, StringComparer.Ordinal).Select(static r => $"{r.GlobalId}={r.Kind.Key}:{r.BaseQuantity.Si:R}")))));

    // The schedule total is ONE RAILED Money fold over the self-contained lines — every line ValueOf() reprices to
    // the reporting currency, THEN sums through the additive operator (the additive identity the empty seed), the
    // by-category partition the same fold keyed on the category, and the by-resource-kind partition the resource
    // Cost (BaseCost x BaseQuantity) — never enumerated per-resource arms or a manual `double` accumulation. A
    // cross-currency line carrying no matching ExchangeRate lifts BimFault.CodecReject onto the Fin rail rather than
    // letting a different-currency `Money + Money` THROW in domain logic, so the fold is total over one reporting currency.
    public Fin<CostRollup> Rollup(Op key, Option<ExchangeRate> fx = default) {
        var report = Items.HeadOrNone().Map(static i => i.Value.Applied.Currency).IfNone(Currency.NoCurrency);
        return Items
            .TraverseM(item => Reprice(item.ValueOf(), report, fx, key).Map(amount => (Key: item.Value.Category.Key, Amount: amount)))
            .As()
            .Map(lines => new CostRollup(
                lines.Fold(Money.AdditiveIdentity, static (total, line) => total + line.Amount),
                lines.Fold(Map<string, Money>(), static (by, line) =>
                    by.AddOrUpdate(line.Key, existing => existing + line.Amount, line.Amount)),
                Resources.Fold(Map<string, Money>(), static (by, resource) =>
                    by.AddOrUpdate(resource.Kind.Key, existing => existing + resource.Cost, resource.Cost))));
    }

    // Lossless lump-sum apportionment: distribute an overhead/contingency lump-sum across the lines by their
    // value-weight ratios through MoneyExtensions.Split — the remainder pennies spread so the parts sum EXACTLY,
    // the allocation a naive `lump / count` multiply silently loses. A zero-weight line set returns the lump unsplit.
    public Seq<(CostItem Line, Money Share)> Apportion(Money lumpSum) =>
        Items.Map(static i => Weight(i)).ToArray() is { Length: > 0 } weights && weights.Any(static w => w > 0)
            ? Items.Zip(lumpSum.Split(weights).ToSeq(), static (line, share) => (Line: line, Share: share))
            : Items.Map(static line => (Line: line, Share: Money.AdditiveIdentity));

    // The integer ratio weight (cents, clamped to the int domain so a billion-unit line never overflows the
    // Split ratio array) — proportional to the line value so the lump-sum distributes by cost weight.
    static int Weight(CostItem line) =>
        (int)Math.Clamp(Math.Round(line.ValueOf().Amount * 100m, MidpointRounding.AwayFromZero), 0m, int.MaxValue);

    // A line already in the report currency (or the no-currency additive zero) passes; a foreign line converts through
    // a matching ExchangeRate; a foreign line with NO matching rate rails CodecReject (shared `cost-currency` detail
    // family) rather than returning a mismatched currency the rollup `+` would throw on — no thrown mismatch escapes.
    static Fin<Money> Reprice(Money line, Currency report, Option<ExchangeRate> fx, Op key) =>
        line.Currency == report || line.Currency == Currency.NoCurrency
            ? Fin.Succ(line)
            : fx.Filter(rate => rate.BaseCurrency == line.Currency).Match(
                Some: rate => Fin.Succ(rate.Convert(line)),
                None: () => FinFail<Money>(new BimFault.CodecReject(key, $"cost-currency:unconvertible:{line.Currency.Code}>{report.Code}")));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CostProjection {
    public static Fin<CostSchedule> Project(IfcCostSchedule schedule, Seq<IfcConstructionResource> resources, ElementGraph graph, Op key) {
        var index = graph.ObjectNodes.Fold(Map<string, NodeId>(),
            static (map, o) => o.ExternalId.Match(Some: id => map.AddOrUpdate(id, o.Id), None: () => map));
        return ItemsOf(schedule)
            .TraverseM(item => CostItemOf(item, index, graph, key))
            .As()
            .Map(items => new CostSchedule(
                schedule.GlobalId,
                CostScheduleKind.Of(schedule.PredefinedType),
                schedule.Name ?? "",
                items,
                resources.Choose(r => ResourceOf(r, key))));
    }

    public static Fin<Seq<CostSchedule>> ProjectAll(Seq<IfcCostSchedule> schedules, Seq<IfcConstructionResource> resources, ElementGraph graph, Op key) =>
        schedules.TraverseM(schedule => Project(schedule, resources, graph, key)).As();

    static Seq<IfcCostItem> ItemsOf(IfcCostSchedule schedule) =>
        schedule.Controls
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcCostItem>()
            .ToSeq();

    static Fin<CostItem> CostItemOf(IfcCostItem item, Map<string, NodeId> index, ElementGraph graph, Op key) {
        var priced = item.Controls
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .Select(static o => o.GlobalId)
            .Where(static id => id.Length > 0)
            .ToSeq();
        return priced.Find(id => !index.ContainsKey(id)).Match(
            Some: id => FinFail<CostItem>(new BimFault.DanglingReference(key, $"cost-priced-miss:{id}")),
            None: () =>
                from value in ValueOf(item, key)
                from quantity in QuantityOf(item, priced.Choose(index.Find).ToSeq(), graph, key)
                select new CostItem(item.GlobalId, item.Name ?? "", value, quantity, priced, None,
                    Optional(item.Nests?.RelatingObject?.GlobalId)));
    }

    // The applied amount + ISO 4217 currency lift TOGETHER into one Money, OR the IfcAppliedValue.Components sub-
    // value tree folds through the IfcArithmeticOperatorEnum so a composite rate (material + labor + equipment
    // sub-rates) resolves rather than reading only the head value the migration source did.
    static Fin<CostValue> ValueOf(IfcCostItem item, Op key) =>
        item.CostValues.AsIterable().HeadOrNone().Match(
            Some: value =>
                from applied in AmountOf(value, key)
                select new CostValue(applied, BasisOf(value), CostCategory.Of(value.Category ?? "")),
            None: () => Fin.Succ(CostValue.Zero));

    static Fin<Money> AmountOf(IfcAppliedValue value, Op key) =>
        value.AppliedValue is IfcMonetaryMeasure monetary
            ? CostMoney.Of((decimal)monetary.Measure, CurrencyOf(value), key)
            : value.Components.AsIterable().ToSeq() is { IsEmpty: false } components
                ? components.TraverseM(c => AmountOf(c, key)).As().Map(parts => Aggregate(value.ArithmeticOperator, parts))
                : Fin.Succ(new Money(0m, Currency.NoCurrency));

    static Money Aggregate(IfcArithmeticOperatorEnum op, Seq<Money> parts) =>
        parts.IsEmpty ? Money.AdditiveIdentity : op switch {
            IfcArithmeticOperatorEnum.SUBTRACT => parts.Tail.Fold(parts.Head, static (a, p) => a - p),
            IfcArithmeticOperatorEnum.MULTIPLY => parts.Tail.Fold(parts.Head, static (a, p) => a * p.Amount),
            IfcArithmeticOperatorEnum.DIVIDE   => parts.Tail.Fold(parts.Head, static (a, p) => p.Amount == 0m ? a : a / p.Amount),
            _                                  => parts.Fold(Money.AdditiveIdentity, static (a, p) => a + p),
        };

    static string CurrencyOf(IfcAppliedValue value) =>
        value.UnitBasis?.UnitComponent is IfcMonetaryUnit unit ? unit.Currency : "";

    static decimal BasisOf(IfcAppliedValue value) =>
        value.UnitBasis?.ValueComponent is IfcMeasureValue basis && basis.Measure > 0d ? (decimal)basis.Measure : 1m;

    // The line takeoff resolved ONCE at projection: the explicit IfcCostItem.CostQuantities (the priced BoQ
    // quantity) when present, else the dominant base quantity off each priced element's seam Bake (Volume ≻ Area ≻
    // Length ≻ Mass ≻ Duration) summed through MeasureValue.Sum, else a Dimensionless unit lump-sum — so a line
    // with neither an explicit nor a derived quantity prices at its rate (quantity 1) rather than zero.
    static Fin<MeasureValue> QuantityOf(IfcCostItem item, Seq<NodeId> priced, ElementGraph graph, Op key) =>
        Measures(item.CostQuantities.AsIterable().ToSeq()) is { IsEmpty: false } explicitQuantities
            ? Dominant(explicitQuantities, key)
            : priced.TraverseM(id => graph.Bake(id, key)).As()
                .Bind(elements => Dominant(elements.Bind(static e => e.Quantities).Bind(static b => b.Quantities.Values.ToSeq()), key));

    static readonly Seq<Dimension> PricingRank =
        Seq(Dimension.VolumeDim, Dimension.AreaDim, Dimension.LengthDim, Dimension.MassDim, Dimension.DurationDim);

    static Fin<MeasureValue> Dominant(Seq<MeasureValue> measures, Op key) =>
        PricingRank.Choose(d => measures.Filter(m => m.Dimension == d) is { IsEmpty: false } same ? Some(same) : None)
            .HeadOrNone()
            .Match(Some: same => MeasureValue.Sum(same, key), None: () => Fin.Succ(MeasureValue.OfSi(Dimension.Dimensionless, 1d)));

    static Seq<MeasureValue> Measures(Seq<IfcPhysicalQuantity> quantities) =>
        quantities.Choose(MeasureOf);

    // The IfcPhysicalSimpleQuantity -> seam MeasureValue decode is OWNED by Projection/semantic#VALUE_NARROWING
    // PropertyLowering.Measure (type-pattern dispatch over the six subtype value accessors LengthValue/AreaValue/
    // VolumeValue/WeightValue/CountValue/TimeValue, SI-base direct); the cost read COMPOSES that one Bim-internal
    // owner — a parallel GetType().Name dimension switch reading the base IfcMeasureValue accessor is the duplicate
    // form deleted here (one-owner + SYMBOLIC_REFERENCE: the type is the discriminant, never its name string).
    static Option<MeasureValue> MeasureOf(IfcPhysicalQuantity? quantity) =>
        quantity is IfcPhysicalSimpleQuantity simple ? Some(PropertyLowering.Measure(simple)) : None;

    // ONE resource fold reading each IfcConstructionResource by runtime subtype onto a ConstructionResource row —
    // the ResourceKind discriminant, the BaseQuantity MeasureValue, the first BaseCosts amount as the optional Money
    // rate, the Skill/Material per modality, and the OperatesOn task GlobalId; a non-construction entity Choose-drops.
    static Option<ConstructionResource> ResourceOf(IfcConstructionResource resource, Op key) =>
        ResourceKind.Of(resource.GetType().Name) is var kind && kind == ResourceKind.NotDefined
            ? None
            : Some(new ConstructionResource(
                resource.GlobalId, resource.Name ?? "", kind,
                MeasureOf(resource.BaseQuantity).IfNone(MeasureValue.Zero),
                BaseCostOf(resource, key),
                SkillOf(resource),
                MaterialOf(resource),
                TaskOf(resource)));

    static Option<Money> BaseCostOf(IfcConstructionResource resource, Op key) =>
        resource.BaseCosts.AsIterable().HeadOrNone()
            .Bind(value => AmountOf(value, key).ToOption());

    static Option<string> SkillOf(IfcConstructionResource resource) => resource switch {
        IfcLaborResource labor => Some(labor.PredefinedType.ToString()),
        IfcCrewResource crew   => Some(crew.PredefinedType.ToString()),
        _                      => None,
    };

    static Option<string> MaterialOf(IfcConstructionResource resource) =>
        resource is IfcConstructionMaterialResource or IfcConstructionProductResource
            ? resource.HasAssociations
                .AsIterable()
                .OfType<IfcRelAssociatesMaterial>()
                .HeadOrNone()
                .Bind(static rel => Optional((rel.RelatingMaterial as IfcMaterial)?.Name))
            : None;

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

- Owner: `ChangeOrder` the priced-revision record carrying the baseline `CostSchedule` `GlobalId`, the priced `CostItem` delta set (added/modified/removed lines against the baseline), the `ChangeOrderStatus` `[SmartEnum<string>]` approval state, and the revision `Instant`; `Contingency` a `CostCategory.Contingency` `Money` reserve carried on `CostSchedule` a drawdown decrements through the native `Money` subtraction; `EarnedValueReport` the typed receipt carrying BCWS (planned value), BCWP (earned value), ACWP (actual cost) as `Money`, the cost-performance index `CPI = BCWP/ACWP` and schedule-performance index `SPI = BCWP/BCWS` as the dimensionless `Money / Money → decimal` ratio, and the estimate-at-completion `EAC = BAC/CPI` and variance-at-completion `VAC = BAC − EAC` as `Money` — never a `(double, string Currency)` carrier (the currency rides each `Money`); `CostSchedule.EarnedValue` the TOTAL fold joining the `Planning/schedule#SCHEDULE` `ConstructionTask` actual/scheduled progress and the external `actuals` incurred-cost feed to the self-contained priced lines at a status `Instant`, never a generic ledger.
- Entry: `CostSchedule.EarnedValue(ScheduleNetwork network, Instant statusDate, Map<string, Money> actuals = default)` folds the earned-value report at a status date — each `CostItem` joins its priced element set to the `Planning/schedule#SCHEDULE` `ConstructionTask` that assigns it (by the `TaskAssignment` `GlobalId` membership), reads the task's planned percent-complete (the fraction of the task's scheduled `Interval` elapsed at `statusDate`) and actual percent-complete (the fraction of the task's actual `Interval` elapsed, or `1.0` when the task `Status` is `Completed`), and contributes `line.ValueOf() × plannedPercent` to BCWS and `× actualPercent` to BCWP, while ACWP reads the line's recorded incurred cost-to-date the `actuals` map supplies per `CostItem.GlobalId` (the EVM cost axis an accounting/Persistence feed produces) and falls back to the schedule-duration proxy (the earned value scaled by the actual-vs-scheduled duration-overrun ratio) ONLY when a line carries no recorded actual — so with `actuals` supplied CPI is a TRUE cost index independent of SPI, and without them the report degrades to a schedule-derived forecast rather than fabricating a cost index; the report partitions BCWS/BCWP/ACWP `Money` over the line set and derives the CPI/SPI/EAC/VAC scalars; the fold is TOTAL (a line whose assigning task the network never declares still contributes its budget to BAC and its recorded spend to ACWP, never a fault) so no graph and no rail are needed — the line value is the pure `CostItem.ValueOf()`. `ChangeOrder.Apply(CostSchedule baseline)` folds the priced delta set onto the baseline producing the revised `CostSchedule` (the delta lines added/superseding/removing the baseline lines by `GlobalId`) so a revision is the existing `CostItem`/`CostValue` algebra applied against a baseline, never a parallel revision store, and `CostSchedule.Drawdown(Money draw)` decrements the `Contingency` `Money` reserve returning the remaining reserve floored at the additive identity.
- Auto: `EarnedValue` reads each `CostItem.ValueOf()` budgeted line value (the pure `Quantity × rate`, the `BAC` budget-at-completion summing the line set as `Money`), resolves each line's assigning `ConstructionTask` through the `network.Assignments` `TaskAssignment` join (a line's priced `GlobalId` set intersects the task's assigned `GlobalId` set), computes the task planned percent as `(statusDate − scheduled.Start) / scheduled.Duration` clamped to `[0,1]` and the actual percent as `(statusDate − actual.Start) / actual.Duration` (or `1.0` on a `Completed` status, `0.0` on an absent actual interval), and folds `BCWS += budget × planned`, `BCWP += budget × actual`, and `ACWP +=` the line's recorded incurred cost (`actuals.Find(item.GlobalId)`) when present else the schedule-duration proxy `budget × actual × cpiDeviation` (the `cpiDeviation` actual-vs-scheduled duration ratio so an over-running task reads `ACWP > BCWP` in the no-actuals fallback) — each accumulator a `Money` summed through the additive operator; the report derives `CPI = ACWP.Amount == 0 ? 1 : BCWP/ACWP`, `SPI = BCWS.Amount == 0 ? 1 : BCWP/BCWS` (the `Money / Money → decimal` ratio), `EAC = CPI == 0 ? BAC : BAC/CPI` (the `Money / decimal → Money` divide), `VAC = BAC − EAC` — one `Fold` over the `(line, task)` join, never enumerated per-line arms; `ChangeOrder.Apply` folds the delta set onto the baseline line map keyed by `CostItem.GlobalId` (an added line inserts, a modified line supersedes, a removed line drops) so the revised schedule re-rolls through the existing `Rollup` fold.
- Receipt: the `EarnedValueReport` is the typed 5D cost-performance evidence the `csharp:Rasm.AppUi/Charts` `EarnedValue/ChangeOrder` report renders — `CPI < 1` reads over-budget (a true cost index once `actuals` are supplied, a schedule-overrun forecast otherwise), `SPI < 1` reads behind-schedule, `EAC > BAC` reads forecast-overrun, the `VAC` the cost variance at completion as `Money`; the `ChangeOrder` revision audit reads the baseline-to-revised line delta and the `ChangeOrderStatus` approval state, and the `Contingency` drawdown reads the remaining `Money` reserve — each carried on the one tracked `CostSchedule`, never a second cost-performance store.
- Packages: GeometryGymIFC_Core, NodaMoney, Thinktecture.Runtime.Extensions, NodaTime, System.IO.Hashing, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new earned-value metric (a to-complete-performance-index, a cost variance `CV = BCWP − ACWP` as `Money`) is one derived scalar on `EarnedValueReport` over the same BCWS/BCWP/ACWP fold; a new change-order status is one `ChangeOrderStatus` row; a new contingency-allocation rule rides the `CostSchedule.Apportion` `Split`; a new progress source rides the existing `ConstructionTask` actual-interval join and a recorded actual-cost feed rides the existing `actuals` map keyed by `CostItem.GlobalId`; never a per-metric report record, never a parallel revision or contingency store, never a re-derived progress source, never a second actual-cost store, and never a `(double, string)` metric carrier.
- Boundary: the earned-value join reads the `Planning/schedule#SCHEDULE` `ConstructionTask` actual/scheduled `Interval` progress by `GlobalId` and re-deriving progress in this owner is the named seam violation — the schedule owns the activity network and its actual interval, the cost owner reads the percent-complete it implies; the metrics are `Money` (BCWS/BCWP/ACWP/EAC/VAC) and the dimensionless `decimal` `Money / Money` ratio (CPI/SPI) and a `(double Bac, …, string Currency)` carrier is the deleted form mirroring the no-`(double, string)`-money law of `[2]-[ESTIMATE]`; the `ChangeOrder` delta is a priced revision against a baseline `CostSchedule` reusing the existing `CostItem`/`CostValue`/`Money` algebra and a parallel `CostRevision` class family or a second revision store is the deleted form; the `Contingency` is a `CostCategory.Contingency` `Money` reserve on the one `CostSchedule`, never a parallel reserve store; the `EarnedValueReport` is the typed receipt and a generic `IReceipt`/ledger is the named defect per the typed-receipt law; the fold joins the cost line to the schedule task by the `TaskAssignment` `GlobalId` membership and a parallel cost-side schedule is the named seam violation; the fold is TOTAL and a vestigial `Fin` rail claiming an abort the code never performs is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using NodaMoney;
using NodaTime;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ChangeOrderStatus {
    public static readonly ChangeOrderStatus Proposed = new("PROPOSED");
    public static readonly ChangeOrderStatus Submitted = new("SUBMITTED");
    public static readonly ChangeOrderStatus Approved  = new("APPROVED");
    public static readonly ChangeOrderStatus Rejected  = new("REJECTED");
    public static readonly ChangeOrderStatus Void      = new("VOID");

    public static ChangeOrderStatus Of(string status) => TryGet(status.Trim().ToUpperInvariant()).IfNone(Proposed);
}

// --- [MODELS] -----------------------------------------------------------------------------
// The contingency reserve is a Money over the schedule's reporting currency; a drawdown is the native Money
// subtraction floored at the additive identity (an over-draw clamps to zero through the IsNegative guard rather
// than going negative), never a hand-clamped `double` over a bare currency string.
public sealed record Contingency(double Percentage, Money Reserve) {
    public static readonly Contingency None = new(0d, Money.AdditiveIdentity);
    public Contingency Drawdown(Money draw) =>
        this with { Reserve = Reserve - draw is var net && Money.IsNegative(net) ? new Money(0m, Reserve.Currency) : net };
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

// The 5D cost-performance receipt: the monetary axes are Money (the currency rides each value, never a bare
// string field), CPI/SPI the dimensionless Money / Money -> decimal ratio. Of derives the indices guarding the
// zero divisors so an empty schedule reads CPI=SPI=1 and EAC=BAC rather than a divide-by-zero.
public readonly record struct EarnedValueReport(
    Money Bac, Money Bcws, Money Bcwp, Money Acwp,
    decimal Cpi, decimal Spi, Money Eac, Money Vac) {
    public bool OverBudget => Cpi < 1m;
    public bool BehindSchedule => Spi < 1m;

    public static EarnedValueReport Of(Money bac, Money bcws, Money bcwp, Money acwp) {
        decimal cpi = acwp.Amount == 0m ? 1m : bcwp / acwp;
        decimal spi = bcws.Amount == 0m ? 1m : bcwp / bcws;
        Money eac = cpi == 0m ? bac : bac / cpi;
        return new(bac, bcws, bcwp, acwp, cpi, spi, eac, bac - eac);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CostPerformance {
    // ACWP (actual cost of work performed) is an EXTERNAL input — the incurred-cost-to-date per CostItem.GlobalId an
    // accounting/Persistence feed supplies through `actuals` (the EVM cost axis, independent of schedule progress).
    // When a line carries no recorded actual the fold degrades to the schedule-duration proxy (the earned value
    // scaled by the actual-vs-scheduled overrun ratio) rather than fabricating a cost index: with actuals present
    // CPI = BCWP/ACWP is a TRUE cost index distinct from SPI; without them the report is a schedule-derived forecast.
    public static EarnedValueReport EarnedValue(this CostSchedule schedule, ScheduleNetwork network, Instant statusDate, Map<string, Money> actuals = default) {
        var taskByElement = network.Assignments
            .Bind(a => a.ElementGlobalIds.Map(id => (Element: id, a.TaskGlobalId)))
            .Fold(Map<string, string>(), static (m, row) => m.AddOrUpdate(row.Element, row.TaskGlobalId));
        var taskById = network.Tasks.Fold(Map<string, ConstructionTask>(), static (m, t) => m.AddOrUpdate(t.GlobalId, t));
        var (bac, bcws, bcwp, acwp) = schedule.Items
            .Map(item => Line(item, taskByElement, taskById, actuals, statusDate))
            .Fold(
                (Bac: Money.AdditiveIdentity, Bcws: Money.AdditiveIdentity, Bcwp: Money.AdditiveIdentity, Acwp: Money.AdditiveIdentity),
                static (acc, l) => (acc.Bac + l.Budget, acc.Bcws + l.Bcws, acc.Bcwp + l.Bcwp, acc.Acwp + l.Acwp));
        return EarnedValueReport.Of(bac, bcws, bcwp, acwp);
    }

    static (Money Budget, Money Bcws, Money Bcwp, Money Acwp) Line(
        CostItem item, Map<string, string> taskByElement, Map<string, ConstructionTask> taskById, Map<string, Money> actuals, Instant statusDate) {
        Money budget = item.ValueOf();
        Money recorded = actuals.Find(item.GlobalId).IfNone(Money.AdditiveIdentity);
        bool hasActual = actuals.ContainsKey(item.GlobalId);
        return item.PricedGlobalIds.HeadOrNone()
            .Bind(taskByElement.Find)
            .Bind(taskById.Find)
            .Match(
                Some: task => {
                    decimal planned = (decimal)Fraction(task.Scheduled, statusDate);
                    decimal actual = task.Status == TaskStatus.Completed ? 1m : (decimal)task.Actual.Map(a => Fraction(a, statusDate)).IfNone(0d);
                    decimal cpiDeviation = (decimal)task.Actual.Map(a => a.Duration.TotalDays / Math.Max(task.Scheduled.Duration.TotalDays, double.Epsilon)).IfNone(1d);
                    // ACWP from the recorded incurred cost when present; else the schedule-duration proxy the no-actuals path degrades to.
                    Money acwp = hasActual ? recorded : budget * (actual * cpiDeviation);
                    return (budget, budget * planned, budget * actual, acwp);
                },
                // A line with no assigning task still contributes its recorded spend to ACWP (and its budget to BAC) — cost incurred without earned value.
                None: () => (budget, Money.AdditiveIdentity, Money.AdditiveIdentity, recorded));
    }

    static double Fraction(Interval interval, Instant statusDate) =>
        interval.Duration.TotalDays <= 0d ? (statusDate >= interval.End ? 1d : 0d)
        : Math.Clamp((statusDate - interval.Start).TotalDays / interval.Duration.TotalDays, 0d, 1d);
}
```

## [04]-[RESEARCH]

- [COST_SCHEDULE_DISPATCH]: the `IfcCostSchedule` container traversal — the `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcCostItem` set the `ItemsOf` fold materializes the priced lines through, each item's own `Controls` `IfcRelAssignsToControl` priced-element set, each item's `CostValues` `IfcCostValue` applied-rate set and `CostQuantities` `IfcPhysicalQuantity` explicit-quantity set, and the `Nests` `IfcRelNests.RelatingObject` parent the bill-of-quantities tree declares — grounds against the live GeometryGym `25.7.30` decompile (`IfcCostItem : IfcControl` exposes `CostValues` `LIST<IfcCostValue>`, `CostQuantities` `LIST<IfcPhysicalQuantity>`, `PredefinedType` `IfcCostItemTypeEnum`; `IfcControl.Controls` `SET<IfcRelAssignsToControl>`; `IfcObjectDefinition.Nests`/`IsNestedBy`/`HasAssociations`/`HasAssignments`) so the `CostItemOf`/`ValueOf`/`QuantityOf` projections discriminate the real cost graph; the `IfcCostSchedule.Controls`/`PredefinedType`, `IfcCostItem.CostValues`/`CostQuantities`/`Controls`/`Nests`, and `IfcRelAssignsToControl.RelatingControl`/`RelatedObjects` member spellings confirm against the `.api/api-geometrygym-ifc` scheduling-cost-resource family (rows 8-16) — the `IfcCostSchedule.Controls`/`IfcCostItem.Controls` `IfcRelAssignsToControl` cost-control path distinct from the `IfcRelAssignsToProcess` resource-to-activity path the resource fold reads; the `IfcCostScheduleTypeEnum` members verified against the decompile are `NOTDEFINED`/`USERDEFINED`/`BUDGET`/`COSTPLAN`/`ESTIMATE`/`TENDER`/`PRICEDBILLOFQUANTITIES`/`UNPRICEDBILLOFQUANTITIES`/`SCHEDULEOFRATES` — the migration source's `RUNNING` row was a PHANTOM (no such enum member) and is removed, the missing `UNPRICEDBILLOFQUANTITIES` added.
- [COST_VALUE_MONETARY]: the `IfcCostValue` applied-rate members the `ValueOf`/`AmountOf` folds read are verified against the live decompile — `IfcCostValue : IfcAppliedValue` inherits `AppliedValue` (`IfcAppliedValueSelect`), `UnitBasis` (`IfcMeasureWithUnit`), `Category` (`String`), `Components` (`LIST<IfcAppliedValue>`), and `ArithmeticOperator` (`IfcArithmeticOperatorEnum` `NONE`/`ADD`/`SUBTRACT`/`MULTIPLY`/`DIVIDE`); the `IfcMonetaryMeasure` direct-amount leg exposes `.Measure` (a `Double`, `IfcMonetaryMeasure : IfcDerivedMeasureValue`) and the `IfcMeasureWithUnit.UnitComponent` (`is IfcMonetaryUnit` whose `.Currency` is the ISO 4217 code) lift TOGETHER into one `NodaMoney` `Money` through the railed `CostMoney.Of((decimal)measure, currency, key)`, the `new Money(decimal, string)` ctor resolving the embedded ISO 4217 `CurrencyRegistry` and the thrown `InvalidCurrencyException` trapping onto `BimFault.CodecReject` lifted BARE — the COMPOSITE leg (`AppliedValue` null, `Components` non-empty) folding the sub-value tree recursively through the `ArithmeticOperator` so a `material + labor + equipment` rate resolves rather than reading only the head; verified: `Money` is a `readonly struct` implementing `IAdditiveIdentity<Money,Money>`/`IMultiplicativeIdentity<Money,decimal>` with the full generic-math operator set (`Money + Money`, `Money * decimal → Money`, `Money / decimal → Money`, `Money / Money → decimal`) native and `Money.AdditiveIdentity` `= new Money(0m, Currency.NoCurrency)`, `MoneyExtensions.Split(int[] ratios)` returning `IEnumerable<Money>` the lossless allocation, the `2.7.0` `lib/net10.0` asset binding (`api-nodamoney`); the `IfcMeasureWithUnit.ValueComponent` (`is IfcMeasureValue` whose `.Measure` is the per-basis denominator) lifts onto the `decimal UnitBasis` so the line rate is the native `Applied / UnitBasis` divide.
- [RESOURCE_MODALITY]: the construction-resource modalities the ONE `ConstructionResource` record discriminates over the `ResourceKind` `[SmartEnum]` are verified against the decompile — `IfcConstructionResource : IfcResource` (abstract) exposes `Usage` (`IfcResourceTime`), `BaseCosts` (`LIST<IfcAppliedValue>`), and `BaseQuantity` (`IfcPhysicalQuantity`), and its six concrete subtypes `IfcLaborResource`/`IfcConstructionMaterialResource`/`IfcConstructionEquipmentResource`/`IfcCrewResource`/`IfcConstructionProductResource`/`IfcSubContractResource` each carry a `PredefinedType` over their own `Ifc…ResourceTypeEnum` — so the migration source's 3-arm `Labor`/`Material`/`Equipment` `[Union]` was a NAIVE SLICE silently dropping `Crew`/`Product`/`Subcontract` through its `_ => None` arm; the collapse to ONE record keyed by `ResourceKind.Of(GetType().Name)` covers all six, lifts `BaseCosts` onto the optional `Money BaseCost` (the resource cost the by-resource rollup partition reads, `BaseCost * BaseQuantity`), reads `BaseQuantity` as a seam `MeasureValue` (the `IfcPhysicalSimpleQuantity.MeasureValue.Measure` SI magnitude, never a bare `double`), reads the `IfcLaborResource`/`IfcCrewResource` `PredefinedType` onto `Skill` (the IFC4.3 schema carrying no `SkillSet` member — the predefined token IS the skill descriptor), reads the `IfcConstructionMaterialResource`/`IfcConstructionProductResource` `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`is IfcMaterial`) `.Name` onto `Material`, and the inherited `HasAssignments` `IfcRelAssignsToProcess.RelatingProcess` `IfcProcess.GlobalId` onto `TaskGlobalId` so the resource binds the `Planning/schedule#SCHEDULE` `ConstructionTask` it resources.
- [SEAM_TAKEOFF_JOIN]: the cost-to-element join reads the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` rather than the retired `BimModel` — the `CostProjection.Project` builds an `ExternalId → NodeId` index over `graph.ObjectNodes` once (the `Node.Object.ExternalId` is the 1:1 IFC `GlobalId` projection attribute [H6] the `Projection/semantic#SEMANTIC_PROJECTOR` records), faults `BimFault.DanglingReference` on a priced `GlobalId` no `Node.Object` declares, and resolves the line takeoff `MeasureValue` from the explicit `IfcCostItem.CostQuantities` or the dominant base quantity off each priced element's `graph.Bake(node, key)` `Element.Quantities` bag (the seam `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` the `Semantics/properties#BASE_QUANTITIES` `QuantityDerivation` derived from the kernel geometry the node references by content key) by the `Dimension` rank `Volume ≻ Area ≻ Length ≻ Mass ≻ Duration` summed through `MeasureValue.Sum` — the retired `QuantityKind` six-case enum replaced by the seam `Dimension` `[ComplexValueObject]` so a `ThermalTransmittance`/`Pressure` measure admits; the takeoff resolves ONCE at projection so the `CostItem` is self-contained (`ValueOf()` pure, `Rollup`/`EarnedValue` need no graph) and the `(QuantityKey, ResourceKey)` `UInt128` identity hashes the resolved `(globalIds, quantity.Si, rate)` triples through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom so the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog re-reads only a changed estimate at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`, and the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified set — re-pricing or minting a second takeoff/schedule/identity source downstream is the named cross-folder drift defect.
- [FAULT_BARE_LIFT]: every cost rejection lifts the typed `Model/faults#FAULT_BAND` `BimFault` case BARE onto the `Fin<T>` rail — the rebuilt band is `Expected`-derived (`IValidationError<BimFault>`) so band 2600 IS the `Expected` `Code` and `Fin.Fail<CostItem>(new BimFault.DanglingReference(key, $"cost-priced-miss:{id}"))` / `Try.lift(() => new Money(amount, code)).Run().MapFail(error => new BimFault.CodecReject(key, $"cost-currency:{error.Message}"))` both carry the case directly, the `.ToError()` lowering hop the migration source threaded through every construction being the named defect the band closes (a `.ToError()` erases the typed arm the `IfcLegality` `Validation` accumulation and the `error.IsType<BimFault.DanglingReference>()` recovery depend on); each case carries the kernel `Op key` operation context, so a cost-currency reject and a priced-miss compose on the one rail with the schedule/quantity faults without a second fault family, and the `MoneyContext` `CostMoney.Context` banker's-rounding policy the composition root installs governs every `Money` operator rather than a `MidpointRounding` argument threaded through each construction.
