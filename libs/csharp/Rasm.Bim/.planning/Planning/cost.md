# [BIM_COST_ESTIMATE]

The host-neutral 5D cost-and-resource projection over the `Rasm.Element` seam graph: one `CostItem` line joining an `IfcCostValue` applied rate to the takeoff `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` the line resolves at projection from the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Element.Quantities` bag (or the cost line's own `IfcCostItem.CostQuantities`), one `ConstructionResource` record discriminated by the `ResourceKind` `[SmartEnum<string>]` over all six `IfcConstructionResource` modalities, and the `CostSchedule.Rollup` `Money`-fold that folds the resolved `(quantity x rate)` lines into one schedule total — one fold, never enumerated per-resource arms. The priced scalar is the `NodaMoney` `Money` `readonly struct` over a `decimal Amount` and an ISO 4217 `Currency`, never a `(double Amount, string Currency)` pair anywhere on the page: a `CostItem` value is `Money`, a unit rate is `Money / decimal`, the quantity x rate join is `Money * (decimal)quantity.Si`, a composite IFC `IfcCostValue.Components` rate folds through the `IfcArithmeticOperatorEnum` operator into one `Money` (a non-monetary `IfcRatioMeasure` component lifting as the currencyless scalar factor), the `CostSchedule.Rollup` is a `Money` sum over the additive operator (`Money.AdditiveIdentity` the no-currency anchor) carrying the lossless per-currency `ByCurrency` subtotal partition BEFORE any convert — a genuinely mixed-currency estimate aggregates without a complete FX table — a cross-currency total composes `NodaMoney.Exchange.ExchangeRate.Convert` over the per-line-matched `Seq<ExchangeRate>` fx table (both rate legs matched, never a single-rate assumption), a lump-sum or contingency allocation across packages is the lossless `MoneyExtensions.Split` penny distribution rather than a remainder-losing `total / n` multiply, and the EARNED-VALUE metrics (BCWS/BCWP/ACWP/EAC/VAC plus the derived `CV`/`SV`/`TCPI`) are `Money` with `CPI`/`SPI`/`TCPI` the dimensionless `Money / Money → decimal` ratio and the sign reads the native `Money.IsNegative` predicates — never a `(double, string Currency)` carrier. The whole estimate folds under one `NodaMoney.Context.MoneyContext` rounding/precision policy (the `CostMoney.Context` banker's-rounding scope the composition root installs) so a single rounding rule governs every line rather than a `MidpointRounding` argument threaded through every construction.

The estimate is a VIEW of the federated `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, never a second quantity or schedule source: each `CostItem` reads its takeoff from the one seam `QuantitySet` bag the `Semantics/properties#BASE_QUANTITIES` `QuantityDerivation` already derived from the kernel geometry the `Node.Object` references by content key, joins to the priced elements through the `IfcRelAssignsToControl.RelatedObjects` cost-control set resolved against the seam graph by the `Node.Object.ExternalId` (the 1:1 IFC `GlobalId` projection attribute [H6]), and binds the resource it consumes to the `Planning/schedule#SCHEDULE` `ConstructionTask` activity network through `IfcRelAssignsToProcess` rather than re-modeling a parallel schedule — so a wall's net-volume takeoff, its concrete unit rate, and the labor crew that places it carry one cost line while the seam keeps the element's content-keyed geometry and the schedule keeps the activity's calendar. The estimate is HOST-NEUTRAL — it reads the in-process GeometryGym cost graph and joins to the seam by stable `GlobalId`/`NodeId`, never a RhinoCommon type — and the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, never a second selection surface. The `(QuantityKey, ResourceKey)` `UInt128` content-key pair `CostSchedule.Identity` derives over the resolved `(globalIds, quantity, rate)` line triples is the reference the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads the schedule by at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`, and the `csharp:Rasm.AppUi/Charts` estimate report renders the same rollup — Bim PRODUCES the cost source, never re-pricing it downstream. A foreign IFC amount enters through the railed `CostMoney.Of((decimal)amount, iso4217, key)` boundary trapping the `InvalidCurrencyException` onto `Model/faults#FAULT_BAND` `BimFault.CodecReject`, a bare `IfcMonetaryMeasure` carrying no per-value `IfcMonetaryUnit` prices in the PROJECT currency (the `IfcContext.UnitsInContext` `IfcMonetaryUnit` resolved once at projection onto `CostSchedule.Currency` — the IFC law for unqualified monetary measures, previously silently degraded to `Currency.NoCurrency`), and every cost rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail (band 2600 IS the `Expected` `Code`), never a `.ToError()` hop, a new fault family, or a thrown currency exception in domain logic.

## [01]-[INDEX]

- [01]-[ESTIMATE]: `CostItem` line (the summed `Values` rate set x ONE resolved `MeasureValue` takeoff by `GlobalId`), `ConstructionResource` the ONE record discriminated by the `ResourceKind` `[SmartEnum<string>]` over the six `IfcConstructionResource` modalities, the `CostValue` rate carrier over the `NodaMoney` `Money`/`Currency`/`ExchangeRate` algebra with the `IfcCostValue.Components` aggregation fold, the `CostScheduleKind`/`ResourceKind`/`CostCategory` `[SmartEnum]` partitions, the `CostSchedule` record with its resolved reporting `Currency` + `Status`, its RAILED `Fin<CostRollup>` `Money`-fold `Rollup` (per-currency `ByCurrency` losslessly, the fx-table repriced `Total`), lossless `Apportion` `Split`, and CANONICALLY-ORDERED `(QuantityKey, ResourceKey)` identity, the `CostMoney` boundary lift + `Reprice` fx-match + `MoneyContext` policy, and the `CostProjection.Project` fold from the GeometryGym `IfcCostSchedule` surface over the seam graph.
- [02]-[EARNED_VALUE]: the `ChangeOrder` priced-revision record over a baseline `CostSchedule`, the `Contingency` `Money` reserve column, and the currency-railed `CostSchedule.EarnedValue` fold (BCWS/BCWP/ACWP as `Money`, CPI/SPI/TCPI as `decimal`, EAC/VAC/CV/SV as `Money`) joining the `Planning/schedule#SCHEDULE` `ConstructionTask` authored progress to the self-contained priced lines.

## [02]-[ESTIMATE]

- Owner: `CostSchedule` the single host-neutral 5D cost-and-resource record carrying the `CostScheduleKind` discriminant, the resolved reporting `Currency` (the project `IfcMonetaryUnit`, else the first priced value's — resolved ONCE at projection, never a head-of-items implicit per fold), the optional `Status` approval state (the GG `Staus` member), the self-contained priced `CostItem` line set (each line carrying its resolved takeoff `MeasureValue`), the `ConstructionResource` resource set, the `Contingency` reserve, and the `(QuantityKey, ResourceKey)` content-key identity the cross-libs cost catalog reads it by, with the railed `Fin<CostRollup>` `Money`-fold `Rollup` schedule-total and the lossless `Apportion` lump-sum distribution; `CostItem` the single priced line record joining the `IfcCostItem.CostValues` `Seq<CostValue>` applied-rate SET (bSI sums an item's cost values — a head-only read is the deleted form; same-currency within one line admitted at projection) to the ONE resolved `MeasureValue Quantity` (the seam takeoff or the explicit `IfcCostItem.CostQuantities`) by the priced element `GlobalId` set, carrying the optional `ResourceGlobalId` it consumes (the `IfcConstructionResource` the item's own `Controls` set assigns, routed OFF the priced set so a resource-controlling line never false-faults the element join) and the `ParentGlobalId` nesting reference the `IfcCostItem` `Nests` hierarchy declares — its `ValueOf` the pure `Σ Rate * (decimal)Quantity.Si` fold needing no graph; `ConstructionResource` the ONE record discriminated by the `ResourceKind` `[SmartEnum<string>]` over the six `IfcConstructionResource` modalities (`Labor`/`Material`/`Equipment`/`Crew`/`Product`/`Subcontract`), carrying its `MeasureValue BaseQuantity`, an optional `Money BaseCost` from `IfcConstructionResource.BaseCosts`, the optional `Skill` (the `IfcLaborResourceTypeEnum`/`IfcCrewResourceTypeEnum` `PredefinedType` descriptor), the optional consumed `Material` (read through the resource's `HasAssociations` `IfcRelAssociatesMaterial`), the optional `TaskGlobalId` it resources through `IfcRelAssignsToProcess`, and the optional `Completion` ratio (`IfcResourceTime.Completion` off the inherited `Usage` — the resource-side actual-progress axis feeding the `Spent` incurred read) — never a per-subtype class family; `CostValue` the applied-rate record carrying its `Money` value (the fold of the `IfcCostValue` direct amount OR its `Components` tree through the `ArithmeticOperator`), its `UnitBasis` per-unit `decimal` denominator, and its `CostCategory` discriminant, the per-basis rate the native `Money / decimal` divide; `CostScheduleKind`/`ResourceKind`/`CostCategory` the cost-schedule-kind / resource-modality / cost-category `[SmartEnum<string>]` vocabularies; `CostMoney` the boundary lift folding a foreign IFC `(double amount, string iso4217)` into a typed `Money` trapping the `InvalidCurrencyException` onto `BimFault.CodecReject`, the ONE `Reprice` fx-table repricing owner (both rate legs matched) `Rollup` and `EarnedValue` compose, plus the `MoneyContext` rounding policy; `CostProjection` the static fold over the GeometryGym `IfcCostSchedule` surface and the seam `ElementGraph`.
- Cases: `ConstructionResource` is ONE record whose `ResourceKind` row discriminates the modality — `Labor` (`IfcLaborResource`, `Skill` = `IfcLaborResourceTypeEnum` `PredefinedType`, `BaseQuantity` crew man-hours), `Material` (`IfcConstructionMaterialResource`, `Material` = the consumed material name through `HasAssociations`, `BaseQuantity` consumed volume/mass), `Equipment` (`IfcConstructionEquipmentResource`, `BaseQuantity` plant-hours), `Crew` (`IfcCrewResource`, `Skill` = `IfcCrewResourceTypeEnum`), `Product` (`IfcConstructionProductResource`, `Material` = the product), `Subcontract` (`IfcSubContractResource`) — each carrying the optional `BaseCost` and `TaskGlobalId`, a seventh modality being one `ResourceKind` row with zero new surface (6 modalities, the `[SmartEnum]` partition the discriminant); the `CostValue` value object carries its `Money` `Applied` rate (the `IfcCostValue.AppliedValue` `IfcMonetaryMeasure.Measure` lifted with the `IfcMonetaryUnit.Currency` ISO 4217 code TOGETHER into one `Money`, or the `IfcAppliedValue.Components` sub-value tree folded through the `IfcArithmeticOperatorEnum` — never a `double` amount beside a bare currency string), its `UnitBasis` per-unit `decimal` denominator (`1m` for a unit rate, the `IfcCostValue.UnitBasis` `IfcMeasureValue` magnitude for a per-basis rate), and its `CostCategory` (`Material`/`Labour`/`Equipment`/`Overhead`/`Subcontract`/`Preliminaries`/`Contingency`/`NotDefined` over the `IfcCostValue.Category` string) — the line rate the native `Applied / UnitBasis` (`Money / decimal → Money`) before multiplying the takeoff quantity; the `CostScheduleKind` rows `Budget`/`CostPlan`/`Estimate`/`Tender`/`PricedBillOfQuantities`/`UnpricedBillOfQuantities`/`ScheduleOfRates`/`UserDefined`/`NotDefined` (9) each frozen over the verified `IfcCostScheduleTypeEnum` member, the `ResourceKind` rows `Labor`/`Material`/`Equipment`/`Crew`/`Product`/`Subcontract`/`NotDefined` (7 — a `UserDefined` row is unreachable dead data because `Of(GetType().Name)` only ever yields the six subtype keys, `NotDefined` the sole `IfNone` fallback) each frozen with its `IfcDomain`, and the `CostCategory` rows (8) over the IFC cost category string.
- Entry: `CostProjection.Project(IfcCostSchedule schedule, Seq<IfcConstructionResource> resources, ElementGraph graph, Op key)` folds one GeometryGym cost schedule into one self-contained `CostSchedule` — materializing the schedule's `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcCostItem` set PLUS each item's transitive `IsNestedBy` sub-item tree once (the nested BoQ child lines a `Controls`-only read drops, deduped by `GlobalId`), building the `ExternalId → NodeId` index over `graph.ObjectNodes` once, resolving the project reporting `Currency` once (`MonetaryOf` over `IfcContext.UnitsInContext`, railed through `CostMoney.Of`), folding each cost item onto a `CostItem` line that resolves its WHOLE `CostValues` applied-rate set (the `Components` tree included; mixed real currencies within one item railing `cost-value-currency-mixed` at admission so `ValueOf` stays total), resolves the priced element `GlobalId` set against the index (`BimFault.DanglingReference` BARE on a priced `GlobalId` the seam graph never declares as a `Node.Object.ExternalId`), and resolves the line takeoff `MeasureValue` (the explicit `IfcCostItem.CostQuantities` when present, else the dominant base quantity off the priced elements' seam `Bake`d `Element.Quantities`, else a unit lump-sum), and folding the resource set onto `ConstructionResource` rows discriminated by `ResourceKind.Of(resource.GetType().Name)` (a non-construction-resource entity filtered before the traverse rather than aborting the whole schedule; a resource `BaseCosts` currency fault railing `BimFault.CodecReject` typed rather than silently dropping the cost); `CostProjection.ProjectAll` lifts every cost schedule in a federated graph onto the `Seq<CostSchedule>` the catalog reads, `CostSchedule.Rollup(Op key, Seq<ExchangeRate> fx = default)` folds the resolved per-value lines into the `Fin<CostRollup>` schedule total — the `ByCurrency` partition aggregating each value's NATIVE currency losslessly before any convert, the `Total` repricing through the `CostMoney.Reprice` fx-table match (railing `Model/faults#FAULT_BAND` `BimFault.CodecReject` on a foreign value carrying no matching rate, never a thrown `Money + Money` mismatch in domain logic) — and `CostSchedule.Apportion(Money lumpSum)` distributes a lump-sum (overhead, contingency) across the lines by their value-weight ratios through the lossless `MoneyExtensions.Split` (a zero-weight line set splits evenly — the lump is never dropped).
- Auto: `Project` reads the `IfcCostSchedule` runtime graph and folds it into the self-contained schedule — the `ItemsOf` projection materializes the schedule's `Controls` `IfcRelAssignsToControl` controlled `IfcCostItem` set plus each item's transitive `IsNestedBy` sub-item closure once (the `NestedItems` recursion mirroring the schedule WBS flatten, deduped by `GlobalId`; a parent line prices only its OWN authored values, a leaf-authored BoQ summing exactly); `ValuesOf` folds each item's WHOLE `CostValues` LIST onto the `Seq<CostValue>` line set (`AmountOf` reading the `IfcMonetaryMeasure.Measure` and the `UnitBasis.UnitComponent` `IfcMonetaryUnit.Currency` ISO 4217 code TOGETHER into one `Money` through `CostMoney.Of` — a bare monetary measure with no per-value unit pricing in the resolved MODEL currency — OR lifting a non-monetary `IfcMeasureValue` leaf (an `IfcRatioMeasure` overhead factor) as the currencyless scalar, OR recursively folding the `IfcAppliedValue.Components` sub-value tree through the `IfcArithmeticOperatorEnum` `ADD`/`SUBTRACT`/`MULTIPLY`/`DIVIDE` operator, the `IfcCostValue.UnitBasis.ValueComponent` `IfcMeasureValue.Measure` onto the `decimal UnitBasis`, the `IfcCostValue.Category` onto `CostCategory`); `QuantityOf` resolves the line takeoff once at projection (the explicit `IfcCostItem.CostQuantities` `IfcPhysicalSimpleQuantity` set decoded through the owned `Projection/semantic#VALUE_NARROWING` `PropertyLowering.Measure`, else `graph.Bake(node, key)` over each priced element and the dominant `MeasureValue` by `Dimension` rank `Volume ≻ Area ≻ Length ≻ Mass ≻ Duration` summed through `MeasureValue.Sum`, else the `Dimensionless` unit lump-sum), so the line value is the pure `Σ CostValue.Rate * (decimal)Quantity.Si` fold and the schedule needs no graph after projection; `CostItemOf` splits the item's `Controls` related set — the `IfcConstructionResource` head onto `ResourceGlobalId`, the `IfcProduct`s onto the priced set (a related `IfcProcess`/`IfcActor` is neither: the line prices as a unit lump-sum with no element join, never a false `DanglingReference` abort) — and threads the `IfcCostItem` `Nests.RelatingObject.GlobalId` parent onto `ParentGlobalId` so a bill-of-quantities tree folds onto the flat line set with its nesting preserved; `ResourceOf` is ONE fold reading each `IfcConstructionResource` by runtime subtype onto a `ConstructionResource` row — `ResourceKind.Of(GetType().Name)` the discriminant, `MeasureOf(resource.BaseQuantity)` the `MeasureValue BaseQuantity`, the first `IfcConstructionResource.BaseCosts` `IfcAppliedValue` onto the optional `Money BaseCost`, the `IfcLaborResource`/`IfcCrewResource` `PredefinedType` onto `Skill`, the `IfcConstructionMaterialResource`/`IfcConstructionProductResource` associated `IfcMaterial.Name` onto `Material`, and the `OperatesOn`/`HasAssignments` `IfcRelAssignsToProcess.RelatingProcess` `IfcProcess.GlobalId` onto `TaskGlobalId` so the resource binds the `Planning/schedule#SCHEDULE` `ConstructionTask` it resources; `CostSchedule.Rollup` flattens the lines to per-value `(category, native)` pairs, partitions the NATIVE amounts by `Currency.Code` losslessly (`ByCurrency` — the mixed-currency aggregation no FX table gates), reprices each through `CostMoney.Reprice` into the schedule `Currency` (`ExchangeRate.Convert` on the fx-table rate matched on BOTH legs, railing `BimFault.CodecReject` when no matching rate exists rather than throwing a `Money + Money` mismatch) then sums them into the `Fin<CostRollup>` schedule total through the additive operator, partitions the total by `CostCategory` per VALUE and the resource cost (`BaseCost * BaseQuantity`, repriced through the SAME `CostMoney.Reprice` owner so a foreign-currency resource rails typed rather than throwing inside the partition) by `ResourceKind` through one `Fold`; `CostSchedule.Identity` derives the `(QuantityKey, ResourceKey)` `UInt128` pair through `XxHash128.HashToUInt128` over the CANONICALLY-ORDERED priced line `(globalIds, quantity.Si, values)` triples and resource `(GlobalId, kind, baseQuantity.Si)` rows (the line set by `GlobalId`, each priced-id and resource set sorted ordinally, the per-line value list LIST-ordered — STEP LISTs are stable across re-parse) so the key is invariant to the unstable `IfcSet` iteration order a re-parse yields and the catalog re-reads only a genuinely changed estimate.
- Receipt: the `Seq<CostSchedule>` is the cost evidence the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads by the `(QuantityKey, ResourceKey)` reference at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` and the `csharp:Rasm.AppUi/Charts` estimate report renders, the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, and the `CostRollup` schedule total with its `ByCurrency` native-amount partition (the lossless mixed-currency read no FX table gates) and its `CostCategory`/`ResourceKind` partitions is the 5D estimate evidence; the priced line carries its resolved takeoff, applied rate, and resourced task on one self-contained record joining the seam quantity and the schedule activity by reference, never a second quantity or schedule store.
- Packages: GeometryGymIFC_Core, NodaMoney, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new cost-schedule kind is one `CostScheduleKind` row reading the next `IfcCostScheduleTypeEnum` member; a new construction-resource modality is one `ResourceKind` row with zero new surface (the ONE `ConstructionResource` record absorbs it on the discriminant — never a new union arm or class); a new cost category is one `CostCategory` row; a composite rate rides the existing `IfcAppliedValue.Components` fold and a second cost value on a line is one `CostValues` LIST entry riding the existing `ValuesOf` traverse; a new per-line binding is one column on `CostItem`; a new convertible currency pair is one `ExchangeRate` row in the `Rollup`/`EarnedValue` fx table and a currency needing no convert reads the lossless `ByCurrency` partition; a regional or custom currency is one `CurrencyRegistry.TryAdd(CurrencyInfo)` registration; a new rounding rule (cash-denomination, exact) is one `CostMoney.Context` `MoneyContext` policy (`CashDenominationRounding`/`NoRounding`), never a `MidpointRounding` argument; a new cost schedule rides the existing `ProjectAll` fold on one row; never a per-resource-type cost record, never a parallel `LaborCost`/`MaterialCost` class family, never a `GetLaborCost`/`GetByCategory` operation family, never a hand-rolled `MonetaryAmount` `(double, string)` carrier beside `Money`, and never a second takeoff or schedule source.
- Boundary: `CostSchedule` is ONE record discriminated by the `CostScheduleKind` row, and `ConstructionResource` is ONE record discriminated by the `ResourceKind` `[SmartEnum]` over all six IFC modalities — a `Labor`/`Material`/`Equipment` `[Union]` slicing only three of six subtypes, a `LaborResource`/`MaterialResource` class family, or three sibling factory methods is the deleted form (the collapse to one record keyed by the smart enum removes the repeated `Switch` accessors and covers every modality), mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`; the priced scalar is the `NodaMoney` `Money` `readonly struct` and a hand-rolled `MonetaryAmount` `(double, string)` record, a `double` cost-arithmetic helper, a naive `total / n` allocation where `MoneyExtensions.Split` is lossless, a stringly currency field validated by hand where the `Money(decimal, string)` ctor resolves the ISO 4217 registry, a thrown `InvalidCurrencyException` in domain code instead of the railed `CostMoney.Of`, and a second rounding policy threaded as a `MidpointRounding` argument where the one `CostMoney.Context` `MoneyContext` governs are the RETIRED forms; the estimate is a VIEW of the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` — the priced quantity is the seam `QuantitySet` `MeasureValue` takeoff joined by `Node.Object.ExternalId` (or the explicit `IfcCostItem.CostQuantities`), a re-derived parallel takeoff or a re-tessellation in this owner being the named seam violation, and the resourced schedule is the `Planning/schedule#SCHEDULE` `ConstructionTask` network joined by `TaskGlobalId`, a re-modeled cost-side schedule being the named seam violation; the retired `BimModel`/`BimElement` collection is GONE — a `federated.Elements` scan over a second stored element record is the deleted form, the cost reading the seam graph the `Bake` fold derives the consumer `Element` from; the GeometryGym `IfcCostSchedule`/`IfcCostItem`/`IfcCostValue`/`IfcAppliedValue`/`IfcConstructionResource` and its six subtypes / `IfcRelAssignsToControl`/`IfcRelAssignsToProcess`/`IfcRelAssociatesMaterial` surface (`.api/api-geometrygym-ifc` scheduling-cost-resource rows 8-16) is consumed as settled vocabulary and a hand-rolled cost reader is the deleted form, the `IfcPhysicalSimpleQuantity`->`MeasureValue` decode composing the owned `Projection/semantic#VALUE_NARROWING` `PropertyLowering.Measure` (one decode owner) rather than a duplicate dimension/value switch; the `NodaMoney` `Money`/`Currency` cross the `Exchange/wire` boundary through `MoneyJsonConverter`/`CurrencyJsonConverter` or the integer `ToMinorUnits` form and never leak past the cost owner; the quantified-element selection is the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` predicate and a parallel cost-element selection arm is the no-second-selection-surface reject; the `(QuantityKey, ResourceKey)` identity is derived through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom over canonically-ordered line/resource rows (invariant to the unstable `IfcSet` iteration order) and minting a second identity scheme for the catalog join is the named cross-folder drift defect; the `CostSchedule.Rollup` total is ONE RAILED `Money` fold over the resolved per-value lines AND the repriced resource costs (a foreign value or resource `BaseCost` lacking a matching `ExchangeRate` in the fx TABLE lifting `BimFault.CodecReject` BARE, never a thrown `Money + Money` mismatch in domain logic or inside a partition; a single-rate `Option<ExchangeRate>` parameter that cannot reprice a three-currency estimate is the deleted form, as is a base-leg-only rate match converting into a third currency, as is an unrepriced `ByResourceKind` partition whose accumulator can throw), never enumerated per-resource arms; the reporting currency is the resolved `CostSchedule.Currency` field and a head-of-items implicit derived per fold is the deleted form; a cost rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail and a `.ToError()` lowering hop or a bare `Fin.Fail` without the typed case is the named defect the rebuilt `Model/faults#FAULT_BAND` band closes.

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
    // NotDefined is the sole IfNone fallback: Of reads GetType().Name, which only ever yields the six subtype
    // keys — a "USERDEFINED" row is unreachable dead data on this discriminant.
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
// The NodaMoney boundary lift + the ambient rounding policy. A foreign IFC (double measure, ISO 4217 code) folds
// into the typed `Money` ONCE through the RAILED Of — the ctor resolves the embedded ISO 4217 registry, the thrown
// InvalidCurrencyException traps onto BimFault.CodecReject BARE; an empty code yields the Currency.NoCurrency
// additive identity. `Context` installs the ONE process-default policy via MoneyContext.CreateAndSetDefault
// (a non-installed Create leaves a policy swap inert): banker's rounding for every Money operator, and
// EnforceZeroCurrencyMatching=false so the NoCurrency identity seeds a currency-bearing sum. DefaultCurrency
// stays UNSET by design — the reporting currency is the per-schedule CostSchedule.Currency, never an ambient
// process value a multi-currency federation would cross-contaminate.
public static class CostMoney {
    public static readonly MoneyContext Context = MoneyContext.CreateAndSetDefault(static options => {
        options.RoundingStrategy = new StandardRounding();
        options.EnforceZeroCurrencyMatching = false;
    }, name: "rasm-cost");

    public static Fin<Money> Of(decimal amount, string iso4217, Op key) =>
        iso4217.Trim() is { Length: > 0 } code
            ? Try.lift<Money>(() => new Money(amount, code)).Run()
                .MapFail(error => new BimFault.CodecReject(key, $"cost-currency:{error.Message}"))
            : Fin.Succ(new Money(amount, Currency.NoCurrency));

    // The ONE repricing owner Rollup and EarnedValue compose: a value already in the report currency (or the
    // no-currency additive zero) passes; a foreign value converts through the fx-table rate matched on BOTH legs
    // (BaseCurrency = the value's, QuoteCurrency = the report's — a rate matched on base alone would convert into a
    // third currency the downstream `+` throws on); a foreign value with no matching rate rails CodecReject (shared
    // `cost-currency` detail family) — no thrown Money + Money mismatch escapes into domain logic.
    public static Fin<Money> Reprice(Money value, Currency report, Seq<ExchangeRate> fx, Op key) =>
        value.Currency == report || value.Currency == Currency.NoCurrency
            ? Fin.Succ(value)
            : fx.Find(rate => rate.BaseCurrency == value.Currency && rate.QuoteCurrency == report).Match(
                Some: rate => Fin.Succ(rate.Convert(value)),
                None: () => FinFail<Money>(new BimFault.CodecReject(key, $"cost-currency:unconvertible:{value.Currency.Code}>{report.Code}")));
}

// The applied rate is a `Money` over a `decimal` amount + a resolved `Currency` — never a `double` amount beside
// a bare currency string. The per-basis rate is the native `Money / decimal` divide (UnitBasis the IfcMeasureValue
// magnitude, 1m for a unit rate), so the line value arithmetic stays in the decimal-precision operator set.
public sealed record CostValue(Money Applied, decimal UnitBasis, CostCategory Category) {
    public Money Rate => UnitBasis == 0m ? Applied : Applied / UnitBasis;
}

// ONE construction-resource record discriminated by the ResourceKind row over the six IFC modalities — the
// kind-specific data (Skill for Labor/Crew, Material for Material/Product) rides Option fields, BaseQuantity is a
// seam MeasureValue (never a bare double), and BaseCost lifts IfcConstructionResource.BaseCosts so the resource
// contributes Cost = BaseCost x BaseQuantity to the by-resource partition. A per-subtype [Union]/class family
// slicing fewer than six modalities with re-projecting Switch accessors is the deleted form.
public sealed record ConstructionResource(
    string GlobalId,
    string Name,
    ResourceKind Kind,
    MeasureValue BaseQuantity,
    Option<Money> BaseCost,
    Option<string> Skill,
    Option<string> Material,
    Option<string> TaskGlobalId,
    Option<double> Completion) {
    public Money Cost => BaseCost.Map(c => c * (decimal)BaseQuantity.Si).IfNone(Money.AdditiveIdentity);

    // The resource's incurred spend at its authored IfcResourceTime.Completion ratio — the resource-side
    // actual the EVM actuals feed reads when no accounting figure exists; None-completion reads zero spend.
    public Money Spent => Completion.Map(c => Cost * (decimal)c).IfNone(Money.AdditiveIdentity);
}

// The priced line joins ONE resolved takeoff to the item's WHOLE IfcCostItem.CostValues set — bSI sums an item's
// cost values (a material value + a labour value on one line, each with its own category and unit basis), so a
// head-only read drops every value after the first. Values is LIST-ordered (the STEP LIST is stable across
// re-parse); projection admits the set only when all values share one currency, so ValueOf stays total.
public sealed record CostItem(
    string GlobalId,
    string Name,
    Seq<CostValue> Values,
    MeasureValue Quantity,
    Seq<string> PricedGlobalIds,
    Option<string> ResourceGlobalId,
    Option<string> ParentGlobalId) {
    // The line value folds every value's rate times the one takeoff — pure decimal-precision Money
    // (Money * decimal -> Money), no graph: the no-currency additive identity seeds the sum, an unpriced line
    // (empty Values) contributes zero, and the cross-multiply is never a `double`.
    public Money ValueOf() => Values.Fold(Money.AdditiveIdentity, (total, value) => total + value.Rate * (decimal)Quantity.Si);
}

public sealed record CostRollup(
    Money Total,
    Map<string, Money> ByCurrency,
    Map<string, Money> ByCategory,
    Map<string, Money> ByResourceKind);

// The reporting Currency is resolved ONCE at projection — the project-wide IfcMonetaryUnit (IfcUnitAssignment)
// when the model declares one, else the first priced value's currency, else NoCurrency — so Rollup and the EVM
// fold read one explicit field, never a head-of-items implicit. Status is the schedule approval state the GG
// `Staus` member carries (PLANNED/APPROVED/AGREED/ISSUED — free IFC text, not a bounded enum).
public sealed record CostSchedule(
    string GlobalId,
    CostScheduleKind Kind,
    string Name,
    Currency Currency,
    Option<string> Status,
    Seq<CostItem> Items,
    Seq<ConstructionResource> Resources,
    Contingency Contingency) {
    public CostSchedule(string globalId, CostScheduleKind kind, string name, Currency currency, Option<string> status, Seq<CostItem> items, Seq<ConstructionResource> resources)
        : this(globalId, kind, name, currency, status, items, resources, Contingency.None) { }

    public Fin<CostSchedule> Drawdown(Money draw, Op key) =>
        Contingency.Drawdown(draw, key).Map(reserve => this with { Contingency = reserve });

    // The identity hashes the RESOLVED (globalIds, quantity.Si, values) line triples and the resource rows under a
    // CANONICAL ordering (the line set by GlobalId, each priced-id set and the resource set sorted ordinally; the
    // per-line value list stays LIST-ordered — STEP LISTs are stable across re-parse) so the content key is
    // INVARIANT to the unstable IfcSet iteration order — the QuantityKey changes only when a takeoff or rate
    // genuinely changes (the catalog re-reads only a changed estimate), never on an incidental reorder.
    public (UInt128 QuantityKey, UInt128 ResourceKey) Identity => (
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join("",
            Items.OrderBy(static i => i.GlobalId, StringComparer.Ordinal).Select(static i =>
                $"{string.Join(",", i.PricedGlobalIds.OrderBy(static g => g, StringComparer.Ordinal))}={i.Quantity.Si:R}x{string.Join("+", i.Values.Map(static v => $"{v.Rate.Amount}{v.Rate.Currency.Code}@{v.Category.Key}"))}")))),
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join("",
            Resources.OrderBy(static r => r.GlobalId, StringComparer.Ordinal).Select(static r => $"{r.GlobalId}={r.Kind.Key}:{r.BaseQuantity.Si:R}")))));

    // ONE RAILED Money fold over the self-contained per-value lines: ByCurrency aggregates each value in its NATIVE
    // currency first (the lossless mixed-currency subtotal no fx table gates), every value then reprices through
    // CostMoney.Reprice into the one reporting Total; ByCategory keys per VALUE (a material and a labour value on
    // one line partition separately), ByResourceKind the resource Cost repriced through the SAME owner — a foreign
    // value OR a foreign-currency resource BaseCost with no matching rate lifts BimFault.CodecReject rather than
    // letting a different-currency `Money + Money` THROW inside any partition.
    public Fin<CostRollup> Rollup(Op key, Seq<ExchangeRate> fx = default) =>
        Items
            .Bind(static item => item.Values.Map(value => (value.Category.Key, Native: value.Rate * (decimal)item.Quantity.Si)))
            .TraverseM(line => CostMoney.Reprice(line.Native, Currency, fx, key).Map(amount => (line.Key, line.Native, Amount: amount)))
            .As()
            .Bind(lines => Resources
                .TraverseM(resource => CostMoney.Reprice(resource.Cost, Currency, fx, key).Map(cost => (resource.Kind.Key, Cost: cost)))
                .As()
                .Map(costs => new CostRollup(
                    lines.Fold(Money.AdditiveIdentity, static (total, line) => total + line.Amount),
                    lines.Fold(Map<string, Money>(), static (by, line) =>
                        by.AddOrUpdate(line.Native.Currency.Code, existing => existing + line.Native, line.Native)),
                    lines.Fold(Map<string, Money>(), static (by, line) =>
                        by.AddOrUpdate(line.Key, existing => existing + line.Amount, line.Amount)),
                    costs.Fold(Map<string, Money>(), static (by, row) =>
                        by.AddOrUpdate(row.Key, existing => existing + row.Cost, row.Cost)))));

    // Lossless lump-sum apportionment: distribute an overhead/contingency lump-sum across the lines by their
    // value-weight ratios through MoneyExtensions.Split — the remainder pennies spread so the parts sum EXACTLY,
    // the allocation a naive `lump / count` multiply silently loses. A zero-weight line set splits EVENLY
    // (Split(int shares)) so the lump is never dropped; an empty line set is the empty allocation.
    public Seq<(CostItem Line, Money Share)> Apportion(Money lumpSum) =>
        Items.IsEmpty
            ? Seq<(CostItem Line, Money Share)>()
            : Items.Map(static i => Weight(i)).ToArray() is var weights && weights.Any(static w => w > 0)
                ? Items.Zip(lumpSum.Split(weights).ToSeq(), static (line, share) => (Line: line, Share: share))
                : Items.Zip(lumpSum.Split(Items.Count).ToSeq(), static (line, share) => (Line: line, Share: share));

    // The integer ratio weight (cents, clamped to the int domain so a billion-unit line never overflows the
    // Split ratio array) — proportional to the line value so the lump-sum distributes by cost weight.
    static int Weight(CostItem line) =>
        (int)Math.Clamp(Math.Round(line.ValueOf().Amount * 100m, MidpointRounding.AwayFromZero), 0m, int.MaxValue);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CostProjection {
    public static Fin<CostSchedule> Project(IfcCostSchedule schedule, Seq<IfcConstructionResource> resources, ElementGraph graph, Op key) {
        var index = graph.ObjectNodes.Fold(Map<string, NodeId>(),
            static (map, o) => o.ExternalId.Match(Some: id => map.AddOrUpdate(id, o.Id), None: () => map));
        return MonetaryOf(schedule, key)
            .Bind(model =>
                from items in ItemsOf(schedule).TraverseM(item => CostItemOf(item, index, graph, model, key)).As()
                from rows in resources
                    .Filter(static r => ResourceKind.Of(r.GetType().Name) != ResourceKind.NotDefined)
                    .TraverseM(r => ResourceOf(r, model, key)).As()
                select new CostSchedule(
                    schedule.GlobalId,
                    CostScheduleKind.Of(schedule.PredefinedType),
                    schedule.Name ?? "",
                    model == Currency.NoCurrency
                        ? items.Bind(static i => i.Values).Map(static v => v.Applied.Currency)
                            .Filter(static c => c != Currency.NoCurrency).Head.IfNone(Currency.NoCurrency)
                        : model,
                    Optional(schedule.Staus).Filter(static s => s.Length > 0),
                    items,
                    rows));
    }

    public static Fin<Seq<CostSchedule>> ProjectAll(Seq<IfcCostSchedule> schedules, Seq<IfcConstructionResource> resources, ElementGraph graph, Op key) =>
        schedules.TraverseM(schedule => Project(schedule, resources, graph, key)).As();

    // The project-wide monetary unit (IfcContext.UnitsInContext IfcUnitAssignment): the IFC currency every bare
    // IfcMonetaryMeasure carrying no per-value IfcMonetaryUnit prices in — railed through CostMoney.Of so an
    // unregistered ISO 4217 code faults typed at projection, never per line. A model declaring no monetary unit
    // reads NoCurrency and the schedule currency falls back to the first priced value's.
    static Fin<Currency> MonetaryOf(IfcCostSchedule schedule, Op key) =>
        Optional(schedule.Database?.Project?.UnitsInContext)
            .Bind(static units => units.Units.AsIterable().OfType<IfcMonetaryUnit>().ToSeq().Head)
            .Match(
                Some: unit => CostMoney.Of(0m, unit.Currency, key).Map(static zero => zero.Currency),
                None: () => Fin.Succ(Currency.NoCurrency));

    // The controlled top-level items PLUS each item's transitive IsNestedBy sub-item tree (the nested BoQ child
    // lines a Controls-only read drops), deduped by GlobalId; a parent line prices only its OWN authored values,
    // the tree shape riding ParentGlobalId — the same IfcRelNests recursion the schedule WBS flatten owns.
    static Seq<IfcCostItem> ItemsOf(IfcCostSchedule schedule) =>
        schedule.Controls
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcCostItem>()
            .SelectMany(NestedItems)
            .DistinctBy(static item => item.GlobalId)
            .ToSeq();

    static Seq<IfcCostItem> NestedItems(IfcCostItem item) =>
        Seq(item) + item.IsNestedBy
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcCostItem>()
            .ToSeq()
            .Bind(NestedItems);

    // The item's Controls set assigns the priced PRODUCTS and, legally, the IfcConstructionResource it consumes —
    // the resource routes onto ResourceGlobalId (the 5D line-to-resource binding), OFF the priced set, and the priced
    // set narrows OfType<IfcProduct> (only products are seam Object nodes): a line controlling an IfcProcess/IfcActor
    // prices as a unit lump-sum with no element join, never a false DanglingReference abort of the whole schedule.
    static Fin<CostItem> CostItemOf(IfcCostItem item, Map<string, NodeId> index, ElementGraph graph, Currency model, Op key) {
        var related = item.Controls
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .ToSeq();
        var priced = related
            .AsIterable().OfType<IfcProduct>().ToSeq()
            .Map(static o => o.GlobalId)
            .Filter(static id => id.Length > 0);
        var resource = related.AsIterable().OfType<IfcConstructionResource>()
            .Select(static r => r.GlobalId)
            .Where(static id => id.Length > 0)
            .ToSeq()
            .Head;
        return priced.Find(id => !index.ContainsKey(id)).Match(
            Some: id => FinFail<CostItem>(new BimFault.DanglingReference(key, $"cost-priced-miss:{id}")),
            None: () =>
                from values in ValuesOf(item, model, key)
                from same in guard(
                    values.Map(static v => v.Applied.Currency).Filter(static c => c != Currency.NoCurrency).Distinct().Count <= 1,
                    new BimFault.CodecReject(key, $"cost-value-currency-mixed:{item.GlobalId}"))
                from quantity in QuantityOf(item, priced.Choose(index.Find).ToSeq(), graph, key)
                select new CostItem(item.GlobalId, item.Name ?? "", values, quantity, priced, resource,
                    Optional(item.Nests?.RelatingObject?.GlobalId)));
    }

    // The item's WHOLE CostValues LIST projects — bSI sums an item's cost values, so a head-only read drops every
    // value after the first; each value's applied amount + ISO 4217 currency lift TOGETHER into one Money, OR the
    // IfcAppliedValue.Components sub-value tree folds through the IfcArithmeticOperatorEnum so a composite rate
    // (material + labor + equipment sub-rates) resolves. Mixed real currencies WITHIN one item reject at admission
    // (the guard above) so CostItem.ValueOf stays total.
    static Fin<Seq<CostValue>> ValuesOf(IfcCostItem item, Currency model, Op key) =>
        item.CostValues.AsIterable().ToSeq()
            .TraverseM(value =>
                from applied in AmountOf(value, model, key)
                select new CostValue(applied, BasisOf(value), CostCategory.Of(value.Category ?? "")))
            .As();

    // A bare IfcMonetaryMeasure with no per-value IfcMonetaryUnit prices in the MODEL currency (the IFC law: the
    // project-wide monetary unit governs unqualified monetary measures); a non-monetary IfcMeasureValue leaf (an
    // IfcRatioMeasure overhead factor under MULTIPLY, the standard composite idiom) lifts as a currencyless scalar
    // so the operator fold scales rather than zeroing the rate.
    static Fin<Money> AmountOf(IfcAppliedValue value, Currency model, Op key) =>
        value.AppliedValue switch {
            IfcMonetaryMeasure monetary => CurrencyOf(value) is { Length: > 0 } code
                ? CostMoney.Of((decimal)monetary.Measure, code, key)
                : Fin.Succ(new Money((decimal)monetary.Measure, model)),
            IfcMeasureValue measure => Fin.Succ(new Money((decimal)measure.Measure, Currency.NoCurrency)),
            _ => value.Components.AsIterable().ToSeq() is { IsEmpty: false } components
                ? components.TraverseM(c => AmountOf(c, model, key)).As()
                    .Bind(parts => parts.Map(static p => p.Currency).Filter(static c => c != Currency.NoCurrency).Distinct().Count <= 1
                        ? Fin.Succ(Aggregate(value.ArithmeticOperator, parts))
                        : Fin.Fail<Money>(new BimFault.CodecReject(key, "cost-currency:component-mixed")))
                : Fin.Succ(new Money(0m, Currency.NoCurrency)),
        };

    // Currency-STABLE composite fold: the mixed-real-currency guard above leaves at most ONE real currency among the
    // parts, so the operators fold over the decimal AMOUNTS and the single real currency stamps once — a scalar-leading
    // MULTIPLY (a ratio authored before the monetary leg) keeps the currency instead of dropping to NoCurrency, and no
    // arm can throw a cross-currency `Money` op. An empty part set folds to the additive identity.
    static Money Aggregate(IfcArithmeticOperatorEnum op, Seq<Money> parts) {
        var currency = parts.Map(static p => p.Currency).Find(static c => c != Currency.NoCurrency).IfNone(Currency.NoCurrency);
        var amounts = parts.Map(static p => p.Amount);
        var head = amounts.Head.IfNone(0m);
        return new Money(op switch {
            IfcArithmeticOperatorEnum.SUBTRACT => amounts.Tail.Fold(head, static (a, p) => a - p),
            IfcArithmeticOperatorEnum.MULTIPLY => amounts.Tail.Fold(head, static (a, p) => a * p),
            IfcArithmeticOperatorEnum.DIVIDE   => amounts.Tail.Fold(head, static (a, p) => p == 0m ? a : a / p),
            _                                  => amounts.Fold(0m, static (a, p) => a + p),
        }, currency);
    }

    static string CurrencyOf(IfcAppliedValue value) =>
        value.UnitBasis?.UnitComponent is IfcMonetaryUnit unit ? unit.Currency : "";

    static decimal BasisOf(IfcAppliedValue value) =>
        value.UnitBasis?.ValueComponent is IfcMeasureValue basis && basis.Measure > 0d ? (decimal)basis.Measure : 1m;

    // The line takeoff resolved ONCE at projection: the explicit IfcCostItem.CostQuantities (the priced BoQ
    // quantity) when present, else the dominant base quantity off each priced element's seam Bake (Volume ≻ Area ≻
    // Length ≻ Mass ≻ Duration) summed through MeasureValue.Sum, else a Dimensionless unit lump-sum — so a line
    // with neither an explicit nor a derived quantity prices at its rate (quantity 1) rather than zero. The Bake-baked
    // Element.Quantities ALREADY fold the Component Type's shared quantity bags into the occurrence (the seam's named
    // Assign.TypeDefinition type→occurrence inheritance), so a priced occurrence reads its standardized takeoff once
    // off the deduped Type rather than the cost owner re-resolving the type bag — one Bake, never a second join.
    static Fin<MeasureValue> QuantityOf(IfcCostItem item, Seq<NodeId> priced, ElementGraph graph, Op key) =>
        Measures(item.CostQuantities.AsIterable().ToSeq()) is { IsEmpty: false } explicitQuantities
            ? Dominant(explicitQuantities, key)
            : priced.TraverseM(id => graph.Bake(id, key)).As()
                .Bind(elements => Dominant(elements.Bind(static e => e.Quantities).Bind(static b => b.Quantities.Values.ToSeq()), key));

    static readonly Seq<Dimension> PricingRank =
        Seq(Dimension.VolumeDim, Dimension.AreaDim, Dimension.LengthDim, Dimension.MassDim, Dimension.DurationDim);

    static Fin<MeasureValue> Dominant(Seq<MeasureValue> measures, Op key) =>
        PricingRank.Choose(d => measures.Filter(m => m.Dimension == d) is { IsEmpty: false } same ? Some(same) : None)
            .Head
            .Match(Some: same => MeasureValue.Sum(same, key), None: () => Fin.Succ(MeasureValue.OfSi(Dimension.Dimensionless, 1d)));

    static Seq<MeasureValue> Measures(Seq<IfcPhysicalQuantity> quantities) =>
        quantities.Choose(MeasureOf);

    // The IfcPhysicalSimpleQuantity -> seam MeasureValue decode is OWNED by Projection/semantic#VALUE_NARROWING
    // PropertyLowering.Measure (type-pattern dispatch over the six subtype value accessors LengthValue/AreaValue/
    // VolumeValue/WeightValue/CountValue/TimeValue, coerced native-unit -> SI by the per-model UnitScale — the
    // cost-schedule quantities carry the SAME mm-trap the [UNIT_COERCION] law names, resolved off the entity's
    // public Database context); the cost read COMPOSES that one Bim-internal owner — a parallel GetType().Name
    // dimension switch reading the base IfcMeasureValue accessor is the duplicate form deleted here.
    static Option<MeasureValue> MeasureOf(IfcPhysicalQuantity? quantity) =>
        quantity is IfcPhysicalSimpleQuantity simple
            ? Some(PropertyLowering.Measure(simple, simple.Database is { } db ? UnitScale.Of(db) : UnitScale.Si))
            : None;

    // ONE resource fold reading each IfcConstructionResource by runtime subtype onto a ConstructionResource row —
    // the ResourceKind discriminant, the BaseQuantity MeasureValue, the RAILED BaseCosts Money (a currency fault
    // lifts BimFault.CodecReject typed, never an Option-swallowed cost), the Skill/Material per modality, the
    // OperatesOn task GlobalId, and the IfcResourceTime.Completion ratio; a non-construction entity filters out
    // before the traverse rather than aborting the schedule.
    static Fin<ConstructionResource> ResourceOf(IfcConstructionResource resource, Currency model, Op key) =>
        BaseCostOf(resource, model, key).Map(baseCost => new ConstructionResource(
            resource.GlobalId, resource.Name ?? "", ResourceKind.Of(resource.GetType().Name),
            MeasureOf(resource.BaseQuantity).IfNone(MeasureValue.Zero),
            baseCost,
            SkillOf(resource),
            MaterialOf(resource),
            TaskOf(resource),
            Optional(resource.Usage).Bind(static usage => usage.Completion is > 0d and <= 1d ? Some(usage.Completion) : None)));

    static Fin<Option<Money>> BaseCostOf(IfcConstructionResource resource, Currency model, Op key) =>
        resource.BaseCosts.AsIterable().Head.Match(
            Some: value => AmountOf(value, model, key).Map(Some),
            None: () => Fin.Succ(Option<Money>.None));

    static Option<string> SkillOf(IfcConstructionResource resource) => resource switch {
        IfcLaborResource labor => Some(labor.PredefinedType.ToString()),
        IfcCrewResource crew   => Some(crew.PredefinedType.ToString()),
        _                      => None,
    };

    static Option<string> MaterialOf(IfcConstructionResource resource) =>
        resource is IfcConstructionMaterialResource or IfcConstructionProductResource
            ? toSeq(resource.HasAssociations
                .AsIterable()
                .OfType<IfcRelAssociatesMaterial>())
                .Head
                .Bind(static rel => Optional((rel.RelatingMaterial as IfcMaterial)?.Name))
            : None;

    static Option<string> TaskOf(IfcConstructionResource resource) =>
        toSeq(resource.HasAssignments
            .AsIterable()
            .OfType<IfcRelAssignsToProcess>())
            .Head
            .Bind(static rel => Optional((rel.RelatingProcess as IfcProcess)?.GlobalId))
            .Filter(static id => id.Length > 0);
}
```

## [03]-[EARNED_VALUE]

- Owner: `ChangeOrder` the priced-revision record carrying the baseline `CostSchedule` `GlobalId`, the priced `CostItem` delta set (added/modified/removed lines against the baseline), the `ChangeOrderStatus` `[SmartEnum<string>]` approval state, and the revision `Instant`; `Contingency` a `CostCategory.Contingency` `Money` reserve carried on `CostSchedule` a drawdown decrements through the native `Money` subtraction RAILED on currency (a foreign-currency draw faults typed, never a thrown mismatch); `EarnedValueReport` the typed receipt carrying BCWS (planned value), BCWP (earned value), ACWP (actual cost) as `Money`, the cost-performance index `CPI = BCWP/ACWP`, schedule-performance index `SPI = BCWP/BCWS`, and to-complete index `TCPI = (BAC − BCWP)/(BAC − ACWP)` as the dimensionless `Money / Money → decimal` ratio, the estimate-at-completion `EAC = BAC/CPI`, variance-at-completion `VAC = BAC − EAC`, and estimate-to-complete `ETC = EAC − ACWP` as `Money`, and the derived cost/schedule variances `CV = BCWP − ACWP` / `SV = BCWP − BCWS` as `Money` whose `Money.IsNegative` sign reads ARE the `OverBudget`/`BehindSchedule` predicates — never a `(double, string Currency)` carrier (the currency rides each `Money`) and never a hand-written `< 0` on the raw `decimal`; `CostSchedule.EarnedValue` the currency-RAILED, task-join-TOTAL fold joining the `Planning/schedule#SCHEDULE` `ConstructionTask` authored progress and the external `actuals` incurred-cost feed to the self-contained priced lines at a status `Instant`, never a generic ledger.
- Entry: `CostSchedule.EarnedValue(ScheduleNetwork network, Instant statusDate, Op key, Map<string, Money> actuals = default, Seq<ExchangeRate> fx = default)` folds the `Fin<EarnedValueReport>` at a status date — each `CostItem` joins its priced element set to the `Planning/schedule#SCHEDULE` `ConstructionTask` that assigns it (by the `TaskAssignment` `GlobalId` membership, the FIRST priced element carrying an assignment — a head-only read starving a line whose leading element is unassigned is the deleted form), reads the task's planned percent-complete (the fraction of the task's scheduled `Interval` elapsed at `statusDate`) and actual percent-complete (the task's AUTHORED `PercentComplete` — the `IfcTaskTime.Completion` ratio the schedule owns — with the actual-`Interval` fraction as the fallback, or `1.0` when the task `Status` is `Completed`), and contributes `line.ValueOf() × plannedPercent` to BCWS and `× actualPercent` to BCWP, while ACWP reads the line's recorded incurred cost-to-date the `actuals` map supplies per `CostItem.GlobalId` (the EVM cost axis an accounting/Persistence feed produces) and falls back to the schedule-duration proxy (the earned value scaled by the actual-vs-scheduled duration-overrun ratio) ONLY when a line carries no recorded actual — so with `actuals` supplied CPI is a TRUE cost index independent of SPI, and without them the report degrades to a schedule-derived forecast rather than fabricating a cost index; the report partitions BCWS/BCWP/ACWP `Money` over the line set and derives the CPI/SPI/TCPI/EAC/VAC/CV/SV scalars; the fold is TOTAL on the task join (a line whose assigning task the network never declares still contributes its budget to BAC and its recorded spend to ACWP, never a fault) and RAILED on currency — every line budget and recorded actual reprices to the schedule `Currency` through `CostMoney.Reprice`, so a mixed-currency schedule faults typed instead of the accumulator `Money + Money` THROWING mid-fold, the exception-in-domain defect the prior total-claim prose hid. `ChangeOrder.Apply(CostSchedule baseline)` folds the priced delta set onto the baseline producing the revised `CostSchedule` (the delta lines added/superseding/removing the baseline lines by `GlobalId`) so a revision is the existing `CostItem`/`CostValue` algebra applied against a baseline, never a parallel revision store, and `CostSchedule.Drawdown(Money draw, Op key)` decrements the `Contingency` `Money` reserve through the currency-railed `Fin<CostSchedule>` fold, the remainder floored at zero in the reserve currency.
- Auto: `EarnedValue` reads each `CostItem.ValueOf()` budgeted line value repriced through `CostMoney.Reprice` (the pure `Quantity × Σrate` in the schedule `Currency`, the `BAC` budget-at-completion summing the line set as `Money`), resolves each line's assigning `ConstructionTask` through the `network.Assignments` `TaskAssignment` join over the first assigned priced element (`PricedGlobalIds.Choose(taskByElement.Find).Head`), computes the task planned percent as `(statusDate − scheduled.Start) / scheduled.Duration` clamped to `[0,1]` and the actual percent as the AUTHORED `task.PercentComplete` (`1.0` on a `Completed` status; the `(statusDate − actual.Start) / actual.Duration` interval fraction only when the schedule authored no ratio; `0.0` on an absent actual interval), and folds `BCWS += budget × planned`, `BCWP += budget × actual`, and `ACWP +=` the line's recorded incurred cost (`actuals.Find(item.GlobalId)`, repriced) when present else the schedule-duration proxy `budget × actual × overrun` (the actual-vs-scheduled duration ratio so an over-running task reads `ACWP > BCWP` in the no-actuals fallback) — each accumulator a `Money` summed through the additive operator; the report derives `CPI = ACWP.Amount == 0 ? 1 : BCWP/ACWP`, `SPI = BCWS.Amount == 0 ? 1 : BCWP/BCWS` (the `Money / Money → decimal` ratio), `EAC = CPI == 0 ? BAC : BAC/CPI` (the `Money / decimal → Money` divide), `VAC = BAC − EAC`, and the expression-bodied `CV`/`SV`/`TCPI` reads — one `Fold` over the `(line, task)` join, never enumerated per-line arms; `ChangeOrder.Apply` folds the delta set onto the baseline line map keyed by `CostItem.GlobalId` (an added line inserts, a modified line supersedes, a removed line drops) so the revised schedule re-rolls through the existing `Rollup` fold.
- Receipt: the `EarnedValueReport` is the typed 5D cost-performance evidence the `csharp:Rasm.AppUi/Charts` `EarnedValue/ChangeOrder` report renders — `OverBudget` reads `Money.IsNegative(Cv)` (a true cost signal once `actuals` are supplied, a schedule-overrun forecast otherwise), `BehindSchedule` reads `Money.IsNegative(Sv)`, `EAC > BAC` reads forecast-overrun, `TCPI > 1` reads the demanded remaining efficiency, the `VAC` the cost variance at completion as `Money`; the `ChangeOrder` revision audit reads the baseline-to-revised line delta and the `ChangeOrderStatus` approval state, and the `Contingency` drawdown reads the remaining `Money` reserve — each carried on the one tracked `CostSchedule`, never a second cost-performance store.
- Packages: GeometryGymIFC_Core, NodaMoney, Thinktecture.Runtime.Extensions, NodaTime, System.IO.Hashing, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new earned-value metric (a weighted-EAC formula variant `EAC = ACWP + (BAC − BCWP)/(CPI × SPI)`, a schedule-adjusted forecast) is one derived scalar on `EarnedValueReport` over the same BCWS/BCWP/ACWP fold the realized `CV`/`SV`/`ETC`/`TCPI` reads already prove; a new change-order status is one `ChangeOrderStatus` row; a new contingency-allocation rule rides the `CostSchedule.Apportion` `Split`; a new progress source rides the existing `ConstructionTask` actual-interval join and a recorded actual-cost feed rides the existing `actuals` map keyed by `CostItem.GlobalId`; never a per-metric report record, never a parallel revision or contingency store, never a re-derived progress source, never a second actual-cost store, and never a `(double, string)` metric carrier.
- Boundary: the earned-value join reads the `Planning/schedule#SCHEDULE` `ConstructionTask` actual/scheduled `Interval` progress by `GlobalId` and re-deriving progress in this owner is the named seam violation — the schedule owns the activity network and its actual interval, the cost owner reads the percent-complete it implies; the metrics are `Money` (BCWS/BCWP/ACWP/EAC/VAC/CV/SV) and the dimensionless `decimal` `Money / Money` ratio (CPI/SPI/TCPI), the sign reads the native `Money.IsNegative` predicates, and a `(double Bac, …, string Currency)` carrier or a hand-written `< 0` on the raw `decimal` is the deleted form mirroring the no-`(double, string)`-money law of `[2]-[ESTIMATE]`; the `ChangeOrder` delta is a priced revision against a baseline `CostSchedule` reusing the existing `CostItem`/`CostValue`/`Money` algebra and a parallel `CostRevision` class family or a second revision store is the deleted form; the `Contingency` is a `CostCategory.Contingency` `Money` reserve on the one `CostSchedule`, never a parallel reserve store; the `EarnedValueReport` is the typed receipt and a generic `IReceipt`/ledger is the named defect per the typed-receipt law; the fold joins the cost line to the schedule task by the `TaskAssignment` `GlobalId` membership, its actual percent is the schedule-AUTHORED `PercentComplete` before any interval re-derivation (re-deriving progress the schedule authored is the named seam violation), and a parallel cost-side schedule is the named seam violation; the fold is TOTAL on the task join and RAILED on currency — a bare `EarnedValueReport` return whose accumulator `Money + Money` can THROW on a mixed-currency schedule is the deleted form (the prose claimed total, the code threw — the exception-in-domain defect), and a vestigial rail claiming an abort the code never performs remains equally deleted.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using NodaMoney;
using NodaMoney.Exchange;
using NodaTime;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

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
// subtraction floored at zero in the reserve currency through the IsNegative guard, RAILED on currency — a
// foreign-currency draw faults typed rather than the raw `Money - Money` throwing in domain logic. A
// percentage-based reserve is authored through Apportion at allocation time, never a stored ratio column.
public sealed record Contingency(Money Reserve) {
    public static readonly Contingency None = new(Money.AdditiveIdentity);
    public Fin<Contingency> Drawdown(Money draw, Op key) =>
        draw.Currency != Reserve.Currency && Reserve.Currency != Currency.NoCurrency && draw.Currency != Currency.NoCurrency
            ? FinFail<Contingency>(new BimFault.CodecReject(key, $"cost-currency:contingency-draw:{draw.Currency.Code}>{Reserve.Currency.Code}"))
            : FinSucc(new Contingency(Reserve - draw is var net && Money.IsNegative(net) ? new Money(0m, Reserve.Currency) : net));
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
        var overrides = Delta.Map(static i => (i.GlobalId, i)).ToMap();
        var retained = baseline.Items.Filter(i => !removed.Contains(i.GlobalId) && !overrides.ContainsKey(i.GlobalId));
        return baseline with { Items = retained.Append(Delta) };
    }
}

// The 5D cost-performance receipt: the monetary axes are Money (the currency rides each value, never a bare
// string field), CPI/SPI the dimensionless Money / Money -> decimal ratio, and the COMPLETE EVM derived set rides
// the same four folds — CV = BCWP - ACWP and SV = BCWP - BCWS as Money, ETC = EAC - ACWP the estimate-to-complete,
// TCPI = (BAC - BCWP)/(BAC - ACWP) the to-complete index. The sign reads are the NATIVE Money.IsNegative predicates
// over CV/SV (CV < 0 <=> CPI < 1), never a hand-written `< 0` on the raw decimal. Of derives the indices guarding
// the zero divisors so an empty schedule reads CPI=SPI=TCPI=1 and EAC=BAC rather than a divide-by-zero.
public readonly record struct EarnedValueReport(
    Money Bac, Money Bcws, Money Bcwp, Money Acwp,
    decimal Cpi, decimal Spi, Money Eac, Money Vac) {
    public Money Cv => Bcwp - Acwp;
    public Money Sv => Bcwp - Bcws;
    public Money Etc => Eac - Acwp;
    public decimal Tcpi => (Bac - Acwp).Amount == 0m ? 1m : (Bac - Bcwp) / (Bac - Acwp);
    public bool OverBudget => Money.IsNegative(Cv);
    public bool BehindSchedule => Money.IsNegative(Sv);

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
    // The fold is RAILED on currency, total on the task join: every line budget and recorded actual reprices to the
    // schedule Currency through the one CostMoney.Reprice fx-table owner (an unrepriced accumulator `Money + Money`
    // would THROW — the exception-in-domain defect the rail closes), while a line whose assigning task the network
    // never declares still contributes budget to BAC and recorded spend to ACWP, never a fault.
    public static Fin<EarnedValueReport> EarnedValue(this CostSchedule schedule, ScheduleNetwork network, Instant statusDate, Op key, Map<string, Money> actuals = default, Seq<ExchangeRate> fx = default) {
        var taskByElement = network.Assignments
            .Bind(a => a.ElementGlobalIds.Map(id => (Element: id, a.TaskGlobalId)))
            .Fold(Map<string, string>(), static (m, row) => m.AddOrUpdate(row.Element, row.TaskGlobalId));
        var taskById = network.Tasks.Fold(Map<string, ConstructionTask>(), static (m, t) => m.AddOrUpdate(t.GlobalId, t));
        return schedule.Items
            .TraverseM(item => Line(item, taskByElement, taskById, actuals, statusDate, schedule.Currency, fx, key))
            .As()
            .Map(lines => lines.Fold(
                (Bac: Money.AdditiveIdentity, Bcws: Money.AdditiveIdentity, Bcwp: Money.AdditiveIdentity, Acwp: Money.AdditiveIdentity),
                static (acc, l) => (acc.Bac + l.Budget, acc.Bcws + l.Bcws, acc.Bcwp + l.Bcwp, acc.Acwp + l.Acwp)))
            .Map(static t => EarnedValueReport.Of(t.Bac, t.Bcws, t.Bcwp, t.Acwp));
    }

    static Fin<(Money Budget, Money Bcws, Money Bcwp, Money Acwp)> Line(
        CostItem item, Map<string, string> taskByElement, Map<string, ConstructionTask> taskById,
        Map<string, Money> actuals, Instant statusDate, Currency report, Seq<ExchangeRate> fx, Op key) =>
        from budget in CostMoney.Reprice(item.ValueOf(), report, fx, key)
        from recorded in actuals.Find(item.GlobalId).Match(
            Some: money => CostMoney.Reprice(money, report, fx, key).Map(Some),
            None: () => Fin.Succ(Option<Money>.None))
        // The task join reads the FIRST priced element that carries an assignment (never a head-only read that
        // starves a line whose leading element is unassigned); the actual percent is the task's AUTHORED
        // PercentComplete (IfcTaskTime.Completion, the schedule-owned progress) with the actual-interval fraction
        // as the fallback — re-deriving progress here when the schedule authored it is the named seam violation.
        select item.PricedGlobalIds.Choose(taskByElement.Find).Head
            .Bind(taskById.Find)
            .Match(
                Some: task => {
                    decimal planned = (decimal)Fraction(task.Scheduled, statusDate);
                    decimal actual = task.Status == TaskStatus.Completed ? 1m
                        : (decimal)task.PercentComplete.IfNone(() => task.Actual.Map(a => Fraction(a, statusDate)).IfNone(0d));
                    // A zero-duration scheduled window (a milestone) has no overrun basis — ratio 1, never a
                    // divide-by-epsilon whose decimal cast overflows mid-fold.
                    decimal overrun = (decimal)task.Actual.Map(a => task.Scheduled.Duration.TotalDays > 0d ? a.Duration.TotalDays / task.Scheduled.Duration.TotalDays : 1d).IfNone(1d);
                    // ACWP from the recorded incurred cost when present; else the schedule-duration proxy the no-actuals path degrades to.
                    Money acwp = recorded.IfNone(budget * (actual * overrun));
                    return (Budget: budget, Bcws: budget * planned, Bcwp: budget * actual, Acwp: acwp);
                },
                // A line with no assigning task still contributes its recorded spend to ACWP (and its budget to BAC) — cost incurred without earned value.
                None: () => (Budget: budget, Bcws: Money.AdditiveIdentity, Bcwp: Money.AdditiveIdentity, Acwp: recorded.IfNone(Money.AdditiveIdentity)));

    static double Fraction(Interval interval, Instant statusDate) =>
        interval.Duration.TotalDays <= 0d ? (statusDate >= interval.End ? 1d : 0d)
        : Math.Clamp((statusDate - interval.Start).TotalDays / interval.Duration.TotalDays, 0d, 1d);
}
```

## [04]-[RESEARCH]

- [COST_SCHEDULE_DISPATCH]: the `IfcCostSchedule` container traversal — the `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcCostItem` set the `ItemsOf` fold materializes the priced lines through, each item's own `Controls` `IfcRelAssignsToControl` priced-element set, each item's `CostValues` `IfcCostValue` applied-rate set and `CostQuantities` `IfcPhysicalQuantity` explicit-quantity set, and the `Nests` `IfcRelNests.RelatingObject` parent the bill-of-quantities tree declares — grounds against the live GeometryGym `25.7.30` decompile (`IfcCostItem : IfcControl` exposes `CostValues` `LIST<IfcCostValue>`, `CostQuantities` `LIST<IfcPhysicalQuantity>`, `PredefinedType` `IfcCostItemTypeEnum`; `IfcControl.Controls` `SET<IfcRelAssignsToControl>`; `IfcObjectDefinition.Nests`/`IsNestedBy`/`HasAssociations`/`HasAssignments`) so the `CostItemOf`/`ValueOf`/`QuantityOf` projections discriminate the real cost graph; the `IfcCostSchedule.Controls`/`PredefinedType`, `IfcCostItem.CostValues`/`CostQuantities`/`Controls`/`Nests`, and `IfcRelAssignsToControl.RelatingControl`/`RelatedObjects` member spellings confirm against the `.api/api-geometrygym-ifc` scheduling-cost-resource family (rows 8-16) — the `IfcCostSchedule.Controls`/`IfcCostItem.Controls` `IfcRelAssignsToControl` cost-control path distinct from the `IfcRelAssignsToProcess` resource-to-activity path the resource fold reads; the `IfcCostScheduleTypeEnum` members verified against the decompile are `NOTDEFINED`/`USERDEFINED`/`BUDGET`/`COSTPLAN`/`ESTIMATE`/`TENDER`/`PRICEDBILLOFQUANTITIES`/`UNPRICEDBILLOFQUANTITIES`/`SCHEDULEOFRATES` — the migration source's `RUNNING` row was a PHANTOM (no such enum member) and is removed, the missing `UNPRICEDBILLOFQUANTITIES` added; `IfcCostSchedule`'s public schedule-status accessor is the GG-typo'd `Staus` (`string`, free IFC text — `Status` does not exist on the public surface) feeding `CostSchedule.Status`, while `SubmittedOn`/`UpdateDate` exist only as PRIVATE `mSubmittedOn`/`mUpdateDate` fields with no public accessor in 25.7.30 — the submission/update stamps are UNREADABLE and stay off the record until GG exposes them; the item's `CostValues` is a `LIST` (STEP LISTs are ordered and stable across re-parse) whose values SUM onto one line per the bSI cost-item law, each carrying its own `Category`/`UnitBasis` — the head-only single-`CostValue` read was a COVERAGE defect dropping every value after the first on a material+labour split line; the nested BoQ child lines enter through the `IfcObjectDefinition.IsNestedBy` closure the `NestedItems` flatten walks (the schedule's `Controls` set holds only the top-level items — a `Controls`-only read drops every nested child line the `Nests` parent references), a parent line pricing only its OWN authored values so a leaf-authored bill sums exactly and the tree shape rides `ParentGlobalId`.
- [COST_VALUE_MONETARY]: the `IfcCostValue` applied-rate members the `ValueOf`/`AmountOf` folds read are verified against the live decompile — `IfcCostValue : IfcAppliedValue` inherits `AppliedValue` (`IfcAppliedValueSelect`), `UnitBasis` (`IfcMeasureWithUnit`), `Category` (`String`), `Components` (`LIST<IfcAppliedValue>`), and `ArithmeticOperator` (`IfcArithmeticOperatorEnum` `NONE`/`ADD`/`SUBTRACT`/`MULTIPLY`/`DIVIDE`); the `IfcMonetaryMeasure` direct-amount leg exposes `.Measure` (a `Double`, `IfcMonetaryMeasure : IfcDerivedMeasureValue`) and the `IfcMeasureWithUnit.UnitComponent` (`is IfcMonetaryUnit` whose `.Currency` is the ISO 4217 code) lift TOGETHER into one `NodaMoney` `Money` through the railed `CostMoney.Of((decimal)measure, currency, key)`, the `new Money(decimal, string)` ctor resolving the embedded ISO 4217 `CurrencyRegistry` and the thrown `InvalidCurrencyException` trapping onto `BimFault.CodecReject` lifted BARE — the COMPOSITE leg (`AppliedValue` null, `Components` non-empty) folding the sub-value tree recursively through the `ArithmeticOperator` so a `material + labor + equipment` rate resolves rather than reading only the head — the component set guards mixed REAL currencies (`cost-currency:component-mixed` BARE, a cross-currency `ADD`/`SUBTRACT` would otherwise throw mid-fold) and the fold is currency-STABLE (the operators fold decimal amounts, the single real currency stamps once, so a ratio authored BEFORE the monetary leg under `MULTIPLY` keeps the currency rather than dropping to `NoCurrency`); verified: `Money` is a `readonly struct` implementing `IAdditiveIdentity<Money,Money>`/`IMultiplicativeIdentity<Money,decimal>` with the full generic-math operator set (`Money + Money`, `Money * decimal → Money`, `Money / decimal → Money`, `Money / Money → decimal`) native, the static sign/magnitude family `Money.Abs`/`IsNegative`/`IsPositive`/`IsZero(in)`/`MinMagnitude`/`MaxMagnitude` real, and `Money.AdditiveIdentity` `= new Money(0m, Currency.NoCurrency)`, `MoneyExtensions.Split(int[] ratios)` returning `IEnumerable<Money>` the lossless allocation, the `2.7.0` `lib/net10.0` asset binding (`api-nodamoney`); the decompiled `NodaMoney.Transaction` is a four-property mutable POCO (`Amount`/`ExchangeRate`/`Tax`/`Discount`) and NOT a multi-currency aggregation bag — the catalog's "wallet/ledger of per-currency totals" claim is a PHANTOM, so the mixed-currency lossless aggregation is OWNED by the `CostRollup.ByCurrency` native-amount partition and a `Transaction`-based rollup is never authored; the ambient `MoneyContext.DefaultCurrency` stays UNSET by design — the reporting currency is the per-schedule `CostSchedule.Currency` resolved once at projection, and a process-wide ambient default would cross-contaminate a federation whose schedules report in different currencies; the model currency resolves through the verified `BaseClassIfc.Database` → `DatabaseIfc.Project` → `IfcContext.UnitsInContext` (`IfcUnitAssignment.Units` `SET<IfcUnit>`) → `IfcMonetaryUnit.Currency` chain; a bare `IfcMonetaryMeasure` with no per-value `IfcMonetaryUnit` prices in that project currency per the IFC unit-assignment law (the prior `Currency.NoCurrency` degradation silently zeroed every unqualified line out of the rollup); a non-monetary `IfcMeasureValue` component (`IfcRatioMeasure` — `IfcMeasureValue.Measure` is the verified `double` accessor) lifts as the currencyless scalar factor so a `MULTIPLY` overhead-ratio component scales the composite rather than zeroing it; the `IfcMeasureWithUnit.ValueComponent` (`is IfcMeasureValue` whose `.Measure` is the per-basis denominator) lifts onto the `decimal UnitBasis` so the line rate is the native `Applied / UnitBasis` divide.
- [RESOURCE_MODALITY]: the construction-resource modalities the ONE `ConstructionResource` record discriminates over the `ResourceKind` `[SmartEnum]` are verified against the decompile — `IfcConstructionResource : IfcResource` (abstract) exposes `Usage` (`IfcResourceTime`), `BaseCosts` (`LIST<IfcAppliedValue>`), and `BaseQuantity` (`IfcPhysicalQuantity`), and its six concrete subtypes `IfcLaborResource`/`IfcConstructionMaterialResource`/`IfcConstructionEquipmentResource`/`IfcCrewResource`/`IfcConstructionProductResource`/`IfcSubContractResource` each carry a `PredefinedType` over their own `Ifc…ResourceTypeEnum` — so the migration source's 3-arm `Labor`/`Material`/`Equipment` `[Union]` was a NAIVE SLICE silently dropping `Crew`/`Product`/`Subcontract` through its `_ => None` arm; the collapse to ONE record keyed by `ResourceKind.Of(GetType().Name)` covers all six, lifts `BaseCosts` onto the optional `Money BaseCost` RAILED (the resource cost the by-resource rollup partition reads, `BaseCost * BaseQuantity`; a `BaseCosts` currency fault lifts `BimFault.CodecReject` typed, never an `Option`-swallowed cost), reads `BaseQuantity` as a seam `MeasureValue` (the `IfcPhysicalSimpleQuantity.MeasureValue.Measure` SI magnitude, never a bare `double`), reads the `IfcLaborResource`/`IfcCrewResource` `PredefinedType` onto `Skill` (the IFC4.3 schema carrying no `SkillSet` member — the predefined token IS the skill descriptor), reads the `IfcConstructionMaterialResource`/`IfcConstructionProductResource` `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`is IfcMaterial`) `.Name` onto `Material`, the inherited `HasAssignments` `IfcRelAssignsToProcess.RelatingProcess` `IfcProcess.GlobalId` onto `TaskGlobalId` so the resource binds the `Planning/schedule#SCHEDULE` `ConstructionTask` it resources, and the inherited `Usage` `IfcResourceTime` (verified members `ScheduleUsage`/`ActualUsage`/`ScheduleWork`/`ActualWork`/`Completion` among the resource-time axis) `Completion` ratio onto the optional `Completion` guarded to `(0, 1]` — the resource-side actual-progress axis the `Spent` incurred read scales `Cost` by; a `USERDEFINED` `ResourceKind` row is UNREACHABLE dead data because the `Of(GetType().Name)` discriminant only ever yields the six subtype keys (`NotDefined` the sole `IfNone` fallback) and is pruned.
- [SEAM_TAKEOFF_JOIN]: the cost-to-element join reads the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` rather than the retired `BimModel` — the `CostProjection.Project` builds an `ExternalId → NodeId` index over `graph.ObjectNodes` once (the `Node.Object.ExternalId` is the 1:1 IFC `GlobalId` projection attribute [H6] the `Projection/semantic#SEMANTIC_PROJECTOR` records), faults `BimFault.DanglingReference` on a priced `GlobalId` no `Node.Object` declares, and resolves the line takeoff `MeasureValue` from the explicit `IfcCostItem.CostQuantities` or the dominant base quantity off each priced element's `graph.Bake(node, key)` `Element.Quantities` bag (the seam `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` the `Semantics/properties#BASE_QUANTITIES` `QuantityDerivation` derived from the kernel geometry the node references by content key, the bag the seam `Bake` already folds the priced occurrence's `Element.Type` `Component` Type quantities into through the named `Assign.TypeDefinition` inheritance so a standardized member prices off its deduped Type takeoff in one `Bake`) by the `Dimension` rank `Volume ≻ Area ≻ Length ≻ Mass ≻ Duration` summed through `MeasureValue.Sum` — the retired `QuantityKind` six-case enum replaced by the seam `Dimension` `[ComplexValueObject]` so a `ThermalTransmittance`/`Pressure` measure admits; the takeoff resolves ONCE at projection so the `CostItem` is self-contained (`ValueOf()` pure, `Rollup`/`EarnedValue` need no graph) and the `(QuantityKey, ResourceKey)` `UInt128` identity hashes the resolved `(globalIds, quantity.Si, values)` triples through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom so the cross-libs `csharp:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog re-reads only a changed estimate at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`, and the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified set — re-pricing or minting a second takeoff/schedule/identity source downstream is the named cross-folder drift defect.
- [FAULT_BARE_LIFT]: every cost rejection lifts the typed `Model/faults#FAULT_BAND` `BimFault` case BARE onto the `Fin<T>` rail — the rebuilt band is `Expected`-derived (`IValidationError<BimFault>`) so band 2600 IS the `Expected` `Code` and `Fin.Fail<CostItem>(new BimFault.DanglingReference(key, $"cost-priced-miss:{id}"))` / `Try.lift<Money>(() => new Money(amount, code)).Run().MapFail(error => new BimFault.CodecReject(key, $"cost-currency:{error.Message}"))` (the explicit type argument — `Try.lift` admits only a `Func<Fin<A>>`, the lambda's `Money` lifting through the implicit `Fin<A>` conversion generic inference alone cannot see) both carry the case directly, the `.ToError()` lowering hop the migration source threaded through every construction being the named defect the band closes (a `.ToError()` erases the typed arm the `IfcLegality` `Validation` accumulation and the `error.IsType<BimFault.DanglingReference>()` recovery depend on); each case carries the kernel `Op key` operation context, so a cost-currency reject and a priced-miss compose on the one rail with the schedule/quantity faults without a second fault family, and the `MoneyContext` `CostMoney.Context` banker's-rounding policy the composition root installs governs every `Money` operator rather than a `MidpointRounding` argument threaded through each construction.
